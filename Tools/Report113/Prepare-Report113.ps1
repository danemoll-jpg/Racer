$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '../..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$built=(Get-Content 'Builds/Racer-0.113.0-review1-Windows/VERSION.txt' | Select-String 'Source commit: ').Line.Split(': ')[-1].Trim()
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.113.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/112000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.113.0-review1
Rivals no longer get stuck on the Laurel Switchbacks shortcut, Retry starts a race exactly like a fresh start, Trickum Rd and the west end of S Cherokee Ln look like the real roads today, rivals take both Mountain Loop Forward shortcuts, and rivals, traffic and police cars have engine sounds.
F3: Debug Mode. F4: screenshot and comment. F6 / Start: Debug menu.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/DebugReporting/DEBUG_MODE.md' -Destination $review
Copy-Item -LiteralPath 'Docs/TrailerMode/TRAILER_MODE.md' -Destination $review
New-Item -ItemType Directory -Force -Path 'Temp' | Out-Null
Set-Content -LiteralPath 'Temp/report113-source-commit.txt' -Value $built
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.113.0-review1-Windows --previous-catalog Builds/LauncherRelease-112000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-112000/assets/game-manifest.json --out Builds/LauncherRelease-113000 --build 113000 --version 0.113.0-review1 --notes "Laurel Switchbacks rivals' line, Retry like a fresh start, Trickum Rd and S Cherokee Ln roadside like today, rivals on the Mountain Forward shortcuts, other vehicles' engine sounds. Source commit $built."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
