param([string]$Runtime='Builds/Racer-0.17.0-review1-Windows/Racer.exe',[string]$Tag='release',[ValidateSet('world','summit','regression')][string]$Group='world',[string]$Course='all')
$ErrorActionPreference='Stop'
$requestedCourse=$Course
function Run([string]$name,[string]$flags,[int]$limit=1200){& "$PSScriptRoot/Run-CR097-Check.ps1" -Runtime $Runtime -Tag $Tag -Name $name -Flags $flags -Limit $limit}
$courses=@('StreetLoopGreybox','LakeWoods','StreetLoopReverse','ForestLoopReverse')
if($Course -ne 'all'){$courses=@($Course)}
foreach($course in $courses){
 if($Group -eq 'world'){
  Run "house3-$course" "-propertyCheck driveway -course $course -testSpeed 4" 400
  Run "approach-$course" "-propertyCheck approach -course $course -testSpeed 4" 400
  Run "return-$course" "-discoverySystems return -course $course" 300
  Run "map-$course" "-discoverySystems map -course $course"
  Run "finish-$course" "-discoveryCheck finish -course $course"
  Run "views-$course" "-discoverySystems views -course $course"
  Run "collections-$course" "-discoverySystems collections -course $course -testSpeed 4" 600
  $saved=Get-Content (Join-Path $PSScriptRoot "../Docs/CR097-100/$Tag/collections-$course/save-root.txt") -Raw
  & "$PSScriptRoot/Run-CR097-Check.ps1" -Runtime $Runtime -Tag $Tag -Name "cold-collections-$course" -Flags '-discoverySystems reload' -ReuseSave $saved.Trim() -Limit 180
 }elseif($Group -eq 'summit'){
  foreach($mode in @('roam','race')){Run "summit-$course-$mode" "-discoveryCheck summit -course $course -mode $mode -testSpeed 4" 900}
  Run "wrong-face-$course" "-discoveryCheck summit -course $course -opposite yes -quick yes -testSpeed 4" 300
 }else{
  foreach($check in @('rules','recovery','roam-recovery','ai')){Run "$check-$course" "-arcadeTest $check -course $course -raceActivities yes -lifeSeed 82089"}
  $vehicles=if($course -in @('LakeWoods','ForestLoopReverse')){@('moto','atv')}else{@('original','tourer','moto','atv')}
  foreach($vehicle in $vehicles){Run "ghost-race-$course-$vehicle" "-threeFeatureTest drive -course $course -vehicle $vehicle -laps 2 -testSpeed 6 -ghostValidation yes -lifeSeed 82089" 900}
  Run "shortcuts-$course" "-reverseReview routes -course $course -repeats 1 -testSpeed 6" 900
 }
}
[DateTime]::UtcNow.ToString('o') | Set-Content (Join-Path $PSScriptRoot "../Docs/CR097-100/$Tag/$Group-$requestedCourse-done.txt")
