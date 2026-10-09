#!/bin/bash
# contact sheet of a clip: sheet.sh clip.mp4 out.png [frames=6]
FF=$(ls /c/Users/danmo/Racer/Tools/Trailer/pylib/imageio_ffmpeg/binaries/ffmpeg*.exe | head -1)
n=${3:-6}
dur=$("$FF" -i "$1" 2>&1 | grep -o "Duration: [0-9:.]*" | cut -d' ' -f2 | awk -F: '{print $1*3600+$2*60+$3}')
fps=$(awk -v n=$n -v d=$dur 'BEGIN{printf "%f", n/d}')
"$FF" -y -loglevel error -i "$1" -vf "fps=$fps,scale=640:-1,tile=3x$(( (n+2)/3 ))" -frames:v 1 "$2"
