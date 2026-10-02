#if UNITY_EDITOR
using System;using System.IO;using System.Linq;using System.Collections;using System.Collections.Generic;using System.Reflection;using UnityEngine;
namespace Racer {
// 0.68 Part A targeted checks (muted). PROBE_RESET="case;case..." per scene. Each case drives/places the player,
// presses reset (or triggers the failsafe / quit) and records before/after station, distance, time and any
// "Waiting" state. Runs unchanged against the 0.67 recovery code to reproduce the reported failures.
public sealed class Report068ResetChecks:MonoBehaviour {
 RaceDirector race;ArcadeVehicle car;VehicleRespawn respawn;readonly List<string> rows=new();string output,scene;
 void Note(string s){rows.Add(s);File.WriteAllLines(output+"/results.txt",rows);Debug.Log("REPORT068 "+s);}
 IEnumerator Start(){AudioListener.volume=0;scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;output=Environment.GetEnvironmentVariable("PROBE_OUT")+"/reset-"+scene;Directory.CreateDirectory(output);Application.runInBackground=true;race=FindAnyObjectByType<RaceDirector>();yield return null;yield return null;
  race.Flow.UseValidationSave(Path.GetFullPath("Temp/Report068ResetSave"));
  var title=FindAnyObjectByType<StartupTitle>();if(title)Destroy(title.gameObject);typeof(StartupTitle).GetField("completed",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,true);race.Flow.EnterMenuAfterTitle();
  car=race.vehicle;respawn=car.GetComponent<VehicleRespawn>();
  foreach(var spec in (Environment.GetEnvironmentVariable("PROBE_RESET")??"").Split(';').Where(x=>x.Length>0)){var a=spec.Split(':');
   yield return Begin(a.Length>1?a[1]:"moto",a[0].StartsWith("roam"));AudioListener.volume=0;
   IEnumerator run=a[0] switch{"overshoot"=>Overshoot(float.Parse(a[2]),float.Parse(a[3])),"undershoot"=>Undershoot(float.Parse(a[2]),float.Parse(a[3])),"fly"=>Fly(a[2],float.Parse(a[3]),float.Parse(a[4])),"offside"=>Offside(float.Parse(a[2]),float.Parse(a[3])),
    "upside"=>Upside(float.Parse(a[2])),"below"=>Below(float.Parse(a[2])),"roambelow"=>Below(float.Parse(a[2])),"quit"=>Quit(Vec(a[2])),"roamspawn"=>SpawnRoam(),"offlanding"=>OffLanding(float.Parse(a[2]),Vec(a[3])),"drop"=>Drop(Vec(a[2]),Vec(a[3])),"pausemenu"=>PauseMenu(),"line"=>Line(a[2],float.Parse(a[3]),float.Parse(a[4])),"overrun"=>Overrun(a[2],float.Parse(a[3]),float.Parse(a[4])),_=>null};
   if(run!=null)yield return run;else Note("unknown case "+spec);}
  File.WriteAllLines(output+"/done.txt",rows);UnityEditor.EditorApplication.Exit(0);}
 static Vector3 Vec(string s){var v=s.Split(',').Select(float.Parse).ToArray();return new Vector3(v[0],v[1],v[2]);}
 IEnumerator Begin(string profile,bool roam){var flow=race.Flow;if(flow.State!=RaceFlow.Stage.Ready){flow.Pause();flow.QuitRace();}yield return null;
  flow.OpenGarage();flow.SelectVehicle(profile);flow.CloseGarage();race.opponents=race.traffic=false;race.laps=1;if(roam)flow.StartFreeRoam();else flow.StartRace();
  while(flow.State!=RaceFlow.Stage.Racing){AudioListener.volume=0;yield return null;}Time.timeScale=1;yield return new WaitForSeconds(.5f);}
 // Credit gates up to station 'from' so the race state matches a real lap at that point.
 void Seed(float from){if(race.FreeRoam)return;var state=race.Racers[0];state.Branch.Clear();race.Progress.Restart();race.Progress.Cross(0,true,race.Clock);float prior=race.road.Relative(from,race.Origin);for(int g=1;g<race.gates.Length;g++)if(race.road.Relative(race.road.Project(race.gates[g].transform.position,out _),race.Origin)<prior)race.Progress.Cross(g,true,race.Clock);}
 void Put(Vector3 p,Quaternion r,Vector3 v){car.Body.isKinematic=false;car.transform.SetPositionAndRotation(p,r);car.Body.position=p;car.Body.rotation=r;car.Body.linearVelocity=v;car.Body.angularVelocity=Vector3.zero;car.ClearSteering();Physics.SyncTransforms();race.ResetSampling(p,race.Clock);respawn.SeedCoursePosition(p);}
 Vector3 Road(float s,float lateral,out Vector3 f){var c=race.road.At(s,out f);var q=c+Vector3.Cross(Vector3.up,Vector3.ProjectOnPlane(f,Vector3.up).normalized)*lateral;if(Physics.Raycast(q+Vector3.up*4,Vector3.down,out var h,10,~0,QueryTriggerInteraction.Ignore))q=h.point;return q+Vector3.up*.7f;}
 float S(Vector3 p,out float lat){float s=race.road.Project(new Vector3(p.x,p.y<-15?race.road.At(race.road.Project(p,out _),out _).y:p.y,p.z),out lat);return s;}
 // Drives the vehicle (full throttle on the route line) and lets it fly; returns when it settles or falls.
 IEnumerator DriveAndFly(float from,float speed,float steerUntil,float maxTime){
  car.enabled=false;var p0=Road(from,0,out var f0);Put(p0,Quaternion.LookRotation(Vector3.ProjectOnPlane(f0,Vector3.up)),Vector3.ProjectOnPlane(f0,Vector3.up).normalized*speed);
  float t0=Time.time,best=from,settled=0;bool flown=false;float lipSpeed=0;Vector3 lipAt=default,landAt=default;
  while(Time.time-t0<maxTime){yield return new WaitForFixedUpdate();float s=race.road.ProjectNear(car.Body.position,best,60,out _);if(car.GroundedWheels>=2&&s>best)best=s;
   float steer=0;if(!flown&&best<steerUntil){var target=race.road.At(best+14,out _);var d=Quaternion.Inverse(Quaternion.LookRotation(Vector3.ProjectOnPlane(car.transform.forward,Vector3.up)))*(target-car.Body.position);steer=Mathf.Clamp(Mathf.Atan2(d.x,d.z)*1.6f,-1,1);}
   if(!flown&&car.GroundedWheels==0&&best>=steerUntil-30){flown=true;lipSpeed=car.Body.linearVelocity.magnitude;lipAt=car.Body.position;}
   if(flown&&landAt==default&&car.GroundedWheels>=1)landAt=car.Body.position;
   car.Simulate(1,0,steer,Time.fixedDeltaTime);
   if(car.Body.position.y<-10)break;if(flown&&car.GroundedWheels>=1){settled+=Time.fixedDeltaTime;if(settled>1.2f)break;}}
  car.enabled=true;Note($"   flight: takeoff {lipAt:F1} at {lipSpeed:F1} m/s; first contact {landAt:F1} (s {S(landAt,out float ll):F1} lat {ll:F1}); now {car.Body.position:F1}");}
 // Drive the route for a few seconds up to station s so the recovery code has real earned anchors (as in play).
 IEnumerator Approach(float s){car.enabled=false;var p0=Road(s-70,0,out var f0);Put(p0,Quaternion.LookRotation(Vector3.ProjectOnPlane(f0,Vector3.up)),Vector3.ProjectOnPlane(f0,Vector3.up).normalized*12);float t0=Time.time,best=s-70;
  while(Time.time-t0<12&&best<s){yield return new WaitForFixedUpdate();float q=race.road.ProjectNear(car.Body.position,best,40,out _);best=Mathf.Max(best,q);var target=race.road.At(best+12,out _);var d=Quaternion.Inverse(Quaternion.LookRotation(Vector3.ProjectOnPlane(car.transform.forward,Vector3.up)))*(target-car.Body.position);car.Simulate(car.ForwardSpeed<16?1:0,0,Mathf.Clamp(Mathf.Atan2(d.x,d.z)*1.6f,-1,1),Time.fixedDeltaTime);}
  car.enabled=true;}
 IEnumerator Overshoot(float from,float speed){Seed(from);yield return DriveAndFly(from,speed,race.GetComponent<MountainFlights>().flights.First(f=>f.name.StartsWith("Homeward")).approachStation+297,14);yield return PressReset($"overshoot Homeward Summit Flight (BUG-008) from s {from} at {speed} m/s");}
 // Trajectory only (no reset): full throttle from 'from' over the named flight; logs takeoff and first contact.
 IEnumerator Fly(string flight,float from,float speed){Seed(from);var f=race.GetComponent<MountainFlights>().flights.First(x=>x.name.StartsWith(flight));Note($"fly {flight} from s {from} at {speed} m/s ({car.GetComponent<VehicleConfiguration>().profileId})");
  using(var log=new StreamWriter($"{output}/fly-{flight}-{car.GetComponent<VehicleConfiguration>().profileId}-{from}-{speed}.csv")){log.WriteLine("t,x,y,z,s,speed,wheels");float t0=Time.time;var co=StartCoroutine(DriveAndFly(from,speed,race.road.Project(f.lip,out _),14));
   while(Time.time-t0<14&&car.Body.position.y>-10){var p=car.Body.position;log.WriteLine($"{Time.time-t0:F2},{p.x:F1},{p.y:F1},{p.z:F1},{S(p,out _):F1},{car.Body.linearVelocity.magnitude:F1},{car.GroundedWheels}");yield return new WaitForSeconds(.1f);}yield return co;}}
 IEnumerator Undershoot(float from,float speed){Seed(from);yield return DriveAndFly(from,speed,race.GetComponent<MountainFlights>().flights.First(f=>f.name.StartsWith("South Face")).approachStation+227,14);yield return PressReset($"undershoot South Face Summit Flight (Reverse) from s {from} at {speed} m/s");}
 IEnumerator Offside(float s,float lateral){Seed(s-70);yield return Approach(s);var q=Road(s,lateral,out var f);Put(q,Quaternion.LookRotation(Vector3.ProjectOnPlane(f,Vector3.up)),Vector3.zero);
  float t0=Time.time;while(Time.time-t0<4&&car.Body.position.y>-10)yield return new WaitForFixedUpdate();yield return PressReset($"off the side at s {s} lateral {lateral}");}
 IEnumerator Upside(float s){Seed(s-70);yield return Approach(s);var q=Road(s,0,out var f);Put(q+Vector3.up*.6f,Quaternion.LookRotation(Vector3.ProjectOnPlane(f,Vector3.up))*Quaternion.Euler(0,0,180),Vector3.zero);
  yield return new WaitForSeconds(2);yield return PressReset($"upside-down on the road at s {s}");}
 IEnumerator PressReset(string label){var from=car.Body.position;float s0=S(from,out float lat0);float t0=Time.time;bool waited=false;string last="";
  if(from.y<-10){Note($"{label}: vehicle already fell below the world before reset at {from:F1}");}
  respawn.ResetVehicle();
  while(Time.time-t0<8){yield return null;if(respawn.LastRecovery!=null&&respawn.LastRecovery.Contains("Waiting"))waited=true;last=respawn.LastRecovery;if(!respawn.Pending)break;}
  var to=car.Body.position;float s1=S(to,out float lat1);float dt=Time.time-t0;
  Note($"{label}: before {from:F1} s={s0:F1} lat={lat0:F1} -> after {to:F1} s={s1:F1} lat={lat1:F1}; along={Mathf.Repeat(s1-s0+race.road.Length/2,race.road.Length)-race.road.Length/2:F1} m; distance={Vector3.Distance(from,to):F1} m; restored={!respawn.Pending} in {dt:F2}s; waitingShown={waited}; last='{last}'; up={car.transform.up.y:F2}; facing={Vector3.Dot(Vector3.ProjectOnPlane(car.transform.forward,Vector3.up).normalized,Vector3.ProjectOnPlane(Fwd(s1),Vector3.up).normalized):F2}; diag={respawn.RecoveryDiagnostic}");
  yield return Hold("after reset");}
 // Dan's BUG-007: anchors earned on the run-up, then an off-track landing that never touches the road.
 IEnumerator OffLanding(float s,Vector3 at){Seed(s-70);yield return Approach(s);var f=Fwd(s);Put(at,Quaternion.LookRotation(Vector3.ProjectOnPlane(f,Vector3.up)),Vector3.zero);
  float t0=Time.time;while(Time.time-t0<2&&car.Body.position.y>-10)yield return new WaitForFixedUpdate();yield return PressReset($"off-track landing at {at} after the run-up to s {s}");}
 // A high-speed impact from flight height onto the ground at a point (does the vehicle pass through?).
 IEnumerator Drop(Vector3 at,Vector3 v){Seed(race.road.Project(at,out _));Put(at,Quaternion.LookRotation(Vector3.ProjectOnPlane(v,Vector3.up)),v);float t0=Time.time,minY=at.y;Vector3 impact=default;
  while(Time.time-t0<9){yield return new WaitForFixedUpdate();minY=Mathf.Min(minY,car.Body.position.y);if(impact==default&&car.GroundedWheels>0)impact=car.Body.position;if(car.Body.position.y<-12)break;}
  Note($"drop from {at} v={v}: first ground contact {(impact==default?"none":impact.ToString("F1"))}; lowest y {minY:F1}; now {car.Body.position:F1}; fell through world={car.Body.position.y<-12}; last='{respawn.LastRecovery}'");}
 // Part D: the in-race pause menu labels the option that leaves the race.
 IEnumerator PauseMenu(){race.Flow.Pause();for(int i=0;i<3;i++)yield return null;var labels=FindObjectsByType<UnityEngine.UI.Text>(FindObjectsSortMode.None).Where(t=>t.isActiveAndEnabled&&t.GetComponentInParent<UnityEngine.UI.Button>()).Select(t=>t.text).ToArray();
  ThreeFeatureValidation.CaptureUi(output+"/pause-menu.png");Note($"pause menu buttons: {string.Join(" | ",labels)}");race.Flow.Resume();yield return null;}
 // Part E: route helpers for main or a branch.
 Vector3 RAt(string route,float s,out Vector3 f){if(route=="Main")return race.road.At(s,out f);var b=race.Branches.First(x=>x.title==route);return b.At(s,out f);}
 float RProj(string route,Vector3 p,float near,out float lat){if(route=="Main")return race.road.ProjectNear(p,near,40,out lat);var b=race.Branches.First(x=>x.title==route);return b.Project(p,out lat);}
 sealed class BermContacts:MonoBehaviour{public int hits;public float maxImpulse;void OnCollisionEnter(Collision c){if(c.collider.name.StartsWith("Ground_Report068 barrier")){hits++;maxImpulse=Mathf.Max(maxImpulse,c.impulse.magnitude);}}}
 void PutOn(string route,float s,float speed){var c=RAt(route,s,out var f);f=Vector3.ProjectOnPlane(f,Vector3.up).normalized;var q=c;if(Physics.Raycast(q+Vector3.up*4,Vector3.down,out var h,10,~0,QueryTriggerInteraction.Ignore))q=h.point;Put(q+Vector3.up*.7f,Quaternion.LookRotation(f),f*speed);}
 // Clean line: full throttle steering along the route; any berm contact is reported.
 IEnumerator Line(string route,float s0,float s1){if(route=="Main")Seed(s0);var bc=car.gameObject.AddComponent<BermContacts>();car.enabled=false;PutOn(route,s0,18);float t0=Time.time,best=s0,air=0,maxAir=0,minUp=1;bool done=false;
  while(Time.time-t0<60){yield return new WaitForFixedUpdate();float s=RProj(route,car.Body.position,best,out _);if(car.GroundedWheels>=2)best=Mathf.Max(best,s);var target=RAt(route,best+14,out _);var d=Quaternion.Inverse(Quaternion.LookRotation(Vector3.ProjectOnPlane(car.transform.forward,Vector3.up)))*(target-car.Body.position);
   car.Simulate(1,0,Mathf.Clamp(Mathf.Atan2(d.x,d.z)*1.6f,-1,1),Time.fixedDeltaTime);minUp=Mathf.Min(minUp,car.transform.up.y);if(car.GroundedWheels==0){air+=Time.fixedDeltaTime;maxAir=Mathf.Max(maxAir,air);}else air=0;if(best>=s1){done=true;break;}if(car.Body.position.y<-10)break;}
  car.enabled=true;Note($"clean line {route} s {s0}-{s1} ({car.GetComponent<VehicleConfiguration>().profileId}): complete={done} best={best:F1} berm contacts={bc.hits} maxAir={maxAir:F2} minUp={minUp:F2}");Destroy(bc);}
 // Overshoot: full throttle, no steering, from s along the route tangent; is the vehicle kept on the track?
 IEnumerator Overrun(string route,float s,float speed){if(route=="Main")Seed(s);var bc=car.gameObject.AddComponent<BermContacts>();car.enabled=false;PutOn(route,s,speed);float t0=Time.time,maxLat=0,air=0,maxAir=0,minUp=1;var start=car.Body.position;
  while(Time.time-t0<5){yield return new WaitForFixedUpdate();car.Simulate(1,0,0,Time.fixedDeltaTime);RProj(route,car.Body.position,s,out float lat);maxLat=Mathf.Max(maxLat,lat);minUp=Mathf.Min(minUp,car.transform.up.y);if(car.GroundedWheels==0){air+=Time.fixedDeltaTime;maxAir=Mathf.Max(maxAir,air);}else air=0;if(car.Body.position.y<-10)break;}
  car.enabled=true;var p=car.Body.position;float sEnd=RProj(route,p,s,out float latEnd);
  Note($"overshoot {route} from s {s} at {speed} m/s, no steering ({car.GetComponent<VehicleConfiguration>().profileId}): berm contacts={bc.hits} (max impulse {bc.maxImpulse:F0}); end s {sEnd:F1} lateral {latEnd:F1} m (max {maxLat:F1}); maxAir={maxAir:F2} minUp={minUp:F2} speed {car.Body.linearVelocity.magnitude:F1}");Destroy(bc);}
 Vector3 Fwd(float s){race.road.At(s,out var f);return f;}
 IEnumerator Hold(string label){float t0=Time.time;float minY=car.Body.position.y;var start=car.Body.position;while(Time.time-t0<2){yield return new WaitForFixedUpdate();minY=Mathf.Min(minY,car.Body.position.y);}
  Note($"   {label}: held 2 s, drop {start.y-minY:F2} m, wheels {car.GroundedWheels}, pos {car.Body.position:F1}");}
 IEnumerator Below(float s){Seed(s-70);yield return Approach(s);var q=Road(s,0,out var f);Put(new Vector3(q.x,-60,q.z),Quaternion.LookRotation(Vector3.ProjectOnPlane(f,Vector3.up)),Vector3.down*20);float t0=Time.time;var from=car.Body.position;
  while(Time.time-t0<5&&car.Body.position.y<-10)yield return new WaitForFixedUpdate();var to=car.Body.position;
  Note($"failsafe below the world ({(race.FreeRoam?"Free Roam":"Race")}) at s {s}: from {from:F1} -> {to:F1} in {Time.time-t0:F2}s recovered={to.y>-10}; last='{respawn.LastRecovery}'; diag={respawn.RecoveryDiagnostic}");yield return Hold("after failsafe");}
 IEnumerator Quit(Vector3 at){var s=race.road.Project(at,out _);Seed(s);race.road.At(s,out var f);Put(at,Quaternion.LookRotation(Vector3.ProjectOnPlane(f,Vector3.up)),Vector3.zero);yield return new WaitForFixedUpdate();
  race.Flow.Pause();race.Flow.QuitRace();yield return null;var p=car.Body.position;bool under=Physics.Raycast(p+Vector3.up*.2f,Vector3.up,out var up,6,~0,QueryTriggerInteraction.Ignore)&&up.collider.name.Contains("driving surface");bool floor=Physics.Raycast(p,Vector3.down,out var dn,3,~0,QueryTriggerInteraction.Ignore);
  Note($"quit race at {at:F1} (s {s:F1}): vehicle {p:F1}; pavement above it={under}; floor below={(floor?dn.collider.name+" "+dn.distance.ToString("F2")+" m":"NONE")}");
  race.Flow.StartFreeRoam();while(race.Flow.State!=RaceFlow.Stage.Racing)yield return null;yield return Hold("free roam after quit");}
 IEnumerator SpawnRoam(){var p=car.Body.position;var sp=respawn.spawnPoint;Note($"free roam start: spawn marker {(sp?sp.position.ToString("F2"):"none")}, vehicle {p:F2}");yield return Hold("free roam start");}
}
}
#endif
