using System;using System.IO;using System.Linq;using System.Text;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 0.95 Parts D and E (editor only, copied into Assets/Editor/Report095Temp while it runs): FreeRoamWorld only.
// D: the lake. Free Roam (its own scene since 0.76, built from Dan's Backyard Reverse) never had the Forest Loop's lake:
//    its ground there is the Street Loop's dry hollow (down to 62.5 m where the lake bed is 77.4 m). The hollow is raised to
//    the Forest scenes' lake bed (raise only: nothing is lowered, so nothing is left hanging; full inside 74 m of the centre,
//    tapering to nothing at 90 m; never within a road's or trail's width + 3 m); the same water object as the Forest scenes
//    goes in (ShallowWater: the slowdown, the escapable shallows, ice in Snow). Trees and trunks on ground raised more than
//    0.3 m are taken out (the Forest scenes have none there); other things on the changed ground are moved with it.
// E1: open cut faces at the edge of the world (BUG-001, BUG-006): every edge of the world's ground tiles with nothing
//    beyond it, where the ground stands more than 0.6 m above the runtime world-edge ring just outside (or no ring), gets
//    a steep earth skirt from the edge down past the ring (drawn and collidable), so there is no see-through and no
//    fall-through. The ring itself is built at load as before (WorldEdge); it is built here only to measure, then removed.
// E3: trunk colliders standing more than 0.3 m above the ground are set down on it (removed when that would put them on a
//    road or trail); the drawn trees follow at load (SceneryTrees.GroundedScenes now includes FreeRoamWorld).
// WORLD_DRY=1: list only.
public static class Report095World {
 const string Scene="Assets/Scenes/FreeRoamWorld.unity",Forest="Assets/Scenes/ForestLoopReverse.unity",AssetDir="Assets/Track/Report095";
 static readonly Vector3 Lake=new(642,78,-20);const float Full=74,Fade=90;
 static string Out=>Environment.GetEnvironmentVariable("PROBE_OUT");
 static bool Dry=>Environment.GetEnvironmentVariable("WORLD_DRY")=="1";
 static string V(Vector3 v)=>$"({v.x:F1}, {v.y:F2}, {v.z:F1})";
 static string P(Transform t){var s=t.name;while(t.parent){t=t.parent;s=t.name+"/"+s;}return s;}
 static (int,int) K(float x,float z)=>(Mathf.RoundToInt(x*10),Mathf.RoundToInt(z*10));
 static float Flat(Vector3 a,Vector3 b)=>new Vector2(a.x-b.x,a.z-b.z).magnitude;
 static bool TreeLike(string n)=>n.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0||n.IndexOf("tree",StringComparison.OrdinalIgnoreCase)>=0;

 public static void Run(){
  var log=new List<string>();
  try{
   // ---- the Forest scene: its ground heights around the lake and its lake object
   EditorSceneManager.OpenScene(Forest);var forest=new Dictionary<(int,int),float>();
   foreach(Transform t in GameObject.Find("Memory loop - north is +Z").transform){var mf=t.GetComponent<MeshFilter>();if(!mf||!t.name.StartsWith("Ground_")||!mf.sharedMesh)continue;var m=t.localToWorldMatrix;
    foreach(var v in mf.sharedMesh.vertices){var w=m.MultiplyPoint3x4(v);if(Flat(w,Lake)<=Fade+4)forest[K(w.x,w.z)]=w.y;}}
   var fl=GameObject.Find("Friend's lake - visible shoreline");var lakeMesh=fl.GetComponent<MeshFilter>().sharedMesh;var lakeMat=fl.GetComponent<MeshRenderer>().sharedMaterial;
   var lakePos=fl.transform.position;var lakeRot=fl.transform.rotation;var lakeScale=fl.transform.lossyScale;bool lakeRound=fl.GetComponent<Racer.ShallowWater>().round;float surface=fl.GetComponent<Racer.ShallowWater>().Surface;
   log.Add($"Forest lake: {V(lakePos)} scale {V(lakeScale)} surface {surface:F2} mesh {AssetDatabase.GetAssetPath(lakeMesh)} material {AssetDatabase.GetAssetPath(lakeMat)}; {forest.Count} forest ground heights");
   // ---- Free Roam
   var scene=EditorSceneManager.OpenScene(Scene);Physics.SyncTransforms();var roots=scene.GetRootGameObjects();
   var roads=roots.SelectMany(g=>g.GetComponentsInChildren<Racer.RaceRoad>(true)).Where(r=>r.points!=null&&r.points.Length>1).ToList();foreach(var r in roads)r.Initialize();
   var trails=roots.SelectMany(g=>g.GetComponentsInChildren<Racer.WoodlandRoute>(true)).Where(w=>w.points!=null&&w.points.Length>1).ToList();foreach(var w in trails)w.Initialize();
   // road / trail samples near the lake (for the protection band)
   var lanes=new List<(Vector3 p,float hw)>();
   foreach(var r in roads)for(float s=0;s<r.Length;s+=1.5f){var p=r.At(s,out _);if(Flat(p,Lake)<Fade+20)lanes.Add((p,r.HalfWidth(s)));}
   foreach(var w in trails)for(float s=0;s<w.Length;s+=1.5f){var p=w.At(s,out _);if(Flat(p,Lake)<Fade+20)lanes.Add((p,w.halfWidth));}
   log.Add($"roads {roads.Count}, trails {trails.Count}; {lanes.Count} road/trail samples within {Fade+20} m of the lake");
   float Weight(Vector3 w){float r=Flat(w,Lake);return r<=Full?1:r>=Fade?0:1-Mathf.SmoothStep(0,1,(r-Full)/(Fade-Full));}
   // a road or trail is never raised: within its width + 3 m the ground may come up only to just under its surface
   float Cap(Vector3 w){float cap=float.MaxValue;foreach(var (p,hw) in lanes){float d=Flat(w,p);if(d<hw+3)cap=Mathf.Min(cap,Mathf.Max(w.y,p.y-.15f));}return cap;}
   // ---- D: the tiles
   var loop=roots.First(g=>g.name=="Memory loop - north is +Z").transform;var delta=new Dictionary<(int,int),float>();var newY=new Dictionary<(int,int),float>();var oldY=new Dictionary<(int,int),float>();
   var tiles=new List<(MeshFilter mf,Vector3[] v)>();
   foreach(Transform t in loop){var mf=t.GetComponent<MeshFilter>();if(!mf||!t.name.StartsWith("Ground_")||!mf.sharedMesh)continue;var m=t.localToWorldMatrix;var inv=t.worldToLocalMatrix;var v=mf.sharedMesh.vertices;bool any=false;
    for(int i=0;i<v.Length;i++){var w=m.MultiplyPoint3x4(v[i]);oldY[K(w.x,w.z)]=w.y;if(Flat(w,Lake)>Fade)continue;if(!forest.TryGetValue(K(w.x,w.z),out var fy))continue;float up=fy-w.y;if(up<=.02f)continue;float k=Weight(w);if(k<=0)continue;
     float d=Mathf.Min(up*k,Cap(w)-w.y);if(d<=.02f)continue;var nw=new Vector3(w.x,w.y+d,w.z);v[i]=inv.MultiplyPoint3x4(nw);delta[K(w.x,w.z)]=d;any=true;}
    if(any)tiles.Add((mf,v));}
   foreach(var kv in oldY)newY[kv.Key]=kv.Value+(delta.TryGetValue(kv.Key,out var dd)?dd:0);
   float Delta(float x,float z){ // bilinear over the 2 m grid
    float gx=Mathf.Floor(x/2)*2,gz=Mathf.Floor(z/2)*2,u=(x-gx)/2,w=(z-gz)/2;float D(float a,float b)=>delta.TryGetValue(K(a,b),out var q)?q:0;
    return Mathf.Lerp(Mathf.Lerp(D(gx,gz),D(gx+2,gz),u),Mathf.Lerp(D(gx,gz+2),D(gx+2,gz+2),u),w);}
   log.Add($"D lake bed: {delta.Count} ground vertices raised (max {(delta.Count>0?delta.Values.Max():0):F2} m) in {tiles.Count} tiles: {string.Join(", ",tiles.Select(t=>t.mf.name))}");
   int wet=delta.Keys.Count(k=>Flat(new Vector3(k.Item1/10f,0,k.Item2/10f),lakePos)<lakeScale.x*.5f&&newY[k]<surface);log.Add($"   after: {wet} lake-footprint vertices under the water ({surface:F2})");
   // shore escape: the steepest ground slope from the water's edge outward, in 16 directions (25 % = 14 degrees wanted on the likely sides)
   for(int a=0;a<16;a++){var dir=Quaternion.Euler(0,a*22.5f,0)*Vector3.forward;float worst=0;Vector3 at=Vector3.zero;float prev=float.NaN;Vector3 pp=Vector3.zero;
    float dryAt=-1;
    for(float r=0;r<Fade+10;r+=2){var q=lakePos+dir*r;float y=Height(newY,q.x,q.z);if(float.IsNaN(y))continue;if(dryAt<0&&y>surface)dryAt=r;if(!float.IsNaN(prev)&&dryAt>=0&&r-dryAt<=20){float g=(y-prev)/2;if(g>worst){worst=g;at=q;}}prev=y;pp=q;}
    log.Add($"   shore {a*22.5f:F0} deg: steepest climb from the water {worst*100:F0} % at {V(at)}");}
   // ---- D: things on the changed ground
   var veg=roots.SelectMany(g=>g.GetComponentsInChildren<MeshRenderer>(true)).Where(r=>r.enabled&&r.gameObject.activeInHierarchy&&(Racer.SceneryTrees.IsOldVegetation(r)||Mountain(r))&&r.bounds.SqrDistance(Lake)<(Fade+10)*(Fade+10)).ToList();
   var vegChanges=new List<(MeshFilter mf,Mesh mesh,int removed,int moved)>();
   foreach(var r in veg){var mf=r.GetComponent<MeshFilter>();var mesh=mf.sharedMesh;var v=mesh.vertices;var t=mesh.triangles;var m=r.transform.localToWorldMatrix;var inv=r.transform.worldToLocalMatrix;
    var parent=Enumerable.Range(0,v.Length).ToArray();int Find(int i){while(parent[i]!=i){parent[i]=parent[parent[i]];i=parent[i];}return i;}
    for(int i=0;i<t.Length;i+=3){int a=Find(t[i]),b=Find(t[i+1]),c=Find(t[i+2]);parent[b]=a;parent[Find(c)]=a;}
    var drop=new HashSet<int>();int removed=0,moved=0;var nv=(Vector3[])v.Clone();
    foreach(var piece in Enumerable.Range(0,v.Length).GroupBy(Find)){var c=piece.Select(i=>m.MultiplyPoint3x4(v[i])).Aggregate(Vector3.zero,(x,y)=>x+y)/piece.Count();float d=Delta(c.x,c.z);
     if(d>.3f){foreach(var i in piece)drop.Add(i);removed++;}else if(d>.02f){foreach(var i in piece)nv[i]=inv.MultiplyPoint3x4(m.MultiplyPoint3x4(v[i])+Vector3.up*d);moved++;}}
    if(removed+moved==0)continue;
    var copy=UnityEngine.Object.Instantiate(mesh);copy.name="FreeRoamWorld-lake-vegetation-"+vegChanges.Count+"-"+Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(mesh));copy.vertices=nv;var tri=new List<int>();for(int i=0;i<t.Length;i+=3)if(!drop.Contains(t[i]))tri.AddRange(new[]{t[i],t[i+1],t[i+2]});copy.SetTriangles(tri,0);copy.RecalculateBounds();
    vegChanges.Add((mf,copy,removed,moved));log.Add($"   vegetation {P(r.transform)} ({AssetDatabase.GetAssetPath(mesh)}): {removed} pieces taken out (ground raised > 0.3 m), {moved} moved up with the ground");}
   var objects=new List<(GameObject g,float d,string what)>();
   var done=new HashSet<Transform>();
   foreach(var leaf in roots.SelectMany(g=>g.GetComponentsInChildren<Transform>(true))){
    if(leaf.IsChildOf(loop)||leaf.GetComponent<Racer.RaceRoad>()||leaf.GetComponent<Racer.WoodlandRoute>())continue;if(!leaf.GetComponent<Renderer>()&&!leaf.GetComponent<Collider>())continue;
    if(leaf.GetComponent<Renderer>() is MeshRenderer lr&&veg.Contains(lr))continue;
    // a small group (a sign: post, board, lettering) moves as one
    var c=leaf;var gp=leaf.parent;if(gp&&gp.parent){var rs=gp.GetComponentsInChildren<Renderer>(true);if(rs.Length>0&&rs.Length<12){var gb=rs[0].bounds;foreach(var x in rs)gb.Encapsulate(x.bounds);if(gb.size.magnitude<12)c=gp;}}
    if(!done.Add(c))continue;
    Bounds b;{var rs=c.GetComponentsInChildren<Renderer>(true);var cs=c.GetComponentsInChildren<Collider>(true);if(rs.Length>0){b=rs[0].bounds;foreach(var x in rs)b.Encapsulate(x.bounds);}else if(cs.Length>0){b=cs[0].bounds;foreach(var x in cs)b.Encapsulate(x.bounds);}else continue;}
    if(b.size.magnitude>40||Flat(b.center,Lake)>Fade+2)continue;
    float d=Delta(b.center.x,b.center.z);if(d<=.02f)continue;float og=Height(oldY,b.center.x,b.center.z),ng=og+d;bool rested=Mathf.Abs(b.min.y-og)<.5f;
    // a tree or trunk standing on raised ground goes (as in the Forest scenes); anything else that stood on the ground moves
    // up with it; something standing above the old ground (the RIDGE RETURN sign hung 11 m over the hollow) stays unless
    // the new ground would bury it
    if(TreeLike(c.name)){if(d>.3f&&(rested||b.min.y<ng-.3f))objects.Add((c.gameObject,d,"remove"));continue;}
    // something left hanging over the hollow (the RIDGE RETURN sign hung 11 m up) is set down on the new ground
    if(rested)objects.Add((c.gameObject,d,"move"));else if(Mathf.Abs(b.min.y-ng)>.05f)objects.Add((c.gameObject,ng-b.min.y,"move"));}
   foreach(var o in objects)log.Add($"   {o.what} {P(o.g.transform)} at {V(o.g.transform.position)} (ground raised {o.d:F2} m)");
   // ---- E1: open cut faces at the world's edge
   var host=new GameObject("Report095 edge probe");UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(host,scene);var edge=host.AddComponent<Racer.WorldEdge>();edge.Build(scene);var ring=GameObject.Find("World edge (0.81)");
   var probe=new List<Collider>();foreach(var mf in ring.GetComponentsInChildren<MeshFilter>()){if(mf.GetComponent<Collider>())continue;var mc=mf.gameObject.AddComponent<MeshCollider>();mc.sharedMesh=mf.sharedMesh;probe.Add(mc);}Physics.SyncTransforms();
   var ringSet=new HashSet<Collider>(probe);
   float RingY(Vector3 p){float best=float.NaN;foreach(var h in Physics.RaycastAll(new Vector3(p.x,p.y+400,p.z),Vector3.down,900,~0,QueryTriggerInteraction.Ignore))if(ringSet.Contains(h.collider)&&(float.IsNaN(best)||h.point.y>best))best=h.point.y;return best;}
   bool Beyond(Vector3 p){foreach(var h in Physics.RaycastAll(new Vector3(p.x,p.y+30,p.z),Vector3.down,60,~0,QueryTriggerInteraction.Ignore))if(!ringSet.Contains(h.collider)&&!h.collider.transform.IsChildOf(host.transform)&&!h.collider.attachedRigidbody&&!h.collider.isTrigger&&(h.collider is MeshCollider||h.collider is TerrainCollider))return true;return false;}
   var skirt=new List<(Vector3 a,Vector3 b,Vector3 o,float drop,Color ca,Color cb)>();var bins=new Dictionary<(int,int),(int n,float gap)>();
   foreach(Transform t in loop){var mf=t.GetComponent<MeshFilter>();if(!mf||!t.name.StartsWith("Ground_")||!mf.sharedMesh)continue;var mesh=mf.sharedMesh;var v=mesh.vertices;var tr=mesh.triangles;var cols=mesh.colors;var m=t.localToWorldMatrix;
    var tiled=tiles.FirstOrDefault(x=>x.mf==mf);if(tiled.mf)v=tiled.v; // the lake tiles as they will be
    var count=new Dictionary<(int,int),(int n,int third)>();
    for(int i=0;i<tr.Length;i+=3)for(int k=0;k<3;k++){int a=tr[i+k],b=tr[i+(k+1)%3];var key=a<b?(a,b):(b,a);count[key]=count.TryGetValue(key,out var e)?(e.n+1,e.third):(1,tr[i+(k+2)%3]);}
    foreach(var e in count){if(e.Value.n!=1)continue;var a=m.MultiplyPoint3x4(v[e.Key.Item1]);var b=m.MultiplyPoint3x4(v[e.Key.Item2]);var third=m.MultiplyPoint3x4(v[e.Value.third]);
     var mid=(a+b)*.5f;var along=b-a;along.y=0;var o=Vector3.Cross(Vector3.up,along).normalized;if(Vector3.Dot(o,third-mid)>0)o=-o;
     if(Beyond(mid+o*.6f))continue; // another ground piece continues past it
     float ry=RingY(mid+o*1.5f);float top=Mathf.Max(a.y,b.y);float gap=float.IsNaN(ry)?99:top-ry;if(gap<=.6f)continue;
     float drop=Mathf.Clamp((float.IsNaN(ry)?25:gap+3),4,45);
     Color ca=cols!=null&&cols.Length==v.Length?cols[e.Key.Item1]:new Color(.33f,.43f,.2f),cb=cols!=null&&cols.Length==v.Length?cols[e.Key.Item2]:new Color(.33f,.43f,.2f);
     skirt.Add((a,b,o,drop,ca,cb));var key2=((int)Mathf.Floor(mid.x/100)*100,(int)Mathf.Floor(mid.z/100)*100);var bb=bins.TryGetValue(key2,out var q)?q:(0,0f);bins[key2]=(bb.Item1+1,Mathf.Max(bb.Item2,Mathf.Min(gap,99)));}}
   UnityEngine.Object.DestroyImmediate(ring);UnityEngine.Object.DestroyImmediate(host);
   log.Add($"E1 world edge: {skirt.Count} open ground edges stand more than 0.6 m above the edge ring (or have none) and get a skirt; by 100 m square (x,z: edges, largest gap):");
   foreach(var kv in bins.OrderBy(k=>k.Key.Item1).ThenBy(k=>k.Key.Item2))log.Add($"   {kv.Key.Item1},{kv.Key.Item2}: {kv.Value.n} edges, gap up to {kv.Value.gap:F1} m");
   // ---- E3: floating trunk colliders
   string OnRoute(Vector3 p){foreach(var r in roads){float s=r.Project(p,out float lat);if(lat<r.HalfWidth(s)+1.5f&&Mathf.Abs(r.At(s,out _).y-p.y)<3)return $"{r.name} s {s:F0}";}foreach(var w in trails){float s=w.Project(p,out float lat);if(lat<w.halfWidth+1.5f&&Mathf.Abs(w.At(s,out _).y-p.y)<3)return $"{w.title} s {s:F0}";}return null;}
   var seat=new List<(Collider c,float gap,string route)>();
   foreach(var c in roots.SelectMany(g=>g.GetComponentsInChildren<Collider>(true)).Where(c=>(c is BoxCollider||c is CapsuleCollider)&&!c.isTrigger&&c.enabled&&c.gameObject.activeInHierarchy&&c.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0)){
    if(objects.Any(o=>o.what=="remove"&&c.transform.IsChildOf(o.g.transform)))continue;var b=c.bounds;var bottom=new Vector3(b.center.x,b.min.y,b.center.z);
    var hits=Physics.RaycastAll(bottom+Vector3.up*.5f,Vector3.down,80,~0,QueryTriggerInteraction.Ignore).Where(h=>!TreeLike(h.collider.name)&&!h.collider.attachedRigidbody).OrderBy(h=>h.distance).ToArray();if(hits.Length==0)continue;
    float gap=bottom.y-hits[0].point.y-Delta(bottom.x,bottom.z);if(gap<=.3f)continue;seat.Add((c,gap,OnRoute(hits[0].point)));}
   log.Add($"E3 trunks: {seat.Count} trunk colliders float more than 0.3 m: {seat.Count(s=>s.route==null)} set down, {seat.Count(s=>s.route!=null)} removed (they would stand on a road or trail)");
   foreach(var s in seat)log.Add($"   {(s.route==null?"seat":"remove")} {P(s.c.transform)} base {V(new Vector3(s.c.bounds.center.x,s.c.bounds.min.y,s.c.bounds.center.z))} down {s.gap:F2} m{(s.route!=null?" (over "+s.route+")":"")}");
   if(Dry){Write(log);EditorApplication.Exit(0);return;}
   // ---- apply
   Directory.CreateDirectory(AssetDir);
   foreach(var (mf,v) in tiles){
    var src=mf.sharedMesh;var copy=UnityEngine.Object.Instantiate(src);copy.name="FreeRoamWorld-lake-"+mf.name;copy.vertices=v;
    // normals of the raised vertices from the new ground around them (seamless across tiles); the rest unchanged
    var n=src.normals;var m=mf.transform.localToWorldMatrix;var inv=mf.transform.worldToLocalMatrix;
    for(int i=0;i<v.Length;i++){var w=m.MultiplyPoint3x4(v[i]);if(!delta.ContainsKey(K(w.x,w.z))&&!Near(delta,w))continue;
     float hx=Height(newY,w.x+2,w.z)-Height(newY,w.x-2,w.z),hz=Height(newY,w.x,w.z+2)-Height(newY,w.x,w.z-2);if(float.IsNaN(hx)||float.IsNaN(hz))continue;n[i]=inv.MultiplyVector(new Vector3(-hx/4,1,-hz/4).normalized).normalized;}
    copy.normals=n;copy.RecalculateBounds();string path=$"{AssetDir}/{copy.name}.asset";AssetDatabase.CreateAsset(copy,path);
    mf.sharedMesh=copy;var mc=mf.GetComponent<MeshCollider>();if(mc){mc.sharedMesh=null;mc.sharedMesh=copy;}EditorUtility.SetDirty(mf);if(mc)EditorUtility.SetDirty(mc);}
   foreach(var (mf,mesh,_,_) in vegChanges){AssetDatabase.CreateAsset(mesh,$"{AssetDir}/{San(mesh.name)}.asset");mf.sharedMesh=mesh;EditorUtility.SetDirty(mf);}
   foreach(var o in objects){if(!o.g)continue;if(o.what=="remove")UnityEngine.Object.DestroyImmediate(o.g);else if(o.what=="move"){o.g.transform.position+=Vector3.up*o.d;EditorUtility.SetDirty(o.g.transform);}}
   var lakeRoot=new GameObject("0.95 Free Roam lake");UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(lakeRoot,scene);
   var lake=new GameObject("Friend's lake - visible shoreline",typeof(MeshFilter),typeof(MeshRenderer));lake.transform.SetParent(lakeRoot.transform,false);lake.transform.SetPositionAndRotation(lakePos,lakeRot);lake.transform.localScale=lakeScale;
   lake.GetComponent<MeshFilter>().sharedMesh=lakeMesh;lake.GetComponent<MeshRenderer>().sharedMaterial=lakeMat;lake.GetComponent<MeshRenderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;lake.AddComponent<Racer.ShallowWater>().round=lakeRound;
   // skirts, in chunks of about 80 m so no one piece counts as a big ground piece for the world edge
   var sroot=new GameObject("0.95 World edge seal");UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(sroot,scene);
   var mat=loop.GetComponentsInChildren<MeshRenderer>().First(r=>r.name.StartsWith("Ground_")).sharedMaterial;int chunkNo=0;
   foreach(var group in skirt.GroupBy(s=>((int)Mathf.Floor((s.a.x+s.b.x)*.5f/80),(int)Mathf.Floor((s.a.z+s.b.z)*.5f/80)))){
    var vs=new List<Vector3>();var cs=new List<Color>();var ts=new List<int>();
    foreach(var s in group){var down=Vector3.down*s.drop;var a2=s.a+s.o*Mathf.Min(6,s.drop*.25f)+down;var b2=s.b+s.o*Mathf.Min(6,s.drop*.25f)+down;int i=vs.Count;
     var earth=new Color(.36f,.30f,.21f);vs.AddRange(new[]{s.a+Vector3.down*.05f,s.b+Vector3.down*.05f,b2,a2});cs.AddRange(new[]{Color.Lerp(s.ca,earth,.35f),Color.Lerp(s.cb,earth,.35f),earth,earth});
     ts.AddRange(new[]{i,i+1,i+2,i,i+2,i+3,i,i+2,i+1,i,i+3,i+2});}
    var mesh=new Mesh{name=$"FreeRoamWorld world edge seal {chunkNo}"};mesh.SetVertices(vs);mesh.SetColors(cs);mesh.SetTriangles(ts,0);mesh.SetUVs(1,Enumerable.Repeat(Vector4.zero,vs.Count).ToList());mesh.RecalculateNormals();mesh.RecalculateBounds();
    AssetDatabase.CreateAsset(mesh,$"{AssetDir}/{mesh.name}.asset");
    var go=new GameObject("Ground_Edge seal "+chunkNo,typeof(MeshFilter),typeof(MeshRenderer),typeof(MeshCollider));go.transform.SetParent(sroot.transform,false);go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<MeshRenderer>().sharedMaterial=mat;go.GetComponent<MeshCollider>().sharedMesh=mesh;go.isStatic=true;chunkNo++;}
   foreach(var s in seat){if(!s.c)continue;if(s.route!=null){var g=s.c.gameObject;if(g.GetComponents<Component>().Length==2&&g.transform.childCount==0)UnityEngine.Object.DestroyImmediate(g);else UnityEngine.Object.DestroyImmediate(s.c);}else{s.c.transform.position-=Vector3.up*s.gap;EditorUtility.SetDirty(s.c.transform);}}
   EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
   log.Add($"APPLIED: {tiles.Count} tile meshes, {vegChanges.Count} vegetation meshes, {objects.Count(o=>o.what=="remove")} trees removed, {objects.Count(o=>o.what=="move")} things moved, lake added, {chunkNo} seal pieces, {seat.Count} trunks");
   Write(log);EditorApplication.Exit(0);
  }catch(Exception e){log.Add("FAILED "+e);Write(log);EditorApplication.Exit(1);}
 }
 // D: the J1 flowing creek (Forest scenes): Free Roam has no channel for it (its ground there is 77-81 m, the creek 73.9 m).
 // The Forest scenes' channel is cut into the Free Roam ground (lower only: inside the creek's footprint + 3 m in full,
 // tapering to nothing 10 m further; never under a road or trail within its width + 3 m, tapering over 6 m more); the
 // creek water goes in. Trees in the water go; anything standing on lowered ground is set down with it.
 public static void Creek(){
  var log=new List<string>();
  try{
   EditorSceneManager.OpenScene(Forest);var fc=GameObject.Find("J1 flowing creek");var ct=fc.transform;var cPos=ct.position;var cRot=ct.rotation;var cScale=ct.lossyScale;var cMesh=fc.GetComponent<MeshFilter>().sharedMesh;var cMat=fc.GetComponent<MeshRenderer>().sharedMaterial;var cw=fc.GetComponent<Racer.ShallowWater>();bool cRound=cw.round;float surface=cw.Surface;
   var w2l=ct.worldToLocalMatrix;float Outside(Vector3 w){var l=w2l.MultiplyPoint3x4(w);float dx=Mathf.Max(0,Mathf.Abs(l.x)-.5f)*cScale.x,dz=Mathf.Max(0,Mathf.Abs(l.z)-.5f)*cScale.z;return Mathf.Sqrt(dx*dx+dz*dz);}
   var forest=new Dictionary<(int,int),float>();
   foreach(Transform t in GameObject.Find("Memory loop - north is +Z").transform){var mf=t.GetComponent<MeshFilter>();if(!mf||!t.name.StartsWith("Ground_")||!mf.sharedMesh)continue;var m=t.localToWorldMatrix;
    foreach(var v in mf.sharedMesh.vertices){var w=m.MultiplyPoint3x4(v);if(Outside(w)<16)forest[K(w.x,w.z)]=w.y;}}
   log.Add($"Forest creek {V(cPos)} scale {V(cScale)} surface {surface:F2}; {forest.Count} forest heights within 16 m");
   var scene=EditorSceneManager.OpenScene(Scene);Physics.SyncTransforms();var roots=scene.GetRootGameObjects();
   var lanes=new List<(Vector3 p,float hw)>();
   foreach(var r in roots.SelectMany(g=>g.GetComponentsInChildren<Racer.RaceRoad>(true)).Where(r=>r.points!=null&&r.points.Length>1)){r.Initialize();for(float s=0;s<r.Length;s+=1.5f){var p=r.At(s,out _);if(Outside(p)<40)lanes.Add((p,r.HalfWidth(s)));}}
   foreach(var w in roots.SelectMany(g=>g.GetComponentsInChildren<Racer.WoodlandRoute>(true)).Where(w=>w.points!=null&&w.points.Length>1)){w.Initialize();for(float s=0;s<w.Length;s+=1.5f){var p=w.At(s,out _);if(Outside(p)<40)lanes.Add((p,w.halfWidth));}}
   log.Add($"{lanes.Count} road/trail samples within 40 m of the creek");
   float Weight(Vector3 w){float o=Outside(w);float k=o<=3?1:o>=13?0:1-Mathf.SmoothStep(0,1,(o-3)/10);if(k<=0)return 0;foreach(var (p,hw) in lanes){float d=Flat(w,p);if(Mathf.Abs(w.y-p.y)>6)continue;float band=hw+3;if(d<band)return 0;if(d<band+6)k=Mathf.Min(k,(d-band)/6);}return k;}
   var loop=roots.First(g=>g.name=="Memory loop - north is +Z").transform;var delta=new Dictionary<(int,int),float>();var oldY=new Dictionary<(int,int),float>();var tiles=new List<(MeshFilter mf,Vector3[] v)>();
   foreach(Transform t in loop){var mf=t.GetComponent<MeshFilter>();if(!mf||!t.name.StartsWith("Ground_")||!mf.sharedMesh)continue;var m=t.localToWorldMatrix;var inv=t.worldToLocalMatrix;var v=mf.sharedMesh.vertices;bool any=false;
    for(int i=0;i<v.Length;i++){var w=m.MultiplyPoint3x4(v[i]);if(Outside(w)>16)continue;oldY[K(w.x,w.z)]=w.y;if(!forest.TryGetValue(K(w.x,w.z),out var fy))continue;float down=w.y-fy;if(down<=.02f)continue;float k=Weight(w);if(k<=0)continue;
     float d=-down*k;v[i]=inv.MultiplyPoint3x4(new Vector3(w.x,w.y+d,w.z));delta[K(w.x,w.z)]=d;any=true;}
    if(any)tiles.Add((mf,v));}
   float Delta(float x,float z){float gx=Mathf.Floor(x/2)*2,gz=Mathf.Floor(z/2)*2,u=(x-gx)/2,w=(z-gz)/2;float D(float a,float b)=>delta.TryGetValue(K(a,b),out var q)?q:0;return Mathf.Lerp(Mathf.Lerp(D(gx,gz),D(gx+2,gz),u),Mathf.Lerp(D(gx,gz+2),D(gx+2,gz+2),u),w);}
   log.Add($"creek channel: {delta.Count} ground vertices lowered (deepest {(delta.Count>0?-delta.Values.Min():0):F2} m) in {tiles.Count} tiles: {string.Join(", ",tiles.Select(t=>t.mf.name+" ("+AssetDatabase.GetAssetPath(t.mf.sharedMesh)+")"))}");
   int wet=0,dry=0;foreach(var k in oldY.Keys){var w=new Vector3(k.Item1/10f,0,k.Item2/10f);if(Outside(w)>0)continue;float y=oldY[k]+(delta.TryGetValue(k,out var q)?q:0);if(y<surface)wet++;else dry++;}log.Add($"   inside the footprint after: {wet} vertices under the water, {dry} above it");
   var veg=roots.SelectMany(g=>g.GetComponentsInChildren<MeshRenderer>(true)).Where(r=>r.enabled&&r.gameObject.activeInHierarchy&&(Racer.SceneryTrees.IsOldVegetation(r)||Mountain(r))).ToList();var vegChanges=new List<(MeshFilter mf,Mesh mesh)>();
   foreach(var r in veg){if(r.bounds.SqrDistance(cPos)>80*80)continue;var mf=r.GetComponent<MeshFilter>();var mesh=mf.sharedMesh;var v=mesh.vertices;var t=mesh.triangles;var m=r.transform.localToWorldMatrix;var inv=r.transform.worldToLocalMatrix;
    var parent=Enumerable.Range(0,v.Length).ToArray();int Find(int i){while(parent[i]!=i){parent[i]=parent[parent[i]];i=parent[i];}return i;}
    for(int i=0;i<t.Length;i+=3){int a=Find(t[i]),b=Find(t[i+1]),c=Find(t[i+2]);parent[b]=a;parent[Find(c)]=a;}
    var drop=new HashSet<int>();int removed=0,moved=0;var nv=(Vector3[])v.Clone();
    foreach(var piece in Enumerable.Range(0,v.Length).GroupBy(Find)){var c=piece.Select(i=>m.MultiplyPoint3x4(v[i])).Aggregate(Vector3.zero,(x,y)=>x+y)/piece.Count();float d=Delta(c.x,c.z);if(d>-.02f)continue;
     if(Outside(c)<=0){foreach(var i in piece)drop.Add(i);removed++;}else{foreach(var i in piece)nv[i]=inv.MultiplyPoint3x4(m.MultiplyPoint3x4(v[i])+Vector3.up*d);moved++;}}
    if(removed+moved==0)continue;var copy=UnityEngine.Object.Instantiate(mesh);copy.name="FreeRoamWorld-creek-vegetation-"+vegChanges.Count+"-"+Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(mesh));copy.vertices=nv;var tri=new List<int>();for(int i=0;i<t.Length;i+=3)if(!drop.Contains(t[i]))tri.AddRange(new[]{t[i],t[i+1],t[i+2]});copy.SetTriangles(tri,0);copy.RecalculateBounds();
    vegChanges.Add((mf,copy));log.Add($"   vegetation {P(r.transform)}: {removed} pieces in the water taken out, {moved} set down with the ground");}
   var objects=new List<(GameObject g,float d,string what)>();var done=new HashSet<Transform>();
   foreach(var leaf in roots.SelectMany(g=>g.GetComponentsInChildren<Transform>(true))){
    if(leaf.IsChildOf(loop)||leaf.GetComponent<Racer.RaceRoad>()||leaf.GetComponent<Racer.WoodlandRoute>())continue;if(!leaf.GetComponent<Renderer>()&&!leaf.GetComponent<Collider>())continue;if(leaf.GetComponent<Renderer>() is MeshRenderer lr&&veg.Contains(lr))continue;
    var c=leaf;var gp=leaf.parent;if(gp&&gp.parent){var rs=gp.GetComponentsInChildren<Renderer>(true);if(rs.Length>0&&rs.Length<12){var gb=rs[0].bounds;foreach(var x in rs)gb.Encapsulate(x.bounds);if(gb.size.magnitude<12)c=gp;}}
    if(!done.Add(c))continue;Bounds b;{var rs=c.GetComponentsInChildren<Renderer>(true);var cs=c.GetComponentsInChildren<Collider>(true);if(rs.Length>0){b=rs[0].bounds;foreach(var x in rs)b.Encapsulate(x.bounds);}else if(cs.Length>0){b=cs[0].bounds;foreach(var x in cs)b.Encapsulate(x.bounds);}else continue;}
    if(b.size.magnitude>40||Outside(b.center)>14)continue;float d=Delta(b.center.x,b.center.z);if(d>-.02f)continue;float og=OldY(oldY,b.center.x,b.center.z);bool rested=Mathf.Abs(b.min.y-og)<.5f;if(!rested)continue;
    objects.Add((c.gameObject,d,TreeLike(c.name)&&Outside(b.center)<=0?"remove":"move"));}
   foreach(var o in objects)log.Add($"   {o.what} {P(o.g.transform)} at {V(o.g.transform.position)} (ground lowered {-o.d:F2} m)");
   if(Dry){Write2(log);EditorApplication.Exit(0);return;}
   Directory.CreateDirectory(AssetDir);
   foreach(var (mf,v) in tiles){var src=mf.sharedMesh;var copy=UnityEngine.Object.Instantiate(src);copy.name="FreeRoamWorld-creek-"+mf.name;copy.vertices=v;copy.RecalculateNormals();
    var n0=src.normals;var n1=copy.normals;var m=mf.transform.localToWorldMatrix;for(int i=0;i<v.Length;i++){var w=m.MultiplyPoint3x4(v[i]);if(!delta.ContainsKey(K(w.x,w.z))&&!Near(delta,w))n1[i]=n0[i];}copy.normals=n1;copy.RecalculateBounds();
    AssetDatabase.CreateAsset(copy,$"{AssetDir}/{copy.name}.asset");mf.sharedMesh=copy;var mc=mf.GetComponent<MeshCollider>();if(mc){mc.sharedMesh=null;mc.sharedMesh=copy;EditorUtility.SetDirty(mc);}EditorUtility.SetDirty(mf);}
   foreach(var (mf,mesh) in vegChanges){AssetDatabase.CreateAsset(mesh,$"{AssetDir}/{San(mesh.name)}.asset");mf.sharedMesh=mesh;EditorUtility.SetDirty(mf);}
   foreach(var o in objects){if(!o.g)continue;if(o.what=="remove")UnityEngine.Object.DestroyImmediate(o.g);else{o.g.transform.position+=Vector3.up*o.d;EditorUtility.SetDirty(o.g.transform);}}
   var root=GameObject.Find("0.95 Free Roam lake");var go=new GameObject("J1 flowing creek",typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(root.transform,false);go.transform.SetPositionAndRotation(cPos,cRot);go.transform.localScale=cScale;
   go.GetComponent<MeshFilter>().sharedMesh=cMesh;go.GetComponent<MeshRenderer>().sharedMaterial=cMat;go.GetComponent<MeshRenderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;go.AddComponent<Racer.ShallowWater>().round=cRound;
   EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();log.Add($"APPLIED: {tiles.Count} tiles, {vegChanges.Count} vegetation meshes, {objects.Count} things, creek added");Write2(log);EditorApplication.Exit(0);
  }catch(Exception e){log.Add("FAILED "+e);Write2(log);EditorApplication.Exit(1);}
 }
 static float OldY(Dictionary<(int,int),float> y,float x,float z){var h=Height(y,x,z);return float.IsNaN(h)?(y.TryGetValue(K(Mathf.Round(x/2)*2,Mathf.Round(z/2)*2),out var q)?q:0):h;}
 static void Write2(List<string> log){Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/creek.txt",log);}
 static bool Near(Dictionary<(int,int),float> d,Vector3 w){for(int a=-1;a<=1;a++)for(int b=-1;b<=1;b++)if(d.ContainsKey(K(w.x+a*2,w.z+b*2)))return true;return false;}
 static float Height(Dictionary<(int,int),float> y,float x,float z){float gx=Mathf.Floor(x/2)*2,gz=Mathf.Floor(z/2)*2,u=(x-gx)/2,w=(z-gz)/2;
  if(!y.TryGetValue(K(gx,gz),out var a)||!y.TryGetValue(K(gx+2,gz),out var b)||!y.TryGetValue(K(gx,gz+2),out var c)||!y.TryGetValue(K(gx+2,gz+2),out var d))return float.NaN;return Mathf.Lerp(Mathf.Lerp(a,b,u),Mathf.Lerp(c,d,u),w);}
 static bool Mountain(Renderer r)=>r.transform.parent&&r.transform.parent.name.StartsWith("Mountain woods")&&(r.name=="Batched Mountain foliage"||r.name=="Batched Weathered timber");
 static string San(string n)=>new string(n.Select(c=>char.IsLetterOrDigit(c)||c==' '||c=='-'||c=='('||c==')'?c:'_').ToArray());
 static void Write(List<string> log){Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/world.txt",log);}
}
