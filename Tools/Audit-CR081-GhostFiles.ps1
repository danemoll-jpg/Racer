param([string]$Tag='release3')
$ErrorActionPreference='Stop'
$project=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$root=Join-Path $project "Docs/CR081-090/$Tag"
$rows=@(Get-ChildItem $root -Directory -Filter 'ghost-*'|ForEach-Object {
 $test=$_
 if(!(Test-Path "$($test.FullName)/process.json")){return}
 $p=Get-Content "$($test.FullName)/process.json" -Raw|ConvertFrom-Json
 $save=[regex]::Match($p.Flags,'-racerTestSave "([^"]+)"').Groups[1].Value
 if(!$save -or !$save.StartsWith((Join-Path $project 'Temp')+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)){throw 'Audit must use isolated test saves'}
 $board=Get-Content (Join-Path $save 'top-ten-v1.json') -Raw|ConvertFrom-Json
 foreach($file in Get-ChildItem (Join-Path $save 'CleanLapGhosts') -File -Filter '*.json' -ErrorAction SilentlyContinue){
  $lap=Get-Content $file.FullName -Raw|ConvertFrom-Json
  $monotonic=$true;$finite=$true;$last=-1.0
  foreach($pose in $lap.poses){
   if($pose.t -le $last){$monotonic=$false};$last=$pose.t
   foreach($value in @($pose.t,$pose.p.x,$pose.p.y,$pose.p.z,$pose.q.x,$pose.q.y,$pose.q.z,$pose.q.w)){if(![double]::IsFinite([double]$value)){$finite=$false}}
  }
  $matched=@($board.entries|Where-Object {!$_.race -and [math]::Abs([double]$_.seconds-[double]$lap.seconds) -lt .000001}).Count -gt 0
  [pscustomobject]@{Test=$test.Name;Key=$lap.key;Seconds=$lap.seconds;Samples=$lap.poses.Count;Bytes=$file.Length;FirstIsZero=$lap.poses[0].t -eq 0;EndpointErrorSeconds=[math]::Abs($last-$lap.seconds);StrictlyIncreasing=$monotonic;FinitePoses=$finite;MatchesActualLapBoardTime=$matched;Bounded=$lap.poses.Count -le 24002 -and $file.Length -lt 12000000;SHA256=(Get-FileHash $file.FullName).Hash}
 }
})
$rows|ConvertTo-Json -Depth 5|Set-Content "$root/ghost-file-audit.json"
$rows|Select-Object Test,Seconds,Samples,Bytes,MatchesActualLapBoardTime,StrictlyIncreasing,EndpointErrorSeconds|Format-Table -AutoSize
