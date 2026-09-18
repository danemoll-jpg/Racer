param([string]$Version='0.3.0-review1')
$ErrorActionPreference='Stop'
$projectRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$name="Racer-$Version-Windows"
$buildRoot=Join-Path $projectRoot "Builds/$name"
$stageRoot=Join-Path $projectRoot "Builds/Package/$name"
$archive=Join-Path $projectRoot "Builds/$name.zip"
if((Test-Path $stageRoot) -or (Test-Path $archive)){throw 'Use a new version; reviewed packages are not overwritten.'}
foreach($item in @('Racer.exe','UnityPlayer.dll','Racer_Data','MonoBleedingEdge','VERSION.txt')){if(!(Test-Path (Join-Path $buildRoot $item))){throw "Missing $item"}}
Get-ChildItem -LiteralPath $buildRoot -Recurse -File | ForEach-Object {
 $relative=[IO.Path]::GetRelativePath($buildRoot,$_.FullName)
 if($relative -notmatch 'DoNotShip|Do Not Ship|BackUpThisFolder|Validation|\.(pdb|mdb|log)$'){
  $target=Join-Path $stageRoot $relative
  New-Item -ItemType Directory -Force ([IO.Path]::GetDirectoryName($target)) | Out-Null
  Copy-Item -LiteralPath $_.FullName -Destination $target
 }
}
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'README-player.txt') -Destination (Join-Path $stageRoot 'README.txt')
$licenses=Join-Path $stageRoot 'LICENSES'
New-Item -ItemType Directory -Force $licenses | Out-Null
foreach($name in @('AUDIO-ASSET-NOTICES.txt','Unity-Windows-Mono-Notices.pdf','PackageNotices')){Copy-Item -LiteralPath (Join-Path $PSScriptRoot "../CR020-021/$name") -Destination $licenses -Recurse}
$manifest=Get-ChildItem -LiteralPath $stageRoot -Recurse -File | ForEach-Object {[pscustomobject]@{Path=[IO.Path]::GetRelativePath($stageRoot,$_.FullName);Bytes=$_.Length;SHA256=(Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash}}
$manifest | ConvertTo-Json -Depth 3 | Set-Content (Join-Path $PSScriptRoot 'package-files.json')
Add-Type -AssemblyName System.IO.Compression.FileSystem
[IO.Compression.ZipFile]::CreateFromDirectory($stageRoot,$archive,[IO.Compression.CompressionLevel]::Optimal,$true)
@{Archive=$archive;Bytes=(Get-Item -LiteralPath $archive).Length;SHA256=(Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash;Files=$manifest.Count}|ConvertTo-Json|Set-Content (Join-Path $PSScriptRoot 'package.json')
Get-Content (Join-Path $PSScriptRoot 'package.json')
