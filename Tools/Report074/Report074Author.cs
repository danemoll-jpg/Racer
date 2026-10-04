using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
// 0.74 local changes (PROJECT_TODO "CURRENT" Part A). Every change is logged to Docs/Report074/author-notes.txt.
// AUTHOR_PARTS selects parts, AUTHOR_SCENES the scenes, AUTHOR_DRY=1 reports without saving.
public static partial class Report074Author {
 const string Folder="Assets/Track/Report074";
 static readonly List<string> notes=new();
 static void Note(string s){notes.Add(s);Debug.Log("REPORT074 "+s);}
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

 static bool IsDrive(Collider c)=>c&&c.name.Contains("driving surface");
 static bool Solid(Collider c)=>c&&!c.isTrigger&&!c.attachedRigidbody;
 static bool IsTerrainB(Collider c)=>c is MeshCollider&&Solid(c)&&c.name.StartsWith("Ground")&&!IsDrive(c);
 static bool Keep(Collider c)=>c&&(c.name.Contains("Report070")||c.name.Contains("barrier")||c.name.Contains("natural edge")||c.name.Contains("outcrop")||c.name.Contains("boulder")||c.name.Contains("rock")||c.name.Contains("portal")||c.name.Contains("vault"));
 static bool SurfaceB(float x,float z,float from,Func<Collider,bool> ok,out RaycastHit hit,float depth){var o=new Vector3(x,from,z);hit=default;
  for(int k=0;k<60&&depth>0;k++){if(!Physics.Raycast(o,Vector3.down,out var h,depth,~0,QueryTriggerInteraction.Ignore))return false;if(h.normal.y>0&&ok(h.collider)){hit=h;return true;}depth-=h.distance+.002f;o=h.point+Vector3.down*.002f;}return false;}
 static Color EarthB=new(.39f,.48f,.29f);
 static void Junction(string label,Vector4[] circles,bool dry,int smoothing=4000){
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
  for(int it=0;it<smoothing;it++){float md=0;for(int i=0;i<NX;i++)for(int j=0;j<NZ;j++){if(kind[i,j]!=3)continue;float sum=0;int n=0;foreach(var (a,b) in new[]{(1,0),(-1,0),(0,1),(0,-1)}){int ii=i+a,jj=j+b;if(ii<0||jj<0||ii>=NX||jj>=NZ)continue;var k=kind[ii,jj];if(k==1||k==2||k==3){sum+=H[ii,jj];n++;}}if(n>0){float hv=sum/n;md=Mathf.Max(md,Mathf.Abs(hv-H[i,j]));H[i,j]=hv;}}if(md<1e-5f&&it>100&&smoothing>=4000)break;}
  float up=0,down=0;foreach(var kv in oldTop){var (i,j)=kv.Key;if(kind[i,j]!=3)continue;up=Mathf.Max(up,H[i,j]-kv.Value);down=Mathf.Min(down,H[i,j]-kv.Value);}
  Note($"{label}: {free} free 0.5 m cells, {pinPave} pinned under pavement edges; new surface vs old top {down:+0.00;-0.00} .. {up:+0.00;-0.00} m");
  foreach(var g in Object.FindObjectsByType<ReverseShortcutGuidance>(FindObjectsInactive.Include,FindObjectsSortMode.None)){var w=g.GetComponent<WoodlandRoute>();if(!w||g.takeoff<0)continue;w.Initialize();var a=w.At(g.takeoff,out _);var b=w.At(g.landing,out _);Note($"{label}: flight window '{w.title}' takeoff s {g.takeoff} at {a:F1}, landing s {g.landing} at {b:F1}; outline distance {Mathf.Max(D(a.x,a.z),D(b.x,b.z)):F1} (>0 = inside)");}
  foreach(var ex in Object.FindObjectsByType<JumpRecoveryExclusion>(FindObjectsInactive.Include,FindObjectsSortMode.None)){bool hit=false;for(float t=0;t<=1;t+=.05f){var q=Vector3.Lerp(ex.start,ex.end,t);if(D(q.x,q.z)>-ex.halfWidth)hit=true;}if(hit)Note($"{label}: jump exclusion '{Path(ex.transform)}' {ex.start:F0}->{ex.end:F0} half width {ex.halfWidth} comes within its width of the outline");}
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
   if(any){if(!AssetDatabase.GetAssetPath(m).StartsWith("Assets/")){Note($"{label}: {mf.name} is not a project asset; left");continue;}m=Own(mf);m.vertices=lv;m.RecalculateNormals();m.RecalculateBounds();Refresh(mf);lowered+=n;touched.Add($"{mf.name} {n}");}}
  Physics.SyncTransforms();
  Note($"{label}: surface built ({tri.Count/3} tris, material {mat.name}); terrain-sheet vertices lowered to 0.8 m under it or the pavement: {lowered} ({string.Join(", ",touched)})");
  var moves=new List<(Collider tr,float baseRef,float dy)>();
  foreach(var tr in trunks){var b=tr.bounds;if(!SurfaceB(b.center.x,b.center.z,b.max.y+15,c=>c==pc||IsTerrainB(c),out var nh,40))continue;float og=trunkOld[tr];bool buried=!float.IsFinite(og)||b.min.y<og-1.5f;float baseRef=buried?b.min.y+.3f:og;float dy=nh.point.y-baseRef;if(Mathf.Abs(dy)<.1f)continue;moves.Add((tr,baseRef,dy));}
  ShiftTrees(moves,new Bounds(new Vector3((x0+x1)/2,0,(z0+z1)/2),new Vector3(x1-x0+6,4000,z1-z0+6)),""+label);
  foreach(var post in posts){var r=post.GetComponent<Renderer>();if(!r)continue;var b=r.bounds;if(!SurfaceB(b.center.x,b.center.z,b.max.y+1,c=>c==pc||IsTerrainB(c)||IsDrive(c),out var nh,30))continue;float dy=nh.point.y-.15f-b.min.y;if(Mathf.Abs(dy)<.05f)continue;post.parent.position+=Vector3.up*dy;EditorUtility.SetDirty(post.parent);Note($"{label}: sign '{Path(post.parent)}' re-seated {dy:+0.00;-0.00} m (post base on the new ground)");}
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

 public static void Run(){
  if(Application.isPlaying)throw new Exception("Edit mode required");Directory.CreateDirectory("Docs/Report074");Physics.queriesHitBackfaces=false;
  var parts=(Environment.GetEnvironmentVariable("AUTHOR_PARTS")??"all").Split(',');bool Do(string p)=>parts.Contains("all")||parts.Contains(p);
  var scenes=(Environment.GetEnvironmentVariable("AUTHOR_SCENES")??"StreetLoopGreybox").Split(',');bool dry=Environment.GetEnvironmentVariable("AUTHOR_DRY")=="1";
  try{
   foreach(var scene in scenes){
    EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();deps=null;Note("SCENE "+scene+(dry?" (dry run, not saved)":""));
    if(Do("tree"))Bug001Tree(dry);
    if(Do("sign"))Bug002Sign(dry);
    if(Do("drive"))Bug003Drive(dry);
    if(Do("b4fix")&&scene=="MountainLoop"){var tr=Object.FindObjectsByType<BoxCollider>(FindObjectsSortMode.None).FirstOrDefault(c=>c.name=="Roadside woodland trunk"&&new Vector2(c.bounds.center.x-735.9f,c.bounds.center.z+133.7f).magnitude<.6f);if(tr){tr.transform.position+=Vector3.up*.97f;EditorUtility.SetDirty(tr.transform);Note($"BUG-004 rerun: trunk at {tr.bounds.center:F2} put back up 0.97 m (its batched pieces were restored with the sheets)");}}
    if(Do("b4")&&scene=="MountainLoop")Junction("BUG-004 main s 12 trail edge",new[]{new Vector4(738.6f,0,-126.5f,4.5f),new Vector4(739.2f,0,-131.5f,4.5f),new Vector4(739.6f,0,-136.5f,4f),new Vector4(733.5f,0,-125f,5.5f),new Vector4(733.5f,0,-131f,6f),new Vector4(734f,0,-137.5f,5.5f)},dry,40);// 40 passes: the bank's own shape, smoothed (fully relaxed it sagged into a ditch)
    if(Do("b5")&&scene=="MountainLoop")Junction("BUG-005 main s 1503 left bank",new[]{new Vector4(1036.5f,0,27.5f,6.5f),new Vector4(1036.5f,0,35.5f,6.5f)},dry);
    if(Do("tunnel"))PartBTunnel(dry);
    if(Do("landmark"))TunnelLandmark(dry);
    if(Do("cave"))PartCCave(dry);
    if(!dry)Save();}
  }finally{File.AppendAllLines("Docs/Report074/author-notes.txt",notes);}
  EditorApplication.Exit(0);}

 static float GroundY(float x,float z,float from=400){foreach(var h in Physics.RaycastAll(new Vector3(x,from,z),Vector3.down,from+200,~0,QueryTriggerInteraction.Ignore).OrderBy(h=>h.distance)){if(h.collider.attachedRigidbody||h.collider.isTrigger)continue;var n=h.collider.name;if(n.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0||n.IndexOf("tree",StringComparison.OrdinalIgnoreCase)>=0)continue;if(!n.StartsWith("Ground"))continue;return h.point.y;}return float.NaN;}

 // ---------- BUG-001: the tree standing in the dirt driveway to the brick house ----------
 // Trunk collider "Roadside woodland trunk" at (440.0, 7.2) with its two batched pieces (trunk, canopy) in "Additional
 // woodland canopy". All three move 6 m south (left of the driver at heading 250) onto the grass, re-grounded.
 static void Bug001Tree(bool dry){var at=new Vector3(440.01f,0,7.24f);var move=new Vector3(0,0,-6f);
  var trunk=Object.FindObjectsByType<BoxCollider>(FindObjectsSortMode.None).FirstOrDefault(c=>c.name=="Roadside woodland trunk"&&new Vector2(c.bounds.center.x-at.x,c.bounds.center.z-at.z).magnitude<.6f);
  if(!trunk){Note($"BUG-001 {Scene}: tree not present; not changed");return;}
  var b=trunk.bounds;var to=new Vector3(b.center.x+move.x,0,b.center.z+move.z);float g0=GroundY(b.center.x,b.center.z),g1=GroundY(to.x,to.z);float dy=g1-g0;
  var clash=Object.FindObjectsByType<Collider>(FindObjectsSortMode.None).Where(c=>c!=trunk&&!c.isTrigger&&!c.name.StartsWith("Ground")&&c.bounds.size.x<8&&c.bounds.size.z<8&&new Vector2(c.bounds.center.x-to.x,c.bounds.center.z-to.z).magnitude<2.5f).Select(c=>Path(c.transform)).ToArray();
  Note($"BUG-001 {Scene}: trunk {Path(trunk.transform)} at {b.center:F2} ground {g0:F2} -> ({to.x:F2},{to.z:F2}) ground {g1:F2} (dy {dy:+0.00;-0.00}); colliders within 2.5 m of the new spot: {(clash.Length==0?"none":string.Join(", ",clash))}; new spot surface {SurfaceName(to.x,to.z)}");
  if(clash.Length>0){Note("BUG-001: not moved (something stands at the new spot)");return;}
  var pieces=Report074Trees.Pieces(new Vector3(at.x,b.center.y,at.z),3).Where(p=>new Vector2(p.b.center.x-at.x,p.b.center.z-at.z).magnitude<1.2f&&p.mf.name.Contains("woodland canopy")).ToList();
  Note($"BUG-001 {Scene}: {pieces.Count} batched pieces ({string.Join(", ",pieces.Select(p=>$"{p.tris.Count} tris {p.b.min.y:F1}..{p.b.max.y:F1}"))})");
  if(dry)return;var delta=move+Vector3.up*dy;
  foreach(var grp in pieces.GroupBy(p=>p.mf)){var mf=grp.Key;var m=Own(mf);var lv=m.vertices;var t=m.triangles;var set=new HashSet<int>();foreach(var p in grp)foreach(var q in p.tris)for(int j=0;j<3;j++)set.Add(t[3*q+j]);
   foreach(int v in set){var w=mf.transform.TransformPoint(lv[v]);lv[v]=mf.transform.InverseTransformPoint(w+delta);}m.vertices=lv;m.RecalculateBounds();EditorUtility.SetDirty(m);}
  trunk.transform.position+=delta;EditorUtility.SetDirty(trunk.transform);Physics.SyncTransforms();Note($"BUG-001 {Scene}: tree moved by {delta:F2}");}

 // A tree (trunk collider + its connected pieces in the batched woodland meshes within 1.2 m of the trunk) moves by delta.
 static void MoveTree(Collider trunk,Vector3 delta,string label){var b=trunk.bounds;var pieces=Report074Trees.Pieces(new Vector3(b.center.x,b.center.y,b.center.z),3).Where(p=>new Vector2(p.b.center.x-b.center.x,p.b.center.z-b.center.z).magnitude<1.2f&&p.b.min.y<b.max.y+1&&p.b.max.y>b.min.y-1).ToList();
  foreach(var grp in pieces.GroupBy(p=>p.mf)){var mf=grp.Key;var m=Own(mf);var lv=m.vertices;var t=m.triangles;var set=new HashSet<int>();foreach(var p in grp)foreach(var q in p.tris)for(int j=0;j<3;j++)set.Add(t[3*q+j]);
   foreach(int v in set){var w=mf.transform.TransformPoint(lv[v]);lv[v]=mf.transform.InverseTransformPoint(w+delta);}m.vertices=lv;m.RecalculateBounds();EditorUtility.SetDirty(m);}
  trunk.transform.position+=delta;EditorUtility.SetDirty(trunk.transform);Physics.SyncTransforms();Note($"{label}: tree {Path(trunk.transform)} at {b.center:F1} moved by {delta:F2} with {pieces.Count} batched pieces ({string.Join(", ",pieces.Select(p=>p.mf.name+" "+p.tris.Count))})");}
 static string SurfaceName(float x,float z){foreach(var h in Physics.RaycastAll(new Vector3(x,400,z),Vector3.down,600,~0,QueryTriggerInteraction.Ignore).OrderBy(h=>h.distance)){if(h.collider.isTrigger||h.collider.attachedRigidbody)continue;var n=h.collider.name;if(n.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0||n.IndexOf("tree",StringComparison.OrdinalIgnoreCase)>=0)continue;return n;}return "none";}

 // ---------- BUG-002: the diamond warning sign on the tall post at the top of the House 3 driveway ----------
 static void Bug002Sign(bool dry){var signs=Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(t=>t.name=="Simple bend warning"&&t.parent&&t.parent.name=="CR-015 modest roadside details"&&new Vector2(t.position.x-501.84f,t.position.z+129.83f).magnitude<8).ToArray();
  if(signs.Length==0){var any=Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None).Where(r=>r.name=="Ochre warning panel"&&new Vector2(r.bounds.center.x-501.84f,r.bounds.center.z+129.83f).magnitude<8).Select(r=>Path(r.transform)).ToArray();Note($"BUG-002 {Scene}: sign not present{(any.Length>0?" (other panel(s): "+string.Join(", ",any)+")":"")}");return;}
  foreach(var s in signs){Note($"BUG-002 {Scene}: removed {Path(s)} at {s.position:F2} ({s.GetComponentsInChildren<Renderer>(true).Length} parts, colliders {s.GetComponentsInChildren<Collider>(true).Length})");if(!dry)Object.DestroyImmediate(s.gameObject);}}

}
