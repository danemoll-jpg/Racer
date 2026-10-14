$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '../..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$built=(Get-Content 'Builds/Racer-0.114.0-review1-Windows/VERSION.txt' | Select-String 'Source commit: ').Line.Split(': ')[-1].Trim()
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.114.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/113000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.114.0-review1
The new Trickum Rd and S Cherokee Ln buildings are whole from outside (their walls were drawn inside out), and two proposals for a combined Street Loop + Mountain course are marked in Free Roam (Settings > Display > Show track proposals).
F3: Debug Mode. F4: screenshot and comment. F6 / Start: Debug menu.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/DebugReporting/DEBUG_MODE.md' -Destination $review
Copy-Item -LiteralPath 'Docs/TrailerMode/TRAILER_MODE.md' -Destination $review
New-Item -ItemType Directory -Force -Path 'Temp' | Out-Null
Set-Content -LiteralPath 'Temp/report114-source-commit.txt' -Value $built
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.114.0-review1-Windows --previous-catalog Builds/LauncherRelease-113000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-113000/assets/game-manifest.json --out Builds/LauncherRelease-114000 --build 114000 --version 0.114.0-review1 --notes "Trickum Rd and S Cherokee Ln buildings whole from outside; combined-course proposals A and B marked in Free Roam. Source commit $built."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
