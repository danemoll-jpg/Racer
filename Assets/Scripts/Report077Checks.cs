#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Racer {
// 0.77 targeted checks (muted, isolated save). PROBE_CASES="case:args;..." starting in Street Loop Forward.
//  views:profile          Free Roam: the four player views on one vehicle, by day and at night in rain (shots + camera
//                         placement: first person at the rider's eyes with the head hidden, front ahead of the bodywork).
//  persist                the chosen view is saved and read back by a fresh load of the settings and of the scene.
//  fpjump:profile         first person through the Summit Homeward giant jump, then a forced wipeout and a reset.
//  race:scene:profile:fp  one solo lap driven by the test pilot in first person: finishes and saves its record.
//  raceviews:scene:profile  a solo race at night in rain driven by the test pilot: the four views in turn (shots + placement).
//  racetrailer:scene:profile  the same lap in Trailer Mode: every trailer camera in turn, then Auto; no record saved.
//  trailer:profile        Free Roam: every trailer camera while riding; HUD hidden; guides; 4K captures at Day,
//                         Night/Rain and Dusk/Snow; the screenshot key.
//  auto:profile:seconds   Free Roam ride with the Auto camera: every cut logged; skip and hold.
//  slowjump:profile       Trailer Mode, Auto camera, 0.25x held through the Summit Homeward giant jump.
//  conditions             Trailer Mode conditions set and restored; settings.json byte-identical before and after.
[DefaultExecutionOrder(1000)]
public sealed class Report077Checks:MonoBehaviour {
 // Camera measurements and captures run here, after every camera script has placed the camera for this frame.
 Action lateOnce;readonly List<Action> lateEvery=new();
 void LateUpdate(){var a=lateOnce;lateOnce=null;a?.Invoke();foreach(var e in lateEvery.ToArray())e();}
 IEnumerator Late(Action a){bool done=false;lateOnce=()=>{a();done=true;};while(!done)yield return null;}
 RaceDirector race;RaceFlow flow;readonly List<string> rows=new();string output,saveDir;int fails;
 void Note(string s){rows.Add(s);File.WriteAllLines(output+"/results.txt",rows);Debug.Log("REPORT077 "+s);}
 void Check(bool ok,string s){if(!ok)fails++;Note((ok?"PASS ":"FAIL ")+s);}
 static string Scene=>SceneManager.GetActiveScene().name;
 IEnumerator Start(){DontDestroyOnLoad(gameObject);AudioListener.volume=0;output=Environment.GetEnvironmentVariable("PROBE_OUT")+"/checks";Directory.CreateDirectory(output);Application.runInBackground=true;
  saveDir=Path.GetFullPath("Temp/Report077Save");if(Directory.Exists(saveDir))Directory.Delete(saveDir,true);Directory.CreateDirectory(saveDir);
  if(Scene!="StreetLoopGreybox"){SceneManager.LoadScene("StreetLoopGreybox");yield return null;yield return null;}
  Bind();yield return null;flow.UseValidationSave(saveDir);
  var title=FindAnyObjectByType<StartupTitle>();if(title)Destroy(title.gameObject);typeof(StartupTitle).GetField("completed",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,true);flow.EnterMenuAfterTitle();
  flow.Save.Settings.master=0;flow.Save.SaveSettings();yield return null;
  foreach(var spec in (Environment.GetEnvironmentVariable("PROBE_CASES")??"").Split(';').Where(x=>x.Length>0)){var a=spec.Split(':');Note("---- "+spec);
   IEnumerator run=a[0] switch{"views"=>Views(a[1]),"persist"=>Persist(),"fpjump"=>FpJump(a[1]),"race"=>RaceCase(a[1],a[2],false),"racetrailer"=>RaceCase(a[1],a[2],true),"raceviews"=>RaceViews(a[1],a[2]),
    "trailer"=>TrailerCase(a[1]),"auto"=>AutoCase(a[1],float.Parse(a[2])),"slowjump"=>SlowJump(a[1]),"conditions"=>Conditions(),_=>null};
   if(run!=null)yield return run;else Note("unknown case "+spec);Bind();if(TrailerMode.Active)TrailerMode.Instance?.End();}
  Note($"done, failures {fails}");File.WriteAllLines(output+"/done.txt",rows);UnityEditor.EditorApplication.Exit(0);}
 void Bind(){race=FindAnyObjectByType<RaceDirector>();flow=race?race.Flow:null;}
 IEnumerator Load(string scene){if(Scene!=scene){SceneManager.LoadScene(scene);yield return null;yield return null;}Bind();float t0=Time.realtimeSinceStartup;while(flow.Save==null&&Time.realtimeSinceStartup-t0<20)yield return null;yield return null;AudioListener.volume=0;}
 IEnumerator Menu(){Bind();if(flow.State!=RaceFlow.Stage.Ready){flow.Pause();flow.QuitRace();}yield return null;yield return null;Bind();}
 IEnumerator EnterRoam(string course,string vehicle){yield return Load(course);yield return Menu();
  flow.Save.Settings.vehicleId=vehicle;flow.Save.SaveSettings();if(Scene==RaceFlow.RoamScene){flow.StartFreeRoam();}else flow.StartFreeRoam();float t0=Time.realtimeSinceStartup;
  while((Scene!=RaceFlow.RoamScene||flow==null||flow.State!=RaceFlow.Stage.Racing)&&Time.realtimeSinceStartup-t0<60){yield return null;Bind();AudioListener.volume=0;}
  Time.timeScale=1;yield return new WaitForSeconds(.5f);}
 ArcadeVehicle Car=>race.vehicle;
 static string Full(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n;}
 void Put(Vector3 p,Quaternion r,Vector3 v){var car=Car;car.Body.isKinematic=false;car.transform.SetPositionAndRotation(p,r);car.Body.position=p;car.Body.rotation=r;car.Body.linearVelocity=v;car.Body.angularVelocity=Vector3.zero;car.ClearSteering();Physics.SyncTransforms();race.ResetSampling(p,race.Clock);car.GetComponent<VehicleRespawn>().CancelRecovery();FindAnyObjectByType<ChaseCamera>()?.Snap();}
 float Steer(Vector3 target){var car=Car;var d=Quaternion.Inverse(Quaternion.LookRotation(Vector3.ProjectOnPlane(car.transform.forward,Vector3.up)))*(target-car.Body.position);return Mathf.Clamp(Mathf.Atan2(d.x,d.z)*1.6f,-1,1);}
 static bool Ground(Vector3 p,out RaycastHit h)=>Physics.Raycast(p+Vector3.up*2,Vector3.down,out h,60,~0,QueryTriggerInteraction.Ignore);
 string Shot(string name,int w=1600,int h=900){var f=$"{output}/{name}.png";ThreeFeatureValidation.CaptureUi(f,w,h);return f;}

 // ---------- a simple road pilot (the nearest road; the test steers, the game's physics drive) ----------
 RaceRoad road;float station;int roadDir=1;
 void PickRoad(){var p=Car.Body.position;float best=1e9f;foreach(var r in FindObjectsByType<RaceRoad>(FindObjectsSortMode.None)){if(r.points==null||r.points.Length<2)continue;float s=r.Project(p,out _);var q=r.At(s,out var f);float d=Vector3.Distance(q,p);if(d<best){best=d;road=r;station=s;roadDir=Vector3.Dot(f,Car.transform.forward)>=0?1:-1;}}}
 // Rides for the given seconds at about pace m/s; each() is called every physics step.
 IEnumerator Drive(float seconds,float pace,Action each=null){var car=Car;if(!road)PickRoad();car.enabled=false;float t0=Time.time;
  while(Time.time-t0<seconds){yield return new WaitForFixedUpdate();var p=car.Body.position;station=road.Project(p,out _);var tgt=road.At(station+roadDir*12,out _);float sp=car.ForwardSpeed;
   car.Simulate(sp<pace?1:0,sp>pace+2?.5f:0,Steer(tgt),Time.fixedDeltaTime);each?.Invoke();}
  car.enabled=true;}
 // Placement checks for the camera: not inside anything solid, ground below it, a clear view of the vehicle.
 string Placement(out bool ok){var cam=Camera.main.transform.position;var car=Car;
  bool inside=Physics.OverlapSphere(cam,.08f,~0,QueryTriggerInteraction.Ignore).Any(c=>!(c.attachedRigidbody&&!c.attachedRigidbody.isKinematic));
  bool below=Physics.Raycast(cam+Vector3.up*.05f,Vector3.down,out var g,500,~0,QueryTriggerInteraction.Ignore);
  var to=car.transform.position+Vector3.up*.8f;var d=to-cam;bool clear=!Physics.SphereCastAll(cam,.15f,d.normalized,Mathf.Max(0,d.magnitude-1.2f),~0,QueryTriggerInteraction.Ignore).Any(h=>!(h.collider.attachedRigidbody&&!h.collider.attachedRigidbody.isKinematic));
  ok=!inside&&below;return $"camera {cam:F1}, {Vector3.Distance(cam,car.transform.position):F1} m from the vehicle, inside geometry {inside}, ground below {(below?g.distance.ToString("F1")+" m":"none")}, clear view {clear}, fov {Camera.main.fieldOfView:F0}, near {Camera.main.nearClipPlane:F2}";}
 Vector3 Eyes(){var e=Car.GetComponentsInChildren<Renderer>().Where(r=>r.name.EndsWith("__eyes")).ToArray();if(e.Length==0)return Vector3.positiveInfinity;var sum=Vector3.zero;foreach(var r in e)sum+=r.transform.TransformPoint(r.GetComponent<MeshFilter>().sharedMesh.bounds.center);return sum/e.Length;}
 int HeadShown()=>Car.GetComponentsInChildren<Renderer>().Count(r=>(r.name.Contains("_Hair")||r.name.Contains("_Hat_")||r.name.EndsWith("__eyes")||r.name.EndsWith("__pupil")||r.name.EndsWith("__mouth")||r.name.Contains("_Base_")&&r.name.EndsWith("__hair"))&&r.shadowCastingMode!=ShadowCastingMode.ShadowsOnly);
 int OwnVisibleInFront(){var planes=GeometryUtility.CalculateFrustumPlanes(Camera.main);var cam=Camera.main.transform;return Car.GetComponentsInChildren<Renderer>().Count(r=>r.enabled&&r.shadowCastingMode!=ShadowCastingMode.ShadowsOnly&&!(r is ParticleSystemRenderer)&&!(r is LineRenderer)&&GeometryUtility.TestPlanesAABB(planes,r.bounds)&&Vector3.Dot(r.bounds.center-cam.position,cam.forward)>0);}

 // ---------- Part C views ----------
 IEnumerator Views(string profile){yield return EnterRoam("StreetLoopGreybox",profile);var views=CameraViews.Current;var cam=Camera.main;
  Check(views&&views.PlayerView==CameraViews.View.Chase,$"default view {views?.PlayerView} ({profile})");
  foreach(var (tag,hour,weather) in new[]{("day",12f,Weather.Clear),("night-rain",23.5f,Weather.Rain)}){
   flow.Save.Settings.roamWeather=(int)weather;WorldLook.Current.SetClock(WorldLook.Current.Day,hour);yield return null;
   foreach(CameraViews.View v in Enum.GetValues(typeof(CameraViews.View))){views.SetPlayerView(v);PickRoad();yield return Drive(3.5f,13);yield return Late(()=>{
    var file=Shot($"view-{profile}-{v}-{tag}");string place=Placement(out bool ok);var p=Car.GetComponent<VehicleConfiguration>().Profile;
    string extra="";bool pass=ok&&views.ShownView==CameraViews.Names[(int)v];
    if(v==CameraViews.View.FirstPerson){var eye=Eyes();float d=float.IsInfinity(eye.x)?-1:Vector3.Distance(cam.transform.position,eye);int shown=HeadShown();extra=$"; {d:F3} m from the rider's eyes; head parts drawn {shown}; own parts in view {OwnVisibleInFront()}";pass&=d>=0&&d<.12f&&shown==0&&cam.nearClipPlane<.06f;}
    if(v==CameraViews.View.Front){var local=Car.transform.InverseTransformPoint(cam.transform.position);extra=$"; local {local:F2} (front of the body at z {p.Size.z*.5f:F2}); own parts in front of the camera {OwnVisibleInFront()}";pass&=local.z>p.Size.z*.5f-.2f;}
    if(v==CameraViews.View.FarChase){float d=Vector3.Distance(cam.transform.position,Car.transform.position);extra=$"; {d:F1} m back (chase offset {p.Camera.magnitude:F1} m)";pass&=d>p.Camera.magnitude*1.2f;}
    if(tag=="night-rain")extra+=$"; look {WorldLook.Current.Preset.name} rain {WorldLook.Current.Preset.rain:F2}, lamps {VehicleLights.Level:F2}";
    Check(pass,$"{profile} {v} ({tag}): {place}{extra} -> {Path.GetFileName(file)}");});}}
  views.SetPlayerView(CameraViews.View.Chase);flow.Save.Settings.roamWeather=0;}
 IEnumerator Persist(){yield return Load("StreetLoopGreybox");yield return Menu();var views=CameraViews.Current;views.SetPlayerView(CameraViews.View.FirstPerson);
  var fresh=new RacerSave(saveDir,"street-loop-gates-v1-laps3");Check(fresh.Settings.cameraView==2,$"settings.json read afresh: cameraView {fresh.Settings.cameraView} (First person)");
  SceneManager.LoadScene("LakeWoods");yield return null;yield return null;yield return Load("LakeWoods");Check(CameraViews.Current&&CameraViews.Current.PlayerView==CameraViews.View.FirstPerson,$"after loading another scene the view is {CameraViews.Current?.PlayerView}");
  CameraViews.Current.SetPlayerView(CameraViews.View.Chase);}

 // ---------- the Summit Homeward giant jump ----------
 // Full throttle from rest at the run-up (local s 5) to the lip, then coasting; returns take-off, flight and landing.
 struct Flight{public float airtime,distance,vTake;public string end;public Vector3 takeoff,land;}
 IEnumerator Homeward(Action<Flight> done,Action<bool,bool> each=null){var root=GameObject.Find("CR094 summit launch")?.transform;if(!root){Check(false,"CR094 summit launch not found in "+Scene);yield break;}
  var car=Car;car.enabled=false;var s0=root.TransformPoint(new Vector3(0,0,5));if(Ground(s0+Vector3.up*10,out var gh))s0=gh.point;Put(s0+Vector3.up*.7f,Quaternion.LookRotation(root.forward),Vector3.zero);
  for(float t=0;t<1.5f;t+=Time.fixedDeltaTime){yield return new WaitForFixedUpdate();car.Simulate(0,1,0,Time.fixedDeltaTime);}
  var rs=car.GetComponent<VehicleRespawn>();int resets=0;Action r=()=>resets++;rs.Respawned+=r;var f=new Flight{end="time"};bool air=false,landed=false;float t0=Time.time,landT=0;
  while(Time.time-t0<60){yield return new WaitForFixedUpdate();var p=car.Body.position;var q=root.InverseTransformPoint(p);
   if(!landed)car.Simulate(1,0,Steer(root.TransformPoint(new Vector3(0,0,q.z+25))),Time.fixedDeltaTime);else car.Simulate(0,.3f,0,Time.fixedDeltaTime);
   if(!air&&q.z>150&&car.GroundedWheels==0){air=true;f.takeoff=p;f.vTake=car.Body.linearVelocity.magnitude;}
   if(air&&!landed){f.airtime+=Time.fixedDeltaTime;if(car.GroundedWheels>=1&&f.airtime>.4f){landed=true;f.land=p;landT=Time.time;}}
   each?.Invoke(air&&!landed,landed);
   if(landed&&Time.time-landT>2.5f){f.end="landed";break;}if(resets>0){f.end="reset";break;}}
  rs.Respawned-=r;car.enabled=true;f.distance=Vector3.ProjectOnPlane(f.land-f.takeoff,Vector3.up).magnitude;done(f);}
 IEnumerator FpJump(string profile){yield return EnterRoam("StreetLoopGreybox",profile);var views=CameraViews.Current;var cam=Camera.main;views.SetPlayerView(CameraViews.View.FirstPerson);yield return new WaitForSeconds(1);
  // sampled every rendered frame: camera turn rate against the vehicle's, distance from the eyes (while not easing out)
  float maxRate=0,maxEye=0,maxBodyRate=0;Quaternion prev=cam.transform.rotation,prevBody=Car.transform.rotation;bool sampling=true;
  Action sample=()=>{float dt=Mathf.Max(Time.deltaTime,1e-4f);var e=Eyes();if(!float.IsInfinity(e.x)&&!views.Rough)maxEye=Mathf.Max(maxEye,Vector3.Distance(cam.transform.position,e));
    maxRate=Mathf.Max(maxRate,Quaternion.Angle(prev,cam.transform.rotation)/dt);maxBodyRate=Mathf.Max(maxBodyRate,Quaternion.Angle(prevBody,Car.transform.rotation)/dt);prev=cam.transform.rotation;prevBody=Car.transform.rotation;};
  lateEvery.Add(sample);int shots=0;Flight f=default;
  yield return Homeward(x=>f=x,(inAir,landed)=>{if(inAir&&shots==0){shots++;lateOnce=()=>Shot($"fp-jump-{profile}-takeoff");}else if(inAir&&shots==1&&Time.frameCount%30==0){shots++;lateOnce=()=>Shot($"fp-jump-{profile}-flight");}else if(landed&&shots==2){shots++;lateOnce=()=>Shot($"fp-jump-{profile}-landing");}});
  lateEvery.Remove(sample);
  Check(f.end=="landed"&&maxEye<.15f,$"first person through the Summit Homeward giant jump ({profile}): {f.end}, take-off {f.vTake:F1} m/s, airtime {f.airtime:F2} s, flight {f.distance:F1} m; camera stayed within {maxEye:F3} m of the eyes; camera turn rate max {maxRate:F0} deg/s (vehicle {maxBodyRate:F0} deg/s)");
  // wipeout: roll the vehicle over at speed; the camera eases out to the chase view, then back after a reset
  var car=Car;Ground(car.Body.position,out var g0);var fwd=Vector3.ProjectOnPlane(car.transform.forward,Vector3.up).normalized;
  Put(g0.point+Vector3.up*1.6f,Quaternion.LookRotation(fwd)*Quaternion.Euler(0,0,160),fwd*14);car.Body.angularVelocity=fwd*4;
  float t0=Time.time;maxRate=0;maxBodyRate=0;prev=cam.transform.rotation;prevBody=car.transform.rotation;float eyeFar=0;var chase=FindAnyObjectByType<ChaseCamera>();float chaseRate=0;Quaternion prevChase=chase.ChaseRotation;
  Action wipe=()=>{float dt=Mathf.Max(Time.deltaTime,1e-4f);maxRate=Mathf.Max(maxRate,Quaternion.Angle(prev,cam.transform.rotation)/dt);maxBodyRate=Mathf.Max(maxBodyRate,Quaternion.Angle(prevBody,car.transform.rotation)/dt);chaseRate=Mathf.Max(chaseRate,Quaternion.Angle(prevChase,chase.ChaseRotation)/dt);prev=cam.transform.rotation;prevBody=car.transform.rotation;prevChase=chase.ChaseRotation;
   var e=Eyes();if(!float.IsInfinity(e.x))eyeFar=Mathf.Max(eyeFar,Vector3.Distance(cam.transform.position,e));};
  lateEvery.Add(wipe);while(Time.time-t0<2.5f)yield return null;lateEvery.Remove(wipe);
  yield return Late(()=>Shot($"fp-wipeout-{profile}"));
  Check(eyeFar>1.5f&&maxRate<Mathf.Max(chaseRate*1.5f,120),$"wipeout in first person: camera eased out to the chase view ({eyeFar:F1} m from the eyes); camera turn rate max {maxRate:F0} deg/s (chase camera {chaseRate:F0}, tumbling vehicle {maxBodyRate:F0})");
  car.GetComponent<VehicleRespawn>().ResetVehicle();yield return new WaitForSeconds(2.5f);float db=-1;yield return Late(()=>{var back=Eyes();db=float.IsInfinity(back.x)?-1:Vector3.Distance(cam.transform.position,back);Shot($"fp-after-reset-{profile}");});Check(db>=0&&db<.15f,$"after the reset the view is first person again ({db:F3} m from the eyes, upright {car.transform.up.y:F2})");
  views.SetPlayerView(CameraViews.View.Chase);}

 // ---------- races ----------
 IEnumerator RaceCase(string scene,string profile,bool trailer){yield return Load(scene);yield return Menu();var s=flow.Save.Settings;s.weather=0;s.timeOfDay=0;flow.Save.SaveSettings();
  flow.OpenGarage();flow.SelectVehicle(profile);flow.CloseGarage();race.opponents=false;race.traffic=false;race.laps=1;string category=race.Category;
  var views=CameraViews.Current;if(!trailer)views.SetPlayerView(CameraViews.View.FirstPerson);
  double before=flow.Save.Best.race;
  if(trailer)TrailerMode.Instance.Begin();
  flow.StartRace();float t0=Time.realtimeSinceStartup;while(flow.State!=RaceFlow.Stage.Racing&&Time.realtimeSinceStartup-t0<30){AudioListener.volume=0;yield return null;}
  var car=Car;var pilot=car.gameObject.AddComponent<RoadDriver>();pilot.Initialize(race,car,true,1,1);pilot.Racer=race.Racers[0];car.GetComponent<VehicleInput>().enabled=false;
  float t1=Time.time;var hud=FindAnyObjectByType<RaceHud>().GetComponent<Canvas>();
  if(trailer){Check(flow.DebugMovementUsed&&flow.DebugMovementReason=="TRAILER MODE"&&!hud.enabled,$"race started in Trailer Mode: records disabled {flow.DebugMovementUsed} ({flow.DebugMovementReason}), HUD drawn {hud.enabled}");
   var guides=FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r=>r.enabled&&TrailerMode.Guide(r)).Cast<Renderer>().Concat(race.gates.SelectMany(g=>g.GetComponentsInChildren<Renderer>()).Where(r=>r.enabled)).ToArray();
   Check(guides.Length>0&&guides.All(r=>r.forceRenderingOff),$"race arrows and gates hidden in Trailer Mode: {guides.Count(r=>r.forceRenderingOff)} of {guides.Length} renderers");yield return Late(()=>Shot($"race-trailer-{scene}-guides-hidden"));
   TrailerMode.Instance.ToggleGuides();yield return null;Check(guides.All(r=>!r.forceRenderingOff),$"G: arrows and gates shown again ({guides.Count(r=>!r.forceRenderingOff)} of {guides.Length})");yield return Late(()=>Shot($"race-trailer-{scene}-guides-shown"));TrailerMode.Instance.ToggleGuides();
   for(int i=0;i<9;i++){if(i==7)continue;views.SelectTrailerCamera((CameraViews.Shot)i);if(i==4)views.CameraAction();yield return new WaitForSeconds(3);int k=i;yield return Late(()=>{string place=Placement(out bool ok);Check(ok&&views.OnScreen==(CameraViews.Shot)k,$"race, trailer camera {CameraViews.ShotNames[k]}: {place} -> {Path.GetFileName(Shot($"race-trailer-{scene}-{k}-{CameraViews.ShotNames[k].Replace(' ','-')}"))}");});}
   views.SelectTrailerCamera(CameraViews.Shot.Auto);int c0=views.AutoCuts;float a0=Time.time;while(flow.State!=RaceFlow.Stage.Results&&Time.time-a0<25){yield return null;}Note($"race, Auto: {views.AutoCuts-c0} cuts in {Time.time-a0:F0} s, last '{views.AutoReason}'");}
  else{yield return new WaitForSeconds(4);yield return Late(()=>Shot($"race-fp-{scene}-{profile}"));}
  while(flow.State!=RaceFlow.Stage.Results&&Time.time-t1<600){AudioListener.volume=0;yield return null;}
  Destroy(pilot);car.GetComponent<VehicleInput>().enabled=true;var p=race.Progress;double after=flow.Save.Best.race;
  if(trailer)Check(flow.State==RaceFlow.Stage.Results&&p.Finished&&after==before&&flow.FinishSummary.StartsWith("TRAILER MODE"),$"race in Trailer Mode {scene} {profile}: finished {p.Finished} in {RaceHud.FormatTime(p.LastLap)}; best race time before {before:F2}, after {after:F2} (unchanged = no record); results say '{flow.FinishSummary}'");
  else Check(flow.State==RaceFlow.Stage.Results&&p.Finished&&!flow.DebugMovementUsed&&after>0&&after!=before,$"race in first person {scene} {profile}: view {views.PlayerView}, finished in {RaceHud.FormatTime(p.LastLap)}, record saved {after:F2} s (records eligible {!flow.DebugMovementUsed}), missed gates {p.MissedGates}");
  if(trailer)TrailerMode.Instance.End();views.SetPlayerView(CameraViews.View.Chase);yield return Menu();}

 IEnumerator RaceViews(string scene,string profile){yield return Load(scene);yield return Menu();var s=flow.Save.Settings;s.weather=(int)Weather.Rain;s.timeOfDay=(int)TimeOfDay.Night;flow.Save.SaveSettings();
  flow.OpenGarage();flow.SelectVehicle(profile);flow.CloseGarage();race.opponents=false;race.traffic=false;race.laps=1;var views=CameraViews.Current;var cam=Camera.main;
  flow.StartRace();float t0=Time.realtimeSinceStartup;while(flow.State!=RaceFlow.Stage.Racing&&Time.realtimeSinceStartup-t0<30){AudioListener.volume=0;yield return null;}
  var car=Car;var pilot=car.gameObject.AddComponent<RoadDriver>();pilot.Initialize(race,car,true,1,1);pilot.Racer=race.Racers[0];car.GetComponent<VehicleInput>().enabled=false;yield return new WaitForSeconds(4);
  foreach(CameraViews.View v in Enum.GetValues(typeof(CameraViews.View))){views.SetPlayerView(v);yield return new WaitForSeconds(3.5f);yield return Late(()=>{string place=Placement(out bool ok);var p=car.GetComponent<VehicleConfiguration>().Profile;
   string extra="";bool pass=ok&&views.ShownView==CameraViews.Names[(int)v];
   if(v==CameraViews.View.FirstPerson){var eye=Eyes();float d=float.IsInfinity(eye.x)?-1:Vector3.Distance(cam.transform.position,eye);extra=$"; {d:F3} m from the eyes, head parts drawn {HeadShown()}";pass&=d>=0&&d<.12f&&HeadShown()==0;}
   if(v==CameraViews.View.Front){var local=car.transform.InverseTransformPoint(cam.transform.position);extra=$"; local {local:F2}";pass&=local.z>p.Size.z*.5f-.2f;}
   Check(pass,$"race {scene} night/rain {profile} {v}: {place}{extra}; look {WorldLook.Current.Preset.name} rain {WorldLook.Current.Preset.rain:F1}, lamps {VehicleLights.Level:F2}; records eligible {!flow.DebugMovementUsed} -> {Path.GetFileName(Shot($"race-view-{profile}-{v}-night-rain"))}");});}
  views.SetPlayerView(CameraViews.View.Chase);Destroy(pilot);car.GetComponent<VehicleInput>().enabled=true;s.weather=0;s.timeOfDay=0;flow.Save.SaveSettings();yield return Menu();}

 // ---------- Trailer Mode in Free Roam ----------
 IEnumerator TrailerCase(string profile){yield return EnterRoam("StreetLoopGreybox",profile);var mode=TrailerMode.Instance;var views=CameraViews.Current;var hud=FindAnyObjectByType<RaceHud>().GetComponent<Canvas>();
  typeof(TrailerMode).GetField("hintShownThisSession",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,false);
  DeveloperLocationHud.Instance.SetEnabled(true);yield return null;var debugCanvas=DeveloperLocationHud.Instance.GetComponent<Canvas>();
  Shot("roam-before-trailer");mode.Begin();yield return null;yield return null;
  Check(TrailerMode.Active&&!hud.enabled&&!debugCanvas.enabled&&mode.HintVisible,$"Trailer Mode on: HUD canvas drawn {hud.enabled}, debug panel drawn {debugCanvas.enabled}, first-time controls hint {mode.HintVisible}");
  yield return new WaitForSecondsRealtime(.2f);Shot("roam-trailer-hint");typeof(TrailerMode).GetField("hintUntil",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(mode,0f);yield return null;
  Check(!mode.HintVisible,"hint gone after its time; nothing else drawn");
  PickRoad();
  for(int i=0;i<9;i++){var s=(CameraViews.Shot)i;views.SelectTrailerCamera(s);if(s==CameraViews.Shot.Fixed)views.CameraAction();
   if(s==CameraViews.Shot.Free){mode.TestMove=new Vector3(.3f,.2f,1);yield return new WaitForSecondsRealtime(1.5f);mode.TestMove=Vector3.zero;}
   yield return Drive(s==CameraViews.Shot.Free?1:4,12);int k=i;yield return Late(()=>{string place=Placement(out bool ok);
   bool shown=s==CameraViews.Shot.Auto?views.OnScreen!=null:views.OnScreen==s;
   Check(ok&&shown,$"Free Roam, trailer camera {CameraViews.ShotNames[i]} (on screen {views.ShownView}): {place} -> {Path.GetFileName(Shot($"roam-trailer-{i}-{CameraViews.ShotNames[i].Replace(' ','-')}"))}");});}
  // free camera cannot go through the ground
  views.SelectTrailerCamera(CameraViews.Shot.Free);yield return new WaitForSecondsRealtime(.7f);mode.TestLook=new Vector2(0,-60);mode.TestMove=new Vector3(0,-1,0);mode.TestSpeed=45;
  yield return new WaitForSecondsRealtime(3);mode.TestMove=Vector3.zero;mode.TestSpeed=0;yield return Late(()=>{string fp=Placement(out bool fok);Check(fok,$"free camera flown straight down for 3 s at 45 m/s stops at the ground: {fp}");});
  var input=Car.GetComponent<VehicleInput>();Check(!input.enabled,"vehicle controls off while the free camera is used");
  views.SelectTrailerCamera(CameraViews.Shot.Chase);yield return null;yield return null;Check(input.enabled,"vehicle controls back after leaving the free camera");
  // HUD key, guides, screenshot key
  mode.ToggleHud();yield return null;Check(hud.enabled,"H: HUD shown again");Shot("roam-trailer-hud-shown");mode.ToggleHud();yield return null;Check(!hud.enabled,"H again: hidden");
  race.GetComponent<ExplorationMap>().SetWaypoint(Car.Body.position+Car.transform.forward*150);yield return new WaitForSecondsRealtime(1.2f);var guide=FindAnyObjectByType<WaypointGuide>();
  var beams=guide.GetComponentsInChildren<LineRenderer>(true);Check(beams.All(b=>b.forceRenderingOff)&&guide.Active,$"waypoint beacon hidden in Trailer Mode ({beams.Length} lines; waypoint still active {guide.Active})");
  mode.ToggleGuides();yield return null;Check(beams.All(b=>!b.forceRenderingOff),"G: beacon shown");Shot("roam-trailer-guides-shown");mode.ToggleGuides();race.GetComponent<ExplorationMap>().ClearWaypointFromGuide();
  var before=Directory.Exists(TrailerMode.ScreenshotFolder)?Directory.GetFiles(TrailerMode.ScreenshotFolder).Length:0;mode.Screenshot();yield return null;yield return new WaitForSecondsRealtime(3);
  var file=mode.LastScreenshot;bool exists=file!=null&&File.Exists(file);var tex=new Texture2D(2,2);if(exists)tex.LoadImage(File.ReadAllBytes(file));
  Check(exists&&tex.width==Screen.width&&tex.height==Screen.height,$"screenshot key: {file} {tex.width}x{tex.height} (screen {Screen.width}x{Screen.height})");if(exists)File.Copy(file,output+"/screenshot-key.png",true);
  // 4K captures of the whole screen (camera + every overlay canvas) with the HUD hidden, three conditions set with the controls
  void Set(float hour,Weather w){var c=WorldLook.Current.Trailer;c.hour=hour;c.weather=w;}
  mode.CycleTimeOfDay();yield return null;
  foreach(var (tag,hour,w) in new[]{("day",12f,Weather.Clear),("night-rain",23.5f,Weather.Rain),("dusk-snow",19.2f,Weather.Snow)}){Set(hour,w);views.SelectTrailerCamera(CameraViews.Shot.Orbit);yield return Drive(3,12);
   var f=Shot($"4k-hud-hidden-{tag}",3840,2160);Check(!hud.enabled&&!debugCanvas.enabled,$"4K capture {tag} ({WorldLook.Current.Preset.name}, rain {WorldLook.Current.Preset.rain:F1} snow {WorldLook.Current.Preset.snowfall:F1}): HUD drawn {hud.enabled}, debug drawn {debugCanvas.enabled} -> {Path.GetFileName(f)}");}
  mode.ToggleHud();yield return null;Shot("4k-hud-shown-dusk-snow",3840,2160);mode.ToggleHud();
  mode.End();yield return null;yield return null;Check(!TrailerMode.Active&&hud.enabled&&debugCanvas.enabled&&WorldLook.Current.Trailer==null&&input.enabled,"Trailer Mode off: HUD and debug panel back, conditions cleared, controls on");
  DeveloperLocationHud.Instance.SetEnabled(false);}

 IEnumerator AutoCase(string profile,float seconds){yield return EnterRoam("StreetLoopGreybox",profile);var mode=TrailerMode.Instance;var views=CameraViews.Current;mode.Begin();views.SelectTrailerCamera(CameraViews.Shot.Auto);PickRoad();
  var cuts=new List<(float t,CameraViews.Shot s,string why,bool air,bool rough)>();CameraViews.Shot? lastShot=null;int framesBlocked=0,frames=0;float t0=Time.time;
  yield return Drive(seconds,14,()=>{var s=views.OnScreen;if(s!=lastShot&&s!=null){cuts.Add((Time.time-t0,s.Value,views.AutoReason,Car.GroundedWheels==0,views.Rough));lastShot=s;}});
  for(int i=0;i<cuts.Count;i++){var c=cuts[i];float len=i+1<cuts.Count?cuts[i+1].t-c.t:Time.time-t0-c.t;Note($"  {c.t,6:F1} s  {CameraViews.ShotNames[(int)c.s],-14} {len,5:F1} s  ({c.why}){(c.air?" airborne":"")}{(c.rough?" rough":"")}");
  }
  bool repeats=cuts.Zip(cuts.Skip(1),(a,b)=>a.s==b.s).Any(x=>x);
  var lengths=new List<float>();for(int i=1;i+1<cuts.Count;i++)if(cuts[i+1].why.StartsWith("time"))lengths.Add(cuts[i+1].t-cuts[i].t);
  bool midAir=cuts.Skip(1).Any(c=>c.air&&!c.why.StartsWith("take-off"));
  Check(cuts.Count>=4&&!repeats&&lengths.All(l=>l>3.9f&&l<8.3f)&&!midAir,$"Auto over {seconds:F0} s: {cuts.Count} shots; same camera twice in a row {repeats}; timed shots held {(lengths.Count>0?lengths.Min():0):F1}-{(lengths.Count>0?lengths.Max():0):F1} s; cut mid-air (not a take-off) {midAir}; shots used {string.Join(", ",cuts.Select(c=>c.s).Distinct())}");
  Shot("auto-last");
  // hold and skip
  mode.TestHold=true;yield return null;yield return null;var held=views.OnScreen;int c0=views.AutoCuts;yield return Drive(10,14);Check(views.AutoCuts==c0&&views.OnScreen==held,$"hold: no cut in 10 s while held ({views.ShownView})");mode.TestHold=false;
  // a skip is consumed on the next rendered frame
  c0=views.AutoCuts;views.CameraAction();yield return null;yield return null;Check(views.AutoCuts==c0+1&&views.OnScreen!=held,$"skip: cut at once to {views.ShownView}");
  mode.End();}

 IEnumerator SlowJump(string profile){yield return EnterRoam("StreetLoopGreybox",profile);var mode=TrailerMode.Instance;var views=CameraViews.Current;
  Flight normal=default;yield return Homeward(x=>normal=x);
  mode.Begin();views.SelectTrailerCamera(CameraViews.Shot.Auto);yield return null;
  float minScale=1,minFixed=1;int shots=0;int cutsBefore=views.AutoCuts;string takeoffReason="";bool slowOn=false;Flight slow=default;int cutsInAir=0;CameraViews.Shot? prevShot=null;
  var root=GameObject.Find("CR094 summit launch").transform;
  yield return Homeward(x=>slow=x,(inAir,landed)=>{var q=root.InverseTransformPoint(Car.Body.position);
   if(!slowOn&&q.z>120&&!landed){slowOn=true;mode.HoldSlowForTest(true);}
   if(landed&&slowOn&&Time.timeScale<.3f&&shots>=3){mode.HoldSlowForTest(false);}
   minScale=Mathf.Min(minScale,Time.timeScale);minFixed=Mathf.Min(minFixed,Time.fixedDeltaTime);
   if(inAir&&views.AutoReason.StartsWith("take-off")&&takeoffReason=="")takeoffReason=views.AutoReason;
   if(inAir&&prevShot!=null&&views.OnScreen!=prevShot)cutsInAir++;prevShot=views.OnScreen;
   if(inAir&&shots<3&&Time.frameCount%25==0){shots++;Shot($"slowjump-{shots}");}});
  mode.HoldSlowForTest(false);yield return new WaitForSecondsRealtime(1);
  Check(slow.end=="landed"&&minScale<.27f&&Mathf.Abs(slow.distance-normal.distance)<Mathf.Max(6,normal.distance*.1f)&&takeoffReason!="",
   $"slow motion through the Summit Homeward jump ({profile}): {slow.end}; time scale down to {minScale:F2}, physics step {minFixed*1000:F1} ms; flight {slow.distance:F1} m, airtime {slow.airtime:F2} s (game time) vs {normal.distance:F1} m / {normal.airtime:F2} s at normal speed; Auto at take-off: '{takeoffReason}', cuts during the flight after it {Math.Max(0,cutsInAir-1)}; back to speed {Time.timeScale:F2}, step {Time.fixedDeltaTime*1000:F0} ms");
  mode.End();yield return null;Check(Time.timeScale==1&&Mathf.Approximately(Time.fixedDeltaTime,.02f),$"after Trailer Mode: time scale {Time.timeScale}, physics step {Time.fixedDeltaTime*1000:F0} ms");}

 IEnumerator Conditions(){yield return EnterRoam("StreetLoopGreybox","moto");var mode=TrailerMode.Instance;var look=WorldLook.Current;var path=Path.Combine(saveDir,"settings.json");
  // the realistic path: pause menu > Trailer Mode > On, resume, film, pause menu > Trailer Mode > Off
  flow.Pause();yield return null;yield return null;var before=File.ReadAllBytes(path);float hour=look.Hour;int day=look.Day;var weather=look.RoamWeather;var preset=look.Preset.name;
  mode.Begin();flow.Resume();yield return new WaitForSeconds(.5f);
  mode.CycleTimeOfDay();mode.NudgeClock(3);mode.CycleWeather();mode.CycleMoon();mode.CycleMoon();yield return new WaitForSeconds(1);
  var c=look.Trailer;Note($"conditions set: {mode.ConditionsLabel}; look {look.Preset.name}, rain {look.Preset.rain:F2} snow {look.Preset.snowfall:F2}");Shot("conditions-set");
  int strikes=look.GetComponent<WeatherEffects>().Strikes;mode.LightningNow();float t0=Time.time;while(look.GetComponent<WeatherEffects>().Strikes==strikes&&Time.time-t0<3)yield return null;
  Check(look.GetComponent<WeatherEffects>().Strikes>strikes&&look.Trailer.weather==Weather.Rain,$"lightning now: strike after {Time.time-t0:F2} s (weather {look.Trailer.weather})");yield return new WaitForSeconds(.1f);Shot("conditions-lightning");
  float h0=look.Trailer.hour;yield return new WaitForSeconds(3);float h1=look.Trailer.hour;mode.ToggleClock();yield return new WaitForSeconds(3);float h2=look.Trailer.hour;mode.ToggleClock();
  Check(h1>h0&&Mathf.Approximately(h1,h2),$"clock runs ({h0:F3} -> {h1:F3} in 3 s) and pauses ({h1:F3} -> {h2:F3})");
  string moon=mode.MoonLabel;mode.CycleMoon();Check(mode.MoonLabel!=moon,$"moon phase cycles: {moon} -> {mode.MoonLabel}");
  Check(Mathf.Approximately(look.Hour,hour)&&look.Day==day,$"saved Free Roam clock untouched while filming: {look.Day} {look.Hour:F3} (was {day} {hour:F3})");
  flow.Pause();yield return null;yield return null;var mid=File.ReadAllBytes(path);Check(mid.SequenceEqual(before),"pause menu during Trailer Mode: settings.json unchanged");
  mode.End();yield return null;yield return null;flow.Save.SaveSettings();var after=File.ReadAllBytes(path);
  Check(after.SequenceEqual(before)&&look.Trailer==null&&Mathf.Approximately(look.Hour,hour)&&look.Day==day&&look.RoamWeather==weather,$"Trailer Mode off: settings.json byte-identical ({after.Length} bytes) after saving; clock {look.Day} {look.Hour:F3}, weather {look.RoamWeather}, look {look.Preset.name} (was {preset})");
  flow.Resume();}
}
}
#endif
