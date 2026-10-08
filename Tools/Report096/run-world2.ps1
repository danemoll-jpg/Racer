Get-Process Unity -ErrorAction SilentlyContinue | Stop-Process -Force; Start-Sleep 3
& (Join-Path $PSScriptRoot 'Run-Unity.ps1') -Method Report096Batch.FillOnly -Scenes "MountainLoop" -Out fill5 -Minutes 60 -NoGraphics
Get-Process Unity -ErrorAction SilentlyContinue | Stop-Process -Force; Start-Sleep 3
& (Join-Path $PSScriptRoot 'Run-Unity.ps1') -Method Report096Batch.LidApply -Scenes "FreeRoamWorld" -Out lid5 -Minutes 60 -NoGraphics
