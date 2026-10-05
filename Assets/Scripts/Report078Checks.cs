#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

namespace Racer {
// 0.78 targeted checks (muted, isolated save). PROBE_CASES="case:args;..." starting in Street Loop Forward.
//  wave:profile            Free Roam: the fist wave for both bodies (shirt types rotated), pressed with an emulated F key
//                          (and RB once); chase and first-person shots at the top of the gesture; the other hand stays put.
//  classic                 Model: Classic — no arms rig, the button does nothing.
//  handling:profile        the same scripted 6 s of driving twice from the same pose, the second time with the fist button
//                          spammed and a celebration: the vehicle ends in exactly the same place (gestures are visual only).
//  race:scene:profile:who  one lap against 3 AI; who = player (the test pilot drives the player and wins) or ai (the player
//                          waits at the start and an AI wins): only the winner celebrates; shots of the celebration.
//  shunt:scene:profile     a race with 3 AI: the player rams an AI from behind until it shakes a fist (aimed back at the player).
//  racecmp:scene:profile  one lap with 3 AI (the test pilot drives the player) with Scenery New, then Classic: times, gates.
//  jumpcmp:profile         Free Roam, Summit Homeward giant jump from rest with New, then Classic: same flight.
//  scenery:scene:views     the scene in Free Roam (FreeRoamWorld) or ready to race (a course): New built (counts, build time),
//                          views = car or x,y,z,yaw[,pitch], separated by |;
//                          every collider identical with New and Classic, shots of each view (x,y,z,yaw;...) New / Classic.
[DefaultExecutionOrder(1000)]
public sealed class Report078Checks:MonoBehaviour {
 Action lateOnce;void LateUpdate(){var a=lateOnce;lateOnce=null;a?.Invoke();}
 IEnumerator Late(Action a){bool done=false;lateOnce=()=>{a();done=true;};while(!done)yield return null;}
 RaceDirector race;RaceFlow flow;readonly List<string> rows=new();string output,saveDir;int fails;
 void Note(string s){rows.Add(s);File.WriteAllLines(output+"/results.txt",rows);Debug.Log("REPORT078 "+s);}
 void Check(bool ok,string s){if(!ok)fails++;Note((ok?"PASS ":"FAIL ")+s);}
 static string Scene=>SceneManager.GetActiveScene().name;
 IEnumerator Start(){DontDestroyOnLoad(gameObject);AudioListener.volume=0;output=Environment.GetEnvironmentVariable("PROBE_OUT")+"/checks";Directory.CreateDirectory(output);Application.runInBackground=true;
  saveDir=Path.GetFullPath("Temp/Report078Save");if(Directory.Exists(saveDir))Directory.Delete(saveDir,true);Directory.CreateDirectory(saveDir);
  if(Scene!="StreetLoopGreybox"){SceneManager.LoadScene("StreetLoopGreybox");yield return null;yield return null;}
  Bind();yield return null;flow.UseValidationSave(saveDir);
  var title=FindAnyObjectByType<StartupTitle>();if(title)Destroy(title.gameObject);typeof(StartupTitle).GetField("completed",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,true);flow.EnterMenuAfterTitle();
  flow.Save.Settings.master=0;flow.Save.SaveSettings();yield return null;

  foreach(var spec in (Environment.GetEnvironmentVariable("PROBE_CASES")??"").Split(';').Where(x=>x.Length>0)){var a=spec.Split(':');Note("---- "+spec);
   IEnumerator run=a[0] switch{"wave"=>Wave(a[1]),"classic"=>Classic(),"handling"=>Handling(a[1]),"race"=>RaceCase(a[1],a[2],a[3]),"shunt"=>Shunt(a[1],a[2]),"scenery"=>SceneryCase(a[1],a.Length>2?a[2]:""),"racecmp"=>RaceCompare(a[1],a[2]),"jumpcmp"=>JumpCompare(a[1]),_=>null};
   if(run!=null)yield return run;else Note("unknown case "+spec);Bind();}
  Note($"done, failures {fails}");File.WriteAllLines(output+"/done.txt",rows);UnityEditor.EditorApplication.Exit(0);}
 void Bind(){race=FindAnyObjectByType<RaceDirector>();flow=race?race.Flow:null;}
 IEnumerator Load(string scene){if(Scene!=scene){SceneManager.LoadScene(scene);yield return null;yield return null;}Bind();float t0=Time.realtimeSinceStartup;while(flow.Save==null&&Time.realtimeSinceStartup-t0<20)yield return null;yield return null;AudioListener.volume=0;}
 IEnumerator Menu(){Bind();if(flow.State!=RaceFlow.Stage.Ready){flow.Pause();flow.QuitRace();}yield return null;yield return null;Bind();}
 IEnumerator EnterRoam(string course,string vehicle){yield return Load(course);yield return Menu();
  flow.Save.Settings.vehicleId=vehicle;flow.Save.SaveSettings();flow.StartFreeRoam();float t0=Time.realtimeSinceStartup;
  while((Scene!=RaceFlow.RoamScene||flow==null||flow.State!=RaceFlow.Stage.Racing)&&Time.realtimeSinceStartup-t0<60){yield return null;Bind();AudioListener.volume=0;}
  Time.timeScale=1;yield return new WaitForSeconds(.5f);}
 ArcadeVehicle Car=>race.vehicle;
 void Put(ArcadeVehicle car,Vector3 p,Quaternion r,Vector3 v){car.Body.isKinematic=false;car.transform.SetPositionAndRotation(p,r);car.Body.position=p;car.Body.rotation=r;car.Body.linearVelocity=v;car.Body.angularVelocity=Vector3.zero;car.ClearSteering();Physics.SyncTransforms();if(car==Car){race.ResetSampling(p,race.Clock);car.GetComponent<VehicleRespawn>().CancelRecovery();FindAnyObjectByType<ChaseCamera>()?.Snap();}}
 float Steer(ArcadeVehicle car,Vector3 target){var d=Quaternion.Inverse(Quaternion.LookRotation(Vector3.ProjectOnPlane(car.transform.forward,Vector3.up)))*(target-car.Body.position);return Mathf.Clamp(Mathf.Atan2(d.x,d.z)*1.6f,-1,1);}
 string Shot(string name,int w=1600,int h=900){var f=$"{output}/{name}.png";ThreeFeatureValidation.CaptureUi(f,w,h);return f;}
 RaceRoad road;int roadDir=1;
 void PickRoad(){var p=Car.Body.position;float best=1e9f;foreach(var r in FindObjectsByType<RaceRoad>(FindObjectsSortMode.None)){if(r.points==null||r.points.Length<2)continue;float s=r.Project(p,out _);var q=r.At(s,out var f);float d=Vector3.Distance(q,p);if(d<best){best=d;road=r;roadDir=Vector3.Dot(f,Car.transform.forward)>=0?1:-1;}}}
 IEnumerator Drive(float seconds,float pace,Action each=null){var car=Car;if(!road)PickRoad();car.enabled=false;float t0=Time.time;
  while(Time.time-t0<seconds){yield return new WaitForFixedUpdate();var p=car.Body.position;float st=road.Project(p,out _);var tgt=road.At(st+roadDir*12,out _);float sp=car.ForwardSpeed;
   car.Simulate(sp<pace?1:0,sp>pace+2?.5f:0,Steer(car,tgt),Time.fixedDeltaTime);each?.Invoke();}
  car.enabled=true;}
 static Transform Find(Transform root,string name)=>root.GetComponentsInChildren<Transform>(true).FirstOrDefault(t=>t.name==name);
 // Press a key / button on an emulated device for two frames.
 IEnumerator Press(bool pad){InputDevice d=pad?InputSystem.AddDevice<Gamepad>():InputSystem.AddDevice<Keyboard>();
  if(pad)InputSystem.QueueStateEvent((Gamepad)d,new GamepadState{buttons=1u<<(int)GamepadButton.RightShoulder});else InputSystem.QueueStateEvent((Keyboard)d,new KeyboardState(Key.F));
  yield return null;yield return null;if(pad)InputSystem.QueueStateEvent((Gamepad)d,new GamepadState());else InputSystem.QueueStateEvent((Keyboard)d,new KeyboardState());yield return null;InputSystem.RemoveDevice(d);}

 // ---------- fist wave ----------
 IEnumerator Wave(string profile){yield return EnterRoam("StreetLoopGreybox",profile);var views=CameraViews.Current;var cam=Camera.main;
  for(int body=0;body<2;body++){
   var look=flow.Save.Settings.rider;look.body=body;look.shirt=(body+(profile=="moto"?0:profile=="atv"?1:2))%3;look.hat=body==0?1:4;RiderLook.Player=look;var cfg=Car.GetComponent<VehicleConfiguration>();cfg.Apply(cfg.profileId);yield return null;
   string who=$"{profile} {RiderLook.Bodies[body]} {RiderLook.Shirts[look.shirt]}";
   var g=Car.GetComponent<RiderGestures>();var visual=Car.transform.Find("Vehicle visual");
   Check(g&&visual.GetComponentInChildren<RiderArms>()?.Ready==true,$"{who}: arms rigged (shoulder, elbow, wrist on both sides)");
   foreach(var view in new[]{CameraViews.View.Chase,CameraViews.View.FirstPerson}){views.SetPlayerView(view);PickRoad();yield return Drive(2.5f,11);
    var wristL=Find(visual,"Wrist L");var wristR=Find(visual,"Wrist R");var shoulderL=Find(visual,"Shoulder L");var head=visual.GetComponentsInChildren<Renderer>().First(r=>r.name.Contains("_Base_")&&r.name.EndsWith("__eyes"));
    Vector3 restR=Car.transform.InverseTransformPoint(wristR.position),restL=Car.transform.InverseTransformPoint(wristL.position);
    int before=g.Waves;bool pad=true;/* the batch editor has no focused Game view, so only gamepads reach the game (F is checked as a binding below) */yield return Press(pad);float t0=Time.time;
    bool started=g.Waves==before+1;float peakW=0,maxRight=0,maxLift=0;bool inView=false,shot=false;Vector3 peakLocal=Vector3.zero;
    yield return Drive(RiderGestures.WaveSeconds+.4f,11,()=>{});
    // sample over the gesture in LateUpdate order (after RiderGestures)
    var fistAction=Car.GetComponent<VehicleInput>().CurrentBindings[4];
    Note($"{who} {view}: pressed {(pad?"RB (emulated pad)":"F (emulated keyboard)")}, started {started}; menu blocked {MenuInput.Blocked}, map owns input {FindAnyObjectByType<ExplorationMap>()?.OwnsInput}, action enabled {fistAction.enabled} controls {string.Join(",",fistAction.controls.Select(c=>c.path))}; gesture {g.Current} age {g.Age:F2} arms {g.HasArms}");
    // replay the measurement on a second wave after the cooldown so samples are taken while it runs
    yield return new WaitForSeconds(RiderGestures.PlayerCooldown+.2f);before=g.Waves;g.Wave();t0=Time.time;
    while(Time.time-t0<RiderGestures.WaveSeconds+.2f){yield return Late(()=>{
      peakW=Mathf.Max(peakW,g.Weight);var l=Car.transform.InverseTransformPoint(wristL.position);float lift=l.y-restL.y;if(lift>maxLift){maxLift=lift;peakLocal=l;}
      maxRight=Mathf.Max(maxRight,Vector3.Distance(Car.transform.InverseTransformPoint(wristR.position),restR));
      if(g.Weight>.97f&&!shot){shot=true;var vp=cam.WorldToViewportPoint(wristL.position);inView=vp.z>0&&vp.x>0&&vp.x<1&&vp.y>0&&vp.y<1;Shot($"wave-{profile}-{RiderLook.Bodies[body]}-{view}");}});}
    var headLocal=Car.transform.InverseTransformPoint(head.transform.TransformPoint(head.GetComponent<MeshFilter>().sharedMesh.bounds.center));
    bool car=!Car.GetComponent<VehicleConfiguration>().Profile.Small;
    bool raised=car?peakLocal.x<-.85f&&maxLift>.15f:peakLocal.y>headLocal.y-.15f;
    bool fp=view==CameraViews.View.FirstPerson;if(fp)raised=maxLift>.15f&&peakLocal.z>headLocal.z+.15f;
    yield return Late(()=>{});yield return Late(()=>{});
    bool ended=g.Current==RiderGestures.Kind.None,controls=Car.GetComponent<VehicleInput>().enabled;
    var armsNow=visual.GetComponentInChildren<RiderArms>();Note($"  end: gesture {g.Current} age {g.Age:F2} arms {g.HasArms}; seen from eyes {armsNow?.SeenFromEyes}; camera {Vector3.Distance(cam.transform.position,head.bounds.center):F2} m from the eyes; main camera {cam.name}");
    bool pass=started&&g.Waves==before+1&&peakW>.97f&&raised&&maxRight<.01f&&(!fp||inView)&&ended&&controls;
    Check(pass,$"{who} {view}: button started it {started}; peak weight {peakW:F2}; fist raised {maxLift:F2} m to vehicle-local {peakLocal:F2} (eyes {headLocal:F2}){(car&&!fp?" — out of the driver's window":fp?" — ahead of the face":"")} ok {raised}; other hand moved {maxRight:F3} m; fist in the camera view {inView}; ended {ended}; controls on {controls}");}}
  var bindings=Car.GetComponent<VehicleInput>().CurrentBindings[4].bindings.Select(b=>b.path).ToArray();
  Check(bindings.Contains("<Keyboard>/f")&&bindings.Contains("<Gamepad>/rightShoulder"),$"fist wave bindings: {string.Join(", ",bindings)}");
  // cooldown: an immediate second press does nothing; after it, works again
  var gg=Car.GetComponent<RiderGestures>();gg.Wave();int n=gg.Waves;bool again=gg.Wave();Check(!again&&gg.Waves==n,$"{profile}: a second press during the gesture / cooldown is ignored");
  views.SetPlayerView(CameraViews.View.Chase);RiderLook.Player=flow.Save.Settings.rider=new RiderLook();var c2=Car.GetComponent<VehicleConfiguration>();c2.Apply(c2.profileId);}

 IEnumerator Classic(){yield return EnterRoam("StreetLoopGreybox","moto");VehicleVisual.NewModels=false;var cfg=Car.GetComponent<VehicleConfiguration>();cfg.Apply(cfg.profileId);yield return null;
  var g=Car.GetComponent<RiderGestures>();int n=g.Waves;yield return Press(false);yield return null;
  Check(!Car.GetComponentInChildren<RiderArms>()&&g.Waves==n&&!g.Wave()&&!g.Celebrate(),"Model: Classic — no arm rig; F, Wave() and Celebrate() do nothing");
  VehicleVisual.NewModels=true;cfg.Apply(cfg.profileId);}

 // ---------- handling unchanged ----------
 IEnumerator Handling(string profile){yield return EnterRoam("StreetLoopGreybox",profile);var car=Car;PickRoad();yield return Drive(2,8);
  var p0=car.Body.position;var r0=car.Body.rotation;var g=car.GetComponent<RiderGestures>();var results=new List<(Vector3 p,Quaternion r,Vector3 v)>();
  // runs 0 and 1 without gestures (the control: how repeatable the same drive is), run 2 with them spammed
  for(int run=0;run<3;run++){car.enabled=false;Put(car,p0,r0,Vector3.zero);yield return new WaitForFixedUpdate();Put(car,p0,r0,Vector3.zero);
   for(int i=0;i<300;i++){yield return new WaitForFixedUpdate();if(run==2){g.Wave();if(i==150)g.Celebrate();}
    car.Simulate(i<200?1:0,i>=200?.6f:0,Mathf.Sin(i*.03f)*.6f,Time.fixedDeltaTime);}
   results.Add((car.Body.position,car.Body.rotation,car.Body.linearVelocity));car.enabled=true;}
  string Gap(int a,int b,out float dp){dp=Vector3.Distance(results[a].p,results[b].p);return $"{dp:F4} m / {Quaternion.Angle(results[a].r,results[b].r):F3} deg / {(results[a].v-results[b].v).magnitude:F4} m/s";}
  string control=Gap(0,1,out float d01),with=Gap(1,2,out float d12);
  Check(d12<=Mathf.Max(.01f,d01*1.5f+.01f),$"{profile}: the same 6 s of driving from the same pose: two runs without gestures end {control} apart (the physics' own repeatability); with the fist button spammed and a celebration ({g.Waves} waves, {g.Celebrations} celebration) {with} from the last run without");}

 // ---------- races ----------
 IEnumerator StartRace(string scene,string profile){yield return Load(scene);yield return Menu();var s=flow.Save.Settings;s.weather=0;s.timeOfDay=0;s.estimateAiFinishes=false;flow.Save.SaveSettings();
  flow.OpenGarage();flow.SelectVehicle(profile);flow.CloseGarage();race.opponents=true;race.traffic=false;race.laps=1;race.difficulty=1;race.opponentRoster=new[]{"moto","atv","original"};
  flow.StartRace();float t0=Time.realtimeSinceStartup;while(flow.State!=RaceFlow.Stage.Racing&&Time.realtimeSinceStartup-t0<30){AudioListener.volume=0;yield return null;}}
 IEnumerator RaceCase(string scene,string profile,string who){yield return StartRace(scene,profile);var car=Car;RoadDriver pilot=null;
  if(who=="player"){pilot=car.gameObject.AddComponent<RoadDriver>();pilot.Initialize(race,car,true,1,1.12f);pilot.Racer=race.Racers[0];car.GetComponent<VehicleInput>().enabled=false;}
  var before=race.Racers.ToDictionary(r=>r,r=>r.Car.GetComponent<RiderGestures>()?.Celebrations??0);
  RacerState winner=null;float t1=Time.time;
  while(winner==null&&Time.time-t1<600){AudioListener.volume=0;winner=race.Racers.FirstOrDefault(r=>r.Progress.Finished);yield return null;}
  if(winner==null){Check(false,$"{scene}: nobody finished");yield break;}
  // film the winner: the chase camera for the player; a camera beside the line for an AI winner
  Camera side=null;var chaseCam=FindAnyObjectByType<ChaseCamera>();var camViews=CameraViews.Current;
  if(winner.IsAi){side=Camera.main;if(chaseCam)chaseCam.enabled=false;if(camViews)camViews.enabled=false;}
  var g=winner.Car.GetComponent<RiderGestures>();int shots=0;float tw=Time.time;
  while(Time.time-tw<RiderGestures.CelebrateSeconds){yield return Late(()=>{if(side){var w=winner.Car.transform;side.transform.position=w.position+w.right*-4.5f+w.forward*5+Vector3.up*1.6f;side.transform.LookAt(w.position+Vector3.up*1f);}
    if(g&&g.Weight>.97f&&shots<2&&Time.frameCount%20==0){shots++;Shot($"celebrate-{who}-{scene}-{profile}-{shots}");}});}
  int Delta(RacerState r)=>(r.Car.GetComponent<RiderGestures>()?.Celebrations??0)-before[r];
  var others=race.Racers.Where(r=>r!=winner).ToArray();
  Check(Delta(winner)==1&&others.All(o=>Delta(o)==0)&&(who=="ai")==winner.IsAi,$"{scene} {who} win: winner {winner.Name} ({winner.Car.GetComponent<VehicleConfiguration>().profileId}) celebrated {Delta(winner)} time(s) this race; others {string.Join(", ",others.Select(o=>o.Name+" "+Delta(o)))}; {shots} shots");
  if(side){if(chaseCam)chaseCam.enabled=true;if(camViews)camViews.enabled=true;}
  while(flow.State!=RaceFlow.Stage.Results&&Time.time-t1<700){AudioListener.volume=0;yield return null;}
  if(pilot){Destroy(pilot);car.GetComponent<VehicleInput>().enabled=true;}
  Note($"{scene}: results {flow.State}, player finished {race.Progress.Finished}");yield return Menu();}

 IEnumerator Shunt(string scene,string profile){yield return StartRace(scene,profile);var car=Car;yield return new WaitForSeconds(6);int before=RiderGestures.AiWaves;bool waved=false;string log="";
  for(int attempt=0;attempt<8&&!waved;attempt++){
   var ai=race.Racers.Where(r=>r.IsAi).Select(r=>r.Car).Where(c=>c.transform.up.y>.8f).OrderBy(c=>Vector3.Distance(c.transform.position,car.transform.position)).FirstOrDefault();if(!ai)break;
   var g=ai.GetComponent<RiderGestures>();int w0=g.Waves;var fwd=Vector3.ProjectOnPlane(ai.transform.forward,Vector3.up).normalized;
   Put(car,ai.Body.position-fwd*6+Vector3.up*.15f,Quaternion.LookRotation(fwd),ai.Body.linearVelocity+fwd*10);car.enabled=false;
   float t0=Time.time;while(Time.time-t0<3.2f){yield return new WaitForFixedUpdate();car.Simulate(Time.time-t0<1?1:0,Time.time-t0>1?1:0,Steer(car,ai.Body.position),Time.fixedDeltaTime);
    if(g.Waves>w0&&!waved){waved=true;yield return new WaitForSeconds(.7f);yield return Late(()=>Shot($"shunt-{scene}-ai-fist"));}}
   car.enabled=true;log+=$" attempt {attempt+1}: {ai.name} waves {w0}->{g.Waves};";}
  Check(waved&&RiderGestures.AiWaves>before,$"{scene}: AI shook a fist after being rammed by the player:{log}");
  yield return Menu();}
 // ---------- scenery ----------
 // PROBE_CONDITIONS=1: every scenery view at Day, Night and Day/Snow (otherwise as the scene is)
 static (string,TimeOfDay?,Weather)[] Conditions=>Environment.GetEnvironmentVariable("PROBE_CONDITIONS")=="1"?new (string,TimeOfDay?,Weather)[]{("-day",TimeOfDay.Day,Weather.Clear),("-night",TimeOfDay.Night,Weather.Clear),("-snow",TimeOfDay.Day,Weather.Snow)}:new (string,TimeOfDay?,Weather)[]{("",null,Weather.Clear)};
 static string Colliders(){var sb=new System.Text.StringBuilder();
  foreach(var c in FindObjectsByType<Collider>(FindObjectsInactive.Include).Where(c=>!c.attachedRigidbody||c.attachedRigidbody.isKinematic).OrderBy(c=>c.GetEntityId().GetHashCode()))
   sb.Append($"{c.name}|{c.GetType().Name}|{c.enabled&&c.gameObject.activeInHierarchy}|{c.isTrigger}|{c.gameObject.layer}|{c.bounds.center:F3}|{c.bounds.size:F3}"+(char)10);
  return sb.ToString();}
 IEnumerator SceneryCase(string scene,string views){
  if(scene==RaceFlow.RoamScene)yield return EnterRoam("StreetLoopGreybox","moto");
  else{yield return Load(scene);yield return Menu();race.opponents=false;race.traffic=false;race.laps=1;flow.StartRace();float tr=Time.realtimeSinceStartup;while(flow.State!=RaceFlow.Stage.Racing&&Time.realtimeSinceStartup-tr<30){AudioListener.volume=0;yield return null;}yield return new WaitForSecondsRealtime(1);}
  var world=SceneryWorld.Current;Check(world&&world.Trees,$"{Scene}: scenery world present, New {Scenery.New}");if(!world)yield break;
  Note($"{Scene}: build {world.BuildMilliseconds:F0} ms ({world.Timings}); {world.Trees.Summary}; buildings redesigned {world.Buildings?.Redesigned} ({string.Join(", ",world.Buildings?.Names??new())}), detailed {world.Buildings?.Detailed}; old renderers hidden {world.Hidden.Count}");
  Note(SceneryTrees.KitInfo+$"; first cell {world.Trees.FirstCellBounds}");
  Note("replaced: "+string.Join(", ",world.Hidden.Where(r=>r).GroupBy(r=>(r.transform.parent?r.transform.parent.name+"/":"")+r.name).Select(g=>$"{g.Key} x{g.Count()}").OrderBy(x=>x)));
    string a=Colliders();Scenery.Set(false);yield return null;string b=Colliders();bool shown=world.Hidden.All(r=>!r||!r.forceRenderingOff);Scenery.Set(true);yield return null;string c=Colliders();
  Check(a==b&&b==c&&shown&&world.Hidden.All(r=>!r||r.forceRenderingOff),$"{Scene}: colliders identical New / Classic / New ({a.Split((char)10).Length-1} static colliders, hash {a.GetHashCode():X8}); Classic shows every old renderer, New hides them");
  int i=0;foreach(var v0 in views.Split('|').Where(x=>x.Length>0)){i++;var v=v0;
   if(v=="car"){foreach(var (cond,t,w) in Conditions)foreach(var on in new[]{true,false}){if(t!=null)WorldLook.Current.Pin(LookPresets.Compose(t.Value,w));Scenery.Set(on);yield return new WaitForSecondsRealtime(t==TimeOfDay.Night?1.4f:.4f);yield return Late(()=>Shot($"scenery-{Scene}-{i}{cond}-{(on?"new":"classic")}"));}if(Conditions.Length>1)WorldLook.Current.Pin(null);Scenery.Set(true);continue;}
   if(v.StartsWith("road")){var rs=float.Parse(v.Substring(4));race.road.Initialize();var rp=race.road.At(rs,out var rf);rf=Vector3.ProjectOnPlane(rf,Vector3.up).normalized;var reye=rp-rf*9+Vector3.up*3.6f;var rr=Quaternion.LookRotation(rp+rf*30+Vector3.up*.5f-reye).eulerAngles;v=$"{reye.x},{reye.y},{reye.z},{rr.y},{(rr.x>180?rr.x-360:rr.x)}";}
   var f=v.Split(',').Select(float.Parse).ToArray();
   var cam=Camera.main;var chase=FindAnyObjectByType<ChaseCamera>();if(chase)chase.enabled=false;var views2=CameraViews.Current;if(views2)views2.enabled=false;
   cam.transform.SetPositionAndRotation(new Vector3(f[0],f[1],f[2]),Quaternion.Euler(f.Length>4?f[4]:8,f[3],0));
   foreach(var (cond,t,w) in Conditions)foreach(var on in new[]{true,false}){if(t!=null)WorldLook.Current.Pin(LookPresets.Compose(t.Value,w));Scenery.Set(on);yield return null;yield return null;yield return new WaitForSecondsRealtime(t==TimeOfDay.Night?1.4f:.3f);yield return Late(()=>{cam.transform.SetPositionAndRotation(new Vector3(f[0],f[1],f[2]),Quaternion.Euler(f.Length>4?f[4]:8,f[3],0));Shot($"scenery-{Scene}-{i}{cond}-{(on?"new":"classic")}");});}
   if(Conditions.Length>1)WorldLook.Current.Pin(null);
   Scenery.Set(true);Note($"view {i} {v}: draw calls {world.Trees.DrawCalls}");if(chase)chase.enabled=true;if(views2)views2.enabled=true;}
  if(scene!=RaceFlow.RoamScene)yield return Menu();}
 IEnumerator RaceCompare(string scene,string profile){
  var res=new List<string>();
  foreach(bool fresh in new[]{true,false}){
   Scenery.Set(fresh);yield return StartRace(scene,profile);var car=Car;var pilot=car.gameObject.AddComponent<RoadDriver>();pilot.Initialize(race,car,true,1,1);pilot.Racer=race.Racers[0];car.GetComponent<VehicleInput>().enabled=false;
   float t1=Time.time;while(flow.State!=RaceFlow.Stage.Results&&Time.time-t1<700){AudioListener.volume=0;yield return null;}
   Destroy(pilot);car.GetComponent<VehicleInput>().enabled=true;
   string Row(RacerState r)=>$"{r.Name} {(r.Progress.Finished?RaceHud.FormatTime(r.Progress.AdjustedTime(race.Clock)):r.Dnf?"DNF":"est.")} missed {r.Progress.MissedGates}";
   res.Add($"{(fresh?"New":"Classic")}: {string.Join("; ",race.Racers.Select(Row))}");
   Check(flow.State==RaceFlow.Stage.Results&&race.Progress.Finished&&race.Racers.All(r=>r.Progress.Finished&&r.Progress.MissedGates==0),$"{scene} {profile} race with 3 AI, scenery {(fresh?"New":"Classic")}: {res.Last()}");
   yield return Menu();}
  Scenery.Set(true);Note($"{scene}: compare {string.Join(" | ",res)}");}
 IEnumerator JumpCompare(string profile){yield return EnterRoam("StreetLoopGreybox",profile);var root=GameObject.Find("CR094 summit launch")?.transform;if(!root){Check(false,"no summit launch");yield break;}
  var res=new List<(float d,float air)>();
  foreach(bool fresh in new[]{true,false}){Scenery.Set(fresh);yield return null;
   var car=Car;car.enabled=false;var s0=root.TransformPoint(new Vector3(0,0,5));if(Physics.Raycast(s0+Vector3.up*10,Vector3.down,out var gh,60,~0,QueryTriggerInteraction.Ignore))s0=gh.point;Put(car,s0+Vector3.up*.7f,Quaternion.LookRotation(root.forward),Vector3.zero);
   for(float t=0;t<1.5f;t+=Time.fixedDeltaTime){yield return new WaitForFixedUpdate();car.Simulate(0,1,0,Time.fixedDeltaTime);}
   bool air=false,landed=false;float airtime=0;Vector3 take=default,land=default;float t0=Time.time;
   while(Time.time-t0<40&&!landed){yield return new WaitForFixedUpdate();var q=root.InverseTransformPoint(car.Body.position);car.Simulate(1,0,Steer(car,root.TransformPoint(new Vector3(0,0,q.z+25))),Time.fixedDeltaTime);
    if(!air&&q.z>150&&car.GroundedWheels==0){air=true;take=car.Body.position;}if(air){airtime+=Time.fixedDeltaTime;if(car.GroundedWheels>=1&&airtime>.4f){landed=true;land=car.Body.position;}}}
   if(landed&&fresh)yield return Late(()=>Shot("jump-landing-new"));
   car.enabled=true;res.Add((Vector3.ProjectOnPlane(land-take,Vector3.up).magnitude,airtime));}
  Scenery.Set(true);
  Check(res.All(r=>r.d>150)&&Mathf.Abs(res[0].d-res[1].d)<3,$"Summit Homeward giant jump ({profile}): New {res[0].d:F1} m in {res[0].air:F2} s, Classic {res[1].d:F1} m in {res[1].air:F2} s");}
}
}
#endif
