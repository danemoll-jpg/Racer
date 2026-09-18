"""Rebuild review tables from retained standalone evidence; no simulated results."""
from pathlib import Path
import csv, re, statistics

D=Path(__file__).resolve().parent
rows=[]
for p in sorted(D.glob('candidate5-*/driving.csv')):
    rows.extend(dict(r,source=p.parent.name) for r in csv.DictReader(p.open()))
replacement=None
for label in ['release-moto-creek','editor-final-moto-creek']:
    p=D/label/'driving.csv'
    if p.exists() and (p.parent/'done.txt').exists():
        replacement=[dict(r,source=label) for r in csv.DictReader(p.open())];break
if replacement:
    rows=[r for r in rows if not(r['profile']=='moto' and r['route']=='Creek Leap')]+replacement
if rows:
    with (D/'comparison-runs.csv').open('w',newline='') as f:
        w=csv.DictWriter(f,fieldnames=list(rows[0]));w.writeheader();w.writerows(rows)
out=['# Measured route comparisons','',
     'Ordinary Unity FixedUpdate motor physics and virtual pedals/steering; no position or force driving after initialization. Two clean attempts (second target +2m/s), one deliberate 1.2s brake plus local recovery, two road controls per route/profile. Both alternatives start 12m before the entrance with the same initial velocity and end at the authored rejoin (branch exit tolerance 3m). Initial velocity is a controlled fixture condition. Traffic/opponents off; isolated saves. Entry speed is measured after braking, not the injected starting value. A failed attempt is retained as a failure, never a usable time.',
     '', '| Route | Vehicle | Clean shortcut seconds | Road seconds | Recovery attempt | Clean entry km/h | Clean exits |',
     '|---|---|---|---|---|---|---|']
for route in ['Creek Leap','Fox Gully','Pine Ridge']:
    for profile in ['original','tourer','moto','atv']:
        group=[r for r in rows if r['route']==route and r['profile']==profile]
        clean=[r for r in group if r['road']=='False' and r['attempt']!='2']
        road=[r for r in group if r['road']=='True']
        failed=[r for r in group if r['road']=='False' and r['attempt']=='2']
        def times(rs):return ' / '.join(r['seconds']+(' DNF' if r['finished']!='True' else '') for r in rs)
        out.append(f"| {route} | {profile} | {times(clean)} | {times(road)} | {times(failed)} | {' / '.join(f'{float(r['entry_mps'])*3.6:.0f}' for r in clean)} | {sum(r['finished']=='True' for r in clean)}/{len(clean)} |")
out+=['','Every row and source folder: [comparison-runs.csv](comparison-runs.csv). Earlier unsuccessful terrain/controller candidates remain in candidate1–candidate3; candidate5 and candidate6 retain the strict landing-envelope failures that motivated the final shoulder fix. The final motorcycle creek rerun supersedes that case only. Initial and failed variants are not claims about the final route.','',
       'Creek: about 120–126km/h into the approach, align with the takeoff, then lift/brake for the hilly road rejoin. Gully: enter around 85–95km/h, accelerate between bends, brake for the crossing/exit crest. Ridge: approach around 125–132km/h after the entry bend; the sign’s 144km/h is a desired straight-ramp target, not a minimum entrance speed. Small vehicles turn more readily but have less forgiving airborne/landing response. Gully’s conservative time saving is modest; the earlier successfully completed car attempts at higher crest speed saved roughly two seconds, with greater risk.','',
       'These are repeatable technical opportunities and failure costs, not a judgment that the routes are fun for a human. No clean legal exit receives a shortcut surcharge. An excursion outside the bounded corridor freezes evidence until the driver returns behind the earned point or recovers locally. Returning to the ordinary road early abandons the branch; unvisited gates then follow ordinary rules.']
(D/'ROUTE-TIMINGS.md').write_text('\n'.join(out),encoding='utf8')

race_rows=[];sector_rows=[]
for folder in sorted(D.glob('standalone-race-*')):
    p=folder/'results.txt'
    if not p.exists():continue
    for line in p.read_text().splitlines()[2:]:
        name,body=line.split(': ',1);parts=dict(x.split('=',1) for x in body.split('; '))
        race_rows.append(dict(run=folder.name,name=name,**parts))
    p=folder/'pace.csv'
    if not p.exists():continue
    last={}
    for r in csv.DictReader(p.open()):
        key=r['name'];state=(r['lap'],r['nextGate'])
        if key in last and state!=last[key][0]:
            old,t=last[key];sector_rows.append(dict(run=folder.name,name=key,lap=old[0],toward_gate=old[1],observed_seconds=f"{float(r['time'])-t:.3f}",arrival_time=r['time'],misses=r['misses'],recoveries=r['recoveries']))
            last[key]=(state,float(r['time']))
        elif key not in last:last[key]=(state,float(r['time']))
for name,data in [('race-summary.csv',race_rows),('race-sectors.csv',sector_rows)]:
    if data:
        with (D/name).open('w',newline='') as f:
            w=csv.DictWriter(f,fieldnames=list(data[0]));w.writeheader();w.writerows(data)
out=['# Mixed-vehicle race measurements','',
     'Full three-lap races: reference Street Classic, Longroof GT, Needle 600, Trail Four; Easy/Normal/Hard; four traffic vehicles. Matched mistake variants brake the reference player for three seconds at CP8, then request local recovery. A separate Hard run removes traffic. Helpers use isolated saves, VSync off and a 60fps cap. Concurrent helper frame figures are not performance benchmarks. The reference player uses the same difficulty policy as opponents; this does not establish human difficulty.',
     '', '| Run | Vehicle | Lap seconds | Race / adjusted seconds | Misses | AI recoveries | Finished |', '|---|---|---|---|---|---|---|']
for r in race_rows:
    out.append(f"| {r['run'].replace('standalone-race-','')} | {r['name']} | {r['lapTimes']} | {r['race']} / {r['adjusted']} | {r['misses']} | {r['recoveries']} | {r['finished']} |")
out+=['','[Race summary](race-summary.csv) includes peak/mean speeds and braking time. [Sector observations](race-sectors.csv) derive next-gate transitions from 0.25s traces; endpoints are approximate to that sampling resolution. Raw pace logs retain throttle, brake, target, ground contact, coordinates and misses. The manually injected player recovery is recorded in each mistake.txt; it is not counted in the AI recovery counter.','',
      'The Hard mistake adds about six seconds to the reference player’s first lap. Final gaps are not monotonic because braking changes subsequent traffic encounters: the Normal mistake run can finish faster than the clean run. Report both; do not interpret that outcome as catch-up assistance. There are no boosted vehicle parameters, teleport catch-up or rubber-banding. All autonomous branch flags remain false: racers use the safe main road until their stunt controller is independently validated. Legal branch rules already apply identically to every RacerState. Human difficulty and subjective racecraft remain Dan’s review.']
(D/'AI-RACES.md').write_text('\n'.join(out),encoding='utf8')
print(f'{len(rows)} route attempts; {len(race_rows)} race results; {len(sector_rows)} sector observations')
