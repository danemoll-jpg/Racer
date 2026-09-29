"""Remove only this task's six identified synthetic records; preserve real saved records."""
import json,pathlib,shutil,hashlib
root=pathlib.Path(__file__).resolve().parents[1]
p=pathlib.Path.home()/'AppData/LocalLow/DefaultCompany/Racer/Phase7/street-loop-gates-v1-laps3/top-ten-v1.json'
data=json.loads(p.read_text(encoding='utf-8-sig'))
own=[e for e in data['entries'] if e['category'].startswith('backyard-')]
assert len(own)==6 and all(e['date'].startswith('2026-09-29T02:1') for e in own), 'Unexpected records; do not modify'
retained=[e for e in data['entries'] if e not in own];ids={e['id'] for e in own}
backup=root/'Temp/BackyardFixtureRecordBackup';backup.mkdir(exist_ok=True)
shutil.copy2(p,backup/p.name)
data['entries']=retained;data['received']=[i for i in data['received'] if i not in ids]
replacement=p.with_name('top-ten-backyard-repair.tmp');replacement.write_text(json.dumps(data,indent=2),encoding='utf-8');replacement.replace(p)
verify=json.loads(p.read_text());assert verify['entries']==retained
(root/'Docs/Backyard/fixture-record-cleanup.json').write_text(json.dumps(dict(removedSyntheticEntries=len(own),realEntriesPreserved=len(retained),backup=str(backup),allRealEntriesUnchanged=True),indent=2))
print('Removed six fixture-created Backyard rows; all pre-existing board entries preserved exactly as data.')
