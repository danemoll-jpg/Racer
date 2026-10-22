# 1.1.0-review1: the fresh Windows build from the pushed completion source (Build-Report1100.cs.txt as a temporary editor script; removed afterwards)
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$editor=Join-Path $root 'Assets/Editor/Report1100Release'
New-Item -ItemType Directory -Force $editor | Out-Null
Copy-Item (Join-Path $PSScriptRoot 'Build-Report1100.cs.txt') (Join-Path $editor 'Report1100Release.cs')
$env:REPORT1100_COMMIT=(git -C $root rev-parse HEAD).Trim()
$log=Join-Path $env:LOCALAPPDATA 'Temp/report1100/release-build.log'
$unity='C:\Program Files\Unity\Hub\Editor\6000.6.1f1\Editor\Unity.exe'
$p=Start-Process -FilePath $unity -ArgumentList @('-batchmode','-projectPath',$root,'-acceptSoftwareTermsForThisRunOnly','-executeMethod','Racer.Editor.Report1100Release.Main','-logFile',$log) -PassThru
$p.WaitForExit()
"exit=$($p.ExitCode) commit=$env:REPORT1100_COMMIT"
