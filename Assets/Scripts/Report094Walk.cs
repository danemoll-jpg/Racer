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
// 0.94 Part A checks (controller only: an emulated gamepad; shots at 3840x2160), on a copy of Dan's save:
//  names94     Dan's copy: named "Dan" without asking, his Top 10 entries shown as "Dan"; Settings > Gameplay > Player name
//              entered with the on-screen keyboard; the three name-tag settings in a race with rivals
//  rivals94    a campaign event: the rivals on its page, the starting grid card, the ahead / behind line, tags, results
//  splitnames94  split-screen: player 2's name entered on the setup screen; both players' times land on the Top 10
public sealed partial class Report080Checks {
 IEnumerator Walk094(string[] a)=>a[0] switch{"names94"=>Names094(),"rivals94"=>Rivals094(a.Length>1?a[1]:"c3-backyard-dash"),"splitnames94"=>SplitNames094(),_=>null};
 // Dan's save files (the copy the runner made of his folder) into the check's own save, then the course reloaded on it
 IEnumerator DanCopy094(){
  var copy=Path.Combine(Environment.GetEnvironmentVariable("PROBE_OUT"),"save-copy");
  foreach(var f in Directory.GetFiles(copy))File.Copy(f,Path.Combine(saveDir,Path.GetFileName(f)),true);
  var st=Path.Combine(saveDir,"settings.json");if(File.Exists(st)){var s=File.ReadAllText(st);Note($"  Dan's settings copy: playerName in file: {s.Contains("\"playerName\"")}");}
  yield return Load("StreetLoopGreybox");yield return Menu();Bind();flow.UseValidationSave(saveDir);Pad091();Menus089.ResetPages();Menus089.Show();yield return Settle091();
  yield return Press091(GamepadButton.DpadDown);yield return Press091(GamepadButton.DpadUp);}
 string CellsText094(){var card=(RectTransform)typeof(RaceMenus).GetField("card",Any).GetValue(Menus089);return string.Join(" | ",card.GetComponentsInChildren<UnityEngine.UI.Text>().Where(t=>t.gameObject.activeInHierarchy&&t.name=="Cell").Select(t=>t.text));}
 // the on-screen keyboard: D-pad from the focused key to a key, then A
 IEnumerator Key094(char c){
  const string keys="1234567890qwertyuiopasdfghjkl;zxcvbnm,./\\:-_@()[]";int target=keys.IndexOf(char.ToLowerInvariant(c));
  for(int guard=0;guard<30;guard++){var n=Sel91Name;if(!n.StartsWith("key-"))break;int at=int.Parse(n.Substring(4));if(at==target)break;
   int dr=target/10-at/10,dc=target%10-at%10;yield return Press091(dr<0?GamepadButton.DpadUp:dr>0?GamepadButton.DpadDown:dc<0?GamepadButton.DpadLeft:GamepadButton.DpadRight);}
  yield return Press091(GamepadButton.A);}
 IEnumerator Names094(){
  yield return DanCopy094();
  Check(flow.Save.Settings.playerName=="Dan"&&PlayerNames.Player=="Dan",$"Dan's existing save is named \"{flow.Save.Settings.playerName}\" without asking (welcome due: {Hints.WelcomeDue})");
  // his Top 10 shows "Dan"
  flow.OpenBoards();yield return Settle091();
  {var has=typeof(RaceMenus).GetMethod("TrackHasTimes",Any);int track=Enumerable.Range(0,RacePlaylists.Titles.Length).FirstOrDefault(t=>(bool)has.Invoke(Menus089,new object[]{t,false}));typeof(RaceMenus).GetField("recordTrack",Any).SetValue(Menus089,track);Menus089.Show();yield return Settle091();}
  var cells=CellsText094();int dans=cells.Split('|').Count(c=>c.Trim()=="Dan");var names=cells.Split('|').Select(c=>c.Trim()).Where(c=>c=="Dan"||c=="Player"||c=="").Distinct();
  Check(dans>0&&!cells.Contains("| Player |"),$"Records: {dans} entries named Dan on the shown board ({(cells.Length>160?cells.Substring(0,160)+"…":cells)})");yield return Late(()=>Shot4k("A-records-Dan"));
  yield return Press091(GamepadButton.East);yield return Settle091();
  // Settings > Gameplay > Player name, the on-screen keyboard with the controller
  flow.OpenSettings();yield return Settle091();yield return Goto091("player-name");yield return Late(()=>Shot4k("A-settings-gameplay"));
  yield return Press091(GamepadButton.A);Check(Page90=="keyboard",$"A on Player name opens the on-screen keyboard (page '{Page90}', focus {Sel91Name})");
  // clear the draft (Delete from the utility row is X), then type "kyle"
  for(int i=0;i<4;i++)yield return Press091(GamepadButton.X);
  foreach(var c in "kyle")yield return Key094(c);
  yield return Late(()=>Shot4k("A-keyboard-entry"));
  for(int i=0;i<20&&Sel91Name!="save";i++)yield return Press091(Sel91Name.StartsWith("key-")?GamepadButton.DpadDown:GamepadButton.DpadRight);
  yield return Press091(GamepadButton.A);
  Check(flow.Save.Settings.playerName=="kyle"&&Page90!="keyboard",$"SAVE: the name is now \"{flow.Save.Settings.playerName}\" (page '{Page90}'); known names {string.Join(", ",PlayerNames.Known)}");
  yield return Late(()=>Shot4k("A-settings-named"));
  PlayerNames.SetPlayer("Dan");
  // the three tag settings: a Race Setup race with three rivals, as each setting shows them
  yield return Press091(GamepadButton.East);yield return Settle091();
  yield return StartRival094("StreetLoopGreybox");
  foreach(var (mode,label) in new[]{(2,"Everyone"),(1,"Players only"),(0,"Off")}){
   flow.Save.Settings.nameTags=mode;yield return new WaitForSeconds(.6f);int shown=NameTags.Shown;
   Check(mode==2?shown>0:shown==0,$"Name tags {label}: {shown} tags shown (three AI rivals ahead, no other player)");yield return Late(()=>Shot4k("A-tags-"+label.Replace(' ','-')));}
  flow.Save.Settings.nameTags=2;yield return Menu();}
 // a Race Setup race on a course with 3 AI rivals, the player driven by the race AI a little behind them
 IEnumerator StartRival094(string scene){
  yield return Load(scene);yield return Menu();flow.Save.Settings.unlockEverything=true;Campaign.Testing=true;flow.OpenGarage();flow.SelectVehicle("original");flow.CloseGarage();
  race.opponents=true;race.traffic=false;race.laps=1;race.opponentRoster=new[]{"tourer","moto","atv"};flow.StartRace();
  float t0=Time.realtimeSinceStartup;while(flow.State!=RaceFlow.Stage.Countdown&&Time.realtimeSinceStartup-t0<40){AudioListener.volume=0;yield return null;}
  yield return new WaitForSeconds(1);
  while(flow.State!=RaceFlow.Stage.Racing&&Time.realtimeSinceStartup-t0<60){AudioListener.volume=0;yield return null;}
  var p1=race.vehicle;p1.GetComponent<VehicleInput>().enabled=false;var pilot=p1.gameObject.AddComponent<RoadDriver>();pilot.Initialize(race,p1,true,1,1);pilot.Racer=race.Racers[0];
  yield return new WaitForSeconds(9);}
 IEnumerator Rivals094(string id){
  yield return DanCopy094();var e=CampaignData.Find(id);bool testing=!Campaign.Available(e);if(testing){flow.Save.Settings.unlockEverything=true;Campaign.Testing=true;}
  Campaign.Selected=id;typeof(RaceMenus).GetField("campaignEvent",Any).SetValue(Menus089,id);typeof(RaceMenus).GetField("page",Any).SetValue(Menus089,"campaign-event");Menus089.Show();yield return Settle091();
  var details=((UnityEngine.UI.Text)typeof(RaceMenus).GetField("details",Any).GetValue(Menus089)).text;var cast=CampaignData.CastFor(e.Rivals).Select(m=>CampaignData.RivalNames[m]).ToArray();
  Check(cast.All(details.Contains),$"{e.Name}'s page lists the rivals: {details.Split('\n').FirstOrDefault(l=>l.StartsWith("Rivals"))}{(testing?" (Testing on: event not yet open on this save)":"")}");yield return Late(()=>Shot4k("A-event-page-rivals"));
  yield return Choose091("start-event");
  float t0=Time.realtimeSinceStartup;while(flow.State!=RaceFlow.Stage.Countdown&&Time.realtimeSinceStartup-t0<60){yield return null;Bind();AudioListener.volume=0;}
  yield return new WaitForSeconds(.8f);var grid=GameObject.Find("Starting grid");var gridText=Text093("Grid");
  Check(grid&&grid.activeInHierarchy&&cast.All(n=>gridText!=null&&gridText.Contains(n)),$"countdown: the starting grid card: {gridText?.Replace("\n"," / ")}");yield return Late(()=>Shot4k("A-grid-card"));
  while(flow.State!=RaceFlow.Stage.Racing&&Time.realtimeSinceStartup-t0<80){yield return null;AudioListener.volume=0;}
  yield return new WaitForSeconds(.6f);Check(!(GameObject.Find("Starting grid")?.activeInHierarchy??false),"GO: the grid card is gone");
  var p1=race.vehicle;p1.GetComponent<VehicleInput>().enabled=false;var pilot=p1.gameObject.AddComponent<RoadDriver>();pilot.Initialize(race,p1,true,1,1);pilot.Racer=race.Racers[0];
  yield return new WaitForSeconds(14);var gaps=Text093("Gaps");
  Check(!string.IsNullOrEmpty(gaps)&&(gaps.Contains("▲")||gaps.Contains("▼"))&&gaps.Contains(" s"),$"racing: the ahead / behind line: {gaps?.Replace("\n"," / ")}");
  Check(NameTags.Shown>0,$"racing: {NameTags.Shown} name tags shown (Everyone)");yield return Late(()=>Shot4k("A-race-gaps-and-tags"));
  // to the finish (the race AI drives), then the results
  t0=Time.realtimeSinceStartup;while(flow.State!=RaceFlow.Stage.Results&&Time.realtimeSinceStartup-t0<420){yield return null;AudioListener.volume=0;if(WinnerShot.Active&&GameObject.Find("Race feedback")?.GetComponent<UnityEngine.UI.Text>().text.StartsWith("WINNER")==true&&!winnerSeen){winnerSeen=true;string w=Text093("Race feedback");Note("  winner shot caption: "+w?.Replace("\n"," "));yield return Late(()=>Shot4k("A-winner-shot"));}}
  yield return Settle091();var cells=CellsText094();
  Check(flow.State==RaceFlow.Stage.Results&&cast.All(cells.Contains)&&cells.Contains("Dan"),$"results: {cells}");yield return Late(()=>Shot4k("A-results-names"));
  Campaign.Testing=false;yield return Menu();}
 bool winnerSeen;
 IEnumerator SplitNames094(){
  yield return DanCopy094();Pads090();
  SplitScreen.P1Device=null;SplitScreen.P2Device=null;SplitScreen.P2Pick="";SplitScreen.P1Name="";
  Menus089.OpenSplitSetup();yield return Settle091();
  // player 2 joins with A on a second controller; player 1 (the pad that opened it) enters player 2's name
  yield return Press090(pad2,GamepadButton.South);yield return Settle091();
  Check(!SplitScreen.P2Ai&&SplitScreen.P2Device==pad2,$"player 2 joined with A on the second controller ({SplitScreen.DeviceName(SplitScreen.P2Device)})");
  Check(Button90("split-p1-name")&&Text(Button90("split-p1-name")).Contains("Dan"),$"player 1's name row: {Text(Button90("split-p1-name"))}");
  yield return Goto091("split-p2-name");yield return Press091(GamepadButton.A);Check(Page90=="keyboard","A on player 2's name: the keyboard");
  for(int i=0;i<12;i++)yield return Press091(GamepadButton.X);
  foreach(var c in "kyle")yield return Key094(c);
  for(int i=0;i<20&&Sel91Name!="save";i++)yield return Press091(Sel91Name.StartsWith("key-")?GamepadButton.DpadDown:GamepadButton.DpadRight);
  yield return Press091(GamepadButton.A);
  Check(SplitScreen.P2Name=="kyle"&&PlayerNames.Known.Contains("kyle"),$"player 2 is \"{SplitScreen.P2Name}\" (remembered: {string.Join(", ",PlayerNames.Known)})");yield return Late(()=>Shot4k("A-split-setup-names"));
  // a 1-lap race on Dan's Backyard Forward, 1 AI rival; both players driven by the race AI pilot
  SplitScreen.P1Vehicle="moto";SplitScreen.P2Vehicle="atv";SplitScreen.Course=6;SplitScreen.Laps=1;SplitScreen.Rivals=1;SplitScreen.RivalsRandom=false;SplitScreen.Traffic=false;SplitScreen.Time=TimeOfDay.Day;SplitScreen.Weather=Weather.Clear;
  int before=flow.Boards.Top(c=>c.StartsWith("backyard-forward"),true).Count;
  flow.StartSplit();yield return SplitRunning090();var split=SplitScreen.Race;
  var p1=race.vehicle;p1.GetComponent<VehicleInput>().enabled=false;var pilot1=p1.gameObject.AddComponent<RoadDriver>();pilot1.Initialize(race,p1,true,1,1);pilot1.Racer=race.Racers[0];
  var c2=split.P2Car;var pilot2=c2.gameObject.AddComponent<RoadDriver>();pilot2.Initialize(race,c2,true,1,1);pilot2.Racer=split.P2;
  NameTags.Trace=true;yield return new WaitForSeconds(10);Check(NameTags.Shown>0,$"split-screen: {NameTags.Shown} name tags (each half draws its own) {NameTags.LastTrace}");NameTags.Trace=false;yield return Late(()=>Shot4k("A-split-tags"));
  float t0=Time.realtimeSinceStartup;while(flow.State!=RaceFlow.Stage.Results&&Time.realtimeSinceStartup-t0<300){yield return null;AudioListener.volume=0;}
  yield return Settle091();
  var board=new RecordBoards(saveDir);var cat1=split?split.Category(1):"";
  var mine=board.Top(c=>c.StartsWith("backyard-forward"),true).Concat(board.Top(c=>c.StartsWith("backyard-forward"),false)).Where(x=>x.id.StartsWith("split-")).ToList();
  Check(mine.Any(x=>x.name=="kyle")&&mine.Any(x=>x.name=="Dan"),$"split-screen times on the Top 10: {string.Join("; ",mine.Select(x=>$"{x.name} {(x.race?"race":"lap")} {RaceHud.FormatTime(x.seconds)} {x.category}"))}");
  var details=((UnityEngine.UI.Text)typeof(RaceMenus).GetField("details",Any).GetValue(Menus089)).text;Note("  results: "+details.Replace("\n"," / ")+" || "+CellsText094());
  yield return Late(()=>Shot4k("A-split-results"));
  flow.QuitSplit(false);yield return new WaitForSecondsRealtime(1);
  // Records shows them under their names
  flow.OpenBoards();yield return Settle091();Note("  records after: "+CellsText094());}
 static string Text(UnityEngine.UI.Button b)=>b?b.GetComponentInChildren<UnityEngine.UI.Text>(true).text:"";
}
}
#endif
