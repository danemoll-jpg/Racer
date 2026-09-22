$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$builds=Join-Path $root 'Builds'
$temp=Join-Path $root 'Temp'
$published=Get-Content (Join-Path $root 'Docs/ForestHill/hosted/result.json') -Raw | ConvertFrom-Json
$launch=Get-Content (Join-Path $root 'Docs/ForestHill/play-racer-launch.json') -Raw | ConvertFrom-Json
$package=Get-Content (Join-Path $builds 'PACKAGE-LATEST.json') -Raw | ConvertFrom-Json
if($published.build -ne 29000 -or !$published.startupReady -or !$published.allLatestFilesMatch -or $launch.version -ne '0.29.0-review1' -or !$launch.responsive -or $package.Version -ne '0.29.0-review1'){throw 'Publication and launcher verification must succeed first'}
function CheckedPath([string]$candidate){
 $full=[IO.Path]::GetFullPath($candidate)
 if(!$full.StartsWith($builds+'\',[StringComparison]::OrdinalIgnoreCase) -and !$full.StartsWith($temp+'\',[StringComparison]::OrdinalIgnoreCase)){throw "Out-of-scope path: $full"}
 $at=$full
 while($at.Length -gt $root.Length){if((Test-Path -LiteralPath $at) -and ((Get-Item -LiteralPath $at -Force).Attributes -band [IO.FileAttributes]::ReparsePoint)){throw "Linked ancestor: $at"};$at=[IO.Path]::GetDirectoryName($at)}
 if(Test-Path -LiteralPath $full -PathType Container){if(Get-ChildItem -LiteralPath $full -Recurse -Force | Where-Object {$_.Attributes -band [IO.FileAttributes]::ReparsePoint}){throw "Linked contents: $full"}}
 return $full
}
$preserved=CheckedPath $package.Preserved
$prior=CheckedPath (Join-Path $preserved 'Latest')
$previous=CheckedPath (Join-Path $builds 'Racer-0.28.0-review1-Windows')
if(!(Get-Content (Join-Path $prior 'VERSION.txt') -Raw).Contains('Racer 0.28.0-review1')){throw 'Unexpected prior build'}
if(Test-Path -LiteralPath $previous){throw 'Previous build destination already exists'}
$oldMusic=Join-Path $prior 'Music';$newMusic=Join-Path $builds 'Latest/Music'
foreach($file in Get-ChildItem -LiteralPath $oldMusic -File -Recurse){$rel=[IO.Path]::GetRelativePath($oldMusic,$file.FullName);$other=Join-Path $newMusic $rel;if(!(Test-Path -LiteralPath $other) -or (Get-FileHash -LiteralPath $file.FullName).Hash -ne (Get-FileHash -LiteralPath $other).Hash){throw "Unique previous music: $rel"}}
# Keep exactly the immediately previous useful build, then remove only known disposable products.
Move-Item -LiteralPath $prior -Destination $previous
$targets=@('PackageWork','Racer-0.27.0-review1-Windows','Racer-0.29.0-review1-Windows','Racer-0.29.0-review1-Windows.zip','LauncherRelease-29000/assets/game.zip') | ForEach-Object {Join-Path $builds $_}
$targets+=$preserved
$targets+=@(Get-ChildItem -LiteralPath $temp -Directory | Where-Object Name -Match '^forest-hill-hosted-\d+$' | ForEach-Object FullName)
$targets+=@(Get-ChildItem -LiteralPath $temp -File -Filter 'ForestHill-original-*.txt' | ForEach-Object FullName)
$verified=@($targets | ForEach-Object {CheckedPath $_} | Where-Object {Test-Path -LiteralPath $_})
$removed=@()
foreach($full in $verified){$item=Get-Item -LiteralPath $full;$bytes=if($item.PSIsContainer){(Get-ChildItem -LiteralPath $full -File -Recurse -Force | Measure-Object Length -Sum).Sum}else{$item.Length};Remove-Item -LiteralPath $full -Recurse -Force;$removed+=[ordered]@{path=$full;bytes=$bytes}}
$remaining=(Get-ChildItem -LiteralPath $builds -Recurse -File -Force | Measure-Object Length -Sum).Sum
if(!(Test-Path -LiteralPath (Join-Path $builds 'Latest/Racer.exe'))){throw 'Latest missing'}
$report=[ordered]@{version='0.29.0-review1';removed=$removed;buildsBytes=$remaining;buildsGiB=[math]::Round($remaining/1GB,3);retained='Latest current build; 0.28 immediately previous build; publisher keys, tools, launcher, SDK and release metadata';preservedMusicVerifiedIdentical=$true}
$report | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $root 'Docs/ForestHill/cleanup.json')
$report | ConvertTo-Json -Depth 5
