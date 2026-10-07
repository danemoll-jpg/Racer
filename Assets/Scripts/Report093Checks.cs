#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Racer {
// 0.93 targeted checks, added to the 0.80 runner (muted; the editor starts on a copy of Dan's save).
//  corridor93:Scene:branch:margin   the branch every metre (point, half width) and every scenery piece (renderer or collider,
//                                   not ground) within half width + margin of its centre line: s, lateral, solid or not
public sealed partial class Report080Checks {
 IEnumerator Run093(string[] a)=>a[0] switch{"corridor93"=>Corridor093(a[1],a[2],F(a[3])),"at93"=>At093(a[1],a[2]),"flights93"=>Flights093(a[1],a[2],a[3],a[4]),"reset93"=>Reset093(a[1],a[2],a[3],F(a[4])),"resetpoints93"=>ResetPoints093(a[1]),"landshot93"=>LandShot093(a[1],a[2],a[3]),"places93"=>Places093(),"heights93"=>Heights093(),_=>Walk093(a)??Run092(a)};
 static WoodlandRoute Branch093(string title)=>FindObjectsByType<WoodlandRoute>(FindObjectsSortMode.None).FirstOrDefault(b=>b.title==title);
 // scenery near a route: everything with a renderer or collider that is not ground, road, route marking or a vehicle
 static bool Scenery093(Transform t){var n=Path092(t).ToLowerInvariant();return !t.GetComponentInParent<ArcadeVehicle>()&&!(n.Contains("terrain")||n.Contains("ground")||n.Contains("road")||n.Contains("gate")||n.Contains("arrow")||n.Contains("route")||n.Contains("trail surface")||n.Contains("grid")||n.Contains("water"));}
 IEnumerator Corridor093(string scene,string title,float margin){
  yield return Load(scene);yield return Menu();var wr=Branch093(title);if(!wr){Note("no branch "+title);yield break;}wr.Initialize();
  var rows=new List<string>{$"{title}: length {wr.Length:F1} m, half width {wr.halfWidth}, entry road {wr.entryRoad:F1}, exit road {wr.exitRoad:F1}"};
  for(float s=0;s<=wr.Length;s+=1){var p=wr.At(s,out var f);rows.Add($"s {s:F0}: {p.x:F2}, {p.y:F2}, {p.z:F2} heading {Mathf.Atan2(f.x,f.z)*Mathf.Rad2Deg:F0}");}
  var found=new Dictionary<string,string>();
  foreach(var ren in FindObjectsByType<Renderer>(FindObjectsSortMode.None)){if(!ren.enabled||ren.bounds.size.magnitude>60||!Scenery093(ren.transform))continue;Near093(wr,ren.transform,ren.bounds,margin,found,"visual");}
  foreach(var col in FindObjectsByType<Collider>(FindObjectsSortMode.None)){if(!col.enabled||col.isTrigger||col.bounds.size.magnitude>60||!Scenery093(col.transform))continue;Near093(wr,col.transform,col.bounds,margin,found,"SOLID");}
  rows.Add($"---- {found.Count} scenery pieces within half width + {margin} m");rows.AddRange(found.Values.OrderBy(x=>x));
  File.WriteAllLines($"{output}/corridor-{scene}-{title.Replace(' ','_')}.txt",rows);Note($"{scene} {title}: {found.Count} scenery pieces within half width + {margin} m (corridor-{scene}-{title.Replace(' ','_')}.txt)");yield return Menu();}
 // at93:Scene:x,y,z  every renderer / collider / terrain whose bounds contain the point (any size), with mesh name
 IEnumerator At093(string scene,string point){
  yield return Load(scene);yield return Menu();var v=point.Split(',').Select(F).ToArray();var c=new Vector3(v[0],v[1],v[2]);var list=new List<string>();
  foreach(var ren in FindObjectsByType<Renderer>(FindObjectsSortMode.None))if(ren.enabled&&ren.bounds.Contains(c))list.Add($"renderer {Path092(ren.transform)} mesh {ren.GetComponent<MeshFilter>()?.sharedMesh?.name} bounds {ren.bounds.min:F1}..{ren.bounds.max:F1} materials {string.Join(",",ren.sharedMaterials.Where(m=>m).Select(m=>m.name))}");
  foreach(var col in FindObjectsByType<Collider>(FindObjectsSortMode.None))if(col.enabled&&col.bounds.Contains(c))list.Add($"collider {Path092(col.transform)} [{col.GetType().Name}] bounds {col.bounds.min:F1}..{col.bounds.max:F1}");
  foreach(var t in Terrain.activeTerrains){var d=t.terrainData;list.Add($"terrain {t.name} trees {d.treeInstanceCount} prototypes {string.Join(",",d.treePrototypes.Select(x=>x.prefab?x.prefab.name:"?"))} details {d.detailPrototypes.Length} drawTrees {t.drawTreesAndFoliage}");}
  list.Add("instanced drawers: "+string.Join(", ",FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).Select(m=>m.GetType().Name).Where(n=>n.Contains("Foliage")||n.Contains("Vegetation")||n.Contains("Instanc")||n.Contains("Bush")||n.Contains("Shrub")||n.Contains("Scatter")).Distinct()));
  File.WriteAllLines($"{output}/at-{scene}-{v[0]:F0}-{v[2]:F0}.txt",list);Note($"{scene} at {c}: {list.Count} (at-{scene}-{v[0]:F0}-{v[2]:F0}.txt)");yield return Menu();}
 //  flights93:Scene:branch:profiles:speeds   the branch ridden from its start at each speed on each vehicle (simple follower on the
 //                                   centre line): takeoff, landing, run-out, and every tree / bush crown the vehicle passed through
 //  reset93:Scene:branch:x,y,z,heading:untilS  ride the branch to untilS, stop the vehicle at the point, press reset: where it ends
 //  resetpoints93:Scene              every shortcut of the course every 4 m: crowns or solid scenery on the centre line or within
 //                                   5 m ahead (where a reset puts the vehicle)
 //  landshot93:Scene:branch:label    shots along the landing (eye 2 m above the ground, looking along the branch)
 static UnityEngine.SceneManagement.Scene bushScene093;static List<SceneryTrees.Placement> bushes093;
 // the 0.43 shortcut undergrowth (collider-free bushes, ShortcutUndergrowth): each connected piece of its mesh as one bush
 static List<SceneryTrees.Placement> Bushes093(){var sc=UnityEngine.SceneManagement.SceneManager.GetActiveScene();if(bushes093!=null&&bushScene093==sc)return bushes093;bushScene093=sc;bushes093=new();
  foreach(var u in FindObjectsByType<ShortcutUndergrowth>(FindObjectsSortMode.None)){var mf=u.GetComponent<MeshFilter>();var r=u.GetComponent<MeshRenderer>();if(!mf||!mf.sharedMesh||!r||!r.enabled)continue;var m=mf.sharedMesh;var v=m.vertices;var t=m.triangles;var w=u.transform.localToWorldMatrix;
   var parent=Enumerable.Range(0,v.Length).ToArray();int Find(int i){while(parent[i]!=i){parent[i]=parent[parent[i]];i=parent[i];}return i;}
   for(int i=0;i<t.Length;i+=3){int a=Find(t[i]),b=Find(t[i+1]),c=Find(t[i+2]);parent[b]=a;parent[Find(c)]=a;}
   var used=new HashSet<int>(t);
   foreach(var g in Enumerable.Range(0,v.Length).Where(used.Contains).GroupBy(Find)){var pts=g.Select(i=>w.MultiplyPoint3x4(v[i])).ToList();var c=pts.Aggregate(Vector3.zero,(x,y)=>x+y)/pts.Count;float lo=pts.Min(q=>q.y),hi=pts.Max(q=>q.y);
    bushes093.Add(new SceneryTrees.Placement{bottom=new Vector3(c.x,lo,c.z),radius=pts.Max(q=>new Vector2(q.x-c.x,q.z-c.z).magnitude),height=hi-lo,source=u.name});}}
  return bushes093;}
 static List<SceneryTrees.Placement> Crowns093()=>(SceneryWorld.Current&&SceneryWorld.Current.Trees?SceneryWorld.Current.Trees.Placements:new List<SceneryTrees.Placement>()).Concat(Bushes093()).ToList();
 static string Crown093(SceneryTrees.Placement c)=>$"{c.source} at {c.bottom.x:F1},{c.bottom.y:F1},{c.bottom.z:F1} r {c.radius:F1} h {c.height:F1}";
 // the crowns a point is inside (with a margin): XZ within radius + margin, height from its bottom to its top
 static IEnumerable<SceneryTrees.Placement> Inside093(Vector3 p,float margin)=>Crowns093().Where(c=>{var d=p-c.bottom;d.y=0;return d.magnitude<c.radius+margin&&p.y>c.bottom.y-.5f&&p.y<c.bottom.y+c.height+.5f;});
 IEnumerator StartOn093(string scene,string profile){
  yield return Load(scene);yield return Menu();flow.Save.Settings.unlockEverything=true;Campaign.Testing=true;flow.OpenGarage();flow.SelectVehicle(profile);flow.CloseGarage();race.opponents=false;race.traffic=false;race.laps=1;flow.StartRace();
  float tr=Time.realtimeSinceStartup;while(flow.State!=RaceFlow.Stage.Racing&&Time.realtimeSinceStartup-tr<40){AudioListener.volume=0;yield return null;}
  yield return new WaitForSeconds(3.5f);}
 IEnumerator Flights093(string scene,string title,string profiles,string speeds){
  var rows=new List<string>();var corridor=new List<string>();
  foreach(var profile in profiles.Split(',')){
   yield return StartOn093(scene,profile);var wr=Branch093(title);if(!wr){Note("no branch "+title);yield break;}wr.Initialize();
   foreach(var sp in speeds.Split(',')){float speed=F(sp);string tag=$"{Scene}-{profile}-{title.Replace(' ','_')}-{speed:F0}";
    yield return Ride(wr,0,wr.Length,speed,tag);var r=LastRide;
    var lines=File.ReadAllLines($"{output}/ride-{tag}.csv").Skip(1).Select(l=>l.Split(',').Select(F).ToArray()).ToList();
    // the longest airborne stretch: takeoff = its first row, landing = the first grounded row after it
    int bestStart=-1,bestLen=0;for(int i=0;i<lines.Count;){if(lines[i][5]==0){int j=i;while(j<lines.Count&&lines[j][5]==0)j++;if(j-i>bestLen){bestLen=j-i;bestStart=i;}i=j;}else i++;}
    if(bestStart<0){rows.Add($"{profile} {speed} m/s: no flight; {(r.reached?"reached the rejoin":"NOT reached")}");Note(rows[^1]);continue;}
    var to=lines[bestStart];var land=lines[Mathf.Min(lines.Count-1,bestStart+bestLen)];
    float maxLat=lines.Skip(bestStart+bestLen).Select(l=>Mathf.Abs(l[9])).DefaultIfEmpty(0).Max();
    var touched=new HashSet<string>();
    foreach(var l in lines.Skip(bestStart)){var p=new Vector3(l[1],l[2],l[3]);foreach(var c in Inside093(p,.8f))touched.Add(Crown093(c)+$" (at s {l[8]:F0})");}
    rows.Add($"{profile} {speed} m/s: takeoff s {to[8]:F1} ({to[1]:F1},{to[2]:F1},{to[3]:F1}) at {to[4]:F1} m/s, air {bestLen*Time.fixedDeltaTime:F2} s, landing s {land[8]:F1} lat {land[9]:+0.0;-0.0} ({land[1]:F1},{land[2]:F1},{land[3]:F1}), run-out to the rejoin within {maxLat:F1} m of the centre, {(r.reached?"reached the rejoin":"NOT reached")}, resets {r.resets}, min up {r.minUp:F2}; crowns passed through: {(touched.Count==0?"none":string.Join(" | ",touched))}");
    Note(rows[^1]);}
   yield return Menu();}
  // what stands in the corridor from the takeoff on: every crown within half width + 3 m of the centre line
  {var wr=Branch093(title);wr.Initialize();foreach(var c in Crowns093()){float s=wr.Project(c.bottom,out _);var q=wr.At(s,out _);var d=c.bottom-q;d.y=0;float edge=d.magnitude-c.radius;if(s>30&&edge<wr.halfWidth+3&&Mathf.Abs(c.bottom.y-q.y)<4)corridor.Add($"s {s,5:F0} centre {d.magnitude:F1} m, crown edge {edge:F1} m: {Crown093(c)}");}}
  rows.Add($"---- crowns within half width + 3 m of the centre line from s 30: {corridor.Count}");rows.AddRange(corridor.OrderBy(x=>x));
  File.WriteAllLines($"{output}/flights-{scene}-{title.Replace(' ','_')}.txt",rows);Note($"{scene} {title}: corridor crowns {corridor.Count} (flights-{scene}-{title.Replace(' ','_')}.txt)");}
 IEnumerator Reset093(string scene,string title,string at,float untilS){
  yield return StartOn093(scene,"moto");var wr=Branch093(title);wr.Initialize();yield return Ride(wr,0,untilS,30,$"reset-approach-{Scene}");
  var v=at.Split(',').Select(F).ToArray();var car=race.vehicle;var pos=new Vector3(v[0],v[1],v[2]);var rot=Quaternion.Euler(0,v[3],0);
  car.Body.position=pos;car.Body.rotation=rot;car.transform.SetPositionAndRotation(pos,rot);car.Body.linearVelocity=car.Body.angularVelocity=Vector3.zero;
  for(int i=0;i<5;i++)yield return new WaitForFixedUpdate();
  // as when Dan rode in: the race has him on the branch (a teleport clears it, so set after placing)
  race.Racers[0].Branch.Begin(wr);race.Racers[0].Branch.Advance(pos,pos,rot*Vector3.forward);
  for(int i=0;i<25;i++)yield return new WaitForFixedUpdate();
  Note($"stopped at {V(car.Body.position)}: route {race.Racers[0].Branch.Route?.title??"main"}; inside crowns: {string.Join(" | ",Inside093(car.Body.position+Vector3.up*.6f,.3f).Select(Crown093))}");
  var resp=car.GetComponent<VehicleRespawn>();resp.ResetVehicle();float t0=Time.time;while(resp.Pending&&Time.time-t0<5)yield return new WaitForFixedUpdate();
  for(int i=0;i<20;i++)yield return new WaitForFixedUpdate();
  var p=car.Body.position;var f=car.transform.forward;float s=wr.Project(p,out float lat);var block=new List<string>();
  for(float d=0;d<=5;d+=.5f)foreach(var c in Inside093(p+f*d+Vector3.up*.6f,.9f))block.Add(Crown093(c));
  var solid=Physics.OverlapBox(p+f*3+Vector3.up*1.2f,new Vector3(1,.6f,2.5f),car.transform.rotation,~0,QueryTriggerInteraction.Ignore).Where(c=>!c.GetComponentInParent<ArcadeVehicle>()&&Scenery093(c.transform)).Select(c=>c.name).Distinct().ToList();
  Check(block.Count==0&&solid.Count==0,$"reset from Dan's position: now at {V(p)} heading {car.transform.eulerAngles.y:F0}, route {race.Racers[0].Branch.Route?.title??"main"} s {s:F1} lat {lat:+0.0;-0.0} ({resp.LastRecovery}); crowns on it or within 5 m ahead: {(block.Count==0?"none":string.Join(" | ",block.Distinct()))}; solid within 5 m ahead: {(solid.Count==0?"none":string.Join(", ",solid))}");
  yield return Late(()=>Shot($"B-reset-{Scene}"));
  yield return Menu();}
 IEnumerator ResetPoints093(string scene){
  yield return Load(scene);yield return Menu();var rows=new List<string>();int bad=0;
  foreach(var wr in race.Branches){wr.Initialize();int n=0;
   for(float s=0;s<=wr.Length;s+=4){var p=wr.At(s,out var f);f.y=0;f.Normalize();var hits=new HashSet<string>();
    for(float d=0;d<=5;d+=.5f){var q=wr.At(s+d,out _);foreach(var c in Inside093(q+Vector3.up*.6f,.9f))hits.Add(Crown093(c));}
    var solid=Physics.OverlapBox(p+f*2.5f+Vector3.up*1.3f,new Vector3(.9f,.6f,2.5f),Quaternion.LookRotation(f),~0,QueryTriggerInteraction.Ignore).Where(c=>!c.GetComponentInParent<ArcadeVehicle>()&&Scenery093(c.transform)&&!Surface093(wr,s,c)).Select(c=>Path092(c.transform)).Distinct().ToList();
    if(hits.Count+solid.Count>0){n++;rows.Add($"{wr.title} s {s:F0} ({p.x:F1},{p.y:F1},{p.z:F1}): {string.Join(" | ",hits.Concat(solid.Select(x=>"SOLID "+x)))}");}}
   bad+=n;Note($"{Scene} {wr.title}: {Mathf.CeilToInt(wr.Length/4)+1} reset points, {n} with crowns or solid scenery on them or within 5 m ahead");}
  File.WriteAllLines($"{output}/resetpoints-{Scene}.txt",rows);Note($"{Scene}: {bad} reset points blocked (resetpoints-{Scene}.txt)");yield return Menu();}
 static bool Surface093(WoodlandRoute wr,float s,Collider c){for(float d=-1;d<=6;d+=.5f){var q=wr.At(s+d,out _);if(Physics.Raycast(q+Vector3.up*4,Vector3.down,out var h,8,~0,QueryTriggerInteraction.Ignore)&&h.collider==c)return true;}return false;}
 // places93 (Free Roam world): the acorns by area, the map's destinations, every road / trail (name, length, ends and middle), and
 // named objects that can anchor directions (houses, the lake, the summit), for the acorn areas' regions and directions
 IEnumerator Places093(){
  yield return EnterScene(RaceFlow.RoamScene);yield return new WaitForSeconds(1);var rows=new List<string>();
  var col=FindAnyObjectByType<ExplorationCollection>();foreach(var x in col.sites.OrderBy(x=>x.approach))rows.Add($"acorn [{x.approach}] {x.id} '{x.title}' at {x.position.x:F0},{x.position.y:F0},{x.position.z:F0}");
  var map=FindAnyObjectByType<ExplorationMap>();foreach(var d in map.destinations)rows.Add($"destination {d.id} '{d.title}' at {d.position.x:F0},{d.position.y:F0},{d.position.z:F0}");
  foreach(var r in FindObjectsByType<RaceRoad>(FindObjectsSortMode.None)){if(r.points==null||r.points.Length<2)continue;var pts=r.points;rows.Add($"road '{Path092(r.transform)}' trail {r.forestTrail} points {pts.Length} from {pts[0].x:F0},{pts[0].z:F0} mid {pts[pts.Length/2].x:F0},{pts[pts.Length/2].z:F0} to {pts[^1].x:F0},{pts[^1].z:F0} | {string.Join(" ",pts.Where((q,i)=>i%Mathf.Max(1,pts.Length/12)==0).Select(q=>$"({q.x:F0},{q.z:F0})"))}");}
  foreach(var t in FindObjectsByType<Transform>(FindObjectsSortMode.None)){var n=t.name.ToLowerInvariant();if((n.Contains("kyle")||n.Contains("dan's")||n.Contains("moll")||n.Contains("lake")||n.Contains("summit")||n.Contains("cherokee")||n.Contains("trickum")||n.Contains("hwy")||n.Contains("highway 92")||n.Contains("mountain"))&&t.parent!=null&&!n.Contains("acorn"))rows.Add($"object '{Path092(t)}' at {t.position.x:F0},{t.position.y:F0},{t.position.z:F0}");}
  var vis=Resources.Load<WorldMapVisual>("WorldMaps/PermanentWorld");rows.Add($"map visual bounds {vis?.bounds} image {vis?.image?.width}x{vis?.image?.height}");
  File.WriteAllLines($"{output}/places.txt",rows);Note($"places: {rows.Count} lines (places.txt)");}
 // heights93 (Free Roam world): the highest ground (trees left out) east of x 500, every 10 m: the mountain summit
 IEnumerator Heights093(){
  yield return EnterScene(RaceFlow.RoamScene);yield return new WaitForSeconds(1);var top=new List<Vector3>();
  for(float x=500;x<=1500;x+=10)for(float z=-700;z<=700;z+=10)foreach(var h in Physics.RaycastAll(new Vector3(x,800,z),Vector3.down,1600,~0,QueryTriggerInteraction.Ignore).OrderBy(h=>h.distance)){var n=h.collider.name.ToLowerInvariant();if(n.Contains("trunk")||n.Contains("tree")||h.collider.attachedRigidbody)continue;top.Add(h.point);break;}
  Note("highest ground: "+string.Join(" | ",top.OrderByDescending(p=>p.y).Take(8).Select(p=>$"{p.x:F0},{p.y:F0},{p.z:F0}")));}
 IEnumerator LandShot093(string scene,string title,string label){
  yield return StartOn093(scene,"moto");var wr=Branch093(title);wr.Initialize();
  var cam=Camera.main;var chase=FindAnyObjectByType<ChaseCamera>();if(chase)chase.enabled=false;var cv=CameraViews.Current;if(cv)cv.enabled=false;
  var hud=FindObjectsByType<Canvas>(FindObjectsSortMode.None);foreach(var c in hud)c.enabled=false;yield return new WaitForSeconds(1);
  foreach(float s0 in new[]{44f,56f}){var eye=wr.At(s0,out var f);f.y=0;float ground=Physics.Raycast(eye+Vector3.up*5,Vector3.down,out var h,20,~0,QueryTriggerInteraction.Ignore)?h.point.y:eye.y;eye.y=ground+2;
   yield return Late(()=>{cam.transform.SetPositionAndRotation(eye,Quaternion.LookRotation(f.normalized+Vector3.down*.08f));Shot($"B-landing-{label}-from-s{s0:F0}");});}
  foreach(var c in hud)if(c)c.enabled=true;if(chase)chase.enabled=true;if(cv)cv.enabled=true;yield return Menu();}
 static void Near093(WoodlandRoute wr,Transform t,Bounds b,float margin,Dictionary<string,string> found,string kind){
  float s=wr.Project(b.center,out _);var p=wr.At(s,out var f);f.y=0;f.Normalize();var right=Vector3.Cross(Vector3.up,f);
  var d=b.center-p;float lat=Vector3.Dot(d,right);float reach=Mathf.Max(b.extents.x,b.extents.z);
  if(Mathf.Abs(lat)-reach>wr.halfWidth+margin||s<=0.01f&&Vector3.Dot(d,f)<-reach||s>=wr.Length-.01f&&Vector3.Dot(d,f)>reach)return;
  var key=Path092(t)+"|"+kind;if(found.ContainsKey(key))return;
  found[key]=$"s {s,6:F1} lat {lat,6:+0.0;-0.0} reach {reach:F1} top {b.max.y-p.y:+0.0;-0.0} {kind} {Path092(t)} at {b.center.x:F1},{b.center.y:F1},{b.center.z:F1}";}
}
}
#endif
