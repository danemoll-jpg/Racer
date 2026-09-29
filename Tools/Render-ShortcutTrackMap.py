"""Annotate a Unity orthographic render with exported saved-world coordinates."""
import json, math
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont
root=Path(__file__).resolve().parents[1]; out=root/'Docs/BackyardForward'
d=json.loads((out/'map-scene.json').read_text())
im=Image.new('RGB',(5040,3750),'#10262b');im.paste(Image.open(out/'SHORTCUT_OVERHEAD.png'),(120,220))
p=ImageDraw.Draw(im,'RGBA')
def font(n,b=False):return ImageFont.truetype('C:/Windows/Fonts/'+('arialbd.ttf' if b else 'arial.ttf'),n)
def xy(x,z):return (120+(x+40)*7.5,220+(206.666667-z)*7.5)
def pts(points):return [xy(a['x'],a['z']) for a in points]
def label(text,x,z,dx=0,dy=0,size=34,color='#ffffff'):
    a,b=xy(x,z);tx,ty=a+dx,b+dy
    box=p.multiline_textbbox((tx,ty),text,font=font(size,True),spacing=7)
    if dx or dy:p.line([(a,b),(tx,ty+15)],fill='#e0eeee',width=3)
    p.rounded_rectangle((box[0]-12,box[1]-9,box[2]+12,box[3]+10),radius=9,fill=(10,30,35,224))
    p.multiline_text((tx,ty),text,font=font(size,True),fill=color,spacing=7)
p.text((120,40),"DAN'S BACKYARD LOOP — FORWARD",font=font(66,True),fill='#ffffff')
p.text((120,128),'APPROVED COURSE  /  TWO OPTIONAL FOREST SHORTCUTS  /  Unity X/Z metres  /  +Z up',font=font(32),fill='#b6d1d4')
for x in range(0,601,50):
    a=xy(x,-220);b=xy(x,206.666667);p.line([a,b],fill=(235,255,255,75),width=2)
    p.text((a[0]-35,3440),f'X {x}',font=font(25),fill='#d7eeee')
for z in range(-200,201,50):
    a=xy(-40,z);b=xy(600,z);p.line([a,b],fill=(235,255,255,75),width=2)
    p.text((12,a[1]-15),f'Z {z}',font=font(23),fill='#d7eeee')
# Paved route centreline is sampled from the existing street network, clipped by image extent.
overlay=Image.new('RGBA',(4800,3200));q=ImageDraw.Draw(overlay)
for road in d['roads']:
    if road['name'] not in ('Phase 3 - Race Systems','Kyle descending driveway'):continue
    q.line([(a-120,b-220) for a,b in pts(road['points'])],fill=(218,225,237,165),width=9)
im.paste(overlay,(120,220),overlay);p=ImageDraw.Draw(im,'RGBA')
route=pts(d['route']);route.append(route[0]);p.line(route,fill='#112d32',width=21,joint='curve');p.line(route,fill='#4ff4e3',width=11,joint='curve')
distance=0
for a,b in zip(route,route[1:]):
    length=math.dist(a,b);distance+=length
    if distance<370 or length==0:continue
    distance=0;ux=(b[0]-a[0])/length;uy=(b[1]-a[1])/length
    p.polygon([(b[0]+ux*22,b[1]+uy*22),(b[0]-ux*18-uy*16,b[1]-uy*18+ux*16),(b[0]-ux*18+uy*16,b[1]-uy*18-ux*16)],fill='#ffffff',outline='#123137',width=3)
for gate in d['gates']:
    x,z=gate['x'],gate['z'];a,b=xy(x,z)
    if 'START' in gate['name']:
        p.rectangle((a-17,b-17,a+17,b+17),fill='white',outline='black',width=5)
        label('START / FINISH\nX 459.6  Z 8.1',x,z,95,-160,32)
    else:
        p.ellipse((a-22,b-22,a+22,b+22),fill='#246ae0',outline='white',width=4)
        n=gate['name'].split()[-1];p.text((a,b),n,font=font(27,True),anchor='mm',fill='white')
        label('CP '+n,x,z,28,22,26)
features=[('DUMP JUMP',(309.2,15.7),(-175,-280)),('FIRST GULLY JUMP',(96.1,-108.6),(-255,125)),('SECOND GULLY JUMP',(107.7,60.7),(-330,-165))]
for text,(x,z),(dx,dy) in features:
    a,b=xy(x,z);p.polygon([(a,b-24),(a+24,b),(a,b+24),(a-24,b)],fill='#ffbe4b',outline='#192c2e',width=4)
    label(f'{text}\nX {x}  Z {z}',x,z,dx,dy,31,color='#ffcc72')
label('Garbage dump\nApproved dense refuse',282,12,-220,230,30)
label('Extended gully / ravine',96,-20,-820,40,32)
label('Dirt / forest trail',192,-81,160,145,32,color='#f4d5a2')
label('Forest return',355,104,60,-200,32)
label("Dan's property",422,-23,-100,150,36)
label('Existing driveway / property pavement\nDirt boundary: X 391.9  Z 9.7',398,10,-230,-330,30)
label('South Cherokee Lane',470,140,115,10,34)
label("Kyle's driveway",511,-27,75,-30,30)
label('Existing lake-shore trail',550,1,40,-170,28)
label('Existing property / driveway',438,-145,-320,190,28)
p.text((4480,285),'N / +Z ↑',font=font(43,True),fill='#ffffff',stroke_width=3,stroke_fill='#173537')
y=3530
for x,c,text in [(120,'#4ff4e3','MAIN RACE ROUTE + arrows'),(1340,'#dbe1ed','EXISTING PAVED ROAD'),(2540,'#c4a377','DIRT / FOREST TRAIL'),(3620,'#ffbe4b','◆ MAJOR JUMP')]:
    p.line((x,y+20,x+80,y+20),fill=c,width=12);p.text((x+100,y),text,font=font(30,True),fill='white')
p.text((120,3600),'Blue numbered circles: checkpoints  •  White square: start/finish  •  Grid: 50 m  •  Trees, buildings, refuse and terrain: actual Unity scene render',font=font(29),fill='#c7dde0')
p.text((120,3660),'Gold: optional shortcuts. Circles: entries / squares: rejoins. Actual saved geometry; overlays show navigation, not trail width.',font=font(28),fill='#c7dde0')
for branch in d['branches']:
    line=pts(branch['points']);p.line(line,fill='#292310',width=21,joint='curve');p.line(line,fill='#ffbd48',width=11,joint='curve')
    for i in range(20,len(line)-10,40):
        a,b=line[i-3],line[i];length=math.dist(a,b);ux=(b[0]-a[0])/length;uy=(b[1]-a[1])/length
        p.polygon([(b[0]+ux*20,b[1]+uy*20),(b[0]-ux*16-uy*13,b[1]-uy*16+ux*13),(b[0]-ux*16+uy*13,b[1]-uy*16-ux*13)],fill='#fff0be',outline='#423411',width=3)
    a,b=line[0];p.ellipse((a-23,b-23,a+23,b+23),fill='#ffbd48',outline='#182a2b',width=4)
    a,b=line[-1];p.rectangle((a-21,b-21,a+21,b+21),fill='#ffbd48',outline='#182a2b',width=4)
    mid=branch['points'][len(branch['points'])//2]
    tree=branch['title']=='Tree-Top Trail'
    label(branch['title'].upper()+'\nOPTIONAL / HARD',mid['x'],mid['z'],-640 if tree else -270,0 if tree else -190,35,color='#ffcc72')
p.text((3100,128),'GOLD: OPTIONAL SHORTCUTS',font=font(32,True),fill='#ffcc72')
for branch in d['branches']:
    jump=35 if branch['title']=='Tree-Top Trail' else 41
    distance=0
    for a,b in zip(branch['points'],branch['points'][1:]):
        length=math.hypot(b['x']-a['x'],b['z']-a['z'])
        if distance+length>=jump:
            t=(jump-distance)/length;x=a['x']+(b['x']-a['x'])*t;z=a['z']+(b['z']-a['z'])*t
            px,pz=xy(x,z);p.polygon([(px,pz-18),(px+18,pz),(px,pz+18),(px-18,pz)],fill='#ffbe4b',outline='#192c2e',width=4)
            label('CANOPY LAUNCH' if jump==35 else 'CABIN ROOF LAUNCH',x,z,-330 if jump==35 else 50,-100 if jump==35 else 80,26,color='#ffcc72')
            break
        distance+=length
im.save(out/'TRACK_MAP.png');im.resize((1344,1000)).save(root/'Temp/map-preview.png')
print('TRACK_MAP.png: 5040 x 3750; actual approved scene and exported route/gates.')


