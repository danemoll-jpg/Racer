param([string]$Group='StreetLoopGreybox',[string]$Tag='release3',[string]$Runtime='Builds/Racer-0.15.0-review3-Windows/Racer.exe')
$ErrorActionPreference='Stop'
function Run([string]$name,[string]$flags,[int]$limit=1200){& "$PSScriptRoot/Run-CR081-Check.ps1" -Runtime $Runtime -Tag $Tag -Name $name -Flags $flags -Limit $limit}
if($Group -eq 'systems'){
 Run 'collection-records-ghost-fixtures' '-explorationCheck systems'
 Run 'property-views' '-explorationCheck views'
 Run 'household-regression' '-correctionPass systems'
 Run 'radio-continuity' '-correctionPass radio'
 Run 'lap-race-boards' '-threeFeatureTest rules'
 Run 'actual-top-ten' '-arcadeTest speed -raceActivities yes -recordsStress yes'
 Run 'connected-trails' '-explorationCheck trails' 1300
}elseif($Group.StartsWith('ramps-')){
 $course=$Group.Substring(6)
 foreach($opposite in @('no','yes')){
  Run "mountain-$course-$opposite" "-explorationCheck ramps -testSpeed 4 -course $course -opposite $opposite" 3600
 }
 foreach($mode in @('race','roam','opposite')){
  $flags="-course $course"
  if($mode -eq 'race'){$flags+=' -raceActivities yes'}else{$flags+=' -rampRoam yes'}
  if($mode -eq 'opposite'){$flags+=' -rampOpposite yes'}
  Run "trickum-$course-$mode" "-arcadeRamp full -testSpeed 4 $flags" 2400
  Run "trickum-high-$course-$mode" "-arcadeRamp high -testSpeed 4 $flags"
  if($course -eq 'StreetLoopReverse'){foreach($edge in @('-10.3','7.3')){Run "trickum-edge-$course-$mode-$edge" "-arcadeRamp case -rampSpeed 24 -rampLine $edge $flags"}}
 }
}else{
 foreach($check in @('rules','recovery','roam-recovery','speed','ai')){Run "$check-$Group" "-arcadeTest $check -course $Group -raceActivities yes -lifeSeed 82089"}
 $vehicles=if($Group -in @('LakeWoods','ForestLoopReverse')){@('moto','atv')}else{@('original','tourer','moto','atv')}
 foreach($vehicle in $vehicles){Run "ghost-race-$Group-$vehicle" "-threeFeatureTest drive -course $Group -vehicle $vehicle -laps 3 -testSpeed 6 -ghostValidation yes -lifeSeed 82089"}
 Run "guidance-$Group" "-review7074 physical -course $Group"
 if($Group -in @('LakeWoods','ForestLoopReverse')){Run "forest-scoring-$Group" "-arcadeTest forest-jump -course $Group"}
 Run "race-jump-$Group" "-arcadeRamp activities -raceActivities yes -course $Group"
}


