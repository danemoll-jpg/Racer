param([Parameter(Mandatory)][string]$Runtime,[Parameter(Mandatory)][ValidateSet('untouched','advance')][string]$Mode)
$ErrorActionPreference='Stop'
$project=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$exe=[IO.Path]::GetFullPath($Runtime)
$evidence=Join-Path $project "Docs/CR112-118/final/title-$Mode"
$save=Join-Path $project ('Temp/cr118-'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Force -Path $save,$evidence | Out-Null
'{"version":1,"master":0.5,"music":1,"frameLimit":60,"vsync":false}' | Set-Content -LiteralPath (Join-Path $save 'settings.json')
$advance=if($Mode -eq 'advance'){'yes'}else{'no'}
$arguments="-batchmode -screen-width 1280 -screen-height 720 -racerTestSave `"$save`" -racerAudioCheck -cr112Check title -advance $advance -evidence `"$evidence`" -logFile `"$project/Logs/cr118-$Mode.log`""
$p=Start-Process -FilePath $exe -ArgumentList $arguments -WorkingDirectory ([IO.Path]::GetDirectoryName($exe)) -WindowStyle Hidden -PassThru
$end=(Get-Date).AddSeconds(30)
while(!$p.HasExited -and (Get-Date) -lt $end){Start-Sleep -Milliseconds 250;$p.Refresh()}
$timeout=!$p.HasExited;if($timeout){$p.Kill();$p.WaitForExit()}
[pscustomobject]@{Mode=$Mode;Runtime=$exe;AssemblySHA256=(Get-FileHash -LiteralPath (Join-Path ([IO.Path]::GetDirectoryName($exe)) 'Racer_Data/Managed/Assembly-CSharp.dll')).Hash;Arguments=$arguments;Timeout=$timeout;Exit=$p.ExitCode}|ConvertTo-Json|Set-Content -LiteralPath "$evidence/process.json"
Get-Content -LiteralPath "$evidence/checks.txt" -ErrorAction SilentlyContinue
