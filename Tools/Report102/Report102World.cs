using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 0.102 world edits (temporary editor tool; copied into Assets/Editor/Report102Temp only while it runs). PART=crest|shoulders|culvert, WORLD_DRY=1 only reports.
//  crest      Part A: the crest on Dan's Backyard Forward main trail before CP 1 (main s 96-130) smoothed (DansBackyardForward, FreeRoamWorld)
//  shoulders  Part B: sloped earth shoulders along both sides of the Tree-Top Trail dirt jump; Part E: along the Cabin boards' foot (both scenes)
//  culvert    Part C: the Forward storm drain's ceiling and walls solid from outside, the closed grate one solid collider down to the floor (DansBackyardForward)
public static class Report102World {
 static string Out=>Environment.GetEnvironmentVariable("PROBE_OUT");
 static bool Dry=>Environment.GetEnvironmentVariable("WORLD_DRY")=="1";
 static string V(Vector3 v)=>$"({v.x:F2},{v.y:F2},{v.z:F2})";
 static string P(Transform t){var s=t.name;while(t.parent){t=t.parent;s=t.name+"/"+s;}return s;}
 static readonly List<string> log=new();
 const string Dir="Assets/Scenery/Report102";
 static Mesh Save(Mesh m,string file){Directory.CreateDirectory(Dir);var path=Dir+"/"+file+".asset";var ex=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(ex){EditorUtility.CopySerialized(m,ex);return ex;}AssetDatabase.CreateAsset(m,path);return m;}
 static float GroundTop(Vector3 p,Func<Collider,bool> ok){float best=float.NaN;foreach(var h in Physics.RaycastAll(new Vector3(p.x,p.y+40,p.z),Vector3.down,120,~0,QueryTriggerInteraction.Ignore)){if(h.collider.attachedRigidbody||!ok(h.collider))continue;if(float.IsNaN(best)||h.point.y>best)best=h.point.y;}return best;}
 static void Normals(Mesh m,Vector3[] v,HashSet<int> changed){var tri=m.triangles;var nr=m.normals;var acc=new Dictionary<int,Vector3>();
  for(int t=0;t<tri.Length;t+=3){int a=tri[t],b=tri[t+1],c=tri[t+2];if(!changed.Contains(a)&&!changed.Contains(b)&&!changed.Contains(c))continue;var fn=Vector3.Cross(v[b]-v[a],v[c]-v[a]);foreach(var k in new[]{a,b,c}){acc.TryGetValue(k,out var s);acc[k]=s+fn;}}
  foreach(var kv in acc)if(kv.Value.sqrMagnitude>1e-12f)nr[kv.Key]=kv.Value.normalized;m.normals=nr;}

 public static void Run(){
  log.Clear();var part=Environment.GetEnvironmentVariable("PART");
  try{
   if(part=="crest")foreach(var sn in new[]{"DansBackyardForward","FreeRoamWorld"})Crest(sn);
   if(part=="shoulders"){var tree=new List<(Vector3 c,Vector3 f)>();foreach(var sn in new[]{"DansBackyardForward","FreeRoamWorld"})Shoulders(sn,tree);}
   if(part=="culvert")Culvert();
   if(part=="skirts")foreach(var sn in new[]{"DansBackyardForward","FreeRoamWorld"}){var sc=EditorSceneManager.OpenScene("Assets/Scenes/"+sn+".unity");var c=GameObject.Find("Backyard optional forest shortcuts").transform.Find("Dirt jump grounded sides").GetComponent<MeshCollider>();
    // Part B: the Tree-Top dirt jump's vertical side skirts snag a wheel or the body at the edge; the 0.102 shoulders now give the sides their
    // collision, so the skirts' collider is off (they are still drawn)
    log.Add($"{sn}: Dirt jump grounded sides collider was {(c.enabled?"on":"off")}, now off");if(!Dry){c.enabled=false;EditorSceneManager.MarkSceneDirty(sc);EditorSceneManager.SaveScene(sc);}}
   if(!Dry)AssetDatabase.SaveAssets();
  }catch(Exception e){log.Add("FAILED "+e);}
  Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/world-"+part+".txt",log);EditorApplication.Exit(0);}

 // ---------- Part A ----------
 // The trail climbs at up to 0.29 then turns flat within about 5 m (main s 104-110): a crest a vehicle leaves the ground on at any racing
 // speed. The centre-line profile over main s 86-136 (0.5 m samples, the ends fixed) is relaxed until its downward bend is nowhere sharper
 // than K (1/m), taking half of each correction from the crest and a quarter from each neighbour (the trail's mean height is kept);
 // the difference is applied to the ground across the trail (full within 6.5 m of the centre, fading out by 10 m; held near tree trunks).
 const float S0=80,S1=140,Hs=.5f;
 static void Crest(string sn){
  float K=float.Parse(Environment.GetEnvironmentVariable("CREST_K")??"0.016",System.Globalization.CultureInfo.InvariantCulture),KC=float.Parse(Environment.GetEnvironmentVariable("CREST_KC")??"0.02",System.Globalization.CultureInfo.InvariantCulture);
  var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+sn+".unity");Physics.SyncTransforms();log.Add("===== "+sn+" crest, K "+K+", KC "+KC);var pre=GameObject.Find("Backyard blue CP 1");float preY=float.Parse(Environment.GetEnvironmentVariable("CREST_PRE_GATE")??"0",System.Globalization.CultureInfo.InvariantCulture);if(pre&&preY!=0&&!Dry){pre.transform.position+=Vector3.up*preY;log.Add($"CP 1 gate put back by {preY:+0.00;-0.00} first");}
  var main=UnityEngine.Object.FindObjectsByType<Racer.RaceRoad>(FindObjectsSortMode.None).First(r=>r.name=="Forward navigation only - no road mesh");main.Initialize();
  var ground=GameObject.Find("Memory loop - north is +Z/Ground_560_320");var gf=ground.GetComponent<MeshFilter>();var gc=ground.GetComponent<MeshCollider>();var mesh=gf.sharedMesh;
  log.Add($"ground mesh {AssetDatabase.GetAssetPath(mesh)} (collider {AssetDatabase.GetAssetPath(gc.sharedMesh)})");
  var orig=AssetDatabase.LoadAssetAtPath<Mesh>(sn=="FreeRoamWorld"?"Assets/_Orig102/roam-orig.asset":"Assets/_Orig102/fwd-orig.asset");if(!orig||orig.vertexCount!=mesh.vertexCount)throw new Exception("original mesh missing or different");
  var probeGo=new GameObject("orig probe");probeGo.transform.SetPositionAndRotation(ground.transform.position,ground.transform.rotation);var oc=probeGo.AddComponent<MeshCollider>();oc.sharedMesh=orig;Physics.SyncTransforms();
  float G(Vector3 p)=>oc.Raycast(new Ray(new Vector3(p.x,p.y+30,p.z),Vector3.down),out var h,80)?h.point.y:float.NaN;
  float Gcur(Vector3 p)=>gc.Raycast(new Ray(new Vector3(p.x,p.y+30,p.z),Vector3.down),out var h,80)?h.point.y:float.NaN;
  int n=Mathf.RoundToInt((S1-S0)/Hs)+3;var y=new double[n];var y0=new double[n];float Sx(int i)=>S0+(i-1)*Hs;
  for(int i=0;i<n;i++){var c=main.At(Sx(i),out _);y[i]=y0[i]=G(c);if(double.IsNaN(y[i]))throw new Exception("no ground at main s "+Sx(i));}
  y[0]=y0[0]=y0[1]; // the paving before S0 is flat: the trail leaves it with no kink
  double Curv(double[] a,int i)=>(a[i-1]+a[i+1]-2*a[i])/(Hs*Hs);bool Free(int j)=>j>=2&&j<=n-3;
  for(int it=0;it<400000;it++){double worst=0;for(int i=1;i<n-1;i++){double c=Curv(y,i);double need=c< -K?(-K-c):c>KC?-(c-KC):0;if(need==0)continue;worst=Math.Max(worst,Math.Abs(need));
    double fi=Free(i)?1:0,fl=Free(i-1)?1:0,fr=Free(i+1)?1:0;double den=2*fi+.5*(fl+fr);if(den==0)continue;double t=need*Hs*Hs/den;y[i]-=t*fi;y[i-1]+=.5*t*fl;y[i+1]+=.5*t*fr;}if(worst<1e-6)break;}
  var delta=new float[n];float most=0,mostAt=0,sum=0;for(int i=0;i<n;i++){delta[i]=(float)(y[i]-y0[i]);sum+=delta[i];if(Mathf.Abs(delta[i])>Mathf.Abs(most)){most=delta[i];mostAt=Sx(i);}}
  double before=0,after=0,sagB=0,sagA=0;float beforeAt=0;for(int i=1;i<n-1;i++){if(-Curv(y0,i)>before){before=-Curv(y0,i);beforeAt=Sx(i);}after=Math.Max(after,-Curv(y,i));sagB=Math.Max(sagB,Curv(y0,i));sagA=Math.Max(sagA,Curv(y,i));}log.Add($"sharpest dip (upward bend) before {sagB:F3}/m, after {sagA:F3}/m");
  log.Add($"centre line s {S0}-{S1}: sharpest downward bend before {before:F3}/m at s {beforeAt:F1} (stays on the ground to {Math.Sqrt(9.81/before):F1} m/s), after {after:F3}/m ({Math.Sqrt(9.81/Math.Max(after,1e-4)):F1} m/s); largest change {most:+0.00;-0.00} m at s {mostAt:F1}; mean change {sum/n:+0.000;-0.000} m");
  for(int i=1;i<n;i+=4)log.Add($"   s {Sx(i),6:F1}: {y0[i]:F2} -> {y[i]:F2} ({delta[i]:+0.00;-0.00})");
  float D(float s){if(s<=S0||s>=S1)return 0;float u=(s-S0)/Hs+1;int i=Mathf.Min(n-2,(int)u);return Mathf.Lerp(delta[i],delta[i+1],u-i);}
  var pavings=UnityEngine.Object.FindObjectsByType<MeshCollider>(FindObjectsSortMode.None).Where(m=>m.name.StartsWith("Ground_Dan")||m.name.StartsWith("Ground_Black asphalt")).ToArray();
  var trunks=UnityEngine.Object.FindObjectsByType<BoxCollider>(FindObjectsSortMode.None).Where(b=>b.name.Contains(" trunk ")).Select(b=>b.bounds.center).Where(c=>{float s=main.Project(c,out float l);return s>S0-15&&s<S1+15&&l<20;}).ToList();
  float Weight(Vector3 w,out float s){float lat;s=main.Project(w,out lat);if(s<S0+.3f||s>S1)return 0;var c=main.At(s,out _);if(Mathf.Abs(c.y-w.y)>4)return 0;foreach(var pv in pavings)if(pv.Raycast(new Ray(new Vector3(w.x,w.y+5,w.z),Vector3.down),out _,10))return 0;float wt=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(5.5f,8f,lat));
   foreach(var t in trunks){float d=new Vector2(w.x-t.x,w.z-t.z).magnitude;if(lat>4)wt*=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.8f,2.2f,d));}return wt;}
  foreach(var t in trunks){float s;float wt=Weight(new Vector3(t.x,G(t),t.z),out s);float lat;main.Project(t,out lat);if(Mathf.Abs(D(s))>.02f)log.Add($"   trunk at {V(t)} (main s {s:F1}, {lat:F1} m off): ground change there {wt*D(s):+0.00;-0.00} (held)");}
  if(Dry)return;
  var v=mesh.vertices;var ov=orig.vertices;var M=ground.transform.localToWorldMatrix;var Mi=ground.transform.worldToLocalMatrix;var changed=new HashSet<int>();float maxMove=0;
  for(int i=0;i<v.Length;i++){var w=M.MultiplyPoint3x4(ov[i]);float s;float wt=Weight(w,out s);float d=wt>0?wt*D(s):0;w.y+=d;var nv=Mi.MultiplyPoint3x4(w);if((nv-v[i]).sqrMagnitude<1e-10f)continue;v[i]=nv;changed.Add(i);maxMove=Mathf.Max(maxMove,Mathf.Abs(d));}
  // dependents: what stood on the old ground here (the CP 1 gate posts and any other prop) moves with it
  var moved=new List<string>();var movedSet=new HashSet<Transform>();
  foreach(var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include,FindObjectsSortMode.None)){var b=r.bounds;if(b.size.x>30||b.size.z>30)continue;var bc=new Vector3(b.center.x,b.min.y,b.center.z);float s;float wt=Weight(new Vector3(bc.x,G(bc),bc.z),out s);if(wt<=0)continue;float d=0;
   float g=Gcur(bc);if(float.IsNaN(g)||Mathf.Abs(b.min.y-g)>.6f)continue;if(r.name.StartsWith("Ground_"))continue;float gNew=float.IsNaN(g)?g:g;d=(G(bc)+wt*D(s))-g;if(Mathf.Abs(d)<.01f)continue;var mover=r.transform.parent&&r.transform.parent.name.StartsWith("Backyard blue CP")?r.transform.parent:r.transform;if(movedSet.Add(mover)){mover.position+=Vector3.up*d;moved.Add($"{P(mover)} {d:+0.00;-0.00}");}}
  mesh.vertices=v;Normals(mesh,v,changed);mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);gc.sharedMesh=null;gc.sharedMesh=mesh;UnityEngine.Object.DestroyImmediate(probeGo);Physics.SyncTransforms();
  log.Add($"ground: {changed.Count} vertices moved (at most {maxMove:F2} m); props reseated: {(moved.Count==0?"none":string.Join("; ",moved))}");
  // after: centre and both trail edges
  float Gn(Vector3 p)=>Gcur(p);foreach(float l in new[]{-3f,0f,3f}){double worst=0;float at=0;float prevY=float.NaN,prevS=float.NaN;for(float s=S0;s<=S1;s+=Hs){var c=main.At(s,out var f);var right=Vector3.Cross(Vector3.up,new Vector3(f.x,0,f.z).normalized);float yy=Gn(c+right*l);float sl=(yy-prevY)/Hs;if(!float.IsNaN(prevS)&&prevS-sl>worst*Hs){worst=(prevS-sl)/Hs;at=s;}prevS=float.IsNaN(prevY)?float.NaN:sl;prevY=yy;}
   log.Add($"   after, {l:+0;-0;0} m: sharpest downward bend {worst:F3}/m at s {at:F1}");}
  EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);}

 // ---------- Parts B and E ----------
 // A shoulder: from the ramp's edge (just under its top) the surface falls away at 1 in SLOPE^-1 until it meets the ground, up to 6 m out; under
 // the ramp, past the shoulder and wherever a trunk or other solid object stands, it lies 8 cm under the ground (hidden). Rows every 0.25 m
 // along the line, columns every 0.25 m out from the edge (row by row, so the first column is exactly on the edge). Collider named Ground_.
 const float SLOPE=.45f,Reach=6f,Cell=.25f;
 static void Shoulders(string sn,List<(Vector3 c,Vector3 f)> tree){
  var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+sn+".unity");Physics.SyncTransforms();log.Add("===== "+sn+" shoulders");
  var root=GameObject.Find("Backyard optional forest shortcuts").transform;
  // Part B line: the Tree-Top Trail (race scene); Free Roam has the same jump but no route, so the race's line is reused
  if(tree.Count==0){var tt=UnityEngine.Object.FindObjectsByType<Racer.WoodlandRoute>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(w=>w.title=="Tree-Top Trail");tt.Initialize();for(float s=8;s<=31;s+=Cell){var c=tt.At(s,out var f);tree.Add((c,f));}}
  var cabin=UnityEngine.Object.FindObjectsByType<Racer.WoodlandRoute>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(w=>w.title=="Abandoned Cabin Jump");cabin.Initialize();var cab=new List<(Vector3 c,Vector3 f)>();for(float s=24.5f;s<=35.25f;s+=Cell){var c=cabin.At(s,out var f);cab.Add((c,f));}
  var dirt=root.Find("Takeoff - Short treehouse dirt jump").GetComponent<MeshCollider>();var boards=root.GetComponentsInChildren<Collider>(true).First(c=>c.name=="Takeoff - Leaning boards through cabin roof");
  Build(sn,"Ground_Tree-Top jump shoulders (0.102)","Tree-Top jump shoulders",tree,dirt,root.Find("Takeoff - Short treehouse dirt jump").GetComponent<MeshRenderer>().sharedMaterial,root,0);
  var runup=root.Find("Ground_Cabin run-up surface (0.101)");
  Build(sn,"Ground_Cabin board shoulders (0.102)","Cabin board shoulders",cab,boards,runup.GetComponent<MeshRenderer>().sharedMaterial,root,.6f);
  if(!Dry){EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);}}
 static readonly string[] Own={"Takeoff - ","Dirt jump grounded sides","Leaning weathered board","Ground_"};
 static void Build(string sn,string name,string file,List<(Vector3 c,Vector3 f)> line,Collider ramp,Material mat,Transform root,float flat){
  var old=root.Find(name);if(old)UnityEngine.Object.DestroyImmediate(old.gameObject);Physics.SyncTransforms();
  bool IsGround(Collider c)=>c.name.StartsWith("Ground_");
  float RampY(Vector3 p)=>ramp.Raycast(new Ray(new Vector3(p.x,p.y+20,p.z),Vector3.down),out var h,40)&&h.normal.y>.7f?h.point.y:float.NaN;
  bool Obstacle(Vector3 p,float g){foreach(var h in Physics.RaycastAll(new Vector3(p.x,g+12,p.z),Vector3.down,12.3f,~0,QueryTriggerInteraction.Ignore)){if(h.collider.attachedRigidbody||h.collider==ramp)continue;if(Own.Any(o=>h.collider.name.StartsWith(o)))continue;if(h.point.y>g+.2f)return true;}return false;}
  var vs=new List<Vector3>();var ts=new List<int>();int visible=0,blocked=0,caps=0;float tallest=0;var rowsLog=new List<string>();
  foreach(int side in new[]{-1,1}){
   var rows=new List<(Vector3 c,Vector3 n,float e,float ey)>();
   foreach(var (c,f) in line){var fl=new Vector3(f.x,0,f.z).normalized;var right=Vector3.Cross(Vector3.up,fl);float e=float.NaN,ey=float.NaN;
    for(float l=0;l<=6;l+=.02f){float yy=RampY(c+right*side*l);if(float.IsNaN(yy)){if(l>.3f)break;continue;}e=l;ey=yy;}
    if(float.IsNaN(e))continue;rows.Add((c,right*side,e,ey));}
   if(rows.Count<2){log.Add($"  {name} side {side}: no ramp found");continue;}
   int cols=Mathf.RoundToInt(Reach/Cell)+1,b0=vs.Count;
   for(int r=0;r<rows.Count;r++){var (c,n,e,ey)=rows[r];float rowTall=0;
    for(int j=0;j<cols;j++){float d=j*Cell;var p=c+n*(e+d);p.y=ey;float g=GroundTop(p,IsGround);if(float.IsNaN(g))g=ey-3;
     float h=ey-.01f-SLOPE*Mathf.Max(0,d-flat);float y=h;bool obstacle=Obstacle(p,Mathf.Max(g,h));if(obstacle)blocked++;
     if(obstacle||h<g-.02f)y=g-.08f;else{visible++;rowTall=Mathf.Max(rowTall,h-g);}
     if(j==0)y=ey-.01f;p.y=y;vs.Add(p);}
    tallest=Mathf.Max(tallest,rowTall);if(r%8==0)rowsLog.Add($"    side {side} row {r}: edge {e:F2} m out at {ey:F2}, shoulder up to {rowTall:F2} m above the ground");}
   for(int r=1;r<rows.Count;r++)for(int j=1;j<cols;j++){int a=b0+(r-1)*cols+j-1,b=a+1,cc=b0+r*cols+j-1,dd=cc+1;ts.AddRange(new[]{a,cc,b,b,cc,dd});}
   // caps: the shoulder's far end and its outer edge go straight down to 10 cm under the ground, so no open edge shows
   void Cap(int i0,int i1){var a=vs[i0];var b=vs[i1];float ga=GroundTop(a,IsGround),gb=GroundTop(b,IsGround);if(float.IsNaN(ga)||float.IsNaN(gb))return;if(a.y<ga+.01f&&b.y<gb+.01f)return;int k=vs.Count;vs.Add(a);vs.Add(b);vs.Add(new Vector3(b.x,gb-.1f,b.z));vs.Add(new Vector3(a.x,ga-.1f,a.z));ts.AddRange(new[]{k,k+1,k+2,k,k+2,k+3});caps++;}
   for(int j=1;j<cols;j++)Cap(b0+(rows.Count-1)*cols+j-1,b0+(rows.Count-1)*cols+j);
   for(int r=1;r<rows.Count;r++)Cap(b0+(r-1)*cols+cols-1,b0+r*cols+cols-1);
  }
  var m=new Mesh{name=sn+" "+file+" (0.102)"};m.SetVertices(vs);m.SetTriangles(ts,0);m.SetUVs(0,vs.Select(p=>new Vector2(p.x*.3f,p.z*.3f)).ToList());m.RecalculateNormals();
  // every face up
  int flipped=0;int nts=ts.Count;for(int k=0;k<nts;k+=3){var fn=Vector3.Cross(vs[ts[k+1]]-vs[ts[k]],vs[ts[k+2]]-vs[ts[k]]);if(fn.y<-1e-6f){(ts[k+1],ts[k+2])=(ts[k+2],ts[k+1]);flipped++;}else if(Mathf.Abs(fn.normalized.y)<.2f){int q=vs.Count;vs.Add(vs[ts[k]]);vs.Add(vs[ts[k+2]]);vs.Add(vs[ts[k+1]]);ts.AddRange(new[]{q,q+1,q+2});}}m.SetVertices(vs);m.SetUVs(0,vs.Select(p=>new Vector2(p.x*.3f,p.z*.3f)).ToList());m.SetTriangles(ts,0);m.RecalculateNormals();m.RecalculateBounds();
  log.Add($"  {name}: {vs.Count} vertices, {ts.Count/3} triangles, {visible} points above the ground (shoulder up to {tallest:F2} m high), {blocked} held under the ground at obstacles, {caps} cap quads; bounds {V(m.bounds.min)}-{V(m.bounds.max)}");
  log.AddRange(rowsLog);
  if(Dry)return;
  m=Save(m,sn+"-"+file);var go=new GameObject(name);go.transform.SetParent(root,false);go.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);go.AddComponent<MeshFilter>().sharedMesh=m;go.AddComponent<MeshRenderer>().sharedMaterial=mat;go.AddComponent<MeshCollider>().sharedMesh=m;go.layer=ramp.gameObject.layer;go.isStatic=true;Physics.SyncTransforms();
  // anything small standing on the ground inside the shoulders' footprint that the shoulder now buries (reported; posts and props are lifted)
  var mc=go.GetComponent<MeshCollider>();
  foreach(var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include,FindObjectsSortMode.None)){var bb=r.bounds;if(r.gameObject==go||bb.size.x>12||bb.size.z>12||r.name.StartsWith("Ground_"))continue;if(!m.bounds.Intersects(bb))continue;
   if(!mc.Raycast(new Ray(new Vector3(bb.center.x,bb.max.y+10,bb.center.z),Vector3.down),out var hh,bb.size.y+20))continue;if(hh.point.y<bb.min.y+.05f)continue;log.Add($"    stands in the shoulder: {P(r.transform)} bottom {bb.min.y:F2}, shoulder {hh.point.y:F2}");}}

 // ---------- Part C ----------
 static void Culvert(){
  var sc=EditorSceneManager.OpenScene("Assets/Scenes/DansBackyardForward.unity");Physics.SyncTransforms();log.Add("===== DansBackyardForward culvert");
  foreach(var g in UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(m=>m.name=="Culvert roof top (0.102)").Select(m=>m.gameObject).ToList())UnityEngine.Object.DestroyImmediate(g);
  var shell=GameObject.Find("Long storm culvert walls and ceiling");var m=shell.GetComponent<MeshFilter>().sharedMesh;var tr=shell.transform;
  var lintel=GameObject.Find("Culvert mouth lintel");var mat=lintel.GetComponent<MeshRenderer>().sharedMaterial;var lt=lintel.transform;
  var axisIn=Vector3.ProjectOnPlane(lt.forward,Vector3.up).normalized; // into the tunnel
  {var sc0=shell.GetComponent<Renderer>().bounds.center;if(Vector3.Dot(sc0-lt.position,axisIn)<0){axisIn=-axisIn;log.Add("  lintel forward points out of the tunnel; reversed");}}
  float Inside(Vector3 p)=>Vector3.Dot(Vector3.ProjectOnPlane(p-lt.position,Vector3.up),axisIn);
  var wv=m.vertices.Select(v=>tr.TransformPoint(v)).ToArray();var t=m.triangles;var verts=new List<Vector3>();var tris=new List<int>();int kept=0,buried=0;
  float GroundOnly(Vector3 p){float best=float.NaN;foreach(var h in Physics.RaycastAll(new Vector3(p.x,p.y+40,p.z),Vector3.down,80,~0,QueryTriggerInteraction.Ignore)){if(!h.collider.name.StartsWith("Ground_")||h.collider.name.StartsWith("Ground_Culvert"))continue;if(float.IsNaN(best)||h.point.y>best)best=h.point.y;}return best;}
  // the ceiling's down-facing triangles inside the tunnel that the ground does not cover: turned up, 4 cm above
  for(int i=0;i<t.Length;i+=3){var a=wv[t[i]];var b=wv[t[i+1]];var c=wv[t[i+2]];var n=Vector3.Cross(b-a,c-a).normalized;if(n.y>-.3f)continue;var ce=(a+b+c)/3;if(Inside(ce)< -.2f)continue;
   float g=GroundOnly(ce);if(!float.IsNaN(g)&&g>ce.y+1f){buried++;continue;}int k=verts.Count;var up=Vector3.up*.04f;verts.Add(a+up);verts.Add(c+up);verts.Add(b+up);tris.Add(k);tris.Add(k+1);tris.Add(k+2);kept++;}
  var key=new Func<Vector3,(int,int,int)>(v=>(Mathf.RoundToInt(v.x*200),Mathf.RoundToInt(v.y*200),Mathf.RoundToInt(v.z*200)));
  Dictionary<((int,int,int),(int,int,int)),(Vector3 a,Vector3 b,Vector3 ce,int n)> Edges(int from){var edges=new Dictionary<((int,int,int),(int,int,int)),(Vector3 a,Vector3 b,Vector3 ce,int n)>();
   for(int i=from;i<tris.Count;i+=3){var ce=(verts[tris[i]]+verts[tris[i+1]]+verts[tris[i+2]])/3;for(int e=0;e<3;e++){var a=verts[tris[i+e]];var b=verts[tris[i+(e+1)%3]];var ka=key(a);var kb=key(b);var k2=ka.CompareTo(kb)<0?(ka,kb):(kb,ka);if(edges.TryGetValue(k2,out var v))edges[k2]=(v.a,v.b,v.ce,v.n+1);else edges[k2]=(a,b,ce,1);}}return edges;}
  // a skirt from every open edge of the roof except at the mouth: 1 m out, 0.2 m down, so the ground meets the concrete with no sliver
  int skirts=0;foreach(var e in Edges(0).Values){if(e.n!=1)continue;var mid=(e.a+e.b)/2;if(Inside(mid)<1.2f)continue;var along=e.b-e.a;along.y=0;if(along.sqrMagnitude<1e-6f)continue;var outw=Vector3.Cross(Vector3.up,along.normalized);if(Vector3.Dot(outw,mid-e.ce)<0)outw=-outw;var o=outw*1f+Vector3.down*.2f;
   int k=verts.Count;verts.Add(e.a);verts.Add(e.b);verts.Add(e.b+o);verts.Add(e.a+o);void T(int x,int y,int z){var nn=Vector3.Cross(verts[y]-verts[x],verts[z]-verts[x]);if(nn.y<0){int q=y;y=z;z=q;}tris.Add(x);tris.Add(y);tris.Add(z);}T(k,k+1,k+2);T(k,k+2,k+3);skirts++;}
  // the side walls where they stand above the ground outside: an outside face, so a vehicle stops against the wall instead of passing in
  int w0=tris.Count,walls=0;
  for(int i=0;i<t.Length;i+=3){var a=wv[t[i]];var b=wv[t[i+1]];var c=wv[t[i+2]];var n=Vector3.Cross(b-a,c-a).normalized;if(n.y<=-.3f||n.y>=.3f)continue;var ce=(a+b+c)/3;if(Inside(ce)< -.2f)continue;var outw=-n;outw.y=0;if(outw.sqrMagnitude<1e-4f)continue;outw.Normalize();float top=Mathf.Max(a.y,Mathf.Max(b.y,c.y));
   float g=GroundOnly(ce+outw*.6f);if(!float.IsNaN(g)&&g>top-.05f)continue;int k=verts.Count;var off=outw*.04f;verts.Add(a+off);verts.Add(c+off);verts.Add(b+off);tris.Add(k);tris.Add(k+1);tris.Add(k+2);walls++;}
  // their lowest open edges carried 1.2 m down, so the ground meets the concrete along the foot of the wall
  int feet=0;foreach(var e in Edges(w0).Values){if(e.n!=1)continue;var mid=(e.a+e.b)/2;if(mid.y>e.ce.y-.05f||Mathf.Abs(e.a.y-e.b.y)>1.5f)continue;
   Vector3 face=Vector3.zero;for(int i=w0;i<tris.Count;i+=3){var c3=(verts[tris[i]]+verts[tris[i+1]]+verts[tris[i+2]])/3;if((c3-e.ce).sqrMagnitude<1e-6f){face=Vector3.Cross(verts[tris[i+1]]-verts[tris[i]],verts[tris[i+2]]-verts[tris[i]]);break;}}
   int k=verts.Count;var dn=Vector3.down*1.2f;verts.Add(e.a);verts.Add(e.b);verts.Add(e.b+dn);verts.Add(e.a+dn);var q=Vector3.Cross(verts[k+1]-verts[k],verts[k+2]-verts[k]);
   if(Vector3.Dot(q,face)<0){tris.AddRange(new[]{k,k+2,k+1,k,k+3,k+2});}else tris.AddRange(new[]{k,k+1,k+2,k,k+2,k+3});feet++;}
  var bounds=new Bounds(verts[0],Vector3.zero);foreach(var v in verts)bounds.Encapsulate(v);
  log.Add($"roof: ceiling triangles turned up {kept} (under the ground, skipped {buried}); skirts {skirts}; wall outside faces {walls}; wall feet {feet}; bounds {V(bounds.min)}-{V(bounds.max)}; material {mat.name}");
  // the grate: one solid box across the whole mouth (pier to pier, 0.4 m under the floor to the lintel), and the bars drawn down to the floor
  var grate=shell.transform.parent.Find("Forward closed drainage grate");var floor=GameObject.Find("Ground_Culvert seamless floor").GetComponent<Collider>();
  float lowest=float.MaxValue;foreach(Transform bar in grate){if(!bar.GetComponent<Renderer>())continue;var bp=bar.position;if(floor.Raycast(new Ray(new Vector3(bp.x,bp.y+10,bp.z),Vector3.down),out var fh,30))lowest=Mathf.Min(lowest,fh.point.y);}
  for(float x=-5.3f;x<=5.31f;x+=.5f){var p=lt.position+lt.right*x;if(floor.Raycast(new Ray(new Vector3(p.x,p.y+10,p.z),Vector3.down),out var fh,30))lowest=Mathf.Min(lowest,fh.point.y);}
  float lintelBottom=lintel.GetComponent<Collider>().bounds.min.y;var barInfo=new List<string>();
  log.Add($"grate: {grate.childCount} bars, the floor across the mouth lowest {lowest:F2}, lintel bottom {lintelBottom:F2}, bars from {grate.Cast<Transform>().Where(b=>b.GetComponent<Renderer>()).Min(b=>b.GetComponent<Renderer>().bounds.min.y):F2} to {grate.Cast<Transform>().Where(b=>b.GetComponent<Renderer>()).Max(b=>b.GetComponent<Renderer>().bounds.max.y):F2}");
  if(Dry)return;
  foreach(Transform bar in grate.Cast<Transform>().ToList()){if(bar.name=="Solid closed grate (0.102)"){UnityEngine.Object.DestroyImmediate(bar.gameObject);continue;}
   var r=bar.GetComponent<Renderer>().bounds;float top=r.max.y,bottom=lowest-.3f;var s=bar.localScale;var p=bar.position;bar.localScale=new Vector3(s.x,top-bottom,s.z);bar.position=new Vector3(p.x,(top+bottom)/2,p.z);}
  var solid=new GameObject("Solid closed grate (0.102)");solid.transform.SetParent(grate,false);solid.transform.SetPositionAndRotation(new Vector3(lt.position.x,(lowest-.4f+lintelBottom)/2,lt.position.z),Quaternion.LookRotation(axisIn));
  var box=solid.AddComponent<BoxCollider>();box.size=new Vector3(lintel.transform.lossyScale.x,lintelBottom-(lowest-.4f),.3f);solid.layer=grate.GetChild(0).gameObject.layer;
  var mesh=new Mesh{name="DansBackyardForward-Report102-CulvertRoof",indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};mesh.SetVertices(verts);mesh.SetTriangles(tris,0);mesh.RecalculateNormals();mesh.RecalculateBounds();mesh.SetUVs(0,verts.Select(v=>new Vector2(v.x*.25f,v.z*.25f)).ToList());
  mesh=Save(mesh,mesh.name);var go=new GameObject("Culvert roof top (0.102)");go.transform.SetParent(shell.transform.parent,true);go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=mat;go.AddComponent<MeshCollider>().sharedMesh=mesh;go.layer=shell.layer;
  log.Add($"solid grate box {V(box.bounds.min)}-{V(box.bounds.max)}; active in the editor {solid.activeInHierarchy} (follows the grate: closed in the Forward race only)");
  EditorSceneManager.MarkSceneDirty(sc);EditorSceneManager.SaveScene(sc);}
}
