from pathlib import Path
root=Path(__file__).resolve().parents[1]
for filename in ['Build-Backyard.cs','Prepare-Backyard.ps1','Stage-Backyard.py','Verify-BackyardPublication.py','Activate-Backyard.ps1','Verify-BackyardPlayRacer.ps1','Cleanup-Backyard.ps1']:
    s=(root/'Tools'/filename).read_text()
    for a,b in [('36000','__NEW__'),('35000','36000'),('34000','35000'),('__NEW__','37000'),('0.36.0-review1','0.37.0-review1'),('Backyard','YardReset'),('backyard','yard-reset')]:s=s.replace(a,b)
    if filename=='Build-Backyard.cs':
        s=s.replace(',"Assets/Scenes/DansYardReset.unity","Assets/Scenes/DansYardResetReverse.unity"','')
        s=s.replace("Dan's YardReset Forward and Reverse; local Kyle driveway transition repair","Rejected Backyard rollback; supported Dan driveway and flat parking; eight anchor markers")
    if filename=='Prepare-Backyard.ps1':
        s=s[:s.index(' --notes ')]+' --notes "Rejected Backyard course removed; forest restored; Kyle fix preserved; Dan driveway, flat parking, fence support and mailbox corrected; eight non-colliding anchor validation markers. No new forest trail. Source commit $sourceCommit."\nif($LASTEXITCODE -ne 0){throw "Publisher preparation failed"}\n'
    target=filename.replace('Backyard','YardReset')
    (root/'Tools'/target).write_text(s)
print('Prepared version 37000 delivery wrappers using the existing signed publisher workflow.')
