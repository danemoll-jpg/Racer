from pathlib import Path
import json, subprocess, hashlib
root=Path(__file__).resolve().parents[1]; baseline='53dc95a622abf2103f1f45f7dc1abd395524f7e0'
paths=subprocess.check_output(['git','diff','--name-only',baseline],cwd=root,text=True).splitlines()
protected=[p for p in paths if p.startswith(('Assets/Scenes/','Assets/Track/','Assets/Materials/'))]
assert not protected,protected
for name in ['RaceDirector','RaceProgress','RacerState','AiFinishEstimate','RacerSave','RecordBoards','RaceRoad','RoadDriver','RacingMiniMap']:
 p='Assets/Scripts/'+name+'.cs'
 current=(root/p).read_bytes().replace(b'\r\n',b'\n')
 old=subprocess.check_output(['git','show',baseline+':'+p],cwd=root).replace(b'\r\n',b'\n')
 assert current==old,name
data=json.loads((root/'Docs/BackyardForward/map-scene.json').read_text())
old=json.loads((root/'Docs/BackyardForward/geometry.json').read_text())
assert data['route']==old['route']
assert len(data['gates'])==9
for gate in old['gates']:
 actual=next(g for g in data['gates'] if g['name']==gate['name'])
 assert all(abs(actual[k]-gate['position'][k])<.0001 for k in ['x','y','z'])
scene=root/data['scene']
result=dict(baseline=baseline,protectedGeometryChanges=protected,unchangedRoutePoints=len(data['route']),unchangedGates=9,sceneSha256=hashlib.sha256(scene.read_bytes()).hexdigest(),unchangedSystems='estimation, timing, progression, AI, records, saves, roads, minimap')
(root/'Docs/TrackUi/preservation.json').write_text(json.dumps(result,indent=2));print(json.dumps(result))
