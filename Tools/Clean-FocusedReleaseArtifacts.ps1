$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$builds=Join-Path $root 'Builds'
$temp=Join-Path $root 'Temp'
$published=Get-Content (Join-Path $root 'Docs/FocusedRecovery/hosted/result.json') -Raw | ConvertFrom-Json
$launch=Get-Content (Join-Path $root 'Docs/FocusedRecovery/play-racer-launch.json') -Raw | ConvertFrom-Json
if($published.build -ne 28000 -or !$published.startupReady -or !$published.allLatestFilesMatch -or $launch.version -ne '0.28.0-review1' -or !$launch.responsive){throw 'Publication and Play-Racer verification must succeed first'}
$preserved=Join-Path $builds 'Preserved/20260922-171854-698-ee67ac'
$oldMusic=Join-Path $preserved 'Latest/Music'
$newMusic=Join-Path $builds 'Latest/Music'
if(Test-Path -LiteralPath $oldMusic){
 foreach($file in Get-ChildItem -LiteralPath $oldMusic -File -Recurse){
  $relative=[IO.Path]::GetRelativePath($oldMusic,$file.FullName);$current=Join-Path $newMusic $relative
  if(!(Test-Path -LiteralPath $current) -or (Get-FileHash -LiteralPath $file.FullName).Hash -ne (Get-FileHash -LiteralPath $current).Hash){throw "Preserved music is unique: $relative. Ask before deleting."}
 }
}
$targets=@('PackageWork','Preserved/20260922-171854-698-ee67ac','Package','Verified','Racer-0.28.0-review1-Windows','Racer-0.28.0-review1-Windows.zip','Racer-0.27.0-review1-Windows.zip','LauncherRelease-28000/assets/game.zip','LauncherRelease-27000/assets/game.zip') | ForEach-Object {Join-Path $builds $_}
$targets+=@(Get-ChildItem -LiteralPath $temp -Directory | Where-Object Name -Match '^(focused-recovery|simple-laurel|surgical-tracks|local-recovery|mountain-menu|cr133)-hosted-\d+$' | ForEach-Object FullName)
$verified=@()
foreach($candidate in $targets){
 $full=[IO.Path]::GetFullPath($candidate)
 if(!$full.StartsWith($builds+'\',[StringComparison]::OrdinalIgnoreCase) -and !$full.StartsWith($temp+'\',[StringComparison]::OrdinalIgnoreCase)){throw "Out-of-scope target: $full"}
 if(!(Test-Path -LiteralPath $full)){continue}
 $at=$full
 while($at.Length -gt $root.Length){if((Get-Item -LiteralPath $at -Force).Attributes -band [IO.FileAttributes]::ReparsePoint){throw "Linked ancestor: $at"};$at=[IO.Path]::GetDirectoryName($at)}
 if((Get-Item -LiteralPath $full).PSIsContainer){if(Get-ChildItem -LiteralPath $full -Recurse -Force | Where-Object {$_.Attributes -band [IO.FileAttributes]::ReparsePoint}){throw "Linked contents: $full"}}
 $verified+=$full
}
# Every resolved target and descendant has been checked before any deletion.
$removed=@()
foreach($full in $verified){$size=(Get-ChildItem -LiteralPath $full -Recurse -File -Force | Measure-Object Length -Sum).Sum;if(Test-Path -LiteralPath $full -PathType Leaf){$size=(Get-Item -LiteralPath $full).Length};Remove-Item -LiteralPath $full -Recurse -Force;$removed+=[ordered]@{path=$full;bytes=$size}}
$remaining=(Get-ChildItem -LiteralPath $builds -Recurse -File -Force | Measure-Object Length -Sum).Sum
if(!(Test-Path -LiteralPath (Join-Path $builds 'Latest/Racer.exe'))){throw 'Latest was lost'}
$report=[ordered]@{version='0.28.0-review1';removed=$removed;buildsBytes=$remaining;buildsGiB=[math]::Round($remaining/1GB,3);retained='Latest current build; 0.27 previous build; launcher/SDK/tools/private key and all release metadata';preservedMusicVerifiedIdentical=$true}
$report | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $root 'Docs/FocusedRecovery/cleanup.json')
$report | ConvertTo-Json -Depth 5
