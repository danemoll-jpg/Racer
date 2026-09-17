import pathlib,subprocess,re,json,numpy as np
root=pathlib.Path('.')
def mesh(text,inds):
 n=int(re.search(r'm_VertexCount: (\d+)',text)[1]);b=bytes.fromhex(re.search(r'_typelessdata: ([0-9a-fA-F]+)',text)[1]);a=np.frombuffer(b,dtype=np.uint8).reshape(n,len(b)//n);channels=re.findall(r'- stream: (\d+)\s+offset: (\d+)\s+format: (\d+)\s+dimension: (\d+)',text)
 used=np.concatenate([a[:,int(channels[i][1]):int(channels[i][1])+int(channels[i][3])*4] for i in inds],axis=1)
 return used,re.search(r'm_IndexBuffer: ([0-9a-fA-F]+)',text)[1]
report={}
for folder,glob,inds,key in [('Assets/Vegetation/Phase6','Forest_*.asset',(0,1,3),'forest'),('Assets/Track/StreetLoop','Ground_*.asset',(0,1),'terrain')]:
 changed=[];files=list((root/folder).glob(glob))
 for p in files:
  old=subprocess.check_output(['git','show','1f90aed:'+p.as_posix()],stderr=subprocess.DEVNULL).decode();a,ai=mesh(p.read_text(),inds);b,bi=mesh(old,inds)
  if a.shape!=b.shape or not np.array_equal(a,b) or ai!=bi:changed.append(p.name)
 report[key]={'changed_geometry':changed,'unchanged_geometry':len(files)-len(changed),'compared':'positions, normals, indices'+(', colors' if key=='forest' else '; excludes intentional ground colors')}
(root/'Docs/CR016-017/mesh-preservation.json').write_text(json.dumps(report,indent=2));print(json.dumps(report,indent=2))
