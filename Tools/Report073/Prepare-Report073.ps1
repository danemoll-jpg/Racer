$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '../..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$built=(Get-Content 'Builds/Racer-0.73.0-review1-Windows/VERSION.txt' | Select-String 'Source commit: ').Line.Split(': ')[-1].Trim()
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.73.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/72000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.73.0-review1
Snow: every lake, creek and pool freezes. You ride on the ice (it slows you down exactly as the water does; no slipping). Two people go sledding beside the road below the Backyard hill, and three play broom hockey on the frozen pool at Dan's house (Snow only, races and Free Roam).
Rain: thunderstorms - an occasional lightning flash and roll of thunder (muffled in caves and tunnels). Settings > Display > Lightning flashes: Off keeps the thunder without the flash.
Clouds: scattered clouds in Clear, a dark overcast in Rain, a pale overcast in Snow, lit by the sun, the dusk and the moon.
Motorcycle: a new Needle 600 and rider made in Blender. Garage > Model: Classic / New switches it (default New); the AI motorcycles follow it. Handling, colliders and camera are unchanged.
F3: Debug Mode. F4: screenshot and comment. F6 / Start: Debug menu.
See Review/VALIDATION.md for every item and the frame-rate measurements.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
Copy-Item -LiteralPath 'Docs/Report073/VALIDATION.md' -Destination (Join-Path $runtime 'VALIDATION.md')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/Report073/VALIDATION.md' -Destination $review
Copy-Item -LiteralPath 'Docs/DebugReporting/DEBUG_MODE.md' -Destination $review
New-Item -ItemType Directory -Force -Path 'Temp' | Out-Null
Set-Content -LiteralPath 'Temp/report073-source-commit.txt' -Value $built
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.73.0-review1-Windows --previous-catalog Builds/LauncherRelease-72000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-72000/assets/game-manifest.json --out Builds/LauncherRelease-73000 --build 73000 --version 0.73.0-review1 --notes "Snow freezes all water (ride on the ice, same slowdown), sledding and broom-hockey snow scenes; thunder and lightning in Rain (Lightning flashes setting); stylized clouds; Blender-made Needle 600 and rider (garage Model: Classic / New). Source commit $built."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
