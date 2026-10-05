#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem.LowLevel;

namespace Racer {
// 0.79 targeted checks (muted, isolated save). PROBE_CASES="case:args;..." starting in Street Loop Forward.
//  survey                  FreeRoamWorld: roads, signs, the reported spots (houses, driveway trees, road colours, junction).
[DefaultExecutionOrder(1000)]
public sealed class Report079Checks:MonoBehaviour {
 Action lateOnce;void LateUpdate(){var a=lateOnce;lateOnce=null;a?.Invoke();}
 IEnumerator Late(Action a){bool done=false;lateOnce=()=>{a();done=true;};while(!done)yield return null;}
 RaceDirector race;RaceFlow flow;readonly List<string> rows=new();string output,saveDir;int fails;
 void Note(string s){rows.Add(s);File.WriteAllLines(output+"/results.txt",rows);Debug.Log("REPORT079 "+s);}
 void Check(bool ok,string s){if(!ok)fails++;Note((ok?"PASS ":"FAIL ")+s);}
 static string Scene=>SceneManager.GetActiveScene().name;
 IEnumerator Start(){DontDestroyOnLoad(gameObject);AudioListener.volume=0;output=Environment.GetEnvironmentVariable("PROBE_OUT")+"/checks";Directory.CreateDirectory(output);Application.runInBackground=true;
  saveDir=Path.GetFullPath("Temp/Report079Save");if(Directory.Exists(saveDir))Directory.Delete(saveDir,true);Directory.CreateDirectory(saveDir);
  if(Scene!="StreetLoopGreybox"){SceneManager.LoadScene("StreetLoopGreybox");yield return null;yield return null;}
  Bind();yield return null;flow.UseValidationSave(saveDir);
  var title=FindAnyObjectByType<StartupTitle>();if(title)Destroy(title.gameObject);typeof(StartupTitle).GetField("completed",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,true);flow.EnterMenuAfterTitle();
  flow.Save.Settings.master=0;flow.Save.SaveSettings();yield return null;
  foreach(var spec in (Environment.GetEnvironmentVariable("PROBE_CASES")??"").Split(';').Where(x=>x.Length>0)){var a=spec.Split(':');Note("---- "+spec);
   IEnumerator run=a[0] switch{"survey"=>Survey(),"census"=>Census(a[1]),"grounding"=>Grounding(a[1]),"spots"=>Spots(a[1]),"roads"=>Roads(),"hud"=>Hud(),"huddebug"=>HudDebug(),"signs"=>Signs(a[1],a.Length>2?a[2]:""),_=>null};
   if(run!=null)yield return run;else Note("unknown case "+spec);Bind();}
  Note($"done, failures {fails}");File.WriteAllLines(output+"/done.txt",rows);UnityEditor.EditorApplication.Exit(0);}
 void Bind(){race=FindAnyObjectByType<RaceDirector>();flow=race?race.Flow:null;}
 IEnumerator Load(string scene){if(Scene!=scene){SceneManager.LoadScene(scene);yield return null;yield return null;}Bind();float t0=Time.realtimeSinceStartup;while(flow.Save==null&&Time.realtimeSinceStartup-t0<20)yield return null;yield return null;AudioListener.volume=0;}
 IEnumerator Menu(){Bind();if(flow.State!=RaceFlow.Stage.Ready){flow.Pause();flow.QuitRace();}yield return null;yield return null;Bind();}
 IEnumerator EnterRoam(string course,string vehicle){yield return Load(course);yield return Menu();
  flow.Save.Settings.vehicleId=vehicle;flow.Save.SaveSettings();flow.StartFreeRoam();float t0=Time.realtimeSinceStartup;
  while((Scene!=RaceFlow.RoamScene||flow==null||flow.State!=RaceFlow.Stage.Racing)&&Time.realtimeSinceStartup-t0<60){yield return null;Bind();AudioListener.volume=0;}
  Time.timeScale=1;yield return new WaitForSeconds(.5f);}
 string Shot(string name,int w=1600,int h=900){var f=$"{output}/{name}.png";ThreeFeatureValidation.CaptureUi(f,w,h);return f;}
 static string P(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n;}
 static string V(Vector3 v)=>$"{v.x:F1},{v.y:F2},{v.z:F1}";
 // the ground at x,z: the highest static, non-trigger surface below `from`, ignoring colliders under `skip`
 public static bool Ground(Vector3 at,float from,Transform skip,out RaycastHit hit){
  var hits=Physics.RaycastAll(new Vector3(at.x,from,at.z),Vector3.down,from+500,~0,QueryTriggerInteraction.Ignore).OrderBy(h=>h.distance);
  foreach(var h in hits){if(skip&&h.collider.transform.IsChildOf(skip))continue;if(h.collider.attachedRigidbody&&!h.collider.attachedRigidbody.isKinematic)continue;hit=h;return true;}
  hit=default;return false;}
 static string Surface(RaycastHit h){var r=h.collider.GetComponent<MeshRenderer>();var mat=r?r.sharedMaterial:null;string col="";
  if(h.collider is MeshCollider mc&&mc.sharedMesh&&mc.sharedMesh.isReadable&&h.triangleIndex>=0&&r&&r.GetComponent<MeshFilter>()?.sharedMesh==mc.sharedMesh){var m=mc.sharedMesh;var cs=m.colors;var tr=m.triangles;if(cs.Length==m.vertexCount){int i=h.triangleIndex*3;var b=h.barycentricCoordinate;var c=cs[tr[i]]*b.x+cs[tr[i+1]]*b.y+cs[tr[i+2]]*b.z;col=$" vcol {c.r:F3},{c.g:F3},{c.b:F3}";}}
  return $"{P(h.collider.transform)} [{h.collider.GetType().Name}] mat {(mat?mat.name:"-")} shader {(mat&&mat.shader?mat.shader.name:"-")}{(mat&&mat.HasProperty("_BaseColor")?" base "+mat.GetColor("_BaseColor"):mat&&mat.HasProperty("_Color")?" color "+mat.GetColor("_Color"):"")}{col} y {h.point.y:F2}";}

 IEnumerator Survey(){yield return EnterRoam("StreetLoopGreybox","moto");yield return new WaitForSeconds(1);
  var sb=new StringBuilder();
  sb.AppendLine("== RaceRoads");foreach(var r in FindObjectsByType<RaceRoad>(FindObjectsInactive.Include,FindObjectsSortMode.None)){if(r.points==null)continue;var b=new Bounds(r.points[0],Vector3.zero);foreach(var p in r.points)b.Encapsulate(p);sb.AppendLine($"{P(r.transform)} active {r.gameObject.activeInHierarchy} n {r.points.Length} trail {r.forestTrail} hwy {r.openHighway} first {V(r.points[0])} last {V(r.points[^1])} bounds {V(b.min)}..{V(b.max)}");}
  sb.AppendLine("== TextMesh");foreach(var t in FindObjectsByType<TextMesh>(FindObjectsInactive.Include,FindObjectsSortMode.None))sb.AppendLine($"{P(t.transform)} active {t.gameObject.activeInHierarchy} '{t.text.Replace("\n","/")}' at {V(t.transform.position)}");
  sb.AppendLine("== roots");foreach(var g in SceneManager.GetActiveScene().GetRootGameObjects())sb.AppendLine($"{g.name} active {g.activeSelf} children {g.transform.childCount}");
  sb.AppendLine("== road-ish objects");foreach(var t in FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None)){var n=t.name.ToLowerInvariant();if(!(n.Contains("road")||n.Contains("lane")||n.Contains("hwy")||n.Contains("highway")||n.Contains("driveway")||n.Contains("asphalt")||n.Contains("pavement")||n.Contains("street")||n.Contains("cherokee")||n.Contains("trickum")||n.Contains("jamerson")))continue;
   var rr=t.GetComponent<Renderer>();var mf=t.GetComponent<MeshFilter>();var mat=rr?rr.sharedMaterial:null;sb.AppendLine($"{P(t)} act {t.gameObject.activeInHierarchy} {(rr?"R bounds "+V(rr.bounds.center)+" size "+V(rr.bounds.size):"")} {(mf&&mf.sharedMesh?"mesh "+mf.sharedMesh.name+" v"+mf.sharedMesh.vertexCount+" colors "+mf.sharedMesh.colors.Length:"")} {(mat?mat.name+"/"+mat.shader.name:"")} col {(t.GetComponent<Collider>()?t.GetComponent<Collider>().GetType().Name:"")}");}
  File.WriteAllText(output+"/survey-world.txt",sb.ToString());sb.Clear();
  // reported spots
  var spots=new (string name,Vector3 p)[]{("driveway trees",new(454,0,-29.9f)),("hwy92 seam",new(169.6f,0,551.4f)),("junction",new(313.6f,0,549.4f)),("house 576",new(576.5f,0,-414.3f)),("house 491",new(491.5f,0,-589.9f)),("house 396",new(396.7f,0,-501.5f)),("house 256",new(256.7f,0,-413.8f))};
  foreach(var (name,p) in spots){sb.AppendLine("== "+name+" "+V(p));
   for(int dx=-30;dx<=30;dx+=6)for(int dz=-30;dz<=30;dz+=15){var q=p+new Vector3(dx,0,dz);if(Ground(q,800,null,out var h))sb.AppendLine($"  ground {dx},{dz}: {Surface(h)}");}
   foreach(var c in Physics.OverlapBox(p+Vector3.up*100,new Vector3(14,400,14),Quaternion.identity,~0,QueryTriggerInteraction.Collide).OrderBy(c=>P(c.transform)))sb.AppendLine($"  collider {P(c.transform)} [{c.GetType().Name}] trig {c.isTrigger} bounds {V(c.bounds.center)} size {V(c.bounds.size)}");
   foreach(var c in FindObjectsByType<Collider>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(c=>(!c.enabled||!c.gameObject.activeInHierarchy)&&Vector2.Distance(new(c.transform.position.x,c.transform.position.z),new(p.x,p.z))<20))sb.AppendLine($"  DISABLED collider {P(c.transform)} [{c.GetType().Name}] enabled {c.enabled} active {c.gameObject.activeInHierarchy} at {V(c.transform.position)}");
   foreach(var r in FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(r=>{var b=r.bounds;return b.size.x<200&&b.size.z<200&&Mathf.Abs(b.center.x-p.x)<b.extents.x+10&&Mathf.Abs(b.center.z-p.z)<b.extents.z+10;}))sb.AppendLine($"  renderer {P(r.transform)} on {r.enabled&&r.gameObject.activeInHierarchy} forceOff {r.forceRenderingOff} bounds {V(r.bounds.center)} size {V(r.bounds.size)} mat {(r.sharedMaterial?r.sharedMaterial.name:"")}");
   var world=SceneryWorld.Current;var trunks=world&&world.Trees?world.Trees.TrunksNear(p,15):new List<string>();foreach(var t in trunks)sb.AppendLine("  new tree "+t);}
  File.WriteAllText(output+"/survey-spots.txt",sb.ToString());sb.Clear();
  // buildings
  var root=SceneManager.GetActiveScene().GetRootGameObjects().FirstOrDefault(g=>g.name=="Remembered houses and approximate buildings");
  if(root)foreach(Transform site in root.transform){var wall=site.GetComponentsInChildren<BoxCollider>(true).FirstOrDefault(b=>b.name=="Wall collision");
   sb.AppendLine($"{site.name} at {V(site.position)} children {string.Join(",",site.Cast<Transform>().Select(c=>c.name))}");
   if(wall){var b=wall.bounds;float lo=1e9f,hi=-1e9f;foreach(var (x,z) in new[]{(b.min.x,b.min.z),(b.max.x,b.min.z),(b.min.x,b.max.z),(b.max.x,b.max.z),(b.center.x,b.center.z)})if(Ground(new Vector3(x,0,z),b.max.y+50,site,out var h)){lo=Mathf.Min(lo,h.point.y);hi=Mathf.Max(hi,h.point.y);}
    sb.AppendLine($"   wall {V(b.min)}..{V(b.max)} ground {lo:F2}..{hi:F2} -> floor above lowest ground {b.min.y-lo:F2}");}}
  File.WriteAllText(output+"/survey-buildings.txt",sb.ToString());
  // views of the reported spots
  var cam=Camera.main;var chase=FindAnyObjectByType<ChaseCamera>();if(chase)chase.enabled=false;var cv=CameraViews.Current;if(cv)cv.enabled=false;
  foreach(var (name,p) in spots){Ground(p,800,null,out var g);var eye=g.point+new Vector3(-14,7,-14);yield return Late(()=>{cam.transform.position=eye;cam.transform.LookAt(g.point+Vector3.up*2);Shot("survey-"+name.Replace(' ','-'));});
   eye=g.point+new Vector3(14,7,14);yield return Late(()=>{cam.transform.position=eye;cam.transform.LookAt(g.point+Vector3.up*2);Shot("survey-"+name.Replace(' ','-')+"-b");});}
  if(chase)chase.enabled=true;if(cv)cv.enabled=true;
  Note("survey written");}

 // Per scene: what drivable-looking surfaces are made of (4 m grid of rays), and what every new tree / bush stands on.
 IEnumerator Census(string scene){
  if(scene==RaceFlow.RoamScene)yield return EnterRoam("StreetLoopGreybox","moto");
  else{yield return Load(scene);yield return Menu();race.opponents=false;race.traffic=false;race.laps=1;flow.StartRace();float tr=Time.realtimeSinceStartup;while(flow.State!=RaceFlow.Stage.Racing&&Time.realtimeSinceStartup-tr<30){AudioListener.volume=0;yield return null;}yield return new WaitForSecondsRealtime(1);}
  var world=SceneryWorld.Current;var roads=FindObjectsByType<RaceRoad>(FindObjectsSortMode.None).Where(r=>r.points!=null&&r.points.Length>1).ToArray();foreach(var r in roads)r.Initialize();
  var bounds=new Bounds(Vector3.zero,Vector3.zero);foreach(var c in FindObjectsByType<MeshCollider>(FindObjectsSortMode.None))if(c.name.StartsWith("Ground_"))bounds.Encapsulate(c.bounds);
  var agg=new Dictionary<string,(int n,Vector3 at)>();
  for(float x=bounds.min.x;x<bounds.max.x;x+=4)for(float z=bounds.min.z;z<bounds.max.z;z+=4){if(!Ground(new Vector3(x,0,z),900,null,out var h))continue;var r=h.collider.GetComponent<Renderer>();var m=r?r.sharedMaterial:null;
   string key=(m?m.name+" | "+m.shader.name:"(no renderer) "+h.collider.name);
   if(m&&m.shader.name.StartsWith("Racer/")&&h.collider is MeshCollider mc&&mc.sharedMesh&&mc.sharedMesh.isReadable&&SceneryGround.Terrain(mc.sharedMesh,out var cs,out var tr)&&h.triangleIndex>=0){int i=h.triangleIndex*3;var b=h.barycentricCoordinate;var c=cs[tr[i]]*b.x+cs[tr[i+1]]*b.y+cs[tr[i+2]]*b.z;key+=c.b-c.r>.005f?" [paved colour]":c.r>c.g+.02f?" [dirt colour]":c.g>Mathf.Max(c.r,c.b)+.02f?" [grass colour]":" [other colour]";}
   agg[key]=agg.TryGetValue(key,out var e)?(e.n+1,e.at):(1,h.point);}
  File.WriteAllLines($"{output}/census-surfaces-{scene}.txt",agg.OrderByDescending(k=>k.Value.n).Select(k=>$"{k.Value.n}\t{k.Key}\tat {V(k.Value.at)}"));
  // trees
  var sb=new StringBuilder("variant\tsource\tx\ty\tz\tradius\theight\tsurface\tcolour\tgap\troad\troadDist\thalfWidth\n");
  if(world&&world.Trees)foreach(var t in world.Trees.Placements){string surf="-",col="-";float gap=99;
   var hits=Physics.RaycastAll(t.bottom+Vector3.up*2.5f,Vector3.down,8,~0,QueryTriggerInteraction.Ignore).Where(h=>!(h.collider is BoxCollider bc&&bc.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0)&&!h.collider.attachedRigidbody).OrderBy(h=>h.distance);
   foreach(var h in hits){var r=h.collider.GetComponent<Renderer>();var m=r?r.sharedMaterial:null;surf=P(h.collider.transform)+" | "+(m?m.name:"-");gap=t.bottom.y-h.point.y;
    if(h.collider is MeshCollider mc&&mc.sharedMesh&&mc.sharedMesh.isReadable&&SceneryGround.Terrain(mc.sharedMesh,out var cs,out var tr)&&h.triangleIndex>=0){int i=h.triangleIndex*3;var b=h.barycentricCoordinate;var c=cs[tr[i]]*b.x+cs[tr[i+1]]*b.y+cs[tr[i+2]]*b.z;col=$"{c.r:F3},{c.g:F3},{c.b:F3}";}break;}
   string rn="-";float rd=999,hw=0;foreach(var r in roads){float s=r.Project(t.bottom,out _);var q=r.At(s,out _);var d=q-t.bottom;d.y=0;if(d.magnitude<rd){rd=d.magnitude;rn=r.name;hw=r.HalfWidth(s);}}
   sb.Append($"{t.variant}\t{t.source}\t{t.bottom.x:F1}\t{t.bottom.y:F2}\t{t.bottom.z:F1}\t{t.radius:F2}\t{t.height:F1}\t{surf}\t{col}\t{gap:F2}\t{rn}\t{rd:F1}\t{hw:F1}\n");}
  File.WriteAllText($"{output}/census-trees-{scene}.tsv",sb.ToString());
  Note($"{scene}: census written ({world?.Trees?.Placements.Count} placements)");
  if(scene!=RaceFlow.RoamScene)yield return Menu();}

 // ---------- Parts D and E: everything stands on the ground; nothing grows on a drivable surface ----------
 IEnumerator EnterScene(string scene){
  if(scene==RaceFlow.RoamScene)yield return EnterRoam("StreetLoopGreybox","moto");
  else{yield return Load(scene);yield return Menu();race.opponents=false;race.traffic=false;race.laps=1;flow.StartRace();float tr=Time.realtimeSinceStartup;while(flow.State!=RaceFlow.Stage.Racing&&Time.realtimeSinceStartup-tr<30){AudioListener.volume=0;yield return null;}yield return new WaitForSecondsRealtime(1);}}
 static bool Visible(Renderer r)=>r&&r.enabled&&r.gameObject.activeInHierarchy&&!r.forceRenderingOff;
 static bool IsTerrain(Renderer r){var m=r.sharedMaterial;return r.name.StartsWith("Ground_")||(m&&m.shader&&(m.shader.name=="Racer/MarkedGround"||(m.shader.name=="Racer/GreyboxGround"&&!(m.HasProperty("_Vegetation")&&m.GetFloat("_Vegetation")>.5f))));}
 // the lowest drawn surface above (x,z) among `meshes` (temporary colliders), from far below
 static float VisualBottom(List<MeshCollider> meshes,Vector3 at,out string what){float best=float.MaxValue;what="-";var ray=new Ray(new Vector3(at.x,at.y-40,at.z),Vector3.up);
  foreach(var c in meshes)if(c.Raycast(ray,out var h,80)&&h.point.y<best){best=h.point.y;what=c.name;}return best;}
 static List<MeshCollider> TempColliders(IEnumerable<MeshRenderer> rs,Transform parent){var list=new List<MeshCollider>();
  foreach(var r in rs){var f=r.GetComponent<MeshFilter>();if(!f||!f.sharedMesh)continue;var g=new GameObject(r.name);g.layer=31;g.transform.SetParent(parent,false);g.transform.SetPositionAndRotation(r.transform.position,r.transform.rotation);g.transform.localScale=r.transform.lossyScale;var mc=g.AddComponent<MeshCollider>();mc.sharedMesh=f.sharedMesh;list.Add(mc);}
  Physics.SyncTransforms();return list;}
 IEnumerator Grounding(string scene){yield return EnterScene(scene);var world=SceneryWorld.Current;Check(world&&world.Buildings&&Scenery.New,$"{Scene}: new scenery built");if(!world||!world.Buildings)yield break;
  var sb=new StringBuilder($"{Scene}: largest gap between the bottom of the drawn building and the ground, sampled every 0.5 m around the base, 1.5 cm outside the walls\n");
  var temp=new GameObject("Report079 temp colliders").transform;
  var root=SceneManager.GetActiveScene().GetRootGameObjects().FirstOrDefault(g=>g.name=="Remembered houses and approximate buildings");
  var renderers=FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r=>Visible(r)&&!IsTerrain(r)&&!r.GetComponentInParent<ArcadeVehicle>()&&!r.GetComponent<TextMesh>()).ToArray();
  int buildings=0,clean=0;float worst=0;
  foreach(Transform site in root.transform){
   // the walls to walk round: the generated buildings' wall collider; for a building with its own model, each of its
   // wall colliders (Dan's house is not a rectangle)
   var walls=site.GetComponentsInChildren<BoxCollider>(true).Where(b=>b.name=="Wall collision").ToList();
   bool own=site.name.StartsWith("Dan")||site.name.StartsWith("Friend")||site.name.StartsWith("Fox");
   if(walls.Count==0)walls=site.GetComponentsInChildren<BoxCollider>().Where(b=>!b.isTrigger&&b.name.IndexOf("wall",StringComparison.OrdinalIgnoreCase)>=0).ToList();
   if(walls.Count==0){sb.AppendLine($"  {site.name}: no walls found, skipped");continue;}
   var worldBox=walls[0].bounds;foreach(var w in walls)worldBox.Encapsulate(w.bounds);worldBox.Expand(new Vector3(1,200,1));
   var near=renderers.Where(r=>r.bounds.Intersects(worldBox)&&(r.transform.IsChildOf(site)||r.name.Contains(site.name)||r.name.StartsWith("Kyle's house")&&site.name.StartsWith("Friend")||r.name.Contains("render batch")||r.transform.parent&&r.transform.parent.name.Contains("render batches")||r.name.EndsWith("(kept)"))).ToList();
   var cols=TempColliders(near,temp);float gap=0;Vector3 at=default;string what="-";int samples=0,bare=0;
   foreach(var wc in walls){var t=wc.transform;var c=wc.center;var e=wc.size*.5f;
    // the four bottom edges in the collider's own frame, 1.5 cm outside (in world metres)
    float ox=.015f/Mathf.Max(1e-4f,Mathf.Abs(t.lossyScale.x)),oz=.015f/Mathf.Max(1e-4f,Mathf.Abs(t.lossyScale.z));
    var corners=new[]{new Vector3(c.x-e.x-ox,c.y-e.y,c.z-e.z-oz),new Vector3(c.x+e.x+ox,c.y-e.y,c.z-e.z-oz),new Vector3(c.x+e.x+ox,c.y-e.y,c.z+e.z+oz),new Vector3(c.x-e.x-ox,c.y-e.y,c.z+e.z+oz)};
    for(int k=0;k<4;k++){var a=t.TransformPoint(corners[k]);var b=t.TransformPoint(corners[(k+1)%4]);float len=Vector3.Distance(a,b);int n=Mathf.Max(1,Mathf.CeilToInt(len/.5f));
     for(int i=0;i<=n;i++){var wp=Vector3.Lerp(a,b,(float)i/n)+Vector3.up*2.5f;if(!Ground(wp,wp.y,site,out var gh))continue;samples++;float vb=VisualBottom(cols,gh.point,out var w);
      if(vb==float.MaxValue||vb>a.y+1.5f){bare++;continue;} // nothing drawn over this point (an open side, a drive-through)
      float g=vb-gh.point.y;if(g>gap){gap=g;at=gh.point;what=w;}}}}
   foreach(var cc in cols)Destroy(cc.gameObject);
   buildings++;if(gap<=.02f)clean++;worst=Mathf.Max(worst,gap);
   sb.AppendLine($"  {(gap<=.02f?"OK  ":"GAP ")}{site.name}{(own?" (own model)":"")}: largest gap {gap:F2} m over {samples} points{(bare>0?$" ({bare} with no wall drawn above)":"")}{(gap>.02f?$" at {V(at)} (lowest drawn part there: {what})":"")}");}
  File.WriteAllText($"{output}/grounding-buildings-{Scene}.txt",sb.ToString());
  Check(clean==buildings,$"{Scene}: buildings with no daylight under them {clean}/{buildings} (worst gap {worst:F2} m; list in grounding-buildings-{Scene}.txt)");
  // posts and rocks swapped by the 0.78 kit: nothing floats (no daylight under the drawn shape: its underside, read from a
  // temporary copy of the drawn mesh, against the ground at points across its footprint), boulders not half buried
  // (their mean ground at most half way up); posts are meant to be set in the ground and outcrops to come out of it, so
  // for those only daylight counts. Stones of a clue cairn rest on the stone below.
  var props=new StringBuilder();int checkedProps=0,floating=0,buried=0;
  var drawn=world.Props.Swapped.Where(x=>x&&Visible(x.GetComponent<MeshRenderer>())).ToList();
  var propCols=TempColliders(drawn.Select(x=>x.GetComponent<MeshRenderer>()),temp);
  for(int pi=0;pi<drawn.Count;pi++){var f=drawn[pi];var r=f.GetComponent<MeshRenderer>();string n=f.name.ToLowerInvariant();bool post=n.Contains("post")&&!n.Contains("board");bool rock=!post&&new[]{"rock","boulder","outcrop","stone","cairn"}.Any(n.Contains)&&!new[]{"board","sign","rocky way","fire ring"}.Any(n.Contains);if(!rock&&!post)continue;
   var b=r.bounds;if(b.size.y<.05f)continue;
   if(post)
   {
    // a post: its bottom against the lowest ground under its corners (posts fixed to a structure, with no ground within
    // 4 m below, are not ground-standing and are left out)
    float lo=float.MaxValue;foreach(var (x,z) in new[]{(b.min.x,b.min.z),(b.max.x,b.min.z),(b.min.x,b.max.z),(b.max.x,b.max.z)})if(Ground(new Vector3(x,0,z),b.max.y+1,f.transform,out var ph)&&ph.point.y>b.min.y-4)lo=Mathf.Min(lo,ph.point.y);
    if(lo==float.MaxValue)continue;checkedProps++;float pg=b.min.y-lo;if(pg>.05f){floating++;props.AppendLine($"  FLOATS {pg:F2} m {P(f.transform)} at {V(b.center)}: bottom {b.min.y:F2}, lowest ground {lo:F2}");}
    continue;
   }
   var mine=propCols[pi];float gap=0,minGap=float.MaxValue,groundSum=0;int k=0;
   for(int ix=0;ix<3;ix++)for(int iz=0;iz<3;iz++){var at=new Vector3(Mathf.Lerp(b.min.x,b.max.x,.25f+ix*.25f),0,Mathf.Lerp(b.min.z,b.max.z,.25f+iz*.25f));
    if(!mine.Raycast(new Ray(new Vector3(at.x,b.min.y-20,at.z),Vector3.up),out var under,b.size.y+40)||under.normal.y>0)continue; // the shape does not cover this point, or is open underneath (sunk into the ground)
    float support=float.MinValue;
    if(Ground(new Vector3(at.x,0,at.z),under.point.y+.5f,f.transform,out var gh))support=gh.point.y;
    for(int oj=0;oj<propCols.Count;oj++)if(oj!=pi&&drawn[oj].transform.parent==f.transform.parent&&propCols[oj].Raycast(new Ray(new Vector3(at.x,under.point.y+.05f,at.z),Vector3.down),out var sh,1f))support=Mathf.Max(support,sh.point.y);
    if(support==float.MinValue)continue;k++;gap=Mathf.Max(gap,under.point.y-support);minGap=Mathf.Min(minGap,under.point.y-support);groundSum+=support;}
   if(k==0)continue;checkedProps++;float meanGround=groundSum/k;bool boulder=!n.Contains("outcrop")&&!n.Contains("cairn")&&!n.Contains("stacked");
   // floating: no contact anywhere (a rock at a cliff edge may overhang; its largest overhang is listed)
   if(gap>.3f)props.AppendLine($"  (overhang {gap:F2} m, resting on the ground) {P(f.transform)} at {V(b.center)}");
   bool isFloat=minGap>.05f,isBuried=boulder&&(meanGround-b.min.y)/b.size.y>.5f;if(isFloat)floating++;if(isBuried)buried++;
   if(isFloat||isBuried)props.AppendLine($"  {(isFloat?$"FLOATS {minGap:F2} m":"HALF BURIED")} {P(f.transform)} at {V(b.center)}: bottom {b.min.y:F2}, mean ground under it {meanGround:F2}, height {b.size.y:F2}");}
  foreach(var c in propCols)Destroy(c.gameObject);
  File.WriteAllText($"{output}/grounding-props-{Scene}.txt",props.ToString());
  Check(floating==0&&buried==0,$"{Scene}: posts and rocks checked {checkedProps} (clue cairns re-seated on today's ground: {world.Props.CairnsSeated}): floating {floating}, half buried {buried} (grounding-props-{Scene}.txt)");
  // trees: what the rule did, then every tree, bush and clump left standing re-checked against drivable surfaces
  var trails=FindObjectsByType<RaceRoad>(FindObjectsSortMode.None).Where(t=>t.forestTrail&&t.points!=null&&t.points.Length>1).ToList();foreach(var t in trails)t.Initialize();
  int left=0;var leftList=new List<string>();foreach(var t in world.Trees.Placements){if(t.bottom.y<-1000)continue;if(SceneryTrees.OnDrivable(t.bottom,trails,out var w)){left++;leftList.Add($"{t.source} at {V(t.bottom)} on {w}");}}
  int kept=world.Trees.DrivableReport.Count(x=>x.StartsWith("KEPT"));
  File.WriteAllLines($"{output}/trees-drivable-{Scene}.txt",new[]{$"{Scene}: rule found {world.Trees.DrivableReport.Count}; removed or hidden {world.Trees.DrivableSkipped}; kept beside a race line {kept}"}.Concat(world.Trees.DrivableReport).Concat(new[]{"-- still standing on a drivable surface (excluding the kept ones):"}).Concat(leftList));
  Check(left==kept,$"{Scene}: trees / bushes / clumps on a drivable surface: found {world.Trees.DrivableReport.Count}, removed or hidden {world.Trees.DrivableSkipped}, kept beside a race line {kept}; left standing on one now {left}");
  // ground detail: ride the camera past the reported driveway and along Hwy 92 so cells fill, then check every placed tuft
  var cam=Camera.main;var chase=FindAnyObjectByType<ChaseCamera>();if(chase)chase.enabled=false;var cv=CameraViews.Current;if(cv)cv.enabled=false;
  foreach(var p in new[]{new Vector3(454,90,-29.9f),new Vector3(169.6f,14,551.4f),new Vector3(313.6f,14,549.4f),new Vector3(430,40,-180)})for(int i=0;i<40;i++){yield return Late(()=>{cam.transform.position=p;});}
  int tufts=0,onRoad=0;foreach(var p in world.Ground.PlacedPoints){tufts++;if(SceneryTrees.OnDrivable(p,trails,out _))onRoad++;}
  if(chase)chase.enabled=true;if(cv)cv.enabled=true;
  Check(onRoad==0,$"{Scene}: ground detail placed so far {tufts}; on a drivable surface {onRoad}");
  Destroy(temp.gameObject);
  // static colliders identical with New and Classic (nothing here adds, moves or removes one), and their census for the
  // comparison against the scene file
  static string Cols(){var sb2=new StringBuilder();foreach(var c in FindObjectsByType<Collider>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(c=>!c.attachedRigidbody||c.attachedRigidbody.isKinematic).OrderBy(c=>P(c.transform)).ThenBy(c=>c.bounds.center.x).ThenBy(c=>c.bounds.center.z))sb2.Append($"{P(c.transform)}|{c.GetType().Name}|{c.enabled&&c.gameObject.activeInHierarchy}|{c.isTrigger}|{c.bounds.center:F3}|{c.bounds.size:F3}\n");return sb2.ToString();}
  string ca=Cols();Scenery.Set(false);yield return null;string cb=Cols();Scenery.Set(true);yield return null;string cn=Cols();File.WriteAllText($"{output}/colliders-{Scene}.txt",ca);
  Check(ca==cb&&cb==cn,$"{Scene}: every static collider identical New / Classic / New ({ca.Split('\n').Length-1} colliders)");
  if(scene!=RaceFlow.RoamScene)yield return Menu();}

 // ---------- Part F: one asphalt ----------
 // Views along Hwy 92 across the seam Dan reported and of the driveway, Day / Night / Rain, New and Classic; the mean
 // colour of the road just either side of the seam (and of the driveway against the road) is read back from each shot.
 static Color Patch(Texture2D t,Vector3 sp,int r=7){Color s=default;int n=0;for(int x=-r;x<=r;x++)for(int y=-r;y<=r;y++){int px=Mathf.RoundToInt(sp.x)+x,py=Mathf.RoundToInt(sp.y)+y;if(px<0||py<0||px>=t.width||py>=t.height)continue;s+=t.GetPixel(px,py);n++;}return n>0?s/n:Color.black;}
 static float Diff(Color a,Color b)=>Mathf.Max(Mathf.Abs(a.r-b.r),Mathf.Max(Mathf.Abs(a.g-b.g),Mathf.Abs(a.b-b.b)));
 static string C(Color c)=>$"{c.r*255:F0},{c.g*255:F0},{c.b*255:F0}";
 IEnumerator Roads(){yield return EnterRoam("StreetLoopGreybox","moto");yield return new WaitForSeconds(1);var world=SceneryWorld.Current;
  Note($"paving: {world.Paving?.Count} renderers use the one asphalt with New: {string.Join("; ",world.Paving?.Names.GroupBy(x=>x).Select(g=>g.Key+(g.Count()>1?" x"+g.Count():""))??new string[0])}");
  var cam=Camera.main;var chase=FindAnyObjectByType<ChaseCamera>();if(chase)chase.enabled=false;var cv=CameraViews.Current;if(cv)cv.enabled=false;var hud=FindObjectsByType<Canvas>(FindObjectsSortMode.None);foreach(var c in hud)c.enabled=false;
  // seam: terrain road ends near x=163.6 on Hwy 92 at z~551; patches 4 m either side in the lane between the dashes
  Ground(new Vector3(158,0,547),50,null,out var gw);Ground(new Vector3(170,0,547),50,null,out var ge);Ground(new Vector3(445,0,-31.9f),120,null,out var gd);Ground(new Vector3(473,0,-36),120,null,out var gr);
  Note($"seam patches on: west {Surface(gw)}; east {Surface(ge)}; driveway {Surface(gd)}; road by the driveway {Surface(gr)}");
  var views=new (string name,Vector3 eye,Vector3 look,Vector3[] probes)[]{("hwy92-seam",new(140,12.5f,546),new(185,9,551),new[]{gw.point,ge.point}),("hwy92-seam-low",new(150,10.4f,549),new(190,8.6f,550),new[]{gw.point,ge.point}),("junction",new(296,13,532),new(318,8.5f,549),new Vector3[0]),("driveway",new(418,91,-19),new(462,85,-33),new[]{gd.point,gr.point})};
  foreach(var (cond,t,w) in new[]{("day",TimeOfDay.Day,Weather.Clear),("night",TimeOfDay.Night,Weather.Clear),("rain",TimeOfDay.Day,Weather.Rain)}){
   WorldLook.Current.Pin(LookPresets.Compose(t,w));
   foreach(var v in views)foreach(bool fresh in new[]{true,false}){Scenery.Set(fresh);yield return null;yield return new WaitForSecondsRealtime(t==TimeOfDay.Night?1.2f:.4f);string file=null;
    yield return Late(()=>{cam.transform.position=v.eye;cam.transform.LookAt(v.look);file=Shot($"roads-{v.name}-{cond}-{(fresh?"new":"classic")}");});
    if(v.probes.Length<2)continue;var tex=new Texture2D(2,2);tex.LoadImage(File.ReadAllBytes(file));
    Vector3 S(Vector3 p){var s=cam.WorldToScreenPoint(p+Vector3.up*.02f);return new Vector3(s.x*tex.width/cam.pixelWidth,s.y*tex.height/cam.pixelHeight,s.z);}
    var a=Patch(tex,S(v.probes[0]));var b=Patch(tex,S(v.probes[1]));Destroy(tex);float d=Diff(a,b);
    string label=v.name=="driveway"?"driveway vs road":"road either side of the seam";
    if(fresh&&v.name!="driveway")Check(d<=.04f,$"{v.name} {cond} New: {label} {C(a)} | {C(b)} (largest channel difference {d*255:F0}/255)");
    else Note($"{v.name} {cond} {(fresh?"New":"Classic")}: {label} {C(a)} | {C(b)} (largest channel difference {d*255:F0}/255)");}}
  WorldLook.Current.Pin(null);Scenery.Set(true);foreach(var c in hud)if(c)c.enabled=true;if(chase)chase.enabled=true;if(cv)cv.enabled=true;}

 // ---------- Part G: street-name signs ----------
 ArcadeVehicle Car=>race.vehicle;
 void Put(ArcadeVehicle car,Vector3 p,Quaternion r){car.Body.isKinematic=false;car.transform.SetPositionAndRotation(p,r);car.Body.position=p;car.Body.rotation=r;car.Body.linearVelocity=Vector3.zero;car.Body.angularVelocity=Vector3.zero;car.ClearSteering();Physics.SyncTransforms();race.ResetSampling(p,race.Clock);car.GetComponent<VehicleRespawn>().CancelRecovery();}
 IEnumerator Signs(string scene,string shots){yield return EnterScene(scene);var signs=SceneryWorld.Current?SceneryWorld.Current.Signs:null;
  Check(signs&&signs.Placed.Count==3,$"{Scene}: street-name signs placed {signs?.Placed.Count}/3");if(!signs)yield break;foreach(var r in signs.Report)Note("  "+r);
  foreach(var (post,lower,upper,centre) in signs.Placed){bool col=Physics.OverlapCapsule(post+Vector3.up*.2f,post+Vector3.up*3.4f,.12f,~0,QueryTriggerInteraction.Collide).Any(c=>c.transform.IsChildOf(signs.transform)||c.name.Contains("Street sign"));
   var lines=CoursePreviewCatalog.Courses.SelectMany(c=>new[]{c.main}.Concat((c.branches??new CoursePreviewCatalog.Path[0]).Select(b=>b.points))).Where(l=>l!=null&&l.Length>1);
   float clear=lines.Min(l=>{float best=1e9f;for(int i=0;i+1<l.Length;i++){var a=new Vector2(l[i].x,l[i].z);var b=new Vector2(l[i+1].x,l[i+1].z);var ab=b-a;float t=ab.sqrMagnitude<1e-6f?0:Mathf.Clamp01(Vector2.Dot(new Vector2(post.x,post.z)-a,ab)/ab.sqrMagnitude);best=Mathf.Min(best,(a+ab*t-new Vector2(post.x,post.z)).magnitude);}return best;});
   Check(!col&&!SceneryTrees.OnDrivable(post+Vector3.up*.05f,null,out _)&&clear>=StreetSigns.LineClearance,$"{Scene}: {lower} / {upper} sign at {V(post)}: no collider, on the verge, {clear:F1} m from the nearest race line or shortcut of any course");}
  if(shots!="shots"){if(scene!=RaceFlow.RoamScene)yield return Menu();yield break;}
  var cam=Camera.main;var chase=FindAnyObjectByType<ChaseCamera>();var cv=CameraViews.Current;var hud=FindObjectsByType<Canvas>(FindObjectsSortMode.None);foreach(var c in hud)c.enabled=false;
  int k=0;foreach(var (post,lower,upper,centre) in signs.Placed){k++;var away=centre-post;away.y=0;away.Normalize();
   // the player's vehicle parked on the road 22 m out, facing the sign (headlights at night), camera just behind it
   Ground(post+away*22,post.y+30,null,out var g);var face=post-g.point;face.y=0;Car.enabled=false;Put(Car,g.point+Vector3.up*.6f,Quaternion.LookRotation(face));
   foreach(var (cond,t) in new[]{("day",TimeOfDay.Day),("night",TimeOfDay.Night)}){WorldLook.Current.Pin(LookPresets.Compose(t,Weather.Clear));if(chase)chase.enabled=false;if(cv)cv.enabled=false;yield return new WaitForSecondsRealtime(t==TimeOfDay.Night?1.5f:.5f);
    yield return Late(()=>{cam.transform.position=g.point-face.normalized*5+Vector3.up*2.4f;cam.transform.LookAt(post+Vector3.up*2.6f);Shot($"sign-{k}-{lower.Replace(' ','_')}-{upper.Replace(' ','_')}-{cond}-approach",1920,1080);});
    yield return Late(()=>{cam.transform.position=post+away*4.5f+Vector3.Cross(Vector3.up,away)*1.5f+Vector3.up*2.2f;cam.transform.LookAt(post+Vector3.up*2.95f);Shot($"sign-{k}-{lower.Replace(' ','_')}-{upper.Replace(' ','_')}-{cond}-close",1920,1080);});}
   Car.enabled=true;}
  WorldLook.Current.Pin(null);foreach(var c in hud)if(c)c.enabled=true;if(chase)chase.enabled=true;if(cv)cv.enabled=true;
  if(scene!=RaceFlow.RoamScene)yield return Menu();}

 // ---------- Parts A, B, C, H: the Free Roam HUD, the minimap, the camera controls on screen ----------
 static Rect ScreenRect(RectTransform r){var c=new Vector3[4];r.GetWorldCorners(c);var cv=r.GetComponentInParent<Canvas>().rootCanvas;Vector2 S(Vector3 w)=>cv.renderMode==RenderMode.ScreenSpaceOverlay?(Vector2)w:RectTransformUtility.WorldToScreenPoint(cv.worldCamera,w);var a=S(c[0]);var b=S(c[2]);return Rect.MinMaxRect(Mathf.Min(a.x,b.x),Mathf.Min(a.y,b.y),Mathf.Max(a.x,b.x),Mathf.Max(a.y,b.y));}
 IEnumerator PadPress(GamepadButton button){var d=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Gamepad>();UnityEngine.InputSystem.InputSystem.QueueStateEvent(d,new UnityEngine.InputSystem.LowLevel.GamepadState{buttons=1u<<(int)button});yield return null;yield return null;UnityEngine.InputSystem.InputSystem.QueueStateEvent(d,new UnityEngine.InputSystem.LowLevel.GamepadState());yield return null;yield return null;UnityEngine.InputSystem.InputSystem.RemoveDevice(d);yield return null;}
 string Shot4k(string name)=>Shot(name,3840,2160);
 IEnumerator HudDebug(){yield return EnterRoam("StreetLoopGreybox","moto");
  for(int i=0;i<4;i++){var huds=FindObjectsByType<RaceHud>(FindObjectsSortMode.None);foreach(var h in huds){object F(string n)=>typeof(RaceHud).GetField(n,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(h);
    Note($"hud {h.GetEntityId()} scene {h.gameObject.scene.name} enabled {h.isActiveAndEnabled} race {(h.race?h.race.name:"-")} flowSession {h.race?.Flow?.SessionStartedAt} seen {F("seenSession")} camUntil {F("cameraHintUntil")} roamUntil {F("roamHintUntil")} now {Time.unscaledTime} state {h.race?.Flow?.State} views {CameraViews.Current} trailer {TrailerMode.Active} canvas {h.GetComponent<Canvas>()?.enabled}");}
   yield return new WaitForSecondsRealtime(.5f);}}
 IEnumerator Hud(){yield return EnterRoam("StreetLoopGreybox","moto");var hud=FindAnyObjectByType<RaceHud>();var mini=RacingMiniMap.Instance;var act=hud?typeof(RaceHud).GetField("activities",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(hud) as UnityEngine.UI.Text:null;
  var speedo=hud.transform.Find("Speedometer") as RectTransform;var miniPanel=mini.transform.parent.parent as RectTransform;
  // just after Free Roam begins: clock, camera hint and the menu hint; no standing text block
  yield return new WaitForSecondsRealtime(.6f);yield return Late(()=>Shot4k("hud-roam-start"));
  Check(hud.ClockVisible&&hud.CameraHintVisible&&act.text.Contains("Esc or Start"),$"Free Roam start: clock shown {hud.ClockVisible}, camera hint '{hud.CameraHintText}' {hud.CameraHintVisible}, text block '{act.text.Replace("\n"," / ")}'");
  // later, normal driving: only the clock (and the minimap and speedometer)
  {float tw=Time.realtimeSinceStartup;while(act.text.Length>0&&Time.realtimeSinceStartup-tw<60)yield return null;Note($"start hints cleared after {Time.realtimeSinceStartup-tw:F1} s of editor time");}
  foreach(var (cond,t,w) in new[]{("day",TimeOfDay.Day,Weather.Clear),("night",TimeOfDay.Night,Weather.Clear),("snow",TimeOfDay.Day,Weather.Snow),("dawn",TimeOfDay.Dawn,Weather.Clear),("dusk",TimeOfDay.Dusk,Weather.Rain)}){WorldLook.Current.Pin(LookPresets.Compose(t,w));yield return new WaitForSecondsRealtime(t==TimeOfDay.Night?1.2f:.5f);yield return Late(()=>Shot4k("hud-roam-normal-"+cond));}
  WorldLook.Current.Pin(null);yield return null;
  var clock=ScreenRect(hud.ClockRect);var mm=ScreenRect(miniPanel);var sp=ScreenRect(speedo);
  Check(hud.ClockVisible&&string.IsNullOrEmpty(act.text)&&!hud.CameraHintVisible&&mini.Shown&&!clock.Overlaps(mm)&&!clock.Overlaps(sp),$"normal Free Roam driving: clock '{hud.ClockRect.GetComponentsInChildren<UnityEngine.UI.Text>()[0].text} {hud.ClockRect.GetComponentsInChildren<UnityEngine.UI.Text>()[1].text}', other text '{act.text}', minimap shown {mini.Shown}; clock {clock} clear of minimap {mm} and speedometer {sp}");
  // at an activity start, during an attempt
  var acts=flow.Activities;var site=acts.Sites.FirstOrDefault(s=>s.kind==ActivitySite.Kind.Smash)??acts.Sites.First(s=>s.kind!=ActivitySite.Kind.Speed);
  Ground(site.transform.position+site.transform.forward*-3,site.transform.position.y+20,null,out var sg);Car.enabled=false;Put(Car,sg.point+Vector3.up*.6f,Quaternion.LookRotation(Vector3.ProjectOnPlane(site.forward.sqrMagnitude>.1f?site.forward:Vector3.forward,Vector3.up)));yield return new WaitForSeconds(.4f);
  yield return Late(()=>Shot4k("hud-roam-activity-start"));Check(act.text.StartsWith(site.title)&&acts.AtSite==site,$"at {site.title}: '{act.text.Replace("\n"," / ")}'");
  Check(acts.Selected==site,"being at the site selected it for pause menu > Activities: "+acts.Selected?.title);acts.BeginAttempt();yield return new WaitForSeconds(3.5f);yield return Late(()=>Shot4k("hud-roam-attempt"));Check(acts.AttemptActive&&act.text.Contains(site.title)&&act.text.Contains("s /"),$"during the attempt: '{act.text.Replace("\n"," / ")}'");
  acts.Cancel();Car.enabled=true;
  // an acorn: drive the vehicle through the nearest acorn site
  var col=race.GetComponent<ExplorationCollection>();var acorn=col.sites.Where(s=>!col.Summary.Contains(s.id)&&Vector3.Distance(s.position,Car.Body.position)>80&&!acts.Sites.Any(a=>Vector3.Distance(a.transform.position,s.position)<a.radius+5)).OrderBy(s=>Vector3.Distance(s.position,Car.Body.position)).First();int before=col.Found;
  Car.enabled=false;Put(Car,acorn.position+Vector3.forward*-3+Vector3.up*.5f,Quaternion.identity);yield return new WaitForFixedUpdate();yield return null;yield return null;
  string seen="";for(int i=0;i<12&&col.Found==before;i++){var p=acorn.position+Vector3.forward*(-3+i*.5f)+Vector3.up*.5f;Car.Body.position=p;Car.transform.position=p;Physics.SyncTransforms();yield return new WaitForFixedUpdate();yield return null;}
  yield return Late(()=>{seen=act.text;Shot4k("hud-roam-acorn");});
  Note($"acorn debug: found {col.Found}, hud '{col.Hud}', until {typeof(ExplorationCollection).GetField("feedbackUntil",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(col)} time {Time.time}, car {V(Car.Body.position)} acorn {V(acorn.position)}");
  Car.enabled=true;Note($"acorn debug 2: hud '{col.Hud}' text '{act.text}' car {V(Car.Body.position)}");
  Check(col.Found==before+1&&seen.Contains($"WOODLAND ACORNS {col.Found}/"),$"acorn collected ({before} -> {col.Found}): '"+seen.Replace("\n"," / ")+"'");
  yield return new WaitForSeconds(6f);Check(!act.text.Contains("ACORN"),$"a few seconds later the acorn line is gone: '{act.text.Replace("\n"," / ")}'");
  // minimap toggled with the controller's B, saved
  bool was=flow.Save.Settings.roamMinimapHidden;yield return PadPress(GamepadButton.East);yield return null;yield return null;
  var json=File.ReadAllText(Directory.GetFiles(saveDir,"*settings*.json",SearchOption.AllDirectories).FirstOrDefault()??Directory.GetFiles(saveDir,"*.json",SearchOption.AllDirectories).First());
  yield return Late(()=>Shot4k("hud-roam-minimap-off"));
  Check(!was&&flow.Save.Settings.roamMinimapHidden&&!mini.Shown&&json.Contains("\"roamMinimapHidden\": true"),$"B turned the minimap off ({was} -> {flow.Save.Settings.roamMinimapHidden}, shown {mini.Shown}); saved in the settings file {json.Contains("\"roamMinimapHidden\": true")}");
  yield return PadPress(GamepadButton.East);yield return null;yield return null;Check(!flow.Save.Settings.roamMinimapHidden&&mini.Shown,$"B again: minimap back on ({mini.Shown})");
  var keys=mini.ToggleAction.bindings.Select(b=>b.path).ToArray();Check(keys.Contains("<Keyboard>/j")&&keys.Contains("<Gamepad>/buttonEast"),"minimap toggle bindings: "+string.Join(", ",keys));
  // the waypoint on the minimap
  var map=flow.GetComponent<ExplorationMap>();typeof(ExplorationMap).GetProperty("Waypoint").SetValue(map,(Vector3?)(Car.Body.position+Car.transform.forward*60));yield return new WaitForSecondsRealtime(.3f);yield return Late(()=>Shot4k("hud-roam-minimap-waypoint"));typeof(ExplorationMap).GetProperty("Waypoint").SetValue(map,null);
  // camera hint on a view change
  yield return PadPress(GamepadButton.West);yield return new WaitForSecondsRealtime(.3f);yield return Late(()=>Shot4k("hud-roam-camera-hint"));
  Check(hud.CameraHintVisible&&hud.CameraHintText.Contains(CameraViews.Current.PlayerViewName),$"view changed with X: hint '{hud.CameraHintText}'");CameraViews.Current.SetPlayerView(CameraViews.View.Chase);
  // pause menu: Camera view row cycles; Trailer / Photo Mode row with its key
  flow.Pause();yield return null;yield return null;yield return Late(()=>Shot4k("menu-roam-pause"));
  var rows=FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None).Where(b=>b.gameObject.activeInHierarchy).ToList();var camRow=rows.FirstOrDefault(b=>b.name=="camera-view");var trailerRow=rows.FirstOrDefault(b=>b.name=="trailer");
  string camLabel=camRow?camRow.GetComponentInChildren<UnityEngine.UI.Text>().text:"-";if(camRow)camRow.onClick.Invoke();yield return null;
  Check(camRow&&trailerRow&&trailerRow.GetComponentInChildren<UnityEngine.UI.Text>().text.Contains("TRAILER / PHOTO MODE")&&trailerRow.GetComponentInChildren<UnityEngine.UI.Text>().text.Contains("F8")&&CameraViews.Current.PlayerView==CameraViews.View.FarChase,$"Free Roam pause menu: '{camLabel}' (pressed: view now {CameraViews.Current.PlayerViewName}), '{trailerRow?.GetComponentInChildren<UnityEngine.UI.Text>().text}'");
  CameraViews.Current.SetPlayerView(CameraViews.View.Chase);flow.Resume();yield return new WaitForSecondsRealtime(.5f);
  // Trailer Mode: the panel on entry, clickable, controller focus, hidden with H (L3), the "show controls" line
  var mode=TrailerMode.Instance;mode.Begin();yield return new WaitForSecondsRealtime(.4f);yield return Late(()=>Shot4k("trailer-panel-shown"));
  var hudCanvas=hud.GetComponent<Canvas>();
  Check(mode.PanelVisible&&!hudCanvas.enabled&&Cursor.visible,$"Trailer Mode on: controls panel shown {mode.PanelVisible}, HUD drawn {hudCanvas.enabled}, pointer visible {Cursor.visible}; entries {mode.PanelEntries.Count}: {string.Join(" | ",mode.PanelEntries.Where(e=>e.button.gameObject.activeInHierarchy).Select(e=>e.label()))}");
  var side=mode.PanelEntries.First(e=>e.button.name=="Camera 3");side.button.onClick.Invoke();yield return null;yield return null;Check(CameraViews.Current.TrailerCamera==CameraViews.Shot.Side,"clicking '3 Side tracking' selects it: "+CameraViews.Current.TrailerCamera);
  var weather=mode.PanelEntries.First(e=>e.button.name=="Weather");string w0=mode.WeatherLabel;weather.button.onClick.Invoke();yield return null;Check(mode.WeatherLabel!=w0,$"clicking Weather: {w0} -> {mode.WeatherLabel}");
  yield return PadPress(GamepadButton.East);yield return null;var sel=UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;Check(mode.PanelFocused&&sel&&sel.name=="Camera 1",$"controller B gives the panel the focus: {mode.PanelFocused}, selected {sel?.name}");
  yield return PadPress(GamepadButton.South);yield return null;Check(CameraViews.Current.TrailerCamera==CameraViews.Shot.Chase,"controller A on the selected entry: camera "+CameraViews.Current.TrailerCamera);
  yield return Late(()=>Shot4k("trailer-panel-controller"));
  yield return PadPress(GamepadButton.East);Check(!mode.PanelFocused,"B again: back to driving");
  mode.Screenshot();yield return null;yield return null;yield return new WaitForSecondsRealtime(1.5f);var png=mode.LastScreenshot;
  if(png!=null&&File.Exists(png))File.Copy(png,output+"/trailer-screenshot-with-panel-open.png",true);Note("P / F12 screenshot taken while the panel was open: "+png+" (copied as trailer-screenshot-with-panel-open.png; the camera alone is rendered)");
  yield return PadPress(GamepadButton.LeftStick);yield return new WaitForSecondsRealtime(3f);yield return Late(()=>Shot4k("trailer-hidden-clean"));
  Check(!mode.PanelVisible&&!hudCanvas.enabled&&!mode.ShowControlsLineVisible,$"L3 (H): panel and HUD hidden: panel {mode.PanelVisible}, HUD {hudCanvas.enabled}, line {mode.ShowControlsLineVisible}");
  yield return PadPress(GamepadButton.South);bool lineSeen=mode.ShowControlsLineVisible;yield return Late(()=>Shot4k("trailer-hidden-line"));Check(lineSeen,"a key while hidden: the 'show controls' line appears");
  {float tw=Time.realtimeSinceStartup;while(mode.ShowControlsLineVisible&&Time.realtimeSinceStartup-tw<30)yield return null;}Check(!mode.ShowControlsLineVisible,"and fades again");
  yield return PadPress(GamepadButton.LeftStick);yield return new WaitForSecondsRealtime(.3f);Check(mode.PanelVisible&&hudCanvas.enabled,"L3 again: HUD and panel back");
  mode.End();yield return null;yield return null;Check(!TrailerMode.Active&&!mode.PanelVisible&&hudCanvas.enabled,"Trailer Mode off: panel gone, HUD back");
  // race HUD: lap box, race minimap key, no clock, camera hint at the start only
  yield return Load("StreetLoopGreybox");yield return Menu();race.opponents=true;race.traffic=false;race.laps=1;flow.StartRace();float t0=Time.realtimeSinceStartup;while(flow.State!=RaceFlow.Stage.Racing&&Time.realtimeSinceStartup-t0<30){AudioListener.volume=0;yield return null;}
  hud=FindAnyObjectByType<RaceHud>();yield return new WaitForSecondsRealtime(.5f);yield return Late(()=>Shot4k("hud-race-start"));
  Check(!hud.ClockVisible&&hud.CameraHintVisible,$"race start: no clock {!hud.ClockVisible}, camera hint '{hud.CameraHintText}'");
  {float tw=Time.realtimeSinceStartup;while(hud.CameraHintVisible&&Time.realtimeSinceStartup-tw<30)yield return null;}yield return Late(()=>Shot4k("hud-race-later"));
  var mk=RacingMiniMap.Instance.transform.parent.parent.GetComponentInChildren<UnityEngine.UI.Text>().text;Check(!hud.CameraHintVisible&&mk==RacingMiniMap.RaceKey&&RacingMiniMap.Instance.Shown,$"race later: hint gone, race minimap '{mk}' shown {RacingMiniMap.Instance.Shown}");
  flow.Pause();yield return null;yield return null;yield return Late(()=>Shot4k("menu-race-pause"));
  rows=FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None).Where(b=>b.gameObject.activeInHierarchy).ToList();
  Check(rows.Any(b=>b.name=="camera-view")&&rows.Any(b=>b.name=="trailer"&&b.GetComponentInChildren<UnityEngine.UI.Text>().text.Contains("TRAILER / PHOTO MODE")),"race pause menu: Camera view and Trailer / Photo Mode rows");
  yield return Menu();yield return null;yield return Late(()=>Shot4k("menu-main"));rows=FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None).Where(b=>b.gameObject.activeInHierarchy).ToList();
  Check(rows.Any(b=>b.name=="trailer"),"main menu: Trailer / Photo Mode row");
  flow.OpenSettings();yield return null;var menus=FindAnyObjectByType<RaceMenus>();typeof(RaceMenus).GetField("page",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(menus,"settings-controls");menus.Show();yield return null;yield return null;
  var labels=FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None).Where(b=>b.gameObject.activeInHierarchy).Select(b=>b.GetComponentInChildren<UnityEngine.UI.Text>().text).ToList();yield return Late(()=>Shot4k("menu-settings-controls"));
  Check(labels.Any(l=>l.StartsWith("Camera / Change view"))&&labels.Any(l=>l.StartsWith("Minimap / On or off"))&&labels.Any(l=>l.StartsWith("Trailer / Photo Mode")),"Settings > Controls lists: "+string.Join(" | ",labels.Where(l=>l.Contains("Camera")||l.Contains("Minimap")||l.Contains("Trailer"))));
  flow.CloseSettings();yield return null;}

 // before / after views of the reported spots (same cameras as the survey)
 IEnumerator Spots(string tag){yield return EnterRoam("StreetLoopGreybox","moto");yield return new WaitForSeconds(1);
  var spots=new (string name,Vector3 p)[]{("driveway trees",new(454,0,-29.9f)),("house 576",new(576.5f,0,-414.3f)),("house 491",new(491.5f,0,-589.9f)),("house 396",new(396.7f,0,-501.5f)),("house 256",new(256.7f,0,-413.8f))};
  var cam=Camera.main;var chase=FindAnyObjectByType<ChaseCamera>();if(chase)chase.enabled=false;var cv=CameraViews.Current;if(cv)cv.enabled=false;var hud=FindObjectsByType<Canvas>(FindObjectsSortMode.None);foreach(var c in hud)c.enabled=false;
  foreach(var (name,p) in spots){Ground(p,800,null,out var g);foreach(var off in new[]{new Vector3(-14,7,-14),new Vector3(14,7,14),new Vector3(-16,3,10),new Vector3(16,3,-10)}){var eye=g.point+off;
    foreach(bool fresh in new[]{true,false}){Scenery.Set(fresh);yield return null;yield return null;yield return Late(()=>{cam.transform.position=eye;cam.transform.LookAt(g.point+Vector3.up*2);Shot($"{tag}-{name.Replace(' ','-')}-{off.x:F0}_{off.z:F0}-{(fresh?"new":"classic")}");});}}}
  Scenery.Set(true);foreach(var c in hud)if(c)c.enabled=true;if(chase)chase.enabled=true;if(cv)cv.enabled=true;}
}
}
#endif
