# 0.113 Part C: the OpenStreetMap map-API download (XML, https://api.openstreetmap.org/api/0.6/map?bbox=-84.4925,34.0695,-84.4735,34.0900) to osm-trickum.json (ways with geometry, tagged nodes)
import sys,json,xml.etree.ElementTree as ET
src,dst=sys.argv[1],sys.argv[2]
r=ET.parse(src).getroot()
nodes={};out={'nodes':[],'ways':[]}
for n in r.findall('node'):
    nodes[n.get('id')]=(float(n.get('lat')),float(n.get('lon')))
    tags={t.get('k'):t.get('v') for t in n.findall('tag')}
    if tags:out['nodes'].append({'id':int(n.get('id')),'lat':nodes[n.get('id')][0],'lon':nodes[n.get('id')][1],'tags':tags})
for w in r.findall('way'):
    tags={t.get('k'):t.get('v') for t in w.findall('tag')}
    ids=[nd.get('ref') for nd in w.findall('nd')]
    geom=[{'lat':nodes[i][0],'lon':nodes[i][1]} for i in ids if i in nodes]
    out['ways'].append({'id':int(w.get('id')),'nodes':[int(i) for i in ids],'geometry':geom,'tags':tags})
json.dump(out,open(dst,'w'))
from collections import Counter
print(len(out['ways']),Counter(w['tags'].get('building') for w in out['ways'] if 'building' in w['tags']))
print(sorted(Counter(w['tags'].get('name') for w in out['ways'] if 'highway' in w['tags']).items(),key=lambda x:-x[1])[:40])
print(Counter(w['tags'].get('landuse') or w['tags'].get('amenity') or w['tags'].get('leisure') for w in out['ways'] if 'building' not in w['tags'] and 'highway' not in w['tags']))
