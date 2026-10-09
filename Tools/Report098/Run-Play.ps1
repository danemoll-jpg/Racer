# Runs Report080Checks (with the 0.98 cases) in editor play mode (muted, isolated save): -Cases "view:MountainLoop:name,x,y,z,yaw,pitch:day;..."
param([Parameter(Mandatory)][string]$Cases,[string]$Out='play',[int]$Minutes=40,[string]$Scene='StreetLoopGreybox')
$env:PROBE_CASES=$Cases
& (Join-Path $PSScriptRoot 'Run-Unity.ps1') -Method Report098Play.Run -Scenes $Scene -Play Report080Checks -Out $Out -Minutes $Minutes
