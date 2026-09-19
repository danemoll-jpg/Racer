from pathlib import Path
import json, math
from PIL import Image, ImageDraw, ImageFont
root=Path(__file__).resolve().parents[1]/'Docs/CR070-074'
fontpath='C:/Windows/Fonts/arial.ttf'
font=lambda n:ImageFont.truetype(fontpath,n)
for course in ('StreetLoopReverse','ForestLoopReverse'):
    data=json.loads((root/f'route-{course}.json').read_text())
    road=data['road'];branches=data['branches'];allp=road+[p for b in branches for p in b['points']]
    loX=min(p['x'] for p in allp)-80;hiX=max(p['x'] for p in allp)+80
    loZ=min(p['z'] for p in allp)-80;hiZ=max(p['z'] for p in allp)+80
    scale=min(1150/(hiX-loX),800/(hiZ-loZ))
    def xy(p):return (80+(p['x']-loX)*scale,115+(hiZ-p['z'])*scale)
    im=Image.new('RGB',(1400,1200),'#f5f3ed');d=ImageDraw.Draw(im)
    title='Street Loop Reverse' if course.startswith('Street') else 'Forest Loop Reverse'
    d.text((65,32),title+' | branches and rejoins',font=font(34),fill='#263943')
    for x in range(math.ceil(loX/100)*100,int(hiX),100):
        a=xy({'x':x,'z':hiZ});b=xy({'x':x,'z':loZ});d.line([a,b],fill='#dddcd5');d.text((a[0]-12,b[1]+12),str(x),font=font(15),fill='#65716d')
    for z in range(math.ceil(loZ/100)*100,int(hiZ),100):
        a=xy({'x':loX,'z':z});b=xy({'x':hiX,'z':z});d.line([a,b],fill='#dddcd5');d.text((18,a[1]-8),str(z),font=font(15),fill='#65716d')
    route=[xy(p) for p in road];d.line(route+[route[0]],fill='#536871',width=6)
    def arrow(a,b,color):
        dx=b[0]-a[0];dy=b[1]-a[1];length=math.hypot(dx,dy)
        if length<1:return
        dx/=length;dy/=length
        d.polygon([b,(b[0]-dx*15-dy*7,b[1]-dy*15+dx*7),(b[0]-dx*15+dy*7,b[1]-dy*15-dx*7)],fill=color)
    for i in range(30,len(route)-20,max(30,len(route)//10)):arrow(route[i],route[i+12],'#536871')
    for i,g in enumerate(data.get('gates',[])):
        x,y=xy(g);d.rectangle((x-4,y-4,x+4,y+4),fill='#33424d')
        d.text((x-26,y-17),'START' if i==0 else str(i),font=font(15),fill='#33424d',stroke_width=1,stroke_fill='#f5f3ed')
    for k,(branch,color) in enumerate(zip(branches,('#007f79','#ba501b'))):
        p=[xy(a) for a in branch['points']];d.line(p,fill=color,width=5)
        x,y=p[0];d.ellipse((x-8,y-8,x+8,y+8),fill=color)
        d.text((x+12,y-26),'Entry '+str(k+1),font=font(20),fill=color)
        x,y=p[-1];d.polygon([(x,y-10),(x+10,y),(x,y+10),(x-10,y)],fill=color)
        d.text((x+12,y+8),'Rejoin '+str(k+1),font=font(20),fill=color)
        arrow(p[len(p)//2],p[min(len(p)-1,len(p)//2+10)],color)
        bypass=', '.join(map(str,branch['gates'])) or 'none'
        d.text((70,990+k*48),f'{k+1}. '+branch['title']+' | explicit bypass: CP '+bypass,font=font(26),fill=color)
    d.text((70,1105),'Circles: optional entrances. Diamonds: rejoins. Arrows: race direction.',font=font(22),fill='#40565f')
    d.text((70,1140),'World X / Z in metres; +Z is north. Bypass credit is earned at branch entry.',font=font(20),fill='#40565f')
    im.save(root/f'map-{course}.png')
