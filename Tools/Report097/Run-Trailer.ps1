# Trailer capture (0.97 Part C): runs Report080Checks "cap97:*" cases in the editor (batch mode, graphics on), muted device but the game's own sound recorded.
# -Cases "cap97:test"   output: <project>/Trailer (git-ignored), ffmpeg from Tools/Trailer/pylib (imageio-ffmpeg wheel)
param([Parameter(Mandatory)][string]$Cases,[string]$Out='trailer',[int]$Minutes=240,[int]$Width=3840,[int]$Height=2160)
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$env:PROBE_TRAILER=Join-Path $root 'Trailer'
$env:PROBE_FFMPEG=(Get-ChildItem (Join-Path $root 'Tools/Trailer/pylib/imageio_ffmpeg/binaries') -Filter 'ffmpeg*.exe' | Select-Object -First 1).FullName
$env:PROBE_CAPW=$Width; $env:PROBE_CAPH=$Height
$env:PROBE_CASES=$Cases
& (Join-Path $PSScriptRoot 'Run-Unity.ps1') -Method Report097Play.Run -Scenes StreetLoopGreybox -Play Report080Checks -Out $Out -Minutes $Minutes
