from pathlib import Path
root=Path(__file__).resolve().parents[1]
for name in ['Build-TrackUi.cs','Stage-TrackUi.py','Verify-TrackUiPublication.py','Activate-TrackUi.ps1','Verify-TrackUiPlayRacer.ps1','Cleanup-TrackUi.ps1','Prepare-TrackUi.ps1']:
 s=(root/'Tools'/name).read_text(encoding='utf-8-sig')
 for a,b in [('41000','__NEXT__'),('40000','41000'),('39000','40000'),('__NEXT__','42000'),('0.41.0-review1','0.42.0-review1'),('TrackUi','BackyardShortcuts'),('trackui-','shortcuts-'),('Approved course map; track selector and post-finish UI','Two optional Backyard forest shortcuts and updated visual map')]:s=s.replace(a,b)
 if name=='Prepare-TrackUi.ps1':
  start=s.index('@\'');end=s.index("'@",start)
  s=s[:start]+"@'\nWoodstock Rush 0.42.0-review1\nDan's Backyard Loop - Forward: optional Tree-Top Trail and Abandoned Cabin Jump.\nThe approved main course is preserved. Both routes use existing gold minimap overlays, checkpoint entitlement and variable AI strategy.\nReview/TRACK_MAP.png shows the actual implemented geometry. Reverse and full-world/menu-map upgrades remain deferred.\nPlay-Racer.cmd uses the existing signed launcher/updater.\n"+s[end:]
  line="foreach($file in @('track-selector.png','waiting.png','results.png','VALIDATION.md')){Copy-Item -LiteralPath (Join-Path 'Docs/BackyardShortcuts' $file) -Destination $review}"
  s=s.replace(line,"foreach($file in @('tree-top.png','cabin.png','VALIDATION.md')){Copy-Item -LiteralPath (Join-Path 'Docs/BackyardShortcuts' $file) -Destination $review}")
  n=s.index('--notes ');end=s.index('\n',n)
  s=s[:n]+'--notes "Two optional Backyard Forward forest shortcuts: Tree-Top Trail and Abandoned Cabin Jump; protected main course; existing checkpoint/minimap/AI integration; updated visual map. Source commit $sourceCommit."'+s[end:]
 (root/'Tools'/name.replace('TrackUi','BackyardShortcuts')).write_text(s,encoding='utf-8')
print('Prepared existing release workflow for game-42000; no publication performed')
