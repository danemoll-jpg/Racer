param([string]$Version='0.6.1-review2')
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$builds=[IO.Path]::GetFullPath((Join-Path $root 'Builds'))
$name="Racer-$Version-Windows"
$stage=Join-Path $builds "Package/$name"
$zip=Join-Path $builds "$name.zip"
$extract=Join-Path $builds "Verified/$name"
$latest=Join-Path $builds 'Latest'
$previous=Join-Path $builds "Latest-before-$Version"
foreach($path in @($stage,$extract,$latest,$previous)) {
 if(![IO.Path]::GetFullPath($path).StartsWith($builds+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)){throw 'Path escaped Builds'}
}
if((Test-Path -LiteralPath $extract) -or (Test-Path -LiteralPath $previous)){throw 'Verification/backup already exists; inspect rather than overwrite'}
$manifest=Get-Content (Join-Path $PSScriptRoot 'package-files.json') -Raw | ConvertFrom-Json
$source=(Get-Content (Join-Path $root 'Temp/correction-source-commit.txt') -Raw).Trim()
if(!(Get-Content (Join-Path $stage 'VERSION.txt') -Raw).Contains("Source commit: $source")){throw 'Source metadata mismatch'}
Add-Type -AssemblyName System.IO.Compression.FileSystem
[IO.Compression.ZipFile]::ExtractToDirectory($zip,$extract)
function Verify-Files([string]$directory) {
 $files=@(Get-ChildItem -LiteralPath $directory -Recurse -File)
 if($files.Count -ne $manifest.Count){throw "Unexpected file count in $directory"}
 foreach($entry in $manifest) {
  $file=Join-Path $directory $entry.Path
  if(!(Test-Path -LiteralPath $file) -or (Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash -ne $entry.SHA256){throw "Hash mismatch: $file"}
 }
}
Verify-Files (Join-Path $extract $name)
Verify-Files $stage
if(Test-Path -LiteralPath $latest){Move-Item -LiteralPath $latest -Destination $previous}
New-Item -ItemType Directory -Path $latest | Out-Null
Get-ChildItem -LiteralPath $stage | ForEach-Object {Copy-Item -LiteralPath $_.FullName -Destination $latest -Recurse}
Verify-Files $latest
@{Source=$source;Files=$manifest.Count;ExtractedVerified=$true;LatestVerified=$true;PreviousLatest=$previous;ZipSHA256=(Get-FileHash -LiteralPath $zip -Algorithm SHA256).Hash} | ConvertTo-Json | Set-Content (Join-Path $PSScriptRoot 'package-verification.json')
Get-Content (Join-Path $PSScriptRoot 'package-verification.json')


