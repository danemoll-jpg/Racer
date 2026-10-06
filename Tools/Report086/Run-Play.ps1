# Runs Report080Checks (with the 0.86 cases) in editor play mode (muted, isolated save): -Cases "storm:roam-day:30;..."
param([Parameter(Mandatory)][string]$Cases,[string]$Out='play',[int]$Minutes=40,[string]$Scene='StreetLoopGreybox')
$env:PROBE_CASES=$Cases
& (Join-Path $PSScriptRoot 'Run-Unity.ps1') -Method Report086Play.Run -Scenes $Scene -Play Report080Checks -Out $Out -Minutes $Minutes
