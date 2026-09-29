from pathlib import Path
root=Path(__file__).resolve().parents[1]
for name in ['Build-Refuse.cs','Stage-Refuse.py','Verify-RefusePublication.py','Activate-Refuse.ps1','Verify-RefusePlayRacer.ps1','Cleanup-Refuse.ps1']:
 s=(root/'Tools'/name).read_text()
 for a,b in [('40000','__NEXT__'),('39000','40000'),('38000','39000'),('__NEXT__','41000'),('0.40.0-review1','0.41.0-review1'),('DumpRefuse','TrackUi'),('refuse-','trackui-'),('Local drivable garbage-dump bowl; accepted Forward course preserved','Approved course map; track selector and post-finish UI')]:s=s.replace(a,b)
 (root/'Tools'/name.replace('Refuse','TrackUi')).write_text(s)
print('Prepared existing publisher/build/verification wrappers for game-41000')
