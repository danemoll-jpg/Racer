function Stop-Unity { Get-Process Unity -ErrorAction SilentlyContinue | Stop-Process -Force; Start-Sleep 6 }
Stop-Unity; & (Join-Path $PSScriptRoot 'Run-Unity.ps1') -Method Report096Batch.FillOnly -Scenes MountainLoop -Out fill7 -Minutes 60 -NoGraphics
Stop-Unity; & (Join-Path $PSScriptRoot 'Run-Unity.ps1') -Method Report096Batch.LidApply -Scenes FreeRoamWorld -Out lid7 -Minutes 60 -NoGraphics
Stop-Unity; & (Join-Path $PSScriptRoot 'Run-Play.ps1') -Cases "aijump96:c4-whiteout:1:300;look96:c4-summit-clock:B6,719.4,88.7,-114.1,127,16|B6b,722,88,-110,150,24|B7,992.1,163.4,92.2,79,16;view:FreeRoamWorld:F2,136.8,62.7,61.5,45,16|F2c,143,70,68,45,65:day" -Out verify3 -Minutes 120
