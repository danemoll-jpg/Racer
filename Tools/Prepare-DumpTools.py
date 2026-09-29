from pathlib import Path
root=Path(__file__).resolve().parents[1]
for filename in ['Build-Forward.cs','Prepare-Forward.ps1','Stage-Forward.py','Verify-ForwardPublication.py','Activate-Forward.ps1','Verify-ForwardPlayRacer.ps1','Cleanup-Forward.ps1','Check-ForwardRuntime.ps1']:
    s=(root/'Tools'/filename).read_text()
    for a,b in [('38000','__NEW__'),('37000','38000'),('36000','37000'),('__NEW__','39000'),('0.38.0-review1','0.39.0-review1'),('BackyardForward','DumpCorrection'),('Forward','Dump'),('forward','dump')]:s=s.replace(a,b)
    s=s.replace('Assets/Scenes/DansDumpCorrection.unity','Assets/Scenes/DansBackyardForward.unity')
    s=s.replace('Dump only: terrain trail, corrected dump, one ravine with two jumps; accepted property preserved','Local drivable garbage-dump bowl; accepted Forward course preserved')
    if filename=='Prepare-Forward.ps1':
        start=s.index('@"');end=s.index('"@',start)+2
        s=s[:start]+'''@"
Woodstock Rush 0.39.0-review1 - Local garbage dump correction

Dan has accepted the Forward course except for the dump. This release changes only that dump: a deep drivable bowl with a lower floor, escapable slopes and irregular scattered junk. Successful jumps retain the accepted launch and landing. Falling in costs driving time without an automatic reset or artificial penalty.

Play-Racer.cmd uses the existing signed launcher/updater. Reverse and optional shortcuts remain deferred. Review contains the local dump views and validation.
"@'''+s[end:]
        start=s.index('foreach($file in @(',s.index('$review='));end=s.index('\n& $python',start)
        s=s[:start]+'''foreach($file in @('approach.png','bowl.png','floor.png','after-heights.csv','VALIDATION.md')){Copy-Item -LiteralPath (Join-Path 'Docs/DumpCorrection' $file) -Destination (Join-Path $review $file)}'''+s[end:]
        start=s.index(' --notes ')
        s=s[:start]+''' --notes "Local garbage dump correction: deep supported bowl, sloped escape and irregular scattered junk. Exact accepted launch/landing and all other Forward route features preserved. No new reset or artificial penalty. Source commit $sourceCommit."
if($LASTEXITCODE -ne 0){throw "Publisher preparation failed"}
'''
    if filename=='Cleanup-Forward.ps1':
        start=s.index('$targets+=');end=s.index('$running=',start);s=s[:start]+s[end:]
    if filename=='Check-ForwardRuntime.ps1':s=s.replace('-backyardDumpCheck','-dumpCorrectionCheck')
    (root/'Tools'/filename.replace('Forward','Dump')).write_text(s)
print('Prepared release 39000 wrappers using the existing publisher and launcher.')
