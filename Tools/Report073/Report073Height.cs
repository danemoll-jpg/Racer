using System;using System.IO;using System.Linq;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 0.73 read-only: highest collider / renderer top in each scene (the cloud layer must stay far above it).
public static class Report073Height{public static void Run(){var o=Environment.GetEnvironmentVariable("PROBE_OUT");var rows=new System.Collections.Generic.List<string>();
 foreach(var s in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){EditorSceneManager.OpenScene("Assets/Scenes/"+s+".unity");
  var c=UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsSortMode.None).Where(x=>x.enabled&&x.gameObject.activeInHierarchy).OrderByDescending(x=>x.bounds.max.y).First();
  var r=UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None).Where(x=>x.enabled&&x.gameObject.activeInHierarchy&&!(x is ParticleSystemRenderer)).OrderByDescending(x=>x.bounds.max.y).First();
  rows.Add($"{s}: highest collider {c.name} top {c.bounds.max.y:F1} m; highest renderer {r.name} top {r.bounds.max.y:F1} m");}
 File.WriteAllLines(o+"/heights.txt",rows);EditorApplication.Exit(0);}}
