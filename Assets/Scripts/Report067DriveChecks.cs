#if UNITY_EDITOR
using System;using System.IO;using System.Linq;using System.Collections;using System.Collections.Generic;using System.Reflection;using UnityEngine;
namespace Racer {
// 0.67 targeted neighbour drives (rule 11 / 5A.6), muted. PROBE_DRIVE="route:from:to:driver:profile:offset;..."
// route = Main (stations from..to) or a branch title (entrance -> rejoin; from/to ignored; the AI's random
// once-per-lap shortcut roll is replaced by a planned entry so the branch itself is always driven).
// driver = AI (production RoadDriver) or throttle (full throttle on a fixed lateral line, main only).
public sealed class Report067DriveChecks:MonoBehaviour {
 RaceDirector race;ArcadeVehicle car;readonly List<string> rows=new();int resets;string output;
 void Note(string s){rows.Add(s);File.WriteAllLines(output+"/results.txt",rows);}
 IEnumerator Start(){AudioListener.volume=0;var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;output=Environment.GetEnvironmentVariable("PROBE_OUT")+"/drive-"+scene;Directory.CreateDirectory(output);Application.runInBackground=true;race=FindAnyObjectByType<RaceDirector>();yield return null;yield return null;
  race.Flow.UseValidationSave(Path.GetFullPath("Temp/Report067DriveSave"));
  var title=FindAnyObjectByType<StartupTitle>();if(title)Destroy(title.gameObject);typeof(StartupTitle).GetField("completed",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,true);race.Flow.EnterMenuAfterTitle();race.Flow.OpenGarage();race.Flow.SelectVehicle("moto");race.Flow.CloseGarage();race.opponents=race.traffic=false;race.laps=1;race.Flow.StartRace();while(race.Flow.State!=RaceFlow.Stage.Racing){AudioListener.volume=0;yield return null;}Time.timeScale=3;AudioListener.volume=0;
  car=race.vehicle;car.GetComponent<VehicleInput>().enabled=false;car.enabled=false;car.GetComponent<VehicleRespawn>().Respawned+=()=>resets++;
  foreach(var spec in (Environment.GetEnvironmentVariable("PROBE_DRIVE")??"").Split(';').Where(x=>x.Length>0)){var a=spec.Split(':');car.GetComponent<VehicleConfiguration>().Apply(a[4]);
   if(a[0]=="Main")yield return Run(a[4],a[3],float.Parse(a[1]),float.Parse(a[2]),float.Parse(a[5]));else if(a[3]=="throttle")yield return BranchThrottle(a[0],a[4]);else yield return Branch(a[0],a[4]);}
  car.Body.isKinematic=true;File.WriteAllLines(output+"/done.txt",rows);UnityEditor.EditorApplication.Exit(0);}
 void Seed(float from){var state=race.Racers[0];state.Branch.Clear();race.Progress.Restart();race.Progress.Cross(0,true,race.Clock);float prior=race.road.Relative(from,race.Origin);for(int g=1;g<race.gates.Length;g++)if(race.road.Relative(race.road.Project(race.gates[g].transform.position,out _),race.Origin)<prior)race.Progress.Cross(g,true,race.Clock);}
 IEnumerator Run(string profile,string driver,float from,float to,float offset){
  Seed(from);RoadDriver pilot=null;
  if(driver=="AI"){foreach(var b in race.Branches)b.aiValidated=false;pilot=car.gameObject.AddComponent<RoadDriver>();pilot.Initialize(race,car,true,1,1);pilot.Racer=race.Racers[0];pilot.Place(from,0);}
  else{var p0=race.road.At(from,out var f0);var r0=Vector3.Cross(Vector3.up,f0).normalized;var q=p0+r0*offset;if(Physics.Raycast(q+Vector3.up*6,Vector3.down,out var h,20,~0,QueryTriggerInteraction.Ignore))q=h.point;car.Body.isKinematic=false;car.transform.SetPositionAndRotation(q+Vector3.up*.65f,Quaternion.LookRotation(Vector3.ProjectOnPlane(f0,Vector3.up)));car.Body.position=car.transform.position;car.Body.rotation=car.transform.rotation;car.Body.angularVelocity=Vector3.zero;car.ClearSteering();Physics.SyncTransforms();}
  race.road.At(from,out var fw);car.Body.linearVelocity=fw*16;race.ResetSampling(car.Body.position,race.Clock);
  int before=resets;float t0=Time.time,best=from,air=0,maxAir=0,minUp=1,maxRoll=0;bool complete=false;string tag=$"{profile}-{driver}-{from}-{to}-{offset}";
  Report067ContactLog contacts=null;var cw=Environment.GetEnvironmentVariable("PROBE_CONTACT");if(!string.IsNullOrEmpty(cw)){var w=cw.Split(':');contacts=car.GetComponent<Report067ContactLog>()??car.gameObject.AddComponent<Report067ContactLog>();contacts.road=race.road;contacts.s0=float.Parse(w[0]);contacts.s1=float.Parse(w[1]);contacts.last=from;contacts.log=new StreamWriter(output+"/contacts-"+tag+".csv");}
  using(var log=new StreamWriter(output+"/"+tag+".csv")){log.WriteLine("t,s,lat,x,y,z,speed,wheels,upY,pitch,roll,resets,recov");
   while(Time.time-t0<90){AudioListener.volume=0;float s=race.road.ProjectNear(car.Body.position,best,40,out float d);best=Mathf.Max(best,s);
    if(driver!="AI"){var target=race.road.At(s+14,out var tf)+Vector3.Cross(Vector3.up,tf).normalized*offset;var delta=Quaternion.Inverse(Quaternion.LookRotation(Vector3.ProjectOnPlane(car.transform.forward,Vector3.up)))*(target-car.Body.position);car.Simulate(1,0,Mathf.Clamp(Mathf.Atan2(delta.x,delta.z)*1.6f,-1,1),Time.fixedDeltaTime);}
    var e=car.transform.eulerAngles;float pitch=Mathf.DeltaAngle(0,e.x),roll=Mathf.DeltaAngle(0,e.z);minUp=Mathf.Min(minUp,car.transform.up.y);
    if(car.GroundedWheels==0){air+=Time.fixedDeltaTime;maxAir=Mathf.Max(maxAir,air);maxRoll=Mathf.Max(maxRoll,Mathf.Abs(roll));}else air=0;
    if(s>to&&s<to+40&&car.GroundedWheels>=2){complete=true;break;}if(resets>before||(pilot&&pilot.RecoveryCount>0))break;
    var p=car.Body.position;log.WriteLine($"{Time.time-t0:F2},{s:F2},{d:F2},{p.x:F2},{p.y:F2},{p.z:F2},{car.ForwardSpeed:F2},{car.GroundedWheels},{car.transform.up.y:F3},{pitch:F1},{roll:F1},{resets-before},{(pilot?pilot.RecoveryCount:0)}");
    yield return new WaitForFixedUpdate();}}
  if(contacts){contacts.log.Dispose();contacts.log=null;}
  Note($"{tag}: complete={complete} best={best:F1} resets={resets-before} recov={(pilot?pilot.RecoveryCount:0)} maxAir={maxAir:F2} minUp={minUp:F2} airMaxRoll={maxRoll:F1}");
  if(pilot){pilot.enabled=false;Destroy(pilot);}car.Body.isKinematic=true;yield return null;yield return null;}
 // Player-like full-throttle line along a branch that the AI is not validated for (e.g. Ridge Cut jumps).
 IEnumerator BranchThrottle(string title,string profile){var branch=race.Branches.FirstOrDefault(b=>b.title==title);if(!branch){Note($"{title}: branch missing");yield break;}
  Seed(branch.entryRoad-5);var p0=branch.At(2,out var f0);if(Physics.Raycast(p0+Vector3.up*6,Vector3.down,out var h0,20,~0,QueryTriggerInteraction.Ignore))p0=h0.point;car.Body.isKinematic=false;car.transform.SetPositionAndRotation(p0+Vector3.up*.65f,Quaternion.LookRotation(Vector3.ProjectOnPlane(f0,Vector3.up)));car.Body.position=car.transform.position;car.Body.rotation=car.transform.rotation;car.Body.angularVelocity=Vector3.zero;car.Body.linearVelocity=Vector3.ProjectOnPlane(f0,Vector3.up).normalized*16;car.ClearSteering();Physics.SyncTransforms();race.ResetSampling(car.Body.position,race.Clock);
  int before=resets;float t0=Time.time,best=0,air=0,maxAir=0,minUp=1,maxRoll=0;bool complete=false;string tag=$"{profile}-throttle-{title.Replace(' ','_')}";
  using(var log=new StreamWriter(output+"/"+tag+".csv")){log.WriteLine("t,branchS,lat,x,y,z,speed,wheels,upY,pitch,roll,resets");
   while(Time.time-t0<90){AudioListener.volume=0;float s=branch.Project(car.Body.position,out float lat);best=Mathf.Max(best,s);var target=branch.At(Mathf.Min(best+14,branch.Length),out _);var delta=Quaternion.Inverse(Quaternion.LookRotation(Vector3.ProjectOnPlane(car.transform.forward,Vector3.up)))*(target-car.Body.position);car.Simulate(1,0,Mathf.Clamp(Mathf.Atan2(delta.x,delta.z)*1.6f,-1,1),Time.fixedDeltaTime);
    var e=car.transform.eulerAngles;float pitch=Mathf.DeltaAngle(0,e.x),roll=Mathf.DeltaAngle(0,e.z);minUp=Mathf.Min(minUp,car.transform.up.y);if(car.GroundedWheels==0){air+=Time.fixedDeltaTime;maxAir=Mathf.Max(maxAir,air);maxRoll=Mathf.Max(maxRoll,Mathf.Abs(roll));}else air=0;
    if(best>branch.Length-8&&car.GroundedWheels>=2){complete=true;break;}if(resets>before)break;var p=car.Body.position;log.WriteLine($"{Time.time-t0:F2},{s:F2},{lat:F2},{p.x:F2},{p.y:F2},{p.z:F2},{car.ForwardSpeed:F2},{car.GroundedWheels},{car.transform.up.y:F3},{pitch:F1},{roll:F1},{resets-before}");yield return new WaitForFixedUpdate();}}
  Note($"{tag}: complete={complete} best={best:F1}/{branch.Length:F1} resets={resets-before} maxAir={maxAir:F2} minUp={minUp:F2} airMaxRoll={maxRoll:F1}");car.Body.isKinematic=true;yield return null;yield return null;}
 IEnumerator Branch(string title,string profile){var branch=race.Branches.FirstOrDefault(b=>b.title==title);if(!branch){Note($"{title}: branch missing");yield break;}
  Seed(branch.entryRoad-40);foreach(var b in race.Branches)b.aiValidated=b==branch;var pilot=car.gameObject.AddComponent<RoadDriver>();pilot.Initialize(race,car,true,1,1);pilot.Racer=race.Racers[0];pilot.Place(branch.entryRoad-40,0);typeof(RoadDriver).GetField("plannedBranch",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(pilot,branch);race.ResetSampling(car.Body.position,race.Clock);
  int before=resets;float t0=Time.time,best=0,air=0,maxAir=0,minUp=1;bool entered=false,rejoined=false;string tag=$"{profile}-AI-{title.Replace(' ','_')}";
  using(var log=new StreamWriter(output+"/"+tag+".csv")){log.WriteLine("t,branchS,x,y,z,speed,wheels,upY,onBranch,recov");
   while(Time.time-t0<150){AudioListener.volume=0;yield return new WaitForFixedUpdate();float s=branch.Project(car.Body.position,out _);bool on=race.Racers[0].Branch.Route==branch;if(on){entered=true;best=Mathf.Max(best,s);}
    minUp=Mathf.Min(minUp,car.transform.up.y);if(car.GroundedWheels==0){air+=Time.fixedDeltaTime;maxAir=Mathf.Max(maxAir,air);}else air=0;var p=car.Body.position;log.WriteLine($"{Time.time-t0:F2},{s:F2},{p.x:F2},{p.y:F2},{p.z:F2},{car.ForwardSpeed:F2},{car.GroundedWheels},{car.transform.up.y:F3},{on},{pilot.RecoveryCount}");
    if(entered&&best>branch.Length-15&&!on){rejoined=true;break;}if(pilot.RecoveryCount>0||resets>before)break;}}
  Note($"{tag}: entered={entered} rejoined={rejoined} best={best:F1}/{branch.Length:F1} resets={resets-before} recov={pilot.RecoveryCount} maxAir={maxAir:F2} minUp={minUp:F2}");
  pilot.enabled=false;Destroy(pilot);branch.aiValidated=false;car.Body.isKinematic=true;yield return null;yield return null;}
}

// Optional contact log (PROBE_CONTACT="s0:s1"): every body contact and wheel ray hit inside the station window.
public sealed class Report067ContactLog:MonoBehaviour{public StreamWriter log;public RaceRoad road;public float s0,s1,last;
 void OnCollisionStay(Collision c){if(log==null)return;float s=road.ProjectNear(transform.position,last,40,out _);last=s;if(s<s0||s>s1)return;foreach(var p in c.contacts){var lp=transform.InverseTransformPoint(p.point);log.WriteLine($"{Time.time:F3},{s:F2},BODY {c.collider.name},{lp.x:F2},{lp.y:F2},{lp.z:F2},{p.normal.x:F2},{p.normal.y:F2},{p.normal.z:F2},{c.impulse.magnitude:F1}");}}
 void FixedUpdate(){if(log==null)return;float s=road.ProjectNear(transform.position,last,40,out _);last=s;if(s<s0||s>s1)return;var car=GetComponent<ArcadeVehicle>();foreach(var local in car.suspensionPoints){var o=transform.TransformPoint(local);if(Physics.Raycast(o,-transform.up,out var h,car.suspensionLength,car.groundMask,QueryTriggerInteraction.Ignore))log.WriteLine($"{Time.time:F3},{s:F2},WHEEL {h.collider.name},{local.x:F2},{local.y:F2},{local.z:F2},{h.normal.x:F2},{h.normal.y:F2},{h.normal.z:F2},{h.distance:F2}");}}
}
}
#endif
