import pathlib,re,subprocess,json
root=pathlib.Path.cwd();notes=[]
for path in ['Assets/Scenes/'+x+'.unity' for x in ['StreetLoopGreybox','StreetLoopReverse','LakeWoods','ForestLoopReverse','DansBackyardForward','DansBackyardReverse']]:
 old=subprocess.check_output(['git','show','HEAD:'+path],text=True);new=pathlib.Path(path).read_text()
 def blocks(s):return {m.group(2):(m.group(1),m.group(3)) for m in re.finditer(r'^--- !u!(\d+) &(-?\d+)\n(.*?)(?=^--- !u!|\Z)',s,re.M|re.S)}
 a,b=blocks(old),blocks(new);changed=[(k,a[k][0],a[k][1],b[k][1]) for k in a.keys()&b.keys() if a[k]!=b[k]]
 notes.append(path+': '+str(len(a.keys()-b.keys()))+' removed blocks, '+str(len(b.keys()-a.keys()))+' added, '+str(len(changed))+' modified')
 for k,t,x,y in changed:
  if t=='1660057539':continue
  notes.append('MODIFIED '+t+' '+k+'\n'+''.join(__import__('difflib').unified_diff(x.splitlines(True),y.splitlines(True))))
 for k in b.keys()-a.keys():notes.append('ADDED '+k+' '+b[k][1][:450])
pathlib.Path('Docs/ReportCleanup/non-mountain-scene-diff.txt').write_text('\n'.join(notes));print('\n'.join(notes)[:12000])

