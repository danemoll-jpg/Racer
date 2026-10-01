import subprocess,pathlib,re,json
root=pathlib.Path(__file__).resolve().parents[1]
base='acb259163904b2ca84e05f75e133b1370dd9537e'
scene='Assets/Scenes/LakeWoods.unity'
def blocks(text):return {m[1]:m[0] for m in re.findall(r'(^--- !u!\d+ &(-?\d+)\n.*?)(?=^--- !u!|\Z)',text,re.M|re.S)}
old=blocks(subprocess.check_output(['git','show',base+':'+scene]).decode().replace('\r\n','\n'))
new=blocks((root/scene).read_text(encoding='utf-8'))
changed=[k for k in old if k in new and old[k]!=new[k]]
assert not set(old)-set(new)
names={k:re.search(r'm_Name: (.*)',v)[1] for k,v in old.items() if v.startswith('--- !u!1 ') and 'm_Name: ' in v}
items=[]
for k in changed:
    v=old[k]; match=re.search(r'm_GameObject: \{fileID: (-?\d+)\}',v);name=names.get(match[1],'') if match else ''
    assert v.startswith(('--- !u!4 ','--- !u!33 ','--- !u!1660057539 ')),(k,name,v[:100])
    if v.startswith('--- !u!4 '):
        assert any(x in name.lower() for x in ['tree','trunk','angular cave wall ledge','fallen slab','fractured boulder','fallen stone','rear slab']), (k,name)
    items.append(dict(id=k,name=name,type=v.splitlines()[0]))
protected=['Assets/Scenes/ForestLoopReverse.unity','Assets/Scenes/DansBackyardReverse.unity','Assets/Scripts/RoadDriver.cs','Assets/Scripts/VehicleRespawn.cs','Assets/Scripts/ArcadeVehicle.cs','Assets/Scripts/UndergroundLife.cs','Play-Racer.cmd']
for p in protected:assert subprocess.check_output(['git','show',base+':'+p]).decode().replace('\r\n','\n')==(root/p).read_text(encoding='utf-8'),p
report=dict(baseline=base,changedExisting=items,noExistingObjectsRemoved=True,allExistingRouteCheckpointGuidanceComponentsUnchanged=True,protectedFilesUnchanged=protected)
(root/'Docs/CaveHillside/preservation.json').write_text(json.dumps(report,indent=2));print('Preserved all route/checkpoint/AI components and protected files; changed blocks:',len(changed))
