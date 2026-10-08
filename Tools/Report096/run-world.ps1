param([Parameter(Mandatory)][string]$Method,[Parameter(Mandatory)][string]$Scenes,[string]$Out='world',[string]$Dry='1',[string]$Fills='',[string]$Tile='',[string]$Points='')
Get-Process Unity -ErrorAction SilentlyContinue | Stop-Process -Force; Start-Sleep 3
$env:WORLD_DRY=$Dry; $env:WORLD_FILLS=$Fills; $env:WORLD_TILE=$Tile; $env:WORLD_POINTS=$Points
& (Join-Path $PSScriptRoot 'Run-Unity.ps1') -Method $Method -Scenes $Scenes -Out $Out -Minutes 90 -NoGraphics
