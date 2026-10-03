#if UNITY_EDITOR
using System;using System.IO;using System.Linq;using System.Collections;using System.Collections.Generic;using System.Reflection;using UnityEngine;
namespace Racer {
// 0.71 targeted checks (muted). PROBE_CASES="case:args;..." per scene:
//  homeward:profile:lat          Free Roam, Summit Homeward Flight: from rest at run-up s 5 (lat m right of the centre line),
//                                full throttle to the lip, then no input after touchdown (coasting run-out). Logs take-off
//                                speed, flight distance, airtime, landing place/ground, the run-out and the activity result;
//  ride:profile:dir              Free Roam, the straight path between the 0.70 report positions (dir 1: (761.9, 80.5) up to
//                                (895.8, 154.3); -1: down), ~12 m/s, steering along the straight line;
//  enter:route:mainS0:speed:lat:profile, line:route:s0:s1:profile, aienter:route:mainS0:profile  as in the 0.70 checks;
//  cut:route:mainS0:speed:lat:profile  as enter, but the line cuts the inside corner: aims at a point lat m right of the branch
//                                centre at branch s 0 straight from the main road (no lead-in along the branch).
public sealed class Report071Checks:MonoBehaviour {
 RaceDirector race;ArcadeVehicle car;VehicleRespawn respawn;readonly List<string> rows=new();string output;int resets,runs;
 void Note(string s){rows.Add(s);File.WriteAllLines(output+"/results.txt",rows);Debug.Log("REPORT071 "+s);}
 IEnumerator Start(){AudioListener.volume=0;var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;output=Environment.GetEnvironmentVariable("PROBE_OUT")+"/checks-"+scene;Directory.CreateDirectory(output);Application.runInBackground=true;race=FindAnyObjectByType<RaceDirector>();yield return null;yield return null;
  race.Flow.UseValidationSave(Path.GetFullPath("Temp/Report071Save"));
  var title=FindAnyObjectByType<StartupTitle>();if(title)Destroy(title.gameObject);typeof(StartupTitle).GetField("completed",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,true);race.Flow.EnterMenuAfterTitle();
  car=race.vehicle;respawn=car.GetComponent<VehicleRespawn>();respawn.Respawned+=()=>resets++;
  foreach(var spec in (Environment.GetEnvironmentVariable("PROBE_CASES")??"").Split(';').Where(x=>x.Length>0)){var a=spec.Split(':');
   string profile=a[0] switch{"homeward"=>a[1],"ride"=>a[1],"line"=>a[4],"enter"=>a[5],"cut"=>a[5],"aienter"=>a[3],"cutxy"=>a[5],"overrun"=>a[4],_=>"moto"};bool roam=a[0]=="homeward"||a[0]=="ride";
   yield return roam?BeginRoam(profile):Begin(profile);AudioListener.volume=0;
   IEnumerator run=a[0] switch{"homeward"=>Homeward(float.Parse(a[2])),"ride"=>Ride(int.Parse(a[2])),"line"=>Line(a[1],float.Parse(a[2]),float.Parse(a[3])),"aienter"=>AiEnter(a[1],float.Parse(a[2])),"enter"=>Enter(a[1],float.Parse(a[2]),float.Parse(a[3]),float.Parse(a[4]),false),"cut"=>Enter(a[1],float.Parse(a[2]),float.Parse(a[3]),float.Parse(a[4]),true),"cutxy"=>CutXY(a[1],V2(a[2]),V2(a[3]),float.Parse(a[4])),"overrun"=>Overrun(a[1],float.Parse(a[2]),float.Parse(a[3])),_=>null};
   if(run!=null)yield return run;else Note("unknown case "+spec);}
  File.WriteAllLines(output+"/done.txt",rows);UnityEditor.EditorApplication.Exit(0);}
 IEnumerator Begin(string profile){var flow=race.Flow;if(flow.State!=RaceFlow.Stage.Ready){flow.Pause();flow.QuitRace();}yield return null;
  flow.OpenGarage();flow.SelectVehicle(profile);flow.CloseGarage();race.opponents=race.traffic=false;race.laps=1;flow.StartRace();
  while(flow.State!=RaceFlow.Stage.Racing){AudioListener.volume=0;yield return null;}Time.timeScale=1;yield return new WaitForSeconds(.5f);}
 IEnumerator BeginRoam(string profile){var flow=race.Flow;if(flow.State!=RaceFlow.Stage.Ready){flow.Pause();flow.QuitRace();}yield return null;
  flow.OpenGarage();flow.SelectVehicle(profile);flow.CloseGarage();race.opponents=race.traffic=false;flow.StartFreeRoam();
  float t0=Time.time;while(flow.State!=RaceFlow.Stage.Racing&&Time.time-t0<10){AudioListener.volume=0;yield return null;}Time.timeScale=1;yield return new WaitForSeconds(.5f);}
 string Profile=>car.GetComponent<VehicleConfiguration>().profileId;
 bool Wiped=>car.GetComponent<VehicleConfiguration>().WipedOut;
 void Seed(float from){var state=race.Racers[0];state.Branch.Clear();race.Progress.Restart();race.Progress.Cross(0,true,race.Clock);float prior=race.road.Relative(from,race.Origin);for(int g=1;g<race.gates.Length;g++)if(race.road.Relative(race.road.Project(race.gates[g].transform.position,out _),race.Origin)<prior)race.Progress.Cross(g,true,race.Clock);}
 void Put(Vector3 p,Quaternion r,Vector3 v){car.Body.isKinematic=false;car.transform.SetPositionAndRotation(p,r);car.Body.position=p;car.Body.rotation=r;car.Body.linearVelocity=v;car.Body.angularVelocity=Vector3.zero;car.ClearSteering();Physics.SyncTransforms();race.ResetSampling(p,race.Clock);respawn.SeedCoursePosition(p);}
 Vector3 RAt(string route,float s,out Vector3 f){if(route=="Main")return race.road.At(s,out f);var b=race.Branches.First(x=>x.title==route);return b.At(s,out f);}
 float RProj(string route,Vector3 p,float near,out float lat){if(route=="Main")return race.road.ProjectNear(p,near,40,out lat);var b=race.Branches.First(x=>x.title==route);return b.Project(p,out lat);}
 Vector3 Right(string route,float s){RAt(route,s,out var f);return Vector3.Cross(Vector3.up,Vector3.ProjectOnPlane(f,Vector3.up).normalized);}
 void PutOn(string route,float s,float speed,float lat=0){var c=RAt(route,s,out var f);f=Vector3.ProjectOnPlane(f,Vector3.up).normalized;var q=c+Right(route,s)*lat;if(Physics.Raycast(q+Vector3.up*4,Vector3.down,out var h,10,~0,QueryTriggerInteraction.Ignore))q=h.point;Put(q+Vector3.up*.7f,Quaternion.LookRotation(f),f*speed);}
 float Steer(Vector3 target){var d=Quaternion.Inverse(Quaternion.LookRotation(Vector3.ProjectOnPlane(car.transform.forward,Vector3.up)))*(target-car.Body.position);return Mathf.Clamp(Mathf.Atan2(d.x,d.z)*1.6f,-1,1);}
 static bool Ground(Vector3 p,out RaycastHit hit){var o=p+Vector3.up*3;hit=default;for(int k=0;k<8&&Physics.Raycast(o,Vector3.down,out var h,60,~0,QueryTriggerInteraction.Ignore);k++){if(!h.collider.attachedRigidbody&&h.normal.y>0){hit=h;return true;}o=h.point+Vector3.down*.02f;}return false;}

 sealed class AllContacts:MonoBehaviour{public readonly List<(float imp,string name,Vector3 p,Vector3 n,float t)> hits=new();
  void Record(Collision c){if(c.impulse.magnitude<1)return;var cp=c.contactCount>0?c.GetContact(0):default;hits.Add((c.impulse.magnitude,c.collider.name,cp.point,cp.normal,Time.time));}
  void OnCollisionEnter(Collision c)=>Record(c);void OnCollisionStay(Collision c)=>Record(c);}

 // ---------- Part A ----------
 IEnumerator Homeward(float lat){var root=GameObject.Find("CR094 summit launch").transform;var act=race.Flow.Activities;var ac=car.gameObject.AddComponent<AllContacts>();car.enabled=false;
  var s0=root.TransformPoint(new Vector3(lat,0,5));if(Ground(s0+Vector3.up*10,out var gh))s0=gh.point;Put(s0+Vector3.up*.7f,Quaternion.LookRotation(root.forward),Vector3.zero);
  for(float t=0;t<1.5f;t+=Time.fixedDeltaTime){yield return new WaitForFixedUpdate();car.Simulate(0,1,0,Time.fixedDeltaTime);}
  int r0=resets;float t0=Time.time;bool air=false,landed=false;Vector3 takeoff=default,land=default;float vTake=0,airT=0,maxY=-1e9f,vLand=0,landT=0,minUpAfter=1,stillFor=0,rolled=0;string landOn="",end="time";Vector3 last=car.Body.position;string obstacle="";int contactsBefore=0;
  using(var log=new StreamWriter($"{output}/homeward-{Profile}-{lat}-{runs++}.csv")){log.WriteLine("t,x,s,y,speed,wheels,upY");
   while(Time.time-t0<60){yield return new WaitForFixedUpdate();var p=car.Body.position;var q=root.InverseTransformPoint(p);
    if(!landed)car.Simulate(1,0,Steer(root.TransformPoint(new Vector3(lat,0,q.z+25))),Time.fixedDeltaTime);
    else car.Simulate(0,0,Steer(p+Vector3.ProjectOnPlane(car.Body.linearVelocity,Vector3.up).normalized*20),Time.fixedDeltaTime);
    log.WriteLine($"{Time.time-t0:F2},{q.x:F2},{q.z:F2},{p.y:F2},{car.Body.linearVelocity.magnitude:F1},{car.GroundedWheels},{car.transform.up.y:F2}");
    if(!air&&q.z>150&&car.GroundedWheels==0){air=true;takeoff=p;vTake=car.Body.linearVelocity.magnitude;contactsBefore=ac.hits.Count;}
    if(air&&!landed){airT+=Time.fixedDeltaTime;maxY=Mathf.Max(maxY,p.y);if(car.GroundedWheels>=1&&airT>.4f){landed=true;land=p;vLand=car.Body.linearVelocity.magnitude;landT=Time.time;if(Ground(p,out var lh))landOn=lh.collider.name;}}
    if(landed){minUpAfter=Mathf.Min(minUpAfter,car.transform.up.y);rolled+=Vector3.Distance(Vector3.ProjectOnPlane(p,Vector3.up),Vector3.ProjectOnPlane(last,Vector3.up));
     foreach(var h in ac.hits.Skip(contactsBefore))if(obstacle==""&&h.name.IndexOf("Ground",StringComparison.Ordinal)<0&&h.imp>50)obstacle=$"{h.name} imp {h.imp:F0} at local s {root.InverseTransformPoint(h.p).z:F0}";contactsBefore=ac.hits.Count;
     if(car.Body.linearVelocity.magnitude<1){stillFor+=Time.fixedDeltaTime;if(stillFor>1){end="stopped";break;}}else stillFor=0;
     if(Wiped){end="wiped out";break;}if(Time.time-landT>30){end="still rolling after 30 s";break;}}
    if(resets>r0){end="reset";break;}if(p.y<-10){end="fell";break;}last=p;}}
  car.enabled=true;var e=car.Body.position;var eq=root.InverseTransformPoint(e);var lq=root.InverseTransformPoint(land);var tq=root.InverseTransformPoint(takeoff);
  yield return new WaitForSeconds(.2f);
  Note($"homeward {Profile} lat {lat:+0;-0}: take-off local s {tq.z:F1} at {vTake:F1} m/s; airtime {airT:F2} s; peak y {maxY:F1}; flight distance {Vector3.ProjectOnPlane(land-takeoff,Vector3.up).magnitude:F1} m; landing local x {lq.x:F1} s {lq.z:F1} y {land.y:F1} on '{landOn}' at {vLand:F1} m/s; run-out {rolled:F0} m -> end local x {eq.x:F1} s {eq.z:F1} ({end}); first obstacle: {(obstacle==""?"none":obstacle)}; min up after landing {minUpAfter:F2}; resets {resets-r0}; activity: LastDistance {act.LastDistance:F1} LastJumpAward {act.LastJumpAward:F1} feedback '{act.Feedback?.Replace('\n','/')}' diag '{act.LastJumpDiagnostic}'");
  Destroy(ac);}
 IEnumerator Ride(int dir){var A=new Vector3(761.93f,0,80.45f);var B=new Vector3(895.79f,0,154.34f);if(dir<0){var x=A;A=B;B=x;}var d=(B-A);d.y=0;float len=d.magnitude;d/=len;car.enabled=false;
  var s0=A;if(Physics.Raycast(new Vector3(A.x,600,A.z),Vector3.down,out var gh,1200,~0,QueryTriggerInteraction.Ignore))s0=gh.point;Put(s0+Vector3.up*.7f,Quaternion.LookRotation(d),d*6);
  int r0=resets;float t0=Time.time,best=0,stuck=0,maxStuck=0,minUp=1,maxGrade=0;string end="time";var prev=car.Body.position;
  using(var log=new StreamWriter($"{output}/ride-{Profile}-{dir}-{runs++}.csv")){log.WriteLine("t,along,off,y,speed,wheels,upY");
   while(Time.time-t0<45){yield return new WaitForFixedUpdate();var p=car.Body.position;var rel=p-s0;rel.y=0;float along=Vector3.Dot(rel,d);float off=Vector3.Dot(rel,Vector3.Cross(Vector3.up,d));best=Mathf.Max(best,along);
    var tgt=new Vector3(s0.x,0,s0.z)+d*(Mathf.Max(best,along)+12);tgt.y=p.y;car.Simulate(car.ForwardSpeed<12?1:0,0,Steer(tgt),Time.fixedDeltaTime);
    minUp=Mathf.Min(minUp,car.transform.up.y);if(car.ForwardSpeed<1.5f){stuck+=Time.fixedDeltaTime;maxStuck=Mathf.Max(maxStuck,stuck);}else stuck=0;
    float hz=Vector3.ProjectOnPlane(p-prev,Vector3.up).magnitude;if(hz>.05f&&car.GroundedWheels>=2)maxGrade=Mathf.Max(maxGrade,Mathf.Abs(p.y-prev.y)/hz);prev=p;
    log.WriteLine($"{Time.time-t0:F2},{along:F1},{off:F1},{p.y:F2},{car.ForwardSpeed:F1},{car.GroundedWheels},{car.transform.up.y:F2}");
    if(along>=len){end="complete";break;}if(maxStuck>4){end="blocked";break;}if(resets>r0){end="reset";break;}if(Wiped){end="wiped out";break;}}}
  car.enabled=true;Note($"ride straight path {(dir>0?"up (0.70 BUG-002 -> BUG-001)":"down (BUG-001 -> BUG-002)")} ({Profile}): {end}; furthest {best:F0} / {len:F0} m; longest stop {maxStuck:F1} s; steepest grade {Mathf.Atan(maxGrade)*Mathf.Rad2Deg:F0} deg; min up {minUp:F2}; resets {resets-r0}; end y {car.Body.position.y:F1}");}

 // ---------- Part B (0.70 harness, plus the corner-cutting line) ----------
 sealed class BermContacts:MonoBehaviour{public int hits;public float maxImpulse;void OnCollisionEnter(Collision c){if(c.collider.name.Contains(" barrier ")){hits++;maxImpulse=Mathf.Max(maxImpulse,c.impulse.magnitude);}}}
 IEnumerator Line(string route,float s0,float s1){if(route=="Main")Seed(s0);var bc=car.gameObject.AddComponent<BermContacts>();car.enabled=false;PutOn(route,s0,18);float t0=Time.time,best=s0,air=0,maxAir=0,minUp=1;bool done=false;int r0=resets;
  while(Time.time-t0<60){yield return new WaitForFixedUpdate();float s=RProj(route,car.Body.position,best,out _);if(car.GroundedWheels>=2)best=Mathf.Max(best,s);car.Simulate(1,0,Steer(RAt(route,best+14,out _)),Time.fixedDeltaTime);
   minUp=Mathf.Min(minUp,car.transform.up.y);if(car.GroundedWheels==0){air+=Time.fixedDeltaTime;maxAir=Mathf.Max(maxAir,air);}else air=0;if(best>=s1){done=true;break;}if(car.Body.position.y<-10||resets>r0)break;}
  car.enabled=true;Note($"clean line {route} s {s0}-{s1} ({Profile}): complete={done} best={best:F1} berm contacts={bc.hits} maxAir={maxAir:F2} minUp={minUp:F2} resets={resets-r0}");Destroy(bc);}
 IEnumerator AiEnter(string route,float s0){var branch=race.Branches.First(b=>b.title==route);Seed(s0);foreach(var b in race.Branches)b.aiValidated=b==branch;
  var pilot=car.gameObject.AddComponent<RoadDriver>();pilot.Initialize(race,car,true,1,1);pilot.Racer=race.Racers[0];pilot.Place(s0,0);
  typeof(RoadDriver).GetField("plannedBranch",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(pilot,branch);race.ResetSampling(car.Body.position,race.Clock);
  int r0=resets;float t0=Time.time,best=0,air=0,maxAir=0,minUp=1;bool entered=false,rejoined=false;
  while(Time.time-t0<120){AudioListener.volume=0;yield return new WaitForFixedUpdate();float s=branch.Project(car.Body.position,out _);bool on=race.Racers[0].Branch.Route==branch;if(on){entered=true;best=Mathf.Max(best,s);}
   minUp=Mathf.Min(minUp,car.transform.up.y);if(car.GroundedWheels==0){air+=Time.fixedDeltaTime;maxAir=Mathf.Max(maxAir,air);}else air=0;
   if(entered&&best>branch.Length-15&&!on){rejoined=true;break;}if(pilot.RecoveryCount>0||resets>r0)break;}
  Note($"AI {route} from main s {s0} ({Profile}): entered={entered} rejoined={rejoined} best={best:F1}/{branch.Length:F1} recoveries={pilot.RecoveryCount} resets={resets-r0} maxAir={maxAir:F2} minUp={minUp:F2}");
  Destroy(pilot);foreach(var b in race.Branches)b.aiValidated=false;yield return null;}
 IEnumerator Enter(string route,float s0,float speed,float latOff,bool cut){Seed(s0);var ac=car.gameObject.AddComponent<AllContacts>();car.enabled=false;PutOn("Main",s0,speed);
  var b=race.Branches.First(x=>x.title==route);var b0=b.At(0,out var bf0);var bRight=Vector3.Cross(Vector3.up,Vector3.ProjectOnPlane(bf0,Vector3.up).normalized);float t0=Time.time,bestM=s0,bestB=-1,minUp=1,maxRoll=0,maxAng=0;Vector3 angAt=default;float angS=-1;bool done=false;int r0=resets;
  using(var log=new StreamWriter($"{output}/{(cut?"cut":"enter")}-{route.Replace(' ','_')}-{s0}-{speed}-{latOff}-{Profile}-{runs++}.csv")){log.WriteLine("t,bs,blat,x,y,z,speed,wheels,upY,roll,ang");
   while(Time.time-t0<14){yield return new WaitForFixedUpdate();var p=car.Body.position;float bs=b.Project(p,out float bl);
    if(bestB<0&&Vector3.Distance(p,b0)<(cut?6:10))bestB=0;if(bestB>=0&&bs>bestB-5)bestB=Mathf.Max(bestB,bs);else bestM=Mathf.Max(bestM,race.road.ProjectNear(p,bestM,40,out _));
    Vector3 tgt;if(bestB<0)tgt=cut?b0+bRight*latOff+Vector3.ProjectOnPlane(bf0,Vector3.up).normalized*4:Vector3.Lerp(race.road.At(bestM+10,out _),b0,Mathf.Clamp01(1-(Vector3.Distance(p,b0)-10)/20));else{var c=b.At(bestB+8,out var f);tgt=c+Vector3.Cross(Vector3.up,Vector3.ProjectOnPlane(f,Vector3.up).normalized)*(cut?0:latOff);}
    car.Simulate(car.ForwardSpeed<speed?1:0,0,Steer(tgt),Time.fixedDeltaTime);
    float roll=Vector3.SignedAngle(Vector3.ProjectOnPlane(Vector3.up,car.transform.forward),car.transform.up,car.transform.forward);float ang=car.Body.angularVelocity.magnitude;
    minUp=Mathf.Min(minUp,car.transform.up.y);maxRoll=Mathf.Max(maxRoll,Mathf.Abs(roll));if(ang>maxAng){maxAng=ang;angAt=p;angS=bestB;}
    log.WriteLine($"{Time.time-t0:F2},{bs:F2},{bl:F2},{p.x:F2},{p.y:F2},{p.z:F2},{car.ForwardSpeed:F1},{car.GroundedWheels},{car.transform.up.y:F2},{roll:F0},{ang:F2}");
    if(bestB>=40){done=true;break;}if(p.y<-10||resets>r0)break;}}
  car.enabled=true;var top=ac.hits.OrderByDescending(h=>h.imp).Take(6).Select(h=>$"{h.name}@{h.p:F1} n{h.n:F2} imp {h.imp:F0}");
  Note($"{(cut?"corner-cut":"enter")} {route} from main s {s0} at {speed} m/s, line {latOff:+0.0;-0.0} m ({Profile}): complete={done} branch s {bestB:F1}; minUp={minUp:F2} maxRoll={maxRoll:F0} deg; max angular velocity {maxAng:F2} rad/s at {angAt:F1} (branch s {angS:F1}); resets={resets-r0}; body contacts {ac.hits.Count}: {string.Join("; ",top)}");Destroy(ac);}
 static Vector2 V2(string t){var q=t.Split(',').Select(float.Parse).ToArray();return new Vector2(q[0],q[1]);}
 // Corner-cutting line: from (x0,z0) at speed straight at (x1,z1) across whatever lies between, then along the branch to s 40.
 IEnumerator CutXY(string route,Vector2 from,Vector2 to,float speed){Seed(0);var ac=car.gameObject.AddComponent<AllContacts>();car.enabled=false;var b=race.Branches.First(x=>x.title==route);
  var p0=new Vector3(from.x,0,from.y);if(Physics.Raycast(new Vector3(from.x,600,from.y),Vector3.down,out var h0,1200,~0,QueryTriggerInteraction.Ignore))p0=h0.point;var dir=new Vector3(to.x-from.x,0,to.y-from.y).normalized;Put(p0+Vector3.up*.7f,Quaternion.LookRotation(dir),dir*speed);
  var tgt0=new Vector3(to.x,p0.y,to.y);bool reached=false;float t0=Time.time,bestB=-1,minUp=1,maxRoll=0,maxAng=0;Vector3 angAt=default;int r0=resets;bool done=false;
  using(var log=new StreamWriter($"{output}/cutxy-{from.x}_{from.y}-{to.x}_{to.y}-{speed}-{Profile}-{runs++}.csv")){log.WriteLine("t,bs,x,y,z,speed,wheels,upY,roll,ang");
   while(Time.time-t0<14){yield return new WaitForFixedUpdate();var p=car.Body.position;float bs=b.Project(p,out float bl);
    if(!reached&&(new Vector2(p.x-to.x,p.z-to.y).magnitude<4||Vector3.Dot(new Vector3(to.x-p.x,0,to.y-p.z),dir)<0)){reached=true;bestB=bs;}
    if(reached)bestB=Mathf.Max(bestB,bs);
    var tgt=reached?b.At(bestB+8,out _):tgt0;car.Simulate(car.ForwardSpeed<speed?1:0,0,Steer(tgt),Time.fixedDeltaTime);
    float roll=Vector3.SignedAngle(Vector3.ProjectOnPlane(Vector3.up,car.transform.forward),car.transform.up,car.transform.forward);float ang=car.Body.angularVelocity.magnitude;
    minUp=Mathf.Min(minUp,car.transform.up.y);maxRoll=Mathf.Max(maxRoll,Mathf.Abs(roll));if(ang>maxAng){maxAng=ang;angAt=p;}
    log.WriteLine($"{Time.time-t0:F2},{bs:F2},{p.x:F2},{p.y:F2},{p.z:F2},{car.ForwardSpeed:F1},{car.GroundedWheels},{car.transform.up.y:F2},{roll:F0},{ang:F2}");
    if(bestB>=40){done=true;break;}if(p.y<-10||resets>r0)break;}}
  car.enabled=true;var top=ac.hits.Where(h=>!h.name.Contains("driving surface")).OrderByDescending(h=>h.imp).Take(5).Select(h=>$"{h.name}@{h.p:F1} n{h.n:F2} imp {h.imp:F0}");
  Note($"corner-cut ({from.x},{from.y})->({to.x},{to.y}) at {speed} m/s ({Profile}): complete={done} branch s {bestB:F1}; minUp={minUp:F2} maxRoll={maxRoll:F0} deg; max ang vel {maxAng:F2} at {angAt:F1}; resets={resets-r0}; non-pavement contacts {ac.hits.Count(h=>!h.name.Contains("driving surface"))}: {string.Join("; ",top)}");Destroy(ac);}
 IEnumerator Overrun(string route,float s,float speed){if(route=="Main")Seed(s);var bc=car.gameObject.AddComponent<BermContacts>();car.enabled=false;PutOn(route,s,speed);float t0=Time.time,air=0,maxAir=0,minUp=1,minY=1e9f;int r0=resets;var start=car.Body.position;
  while(Time.time-t0<5){yield return new WaitForFixedUpdate();car.Simulate(1,0,0,Time.fixedDeltaTime);minY=Mathf.Min(minY,car.Body.position.y);minUp=Mathf.Min(minUp,car.transform.up.y);if(car.GroundedWheels==0){air+=Time.fixedDeltaTime;maxAir=Mathf.Max(maxAir,air);}else air=0;if(car.Body.position.y<-10||resets>r0)break;}
  car.enabled=true;var p=car.Body.position;float mainS=race.road.ProjectNear(p,race.road.Project(start,out _),80,out float mainLat);bool onPave=Physics.Raycast(p+Vector3.up*.5f,Vector3.down,out var h,3,~0,QueryTriggerInteraction.Ignore)&&h.collider.name.Contains("driving surface");
  Note($"overshoot {route} from s {s} at {speed} m/s, no steering ({Profile}): berm contacts={bc.hits} (max impulse {bc.maxImpulse:F0}); end {p:F1} main s {mainS:F1} lat {mainLat:F1}; on pavement={onPave}; dropped {start.y-minY:F1} m below the start; maxAir={maxAir:F2} minUp={minUp:F2} speed {car.Body.linearVelocity.magnitude:F1}");Destroy(bc);}
}
}
#endif
