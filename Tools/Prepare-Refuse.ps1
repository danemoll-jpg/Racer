$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify release source commit'}
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.40.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/39000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@"
Woodstock Rush 0.40.0-review1 - Local garbage dump correction

Dan has accepted the Forward course except for the dump. This release changes only that dump: 2,100 overlapping pieces of refuse across the accepted bowl and local grounded resistance that slows escape without solid debris obstacles. Successful jumps retain the accepted launch and landing. Falling in costs driving time without an automatic reset or artificial penalty.

Play-Racer.cmd uses the existing signed launcher/updater. Reverse and optional shortcuts remain deferred. Review contains the local dump views and validation.
"@ | Set-Content (Join-Path $runtime 'README.txt')
Copy-Item -LiteralPath 'Docs/DumpRefuse/VALIDATION.md' -Destination (Join-Path $runtime 'VALIDATION.md')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
foreach($file in @('approach.png','bowl.png','floor.png','VALIDATION.md')){Copy-Item -LiteralPath (Join-Path 'Docs/DumpRefuse' $file) -Destination (Join-Path $review $file)}
& $python Tools/Prepare-ComponentUpdate.py game `
 --source Builds/Racer-0.40.0-review1-Windows `
 --previous-catalog Builds/LauncherRelease-39000/assets/update-catalog.json `
 --previous-manifest Builds/LauncherRelease-39000/assets/game-manifest.json `
 --out Builds/LauncherRelease-40000 --build 40000 --version 0.40.0-review1 `
 --notes "Local garbage dump correction: dense accumulated trash and forgiving grounded traversal resistance. Exact accepted launch/landing and all other Forward route features preserved. No new reset or artificial penalty. Source commit $sourceCommit."
if($LASTEXITCODE -ne 0){throw "Publisher preparation failed"}
