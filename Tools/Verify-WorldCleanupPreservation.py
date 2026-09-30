import re,subprocess,json
from pathlib import Path
r=Path(__file__).resolve().parents[1];base='132bc5719436a5cebdcc76c3c16d1816e088ee13';report=[]
def blocks(s):return {re.search(r'^--- !u!\d+ &(\d+)',b)[1]:b for b in re.split(r'(?=^--- !u!)',s,flags=re.M) if b.startswith('--- !u!')}
def guid(name):return re.search(r'guid: (\w+)',(r/f'Assets/Scripts/{name}.cs.meta').read_text())[1]
allowed={guid(n) for n in ('RaceDirector','ExplorationMap','ExplorationCollection')}
for p in (r/'Assets/Scenes').glob('*.unity'):
 rel=p.relative_to(r).as_posix();old=subprocess.check_output(['git','show',base+':'+rel],cwd=r).decode();new=p.read_text();a=blocks(old);b=blocks(new)
 roots={k for k,v in a.items() if 'm_Name: TEMPORARY Backyard anchor validation - no route\n' in v};removed=set(roots)
 # Follow the serialized GameObject/Transform hierarchy to permit only the temporary marker subtree.
 changed=True
 while changed:
  changed=False
  for k,v in a.items():
   go=re.search(r'm_GameObject: \{fileID: (\d+)\}',v);parent=re.search(r'm_Father: \{fileID: (\d+)\}',v)
   if k not in removed and ((go and go[1] in removed) or (parent and parent[1] in removed)):
    removed.add(k);changed=True
    if go:removed.add(go[1])
 bad=[];counts={'unchanged':0,'removed':0,'modified':0,'added':len(b.keys()-a.keys())}
 for k,v in a.items():
  if k not in b:
   counts['removed']+=1
   if k not in removed:bad.append('unexpected removal '+k)
  elif v==b[k]:counts['unchanged']+=1
  else:
   counts['modified']+=1;sg=re.search(r'm_Script: .*guid: (\w+)',v)
   permitted=(sg and sg[1] in allowed) or (v.startswith('--- !u!102 ') and "KYLE''S WOODED DRIVE" in v) or v.startswith('--- !u!1660057539 ')
   if not permitted:bad.append('unexpected modification '+k+' '+v[:90])
 report.append({'scene':p.stem,**counts,'unexpected':bad});assert not bad,(p.name,bad[:6])
protected=['Assets/Scripts/RacingMiniMap.cs','Assets/Scripts/RaceFlow.cs','Assets/Scripts/ArcadeVehicle.cs','Assets/Scripts/RoadDriver.cs','Assets/Scripts/RaceProgress.cs','Assets/Scripts/RaceRoad.cs','Assets/Scripts/WoodlandRoute.cs','Assets/Scripts/ShortcutStrategy.cs','Assets/Scripts/BackyardForwardCourse.cs','Assets/Scripts/VehicleRespawn.cs']
for p in protected:
 if (r/p).exists():subprocess.run(['git','diff','--exit-code',base,'--',p],cwd=r,check=True,stdout=subprocess.DEVNULL)
changed=subprocess.check_output(['git','diff','--name-only',base],cwd=r,text=True).splitlines();assert not any(p.startswith('Assets/Track/') for p in changed),'Protected world geometry assets changed'
(r/'Docs/WorldCleanup/preservation.json').write_text(json.dumps(report,indent=2));print('PASS: existing scene blocks preserved except requested marker/UI/landmark changes; all track assets, minimap, flow, physics, AI and recovery preserved.')
