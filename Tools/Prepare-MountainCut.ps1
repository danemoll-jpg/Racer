$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.62.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/61000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.62.0-review1
Mountain Reverse lower main route restored through a rock cut beneath the supported Summit junction.
Gold marks optional routes; teal follows the main course. Reverse Summit Traverse now merges facing the race direction.
Accepted Forest cave, Backyard courses, global vehicle settings and music preserved.
See Review/VALIDATION.md for targeted checks and gameplay-review limitations.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
Copy-Item -LiteralPath 'Docs/MountainCut/VALIDATION.md' -Destination (Join-Path $runtime 'VALIDATION.md')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
foreach($file in @('VALIDATION.md','release-main885.png','release-junction.png')){Copy-Item -LiteralPath (Join-Path 'Docs/MountainCut' $file) -Destination $review}
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.62.0-review1-Windows --previous-catalog Builds/LauncherRelease-61000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-61000/assets/game-manifest.json --out Builds/LauncherRelease-62000 --build 62000 --version 0.62.0-review1 --notes "Restored Mountain Reverse tunnel clearance, original lower run-up grade, natural upper supports and local sign/gate corrections. Source commit $sourceCommit."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
