"""Automatic-update round, Part A7: the launcher's automatic flow on a test install of 1.0.0 (isolated test saves, hidden,
batch mode) against a local signed test catalog that offers a newer game (build 1000001, the 1.0.0 files repackaged).
  update:  left alone, it downloads, verifies, installs and starts 1000001
  skip:    B / Esc during the download: plays 1000000 at once, nothing installed
  offline: the catalog cannot be reached: plays 1000000
Never touches Dan's Builds/Latest or saves (read only: the 1.0.0 runtime and soundtrack are copied from it)."""
import ctypes,ctypes.wintypes as wt,functools,http.server,importlib.util,json,pathlib,shutil,socket,subprocess,threading,time,zipfile
ROOT=pathlib.Path(__file__).resolve().parents[2];EVIDENCE=ROOT/'Docs/ReportAuto';LATEST=ROOT/'Builds/Latest'
spec=importlib.util.spec_from_file_location('release',ROOT/'Tools/Prepare-LauncherRelease.py');release=importlib.util.module_from_spec(spec);spec.loader.exec_module(release)
from cryptography.hazmat.primitives import serialization
key=serialization.load_pem_private_key((ROOT/'Builds/PublisherPrivate/launcher-signing.pem').read_bytes(),None)
LAUNCHER=ROOT/'Builds/Launcher/WoodstockRushLauncher.exe';CHECKS=ROOT/'Builds/Launcher/LauncherChecks.exe'
GAME_MANIFEST=ROOT/'Builds/LauncherRelease-1000000/assets/game-manifest.json';MUSIC_MANIFEST=LATEST/'soundtrack-manifest.json'
WORK=ROOT/'Temp'/('reportauto-'+str(time.time_ns()));assets=WORK/'assets';assets.mkdir(parents=True)
latest_state=(LATEST/'state.json').read_bytes()
class Handler(http.server.SimpleHTTPRequestHandler):
    rate=None  # bytes per second for game.zip (None: full speed)
    def copyfile(self,source,output):
        if self.rate is None or not self.path.endswith('.zip'):return super().copyfile(source,output)
        for block in iter(lambda:source.read(64*1024),b''):output.write(block);time.sleep(len(block)/self.rate)
    def log_message(self,*a):pass
server=http.server.ThreadingHTTPServer(('127.0.0.1',0),functools.partial(Handler,directory=str(assets)));threading.Thread(target=server.serve_forever,daemon=True).start()
base=f'http://127.0.0.1:{server.server_port}'
def payload(envelope):
    p=subprocess.run([str(CHECKS),str(WORK/'verify-root'),str(WORK/'verify-saves'),'verify',str(envelope)],capture_output=True,text=True);assert p.returncode==0,p.stderr;return json.loads(p.stdout)
old=payload(GAME_MANIFEST);assert old['build']==1000000
game_dir=LATEST/'versions/1000000'
# The newer test game: the same 1.0.0 files as build 1000001, packaged and signed like a real release (local URL).
archive=assets/'game.zip'
with zipfile.ZipFile(archive,'w',zipfile.ZIP_DEFLATED,compresslevel=1) as z:
    for f in old['files']:z.write(game_dir/f['path'],f['path'])
new=dict(old,build=1000001,version='1.0.1-test',notes='Automatic-update check (test only)',package=dict(url=base+'/game.zip',bytes=archive.stat().st_size,sha256=release.digest(archive)))
(assets/'game-manifest.json').write_text(json.dumps(release.sign(new,key)))
shutil.copy2(MUSIC_MANIFEST,assets/'soundtrack-manifest.json')
(assets/'update-catalog.json').write_text(json.dumps(release.sign(dict(schema=1,type='catalog',game=base+'/game-manifest.json',soundtrack=base+'/soundtrack-manifest.json'),key)))
def seed(name):
    root=WORK/name;saves=WORK/(name+'-saves');saves.mkdir(parents=True)
    p=subprocess.run([str(CHECKS),str(root),str(saves),'seed',str(game_dir),str(GAME_MANIFEST),str(LATEST/'shared/soundtracks/1'),str(MUSIC_MANIFEST)],capture_output=True,text=True);assert p.returncode==0,p.stderr
    (saves/'settings.json').write_text(json.dumps(dict(version=1,master=0,radioOn=False,frameLimit=60,vsync=False)));return root,saves
user32=ctypes.windll.user32
def window_of(pid):
    found=[]
    @ctypes.WINFUNCTYPE(wt.BOOL,wt.HWND,wt.LPARAM)
    def each(h,_):
        p=wt.DWORD();user32.GetWindowThreadProcessId(h,ctypes.byref(p));buf=ctypes.create_unicode_buffer(64);user32.GetClassNameW(h,buf,64)
        if p.value==pid and buf.value=='WoodstockRushLauncher':found.append(h)
        return True
    user32.EnumWindows(each,0);return found[0] if found else None
def run(name,catalog,skip_on=None):
    root,saves=seed(name);ev=WORK/(name+'-evidence');ev.mkdir()
    si=subprocess.STARTUPINFO();si.dwFlags=subprocess.STARTF_USESHOWWINDOW;si.wShowWindow=0
    started=time.time();p=subprocess.Popen([str(LAUNCHER),'--root',str(root),'--auto-check','--test-save',str(saves),'--evidence',str(ev),'--catalog',catalog],startupinfo=si)
    skipped=None
    if skip_on:
        while time.time()-started<120 and p.poll() is None:
            text=(ev/'auto-status.txt').read_text(encoding='utf-8') if (ev/'auto-status.txt').exists() else ''
            if skip_on in text:time.sleep(1.5);user32.PostMessageW(window_of(p.pid),0x0100,0x1B,0);skipped=time.time();break  # WM_KEYDOWN VK_ESCAPE
            time.sleep(0.2)
        assert skipped,'download never started'
    code=p.wait(timeout=300)
    startup=json.loads((ev/'startup.json').read_text()) if (ev/'startup.json').exists() else {}
    state=json.loads(subprocess.run([str(CHECKS),str(root),str(saves),'status'],capture_output=True,text=True).stdout)
    lines=(ev/'auto-status.txt').read_text(encoding='utf-8').splitlines()
    t0=int(lines[0].split()[0]);status=[f'{(int(l.split()[0])-t0)/1000:6.1f}s {l.split(" ",1)[1]}' for l in lines]
    result=dict(run=name,launcherExit=code,status=status,gameStarted=startup.get('game'),ready=startup.get('ready'),installedBuild=state['game']['manifest']['build'],
                versions=sorted(x.name for x in (root/'versions').iterdir()),seconds=round(time.time()-started,1))
    if skipped:result['skipToReadySeconds']=round(pathlib.Path(ev/'startup.json').stat().st_mtime-skipped,1)
    for png in sorted(ev.glob('auto-*.png')):shutil.copy2(png,EVIDENCE/f'{name}-{png.name}')
    return result
EVIDENCE.mkdir(parents=True,exist_ok=True)
for png in EVIDENCE.glob('*-auto-*.png'):png.unlink()
results=[]
r=run('update',base+'/update-catalog.json');results.append(r)
assert r['launcherExit']==0 and r['ready'] and 'versions\\1000001\\Racer.exe' in r['gameStarted'] and r['installedBuild']==1000001,r
Handler.rate=8*1024*1024  # slow enough to press B mid-download
r=run('skip',base+'/update-catalog.json',skip_on='Downloading');results.append(r)
assert r['launcherExit']==0 and r['ready'] and 'versions\\1000000\\Racer.exe' in r['gameStarted'] and r['installedBuild']==1000000 and r['versions']==['1000000'],r
s=socket.socket();s.bind(('127.0.0.1',0));closed=s.getsockname()[1];s.close()
r=run('offline',f'http://127.0.0.1:{closed}/update-catalog.json');results.append(r)
assert r['launcherExit']==0 and r['ready'] and 'versions\\1000000\\Racer.exe' in r['gameStarted'] and r['installedBuild']==1000000,r
assert (LATEST/'state.json').read_bytes()==latest_state,'Builds/Latest changed'
server.shutdown()
out=dict(results=results,latestStateUnchanged=True,testGame=dict(build=1000001,version='1.0.1-test',zipBytes=archive.stat().st_size),work=str(WORK))
(EVIDENCE/'auto-update-check.json').write_text(json.dumps(out,indent=2));print(json.dumps(out,indent=2))
shutil.rmtree(WORK,ignore_errors=True)
