"""0.76 Part A + Part B item 5: restore every course scene to its 0.73 state (a877a39d) around the House 3 driveway
and remove 0.74's Free-Roam-only storm-drain copies, while keeping the other 0.74 fixes (BUG-001 tree, BUG-002 sign,
Mountain Forward BUG-004/005 and the Part C cave).

Works document by document on the scene YAML: every document reverts to its 0.73 version (added documents are dropped,
removed ones come back) unless it belongs to a kept fix. Usage: restore_scenes.py <0.73 scene dir> [--write]"""
import sys,re,os,collections
sys.path.insert(0,os.path.dirname(__file__));import scene_yaml as y
SCENES=['StreetLoopGreybox','StreetLoopReverse','LakeWoods','ForestLoopReverse','MountainLoop','MountainLoopReverse','DansBackyardForward','DansBackyardReverse']
ROOTS_ID='9223372036854775807'
def pos(body):
    m=re.search(r'm_LocalPosition: \{x: ([-\d.e]+), y: ([-\d.e]+), z: ([-\d.e]+)\}',body);return tuple(map(float,m.groups())) if m else None
def keep(scene,s,f,old):
    """True when the current (0.75) version of document f is kept. s: current scene, old: 0.73 scene."""
    src=s if f in s.by else old;p=src.path_doc(f);cls=src.by[f][1]
    if scene.startswith('MountainLoop'):
        if p.startswith('CR056 Forest Loop') or p.startswith('Echo eroded hillside') or p.startswith('Ground_Echo wooded hillside'):return True  # 0.74 Part C cave
        if p.startswith('Ground_Report071 patch BUG-00'):return True                      # BUG-004/005
    if p.startswith('CR-015 modest roadside details'):return True                       # BUG-002 sign removal (parent child list)
    if (cls==1001 or 'stripped' in src.by[f][2]) and f in old.by and f not in s.by:                                 # BUG-002: the removed sign prefab instance
        b=old.by[f][3]
        if cls==4:                                                                       # its stripped transform -> the instance
            m=re.search(r'm_PrefabInstance: \{fileID: (-?\d+)',b);b=old.by[m.group(1)][3] if m and m.group(1) in old.by else ''
        tp=re.search(r'm_TransformParent: \{fileID: (-?\d+)',b)
        return bool(tp) and old.path_doc(tp.group(1)).startswith('CR-015 modest roadside details')
    if p.endswith('Roadside woodland trunk') and cls==4 and f in s.by and f in old.by:
        a=pos(old.by[f][3]);b=pos(s.by[f][3])
        if scene=='StreetLoopGreybox' and abs(a[0]-440.01)<.6 and abs(a[2]-7.24)<.6:return True   # BUG-001 tree
        if scene=='MountainLoop' and abs(a[0]-735.9)<1.5 and abs(a[2]+133.7)<1.5:return True      # BUG-004 trunk put back up
        return False
    return False
REVERT_PREFIX=('Storm drain tunnel (Free Roam)','CR097 supported properties and camp/House 3 valley driveway','CR091-096 exploration world/House 3 ',
 'Memory loop - north is +Z/Ground_640_240','Memory loop - north is +Z/Ground_560_240','Woods replacing later subdivisions/Tree trunk',
 'World cleanup additional woodland/','Phase 3 - Race Systems')
def run(scene,olddir,write):
    old=y.load(os.path.join(olddir,scene+'.073.unity'));cur_path=f'Assets/Scenes/{scene}.unity';s=y.load(cur_path)
    ids=set(old.by)|set(s.by);out={};log=collections.Counter();unknown=collections.Counter()
    for f in ids:
        if f==ROOTS_ID:continue
        inold=f in old.by;incur=f in s.by
        if inold and incur and old.by[f][3]==s.by[f][3]:out[f]=s.by[f];continue
        k=keep(scene,s,f,old);p=(s if incur else old).path_doc(f)
        tag=('KEEP ' if k else 'REVERT ')+('changed' if inold and incur else 'added' if incur else 'removed')+' '+'/'.join(p.split('/')[:2])
        log[tag]+=1
        if not k and not p.startswith(REVERT_PREFIX) and not (p.startswith('<') and not incur):unknown[tag]+=1
        if k:
            if incur:out[f]=s.by[f]
        else:
            if inold:out[f]=old.by[f]
    # SceneRoots: current list, minus transforms that no longer exist, plus 0.73 roots that are back.
    rf,rc,rh,rb=s.by[ROOTS_ID];lines=rb.split('\n');head=[];roots=[];tail=[];mode=0
    for l in lines:
        m=re.match(r'  - \{fileID: (-?\d+)\}',l)
        if m and mode<2:mode=1;roots.append(m.group(1));continue
        (head if mode==0 else tail).append(l);mode=2 if mode==1 else mode
    final=[r for r in roots if r in out]
    for l in old.by[ROOTS_ID][3].split('\n'):
        m=re.match(r'  - \{fileID: (-?\d+)\}',l)
        if m and m.group(1) in out and m.group(1) not in final:final.append(m.group(1))
    body='\n'.join(head+[f'  - {{fileID: {r}}}' for r in final]+tail)
    out[ROOTS_ID]=(rf,rc,rh,body)
    text=s.pre+''.join(d[2]+d[3] for d in sorted(out.values(),key=lambda d:int(d[0])))
    print(f'== {scene}: roots {len(roots)} -> {len(final)}')
    for k,n in sorted(log.items()):print(f'{n:7d} {k}')
    if unknown:print('UNCLASSIFIED REVERTS:',dict(unknown))
    if write:
        with open(cur_path,'w',encoding='utf-8',newline='') as fh:fh.write(text)
if __name__=='__main__':
    for sc in SCENES:run(sc,sys.argv[1],'--write' in sys.argv)
