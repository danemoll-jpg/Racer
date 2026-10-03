using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
// 0.71 local changes (PROJECT_TODO "CURRENT" Parts A-B). Every change is logged to Docs/Report071/author-notes.txt.
// AUTHOR_PARTS selects parts, AUTHOR_SCENES the scenes, AUTHOR_DRY=1 reports without saving.
public static class Report071Author {
 const string Folder="Assets/Track/Report071";
 static readonly List<string> notes=new();
 static void Note(string s){notes.Add(s);Debug.Log("REPORT071 "+s);}
 static string Scene=>UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
 static string Path(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n.Replace('\n','/');}
 static Mesh Store(string name,Mesh mesh){Directory.CreateDirectory(Folder);string path=$"{Folder}/{Scene}-{name}.asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(old){EditorUtility.CopySerialized(mesh,old);Object.DestroyImmediate(mesh);return old;}AssetDatabase.CreateAsset(mesh,path);return mesh;}
 static void Save(){var s=UnityEngine.SceneManagement.SceneManager.GetActiveScene();EditorSceneManager.MarkSceneDirty(s);EditorSceneManager.SaveScene(s);AssetDatabase.SaveAssets();}
 static void Refresh(MeshFilter mf){if(mf.TryGetComponent<MeshCollider>(out var mc)){mc.sharedMesh=null;mc.sharedMesh=mf.sharedMesh;}EditorUtility.SetDirty(mf.sharedMesh);Physics.SyncTransforms();}
 // A mesh asset also used by another scene is cloned for this scene before it is edited (the other scene is unchanged).
 static Dictionary<string,HashSet<string>> deps;
 static Mesh Own(MeshFilter mf){var m=mf.sharedMesh;var path=AssetDatabase.GetAssetPath(m);
  if(deps==null){deps=new();foreach(var sc in AssetDatabase.FindAssets("t:Scene",new[]{"Assets/Scenes"}).Select(AssetDatabase.GUIDToAssetPath))deps[System.IO.Path.GetFileNameWithoutExtension(sc)]=new HashSet<string>(AssetDatabase.GetDependencies(sc,false));}
  var users=deps.Where(kv=>kv.Key!=Scene&&kv.Value.Contains(path)).Select(kv=>kv.Key).ToArray();if(users.Length==0)return m;
  var copy=Object.Instantiate(m);copy.name=m.name;var own=Store("own-"+System.IO.Path.GetFileNameWithoutExtension(path),copy);mf.sharedMesh=own;if(mf.TryGetComponent<MeshCollider>(out var mc)&&mc.sharedMesh==m)mc.sharedMesh=own;
  Note($"{mf.name}: mesh {path} is shared with {string.Join(",",users)}; this scene now uses its own copy {AssetDatabase.GetAssetPath(own)}");return own;}

 public static void Run(){
  if(Application.isPlaying)throw new Exception("Edit mode required");Directory.CreateDirectory("Docs/Report071");Physics.queriesHitBackfaces=false;
  var parts=(Environment.GetEnvironmentVariable("AUTHOR_PARTS")??"all").Split(',');bool Do(string p)=>parts.Contains("all")||parts.Contains(p);
  var scenes=(Environment.GetEnvironmentVariable("AUTHOR_SCENES")??"StreetLoopGreybox").Split(',');bool dry=Environment.GetEnvironmentVariable("AUTHOR_DRY")=="1";
  try{
   foreach(var scene in scenes){
    EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();deps=null;Note("SCENE "+scene+(dry?" (dry run, not saved)":""));
    if(Do("a"))PartA(dry);
    if(Do("aribbon"))RibbonGround(dry);
    if(scene=="MountainLoop"&&Do("b12"))Junction("BUG-001/002 Climbing Ridge Cut entrance",new[]{new Vector4(757,0,-117,15),new Vector4(777,0,-101,10),new Vector4(771,0,-111,6)},dry);
    if(scene=="MountainLoop"&&Do("b3"))Junction("BUG-003 Climbing Ridge Cut rejoin pit",new[]{new Vector4(1012,0,-26,17),new Vector4(1028,0,-37,14),new Vector4(1000,0,-38,10),new Vector4(1033,0,-16,12),new Vector4(1034,0,-5,10)},dry);
    if(!dry)Save();}
  }finally{File.AppendAllLines("Docs/Report071/author-notes.txt",notes);}
  EditorApplication.Exit(0);}

 // ---------- Part A: Free Roam Summit Homeward Flight - the raised catch mound becomes natural open ground ----------
 // The launch frame: CR094 summit launch (s = forward along the flight, x = right). CR-103 (752ca2f1) built the catch as
 // terrain lifted to LandingHeight(s) over s 320-540, |x| < 43 (blended), plus a 48 m-wide collidable landing sheet. The
 // sheet is removed and every terrain vertex of the mountain tiles inside s 316-545, |x| <= 50 is replaced by the smooth
 // (harmonic) surface spanning the untouched ground around that box: the surrounding terrain level, no mound, no cliff.
 // Approach, run-up, lip and site (s <= 220) are outside the box and unchanged. Dependents follow the new ground: trees
 // (trunk collider + batched pieces), the RIDGE RETURN sign, the supported-return ribbon and the route points in the box.
 const float S0=316,S1=545,XW=50,CW=22,C0=300,C1=690;
 static bool InCorridor(Vector3 w){var q=launch.InverseTransformPoint(w);return q.z>=C0&&q.z<=C1&&Mathf.Abs(q.x)<=CW;}
 static Transform launch;
 static bool InBox(Vector3 w,float pad=0){var q=launch.InverseTransformPoint(w);return q.z>=S0-pad&&q.z<=S1+pad&&Mathf.Abs(q.x)<=XW+pad;}
 static MeshCollider[] tileColliders;
 static bool Tile(Collider c)=>c is MeshCollider m&&tileColliders.Contains(m);
 static bool TopOf(Vector3 p,Func<Collider,bool> ok,out RaycastHit hit){var o=new Vector3(p.x,600,p.z);hit=default;for(int k=0;k<30&&Physics.Raycast(o,Vector3.down,out var h,1200,~0,QueryTriggerInteraction.Ignore);k++){if(h.normal.y>0&&ok(h.collider)){hit=h;return true;}o=h.point+Vector3.down*.02f;}return false;}
 static void PartA(bool dry){
  launch=Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).FirstOrDefault(t=>t.name=="CR094 summit launch");
  var landing=launch?launch.Find("Ground_CR103 smooth landing"):null;
  if(!landing||!landing.gameObject.activeInHierarchy){Note($"Part A {Scene}: catch landing {(landing?"inactive":"absent")}; scene not changed");return;}
  var tilesRoot=GameObject.Find("Memory loop - north is +Z");
  var lo=launch.TransformPoint(new Vector3(-XW,0,S0));var corners=new[]{new Vector3(-XW,0,S0),new Vector3(XW,0,S0),new Vector3(-XW,0,S1),new Vector3(XW,0,S1)}.Select(c=>launch.TransformPoint(c)).ToArray();
  var box=new Bounds(corners[0],Vector3.zero);foreach(var c in corners)box.Encapsulate(c);box.Expand(new Vector3(4,2000,4));
  var mfs=tilesRoot.GetComponentsInChildren<MeshFilter>().Where(m=>m.sharedMesh&&m.GetComponent<MeshCollider>()&&m.GetComponent<Renderer>()&&m.GetComponent<Renderer>().bounds.Intersects(box)).ToArray();
  tileColliders=tilesRoot.GetComponentsInChildren<MeshCollider>();
  Note($"Part A {Scene}: terrain tiles touching the box: {string.Join(", ",mfs.Select(m=>m.name+" ("+AssetDatabase.GetAssetPath(m.sharedMesh).Replace("Assets/Track/","")+")"))}");
  // Everything else standing in or over the box (for the record and the dependency pass).
  var others=Object.FindObjectsByType<Collider>(FindObjectsSortMode.None).Where(c=>!c.isTrigger&&!Tile(c)&&c.transform!=landing&&InBox(c.bounds.center,2)&&c.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)<0).ToArray();
  foreach(var c in others)Note($"Part A {Scene}: non-terrain collider in the box: {Path(c.transform)} centre {c.bounds.center:F1} size {c.bounds.size:F1}");
  // Old ground (terrain only) at trunks, objects and route points, read before any change.
  var lc=landing.GetComponent<Collider>();
  float OldTop(Vector3 p)=>TopOf(p,c=>Tile(c)||c==lc,out var h)?h.point.y:float.NaN;
  // Graph over the tile vertices (vertices at the same x,z on adjacent tiles are one node).
  (int,int) K(Vector3 p)=>(Mathf.RoundToInt(p.x*50),Mathf.RoundToInt(p.z*50));
  var y=new Dictionary<(int,int),float>();var pos=new Dictionary<(int,int),Vector3>();var nb=new Dictionary<(int,int),HashSet<(int,int)>>();var inBox=new HashSet<(int,int)>();var world=new Dictionary<MeshFilter,Vector3[]>();
  foreach(var mf in mfs){var w=mf.sharedMesh.vertices.Select(v=>mf.transform.TransformPoint(v)).ToArray();world[mf]=w;var t=mf.sharedMesh.triangles;
   for(int i=0;i<w.Length;i++){var k=K(w[i]);if(!pos.ContainsKey(k)){pos[k]=w[i];y[k]=w[i].y;}else y[k]=Mathf.Max(y[k],w[i].y);if(InBox(w[i]))inBox.Add(k);}
   for(int i=0;i<t.Length;i+=3)for(int e=0;e<3;e++){var a=K(w[t[i+e]]);var b=K(w[t[i+(e+1)%3]]);if(a.Equals(b))continue;if(!nb.TryGetValue(a,out var na))nb[a]=na=new();na.Add(b);if(!nb.TryGetValue(b,out var nbb))nb[b]=nbb=new();nbb.Add(a);}}
  var free=inBox.Where(k=>nb.TryGetValue(k,out var n)&&n.All(o=>inBox.Contains(o))).ToList();var ny=new Dictionary<(int,int),float>(y);
  int iters=0;for(int it=0;it<40000;it++){iters=it;float md=0;foreach(var k in free){float sum=0;int c=0;foreach(var o in nb[k]){sum+=ny[o];c++;}float v=sum/c;md=Mathf.Max(md,Mathf.Abs(v-ny[k]));ny[k]=v;}if(md<1e-5f&&it>200)break;}
  float maxUp=float.MinValue,maxDown=float.MaxValue;foreach(var k in free){float d=ny[k]-y[k];maxUp=Mathf.Max(maxUp,d);maxDown=Mathf.Min(maxDown,d);}
  Note($"Part A {Scene}: {inBox.Count} terrain nodes in the box, {free.Count} replaced by the smooth surface ({inBox.Count-free.Count} rim nodes keep their height); {iters} iterations; ground change {maxDown:+0.0;-0.0} .. {maxUp:+0.0;-0.0} m");
  // Section report of the new ground (launch frame), for the record.
  float NewAt(Vector3 w){// nearest-node interpolation is enough for the report; the mesh is used for everything else
   var q=K(w);float best=1e9f,val=float.NaN;foreach(var k in inBox){var p=pos[k];float d=(p.x-w.x)*(p.x-w.x)+(p.z-w.z)*(p.z-w.z);if(d<best){best=d;val=ny[k];}}return val;}
  for(float s=S0;s<=S1;s+=20)Note($"Part A {Scene}: section s {s}: "+string.Join("  ",new[]{-40f,-20,0,20,40}.Select(x=>{var w=launch.TransformPoint(new Vector3(x,0,s));return $"x{x}: {OldTop(w):F1} -> {NewAt(w):F1}";})));
  // Trees: trunk colliders in the box (and 2 m around), with their old ground.
  var trunks=Object.FindObjectsByType<Collider>(FindObjectsSortMode.None).Where(c=>!c.isTrigger&&c.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0&&c.bounds.size.x<3&&InBox(c.bounds.center,2)).ToArray();
  var oldG=trunks.ToDictionary(c=>c,c=>OldTop(c.bounds.center));var tileSet=new HashSet<MeshFilter>(mfs);
  // Signs / grounded objects in the box (CR102 summit signs) with their old ground under their base.
  var signs=Object.FindObjectsByType<PhysicalSign>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(s=>InBox(s.transform.position,2)).ToArray();
  var signOld=signs.ToDictionary(s=>s,s=>OldTop(s.transform.position));
  // Routes: the activity's safe-return route (points on the old catch profile) and the exploration trail.
  var routes=Object.FindObjectsByType<RaceRoad>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(r=>r.points!=null&&r.points.Any(p=>InBox(p))).ToArray();
  foreach(var r in routes){var outside=r.points.Where(p=>!InBox(p,6)&&InBox(p,40)).Select(p=>p.y-OldTop(p)).Where(float.IsFinite).OrderBy(v=>v).ToArray();float off=outside.Length>0?outside[outside.Length/2]:0;
   Note($"Part A {Scene}: route '{Path(r.transform)}': {r.points.Count(p=>InBox(p))} points in the box; median height above the ground just outside it {off:+0.00;-0.00} m");}
  // Visual-only objects in the box (no collider): listed for the dependency pass.
  foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None)){var mf=r.GetComponent<MeshFilter>();if(mf&&tileSet.Contains(mf))continue;if(r.GetComponent<Collider>()||r.transform.IsChildOf(launch))continue;var b=r.bounds;if(b.size.x>60||b.size.z>60||!InBox(b.center,2))continue;
   string n=r.name;if(n.IndexOf("tree",StringComparison.OrdinalIgnoreCase)>=0||n.Contains("canopy")||n.Contains("woodland")||n.Contains("foliage")||n.Contains("timber"))continue;
   Note($"Part A {Scene}: visual object in the box: {Path(r.transform)} base {b.min.y:F1} centre {b.center:F1} size {b.size:F1}; old ground {OldTop(b.center):F1}");}
  // Obstacles along the flight line beyond the old catch (run-out corridor s 300-760, |x| < 35), for the record.
  foreach(var c in Object.FindObjectsByType<Collider>(FindObjectsSortMode.None)){if(c.isTrigger||Tile(c)||c.transform.IsChildOf(launch))continue;var b=c.bounds;if(b.size.x>60||b.size.z>60)continue;var q=launch.InverseTransformPoint(b.center);if(q.z<300||q.z>760||Mathf.Abs(q.x)>35)continue;
   Note($"Part A {Scene}: in the run-out corridor: {Path(c.transform)} local x {q.x:F1} s {q.z:F1} size {b.size:F1}");}
  // Visual-only grounded groups (acorn-clue cairns, the RIDGE RETURN sign): parent of each visual object in the box.
  var groups=new Dictionary<Transform,(float baseY,Vector3 ctr,float oldG)>();
  foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None)){var mf=r.GetComponent<MeshFilter>();if(mf&&tileSet.Contains(mf))continue;if(r.GetComponent<Collider>()||r.transform.IsChildOf(launch)||!r.transform.parent)continue;var b=r.bounds;if(b.size.x>60||b.size.z>60||!InBox(b.center,2))continue;
   string n=r.name;if(n.IndexOf("tree",StringComparison.OrdinalIgnoreCase)>=0||n.Contains("canopy")||n.Contains("woodland")||n.Contains("foliage")||n.Contains("timber"))continue;
   var g=r.transform.parent;if(!(g.name.StartsWith("Acorn clue")||g.parent&&g.parent.name.StartsWith("CR102 summit signs")))continue;
   if(!groups.ContainsKey(g)){var rs=g.GetComponentsInChildren<Renderer>();var bb=rs[0].bounds;foreach(var x in rs)bb.Encapsulate(x.bounds);groups[g]=(bb.min.y,bb.center,OldTop(bb.center));}}
  // Run-out corridor: trees standing on the flight line beyond the lip (|x| <= 22 m, s 300-690) are removed, trunk and pieces.
  var clear=Object.FindObjectsByType<Collider>(FindObjectsSortMode.None).Where(c=>!c.isTrigger&&c.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0&&c.bounds.size.x<3&&InCorridor(c.bounds.center)).ToArray();
  Note($"Part A {Scene}: trees on the run-out corridor (|x| <= {CW} m, s {C0}-{C1}) to remove: {clear.Length}");
  if(dry){foreach(var c in trunks)Note($"Part A {Scene} (dry): trunk {Path(c.transform)} at {c.bounds.center:F1} old ground {oldG[c]:F1} new {NewAt(c.bounds.center):F1}");
   foreach(var r in routes)foreach(var p in r.points.Where(p=>InBox(p)))Note($"Part A {Scene} (dry): route {r.name} point {p:F1} old top {OldTop(p):F1} new ground {NewAt(p):F1}");return;}
  // Apply: remove the landing sheet, write the terrain.
  Note($"Part A {Scene}: removed '{Path(landing)}' (mesh {AssetDatabase.GetAssetPath(landing.GetComponent<MeshFilter>().sharedMesh)} kept on disk for history)");Object.DestroyImmediate(landing.gameObject);
  foreach(var mf in mfs){var w=world[mf];var lv=mf.sharedMesh.vertices;bool any=false;for(int i=0;i<w.Length;i++){var k=K(w[i]);if(!inBox.Contains(k))continue;float v=ny[k];if(Mathf.Abs(v-w[i].y)<1e-4f)continue;var q=w[i];q.y=v;lv[i]=mf.transform.InverseTransformPoint(q);any=true;}
   if(any){var m=Own(mf);m.vertices=lv;m.RecalculateNormals();m.RecalculateBounds();Refresh(mf);}}
  Physics.SyncTransforms();
  float NewTop(Vector3 p)=>TopOf(p,Tile,out var h)?h.point.y:float.NaN;
  // Supported return ribbon: vertices in the box sit 3.5 cm above the new ground (as authored).
  var ret=launch.Find("Ground_CR103 supported return");if(ret){var mf=ret.GetComponent<MeshFilter>();var lv=mf.sharedMesh.vertices;int moved=0;float md=0;
   for(int i=0;i<lv.Length;i++){var w=mf.transform.TransformPoint(lv[i]);if(!InBox(w))continue;float g=NewTop(w);if(!float.IsFinite(g))continue;md=Mathf.Max(md,Mathf.Abs(g+.035f-w.y));w.y=g+.035f;lv[i]=mf.transform.InverseTransformPoint(w);moved++;}
   if(moved>0){var m=Own(mf);m.vertices=lv;m.RecalculateNormals();m.RecalculateBounds();Refresh(mf);}Note($"Part A {Scene}: supported-return ribbon: {moved} vertices in the box re-seated 3.5 cm above the new ground (largest move {md:F1} m)");}
  // Route points in the box follow the new ground (same height above it as the route has just outside the box).
  foreach(var r in routes){var outside=r.points.Where(p=>!InBox(p,6)&&InBox(p,40)).Select(p=>p.y-NewTop(p)).Where(float.IsFinite).OrderBy(v=>v).ToArray();float off=outside.Length>0?outside[outside.Length/2]:0;int n=0;float md=0;
   for(int i=0;i<r.points.Length;i++){var p=r.points[i];if(!InBox(p))continue;float g=NewTop(p);if(!float.IsFinite(g))continue;md=Mathf.Max(md,Mathf.Abs(g+off-p.y));p.y=g+off;r.points[i]=p;n++;}
   r.Initialize();EditorUtility.SetDirty(r);Note($"Part A {Scene}: route '{r.name}': {n} points in the box re-seated {off:+0.00;-0.00} m above the new ground (largest move {md:F1} m)");}
  // Trees: the trunk collider and that tree's pieces in the batched tree meshes move by the ground change under the trunk.
  var runBox=box;foreach(var c in new[]{new Vector3(-CW,0,C1),new Vector3(CW,0,C1),new Vector3(-CW,0,C0),new Vector3(CW,0,C0)})runBox.Encapsulate(launch.TransformPoint(c));
  var batched=Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Where(m=>m.sharedMesh&&!m.GetComponent<Collider>()&&m.GetComponent<Renderer>()&&m.GetComponent<Renderer>().bounds.Intersects(runBox)&&(m.name.IndexOf("tree",StringComparison.OrdinalIgnoreCase)>=0||m.name.Contains("canopy")||m.name.Contains("woodland")||m.name.Contains("foliage")||m.name.Contains("timber"))).ToArray();
  var comps=new List<(MeshFilter mf,int[] verts,Vector3 ctr,float minY)>();
  foreach(var mf in batched){var m=mf.sharedMesh;var t=m.triangles;var par=Enumerable.Range(0,m.vertexCount).ToArray();int F(int a){while(par[a]!=a){par[a]=par[par[a]];a=par[a];}return a;}
   for(int i=0;i<t.Length;i+=3){int a=F(t[i]),b=F(t[i+1]);par[b]=a;int c=F(t[i+2]);par[c]=F(a);}
   var w=m.vertices.Select(v=>mf.transform.TransformPoint(v)).ToArray();var used=new HashSet<int>(t);
   foreach(var g in used.GroupBy(F)){var vs=g.ToArray();var c=Vector3.zero;float mn=1e9f;foreach(int v in vs){c+=w[v];mn=Mathf.Min(mn,w[v].y);}comps.Add((mf,vs,c/vs.Length,mn));}}
  // Remove corridor trees: trunk collider object and that tree's pieces (triangles) in the batched meshes.
  var drop=new Dictionary<MeshFilter,HashSet<int>>();int removedPieces=0;
  foreach(var tr in clear){var b=tr.bounds;foreach(var cp in comps){if(new Vector2(cp.ctr.x-b.center.x,cp.ctr.z-b.center.z).magnitude>2.2f||cp.minY<b.min.y-1.5f||cp.minY>b.min.y+12)continue;if(!drop.TryGetValue(cp.mf,out var set))drop[cp.mf]=set=new();foreach(int v in cp.verts)set.Add(v);removedPieces++;}
   Note($"Part A {Scene}: removed run-out tree '{Path(tr.transform)}' at local x {launch.InverseTransformPoint(b.center).x:F1} s {launch.InverseTransformPoint(b.center).z:F1}");Object.DestroyImmediate(tr.gameObject);}
  foreach(var kv in drop){var mf=kv.Key;var m=Own(mf);var t=m.triangles;var keep=new List<int>();for(int i=0;i<t.Length;i+=3)if(!kv.Value.Contains(t[i]))keep.AddRange(new[]{t[i],t[i+1],t[i+2]});m.SetTriangles(keep,0);m.RecalculateBounds();EditorUtility.SetDirty(m);}
  comps.RemoveAll(cp=>drop.TryGetValue(cp.mf,out var set)&&set.Contains(cp.verts[0]));trunks=trunks.Where(t=>t).ToArray();
  Note($"Part A {Scene}: run-out corridor cleared: {clear.Length} trees ({removedPieces} batched pieces)");
  int movedTrees=0,movedPieces=0;var shift=new Dictionary<MeshFilter,Dictionary<int,float>>();
  foreach(var tr in trunks){if(!float.IsFinite(oldG[tr]))continue;var b=tr.bounds;float g=NewTop(b.center);if(!float.IsFinite(g))continue;
   // A tree standing on the old ground keeps its relation to the ground; one the old mound had buried (base more than
   // 1.5 m under the old surface) is stood on the new ground, 0.3 m sunk.
   bool buried=b.min.y<oldG[tr]-1.5f;float baseRef=buried?b.min.y+.3f:oldG[tr];float dy=g-baseRef;if(Mathf.Abs(dy)<.05f)continue;
   tr.transform.position+=Vector3.up*dy;movedTrees++;int pieces=0;
   foreach(var cp in comps){if(new Vector2(cp.ctr.x-b.center.x,cp.ctr.z-b.center.z).magnitude>2.2f||cp.minY<baseRef-1.5f||cp.minY>baseRef+12)continue;if(!shift.TryGetValue(cp.mf,out var sm))shift[cp.mf]=sm=new();foreach(int v in cp.verts)sm[v]=dy;movedPieces++;pieces++;}
   Note($"Part A {Scene}: tree at {b.center:F1} re-grounded {dy:+0.00;-0.00} m{(buried?" (was buried by the old mound)":"")} (trunk collider '{Path(tr.transform)}' and {pieces} batched pieces)");}
  foreach(var kv in shift){var mf=kv.Key;var m=Own(mf);var lv=m.vertices;foreach(var e in kv.Value){var q=mf.transform.TransformPoint(lv[e.Key]);q.y+=e.Value;lv[e.Key]=mf.transform.InverseTransformPoint(q);}m.vertices=lv;m.RecalculateBounds();EditorUtility.SetDirty(m);}
  Note($"Part A {Scene}: trees re-grounded {movedTrees} ({movedPieces} batched pieces moved with them)");
  foreach(var kv in groups){var g=kv.Key;var (baseY,ctr,og)=kv.Value;float ng=NewTop(ctr);if(!float.IsFinite(ng)||!float.IsFinite(og))continue;bool buried=baseY<og-1.5f;float dy=buried?ng-.05f-baseY:ng-og;if(Mathf.Abs(dy)<.05f)continue;
   g.position+=Vector3.up*dy;EditorUtility.SetDirty(g);Note($"Part A {Scene}: '{Path(g)}' re-grounded {dy:+0.00;-0.00} m{(buried?" (was buried by the old mound)":"")}");}
  foreach(var s in signs){float g=NewTop(s.transform.position);if(!float.IsFinite(g)||!float.IsFinite(signOld[s]))continue;float dy=g-signOld[s];if(Mathf.Abs(dy)<.05f)continue;s.transform.position+=Vector3.up*dy;EditorUtility.SetDirty(s.transform);Note($"Part A {Scene}: sign '{Path(s.transform)}' re-grounded {dy:+0.00;-0.00} m");}
 }
 // ---------- Part B: one smooth collidable surface over a union of circles (x, -, z, radius) ----------
 // 0.5 m grid. A cell whose top surface is pavement is left open; pavement cells bordering the new surface are pinned 3 cm
 // under the pavement (the surface tucks under the edge, no slot). Cells whose top is a surface that must stay (the 0.70
 // gores, berms, rock barriers) are left open and their neighbours are pinned 2 cm under it (flush). Cells within 0.75 m of
 // the union's outline are pinned 2 cm under the existing ground; all other cells take the smooth (harmonic) surface.
 // Then every other terrain-type sheet's vertices that lie less than 0.8 m below (or up to 4 m above) the new surface or the
 // pavement, inside the outline shrunk 1 m, go to 0.8 m below it - only where the new surface/pavement covers the point and
 // 0.4 m around it, so nothing a wheel or a compressed body can meet is left just under the surface, and no slot opens.
 // Pavement and kept surfaces are never edited.
 static bool IsDrive(Collider c)=>c&&c.name.Contains("driving surface");
 static bool Solid(Collider c)=>c&&!c.isTrigger&&!c.attachedRigidbody;
 static bool IsTerrainB(Collider c)=>c is MeshCollider&&Solid(c)&&c.name.StartsWith("Ground")&&!IsDrive(c);
 static bool Keep(Collider c)=>c&&(c.name.Contains("Report070")||c.name.Contains("barrier")||c.name.Contains("natural edge")||c.name.Contains("outcrop")||c.name.Contains("boulder")||c.name.Contains("rock")||c.name.Contains("portal")||c.name.Contains("vault"));
 static bool SurfaceB(float x,float z,float from,Func<Collider,bool> ok,out RaycastHit hit,float depth){var o=new Vector3(x,from,z);hit=default;
  for(int k=0;k<60&&depth>0;k++){if(!Physics.Raycast(o,Vector3.down,out var h,depth,~0,QueryTriggerInteraction.Ignore))return false;if(h.normal.y>0&&ok(h.collider)){hit=h;return true;}depth-=h.distance+.002f;o=h.point+Vector3.down*.002f;}return false;}
 static Color EarthB=new(.39f,.48f,.29f);
 static void Junction(string label,Vector4[] circles,bool dry){
  float D(float x,float z){float best=-1e9f;foreach(var c in circles)best=Mathf.Max(best,c.w-new Vector2(x-c.x,z-c.z).magnitude);return best;}
  string nm="Ground_Report071 patch "+label;foreach(var old in Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Where(x=>x.name==nm).ToArray())Object.DestroyImmediate(old.gameObject);Physics.SyncTransforms();
  float x0=circles.Min(c=>c.x-c.w),x1=circles.Max(c=>c.x+c.w),z0=circles.Min(c=>c.z-c.w),z1=circles.Max(c=>c.z+c.w);
  int NX=Mathf.CeilToInt((x1-x0)/.5f)+1,NZ=Mathf.CeilToInt((z1-z0)/.5f)+1;var H=new float[NX,NZ];var kind=new int[NX,NZ];// 0 out,1 pave-pinned,2 rim/keep-pinned,3 free,4 pavement,5 kept surface
  float yRef=0;{var c=circles[0];if(SurfaceB(c.x,c.z,600,Solid,out var h,1200))yRef=h.point.y;}
  var oldTop=new Dictionary<(int,int),float>();Material mat=null;
  for(int i=0;i<NX;i++)for(int j=0;j<NZ;j++){float x=x0+i*.5f,z=z0+j*.5f;float d=D(x,z);if(d<0){kind[i,j]=0;continue;}
   if(!SurfaceB(x,z,yRef+40,Solid,out var top,80)){kind[i,j]=3;H[i,j]=yRef;continue;}// nothing found: filled by the smooth surface
   if(IsDrive(top.collider)){kind[i,j]=4;H[i,j]=top.point.y-.03f;continue;}
   if(Keep(top.collider)||top.collider.name.StartsWith("Ground_Report071")){kind[i,j]=5;H[i,j]=top.point.y-.02f;continue;}
   if(!IsTerrainB(top.collider)){if(!SurfaceB(x,z,top.point.y-.01f,IsTerrainB,out top,40)){kind[i,j]=3;H[i,j]=yRef;continue;}}
   if(!mat&&top.collider.GetComponent<Renderer>()&&top.collider.name.Contains("earth banks"))mat=top.collider.GetComponent<Renderer>().sharedMaterial;
   H[i,j]=top.point.y;oldTop[(i,j)]=top.point.y;kind[i,j]=d<.75f?2:3;if(kind[i,j]==2)H[i,j]-=.02f;}
  bool Nb(int i,int j,Func<int,bool> f){for(int a=-1;a<=1;a++)for(int b=-1;b<=1;b++){int ii=i+a,jj=j+b;if(ii>=0&&jj>=0&&ii<NX&&jj<NZ&&f(kind[ii,jj]))return true;}return false;}
  var k2=(int[,])kind.Clone();
  for(int i=0;i<NX;i++)for(int j=0;j<NZ;j++){if(kind[i,j]==4&&Nb(i,j,k=>k==2||k==3))k2[i,j]=1;
   if(kind[i,j]==3&&Nb(i,j,k=>k==5)){k2[i,j]=2;float y=float.MinValue;for(int a=-1;a<=1;a++)for(int b=-1;b<=1;b++){int ii=i+a,jj=j+b;if(ii>=0&&jj>=0&&ii<NX&&jj<NZ&&kind[ii,jj]==5)y=Mathf.Max(y,H[ii,jj]);}H[i,j]=y;}}
  kind=k2;int free=0,pinPave=0;foreach(var k in kind){if(k==3)free++;if(k==1)pinPave++;}
  for(int it=0;it<4000;it++){float md=0;for(int i=0;i<NX;i++)for(int j=0;j<NZ;j++){if(kind[i,j]!=3)continue;float sum=0;int n=0;foreach(var (a,b) in new[]{(1,0),(-1,0),(0,1),(0,-1)}){int ii=i+a,jj=j+b;if(ii<0||jj<0||ii>=NX||jj>=NZ)continue;var k=kind[ii,jj];if(k==1||k==2||k==3){sum+=H[ii,jj];n++;}}if(n>0){float hv=sum/n;md=Mathf.Max(md,Mathf.Abs(hv-H[i,j]));H[i,j]=hv;}}if(md<1e-5f&&it>100)break;}
  float up=0,down=0;foreach(var kv in oldTop){var (i,j)=kv.Key;if(kind[i,j]!=3)continue;up=Mathf.Max(up,H[i,j]-kv.Value);down=Mathf.Min(down,H[i,j]-kv.Value);}
  Note($"Part B {label}: {free} free 0.5 m cells, {pinPave} pinned under pavement edges; new surface vs old top {down:+0.00;-0.00} .. {up:+0.00;-0.00} m");
  foreach(var g in Object.FindObjectsByType<ReverseShortcutGuidance>(FindObjectsInactive.Include,FindObjectsSortMode.None)){var w=g.GetComponent<WoodlandRoute>();if(!w||g.takeoff<0)continue;w.Initialize();var a=w.At(g.takeoff,out _);var b=w.At(g.landing,out _);Note($"Part B {label}: flight window '{w.title}' takeoff s {g.takeoff} at {a:F1}, landing s {g.landing} at {b:F1}; outline distance {Mathf.Max(D(a.x,a.z),D(b.x,b.z)):F1} (>0 = inside)");}
  foreach(var ex in Object.FindObjectsByType<JumpRecoveryExclusion>(FindObjectsInactive.Include,FindObjectsSortMode.None)){bool hit=false;for(float t=0;t<=1;t+=.05f){var q=Vector3.Lerp(ex.start,ex.end,t);if(D(q.x,q.z)>-ex.halfWidth)hit=true;}if(hit)Note($"Part B {label}: jump exclusion '{Path(ex.transform)}' {ex.start:F0}->{ex.end:F0} half width {ex.halfWidth} comes within its width of the outline");}
  if(dry){for(int j=NZ-1;j>=0;j-=4){var sb=new System.Text.StringBuilder($"{z0+j*.5f,7:F1} ");for(int i=0;i<NX;i+=2){var k=kind[i,j];sb.Append(k==0?' ':k==4?'P':k==5?'K':k==1?'e':(oldTop.TryGetValue((i,j),out var o)?((char)('0'+Mathf.Clamp(Mathf.RoundToInt((H[i,j]-o)+5),0,9))):'?'));}Note(sb.ToString());}
   Note("(x from "+x0+" every 1 m; digit = new - old + 5 m; P pavement, e flush edge, K kept surface)");return;}
  var v=new List<Vector3>();var col=new List<Color>();var tri=new List<int>();bool In(int i,int j)=>kind[i,j]==1||kind[i,j]==2||kind[i,j]==3;Vector3 P(int i,int j)=>new(x0+i*.5f,H[i,j],z0+j*.5f);
  void Tri(Vector3 a,Vector3 b,Vector3 c){var n=Vector3.Cross(b-a,c-a);if(n.y<0){var t=b;b=c;c=t;}int k=v.Count;v.AddRange(new[]{a,b,c});col.AddRange(new[]{EarthB,EarthB,EarthB});tri.AddRange(new[]{k,k+1,k+2});}
  for(int i=0;i+1<NX;i++)for(int j=0;j+1<NZ;j++){if(!In(i,j)||!In(i+1,j)||!In(i,j+1)||!In(i+1,j+1))continue;if(kind[i,j]==1&&kind[i+1,j]==1&&kind[i,j+1]==1&&kind[i+1,j+1]==1)continue;Tri(P(i,j),P(i+1,j),P(i+1,j+1));Tri(P(i,j),P(i+1,j+1),P(i,j+1));}
  var mesh=new Mesh{indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};mesh.SetVertices(v);mesh.SetColors(col);mesh.SetTriangles(tri,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
  var go=new GameObject(nm,typeof(MeshFilter),typeof(MeshRenderer),typeof(MeshCollider));var stored=Store("patch-"+label.Split(' ')[0].ToLower().Replace('/','-'),mesh);go.GetComponent<MeshFilter>().sharedMesh=stored;go.GetComponent<MeshCollider>().sharedMesh=stored;
  if(!mat)mat=Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).First(x=>x.name.Contains("earth banks")).sharedMaterial;go.GetComponent<MeshRenderer>().sharedMaterial=mat;GameObjectUtility.SetStaticEditorFlags(go,StaticEditorFlags.BatchingStatic);Physics.SyncTransforms();
  var pc=go.GetComponent<MeshCollider>();
  var trunks=Object.FindObjectsByType<Collider>(FindObjectsSortMode.None).Where(c=>!c.isTrigger&&c.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0&&c.bounds.size.x<3&&D(c.bounds.center.x,c.bounds.center.z)>0).ToArray();
  var trunkOld=trunks.ToDictionary(c=>c,c=>SurfaceB(c.bounds.center.x,c.bounds.center.z,c.bounds.max.y+3,x=>x!=pc&&IsTerrainB(x),out var th,40)?th.point.y:float.NaN);
  var posts=Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Where(t=>t.name=="Whole timber post"&&t.parent&&D(t.position.x,t.position.z)>0).ToArray();
  // Cover height at a point: the new surface (bilinear on its own grid, where all four corners belong to it) and/or the
  // pavement (raycast for the driving surface only), whichever is higher.
  bool PatchAt(float x,float z,out float y){y=0;float fi=(x-x0)/.5f,fj=(z-z0)/.5f;int i=Mathf.FloorToInt(fi),j=Mathf.FloorToInt(fj);if(i<0||j<0||i+1>=NX||j+1>=NZ)return false;
   if(!In(i,j)||!In(i+1,j)||!In(i,j+1)||!In(i+1,j+1))return false;float u=fi-i,t=fj-j;y=Mathf.Lerp(Mathf.Lerp(H[i,j],H[i+1,j],u),Mathf.Lerp(H[i,j+1],H[i+1,j+1],u),t);return true;}
  bool CoverAt(float x,float z,float yv,out float top){top=float.MinValue;bool any=false;if(PatchAt(x,z,out float py)){top=py;any=true;}if(SurfaceB(x,z,yv+4.5f,IsDrive,out var h,10)){top=Mathf.Max(top,h.point.y);any=true;}return any;}
  int lowered=0;var touched=new List<string>();
  foreach(var mf in Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Where(x=>x.sharedMesh&&x.TryGetComponent<MeshCollider>(out var c)&&IsTerrainB(c)&&!Keep(c)&&x.gameObject!=go).ToArray()){
   var bb=mf.GetComponent<Collider>().bounds;if(bb.max.x<x0||bb.min.x>x1||bb.max.z<z0||bb.min.z>z1)continue;
   var m=mf.sharedMesh;var lv=m.vertices;bool any=false;int n=0;
   for(int i=0;i<lv.Length;i++){var w=mf.transform.TransformPoint(lv[i]);if(w.x<x0||w.x>x1||w.z<z0||w.z>z1||D(w.x,w.z)<1f)continue;
    if(!CoverAt(w.x,w.z,w.y,out float top))continue;if(w.y<top-.8f||w.y>top+4f)continue;
    bool covered=true;foreach(var dd in new[]{Vector3.right,Vector3.left,Vector3.forward,Vector3.back})if(!CoverAt(w.x+dd.x*.4f,w.z+dd.z*.4f,w.y,out _)){covered=false;break;}if(!covered)continue;
    w.y=top-.8f;lv[i]=mf.transform.InverseTransformPoint(w);n++;any=true;}
   if(any){if(!AssetDatabase.GetAssetPath(m).StartsWith("Assets/")){Note($"Part B {label}: {mf.name} is not a project asset; left");continue;}m=Own(mf);m.vertices=lv;m.RecalculateNormals();m.RecalculateBounds();Refresh(mf);lowered+=n;touched.Add($"{mf.name} {n}");}}
  Physics.SyncTransforms();
  Note($"Part B {label}: surface built ({tri.Count/3} tris, material {mat.name}); terrain-sheet vertices lowered to 0.8 m under it or the pavement: {lowered} ({string.Join(", ",touched)})");
  var moves=new List<(Collider tr,float baseRef,float dy)>();
  foreach(var tr in trunks){var b=tr.bounds;if(!SurfaceB(b.center.x,b.center.z,b.max.y+15,c=>c==pc||IsTerrainB(c),out var nh,40))continue;float og=trunkOld[tr];bool buried=!float.IsFinite(og)||b.min.y<og-1.5f;float baseRef=buried?b.min.y+.3f:og;float dy=nh.point.y-baseRef;if(Mathf.Abs(dy)<.1f)continue;moves.Add((tr,baseRef,dy));}
  ShiftTrees(moves,new Bounds(new Vector3((x0+x1)/2,0,(z0+z1)/2),new Vector3(x1-x0+6,4000,z1-z0+6)),"Part B "+label);
  foreach(var post in posts){var r=post.GetComponent<Renderer>();if(!r)continue;var b=r.bounds;if(!SurfaceB(b.center.x,b.center.z,b.max.y+1,c=>c==pc||IsTerrainB(c)||IsDrive(c),out var nh,30))continue;float dy=nh.point.y-.15f-b.min.y;if(Mathf.Abs(dy)<.05f)continue;post.parent.position+=Vector3.up*dy;EditorUtility.SetDirty(post.parent);Note($"Part B {label}: sign '{Path(post.parent)}' re-seated {dy:+0.00;-0.00} m (post base on the new ground)");}
 }
 // Trees: each trunk collider and that tree's pieces in the batched tree meshes (connected parts within 2.2 m of the trunk,
 // starting 1.5 m below to 12 m above its base) move up/down by the ground change under the trunk.
 static void ShiftTrees(List<(Collider tr,float baseRef,float dy)> moves,Bounds area,string label){if(moves.Count==0){Note($"{label}: trees re-grounded 0");return;}
  var batched=Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Where(m=>m.sharedMesh&&!m.GetComponent<Collider>()&&m.GetComponent<Renderer>()&&m.GetComponent<Renderer>().bounds.Intersects(area)&&(m.name.IndexOf("tree",StringComparison.OrdinalIgnoreCase)>=0||m.name.Contains("canopy")||m.name.Contains("woodland")||m.name.Contains("foliage")||m.name.Contains("timber")||m.name.Contains("crowns")||m.name.Contains("Forest detail"))).ToArray();
  var comps=new List<(MeshFilter mf,int[] verts,Vector3 ctr,float minY)>();
  foreach(var mf in batched){var m=mf.sharedMesh;var t=m.triangles;var par=Enumerable.Range(0,m.vertexCount).ToArray();int F(int a){while(par[a]!=a){par[a]=par[par[a]];a=par[a];}return a;}
   for(int i=0;i<t.Length;i+=3){int a=F(t[i]),b=F(t[i+1]);par[b]=a;int c=F(t[i+2]);par[c]=F(a);}
   var w=m.vertices.Select(v=>mf.transform.TransformPoint(v)).ToArray();var used=new HashSet<int>(t);
   foreach(var g in used.GroupBy(F)){var vs=g.ToArray();var c=Vector3.zero;float mn=1e9f;foreach(int v in vs){c+=w[v];mn=Mathf.Min(mn,w[v].y);}comps.Add((mf,vs,c/vs.Length,mn));}}
  var shift=new Dictionary<MeshFilter,Dictionary<int,float>>();int pieces=0;
  foreach(var (tr,baseRef,dy) in moves){var b=tr.bounds;int n=0;foreach(var cp in comps){if(new Vector2(cp.ctr.x-b.center.x,cp.ctr.z-b.center.z).magnitude>2.2f||cp.minY<baseRef-1.5f||cp.minY>baseRef+12)continue;if(!shift.TryGetValue(cp.mf,out var sm))shift[cp.mf]=sm=new();foreach(int v in cp.verts)sm[v]=dy;n++;}
   tr.transform.position+=Vector3.up*dy;pieces+=n;Note($"{label}: tree at {b.center:F1} re-grounded {dy:+0.00;-0.00} m (trunk '{Path(tr.transform)}' and {n} batched pieces)");}
  foreach(var kv in shift){var mf=kv.Key;var m=Own(mf);var lv=m.vertices;foreach(var e in kv.Value){var q=mf.transform.TransformPoint(lv[e.Key]);q.y+=e.Value;lv[e.Key]=mf.transform.InverseTransformPoint(q);}m.vertices=lv;m.RecalculateBounds();EditorUtility.SetDirty(m);}
  Note($"{label}: trees re-grounded {moves.Count} ({pieces} batched pieces moved with them)");}
 // Part A follow-up: the supported-return ribbon (14 m wide, vertices only at its two edges) was re-seated on the new ground
 // at its edges, so the new ground bulged through its middle (grass on the trail). Terrain-tile vertices inside the changed
 // box that stand above the ribbon (or less than 8 cm under it) go 15 cm under it. Nothing else changes.
 static void RibbonGround(bool dry){
  launch=Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).FirstOrDefault(t=>t.name=="CR094 summit launch");
  var ret=launch?launch.Find("Ground_CR103 supported return"):null;if(!ret||!ret.gameObject.activeInHierarchy){Note($"Part A ribbon {Scene}: no active supported-return ribbon; not changed");return;}
  var rc=ret.GetComponent<MeshCollider>();var tiles=GameObject.Find("Memory loop - north is +Z").GetComponentsInChildren<MeshFilter>().Where(m=>m.sharedMesh&&m.GetComponent<MeshCollider>()&&m.GetComponent<Renderer>().bounds.Intersects(rc.bounds)).ToArray();int n=0;float md=0;
  foreach(var mf in tiles){var lv=mf.sharedMesh.vertices;bool any=false;
   for(int i=0;i<lv.Length;i++){var w=mf.transform.TransformPoint(lv[i]);if(!InBox(w,2))continue;if(!rc.Raycast(new Ray(new Vector3(w.x,w.y+50,w.z),Vector3.down),out var h,60))continue;
    if(w.y<h.point.y-.08f||w.y>h.point.y+3)continue;md=Mathf.Max(md,w.y-(h.point.y-.15f));w.y=h.point.y-.15f;lv[i]=mf.transform.InverseTransformPoint(w);n++;any=true;}
   if(any&&!dry){var m=Own(mf);m.vertices=lv;m.RecalculateNormals();m.RecalculateBounds();Refresh(mf);}}
  Note($"Part A ribbon {Scene}: terrain vertices under the supported-return ribbon lowered to 15 cm under it: {n} (largest move {md:F2} m){(dry?" (dry run)":"")}");}
}
