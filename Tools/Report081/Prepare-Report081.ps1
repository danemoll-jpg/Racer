$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '../..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$built=(Get-Content 'Builds/Racer-0.81.0-review1-Windows/VERSION.txt' | Select-String 'Source commit: ').Line.Split(': ')[-1].Trim()
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.81.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/80000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.81.0-review1
New vehicles: four cars - Sundown Roadster (a 1960s roadster, top down), Highball Fastback (1960s fastback), Pebble Coupe (1970s rear-engined compact), Skyfin Cruiser (1950s finned cruiser) - and two motorcycles - Ridge Scrambler (dirt bike) and Drifter Twin (cruiser). Garage: the Vehicle row steps through all ten (left / right). AI fields use them too.
Traffic: the ambient traffic cars are new Blender models (sedan, wagon, pickup, van) with lit lamps at night and a driver; Model: Classic shows the old ones.
The people in the scripted scenes (coffee at the fence, the two at Kyle's, football, the campsite, the sled and broom hockey in Snow) are three recurring characters with proper faces, hair, hands and clothes.
Fixes from the 0.80 review: the old dark road-name boards are gone (the green street signs replace them); the world no longer ends in empty haze - wooded hills all round, with a steep bank and an invisible wall at the edge; the bump that flipped bikes in Mountain Loop Reverse at the reverse deck is gone.
F3: Debug Mode. F4: screenshot and comment. F6 / Start: Debug menu.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/DebugReporting/DEBUG_MODE.md' -Destination $review
Copy-Item -LiteralPath 'Docs/TrailerMode/TRAILER_MODE.md' -Destination $review
New-Item -ItemType Directory -Force -Path 'Temp' | Out-Null
Set-Content -LiteralPath 'Temp/report081-source-commit.txt' -Value $built
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.81.0-review1-Windows --previous-catalog Builds/LauncherRelease-80000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-80000/assets/game-manifest.json --out Builds/LauncherRelease-81000 --build 81000 --version 0.81.0-review1 --notes "New vehicles (four cars incl. a convertible, two motorcycles), Blender traffic cars, detailed people in the scripted scenes, 0.80 fixes (old road-name boards removed, world edge hills and wall, Mountain Loop Reverse bump). Source commit $built."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
