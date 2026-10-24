"""Automatic-update round: install the new launcher in Dan's Builds/Latest, run Play-Racer.cmd (muted, settings restored),
and confirm the automatic check left 1.1.0-review1, its versions and every save file exactly as they were."""
import hashlib,json,pathlib,shutil,subprocess,sys
ROOT=pathlib.Path(__file__).resolve().parents[2];LATEST=ROOT/'Builds/Latest';SAVES=pathlib.Path.home()/'AppData/LocalLow/DefaultCompany/Racer'
def tree(folder):return {str(p.relative_to(folder)):hashlib.sha256(p.read_bytes()).hexdigest() for p in sorted(folder.rglob('*')) if p.is_file() and not p.name.startswith('Player')}  # Unity's own Player.log / Player-prev.log rotate on every launch
def latest():
    state=json.loads((LATEST/'state.json').read_text());state.pop('running',None)
    return dict(state=state,versions={v.name:sorted((str(p.relative_to(v)),p.stat().st_size) for p in v.rglob('*') if p.is_file()) for v in (LATEST/'versions').iterdir()},
                music=sorted((str(p.relative_to(LATEST/'shared')),p.stat().st_size) for p in (LATEST/'shared').rglob('*') if p.is_file()))
before_saves=tree(SAVES);before=latest();log_before=(LATEST/'recovery.log').read_bytes()
shutil.copy2(ROOT/'Builds/Launcher/WoodstockRushLauncher.exe',LATEST/'WoodstockRushLauncher.exe')
subprocess.run(['powershell','-NoProfile','-File',str(ROOT/'Tools/ReportAuto/Verify-ReportAutoMutedLaunch.ps1')],check=True)
after_saves=tree(SAVES);after=latest();log=(LATEST/'recovery.log').read_bytes()[len(log_before):].decode('utf-8','replace').splitlines()
changed=[k for k in before_saves if k in after_saves and before_saves[k]!=after_saves[k]];added=[k for k in after_saves if k not in before_saves]
result=dict(latestUnchanged=before==after,game=after['state']['game']['manifest']['version'],build=after['state']['game']['manifest']['build'],
            savesFilesBefore=len(before_saves),savesFilesAfter=len(after_saves),changed=changed,added=added,launcherLog=[l.split(' ',1)[1] for l in log],
            launcherSha256=hashlib.sha256((LATEST/'WoodstockRushLauncher.exe').read_bytes()).hexdigest())
(ROOT/'Docs/ReportAuto/latest-untouched.json').write_text(json.dumps(result,indent=2));print(json.dumps(result,indent=2))
sys.exit(0 if result['latestUnchanged'] and not changed and not added and result['build']==1010000 else 1)
