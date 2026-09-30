"""Annotate the saved-scene orthographic image; all overlay coordinates are exported by Unity."""
import json, math
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont
root=Path(__file__).resolve().parents[1]; out=root/'Docs/WorldMap'
Image.MAX_IMAGE_PIXELS=150_000_000
data=json.loads((out/'DansBackyardForward.json').read_text()); backyard_bounds=data['bounds']
b=json.loads((out/'MountainLoop.json').read_text())['bounds']
base=Image.open(out/'MOUNTAIN_BASE.png').convert('RGB'); w,h=base.size
# The project stores regional geography in separate course scene variants.
# Composite only actual orthographic pixels with exact world-coordinate registration.
def paste_region(source_name, source_scene, region):
    src=Image.open(out/source_name).convert('RGB');sb=json.loads((out/(source_scene+'.json')).read_text())['bounds']
    x0,z0,x1,z1=region
    crop=(round((x0-sb['x'])/sb['width']*src.width),round((sb['y']+sb['height']-z1)/sb['height']*src.height),round((x1-sb['x'])/sb['width']*src.width),round((sb['y']+sb['height']-z0)/sb['height']*src.height))
    target=(round((x0-b['x'])/b['width']*w),round((b['y']+b['height']-z1)/b['height']*h),round((x1-b['x'])/b['width']*w),round((b['y']+b['height']-z0)/b['height']*h))
    base.paste(src.crop(crop).resize((target[2]-target[0],target[3]-target[1]),Image.Resampling.LANCZOS),target[:2])
paste_region('FOREST_BASE.png','LakeWoods',(-1350,-850,800,950))
paste_region('BACKYARD_BASE.png','DansBackyardReverse',(-40,-220,600,206.666667))
base.save(out/'WORLD_BASE.png')
def font(n,bold=False): return ImageFont.truetype('C:/Windows/Fonts/'+('arialbd.ttf' if bold else 'arial.ttf'),n)
def point(p): return ((p['x']-b['x'])/b['width']*w,(b['y']+b['height']-p['z'])/b['height']*h)
canvas=Image.new('RGB',(w+300,h+440),'#10262b');canvas.paste(base,(150,240)); d=ImageDraw.Draw(canvas,'RGBA')
d.text((150,40),'WOODSTOCK RUSH / WORLD MAP',font=font(90,True),fill='white')
d.text((150,153),'Actual saved world geometry  •  North / +Z up  •  Unity X/Z metres  •  200 m grid',font=font(42),fill='#bcd7d9')
for x in range(math.ceil(b['x']/200)*200,int(b['x']+b['width'])+1,200):
    px=150+(x-b['x'])/b['width']*w;d.line((px,240,px,h+240),fill=(230,250,255,70),width=2);d.text((px,h+266),f'X {x}',anchor='mt',font=font(32),fill='white')
for z in range(math.ceil(b['y']/200)*200,int(b['y']+b['height'])+1,200):
    py=240+(b['y']+b['height']-z)/b['height']*h;d.line((150,py,w+150,py),fill=(230,250,255,70),width=2);d.text((140,py),f'Z {z}',anchor='rm',font=font(30),fill='white')
def label(title,p):
    x,y=point(p);x+=150;y+=240;rect=d.textbbox((x+18,y+12),title,font=font(35,True));d.rounded_rectangle((rect[0]-9,rect[1]-6,rect[2]+9,rect[3]+6),radius=8,fill=(7,22,27,215));d.ellipse((x-7,y-7,x+7,y+7),fill='#ffdb8a');d.text((x+18,y+12),title,font=font(35,True),fill='white')
for item in data['landmarks']:label(item['title'],item['position'])
# These planning anchors are the accepted Backyard atlas coordinates, all within the saved terrain.
for title,p in [('Garbage dump',{'x':282,'z':12}),('Extended ravine',{'x':96,'z':-20})]:label(title,p)
d.text((150,h+355),f"Coverage X {b['x']:g} … {b['x']+b['width']:g} / Z {b['y']:g} … {b['y']+b['height']:g}   |   Scene composite: Forest / Backyard / Mountain   |   Clean geography: WORLD_BASE.png",font=font(33),fill='#bcd7d9')
canvas.save(out/'WORLD_MAP.png');canvas.resize((1500,round(canvas.height*1500/canvas.width))).save(out/'WORLD_MAP-preview.png')
# Reusable transparent overlays; do not bake all courses into the clean world image.
overlay_dir=out/'Overlays';overlay_dir.mkdir(exist_ok=True)
for path in out.glob('*.json'):
    course=json.loads(path.read_text())
    if not isinstance(course,dict) or 'route' not in course:continue
    cb=b;ow=w;oh=h;layer=Image.new('RGBA',(ow,oh));q=ImageDraw.Draw(layer)
    def xy(p):return ((p['x']-cb['x'])/cb['width']*ow,(cb['y']+cb['height']-p['z'])/cb['height']*oh)
    def route(points,color,closed=False):
        pts=[xy(p) for p in points];pts+=pts[:1] if closed else [];q.line(pts,fill='#102b31',width=10,joint='curve');q.line(pts,fill=color,width=5,joint='curve');distance=0
        for a,c in zip(pts,pts[1:]):
            length=math.dist(a,c);distance+=length
            if distance<150 or not length:continue
            distance=0;ux=(c[0]-a[0])/length;uy=(c[1]-a[1])/length;q.polygon([c,(c[0]-ux*16-uy*8,c[1]-uy*16+ux*8),(c[0]-ux*16+uy*8,c[1]-uy*16-ux*8)],fill='white')
    route(course['route'],'#4ff4e3',True)
    for branch in course['branches']:route(branch['points'],'#ffbd48')
    for i,g in enumerate(course['gates']):
        x,y=xy(g['position']);q.ellipse((x-10,y-10,x+10,y+10),fill='white' if i==0 else '#397deb',outline='#102b31',width=2);q.text((x+15,y-8),'S/F' if i==0 else str(i),font=font(24,True),fill='white',stroke_width=2,stroke_fill='#102b31')
    layer.save(overlay_dir/(path.stem+'.png'))
print(f'WORLD_MAP.png {canvas.width}x{canvas.height}; base {w}x{h}; seven selectable transparent course overlays')

