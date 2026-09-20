param([string]$Runtime='Builds/Racer-0.15.0-review1-Windows/Racer.exe',[string]$Tag='final',[Parameter(Mandatory)][string]$Name,[Parameter(Mandatory)][string]$Flags,[int]$Limit=1800)
$ErrorActionPreference='Stop'
$project=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$exe=[IO.Path]::GetFullPath((Join-Path $project $Runtime))
$evidence=Join-Path $project "Docs/CR081-090/$Tag/$Name"
New-Item -ItemType Directory -Force -Path $evidence | Out-Null
$save=Join-Path $project ('Temp/cr081-'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Force -Path $save | Out-Null
# This directory belongs only to this validation process. Preserve all player preferences.
if($Flags -notmatch '-racerAudioCheck'){'{"version":1,"master":0,"frameLimit":60}' | Set-Content -LiteralPath (Join-Path $save 'settings.json')}
$arguments="-batchmode -screen-width 1280 -screen-height 720 -racerTestSave `"$save`" -evidence `"$evidence`" -logFile `"$project/Logs/cr081-$Tag-$Name.log`" $Flags"
$p=Start-Process -FilePath $exe -ArgumentList $arguments -WorkingDirectory $project -WindowStyle Hidden -PassThru
$end=(Get-Date).AddSeconds($Limit)
while(!$p.HasExited -and (Get-Date) -lt $end){Start-Sleep -Milliseconds 500;$p.Refresh()}
$timeout=!$p.HasExited;if($timeout){$p.Kill();$p.WaitForExit()}
$result=[pscustomobject]@{Name=$Name;Runtime=$exe;RuntimeAssemblySHA256=(Get-FileHash -LiteralPath (Join-Path ([IO.Path]::GetDirectoryName($exe)) 'Racer_Data/Managed/Assembly-CSharp.dll')).Hash;Flags=$arguments;PID=$p.Id;Timeout=$timeout;Exit=$p.ExitCode;CompletedUtc=[DateTime]::UtcNow.ToString('o')}
$result|ConvertTo-Json|Set-Content -LiteralPath "$evidence/process.json"
$result|ConvertTo-Json


