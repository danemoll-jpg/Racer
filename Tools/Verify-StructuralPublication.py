"""One public signed installation/startup check; no race or gameplay tuning."""
import hashlib,json,pathlib,shutil,subprocess,time,urllib.request
ROOT=pathlib.Path(__file__).resolve().parents[1]
EVIDENCE=ROOT/'Docs/CR133-137/hosted'
EVIDENCE.mkdir(exist_ok=True)
WORK=ROOT/'Temp'/('cr133-hosted-'+str(time.time_ns()))
INSTALL=WORK/'install'
SAVES=WORK/'isolated-saves'
SAVES.mkdir(parents=True)
(SAVES/'settings.json').write_text(json.dumps(dict(version=1,master=0,radioOn=False,frameLimit=60,vsync=False)))
CATALOG='https://github.com/danemoll-jpg/woodstock-rush-releases/releases/latest/download/update-catalog.json'
def fetch(url,path):
    with urllib.request.urlopen(url,timeout=40) as response:path.write_bytes(response.read())
def cli(command,*args):
    p=subprocess.run([str(ROOT/'Builds/Launcher/LauncherChecks.exe'),str(INSTALL),str(SAVES),command,*map(str,args)],capture_output=True,text=True,timeout=660)
    if p.returncode:raise RuntimeError(p.stderr)
    return p.stdout
catalog_path=WORK/'catalog.json'
fetch(CATALOG,catalog_path)
catalog=json.loads(cli('verify',catalog_path))
manifest_path=WORK/'game-manifest.json'
fetch(catalog['game'],manifest_path)
manifest=json.loads(cli('verify',manifest_path))
assert manifest['build']==24000 and manifest['version']=='0.24.0-review1'
# Confirm the ordinary Play-Racer.cmd target contains the exact public files.
for item in manifest['files']:
    path=ROOT/'Builds/Latest'/item['path']
    assert path.stat().st_size==item['bytes'],item['path']
    assert hashlib.sha256(path.read_bytes()).hexdigest()==item['sha256'],item['path']
print('Public signed manifest matches every installed Latest game file.',flush=True)
# Exercise the existing updater's actual download, signature, extraction and activation.
cli('game',manifest_path)
state=json.loads((INSTALL/'state.json').read_text())
assert state['game']['manifest']['build']==24000
shutil.copy2(ROOT/'Builds/Launcher/WoodstockRushLauncher.exe',INSTALL)
shutil.copy2(ROOT/'Builds/Latest/WoodstockRush-Cover.png',INSTALL)
si=subprocess.STARTUPINFO();si.dwFlags=subprocess.STARTF_USESHOWWINDOW;si.wShowWindow=0
p=subprocess.Popen([str(INSTALL/'WoodstockRushLauncher.exe'),'--startup-check','--test-save',str(SAVES),'--evidence',str(EVIDENCE)],cwd=INSTALL,startupinfo=si)
code=p.wait(timeout=115)
assert code==0 and (EVIDENCE/'startup.json').exists() and not (EVIDENCE/'failure.txt').exists()
log=(EVIDENCE/'game.log').read_text(errors='replace')
assert 'NullReferenceException' not in log and 'MissingReferenceException' not in log
result=dict(version=manifest['version'],build=24000,catalog=CATALOG,manifest=catalog['game'],publicDownload=True,pinnedSignatureVerified=True,allLatestFilesMatch=True,files=len(manifest['files']),startupReady=True,exit=code,work=str(WORK))
(EVIDENCE/'result.json').write_text(json.dumps(result,indent=2))
print(json.dumps(result))
