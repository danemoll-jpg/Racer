from pathlib import Path
for name in ['Build-BackyardShortcuts.cs','Stage-BackyardShortcuts.py','Verify-BackyardShortcutsPublication.py','Activate-BackyardShortcuts.ps1','Verify-BackyardShortcutsPlayRacer.ps1','Cleanup-BackyardShortcuts.ps1','Prepare-BackyardShortcuts.ps1']:
 s=Path('Tools',name).read_text(encoding='utf-8-sig')
 for a,b in [('42000','__NEXT__'),('41000','42000'),('40000','41000'),('__NEXT__','43000'),('0.42.0-review1','0.43.0-review1'),('BackyardShortcuts','ShortcutRevision'),('shortcuts-','revision-'),('Two optional Backyard forest shortcuts and updated visual map','Revised treehouse trail, cabin boards and traversable undergrowth')]:s=s.replace(a,b)
 Path('Tools',name.replace('BackyardShortcuts','ShortcutRevision')).write_text(s,encoding='utf-8')
print('Prepared existing release workflow for 43000')
