$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '../..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$built=(Get-Content 'Builds/Racer-0.107.0-review1-Windows/VERSION.txt' | Select-String 'Source commit: ').Line.Split(': ')[-1].Trim()
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.107.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/106000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.107.0-review1
Red Clay Stunt Park follow-ups: forgiving landings (a nose- or tail-first landing that comes back onto the wheels counts; only a real roof or side landing bails), a trick that takes you out of the park still counts, a reset in the Stunt event puts you back in the park, 5 seconds to chain a combo, finish the trick at the buzzer, no hay bales in the way.
F3: Debug Mode. F4: screenshot and comment. F6 / Start: Debug menu.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/DebugReporting/DEBUG_MODE.md' -Destination $review
Copy-Item -LiteralPath 'Docs/TrailerMode/TRAILER_MODE.md' -Destination $review
New-Item -ItemType Directory -Force -Path 'Temp' | Out-Null
Set-Content -LiteralPath 'Temp/report107-source-commit.txt' -Value $built
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.107.0-review1-Windows --previous-catalog Builds/LauncherRelease-106000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-106000/assets/game-manifest.json --out Builds/LauncherRelease-107000 --build 107000 --version 0.107.0-review1 --notes "Stunt park follow-ups: forgiving landings with a recovery window, tricks that leave the park count, Stunt event resets back into the park, 5 s to chain a combo, finish the trick at the buzzer, hay bales removed. Source commit $built."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
