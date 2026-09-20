param([string]$Runtime='Builds/CR091-inspection/Racer.exe',[string]$Tag='inspection',[int]$Lane=1,[string]$Course='all')
$ErrorActionPreference='Stop'
function Run([string]$name,[string]$flags,[int]$limit=1200){& "$PSScriptRoot/Run-CR091-Check.ps1" -Runtime $Runtime -Tag $Tag -Name $name -Flags $flags -Limit $limit}
$courses=@('StreetLoopGreybox','LakeWoods','StreetLoopReverse','ForestLoopReverse')
if($Course -ne 'all'){if($Course -notin $courses){throw 'Unknown course'};$courses=@($Course)}
if($Lane -eq 1){
 foreach($course in $courses){Run "finish-$course" "-discoveryCheck finish -course $course";Run "map-$course" "-discoverySystems map -course $course";Run "views-$course" "-discoverySystems views -course $course"}
 Run 'collections' '-discoverySystems collections -testSpeed 4' 900
 $project=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
 $save=Get-Content "$project/Docs/CR091-096/$Tag/collections/save-root.txt"
 & "$PSScriptRoot/Run-CR091-Check.ps1" -Runtime $Runtime -Tag $Tag -Name 'collections-cold-reload' -Flags '-discoverySystems reload' -ReuseSave $save
 foreach($course in @('LakeWoods','StreetLoopReverse','ForestLoopReverse')){Run "collections-$course" "-discoverySystems collections -course $course -testSpeed 4" 900}
 & "$PSScriptRoot/Run-CR091-Check.ps1" -Runtime $Runtime -Tag $Tag -Name 'confirmed-restart-after-reload' -Flags '-discoverySystems map' -ReuseSave $save
 foreach($check in @(@('households','-correctionPass systems'),@('radio','-correctionPass radio'),@('smash','-arcadeTest smash'),@('boards','-threeFeatureTest rules'),@('top-ten','-arcadeTest speed -raceActivities yes -recordsStress yes'))){Run $check[0] $check[1]}
}elseif($Lane -eq 2){
 foreach($course in $courses){foreach($mode in @('race','roam')){Run "summit-$course-$mode" "-discoveryCheck summit -course $course -mode $mode -testSpeed 4" 1800};Run "summit-$course-return-face" "-discoveryCheck summit -course $course -opposite yes -quick yes -testSpeed 4"}
}elseif($Lane -eq 3){
 foreach($course in $courses){
  foreach($check in @('rules','recovery','roam-recovery','speed','ai')){Run "$check-$course" "-arcadeTest $check -course $course -raceActivities yes -lifeSeed 82089"}
  $vehicles=if($course -in @('LakeWoods','ForestLoopReverse')){@('moto','atv')}else{@('original','tourer','moto','atv')}
  foreach($vehicle in $vehicles){Run "ghost-race-$course-$vehicle" "-threeFeatureTest drive -course $course -vehicle $vehicle -laps 3 -testSpeed 6 -ghostValidation yes -lifeSeed 82089" 1800}
  Run "guidance-$course" "-review7074 physical -course $course" 1800
  Run "shortcuts-$course" "-reverseReview routes -course $course -repeats 1 -testSpeed 6" 1800
  Run "race-jump-$course" "-arcadeRamp activities -raceActivities yes -course $course" 1200
 }
}else{throw 'Lane must be 1, 2 or 3'}
[DateTime]::UtcNow.ToString('o')|Set-Content (Join-Path $PSScriptRoot "../Docs/CR091-096/$Tag/lane-$Lane-$Course-done.txt")
