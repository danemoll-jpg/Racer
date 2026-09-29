$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$exe=Join-Path $root 'Builds/Racer-0.38.0-review1-Windows/Racer.exe'
$saves=Join-Path $root 'Temp/ForwardRuntimeSave'
$evidence=Join-Path $root 'Docs/BackyardForward/runtime-checks'
if(!(Test-Path (Join-Path $root 'Docs/BackyardForward/build-done.txt'))){throw 'Fresh build required'}
if(Test-Path (Join-Path $evidence 'done.txt')){throw 'Runtime check already complete; inspect before repeating'}
New-Item -ItemType Directory -Force -Path $saves,$evidence | Out-Null
@{version=1;master=0;radioOn=$false;frameLimit=60;vsync=$false} | ConvertTo-Json | Set-Content (Join-Path $saves 'settings.json')
$arguments=@('-batchmode','-screen-fullscreen','0','-screen-width','1280','-screen-height','720','-racerTestSave',('"'+$saves+'"'),'-backyardForwardCheck',('"'+$evidence+'"'),'-logFile',('"'+(Join-Path $evidence 'game.log')+'"'))
$process=Start-Process -FilePath $exe -ArgumentList $arguments -WorkingDirectory (Split-Path $exe) -WindowStyle Hidden -PassThru
$process.Id | Set-Content (Join-Path $evidence 'process-id.txt')
"Started isolated fresh-player Forward check, PID $($process.Id)"
