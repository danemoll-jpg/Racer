param([string]$Runtime='Builds/Racer-0.18.0-review1-Windows/Racer.exe',[string]$Tag='final',[Parameter(Mandatory)][string]$Name,[Parameter(Mandatory)][string]$Flags,[int]$Limit=1800,[string]$ReuseSave="",[int]$Width=1280,[int]$Height=720)
$ErrorActionPreference='Stop'
$project=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$exe=[IO.Path]::GetFullPath((Join-Path $project $Runtime))
$evidence=Join-Path $project "Docs/CR101-104/$Tag/$Name"
New-Item -ItemType Directory -Force -Path $evidence | Out-Null
$save=Join-Path $project ('Temp/cr101-'+[Guid]::NewGuid().ToString('N'))
if($ReuseSave){$save=[IO.Path]::GetFullPath($ReuseSave);if(!$save.StartsWith((Join-Path $project "Temp")+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)){throw "Reuse only an isolated project Temp save"}}
New-Item -ItemType Directory -Force -Path $save | Out-Null
# This directory belongs only to this validation process. Preserve all player preferences.
if(!$ReuseSave -and $Flags -notmatch '-racerAudioCheck'){'{"version":1,"master":0,"frameLimit":60}' | Set-Content -LiteralPath (Join-Path $save 'settings.json')}
$titleFlag=if($Flags -match '-titleCheck|-voiceCheck'){''}else{'-racerSkipTitle'}
$arguments="$titleFlag -batchmode -screen-width $Width -screen-height $Height -racerTestSave `"$save`" -evidence `"$evidence`" -logFile `"$project/Logs/cr101-$Tag-$Name.log`" $Flags"
$p=Start-Process -FilePath $exe -ArgumentList $arguments -WorkingDirectory $project -WindowStyle Hidden -PassThru
$peak=0L; $end=(Get-Date).AddSeconds($Limit)
while(!$p.HasExited -and (Get-Date) -lt $end){$peak=[math]::Max($peak,$p.PeakWorkingSet64);Start-Sleep -Milliseconds 500;$p.Refresh()}
$timeout=!$p.HasExited;if($timeout){$p.Kill();$p.WaitForExit()}
$result=[pscustomobject]@{Name=$Name;Runtime=$exe;RuntimeAssemblySHA256=(Get-FileHash -LiteralPath (Join-Path ([IO.Path]::GetDirectoryName($exe)) 'Racer_Data/Managed/Assembly-CSharp.dll')).Hash;Flags=$arguments;PID=$p.Id;Timeout=$timeout;Exit=$p.ExitCode;PeakWorkingSetMiB=[math]::Round($peak/1MB,2);CompletedUtc=[DateTime]::UtcNow.ToString('o')}
$result|ConvertTo-Json|Set-Content -LiteralPath "$evidence/process.json"
$result|ConvertTo-Json





