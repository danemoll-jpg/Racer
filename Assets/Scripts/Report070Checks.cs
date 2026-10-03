#if UNITY_EDITOR
using System;using System.IO;using System.Linq;using System.Collections;using System.Collections.Generic;using System.Reflection;using UnityEngine;
namespace Racer {
// 0.70 targeted checks (muted). PROBE_CASES="case:args;..." per scene:
//  offon:route:s:side:profile:speed  ride off the pavement edge at s (side -1 left / 1 right) and straight back on;
//  line:route:s0:s1:profile          full-throttle clean line (berm contacts reported);
//  overrun:route:s:speed:profile     full throttle, no steering, from s along the route tangent (is the rider kept on?);
//  reset:s:side:profile              drive up to s, leave the vehicle off-track beside the edge, press reset;
//  climb:route:s0:s1:profile:speed    full throttle from s0 (start speed) steering along the route, logging height above the
//                                      road, lateral offset and speed every step from s0 to s1 (Dan's crest approach, 0.70 Part C);
//  enter:route:mainS0:speed:lat:profile  from main s0 at speed, steer into the branch on a line lat m right of its centre,
//                                      hold speed to branch s 40; logs body contacts and roll / angular-velocity spikes (0.70 Part E);
//  aienter:route:mainS0:profile        production AI (RoadDriver) with the branch planned, from main s0 (after the start line, so
//                                      the plan is not dropped as "past the exit"): does it enter, traverse and rejoin?
public sealed class Report070Checks:MonoBehaviour {
 RaceDirector race;ArcadeVehicle car;VehicleRespawn respawn;readonly List<string> rows=new();string output;int resets;
 void Note(string s){rows.Add(s);File.WriteAllLines(output+"/results.txt",rows);Debug.Log("REPORT070 "+s);}
 IEnumerator Start(){AudioListener.volume=0;var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;output=Environment.GetEnvironmentVariable("PROBE_OUT")+"/checks-"+scene;Directory.CreateDirectory(output);Application.runInBackground=true;race=FindAnyObjectByType<RaceDirector>();yield return null;yield return null;
  race.Flow.UseValidationSave(Path.GetFullPath("Temp/Report070Save"));
  var title=FindAnyObjectByType<StartupTitle>();if(title)Destroy(title.gameObject);typeof(StartupTitle).GetField("completed",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,true);race.Flow.EnterMenuAfterTitle();
  car=race.vehicle;respawn=car.GetComponent<VehicleRespawn>();respawn.Respawned+=()=>resets++;
  foreach(var spec in (Environment.GetEnvironmentVariable("PROBE_CASES")??"").Split(';').Where(x=>x.Length>0)){var a=spec.Split(':');
   string profile=a[0] switch{"offon"=>a[4],"line"=>a[4],"overrun"=>a[4],"reset"=>a[3],"climb"=>a[4],"enter"=>a[5],"aienter"=>a[3],_=>"moto"};yield return Begin(profile);AudioListener.volume=0;
   IEnumerator run=a[0] switch{"offon"=>OffOn(a[1],float.Parse(a[2]),int.Parse(a[3]),float.Parse(a[5])),"line"=>Line(a[1],float.Parse(a[2]),float.Parse(a[3])),"overrun"=>Overrun(a[1],float.Parse(a[2]),float.Parse(a[3])),"reset"=>Reset(float.Parse(a[1]),int.Parse(a[2])),"aienter"=>AiEnter(a[1],float.Parse(a[2])),"enter"=>Enter(a[1],float.Parse(a[2]),float.Parse(a[3]),float.Parse(a[4])),"climb"=>Climb(a[1],float.Parse(a[2]),float.Parse(a[3]),a.Length>5?float.Parse(a[5]):12),_=>null};
   if(run!=null)yield return run;else Note("unknown case "+spec);}
  File.WriteAllLines(output+"/done.txt",rows);UnityEditor.EditorApplication.Exit(0);}
 IEnumerator Begin(string profile){var flow=race.Flow;if(flow.State!=RaceFlow.Stage.Ready){flow.Pause();flow.QuitRace();}yield return null;
  flow.OpenGarage();flow.SelectVehicle(profile);flow.CloseGarage();race.opponents=race.traffic=false;race.laps=1;flow.StartRace();
  while(flow.State!=RaceFlow.Stage.Racing){AudioListener.volume=0;yield return null;}Time.timeScale=1;yield return new WaitForSeconds(.5f);}
 string Profile=>car.GetComponent<VehicleConfiguration>().profileId;
 void Seed(float from){var state=race.Racers[0];state.Branch.Clear();race.Progress.Restart();race.Progress.Cross(0,true,race.Clock);float prior=race.road.Relative(from,race.Origin);for(int g=1;g<race.gates.Length;g++)if(race.road.Relative(race.road.Project(race.gates[g].transform.position,out _),race.Origin)<prior)race.Progress.Cross(g,true,race.Clock);}
 void Put(Vector3 p,Quaternion r,Vector3 v){car.Body.isKinematic=false;car.transform.SetPositionAndRotation(p,r);car.Body.position=p;car.Body.rotation=r;car.Body.linearVelocity=v;car.Body.angularVelocity=Vector3.zero;car.ClearSteering();Physics.SyncTransforms();race.ResetSampling(p,race.Clock);respawn.SeedCoursePosition(p);}
 Vector3 RAt(string route,float s,out Vector3 f){if(route=="Main")return race.road.At(s,out f);var b=race.Branches.First(x=>x.title==route);return b.At(s,out f);}
 float RProj(string route,Vector3 p,float near,out float lat){if(route=="Main")return race.road.ProjectNear(p,near,40,out lat);var b=race.Branches.First(x=>x.title==route);return b.Project(p,out lat);}
 Vector3 Right(string route,float s){RAt(route,s,out var f);return Vector3.Cross(Vector3.up,Vector3.ProjectOnPlane(f,Vector3.up).normalized);}
 float SignedLat(string route,Vector3 p,float s){var c=RAt(route,s,out _);return Vector3.Dot(p-c,Right(route,s));}
 static bool IsDrive(Collider c)=>c&&c.name.Contains("driving surface");
 bool Pave(Vector3 q,float near,out float y){y=0;var o=new Vector3(q.x,near+.6f,q.z);float depth=1.2f;for(int k=0;k<6&&depth>0&&Physics.Raycast(o,Vector3.down,out var h,depth,~0,QueryTriggerInteraction.Ignore);k++){if(IsDrive(h.collider)&&h.normal.y>.3f){y=h.point.y;return true;}depth-=h.distance+.01f;o=h.point+Vector3.down*.01f;}return false;}
 float Edge(string route,float s,int side){var c=RAt(route,s,out _);var right=Right(route,s)*side;if(!Physics.Raycast(c+Vector3.up*2,Vector3.down,out var h,4,~0,QueryTriggerInteraction.Ignore))return 4;float py=h.point.y,w=0;for(;w<16;w+=.05f){if(!Pave(c+right*(w+.05f),py,out float ny))break;py=ny;}return w;}
 void PutOn(string route,float s,float speed,float lat=0){var c=RAt(route,s,out var f);f=Vector3.ProjectOnPlane(f,Vector3.up).normalized;var q=c+Right(route,s)*lat;if(Physics.Raycast(q+Vector3.up*4,Vector3.down,out var h,10,~0,QueryTriggerInteraction.Ignore))q=h.point;Put(q+Vector3.up*.7f,Quaternion.LookRotation(f),f*speed);}
 float Steer(Vector3 target){var d=Quaternion.Inverse(Quaternion.LookRotation(Vector3.ProjectOnPlane(car.transform.forward,Vector3.up)))*(target-car.Body.position);return Mathf.Clamp(Mathf.Atan2(d.x,d.z)*1.6f,-1,1);}
 // Ride off the edge and straight back on: steer to 3 m beyond the edge over 10 m, hold 10 m, return to the centre.
 IEnumerator OffOn(string route,float s,int side,float speed){if(route=="Main")Seed(s-30);float W=Edge(route,s,side);car.enabled=false;PutOn(route,s-25,speed);int r0=resets;
  float t0=Time.time,best=s-25,maxOut=-99,air=0,maxAir=0,minUp=1,stuck=0,maxStuck=0;bool wentOff=false,back=false;float backAt=-1;
  float Target(float q)=>q<s?0:q<s+10?side*(W+3)*(q-s)/10f:q<s+20?side*(W+3):q<s+32?side*(W+3)*(1-(q-s-20)/12f):0;
  using(var log=new StreamWriter($"{output}/offon-{route.Replace(' ','_')}-{s}-{side}-{Profile}.csv")){log.WriteLine("t,s,lat,y,speed,wheels,upY");
   while(Time.time-t0<25){yield return new WaitForFixedUpdate();var p=car.Body.position;float q=RProj(route,p,best,out _);if(q>best-5)best=Mathf.Max(best,q);float lat=SignedLat(route,p,q);
    float la=best+8;var tgt=RAt(route,la,out _)+Right(route,la)*Target(la);car.Simulate(car.ForwardSpeed<speed?1:0,0,Steer(tgt),Time.fixedDeltaTime);
    float outLat=lat*side;maxOut=Mathf.Max(maxOut,outLat);if(outLat>W+.8f)wentOff=true;if(wentOff&&!back&&outLat<W-1f&&car.GroundedWheels>=2){back=true;backAt=q;}
    minUp=Mathf.Min(minUp,car.transform.up.y);if(car.GroundedWheels==0){air+=Time.fixedDeltaTime;maxAir=Mathf.Max(maxAir,air);}else air=0;
    if(car.ForwardSpeed<1){stuck+=Time.fixedDeltaTime;maxStuck=Mathf.Max(maxStuck,stuck);}else stuck=0;
    log.WriteLine($"{Time.time-t0:F2},{q:F2},{lat:F2},{p.y:F2},{car.ForwardSpeed:F1},{car.GroundedWheels},{car.transform.up.y:F2}");
    if(back&&q>s+40)break;if(resets>r0||p.y<-10||maxStuck>3)break;}}
  car.enabled=true;Note($"off-and-on {route} s {s} {(side<0?"left":"right")} ({Profile}, {speed} m/s): edge {W:F2} m from centre; went off={wentOff} (max {maxOut:F1} m out); back on={back}{(back?$" at s {backAt:F1}":"")}; resets={resets-r0}; stuck(max {maxStuck:F1}s); maxAir={maxAir:F2}; minUp={minUp:F2}");}
 sealed class BermContacts:MonoBehaviour{public int hits;public float maxImpulse;void OnCollisionEnter(Collision c){if(c.collider.name.Contains(" barrier ")){hits++;maxImpulse=Mathf.Max(maxImpulse,c.impulse.magnitude);}}}
 IEnumerator Line(string route,float s0,float s1){if(route=="Main")Seed(s0);var bc=car.gameObject.AddComponent<BermContacts>();car.enabled=false;PutOn(route,s0,18);float t0=Time.time,best=s0,air=0,maxAir=0,minUp=1;bool done=false;int r0=resets;
  while(Time.time-t0<60){yield return new WaitForFixedUpdate();float s=RProj(route,car.Body.position,best,out _);if(car.GroundedWheels>=2)best=Mathf.Max(best,s);car.Simulate(1,0,Steer(RAt(route,best+14,out _)),Time.fixedDeltaTime);
   minUp=Mathf.Min(minUp,car.transform.up.y);if(car.GroundedWheels==0){air+=Time.fixedDeltaTime;maxAir=Mathf.Max(maxAir,air);}else air=0;if(best>=s1){done=true;break;}if(car.Body.position.y<-10||resets>r0)break;}
  car.enabled=true;Note($"clean line {route} s {s0}-{s1} ({Profile}): complete={done} best={best:F1} berm contacts={bc.hits} maxAir={maxAir:F2} minUp={minUp:F2} resets={resets-r0}");Destroy(bc);}
 IEnumerator Overrun(string route,float s,float speed){if(route=="Main")Seed(s);var bc=car.gameObject.AddComponent<BermContacts>();car.enabled=false;PutOn(route,s,speed);float t0=Time.time,air=0,maxAir=0,minUp=1,minY=1e9f;int r0=resets;var start=car.Body.position;
  while(Time.time-t0<5){yield return new WaitForFixedUpdate();car.Simulate(1,0,0,Time.fixedDeltaTime);minY=Mathf.Min(minY,car.Body.position.y);minUp=Mathf.Min(minUp,car.transform.up.y);if(car.GroundedWheels==0){air+=Time.fixedDeltaTime;maxAir=Mathf.Max(maxAir,air);}else air=0;if(car.Body.position.y<-10||resets>r0)break;}
  car.enabled=true;var p=car.Body.position;float mainS=race.road.ProjectNear(p,race.road.Project(start,out _),80,out float mainLat);bool onPave=Physics.Raycast(p+Vector3.up*.5f,Vector3.down,out var h,3,~0,QueryTriggerInteraction.Ignore)&&IsDrive(h.collider);
  Note($"overshoot {route} from s {s} at {speed} m/s, no steering ({Profile}): berm contacts={bc.hits} (max impulse {bc.maxImpulse:F0}); end {p:F1} main s {mainS:F1} lat {mainLat:F1}; on pavement={onPave}; dropped {start.y-minY:F1} m below the start; maxAir={maxAir:F2} minUp={minUp:F2} speed {car.Body.linearVelocity.magnitude:F1}");Destroy(bc);}

 // Full throttle up the hill and over the crest, steering at the route (a rider's normal line); airborne the steering does nothing.
 // Per step: s, signed lateral, height of the body above the route centre line, speed, grounded wheels.
 IEnumerator Climb(string route,float s0,float s1,float speed){if(route=="Main")Seed(s0);var bc=car.gameObject.AddComponent<BermContacts>();car.enabled=false;PutOn(route,s0,speed);float t0=Time.time,best=s0,maxH=0,maxHS=0,maxHLat=0,maxHSpeed=0,minUp=1,minY=1e9f,maxLat=0;bool done=false;int r0=resets;float firstAirS=-1,firstAirV=0;
  using(var log=new StreamWriter($"{output}/climb-{route.Replace(' ','_')}-{s0}-{Profile}-{runs++}.csv")){log.WriteLine("t,s,lat,hAbove,y,speed,wheels,upY,x,z");
   while(Time.time-t0<45){yield return new WaitForFixedUpdate();var p=car.Body.position;float s=RProj(route,p,best,out _);if(s>best-8)best=Mathf.Max(best,s);float lat=SignedLat(route,p,s);var c=RAt(route,s,out _);float hAbove=p.y-c.y;
    car.Simulate(1,0,Steer(RAt(route,best+14,out _)),Time.fixedDeltaTime);
    minUp=Mathf.Min(minUp,car.transform.up.y);minY=Mathf.Min(minY,p.y);if(Mathf.Abs(lat)>Mathf.Abs(maxLat))maxLat=lat;
    if(car.GroundedWheels==0&&firstAirS<0&&s>s0+20){firstAirS=s;firstAirV=car.Body.linearVelocity.magnitude;}
    if(hAbove>maxH){maxH=hAbove;maxHS=s;maxHLat=lat;maxHSpeed=car.Body.linearVelocity.magnitude;}
    log.WriteLine($"{Time.time-t0:F2},{s:F2},{lat:F2},{hAbove:F2},{p.y:F2},{car.Body.linearVelocity.magnitude:F1},{car.GroundedWheels},{car.transform.up.y:F2},{p.x:F2},{p.z:F2}");
    if(best>=s1){done=true;break;}if(p.y<-10||resets>r0)break;}}
  car.enabled=true;var e=car.Body.position;float es=RProj(route,e,best,out _);
  Note($"climb {route} s {s0}->{s1} full throttle ({Profile}, start {speed} m/s): complete={done} best s {best:F1}; first airborne s {firstAirS:F1} at {firstAirV:F1} m/s; max height above road {maxH:F2} m at s {maxHS:F1} lat {maxHLat:F1} ({maxHSpeed:F1} m/s); max |lat| {maxLat:F1}; berm contacts={bc.hits} (max impulse {bc.maxImpulse:F0}); end s {es:F1} lat {SignedLat(route,e,es):F1} y {e.y:F1}; resets={resets-r0}; minUp={minUp:F2}");Destroy(bc);}
 int runs;

 IEnumerator AiEnter(string route,float s0){var branch=race.Branches.First(b=>b.title==route);Seed(s0);foreach(var b in race.Branches)b.aiValidated=b==branch;
  var pilot=car.gameObject.AddComponent<RoadDriver>();pilot.Initialize(race,car,true,1,1);pilot.Racer=race.Racers[0];pilot.Place(s0,0);
  typeof(RoadDriver).GetField("plannedBranch",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(pilot,branch);race.ResetSampling(car.Body.position,race.Clock);
  int r0=resets;float t0=Time.time,best=0,air=0,maxAir=0,minUp=1;bool entered=false,rejoined=false;
  while(Time.time-t0<120){AudioListener.volume=0;yield return new WaitForFixedUpdate();float s=branch.Project(car.Body.position,out _);bool on=race.Racers[0].Branch.Route==branch;if(on){entered=true;best=Mathf.Max(best,s);}
   minUp=Mathf.Min(minUp,car.transform.up.y);if(car.GroundedWheels==0){air+=Time.fixedDeltaTime;maxAir=Mathf.Max(maxAir,air);}else air=0;
   if(entered&&best>branch.Length-15&&!on){rejoined=true;break;}if(pilot.RecoveryCount>0||resets>r0)break;}
  Note($"AI {route} from main s {s0} ({Profile}): entered={entered} rejoined={rejoined} best={best:F1}/{branch.Length:F1} recoveries={pilot.RecoveryCount} resets={resets-r0} maxAir={maxAir:F2} minUp={minUp:F2}");
  Destroy(pilot);foreach(var b in race.Branches)b.aiValidated=false;yield return null;}

 sealed class AllContacts:MonoBehaviour{public readonly List<(float imp,string name,Vector3 p,Vector3 n,float t)> hits=new();
  void Record(Collision c){if(c.impulse.magnitude<1)return;var cp=c.contactCount>0?c.GetContact(0):default;hits.Add((c.impulse.magnitude,c.collider.name,cp.point,cp.normal,Time.time));}
  void OnCollisionEnter(Collision c)=>Record(c);void OnCollisionStay(Collision c)=>Record(c);}
 IEnumerator Enter(string route,float s0,float speed,float latOff){Seed(s0);var ac=car.gameObject.AddComponent<AllContacts>();car.enabled=false;PutOn("Main",s0,speed);
  var b=race.Branches.First(x=>x.title==route);var b0=b.At(0,out _);float t0=Time.time,bestM=s0,bestB=-1,minUp=1,maxRoll=0,maxAng=0;Vector3 angAt=default;float angS=-1;bool done=false;int r0=resets;var spikes=new List<string>();
  using(var log=new StreamWriter($"{output}/enter-{route.Replace(' ','_')}-{s0}-{speed}-{latOff}-{Profile}-{runs++}.csv")){log.WriteLine("t,bs,blat,x,y,z,speed,wheels,upY,roll,ang");
   while(Time.time-t0<14){yield return new WaitForFixedUpdate();var p=car.Body.position;float bs=b.Project(p,out float bl);
    if(bestB<0&&Vector3.Distance(p,b0)<10)bestB=0;if(bestB>=0&&bs>bestB-5)bestB=Mathf.Max(bestB,bs);else bestM=Mathf.Max(bestM,race.road.ProjectNear(p,bestM,40,out _));
    Vector3 tgt;if(bestB<0)tgt=Vector3.Lerp(race.road.At(bestM+10,out _),b0,Mathf.Clamp01(1-(Vector3.Distance(p,b0)-10)/20));else{var c=b.At(bestB+8,out var f);tgt=c+Vector3.Cross(Vector3.up,Vector3.ProjectOnPlane(f,Vector3.up).normalized)*latOff;}
    car.Simulate(car.ForwardSpeed<speed?1:0,0,Steer(tgt),Time.fixedDeltaTime);
    float roll=Vector3.SignedAngle(Vector3.ProjectOnPlane(Vector3.up,car.transform.forward),car.transform.up,car.transform.forward);float ang=car.Body.angularVelocity.magnitude;
    minUp=Mathf.Min(minUp,car.transform.up.y);maxRoll=Mathf.Max(maxRoll,Mathf.Abs(roll));if(ang>maxAng){maxAng=ang;angAt=p;angS=bestB;}
    log.WriteLine($"{Time.time-t0:F2},{bs:F2},{SignedLatB(b,p,bs):F2},{p.x:F2},{p.y:F2},{p.z:F2},{car.ForwardSpeed:F1},{car.GroundedWheels},{car.transform.up.y:F2},{roll:F0},{ang:F2}");
    if(bestB>=40){done=true;break;}if(p.y<-10||resets>r0)break;}}
  car.enabled=true;var top=ac.hits.OrderByDescending(h=>h.imp).Take(6).Select(h=>$"{h.name}@{h.p:F1} n{h.n:F2} imp {h.imp:F0}");
  Note($"enter {route} from main s {s0} at {speed} m/s, line {latOff:+0.0;-0.0} m ({Profile}): complete={done} branch s {bestB:F1}; minUp={minUp:F2} maxRoll={maxRoll:F0} deg; max angular velocity {maxAng:F2} rad/s at {angAt:F1} (branch s {angS:F1}); resets={resets-r0}; body contacts {ac.hits.Count}: {string.Join("; ",top)}");Destroy(ac);}
 float SignedLatB(WoodlandRoute b,Vector3 p,float s){var c=b.At(s,out var f);return Vector3.Dot(p-c,Vector3.Cross(Vector3.up,Vector3.ProjectOnPlane(f,Vector3.up).normalized));}
 IEnumerator Reset(float s,int side){Seed(s-70);car.enabled=false;PutOn("Main",s-60,14);float t0=Time.time,best=s-60;
  while(Time.time-t0<12&&best<s){yield return new WaitForFixedUpdate();best=Mathf.Max(best,race.road.ProjectNear(car.Body.position,best,40,out _));car.Simulate(car.ForwardSpeed<14?1:0,0,Steer(race.road.At(best+12,out _)),Time.fixedDeltaTime);}
  float W=Edge("Main",s,side);var c=race.road.At(s,out var f);var q=c+Right("Main",s)*side*(W+2.5f);if(Physics.Raycast(q+Vector3.up*6,Vector3.down,out var h,30,~0,QueryTriggerInteraction.Ignore))q=h.point;Put(q+Vector3.up*.7f,Quaternion.LookRotation(Vector3.ProjectOnPlane(f,Vector3.up)),Vector3.zero);car.enabled=true;
  yield return new WaitForSeconds(1.5f);var from=car.Body.position;float s0=race.road.ProjectNear(from,s,40,out float l0);t0=Time.time;respawn.ResetVehicle();bool waited=false;
  while(Time.time-t0<8){yield return null;if(respawn.LastRecovery!=null&&respawn.LastRecovery.Contains("Waiting"))waited=true;if(!respawn.Pending)break;}
  var to=car.Body.position;float s1=race.road.ProjectNear(to,s,60,out float l1);race.road.At(s1,out var f1);
  Note($"reset beside the edge at s {s} ({(side<0?"left":"right")}, {W+2.5f:F1} m from centre, {Profile}): before s {s0:F1} lat {l0:F1} -> after s {s1:F1} lat {l1:F1}; along {s1-s0:F1} m; distance {Vector3.Distance(from,to):F1} m; restored={!respawn.Pending} in {Time.time-t0:F2}s; waiting shown={waited}; facing={Vector3.Dot(Vector3.ProjectOnPlane(car.transform.forward,Vector3.up).normalized,Vector3.ProjectOnPlane(f1,Vector3.up).normalized):F2}; up={car.transform.up.y:F2}");}
}
}
#endif
