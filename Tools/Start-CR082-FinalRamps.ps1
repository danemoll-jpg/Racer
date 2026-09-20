param([string]$Tag='release2')
$ErrorActionPreference='Stop'
$project=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$report=Join-Path $project 'Docs/CR082-089/release-build.txt'
$deadline=(Get-Date).AddMinutes(20)
while(!(Test-Path -LiteralPath $report)){if((Get-Date) -gt $deadline){throw 'Final build did not complete'};Start-Sleep -Milliseconds 500}
if(!(Get-Content -LiteralPath $report -Raw).StartsWith('Succeeded errors=0')){throw 'Final build failed'}
$rows=@()
foreach($course in @('StreetLoopGreybox','StreetLoopReverse')){
 foreach($entry in @(@('race','no'),@('roam','no'),@('roam','yes'))){
  $arguments="-NoProfile -File `"$PSScriptRoot/Run-CR082-RampLane.ps1`" -Tag $Tag -Course $course -Mode $($entry[0]) -Opposite $($entry[1])"
  $p=Start-Process -FilePath pwsh -ArgumentList $arguments -WorkingDirectory $project -WindowStyle Hidden -PassThru
  $rows+=[pscustomobject]@{Course=$course;Mode=$entry[0];Opposite=$entry[1];RunnerPID=$p.Id;StartedUtc=[DateTime]::UtcNow.ToString('o')}
 }
}
$rows|ConvertTo-Json|Set-Content -LiteralPath "$project/Docs/CR082-089/$Tag-ramp-runners.json"
$rows|Format-Table
