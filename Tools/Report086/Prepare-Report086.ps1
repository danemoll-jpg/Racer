$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '../..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$built=(Get-Content 'Builds/Racer-0.86.0-review1-Windows/VERSION.txt' | Select-String 'Source commit: ').Line.Split(': ')[-1].Trim()
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.86.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/85000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.86.0-review1
The House 3 swimming pool is a real in-ground pool: the coping is flush with the lawn, and the shallow end slopes out of the water.
The game icon is the ATV.
Forest Loop Reverse: the outside of the bend at the foot of the Granite Saddle climb no longer falls away, so vehicles keep their speed to the jump.
F3: Debug Mode. F4: screenshot and comment. F6 / Start: Debug menu.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/DebugReporting/DEBUG_MODE.md' -Destination $review
Copy-Item -LiteralPath 'Docs/TrailerMode/TRAILER_MODE.md' -Destination $review
New-Item -ItemType Directory -Force -Path 'Temp' | Out-Null
Set-Content -LiteralPath 'Temp/report086-source-commit.txt' -Value $built
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.86.0-review1-Windows --previous-catalog Builds/LauncherRelease-85000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-85000/assets/game-manifest.json --out Builds/LauncherRelease-86000 --build 86000 --version 0.86.0-review1 --notes "In-ground House 3 pool with a shallow-end slope, the ATV game icon, Forest Reverse Granite Saddle approach kept fast. Source commit $built."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
