param([string]$Course='systems',[string]$Tag='release2')
$ErrorActionPreference='Stop'
$project=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$report=Join-Path $project 'Docs/CR082-089/release-build.txt'
$deadline=(Get-Date).AddMinutes(20)
while(!(Test-Path -LiteralPath $report)){if((Get-Date) -gt $deadline){throw 'Final build did not complete'};Start-Sleep -Milliseconds 500}
if(!(Get-Content -LiteralPath $report -Raw).StartsWith('Succeeded errors=0')){throw 'Final build failed'}
function Run([string]$name,[string]$flags,[int]$limit=900){& "$PSScriptRoot/Run-CR082-Check.ps1" -Tag $Tag -Name $name -Flags $flags -Limit $limit}
if($Course -eq 'systems'){
 Run 'systems' '-correctionPass systems'
 Run 'radio' '-correctionPass radio'
 Run 'art' '-review6466 art'
 Run 'departure' '-correctionPass departure'
 Run 'records' '-threeFeatureTest rules'
 Run 'estimates' '-review6466 rules'
}else{
 foreach($group in @('ai','rules','recovery','roam-recovery','speed','smash')){Run "$group-$Course" "-arcadeTest $group -course $Course -lifeSeed 82089"}
 $vehicle=if($Course -in @('LakeWoods','ForestLoopReverse')){'moto'}else{'original'}
 $raceFlags="-threeFeatureTest drive -course $Course -vehicle $vehicle -laps 3 -testSpeed 6 -lifeSeed 82089"
 Run "race-$Course" $raceFlags
 if(Select-String -LiteralPath "$project/Docs/CR082-089/$Tag/race-$Course/checks.txt" -Pattern '^FAIL' -Quiet){Run "race-extended-$Course" "$raceFlags -finishGrace 300"}
 Run "guidance-$Course" "-review7074 physical -course $Course"
 if($Course -in @('LakeWoods','ForestLoopReverse')){Run "forest-scoring-$Course" "-arcadeTest forest-jump -course $Course"}
}
