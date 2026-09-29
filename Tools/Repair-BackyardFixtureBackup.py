"""Remove only identified test rows from the original board backup; retain real saves."""
import json,pathlib,shutil
root=pathlib.Path(__file__).resolve().parents[1]
p=pathlib.Path.home()/'AppData/LocalLow/DefaultCompany/Racer/Phase7/street-loop-gates-v1-laps3/top-ten-v1.json.bak'
d=json.loads(p.read_text(encoding='utf-8-sig'))
own=[e for e in d['entries'] if e['category'].startswith('backyard-')]
assert all(e['date'].startswith('2026-09-29T02:1') for e in own),'Unexpected later user records; stop'
if own:
 backup=root/'Temp/BackyardFixtureRecordBackup';backup.mkdir(exist_ok=True)
 shutil.copy2(p,backup/p.name)
 kept=[e for e in d['entries'] if e not in own];ids={e['id'] for e in own}
 d['entries']=kept;d['received']=[i for i in d['received'] if i not in ids]
 tmp=p.with_name('backyard-backup-repair.tmp');tmp.write_text(json.dumps(d,indent=2),encoding='utf-8');tmp.replace(p)
 assert json.loads(p.read_text())['entries']==kept
 (root/'Docs/Backyard/fixture-backup-cleanup.json').write_text(json.dumps(dict(removedSyntheticEntries=len(own),realEntriesPreserved=len(kept),allRealEntriesUnchanged=True),indent=2))
print('Backup repaired:',len(own),'test rows; all real records preserved.')
