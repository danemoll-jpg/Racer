using System;using System.IO;using System.Linq;using System.Text;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;using Racer;
// 0.87 (read only): every renderer smaller than 8 m within PROPS_R m of a branch (PROPS_BRANCH) in LakeWoods: its bottom
// against the ground (Ground* colliders) under its centre.
public static class Report087Props {
 static string P(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n;}
 public static void Run(){var o=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(o);var sb=new StringBuilder();
  var s=EditorSceneManager.OpenScene("Assets/Scenes/LakeWoods.unity");Physics.SyncTransforms();var roots=s.GetRootGameObjects();
  var br=roots.SelectMany(g=>g.GetComponentsInChildren<WoodlandRoute>(true)).First(b=>b.title==(Environment.GetEnvironmentVariable("PROPS_BRANCH")??"Summit Climb"));br.Initialize();float R=15;
  foreach(var r in roots.SelectMany(g=>g.GetComponentsInChildren<Renderer>(true))){var b=r.bounds;if(b.size.x>8||b.size.z>8||!r.enabled||!r.gameObject.activeInHierarchy)continue;float st=br.Project(b.center,out float lat);if(lat>R||st<1||st>br.Length-1)continue;
   float best=float.NaN;foreach(var h in Physics.RaycastAll(new Vector3(b.center.x,b.max.y+30,b.center.z),Vector3.down,80,~0,QueryTriggerInteraction.Ignore)){if(!h.collider.name.StartsWith("Ground"))continue;if(float.IsNaN(best)||Mathf.Abs(h.point.y-b.min.y)<Mathf.Abs(best-b.min.y))best=h.point.y;}
   float gap=b.min.y-best;sb.AppendLine($"{(Mathf.Abs(gap)>.15f?"CHECK ":"ok    ")}{gap:+0.00;-0.00} m  {P(r.transform)}  s {st:F0} lat {lat:F1} bottom {b.min.y:F2} ground {best:F2} size {b.size.x:F1}x{b.size.y:F1}x{b.size.z:F1}");}
  File.WriteAllText(o+"/props.txt",sb.ToString());EditorApplication.Exit(0);}
}
