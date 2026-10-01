import pathlib, subprocess, json
root=pathlib.Path(__file__).resolve().parents[1]
base='5e11668cb53f3921812c8941b883ecf132574839'
paths=[p for p in subprocess.check_output(['git','ls-files','Assets/Scenes','Assets/Scripts'],text=True).splitlines() if not p.endswith('.meta') and p!='Assets/Scenes/MountainLoopReverse.unity']
paths+=['Play-Racer.cmd','CODEX_RULES.md']
for p in paths:
    old=subprocess.check_output(['git','show',base+':'+p]).replace(b'\r\n',b'\n')
    assert old==(root/p).read_bytes().replace(b'\r\n',b'\n'),p
report=dict(baseline=base,protectedFiles=len(paths),otherScenesAndExistingGameplaySourcesUnchanged=True,mainXZAndBranchCoordinatesAndGates='See geometry-checks.txt; lower Y restored from pre-polish baseline',newFixture='Editor only, absent from saved scene')
assert 'Temporary Summit rival checks' not in (root/'Assets/Scenes/MountainLoopReverse.unity').read_text()
(root/'Docs/MountainCut/preservation.json').write_text(json.dumps(report,indent=2))
print(json.dumps(report))
