#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Racer {
// 0.87 targeted checks, added to the 0.80 runner (same muted isolated save, same helpers): PROBE_CASES="case:args;...".
public sealed partial class Report080Checks {
 IEnumerator Cases087(string[] a)=>a[0] switch{"summit87"=>Summit087(a[1],a[2],a.Length>3?a[3]:"clean,main",a.Length>4?F(a[4]):30),"climb87"=>Climb087(a[1]),"shots87"=>Shots087(),"kyledrive87"=>KyleDrive087(a[1]),"kyleshots87"=>KyleShots087(a.Length>1?a[1]:"FreeRoamWorld",a.Length>2?a[2]:"day"),"lap87"=>Lap087(a[1],a.Length>2?a[2]:"main,clean"),_=>null};

 // A driver for the timed runs: full throttle, steering at a point ahead on the path (8 m + 0.3 s), braking only when the
 // path's bend in the next 30 m needs a lower speed than this vehicle's grip allows (v = sqrt(0.9 grip R)). Same driver on
 // the main and on the shortcut, so the difference is the shortcut.
 sealed class Path087{public Vector3[] p;public float[] s;public Path087(IEnumerable<Vector3> pts){p=pts.ToArray();s=new float[p.Length];for(int i=1;i<p.Length;i++)s[i]=s[i-1]+Vector2.Distance(new(p[i-1].x,p[i-1].z),new(p[i].x,p[i].z));}
  public float Length=>s[^1];
  public Vector3 At(float d){d=Mathf.Clamp(d,0,s[^1]);int i=1;while(i<s.Length-1&&s[i]<d)i++;return Vector3.Lerp(p[i-1],p[i],Mathf.InverseLerp(s[i-1],s[i],d));}
  public float Project(Vector3 q,float near,float window,out float lat){float best=float.MaxValue,r=0;lat=0;for(int i=1;i<p.Length;i++){if(s[i]<near-window||s[i-1]>near+window)continue;var a=p[i-1];var v=p[i]-a;v.y=0;var w=q-a;w.y=0;float t=Mathf.Clamp01(Vector3.Dot(w,v)/Mathf.Max(1e-5f,v.sqrMagnitude));var d=w-v*t;if(d.sqrMagnitude<best){best=d.sqrMagnitude;r=Mathf.Lerp(s[i-1],s[i],t);lat=Vector3.Dot(d,Vector3.Cross(Vector3.up,v).normalized);}}return r;}
  public float Radius(float d){var a=At(d-4);var b=At(d);var c=At(d+4);var u=b-a;u.y=0;var v=c-b;v.y=0;float ang=Vector3.Angle(u,v)*Mathf.Deg2Rad;return ang<1e-4f?1e4f:8f/ang;}}
 sealed class Touch087:MonoBehaviour{public List<string> hits=new();void OnCollisionEnter(Collision c){var n=c.collider.name;if(n.StartsWith("Ground_")&&!n.Contains("slab"))return;if(n.StartsWith("Ground")&&!n.Contains("slab"))return;hits.Add($"{Time.time:F2} {n} at {c.GetContact(0).point.x:F1},{c.GetContact(0).point.y:F1},{c.GetContact(0).point.z:F1} impulse {c.impulse.magnitude:F0}");}}
 static readonly List<string> SummitTable=new();
 // summit87:Scene:profile:runs:v0. runs: main (main s 1826 -> 2110), clean (same, by the Summit Climb), kink1 (taking the lower
 // offset bend straight on: the line the climb came up on), crest (arriving at the crest 2 m right of the line and straight
 // on over it, not setting up for the merge). The time is from the start to the main's station 2110 (after the rejoin).
 IEnumerator Summit087(string scene,string profile,string runs,float v0){
  foreach(var run in runs.Split(',')){
   yield return Enter085(scene,profile);var car=race.vehicle;var rd=race.road;rd.Initialize();var wr=FindObjectsByType<WoodlandRoute>(FindObjectsSortMode.None).FirstOrDefault(b=>b.title=="Summit Climb");if(!wr){Note("no Summit Climb");yield break;}wr.Initialize();
   const float S0=1826,S1=2110;var pts=new List<Vector3>();
   if(run=="main"){for(float s=S0;s<=S1;s+=1)pts.Add(rd.At(s,out _));}
   else{for(float s=S0;s<wr.entryRoad;s+=1)pts.Add(rd.At(s,out _));
    for(float s=0;s<=wr.Length;s+=.65f){var q=wr.At(s,out var f);
     if(run=="kink1"&&s>36&&s<74){var a=wr.At(36,out var fa);fa.y=0;fa.Normalize();q=a+fa*(s-36);}
     if(run=="crest"&&s>132&&s<175){var a=wr.At(132,out var fa);fa.y=0;fa.Normalize();q=a+fa*(s-132)+Vector3.Cross(Vector3.up,fa)*2;}
     pts.Add(q);}
    for(float s=wr.exitRoad+2;s<=S1;s+=1)pts.Add(rd.At(s,out _));}
   var path=new Path087(pts);float grip=car.maxGripAcceleration;
   car.GetComponent<VehicleInput>().enabled=false;car.enabled=false;var resp=car.GetComponent<VehicleRespawn>();int resets=0;Action onR=()=>resets++;resp.Respawned+=onR;var touch=car.gameObject.AddComponent<Touch087>();
   var start=path.At(0);var f0=path.At(2)-start;f0.y=0;start.y=Ground085(start,start.y+5);var rot=Quaternion.LookRotation(f0.normalized);
   car.Body.position=start+Vector3.up*(car.suspensionLength*.7f);car.Body.rotation=rot;car.transform.SetPositionAndRotation(car.Body.position,rot);car.Body.linearVelocity=Vector3.zero;car.Body.angularVelocity=Vector3.zero;car.ClearSteering();resp.SeedCoursePosition(start);
   for(int i=0;i<20;i++){car.Simulate(0,0,0,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();}
   car.Body.linearVelocity=car.transform.forward*v0;float t0=Time.time,air=0,maxAir=0,minUp=1,maxLat=0,minV=99,maxJolt=0,prevVy=car.Body.linearVelocity.y,d=0;bool done=false;Vector3 airAt=default;string branchSeen="";
   var log=new StringBuilder("t,x,y,z,speed,wheels,upY,pathS,lateral,branch\n");
   while(Time.time-t0<40){AudioListener.volume=0;var p=car.Body.position;d=path.Project(p,d,25,out float lat);if(rd.ProjectNear(p,S1-60,80,out _)>=S1&&d>path.Length-30){done=true;break;}
    float v=car.ForwardSpeed;var tgt=path.At(d+8+.3f*Mathf.Max(0,v));var to=tgt-p;to.y=0;float ang=Vector3.SignedAngle(Vector3.ProjectOnPlane(car.transform.forward,Vector3.up),to,Vector3.up);
    float need=99;for(float k=2;k<=30;k+=2)need=Mathf.Min(need,Mathf.Sqrt(.9f*grip*path.Radius(d+k))+k*.35f);
    float thr=v<need?1:0,brk=v>need+1.5f?Mathf.Clamp01((v-need)/5):0;car.Simulate(thr,brk,car.GroundedWheels>0?Mathf.Clamp(ang/25f,-1,1):0,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();
    var vel=car.Body.linearVelocity;float j=Mathf.Abs(vel.y-prevVy)/Time.fixedDeltaTime;prevVy=vel.y;if(car.GroundedWheels>0)maxJolt=Mathf.Max(maxJolt,j);
    minUp=Mathf.Min(minUp,car.transform.up.y);maxLat=Mathf.Max(maxLat,Mathf.Abs(lat));minV=Mathf.Min(minV,v);if(car.GroundedWheels==0){air+=Time.fixedDeltaTime;if(air>maxAir){maxAir=air;airAt=p;}}else air=0;
    var br=race.Racers.Count>0&&race.Racers[0].Branch.Route?race.Racers[0].Branch.Route.title:"main";if(br!="main")branchSeen=br;
    log.AppendLine($"{Time.time-t0:F2},{p.x:F2},{p.y:F2},{p.z:F2},{v:F2},{car.GroundedWheels},{car.transform.up.y:F3},{d:F1},{lat:F2},{br}");if(resets>0)break;}
   float time=Time.time-t0;resp.Respawned-=onR;Destroy(touch);car.enabled=true;car.GetComponent<VehicleInput>().enabled=true;
   File.WriteAllText($"{output}/B-run-{profile}-{run}.csv",log.ToString());
   var line=$"{profile} {run}: {(done?$"{time:F2} s":"NOT through")} (start {v0} m/s at main s {S0}, to main s {S1}), resets {resets}, slowest {minV:F1} m/s, longest air {maxAir:F2} s{(maxAir>.3f?" at "+V(airAt):"")}, min up {minUp:F2}, max jolt {maxJolt:F0}, widest {maxLat:F1} m off the path, race branch '{branchSeen}', hit: {(touch.hits.Count==0?"nothing":string.Join("; ",touch.hits.Take(4)))}";
   SummitTable.Add($"{profile}\t{run}\t{(done?time.ToString("F2"):"")}\t{resets}\t{line}");File.WriteAllLines(output+"/summit-table.tsv",SummitTable);
   Check(done&&resets==0&&(run!="clean"||touch.hits.Count==0),line);yield return Menu();}}

 // The weakest car from a standing start at the foot of the climb (branch s 20), full throttle up the centre line.
 IEnumerator Climb087(string profile){yield return Enter085("LakeWoods",profile);var car=race.vehicle;var wr=FindObjectsByType<WoodlandRoute>(FindObjectsSortMode.None).First(b=>b.title=="Summit Climb");wr.Initialize();
  car.GetComponent<VehicleInput>().enabled=false;car.enabled=false;var resp=car.GetComponent<VehicleRespawn>();int resets=0;Action onR=()=>resets++;resp.Respawned+=onR;
  var start=wr.At(24,out var f0);f0.y=0;start.y=Ground085(start,start.y+4);var rot=Quaternion.LookRotation(f0.normalized);car.Body.position=start+Vector3.up*(car.suspensionLength*.7f);car.Body.rotation=rot;car.transform.SetPositionAndRotation(car.Body.position,rot);car.Body.linearVelocity=Vector3.zero;car.Body.angularVelocity=Vector3.zero;car.ClearSteering();resp.SeedCoursePosition(start);
  for(int i=0;i<30;i++){car.Simulate(0,1,0,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();}
  float t0=Time.time,minV=99,top=0;var speeds=new StringBuilder();float nextMark=30;
  while(Time.time-t0<40&&resets==0){AudioListener.volume=0;var p=car.Body.position;float s=wr.Project(p,out _);top=Mathf.Max(top,s);if(s>=155)break;var tgt=wr.At(s+7,out _);var to=tgt-p;to.y=0;float ang=Vector3.SignedAngle(Vector3.ProjectOnPlane(car.transform.forward,Vector3.up),to,Vector3.up);
   car.Simulate(1,0,Mathf.Clamp(ang/25f,-1,1),Time.fixedDeltaTime);yield return new WaitForFixedUpdate();top=Mathf.Max(top,s);if(Time.time-t0>3&&s>40&&s<150)minV=Mathf.Min(minV,car.ForwardSpeed);if(s>=nextMark){speeds.Append($" s{nextMark:F0}:{car.ForwardSpeed:F1}");nextMark+=20;}}
  resp.Respawned-=onR;car.enabled=true;car.GetComponent<VehicleInput>().enabled=true;
  Check(top>=155&&resets==0,$"{profile} standing start at Summit Climb s 24 (foot of the climb), full throttle: reached s {top:F0} (the crest is s 150) in {Time.time-t0:F1} s, resets {resets}, slowest after 40 m {minV:F1} m/s; speed by station:{speeds}");yield return Menu();}

 // Views: the fork from the main, the two kinks, the rough middle, the crest from below and from above, the merge.
 IEnumerator Shots087(){yield return EnterScene("LakeWoods");yield return new WaitForSeconds(1);Day();var wr=FindObjectsByType<WoodlandRoute>(FindObjectsSortMode.None).First(b=>b.title=="Summit Climb");wr.Initialize();var rd=race.road;rd.Initialize();
  var cam=Camera.main;var chase=FindAnyObjectByType<ChaseCamera>();if(chase)chase.enabled=false;var cv=CameraViews.Current;if(cv)cv.enabled=false;var hud=FindObjectsByType<Canvas>(FindObjectsSortMode.None);foreach(var c in hud)c.enabled=false;
  if(race.vehicle)race.vehicle.gameObject.SetActive(false);
  var views=new List<(string n,Vector3 eye,Vector3 look)>();
  {var e=rd.At(wr.entryRoad-30,out _);var l=rd.At(wr.entryRoad+8,out _);views.Add(("B-fork-from-main",e+Vector3.up*2.2f,l+Vector3.up*1.5f));}
  {var e=wr.At(26,out _);var l=wr.At(62,out _);views.Add(("B-kink1",e+Vector3.up*2.2f,l+Vector3.up*1f));}
  {var e=wr.At(68,out _);var l=wr.At(100,out _);views.Add(("B-rough-middle",e+Vector3.up*2.2f,l+Vector3.up*.6f));}
  {var e=wr.At(100,out _);var l=wr.At(136,out _);views.Add(("B-kink2",e+Vector3.up*2.2f,l+Vector3.up*1f));}
  {var e=wr.At(128,out _);var l=wr.At(158,out _);views.Add(("B-crest-from-below",e+Vector3.up*2.2f,l+Vector3.up*1.2f));}
  {var e=wr.At(158,out _);var l=wr.At(200,out _);views.Add(("B-crest-top-to-merge",e+Vector3.up*2.2f,l+Vector3.up*1f));}
  {var e=wr.At(178,out _);var l=rd.At(2060,out _);views.Add(("B-merge-sight-to-hook",e+Vector3.up*1.6f,l+Vector3.up*1f));}
  {var c=wr.At(100,out _);views.Add(("B-overview",c+new Vector3(-55,70,-25),c+Vector3.up*-5));}
  foreach(var v in views)yield return Late(()=>{cam.transform.position=v.eye;cam.transform.LookAt(v.look);Shot(v.n);});
  if(race.vehicle)race.vehicle.gameObject.SetActive(true);foreach(var c in hud)if(c)c.enabled=true;if(chase)chase.enabled=true;if(cv)cv.enabled=true;Note($"Summit Climb views: {string.Join(", ",views.Select(v=>v.n))}");yield return Menu();}

 // One lap of Forest Loop Forward on the race autopilot (the player's own vehicle, no rivals), per mode: main (the Summit
 // Climb closed to it), clean (the autopilot sent up the Summit Climb), kink1 / crest (sent up it, and at the lower kink /
 // the crest a driver who takes it straight on: full throttle, bars held on the heading he arrived with, until he hits
 // something or is 38 m on; then, if stopped, backs off 1.2 s and the autopilot drives on). Lap time, the time from main
 // s 1826 to s 2110, resets, missed gates, what was hit. lap87:profile:modes
 static readonly System.Reflection.FieldInfo PlannedField=typeof(RoadDriver).GetField("plannedBranch",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);
 IEnumerator Lap087(string profile,string modes){
  foreach(var mode in modes.Split(',')){
   yield return Load("LakeWoods");yield return Menu();var st=flow.Save.Settings;st.weather=0;st.timeOfDay=0;st.estimateAiFinishes=false;flow.Save.SaveSettings();
   flow.OpenGarage();flow.SelectVehicle(profile);flow.CloseGarage();race.opponents=false;race.traffic=false;race.laps=1;race.difficulty=1;
   flow.StartRace();float tr=Time.realtimeSinceStartup;while(flow.State!=RaceFlow.Stage.Racing&&Time.realtimeSinceStartup-tr<40){AudioListener.volume=0;yield return null;}
   var car=race.vehicle;var rd=race.road;rd.Initialize();var wr=FindObjectsByType<WoodlandRoute>(FindObjectsSortMode.None).First(b=>b.title=="Summit Climb");wr.Initialize();
   if(mode=="main")wr.aiValidated=false;
   var pilot=car.gameObject.AddComponent<RoadDriver>();pilot.Initialize(race,car,true,1,1);pilot.Racer=race.Racers[0];car.GetComponent<VehicleInput>().enabled=false;
   var touch=car.gameObject.AddComponent<Touch087>();float tA=-1,tB=-1,t0=Time.time,maxAir=0,air=0;bool forced=false,mistakeDone=false;Vector3 airAt=default;string seen="";float lastS=0;var trace=new StringBuilder("t,x,y,z,speed,mainS,mainLat,branch,branchS,branchLat,throttle,brake,target,wheels\n");
   while(flow.State!=RaceFlow.Stage.Results&&Time.time-t0<240){AudioListener.volume=0;
    var p=car.Body.position;float ms=rd.ProjectNear(p,lastS,120,out float mlat);lastS=ms;
    if(tA<0&&ms>=1826&&ms<1900&&mlat<8)tA=Time.time;if(tA>=0&&tB<0&&ms>=2110&&ms<2160&&mlat<8)tB=Time.time;
    if(mode!="main"&&!forced&&ms>1700&&ms<wr.entryRoad-6){PlannedField.SetValue(pilot,wr);forced=true;}
    if(race.Racers[0].Branch.Route)seen=race.Racers[0].Branch.Route.title;
    if(ms>1790&&ms<2140){float bs2=wr.Project(p,out float bl2);trace.AppendLine($"{Time.time-t0:F2},{p.x:F2},{p.y:F2},{p.z:F2},{car.ForwardSpeed:F2},{ms:F1},{mlat:F1},{(race.Racers[0].Branch.Route?race.Racers[0].Branch.Route.title:"main")},{bs2:F1},{bl2:F2},{pilot.LastThrottle:F2},{pilot.LastBrake:F2},{pilot.TargetSpeed:F1},{car.GroundedWheels},{pilot.LastObstacle}");}
    if(car.GroundedWheels==0){air+=Time.deltaTime;if(air>maxAir){maxAir=air;airAt=p;}}else air=0;
    if((mode=="kink1"||mode=="crest"||mode=="late1"||mode=="late2")&&!mistakeDone&&race.Racers[0].Branch.Route==wr){float bs=wr.Project(p,out _);float from=mode=="kink1"||mode=="late1"?56:mode=="late2"?128:150;float hold=mode.StartsWith("late")?.45f:6;
     if(bs>=from){mistakeDone=true;pilot.enabled=false;var head=Vector3.ProjectOnPlane(car.transform.forward,Vector3.up).normalized;int hits=touch.hits.Count;float m0=Time.time;
      while(Time.time-m0<hold){var q=car.Body.position;if(touch.hits.Count>hits||wr.Project(q,out _)>from+38)break;float ang=Vector3.SignedAngle(Vector3.ProjectOnPlane(car.transform.forward,Vector3.up),head,Vector3.up);car.Simulate(1,0,car.GroundedWheels>0?Mathf.Clamp(ang/25f,-1,1):0,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();}
      if(car.ForwardSpeed<4){float b0=Time.time;while(Time.time-b0<1.2f){car.Simulate(0,1,mode=="crest"?1:-1,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();}}
      Note($"  {profile} {mode}: the mistake ended {Time.time-m0:F1} s after branch s {from} at {V(car.Body.position)}, speed {car.ForwardSpeed:F1}; hits {string.Join("; ",touch.hits.Skip(hits))}");pilot.enabled=true;}}
    yield return null;}
   var r=race.Racers[0];string lap=r.Progress.Finished?RaceHud.FormatTime(r.Progress.AdjustedTime(race.Clock)):"not finished";float lapS=r.Progress.Finished?(float)r.Progress.AdjustedTime(race.Clock):-1;
   File.WriteAllText($"{output}/B-lap-{profile}-{mode}.csv",trace.ToString());Destroy(pilot);Destroy(touch);car.GetComponent<VehicleInput>().enabled=true;wr.aiValidated=true;
   var line=$"{profile} {mode}: lap {lap}, main s 1826 -> 2110 {(tA>=0&&tB>=0?(tB-tA).ToString("F2")+" s":"n/a")}, branch taken '{seen}', missed gates {r.Progress.MissedGates}, resets {r.Recoveries}, autopilot recoveries {pilot.RecoveryCount}, longest air {maxAir:F2} s{(maxAir>.4f?" at "+V(airAt):"")}, hit: {(touch.hits.Count==0?"nothing":string.Join("; ",touch.hits.Take(5)))}";
   SummitTable.Add($"{profile}	{mode}	{lapS:F2}	{(tA>=0&&tB>=0?(tB-tA).ToString("F2"):"")}	{r.Recoveries}	{line}");File.WriteAllLines(output+"/summit-table.tsv",SummitTable);
   Check(r.Progress.Finished&&r.Progress.MissedGates==0,line);yield return Menu();}}

 // Part A: Kyle's house: the photo's angle (front, from the right), the left side (garage doors), the back (deck), the
 // right side (screened porch), from the road; poses in the house frame. kyleshots87:scene:day|night
 IEnumerator KyleShots087(string scene,string when){if(scene==RaceFlow.RoamScene)yield return EnterRoam("StreetLoopGreybox","moto");else yield return EnterScene(scene);yield return new WaitForSeconds(1.5f);
  if(when=="night"){if(WorldLook.Current)WorldLook.Current.Pin(LookPresets.Compose(TimeOfDay.Night,Weather.Clear));}else Day();
  var site=GameObject.Find("Friend across street - blue circle")?.transform;if(!site){Note("no Kyle's house in "+scene);yield break;}
  var cam=Camera.main;var chase=FindAnyObjectByType<ChaseCamera>();if(chase)chase.enabled=false;var cv=CameraViews.Current;if(cv)cv.enabled=false;var hud=FindObjectsByType<Canvas>(FindObjectsSortMode.None);foreach(var c in hud)c.enabled=false;
  if(race&&race.vehicle)race.vehicle.gameObject.SetActive(false);yield return new WaitForSeconds(.5f);
  var views=new List<(string n,Vector3 eye,Vector3 look)>{("A-kyle-photo-angle",new(-5.5f,4.3f,13.5f),new(-.5f,3.4f,3f)),("A-kyle-left-garage",new(27f,4.2f,9.5f),new(7.6f,1.0f,-1.6f)),
   ("A-kyle-back-deck",new(-1f,6.8f,-16f),new(-1f,2.4f,-4f)),("A-kyle-right-screened-porch",new(-25f,4.6f,4f),new(-9.5f,3.2f,-1f)),("A-kyle-from-the-road",new(0f,12.5f,37f),new(0f,3.5f,0f)),("A-kyle-from-the-drive-entrance",new(40f,7f,30f),new(2f,3.5f,0f))};
  foreach(var v in views)yield return Late(()=>{cam.transform.position=site.TransformPoint(v.eye);cam.transform.LookAt(site.TransformPoint(v.look));Shot(v.n+(when=="night"?"-night":""));});
  if(race&&race.vehicle)race.vehicle.gameObject.SetActive(true);foreach(var c in hud)if(c)c.enabled=true;if(chase)chase.enabled=true;if(cv)cv.enabled=true;Note($"Kyle's house views ({scene}, {when}): {string.Join(", ",views.Select(v=>v.n))}");
  if(scene!=RaceFlow.RoamScene)yield return Menu();}

 // Part A: down Kyle's driveway from the street to the garage doors in Free Roam, following the drive's route points at up
 // to 9 m/s and stopping on the apron: reaches it, what it touched, the steepest pitch. kyledrive87:profiles
 IEnumerator KyleDrive087(string profiles){foreach(var profile in profiles.Split(',')){yield return EnterRoam("StreetLoopGreybox",profile);yield return new WaitForSeconds(1);
  var road=GameObject.Find("Kyle descending driveway")?.GetComponent<RaceRoad>();if(!road){Note("no Kyle driveway");yield break;}var path=new Path087(road.points);var car=race.vehicle;
  car.GetComponent<VehicleInput>().enabled=false;car.enabled=false;var resp=car.GetComponent<VehicleRespawn>();int resets=0;Action onR=()=>resets++;resp.Respawned+=onR;var touch=car.gameObject.AddComponent<Touch087>();
  var start=path.At(1);var f0=path.At(4)-start;f0.y=0;start.y=Ground085(start,start.y+5);var rot=Quaternion.LookRotation(f0.normalized);car.Body.position=start+Vector3.up*(car.suspensionLength*.7f);car.Body.rotation=rot;car.transform.SetPositionAndRotation(car.Body.position,rot);car.Body.linearVelocity=Vector3.zero;car.Body.angularVelocity=Vector3.zero;car.ClearSteering();resp.SeedCoursePosition(start);
  for(int i=0;i<25;i++){car.Simulate(0,1,0,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();}
  float t0=Time.time,d=0,maxPitch=0;bool reached=false;
  while(Time.time-t0<40&&resets==0){AudioListener.volume=0;var p=car.Body.position;d=path.Project(p,d,20,out float lat);if(d>=path.Length-1.5f){reached=true;break;}
   var tgt=path.At(d+5);var to=tgt-p;to.y=0;float ang=Vector3.SignedAngle(Vector3.ProjectOnPlane(car.transform.forward,Vector3.up),to,Vector3.up);float v=car.ForwardSpeed;float lim=d>path.Length-12?4:9;
   car.Simulate(v<lim?.6f:0,v>lim+1?.6f:0,Mathf.Clamp(ang/25f,-1,1),Time.fixedDeltaTime);yield return new WaitForFixedUpdate();maxPitch=Mathf.Max(maxPitch,Vector3.Angle(car.transform.forward,Vector3.ProjectOnPlane(car.transform.forward,Vector3.up)));}
  for(int i=0;i<60;i++){car.Simulate(0,1,0,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();}
  var site=GameObject.Find("Friend across street - blue circle").transform;var l=site.InverseTransformPoint(car.Body.position);
  resp.Respawned-=onR;Destroy(touch);car.enabled=true;car.GetComponent<VehicleInput>().enabled=true;
  Check(reached&&resets==0&&touch.hits.Count==0,$"{profile} down Kyle's driveway ({path.Length:F0} m) to the garage: {(reached?$"on the apron in {Time.time-t0:F1} s, stopped at house-frame {V(l)} ({l.x-7.6f:F1} m from the garage doors)":"NOT reached")}, resets {resets}, steepest pitch {maxPitch:F0} deg, touched {(touch.hits.Count==0?"nothing":string.Join("; ",touch.hits.Take(3)))}");
  yield return Late(()=>{var cam=Camera.main;var ch=FindAnyObjectByType<ChaseCamera>();if(ch)ch.enabled=false;cam.transform.position=site.TransformPoint(new Vector3(24f,4.6f,4f));cam.transform.LookAt(site.TransformPoint(new Vector3(8f,1.2f,-1.5f)));Shot("A-kyle-drive-"+profile+"-at-the-garage");if(ch)ch.enabled=true;});}}
}
}
#endif
