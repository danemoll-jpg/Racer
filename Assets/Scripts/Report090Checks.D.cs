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
// 0.90 Part D checks: split-screen on the runner's muted isolated save, with emulated controllers (InputSystem test devices).
public sealed partial class Report080Checks {
 partial void Split090(string[] a,ref IEnumerator run){
  run=a[0] switch{"splitsetup90"=>SplitSetup090(),"splitrace90"=>SplitRace090(int.Parse(a[1]),a[2],a[3]=="lr",int.Parse(a[4])),"splitkeys90"=>SplitKeys090(),"splitafter90"=>SplitAfter090(),_=>null};}
 Gamepad pad1,pad2;
 void Pads090(){if(pad1==null||!pad1.added)pad1=InputSystem.AddDevice<Gamepad>("Report090 pad 1");if(pad2==null||!pad2.added)pad2=InputSystem.AddDevice<Gamepad>("Report090 pad 2");}
 IEnumerator Press090(InputDevice d,GamepadButton b){InputSystem.QueueStateEvent((Gamepad)d,new GamepadState().WithButton(b));yield return null;yield return null;InputSystem.QueueStateEvent((Gamepad)d,new GamepadState());yield return null;yield return null;}
 IEnumerator SplitRunning090(){float t0=Time.realtimeSinceStartup;yield return null;
  while((flow==null||!flow.Started||LoadingScreen.Holding||flow.State!=RaceFlow.Stage.Racing)&&Time.realtimeSinceStartup-t0<120){yield return null;Bind();AudioListener.volume=0;}}

 // The setup screen: player 1 = the device that opened it, player 2 joins with Start on a second controller; AI driver
 // switch; every vehicle (no campaign lock), every course; START only when both are ready.
 IEnumerator SplitSetup090(){
  if(File.Exists(Path.Combine(saveDir,Campaign.File)))File.Delete(Path.Combine(saveDir,Campaign.File));
  yield return Load("StreetLoopGreybox");yield return Menu();Campaign.Load(saveDir);Pads090();
  SplitScreen.P1Device=null;SplitScreen.P2Device=null;SplitScreen.P2Ai=false;Menus089.ResetPages();Menus089.Show();yield return null;
  Check(Button90("split"),"main menu: SPLIT SCREEN entry");
  Menus089.OpenSplitSetup();SplitScreen.P1Device=pad1;Menus089.Show();yield return new WaitForSecondsRealtime(.3f);
  Check(Page90=="split"&&Button90("split-start")&&!Button90("split-start").interactable,"setup screen; START waits for player 2");
  yield return Late(()=>Shot("D-setup-waiting"));
  yield return Press090(pad2,GamepadButton.Start);yield return new WaitForSecondsRealtime(.3f);
  Check(SplitScreen.P2Device==pad2&&Button90("split-start").interactable,$"player 2 joined with Start on the second controller: {SplitScreen.DeviceName(SplitScreen.P2Device)}");
  yield return Press090(pad1,GamepadButton.Start);Check(SplitScreen.P1Device==pad1&&SplitScreen.P2Device==pad2,"player 1's own Start does not take player 2");
  Check(SplitScreen.Vehicles.Length==10&&!Campaign.Owns("tourer"),$"every vehicle offered ({SplitScreen.Vehicles.Length}; the mower once earned) although the campaign owns {Campaign.Current.owned.Count}");
  SplitScreen.P1Vehicle="tourer";SplitScreen.P2Vehicle="skyfin";SplitScreen.Course=6;Menus089.Show();yield return new WaitForSecondsRealtime(.3f);yield return Late(()=>Shot("D-setup-ready"));
  SplitScreen.P2Ai=true;Menus089.Show();yield return new WaitForSecondsRealtime(.2f);yield return Late(()=>Shot("D-setup-ai"));
  Note("setup rows: "+string.Join(" | ",Labels089().Where(l=>l.StartsWith("split"))));}

 // A full race: course, player 2 "ai" or "pad" (pad: player 2's vehicle driven by the race AI through pad 2's slot, i.e. the
 // human path with an autopilot), layout, laps. Player 1 by the race AI. Checks laps, results, the halves, shared radio,
 // pause from each side, a controller unplugged and replugged, stock vehicles, nothing written.
 IEnumerator SplitRace090(int course,string p2,bool leftRight,int laps){
  yield return Load("StreetLoopGreybox");yield return Menu();Pads090();
  // campaign upgrades on the Longroof GT must not reach split-screen
  Campaign.DebugUnlockAll();var tourer=VehicleProfile.Find("tourer");Campaign.DebugAddMoney(50000);for(int s=0;s<4;s++)Campaign.BuyUpgrade(tourer,s);
  var before=Protected089();before.Remove("exploration-map-woodstock-world-v2.json"); // map discovery is written in every race, as before
  flow.Save.Settings.splitLeftRight=leftRight;flow.Save.SaveSettings();
  SplitScreen.P1Device=pad1;SplitScreen.P2Device=pad2;SplitScreen.P2Ai=p2=="ai";SplitScreen.P1Vehicle="tourer";SplitScreen.P2Vehicle="atv";SplitScreen.Course=course;SplitScreen.Laps=laps;
  flow.StartSplit();yield return SplitRunning090();
  var split=SplitScreen.Race;var cams=FindObjectsByType<Camera>(FindObjectsSortMode.None).Where(c=>c.isActiveAndEnabled&&c.targetTexture==null).ToArray();
  Check(SplitScreen.Active&&split&&race.Racers.Count==2&&cams.Length>=2&&Scene==RacePlaylists.Scenes[course]&&WorldLook.Current.RaceTime==TimeOfDay.Day&&WorldLook.Current.RaceWeather==Weather.Clear&&!race.traffic,
   $"{Scene}: 2 racers ({race.Racers[0].Name} / {race.Racers[1].Name}), cameras [{string.Join(", ",cams.Select(c=>c.name+" "+c.rect))}], {WorldLook.Current.RaceTime}/{WorldLook.Current.RaceWeather}, traffic {race.traffic}");
  Check(Mathf.Abs(race.vehicle.topSpeed-tourer.Speed)<.01f&&Mathf.Abs(race.vehicle.acceleration-tourer.Acceleration)<.01f,$"player 1's Longroof GT stock in split-screen (campaign upgrades all level 3): top speed {race.vehicle.topSpeed:F1}, acceleration {race.vehicle.acceleration:F2}");
  // drive both: player 1 by the race AI; a human player 2 by the race AI on its own vehicle (the human path stays: input, reset, camera, HUD)
  var p1=race.vehicle;p1.GetComponent<VehicleInput>().enabled=false;var pilot1=p1.gameObject.AddComponent<RoadDriver>();pilot1.Initialize(race,p1,true,1,1);pilot1.Racer=race.Racers[0];
  RoadDriver pilot2=null;if(p2!="ai"){var c2=split.P2Car;pilot2=c2.gameObject.AddComponent<RoadDriver>();pilot2.Initialize(race,c2,true,1,1);pilot2.Racer=split.P2;}
  yield return new WaitForSeconds(4);yield return Late(()=>Shot($"D-race-{Scene}-{(leftRight?"left-right":"top-bottom")}"));
  // shared radio: pad 1 then pad 2 within half a second = one command; pad 2 again later = another
  int presses=flow.Radio.SharedPresses;yield return Press090(pad1,GamepadButton.DpadUp);yield return Press090(pad2,GamepadButton.DpadUp);int afterTwo=flow.Radio.SharedPresses;
  yield return new WaitForSecondsRealtime(.7f);yield return Press090(pad2,GamepadButton.DpadUp);
  Check(afterTwo==presses+1&&flow.Radio.SharedPresses==presses+2,$"one radio: pad 1 then pad 2 within 0.5 s = {afterTwo-presses} command; pad 2 later = {flow.Radio.SharedPresses-afterTwo} more");
  // pause from player 2; player 1's Back does nothing; player 2 resumes; then pause from player 1
  yield return Press090(pad2,GamepadButton.Start);bool paused2=flow.State==RaceFlow.Stage.Paused&&split.PausedBy==pad2;
  yield return Press090(pad1,GamepadButton.East);yield return Press090(pad1,GamepadButton.Start);bool still=flow.State==RaceFlow.Stage.Paused;
  yield return Late(()=>Shot("D-pause-player2"));
  yield return Press090(pad2,GamepadButton.Start);bool resumed=flow.State==RaceFlow.Stage.Racing;
  yield return Press090(pad1,GamepadButton.Start);bool paused1=flow.State==RaceFlow.Stage.Paused&&split.PausedBy==pad1;yield return Press090(pad1,GamepadButton.Start);
  Check(paused2&&still&&resumed&&paused1&&flow.State==RaceFlow.Stage.Racing,$"pause: from player 2 {paused2}; player 1's B / Start ignored {still}; player 2 resumed {resumed}; from player 1 {paused1}; resumed {flow.State}");
  // a controller unplugged and plugged in again
  if(p2!="ai"){InputSystem.RemoveDevice(pad2);yield return null;yield return null;bool held=flow.State==RaceFlow.Stage.Paused&&split.MissingPlayer==2&&!Button90("resume").interactable;
   yield return Late(()=>Shot("D-controller-missing"));
   pad2=InputSystem.AddDevice<Gamepad>("Report090 pad 2 again");yield return null;yield return null;bool back=split.MissingPlayer==0&&SplitScreen.P2Device==pad2;Menus089.Show();yield return null;
   bool canResume=Button90("resume")&&Button90("resume").interactable;flow.Resume();yield return null;
   Check(held&&back&&canResume&&flow.State==RaceFlow.Stage.Racing,$"player 2's controller unplugged: paused, Resume off {held}; plugged in again: player 2 back on it {back}, Resume on {canResume}, racing {flow.State}");}
  Time.timeScale=3;float t0=Time.time,r0=Time.realtimeSinceStartup;while(flow.State!=RaceFlow.Stage.Results&&Time.time-t0<600*laps&&Time.realtimeSinceStartup-r0<700){AudioListener.volume=0;yield return null;}Time.timeScale=1;yield return null;
  var order=race.Ordered(true);yield return Page089($"D-results-{Scene}");
  Check(flow.State==RaceFlow.Stage.Results&&race.Racers.All(r=>r.Progress.Finished&&r.Progress.LapTimes.Count==laps)&&Button90("rematch")&&Button90("change-setup"),
   $"results: {string.Join(" | ",order.Select((r,i)=>$"{i+1}. {r.Name} {(r.Progress.Finished?RaceHud.FormatTime(r.ClassifiedTime(race.Clock)):"DNF")} laps {r.Progress.LapTimes.Count} misses {r.Progress.MissedGates}"))}");
  var after=Protected089();var changed=before.Where(kv=>!after.TryGetValue(kv.Key,out var h)||h!=kv.Value).Select(kv=>kv.Key).ToArray();
  Check(changed.Length==0,$"no record, board, ghost, activity or acorn file written: {(changed.Length==0?"all "+before.Count+" identical":"CHANGED "+string.Join(", ",changed))}");
  if(pilot1)Destroy(pilot1);
  flow.QuitSplit(false);yield return null;
  Check(!SplitScreen.Active&&Camera.main.rect==new Rect(0,0,1,1)&&FindObjectsByType<Camera>(FindObjectsSortMode.None).Count(c=>c.name=="Player 2 camera")==0&&race.vehicle.GetComponent<VehicleInput>().Device==null,"back to the menu: one full-screen camera, player 2's camera and vehicle gone, player 1's input on every device again");}

 // Player 2 on the keyboard, driven by key presses (W and A / D from the road ahead) for a lap of Street Loop - Forward;
 // player 1 on a controller (the race AI).
 IEnumerator SplitKeys090(){
  // the batch editor has no focus: let keyboard input through for this check only (restored at the end)
  var bg=InputSystem.settings.backgroundBehavior;var ed=InputSystem.settings.editorInputBehaviorInPlayMode;InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
  yield return Load("StreetLoopGreybox");yield return Menu();Pads090();var kb=InputSystem.AddDevice<Keyboard>("Report090 keyboard");
  SplitScreen.P1Device=pad1;SplitScreen.P2Device=kb;SplitScreen.P2Ai=false;SplitScreen.P1Vehicle="original";SplitScreen.P2Vehicle="atv";SplitScreen.Course=0;SplitScreen.Laps=1;
  flow.Save.Settings.splitLeftRight=false;flow.Save.SaveSettings();flow.StartSplit();yield return SplitRunning090();
  var split=SplitScreen.Race;var car=split.P2Car;var p1=race.vehicle;p1.GetComponent<VehicleInput>().enabled=false;var pilot1=p1.gameObject.AddComponent<RoadDriver>();pilot1.Initialize(race,p1,true,1,1);pilot1.Racer=race.Racers[0];
  float t0=Time.time;int resets=0;System.Action counted=()=>resets++;car.GetComponent<VehicleRespawn>().Respawned+=counted;
  while(!split.P2.Progress.Finished&&Time.time-t0<420&&flow.State==RaceFlow.Stage.Racing){AudioListener.volume=0;
   var p=car.Body.position;float s=race.road.Project(p,out _);var to=race.road.At(s+14,out _)-p;to.y=0;float ang=Vector3.SignedAngle(Vector3.ProjectOnPlane(car.transform.forward,Vector3.up),to,Vector3.up);
   var keys=new List<Key>{Key.W};if(ang<-6)keys.Add(Key.A);if(ang>6)keys.Add(Key.D);if(car.ForwardSpeed>30&&Mathf.Abs(ang)>25){keys.Remove(Key.W);keys.Add(Key.S);}
   if(car.ForwardSpeed<1&&Time.time-t0>20&&(int)(Time.time*2)%40==0)keys.Add(Key.R);
   InputSystem.QueueStateEvent(kb,new KeyboardState(keys.ToArray()));yield return null;}
  InputSystem.QueueStateEvent(kb,new KeyboardState());car.GetComponent<VehicleRespawn>().Respawned-=counted;
  Check(split.P2.Progress.Finished||split.P2.Progress.CompletedLaps>=1,$"player 2 on the keyboard drove the lap: finished {split.P2.Progress.Finished} in {(split.P2.Progress.Finished?RaceHud.FormatTime(split.P2.ClassifiedTime(race.Clock)):"-")}, missed gates {split.P2.Progress.MissedGates}, resets {resets}; player 1's AI meanwhile at lap {race.Racers[0].Progress.CompletedLaps}");
  yield return Late(()=>Shot("D-keyboard-player2"));
  flow.Pause();yield return null;flow.QuitRace();yield return null;InputSystem.RemoveDevice(kb);InputSystem.settings.backgroundBehavior=bg;InputSystem.settings.editorInputBehaviorInPlayMode=ed;}

 // After split-screen: one normal single-player race (1 lap, solo, by the race AI) and one campaign event start as before.
 IEnumerator SplitAfter090(){
  yield return Load("StreetLoopGreybox");yield return Menu();
  race.opponents=true;race.laps=1;race.traffic=true;flow.StartRace();float t0=Time.realtimeSinceStartup;while(flow.State!=RaceFlow.Stage.Racing&&Time.realtimeSinceStartup-t0<60){AudioListener.volume=0;yield return null;}
  var pilot=Pilot089(1);yield return Results089(400,3);Unpilot089(pilot);if(flow.State==RaceFlow.Stage.Racing&&race.Progress.Finished){race.FinalizeUnfinishedAi();yield return Results089(10);}
  Check(flow.State==RaceFlow.Stage.Results&&race.Racers.Count==4&&Camera.main.rect==new Rect(0,0,1,1)&&!SplitScreen.Active&&race.Progress.Finished,$"single-player race after split-screen: {race.Racers.Count} racers, finished {race.Progress.Finished}, one camera {Camera.main.rect}");
  yield return Page089("D-single-player-results");flow.QuitRace();yield return null;
  if(File.Exists(Path.Combine(saveDir,Campaign.File)))File.Delete(Path.Combine(saveDir,Campaign.File));Campaign.Load(saveDir);
  flow.StartCampaignEvent(CampaignData.Events[0],"atv");yield return EventRunning089();
  Check(CampaignRun.Active==CampaignData.Events[0]&&race.Racers.Count==4&&!SplitScreen.Active,"a campaign event starts as before");
  flow.Pause();yield return null;flow.QuitRace();yield return null;}
}}
#endif
