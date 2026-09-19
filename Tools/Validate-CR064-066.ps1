param([string]$Version='0.10.0-review1',[string]$StartAt='release-art')
$ErrorActionPreference='Stop'
$project=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$exe=Join-Path $project "Builds/Racer-$Version-Windows/Racer.exe"
$evidence=Join-Path $project 'Docs/CR064-066'
$rows=[Collections.Generic.List[object]]::new();$script:started=$false
function Run-Check([string]$name,[string]$flags,[int]$seconds=180,[string]$done='',[string]$work=''){
 if(!$script:started){if($name -ne $StartAt){return};$script:started=$true}
 if(!$work){$work=$project}
 New-Item -ItemType Directory -Force $work | Out-Null
 $playerArguments="-batchmode -screen-width 1280 -screen-height 720 -racerTestSave `"$project/Temp/cr6466-$name-save`" -evidence `"$evidence/$name`" -logFile `"$project/Logs/cr6466-$name.log`" $flags"
 $process=Start-Process -FilePath $exe -ArgumentList $playerArguments -WorkingDirectory $work -WindowStyle Hidden -PassThru
 Write-Output "START $name pid=$($process.Id)"
 $end=(Get-Date).AddSeconds($seconds);$finished=$false
 while(!$process.HasExited -and (Get-Date) -lt $end){
  if($done -and (Test-Path -LiteralPath $done)){try{if((Get-Content -LiteralPath $done -Tail 1) -eq 'DONE'){$finished=$true;break}}catch{}}
  Start-Sleep -Milliseconds 500;$process.Refresh()
 }
 if(!$process.HasExited){Stop-Process -Id $process.Id;$process.WaitForExit()}
 $rows.Add([pscustomobject]@{Name=$name;Exit=$process.ExitCode;CompletedFixture=$finished;Arguments=$flags;Finished=(Get-Date -Format o)})
 $rows | ConvertTo-Json | Set-Content "$evidence/standalone-runs-$StartAt.json"
 Write-Output "END $name exit=$($process.ExitCode) fixture=$finished"
}
Run-Check 'release-art' '-review6466 art'
Run-Check 'release-ai-forest' '-review6466 rules -course LakeWoods'
Run-Check 'release-ai-street' '-review6466 rules'
Run-Check 'release-radio' '-review6466 radio'
$flowWork=Join-Path $project 'Temp/cr6466-flow-work'
Run-Check 'preservation-flow' '-woodlandFlow' 240 "$flowWork/Docs/CR041-045/flow/standalone-flow.txt" $flowWork
New-Item -ItemType Directory -Force "$evidence/preservation-flow" | Out-Null
Copy-Item "$flowWork/Docs/CR041-045/flow/*" "$evidence/preservation-flow/" -Force
$forestWork=Join-Path $project 'Temp/cr6466-forest-work'
Run-Check 'preservation-forest' '-forestChecks' 120 '' $forestWork
Copy-Item "$forestWork/Docs/CR056" "$evidence/preservation-forest" -Recurse -Force
Run-Check 'preservation-records' '-threeFeatureTest rules'
Run-Check 'preservation-radio' '-threeFeatureTest radio'
Run-Check 'preservation-jumps' '-threeFeatureTest jumps' 240
Run-Check 'preservation-signs-street' '-signWildlifeTest signs -course StreetLoopGreybox' 240
Run-Check 'preservation-signs-forest' '-signWildlifeTest signs -course LakeWoods' 240
Run-Check 'performance-street' '-signWildlifeTest drive -course StreetLoopGreybox' 230
Run-Check 'performance-forest' '-signWildlifeTest drive -course LakeWoods' 230
Run-Check 'normal-ai-street' '-threeFeatureTest drive -course StreetLoopGreybox -vehicle original -laps 2 -testSpeed 6' 240
Run-Check 'normal-ai-forest' '-threeFeatureTest drive -course LakeWoods -vehicle moto -laps 2 -testSpeed 6' 240
Run-Check 'preservation-forest-jumps' '-forestProbe -nominal -forestEvidence Docs/CR064-066/preservation-forest-jumps' 240
Run-Check 'preservation-street-jump' '-correctionJump -jumpEvidence Docs/CR064-066/preservation-street-jump' 240
'Completed all standalone checks' | Set-Content "$evidence/standalone-done.txt"



