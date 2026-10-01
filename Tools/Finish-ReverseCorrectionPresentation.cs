using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;public static class FinishReverseCorrectionPresentation {public static string Main(){Views();Previews();return "Final views and saved course previews captured";}static string Views(){System.Action work=()=>{try{
UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/DansBackyardReverse.unity");var branches=UnityEngine.Object.FindObjectsByType<Racer.WoodlandRoute>();var ridge=branches.First(b=>b.title.StartsWith("Abandoned"));var drain=branches.First(b=>b.title.StartsWith("Storm"));var g=new GameObject("Temporary final shortcut review");var cam=g.AddComponent<Camera>();cam.CopyFrom(Camera.main);cam.enabled=false;bool fog=RenderSettings.fog;RenderSettings.fog=false;
void Shot(string name,Vector3 eye,Vector3 target,float size=0){cam.orthographic=size>0;cam.orthographicSize=Mathf.Max(size,1);cam.fieldOfView=65;cam.transform.SetPositionAndRotation(eye,Quaternion.LookRotation(target-eye));var rt=new RenderTexture(1500,950,24);var prior=RenderTexture.active;cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;var tex=new Texture2D(1500,950,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1500,950),0,0);tex.Apply();System.IO.File.WriteAllBytes("Docs/ReverseCorrection/"+name+".png",tex.EncodeToPNG());RenderTexture.active=prior;cam.targetTexture=null;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(tex);}
Shot("exit-to-jump",new Vector3(110,33,-66.1f),new Vector3(88,38,-66.1f));Shot("ridge-exposure",new Vector3(354,96,80),ridge.At(50,out _));Shot("logging-entry",new Vector3(398,85,78),ridge.At(24,out _)+Vector3.up);Shot("logging-ridge",ridge.At(40,out _)+Vector3.up*2,ridge.At(57,out _)+Vector3.up);
Shot("drain-entrance",new Vector3(178,69,81),drain.At(28,out _)+Vector3.up*2);Shot("drain-interior",drain.At(45,out _)+Vector3.up*2,drain.At(62,out _)+Vector3.up*2);Shot("gully-exit",new Vector3(99,34,-66),new Vector3(118,34,-66));Shot("gully-jump",new Vector3(110,72,-95),new Vector3(90,34,-66),40);
RenderSettings.fog=fog;UnityEngine.Object.DestroyImmediate(g);UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/DansBackyardForward.unity");var state=UnityEngine.Object.FindAnyObjectByType<Racer.ReverseShortcutWorldState>();typeof(Racer.ReverseShortcutWorldState).GetMethod("OnEnable",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).Invoke(state,null);var race=UnityEngine.Object.FindAnyObjectByType<Racer.RaceDirector>();race.FreeRoam=false;state.Refresh();g=new GameObject("Temporary Forward grate review");cam=g.AddComponent<Camera>();cam.CopyFrom(Camera.main);cam.enabled=false;fog=RenderSettings.fog;RenderSettings.fog=false;Shot("forward-closed-grate",new Vector3(178,69,81),new Vector3(163,64,73));RenderSettings.fog=fog;UnityEngine.Object.DestroyImmediate(g);UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/DansBackyardReverse.unity");System.IO.File.WriteAllText("Docs/ReverseCorrection/views-done.txt","Seven actual geometry/state views; no scene changes saved");
}catch(Exception e){System.IO.File.WriteAllText("Docs/ReverseCorrection/views-error.txt",e.ToString());}};work();return "Capture final shortcut geometry and Forward grate state";


}
static string Previews(){System.Action work=()=>{try{
 if(UnityEditor.EditorApplication.isPlaying||UnityEditor.EditorApplication.isCompiling)throw new Exception("Saved edit mode required");
 var original=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
 if(original.isDirty)throw new Exception("Unsaved scene must be preserved");
 var catalog=new Racer.CoursePreviewCatalog();var courses=new List<Racer.CoursePreviewCatalog.Course>();
 foreach(string name in Racer.RacePlaylists.Scenes){
  string path="Assets/Scenes/"+name+".unity";var scene=UnityEngine.SceneManagement.SceneManager.GetSceneByPath(path);bool opened=!scene.IsValid()||!scene.isLoaded;
  if(opened)scene=UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path,UnityEditor.SceneManagement.OpenSceneMode.Additive);
  try{
   if(scene.isDirty)throw new Exception("Unsaved course: "+name);
   var roots=scene.GetRootGameObjects();var race=roots.SelectMany(g=>g.GetComponentsInChildren<Racer.RaceDirector>(true)).Single();
   courses.Add(new Racer.CoursePreviewCatalog.Course{scene=name,id=race.courseId,main=race.road.points.ToArray(),gates=race.gates.Select(g=>g.transform.position).ToArray(),branches=roots.SelectMany(g=>g.GetComponentsInChildren<Racer.WoodlandRoute>()).Where(b=>b.gameObject.activeInHierarchy).Select(b=>new Racer.CoursePreviewCatalog.Path{points=b.points.ToArray(),undergroundStart=b.undergroundStart,undergroundEnd=b.undergroundEnd}).ToArray()});
  }finally{if(opened)UnityEditor.SceneManagement.EditorSceneManager.CloseScene(scene,true);}
 }
 UnityEngine.SceneManagement.SceneManager.SetActiveScene(original);catalog.courses=courses.ToArray();
 System.IO.File.WriteAllText("Assets/Resources/WorldMaps/CoursePreviews.json",JsonUtility.ToJson(catalog,true));UnityEditor.AssetDatabase.Refresh();
 System.IO.File.WriteAllText("Docs/ReverseCorrection/capture.txt",string.Join("\n",courses.Select(c=>c.scene+" | "+c.id+" | main="+c.main.Length+" branches="+c.branches.Length+" gates="+c.gates.Length)));
}catch(Exception e){System.IO.File.WriteAllText("Docs/ReverseCorrection/capture-error.txt",e.ToString());}};work();return "Scheduled saved-course preview capture; scenes will not be saved";

}}
