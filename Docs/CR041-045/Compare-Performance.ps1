$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$results=@()
foreach($period in @('before','after')){
 $folder=if($period -eq 'before'){'Racer-0.6.0-review1-Windows'}else{'Latest'}
 $build=Join-Path $root "Builds/$folder"
 $expected=if($period -eq 'before'){'0.6.0-review1'}else{'0.6.1-review2'}
 if(!(Get-Content "$build/VERSION.txt" -Raw).Contains($expected)){throw "Wrong $period build"}
 $work=Join-Path $root "Temp/correction-performance-$period"
 if(Test-Path $work){throw "Evidence directory exists: $work"}
 New-Item -ItemType Directory -Path "$work/save" -Force | Out-Null
 @{version=1;master=.8;ambience=1;feedback=.65;vehicle=.75;frameLimit=120;vsync=$false;vehicleId='original';difficulty=1;opponents=$true;traffic=$true;opponentChoices=@('tourer','moto','atv');opponentRoster=@('tourer','moto','atv');bodyColors=@(-1,-1,-1,-1)} | ConvertTo-Json | Set-Content "$work/save/settings.json"
 $arguments="-screen-fullscreen 0 -screen-width 1280 -screen-height 720 -woodlandRace -racerDifficulty 1 -woodlandLaps 1 -racerTestSave `"$work/save`" -logFile `"$work/player.log`""
 "Starting comparable $period single-player process" | Write-Output
 $process=Start-Process -FilePath "$build/Racer.exe" -WorkingDirectory $work -ArgumentList $arguments -PassThru
 $peak=0L
 while(!$process.WaitForExit(1000)){$process.Refresh();$peak=[Math]::Max($peak,$process.PeakWorkingSet64)}
 if($process.ExitCode -ne 0){throw "$period exit $($process.ExitCode)"}
 $sourceDir=if($period -eq 'before'){'CR034-039'}else{'CR041-045'}
 $evidence="$work/Docs/$sourceDir/standalone-race-1-clean-traffic"
 $target=Join-Path $PSScriptRoot "performance-$period"
 Copy-Item -LiteralPath $evidence -Destination $target -Recurse
 $text=Get-Content "$target/results.txt" -Raw
 if($text -notmatch 'Final=True'){throw "$period race did not complete"}
 $match=[regex]::Match($text,'median=([0-9.]+)ms; p95=([0-9.]+)ms')
 $results += [pscustomobject]@{Period=$period;Version=$expected;PeakWorkingSetMiB=[Math]::Round($peak/1048576,1);MedianMs=[double]$match.Groups[1].Value;P95Ms=[double]$match.Groups[2].Value;AssemblySHA256=(Get-FileHash "$build/Racer_Data/Managed/Assembly-CSharp.dll").Hash;Settings='One visible standalone, 1280x720, VSync off, 120fps cap, Normal, original player and tourer/moto/ATV, one full lap, traffic on (4 before / 20 after)'}
 $results | ConvertTo-Json -Depth 4 | Set-Content (Join-Path $PSScriptRoot 'performance-comparison.json')
}
$results | Format-Table Period,Version,PeakWorkingSetMiB,MedianMs,P95Ms

