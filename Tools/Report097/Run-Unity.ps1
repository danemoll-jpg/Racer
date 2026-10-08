# Runs one batch-mode Unity method for the 0.97 round with the temporary editor tools installed.
param([Parameter(Mandatory)][string]$Method,[string]$Scenes='MountainLoopReverse',[string]$Play='',[string]$Out='report097',[int]$Minutes=40,[switch]$NoGraphics)
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$temp=Join-Path $root 'Assets/Editor/Report097Temp'
New-Item -ItemType Directory -Force -Path $temp | Out-Null
@(Get-ChildItem (Join-Path $PSScriptRoot "*.cs")) | ForEach-Object { Copy-Item $_.FullName (Join-Path $temp $_.Name) -Force }
# Unity clears the project Temp folder on exit, so evidence goes to a scratch folder outside the project.
$scratch=if($env:REPORT095_SCRATCH){$env:REPORT095_SCRATCH}else{Join-Path $env:LOCALAPPDATA 'Temp/report097'}
$outFull=[IO.Path]::GetFullPath((Join-Path $scratch $Out)); New-Item -ItemType Directory -Force -Path $outFull | Out-Null
$env:PROBE_OUT=$outFull; $env:PROBE_SCENES=$Scenes; $env:PROBE_PLAY=$Play
$log=Join-Path $outFull ("unity-"+($Method -replace '[^A-Za-z0-9]','_')+".log")
# 0.97: the editor starts on a copy of Dan's save (RaceFlow reads -racerTestSave before anything loads), never on his real one
$dan=Join-Path $env:USERPROFILE 'AppData/LocalLow/DefaultCompany/Racer/Phase7/street-loop-gates-v1-laps3'
$saveCopy=Join-Path $outFull 'save-copy'; if(Test-Path $saveCopy){Remove-Item -Recurse -Force $saveCopy}; New-Item -ItemType Directory -Force $saveCopy | Out-Null
Get-ChildItem $dan -File | Where-Object { $_.Extension -in '.json','.bak' } | ForEach-Object { Copy-Item $_.FullName $saveCopy }
$args=@('-racerTestSave',$saveCopy,'-batchmode','-projectPath',$root,'-acceptSoftwareTermsForThisRunOnly','-executeMethod',$Method,'-logFile',$log)
if($NoGraphics){$args+='-nographics'}
$p=Start-Process -FilePath 'C:\Program Files\Unity\Hub\Editor\6000.6.1f1\Editor\Unity.exe' -ArgumentList $args -PassThru
if(!$p.WaitForExit($Minutes*60000)){Stop-Process -Id $p.Id -Force;"TIMEOUT"}
"exit=$($p.ExitCode) log=$log"
Select-String -Path $log -Pattern 'error CS|Exception|Compilation failed' | Select-Object -First 15 | ForEach-Object { $_.Line }
