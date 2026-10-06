# Runs one batch-mode Unity method for the 0.82 round with the temporary editor tools installed.
param([Parameter(Mandatory)][string]$Method,[string]$Scenes='MountainLoopReverse',[string]$Play='',[string]$Out='report082',[int]$Minutes=40,[switch]$NoGraphics)
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$temp=Join-Path $root 'Assets/Editor/Report082Temp'
New-Item -ItemType Directory -Force -Path $temp | Out-Null
Get-ChildItem (Join-Path $PSScriptRoot '*.cs') | ForEach-Object { Copy-Item $_.FullName (Join-Path $temp $_.Name) -Force }
# Unity clears the project Temp folder on exit, so evidence goes to a scratch folder outside the project.
$scratch=if($env:REPORT082_SCRATCH){$env:REPORT082_SCRATCH}else{Join-Path $env:LOCALAPPDATA 'Temp/report082'}
$outFull=[IO.Path]::GetFullPath((Join-Path $scratch $Out)); New-Item -ItemType Directory -Force -Path $outFull | Out-Null
$env:PROBE_OUT=$outFull; $env:PROBE_SCENES=$Scenes; $env:PROBE_PLAY=$Play
$log=Join-Path $outFull ("unity-"+($Method -replace '[^A-Za-z0-9]','_')+".log")
$args=@('-batchmode','-projectPath',$root,'-acceptSoftwareTermsForThisRunOnly','-executeMethod',$Method,'-logFile',$log)
if($NoGraphics){$args+='-nographics'}
$p=Start-Process -FilePath 'C:\Program Files\Unity\Hub\Editor\6000.6.1f1\Editor\Unity.exe' -ArgumentList $args -PassThru
if(!$p.WaitForExit($Minutes*60000)){Stop-Process -Id $p.Id -Force;"TIMEOUT"}
"exit=$($p.ExitCode) log=$log"
Select-String -Path $log -Pattern 'error CS|Exception|Compilation failed' | Select-Object -First 15 | ForEach-Object { $_.Line }
