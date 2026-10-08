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
// 0.91 controller-only walk: every screen added in 0.89 and 0.90 worked with the emulated controller alone (D-pad, A, B,
// LB / RB, Start), plus split-screen with a controller as player 1 and The Opening Jump on a copy of Dan's save.
public sealed partial class Report080Checks {
 partial void Cases091(string[] a,ref IEnumerator run){
  run=a[0] switch{"pausediag91"=>PauseDiag091(),"walk91"=>Walk091(),"campaignwalk91"=>CampaignWalk091(),"newplayer91"=>NewPlayer091(),"splitpad91"=>SplitPad091(),"jumpend91"=>JumpEnd091(a[1],a[2],a.Length>3&&a[3]=="menu"),"roam91"=>Roam091(),"runup91"=>RunUp091(),_=>Run095(a)};}
 readonly List<string> walked=new();
 GameObject Sel91=>EventSystem.current?EventSystem.current.currentSelectedGameObject:null;
 string Sel91Name=>Sel91?Sel91.name:"(nothing)";
 // the menu's visible, usable rows, as drawn: top to bottom, then left to right
 List<UnityEngine.UI.Button> Rows091(){Canvas.ForceUpdateCanvases();var card=Menus089.transform;var menuCard=(RectTransform)typeof(RaceMenus).GetField("card",Any).GetValue(Menus089);
  return FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None).Where(b=>b.gameObject.activeInHierarchy&&b.interactable&&b.transform.IsChildOf(menuCard)&&b.navigation.mode!=UnityEngine.UI.Navigation.Mode.None).OrderByDescending(b=>Mathf.Round(b.transform.position.y)).ThenBy(b=>b.transform.position.x).ToList();}
 IEnumerator Settle091(){yield return null;yield return null;yield return new WaitForSecondsRealtime(.15f);}
 IEnumerator Press091(GamepadButton b){if(b==GamepadButton.Start)yield return new WaitForSecondsRealtime(.5f);yield return Tap091(b);yield return Settle091();}
 // Down through every row (rows + 1 presses): every row reached, focus never lost; ordered = each press moves to the next
 // row as drawn (wrapping at the end). Then B must go back one level (back = the page expected after it; null = skip).
 IEnumerator Walk091(string label,bool ordered,Func<bool> back=null,bool lateral=false){
  yield return Settle091();var rows=Rows091();var seen=new List<GameObject>();bool lost=false;
  string missed=string.Join(", ",Menus089.UnreachableRows());
  if(!Sel91)lost=true;
  for(int i=0;i<=rows.Count;i++){if(Sel91)seen.Add(Sel91);else lost=true;yield return Press091(GamepadButton.DpadDown);}
  if(lateral){var sw=rows.Where(r=>!seen.Contains(r.gameObject)).ToList();var start=sw.FirstOrDefault()?.transform.parent;var first=rows.FirstOrDefault(r=>r.transform.parent==start&&seen.Contains(r.gameObject));
   if(first){EventSystem.current.SetSelectedGameObject(first.gameObject);for(int i=0;i<12;i++){yield return Press091(GamepadButton.DpadRight);if(Sel91)seen.Add(Sel91);}}}
  var unreached=rows.Where(r=>!seen.Contains(r.gameObject)).Select(r=>r.name).ToList();
  bool order=true;if(ordered&&rows.Count>1){var idx=seen.Take(rows.Count).Select(g=>rows.FindIndex(r=>r.gameObject==g)).ToList();for(int i=1;i<idx.Count;i++)if(idx[i]!=(idx[i-1]+1)%rows.Count)order=false;}
  walked.Add(label);
  Check(unreached.Count==0&&!lost&&order&&missed.Length==0,$"{label}: {rows.Count} rows [{string.Join(", ",rows.Select(r=>r.name))}]; reached all {unreached.Count==0}{(unreached.Count>0?" MISSED "+string.Join(",",unreached):"")}, focus kept {!lost}{(ordered?$", in drawn order {order} ({string.Join(">",seen.Take(rows.Count).Select(g=>g.name))})":"")}, structural check {(missed.Length==0?"clean":"UNREACHABLE "+missed)}");
  yield return Late(()=>Shot("walk-"+label.Replace(' ','-').Replace('/','-').Replace(':','-')));
  if(back!=null){yield return Press091(GamepadButton.East);yield return Settle091();Check(back(),$"{label}: B went back one level (now {flow.State} / '{Page90}')");}}
 // Moves the focus down (D-pad only) to the named row; false when it was never reached.
 IEnumerator Goto091(string name,int limit=60){for(int i=0;i<limit&&Sel91Name!=name;i++)yield return Press091(GamepadButton.DpadDown);if(Sel91Name!=name){Check(false,$"D-pad could not reach '{name}' (focus on {Sel91Name}, page '{Page90}')");}}
 IEnumerator Choose091(string name){yield return Goto091(name);yield return Press091(GamepadButton.A);yield return Settle091();}
 bool Ready91(string p)=>flow.State==RaceFlow.Stage.Ready&&Page90==p;
 IEnumerator Start091(){Pad091();yield return Load("StreetLoopGreybox");yield return Menu();yield return DanSave091();Menus089.ResetPages();Menus089.Show();yield return Settle091();
  yield return Press091(GamepadButton.DpadDown);yield return Press091(GamepadButton.DpadUp);} // the pad becomes the menu device

 // The main menu, Settings, the Garage and Split Screen's entry; then the campaign screens.
 IEnumerator Walk091(){
  yield return Start091();
  yield return Walk091("main menu",true);
  Check(Rows091().Any(r=>r.name=="campaign")&&Rows091().Any(r=>r.name=="split"),"main menu shows CAMPAIGN and SPLIT SCREEN");
  yield return Choose091("campaign");Check(Ready91("campaign"),$"A on CAMPAIGN opened the campaign screen ('{Page90}')");
  yield return Press091(GamepadButton.East);Check(Ready91(""),"B from the campaign screen: main menu");
  yield return Choose091("split");Check(Ready91("split")&&SplitScreen.P1Device==pad91,$"A on SPLIT SCREEN opened the setup with player 1 = the controller ({SplitScreen.DeviceName(SplitScreen.P1Device)})");
  yield return Walk091("split-screen setup",true,()=>Ready91(""));
  yield return Choose091("settings");Check(flow.State==RaceFlow.Stage.Settings,"A on SETTINGS");
  yield return Walk091("settings / gameplay (testing switch, hints rows)",true);
  string hints=Button90("hints")?.GetComponentInChildren<UnityEngine.UI.Text>(true).text;yield return Choose091("hints");string hints2=Button90("hints")?.GetComponentInChildren<UnityEngine.UI.Text>(true).text;yield return Press091(GamepadButton.A);
  Check(hints!=hints2&&Button90("hints")?.GetComponentInChildren<UnityEngine.UI.Text>(true).text==hints,$"Hints row toggles with A ({hints} > {hints2} > back)");
  yield return Press091(GamepadButton.RightShoulder);bool rb=Page90=="settings-audio";yield return Press091(GamepadButton.LeftShoulder);
  Check(rb&&Page90=="settings-gameplay","RB / LB change the Settings tab");
  yield return Press091(GamepadButton.East);Check(Ready91(""),"B from Settings: main menu");
  yield return Choose091("garage");Check(flow.State==RaceFlow.Stage.Garage&&Page90=="","A on GARAGE opened the garage");
  yield return Walk091("garage (vehicles, colours, Model, Rider, Shop, Done)",false,()=>Ready91(""),true);
  yield return CampaignWalk091b();}

 IEnumerator CampaignWalk091(){yield return Start091();yield return CampaignWalk091b();}
 IEnumerator CampaignWalk091b(){
  yield return Choose091("campaign");
  yield return Walk091("campaign screen",false);
  // a chapter row: right goes to its events
  yield return Choose091("chapter-1");Check(Sel91Name.StartsWith("cev-c1"),$"A on chapter 1 shows its events with the focus on the first ({Sel91Name})");
  yield return Goto091("ccup-cup-street");yield return Press091(GamepadButton.A);Check(Ready91("campaign-cup"),"A on the Street Cup row: its page");
  yield return Walk091("championship page (vehicle, START CHAMPIONSHIP, Back)",true,()=>Ready91("campaign"));
  yield return Choose091("chapter-2");yield return Goto091("cev-c2-opening-jump");yield return Press091(GamepadButton.A);
  Check(Ready91("campaign-event"),"A on The Opening Jump: its event page");
  string v1=Button90("event-vehicle")?.GetComponentInChildren<UnityEngine.UI.Text>(true).text;yield return Goto091("event-vehicle");yield return Press091(GamepadButton.DpadRight);
  string v2=Button90("event-vehicle")?.GetComponentInChildren<UnityEngine.UI.Text>(true).text;Check(v1!=v2,$"event page: right on the vehicle row changes it ({v1} > {v2})");yield return Press091(GamepadButton.DpadLeft);
  yield return Walk091("event page (vehicle, START EVENT, Back)",true,()=>Ready91("campaign"));
  yield return Choose091("shop");Check(flow.State==RaceFlow.Stage.Garage&&Page90=="shop","A on Shop: the Shop");
  string s1=Button90("shop-vehicle")?.GetComponentInChildren<UnityEngine.UI.Text>(true).text;yield return Goto091("shop-vehicle");yield return Press091(GamepadButton.DpadRight);
  Check(s1!=Button90("shop-vehicle")?.GetComponentInChildren<UnityEngine.UI.Text>(true).text,"Shop: right on the vehicle row steps the vehicle");
  // an owned vehicle: its upgrades can be bought (Dan has money); open the confirmation and cancel it with B
  for(int i=0;i<12&&!(Button90("upgrade-0")&&Button90("upgrade-0").interactable);i++){yield return Goto091("shop-vehicle");yield return Press091(GamepadButton.DpadRight);}
  yield return Walk091("Shop (vehicle, buy, upgrades, Back)",true);
  yield return Choose091("upgrade-0");Check(Button90("confirm")&&Button90("cancel"),"A on an upgrade: the confirmation");
  yield return Walk091("upgrade confirmation",true,()=>flow.State==RaceFlow.Stage.Garage&&Page90=="shop");
  yield return Press091(GamepadButton.East);Check(Ready91("campaign"),$"B from the Shop (opened from the campaign): campaign screen ({flow.State} / '{Page90}')");
  yield return Choose091("new-campaign");yield return Walk091("New Campaign confirmation",true,()=>Ready91("campaign"));
  Check(Campaign.Current.money==15770,"cancelled: Dan's campaign copy untouched");
  yield return Choose091("continue");Check(Ready91("campaign-event")&&(string)Field90("campaignEvent")=="c2-opening-jump","Continue: the next event's page (The Opening Jump)");
  yield return Press091(GamepadButton.East);yield return Press091(GamepadButton.East);Check(Ready91(""),"B, B: back to the main menu");
  // the garage opened from the main menu after the Shop shows the garage, not the Shop
  yield return Choose091("garage");Check(flow.State==RaceFlow.Stage.Garage&&Page90=="",$"GARAGE after visiting the Shop opens the garage ('{Page90}')");yield return Press091(GamepadButton.East);
  // a championship round: start with A, leave it from the pause menu (counts as did not finish), standings, resume page
  yield return Choose091("campaign");yield return Choose091("chapter-1");yield return Goto091("ccup-cup-street");yield return Press091(GamepadButton.A);
  yield return Choose091("cup-start");yield return EventRunning089();Check(CampaignRun.Cup!=null&&flow.State==RaceFlow.Stage.Racing,"A on START CHAMPIONSHIP: round 1 racing");
  yield return Press091(GamepadButton.Start);Check(flow.State==RaceFlow.Stage.Paused,"Start: paused");
  yield return Walk091("championship race pause menu",true);
  yield return Press091(GamepadButton.Start);Check(flow.State==RaceFlow.Stage.Racing,"Start again: racing");
  // round 1 finished by the race AI (time 3x), then its result page worked with the controller
  var pilot=Pilot089(1);yield return Results089(600,3);Unpilot089(pilot);if(flow.State==RaceFlow.Stage.Racing&&race.Progress.Finished){race.FinalizeUnfinishedAi();yield return Results089(10);}
  Check(flow.State==RaceFlow.Stage.Results&&Button90("next-race"),"round 1 finished: its result with the standings and Next race");
  yield return Walk091("championship round result and standings",false);
  yield return Choose091("next-race");yield return EventRunning089();Check(CampaignRun.Cup!=null&&CampaignRun.Active.Round==1,"A on NEXT RACE: round 2 racing");
  yield return Press091(GamepadButton.Start);yield return Choose091("return");yield return Choose091("confirm");yield return Settle091();
  Check(Ready91("campaign"),"round 2 left from the pause menu (did not finish): the campaign screen");
  yield return Goto091("ccup-cup-street");yield return Press091(GamepadButton.A);
  yield return Walk091("championship page in progress (Resume, standings, Restart, Abandon)",true);
  yield return Choose091("cup-abandon");yield return Walk091("Abandon confirmation",true,()=>Ready91("campaign-cup"));
  yield return Choose091("cup-restart");yield return Walk091("Restart confirmation",true,()=>Ready91("campaign-cup"));
  yield return Press091(GamepadButton.East);yield return Press091(GamepadButton.East);Check(Ready91(""),"B, B: main menu");
  Note("screens walked: "+string.Join(" | ",walked));}

 // Why Start does not pause a race in the walk: the state around one press.
 IEnumerator PauseDiag091(){
  Pad091();yield return Load("StreetLoopGreybox");yield return Menu();yield return DanSave091();flow.StartCampaignEvent(CampaignData.Events[0],"original");yield return EventRunning089();
  yield return new WaitForSecondsRealtime(1);var m=flow.PauseAction;
  Note($"  before: state {flow.State}, blocked {MenuInput.Blocked}, uiBlocked {MenuInput.UiBlocked}, holding {LoadingScreen.Holding}, controls card {flow.ControlsCard}, menu enabled {m.enabled}, controls [{string.Join(",",m.controls.Select(c=>c.path))}], pad added {pad91.added} enabled {pad91.enabled}, devices [{string.Join(",",InputSystem.devices.Select(d=>d.name+(d.enabled?"":"(off)")))}]");
  InputSystem.QueueStateEvent(pad91,new GamepadState().WithButton(GamepadButton.Start));bool seen=false;for(int i=0;i<3;i++){yield return null;seen|=m.WasPressedThisFrame()||pad91.startButton.isPressed;Note($"  frame {i}: start pressed {pad91.startButton.isPressed}, action pressed {m.IsPressed()}, state {flow.State}, blocked {MenuInput.Blocked}");}
  InputSystem.QueueStateEvent(pad91,new GamepadState());yield return Settle091();Note($"  after: state {flow.State}");
  if(flow.State==RaceFlow.Stage.Paused){flow.QuitRace();}else{flow.Pause();yield return null;flow.QuitRace();}yield return null;}
 // In a jump event Y (reset) puts the vehicle back at the run-up, facing the jump, and the event goes on.
 IEnumerator RunUp091(){
  Pad091();yield return Load("StreetLoopGreybox");yield return Menu();yield return DanSave091();var e=CampaignData.Find("c2-opening-jump");flow.StartCampaignEvent(e,"atv");yield return EventRunning089();
  var start=race.vehicle.Body.position;float t0=Time.time;while(Time.time-t0<6){Hold091(1,0,0);yield return null;}Hold091(0,0,0);float away=Vector3.Distance(start,race.vehicle.Body.position);
  yield return new WaitForSecondsRealtime(.5f);yield return Press091(GamepadButton.Y);yield return new WaitForSecondsRealtime(.5f);
  float back=Vector3.Distance(start,race.vehicle.Body.position);
  Check(away>30&&back<3&&flow.State==RaceFlow.Stage.Racing&&!CampaignRun.Done,$"Y in the jump event: {away:F0} m down the road, then back at the run-up ({back:F1} m from the start), event still running");
  flow.Pause();yield return null;flow.QuitRace();yield return null;}
 // A new player: the welcome panel, Start the campaign, the first event's page, the controls card (one press).
 IEnumerator NewPlayer091(){
  Pad091();yield return Load("StreetLoopGreybox");yield return Menu();if(File.Exists(Path.Combine(saveDir,Campaign.File)))File.Delete(Path.Combine(saveDir,Campaign.File));Campaign.Load(saveDir);
  flow.Save.Settings.hints=true;flow.Save.SaveSettings();Hints.Reset();Menus089.OpenWelcome();yield return Settle091();yield return Press091(GamepadButton.DpadDown);yield return Press091(GamepadButton.DpadUp);
  yield return Walk091("welcome panel",true);
  yield return Choose091("welcome-campaign");Check(Ready91("campaign-event"),"START THE CAMPAIGN: chapter 1's first event page");
  yield return Choose091("start-event");float t0=Time.realtimeSinceStartup;while(!(flow.ControlsCard&&flow.State==RaceFlow.Stage.Countdown)&&Time.realtimeSinceStartup-t0<90){yield return null;Bind();AudioListener.volume=0;}
  Check(flow.ControlsCard,"the controls card holds the first event's countdown");yield return new WaitForSecondsRealtime(.6f);yield return Late(()=>Shot("walk-controls-card"));
  yield return Press091(GamepadButton.A);yield return new WaitForSecondsRealtime(.3f);Check(!flow.ControlsCard,"one press (A) dismissed it");
  yield return EventRunning089();yield return Press091(GamepadButton.Start);
  yield return Walk091("campaign race pause menu",true);
  yield return Choose091("return");yield return Choose091("confirm");yield return Settle091();Check(Ready91("campaign"),$"END RACE from the pause menu: campaign screen ('{Page90}')");
  flow.Save.Settings.hints=false;flow.Save.SaveSettings();}

 // Free Roam: its pause menu (CAMPAIGN and SPLIT SCREEN there too) with the controller.
 IEnumerator Roam091(){
  yield return Start091();yield return Choose091("roam");yield return Choose091("explore");
  float t0=Time.realtimeSinceStartup;while((flow==null||!flow.Started||LoadingScreen.Holding||flow.State!=RaceFlow.Stage.Racing)&&Time.realtimeSinceStartup-t0<120){yield return null;Bind();AudioListener.volume=0;}
  yield return new WaitForSecondsRealtime(1);yield return Press091(GamepadButton.Start);Check(flow.State==RaceFlow.Stage.Ready&&flow.RoamMenu,"Start in Free Roam: its menu");
  yield return Walk091("Free Roam pause menu",true);
  yield return Choose091("campaign");Check(Ready91("campaign"),"Free Roam menu: A on CAMPAIGN opens the campaign screen");
  yield return Press091(GamepadButton.East);yield return Choose091("split");Check(Ready91("split")&&SplitScreen.P1Device==pad91,"Free Roam menu: A on SPLIT SCREEN opens the setup (player 1 the controller)");
  yield return Press091(GamepadButton.East);yield return Choose091("resume");Check(flow.State==RaceFlow.Stage.Racing,"RESUME DRIVING");}

 // Split-screen with the controller as player 1: the AI as player 2 (a lap, finish, rematch, quit); then the keyboard as
 // player 2 (join with Enter, its own choices, each device drives its own vehicle, pause from either).
 IEnumerator SplitPad091(){
  yield return Start091();var kb=InputSystem.AddDevice<Keyboard>("Report091 keyboard");
  SplitScreen.P1Device=null;SplitScreen.P2Device=null;SplitScreen.P2Ai=false;SplitScreen.Laps=1;SplitScreen.Course=0;
  yield return Choose091("split");Check(SplitScreen.P1Device==pad91&&SplitScreen.P2Ai&&Button90("split-start").interactable,$"opened with the controller: player 1 {SplitScreen.DeviceName(SplitScreen.P1Device)}, player 2 the AI by default, START ready");
  // laps 1 with the D-pad on the Laps row
  yield return Goto091("split-laps");for(int i=0;i<5;i++)yield return Press091(GamepadButton.DpadLeft);Check(SplitScreen.Laps==1,"Laps row with the D-pad: 1");
  yield return Late(()=>Shot("B-setup-controller-ai"));
  yield return Choose091("split-start");yield return SplitRunning090();Check(SplitScreen.Active&&race.vehicle.GetComponent<VehicleInput>().Device==pad91,"race: player 1's vehicle reads the controller");
  // drive the lap with the controller (stick and triggers)
  var car=race.vehicle;float t0=Time.time;while(!race.Racers[0].Progress.Finished&&Time.time-t0<400&&flow.State==RaceFlow.Stage.Racing){AudioListener.volume=0;var p=car.Body.position;float s=race.road.Project(p,out _);var to=race.road.At(s+14,out _)-p;to.y=0;
   float ang=Vector3.SignedAngle(Vector3.ProjectOnPlane(car.transform.forward,Vector3.up),to,Vector3.up);float v=car.ForwardSpeed;bool corner=Mathf.Abs(ang)>20;Hold091(v<(corner?16:30)?1:0,v>(corner?20:34)?.6f:0,ang/22f);yield return null;}
  Hold091(0,0,0);Check(race.Racers[0].Progress.Finished,$"player 1 drove the lap with the controller: {(race.Racers[0].Progress.Finished?RaceHud.FormatTime(race.Racers[0].ClassifiedTime(race.Clock)):"not finished")}, missed gates {race.Racers[0].Progress.MissedGates}");
  yield return Results089(200,3);Check(flow.State==RaceFlow.Stage.Results,"split results");
  yield return Walk091("split-screen results",true);
  yield return Choose091("rematch");yield return SplitRunning090();Check(flow.State==RaceFlow.Stage.Racing&&SplitScreen.Active,"REMATCH with A: racing again");
  yield return Press091(GamepadButton.Start);yield return Walk091("split-screen pause menu",true);
  yield return Choose091("return");yield return Choose091("confirm");yield return Settle091();Check(Ready91("")&&!SplitScreen.Active,"END RACE with A: main menu");
  // the keyboard joins as player 2 with Enter; its arrows change only player 2's vehicle / colour
  yield return Choose091("split");InputSystem.QueueStateEvent(kb,new KeyboardState(Key.Enter));yield return null;yield return null;InputSystem.QueueStateEvent(kb,new KeyboardState());yield return Settle091();
  Check(SplitScreen.P2Device==kb&&!SplitScreen.P2Ai&&SplitScreen.P1Device==pad91,$"Enter on the keyboard: player 2 = {SplitScreen.DeviceName(SplitScreen.P2Device)}");
  string v2=SplitScreen.P2Vehicle,sel=Sel91Name;InputSystem.QueueStateEvent(kb,new KeyboardState(Key.RightArrow));yield return null;yield return null;InputSystem.QueueStateEvent(kb,new KeyboardState());yield return Settle091();
  Check(SplitScreen.P2Vehicle!=v2&&Sel91Name==sel,$"player 2's right arrow: its vehicle {v2} > {SplitScreen.P2Vehicle}; the menu focus stays ({Sel91Name})");
  InputSystem.QueueStateEvent(kb,new KeyboardState(Key.Escape));yield return null;yield return null;InputSystem.QueueStateEvent(kb,new KeyboardState());yield return Settle091();
  Check(Ready91("split"),"player 2's Esc does not leave the setup (player 1's device works the screen)");
  // a device row cycles: player 1 to the keyboard is not offered while player 2 holds it; player 2 to the AI and back
  yield return Goto091("split-p2");yield return Press091(GamepadButton.DpadLeft);bool ai=SplitScreen.P2Ai;yield return Press091(GamepadButton.DpadRight);
  Check(ai&&SplitScreen.P2Device==kb&&!SplitScreen.P2Ai,"player 2's device row: left to the AI driver, right back to the keyboard");
  yield return Late(()=>Shot("B-setup-controller-keyboard"));
  yield return Choose091("split-start");yield return SplitRunning090();var split=SplitScreen.Race;
  Check(race.vehicle.GetComponent<VehicleInput>().Device==pad91&&split.P2Car.GetComponent<VehicleInput>().Device==kb,"race: player 1 reads the controller, player 2 the keyboard");
  var p1=race.vehicle;var p2=split.P2Car;var a1=p1.Body.position;var a2=p2.Body.position;float tt=Time.time;while(Time.time-tt<2.5f){Hold091(1,0,0);yield return null;}Hold091(0,0,0);
  bool onlyP1=Vector3.Distance(a1,p1.Body.position)>5&&Vector3.Distance(a2,p2.Body.position)<1.5f;
  var b2=p2.Body.position;tt=Time.time;while(Time.time-tt<2.5f){InputSystem.QueueStateEvent(kb,new KeyboardState(Key.W));yield return null;}InputSystem.QueueStateEvent(kb,new KeyboardState());
  Check(onlyP1&&Vector3.Distance(b2,p2.Body.position)>5,$"the controller's trigger moved only player 1; the keyboard's W moved player 2 ({Vector3.Distance(b2,p2.Body.position):F1} m)");
  InputSystem.QueueStateEvent(kb,new KeyboardState(Key.Enter));yield return null;yield return null;InputSystem.QueueStateEvent(kb,new KeyboardState());yield return Settle091();
  bool paused=flow.State==RaceFlow.Stage.Paused;yield return Press091(GamepadButton.DpadDown);bool padMoves=Sel91Name!="resume";
  Check(paused&&padMoves,$"paused from the keyboard (player 2); the controller (player 1) works the pause menu ({Sel91Name})");
  yield return Choose091("return");yield return Choose091("confirm");yield return Settle091();Check(Ready91("")&&!SplitScreen.Active,"END RACE with the controller: main menu");
  InputSystem.RemoveDevice(kb);}

 // The Opening Jump on a copy of Dan's save: started with the controller (menu) or directly, taken flat out with the
 // controller until a scored jump, then Pause > END EVENT with the controller; passed, medal shown, next event open.
 IEnumerator JumpEnd091(string id,string vehicle,bool menu){
  var e=CampaignData.Find(id);
  if(menu){yield return Start091();yield return Choose091("campaign");yield return Choose091("chapter-2");yield return Goto091("cev-"+id);yield return Press091(GamepadButton.A);
   for(int i=0;i<12&&(string)Field90("campaignVehicle")!=vehicle;i++){yield return Goto091("event-vehicle");yield return Press091(GamepadButton.DpadRight);}
   yield return Choose091("start-event");}
  else{Pad091();yield return Load("StreetLoopGreybox");yield return Menu();yield return DanSave091();flow.StartCampaignEvent(e,vehicle);}
  yield return EventRunning089();Check(CampaignRun.Vehicle==vehicle,$"{id} running in the {VehicleProfile.Find(vehicle).Name}");
  var flights=new List<string>();bool done=false;var watch=flow.GetComponent<CampaignTrapWatch>();string hud=null;
  var c=StartCoroutine(Wrap089(FlatOut091(e.TimeLimit+2,flights),()=>done=true));float r0=Time.realtimeSinceStartup;
  while(!done&&flow.State==RaceFlow.Stage.Racing&&!(watch&&watch.Best>0&&flow.Activities.Feedback!=null&&flow.Activities.Feedback.Contains("SCORED"))&&Time.realtimeSinceStartup-r0<e.TimeLimit*3+30){if(watch&&watch.Hud!=null)hud=watch.Hud;yield return null;}
  yield return new WaitForSecondsRealtime(.4f);yield return Late(()=>Shot($"C-jump-{vehicle}-scored"));string shown=race.Flow.Activities.Hud;
  if(!done){StopCoroutine(c);Hold091(0,0,0);}
  foreach(var f in flights)Note("  "+f);
  Check(watch&&watch.Best>0&&flow.State==RaceFlow.Stage.Racing,$"flat out: scored {(watch?Campaign.Measure(e,watch.Best):"-")} and the event goes on; HUD: {shown?.Replace("\n"," / ")}");
  yield return Press091(GamepadButton.Start);yield return Choose091("end-event");yield return Results089(10);
  var o=CampaignRun.Last;Check(flow.State==RaceFlow.Stage.Results&&o!=null&&o.Passed&&o.Medal>0,$"Pause > END EVENT with A: {(o==null?"no result":o.Headline)}, passed {o?.Passed}");
  yield return Late(()=>Shot($"C-jump-{vehicle}-results"));
  var next=CampaignData.Events.SkipWhile(x=>x!=e).Skip(1).FirstOrDefault();Check(next!=null&&Campaign.Available(next),$"next event open: {next?.Name}");
  yield return Walk091($"jump event results ({vehicle})",true);
  yield return Choose091("campaign-continue");Check(Ready91("campaign"),"CONTINUE: the campaign screen");}
}
}
#endif
