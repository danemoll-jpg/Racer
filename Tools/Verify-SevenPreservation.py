from pathlib import Path
import subprocess,re,json
root=Path(__file__).resolve().parents[1]; baseline='7dc6292069ee7f2b05233d2d06bf0de6140bb151'
def old(p):return subprocess.check_output(['git','show',baseline+':'+p],cwd=root).decode().replace('\r\n','\n')
def blocks(s):return {re.search(r'^--- !u!\d+ &(\d+)',b).group(1):b for b in re.split(r'(?=^--- !u!)',s,flags=re.M) if b.startswith('--- !u!')}
checks=[]
for name in ['StreetLoopGreybox','StreetLoopReverse','ForestLoopReverse','DansBackyardForward','MountainLoop','MountainLoopReverse']:
 p='Assets/Scenes/'+name+'.unity'; assert old(p)==(root/p).read_text(encoding='utf-8-sig'),name;checks.append(name+' unchanged')
for name in ['RaceDirector','RaceProgress','RacerState','RaceRoad','RacingMiniMap','ShortcutStrategy','ArcadeVehicle','RoadDriver','RaceFlow','LocalRadio','VehicleRespawn','VehicleSurfaceContacts']:
 p='Assets/Scripts/'+name+'.cs';assert old(p)==(root/p).read_text(encoding='utf-8-sig'),name;checks.append(name+' unchanged')
gate_guid=re.search(r'guid: (\w+)',(root/'Assets/Scripts/RaceGate.cs.meta').read_text()).group(1)
details={}
for name in ['DansBackyardReverse','LakeWoods']:
 p='Assets/Scenes/'+name+'.unity';a=blocks(old(p));b=blocks((root/p).read_text(encoding='utf-8-sig'))
 gates=[k for k,v in a.items() if gate_guid in v];assert all(a[k]==b.get(k) for k in gates),name+' gates changed'
 preserved=[]
 for k,v in a.items():
  n=re.search(r'  m_Name: (.*)',v)
  if not n or not re.search(r'\b(house|garage|kennel|shed|fence|gate|pool)\b',n[1],re.I):continue
  if k not in b:continue
  transform_ids=[m[1] for m in re.finditer(r'component: \{fileID: (\d+)\}',v) if a.get(m[1],'').startswith('--- !u!4 ')]
  for t in transform_ids:assert a[t]==b.get(t),name+' protected structure moved '+n[1]
  preserved.append(n[1])
 details[name]=dict(gatesUnchanged=len(gates),protectedStructureTransforms=preserved)
(root/'Docs/SevenCorrections/preservation.json').write_text(json.dumps(dict(baseline=baseline,checks=checks,scenes=details),indent=2))
print('PASS six other scenes, twelve global systems, gates and protected structure transforms')

