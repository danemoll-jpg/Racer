param([Parameter(Mandatory)][string]$Starter,[Parameter(Mandatory)][string]$ExistingGameDirectory)
$ErrorActionPreference='Stop'
$source=[IO.Path]::GetFullPath($Starter)
$target=[IO.Path]::GetFullPath($ExistingGameDirectory)
if($source -eq $target -or $target.StartsWith($source+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)){throw 'Choose the existing portable game folder, outside the starter.'}
function Assert-NoLinks([string]$Path){
 $item=Get-Item -LiteralPath $Path -ErrorAction Stop
 if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw "Linked path not supported: $Path"}
 if($item.PSIsContainer){foreach($child in Get-ChildItem -LiteralPath $Path -Force){Assert-NoLinks $child.FullName}}
}
Assert-NoLinks $source
Assert-NoLinks $target
foreach($name in @('WoodstockRushLauncher.exe','state.json','versions','shared','game-manifest.json','soundtrack-manifest.json')){if(!(Test-Path -LiteralPath (Join-Path $source $name))){throw "Incomplete starter: $name"}}
if(!(Test-Path -LiteralPath "$target/Racer.exe")){throw 'Existing Racer.exe required to preserve its shortcut path.'}
if(Test-Path -LiteralPath "$target/state.json"){throw 'Launcher state already exists; do not overwrite an installation.'}
if(Get-Process Racer,WoodstockRushLauncher -ErrorAction SilentlyContinue){throw 'Close the game and launcher before migration.'}
foreach($item in Get-ChildItem -LiteralPath $source -Force){if($item.Name -ne 'WoodstockRush-Cover.png' -and (Test-Path -LiteralPath (Join-Path $target $item.Name))){throw "Migration would overwrite $($item.Name); inspect before changing the existing installation."}}
$backup=Join-Path $target ('LauncherMigrationBackup-'+(Get-Date -Format 'yyyyMMdd-HHmmss'))
New-Item -ItemType Directory -Path $backup | Out-Null
Copy-Item -LiteralPath "$target/Racer.exe" -Destination "$backup/Racer.exe"
$oldHash=(Get-FileHash -LiteralPath "$target/Racer.exe").Hash
if((Get-FileHash -LiteralPath "$backup/Racer.exe").Hash -ne $oldHash){throw 'Executable backup failed verification.'}
$state=Get-Content -LiteralPath "$source/state.json" -Raw | ConvertFrom-Json
$owned=@{}
foreach($file in $state.music.manifest.files){$owned[$file.path]=$file.sha256}
foreach($item in Get-ChildItem -LiteralPath $source -Force){
 $dest=Join-Path $target $item.Name
 if($item.Name -eq 'WoodstockRush-Cover.png' -and (Test-Path -LiteralPath $dest)){continue}
 Copy-Item -LiteralPath $item.FullName -Destination $dest -Recurse
}
$legacy=Join-Path $target 'Music'
if(Test-Path -LiteralPath $legacy){foreach($file in Get-ChildItem -LiteralPath $legacy -File -Recurse){
 $relative=[IO.Path]::GetRelativePath($legacy,$file.FullName).Replace('\','/')
 $hash=(Get-FileHash -LiteralPath $file.FullName).Hash.ToLowerInvariant()
 if($owned.ContainsKey($relative) -and $owned[$relative] -eq $hash){continue}
 $dest=Join-Path "$target/shared/PersonalMusic" $relative
 if(Test-Path -LiteralPath $dest){throw "Personal migration conflict: $relative. Original retained in Music."}
 New-Item -ItemType Directory -Force ([IO.Path]::GetDirectoryName($dest)) | Out-Null
 Copy-Item -LiteralPath $file.FullName -Destination $dest
}}
# Last step keeps the exact executable path used by the existing Steam shortcut.
Copy-Item -LiteralPath "$target/WoodstockRushLauncher.exe" -Destination "$target/Racer.exe" -Force
if((Get-FileHash -LiteralPath "$target/Racer.exe").Hash -ne (Get-FileHash -LiteralPath "$target/WoodstockRushLauncher.exe").Hash){throw 'Bootstrap copy verification failed; restore backed-up Racer.exe.'}
Write-Host "Installed at the existing Racer.exe path. Original retained at $backup. Saves and Steam shortcut unchanged. Confirm existing progress before accepting migration."
