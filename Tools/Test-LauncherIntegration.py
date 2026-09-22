"""One actual signed runtime update, muted startup, and rollback startup."""
import functools,http.server,importlib.util,json,pathlib,shutil,subprocess,threading,time,wave,sys
from cryptography.hazmat.primitives import serialization
ROOT=pathlib.Path(__file__).resolve().parents[1]
spec=importlib.util.spec_from_file_location('release',ROOT/'Tools/Prepare-LauncherRelease.py');r=importlib.util.module_from_spec(spec);spec.loader.exec_module(r)
resume=len(sys.argv)>1
work=pathlib.Path(sys.argv[1]).resolve() if resume else ROOT/'Temp'/('cr119-integration-'+str(time.time_ns()));install=work/'install'
saves=work/'isolated-saves';custom=work/'remembered-custom'
settings=dict(version=1,master=0,ambience=.37,feedback=.41,vehicle=.29,music=.23,frameLimit=60,vsync=False,musicSource='bundled',musicFolder=str(custom),radioOn=False)
playlist=dict(version=1,playlists=[dict(name='Preserve this playlist',entries=[dict(course=4,laps=1)])]);personal=install/'shared/PersonalMusic/Personal fixture'
if not resume:
    shutil.copytree(ROOT/'Builds/LauncherRelease-21001/starter',install)
    shutil.copy2(ROOT/'Builds/Launcher/WoodstockRushLauncher.exe',install);shutil.copy2(ROOT/'Builds/Latest/WoodstockRush-Cover.png',install)
    saves.mkdir();custom.mkdir();(saves/'settings.json').write_text(json.dumps(settings));(saves/'race-playlists-v1.json').write_text(json.dumps(playlist));personal.mkdir()
    with wave.open(str(personal/'preserved.wav'),'wb') as f:f.setnchannels(1);f.setsampwidth(2);f.setframerate(8000);f.writeframes(b'\0\0'*800)
playlist_hash=__import__('hashlib').sha256(json.dumps(playlist).encode()).hexdigest()
class Handler(http.server.SimpleHTTPRequestHandler):
    def log_message(self,*a):pass
assets=ROOT/'Builds/LauncherRelease-21002/assets';server=http.server.ThreadingHTTPServer(('127.0.0.1',0),functools.partial(Handler,directory=str(assets)));threading.Thread(target=server.serve_forever,daemon=True).start()
manifest=json.loads((ROOT/'Builds/LauncherRelease-21002/INVENTORY.json').read_text())['game'];manifest['package']['url']=f'http://127.0.0.1:{server.server_port}/game.zip'
key=serialization.load_pem_private_key((ROOT/'Builds/PublisherPrivate/launcher-signing.pem').read_bytes(),None);signed=work/'fixture-game.json';signed.write_text(json.dumps(r.sign(manifest,key)))
evidence=ROOT/'Docs/CR119/integration';evidence.mkdir(exist_ok=True);results=[]
def cli(command,*args):subprocess.run([str(ROOT/'Builds/Launcher/LauncherChecks.exe'),str(install),str(saves),command,*map(str,args)],check=True,capture_output=True,text=True,timeout=180)
def startup(name,build):
    out=evidence/name;out.mkdir(exist_ok=True);si=subprocess.STARTUPINFO();si.dwFlags=subprocess.STARTF_USESHOWWINDOW;si.wShowWindow=0
    p=subprocess.Popen([str(install/'WoodstockRushLauncher.exe'),'--startup-check','--test-save',str(saves),'--evidence',str(out)],startupinfo=si,cwd=install)
    try: code=p.wait(timeout=115)
    except subprocess.TimeoutExpired:raise RuntimeError('Launcher startup did not finish; retained running child diagnostics for inspection')
    assert code==0 and (out/'startup.json').exists() and not (out/'failure.txt').exists(),(code,out)
    state=json.loads((install/'state.json').read_text());assert state['game']['manifest']['build']==build and 'running' not in state
    saved=json.loads((saves/'settings.json').read_text())
    for k,v in settings.items():assert (abs(saved[k]-v)<1e-6 if isinstance(v,float) else saved[k]==v),(k,saved[k],v)
    assert r.digest(saves/'race-playlists-v1.json')==playlist_hash
    assert (personal/'preserved.wav').exists()
    results.append(dict(check=name,build=build,exit=code,ready=True,muted=True,settingsAndPlaylistPreserved=True))
try:
    if not resume:cli('game',signed);startup('updated-startup',21002)
    else:
        saved=json.loads((saves/'settings.json').read_text())
        for k,v in settings.items():assert (abs(saved[k]-v)<1e-6 if isinstance(v,float) else saved[k]==v),(k,saved[k],v)
        assert r.digest(saves/'race-playlists-v1.json')==playlist_hash
        assert json.loads((evidence/'updated-startup/startup.json').read_text())['ready']
        results.append(dict(check='updated-startup',build=21002,exit=0,ready=True,muted=True,settingsAndPlaylistPreserved=True,reusedPriorSuccessfulStartup=True))
    log=(evidence/'updated-startup/game.log').read_text(errors='replace');line=next(x for x in log.splitlines() if 'LAUNCHER_DISCOVERY ' in x);discovery=json.loads(line.split('LAUNCHER_DISCOVERY ',1)[1]);assert set(discovery['stations'])=={'Groove','Louis Prima','Personal fixture','Punk Rock Classics','Rock Classics'} and discovery['tracks']==0,discovery # Count is the selected station; radio is deliberately Off.
    cli('rollback');startup('rollback-startup',21001)
    assert list((install/'save-backups').rglob('race-playlists-v1.json'))
    (evidence/'results.json').write_text(json.dumps(dict(work=str(work),checks=results,discovery=discovery,saveBackups=True),indent=2));print(json.dumps(results))
finally:server.shutdown()
