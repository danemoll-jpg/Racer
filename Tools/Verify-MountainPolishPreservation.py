import pathlib,subprocess,re,json
root=pathlib.Path(__file__).resolve().parents[1];base='b29330bc4ceecf8eda2dba77697583f538db002e'
def old(path):return subprocess.check_output(['git','show',base+':'+path]).decode().replace('\r\n','\n')
def blocks(text):return {m[1]:m[0] for m in re.findall(r'(^--- !u!\d+ &(-?\d+)\n.*?)(?=^--- !u!|\Z)',text,re.M|re.S)}
scene='Assets/Scenes/LakeWoods.unity';before=blocks(old(scene));after=blocks((root/scene).read_text(encoding='utf-8-sig'))
names={k:re.search(r'm_Name: (.*)',v)[1] for k,v in before.items() if v.startswith('--- !u!1 ') and 'm_Name: ' in v}
changed=[]
assert before.keys()==after.keys(),'Forest scene object inventory changed'
for key,v in before.items():
 if v==after[key]:continue
 m=re.search(r'm_GameObject: \{fileID: (-?\d+)\}',v);name=names.get(m[1],'') if m else ''
 assert ('trunk' in name.lower() or 'tree' in name.lower() or 'canopy' in name.lower() or 'batch' in name.lower()),(key,name)
 changed.append(dict(id=key,name=name))
protected=['Assets/Scenes/ForestLoopReverse.unity','Assets/Scenes/DansBackyardForward.unity','Assets/Scenes/DansBackyardReverse.unity','Assets/Scenes/StreetLoopGreybox.unity','Assets/Scenes/StreetLoopReverse.unity','Assets/Scripts/RoadDriver.cs','Assets/Scripts/VehicleRespawn.cs','Assets/Scripts/ArcadeVehicle.cs','Play-Racer.cmd','CODEX_RULES.md']
for p in protected:assert old(p)==(root/p).read_text(encoding='utf-8-sig'),p
assert not subprocess.check_output(['git','diff',base,'--','Assets/Track/CaveHillside','Assets/Track/CaveRestore','Assets/Track/FinalTwo','Assets/Track/SevenCorrections'])
report=dict(baseline=base,forestChangedComponents=changed,caveGeometryAndObstacleUnchanged=True,protectedFiles=protected)
(root/'Docs/MountainPolish/preservation.json').write_text(json.dumps(report,indent=2));print(json.dumps(report))
