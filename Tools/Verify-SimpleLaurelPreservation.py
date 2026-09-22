import pathlib,re,subprocess
root=pathlib.Path(__file__).resolve().parents[1]
p='Assets/Scenes/StreetLoopReverse.unity'
before=subprocess.check_output(['git','show','e88f23e6:'+p],cwd=root).decode()
after=(root/p).read_text()
def behaviours(s):return {m.group(1):m.group(0) for m in re.finditer(r'^--- !u!114 &(\d+)\n.*?(?=^--- !u!|\Z)',s,re.M|re.S)}
a,b=behaviours(before),behaviours(after)
assert a==b,'A serialized behaviour changed'
changed=subprocess.check_output(['git','diff','--name-only','e88f23e6'],cwd=root,text=True).splitlines()
assert not any(p.startswith('Assets/Scripts/') for p in changed)
assert [p for p in changed if p.startswith('Assets/Scenes/')]==['Assets/Scenes/StreetLoopReverse.unity']
text=f'{len(a)} serialized MonoBehaviour blocks unchanged, including recovery, route and main-road configuration.\nAll runtime source unchanged.\nOnly StreetLoopReverse scene changed; all other scenes unchanged.\n'
(root/'Docs/SimpleLaurel/preservation.txt').write_text(text)
print(text)
