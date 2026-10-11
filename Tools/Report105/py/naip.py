# NAIP image (EPSG:3857 extent in naip-hwy92.json) <-> local real metres
import json,math
from PIL import Image
from hwymap import ROOT,ll2xy,xy2ll
M=json.load(open(ROOT+'/SourceArt/Reference/Hwy92/naip-hwy92.json'));E=M['extent']
IMG=Image.open(ROOT+'/SourceArt/Reference/Hwy92/naip-hwy92.jpg')
def merc(lat,lon):return (lon*20037508.34/180,math.log(math.tan((90+lat)*math.pi/360))*6378137)
def xy2px(x,y):
    lat,lon=xy2ll(x,y);mx,my=merc(lat,lon)
    return ((mx-E['xmin'])/(E['xmax']-E['xmin'])*M['width'],(E['ymax']-my)/(E['ymax']-E['ymin'])*M['height'])
def crop(x0,y0,x1,y1):
    a=xy2px(x0,y1);b=xy2px(x1,y0);return IMG.crop((int(a[0]),int(a[1]),int(b[0]),int(b[1]))),a
