Get-Process Unity -ErrorAction SilentlyContinue | Stop-Process -Force; Start-Sleep 6
& (Join-Path $PSScriptRoot 'Run-Play.ps1') -Cases "getaway96:two:moto:1" -Out verify14 -Minutes 60
