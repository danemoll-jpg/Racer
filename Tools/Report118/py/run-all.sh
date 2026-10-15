#!/bin/sh
# 0.118: the course files from the 0.117 ones (git 16ddd31c), in order. Run from the project root.
#  1. Report118Bank (Unity, read-only): Tools/Report118/Run-Unity.ps1 -Method Report118Bank.Run -Scenes 'MountainLoop;MountainLoopReverse' -Out bank -NoGraphics
#  2. bank.py: Kyle's bank, one even slope (Part A2) -> bank-reshape.json
#  3. step1.py: Dan's two closures out (Part A), the bank on both courses, the arrow boards out and the turns listed (Part B) -> stage1-*.json
#  4. Report118Gates (Unity, read-only): $env:PROBE_COURSES='LeviathanForward;LeviathanReverse'; Run-Unity.ps1 -Method Report118Gates.Run -Out gates -NoGraphics
#  5. step3.py: the gates added, renumbered, the course ids -> Assets/Resources/Leviathan/*.json
#  6. the Tracks preview (Tools/Report117/py/previews.py, unchanged: it reads the course files)
set -e
B=${1:-$LOCALAPPDATA/Temp/report118/bank/bankgrid-MountainLoop.txt}; G=${2:-$LOCALAPPDATA/Temp/report118/gates}
python Tools/Report118/py/bank.py "$B"
python Tools/Report118/py/step1.py "$B"
echo "now run Report118Gates (step 4), then: python Tools/Report118/py/step3.py $G && python Tools/Report117/py/previews.py"
