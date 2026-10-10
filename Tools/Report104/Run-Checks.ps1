# The 0.104 built-player checks, one after another (each its own player run on a copy of Dan's save).
$r=Join-Path $PSScriptRoot 'Run-Player.ps1'
& $r -Out lap-forward -PlayerArgs "-p104kind lap -p104course 0 -p104vehicle original" -Minutes 18
& $r -Out shortcut -PlayerArgs "-p104kind shortcut -p104vehicles mower,original -p104runs 3" -Minutes 15
& $r -Out menus -PlayerArgs "-p104kind menus" -Minutes 10
& $r -Out police -PlayerArgs "-p104kind police" -Minutes 10
& $r -Out top-after -PlayerArgs "-p104kind top" -Minutes 10
& $r -Out top-before -PlayerArgs "-p104kind top -p104off" -Minutes 10
"ALL DONE"
