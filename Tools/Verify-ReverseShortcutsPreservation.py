from pathlib import Path
import subprocess,re,json
root=Path(__file__).resolve().parents[1]
baseline='771bb757ae1fc8383cd9b73a1b964c71ca3b4ca5'
def old(p):return subprocess.check_output(['git','show',baseline+':'+p],cwd=root).decode().replace('\r\n','\n')
def blocks(s):return {re.search(r'^--- !u!\d+ &(\d+)',b).group(1):b for b in re.split(r'(?=^--- !u!)',s,flags=re.M) if b.startswith('--- !u!')}
results=[]
for name in ['StreetLoopGreybox','StreetLoopReverse','LakeWoods','ForestLoopReverse','MountainLoop','MountainLoopReverse']:
 p='Assets/Scenes/'+name+'.unity';assert old(p)==(root/p).read_text(encoding='utf-8-sig'),name;results.append(name+' unchanged')
for name in ['RaceDirector','RaceProgress','RacerState','RaceRoad','RoadDriver','RacingMiniMap','ShortcutStrategy','ArcadeVehicle','VehicleRespawn','RaceFlow','LocalRadio']:
 p='Assets/Scripts/'+name+'.cs'
 if not (root/p).exists():continue
 assert old(p)==(root/p).read_text(encoding='utf-8-sig'),name;results.append(name+' unchanged')
guids=[re.search(r'guid: (\w+)',(root/('Assets/Scripts/'+n+'.cs.meta')).read_text()).group(1) for n in ['RaceRoad','RaceGate','BackyardForwardCourse','ForestLayout']]
details={}
for name in ['DansBackyardForward','DansBackyardReverse']:
 p='Assets/Scenes/'+name+'.unity';a=blocks(old(p));b=blocks((root/p).read_text(encoding='utf-8-sig'))
 protected=[k for k,v in a.items() if any(g in v for g in guids)]
 assert all(a[k]==b.get(k) for k in protected),name+' original route/gate/flight component changed'
 details[name]=dict(protectedOriginalComponents=len(protected),unchangedBlocks=sum(v==b.get(k) for k,v in a.items()),removed=[re.search(r'  m_Name: (.*)',v).group(1) for k,v in a.items() if k not in b and re.search(r'  m_Name: (.*)',v)])
 results.append(name+' accepted route/gate/flight components unchanged')
out=dict(baseline=baseline,checks=results,scenes=details)
(root/'Docs/ReverseShortcuts/preservation.json').write_text(json.dumps(out,indent=2))
print(json.dumps(out,indent=2))
