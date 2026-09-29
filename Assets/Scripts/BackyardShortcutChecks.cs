using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Racer {
// Explicit local-segment fixture. Never enabled in ordinary gameplay.
public sealed class BackyardShortcutChecks:MonoBehaviour {
 public string Only=""; public static string Output="Docs/BackyardShortcuts/checks";
 readonly List<string> checks=new();RaceDirector race;ArcadeVehicle car;int resets;float lastTime;readonly Dictionary<string,float> times=new();
 void Check(bool ok,string message){checks.Add((ok?"PASS ":"FAIL ")+message);File.WriteAllLines(Output+"/checks.txt",checks);}
 public static void Launch(){var g=new GameObject("Bounded shortcut checks");DontDestroyOnLoad(g);g.AddComponent<BackyardShortcutChecks>();}
 void OnEnable()=>SceneManager.sceneLoaded+=Loaded;
 void OnDisable()=>SceneManager.sceneLoaded-=Loaded;
 void Loaded(Scene scene,LoadSceneMode mode){foreach(var t in FindObjectsByType<ContinuationTraffic>())t.enabled=false;}
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,"-backyardShortcutCheck");if(i>=0&&Array.IndexOf(a,"-racerTestSave")>=0){Output=a[i+1];Launch();}}
 IEnumerator Start(){DontDestroyOnLoad(gameObject);Directory.CreateDirectory(Output);AudioListener.volume=0;Application.runInBackground=true;if(SceneManager.GetActiveScene().name!="DansBackyardForward")SceneManager.LoadScene("DansBackyardForward");yield return null;yield return null;race=FindAnyObjectByType<RaceDirector>();
 #if UNITY_EDITOR
 race.Flow.UseValidationSave(Path.GetFullPath("Temp/BackyardShortcutsSave"));
 #endif
 race.Flow.OpenGarage();race.Flow.SelectVehicle("moto");race.Flow.CloseGarage();race.opponents=race.traffic=false;race.laps=1;race.Flow.StartRace();while(race.Flow.State!=RaceFlow.Stage.Racing){AudioListener.volume=0;yield return null;}car=race.vehicle;car.GetComponent<VehicleInput>().enabled=false;car.enabled=false;car.GetComponent<VehicleRespawn>().Respawned+=()=>resets++;
 foreach(var b in race.Branches.Where(b=>Only==""||b.title==Only)){Check(b.aiValidated&&b.bypassedGates.Length>0,b.title+": production branch/AI metadata registered");
 yield return Place(b,false);yield return Drive(b,false);times[b.title+" main"]=lastTime;
 yield return Place(b,true);yield return Drive(b,true);times[b.title]=lastTime;Check(times[b.title+" main"]>lastTime+.5f,$"{b.title}: main {times[b.title+" main"]:F2}s / shortcut {lastTime:F2}s; saving {times[b.title+" main"]-lastTime:F2}s");
 }
 // Model dimensions are checked without a vehicle-driving matrix.
 foreach(string id in new[]{"moto","atv","original","tourer"}){car.GetComponent<VehicleConfiguration>().Apply(id);var box=car.GetComponent<BoxCollider>();Check(box.size.x*car.transform.lossyScale.x<4.7f-1,$"{id}: body width {box.size.x:F2}m fits 4.7m tree-top trail with margin");}car.GetComponent<VehicleConfiguration>().Apply("moto");
 // Ordinary production AI receives a single local route commitment for each route.
 foreach(var b in race.Branches.Where(b=>Only==""||b.title==Only)){yield return Place(b,true);var pilot=car.gameObject.AddComponent<RoadDriver>();pilot.Initialize(race,car,true,1,1);pilot.Racer=race.Racers[0];typeof(RoadDriver).GetField("plannedBranch",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).SetValue(pilot,b);float start=Time.time,best=0,air=0,maxAir=0;using(var log=new StreamWriter(Output+"/AI-"+b.title.Replace(' ','-')+".csv")){log.WriteLine("time,station,lateral,speed,target,obstacle,recoveries");while(Time.time-start<22){yield return new WaitForFixedUpdate();AudioListener.volume=0;float s=b.Project(car.Body.position,out var lateral);best=Math.Max(best,s);if(car.GroundedWheels==0){air+=Time.fixedDeltaTime;maxAir=Math.Max(maxAir,air);}else air=0;log.WriteLine($"{Time.time-start:F2},{s:F2},{lateral:F2},{car.ForwardSpeed:F2},{pilot.TargetSpeed:F2},{pilot.LastObstacle},{pilot.RecoveryCount}");if(s>b.Length-4||pilot.RecoveryCount>0)break;}}Check(best>b.Length-8||pilot.RecoveryCount>0,$"{b.title}: production AI attempts/rejoins or recovers, best={best:F1}/{b.Length:F1}m, recoveries={pilot.RecoveryCount}, air={maxAir:F2}s");pilot.enabled=false;Destroy(pilot);yield return null;}
 // Fall onto intact ground, retain earned entitlement and let existing recovery remain available.
 var tree=race.Branches.First(b=>b.title=="Tree-Top Trail");yield return Place(tree,true);race.Racers[0].Branch.Begin(tree);var fall=tree.At(60,out var f);fall+=Vector3.Cross(Vector3.up,f).normalized*5;car.Body.position=fall+Vector3.up*.6f;car.transform.position=car.Body.position;car.Body.linearVelocity=Vector3.zero;Physics.SyncTransforms();int before=resets;float low=car.Body.position.y;for(int i=0;i<150;i++){car.Simulate(0,0,0,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();AudioListener.volume=0;}Check(resets==before&&car.GroundedWheels>=2&&car.Body.position.y<low-2,$"Tree-top fall lands on forest ground without automatic reset: drop={low-car.Body.position.y:F1}m, wheels={car.GroundedWheels}, resets={resets-before}");
 File.WriteAllText(Output+"/timing.json",Newtonsoft.Json.JsonConvert.SerializeObject(times));File.WriteAllLines(Output+"/done.txt",checks);car.Body.isKinematic=true;race.Flow.Pause();if(!Application.isEditor)Application.Quit(checks.Any(s=>s.StartsWith("FAIL"))?1:0);
 }
 IEnumerator Place(WoodlandRoute b,bool branch){race.Progress.Restart();race.Racers[0].Branch.Clear();race.Progress.Cross(0,true,race.Clock);for(int i=1;i<race.gates.Length;i++)if(race.road.Project(race.gates[i].transform.position,out _)<b.entryRoad-1)race.Progress.Cross(i,true,race.Clock);var p=race.road.At(b.entryRoad,out var f);if(branch){p=b.At(0,out f);}car.enabled=false;car.GetComponent<VehicleInput>().enabled=false;car.Body.isKinematic=false;car.Body.position=p+Vector3.up*.55f;car.Body.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(f,Vector3.up));car.transform.SetPositionAndRotation(car.Body.position,car.Body.rotation);car.Body.linearVelocity=car.Body.angularVelocity=Vector3.zero;car.ClearSteering();Physics.SyncTransforms();for(int i=0;i<30;i++){car.Simulate(0,0,0,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();}race.Progress.Restart();race.Progress.Cross(0,true,race.Clock);for(int i=1;i<race.gates.Length;i++)if(race.road.Project(race.gates[i].transform.position,out _)<b.entryRoad-1)race.Progress.Cross(i,true,race.Clock);car.Body.linearVelocity=f.normalized*26;race.ResetSampling(car.Body.position,race.Clock);}
 float Steer(Vector3 target){var local=Quaternion.Inverse(Quaternion.LookRotation(Vector3.ProjectOnPlane(car.transform.forward,Vector3.up)))*(target-car.Body.position);float look=Math.Max(3,new Vector2(local.x,local.z).magnitude);return Mathf.Clamp(Mathf.Atan(2*car.wheelbase*Mathf.Sin(Mathf.Atan2(local.x,local.z))/look)/(Mathf.Lerp(car.slowSteerAngle,car.fastSteerAngle,Mathf.Clamp01(car.ForwardSpeed/car.topSpeed))*Mathf.Deg2Rad),-1,1);}
 IEnumerator Drive(WoodlandRoute b,bool branch){string name=(branch?"shortcut-":"main-")+b.title.Replace(' ','-');float start=Time.time,air=0,maxAir=0,best=0;bool reached=false,entered=false;int before=resets,deckFrames=0;using(var log=new StreamWriter(Output+"/"+name+".csv")){log.WriteLine("time,x,y,z,speed,station,lateral,wheels,nextGate,branch");while(Time.time-start<22){float s=branch?b.Project(car.Body.position,out _):race.road.Project(car.Body.position,out _)-b.entryRoad;best=Math.Max(best,s);float length=branch?b.Length:b.exitRoad-b.entryRoad;Vector3 target=branch?b.At(s+9,out _):race.road.At(b.entryRoad+s+10,out _);if(s>=length-3){reached=true;break;}float speed=car.ForwardSpeed,desired=branch?Mathf.Min(32,b.SpeedAt(s)):32;for(float ahead=0;ahead<50;ahead+=6){Vector3 f0,f1;if(branch){b.At(s+ahead,out f0);b.At(s+ahead+6,out f1);}else{race.road.At(b.entryRoad+s+ahead,out f0);race.road.At(b.entryRoad+s+ahead+6,out f1);}float curve=Vector3.Angle(Vector3.ProjectOnPlane(f0,Vector3.up),Vector3.ProjectOnPlane(f1,Vector3.up))*Mathf.Deg2Rad/6;float safe=Mathf.Sqrt(car.maxGripAcceleration*.65f/Mathf.Max(.0001f,curve));desired=Mathf.Min(desired,Mathf.Sqrt(safe*safe+2*car.braking*.8f*Mathf.Max(0,ahead-8)));}car.Simulate(speed<desired?1:0,speed>desired+.5f?Mathf.Clamp01((speed-desired)*.3f):0,Steer(target),Time.fixedDeltaTime);yield return new WaitForFixedUpdate();AudioListener.volume=0;if(car.GroundedWheels==0){air+=Time.fixedDeltaTime;maxAir=Math.Max(maxAir,air);}else air=0;if(branch&&b.title=="Tree-Top Trail"&&s>57&&s<100&&car.GroundedWheels>=2&&Mathf.Abs(car.Body.position.y-b.At(s,out _).y)<1.3f)deckFrames++;entered|=race.Racers[0].Branch.Route==b;var p=car.Body.position;b.Project(p,out var lateral);log.WriteLine($"{Time.time-start:F2},{p.x:F3},{p.y:F3},{p.z:F3},{car.ForwardSpeed:F2},{s:F2},{lateral:F2},{car.GroundedWheels},{race.Progress.NextGate},{race.Racers[0].Branch.Route==b}");}}
 lastTime=Time.time-start;Check(reached&&resets==before,$"{name}: physical normal-input traverse {lastTime:F2}s, progress={best:F1}m, maxAir={maxAir:F2}s, resets={resets-before}");if(branch){if(b.title=="Tree-Top Trail")Check(deckFrames>20,$"Tree-top receiving trail physically driven on elevated support: {deckFrames} frames");Check(entered&&race.Progress.MissedGates==0&&race.Progress.NextGate>b.bypassedGates.Max(),$"{b.title}: actual entrance/bypass entitlement, nextGate={race.Progress.NextGate}, misses={race.Progress.MissedGates}, entered={entered}");Check(maxAir>.75f,$"{b.title}: substantial intentional airtime {maxAir:F2}s");}}
}
}







