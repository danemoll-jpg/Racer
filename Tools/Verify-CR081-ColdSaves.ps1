param([int]$WaitFor=84668)
$ErrorActionPreference='Stop'
$project=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
while(Get-Process -Id $WaitFor -ErrorAction SilentlyContinue){Start-Sleep -Seconds 2}
function SavedPath([string]$name){
 $p=Get-Content "$project/Docs/CR081-090/release3/$name/process.json" -Raw|ConvertFrom-Json
 $result=[regex]::Match($p.Flags,'-racerTestSave "([^"]+)"').Groups[1].Value
 if(!$result){throw "Missing isolated save: $name"};return $result
}
foreach($course in @('StreetLoopGreybox','LakeWoods','StreetLoopReverse','ForestLoopReverse')){
 $vehicle=if($course -in @('LakeWoods','ForestLoopReverse')){'moto'}else{'original'}
 $name="ghost-race-$course-$vehicle"
 if(!(Test-Path "$project/Docs/CR081-090/release3/$name/ghost-storage.txt")){$name="ghost-solo-$course-$vehicle"}
 & "$PSScriptRoot/Run-CR081-Check.ps1" -Tag release3 -Name "ghost-cold-$course" -Flags "-threeFeatureTest drive -course $course -vehicle $vehicle -laps 1 -testSpeed 6 -ghostValidation yes -ghostSolo yes" -ReuseSave (SavedPath $name)
}
& "$PSScriptRoot/Run-CR081-Check.ps1" -Tag release3 -Name activity-cold-save -Flags '-arcadeTest persistence' -ReuseSave (SavedPath actual-top-ten)
[DateTime]::UtcNow.ToString('o')|Set-Content "$project/Docs/CR081-090/release3/cold-saves-done.txt"
