$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '../..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$built=(Get-Content 'Builds/Racer-0.102.0-review1-Windows/VERSION.txt' | Select-String 'Source commit: ').Line.Split(': ')[-1].Trim()
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.102.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/101000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.102.0-review1
Dan's Backyard Forward: the bump before CP 1 smoothed out (the yard-edge drop, the dip and the crest made one gentle swell, so you stay on the trail at full speed); the Tree-Top Trail dirt jump has sloped earth sides, so its far left and right no longer catch; the storm drain beside the trail is solid from outside and its grate is one solid barrier (closed on Forward); every reset in the Abandoned Cabin brush puts you past it; the Cabin boards have earth shoulders at their foot, so an angled entry keeps its speed (the same jump pieces and brush reset in Free Roam).
F3: Debug Mode. F4: screenshot and comment. F6 / Start: Debug menu.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/DebugReporting/DEBUG_MODE.md' -Destination $review
Copy-Item -LiteralPath 'Docs/TrailerMode/TRAILER_MODE.md' -Destination $review
New-Item -ItemType Directory -Force -Path 'Temp' | Out-Null
Set-Content -LiteralPath 'Temp/report102-source-commit.txt' -Value $built
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.102.0-review1-Windows --previous-catalog Builds/LauncherRelease-101000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-101000/assets/game-manifest.json --out Builds/LauncherRelease-102000 --build 102000 --version 0.102.0-review1 --notes "Dan's Backyard Forward: bump before CP 1 smoothed, Tree-Top jump edges no longer catch, storm drain side and grate solid (closed on Forward), every Cabin brush reset goes past the brush, Cabin board transition keeps speed (jump pieces and brush reset also in Free Roam). Source commit $built."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
