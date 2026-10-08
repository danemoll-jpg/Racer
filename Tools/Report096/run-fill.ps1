Get-Process Unity -ErrorAction SilentlyContinue | Stop-Process -Force; Start-Sleep 3
& (Join-Path $PSScriptRoot 'Run-Unity.ps1') -Method Report096Batch.FillOnly -Scenes "MountainLoop" -Out fill4 -Minutes 60 -NoGraphics
