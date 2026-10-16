using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// Temporary 0.96 world tool (copied into Assets/Editor/Report096Temp only while it runs). Probe: what is at each point.
public static class Report096World {
 static string Out=>Environment.GetEnvironmentVariable("PROBE_OUT");
 static string P(Transform t){var s=t.name;while(t.parent){t=t.parent;s=t.name+"/"+s;}return s;}
 public static void Probe(){
  var scene=Environment.GetEnvironmentVariable("PROBE_SCENES");EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");
  var log=new List<string>();Physics.SyncTransforms();
  foreach(var spec in Environment.GetEnvironmentVariable("WORLD_POINTS").Split('|')){var a=spec.Split(',').Select(x=>float.Parse(x,System.Globalization.CultureInfo.InvariantCulture)).ToArray();var c=new Vector3(a[0],a[1],a[2]);float r=a[3];
   log.Add($"==== {c} r {r}");
   foreach(var col in UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsInactive.Exclude,FindObjectsSortMode.None)){var b=col.bounds;var d=new Vector3(Mathf.Max(b.min.x-c.x,0,c.x-b.max.x),0,Mathf.Max(b.min.z-c.z,0,c.z-b.max.z));if(d.magnitude>r)continue;
    string mesh=col is MeshCollider mc&&mc.sharedMesh?$"mesh {mc.sharedMesh.name} v{mc.sharedMesh.vertexCount}":col.GetType().Name;log.Add($"  COL {P(col.transform)} [{mesh}] bounds {b.min:F1}..{b.max:F1} layer {col.gameObject.layer} trig {col.isTrigger}");}
   foreach(var rd in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude,FindObjectsSortMode.None)){var b=rd.bounds;var d=new Vector3(Mathf.Max(b.min.x-c.x,0,c.x-b.max.x),0,Mathf.Max(b.min.z-c.z,0,c.z-b.max.z));if(d.magnitude>r||b.size.magnitude>400)continue;
    var mf=rd.GetComponent<MeshFilter>();log.Add($"  REN {P(rd.transform)} [{(mf&&mf.sharedMesh?mf.sharedMesh.name+" v"+mf.sharedMesh.vertexCount:rd.GetType().Name)}] bounds {b.min:F1}..{b.max:F1}");}
   foreach(var rd in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude,FindObjectsSortMode.None)){var b=rd.bounds;if(b.size.magnitude<=400||!b.Contains(c))continue;var mf=rd.GetComponent<MeshFilter>();
    log.Add($"  BIG {P(rd.transform)} [{(mf&&mf.sharedMesh?mf.sharedMesh.name+" v"+mf.sharedMesh.vertexCount+" sub"+mf.sharedMesh.subMeshCount:rd.GetType().Name)}] mats {string.Join(",",rd.sharedMaterials.Select(m=>m?m.name+"/"+(m.shader?m.shader.name:"-"):"null"))} cull {(rd.sharedMaterial&&rd.sharedMaterial.HasProperty("_Cull")?rd.sharedMaterial.GetFloat("_Cull").ToString():"?")}");}
   // ground heights on a 2 m grid
   var rows=new List<string>();for(float z=r;z>=-r;z-=2){var row="";for(float x=-r;x<=r;x+=2){var o=new Vector3(c.x+x,c.y+120,c.z+z);if(Physics.Raycast(o,Vector3.down,out var h,300,~0,QueryTriggerInteraction.Ignore))row+=(h.point.y-c.y).ToString("F0").PadLeft(4);else row+="   X";}rows.Add(row);}
   log.Add("  ground height relative to the point (m), north up, 2 m cells:");log.AddRange(rows.Select(x=>"  "+x));}
  Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/probe.txt",log);}
}
// ---- seams: boundary edges of the ground meshes in FreeRoamWorld and the near-parallel pairs of them (slivers)
public static class Report096Seams {
 static string Out=>Environment.GetEnvironmentVariable("PROBE_OUT");
 public struct BE{public Vector3 a,b;public Vector3 n;public string src;public int ia,ib;}
 public static List<BE> Boundary(out List<string> names,string prefix="Ground_"){
  names=new List<string>();var edges=new Dictionary<(long,long),(int count,BE e)>();
  long Key(Vector3 v)=>((long)Mathf.RoundToInt(v.x*20)&0x1FFFFF)<<42|((long)Mathf.RoundToInt(v.y*20)&0x1FFFFF)<<21|((long)Mathf.RoundToInt(v.z*20)&0x1FFFFF);
  foreach(var mf in UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Exclude,FindObjectsSortMode.None)){
   if(!mf.sharedMesh||!mf.name.StartsWith(prefix)||!mf.GetComponent<MeshRenderer>())continue;names.Add(mf.name+" v"+mf.sharedMesh.vertexCount);
   var m=mf.transform.localToWorldMatrix;var v=mf.sharedMesh.vertices.Select(x=>m.MultiplyPoint3x4(x)).ToArray();
   for(int s=0;s<mf.sharedMesh.subMeshCount;s++){var t=mf.sharedMesh.GetTriangles(s);
    for(int i=0;i<t.Length;i+=3){var n=Vector3.Cross(v[t[i+1]]-v[t[i]],v[t[i+2]]-v[t[i]]);if(n.sqrMagnitude<1e-10f)continue;n.Normalize();
     for(int k=0;k<3;k++){int ia=t[i+k],ib=t[i+(k+1)%3];long ka=Key(v[ia]),kb=Key(v[ib]);if(ka==kb)continue;var key=ka<kb?(ka,kb):(kb,ka);
      if(edges.TryGetValue(key,out var cur))edges[key]=(cur.count+1,cur.e);else edges[key]=(1,new BE{a=v[ia],b=v[ib],n=n,src=mf.name});}}}}
  return edges.Values.Where(e=>e.count==1).Select(e=>e.e).ToList();}
 public static void Run(){
  EditorSceneManager.OpenScene("Assets/Scenes/"+Environment.GetEnvironmentVariable("PROBE_SCENES")+".unity");var log=new List<string>();
  var be=Boundary(out var names);log.Add("ground meshes: "+names.Count);log.AddRange(names.Take(80));log.Add("boundary edges: "+be.Count);
  // pair: edges whose midpoints are within 2.5 m horizontally and 1.2 m vertically and which face away from each other
  var grid=new Dictionary<(int,int),List<int>>();for(int i=0;i<be.Count;i++){var m=(be[i].a+be[i].b)/2;var k=(Mathf.FloorToInt(m.x/3),Mathf.FloorToInt(m.z/3));if(!grid.TryGetValue(k,out var l))grid[k]=l=new List<int>();l.Add(i);}
  var pairs=new List<(int,int,float)>();
  for(int i=0;i<be.Count;i++){var mi=(be[i].a+be[i].b)/2;var ci=(Mathf.FloorToInt(mi.x/3),Mathf.FloorToInt(mi.z/3));
   for(int dx=-1;dx<=1;dx++)for(int dz=-1;dz<=1;dz++)if(grid.TryGetValue((ci.Item1+dx,ci.Item2+dz),out var l))foreach(int j in l){if(j<=i)continue;
    var mj=(be[j].a+be[j].b)/2;var h=new Vector3(mj.x-mi.x,0,mj.z-mi.z);if(h.magnitude>2.5f||Mathf.Abs(mj.y-mi.y)>1.2f)continue;
    var di=(be[i].b-be[i].a);di.y=0;var dj=(be[j].b-be[j].a);dj.y=0;if(di.magnitude<.3f||dj.magnitude<.3f)continue;if(Mathf.Abs(Vector3.Dot(di.normalized,dj.normalized))<.8f)continue;
    // the triangles lie on opposite sides: i's triangle is on the side away from j
    var side=Vector3.Cross(Vector3.up,di.normalized);float ni=Vector3.Dot(be[i].n,Vector3.up),nj=Vector3.Dot(be[j].n,Vector3.up);if(ni<.2f||nj<.2f)continue;
    pairs.Add((i,j,h.magnitude));}}
  log.Add("sliver pairs (edges 0-2.5 m apart, parallel, both facing up): "+pairs.Count);
  foreach(var p in pairs.OrderBy(x=>x.Item3).Take(400)){var m=(be[p.Item1].a+be[p.Item1].b)/2;log.Add($"  at ({m.x:F1},{m.y:F1},{m.z:F1}) gap {p.Item3:F2} m  len {(be[p.Item1].b-be[p.Item1].a).magnitude:F1}/{(be[p.Item2].b-be[p.Item2].a).magnitude:F1}  {be[p.Item1].src} | {be[p.Item2].src}");}
  Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/seams.txt",log);}
}
// ---- holes: dense downward rays; cells with no ground under them, or ground far below the neighbours (a see-through sliver)
public static class Report096Holes {
 static string Out=>Environment.GetEnvironmentVariable("PROBE_OUT");
 public static float Ground(Vector3 o,out Collider col){col=null;var hits=Physics.RaycastAll(o,Vector3.down,400,~0,QueryTriggerInteraction.Ignore);float best=float.NaN;foreach(var h in hits){if(!(h.collider is MeshCollider))continue;if(float.IsNaN(best)||h.point.y>best){best=h.point.y;col=h.collider;}}return best;}
 public static void Run(){
  EditorSceneManager.OpenScene("Assets/Scenes/"+Environment.GetEnvironmentVariable("PROBE_SCENES")+".unity");Physics.SyncTransforms();var log=new List<string>();
  float step=float.Parse(Environment.GetEnvironmentVariable("WORLD_STEP")??"0.3",System.Globalization.CultureInfo.InvariantCulture);
  foreach(var spec in Environment.GetEnvironmentVariable("WORLD_POINTS").Split('|')){var a=spec.Split(',').Select(x=>float.Parse(x,System.Globalization.CultureInfo.InvariantCulture)).ToArray();var c=new Vector3(a[0],a[1],a[2]);float r=a[3];
   int n=Mathf.CeilToInt(2*r/step)+1;var h=new float[n,n];var cols=new Collider[n,n];
   for(int i=0;i<n;i++)for(int j=0;j<n;j++){h[i,j]=Ground(new Vector3(c.x-r+i*step,c.y+100,c.z-r+j*step),out cols[i,j]);}
   int none=0,drop=0;var rows=new List<string>();var names=new Dictionary<string,int>();
   for(int j=n-1;j>=0;j--){var row="";for(int i=0;i<n;i++){char ch='.';if(float.IsNaN(h[i,j])){ch='X';none++;}else{float m=0;int k=0;foreach(var (di,dj) in new[]{(-3,0),(3,0),(0,-3),(0,3)}){int ii=i+di,jj=j+dj;if(ii<0||jj<0||ii>=n||jj>=n||float.IsNaN(h[ii,jj]))continue;m+=h[ii,jj];k++;}
      if(k>=3&&h[i,j]<m/k-1.5f){ch='o';drop++;}if(cols[i,j]){names.TryGetValue(cols[i,j].name,out var q);names[cols[i,j].name]=q+1;}}row+=ch;}rows.Add(row);}
   log.Add($"==== {c} r {r} step {step}: {none} cells with no ground, {drop} cells 1.5 m below the average of their neighbours 3 cells away");
   log.Add("  ground colliders hit: "+string.Join(", ",names.OrderByDescending(x=>x.Value).Take(14).Select(x=>x.Key+" "+x.Value)));
   if(n<=260)log.AddRange(rows.Select(x=>"  "+x));}
  Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/holes.txt",log);}
}
// ---- roadside fills (Mountain Loop BUG-003, 006, 007): a graded earth shelf from the road's edge down to the ground below, where the
// ground beside the road drops away more than 1 m within a few metres (a ravine, a hole): the patch is max(ground, road - 0.3 - 0.55 * distance
// from the edge) tapering to nothing at both ends and at 14 m out; drawn and collidable; the ground's own material.
public static class Report096Fill {
 static string Out=>Environment.GetEnvironmentVariable("PROBE_OUT");
 const float Window=30,Slope=.12f,Threshold=.8f;
 static float GroundBelow(Vector3 o,out Collider col){col=null;float best=float.NaN;foreach(var h in Physics.RaycastAll(o,Vector3.down,400,~0,QueryTriggerInteraction.Ignore)){if(!(h.collider is MeshCollider)||h.collider.name.StartsWith("Ground_Report096"))continue;if(float.IsNaN(best)||h.point.y>best){best=h.point.y;col=h.collider;}}return best;}
 public static void Run(){
  var log=new List<string>();bool dry=Environment.GetEnvironmentVariable("WORLD_DRY")=="1";
  foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){
   var sc=EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();var race=UnityEngine.Object.FindAnyObjectByType<Racer.RaceDirector>();var road=race.road;road.Initialize();
   if(!dry)foreach(var g in UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(m=>m.name.StartsWith("Ground_Report096 fill")).Select(m=>m.gameObject).ToList())UnityEngine.Object.DestroyImmediate(g);
   foreach(var spec in Environment.GetEnvironmentVariable("WORLD_FILLS").Split('|')){
    if(spec.StartsWith("P:")){var pa=spec.Substring(2).Split(',');Pit(sc,pa[0],float.Parse(pa[1],System.Globalization.CultureInfo.InvariantCulture),float.Parse(pa[2],System.Globalization.CultureInfo.InvariantCulture),float.Parse(pa[3],System.Globalization.CultureInfo.InvariantCulture),dry,log,scene);continue;}
    var a=spec.Split(',');string name=a[0];float x=float.Parse(a[1],System.Globalization.CultureInfo.InvariantCulture),z=float.Parse(a[2],System.Globalization.CultureInfo.InvariantCulture);
    // the nearest road point by horizontal distance (the bug's own height is not given: use the road's own heights)
    float s0=0,bestD=float.MaxValue;for(float s=0;s<road.Length;s+=1){var q=road.At(s,out _);float d=new Vector2(q.x-x,q.z-z).magnitude;if(d<bestD){bestD=d;s0=s;}}
    log.Add($"{scene} {name}: bug at ({x:F1},{z:F1}): nearest road point station {s0:F1}, {bestD:F1} m away horizontally, road y {road.At(s0,out _).y:F1}");
    if(bestD>40){log.Add("   not beside this scene's race road: skipped");continue;}
    float Reach=a.Length>4?float.Parse(a[4],System.Globalization.CultureInfo.InvariantCulture):12;
    var protect=new List<(Func<Vector3,(float lat,float y)> q,float hw)>();
    foreach(var w in UnityEngine.Object.FindObjectsByType<Racer.WoodlandRoute>(FindObjectsInactive.Exclude,FindObjectsSortMode.None)){if(w.points==null||w.points.Length<4)continue;w.Initialize();var ww=w;protect.Add((p=>{float t=ww.Project(p,out float l);return (l,ww.At(t,out _).y);},ww.halfWidth+2f));}
    foreach(var rr in UnityEngine.Object.FindObjectsByType<Racer.RaceRoad>(FindObjectsInactive.Exclude,FindObjectsSortMode.None)){if(rr==road||rr.points==null||rr.points.Length<4||rr.openHighway)continue;rr.Initialize();var r2=rr;protect.Add((p=>{float t=r2.Project(p,out float l);return (l,r2.At(t,out _).y);},3f));}
    bool Protected(Vector3 p,float g){foreach(var (q,hw) in protect){var (l,y)=q(p);if(l<hw+Mathf.Abs(y-p.y)*0f&&Mathf.Abs(y-g)<2.5f)return true;}return false;}
    float half=0;for(float s=s0-Window;s<=s0+Window;s+=5)half=Mathf.Max(half,road.HalfWidth(s));
    for(int side=-1;side<=1;side+=2){
     string label=side<0?"left":"right";float worst=0;
     for(float s=s0-Window;s<=s0+Window;s+=2){var c=road.At(s,out var f);var right=Vector3.Cross(Vector3.up,f).normalized;for(float l=half+1;l<=half+Reach+2;l+=2){var p=c+right*side*l;float g=GroundBelow(new Vector3(p.x,c.y+60,p.z),out _);if(float.IsNaN(g))continue;worst=Mathf.Max(worst,c.y-g);}}
     log.Add($"   {label}: the ground drops up to {worst:F1} m below the road within {Reach+2:F0} m of its edge");
     if(worst<Threshold)continue;
     var rowsS=new List<float>();for(float s=s0-Window;s<=s0+Window+.01f;s+=1.5f)rowsS.Add(s);var lats=new List<float>();for(float l=half-.8f;l<=half+Reach+.01f;l+=1.5f)lats.Add(l);int cols=lats.Count;
     var verts=new List<Vector3>();var groundY=new List<float>();var fillsAt=new List<bool>();var matCount=new Dictionary<Material,int>();
     foreach(var s in rowsS){var c=road.At(s,out var f);var right=Vector3.Cross(Vector3.up,f).normalized;
      for(int k=0;k<cols;k++){float l=lats[k];var p=c+right*side*l;float g=GroundBelow(new Vector3(p.x,c.y+60,p.z),out var col);if(float.IsNaN(g))g=c.y-12;
       if(col&&k>cols/2){var mr=col.GetComponent<MeshRenderer>();if(mr&&mr.sharedMaterial){matCount.TryGetValue(mr.sharedMaterial,out var q);matCount[mr.sharedMaterial]=q+1;}}
       float fillY=c.y-.3f-Slope*Mathf.Max(0,l-(half+.5f));bool fills=fillY>g+.12f&&!Protected(p,g);verts.Add(new Vector3(p.x,fills?fillY:g+.025f,p.z));groundY.Add(g);fillsAt.Add(fills);}}
     var tris=new List<int>();var included=new bool[rowsS.Count-1,cols-1];
     for(int r=0;r+1<rowsS.Count;r++)for(int k=0;k+1<cols;k++){int i0=r*cols+k,i1=i0+1,i2=i0+cols,i3=i2+1;if(!(fillsAt[i0]||fillsAt[i1]||fillsAt[i2]||fillsAt[i3]))continue;included[r,k]=true;
      tris.AddRange(new[]{i0,i2,i1,i1,i2,i3});}
     // up-facing
     for(int q=0;q<tris.Count;q+=3){var n=Vector3.Cross(verts[tris[q+1]]-verts[tris[q]],verts[tris[q+2]]-verts[tris[q]]);if(n.y<0){int tmp=tris[q+1];tris[q+1]=tris[q+2];tris[q+2]=tmp;}}
     // curtains: every edge of the patch with nothing beyond it, where the patch stands clear of the ground, drops to the ground (both faces)
     int curtainEdges=0;var topCount=verts.Count;var curtainVerts=new Dictionary<int,int>();int Drop(int i){if(curtainVerts.TryGetValue(i,out var d))return d;d=verts.Count;verts.Add(new Vector3(verts[i].x,groundY[i]-.3f,verts[i].z));groundY.Add(groundY[i]);fillsAt.Add(false);curtainVerts[i]=d;return d;}
     bool Inc(int r,int k)=>r>=0&&k>=0&&r<rowsS.Count-1&&k<cols-1&&included[r,k];
     for(int r=0;r+1<rowsS.Count;r++)for(int k=0;k+1<cols;k++){if(!included[r,k])continue;int i0=r*cols+k,i1=i0+1,i2=i0+cols,i3=i2+1;
      void Edge(bool beyond,int p,int q){if(beyond)return;if(verts[p].y-groundY[p]<.4f&&verts[q].y-groundY[q]<.4f)return;int dp=Drop(p),dq=Drop(q);tris.AddRange(new[]{p,q,dq,p,dq,dp,p,dq,q,p,dp,dq});curtainEdges++;}
      Edge(Inc(r-1,k),i0,i1);Edge(Inc(r+1,k),i2,i3);Edge(Inc(r,k-1),i0,i2);Edge(Inc(r,k+1),i1,i3);}
     material=matCount.OrderByDescending(m=>m.Value).Select(m=>m.Key).FirstOrDefault();
     log.Add($"   {label}: patch {rowsS.Count}x{cols}, {tris.Count/3} triangles, {curtainEdges} curtain edges, material {(material?material.name:"none")}");
     if(dry||tris.Count==0)continue;
     var mesh=new Mesh{name=$"{scene}-Report096-{name}-{label}",indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};mesh.SetVertices(verts);mesh.SetTriangles(tris,0);mesh.RecalculateNormals();mesh.RecalculateBounds();mesh.SetColors(verts.Select(_=>new Color32(89,102,77,255)).ToList());
     Directory.CreateDirectory("Assets/Scenery/Report096");AssetDatabase.CreateAsset(mesh,$"Assets/Scenery/Report096/{mesh.name}.asset");
     var go=new GameObject($"Ground_Report096 fill {name} {label}");go.AddComponent<MeshFilter>().sharedMesh=mesh;var mrr=go.AddComponent<MeshRenderer>();if(material)mrr.sharedMaterial=material;go.AddComponent<MeshCollider>().sharedMesh=mesh;
     UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go,sc);
    }
   }
   if(!dry){Physics.SyncTransforms();EditorSceneManager.MarkSceneDirty(sc);EditorSceneManager.SaveScene(sc);}}
  Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/fill.txt",log);AssetDatabase.SaveAssets();
 }
 // a pit (a crater or bowl in the ground): the ground inside a disc is brought up to the surface that joins its rim (Laplace relaxation from the rim), where that is higher
 static void Pit(UnityEngine.SceneManagement.Scene sc,string name,float cx,float cz,float R,bool dry,List<string> log,string scene){
  const float step=1f;int n=Mathf.CeilToInt(2*R/step)+1;var g=new float[n,n];var cols=new Collider[n,n];var inside=new bool[n,n];var rim=new bool[n,n];float y0=0;
  var probe=GroundBelow(new Vector3(cx,2000,cz),out _);y0=float.IsNaN(probe)?100:probe;
  for(int i=0;i<n;i++)for(int j=0;j<n;j++){float x=cx-R+i*step,z=cz-R+j*step;float d=Mathf.Sqrt((x-cx)*(x-cx)+(z-cz)*(z-cz));inside[i,j]=d<=R;rim[i,j]=inside[i,j]&&d>R-2.2f;g[i,j]=GroundBelow(new Vector3(x,y0+80,z),out cols[i,j]);if(float.IsNaN(g[i,j]))g[i,j]=y0-30;}
  var H=(float[,])g.Clone();float sum=0;int cnt=0;for(int i=0;i<n;i++)for(int j=0;j<n;j++)if(rim[i,j]){sum+=g[i,j];cnt++;}for(int i=0;i<n;i++)for(int j=0;j<n;j++)if(inside[i,j]&&!rim[i,j])H[i,j]=cnt>0?sum/cnt:y0;
  for(int it=0;it<1200;it++){var N=(float[,])H.Clone();for(int i=1;i<n-1;i++)for(int j=1;j<n-1;j++){if(!inside[i,j]||rim[i,j])continue;N[i,j]=(H[i-1,j]+H[i+1,j]+H[i,j-1]+H[i,j+1])/4;}H=N;}
  var fills=new bool[n,n];float deepest=0;int fcount=0;for(int i=0;i<n;i++)for(int j=0;j<n;j++){if(!inside[i,j]||rim[i,j])continue;if(H[i,j]>g[i,j]+.15f){fills[i,j]=true;fcount++;deepest=Mathf.Max(deepest,H[i,j]-g[i,j]);}}
  log.Add($"{scene} pit {name} at ({cx:F0},{cz:F0}) r {R:F0}: {fcount} cells to fill, the deepest {deepest:F1} m");
  if(dry||fcount==0)return;
  var verts=new List<Vector3>();var idx=new int[n,n];var mats=new Dictionary<Material,int>();
  for(int i=0;i<n;i++)for(int j=0;j<n;j++){idx[i,j]=-1;if(!inside[i,j])continue;idx[i,j]=verts.Count;float x=cx-R+i*step,z=cz-R+j*step;verts.Add(new Vector3(x,fills[i,j]?H[i,j]:g[i,j]+.025f,z));if(cols[i,j]&&(i+j)%3==0){var mr=cols[i,j].GetComponent<MeshRenderer>();if(mr&&mr.sharedMaterial){mats.TryGetValue(mr.sharedMaterial,out var q);mats[mr.sharedMaterial]=q+1;}}}
  var tris=new List<int>();for(int i=0;i+1<n;i++)for(int j=0;j+1<n;j++){int a=idx[i,j],b=idx[i+1,j],c=idx[i,j+1],d=idx[i+1,j+1];if(a<0||b<0||c<0||d<0)continue;if(!(fills[i,j]||fills[i+1,j]||fills[i,j+1]||fills[i+1,j+1]))continue;tris.AddRange(new[]{a,c,b,b,c,d});}
  var mesh=new Mesh{name=$"{scene}-Report096-{name}",indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};mesh.SetVertices(verts);mesh.SetTriangles(tris,0);mesh.RecalculateNormals();mesh.RecalculateBounds();mesh.SetColors(verts.Select(_=>new Color32(89,102,77,255)).ToList());
  Directory.CreateDirectory("Assets/Scenery/Report096");AssetDatabase.CreateAsset(mesh,$"Assets/Scenery/Report096/{mesh.name}.asset");
  var go=new GameObject($"Ground_Report096 fill {name}");go.AddComponent<MeshFilter>().sharedMesh=mesh;var mrr=go.AddComponent<MeshRenderer>();var mat=mats.OrderByDescending(m=>m.Value).Select(m=>m.Key).FirstOrDefault();if(mat)mrr.sharedMaterial=mat;go.AddComponent<MeshCollider>().sharedMesh=mesh;UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go,sc);
 }
 static Material material;
}
// ---- down-facing triangles in the ground tiles (a single-sided renderer shows nothing there: sky through a "hole" that has collision)
public static class Report096Flip {
 static string Out=>Environment.GetEnvironmentVariable("PROBE_OUT");
 public static void Scan(){
  EditorSceneManager.OpenScene("Assets/Scenes/"+Environment.GetEnvironmentVariable("PROBE_SCENES")+".unity");var log=new List<string>();var pts=Environment.GetEnvironmentVariable("WORLD_POINTS").Split('|').Select(s=>{var a=s.Split(',').Select(x=>float.Parse(x,System.Globalization.CultureInfo.InvariantCulture)).ToArray();return new Vector3(a[0],a[1],a[2]);}).ToArray();
  int total=0;foreach(var mf in UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Exclude,FindObjectsSortMode.None)){if(!mf.sharedMesh||!mf.name.StartsWith("Ground_")||!mf.GetComponent<MeshRenderer>())continue;
   var m=mf.transform.localToWorldMatrix;var v=mf.sharedMesh.vertices.Select(x=>m.MultiplyPoint3x4(x)).ToArray();var t=mf.sharedMesh.triangles;int down=0,thin=0;var near=new List<string>();
   for(int i=0;i<t.Length;i+=3){var n=Vector3.Cross(v[t[i+1]]-v[t[i]],v[t[i+2]]-v[t[i]]);float area=n.magnitude/2;if(area<1e-6f)continue;n.Normalize();if(n.y<-.02f){down++;var c=(v[t[i]]+v[t[i+1]]+v[t[i+2]])/3;foreach(var p in pts)if(Vector3.Distance(new Vector3(c.x,0,c.z),new Vector3(p.x,0,p.z))<25)near.Add($"({c.x:F1},{c.y:F1},{c.z:F1}) ny {n.y:F2} area {area:F3}");}}
   total+=down;if(down>0)log.Add($"{mf.name} [{mf.sharedMesh.name}] {t.Length/3} triangles, {down} face down, near the points: {near.Count}");foreach(var s in near.Take(30))log.Add("    "+s);}
  log.Add("total down-facing ground triangles: "+total);Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/flip.txt",log);}
}
// ---- cracks inside one ground mesh: boundary edges near a point and what lies on them (T-junctions)
public static class Report096Cracks {
 static string Out=>Environment.GetEnvironmentVariable("PROBE_OUT");
 public static void Scan(){
  EditorSceneManager.OpenScene("Assets/Scenes/"+Environment.GetEnvironmentVariable("PROBE_SCENES")+".unity");var log=new List<string>();
  var a=Environment.GetEnvironmentVariable("WORLD_POINTS").Split(',').Select(x=>float.Parse(x,System.Globalization.CultureInfo.InvariantCulture)).ToArray();var c=new Vector3(a[0],a[1],a[2]);float r=a[3];string tile=Environment.GetEnvironmentVariable("WORLD_TILE");
  var mf=UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Exclude,FindObjectsSortMode.None).First(x=>x.name==tile);var m=mf.transform.localToWorldMatrix;var mesh=mf.sharedMesh;
  var v=mesh.vertices.Select(x=>m.MultiplyPoint3x4(x)).ToArray();var t=mesh.triangles;log.Add($"{tile} [{mesh.name}] {v.Length} vertices {t.Length/3} triangles, submeshes {mesh.subMeshCount}");
  // weld by position
  var weld=new Dictionary<(int,int,int),int>();var map=new int[v.Length];for(int i=0;i<v.Length;i++){var k=(Mathf.RoundToInt(v[i].x*1000),Mathf.RoundToInt(v[i].y*1000),Mathf.RoundToInt(v[i].z*1000));if(!weld.TryGetValue(k,out var q))weld[k]=q=i;map[i]=q;}
  log.Add($"distinct positions {weld.Count} of {v.Length} vertices (welded to 1 mm)");
  var edges=new Dictionary<(int,int),int>();for(int i=0;i<t.Length;i+=3)for(int k=0;k<3;k++){int p=map[t[i+k]],q=map[t[i+(k+1)%3]];if(p==q)continue;var key=p<q?(p,q):(q,p);edges.TryGetValue(key,out var n);edges[key]=n+1;}
  var boundary=edges.Where(e=>e.Value==1).Select(e=>e.Key).ToList();log.Add($"boundary edges {boundary.Count}, edges used 3+ times {edges.Count(e=>e.Value>2)}");
  var near=boundary.Where(e=>{var mid=(v[e.Item1]+v[e.Item2])/2;return Vector3.Distance(new Vector3(mid.x,0,mid.z),new Vector3(c.x,0,c.z))<r;}).ToList();log.Add($"boundary edges within {r} m of the point: {near.Count}");
  foreach(var e in near.Take(80)){var p=v[e.Item1];var q=v[e.Item2];
   // the closest other vertex to the edge's interior
   float best=float.MaxValue;int bi=-1;var d=q-p;float L=d.magnitude;for(int i=0;i<v.Length;i++){if(map[i]==e.Item1||map[i]==e.Item2)continue;var w=v[i]-p;float s=Vector3.Dot(w,d)/(L*L);if(s<.02f||s>.98f)continue;float dist=(w-d*s).magnitude;if(dist<best){best=dist;bi=i;}}
   log.Add($"  ({p.x:F2},{p.y:F2},{p.z:F2})-({q.x:F2},{q.y:F2},{q.z:F2}) len {L:F2}; nearest other vertex to its interior {best*100:F1} cm");}
  Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/cracks.txt",log);}
}
// ---- crack repair: T-junctions where a coarse triangle's edge has finer vertices lying on it (the LOD steps in the ground tiles), which leave
// hairline gaps (sky through them). Each such triangle is re-cut as a fan through those vertices (conforming to the finer side); nothing else moves.
public static class Report096Weld {
 static string Out=>Environment.GetEnvironmentVariable("PROBE_OUT");
 static bool Dry=>Environment.GetEnvironmentVariable("WORLD_DRY")=="1";
 static (int,int,int) PK(Vector3 p)=>(Mathf.RoundToInt(p.x*1000),Mathf.RoundToInt(p.y*1000),Mathf.RoundToInt(p.z*1000));
 public static void Run(){
  var sc=EditorSceneManager.OpenScene("Assets/Scenes/"+Environment.GetEnvironmentVariable("PROBE_SCENES")+".unity");var log=new List<string>();int fixedTiles=0,totalSplit=0,totalBoundaryBefore=0,totalBoundaryAfter=0,totalNew=0;
  var only=Environment.GetEnvironmentVariable("WORLD_TILE");
  var tiles=UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Exclude,FindObjectsSortMode.None).Where(mf=>mf.sharedMesh&&mf.name.StartsWith("Ground_")&&mf.GetComponent<MeshRenderer>()&&!mf.name.StartsWith("Ground_Edge apron")&&mf.sharedMesh.subMeshCount==1).OrderBy(x=>x.name).ToList();
  // every vertex of every ground tile, by 2 m cell (a vertex of a neighbouring tile may lie on this tile's edge too)
  var global=new Dictionary<(int,int),List<Vector3>>();var seen=new HashSet<(int,int,int)>();
  foreach(var mf in tiles){var m=mf.transform.localToWorldMatrix;foreach(var lv in mf.sharedMesh.vertices){var w=m.MultiplyPoint3x4(lv);if(!seen.Add(PK(w)))continue;var key=(Mathf.FloorToInt(w.x/2),Mathf.FloorToInt(w.z/2));if(!global.TryGetValue(key,out var l))global[key]=l=new List<Vector3>();l.Add(w);}}
  log.Add($"{tiles.Count} ground tiles, {seen.Count} distinct vertex positions");
  foreach(var mf in tiles){
   if(!string.IsNullOrEmpty(only)&&mf.name!=only)continue;
   var mesh=mf.sharedMesh;var m=mf.transform.localToWorldMatrix;var inv=mf.transform.worldToLocalMatrix;var lv=mesh.vertices;var ln=mesh.normals;var luv=mesh.uv;var v=lv.Select(x=>m.MultiplyPoint3x4(x)).ToList();var t=mesh.triangles;
   var weld=new Dictionary<(int,int,int),int>();var map=new int[v.Count];for(int i=0;i<v.Count;i++){var k=PK(v[i]);if(!weld.TryGetValue(k,out var q))weld[k]=q=i;map[i]=q;}
   var edgeCount=new Dictionary<(int,int),int>();for(int i=0;i<t.Length;i+=3)for(int k=0;k<3;k++){int p=map[t[i+k]],q=map[t[i+(k+1)%3]];if(p==q)continue;var key=p<q?(p,q):(q,p);edgeCount.TryGetValue(key,out var n);edgeCount[key]=n+1;}
   var boundary=new HashSet<(int,int)>(edgeCount.Where(e=>e.Value==1).Select(e=>e.Key));totalBoundaryBefore+=boundary.Count;if(boundary.Count==0)continue;
   var inserts=new Dictionary<(int,int),List<(float s,Vector3 w)>>();
   foreach(var e in boundary){var p=v[e.Item1];var q=v[e.Item2];var d=q-p;d.y=0;float L=d.magnitude;if(L<.05f)continue;var dir=d/L;var list=new List<(float,Vector3)>();var got=new HashSet<(int,int,int)>();
    int x0=Mathf.FloorToInt(Mathf.Min(p.x,q.x)/2),x1=Mathf.FloorToInt(Mathf.Max(p.x,q.x)/2),z0=Mathf.FloorToInt(Mathf.Min(p.z,q.z)/2),z1=Mathf.FloorToInt(Mathf.Max(p.z,q.z)/2);
    for(int gx=x0;gx<=x1;gx++)for(int gz=z0;gz<=z1;gz++)if(global.TryGetValue((gx,gz),out var cell))foreach(var wv in cell){var w=wv-p;w.y=0;float sp=Vector3.Dot(w,dir);if(sp<.02f||sp>L-.02f)continue;float off=(w-dir*sp).magnitude;if(off>.003f)continue;
      float ey=Mathf.Lerp(p.y,q.y,sp/L);if(Mathf.Abs(wv.y-ey)>1.5f)continue;if(PK(wv).Equals(PK(p))||PK(wv).Equals(PK(q)))continue;if(!got.Add(PK(wv)))continue;list.Add((sp/L,wv));}
    if(list.Count>0)inserts[e]=list.OrderBy(x=>x.Item1).ToList();}
   if(inserts.Count==0)continue;
   var newPos=new Dictionary<(int,int,int),int>();var addV=new List<Vector3>();var addN=new List<Vector3>();var addUV=new List<Vector2>();
   int Index(Vector3 w,int ea,int eb,float sp){var k=PK(w);if(weld.TryGetValue(k,out var existing))return existing;if(newPos.TryGetValue(k,out var made))return made;
    int idx=v.Count+addV.Count;addV.Add(inv.MultiplyPoint3x4(w));addN.Add(ln.Length>ea&&ln.Length>eb?Vector3.Lerp(ln[ea],ln[eb],sp).normalized:Vector3.up);addUV.Add(luv.Length>ea&&luv.Length>eb?Vector2.Lerp(luv[ea],luv[eb],sp):Vector2.zero);newPos[k]=idx;return idx;}
   var vAll=new Func<int,Vector3>(i=>i<v.Count?v[i]:m.MultiplyPoint3x4(addV[i-v.Count]));
   var newTris=new List<int>();int split=0;
   for(int i=0;i<t.Length;i+=3){int a=t[i],b=t[i+1],c=t[i+2];var poly=new List<int>();bool any=false;
    for(int k=0;k<3;k++){int p=t[i+k],q=t[i+(k+1)%3];poly.Add(p);int mp=map[p],mq=map[q];var key=mp<mq?(mp,mq):(mq,mp);if(inserts.TryGetValue(key,out var ins)){var seq=mp<mq?ins:Enumerable.Reverse(ins).ToList();foreach(var x in seq){float sp=mp<mq?x.s:1-x.s;poly.Add(Index(x.w,p,q,sp));}any=true;}}
    if(!any){newTris.AddRange(new[]{a,b,c});continue;}split++;
    int bestApex=-1;float bestScore=-1;var origN=Vector3.Cross(v[b]-v[a],v[c]-v[a]);
    for(int k=0;k<poly.Count;k++){if(!(poly[k]==a||poly[k]==b||poly[k]==c))continue;float total=0;bool ok=true;for(int j=1;j+1<poly.Count;j++){var pa=vAll(poly[(k+j)%poly.Count]);var pb=vAll(poly[(k+j+1)%poly.Count]);var n=Vector3.Cross(pa-vAll(poly[k]),pb-vAll(poly[k]));if(Vector3.Dot(n,origN)<-1e-9f){ok=false;break;}if(n.magnitude>1e-7f)total+=1000+n.magnitude;}if(ok&&total>bestScore){bestScore=total;bestApex=k;}}
    if(bestApex<0)bestApex=0;
    for(int j=1;j+1<poly.Count;j++){int pa=poly[(bestApex+j)%poly.Count],pb=poly[(bestApex+j+1)%poly.Count],p0=poly[bestApex];if(Vector3.Cross(vAll(pa)-vAll(p0),vAll(pb)-vAll(p0)).magnitude<1e-7f)continue;newTris.AddRange(new[]{p0,pa,pb});}}
   // boundary edges left (counting new vertices by position)
   int Map(int i)=>i<map.Length?map[i]:i;var ec2=new Dictionary<(int,int),int>();for(int i=0;i<newTris.Count;i+=3)for(int k=0;k<3;k++){int p=Map(newTris[i+k]),q=Map(newTris[i+(k+1)%3]);if(p==q)continue;var key=p<q?(p,q):(q,p);ec2.TryGetValue(key,out var n);ec2[key]=n+1;}
   int after=ec2.Count(e=>e.Value==1);totalBoundaryAfter+=after;totalSplit+=split;totalNew+=addV.Count;fixedTiles++;
   log.Add($"{mf.name}: {boundary.Count} boundary edges, {inserts.Count} with vertices on them; {split} triangles re-cut ({addV.Count} vertices added); {after} boundary edges left");
   if(Dry)continue;
   var copy=UnityEngine.Object.Instantiate(mesh);copy.name="FreeRoamWorld-"+mf.name+"-conformed";copy.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;
   var nv=new List<Vector3>(lv);nv.AddRange(addV);copy.SetVertices(nv);if(ln.Length==lv.Length){var nn=new List<Vector3>(ln);nn.AddRange(addN);copy.SetNormals(nn);}if(luv.Length==lv.Length){var nu=new List<Vector2>(luv);nu.AddRange(addUV);copy.SetUVs(0,nu);}
   copy.SetTriangles(newTris,0);copy.RecalculateBounds();
   Directory.CreateDirectory("Assets/Scenery/Report096");AssetDatabase.CreateAsset(copy,"Assets/Scenery/Report096/"+copy.name+".asset");
   bool sameCollider=false;var mc=mf.GetComponent<MeshCollider>();if(mc&&mc.sharedMesh==mesh){mc.sharedMesh=copy;sameCollider=true;}mf.sharedMesh=copy;log.Add($"   mesh {copy.name} saved; collider {(sameCollider?"uses it":"unchanged")}");}
  log.Add($"tiles repaired {fixedTiles}; triangles re-cut {totalSplit}; vertices added {totalNew}; boundary edges before {totalBoundaryBefore} after {totalBoundaryAfter} (tiles with T-junctions only)");
  if(!Dry){EditorSceneManager.MarkSceneDirty(sc);EditorSceneManager.SaveScene(sc);AssetDatabase.SaveAssets();}
  Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/weld.txt",log);}
}
// ---- BUG-004: roads and trails that leave the race route and are not the main road or a shortcut get a timber barrier with a ROAD CLOSED sign
// 22 m beyond where they leave (so it reads before you reach it), in the race scenes only (Free Roam is its own scene and keeps them open).
public static class Report096Close {
 static string Out=>Environment.GetEnvironmentVariable("PROBE_OUT");
 static bool Dry=>Environment.GetEnvironmentVariable("WORLD_DRY")=="1";
 static float Dist(Vector3 p,List<Racer.RaceRoad> roads,List<Racer.WoodlandRoute> trails){float best=float.MaxValue;foreach(var r in roads){float s=r.Project(p,out float lat);var c=r.At(s,out _);if(Mathf.Abs(c.y-p.y)<9)best=Mathf.Min(best,lat);}foreach(var w in trails){float s=w.Project(p,out float lat);var c=w.At(s,out _);if(Mathf.Abs(c.y-p.y)<9)best=Mathf.Min(best,lat);}return best;}
 static Material Mat(string name,Color c){string path="Assets/Scenery/Report096/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(m)return m;Directory.CreateDirectory("Assets/Scenery/Report096");m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.name=name;m.SetColor("_BaseColor",c);m.SetFloat("_Smoothness",.1f);AssetDatabase.CreateAsset(m,path);return m;}
 static GameObject Cube(string name,Transform parent,Vector3 local,Vector3 size,Material m,bool collider){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=local;g.transform.localScale=size;g.GetComponent<MeshRenderer>().sharedMaterial=m;if(!collider)UnityEngine.Object.DestroyImmediate(g.GetComponent<BoxCollider>());return g;}
 static void Build(Transform parent,string name,Vector3 at,Vector3 f,float half){
  var root=new GameObject(name).transform;root.SetParent(parent,false);root.position=at;root.rotation=Quaternion.LookRotation(f);
  var wood=Mat("RoadClosedTimber",new Color(.42f,.29f,.17f));var panel=Mat("RoadClosedSign",new Color(.55f,.08f,.06f));float w=2*(half+1.2f);
  Cube("Post left",root,new Vector3(-w/2,.65f,0),new Vector3(.26f,1.3f,.26f),wood,false);Cube("Post right",root,new Vector3(w/2,.65f,0),new Vector3(.26f,1.3f,.26f),wood,false);
  Cube("Rail low",root,new Vector3(0,.5f,0),new Vector3(w,.2f,.14f),wood,false);Cube("Rail high",root,new Vector3(0,.95f,0),new Vector3(w,.2f,.14f),wood,false);
  var block=Cube("Barrier collision",root,new Vector3(0,.7f,0),new Vector3(w,1.4f,.3f),wood,true);block.GetComponent<MeshRenderer>().enabled=false;
  Cube("Sign backing",root,new Vector3(0,1.55f,0),new Vector3(1.7f,.6f,.06f),panel,false);Cube("Sign post",root,new Vector3(0,.8f,.04f),new Vector3(.12f,1.5f,.1f),wood,false);
  { var t=new GameObject("Road closed lettering").AddComponent<TextMesh>();t.transform.SetParent(root,false);t.transform.localPosition=new Vector3(0,1.55f,-.04f);t.transform.localRotation=Quaternion.Euler(0,180,0);
   t.text="ROAD CLOSED";t.anchor=TextAnchor.MiddleCenter;t.alignment=TextAlignment.Center;t.characterSize=.1f;t.fontSize=48;t.color=Color.white;t.fontStyle=FontStyle.Bold; }
 }
 public static void Run(){
  var log=new List<string>();
  foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){
   var sc=EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();var race=UnityEngine.Object.FindAnyObjectByType<Racer.RaceDirector>();
   var all=UnityEngine.Object.FindObjectsByType<Racer.RaceRoad>(FindObjectsInactive.Exclude,FindObjectsSortMode.None).Where(r=>r.points!=null&&r.points.Length>3).ToList();foreach(var r in all)r.Initialize();
   var trails=UnityEngine.Object.FindObjectsByType<Racer.WoodlandRoute>(FindObjectsInactive.Exclude,FindObjectsSortMode.None).Where(w=>w.points!=null&&w.points.Length>3).ToList();foreach(var w in trails)w.Initialize();
   var route=new List<Racer.RaceRoad>{race.road};var others=all.Where(r=>r!=race.road&&r.forestTrail&&!r.openHighway&&r.Length>60&&r.name.IndexOf("driveway",StringComparison.OrdinalIgnoreCase)<0).ToList();var placed=new List<Vector3>();
   log.Add($"==== {scene}: race road '{race.road.name}', {trails.Count} shortcut trails, {others.Count} exploration trails to look at");
   if(!Dry){var old=GameObject.Find("Closed roads (race only)");if(old)UnityEngine.Object.DestroyImmediate(old);}
   Transform parent=null;int made=0;
   foreach(var road in others){
    var runs=new List<(float a,float b)>();bool inRun=false;float start=0;const float step=2;var inside=new List<bool>();var ss=new List<float>();
    for(float s=0;s<road.Length;s+=step){var p=road.At(s,out _);inside.Add(Dist(p,route,trails)<12);ss.Add(s);}
    for(int i=0;i<ss.Count;i++){if(!inside[i]&&!inRun){inRun=true;start=ss[i];}if((inside[i]||i==ss.Count-1)&&inRun){inRun=false;runs.Add((start,ss[i]));}}
    foreach(var run in runs){
     float len=run.b-run.a;if(len<45)continue;
     var pa=road.At(Mathf.Max(0,run.a-4),out _);var pb=road.At(Mathf.Min(road.Length-.1f,run.b+4),out _);bool touchA=run.a>step&&Dist(pa,route,trails)<14,touchB=run.b<road.Length-step&&Dist(pb,route,trails)<14;
     var ends=new List<(float s,bool leaves)>();if(touchA)ends.Add((run.a+22f,true));if(touchB)ends.Add((run.b-22f,true));
     foreach(var end in ends){
      float s=Mathf.Clamp(end.s,run.a+8,run.b-8);var at=road.At(s,out var f);f.y=0;f.Normalize();float d=Dist(at,route,trails);
      if(d<18){log.Add($"   {road.name}: barrier spot ({at.x:F0},{at.y:F0},{at.z:F0}) is only {d:F0} m from the route: skipped");continue;}
      if(Physics.Raycast(new Vector3(at.x,at.y+30,at.z),Vector3.down,out var hit,80,~0,QueryTriggerInteraction.Ignore))at.y=hit.point.y;
      if(placed.Any(q=>Vector3.Distance(q,at)<30)){log.Add($"   {road.name}: ({at.x:F0},{at.y:F0},{at.z:F0}) already has a barrier within 30 m");continue;}placed.Add(at);
      float half=road.HalfWidth(s);made++;
      log.Add($"   {road.name}: outside run {run.a:F0}..{run.b:F0} ({len:F0} m) leaves the route; barrier {made} at ({at.x:F1},{at.y:F1},{at.z:F1}) across {2*(half+1.2f):F1} m heading {Vector3.SignedAngle(Vector3.forward,f,Vector3.up):F0}");
      if(Dry)continue;
      if(!parent){parent=new GameObject("Closed roads (race only)").transform;UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(parent.gameObject,sc);}
      Build(parent,$"Road closed barrier {made}",at,f,half);
     }
    }
   }
   if(!Dry&&made>0){EditorSceneManager.MarkSceneDirty(sc);EditorSceneManager.SaveScene(sc);}
   log.Add($"   {scene}: {made} barrier(s){(Dry?" (dry run)":"")}");
  }
  if(!Dry)AssetDatabase.SaveAssets();Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/close.txt",log);
 }
}
// ---- BUG-002: the storm culvert's shell against the terrain around the bug (heights by collider name)
public static class Report096Box {
 static string Out=>Environment.GetEnvironmentVariable("PROBE_OUT");
 static float Hit(Vector3 o,Func<string,bool> pick){var hits=Physics.RaycastAll(o,Vector3.down,300,~0,QueryTriggerInteraction.Ignore);float best=float.NaN;foreach(var h in hits){if(!pick(h.collider.name))continue;if(float.IsNaN(best)||h.point.y>best)best=h.point.y;}return best;}
 public static void Scan(){
  EditorSceneManager.OpenScene("Assets/Scenes/FreeRoamWorld.unity");Physics.SyncTransforms();var log=new List<string>();
  var a=Environment.GetEnvironmentVariable("WORLD_POINTS").Split(',').Select(x=>float.Parse(x,System.Globalization.CultureInfo.InvariantCulture)).ToArray();var c=new Vector3(a[0],a[1],a[2]);float r=a[3];
  log.Add("terrain height (Ground_400_400) / culvert shell top / culvert floor, every 3 m, relative to the bug's y; shell-minus-terrain shown (positive = the shell stands above the ground)");
  var rows=new List<string>();for(float z=r;z>=-r;z-=3){var row=$"z{c.z+z,6:F0} ";for(float x=-r;x<=r;x+=3){var o=new Vector3(c.x+x,c.y+60,c.z+z);float g=Hit(o,n=>n.StartsWith("Ground_4")||n.StartsWith("Ground_5")),s=Hit(o,n=>n.StartsWith("Long storm culvert")),f=Hit(o,n=>n.StartsWith("Ground_Culvert"));
     row+=float.IsNaN(s)?(float.IsNaN(g)?"  . ":" "+(g-c.y).ToString("F0").PadLeft(3)):" "+("["+(s-g).ToString("F0")+"]").PadLeft(4);}rows.Add(row);}
  log.AddRange(rows);log.Add("x from "+(c.x-r)+" step 3. [n] = shell top n m above the terrain; plain = terrain height relative to the bug where there is no shell");
  // the shell mesh itself: vertices within r, their heights against the terrain
  foreach(var mf in UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Exclude,FindObjectsSortMode.None)){if(!mf.sharedMesh||!mf.name.StartsWith("Long storm culvert"))continue;var m=mf.transform.localToWorldMatrix;var v=mf.sharedMesh.vertices.Select(x=>m.MultiplyPoint3x4(x)).Where(w=>Mathf.Abs(w.x-c.x)<r&&Mathf.Abs(w.z-c.z)<r).ToList();
   int above=0;float maxAbove=0;foreach(var w in v){float g=Hit(new Vector3(w.x,w.y+60,w.z),n=>n.StartsWith("Ground_4")||n.StartsWith("Ground_5"));if(float.IsNaN(g))continue;if(w.y>g+.1f){above++;maxAbove=Mathf.Max(maxAbove,w.y-g);}}
   log.Add($"{mf.name} [{mf.sharedMesh.name}]: {v.Count} vertices in the area, {above} above the terrain (up to {maxAbove:F1} m); mesh has {mf.sharedMesh.vertexCount} vertices, {mf.sharedMesh.triangles.Length/3} triangles; materials {string.Join(",",mf.GetComponent<MeshRenderer>().sharedMaterials.Select(x=>x?x.name:"null"))}");}
  Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/box.txt",log);}
}
// ---- one editor launch for several jobs (each launch costs minutes before it does anything)
public static class Report096Colours {
 // the vertices the crack repair added have no colour (the ground shader draws vertex colours): each takes the colour (and second UV set) of the nearest vertex that has one
 public static void Run(){
  EditorSceneManager.OpenScene("Assets/Scenes/FreeRoamWorld.unity");var log=new List<string>();int total=0;
  foreach(var mf in UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Exclude,FindObjectsSortMode.None)){var mesh=mf.sharedMesh;if(!mesh||!mesh.name.EndsWith("-conformed"))continue;
   var col=mesh.colors32;if(col.Length!=mesh.vertexCount)continue;var uv2=mesh.uv2;var v=mesh.vertices;var grid=new Dictionary<(int,int,int),List<int>>();
   for(int i=0;i<v.Length;i++)if(col[i].a>0){var k=((int)Mathf.Floor(v[i].x),(int)Mathf.Floor(v[i].y),(int)Mathf.Floor(v[i].z));if(!grid.TryGetValue(k,out var l))grid[k]=l=new List<int>();l.Add(i);}
   int fixedCount=0;for(int i=0;i<v.Length;i++){if(col[i].a>0)continue;int best=-1;float bd=float.MaxValue;var k0=((int)Mathf.Floor(v[i].x),(int)Mathf.Floor(v[i].y),(int)Mathf.Floor(v[i].z));
     for(int dx=-1;dx<=1;dx++)for(int dy=-1;dy<=1;dy++)for(int dz=-1;dz<=1;dz++)if(grid.TryGetValue((k0.Item1+dx,k0.Item2+dy,k0.Item3+dz),out var l))foreach(int j in l){float d=(v[j]-v[i]).sqrMagnitude;if(d<bd){bd=d;best=j;}}
     if(best>=0){col[i]=col[best];if(uv2.Length==v.Length)uv2[i]=uv2[best];fixedCount++;}}
   if(fixedCount>0){mesh.SetColors(new List<Color32>(col));if(uv2.Length==v.Length)mesh.SetUVs(1,new List<Vector2>(uv2));EditorUtility.SetDirty(mesh);total+=fixedCount;log.Add($"{mesh.name}: {fixedCount} colourless vertices given the colour of the nearest vertex");}}
  AssetDatabase.SaveAssets();log.Add("total "+total);Directory.CreateDirectory(Environment.GetEnvironmentVariable("PROBE_OUT"));File.WriteAllLines(Environment.GetEnvironmentVariable("PROBE_OUT")+"/colours.txt",log);}
}
public static class Report096Batch {
 public static void FillOnly(){string root=Environment.GetEnvironmentVariable("PROBE_OUT");Set("PROBE_OUT",root+"/fill");Set("WORLD_DRY","0");Set("PROBE_SCENES","MountainLoop,MountainLoopReverse");Set("WORLD_FILLS","B3,1055.64,134.36,35|B6,726.64,-119.49,35,22|B7,1000.96,93.88,35|P:B6pit,714,-126,14");try{Report096Fill.Run();}catch(Exception e){File.WriteAllText(root+"-fill-error.txt",e.ToString());}EditorApplication.Exit(0);}
 public static void FixAndLid(){Report096Colours.Run();Environment.SetEnvironmentVariable("WORLD_DRY","0");Environment.SetEnvironmentVariable("PROBE_OUT",Environment.GetEnvironmentVariable("PROBE_OUT")+"/lid");Report096Lid.Run();EditorApplication.Exit(0);}
 public static void MatProbe(){Report096Mat.Run();EditorApplication.Exit(0);}
 public static void LidDry(){Environment.SetEnvironmentVariable("WORLD_DRY","1");Report096Lid.Run();EditorApplication.Exit(0);}
 public static void LidApply(){Environment.SetEnvironmentVariable("WORLD_DRY","0");Report096Lid.Run();EditorApplication.Exit(0);}
 static void Set(string k,string v)=>Environment.SetEnvironmentVariable(k,v);
 public static void Analyse(){
  string root=Environment.GetEnvironmentVariable("PROBE_OUT");
  Set("PROBE_OUT",root+"/box");Set("PROBE_SCENES","FreeRoamWorld");Set("WORLD_POINTS","146,60,65,26");try{Report096Box.Scan();}catch(Exception e){File.WriteAllText(root+"-box-error.txt",e.ToString());}
  Set("PROBE_OUT",root+"/close");Set("WORLD_DRY","1");Set("PROBE_SCENES","MountainLoop,MountainLoopReverse,ForestLoopReverse,LakeWoods,DansBackyardForward,DansBackyardReverse,StreetLoopGreybox,StreetLoopReverse");try{Report096Close.Run();}catch(Exception e){File.WriteAllText(root+"-close-error.txt",e.ToString());}
  Set("PROBE_OUT",root+"/fill");Set("WORLD_DRY","1");Set("PROBE_SCENES","MountainLoop,MountainLoopReverse");Set("WORLD_FILLS","B3,1055.64,134.36,35|B6,726.64,-119.49,35,22|B7,1000.96,93.88,35|P:B6pit,714,-126,14");try{Report096Fill.Run();}catch(Exception e){File.WriteAllText(root+"-fill-error.txt",e.ToString());}
  EditorApplication.Exit(0);}
 public static void Apply(){
  string root=Environment.GetEnvironmentVariable("PROBE_OUT");
  Set("PROBE_OUT",root+"/close");Set("WORLD_DRY","0");try{Report096Close.Run();}catch(Exception e){File.WriteAllText(root+"-close-error.txt",e.ToString());}
  Set("PROBE_OUT",root+"/fill");Set("WORLD_DRY","0");Set("PROBE_SCENES","MountainLoop,MountainLoopReverse");Set("WORLD_FILLS","B3,1055.64,134.36,35|B6,726.64,-119.49,35,22|B7,1000.96,93.88,35|P:B6pit,714,-126,14");try{Report096Fill.Run();}catch(Exception e){File.WriteAllText(root+"-fill-error.txt",e.ToString());}
  EditorApplication.Exit(0);}
}
// ---- BUG-002: the culvert's dark shell shows through the hole cut in the ground (and has no collision from above). A lid of ground over every
// cell where the ground is cut away and the shell lies below: terrain-matched height, never lower than the shell top + 6 cm; the shell and the
// tunnel under it are untouched.
public static class Report096Lid {
 static string Out=>Environment.GetEnvironmentVariable("PROBE_OUT");
 const float Cell=.5f;
 static float Terrain(float x,float y,float z,out Collider col){col=null;float best=float.NaN;foreach(var h in Physics.RaycastAll(new Vector3(x,y,z),Vector3.down,300,~0,QueryTriggerInteraction.Ignore)){var n=h.collider.name;if(!(h.collider is MeshCollider)||!n.StartsWith("Ground_")||n.StartsWith("Ground_Culvert")||n.StartsWith("Ground_Report096")||n.StartsWith("Ground_Edge apron"))continue;if(float.IsNaN(best)||h.point.y>best){best=h.point.y;col=h.collider;}}return best;}
 public static void Run(){
  bool dry=Environment.GetEnvironmentVariable("WORLD_DRY")=="1";var log=new List<string>();
  var sc=EditorSceneManager.OpenScene("Assets/Scenes/FreeRoamWorld.unity");if(!dry){var oldLid=GameObject.Find("Ground_Report096 culvert lid");if(oldLid)UnityEngine.Object.DestroyImmediate(oldLid);}Physics.SyncTransforms();
  var shell=UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Exclude,FindObjectsSortMode.None).First(m=>m.name.StartsWith("Long storm culvert"));var mm=shell.transform.localToWorldMatrix;var sv=shell.sharedMesh.vertices.Select(v=>mm.MultiplyPoint3x4(v)).ToArray();var st=shell.sharedMesh.triangles;
  var b=new Bounds(sv[0],Vector3.zero);foreach(var v in sv)b.Encapsulate(v);int nx=Mathf.CeilToInt(b.size.x/Cell)+8,nz=Mathf.CeilToInt(b.size.z/Cell)+8;float x0=b.min.x-4*Cell,z0=b.min.z-4*Cell;
  float[,] ComputeTop(Vector3[] sv2){var top2=new float[nx,nz];for(int i=0;i<nx;i++)for(int j=0;j<nz;j++)top2[i,j]=float.NaN;
   for(int q=0;q<st.Length;q+=3){var a=sv2[st[q]];var bb=sv2[st[q+1]];var c=sv2[st[q+2]];var tn=Vector3.Cross(bb-a,c-a).normalized;if(tn.y>-.4f)continue; // ceilings only (they face down into the tunnel)
    int ix0=Mathf.Max(0,Mathf.FloorToInt((Mathf.Min(a.x,Mathf.Min(bb.x,c.x))-x0)/Cell)),ix1=Mathf.Min(nx-1,Mathf.CeilToInt((Mathf.Max(a.x,Mathf.Max(bb.x,c.x))-x0)/Cell));int iz0=Mathf.Max(0,Mathf.FloorToInt((Mathf.Min(a.z,Mathf.Min(bb.z,c.z))-z0)/Cell)),iz1=Mathf.Min(nz-1,Mathf.CeilToInt((Mathf.Max(a.z,Mathf.Max(bb.z,c.z))-z0)/Cell));
    float den=(bb.z-c.z)*(a.x-c.x)+(c.x-bb.x)*(a.z-c.z);if(Mathf.Abs(den)<1e-6f)continue;
    for(int i=ix0;i<=ix1;i++)for(int j=iz0;j<=iz1;j++){float px=x0+(i+.5f)*Cell,pz=z0+(j+.5f)*Cell;float w1=((bb.z-c.z)*(px-c.x)+(c.x-bb.x)*(pz-c.z))/den,w2=((c.z-a.z)*(px-c.x)+(a.x-c.x)*(pz-c.z))/den,w3=1-w1-w2;if(w1<-.001f||w2<-.001f||w3<-.001f)continue;float y=w1*a.y+w2*bb.y+w3*c.y;if(float.IsNaN(top2[i,j])||y>top2[i,j])top2[i,j]=y;}}
   return top2;}
  var top=ComputeTop(sv);
  var T=new float[nx,nz];var cut=new bool[nx,nz];var mats=new Dictionary<Material,int>();float yTop=b.max.y+20;int cutCount=0;
  for(int i=0;i<nx;i++)for(int j=0;j<nz;j++){float px=x0+(i+.5f)*Cell,pz=z0+(j+.5f)*Cell;T[i,j]=Terrain(px,yTop,pz,out var col);if(!float.IsNaN(top[i,j])&&(float.IsNaN(T[i,j])||T[i,j]<top[i,j]-.2f)){cut[i,j]=true;T[i,j]=float.NaN;cutCount++;}else if(col&&(i+j)%11==0){var mr=col.GetComponent<MeshRenderer>();if(mr&&mr.sharedMaterial){mats.TryGetValue(mr.sharedMaterial,out var q);mats[mr.sharedMaterial]=q+1;}}}
  log.Add($"shell {shell.name}: bounds {b.min:F0}..{b.max:F0}; grid {nx}x{nz} cells of {Cell} m; {cutCount} cells where the ground is cut away over the shell");
  if(cutCount==0){Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/lid.txt",log);return;}
  // region = cut cells + 3 cells of margin; the lid's base height by relaxation from the surrounding terrain
  var inRegion=new bool[nx,nz];for(int i=0;i<nx;i++)for(int j=0;j<nz;j++)if(cut[i,j])for(int di=-3;di<=3;di++)for(int dj=-3;dj<=3;dj++){int a=i+di,c=j+dj;if(a>=0&&c>=0&&a<nx&&c<nz)inRegion[a,c]=true;}
  var H=new float[nx,nz];var known=new bool[nx,nz];float sum=0;int cnt=0;for(int i=0;i<nx;i++)for(int j=0;j<nz;j++)if(!float.IsNaN(T[i,j])){H[i,j]=T[i,j];known[i,j]=true;sum+=T[i,j];cnt++;}
  for(int i=0;i<nx;i++)for(int j=0;j<nz;j++)if(!known[i,j])H[i,j]=cnt>0?sum/cnt:0;
  for(int it=0;it<900;it++){var N=(float[,])H.Clone();for(int i=1;i<nx-1;i++)for(int j=1;j<nz-1;j++){if(known[i,j]||!inRegion[i,j])continue;N[i,j]=(H[i-1,j]+H[i+1,j]+H[i,j-1]+H[i,j+1])/4;}H=N;}
  // lower the shell where it stands above the relaxed ground: never below 3.4 m over the tunnel floor (the route stays drivable)
  { var nv=(Vector3[])sv.Clone();int lowered=0;float maxDrop=0;
    for(int k=0;k<sv.Length;k++){var w=sv[k];int ix=Mathf.FloorToInt((w.x-x0)/Cell),iz=Mathf.FloorToInt((w.z-z0)/Cell);if(ix<0||iz<0||ix>=nx||iz>=nz||!inRegion[ix,iz])continue;
     float floorY=float.NaN;foreach(var h in Physics.RaycastAll(new Vector3(w.x,w.y-.25f,w.z),Vector3.down,80,~0,QueryTriggerInteraction.Ignore)){if(!h.collider.name.StartsWith("Ground_Culvert"))continue;if(float.IsNaN(floorY)||h.point.y>floorY)floorY=h.point.y;}
     if(float.IsNaN(floorY))continue;float cap=Mathf.Max(H[ix,iz]+.15f,floorY+3.4f);if(w.y>cap){nv[k].y=cap;lowered++;maxDrop=Mathf.Max(maxDrop,w.y-cap);}}
    log.Add($"shell lowered at {lowered} of {sv.Length} vertices (the most {maxDrop:F1} m), never under 3.4 m of clearance");
    if(!dry&&lowered>0){var inv=shell.transform.worldToLocalMatrix;var copy=UnityEngine.Object.Instantiate(shell.sharedMesh);copy.name="FreeRoamWorld-Report096-culvert-shell";copy.SetVertices(nv.Select(v=>inv.MultiplyPoint3x4(v)).ToList());copy.RecalculateBounds();copy.RecalculateNormals();
     Directory.CreateDirectory("Assets/Scenery/Report096");AssetDatabase.CreateAsset(copy,"Assets/Scenery/Report096/"+copy.name+".asset");var mcol=shell.GetComponent<MeshCollider>();if(mcol&&mcol.sharedMesh==shell.sharedMesh)mcol.sharedMesh=copy;shell.sharedMesh=copy;}
    sv=nv;top=ComputeTop(sv);}
  // the lid: the covered cells at their height, and a berm round them: 8 m of smooth slope down to the ground (a buried culvert, not a slab)
  const int M=16;var dist=new int[nx,nz];var liftNear=new float[nx,nz];for(int i=0;i<nx;i++)for(int j=0;j<nz;j++)dist[i,j]=int.MaxValue;
  var queue=new Queue<(int,int)>();float maxLift=0;
  for(int i=0;i<nx;i++)for(int j=0;j<nz;j++)if(cut[i,j]){dist[i,j]=0;float ceil=float.IsNaN(top[i,j])?H[i,j]:top[i,j]+.06f;liftNear[i,j]=Mathf.Max(0,ceil-H[i,j]);maxLift=Mathf.Max(maxLift,liftNear[i,j]);queue.Enqueue((i,j));}
  while(queue.Count>0){var (qi,qj)=queue.Dequeue();if(dist[qi,qj]>=M)continue;foreach(var (di,dj) in new[]{(1,0),(-1,0),(0,1),(0,-1)}){int a=qi+di,c=qj+dj;if(a<0||c<0||a>=nx||c>=nz)continue;if(dist[a,c]<=dist[qi,qj]+1)continue;dist[a,c]=dist[qi,qj]+1;liftNear[a,c]=liftNear[qi,qj];queue.Enqueue((a,c));}}
  var verts=new List<Vector3>();var index=new int[nx,nz];
  for(int i=0;i<nx;i++)for(int j=0;j<nz;j++){index[i,j]=-1;if(dist[i,j]>M)continue;float px=x0+(i+.5f)*Cell,pz=z0+(j+.5f)*Cell;float y;
   if(cut[i,j])y=Mathf.Max(H[i,j],float.IsNaN(top[i,j])?-1e9f:top[i,j]+.06f);
   else if(known[i,j]){float sft=1-(float)dist[i,j]/(M+1);float w=sft*sft*(3-2*sft);y=T[i,j]+.02f+liftNear[i,j]*w;}
   else y=H[i,j];
   if(!cut[i,j]&&!float.IsNaN(top[i,j]))y=Mathf.Max(y,top[i,j]+.06f);
   index[i,j]=verts.Count;verts.Add(new Vector3(px,y,pz));}
  var tris=new List<int>();for(int i=0;i+1<nx;i++)for(int j=0;j+1<nz;j++){int a=index[i,j],c=index[i+1,j],d=index[i,j+1],e=index[i+1,j+1];if(a<0||c<0||d<0||e<0)continue;tris.AddRange(new[]{a,d,c,c,d,e});}
  var material=mats.OrderByDescending(m=>m.Value).Select(m=>m.Key).FirstOrDefault();
  log.Add($"lid: {verts.Count} vertices, {tris.Count/3} triangles, highest above the relaxed ground {maxLift:F2} m, material {(material?material.name:"none")}");
  if(!dry&&tris.Count>0){var mesh=new Mesh{name="FreeRoamWorld-Report096-culvert-lid",indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};mesh.SetVertices(verts);mesh.SetTriangles(tris,0);mesh.RecalculateNormals();mesh.RecalculateBounds();mesh.SetColors(verts.Select(_=>new Color32(89,102,77,255)).ToList());mesh.SetColors(verts.Select(_=>new Color32(89,102,77,255)).ToList());
   Directory.CreateDirectory("Assets/Scenery/Report096");AssetDatabase.CreateAsset(mesh,"Assets/Scenery/Report096/"+mesh.name+".asset");
   var go=new GameObject("Ground_Report096 culvert lid");go.AddComponent<MeshFilter>().sharedMesh=mesh;var mr=go.AddComponent<MeshRenderer>();if(material)mr.sharedMaterial=material;go.AddComponent<MeshCollider>().sharedMesh=mesh;UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go,sc);
   EditorSceneManager.MarkSceneDirty(sc);EditorSceneManager.SaveScene(sc);AssetDatabase.SaveAssets();}
  Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/lid.txt",log);}
}
public static class Report096Mat {
 public static void Run(){
  EditorSceneManager.OpenScene("Assets/Scenes/FreeRoamWorld.unity");var log=new List<string>();
  foreach(var mf in UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Exclude,FindObjectsSortMode.None)){if(!mf.sharedMesh||!(mf.name=="Ground_400_400"||mf.name.StartsWith("Ground_Report096")))continue;var mr=mf.GetComponent<MeshRenderer>();var m=mr.sharedMaterial;var mesh=mf.sharedMesh;
   log.Add($"{mf.name} [{mesh.name}] verts {mesh.vertexCount} colors {mesh.colors32.Length} uv {mesh.uv.Length} uv2 {mesh.uv2.Length} normals {mesh.normals.Length} tangents {mesh.tangents.Length}; material {m.name} shader {m.shader.name}; keywords {string.Join(",",m.shaderKeywords)}");
   if(mesh.colors32.Length>0){var c=mesh.colors32;log.Add($"   colour samples: {c[0]} {c[c.Length/3]} {c[c.Length/2]} {c[c.Length-1]}");}
   if(mesh.uv.Length>0){var u=mesh.uv;log.Add($"   uv samples: {u[0]} {u[u.Length/3]} {u[u.Length/2]}");}
   for(int i=0;i<m.shader.GetPropertyCount();i++){var n=m.shader.GetPropertyName(i);var t=m.shader.GetPropertyType(i);log.Add($"   prop {n} {t} {(t==UnityEngine.Rendering.ShaderPropertyType.Color?m.GetColor(n).ToString():t==UnityEngine.Rendering.ShaderPropertyType.Float||t==UnityEngine.Rendering.ShaderPropertyType.Range?m.GetFloat(n).ToString():t==UnityEngine.Rendering.ShaderPropertyType.Texture?(m.GetTexture(n)?m.GetTexture(n).name:"none"):"")}");}}
  Directory.CreateDirectory(Environment.GetEnvironmentVariable("PROBE_OUT"));File.WriteAllLines(Environment.GetEnvironmentVariable("PROBE_OUT")+"/mat.txt",log);}
 public static void Go(){Run();EditorApplication.Exit(0);}
}
