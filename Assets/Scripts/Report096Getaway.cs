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
using UnityEngine.EventSystems;

namespace Racer {
// 0.96 Part E checks (see Report096Checks): getaway96:mode:vehicle:difficulty
//   road    the runner does nothing clever: it drives the roads at pace 1.25 (the 0.94 AI runner); caught within ~2 minutes expected on Normal
//   hidden  the runner leaves the road far from every cop and stays still out of sight: the radio line, the exits covered, the escape meter to ESCAPED
//   block   heat 3 forced: a roadblock appears ahead of the runner on the road
//   two     two-player Getaway with the AI as player 2: both run, the split HUD
public sealed partial class Report080Checks {
 string mode96="";
 IEnumerator StartGetaway096(string vehicle,int difficulty,int minutes,bool two=false){
  yield return DanCopy095();Pads090();SplitScreen.Solo=!two;SplitScreen.P2Device=null;SplitScreen.P2AiRunner=two;SplitScreen.P1Device=pad1;SplitScreen.Mode=SplitScreen.Kind.Police;SplitScreen.PoliceGame=SplitScreen.Game.Getaway;if(mode96=="runner"){SplitScreen.PoliceGame=SplitScreen.Game.CopRunner;SplitScreen.SoloRole=2;SplitScreen.CopFirst=2;}
  SplitScreen.PoliceDifficulty=difficulty;SplitScreen.PoliceMinutes=minutes;SplitScreen.Course=0;SplitScreen.Time=TimeOfDay.Day;SplitScreen.Weather=Weather.Clear;SplitScreen.Traffic=true;SplitScreen.P1Vehicle=vehicle;SplitScreen.P2Vehicle="atv";SplitScreen.P2Ai=SplitScreen.Solo||two;
  flow.StartSplit();yield return WaitScene095(()=>SplitScreen.Active&&GetawayChase.Current&&GetawayChase.Current.Runners.Count>0&&flow.State==RaceFlow.Stage.Racing,150);}
 string State096(GetawayChase g){var r=g.Runners[0];return $"t {g.Clock:F0}s heat {g.Heat} cops {g.Cops.Count(c=>!c.block)}+{g.Cops.Count(c=>c.block)} blocks, seen {r.seen} escape {r.escape:F2} bust {r.bust:F2} offroad {r.offRoad} roles [{string.Join(",",g.Cops.Select(c=>c.driver.Role.ToString()[0]+""+(c.sees?"*":"")))}] dist {string.Join("/",g.Cops.Select(c=>Mathf.RoundToInt(Vector3.Distance(c.car.Body.position,r.body.position))))} speed {r.body.linearVelocity.magnitude:F0} fails {string.Join("/",g.Cops.Select(c=>c.driver.GoFails+(c.driver.Arrived?"A":"")))} copV {string.Join("/",g.Cops.Select(c=>Mathf.RoundToInt(c.car.Body.linearVelocity.magnitude)))} goal->runner {string.Join("/",g.Cops.Select(c=>c.driver.GoalNode>=0?Mathf.RoundToInt(Vector3.Distance(g.Net.P[c.driver.GoalNode],r.body.position)):-1))}";}
 // the menus, with a controller only: the Game row reaches Getaway, every row reachable, B goes back; the solo Runner role; two players with the AI as player 2
 IEnumerator GetawaySetup096(){
  yield return DanCopy095();Pads090();SplitScreen.Solo=true;SplitScreen.P2Device=null;SplitScreen.P2AiRunner=false;SplitScreen.PoliceGame=SplitScreen.Game.CopRunner;SplitScreen.SoloRole=1;
  yield return Choose091("police");yield return Goto091("police-game");yield return Press091(GamepadButton.DpadRight);yield return Press091(GamepadButton.DpadRight);
  Check(SplitScreen.PoliceGame==SplitScreen.Game.Getaway&&Button90("police-start")&&Text(Button90("police-start")).StartsWith("START GETAWAY")&&Button90("police-difficulty"),$"Game: Getaway chosen with the D-pad: {Title095} / {Text(Button90("police-game"))} / rows [{string.Join(", ",Rows091().Select(r=>r.name))}]");
  yield return Late(()=>Shot4k("E-getaway-setup-solo"));yield return Walk091("getaway setup (1 player)",true,()=>Page90=="");
  yield return Choose091("police");yield return Goto091("police-game");yield return Press091(GamepadButton.DpadRight);
  yield return Goto091("police-players");yield return Press091(GamepadButton.DpadRight);
  Check(!SplitScreen.Solo&&Button90("split-p2"),$"two players: rows [{string.Join(", ",Rows091().Select(r=>r.name))}]");
  yield return Goto091("split-p2");yield return Press091(GamepadButton.DpadRight);
  Check(SplitScreen.P2AiRunner&&Text(Button90("split-p2")).Contains("AI"),$"player 2 as the AI runner: {Text(Button90("split-p2"))}; start enabled {Button90("police-start").interactable}");
  yield return Late(()=>Shot4k("E-getaway-setup-two"));yield return Walk091("getaway setup (2 players, AI as player 2)",true,()=>Page90=="");
  // the solo Runner role in Cop vs Runner
  SplitScreen.P2AiRunner=false;SplitScreen.Solo=true;SplitScreen.PoliceGame=SplitScreen.Game.CopRunner;SplitScreen.SoloRole=1;Menus089.Show();yield return Settle091();
  yield return Choose091("police");Check(Page90=="police"&&Button90("police-role"),$"Cop vs Runner solo: {Text(Button90("police-role"))}");
  yield return Goto091("police-role");yield return Press091(GamepadButton.DpadRight);
  Check(SplitScreen.SoloRole==2&&Text(Button90("police-role")).Contains("Runner")&&!Text(Button90("police-role")).Contains("coming later")&&Button90("police-difficulty"),$"the Runner role now works: {Text(Button90("police-role"))}; rows [{string.Join(", ",Rows091().Select(r=>r.name))}]");
  yield return Late(()=>Shot4k("E-runner-role-setup"));yield return Walk091("police setup, Runner role",true,()=>Page90=="");}
 IEnumerator Getaway096(string mode,string vehicle,int difficulty){
  mode96=mode;yield return StartGetaway096(vehicle,difficulty,mode=="runner"?3:5,mode=="two");var g=GetawayChase.Current;
  Check(g!=null&&g.Net!=null&&g.Net.Count>500,$"the road network: {g?.Net?.Count} nodes on {g?.Net?.Roads.Count} roads, {g?.Net?.ComponentCount} connected parts (largest {g?.Net?.Component.Count(c=>c==g.Net.LargestComponent)} nodes)");
  Note("roads: "+string.Join("; ",Enumerable.Range(0,g.Net.Roads.Count).Select(i=>g.Net.RoadNames[i]+" "+g.Net.Component.Where((c,k)=>g.Net.Road[k]==i).Distinct().Count()+" parts, "+g.Net.Road.Count(r=>r==i)+" nodes")));
  Check(g.Cops.Count==(mode=="runner"?1:2)&&SplitScreen.OneView==(mode!="two"),$"solo={SplitScreen.Solo}: {g.Cops.Count} cops waiting behind the runner, one view {SplitScreen.OneView}, runners {g.Runners.Count}");
  float t0=Time.time;while(g.State==GetawayChase.Phase.Starting&&Time.time-t0<20)yield return null;
  var runner=g.Runners[0];var car=race.vehicle;
  yield return Late(()=>Shot4k("E-getaway-start-"+mode));
  RoadDriver pilot=null;
  if(mode=="road"||mode=="two"||mode=="block"||mode=="runner"){pilot=car.gameObject.AddComponent<RoadDriver>();pilot.Initialize(race,car,false,1,1.25f);car.GetComponent<VehicleInput>().enabled=false;}
  if(mode=="block"){var heat=typeof(GetawayChase).GetProperty("Heat");heat.SetValue(g,3);}
  if(mode=="hidden"){
   // leave the road: put the runner on open ground far from every cop (the road network's far side), held still
   yield return new WaitForSeconds(6);
   var far=g.Net.P.Select((p,i)=>(p,i)).Where(x=>Vector3.Distance(x.p,car.Body.position)>600&&Vector3.Distance(x.p,car.Body.position)<1200).OrderBy(x=>UnityEngine.Random.value).First();
   var away=far.p+Vector3.Cross(Vector3.up,g.Net.Tangent(far.i)).normalized*60;away.y=far.p.y;
   if(Physics.Raycast(away+Vector3.up*80,Vector3.down,out var hit,200,~0,QueryTriggerInteraction.Ignore))away=hit.point+Vector3.up*1.2f;
   car.Body.position=away;car.Body.rotation=Quaternion.identity;car.transform.SetPositionAndRotation(away,Quaternion.identity);car.Body.linearVelocity=Vector3.zero;
   Note($"runner put off-road at {V(away)}, {g.Net.OffNet(away):F0} m from the road");}
  Time.timeScale=mode=="block"?1:3;float last=-10;t0=Time.realtimeSinceStartup;bool shotHud=false,shotBlock=false,shotExit=false;string firstRadio="";
  while(g.State!=GetawayChase.Phase.Over&&g.State!=GetawayChase.Phase.Done&&flow.State==RaceFlow.Stage.Racing&&Time.realtimeSinceStartup-t0<(mode=="block"?150:600)){AudioListener.volume=0;yield return null;
   if(g.Radio!=""&&g.Radio!=firstRadio){firstRadio=g.Radio;Note("   radio: "+g.Radio);}
   if(g.Clock-last>=10){last=g.Clock;Note("   "+State096(g));}
   if(!shotHud&&g.Clock>12){shotHud=true;Time.timeScale=1;yield return Late(()=>Shot4k("E-getaway-hud-"+mode));Time.timeScale=mode=="block"?1:3;}
   if(mode=="block"&&!shotBlock&&g.Blocks.Count>0){shotBlock=true;Note("   roadblock at "+V(g.Blocks[0].centre)+" ahead "+Vector3.Distance(g.Blocks[0].centre,race.vehicle.Body.position).ToString("F0")+" m");}
   if(mode=="block"&&shotBlock&&!shotExit&&g.Blocks.Count>0&&Vector3.Distance(g.Blocks[0].centre,car.Body.position)<75){shotExit=true;yield return Late(()=>Shot4k("E-getaway-roadblock"));}
   if(mode=="hidden"&&!shotExit&&g.ExitPoints.Count>0&&g.Cops.Count(c=>c.driver.Role==CopDriver.Task.Exit&&c.driver.Arrived)>0){shotExit=true;Time.timeScale=1;yield return Late(()=>Shot4k("E-getaway-exit-minimap"));
    var wait=g.Cops.First(c=>c.driver.Role==CopDriver.Task.Exit&&c.driver.Arrived);Note($"   a cop waits at an exit with lights on: {V(wait.car.Body.position)}; exits {g.ExitPoints.Count}");
    var cam=Camera.main;var chase=FindAnyObjectByType<ChaseCamera>();if(chase)chase.enabled=false;var p=wait.car.Body.position;yield return Late(()=>{cam.transform.SetPositionAndRotation(p+new Vector3(-9,4.5f,-9),Quaternion.LookRotation(p-(p+new Vector3(-9,4.5f,-9))+Vector3.up));Shot4k("E-getaway-cop-at-exit");});if(chase)chase.enabled=true;Time.timeScale=3;}
  }
  Time.timeScale=1;yield return new WaitForSeconds(.5f);
  Check(g.State==GetawayChase.Phase.Over||g.State==GetawayChase.Phase.Done||mode=="block",$"mode {mode} ({vehicle}, difficulty {difficulty}): ended {g.State} at {g.Clock:F0} s: {string.Join(" / ",g.Runners.Select(r=>r.outcome+" "+r.freeSeconds.ToString("F0")+"s heat "+r.topHeat+" dodged "+r.dodged+" blocks "+r.blocks))}");
  if(mode=="road")Check(g.Runners[0].caught&&g.Runners[0].freeSeconds<=150,$"caught within about 2 minutes on Normal: {(g.Runners[0].caught?"caught at "+g.Runners[0].freeSeconds.ToString("F0")+" s":"NOT caught: "+g.Runners[0].outcome)}");
  if(mode=="hidden")Check(g.Runners[0].escaped,$"hidden off-road: {g.Runners[0].outcome} after {g.Runners[0].freeSeconds:F0} s; radio {string.Join(" | ",g.RadioLog)}");
  if(mode=="block")Check(shotBlock,$"heat 3: a roadblock went up ({g.Blocks.Count} up now)");
  if(g.State==GetawayChase.Phase.Over){float w0=Time.realtimeSinceStartup;while(flow.State!=RaceFlow.Stage.Results&&Time.realtimeSinceStartup-w0<30)yield return null;yield return Settle091();Check(flow.State==RaceFlow.Stage.Results,$"results: {Title095} / {Details095} || {CellsText094()}");yield return Late(()=>Shot4k("E-getaway-results-"+mode));}
  flow.QuitSplit(false);yield return WaitScene095(()=>!SplitScreen.Active&&flow.State==RaceFlow.Stage.Ready);}
}
}
#endif
