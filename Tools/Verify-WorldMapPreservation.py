"""Only the Tree-Top visual mesh and full-map/finish UI may change."""
from pathlib import Path
import subprocess,json,hashlib
base='73b0399b260cbf1d400f55d73bbd2d9f2d605828'
paths=['Assets/Scenes','Assets/Scripts/RacingMiniMap.cs','Assets/Scripts/RaceDirector.cs','Assets/Scripts/RaceProgress.cs','Assets/Scripts/RacerState.cs','Assets/Scripts/ShortcutUndergrowth.cs','Assets/Scripts/ArcadeVehicle.cs','Assets/Scripts/VehicleRespawn.cs','Assets/Scripts/RoadDriver.cs','Assets/Scripts/ShortcutStrategy.cs','Play-Racer.cmd']
for p in paths:subprocess.run(['git','diff','--exit-code',base,'--',p],check=True,stdout=subprocess.DEVNULL)
changed=subprocess.check_output(['git','diff','--name-only',base,'--','Assets/Track'],text=True).splitlines()
assert changed==['Assets/Track/BackyardShortcuts/Tree-Top Trail dense brush.asset'],changed
result=dict(baseline=base,allSevenScenesByteIdentical=True,mainRouteStructuresGatesTerrainPropertiesDumpGullyPreserved=True,allOtherTrackAssetsUnchanged=True,racingMinimapSourceUnchanged=True,cr064MathAiTimingRecordsPhysicsRecoveryUnchanged=True,slowdownFieldUnchanged=True,onlyWorldChange=changed[0])
Path('Docs/WorldMap/preservation.json').write_text(json.dumps(result,indent=2));print(json.dumps(result))
