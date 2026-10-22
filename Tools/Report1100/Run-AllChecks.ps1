# 1.1: the replay checks in the check build, one after another (each on its own copy of Dan's save; -p1100part best on an empty save is not needed: its board is new)
$ErrorActionPreference='Continue'
$run=Join-Path $PSScriptRoot 'Run-Player.ps1'
foreach($part in @('main','best','stunt','split','lev')){ & $run -Out $part -Build 'Builds/Check1100' -PlayerArgs "-p106kind replay1100 -p1100part $part" -Minutes 30 }
