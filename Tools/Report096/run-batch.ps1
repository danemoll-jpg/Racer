param([Parameter(Mandatory)][string]$Method,[Parameter(Mandatory)][string]$Scenes,[string]$Out='batch')
Get-Process Unity -ErrorAction SilentlyContinue | Stop-Process -Force; Start-Sleep 3
& (Join-Path $PSScriptRoot 'Run-Unity.ps1') -Method $Method -Scenes $Scenes -Out $Out -Minutes 120 -NoGraphics
