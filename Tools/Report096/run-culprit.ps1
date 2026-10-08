param([string]$Cam,[string]$Target,[string]$Mode='pale',[string]$Out='culprit')
Get-Process Unity -ErrorAction SilentlyContinue | Stop-Process -Force; Start-Sleep 3
$env:CULPRIT_MODE=$Mode
& (Join-Path $PSScriptRoot 'Run-Play.ps1') -Cases "culprit96:${Cam}:${Target}" -Out $Out -Minutes 60
