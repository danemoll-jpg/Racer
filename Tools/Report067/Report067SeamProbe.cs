using System;using System.Reflection;using UnityEditor;using UnityEditor.SceneManagement;
// Read-only diagnostic: runs the seam pass on an opened scene with SEAM_DEBUG logging and exits without saving.
public static class Report067SeamProbe {
 public static void Run(){EditorSceneManager.OpenScene("Assets/Scenes/"+Environment.GetEnvironmentVariable("PROBE_SCENES")+".unity");UnityEngine.Physics.queriesHitBackfaces=true;
  var t=Type.GetType("Report067Author")??Array.Find(AppDomain.CurrentDomain.GetAssemblies(),a=>a.GetType("Report067Author")!=null).GetType("Report067Author");
  t.GetMethod("Seams",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,null);EditorApplication.Exit(0);}
}
