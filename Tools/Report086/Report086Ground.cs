using System;using System.IO;using System.Linq;using System.Text;using System.Collections.Generic;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;using Racer;
// 0.86 Part A (read only): what lies under and around the House 3 pool in each scene (PROBE_SCENES=a,b): every collider and
// renderer whose bounds reach the pool's outer footprint + 6 m, the down-ray stack at the pool centre and corners, and for
// every mesh there, its asset, whether another scene's object shares it, and its vertices inside the footprint + 3 m.
public static class Report086Ground {
 static string V(Vector3 v)=>$"{v.x:F2},{v.y:F2},{v.z:F2}";
 static string P(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n;}
 public static void Run(){var o=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(o);var sb=new StringBuilder();
  var area=new Bounds(new Vector3(416,34,-197),new Vector3(19.2f+12,40,17.2f+12));var tight=new Bounds(new Vector3(416,34,-197),new Vector3(19.2f+6,40,17.2f+6));
  foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){var s=EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();var roots=s.GetRootGameObjects();
   sb.AppendLine("== "+scene);
   foreach(var c in roots.SelectMany(g=>g.GetComponentsInChildren<Collider>(true))){if(!c.bounds.Intersects(area))continue;var mc=c as MeshCollider;
    sb.AppendLine($"  C {P(c.transform)} [{c.GetType().Name}] en {c.enabled&&c.gameObject.activeInHierarchy} trig {c.isTrigger} b {V(c.bounds.center)} s {V(c.bounds.size)}{(mc&&mc.sharedMesh?" mesh "+AssetDatabase.GetAssetPath(mc.sharedMesh)+"#"+mc.sharedMesh.name+" v"+mc.sharedMesh.vertexCount+" rd "+mc.sharedMesh.isReadable:"")}");}
   foreach(var r in roots.SelectMany(g=>g.GetComponentsInChildren<MeshRenderer>(true))){if(!r.bounds.Intersects(area)||r.bounds.size.x>400)continue;var mf=r.GetComponent<MeshFilter>();
    if(r.name.IndexOf("tree",StringComparison.OrdinalIgnoreCase)>=0)continue;
    sb.AppendLine($"  R {P(r.transform)} on {r.enabled&&r.gameObject.activeInHierarchy} b {V(r.bounds.center)} s {V(r.bounds.size)} mesh {(mf&&mf.sharedMesh?AssetDatabase.GetAssetPath(mf.sharedMesh)+"#"+mf.sharedMesh.name+" v"+mf.sharedMesh.vertexCount:"-")} mat {(r.sharedMaterial?r.sharedMaterial.name:"-")}");}
   foreach(var p in new[]{new Vector3(416,0,-197),new Vector3(407.3f,0,-204.8f),new Vector3(424.7f,0,-189.3f),new Vector3(405.5f,0,-197),new Vector3(426.5f,0,-197),new Vector3(416,0,-206.5f),new Vector3(416,0,-187.5f)}){
    sb.AppendLine($"  stack at {p.x:F1},{p.z:F1}: "+string.Join(" | ",Physics.RaycastAll(new Vector3(p.x,80,p.z),Vector3.down,120,~0,QueryTriggerInteraction.Ignore).OrderBy(h=>h.distance).Select(h=>$"{h.collider.name} {h.point.y:F2}")));}
   // Meshes of ground colliders reaching the tight footprint: vertices inside it (world), sorted.
   foreach(var mc in roots.SelectMany(g=>g.GetComponentsInChildren<MeshCollider>(true)).Where(c=>c.bounds.Intersects(tight)&&c.sharedMesh)){var m=mc.sharedMesh;if(!m.isReadable){sb.AppendLine("  unreadable "+m.name);continue;}
    var t=mc.transform;var vs=m.vertices.Select(v=>t.TransformPoint(v)).Where(w=>w.x>403.4f&&w.x<428.6f&&w.z>-208.6f&&w.z<-185.4f).ToList();
    var xs=vs.Select(v=>Mathf.Round(v.x*100)/100).Distinct().OrderBy(x=>x).ToList();var zs=vs.Select(v=>Mathf.Round(v.z*100)/100).Distinct().OrderBy(x=>x).ToList();
    sb.AppendLine($"  mesh {P(t)} {m.name}: {vs.Count} vertices in footprint+3 m; distinct x {xs.Count}: {string.Join(" ",xs.Take(60))}; distinct z {zs.Count}: {string.Join(" ",zs.Take(60))}; y {(vs.Count>0?vs.Min(v=>v.y):0):F2}..{(vs.Count>0?vs.Max(v=>v.y):0):F2}");
    var users=new List<string>();foreach(var g in AssetDatabase.FindAssets("t:Scene",new[]{"Assets/Scenes"})){}
    sb.AppendLine("   dependents of asset: scenes listing it: "+string.Join(",",AssetDatabase.FindAssets("t:Scene",new[]{"Assets/Scenes"}).Select(AssetDatabase.GUIDToAssetPath).Where(sp=>AssetDatabase.GetDependencies(sp,false).Contains(AssetDatabase.GetAssetPath(m))).Select(Path.GetFileNameWithoutExtension)));}
  }
  File.WriteAllText(o+"/ground.txt",sb.ToString());EditorApplication.Exit(0);}
}
