# Part A before/after counts per scene (stations measured in both sweeps only) with ONE protection rule: the flags of the after sweep (jump zones, flights,
# multi-level, covered, 0.68 berms, junctions - all independent of this round's edits) applied to both sweeps.
# sawtooth: edge position deviates > 0.08 m from the local straight edge line (+-1.5 m). step: terrain adjoins (ground 1 m out no more
# than 2 m below) and the ground 0.3 m outside the edge is more than 0.15 m above/below the pavement edge.
import sys,collections,math
sys.path.insert(0,__import__('os').path.dirname(__file__))
from edge_stats import load,analyse
def key(r):return (r['route'],r['side'],round(r['s']*2))
out=[]
for sc in sys.argv[2:]:
    before=load(f"{sys.argv[1]}/before-{sc}.csv");after=load(f"{sys.argv[1]}/after-{sc}.csv");analyse(before);analyse(after)
    flags={key(r):r['flags'] for r in after}
    common=set(key(r) for r in before)&set(flags)
    res={}
    for tag,rows in (('before',before),('after',after)):
        c=collections.Counter()
        for r in rows:
            if key(r) not in common:continue
            f=flags.get(key(r),r['flags'])
            grp='protected' if f else 'open'
            c[grp]+=1
            if r['saw']:c[grp+' sawtooth']+=1
            if r['step']:c[grp+' step']+=1
        res[tag]=c
    out.append(f"## {sc}\n\n| 0.5 m edge stations | before | after |\n|---|---|---|")
    for k in ('open','open sawtooth','open step','protected','protected sawtooth','protected step'):
        out.append(f"| {k} | {res['before'][k]} | {res['after'][k]} |")
    out.append("")
print("\n".join(out))
