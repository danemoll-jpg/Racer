$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.58.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/57000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.58.0-review1
Focused final corrections: audible localized rat squeaks/scurrying and one Forest Forward cave rockfall with a right-side racing passage.
Forward closes the new drain grate and logging gate; free roam opens them.
Gold marks optional routes and dashed gold marks the underground drain beneath existing surface trails.
Accepted main routes, original Forward shortcuts, global vehicle settings and existing probabilistic AI choice, HUD, racing minimap and music preserved.
See Review/VALIDATION.md for targeted checks and gameplay-review limitations.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
Copy-Item -LiteralPath 'Docs/FinalTwo/VALIDATION.md' -Destination (Join-Path $runtime 'VALIDATION.md')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
foreach($file in @('VALIDATION.md','after-cave-180.png','after-cave-192.png')){Copy-Item -LiteralPath (Join-Path 'Docs/FinalTwo' $file) -Destination $review}
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.58.0-review1-Windows --previous-catalog Builds/LauncherRelease-57000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-57000/assets/game-manifest.json --out Builds/LauncherRelease-58000 --build 58000 --version 0.58.0-review1 --notes "Two focused corrections: stronger brief rat event synchronized to the fleeing group, and a natural Forest Forward rockfall with local AI alignment and recovery exclusion. Accepted scenery and global systems preserved. Source commit $sourceCommit."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}






