import subprocess,json,pathlib,re
root=pathlib.Path(__file__).resolve().parents[1]
baseline='777b62427c2d527df78d6dc1a27db7086f345778'
names=['StreetLoopGreybox','StreetLoopReverse','LakeWoods','ForestLoopReverse','MountainLoop','MountainLoopReverse']
rows=[]
for name in names:
 path=f'Assets/Scenes/{name}.unity'
 before=subprocess.check_output(['git','show',baseline+':'+path],cwd=root).decode().replace('\r\n','\n')
 after=(root/path).read_text()
 a=re.split(r'(?=^--- !u!)',before,flags=re.M);b=re.split(r'(?=^--- !u!)',after,flags=re.M)
 assert len(a)==len(b),(name,'object count changed')
 changed=[(x,y) for x,y in zip(a,b) if x!=y]
 assert len(changed)==2,(name,'unexpected blocks',len(changed))
 for x,y in changed:
  assert re.sub(r'm_Mesh:.*','m_Mesh: REPLACED',x)==re.sub(r'm_Mesh:.*','m_Mesh: REPLACED',y),(name,'non-mesh change')
 guid=re.search(r'guid: (\w+)',(root/f'Assets/Track/Backyard/{name}-Kyle-edge.asset.meta').read_text()).group(1)
 assert all(guid in y for x,y in changed)
 rows.append(dict(scene=name,changedBlocks=2,change='Kyle driveway renderer/collider mesh references only',preserved='All other scene objects and serialized fields'))
(root/'Docs/Backyard/preservation.json').write_text(json.dumps(rows,indent=2))
print('PASS: Six original scenes differ only in the two Kyle driveway mesh references each.')
