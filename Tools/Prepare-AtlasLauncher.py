"""Use the existing signed Store.Seed + Install-Launcher workflow; no new identity."""
import base64,json,pathlib,shutil,subprocess,urllib.request
root=pathlib.Path(__file__).resolve().parents[1]
draft=root/'Builds/LauncherRelease-30000/assets'
starter=root/'Temp/route-atlas-launcher-starter'
if starter.exists():raise RuntimeError('Starter already exists; inspect before retry')
starter.mkdir(parents=True)
catalog=json.loads(base64.b64decode(json.loads((draft/'update-catalog.json').read_text())['payload']))
music=starter/'soundtrack-manifest.json'
with urllib.request.urlopen(catalog['soundtrack'],timeout=40) as response:music.write_bytes(response.read())
game=draft/'game-manifest.json'
subprocess.run([str(root/'Builds/Launcher/LauncherChecks.exe'),str(starter),str(root/'Temp/route-atlas-launcher-saves'),'seed',str(root/'Builds/Latest'),str(game),str(root/'BundleMusic'),str(music)],check=True)
shutil.copy2(game,starter/'game-manifest.json')
for name,source in [('WoodstockRushLauncher.exe',root/'Builds/Launcher/WoodstockRushLauncher.exe'),('WoodstockRush-Cover.png',root/'Builds/Latest/WoodstockRush-Cover.png')]:shutil.copy2(source,starter/name)
print('Signed starter prepared with the existing launcher, verified game and staged music:',starter)
