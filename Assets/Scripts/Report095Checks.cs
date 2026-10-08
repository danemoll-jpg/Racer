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

namespace Racer {
// 0.95 targeted checks, added to the 0.80 runner (muted; the editor starts on a copy of Dan's save; menus by emulated
// controller only):
//  police95   main menu > POLICE CHASE: the setup (walked with the D-pad), from Free Roam (asks once), a solo full-screen
//             chase as the cop to a catch, the results, back to Free Roam; a two-player start still swaps
//  names95    a campaign event (grid card, gap line, tags), a quick race (none), a split-screen race with two AI rivals
//             (only the other player)
//  lake95     the Free Roam lake from Dan's BUG-004 position day and night; a vehicle driven in and out; ice in Snow
//  holes95    a shot at each of the five reported positions; a reset at BUG-002 and BUG-003; the floating-tree check
//  acorns95   on an isolated save: a pickup banner, the 24th (the unlock panel); a copy of Dan's save: the missed panel once
public sealed partial class Report080Checks {
 IEnumerator Run095(string[] a)=>a[0] switch{"police95"=>Police095(),"names95"=>Names095(),"lake95"=>Lake095(),"holes95"=>Holes095(),"acorns95"=>Acorns095(),"missed95"=>Missed095(),_=>Run094(a)};
 IEnumerator DanCopy095(){
  var copy=Path.Combine(Environment.GetEnvironmentVariable("PROBE_OUT"),"save-copy");
  foreach(var f in Directory.GetFiles(copy))File.Copy(f,Path.Combine(saveDir,Path.GetFileName(f)),true);
  yield return Load("StreetLoopGreybox");yield return Menu();Bind();flow.UseValidationSave(saveDir);UnlockNotice.Pending.Clear();UnlockNotice.Showing=null;flow.Save.Settings.unlocksSeen=new[]{UnlockNotice.AcornMower};flow.Save.SaveSettings();
  Pad091();Menus089.ResetPages();Menus089.Show();yield return Settle091();yield return Press091(GamepadButton.DpadDown);yield return Press091(GamepadButton.DpadUp);}
 string Details095=>((UnityEngine.UI.Text)typeof(RaceMenus).GetField("details",Any).GetValue(Menus089)).text.Replace("\n"," / ");
 string Title095=>((UnityEngine.UI.Text)typeof(RaceMenus).GetField("title",Any).GetValue(Menus089)).text;
 IEnumerator WaitScene095(Func<bool> done,float seconds=90){float t0=Time.realtimeSinceStartup;while(Time.realtimeSinceStartup-t0<seconds&&(!done()||flow==null||!flow.Started||LoadingScreen.Holding)){yield return null;Bind();AudioListener.volume=0;}yield return Settle091();}
 IEnumerator Police095(){
  yield return DanCopy095();Pads090();SplitScreen.Solo=true;SplitScreen.P2Device=null;
  // the main menu: POLICE CHASE after SPLIT SCREEN; the one-time hint
  var rows=Rows091().Select(r=>r.name).ToList();int si=rows.IndexOf("split"),pi=rows.IndexOf("police");
  Check(pi==si+1,$"main menu rows: [{string.Join(", ",rows)}]; hint now: \"{Hints.Text}\" (seen {Hints.Seen("police-new")})");yield return Late(()=>Shot4k("B-main-menu"));
  yield return Choose091("police");
  Check(Page90=="police"&&Button90("police-start")&&Button90("police-players"),$"POLICE CHASE opens its setup: {Title095} / {Details095}");
  yield return Walk091("police setup (1 player)",true,()=>Page90=="");
  yield return Choose091("roam");Check(Page90=="roam"&&Button90("police"),$"the Free Roam page: rows [{string.Join(", ",Rows091().Select(r=>r.name))}]");yield return Press091(GamepadButton.East);
  yield return Choose091("police");
  Check(Text(Button90("police-role")).Contains("Runner: coming later")&&Text(Button90("police-players")).Contains("1"),$"one player: {Text(Button90("police-players"))} / {Text(Button90("police-role"))}");
  var strip=GameObject.Find("Police Chase vehicles");Check(strip&&strip.GetComponentsInChildren<UnityEngine.UI.Text>().Any(t=>t.text.Contains("Patrol Car"))&&strip.transform.Find("Patrol car stats"),"the patrol car turns on the setup screen with its stat bars");
  yield return Late(()=>Shot4k("B-police-setup-solo"));
  // from Free Roam: the pause menu has POLICE CHASE; it asks once
  yield return Press091(GamepadButton.East);flow.StartFreeRoam();yield return WaitScene095(()=>Scene==RaceFlow.RoamScene&&flow.State==RaceFlow.Stage.Racing);
  int roamCourse=RaceFlow.RoamCourse;yield return Press091(GamepadButton.Start);
  Check(flow.State==RaceFlow.Stage.Ready&&flow.RoamMenu&&Button90("police"),$"Free Roam paused: rows [{string.Join(", ",Rows091().Select(r=>r.name))}]");
  yield return Choose091("police");Check(Title095=="POLICE CHASE"&&Details095.Contains("Leave Free Roam"),$"from Free Roam it asks: {Title095} / {Details095}");yield return Late(()=>Shot4k("B-from-roam-asks"));
  yield return Choose091("confirm");Check(Page90=="police"&&SplitScreen.FromRoam,$"confirmed: the setup ({Details095})");
  SplitScreen.P2Vehicle="original";SplitScreen.Course=0;SplitScreen.Time=TimeOfDay.Day;SplitScreen.Weather=Weather.Clear;SplitScreen.Traffic=false;SplitScreen.PoliceMinutes=3;Menus089.Show();yield return Settle091();
  yield return Choose091("police-start");yield return WaitScene095(()=>SplitScreen.Active&&PoliceChase.Current&&SplitScreen.Race&&SplitScreen.Race.P2Car);
  var police=PoliceChase.Current;var split=SplitScreen.Race;var cam=Camera.main;
  Check(police.Cop==1&&police.Rounds==1&&SplitScreen.OneView&&cam.rect==new Rect(0,0,1,1)&&!split.Camera2.enabled&&SplitScreen.Views.Count<=1,$"solo chase: full screen (camera {cam.rect}, second camera drawn {split.Camera2.enabled}), player 1 the cop in the {race.vehicle.GetComponent<VehicleConfiguration>().Profile.Name}, the AI runs in the {split.P2Car.GetComponent<VehicleConfiguration>().Profile.Name}; rounds {police.Rounds}");
  float t0=Time.time;while(police.State==PoliceChase.Phase.Starting&&Time.time-t0<10)yield return null;yield return new WaitForSeconds(1.5f);
  var hud=FindObjectsByType<UnityEngine.UI.Text>(FindObjectsSortMode.None).Where(t=>t.enabled&&t.gameObject.activeInHierarchy&&t.text.Contains("BUST")).Select(t=>t.text.Replace("\n"," / ")).ToList();
  Check(hud.Count==1&&hud[0].Contains("COP")&&hud[0].Contains("AI"),$"one HUD: {string.Join(" || ",hud)}");yield return Late(()=>Shot4k("B-solo-chase-hud"));
  // the runner stops; the cop pulls up behind it: caught
  var runner=split.P2Car;var ai=runner.GetComponent<RoadDriver>();if(ai)ai.enabled=false;var cop=race.vehicle;cop.GetComponent<VehicleInput>().enabled=false;
  for(int k=0;k<40&&police.State==PoliceChase.Phase.Running;k++){runner.Body.linearVelocity=Vector3.zero;var f=Vector3.ProjectOnPlane(runner.transform.forward,Vector3.up).normalized;
   if(Vector3.Distance(cop.Body.position,runner.Body.position)>9){var at=runner.Body.position-f*7f+Vector3.up*.6f;cop.Body.position=at;cop.Body.rotation=Quaternion.LookRotation(f);cop.transform.SetPositionAndRotation(at,cop.Body.rotation);}cop.Body.linearVelocity=cop.Body.angularVelocity=Vector3.zero;yield return new WaitForSeconds(.25f);}
  Check(police.State==PoliceChase.Phase.Caught&&police.Runs.Count==1,$"caught: {police.SoloVerdict} (meter {police.Meter:P0})");yield return Late(()=>Shot4k("B-solo-caught"));
  t0=Time.realtimeSinceStartup;while(flow.State!=RaceFlow.Stage.Results&&Time.realtimeSinceStartup-t0<20)yield return null;yield return Settle091();
  Check(flow.State==RaceFlow.Stage.Results&&Details095.StartsWith("CAUGHT in ")&&Button90("rematch")&&Text(Button90("menu"))=="Back to Free Roam",$"results: {Title095} / {Details095} || {CellsText094()} || last row \"{Text(Button90("menu"))}\"");yield return Late(()=>Shot4k("B-solo-results"));
  yield return Choose091("menu");yield return WaitScene095(()=>Scene==RaceFlow.RoamScene&&!SplitScreen.Active&&flow.State==RaceFlow.Stage.Racing);
  Check(Scene==RaceFlow.RoamScene&&!SplitScreen.Active&&race.FreeRoam&&RaceFlow.RoamCourse==roamCourse,$"back in single-player Free Roam ({Scene}, {flow.State}, start {RaceFlow.RoamCourse})");
  // two players still swap
  yield return Load("StreetLoopGreybox");yield return Menu();Bind();Menus089.ResetPages();Menus089.Show();yield return Settle091();
  yield return Choose091("police");yield return Goto091("police-players");yield return Press091(GamepadButton.DpadRight);yield return Press090(pad2,GamepadButton.South);yield return Settle091();
  Check(!SplitScreen.Solo&&SplitScreen.P2Device==pad2&&Button90("police-cop")&&Button90("police-start").interactable,$"two players: {Text(Button90("police-players"))}, player 2 {SplitScreen.DeviceName(SplitScreen.P2Device)}, {Text(Button90("police-cop"))}");
  yield return Walk091("police setup (2 players)",true);yield return Late(()=>Shot4k("B-police-setup-two"));
  yield return Choose091("police-start");yield return WaitScene095(()=>SplitScreen.Active&&PoliceChase.Current&&SplitScreen.Race&&SplitScreen.Race.P2Car);police=PoliceChase.Current;
  Check(police.Rounds==2&&police.Cop==1&&!SplitScreen.OneView&&SplitScreen.Race.Camera2.enabled,$"two players: split-screen, {police.Rounds} rounds, cop {police.Cop}");
  t0=Time.time;while(police.State==PoliceChase.Phase.Starting&&Time.time-t0<10)yield return null;
  typeof(PoliceChase).GetMethod("End",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(police,new object[]{false});
  yield return WaitScene095(()=>PoliceChase.Current&&PoliceChase.Current.Round==1&&SplitScreen.Race&&SplitScreen.Race.P2Car,30);police=PoliceChase.Current;
  Check(police.Round==1&&police.Cop==2&&race.vehicle.GetComponent<VehicleConfiguration>().profileId!=VehicleProfile.Police.Id,$"round 2: the roles swapped (cop {police.Cop}; player 1 now in the {race.vehicle.GetComponent<VehicleConfiguration>().Profile.Name})");
  flow.QuitSplit(false);yield return WaitScene095(()=>!SplitScreen.Active&&flow.State==RaceFlow.Stage.Ready);SplitScreen.Solo=true;}
 IEnumerator Names095(){
  // campaign: as 0.94
  yield return Rivals094("c3-backyard-dash");
  // a quick race: no names
  yield return StartRival094("DansBackyardForward");
  bool grid=GameObject.Find("Starting grid")?.activeInHierarchy??false;string gaps=Text093("Gaps");NameTags.Trace=true;yield return new WaitForSeconds(1);
  Check(!grid&&string.IsNullOrEmpty(gaps)&&NameTags.Shown==0,$"quick race: grid card {grid}, gap line \"{gaps}\", tags {NameTags.Shown}");NameTags.Trace=false;yield return Late(()=>Shot4k("C-quick-race"));
  {var p=flow.Race.Progress;float t0=Time.realtimeSinceStartup;flow.Race.FinalizeUnfinishedAi();while(flow.State!=RaceFlow.Stage.Results&&Time.realtimeSinceStartup-t0<300){yield return null;AudioListener.volume=0;}}
  yield return Settle091();var cells=CellsText094();Check(cells.Contains("Rival ")&&cells.Contains("You")&&!CampaignData.RivalNames.Any(cells.Contains),$"quick race results: {cells}");yield return Late(()=>Shot4k("C-quick-results"));
  yield return Menu();
  // split-screen: two AI rivals, player 2 a person: only player 2 named
  Pads090();SplitScreen.P1Device=pad91;SplitScreen.P2Device=pad2;SplitScreen.P2Ai=false;SplitScreen.Mode=SplitScreen.Kind.Race;SplitScreen.P2Pick="kyle";
  SplitScreen.P1Vehicle="moto";SplitScreen.P2Vehicle="atv";SplitScreen.Course=6;SplitScreen.Laps=1;SplitScreen.Rivals=2;SplitScreen.Traffic=false;SplitScreen.Time=TimeOfDay.Day;SplitScreen.Weather=Weather.Clear;
  flow.StartSplit();yield return SplitRunning090();var split=SplitScreen.Race;
  var p1=race.vehicle;p1.GetComponent<VehicleInput>().enabled=false;var d1=p1.gameObject.AddComponent<RoadDriver>();d1.Initialize(race,p1,true,1,1);d1.Racer=race.Racers[0];
  var c2=split.P2Car;var d2=c2.gameObject.AddComponent<RoadDriver>();d2.Initialize(race,c2,true,1,1);d2.Racer=split.P2;
  NameTags.Trace=true;yield return new WaitForSeconds(8);int shown=NameTags.Shown;string trace=NameTags.LastTrace;NameTags.Trace=false;
  var halfGrid=FindObjectsByType<UnityEngine.UI.Text>(FindObjectsSortMode.None).Count(t=>t.gameObject.activeInHierarchy&&t.text.Contains("STARTING GRID"));
  Check(shown<=2&&!trace.Contains("EMBER")&&!trace.Contains("GOLD")&&halfGrid==0,$"split-screen with 2 AI rivals: {shown} tags (only the other player; trace {trace})");yield return Late(()=>Shot4k("C-split-tags"));
  {float t0=Time.realtimeSinceStartup;while(flow.State!=RaceFlow.Stage.Results&&Time.realtimeSinceStartup-t0<300){yield return null;AudioListener.volume=0;}}
  yield return Settle091();cells=CellsText094();Check(cells.Contains("kyle")&&cells.Contains("AI rival")&&!cells.Contains("EMBER"),$"split-screen results: {cells}");yield return Late(()=>Shot4k("C-split-results"));
  flow.QuitSplit(false);yield return WaitScene095(()=>!SplitScreen.Active&&flow.State==RaceFlow.Stage.Ready);
  // the setting
  flow.OpenSettings();yield return Settle091();Check(Text(Button90("name-tags")).Contains("On")||Text(Button90("name-tags")).Contains("Off"),$"Settings: \"{Text(Button90("name-tags"))}\"");flow.CloseSettings();}
 IEnumerator Lake095(){
  yield return DanCopy095();flow.StartFreeRoam();yield return WaitScene095(()=>Scene==RaceFlow.RoamScene&&flow.State==RaceFlow.Stage.Racing);
  var lake=FindObjectsByType<ShallowWater>(FindObjectsSortMode.None).FirstOrDefault(w=>w.name.StartsWith("Friend's lake"));
  Check(lake&&lake.GetComponent<MeshRenderer>().enabled,$"Free Roam has the lake: {(lake?$"{lake.name} surface {lake.Surface:F2}":"none")}; shores built {FindObjectsByType<SceneryWater>(FindObjectsSortMode.None).Sum(w=>w.Shores)}");
  var cam=Camera.main;var chase=cam.GetComponent<ChaseCamera>();var views=CameraViews.Current;
  foreach(var time in new[]{TimeOfDay.Day,TimeOfDay.Night}){
   WorldLook.Current?.SetClock(WorldLook.Current.Day,time==TimeOfDay.Night?23:13);yield return new WaitForSeconds(1.5f);yield return Put093(new Vector3(710.5f,79.65f,-79.22f),300);
   chase.enabled=false;if(views)views.enabled=false;yield return Late(()=>{cam.transform.SetPositionAndRotation(new Vector3(710.5f,81.4f,-79.22f),Quaternion.Euler(6,300,0));Shot4k("D-lake-from-BUG-004-"+time);});chase.enabled=true;if(views)views.enabled=true;}
  WorldLook.Current?.SetClock(WorldLook.Current.Day,13);
  // into the water from the west shore and out again
  var car=race.vehicle;car.GetComponent<VehicleInput>().enabled=false;yield return Put093(new Vector3(590,0,-20),90);float maxIn=0;
  for(int i=0;i<250;i++){car.enabled=false;car.Simulate(1,0,0,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();maxIn=Mathf.Max(maxIn,ShallowWater.Sample(car,out _));}
  var inAt=car.Body.position;Check(maxIn>.3f&&lake.Contains(inAt),$"driven in from the west shore: in the water at {V(inAt)}, immersion up to {maxIn:P0}");
  yield return Late(()=>Shot4k("D-in-the-lake"));
  for(int i=0;i<120;i++){car.enabled=false;car.Simulate(0,1,0,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();}
  car.Body.rotation=Quaternion.Euler(0,270,0);car.transform.rotation=car.Body.rotation;float dryT=-1;
  for(int i=0;i<900&&dryT<0;i++){car.enabled=false;car.Simulate(1,0,0,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();if(ShallowWater.Sample(car,out _)<=0&&!lake.Contains(car.Body.position))dryT=i*Time.fixedDeltaTime;}
  car.enabled=true;Check(dryT>=0,$"driven back out without a reset: dry ground after {dryT:F1} s at {V(car.Body.position)}");
  // the BUG-004 shore: a vehicle set at the water's edge there drives (or resets) out
  yield return Put093(new Vector3(690,0,-66),250);var resp=car.GetComponent<VehicleRespawn>();resp.ResetVehicle();float r0=Time.realtimeSinceStartup;while(resp.Pending&&Time.realtimeSinceStartup-r0<6)yield return null;yield return new WaitForSeconds(.5f);
  Check(!resp.Pending&&ShallowWater.Sample(car,out _)<=0,$"a reset at the BUG-004 shore: {resp.LastRecovery} at {V(car.Body.position)}");
  // Snow: the lake freezes
  ShallowWater.SetFrozen(true);Check(lake.Ice,$"Snow: the lake has its ice ({(lake.Ice?"yes":"no")})");ShallowWater.SetFrozen(false);
  yield return Menu();}
 static readonly (string id,Vector3 p,float yaw)[] Spots095={("BUG-001",new Vector3(1193.72f,89.38f,149.28f),65),("BUG-006",new Vector3(806.89f,64.03f,328.19f),62),("BUG-002",new Vector3(811.92f,94.46f,231.21f),90),("BUG-003",new Vector3(1158.74f,109.96f,166.14f),0),("BUG-007",new Vector3(1008.87f,164.63f,107.92f),90)};
 IEnumerator Holes095(){
  yield return DanCopy095();flow.StartFreeRoam();yield return WaitScene095(()=>Scene==RaceFlow.RoamScene&&flow.State==RaceFlow.Stage.Racing);
  var cam=Camera.main;var chase=cam.GetComponent<ChaseCamera>();var views=CameraViews.Current;var car=race.vehicle;var resp=car.GetComponent<VehicleRespawn>();
  foreach(var (id,p,yaw) in Spots095){
   yield return Put093(p,yaw);chase.enabled=false;if(views)views.enabled=false;yield return Late(()=>{cam.transform.SetPositionAndRotation(p+Vector3.up*1.7f,Quaternion.Euler(4,yaw,0));Shot4k("E-"+id);});chase.enabled=true;if(views)views.enabled=true;
   if(id=="BUG-002"||id=="BUG-003"){
    bool hollow=VehicleRespawn.Hollow(car.Body.position);resp.ResetVehicle();float r0=Time.realtimeSinceStartup;while(resp.Pending&&Time.realtimeSinceStartup-r0<8)yield return null;yield return new WaitForSeconds(.6f);
    var at=car.Body.position;Check(!resp.Pending&&Vector3.Distance(at,p)>6&&!VehicleRespawn.Hollow(at),$"{id}: in the pit (a hollow: {hollow}); reset: {resp.LastRecovery} to {V(at)}, {Vector3.Distance(at,p):F0} m away, not a hollow; {resp.RecoveryDiagnostic}");
    yield return Late(()=>Shot4k("E-"+id+"-after-reset"));}}
  // the floating-tree check over all of Free Roam (0.84 Part K's), BUG-007 within 60 m marked
  yield return Floaters(RaceFlow.RoamScene,"1008.87,107.92,60");
  yield return Menu();}
 // a copy of Dan's save (reward earned, panel never shown): the panel once at the main menu; A remembers it; not again
 IEnumerator Missed095(){yield return DanCopy094();yield return Settle091();
  Check(Page90=="unlock"&&VehicleUnlocks.RewardEarned&&!UnlockNotice.Seen(flow.Save,UnlockNotice.AcornMower),$"Dan's save: the main menu shows the missed panel: {Title095} / {Details095}");yield return Late(()=>Shot4k("F-missed-panel-dans-save"));
  yield return Press091(GamepadButton.A);yield return Settle091();Check(Page90==""&&UnlockNotice.Seen(flow.Save,UnlockNotice.AcornMower),$"A: back to the main menu (page '{Page90}'), remembered in settings");
  yield return Load("DansBackyardForward");yield return Menu();Bind();yield return Settle091();yield return Settle091();Check(Page90!="unlock"&&UnlockNotice.Showing==null,$"the main menu again (another scene, same save): no panel (page '{Page90}')");}
 IEnumerator Acorns095(){
  // an isolated save: two acorns short of the reward
  // (the check's own save: its acorns set to 22 of 24 and the reward not earned, in memory; nothing of Dan's is written)
  yield return DanCopy095();flow.Save.SaveSettings();flow.StartFreeRoam();yield return WaitScene095(()=>Scene==RaceFlow.RoamScene&&flow.State==RaceFlow.Stage.Racing);
  var col=race.GetComponent<ExplorationCollection>();
  var data=typeof(ExplorationCollection).GetField("data",Any).GetValue(col) as ExplorationCollection.Save;data.found.Clear();data.found.AddRange(col.sites.Take(col.sites.Length-2).Select(s=>s.id));data.rewardEarned=false;VehicleUnlocks.Set(col.Found,false);
  flow.Save.Settings.unlocksSeen=new string[0];UnlockNotice.Pending.Clear();UnlockNotice.Showing=null;yield return new WaitForSeconds(.5f);
  var car=race.vehicle;
  IEnumerator Through(ExplorationCollection.Site s){yield return Put093(s.position+Vector3.forward*6,180);car.GetComponent<VehicleInput>().enabled=false;col.ResetMovement();for(int i=0;i<90&&flow.State==RaceFlow.Stage.Racing;i++){car.Body.MovePosition(Vector3.Lerp(s.position+Vector3.forward*6,s.position-Vector3.forward*6,i/89f));yield return null;}car.GetComponent<VehicleInput>().enabled=true;}
  var next=col.sites.First(s=>!col.Discovered(s.id));yield return Through(next);yield return new WaitForSeconds(.3f);
  var banner=AcornBanner.Current?.Shown??"";Check(col.Found==col.sites.Length-1&&banner.StartsWith($"ACORN FOUND   {col.sites.Length-1} / {col.sites.Length}")&&banner.Contains(" / ")&&banner.Split('/').Length>=3,$"a pickup: banner \"{banner}\"");
  WorldLook.Current?.SetClock(WorldLook.Current.Day,13);yield return Late(()=>Shot4k("F-pickup-banner-day"));WorldLook.Current?.SetClock(WorldLook.Current.Day,23);yield return new WaitForSeconds(.2f);yield return Late(()=>Shot4k("F-pickup-banner-night"));WorldLook.Current?.SetClock(WorldLook.Current.Day,13);
  yield return new WaitForSeconds(AcornBanner.Seconds);Check(AcornBanner.Current.Shown=="",$"the banner is gone after {AcornBanner.Seconds} s");
  // the 24th: the panel, the game paused, A closes it and remembers it
  var last=col.sites.First(s=>!col.Discovered(s.id));yield return Through(last);float t0=Time.realtimeSinceStartup;while(Page90!="unlock"&&Time.realtimeSinceStartup-t0<5){yield return null;}
  yield return Settle091();var stats=GameObject.Find("Unlocked vehicle stats");
  Check(Page90=="unlock"&&flow.State==RaceFlow.Stage.Ready&&Time.timeScale==0&&Title095.StartsWith("ALL 24 WOODLAND ACORNS FOUND")&&Details095.Contains("TURF ROCKET")&&Details095.Contains("Garage")&&stats&&VehicleUnlocks.RewardEarned,$"the 24th: {Title095} / {Details095}; paused {Time.timeScale==0}; stat bars {(stats?stats.transform.childCount:0)}");
  yield return Late(()=>Shot4k("F-unlock-panel"));
  yield return Press091(GamepadButton.East);Check(Page90=="unlock","B does not close the panel");
  yield return Press091(GamepadButton.A);yield return Settle091();
  Check(Page90!="unlock"&&flow.State==RaceFlow.Stage.Racing&&UnlockNotice.Seen(flow.Save,UnlockNotice.AcornMower),$"A: closed, driving again ({flow.State}), remembered {UnlockNotice.Seen(flow.Save,UnlockNotice.AcornMower)}");
  yield return Menu();yield return WaitScene095(()=>flow.State==RaceFlow.Stage.Ready);
  }
}
}
#endif
