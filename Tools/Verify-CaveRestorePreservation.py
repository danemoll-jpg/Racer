import subprocess,pathlib,re,json
root=pathlib.Path(__file__).resolve().parents[1];base='0b94bd492ac7a4ff45d50a251b2035428ec41de6';source='5985a27c17e9245c5aa3e44727c1cd5a72bba5dd';scene='Assets/Scenes/LakeWoods.unity'
def blocks(text):return {m[1]:m[0] for m in re.findall(r'(^--- !u!\d+ &(-?\d+)\n.*?)(?=^--- !u!|\Z)',text,re.M|re.S)}
def historical(commit,path):return subprocess.check_output(['git','show',commit+':'+path]).decode().replace('\r\n','\n')
old=blocks(historical(base,scene));new=blocks((root/scene).read_text(encoding='utf-8'));accepted=blocks(historical(source,scene))
names={k:re.search(r'm_Name: (.*)',v)[1] for k,v in old.items() if v.startswith('--- !u!1 ') and 'm_Name: ' in v}
def name(k):
    v=old[k];m=re.search(r'm_GameObject: \{fileID: (-?\d+)\}',v);return names.get(m[1],'') if m else names.get(k,'')
removed=set(old)-set(new);changed=[k for k in old if k in new and old[k]!=new[k]]
for k in removed:assert name(k)=='Angular cave wall ledge',(k,name(k))
allowed=['Angular cave wall ledge','Fallen cave stone','Wall-side fallen slab','Central fractured boulder','Opening-side fallen stone','Broken rear slab']
for k in changed:
    v=old[k];n=name(k)
    # Parent list loses the removed decorative ledges; no unrelated transform fields change.
    if n not in allowed:
        assert v.startswith('--- !u!4 ') and re.sub(r'  m_Children:\n(?:  -.*\n)*','',v)==re.sub(r'  m_Children:\n(?:  -.*\n)*','',new[k]),(k,n)
for rock in json.loads((root/'Docs/CaveRestore/history.json').read_text(encoding='utf-8'))['rocks']:
    tid=rock['transformId']
    for field in ['m_LocalPosition','m_LocalRotation','m_LocalScale']:
        def values(v):return [float(x) for x in re.findall(r'[xyzw]: ([-\d.eE+]+)',re.search(field+r': .*',v)[0])]
        assert all(abs(a-b)<1e-6 for a,b in zip(values(new[tid]),values(accepted[tid]))),rock['name']
protected=['Assets/Scenes/ForestLoopReverse.unity','Assets/Scenes/DansBackyardForward.unity','Assets/Scenes/DansBackyardReverse.unity','Assets/Scripts/RoadDriver.cs','Assets/Scripts/VehicleRespawn.cs','Assets/Scripts/ArcadeVehicle.cs','Assets/Scripts/UndergroundLife.cs','Play-Racer.cmd']
for p in protected:assert historical(base,p)==(root/p).read_text(encoding='utf-8'),p
assert not subprocess.check_output(['git','diff',base,'--','Assets/Track/CaveHillside','Assets/Track/FinalTwo','Assets/Track/SevenCorrections'])
# The same local AI line/guidance and recovery metadata are retained from accepted 0.58.
for k,v in new.items():
    if v.startswith('--- !u!114 ') and k in accepted:assert v==accepted[k],('historical route/component changed',k)
report=dict(baseline=base,obstacleSource=source,originalFourObstacleTransformsRestored=True,removedOnlyWallLedgeComponents=len(removed),changedExisting=len(changed),protectedFiles=protected,allRouteCheckpointAiRecoveryComponentsUnchanged=True,hillsideShellFloorMaterialsMeshesUnchanged=True)
(root/'Docs/CaveRestore/preservation.json').write_text(json.dumps(report,indent=2));print(json.dumps(report))

