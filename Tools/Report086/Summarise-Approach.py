"""0.86 Part D: summarise Granite Saddle drive traces: speed at s 50/70/90/110/150, the lowest speed between s 50 and 130,
the widest the vehicle ran off the centre line (s 40-130), speed at the lip (last wheel contact before the long flight)
and where it came down (water or trail). Usage: python Summarise-Approach.py profile.csv trace.csv [...]"""
import csv,math,sys
prof=[l.split(',') for l in open(sys.argv[1]) if l[:1].isdigit()]
line=[(float(p[0]),float(p[1]),float(p[2])) for p in prof]
def lateral(x,z):
    best=1e9;bs=0
    for s,px,pz in line:
        d=math.hypot(x-px,z-pz)
        if d<best:best=d;bs=s
    return best,bs
for f in sys.argv[2:]:
    rows=list(csv.DictReader(open(f)))
    for r in rows:
        for k in r:r[k]=float(r[k])
    at={}
    for target in (50,70,90,110,150):
        r=min(rows,key=lambda r:abs(r.get('branchS')-target));at[target]=r['speed']
    seg=[r for r in rows if 50<=r.get('branchS')<=130]
    low=min(seg,key=lambda r:r['speed'])
    wide=max((lateral(r['x'],r['z'])+(r,) for r in rows if 40<=r.get('branchS')<=130),key=lambda t:t[0])
    # flight: longest run of wheels==0 after branchS 180
    best=(0,None,None);run=None
    for i,r in enumerate(rows):
        if r.get('branchS')<180:continue
        if r['wheels']==0:
            if run is None:run=i
        else:
            if run is not None and i-run>best[0]:best=(i-run,run,i)
            run=None
    lip=rows[best[1]-1] if best[1] else None;land=rows[best[2]] if best[2] else None
    name=f.replace(chr(92),'/').split('/')[-1]
    print(f"{name}: speed s50 {at[50]:.1f}, s70 {at[70]:.1f}, s90 {at[90]:.1f}, s110 {at[110]:.1f}, s150 {at[150]:.1f} m/s; lowest s50-130 {low['speed']:.1f} at s {low['branchS']:.0f} ({low['x']:.0f},{low['z']:.0f}); widest {wide[0]:.1f} m off the centre line at s {wide[1]:.0f}; "
          +(f"lip {lip['speed']:.1f} m/s at ({lip['x']:.0f},{lip['y']:.1f},{lip['z']:.0f}), flight {best[0]*0.02:.2f}s, down at ({land['x']:.0f},{land['y']:.1f},{land['z']:.0f})" if lip else "no flight"))
