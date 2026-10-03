using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Object=UnityEngine.Object;
// 0.72 local changes (PROJECT_TODO "CURRENT" Part A). Every change is logged to Docs/Report072/author-notes.txt.
// AUTHOR_PARTS selects parts, AUTHOR_SCENES the scenes, AUTHOR_DRY=1 reports without saving.
public static class Report072Author {
 const string Folder="Assets/Track/Report072";
 static readonly List<string> notes=new();
 static void Note(string s){notes.Add(s);Debug.Log("REPORT072 "+s);}
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
  if(Application.isPlaying)throw new Exception("Edit mode required");Directory.CreateDirectory("Docs/Report072");Physics.queriesHitBackfaces=false;
  var parts=(Environment.GetEnvironmentVariable("AUTHOR_PARTS")??"all").Split(',');bool Do(string p)=>parts.Contains("all")||parts.Contains(p);
  var scenes=(Environment.GetEnvironmentVariable("AUTHOR_SCENES")??"StreetLoopGreybox").Split(',');bool dry=Environment.GetEnvironmentVariable("AUTHOR_DRY")=="1";
  try{
   if(Do("cmats"))WeatherMaterials(dry);
   if(Environment.GetEnvironmentVariable("AUTHOR_NOSCENES")=="1")scenes=new string[0];
   foreach(var scene in scenes){
    EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();deps=null;Note("SCENE "+scene+(dry?" (dry run, not saved)":""));
    if(Do("a"))PartA(dry);
    if(Do("atrees"))TreePieces(dry);
    if(Do("aspike"))Spike(dry);
    if(Do("bflights"))Flights(dry);
    if(!dry)Save();}
  }finally{File.AppendAllLines("Docs/Report072/author-notes.txt",notes);}
  EditorApplication.Exit(0);}

 // ---------- Part A: the Summit Homeward supported-return ribbon becomes one clean surface ----------
 // The ribbon (CR-103, re-seated by 0.71) is a 14 m strip with vertices only at its two edges. At its hairpin (around
 // (692, 80, 100)) the inner edge doubles back, so quads fold into bow-ties, and the two legs overlap each other for ~25 m
 // at different heights: the torn patch of 0.71 BUG-001. It is rebuilt as the union of the same strip around its own centre
 // line (pair midpoints, pair half-widths, flat ends on the original end lines), sampled on a 0.5 m grid with the outline
 // interpolated (marching squares, no stair-steps). Height = the ground under it (smoothed over 1 m, never below the ground)
 // + 4 cm; a 35 cm skirt tucks the outline under the ground. Terrain-tile vertices inside the outline that would come within
 // 8 cm of the new surface go to 12 cm under it. Same material, same object, same collider.
 static bool Veg(Collider c)=>c.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0||c.name.IndexOf("tree",StringComparison.OrdinalIgnoreCase)>=0;
 static void PartA(bool dry){
  var launch=Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).FirstOrDefault(t=>t.name=="CR094 summit launch");
  var ret=launch?launch.Find("Ground_CR103 supported return"):null;
  if(!ret||!ret.gameObject.activeInHierarchy){Note($"Part A {Scene}: no active supported-return ribbon; not changed");return;}
  var mf=ret.GetComponent<MeshFilter>();var rc=ret.GetComponent<MeshCollider>();var src=mf.sharedMesh;
  // Second pass (0.72 follow-up): the surface built by the first pass is kept; the outline source is the 0.71 strip again.
  bool built=src.name.Contains("cr103-return-clean");bool second=built&&Environment.GetEnvironmentVariable("AUTHOR_FIRST")!="1";bool third=second&&Environment.GetEnvironmentVariable("AUTHOR_THIRD")=="1";
  if(built)src=AssetDatabase.LoadAssetAtPath<Mesh>($"Assets/Track/Report071/{Scene}-own-cr103-return.asset");
  float Built(float x,float z)=>second&&rc.Raycast(new Ray(new Vector3(x,600,z),Vector3.down),out var bh,1200)?bh.point.y:float.NaN;
  var wv=src.vertices.Select(v=>ret.TransformPoint(v)).ToArray();int n=wv.Length/2;
  var c=new Vector2[n];var hw=new float[n];for(int k=0;k<n;k++){var a=wv[2*k];var b=wv[2*k+1];c[k]=new Vector2(a.x+b.x,a.z+b.z)/2;hw[k]=new Vector2(a.x-b.x,a.z-b.z).magnitude/2;}
  Note($"Part A {Scene}: ribbon {AssetDatabase.GetAssetPath(src)}: {wv.Length} vertices, {src.triangles.Length/3} triangles, {n} edge pairs, half-width {hw.Min():F2}..{hw.Max():F2} m, centre line {c[0]} .. {c[n-1]}");
  float Sdf(Vector2 p){float best=1e9f;for(int k=0;k<n-1;k++){var a=c[k];var b=c[k+1];var ab=b-a;float L2=ab.sqrMagnitude;if(L2<1e-6f)continue;float t=Vector2.Dot(p-a,ab)/L2;float L=Mathf.Sqrt(L2);float v;
    if(k==0&&t<0){float e=Mathf.Abs((p.x-a.x)*ab.y-(p.y-a.y)*ab.x)/L;v=Mathf.Max(e-hw[0],-t*L);}
    else if(k==n-2&&t>1){float e=Mathf.Abs((p.x-a.x)*ab.y-(p.y-a.y)*ab.x)/L;v=Mathf.Max(e-hw[n-1],(t-1)*L);}
    else{float tc=Mathf.Clamp01(t);v=(p-(a+ab*tc)).magnitude-Mathf.Lerp(hw[k],hw[k+1],tc);}
    if(v<best)best=v;}
   // third pass: the ends follow the original end lines (pair 0 and pair n-1), not the perpendicular of the centre line
   if(third){foreach(var (e0,e1,inner) in new[]{(wv[0],wv[1],c[Mathf.Min(2,n-1)]),(wv[2*n-2],wv[2*n-1],c[Mathf.Max(0,n-3)])}){var A=new Vector2(e0.x,e0.z);var B=new Vector2(e1.x,e1.z);var mid=(A+B)/2;if((p-mid).magnitude>12)continue;
     var nrm=new Vector2(-(B-A).y,(B-A).x).normalized;if(Vector2.Dot(inner-mid,nrm)>0)nrm=-nrm;best=Mathf.Max(best,Vector2.Dot(p-mid,nrm));}}
   return best;}
  // Ground under the ribbon: the top surface of any solid ground collider except the ribbon itself (trees ignored).
  float Ground(float x,float z){foreach(var h in Physics.RaycastAll(new Vector3(x,600,z),Vector3.down,1200,~0,QueryTriggerInteraction.Ignore).OrderBy(h=>h.distance)){var col=h.collider;
    if(h.normal.y>0&&col!=rc&&!col.attachedRigidbody&&!Veg(col))return h.point.y;}return float.NaN;}
  // Old ribbon height above the ground at its own vertices (is it a terrain-hugging trail everywhere?).
  var gaps=wv.Select(v=>v.y-Ground(v.x,v.z)).Where(g=>!float.IsNaN(g)).OrderBy(g=>g).ToArray();
  Note($"Part A {Scene}: old ribbon vertex height above the ground: min {gaps.First():F2} median {gaps[gaps.Length/2]:F2} max {gaps.Last():F2} m (over 0.5 m: {gaps.Count(g=>g>.5f)})");
  const float G=.5f;float x0=wv.Min(v=>v.x)-8,z0=wv.Min(v=>v.z)-8;int nx=(int)((wv.Max(v=>v.x)+8-x0)/G)+1,nz=(int)((wv.Max(v=>v.z)+8-z0)/G)+1;
  var d=new float[nx,nz];var T=new float[nx,nz];var H=new float[nx,nz];
  for(int i=0;i<nx;i++)for(int j=0;j<nz;j++){d[i,j]=Sdf(new Vector2(x0+i*G,z0+j*G));T[i,j]=d[i,j]<2.6f?Ground(x0+i*G,z0+j*G):float.NaN;}
  int missing=0;for(int i=0;i<nx;i++)for(int j=0;j<nz;j++){if(d[i,j]>2f){H[i,j]=float.NaN;continue;}float s=0;int m=0;for(int a=-2;a<=2;a++)for(int b=-2;b<=2;b++){int p=i+a,q=j+b;if(p<0||q<0||p>=nx||q>=nz||float.IsNaN(T[p,q]))continue;s+=T[p,q];m++;}
   if(m==0||float.IsNaN(T[i,j])){H[i,j]=float.NaN;if(d[i,j]<=0)missing++;continue;}H[i,j]=Mathf.Max(s/m,T[i,j])+.04f;
   if(second){float b=Built(x0+i*G,z0+j*G);if(!float.IsNaN(b))H[i,j]=b;}
   // the outline comes down to 1.5 cm above the ground over its last metre (no lip to ride onto)
   if(second&&!third&&d[i,j]>-1f&&d[i,j]<=0)H[i,j]-=.025f*(1+d[i,j]);}
  // Third pass: only where 0.71 was broken (two or more layers of the 0.71 strip, or a back face on top, dilated 1.5 m) does the
  // rebuilt surface stay; elsewhere the surface is the 0.71 strip's own surface (or the ground + 1.5 cm where the strip never
  // covered), blended over 3 m. The 0.71 driving surface on the Summit Homeward landing and run-out is kept exactly.
  // Third pass (final rule): inside the protected Summit Homeward landing / run-out corridor (launch frame |x| < 12,
  // s 432-570) the surface is exactly the 0.71 strip's top layer (smoothed over 2.5 m only where 0.71 had two layers), with
  // sheet-like edges, as 0.71 drove; everywhere else (the hairpin and the overlap beyond the corridor) it is the clean
  // rebuilt surface: the 0.71 ground (git HEAD terrain, Assets/Report072Head) smoothed 1 m, never below it, + 4 cm.
  // 10 m blend beyond it (|x| 12-22). Deterministic: the same inputs every time it runs.
  if(third){var tmp=new GameObject("report072 old strip");tmp.transform.SetPositionAndRotation(ret.position,ret.rotation);tmp.transform.localScale=ret.lossyScale;var oc=tmp.AddComponent<MeshCollider>();oc.sharedMesh=src;
   var heads=new List<GameObject>();foreach(var tf in GameObject.Find("Memory loop - north is +Z").GetComponentsInChildren<MeshFilter>()){if(!tf.sharedMesh)continue;var hp="Assets/Report072Head/"+System.IO.Path.GetFileName(AssetDatabase.GetAssetPath(tf.sharedMesh));
    var hm=File.Exists(hp)?AssetDatabase.LoadAssetAtPath<Mesh>(hp):tf.GetComponent<MeshCollider>()?tf.sharedMesh:null;if(!hm)continue;var g=new GameObject("report072 head "+tf.name);g.layer=30;g.transform.SetPositionAndRotation(tf.transform.position,tf.transform.rotation);g.transform.localScale=tf.transform.lossyScale;g.AddComponent<MeshCollider>().sharedMesh=hm;heads.Add(g);}
   Physics.SyncTransforms();Physics.queriesHitBackfaces=true;
   float GroundHead(float x,float z)=>Physics.Raycast(new Vector3(x,600,z),Vector3.down,out var hh,1200,1<<30,QueryTriggerInteraction.Ignore)?hh.point.y:float.NaN;
   var broken=new bool[nx,nz];var oldTop=new float[nx,nz];var Th=new float[nx,nz];
   for(int i=0;i<nx;i++)for(int j=0;j<nz;j++){oldTop[i,j]=Th[i,j]=float.NaN;if(d[i,j]>2.6f)continue;Th[i,j]=GroundHead(x0+i*G,z0+j*G);if(d[i,j]>1f)continue;var o=new Vector3(x0+i*G,600,z0+j*G);
    var hs=Physics.RaycastAll(o,Vector3.down,1200,~0,QueryTriggerInteraction.Collide).Where(h=>h.collider==oc).ToArray();
    if(hs.Length>0){oldTop[i,j]=hs.Max(h=>h.point.y);broken[i,j]=hs.Length>1||hs.Any(h=>h.normal.y<0);}}
   Physics.queriesHitBackfaces=false;Object.DestroyImmediate(tmp);foreach(var g in heads)Object.DestroyImmediate(g);Physics.SyncTransforms();
   float Wc(int i,int j){var q=launch.InverseTransformPoint(new Vector3(x0+i*G,0,z0+j*G));float w=Mathf.Clamp01((22-Mathf.Abs(q.x))/10)*Mathf.Clamp01((q.z-424)/8)*Mathf.Clamp01((578-q.z)/8);return w*w*(3-2*w);}
   // Beside the hairpin (the spike patch) the ground near the outline is now lower than 0.71's: there the edge follows it.
   for(int i=0;i<nx;i++)for(int j=0;j<nz;j++){float wx=x0+i*G,wz=z0+j*G;if(wx>=675&&wx<=696&&wz>=103&&wz<=122&&d[i,j]>-1.5f&&!float.IsNaN(T[i,j])&&!float.IsNaN(Th[i,j]))Th[i,j]=Mathf.Min(Th[i,j],T[i,j]);}
   int nk=0,nr=0,nbk=0;var Hn=new float[nx,nz];
   for(int i=0;i<nx;i++)for(int j=0;j<nz;j++){Hn[i,j]=float.NaN;if(d[i,j]>2f||float.IsNaN(Th[i,j]))continue;
    float s0=0;int m0=0;for(int a=-2;a<=2;a++)for(int b=-2;b<=2;b++){int p=i+a,q=j+b;if(p<0||q<0||p>=nx||q>=nz||float.IsNaN(Th[p,q]))continue;s0+=Th[p,q];m0++;}
    float rebuilt=Mathf.Max(s0/m0,Th[i,j])+.04f;float w=Wc(i,j);
    float keep=oldTop[i,j];if(!float.IsNaN(keep)&&broken[i,j]){float s1=0;int m1=0;for(int a=-2;a<=2;a++)for(int b=-2;b<=2;b++){int p=i+a,q=j+b;if(p<0||q<0||p>=nx||q>=nz||float.IsNaN(oldTop[p,q]))continue;s1+=oldTop[p,q];m1++;}keep=s1/m1;if(w>0)nbk++;}
    if(float.IsNaN(keep))keep=float.IsNaN(T[i,j])?rebuilt:T[i,j]+.015f;
    if(d[i,j]>0&&w>.5f)continue;// outside the outline in the kept corridor: the edge takes the inside height (a sheet edge, no ramp)
    Hn[i,j]=Mathf.Lerp(rebuilt,keep,w);if(w>=1)nk++;else if(w<=0)nr++;}
   H=Hn;
   Note($"Part A {Scene}: third pass (final rule): {nk} grid points keep the 0.71 surface (landing / run-out corridor; {nbk} of them where 0.71 had two layers, smoothed), {nr} take the rebuilt surface, the rest blend; ends on the original end lines");}
  if(missing>0){var miss=new List<Vector2>();for(int i=0;i<nx;i++)for(int j=0;j<nz;j++)if(d[i,j]<=0&&float.IsNaN(H[i,j]))miss.Add(new Vector2(x0+i*G,z0+j*G));Note($"Part A {Scene}: no ground at {string.Join(" ",miss.Take(60).Select(m=>$"({m.x:F1},{m.y:F1})"))}");}
  foreach(var v in wv){float g=v.y-Ground(v.x,v.z);if(g>.4f||float.IsNaN(g))Note($"Part A {Scene}: old ribbon vertex {v:F2} is {g:F2} m above the ground");}
  if(missing>0){Note($"Part A {Scene}: {missing} grid points inside the outline have no ground under them; not changed");return;}
  // Marching squares: each cell's square clipped to d <= 0, vertices shared through corner / edge keys.
  var verts=new List<Vector3>();var tris=new List<int>();var key=new Dictionary<(int,int,int),int>();
  int Corner(int i,int j){var k=(i,j,0);if(!key.TryGetValue(k,out var id)){id=verts.Count;verts.Add(new Vector3(x0+i*G,H[i,j],z0+j*G));key[k]=id;}return id;}
  int Edge(int i,int j,int i2,int j2){var k=i2==i?(i,Mathf.Min(j,j2),2):(Mathf.Min(i,i2),j,1);
   if(!key.TryGetValue(k,out var id)){float da=d[i,j],db=d[i2,j2];float t=da/(da-db);float ex=x0+Mathf.Lerp(i,i2,t)*G,ez=z0+Mathf.Lerp(j,j2,t)*G;float g=second&&!third?Ground(ex,ez):float.NaN;
    float ha=H[i,j],hb=H[i2,j2];if(float.IsNaN(ha))ha=hb;if(float.IsNaN(hb))hb=ha;
    id=verts.Count;verts.Add(new Vector3(ex,float.IsNaN(g)?Mathf.Lerp(ha,hb,t):g+.015f,ez));key[k]=id;}return id;}
  for(int i=0;i<nx-1;i++)for(int j=0;j<nz-1;j++){var cs=new[]{(i,j),(i,j+1),(i+1,j+1),(i+1,j)};if(cs.All(q=>d[q.Item1,q.Item2]>0))continue;
   var poly=new List<int>();for(int e=0;e<4;e++){var A=cs[e];var B=cs[(e+1)%4];bool ia=d[A.Item1,A.Item2]<=0,ib=d[B.Item1,B.Item2]<=0;if(ia)poly.Add(Corner(A.Item1,A.Item2));if(ia!=ib)poly.Add(Edge(A.Item1,A.Item2,B.Item1,B.Item2));}
   for(int e=1;e+1<poly.Count;e++){int a=poly[0],b=poly[e],cc=poly[e+1];var nrm=Vector3.Cross(verts[b]-verts[a],verts[cc]-verts[a]);if(nrm.sqrMagnitude<1e-10f)continue;if(nrm.y<0){var tmp=b;b=cc;cc=tmp;}tris.Add(a);tris.Add(b);tris.Add(cc);}}
  int topTris=tris.Count/3;
  {int nanV=verts.Count(v=>float.IsNaN(v.y));if(nanV>0){Note($"Part A {Scene}: WARNING {nanV} vertices without a height, e.g. {string.Join(" ",verts.Where(v=>float.IsNaN(v.y)).Take(5).Select(v=>$"({v.x:F1},{v.z:F1})"))}");
    var v0=verts.First(v=>float.IsNaN(v.y));int ci=Mathf.RoundToInt((v0.x-x0)/G),cj=Mathf.RoundToInt((v0.z-z0)/G);Note($"   corner {ci},{cj}: d {d[ci,cj]:F2} T {T[ci,cj]:F2} H {H[ci,cj]:F2} built {Built(x0+ci*G,z0+cj*G):F2}");}}
  // Skirt: every outline edge gets a strip 35 cm out (down the outline normal) and 30 cm down, under the ground.
  var edgeUse=new Dictionary<(int,int),int>();for(int t=0;t<tris.Count;t+=3)for(int e=0;e<3;e++){int a=tris[t+e],b=tris[t+(e+1)%3];var k=a<b?(a,b):(b,a);edgeUse[k]=edgeUse.TryGetValue(k,out var u)?u+1:1;}
  var skirt=new Dictionary<int,int>();int Skirt(int v){if(skirt.TryGetValue(v,out var s))return s;var p=verts[v];float e=.05f;var gx=Sdf(new Vector2(p.x+e,p.z))-Sdf(new Vector2(p.x-e,p.z));var gz=Sdf(new Vector2(p.x,p.z+e))-Sdf(new Vector2(p.x,p.z-e));var g=new Vector2(gx,gz).normalized;
   s=verts.Count;verts.Add(new Vector3(p.x+g.x*.35f,p.y-.3f,p.z+g.y*.35f));skirt[v]=s;return s;}
  int outline=0;foreach(var kv in edgeUse.Where(kv=>kv.Value==1).ToList()){outline++;int a=kv.Key.Item1,b=kv.Key.Item2,sa=Skirt(a),sb=Skirt(b);var mid=(verts[a]+verts[b])/2;var outDir=(verts[sa]+verts[sb])/2-mid;outDir.y=0;
   var nrm=Vector3.Cross(verts[b]-verts[a],verts[sa]-verts[a]);if(Vector3.Dot(nrm,outDir)+nrm.y<0){var tmp=a;a=b;b=tmp;var t2=sa;sa=sb;sb=t2;}tris.AddRange(new[]{a,b,sa,b,sb,sa});}
  // Bilinear new-surface height (for the terrain pass).
  float NewH(float x,float z){float fi=(x-x0)/G,fj=(z-z0)/G;int i=Mathf.Clamp((int)fi,0,nx-2),j=Mathf.Clamp((int)fj,0,nz-2);float u=fi-i,w=fj-j;float h00=H[i,j],h10=H[i+1,j],h01=H[i,j+1],h11=H[i+1,j+1];
   var hs=new[]{h00,h10,h01,h11}.Where(h=>!float.IsNaN(h)).ToArray();if(hs.Length<4)return hs.Length>0?hs.Max():float.NaN;return Mathf.Lerp(Mathf.Lerp(h00,h10,u),Mathf.Lerp(h01,h11,u),w);}
  float oldArea=0,newArea=0;var st=src.triangles;for(int t=0;t<st.Length;t+=3)oldArea+=Vector3.Cross(wv[st[t+1]]-wv[st[t]],wv[st[t+2]]-wv[st[t]]).magnitude/2;
  for(int t=0;t<topTris*3;t+=3){var A=verts[tris[t]];var B=verts[tris[t+1]];var C=verts[tris[t+2]];newArea+=Vector3.Cross(B-A,C-A).magnitude/2;}
  Note($"Part A {Scene}: new ribbon: {verts.Count} vertices, {topTris} top triangles + skirt on {outline} outline edges; top area {newArea:F0} m2 (old strip triangles {oldArea:F0} m2 incl. overlaps); height = ground +4 cm (1 m smoothing, never below the ground)");
  // Objects standing inside the outline (for the dependency pass).
  foreach(var col in Object.FindObjectsByType<Collider>(FindObjectsSortMode.None).Where(q=>!q.isTrigger&&q!=rc&&q.bounds.size.x<12&&q.bounds.size.z<12&&Sdf(new Vector2(q.bounds.center.x,q.bounds.center.z))<.5f)){
   var b=col.bounds;float g=NewH(b.center.x,b.center.z);Note($"Part A {Scene}: object on the outline: {Path(col.transform)} base {b.min.y:F2} new surface {g:F2} (size {b.size:F1})");}
  // Terrain-tile vertices inside the outline that would come within 8 cm of the new surface go to 12 cm under it.
  var tilesRoot=GameObject.Find("Memory loop - north is +Z");var area=new Bounds(new Vector3(x0+nx*G/2,0,z0+nz*G/2),new Vector3(nx*G,4000,nz*G));
  int moved=0;float maxMove=0;var touched=new List<string>();
  if(!second)foreach(var tf in tilesRoot.GetComponentsInChildren<MeshFilter>().Where(m=>m.sharedMesh&&m.GetComponent<MeshCollider>()&&m.GetComponent<Renderer>()&&m.GetComponent<Renderer>().bounds.Intersects(area))){
   var lv=tf.sharedMesh.vertices;bool any=false;
   for(int i=0;i<lv.Length;i++){var w=tf.transform.TransformPoint(lv[i]);if(Sdf(new Vector2(w.x,w.z))>-.15f)continue;float h=NewH(w.x,w.z);if(float.IsNaN(h)||w.y<h-.08f||w.y>h+2)continue;
    maxMove=Mathf.Max(maxMove,w.y-(h-.12f));w.y=h-.12f;lv[i]=tf.transform.InverseTransformPoint(w);moved++;any=true;}
   if(any){touched.Add(tf.name);if(!dry){var m=Own(tf);m.vertices=lv;m.RecalculateNormals();m.RecalculateBounds();Refresh(tf);}}}
  Note($"Part A {Scene}: terrain-tile vertices under the new ribbon lowered to 12 cm under it: {moved} (largest move {maxMove:F2} m) in {string.Join(", ",touched)}");
  if(third){int restored=0;foreach(var tf in tilesRoot.GetComponentsInChildren<MeshFilter>().Where(m=>m.sharedMesh&&m.GetComponent<MeshCollider>()&&m.GetComponent<Renderer>()&&m.GetComponent<Renderer>().bounds.Intersects(area))){
    var hp="Assets/Report072Head/"+System.IO.Path.GetFileName(AssetDatabase.GetAssetPath(tf.sharedMesh));var hm=File.Exists(hp)?AssetDatabase.LoadAssetAtPath<Mesh>(hp):null;if(!hm||hm.vertexCount!=tf.sharedMesh.vertexCount)continue;
    var lv=tf.sharedMesh.vertices;var hv=hm.vertices;bool any=false;
    for(int i=0;i<lv.Length;i++){if(lv[i].y>=hv[i].y-.005f)continue;var w=tf.transform.TransformPoint(lv[i]);if(w.x>=676&&w.x<=695&&w.z>=108.5f&&w.z<=120.5f)continue;// the spike patch
     if(Sdf(new Vector2(w.x,w.z))<-.05f)continue;lv[i]=hv[i];restored++;any=true;}
    if(any&&!dry){var m=Own(tf);m.vertices=lv;m.RecalculateNormals();m.RecalculateBounds();Refresh(tf);}}
   Note($"Part A {Scene}: third pass: terrain-tile vertices outside the final outline that the first pass had lowered, put back to their 0.71 height: {restored}");
   // Spike patch beside the hairpin: ground just outside the outline that the spike smoothing took below the ribbon edge
   // comes back up to a shoulder of at most 1:1.8 from the edge (4 cm under it), so the edge never overhangs.
   int raised=0;float maxRaise=0;foreach(var tf in tilesRoot.GetComponentsInChildren<MeshFilter>().Where(m=>m.sharedMesh&&m.GetComponent<MeshCollider>()&&m.GetComponent<Renderer>()&&m.GetComponent<Renderer>().bounds.Intersects(area))){
    var lv=tf.sharedMesh.vertices;bool any=false;
    for(int i=0;i<lv.Length;i++){var w=tf.transform.TransformPoint(lv[i]);if(!(w.x>=676&&w.x<=695&&w.z>=104f&&w.z<=121f))continue;var p2=new Vector2(w.x,w.z);float dd=Sdf(p2);if(dd<=0||dd>5)continue;
     float e=.05f;var gr=new Vector2(Sdf(new Vector2(w.x+e,w.z))-Sdf(new Vector2(w.x-e,w.z)),Sdf(new Vector2(w.x,w.z+e))-Sdf(new Vector2(w.x,w.z-e))).normalized;var edge=p2-gr*(dd+.3f);
     float he=NewH(edge.x,edge.y);if(float.IsNaN(he))continue;float target=he-.04f-.55f*dd;if(w.y>=target)continue;maxRaise=Mathf.Max(maxRaise,target-w.y);w.y=target;lv[i]=tf.transform.InverseTransformPoint(w);raised++;any=true;}
    if(any&&!dry){var m=Own(tf);m.vertices=lv;m.RecalculateNormals();m.RecalculateBounds();Refresh(tf);}}
   Note($"Part A {Scene}: third pass: ground beside the hairpin's outer edge raised to meet the ribbon edge: {raised} vertices (largest {maxRaise:F2} m)");}
  if(dry){Note($"Part A {Scene}: dry run, nothing written");return;}
  var lvs=verts.Select(v=>ret.InverseTransformPoint(v)).ToArray();
  var mesh=new Mesh{name="cr103-return-clean",indexFormat=lvs.Length>65000?UnityEngine.Rendering.IndexFormat.UInt32:UnityEngine.Rendering.IndexFormat.UInt16};mesh.vertices=lvs;mesh.triangles=tris.ToArray();mesh.RecalculateNormals();mesh.RecalculateBounds();
  var format=mesh.indexFormat;
  if(!second){var st1=Store("cr103-return-clean",mesh);mf.sharedMesh=st1;rc.sharedMesh=null;rc.sharedMesh=st1;EditorUtility.SetDirty(mf);EditorUtility.SetDirty(rc);Physics.SyncTransforms();Note($"Part A {Scene}: ribbon replaced by {AssetDatabase.GetAssetPath(st1)} (first pass: renderer and collider)");return;}
  var stored=Store("cr103-return-clean",mesh);
  // The skirt is visual only: the collider is the top surface alone (a skirt face is a ramp to a vehicle crossing the edge).
  var cm=new Mesh{name="cr103-return-clean-collider",indexFormat=format};var used=tris.Take(topTris*3).ToArray();cm.vertices=lvs;cm.triangles=used;cm.RecalculateNormals();cm.RecalculateBounds();
  var storedC=Store("cr103-return-clean-collider",cm);
  mf.sharedMesh=stored;rc.sharedMesh=null;rc.sharedMesh=storedC;EditorUtility.SetDirty(mf);EditorUtility.SetDirty(rc);Physics.SyncTransforms();
  Note($"Part A {Scene}: ribbon replaced by {AssetDatabase.GetAssetPath(stored)} (renderer, with the skirt) and {AssetDatabase.GetAssetPath(storedC)} (collider, top surface only){(third?"; third pass":second?"; second pass: surface kept, outline tapered to 1.5 cm above the ground, collider without the skirt":"")}");}

 // ---------- Part A follow-up: tree pieces 0.71 left behind ----------
 // 0.71 removed or re-grounded trees in and beside the box by moving "that tree's pieces" (connected parts near the trunk) in
 // the batched tree meshes. Crown lobes further from the trunk were missed, so pieces of crowns were left at the old height or
 // without their trunk (the pale faceted shards beside the 0.72 BUG-001 view). Against the 0.70 mesh (git a5b2ebaa copies in
 // Assets/Report072Old, or the original of an "-own-" copy): every 0.70 connected piece is matched to its tree (nearest trunk
 // piece within 4.5 m, below it). A piece whose trunk was removed is removed; a piece that did not move with its trunk (or was
 // split) is moved by its trunk's displacement. Nothing else changes.
 static Mesh OldMesh(Mesh m){var ap=AssetDatabase.GetAssetPath(m);var on=System.IO.Path.GetFileNameWithoutExtension(ap);int ix=on.IndexOf("-own-");if(ix>=0)on=on.Substring(ix+5);
  var op="Assets/Report072Old/"+on+".asset";if(File.Exists(op))return AssetDatabase.LoadAssetAtPath<Mesh>(op);if(ix<0)return null;
  var alt=AssetDatabase.FindAssets(on+" t:Mesh").Select(AssetDatabase.GUIDToAssetPath).FirstOrDefault(x=>System.IO.Path.GetFileNameWithoutExtension(x)==on&&!x.Contains("/Report07"));return alt==null?null:AssetDatabase.LoadAssetAtPath<Mesh>(alt);}
 static void TreePieces(bool dry){
  var launch=Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).FirstOrDefault(t=>t.name=="CR094 summit launch");if(!launch||!launch.gameObject.activeInHierarchy){Note($"Part A trees {Scene}: no launch");return;}
  bool Area(Vector3 w){var q=launch.InverseTransformPoint(w);return q.z>=280&&q.z<=720&&Mathf.Abs(q.x)<=75;}
  int removedP=0,movedP=0;
  foreach(var mf in Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None)){var r=mf.GetComponent<Renderer>();if(!r||!r.enabled||!mf.sharedMesh)continue;var path=Path(mf.transform);
   if(!(path.Contains("tree")||path.Contains("Tree")||path.Contains("foliage")||path.Contains("canopy")||path.Contains("Woods")||path.Contains("woodland")||path.Contains("timber")))continue;
   var m=mf.sharedMesh;var om=OldMesh(m);if(!om)continue;if(om.vertexCount!=m.vertexCount){Note($"Part A trees {Scene}: {path}: vertex count changed ({om.vertexCount} -> {m.vertexCount}); skipped");continue;}
   var O=om.vertices;var N=m.vertices;var tr=mf.transform;
   // 0.70 pieces (topology + welded positions over all submeshes)
   var parent=Enumerable.Range(0,O.Length).ToArray();int Find(int a){while(parent[a]!=a){parent[a]=parent[parent[a]];a=parent[a];}return a;}void Join(int a,int b){a=Find(a);b=Find(b);if(a!=b)parent[b]=a;}
   var weld=new Dictionary<Vector3Int,int>();for(int i=0;i<O.Length;i++){var k=Vector3Int.RoundToInt(O[i]*200);if(weld.TryGetValue(k,out var j))Join(j,i);else weld[k]=i;}
   var oldTris=new List<(int a,int b,int c,int sub)>();for(int sm=0;sm<om.subMeshCount;sm++){var t=om.GetTriangles(sm);for(int i=0;i<t.Length;i+=3){oldTris.Add((t[i],t[i+1],t[i+2],sm));Join(t[i],t[i+1]);Join(t[i],t[i+2]);}}
   var newTris=new HashSet<(int,int,int)>();for(int sm=0;sm<m.subMeshCount;sm++){var t=m.GetTriangles(sm);for(int i=0;i<t.Length;i+=3)newTris.Add((t[i],t[i+1],t[i+2]));}
   var pieces=new Dictionary<int,List<int>>();for(int q=0;q<oldTris.Count;q++){int g=Find(oldTris[q].a);if(!pieces.TryGetValue(g,out var l))pieces[g]=l=new();l.Add(q);}
   var info=new List<(int g,List<int> tris,HashSet<int> verts,Bounds ob,bool trunk,int present)>();
   foreach(var kv in pieces){var vs=new HashSet<int>();foreach(var q in kv.Value){vs.Add(oldTris[q].a);vs.Add(oldTris[q].b);vs.Add(oldTris[q].c);}
    var w0=tr.TransformPoint(O[vs.First()]);var b=new Bounds(w0,Vector3.zero);foreach(var v in vs)b.Encapsulate(tr.TransformPoint(O[v]));if(!Area(b.center))continue;
    var mat=r.sharedMaterials.Length>oldTris[kv.Value[0]].sub?r.sharedMaterials[oldTris[kv.Value[0]].sub]:null;var mn=mat?mat.name.ToLowerInvariant():"";
    bool trunk=mn.Contains("bark")||mn.Contains("trunk")||(b.size.y>b.size.x*1.6f&&b.size.y>b.size.z*1.6f&&b.size.x<1.8f&&b.size.z<1.8f);
    int present=kv.Value.Count(q=>newTris.Contains((oldTris[q].a,oldTris[q].b,oldTris[q].c)));info.Add((kv.Key,kv.Value,vs,b,trunk,present));}
   if(info.Count==0)continue;
   Vector3 Disp(HashSet<int> vs){var low=vs.OrderBy(v=>O[v].y).First();return tr.TransformVector(N[low]-O[low]);}
   bool Uniform(HashSet<int> vs){var d0=N[vs.First()]-O[vs.First()];return vs.All(v=>((N[v]-O[v])-d0).sqrMagnitude<.0025f);}
   var trunks=info.Where(p=>p.trunk).ToList();var removeTris=new HashSet<int>();var newV=(Vector3[])N.Clone();bool changed=false;
   foreach(var p in info.Where(p=>!p.trunk&&p.present>0)){
    var t=trunks.Where(k=>new Vector2(k.ob.center.x-p.ob.center.x,k.ob.center.z-p.ob.center.z).magnitude<4.5f&&k.ob.max.y<p.ob.max.y+.5f).OrderBy(k=>new Vector2(k.ob.center.x-p.ob.center.x,k.ob.center.z-p.ob.center.z).magnitude).FirstOrDefault();
    if(p.ob.center.x>672&&p.ob.center.x<698&&p.ob.center.z>98&&p.ob.center.z<122)Note($"Part A trees {Scene}: (view) {path}: piece {p.ob.center:F1} size {p.ob.size:F1} tris {p.present}/{p.tris.Count} trunk {(t.tris==null?"none":t.ob.center.ToString("F1")+" present "+t.present)} moved {Disp(p.verts).y:F2} uniform {Uniform(p.verts)}");
    if(t.tris==null){// no trunk: a whole crown is left alone; a stray fragment (12 triangles or fewer, no trunk) is removed
     if(p.present==p.tris.Count&&p.tris.Count<=12&&trunks.Count>0){var lq0=launch.InverseTransformPoint(p.ob.center);Note($"Part A trees {Scene}: {path}: stray crown fragment at {p.ob.center:F1} (launch x {lq0.x:F0} s {lq0.z:F0}, {p.tris.Count} tris, {p.ob.size:F1}, no trunk); removed");foreach(var q in p.tris)removeTris.Add(q);removedP++;changed=true;}
     continue;}
    var lq=launch.InverseTransformPoint(p.ob.center);
    if(t.present==0){Note($"Part A trees {Scene}: {path}: piece at {p.ob.center:F1} (launch x {lq.x:F0} s {lq.z:F0}, {p.present}/{p.tris.Count} tris) lost its trunk in 0.71; removed");foreach(var q in p.tris)removeTris.Add(q);removedP++;changed=true;continue;}
    var dt=Disp(t.verts);bool split=!Uniform(p.verts);var dp=Disp(p.verts);
    if(split||(dp-dt).magnitude>.2f||p.present<p.tris.Count){
     if(p.present<p.tris.Count){Note($"Part A trees {Scene}: {path}: piece at {p.ob.center:F1} partly removed in 0.71 ({p.present}/{p.tris.Count} tris) though its trunk stayed; left as is");continue;}
     Note($"Part A trees {Scene}: {path}: piece at {p.ob.center:F1} (launch x {lq.x:F0} s {lq.z:F0}) {(split?"was split":"did not move with its trunk")} (piece moved {dp.y:F2} m, trunk {dt.y:F2} m); moved with the trunk");
     var dl=tr.InverseTransformVector(dt);foreach(var v in p.verts)newV[v]=O[v]+dl;movedP++;changed=true;}}
   if(!changed||dry)continue;
   var own=Own(mf);own.vertices=newV;var gone=new HashSet<(int,int,int)>(removeTris.Select(q=>(oldTris[q].a,oldTris[q].b,oldTris[q].c)));
   if(gone.Count>0)for(int sm=0;sm<own.subMeshCount;sm++){var t=own.GetTriangles(sm);var outT=new List<int>();for(int i=0;i<t.Length;i+=3)if(!gone.Contains((t[i],t[i+1],t[i+2])))outT.AddRange(new[]{t[i],t[i+1],t[i+2]});own.SetTriangles(outT,sm);}
   own.RecalculateBounds();Refresh(mf);}
  Note($"Part A trees {Scene}: pieces removed {removedP}, pieces moved with their trunk {movedP}{(dry?" (dry run)":"")}");}

 // ---------- Part A follow-up: the pale spiked bank beside the ribbon's hairpin ----------
 // Beside the hairpin (x 677-694, z 109.5-119.5) the terrain rises in spikes to ~84 m and then drops 4-5 m in a cliff to the
 // forest floor (~74.5 m): the "pale faceted terrain poking up" in the 0.72 BUG-001 view (hidden by the catch mound before
 // 0.71). Terrain-tile nodes in that patch (not under the ribbon) are replaced by the smooth (harmonic) surface spanning the
 // untouched ground around it. Trees standing in it move with the ground: trunk collider plus its pieces in the batched
 // tree meshes (pieces within 4.5 m whose nearest trunk it is).
 static void Spike(bool dry){
  var launch=Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).FirstOrDefault(t=>t.name=="CR094 summit launch");
  var ret=launch?launch.Find("Ground_CR103 supported return"):null;if(!ret||!ret.gameObject.activeInHierarchy){Note($"Part A spike {Scene}: no ribbon; not changed");return;}
  var rc=ret.GetComponent<MeshCollider>();const float X0=677,X1=694,Z0=109.5f,Z1=119.5f;
  bool UnderRibbon(Vector3 w)=>rc.Raycast(new Ray(new Vector3(w.x,w.y+30,w.z),Vector3.down),out _,60)||rc.Raycast(new Ray(new Vector3(w.x+.8f,w.y+30,w.z),Vector3.down),out _,60)||rc.Raycast(new Ray(new Vector3(w.x-.8f,w.y+30,w.z),Vector3.down),out _,60)||rc.Raycast(new Ray(new Vector3(w.x,w.y+30,w.z+.8f),Vector3.down),out _,60)||rc.Raycast(new Ray(new Vector3(w.x,w.y+30,w.z-.8f),Vector3.down),out _,60);
  bool InPatch(Vector3 w)=>w.x>=X0&&w.x<=X1&&w.z>=Z0&&w.z<=Z1&&!UnderRibbon(w);
  var tilesRoot=GameObject.Find("Memory loop - north is +Z");var area=new Bounds(new Vector3((X0+X1)/2,0,(Z0+Z1)/2),new Vector3(X1-X0+30,4000,Z1-Z0+30));
  var mfs=tilesRoot.GetComponentsInChildren<MeshFilter>().Where(m=>m.sharedMesh&&m.GetComponent<MeshCollider>()&&m.GetComponent<Renderer>().bounds.Intersects(area)).ToArray();var tileCols=mfs.Select(m=>m.GetComponent<MeshCollider>()).ToArray();
  float TileTop(float x,float z){foreach(var h in Physics.RaycastAll(new Vector3(x,600,z),Vector3.down,1200,~0,QueryTriggerInteraction.Ignore).OrderBy(h=>h.distance))if(h.normal.y>0&&tileCols.Contains(h.collider as MeshCollider))return h.point.y;return float.NaN;}
  (int,int) K(Vector3 p)=>(Mathf.RoundToInt(p.x*50),Mathf.RoundToInt(p.z*50));
  var y=new Dictionary<(int,int),float>();var nb=new Dictionary<(int,int),HashSet<(int,int)>>();var patch=new HashSet<(int,int)>();var world=new Dictionary<MeshFilter,Vector3[]>();
  foreach(var mf in mfs){var w=mf.sharedMesh.vertices.Select(v=>mf.transform.TransformPoint(v)).ToArray();world[mf]=w;var t=mf.sharedMesh.triangles;
   for(int i=0;i<w.Length;i++){var k=K(w[i]);y[k]=y.TryGetValue(k,out var o)?Mathf.Max(o,w[i].y):w[i].y;if(InPatch(w[i]))patch.Add(k);}
   for(int i=0;i<t.Length;i+=3)for(int e=0;e<3;e++){var a=K(w[t[i+e]]);var b=K(w[t[i+(e+1)%3]]);if(a.Equals(b))continue;if(!nb.TryGetValue(a,out var na))nb[a]=na=new();na.Add(b);if(!nb.TryGetValue(b,out var nbb))nb[b]=nbb=new();nbb.Add(a);}}
  var free=patch.Where(k=>nb.ContainsKey(k)).ToList();var ny=new Dictionary<(int,int),float>(y);
  for(int it=0;it<20000;it++){float md=0;foreach(var k in free){float sum=0;int c=0;foreach(var o in nb[k]){sum+=ny[o];c++;}float v=sum/c;md=Mathf.Max(md,Mathf.Abs(v-ny[k]));ny[k]=v;}if(md<1e-5f&&it>100)break;}
  float up=0,down=0;foreach(var k in free){up=Mathf.Max(up,ny[k]-y[k]);down=Mathf.Min(down,ny[k]-y[k]);}
  Note($"Part A spike {Scene}: {free.Count} terrain nodes in x {X0}-{X1}, z {Z0}-{Z1} (not under the ribbon) replaced by the smooth surface; change {down:F2} .. +{up:F2} m");
  // trees standing in the patch (+1 m): old ground under the trunk
  var trunks=Object.FindObjectsByType<Collider>(FindObjectsSortMode.None).Where(c=>c.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0&&c.bounds.center.x>=X0-1&&c.bounds.center.x<=X1+1&&c.bounds.center.z>=Z0-1&&c.bounds.center.z<=Z1+1).ToList();
  var oldG=trunks.ToDictionary(c=>c,c=>TileTop(c.bounds.center.x,c.bounds.center.z));
  bool terrainOnly=Environment.GetEnvironmentVariable("AUTHOR_SPIKE_TERRAIN_ONLY")=="1";if(terrainOnly)trunks.Clear();
  if(dry){foreach(var c in trunks)Note($"Part A spike {Scene}: tree {Path(c.transform)} at {c.bounds.center:F1} base {c.bounds.min.y:F2} ground {oldG[c]:F2} (dry run)");return;}
  foreach(var mf in mfs){var w=world[mf];bool any=false;var lv=mf.sharedMesh.vertices;for(int i=0;i<w.Length;i++){var k=K(w[i]);if(!patch.Contains(k))continue;w[i].y=ny[k];lv[i]=mf.transform.InverseTransformPoint(w[i]);any=true;}
   if(any){var m=Own(mf);m.vertices=lv;m.RecalculateNormals();m.RecalculateBounds();Refresh(mf);}}
  Physics.SyncTransforms();
  var batches=Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Where(m=>{var p=Path(m.transform);return m.sharedMesh&&m.GetComponent<Renderer>()&&m.GetComponent<Renderer>().bounds.Intersects(area)&&(p.Contains("tree")||p.Contains("Tree")||p.Contains("Woods")||p.Contains("woodland")||p.Contains("foliage")||p.Contains("canopy"));}).ToArray();
  var allTrunks=Object.FindObjectsByType<Collider>(FindObjectsSortMode.None).Where(c=>c.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0&&area.Contains(c.bounds.center)).ToList();
  foreach(var c in trunks){float g=TileTop(c.bounds.center.x,c.bounds.center.z);float dy=g-oldG[c];if(float.IsNaN(dy)||Mathf.Abs(dy)<.03f){Note($"Part A spike {Scene}: tree {Path(c.transform)} at {c.bounds.center:F1}: ground change {dy:F2} m; not moved");continue;}
   var ctr=new Vector2(c.bounds.center.x,c.bounds.center.z);int pieces=0;
   foreach(var mf in batches){var m=mf.sharedMesh;var vs=m.vertices;var tr=mf.transform;var t=m.triangles;
    var parent=Enumerable.Range(0,vs.Length).ToArray();int Find(int a){while(parent[a]!=a){parent[a]=parent[parent[a]];a=parent[a];}return a;}
    var weld=new Dictionary<Vector3Int,int>();for(int i=0;i<vs.Length;i++){var k=Vector3Int.RoundToInt(vs[i]*200);if(weld.TryGetValue(k,out var j))parent[Find(i)]=Find(j);else weld[k]=i;}
    for(int i=0;i<t.Length;i+=3){parent[Find(t[i+1])]=Find(t[i]);parent[Find(t[i+2])]=Find(t[i]);}
    var groups=new Dictionary<int,List<int>>();for(int i=0;i<vs.Length;i++){var w=tr.TransformPoint(vs[i]);if(new Vector2(w.x,w.z).magnitude<0)continue;if((new Vector2(w.x,w.z)-ctr).magnitude>7)continue;int gidx=Find(i);if(!groups.TryGetValue(gidx,out var l))groups[gidx]=l=new();l.Add(i);}
    bool any=false;var nv=(Vector3[])vs.Clone();
    foreach(var grp in groups.Values){var cen=grp.Aggregate(Vector3.zero,(a,i)=>a+tr.TransformPoint(vs[i]))/grp.Count;var c2=new Vector2(cen.x,cen.z);if((c2-ctr).magnitude>4.5f)continue;
     var nearest=allTrunks.OrderBy(o=>(new Vector2(o.bounds.center.x,o.bounds.center.z)-c2).magnitude).First();if(nearest!=c)continue;if(cen.y<c.bounds.min.y-1.5f||cen.y>c.bounds.max.y+14)continue;
     var dl=tr.InverseTransformVector(Vector3.up*dy);foreach(var i in grp)nv[i]=vs[i]+dl;any=true;pieces++;}
    if(any){var own=Own(mf);own.vertices=nv;own.RecalculateBounds();Refresh(mf);}}
   c.transform.position+=Vector3.up*dy;EditorUtility.SetDirty(c.transform);
   Note($"Part A spike {Scene}: tree {Path(c.transform)} at {c.bounds.center:F1} moved {dy:F2} m with the ground (trunk collider + {pieces} batched pieces)");}}

 // ---------- Part B: AI pedal planning per authored flight (MountainFlights data only; no geometry) ----------
 // Forward Homeward: AI take-offs at 31.4 m/s overshoot onto the upper deck and crash (0.72 before-race); 29.0-29.3 land and
 // continue -> take-off limit 29.5 m/s. Reverse South Face: 36.4-36.8 m/s come up short of the receiving deck, 37.2+ land ->
 // the AI may enter the run-up at 31 m/s instead of the general 24 (27 still left the ATV at 36.5-36.9 m/s, short).
 static void Flights(bool dry){
  var mf=Object.FindAnyObjectByType<Racer.MountainFlights>();if(!mf){Note($"Part B {Scene}: no MountainFlights");return;}
  foreach(var f in mf.flights){float t0=f.aiTakeoffSpeed,e0=f.aiEntrySpeed;
   if(f.name=="Homeward Summit Flight")f.aiTakeoffSpeed=29.5f;
   if(f.name=="South Face Summit Flight")f.aiEntrySpeed=31f;
   Note($"Part B {Scene}: {f.name}: AI take-off limit {t0} -> {f.aiTakeoffSpeed}, AI run-up entry speed {e0} -> {f.aiEntrySpeed} (0 = unchanged){(dry?" (dry run)":"")}");}
  if(!dry)EditorUtility.SetDirty(mf);}

 // ---------- Part C: materials the weather code loads from Resources (so their shaders are included in the build) ----------
 static void WeatherMaterials(bool dry){
  Directory.CreateDirectory("Assets/Resources");
  Material Particle(string name,Color c){var m=new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit")){name=name};m.SetFloat("_Surface",1);m.SetFloat("_Blend",0);m.SetColor("_BaseColor",c);
   m.SetFloat("_SrcBlend",(float)UnityEngine.Rendering.BlendMode.SrcAlpha);m.SetFloat("_DstBlend",(float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);m.SetFloat("_ZWrite",0);m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");m.renderQueue=3050;return m;}
  void Save(Material m,string path){var old=AssetDatabase.LoadAssetAtPath<Material>(path);if(old){EditorUtility.CopySerialized(m,old);EditorUtility.SetDirty(old);}else AssetDatabase.CreateAsset(m,path);Note($"Part C: material {path} ({m.shader.name})");}
  if(dry)return;
  Save(Particle("WeatherRain",new Color(.78f,.82f,.90f,.55f)),"Assets/Resources/WeatherRain.mat");
  Save(Particle("WeatherSnow",new Color(1,1,1,.9f)),"Assets/Resources/WeatherSnow.mat");
  var stars=new Material(Shader.Find("Racer/NightStars")){name="NightStars"};Save(stars,"Assets/Resources/NightStars.mat");
  AssetDatabase.SaveAssets();}
}
