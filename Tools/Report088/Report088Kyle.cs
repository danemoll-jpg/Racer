using System;using System.IO;using System.Linq;using System.Text;using System.Collections.Generic;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;using Racer;
// 0.88 Part A (read only): Kyle's house ("Friend across street - blue circle") in each scene: its transform and colliders,
// the ground around it on a 1 m grid in the house's own frame (x right = north, z forward = front / west), the driveway,
// trees and small objects within 45 m, the two-men vignette, the ground tiles there (and which scenes share them), and how
// close every race line (main, branches) of the scene passes. Writes kyle-<scene>.txt and kyle-heights-<scene>.csv.
public static class Report088Kyle {
 static string P(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n;}
 static string V(Vector3 v)=>$"{v.x:F2},{v.y:F2},{v.z:F2}";
 public static void Run(){var o=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(o);
  foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){var sb=new StringBuilder();
   try{var s=EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();var roots=s.GetRootGameObjects();
    var house=roots.SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).FirstOrDefault(t=>t.name=="Friend across street - blue circle");if(!house){File.WriteAllText($"{o}/kyle-{scene}.txt","no house");continue;}
    sb.AppendLine($"house {P(house)} pos {V(house.position)} rot {house.eulerAngles} right {V(house.right)} forward {V(house.forward)}");
    foreach(var c in house.GetComponentsInChildren<Collider>(true)){var b=c.bounds;sb.AppendLine($"  collider {P(c.transform)} {c.GetType().Name} enabled {c.enabled} bounds {V(b.min)} - {V(b.max)}");}
    foreach(var r in house.GetComponentsInChildren<Renderer>(true))sb.AppendLine($"  renderer {P(r.transform)} enabled {r.enabled} mat {r.sharedMaterial?.name}");
    foreach(Transform c in house)sb.AppendLine($"  child {c.name} local {V(c.localPosition)} scale {V(c.localScale)} prefab {PrefabUtility.IsPartOfPrefabInstance(c.gameObject)}");
    // ground grid in the house frame
    var h=new StringBuilder("# local x (right, north) from -40 to 40, z (forward, front/west) from -40 to 40, 1 m; ground = highest Ground* hit\n");
    for(int z=-40;z<=40;z++){var row=new List<string>();for(int x=-40;x<=40;x++){var w=house.TransformPoint(new Vector3(x,0,z));float best=float.NaN;string what="";
      foreach(var hit in Physics.RaycastAll(new Vector3(w.x,w.y+60,w.z),Vector3.down,140,~0,QueryTriggerInteraction.Ignore)){if(!hit.collider.name.StartsWith("Ground"))continue;if(float.IsNaN(best)||hit.point.y>best){best=hit.point.y;what=hit.collider.name;}}
      row.Add(float.IsNaN(best)?"":(best-house.position.y).ToString("F2",System.Globalization.CultureInfo.InvariantCulture)+(what.Contains("drive")||what.Contains("Kyle")?"d":""));}h.AppendLine(string.Join(",",row));}
    File.WriteAllText($"{o}/kyle-heights-{scene}.csv",h.ToString());
    var near=new Bounds(house.position,new Vector3(90,80,90));
    foreach(var mc in roots.SelectMany(g=>g.GetComponentsInChildren<MeshCollider>(true)).Where(c=>c.name.StartsWith("Ground")&&c.sharedMesh&&c.bounds.Intersects(near))){var path=AssetDatabase.GetAssetPath(mc.sharedMesh);
     sb.AppendLine($"ground {P(mc.transform)} {path} verts {mc.sharedMesh.vertexCount} renderer-shares {mc.GetComponent<MeshFilter>()?.sharedMesh==mc.sharedMesh} scenes {string.Join(",",AssetDatabase.FindAssets("t:Scene",new[]{"Assets/Scenes"}).Select(AssetDatabase.GUIDToAssetPath).Where(sp=>AssetDatabase.GetDependencies(sp,false).Contains(path)).Select(Path.GetFileNameWithoutExtension))}");}
    foreach(var rr in roots.SelectMany(g=>g.GetComponentsInChildren<RaceRoad>(true)).Where(r=>r.name.Contains("Kyle"))){sb.AppendLine($"road {P(rr.transform)} points {string.Join(" ",rr.points.Select(V))}");}
    foreach(var c in roots.SelectMany(g=>g.GetComponentsInChildren<Collider>(true))){if(c.transform.IsChildOf(house)||c.isTrigger)continue;var b=c.bounds;if(!near.Intersects(b)||b.size.x>60||b.size.z>60)continue;var l=house.InverseTransformPoint(new Vector3(b.center.x,b.min.y,b.center.z));if(Mathf.Abs(l.x)>45||Mathf.Abs(l.z)>45)continue;
     sb.AppendLine($"  near {(c.name.ToLower().Contains("trunk")||c.name.ToLower().Contains("tree")?"tree":"obj")} {P(c.transform)} {c.GetType().Name} local {V(l)} size {V(b.size)}");}
    foreach(var al in roots.SelectMany(g=>g.GetComponentsInChildren<AmbientLife>(true))){if(al.smoking!=null)sb.AppendLine($"vignette smoking {string.Join(" ",al.smoking.Select(p=>V(p)+" local "+V(house.InverseTransformPoint(p))))}");}
    // 0.88: mailboxes, activity sites, every renderer within 45 m (non-vegetation) with material, all RaceRoads within 60 m
    foreach(var t in roots.SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).Where(t=>t.name.StartsWith("Mailbox")))sb.AppendLine($"mailbox {P(t)} at {V(t.position)} yaw {t.eulerAngles.y:F0} local {V(house.InverseTransformPoint(t.position))} prefab {PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(t.gameObject)} colliders {t.GetComponentsInChildren<Collider>(true).Length}");
    foreach(var a in roots.SelectMany(g=>g.GetComponentsInChildren<ActivitySite>(true))){var l=house.InverseTransformPoint(a.transform.position);sb.AppendLine($"activity {a.id} '{a.title}' at {V(a.transform.position)} local {V(l)} radius {a.radius} props {(a.props==null?0:a.props.Length)}");if(l.magnitude<90&&a.props!=null)foreach(var bp in a.props)if(bp)sb.AppendLine($"   prop {P(bp.transform)} {V(bp.transform.position)} local {V(house.InverseTransformPoint(bp.transform.position))}");}
    foreach(var r in roots.SelectMany(g=>g.GetComponentsInChildren<Renderer>(true))){var b=r.bounds;var l=house.InverseTransformPoint(b.center);if(Mathf.Abs(l.x)>45||Mathf.Abs(l.z)>45)continue;var m=r.sharedMaterial;var veg=m&&m.HasProperty("_Vegetation")&&m.GetFloat("_Vegetation")>.5f;if(veg)continue;sb.AppendLine($"  render {P(r.transform)} mat {m?.name} local {V(l)} size {V(b.size)} col {(r.GetComponent<Collider>()?r.GetComponent<Collider>().GetType().Name:"-")} mesh {(r.GetComponent<MeshFilter>()?AssetDatabase.GetAssetPath(r.GetComponent<MeshFilter>().sharedMesh):"")}");}
    foreach(var rr in roots.SelectMany(g=>g.GetComponentsInChildren<RaceRoad>(true))){rr.Initialize();float best=1e9f;Vector3 bp=default;foreach(var q in rr.points){float dd=Vector2.Distance(new(q.x,q.z),new(house.position.x,house.position.z));if(dd<best){best=dd;bp=q;}}if(best<80)sb.AppendLine($"raceroad {P(rr.transform)} nearest pt {best:F1} at {V(bp)} local {V(house.InverseTransformPoint(bp))}");}
    // race lines
    foreach(var d in roots.SelectMany(g=>g.GetComponentsInChildren<RaceDirector>(true))){var rd=d.road;rd.Initialize();float best=1e9f,bs=0;for(float st=0;st<rd.Length;st+=1){var q=rd.At(st,out _);float dd=Vector2.Distance(new(q.x,q.z),new(house.position.x,house.position.z));if(dd<best){best=dd;bs=st;}}
     sb.AppendLine($"race {d.courseId} main nearest {best:F1} m at s {bs:F0} ({V(rd.At(bs,out _))}, local {V(house.InverseTransformPoint(rd.At(bs,out _)))}) halfWidth {rd.HalfWidth(bs):F1}");
     foreach(var b in roots.SelectMany(g=>g.GetComponentsInChildren<WoodlandRoute>(true))){b.Initialize();float bb=1e9f;Vector3 bp=default;foreach(var q in b.points){float dd=Vector2.Distance(new(q.x,q.z),new(house.position.x,house.position.z));if(dd<bb){bb=dd;bp=q;}}sb.AppendLine($"  branch {b.title} nearest {bb:F1} m at {V(bp)} local {V(house.InverseTransformPoint(bp))} hw {b.halfWidth}");}
     var fl=roots.SelectMany(g=>g.GetComponentsInChildren<ForestLayout>(true)).FirstOrDefault();}
   }catch(Exception e){sb.AppendLine("ERROR "+e);}
   File.WriteAllText($"{o}/kyle-{scene}.txt",sb.ToString());}
  EditorApplication.Exit(0);}
}
