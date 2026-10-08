using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 0.95 editor-only probes and authoring (copied into Assets/Editor/Report095Temp while it runs).
public static class Report095Author {
 static string Out=>Environment.GetEnvironmentVariable("PROBE_OUT");
 static string V(Vector3 v)=>$"({v.x:F1}, {v.y:F1}, {v.z:F1})";
 // Part D: every ShallowWater in every course scene and in FreeRoamWorld (name, centre, size, surface, active, renderer,
 // material), and what is at Dan's two lake positions in each.
 public static void Waters(){
  var lines=new List<string>();var points=new[]{new Vector3(667.39f,73.03f,-18.02f),new Vector3(710.50f,79.65f,-79.22f)};
  foreach(var path in Directory.GetFiles("Assets/Scenes","*.unity")){
   var scene=EditorSceneManager.OpenScene(path);lines.Add("==== "+scene.name);
   foreach(var w in scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Racer.ShallowWater>(true))){
    var t=w.transform;var r=w.GetComponent<MeshRenderer>();var s=t.lossyScale;float near=points.Min(p=>Vector2.Distance(new Vector2(p.x,p.z),new Vector2(t.position.x,t.position.z)));
    lines.Add($"{(near<150?"NEAR ":"     ")}{Path(t)} at {V(t.position)} scale {V(s)} surface {w.Surface:F2} round {w.round} activeSelf {w.gameObject.activeSelf} inHierarchy {w.gameObject.activeInHierarchy} renderer {(r?(r.enabled?"on":"OFF")+" "+(r.sharedMaterial?r.sharedMaterial.name:"none"):"none")} dist {near:F0}");}
   foreach(var p in points){var hits=Physics.RaycastAll(p+Vector3.up*30,Vector3.down,80,~0,QueryTriggerInteraction.Collide).OrderBy(h=>h.distance);
    lines.Add($"  at {V(p)}: "+string.Join(" | ",hits.Select(h=>$"{Path(h.collider.transform)} y {h.point.y:F2}{(h.collider.isTrigger?" trigger":"")}")));}
  }
  Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/waters.txt",lines);EditorApplication.Exit(0);}

 // Part D: the Forest Loop content (CR056) in LakeWoods against FreeRoamWorld: its children, the ground tiles' meshes near the
 // lake and the J1 creek, and a height grid over both (each scene's ground under the same points).
 public static void Lake(){
  var lines=new List<string>();var areas=new[]{(c:new Vector3(642,78,-20),r:90f,name:"Friend's lake"),(c:new Vector3(364.5f,73.9f,234.9f),r:50f,name:"J1 creek")};
  var grids=new Dictionary<string,Dictionary<(int,int),float>>();
  foreach(var path in new[]{"Assets/Scenes/LakeWoods.unity","Assets/Scenes/FreeRoamWorld.unity","Assets/Scenes/ForestLoopReverse.unity"}){
   var scene=EditorSceneManager.OpenScene(path);lines.Add("==== "+scene.name);var grid=new Dictionary<(int,int),float>();grids[scene.name]=grid;
   var root=scene.GetRootGameObjects().FirstOrDefault(g=>g.name=="CR056 Forest Loop");
   if(root)foreach(Transform ch in root.transform)lines.Add($"  CR056 child {ch.name} at {V(ch.position)} active {ch.gameObject.activeSelf} comps {string.Join(",",ch.GetComponents<Component>().Select(c=>c.GetType().Name))} children {ch.childCount}");
   else lines.Add("  no CR056 Forest Loop root; roots: "+string.Join(", ",scene.GetRootGameObjects().Select(g=>g.name)));
   foreach(var a in areas){
    foreach(var mf in scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<MeshFilter>(true)).Where(m=>m.sharedMesh&&m.GetComponent<Collider>()&&m.GetComponent<Renderer>()&&m.GetComponent<Renderer>().bounds.SqrDistance(a.c)<a.r*a.r&&m.GetComponent<Renderer>().bounds.size.magnitude>30))
     lines.Add($"  [{a.name}] {Path(mf.transform)} mesh {mf.sharedMesh.name} ({AssetDatabase.GetAssetPath(mf.sharedMesh)}) collider {(mf.GetComponent<MeshCollider>()?mf.GetComponent<MeshCollider>().sharedMesh?.name:"-")} active {mf.gameObject.activeInHierarchy}");
    for(int x=-(int)a.r;x<=a.r;x+=6)for(int z=-(int)a.r;z<=a.r;z+=6){var p=a.c+new Vector3(x,0,z);var hits=Physics.RaycastAll(p+Vector3.up*60,Vector3.down,140,~0,QueryTriggerInteraction.Ignore).Where(h=>!h.collider.GetComponentInParent<Racer.ShallowWater>()).OrderBy(h=>h.distance).ToList();
     if(hits.Count>0)grid[((int)p.x,(int)p.z)]=hits[0].point.y;}}
  }
  // the ground differences (LakeWoods against Free Roam) per area
  foreach(var other in new[]{"FreeRoamWorld","ForestLoopReverse"}){
   var a=grids["LakeWoods"];var b=grids[other];var diff=a.Keys.Where(b.ContainsKey).Select(k=>(k,d:a[k]-b[k])).Where(t=>Mathf.Abs(t.d)>.05f).ToList();
   lines.Add($"== LakeWoods minus {other}: {diff.Count} of {a.Count} points differ by more than 5 cm; max {(diff.Count>0?diff.Max(t=>t.d):0):F2}, min {(diff.Count>0?diff.Min(t=>t.d):0):F2}");
   foreach(var t in diff.OrderBy(t=>t.k.Item1).ThenBy(t=>t.k.Item2).Take(400))lines.Add($"   {t.k.Item1},{t.k.Item2}: LakeWoods {a[t.k]:F2} {other} {b[t.k]:F2} diff {t.d:F2}");}
  Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/lake.txt",lines);
  File.WriteAllLines(Out+"/grids.csv",new[]{"x,z,"+string.Join(",",grids.Keys)}.Concat(grids["LakeWoods"].Keys.Select(k=>$"{k.Item1},{k.Item2},"+string.Join(",",grids.Keys.Select(n=>grids[n].TryGetValue(k,out var y)?y.ToString("F2"):"")))));EditorApplication.Exit(0);}

 // Part D: the lake region in FreeRoamWorld against ForestLoopReverse (which has the lake and the same Kyle drive): the
 // ground tiles' vertices that differ (same tile names, matched by x / z), and every object standing in the region.
 static readonly Vector3 LakeCentre=new(642,78,-20);const float LakeRegion=95;
 public static void LakeTiles(){
  var lines=new List<string>();var tiles=new Dictionary<string,Dictionary<string,Mesh>>();var objects=new Dictionary<string,List<string>>();
  foreach(var path in new[]{"Assets/Scenes/ForestLoopReverse.unity","Assets/Scenes/FreeRoamWorld.unity"}){
   var scene=EditorSceneManager.OpenScene(path);var mine=tiles[scene.name]=new Dictionary<string,Mesh>();var objs=objects[scene.name]=new List<string>();
   var loop=scene.GetRootGameObjects().First(g=>g.name=="Memory loop - north is +Z");
   foreach(Transform t in loop.transform){var mf=t.GetComponent<MeshFilter>();if(mf&&t.name.StartsWith("Ground_")&&mf.sharedMesh)mine[t.name]=mf.sharedMesh;}
   foreach(var r in scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Renderer>(true))){
    var b=r.bounds;var c=b.center;if(new Vector2(c.x-LakeCentre.x,c.z-LakeCentre.z).magnitude>LakeRegion||b.size.magnitude>60)continue;
    objs.Add($"{Path(r.transform)}|{r.transform.position.x:F1},{r.transform.position.y:F1},{r.transform.position.z:F1}|active {r.gameObject.activeInHierarchy}|collider {(r.GetComponent<Collider>()!=null)}");}
   foreach(var c in scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Collider>(true)).Where(c=>!c.GetComponent<Renderer>())){
    var b=c.bounds;var cc=b.center;if(new Vector2(cc.x-LakeCentre.x,cc.z-LakeCentre.z).magnitude>LakeRegion||b.size.magnitude>60)continue;objs.Add($"{Path(c.transform)}|{c.transform.position.x:F1},{c.transform.position.y:F1},{c.transform.position.z:F1}|collider only {c.GetType().Name} trigger {c.isTrigger}");}}
  foreach(var name in tiles["FreeRoamWorld"].Keys.Intersect(tiles["ForestLoopReverse"].Keys).OrderBy(n=>n)){
   var a=tiles["FreeRoamWorld"][name];var b=tiles["ForestLoopReverse"][name];if(a==b)continue;var va=a.vertices;var vb=b.vertices;
   var index=new Dictionary<(int,int),float>();foreach(var v in vb)index[(Mathf.RoundToInt(v.x*10),Mathf.RoundToInt(v.z*10))]=v.y;
   int differ=0,unmatched=0,inLake=0;float maxD=0;Vector3 far=Vector3.zero;var bounds=new Bounds();bool first=true;
   foreach(var v in va){if(!index.TryGetValue((Mathf.RoundToInt(v.x*10),Mathf.RoundToInt(v.z*10)),out var y)){unmatched++;continue;}if(Mathf.Abs(y-v.y)>.02f){differ++;var w=new Vector3(v.x,v.y,v.z);if(first){bounds=new Bounds(w,Vector3.zero);first=false;}else bounds.Encapsulate(w);if(new Vector2(v.x-LakeCentre.x,v.z-LakeCentre.z).magnitude<LakeRegion)inLake++;else if(Mathf.Abs(y-v.y)>maxD){maxD=Mathf.Abs(y-v.y);far=w;}}}
   lines.Add($"{name}: FreeRoam {a.name} ({va.Length} v) / ForestReverse {b.name} ({vb.Length} v): {differ} differ ({inLake} within {LakeRegion} m of the lake), {unmatched} unmatched; differing bounds {bounds.min}..{bounds.max}; largest outside the region {maxD:F2} at {far}");}
  lines.Add("== objects in the lake region only in FreeRoamWorld");lines.AddRange(objects["FreeRoamWorld"].Except(objects["ForestLoopReverse"]));
  lines.Add("== objects in the lake region only in ForestLoopReverse");lines.AddRange(objects["ForestLoopReverse"].Except(objects["FreeRoamWorld"]));
  Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/lake-tiles.txt",lines);EditorApplication.Exit(0);}

 // Part D dry run: in FreeRoamWorld, the ground difference to ForestLoopReverse by distance from the lake centre; the roads
 // and trails in the region; every renderer whose mesh has vertices in it (with the asset and the vertex count there).
 public static void LakeDry(){
  var lines=new List<string>();
  EditorSceneManager.OpenScene("Assets/Scenes/ForestLoopReverse.unity");var forest=new Dictionary<(int,int),float>();
  foreach(Transform t in GameObject.Find("Memory loop - north is +Z").transform){var mf=t.GetComponent<MeshFilter>();if(!mf||!t.name.StartsWith("Ground_")||!mf.sharedMesh)continue;var m=t.localToWorldMatrix;foreach(var v in mf.sharedMesh.vertices){var w=m.MultiplyPoint3x4(v);if(Flat(w)<120)forest[(Mathf.RoundToInt(w.x*10),Mathf.RoundToInt(w.z*10))]=w.y;}}
  var scene=EditorSceneManager.OpenScene("Assets/Scenes/FreeRoamWorld.unity");var rings=new SortedDictionary<int,(int n,float max,float min)>();
  foreach(Transform t in GameObject.Find("Memory loop - north is +Z").transform){var mf=t.GetComponent<MeshFilter>();if(!mf||!t.name.StartsWith("Ground_")||!mf.sharedMesh)continue;var m=t.localToWorldMatrix;
   foreach(var v in mf.sharedMesh.vertices){var w=m.MultiplyPoint3x4(v);float r=Flat(w);if(r>120||!forest.TryGetValue((Mathf.RoundToInt(w.x*10),Mathf.RoundToInt(w.z*10)),out var y))continue;float d=y-w.y;if(Mathf.Abs(d)<.02f)continue;int k=(int)(r/5)*5;var e=rings.TryGetValue(k,out var o)?o:(0,float.MinValue,float.MaxValue);rings[k]=(e.Item1+1,Mathf.Max(e.Item2,d),Mathf.Min(e.Item3,d));}}
  foreach(var kv in rings)lines.Add($"ring {kv.Key}-{kv.Key+5} m: {kv.Value.n} vertices differ, forest minus Free Roam {kv.Value.min:F2}..{kv.Value.max:F2}");
  foreach(var road in UnityEngine.Object.FindObjectsByType<Racer.RaceRoad>(FindObjectsSortMode.None)){road.Initialize();int near=0;float rmin=999;for(float s=0;s<road.Length;s+=2){var p=road.At(s,out _);float r=Flat(p);if(r<100){near++;rmin=Mathf.Min(rmin,r);}}if(near>0)lines.Add($"road {Path(road.transform)}: {near} samples within 100 m, nearest {rmin:F1} m");}
  foreach(var wr in UnityEngine.Object.FindObjectsByType<Racer.WoodlandRoute>(FindObjectsSortMode.None)){try{wr.Initialize();}catch{}int near=0;float rmin=999;if(wr.points!=null)foreach(var p in wr.points){float r=Flat(p);if(r<100){near++;rmin=Mathf.Min(rmin,r);}}if(near>0)lines.Add($"trail {wr.title} ({Path(wr.transform)}): {near} points within 100 m, nearest {rmin:F1} m");}
  foreach(var mf in scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<MeshFilter>(true))){if(!mf.sharedMesh||mf.name.StartsWith("Ground_")||!mf.sharedMesh.isReadable)continue;var r=mf.GetComponent<Renderer>();if(!r||r.bounds.SqrDistance(LakeCentre)>100*100)continue;var m=mf.transform.localToWorldMatrix;int n=0,inWater=0;foreach(var v in mf.sharedMesh.vertices){var w=m.MultiplyPoint3x4(v);float d=Flat(w);if(d<85)n++;if(d<67)inWater++;}
   if(n>0)lines.Add($"mesh {Path(mf.transform)} '{mf.sharedMesh.name}' ({AssetDatabase.GetAssetPath(mf.sharedMesh)}) mat {(r.sharedMaterial?r.sharedMaterial.name:"-")} collider {(mf.GetComponent<Collider>()!=null)}: {n} vertices within 85 m, {inWater} within 67 m; total {mf.sharedMesh.vertexCount}");}
  Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/lake-dry.txt",lines);EditorApplication.Exit(0);}
 static float Flat(Vector3 w)=>new Vector2(w.x-LakeCentre.x,w.z-LakeCentre.z).magnitude;

 // Part E: Dan's five FreeRoamWorld reports: what is there (colliders within 25 m), a 1 m height grid around each with the
 // holes (no ground within 40 m below) marked, and the open (boundary) edges of ground meshes within 20 m.
 static readonly (string id,Vector3 p)[] Reports={("BUG-001",new Vector3(1193.72f,89.38f,149.28f)),("BUG-006",new Vector3(806.89f,64.03f,328.19f)),("BUG-002",new Vector3(811.92f,94.46f,231.21f)),("BUG-003",new Vector3(1158.74f,109.96f,166.14f)),("BUG-007",new Vector3(1008.87f,164.63f,107.92f))};
 public static void Spots(){
  var lines=new List<string>();var scene=EditorSceneManager.OpenScene("Assets/Scenes/FreeRoamWorld.unity");
  var cols=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Collider>(true)).Where(c=>c.enabled&&c.gameObject.activeInHierarchy&&!c.isTrigger).ToList();
  foreach(var (id,p) in Reports){
   lines.Add($"==== {id} at {V(p)}");
   foreach(var c in cols.Where(c=>c.bounds.SqrDistance(p)<25*25).OrderBy(c=>c.bounds.SqrDistance(p)).Take(40))lines.Add($"  {Path(c.transform)} {c.GetType().Name} {(c is MeshCollider mc&&mc.sharedMesh?mc.sharedMesh.name+" ("+AssetDatabase.GetAssetPath(mc.sharedMesh)+")":"")} bounds {V(c.bounds.min)}..{V(c.bounds.max)}");
   for(int dz=15;dz>=-15;dz-=1){var sb=new System.Text.StringBuilder($"  {dz,3} ");for(int dx=-15;dx<=15;dx++){var q=p+new Vector3(dx,0,dz);
     if(Physics.Raycast(q+Vector3.up*40,Vector3.down,out var h,80,~0,QueryTriggerInteraction.Ignore))sb.Append($"{h.point.y-p.y,5:F1}");else sb.Append("  ###");}lines.Add(sb.ToString());}
   foreach(var mc in cols.OfType<MeshCollider>().Where(c=>c.sharedMesh&&c.sharedMesh.isReadable&&c.bounds.SqrDistance(p)<20*20)){
    var m=mc.sharedMesh;var v=m.vertices;var t=m.triangles;var tr=mc.transform.localToWorldMatrix;var count=new Dictionary<(int,int),int>();
    for(int i=0;i<t.Length;i+=3)for(int k=0;k<3;k++){int a=t[i+k],b=t[i+(k+1)%3];var key=a<b?(a,b):(b,a);count[key]=count.TryGetValue(key,out var n)?n+1:1;}
    foreach(var e in count.Where(e=>e.Value==1)){var a=tr.MultiplyPoint3x4(v[e.Key.Item1]);var b=tr.MultiplyPoint3x4(v[e.Key.Item2]);if(Vector3.Distance((a+b)/2,p)<20)lines.Add($"  open edge of {Path(mc.transform)}: {V(a)}-{V(b)}");}}
  }
  Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/spots.txt",lines);EditorApplication.Exit(0);}

 // Part E1: FreeRoamWorld ground scan on a 2 m grid (static mesh colliders, vehicles and triggers excluded): every cell with
 // no ground under it that touches a cell with ground (the edge of the world or a hole), grouped into connected runs; per
 // run: cells, bounds, the ground height beside it and how steep that ground is (a vehicle can drive to an edge whose ground
 // is under 30 degrees). Enclosed holes (no-ground runs not touching the scan's outer border) are marked.
 public static void Holes(){
  var lines=new List<string>();var scene=EditorSceneManager.OpenScene("Assets/Scenes/FreeRoamWorld.unity");Physics.SyncTransforms();
  var ground=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<MeshCollider>(true)).Where(c=>c.enabled&&c.gameObject.activeInHierarchy&&!c.isTrigger&&!c.attachedRigidbody&&c.bounds.size.x>30&&c.bounds.size.z>30).ToList();
  var b=ground[0].bounds;foreach(var c in ground)b.Encapsulate(c.bounds);const float step=2;
  int nx=Mathf.CeilToInt(b.size.x/step)+1,nz=Mathf.CeilToInt(b.size.z/step)+1;var h=new float[nx,nz];var n=new Vector3[nx,nz];var has=new bool[nx,nz];var set=new HashSet<Collider>(ground);
  for(int i=0;i<nx;i++)for(int k=0;k<nz;k++){var o=new Vector3(b.min.x+i*step,b.max.y+10,b.min.z+k*step);
   foreach(var hit in Physics.RaycastAll(o,Vector3.down,b.size.y+40,~0,QueryTriggerInteraction.Ignore).OrderBy(x=>x.distance)){if(!set.Contains(hit.collider))continue;has[i,k]=true;h[i,k]=hit.point.y;n[i,k]=hit.normal;break;}}
  lines.Add($"scan {nx}x{nz} cells of {step} m over {V(b.min)}..{V(b.max)}; {ground.Count} ground colliders; cells with ground {has.Cast<bool>().Count(x=>x)}");
  // no-ground runs that touch ground
  var seen=new bool[nx,nz];int runs=0;var report=new List<(int cells,int edge,bool enclosed,float drive,Vector3 min,Vector3 max,float y)>();
  for(int i=0;i<nx;i++)for(int k=0;k<nz;k++){if(has[i,k]||seen[i,k])continue;var q=new Queue<(int,int)>();q.Enqueue((i,k));seen[i,k]=true;int cells=0,edge=0,drivable=0;bool border=false;var mn=new Vector3(1e9f,0,1e9f);var mx=new Vector3(-1e9f,0,-1e9f);float ys=0;
   while(q.Count>0){var (a,c)=q.Dequeue();cells++;if(a==0||c==0||a==nx-1||c==nz-1)border=true;
    foreach(var (da,dc) in new[]{(1,0),(-1,0),(0,1),(0,-1)}){int x=a+da,z=c+dc;if(x<0||z<0||x>=nx||z>=nz)continue;
     if(has[x,z]){edge++;ys+=h[x,z];if(Vector3.Angle(n[x,z],Vector3.up)<30)drivable++;var w=new Vector3(b.min.x+x*step,0,b.min.z+z*step);mn=Vector3.Min(mn,w);mx=Vector3.Max(mx,w);continue;}
     if(!seen[x,z]){seen[x,z]=true;q.Enqueue((x,z));}}}
   if(edge>0)report.Add((cells,edge,!border,edge>0?drivable/(float)edge:0,mn,mx,edge>0?ys/edge:0));}
  foreach(var r in report.OrderByDescending(r=>r.edge)){runs++;lines.Add($"{(r.enclosed?"HOLE ":"EDGE ")}{r.cells} empty cells, {r.edge} edge cells ({r.drive:P0} of the ground beside it drivable), ground beside {V(r.min)}..{V(r.max)} mean y {r.y:F1}");}
  // the edge cells themselves (drivable ones), in 20 m bins, for the outer edge
  var bins=new Dictionary<(int,int),(int all,int drive,float y)>();
  for(int i=0;i<nx;i++)for(int k=0;k<nz;k++){if(!has[i,k])continue;bool e=false;foreach(var (da,dc) in new[]{(1,0),(-1,0),(0,1),(0,-1)}){int x=i+da,z=k+dc;if(x>=0&&z>=0&&x<nx&&z<nz&&!has[x,z])e=true;}if(!e)continue;
   var key=((int)Mathf.Floor((b.min.x+i*step)/20)*20,(int)Mathf.Floor((b.min.z+k*step)/20)*20);var v=bins.TryGetValue(key,out var o)?o:(0,0,0f);bins[key]=(v.Item1+1,v.Item2+(Vector3.Angle(n[i,k],Vector3.up)<30?1:0),v.Item3+h[i,k]);}
  lines.Add("== edge bins (20 m): x,z: edge cells, drivable, mean y");foreach(var kv in bins.OrderBy(k=>k.Key.Item1).ThenBy(k=>k.Key.Item2))lines.Add($"   {kv.Key.Item1},{kv.Key.Item2}: {kv.Value.all} {kv.Value.drive} {kv.Value.y/kv.Value.all:F1}");
  Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/holes.txt",lines);EditorApplication.Exit(0);}

 // Part E1: the same scan with the runtime world edge built (its hills given colliders for the scan, so a cell counts as
 // covered when the eye would see ground or hill there): what is still open.
 public static void HolesWithEdge(){
  var lines=new List<string>();var scene=EditorSceneManager.OpenScene("Assets/Scenes/FreeRoamWorld.unity");
  var host=new GameObject("edge host");UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(host,scene);var edge=host.AddComponent<Racer.WorldEdge>();edge.Build(scene);lines.AddRange(Racer.WorldEdge.Report.Select(r=>"world edge: "+r));
  var root=GameObject.Find("World edge (0.81)");int hills=0;foreach(var mf in root.GetComponentsInChildren<MeshFilter>()){if(mf.GetComponent<Collider>())continue;mf.gameObject.AddComponent<MeshCollider>().sharedMesh=mf.sharedMesh;hills++;}
  Physics.SyncTransforms();lines.Add($"hills given scan colliders: {hills}");
  var ground=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<MeshCollider>(true)).Where(c=>c.enabled&&c.gameObject.activeInHierarchy&&!c.isTrigger&&!c.attachedRigidbody&&(c.bounds.size.x>30&&c.bounds.size.z>30||c.transform.IsChildOf(root.transform))).ToList();
  var b=new Bounds(new Vector3(0,90,50),new Vector3(2700,400,1800));const float step=2;var set=new HashSet<Collider>(ground);
  int nx=Mathf.CeilToInt(b.size.x/step)+1,nz=Mathf.CeilToInt(b.size.z/step)+1;var h=new float[nx,nz];var has=new bool[nx,nz];var hill=new bool[nx,nz];
  for(int i=0;i<nx;i++)for(int k=0;k<nz;k++){var o=new Vector3(b.min.x+i*step,b.max.y+10,b.min.z+k*step);
   foreach(var hit in Physics.RaycastAll(o,Vector3.down,b.size.y+40,~0,QueryTriggerInteraction.Ignore).OrderBy(x=>x.distance)){if(!set.Contains(hit.collider))continue;has[i,k]=true;h[i,k]=hit.point.y;hill[i,k]=hit.collider.transform.IsChildOf(root.transform);break;}}
  var seen=new bool[nx,nz];
  for(int i=0;i<nx;i++)for(int k=0;k<nz;k++){if(has[i,k]||seen[i,k])continue;var q=new Queue<(int,int)>();q.Enqueue((i,k));seen[i,k]=true;int cells=0,edgeCells=0;bool border=false;var mn=new Vector3(1e9f,0,1e9f);var mx=new Vector3(-1e9f,0,-1e9f);float ys=0;
   while(q.Count>0){var (a,c)=q.Dequeue();cells++;if(a==0||c==0||a==nx-1||c==nz-1)border=true;
    foreach(var (da,dc) in new[]{(1,0),(-1,0),(0,1),(0,-1)}){int x=a+da,z=c+dc;if(x<0||z<0||x>=nx||z>=nz)continue;
     if(has[x,z]){if(!hill[x,z]){edgeCells++;ys+=h[x,z];var w=new Vector3(b.min.x+x*step,0,b.min.z+z*step);mn=Vector3.Min(mn,w);mx=Vector3.Max(mx,w);}continue;}
     if(!seen[x,z]){seen[x,z]=true;q.Enqueue((x,z));}}}
   if(edgeCells>0)lines.Add($"{(border?"OPEN TO THE OUTSIDE ":"ENCLOSED ")}{cells} empty cells beside {edgeCells} world-ground cells {V(mn)}..{V(mx)} mean y {ys/edgeCells:F1}");}
  // the world ground cells beside empty ones, by 20 m bins
  var bins=new Dictionary<(int,int),int>();
  for(int i=1;i<nx-1;i++)for(int k=1;k<nz-1;k++){if(!has[i,k]||hill[i,k])continue;if(has[i+1,k]&&has[i-1,k]&&has[i,k+1]&&has[i,k-1])continue;var key=((int)Mathf.Floor((b.min.x+i*step)/20)*20,(int)Mathf.Floor((b.min.z+k*step)/20)*20);bins[key]=bins.TryGetValue(key,out var o)?o+1:1;}
  lines.Add("== bins x,z: ground cells beside nothing");foreach(var kv in bins.OrderBy(k=>k.Key.Item1).ThenBy(k=>k.Key.Item2))lines.Add($"   {kv.Key.Item1},{kv.Key.Item2}: {kv.Value}");
  Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/holes-edge.txt",lines);EditorApplication.Exit(0);}

 // what is at given points (every collider down the ray, any size; renderers whose bounds hold the point)
 public static void At(){
  var lines=new List<string>();var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+(Environment.GetEnvironmentVariable("AT_SCENE")??"FreeRoamWorld")+".unity");Physics.SyncTransforms();
  var rends=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Renderer>(true)).ToList();
  foreach(var spec in Environment.GetEnvironmentVariable("AT_POINTS").Split(';')){var f=spec.Split(',').Select(x=>float.Parse(x,System.Globalization.CultureInfo.InvariantCulture)).ToArray();var p=new Vector3(f[0],f[1],f[2]);
   lines.Add($"== {V(p)}: colliders: "+string.Join(" | ",Physics.RaycastAll(p+Vector3.up*60,Vector3.down,160,~0,QueryTriggerInteraction.Collide).OrderBy(h=>h.distance).Select(h=>$"{Path(h.collider.transform)} y {h.point.y:F2}{(h.collider.isTrigger?" (trigger)":"")}")));
   foreach(var r in rends.Where(r=>{var b=r.bounds;return p.x>=b.min.x&&p.x<=b.max.x&&p.z>=b.min.z&&p.z<=b.max.z&&b.size.magnitude<400;}).Take(30))lines.Add($"   renderer {Path(r.transform)} bounds {V(r.bounds.min)}..{V(r.bounds.max)} active {r.gameObject.activeInHierarchy}");}
  Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/at.txt",lines);EditorApplication.Exit(0);}

 // editor shots of FreeRoamWorld (authored scene; no runtime scenery): SHOTS="name:x,y,z:yaw:pitch;..."
 public static void Shots(){
  var scene=EditorSceneManager.OpenScene("Assets/Scenes/FreeRoamWorld.unity");var go=new GameObject("shot cam");var cam=go.AddComponent<Camera>();cam.fieldOfView=60;cam.farClipPlane=3000;cam.nearClipPlane=.1f;
  var rt=new RenderTexture(1600,900,24);cam.targetTexture=rt;var tex=new Texture2D(1600,900,TextureFormat.RGB24,false);Directory.CreateDirectory(Out+"/shots");
  if(Environment.GetEnvironmentVariable("SHOT_EDGE")=="1"){var host=new GameObject("edge host");UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(host,scene);host.AddComponent<Racer.WorldEdge>().Build(scene);}
  foreach(var spec in Environment.GetEnvironmentVariable("SHOTS").Split(';')){var a=spec.Split(':');var f=a[1].Split(',').Select(x=>float.Parse(x,System.Globalization.CultureInfo.InvariantCulture)).ToArray();
   go.transform.SetPositionAndRotation(new Vector3(f[0],f[1],f[2]),Quaternion.Euler(float.Parse(a[3],System.Globalization.CultureInfo.InvariantCulture),float.Parse(a[2],System.Globalization.CultureInfo.InvariantCulture),0));
   cam.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1600,900),0,0);tex.Apply();RenderTexture.active=null;File.WriteAllBytes(Out+"/shots/"+a[0]+".jpg",tex.EncodeToJPG(88));}
  EditorApplication.Exit(0);}

 // Part D: the roads and trails near the lake in FreeRoamWorld: samples within 95 m (height, ground, distance)
 public static void LakeRoads(){
  var lines=new List<string>();var scene=EditorSceneManager.OpenScene("Assets/Scenes/FreeRoamWorld.unity");Physics.SyncTransforms();
  foreach(var r in scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Racer.RaceRoad>(true))){if(r.points==null||r.points.Length<2)continue;r.Initialize();bool any=false;
   for(float s=0;s<r.Length;s+=6){var p=r.At(s,out _);float d=Flat(p);if(d>95)continue;if(!any){lines.Add($"== {Path(r.transform)} len {r.Length:F0} trail {r.forestTrail}");any=true;}
    float g=Physics.Raycast(p+Vector3.up*3,Vector3.down,out var h,20,~0,QueryTriggerInteraction.Ignore)?h.point.y:float.NaN;lines.Add($"   s {s:F0}: {V(p)} r {d:F1} hw {r.HalfWidth(s):F1} ground {g:F2} ({(float.IsNaN(g)?"-":h.collider.name)})");}}
  Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/lake-roads.txt",lines);EditorApplication.Exit(0);}
 static string Path(Transform t){var s=t.name;while(t.parent){t=t.parent;s=t.name+"/"+s;}return s;}
}
