using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
// 0.80 local geometry repairs for Dan's debug session 2026-10-05_14-24-33-951_f0ab5f (PROJECT_TODO "CURRENT" Part A 5-7).
// Every change is local and logged to Docs/Report080/author-notes.txt. No route, checkpoint, physics or AI data changes.
// Section 5A: driving surfaces, route corridors, jump systems and the 0.68-0.70 berms are never edited or built over.
// AUTHOR_PARTS=b008,... AUTHOR_SCENES=MountainLoopReverse,... AUTHOR_DRY=1 (measure only, not saved)
public static partial class Report080Author {
 const string Folder="Assets/Track/Report080";
 static readonly List<string> notes=new();
 static void Note(string s){notes.Add(s);Debug.Log("REPORT080 "+s);}
 static string Scene=>UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
 static Mesh Store(string name,Mesh mesh){Directory.CreateDirectory(Folder);string path=$"{Folder}/{Scene}-{name}.asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(old){EditorUtility.CopySerialized(mesh,old);Object.DestroyImmediate(mesh);return old;}AssetDatabase.CreateAsset(mesh,path);return mesh;}
 static GameObject Make(string name,Mesh mesh,Material mat){foreach(var old in Object.FindObjectsByType<MeshFilter>().Where(x=>x.name==name).ToArray())Object.DestroyImmediate(old.gameObject);
  var g=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer),typeof(MeshCollider));g.GetComponent<MeshFilter>().sharedMesh=mesh;g.GetComponent<MeshRenderer>().sharedMaterial=mat;g.GetComponent<MeshCollider>().sharedMesh=mesh;GameObjectUtility.SetStaticEditorFlags(g,StaticEditorFlags.BatchingStatic);Physics.SyncTransforms();return g;}
 static void Save(){var s=UnityEngine.SceneManagement.SceneManager.GetActiveScene();EditorSceneManager.MarkSceneDirty(s);EditorSceneManager.SaveScene(s);AssetDatabase.SaveAssets();}
 static bool IsDrive(Collider c)=>c&&c.name.Contains("driving surface");
 static RaceDirector Race=>Object.FindAnyObjectByType<RaceDirector>(FindObjectsInactive.Include);
 // route centre lines (main + branches) as point lists with half-widths
 static List<(string n,Vector3[] p,float hw)> Routes(){var r=Race;var list=new List<(string,Vector3[],float)>();if(r&&r.road){r.road.Initialize();var pts=new List<Vector3>();for(float s=0;s<r.road.Length;s+=1)pts.Add(r.road.At(s,out _));list.Add(("Main",pts.ToArray(),8.2f));}
  foreach(var b in Object.FindObjectsByType<WoodlandRoute>()){if(!b.gameObject.activeInHierarchy||b.points==null)continue;list.Add((b.title,b.points,b.halfWidth));}return list;}
 // within a route's corridor (horizontal distance < half-width + pad, height within -1.5..+6 m of the route)
 static string Corridor(List<(string n,Vector3[] p,float hw)> routes,Vector3 q,float pad){foreach(var (n,p,hw) in routes)foreach(var c in p){float dy=q.y-c.y;if(dy<-1.5f||dy>6)continue;if(new Vector2(c.x-q.x,c.z-q.z).magnitude<hw+pad)return n;}return null;}
 // the first static solid surface below p, ignoring 'skip'
 static bool Below(Vector3 p,Collider skip,out RaycastHit hit){var hs=Physics.RaycastAll(p+Vector3.up*.02f,Vector3.down,80,~0,QueryTriggerInteraction.Ignore).Where(h=>h.collider!=skip&&!h.collider.attachedRigidbody).OrderBy(h=>h.distance).ToArray();hit=hs.FirstOrDefault();return hs.Length>0;}

 public static void Run(){
  if(Application.isPlaying)throw new Exception("Edit mode required");Directory.CreateDirectory("Docs/Report080");Physics.queriesHitBackfaces=false;
  var parts=(Environment.GetEnvironmentVariable("AUTHOR_PARTS")??"all").Split(',');bool Do(string p)=>parts.Contains("all")||parts.Contains(p);
  var scenes=(Environment.GetEnvironmentVariable("AUTHOR_SCENES")??"MountainLoopReverse").Split(',');bool dry=Environment.GetEnvironmentVariable("AUTHOR_DRY")=="1";
  try{
   foreach(var scene in scenes){
    EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();Note("SCENE "+scene+(dry?" (dry run, not saved)":""));
    if(Do("b008"))Skirt("Ground_Report070 barrier summit crest outcrop (0.70 BUG-005/006)","Ground_Report080 BUG-008 outcrop skirt",dry);
    if(Do("b008fill"))FillPit("BUG-008 trench fill",new Vector3(1004,0,118),new Vector3(1060,0,174),new Vector3(1028,0,141),16,dry);
    if(Do("b009"))SeamCover("Downhill Ridge Cut",0,24,dry);
    if(Do("b010"))Embank("BUG-010 deck",new Vector3(965,0,140),new Vector3(1015,0,335),dry);
    if(Do("b010all"))Embank("mountain decks",new Vector3(-5000,0,-5000),new Vector3(5000,0,5000),dry);
    if(!dry)Save();}
  }finally{File.AppendAllLines("Docs/Report080/author-notes.txt",notes);}
  EditorApplication.Exit(0);}

 // ---------- BUG-008: the crest outcrop is an open shell; where its lower rim hangs above the ground there is a cavity
 // under it that a vehicle can slide into. Close every such rim edge with a vertical rock face from the rim down to
 // 0.6 m below the ground beneath it (visible and collidable, both faces). Never over a driving surface or inside a
 // route corridor (those edges are listed and left).
 static void Skirt(string source,string name,bool dry){
  var mf=Object.FindObjectsByType<MeshFilter>().FirstOrDefault(m=>m.name==source);if(!mf){Note($"BUG-008: '{source}' not in {Scene}");return;}
  var col=mf.GetComponent<MeshCollider>();var mesh=mf.sharedMesh;var w=mesh.vertices.Select(v=>mf.transform.TransformPoint(v)).ToArray();var tris=mesh.triangles;
  // weld by position (1 cm), then boundary edges (used by one triangle) with the triangle's third vertex
  var id=new Dictionary<(int,int,int),int>();int[] wid=new int[w.Length];for(int i=0;i<w.Length;i++){var k=(Mathf.RoundToInt(w[i].x*100),Mathf.RoundToInt(w[i].y*100),Mathf.RoundToInt(w[i].z*100));if(!id.TryGetValue(k,out var v)){v=id.Count;id[k]=v;}wid[i]=v;}
  var pos=new Vector3[id.Count];for(int i=0;i<w.Length;i++)pos[wid[i]]=w[i];
  var edges=new Dictionary<(int,int),(int count,int third)>();
  for(int t=0;t<tris.Length;t+=3)for(int e=0;e<3;e++){int a=wid[tris[t+e]],b=wid[tris[t+(e+1)%3]],c=wid[tris[t+(e+2)%3]];if(a==b)continue;var key=a<b?(a,b):(b,a);edges[key]=edges.TryGetValue(key,out var x)?(x.count+1,x.third):(1,c);}
  var routes=Routes();var v3=new List<Vector3>();var cl=new List<Color>();var tr=new List<int>();var rock=new Color(.34f,.36f,.29f);
  int open=0,closed=0,left=0;float worst=0,area=0;var skipped=new List<string>();
  foreach(var kv in edges){if(kv.Value.count!=1)continue;var a=pos[kv.Key.Item1];var b=pos[kv.Key.Item2];
   if(!Below(a,col,out var ha)||!Below(b,col,out var hb))continue;float ga=a.y-ha.point.y,gb=b.y-hb.point.y;if(Mathf.Max(ga,gb)<.15f)continue;open++;worst=Mathf.Max(worst,Mathf.Max(ga,gb));
   var mid=(a+b)*.5f;string why=IsDrive(ha.collider)||IsDrive(hb.collider)?"over a driving surface":Corridor(routes,new Vector3(mid.x,Mathf.Min(ha.point.y,hb.point.y),mid.z),1)!=null?"in the "+Corridor(routes,new Vector3(mid.x,Mathf.Min(ha.point.y,hb.point.y),mid.z),1)+" corridor":null;
   if(why!=null){left++;skipped.Add($"({mid.x:F1}, {mid.y:F1}, {mid.z:F1}) gap {Mathf.Max(ga,gb):F2} m: {why}");continue;}
   var a2=new Vector3(a.x,ha.point.y-.6f,a.z);var b2=new Vector3(b.x,hb.point.y-.6f,b.z);
   // outward: away from the triangle's third vertex
   var third=pos[kv.Value.third];var along=b-a;along.y=0;var outv=Vector3.Cross(Vector3.up,along).normalized;if(Vector3.Dot(outv,new Vector3(third.x-mid.x,0,third.z-mid.z))>0)outv=-outv;
   void Quad(Vector3 p0,Vector3 p1,Vector3 p2,Vector3 p3,Vector3 face){int k=v3.Count;v3.AddRange(new[]{p0,p1,p2,p3});cl.AddRange(new[]{rock,rock,rock,rock});var n=Vector3.Cross(p1-p0,p2-p0);if(Vector3.Dot(n,face)>=0)tr.AddRange(new[]{k,k+1,k+2,k,k+2,k+3});else tr.AddRange(new[]{k,k+2,k+1,k,k+3,k+2});}
   Quad(a,b,b2,a2,outv);Quad(a,b,b2,a2,-outv);closed++;area+=along.magnitude*(Mathf.Max(ga,gb)+.6f);}
  Note($"BUG-008 {Scene}: outcrop rim edges hanging above the ground (>0.15 m): {open}, worst {worst:F2} m; closed {closed} ({area:F0} m² of rock face); left {left}"+(skipped.Count>0?": "+string.Join("; ",skipped.Take(20)):""));
  if(dry||closed==0)return;
  var m=new Mesh{indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};m.SetVertices(v3);m.SetColors(cl);m.SetTriangles(tr,0);m.RecalculateNormals();m.RecalculateBounds();
  var g=Make(name,Store("BUG-008 outcrop skirt",m),mf.GetComponent<MeshRenderer>().sharedMaterial);
  Note($"BUG-008 {Scene}: built '{g.name}' ({v3.Count} vertices) with a mesh collider, outcrop material");}
}
