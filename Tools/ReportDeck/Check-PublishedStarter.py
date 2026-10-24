"""Steam Deck launcher round, Part B6: download the published starter ZIP fresh, extract it into an empty folder, and start
1.0.0 through its own launcher (startup check: isolated test save, hidden, batch mode). Never touches Dan's saves."""
import hashlib,json,pathlib,subprocess,time,urllib.request,zipfile
ROOT=pathlib.Path(__file__).resolve().parents[2];EVIDENCE=ROOT/'Docs/ReportDeck/starter'
inventory=json.loads((ROOT/'Builds/StarterRelease-1000000/INVENTORY.json').read_text());asset=inventory['assets'][0]
URL=f"https://github.com/danemoll-jpg/woodstock-rush-releases/releases/download/{inventory['tag']}/{asset['path']}"
WORK=ROOT/'Temp'/('reportdeck-fresh-'+str(time.time_ns()));WORK.mkdir(parents=True);EVIDENCE.mkdir(parents=True,exist_ok=True)
zip_path=WORK/'download'/asset['path'];zip_path.parent.mkdir()
h=hashlib.sha256()
with urllib.request.urlopen(URL,timeout=60) as response,zip_path.open('wb') as out:
    for block in iter(lambda:response.read(1024*1024),b''):h.update(block);out.write(block)
assert zip_path.stat().st_size==asset['bytes'] and h.hexdigest()==asset['sha256'],'Downloaded starter differs from the publisher inventory'
install=WORK/'extracted';install.mkdir()
with zipfile.ZipFile(zip_path) as z:z.extractall(install)
root=install/'WoodstockRush';state=json.loads((root/'state.json').read_text())
saves=WORK/'isolated-saves';saves.mkdir()
(saves/'settings.json').write_text(json.dumps(dict(version=1,master=0,radioOn=False,frameLimit=60,vsync=False)))
si=subprocess.STARTUPINFO();si.dwFlags=subprocess.STARTF_USESHOWWINDOW;si.wShowWindow=0
p=subprocess.Popen([str(root/'WoodstockRushLauncher.exe'),'--startup-check','--test-save',str(saves),'--evidence',str(EVIDENCE)],cwd=root,startupinfo=si)
code=p.wait(timeout=180)
assert code==0 and (EVIDENCE/'startup.json').exists() and not (EVIDENCE/'failure.txt').exists(),'Starter did not start the game'
startup=json.loads((EVIDENCE/'startup.json').read_text())
result=dict(url=URL,bytes=asset['bytes'],sha256=asset['sha256'],downloadMatches=True,installedVersion=state['game']['manifest']['version'],installedBuild=state['game']['manifest']['build'],
            gameStarted=startup['game'],ready=startup['ready'],launcherExit=code,launcherSha256=hashlib.sha256((root/'WoodstockRushLauncher.exe').read_bytes()).hexdigest(),work=str(WORK))
assert result['installedBuild']==1000000 and 'versions\\1000000\\Racer.exe' in startup['game']
(EVIDENCE/'result.json').write_text(json.dumps(result,indent=2));print(json.dumps(result))
