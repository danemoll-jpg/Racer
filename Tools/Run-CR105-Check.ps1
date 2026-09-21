param([string]$Runtime='Builds/CR105-build-first/Racer.exe',[string]$Tag='first',[Parameter(Mandatory)][string]$Name,[Parameter(Mandatory)][string]$Flags,[int]$Limit=1800,[string]$ReuseSave='')
$ErrorActionPreference='Stop'
$project=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$exe=[IO.Path]::GetFullPath((Join-Path $project $Runtime))
$evidence=Join-Path $project "Docs/CR105-111/$Tag/$Name"
New-Item -ItemType Directory -Force -Path $evidence | Out-Null
$save=Join-Path $project ('Temp/cr105-'+[Guid]::NewGuid().ToString('N'))
if($ReuseSave){$save=[IO.Path]::GetFullPath($ReuseSave);if(!$save.StartsWith((Join-Path $project 'Temp')+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)){throw 'Only isolated Temp saves allowed'}}
New-Item -ItemType Directory -Force -Path $save | Out-Null
if(!$ReuseSave){'{"version":1,"master":0,"frameLimit":60,"vsync":false}' | Set-Content -LiteralPath (Join-Path $save 'settings.json')}
$arguments="-racerSkipTitle -batchmode -screen-width 1280 -screen-height 720 -racerTestSave `"$save`" -evidence `"$evidence`" -logFile `"$project/Logs/cr105-$Tag-$Name.log`" $Flags"
$p=Start-Process -FilePath $exe -ArgumentList $arguments -WorkingDirectory ([IO.Path]::GetDirectoryName($exe)) -WindowStyle Hidden -PassThru
$end=(Get-Date).AddSeconds($Limit)
while(!$p.HasExited -and (Get-Date) -lt $end){Start-Sleep -Milliseconds 500;$p.Refresh()}
$timeout=!$p.HasExited;if($timeout){$p.Kill();$p.WaitForExit()}
$result=[pscustomobject]@{Name=$Name;Runtime=$exe;AssemblySHA256=(Get-FileHash -LiteralPath (Join-Path ([IO.Path]::GetDirectoryName($exe)) 'Racer_Data/Managed/Assembly-CSharp.dll')).Hash;Flags=$arguments;Save=$save;PID=$p.Id;Timeout=$timeout;Exit=$p.ExitCode;CompletedUtc=[DateTime]::UtcNow.ToString('o')}
$result|ConvertTo-Json|Set-Content -LiteralPath "$evidence/process.json"
$result|ConvertTo-Json
