$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '../..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$built=(Get-Content 'Builds/Racer-0.85.0-review1-Windows/VERSION.txt' | Select-String 'Source commit: ').Line.Split(': ')[-1].Trim()
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.85.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/84000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.85.0-review1
The House 3 swimming pool has a beach entry at its north end and a paved apron: every vehicle can drive out of it.
Hands hold the steering wheel and turn with it in the cars; bike and ATV hands follow the bars.
Mountain Loop Forward: running wide just after the start no longer throws the bike off the road edge.
Forest Loop Reverse: no more trees standing on the Granite Saddle and Fern Gully shortcut trails.
F3: Debug Mode. F4: screenshot and comment. F6 / Start: Debug menu.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/DebugReporting/DEBUG_MODE.md' -Destination $review
Copy-Item -LiteralPath 'Docs/TrailerMode/TRAILER_MODE.md' -Destination $review
New-Item -ItemType Directory -Force -Path 'Temp' | Out-Null
Set-Content -LiteralPath 'Temp/report085-source-commit.txt' -Value $built
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.85.0-review1-Windows --previous-catalog Builds/LauncherRelease-84000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-84000/assets/game-manifest.json --out Builds/LauncherRelease-85000 --build 85000 --version 0.85.0-review1 --notes "House 3 pool beach entry, hands on the steering wheel, Mountain Forward road edge, trees off the Forest Reverse shortcuts. Source commit $built."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
