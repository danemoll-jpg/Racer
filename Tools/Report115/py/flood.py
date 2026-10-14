import sys,collections,os
x0,z0=440,-150
name=sys.argv[1]; STEP=float(os.environ.get('STEP','0.45')); MINN=float(os.environ.get('MINN','0.87')); zmax=float(sys.argv[2])
nodes={}
for l in open(f'grid/grid-{name}.txt',encoding='utf-8'):
    p=l.rstrip('\n').split('|'); x,z=map(float,p[0].split()); nodes[(int(round(x-x0)),int(round(z-z0)))]=[(float(q.split(' ',2)[0]),float(q.split(' ',2)[1]),q.split(' ',2)[2]) for q in p[1:]]
def ok(h,ix,iz):
    if h[1]<MINN or 'trunk' in h[2].lower() or 'architecture' in h[2]: return False
    if (x0+ix-642)**2+(z0+iz+20)**2<float(os.environ.get("LAKER","67"))**2 and (h[0]<78.05 or os.environ.get("DRY","0")=="1"): return False
    return z0+iz<=zmax
def bfs(start):
    sx,sz=int(round(start[0]-x0)),int(round(start[1]-z0)); seen={}; dq=collections.deque()
    for k,h in enumerate(nodes[(sx,sz)]):
        if ok(h,sx,sz): seen[(sx,sz,k)]=None; dq.append((sx,sz,k))
    while dq:
        ix,iz,k=dq.popleft(); y=nodes[(ix,iz)][k][0]
        for dx,dz in ((1,0),(-1,0),(0,1),(0,-1)):
            n=(ix+dx,iz+dz)
            if n not in nodes: continue
            for kk,h in enumerate(nodes[n]):
                if ok(h,*n) and abs(h[0]-y)<=STEP and (n[0],n[1],kk) not in seen: seen[(n[0],n[1],kk)]=(ix,iz,k); dq.append((n[0],n[1],kk))
    return seen
seen=bfs(tuple(map(float,sys.argv[3].split(','))))
for t in sys.argv[4:]:
    tx,tz=map(float,t.split(',')); ix,iz=int(round(tx-x0)),int(round(tz-z0)); r=[k for k in range(3) if (ix,iz,k) in seen]
    print(name,'z<=',zmax,'from',sys.argv[3],'to',t,'REACHABLE' if r else 'not reachable')
    if r:
        node=(ix,iz,r[0]); path=[]
        while node: path.append(node); node=seen[node]
        print('   path via', [(x0+p[0],z0+p[1]) for p in path[::int(os.environ.get("EVERY","25"))]])
