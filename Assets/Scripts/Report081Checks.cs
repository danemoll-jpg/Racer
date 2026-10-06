#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Racer {
// 0.81 targeted checks, added to the 0.80 runner (same muted isolated save, same helpers): PROBE_CASES="case:args;...".
public sealed partial class Report080Checks {
 partial void More(string[] a,ref IEnumerator run){
  run=a[0] switch{"edge"=>Edge(a[1]),"overlap"=>Overlap(a[1],a[2]),"wall"=>Wall(a[1],a[2],a[3]),"vehicle"=>VehicleCheck(a[1]),"garage"=>GarageCheck(),"traffic"=>TrafficCheck(a[1]),"people"=>PeopleCheck(),_=>null};}

 // Part A/B: one new vehicle on one road (Free Roam from Street Loop): wheels on the ground, rider seated, headlights at
 // night, fist wave; a day and a night shot. vehicle:id
 IEnumerator VehicleCheck(string id){yield return EnterRoam("StreetLoopGreybox",id);yield return new WaitForSeconds(1.5f);
  var car=race.vehicle;var cfg=car.GetComponent<VehicleConfiguration>();var visual=car.transform.Find("Vehicle visual");
  var rider=visual?visual.GetComponentsInChildren<Transform>(true).FirstOrDefault(t=>t.name=="Rider"):null;int riderParts=rider?rider.GetComponentsInChildren<Renderer>().Length:0;
  var model=visual?visual.GetComponentsInChildren<Transform>(true).FirstOrDefault(t=>t.name.EndsWith(" model")):null;
  float worstGap=0;foreach(var r in visual.GetComponentsInChildren<Renderer>().Where(r=>r.name.StartsWith("Wheel"))){var b=r.bounds;if(Physics.Raycast(b.center+Vector3.up*2,Vector3.down,out var gh,6,~0,QueryTriggerInteraction.Ignore)&&!gh.collider.attachedRigidbody)worstGap=Mathf.Max(worstGap,Mathf.Abs(b.min.y-gh.point.y));}
  Check(cfg.profileId==id&&model&&riderParts>0&&car.GroundedWheels>=2&&worstGap<(cfg.Profile.Small?.12f:.45f),$"{id} ({cfg.Profile.Name}, {cfg.Profile.Class}): model {(model?model.name:"MISSING")}, rider parts {riderParts}, grounded wheels {car.GroundedWheels}, wheel bottom to ground {worstGap:F3} m, box {V(car.GetComponent<BoxCollider>().size)}");
  var g=car.GetComponent<RiderGestures>();int w0=g.Waves;bool waved=g.Wave();yield return new WaitForSeconds(.6f);float weight=g.Weight;
  yield return Late(()=>Shot($"vehicle-{id}-wave"));
  Check(waved&&g.Waves==w0+1&&weight>.5f,$"{id} fist wave: started {waved}, weight at 0.6 s {weight:F2}");
  yield return new WaitForSeconds(2.5f);
  var cam=Camera.main;var chase=FindAnyObjectByType<ChaseCamera>();if(chase)chase.enabled=false;var cv=CameraViews.Current;if(cv)cv.enabled=false;
  foreach(var light in new[]{"day","night"}){WorldLook.Current?.Pin(LookPresets.Compose(light=="night"?TimeOfDay.Night:TimeOfDay.Day,Weather.Clear));yield return new WaitForSeconds(1.2f);
   var t=car.transform;var eye=t.position+t.right*3.6f+t.forward*3.8f+Vector3.up*1.4f;yield return Late(()=>{cam.transform.position=eye;cam.transform.LookAt(t.position+Vector3.up*.5f);Shot($"vehicle-{id}-{light}");});
   if(light=="day"){var top=t.position+Vector3.up*6.5f-t.forward*1.5f;yield return Late(()=>{cam.transform.position=top;cam.transform.LookAt(t.position);Shot($"vehicle-{id}-above");});}
   if(light=="night"){var lamp=car.GetComponentsInChildren<Light>().FirstOrDefault(l=>l.name=="Headlight");Check(lamp&&lamp.enabled&&VehicleLights.Level>.5f,$"{id} night: headlight {(lamp&&lamp.enabled?"on":"off")}, lamp level {VehicleLights.Level:F2}");}}
  WorldLook.Current?.Pin(LookPresets.Compose(TimeOfDay.Day,Weather.Clear));if(chase)chase.enabled=true;if(cv)cv.enabled=true;}
 // Garage: every vehicle in the garage list with the rotating preview (one shot each). garage
 IEnumerator GarageCheck(){yield return Load("StreetLoopGreybox");yield return Menu();flow.OpenGarage();yield return new WaitForSecondsRealtime(.5f);
  var names=new List<string>();foreach(var p in race.EligibleVehicles){flow.SelectVehicle(p.Id);yield return new WaitForSecondsRealtime(.6f);names.Add(p.Name);var id=p.Id;yield return Late(()=>Shot($"garage-{id}"));}
  Check(names.Count==VehicleProfile.All.Length,$"garage lists {names.Count} vehicles: {string.Join(", ",names)}");flow.SelectVehicle("original");flow.CloseGarage();}
 // Part C: Street Loop with traffic, a view of one traffic car of each body by day or night. traffic:day|night
 IEnumerator TrafficCheck(string light){yield return Load("StreetLoopGreybox");yield return Menu();
  race.opponents=false;race.traffic=true;race.laps=1;flow.StartRace();float tr=Time.realtimeSinceStartup;while(flow.State!=RaceFlow.Stage.Racing&&Time.realtimeSinceStartup-tr<30){AudioListener.volume=0;yield return null;}
  yield return new WaitForSeconds(4);WorldLook.Current?.Pin(LookPresets.Compose(light=="night"?TimeOfDay.Night:TimeOfDay.Day,Weather.Clear));yield return new WaitForSeconds(1.5f);
  var cars=FindObjectsByType<AmbientVehicle>(FindObjectsSortMode.None);var kinds=cars.GroupBy(c=>AmbientVehicle.BodyNames[c.BodyType]).Select(g=>g.Key+" "+g.Count());
  int modern=cars.Count(c=>c.GetComponentsInChildren<Transform>().Any(t=>t.name.StartsWith("Traffic")&&t.name.EndsWith(" model")));
  Check(cars.Length>0&&modern==cars.Length,$"traffic ({light}): {cars.Length} cars [{string.Join(", ",kinds)}], Blender kit shown on {modern}");
  var cam=Camera.main;var chase=FindAnyObjectByType<ChaseCamera>();if(chase)chase.enabled=false;var cv=CameraViews.Current;if(cv)cv.enabled=false;
  int k=0;foreach(var c in cars.GroupBy(c=>c.BodyType).Select(g=>g.First()).ToArray()){var t=c.transform;var eye=t.position+t.right*4.5f+t.forward*5f+Vector3.up*1.8f;int n=k++;string kind=AmbientVehicle.BodyNames[c.BodyType].Replace(' ','_').Replace('/','_');yield return Late(()=>{cam.transform.position=eye;cam.transform.LookAt(t.position);Shot($"traffic-{light}-{n}-{kind}");});}
  if(chase)chase.enabled=true;if(cv)cv.enabled=true;yield return Menu();}
 // the player's vehicle parked (kinematic) on the ground near a scene, so the scene animates (it runs near the player)
 void Park(Vector3 at){var car=race.vehicle;if(Physics.Raycast(at+Vector3.up*60,Vector3.down,out var gh,200,~0,QueryTriggerInteraction.Ignore))at=gh.point+Vector3.up*.8f;car.Body.isKinematic=true;car.transform.position=at;car.Body.position=at;Physics.SyncTransforms();}
 // Part D: each scripted scene forced, one shot close enough to see the people. people
 IEnumerator PeopleCheck(){yield return EnterRoam("StreetLoopGreybox","moto");yield return new WaitForSeconds(1);
  var life=FindAnyObjectByType<AmbientLife>();var cam=Camera.main;var chase=FindAnyObjectByType<ChaseCamera>();if(chase)chase.enabled=false;var cv=CameraViews.Current;if(cv)cv.enabled=false;
  WorldLook.Current?.Pin(LookPresets.Compose(TimeOfDay.Day,Weather.Clear));
  Vector3 Mid(Vector3[] a)=>a.Aggregate(Vector3.zero,(x,y)=>x+y)/a.Length;
  foreach(var (want,name,spots) in new[]{(0,"football",life.football),(1,"coffee",life.coffee),(2,"kyle",life.smoking)}){
   for(int seed=1;seed<200;seed++){AmbientLife.ForcedSeed=seed;life.SelectScenes();if(want<2?life.DanScene==want:life.FriendScene)break;}
   var c=Mid(spots)+Vector3.up*1.2f;var people=FindObjectsByType<ScenePerson>(FindObjectsSortMode.None).Where(p=>p.isActiveAndEnabled&&(p.transform.position-c).magnitude<15).ToArray();
   Check(people.Length==spots.Length,$"{name}: {people.Length} characters ({string.Join(", ",people.Select(p=>p.Who.Name))}) at {V(c)}");
   var dir=spots.Length>1?Vector3.Cross(Vector3.up,(spots[1]-spots[0]).normalized):Vector3.forward;if(name=="kyle")dir=-dir;var from=c+dir*(name=="football"?9f:5.5f)+Vector3.up*1.6f;
   Park(c+dir*14);
   yield return new WaitForSeconds(1.5f);yield return Late(()=>{cam.transform.position=from;cam.transform.LookAt(c);Shot("people-"+name);});}
  AmbientLife.ForcedSeed=0;
  var camp=FindObjectsByType<Transform>(FindObjectsSortMode.None).FirstOrDefault(t=>t.name=="Permanent mountainside camp / two seated guys");
  if(camp){var sp=camp.GetComponentsInChildren<ScenePerson>();Check(sp.Length==2,$"camp: {sp.Length} characters ({string.Join(", ",sp.Select(p=>p.Who.Name))})");
   var cc=camp.position+Vector3.up*1;var from=camp.position+camp.forward*5.5f+Vector3.up*2.2f;yield return new WaitForSeconds(.5f);yield return Late(()=>{cam.transform.position=from;cam.transform.LookAt(cc);Shot("people-camp");});}
  WorldLook.Current?.Pin(LookPresets.Compose(TimeOfDay.Day,Weather.Snow));yield return new WaitForSeconds(2);
  var snow=FindAnyObjectByType<SnowScenes>();
  if(snow){Park(snow.SledPosition+new Vector3(14,0,0));yield return new WaitForSeconds(1);Check(snow.SledActive&&snow.HockeyActive,$"snow scenes: sled {snow.SledActive}, hockey {snow.HockeyActive}");
   for(int i=0;i<3;i++){var sp=snow.SledPosition;int n=i;yield return Late(()=>{cam.transform.position=sp+new Vector3(4,2,1.5f);cam.transform.LookAt(sp+Vector3.up*.8f);Shot("people-sled-"+n);});yield return new WaitForSeconds(3.5f);}
   Park(snow.BallPosition+new Vector3(-14,0,0));yield return new WaitForSeconds(2.5f);var bp=snow.BallPosition;yield return Late(()=>{cam.transform.position=bp+new Vector3(-6,3,0);cam.transform.LookAt(bp+Vector3.up*.9f);Shot("people-hockey");});}
  WorldLook.Current?.Pin(LookPresets.Compose(TimeOfDay.Day,Weather.Clear));if(chase)chase.enabled=true;if(cv)cv.enabled=true;}
 // BUG-002: ride straight at the world's edge: wall:Scene:profile:x,z,yaw,speed. Full throttle for 8 s from the ground at
 // (x, z) heading yaw; pass = never below the ground's lowest point - 15 m (no fall reset), ends within 60 m of the start.
 IEnumerator Wall(string scene,string profile,string spec){
  if(scene==RaceFlow.RoamScene)yield return EnterRoam("StreetLoopGreybox",profile);else yield return EnterScene(scene);yield return new WaitForSeconds(1);
  var f=spec.Split(',').Select(F).ToArray();var car=race.vehicle;car.GetComponent<VehicleInput>().enabled=false;car.enabled=false;
  var p=new Vector3(f[0],600,f[1]);Physics.Raycast(p,Vector3.down,out var gh,900,~0,QueryTriggerInteraction.Ignore);var start=gh.point+Vector3.up*.8f;var rot=Quaternion.Euler(0,f[2],0);
  Put(car,start,rot,rot*Vector3.forward*f[3]);int resets=0;System.Action onR=()=>resets++;var resp=car.GetComponent<VehicleRespawn>();resp.Respawned+=onR;
  float minY=start.y,far=0;for(float t=0;t<8;t+=Time.fixedDeltaTime){AudioListener.volume=0;car.Simulate(1,0,0,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();var q=car.Body.position;minY=Mathf.Min(minY,q.y);far=Mathf.Max(far,Vector3.Dot(q-start,rot*Vector3.forward));if(t>2.5f&&t<2.53f)yield return Late(()=>Shot($"wall-{Scene}-{profile}-{f[0]:F0}-{f[1]:F0}"));}
  resp.Respawned-=onR;var end=car.Body.position;
  Check(resets==0&&minY>-15&&far<60,$"{Scene} {profile} at the edge from {V(start)} heading {f[2]} at {f[3]} m/s: furthest {far:F1} m out, lowest y {minY:F1}, resets {resets}, end {V(end)}, up {car.transform.up.y:F2}");
  car.enabled=true;car.GetComponent<VehicleInput>().enabled=true;
  Note("world edge: "+string.Join(" | ",WorldEdge.Report));if(scene!=RaceFlow.RoamScene)yield return Menu();}
 // Every collider touching small spheres at points: overlap:Scene:x,y,z,r|... with the closest point on each.
 IEnumerator Overlap(string scene,string specs){yield return EnterScene(scene);yield return new WaitForSeconds(1);var sb=new StringBuilder();
  foreach(var spec in specs.Split('|')){var f=spec.Split(',');var p=ParseV(string.Join(",",f.Take(3)));float r=F(f[3]);sb.AppendLine($"== {V(p)} r {r}");
   foreach(var c in Physics.OverlapSphere(p,r,~0,QueryTriggerInteraction.Collide)){var q=c is MeshCollider mc&&!mc.convex?c.ClosestPointOnBounds(p):c.ClosestPoint(p);sb.AppendLine($"  {P(c.transform)} [{c.GetType().Name}] trig {c.isTrigger} rb {(c.attachedRigidbody?c.attachedRigidbody.name:"-")} closest {V(q)} bounds {V(c.bounds.min)}..{V(c.bounds.max)}");}
   foreach(var dir in new[]{Vector3.forward,Vector3.back,Vector3.left,Vector3.right,new Vector3(1,0,1).normalized,new Vector3(-1,0,-1).normalized,new Vector3(1,0,-1).normalized,new Vector3(-1,0,1).normalized})
    foreach(float dy in new[]{-.3f,-.15f,0f,.15f,.3f})foreach(var h in Physics.RaycastAll(p+Vector3.up*dy-dir*3,dir,6,~0,QueryTriggerInteraction.Ignore))if(!h.collider.attachedRigidbody)sb.AppendLine($"  ray {V(dir)} dy {dy}: {P(h.collider.transform)} at {V(h.point)} n {V(h.normal)}");}
  File.WriteAllText($"{output}/overlap-{Scene}.txt",sb.ToString());Note($"{Scene}: overlap written");if(scene!=RaceFlow.RoamScene)yield return Menu();}

 // The world's edge: every static ground collider's bounds, and, along rays from the centre every 2 degrees, the last
 // point that still has ground under it (x, z, y, collider). edge:Scene
 IEnumerator Edge(string scene){yield return EnterScene(scene);yield return new WaitForSeconds(1);
  var sb=new StringBuilder();Bounds all=default;bool any=false;
  foreach(var c in FindObjectsByType<Collider>(FindObjectsSortMode.None)){if(c.isTrigger||c.attachedRigidbody||!c.enabled)continue;var b=c.bounds;if(b.size.x<150&&b.size.z<150)continue;
   sb.AppendLine($"big {P(c.transform)} [{c.GetType().Name}] b {V(b.center)} s {V(b.size)} min {V(b.min)} max {V(b.max)}");if(!any){all=b;any=true;}else all.Encapsulate(b);}
  sb.AppendLine($"union min {V(all.min)} max {V(all.max)}");
  var c0=new Vector3(all.center.x,0,all.center.z);
  sb.AppendLine("angle,x,z,y,collider");
  for(int deg=0;deg<360;deg+=2){var d=Quaternion.Euler(0,deg,0)*Vector3.forward;Vector3 last=default;string name="-";
   for(float r=0;r<3000;r+=4){var p=c0+d*r;if(Physics.Raycast(new Vector3(p.x,900,p.z),Vector3.down,out var h,1400,~0,QueryTriggerInteraction.Ignore)&&!h.collider.attachedRigidbody){last=h.point;name=h.collider.name;}else if(r>200&&last!=default)break;}
   sb.AppendLine($"{deg},{last.x:F0},{last.z:F0},{last.y:F1},{name}");}
  File.WriteAllText($"{output}/edge-{Scene}.txt",sb.ToString());Note($"{Scene}: edge written, union {V(all.min)} .. {V(all.max)}");
  if(scene!=RaceFlow.RoamScene)yield return Menu();}
}
}
#endif
