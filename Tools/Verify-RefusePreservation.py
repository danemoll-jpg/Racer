import pathlib,subprocess,re,json
root=pathlib.Path(__file__).resolve().parents[1]
baseline='6396cb50de28a157ee1a3b3b34f79d6d7d6873a9'
path='Assets/Scenes/DansBackyardForward.unity'
def blocks(s):return {m[1]:m[2] for m in re.findall(r'(^--- !u!\d+ &(-?\d+)\n)(.*?)(?=^--- !u!|\Z)',s,re.M|re.S)}
a=blocks(subprocess.check_output(['git','show',baseline+':'+path],cwd=root,text=True));b=blocks((root/path).read_text())
def owner(block,all):
    go=re.search(r'm_GameObject: \{fileID: (\d+)\}',block)
    body=all.get(go[1],'') if go else block
    name=re.search(r'm_Name: (.*)',body)
    return name[1] if name else '?'
changes=[]
for key in a.keys()&b.keys():
    if a[key]==b[key]:continue
    name=owner(b[key],b)
    assert name=="Dan's Backyard Forward - terrain trail",(key,name)
    changes.append(dict(id=key,name=name))
old_names=['Old dump - scattered salvage - no collision','Discarded rusted drum','Loose drum lid','Broken pallet board','Dented old container','Discarded sheet metal','Old rubber wheel','Bent scrap beam','Discarded short timber']
for key in a.keys()-b.keys():assert owner(a[key],a) in old_names,(key,owner(a[key],a))
for key in b.keys()-a.keys():assert owner(b[key],b)=='Old dump - dense push-through refuse' or re.fullmatch(r'Refuse-[0-7]',owner(b[key],b)),(key,owner(b[key],b))
changed=subprocess.check_output(['git','diff',baseline,'--name-only'],cwd=root,text=True).splitlines()
assert not any(p.startswith('Assets/Scenes/') and p!=path for p in changed)
assert not any(p.startswith('Assets/Track/') and not p.startswith('Assets/Track/DumpRefuse') for p in changed)
assert not any(p.startswith('Assets/Scripts/') and 'DumpRefuse' not in p for p in changed)
result=dict(baseline=baseline,changedExistingSceneBlocks=changes,unchangedSceneBlocks=sum(a[k]==b[k] for k in a.keys()&b.keys()),removedBlocks=len(a.keys()-b.keys()),addedBlocks=len(b.keys()-a.keys()),terrainTreesRouteGatesLaunchLandingMotorRecoveryOtherScenesUnchanged=True)
(root/'Docs/DumpRefuse/preservation.json').write_text(json.dumps(result,indent=2));print(json.dumps(result))
