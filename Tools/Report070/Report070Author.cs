using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
// 0.70 local repairs for Dan's debug session 2026-10-02_20-28-25-546_11ee21 (PROJECT_TODO "CURRENT" Parts A-F).
// Every change is local and logged to Docs/Report070/author-notes.txt. No route, checkpoint, physics or AI data changes.
// Section 5A: jump ramps/lips/flights/landings, multi-level routes, tunnels, junctions and the 0.68 berms are never edited.
public static class Report070Author {
 const string Folder="Assets/Track/Report070";
 static readonly List<string> notes=new();
 static void Note(string s){notes.Add(s);Debug.Log("REPORT070 "+s);}
 static string Scene=>UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
 static Color Earth=new(.39f,.48f,.29f),Rock=new(.34f,.36f,.29f);
 static bool IsDrive(Collider c)=>c&&c.name.Contains("driving surface");
 static bool IsTerrain(Collider c)=>c is MeshCollider&&!c.isTrigger&&!c.attachedRigidbody&&c.name.StartsWith("Ground");
 // Natural barriers, rock formations and structural roofs: sampled as ground but never lowered or built over.
 static bool IsBarrier(Collider c)=>c&&(c.name.Contains("natural edge")||c.name.Contains("outcrop")||c.name.Contains("boulder")||c.name.Contains("vault")||c.name.Contains("barrier")||c.name.Contains("portal")||c.name.Contains("rock"));
 static bool IsRoof(Collider c)=>c&&(c.name.Contains("mountain cap")||c.name.Contains("tunnel ceiling")||c.name.Contains("South Face hillside"));
 static bool Solid(Collider c)=>c&&!c.isTrigger&&!c.attachedRigidbody;
 static bool Surface(float x,float z,float from,Func<Collider,bool> ok,out RaycastHit hit,float depth=260){var o=new Vector3(x,from,z);hit=default;
  for(int k=0;k<60&&depth>0;k++){if(!Physics.Raycast(o,Vector3.down,out var h,depth,~0,QueryTriggerInteraction.Ignore))return false;if(h.normal.y>0&&ok(h.collider)){hit=h;return true;}depth-=h.distance+.02f;o=h.point+Vector3.down*.02f;}return false;}
 static bool Pave(Vector3 q,float near,out float y){y=0;if(!Surface(q.x,q.z,near+.6f,IsDrive,out var h,1.2f))return false;y=h.point.y;return true;}
 // Ground beside the road: first solid upward non-pavement surface below 'from' (excluding this round's new meshes).
 static bool Ground(Vector3 q,float from,float depth,out RaycastHit hit)=>Surface(q.x,q.z,from,c=>IsTerrain(c)&&!IsDrive(c)&&!c.name.StartsWith("Ground_Report070"),out hit,depth);
 static Mesh Store(string name,Mesh mesh){Directory.CreateDirectory(Folder);string path=$"{Folder}/{Scene}-{name}.asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(old){EditorUtility.CopySerialized(mesh,old);Object.DestroyImmediate(mesh);return old;}AssetDatabase.CreateAsset(mesh,path);return mesh;}
 static Mesh Build(List<Vector3> v,List<Color> c,List<int> t){var m=new Mesh{indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};m.SetVertices(v);m.SetColors(c);m.SetTriangles(t,0);m.RecalculateNormals();m.RecalculateBounds();return m;}
 static void Tri(List<Vector3> v,List<Color> c,List<int> t,Vector3 a,Vector3 b,Vector3 d,Vector3 want){var n=Vector3.Cross(b-a,d-a);if(n.sqrMagnitude<1e-8f)return;if(Vector3.Dot(n,want)<0){var x=b;b=d;d=x;n=-n;}var col=n.normalized.y>.72f?Earth:Rock;int k=v.Count;v.AddRange(new[]{a,b,d});c.AddRange(new[]{col,col,col});t.AddRange(new[]{k,k+1,k+2});}
 static GameObject Make(string name,Mesh mesh,Material mat){foreach(var old in Object.FindObjectsByType<MeshFilter>().Where(x=>x.name=="Ground_Report070 "+name).ToArray())Object.DestroyImmediate(old.gameObject);
  var g=new GameObject("Ground_Report070 "+name,typeof(MeshFilter),typeof(MeshRenderer),typeof(MeshCollider));g.GetComponent<MeshFilter>().sharedMesh=mesh;g.GetComponent<MeshRenderer>().sharedMaterial=mat;g.GetComponent<MeshCollider>().sharedMesh=mesh;GameObjectUtility.SetStaticEditorFlags(g,StaticEditorFlags.BatchingStatic);Physics.SyncTransforms();return g;}
 static Material GroundMaterial(){var r=Object.FindObjectsByType<MeshRenderer>().FirstOrDefault(x=>x.name.Contains("continuous solid 0"))??Object.FindObjectsByType<MeshRenderer>().First(x=>x.name.StartsWith("Ground_Mountain"));return r.sharedMaterial;}
 static void Save(){var s=UnityEngine.SceneManagement.SceneManager.GetActiveScene();EditorSceneManager.MarkSceneDirty(s);EditorSceneManager.SaveScene(s);AssetDatabase.SaveAssets();}
 static void Refresh(MeshFilter mf){if(mf.TryGetComponent<MeshCollider>(out var mc)){mc.sharedMesh=null;mc.sharedMesh=mf.sharedMesh;}EditorUtility.SetDirty(mf.sharedMesh);Physics.SyncTransforms();}
 static RaceDirector Race=>Object.FindAnyObjectByType<RaceDirector>(FindObjectsInactive.Include);
 static string Path(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n.Replace('\n','/');}

 // ---------- routes, a spatial index of route samples, and the protection rule ----------
 sealed class R{public string n;public Func<float,Vector3> at;public Func<float,Vector3> fw;public float len;public Func<float,float> hw;public ReverseShortcutGuidance guide;}
 static List<R> routes;static Dictionary<(int,int),List<(int ri,float s,Vector3 p)>> index;
 static void LoadRoutes(){var road=Race.road;road.Initialize();routes=new List<R>{new(){n="Main",at=x=>road.At(x,out _),fw=x=>{road.At(x,out var f);return f;},len=road.Length,hw=x=>road.HalfWidth(x)}};
  foreach(var b in Object.FindObjectsByType<WoodlandRoute>().Where(b=>b.gameObject.activeInHierarchy)){b.Initialize();var bb=b;routes.Add(new(){n=b.title,at=x=>bb.At(x,out _),fw=x=>{bb.At(x,out var f);return f;},len=b.Length,hw=x=>bb.halfWidth,guide=b.GetComponent<ReverseShortcutGuidance>()});}
  index=new();for(int i=0;i<routes.Count;i++)for(float s=0;s<=routes[i].len;s+=1){var p=routes[i].at(s);var k=((int)Mathf.Floor(p.x/10),(int)Mathf.Floor(p.z/10));if(!index.TryGetValue(k,out var l))index[k]=l=new();l.Add((i,s,p));}
  edgeCache=new();}
 // Nearest route sample (horizontal) with |dy| < maxDy; returns route index or -1.
 static int Nearest(Vector3 p,float maxDy,out float s,out float d){int best=-1;s=0;d=float.MaxValue;int cx=(int)Mathf.Floor(p.x/10),cz=(int)Mathf.Floor(p.z/10);
  for(int i=-1;i<=1;i++)for(int j=-1;j<=1;j++)if(index.TryGetValue((cx+i,cz+j),out var l))foreach(var (ri,ss,q) in l){if(Mathf.Abs(q.y-p.y)>maxDy)continue;float dd=new Vector2(q.x-p.x,q.z-p.z).magnitude;if(dd<d){d=dd;best=ri;s=ss;}}
  if(best>=0){var r=routes[best];for(float t=Mathf.Max(0,s-1);t<=Mathf.Min(r.len,s+1);t+=.1f){var q=r.at(t);float dd=new Vector2(q.x-p.x,q.z-p.z).magnitude;if(dd<d){d=dd;s=t;}}}return best;}
 // Is p inside another route's corridor (another route, or a part of the same route more than 40 m away)?
 static string OtherCorridor(Vector3 p,int self,float selfS,float pad=1.5f){int cx=(int)Mathf.Floor(p.x/10),cz=(int)Mathf.Floor(p.z/10);
  for(int i=-1;i<=1;i++)for(int j=-1;j<=1;j++)if(index.TryGetValue((cx+i,cz+j),out var l))foreach(var (ri,ss,q) in l){if(ri==self&&Mathf.Abs(ss-selfS)<40)continue;float dy=p.y-q.y;if(dy<-1.5f||dy>6)continue;if(new Vector2(q.x-p.x,q.z-p.z).magnitude<routes[ri].hw(ss)+pad)return routes[ri].n;}return null;}
 static Dictionary<(int,int,int),(float w,float py,Vector3 e)> edgeCache;
 // Pavement edge: walked out from the centre line in 0.05 m steps (NaN where the centre is unpaved).
 static float EdgeW(int ri,float s,int side,out float py,out Vector3 e){var key=(ri,side,Mathf.RoundToInt(s*2));if(edgeCache.TryGetValue(key,out var c)){py=c.py;e=c.e;return c.w;}
  var r=routes[ri];var ctr=r.at(s);var right=Right(r,s)*side;e=ctr;float w=float.NaN;py=0;
  if(Surface(ctr.x,ctr.z,ctr.y+2f,IsDrive,out var ch,4f)){py=ch.point.y;w=0;for(;w<16;w+=.05f){if(!Pave(ctr+right*(w+.05f),py,out float ny))break;py=ny;}e=ctr+right*w;e.y=py;}
  edgeCache[key]=(w,py,e);return w;}
 static Vector3 Right(R r,float s)=>Vector3.Cross(Vector3.up,Vector3.ProjectOnPlane(r.fw(s),Vector3.up).normalized);
 // Protected: jump/flight systems (game exclusion zones, branch flight windows, whole flight systems from the approach,
 // activity jumps), multi-level
 // routes above/below, covered roads (tunnel/cut roof overhead), and the 0.68 berms.
 static string Protected(int ri,float s,Vector3 e,Vector3 outv){var r=routes[ri];
  foreach(var z in exclusions)if(z.Contains(e))return "J";
  if(r.guide&&r.guide.takeoff>=0&&s>=r.guide.takeoff-15&&s<=r.guide.landing+10)return "J";
  // Whole flight systems, approach included (5A.5): approach station -40 m to the end of the landing.
  if(r.n=="Main"&&flights!=null)foreach(var f in flights.flights)if(s>=f.approachStation-40&&s<=f.endStation)return "F";
  // Activity jumps (Fern Creek Leap, High Ridge Drop, ...): 40 m either side of the site along any route within 25 m.
  foreach(var a in jumps){var ap=a.transform.position;var q0=r.at(s);if(new Vector2(ap.x-q0.x,ap.z-q0.z).magnitude<40&&Mathf.Abs(ap.y-q0.y)<12)return "J";}
  var probe=e+outv*1.5f;int cx=(int)Mathf.Floor(probe.x/10),cz=(int)Mathf.Floor(probe.z/10);
  for(int i=-1;i<=1;i++)for(int j=-1;j<=1;j++)if(index.TryGetValue((cx+i,cz+j),out var l))foreach(var (oi,os,q) in l){if(oi==ri&&Mathf.Abs(os-s)<40)continue;float dy=Mathf.Abs(probe.y-q.y);if(dy>1.5f&&dy<40&&new Vector2(q.x-probe.x,q.z-probe.z).magnitude<routes[oi].hw(os)+3)return "M";}
  if(Physics.Raycast(e+Vector3.up*.3f,Vector3.up,out var up,10,~0,QueryTriggerInteraction.Ignore)&&Solid(up.collider))return "T";
  for(float d=.3f;d<=1.5f;d+=.4f)if(Surface(e.x+outv.x*d,e.z+outv.z*d,e.y+2.5f,Solid,out var bh,5)&&bh.collider.name.Contains("Report068 barrier"))return "B";
  return null;}
 static JumpRecoveryExclusion[] exclusions;static MountainFlights flights;static ActivitySite[] jumps;


 public static void Run(){
  if(Application.isPlaying)throw new Exception("Edit mode required");Directory.CreateDirectory("Docs/Report070");Physics.queriesHitBackfaces=false;
  var parts=(Environment.GetEnvironmentVariable("AUTHOR_PARTS")??"all").Split(',');bool Do(string p)=>parts.Contains("all")||parts.Contains(p);
  var scenes=(Environment.GetEnvironmentVariable("AUTHOR_SCENES")??"MountainLoopReverse").Split(',');bool dry=Environment.GetEnvironmentVariable("AUTHOR_DRY")=="1";
  try{
   foreach(var scene in scenes){
    EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();Note("SCENE "+scene+(dry?" (dry run, not saved)":""));
    if(Do("landmark"))Landmark();
    if(scene.StartsWith("StreetLoop")||scene=="LakeWoods"||scene=="ForestLoopReverse"||scene.StartsWith("DansBackyard")||scene.StartsWith("Mountain")){
     if(Do("b003"))SmoothBand("BUG-003 trench wall",new[]{new Vector3(221,0,456),new Vector3(225,0,462),new Vector3(258,0,496),new Vector3(290,0,530),new Vector3(306,0,532)},12f,null,dry);
     if(Do("b004"))SmoothBand("BUG-004 highway verge",new[]{new Vector3(205,0,568f),new Vector3(282,0,566.5f)},6f,c=>c.name.Contains("CR113")&&c.name.Contains("seam"),dry);}
    if(scene.StartsWith("Mountain")&&Do("camp"))Camp();
    if(scene.StartsWith("Mountain")){LoadRoutes();exclusions=Object.FindObjectsByType<JumpRecoveryExclusion>(FindObjectsInactive.Include);flights=Object.FindAnyObjectByType<MountainFlights>(FindObjectsInactive.Include);jumps=Object.FindObjectsByType<ActivitySite>(FindObjectsInactive.Include).Where(a=>a.kind==ActivitySite.Kind.Jump).ToArray();
     if(scene=="MountainLoopReverse"&&Do("crestplan"))CrestPlan();
     if(scene=="MountainLoopReverse"&&Do("crest"))Crest();
     // BUG-009: the two gores at the Climbing Ridge Cut rejoin (right: CRC end / main road north; left: CRC / main road
     // south-west) get the 0.69 junction surface; terrain under it goes 0.4 m below it (no hidden faces, Part E).
     if(scene=="MountainLoop"&&Do("b009")){Patch("BUG-009 right gore",new Vector3(1011f,139.6f,-49.5f),7.5f,.4f);Patch("BUG-009 left gore",new Vector3(1006.5f,137.6f,-63f),9.5f,.4f);}
     if(scene=="MountainLoop"&&Do("b008unsub"))UndoBankSubdivide();
     if(scene=="MountainLoop"&&Do("b008fix"))HiddenFacesVisibleFix(new Vector3(740,0,-126),new Vector3(776,0,-104));
     if(scene=="MountainLoop"&&Do("b008"))HiddenFaces("BUG-008 Climbing Ridge Cut entry",new Vector3(740,0,-126),new Vector3(776,0,-104));}
    if(!dry)Save();}
  }finally{File.AppendAllLines("Docs/Report070/author-notes.txt",notes);}
  EditorApplication.Exit(0);}

 // ---------- Part C: crest outcrop (plan: measured pavement edges of the main road and the Summit Traverse entrance) ----------
 static void CrestPlan(){var rows=new List<string>{"route,s,side,W,ex,ey,ez,cx,cz"};int mi=routes.FindIndex(x=>x.n=="Main"),ti=routes.FindIndex(x=>x.n=="Summit Traverse");
  for(float s=430;s<=505;s+=1){float W=EdgeW(mi,s,1,out _,out var e);var c=routes[mi].at(s);rows.Add($"Main,{s},1,{W:F2},{e.x:F2},{e.y:F2},{e.z:F2},{c.x:F2},{c.z:F2}");}
  for(float s=0;s<=45;s+=1){float W=EdgeW(ti,s,-1,out _,out var e);var c=routes[ti].at(s);rows.Add($"Summit Traverse,{s},-1,{W:F2},{e.x:F2},{e.y:F2},{e.z:F2},{c.x:F2},{c.z:F2}");}
  File.WriteAllLines("Docs/Report070/crest-edges.csv",rows);Note("Part C plan: measured edges written to crest-edges.csv");}
 // ---------- Part F: smoothing (Street Loop world) ----------
 // Terrain vertices within 'band' m of the polyline are Laplacian-smoothed (Gauss-Seidel on the mesh graph; vertices at the
 // same x,z are one node, so vertical sawtooth faces collapse into a slope). Pinned: the band rim (original ground), nodes
 // within 4 m of buildings/props (unchanged), and - when 'road' is given - nodes on or within 1.2 m of that road surface,
 // pinned 3 cm under it (flush). Road surfaces are never edited. Trees standing in the band are re-grounded: the trunk
 // collider and that tree's pieces in the batched tree meshes move by the ground change under the trunk.
 static bool IsTerrainTile(MeshFilter mf)=>mf.name.StartsWith("Ground_")&&!mf.name.Contains("seam")&&!mf.name.Contains("CR113")&&!mf.name.Contains("driving")&&!mf.name.StartsWith("Ground_Report0")&&mf.TryGetComponent<MeshCollider>(out _);
 static bool TerrainTileCollider(Collider x)=>x is MeshCollider m2&&m2.GetComponent<MeshFilter>()&&IsTerrainTile(m2.GetComponent<MeshFilter>());
 static float DistPoly(Vector3[] poly,Vector3 p){float best=1e9f;for(int i=0;i+1<poly.Length;i++){var a=new Vector2(poly[i].x,poly[i].z);var b=new Vector2(poly[i+1].x,poly[i+1].z);var q=new Vector2(p.x,p.z);var ab=b-a;float t=Mathf.Clamp01(Vector2.Dot(q-a,ab)/ab.sqrMagnitude);best=Mathf.Min(best,(a+ab*t-q).magnitude);}return best;}
 static bool InBox(Bounds b,Vector3 lo,Vector3 hi)=>b.max.x>lo.x&&b.min.x<hi.x&&b.max.z>lo.z&&b.min.z<hi.z;
 static void SmoothBand(string label,Vector3[] poly,float band,Func<Collider,bool> road,bool dry){
  var lo=new Vector3(poly.Min(p=>p.x)-band-2,0,poly.Min(p=>p.z)-band-2);var hi=new Vector3(poly.Max(p=>p.x)+band+2,0,poly.Max(p=>p.z)+band+2);
  var mfs=Object.FindObjectsByType<MeshFilter>().Where(m=>m.sharedMesh&&IsTerrainTile(m)&&m.GetComponent<Renderer>()&&InBox(m.GetComponent<Renderer>().bounds,lo,hi)).ToArray();
  if(mfs.Length==0){Note($"Part F {label}: no terrain here in {Scene}");return;}
  (int,int) K(Vector3 p)=>(Mathf.RoundToInt(p.x*50),Mathf.RoundToInt(p.z*50));
  var y=new Dictionary<(int,int),float>();var pos=new Dictionary<(int,int),Vector3>();var nb=new Dictionary<(int,int),HashSet<(int,int)>>();var inBand=new HashSet<(int,int)>();
  var world=new Dictionary<MeshFilter,Vector3[]>();
  foreach(var mf in mfs){var w=mf.sharedMesh.vertices.Select(v=>mf.transform.TransformPoint(v)).ToArray();world[mf]=w;var t=mf.sharedMesh.triangles;
   for(int i=0;i<w.Length;i++){var k=K(w[i]);if(!pos.ContainsKey(k)){pos[k]=w[i];y[k]=w[i].y;}else y[k]=Mathf.Max(y[k],w[i].y);if(DistPoly(poly,w[i])<=band)inBand.Add(k);}
   for(int i=0;i<t.Length;i+=3)for(int e=0;e<3;e++){var a=K(w[t[i+e]]);var b=K(w[t[i+(e+1)%3]]);if(a.Equals(b))continue;if(!nb.TryGetValue(a,out var na))nb[a]=na=new();na.Add(b);if(!nb.TryGetValue(b,out var nbb))nb[b]=nbb=new();nbb.Add(a);}}
  var pinned=new Dictionary<(int,int),float>();int pinRoad=0,pinObj=0;
  var objs=Object.FindObjectsByType<Collider>().Where(c=>!c.isTrigger&&!c.attachedRigidbody&&!TerrainTileCollider(c)&&(road==null||!road(c))&&c.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)<0&&!c.name.StartsWith("Ground_")&&InBox(c.bounds,lo,hi)).ToArray();
  foreach(var k in inBand){var p=pos[k];bool rim=!nb.TryGetValue(k,out var n)||n.Any(o=>!inBand.Contains(o));if(rim){pinned[k]=y[k];continue;}
   if(objs.Any(c=>{var b=c.bounds;b.Expand(new Vector3(8,80,8));return b.Contains(new Vector3(p.x,c.bounds.center.y,p.z));})){pinned[k]=y[k];pinObj++;continue;}
   if(road!=null){if(Surface(p.x,p.z,p.y+3,road,out var on,6)){pinned[k]=on.point.y-.03f;pinRoad++;continue;}
    float ry=float.NaN;for(int a=0;a<8&&float.IsNaN(ry);a++){var d=Quaternion.Euler(0,a*45,0)*Vector3.forward*1.2f;if(Surface(p.x+d.x,p.z+d.z,p.y+3,road,out var rh,6))ry=rh.point.y;}
    if(!float.IsNaN(ry)){pinned[k]=ry-.03f;pinRoad++;continue;}}}
  var ny=new Dictionary<(int,int),float>(y);foreach(var kv in pinned)ny[kv.Key]=kv.Value;var free=inBand.Where(k=>!pinned.ContainsKey(k)).ToList();
  int iters=0;for(int it=0;it<20000;it++){iters=it;float md=0;foreach(var k in free){float sum=0;int c=0;foreach(var o in nb[k]){sum+=ny[o];c++;}if(c==0)continue;float v=sum/c;md=Mathf.Max(md,Mathf.Abs(v-ny[k]));ny[k]=v;}if(md<1e-6f&&it>500)break;}
  float maxUp=0,maxDown=0;foreach(var k in inBand){float d=ny[k]-y[k];maxUp=Mathf.Max(maxUp,d);maxDown=Mathf.Min(maxDown,d);}
  var trunks=Object.FindObjectsByType<Collider>().Where(c=>!c.isTrigger&&c.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0&&c.bounds.size.x<3&&DistPoly(poly,c.bounds.center)<=band+1).ToArray();
  var oldG=trunks.ToDictionary(c=>c,c=>Surface(c.bounds.center.x,c.bounds.center.z,c.bounds.max.y+5,TerrainTileCollider,out var th,60)?th.point.y:float.NaN);
  Note($"Part F {Scene} {label}: {mfs.Length} terrain meshes, {inBand.Count} nodes in the {band} m band ({pinned.Count} pinned: objects {pinObj}, flush to the road {pinRoad}); {iters} iterations; ground change {maxDown:+0.00;-0.00} .. {maxUp:+0.00;-0.00} m; trunks in the band {trunks.Length}{(dry?" (dry run)":"")}");
  if(dry)return;
  foreach(var mf in mfs){var w=world[mf];var lv=mf.sharedMesh.vertices;bool any=false;for(int i=0;i<w.Length;i++){var k=K(w[i]);if(!inBand.Contains(k))continue;float v=ny[k];if(Mathf.Abs(v-w[i].y)<1e-4f)continue;var q=w[i];q.y=v;lv[i]=mf.transform.InverseTransformPoint(q);any=true;}
   if(any){var m=Own(mf);m.vertices=lv;m.RecalculateNormals();m.RecalculateBounds();Refresh(mf);}}
  Physics.SyncTransforms();
  var batched=Object.FindObjectsByType<MeshFilter>().Where(m=>m.sharedMesh&&!m.GetComponent<Collider>()&&m.GetComponent<Renderer>()&&InBox(m.GetComponent<Renderer>().bounds,lo,hi)&&(m.name.IndexOf("tree",StringComparison.OrdinalIgnoreCase)>=0||m.name.Contains("canopy")||m.name.Contains("woodland")||m.name.Contains("foliage")||m.name.Contains("timber"))).ToArray();
  var comps=new List<(MeshFilter mf,int[] verts,Vector3 ctr,float minY)>();
  foreach(var mf in batched){var m=mf.sharedMesh;var t=m.triangles;var par=Enumerable.Range(0,m.vertexCount).ToArray();int F(int a){while(par[a]!=a){par[a]=par[par[a]];a=par[a];}return a;}
   for(int i=0;i<t.Length;i+=3){int a=F(t[i]),b=F(t[i+1]);par[b]=a;int c=F(t[i+2]);par[c]=F(a);}
   var w=m.vertices.Select(v=>mf.transform.TransformPoint(v)).ToArray();var used=new HashSet<int>(t);
   foreach(var g in used.GroupBy(F)){var vs=g.ToArray();var c=Vector3.zero;float mn=1e9f;foreach(int v in vs){c+=w[v];mn=Mathf.Min(mn,w[v].y);}comps.Add((mf,vs,c/vs.Length,mn));}}
  int movedTrees=0,movedPieces=0;var shift=new Dictionary<MeshFilter,Dictionary<int,float>>();
  foreach(var tr in trunks){if(float.IsNaN(oldG[tr]))continue;var b=tr.bounds;if(!Surface(b.center.x,b.center.z,b.max.y+5,TerrainTileCollider,out var nh,60))continue;
   float dy=nh.point.y-oldG[tr];if(Mathf.Abs(dy)<.05f)continue;tr.transform.position+=Vector3.up*dy;movedTrees++;int pieces=0;
   foreach(var cp in comps){if(new Vector2(cp.ctr.x-b.center.x,cp.ctr.z-b.center.z).magnitude>2.2f||cp.minY<oldG[tr]-1.5f||cp.minY>oldG[tr]+12)continue;if(!shift.TryGetValue(cp.mf,out var sm))shift[cp.mf]=sm=new();foreach(int v in cp.verts)sm[v]=dy;movedPieces++;pieces++;}
   Note($"Part F {label}: tree at {b.center:F1} re-grounded {dy:+0.00;-0.00} m (trunk collider '{Path(tr.transform)}' and {pieces} batched pieces)");}
  foreach(var kv in shift){var mf=kv.Key;var m=Own(mf);var lv=m.vertices;foreach(var e in kv.Value){var q=mf.transform.TransformPoint(lv[e.Key]);q.y+=e.Value;lv[e.Key]=mf.transform.InverseTransformPoint(q);}m.vertices=lv;m.RecalculateBounds();EditorUtility.SetDirty(m);}
  Note($"Part F {label}: trees re-grounded {movedTrees} ({movedPieces} batched pieces moved with them)");}

 // ---------- Part E: hidden faces just under the pavement (BUG-008) ----------
 // Entry runs (Report070Checks 'enter') hit 'Ground_Report068 seam support' at branch s 9-11 with every speed >= 14 m/s:
 // the 0.68 collidable seam cover runs 5-11 cm under the pavement there with faces tilted up to 39 deg, some facing
 // against the direction of travel, so a compressed body meets a hidden ramp. The cover is a fan of slivers spanning
 // the whole entry; only its left ends close a real slot. Its vertices under the pavement go to 0.6 m below it, the
 // uncovered ends stay, so the fan slopes away under the driving line and the slot stays closed.

 // Top driving-adjacent surface 0-0.3 m above q and 0.4 m around it: pavement or the 0.69 patch (what a vehicle rides on
 // here). Points within 0.4 m of an edge of that cover are never changed, so edges stay flush and slots stay closed.
 static bool CoveredTop(Vector3 q,out float py){py=0;bool Top(Vector3 r,out float y){y=0;if(Surface(r.x,r.z,q.y+.3f,c=>IsDrive(c)||c.name.StartsWith("Ground_Report069 patch"),out var h,.31f)&&h.point.y>=q.y-.001f){y=h.point.y;return true;}return false;}
  if(!Top(q,out py))return false;foreach(var d in new[]{Vector3.right,Vector3.left,Vector3.forward,Vector3.back})if(!Top(q+d*.4f,out _))return false;return true;}
 static void HiddenFaces(string label,Vector3 lo,Vector3 hi){int lowered=0,split=0;var log=new List<string>();
  foreach(var mf in Object.FindObjectsByType<MeshFilter>().Where(x=>x.name=="Ground_Report068 seam support"&&x.sharedMesh).ToArray()){var m=mf.sharedMesh;
   var lv=m.vertices.ToList();var cols=m.colors.ToList();bool hasCol=cols.Count==lv.Count;var tris=m.triangles;var keep=new List<int>();var w=lv.Select(x=>mf.transform.TransformPoint(x)).ToArray();
   bool Covered(Vector3 q,out float py)=>CoveredTop(q,out py);
   const int N=8;
   for(int t=0;t<tris.Length;t+=3){int ia=tris[t],ib=tris[t+1],ic=tris[t+2];var a=w[ia];var b=w[ib];var c=w[ic];var ctr=(a+b+c)/3;
    bool any=false;for(int u=0;u<=4&&!any;u++)for(int v=0;v<=4-u&&!any;v++)if(Covered(a+(b-a)*(u/4f)+(c-a)*(v/4f),out _))any=true;
    if(ctr.x<lo.x||ctr.x>hi.x||ctr.z<lo.z||ctr.z>hi.z||!any){keep.AddRange(new[]{ia,ib,ic});continue;}
    split++;var idx=new int[N+1,N+1];
    for(int u=0;u<=N;u++)for(int v=0;v<=N-u;v++){float fu=u/(float)N,fv=v/(float)N;var p=a+(b-a)*fu+(c-a)*fv;if(Covered(p,out float py)){p.y=py-.6f;lowered++;}
     idx[u,v]=lv.Count;lv.Add(mf.transform.InverseTransformPoint(p));if(hasCol)cols.Add(Color.Lerp(Color.Lerp(cols[ia],cols[ib],fu),cols[ic],fv));}
    for(int u=0;u<N;u++)for(int v=0;v<N-u;v++){keep.AddRange(new[]{idx[u,v],idx[u+1,v],idx[u,v+1]});if(v<N-u-1)keep.AddRange(new[]{idx[u+1,v],idx[u+1,v+1],idx[u,v+1]});}
    log.Add($"tri {t/3} {a:F2} {b:F2} {c:F2}: subdivided {N}x{N}; points under the pavement lowered to 0.6 m below it");}
   if(split>0){m=Own(mf);m.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;m.SetVertices(lv);if(hasCol)m.SetColors(cols);m.SetTriangles(keep,0);m.RecalculateNormals();m.RecalculateBounds();Refresh(mf);}}
  // The terrain layers just under the pavement and the 0.69 patch (earth banks, older shoulders): vertices 0-0.3 m under
  // the top surface go to 0.6 m below it (no subdivision needed: these meshes are dense here).
  int terr=0;foreach(var mf in Object.FindObjectsByType<MeshFilter>().Where(x=>x.sharedMesh&&(x.name=="Ground_CR133 mountain earth banks"||x.name=="Ground_MountainPolish supported shoulders")).ToArray()){var m=mf.sharedMesh;var lv=m.vertices;bool any=false;
   for(int i=0;i<lv.Length;i++){var p=mf.transform.TransformPoint(lv[i]);if(p.x<lo.x||p.x>hi.x||p.z<lo.z||p.z>hi.z)continue;if(!CoveredTop(p,out float py))continue;p.y=py-.6f;lv[i]=mf.transform.InverseTransformPoint(p);terr++;any=true;}
   if(any){m=Own(mf);m.vertices=lv;m.RecalculateNormals();m.RecalculateBounds();Refresh(mf);log.Add($"{mf.name}: vertices under the top surface lowered");}}
  Note($"Part E {label}: earth-bank / older-shoulder vertices lying 0-0.3 m under the pavement or the 0.69 patch lowered to 0.6 m below it: {terr}");
  File.WriteAllLines("Docs/Report070/partE-hidden-faces.txt",log);
  Note($"Part E {label}: seam-support triangles reaching under the pavement {split}, each subdivided 8x8; {lowered} points that lie under the pavement lowered to 0.6 m below it; points outside the pavement (the left-edge slot cover) unchanged (partE-hidden-faces.txt)");}

 // Correction to the earth-bank step above: a lowered vertex whose triangles reach beyond the pavement / patch showed as a
 // visible shelf and notch beside the right edge. Inside the box, every earth-bank / shoulder vertex that the step lowered is
 // put back to its committed height (temporary copies Assets/Track/Report070/tmp-orig-*.asset) unless every triangle
 // touching it lies fully under the pavement or the 0.69 patch (5x5 samples per triangle, original heights, 0-0.3 m below).
 static void HiddenFacesVisibleFix(Vector3 lo,Vector3 hi){int restored=0,kept=0;
  foreach(var mf in Object.FindObjectsByType<MeshFilter>().Where(x=>x.sharedMesh&&(x.name=="Ground_CR133 mountain earth banks"||x.name=="Ground_MountainPolish supported shoulders")).ToArray()){
   var m=mf.sharedMesh;var id=System.IO.Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(m)).Replace("MountainLoop-solid-retained-2-","");
   var orig=AssetDatabase.LoadAssetAtPath<Mesh>($"Assets/Track/Report070/tmp-orig-{id}.asset");if(!orig||orig.vertexCount!=m.vertexCount){Note($"Part E fix: no matching original for {mf.name} ({id})");continue;}
   var lv=m.vertices;var ov=orig.vertices;var t=m.triangles;var tris=new Dictionary<int,List<int>>();for(int i=0;i<t.Length;i+=3)for(int k=0;k<3;k++){if(!tris.TryGetValue(t[i+k],out var l))tris[t[i+k]]=l=new();l.Add(i);}
   bool Cov(Vector3 q)=>Surface(q.x,q.z,q.y+.3f,c=>IsDrive(c)||c.name.StartsWith("Ground_Report069 patch"),out var h,.31f)&&h.point.y>=q.y-.001f;
   bool any=false;
   for(int i=0;i<lv.Length;i++){if((lv[i]-ov[i]).sqrMagnitude<1e-8f)continue;var w=mf.transform.TransformPoint(ov[i]);if(w.x<lo.x||w.x>hi.x||w.z<lo.z||w.z>hi.z)continue;
    bool full=true;if(tris.TryGetValue(i,out var adj))foreach(int ti in adj){var a=mf.transform.TransformPoint(ov[t[ti]]);var b=mf.transform.TransformPoint(ov[t[ti+1]]);var c=mf.transform.TransformPoint(ov[t[ti+2]]);
     for(int u=0;u<=4&&full;u++)for(int v=0;v<=4-u&&full;v++)if(!Cov(a+(b-a)*(u/4f)+(c-a)*(v/4f)))full=false;if(!full)break;}
    if(full){kept++;continue;}lv[i]=ov[i];restored++;any=true;}
   if(any){m.vertices=lv;m.RecalculateNormals();m.RecalculateBounds();Refresh(mf);}}
  Note($"Part E fix: earth-bank / shoulder vertices put back to their committed height (a triangle reached beyond the cover) {restored}; kept lowered (all their triangles fully under the pavement or patch) {kept}");}

 // A tried step (subdividing earth-bank triangles partly under the cover and lowering covered sub-points) exposed short
 // dark slopes where the earth bank interpenetrates the 0.69 patch, so it was undone exactly: the topology before it was
 // the committed one, so the committed triangle list is restored and the appended vertices dropped.
 static void UndoBankSubdivide(){foreach(var mf in Object.FindObjectsByType<MeshFilter>().Where(x=>x.sharedMesh&&(x.name=="Ground_CR133 mountain earth banks"||x.name=="Ground_MountainPolish supported shoulders")).ToArray()){
  var m=mf.sharedMesh;var id=System.IO.Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(m)).Replace("MountainLoop-solid-retained-2-","");var orig=AssetDatabase.LoadAssetAtPath<Mesh>($"Assets/Track/Report070/tmp-orig-{id}.asset");
  if(!orig||m.vertexCount<=orig.vertexCount){Note($"Part E undo: {mf.name} nothing to undo");continue;}int n=orig.vertexCount;var lv=m.vertices.Take(n).ToList();var cols=m.colors;
  m.SetTriangles(orig.triangles,0);m.SetVertices(lv);if(cols.Length>=n)m.SetColors(cols.Take(n).ToList());m.RecalculateNormals();m.RecalculateBounds();Refresh(mf);
  Note($"Part E undo: {mf.name} back to {n} vertices / {orig.triangles.Length/3} triangles (committed topology, post-correction heights)");}}

 // ---------- Part D: campsite ----------
 // BUG-007: in the Mountain scenes the CR-094 camp (no colliders) was placed for the Street Loop world's ground. Each
 // piece is seated on this scene's own ground (terrain only; roads/ramps above are ignored): the tent and each seated
 // figure with its log move as one unit, the campfire and each fire-ring stone individually; the base of each unit goes
 // to the ground under its own centre (5 cm sunk). Terrain is not changed. Street Loop / Free Roam world scenes untouched.
 static void Camp(){var root=Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).FirstOrDefault(t=>t.name=="Permanent mountainside camp / two seated guys");
  if(!root){Note("Part D: camp not found");return;}
  foreach(Transform u in root){var rs=u.GetComponentsInChildren<Renderer>(true);if(rs.Length==0)continue;var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);
   if(!Ground(new Vector3(b.center.x,0,b.center.z),b.max.y+3,80,out var gh)){Note($"Part D: no ground under '{u.name}' at {b.center:F1}; left");continue;}
   float lo=float.MaxValue,hi=float.MinValue;foreach(var (dx,dz) in new[]{(-1,-1),(-1,1),(1,-1),(1,1)}){if(Ground(new Vector3(b.center.x+dx*b.extents.x*.8f,0,b.center.z+dz*b.extents.z*.8f),b.max.y+3,80,out var ch)){lo=Mathf.Min(lo,ch.point.y);hi=Mathf.Max(hi,ch.point.y);}}
   float dy=gh.point.y-.05f-b.min.y;u.position+=Vector3.up*dy;
   Note($"Part D {Scene}: '{u.name}' base {b.min.y:F2} -> {b.min.y+dy:F2} (moved {dy:+0.00;-0.00} m) on '{gh.collider.name}'; ground under its footprint {lo:F2}..{hi:F2}");}
  EditorUtility.SetDirty(root.gameObject);}
 // Free Roam landmark "Campsite": an ordinary ExplorationMap destination (map label, 45 m discovery, free-roam fast travel)
 // beside the camp, facing it; added to every course scene's map so the shared world map stays identical.
 static readonly Vector3 CampsiteArrival=new(982f,164.2f,76f);
 static void Landmark(){var map=Object.FindAnyObjectByType<ExplorationMap>(FindObjectsInactive.Include);if(!map){Note("Part D: no ExplorationMap");return;}
  if(map.destinations.Any(d=>d.id=="campsite")){Note($"Part D {Scene}: Campsite landmark already present");return;}
  var list=map.destinations.ToList();list.Add(new ExplorationMap.Destination{id="campsite",title="Campsite",position=CampsiteArrival,yaw=90f});map.destinations=list.ToArray();EditorUtility.SetDirty(map);
  Note($"Part D {Scene}: added landmark 'Campsite' at {CampsiteArrival} yaw 90 ({map.destinations.Length} destinations)");}

 // ---------- Part C: rock outcrop at the Reverse summit crest (BUG-005/006) ----------
 // Measured (Docs/Report070/BARRIERS.md): full-throttle motorcycles/ATVs leave the crest at 31-35 m/s and cross the
 // outside of the bend (main s 448-462) 4-9.5 m above the road, over the road-level plateau (the gore between the main
 // road and the Summit Traverse entrance). One rock outcrop fills that gore: an 85 deg cliff face 7 m beyond the main
 // road's right pavement edge (just behind the OPTIONAL SHORTCUT / SUMMIT TRAVERSE sign, which stays in front of it) and
 // an 85 deg face 1 m beyond the Summit Traverse's left pavement edge; top 12 m above the pavement edge, 3 m crown, back
 // face 1:0.7 down to the ground. Each face's solid is clipped where it would enter the other road's pavement or the
 // runoff in front of the other face, so the two meet in one corner behind the sign. Far ends taper over 6 m; the corner
 // end is capped. The 0.69 berm here is removed (absorbed). Collidable, grounded.
 const float CrestTop=12f,MainFace=7f,StFace=1f,Crown=3f;
 static void Crest(){int mi=routes.FindIndex(x=>x.n=="Main"),ti=routes.FindIndex(x=>x.n=="Summit Traverse");
  var st=routes[ti];if(st.guide)Note($"Part C: Summit Traverse flight window takeoff {st.guide.takeoff} landing {st.guide.landing}");
  // Pavement edge polylines (measured, 0.5 m) used for the keep-out tests.
  List<(Vector3 e,Vector3 o)> Edge(int ri,int side,float s0,float s1){var l=new List<(Vector3,Vector3)>();for(float s=s0;s<=s1;s+=.5f){float W=EdgeW(ri,s,side,out _,out var e);if(float.IsNaN(W)||W>9.5f)continue;l.Add((e,Right(routes[ri],s)*side));}return l;}
  var mainEdge=Edge(mi,1,440,505);var stEdge=Edge(ti,-1,3,48);
  // Outward distance from an edge polyline (negative = on the pavement side); NaN where the point is beside neither end.
  float Out(List<(Vector3 e,Vector3 o)> edge,Vector3 p){float best=float.NaN,bd=1e9f;foreach(var (e,o) in edge){var d=new Vector2(p.x-e.x,p.z-e.z);float along=Mathf.Abs(d.x*o.z-d.y*o.x);if(along<bd&&along<.6f){bd=along;best=d.x*o.x+d.y*o.z;}}return best;}
  bool OnPave(Vector3 p){for(int i=0;i<5;i++){var q=p+new Vector3(i==1?1:i==2?-1:0,0,i==3?1:i==4?-1:0);if(Surface(q.x,q.z,p.y+20,IsDrive,out _,40))return true;}return false;}
  bool KeepOutMain(Vector3 p){float d=Out(stEdge,p);return OnPave(p)||(!float.IsNaN(d)&&d<StFace);}
  bool KeepOutSt(Vector3 p){float d=Out(mainEdge,p);return OnPave(p)||(!float.IsNaN(d)&&d<MainFace);}
  var removed=Object.FindObjectsByType<MeshFilter>().Where(x=>x.name=="Ground_Report069 barrier Summit crest bend (BUG-005)").ToArray();foreach(var r in removed){Note($"Part C: removed the 0.69 berm '{r.name}' (absorbed into the outcrop)");Object.DestroyImmediate(r.gameObject);}Physics.SyncTransforms();
  var v=new List<Vector3>();var c=new List<Color>();var t=new List<int>();var log=new List<string>{"SCENE "+Scene};int made=0;
  void Ridge(string name,int ri,int side,float s0,float s1,float face,Func<Vector3,bool> keepOut,bool capStart){var r=routes[ri];List<Vector3> prev=null;bool first=true;float lastS=s1;
   var stations=new List<(float s,List<Vector3> pts)>();
   for(float s=s0;s<=s1;s+=.5f){float W=EdgeW(ri,s,side,out float py,out var e);var outv=Right(r,s)*side;if(float.IsNaN(W)||W>9.5f){log.Add($"{name} s {s}: junction/unpaved");continue;}
    var f0=e+outv*face;if(keepOut(f0)){log.Add($"{name} s {s}: face point in the other road's keep-out; skipped");continue;}
    float taper=Mathf.Clamp01((s1-s)/6f);float top=py+Mathf.Max(.8f,CrestTop*taper);
    float g0=Ground(f0,top+2,60,out var gh)?gh.point.y:py-1;var p0=f0;p0.y=Mathf.Min(g0,py)-.3f;
    // Natural rock faceting: deterministic noise only pushes the face back (never toward the road) and varies the top.
    float n1=Mathf.PerlinNoise(s*.23f,side*7.1f),n2=Mathf.PerlinNoise(s*.41f+3,side*2.3f),n3=Mathf.PerlinNoise(s*.17f+9,side*5.7f);top+=(n3-.5f)*1.6f*taper;
    var p1=e+outv*(face+.6f*n2+(top-p0.y)/Mathf.Tan(85*Mathf.Deg2Rad));p1.y=top;var pm=e+outv*(face+.25f+.9f*n1+.5f*(top-p0.y)/Mathf.Tan(85*Mathf.Deg2Rad));pm.y=p0.y+(top-p0.y)*(.45f+.2f*n2);
    var p2=p1+outv*Crown;var pts=new List<Vector3>{p0,pm,p1};
    if(keepOut(p2)){var q=p1;q.y=p0.y;pts.Add(q);}else{pts.Add(p2);float y=top;var q=p2;
     for(int k=0;k<40;k++){var n=q+outv*.7f;n.y=y-1;float gq=Ground(n,top+2,80,out var g2)?g2.point.y:float.NegativeInfinity;
      if(keepOut(n)){var drop=q;drop.y=Ground(q,top+2,80,out var g3)?g3.point.y-.2f:q.y-12;pts.Add(drop);break;}
      if(!float.IsNegativeInfinity(gq)&&n.y<=gq){n.y=gq-.2f;pts.Add(n);break;}pts.Add(n);q=n;y=n.y;}}
    while(pts.Count<20)pts.Add(pts[^1]);if(pts.Count>20)pts.RemoveRange(20,pts.Count-20);stations.Add((s,pts));}
   for(int i=0;i<stations.Count;i++){var pts=stations[i].pts;var outv=Right(r,stations[i].s)*side;
    if(i>0&&stations[i].s-stations[i-1].s<=.51f){var a=stations[i-1].pts;for(int k=0;k+1<pts.Count;k++){Tri(v,c,t,a[k],a[k+1],pts[k+1],Vector3.up-outv*.01f);Tri(v,c,t,a[k],pts[k+1],pts[k],Vector3.up-outv*.01f);}}
    bool startOfRun=i==0||stations[i].s-stations[i-1].s>.51f;if(startOfRun&&capStart){var fwd=Vector3.ProjectOnPlane(r.fw(stations[i].s),Vector3.up).normalized;for(int k=1;k+1<pts.Count;k++)Tri(v,c,t,pts[0],pts[k],pts[k+1],-fwd);}
    made++;}
   if(stations.Count>0)log.Add($"{name}: stations {stations.Count} from s {stations[0].s} to {stations[^1].s}; face {face} m beyond the pavement edge; top {CrestTop} m above it");}
  Ridge("main face","Main"==routes[mi].n?mi:mi,1,448,494,MainFace,KeepOutMain,true);
  Ridge("Summit Traverse face",ti,-1,4,42,StFace,KeepOutSt,true);
  for(int i=0;i<c.Count;i++)c[i]=c[i]==Rock?Rock:Earth;
  var g=Make("barrier summit crest outcrop (0.70 BUG-005/006)",Store("barrier-crest-outcrop",Build(v,c,t)),GroundMaterial());
  Note($"Part C {Scene}: summit crest rock outcrop built: {made} cross-sections, {t.Count/3} tris; cliff face {MainFace} m beyond the main right edge (s 448-494) and {StFace} m beyond the Summit Traverse left edge (s 4-42); top {CrestTop} m above the pavement edge");
  Dependents(g.GetComponent<MeshCollider>(),"Part C outcrop");File.WriteAllLines("Docs/Report070/partC-outcrop-log.txt",log);}

 // Terrain (not pavement, barriers, roofs or this round's meshes) whose vertices stand above a new surface inside its
 // footprint (up to 'reach' above it) is lowered 0.1 m under it, never toward a road below (5A.4).
 static int LowerUnder(MeshCollider top,float reach,string label,Vector3 centre=default,float radius=0,float sink=.1f){int n=0;var tb=top.bounds;
  foreach(var mf in Object.FindObjectsByType<MeshFilter>().Where(x=>x.sharedMesh&&((x.TryGetComponent<MeshCollider>(out var c)&&IsTerrain(c)&&!IsDrive(c)&&!IsBarrier(c)&&!IsRoof(c))||x.name=="Ground_Report067 edge seams")&&!x.name.StartsWith("Ground_Report070")&&x.GetComponent<Renderer>()&&x.GetComponent<Renderer>().bounds.Intersects(tb)).ToArray()){
   var m=mf.sharedMesh;var path=AssetDatabase.GetAssetPath(m);var lv=m.vertices;bool any=false;
   for(int i=0;i<lv.Length;i++){var w=mf.transform.TransformPoint(lv[i]);if(w.x<tb.min.x||w.x>tb.max.x||w.z<tb.min.z||w.z>tb.max.z)continue;
    if(radius>0&&new Vector2(w.x-centre.x,w.z-centre.z).magnitude>radius)continue;
    if(!top.Raycast(new Ray(new Vector3(w.x,w.y+reach+.5f,w.z),Vector3.down),out var h,reach+40))continue;float target=h.point.y-sink;if(w.y<=target+.04f||w.y>h.point.y+reach)continue;
    if(Physics.Raycast(new Vector3(w.x,target-.05f,w.z),Vector3.down,out var below,6,~0,QueryTriggerInteraction.Ignore)&&IsDrive(below.collider))continue;
    w.y=target;lv[i]=mf.transform.InverseTransformPoint(w);n++;any=true;}
   if(any){if(!path.StartsWith("Assets/")){Note($"{label}: {mf.name} mesh is not a project asset ({path}); vertices left");continue;}m=Own(mf);m.vertices=lv;m.RecalculateNormals();m.RecalculateBounds();Refresh(mf);}}
  return n;}
 // A mesh asset also used by another scene is cloned for this scene before it is edited (the other scene is unchanged).
 static Dictionary<string,HashSet<string>> deps;
 static Mesh Own(MeshFilter mf){var m=mf.sharedMesh;var path=AssetDatabase.GetAssetPath(m);
  if(deps==null){deps=new();foreach(var sc in AssetDatabase.FindAssets("t:Scene",new[]{"Assets/Scenes"}).Select(AssetDatabase.GUIDToAssetPath))deps[System.IO.Path.GetFileNameWithoutExtension(sc)]=new HashSet<string>(AssetDatabase.GetDependencies(sc,false));}
  var users=deps.Where(kv=>kv.Key!=Scene&&kv.Value.Contains(path)).Select(kv=>kv.Key).ToArray();if(users.Length==0)return m;
  var copy=Object.Instantiate(m);copy.name=m.name;var own=Store("own-"+System.IO.Path.GetFileNameWithoutExtension(path),copy);mf.sharedMesh=own;if(mf.TryGetComponent<MeshCollider>(out var mc)&&mc.sharedMesh==m)mc.sharedMesh=own;
  Note($"{mf.name}: mesh {path} is shared with {string.Join(",",users)}; this scene now uses its own copy {AssetDatabase.GetAssetPath(own)}");return own;}
 // Grounded objects inside a new surface's footprint: posts/signs that would be buried are raised onto it, trees noted.
 static void Dependents(MeshCollider mc,string label){int signs=0,trees=0;
  foreach(var c in Object.FindObjectsByType<Collider>().Where(x=>x!=mc&&!x.isTrigger&&!x.attachedRigidbody).ToArray()){var b=c.bounds;if(b.size.x>8||b.size.z>8)continue;
   bool trunk=c.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0;var sign=c.GetComponentInParent<PhysicalSign>();bool post=c.name.IndexOf("post",StringComparison.OrdinalIgnoreCase)>=0;if(!trunk&&!sign&&!post)continue;
   if(!mc.Raycast(new Ray(new Vector3(b.center.x,b.max.y+40,b.center.z),Vector3.down),out var h,120)||h.point.y<b.min.y+.25f||h.point.y>b.max.y)continue;
   if(trunk&&!sign){trees++;Note($"{label}: tree trunk '{Path(c.transform)}' base {h.point.y-b.min.y:F2} m inside the new surface (tree stands on it)");continue;}
   var root=sign?sign.transform:c.transform.parent?c.transform.parent:c.transform;float dy=h.point.y-.2f-b.min.y;root.position+=Vector3.up*dy;signs++;Note($"{label}: raised sign/post '{Path(root)}' {dy:F2} m onto the new surface");}
  Note($"{label}: signs/posts raised {signs}; tree bases inside the new surface {trees}");}

 // ---------- BUG-006 / BUG-007: one smooth surface where lumpy/torn terrain sheets meet the road ----------
 // 0.5 m grid in a circle: cells over pavement that border open ground are pinned 3 cm under the pavement (flush,
 // tucked under its edge); the rim is pinned 2 cm under the existing ground; everything between is the smooth
 // (harmonic) surface joining them. Terrain sheets above it inside the circle are lowered under it. Collidable.
 static void Patch(string label,Vector3 c,float R,float sink=.1f){var mat=Surface(c.x,c.z,c.y+3,x=>IsTerrain(x)&&!IsDrive(x),out var mh,8)&&mh.collider.GetComponent<Renderer>()?mh.collider.GetComponent<Renderer>().sharedMaterial:GroundMaterial();
  int N=Mathf.CeilToInt(R/.5f);int S=2*N+1;var H=new float[S,S];var kind=new int[S,S];// 0 out,1 pave-pinned,2 rim-pinned,3 free,4 deep pavement
  for(int i=0;i<S;i++)for(int j=0;j<S;j++){float x=c.x+(i-N)*.5f,z=c.z+(j-N)*.5f;float r=new Vector2(x-c.x,z-c.z).magnitude;if(r>R){kind[i,j]=0;continue;}
   if(Surface(x,z,c.y+3,Solid,out var top,7)&&IsDrive(top.collider)){kind[i,j]=4;H[i,j]=top.point.y-.03f;continue;}
   if(!Ground(new Vector3(x,0,z),c.y+3,8,out var gh)||IsBarrier(gh.collider)){kind[i,j]=0;continue;}
   H[i,j]=gh.point.y;kind[i,j]=r>R-.75f?2:3;if(kind[i,j]==2)H[i,j]-=.02f;}
  for(int i=0;i<S;i++)for(int j=0;j<S;j++)if(kind[i,j]==4){bool edge=false;for(int a=-1;a<=1;a++)for(int b=-1;b<=1;b++){int ii=i+a,jj=j+b;if(ii>=0&&jj>=0&&ii<S&&jj<S&&(kind[ii,jj]==2||kind[ii,jj]==3))edge=true;}if(edge)kind[i,j]=1;}
  for(int it=0;it<1500;it++){var nh=(float[,])H.Clone();for(int i=0;i<S;i++)for(int j=0;j<S;j++){if(kind[i,j]!=3)continue;float sum=0;int n=0;foreach(var (a,b) in new[]{(1,0),(-1,0),(0,1),(0,-1)}){int ii=i+a,jj=j+b;if(ii<0||jj<0||ii>=S||jj>=S)continue;var k=kind[ii,jj];if(k==1||k==2||k==3){sum+=H[ii,jj];n++;}}if(n>0)nh[i,j]=sum/n;}H=nh;}
  var v=new List<Vector3>();var col=new List<Color>();var tri=new List<int>();bool In(int i,int j)=>kind[i,j]==1||kind[i,j]==2||kind[i,j]==3;
  Vector3 P(int i,int j)=>new(c.x+(i-N)*.5f,H[i,j],c.z+(j-N)*.5f);
  for(int i=0;i+1<S;i++)for(int j=0;j+1<S;j++){if(!In(i,j)||!In(i+1,j)||!In(i,j+1)||!In(i+1,j+1))continue;if(kind[i,j]==1&&kind[i+1,j]==1&&kind[i,j+1]==1&&kind[i+1,j+1]==1)continue;
   Tri(v,col,tri,P(i,j),P(i+1,j),P(i+1,j+1),Vector3.up);Tri(v,col,tri,P(i,j),P(i+1,j+1),P(i,j+1),Vector3.up);}
  for(int i=0;i<col.Count;i++)col[i]=Earth;
  var cellsFree=0;foreach(var k in kind)if(k==3)cellsFree++;
  var g=Make("patch "+label,Store("patch-"+label.ToLower().Replace(' ','-'),Build(v,col,tri)),mat);var gc=g.GetComponent<MeshCollider>();
  int lowered=LowerUnder(gc,3f,label,c,R-1.25f,sink);
  Note($"{label}: smooth collidable surface r {R} m around {c} (terrain left under it sunk {sink} m below it) ({cellsFree} free 0.5 m cells, {tri.Count/3} tris, material {mat.name}); pinned flush 3 cm under the pavement edge and 2 cm under the ground at the rim; terrain vertices lowered under it {lowered}");
  Dependents(gc,label);}
}
