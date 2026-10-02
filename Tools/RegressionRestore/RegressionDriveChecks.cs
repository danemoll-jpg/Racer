#if UNITY_EDITOR
using System;using System.IO;using System.Linq;using System.Collections;using System.Collections.Generic;using System.Reflection;using UnityEngine;
namespace Racer {
// Temporary regression drive harness (probe copy only).
public sealed class RegressionDriveChecks:MonoBehaviour {
 public static string Output;RaceDirector race;ArcadeVehicle car;readonly List<string> rows=new();int resets;
 void Note(string s){rows.Add(s);File.WriteAllLines(Output+"/results.txt",rows);}
 IEnumerator Start(){var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;Output=Environment.GetEnvironmentVariable("PROBE_OUT")+"/drive-"+scene;Directory.CreateDirectory(Output);AudioListener.volume=0;Application.runInBackground=true;race=FindAnyObjectByType<RaceDirector>();yield return null;yield return null;
 race.Flow.UseValidationSave(Path.GetFullPath("Temp/RegressionDriveSave"));
 var title=FindAnyObjectByType<StartupTitle>();if(title)Destroy(title.gameObject);typeof(StartupTitle).GetField("completed",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,true);race.Flow.EnterMenuAfterTitle();race.Flow.OpenGarage();race.Flow.SelectVehicle("moto");race.Flow.CloseGarage();race.opponents=race.traffic=false;race.laps=1;race.Flow.StartRace();while(race.Flow.State!=RaceFlow.Stage.Racing){AudioListener.volume=0;yield return null;}Time.timeScale=3;
 car=race.vehicle;car.GetComponent<VehicleInput>().enabled=false;car.enabled=false;car.GetComponent<VehicleRespawn>().Respawned+=()=>resets++;
 string mode=Environment.GetEnvironmentVariable("PROBE_DRIVE")??"all";
 foreach(var profile in new[]{"moto","atv"}){car.GetComponent<VehicleConfiguration>().Apply(profile);
  if(mode.Contains("jump")||mode=="all"){yield return Run(profile,"AI",800,1300,0);if(profile=="moto")foreach(var off in new[]{-5f,0f,5f})yield return Run(profile,"throttle",800,1300,off);else yield return Run(profile,"throttle",800,1300,0);}
  if(mode.Contains("low")||mode=="all"){yield return Run(profile,"AI",1600,1820,0);if(profile=="moto")yield return Run(profile,"throttle",1600,1820,0);}
 }
 car.Body.isKinematic=true;File.WriteAllLines(Output+"/done.txt",rows);UnityEditor.EditorApplication.Exit(0);}
 IEnumerator Run(string profile,string driver,float from,float to,float offset){
 var state=race.Racers[0];state.Branch.Clear();race.Progress.Restart();race.Progress.Cross(0,true,race.Clock);race.Progress.Cross(1,true,race.Clock);
 RoadDriver pilot=null;if(driver=="AI"){pilot=car.gameObject.AddComponent<RoadDriver>();pilot.Initialize(race,car,true,1,1);pilot.Racer=state;foreach(var b in race.Branches)pilot.Shortcuts.TryDecide(b,0,0,0,out _);pilot.Place(from,0);}
 else{var p0=race.road.At(from,out var f0);var r0=Vector3.Cross(Vector3.up,f0).normalized;var pav=GameObject.Find("Ground_CR133 mountain driving surface").GetComponent<Collider>();var q=p0+r0*offset;if(pav.Raycast(new Ray(q+Vector3.up*6,Vector3.down),out var h,20))q=h.point;car.Body.isKinematic=false;car.transform.SetPositionAndRotation(q+Vector3.up*.65f,Quaternion.LookRotation(Vector3.ProjectOnPlane(f0,Vector3.up)));car.Body.position=car.transform.position;car.Body.rotation=car.transform.rotation;car.Body.angularVelocity=Vector3.zero;car.ClearSteering();Physics.SyncTransforms();}
 race.road.At(from,out var fw);car.Body.linearVelocity=fw*16;race.ResetSampling(car.Body.position,race.Clock);
 int before=resets;float t0=Time.time,best=from,air=0,maxAir=0,minUp=1,takeoffSpeed=0,maxRoll=0,maxPitchRate=0,maxRollRate=0;bool complete=false,flying=false;float landS=0;string firstHit="";
 string tag=$"{profile}-{driver}-{from}-{offset}";var cl=car.GetComponent<RegressionContactLog>()??car.gameObject.AddComponent<RegressionContactLog>();cl.road=race.road;cl.log=new StreamWriter(Output+"/contacts-"+tag+".csv");
 using(var log=new StreamWriter(Output+"/"+tag+".csv")){log.WriteLine("t,s,lat,x,y,z,speed,wheels,upY,pitch,roll,yawDelta,resets,recov");
 while(Time.time-t0<70){AudioListener.volume=0;float s=race.road.Project(car.Body.position,out float d);best=Mathf.Max(best,s);
  if(driver!="AI"){var target=race.road.At(s+14,out var tf)+Vector3.Cross(Vector3.up,tf).normalized*offset;var delta=Quaternion.Inverse(Quaternion.LookRotation(Vector3.ProjectOnPlane(car.transform.forward,Vector3.up)))*(target-car.Body.position);float steer=Mathf.Clamp(Mathf.Atan2(delta.x,delta.z)*1.6f,-1,1);car.Simulate(1,0,steer,Time.fixedDeltaTime);}
  var e=car.transform.eulerAngles;float pitch=Mathf.DeltaAngle(0,e.x),roll=Mathf.DeltaAngle(0,e.z);race.road.At(s,out var rf);float yawDelta=Vector3.SignedAngle(Vector3.ProjectOnPlane(rf,Vector3.up),Vector3.ProjectOnPlane(car.transform.forward,Vector3.up),Vector3.up);
  minUp=Mathf.Min(minUp,car.transform.up.y);
  if(car.GroundedWheels==0){if(!flying){flying=true;if(s>950&&s<1000&&takeoffSpeed==0)takeoffSpeed=car.Body.linearVelocity.magnitude;}air+=Time.fixedDeltaTime;maxAir=Mathf.Max(maxAir,air);if(s>950&&s<1300){maxRoll=Mathf.Max(maxRoll,Mathf.Abs(roll));var av=car.transform.InverseTransformDirection(car.Body.angularVelocity)*Mathf.Rad2Deg;maxPitchRate=Mathf.Max(maxPitchRate,Mathf.Abs(av.x));maxRollRate=Mathf.Max(maxRollRate,Mathf.Abs(av.z));}}
  else{if(flying&&air>1&&landS==0)landS=s;flying=false;air=0;}
  if(s>to&&s<to+40&&car.GroundedWheels>=2){complete=true;break;}if(resets>before||(pilot&&pilot.RecoveryCount>0))break;
  var p=car.Body.position;log.WriteLine($"{Time.time-t0:F2},{s:F2},{d:F2},{p.x:F2},{p.y:F2},{p.z:F2},{car.ForwardSpeed:F2},{car.GroundedWheels},{car.transform.up.y:F3},{pitch:F1},{roll:F1},{yawDelta:F1},{resets-before},{(pilot?pilot.RecoveryCount:0)}");
  if(driver=="AI")yield return new WaitForFixedUpdate();else yield return new WaitForFixedUpdate();}}
 cl.log.Dispose();cl.log=null;Note($"{tag}: complete={complete} best={best:F1} resets={resets-before} recov={(pilot?pilot.RecoveryCount:0)} maxAir={maxAir:F2} minUp={minUp:F2} takeoffSpeed={takeoffSpeed:F1} flightMaxRoll={maxRoll:F1} flightMaxPitchRate={maxPitchRate:F0} flightMaxRollRate={maxRollRate:F0} landS={landS:F1}");
 if(pilot){pilot.enabled=false;Destroy(pilot);}car.Body.isKinematic=true;yield return null;yield return null;}
}}
#endif
