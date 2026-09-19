param([ValidatePattern('^[0-9]+\.[0-9]+\.[0-9]+(?:-[A-Za-z0-9-]+)?$')][string]$Version='0.9.1-review1')
$ErrorActionPreference='Stop'
$projectRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$builds=Join-Path $projectRoot 'Builds'
$runtime=Join-Path $builds "Racer-$Version-Windows"
$latest=Join-Path $builds 'Latest'
$archive="$runtime.zip"
$music=Join-Path $projectRoot 'BundleMusic'
$stamp=(Get-Date -Format 'yyyyMMdd-HHmmss-fff')+'-'+[Guid]::NewGuid().ToString('N').Substring(0,6)
$stage=Join-Path $builds "PackageWork/$stamp/Racer-$Version-Windows"
$preserved=Join-Path $builds "Preserved/$stamp"
function Assert-BuildPath([string]$path){
 $absolute=[IO.Path]::GetFullPath($path)
 if(!$absolute.StartsWith($builds+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)){throw "Unsafe build target: $absolute"}
 # Refuse linked ancestors before moving/copying runtime trees.
 $at=$absolute
 while($at.Length -ge $builds.Length){if((Test-Path -LiteralPath $at) -and ((Get-Item -LiteralPath $at -Force).Attributes -band [IO.FileAttributes]::ReparsePoint)){throw "Linked build target: $at"};$at=[IO.Path]::GetDirectoryName($at)}
}
function Files-NoLinks([string]$root){
 if((Get-Item -LiteralPath $root -Force).Attributes -band [IO.FileAttributes]::ReparsePoint){throw "Refusing linked root $root"}
 $pending=[Collections.Generic.Stack[string]]::new();$pending.Push($root)
 while($pending.Count){foreach($item in Get-ChildItem -LiteralPath $pending.Pop() -Force){
  if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw "Refusing linked entry $($item.FullName). Use an ordinary copied file."}
  if($item.PSIsContainer){$pending.Push($item.FullName)}else{$item}
 }}
}
foreach($path in @($runtime,$latest,$stage,$preserved,$archive)){Assert-BuildPath $path}
foreach($required in @('Racer.exe','UnityPlayer.dll','Racer_Data','MonoBleedingEdge','VERSION.txt')){if(!(Test-Path -LiteralPath (Join-Path $runtime $required))){throw "Build $Version first: missing $required"}}
if(!(Get-Content -LiteralPath "$runtime/VERSION.txt" -Raw).Contains("Racer $Version")){throw 'Runtime version mismatch'}
New-Item -ItemType Directory -Force $music,$stage,$preserved | Out-Null
# Read only the deliberate staging folder, never preferences or a selected custom collection.
$stagedSongs=@(Files-NoLinks $music | Where-Object Extension -in @('.mp3','.wav','.ogg'))
$runtimeFiles=@(Files-NoLinks $runtime)
foreach($file in $runtimeFiles){
 $relative=[IO.Path]::GetRelativePath($runtime,$file.FullName)
 if($relative -match '^Music[\\/]|DoNotShip|Do Not Ship|BackUpThisFolder|Validation|\.(pdb|mdb|log)$'){continue}
 # Loose personal audio never rides along from an old runtime, even outside Music.
 # Authored game sounds are embedded Unity assets; only BundleMusic supplies loose songs.
 if($file.Extension -in @('.mp3','.wav','.ogg','.flac','.m4a','.aac','.wma','.opus','.aif','.aiff')){continue}
 $destination=Join-Path $stage $relative;New-Item -ItemType Directory -Force ([IO.Path]::GetDirectoryName($destination)) | Out-Null
 Copy-Item -LiteralPath $file.FullName -Destination $destination
}
New-Item -ItemType Directory -Force "$stage/Music" | Out-Null
foreach($song in $stagedSongs){
 $destination=Join-Path "$stage/Music" ([IO.Path]::GetRelativePath($music,$song.FullName));New-Item -ItemType Directory -Force ([IO.Path]::GetDirectoryName($destination)) | Out-Null
 Copy-Item -LiteralPath $song.FullName -Destination $destination
 if((Get-FileHash -LiteralPath $song.FullName).Hash -ne (Get-FileHash -LiteralPath $destination).Hash){throw 'Staged music verification failed'}
}
Copy-Item -LiteralPath "$projectRoot/Docs/CR040-054-055/RADIO.md" -Destination "$stage/RADIO.md" -Force
'Each immediate folder is a radio channel; nested artist/album folders belong to that channel. Root songs form General. Add MP3, PCM WAV or Ogg Vorbis, then Rescan. See RADIO.md beside Racer.exe. Stage selected songs in project BundleMusic before repackaging.' | Set-Content -LiteralPath "$stage/Music/README-Racer.txt"
(Get-Content -LiteralPath "$projectRoot/Docs/CR061-062/README-player.txt" -Raw).Replace("0.9.1-review1",$Version) | Set-Content -LiteralPath "$stage/README.txt"
$licenses=Join-Path $stage 'Licenses';New-Item -ItemType Directory -Force $licenses | Out-Null
foreach($notice in @('AUDIO-ASSET-NOTICES.txt','Unity-Windows-Mono-Notices.pdf','PackageNotices')){Copy-Item -LiteralPath "$projectRoot/Docs/CR020-021/$notice" -Destination $licenses -Recurse -Force}
Copy-Item -LiteralPath "$projectRoot/Docs/CR028-033/VEHICLE-ASSET-NOTICES.txt" -Destination $licenses -Force
$manifest=@(Files-NoLinks $stage | ForEach-Object {[pscustomobject]@{Path=[IO.Path]::GetRelativePath($stage,$_.FullName);Bytes=$_.Length;SHA256=(Get-FileHash -LiteralPath $_.FullName).Hash}})
$newZip=Join-Path $builds "PackageWork/$stamp/package.zip"
Add-Type -AssemblyName System.IO.Compression.FileSystem
[IO.Compression.ZipFile]::CreateFromDirectory($stage,$newZip,[IO.Compression.CompressionLevel]::Optimal,$true)
$extract=Join-Path $builds "PackageWork/$stamp/extracted"
[IO.Compression.ZipFile]::ExtractToDirectory($newZip,$extract)
$extracted=Join-Path $extract "Racer-$Version-Windows"
foreach($entry in $manifest){if((Get-FileHash -LiteralPath (Join-Path $extracted $entry.Path)).Hash -ne $entry.SHA256){throw "ZIP mismatch $($entry.Path)"}}
# Validate fully before replacement. No deletion: keep complete old runtime/Latest/ZIP.
try {
 if(Test-Path -LiteralPath $latest){$null=@(Files-NoLinks $latest);Move-Item -LiteralPath $latest -Destination "$preserved/Latest";Write-Host "Previous Latest (including directly added Music) preserved at $preserved/Latest"}
 Move-Item -LiteralPath $runtime -Destination "$preserved/Versioned-runtime"
 if(Test-Path -LiteralPath $archive){Move-Item -LiteralPath $archive -Destination "$preserved/Previous.zip"}
 Copy-Item -LiteralPath $stage -Destination $runtime -Recurse
 Copy-Item -LiteralPath $stage -Destination $latest -Recurse
 Move-Item -LiteralPath $newZip -Destination $archive
 foreach($target in @($runtime,$latest)){
 $actual=@(Files-NoLinks $target)
 if($actual.Count -ne $manifest.Count){throw "Runtime count mismatch $target"}
 foreach($entry in $manifest){if((Get-FileHash -LiteralPath (Join-Path $target $entry.Path)).Hash -ne $entry.SHA256){throw "Runtime mismatch $target/$($entry.Path)"}}
 }
} catch {
 # A locked file or failed copy must not leave the regular entry point half installed.
 foreach($pair in @(@($runtime,"$preserved/Versioned-runtime",'Incomplete-runtime'),@($latest,"$preserved/Latest",'Incomplete-Latest'),@($archive,"$preserved/Previous.zip",'Incomplete.zip'))){
  if(Test-Path -LiteralPath $pair[1]){
   if(Test-Path -LiteralPath $pair[0]){Move-Item -LiteralPath $pair[0] -Destination (Join-Path $preserved $pair[2])}
   Move-Item -LiteralPath $pair[1] -Destination $pair[0]
  }
 }
 throw
}
$manifest | ConvertTo-Json -Depth 3 | Set-Content -LiteralPath "$preserved/package-files.json"
$report=[ordered]@{Version=$Version;PackagedAt=(Get-Date -Format o);Songs=$stagedSongs.Count;Files=$manifest.Count;Runtime=$runtime;Latest=$latest;ZIP=$archive;ZipSHA256=(Get-FileHash -LiteralPath $archive).Hash;Preserved=$preserved;Extracted=$extracted;Verified='All extracted ZIP, versioned runtime and Latest files match SHA256';ExternalCollections='Not read or copied'}
$report | ConvertTo-Json | Set-Content -LiteralPath "$builds/PACKAGE-LATEST.json"
$report | ConvertTo-Json
if(!$stagedSongs.Count){Write-Host 'No songs are staged in BundleMusic; the shareable Music folder is empty except instructions.'}
Write-Host 'To migrate chosen songs from preserved Latest/Music, copy them to BundleMusic and run this command again.'
