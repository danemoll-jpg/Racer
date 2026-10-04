using System;using System.IO;using System.Linq;using System.Text;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
// 0.76 Part B audit (read-only): an inventory of the Free Roam content of each scene, so FreeRoamWorld can be compared with
// the eight course scenes. PROBE_SCENES = comma list (default: all 8 courses + FreeRoamWorld); output in PROBE_OUT.
public static class Report076Audit {
 static string P(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n.Replace('\n','/');}
 static string V(Vector3 v)=>$"({v.x:F1},{v.y:F1},{v.z:F1})";
 public static void Run(){
  var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(outDir);
  var list=(Environment.GetEnvironmentVariable("PROBE_SCENES")??"").Split(',',StringSplitOptions.RemoveEmptyEntries);
  if(list.Length==0||list[0]=="all")list=RacePlaylists.Scenes.Concat(new[]{"FreeRoamWorld"}).ToArray();
  foreach(var scene in list){
   EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");var sb=new StringBuilder();
   var race=Object.FindAnyObjectByType<RaceDirector>(FindObjectsInactive.Include);
   if(race){var rs=race.vehicle.GetComponent<VehicleRespawn>();
    sb.AppendLine($"RACE id={race.courseId} name={race.courseName} reverse={race.reverseCourse} forestTrail={(race.road?race.road.forestTrail:false)} vehicle={V(race.vehicle.transform.position)} yaw={race.vehicle.transform.eulerAngles.y:F1} spawn={(rs&&rs.spawnPoint?V(rs.spawnPoint.position)+" yaw="+rs.spawnPoint.eulerAngles.y.ToString("F1"):"none")} gate0={V(race.gates[0].transform.position)} road={(race.road?race.road.name:"none")} ambient={(race.ambientRoad?race.ambientRoad.name:"none")} through={(race.throughRoad?race.throughRoad.name:"none")}");}
   var map=Object.FindAnyObjectByType<ExplorationMap>(FindObjectsInactive.Include);
   if(map)foreach(var d in map.destinations)sb.AppendLine($"LANDMARK {d.id} | {d.title} | {V(d.position)} yaw {d.yaw:F0}");
   var col=Object.FindAnyObjectByType<ExplorationCollection>(FindObjectsInactive.Include);
   if(col)foreach(var s in col.sites)sb.AppendLine($"ACORN {s.id} | {s.title} | {V(s.position)}");
   foreach(var a in Object.FindObjectsByType<ActivitySite>(FindObjectsInactive.Include,FindObjectsSortMode.None).OrderBy(a=>a.id))sb.AppendLine($"ACTIVITY {a.id} | {a.title} | {a.kind} | {V(a.transform.position)} | active={a.gameObject.activeInHierarchy} | {P(a.transform)}");
   foreach(var f in Object.FindObjectsByType<FreeRoamOnly>(FindObjectsInactive.Include,FindObjectsSortMode.None))sb.AppendLine($"FREEROAMONLY {P(f.transform)} content={(f.content?f.content.name:"-")} meshes={f.meshes?.Length} moved={f.moved?.Length}");
   foreach(var t in new[]{typeof(AmbientLife),typeof(SnowScenes),typeof(Wildlife),typeof(UndergroundLife),typeof(WoodlandAmbience),typeof(BackyardReverseBarriers),typeof(ReverseShortcutWorldState),typeof(CircuitBoundary),typeof(MountainFlights),typeof(DumpRefuse),typeof(ShortcutUndergrowth),typeof(BreakableProp),typeof(ShallowWater),typeof(PhysicalSign),typeof(WoodlandRoute),typeof(RaceRoad),typeof(JumpRecoveryExclusion),typeof(ReverseShortcutGuidance)}){
    var objs=Object.FindObjectsByType(t,FindObjectsInactive.Include,FindObjectsSortMode.None);sb.AppendLine($"COMPONENT {t.Name} x{objs.Length}"+(objs.Length<=12?": "+string.Join("; ",objs.Select(o=>P(((Component)o).transform)+(((Component)o).gameObject.activeInHierarchy?"":" [inactive]"))):""));}
   var life=Object.FindAnyObjectByType<AmbientLife>(FindObjectsInactive.Include);if(life)sb.AppendLine($"BATS entrance={V(life.batEntrance)} roost={V(life.batRoost)} forward={V(life.batForward)}");
   var roots=EditorSceneManager.GetActiveScene().GetRootGameObjects();
   foreach(var r in roots){var rends=r.GetComponentsInChildren<Renderer>(true);var cols=r.GetComponentsInChildren<Collider>(true);
    var b=rends.Length>0?rends[0].bounds:new Bounds(r.transform.position,Vector3.zero);foreach(var x in rends)b.Encapsulate(x.bounds);
    long tris=0;foreach(var mf in r.GetComponentsInChildren<MeshFilter>(true))if(mf.sharedMesh)tris+=mf.sharedMesh.triangles.Length/3;
    sb.AppendLine($"ROOT {r.name} | active={r.activeSelf} | children={r.transform.childCount} | renderers={rends.Length} | colliders={cols.Length} | tris={tris} | bounds {V(b.min)}..{V(b.max)}");}
   // Second-level groups whose name hints at Free Roam features, with active state and size.
   var rx=new System.Text.RegularExpressions.Regex("cave|camp|homeward|cr103|cr094|summit|storm|culvert|tunnel|drain|dump|gully|rat|bat|jump|landing|path|trail|ramp|flight|echo",System.Text.RegularExpressions.RegexOptions.IgnoreCase);
   var groups=new SortedDictionary<string,(int on,int off,int tris)>();
   foreach(var mf in Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include,FindObjectsSortMode.None)){var p=P(mf.transform);if(!rx.IsMatch(p))continue;var parts=p.Split('/');var key=string.Join("/",parts.Take(Math.Min(3,parts.Length)));var g=groups.TryGetValue(key,out var v)?v:(0,0,0);bool on=mf.gameObject.activeInHierarchy;groups[key]=(g.Item1+(on?1:0),g.Item2+(on?0:1),g.Item3+(mf.sharedMesh?mf.sharedMesh.triangles.Length/3:0));}
   foreach(var kv in groups)sb.AppendLine($"FEATURE {kv.Key} | on={kv.Value.on} off={kv.Value.off} tris={kv.Value.tris}");
   File.WriteAllText(Path.Combine(outDir,"audit-"+scene+".txt"),sb.ToString());Debug.Log("REPORT076 audit "+scene);
  }
  EditorApplication.Exit(0);}
}
