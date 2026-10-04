using System;using System.IO;using System.Linq;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Object=UnityEngine.Object;
// 0.74 read-only: the LakeWoods Echo Cave branch line (for a ride through the copied cave in the Mountain scenes) and the
// Backyard Reverse storm-drain branch line (for rides through the tunnel), written to Docs/Report074 as "x y z" lines.
public static class Report074Export {
 public static void Run(){
  EditorSceneManager.OpenScene("Assets/Scenes/LakeWoods.unity");var cave=Object.FindObjectsByType<Racer.WoodlandRoute>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(w=>w.title=="Echo Cave");
  File.WriteAllLines("Docs/Report074/echo-cave-route.txt",cave.points.Select(p=>$"{p.x:F3} {p.y:F3} {p.z:F3}"));
  EditorSceneManager.OpenScene("Assets/Scenes/DansBackyardReverse.unity");var drain=Object.FindObjectsByType<Racer.WoodlandRoute>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(w=>w.title.StartsWith("Storm"));
  File.WriteAllLines("Docs/Report074/storm-drain-route.txt",drain.points.Select(p=>$"{p.x:F3} {p.y:F3} {p.z:F3}"));
  File.WriteAllText("Docs/Report074/storm-drain-route-meta.txt",$"underground {drain.undergroundStart:F1}..{drain.undergroundEnd:F1} of {drain.Length:F1} m, recommended speed {drain.recommendedSpeed}");
  EditorApplication.Exit(0);}
}
