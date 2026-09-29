$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$runtime=Join-Path $root 'Builds/Racer-0.37.0-review1-Windows'
if(!(Test-Path (Join-Path $root 'Docs/YardReset/build-done.txt'))){throw 'Successful fresh build required'}
$commit=(Get-Content (Join-Path $root 'Temp/yard-reset-source-commit.txt') -Raw).Trim()
if((Get-Content (Join-Path $runtime 'VERSION.txt') -Raw) -notmatch $commit){throw 'Runtime source identity mismatch'}
foreach($name in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $root "Builds/Latest/$name") -Destination (Join-Path $runtime $name)}
$review=Join-Path $runtime 'Review'
$null=New-Item -ItemType Directory -Path $review -Force
foreach($name in @('anchor-validation.svg','restored-world-overhead.png','entrance.png','parking.png','property-overhead.png','anchor-7.png','restored-forest.png')){Copy-Item -LiteralPath (Join-Path $root "Docs/YardReset/$name") -Destination (Join-Path $review $name)}
@'
Woodstock Rush 0.37.0-review1 - Property reset and anchor validation

Dan's Backyard's rejected first attempt is removed. The established forest and Kyle driveway fix remain. Dan's existing beige driveway is smoothed and supported, lower parking is flat, affected fences are reseated and the existing mailbox is left of the main entrance when looking toward the property.

Use Street Loop free roam to inspect the eight pink numbered anchors. Their exact X/Z positions are recorded in Review/anchor-validation.svg. The markers have no colliders and their labels remain visible through trees. No new roads, trail, dump, gully or race route has been constructed. Dan must validate the anchors before Forward construction is authorized.

Play-Racer.cmd uses the existing signed launcher/updater. Music and saved settings remain separate and preserved. F3 toggles the XYZ display; F4 copies the current location.
'@ | Set-Content (Join-Path $runtime 'README.txt')
Copy-Item -LiteralPath (Join-Path $root 'Docs/YardReset/VALIDATION.md') -Destination (Join-Path $runtime 'VALIDATION.md')
Write-Output 'Preserved cover/radio documentation and added the current anchor review atlas to the fresh runtime.'
