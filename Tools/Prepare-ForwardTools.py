from pathlib import Path
root=Path(__file__).resolve().parents[1]
for filename in ['Build-YardReset.cs','Prepare-YardReset.ps1','Stage-YardReset.py','Verify-YardResetPublication.py','Activate-YardReset.ps1','Verify-YardResetPlayRacer.ps1','Cleanup-YardReset.ps1']:
    s=(root/'Tools'/filename).read_text()
    for a,b in [('37000','__NEW__'),('36000','37000'),('35000','36000'),('__NEW__','38000'),('0.37.0-review1','0.38.0-review1'),('YardReset','Forward'),('yard-reset','forward')]:s=s.replace(a,b)
    s=s.replace('Docs/Forward','Docs/BackyardForward')
    if filename=='Build-YardReset.cs':
        s=s.replace('"Assets/Scenes/MountainLoopReverse.unity"','"Assets/Scenes/MountainLoopReverse.unity","Assets/Scenes/DansBackyardForward.unity"')
        s=s.replace('Rejected Backyard rollback; supported Dan driveway and flat parking; eight anchor markers','Forward only: terrain trail, corrected dump, one ravine with two jumps; accepted property preserved')
    if filename=='Prepare-YardReset.ps1':
        s=s[:s.index(' --notes ')]+' --notes "Dan\'s Backyard Forward: nine hard anchors, corrected dump launch, physical dump depression, one substantial ravine with two crossings, narrow wooded terrain trail and additional jumps. Accepted property and roads preserved. Reverse and optional shortcuts deferred. Source commit $sourceCommit."\nif($LASTEXITCODE -ne 0){throw "Publisher preparation failed"}\n'
    (root/'Tools'/filename.replace('YardReset','Forward')).write_text(s)
print('Prepared 38000 wrappers around existing signed publisher and launcher.')
