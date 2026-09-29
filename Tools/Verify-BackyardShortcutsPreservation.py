from pathlib import Path
import subprocess,re,json
root=Path(__file__).resolve().parents[1]; baseline='79797bc57e49da9f5de6a8cb86f972c82bd672d4'
def old(p):return subprocess.check_output(['git','show',baseline+':'+p],cwd=root).decode().replace('\r\n','\n')
def blocks(s):return {re.search(r'^--- !u!\d+ &(\d+)',b).group(1):b for b in re.split(r'(?=^--- !u!)',s,flags=re.M) if b.startswith('--- !u!')}
p='Assets/Scenes/DansBackyardForward.unity';a=blocks(old(p));b=blocks((root/p).read_text(encoding="utf-8"));changes=[];removed=[]
for key,block in a.items():
 if key not in b:removed.append(key);continue
 if block!=b[key]:changes.append(dict(id=key,before=block,after=b[key]))
for name in ['StreetLoopGreybox','StreetLoopReverse','LakeWoods','ForestLoopReverse','MountainLoop','MountainLoopReverse']:
 p='Assets/Scenes/'+name+'.unity';assert old(p)==(root/p).read_text(encoding="utf-8"),name
for name in ['RaceDirector','RaceProgress','RacerState','RaceRoad','RoadDriver','RacingMiniMap','ShortcutStrategy','ArcadeVehicle','VehicleRespawn']:
 p='Assets/Scripts/'+name+'.cs';assert old(p)==(root/p).read_text(encoding="utf-8"),name
baseline_data=json.loads((root/'Docs/BackyardShortcuts/baseline.json').read_text(encoding="utf-8"))
route_guid=re.search(r'guid: (\w+)',(root/'Assets/Scripts/RaceRoad.cs.meta').read_text(encoding="utf-8")).group(1)
gate_guid=re.search(r'guid: (\w+)',(root/'Assets/Scripts/RaceGate.cs.meta').read_text(encoding="utf-8")).group(1)
protected=[k for k,v in a.items() if route_guid in v or gate_guid in v]
assert all(a[k]==b[k] for k in protected),'Route/gate metadata modified'
# Explain every old block that changed; root child lists, two copied vegetation meshes and course record identity only.
removedObjects=[re.search(r"  m_Name: (.*)",a[k]).group(1) for k in removed if re.search(r"  m_Name: (.*)",a[k])]
assert all("tree" in n.lower() or "trunk" in n.lower() for n in removedObjects),removedObjects
unexpected=[]
for c in changes:
 before,after=c['before'],c['after']
 if 'm_Roots:' in before:continue
 if 'm_Children:' in before and re.sub(r'  m_Children:.*?(?=  m_Father:)', '',before,flags=re.S)==re.sub(r'  m_Children:.*?(?=  m_Father:)', '',after,flags=re.S):continue
 if re.sub(r'  m_Mesh:.*','',before)==re.sub(r'  m_Mesh:.*','',after):continue
 if before.replace('backyard-forward-v2-terrain','backyard-forward-v3-forest-shortcuts')==after:continue
 unexpected.append(c)
result=dict(baseline=baseline,unchangedRoutePoints=len(baseline_data['route']),unchangedGateCount=len(baseline_data['gates']),unchangedExistingSceneBlocks=sum(k in b and a[k]==b[k] for k in a),removedBlocks=len(removed),removedExistingTrees=len(removedObjects),removedObjectNames=removedObjects,changedBlocks=len(changes),unexpectedChanges=unexpected,protectedOtherCourses=6,protectedSystems=10)
(root/'Docs/BackyardShortcuts/preservation.json').write_text(json.dumps(result,indent=2));print(json.dumps({k:v for k,v in result.items() if k!='unexpectedChanges'}));assert not unexpected, str([(x['id'],x['before'][:120]) for x in unexpected])


