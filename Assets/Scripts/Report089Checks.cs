#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Racer {
// 0.89 targeted checks, added to the 0.80 runner (same muted isolated save, same helpers): PROBE_CASES="case:args;...".
public sealed partial class Report080Checks {
 IEnumerator Cases089(string[] a)=>a[0] switch{"targets89"=>Targets089(a[1]),"flow89"=>Flow089(),"locks89"=>Locks089(),"screen89"=>Screen089(),"carshots89"=>CarShots089(a[1]),_=>null};
 // Part F: the 0.88 orbit shots (day and night with headlights) for any vehicle, with Testing on and the acorn reward set
 // in memory so the campaign and the mower lock do not swap the vehicle.
 IEnumerator CarShots089(string profiles){flow.Save.Settings.unlockEverything=true;Campaign.Testing=true;flow.Save.SaveSettings();File.WriteAllText(Path.Combine(saveDir,VehicleUnlocks.AcornFile),JsonUtility.ToJson(new ExplorationCollection.Save{rewardEarned=true},true));
  yield return CarShots088(profiles,"089");flow.Save.Settings.unlockEverything=false;Campaign.Testing=false;flow.Save.SaveSettings();}
 // The campaign screen with some progress (two events marked won on the isolated save) and the Shop, for the shots.
 IEnumerator Screen089(){if(File.Exists(Path.Combine(saveDir,Campaign.File)))File.Delete(Path.Combine(saveDir,Campaign.File));
  yield return Load("StreetLoopGreybox");yield return Menu();Campaign.Load(saveDir);
  foreach(var e in CampaignData.Events.Take(2)){Campaign.Selected=e.Id;Campaign.DebugMarkWon();}Campaign.DebugAddMoney(3400);Campaign.Selected=CampaignData.Events[2].Id;
  Menus089.OpenCampaign();yield return Page089("A-campaign-screen");
  Note("campaign screen: "+string.Join(" | ",Labels089().Where(l=>l.StartsWith("chapter")||l.StartsWith("cev-")||l.StartsWith("continue")||l.StartsWith("new-campaign"))));
  typeof(RaceMenus).GetMethod("OpenCampaignShop",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).Invoke(Menus089,null);
  typeof(RaceMenus).GetField("shopVehicle",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).SetValue(Menus089,"roadster");yield return Page089("C-shop-roadster");
  typeof(RaceMenus).GetField("shopVehicle",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).SetValue(Menus089,"moto");yield return Page089("C-shop-prize");
  typeof(RaceMenus).GetMethod("ShopBack",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).Invoke(Menus089,null);yield return null;
  flow.OpenGarage();typeof(RaceMenus).GetField("garageLocked",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).SetValue(Menus089,"tourer");yield return Page089("B-garage-locked");
  typeof(RaceMenus).GetField("garageLocked",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).SetValue(Menus089,null);flow.CloseGarage();yield return null;
  File.Delete(Path.Combine(saveDir,Campaign.File));Campaign.Load(saveDir);}

 // Waits until the campaign event that was just started is running (after the loading screen and the countdown).
 IEnumerator EventRunning089(){float t0=Time.realtimeSinceStartup;yield return null;
  float next=5;while((flow==null||!flow.Started||LoadingScreen.Holding||flow.State!=RaceFlow.Stage.Racing)&&Time.realtimeSinceStartup-t0<90){yield return null;Bind();AudioListener.volume=0;
   if(Time.realtimeSinceStartup-t0>next){next+=5;Note($"  waiting: scene {Scene}, flow {(flow?"yes":"no")}, started {flow?.Started}, holding {LoadingScreen.Holding}, state {flow?.State}, timeScale {Time.timeScale}, campaign {CampaignRun.Active?.Id} pending {CampaignRun.PendingStart}");}}
  Note($"  running after {Time.realtimeSinceStartup-t0:F0} s: state {flow?.State}, campaign {CampaignRun.Active?.Id}");}
 IEnumerator Results089(float limit,float scale=1){Time.timeScale=scale;float t0=Time.time,r0=Time.realtimeSinceStartup;
  while(flow.State!=RaceFlow.Stage.Results&&Time.time-t0<limit&&Time.realtimeSinceStartup-r0<limit*2+60){AudioListener.volume=0;yield return null;}Time.timeScale=1;yield return null;}
 RoadDriver Pilot089(int skill){var car=race.vehicle;race.difficulty=skill;var pilot=car.gameObject.AddComponent<RoadDriver>();pilot.Initialize(race,car,true,1,1);pilot.Racer=race.Racers[0];car.GetComponent<VehicleInput>().enabled=false;return pilot;}
 void Unpilot089(RoadDriver pilot){if(pilot)Destroy(pilot);if(race&&race.vehicle)race.vehicle.GetComponent<VehicleInput>().enabled=true;}
 // Runs Floor089 beside the caller and stops it when the results come up (they stop time, and a coroutine waiting for a
 // physics step would then never resume).
 IEnumerator RunFloor089(float seconds){bool done=false;var c=StartCoroutine(Wrap089(Floor089(seconds),()=>done=true));float r0=Time.realtimeSinceStartup;
  while(!done&&flow.State!=RaceFlow.Stage.Results&&Time.realtimeSinceStartup-r0<seconds*3+30)yield return null;
  if(!done){StopCoroutine(c);var car=race.vehicle;car.enabled=true;car.GetComponent<VehicleInput>().enabled=true;}}
 IEnumerator Wrap089(IEnumerator inner,Action after){yield return inner;after();}
 // Straight full throttle along the course road (the speed trap run): steer at the road 15 m ahead.
 IEnumerator Floor089(float seconds){var car=race.vehicle;car.GetComponent<VehicleInput>().enabled=false;car.enabled=false;float t0=Time.time;
  while(Time.time-t0<seconds&&flow.State==RaceFlow.Stage.Racing&&!CampaignRun.Done){AudioListener.volume=0;var p=car.Body.position;float s=race.road.Project(p,out _);var to=race.road.At(s+15,out _)-p;to.y=0;
   float ang=Vector3.SignedAngle(Vector3.ProjectOnPlane(car.transform.forward,Vector3.up),to,Vector3.up);car.Simulate(1,0,Mathf.Clamp(ang/25f,-1,1),Time.fixedDeltaTime);yield return new WaitForFixedUpdate();}
  car.enabled=true;car.GetComponent<VehicleInput>().enabled=true;}

 // Medal targets from the two starters, through the real event flow with Testing on (nothing saved): the speed trap at
 // full throttle from the event's standing start, and the time trial's flying lap driven by the race AI at Normal and Hard.
 IEnumerator Targets089(string vehicles){
  var rows=new List<string>();flow.Save.Settings.unlockEverything=true;Campaign.Testing=true;flow.Save.SaveSettings();
  var trap=CampaignData.Find("c1-hwy92-trap");var tt=CampaignData.Find("c1-against-clock");
  foreach(var v in vehicles.Split(',')){
   flow.StartCampaignEvent(trap,v);yield return EventRunning089();
   var start=race.vehicle.Body.position;yield return RunFloor089(trap.TimeLimit+2);yield return Results089(10);
   var o=CampaignRun.Last;string line=$"trap {v}: start {V(start)}, result {(o==null?"none":o.Dnf?"no crossing":DisplayUnits.Speed(o.Score)+$" ({o.Score:F2} m/s)")}, medal {o?.Medal}";rows.Add(line);Note(line);
   flow.QuitRace();yield return null;
   foreach(int skill in new[]{1,2}){
    flow.StartCampaignEvent(tt,v);yield return EventRunning089();var pilot=Pilot089(skill);yield return Results089(400,3);Unpilot089(pilot);
    var t=CampaignRun.Last;line=$"time trial {v} AI skill {skill}: {(t==null||t.Dnf?"DNF":RaceHud.FormatTime(t.Time)+$" ({t.Time:F2} s)")}, missed gates {race.Progress.MissedGates}, laps {race.Progress.LapTimes.Count}";rows.Add(line);Note(line);
    flow.QuitRace();yield return null;}}
  flow.Save.Settings.unlockEverything=false;Campaign.Testing=false;flow.Save.SaveSettings();
  File.WriteAllLines(output+"/campaign-targets.txt",rows);}

 static string Hash089(string path){if(!File.Exists(path))return "missing";using var sha=System.Security.Cryptography.SHA256.Create();return BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))).Replace("-","").Substring(0,16);}
 Dictionary<string,string> Protected089(){var d=new Dictionary<string,string>();foreach(var f in Directory.GetFiles(saveDir).Where(f=>!Path.GetFileName(f).StartsWith("campaign")&&!f.EndsWith(".bak")&&!f.EndsWith(".tmp")&&!Path.GetFileName(f).StartsWith("settings")))d[Path.GetFileName(f)]=Hash089(f);return d;}
 string[] Labels089()=>FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None).Where(b=>b.gameObject.activeInHierarchy).Select(b=>b.name+"="+string.Join(" ",b.GetComponentsInChildren<UnityEngine.UI.Text>(true).Where(t=>t.gameObject.activeSelf).Select(t=>t.text.Replace("\n"," / ")))).ToArray();
 RaceMenus Menus089=>FindAnyObjectByType<RaceMenus>();
 IEnumerator Page089(string shot){Menus089.Show();yield return new WaitForSecondsRealtime(.4f);yield return Late(()=>Shot(shot));}

 // New campaign on the isolated save (with Dan-like records and 22 acorns already there): the menu entry, the campaign
 // screen, event 1 (a race), its payout and event 2 opening; event 2 (speed trap) and event 3 (time trial) each run once;
 // event 5 opens Street Loop - Reverse with its conditions; the final's 6-car grid; the shop refusing and then buying; the
 // F6 entries; and the records / acorns unchanged.
 IEnumerator Flow089(){
  // fixtures: a lap record, a Top 10 board and 22 of the acorns, as on Dan's install
  var col=FindAnyObjectByType<ExplorationCollection>();var ids=col?col.sites.Select(x=>x.id).ToList():new List<string>();
  File.WriteAllText(Path.Combine(saveDir,VehicleUnlocks.AcornFile),JsonUtility.ToJson(new ExplorationCollection.Save{found=ids.Take(22).ToList()},true));
  flow.Boards.Add("fixture-1",race.Category,false,131.5,"original");flow.Save.RecordLap(131.5);
  if(File.Exists(Path.Combine(saveDir,Campaign.File)))File.Delete(Path.Combine(saveDir,Campaign.File));Campaign.Load(saveDir);
  var before=Protected089();Note("protected files: "+string.Join(", ",before.Select(kv=>kv.Key+" "+kv.Value)));
  Check(!Campaign.Exists&&Campaign.Current.money==0&&Campaign.Current.owned.SequenceEqual(new[]{"original","atv"})&&Campaign.CourseOpen(0)&&Enumerable.Range(1,7).All(c=>!Campaign.CourseOpen(c)),
   $"no campaign save: new player: money {Campaign.Current.money}, owned [{string.Join(",",Campaign.Current.owned)}], open courses [{string.Join(",",Enumerable.Range(0,8).Where(Campaign.CourseOpen).Select(c=>RacePlaylists.Titles[c]))}]");
  // the main menu: CAMPAIGN first
  yield return Menu();Menus089.ResetPages();Menus089.Show();yield return new WaitForSecondsRealtime(.3f);
  var first=FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None).Where(b=>b.gameObject.activeInHierarchy&&b.transform.parent&&b.transform.parent.name=="Content").OrderBy(b=>b.transform.GetSiblingIndex()).FirstOrDefault();
  yield return Late(()=>Shot("A-main-menu"));
  Check(first&&first.name=="campaign",$"main menu first entry: '{first?.name}'");
  Menus089.OpenCampaign();yield return Page089("A-campaign-screen-new");
  Note("campaign screen: "+string.Join(" | ",Labels089().Where(l=>l.StartsWith("chapter")||l.StartsWith("cev-")||l.StartsWith("continue")||l.StartsWith("shop")||l.StartsWith("new-campaign"))));
  // event 1: a race, driven by the race AI on the Trail Four (a starter)
  var e1=CampaignData.Events[0];
  typeof(RaceMenus).GetField("campaignEvent",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).SetValue(Menus089,e1.Id);
  typeof(RaceMenus).GetField("campaignVehicle",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).SetValue(Menus089,"atv");
  typeof(RaceMenus).GetMethod("Navigate",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).Invoke(Menus089,new object[]{"campaign-event"});
  yield return Page089("A-event-page");
  var startButton=FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None).FirstOrDefault(b=>b.name=="start-event"&&b.gameObject.activeInHierarchy);
  Check(startButton&&startButton.interactable,"event 1 page: START EVENT offered");
  startButton.onClick.Invoke();yield return null;yield return EventRunning089();
  Check(CampaignRun.Active==e1&&race.Racers.Count==4&&race.laps==2&&race.difficulty==0&&!race.traffic&&race.vehicle.GetComponent<VehicleConfiguration>().profileId=="atv"&&WorldLook.Current.RaceTime==TimeOfDay.Day,
   $"event 1 running: {race.Racers.Count} racers ({race.RosterLabel}), {race.laps} laps, difficulty {race.difficulty}, traffic {race.traffic}, vehicle {race.vehicle.GetComponent<VehicleConfiguration>().profileId}, {WorldLook.Current.RaceTime}/{WorldLook.Current.RaceWeather}");
  var pilot=Pilot089(1);yield return Results089(500,3);Unpilot089(pilot);
  if(flow.State==RaceFlow.Stage.Racing&&race.Progress.Finished){race.FinalizeUnfinishedAi();yield return Results089(10);}
  var o1=CampaignRun.Last;yield return Page089("A-results-event1");
  var saved=JsonUtility.FromJson<Campaign.State>(File.ReadAllText(Path.Combine(saveDir,Campaign.File)));
  Check(o1!=null&&!o1.Dnf&&o1.Pay>0&&saved.money==o1.Pay+o1.Bonus&&saved.results.Any(r=>r.id==e1.Id&&r.runs==1),
   $"event 1 result: {o1?.Headline}, pay {o1?.Pay}, bonus {o1?.Bonus}, unlocked [{(o1==null?"":string.Join("; ",o1.Unlocked))}]; saved money {saved.money}; best '{Campaign.BestText(e1)}'");
  Check(Campaign.Available(CampaignData.Events[1])==o1.Passed,$"event 2 opens when event 1 is passed: passed {o1.Passed}, event 2 available {Campaign.Available(CampaignData.Events[1])}");
  flow.QuitRace();yield return null;yield return Page089("A-campaign-after-event1");
  Check(flow.State==RaceFlow.Stage.Ready&&!race.opponents==!flow.Save.Settings.opponents&&race.laps==Mathf.Clamp(flow.Save.Settings.laps,1,5)&&race.difficulty==flow.Save.Settings.difficulty,
   $"Continue: back on the campaign screen; Race settings restored (opponents {race.opponents}, laps {race.laps}, difficulty {race.difficulty})");
  if(!o1.Passed){Campaign.Selected=e1.Id;Campaign.DebugMarkWon();Note("event 1 not passed by the AI driver: marked won (F6) to go on");}
  // event 2: the speed trap, full throttle on a starter
  var e2=CampaignData.Events[1];int money2=Campaign.Current.money;
  flow.StartCampaignEvent(e2,"original");yield return EventRunning089();
  var at2=race.vehicle.Body.position;yield return RunFloor089(e2.TimeLimit+2);yield return Results089(10);
  var o2=CampaignRun.Last;yield return Page089("A-results-event2-trap");
  Check(o2!=null&&!o2.Dnf&&Campaign.Current.money==money2+o2.Pay+o2.Bonus,$"event 2 (speed trap): start {V(at2)}, {o2?.Headline}, pay {o2?.Pay} + bonus {o2?.Bonus}, money {Campaign.Current.money}");
  flow.QuitRace();yield return null;
  if(!o2.Passed){Campaign.Selected=e2.Id;Campaign.DebugMarkWon();Note("event 2 not passed: marked won (F6)");}
  // event 3: the time trial, a flying lap by the race AI (Normal)
  var e3=CampaignData.Events[2];
  flow.StartCampaignEvent(e3,"atv");yield return EventRunning089();var at3=race.vehicle.Body.position;
  pilot=Pilot089(1);yield return Results089(400,3);Unpilot089(pilot);
  var o3=CampaignRun.Last;yield return Page089("A-results-event3-timetrial");
  Check(o3!=null&&!o3.Dnf&&race.Racers.Count==1,$"event 3 (time trial): start {V(at3)} ({e3.RunUp} m before START), {o3?.Headline}, pay {o3?.Pay} + bonus {o3?.Bonus}, money {Campaign.Current.money}");
  flow.QuitRace();yield return null;
  if(!o3.Passed){Campaign.Selected=e3.Id;Campaign.DebugMarkWon();Note("event 3 not passed: marked won (F6)");}
  // event 4 marked won through the F6 entry; event 5 started: Street Loop - Reverse opens, Dusk / Rain
  DeveloperLocationHud.Instance.SetEnabled(true);yield return null;
  UnityEngine.UI.Button F6(string n)=>DeveloperLocationHud.Instance.GetComponentsInChildren<UnityEngine.UI.Button>(true).First(b=>b.name==n);
  IEnumerator Free(){float r=Time.realtimeSinceStartup;while(MenuInput.UiBlocked&&Time.realtimeSinceStartup-r<5)yield return null;}
  Campaign.Selected=CampaignData.Events[3].Id;yield return Free();F6("Mark event won").onClick.Invoke();yield return null;
  Check(Campaign.Passed(CampaignData.Events[3])&&Campaign.Available(CampaignData.Events[4]),$"F6 Mark event won: '{CampaignData.Events[3].Name}' passed, event 5 open");
  var e5=CampaignData.Events[4];Check(!Campaign.CourseOpen(1),"Street Loop - Reverse locked before event 5");
  flow.StartCampaignEvent(e5,"original");yield return EventRunning089();yield return new WaitForSecondsRealtime(2);
  Check(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name=="StreetLoopReverse"&&Campaign.CourseOpen(1)&&WorldLook.Current.RaceTime==TimeOfDay.Dusk&&WorldLook.Current.RaceWeather==Weather.Rain&&race.Racers.Count==4,
   $"event 5 started: scene {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}, Reverse now open {Campaign.CourseOpen(1)}, {WorldLook.Current.RaceTime}/{WorldLook.Current.RaceWeather}, {race.Racers.Count} racers");
  yield return Late(()=>Shot("D-event5-dusk-rain"));
  flow.Pause();yield return null;flow.QuitRace();yield return null;
  Check(CampaignRun.Active==null&&Campaign.ResultOf(e5.Id)==null&&flow.State==RaceFlow.Stage.Ready,"event 5 ended from the pause menu: no result, no payout, campaign screen");
  Campaign.Selected=e5.Id;yield return Free();F6("Mark event won").onClick.Invoke();yield return null;
  // the final: 5 rivals on the grid, nobody past the line before GO
  var e6=CampaignData.Events[5];
  flow.StartCampaignEvent(e6,"original");float tw=Time.realtimeSinceStartup;yield return null;
  while((flow==null||!flow.Started||LoadingScreen.Holding||flow.State!=RaceFlow.Stage.Countdown)&&Time.realtimeSinceStartup-tw<90){yield return null;Bind();AudioListener.volume=0;}
  yield return new WaitForSeconds(.5f);
  var grid=race.Racers.Select(r=>race.road.Relative(race.road.Project(r.Car.Body.position,out _),race.Origin)).Select(s=>s>race.road.Length/2?s-race.road.Length:s).ToArray();
  yield return Late(()=>Shot("D-final-grid"));
  if(CampaignRun.Active!=e6){Check(false,"final did not start");yield break;}
  yield return EventRunning089();yield return new WaitForSecondsRealtime(6);
  Check(race.Racers.Count==6&&grid.All(s=>s<-3)&&race.Racers.Skip(1).All(r=>r.Progress.LapActive),$"final: {race.Racers.Count} racers ({race.RosterLabel}); grid metres before the line [{string.Join(", ",grid.Select(s=>s.ToString("F0")))}]; every rival's lap started after GO {race.Racers.Skip(1).All(r=>r.Progress.LapActive)}");
  flow.Pause();yield return null;flow.QuitRace();yield return null;
  // the shop: refused without the money, bought with it
  int cash=Campaign.Current.money;var pebble=VehicleProfile.Find("pebble");
  bool refused=!Campaign.CanBuy(pebble,out string why)&&cash<Campaign.Price("pebble");
  typeof(RaceMenus).GetMethod("OpenCampaignShop",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).Invoke(Menus089,null);
  typeof(RaceMenus).GetField("shopVehicle",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).SetValue(Menus089,"pebble");yield return Page089("C-shop-cannot-buy");
  var buy=FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None).FirstOrDefault(b=>b.name=="buy"&&b.gameObject.activeInHierarchy);
  Check(refused&&buy&&!buy.interactable&&!Campaign.Owns("pebble"),$"shop with {Campaign.Money(cash)}: Pebble Coupe ({Campaign.Money(Campaign.Price("pebble"))}) refused: '{why}', Buy enabled {buy?.interactable}");
  yield return Free();F6("Campaign +$5,000").onClick.Invoke();if(Campaign.Current.money<Campaign.Price("pebble"))F6("Campaign +$5,000").onClick.Invoke();yield return null;
  cash=Campaign.Current.money;Menus089.Show();yield return null;buy=FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None).FirstOrDefault(b=>b.name=="buy"&&b.gameObject.activeInHierarchy);
  buy.onClick.Invoke();yield return Page089("C-shop-confirm");
  var confirm=FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None).FirstOrDefault(b=>b.name=="confirm"&&b.gameObject.activeInHierarchy);confirm.onClick.Invoke();yield return Page089("C-shop-bought");
  saved=JsonUtility.FromJson<Campaign.State>(File.ReadAllText(Path.Combine(saveDir,Campaign.File)));
  Check(Campaign.Owns("pebble")&&saved.owned.Contains("pebble")&&saved.money==cash-6000&&race.PlayerVehicles.Any(p=>p.Id=="pebble"),$"bought with {Campaign.Money(cash)}: Pebble owned (saved), money {Campaign.Money(saved.money)}; garage offers [{string.Join(", ",race.PlayerVehicles.Select(p=>p.Name))}]");
  typeof(RaceMenus).GetMethod("ShopBack",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).Invoke(Menus089,null);yield return Page089("A-campaign-screen-later");
  Check(flow.State==RaceFlow.Stage.Ready,"Shop Back returns to the campaign screen");
  // records and acorns untouched
  var after=Protected089();var changed=before.Where(kv=>!after.TryGetValue(kv.Key,out var h)||h!=kv.Value).Select(kv=>kv.Key).ToArray();
  Check(changed.Length==0&&VehicleUnlocks.AcornsFound==22,$"records, boards, ghosts and acorns unchanged after the campaign runs: {(changed.Length==0?"all "+before.Count+" files identical":"CHANGED "+string.Join(", ",changed))}; acorns {VehicleUnlocks.AcornsFound}/24");
  // F6: unlock all, then reset
  yield return Free();F6("Unlock campaign").onClick.Invoke();yield return null;
  Check(Enumerable.Range(0,8).All(Campaign.CourseOpen)&&VehicleProfile.All.Where(p=>!p.Reward).All(p=>Campaign.Owns(p.Id))&&Campaign.Current.chapter==4,"F6 Unlock campaign: every course, vehicle and chapter");
  yield return Free();F6("Reset campaign").onClick.Invoke();yield return null;
  saved=JsonUtility.FromJson<Campaign.State>(File.ReadAllText(Path.Combine(saveDir,Campaign.File)));
  Check(saved.money==0&&saved.owned.Count==2&&saved.results.Count==0&&!Campaign.CourseOpen(1),"F6 Reset campaign: a new campaign written");
  DeveloperLocationHud.Instance.SetEnabled(false);
  File.WriteAllText(output+"/campaign-save-final.json",File.ReadAllText(Path.Combine(saveDir,Campaign.File)));}

 // Race lists and Free Roam with a new campaign; the Testing switch on and off.
 IEnumerator Locks089(){
  if(File.Exists(Path.Combine(saveDir,Campaign.File)))File.Delete(Path.Combine(saveDir,Campaign.File));
  yield return Load("StreetLoopGreybox");yield return Menu();Campaign.Load(saveDir);
  flow.OpenCourses();yield return Page089("B-tracks-locked");
  var rows=Labels089().Where(l=>l.StartsWith("course-")).ToArray();Note("Tracks: "+string.Join(" | ",rows));
  Check(rows.Count(r=>r.Contains("LOCKED"))==7,$"Race Tracks: {rows.Count(r=>r.Contains("LOCKED"))} of {rows.Length} locked with how to get them");
  flow.SelectCourseEntry(3);yield return new WaitForSecondsRealtime(3);Check(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name=="StreetLoopGreybox","picking a locked track does nothing but say how to get it");
  flow.CloseGarage();yield return null;
  flow.OpenGarage();flow.SelectVehicle("moto");yield return null;
  var lockField=typeof(RaceMenus).GetField("garageLocked",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);lockField.SetValue(Menus089,"moto");yield return Page089("B-garage-locked-race");
  var title=FindObjectsByType<UnityEngine.UI.Text>(FindObjectsSortMode.None).Select(t=>t.text).FirstOrDefault(t=>t.StartsWith("LOCKED:"))??"";
  Check(race.vehicle.GetComponent<VehicleConfiguration>().profileId!="moto"&&race.PlayerVehicles.Select(p=>p.Id).SequenceEqual(new[]{"original","atv"}),$"Race garage: choosing the Needle 600 refused; selectable [{string.Join(", ",race.PlayerVehicles.Select(p=>p.Name))}]; locked line '{title.Replace("\n"," / ")}'");
  lockField.SetValue(Menus089,null);flow.CloseGarage();yield return null;
  // Free Roam: the whole world open (every course offered as the start), only owned vehicles
  yield return EnterRoam("StreetLoopGreybox","moto");
  Check(race.vehicle.GetComponent<VehicleConfiguration>().profileId=="original"&&race.PlayerVehicles.Length==2,$"Free Roam with the Needle 600 as the saved choice: driving '{race.vehicle.GetComponent<VehicleConfiguration>().profileId}'; selectable {race.PlayerVehicles.Length}");
  flow.Pause();yield return null;flow.OpenGarage();lockField.SetValue(Menus089,"tourer");yield return Page089("B-garage-locked-roam");lockField.SetValue(Menus089,null);flow.CloseGarage();yield return null;
  var menus=Menus089;typeof(RaceMenus).GetMethod("Navigate",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).Invoke(menus,new object[]{"roam"});yield return null;
  flow.OpenCourses();yield return Page089("B-roam-tracks-open");
  rows=Labels089().Where(l=>l.StartsWith("course-")).ToArray();Check(rows.Length==8&&rows.All(r=>!r.Contains("LOCKED")),$"Free Roam start list: {rows.Length} courses, none locked");
  flow.CloseGarage();yield return null;flow.Resume();yield return null;
  // Testing on: everything; nothing written to the campaign save; off again
  flow.Save.Settings.unlockEverything=true;Campaign.Testing=true;
  Check(race.PlayerVehicles.Length==10&&Enumerable.Range(0,8).All(Campaign.CourseOpen)&&!Campaign.Save(),$"Testing on: {race.PlayerVehicles.Length} vehicles selectable (the mower stays the acorn reward), all 8 courses, campaign save not written");
  flow.Pause();yield return null;flow.OpenSettings();yield return Page089("B-settings-testing-on");flow.CloseSettings();yield return null;
  flow.Save.Settings.unlockEverything=false;Campaign.Testing=false;flow.Save.SaveSettings();
  Check(race.PlayerVehicles.Length==2&&!Campaign.CourseOpen(2)&&!File.Exists(Path.Combine(saveDir,Campaign.File)),"Testing off: back to the campaign's 2 vehicles and Street Loop; no campaign file written");}
}}
#endif
