$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$reportPath=Join-Path $root 'Builds/PACKAGE-LATEST.json'
$report=Get-Content -LiteralPath $reportPath -Raw | ConvertFrom-Json
if($report.Version -ne '0.20.0-review1'){throw 'Expected review preview package'}
$buildRoot=[IO.Path]::GetFullPath((Join-Path $root 'Builds'))+[IO.Path]::DirectorySeparatorChar
foreach($path in @($report.Runtime,$report.Latest,$report.Extracted,$report.ZIP,$report.Preserved)){
 if(![IO.Path]::GetFullPath($path).StartsWith($buildRoot,[StringComparison]::OrdinalIgnoreCase)){throw "Outside build workspace: $path"}
 if((Get-Item -LiteralPath $path).Attributes -band [IO.FileAttributes]::ReparsePoint){throw "Linked target: $path"}
}
$files=@{'README.txt'='README-player.txt';'VALIDATION.md'='VALIDATION.md'}
# Only these owned review-document entries change. Preserve the pre-final ZIP.
Copy-Item -LiteralPath $report.ZIP -Destination (Join-Path $report.Preserved 'Before-final-documentation.zip')
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip=[IO.Compression.ZipFile]::Open($report.ZIP,[IO.Compression.ZipArchiveMode]::Update)
try{
 foreach($name in $files.Keys){
  $source=Join-Path $root ('Docs/CR112-118/'+$files[$name])
  foreach($target in @($report.Runtime,$report.Latest,$report.Extracted)){Copy-Item -LiteralPath $source -Destination (Join-Path $target $name) -Force}
  $entryName='Racer-0.20.0-review1-Windows/'+$name
  $old=$zip.GetEntry($entryName);if(!$old){throw "Missing owned entry $entryName"};$old.Delete()
  [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip,$source,$entryName,[IO.Compression.CompressionLevel]::Optimal)|Out-Null
 }
}finally{$zip.Dispose()}
$manifestPath=Join-Path $report.Preserved 'package-files.json'
$manifest=Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
$zip=[IO.Compression.ZipFile]::OpenRead($report.ZIP)
try{
 foreach($entry in $manifest){
  $path=Join-Path $report.Runtime $entry.Path
  if($files.ContainsKey($entry.Path)){$entry.Bytes=(Get-Item -LiteralPath $path).Length;$entry.SHA256=(Get-FileHash -LiteralPath $path).Hash}
  foreach($target in @($report.Runtime,$report.Latest,$report.Extracted)){if((Get-FileHash -LiteralPath (Join-Path $target $entry.Path)).Hash -ne $entry.SHA256){throw "File mismatch $target/$($entry.Path)"}}
  $z=$zip.GetEntry('Racer-0.20.0-review1-Windows/'+$entry.Path.Replace('\','/'));if(!$z){throw "ZIP missing $($entry.Path)"}
  $stream=$z.Open();$sha=[Security.Cryptography.SHA256]::Create()
  try{$actual=[Convert]::ToHexString($sha.ComputeHash($stream))}finally{$stream.Dispose();$sha.Dispose()}
  if($actual -ne $entry.SHA256){throw "ZIP mismatch $($entry.Path)"}
 }
}finally{$zip.Dispose()}
$manifest|ConvertTo-Json -Depth 3|Set-Content -LiteralPath $manifestPath
$report.ZipSHA256=(Get-FileHash -LiteralPath $report.ZIP).Hash
$report.PackagedAt=Get-Date -Format o
$report|ConvertTo-Json|Set-Content -LiteralPath $reportPath
Copy-Item -LiteralPath $reportPath -Destination (Join-Path $root 'Docs/CR112-118/package-verification.json') -Force
$report|ConvertTo-Json
