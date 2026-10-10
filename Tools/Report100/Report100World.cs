using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// Temporary 0.100 world tools (copied into Assets/Editor/Report100Temp only while they run).
public static class Report100Section {
 static string Out=>Environment.GetEnvironmentVariable("PROBE_OUT");
 static float F(string s)=>float.Parse(s,System.Globalization.CultureInfo.InvariantCulture);
 // PROBE_RANGE="s0,s1,step,l0,l1,lstep": cross-sections of the race road; a cell = road surface - ground (metres, rounded; . within 0.5, ^ above, # more than 9),
 // R = this road, O = another race road / trail surface; plus a list of the colliders hit at each station's left side
 public static void Run(){
  var log=new List<string>();var r=Environment.GetEnvironmentVariable("PROBE_RANGE").Split(',');float s0=F(r[0]),s1=F(r[1]),st=F(r[2]),l0=F(r[3]),l1=F(r[4]),ls=F(r[5]);
  foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){
   EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();var road=UnityEngine.Object.FindAnyObjectByType<Racer.RaceDirector>().road;road.Initialize();
   var others=new List<(Racer.RaceRoad rr,Racer.WoodlandRoute w)>();
   foreach(var rr in UnityEngine.Object.FindObjectsByType<Racer.RaceRoad>(FindObjectsInactive.Exclude,FindObjectsSortMode.None))if(rr!=road&&rr.points!=null&&rr.points.Length>3){rr.Initialize();others.Add((rr,null));}
   foreach(var w in UnityEngine.Object.FindObjectsByType<Racer.WoodlandRoute>(FindObjectsInactive.Exclude,FindObjectsSortMode.None))if(w.points!=null&&w.points.Length>3){w.Initialize();others.Add((null,w));}
   log.Add($"== {scene}: road length {road.Length:F0}; columns lateral {l0}..{l1} step {ls} (negative = left)");
   var hitNames=new Dictionary<string,int>();
   for(float s=s0;s<=s1;s+=st){var c=road.At(s,out var f);var right=Vector3.Cross(Vector3.up,Vector3.ProjectOnPlane(f,Vector3.up).normalized);float hw=road.HalfWidth(s);var sb=new System.Text.StringBuilder($"s{s,5:F0} ({c.x,5:F0},{c.y,4:F0},{c.z,5:F0}) hw{hw:F0} ");
    for(float l=l0;l<=l1;l+=ls){var p=c+right*l;string ch="n";if(Physics.Raycast(new Vector3(p.x,c.y+60,p.z),Vector3.down,out var h,300,~0,QueryTriggerInteraction.Ignore)){float d=c.y-h.point.y;
      bool other=false;foreach(var (rr,w) in others){if(rr!=null){float t=rr.Project(p,out float lat);if(Mathf.Abs(lat)<=rr.HalfWidth(t)&&Mathf.Abs(rr.At(t,out _).y-h.point.y)<1.5f)other=true;}else{w.Project(p,out float lat);if(Mathf.Abs(lat)<=w.halfWidth)other=true;}}
      ch=Mathf.Abs(l)<=hw?"R":other?"O":d<-.5f?"^":d<.5f?".":d>9.5f?"#":Mathf.RoundToInt(d).ToString();if(l<-hw){hitNames.TryGetValue(h.collider.name,out var q);hitNames[h.collider.name]=q+1;}}sb.Append(ch);}
    log.Add(sb.ToString());}
   log.Add("left-side top colliders:");foreach(var kv in hitNames.OrderByDescending(k=>k.Value).Take(30))log.Add($"  {kv.Value,5} {kv.Key}");
   foreach(var (rr,w) in others){var p=road.At((s0+s1)/2,out _);if(rr!=null){float t=rr.Project(p,out float lat);log.Add($"  other road {rr.name} nearest {Vector3.Distance(rr.At(t,out _),p):F0} m");}else{float t=w.Project(p,out float lat);log.Add($"  trail {w.title} nearest lateral {lat:F0}");}}}
  Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/sections.txt",log);EditorApplication.Exit(0);}
}
public static class Report100Shots {
 static string Out=>Environment.GetEnvironmentVariable("PROBE_OUT");
 // PROBE_VIEWS="name,x,y,z,yaw,pitch[,ortho size]|..." renders the saved scene (authored ground, roads, objects; no runtime trees) from each eye point; ortho size > 0 = a top-down orthographic view
 public static void Run(){
  Directory.CreateDirectory(Out);var inv=System.Globalization.CultureInfo.InvariantCulture;
  foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){
   EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");
   var go=new GameObject("shotcam");var cam=go.AddComponent<Camera>();cam.fieldOfView=60;cam.nearClipPlane=.3f;cam.farClipPlane=2500;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.55f,.7f,.9f);
   go.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
   foreach(var v in Environment.GetEnvironmentVariable("PROBE_VIEWS").Split('|')){var a=v.Split(',');
    go.transform.SetPositionAndRotation(new Vector3(float.Parse(a[1],inv),float.Parse(a[2],inv),float.Parse(a[3],inv)),Quaternion.Euler(float.Parse(a[5],inv),float.Parse(a[4],inv),0));
    float ortho=a.Length>6?float.Parse(a[6],inv):0;cam.orthographic=ortho>0;if(ortho>0)cam.orthographicSize=ortho;
    var rt=new RenderTexture(1600,900,24);cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;var tex=new Texture2D(1600,900,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1600,900),0,0);tex.Apply();RenderTexture.active=null;
    File.WriteAllBytes($"{Out}/{scene}-{a[0]}.png",tex.EncodeToPNG());}
   UnityEngine.Object.DestroyImmediate(go);}
  EditorApplication.Exit(0);}
}
public static class Report100Map {
 static string Out=>Environment.GetEnvironmentVariable("PROBE_OUT");
 static float F(string s)=>float.Parse(s,System.Globalization.CultureInfo.InvariantCulture);
 // PROBE_BOX="x0,x1,z0,z1,step,yref": top-surface height map; cell = (top hit y - yref) rounded to metres: 0-9, a-z for -1..-26 (a=-1), '+' >9, 'n' nothing hit; R = on the race road, O = another road
 public static void Run(){
  var log=new List<string>();var b=Environment.GetEnvironmentVariable("PROBE_BOX").Split(',');float x0=F(b[0]),x1=F(b[1]),z0=F(b[2]),z1=F(b[3]),st=F(b[4]),yr=F(b[5]);
  foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){
   EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();var road=UnityEngine.Object.FindAnyObjectByType<Racer.RaceDirector>().road;road.Initialize();
   var others=UnityEngine.Object.FindObjectsByType<Racer.RaceRoad>(FindObjectsInactive.Exclude,FindObjectsSortMode.None).Where(r=>r!=road&&r.points!=null&&r.points.Length>3).ToList();foreach(var o in others)o.Initialize();
   log.Add($"== {scene} rows z {z1}..{z0}, cols x {x0}..{x1} step {st}, yref {yr}");
   var names=new Dictionary<string,int>();
   for(float z=z1;z>=z0;z-=st){var sb=new System.Text.StringBuilder($"{z,5:F0} ");
    for(float x=x0;x<=x1;x+=st){var p=new Vector3(x,yr+80,z);string c="n";
     if(Physics.Raycast(p,Vector3.down,out var h,400,~0,QueryTriggerInteraction.Ignore)){int d=Mathf.RoundToInt(h.point.y-yr);
      float t=road.Project(h.point,out float lat);bool onR=Mathf.Abs(lat)<=road.HalfWidth(t)&&Mathf.Abs(road.At(t,out _).y-h.point.y)<1.5f;bool onO=false;foreach(var o in others){float u=o.Project(h.point,out float l2);if(Mathf.Abs(l2)<=o.HalfWidth(u)&&Mathf.Abs(o.At(u,out _).y-h.point.y)<1.5f)onO=true;}
      c=onR?"R":onO?"O":d>9?"+":d>=0?d.ToString():d>=-26?((char)('a'-1-d)).ToString():"#";if(!onR&&!onO){names.TryGetValue(h.collider.name,out var q);names[h.collider.name]=q+1;}}
     sb.Append(c);}log.Add(sb.ToString());}
   foreach(var kv in names.OrderByDescending(k=>k.Value).Take(25))log.Add($"  {kv.Value,5} {kv.Key}");}
  Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/map.txt",log);EditorApplication.Exit(0);}
}
public static class Report100Pair {
 static string Out=>Environment.GetEnvironmentVariable("PROBE_OUT");
 static float F(string s)=>float.Parse(s,System.Globalization.CultureInfo.InvariantCulture);
 // PROBE_RANGE="s0,s1,step" PROBE_OTHER=road name: for each race-road station, the other road's nearest point on the left: lateral, its height minus ours, its half width,
 // and the top ground every 1 m between the two verges (relative to the straight line joining the two verges: . within 0.3, digits = metres below, ^ above)
 public static void Run(){
  var log=new List<string>();var r=Environment.GetEnvironmentVariable("PROBE_RANGE").Split(',');float s0=F(r[0]),s1=F(r[1]),st=F(r[2]);string on=Environment.GetEnvironmentVariable("PROBE_OTHER");
  foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){
   EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();var road=UnityEngine.Object.FindAnyObjectByType<Racer.RaceDirector>().road;road.Initialize();
   var o=on=="self"?road:UnityEngine.Object.FindObjectsByType<Racer.RaceRoad>(FindObjectsInactive.Exclude,FindObjectsSortMode.None).First(x=>x.name==on);o.Initialize();
   log.Add($"== {scene} other '{on}' length {o.Length:F0}");
   var pts=new List<(float t,Vector3 p)>();for(float t=0;t<o.Length;t+=.5f)pts.Add((t,o.At(t,out _)));
   for(float s=s0;s<=s1;s+=st){var c=road.At(s,out var f);var right=Vector3.Cross(Vector3.up,Vector3.ProjectOnPlane(f,Vector3.up).normalized);float hw=road.HalfWidth(s);
    // march left until inside the other road
    float found=float.NaN;float oy=0,ohw=0,ot=0;for(float l=hw;l<120;l+=.5f){var p=c-right*l;float t,lat;if(on=="self"){t=-1;lat=1e9f;foreach(var q in pts){if(Mathf.Min(road.Relative(q.t,s),road.Relative(s,q.t))<150)continue;float dd=new Vector2(q.p.x-p.x,q.p.z-p.z).magnitude;if(dd<lat){lat=dd;t=q.t;}}if(t<0)continue;}else t=o.Project(p,out lat);if(Mathf.Abs(lat)<=o.HalfWidth(t)){found=l;oy=o.At(t,out _).y;ohw=o.HalfWidth(t);ot=t;break;}}
    var sb=new System.Text.StringBuilder($"s{s,5:F0} ({c.x:F0},{c.y:F1},{c.z:F0}) hw {hw:F1} ");
    if(float.IsNaN(found)){sb.Append("no other road within 120 m left");log.Add(sb.ToString());continue;}
    sb.Append($"other at lateral -{found:F1} (t {ot:F0}) dy {oy-c.y:+0.0;-0.0} ohw {ohw:F1} | ");
    for(float l=hw;l<=found;l+=1){var p=c-right*l;float a=(l-hw)/Mathf.Max(.1f,found-hw);float ly=Mathf.Lerp(c.y,oy,a);string ch="n";if(Physics.Raycast(new Vector3(p.x,ly+40,p.z),Vector3.down,out var h,200,~0,QueryTriggerInteraction.Ignore)){float d=ly-h.point.y;ch=d<-.3f?"^":d<.3f?".":d>9.5f?"#":Mathf.Max(1,Mathf.RoundToInt(d)).ToString();}sb.Append(ch);}
    log.Add(sb.ToString());}}
  Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/pair.txt",log);EditorApplication.Exit(0);}
}
public static class Report100Channel {
 static string Out=>Environment.GetEnvironmentVariable("PROBE_OUT");
 static float F(string s)=>float.Parse(s,System.Globalization.CultureInfo.InvariantCulture);
 // the original ground: every ground mesh except the 0.99 / 0.100 fills
 static float GroundAt(float x,float y,float z,out Collider col){col=null;float best=float.NaN;foreach(var h in Physics.RaycastAll(new Vector3(x,y+60,z),Vector3.down,300,~0,QueryTriggerInteraction.Ignore)){if(!(h.collider is MeshCollider)||!h.collider.name.StartsWith("Ground_")||h.collider.name.StartsWith("Ground_Report099 fill")||h.collider.name.StartsWith("Ground_Report100"))continue;if(float.IsNaN(best)||h.point.y>best){best=h.point.y;col=h.collider;}}return best;}
 // CHANNEL="s0,s1,maxLat,window": Part A. Between the race road's left verge and its own return leg on the left (any station more than `window` m away along the road),
 // the ground is raised to the plane joining the two verges (each verge = the ground just outside that road's edge, never above the road surface), for stations s0..s1.
 // Where no return leg is within maxLat, the plane is level with this road's verge out to maxLat. Only raises; existing higher ground stays. Replaces the 0.99 LeftTrench fill.
 public static void Run(){
  var log=new List<string>();var a=Environment.GetEnvironmentVariable("CHANNEL").Split(',');float s0=F(a[0]),s1=F(a[1]),maxLat=F(a[2]),win=F(a[3]),noneLat=F(a[4]);bool dry=Environment.GetEnvironmentVariable("WORLD_DRY")=="1";
  const string scene="MountainLoop";var sc=EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();
  var prev=GameObject.Find("Ground_Report099 fill LeftTrench");var prevMat=prev?prev.GetComponent<MeshRenderer>().sharedMaterial:null;log.Add("0.99 fill material "+(prevMat?prevMat.name:"none"));var road=UnityEngine.Object.FindAnyObjectByType<Racer.RaceDirector>().road;road.Initialize();
  var pts=new List<(float t,Vector3 p)>();for(float t=0;t<road.Length;t+=.5f)pts.Add((t,road.At(t,out _)));
  const float step=1f;int ns=Mathf.CeilToInt((s1-s0)/step)+1;var rows=new List<(Vector3[] p,bool[] fill)>();int cells=0;float deepest=0;var mats=new Dictionary<Material,int>();var buried=new HashSet<string>();
  var rends=UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude,FindObjectsSortMode.None).Where(r=>!r.name.StartsWith("Ground_")&&r.bounds.size.magnitude<40).ToList();
  for(int i=0;i<ns;i++){float s=s0+i*step;var c=road.At(s,out var f);var right=Vector3.Cross(Vector3.up,Vector3.ProjectOnPlane(f,Vector3.up).normalized);float hw=road.HalfWidth(s);
   // the return leg: march left from the verge
   float far=float.NaN,ofy=0;for(float l=hw;l<=maxLat;l+=.5f){var p=c-right*l;float best=1e9f,bt=-1;foreach(var q in pts){if(Mathf.Min(road.Relative(q.t,s),road.Relative(s,q.t))<win)continue;float dd=new Vector2(q.p.x-p.x,q.p.z-p.z).magnitude;if(dd<best){best=dd;bt=q.t;}}if(bt>=0&&best<=road.HalfWidth(bt)){far=l;ofy=road.At(bt,out _).y;break;}}
   if(!float.IsNaN(far)&&Mathf.Abs(ofy-c.y)>3f)far=float.NaN; // a leg at another level (the summit climb at the junction) is not the parallel road
   float near=hw+.6f;var pn=c-right*(near+.6f);float vn=GroundAt(pn.x,c.y,pn.z,out _);if(float.IsNaN(vn)||vn<c.y-.6f||vn>c.y)vn=Mathf.Min(c.y,float.IsNaN(vn)?c.y-.2f:Mathf.Max(vn,c.y-.2f));
   vn=Mathf.Min(vn,c.y-.06f); // tucked just under each road's driving surface, never over its edge
   float vf=vn,end=noneLat;if(!float.IsNaN(far)){end=far-.3f;var pf=c-right*(far-.6f);vf=GroundAt(pf.x,ofy,pf.z,out _);if(float.IsNaN(vf)||vf<ofy-.6f||vf>ofy)vf=Mathf.Min(ofy,float.IsNaN(vf)?ofy-.2f:Mathf.Max(vf,ofy-.2f));vf=Mathf.Min(vf,ofy-.06f);}
   int nl=Mathf.Max(2,Mathf.CeilToInt((end-near)/step)+1);var P=new Vector3[nl];var Fl=new bool[nl];var sb=new System.Text.StringBuilder();
   for(int j=0;j<nl;j++){float l=Mathf.Min(near+j*step,end);var p=c-right*l;float g=GroundAt(p.x,c.y,p.z,out var col);if(float.IsNaN(g))g=c.y-30;float plane=Mathf.Lerp(vn,vf,(l-near)/Mathf.Max(.1f,end-near));
    // with no return leg, fade back to the natural ground over the last 8 m
    if(float.IsNaN(far)){float w=Mathf.SmoothStep(0,1,Mathf.Clamp01((end-l)/8f));plane=Mathf.Lerp(g,Mathf.Max(g,plane),w);}
    {float ws=Mathf.SmoothStep(0,1,Mathf.Clamp01(Mathf.Min(s-s0,s1-s)/6f));plane=Mathf.Lerp(g,Mathf.Max(g,plane),ws);} // both ends meet the ground as it is
    bool fl=plane>g+.05f;P[j]=new Vector3(p.x,fl?plane:g-.05f,p.z);Fl[j]=fl;if(fl){cells++;deepest=Mathf.Max(deepest,plane-g);if(col&&j%3==0){var mr=col.GetComponent<MeshRenderer>();if(mr&&mr.sharedMaterial&&!col.name.Contains("driving surface")){mats.TryGetValue(mr.sharedMaterial,out var q);mats[mr.sharedMaterial]=q+1;}}
     if(plane-g>.3f)foreach(var r in rends){var b=r.bounds;if(b.min.y<plane-.05f&&b.max.y>g&&b.Contains(new Vector3(p.x,b.center.y,p.z)))buried.Add($"{r.name} at {r.transform.position:F1} bottom {b.min.y:F2} new ground {plane:F2}");}}
    sb.Append(!fl?".":plane-g>9.5f?"#":Mathf.Max(1,Mathf.RoundToInt(plane-g)).ToString());}
   rows.Add((P,Fl));if(i%10==0)log.Add($"s{s:F0} verge {vn-c.y:+0.00;-0.00} far {(float.IsNaN(far)?"none":$"-{far:F1} ({vf-c.y:+0.00;-0.00})")} | {sb}");}
  log.Add($"cells raised {cells}, deepest {deepest:F1} m");foreach(var bz in buried)log.Add("  object in the raised volume: "+bz);
  if(!dry){
   foreach(var g in UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(m=>m.name.StartsWith("Ground_Report100")||m.name=="Ground_Report099 fill LeftTrench").Select(m=>m.gameObject).ToList()){log.Add("removed "+g.name);UnityEngine.Object.DestroyImmediate(g);}
   // rows have different widths: triangulate each pair of rows by walking both (a ribbon between consecutive cross-sections)
   var verts=new List<Vector3>();var tris=new List<int>();var start=new List<int>();foreach(var r in rows){start.Add(verts.Count);verts.AddRange(r.p);}
   void T(int p,int q,int r){var n=Vector3.Cross(verts[q]-verts[p],verts[r]-verts[p]);if(n.y<0){int t=q;q=r;r=t;}tris.AddRange(new[]{p,q,r});}
   for(int i=0;i+1<rows.Count;i++){var A=rows[i];var B=rows[i+1];int na=A.p.Length,nb=B.p.Length,ia=0,ib=0;
    while(ia<na-1||ib<nb-1){bool advA=ib>=nb-1||(ia<na-1&&(float)(ia+1)/na<=(float)(ib+1)/nb);
     int a0=start[i]+ia,b0=start[i+1]+ib;bool any=A.fill[ia]||B.fill[ib]||(advA?A.fill[ia+1]:B.fill[ib+1]);
     if(advA){if(any)T(a0,a0+1,b0);ia++;}else{if(any)T(a0,b0+1,b0);ib++;}}}
   var mesh=new Mesh{name=$"{scene}-Report100-Channel",indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};mesh.SetVertices(verts);mesh.SetTriangles(tris,0);mesh.RecalculateNormals();mesh.RecalculateBounds();mesh.SetColors(verts.Select(_=>new Color32(89,102,77,255)).ToList());
   Directory.CreateDirectory("Assets/Scenery/Report100");AssetDatabase.CreateAsset(mesh,$"Assets/Scenery/Report100/{mesh.name}.asset");
   var go=new GameObject("Ground_Report100 fill Channel");go.AddComponent<MeshFilter>().sharedMesh=mesh;var mrr=go.AddComponent<MeshRenderer>();
   var mat=prevMat?prevMat:mats.OrderByDescending(m=>m.Value).Select(m=>m.Key).FirstOrDefault();if(mat)mrr.sharedMaterial=mat;go.AddComponent<MeshCollider>().sharedMesh=mesh;UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go,sc);
   log.Add($"mesh {mesh.name}: {verts.Count} vertices, {tris.Count/3} triangles, material {(mat?mat.name:"none")}");
   Physics.SyncTransforms();EditorSceneManager.MarkSceneDirty(sc);EditorSceneManager.SaveScene(sc);AssetDatabase.SaveAssets();}
  Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/channel.txt",log);EditorApplication.Exit(0);}
}
public static class Report100FillCheck {
 static string Out=>Environment.GetEnvironmentVariable("PROBE_OUT");
 // every vertex of the 0.100 fill that lies over the race road (any leg) and is not below the road surface; and what tops the return leg's edge there
 public static void Run(){
  var log=new List<string>();EditorSceneManager.OpenScene("Assets/Scenes/MountainLoop.unity");Physics.SyncTransforms();var road=UnityEngine.Object.FindAnyObjectByType<Racer.RaceDirector>().road;road.Initialize();
  var pts=new List<(float t,Vector3 p)>();for(float t=0;t<road.Length;t+=.5f)pts.Add((t,road.At(t,out _)));
  var mf=GameObject.Find("Ground_Report100 fill Channel").GetComponent<MeshFilter>();int n=0,over=0;
  foreach(var v in mf.sharedMesh.vertices){var w=mf.transform.TransformPoint(v);float best=1e9f,bt=0;foreach(var q in pts){float d=new Vector2(q.p.x-w.x,q.p.z-w.z).magnitude;if(d<best){best=d;bt=q.t;}}
   if(best<=road.HalfWidth(bt)+.3f){n++;float ry=road.At(bt,out _).y;if(w.y>ry-.03f){over++;if(over<40)log.Add($"vertex {w:F2} over road t {bt:F0} lateral {best:F1} road y {ry:F2} (+{w.y-ry:F2})");}}}
  log.Add($"vertices within the road outline {n}, at or above its surface {over}");
  // what is the top surface along the return leg's right edge at t 2100..2170
  for(float t=2100;t<=2170;t+=5){var c=road.At(t,out var f);var right=Vector3.Cross(Vector3.up,Vector3.ProjectOnPlane(f,Vector3.up).normalized);var sb=new System.Text.StringBuilder($"t{t:F0} y{c.y:F2}: ");
   foreach(float l in new[]{-6f,-4f,-2f,0f,2f,4f,6f,8f}){var p=c+right*l;if(Physics.Raycast(new Vector3(p.x,c.y+30,p.z),Vector3.down,out var h,80,~0,QueryTriggerInteraction.Ignore))sb.Append($"{l:+0;-0}:{h.point.y-c.y:+0.00;-0.00} {h.collider.name.Replace("Ground_","").Substring(0,Math.Min(18,h.collider.name.Length-7))} | ");}
   log.Add(sb.ToString());}
  Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/fillcheck.txt",log);EditorApplication.Exit(0);}
}
public static class Report100Drain {
 static string Out=>Environment.GetEnvironmentVariable("PROBE_OUT");
 // every object whose renderer or collider touches the box x 120..205, z 35..110 in FreeRoamWorld: renderer bounds, collider type / bounds / enabled / trigger
 public static void List(){
  var log=new List<string>();EditorSceneManager.OpenScene("Assets/Scenes/FreeRoamWorld.unity");Physics.SyncTransforms();
  var box=new Bounds(new Vector3(162,60,72),new Vector3(85,60,75));
  foreach(var go in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).Select(t=>t.gameObject)){
   var r=go.GetComponent<Renderer>();var cs=go.GetComponents<Collider>();if(!r&&cs.Length==0)continue;
   bool hit=(r&&r.bounds.Intersects(box))||cs.Any(c=>c.bounds.Intersects(box));if(!hit)continue;
   if(r&&r.bounds.size.x>400&&cs.Length==0)continue;
   string path=go.name;var p=go.transform.parent;int k=0;while(p&&k<3){path=p.name+"/"+path;p=p.parent;k++;}
   log.Add($"{path} | active {go.activeInHierarchy} | rend {(r?$"{r.bounds.min:F1}..{r.bounds.max:F1} {(r is MeshRenderer mr&&mr.sharedMaterial?mr.sharedMaterial.name:"")}":"none")} | cols {string.Join(", ",cs.Select(c=>$"{c.GetType().Name}{(c.enabled?"":" OFF")}{(c.isTrigger?" TRIG":"")} {c.bounds.min:F1}..{c.bounds.max:F1}"))}");}
  Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/drain-objects.txt",log.OrderBy(x=>x));EditorApplication.Exit(0);}
}
public static class Report100DrainRays {
 static string Out=>Environment.GetEnvironmentVariable("PROBE_OUT");
 // PROBE_POINTS="x,z|...": every collider surface a vertical ray from y 120 passes (back faces included), with the face normal's y (up-facing > 0)
 public static void Run(){
  var log=new List<string>();EditorSceneManager.OpenScene("Assets/Scenes/"+Environment.GetEnvironmentVariable("PROBE_SCENES")+".unity");Physics.SyncTransforms();var inv=System.Globalization.CultureInfo.InvariantCulture;
  bool old=Physics.queriesHitBackfaces;Physics.queriesHitBackfaces=true;
  foreach(var pt in Environment.GetEnvironmentVariable("PROBE_POINTS").Split('|')){var a=pt.Split(',');float x=float.Parse(a[0],inv),z=float.Parse(a[1],inv);
   var hits=Physics.RaycastAll(new Vector3(x,120,z),Vector3.down,200,~0,QueryTriggerInteraction.Ignore).OrderBy(h=>h.distance);
   log.Add($"({x},{z}): "+string.Join(" | ",hits.Select(h=>$"{h.point.y:F2} {h.collider.name} n{h.normal.y:+0.00;-0.00}")));}
  Physics.queriesHitBackfaces=old;Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/rays.txt",log);EditorApplication.Exit(0);}
}
public static class Report100Roof {
 static Vector3 FaceOf(Vector3 ce,List<Vector3> v,List<int> t,int w0){for(int i=w0;i<t.Count;i+=3){var c=(v[t[i]]+v[t[i+1]]+v[t[i+2]])/3;if((c-ce).sqrMagnitude<1e-6f)return Vector3.Cross(v[t[i+1]]-v[t[i]],v[t[i+2]]-v[t[i]]);}return Vector3.zero;}
 static string Out=>Environment.GetEnvironmentVariable("PROBE_OUT");
 // Part F: the exposed culvert ceiling has only its inside (down-facing) face, so a vehicle on top drops through. A roof: the ceiling's
 // down-facing triangles in the exposed stretch (x 134..170, z 36..80; skipped where the ground is well above them), turned to face up and
 // lifted 4 cm, drawn in the culvert mouth's concrete and collidable. Nothing inside the tunnel changes. WORLD_DRY=1 only reports.
 public static void Run(){
  var log=new List<string>();bool dry=Environment.GetEnvironmentVariable("WORLD_DRY")=="1";
  var sc=EditorSceneManager.OpenScene("Assets/Scenes/FreeRoamWorld.unity");Physics.SyncTransforms();
  foreach(var g in UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(m=>m.name=="Culvert roof top (0.100)").Select(m=>m.gameObject).ToList())UnityEngine.Object.DestroyImmediate(g);
  var shell=GameObject.Find("Long storm culvert walls and ceiling");var mf=shell.GetComponent<MeshFilter>();var m=mf.sharedMesh;var tr=shell.transform;
  var lintel=GameObject.Find("Culvert mouth lintel");var mat=lintel.GetComponent<MeshRenderer>().sharedMaterial;
  var wv=m.vertices.Select(v=>tr.TransformPoint(v)).ToArray();var t=m.triangles;var verts=new List<Vector3>();var tris=new List<int>();int kept=0,buried=0;
  float GroundAbove(Vector3 p){float best=float.NaN;foreach(var h in Physics.RaycastAll(new Vector3(p.x,p.y+40,p.z),Vector3.down,60,~0,QueryTriggerInteraction.Ignore)){if(!h.collider.name.StartsWith("Ground_")||h.collider.name.StartsWith("Ground_Culvert"))continue;if(float.IsNaN(best)||h.point.y>best)best=h.point.y;}return best;}
  for(int i=0;i<t.Length;i+=3){var a=wv[t[i]];var b=wv[t[i+1]];var c=wv[t[i+2]];var n=Vector3.Cross(b-a,c-a).normalized;if(n.y>-.3f)continue;var ce=(a+b+c)/3;
   if(ce.x<134||ce.x>170||ce.z<36||ce.z>80)continue;float g=GroundAbove(ce);if(!float.IsNaN(g)&&g>ce.y+1f){buried++;continue;}
   int k=verts.Count;var up=Vector3.up*.04f;verts.Add(a+up);verts.Add(c+up);verts.Add(b+up);tris.Add(k);tris.Add(k+1);tris.Add(k+2);kept++;}
  // a skirt from every open edge of the roof (not at the mouth end, x > 166.5): 1 m out, 0.2 m down, so the ground meets the concrete with no sliver
  var key=new Func<Vector3,(int,int,int)>(v=>(Mathf.RoundToInt(v.x*200),Mathf.RoundToInt(v.y*200),Mathf.RoundToInt(v.z*200)));var edges=new Dictionary<((int,int,int),(int,int,int)),(Vector3 a,Vector3 b,Vector3 ce,int n)>();
  for(int i=0;i<tris.Count;i+=3){var ce=(verts[tris[i]]+verts[tris[i+1]]+verts[tris[i+2]])/3;for(int e=0;e<3;e++){var a=verts[tris[i+e]];var b=verts[tris[i+(e+1)%3]];var ka=key(a);var kb=key(b);var k2=ka.CompareTo(kb)<0?(ka,kb):(kb,ka);if(edges.TryGetValue(k2,out var v))edges[k2]=(v.a,v.b,v.ce,v.n+1);else edges[k2]=(a,b,ce,1);}}
  int skirts=0;foreach(var e in edges.Values){if(e.n!=1)continue;var mid=(e.a+e.b)/2;if(mid.x>166.5f)continue;var along=e.b-e.a;along.y=0;if(along.sqrMagnitude<1e-6f)continue;var outw=Vector3.Cross(Vector3.up,along.normalized);if(Vector3.Dot(outw,mid-e.ce)<0)outw=-outw;var o=outw*1f+Vector3.down*.2f;
   int k=verts.Count;verts.Add(e.a);verts.Add(e.b);verts.Add(e.b+o);verts.Add(e.a+o);
   void T(int x,int y,int z){var n=Vector3.Cross(verts[y]-verts[x],verts[z]-verts[x]);if(n.y<0){int q=y;y=z;z=q;}tris.Add(x);tris.Add(y);tris.Add(z);}T(k,k+1,k+2);T(k,k+2,k+3);skirts++;}
  log.Add($"skirt quads on open edges: {skirts}");
  // the side walls where they stand above the ground outside (the north-west side is up to 5 m out of the ground): their outside faces too,
  // so a vehicle that meets the wall stops against it instead of passing through into the tunnel
  int walls=0;float GroundOnly(Vector3 p){float best=float.NaN;foreach(var h in Physics.RaycastAll(new Vector3(p.x,p.y+40,p.z),Vector3.down,80,~0,QueryTriggerInteraction.Ignore)){if(!h.collider.name.StartsWith("Ground_")||h.collider.name.StartsWith("Ground_Culvert"))continue;if(float.IsNaN(best)||h.point.y>best)best=h.point.y;}return best;}
  for(int i=0;i<t.Length;i+=3){var a=wv[t[i]];var b=wv[t[i+1]];var c=wv[t[i+2]];var n=Vector3.Cross(b-a,c-a).normalized;if(n.y<=-.3f||n.y>=.3f)continue;var ce=(a+b+c)/3;
   if(ce.x<134||ce.x>170||ce.z<36||ce.z>80)continue;var outw=-n;outw.y=0;if(outw.sqrMagnitude<1e-4f)continue;outw.Normalize();float top=Mathf.Max(a.y,Mathf.Max(b.y,c.y));
   float g=GroundOnly(ce+outw*.6f);if(!float.IsNaN(g)&&g>top-.05f)continue; // buried: the ground covers it
   int k=verts.Count;var off=outw*.04f;verts.Add(a+off);verts.Add(c+off);verts.Add(b+off);tris.Add(k);tris.Add(k+1);tris.Add(k+2);walls++;}
  log.Add($"wall triangles given an outside face: {walls}");
  // their lowest open edges carried 1.2 m straight down, so the ground meets the concrete everywhere along the foot of the wall
  {int w0=tris.Count-walls*3;var we=new Dictionary<((int,int,int),(int,int,int)),(Vector3 a,Vector3 b,Vector3 ce,int n)>();
   for(int i=w0;i<tris.Count;i+=3){var ce=(verts[tris[i]]+verts[tris[i+1]]+verts[tris[i+2]])/3;for(int e=0;e<3;e++){var a=verts[tris[i+e]];var b=verts[tris[i+(e+1)%3]];var ka=key(a);var kb=key(b);var k2=ka.CompareTo(kb)<0?(ka,kb):(kb,ka);if(we.TryGetValue(k2,out var v))we[k2]=(v.a,v.b,v.ce,v.n+1);else we[k2]=(a,b,ce,1);}}
   int feet=0;foreach(var e in we.Values){if(e.n!=1)continue;var mid=(e.a+e.b)/2;if(mid.y>e.ce.y-.05f||Mathf.Abs(e.a.y-e.b.y)>1.5f)continue;
    int k=verts.Count;var dn=Vector3.down*1.2f;verts.Add(e.a);verts.Add(e.b);verts.Add(e.b+dn);verts.Add(e.a+dn);
    var face=Vector3.Cross(verts[k+1]-verts[k],verts[k+2]-verts[k]);var outw=e.ce-mid;outw.y=0; // the triangle's outward side: its centroid lies above the foot, so use the wall's own facing
    var tn=Vector3.Cross(verts[tris[w0+1]]-verts[tris[w0]],verts[tris[w0+2]]-verts[tris[w0]]);
    if(Vector3.Dot(face,FaceOf(e.ce,verts,tris,w0))<0){tris.Add(k);tris.Add(k+2);tris.Add(k+1);tris.Add(k);tris.Add(k+3);tris.Add(k+2);}else{tris.Add(k);tris.Add(k+1);tris.Add(k+2);tris.Add(k);tris.Add(k+2);tris.Add(k+3);}feet++;}
   log.Add($"wall feet carried down: {feet}");}
  var bounds=new Bounds(verts[0],Vector3.zero);foreach(var v in verts)bounds.Encapsulate(v);
  log.Add($"ceiling triangles turned up: {kept} (skipped under the ground: {buried}); roof bounds {bounds.min:F2}..{bounds.max:F2}; material {mat.name}");
  if(!dry){var mesh=new Mesh{name="FreeRoamWorld-Report100-CulvertRoof",indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};mesh.SetVertices(verts);mesh.SetTriangles(tris,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
   // planar UVs so the concrete texture tiles like on the mouth
   mesh.SetUVs(0,verts.Select(v=>new Vector2(v.x*.25f,v.z*.25f)).ToList());
   Directory.CreateDirectory("Assets/Scenery/Report100");AssetDatabase.CreateAsset(mesh,"Assets/Scenery/Report100/"+mesh.name+".asset");
   var go=new GameObject("Culvert roof top (0.100)");go.transform.SetParent(shell.transform.parent,true);go.AddComponent<MeshFilter>().sharedMesh=mesh;var r=go.AddComponent<MeshRenderer>();r.sharedMaterial=mat;go.AddComponent<MeshCollider>().sharedMesh=mesh;go.layer=shell.layer;
   EditorSceneManager.MarkSceneDirty(sc);EditorSceneManager.SaveScene(sc);AssetDatabase.SaveAssets();log.Add("saved");}
  Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/roof.txt",log);EditorApplication.Exit(0);}
}
public static class Report100Gaps {
 static string Out=>Environment.GetEnvironmentVariable("PROBE_OUT");
 // top surface over x 132..182, z 34..84 every 0.5 m: R roof, g ground, F culvert floor (open from above), c other concrete, . other, n nothing
 public static void Run(){
  var log=new List<string>();EditorSceneManager.OpenScene("Assets/Scenes/FreeRoamWorld.unity");Physics.SyncTransforms();var fl=new List<string>();
  for(float z=84;z>=34;z-=.5f){var sb=new System.Text.StringBuilder($"{z,5:F1} ");for(float x=132;x<=182;x+=.5f){string c="n";if(Physics.Raycast(new Vector3(x,120,z),Vector3.down,out var h,200,~0,QueryTriggerInteraction.Ignore)){var nm=h.collider.name;c=nm.StartsWith("Culvert roof")?"R":nm.StartsWith("Ground_Culvert")?"F":nm.StartsWith("Ground_")?"g":nm.Contains("Culvert")||nm.Contains("culvert")?"c":".";if(c=="F")fl.Add($"{x:F1},{z:F1} y{h.point.y:F2}");}sb.Append(c);}log.Add(sb.ToString());}
  log.Add("open-floor cells: "+fl.Count);Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/gaps.txt",log);EditorApplication.Exit(0);}
}
public static class Report100Compile { public static void Done(){ EditorApplication.Exit(0); } }
