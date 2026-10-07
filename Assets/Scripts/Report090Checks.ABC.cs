#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

namespace Racer {
// 0.90 checks for Parts A-C (locks, hints, campaign round 2), on the runner's muted isolated save.
public sealed partial class Report080Checks {
 partial void More090(string[] a,ref IEnumerator run){
  run=a[0] switch{"locks90"=>Locks090(),"hints90"=>Hints090(),"cups90"=>Cups090(),"upgrade90"=>Upgrade090(),"dan90"=>Dan090(string.Join(":",a.Skip(1))),"events90"=>Events090(),"champion90"=>Champion090(),_=>Split090Cases(a)};}
 partial void Split090(string[] a,ref IEnumerator run);
 IEnumerator Split090Cases(string[] a){IEnumerator run=null;Split090(a,ref run);return run;}
 static readonly BindingFlags Any=BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.Instance;
 object Field90(string name)=>typeof(RaceMenus).GetField(name,Any).GetValue(Menus089);
 void SetField90(string name,object v)=>typeof(RaceMenus).GetField(name,Any).SetValue(Menus089,v);
 void Call90(string name,params object[] args)=>typeof(RaceMenus).GetMethod(name,Any).Invoke(Menus089,args);
 string Page90=>(string)Field90("page");
 UnityEngine.UI.Button Button90(string name)=>FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None).FirstOrDefault(b=>b.name==name&&b.gameObject.activeInHierarchy);
 IEnumerator Fresh090(){if(File.Exists(Path.Combine(saveDir,Campaign.File)))File.Delete(Path.Combine(saveDir,Campaign.File));yield return Load("StreetLoopGreybox");yield return Menu();Campaign.Load(saveDir);Menus089.ResetPages();}
 bool Silhouetted(){var root=(GameObject)Field90("previewRoot");return root&&root.GetComponentsInChildren<Renderer>(true).All(r=>r.sharedMaterials.All(m=>m&&m.name=="Locked vehicle silhouette"));}
 bool Badge(){var b=(GameObject)Field90("lockBadge");return b&&b.activeInHierarchy;}

 // Part A: locked vehicles are silhouettes with a padlock (Garage, Shop), locked tracks dimmed with a padlock and a grey
 // route, the main menu status line in both states, and RACE from a locked course's scene opening Race Setup elsewhere.
 IEnumerator Locks090(){
  yield return Fresh090();
  foreach(var e in CampaignData.Events.Take(2)){Campaign.Selected=e.Id;Campaign.DebugMarkWon();}Campaign.DebugAddMoney(4200);
  Menus089.Show();yield return new WaitForSecondsRealtime(.3f);yield return Late(()=>Shot("A-main-menu-status"));
  var label=Button90("campaign")?.GetComponentInChildren<UnityEngine.UI.Text>(true).text??"";
  Check(label.Contains("Chapter 1: Street Loop")&&label.Contains("2 of 7 events")&&label.Contains("$"),$"main menu line under CAMPAIGN: '{label.Replace("\n"," / ")}'");
  flow.Save.Settings.unlockEverything=true;Campaign.Testing=true;Menus089.Show();yield return new WaitForSecondsRealtime(.3f);yield return Late(()=>Shot("A-main-menu-testing"));
  label=Button90("campaign")?.GetComponentInChildren<UnityEngine.UI.Text>(true).text??"";Check(label.Contains("TESTING: everything unlocked"),$"Testing on: '{label.Replace("\n"," / ")}'");
  flow.Save.Settings.unlockEverything=false;Campaign.Testing=false;flow.Save.SaveSettings();
  // Garage: a locked car and the mower, both silhouettes with the padlock
  flow.OpenGarage();SetField90("garageLocked","tourer");yield return Page089("A-garage-locked-silhouette");
  Check(Silhouetted()&&Badge(),$"Garage: a locked Longroof GT is a silhouette {Silhouetted()} with the padlock and how to get it {Badge()}");
  SetField90("garageLocked",null);Menus089.Show();yield return new WaitForSecondsRealtime(.3f);
  Check(!Silhouetted()&&!Badge(),"Garage: the player's own vehicle in its real colours, no padlock");
  flow.CloseGarage();yield return null;
  // Shop: not owned = silhouette, stat bars and price shown; owned = real
  Call90("OpenCampaignShop");SetField90("shopVehicle","skyfin");yield return Page089("A-shop-locked");
  var stats=(RectTransform)Field90("statBlock");Check(Silhouetted()&&Badge()&&stats&&stats.gameObject.activeInHierarchy,$"Shop: the Skyfin Cruiser (not owned) is a silhouette {Silhouetted()} with the padlock {Badge()}; stat bars shown {stats&&stats.gameObject.activeInHierarchy}");
  SetField90("shopVehicle","atv");Menus089.Show();yield return new WaitForSecondsRealtime(.3f);Check(!Silhouetted()&&!Badge(),"Shop: the Trail Four (owned) in its real colours");
  Call90("ShopBack");yield return null;Menus089.ResetPages();
  // Tracks: dimmed rows with a padlock; a locked course's route grey on the map
  flow.OpenCourses();yield return null;var row=Button90("course-4");EventSystem.current.SetSelectedGameObject(row.gameObject);yield return new WaitForSecondsRealtime(.4f);
  yield return Late(()=>Shot("A-tracks-locked-grey"));
  var overlay=FindAnyObjectByType<WorldMapCourseOverlay>();int padlocks=FindObjectsByType<PadlockMark>(FindObjectsSortMode.None).Count(p=>p.isActiveAndEnabled);
  Check(overlay&&overlay.Locked&&padlocks==7,$"Tracks: highlighted Mountain Loop - Forward drawn grey {overlay?.Locked}; padlocks on {padlocks} locked rows");
  EventSystem.current.SetSelectedGameObject(Button90("course-0").gameObject);yield return new WaitForSecondsRealtime(.3f);Check(!overlay.Locked,"Tracks: the open Street Loop - Forward in colour");
  flow.CloseGarage();yield return null;
  // RACE from a locked course's scene (reached through Free Roam): Race Setup opens on an open course
  yield return EnterRoam("MountainLoop","atv");flow.Pause();yield return null;flow.QuitRace();float t0=Time.realtimeSinceStartup;
  while((Scene!="MountainLoop"||flow==null||!flow.Started||LoadingScreen.Holding)&&Time.realtimeSinceStartup-t0<60){yield return null;Bind();}
  yield return Menu();Menus089.ResetPages();Menus089.Show();yield return null;
  Button90("race").onClick.Invoke();t0=Time.realtimeSinceStartup;yield return null;
  while((Scene=="MountainLoop"||flow==null||!flow.Started||LoadingScreen.Holding)&&Time.realtimeSinceStartup-t0<60){yield return null;Bind();}
  yield return new WaitForSecondsRealtime(.5f);yield return Late(()=>Shot("A-race-setup-from-locked"));
  Check(Scene=="StreetLoopGreybox"&&Page90=="race"&&Button90("start")&&Button90("start").interactable,$"RACE in the (locked) Mountain Loop scene opened Race Setup on {Scene}, page '{Page90}', START enabled {Button90("start")?.interactable}");}

 // Part B: a fresh save from the title: the welcome panel, Start the campaign, the first event's page, the controls card
 // holding the countdown until one press, one in-play hint; CAMPAIGN the default selection; the Settings rows.
 IEnumerator Hints090(){
  yield return Fresh090();flow.Save.Settings.hints=true;flow.Save.Settings.hintsSeen=new string[0];flow.Save.SaveSettings();
  Menus089.Show();yield return new WaitForSecondsRealtime(.3f);
  Check(EventSystem.current.currentSelectedGameObject?.name=="campaign",$"main menu default selection for a new player: '{EventSystem.current.currentSelectedGameObject?.name}'");
  flow.EnterFreeRoamAfterTitle();yield return new WaitForSecondsRealtime(.4f);yield return Late(()=>Shot("B-welcome"));
  Check(Page90=="welcome"&&Scene=="StreetLoopGreybox"&&Button90("welcome-campaign")&&Button90("welcome-roam"),$"after the title: welcome panel '{Page90}' with Start the campaign / Look around first");
  Button90("welcome-campaign").onClick.Invoke();yield return new WaitForSecondsRealtime(.4f);yield return Late(()=>Shot("B-first-event-page"));
  Check(Page90=="campaign-event"&&(string)Field90("campaignEvent")=="c1-first-lap"&&Hints.Seen("welcome"),$"Start the campaign: page '{Page90}', event '{Field90("campaignEvent")}'");
  Button90("start-event").onClick.Invoke();float t0=Time.realtimeSinceStartup;yield return null;
  while((flow==null||!flow.Started||LoadingScreen.Holding||flow.State!=RaceFlow.Stage.Countdown)&&Time.realtimeSinceStartup-t0<90){yield return null;Bind();AudioListener.volume=0;}
  yield return new WaitForSecondsRealtime(1.5f);float held=flow.CountdownRemaining;yield return new WaitForSecondsRealtime(1.5f);
  yield return Late(()=>Shot("B-controls-card"));
  Check(flow.ControlsCard&&Mathf.Approximately(held,flow.CountdownRemaining)&&flow.State==RaceFlow.Stage.Countdown,$"controls card up, countdown held at {flow.CountdownRemaining:F1}");
  var kb=InputSystem.AddDevice<Gamepad>("Report090 pad");InputSystem.QueueStateEvent(kb,new GamepadState().WithButton(GamepadButton.South));yield return null;yield return null;
  InputSystem.QueueStateEvent(kb,new GamepadState());yield return null;
  t0=Time.realtimeSinceStartup;while(flow.State!=RaceFlow.Stage.Racing&&Time.realtimeSinceStartup-t0<8)yield return null;
  Check(!flow.ControlsCard&&Hints.Seen("controls")&&flow.State==RaceFlow.Stage.Racing,$"one press dismissed it; the countdown ran: {flow.State}");
  InputSystem.RemoveDevice(kb);
  // sitting still on the grid: the stuck hint once, small and timed
  t0=Time.realtimeSinceStartup;while(!Hints.Visible&&Time.realtimeSinceStartup-t0<25)yield return null;
  yield return Late(()=>Shot("B-hint-stuck"));
  Check(Hints.Visible&&Hints.Text.Contains("Stuck")&&Hints.Seen("stuck"),$"in-play hint: '{Hints.Text}'");
  flow.Pause();yield return null;flow.QuitRace();yield return null;
  flow.OpenSettings();yield return Page089("B-settings-hints");
  Check(Button90("hints")&&Button90("hints-again"),$"Settings > Gameplay: '{Button90("hints")?.GetComponentInChildren<UnityEngine.UI.Text>().text}', '{Button90("hints-again")?.GetComponentInChildren<UnityEngine.UI.Text>().text}'");
  Button90("hints-again").onClick.Invoke();yield return null;Check(!Hints.AnySeen,"Show hints again: every hint can show again");
  flow.CloseSettings();yield return null;flow.Save.Settings.hints=false;flow.Save.SaveSettings();}

 // Part C: the Street Cup on an isolated save, played start to finish by the race AI on the Trail Four, with a quit to
 // the campaign screen and a resume after round 1; standings, payout and the trophy.
 IEnumerator Cups090(){
  yield return Fresh090();foreach(var e in CampaignData.InChapter(1)){Campaign.Selected=e.Id;Campaign.DebugMarkWon();}
  var cup=CampaignData.FindCup("cup-street");int money0=Campaign.Current.money;
  Check(Campaign.CupOpen(cup)&&!Campaign.CupOpen(CampaignData.FindCup("cup-woodland")),"Street Cup open once the chapter 1 final is passed; the Woodland Cup still locked");
  Menus089.OpenCampaign();SetField90("campaignChapter",1);Campaign.Selected=cup.Id;yield return Page089("C-campaign-chapter1-cup");
  SetField90("campaignCup",cup.Id);Call90("Navigate","campaign-cup");yield return Page089("C-cup-page");
  SetField90("campaignVehicle","atv");Menus089.Show();yield return null;Button90("cup-start").onClick.Invoke();
  for(int round=0;round<cup.Rounds.Length;round++){
   yield return EventRunning089();
   Check(CampaignRun.Cup==cup&&CampaignRun.Active.Round==round&&race.Racers.Count==6&&race.Racers.Skip(1).Select(r=>r.Name).SequenceEqual(CampaignData.RivalNames)&&Scene==RacePlaylists.Scenes[cup.Rounds[round].Course],
    $"round {round+1}: {Scene}, {race.Racers.Count} racers [{string.Join(", ",race.Racers.Skip(1).Select(r=>r.Name+" "+r.Car.GetComponent<VehicleConfiguration>().profileId))}], {race.laps} laps, {WorldLook.Current.RaceTime}/{WorldLook.Current.RaceWeather}");
   var pilot=Pilot089(2);yield return Results089(900,3);Unpilot089(pilot);
   if(flow.State==RaceFlow.Stage.Racing&&race.Progress.Finished){race.FinalizeUnfinishedAi();yield return Results089(10);}
   var o=CampaignRun.Last;var progress=Campaign.CupResult(cup.Id);
   yield return Page089($"C-cup-round{round+1}-results");
   Check(o!=null&&o.Places!=null&&progress.races.Count==round+1,$"round {round+1} result: {o?.Headline}, places [{(o?.Places==null?"":string.Join(",",o.Places))}], table: {string.Join(" | ",Campaign.Standings(progress).Select(s=>Campaign.DriverName(s.driver)+" "+s.points))}");
   if(round==0){
    Check(Button90("next-race")&&Button90("campaign-retry")==null,"round 1 results: Next race offered, no retry");
    flow.QuitRace();yield return null;Menus089.Show();yield return new WaitForSecondsRealtime(.3f);yield return Late(()=>Shot("C-campaign-cup-in-progress"));
    var rowLabel=Button90("ccup-"+cup.Id)?.GetComponentInChildren<UnityEngine.UI.Text>(true).text??"";
    var saved=JsonUtility.FromJson<Campaign.State>(File.ReadAllText(Path.Combine(saveDir,Campaign.File)));
    Check(rowLabel.Contains("Round 2 of 3")&&saved.cups.Any(c=>c.id==cup.Id&&c.active&&c.races.Count==1),$"quit after round 1: saved ({saved.cups.FirstOrDefault()?.races.Count} race), row '{rowLabel.Replace("\n"," / ")}'");
    Button90("continue").onClick.Invoke();yield return new WaitForSecondsRealtime(.3f);Check(Page90=="campaign-cup"&&Button90("cup-resume"),"Continue opens the championship with Resume");
    Button90("cup-resume").onClick.Invoke();}
   else if(round<cup.Rounds.Length-1)Button90("next-race").onClick.Invoke();
   else{var fin=Campaign.CupResult(cup.Id);
    Check(o.CupFinished&&fin.finished&&!fin.active&&o.Pay>0&&Campaign.Current.money==money0+o.Pay+o.Bonus,$"final: {Campaign.Ordinal(o.CupPosition)} overall, pay {o.Pay} + bonus {o.Bonus}, trophy {fin.won}, money {Campaign.Current.money}");
    flow.QuitRace();yield return null;Menus089.OpenCampaign();SetField90("campaignChapter",1);yield return Page089("C-campaign-cup-trophy");}}}

 // Part C: one upgrade bought and felt: the Street Classic's standing start to 30 m/s and its speed at the Hwy 92 trap,
 // stock and then with Top speed and Acceleration at level 3; Race outside the campaign stays stock.
 IEnumerator Upgrade090(){
  yield return Fresh090();Campaign.Selected="c1-first-lap";Campaign.DebugMarkWon();Campaign.DebugAddMoney(30000);
  var e=CampaignData.Find("c1-hwy92-trap");var p=VehicleProfile.Find("original");var rows=new List<string>();float stockTop=0,stockAccel=0;
  for(int pass=0;pass<2;pass++){
   if(pass==1){for(int s=0;s<2;s++)for(int l=0;l<3;l++)Campaign.BuyUpgrade(p,s);
    Call90("OpenCampaignShop");SetField90("shopVehicle","original");yield return Page089("C-shop-upgraded");Call90("ShopBack");yield return null;}
   flow.StartCampaignEvent(e,"original");yield return EventRunning089();var car=race.vehicle;
   float t0=Time.time,reach=-1;bool done=false;var c=StartCoroutine(Wrap089(Floor089(e.TimeLimit+2),()=>done=true));
   while(!done&&flow.State==RaceFlow.Stage.Racing&&Time.time-t0<e.TimeLimit+4){if(reach<0&&car.ForwardSpeed>=30)reach=Time.time-t0;yield return null;}
   yield return Results089(10);var o=CampaignRun.Last;
   if(pass==0){stockTop=car.topSpeed;stockAccel=car.acceleration;}
   string line=$"{(pass==0?"stock":"upgraded ("+Campaign.UpgradeSummary("original")+")")}: motor top speed {car.topSpeed:F1}, acceleration {car.acceleration:F2}; 0-30 m/s {reach:F2} s; trap {o?.Score:F2} m/s";rows.Add(line);Note(line);
   flow.QuitRace();yield return null;
   if(pass==1)Check(Mathf.Abs(race.vehicle.topSpeed-stockTop)<.01f&&Mathf.Abs(race.vehicle.acceleration-stockAccel)<.01f,$"after the event the Race vehicle is stock again: top speed {race.vehicle.topSpeed:F1}, acceleration {race.vehicle.acceleration:F2}");}
  File.WriteAllLines(output+"/upgrade-felt.txt",rows);}

 // Part C: the class event lists only motorcycles and the ATV; every chapter 2-4 event and championship is defined.
 IEnumerator Events090(){
  yield return Fresh090();Campaign.DebugUnlockAll();var two=CampaignData.Find("c3-two-wheels");
  var list=((VehicleProfile[])typeof(RaceMenus).GetMethod("EventVehicles",Any).Invoke(Menus089,new object[]{two})).Select(v=>v.Class).Distinct().ToArray();
  Check(list.All(c=>c=="Motorcycle"||c=="ATV")&&list.Length==2,$"Two Wheels Only offers: {string.Join(", ",list)}");
  for(int ch=2;ch<=4;ch++)Note($"chapter {ch}: "+string.Join(" | ",CampaignData.InChapter(ch).Select(x=>$"{x.Name} ({x.KindLabel}, {x.CourseTitle}, {x.Conditions})")));
  Menus089.OpenCampaign();foreach(int ch in new[]{2,3,4}){SetField90("campaignChapter",ch);yield return Page089("C-campaign-chapter"+ch);}}

 // Part C: the ending: rounds 1-7 of the Grand Championship recorded as wins, the last round left early (a did-not-finish
 // that still leaves the player first on points), the results with the Champion of Woodstock screen.
 IEnumerator Champion090(){
  yield return Fresh090();Campaign.DebugUnlockAll();var cup=CampaignData.FindCup("cup-grand");Campaign.StartCup(cup,"atv");
  for(int i=0;i<cup.Rounds.Length-1;i++)Campaign.CommitCupRace(new Campaign.Outcome{Event=CampaignRun.RoundEvent(cup,i),Places=new[]{1,2,3,4,5,6},Time=200,RaceSeconds=200});
  flow.StartCupRound(cup);yield return EventRunning089();yield return new WaitForSeconds(3);
  flow.Pause();yield return null;flow.QuitRace();yield return new WaitForSecondsRealtime(.5f);
  var o=CampaignRun.Last;Check(flow.State==RaceFlow.Stage.Results&&o!=null&&o.CupFinished&&o.Won&&Campaign.Current.complete,$"Grand Championship: last round left early (DNF), final {Campaign.Ordinal(o?.CupPosition??0)}, campaign complete {Campaign.Current.complete}");
  yield return Page089("C-grand-final-results");Button90("champion")?.onClick.Invoke();yield return Page089("C-champion-of-woodstock");
  flow.QuitRace();yield return null;Menus089.Show();yield return null;var label=Button90("campaign")?.GetComponentInChildren<UnityEngine.UI.Text>(true).text??"";Note("main menu after: "+label.Replace("\n"," / "));}

 // Part C: Dan's real campaign save (a copy) loads and carries on: money, vehicles, chapter 1 results, the Needle 600; the
 // Street Cup open; chapter 2 open; written as version 2 only when it is next saved, and that loads again.
 IEnumerator Dan090(string copy){
  yield return Load("StreetLoopGreybox");yield return Menu();
  var target=Path.Combine(saveDir,Campaign.File);File.Copy(copy,target,true);string before=File.ReadAllText(target);
  Campaign.Load(saveDir);var st=Campaign.Current;
  Check(Campaign.Error==null&&st.money==13000&&st.owned.Contains("moto")&&st.owned.Count==3&&st.chapter==2&&st.results.Count==6&&st.results.All(r=>r.passed)&&st.version==Campaign.Version,
   $"Dan's save loaded: money {st.money}, owned [{string.Join(",",st.owned)}], chapter {st.chapter}, results {st.results.Count} passed, version {st.version} in memory, earned {st.earned}, racing {st.secondsRacing:F0} s");
  Check(File.ReadAllText(target)==before,"loading did not write the file");
  Check(Campaign.CupOpen(CampaignData.FindCup("cup-street"))&&Campaign.Available(CampaignData.Find("c2-into-woods"))&&!Campaign.Available(CampaignData.Find("c2-cave-run")),"Street Cup open; chapter 2 open at its first event");
  Menus089.ResetPages();Menus089.Show();yield return new WaitForSecondsRealtime(.3f);yield return Late(()=>Shot("C-dan-main-menu"));
  Note("main menu: "+(Button90("campaign")?.GetComponentInChildren<UnityEngine.UI.Text>(true).text??"").Replace("\n"," / "));
  Menus089.OpenCampaign();SetField90("campaignChapter",1);yield return Page089("C-dan-campaign-chapter1");SetField90("campaignChapter",2);yield return Page089("C-dan-campaign-chapter2");
  Check(Campaign.Save(),"saved once (as version 2)");Campaign.Load(saveDir);
  Check(Campaign.Error==null&&Campaign.Current.money==13000&&Campaign.Current.results.Count==6&&JsonUtility.FromJson<Campaign.State>(File.ReadAllText(target)).version==2,"the version 2 file loads again with the same progress");}
}}
#endif
