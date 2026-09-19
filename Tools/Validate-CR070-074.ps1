param([string]$Version='0.12.0-review1',[string]$StartAt='rules-street',[string]$EvidenceTag='final',[string]$Only='',[int]$Repeats=1,[int]$Speed=3)
$ErrorActionPreference='Stop'
$project=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$exe=Join-Path $project "Builds/Racer-$Version-Windows/Racer.exe"
$evidence=Join-Path $project ('Docs/CR070-074/'+$EvidenceTag)
New-Item -ItemType Directory -Force $evidence | Out-Null
$runId=[Guid]::NewGuid().ToString("N").Substring(0,8);$rows=[Collections.Generic.List[object]]::new();$script:started=$false
function Run([string]$name,[string]$flags,[int]$seconds=180,[string]$save=''){
 if($Only -and $name -notin $Only.Split(',')){return}
 if(!$script:started){if($name -ne $StartAt){return};$script:started=$true}
 if(!$save){$save="Temp/cr7074-$EvidenceTag-$runId-$name-save"}
 $args="-batchmode -screen-width 1280 -screen-height 720 -racerTestSave `"$project/$save`" -evidence `"$evidence/$name`" -logFile `"$project/Logs/cr7074-$name.log`" $flags"
 $p=Start-Process -FilePath $exe -ArgumentList $args -WorkingDirectory $project -WindowStyle Hidden -PassThru
 Write-Output "START $name pid=$($p.Id)"
 $peakMemory=0L;$end=(Get-Date).AddSeconds($seconds)
 while(!$p.HasExited -and (Get-Date) -lt $end){Start-Sleep -Milliseconds 500;$p.Refresh();if(!$p.HasExited){$peakMemory=[Math]::Max($peakMemory,$p.WorkingSet64)}}
 if(!$p.HasExited){$p.Kill();$p.WaitForExit()}
 $rows.Add([pscustomobject]@{Name=$name;Exit=$p.ExitCode;PeakWorkingSetMiB=[Math]::Round($peakMemory/1MB,2);Flags=$flags;Finished=(Get-Date -Format o)})
 $rows | ConvertTo-Json | Set-Content "$evidence/standalone-$StartAt.json"
 Write-Output "END $name exit=$($p.ExitCode)"
}
Run 'rules-street' '-review7074 rules'
Run 'rules-forest' '-review7074 rules -course LakeWoods'
Run 'rules-street-reverse' '-review7074 rules -course StreetLoopReverse'
Run 'rules-forest-reverse' '-review7074 rules -course ForestLoopReverse'
Run 'physical-street' '-review7074 physical'
Run 'physical-forest' '-review7074 physical -course LakeWoods'
Run 'physical-street-reverse' '-review7074 physical -course StreetLoopReverse'
Run 'physical-forest-reverse' '-review7074 physical -course ForestLoopReverse'
Run 'hairpin-street' '-review7074 hairpin' 360
Run 'hairpin-street-reverse' '-review7074 hairpin -course StreetLoopReverse' 360
Run 'layout-street' '-reverseReview rules -course StreetLoopReverse'
Run 'layout-forest' '-reverseReview rules -course ForestLoopReverse'
Run 'routes-street' "-reverseReview routes -course StreetLoopReverse -repeats $Repeats -testSpeed $Speed" 900
Run 'routes-forest' "-reverseReview routes -course ForestLoopReverse -repeats $Repeats -testSpeed $Speed" 900
Run 'jumps-street-reverse' '-reverseReview jumps -course StreetLoopReverse' 300
Run 'jumps-forest-reverse' '-reverseReview jumps -course ForestLoopReverse' 500
Run 'race-street' '-threeFeatureTest drive -laps 2 -testSpeed 6' 300
Run 'race-forest' '-threeFeatureTest drive -course LakeWoods -vehicle moto -laps 2 -testSpeed 6' 300
Run 'race-street-reverse' '-threeFeatureTest drive -course StreetLoopReverse -laps 2 -testSpeed 6' 300
Run 'race-forest-reverse' '-threeFeatureTest drive -course ForestLoopReverse -vehicle moto -laps 2 -testSpeed 6' 300
Run 'records' '-threeFeatureTest rules'
Run 'estimates-street-reverse' '-review6466 rules -course StreetLoopReverse'
Run 'estimates-forest-reverse' '-review6466 rules -course ForestLoopReverse'
Run 'art' '-review6466 art' 180
Run 'wildlife' '-signWildlifeTest wildlife -course LakeWoods' 240
Run 'audio' '-signWildlifeTest audio -course LakeWoods' 240
Run 'performance-street' '-signWildlifeTest drive -course StreetLoopGreybox' 240
Run 'performance-forest' '-signWildlifeTest drive -course LakeWoods' 240
Run 'performance-street-reverse' '-signWildlifeTest drive -course StreetLoopReverse' 240
Run 'performance-forest-reverse' '-signWildlifeTest drive -course ForestLoopReverse' 240
'Complete' | Set-Content "$evidence/standalone-done.txt"
