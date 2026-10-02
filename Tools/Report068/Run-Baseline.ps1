# 0.68 like-for-like baseline: runs the given drives on the 0.67 Mountain Forward scene (HEAD), then restores the 0.68 scene byte-for-byte.
param([Parameter(Mandatory)][string]$Drives,[string]$Scene='MountainLoop',[string]$Out='drives-baseline067')
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$scenePath=Join-Path $root "Assets/Scenes/$Scene.unity"
$backup=Join-Path $env:LOCALAPPDATA "Temp/report068/$Scene-068.unity"
if(Test-Path -LiteralPath $backup){throw 'Existing scene backup requires inspection'}
Copy-Item -LiteralPath $scenePath -Destination $backup
$hash=(Get-FileHash -LiteralPath $scenePath -Algorithm SHA256).Hash
cmd /c "git -C `"$root`" show HEAD:Assets/Scenes/$Scene.unity > `"$scenePath`""
if($LASTEXITCODE -ne 0){Copy-Item -LiteralPath $backup -Destination $scenePath -Force;throw 'git show failed'}
try{
 $env:PROBE_DRIVE=$Drives
 & (Join-Path $PSScriptRoot 'Run-Unity.ps1') -Method Report068Play.Run -Scenes $Scene -Play Report067DriveChecks -Out $Out -Minutes 40
}finally{
 Copy-Item -LiteralPath $backup -Destination $scenePath -Force
 if((Get-FileHash -LiteralPath $scenePath -Algorithm SHA256).Hash -ne $hash){throw '0.68 scene restore mismatch; backup retained'}
 Remove-Item -LiteralPath $backup
 "0.68 scene restored ($hash)"
}
