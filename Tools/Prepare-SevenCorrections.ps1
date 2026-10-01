$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$sourceCommit=git rev-parse HEAD
if($LASTEXITCODE -ne 0){throw 'Cannot identify completion source commit'}
$runtime=Join-Path (Get-Location) 'Builds/Racer-0.57.0-review1-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/56000'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
@'
Woodstock Rush 0.57.0-review1
Seven corrections: progressive ridge/drain transitions, removed approach wedge, confined dark wet drain with five rats, reduced pool-house jump, and enclosed natural Forest Forward cave.
Forward closes the new drain grate and logging gate; free roam opens them.
Gold marks optional routes and dashed gold marks the underground drain beneath existing surface trails.
Accepted main routes, original Forward shortcuts, global vehicle settings and existing probabilistic AI choice, HUD, racing minimap and music preserved.
See Review/VALIDATION.md for targeted checks and gameplay-review limitations.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
Copy-Item -LiteralPath 'Docs/SevenCorrections/VALIDATION.md' -Destination (Join-Path $runtime 'VALIDATION.md')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
foreach($file in @('VALIDATION.md','after-drain.png','after-cave-160.png','after-ridge.png')){Copy-Item -LiteralPath (Join-Path 'Docs/SevenCorrections' $file) -Destination $review}
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-0.57.0-review1-Windows --previous-catalog Builds/LauncherRelease-56000/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-56000/assets/game-manifest.json --out Builds/LauncherRelease-57000 --build 57000 --version 0.57.0-review1 --notes "Seven corrections: smooth ridge and drain transitions; exposed approach wedge removed; confined wet drain and five visible rats; reduced pool-house jump; enclosed natural Forest Forward cave. Systemic recovery preserved; no Reverse cave routing, global physics or music changes. Source commit $sourceCommit."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}




