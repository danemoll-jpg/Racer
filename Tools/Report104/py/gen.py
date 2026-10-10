# 0.104 Part B: plans the Hwy 92 roadside "like today" and writes Assets/Resources/Hwy92/Hwy92Roadside.json, which the game builds at load
# in every scene (Racer.Hwy92Roadside). Everything is planned here in game x/z; heights come from the ground at load.
# Sources (placement only, never shown): OpenStreetMap buildings / ways / signals / shops (SourceArt/Reference/Hwy92/osm-*.json) and the
# USGS NAIP aerial image (naip-hwy92.jpg) for the paved lots. Real places become invented, generic businesses.
import json,math,random,pickle,sys,base64,zlib
import numpy as np
from shapely.geometry import Polygon,MultiPolygon,LineString,Point,box,MultiLineString
from shapely.ops import unary_union,nearest_points
import shapely
from hwymap import *
import lots as L
from plot import POLY as PLOT_REAL,game_outline
OUT=ROOT+'/Assets/Resources/Hwy92/Hwy92Roadside.json'
rng=random.Random(104)
XMIN,XMAX,ZMAX=-798.0,798.0,848.0           # the game's ground along the stretch (tiles x -800..800, z up to 850)
WORLD=box(XMIN,380,XMAX,ZMAX)
log=[]
def say(s):log.append(s);print(s)

# ------------------------------------------------------------------ game constraints
HWY=LineString(GCL)
def gd(p):return project(GCL,GS,np.asarray(p,float))   # (s, d) on the game centreline
ROAD_EDGE=8.2
HWY_PAVED=HWY.buffer(ROAD_EDGE+.4,cap_style=2)          # nothing paved by us inside the highway itself
courses=json.load(open(ROOT+'/Assets/Resources/WorldMaps/CoursePreviews.json'))['courses']
routes=[];shortcut=None
for c in courses:
    lines=[('main',c['main'])]+[('branch%d'%i,b['points']) for i,b in enumerate(c.get('branches',[]))]
    for nm,pts in lines:
        xy=[(p['x'],p['z']) for p in pts]
        if len(xy)>1:routes.append((c['scene'],nm,LineString(xy)))
        if c['scene']=='StreetLoopReverse' and nm=='branch1':shortcut=LineString(xy)
# the parts of the race lines that leave Hwy 92 (on the highway they keep to its lanes): Trickum Rd, S Cherokee Ln, the shortcut
off=[]
for sc,nm,ln in routes:
    cur=[]
    for x,z in ln.coords:
        if WORLD.buffer(60).contains(Point(x,z)) and abs(gd((x,z))[1])>9.5:cur.append((x,z))
        else:
            if len(cur)>1:off.append(LineString(cur))
            cur=[]
    if len(cur)>1:off.append(LineString(cur))
OFF=unary_union(off)
SIDE=unary_union([l for l in off if l.distance(Point(-606,541))<40 or l.distance(Point(322,557))<40])   # Trickum Rd and S Cherokee Ln
SIDE_ROAD=SIDE.buffer(8.5)                        # the side roads' asphalt and verge (the route keeps to one lane)
SOLID_CLEAR=OFF.buffer(10)                       # no solid within 10 m of a race line off the highway
# and none near where Granite Creek Cut leaves Hwy 92 (a vehicle running wide at the turn-off)
SOLID_CLEAR=SOLID_CLEAR.union(Point(-76.4,529.1).buffer(30))
SHORT=shortcut
say(f'shortcut Granite Creek Cut (Street Loop Reverse branch 1): {len(shortcut.coords)} points from {np.round(shortcut.coords[0],1).tolist()} to {np.round(shortcut.coords[-1],1).tolist()}')
prot=[]
for sn in ['StreetLoopReverse','StreetLoopGreybox','FreeRoamWorld','MountainLoop','LakeWoods','ForestLoopReverse','DansBackyardForward']:
    for l in open(f'C:/Users/danmo/AppData/Local/Temp/report104/protect/protect-{sn}.txt'):
        if l.startswith('#'):continue
        x,z,r,kind,path=l.rstrip('\n').split('\t')
        if 'Forest detail batch' in path or 'World edge' in path or 'gate marking' in path or 'Route atlas' in path or 'Breakable sign' in path:continue
        prot.append(Point(float(x),float(z)).buffer(max(float(r),1)+4))
# the 0.79 street-name sign posts stand on the junction corners: keep the corners free
for c in [(-606,541),(322,557)]:prot.append(Point(c).buffer(26))
PROT=unary_union(prot)
plot_game=Polygon(game_outline()).intersection(WORLD)
say(f'plot (Weatherstone subdivision across from S Cherokee Ln) in game: {[[round(x,1),round(z,1)] for x,z in plot_game.exterior.coords][:-1]}, area {plot_game.area:.0f} m2')

def r2g_poly(p,step=3.0):
    p=Polygon(p) if not isinstance(p,Polygon) else p
    ext=p.exterior;n=max(4,int(ext.length/step))
    pts=[ext.interpolate(i/n,normalized=True) for i in range(n)]
    g=Polygon([tuple(r2g((q.x,q.y))) for q in pts]).buffer(0)
    holes=[Polygon([tuple(r2g(q)) for q in h.coords]) for h in p.interiors]
    for h in holes:g=g.difference(h)
    return g
def r2g_rect(p):
    """a building footprint: its corners mapped (buildings stay straight-edged)"""
    q=Polygon([tuple(r2g(c)) for c in list(p.exterior.coords)[:-1]]).buffer(0)
    return q
def pieces(g):
    if g.is_empty:return []
    return [x for x in (g.geoms if hasattr(g,'geoms') else [g]) if isinstance(x,Polygon)]
def coords(p,nd=2):return [round(v,nd) for x,z in list(p.exterior.coords)[:-1] for v in (x,z)]

# ------------------------------------------------------------------ the game's ground (2 m grid from the scene, z 380-760): relief and slope
import cv2 as _cv
_G=np.load('C:/Users/danmo/AppData/Local/Temp/report104/grid/grid-StreetLoopReverse.npz')['g']
def ground_h(x,z):
    j=int(round((x+1260)/2));i=int(round((z-380)/2))
    return _G[i,j] if 0<=i<_G.shape[0] and 0<=j<_G.shape[1] else np.nan
def relief(poly):
    mnx,mnz,mxx,mxz=poly.bounds;hs=[ground_h(x,zz) for x in np.arange(mnx,mxx+1,2) for zz in np.arange(mnz,mxz+1,2) if poly.buffer(1).contains(Point(x,zz))]
    hs=[h for h in hs if not np.isnan(h)];return (max(hs)-min(hs)) if hs else 0.0
def steep_area(limit=.12):
    gy,gx=np.gradient(np.nan_to_num(_G,nan=0.0),2.0);sl=np.hypot(gx,gy);m=np.ascontiguousarray(((sl>limit)&~np.isnan(_G)).astype(np.uint8)*255)
    m=_cv.morphologyEx(m,_cv.MORPH_OPEN,np.ones((3,3),np.uint8))
    cs,_=_cv.findContours(m,_cv.RETR_EXTERNAL,_cv.CHAIN_APPROX_SIMPLE);out=[]
    for c in cs:
        if len(c)<3:continue
        p=Polygon([(-1260+2*x,380+2*y) for [[x,y]] in c]).buffer(0)
        if p.area>40:out.append(p)
    return unary_union(out)
STEEP=steep_area()
RELIEF_LIMIT=4.0
# ------------------------------------------------------------------ buildings
NAMES={
 'grocery':['Hometown Grocery','Valley Fresh Market'],
 'gas':['Quick Fill','Pine Gas & Go','Fuel Stop'],
 'fastfood':['Burger Barn','Cluck Shack','Taco Trail','Donut Den','Biscuit Barn','Pizza Corner','Sandwich Stop','Wing Shack','Shake Shop'],
 'bank':['Ridgeline Bank','Pinecrest Savings','County Trust Bank'],
 'tenant':['Nails & Spa','Cleaners','Hair Studio','Phone Repair','Family Dental','Insurance','Thai Kitchen','Mattress Outlet','Pet Supply','Tax Service','Cafe','Pizza','Vitamins','Shipping','Barber','Optical','Sushi','Mexican Grill','Laundry','Donuts','Yoga','Florist'],
 'office':['Professional Center','Medical Offices','Office Park','Law Offices','Realty'],
 'storage':['Store-All Storage'],'auto':['Tire & Brake','Auto Service'],'church':['Community Church'],'civic':['Municipal Center'],
 'senior':['Oak Terrace'],'fitness':['Fitness Center'],'vet':['Animal Clinic'],'pharmacy':['Pharmacy'],'liquor':['Beverage Shop'],'venue':['Event Hall'],
 'apartments':[],'house':[],'convenience':['Quick Fill Market','Gas & Go Market']}
used={k:0 for k in NAMES}
def name_for(k):
    lst=NAMES.get(k) or [];
    if not lst:return ''
    n=lst[used[k]%len(lst)];used[k]+=1;return n
def classify(b):
    t=b['tags'];pk=b['pois'];a=b['area']
    if t.get('amenity')=='fuel':return 'canopy_fuel'
    if t.get('building')=='roof':return 'canopy' if a>=60 else None
    if t.get('building')=='shed' or a<40:return None
    if t.get('shop')=='supermarket' or (a>3500 and t.get('building')=='retail'):return 'grocery'
    if t.get('shop')=='convenience' and 'fuel' not in t.get('amenity',''):return 'pharmacy' if t.get('amenity')=='pharmacy' else 'convenience'
    if t.get('amenity')=='pharmacy':return 'pharmacy'
    if t.get('amenity') in('fast_food','cafe','restaurant') and a<700:return 'fastfood'
    if t.get('amenity')=='bank' or 'bank' in pk:return 'bank' if a<800 else 'office'
    if t.get('shop')=='storage_rental':return 'storage'
    if t.get('shop') in('tyres','car_repair') or 'car_repair' in pk:return 'auto'
    if t.get('amenity')=='place_of_worship':return 'church'
    if t.get('amenity')=='townhall' or t.get('building')=='public':return 'civic'
    if t.get('leisure')=='fitness_centre' or 'fitness_centre' in pk:return 'fitness'
    if t.get('amenity')=='veterinary':return 'vet'
    if t.get('shop')=='alcohol':return 'liquor'
    if t.get('amenity')=='events_venue':return 'venue'
    if t.get('building')=='apartments':return 'apartments'
    if 'Gardens' in t.get('name','') or a>4500:return 'senior'
    if len(pk)>=2 or (t.get('building') in('retail','commercial') and a>=700):return 'strip'
    if pk and any(k in('fast_food','cafe','restaurant') for k in pk) and a<700:return 'fastfood'
    if t.get('office') or 'insurance' in pk or 'dentist' in pk:return 'office'
    if a<330 and abs(b['d'])>75:return 'house'
    if a>=1500:return 'office'
    return 'retail' if abs(b['d'])<110 else 'office'
HEIGHT={'grocery':9.5,'strip':6.8,'retail':6.2,'fastfood':5.4,'bank':6.2,'office':7.6,'storage':12.5,'auto':6.0,'church':8.0,'civic':8.5,
        'fitness':8.0,'vet':5.6,'pharmacy':6.4,'liquor':6.0,'venue':6.2,'apartments':9.5,'senior':10.5,'house':5.6,'convenience':5.0}
real=L.roadside()
plot_r=PLOT_REAL
buildings=[];canopies=[];skipped=[]
for b in real:
    k=classify(b)
    if not k:continue
    rp=Polygon(b['poly']).buffer(0)
    if plot_r.buffer(2).contains(rp.centroid):skipped.append(('inside the stunt plot',b['id']));continue
    g=r2g_rect(rp)
    if g.is_empty or not WORLD.contains(g.centroid):continue
    g=g.intersection(WORLD.buffer(-2))
    cut=HWY.buffer(ROAD_EDGE+7).union(SIDE_ROAD.buffer(5.5)).union(PROT).union(plot_game.buffer(3))
    g2=g.difference(cut)
    if g2.area<.6*g.area or g2.area<25:skipped.append((f'{k} on a road / protected spot / the plot',b['id']));continue
    g=max(pieces(g2),key=lambda p:p.area)
    rl=relief(g)
    if rl>RELIEF_LIMIT:skipped.append((f'{k}: the ground in the game rises {rl:.1f} m across it (a hill the real lot does not have)',b['id']));continue
    if k.startswith('canopy'):
        if g.distance(OFF)<10 or g.intersects(HWY.buffer(ROAD_EDGE+6)):skipped.append(('canopy by a race line',b['id']));continue
        canopies.append(dict(kind=k,poly=g,src=b));continue
    buildings.append(dict(kind=k,poly=g,src=b,h=HEIGHT[k]*(1.0 if k!='office' or b['area']<1200 else 1.25)))
say(f'buildings planned: {len(buildings)} ({", ".join(f"{k} {sum(1 for x in buildings if x["kind"]==k)}" for k in sorted(set(x["kind"] for x in buildings)))}); canopies {len(canopies)}; skipped {len(skipped)}')

# the shortcut: a building on its line becomes a drive-through (a passage 13 m wide cut through it, roofed over)
SC_CORR=SHORT.buffer(6.5)
passages=[]
for b in buildings:
    if b['poly'].intersects(SC_CORR):
        cutp=b['poly'].intersection(SHORT.buffer(6.5,cap_style=2))
        rest=pieces(b['poly'].difference(SHORT.buffer(6.5)))
        say(f"shortcut passes through {b['kind']} (OSM way {b['src']['id']}) at {np.round(b['poly'].centroid.coords[0],1).tolist()}: drive-through passage {cutp.area:.0f} m2")
        b['parts']=[r for r in rest if r.area>12];b['passage']=cutp
        passages.append(b)
# solids also stay 10 m from every other race line off the highway
for b in buildings:
    parts=b.get('parts',[b['poly']])
    keep=[]
    for p in parts:
        lines=OFF.difference(SC_CORR.buffer(1)) if b in passages else OFF
        q=p.difference(lines.buffer(10)) if p.distance(lines)<10 else p
        keep+= [r for r in pieces(q) if r.area>12]
    b['parts']=keep
buildings=[b for b in buildings if b['parts']]

# ------------------------------------------------------------------ pavement
P_real=pickle.load(open('pave.pkl','rb'))
band_r=[]
for p in pieces(P_real):
    c=p.centroid;s,d=project(RCL,RS,np.array([c.x,c.y]))
    if abs(d)>250:continue
    band_r.append(p)
paved=unary_union([r2g_poly(p) for p in band_r])
# entrances where the real ones are: OSM driveways / service roads that reach Hwy 92, carried to the game's road edge
drives=[];drive_mouths=[]
for w in WAYS:
    t=w.get('tags',{})
    if t.get('highway')!='service' or t.get('service') in('parking_aisle','drive-through','alley'):continue
    p=wxy(w)
    for end,nxt in((p[0],p[1:]),(p[-1],p[-2::-1])):
        s,d=project(RCL,RS,end)
        if abs(d)<real_half_at(end[0])+9:
            g=[r2g(q) for q in [end]+list(nxt)]
            gl=LineString(g)
            gl=gl.intersection(Point(g[0]).buffer(60))
            if gl.is_empty or gl.geom_type!='LineString':continue
            gs_,gdv=gd(g[0]);pe,tan,nrm=at(GCL,GS,gs_)
            mouth=pe+nrm*math.copysign(ROAD_EDGE+.2,gdv)
            line=LineString([tuple(mouth)]+list(gl.coords))
            if not WORLD.contains(Point(mouth)) or SIDE_ROAD.contains(Point(mouth)) or plot_game.buffer(2).contains(Point(mouth)):continue
            drives.append(line.buffer(3.6).union(Point(mouth).buffer(6.5)));drive_mouths.append(mouth)
paved=unary_union([paved]+drives)
# public side streets of today that the game does not have (their first 120 m): Trickum Rd north of Hwy 92, subdivision entrances
stubs=[];stub_lines=[]
for w in WAYS:
    t=w.get('tags',{});hw=t.get('highway')
    if hw not in('residential','tertiary','unclassified','secondary'):continue   # (primary_link: Hwy 92's own turn lanes)
    nm=t.get('name','')
    p=wxy(w)
    for end,nxt in((p[0],p[1:]),(p[-1],p[-2::-1])):
        s,d=project(RCL,RS,end)
        if abs(d)>real_half_at(end[0])+6 or len(nxt)<1:continue
        g=[r2g(q) for q in [end]+list(nxt)];gl=LineString(g)
        if gl.length<8 or gl.intersects(plot_game.buffer(-.5)) or nm.startswith(('Weatherstone','South Cherokee')):continue   # the plot keeps no streets
        gl=shapely.ops.substring(gl,0,min(gl.length,95))
        gs_,gdv=gd(g[0]);pe,tan,nrm=at(GCL,GS,gs_);mouth=pe+nrm*math.copysign(ROAD_EDGE,gdv)
        line=LineString([tuple(mouth)]+list(gl.coords)[1:])
        if SIDE_ROAD.buffer(4).intersects(Point(mouth)) or not WORLD.contains(Point(mouth)):continue
        if plot_game.buffer(-1).intersects(line):continue
        half=5.2 if hw in('tertiary','secondary') else 3.8
        stubs.append(line.buffer(half,cap_style=2).union(Point(mouth).buffer(half+3)));stub_lines.append((line,half,nm))
        say(f'side street of today: {nm or hw} at x {mouth[0]:.0f} ({"north" if gdv>0 else "south"} side), {line.length:.0f} m drawn')
STREETS=unary_union(stubs) if stubs else Polygon()
bfoot=unary_union([p for b in buildings for p in b['parts']])
paved=paved.difference(STEEP.difference(unary_union(drives).buffer(.5) if drives else Polygon())).difference(HWY_PAVED).difference(SIDE_ROAD.buffer(-1.5)).difference(plot_game).difference(bfoot.buffer(.25)).intersection(WORLD.buffer(-1))
paved=unary_union([p for p in pieces(paved) if p.area>=50]).simplify(.4)
STREETS=STREETS.difference(HWY_PAVED).difference(SIDE_ROAD.buffer(-1.5)).intersection(WORLD.buffer(-1))
say(f'paved lots and drives: {paved.area:.0f} m2 in {len(pieces(paved))} pieces; {len(drives)} entrances from Hwy 92; side streets {STREETS.area:.0f} m2')

# ------------------------------------------------------------------ fronts: which walls face the road or the lot
def fronts(poly):
    cs=list(poly.exterior.coords)[:-1];n=len(cs);out=[]
    ccw=Polygon(cs).exterior.is_ccw
    for i in range(n):
        a=np.array(cs[i]);b=np.array(cs[(i+1)%n]);e=b-a;L_=np.linalg.norm(e)
        if L_<3:out.append(0);continue
        nrm=np.array([e[1],-e[0]])/L_ if ccw else np.array([-e[1],e[0]])/L_   # outward
        mid=(a+b)/2;s,d=gd(mid);_,_,hn=at(GCL,GS,s);toroad=-hn*np.sign(d)
        facing_road=np.dot(nrm,toroad)
        probe=Point(mid+nrm*6)
        lot=paved.contains(probe) or STREETS.contains(probe)
        out.append(2 if facing_road>.6 and L_>5 else 1 if lot and L_>4 else 0)
    if 2 not in out and 1 not in out:
        # no wall faces the road or a lot: the longest one facing the road most
        best=max(range(n),key=lambda i:np.linalg.norm(np.array(cs[(i+1)%n])-np.array(cs[i])))
        out[best]=2
    return out

# ------------------------------------------------------------------ parking stalls, islands, lot lights
paint=[]      # (polygon, colour)
islands=[];trees=[];shrubs=[];lights=[];pylons=[];signals=[];police=[]
def line_poly(a,b,w):return LineString([tuple(a),tuple(b)]).buffer(w/2,cap_style=2)
def lot_axis(piece):
    # stalls run square to the nearest building's front, else to the road
    c=piece.centroid;best=None
    for b in buildings:
        for p in b['parts']:
            dd=p.distance(c)
            if dd<45 and (best is None or dd<best[0]):best=(dd,p)
    if best:
        r=best[1].minimum_rotated_rectangle;cs=np.array(r.exterior.coords)
        e=cs[1]-cs[0] if np.linalg.norm(cs[1]-cs[0])>np.linalg.norm(cs[2]-cs[1]) else cs[2]-cs[1]
        return e/np.linalg.norm(e)
    s,d=gd((c.x,c.y));_,tan,_=at(GCL,GS,s);return tan
mouth_zone=unary_union([Point(m).buffer(14) for m in drive_mouths]+[HWY.buffer(ROAD_EDGE+9)]) if drive_mouths else HWY.buffer(ROAD_EDGE+9)
nstall=0
for piece in sorted(pieces(paved),key=lambda p:-p.area):
    if piece.area<350:continue
    u=lot_axis(piece);v=np.array([-u[1],u[0]])
    cs=np.array(piece.exterior.coords);pu=cs@u;pv=cs@v
    inner=piece.buffer(-1.2)
    if inner.is_empty:continue
    blocked=bfoot.buffer(3.5).union(mouth_zone).union(STREETS.buffer(1))
    # modules across v: aisle 6.6 | stall 5.2 | stall 5.2 (back to back) ...
    v0=pv.min()+6.6;stall=5.2;width=2.75
    vv=v0
    while vv+2*stall<pv.max()-1:
        for row,(va,vb) in enumerate(((vv,vv+stall),(vv+stall,vv+2*stall))):
            run=[]
            uu=pu.min()+1
            while uu<pu.max():
                a=u*uu+v*va;b=u*uu+v*vb
                seg=LineString([tuple(a),tuple(b)])
                ok=inner.contains(seg) and not blocked.intersects(seg)
                if ok:run.append(uu)
                if (not ok or uu+width>=pu.max()) and len(run)>=3:
                    for x in run:paint.append((line_poly(u*x+v*va,u*x+v*vb,.13),'white'))
                    nstall+=len(run)-1
                    # an island at each end of the run with a tree (only on the back-to-back row pair's outer row)
                    if row==0 and len(run)>=5:
                        for endu,sgn in((run[0],-1),(run[-1],1)):
                            isl=Polygon([tuple(u*(endu+sgn*.25)+v*(vv+.3)),tuple(u*(endu+sgn*2.4)+v*(vv+.3)),tuple(u*(endu+sgn*2.4)+v*(vv+2*stall-.3)),tuple(u*(endu+sgn*.25)+v*(vv+2*stall-.3))])
                            if piece.contains(isl) and not bfoot.buffer(1).intersects(isl) and not HWY.buffer(ROAD_EDGE+4).intersects(isl):
                                islands.append(isl);c=isl.centroid
                                if not SOLID_CLEAR.contains(c):trees.append((c.x,c.y,rng.uniform(.8,1.1),'island'))
                    run=[]
                elif not ok:run=[]
                uu+=width
        vv+=2*stall+6.6
say(f'parking: {nstall} stalls painted, {len(islands)} islands with trees')

# ------------------------------------------------------------------ fast-food drive-through lanes (a painted lane around the building)
for b in buildings:
    if b['kind'] in('fastfood','bank','pharmacy') and 'passage' not in b:
        ring=b['parts'][0].buffer(4.2,join_style=2).exterior
        seg=LineString(ring.coords)
        lane=seg.buffer(.08)
        lane=lane.intersection(paved)
        if not lane.is_empty:paint.append((lane,'yellow'))

# ------------------------------------------------------------------ sidewalks along Hwy 92
sw=[]
for side in(1,-1):
    pts=[]
    for i,s in enumerate(np.arange(GS[0],GS[-1],2.0)):
        p,tan,nrm=at(GCL,GS,s)
        if XMIN<=p[0]<=XMAX:pts.append(tuple(p+nrm*side*12.0))
    if len(pts)>1:sw.append(LineString(pts).buffer(.9,cap_style=2))
SIDEWALK=unary_union(sw).difference(SIDE_ROAD).difference(STREETS.buffer(-0.5)).difference(STEEP.buffer(1)).intersection(WORLD)
SIDEWALK=unary_union([p for p in pieces(SIDEWALK) if p.area>=12])
say(f'sidewalks: {SIDEWALK.area:.0f} m2 along both sides')

# ------------------------------------------------------------------ street lights, landscaping along the frontage
solid_ok=lambda pt,r=0: not (SOLID_CLEAR.contains(pt) or PROT.contains(pt) or SIDE_ROAD.buffer(4).contains(pt) or STREETS.buffer(2).contains(pt) or plot_game.contains(pt) or not WORLD.buffer(-3).contains(pt) or bfoot.buffer(2+r).contains(pt) or any(Point(m).distance(pt)<9 for m in drive_mouths))
for side,offset in((1,0),(-1,30)):
    s=GS[0]+offset
    while s<GS[-1]:
        p,tan,nrm=at(GCL,GS,s);q=p+nrm*side*14.0
        if XMIN+5<q[0]<XMAX-5 and solid_ok(Point(q)):lights.append((q[0],q[1],math.degrees(math.atan2(-nrm[0]*side,-nrm[1]*side))))
        s+=58
say(f'street lights: {len(lights)}')
for side in(1,-1):
    s=GS[0]+11
    while s<GS[-1]:
        p,tan,nrm=at(GCL,GS,s);q=p+nrm*side*rng.uniform(16.5,19.5);pt=Point(q)
        if XMIN+5<q[0]<XMAX-5 and solid_ok(pt) and not paved.buffer(1.5).contains(pt) and not any(Point(l[0],l[1]).distance(pt)<7 for l in lights):
            trees.append((q[0],q[1],rng.uniform(.85,1.2),'frontage'))
        s+=rng.uniform(15,24)
for b in buildings:
    if b['kind'] in('house','apartments','senior','storage'):continue
    p=b['parts'][0];fr=fronts(p);cs=list(p.exterior.coords)[:-1]
    for i,f in enumerate(fr):
        if f!=2:continue
        a=np.array(cs[i]);c=np.array(cs[(i+1)%len(cs)]);e=c-a;Lr=np.linalg.norm(e)
        nrm=np.array([e[1],-e[0]])/Lr if Polygon(cs).exterior.is_ccw else np.array([-e[1],e[0]])/Lr
        for t in np.arange(1.5,Lr-1.5,3.2):
            q=a+e/Lr*t+nrm*1.1
            if not paved.buffer(-.3).contains(Point(q)):shrubs.append((q[0],q[1],rng.uniform(.7,1.1)))
say(f'trees {len(trees)}, shrubs {len(shrubs)}')

# ------------------------------------------------------------------ pylon signs: one per business lot, at the road
def front_point(poly):
    c=poly.centroid;s,d=gd((c.x,c.y));p,tan,nrm=at(GCL,GS,s);return s,d,p,tan,nrm
for b in buildings:
    k=b['kind']
    if k in('house','apartments'):continue
    s,d,p,tan,nrm=front_point(b['parts'][0])
    if abs(d)>95:continue
    side=math.copysign(1,d)
    for ds in(0,10,-10,20,-20,30,-30):
        q=at(GCL,GS,s+ds)[0]+at(GCL,GS,s+ds)[2]*side*16.5;pt=Point(q)
        if solid_ok(pt,1) and not any(Point(x[0],x[1]).distance(pt)<14 for x in pylons) and not any(Point(l[0],l[1]).distance(pt)<6 for l in lights):
            b['name']=b.get('name') or name_for({'strip':'tenant','retail':'tenant'}.get(k,k)) or ''
            tall=k in('grocery','strip','gas','convenience','fastfood')
            face=tan*side;yaw=math.degrees(math.atan2(face[0],face[1]))   # the lettered face looks at the traffic coming on this side
            label=b['name'] if k not in('strip',) else 'Shops'
            pylons.append((q[0],q[1],yaw,label,'tall' if tall else 'low',k));break
for c in canopies:
    if c['kind']!='canopy_fuel':continue
    s,d,p,tan,nrm=front_point(c['poly']);side=math.copysign(1,d)
    c['name']=name_for('gas')
    for ds in(-12,12,-22,22,0):
        q=at(GCL,GS,s+ds)[0]+at(GCL,GS,s+ds)[2]*side*16.5;pt=Point(q)
        if solid_ok(pt,1) and not any(Point(x[0],x[1]).distance(pt)<12 for x in pylons):
            pylons.append((q[0],q[1],math.degrees(math.atan2(tan[0]*side,tan[1]*side)),c['name'],'gas','gas'));break
say(f'pylon signs: {len(pylons)}')

# ------------------------------------------------------------------ names for the rest; storefront tenants
for b in buildings:
    k=b['kind']
    if 'name' not in b:b['name']=name_for({'strip':'tenant','retail':'tenant'}.get(k,k)) if k not in('house','apartments') else ''
    if k=='strip':
        p=b['parts'][0];fr=fronts(p);cs=list(p.exterior.coords)[:-1]
        i=max(range(len(cs)),key=lambda i:(fr[i]>0)*np.linalg.norm(np.array(cs[(i+1)%len(cs)])-np.array(cs[i])))
        n=max(2,int(np.linalg.norm(np.array(cs[(i+1)%len(cs)])-np.array(cs[i]))/8.5))
        b['tenants']=[name_for('tenant') for _ in range(n)]

# ------------------------------------------------------------------ traffic lights at today's signalled junctions
sig=[np.array(ll2xy(e['lat'],e['lon'])) for e in osm['elements'] if e['type']=='node' and e.get('tags',{}).get('highway')=='traffic_signals']
cl=[]
for q in sig:
    for c in cl:
        if np.linalg.norm(np.mean(c,0)-q)<70:c.append(q);break
    else:cl.append([q])
for c in cl:
    m=np.mean(c,0);s_r,d_r=project(RCL,RS,m)
    if abs(d_r)>40:continue
    g=r2g(m)
    if not(XMIN+20<g[0]<XMAX-20):continue
    s,_=gd(g)
    signals.append(s)
    say(f'traffic lights at x {g[0]:.0f} (real junction at {np.round(m,0).tolist()}, {len(c)} signal nodes)')
def arrow(base,t,shape):
    """a lane arrow (paint), 7.5 m long, pointing along t; 'left' = through + left turn, 'right' = through + right turn"""
    r=np.array([t[1],-t[0]])
    def P(a,c):return tuple(base+t*a+r*c)
    parts=[Polygon([P(0,-.15),P(5.6,-.15),P(5.6,.15),P(0,.15)]),Polygon([P(5.4,-.5),P(7.5,0),P(5.4,.5)])]
    k=-1 if shape=='left' else 1
    parts.append(LineString([P(2.2,0),P(3.9,k*1.0)]).buffer(.15,cap_style=2))
    parts.append(Polygon([P(3.4,k*1.45),P(4.7,k*1.35),P(4.1,k*.5)]))
    return unary_union(parts)
sig_objs=[]
for s in signals:
    p,tan,nrm=at(GCL,GS,s)
    for direction in(1,-1):     # +1 eastbound (south half, right of travel), -1 westbound
        t=tan*direction;right=np.array([t[1],-t[0]])
        # the mast pole on the far right corner, past the junction, behind the sidewalk; its arm reaches over this direction's lanes
        pole=None
        for along in(16,22,28,34,10):
            q=p+t*along+right*14.6
            if not(SOLID_CLEAR.contains(Point(q)) or PROT.contains(Point(q)) or STREETS.buffer(1.5).contains(Point(q)) or SIDE_ROAD.buffer(1.5).contains(Point(q)) or bfoot.buffer(2).contains(Point(q))):pole=q;break
        if pole is None:say(f'  no clear corner for a signal pole at s {s:.0f} ({direction})');continue
        sig_objs.append(dict(pole=[round(pole[0],2),round(pole[1],2)],arm=[round(-right[0],4),round(-right[1],4)],face=[round(-t[0],4),round(-t[1],4)],heads=[14.6-6.15,14.6-2.05]))
        # the stop bar across this direction's two lanes 13 m before the junction, lane arrows 30 m before it
        a=p-t*13+right*.35;b=p-t*13+right*8.0;paint.append((line_poly(a,b,.45),'white'))
        for lane,shape in((2.05,'left'),(6.15,'right')):paint.append((arrow(p-t*43+right*lane,t,shape),'white'))
    # a crosswalk across Hwy 92 on one side of the junction
    for ofs in(-10.6,-8.2):
        a=p+tan*ofs+nrm*8.0;b=p+tan*ofs-nrm*8.0;paint.append((line_poly(a,b,.3),'white'))
say(f'traffic light poles: {len(sig_objs)}')

# ------------------------------------------------------------------ hidden-police places on Hwy 92: in a lot, nose to the road
cands=[]
for piece in pieces(paved):
    for b in [piece.boundary]:
        pass
for m in drive_mouths:
    s,d=gd(m);p,tan,nrm=at(GCL,GS,s);side=math.copysign(1,d)
    for ds in(9,-9,14,-14,20,-20,26,-26):
        q=at(GCL,GS,s+ds)[0]+at(GCL,GS,s+ds)[2]*side*(ROAD_EDGE+(11.5 if abs(ds)<15 else 15));pt=Point(q)
        if paved.buffer(-1.6).contains(pt) and not bfoot.buffer(5).contains(pt) and not any(pp[0].distance(pt)<8 for pp in [(Point(x[0],x[1]),) for x in pylons]):
            cands.append((q,-nrm*side));break
rng2=random.Random(92)
rng2.shuffle(cands)
for q,f in cands:
    if all(np.linalg.norm(q-np.array(x[0]))>170 for x in police):police.append((q,f))
    if len(police)>=4:break
say(f'hidden-police places on Hwy 92: {[np.round(q,1).tolist() for q,_ in police]}')

if __name__=='__main__' and 'diag' in sys.argv:
    from collections import Counter
    print(Counter(r.split(':')[0] if 'rises' in r else r for r,_ in skipped))
    for r,i in skipped:
        if 'rises' in r:print(' ',r,i)
    for b in buildings:
        dsc=b['poly'].distance(SHORT)
        if dsc<25:print('near shortcut',b['kind'],round(dsc,1),np.round(b['poly'].centroid.coords[0],1),'parts',len(b['parts']),[round(p.area) for p in b['parts']],round(b['poly'].area))

# ------------------------------------------------------------------ output
def tris(geom):
    out=[]
    for p in pieces(geom):
        p=p.buffer(0)
        for q in pieces(p):
            for t in shapely.constrained_delaunay_triangles(q).geoms:
                cs=list(t.exterior.coords)[:3]
                if t.area<1e-3:continue
                # clockwise seen from above (x right, z up), the order Unity draws facing up (the game also checks)
                (ax,az),(bx,bz),(cx,cz)=cs
                if (bx-ax)*(cz-az)-(bz-az)*(cx-ax)>0:cs=[cs[0],cs[2],cs[1]]
                out+= [round(v,2) for c in cs for v in c]
    return out
def part_out(p):
    p=shapely.geometry.polygon.orient(p.simplify(.15),1.0)   # counter-clockwise
    return dict(pts=coords(p),fronts=fronts(p),roof=tris(p))
def removal_sites():
    sites={}
    for l in open('C:/Users/danmo/AppData/Local/Temp/report104/survey/survey-StreetLoopReverse.txt'):
        if 'CR019' in l and '/Foundation |' in l:
            nm=l.split('/')[1];c=l.split('| (')[1].split(')')[0].split(',');sites[nm]=(float(c[0]),float(c[2]))
    rem=[k for k,(x,z) in sorted(sites.items()) if not plot_game.contains(Point(x,z))]
    keep=[k for k,(x,z) in sorted(sites.items()) if plot_game.contains(Point(x,z))]
    return rem,keep,sites
remove,keep_in_plot,sites=removal_sites()
say(f'old placeholder roadside removed: {len(remove)} ({", ".join(remove)}); inside the stunt plot, left as they are: {", ".join(keep_in_plot) or "none"}')
# yellow centre lines on today's side streets; white edge of the sidewalk is its own colour
for line,half,nm in stub_lines:
    seg=line.difference(HWY.buffer(ROAD_EDGE+3))
    for off_ in(-.18,.18):
        l2=seg.parallel_offset(off_) if seg.geom_type=='LineString' and seg.length>4 else None
        if l2 is not None and not l2.is_empty:paint.append((l2.buffer(.06,cap_style=2).intersection(STREETS),'yellow'))
white=unary_union([g for g,c in paint if c=='white']);yellow=unary_union([g for g,c in paint if c=='yellow'])
asphalt=paved.difference(STREETS)
island_g=unary_union(islands) if islands else Polygon()
clear=unary_union([b for b in [bfoot.buffer(3),paved.buffer(1.5),STREETS.buffer(2),SIDEWALK.buffer(1),island_g]+[c['poly'].buffer(2) for c in canopies]+[Point(x[0],x[1]).buffer(3) for x in lights+pylons]+[Point(o['pole']).buffer(3) for o in sig_objs]]).difference(plot_game).intersection(WORLD)
# the clear zone as a 1 m bitmap (x from XMIN, z from 380), zlib + base64
import cv2
W=int(XMAX-XMIN)+1;H=int(ZMAX-380)+1;bm=np.zeros((H,W),np.uint8)
for p in pieces(clear):
    cv2.fillPoly(bm,[np.array([[x-XMIN,z-380] for x,z in p.exterior.coords],np.int32)],1)
    for h in p.interiors:cv2.fillPoly(bm,[np.array([[x-XMIN,z-380] for x,z in h.coords],np.int32)],0)
data=dict(version='0.104',source='OpenStreetMap contributors (ODbL) and USGS NAIP imagery, placement only; every name invented',
    stretch=dict(x0=XMIN,x1=XMAX,z0=380,z1=ZMAX),
    buildings=[dict(kind=b['kind'],name=b.get('name',''),h=round(b['h'],2),tenants=b.get('tenants',[]),parts=[part_out(p) for p in b['parts']],
        osm=b['src']['id']) for b in buildings],
    canopies=[dict(kind=c['kind'],name=c.get('name',''),pts=coords(shapely.geometry.polygon.orient(c['poly'].minimum_rotated_rectangle,1.0)),h=5.4 if c['kind']=='canopy_fuel' else 4.2) for c in canopies],
    flat=dict(asphalt=tris(asphalt),street=tris(STREETS),sidewalk=tris(SIDEWALK.difference(asphalt.buffer(-.01)) if False else SIDEWALK),island=tris(island_g),white=tris(white),yellow=tris(yellow)),
    lights=[round(v,2) for a,b,c in lights for v in (a,b,c)],
    trees=[round(v,2) for a,b,c,_ in trees for v in (a,b,c,0 if SOLID_CLEAR.contains(Point(a,b)) else 1)],
    shrubs=[round(v,2) for a,b,c in shrubs for v in (a,b,c)],
    pylons=[dict(x=round(a,2),z=round(b,2),yaw=round(c,1),label=lab,style=st,kind=k) for a,b,c,lab,st,k in pylons],
    signals=sig_objs,
    police=[dict(pos=[round(q[0],2),round(q[1],2)],forward=[round(f[0],4),round(f[1],4)]) for q,f in police],
    remove=remove,keepInPlot=keep_in_plot,
    plot=coords(plot_game),
    clear=dict(x0=XMIN,z0=380,w=W,h=H,bits=base64.b64encode(zlib.compress(np.packbits(bm.flatten()).tobytes(),9)).decode()))
import os;os.makedirs(os.path.dirname(OUT),exist_ok=True)
json.dump(data,open(OUT,'w'),separators=(',',':'))
say(f'written {OUT}: {os.path.getsize(OUT)/1e6:.2f} MB; flat triangles: '+', '.join(f'{k} {len(v)//6}' for k,v in data['flat'].items()))
open(ROOT+'/Docs/Report104/Lists/plan.txt','w').write('\n'.join(log)+'\n') if os.path.isdir(ROOT+'/Docs/Report104/Lists') else None
pickle.dump(dict(paved=paved,streets=STREETS,sidewalk=SIDEWALK,bfoot=bfoot,plot=plot_game,buildings=[(b['kind'],b['parts']) for b in buildings],canopies=[c['poly'] for c in canopies],lights=lights,trees=trees,pylons=pylons,signals=sig_objs,police=police,white=white,yellow=yellow,islands=island_g,clear=clear),open('plan.pkl','wb'))
