function Stop-Unity { Get-Process Unity -ErrorAction SilentlyContinue | Stop-Process -Force; Start-Sleep 6 }
Stop-Unity; & (Join-Path $PSScriptRoot 'Run-Unity.ps1') -Method Report096Batch.LidApply -Scenes FreeRoamWorld -Out lid8 -Minutes 60 -NoGraphics
Stop-Unity; & (Join-Path $PSScriptRoot 'Run-Play.ps1') -Cases "view:FreeRoamWorld:F2,136.8,62.7,61.5,45,16|F2c,143,70,68,45,65|F2e,160,72,50,150,20:day;aijump96:c4-whiteout:1:300;getaway96:road:moto:1;getaway96:hidden:original:1;getaway96:block:atv:1;getaway96:two:moto:1" -Out verify4 -Minutes 150
