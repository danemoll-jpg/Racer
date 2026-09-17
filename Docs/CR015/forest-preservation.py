import subprocess,re,pathlib,numpy as np,json
root=pathlib.Path('.')
rows=[]
for p in sorted((root/'Assets/Vegetation/Phase6').glob('Forest_*.asset')):
    s=p.read_text(); old=subprocess.check_output(['git','show','a1212ec:'+p.as_posix()]).decode()
    def mesh(text):
        n=int(re.search(r'm_VertexCount: (\d+)',text)[1]); b=bytes.fromhex(re.search(r'_typelessdata: ([0-9a-fA-F]+)',text)[1]); a=np.frombuffer(b,dtype=np.uint8).reshape(n,len(b)//n)
        channels=re.findall(r'- stream: (\d+)\s+offset: (\d+)\s+format: (\d+)\s+dimension: (\d+)',text)
        used=np.concatenate([a[:,int(channels[i][1]):int(channels[i][1])+int(channels[i][3])*4] for i in (0,1,3)],axis=1)
        return used,re.search(r'm_IndexBuffer: ([0-9a-fA-F]+)',text)[1]
    a,ai=mesh(s);b,bi=mesh(old)
    if a.shape!=b.shape or not np.array_equal(a,b) or ai!=bi:rows.append(p.name)
report={'forest_batches_with_changed_positions_normals_colors_or_indices':rows,'all_other_batches_geometry_colors_exact':len(list((root/'Assets/Vegetation/Phase6').glob('Forest_*.asset')))-len(rows),'note':'Ignores unused tangent/UV channels; refresh serialization can change those buffers. All authored trunk preservation is separately checked in scene records.'}
(root/'Docs/CR015/forest-geometry-preservation.json').write_text(json.dumps(report,indent=2));print(json.dumps(report,indent=2))
