$ErrorActionPreference='Stop'
# Read captured PCM samples only. This is not an OS-endpoint or listening test.
$rows=@()
foreach($file in Get-ChildItem "$PSScriptRoot/audio-final/*.wav") {
 $bytes=[IO.File]::ReadAllBytes($file.FullName)
 $channels=[BitConverter]::ToInt16($bytes,22);$rate=[BitConverter]::ToInt32($bytes,24)
 $samples=New-Object short[] (($bytes.Length-44)/2)
 [Buffer]::BlockCopy($bytes,44,$samples,0,$bytes.Length-44)
 $bin=[int]($rate*$channels*.1);$prePeak=0.0;$laterPeak=0.0;$maxRms=0.0;$maxTime=0.0
 for($start=0;$start -lt $samples.Length;$start+=$bin) {
  $sum=0.0;$count=[Math]::Min($bin,$samples.Length-$start)
  for($j=0;$j -lt $count;$j++) {
   $value=$samples[$start+$j]/32767.0;$sum+=$value*$value
   if($start -lt $bin*2){$prePeak=[Math]::Max($prePeak,[Math]::Abs($value))}
   else{$laterPeak=[Math]::Max($laterPeak,[Math]::Abs($value))}
  }
  $rms=[Math]::Sqrt($sum/$count)
  if($rms -gt $maxRms){$maxRms=$rms;$maxTime=$start/($rate*$channels)}
 }
 $rows += [pscustomobject]@{File=$file.Name;First200msPeak=[Math]::Round($prePeak,6);LaterPeak=[Math]::Round($laterPeak,6);Maximum100msRMS=[Math]::Round($maxRms,6);MaximumBinStartSeconds=[Math]::Round($maxTime,3)}
}
$rows | Export-Csv "$PSScriptRoot/audio-transients.csv" -NoTypeInformation
$rows | Format-Table
