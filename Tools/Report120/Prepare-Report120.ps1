$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '../..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$built=(Get-Content 'Builds/Racer-0.120.0-review1-Windows/VERSION.txt' | Select-String 'Source commit: ').Line.Split(': ')[-1].Trim()
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.120.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/119000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.120.0-review1
Stunt Races and the Stunt park: your total stunt points on screen and a trick panel (the trick as it happens, its multiplier, the combo); every different trick type in one jump raises the multiplier (flip + roll + spin: x3); kickers and bumps on the Stunt Race courses; tricks that take off in the park count wherever they land again; the garage shows the stunt vehicles for stunt events.
F3: Debug Mode. F4: screenshot and comment. F6 / Start: Debug menu.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/DebugReporting/DEBUG_MODE.md' -Destination $review
Copy-Item -LiteralPath 'Docs/TrailerMode/TRAILER_MODE.md' -Destination $review
New-Item -ItemType Directory -Force -Path 'Temp' | Out-Null
Set-Content -LiteralPath 'Temp/report120-source-commit.txt' -Value $built
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.120.0-review1-Windows --previous-catalog Builds/LauncherRelease-119000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-119000/assets/game-manifest.json --out Builds/LauncherRelease-120000 --build 120000 --version 0.120.0-review1 --notes "Stunt Races and the Stunt park after Dan's first drive of 0.119: points on screen, a multiplier per trick type, bumps on the stunt courses, tricks out of the park count, the garage shows stunt vehicles. Source commit $built."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
