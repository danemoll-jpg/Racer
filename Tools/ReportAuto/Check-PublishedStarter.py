"""Automatic-update round, Part C4: download the published starter ZIP fresh into an empty folder, check the icon and poster in
its top folder, and let its launcher run the automatic flow against the real update catalog (isolated test save, hidden,
batch mode): it should find 1.0.0 up to date and start it by itself. Never touches Dan's saves."""
import hashlib,json,pathlib,shutil,subprocess,time,urllib.request,zipfile
ROOT=pathlib.Path(__file__).resolve().parents[2];EVIDENCE=ROOT/'Docs/ReportAuto/starter'
inventory=json.loads((ROOT/'Builds/StarterRelease-1000000-r2/INVENTORY.json').read_text());asset=inventory['assets'][0]
URL=f"https://github.com/danemoll-jpg/woodstock-rush-releases/releases/download/{inventory['tag']}/{asset['path']}"
WORK=ROOT/'Temp'/('reportauto-fresh-'+str(time.time_ns()));WORK.mkdir(parents=True)
if EVIDENCE.exists():shutil.rmtree(EVIDENCE)
EVIDENCE.mkdir(parents=True)
zip_path=WORK/'download'/asset['path'];zip_path.parent.mkdir()
h=hashlib.sha256()
with urllib.request.urlopen(URL,timeout=60) as response,zip_path.open('wb') as out:
    for block in iter(lambda:response.read(1024*1024),b''):h.update(block);out.write(block)
assert zip_path.stat().st_size==asset['bytes'] and h.hexdigest()==asset['sha256'],'Downloaded starter differs from the publisher inventory'
install=WORK/'extracted';install.mkdir()
with zipfile.ZipFile(zip_path) as z:z.extractall(install)
root=install/'WoodstockRush';top=sorted(p.name for p in root.iterdir() if p.is_file())
images={n:hashlib.sha256((root/n).read_bytes()).hexdigest()==hashlib.sha256(s.read_bytes()).hexdigest() for n,s in
        (('WoodstockRush-Icon.png',ROOT/'Assets/Branding/WoodstockRushIcon.png'),('WoodstockRush-Poster.png',ROOT/'SourceArt/Poster/WoodstockRushPoster.png'))}
assert all(images.values()),images
saves=WORK/'isolated-saves';saves.mkdir()
(saves/'settings.json').write_text(json.dumps(dict(version=1,master=0,radioOn=False,frameLimit=60,vsync=False)))
si=subprocess.STARTUPINFO();si.dwFlags=subprocess.STARTF_USESHOWWINDOW;si.wShowWindow=0
p=subprocess.Popen([str(root/'WoodstockRushLauncher.exe'),'--auto-check','--test-save',str(saves),'--evidence',str(EVIDENCE)],cwd=root,startupinfo=si)
code=p.wait(timeout=300)
assert code==0 and (EVIDENCE/'startup.json').exists() and not (EVIDENCE/'failure.txt').exists(),'Starter did not start the game'
startup=json.loads((EVIDENCE/'startup.json').read_text());lines=(EVIDENCE/'auto-status.txt').read_text(encoding='utf-8').splitlines()
t0=int(lines[0].split()[0]);status=[f'{(int(l.split()[0])-t0)/1000:6.1f}s {l.split(" ",1)[1]}' for l in lines]
state=json.loads((root/'state.json').read_text())
result=dict(url=URL,bytes=asset['bytes'],sha256=asset['sha256'],downloadMatches=True,topFolder=top,imagesMatchSources=images,status=status,
            installedBuild=state['game']['manifest']['build'],gameStarted=startup['game'],ready=startup['ready'],launcherExit=code,
            launcherSha256=hashlib.sha256((root/'WoodstockRushLauncher.exe').read_bytes()).hexdigest())
assert result['installedBuild']==1000000 and 'versions\\1000000\\Racer.exe' in startup['game'] and any('Up to date' in s for s in status),result
(EVIDENCE/'result.json').write_text(json.dumps(result,indent=2));print(json.dumps(result,indent=2))
shutil.rmtree(WORK,ignore_errors=True)
