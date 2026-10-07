"""0.87: before / after collider census and route data of a scene (Report087Census / Report087Routes), as a short list:
colliders removed, added and changed (by path; duplicate paths compared as multisets), and every route line that differs."""
import sys,collections
b,a,scene,out=sys.argv[1:5]
def load(p):
    rows=collections.Counter();
    for l in list(open(p,encoding='utf-8'))[1:]:rows[l.rstrip('\n')]+=1
    return rows
B=load(f'{b}/{scene}-colliders.tsv');A=load(f'{a}/{scene}-colliders.tsv')
gone=B-A;new=A-B
def path(r):return r.split('\t')[0]
gp=collections.Counter(path(r) for r in gone.elements());np_=collections.Counter(path(r) for r in new.elements())
lines=[f'{scene}: colliders before {sum(B.values())}, after {sum(A.values())}; rows only before {sum(gone.values())}, only after {sum(new.values())}']
changed=set(gp)&set(np_)
lines.append('-- changed (same path, different shape or mesh):')
for p in sorted(changed):
    for r in gone.elements():
        if path(r)==p:lines.append('  before '+r)
    for r in new.elements():
        if path(r)==p:lines.append('  after  '+r)
lines.append('-- removed:')
lines+=['  '+r for r in sorted(gone.elements()) if path(r) not in changed]
lines.append('-- added:')
lines+=['  '+r for r in sorted(new.elements()) if path(r) not in changed]
rb=open(f'{b}/routes-{scene}.txt',encoding='utf-8').read().splitlines();ra=open(f'{a}/routes-{scene}.txt',encoding='utf-8').read().splitlines()
lines.append('-- route data lines only before:');lines+=['  '+l[:220] for l in rb if l not in ra]
lines.append('-- route data lines only after:');lines+=['  '+l[:220] for l in ra if l not in rb]
open(out,'w',encoding='utf-8').write('\n'.join(lines)+'\n');print('\n'.join(l for l in lines if not l.startswith('  ')))
