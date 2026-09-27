"""Use the existing signed Store.Seed + Install-Launcher workflow; no new identity."""
import base64,json,pathlib,shutil,subprocess,urllib.request
root=pathlib.Path(__file__).resolve().parents[1]
draft=root/'Builds/LauncherRelease-30000/assets'
starter=root/'Temp/route-atlas-launcher-starter'
if starter.exists():
    # Resume only the inspected, uninitialized Seed attempt; never overwrite an install.
    allowed={'launcher.lock','soundtrack-manifest.json'}
    if any(p.relative_to(starter).as_posix() not in allowed for p in starter.rglob('*') if p.is_file()):raise RuntimeError('Unexpected existing starter content')
else:starter.mkdir(parents=True)
catalog=json.loads(base64.b64decode(json.loads((draft/'update-catalog.json').read_text())['payload']))
music=starter/'soundtrack-manifest.json'
with urllib.request.urlopen(catalog['soundtrack'],timeout=40) as response:music.write_bytes(response.read())
game=draft/'game-manifest.json'
verified=json.loads((root/'Docs/RouteAtlas/hosted/result.json').read_text())
assert verified['build']==30000 and verified['startupReady']
installed=pathlib.Path(verified['work'])/'install'
state=json.loads((installed/'state.json').read_text())
source=installed/state['game']['directory']
# The public install has the exact signed inventory; flat Latest also contains music.
subprocess.run([str(root/'Builds/Launcher/LauncherChecks.exe'),str(starter),str(root/'Temp/route-atlas-launcher-saves'),'seed',str(source),str(game),str(root/'BundleMusic'),str(music)],check=True)
shutil.copy2(game,starter/'game-manifest.json')
for name,source in [('WoodstockRushLauncher.exe',root/'Builds/Launcher/WoodstockRushLauncher.exe'),('WoodstockRush-Cover.png',root/'Builds/Latest/WoodstockRush-Cover.png')]:shutil.copy2(source,starter/name)
print('Signed starter prepared with the existing launcher, verified game and staged music:',starter)
