param([int[]]$WaitFor=@(80208,99572,88840))
$ErrorActionPreference='Stop'
$project=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
# Reuse the first finished lane's slot; keep at most three test games active.
while(@($WaitFor|Where-Object {Get-Process -Id $_ -ErrorAction SilentlyContinue}).Count -eq $WaitFor.Count){Start-Sleep -Seconds 2}
function Run([string]$name,[string]$flags){& "$PSScriptRoot/Run-CR081-Check.ps1" -Tag release3 -Name $name -Flags $flags}
foreach($course in @('StreetLoopGreybox','LakeWoods','StreetLoopReverse','ForestLoopReverse')){
 Run "navigation-rules-$course" "-review7074 rules -course $course"
 Run "shortcut-physical-$course" "-reverseReview routes -course $course -repeats 1 -testSpeed 6"
 $vehicles=if($course -in @('LakeWoods','ForestLoopReverse')){@('moto','atv')}else{@('original','tourer','moto','atv')}
 foreach($vehicle in $vehicles){
  $file="$project/Docs/CR081-090/release3/ghost-race-$course-$vehicle/ghost-storage.txt"
  if(!(Test-Path $file)){Run "ghost-solo-$course-$vehicle" "-threeFeatureTest drive -course $course -vehicle $vehicle -laps 4 -testSpeed 6 -ghostValidation yes -ghostSolo yes"}
 }
}
Run recovery-ForestLoopReverse-repeat '-arcadeTest recovery -course ForestLoopReverse -raceActivities yes -lifeSeed 82089'
# No other validation lane remains active during these rendering measurements.
while(@($WaitFor|Where-Object {Get-Process -Id $_ -ErrorAction SilentlyContinue}).Count){Start-Sleep -Seconds 2}
Run performance-mountain '-explorationPerformance yes -area mountain -lifeSeed 82089'
Run performance-home '-explorationPerformance yes -area home -lifeSeed 82089'
[DateTime]::UtcNow.ToString('o')|Set-Content "$project/Docs/CR081-090/release3/supplemental-done.txt"
