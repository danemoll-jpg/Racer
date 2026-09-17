import json,heapq,math,pathlib
p=pathlib.Path('Docs/CR016-017');trees=json.loads((p/'local-trunks.json').read_text());step=2
nodes={(x,z) for x in range(320,451,step) for z in range(25,137,step) if all(math.hypot(x-t[0],z-t[2])>2.9 for t in trees)}
a=min(nodes,key=lambda v:math.dist(v,(350,110)));b=min(nodes,key=lambda v:math.dist(v,(418,30)));q=[(0,a)];dist={a:0};prev={}
while q:
 _,v=heapq.heappop(q)
 if v==b:break
 for dx,dz in [(step,0),(-step,0),(0,step),(0,-step),(step,step),(step,-step),(-step,step),(-step,-step)]:
  w=(v[0]+dx,v[1]+dz)
  if w not in nodes or (v[0]+dx,v[1]) not in nodes or (v[0],v[1]+dz) not in nodes:continue
  c=dist[v]+math.hypot(dx,dz)
  if c<dist.get(w,1e9):dist[w]=c;prev[w]=v;heapq.heappush(q,(c+math.dist(w,b),w))
assert b in prev
path=[b]
while path[-1]!=a:path.append(prev[path[-1]])
path.reverse();(p/'forest-path.json').write_text(json.dumps([{'x':x,'y':0,'z':z} for x,z in path]));print('forest path',a,b,'length',dist[b])
