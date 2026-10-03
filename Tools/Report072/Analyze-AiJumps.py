"""0.72 Part B: per-jump AI outcome table from the airace logs (Report072Checks airace CSV + RECOVERY lines).
An attempt = an AI crossing the flight's lip (along the flight axis) while on that flight's stations. It succeeds when the AI
reaches the end of the landing (+20 m), or is still moving above the world 15 s after the lip, with no recovery; it fails
when a recovery happens first (from where, to where, how far) or it is stopped / below the world 15 s after the lip.
Usage: Analyze-AiJumps.py <checks folder> [<checks folder> ...]"""
import csv,re,sys,pathlib,collections,math
# flight lip / landing-end positions along each flight axis (from the scenes' MountainFlights data)
FLIGHTS={'mountain-forward':[('Eastbound Gully Flight',260,540,(810,-275),(1,0)),('Homeward Summit Flight',290,550,(1259.3573,226.46333),(-0.9619904,-0.27308345))],
         'mountain-reverse':[('Westbound Gully Flight',220,500,(1225,-300),(-1,0)),('South Face Summit Flight',220,480,(990,285),(0,-1))]}
def course(name):return 'mountain-forward' if 'forward' in name else 'mountain-reverse'
rows=[];table=collections.defaultdict(lambda:[0,0,[]]);rec_all=[]
for folder in map(pathlib.Path,sys.argv[1:]):
    results=(folder/'results.txt').read_text().splitlines()
    for f in sorted(folder.glob('airace-*.csv')):
        fl=FLIGHTS[course(f.name)]
        data=collections.defaultdict(list)
        with f.open() as h:
            for r in csv.DictReader(h):data[r['ai']].append(r)
        for ai,samples in data.items():
            state=[None]*len(fl);lastRec=int(samples[0]['recov']);prevAlong=[None]*len(fl)
            for r in samples:
                al=[float(x) for x in r['flightAlong'].split('/')];rec=int(r['recov']);y=float(r['y'])
                for i,(name,lip,end,st,fw) in enumerate(fl):
                    a=al[i];lat=abs((float(r['x'])-st[0])*fw[1]-(float(r['z'])-st[1])*fw[0])
                    if state[i] is None and prevAlong[i] is not None and prevAlong[i]<lip-5<=a<lip+40 and lat<25 and rec==lastRec:state[i]=dict(t=float(r['t']),lap=r['lap'],rec=rec)
                    elif state[i] is not None:
                        if rec!=state[i]['rec']:
                            table[(course(f.name),name)][1]+=1;table[(course(f.name),name)][2].append(f"{ai} lap {state[i]['lap']} at {state[i]['t']:.0f}s");state[i]=None
                        elif a>=end+20 or float(r['t'])-state[i]['t']>15:
                            moving=abs(float(r['speed']))>3 and y>-15
                            if moving:table[(course(f.name),name)][0]+=1
                            else:table[(course(f.name),name)][1]+=1;table[(course(f.name),name)][2].append(f"{ai} lap {state[i]['lap']} stopped / fell 15 s after the lip (speed {r['speed']}, y {y:.0f})")
                            state[i]=None
                    prevAlong[i]=a
                lastRec=rec
    for line in results:
        m=re.match(r'RECOVERY (\w+) lap (\d) from \(([^)]*)\) s (\d+).*?to \(([^)]*)\) s (\d+).*?moved (\d+) m; backward along route (\d+)/(\d+)',line)
        if m:
            back=int(m.group(8));L=int(m.group(9));signed=back if back<L/2 else back-L
            d=re.search(r'along=([-\d.]+)m',line)
            rec_all.append((folder.parent.name,m.group(1),m.group(2),m.group(3),m.group(5),int(m.group(7)),round(float(d.group(1))) if d else -signed))
print('| Course | Flight | Attempts | Landed and continued | Failed (recovered) | Success |')
print('|---|---|---|---|---|---|')
for (c,n),(ok,bad,where) in sorted(table.items()):
    tot=ok+bad;print(f"| {c} | {n} | {tot} | {ok} | {bad}{(' ('+'; '.join(where)+')') if where else ''} | {100*ok/max(tot,1):.0f}% |")
print()
print(f"AI recoveries: {len(rec_all)}")
if rec_all:
    d=[r[5] for r in rec_all];print(f"distance moved: median {sorted(d)[len(d)//2]} m, max {max(d)} m; along the route from the nearest point (+ forward / - back; earned-anchor recoveries: from tracked station): "+', '.join(f"{r[6]:+d}" for r in rec_all))
    for r in rec_all:print(f"  {r[0]} {r[1]} lap {r[2]}: from ({r[3]}) to ({r[4]}), moved {r[5]} m, along route {r[6]:+d} m")
