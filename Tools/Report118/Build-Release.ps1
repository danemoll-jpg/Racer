# 0.118: the fresh Windows build from the pushed completion source (Build-Report118.cs.txt as a temporary editor script; removed afterwards)
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$editor=Join-Path $root 'Assets/Editor/Report118Release'
New-Item -ItemType Directory -Force $editor | Out-Null
Copy-Item (Join-Path $PSScriptRoot 'Build-Report118.cs.txt') (Join-Path $editor 'Report118Release.cs')
$env:REPORT118_COMMIT=(git -C $root rev-parse HEAD).Trim()
$log=Join-Path $env:LOCALAPPDATA 'Temp/report118/release-build.log'
$unity='C:\Program Files\Unity\Hub\Editor\6000.6.1f1\Editor\Unity.exe'
$p=Start-Process -FilePath $unity -ArgumentList @('-batchmode','-projectPath',$root,'-acceptSoftwareTermsForThisRunOnly','-executeMethod','Racer.Editor.Report118Release.Main','-logFile',$log) -PassThru
$p.WaitForExit()
"exit=$($p.ExitCode) commit=$env:REPORT118_COMMIT"
