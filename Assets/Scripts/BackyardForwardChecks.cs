using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Racer {
// Explicit isolated technical check. Never instantiated in ordinary gameplay.
public sealed class BackyardForwardChecks:MonoBehaviour {
public static string Output="Docs/BackyardForward/drive";
readonly List<string> checks=new();
void OnEnable()=>SceneManager.sceneLoaded+=OnTestScene;
void OnDisable()=>SceneManager.sceneLoaded-=OnTestScene;
void OnTestScene(Scene scene,LoadSceneMode mode){foreach(var t in FindObjectsByType<ContinuationTraffic>())t.enabled=false;}
void Check(bool ok,string label){checks.Add((ok?"PASS ":"FAIL ")+label);File.WriteAllLines(Output+"/checks.txt",checks);}
public static void Launch(){var go=new GameObject("Forward targeted check");DontDestroyOnLoad(go);go.AddComponent<BackyardForwardChecks>();}
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-backyardForwardCheck");if(at<0||Array.IndexOf(args,"-racerTestSave")<0)return;Output=args[at+1];Launch();}
IEnumerator Start(){Directory.CreateDirectory(Output);Application.runInBackground=true;AudioListener.volume=0;yield return null;yield return null;
var initial=FindAnyObjectByType<RaceDirector>();
#if UNITY_EDITOR
initial.Flow.UseValidationSave(Path.GetFullPath("Temp/BackyardForwardCheckSave"));
#endif
initial.Flow.OpenCourses();yield return null;
var button=FindObjectsByType<UnityEngine.UI.Button>().Single(b=>b.gameObject.activeInHierarchy&&b.GetComponentsInChildren<UnityEngine.UI.Text>().Any(t=>t.text=="Dan's Backyard - Forward"));button.onClick.Invoke();yield return null;yield return null;
var race=FindAnyObjectByType<RaceDirector>();Check(SceneManager.GetActiveScene().name=="DansBackyardForward"&&race.courseId=="backyard-forward-v2-terrain","Forward launches via menu");AudioListener.volume=0;
#if UNITY_EDITOR
race.Flow.UseValidationSave(Path.GetFullPath("Temp/BackyardForwardCheckSave"));
#endif
race.Flow.OpenGarage();race.Flow.SelectVehicle("moto");race.Flow.CloseGarage();race.laps=1;race.opponents=true;race.traffic=false;race.difficulty=1;race.opponentRoster=new[]{"moto","atv","moto"};race.Flow.Save.Settings.estimateAiFinishes=false;
race.Racers[0]=new RacerState("YOU reference pilot",race.vehicle,race.gates.Length-1,1);race.Flow.StartRace();
Check(race.Racers.All(r=>Vector3.Dot(r.Car.transform.forward,Vector3.left)>.97f),"Player and AI start straight west along accepted driveway");
Check(race.Branches.Length==0,"No optional shortcut entitlement");Check(FindAnyObjectByType<RacingMiniMap>()!=null,"Existing minimap active");
Check(race.gates.Skip(1).All(g=>g.GetComponentsInChildren<Renderer>().Where(r=>r.name.Contains("marking")).All(r=>r.sharedMaterial.name=="RaceGate")),"All intermediate gates use accepted blue checkpoint material");
Check(race.gates.All(g=>g.GetComponentsInChildren<Collider>().Length==0),"Gates have no colliders");
var pilot=race.vehicle.gameObject.AddComponent<RoadDriver>();pilot.Initialize(race,race.vehicle,true,1,1);pilot.Racer=race.Racers[0];
float begin=Time.realtimeSinceStartup,next=0;var air=new float[4];var maxAir=new float[4];
using(var log=new StreamWriter(Output+"/pace.csv")){log.WriteLine("time,name,x,y,z,speed,target,grounded,nextGate,misses,recoveries,obstacle");while(!race.ClassificationFinal&&Time.realtimeSinceStartup-begin<240){yield return null;AudioListener.volume=0;if(race.Flow.State!=RaceFlow.Stage.Racing)continue;for(int i=0;i<race.Racers.Count;i++){var r=race.Racers[i];if(r.Car.GroundedWheels==0)air[i]+=Time.deltaTime;else air[i]=0;maxAir[i]=Mathf.Max(maxAir[i],air[i]);}if(Time.time<next)continue;next=Time.time+.25f;foreach(var r in race.Racers){var d=r.Car.GetComponent<RoadDriver>();var p=r.Car.Body.position;log.WriteLine($"{race.Clock:F2},{r.Name},{p.x:F3},{p.y:F3},{p.z:F3},{r.Car.ForwardSpeed:F2},{d.TargetSpeed:F2},{r.Car.GroundedWheels},{r.Progress.NextGate},{r.Progress.MissedGates},{d.RecoveryCount},{d.LastObstacle}");}log.Flush();}}
for(int i=0;i<race.Racers.Count;i++){var r=race.Racers[i];Check(r.Progress.Finished&&!r.Estimated&&!r.Dnf,r.Name+" physical full lap; misses="+r.Progress.MissedGates+" recoveries="+r.Car.GetComponent<RoadDriver>().RecoveryCount+" longestAir="+maxAir[i].ToString("F2"));}
ScreenCapture.CaptureScreenshot(Path.GetFullPath(Output+"/finish.png"));File.WriteAllLines(Output+"/done.txt",checks);pilot.enabled=false;race.Flow.Pause();
if(!Application.isEditor)Application.Quit(checks.Any(c=>c.StartsWith("FAIL"))?1:0);
}
}}
