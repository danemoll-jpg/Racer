$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '../..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$built=(Get-Content 'Builds/Racer-0.97.0-review1-Windows/VERSION.txt' | Select-String 'Source commit: ').Line.Split(': ')[-1].Trim()
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.97.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/96000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.97.0-review1
Police Getaway is much harder: cops match your vehicle's top speed and keep up, backups join ahead of you out of sight with a radio call, heat rises every 30 s while chased, call-ins place units at the exits within seconds, escaping takes work (a 30 s unseen meter that does not fill with a cop near), and from heat 4 a police helicopter follows with a spotlight.
Weather changes grip: Rain and Snow make vehicles slide more (ice most of all), AI and cops brake and corner to match. Settings > Gameplay > Weather affects grip (On by default). Rain and Snow races have their own records.
F3: Debug Mode. F4: screenshot and comment. F6 / Start: Debug menu.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/DebugReporting/DEBUG_MODE.md' -Destination $review
Copy-Item -LiteralPath 'Docs/TrailerMode/TRAILER_MODE.md' -Destination $review
New-Item -ItemType Directory -Force -Path 'Temp' | Out-Null
Set-Content -LiteralPath 'Temp/report097-source-commit.txt' -Value $built
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.97.0-review1-Windows --previous-catalog Builds/LauncherRelease-96000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-96000/assets/game-manifest.json --out Builds/LauncherRelease-97000 --build 97000 --version 0.97.0-review1 --notes "Getaway is much harder (cops keep up, backups join ahead, call-ins place units at exits, heat, escape meter, police helicopter); weather changes grip (Rain, Snow, ice; Settings > Gameplay). Source commit $built."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
