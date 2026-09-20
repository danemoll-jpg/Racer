param([int]$WaitFor,[int]$Lane)
$ErrorActionPreference='Stop'
$project=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
while(Get-Process -Id $WaitFor -ErrorAction SilentlyContinue){Start-Sleep -Seconds 2}
function Run([string]$name,[string]$flags){& "$PSScriptRoot/Run-CR081-Check.ps1" -Tag release3 -Name $name -Flags $flags}
function Matrix([string]$group){& "$PSScriptRoot/Run-CR081-Matrix.ps1" -Group $group -Tag release3}
if($Lane -eq 1){
 $trails=Get-Content "$project/Docs/CR081-090/release3/connected-trails/trails.txt"
 if(@($trails -match 'complete=True').Count -ne 4){throw 'Connected trail validation requires inspection before this lane continues'}
 Run property-views '-explorationCheck views'
 Run household-regression '-correctionPass systems'
 Run radio-continuity '-correctionPass radio'
 Run lap-race-boards '-threeFeatureTest rules'
 Run actual-top-ten '-arcadeTest speed -raceActivities yes -recordsStress yes'
 Matrix StreetLoopGreybox
 Matrix ramps-StreetLoopGreybox
}elseif($Lane -eq 2){
 $p=Get-Content "$project/Docs/CR081-090/release3/collection-records-ghost-fixtures/process.json" -Raw|ConvertFrom-Json
 $save=[regex]::Match($p.Flags,'-racerTestSave "([^"]+)"').Groups[1].Value
 if(!$save){throw 'Missing isolated collection save'}
 & "$PSScriptRoot/Run-CR081-Check.ps1" -Tag release3 -Name collection-relaunch-four-courses -Flags '-explorationCheck reload' -ReuseSave $save
 Matrix ForestLoopReverse
 Matrix ramps-LakeWoods
 Matrix ramps-ForestLoopReverse
}elseif($Lane -eq 3){Matrix LakeWoods;Matrix ramps-StreetLoopReverse}
else{throw 'Unknown validation lane'}
[DateTime]::UtcNow.ToString('o')|Set-Content "$project/Docs/CR081-090/release3/lane-$Lane-done.txt"
