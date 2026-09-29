using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Racer {
// Opt-in bounded dump checks; absent from ordinary gameplay and always uses isolated saves.
public sealed class DumpRefuseChecks:MonoBehaviour {
 public static string Output="Docs/DumpRefuse/runtime-checks";
 readonly List<string> checks=new(); StreamWriter log; RaceDirector race; ArcadeVehicle car; Vector3 lip,axis,side; int resets;
 float clearTime,failedTime;
 void Check(bool ok,string text){checks.Add((ok?"PASS ":"FAIL ")+text);File.WriteAllLines(Output+"/checks.txt",checks);}
 public static void Launch(){var go=new GameObject("Dump correction targeted check");DontDestroyOnLoad(go);go.AddComponent<DumpRefuseChecks>();}
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){var args=Environment.GetCommandLineArgs();int n=Array.IndexOf(args,"-dumpRefuseCheck");if(n<0||Array.IndexOf(args,"-racerTestSave")<0)return;Output=args[n+1];Launch();}
 void OnEnable()=>SceneManager.sceneLoaded+=Loaded;
 void OnDisable()=>SceneManager.sceneLoaded-=Loaded;
 void Loaded(Scene scene,LoadSceneMode mode){foreach(var t in FindObjectsByType<ContinuationTraffic>())t.enabled=false;}
 float Ground(Vector3 p)=>Physics.RaycastAll(new Vector3(p.x,250,p.z),Vector3.down,500,1,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name.StartsWith("Ground_")).OrderBy(h=>h.distance).First().point.y;
 IEnumerator Start(){Directory.CreateDirectory(Output);Application.runInBackground=true;AudioListener.volume=0;SceneManager.LoadScene("DansBackyardForward");yield return null;yield return null;
 race=FindAnyObjectByType<RaceDirector>();
 #if UNITY_EDITOR
 race.Flow.UseValidationSave(Path.GetFullPath("Temp/DumpRefuseSave"));
 #endif
 race.Flow.OpenGarage();race.Flow.SelectVehicle("moto");race.Flow.CloseGarage();race.opponents=false;race.traffic=false;race.laps=1;race.Flow.StartRace();while(race.Flow.State!=RaceFlow.Stage.Racing){AudioListener.volume=0;yield return null;}
 car=race.vehicle;car.enabled=false;car.GetComponent<VehicleInput>().enabled=false;car.GetComponent<VehicleRespawn>().Respawned+=()=>resets++;
 var course=race.GetComponent<BackyardForwardCourse>();lip=course.flightStarts[0];axis=course.flightDirections[0];side=Vector3.Cross(Vector3.up,axis);
 log=new StreamWriter(Output+"/pace.csv");log.WriteLine("case,time,x,y,z,speed,wheels,nextGate,resets");
 var refuse=FindAnyObjectByType<DumpRefuse>();
 yield return Place(lip-axis*38,axis,32);Seed();yield return Drive("clear",32,13,true,false);Check(race.Progress.NextGate==3&&race.Progress.MissedGates==0,"Normal physical crossing progresses through unchanged CP2");
 foreach(string vehicle in new[]{"moto","atv"}){
  if(vehicle=="atv"){race.Flow.Pause();race.Flow.QuitRace();race.Flow.OpenGarage();race.Flow.SelectVehicle(vehicle);race.Flow.CloseGarage();race.Flow.StartRace();while(race.Flow.State!=RaceFlow.Stage.Racing){AudioListener.volume=0;yield return null;}car=race.vehicle;car.enabled=false;car.GetComponent<VehicleInput>().enabled=false;car.GetComponent<VehicleRespawn>().Respawned+=()=>resets++;}
  Check(car.GetComponent<VehicleConfiguration>().profileId==vehicle,"Actual vehicle profile: "+car.GetComponent<VehicleConfiguration>().profileId);
  refuse.enabled=false;yield return Place(lip-axis*2,axis,8);Seed();yield return Drive(vehicle+"-empty",car.topSpeed,30,false,false);float empty=failedTime;
  refuse.enabled=true;yield return Place(lip-axis*2,axis,8);Seed();yield return Drive(vehicle+"-trash",car.topSpeed,30,false,false);
  Check(failedTime>empty+1,vehicle+" trash slows physical escape: "+failedTime.ToString("F2")+"s versus empty "+empty.ToString("F2")+"s");
  yield return Place(lip+axis*20-side*2,axis,0);yield return Drive(vehicle+"-maneuver",6,30,false,true);
 }
 Check(resets==0,"Both vehicles retain normal recovery and escape without reset");
 var junk=GameObject.Find("Old dump - dense push-through refuse");Check(junk&&junk.GetComponentsInChildren<Collider>().Length==0&&junk.GetComponentsInChildren<Renderer>().Length==8,"Dense trash is eight renderers with no snag, flip or launch colliders");
 car.Body.isKinematic=true;
 log.Dispose();File.WriteAllLines(Output+"/done.txt",checks);race.Flow.Pause();if(!Application.isEditor)Application.Quit(checks.Any(c=>c.StartsWith("FAIL"))?1:0);
 }
 void Seed(){race.Progress.Restart();race.Progress.Cross(0,true,Time.timeAsDouble);race.Progress.Cross(1,true,Time.timeAsDouble);race.ResetSampling(car.Body.position,Time.timeAsDouble);}
 IEnumerator Place(Vector3 p,Vector3 direction,float speed){car.enabled=false;car.GetComponent<VehicleInput>().enabled=false;p.y=Ground(p)+.55f;car.Body.isKinematic=false;car.Body.position=p;car.Body.rotation=Quaternion.LookRotation(direction);car.transform.SetPositionAndRotation(p,car.Body.rotation);car.Body.linearVelocity=car.Body.angularVelocity=Vector3.zero;car.ClearSteering();Physics.SyncTransforms();for(int n=0;n<35;n++){car.Simulate(0,0,0,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();}var tangent=direction+Vector3.up*((Ground(p+direction)-Ground(p-direction))*.5f);car.Body.linearVelocity=tangent.normalized*speed;Physics.SyncTransforms();}
 float Steer(Vector3 target){var local=Quaternion.Inverse(Quaternion.LookRotation(Vector3.ProjectOnPlane(car.transform.forward,Vector3.up)))*(target-car.Body.position);float length=Math.Max(3,new Vector2(local.x,local.z).magnitude);float angle=Mathf.Atan2(local.x,local.z);return Mathf.Clamp(Mathf.Atan(2*car.wheelbase*Mathf.Sin(angle)/length)/(Mathf.Lerp(car.slowSteerAngle,car.fastSteerAngle,Mathf.Clamp01(car.ForwardSpeed/car.topSpeed))*Mathf.Deg2Rad),-1,1);}
 void Trace(string name,float time){var p=car.Body.position;log.WriteLine($"{name},{time:F3},{p.x:F3},{p.y:F3},{p.z:F3},{car.ForwardSpeed:F3},{car.GroundedWheels},{race.Progress.NextGate},{resets}");log.Flush();}
 IEnumerator Drive(string name,float targetSpeed,float limit,bool success,bool maneuver){float begin=Time.time,entry=-1,air=0,maxAir=0,minY=999,maxSide=0;int supported=0;float minUp=1,maxUpSpeed=0;bool shortContact=false,reached=false;float reachedAt=0;int wp=0;var waypoints=new[]{lip+axis*25+side*3,lip+axis*32-side*3,lip+axis*42,lip+axis*57};
 while(Time.time-begin<limit){float along=Vector3.Dot(car.Body.position-lip,axis);if(entry<0&&along>=-2)entry=Time.time;minY=Math.Min(minY,car.Body.position.y);if(car.GroundedWheels==0&&along>0){air+=Time.fixedDeltaTime;maxAir=Math.Max(maxAir,air);}else air=0;if(along>5&&along<45&&car.GroundedWheels>=2){shortContact=true;supported++;maxSide=Math.Max(maxSide,Math.Abs(Vector3.Dot(car.Body.position-lip,side)));}
 if(along>=55&&car.GroundedWheels>=2&&!reached){reached=true;reachedAt=Time.time;if(success)clearTime=Time.time-entry;else if(!maneuver)failedTime=Time.time-entry;}
 if(reached&&(!success||race.Progress.NextGate==3))break;
 Vector3 target=lip+axis*(along+10);if(maneuver){while(wp<waypoints.Length-1&&Vector3.ProjectOnPlane(waypoints[wp]-car.Body.position,Vector3.up).magnitude<3)wp++;target=waypoints[wp];}else if(success&&along>55){float station=race.road.Project(car.Body.position,out _);target=race.road.At(station+10,out _);}
 float speed=car.ForwardSpeed;car.Simulate(speed<targetSpeed?1:0,speed>targetSpeed+2?.2f:0,Steer(target),Time.fixedDeltaTime);yield return new WaitForFixedUpdate();AudioListener.volume=0;Trace(name,Time.time-begin);if(car.GroundedWheels>=2)minUp=Math.Min(minUp,car.transform.up.y);maxUpSpeed=Math.Max(maxUpSpeed,car.Body.linearVelocity.y);
 }
 Check(minUp>.5f&&maxUpSpeed<16,$"{name}: no ordinary-contact flip or launch; minUp={minUp:F3}, maxUpSpeed={maxUpSpeed:F2}");
 if(success)Check(reached&&!shortContact&&maxAir>1,$"Normal 32m/s approach clears entire dump: air={maxAir:F2}s, shortContact={shortContact}, crossing={clearTime:F2}s");
 else if(!maneuver)Check(reached&&shortContact&&minY<63&&supported>10,$"{name} short 8m/s jump lands in bowl and full-throttle climbs out: minY={minY:F2}, supportedFrames={supported}, crossing={failedTime:F2}s");
 else Check(reached&&wp==3&&maxSide>1&&supported>50,$"Steering around floor and climbing escape slope: waypoints={wp+1}, supportedFrames={supported}, maxSide={maxSide:F2}m");
 }
}
}
