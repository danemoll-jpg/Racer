"""Bounded updater fixtures: no game/audio/race regression tests."""
import base64, copy, functools, hashlib, http.server, importlib.util, json, pathlib, subprocess, threading, time, zipfile
ROOT=pathlib.Path(__file__).resolve().parents[1]
spec=importlib.util.spec_from_file_location('release',ROOT/'Tools/Prepare-LauncherRelease.py');release=importlib.util.module_from_spec(spec);spec.loader.exec_module(release)
from cryptography.hazmat.primitives import serialization
key=serialization.load_pem_private_key((ROOT/'Builds/PublisherPrivate/launcher-signing.pem').read_bytes(),None)
root=ROOT/'Temp'/('cr119-fixtures-'+str(time.time_ns()));root.mkdir();assets=root/'assets';assets.mkdir();installation=root/'install';saves=root/'saves';saves.mkdir();(saves/'settings.json').write_text('{"master":0,"marker":"keep"}')
requests=[]
class Handler(http.server.SimpleHTTPRequestHandler):
    def do_GET(self): requests.append(self.path);super().do_GET()
    def log_message(self,*a): pass
server=http.server.ThreadingHTTPServer(('127.0.0.1',0),functools.partial(Handler,directory=str(assets)));threading.Thread(target=server.serve_forever,daemon=True).start();base=f'http://127.0.0.1:{server.server_port}'
results=[]
def run(command,*args,ok=True,error=None):
    p=subprocess.run([str(ROOT/'Builds/Launcher/LauncherChecks.exe'),str(installation),str(saves),command,*map(str,args)],capture_output=True,text=True,timeout=40)
    assert (p.returncode==0)==ok,(command,p.stdout,p.stderr)
    if error: assert error in p.stderr,(command,p.stderr)
    return p.stdout
def record(name): results.append(dict(check=name,outcome='PASS'))
def signed(name,payload):
    path=assets/name;path.write_text(json.dumps(release.sign(payload,key)));return path
def file(name,data):
    path=assets/name;path.write_bytes(data);return dict(path=name,bytes=len(data),sha256=hashlib.sha256(data).hexdigest(),url=base+'/'+name)
def music(build,files):return dict(schema=1,type='soundtrack',platform='windows-x64',minLauncher=1,build=build,version=str(build),files=files)
def game(build,unsafe=False):
    name=f'g{build}.zip';data=f'fixture executable marker {build}'.encode();archive=assets/name
    with zipfile.ZipFile(archive,'w') as z:z.writestr('../escape.exe' if unsafe else 'Racer.exe',data)
    return dict(schema=1,type='game',platform='windows-x64',minLauncher=1,build=build,version=str(build),files=[dict(path='Racer.exe',bytes=len(data),sha256=hashlib.sha256(data).hexdigest())],package=dict(url=base+'/'+name,bytes=archive.stat().st_size,sha256=release.digest(archive)))
try:
    old=game(1);old_dir=root/'old';old_dir.mkdir();(old_dir/'Racer.exe').write_bytes(b'fixture executable marker 1')
    tracks=[file(n,d) for n,d in [('stay.mp3',b'unchanged'),('change.mp3',b'original'),('withdraw.mp3',b'withdraw'),('local.mp3',b'managed')]]
    old_music=root/'old-music';old_music.mkdir()
    for f in tracks:(old_music/f['path']).write_bytes((assets/f['path']).read_bytes())
    run('seed',old_dir,signed('old.json',old),old_music,signed('music1.json',music(1,tracks)));record('signed starter inventory and isolated saves')
    new=game(2);new_path=signed('new.json',new)
    run('game',new_path);assert json.loads(run('status'))['game']['manifest']['build']==2;record('signed old-to-new activation')
    assert (saves/'settings.json').read_text()=='{"master":0,"marker":"keep"}'
    assert list((installation/'save-backups').rglob('settings.json'));record('pre-update save backup; current save unchanged')
    run('rollback');assert json.loads(run('status'))['game']['manifest']['build']==1;record('rollback keeps current saves')
    run('check','http://127.0.0.1:1/missing',ok=False);record('offline leaves installed version selected')
    unsigned=assets/'unsigned.json';unsigned.write_text(json.dumps(new));run('verify',unsigned,ok=False)
    altered=json.loads(new_path.read_text());raw=bytearray(base64.b64decode(altered['payload']));raw[-2]^=1;altered['payload']=base64.b64encode(raw).decode();unsigned.write_text(json.dumps(altered));run('verify',unsigned,ok=False);record('unsigned and changed signature payload rejected')
    corrupt=game(3);corrupt['package']['sha256']='0'*64;run('game',signed('corrupt.json',corrupt),ok=False);record('corrupt package rejected before activation')
    run('cancel-game',signed('cancel.json',game(6)),ok=False,error='cancelled');record('cancel download before activation')
    run('busy-game',signed('busy.json',game(7)),ok=False,error='Game is running');record('running game defers installation')
    bad=game(4,True);run('game',signed('unsafe.json',bad),ok=False);assert not (installation/'escape.exe').exists();record('ZIP traversal rejected')
    state=json.loads((installation/'state.json').read_text());state['fixtureAvailableBytes']=0;(installation/'state.json').write_text(json.dumps(state));run('game',signed('space.json',game(5)),ok=False);state.pop('fixtureAvailableBytes');(installation/'state.json').write_text(json.dumps(state));record('insufficient storage keeps current game')
    (installation/'transaction.json').write_text(json.dumps(dict(phase='prepared',kind='game',transaction='interruption-fixture')));run('status');assert json.loads((installation/'transaction.json').read_text())['phase']=='recovered';record('interrupted activation uses prior atomic pointer')
    personal=installation/'shared/PersonalMusic';(personal/'mine.mp3').write_bytes(b'personal');managed=installation/'shared/soundtracks/1';(managed/'local.mp3').write_bytes(b'locally modified');(managed/'unknown.mp3').write_bytes(b'unknown')
    next_tracks=[tracks[0],file('change.mp3',b'changed'),file('add.mp3',b'added')]
    requests.clear();run('music',signed('music2.json',music(2,next_tracks)));assert sorted(requests)==['/add.mp3','/change.mp3'],requests
    for name,data in [('mine.mp3',b'personal'),('local.mp3',b'locally modified'),('unknown.mp3',b'unknown')]:assert (personal/name).read_bytes()==data
    assert not (installation/'shared/soundtracks/2/withdraw.mp3').exists();assert (managed/'withdraw.mp3').exists();record('music add/change/withdraw; unchanged bytes reused; personal/modified/unknown preserved')
    assert json.loads(run('status'))['game']['manifest']['build']==1;record('soundtrack activation leaves game pointer unchanged')
    (installation/'transaction.json').write_text(json.dumps(dict(phase='prepared',kind='soundtrack',transaction='music-interruption')));run('status');assert json.loads(run('status'))['music']['manifest']['build']==2;record('soundtrack interrupted transaction recovery')
finally:
    server.shutdown();out=ROOT/'Docs/CR119/core-fixtures.json';out.write_text(json.dumps(dict(root=str(root),checks=results),indent=2));print(out);print(json.dumps(results,indent=2))
