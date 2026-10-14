# small exact-replacement helper for the 0.115 source edits: keeps each file's own line endings; every "old" must occur exactly once
import sys
def edit(p, pairs):
    s = open(p, encoding='utf-8', newline='').read(); nl = '\r\n' if '\r\n' in s else '\n'
    for old, new in pairs:
        o = old.replace('\n', nl); n = new.replace('\n', nl)
        c = s.count(o)
        if c != 1: raise SystemExit(f'{p}: {c} matches for {old[:90]!r}')
        s = s.replace(o, n)
    open(p, 'w', encoding='utf-8', newline='').write(s)
