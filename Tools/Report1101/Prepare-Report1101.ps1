$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '../..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$built=(Get-Content 'Builds/Racer-1.1.0-Windows/VERSION.txt' | Select-String 'Source commit: ').Line.Split(': ')[-1].Trim()
$runtime=Join-Path (Get-Location) 'Builds/Racer-1.1.0-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/1000000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
# as 1.0: the player-facing README does not list the debug keys, and Review/ carries only TRAILER_MODE.md (the debug tool stays in the game, hidden)
@'
Woodstock Rush 1.1.0
Race replays: every race is recorded. On the finish screen, WATCH REPLAY (X / V) and SAVE REPLAY (Y / K).
Watch with TV, chase and free cameras; the #1 lap and race of each Top 10 board are kept; main menu > REPLAYS.
Settings > Display > Screen mode: Full screen (the default, at the display's own resolution) or Windowed.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/TrailerMode/TRAILER_MODE.md' -Destination $review
New-Item -ItemType Directory -Force -Path 'Temp' | Out-Null
Set-Content -LiteralPath 'Temp/report1101-source-commit.txt' -Value $built
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-1.1.0-Windows --previous-catalog Builds/LauncherRelease-1000000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-1000000/assets/game-manifest.json --out Builds/LauncherRelease-1010100 --build 1010100 --version 1.1.0 --notes "Woodstock Rush 1.1.0: race replays (every race recorded, TV / chase / free cameras, the #1 lap and race kept, save any race); the game opens full screen at the display's own resolution, with Settings > Display > Screen mode (Full screen / Windowed), and every menu fits a 1280x800 screen (Steam Deck). Source commit $built."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
