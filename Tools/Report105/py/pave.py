# Paved ground (parking lots, drives, aprons) along the stretch from the NAIP image: grey, unsaturated pixels, minus building
# roofs (OSM footprints) and Hwy 92 itself, cleaned (closing fills cars and tree shadows, small specks dropped), as polygons
# in real metres. Rasterised in real space at 1 m.
import numpy as np,cv2,math,json
from PIL import Image
from hwymap import *
from naip import IMG,xy2px,M,E
from shapely.geometry import Polygon,MultiPolygon,LineString
from shapely.ops import unary_union
X0,X1,Y0,Y1=-1260,1800,-150,560   # real metres covering the game's stretch and depth
def build():
    W=X1-X0;H=Y1-Y0
    xs=np.arange(X0,X1)+.5;ys=np.arange(Y1,Y0,-1)-.5
    # NAIP pixel for every 1 m cell (the image is ~1 m/px; nearest sample)
    lat=LAT0+ys/110574;lon=LON0+xs/(111320*math.cos(math.radians(LAT0)))
    mx=lon*20037508.34/180;my=np.log(np.tan((90+lat)*np.pi/360))*6378137
    px=((mx-E['xmin'])/(E['xmax']-E['xmin'])*M['width']).astype(int);py=((E['ymax']-my)/(E['ymax']-E['ymin'])*M['height']).astype(int)
    a=np.asarray(IMG).astype(int);rgb=a[np.clip(py,0,a.shape[0]-1)][:,np.clip(px,0,a.shape[1]-1)]
    sm=cv2.blur(np.ascontiguousarray(rgb.astype(np.float32)),(3,3))
    r,g,b=sm[...,0],sm[...,1],sm[...,2];br=sm.mean(-1)
    # asphalt and concrete: bright and neutral to bluish (grass and dry lawn are yellower: blue well below red; trees are dark green)
    grey=(br>132)&(br<235)&((b-r)>-9)&((g-b)<10)
    mask=np.ascontiguousarray(grey.astype(np.uint8)*255)
    def rast(polys,val,img):
        for p in polys:
            q=np.array([[(x-X0),(Y1-y)] for x,y in p],np.int32);cv2.fillPoly(img,[q],val)
    bld=np.zeros_like(mask);rast([wxy(w) for w in WAYS if 'building' in w.get('tags',{})],255,bld)
    # roads that are not lots: Hwy 92 corridor and the public side roads (kept out; curb cuts come from where lots meet them)
    road=np.zeros_like(mask)
    for w in WAYS:
        t=w.get('tags',{});hw=t.get('highway')
        if hw in('primary','primary_link','tertiary','residential','secondary','unclassified'):
            q=np.array([[(x-X0),(Y1-y)] for x,y in wxy(w)],np.int32);cv2.polylines(road,[q],False,255,thickness=int({'primary':12,'primary_link':9}.get(hw,9)))
    mask=cv2.morphologyEx(mask,cv2.MORPH_CLOSE,cv2.getStructuringElement(cv2.MORPH_ELLIPSE,(7,7)))
    mask=cv2.morphologyEx(mask,cv2.MORPH_OPEN,cv2.getStructuringElement(cv2.MORPH_ELLIPSE,(5,5)))
    mask[bld>0]=0;mask[road>0]=0
    n,lab,st,_=cv2.connectedComponentsWithStats(mask,8)
    keep=np.zeros_like(mask)
    for i in range(1,n):
        if st[i,cv2.CC_STAT_AREA]>=250:keep[lab==i]=255
    cs,_=cv2.findContours(keep,cv2.RETR_CCOMP,cv2.CHAIN_APPROX_SIMPLE)
    polys=[]
    for c in cs:
        if len(c)<4:continue
        p=Polygon([(X0+x,Y1-y) for [[x,y]] in c]).buffer(0)
        if p.area>=250:polys.append(p.simplify(1.2))
    U=unary_union(polys)
    return keep,U,rgb.astype(np.uint8),bld,road
if __name__=='__main__':
    keep,U,rgb,bld,road=build()
    print('paved area m2',round(U.area),'pieces',len(getattr(U,'geoms',[U])))
    vis=rgb.copy();vis[keep>0]=(vis[keep>0]*.4+np.array([255,0,255])*.6).astype(np.uint8);vis[bld>0]=(vis[bld>0]*.5+np.array([0,255,255])*.5).astype(np.uint8)
    Image.fromarray(vis).save('C:/Users/danmo/AppData/Local/Temp/report104/pave-vis.jpg')
    import pickle;pickle.dump(U,open('pave.pkl','wb'))
