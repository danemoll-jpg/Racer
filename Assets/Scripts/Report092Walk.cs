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
// 0.92 controller checks (the 0.91 emulated gamepad, D-pad / A / B / LB / RB / Start; Dan's save only ever copied):
//  save92        Dan's save copy: the prize granted on load (said once, OK with A), the four finals' prizes
//  prize92       passing chapter 2's final awards the Pebble Coupe; a save that bought it is refunded
//  pick92        campaign: the vehicle in the garage view, the Shop from it, the event started, the next event preselected
//  splitpick92   split-screen: the players' garages with the AI as player 2, then with two controllers at once
//  noacycle92    one pass through the menus pressing A on every value row: none changes (left / right do)
//  split92:course:time:weather:rivals:traffic(0/1):lr(0/1)   a split-screen stage-2 race, results, rematch, change setup
public sealed partial class Report080Checks {
 IEnumerator Walk092(string[] a)=>a[0] switch{"save92"=>Save092(),"prize92"=>Prize092(),"pick92"=>Pick092(),"splitpick92"=>SplitPick092(),"noacycle92"=>NoACycle092(),"paint92"=>Paint092(),
  "celebrate92"=>Celebrate092(a[1]=="p1"),"split92"=>Split092(int.Parse(a[1]),(TimeOfDay)Enum.Parse(typeof(TimeOfDay),a[2]),(Weather)Enum.Parse(typeof(Weather),a[3]),int.Parse(a[4]),a[5]=="1",a[6]=="1"),_=>null};
 // the champion's paint (offered while Testing is on, as when the Grand Championship is won): gold with a roundel each side
 IEnumerator Paint092(){
  Pad091();yield return Load("StreetLoopGreybox");yield return Menu();flow.Save.Settings.unlockEverything=true;Campaign.Testing=true;
  foreach(var id in new[]{"original","moto"}){flow.OpenGarage();flow.SelectVehicle(id);flow.SetColor(VehiclePaint.Champion);yield return Settle091();yield return new WaitForSecondsRealtime(.5f);
   var roundels=race.vehicle.transform.Find("Champion roundel");var preview=GameObject.Find("Garage display model");
   Check(roundels&&roundels.childCount==2&&preview&&preview.transform.Find("Champion roundel")&&Text092("colour")!=null&&Text092("colour").Contains("Champion"),$"{VehicleProfile.Find(id).Name}: the champion's scheme on the vehicle and the garage preview (roundels {roundels?.childCount}), Colour row '{Text092("colour")}'");
   yield return Late(()=>Shot("D-champion-paint-"+id));flow.SetColor(1);flow.CloseGarage();yield return Settle091();}
  Check(race.vehicle.transform.Find("Champion roundel")==null,"another colour removes the roundels");
  flow.Save.Settings.unlockEverything=false;Campaign.Testing=false;flow.Save.SaveSettings();}
 // Dan's save copy on the main menu; the one-time "campaign updated" notice (if shown) closed with A
 IEnumerator Start092(){yield return Start091();for(int i=0;i<3&&Menus089.ModalOpen;i++){yield return Press091(GamepadButton.A);yield return Settle091();}}
 string Text092(string id)=>Button90(id)?.GetComponentInChildren<UnityEngine.UI.Text>(true).text;

 IEnumerator Save092(){
  Pad091();yield return Load("StreetLoopGreybox");yield return Menu();var before=Campaign.Current;
  var copy=Environment.GetEnvironmentVariable("PROBE_DANSAVE");File.Copy(copy,Path.Combine(saveDir,Campaign.File),true);
  int money=JsonUtility.FromJson<Campaign.State>(File.ReadAllText(copy)).money;Campaign.Load(saveDir);
  Check(Campaign.Owns("pebble")&&Campaign.Current.money==money&&Campaign.LoadNotice!=null,$"Dan's save copy: the Forest Final passed, so the Pebble Coupe is granted (owned [{string.Join(",",Campaign.Current.owned)}]); money unchanged {Campaign.Money(Campaign.Current.money)} (he never bought it); notice: {Campaign.LoadNotice?.Replace("\n"," / ")}");
  var saved=JsonUtility.FromJson<Campaign.State>(File.ReadAllText(Path.Combine(saveDir,Campaign.File)));
  Check(saved.version==2&&!saved.owned.Contains("pebble"),$"loading never writes: the file is still version {saved.version}");
  Menus089.ResetPages();Menus089.Show();yield return Settle091();yield return Press091(GamepadButton.DpadDown);yield return Settle091();
  Check(Button90("ok")&&Menus089.ModalOpen,$"the main menu shows it once: '{Field90("modalTitle")}' with one OK row");yield return Late(()=>Shot("D-campaign-updated"));
  yield return Press091(GamepadButton.A);saved=JsonUtility.FromJson<Campaign.State>(File.ReadAllText(Path.Combine(saveDir,Campaign.File)));
  Check(!Menus089.ModalOpen&&Campaign.LoadNotice==null&&saved.version==3&&saved.owned.Contains("pebble")&&saved.money==money,$"A on OK closes it and writes the save (version {saved.version}, Pebble Coupe owned, money {Campaign.Money(saved.money)}), so it is said once");
  Campaign.Load(saveDir);Check(Campaign.LoadNotice==null,"loading again: nothing more to say");
  var finals=CampaignData.Events.Where(e=>e.Final).ToArray();
  Check(finals.Length==4&&finals.All(e=>e.Prize!=null),"every chapter final awards a vehicle: "+string.Join(", ",finals.Select(e=>$"{e.Name} > {VehicleProfile.Find(e.Prize).Name}")));
  Check(CampaignData.Prices.Length==4&&!CampaignData.Prices.Any(p=>CampaignData.Events.Any(e=>e.Prize==p.id)),"the Shop sells four: "+string.Join(", ",CampaignData.Prices.Select(p=>VehicleProfile.Find(p.id).Name+" "+Campaign.Money(p.price))));
  // the campaign screen: chapter 2's final (won: revealed) and chapter 3's (a silhouette marked PRIZE)
  foreach(var id in new[]{"c2-forest-final","c3-backyard-final","c4-summit-final"}){var e=CampaignData.Find(id);Campaign.Selected=id;Menus089.OpenCampaign(e.Chapter);yield return Settle091();var row=Button90("cev-"+id);if(row)EventSystem.current.SetSelectedGameObject(row.gameObject);yield return Settle091();
   var overlay=GameObject.Find("Prize preview");var caption=overlay?overlay.GetComponentInChildren<UnityEngine.UI.Text>(true).text:null;
   Check(overlay&&overlay.activeInHierarchy&&caption!=null&&caption.Contains(VehicleProfile.Find(e.Prize).Name),$"campaign screen, {e.Name} highlighted: the prize beside the map '{caption}' ({(Campaign.Owns(e.Prize)?"won, shown":"a silhouette until won")})");
   yield return Late(()=>Shot("D-campaign-prize-"+id));}
  Campaign.Selected="c3-backyard-final";typeof(RaceMenus).GetField("campaignEvent",Any).SetValue(Menus089,"c3-backyard-final");typeof(RaceMenus).GetField("page",Any).SetValue(Menus089,"campaign-event");Menus089.Show();yield return Settle091();
  Check(GameObject.Find("Preview scrambler"),"the Backyard Final's page: the Ridge Scrambler as a PRIZE card (silhouette)");yield return Late(()=>Shot("D-event-prize"));
  Menus089.ResetPages();Menus089.Show();}

 IEnumerator Prize092(){
  yield return Load("StreetLoopGreybox");yield return Menu();
  // passing (2nd) chapter 2's final on a campaign that has not
  var file=Path.Combine(saveDir,Campaign.File);if(File.Exists(file))File.Delete(file);Campaign.Load(saveDir);Campaign.Testing=false;flow.Save.Settings.unlockEverything=false;
  var e=CampaignData.Find("c2-forest-final");var o=Campaign.Commit(new Campaign.Outcome{Event=e,Vehicle="original",Place=2,Field=6,Time=300});
  Check(o.Passed&&Campaign.Owns("pebble")&&o.PrizeWon=="pebble"&&o.Unlocked.Contains("Vehicle: Pebble Coupe"),$"2nd in the Forest Final: passed, the Pebble Coupe won ({string.Join("; ",o.Unlocked)}); bonus {o.Bonus} (first win only)");
  // its results page shows it revealed
  Menus089.ResetPages();
  // a save from 0.91 that bought the Pebble Coupe: refunded on load, kept
  var st=new Campaign.State{version=2,money=1000,owned=new List<string>{"original","atv","pebble"},courses=new List<string>{RacePlaylists.Scenes[0]}};File.WriteAllText(file,JsonUtility.ToJson(st));
  Campaign.Load(saveDir);Check(Campaign.Current.money==7000&&Campaign.Owns("pebble")&&Campaign.LoadNotice!=null&&Campaign.LoadNotice.Contains("refunded"),$"a 0.91 save that bought it: {Campaign.Money(1000)} > {Campaign.Money(Campaign.Current.money)}, still owned; '{Campaign.LoadNotice}'");
  Campaign.LoadNotice=null;File.Delete(file);Campaign.Load(saveDir);
  // the champion's paint is offered only once the Grand Championship is won
  Check(VehiclePaint.Count==VehiclePaint.Colors.Length,"the champion's paint is not offered before the Grand Championship is won");}

 IEnumerator Pick092(){
  yield return Start092();
  yield return Choose091("campaign");yield return Choose091("continue");
  Check(Ready91("campaign-event"),$"Continue: the next event's page ({Field90("campaignEvent")})");
  string first=(string)Field90("campaignVehicle");Check(GameObject.Find("Event vehicles")&&GameObject.Find("Preview "+first),$"the event page shows the chosen vehicle as a small preview ({VehicleProfile.Find(first).Name})");
  yield return Late(()=>Shot("B-event-page"));
  yield return Choose091("event-vehicle");Check(Ready91("vehicle-pick")&&Button90("pick-use"),"A on the vehicle row: the garage view");
  yield return Walk091("vehicle choice (garage view)",true);
  string v1=(string)Field90("pickVehicle");yield return Goto091("pick-vehicle");yield return Press091(GamepadButton.DpadRight);string v2=(string)Field90("pickVehicle");
  Check(v1!=v2,$"right: the next owned vehicle ({v1} > {v2}), its stats and preview");yield return Late(()=>Shot("B-vehicle-pick"));
  // dimmed: an owned vehicle the event does not allow (if any) cannot be used
  string useText=Text092("pick-use");bool usable=Button90("pick-use")&&Button90("pick-use").interactable;
  Note($"  {VehicleProfile.Find(v2).Name}: '{useText}', usable {usable}");
  for(int i=0;i<8&&!(Button90("pick-use")&&Button90("pick-use").interactable);i++){yield return Goto091("pick-vehicle");yield return Press091(GamepadButton.DpadRight);}
  string chosen=(string)Field90("pickVehicle");
  // the Shop is one press away, and comes back here
  yield return Choose091("pick-shop");Check(flow.State==RaceFlow.Stage.Garage&&Page90=="shop","Shop… from the garage view: the Shop");
  yield return Press091(GamepadButton.East);Check(Ready91("vehicle-pick"),$"B from that Shop: back to the garage view ({flow.State} / '{Page90}')");
  yield return Choose091("pick-use");Check(Ready91("campaign-event")&&(string)Field90("campaignVehicle")==chosen,$"USE THIS VEHICLE: back to the event page with the {VehicleProfile.Find(chosen).Name}");
  yield return Choose091("start-event");yield return EventRunning089();Check(CampaignRun.Vehicle==chosen,$"the event runs in the {VehicleProfile.Find(chosen).Name}");
  yield return Press091(GamepadButton.Start);yield return Choose091("return");if(Button90("confirm"))yield return Choose091("confirm");yield return Settle091();
  Check(Ready91("campaign"),$"left from the pause menu: the campaign screen ('{Page90}')");
  // another event: opens on the vehicle last driven in the campaign
  var other=CampaignData.Events.First(x=>Campaign.Available(x)&&x.Id!=(string)Field90("campaignEvent")&&x.Allows(VehicleProfile.Find(chosen)));
  typeof(RaceMenus).GetField("campaignChapter",Any).SetValue(Menus089,other.Chapter);Menus089.Show();yield return Settle091();
  yield return Goto091("cev-"+other.Id);if(Sel91Name!="cev-"+other.Id){yield return Choose091("chapter-"+other.Chapter);yield return Goto091("cev-"+other.Id);}
  yield return Press091(GamepadButton.A);
  Check(Ready91("campaign-event")&&(string)Field90("campaignVehicle")==chosen,$"{other.Name}'s page opens on the vehicle last driven: {VehicleProfile.Find((string)Field90("campaignVehicle")).Name}");
  yield return Press091(GamepadButton.East);yield return Press091(GamepadButton.East);}

 IEnumerator SplitPick092(){
  yield return Start092();SplitScreen.P1Device=null;SplitScreen.P2Device=null;SplitScreen.P2Ai=true;
  yield return Choose091("split");Check(Ready91("split")&&GameObject.Find("Players' vehicles"),"the setup screen shows both players' vehicles as previews");yield return Late(()=>Shot("B-split-setup"));
  yield return Choose091("split-vehicles");Check(Ready91("split-garage"),"A on VEHICLES AND COLOURS: the players' garages");
  string p1=SplitScreen.P1Vehicle,p2=SplitScreen.P2Vehicle;int c1=SplitScreen.P1Color;
  yield return Press091(GamepadButton.DpadRight);yield return Press091(GamepadButton.DpadDown);
  Check(SplitScreen.P1Vehicle!=p1&&SplitScreen.P1Color!=c1&&SplitScreen.P2Vehicle==p2,$"player 1's pad: right the vehicle ({p1} > {SplitScreen.P1Vehicle}), down the colour ({VehiclePaint.Name(c1)} > {VehiclePaint.Name(SplitScreen.P1Color)}); player 2's unchanged");
  yield return Late(()=>Shot("B-split-garage-ai"));
  yield return Press091(GamepadButton.A);Check((int)Field90("pickFocus")==2,"A: player 1 ready, now the AI's half");
  yield return Press091(GamepadButton.DpadRight);Check(SplitScreen.P2Vehicle!=p2,$"player 1 chooses the AI's vehicle too ({p2} > {SplitScreen.P2Vehicle})");
  yield return Press091(GamepadButton.A);Check(Ready91("split"),"A: both chosen, back to the setup");
  // two controllers: player 2 joins with A, both choose at once in their own halves
  var pad2=InputSystem.AddDevice<Gamepad>("Report092 pad 2");
  IEnumerator Tap2(GamepadButton b){InputSystem.QueueStateEvent(pad2,new GamepadState().WithButton(b));yield return null;yield return null;InputSystem.QueueStateEvent(pad2,new GamepadState());yield return Settle091();}
  yield return Tap2(GamepadButton.A);Check(SplitScreen.P2Device==pad2&&!SplitScreen.P2Ai,"A on a second controller: player 2 joins");
  yield return Choose091("split-vehicles");Check(Ready91("split-garage"),"the players' garages, two players");
  p1=SplitScreen.P1Vehicle;p2=SplitScreen.P2Vehicle;
  InputSystem.QueueStateEvent(pad91,new GamepadState().WithButton(GamepadButton.DpadRight));InputSystem.QueueStateEvent(pad2,new GamepadState().WithButton(GamepadButton.DpadLeft));yield return null;yield return null;
  InputSystem.QueueStateEvent(pad91,new GamepadState());InputSystem.QueueStateEvent(pad2,new GamepadState());yield return Settle091();
  Check(SplitScreen.P1Vehicle!=p1&&SplitScreen.P2Vehicle!=p2,$"both at once: player 1 right ({p1} > {SplitScreen.P1Vehicle}), player 2 left ({p2} > {SplitScreen.P2Vehicle})");
  yield return Late(()=>Shot("B-split-garage-two"));
  yield return Tap2(GamepadButton.A);Check(Ready91("split-garage")&&(bool)Field90("pickReady2"),"player 2 ready (A on its controller)");
  yield return Tap2(GamepadButton.B);Check(!(bool)Field90("pickReady2")&&Ready91("split-garage"),"player 2's B takes its ready back, the screen stays");
  yield return Tap2(GamepadButton.A);yield return Press091(GamepadButton.A);Check(Ready91("split"),"both ready: back to the setup");
  InputSystem.RemoveDevice(pad2);yield return Settle091();yield return Press091(GamepadButton.East);}

 // A on every value row of a page: the value and the page stay (a row with a list may open it; B returns); right changes it.
 int cycledRows,aChanged;readonly List<string> converted=new();
 IEnumerator ValueRows092(string label){
  yield return Settle091();var adj=(Dictionary<int,Action<int>>)Field90("adjustments");var lists=new HashSet<int>((HashSet<int>)Field90("listRows"));var btns=(List<UnityEngine.UI.Button>)Field90("buttons");
  var rows=adj.Keys.Where(i=>i<btns.Count&&btns[i].gameObject.activeInHierarchy&&btns[i].interactable).Select(i=>(i,id:btns[i].name)).ToList();int changed=0;
  foreach(var (i,id) in rows){if(id=="unlock-everything"||id=="channel"||id=="source"||id=="trailer-toggle")continue;
   var b=Button90(id);if(!b)continue;EventSystem.current.SetSelectedGameObject(b.gameObject);yield return Settle091();
   string before=Text092(id),pg=Page90;var st=flow.State;yield return Press091(GamepadButton.A);yield return Settle091();
   bool opened=lists.Contains(i)&&(Page90!=pg||flow.State!=st);
   if(opened){converted.Add($"{label}/{id} (A opens its list)");yield return Press091(GamepadButton.East);yield return Settle091();continue;}
   if(Text092(id)!=before||Page90!=pg||flow.State!=st){aChanged++;Check(false,$"{label}: A changed '{id}' ({before} > {Text092(id)})");continue;}
   b=Button90(id);if(!b)continue;EventSystem.current.SetSelectedGameObject(b.gameObject);yield return Settle091();yield return Press091(GamepadButton.DpadRight);yield return Settle091();string after=Text092(id);
   b=Button90(id);if(b){EventSystem.current.SetSelectedGameObject(b.gameObject);yield return Settle091();yield return Press091(GamepadButton.DpadLeft);yield return Settle091();}
   if(after!=before)changed++;converted.Add($"{label}/{id}");cycledRows++;}
  Check(true,$"{label}: {rows.Count} value rows, A changed none; right changed {changed} ({string.Join(", ",rows.Select(r=>r.id))})");}
 IEnumerator NoACycle092(){
  yield return Start092();Campaign.LoadNotice=null;
  yield return Choose091("race");yield return ValueRows092("Race Setup");
  yield return Choose091("opponents");yield return ValueRows092("Opponents");yield return Press091(GamepadButton.East);
  yield return Press091(GamepadButton.East);
  yield return Choose091("garage");yield return ValueRows092("Garage");yield return Choose091("rider");yield return ValueRows092("Garage / Rider");yield return Press091(GamepadButton.East);
  yield return Choose091("done");
  yield return Choose091("settings");for(int t=0;t<4;t++){yield return ValueRows092("Settings "+Page90);yield return Press091(GamepadButton.RightShoulder);}yield return Press091(GamepadButton.East);
  yield return Choose091("campaign");yield return Choose091("continue");yield return ValueRows092("Campaign event page");yield return Choose091("event-vehicle");yield return ValueRows092("Campaign vehicle choice");
  yield return Choose091("pick-shop");yield return ValueRows092("Shop");yield return Press091(GamepadButton.East);yield return Press091(GamepadButton.East);yield return Press091(GamepadButton.East);yield return Press091(GamepadButton.East);
  yield return Choose091("split");yield return ValueRows092("Split Screen setup");yield return Press091(GamepadButton.East);
  yield return Choose091("records");yield return ValueRows092("Records");for(int t=0;t<4;t++){yield return Press091(GamepadButton.RightShoulder);yield return ValueRows092("Records tab "+(t+2));}yield return Press091(GamepadButton.East);
  yield return Choose091("race");yield return Choose091("playlists");yield return Choose091("new");yield return Choose091("add");yield return ValueRows092("Playlist editor (entry)");
  yield return Choose091("cancel");Note($"value rows converted and checked: {cycledRows}; changed by A: {aChanged}");File.WriteAllLines(output+"/value-rows.txt",converted);}

 // Part F item 10: no rivals, player 2 the AI driver; p1: player 1 (race AI pilot, Needle 600) wins against player 2 held
 // back 25 s, the winner shot in player 1's half; else player 2 (AI) wins while player 1 waits: it celebrates on the road and
 // no camera is taken. A picture of the winner's half each time (rendered from that half's camera).
 void ViewShot092(Camera cam,string name){var rect=cam.rect;var rt=new RenderTexture(1600,(int)(1600*cam.pixelHeight/(float)Mathf.Max(1,cam.pixelWidth)),24);var was=cam.targetTexture;cam.rect=new Rect(0,0,1,1);cam.targetTexture=rt;cam.Render();cam.targetTexture=was;cam.rect=rect;
  var prev=RenderTexture.active;RenderTexture.active=rt;var tex=new Texture2D(rt.width,rt.height,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);tex.Apply();RenderTexture.active=prev;
  File.WriteAllBytes($"{output}/{name}.png",tex.EncodeToPNG());Destroy(tex);rt.Release();Destroy(rt);}
 IEnumerator Celebrate092(bool p1Wins){
  Pad091();yield return Load("StreetLoopGreybox");yield return Menu();flow.Save.Settings.splitLeftRight=false;flow.Save.SaveSettings();
  SplitScreen.P1Device=pad91;SplitScreen.P2Device=null;SplitScreen.P2Ai=true;SplitScreen.Course=0;SplitScreen.Laps=1;SplitScreen.Rivals=0;SplitScreen.Time=TimeOfDay.Day;SplitScreen.Weather=Weather.Clear;SplitScreen.Traffic=false;
  SplitScreen.P1Vehicle="moto";SplitScreen.P2Vehicle=p1Wins?"atv":"moto";
  flow.StartSplit();yield return SplitRunning090();var split=SplitScreen.Race;var p2=split.P2Car;
  RoadDriver pilot=null;if(p1Wins)pilot=Pilot089(2);else race.vehicle.GetComponent<VehicleInput>().enabled=false;
  if(p1Wins){p2.Body.isKinematic=true;p2.GetComponent<RoadDriver>().enabled=false;}
  float t0=Time.time;bool released=!p1Wins;var winner=p1Wins?race.Racers[0]:split.P2;bool shot=false,celebrated=false,otherShot=false;Time.timeScale=3;
  while(flow.State==RaceFlow.Stage.Racing&&Time.time-t0<400){yield return null;AudioListener.volume=0;
   if(!released&&Time.time-t0>25){released=true;p2.Body.isKinematic=false;p2.GetComponent<RoadDriver>().enabled=true;}
   if(winner.Progress.Finished&&!shot){Time.timeScale=1;yield return new WaitForSeconds(.6f);celebrated=winner.Car.GetComponent<RiderGestures>().Current==RiderGestures.Kind.Celebrate;
    otherShot=split.ShotActive(p1Wins?2:1);bool myShot=split.ShotActive(1);var cam=p1Wins?Camera.main:split.Camera2;ViewShot092(cam,p1Wins?"F-celebration-player1-wins":"F-celebration-ai-wins");shot=true;
    Check(celebrated&&(p1Wins?myShot:!myShot)&&!otherShot,p1Wins?$"player 1 won: raises both fists, the winner shot in player 1's half ({myShot}), player 2's half untouched (still racing: {!split.P2.Progress.Finished})":$"player 2 (the AI driver) won: raises both fists on the road ({celebrated}); no camera taken from player 1 ({!myShot})");
    break;}}
  Time.timeScale=1;if(pilot)Unpilot089(pilot);race.vehicle.GetComponent<VehicleInput>().enabled=true;
  Check(shot,$"{(p1Wins?"player 1":"player 2 (AI)")} finished first");
  // the fist wave on player 1's own controller (LB) in split-screen
  var rg=race.vehicle.GetComponent<RiderGestures>();int before=rg.Waves;yield return new WaitForSeconds(4);
  if(flow.State==RaceFlow.Stage.Racing){InputSystem.QueueStateEvent(pad91,new GamepadState().WithButton(GamepadButton.LeftShoulder));yield return null;yield return null;InputSystem.QueueStateEvent(pad91,new GamepadState());yield return new WaitForSeconds(.3f);
   Check(rg.Waves>before,$"LB on player 1's controller: player 1's rider waves a fist ({before} > {rg.Waves})");}
  flow.Pause();yield return null;flow.QuitRace();yield return null;}
 IEnumerator Split092(int course,TimeOfDay time,Weather weather,int rivals,bool traffic,bool lr){
  Pad091();yield return Load("StreetLoopGreybox");yield return Menu();flow.Save.Settings.splitLeftRight=lr;flow.Save.SaveSettings();
  SplitScreen.P1Device=pad91;SplitScreen.P2Device=null;SplitScreen.P2Ai=true;SplitScreen.Course=course;SplitScreen.Laps=1;SplitScreen.Rivals=rivals;SplitScreen.Time=time;SplitScreen.Weather=weather;SplitScreen.Traffic=traffic;
  string label=$"{RacePlaylists.Titles[course]} {time}/{weather}, {rivals} rivals, traffic {(traffic?"on":"off")}, {(lr?"left / right":"top / bottom")}";
  flow.StartSplit();yield return SplitRunning090();var split=SplitScreen.Race;yield return new WaitForSeconds(5);
  var look=WorldLook.Current;var weatherFx=FindAnyObjectByType<WeatherEffects>();
  ParticleSystem Ps(string f)=>(ParticleSystem)typeof(WeatherEffects).GetField(f,Any).GetValue(weatherFx);
  var cams=SplitScreen.Views;bool masks=cams.Count==2&&(cams[0].cullingMask&(1<<SplitRace.View2Layer))==0&&(cams[1].cullingMask&(1<<SplitRace.View1Layer))==0&&(cams[1].cullingMask&(1<<SplitRace.View2Layer))!=0;
  var r1=Ps("rain");var r2=Ps("rain2");var s1=Ps("snow");var s2=Ps("snow2");
  int falling1=weather==Weather.Rain?r1.particleCount:weather==Weather.Snow?s1.particleCount:0,falling2=weather==Weather.Rain?(r2?r2.particleCount:0):weather==Weather.Snow?(s2?s2.particleCount:0):0;
  Check(race.Racers.Count==2+rivals&&look.RaceTime==time&&look.RaceWeather==weather&&masks&&(weather==Weather.Clear||falling1>0&&falling2>0)&&r1.gameObject.layer==SplitRace.View1Layer&&(!r2||r2.gameObject.layer==SplitRace.View2Layer),
   $"{label}: racing with {race.Racers.Count} (2 players + {rivals} AI); conditions {look.RaceTime}/{look.RaceWeather}; each view's falling weather on its own layer (view 1 {falling1}, view 2 {falling2} particles), camera masks {masks}");
  int cars=FindObjectsByType<AmbientVehicle>(FindObjectsSortMode.None).Count(v=>v.gameObject.activeInHierarchy&&v.GetComponent<RoadDriver>()); /* the Mountain's scenery loop cars are not traffic */Check(traffic?cars>0:cars==0,$"{label}: traffic {cars} cars");
  var lights=FindObjectsByType<VehicleLights>(FindObjectsSortMode.None).Length;Note($"  vehicle lamps on {lights} vehicles, level {VehicleLights.Level:F2}; thunder strikes so far {weatherFx.Strikes}");
  yield return Late(()=>Shot($"F-{time}-{weather}-{(lr?"lr":"tb")}"));
  // pause from the controller and resume
  yield return Press091(GamepadButton.Start);Check(flow.State==RaceFlow.Stage.Paused,"Start: the split-screen pause menu");yield return Walk091("split-screen pause (stage 2)",true);yield return Press091(GamepadButton.Start);
  // player 1 driven by the race AI (3x) to the finish; both players done, the rivals' finishes estimated
  var pilot=Pilot089(1);yield return Results089(600,3);Unpilot089(pilot);
  Check(flow.State==RaceFlow.Stage.Results&&race.Racers.All(r=>r.Classified||r.Dnf),$"{label}: results with everyone classified ({string.Join(", ",race.Ordered(true).Select(r=>(r==race.Racers[0]?"P1":r==split.P2?"P2":r.Name)+(r.Dnf?" DNF":r.Estimated?" est.":"")))})");
  var standings=FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None).Count(b=>b.gameObject.activeInHierarchy&&b.name.StartsWith("split-standing-"));
  Check(standings==2+rivals,$"results table: {standings} rows (every finisher, the AI too)");yield return Late(()=>Shot($"F-results-{time}-{weather}"));
  yield return Choose091("rematch");yield return SplitRunning090();
  Check(SplitScreen.Active&&race.Racers.Count==2+rivals&&WorldLook.Current.RaceWeather==weather,"REMATCH: the same race again (rivals and conditions kept)");
  var p2=Pilot089(1);yield return Results089(600,3);Unpilot089(p2);
  Check(flow.State==RaceFlow.Stage.Results,"the rematch finished: results again");
  yield return Choose091("change-setup");Check(Ready91("split")&&SplitScreen.Rivals==rivals&&SplitScreen.Weather==weather&&SplitScreen.Time==time&&SplitScreen.Traffic==traffic,"Change setup: the setup screen with every choice kept");
  yield return Late(()=>Shot("F-setup-kept"));yield return Press091(GamepadButton.East);
  flow.Save.Settings.splitLeftRight=false;flow.Save.SaveSettings();}
}
}
#endif
