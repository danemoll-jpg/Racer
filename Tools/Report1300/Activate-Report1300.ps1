# 1.3.0-review6 (published at Dan's request; activated locally first): the existing updater activates the staged 1030500 from its
# locally signed manifest. Stage-Report1300.py has already copied the runtime into Builds/Latest/versions/1030500, so the updater only checks
# every file against the signed manifest and switches to it (nothing downloaded); the published catalog (1010100, the 1.1.0 release) is older, so the launcher keeps it.
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$latest=Join-Path $root 'Builds/Latest'
$evidence=Join-Path $root 'Docs/Report1300'
$manifest=Join-Path $root 'Builds/LauncherRelease-1030500/assets/game-manifest.json'
$before=Get-Content (Join-Path $latest 'state.json') -Raw | ConvertFrom-Json
$musicBefore=$before.music | ConvertTo-Json -Depth 30 -Compress
$checks=Join-Path $root 'Builds/Launcher/LauncherChecks.exe'
$saves=Join-Path $root 'Temp/Report1300-activation-saves'
& $checks $latest $saves game $manifest
if($LASTEXITCODE -ne 0){throw 'Updater activation failed'}
$after=Get-Content (Join-Path $latest 'state.json') -Raw | ConvertFrom-Json
if($after.game.manifest.build -ne 1030500){throw 'Incorrect game activated'}
if(($after.music | ConvertTo-Json -Depth 30 -Compress) -ne $musicBefore){throw 'Soundtrack state changed'}
Copy-Item -LiteralPath $manifest -Destination (Join-Path $latest 'game-manifest.json')
$result=& $checks $latest $saves check 'https://github.com/danemoll-jpg/woodstock-rush-releases/releases/latest/download/update-catalog.json'
if($LASTEXITCODE -ne 0){throw 'Catalog check failed'}
$result | Set-Content (Join-Path $evidence 'launcher-catalog-check.json')
@{game=$after.game.directory;build=$after.game.manifest.build;previous=$after.previousGame.directory;publishedCatalog=($result|ConvertFrom-Json)} | ConvertTo-Json -Depth 5
