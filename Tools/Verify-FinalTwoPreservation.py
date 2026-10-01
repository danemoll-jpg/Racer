import subprocess,pathlib,re,json
root=pathlib.Path(__file__).resolve().parents[1]
base='414d092ce92288927cb550e15def201085d6646f'
scene='Assets/Scenes/LakeWoods.unity'
def blocks(text):return {m[1]:m[0] for m in re.findall(r'(^--- !u!\d+ &(-?\d+)\n.*?)(?=^--- !u!|\Z)',text,re.M|re.S)}
old=blocks(subprocess.check_output(['git','show',base+':'+scene]).decode().replace('\r\n','\n'))
new=blocks((root/scene).read_text(encoding="utf-8"))
changed=[k for k in old if k in new and old[k]!=new[k]]
assert all('points:' in old[k] or 'm_Name: Echo Cave' in old[k] or 'm_Children:' in old[k] for k in changed),changed
assert not set(old)-set(new)
protected=['Assets/Scenes/DansBackyardReverse.unity','Assets/Scenes/ForestLoopReverse.unity','Assets/Scripts/RoadDriver.cs','Assets/Scripts/VehicleRespawn.cs','Assets/Scripts/ArcadeVehicle.cs']
for p in protected:
    if (root/p).exists():assert subprocess.check_output(['git','show',base+':'+p]).decode().replace('\r\n','\n')==(root/p).read_text(encoding="utf-8"),p
report={'baseline':base,'changedExistingSceneBlocks':changed,'addedSceneBlocks':len(set(new)-set(old)),'noExistingObjectsRemoved':True,'protectedFilesUnchanged':protected}
(root/'Docs/FinalTwo/preservation.json').write_text(json.dumps(report,indent=2));print(json.dumps(report))


