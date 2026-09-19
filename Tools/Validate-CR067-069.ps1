param([string]$Version='0.11.0-review1',[string]$StartAt='rules-street',[string]$EvidenceTag='final',[string]$Only='')
$ErrorActionPreference='Stop'
$project=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$exe=Join-Path $project "Builds/Racer-$Version-Windows/Racer.exe"
$evidence=Join-Path $project ('Docs/CR067-069/'+$EvidenceTag)
New-Item -ItemType Directory -Force $evidence | Out-Null
$rows=[Collections.Generic.List[object]]::new();$script:started=$false
function Run([string]$name,[string]$flags,[int]$seconds=180,[string]$save=''){
 if($Only -and $name -notin $Only.Split(',')){return}
 if(!$script:started){if($name -ne $StartAt){return};$script:started=$true}
 if(!$save){$save="Temp/cr6769-$EvidenceTag-$name-save"}
 $args="-batchmode -screen-width 1280 -screen-height 720 -racerTestSave `"$project/$save`" -evidence `"$evidence/$name`" -logFile `"$project/Logs/cr6769-$name.log`" $flags"
 $p=Start-Process -FilePath $exe -ArgumentList $args -WorkingDirectory $project -WindowStyle Hidden -PassThru
 Write-Output "START $name pid=$($p.Id)"
 $end=(Get-Date).AddSeconds($seconds)
 while(!$p.HasExited -and (Get-Date) -lt $end){Start-Sleep -Milliseconds 500;$p.Refresh()}
 if(!$p.HasExited){Stop-Process -Id $p.Id;$p.WaitForExit()}
 $rows.Add([pscustomobject]@{Name=$name;Exit=$p.ExitCode;Flags=$flags;Finished=(Get-Date -Format o)})
 $rows | ConvertTo-Json | Set-Content "$evidence/standalone-$StartAt.json"
 Write-Output "END $name exit=$($p.ExitCode)"
}
Run 'rules-street' '-review6769 rules'
Run 'rules-forest' '-review6769 rules -course LakeWoods'
Run 'households-street-launch1' '-review6769 households' 180 'Temp/cr6769-ordinary-save'
Run 'households-street-launch2' '-review6769 households' 180 'Temp/cr6769-ordinary-save'
Run 'households-forest-launch1' '-review6769 households -course LakeWoods' 180 'Temp/cr6769-ordinary-save'
Run 'households-forest-launch2' '-review6769 households -course LakeWoods' 180 'Temp/cr6769-ordinary-save'
Run 'physical-street' '-review6769 physical'
Run 'physical-forest' '-review6769 physical -course LakeWoods'
Run 'hairpin' '-review6769 hairpin' 360
Run 'ai-street' '-threeFeatureTest drive -laps 2 -testSpeed 6' 240
Run 'ai-forest' '-threeFeatureTest drive -course LakeWoods -vehicle moto -laps 2 -testSpeed 6' 240
Run 'records' '-threeFeatureTest rules'
Run 'estimates-street' '-review6466 rules'
Run 'estimates-forest' '-review6466 rules -course LakeWoods'
Run 'radio' '-review6466 radio'
Run 'art' '-review6466 art'
Run 'performance-street' '-signWildlifeTest drive -course StreetLoopGreybox' 240
Run 'performance-forest' '-signWildlifeTest drive -course LakeWoods' 240
'Complete' | Set-Content "$evidence/standalone-done.txt"
