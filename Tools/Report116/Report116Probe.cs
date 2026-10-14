using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 0.116 (read-only, temporary; scenes never saved): Part B's first question. Granite Creek Cut and Laurel Switchbacks (their routes in
// StreetLoopReverse.unity): the ground under them in StreetLoopReverse and in MountainLoopReverse (every 10 m), what stands on them
// (solids at wheel height), every sign / shortcut marking / lettering near them in MountainLoopReverse, a top-down and a driver's-view
// render of each in both scenes. Output in PROBE_OUT.
public static class Report116Probe {
 static string P(Transform t){var s=t.name;while(t.parent){t=t.parent;s=t.name+"/"+s;}return s;}
 static string V(Vector3 v)=>FormattableString.Invariant($"({v.x:F1},{v.y:F1},{v.z:F1})");
 static RaycastHit[] Down(Vector3 p,float above=60,float depth=160)=>Physics.RaycastAll(new Vector3(p.x,p.y+above,p.z),Vector3.down,depth,~0,QueryTriggerInteraction.Ignore).Where(h=>h.normal.y>.3f&&!h.collider.attachedRigidbody).OrderBy(h=>h.distance).ToArray();
 static void Render(string file,Vector3 at,Quaternion rot,bool ortho,float size,int w,int h){
  var go=new GameObject("probe cam");var cam=go.AddComponent<Camera>();cam.orthographic=ortho;if(ortho)cam.orthographicSize=size;else cam.fieldOfView=60;cam.nearClipPlane=ortho?1:.3f;cam.farClipPlane=3000;
  cam.clearFlags=CameraClearFlags.Skybox;go.transform.SetPositionAndRotation(at,rot);var rt=new RenderTexture(w,h,24);cam.targetTexture=rt;cam.aspect=(float)w/h;cam.Render();
  var prev=RenderTexture.active;RenderTexture.active=rt;var tex=new Texture2D(w,h,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,w,h),0,0);tex.Apply();RenderTexture.active=prev;
  File.WriteAllBytes(file,tex.EncodeToJPG(88));cam.targetTexture=null;rt.Release();UnityEngine.Object.DestroyImmediate(go);}
 public static void Shortcuts(){
  var o=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(o);var log=new List<string>();
  try{
   EditorSceneManager.OpenScene("Assets/Scenes/StreetLoopReverse.unity");Physics.SyncTransforms();
   var routes=UnityEngine.Object.FindObjectsByType<Racer.WoodlandRoute>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(r=>r.title=="Granite Creek Cut"||r.title=="Laurel Switchbacks").OrderBy(r=>r.title).ToArray();
   var lines=routes.Select(r=>(r.title,pts:(Vector3[])r.points.Clone(),ai:r.HasAiLine?(Vector3[])r.aiPoints.Clone():null,r.halfWidth)).ToArray();
   var samples=new Dictionary<string,List<(Vector3 p,float sl)>>();
   foreach(var l in lines){var list=new List<(Vector3,float)>();float s=0,next=0;for(int i=0;i<l.pts.Length;i++){if(i>0)s+=Vector2.Distance(new(l.pts[i].x,l.pts[i].z),new(l.pts[i-1].x,l.pts[i-1].z));if(s>=next||i==l.pts.Length-1){list.Add((l.pts[i],s));next=s+10;}}samples[l.title]=list;}
   var groundSL=new Dictionary<string,float[]>();
   foreach(var l in lines){var g=samples[l.title].Select(x=>{var h=Down(x.p,8,20);return h.Length>0?h[0].point.y:float.NaN;}).ToArray();groundSL[l.title]=g;}
   foreach(var l in lines){var a=l.pts[0];var f=(l.pts[Math.Min(8,l.pts.Length-1)]-a);f.y=0;f.Normalize();
    Render(Path.Combine(o,$"view-SLR-{l.title.Split(' ')[0]}-entry.jpg"),a-f*25+Vector3.up*2.2f,Quaternion.LookRotation(f*30+Vector3.down*2),false,0,1280,720);
    var b=new Bounds(l.pts[0],Vector3.zero);foreach(var p in l.pts)b.Encapsulate(p);Render(Path.Combine(o,$"top-SLR-{l.title.Split(' ')[0]}.jpg"),new Vector3(b.center.x,1500,b.center.z),Quaternion.Euler(90,0,0),true,Mathf.Max(b.extents.x,b.extents.z)+40,1400,1400);}
   EditorSceneManager.OpenScene("Assets/Scenes/MountainLoopReverse.unity");Physics.SyncTransforms();
   foreach(var l in lines){
    log.Add($"== {l.title}: {l.pts.Length} points, {samples[l.title].Last().sl:F0} m, half width {l.halfWidth}{(l.ai!=null?$", rivals' line {l.ai.Length} points":"")}");
    log.Add("   s     x       z      route y  ground SLR  ground MLR (top 3 hits: y name)   diff   solids at wheel height in MLR");
    var sm=samples[l.title];int big=0;float maxd=0;
    for(int k=0;k<sm.Count;k++){var (p,s)=sm[k];var hits=Down(p,60,160);float mlr=hits.Length>0?hits.OrderBy(h=>Mathf.Abs(h.point.y-p.y)).First().point.y:float.NaN;float d=mlr-groundSL[l.title][k];if(!float.IsNaN(d)){maxd=Mathf.Max(maxd,Mathf.Abs(d));if(Mathf.Abs(d)>1)big++;}
     var top=hits.Length>0?hits[0].point.y:p.y;var solids=Physics.OverlapCapsule(new Vector3(p.x,top+.7f,p.z),new Vector3(p.x,top+1.6f,p.z),l.halfWidth*.6f,~0,QueryTriggerInteraction.Ignore).Where(c=>!c.attachedRigidbody).Select(c=>c.name).Distinct().Take(4);
     log.Add(FormattableString.Invariant($"{s,6:F0} {p.x,7:F1} {p.z,7:F1} {p.y,8:F1} {groundSL[l.title][k],10:F1} {mlr,10:F1}  [{string.Join("; ",hits.Take(3).Select(h=>FormattableString.Invariant($"{h.point.y:F1} {h.collider.name}")))}] {d,6:F1}   {string.Join(", ",solids)}"));}
    log.Add(FormattableString.Invariant($"   samples {sm.Count}, ground differs >1 m at {big}, max difference {maxd:F1} m"));
    var a=l.pts[0];var f=(l.pts[Math.Min(8,l.pts.Length-1)]-a);f.y=0;f.Normalize();var ah=Down(a,60,160);float ay=ah.Length>0?ah.OrderBy(h=>Mathf.Abs(h.point.y-a.y)).First().point.y:a.y;
    Render(Path.Combine(o,$"view-MLR-{l.title.Split(' ')[0]}-entry.jpg"),new Vector3(a.x,ay,a.z)-f*25+Vector3.up*2.2f,Quaternion.LookRotation(f*30+Vector3.down*2),false,0,1280,720);
    var b=new Bounds(l.pts[0],Vector3.zero);foreach(var p in l.pts)b.Encapsulate(p);Render(Path.Combine(o,$"top-MLR-{l.title.Split(' ')[0]}.jpg"),new Vector3(b.center.x,1500,b.center.z),Quaternion.Euler(90,0,0),true,Mathf.Max(b.extents.x,b.extents.z)+40,1400,1400);
    // what is near the route in MLR: signs, markings, lettering, anything with shortcut / optional / the names in it
    log.Add("   near it in MountainLoopReverse (within 30 m of the route): signs, markings and lettering");
    var near=new List<string>();
    foreach(var t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None)){
     var n=t.name;var tm=t.GetComponent<TextMesh>();bool interesting=tm||n.IndexOf("sign",StringComparison.OrdinalIgnoreCase)>=0||n.IndexOf("shortcut",StringComparison.OrdinalIgnoreCase)>=0||n.IndexOf("optional",StringComparison.OrdinalIgnoreCase)>=0||n.IndexOf("granite",StringComparison.OrdinalIgnoreCase)>=0||n.IndexOf("laurel",StringComparison.OrdinalIgnoreCase)>=0||n.IndexOf("arrow",StringComparison.OrdinalIgnoreCase)>=0||n.IndexOf("chevron",StringComparison.OrdinalIgnoreCase)>=0||n.IndexOf("gold",StringComparison.OrdinalIgnoreCase)>=0||n.IndexOf("marker",StringComparison.OrdinalIgnoreCase)>=0;
     if(!interesting)continue;var q=t.position;float best=float.MaxValue;foreach(var p in l.pts)best=Mathf.Min(best,Vector2.Distance(new(p.x,p.z),new(q.x,q.z)));if(best>30)continue;
     near.Add($"   {(t.gameObject.activeInHierarchy?"ACTIVE  ":"inactive")} {best,5:F1} m  {V(q)}  {P(t)}{(tm?"  text '"+tm.text.Replace("\n"," / ")+"'":"")}");}
    log.AddRange(near.Distinct().OrderBy(x=>x).Take(120));}
   // every shortcut sign / advice in MLR, wherever it is
   log.Add("== every object named with Shortcut / Granite / Laurel in MountainLoopReverse");
   foreach(var t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(t=>t.name.IndexOf("shortcut",StringComparison.OrdinalIgnoreCase)>=0||t.name.IndexOf("granite",StringComparison.OrdinalIgnoreCase)>=0||t.name.IndexOf("laurel",StringComparison.OrdinalIgnoreCase)>=0).OrderBy(t=>P(t))){
    var texts=t.GetComponentsInChildren<TextMesh>(true).Select(x=>x.text.Replace("\n"," / "));log.Add($"   {(t.gameObject.activeInHierarchy?"ACTIVE  ":"inactive")} {V(t.position)} {P(t)} comps {string.Join(";",t.GetComponents<Component>().Select(c=>c?c.GetType().Name:"null"))} texts [{string.Join(" | ",texts)}]");}
  }catch(Exception e){log.Add("ERROR "+e);}
  File.WriteAllLines(Path.Combine(o,"shortcuts-probe.txt"),log);EditorApplication.Exit(0);}
}
// 0.116 Part B candidates: the Street Loop Forward's trails that MountainLoopReverse.unity also has (LeviathanForward.json's branches),
// driven the other way: the driving surface every 2 m (top hit within 6 m of the line), grade over 10 m, steps, gaps, solids at wheel height,
// lettering and NO ENTRY near them; a top-down of each. PROBE_OUT/candidates.txt
public static class Report116Candidates {

 static void Survey(List<string> log,string o,string title,Vector3[] pts,float hw){
    // resample every 2 m
    var rs=new List<Vector3>();float acc=0;rs.Add(pts[0]);for(int i=1;i<pts.Length;i++){var a=pts[i-1];var c=pts[i];float d=Vector2.Distance(new(a.x,a.z),new(c.x,c.z));float t=2-acc;while(t<=d){rs.Add(Vector3.Lerp(a,c,t/d));t+=2;}acc=d-(t-2);}rs.Add(pts[^1]);
    var ys=new float[rs.Count];var names=new string[rs.Count];float prev=rs[0].y;
    for(int i=0;i<rs.Count;i++){var p=rs[i];var hits=Physics.RaycastAll(new Vector3(p.x,p.y+8,p.z),Vector3.down,22,~0,QueryTriggerInteraction.Ignore).Where(h=>h.normal.y>.35f&&!h.collider.attachedRigidbody&&h.collider.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)<0).OrderBy(h=>Mathf.Abs(h.point.y-(i>0?prev:p.y))).ToArray();
     if(hits.Length>0){ys[i]=hits[0].point.y;names[i]=hits[0].collider.name+$" n{hits[0].normal.y:F2}";prev=ys[i];}else{ys[i]=float.NaN;names[i]="NO GROUND";}}
    log.Add($"== {title}: {rs.Count*2} m, half width {hw}, from {V(rs[0])} to {V(rs[^1])}, line y {rs[0].y:F1} -> {rs[^1].y:F1}");
    float maxUp=0,maxDown=0;int gaps=0;var events=new List<string>();
    for(int i=0;i<rs.Count;i++){
     if(float.IsNaN(ys[i])){gaps++;events.Add($"  s {i*2} {V(rs[i])} NO GROUND");continue;}
     if(i>0&&!float.IsNaN(ys[i-1])&&Mathf.Abs(ys[i]-ys[i-1])>.6f)events.Add(FormattableString.Invariant($"  s {i*2} step {ys[i]-ys[i-1]:+0.0;-0.0} m on {names[i]} (was {names[i-1]})"));
     if(i>=5&&!float.IsNaN(ys[i-5])){float g=(ys[i]-ys[i-5])/10;if(g>maxUp)maxUp=g;if(-g>maxDown)maxDown=-g;if(g>.3f)events.Add(FormattableString.Invariant($"  s {i*2} climb {g*100:F0}% over 10 m at {V(rs[i])} on {names[i]}"));if(g<-.45f)events.Add(FormattableString.Invariant($"  s {i*2} drop {g*100:F0}% over 10 m at {V(rs[i])} on {names[i]}"));}
     var solids=Physics.OverlapCapsule(new Vector3(rs[i].x,ys[i]+1.3f,rs[i].z),new Vector3(rs[i].x,ys[i]+1.8f,rs[i].z),1.0f,~0,QueryTriggerInteraction.Ignore).Where(c=>!c.attachedRigidbody&&!c.isTrigger&&!c.name.StartsWith("Ground_")).Select(c=>c.name).Distinct().ToArray();
     if(solids.Length>0)events.Add($"  s {i*2} SOLID {string.Join(", ",solids)} at {V(rs[i])}");}
    log.Add(FormattableString.Invariant($"  max climb {maxUp*100:F0}%, max drop {maxDown*100:F0}% (10 m windows), no ground at {gaps} samples"));
    log.AddRange(events.Take(200));
    log.Add("  profile (every 10 m): "+string.Join(" ",Enumerable.Range(0,rs.Count/5+1).Select(k=>Math.Min(k*5,rs.Count-1)).Select(k=>FormattableString.Invariant($"{k*2}:{ys[k]:F1}"))));
    log.Add("  surfaces: "+string.Join(", ",names.GroupBy(x=>x.Split(' ')[0]).Select(g=>g.Key+" x"+g.Count())));
    foreach(var t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None)){
     var tm=t.GetComponent<TextMesh>();bool hit=tm||t.name.IndexOf("NO ENTRY",StringComparison.OrdinalIgnoreCase)>=0||t.name.IndexOf("ramp",StringComparison.OrdinalIgnoreCase)>=0||t.name.IndexOf("lip",StringComparison.OrdinalIgnoreCase)>=0||t.name.IndexOf("jump",StringComparison.OrdinalIgnoreCase)>=0||t.name.IndexOf("kicker",StringComparison.OrdinalIgnoreCase)>=0||t.name.IndexOf("glass",StringComparison.OrdinalIgnoreCase)>=0;
     if(!hit)continue;float best=rs.Min(p=>Vector2.Distance(new(p.x,p.z),new(t.position.x,t.position.z)));if(best>14)continue;
     log.Add($"  near ({best:F0} m) {(t.gameObject.activeInHierarchy?"ACTIVE":"inactive")} {V(t.position)} {t.name}{(tm?" '"+tm.text.Replace("\n"," / ")+"'":"")}");}
    var bb=new Bounds(rs[0],Vector3.zero);foreach(var p in rs)bb.Encapsulate(p);
    var go=new GameObject("cam");var cam=go.AddComponent<Camera>();cam.orthographic=true;cam.orthographicSize=Mathf.Max(bb.extents.x,bb.extents.z)+25;cam.farClipPlane=3000;go.transform.SetPositionAndRotation(new Vector3(bb.center.x,1500,bb.center.z),Quaternion.Euler(90,0,0));
    var rt=new RenderTexture(1200,1200,24);cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;var tex=new Texture2D(1200,1200,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1200,1200),0,0);
    float sz=cam.orthographicSize;for(int i=0;i<rs.Count;i++){int x=(int)((rs[i].x-bb.center.x)/sz*600+600),y=(int)((rs[i].z-bb.center.z)/sz*600+600);for(int dx=-1;dx<=1;dx++)for(int dy=-1;dy<=1;dy++)if(x+dx>=0&&x+dx<1200&&y+dy>=0&&y+dy<1200)tex.SetPixel(x+dx,y+dy,i<10?Color.green:i>rs.Count-10?Color.red:Color.yellow);}
    tex.Apply();File.WriteAllBytes(Path.Combine(o,$"cand-{title.Replace(" ","_")}.jpg"),tex.EncodeToJPG(85));RenderTexture.active=null;rt.Release();UnityEngine.Object.DestroyImmediate(go);
 }
 // the Forest Loop's trail in MountainLoopReverse.unity between own stations (PROBE_SPANS "a-b;c-d", wrapping past the end)
 public static void Forest(){
  var o=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(o);var log=new List<string>();
  try{
   EditorSceneManager.OpenScene("Assets/Scenes/MountainLoopReverse.unity");Physics.SyncTransforms();
   var road=UnityEngine.Object.FindObjectsByType<Racer.RaceRoad>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(r=>r.transform.parent&&r.transform.parent.name=="CR056 Forest Loop");
   var p=road.points;var st=new float[p.Length];for(int i=1;i<p.Length;i++)st[i]=st[i-1]+Vector2.Distance(new(p[i].x,p[i].z),new(p[i-1].x,p[i-1].z));float L=st[^1];
   foreach(var span in Environment.GetEnvironmentVariable("PROBE_SPANS").Split(';')){
    var ab=span.Split('-').Select(x=>float.Parse(x,System.Globalization.CultureInfo.InvariantCulture)).ToArray();var list=new List<Vector3>();
    if(ab[1]>ab[0]){for(int i=0;i<p.Length;i++)if(st[i]>=ab[0]&&st[i]<=ab[1])list.Add(p[i]);}
    else{for(int i=0;i<p.Length;i++)if(st[i]>=ab[0])list.Add(p[i]);for(int i=0;i<p.Length;i++)if(st[i]<=ab[1])list.Add(p[i]);}
    Survey(log,o,"Forest Loop "+span,list.ToArray(),5);}
  }catch(Exception e){log.Add("ERROR "+e);}
  File.WriteAllLines(Path.Combine(o,"forest.txt"),log);EditorApplication.Exit(0);}
 static string V(Vector3 v)=>FormattableString.Invariant($"({v.x:F1},{v.y:F1},{v.z:F1})");
 public static void Run(){
  var o=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(o);var log=new List<string>();
  try{
   var data=JsonUtility.FromJson<Racer.LeviathanCourse.Data>(File.ReadAllText("Assets/Resources/Leviathan/LeviathanForward.json"));
   EditorSceneManager.OpenScene("Assets/Scenes/MountainLoopReverse.unity");Physics.SyncTransforms();
   foreach(var b in data.branches){
    var f=b.pts;var pts=new Vector3[f.Length/3];for(int k=0;k<pts.Length;k++)pts[k]=new Vector3(f[3*k],f[3*k+1],f[3*k+2]);Array.Reverse(pts);
    Survey(log,o,b.title+" reversed",pts,b.halfWidth);
   }
  }catch(Exception e){log.Add("ERROR "+e);}
  File.WriteAllLines(Path.Combine(o,"candidates.txt"),log);EditorApplication.Exit(0);}
}
