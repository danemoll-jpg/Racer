$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$builds=Join-Path $root 'Builds';$temp=Join-Path $root 'Temp';$evidence=Join-Path $root 'Docs/PropertyCorrections'
$hosted=Get-Content (Join-Path $evidence 'hosted/result.json') -Raw | ConvertFrom-Json
$launch=Get-Content (Join-Path $evidence 'play-racer-launch.json') -Raw | ConvertFrom-Json
$state=Get-Content (Join-Path $builds 'Latest/state.json') -Raw | ConvertFrom-Json
if(!$hosted.startupReady -or !$launch.responsive -or $launch.build -ne 34000 -or $state.game.manifest.build -ne 34000 -or $state.previousGame.manifest.build -ne 33000){throw 'Verified current/previous delivery required'}
$before=(Get-ChildItem -LiteralPath $builds -File -Recurse | Measure-Object Length -Sum).Sum
$freeBefore=(Get-PSDrive C).Free
$targets=@((Join-Path $builds 'Racer-0.34.0-review1-Windows'),(Join-Path $builds 'LauncherRelease-34000/assets/game.zip'),$hosted.work,(Join-Path $temp 'property-python'),(Join-Path $temp 'property-corrections-runtime-saves'),(Join-Path $temp 'property-corrections-pre-clearance-build'),(Join-Path $temp 'property-corrections-editor-saves'))
$obsolete=Join-Path $builds 'Latest/versions/32000'
if(Test-Path -LiteralPath $obsolete){
 if($state.game.directory -eq 'versions/32000' -or $state.previousGame.directory -eq 'versions/32000'){throw 'Old version remains active'}
 $targets+=$obsolete
}
$removed=@()
foreach($target in $targets){
 $absolute=[IO.Path]::GetFullPath($target)
 if(!$absolute.StartsWith($builds+'\',[StringComparison]::OrdinalIgnoreCase) -and !$absolute.StartsWith($temp+'\',[StringComparison]::OrdinalIgnoreCase)){throw "Outside disposable roots: $absolute"}
 if($absolute.StartsWith((Join-Path $builds 'Latest'),[StringComparison]::OrdinalIgnoreCase) -and $absolute -ne $obsolete){throw 'Current/previous runtime protected'}
 if(Test-Path -LiteralPath $absolute){
  $items=@(Get-Item -LiteralPath $absolute)+@(Get-ChildItem -LiteralPath $absolute -Recurse -Force)
  if($items | Where-Object {$_.Attributes -band [IO.FileAttributes]::ReparsePoint}){throw "Reparse point in target: $absolute"}
  Remove-Item -LiteralPath $absolute -Recurse -Force;$removed+=$absolute
 }
}
$after=(Get-ChildItem -LiteralPath $builds -File -Recurse | Measure-Object Length -Sum).Sum
[ordered]@{beforeBytes=$before;afterBytes=$after;recoveredBuildsBytes=$before-$after;freeBytesBefore=$freeBefore;freeBytesAfter=(Get-PSDrive C).Free;removed=$removed;preserved='Current complete root runtime, managed 34000 and previous 33000, all music, source, saves, publisher identity/tools, launcher and signed evidence'} | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $evidence 'cleanup.json')
Get-Content (Join-Path $evidence 'cleanup.json')


