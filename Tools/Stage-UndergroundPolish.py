"""Stage only the freshly built signed inventory, keeping the named launcher intact."""
import pathlib,json,base64,hashlib,shutil,zipfile,subprocess
root=pathlib.Path(__file__).resolve().parents[1];out=root/'Docs/UndergroundPolish';draft=root/'Builds/LauncherRelease-56000/assets'
runtime=root/'Builds/Racer-0.56.0-review1-Windows';latest=root/'Builds/Latest';versioned=latest/'versions/56000'
def digest(p):return hashlib.sha256(p.read_bytes()).hexdigest()
manifest=json.loads(subprocess.check_output([str(root/'Builds/Launcher/LauncherChecks.exe'),str(root/'Temp/undergroundpolish-signature-check'),str(root/'Temp/undergroundpolish-signature-saves'),'verify',str(draft/'game-manifest.json')],text=True))
assert manifest['build']==56000
commit=(root/'Temp/undergroundpolish-source-commit.txt').read_text().strip()
assert commit in (runtime/'VERSION.txt').read_text() and commit in manifest['notes']
prior=json.loads((latest/'state.json').read_text())['game'];assert prior['manifest']['build']==55000
old={i['path']:i for i in prior['manifest']['files']};changed=[]
with zipfile.ZipFile(draft/'game.zip') as archive:
    assert set(archive.namelist())=={i['path'] for i in manifest['files']}
    for item in manifest['files']:
        p=runtime/item['path'];assert p.stat().st_size==item['bytes'] and digest(p)==item['sha256'],item['path']
        assert hashlib.sha256(archive.read(item['path'])).hexdigest()==item['sha256'],item['path']
        if item['path'] not in old or old[item['path']]['sha256']!=item['sha256']:changed.append(item['path'])
assert any(p.startswith('Racer_Data/') and not p.endswith('.txt') for p in changed),'Only metadata changed'
assert not versioned.exists(),'Do not overwrite an existing version; inspect first'
launcherHash=digest(latest/'WoodstockRushLauncher.exe')
new_paths={i['path'] for i in manifest['files']}
obsolete=[]
for relative,item in old.items():
    if relative in new_paths:continue
    target=(latest/relative).resolve()
    assert target.is_relative_to(latest.resolve()),'Old manifest path escapes Latest'
    if target.exists():
        assert target.is_file() and digest(target)==item['sha256'],'Unexpected modification to obsolete runtime file: '+relative
        obsolete.append(target)
for destination in [versioned,latest]:
    for item in manifest['files']:
        target=destination/item['path'];target.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(runtime/item['path'],target)
    for item in manifest['files']:assert digest(destination/item['path'])==item['sha256'],str(destination/item['path'])
for target in obsolete:target.unlink()
assert digest(latest/'WoodstockRushLauncher.exe')==launcherHash
assert digest(latest/'Racer.exe')!=launcherHash,'Root Racer.exe must be the Unity game, not launcher alias'
result=dict(version=manifest['version'],build=56000,sourceCommit=commit,files=len(manifest['files']),changedRuntimeFiles=changed,buildOutput=str(runtime),latest=str(latest/'Racer.exe'),versioned=str(versioned/'Racer.exe'),exeSha256=digest(latest/'Racer.exe'),previousExeSha256=old['Racer.exe']['sha256'],inventoryAndZipMatch=True,namedLauncherPreserved=True)
result['removedObsoletePriorGameFiles']=[p.relative_to(latest).as_posix() for p in obsolete]
(out/'runtime-identity.json').write_text(json.dumps(result,indent=2));print(json.dumps(result))

