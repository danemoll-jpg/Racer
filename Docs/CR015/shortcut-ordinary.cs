var original=System.IO.File.ReadAllBytes("Docs/PHASE5_REALTIME.txt");var stamp=System.IO.File.GetLastWriteTimeUtc("Docs/PHASE5_REALTIME.txt");
Racer.Editor.Phase5Validation.Realtime();
UnityEditor.EditorApplication.CallbackFunction done=null;done=()=>{if(System.IO.File.GetLastWriteTimeUtc("Docs/PHASE5_REALTIME.txt")==stamp)return;UnityEditor.EditorApplication.update-=done;System.IO.File.Copy("Docs/PHASE5_REALTIME.txt","Docs/CR015/shortcut-ordinary.txt",true);System.IO.File.WriteAllBytes("Docs/PHASE5_REALTIME.txt",original);};UnityEditor.EditorApplication.update+=done;
return "Ordinary-frame shortcut started";
