"""0.76 Part B audit: what Free Roam content exists in a course scene but not (or not the same) in FreeRoamWorld."""
import sys,os,re,collections
d=sys.argv[1];W='FreeRoamWorld'
def load(s):
    out=collections.defaultdict(dict)
    for l in open(os.path.join(d,f'audit-{s}.txt'),encoding='utf-8'):
        kind,_,rest=l.rstrip('\n').partition(' ')
        if kind in('LANDMARK','ACORN','ACTIVITY'):
            f=[x.strip() for x in rest.split('|')];out[kind][f[0]]=' | '.join(f[1:])
        elif kind=='ROOT':
            f=[x.strip() for x in rest.split('|')];out[kind][f[0]]=' | '.join(f[1:])
        elif kind=='FEATURE':
            f=[x.strip() for x in rest.split('|')];out[kind][f[0]]=f[1]
        elif kind=='COMPONENT':
            m=re.match(r'(\w+) x(\d+)',rest);out[kind][m.group(1)]=m.group(2)
        elif kind in('RACE','BATS'):out[kind]['']=rest
    return out
w=load(W)
scenes=[f[6:-4] for f in os.listdir(d) if f.startswith('audit-') and f[6:-4]!=W]
for s in sorted(scenes):
    a=load(s);print(f'\n######## {s}\n  {a["RACE"][""][:200]}')
    for k in('LANDMARK','ACORN','ACTIVITY','COMPONENT'):
        for i,v in a[k].items():
            if i not in w[k]:print(f'  MISSING {k} {i} | {v}')
            elif w[k][i]!=v:print(f'  DIFFERS {k} {i} | course: {v} | world: {w[k][i]}')
    for i,v in a['ROOT'].items():
        if i not in w['ROOT']:print(f'  MISSING ROOT {i} | {v}')
        else:
            tc=re.search(r'tris=(\d+)',v).group(1);tw=re.search(r'tris=(\d+)',w['ROOT'][i]).group(1)
            if tc!=tw:print(f'  DIFFERS ROOT {i} | course {v} | world {w["ROOT"][i]}')
    for i,v in a['FEATURE'].items():
        if i not in w['FEATURE']:print(f'  MISSING FEATURE {i} | {v}')
        elif w['FEATURE'][i]!=v:print(f'  DIFFERS FEATURE {i} | course {v} | world {w["FEATURE"][i]}')
