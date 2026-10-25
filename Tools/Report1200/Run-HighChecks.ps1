# 1.2 HIGH MODE checks in the check build (Builds/Check1200), each on its own copy of Dan's save (Run-Player.ps1), windowed and minimized:
#   pages at 1280x800 and at 3840x2160 (the menus by the controller only, one shot of each HIGH MODE page, then RUN FROM THE COPS started),
#   a HIGH MODE lap of each mountain course steering at the edges, a crooked landing (HIGH, then normal), a HIGH race's records, the wreck question.
param([string[]]$Only=@())
$ErrorActionPreference='Stop'
$run=Join-Path $PSScriptRoot 'Run-Player.ps1'
function Want($n){ return $Only.Count -eq 0 -or $Only -contains $n }
if(Want 'pages'){ & $run -Out 'pages-1280x800' -Width 1280 -Height 800 -PlayerArgs '-p106kind high1200 -p1200part pages -p106w 1280 -p106h 800' -Minutes 12
                  & $run -Out 'pages-3840x2160' -Width 3840 -Height 2160 -PlayerArgs '-p106kind high1200 -p1200part pages -p106w 3840 -p106h 2160' -Minutes 12 }
if(Want 'mountain'){ foreach($c in 4,5){ & $run -Out "mountain-$c" -Width 1280 -Height 720 -PlayerArgs "-p106kind high1200 -p1200part mountain -p1200course $c" -Minutes 20 } }
if(Want 'landing'){ & $run -Out 'landing' -Width 1280 -Height 720 -PlayerArgs '-p106kind high1200 -p1200part landing' -Minutes 8 }
if(Want 'records'){ & $run -Out 'records' -Width 1280 -Height 720 -PlayerArgs '-p106kind high1200 -p1200part records' -Minutes 15 }
if(Want 'prompt'){ & $run -Out 'prompt' -Width 1280 -Height 720 -PlayerArgs '-p106kind high1200 -p1200part prompt' -Minutes 8 }
