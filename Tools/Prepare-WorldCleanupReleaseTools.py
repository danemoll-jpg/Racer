from pathlib import Path
r=Path(__file__).resolve().parents[1]
for prefix,ext in [('Build','cs'),('Prepare','ps1'),('Stage','py'),('Activate','ps1'),('Verify','py'),('Cleanup','ps1')]:
 name=f'{prefix}-WorldMap.{ext}'
 if prefix=='Verify':continue
 s=(r/'Tools'/name).read_text(encoding='utf-8-sig')
 for a,b in [('44000','45000'),('43000','44000'),('0.44.0','0.45.0'),('WorldMap','WorldCleanup'),('worldmap','worldcleanup')]:s=s.replace(a,b)
 if prefix=='Prepare':s=s.replace("Join-Path 'Docs/WorldCleanup' $file","Join-Path 'Docs/WorldMap' $file")
 (r/'Tools'/f'{prefix}-WorldCleanup.{ext}').write_text(s,encoding='utf-8')
for suffix in ['Publication.py','PlayRacer.ps1']:
 s=(r/'Tools'/('Verify-WorldMap'+suffix)).read_text(encoding='utf-8-sig')
 for a,b in [('44000','45000'),('43000','44000'),('0.44.0','0.45.0'),('WorldMap','WorldCleanup'),('worldmap','worldcleanup')]:s=s.replace(a,b)
 (r/'Tools'/('Verify-WorldCleanup'+suffix)).write_text(s,encoding='utf-8')
print('Release tools derived from existing publisher workflow.')
