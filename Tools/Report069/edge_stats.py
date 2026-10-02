# Part A edge statistics from Report069Edges CSVs.
# sawtooth station: the pavement edge position (w) deviates more than 0.08 m from the local straight edge line (+-1.5 m).
# step station: ground just outside the edge (0.3 m) is not flush (> 0.15 m below or above the pavement edge),
#   counted only where terrain adjoins the road (ground within 2 m below the edge at 1 m out; deeper is a drop-off/embankment).
# Protected stations (flags J,F,M,T,B) are counted separately and never "fixed".
import csv,sys,collections,math
def load(p):
    rows=list(csv.DictReader(open(p)))
    for r in rows:
        for k in ('s','x','y','z','w','hw','g03','g10','g20','g30'): r[k]=float(r[k]) if r[k] not in ('NaN','') else math.nan
        r['side']=int(r['side'])
    return rows
def analyse(rows):
    by=collections.defaultdict(list)
    for r in rows: by[(r['route'],r['side'])].append(r)
    out=collections.Counter(); worst=[]
    for k,L in by.items():
        L.sort(key=lambda r:r['s'])
        for i,r in enumerate(L):
            # deviation from the local straight edge line (least squares over +-1.5 m): a widening or curving
            # edge is not a sawtooth, a stepped one is.
            win=[(q['s']-r['s'],q['w']) for q in L[max(0,i-3):i+4] if abs(q['s']-r['s'])<=1.51]
            n=len(win);sx=sum(a for a,_ in win);sy=sum(b for _,b in win);sxx=sum(a*a for a,_ in win);sxy=sum(a*b for a,b in win);den=n*sxx-sx*sx
            fit=(sy*sxx-sx*sxy)/den if abs(den)>1e-9 else sy/n
            r['saw']=n>=5 and abs(r['w']-fit)>0.08
            g=r['g03']; adj=not math.isnan(r['g10']) and r['g10']>-2.0
            r['adj']=adj
            r['step']=adj and (math.isnan(g) or abs(g)>0.15)
            p='prot' if r['flags'] else 'open'
            out[p+'_stations']+=1
            if r['saw']: out[p+'_saw']+=1
            if r['step']: out[p+'_step']+=1
            if r['adj']: out[p+'_adjoin']+=1
            if not r['flags'] and (r['saw'] or r['step']): worst.append(r)
    return out,worst
if __name__=='__main__':
    for p in sys.argv[1:]:
        rows=load(p); out,worst=analyse(rows)
        print(p); [print(f"  {k}: {v}") for k,v in sorted(out.items())]
        rc=collections.Counter(r['route'] for r in worst); print("  open saw/step by route:",dict(rc))
        st=[r['g03'] for r in worst if r['step'] and not math.isnan(r['g03'])]
        if st: st.sort(); print("  step g03 quantiles:",[round(st[int(q*(len(st)-1))],2) for q in (0,.1,.25,.5,.75,.9,1)])
