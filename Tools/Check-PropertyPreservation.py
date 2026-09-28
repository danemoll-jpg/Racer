"""Compare saved Unity object blocks against the clean task checkpoint."""
import pathlib,re,subprocess,json
root=pathlib.Path(__file__).resolve().parents[1]
before='83e1c41c3f46b7fb960a345b6800dc078bdd1a42'
rows=[]
def blocks(s):
    return {m.group(1):m.group(2) for m in re.finditer(r'^--- !u!\d+ &(-?\d+)\n(.*?)(?=^--- !u!|\Z)',s,re.M|re.S)}
for path in sorted((root/'Assets/Scenes').glob('*.unity')):
    rel=path.relative_to(root).as_posix()
    old=blocks(subprocess.check_output(['git','show',f'{before}:{rel}']).decode().replace('\r\n','\n'))
    new=blocks(path.read_text())
    changed=[(k,old[k],new[k]) for k in old.keys()&new.keys() if old[k]!=new[k]]
    deleted=[old[k] for k in old.keys()-new.keys()]
    # All existing scene changes must be traceable to the sign, obsolete paving,
    # wildlife clip reference or Unity's list of root objects.
    allowed_names=('House 3 / Rocky Way Acres entrance','Tall grounded entrance post','Dan beige concrete driveway','Ground_Beige concrete property surface','Ground_Dirt path threshold')
    allowed_ids={k for k,b in old.items() if any('  m_Name: '+n+'\n' in b for n in allowed_names)}
    owner=lambda b:re.search(r'm_GameObject: \{fileID: (-?\d+)\}',b)
    approved=[]
    for k,a,b in changed:
        match=owner(a)
        okay=k in allowed_ids or (match and match[1] in allowed_ids) or a.startswith('SceneRoots:')
        if '  coyoteCalls:' in a:
            strip=lambda s:re.sub(r'  coyoteCalls:\n(?:  - .*\n)*','',s)
            okay=strip(a)==strip(b)
        assert okay, (rel,k,a[:180],b[:180])
        approved.append(k)
    for b in deleted:
        match=owner(b)
        assert 'm_Name: Ground_Dirt path threshold\n' in b or (match and match[1] in allowed_ids),(rel,b[:150])
    rows.append(dict(scene=path.stem,existingChanged=approved,deletedBlocks=len(deleted),newBlocks=len(new.keys()-old.keys()),unrelatedExistingObjectsPreserved=True))
out=root/'Docs/PropertyCorrections/preservation.json';out.write_text(json.dumps(rows,indent=2))
print(json.dumps(rows))
