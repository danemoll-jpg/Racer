$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$builds=Join-Path $root 'Builds'
$temp=Join-Path $root 'Temp'
$evidence=Join-Path $root 'Docs/ForestWaterJump'
$hosted=Get-Content (Join-Path $evidence 'hosted/result.json') -Raw | ConvertFrom-Json
$launch=Get-Content (Join-Path $evidence 'play-racer-launch.json') -Raw | ConvertFrom-Json
if(!$hosted.startupReady -or !$launch.responsive -or $launch.build -ne 31000){throw 'Delivery verification required before cleanup'}
$state=Get-Content (Join-Path $builds 'Latest/state.json') -Raw | ConvertFrom-Json
if($state.game.manifest.build -ne 31000 -or $state.previousGame.manifest.build -ne 30000){throw 'Retain current and previous signed runtimes'}
$old=Join-Path $builds 'Racer-0.29.0-review1-Windows'
$musicChecked=0
if(Test-Path -LiteralPath $old){
 $oldReadme=Join-Path $old 'Music/README-Racer.txt'
 if(Test-Path -LiteralPath $oldReadme){Copy-Item -LiteralPath $oldReadme -Destination (Join-Path $evidence 'previous-package-music-readme.txt')}
 foreach($file in Get-ChildItem -LiteralPath (Join-Path $old 'Music') -File -Recurse){
  $relative=$file.FullName.Substring((Join-Path $old 'Music').Length+1)
  $candidates=@((Join-Path $root ('BundleMusic/'+$relative)),(Join-Path $builds ('Latest/shared/soundtracks/1/'+$relative)))
  if($relative -eq 'README-Racer.txt'){$candidates+=Join-Path $evidence 'previous-package-music-readme.txt'}
  $matched=$false
  foreach($candidate in $candidates){if((Test-Path -LiteralPath $candidate) -and (Get-FileHash -LiteralPath $candidate).Hash -eq (Get-FileHash -LiteralPath $file.FullName).Hash){$matched=$true;break}}
  if(!$matched){throw "Old music file has no verified preserved duplicate: $relative"}
  $musicChecked++
 }
}
$before=(Get-ChildItem -LiteralPath $builds -File -Recurse | Measure-Object Length -Sum).Sum
$targets=@($old,(Join-Path $builds 'Racer-0.31.0-review1-Windows'),(Join-Path $builds 'LauncherRelease-31000/assets/game.zip'),$hosted.work)
$removed=@()
foreach($target in $targets){
 $absolute=[IO.Path]::GetFullPath($target)
 if(!$absolute.StartsWith($builds+'\',[StringComparison]::OrdinalIgnoreCase) -and !$absolute.StartsWith($temp+'\',[StringComparison]::OrdinalIgnoreCase)){throw "Outside disposable roots: $absolute"}
 if($absolute.StartsWith((Join-Path $builds 'Latest'),[StringComparison]::OrdinalIgnoreCase)){throw 'Latest is protected'}
 if(Test-Path -LiteralPath $absolute){
  $item=Get-Item -LiteralPath $absolute
  $links=@($item)+@(Get-ChildItem -LiteralPath $absolute -Recurse -Force -ErrorAction Stop)
  if($links | Where-Object {$_.Attributes -band [IO.FileAttributes]::ReparsePoint}){throw "Reparse point in cleanup target: $absolute"}
  Remove-Item -LiteralPath $absolute -Recurse -Force
  $removed+=$absolute
 }
}
$after=(Get-ChildItem -LiteralPath $builds -File -Recurse | Measure-Object Length -Sum).Sum
[ordered]@{beforeBytes=$before;afterBytes=$after;afterGiB=[math]::Round($after/1GB,3);removed=$removed;duplicateMusicFilesVerified=$musicChecked;preserved='Complete root Latest runtime, current 31000 and previous 30000 managed runtimes, music, saves, publisher identity/tools, launcher, source and metadata';freeBytesAfter=(Get-PSDrive C).Free} | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $evidence 'cleanup.json')
Get-Content (Join-Path $evidence 'cleanup.json')
