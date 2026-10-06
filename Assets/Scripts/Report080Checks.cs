#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Racer {
// 0.80 targeted checks (muted, isolated save). PROBE_CASES="case:args;..." starting in Street Loop Forward.
//  near:Scene:x,y,z,r|x,y,z,r   everything (renderers, colliders) within r metres of each point, plus a few views.
[DefaultExecutionOrder(1000)]
public sealed partial class Report080Checks:MonoBehaviour {
 Action lateOnce;void LateUpdate(){var a=lateOnce;lateOnce=null;a?.Invoke();}
 IEnumerator Late(Action a){bool done=false;lateOnce=()=>{a();done=true;};while(!done)yield return null;}
 RaceDirector race;RaceFlow flow;readonly List<string> rows=new();string output,saveDir;int fails;
 void Note(string s){rows.Add(s);File.WriteAllLines(output+"/results.txt",rows);Debug.Log("REPORT080 "+s);}
 void Check(bool ok,string s){if(!ok)fails++;Note((ok?"PASS ":"FAIL ")+s);}
 static string Scene=>SceneManager.GetActiveScene().name;
 IEnumerator Start(){DontDestroyOnLoad(gameObject);AudioListener.volume=0;output=Environment.GetEnvironmentVariable("PROBE_OUT")+"/checks";Directory.CreateDirectory(output);Application.runInBackground=true;
  saveDir=Path.GetFullPath("Temp/Report080Save");if(Directory.Exists(saveDir))Directory.Delete(saveDir,true);Directory.CreateDirectory(saveDir);
  if(Scene!="StreetLoopGreybox"){SceneManager.LoadScene("StreetLoopGreybox");yield return null;yield return null;}
  Bind();yield return null;{float t0=Time.realtimeSinceStartup;while((!flow.Started||LoadingScreen.Holding)&&Time.realtimeSinceStartup-t0<60){yield return null;Bind();}}flow.UseValidationSave(saveDir);
  var title=FindAnyObjectByType<StartupTitle>();if(title)Destroy(title.gameObject);typeof(StartupTitle).GetField("completed",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,true);flow.EnterMenuAfterTitle();
  flow.Save.Settings.master=0;flow.Save.SaveSettings();yield return null;
  foreach(var spec in (Environment.GetEnvironmentVariable("PROBE_CASES")??"").Split(';').Where(x=>x.Length>0)){var a=spec.Split(':');Note("---- "+spec);
   IEnumerator run=Case(a);
   if(run!=null)yield return run;else Note("unknown case "+spec);Bind();}
  Note($"done, failures {fails}");File.WriteAllLines(output+"/done.txt",rows);UnityEditor.EditorApplication.Exit(0);}
 IEnumerator Case(string[] a)=>a[0] switch{"near"=>Near(a[1],a[2]),"grid"=>Grid(a[1],a[2]),"top"=>Top(a[1],a[2]),"view"=>View(a[1],a[2],a.Length>3?a[3]:"day"),
  "report"=>LoadReport(a[1]),"jump"=>Jump(a[1]),"rides"=>Rides(a[1],a[2]),"resets"=>Resets(a[1],a[2]),"buried"=>Buried(a[1]),"force"=>Force(a[1],a[2],a[3]),"fist"=>Fist(a[1]),"escape"=>Escape(a[1],a[2],a[3]),"carrace"=>CarRace(a[1],a[2],a[3],a.Length>4?F(a[4]):1),"ride"=>RideCase(a[1],a[2],a[3],F(a[4]),F(a[5]),F(a[6]),a.Length>7?F(a[7]):0),"drive"=>Drive(a[1],a[2],a[3],float.Parse(a[4],System.Globalization.CultureInfo.InvariantCulture),float.Parse(a[5],System.Globalization.CultureInfo.InvariantCulture),a.Length>6?a[6]:""),_=>MoreCases(a)};
 // Branch ride with a simple follower (no race AI): ride:Scene:profile:branch:fromS:toS:speed m/s
 // The vehicle starts on the branch centre line at fromS and is steered along it (look-ahead 10 m) holding the speed;
 // logs each physics step and reports air time, the lowest up vector, resets and whether it reached toS.
 public struct RideResult{public bool reached,stuck;public float maxAir,minUp,maxJolt,endS,seconds;public Vector3 joltAt,end;public int resets;}
 public RideResult LastRide;
 IEnumerator RideCase(string scene,string profile,string branch,float fromS,float toS,float speed,float offset){
  yield return Load(scene);yield return Menu();flow.OpenGarage();flow.SelectVehicle(profile);flow.CloseGarage();race.opponents=false;race.traffic=false;race.laps=1;flow.StartRace();
  float tr=Time.realtimeSinceStartup;while(flow.State!=RaceFlow.Stage.Racing&&Time.realtimeSinceStartup-tr<40){AudioListener.volume=0;yield return null;}
  yield return new WaitForSeconds(3.5f);
  var wr=FindObjectsByType<WoodlandRoute>(FindObjectsSortMode.None).FirstOrDefault(b=>b.title==branch);if(!wr){Note("no branch "+branch);yield break;}
  yield return Ride(wr,fromS,toS,speed,$"{Scene}-{profile}-{branch.Replace(' ','_').Replace('/','_')}-{fromS:F0}-{speed:F0}-{offset:F0}",offset);
  var r=LastRide;Note($"{Scene} {profile} ride {branch} {fromS}->{toS} at {speed} m/s offset {offset} m: {(r.reached?"REACHED":"NOT reached")} end s {r.endS:F1} in {r.seconds:F1}s, resets {r.resets}, stuck {r.stuck}, max air {r.maxAir:F2}s, min up {r.minUp:F2}, max jolt {r.maxJolt:F0} m/s² at {V(r.joltAt)}");
  yield return Menu();}
 IEnumerator Ride(WoodlandRoute wr,float fromS,float toS,float speed,string tag,float offset=0){
  var car=race.vehicle;car.GetComponent<VehicleInput>().enabled=false;car.enabled=false;
  var start=wr.At(fromS,out var f0);f0.y=0;start+=Vector3.Cross(Vector3.up,f0).normalized*offset;
  car.Body.position=start+Vector3.up*Mathf.Max(.4f,car.suspensionLength-Physics.gravity.magnitude/car.springStrength);car.Body.rotation=Quaternion.LookRotation(f0.normalized);car.transform.SetPositionAndRotation(car.Body.position,car.Body.rotation);
  car.Body.linearVelocity=Vector3.zero;car.Body.angularVelocity=Vector3.zero;car.ClearSteering();car.GetComponent<VehicleRespawn>().SeedCoursePosition(start);
  for(int i=0;i<20;i++){car.Simulate(0,0,0,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();}
  car.Body.linearVelocity=car.transform.forward*speed;
  int resets=0;System.Action onR=()=>resets++;var resp=car.GetComponent<VehicleRespawn>();resp.Respawned+=onR;
  var log=new StringBuilder("t,x,y,z,speed,wheels,upY,vy,s,lat\n");float t0=Time.time,air=0,maxAir=0,minUp=1,maxJolt=0,best=fromS,bestAt=Time.time;Vector3 joltAt=default;var prevV=car.Body.linearVelocity;bool reached=false,stuck=false;
  float limit=(toS-fromS)/Mathf.Max(4,speed*.5f)+15;
  float rt0=Time.realtimeSinceStartup;
  while(Time.time-t0<limit&&Time.realtimeSinceStartup-rt0<limit*4+60&&flow.State==RaceFlow.Stage.Racing){AudioListener.volume=0;
   var p=car.Body.position;float s=wr.Project(p,out float lat);
   var target=wr.At(s+10,out var tf);tf.y=0;target+=Vector3.Cross(Vector3.up,tf).normalized*offset;var to=target-p;to.y=0;float ang=Vector3.SignedAngle(Vector3.ProjectOnPlane(car.transform.forward,Vector3.up),to,Vector3.up);
   float steer=Mathf.Clamp(ang/25f,-1,1);float v=car.ForwardSpeed;float thr=v<speed?1:0,brk=v>speed+3?.5f:0;
   car.Simulate(thr,brk,steer,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();
   var vel=car.Body.linearVelocity;float jolt=Mathf.Abs(vel.y-prevV.y)/Time.fixedDeltaTime;if(jolt>maxJolt&&car.GroundedWheels>0){maxJolt=jolt;joltAt=car.Body.position;}prevV=vel;
   minUp=Mathf.Min(minUp,car.transform.up.y);if(car.GroundedWheels==0){air+=Time.fixedDeltaTime;maxAir=Mathf.Max(maxAir,air);}else air=0;
   log.AppendLine($"{Time.time-t0:F2},{p.x:F2},{p.y:F2},{p.z:F2},{v:F2},{car.GroundedWheels},{car.transform.up.y:F3},{vel.y:F2},{s:F1},{lat:F2}");
   if(s>best+.5f){best=s;bestAt=Time.time;}
   if(s>=toS-1){reached=true;break;}
   if(Time.time-bestAt>6){stuck=true;break;}
   if(car.transform.up.y<.2f&&car.GroundedWheels==0&&air>3){break;}}
  resp.Respawned-=onR;
  File.WriteAllText($"{output}/ride-{tag}.csv",log.ToString());
  LastRide=new RideResult{reached=reached,stuck=stuck,maxAir=maxAir,minUp=minUp,maxJolt=maxJolt,joltAt=joltAt,endS=best,seconds=Time.time-t0,resets=resets,end=car.Body.position};
  car.enabled=true;car.GetComponent<VehicleInput>().enabled=true;}
 // What the 0.80 load-time systems did in a scene: posts on pavement, junction paint, bikes-only signs.
 IEnumerator LoadReport(string scene){yield return EnterScene(scene);yield return new WaitForSeconds(1);
  var sb=new StringBuilder($"== {Scene}\nposts removed {RoadPosts.Removed.Count}, kept {RoadPosts.Kept.Count}\n");foreach(var l in RoadPosts.Removed)sb.AppendLine("  removed "+l);foreach(var l in RoadPosts.Kept)sb.AppendLine("  kept "+l);
  var w=SceneryWorld.Current;if(w&&w.Paint)foreach(var l in w.Paint.Report)sb.AppendLine("  paint: "+l);
  foreach(var l in MountainDirt.Report)sb.AppendLine("  dirt: "+l);
  File.AppendAllText($"{output}/load-report.txt",sb.ToString());Note($"{Scene}: posts removed {RoadPosts.Removed.Count}, kept {RoadPosts.Kept.Count}");
  if(scene!=RaceFlow.RoamScene)yield return Menu();}
 // Part C: one race, the player's car on an autopilot, three AI: carrace:Scene:player:ai1,ai2,ai3[:timeScale]
 // Per racer: vehicle, finished / time / missed gates / resets / shortcuts taken / longest stall (no 10 m progress).
 public static readonly List<string> RaceTable=new();
 IEnumerator CarRace(string scene,string player,string roster,float scale){
  yield return Load(scene);yield return Menu();var st=flow.Save.Settings;st.weather=0;st.timeOfDay=0;st.estimateAiFinishes=false;flow.Save.SaveSettings();
  flow.OpenGarage();flow.SelectVehicle(player);flow.CloseGarage();race.opponents=true;race.traffic=false;race.laps=1;race.difficulty=1;race.opponentRoster=roster.Split(',');
  flow.StartRace();float t0=Time.realtimeSinceStartup;while(flow.State!=RaceFlow.Stage.Racing&&Time.realtimeSinceStartup-t0<40){AudioListener.volume=0;yield return null;}
  var car=race.vehicle;string got=car.GetComponent<VehicleConfiguration>().profileId;
  var pilot=car.gameObject.AddComponent<RoadDriver>();pilot.Initialize(race,car,true,1,1);pilot.Racer=race.Racers[0];car.GetComponent<VehicleInput>().enabled=false;
  var branches=race.Racers.ToDictionary(r=>r,r=>new HashSet<string>());var lastPos=race.Racers.ToDictionary(r=>r,r=>r.Car.Body.position);var lastMove=race.Racers.ToDictionary(r=>r,r=>Time.time);var worst=race.Racers.ToDictionary(r=>r,r=>0f);var worstAt=race.Racers.ToDictionary(r=>r,r=>Vector3.zero);
  Time.timeScale=scale;float t1=Time.time;
  float rt1=Time.realtimeSinceStartup;
  while(flow.State!=RaceFlow.Stage.Results&&Time.time-t1<900&&Time.realtimeSinceStartup-rt1<1800){AudioListener.volume=0;yield return null;
   foreach(var r in race.Racers){if(r.Branch.Route)branches[r].Add(r.Branch.Route.title);var p=r.Car.Body.position;
    if(r.Progress.Finished){lastMove[r]=Time.time;continue;}
    if((p-lastPos[r]).magnitude>10){lastPos[r]=p;lastMove[r]=Time.time;}else{float stall=Time.time-lastMove[r];if(stall>worst[r]){worst[r]=stall;worstAt[r]=p;}}}}
  Time.timeScale=1;Destroy(pilot);car.GetComponent<VehicleInput>().enabled=true;
  string Prof(RacerState r)=>r.Car.GetComponent<VehicleConfiguration>().profileId;
  int Resets(RacerState r)=>r.Recoveries;
  string Row(RacerState r)=>$"{(r.IsAi?r.Name:"PLAYER")} {Prof(r)}: {(r.Progress.Finished?RaceHud.FormatTime(r.Progress.AdjustedTime(race.Clock)):r.Dnf?"DNF":"not finished")} missed {r.Progress.MissedGates} resets {Resets(r)} longest stall {worst[r]:F0}s{(worst[r]>8?" at "+V(worstAt[r]):"")} shortcuts [{string.Join(", ",branches[r])}]";
  bool cars=race.Racers.Count(r=>!VehicleProfile.Find(Prof(r)).Small)>=3;
  bool ok=flow.State==RaceFlow.Stage.Results&&race.Racers.All(r=>r.Progress.Finished&&r.Progress.MissedGates==0)&&got==player;
  var line=$"{scene} player {got} vs {string.Join("/",race.Racers.Skip(1).Select(Prof))}: {string.Join("; ",race.Racers.Select(Row))}";
  Check(ok,line);RaceTable.Add($"{scene}\t{player}\t{(ok?"pass":"FAIL")}\t{line}");File.WriteAllLines(output+"/race-table.tsv",RaceTable);
  yield return Menu();}
 // Part C: every shortcut in a scene with one vehicle at the shortcut's own recommended speed (capped 32 m/s):
 // rides:Scene:profile. Same follower as ride; the motorcycle is the control.
 public static readonly List<string> RideTable=new();
 IEnumerator Rides(string scene,string profile){
  yield return Load(scene);yield return Menu();flow.OpenGarage();flow.SelectVehicle(profile);flow.CloseGarage();race.opponents=false;race.traffic=false;race.laps=1;flow.StartRace();
  float tr=Time.realtimeSinceStartup;while(flow.State!=RaceFlow.Stage.Racing&&Time.realtimeSinceStartup-tr<40){AudioListener.volume=0;yield return null;}
  yield return new WaitForSeconds(3.5f);
  foreach(var wr in FindObjectsByType<WoodlandRoute>(FindObjectsSortMode.None).Where(b=>b.isActiveAndEnabled&&b.points!=null&&b.points.Length>1).OrderBy(b=>b.title)){
   float sp=Mathf.Min(32,Mathf.Max(10,wr.SpeedAt(0)));float len=wr.Length;
   yield return Ride(wr,0,len-3,sp,$"{Scene}-{profile}-{wr.title.Replace(' ','_').Replace('/','_')}");var r=LastRide;
   var line=$"{Scene}\t{wr.title}\t{profile}\t{(r.reached?"through":"FAILED")}\tlen {len:F0} m, half-width {wr.halfWidth:F2}, speed {sp:F0} m/s, end s {r.endS:F0}, {r.seconds:F1}s, stuck {r.stuck}, max air {r.maxAir:F2}s, min up {r.minUp:F2}, end {V(r.end)}";
   RideTable.Add(line);File.WriteAllLines(output+"/ride-table.tsv",RideTable);Note(line);}
  yield return Menu();}
 // Part C.6: the reset (0.68 rule) with a car off every shortcut and off the main road: resets:Scene:profile.
 // The car is put down 2.5 m beyond a trail's edge (beside the middle of each shortcut, and at four main-road
 // stations), R is pressed; it must come back upright on that course's route within 3 s.
 public static readonly List<string> ResetTable=new();
 IEnumerator Resets(string scene,string profile){
  yield return Load(scene);yield return Menu();flow.OpenGarage();flow.SelectVehicle(profile);flow.CloseGarage();race.opponents=false;race.traffic=false;race.laps=1;flow.StartRace();
  float tr=Time.realtimeSinceStartup;while(flow.State!=RaceFlow.Stage.Racing&&Time.realtimeSinceStartup-tr<40){AudioListener.volume=0;yield return null;}
  yield return new WaitForSeconds(3.5f);var car=race.vehicle;var resp=car.GetComponent<VehicleRespawn>();race.road.Initialize();
  var spots=new List<(string what,Vector3 at,Vector3 fw,float hw)>();
  foreach(var wr in FindObjectsByType<WoodlandRoute>(FindObjectsSortMode.None).Where(b=>b.isActiveAndEnabled&&b.points!=null&&b.points.Length>1)){var p=wr.At(wr.Length*.5f,out var f);spots.Add((wr.title+" (middle)",p,f,wr.halfWidth));}
  foreach(float k in new[]{.15f,.4f,.65f,.9f}){float s=race.road.Length*k;var p=race.road.At(s,out var f);spots.Add(($"main road s {s:F0}",p,f,race.road.HalfWidth(s)));}
  foreach(var (what,at,fw,hw) in spots){
   var r=Vector3.Cross(Vector3.up,new Vector3(fw.x,0,fw.z).normalized);var off=at+r*(hw+2.5f);
   if(Physics.Raycast(off+Vector3.up*30,Vector3.down,out var gh,80,~0,QueryTriggerInteraction.Ignore)&&!gh.collider.attachedRigidbody)off=gh.point;
   Put(car,off+Vector3.up*1f,Quaternion.LookRotation(new Vector3(fw.x,0,fw.z)),Vector3.zero);
   for(int i=0;i<10;i++){car.Simulate(0,0,0,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();}
   int n=0;System.Action h=()=>n++;resp.Respawned+=h;resp.ResetVehicle();float t0=Time.time;while(Time.time-t0<3){yield return new WaitForFixedUpdate();car.Simulate(0,0,0,Time.fixedDeltaTime);}resp.Respawned-=h;
   var p=car.Body.position;
   float nearest=race.road.Project(p,out float lat0)>=0?lat0-race.road.HalfWidth(race.road.Project(p,out _)):999;
   foreach(var wr in FindObjectsByType<WoodlandRoute>(FindObjectsSortMode.None)){wr.Project(p,out float l);nearest=Mathf.Min(nearest,l-wr.halfWidth);}
   bool ok=n>0&&car.transform.up.y>.9f&&nearest<.5f;
   var line=$"{Scene}\t{profile}\t{what}\t{(ok?"pass":"FAIL")}\trespawned {n}, at {V(p)}, up {car.transform.up.y:F2}, {Mathf.Max(0,nearest):F1} m outside the nearest route edge";
   ResetTable.Add(line);File.WriteAllLines(output+"/reset-table.tsv",ResetTable);Check(ok,line);}
  yield return Menu();}
 // Part C: the race autopilot (the AI's own driver) forced into one shortcut: force:Scene:profile:title
 // Placed on the main road 90 m before the entrance (earlier gates credited), told to take the shortcut, run until it is
 // 40 m past the shortcut's exit or 120 s: did it enter, how far along the shortcut it got, resets, airtime, stalls.
 public static readonly List<string> ForceTable=new();
 IEnumerator Force(string scene,string profile,string title){
  yield return Load(scene);yield return Menu();flow.OpenGarage();flow.SelectVehicle(profile);flow.CloseGarage();race.opponents=false;race.traffic=false;race.laps=1;flow.StartRace();
  float tr=Time.realtimeSinceStartup;while(flow.State!=RaceFlow.Stage.Racing&&Time.realtimeSinceStartup-tr<40){AudioListener.volume=0;yield return null;}
  yield return new WaitForSeconds(3.5f);
  var wr=FindObjectsByType<WoodlandRoute>(FindObjectsSortMode.None).FirstOrDefault(b=>b.title==title&&b.isActiveAndEnabled);if(!wr){Note("no shortcut "+title);yield return Menu();yield break;}
  var car=race.vehicle;car.GetComponent<VehicleInput>().enabled=false;car.enabled=false;
  var pilot=car.gameObject.AddComponent<RoadDriver>();pilot.Initialize(race,car,true,1,1);pilot.Racer=race.Racers[0];pilot.enabled=false;
  float from=Mathf.Repeat(wr.entryRoad-90,race.road.Length);pilot.Place(from,0);
  race.Progress.Restart();race.Progress.Cross(0,true,race.Clock);for(int i=1;i<race.gates.Length;i++)if(race.road.Relative(race.road.Project(race.gates[i].transform.position,out _),race.Origin)<race.road.Relative(from,race.Origin))race.Progress.Cross(i,true,race.Clock);
  for(int i=0;i<30;i++){car.Simulate(0,0,0,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();}
  race.ResetSampling(car.Body.position,race.Clock);car.Body.linearVelocity=car.transform.forward*15;
  var field=typeof(RoadDriver).GetField("plannedBranch",BindingFlags.Instance|BindingFlags.NonPublic);
  int resets=race.Racers[0].Recoveries;pilot.enabled=true;bool entered=false;float best=0,air=0,maxAir=0,minUp=1,t0=Time.time,lastGain=Time.time;
  float rt0=Time.realtimeSinceStartup;
  while(Time.time-t0<120&&Time.realtimeSinceStartup-rt0<300&&flow.State==RaceFlow.Stage.Racing){AudioListener.volume=0;
   var rs=race.Racers[0];float s=race.road.Project(car.Body.position,out _);
   if(!entered&&rs.Branch.Route==null&&race.road.Relative(s,from)<race.road.Relative(wr.entryRoad,from)+5)field.SetValue(pilot,wr);
   if(rs.Branch.Route==wr)entered=true;
   if(entered){float bs=wr.Project(car.Body.position,out float lat);if(lat<wr.halfWidth+4&&bs>best+.5f){best=bs;lastGain=Time.time;}}
   yield return new WaitForFixedUpdate();
   minUp=Mathf.Min(minUp,car.transform.up.y);if(car.GroundedWheels==0){air+=Time.fixedDeltaTime;maxAir=Mathf.Max(maxAir,air);}else air=0;
   if(entered&&best>=wr.Length-8&&race.road.Relative(race.road.Project(car.Body.position,out _),race.road.Project(wr.points[^1],out _))>40&&race.road.Relative(race.road.Project(car.Body.position,out _),race.road.Project(wr.points[^1],out _))<200)break;
   if(entered&&Time.time-lastGain>25)break;}
  resets=race.Racers[0].Recoveries-resets;bool through=entered&&best>=wr.Length-8;
  var line=$"{scene}\t{title}\t{profile}\t{(through?"through":entered?"FAILED":"not entered")}\tentered {entered}, reached s {best:F0} of {wr.Length:F0}, resets {resets}, max air {maxAir:F2}s, min up {minUp:F2}, {Time.time-t0:F0}s";
  ForceTable.Add(line);File.WriteAllLines(output+"/force-table.tsv",ForceTable);Note(line);
  Destroy(pilot);car.enabled=true;car.GetComponent<VehicleInput>().enabled=true;yield return Menu();}
 // Part B: the fist wave on LB. An emulated controller (the batch editor has no focused Game view, so only gamepads
 // reach the game; F is checked as a binding): LB while driving waves, RB does nothing; in Trailer Mode LB is the
 // 0.25x hold and RB the 0.5x toggle, and neither waves. fist:profile
 IEnumerator PressPad(GamepadButton b,int frames=2){var d=InputSystem.AddDevice<Gamepad>();InputSystem.QueueStateEvent(d,new GamepadState{buttons=1u<<(int)b});for(int i=0;i<frames;i++)yield return null;InputSystem.QueueStateEvent(d,new GamepadState());yield return null;InputSystem.RemoveDevice(d);}
 IEnumerator Fist(string profile){yield return EnterRoam("StreetLoopGreybox",profile);yield return new WaitForSeconds(1);
  var car=race.vehicle;var g=car.GetComponent<RiderGestures>();var fist=car.GetComponent<VehicleInput>().CurrentBindings[4];
  var paths=string.Join(", ",Enumerable.Range(0,fist.bindings.Count).Select(i=>fist.bindings[i].effectivePath));
  Check(paths.Contains("<Gamepad>/leftShoulder")&&paths.Contains("<Keyboard>/f")&&!paths.Contains("rightShoulder"),$"fist wave bindings: {paths}");
  int w0=g.Waves;yield return PressPad(GamepadButton.LeftShoulder);yield return new WaitForSeconds(.2f);int afterLB=g.Waves;
  yield return new WaitForSeconds(RiderGestures.WaveSeconds+RiderGestures.PlayerCooldown+.4f);
  int w1=g.Waves;yield return PressPad(GamepadButton.RightShoulder);yield return new WaitForSeconds(.3f);int afterRB=g.Waves;
  Check(afterLB==w0+1&&afterRB==w1,$"{profile} driving: LB started a wave ({w0} -> {afterLB}); RB did nothing ({w1} -> {afterRB})");
  // the Settings > Controls row reads the same action
  Note($"Settings > Controls row 'Driving / {fist.name}': {MenuInput.Binding(fist)}");
  yield return new WaitForSeconds(RiderGestures.WaveSeconds+RiderGestures.PlayerCooldown+.4f);
  TrailerMode.Instance.Begin();yield return new WaitForSeconds(.5f);bool on=TrailerMode.Active;bool half0=TrailerMode.Instance.HalfSpeed;int w2=g.Waves;
  var d=InputSystem.AddDevice<Gamepad>();InputSystem.QueueStateEvent(d,new GamepadState{buttons=1u<<(int)GamepadButton.LeftShoulder});yield return new WaitForSecondsRealtime(.5f);float heldScale=Time.timeScale;InputSystem.QueueStateEvent(d,new GamepadState());yield return new WaitForSecondsRealtime(.4f);InputSystem.RemoveDevice(d);
  yield return PressPad(GamepadButton.RightShoulder);yield return new WaitForSecondsRealtime(.3f);bool half1=TrailerMode.Instance.HalfSpeed;int w3=g.Waves;
  Check(on&&w3==w2&&Mathf.Abs(heldScale-.25f)<.06f&&half1!=half0,$"Trailer Mode: LB held -> time scale {heldScale:F2} (0.25 expected), RB toggled 0.5x {half0} -> {half1}, waves {w2} -> {w3} (none)");
  if(half1)yield return PressPad(GamepadButton.RightShoulder);TrailerMode.Instance.End();yield return new WaitForSecondsRealtime(.5f);}
 // BUG-008: drop a vehicle at points on a ring around a spot (and on the spot), let it settle, then ride toward the
 // nearest main-road point for up to 12 s: escape:Scene:profile:x,z,radius. Pass = back within 4 m of the road.
 IEnumerator Escape(string scene,string profile,string spot){
  yield return Load(scene);yield return Menu();flow.OpenGarage();flow.SelectVehicle(profile);flow.CloseGarage();race.opponents=false;race.traffic=false;race.laps=1;flow.StartRace();
  float tr=Time.realtimeSinceStartup;while(flow.State!=RaceFlow.Stage.Racing&&Time.realtimeSinceStartup-tr<40){AudioListener.volume=0;yield return null;}
  yield return new WaitForSeconds(3.5f);var car=race.vehicle;car.GetComponent<VehicleInput>().enabled=false;car.enabled=false;var resp=car.GetComponent<VehicleRespawn>();resp.enabled=false;
  var f=spot.Split(',').Select(F).ToArray();var c=new Vector3(f[0],0,f[1]);float rad=f[2];race.road.Initialize();
  var pts=new List<Vector3>{c};for(int k=0;k<12;k++)pts.Add(c+Quaternion.Euler(0,k*30,0)*Vector3.forward*rad);for(int k=0;k<8;k++)pts.Add(c+Quaternion.Euler(0,k*45+22.5f,0)*Vector3.forward*rad*.5f);
  int ok=0,n=0;var sb=new StringBuilder();
  foreach(var q in pts){if(!Physics.Raycast(new Vector3(q.x,400,q.z),Vector3.down,out var gh,600,~0,QueryTriggerInteraction.Ignore))continue;
   // skip points on the road itself
   float s0=race.road.Project(gh.point,out float lat0);if(lat0<race.road.HalfWidth(s0))continue;n++;
   Put(car,gh.point+Vector3.up*1.5f,Quaternion.LookRotation(Vector3.ProjectOnPlane(race.road.At(s0,out _)-gh.point,Vector3.up).normalized),Vector3.zero);
   for(float t=0;t<2;t+=Time.fixedDeltaTime){car.Simulate(0,0,0,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();}
   var settled=car.Body.position;bool back=false;float t1=0;
   for(;t1<12&&flow.State==RaceFlow.Stage.Racing;t1+=Time.fixedDeltaTime){float s=race.road.Project(car.Body.position,out float lat);if(lat<race.road.HalfWidth(s)+4&&Mathf.Abs(race.road.At(s,out _).y-car.Body.position.y)<4){back=true;break;}
    var target=race.road.At(s,out _);car.Simulate(.7f,0,Steer(car,target),Time.fixedDeltaTime);yield return new WaitForFixedUpdate();}
   if(back)ok++;sb.AppendLine($"{(back?"out":"TRAPPED")} from {V(gh.point)} (settled {V(settled)}) in {t1:F1}s");}
  resp.enabled=true;car.enabled=true;car.GetComponent<VehicleInput>().enabled=true;
  File.WriteAllText($"{output}/escape-{Scene}-{profile}.txt",sb.ToString());
  Check(ok==n,$"{Scene} {profile}: dropped at {n} points around ({c.x}, {c.z}) within {rad} m (off the road), {ok} rode back onto the course within 12 s");
  yield return Menu();}
 // Section 4 for the 0.80 ground: nothing (tree, trunk collider, small prop) has its base more than 0.3 m under one of
 // the new 0.80 surfaces, and nothing stands floating over it: buried:Scene
 IEnumerator Buried(string scene){yield return EnterScene(scene);yield return new WaitForSeconds(1);
  var mine=FindObjectsByType<MeshCollider>(FindObjectsSortMode.None).Where(c=>c.name.StartsWith("Ground_Report080")).ToArray();
  if(mine.Length==0){Note($"{Scene}: no 0.80 ground here");yield break;}
  var bases=new List<(string what,Vector3 p)>();var world=SceneryWorld.Current;
  if(world&&world.Trees)foreach(var t in world.Trees.Placements)bases.Add(("tree "+t.source,t.bottom));
  foreach(var b in FindObjectsByType<BoxCollider>(FindObjectsSortMode.None))if(b.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0)bases.Add(("trunk collider "+P(b.transform),new Vector3(b.bounds.center.x,b.bounds.min.y,b.bounds.center.z)));
  foreach(var r in FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None)){var bb=r.bounds;if(bb.size.x<3&&bb.size.z<3&&bb.size.y>.2f&&bb.size.y<8&&!r.GetComponentInParent<ArcadeVehicle>()&&!r.name.StartsWith("Ground"))bases.Add(("object "+P(r.transform),new Vector3(bb.center.x,bb.min.y,bb.center.z)));}
  int buried=0,floating=0;var sb=new StringBuilder();
  foreach(var (what,p) in bases){foreach(var c in mine){var b=c.bounds;if(p.x<b.min.x-1||p.x>b.max.x+1||p.z<b.min.z-1||p.z>b.max.z+1)continue;
    if(c.Raycast(new Ray(new Vector3(p.x,b.max.y+5,p.z),Vector3.down),out var h,b.size.y+20)){float d=h.point.y-p.y;if(d>.3f){buried++;sb.AppendLine($"BURIED {d:F2} m: {what} at {V(p)} under {c.name}");}else if(d<-1f&&d>-30){/* standing above our ground: is anything else under it? */if(!Physics.Raycast(p+Vector3.up*.1f,Vector3.down,out var g2,Mathf.Abs(d)-.05f,~0,QueryTriggerInteraction.Ignore)){floating++;sb.AppendLine($"floating {-d:F2} m over it: {what} at {V(p)}");}}}}}
  File.WriteAllText($"{output}/buried-{Scene}.txt",sb.ToString());
  Check(buried==0,$"{Scene}: {bases.Count} trees, trunks and objects checked against {mine.Length} new 0.80 surfaces: {buried} buried more than 0.3 m, {floating} standing above them in the air");
  if(scene!=RaceFlow.RoamScene)yield return Menu();}
 // BUG-006: the Summit giant jump in Free Roam (same run as 0.78's check): jump:profile
 void Put(ArcadeVehicle car,Vector3 p,Quaternion r,Vector3 v){car.Body.isKinematic=false;car.transform.SetPositionAndRotation(p,r);car.Body.position=p;car.Body.rotation=r;car.Body.linearVelocity=v;car.Body.angularVelocity=Vector3.zero;car.ClearSteering();Physics.SyncTransforms();race.ResetSampling(p,race.Clock);car.GetComponent<VehicleRespawn>().CancelRecovery();FindAnyObjectByType<ChaseCamera>()?.Snap();}
 float Steer(ArcadeVehicle car,Vector3 target){var d=Quaternion.Inverse(Quaternion.LookRotation(Vector3.ProjectOnPlane(car.transform.forward,Vector3.up)))*(target-car.Body.position);return Mathf.Clamp(Mathf.Atan2(d.x,d.z)*1.6f,-1,1);}
 IEnumerator Jump(string profile){yield return EnterRoam("StreetLoopGreybox",profile);var root=GameObject.Find("CR094 summit launch")?.transform;if(!root){Check(false,"no summit launch");yield break;}
  var car=race.vehicle;car.enabled=false;var s0=root.TransformPoint(new Vector3(0,0,5));if(Physics.Raycast(s0+Vector3.up*10,Vector3.down,out var gh,60,~0,QueryTriggerInteraction.Ignore))s0=gh.point;Put(car,s0+Vector3.up*.7f,Quaternion.LookRotation(root.forward),Vector3.zero);
  for(float t=0;t<1.5f;t+=Time.fixedDeltaTime){yield return new WaitForFixedUpdate();car.Simulate(0,1,0,Time.fixedDeltaTime);}
  bool air=false,landed=false;float airtime=0,minUp=1,after=0;Vector3 take=default,land=default;float t0=Time.time;
  while(Time.time-t0<50&&(!landed||(after<12&&car.Body.linearVelocity.magnitude>1))){yield return new WaitForFixedUpdate();var q=root.InverseTransformPoint(car.Body.position);car.Simulate(landed?0:1,0,landed?0:Steer(car,root.TransformPoint(new Vector3(0,0,q.z+25))),Time.fixedDeltaTime);
   if(!air&&q.z>150&&car.GroundedWheels==0){air=true;take=car.Body.position;}if(air&&!landed){airtime+=Time.fixedDeltaTime;if(car.GroundedWheels>=1&&airtime>.4f){landed=true;land=car.Body.position;}}
   if(landed){after+=Time.fixedDeltaTime;minUp=Mathf.Min(minUp,car.transform.up.y);if(after>.6f&&after<.65f)yield return Late(()=>Shot($"jump-landing-{profile}"));}}
  car.enabled=true;float d=Vector3.ProjectOnPlane(land-take,Vector3.up).magnitude;float roll=Vector3.ProjectOnPlane(car.Body.position-land,Vector3.up).magnitude;
  Check(landed&&d>200&&minUp>.6f,$"Summit giant jump ({profile}, full throttle from rest, coasting after the landing as in 0.71): {d:F1} m in {airtime:F2} s, landed at {V(land)}, rolled out {roll:F0} m, lowest up after landing {minUp:F2}");}
 // Eye views: view:Scene:name,x,y,z,yaw,pitch|...[:day|night]
 IEnumerator View(string scene,string specs,string light){yield return EnterScene(scene);yield return new WaitForSeconds(1);
  WorldLook.Current?.Pin(LookPresets.Compose(light=="night"?TimeOfDay.Night:TimeOfDay.Day,Weather.Clear));
  var cam=Camera.main;var chase=FindAnyObjectByType<ChaseCamera>();if(chase)chase.enabled=false;var cv=CameraViews.Current;if(cv)cv.enabled=false;
  var hud=FindObjectsByType<Canvas>(FindObjectsSortMode.None);foreach(var c in hud)c.enabled=false;
  foreach(var spec in specs.Split('|')){var f=spec.Split(',');var inv=System.Globalization.CultureInfo.InvariantCulture;string name=f[0];var p=new Vector3(float.Parse(f[1],inv),float.Parse(f[2],inv),float.Parse(f[3],inv));float yaw=float.Parse(f[4],inv),pitch=float.Parse(f[5],inv);
   yield return Late(()=>{cam.transform.SetPositionAndRotation(p,Quaternion.Euler(pitch,yaw,0));Shot($"view-{Scene}-{name}-{light}");});}
  foreach(var c in hud)if(c)c.enabled=true;if(chase)chase.enabled=true;if(cv)cv.enabled=true;
  Note($"{Scene}: views written");
  if(scene!=RaceFlow.RoamScene)yield return Menu();}
 // Race-speed trace: drive:Scene:profile:branch(or Main):fromStation:seconds[:speed m/s] - the player on an autopilot from a
 // main-road station (gates before it credited), forced onto the branch when one is named; every physics step logged.
 IEnumerator Drive(string scene,string profile,string branch,float from,float seconds,string speedArg){
  yield return Load(scene);yield return Menu();flow.OpenGarage();flow.SelectVehicle(profile);flow.CloseGarage();race.opponents=false;race.traffic=false;race.laps=1;flow.StartRace();
  float tr=Time.realtimeSinceStartup;while(flow.State!=RaceFlow.Stage.Racing&&Time.realtimeSinceStartup-tr<40){AudioListener.volume=0;yield return null;}
  var car=race.vehicle;car.GetComponent<VehicleInput>().enabled=false;car.enabled=false;
  var pilot=car.gameObject.AddComponent<RoadDriver>();pilot.Initialize(race,car,true,1,1);pilot.Racer=race.Racers[0];pilot.enabled=false;pilot.Place(from,0);
  race.Progress.Restart();race.Progress.Cross(0,true,race.Clock);for(int i=1;i<race.gates.Length;i++)if(race.road.Relative(race.road.Project(race.gates[i].transform.position,out _),race.Origin)<race.road.Relative(from,race.Origin))race.Progress.Cross(i,true,race.Clock);
  for(int i=0;i<30;i++){car.Simulate(0,0,0,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();}
  race.ResetSampling(car.Body.position,race.Clock);float v0=speedArg.Length>0?float.Parse(speedArg,System.Globalization.CultureInfo.InvariantCulture):20;car.Body.linearVelocity=car.transform.forward*v0;
  var wr=branch=="Main"?null:FindObjectsByType<WoodlandRoute>(FindObjectsSortMode.None).FirstOrDefault(b=>b.title==branch);
  if(wr)typeof(RoadDriver).GetField("plannedBranch",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(pilot,wr);
  int resets=0;car.GetComponent<VehicleRespawn>().Respawned+=()=>resets++;pilot.enabled=true;
  var log=new StringBuilder("t,x,y,z,speed,wheels,upY,vy,angvel,station,branchS\n");float t0=Time.time,air=0,maxAir=0,minUp=1;Vector3 prevV=car.Body.linearVelocity;float maxJolt=0;Vector3 joltAt=default;
  while(Time.time-t0<seconds){AudioListener.volume=0;yield return new WaitForFixedUpdate();var p=car.Body.position;var v=car.Body.linearVelocity;
   float jolt=Mathf.Abs(v.y-prevV.y)/Time.fixedDeltaTime;if(jolt>maxJolt&&car.GroundedWheels>0){maxJolt=jolt;joltAt=p;}prevV=v;
   minUp=Mathf.Min(minUp,car.transform.up.y);if(car.GroundedWheels==0){air+=Time.fixedDeltaTime;maxAir=Mathf.Max(maxAir,air);}else air=0;
   float st=race.road.Project(p,out _);float bs=wr?wr.Project(p,out _):0;
   log.AppendLine($"{Time.time-t0:F2},{p.x:F2},{p.y:F2},{p.z:F2},{car.ForwardSpeed:F2},{car.GroundedWheels},{car.transform.up.y:F3},{v.y:F2},{car.Body.angularVelocity.magnitude:F2},{st:F1},{bs:F1}");}
  File.WriteAllText($"{output}/drive-{Scene}-{profile}-{branch.Replace(' ','_')}-{from:F0}-{v0:F0}.csv",log.ToString());
  Note($"{Scene} {profile} {branch} from {from} at {v0} m/s: resets {resets}+{pilot.RecoveryCount}, max air {maxAir:F2}s, min up {minUp:F2}, max vertical jolt {maxJolt:F0} m/s² at {V(joltAt)}, end {V(car.Body.position)}");
  Destroy(pilot);yield return Menu();}
 // Top-down orthographic views (daylight) and an inventory of thin flat paint pieces: top:Scene:name,x,z,size|...
 static void Day(){if(WorldLook.Current)WorldLook.Current.Pin(LookPresets.Compose(TimeOfDay.Day,Weather.Clear));}
 static bool PaintLike(Renderer r){if(r is not MeshRenderer||!r.GetComponent<MeshFilter>())return false;var b=r.bounds;return b.size.y<.08f&&Mathf.Min(b.size.x,b.size.z)<1.5f&&Mathf.Max(b.size.x,b.size.z)<400;}
 IEnumerator Top(string scene,string specs){yield return EnterScene(scene);yield return new WaitForSeconds(1);
  Day();
  var cam=Camera.main;var chase=FindAnyObjectByType<ChaseCamera>();if(chase)chase.enabled=false;var cv=CameraViews.Current;if(cv)cv.enabled=false;
  foreach(var spec in specs.Split('|')){var f=spec.Split(',');var inv=System.Globalization.CultureInfo.InvariantCulture;string name=f[0];float x=float.Parse(f[1],inv),z=float.Parse(f[2],inv),size=float.Parse(f[3],inv);
   var sb=new StringBuilder("path,active,cx,cy,cz,sx,sz,yaw,mat,verts\n");
   foreach(var r in FindObjectsByType<Renderer>(FindObjectsInactive.Include,FindObjectsSortMode.None)){var b=r.bounds;if(Mathf.Abs(b.center.x-x)>size*.5f+b.extents.x||Mathf.Abs(b.center.z-z)>size*.5f+b.extents.z)continue;
    bool paint=PaintLike(r)||r.name.Contains("lane")||r.name.Contains("yellow")||r.name.Contains("Lane")||r.name.Contains("median")||r.name.Contains("stop")||r.name.Contains("dash");if(!paint)continue;
    var mf=r.GetComponent<MeshFilter>();sb.AppendLine($"\"{P(r.transform)}\",{r.enabled&&r.gameObject.activeInHierarchy},{b.center.x:F2},{b.center.y:F2},{b.center.z:F2},{b.size.x:F2},{b.size.z:F2},{r.transform.eulerAngles.y:F1},\"{(r.sharedMaterial?r.sharedMaterial.name:"")}\",{(mf&&mf.sharedMesh?mf.sharedMesh.vertexCount:0)}");}
   File.WriteAllText($"{output}/paint-{Scene}-{name}.csv",sb.ToString());
   float y=0;if(Physics.Raycast(new Vector3(x,600,z),Vector3.down,out var gh,900,~0,QueryTriggerInteraction.Ignore))y=gh.point.y;
   bool ortho=cam.orthographic;float os=cam.orthographicSize,far=cam.farClipPlane;
   yield return Late(()=>{cam.orthographic=true;cam.orthographicSize=size*.5f*900f/1600f;cam.transform.SetPositionAndRotation(new Vector3(x,y+150,z),Quaternion.Euler(90,0,0));cam.farClipPlane=400;Shot($"top-{Scene}-{name}",1600,900);});
   yield return Late(()=>{cam.orthographic=ortho;cam.orthographicSize=os;cam.farClipPlane=far;});}
  if(chase)chase.enabled=true;if(cv)cv.enabled=true;
  Note($"{Scene}: top views written");
  if(scene!=RaceFlow.RoamScene)yield return Menu();}
 partial void More(string[] a,ref IEnumerator run);
 IEnumerator MoreCases(string[] a){IEnumerator run=null;More(a,ref run);return run;}
 void Bind(){race=FindAnyObjectByType<RaceDirector>();flow=race?race.Flow:null;}
 IEnumerator Load(string scene){if(Scene!=scene){SceneManager.LoadScene(scene);yield return null;yield return null;}Bind();float t0=Time.realtimeSinceStartup;while((flow.Save==null||!flow.Started||LoadingScreen.Holding)&&Time.realtimeSinceStartup-t0<60){yield return null;Bind();}yield return null;AudioListener.volume=0;}
 IEnumerator Menu(){Bind();if(flow.State!=RaceFlow.Stage.Ready){flow.Pause();flow.QuitRace();}yield return null;yield return null;Bind();}
 IEnumerator EnterRoam(string course,string vehicle){yield return Load(course);yield return Menu();
  flow.Save.Settings.vehicleId=vehicle;flow.Save.SaveSettings();flow.StartFreeRoam();float t0=Time.realtimeSinceStartup;
  while((Scene!=RaceFlow.RoamScene||flow==null||flow.State!=RaceFlow.Stage.Racing||LoadingScreen.Holding)&&Time.realtimeSinceStartup-t0<60){yield return null;Bind();AudioListener.volume=0;}
  Time.timeScale=1;yield return new WaitForSeconds(.5f);}
 IEnumerator EnterScene(string scene){
  if(scene==RaceFlow.RoamScene)yield return EnterRoam("StreetLoopGreybox","moto");
  else{yield return Load(scene);yield return Menu();race.opponents=false;race.traffic=false;race.laps=1;flow.StartRace();float tr=Time.realtimeSinceStartup;while(flow.State!=RaceFlow.Stage.Racing&&Time.realtimeSinceStartup-tr<30){AudioListener.volume=0;yield return null;}yield return new WaitForSecondsRealtime(1);}}
 string Shot(string name,int w=1600,int h=900){var f=$"{output}/{name}.png";ThreeFeatureValidation.CaptureUi(f,w,h);return f;}
 static string P(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n;}
 static float F(string x)=>float.Parse(x,System.Globalization.CultureInfo.InvariantCulture);
 static string V(Vector3 v)=>$"{v.x:F1},{v.y:F2},{v.z:F1}";
 static Vector3 ParseV(string s){var f=s.Split(',').Select(x=>float.Parse(x,System.Globalization.CultureInfo.InvariantCulture)).ToArray();return new Vector3(f[0],f[1],f[2]);}
 static string Mat(Renderer r){var m=r?r.sharedMaterial:null;if(!m)return "-";return m.name+"/"+(m.shader?m.shader.name:"-")+(m.HasProperty("_BaseColor")?" base "+m.GetColor("_BaseColor"):m.HasProperty("_Color")?" color "+m.GetColor("_Color"):"");}

 // Everything within r of each point: renderers (bounds), colliders, and two views.
 IEnumerator Near(string scene,string points){yield return EnterScene(scene);yield return new WaitForSeconds(1);
  var sb=new StringBuilder();int k=0;
  var cam=Camera.main;var chase=FindAnyObjectByType<ChaseCamera>();
  foreach(var spec in points.Split('|')){var f=spec.Split(',');var p=ParseV(string.Join(",",f.Take(3)));float r=f.Length>3?float.Parse(f[3],System.Globalization.CultureInfo.InvariantCulture):12;
   sb.AppendLine($"== point {V(p)} r {r}");
   foreach(var rr in FindObjectsByType<Renderer>(FindObjectsInactive.Include,FindObjectsSortMode.None)){var b=rr.bounds;if(b.SqrDistance(p)>r*r)continue;if(b.size.x>400||b.size.z>400)continue;
    var mf=rr.GetComponent<MeshFilter>();sb.AppendLine($"  R {P(rr.transform)} on {rr.enabled&&rr.gameObject.activeInHierarchy&&!rr.forceRenderingOff} b {V(b.center)} s {V(b.size)} mesh {(mf&&mf.sharedMesh?mf.sharedMesh.name+" v"+mf.sharedMesh.vertexCount:"-")} mat {Mat(rr)} comps {string.Join(",",rr.GetComponents<Component>().Select(c=>c.GetType().Name))}");}
   foreach(var c in FindObjectsByType<Collider>(FindObjectsInactive.Include,FindObjectsSortMode.None)){var b=c.bounds;if(b.SqrDistance(p)>r*r)continue;if(b.size.x>400||b.size.z>400)continue;
    sb.AppendLine($"  C {P(c.transform)} [{c.GetType().Name}] en {c.enabled&&c.gameObject.activeInHierarchy} trig {c.isTrigger} b {V(b.center)} s {V(b.size)} rb {(c.attachedRigidbody?c.attachedRigidbody.isKinematic?"kin":"dyn":"-")}");}
   var hits=Physics.RaycastAll(p+Vector3.up*60,Vector3.down,200,~0,QueryTriggerInteraction.Ignore).OrderBy(h=>h.distance);
   foreach(var h in hits)sb.AppendLine($"  down-hit {P(h.collider.transform)} y {h.point.y:F2} n {V(h.normal)}");
   File.WriteAllText($"{output}/near-{Scene}.txt",sb.ToString());
   if(chase)chase.enabled=false;var cv=CameraViews.Current;if(cv)cv.enabled=false;
   foreach(var off in new[]{new Vector3(-10,5,-10),new Vector3(10,5,10),new Vector3(0,25,-1)}){var eye=p+off;int n=k++;yield return Late(()=>{cam.transform.position=eye;cam.transform.LookAt(p);Shot($"near-{Scene}-{n}");});}
   if(chase)chase.enabled=true;if(cv)cv.enabled=true;}
  Note($"{Scene}: near written");
  if(scene!=RaceFlow.RoamScene)yield return Menu();}

 // Height grid around each point: x,z,top y,top collider; second surface below; plus route samples in the area.
 // grid:Scene:name,x,z,half,step|...
 IEnumerator Grid(string scene,string specs){yield return EnterScene(scene);yield return new WaitForSeconds(1);
  var roads=new List<(string n,Func<float,Vector3> at,float len,Func<float,float> hw)>();
  if(race.road){race.road.Initialize();var rd=race.road;roads.Add(("Main",s=>rd.At(s,out _),rd.Length,s=>rd.HalfWidth(s)));}
  foreach(var b in FindObjectsByType<WoodlandRoute>(FindObjectsSortMode.None)){b.Initialize();var bb=b;roads.Add((b.title,s=>bb.At(s,out _),b.Length,s=>bb.halfWidth));}
  foreach(var spec in specs.Split('|')){var f=spec.Split(',');string name=f[0];var inv=System.Globalization.CultureInfo.InvariantCulture;float cx=float.Parse(f[1],inv),cz=float.Parse(f[2],inv),half=float.Parse(f[3],inv),step=float.Parse(f[4],inv);
   var sb=new StringBuilder("x,z,y,name,y2,name2\n");var names=new Dictionary<string,int>();
   for(float x=cx-half;x<=cx+half;x+=step)for(float z=cz-half;z<=cz+half;z+=step){
    var hits=Physics.RaycastAll(new Vector3(x,600,z),Vector3.down,900,~0,QueryTriggerInteraction.Ignore).Where(h=>!h.collider.attachedRigidbody||h.collider.attachedRigidbody.isKinematic).Where(h=>!h.collider.GetComponentInParent<ArcadeVehicle>()).OrderBy(h=>h.distance).ToArray();
    string N(RaycastHit h){var n=h.collider.name;if(!names.ContainsKey(n))names[n]=names.Count;return names[n].ToString();}
    if(hits.Length==0){sb.AppendLine($"{x:F1},{z:F1},nan,-1,nan,-1");continue;}
    var h0=hits[0];var h1=hits.Skip(1).FirstOrDefault(h=>h.point.y<h0.point.y-.3f);
    sb.AppendLine($"{x:F1},{z:F1},{h0.point.y:F2},{N(h0)},{(h1.collider?h1.point.y.ToString("F2"):"nan")},{(h1.collider?N(h1):"-1")}");}
   File.WriteAllText($"{output}/grid-{Scene}-{name}.csv",sb.ToString());
   string Vis(string n){var c=FindObjectsByType<Collider>(FindObjectsSortMode.None).FirstOrDefault(x=>x.name==n);if(!c)return "?";var r=c.GetComponent<Renderer>();return r?(r.enabled&&r.gameObject.activeInHierarchy&&!r.forceRenderingOff?"visible":"hidden renderer"):"no renderer";}
   File.WriteAllLines($"{output}/grid-{Scene}-{name}-names.txt",names.OrderBy(k=>k.Value).Select(k=>$"{k.Value}\t{k.Key}\t{Vis(k.Key)}"));
   var rs=new StringBuilder("route,s,x,y,z,hw\n");
   foreach(var r in roads)for(float s=0;s<=r.len;s+=1){var q=r.at(s);if(Mathf.Abs(q.x-cx)>half||Mathf.Abs(q.z-cz)>half)continue;rs.AppendLine($"{r.n},{s:F0},{q.x:F2},{q.y:F2},{q.z:F2},{r.hw(s):F2}");}
   File.WriteAllText($"{output}/grid-{Scene}-{name}-routes.csv",rs.ToString());}
  Note($"{Scene}: grids written");
  if(scene!=RaceFlow.RoamScene)yield return Menu();}
}
}
#endif
