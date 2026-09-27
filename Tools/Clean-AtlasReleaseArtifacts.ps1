$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$builds=Join-Path $root 'Builds';$temp=Join-Path $root 'Temp';$latest=Join-Path $builds 'Latest'
$published=Get-Content (Join-Path $root 'Docs/RouteAtlas/hosted/result.json') -Raw|ConvertFrom-Json
$launch=Get-Content (Join-Path $root 'Docs/RouteAtlas/play-racer-launch.json') -Raw|ConvertFrom-Json
$package=Get-Content (Join-Path $builds 'PACKAGE-LATEST.json') -Raw|ConvertFrom-Json
Copy-Item -LiteralPath (Join-Path $builds 'PACKAGE-LATEST.json') -Destination (Join-Path $root 'Docs/RouteAtlas/package-verification.json')
$state=Get-Content (Join-Path $latest 'state.json') -Raw|ConvertFrom-Json
if($published.build -ne 30000 -or !$published.startupReady -or !$launch.responsive -or $launch.build -ne 30000 -or $package.Version -ne '0.30.0-review1' -or $state.game.manifest.build -ne 30000){throw 'Verified publication, package and launcher required before cleanup'}
function CheckedPath([string]$candidate){
 $full=[IO.Path]::GetFullPath($candidate)
 if(!$full.StartsWith($builds+'\',[StringComparison]::OrdinalIgnoreCase) -and !$full.StartsWith($temp+'\',[StringComparison]::OrdinalIgnoreCase)){throw "Out-of-scope path $full"}
 foreach($protected in @('PublisherPrivate','PublisherTools','Launcher','LauncherSDK','LauncherPrototype')){if($full -eq (Join-Path $builds $protected) -or $full.StartsWith((Join-Path $builds $protected)+'\',[StringComparison]::OrdinalIgnoreCase)){throw "Protected target $full"}}
 $at=$full;while($at.Length -gt $root.Length){if((Test-Path -LiteralPath $at) -and ((Get-Item -LiteralPath $at -Force).Attributes -band [IO.FileAttributes]::ReparsePoint)){throw "Linked ancestor $at"};$at=[IO.Path]::GetDirectoryName($at)}
 if(Test-Path -LiteralPath $full -PathType Container){if(Get-ChildItem -LiteralPath $full -Recurse -Force|Where-Object {$_.Attributes -band [IO.FileAttributes]::ReparsePoint}){throw "Linked content $full"}}
 return $full
}
$before=(Get-ChildItem -LiteralPath $builds -File -Recurse -Force|Measure-Object Length -Sum).Sum
$preserved=CheckedPath $package.Preserved;$prior=CheckedPath (Join-Path $preserved 'Latest');$previous=CheckedPath (Join-Path $builds 'Racer-0.29.0-review1-Windows')
if(!(Get-Content (Join-Path $prior 'VERSION.txt') -Raw).Contains('Racer 0.29.0-review1')){throw 'Unexpected previous build'}
if(Test-Path -LiteralPath $previous){throw 'Previous destination already exists; inspect instead of overwriting'}
foreach($file in Get-ChildItem -LiteralPath (Join-Path $prior 'Music') -File -Recurse){$rel=[IO.Path]::GetRelativePath((Join-Path $prior 'Music'),$file.FullName);$other=Join-Path $latest ('Music/'+$rel);if(!(Test-Path -LiteralPath $other) -or (Get-FileHash -LiteralPath $file.FullName).Hash -ne (Get-FileHash -LiteralPath $other).Hash){throw "Unique previous music $rel"}}
Move-Item -LiteralPath $prior -Destination $previous
$targets=@('PackageWork','Racer-0.28.0-review1-Windows','Racer-0.30.0-review1-Windows','Racer-0.30.0-review1-Windows.zip','LauncherRelease-30000/assets/game.zip')|ForEach-Object {Join-Path $builds $_}
$targets+=$preserved
$oldArchiveCheck=Get-Content (Join-Path $root 'Docs/RouteAtlas/old-backup-comparison.json') -Raw|ConvertFrom-Json
if($oldArchiveCheck.files -eq 451 -and @($oldArchiveCheck.mismatches).Count -eq 0 -and $oldArchiveCheck.retainedSource -eq $package.Preserved){$targets+=Join-Path $builds 'WoodstockRush-0.29.0-review1-FullBackup.zip'}
$targets+=@(Get-ChildItem -LiteralPath $temp -Directory|Where-Object Name -Match '^route-atlas-(hosted-\d+|launcher-starter)$'|ForEach-Object FullName)
# After standard launcher migration, the signed current runtime lives in versions/30000.
# Verify redundant flat engine/runtime files against that signed inventory before removing them.
$active=CheckedPath (Join-Path $latest $state.game.directory)
foreach($backup in @(Get-ChildItem -LiteralPath $latest -Directory -Filter 'LauncherMigrationBackup-*')){
 $contents=@(Get-ChildItem -LiteralPath $backup.FullName -File -Recurse)
 if($contents.Count -eq 1 -and $contents[0].Name -eq 'Racer.exe' -and (Get-FileHash -LiteralPath $contents[0].FullName).Hash -eq (Get-FileHash -LiteralPath (Join-Path $active 'Racer.exe')).Hash){$targets+=$backup.FullName}
}
foreach($name in @('Racer_Data','MonoBleedingEdge','D3D12','UnityPlayer.dll','UnityCrashHandler64.exe','dstorage.dll','dstoragecore.dll')){
 $flat=CheckedPath (Join-Path $latest $name);if(!(Test-Path -LiteralPath $flat)){continue}
 $files=if(Test-Path -LiteralPath $flat -PathType Container){@(Get-ChildItem -LiteralPath $flat -File -Recurse)}else{@(Get-Item -LiteralPath $flat)}
 foreach($file in $files){$rel=[IO.Path]::GetRelativePath($latest,$file.FullName).Replace('\','/');$item=@($state.game.manifest.files|Where-Object path -eq $rel);if($item.Count -ne 1 -or (Get-FileHash -LiteralPath $file.FullName).Hash.ToLowerInvariant() -ne $item[0].sha256 -or (Get-FileHash -LiteralPath (Join-Path $active $rel)).Hash.ToLowerInvariant() -ne $item[0].sha256){throw "Flat runtime is not an exact signed duplicate: $rel"}}
 $targets+=$flat
}
# Keep original music, all launcher metadata, root bootstrap, atlas deliverables and publisher identity.
$removed=@();foreach($full in @($targets|ForEach-Object {CheckedPath $_}|Where-Object {Test-Path -LiteralPath $_})){$item=Get-Item -LiteralPath $full;$bytes=if($item.PSIsContainer){(Get-ChildItem -LiteralPath $full -File -Recurse -Force|Measure-Object Length -Sum).Sum}else{$item.Length};Remove-Item -LiteralPath $full -Recurse -Force;$removed+=[ordered]@{path=$full;bytes=$bytes}}
$after=(Get-ChildItem -LiteralPath $builds -File -Recurse -Force|Measure-Object Length -Sum).Sum
if(!(Test-Path -LiteralPath (Join-Path $active 'Racer.exe')) -or !(Test-Path -LiteralPath (Join-Path $builds 'PublisherPrivate/launcher-signing.pem'))){throw 'Required retained files missing'}
$report=[ordered]@{version='0.30.0-review1';beforeBytes=$before;afterBytes=$after;beforeGiB=[math]::Round($before/1GB,3);afterGiB=[math]::Round($after/1GB,3);removed=$removed;current=$active;previous=$previous;retained='PublisherPrivate/key, PublisherTools, Launcher/SDK, current signed install and all metadata, prior 0.29 build, source/Git/Docs/atlas/BundleMusic, user saves and intentionally retained music';preservedMusicVerifiedIdentical=$true}
$report|ConvertTo-Json -Depth 6|Set-Content (Join-Path $root 'Docs/RouteAtlas/cleanup.json');$report|ConvertTo-Json -Depth 6
$package.Runtime=$active;$package.ZIP=$null;$package.Extracted=$null;$package.Preserved=$previous
$package|Add-Member NoteProperty PublishedRelease 'https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-30000'
$package|Add-Member NoteProperty Cleanup 'Disposable archives/staging removed after public signature/download/startup and Play-Racer verification. Original package report retained in Docs/RouteAtlas/package-verification.json.'
$package|ConvertTo-Json -Depth 6|Set-Content (Join-Path $builds 'PACKAGE-LATEST.json')
