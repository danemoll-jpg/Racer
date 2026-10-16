$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '../..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$built=(Get-Content 'Builds/Racer-1.0.0-Windows/VERSION.txt' | Select-String 'Source commit: ').Line.Split(': ')[-1].Trim()
$runtime=Join-Path (Get-Location) 'Builds/Racer-1.0.0-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/121000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
# 1.0: the player-facing README no longer lists the debug keys, and Review/ no longer carries DEBUG_MODE.md (the tool stays in the game, hidden)
@'
Woodstock Rush 1.0.0
The finished game: the campaign and its five chapters, quick races on every course and direction, Stunt Races, split screen,
Free Roam with its activities, the Police Chase and the Stunt Event.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/TrailerMode/TRAILER_MODE.md' -Destination $review
New-Item -ItemType Directory -Force -Path 'Temp' | Out-Null
Set-Content -LiteralPath 'Temp/report1000-source-commit.txt' -Value $built
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-1.0.0-Windows --previous-catalog Builds/LauncherRelease-121000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-121000/assets/game-manifest.json --out Builds/LauncherRelease-1000000 --build 1000000 --version 1.0.0 --notes "Woodstock Rush 1.0: the finished game. Source commit $built."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
