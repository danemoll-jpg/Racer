import pathlib,subprocess,re
root=pathlib.Path(__file__).resolve().parents[1];rows=[]
for name in ['StreetLoopGreybox','StreetLoopReverse','LakeWoods','ForestLoopReverse','MountainLoop','MountainLoopReverse']:
    text=subprocess.check_output(['git','show','7ed9adab:Assets/Scenes/'+name+'.unity'],cwd=root,text=True)
    blocks=list(re.finditer(r'^--- !u!(\d+) &(\d+).*?(?=^--- !u!|\Z)',text,re.M|re.S))
    obj=next(m[2] for m in blocks if m[1]=='1' and '\n  m_Name: Ground_560_320\n' in m[0])
    mesh=next(m[0] for m in blocks if m[1]=='33' and f'm_GameObject: {{fileID: {obj}}}' in m[0])
    rows.append(name+'|'+re.search(r'm_Mesh:.*guid: (\w+)',mesh)[1])
(root/'Docs/YardReset/original-terrain-guids.txt').write_text('\n'.join(rows))
print('\n'.join(rows))
