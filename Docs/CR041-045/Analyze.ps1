$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$summary=@()
foreach($difficulty in @(1,2)) {
 $file=Join-Path $PSScriptRoot (@{1='normal';2='hard'}[$difficulty]+'-race-trace.csv.traffic.csv')
 if(!(Test-Path $file)){continue}
 $rows=@(Import-Csv $file | Where-Object {[double]$_.time -gt 10})
 if(!$rows.Count){continue}
 $highway=@($rows | Where-Object {[double]$_.station -ge 3760 -and [double]$_.station -le 4600})
 $occupancy=@($rows | Group-Object time | ForEach-Object { @($_.Group | Where-Object {[double]$_.station -ge 3760 -and [double]$_.station -le 4600}).Count })
 $cars=@($rows | Group-Object name)
 $crossings=0
 foreach($car in $cars){$previous=$null;foreach($row in $car.Group){if($previous -and [Math]::Abs([double]$row.station-[double]$previous.station) -lt 30 -and (([double]$previous.station-4100)*([double]$row.station-4100)) -lt 0){$crossings++};$previous=$row}}
 $end=@($cars | ForEach-Object {$_.Group[-1]})
 $duration=([double]$rows[-1].time-[double]$rows[0].time)
 $summary += [pscustomobject]@{
  Difficulty=$difficulty;TrafficCars=$cars.Count;DedicatedHighway=@($end | Where-Object highwayPool -eq 'True').Count
  SampleSeconds=$duration;HighwayCarsMean=($occupancy|Measure-Object -Average).Average;HighwayCarsMin=($occupancy|Measure-Object -Minimum).Minimum;HighwayCarsMax=($occupancy|Measure-Object -Maximum).Maximum
  HighwaySpeedMeanMps=($highway|Measure-Object speed -Average).Average;HighwaySpeedPeakMps=($highway|Measure-Object speed -Maximum).Maximum
  LaneSamples=@($highway|Group-Object {if([double]$_.lateral -lt -4){'west-outer'}elseif([double]$_.lateral -lt 0){'west-inner'}elseif([double]$_.lateral -lt 4){'east-inner'}else{'east-outer'}} | Select-Object Name,Count)
  Station4100Passes=$crossings;PassesPerMinute=$crossings/$duration*60
  Recoveries=($end|Measure-Object recoveries -Sum).Sum;Recycles=($end|Measure-Object recycles -Sum).Sum
  MinimumRecycleDistance=($end|Where-Object {[int]$_.recycles -gt 0}|Measure-Object minRecycleDistance -Minimum).Minimum
  HighwayStoppedSamplePercent=100*@($highway|Where-Object {[double]$_.speed -lt 2}).Count/[Math]::Max(1,$highway.Count)
 }
}
$summary | ConvertTo-Json -Depth 6 | Set-Content (Join-Path $PSScriptRoot 'traffic-measurements.json')
foreach($period in @('CR034-039','CR041-045')) {foreach($difficulty in @(1,2)){
 $dir=Join-Path $root "Docs/$period/standalone-race-$difficulty-clean-traffic"
 if(!(Test-Path "$dir/pace.csv")){continue}
 $sectors=@()
 foreach($car in (Import-Csv "$dir/pace.csv" | Group-Object name)){
  $previous=$null;$lastTime=0
  foreach($row in $car.Group){
   if(!$previous){$previous=$row;$lastTime=[double]$row.time;continue}
   if($row.lap -ne $previous.lap -or $row.nextGate -ne $previous.nextGate){
    $sectors += [pscustomobject]@{Period=$period;Difficulty=$difficulty;Racer=$car.Name;Lap=1+[int]$previous.lap;FromExpectedGate=$previous.nextGate;ToExpectedGate=$row.nextGate;Seconds=[Math]::Round([double]$row.time-$lastTime,3);CumulativeMisses=$row.misses}
    $lastTime=[double]$row.time
   };$previous=$row
  }
 }
 $sectors | Export-Csv (Join-Path $PSScriptRoot "sectors-$period-$difficulty.csv") -NoTypeInformation
}}
'Sector transitions are sampled at approximately 0.25 seconds; use exact lap totals in results.txt. Traffic flow counts exclude recycle jumps.' | Set-Content (Join-Path $PSScriptRoot 'measurement-notes.txt')
