# 0.93 Part A: runs MenuNavCheck in a built player on a copy of Dan's save (campaign in progress), muted, windowed.
param([string]$Player="$env:LOCALAPPDATA\Temp\report093\build-test\Racer.exe",[string]$Out="$env:LOCALAPPDATA\Temp\report093\nav",[int]$Width=1920,[int]$Height=1080,[string]$Full="0",[string[]]$Extra=@())
$ErrorActionPreference='Stop'
$dan=Join-Path $env:USERPROFILE 'AppData/LocalLow/DefaultCompany/Racer/Phase7/street-loop-gates-v1-laps3'
if(Test-Path $Out){Remove-Item -Recurse -Force $Out}
$save=Join-Path $Out 'save'; New-Item -ItemType Directory -Force $save | Out-Null
foreach($f in 'campaign-v1.json','settings.json','race-playlists-v1.json','activities-v1.json','exploration-map-woodstock-world-v2.json'){ if(Test-Path (Join-Path $dan $f)){Copy-Item (Join-Path $dan $f) $save} }
$s=Get-Content (Join-Path $save 'settings.json') -Raw | ConvertFrom-Json; $s.master=0; $s.hints=$false; ($s|ConvertTo-Json -Depth 30) | Set-Content (Join-Path $save 'settings.json')
$a=@('-menuNavCheck',"$Out\run",'-racerTestSave',$save,'-screen-fullscreen',$Full,'-screen-width',$Width,'-screen-height',$Height,'-logFile',"$Out\player.log")+$Extra
$p=Start-Process $Player -ArgumentList $a -PassThru; if(!$p.WaitForExit(900000)){Stop-Process -Id $p.Id -Force; "TIMEOUT"}
Get-Content "$Out\run\menunav.txt"
