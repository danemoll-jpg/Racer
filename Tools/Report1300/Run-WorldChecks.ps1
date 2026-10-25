# 1.3 Free Roam world checks in the check build (Builds/Check1300), each on its own copy of Dan's save (Run-Player.ps1), windowed and minimized.
# -Data: items from that file instead of the built-in Resources/RoamWorld.json (placing them).
param([string[]]$Only=@(),[string]$Data='',[string]$Tag='')
$ErrorActionPreference='Stop'
$run=Join-Path $PSScriptRoot 'Run-Player.ps1'
$d=if($Data){"-p1300data `"$Data`""}else{''}
function Want($n){ return $Only.Count -eq 0 -or $Only -contains $n }
if(Want 'shots'){ & $run -Out "shots$Tag" -Width 1600 -Height 900 -PlayerArgs "-p106kind world1300 -p1300part shots -p106w 1600 -p106h 900 $d" -Minutes 15 }
if(Want 'jumps'){ & $run -Out "jumps$Tag" -Width 1280 -Height 720 -PlayerArgs "-p106kind world1300 -p1300part jumps -p1300high 2 -p106w 1280 -p106h 720 $d" -Minutes 20 }
if(Want 'flips'){ & $run -Out "flips$Tag" -Width 1280 -Height 720 -PlayerArgs "-p106kind world1300 -p1300part flips -p106w 1280 -p106h 720 $d" -Minutes 10 }
if(Want 'trees'){ & $run -Out "trees$Tag" -Width 1280 -Height 720 -PlayerArgs "-p106kind world1300 -p1300part trees -p106w 1280 -p106h 720 $d" -Minutes 10 }
if(Want 'loops'){ & $run -Out "loops$Tag" -Width 1280 -Height 720 -PlayerArgs "-p106kind world1300 -p1300part loops -p106w 1280 -p106h 720 $d" -Minutes 6 }
$views='-p1300views "forest-west|-370|50|90;forest-middle|-70|-10|45;forest-north-east|230|230|0"'
if(Want 'perf'){ & $run -Out "perf-before$Tag" -Width 1920 -Height 1080 -PlayerArgs "-p106kind world1300 -p1300part perf -p1300off $views -p106w 1920 -p106h 1080" -Minutes 8
                 & $run -Out "perf-after$Tag" -Width 1920 -Height 1080 -PlayerArgs "-p106kind world1300 -p1300part perf $views -p106w 1920 -p106h 1080 $d" -Minutes 8 }
if(Want 'smash'){ & $run -Out "smash$Tag" -Width 1280 -Height 720 -PlayerArgs "-p106kind world1300 -p1300part smash -p1300spots 4 -p106w 1280 -p106h 720 $d" -Minutes 10 }
