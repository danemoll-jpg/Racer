param([string]$Dry='1',[string]$Tile='',[string]$Points='',[string]$Out='weld3')
$env:WORLD_DRY=$Dry; $env:WORLD_TILE=$Tile; $env:WORLD_POINTS=$Points
& (Join-Path $PSScriptRoot 'Run-Unity.ps1') -Method Report096Weld.Run -Scenes FreeRoamWorld -Out $Out -Minutes 50 -NoGraphics
