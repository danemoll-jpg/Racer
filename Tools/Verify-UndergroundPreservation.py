from pathlib import Path
import subprocess,re,json
root=Path(__file__).resolve().parents[1]
baseline='8ab1294278a0708a6e7ab2e4ab5490d5c442eba9'
def old(p):return subprocess.check_output(['git','show',baseline+':'+p],cwd=root).decode().replace('\r\n','\n')
def normalize(s):
 for line in ['  undergroundStart: 0\n','  undergroundEnd: 0\n','  entryHeightBelow: 3\n','  entryHeightAbove: 12\n']:s=s.replace(line,'')
 return s
def blocks(s):return {re.search(r'^--- !u!\d+ &(\d+)',b).group(1):b for b in re.split(r'(?=^--- !u!)',s,flags=re.M) if b.startswith('--- !u!')}
results=[]
for name in ['StreetLoopGreybox','StreetLoopReverse','LakeWoods','DansBackyardForward','MountainLoop','MountainLoopReverse']:
 p='Assets/Scenes/'+name+'.unity';assert old(p)==(root/p).read_text(encoding='utf-8-sig'),name;results.append(name+' unchanged')
for name in ['RaceDirector','RaceProgress','RacerState','RaceRoad','RacingMiniMap','ShortcutStrategy','ArcadeVehicle','RoadDriver','RaceFlow','LocalRadio']:
 p='Assets/Scripts/'+name+'.cs'
 if not (root/p).exists():continue
 assert old(p)==(root/p).read_text(encoding='utf-8-sig'),name;results.append(name+' unchanged')
guids=[re.search(r'guid: (\w+)',(root/('Assets/Scripts/'+n+'.cs.meta')).read_text()).group(1) for n in ['RaceRoad','RaceGate','BackyardForwardCourse','ForestLayout','WoodlandRoute','ReverseShortcutGuidance']]
details={}
for name in ['ForestLoopReverse','DansBackyardReverse']:
 p='Assets/Scenes/'+name+'.unity';a=blocks(old(p));b=blocks((root/p).read_text(encoding='utf-8-sig'))
 protected=[k for k,v in a.items() if any(g in v for g in guids)]
 
 for k in protected:
  if normalize(a[k])!=normalize(b.get(k,"")):
   import difflib
   print(k,''.join(difflib.unified_diff(a[k].splitlines(True),b.get(k,'').splitlines(True))))
 assert all(normalize(a[k])==normalize(b.get(k,"")) for k in protected),name+' original route/gate/flight component changed'
 details[name]=dict(protectedOriginalComponents=len(protected),unchangedBlocks=sum(v==b.get(k) for k,v in a.items()),removed=[re.search(r'  m_Name: (.*)',v).group(1) for k,v in a.items() if k not in b and re.search(r'  m_Name: (.*)',v)])
 results.append(name+' accepted route/gate/flight components unchanged')
out=dict(baseline=baseline,checks=results,scenes=details)
(root/'Docs/UndergroundPolish/preservation.json').write_text(json.dumps(out,indent=2))
print(json.dumps(out,indent=2))




