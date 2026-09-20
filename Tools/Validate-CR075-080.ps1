param([string]$Version='0.13.0-review7',[string]$Group='rules',[string]$Tag='candidate1',[string]$Course='',[string]$RuntimePath='')
$ErrorActionPreference='Stop'
$project=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$exe=Join-Path $project "Builds/Racer-$Version-Windows/Racer.exe"
if($RuntimePath){$exe=[IO.Path]::GetFullPath($RuntimePath)}
$root=Join-Path $project "Docs/CR075-080/$Tag"
New-Item -ItemType Directory -Force $root | Out-Null
$id=[Guid]::NewGuid().ToString('N');$rows=[Collections.Generic.List[object]]::new()
function Run([string]$name,[string]$flags,[int]$limit=240){
 $args="-batchmode -screen-width 1280 -screen-height 720 -racerTestSave `"$project/Temp/cr7580-$id-$name`" -evidence `"$root/$name`" -logFile `"$project/Logs/cr7580-$Tag-$name.log`" $flags"
 $p=Start-Process -FilePath $exe -ArgumentList $args -WorkingDirectory $project -WindowStyle Hidden -PassThru
 "START $name pid=$($p.Id)"
 $end=(Get-Date).AddSeconds($limit);$peak=0L
 while(!$p.HasExited -and (Get-Date) -lt $end){Start-Sleep -Milliseconds 500;$p.Refresh();if(!$p.HasExited){$peak=[Math]::Max($peak,$p.WorkingSet64)}}
 $timeout=!$p.HasExited;if($timeout){$p.Kill();$p.WaitForExit()}
 $rows.Add([pscustomobject]@{Version=$Version;Name=$name;Exit=$p.ExitCode;Timeout=$timeout;PeakMiB=[Math]::Round($peak/1MB,2);Flags=$flags})
 $rows|ConvertTo-Json|Set-Content "$root/$Group-processes.json"
 "END $name exit=$($p.ExitCode) timeout=$timeout"
}
$courses=@('StreetLoopGreybox','LakeWoods','StreetLoopReverse','ForestLoopReverse');if($Course){$courses=@($Course)}
if($Group -in @('rules','recovery','speed','ai','smash','roam-recovery')){foreach($course in $courses){Run "$Group-$course" "-arcadeTest $Group -course $course" 400}}
if($Group -eq 'ramps'){foreach($course in @('StreetLoopReverse','StreetLoopGreybox') | Where-Object { !$Course -or $_ -eq $Course }){Run "ramps-$course" "-arcadeRamp full -course $course" 1600}}
if($Group -eq 'jumps'){foreach($course in @('StreetLoopReverse','StreetLoopGreybox') | Where-Object { !$Course -or $_ -eq $Course }){Run "activities-$course" "-arcadeRamp activities -course $course" 420}}
if($Group -eq 'ramp-high'){foreach($course in @('StreetLoopReverse','StreetLoopGreybox')){Run "ramp-high-$course" "-arcadeRamp high -course $course" 400}}
if($Group -eq 'flight-guards'){foreach($course in @('StreetLoopReverse','StreetLoopGreybox')){Run "flight-guards-$course" "-arcadeRamp recovery -course $course" 180}}
if($Group -eq 'forest-scoring'){foreach($course in @('LakeWoods','ForestLoopReverse')){Run "scoring-$course" "-arcadeTest forest-jump -course $course" 360}}
if($Group -eq 'offroute'){foreach($course in @('LakeWoods','ForestLoopReverse')){Run "offroute-$course" "-arcadeTest offroute -course $course" 120}}
if($Group -eq 'races'){foreach($course in $courses){$vehicle=if($course -in @('LakeWoods','ForestLoopReverse')){'moto'}else{'original'};Run "race-$course" "-threeFeatureTest drive -course $course -vehicle $vehicle -laps 3 -testSpeed 6" 420}}
if($Group -eq 'guidance'){foreach($course in $courses){Run "guidance-$course" "-review7074 physical -course $course" 240;Run "navigation-rules-$course" "-review7074 rules -course $course" 180}}
if($Group -eq 'forest-ramps'){foreach($course in @('LakeWoods','ForestLoopReverse')){Run "jumps-$course" "-reverseReview jumps -course $course" 600}}
if($Group -eq 'performance'){foreach($course in $courses){Run "performance-$course" "-signWildlifeTest drive -course $course" 240}}
if($Group -eq 'roam-performance'){foreach($course in $courses){Run "roam-performance-$course" "-signWildlifeTest drive -roamPerformance -course $course" 240}}
if($Group -eq 'records'){Run 'records' '-threeFeatureTest rules';Run 'estimates' '-review6466 rules';Run 'art' '-review6466 art'}

if($Group -eq 'persistence'){foreach($course in $courses){$saved=Get-ChildItem "$project/Temp" -Directory -Filter "cr7580-*-rules-$course" | Where-Object {Test-Path (Join-Path $_.FullName "activities-v1.json")} | Sort-Object LastWriteTime -Descending | Select-Object -First 1;if(!$saved){throw 'Run rules before persistence'};Run "persistence-$course" "-arcadeTest persistence -course $course -racerTestSave `"$($saved.FullName)`"" 180}}


if($Group -eq 'forest-street-ramps'){foreach($course in @('LakeWoods','ForestLoopReverse')){Run "forest-street-ramps-$course" "-arcadeRamp full -course $course" 900;Run "forest-street-high-$course" "-arcadeRamp high -course $course" 240}}


if($Group -eq 'course-ramp'){if(!$Course){throw 'Course required'};Run "ramps-$Course" "-arcadeRamp full -course $Course" 1600;Run "high-$Course" "-arcadeRamp high -course $Course" 600}

if($Group -eq 'races-extended'){foreach($course in $courses){$vehicle=if($course -in @('LakeWoods','ForestLoopReverse')){'moto'}else{'original'};Run "race-extended-$course" "-threeFeatureTest drive -course $course -vehicle $vehicle -laps 3 -testSpeed 6 -finishGrace 300" 600}}

if($Group -eq 'landing-followup'){Run 'tourer-36-centred-settle' '-arcadeRamp case -course StreetLoopGreybox -rampVehicle tourer -rampSpeed 36 -rampLine 0 -rampSettle 2' 120}
'Complete'|Set-Content "$root/$Group-done.txt"
