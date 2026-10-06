"""0.81 evidence: Blender render sheets (one per new vehicle, the traffic kit) and the in-game check shots (JPEG, max 1600 px
wide) and check results into Docs/Report081."""
import os, glob, shutil, subprocess, sys
from PIL import Image
T = os.path.join(os.environ['LOCALAPPDATA'], 'Temp', 'report081')
root = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
doc = os.path.join(root, 'Docs', 'Report081'); models = os.path.join(doc, 'Models'); shots = os.path.join(doc, 'Shots'); lists = os.path.join(doc, 'Lists')
for d in (models, shots, lists): os.makedirs(d, exist_ok=True)
R = os.path.join(T, 'renders'); sheet = os.path.join(root, 'Tools', 'Blender', 'contact_sheet.py')
for n in ('Roadster', 'Fastback', 'Pebble', 'Skyfin', 'Scrambler', 'Drifter'):
    fs = [os.path.join(R, f'{n}-{v}.png') for v in ('three-quarter', 'side', 'front', 'rear-three-quarter', 'top', 'cabin' if n not in ('Scrambler', 'Drifter') else 'rider-close')]
    subprocess.run([sys.executable, sheet, os.path.join(models, f'{n}.png'), '3', '560'] + fs, check=True)
subprocess.run([sys.executable, sheet, os.path.join(models, 'TrafficKit.png'), '4', '480'] + [os.path.join(R, f'Traffic{n}-{v}.png') for v in ('three-quarter', 'rear-three-quarter') for n in ('Sedan', 'Wagon', 'Pickup', 'Van')], check=True)
def jpg(src, name):
    if not os.path.exists(src): print('missing', src); return
    im = Image.open(src).convert('RGB')
    if im.width > 1600: im = im.resize((1600, round(im.height * 1600 / im.width)), Image.LANCZOS)
    im.save(os.path.join(shots, name), 'JPEG', quality=85)
c = lambda run, f: os.path.join(T, run, 'checks', f)
jpg(c('verify1', 'view-FreeRoamWorld-b001-day.png'), 'BUG-001-after-day.jpg')
jpg(c('verify3', 'view-FreeRoamWorld-b002-day.png'), 'BUG-002-after-reported-position.jpg')
jpg(c('verify3', 'view-FreeRoamWorld-b002far-day.png'), 'BUG-002-after-near-edge.jpg')
jpg(c('verify3', 'view-FreeRoamWorld-summit-day.png'), 'BUG-002-after-summit-east.jpg')
jpg(c('verify1', 'view-FreeRoamWorld-west-day.png'), 'BUG-002-after-west.jpg')
jpg(c('verify1', 'view-FreeRoamWorld-south-day.png'), 'BUG-002-after-south.jpg')
jpg(c('verify2', 'view-FreeRoamWorld-summit2-night.png'), 'BUG-002-after-summit-night.jpg')
jpg(c('verify1', 'wall-FreeRoamWorld-moto-1180-305.png'), 'BUG-002-ride-at-edge.jpg')
for v in ('roadster', 'fastback', 'pebble', 'skyfin', 'scrambler', 'drifter'):
    for k in ('day', 'night', 'wave'): jpg(c('models1', f'vehicle-{v}-{k}.png'), f'vehicle-{v}-{k}.jpg')
    jpg(c('models1', f'garage-{v}.png'), f'garage-{v}.jpg')
for f in glob.glob(c('models1', 'traffic-*.png')): jpg(f, os.path.basename(f)[:-4] + '.jpg')
for f in glob.glob(c('races1', 'people-*.png')): jpg(f, os.path.basename(f)[:-4] + '.jpg')
for run in ('probe2', 'verify1', 'verify2', 'models1', 'models2', 'races1'):
    r = c(run, 'results.txt')
    if os.path.exists(r): shutil.copy(r, os.path.join(lists, f'results-{run}.txt'))
shutil.copy(os.path.join(T, 'bench', 'run', 'conditions.txt'), os.path.join(lists, 'frame-rate-3840x2160.txt'))
for f in glob.glob(os.path.join(T, 'bench', 'run', '*.jpg')): jpg(f, 'bench-' + os.path.basename(f))
print(len(os.listdir(models)), 'model sheets,', len(os.listdir(shots)), 'shots,', len(os.listdir(lists)), 'lists')
