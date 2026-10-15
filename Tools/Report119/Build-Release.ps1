# 0.119: the fresh Windows build from the pushed completion source (Build-Report119.cs.txt as a temporary editor script; removed afterwards)
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$editor=Join-Path $root 'Assets/Editor/Report119Release'
New-Item -ItemType Directory -Force $editor | Out-Null
Copy-Item (Join-Path $PSScriptRoot 'Build-Report119.cs.txt') (Join-Path $editor 'Report119Release.cs')
$env:REPORT119_COMMIT=(git -C $root rev-parse HEAD).Trim()
$log=Join-Path $env:LOCALAPPDATA 'Temp/report119/release-build.log'
$unity='C:\Program Files\Unity\Hub\Editor\6000.6.1f1\Editor\Unity.exe'
$p=Start-Process -FilePath $unity -ArgumentList @('-batchmode','-projectPath',$root,'-acceptSoftwareTermsForThisRunOnly','-executeMethod','Racer.Editor.Report119Release.Main','-logFile',$log) -PassThru
$p.WaitForExit()
"exit=$($p.ExitCode) commit=$env:REPORT119_COMMIT"
