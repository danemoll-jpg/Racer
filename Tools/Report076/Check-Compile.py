"""Fast type check of the 0.76 editor tools (plus any extra .cs given) against the Unity assemblies, without Unity.
Uses the runtime assembly built by Check-Runtime.py when present (so new runtime types are visible), else the last
Unity-compiled Assembly-CSharp."""
import glob,subprocess,sys,os,tempfile
U=r"C:\Program Files\Unity\Hub\Editor\6000.6.1f1\Editor\Data";R=r"C:\Users\danmo\Racer"
rt=os.path.join(tempfile.gettempdir(),"check076rt.dll")
runtime=rt if os.path.exists(rt) else R+r"\Library\ScriptAssemblies\Assembly-CSharp.dll"
refs=glob.glob(U+r"\Managed\UnityEngine\*.dll")+[U+r"\NetStandard\ref\2.1.0\netstandard.dll"]+glob.glob(U+r"\NetStandard\compat\2.1.0\shims\netfx\*.dll")+[runtime,R+r"\Library\ScriptAssemblies\Assembly-CSharp-Editor.dll"]+glob.glob(R+r"\Library\ScriptAssemblies\Unity.InputSystem.dll")
files=glob.glob(R+r"\Tools\Report076\*.cs")+sys.argv[1:]
rsp=os.path.join(tempfile.gettempdir(),"check076.rsp")
with open(rsp,"w") as f:
    f.write("-nologo -noconfig -nostdlib -t:library -langversion:9 -nowarn:0618,0414,0219,0168,0649,1701,1702 -define:UNITY_EDITOR\n-out:\""+os.path.join(tempfile.gettempdir(),"check076.dll")+"\"\n")
    for r in refs: f.write(f'-r:"{r}"\n')
    for s in files: f.write(f'"{s}"\n')
p=subprocess.run([U+r"\NetCoreRuntime\dotnet.exe",U+r"\DotNetSdk\sdk\8.0.318\Roslyn\bincore\csc.dll","@"+rsp],capture_output=True,text=True)
errs=[l for l in (p.stdout+p.stderr).splitlines() if " error " in l]
print("\n".join(errs[:25]) if errs else "OK (no errors)")
