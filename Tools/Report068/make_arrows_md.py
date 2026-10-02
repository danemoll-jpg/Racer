# Builds Docs/Report068/ARROWS.md from the applied arrow classification CSVs (Docs/Report068/arrows/).
import csv
names={'StreetLoopGreybox':'Street Loop — Forward','StreetLoopReverse':'Street Loop — Reverse','LakeWoods':'Forest Loop — Forward','ForestLoopReverse':'Forest Loop — Reverse','DansBackyardForward':"Dan's Backyard Loop — Forward",'DansBackyardReverse':"Dan's Backyard Loop — Reverse",'MountainLoop':'Mountain Loop — Forward','MountainLoopReverse':'Mountain Loop — Reverse'}
out=["# 0.68 Part F — redundant ground arrows","",
"Dan, 2026-10-02: the game \"goes a little crazy with the arrows\"; redundant arrows may be removed, and any single removal can be asked back.","",
"**Rules applied** (`Tools/Report068/Report068Arrows.cs`; dry run reviewed first, then applied):","",
"- Removed: arrows on no route of the course (inherited from another course, e.g. the 72 Street Loop Route Atlas arrows in each Backyard scene, or floating over an inactive old runway); arrows pointing backwards for every route they lie on (none found); duplicates of another arrow for the same instruction within 12 m (the Route Atlas arrow is kept over older copies); repeated arrows on stretches with no turn (> 20° within 40 m) and no fork (40 m before to 80 m after), keeping one reassurance arrow per 150 m.",
"- Kept: every arrow before a turn, fork or junction; every gold shortcut arrow on the main road; every Route Atlas fork/rejoin arrow; Dan's 0.67 BUG-020 straight arrow (Backyard Forward, 70.5, 45.1, 61.1).",
"- Kept arrows more than 0.2 m off the surface were reseated flat on it (0.67 BUG-006 rule); see below.","",
"Signs, gates, minimap, route lines and wrong-way guidance are unchanged.","",
"## Counts","","| Course / direction | Before | After | Removed |","|---|---:|---:|---:|"]
detail=[];tb=ta=0
for sc in names:
    rows=list(csv.DictReader(open(f'Docs/Report068/arrows/arrows-{sc}.csv',encoding='utf-8')))
    rem=[r for r in rows if r['action']=='REMOVE'];tb+=len(rows);ta+=len(rows)-len(rem)
    out.append(f"| {names[sc]} | {len(rows)} | {len(rows)-len(rem)} | {len(rem)} |")
    detail+= [f"### {names[sc]} — removed {len(rem)}",""]
    if rem:
        detail+=["| Route | Station | x | y | z | Heading | Reason | Object | Mesh |","|---|---:|---:|---:|---:|---:|---|---|---|"]
        detail+=[f"| {r['route'] or '—'} | {r['station']} | {r['x']} | {r['y']} | {r['z']} | {r['heading']} | {r['reason']} | {r['object']} | {r['mesh']} |" for r in rem]
    detail.append("")
out.append(f"| **All courses** | **{tb}** | **{ta}** | **{tb-ta}** |")
out+=["","## Reseated kept arrows","",
"- Forest Loop Reverse: 'Optional gold / House 3 Detour' (313.7, 46.1, -247.2).",
"- Backyard Forward: 'Main teal trail arrow' (205.9, 73.0, 98.3), corrected 0.51 m.",
"- Backyard Reverse: 'Reverse teal ground arrow' (304.4, 76.7, 102.6), 0.65 m, and (280.3, 76.3, 119.8), 0.52 m.",
"- Mountain Reverse: 'CR122 crossing gold arrow' (698.4, 80.1, -81.5), 0.99 m, and (707.3, 79.2, -70.7), 1.09 m (were floating over Downhill Ridge Cut).",
"- Mountain Reverse (BUG-005): 'Main teal / turn' (747, 85.8, -114.5), reseated after the crease was smoothed.","",
"## Restoring one arrow","","Each removed arrow is listed below with its object path and mesh asset. To bring one back, restore that GameObject from the 0.67 scene (commit `abb1652d`), or ask for it by coordinates. The full per-arrow classification (kept and removed, with reasons) is in `Docs/Report068/arrows/arrows-<scene>.csv`.","","## Removed arrows by course",""]
open('Docs/Report068/ARROWS.md','w',encoding='utf-8').write("\n".join(out+detail)+"\n")
print(tb,ta)
