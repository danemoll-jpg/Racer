$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$latest=Join-Path $root 'Builds/Latest'
$evidence=Join-Path $root 'Docs/McFaddenEntrance'
$hosted=Get-Content (Join-Path $evidence 'hosted/result.json') -Raw | ConvertFrom-Json
if(!$hosted.startupReady -or $hosted.build -ne 35000){throw 'Public download must pass first'}
$manifest=Join-Path $hosted.work 'game-manifest.json'
$before=Get-Content (Join-Path $latest 'state.json') -Raw | ConvertFrom-Json
$musicBefore=$before.music | ConvertTo-Json -Depth 30 -Compress
$checks=Join-Path $root 'Builds/Launcher/LauncherChecks.exe'
$saves=Join-Path $root 'Temp/mcfadden-entrance-activation-saves'
& $checks $latest $saves game $manifest
if($LASTEXITCODE -ne 0){throw 'Production updater activation failed'}
$after=Get-Content (Join-Path $latest 'state.json') -Raw | ConvertFrom-Json
if($after.game.manifest.build -ne 35000){throw 'Incorrect game activated'}
if(($after.music | ConvertTo-Json -Depth 30 -Compress) -ne $musicBefore){throw 'Soundtrack state changed'}
Copy-Item -LiteralPath $manifest -Destination (Join-Path $latest 'game-manifest.json')
$result=& $checks $latest $saves check 'https://github.com/danemoll-jpg/woodstock-rush-releases/releases/latest/download/update-catalog.json'
if($LASTEXITCODE -ne 0){throw 'Public catalog check failed'}
$result | Set-Content (Join-Path $evidence 'launcher-catalog-check.json')
$result



