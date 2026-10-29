$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$builds=Join-Path $root 'Builds'
$evidence=Join-Path $root 'Docs/Report1300'
$launch=Get-Content (Join-Path $evidence 'play-racer-launch.json') -Raw | ConvertFrom-Json
$state=Get-Content (Join-Path $builds 'Latest/state.json') -Raw | ConvertFrom-Json
if($state.game.manifest.build -ne 1030600){throw 'Delivery gates missing'}
if(($launch | ConvertTo-Json -Depth 20) -notmatch '1030600'){throw 'Production launcher verification missing'}
function Bytes([string]$p){$n=(Get-ChildItem -LiteralPath $p -File -Recurse | Measure-Object -Property Length -Sum).Sum;if($null -eq $n){return 0};return [long]$n}
$before=Bytes $builds;$freeBefore=(Get-PSDrive C).Free
$targets=@((Join-Path $builds 'Racer-1.3.0-review7-Windows'),(Join-Path $builds 'LauncherRelease-1030600/assets/game.zip'),(Join-Path $root 'Temp/Report1300-activation-saves'),(Join-Path $root 'Temp/Report1300-signature-check'),(Join-Path $root 'Temp/Report1300-signature-saves'),(Join-Path $builds 'Latest/versions/1030400'),(Join-Path $builds 'Check1300r7'),(Join-Path $builds 'Check1300r7-build.txt'),(Join-Path $builds 'Check1300r7b'),(Join-Path $builds 'Check1300r7b-build.txt'),(Join-Path $builds 'Check1300b'),(Join-Path $builds 'Check1300b-build.txt'))
if($state.previousGame.directory -ne 'versions/1030500'){throw 'The previous managed runtime must be 1030500 before 1030400 is removed'}
$running=Get-CimInstance Win32_Process | Where-Object {$_.ExecutablePath -and $_.ExecutablePath.StartsWith($root,[StringComparison]::OrdinalIgnoreCase)}
$removed=@()
foreach($candidate in $targets){
 $path=[IO.Path]::GetFullPath($candidate)
 if(!($path.StartsWith($builds+'\',[StringComparison]::OrdinalIgnoreCase) -or $path.StartsWith((Join-Path $root 'Temp')+'\',[StringComparison]::OrdinalIgnoreCase))){throw "Unsafe cleanup target: $path"}
 if($running | Where-Object {$_.ExecutablePath.StartsWith($path+'\',[StringComparison]::OrdinalIgnoreCase)}){throw "Active process under $path"}
 if(Test-Path -LiteralPath $path){$size=if((Get-Item -LiteralPath $path).PSIsContainer){Bytes $path}else{(Get-Item -LiteralPath $path).Length};Remove-Item -LiteralPath $path -Recurse -Force;$removed+=@{path=$path;bytes=$size}}
}
$after=Bytes $builds;$freeAfter=(Get-PSDrive C).Free
@{buildsBefore=$before;buildsAfter=$after;freeBefore=$freeBefore;freeAfter=$freeAfter;buildsRecovered=$before-$after;removed=$removed;retained=@('Latest complete root runtime','managed 1030600','managed 1030500 (1.3.0-review6)','music, publisher keys/tools, launcher, metadata, source, evidence, saves')} | ConvertTo-Json -Depth 8 | Set-Content (Join-Path $evidence 'cleanup.json')
Get-Content (Join-Path $evidence 'cleanup.json')



