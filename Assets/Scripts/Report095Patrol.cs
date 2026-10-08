#if UNITY_EDITOR
using System;
using System.Linq;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Racer {
// 0.95 Part G check (controller only for the menus):
//  patrol95   POLICE CHASE > Game: Speed Patrol, one player, 3 minutes: the radar and limit on the HUD, a speeder clocked,
//             chased and pulled over (a catch), one let go (gets away), a traffic hit (-50), lights on a car that was not
//             speeding (-25 NO VIOLATION); the round's end, the results and the Top 10; a two-player start with both radars.
//             The events are set up by the check (the cop is put where each needs it); the game's own rules decide them.
public sealed partial class Report080Checks {
 IEnumerator Patrol095(){
  yield return DanCopy095();Pads090();SplitScreen.Solo=true;SplitScreen.P2Device=null;SplitScreen.PoliceGame=SplitScreen.Game.CopRunner;
  yield return Choose091("police");yield return Goto091("police-game");yield return Press091(GamepadButton.DpadRight);
  Check(SplitScreen.PoliceGame==SplitScreen.Game.SpeedPatrol&&Button90("police-start")&&Text(Button90("police-start")).StartsWith("START PATROL"),$"Game: Speed Patrol chosen with the D-pad: {Title095} / rows [{string.Join(", ",Rows091().Select(r=>r.name))}]");
  yield return Walk091("speed patrol setup (1 player)",true);
  yield return Goto091("police-limit");for(int i=0;i<3;i++)yield return Press091(GamepadButton.DpadLeft);
  Check(SplitScreen.PoliceMinutes==3,$"round length: {Text(Button90("police-limit"))}");yield return Late(()=>Shot4k("G-setup-solo"));
  SplitScreen.Course=0;SplitScreen.Time=TimeOfDay.Day;SplitScreen.Weather=Weather.Clear;Menus089.Show();yield return Settle091();
  yield return Choose091("police-start");yield return WaitScene095(()=>SplitScreen.Active&&SpeedPatrol.Current&&SpeedPatrol.Current.Cops.Count>0);
  var patrol=SpeedPatrol.Current;var cop=patrol.CopOf(1);var car=race.vehicle;
  Check(patrol.Cops.Count==1&&SplitScreen.OneView&&race.traffic&&car.GetComponent<VehicleConfiguration>().profileId==VehicleProfile.Police.Id,$"solo patrol: one cop in the Patrol Car, full screen, traffic {race.traffic} ({race.Drivers.Count} cars)");
  float t0=Time.time;while(patrol.State==SpeedPatrol.Phase.Starting&&Time.time-t0<10)yield return null;yield return new WaitForSeconds(2);
  // traffic keeps to the limits
  var speeds=race.Drivers.Where(d=>d&&d.GetComponent<AmbientVehicle>()).Select(d=>(d,lim:SpeedPatrol.LimitAt(race,d.Car.Body.position,out _),mph:SpeedPatrol.Mph(d.Car.Body.linearVelocity.magnitude))).ToList();
  Check(speeds.All(x=>x.mph<=x.lim+2),$"ordinary traffic at or under the limit: {string.Join(", ",speeds.Take(10).Select(x=>$"{x.mph:0}/{x.lim}"))}");
  var input=car.GetComponent<VehicleInput>();input.enabled=false;
  void Behind(ArcadeVehicle target,float back){var f=Vector3.ProjectOnPlane(target.transform.forward,Vector3.up).normalized;var at=target.Body.position-f*back+Vector3.up*.6f;car.Body.position=at;car.Body.rotation=Quaternion.LookRotation(f);car.transform.SetPositionAndRotation(at,car.Body.rotation);car.Body.linearVelocity=target.Body.linearVelocity;car.Body.angularVelocity=Vector3.zero;}
  IEnumerator NewSpeeder(){cop.nextSpawn=0;float s0=Time.time;while(Time.time-s0<5&&!patrol.Speeders.Any(s=>!s.clocked&&!s.caught&&!s.gone)){cop.nextSpawn=0;yield return null;}}
  // 1. a speeder: clocked by the radar, lights on, chased and pulled over
  yield return NewSpeeder();var sp=patrol.Speeders.FirstOrDefault(s=>!s.clocked&&!s.caught&&!s.gone);
  if(sp!=null){
   yield return new WaitForSeconds(2);for(int i=0;i<40&&!sp.clocked;i++){Behind(sp.Car,40);yield return new WaitForFixedUpdate();}
   Check(sp.clocked&&cop.radarOver,$"radar: {cop.radar:0} mph in a {cop.radarLimit} (red {cop.radarOver}); speeder clocked {sp.clockedOver} over; on the minimap {patrol.ClockedMarks.Count()}");
   yield return Late(()=>Shot4k("G-radar"));
   cop.siren=true;for(int i=0;i<12&&!sp.pursuit;i++){Behind(sp.Car,20);yield return new WaitForFixedUpdate();}
   Check(sp.pursuit,"lights on within 30 m: the pursuit starts");
   float c0=Time.time;bool shot=false;while(!sp.caught&&Time.time-c0<20){Behind(sp.Car,9);yield return new WaitForFixedUpdate();if(!shot&&patrol.MeterFor(1)>.5f){shot=true;yield return Late(()=>Shot4k("G-pull-over"));}}
   Check(sp.caught&&cop.catches==1,$"pulled over: {cop.line} (points {cop.points}, fastest {cop.fastest})");cop.siren=false;}
  else Check(false,"no speeder appeared");
  car.Body.linearVelocity=Vector3.zero;
  // 2. one that gets away: clocked, never caught (its 60 s run out)
  yield return NewSpeeder();var away=patrol.Speeders.FirstOrDefault(s=>!s.clocked&&!s.caught&&!s.gone);
  if(away!=null){yield return new WaitForSeconds(2);for(int i=0;i<40&&!away.clocked;i++){Behind(away.Car,40);yield return new WaitForFixedUpdate();}
   car.Body.position+=Vector3.up*0;away.clockedAt=Time.time-SpeedPatrol.GetAwaySeconds+1.5f;yield return new WaitForSeconds(2.5f);
   Check(away.gone&&patrol.GotAway==1,$"a clocked speeder not caught in 60 s gets away ({patrol.GotAway}); line \"{cop.line}\"");}
  // 3. a traffic hit
  var plain=race.Drivers.First(d=>d&&d.GetComponent<AmbientVehicle>()&&!patrol.Speeders.Any(s=>s.driver==d));int before=cop.points;
  {var f=Vector3.ProjectOnPlane(plain.Car.transform.forward,Vector3.up).normalized;var at=plain.Car.Body.position-f*7+Vector3.up*.5f;car.Body.position=at;car.Body.rotation=Quaternion.LookRotation(f);car.transform.SetPositionAndRotation(at,car.Body.rotation);car.Body.linearVelocity=f*(plain.Car.Body.linearVelocity.magnitude+12);}
  for(int i=0;i<60&&cop.hits==0;i++)yield return new WaitForFixedUpdate();
  Check(cop.hits==1&&cop.points==before-SpeedPatrol.HitPenalty,$"hitting traffic: {cop.line} ({before} > {cop.points})");car.Body.linearVelocity=Vector3.zero;yield return new WaitForSeconds(3.2f);
  // 4. lights on a car that was not speeding, within 20 m for 4 s
  plain=race.Drivers.First(d=>d&&d.GetComponent<AmbientVehicle>()&&!patrol.Speeders.Any(s=>s.driver==d)&&!cop.lastHit.ContainsKey(d.Car));before=cop.points;cop.siren=true;
  float n0=Time.time;while(cop.noViolations==0&&Time.time-n0<8){Behind(plain.Car,12);yield return new WaitForFixedUpdate();}cop.siren=false;
  Check(cop.noViolations==1&&cop.points==before-SpeedPatrol.NoViolationPenalty,$"lights on a car that was not speeding: {cop.line} ({before} > {cop.points})");yield return Late(()=>Shot4k("G-no-violation"));
  // the round's end: the results and the Top 10
  typeof(SpeedPatrol).GetProperty("Clock").SetValue(patrol,patrol.Limit-1);input.enabled=true;
  t0=Time.realtimeSinceStartup;while(flow.State!=RaceFlow.Stage.Results&&Time.realtimeSinceStartup-t0<30)yield return null;yield return Settle091();
  var top=SpeedPatrol.Top(3);
  Check(flow.State==RaceFlow.Stage.Results&&Details095.StartsWith("SCORE")&&top.Any(e=>e.name=="Dan"&&e.points==cop.points)&&Button90("rematch"),$"results: {Title095} / {Details095} || {CellsText094()}");yield return Late(()=>Shot4k("G-results-top10"));
  yield return Choose091("menu");yield return WaitScene095(()=>!SplitScreen.Active&&flow.State==RaceFlow.Stage.Ready);
  // two players: both cops, both radars
  Menus089.ResetPages();Menus089.Show();yield return Settle091();
  yield return Choose091("police");yield return Goto091("police-players");yield return Press091(GamepadButton.DpadRight);yield return Press090(pad2,GamepadButton.South);yield return Settle091();
  yield return Choose091("police-start");yield return WaitScene095(()=>SplitScreen.Active&&SpeedPatrol.Current&&SpeedPatrol.Current.Cops.Count==2);patrol=SpeedPatrol.Current;
  t0=Time.time;while(patrol.State==SpeedPatrol.Phase.Starting&&Time.time-t0<10)yield return null;
  var c1=patrol.CopOf(1);var c2=patrol.CopOf(2);var t=race.Drivers.Where(d=>d&&d.GetComponent<AmbientVehicle>()).Take(2).ToList();
  for(int i=0;i<10;i++){foreach(var (c,d) in new[]{(c1,t[0]),(c2,t[1])}){var f=Vector3.ProjectOnPlane(d.Car.transform.forward,Vector3.up).normalized;var at=d.Car.Body.position-f*30+Vector3.up*.6f;c.car.Body.position=at;c.car.Body.rotation=Quaternion.LookRotation(f);c.car.transform.SetPositionAndRotation(at,c.car.Body.rotation);c.car.Body.linearVelocity=d.Car.Body.linearVelocity;}yield return new WaitForFixedUpdate();}
  Check(c1.radar>=0&&c2.radar>=0&&SplitScreen.Race.P2Car.GetComponent<VehicleConfiguration>().profileId==VehicleProfile.Police.Id,$"two players: both in patrol cars; radars {c1.radar:0} mph and {c2.radar:0} mph");yield return Late(()=>Shot4k("G-two-radars"));
  flow.QuitSplit(false);yield return WaitScene095(()=>!SplitScreen.Active&&flow.State==RaceFlow.Stage.Ready);SplitScreen.Solo=true;SplitScreen.PoliceGame=SplitScreen.Game.CopRunner;}
}
}
#endif
