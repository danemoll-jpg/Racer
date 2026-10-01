$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.59.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/58000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.59.0-review1
Forest Forward cave hillside integration and lower, readable rockfall with the challenging right-side passage retained.
Forward closes the new drain grate and logging gate; free roam opens them.
Gold marks optional routes and dashed gold marks the underground drain beneath existing surface trails.
Accepted main routes, original Forward shortcuts, global vehicle settings and existing probabilistic AI choice, HUD, racing minimap and music preserved.
See Review/VALIDATION.md for targeted checks and gameplay-review limitations.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
Copy-Item -LiteralPath 'Docs/CaveHillside/VALIDATION.md' -Destination (Join-Path $runtime 'VALIDATION.md')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
foreach($file in @('VALIDATION.md','view-40.png','view-195.png')){Copy-Item -LiteralPath (Join-Path 'Docs/CaveHillside' $file) -Destination $review}
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.59.0-review1-Windows --previous-catalog Builds/LauncherRelease-58000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-58000/assets/game-manifest.json --out Builds/LauncherRelease-59000 --build 59000 --version 0.59.0-review1 --notes "Forest Forward cave integrated into a wooded hillside; eye-line slabs lowered, challenging right passage and local AI preserved. Source commit $sourceCommit."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}






