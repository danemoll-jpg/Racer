"""Bounded serialized preservation check: no route, woodland or Kyle edits."""
import pathlib,re,subprocess,json
root=pathlib.Path(__file__).resolve().parents[1]
checkpoint='7ed9adab27fb22a765e24dad6d2eb8d66ab32c6c'
def blocks(text):return {m[2]:(int(m[1]),m[0]) for m in re.finditer(r'^--- !u!(\d+) &(-?\d+).*?(?=^--- !u!|\Z)',text,re.M|re.S)}
rows=[]
for name in ['StreetLoopGreybox','StreetLoopReverse','LakeWoods','ForestLoopReverse','MountainLoop','MountainLoopReverse']:
    path=f'Assets/Scenes/{name}.unity'
    before=blocks(subprocess.check_output(['git','show',checkpoint+':'+path],cwd=root,text=True))
    after=blocks((root/path).read_text())
    objects={key:re.search(r'^  m_Name: (.*)$',b,re.M).group(1) for key,(kind,b) in before.items() if kind==1 and re.search(r'^  m_Name: (.*)$',b,re.M)}
    changed=[]
    for key,(kind,old) in before.items():
        assert key in after,(name,'removed established object',key)
        if after[key]==(kind,old):continue
        new=after[key][1]
        owner=re.search(r'm_GameObject: \{fileID: (\d+)\}',old)
        title=objects.get(owner[1],'') if owner else ''
        allowed=(kind==1001 and "Mailbox - Dan - blue X" in old and re.sub(r"(propertyPath: m_LocalPosition\.[xyz]\n      value: )[^\n]+",r"\1POSITION",old)==re.sub(r"(propertyPath: m_LocalPosition\.[xyz]\n      value: )[^\n]+",r"\1POSITION",new)) or kind==1660057539 or (kind in (33,64) and (title.startswith('Ground_') or title.startswith('Grounded '))) or (kind==65 and title.startswith('Grounded ')) or (kind==4 and title in ['Fence endpoint A','Fence endpoint B','Mailbox - Dan - blue X'])
        assert allowed,(name,'unexpected scene change',key,kind,title)
        if kind in (33,64):
            assert re.sub(r'm_Mesh:.*','m_Mesh: REPLACED',old)==re.sub(r'm_Mesh:.*','m_Mesh: REPLACED',new),(name,title,'non-mesh field changed')
            assert 'Kyle' not in title,(name,'Kyle changed')
        changed.append(dict(id=key,type=kind,object=title))
    for bid,(kind,b) in before.items():
        owner=re.search(r'm_GameObject: \{fileID: (\d+)\}',b)
        title=objects.get(owner[1],'') if owner else ''
        if 'trees' in title.lower() or 'trunk' in title.lower():
            assert after[bid]==(kind,b),(name,'forest changed',title)
    rows.append(dict(scene=name,changedExisting=changed,allOtherBlocksPreserved=True))
# No original asset is modified: all local support geometry is cloned.
modified=subprocess.check_output(['git','diff','--name-only','--diff-filter=M',checkpoint],cwd=root,text=True).splitlines()
assert not any(p.startswith('Assets/Track/') and p.endswith('.asset') for p in modified)
for path in root.glob('Assets/Track/Backyard/*Kyle*'):
    rel=path.relative_to(root).as_posix();assert path.read_bytes().replace(b'\r\n',b'\n')==subprocess.check_output(['git','show',checkpoint+':'+rel],cwd=root).replace(b'\r\n',b'\n'),rel
(root/'Docs/YardReset/preservation.json').write_text(json.dumps(rows,indent=2))
print('PASS: six established scenes retain every existing object; only local support meshes, fence endpoints/colliders and mailbox changed. Forest and Kyle assets preserved exactly.')
