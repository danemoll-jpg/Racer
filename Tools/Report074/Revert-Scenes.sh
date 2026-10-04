#!/bin/sh
# Development helper: put the named scenes (and every non-Mountain asset changed since HEAD whose name carries the scene
# name) back to HEAD, and delete this round's generated assets for them, so a part can be re-run from a clean state.
cd /c/Users/danmo/Racer || exit 1
for sc in "$@"; do
  git checkout -- "Assets/Scenes/$sc.unity"
  git status --short | grep -v '^??' | sed 's/^ M //; s/"//g' | grep -- "$sc-" | while read -r f; do git checkout -- "$f"; done
  rm -f Assets/Track/Report074/$sc-* 
done
git status --short | grep -v '^??'
