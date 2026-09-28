"""Confirm the two local changes leave all other saved scene blocks intact."""
import pathlib,re,subprocess,json
root=pathlib.Path(__file__).resolve().parents[1]
checkpoint='eb65014b114f3e86fe5126b3fcb26b2f6b0f3110'
def blocks(s):return {m[1]:m[2] for m in re.finditer(r'^--- !u!\d+ &(-?\d+)\n(.*?)(?=^--- !u!|\Z)',s,re.M|re.S)}
rows=[]
for path in sorted((root/'Assets/Scenes').glob('*.unity')):
    rel=path.relative_to(root).as_posix()
    old=blocks(subprocess.check_output(['git','show',f'{checkpoint}:{rel}']).decode().replace('\r\n','\n'));new=blocks(path.read_text())
    assert old.keys()==new.keys(),rel
    names=('House 3 / Rocky Way Acres entrance','Tall grounded entrance post','Ground_House3 supported valley driveway')
    allowed={k for k,b in old.items() if any('  m_Name: '+n+'\n' in b for n in names)}
    changed=[]
    for key in old:
        if old[key]==new[key]:continue
        a=old[key];owner=re.search(r'm_GameObject: \{fileID: (-?\d+)\}',a)
        assert owner and owner[1] in allowed and (a.startswith('Transform:') or a.startswith('MeshFilter:')),(rel,key,a[:160])
        changed.append(key)
    rows.append(dict(scene=path.stem,changedBlocks=changed,allCollidersAndRoadsUnchanged=True,noAddedOrDeletedObjects=True))
(root/'Docs/McFaddenEntrance/preservation.json').write_text(json.dumps(rows,indent=2));print(json.dumps(rows))
