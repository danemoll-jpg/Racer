$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$latest=Join-Path $root 'Builds/Latest'
$state=Get-Content (Join-Path $latest 'state.json') -Raw | ConvertFrom-Json
if($state.game.manifest.build -ne 38000 -or $state.game.manifest.version -ne '0.38.0-review1'){throw 'Installed signed version mismatch'}
$expected=[IO.Path]::GetFullPath((Join-Path $latest ($state.game.directory+'/Racer.exe')))
if(!$expected.StartsWith($latest+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Game path outside stable launcher installation'}
$before=@(Get-Process Racer,WoodstockRushLauncher -ErrorAction SilentlyContinue | ForEach-Object Id)
& (Join-Path $root 'Play-Racer.cmd')
$deadline=(Get-Date).AddSeconds(50);$launched=$null
do {
 Start-Sleep -Seconds 2
 $found=@(Get-Process Racer -ErrorAction SilentlyContinue | Where-Object {$_.Id -notin $before -and $_.Path -eq $expected})
 if($found.Count -gt 1){throw 'Ambiguous newly launched game processes'}
 if($found.Count -eq 1){$launched=$found[0];$launched.Refresh()}
}while((!$launched -or !$launched.MainWindowHandle -or !$launched.Responding) -and (Get-Date) -lt $deadline)
if(!$launched -or !$launched.MainWindowHandle -or !$launched.Responding){throw 'New signed release did not present a responsive game window'}
$record=[ordered]@{entryPoint=(Join-Path $root 'Play-Racer.cmd');path=$launched.Path;version='0.38.0-review1';build=38000;pid=$launched.Id;responsive=$launched.Responding;window=$launched.MainWindowTitle;stableLauncherRoot=$latest;futureSignedUpdatesPreserved=$true;verifiedAt=(Get-Date -Format o)}
$record | ConvertTo-Json | Set-Content (Join-Path $root 'Docs/BackyardForward/play-racer-launch.json')
$null=$launched.CloseMainWindow()
if(!$launched.WaitForExit(5000)){$remaining=Get-Process -Id $launched.Id -ErrorAction SilentlyContinue;if($remaining -and $remaining.Path -eq $expected){$remaining.Kill()}}
foreach($launcher in @(Get-Process WoodstockRushLauncher -ErrorAction SilentlyContinue | Where-Object {$_.Id -notin $before -and $_.Path -eq (Join-Path $latest 'WoodstockRushLauncher.exe')})){
 # The launcher processes child-exit on its timer before accepting WM_CLOSE.
 $null=$launcher.CloseMainWindow()
 if(!$launcher.WaitForExit(5000)){$null=$launcher.CloseMainWindow();$null=$launcher.WaitForExit(5000)}
}
$record | ConvertTo-Json
