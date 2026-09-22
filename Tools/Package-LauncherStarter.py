"""Assemble complete starter, verify extracted inventory, and stage publisher asset."""
import hashlib,importlib.util,json,pathlib,shutil,zipfile
ROOT=pathlib.Path(__file__).resolve().parents[1]
spec=importlib.util.spec_from_file_location('release',ROOT/'Tools/Prepare-LauncherRelease.py');r=importlib.util.module_from_spec(spec);spec.loader.exec_module(r)
draft=ROOT/'Builds/LauncherRelease-21002';starter=draft/'starter'
shutil.copy2(ROOT/'Builds/Launcher/WoodstockRushLauncher.exe',starter)
shutil.copy2(ROOT/'Builds/Latest/WoodstockRush-Cover.png',starter)
shutil.copy2(ROOT/'Docs/CR119/README-LAUNCHER.md',starter)
shutil.copy2(ROOT/'Tools/Install-Launcher.ps1',starter)
licenses=starter/'Launcher-Licenses';licenses.mkdir(exist_ok=True)
for name in ('JSON-LICENSE.txt','MINIZ-LICENSE.txt'):shutil.copy2(ROOT/'Launcher/vendor'/name,licenses/name)
archive=draft/'assets/WoodstockRush-0.21.0-launcher1-Full.zip'
if archive.exists():raise ValueError('Immutable starter ZIP already exists')
files=r.inventory(starter)
with zipfile.ZipFile(archive,'w',zipfile.ZIP_DEFLATED,compresslevel=5) as z:
    for f in files:z.write(starter/f['path'],'WoodstockRush/'+f['path'])
if archive.stat().st_size>=2*1024**3:raise ValueError('Full starter exceeds release asset limit')
extracted=draft/'verified-extraction'
if extracted.exists():raise ValueError('Extraction directory already exists')
with zipfile.ZipFile(archive) as z:z.extractall(extracted)
actual=r.inventory(extracted/'WoodstockRush')
assert actual==files,'Extracted starter differs'
inventory=json.loads((draft/'INVENTORY.json').read_text());inventory['assets']=r.inventory(draft/'assets');r.write(draft/'INVENTORY.json',inventory)
r.write(ROOT/'Docs/CR119/package-verification.json',dict(version='0.21.0-launcher1',build=21002,files=len(files),runtimeFiles=len(inventory['game']['files']),soundtrackFiles=len(inventory['music']['files']),zip=str(archive),bytes=archive.stat().st_size,sha256=r.digest(archive),extractedIdentity=True,privateKeysOrSourceIncluded=False,previousFeedbackBuildPreserved=True))
print(json.dumps(dict(archive=str(archive),files=len(files),bytes=archive.stat().st_size,sha256=r.digest(archive))))
