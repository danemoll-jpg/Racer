#!/bin/sh
# 0.117: the course files from the 0.116 ones, in order (each step reads the previous one's output). Run from the project root.
set -e
L=${1:-$LOCALAPPDATA/Temp/report117/lines}; G=${2:-$LOCALAPPDATA/Temp/report117/gridJF/grid-J.txt}
python Tools/Report117/py/splice.py "$L" "$G"
python Tools/Report117/py/blocks.py "$L"
python Tools/Report117/py/extras.py
python Tools/Report117/py/signs.py
python Tools/Report117/py/previews.py
python Tools/Report117/py/audit.py
