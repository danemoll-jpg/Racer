#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Racer {
// 0.76 targeted checks (muted, isolated save). PROBE_CASES="case:args;..." starting in Street Loop Forward.
// PROBE_SAVE=copy -> the isolated save starts as a copy of the player's own save (progress carried over is compared).
//  storm:case:seconds          StormProbe in the editor (roam-day / roam-night / race-day).
//  roam                        Free Roam from each of the 8 courses: FreeRoamWorld loads, the player is at that course's
//                              start and on the ground, the course name / vehicle rule follow; Race Setup goes back to that
//                              course's scene; the Free Roam clock is saved on the way out and resumed.
//  map                         FreeRoamWorld map: several course routes shown at once (one race-only), scene and player
//                              unchanged, choice remembered after leaving and coming back; screenshot.
//  progress                    acorns, map landmarks and activity personal bests: course scene vs FreeRoamWorld.
//  world                       FreeRoamWorld weather, waypoint, fast travel.
//  ride:route:profile:dir:pace FreeRoamWorld ride along a road / branch (as 0.74).
//  race:scene:opponents        one lap raced by the AI pilot; finish, results, record.
public sealed class Report076Checks:MonoBehaviour {
 RaceDirector race;RaceFlow flow;readonly List<string> rows=new();string output,saveDir;int fails;
 void Note(string s){rows.Add(s);File.WriteAllLines(output+"/results.txt",rows);Debug.Log("REPORT076 "+s);}
 void Check(bool ok,string s){if(!ok)fails++;Note((ok?"PASS ":"FAIL ")+s);}
 static string Scene=>SceneManager.GetActiveScene().name;
 IEnumerator Start(){DontDestroyOnLoad(gameObject);AudioListener.volume=0;output=Environment.GetEnvironmentVariable("PROBE_OUT")+"/checks";Directory.CreateDirectory(output);Application.runInBackground=true;
  saveDir=Path.GetFullPath("Temp/Report076Save");if(Directory.Exists(saveDir))Directory.Delete(saveDir,true);Directory.CreateDirectory(saveDir);
  if(Environment.GetEnvironmentVariable("PROBE_SAVE")=="copy"){var src=Path.Combine(Application.persistentDataPath,"Phase7","street-loop-gates-v1-laps3");foreach(var f in Directory.GetFiles(src))File.Copy(f,Path.Combine(saveDir,Path.GetFileName(f)));Note("isolated save = copy of "+src);}
  if(Scene!="StreetLoopGreybox"){SceneManager.LoadScene("StreetLoopGreybox");yield return null;yield return null;}
  Bind();yield return null;flow.UseValidationSave(saveDir);
  var title=FindAnyObjectByType<StartupTitle>();if(title)Destroy(title.gameObject);typeof(StartupTitle).GetField("completed",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,true);flow.EnterMenuAfterTitle();
  flow.Save.Settings.master=0;flow.Save.SaveSettings();yield return null;
  foreach(var spec in (Environment.GetEnvironmentVariable("PROBE_CASES")??"").Split(';').Where(x=>x.Length>0)){var a=spec.Split(':');
   IEnumerator run=a[0] switch{"storm"=>StormProbe.Run(output,a[1],float.Parse(a[2])),"roam"=>RoamAll(),"map"=>MapCase(),"progress"=>Progress(),"world"=>WorldCase(),"ride"=>Ride(a[1],a[2],a[3]=="back",float.Parse(a[4])),"race"=>RaceCase(a[1],a[2]=="ai"),"garagedebug"=>GarageDebug(),_=>null};
   if(run!=null)yield return run;else Note("unknown case "+spec);Bind();}
  Note($"done, failures {fails}");File.WriteAllLines(output+"/done.txt",rows);UnityEditor.EditorApplication.Exit(0);}
 void Bind(){race=FindAnyObjectByType<RaceDirector>();flow=race?race.Flow:null;}
 IEnumerator Load(string scene){if(Scene!=scene){SceneManager.LoadScene(scene);yield return null;yield return null;}Bind();float t0=Time.realtimeSinceStartup;while(flow.Save==null&&Time.realtimeSinceStartup-t0<20)yield return null;yield return null;AudioListener.volume=0;}
 IEnumerator Menu(){Bind();if(flow.State!=RaceFlow.Stage.Ready){flow.Pause();flow.QuitRace();}yield return null;yield return null;Bind();}
 IEnumerator EnterRoam(string course,string vehicle="moto"){yield return Load(course);yield return Menu();if(Scene==RaceFlow.RoamScene)yield break;
  flow.Save.Settings.vehicleId=vehicle;flow.Save.SaveSettings();flow.StartFreeRoam();float t0=Time.realtimeSinceStartup;
  while((Scene!=RaceFlow.RoamScene||flow==null||flow.State!=RaceFlow.Stage.Racing)&&Time.realtimeSinceStartup-t0<60){yield return null;Bind();AudioListener.volume=0;}
  Time.timeScale=1;yield return new WaitForSeconds(.3f);}
 ArcadeVehicle Car=>race.vehicle;
 static string Full(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n;}

 // ---------- Free Roam from every course ----------
 IEnumerator RoamAll(){
  for(int i=0;i<RacePlaylists.Scenes.Length;i++){var course=RacePlaylists.Scenes[i];
   yield return Load(course);yield return Menu();
   int resets=0;yield return EnterRoam(course);var rs=Car.GetComponent<VehicleRespawn>();Action r=()=>resets++;rs.Respawned+=r;
   var c=CoursePreviewCatalog.Courses[i];var start=Car.Body.position;yield return new WaitForSeconds(3);var p=Car.Body.position;
   bool ground=Physics.Raycast(p+Vector3.up,Vector3.down,out var h,4,~0,QueryTriggerInteraction.Ignore);
   string shot=$"{output}/roam-start-{course}.png";ThreeFeatureValidation.CaptureUi(shot,1600,900);
   Check(Scene==RaceFlow.RoamScene&&race.FreeRoam&&RaceFlow.RoamCourse==i&&new Vector2(start.x-c.start.x,start.z-c.start.z).magnitude<3&&ground&&resets==0&&race.courseName==RacePlaylists.Titles[i]&&race.Forest==c.forest,
    $"Free Roam from {course}: scene {Scene}, start {start:F1} (course start {c.start:F1}), after 3 s {p:F1} on '{(ground?Full(h.collider.transform):"nothing")}' {(ground?h.distance-1:float.NaN):F2} m up, resets {resets}, title '{race.courseName}', small vehicles only {race.Forest}");
   rs.Respawned-=r;
   float hour=WorldLook.Current.Hour;int day=WorldLook.Current.Day;
   flow.Pause();yield return null;flow.OpenRaceSetupFromRoam();float t0=Time.realtimeSinceStartup;while(Scene!=course&&Time.realtimeSinceStartup-t0<60)yield return null;yield return null;yield return null;Bind();
   var page=(string)typeof(RaceMenus).GetField("page",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(FindAnyObjectByType<RaceMenus>());
   Check(Scene==course&&flow.State==RaceFlow.Stage.Ready&&page=="race"&&!race.FreeRoam&&Mathf.Abs(flow.Save.Settings.roamHour-hour)<.05f&&flow.Save.Settings.roamDay==day,$"  Race from Free Roam: back in {Scene}, menu page '{page}', Free Roam clock saved day {flow.Save.Settings.roamDay} {flow.Save.Settings.roamHour:F2} (was {day} {hour:F2})");}
  // the clock resumes where it was left
  float saved=flow.Save.Settings.roamHour;yield return EnterRoam(Scene);Check(Mathf.Abs(WorldLook.Current.Hour-saved)<.1f,$"Free Roam clock resumes at {WorldLook.Current.Hour:F2} (saved {saved:F2})");
  // Return to Menu and Start Race from FreeRoamWorld go back to the selected course
  flow.Pause();yield return null;var menus=FindAnyObjectByType<RaceMenus>();flow.StartRace();float t1=Time.realtimeSinceStartup;while((Scene==RaceFlow.RoamScene||flow==null||flow.State!=RaceFlow.Stage.Countdown&&flow.State!=RaceFlow.Stage.Racing)&&Time.realtimeSinceStartup-t1<60){yield return null;Bind();}
  Check(Scene==RacePlaylists.Scenes[RaceFlow.RoamCourse]&&!race.FreeRoam&&(flow.State==RaceFlow.Stage.Countdown||flow.State==RaceFlow.Stage.Racing),$"Start Race from FreeRoamWorld starts the race in {Scene} ({flow.State})");
  yield return Menu();}

 // ---------- map overlays ----------
 IEnumerator MapCase(){yield return EnterRoam("StreetLoopGreybox");var map=race.GetComponent<ExplorationMap>();map.Open();yield return null;
  var pos=Car.Body.position;var toggle=typeof(ExplorationMap).GetMethod("ToggleCourseRoute",BindingFlags.Instance|BindingFlags.NonPublic);
  var data=(ExplorationMap.Data)typeof(ExplorationMap).GetField("data",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(map);
  foreach(var s in new[]{"LakeWoods","DansBackyardReverse","MountainLoop"})toggle.Invoke(map,new object[]{Array.IndexOf(RacePlaylists.Scenes,s)});
  var zoomF=typeof(ExplorationMap).GetField("zoom",BindingFlags.Instance|BindingFlags.NonPublic);zoomF.SetValue(map,1f);typeof(ExplorationMap).GetField("center",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(map,new Vector2(.5f,.5f));typeof(ExplorationMap).GetMethod("Draw",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(map,null);
  yield return null;ThreeFeatureValidation.CaptureUi(output+"/map-four-routes.png",1920,1080);
  Check(Scene==RaceFlow.RoamScene&&Vector3.Distance(pos,Car.Body.position)<.01f&&data.routes.Count==4&&data.routesShown,$"map: routes shown {string.Join(", ",data.routes)} (one race-only, dashed); scene {Scene}; player moved {Vector3.Distance(pos,Car.Body.position):F3} m");
  map.Close();yield return Load("LakeWoods");yield return Menu();yield return EnterRoam("LakeWoods");map=race.GetComponent<ExplorationMap>();map.Open();yield return null;
  data=(ExplorationMap.Data)typeof(ExplorationMap).GetField("data",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(map);
  Check(data.routesChosen&&data.routes.Count==4&&((WorldMapCourseOverlay)typeof(ExplorationMap).GetField("courseOverlay",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(map)).gameObject.activeSelf,$"map choice remembered after leaving Free Roam and starting it from another course: {string.Join(", ",data.routes)}");
  ThreeFeatureValidation.CaptureUi(output+"/map-remembered.png",1920,1080);map.Close();yield return null;}

 // ---------- progress carried over ----------
 IEnumerator Progress(){yield return Load("StreetLoopGreybox");yield return Menu();
  var col=race.GetComponent<ExplorationCollection>();int acorns=col.Found;var map=race.GetComponent<ExplorationMap>();int landmarks=map.destinations.Count(d=>map.Discovered(d.id));int cells=map.RevealedCount;
  var before=flow.Activities.Sites.ToDictionary(s=>s.id,s=>flow.Activities.PersonalBest(s)?.value??0);
  yield return EnterRoam("StreetLoopGreybox");col=race.GetComponent<ExplorationCollection>();map=race.GetComponent<ExplorationMap>();
  Check(col.Found==acorns&&col.sites.Length==24&&map.RevealedCount==cells,$"acorns {col.Found}/{col.sites.Length} (course scene {acorns}), explored map cells {map.RevealedCount} (course scene {cells}), landmarks discovered {map.destinations.Count(d=>map.Discovered(d.id))}/{map.destinations.Length} (course scene {landmarks}/..)");
  foreach(var s in flow.Activities.Sites){var pb=flow.Activities.PersonalBest(s)?.value??0;before.TryGetValue(s.id,out var b);Check(pb>=b,$"activity {s.id} {s.title}: personal best {pb:F2} in FreeRoamWorld (Street Loop Forward {b:F2}; carried from {s.legacyRecords?.Length??0} course keys)");}}

 // ---------- weather, waypoint, fast travel ----------
 IEnumerator WorldCase(){yield return Load("DansBackyardReverse");yield return Menu();flow.Save.Settings.roamWeather=(int)Weather.Rain;flow.Save.SaveSettings();yield return EnterRoam("DansBackyardReverse");yield return new WaitForSeconds(1);
  Check(WorldLook.Current.Mode=="Free Roam"&&WorldLook.Current.Preset.rain>.9f,$"Free Roam weather Rain in FreeRoamWorld: look {WorldLook.Current.Preset.name} rain {WorldLook.Current.Preset.rain:F2}, clock {WorldLook.Current.RoamClock}");
  var map=race.GetComponent<ExplorationMap>();map.SetWaypoint(Car.Body.position+Car.transform.forward*120);yield return new WaitForSeconds(.5f);Check(race.GetComponent<WaypointGuide>()?.Active==true,$"waypoint set at {map.Waypoint:F0}, guide active");
  int target=Array.FindIndex(map.destinations,d=>d.id=="campsite");var data=(ExplorationMap.Data)typeof(ExplorationMap).GetField("data",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(map);if(!data.landmarks.Contains("campsite"))data.landmarks.Add("campsite");
  bool ok=map.Travel(target);yield return new WaitForSeconds(2);Check(ok&&Vector3.Distance(Car.Body.position,map.destinations[target].position)<15,$"fast travel to {map.destinations[target].title}: {ok}, now {Car.Body.position:F0}");
  flow.Save.Settings.roamWeather=0;flow.Save.SaveSettings();}

 // ---------- rides in FreeRoamWorld (as 0.74) ----------
 void Put(Vector3 p,Quaternion r,Vector3 v){var car=Car;car.Body.isKinematic=false;car.transform.SetPositionAndRotation(p,r);car.Body.position=p;car.Body.rotation=r;car.Body.linearVelocity=v;car.Body.angularVelocity=Vector3.zero;car.ClearSteering();Physics.SyncTransforms();race.ResetSampling(p,race.Clock);car.GetComponent<VehicleRespawn>().SeedCoursePosition(p);}
 float Steer(Vector3 target){var car=Car;var d=Quaternion.Inverse(Quaternion.LookRotation(Vector3.ProjectOnPlane(car.transform.forward,Vector3.up)))*(target-car.Body.position);return Mathf.Clamp(Mathf.Atan2(d.x,d.z)*1.6f,-1,1);}
 static List<Vector3> Route(string name){
  if(name.StartsWith("pts=")){return name.Substring(4).Split('/').Select(q=>{var a=q.Split(',').Select(float.Parse).ToArray();var v=new Vector3(a[0],300,a[1]);return Physics.Raycast(v,Vector3.down,out var h,600,1,QueryTriggerInteraction.Ignore)?h.point:new Vector3(a[0],0,a[1]);}).ToList();}
  foreach(var r in FindObjectsByType<RaceRoad>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(r.name==name||Full(r.transform).EndsWith(name))return r.points.ToList();
  foreach(var r in FindObjectsByType<WoodlandRoute>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(r.title==name||r.name==name||Full(r.transform).EndsWith(name))return r.points.ToList();return null;}
 static float Len(List<Vector3> p){float s=0;for(int i=0;i+1<p.Count;i++)s+=Vector3.Distance(p[i],p[i+1]);return s;}
 static Vector3 AtMetre(List<Vector3> pts,float m,out Vector3 f){float s=0;for(int i=0;i+1<pts.Count;i++){float l=Vector3.Distance(pts[i],pts[i+1]);if(s+l>=m){f=(pts[i+1]-pts[i]).normalized;return Vector3.Lerp(pts[i],pts[i+1],(m-s)/Mathf.Max(l,1e-4f));}s+=l;}f=(pts[^1]-pts[^2]).normalized;return pts[^1];}
 IEnumerator Ride(string routeName,string profile,bool back,float pace){yield return EnterRoam(VehicleProfile.Find(profile).Small?"DansBackyardReverse":"StreetLoopGreybox",profile);Check(race.vehicle.GetComponent<VehicleConfiguration>().profileId==profile,$"ride vehicle {race.vehicle.GetComponent<VehicleConfiguration>().profileId}");race.traffic=false;AudioListener.volume=0;
  var pts=Route(routeName);if(pts==null){Check(false,"route not found "+routeName);yield break;}if(back)pts.Reverse();float L=Len(pts);
  var start=AtMetre(pts,1,out var f0);if(Physics.Raycast(start+Vector3.up*30,Vector3.down,out var g,80,1,QueryTriggerInteraction.Ignore))start=g.point;f0.y=0;
  var car=Car;car.enabled=false;Put(start+Vector3.up*.7f,Quaternion.LookRotation(f0.normalized),f0.normalized*pace*.5f);
  int resets=0;var rs=car.GetComponent<VehicleRespawn>();Action onReset=()=>resets++;rs.Respawned+=onReset;var cfg=car.GetComponent<VehicleConfiguration>();
  bool stuckNoted=false;float t0=Time.time,minSpeed=float.MaxValue,air=0,airMax=0,maxSpeed=0,slow=0;int wipes=0;bool wasWiped=false;float m=1;string end="time";
  while(Time.time-t0<Mathf.Max(40,L/pace*2.5f)){yield return new WaitForFixedUpdate();var p=car.Body.position;
   float best=1e9f,bm=m;for(float q=m;q<Mathf.Min(L,m+25);q+=.5f){var c=AtMetre(pts,q,out _);float d=new Vector2(c.x-p.x,c.z-p.z).sqrMagnitude;if(d<best){best=d;bm=q;}}m=bm;
   var tgt=AtMetre(pts,Mathf.Min(m+8,L),out _);float sp=car.ForwardSpeed;
   car.Simulate(sp<pace?1:0,sp>pace+1.5f?.6f:0,Steer(tgt),Time.fixedDeltaTime);
   float elapsed=Time.time-t0;if(elapsed>2.5f&&m<L-8){minSpeed=Mathf.Min(minSpeed,sp);if(sp<pace*.5f&&car.GroundedWheels>=2){slow+=Time.fixedDeltaTime;if(slow>3&&!stuckNoted){stuckNoted=true;Note($"  held up at {p:F1} (route metre {m:F0}), under: {(Physics.Raycast(p+Vector3.up,Vector3.down,out var uh,4)?Full(uh.collider.transform):"-")}; ahead: {(Physics.Raycast(p+Vector3.up*.5f,car.transform.forward,out var fh,4)?Full(fh.collider.transform):"-")}");}}}maxSpeed=Mathf.Max(maxSpeed,sp);
   if(car.GroundedWheels<2){air+=Time.fixedDeltaTime;airMax=Mathf.Max(airMax,air);}else air=0;
   bool wiped=cfg&&cfg.WipedOut;if(wiped&&!wasWiped)wipes++;wasWiped=wiped;
   if(m>=L-3){end="complete";break;}if(resets>0){end="reset";break;}}
  rs.Respawned-=onReset;car.enabled=true;
  Check(end=="complete"&&wipes==0&&slow<1.5f,$"RIDE {routeName} {(back?"back":"fwd")} {profile} pace {pace}: {end} in {Time.time-t0:F1} s over {L:F0} m; slowest {(minSpeed==float.MaxValue?0:minSpeed):F1} m/s, fastest {maxSpeed:F1} m/s; time held under half pace on the ground {slow:F1} s; longest airborne {airMax:F2} s; wipe-outs {wipes}; resets {resets}");}

 IEnumerator GarageDebug(){yield return Menu();flow.OpenGarage();yield return null;yield return null;var menus=FindAnyObjectByType<RaceMenus>();
  foreach(var id in new[]{"moto","tourer"}){flow.SelectVehicle(id);yield return null;var root=(GameObject)typeof(RaceMenus).GetField("previewRoot",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(menus);
   Note($"{id}: radius {typeof(RaceMenus).GetField("previewRadius",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(menus)} centre {typeof(RaceMenus).GetField("previewCenter",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(menus)} root {root.transform.position} {root.transform.eulerAngles}");
   foreach(var r in root.GetComponentsInChildren<Renderer>().OrderByDescending(r=>r.bounds.size.magnitude).Take(6))Note($"  {Full(r.transform)} {r.GetType().Name} enabled {r.enabled} bounds {r.bounds.center:F1} size {r.bounds.size:F1} rot {r.transform.eulerAngles:F0}");}
  var cam=(Camera)typeof(RaceMenus).GetField("previewCamera",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(menus);ThreeFeatureValidation.CaptureUi(output+"/garage-editor.png",1920,1080);Note($"camera {cam.transform.position:F1} {cam.transform.eulerAngles:F1} fov {cam.fieldOfView}");flow.CloseGarage();}
 // ---------- one race ----------
 IEnumerator RaceCase(string scene,bool ai){yield return Load(scene);yield return Menu();var s=flow.Save.Settings;s.weather=0;s.timeOfDay=0;s.vehicleId="moto";s.estimateAiFinishes=true;flow.Save.SaveSettings();
  if(race.opponents!=ai)flow.ToggleOpponents();race.laps=1;race.traffic=true;string category=race.Category;
  flow.StartRace();float t0=Time.realtimeSinceStartup;while(flow.State!=RaceFlow.Stage.Racing&&Time.realtimeSinceStartup-t0<30){AudioListener.volume=0;yield return null;}
  var car=Car;var pilot=car.gameObject.AddComponent<RoadDriver>();pilot.Initialize(race,car,true,1,1);pilot.Racer=race.Racers[0];car.GetComponent<VehicleInput>().enabled=false;
  int resets=0;var rs=car.GetComponent<VehicleRespawn>();Action r=()=>resets++;rs.Respawned+=r;
  float t1=Time.time;while(flow.State!=RaceFlow.Stage.Results&&Time.time-t1<600){AudioListener.volume=0;yield return null;}
  rs.Respawned-=r;Destroy(pilot);car.GetComponent<VehicleInput>().enabled=true;
  var p=race.Progress;var file=Directory.GetFiles(saveDir,"records-"+race.courseId+"*").Length;
  Check(flow.State==RaceFlow.Stage.Results&&p.Finished&&p.MissedGates==0,$"RACE {scene} ({race.courseName}, {(ai?"3 AI":"solo")}, 1 lap): {(p.Finished?"finished "+RaceHud.FormatTime(p.LastLap):"not finished")}, missed gates {p.MissedGates}, recoveries {resets}, AI {string.Join(", ",race.Racers.Skip(1).Select(x=>x.Name+(x.Classified?" finished":x.Dnf?" DNF":" racing")))}; record files for this course {file}");
  yield return Menu();}
}
}
#endif
