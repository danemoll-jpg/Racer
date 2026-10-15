using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 0.117 (read-only, temporary; scenes never saved). PROBE_SCENES "MountainLoop;MountainLoopReverse".
//  Lines(): every RaceRoad / WoodlandRoute in each scene (active or not) resampled every 3 m -> lines-<scene>.json; the ground under each
//           Leviathan gate (the course data of that scene; Forward's gully fill added as in the race) -> gates-<scene>.txt
//  Views(): PROBE_VIEWS "name|scene|x|y|z|heading;..." -> view-<name>.jpg (driver's eye, 1600x900) and top-<name>.jpg (180 m square, north up,
//           Leviathan's line cyan, other race lines red, trails orange, the point white)
public static class Report117Probe {
 static string V(Vector3 v)=>FormattableString.Invariant($"({v.x:F1},{v.y:F1},{v.z:F1})");
 static string P(Transform t){var s=t.name;while(t.parent){t=t.parent;s=t.name+"/"+s;}return s;}
 static Racer.LeviathanCourse.Data Data(string scene)=>JsonUtility.FromJson<Racer.LeviathanCourse.Data>(File.ReadAllText("Assets/Resources/Leviathan/"+(scene=="MountainLoop"?"LeviathanForward":"LeviathanReverse")+".json"));
 static void Open(string scene){
  EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");
  if(scene=="MountainLoop"){var mesh=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Resources/Leviathan/LeviathanForwardGullyFill.asset");if(mesh){var g=new GameObject("Ground_fill (probe)");g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).FirstOrDefault(r=>r.name=="Ground_640_240")?.sharedMaterial;g.AddComponent<MeshCollider>().sharedMesh=mesh;}}
  Physics.SyncTransforms();}
 static List<Vector3> Resample(Vector3[] p,float step){var o=new List<Vector3>();if(p==null||p.Length==0)return o;o.Add(p[0]);float acc=0;for(int i=1;i<p.Length;i++){float d=Vector3.Distance(p[i-1],p[i]);float t=step-acc;while(t<=d){o.Add(Vector3.Lerp(p[i-1],p[i],t/d));t+=step;}acc=d-(t-step);}o.Add(p[^1]);return o;}
 public static void Lines(){
  var o=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(o);
  foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(';')){
   var log=new List<string>();
   try{
    Open(scene);var sb=new System.Text.StringBuilder("[");bool first=true;
    void Add(string kind,Transform t,bool active,Vector3[] pts,float hw){if(pts==null||pts.Length<2)return;var r=Resample(pts,3);if(!first)sb.Append(",");first=false;
     sb.Append("{\"kind\":\""+kind+"\",\"path\":\""+P(t).Replace("\"","'")+"\",\"active\":"+(active?"true":"false")+FormattableString.Invariant($",\"hw\":{hw:F2}")+",\"pts\":[");sb.Append(string.Join(",",r.Select(v=>FormattableString.Invariant($"{v.x:F1},{v.y:F1},{v.z:F1}"))));sb.Append("]}");}
    foreach(var rr in UnityEngine.Object.FindObjectsByType<Racer.RaceRoad>(FindObjectsInactive.Include,FindObjectsSortMode.None))Add("road",rr.transform,rr.gameObject.activeInHierarchy,rr.points,0);
    foreach(var w in UnityEngine.Object.FindObjectsByType<Racer.WoodlandRoute>(FindObjectsInactive.Include,FindObjectsSortMode.None)){Add("trail",w.transform,w.gameObject.activeInHierarchy,w.points,w.halfWidth);}
    sb.Append("]");File.WriteAllText(Path.Combine(o,"lines-"+scene+".json"),sb.ToString());
    var d=Data(scene);
    for(int i=0;i<d.gates.Length;i++){var g=d.gates[i];var p=new Vector3(g.pos[0],g.pos[1],g.pos[2]);
     var hits=Physics.RaycastAll(p+Vector3.up*30,Vector3.down,60,~0,QueryTriggerInteraction.Ignore).Where(h=>!h.collider.attachedRigidbody&&h.normal.y>.4f&&h.collider.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)<0).OrderBy(h=>h.distance).ToArray();
     string reuse="";if(!string.IsNullOrEmpty(g.reuse)){var t=UnityEngine.Object.FindObjectsByType<Racer.RaceGate>(FindObjectsInactive.Include,FindObjectsSortMode.None).FirstOrDefault(x=>P(x.transform).EndsWith(g.reuse));if(t){var rs=t.GetComponentsInChildren<Renderer>(true);if(rs.Length>0){var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);reuse=FormattableString.Invariant($" reused: transform {V(t.transform.position)} renderers y {b.min.y:F1}..{b.max.y:F1} width {b.size.x:F1}x{b.size.z:F1}");}}}
     log.Add(FormattableString.Invariant($"{i,2} {g.name,-44} pos {V(p)} hw {g.halfWidth} hh {g.halfHeight} up {g.upperHeight} | top hits: {string.Join("; ",hits.Take(3).Select(h=>FormattableString.Invariant($"{h.point.y:F2} {h.collider.name}")))}{reuse}"));}
   }catch(Exception e){log.Add("ERROR "+e);}
   File.WriteAllLines(Path.Combine(o,"gates-"+scene+".txt"),log);}
  EditorApplication.Exit(0);}
 static Texture2D Render(Vector3 at,Quaternion rot,bool ortho,float size,int w,int h,out Camera keep){
  var go=new GameObject("probe cam");var cam=go.AddComponent<Camera>();cam.orthographic=ortho;if(ortho)cam.orthographicSize=size;else cam.fieldOfView=62;cam.nearClipPlane=ortho?1:.3f;cam.farClipPlane=3000;
  cam.clearFlags=CameraClearFlags.Skybox;go.transform.SetPositionAndRotation(at,rot);var rt=new RenderTexture(w,h,24);cam.targetTexture=rt;cam.aspect=(float)w/h;cam.Render();
  var prev=RenderTexture.active;RenderTexture.active=rt;var tex=new Texture2D(w,h,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,w,h),0,0);tex.Apply();RenderTexture.active=prev;cam.targetTexture=null;rt.Release();keep=cam;return tex;}
 static Vector3 C0;static float Half;static void Dot(Texture2D t,Camera cam,Vector3 p,Color c,int r){int x=(int)((p.x-C0.x)/Half*t.width*.5f+t.width*.5f),y=(int)((p.z-C0.z)/Half*t.height*.5f+t.height*.5f);for(int dx=-r;dx<=r;dx++)for(int dy=-r;dy<=r;dy++)if(x+dx>=0&&x+dx<t.width&&y+dy>=0&&y+dy<t.height)t.SetPixel(x+dx,y+dy,c);}
 public static void Views(){
  var o=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(o);var log=new List<string>();var inv=System.Globalization.CultureInfo.InvariantCulture;
  try{
   string open=null;
   foreach(var spec in Environment.GetEnvironmentVariable("PROBE_VIEWS").Split(';',StringSplitOptions.RemoveEmptyEntries)){
    var f=spec.Split('|');string name=f[0],scene=f[1];var p=new Vector3(float.Parse(f[2],inv),float.Parse(f[3],inv),float.Parse(f[4],inv));float hd=float.Parse(f[5],inv);float half=f.Length>6?float.Parse(f[6],inv):90;
    if(open!=scene){Open(scene);open=scene;}
    var fwd=Quaternion.Euler(0,hd,0)*Vector3.forward;
    var tex=Render(p+Vector3.up*1.7f-fwd*1.5f,Quaternion.LookRotation(fwd+Vector3.down*.06f),false,0,1600,900,out var c1);UnityEngine.Object.DestroyImmediate(c1.gameObject);File.WriteAllBytes(Path.Combine(o,"view-"+name+".jpg"),tex.EncodeToJPG(85));
    C0=p;Half=half;var top=Render(new Vector3(p.x,p.y+400,p.z),Quaternion.Euler(90,0,0),true,half,1200,1200,out var cam);
    var pal=new[]{Color.red,new Color(1,.55f,0),Color.yellow,Color.green,new Color(.3f,.5f,1),new Color(1,.4f,.8f),new Color(.6f,1,.6f),new Color(.7f,.3f,0),new Color(.5f,0,.5f),Color.black,new Color(1,1,.6f),new Color(0,.5f,.3f)};int ci=0;var legend=new List<string>();
    foreach(var rr in UnityEngine.Object.FindObjectsByType<Racer.RaceRoad>(FindObjectsInactive.Include,FindObjectsSortMode.None).Cast<Component>().Concat(UnityEngine.Object.FindObjectsByType<Racer.WoodlandRoute>(FindObjectsInactive.Include,FindObjectsSortMode.None)).OrderBy(x=>P(x.transform))){
     var pts=rr is Racer.RaceRoad ra?ra.points:((Racer.WoodlandRoute)rr).points;var rs=Resample(pts,2).Where(q=>Mathf.Abs(q.x-p.x)<half&&Mathf.Abs(q.z-p.z)<half).ToList();if(rs.Count==0)continue;var col=pal[ci++%pal.Length];
     foreach(var q in rs)Dot(top,cam,q,col,1);legend.Add($"   {col} {(rr.gameObject.activeInHierarchy?"":"(inactive) ")}{P(rr.transform)}");}
    log.AddRange(legend);
    var d=Data(scene);var lp=new Vector3[d.pts.Length/3];for(int i=0;i<lp.Length;i++)lp[i]=new Vector3(d.pts[3*i],d.pts[3*i+1],d.pts[3*i+2]);
    foreach(var q in Resample(lp,1.5f))if(Mathf.Abs(q.x-p.x)<half&&Mathf.Abs(q.z-p.z)<half)Dot(top,cam,q,Color.cyan,1);
    foreach(var g in d.gates){var gp=new Vector3(g.pos[0],g.pos[1],g.pos[2]);var r=Quaternion.Euler(0,g.rot[1],0)*Vector3.right;for(float k=-g.halfWidth;k<=g.halfWidth;k+=.5f)Dot(top,cam,gp+r*k,Color.magenta,1);}
    if(d.blocks!=null)foreach(var bl in d.blocks){var bp=new Vector3(bl.pos[0],0,bl.pos[1]);var br=Quaternion.Euler(0,bl.yaw,0)*Vector3.right;var bf=Quaternion.Euler(0,bl.yaw,0)*Vector3.forward;float hw=bl.half>0?bl.half:3;for(float k=-hw;k<=hw;k+=.25f){Dot(top,cam,bp+br*k,Color.white,1);}for(float k=0;k<=4;k+=.25f)Dot(top,cam,bp-bf*k,Color.red,1);}
    if(d.signs!=null)foreach(var sg in d.signs)Dot(top,cam,new Vector3(sg.pos[0],0,sg.pos[1]),sg.kind=="arrow"?Color.yellow:sg.kind=="board"?new Color(1,.6f,0):new Color(1,1,.5f),2);
    Dot(top,cam,p,Color.white,4);Dot(top,cam,p+fwd*6,Color.white,2);Dot(top,cam,p+fwd*10,Color.white,2);
    top.Apply();UnityEngine.Object.DestroyImmediate(cam.gameObject);File.WriteAllBytes(Path.Combine(o,"top-"+name+".jpg"),top.EncodeToJPG(85));
    log.Insert(log.Count-legend.Count,$"{name} {scene} {V(p)} heading {hd}");}
  }catch(Exception e){log.Add("ERROR "+e);}
  File.WriteAllLines(Path.Combine(o,"views.txt"),log);EditorApplication.Exit(0);}
 // PROBE_NEAR "scene|x|z|r;...": every object with a renderer or collider within r (XZ) -> near.txt (path, active, position, bounds size, collider)
 public static void Near(){
  var o=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(o);var log=new List<string>();var inv=System.Globalization.CultureInfo.InvariantCulture;string open=null;
  try{foreach(var spec in Environment.GetEnvironmentVariable("PROBE_NEAR").Split(';',StringSplitOptions.RemoveEmptyEntries)){
   var f=spec.Split('|');if(open!=f[0]){Open(f[0]);open=f[0];}float x=float.Parse(f[1],inv),z=float.Parse(f[2],inv),r=float.Parse(f[3],inv);log.Add($"== {spec}");
   foreach(var t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None)){
    var rd=t.GetComponent<Renderer>();var c=t.GetComponent<Collider>();if(!rd&&!c)continue;var b=rd?rd.bounds:c.bounds;if(b.size.x>120||b.size.z>120)continue;
    if(Vector2.Distance(new(b.center.x,b.center.z),new(x,z))>r)continue;
    log.Add(FormattableString.Invariant($"   {(t.gameObject.activeInHierarchy?"A":"-")} {Vector2.Distance(new(b.center.x,b.center.z),new(x,z)),5:F1} {V(b.center)} size {V(b.size)} {(c?c.GetType().Name+(c.isTrigger?"(trigger)":""):"")} {P(t)}"));}
  }}catch(Exception e){log.Add("ERROR "+e);}
  File.WriteAllLines(Path.Combine(o,"near.txt"),log);EditorApplication.Exit(0);}
 // PROBE_SCENES + PROBE_REGEX: every object whose name matches (top of each match only) -> named.txt (path, active, bounds, colliders, texts)
 public static void Named(){
  var o=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(o);var log=new List<string>();var rx=new System.Text.RegularExpressions.Regex(Environment.GetEnvironmentVariable("PROBE_REGEX"),System.Text.RegularExpressions.RegexOptions.IgnoreCase);
  try{foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(';')){Open(scene);log.Add("== "+scene);
   foreach(var t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).OrderBy(x=>P(x))){
    if(!rx.IsMatch(t.name))continue;if(t.parent&&rx.IsMatch(t.parent.name))continue;
    var rs=t.GetComponentsInChildren<Renderer>(true);var b=rs.Length>0?rs[0].bounds:new Bounds(t.position,Vector3.zero);foreach(var r in rs)b.Encapsulate(r.bounds);
    var cols=t.GetComponentsInChildren<Collider>(true).Where(c=>!c.isTrigger).Count();var texts=t.GetComponentsInChildren<TextMesh>(true).Select(x=>x.text.Replace('\n','/'));
    log.Add(FormattableString.Invariant($"   {(t.gameObject.activeInHierarchy?"A":"-")} {V(t.position)} yaw {t.eulerAngles.y:F0} bounds {V(b.center)} size {V(b.size)} solid colliders {cols} children {t.childCount} [{string.Join(" | ",texts)}] {P(t)}"));}}
  }catch(Exception e){log.Add("ERROR "+e);}
  File.WriteAllLines(Path.Combine(o,"named.txt"),log);EditorApplication.Exit(0);}
 // PROBE_DUMP "scene|exact object name;...": the object's hierarchy: local transforms, components, materials, collider sizes, texts -> dump.txt
 public static void Dump(){
  var o=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(o);var log=new List<string>();string open=null;
  void Rec(Transform t,int depth){var pad=new string(' ',2+depth*2);var comps=t.GetComponents<Component>().Where(c=>c&&!(c is Transform)).Select(c=>{var n=c.GetType().Name;if(c is Renderer r)n+="["+string.Join(",",r.sharedMaterials.Select(m=>m?m.name+"/"+m.shader.name:"null"))+"]";if(c is BoxCollider bc)n+=FormattableString.Invariant($"(c {V(bc.center)} s {V(bc.size)}{(bc.isTrigger?" trigger":"")})");if(c is MeshFilter mf)n+="("+(mf.sharedMesh?mf.sharedMesh.name:"null")+")";if(c is TextMesh tm)n+=FormattableString.Invariant($"('{tm.text.Replace((char)10,'/')}' size {tm.characterSize} font {tm.fontSize} {tm.font?.name})");if(c is MonoBehaviour mb)n+="("+mb.enabled+")";return n;});
   log.Add(FormattableString.Invariant($"{pad}{(t.gameObject.activeSelf?"":"(off) ")}{t.name} lp {V(t.localPosition)} le {V(t.localEulerAngles)} ls {V(t.localScale)} world {V(t.position)} : {string.Join("; ",comps)}"));
   foreach(Transform c in t)Rec(c,depth+1);}
  try{foreach(var spec in Environment.GetEnvironmentVariable("PROBE_DUMP").Split(';',StringSplitOptions.RemoveEmptyEntries)){var f=spec.Split('|');if(open!=f[0]){Open(f[0]);open=f[0];}
   foreach(var t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(x=>x.name==f[1])){log.Add("== "+f[0]+" "+P(t));Rec(t,0);}}
  }catch(Exception e){log.Add("ERROR "+e);}
  File.WriteAllLines(Path.Combine(o,"dump.txt"),log);EditorApplication.Exit(0);}
}
