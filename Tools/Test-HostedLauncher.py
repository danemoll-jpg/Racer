"""One public signed update and muted startup; no gameplay/audio regression suite."""
import hashlib,json,pathlib,shutil,subprocess,time,urllib.request
ROOT=pathlib.Path(__file__).resolve().parents[1]
CATALOG='https://github.com/danemoll-jpg/woodstock-rush-releases/releases/latest/download/update-catalog.json'
MANIFEST='https://github.com/danemoll-jpg/woodstock-rush-releases/releases/download/build-21002/game-manifest.json'
work=ROOT/'Temp'/('cr119-hosted-'+str(time.time_ns()));install=work/'install';saves=work/'isolated-saves';evidence=ROOT/'Docs/CR119/hosted';evidence.mkdir(exist_ok=True)
shutil.copytree(ROOT/'Builds/LauncherRelease-21001/starter',install)
shutil.copy2(ROOT/'Builds/Launcher/WoodstockRushLauncher.exe',install);shutil.copy2(ROOT/'Builds/Latest/WoodstockRush-Cover.png',install)
saves.mkdir();(saves/'settings.json').write_text(json.dumps(dict(version=1,master=0,radioOn=False,musicSource='bundled',frameLimit=60,vsync=False)))
playlist=saves/'race-playlists-v1.json';playlist.write_text(json.dumps(dict(version=1,playlists=[dict(name='Hosted update preservation',entries=[dict(course=4,laps=1)])])))
original=hashlib.sha256(playlist.read_bytes()).hexdigest()
def cli(command,*args):
    p=subprocess.run([str(ROOT/'Builds/Launcher/LauncherChecks.exe'),str(install),str(saves),command,*map(str,args)],capture_output=True,text=True,timeout=660)
    if p.returncode:raise RuntimeError(p.stderr)
    return p.stdout
check=json.loads(cli('check',CATALOG));assert check['gameAvailable'] and not check['musicAvailable'],check
with urllib.request.urlopen(MANIFEST,timeout=30) as response: payload=response.read()
manifest=work/'hosted-manifest.json';manifest.write_bytes(payload)
cli('game',manifest)
state=json.loads((install/'state.json').read_text());assert state['game']['manifest']['build']==21002
si=subprocess.STARTUPINFO();si.dwFlags=subprocess.STARTF_USESHOWWINDOW;si.wShowWindow=0
p=subprocess.Popen([str(install/'WoodstockRushLauncher.exe'),'--startup-check','--test-save',str(saves),'--evidence',str(evidence)],cwd=install,startupinfo=si)
code=p.wait(timeout=115);assert code==0 and (evidence/'startup.json').exists() and not (evidence/'failure.txt').exists()
assert hashlib.sha256(playlist.read_bytes()).hexdigest()==original
assert json.loads((saves/'settings.json').read_text())['master']==0
state=json.loads((install/'state.json').read_text());assert 'running' not in state
(evidence/'startup-output.txt').write_text((evidence/'game.log').read_text())
result=dict(catalog=CATALOG,manifest=MANIFEST,fromBuild=21001,toBuild=21002,publicDownload=True,signatureAndInventoryVerified=True,packageBytes=state['game']['manifest']['package']['bytes'],startupReady=True,parentLifetime=True,exit=code,muted=True,playlistPreserved=True,work=str(work))
(evidence/'result.json').write_text(json.dumps(result,indent=2));print(json.dumps(result))
