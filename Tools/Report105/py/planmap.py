# Top-down drawing of the planned roadside over the game's ground (2 px per m), with the race lines and the stunt plot outlined.
import pickle,json,numpy as np,sys
from PIL import Image,ImageDraw
from hwymap import *
from gen import pieces,SHORT
P=pickle.load(open('plan.pkl','rb'))
S=2;X0,Z0,X1,Z1=-800,380,800,850
W=(X1-X0)*S;H=(Z1-Z0)*S
z=np.load('C:/Users/danmo/AppData/Local/Temp/report104/grid/grid-StreetLoopReverse.npz');g=z['g']
img=Image.new('RGB',(W,H),(70,120,60));d=ImageDraw.Draw(img,'RGBA')
def px(x,zz):return ((x-X0)*S,(Z1-zz)*S)
# ground shade by height
gh=g.copy();lo,hi=np.nanmin(gh),np.nanmax(gh)
for i in range(gh.shape[0]):
    for j in range(gh.shape[1]):
        x=-1260+2*j;zz=380+2*i
        if X0<=x<X1 and not np.isnan(gh[i,j]):
            t=(gh[i,j]-lo)/(hi-lo);d.rectangle([px(x,zz+2),px(x+2,zz)],fill=(int(70+60*t),int(115+70*t),int(55+40*t)))
def poly(geom,fill,outline=None):
    for p in pieces(geom):
        d.polygon([px(x,zz) for x,zz in p.exterior.coords],fill=fill,outline=outline)
        for h in p.interiors:d.polygon([px(x,zz) for x,zz in h.coords],fill=(70,120,60))
d.line([px(x,zz) for x,zz in GCL if X0-50<x<X1+50],fill=(60,60,64),width=int(16.4*S))
poly(P['paved'],(64,66,70));poly(P['streets'],(60,62,66));poly(P['sidewalk'],(190,190,185));poly(P['islands'],(90,140,70))
poly(P['white'],(255,255,255));poly(P['yellow'],(240,200,40))
cols={'grocery':(200,90,60),'strip':(210,160,90),'retail':(200,170,120),'fastfood':(220,60,60),'bank':(90,120,200),'office':(150,150,170),'house':(180,150,130),'senior':(160,120,160),'church':(240,240,240)}
for k,parts in P['buildings']:
    for p in parts:poly(p,cols.get(k,(170,140,100)),(20,20,20))
for c in P['canopies']:poly(c,(230,230,230),(0,0,0))
for x,zz,*_ in P['trees']:a,b=px(x,zz);d.ellipse((a-4,b-4,a+4,b+4),fill=(30,90,30))
for x,zz,*_ in P['lights']:a,b=px(x,zz);d.ellipse((a-3,b-3,a+3,b+3),fill=(255,255,120))
for x,zz,*_ in P['pylons']:a,b=px(x,zz);d.rectangle((a-4,b-4,a+4,b+4),fill=(255,0,200))
for s in P['signals']:a,b=px(*s['pole']);d.ellipse((a-6,b-6,a+6,b+6),fill=(255,0,0))
for q,f in P['police']:a,b=px(*q);d.rectangle((a-6,b-10,a+6,b+10),outline=(0,0,255),width=3)
co=json.load(open(ROOT+'/Assets/Resources/WorldMaps/CoursePreviews.json'))['courses']
for c in co:
    if c['scene']=='StreetLoopReverse':
        d.line([px(p['x'],p['z']) for p in c['main']],fill=(0,200,255),width=2)
        for b in c['branches']:d.line([px(p['x'],p['z']) for p in b['points']],fill=(255,140,0),width=3)
d.line([px(x,zz) for x,zz in P['plot'].exterior.coords],fill=(255,255,0),width=4)
for x in range(-800,801,100):d.text(px(x,383),str(x),fill=(255,255,255))
img.save(sys.argv[1]);print(img.size)
