$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '../..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$built=(Get-Content 'Builds/Racer-1.2.0-review2-Windows/VERSION.txt' | Select-String 'Source commit: ').Line.Split(': ')[-1].Trim()
$runtime=Join-Path (Get-Location) 'Builds/Racer-1.2.0-review2-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/1020000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
# as 1.0: the player-facing README does not list the debug keys, and Review/ carries only TRAILER_MODE.md (the debug tool stays in the game, hidden)
@'
Woodstock Rush 1.2.0-review2
HIGH MODE: the switch at the top of the main menu. Simple and forgiving: everything unlocked, bumpers along the race courses,
no wrecks (a vehicle on its roof flips back up), its own HIGH records. In split screen each player has their own HIGH MODE row.
FREE ROAM on the main menu asks 1 or 2 players.
Race replays: every race is recorded. On the finish screen, WATCH REPLAY (X / V) and SAVE REPLAY (Y / K).
Settings > Display > Screen mode: Full screen (the default) or Windowed.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/TrailerMode/TRAILER_MODE.md' -Destination $review
New-Item -ItemType Directory -Force -Path 'Temp' | Out-Null
Set-Content -LiteralPath 'Temp/Report1200-source-commit.txt' -Value $built
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-1.2.0-review2-Windows --previous-catalog Builds/LauncherRelease-1020000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-1020000/assets/game-manifest.json --out Builds/LauncherRelease-1020100 --build 1020100 --version 1.2.0-review2 --notes "Woodstock Rush 1.2.0-review2: HIGH MODE (the main menu's switch: fewer menu choices, everything unlocked, bumpers along the race courses, no wrecks, its own HIGH records; in split screen each player chooses their own) and FREE ROAM for 1 or 2 players on the main menu. Source commit $built."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
