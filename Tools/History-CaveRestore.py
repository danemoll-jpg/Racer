import subprocess,re,json,pathlib
root=pathlib.Path(__file__).resolve().parents[1]
source='5985a27c17e9245c5aa3e44727c1cd5a72bba5dd'
scene='Assets/Scenes/LakeWoods.unity'
def blocks(text):return dict((m[1],m[0]) for m in re.findall(r'(^--- !u!\d+ &(-?\d+)\n.*?)(?=^--- !u!|\Z)',text,re.M|re.S))
old=blocks(subprocess.check_output(['git','show',source+':'+scene]).decode().replace('\r\n','\n'))
current=blocks((root/scene).read_text())
names=['Wall-side fallen slab','Central fractured boulder','Opening-side fallen stone','Broken rear slab']
rocks=[]
for name in names:
    gid=next(k for k,v in old.items() if v.startswith('--- !u!1 ') and f'  m_Name: {name}\n' in v)
    tid=next(k for k,v in old.items() if v.startswith('--- !u!4 ') and f'  m_GameObject: {{fileID: {gid}}}' in v)
    def vec(key):return {k:float(v) for k,v in re.findall(r'([xyzw]): ([-\d.eE+]+)',re.search(key+r': \{([^}]+)',old[tid])[1])}
    components=[k for k,v in old.items() if f'  m_GameObject: {{fileID: {gid}}}' in v and not v.startswith('--- !u!4 ')]
    assert all(old[k]==current[k] for k in components),'Non-transform obstacle change'
    rocks.append(dict(name=name,position=vec('m_LocalPosition'),rotation=vec('m_LocalRotation'),scale=vec('m_LocalScale'),transformId=tid))
assert not subprocess.check_output(['git','diff',source,'HEAD','--','Assets/Track/FinalTwo'])
report=dict(source=source,version='0.58.0-review1 / game-58000',rocks=rocks,originalMeshAssetsAndColliderComponentsUnchanged=True)
(root/'Docs/CaveRestore/history.json').write_text(json.dumps(report,indent=2))
print(json.dumps(report,indent=2))
