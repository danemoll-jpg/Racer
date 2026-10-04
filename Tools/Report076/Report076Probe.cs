using System;using System.IO;using System.Linq;using System.Text;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
// 0.76 Part B probes (read-only). Cave(): which terrain/tree objects lie under the LakeWoods Echo Cave in LakeWoods and in
// FreeRoamWorld (mesh identity and vertex hash). Points(): what supports each course start and each activity site in
// FreeRoamWorld.
public static class Report076Probe {
 static string P(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n.Replace('\n','/');}
 static string V(Vector3 v)=>$"({v.x:F1},{v.y:F1},{v.z:F1})";
 static string Out=>Environment.GetEnvironmentVariable("PROBE_OUT");
 static string Hash(Mesh m,Transform t){if(!m)return "-";unchecked{long h=17;foreach(var v in m.vertices){var w=t.TransformPoint(v);h=h*31+Mathf.RoundToInt(w.x*100);h=h*31+Mathf.RoundToInt(w.y*100);h=h*31+Mathf.RoundToInt(w.z*100);}return h.ToString("x");}}
 public static void Cave(){var sb=new StringBuilder();var area=new Bounds(new Vector3(50,40,95),new Vector3(300,120,440));
  foreach(var scene in new[]{"LakeWoods","FreeRoamWorld","MountainLoop"}){EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");sb.AppendLine("SCENE "+scene);
   foreach(var mf in Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include,FindObjectsSortMode.None).OrderBy(m=>P(m.transform))){var r=mf.GetComponent<Renderer>();var c=mf.GetComponent<Collider>();var b=r?r.bounds:(c?c.bounds:default);if(!b.Intersects(area))continue;var p=P(mf.transform);
    if(!(p.StartsWith("Memory loop")||p.Contains("Ground")||p.StartsWith("Woods")||p.StartsWith("World cleanup")))continue;
    sb.AppendLine($"  {p} | active={mf.gameObject.activeInHierarchy} | {AssetDatabase.GetAssetPath(mf.sharedMesh)} | {mf.sharedMesh?.name} | hash {Hash(mf.sharedMesh,mf.transform)} | {V(b.min)}..{V(b.max)}");}
   // tree trunk colliders inside the cave hillside footprint
   var trunks=Object.FindObjectsByType<Collider>(FindObjectsSortMode.None).Where(c=>c.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0&&new Bounds(new Vector3(50,40,95),new Vector3(300,120,440)).Contains(c.bounds.center)).ToArray();
   sb.AppendLine($"  trunks in area: {trunks.Length}; hash {string.Join("",trunks.Select(t=>V(t.bounds.center))).GetHashCode():x}");}
  File.WriteAllText(Path.Combine(Out,"cave-probe.txt"),sb.ToString());EditorApplication.Exit(0);}
 static string Under(Vector3 p,out float drop){drop=float.NaN;foreach(var h in Physics.RaycastAll(p+Vector3.up*3,Vector3.down,60,~0,QueryTriggerInteraction.Ignore).OrderBy(h=>h.distance)){if(h.collider.attachedRigidbody)continue;drop=p.y-h.point.y;return P(h.collider.transform);}return "nothing within 60 m";}
 public static void Points(){var sb=new StringBuilder();
  // course starts and activity sites read from the course scenes, then tested in FreeRoamWorld
  var starts=new List<(string,Vector3,float)>();var sites=new List<(string,string,Vector3)>();
  foreach(var scene in RacePlaylists.Scenes){EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");var race=Object.FindAnyObjectByType<RaceDirector>();var rs=race.vehicle.GetComponent<VehicleRespawn>();
   var t=rs&&rs.spawnPoint?rs.spawnPoint:race.vehicle.transform;starts.Add((scene,t.position,t.eulerAngles.y));
   foreach(var a in Object.FindObjectsByType<ActivitySite>(FindObjectsSortMode.None))sites.Add((scene,a.id+" "+a.title,a.transform.position));}
  EditorSceneManager.OpenScene("Assets/Scenes/FreeRoamWorld.unity");Physics.SyncTransforms();
  foreach(var (s,p,y) in starts){var u=Under(p,out float d);sb.AppendLine($"START {s} {V(p)} yaw {y:F0} -> {u} drop {d:F2}");}
  foreach(var (s,n,p) in sites.Distinct()){var u=Under(p,out float d);sb.AppendLine($"SITE {s} {n} {V(p)} -> {u} drop {d:F2}");}
  File.WriteAllText(Path.Combine(Out,"points-probe.txt"),sb.ToString());EditorApplication.Exit(0);}
}
public static class Report076CaveMap {
 static string Out=>Environment.GetEnvironmentVariable("PROBE_OUT");
 // Terrain height grid (Memory loop Ground tiles only) and the cave/backyard footprints, LakeWoods vs FreeRoamWorld.
 static float[,] Grid(out bool[,] cave,out bool[,] yard,float x0,float z0,int nx,int nz,float step,StringBuilder sb){
  var h=new float[nx,nz];cave=new bool[nx,nz];yard=new bool[nx,nz];
  var tiles=Object.FindObjectsByType<MeshCollider>(FindObjectsSortMode.None).Where(c=>c.transform.parent&&c.transform.parent.name.StartsWith("Memory loop")&&c.name.StartsWith("Ground_")).ToArray();
  var caveCols=Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None).Where(r=>{var n=r.name;return n.Contains("Echo Cave")||n.Contains("cave")||n.Contains("Cave");}).ToArray();
  var yardR=Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None).Where(r=>{var t=r.transform;string p=t.name;while(t.parent){t=t.parent;p=t.name+"/"+p;}return p.Contains("Culvert")||p.Contains("culvert")||p.Contains("Gully")||p.Contains("gully")||p.Contains("dump")||p.Contains("Dump")||p.Contains("Refuse")||p.Contains("Reverse optional forest shortcuts");}).ToArray();
  foreach(var r in caveCols)sb.AppendLine($"  cave piece {r.name} {r.bounds.min:F0}..{r.bounds.max:F0}");
  for(int i=0;i<nx;i++)for(int j=0;j<nz;j++){float x=x0+i*step,z=z0+j*step;h[i,j]=float.NaN;var ray=new Ray(new Vector3(x,400,z),Vector3.down);
   foreach(var t in tiles)if(t.Raycast(ray,out var hit,600)){h[i,j]=float.IsNaN(h[i,j])?hit.point.y:Mathf.Max(h[i,j],hit.point.y);}
   var pt=new Vector3(x,0,z);foreach(var r in caveCols){var b=r.bounds;if(x>=b.min.x&&x<=b.max.x&&z>=b.min.z&&z<=b.max.z&&b.size.x<120&&b.size.z<120){cave[i,j]=true;break;}}
   foreach(var r in yardR){var b=r.bounds;if(x>=b.min.x&&x<=b.max.x&&z>=b.min.z&&z<=b.max.z&&b.size.x<150&&b.size.z<150){yard[i,j]=true;break;}}}
  return h;}
 public static void Run(){float x0=-140,z0=-160,step=4;int nx=95,nz=125;var sb=new StringBuilder();
  EditorSceneManager.OpenScene("Assets/Scenes/LakeWoods.unity");Physics.SyncTransforms();sb.AppendLine("LakeWoods cave pieces:");var a=Grid(out var cave,out _,x0,z0,nx,nz,step,sb);
  EditorSceneManager.OpenScene("Assets/Scenes/FreeRoamWorld.unity");Physics.SyncTransforms();sb.AppendLine("FreeRoamWorld:");var b=Grid(out _,out var yard,x0,z0,nx,nz,step,sb);
  sb.AppendLine($"map x {x0}..{x0+(nx-1)*step} (left->right), z {z0+(nz-1)*step} (top) .. {z0} (bottom), {step} m cells. C cave footprint (LakeWoods), Y backyard feature (FreeRoamWorld), digit |dh| m (0 = same, 9 = 9+), . same terrain, # both C and changed");
  for(int j=nz-1;j>=0;j--){var line=new StringBuilder($"{z0+j*step,6:F0} ");for(int i=0;i<nx;i++){float d=Mathf.Abs(a[i,j]-b[i,j]);bool ch=float.IsNaN(d)?true:d>.05f;char c=cave[i,j]?(ch?'#':'C'):yard[i,j]?'Y':ch?(float.IsNaN(d)?'?':(char)('0'+Mathf.Clamp(Mathf.CeilToInt(d),0,9))):'.';line.Append(c);}sb.AppendLine(line.ToString());}
  File.WriteAllText(Path.Combine(Out,"cave-map.txt"),sb.ToString());EditorApplication.Exit(0);}
}
