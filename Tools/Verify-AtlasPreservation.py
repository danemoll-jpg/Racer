import re,subprocess,pathlib,json
root=pathlib.Path(__file__).resolve().parents[1]
checkpoint='aa96293f564eb99d100864cb503351be44cd8495'
def blocks(text):return {m[2]:(int(m[1]),m[0]) for m in re.finditer(r'^--- !u!(\d+) &(-?\d+).*?(?=^--- !u!|\Z)',text,re.M|re.S)}
path='Assets/Scenes/StreetLoopReverse.unity'
before=blocks(subprocess.check_output(['git','show',checkpoint+':'+path],cwd=root,text=True))
after=blocks((root/path).read_text())
changes=[]
for key,(kind,old) in before.items():
    new=after.get(key)
    if new!=(kind,old):
        changes.append(dict(id=key,kind=kind,missing=new is None,diff='\n'.join(__import__('difflib').unified_diff(old.splitlines(),new[1].splitlines() if new else [],n=1))))
(root/'Docs/RouteAtlas/laurel-serialized-diff.json').write_text(json.dumps(changes,indent=2))
print('Changed existing Laurel-scene serialized blocks:',len(changes))
for c in changes[:8]:print(c['id'],c['kind'],c['diff'][:1200])
assert all(c['kind']==1660057539 for c in changes),'Existing Laurel scene content changed'
changed=subprocess.check_output(['git','diff','--name-only','--diff-filter=M',checkpoint],cwd=root,text=True).splitlines()
assert not any(p.startswith('Assets/') and p.endswith('.asset') for p in changed),'An existing mesh asset was edited'
runtime=[p for p in changed if p.startswith('Assets/Scripts/') and '/Editor/' not in p]
assert not runtime,'Existing runtime behavior changed: '+str(runtime)
summary=f'PASS: all {len(before)-1} existing StreetLoopReverse serialized blocks unchanged; only the scene root list gains a separate visual guidance root.\nPASS: all pre-existing mesh assets unchanged. Laurel approach, ramp, launch, flight clearance, practical landing/runout terrain, fencing, colliders, rejoin, gate/entitlement, navigation and recovery data therefore retain their checkpoint contents.\nPASS: no existing runtime source changed (physics, AI, recovery included).\n'
(root/'Docs/RouteAtlas/preservation.txt').write_text(summary)
print(summary)
maps={}
for scene in ['StreetLoopGreybox','LakeWoods','ForestLoopReverse','MountainLoop','MountainLoopReverse']:
    text=subprocess.check_output(['git','show',checkpoint+':Assets/Scenes/'+scene+'.unity'],cwd=root,text=True)
    maps[scene]={key:re.search(r'm_Mesh:.*?guid: ([a-f0-9]+)',value).group(1) for key,(kind,value) in blocks(text).items() if kind==33 and re.search(r'm_Mesh:.*?guid: ([a-f0-9]+)',value)}
(root/'Docs/RouteAtlas/baseline-mesh-references.json').write_text(json.dumps(maps))
