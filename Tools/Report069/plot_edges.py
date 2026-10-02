# Top-down plot of Report069Edges rows near a point: edge points per route (colour), red where ground 1 m out is >2 m below.
import csv,sys
from PIL import Image,ImageDraw
src,dst,cx,cz,R=sys.argv[1],sys.argv[2],float(sys.argv[3]),float(sys.argv[4]),float(sys.argv[5]); S=900/(2*R)
img=Image.new('RGB',(940,940),(255,255,255)); d=ImageDraw.Draw(img)
P=lambda x,z:(20+(x-cx+R)*S,20+(cz+R-z)*S)
cols={'Main':(40,90,200),'Summit Traverse':(230,150,0),'Climbing Ridge Cut':(0,160,80),'Downhill Ridge Cut':(160,0,160)}
for r in csv.DictReader(open(src)):
    x,z=float(r['x']),float(r['z'])
    if abs(x-cx)>R or abs(z-cz)>R: continue
    g=float(r['g10']) if r['g10']!='NaN' else -99
    c=(220,0,0) if g<-2 else cols.get(r['route'],(0,0,0))
    px,pz=P(x,z); d.ellipse([px-2,pz-2,px+2,pz+2],fill=c)
    s=float(r['s'])
    if s%10==0 and r['side']=='1': d.text((px+3,pz),f"{r['route'][:2]}{s:.0f}",fill=(0,0,0))
for i in range(-int(R),int(R)+1,5):
    d.text(P(cx+i,cz+R-1),f"{cx+i:.0f}",fill=(90,90,90)); d.text(P(cx-R,cz+i),f"{cz+i:.0f}",fill=(90,90,90))
img.save(dst)
