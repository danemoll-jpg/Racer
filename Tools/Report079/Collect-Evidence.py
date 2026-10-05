"""0.79 evidence: copies the chosen check screenshots (as JPEG, max 1920 px wide) and the per-scene lists into Docs/Report079."""
import os, glob, shutil
from PIL import Image
T = os.path.join(os.environ['LOCALAPPDATA'], 'Temp', 'report079')
root = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
shots = os.path.join(root, 'Docs', 'Report079', 'Shots'); lists = os.path.join(root, 'Docs', 'Report079', 'Lists')
os.makedirs(shots, exist_ok=True); os.makedirs(lists, exist_ok=True)
def jpg(src, name):
    im = Image.open(src).convert('RGB')
    if im.width > 1920: im = im.resize((1920, round(im.height * 1920 / im.width)), Image.LANCZOS)
    im.save(os.path.join(shots, name), 'JPEG', quality=85)
# before (0.78 code, survey run) and after (0.79) at the reported spots
for f in glob.glob(os.path.join(T, 'survey', 'checks', 'survey-*.png')):
    n = os.path.basename(f)
    if 'junction' in n or 'hwy92' in n: continue
    jpg(f, 'before-' + n[len('survey-'):-4] + '.jpg')
for f in glob.glob(os.path.join(T, 'final', 'checks', 'after-*.png')): jpg(f, os.path.basename(f)[:-4] + '.jpg')
for f in glob.glob(os.path.join(T, 'final', 'checks', 'roads-*.png')): jpg(f, os.path.basename(f)[:-4] + '.jpg')
for f in glob.glob(os.path.join(T, 'final', 'checks', 'sign-*.png')): jpg(f, os.path.basename(f)[:-4] + '.jpg')
for f in glob.glob(os.path.join(T, 'final3', 'checks', '*.png')):
    n = os.path.basename(f)
    if n.startswith(('hud-', 'menu-', 'trailer-')): jpg(f, n[:-4] + '.jpg')
# lists: buildings, props, trees, sign placement, results
for run in ('full3', 'final2', 'final', 'final3', 'full'):
    for f in glob.glob(os.path.join(T, run, 'checks', '*.txt')):
        n = os.path.basename(f)
        if n.startswith(('grounding-', 'trees-drivable-')) and not os.path.exists(os.path.join(lists, n)): shutil.copy(f, os.path.join(lists, n))
    r = os.path.join(T, run, 'checks', 'results.txt')
    if os.path.exists(r): shutil.copy(r, os.path.join(lists, 'results-' + run + '.txt'))
print(len(os.listdir(shots)), 'shots,', len(os.listdir(lists)), 'lists')
