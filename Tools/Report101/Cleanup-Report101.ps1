$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$builds=Join-Path $root 'Builds'
$evidence=Join-Path $root 'Docs/Report101'
$launch=Get-Content (Join-Path $evidence 'play-racer-launch.json') -Raw | ConvertFrom-Json
$hosted=Get-Content (Join-Path $evidence 'hosted/result.json') -Raw | ConvertFrom-Json
$state=Get-Content (Join-Path $builds 'Latest/state.json') -Raw | ConvertFrom-Json
if($state.game.manifest.build -ne 101000 -or !$hosted.startupReady){throw 'Delivery gates missing'}
if(($launch | ConvertTo-Json -Depth 20) -notmatch '101000'){throw 'Production launcher verification missing'}
function Bytes([string]$p){$n=(Get-ChildItem -LiteralPath $p -File -Recurse | Measure-Object -Property Length -Sum).Sum;if($null -eq $n){return 0};return [long]$n}
$before=Bytes $builds;$freeBefore=(Get-PSDrive C).Free
$targets=@((Join-Path $builds 'Racer-0.101.0-review1-Windows'),(Join-Path $builds 'LauncherRelease-101000/assets/game.zip'),(Join-Path $builds 'Latest/versions/99000'),(Join-Path $builds 'Check101'),(Join-Path $builds 'Check101b'),(Join-Path $builds 'Racer-0.101.0-check1-Windows'),(Join-Path $builds 'Racer-0.101.0-check2-Windows'),(Join-Path $root 'Temp/Report101Save'),[string]$hosted.work)+@(Get-ChildItem $builds -Directory -Filter 'Check101-*' | ForEach-Object FullName)
$running=Get-CimInstance Win32_Process | Where-Object {$_.ExecutablePath -and $_.ExecutablePath.StartsWith($root,[StringComparison]::OrdinalIgnoreCase)}
$removed=@()
foreach($candidate in $targets){
 $path=[IO.Path]::GetFullPath($candidate)
 if(!($path.StartsWith($builds+'\',[StringComparison]::OrdinalIgnoreCase) -or $path.StartsWith((Join-Path $root 'Temp')+'\',[StringComparison]::OrdinalIgnoreCase))){throw "Unsafe cleanup target: $path"}
 if($running | Where-Object {$_.ExecutablePath.StartsWith($path+'\',[StringComparison]::OrdinalIgnoreCase)}){throw "Active process under $path"}
 if(Test-Path -LiteralPath $path){$size=if((Get-Item -LiteralPath $path).PSIsContainer){Bytes $path}else{(Get-Item -LiteralPath $path).Length};Remove-Item -LiteralPath $path -Recurse -Force;$removed+=@{path=$path;bytes=$size}}
}
$after=Bytes $builds;$freeAfter=(Get-PSDrive C).Free
@{buildsBefore=$before;buildsAfter=$after;freeBefore=$freeBefore;freeAfter=$freeAfter;buildsRecovered=$before-$after;removed=$removed;retained=@('Latest complete root runtime','managed 101000','managed 100000','music, publisher keys/tools, launcher, metadata, source, evidence, saves')} | ConvertTo-Json -Depth 8 | Set-Content (Join-Path $evidence 'cleanup.json')
Get-Content (Join-Path $evidence 'cleanup.json')



