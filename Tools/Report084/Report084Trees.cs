using System;using System.IO;using System.Linq;using System.Text;using System.Collections.Generic;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;
// 0.84 Part K (BUG-001 "floating trees", ForestLoopReverse only): tree-trunk colliders whose base is more than 0.3 m above
// the ground under it (the ground there was lowered by later edits: the 0.31 pool basin and lake-exit bank, the 0.29 hill
// smoothing, the cuttings). Each is moved straight down onto that ground (the 0.78 scenery draws the tree from the trunk's
// base, so the drawn tree follows), unless it would then stand on a route (the main or a branch, within its half-width +
// 1.5 m): such a trunk was hanging over a trail and is removed. Nothing else in the scene changes. TREES_DRY=1 lists only.
public static class Report084Trees {
 static string P(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n;}
 static string V(Vector3 v)=>$"{v.x:F2},{v.y:F2},{v.z:F2}";
 static bool TreeLike(Collider c)=>c.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0||c.name.IndexOf("tree",StringComparison.OrdinalIgnoreCase)>=0;
 public static void Run(){bool dry=Environment.GetEnvironmentVariable("TREES_DRY")=="1";var o=Environment.GetEnvironmentVariable("PROBE_OUT");var sb=new StringBuilder();
  try{var scene=EditorSceneManager.OpenScene("Assets/Scenes/ForestLoopReverse.unity");Physics.SyncTransforms();var roots=scene.GetRootGameObjects();
   var race=roots.SelectMany(g=>g.GetComponentsInChildren<Racer.RaceDirector>(true)).First();var road=race.road;road.Initialize();
   var branches=roots.SelectMany(g=>g.GetComponentsInChildren<Racer.WoodlandRoute>(true)).ToList();foreach(var b in branches)b.Initialize();
   string OnRoute(Vector3 p){float s=road.Project(p,out float lat);if(lat<road.HalfWidth(s)+1.5f)return $"main s {s:F0} ({lat:F1} m)";foreach(var b in branches){float bs=b.Project(p,out float bl);if(bl<b.halfWidth+1.5f)return $"{b.title} s {bs:F0} ({bl:F1} m)";}return null;}
   var trunks=roots.SelectMany(g=>g.GetComponentsInChildren<Collider>(true)).Where(c=>(c is BoxCollider||c is CapsuleCollider)&&!c.isTrigger&&c.enabled&&c.gameObject.activeInHierarchy&&c.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0).ToList();
   int seated=0,removed=0;var toRemove=new List<Collider>();
   foreach(var c in trunks){var b=c.bounds;var bottom=new Vector3(b.center.x,b.min.y,b.center.z);
    var hits=Physics.RaycastAll(bottom+Vector3.up*.5f,Vector3.down,80,~0,QueryTriggerInteraction.Ignore).Where(h=>!TreeLike(h.collider)).OrderBy(h=>h.distance).ToArray();if(hits.Length==0)continue;
    float gap=bottom.y-hits[0].point.y;if(gap<=.3f)continue;var route=OnRoute(hits[0].point);
    if(route!=null){removed++;toRemove.Add(c);sb.AppendLine($"REMOVE {P(c.transform)} [{c.GetType().Name}] base {V(bottom)} floating {gap:F2} m over {hits[0].collider.name}: it hangs over {route}");continue;}
    seated++;sb.AppendLine($"SEAT {P(c.transform)} [{c.GetType().Name}] base {V(bottom)} down {gap:F2} m onto {hits[0].collider.name}");
    if(!dry){Undo.RecordObject(c.transform,"seat");c.transform.position-=Vector3.up*gap;}}
   if(!dry)foreach(var c in toRemove){var go=c.gameObject;if(go.GetComponents<Component>().Length==2&&go.transform.childCount==0)UnityEngine.Object.DestroyImmediate(go);else UnityEngine.Object.DestroyImmediate(c);}
   sb.Insert(0,$"ForestLoopReverse: {trunks.Count} trunk colliders; seated {seated}, removed {removed}{(dry?" (dry run)":"")}\n");
   if(!dry){EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);}
  }catch(Exception e){sb.AppendLine("ERROR "+e);}
  Directory.CreateDirectory(o);File.WriteAllText(o+"/K-trunks.txt",sb.ToString());EditorApplication.Exit(0);}
}
// 0.84 Part I (approved by Dan 2026-10-06): the hidden tree-trunk collider 0.79 kept on the Dan's Backyard Reverse trail at
// (208.0, 72.35, 89.8), 2.3 m from the race line, removed; the same one in Dan's Backyard Forward if it is there. Nothing
// else in those scenes changes. TREES_DRY=1 lists only.
public static class Report084Stray {
 public static void Run(){bool dry=Environment.GetEnvironmentVariable("TREES_DRY")=="1";var o=Environment.GetEnvironmentVariable("PROBE_OUT");var sb=new StringBuilder();var at=new Vector3(208.0f,72.35f,89.8f);
  try{foreach(var name in new[]{"DansBackyardReverse","DansBackyardForward"}){var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+name+".unity");int n=0;
    foreach(var c in scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Collider>(true)).Where(c=>c.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0).ToList()){
     var b=c.bounds;var bottom=new Vector3(b.center.x,b.min.y,b.center.z);var d=bottom-at;if(new Vector2(d.x,d.z).magnitude>.6f||Mathf.Abs(d.y)>1.5f)continue;
     var t=c.transform;string path=t.name;var p=t.parent;while(p){path=p.name+"/"+path;p=p.parent;}
     sb.AppendLine($"{name}: REMOVE {path} [{c.GetType().Name}] base ({bottom.x:F2}, {bottom.y:F2}, {bottom.z:F2}), components {string.Join(",",c.GetComponents<Component>().Select(x=>x.GetType().Name))}, children {t.childCount}");n++;
     if(!dry){var go=c.gameObject;if(go.GetComponents<Component>().Length==2&&t.childCount==0)UnityEngine.Object.DestroyImmediate(go);else UnityEngine.Object.DestroyImmediate(c);}}
    sb.AppendLine($"{name}: {n} removed{(dry?" (dry run)":"")}");
    if(!dry&&n>0){EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);}}
  }catch(Exception e){sb.AppendLine("ERROR "+e);}
  Directory.CreateDirectory(o);File.WriteAllText(o+"/I-stray.txt",sb.ToString());EditorApplication.Exit(0);}
}
