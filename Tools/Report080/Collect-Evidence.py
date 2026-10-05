"""0.80 evidence: Dan's report screenshots (before), the check screenshots (after, as JPEG, max 1920 px wide) and the
check lists and tables into Docs/Report080."""
import os, glob, shutil
from PIL import Image
T = os.path.join(os.environ['LOCALAPPDATA'], 'Temp', 'report080')
root = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
shots = os.path.join(root, 'Docs', 'Report080', 'Shots'); lists = os.path.join(root, 'Docs', 'Report080', 'Lists')
os.makedirs(shots, exist_ok=True); os.makedirs(lists, exist_ok=True)
def jpg(src, name):
    if not os.path.exists(src): print('missing', src); return
    im = Image.open(src).convert('RGB')
    if im.width > 1920: im = im.resize((1920, round(im.height * 1920 / im.width)), Image.LANCZOS)
    im.save(os.path.join(shots, name), 'JPEG', quality=85)
dan = os.path.join(os.environ['USERPROFILE'], 'AppData', 'LocalLow', 'DefaultCompany', 'Racer', 'DebugReports', '2026-10-05_14-24-33-951_f0ab5f', 'Screenshots')
for i in range(1, 11): jpg(os.path.join(dan, f'BUG-{i:03d}.png'), f'before-BUG-{i:03d}-dan-0.79.jpg')
# before, our own views on the 0.79 state
jpg(os.path.join(T, 'probe3', 'checks', 'top-FreeRoamWorld-trickum.png'), 'before-junction-trickum-top.jpg')
jpg(os.path.join(T, 'probe3', 'checks', 'top-FreeRoamWorld-cherokee.png'), 'before-junction-cherokee-top.jpg')
jpg(os.path.join(T, 'probe3', 'checks', 'view-MountainLoopReverse-b010eye-day.png'), 'before-BUG-010-day.jpg')
jpg(os.path.join(T, 'probe3', 'checks', 'view-MountainLoopReverse-b008eye-day.png'), 'before-BUG-008-day.jpg')
# after
for n in ('BUG-001', 'BUG-002', 'BUG-003', 'BUG-006', 'BUG-007'): jpg(os.path.join(T, 'verifyB', 'checks', f'view-FreeRoamWorld-{n}-day.png'), f'after-{n}-day.jpg')
for n in ('BUG-008', 'BUG-008wide', 'BUG-009', 'BUG-009race', 'BUG-010', 'BUG-010side'): jpg(os.path.join(T, 'verifyB', 'checks', f'view-MountainLoopReverse-{n}-day.png'), f'after-{n}-day.jpg')
for n in ('BUG-004', 'BUG-005'):
    jpg(os.path.join(T, 'verifyG', 'checks', f'view-FreeRoamWorld-{n}-day.png'), f'after-{n}-day.jpg')
    jpg(os.path.join(T, 'verifyG', 'checks', f'view-FreeRoamWorld-{n}n-night.png'), f'after-{n}-night.jpg')
for n in ('trickum', 'trickeast', 'seamend', 'cherokee', 'cheroseam', 'stc'): jpg(os.path.join(T, 'verifyG', 'checks', f'top-FreeRoamWorld-{n}.png'), f'after-junction-{n}-top.jpg')
jpg(os.path.join(T, 'verifyA', 'checks', 'view-FreeRoamWorld-b006up-day.png'), 'after-BUG-006-above-day.jpg')
jpg(os.path.join(T, 'verifyB', 'checks', 'view-MountainLoopReverse-tree-day.png'), 'after-embankment-861-58-day.jpg')
for p in ('moto', 'atv'): jpg(os.path.join(T, 'partc1', 'checks', f'jump-landing-{p}.png'), f'after-giant-jump-landing-{p}.jpg')
# lists and tables
copies = {
    ('verifyG', 'load-report.txt'): 'load-report-FreeRoamWorld-StreetLoop.txt',
    ('verifyB', 'buried-MountainLoopReverse.txt'): 'buried-MountainLoopReverse.txt',
    ('verifyB', 'escape-MountainLoopReverse-moto.txt'): 'escape-MountainLoopReverse-moto.txt',
    ('partc3', 'escape-MountainLoopReverse-atv.txt'): 'escape-MountainLoopReverse-atv.txt',
    ('partc1', 'ride-table.tsv'): 'partC-shortcut-rides.tsv',
    ('partc2', 'force-table.tsv'): 'partC-shortcut-ai-forced.tsv',
    ('partc2', 'reset-table.tsv'): 'partC-resets.tsv',
    ('partc3', 'force-table.tsv'): 'partC-shortcut-ai-forced-controls.tsv',
    ('partc3', 'race-table.tsv'): 'partC-races.tsv',
    ('partc4', 'race-table.tsv'): 'partC-races-backyard-1x.tsv',
    ('probe5', 'load-report.txt'): 'load-report-course-scenes.txt',
    ('verifyB', 'race-table.tsv'): 'partC-races-mountain-final.tsv',
}
for (run, f), name in copies.items():
    src = os.path.join(T, run, 'checks', f)
    if os.path.exists(src): shutil.copy(src, os.path.join(lists, name))
    else: print('missing', src)
for run in ('probe5', 'verifyA', 'partc1', 'partc2', 'partc3', 'partc4', 'verifyB', 'verifyG'):
    r = os.path.join(T, run, 'checks', 'results.txt')
    if os.path.exists(r): shutil.copy(r, os.path.join(lists, 'results-' + run + '.txt'))
print(len(os.listdir(shots)), 'shots,', len(os.listdir(lists)), 'lists')
