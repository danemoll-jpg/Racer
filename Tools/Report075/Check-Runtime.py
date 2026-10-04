"""Fast type check of the runtime assembly (Assets/Scripts minus Editor folders) with optional replacement files:
Check-Runtime.py [replacement.cs ...] - a replacement replaces the file of the same name; new names are added."""
import glob,subprocess,sys,os,tempfile
U=r"C:\Program Files\Unity\Hub\Editor\6000.6.1f1\Editor\Data";R=r"C:\Users\danmo\Racer"
refs=glob.glob(U+r"\Managed\UnityEngine\*.dll")+[U+r"\NetStandard\ref\2.1.0\netstandard.dll"]+glob.glob(U+r"\NetStandard\compat\2.1.0\shims\netfx\*.dll")
refs+=[f for f in glob.glob(R+r"\Library\ScriptAssemblies\*.dll") if not os.path.basename(f).startswith("Assembly-CSharp") and "Editor" not in os.path.basename(f)]
refs+=glob.glob(R+r"\Assets\Plugins\**\*.dll",recursive=True)
src={os.path.basename(f):f for f in glob.glob(R+r"\Assets\Scripts\**\*.cs",recursive=True) if (os.sep+"Editor"+os.sep) not in f}
for f in sys.argv[1:]: src[os.path.basename(f)]=f
rsp=os.path.join(tempfile.gettempdir(),"check075rt.rsp")
with open(rsp,"w") as fh:
    fh.write("-nologo -noconfig -nostdlib -t:library -langversion:9 -unsafe -nowarn:0618,0414,0219,0168,0649,0436 -define:UNITY_EDITOR;UNITY_2021_3_OR_NEWER;UNITY_6000_0_OR_NEWER\n-out:\""+os.path.join(tempfile.gettempdir(),"check075rt.dll")+"\"\n")
    for r in refs: fh.write(f'-r:"{r}"\n')
    for s in src.values(): fh.write(f'"{s}"\n')
p=subprocess.run([U+r"\NetCoreRuntime\dotnet.exe",U+r"\DotNetSdk\sdk\8.0.318\Roslyn\bincore\csc.dll","@"+rsp],capture_output=True,text=True)
errs=[l for l in (p.stdout+p.stderr).splitlines() if " error " in l]
print("\n".join(errs[:30]) if errs else "OK (no errors)")
