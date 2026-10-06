#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Racer {
// 0.85 targeted checks, added to the 0.80 runner (same muted isolated save, same helpers): PROBE_CASES="case:args;...".
public sealed partial class Report080Checks {
 IEnumerator Cases085(string[] a)=>a[0] switch{"escape85"=>Escape085(a[1],a[2],a[3],a.Length>4?a[4]:"0,45,90,135,180,225,270,315"),"trees85"=>Trees085(a[1]),"lineshots"=>LineShots(a[1],a[2],a[3]),"hands85"=>Hands085(a[1]),"wide85"=>Wide085(a[1],a[2],F(a[3]),F(a[4]),F(a[5]),a[6]),_=>Cases086(a)};

 // Part A: trees, bushes and clumps on the race lines (main and optional) of a scene with the new scenery: what the rule
 // found / left out / kept, anything drawn still standing on a race line, and trunk colliders on one. trees85:Scene
 IEnumerator Trees085(string scene){yield return EnterScene(scene);yield return new WaitForSeconds(1);var world=SceneryWorld.Current;
  if(!world||world.Trees==null){Check(false,$"{Scene}: no new scenery");if(scene!=RaceFlow.RoamScene)yield return Menu();yield break;}
  var rep=world.Trees.DrivableReport;int onLine=rep.Count(x=>x.Contains("optional line"));
  int kept=rep.Count(x=>x.StartsWith("KEPT"));var keptOnLine=rep.Where(x=>x.StartsWith("KEPT")&&x.Contains("optional line")).ToList();
  var left=new List<string>();foreach(var t in world.Trees.Placements){if(t.bottom.y<-1000)continue;if(SceneryTrees.OnRaceLine(t.bottom,out var w,0))left.Add($"{t.source} at {V(t.bottom)} on {w}");}
  var cols=new List<string>();foreach(var b in FindObjectsByType<BoxCollider>(FindObjectsSortMode.None)){if(b.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)<0||!b.enabled)continue;var bb=b.bounds;var bottom=new Vector3(bb.center.x,bb.min.y,bb.center.z);if(SceneryTrees.OnRaceLine(bottom,out var w,0))cols.Add($"{P(b.transform)} at {V(bottom)} on {w}");}
  File.WriteAllLines($"{output}/A-trees-{Scene}.txt",new[]{$"{Scene}: rule entries {rep.Count} (on a race line {onLine}), left out {world.Trees.DrivableSkipped}, kept (collider near a line) {kept}"}.Concat(rep).Concat(new[]{"-- drawn and still on a race line:"}).Concat(left).Concat(new[]{"-- trunk colliders on a race line:"}).Concat(cols));
  Check(cols.Count==0&&left.Count<=cols.Count,$"{Scene}: trees / bushes / clumps left out on a drivable surface or race line {world.Trees.DrivableSkipped} (of which on a race line {onLine}); drawn still on a race line {left.Count}; trunk colliders on a race line {cols.Count}");
  if(scene!=RaceFlow.RoamScene)yield return Menu();}
 // One view along an optional line at each station: lineshots:Scene:branch title:s1,s2
 IEnumerator LineShots(string scene,string branch,string stations){yield return EnterScene(scene);yield return new WaitForSeconds(1);Day();
  var wr=FindObjectsByType<WoodlandRoute>(FindObjectsSortMode.None).FirstOrDefault(b=>b.title==branch);if(!wr){Note("no branch "+branch);yield break;}
  var cam=Camera.main;var chase=FindAnyObjectByType<ChaseCamera>();if(chase)chase.enabled=false;var cv=CameraViews.Current;if(cv)cv.enabled=false;var hud=FindObjectsByType<Canvas>(FindObjectsSortMode.None);foreach(var c in hud)c.enabled=false;
  foreach(var st in stations.Split(',')){float s=F(st);var p=wr.At(s,out var f);var ahead=wr.At(s+25,out _);var eye=p+Vector3.up*2.2f;
   yield return Late(()=>{cam.transform.position=eye;cam.transform.LookAt(ahead+Vector3.up*1.2f);Shot($"line-{Scene}-{branch.Replace(' ','_')}-{s:F0}");});}
  foreach(var c in hud)if(c)c.enabled=true;if(chase)chase.enabled=true;if(cv)cv.enabled=true;Note($"{Scene} {branch}: views at {stations}");yield return Menu();}

 // Part C: ride the main at a lateral offset (+ = right of travel), holding the speed, from one station to another, once per
 // offset: air time, lowest up vector, largest vertical jolt, resets. wide85:Scene:profile:from:to:speed:offset1,offset2
 IEnumerator Wide085(string scene,string profile,float from,float to,float speed,string offsets){
  foreach(var os in offsets.Split(',')){float offset=F(os);yield return Enter085(scene,profile);var car=race.vehicle;var rd=race.road;rd.Initialize();
   car.GetComponent<VehicleInput>().enabled=false;car.enabled=false;var resp=car.GetComponent<VehicleRespawn>();int resets=0;Action onR=()=>resets++;resp.Respawned+=onR;
   var contacts=new List<string>{"t,x,y,z,collider,lx,ly,lz,nx,ny,nz,impulse"};var cl=car.gameObject.AddComponent<ContactLog083>();cl.log=contacts;
   Vector3 Line(float st,out Vector3 f){var q=rd.At(st,out f);f.y=0;f.Normalize();return q+Vector3.Cross(Vector3.up,f)*offset;}
   var start=Line(from,out var f0);start.y=Ground085(start,start.y+5);car.Body.position=start+Vector3.up*.7f;car.Body.rotation=Quaternion.LookRotation(f0);car.transform.SetPositionAndRotation(car.Body.position,car.Body.rotation);
   car.Body.linearVelocity=Vector3.zero;car.Body.angularVelocity=Vector3.zero;car.ClearSteering();resp.SeedCoursePosition(start);
   for(int i=0;i<20;i++){car.Simulate(0,0,0,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();}
   car.Body.linearVelocity=car.transform.forward*speed;float t0=Time.time,air=0,maxAir=0,minUp=1,maxJolt=0,prevVy=car.Body.linearVelocity.y;Vector3 joltAt=default;bool reached=false;
   var log=new StringBuilder("t,x,y,z,speed,wheels,upY,station,lateral\n");
   while(Time.time-t0<20){var p=car.Body.position;float st=rd.ProjectNear(p,from,400,out float lat);if(st>=to){reached=true;break;}
    var tgt=Line(st+8,out _);var dir=tgt-p;dir.y=0;float ang=Vector3.SignedAngle(Vector3.ProjectOnPlane(car.transform.forward,Vector3.up),dir,Vector3.up);
    float v=car.ForwardSpeed;car.Simulate(v<speed?1:0,v>speed+3?.5f:0,Mathf.Clamp(ang/25f,-1,1),Time.fixedDeltaTime);yield return new WaitForFixedUpdate();
    float vy=car.Body.linearVelocity.y,j=Mathf.Abs(vy-prevVy)/Time.fixedDeltaTime;prevVy=vy;if(car.GroundedWheels>0&&j>maxJolt){maxJolt=j;joltAt=car.Body.position;}
    minUp=Mathf.Min(minUp,car.transform.up.y);if(car.GroundedWheels==0){air+=Time.fixedDeltaTime;maxAir=Mathf.Max(maxAir,air);}else air=0;
    log.AppendLine($"{Time.time-t0:F2},{p.x:F2},{p.y:F2},{p.z:F2},{v:F2},{car.GroundedWheels},{car.transform.up.y:F3},{st:F1},{lat:F2}");if(resets>0)break;}
   resp.Respawned-=onR;car.enabled=true;car.GetComponent<VehicleInput>().enabled=true;File.WriteAllText($"{output}/wide-{Scene}-{profile}-{offset:F0}.csv",log.ToString());Destroy(cl);File.WriteAllLines($"{output}/wide-contacts-{Scene}-{profile}-{offset:F0}.csv",contacts);
   Check(reached&&resets==0&&minUp>.7f,$"{Scene} {profile} main {from}->{to} at {speed} m/s, {offset:+0.0;-0.0} m off the centre: {(reached?"through":"NOT through")}, resets {resets}, max air {maxAir:F2}s, min up {minUp:F2}, max jolt {maxJolt:F0} m/s² at {V(joltAt)}");
   yield return Menu();}}

 // Part B: hands on the steering. Each vehicle held still on a straight with the steering at full left, centre and full
 // right: the distance of each wrist from the steering pivot (constant = the hands stay on the rim / grips), and first-
 // person and outside shots. hands85:profile1,profile2
 IEnumerator Hands085(string profiles){yield return Load("StreetLoopGreybox");yield return Menu();var rd=race.road;rd.Initialize();
  var st=flow.Save.Settings;st.weather=0;st.timeOfDay=0;flow.Save.SaveSettings();
  foreach(var profile in profiles.Split(',')){foreach(var view in new[]{2,0}){
   st.cameraView=view;flow.Save.SaveSettings();flow.OpenGarage();flow.SelectVehicle(profile);flow.CloseGarage();race.opponents=false;race.traffic=false;race.laps=1;flow.StartRace();
   float tr=Time.realtimeSinceStartup;while(flow.State!=RaceFlow.Stage.Racing&&Time.realtimeSinceStartup-tr<40){AudioListener.volume=0;yield return null;}
   var car=race.vehicle;car.GetComponent<VehicleInput>().enabled=false;car.enabled=false;var p=rd.At(300,out var fw);fw.y=0;
   var visual=car.transform.Find("Vehicle visual");var arms=visual?visual.GetComponentInChildren<RiderArms>(true):null;var pivot=visual?visual.Cast<Transform>().Select(t=>t.GetComponent<MotorcycleFrontEnd>()).FirstOrDefault(m=>m):null;
   var wrists=arms?arms.GetComponentsInChildren<Transform>(true).Where(t=>t.name.StartsWith("Wrist")).ToArray():new Transform[0];var line=new List<string>();
   foreach(var steer in new[]{-1f,0f,1f}){
    car.Body.position=p+Vector3.up*.6f;car.Body.rotation=Quaternion.LookRotation(fw);car.transform.SetPositionAndRotation(car.Body.position,car.Body.rotation);car.Body.linearVelocity=Vector3.zero;car.Body.angularVelocity=Vector3.zero;
    for(int i=0;i<60;i++){car.Simulate(0,0,steer,Time.fixedDeltaTime);car.Body.linearVelocity=Vector3.zero;car.Body.angularVelocity=Vector3.zero;car.Body.rotation=Quaternion.LookRotation(fw);yield return new WaitForFixedUpdate();}
    yield return null;yield return null;string d="";
    yield return Late(()=>{if(pivot)d=string.Join(" ",wrists.Select(w=>$"{w.name} {Vector3.Distance(w.position,pivot.transform.position):F3}"));
     if(view==0){var cam=Camera.main;var chase=FindAnyObjectByType<ChaseCamera>();if(chase)chase.enabled=false;var cv=CameraViews.Current;if(cv)cv.enabled=false;
      var right=Vector3.Cross(Vector3.up,fw);cam.transform.position=car.transform.position+right*2.2f+fw*1.6f+Vector3.up*1.4f;cam.transform.LookAt(car.transform.position+Vector3.up*.9f+fw*.4f);}
     Shot($"B-{profile}-{(view==2?"fpv":"outside")}-{(steer<0?"left":steer>0?"right":"centre")}",1600,900);});
    line.Add($"steer {steer:+0;-0;0} (shown {car.VisualSteering:F2}): wrist to steering pivot {d}");}
   Note($"{profile} {(view==2?"first person":"outside")}: {string.Join(" | ",line)}");
   var chase2=FindAnyObjectByType<ChaseCamera>();if(chase2)chase2.enabled=true;if(CameraViews.Current)CameraViews.Current.enabled=true;
   car.enabled=true;car.GetComponent<VehicleInput>().enabled=true;yield return Menu();}}
  st.cameraView=0;flow.Save.SaveSettings();}

 // Enter a scene with one vehicle: Free Roam (FreeRoamWorld) or a solo race with no rivals or traffic.
 IEnumerator Enter085(string scene,string profile){
  if(scene==RaceFlow.RoamScene){yield return EnterRoam("StreetLoopGreybox",profile);yield break;}
  yield return Load(scene);yield return Menu();flow.OpenGarage();flow.SelectVehicle(profile);flow.CloseGarage();race.opponents=false;race.traffic=false;race.laps=1;flow.StartRace();
  float tr=Time.realtimeSinceStartup;while(flow.State!=RaceFlow.Stage.Racing&&Time.realtimeSinceStartup-tr<40){AudioListener.volume=0;yield return null;}
  yield return new WaitForSeconds(3.5f);}
 static bool Skip085(Collider c)=>c.isTrigger||c.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0||c.name.IndexOf("tree",StringComparison.OrdinalIgnoreCase)>=0||(c.attachedRigidbody&&!c.attachedRigidbody.isKinematic)||c.GetComponentInParent<ArcadeVehicle>();
 static float Ground085(Vector3 p,float top){float best=float.NaN;foreach(var h in Physics.RaycastAll(new Vector3(p.x,top,p.z),Vector3.down,top-p.y+60,~0,QueryTriggerInteraction.Ignore)){if(Skip085(h.collider)||h.collider.name.StartsWith("Frozen water"))continue;if(float.IsNaN(best)||h.point.y>best)best=h.point.y;}return best;}
 // In water: inside any active water whose surface is above the ground there.
 bool Wet085(Vector3 p){float g=Ground085(p,p.y+3);foreach(var w in ShallowWater.Active)if(w&&w.gameObject.scene==SceneManagerScene()&&w.Contains(p)&&(float.IsNaN(g)||w.Surface>g+.02f))return true;return false;}
 static UnityEngine.SceneManagement.Scene SceneManagerScene()=>UnityEngine.SceneManagement.SceneManager.GetActiveScene();
 // The deepest bed point of a water body (0.5 m grid over its footprint).
 static Vector3 Deepest085(ShallowWater w){var t=w.transform;Vector3 best=t.position;float low=float.PositiveInfinity;
  for(float u=-.48f;u<=.48f;u+=.5f/Mathf.Max(4,Mathf.Max(t.lossyScale.x,t.lossyScale.z)))for(float v=-.48f;v<=.48f;v+=.5f/Mathf.Max(4,Mathf.Max(t.lossyScale.x,t.lossyScale.z))){
   var p=t.TransformPoint(new Vector3(u,0,v));if(!w.Contains(p))continue;float g=Ground085(p,w.Surface+1.2f);
   // Within 5 cm of the deepest, the point nearest the middle (a flat pool floor: its centre, clear of the walls).
   if(float.IsNaN(g))continue;float d=new Vector2(u,v).sqrMagnitude*.001f;if(g+d<low){low=g+d;best=new Vector3(p.x,g,p.z);}}
  return best;}

 // Part F: escape85:Scene:profiles:waterNameSubstring[:headings]. For each matching water body and vehicle: the vehicle is
 // set down at the deepest point of the bed facing each heading in turn and driven straight on at full throttle (steering
 // only to hold the heading) for up to 25 s. Out = on dry ground (not in any water) 3 m clear of the water for 1 s, without
 // a reset. Writes the result per heading and a summary per water body / vehicle.
 IEnumerator Escape085(string scene,string profiles,string filter,string headings){
  foreach(var profile in profiles.Split(',')){
   yield return Enter085(scene,profile);
   var car=race.vehicle;var waters=ShallowWater.Active.Where(w=>w&&w.gameObject.scene==SceneManagerScene()&&w.name.IndexOf(filter,StringComparison.OrdinalIgnoreCase)>=0).ToList();
   if(waters.Count==0){Note($"{Scene}: no water matching '{filter}'");continue;}
   foreach(var w in waters){var deep=Deepest085(w);var sb=new List<string>();int outs=0,n=0;
    foreach(var hs in headings.Split(',')){float hdg=F(hs);n++;
     car.GetComponent<VehicleInput>().enabled=false;car.enabled=false;var resp=car.GetComponent<VehicleRespawn>();int resets=0;Action onR=()=>resets++;resp.Respawned+=onR;
     var rot=Quaternion.Euler(0,hdg,0);car.Body.position=deep+Vector3.up*(car.suspensionLength*.7f);car.Body.rotation=rot;car.transform.SetPositionAndRotation(car.Body.position,rot);
     car.Body.linearVelocity=Vector3.zero;car.Body.angularVelocity=Vector3.zero;car.ClearSteering();resp.SeedCoursePosition(deep);
     for(int i=0;i<25;i++){car.Simulate(0,0,0,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();}
     float t0=Time.time,dryFor=0,maxImm=0;bool escaped=false;var dir=rot*Vector3.forward;
     while(Time.time-t0<25){AudioListener.volume=0;var p=car.Body.position;maxImm=Mathf.Max(maxImm,car.WaterImmersion);
      float ang=Vector3.SignedAngle(Vector3.ProjectOnPlane(car.transform.forward,Vector3.up),dir,Vector3.up);
      car.Simulate(1,0,Mathf.Clamp(ang/25f,-1,1),Time.fixedDeltaTime);yield return new WaitForFixedUpdate();
      bool clear=!Wet085(p)&&!Wet085(p+dir*3)&&car.WaterImmersion<=0.001f;dryFor=clear?dryFor+Time.fixedDeltaTime:0;
      if(dryFor>1){escaped=resets==0;break;}
      if(resets>0)break;}
     resp.Respawned-=onR;car.enabled=true;car.GetComponent<VehicleInput>().enabled=true;
     if(escaped)outs++;sb.Add($"{hdg:F0}:{(escaped?$"out {Time.time-t0:F1}s":resets>0?"reset":"stuck")}");}
    Check(outs>0,$"{Scene} {w.name} ({V(w.transform.position)}, surface {w.Surface:F2}, deepest {V(deep)}) {profile}: out in {outs}/{n} headings [{string.Join(" ",sb)}]");}
   yield return Menu();}}
}
}
#endif
