using System;using System.IO;using System.Linq;using System.Text;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
// 0.76 Part B: turns the FreeRoamWorld scene (a copy of Dan's Backyard Loop - Reverse) into the Free Roam world.
// WORLD_DRY=1 reports only. Notes go to PROBE_OUT/world-notes.txt.
public static class Report076World {
 static readonly StringBuilder notes=new();static void Note(string s){notes.AppendLine(s);Debug.Log("REPORT076 "+s);}
 static string P(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n.Replace('\n','/');}
 static string V(Vector3 v)=>$"({v.x:F1},{v.y:F1},{v.z:F1})";
 public static void Run(){bool dry=Environment.GetEnvironmentVariable("WORLD_DRY")=="1";var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");
  try{
   // 1. Activity sites of every course scene: id, kind, position, record course.
   var legacy=new List<(string id,ActivitySite.Kind kind,Vector3 p,string course,string scene)>();
   foreach(var scene in RacePlaylists.Scenes){EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");var race=Object.FindAnyObjectByType<RaceDirector>();
    foreach(var a in Object.FindObjectsByType<ActivitySite>(FindObjectsSortMode.None))legacy.Add((a.id,a.kind,a.transform.position,ArcadeActivities.ActivityCourse(race.courseId),scene));}
   var world=EditorSceneManager.OpenScene("Assets/Scenes/FreeRoamWorld.unity");var wr=Object.FindAnyObjectByType<RaceDirector>();
   Note($"FreeRoamWorld course id {wr.courseId} -> free-roam-world");if(!dry){wr.courseId="free-roam-world";wr.courseName="Free Roam";EditorUtility.SetDirty(wr);}
   foreach(var a in Object.FindObjectsByType<ActivitySite>(FindObjectsInactive.Include,FindObjectsSortMode.None).OrderBy(a=>a.id)){
    var same=legacy.Where(l=>l.id==a.id&&l.kind==a.kind&&Vector3.Distance(l.p,a.transform.position)<15).ToList();
    var keys=same.Select(l=>l.id+"/"+l.course).Distinct().ToArray();
    Note($"ACTIVITY {a.id} {a.title} at {V(a.transform.position)}: records carried from {string.Join(", ",same.Select(l=>l.scene+" ("+l.course+")"))}");
    if(!dry){a.legacyRecords=keys;EditorUtility.SetDirty(a);}}
   foreach(var l in legacy.Where(l=>!Object.FindObjectsByType<ActivitySite>(FindObjectsInactive.Include,FindObjectsSortMode.None).Any(a=>a.id==l.id&&a.kind==l.kind&&Vector3.Distance(l.p,a.transform.position)<15)))
    Note($"NOT CARRIED activity {l.id} ({l.kind}) at {V(l.p)} in {l.scene}");
   // 2. Race guidance of the Backyard Reverse race (ground arrows / direction markers): listed; hidden unless dry.
   var arrows=Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None).Where(r=>{var p=P(r.transform);var n=r.name.ToLowerInvariant();return !r.GetComponentInParent<PhysicalSign>()&&(n.Contains("arrow")||n.Contains("chevron"));}).ToArray();
   foreach(var g in arrows.GroupBy(r=>P(r.transform.parent?r.transform.parent:r.transform)))Note($"ARROWS {g.Key}: {g.Count()} ({string.Join(", ",g.Select(r=>r.name).Distinct().Take(4))})");
   // The Backyard races' own direction arrows mean nothing in the one Free Roam world (the map shows any course's route).
   foreach(var r in arrows.Where(r=>r.name=="Reverse teal ground arrow"||r.name=="Gold optional route chevron")){if(r.GetComponents<Component>().Length>3)continue;if(!dry)r.gameObject.SetActive(false);}
   Note($"ARROWS hidden: {arrows.Count(r=>r.name=="Reverse teal ground arrow")} Backyard Reverse teal ground arrows, {arrows.Count(r=>r.name=="Gold optional route chevron")} Backyard Forward gold shortcut chevrons");
   // 3. The winding driveway: batched tree pieces near it with no trunk under them, or inside the driving width.
   var drive=Object.FindObjectsByType<RaceRoad>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(r=>r.name=="House 3 valley driveway");drive.Initialize();
   var pts=drive.points.ToArray();float Dist(Vector3 q){float best=1e9f;for(int i=0;i+1<pts.Length;i++){var a=pts[i];var b=pts[i+1];var ab=new Vector2(b.x-a.x,b.z-a.z);var aq=new Vector2(q.x-a.x,q.z-a.z);float t=Mathf.Clamp01(Vector2.Dot(aq,ab)/Mathf.Max(1e-6f,ab.sqrMagnitude));best=Mathf.Min(best,(aq-ab*t).magnitude);}return best;}
   var trunks=Object.FindObjectsByType<Collider>(FindObjectsSortMode.None).Where(c=>c.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0&&Dist(c.bounds.center)<25).ToArray();
   Note($"DRIVE {pts.Length} points {V(pts[0])}..{V(pts[^1])}; trunks within 25 m: {trunks.Length}; within 4 m: {string.Join(", ",trunks.Where(t=>Dist(t.bounds.center)<4).Select(t=>V(t.bounds.center)+" "+Dist(t.bounds.center).ToString("F1")))}");
   if(!dry)FixDriveTrees(Dist,trunks);
   foreach(var mf in Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Where(m=>m.sharedMesh&&!m.GetComponent<Collider>()&&m.GetComponent<Renderer>()&&(m.name.IndexOf("tree",StringComparison.OrdinalIgnoreCase)>=0||m.name.Contains("canopy")||m.name.Contains("woodland")||m.name.Contains("foliage")||m.name.Contains("timber")||m.name.Contains("crowns")))){
    var m=mf.sharedMesh;var tri=m.triangles;var par=Enumerable.Range(0,m.vertexCount).ToArray();int F(int x){while(par[x]!=x){par[x]=par[par[x]];x=par[x];}return x;}
    for(int i=0;i<tri.Length;i+=3){int a=F(tri[i]),b=F(tri[i+1]);par[b]=a;int c=F(tri[i+2]);par[c]=F(a);}
    var w=m.vertices.Select(v=>mf.transform.TransformPoint(v)).ToArray();
    foreach(var g in new HashSet<int>(tri).GroupBy(F)){var vs=g.ToArray();var b=new Bounds(w[vs[0]],Vector3.zero);foreach(int v in vs)b.Encapsulate(w[v]);var c=b.center;float d=Dist(c);if(d>12)continue;
     bool trunk=trunks.Any(t=>new Vector2(t.bounds.center.x-c.x,t.bounds.center.z-c.z).magnitude<3.5f);
     float ground=float.NaN;foreach(var h in Physics.RaycastAll(new Vector3(c.x,b.max.y+30,c.z),Vector3.down,120,~0,QueryTriggerInteraction.Ignore).OrderBy(h=>h.distance)){if(h.collider.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0)continue;ground=h.point.y;break;}
     bool inWidth=d<4.5f&&b.min.y<ground+4;bool floating=b.min.y>ground+6&&!trunk;bool buried=b.max.y<ground+.2f;
     if(!trunk||inWidth||buried)Note($"TREE PIECE {P(mf.transform)} {vs.Length} verts at {V(c)} size {V(b.size)} drive dist {d:F1} ground {ground:F1} min {b.min.y:F1}: {(trunk?"":"no trunk; ")}{(inWidth?"IN DRIVING WIDTH; ":"")}{(floating?"floating; ":"")}{(buried?"buried; ":"")}");}}
   if(!dry){EditorSceneManager.MarkSceneDirty(world);EditorSceneManager.SaveScene(world);AssetDatabase.SaveAssets();}
  }finally{File.WriteAllText(Path.Combine(outDir,"world-notes.txt"),notes.ToString());}
  EditorApplication.Exit(0);}
 // 0.74 moved seven trees out of the winding driveway with only one of each tree's three canopy lobes; the other two
 // stayed at the old spot (floating over the drive). They follow their tree now. Trunkless crowns hanging in the air
 // within 12 m of the drive (pre-existing, now beside it) are removed. FreeRoamWorld's own mesh copies only.
 static readonly (Vector3 at,Vector3 delta)[] Moved={(new(472.8f,76.5f,-171.7f),new(-2.97f,-.03f,1.82f)),(new(449.8f,64.3f,-200.6f),new(-2.29f,-.03f,3.09f)),(new(455.2f,67.0f,-195.5f),new(-2.43f,-.02f,2.42f)),(new(479.4f,77.5f,-165.1f),new(-6.40f,.03f,3.43f)),(new(487.3f,80.5f,-159.9f),new(2.06f,.36f,-.86f)),(new(467.6f,71.2f,-185.0f),new(-5.16f,-.02f,3.60f)),(new(466.0f,70.6f,-193.0f),new(2.55f,-.02f,-2.02f))};
 static Mesh OwnMesh(MeshFilter mf){var m=mf.sharedMesh;var path=AssetDatabase.GetAssetPath(m);if(path.StartsWith("Assets/Track/FreeRoamWorld/"))return m;
  var copy=Object.Instantiate(m);copy.name=m.name;var dst="Assets/Track/FreeRoamWorld/FreeRoamWorld-"+System.IO.Path.GetFileNameWithoutExtension(path)+".asset";AssetDatabase.CreateAsset(copy,dst);mf.sharedMesh=copy;Note($"own mesh copy {dst} for {P(mf.transform)}");return copy;}
 static void FixDriveTrees(Func<Vector3,float> dist,Collider[] trunks){
  foreach(var mf in Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Where(m=>m.sharedMesh&&!m.GetComponent<Collider>()&&(m.name.Contains("woodland canopy")||m.name.Contains("Complete authored trees"))).ToArray()){
   var m=mf.sharedMesh;var tri=m.triangles;var par=Enumerable.Range(0,m.vertexCount).ToArray();int F(int x){while(par[x]!=x){par[x]=par[par[x]];x=par[x];}return x;}
   for(int i=0;i<tri.Length;i+=3){int a=F(tri[i]),b=F(tri[i+1]);par[b]=a;int c=F(tri[i+2]);par[c]=F(a);}
   var w=m.vertices.Select(v=>mf.transform.TransformPoint(v)).ToArray();var move=new Dictionary<int,Vector3>();var drop=new HashSet<int>();
   foreach(var g in new HashSet<int>(tri).GroupBy(F)){var vs=g.ToArray();var b=new Bounds(w[vs[0]],Vector3.zero);foreach(int v in vs)b.Encapsulate(w[v]);var c=b.center;
    if(trunks.Any(t=>new Vector2(t.bounds.center.x-c.x,t.bounds.center.z-c.z).magnitude<3.5f))continue;
    var hit=Moved.Where(t=>new Vector2(t.at.x-c.x,t.at.z-c.z).magnitude<3f).ToArray();
    if(hit.Length==1&&mf.name.Contains("woodland canopy")){foreach(int v in vs)move[v]=hit[0].delta;Note($"lobe at {V(c)} follows its tree {V(hit[0].at)} by {V(hit[0].delta)}");continue;}
    if(dist(c)>12||vs.Length<5)continue;
    float ground=float.NaN;foreach(var h in Physics.RaycastAll(new Vector3(c.x,b.max.y+30,c.z),Vector3.down,150,~0,QueryTriggerInteraction.Ignore).OrderBy(h=>h.distance)){if(h.collider.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0)continue;ground=h.point.y;break;}
    if(b.min.y>ground+4){foreach(int v in vs)drop.Add(v);Note($"trunkless crown {V(c)} size {V(b.size)} {b.min.y-ground:F1} m above the ground removed ({P(mf.transform)})");}}
   if(move.Count==0&&drop.Count==0)continue;m=OwnMesh(mf);var lv=m.vertices;foreach(var kv in move){var q=mf.transform.TransformPoint(lv[kv.Key]);lv[kv.Key]=mf.transform.InverseTransformPoint(q+kv.Value);}m.vertices=lv;
   for(int sub=0;sub<m.subMeshCount;sub++){var t=m.GetTriangles(sub);var keep=new List<int>();for(int i=0;i<t.Length;i+=3)if(!drop.Contains(t[i]))keep.AddRange(new[]{t[i],t[i+1],t[i+2]});m.SetTriangles(keep,sub);}
   m.RecalculateBounds();EditorUtility.SetDirty(m);}
  Physics.SyncTransforms();}
}
public static partial class Report076WorldTrees {}
public static class Report076TreeCompare {
 // Are batched tree pieces at these centres present in the race scene too (pre-existing) ?
 public static void Run(){var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");var sb=new StringBuilder();
  var probes=new[]{new Vector3(140.0f,66f,69.9f),new Vector3(138.5f,64f,53.1f)};
  foreach(var scene in new[]{"DansBackyardReverse","FreeRoamWorld"}){EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");sb.AppendLine("SCENE "+scene);
   foreach(var mf in Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Where(m=>m.sharedMesh&&!m.GetComponent<Collider>()&&(m.name.Contains("tree")||m.name.Contains("canopy")))){
    var m=mf.sharedMesh;var tri=m.triangles;var par=Enumerable.Range(0,m.vertexCount).ToArray();int F(int x){while(par[x]!=x){par[x]=par[par[x]];x=par[x];}return x;}
    for(int i=0;i<tri.Length;i+=3){int a=F(tri[i]),b=F(tri[i+1]);par[b]=a;int c=F(tri[i+2]);par[c]=F(a);}
    var w=m.vertices.Select(v=>mf.transform.TransformPoint(v)).ToArray();
    foreach(var g in new HashSet<int>(tri).GroupBy(F)){var vs=g.ToArray();var bb=new Bounds(w[vs[0]],Vector3.zero);foreach(int v in vs)bb.Encapsulate(w[v]);
     foreach(var p in probes)if((bb.center-p).magnitude<6)sb.AppendLine($"  near {p}: {mf.name} {vs.Length} verts centre {bb.center:F1} size {bb.size:F1}");}}}
  File.WriteAllText(Path.Combine(outDir,"tree-compare.txt"),sb.ToString());EditorApplication.Exit(0);}
}
