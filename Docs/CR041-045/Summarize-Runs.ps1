$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$routeDir="$root/Docs/CR034-039/correction-release2-all"
$routes=@(Import-Csv "$routeDir/driving.csv")
$objects=@(Get-Content "$routeDir/physical-objects.txt")
$routeSummary=[ordered]@{
 Attempts=$routes.Count
 Finished=@($routes|Where-Object finished -eq 'True').Count
 Misses=($routes|Measure-Object misses -Sum).Sum
 PenaltySeconds=($routes|Measure-Object penalty -Sum).Sum
 BranchExits=($routes|Measure-Object branch_exits -Sum).Sum
 Recoveries=($routes|Measure-Object recoveries -Sum).Sum
 WarningBuzzes=($objects|ForEach-Object {if($_ -match 'buzzes=(\d+)'){[int]$Matches[1]}}|Measure-Object -Sum).Sum
 GullyBothPanesBroken=@($objects|Where-Object {$_ -match 'Fox Gully.*road=False.*glassBroken=2/2'}).Count
 ProfileCounts=@($routes|Group-Object profile|Select-Object Name,Count)
}
$routeSummary|ConvertTo-Json -Depth 5|Set-Content "$PSScriptRoot/routes-summary.json"
$raceRows=@()
foreach($period in @('CR034-039','CR041-045')) {
 foreach($difficulty in @(0,1,2)) {
  foreach($run in @('clean','mistake')) {
   $path="$root/Docs/$period/standalone-race-$difficulty-$run-traffic/results.txt"
   if(!(Test-Path $path)){continue}
   $lines=Get-Content $path
   if(!($lines|Where-Object {$_ -match 'Final=True'})){continue}
   $parsed=@()
   foreach($line in $lines) {
    if($line -match '^(.*?) (original|tourer|moto|atv): finished=(\w+); DNF=(\w+); laps=(\d+); lapTimes=([\d./]+); race=([\d.]+); adjusted=([\d.]+); peak=([\d.]+)m/s; mean=([\d.]+)m/s; brakeSeconds=([\d.]+); recoveries=(\d+); misses=(\d+)') {
     $parsed += [pscustomobject]@{Period=$period;Difficulty=$difficulty;Run=$run;Racer=$Matches[1];Profile=$Matches[2];Finished=$Matches[3];DNF=$Matches[4];Laps=[int]$Matches[5];LapTimes=$Matches[6];RaceSeconds=[double]$Matches[7];AdjustedSeconds=[double]$Matches[8];PeakMps=[double]$Matches[9];MeanMps=[double]$Matches[10];BrakeSeconds=[double]$Matches[11];Recoveries=[int]$Matches[12];Misses=[int]$Matches[13];GapToWinner=0.0;GapToReference=0.0}
    }
   }
   $winner=($parsed|Measure-Object AdjustedSeconds -Minimum).Minimum
   $reference=($parsed|Where-Object Profile -eq 'original').AdjustedSeconds
   foreach($row in $parsed){$row.GapToWinner=[Math]::Round($row.AdjustedSeconds-$winner,3);$row.GapToReference=[Math]::Round($row.AdjustedSeconds-$reference,3)}
   $raceRows+=$parsed
  }
 }
}
$raceRows|Export-Csv "$PSScriptRoot/race-measurements.csv" -NoTypeInformation
$routeSummary|ConvertTo-Json -Depth 5
$raceRows|Where-Object Period -eq 'CR041-045'|Format-Table Difficulty,Run,Profile,LapTimes,RaceSeconds,GapToWinner,GapToReference,Misses,Recoveries
