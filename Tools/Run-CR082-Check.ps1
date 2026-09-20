param([string]$Runtime='Builds/Racer-0.14.0-review2-Windows/Racer.exe',[string]$Tag='final',[Parameter(Mandatory)][string]$Name,[Parameter(Mandatory)][string]$Flags,[int]$Limit=1800)
$ErrorActionPreference='Stop'
$project=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$exe=[IO.Path]::GetFullPath((Join-Path $project $Runtime))
$evidence=Join-Path $project "Docs/CR082-089/$Tag/$Name"
New-Item -ItemType Directory -Force -Path $evidence | Out-Null
$save=Join-Path $project ('Temp/cr082-'+[Guid]::NewGuid().ToString('N'))
$arguments="-batchmode -screen-width 1280 -screen-height 720 -racerTestSave `"$save`" -evidence `"$evidence`" -logFile `"$project/Logs/cr082-$Tag-$Name.log`" $Flags"
$p=Start-Process -FilePath $exe -ArgumentList $arguments -WorkingDirectory $project -WindowStyle Hidden -PassThru
$end=(Get-Date).AddSeconds($Limit)
while(!$p.HasExited -and (Get-Date) -lt $end){Start-Sleep -Milliseconds 500;$p.Refresh()}
$timeout=!$p.HasExited;if($timeout){$p.Kill();$p.WaitForExit()}
$result=[pscustomobject]@{Name=$Name;Runtime=$exe;RuntimeAssemblySHA256=(Get-FileHash -LiteralPath (Join-Path ([IO.Path]::GetDirectoryName($exe)) 'Racer_Data/Managed/Assembly-CSharp.dll')).Hash;Flags=$arguments;PID=$p.Id;Timeout=$timeout;Exit=$p.ExitCode;CompletedUtc=[DateTime]::UtcNow.ToString('o')}
$result|ConvertTo-Json|Set-Content -LiteralPath "$evidence/process.json"
$result|ConvertTo-Json

