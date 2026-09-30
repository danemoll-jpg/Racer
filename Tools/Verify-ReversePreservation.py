"""Bounded preservation checks against the pre-task safety checkpoint."""
import json,re,subprocess
from pathlib import Path
r=Path(__file__).resolve().parents[1]
base='50c108a7ba9dd51db89557bc4d7f17a0bd75cbf5'
def blocks(s):
 return {re.search(r'^--- !u!\d+ &(\d+)',b)[1]:b for b in re.split(r'(?=^--- !u!)',s,flags=re.M) if b.startswith('--- !u!')}
def guid(name):return re.search(r'guid: (\w+)',(r/f'Assets/Scripts/{name}.cs.meta').read_text())[1]
rel='Assets/Scenes/DansBackyardForward.unity'
a=blocks(subprocess.check_output(['git','show',base+':'+rel],cwd=r).decode())
b=blocks((r/rel).read_text())
types={guid(n):n for n in ['RaceRoad','RaceGate','WoodlandRoute','BackyardForwardCourse','ForestLayout','ShortcutUndergrowth']}
checked=[]
for key,block in a.items():
 m=re.search(r'm_Script: .*guid: (\w+)',block)
 if not m or m[1] not in types:continue
 assert b.get(key)==block, f'Protected Forward {types[m[1]]} changed: {key}'
 checked.append({'type':types[m[1]],'id':key})
protected=['RacingMiniMap','RaceFlow','ArcadeVehicle','RoadDriver','RaceProgress','RaceRoad','WoodlandRoute','ShortcutStrategy','BackyardForwardCourse','VehicleRespawn','BreakableProp']
for n in protected:
 subprocess.run(['git','diff','--exit-code',base,'--',f'Assets/Scripts/{n}.cs'],cwd=r,check=True,stdout=subprocess.DEVNULL)
changed=subprocess.check_output(['git','diff','--name-only',base],cwd=r,text=True).splitlines()
assert not any(p.startswith(('Assets/Track/BackyardForward/','Assets/Track/BackyardShortcuts/')) for p in changed), 'Approved Forward mesh assets modified'
(r/'Docs/BackyardReverse/preservation.json').write_text(json.dumps({'checkpoint':base,'forwardComponentsUnchanged':checked,'globalSystemsUnchanged':protected,'approvedForwardMeshAssets':'unchanged'},indent=2))
print(f'PASS: {len(checked)} Forward route/gate/shortcut/flight/undergrowth components, approved mesh assets and {len(protected)} global systems unchanged.')
