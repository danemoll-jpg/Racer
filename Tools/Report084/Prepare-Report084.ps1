$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '../..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$built=(Get-Content 'Builds/Racer-0.84.0-review1-Windows/VERSION.txt' | Select-String 'Source commit: ').Line.Split(': ')[-1].Trim()
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.84.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/83000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.84.0-review1
Forest Loop Reverse: McFadden Cut is now the main route; Granite Saddle with the pool-and-lake jump is the optional line (new records ID).
Floating trees on Forest Loop Reverse grounded; the stray trunk on the Dan's Backyard trail removed.
Vehicles no longer creep with no throttle (and resting triggers no longer count as throttle).
The Forest jump and speed traps now live in Free Roam: Pool-house jump, S Cherokee hill speed trap, Hwy 92 east speed trap.
Fences and cave rock get the new scenery look; Dan's Backyard AI keep to the gates.
START RACE responds at once (the race is built behind the loading screen); title voice safeguards.
Sundown Roadster windscreen raised so first person sees the road; Woodstock Rush window icon and title.
F3: Debug Mode. F4: screenshot and comment. F6 / Start: Debug menu.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/DebugReporting/DEBUG_MODE.md' -Destination $review
Copy-Item -LiteralPath 'Docs/TrailerMode/TRAILER_MODE.md' -Destination $review
New-Item -ItemType Directory -Force -Path 'Temp' | Out-Null
Set-Content -LiteralPath 'Temp/report084-source-commit.txt' -Value $built
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.84.0-review1-Windows --previous-catalog Builds/LauncherRelease-83000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-83000/assets/game-manifest.json --out Builds/LauncherRelease-84000 --build 84000 --version 0.84.0-review1 --notes "Forest Loop Reverse main is McFadden Cut, Forest activities in Free Roam, no idle creep, fences and cave rock, floating trees, Backyard AI gates, Start Race response, title voice, roadster windscreen, window icon. Source commit $built."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
