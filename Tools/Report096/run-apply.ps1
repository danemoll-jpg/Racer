Get-Process Unity -ErrorAction SilentlyContinue | Stop-Process -Force; Start-Sleep 3
& (Join-Path $PSScriptRoot 'Run-Unity.ps1') -Method Report096Batch.Apply -Scenes "MountainLoop,MountainLoopReverse,ForestLoopReverse,LakeWoods,DansBackyardForward,DansBackyardReverse,StreetLoopGreybox,StreetLoopReverse" -Out apply3 -Minutes 120 -NoGraphics
