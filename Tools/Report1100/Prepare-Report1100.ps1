$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '../..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$built=(Get-Content 'Builds/Racer-1.1.0-review1-Windows/VERSION.txt' | Select-String 'Source commit: ').Line.Split(': ')[-1].Trim()
$runtime=Join-Path (Get-Location) 'Builds/Racer-1.1.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/1000000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
# as 1.0: the player-facing README does not list the debug keys, and Review/ carries only TRAILER_MODE.md (the debug tool stays in the game, hidden)
@'
Woodstock Rush 1.1.0-review1
Race replays: every race is recorded. On the finish screen, WATCH REPLAY (X / V) and SAVE REPLAY (Y / K).
Watch with TV, chase and free cameras; the #1 lap and race of each Top 10 board are kept; main menu > REPLAYS.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/TrailerMode/TRAILER_MODE.md' -Destination $review
New-Item -ItemType Directory -Force -Path 'Temp' | Out-Null
Set-Content -LiteralPath 'Temp/report1100-source-commit.txt' -Value $built
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-1.1.0-review1-Windows --previous-catalog Builds/LauncherRelease-1000000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-1000000/assets/game-manifest.json --out Builds/LauncherRelease-1010000 --build 1010000 --version 1.1.0-review1 --notes "Woodstock Rush 1.1.0-review1: race replays (record every race, TV / chase / free cameras, the #1 lap and race kept, save any race). Source commit $built."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
