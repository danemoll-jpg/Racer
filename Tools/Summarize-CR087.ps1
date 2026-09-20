param([string]$Root='Docs/CR082-089/baseline-instrumented',[string]$Output='Docs/CR082-089/baseline-speed-events.csv')
$ErrorActionPreference='Stop'
$events=@()
foreach($file in Get-ChildItem -LiteralPath $Root -Recurse -Filter '*.csv') {
 if($file.Name -eq 'ramps.csv'){continue}
 $rows=@(Import-Csv -LiteralPath $file.FullName)
 if($rows.Count -lt 3 -or !$rows[0].PSObject.Properties['speed'] -or !$rows[0].PSObject.Properties['time']){continue}
 for($i=1;$i -lt $rows.Count;$i++) {
  $a=$rows[$i-1];$b=$rows[$i]
  $drop=[double]$a.speed-[double]$b.speed
  if($drop -lt 1){continue}
  $after=$rows[[Math]::Min($rows.Count-1,$i+25)]
  $events += [pscustomobject]@{Mode=$file.Directory.Name;Case=$file.BaseName;Time=$b.time;Before=$a.speed;At=$b.speed;AfterHalfSecond=$after.speed;Drop=$drop;VelocityBefore=$a.velocity;VelocityAt=$b.velocity;VelocityAfter=$after.velocity;X=$b.x;Y=$b.y;Z=$b.z;Travel=$b.travel;Throttle=$a.throttle;Brake=$a.brake;Steering=$a.steering;Grounded=$b.grounded;Water=$b.water;Up=$b.up;Lift=$b.lift;Torque=$b.torque;Contact=$b.contact}
 }
}
$events|Export-Csv -LiteralPath $Output -NoTypeInformation
$events|Group-Object Mode|Select-Object Name,Count
