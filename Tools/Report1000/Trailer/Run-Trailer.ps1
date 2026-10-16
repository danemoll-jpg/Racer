# 1.0 trailer capture: the 0.97 offline method (Report080Checks "cap97:*" / "cap100:*" cases in the editor's play mode, the 1.0.0 source;
# never in the released menus). Batch mode with graphics unless -Windowed. Output: <project>/Trailer (git-ignored). ffmpeg from Tools/Trailer/pylib.
param([Parameter(Mandatory)][string]$Cases,[string]$Out='trailer',[int]$Minutes=240,[int]$Width=3840,[int]$Height=2160,[switch]$Windowed)
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../..'))
$env:PROBE_TRAILER=Join-Path $root 'Trailer'
$env:PROBE_FFMPEG=(Get-ChildItem (Join-Path $root 'Tools/Trailer/pylib/imageio_ffmpeg/binaries') -Filter 'ffmpeg*.exe' | Select-Object -First 1).FullName
$env:PROBE_CAPW=$Width; $env:PROBE_CAPH=$Height
$env:PROBE_CASES=$Cases
& (Join-Path $PSScriptRoot 'Run-Unity.ps1') -Method Report097Play.Run -Scenes StreetLoopGreybox -Play Report080Checks -Out $Out -Minutes $Minutes -Windowed:$Windowed -AudioCheck
