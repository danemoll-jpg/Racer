$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '../..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$built=(Get-Content 'Builds/Racer-0.121.0-review1-Windows/VERSION.txt' | Select-String 'Source commit: ').Line.Split(': ')[-1].Trim()
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.121.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/120000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.121.0-review1
Leviathan Forward: rivals no longer drive in circles at CP 44 (Hwy 92). A gate flown past in the air now counts as missed when you land, so nobody is left looking for it; the Stunt Race kicker that threw riders over CP 44 now stands on Hwy 92 west.
F3: Debug Mode. F4: screenshot and comment. F6 / Start: Debug menu.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/DebugReporting/DEBUG_MODE.md' -Destination $review
Copy-Item -LiteralPath 'Docs/TrailerMode/TRAILER_MODE.md' -Destination $review
New-Item -ItemType Directory -Force -Path 'Temp' | Out-Null
Set-Content -LiteralPath 'Temp/report121-source-commit.txt' -Value $built
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.121.0-review1-Windows --previous-catalog Builds/LauncherRelease-120000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-120000/assets/game-manifest.json --out Builds/LauncherRelease-121000 --build 121000 --version 0.121.0-review1 --notes "Leviathan Forward: rivals no longer circle at CP 44 (Hwy 92); a gate flown past is charged on landing; the Stunt Race kicker before CP 44 moved. Source commit $built."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
