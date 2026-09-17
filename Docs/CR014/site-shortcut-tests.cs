var geometry=System.IO.File.ReadAllBytes("Docs/PHASE6_GEOMETRY.txt");
try { Racer.Editor.Phase6Validation.Geometry(); System.IO.File.Copy("Docs/PHASE6_GEOMETRY.txt","Docs/CR014/building-yard-terrain.txt",true); } finally { System.IO.File.WriteAllBytes("Docs/PHASE6_GEOMETRY.txt",geometry); }
var original=System.IO.File.ReadAllBytes("Docs/PHASE5_REALTIME.txt");var stamp=System.IO.File.GetLastWriteTimeUtc("Docs/PHASE5_REALTIME.txt");
Racer.Editor.Phase5Validation.Realtime();
UnityEditor.EditorApplication.CallbackFunction done=null;done=()=>{if(System.IO.File.GetLastWriteTimeUtc("Docs/PHASE5_REALTIME.txt")==stamp)return;UnityEditor.EditorApplication.update-=done;System.IO.File.Copy("Docs/PHASE5_REALTIME.txt","Docs/CR014/shortcut-ordinary.txt",true);System.IO.File.WriteAllBytes("Docs/PHASE5_REALTIME.txt",original);};UnityEditor.EditorApplication.update+=done;
return "yard/building/terrain checks and ordinary-frame shortcut test started";
