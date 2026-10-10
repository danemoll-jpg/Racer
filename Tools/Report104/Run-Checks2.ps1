# 0.104 second batch: Granite Creek Cut by each vehicle, with and without the new roadside; then frame time at 3840x2160 (full screen)
# and split-screen, before (roadside off) and after.
$r=Join-Path $PSScriptRoot 'Run-Player.ps1'; $pv=(Get-Content (Join-Path $PSScriptRoot 'perf-views.txt') -Raw).Trim()
foreach($v in 'mower','original'){ foreach($off in '','-p104off'){ $n="shortcut-$v$(if($off){'-before'})"; & $r -Out $n -PlayerArgs "-p104kind shortcut -p104vehicles $v -p104runs 3 $off" -Minutes 12 } }
foreach($split in '0','1'){ foreach($off in '-p104off',''){ $n="perf-$(if($split -eq '1'){'split'}else{'full'})$(if($off){'-before'}else{'-after'})"; & $r -Out $n -Width 3840 -Height 2160 -Visible -PlayerArgs "-p104kind perf -p104split $split $off -p104views `"$pv`"" -Minutes 12 } }
"ALL DONE"
