$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$runtime=Join-Path $root 'Builds/Racer-0.36.0-review1-Windows'
$evidence=Join-Path $root 'Docs/Backyard/runtime-checks'
$saves=Join-Path $root 'Temp/BackyardRuntimeSave'
$null=New-Item -ItemType Directory -Path $evidence,$saves -Force
'{"version":1,"master":0,"radioOn":false,"frameLimit":60,"vsync":false}' | Set-Content (Join-Path $saves 'settings.json')
$commit=(Get-Content (Join-Path $root 'Temp/backyard-source-commit.txt') -Raw).Trim()
if((Get-Content (Join-Path $runtime 'VERSION.txt') -Raw) -notmatch $commit){throw 'Fresh runtime source mismatch'}
$arguments=@('-screen-fullscreen','0','-screen-width','1280','-screen-height','720','-racerTestSave',$saves,'-backyardValidation',$evidence,'-logFile',(Join-Path $evidence 'game.log'))
$p=Start-Process -FilePath (Join-Path $runtime 'Racer.exe') -WorkingDirectory $runtime -ArgumentList $arguments -WindowStyle Hidden -PassThru
if(!$p.WaitForExit(180000)){$p.Kill();throw 'Own runtime fixture timed out'}
$checks=Get-Content (Join-Path $evidence 'done.txt')
if($p.ExitCode -ne 0 -or @($checks | Where-Object {$_ -like 'FAIL *'}).Count -gt 0){throw 'Fresh runtime acceptance failed'}
if(@($checks | Where-Object {$_ -like 'PASS *'}).Count -ne 20){throw 'Expected 20 core/menu checks'}
$checks
