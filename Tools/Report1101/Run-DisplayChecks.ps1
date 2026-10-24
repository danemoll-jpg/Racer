# 1.1.0 display checks in the check build: the pages at 1280x800 and (for comparison) 1280x720, the Screen mode option by the controller,
# and two starts with deliberately stale Unity screen values in the registry (Dan's values are exported first and imported back afterwards):
#   startup-stale-window    Unity's saved mode Windowed 1024x768, the game's setting the default (Full screen): it must open full screen, native size
#   startup-big-window      Unity's saved mode Windowed at the display's full size, the game's setting Windowed: the window must come out smaller than the display
param([switch]$SkipPages)
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$run=Join-Path $PSScriptRoot 'Run-Player.ps1'
$scratch=Join-Path $env:LOCALAPPDATA 'Temp/report1101'
$key='HKCU\Software\DefaultCompany\Racer'; $ps='HKCU:\Software\DefaultCompany\Racer'
$backup=Join-Path $scratch 'registry-before.reg'
if(!$SkipPages){
 & $run -Out 'pages-1610' -Width 1280 -Height 800 -PlayerArgs '-p106kind display1101 -p1101part pages -p106w 1280 -p106h 800' -Minutes 15
 & $run -Out 'pages-169' -Width 1280 -Height 720 -PlayerArgs '-p106kind display1101 -p1101part pages -p106w 1280 -p106h 720' -Minutes 15
}
& $run -Out 'toggle' -Width 1280 -Height 800 -PlayerArgs '-p106kind display1101 -p1101part toggle -p106w 1280 -p106h 800' -Minutes 8
reg export $key $backup /y | Out-Null
try{
 function Stale($mode,$w,$h){ foreach($n in @{'Screenmanager Fullscreen mode_h3630240806'=$mode;'Screenmanager Resolution Width_h182942802'=$w;'Screenmanager Resolution Height_h2627697771'=$h;'Screenmanager Resolution Window Width_h2524650974'=$w;'Screenmanager Resolution Window Height_h1684712807'=$h;'Screenmanager Resolution Use Native_h1405027254'=0}.GetEnumerator()){ Set-ItemProperty -Path $ps -Name $n.Key -Value $n.Value -Type DWord } }
 Stale 3 1024 768
 & $run -Out 'startup-stale-window' -NoScreenArgs -PlayerArgs '-p106kind display1101 -p1101part startup' -Minutes 6
 Add-Type -AssemblyName System.Windows.Forms
 $native=(Get-CimInstance Win32_VideoController | Where-Object CurrentHorizontalResolution | Select-Object -First 1)
 Stale 3 $native.CurrentHorizontalResolution $native.CurrentVerticalResolution
 # a test save whose Screen mode is Windowed (a copy of Dan's save files, edited; never his own)
 $save=Join-Path $scratch 'windowed-save'; if(Test-Path $save){Remove-Item -Recurse -Force $save}; New-Item -ItemType Directory -Force $save | Out-Null
 $dan=Join-Path $env:USERPROFILE 'AppData/LocalLow/DefaultCompany/Racer/Phase7/street-loop-gates-v1-laps3'
 Get-ChildItem $dan -File | Where-Object { $_.Extension -in '.json','.bak' } | ForEach-Object { Copy-Item $_.FullName $save }
 $s=Get-Content (Join-Path $save 'settings.json') -Raw | ConvertFrom-Json; $s | Add-Member -NotePropertyName windowed -NotePropertyValue $true -Force
 $s | ConvertTo-Json -Depth 20 | Set-Content (Join-Path $save 'settings.json') -Encoding utf8
 & $run -Out 'startup-big-window' -NoScreenArgs -SaveDir $save -PlayerArgs '-p106kind display1101 -p1101part startup' -Minutes 6
}finally{
 reg import $backup 2>$null
 "registry restored from $backup"
}
