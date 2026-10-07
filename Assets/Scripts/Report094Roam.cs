#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Racer {
// 0.94 Parts B and C checks (controller only for the menus: emulated gamepads; shots at 3840x2160), on a copy of Dan's save:
//  roam94:layout       split-screen Free Roam: Mode chosen on the setup screen with the controller, player 2 a second
//                      controller (human): both vehicles, both minimaps, the direction line, each view in each half (X on
//                      each player's own controller), first person heads; pause (Start), MAP, travel brings both
//  roamai94:layout     the same with player 2 the AI (it cruises the roads)
//  solo94              single-player Free Roam afterwards: acorns, discovery and its weather setting as before
//  police94:time       Police Chase with the AI as the runner: setup rows (the AI cop greyed), the start (runner first, the
//                      cop 3 s later), the meter filling and draining, a reset by the runner (held 2 s), caught, the results;
//                      shots of the patrol car with its lights
public sealed partial class Report080Checks {
 IEnumerator Roam094(string[] a)=>a[0] switch{"roam94"=>SplitRoam094(a[1]=="lr",false),"roamai94"=>SplitRoam094(a[1]=="lr",true),"solo94"=>Solo094(),"police94"=>Police094(a[1]),_=>null};
 IEnumerator SetupRoam094(SplitScreen.Kind mode,bool ai,bool lr,TimeOfDay time=TimeOfDay.Day,Weather weather=Weather.Clear){
  yield return DanCopy094();Pads090();SplitScreen.P1Device=null;SplitScreen.P2Device=null;SplitScreen.Mode=SplitScreen.Kind.Race;
  flow.Save.Settings.splitLeftRight=lr;Menus089.OpenSplitSetup();yield return Settle091();
  if(!ai){yield return Press090(pad2,GamepadButton.South);yield return Settle091();}
  yield return Goto091("split-mode");for(int i=0;i<3&&SplitScreen.Mode!=mode;i++)yield return Press091(GamepadButton.DpadRight);
  Check(SplitScreen.Mode==mode,$"setup: Mode {SplitScreen.Mode} chosen with the D-pad; rows [{string.Join(", ",Rows091().Select(r=>r.name))}]");
  SplitScreen.P1Vehicle="moto";SplitScreen.P2Vehicle="atv";SplitScreen.Course=0;SplitScreen.Time=time;SplitScreen.Weather=weather;SplitScreen.Traffic=true;Menus089.Show();yield return Settle091();
  yield return Late(()=>Shot4k($"B-setup-{mode}{(ai?"-ai":"")}"));
  yield return Choose091("split-start");yield return SplitRunning090();yield return new WaitForSeconds(2);}
 IEnumerator SplitRoam094(bool lr,bool ai){
  yield return SetupRoam094(SplitScreen.Kind.FreeRoam,ai,lr);string tag=(lr?"lr":"tb")+(ai?"-ai":"");
  var split=SplitScreen.Race;var roam=SplitRoam.Current;
  Check(Scene==RaceFlow.RoamScene&&race.FreeRoam&&split&&roam&&split.P2Car&&split.Camera2,$"{tag}: Free Roam for two in {Scene}: player 2's {split?.P2Car?.GetComponent<VehicleConfiguration>().Profile.Name} {(split&&split.P2Car?Vector3.Distance(split.P2Car.transform.position,race.vehicle.transform.position):-1):F1} m from player 1");
  var p2start=split.P2Car.transform.position;
  // both players drive on (player 1 with the traffic driver, player 2 by its own device or the AI)
  var p1=race.vehicle;var pilot=p1.gameObject.AddComponent<RoadDriver>();pilot.Initialize(race,p1,false,-1,1.1f);
  RoadDriver pilot2=null;if(!ai){pilot2=split.P2Car.gameObject.AddComponent<RoadDriver>();pilot2.Initialize(race,split.P2Car,false,-1,1.0f);}
  yield return new WaitForSeconds(12);
  float moved=Vector3.Distance(p2start,split.P2Car.transform.position);
  var toward=FindObjectsByType<UnityEngine.UI.Text>(FindObjectsSortMode.None).Where(t=>t.enabled&&t.text.Contains("FREE ROAM")).Select(t=>t.text.Replace("\n"," / ")).ToList();
  Check(moved>40&&toward.Count==2,$"{tag}: player 2 moved {moved:F0} m in 12 s{(ai?" (the AI cruising the roads)":"")}; each half: {string.Join(" | ",toward)}");
  Check(NameTags.Shown>=0,$"{tag}: name tags shown now: {NameTags.Shown}");
  yield return Late(()=>Shot4k($"B-roam-{tag}"));
  if(!ai){
   // each view in each half: X on each player's own controller
   var v1=CameraViews.Current;var v2=split.Camera2.GetComponent<CameraViews>();
   for(int k=0;k<4;k++){
    string n1=v1.ShownView,n2=v2.ShownView;
    Check(v1.PlayerViewName==CameraViews.Names[(v1.PlayerView==CameraViews.View.Chase?0:(int)v1.PlayerView)]&&v1!=v2,$"{tag}: view {k+1}: player 1 {v1.PlayerViewName} (shown {n1}), player 2 {v2.PlayerViewName} (shown {n2}); head hidden: P1 {v1.HeadHidden}, P2 {v2.HeadHidden}");
    yield return Late(()=>Shot4k($"B-views-{tag}-{k+1}-{v1.PlayerViewName.Replace(' ','-')}-{v2.PlayerViewName.Replace(' ','-')}"));
    var before1=v1.PlayerView;var before2=v2.PlayerView;
    yield return Press090(pad91,GamepadButton.West);yield return new WaitForSeconds(.2f);bool only1=v1.PlayerView!=before1&&v2.PlayerView==before2;
    yield return Press090(pad2,GamepadButton.West);yield return new WaitForSeconds(.8f);
    Check(only1&&v2.PlayerView!=before2,$"{tag}: X on player 1's controller changed only player 1's view ({before1} > {v1.PlayerView}); X on player 2's changed player 2's ({before2} > {v2.PlayerView})");}
   // first person in both halves at once: each head hidden only from its own camera
   while(v1.PlayerView!=CameraViews.View.FirstPerson){yield return Press090(pad91,GamepadButton.West);yield return new WaitForSeconds(.2f);}
   while(v2.PlayerView!=CameraViews.View.FirstPerson){yield return Press090(pad2,GamepadButton.West);yield return new WaitForSeconds(.2f);}
   yield return new WaitForSeconds(1.2f);Check(v1.HeadHidden&&v2.HeadHidden&&CameraViews.For(p1)==v1&&CameraViews.For(split.P2Car)==v2,$"{tag}: first person in both halves: both heads hidden from their own cameras (P1 {v1.HeadHidden}, P2 {v2.HeadHidden})");
   yield return Late(()=>Shot4k($"B-first-person-both-{tag}"));
   // pause with player 2's Start; MAP; travel brings both players
   yield return Press090(pad2,GamepadButton.Start);yield return Settle091();
   Check(flow.State==RaceFlow.Stage.Paused&&Button90("map"),$"{tag}: Start on player 2's controller pauses: rows [{string.Join(", ",Rows091().Select(r=>r.name))}]");yield return Late(()=>Shot4k($"B-pause-{tag}"));
   yield return Choose091("map");var map=flow.GetComponent<ExplorationMap>();Check(map.Opened,$"{tag}: MAP opens");yield return Late(()=>Shot4k($"B-map-{tag}"));
   int dest=Enumerable.Range(0,map.destinations.Length).FirstOrDefault(i=>map.Discovered(map.destinations[i].id)&&Vector3.Distance(map.destinations[i].position,p1.transform.position)>300);
   bool travelled=map.Travel(dest);yield return new WaitForSecondsRealtime(.5f);float apart=Vector3.Distance(p1.transform.position,split.P2Car.transform.position);
   Check(travelled&&apart<15,$"{tag}: travel to {map.destinations[dest].title}: player 1 at {Vector3.Distance(p1.transform.position,map.destinations[dest].position):F0} m from it, player 2 {apart:F1} m from player 1");
   map.Close();yield return Settle091();if(flow.State!=RaceFlow.Stage.Racing)flow.Resume();yield return new WaitForSeconds(1);yield return Late(()=>Shot4k($"B-after-travel-{tag}"));}
  Destroy(pilot);if(pilot2)Destroy(pilot2);
  flow.Pause();yield return Settle091();flow.QuitSplit(false);float t0=Time.realtimeSinceStartup;while(Time.realtimeSinceStartup-t0<90&&(Scene==RaceFlow.RoamScene||flow==null||!flow.Started||LoadingScreen.Holding)){yield return null;Bind();}
  Check(!SplitScreen.Active&&!SplitRoam.Current&&flow.State==RaceFlow.Stage.Ready,$"{tag}: back to the menu ({Scene}, {flow.State})");}
 IEnumerator Solo094(){
  yield return DanCopy094();flow.Save.Settings.roamWeather=1;
  yield return EnterScene(RaceFlow.RoamScene);yield return new WaitForSeconds(2);
  var col=race.GetComponent<ExplorationCollection>();int found=col.Found;
  Check(!SplitScreen.Active&&race.FreeRoam&&WorldLook.Current.Mode=="Free Roam"&&WorldLook.Current.RoamWeather==Weather.Rain,$"single-player Free Roam: mode {WorldLook.Current.Mode}, weather setting Rain shown as {WorldLook.Current.RoamWeather}, acorns {found}/24, no second player {(SplitRoam.Current==null)}");
  var site=col.sites.FirstOrDefault(s=>!col.Discovered(s.id));
  if(site!=null){yield return Put093(site.position+Vector3.forward*6,180);var car=race.vehicle;car.Body.linearVelocity=-Vector3.forward*8;yield return new WaitForSeconds(2.5f);
   Check(col.Found==found+1,$"single-player Free Roam: driving through {site.title} found it ({found} > {col.Found}): acorns still recorded");}
  yield return Late(()=>Shot4k("B-solo-roam"));flow.Save.Settings.roamWeather=0;}
 IEnumerator Police094(string time){
  var t=(TimeOfDay)Enum.Parse(typeof(TimeOfDay),time);
  yield return SetupRoam094(SplitScreen.Kind.Police,true,false,t,Weather.Clear);
  var police=PoliceChase.Current;var split=SplitScreen.Race;
  Check(police&&police.Cop==1&&police.Rounds==1,$"police: player 1 is the cop (the AI can only run: one round), the cop's vehicle {race.vehicle.GetComponent<VehicleConfiguration>().Profile.Name}, the runner's {split.P2Car.GetComponent<VehicleConfiguration>().Profile.Name}");
  var cop=race.vehicle;var runner=split.P2Car;float gap=Vector3.Distance(cop.Body.position,runner.Body.position);
  Check(gap>60&&gap<100,$"police: the cop starts {gap:F0} m behind the runner");
  // GO: the runner first, the cop 3 s later
  float t0=Time.time;while(police.State==PoliceChase.Phase.Starting&&!police.Holds(cop)==false&&police.Holds(runner)&&Time.time-t0<6)yield return null;
  float runnerGo=Time.time;while(police.Holds(cop)&&Time.time-t0<10)yield return null;float copGo=Time.time;
  Check(copGo-runnerGo>2.5f&&copGo-runnerGo<3.5f,$"police: the runner released, the cop {copGo-runnerGo:F1} s later; siren {police.Siren}");
  yield return new WaitForSeconds(2);yield return Late(()=>Shot4k($"C-chase-start-{time}"));
  // the cop chases (a simple pilot: full throttle at the runner, braking when close)
  cop.GetComponent<VehicleInput>().enabled=false;bool piloting=true;float meterMax=0;
  IEnumerator Chase(float seconds,bool stopRunner){float c0=Time.time;while(Time.time-c0<seconds&&police.State==PoliceChase.Phase.Running){
    yield return new WaitForFixedUpdate();if(!piloting)continue;var to=runner.Body.position-cop.Body.position;float d=to.magnitude;
    // far away: the cop follows the road (the traffic driver, quicker than traffic, the runner's way); close: straight at it
    var road=cop.GetComponent<RoadDriver>();if(d>45){var net=race.ambientRoad?race.ambientRoad:race.road;float L=net.Length,delta=Mathf.Repeat(net.Project(runner.Body.position,out _)-net.Project(cop.Body.position,out _)+L*.5f,L)-L*.5f;int dir=delta>=0?1:-1;
     if(road&&road.Direction!=dir){DestroyImmediate(road);road=null;}if(!road){road=cop.gameObject.AddComponent<RoadDriver>();road.Initialize(race,cop,false,dir,1.35f);}meterMax=Mathf.Max(meterMax,police.Meter);continue;}
    if(road)Destroy(road);cop.enabled=false;var local=Quaternion.Inverse(Quaternion.LookRotation(Vector3.ProjectOnPlane(cop.transform.forward,Vector3.up)))*to;
    float steer=Mathf.Clamp(Mathf.Atan2(local.x,local.z)*1.6f,-1,1);float speed=cop.ForwardSpeed;float want=d<12?3:d<40?14:40;
    cop.Simulate(speed<want?1:0,speed>want+2?.8f:0,steer,Time.fixedDeltaTime);meterMax=Mathf.Max(meterMax,police.Meter);}}
  yield return Chase(20,false);
  // the runner stops (its driver off): the cop closes in and the meter fills; then the cop backs off and it drains
  var ai=runner.GetComponent<RoadDriver>();if(ai)ai.enabled=false;runner.Body.linearVelocity=Vector3.zero;
  // (the test pilot cannot follow the road network far, so the cop is set on the road 25 m behind the stopped runner)
  IEnumerator Behind(){var net=race.ambientRoad?race.ambientRoad:race.road;float sr=net.Project(runner.Body.position,out _);var f0=Vector3.ProjectOnPlane(runner.transform.forward,Vector3.up).normalized;
   foreach(float back in new[]{25f,-25f,32f}){var at=net.At(sr-back*Mathf.Sign(Vector3.Dot(net.At(sr+1,out _)-net.At(sr,out _),f0)),out var dir);
    var rg=cop.GetComponent<RoadDriver>();if(rg)DestroyImmediate(rg);cop.enabled=true;if(cop.GetComponent<VehicleRespawn>().TryFastTravel(at+Vector3.up,Quaternion.LookRotation(Vector3.ProjectOnPlane(runner.Body.position-at,Vector3.up))))break;}
   yield return new WaitForFixedUpdate();}
  yield return Behind();
  float c1=Time.time;while(police.Meter<.5f&&Time.time-c1<150&&police.State==PoliceChase.Phase.Running){yield return Chase(.1f,true);if(!runner.enabled)runner.Simulate(0,1,0,Time.fixedDeltaTime);}
  float filled=police.Meter;Check(filled>=.5f,$"police: the runner stopped, the cop within {Vector3.Distance(cop.Body.position,runner.Body.position):F1} m: the meter filled to {filled:P0}");yield return Late(()=>Shot4k($"C-meter-filling-{time}"));
  piloting=false;if(cop.GetComponent<RoadDriver>())Destroy(cop.GetComponent<RoadDriver>());cop.enabled=true;var back=cop.Body.position-(runner.Body.position-cop.Body.position).normalized*20;cop.Body.position=back+Vector3.up;cop.Body.linearVelocity=Vector3.zero;cop.transform.position=cop.Body.position;
  yield return new WaitForSeconds(1);Check(police.Meter<filled-.2f,$"police: the cop 20 m away: the meter drains {filled:P0} > {police.Meter:P0}");
  // a reset by the runner holds it still for 2 s
  var resp=runner.GetComponent<VehicleRespawn>();resp.ResetVehicle();float r0=Time.time;while(resp.Pending&&Time.time-r0<5)yield return null;
  yield return new WaitForSeconds(.3f);bool heldNow=police.Holds(runner);yield return new WaitForSeconds(2.2f);
  Check(police.RunnerResets==1&&heldNow&&!police.Holds(runner),$"police: the runner's reset held it for 2 s (resets {police.RunnerResets}, held after the reset {heldNow}, released after 2 s {!police.Holds(runner)})");
  // caught: the cop drives up to the stopped runner
  if(ai)ai.enabled=false;piloting=true;yield return Behind();float c2=Time.time;
  while(police.State==PoliceChase.Phase.Running&&Time.time-c2<60){runner.Body.linearVelocity=Vector3.zero;yield return Chase(.1f,true);}
  Check(police.State==PoliceChase.Phase.Caught&&police.Runs.Count==1&&police.Runs[0].caught,$"police: CAUGHT after {RaceHud.FormatTime(police.Clock)} (meter max {meterMax:P0})");
  yield return Late(()=>Shot4k($"C-caught-{time}"));
  // the patrol car with its lights flashing, from the side
  var lights=cop.GetComponent<PoliceLights>();var cam=Camera.main;var chase=cam.GetComponent<ChaseCamera>();var views=CameraViews.Current;chase.enabled=false;if(views)views.enabled=false;
  foreach(var c in FindObjectsByType<Canvas>(FindObjectsSortMode.None))c.enabled=false;lights.Siren=true;var rect=cam.rect;cam.rect=new Rect(0,0,1,1);var cam2=split.Camera2;if(cam2)cam2.enabled=false;
  for(int k=0;k<2;k++){yield return new WaitForSeconds(k==0?.05f:.65f);var w=cop.transform;cam.transform.position=w.position+w.right*6.5f+w.forward*3.5f+Vector3.up*2.2f;cam.transform.LookAt(w.position+Vector3.up*.6f);yield return Late(()=>Shot4k($"C-patrol-car-{time}-{k+1}"));}
  cam.rect=rect;if(cam2)cam2.enabled=true;foreach(var c in FindObjectsByType<Canvas>(FindObjectsSortMode.None))c.enabled=true;chase.enabled=true;if(views)views.enabled=true;
  float e0=Time.realtimeSinceStartup;while(flow.State!=RaceFlow.Stage.Results&&Time.realtimeSinceStartup-e0<20)yield return null;yield return Settle091();
  Check(flow.State==RaceFlow.Stage.Results&&Button90("rematch"),$"police: results: {((UnityEngine.UI.Text)typeof(RaceMenus).GetField("title",Any).GetValue(Menus089)).text} / {((UnityEngine.UI.Text)typeof(RaceMenus).GetField("details",Any).GetValue(Menus089)).text.Replace("\n"," ")} || {CellsText094()}");
  yield return Late(()=>Shot4k($"C-results-{time}"));
  yield return Choose091("menu");float q0=Time.realtimeSinceStartup;while(Time.realtimeSinceStartup-q0<90&&(Scene==RaceFlow.RoamScene||flow==null||!flow.Started||LoadingScreen.Holding)){yield return null;Bind();}
  Check(!SplitScreen.Active&&!PoliceChase.Current,$"police: Main menu: back in {Scene}");}
}
}
#endif
