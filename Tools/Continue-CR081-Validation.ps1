param([int]$WaitFor,[int]$Lane)
$ErrorActionPreference='Stop'
while(Get-Process -Id $WaitFor -ErrorAction SilentlyContinue){Start-Sleep -Seconds 2}
function Matrix([string]$group){& "$PSScriptRoot/Run-CR081-Matrix.ps1" -Group $group -Tag release3}
if($Lane -eq 1){
 & "$PSScriptRoot/Run-CR081-Check.ps1" -Tag release3 -Name controller-navigation -Flags '-explorationCheck controls'
 $project=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
 $p=Get-Content "$project/Docs/CR081-090/release3/collection-records-ghost-fixtures/process.json" -Raw|ConvertFrom-Json
 $save=[regex]::Match($p.Flags,'-racerTestSave "([^"]+)"').Groups[1].Value
 if(!$save){throw 'Missing isolated collection save'}
 & "$PSScriptRoot/Run-CR081-Check.ps1" -Tag release3 -Name collection-relaunch-four-courses -Flags '-explorationCheck reload' -ReuseSave $save
 Matrix StreetLoopGreybox
 Matrix ramps-StreetLoopGreybox
}elseif($Lane -eq 2){Matrix LakeWoods;Matrix ramps-StreetLoopReverse}
elseif($Lane -eq 3){Matrix ramps-LakeWoods;Matrix ramps-ForestLoopReverse}
else{throw 'Unknown validation lane'}
