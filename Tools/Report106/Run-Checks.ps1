# Runs the 0.106 built-player checks one after another (minimized players, copies of Dan's save); -Only "views,menus" picks some.
param([string]$Only='')
$ErrorActionPreference='Continue'
$run=Join-Path $PSScriptRoot 'Run-Player.ps1'
$all=[ordered]@{
  'views'   =@{a='-p106kind views'; m=10}
  'features'=@{a='-p106kind features -p106vehicle stuntbuggy,stuntbike,original -p106tricks "K3 big=back;K4 huge=back;K4 huge=front;K4 huge=spin;school-bus=roll;barrel=roll;K3 big=halfback"'; m=40}
  'menus'   =@{a='-p106kind menus'; m=10}
  'lap0'    =@{a='-p106kind lap -p106course 0'; m=20}
  'lap1'    =@{a='-p106kind lap -p106course 1'; m=20}
  'event1'  =@{a='-p106kind event -p106players 1'; m=20}
  'event2'  =@{a='-p106kind event -p106players 2'; m=20}
  # Part A 6 at 3840x2160 full screen (this check is about 4K: visible for about a minute each), after and before (-p106off), one view and split-screen
  'perf-full-after' =@{a='-p106kind perf -p106w 3840 -p106h 2160 -p106full 1'; m=8; big=1}
  'perf-full-before'=@{a='-p106kind perf -p106w 3840 -p106h 2160 -p106full 1 -p106off'; m=8; big=1}
  'perf-split-after' =@{a='-p106kind perf -p106split 1 -p106w 3840 -p106h 2160 -p106full 1'; m=8; big=1}
  'perf-split-before'=@{a='-p106kind perf -p106split 1 -p106w 3840 -p106h 2160 -p106full 1 -p106off'; m=8; big=1}
}
foreach($k in $all.Keys){
  if($Only -and -not ($Only.Split(',') -contains $k)){continue}
  "== $k"
  if($all[$k].big){& $run -Out $k -PlayerArgs $all[$k].a -Minutes $all[$k].m -Width 3840 -Height 2160 -Visible | Select-Object -First 3}
  else{& $run -Out $k -PlayerArgs $all[$k].a -Minutes $all[$k].m | Select-Object -First 3}
}
