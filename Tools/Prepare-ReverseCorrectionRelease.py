"""Reuse the established signed game-component delivery, with a new identity."""
from pathlib import Path
root=Path(__file__).resolve().parents[1]
for prefix,suffix in [('Build','cs'),('Prepare','ps1'),('Stage','py'),('Verify','Publication.py'),('Verify','MutedLaunch.ps1'),('Activate','ps1'),('Verify','PlayRacer.ps1'),('Cleanup','ps1')]:
    old=f'{prefix}-ReverseShortcuts'+('.'+suffix if '.' not in suffix else suffix)
    new=old.replace('ReverseShortcuts','ReverseCorrection')
    source=root/'Tools'/old
    text=source.read_text(encoding='utf-8-sig')
    text=text.replace('0.54.0','0.55.0').replace('54000','55000').replace('53000','54000')
    text=text.replace('ReverseShortcuts','ReverseCorrection').replace('reverseshortcuts','reversecorrection')
    text=text.replace('Two optional player-only Backyard Reverse shortcuts','Two corrected rival-AI-capable Backyard Reverse shortcuts')
    text=text.replace('Player-only shortcuts; detailed timing and handling await Dan review.','Raised ridge, extended wet drain, gully-wall exit and straight jump; both branches support rival AI. Handling awaits Dan review.')
    text=text.replace('global vehicle/AI settings','global vehicle settings and existing probabilistic AI choice')
    (root/'Tools'/new).write_text(text,encoding='utf-8')
print('Prepared existing publisher/staging/verification workflow for 55000.')
