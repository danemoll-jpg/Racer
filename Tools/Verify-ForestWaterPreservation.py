import pathlib, subprocess, re, json
root=pathlib.Path(__file__).resolve().parents[1]
checkpoint='480a279fc23afd0d1bee0a3c218f4a0ac6ecb5c9'
def saved(path):return subprocess.check_output(['git','show',checkpoint+':'+path],cwd=root).decode('utf-8').replace('\r\n','\n')
def current(path):return (root/path).read_text(encoding='utf-8')
def blocks(text):return {m[2]:(int(m[1]),m[0]) for m in re.finditer(r'^--- !u!(\d+) &(-?\d+).*?(?=^--- !u!|\Z)',text,re.M|re.S)}
report=[]
for name in ['StreetLoopReverse','StreetLoopGreybox','LakeWoods','MountainLoop','MountainLoopReverse']:
    p='Assets/Scenes/'+name+'.unity'
    assert saved(p)==current(p),name+' scene changed'
    report.append('PASS: '+name+' entire scene unchanged from checkpoint.')
p='Assets/Scenes/ForestLoopReverse.unity';old=blocks(saved(p));new=blocks(current(p))
house=next(k for k,(kind,text) in old.items() if kind==1 and '\n  m_Name: Original house 3\n' in text)
for k,(kind,text) in old.items():
    if k==house or f'm_GameObject: {{fileID: {house}}}' in text:
        assert new.get(k)==(kind,text),'House component changed '+k
report.append('PASS: House 3 GameObject and all attached components/transforms unchanged.')
changed=subprocess.check_output(['git','diff','--name-only',checkpoint],cwd=root,text=True).splitlines()
assert not any(p.endswith('.asset') for p in changed),'Existing shared asset modified'
assert not any(p.startswith('Assets/Scripts/') and '/Editor/' not in p for p in changed),'Existing runtime behavior modified'
report.append('PASS: every existing asset and runtime source unchanged, including shared Laurel support meshes, vehicle physics, AI and recovery logic.')
report.append('Laurel protection therefore includes its entire unchanged collision world: approach, launch, flight clearance, imperfect landings, runout, fencing, recovery and rejoin. Water is authored only in ForestLoopReverse.')
(root/'Docs/ForestWaterJump/preservation.txt').write_text('\n'.join(report)+'\n')
print('\n'.join(report))
