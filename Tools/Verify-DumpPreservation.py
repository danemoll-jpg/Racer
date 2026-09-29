import pathlib,subprocess,re,json
root=pathlib.Path(__file__).resolve().parents[1]
baseline='c27b59c777bb1fa7e01c225b0e1a13cab0039201'
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
    assert name in ['Ground_480_320','Complete authored trees','Woods replacing later subdivisions',"Dan's Backyard Forward - terrain trail"] or name=='Tree trunk' or name.startswith('CR014 trunk '),(key,name)
    changes.append(dict(id=key,type=b[key].splitlines()[0],name=name))
for key in a.keys()-b.keys():
    name=owner(a[key],a)
    assert name.startswith('Old dump ') or name in ['CR014 trunk 02624','CR014 trunk 03256'],(key,name)
for key in b.keys()-a.keys():assert owner(b[key],b) in ['Old dump - scattered salvage - no collision','Discarded rusted drum','Loose drum lid','Broken pallet board','Dented old container','Discarded sheet metal','Old rubber wheel','Bent scrap beam','Discarded short timber']
changed=subprocess.check_output(['git','diff',baseline,'--name-only'],cwd=root,text=True).splitlines()
assert not any(p.startswith('Assets/Scenes/') and p!=path for p in changed)
assert not any(p.startswith('Assets/Scripts/') and 'DumpCorrectionChecks' not in p for p in changed)
result=dict(baseline=baseline,changedExistingSceneBlocks=changes,unchangedSceneBlocks=sum(a[k]==b[k] for k in a.keys()&b.keys()),removedBlocks=len(a.keys()-b.keys()),addedBlocks=len(b.keys()-a.keys()),otherScenesAndGameplaySystemsUnchanged=True)
(root/'Docs/DumpCorrection/preservation.json').write_text(json.dumps(result,indent=2));print(json.dumps(result))
