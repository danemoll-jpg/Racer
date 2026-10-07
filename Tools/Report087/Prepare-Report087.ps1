$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '../..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$built=(Get-Content 'Builds/Racer-0.87.0-review1-Windows/VERSION.txt' | Select-String 'Source commit: ').Line.Split(': ')[-1].Trim()
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.87.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/86000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.87.0-review1
Kyle's house is rebuilt from Dan's photo: the long ranch with its front gable, porch, chimney and screened porch, and the walk-out lower level with two garage doors and a back deck. The driveway now leads to the garage.
Forest Loop Forward has a second shortcut, the Summit Climb: a narrow wooded climb with two kinks, a rough middle and a blind crest, rejoining just before the finish.
The launcher shows the ATV icon.
F3: Debug Mode. F4: screenshot and comment. F6 / Start: Debug menu.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/DebugReporting/DEBUG_MODE.md' -Destination $review
Copy-Item -LiteralPath 'Docs/TrailerMode/TRAILER_MODE.md' -Destination $review
New-Item -ItemType Directory -Force -Path 'Temp' | Out-Null
Set-Content -LiteralPath 'Temp/report087-source-commit.txt' -Value $built
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.87.0-review1-Windows --previous-catalog Builds/LauncherRelease-86000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-86000/assets/game-manifest.json --out Builds/LauncherRelease-87000 --build 87000 --version 0.87.0-review1 --notes "Kyle's house from Dan's photo, Forest Loop Forward Summit Climb shortcut, launcher ATV icon. Source commit $built."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
