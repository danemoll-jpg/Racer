using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using Unity.Profiling;

namespace Racer {
// Opt-in measurement only. No scene component or normal-game startup cost.
public sealed class WoodlandBenchmark : MonoBehaviour {
 [Serializable] public class Route { public string name; public Vector3[] points; public float speed=7; public bool fixedView; public float lookAhead; }
 [Serializable] public class Plan { public Route[] routes; public bool disableForestRendering; public float seconds=15; }
 public Plan plan; public string output; public bool quit; public int repeats=2;
 ArcadeVehicle car; Gamepad pad; int section,index; float start,peak,minUp,maxError,distance; Vector3 previous;
 readonly List<double> frames=new(),main=new(),render=new(),physics=new();
 readonly List<string> rows=new(); ProfilerRecorder mainRec,renderRec,physicsRec,drawRec,triRec; Renderer[] disabledRenderers;
 InputSettings.BackgroundBehavior background; bool run; ChaseCamera chase;
 float oldTimeScale;
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Boot(){
  var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-woodlandBenchmark");
  if(i<0||i+2>=args.Length)return;
  Begin(args[i+1],args[i+2],true);
 }
 public static WoodlandBenchmark Begin(string file,string report,bool exit=false){
  var b=new GameObject("CR-014 opt-in benchmark").AddComponent<WoodlandBenchmark>();b.plan=JsonUtility.FromJson<Plan>(File.ReadAllText(file));b.output=report;b.quit=exit;return b;
 }
 void Start(){
  car=FindAnyObjectByType<ArcadeVehicle>();chase=Camera.main.GetComponent<ChaseCamera>();
  if(plan.disableForestRendering){disabledRenderers=GameObject.Find("Woods replacing later subdivisions").GetComponentsInChildren<Renderer>();foreach(var r in disabledRenderers)r.enabled=false;}
  background=InputSystem.settings.backgroundBehavior;InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
  run=Application.runInBackground;Application.runInBackground=true;oldTimeScale=Time.timeScale;Time.timeScale=1;
  pad=InputSystem.AddDevice<Gamepad>();
  mainRec=ProfilerRecorder.StartNew(ProfilerCategory.Internal,"Main Thread",1);
  renderRec=ProfilerRecorder.StartNew(ProfilerCategory.Internal,"Render Thread",1);
  physicsRec=ProfilerRecorder.StartNew(ProfilerCategory.Physics,"Physics.Simulate",1);
  drawRec=ProfilerRecorder.StartNew(ProfilerCategory.Render,"Draw Calls Count",1);triRec=ProfilerRecorder.StartNew(ProfilerCategory.Render,"Triangles Count",1);
  rows.Add($"CR-014 ordinary-frame virtual Gamepad; {Application.unityVersion}; editor={Application.isEditor}; {Screen.width}x{Screen.height}; quality={QualitySettings.names[QualitySettings.GetQualityLevel()]}; vSync={QualitySettings.vSyncCount}; target={Application.targetFrameRate}; CPU={SystemInfo.processorType}; GPU={SystemInfo.graphicsDeviceName}; RAM={SystemInfo.systemMemorySize}MB; graphics={SystemInfo.graphicsDeviceType}; shadowDistance={QualitySettings.shadowDistance}; fixedDelta={Time.fixedDeltaTime}");
  rows.Add($"Recorder validity main={mainRec.Valid}, render={renderRec.Valid}, physics={physicsRec.Valid}, draws={drawRec.Valid}, triangles={triRec.Valid}. CPU markers include waits; no GPU timing claim. 3s warmup + {Mathf.Max(15,plan.seconds)-3}s measured per section; repetitions={repeats}; forestRenderingDisabled={plan.disableForestRendering}. Initial pose only is teleported, no manual physics stepping.");
  rows.Add("run,route,n,median_ms,p95_ms,p99_ms,max_ms,over33ms,main_median_ms,render_median_ms,physics_median_ms,allocated_MB,reserved_MB,peak_mps,min_up,max_path_error_m,distance_m,end_index,physics_p95_ms,last_draws,last_triangles");
  Next();
 }
 void Next(){
  var r=plan.routes[section%plan.routes.Length];index=0;start=Time.realtimeSinceStartup;peak=0;minUp=1;maxError=0;distance=0;
  var p=r.points[0];var f=r.points[1]-p;f.y=0;
  car.Body.position=p+Vector3.up*.7f;car.Body.rotation=Quaternion.LookRotation(f);car.transform.SetPositionAndRotation(car.Body.position,car.Body.rotation);car.Body.linearVelocity=car.Body.angularVelocity=Vector3.zero;car.ClearSteering();Physics.SyncTransforms();previous=car.Body.position;
  chase.enabled=true;chase.Snap();if(r.fixedView){chase.enabled=false;Camera.main.transform.SetPositionAndRotation(p+Vector3.up*3,Quaternion.LookRotation(f));}
  frames.Clear();main.Clear();render.Clear();physics.Clear();
 }
 static double Percentile(List<double> a,double q){if(a.Count==0)return -1;var b=a.OrderBy(x=>x).ToArray();return b[Math.Min(b.Length-1,(int)(q*b.Length))];}
 void Update(){
  if(car==null||plan==null)return;var r=plan.routes[section%plan.routes.Length];float elapsed=Time.realtimeSinceStartup-start;
  if(elapsed>=3){frames.Add(Time.unscaledDeltaTime*1000);if(mainRec.Valid)main.Add(mainRec.LastValue/1e6);if(renderRec.Valid)render.Add(renderRec.LastValue/1e6);if(physicsRec.Valid)physics.Add(physicsRec.LastValue/1e6);}
  float best=float.MaxValue;for(int j=Math.Max(0,index-4);j<Math.Min(r.points.Length,index+20);j++){var d=car.Body.position-r.points[j];d.y=0;if(d.sqrMagnitude<best){best=d.sqrMagnitude;index=j;}}
  int ahead=index+Math.Max(2,Mathf.RoundToInt(Mathf.Abs(car.ForwardSpeed)*.3f));
  if(r.lookAhead>0){ahead=index;float length=0;while(ahead<r.points.Length-1&&length<r.lookAhead){length+=Vector3.Distance(r.points[ahead],r.points[ahead+1]);ahead++;}}
  var target=r.points[Math.Min(r.points.Length-1,ahead)];
  var local=car.transform.InverseTransformPoint(target);float curvature=2*local.x/Mathf.Max(1,local.x*local.x+local.z*local.z);
  float angle=Mathf.Lerp(car.slowSteerAngle,car.fastSteerAngle,Mathf.Clamp01(Mathf.Abs(car.ForwardSpeed)/car.topSpeed));
  float steer=Mathf.Clamp(Mathf.Atan(curvature*car.wheelbase)*Mathf.Rad2Deg/angle,-1,1);float stick=Mathf.Abs(steer)<.00001f?0:Mathf.Sign(steer)*(.12f+.83f*Mathf.Abs(steer));
  float desired=r.fixedView||index>=r.points.Length-3?0:r.speed;
  InputSystem.QueueStateEvent(pad,new GamepadState{leftStick=new Vector2(stick,0),rightTrigger=Mathf.Clamp01((desired-car.ForwardSpeed)*.7f+.35f)*(desired>0?1:0),leftTrigger=car.ForwardSpeed>desired+.3f?Mathf.Clamp01((car.ForwardSpeed-desired)*.5f):0});
  peak=Mathf.Max(peak,car.ForwardSpeed);minUp=Mathf.Min(minUp,car.transform.up.y);maxError=Mathf.Max(maxError,Mathf.Sqrt(best));distance+=Vector3.Distance(previous,car.Body.position);previous=car.Body.position;
  if(elapsed<Mathf.Max(15,plan.seconds))return;
  rows.Add(FormattableString.Invariant($"{section/plan.routes.Length+1},{r.name},{frames.Count},{Percentile(frames,.5):F3},{Percentile(frames,.95):F3},{Percentile(frames,.99):F3},{Percentile(frames,1):F3},{frames.Count(x=>x>33.33)},{Percentile(main,.5):F3},{Percentile(render,.5):F3},{Percentile(physics,.5):F3},{UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong()/1048576.0:F1},{UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong()/1048576.0:F1},{peak:F2},{minUp:F3},{maxError:F2},{distance:F1},{index},{Percentile(physics,.95):F3},{(drawRec.Valid?drawRec.LastValue:-1)},{(triRec.Valid?triRec.LastValue:-1)}"));
  File.WriteAllLines(output,rows);section++;if(section<plan.routes.Length*repeats){Next();return;}
  Destroy(gameObject);if(quit)Application.Quit();
 }
 void OnDestroy(){if(pad!=null&&pad.added)InputSystem.RemoveDevice(pad);mainRec.Dispose();renderRec.Dispose();physicsRec.Dispose();drawRec.Dispose();triRec.Dispose();if(disabledRenderers!=null)foreach(var r in disabledRenderers)if(r)r.enabled=true;InputSystem.settings.backgroundBehavior=background;Application.runInBackground=run;Time.timeScale=oldTimeScale;if(chase)chase.enabled=true;}
}
}
