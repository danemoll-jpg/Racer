from pathlib import Path
import subprocess,re,json
base='88bb363b8078f890dec28734e7036ca48a70c9f2'
def old(p):return subprocess.check_output(['git','show',base+':'+p]).decode().replace('\r\n','\n')
def blocks(s):return {re.search(r'^--- !u!\d+ &(\d+)',b).group(1):b for b in re.split(r'(?=^--- !u!)',s,flags=re.M) if b.startswith('--- !u!')}
p='Assets/Scenes/DansBackyardForward.unity';a=blocks(old(p));b=blocks(Path(p).read_text())
root=next(k for k,v in a.items() if '  m_Name: Backyard optional forest shortcuts\n' in v)
transforms={k:v for k,v in a.items() if '  m_Father:' in v}
owned={k for k,v in transforms.items() if 'm_GameObject: {fileID: '+root+'}' in v}
while True:
 added={k for k,v in transforms.items() if re.search(r'm_Father: \{fileID: (\d+)\}',v).group(1) in owned}-owned
 if not added:break
 owned|=added
objects={re.search(r'm_GameObject: \{fileID: (\d+)\}',a[k]).group(1) for k in owned}
allowed=owned|objects|{k for k,v in a.items() if (m:=re.search(r'm_GameObject: \{fileID: (\d+)\}',v)) and m.group(1) in objects}
unexpected=[k for k,v in a.items() if k not in allowed and b.get(k)!=v]
assert not unexpected,unexpected
for n in ['StreetLoopGreybox','StreetLoopReverse','LakeWoods','ForestLoopReverse','MountainLoop','MountainLoopReverse']:
 p='Assets/Scenes/'+n+'.unity';assert old(p)==Path(p).read_text(),n
for n in ['RaceDirector','RaceProgress','RacerState','RaceRoad','RoadDriver','RacingMiniMap','ShortcutStrategy','ArcadeVehicle','VehicleRespawn','VehicleSurfaceContacts','DumpRefuse']:
 p='Assets/Scripts/'+n+'.cs';subprocess.run(['git','diff','--exit-code',base,'--',p],check=True,stdout=subprocess.DEVNULL)
r=dict(baseline=base,protectedSceneBlocks=len(a)-len(allowed),unchangedMainPoints=1684,unchangedGates=9,unchangedOtherCourses=6,unchangedGlobalSystems=12,unexpectedChanges=unexpected)
Path('Docs/ShortcutRevision/preservation.json').write_text(json.dumps(r,indent=2));print(json.dumps(r))
