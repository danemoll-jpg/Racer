#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Racer {
// 0.72 Part C targeted checks (muted). PROBE_CASES="case:args;..." per scene:
//  menu                     Race setup Time of Day / Weather and Free Roam Weather rows: controller (D-pad + A) and keyboard
//                           (arrows + Space) cycle them; defaults Day / Clear; values written to settings.json and read back
//                           by a fresh RacerSave (what a relaunch loads); garage stays Clear Day.
//  race:tod:weather:laps    A race with the normal AI field at those conditions; the player car is driven by the AI driver.
//                           Start, every checkpoint, finish, results, record saved; the look shown during the race.
//  cycle:fromHour:minutes   Free Roam from fromHour through `minutes` real minutes at 4x time scale: per-frame light change
//                           (no pops), preset names, lamps switching on, clock text in the HUD line.
public sealed class Report072CondChecks:MonoBehaviour {
 RaceDirector race;RaceFlow flow;readonly List<string> rows=new();string output;Gamepad pad;Keyboard keyboard;int fails;
 void Note(string s){rows.Add(s);File.WriteAllLines(output+"/results.txt",rows);Debug.Log("REPORT072 "+s);}
 void Check(bool ok,string s){if(!ok)fails++;Note((ok?"PASS ":"FAIL ")+s);}
 InputSettings.BackgroundBehavior oldBackground;InputSettings.EditorInputBehaviorInPlayMode oldEditorInput;
 void OnDestroy(){InputSystem.settings.backgroundBehavior=oldBackground;InputSystem.settings.editorInputBehaviorInPlayMode=oldEditorInput;}
 IEnumerator Start(){AudioListener.volume=0;var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;output=Environment.GetEnvironmentVariable("PROBE_OUT")+"/cond-"+scene;Directory.CreateDirectory(output);Application.runInBackground=true;
  race=FindAnyObjectByType<RaceDirector>();yield return null;yield return null;flow=race.Flow;
  var saveDir=Path.GetFullPath("Temp/Report072CondSave");if(Directory.Exists(saveDir))Directory.Delete(saveDir,true);flow.UseValidationSave(saveDir);
  var title=FindAnyObjectByType<StartupTitle>();if(title)Destroy(title.gameObject);typeof(StartupTitle).GetField("completed",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,true);flow.EnterMenuAfterTitle();
  oldBackground=InputSystem.settings.backgroundBehavior;oldEditorInput=InputSystem.settings.editorInputBehaviorInPlayMode;
  InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
  pad=InputSystem.AddDevice<Gamepad>();keyboard=InputSystem.AddDevice<Keyboard>();yield return Settle();
  foreach(var spec in (Environment.GetEnvironmentVariable("PROBE_CASES")??"").Split(';').Where(x=>x.Length>0)){var a=spec.Split(':');
   IEnumerator run=a[0] switch{"menu"=>Menu(),"race"=>Race((TimeOfDay)Enum.Parse(typeof(TimeOfDay),a[1]),(Weather)Enum.Parse(typeof(Weather),a[2]),int.Parse(a[3])),"cycle"=>Cycle(float.Parse(a[1]),float.Parse(a[2])),"weather"=>WeatherProbe(),"debug"=>DebugMeta(),_=>null};
   if(run!=null)yield return run;else Note("unknown case "+spec);}
  Note($"done, failures {fails}");File.WriteAllLines(output+"/done.txt",rows);UnityEditor.EditorApplication.Exit(0);}
 IEnumerator Settle(){yield return null;yield return null;float t0=Time.realtimeSinceStartup;while(MenuInput.Blocked&&Time.realtimeSinceStartup-t0<3)yield return null;yield return null;}
 IEnumerator Key1(Key k){yield return Settle();InputSystem.QueueStateEvent(keyboard,new KeyboardState(k));yield return null;yield return null;InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return Settle();}
 IEnumerator Pad1(GamepadButton b){yield return Settle();InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(b));yield return null;yield return null;InputSystem.QueueStateEvent(pad,new GamepadState());yield return Settle();}
 IEnumerable<UnityEngine.UI.Button> Buttons=>FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None);
 UnityEngine.UI.Button Button(string id)=>Buttons.FirstOrDefault(b=>b.name==id&&b.gameObject.activeInHierarchy);
 string Label(string id){var b=Button(id);return b?b.GetComponentInChildren<UnityEngine.UI.Text>(true).text:"(missing)";}
 string Focus=>EventSystem.current&&EventSystem.current.currentSelectedGameObject?EventSystem.current.currentSelectedGameObject.name:"none";
 IEnumerator Select(string id){yield return Settle();var b=Button(id);if(b)EventSystem.current.SetSelectedGameObject(b.gameObject);yield return null;}
 IEnumerator Nav(string id,bool usePad){// move focus with D-pad / arrow keys until it reaches id (at most 14 steps down)
  for(int i=0;i<14&&Focus!=id;i++){if(usePad)yield return Pad1(GamepadButton.DpadDown);else yield return Key1(Key.DownArrow);}}

 // ---------- menu ----------
 IEnumerator Menu(){
  if(flow.State!=RaceFlow.Stage.Ready){flow.Pause();flow.QuitRace();yield return Settle();}
  var s=flow.Save.Settings;Check(s.timeOfDay==0&&s.weather==0&&s.roamWeather==0,$"defaults: Time of Day {flow.TimeOfDayLabel}, Weather {flow.WeatherLabel}, Free Roam weather {flow.RoamWeatherLabel}");
  yield return Select("race");yield return Pad1(GamepadButton.South);
  Check(Label("timeofday")=="Time of Day: Day"&&Label("weather")=="Weather: Clear",$"Race setup rows: '{Label("timeofday")}', '{Label("weather")}' (between Traffic '{Label("traffic")}' and '{Label("playlists")}')");
  // controller: D-pad down from START RACE to Time of Day, A cycles Day -> Dusk -> Night
  yield return Select("start");yield return Nav("timeofday",true);Check(Focus=="timeofday","controller D-pad reaches Time of Day");
  yield return Pad1(GamepadButton.South);Check(Label("timeofday")=="Time of Day: Dusk"&&s.timeOfDay==1,$"controller A: {Label("timeofday")}");
  yield return Pad1(GamepadButton.South);Check(Label("timeofday")=="Time of Day: Night"&&s.timeOfDay==2,$"controller A again: {Label("timeofday")}");
  yield return Pad1(GamepadButton.DpadDown);Check(Focus=="weather","controller D-pad down reaches Weather");
  yield return Pad1(GamepadButton.South);Check(Label("weather")=="Weather: Rain"&&s.weather==1,$"controller A: {Label("weather")}");
  // keyboard: arrows + Space
  yield return Select("start");yield return Nav("weather",false);Check(Focus=="weather","keyboard arrows reach Weather");
  yield return Key1(Key.Space);Check(Label("weather")=="Weather: Snow"&&s.weather==2,$"keyboard Space: {Label("weather")}");
  yield return Key1(Key.UpArrow);yield return Key1(Key.Space);Check(Label("timeofday")=="Time of Day: Day"&&s.timeOfDay==0,$"keyboard Space on Time of Day wraps: {Label("timeofday")}");
  // mouse
  Button("timeofday").onClick.Invoke();yield return Settle();Check(Label("timeofday")=="Time of Day: Dusk",$"mouse click: {Label("timeofday")}");
  Check(WorldLook.Current&&WorldLook.Current.Preset.name=="Clear Day",$"menus stay Clear Day while options change (showing '{WorldLook.Current?.Preset.name}')");
  // Free Roam page
  yield return Pad1(GamepadButton.East);yield return Select("roam");yield return Pad1(GamepadButton.South);
  Check(Label("roam-weather")=="Weather: Clear",$"Free Roam page row: '{Label("roam-weather")}' after '{Label("explore")}'");
  yield return Select("explore");yield return Pad1(GamepadButton.DpadDown);Check(Focus=="roam-weather","controller reaches Free Roam Weather");
  yield return Pad1(GamepadButton.South);Check(Label("roam-weather")=="Weather: Rain"&&s.roamWeather==1,$"controller A: {Label("roam-weather")}");
  yield return Key1(Key.Space);Check(Label("roam-weather")=="Weather: Snow"&&s.roamWeather==2,$"keyboard Space: {Label("roam-weather")}");
  yield return Pad1(GamepadButton.East);
  // persistence: what a relaunch reads
  var fresh=new RacerSave(Path.GetFullPath("Temp/Report072CondSave"),"street-loop-gates-v1-laps3");
  Check(fresh.Settings.timeOfDay==1&&fresh.Settings.weather==2&&fresh.Settings.roamWeather==2,$"settings.json read by a fresh save (relaunch): timeOfDay {fresh.Settings.timeOfDay} (Dusk), weather {fresh.Settings.weather} (Snow), roamWeather {fresh.Settings.roamWeather} (Snow)");
  // garage stays Clear Day
  flow.OpenGarage();yield return Settle();yield return new WaitForSecondsRealtime(.5f);Check(WorldLook.Current.Mode=="Menu"&&WorldLook.Current.Preset.name=="Clear Day",$"garage shows Clear Day (mode {WorldLook.Current.Mode}, preset {WorldLook.Current.Preset.name})");
  flow.CloseGarage();yield return Settle();
  s.timeOfDay=0;s.weather=0;s.roamWeather=0;flow.Save.SaveSettings();}

 // ---------- race at given conditions ----------
 IEnumerator Race(TimeOfDay t,Weather w,int laps){
  if(flow.State!=RaceFlow.Stage.Ready){flow.Pause();flow.QuitRace();yield return Settle();}
  flow.Save.Settings.timeOfDay=(int)t;flow.Save.Settings.weather=(int)w;flow.Save.SaveSettings();
  flow.OpenGarage();flow.SelectVehicle("moto");flow.CloseGarage();race.opponents=true;race.laps=laps;flow.Save.Settings.estimateAiFinishes=true;
  string category=race.Category;var recordsBefore=File.Exists(Path.Combine(Path.GetFullPath("Temp/Report072CondSave"),"records.json"))?File.ReadAllText(Path.Combine(Path.GetFullPath("Temp/Report072CondSave"),"records.json")):"";
  flow.StartRace();float t0=Time.realtimeSinceStartup;while(flow.State!=RaceFlow.Stage.Countdown&&flow.State!=RaceFlow.Stage.Racing&&Time.realtimeSinceStartup-t0<30)yield return null;
  yield return null;yield return null;var look=WorldLook.Current;
  Check(look.Mode=="Race"&&look.RaceTime==t&&look.RaceWeather==w,$"{t}/{w}: race look '{look.Preset.name}' (mode {look.Mode}); lights {look.Preset.lights:F2} wet {look.Preset.wetness:F1} snow {look.Preset.snow:F1}");
  while(flow.State==RaceFlow.Stage.Countdown)yield return null;
  var car=race.vehicle;var pilot=car.gameObject.AddComponent<RoadDriver>();pilot.Initialize(race,car,true,1,1);pilot.Racer=race.Racers[0];car.GetComponent<VehicleInput>().enabled=false;
  Time.timeScale=3;int gates=0,last=race.Progress.NextGate;float start=Time.time;
  var lamps=FindObjectsByType<VehicleLights>(FindObjectsSortMode.None).Length;
  while(!race.ClassificationFinal&&Time.time-start<600){yield return null;AudioListener.volume=0;if(race.Progress.NextGate!=last){gates++;last=race.Progress.NextGate;}if(flow.State==RaceFlow.Stage.Results)break;}
  Time.timeScale=1;
  var p=race.Progress;Check(p.Finished&&p.CompletedLaps==laps,$"{t}/{w}: player (driven by the AI driver) finished {p.CompletedLaps}/{laps} laps, checkpoint changes {gates}, missed gates {p.MissedGates}, time {RaceHud.FormatTime(p.AdjustedTime(race.Clock))}, recoveries {pilot.RecoveryCount}");
  Check(look.Preset.lights>0||t==TimeOfDay.Day,$"{t}/{w}: vehicle lamps on during the race (level {VehicleLights.Level:F2}, vehicles with lamps {lamps})");
  float waitResults=Time.realtimeSinceStartup;while(flow.State!=RaceFlow.Stage.Results&&Time.realtimeSinceStartup-waitResults<60)yield return null;
  Check(flow.State==RaceFlow.Stage.Results,$"{t}/{w}: results screen shown (state {flow.State}); standings:\n{race.Standings()}");
  var recordsAfter=File.Exists(Path.Combine(Path.GetFullPath("Temp/Report072CondSave"),"records.json"))?File.ReadAllText(Path.Combine(Path.GetFullPath("Temp/Report072CondSave"),"records.json")):"";
  var boardsFile=Directory.GetFiles(Path.GetFullPath("Temp/Report072CondSave"),"*.json").Select(Path.GetFileName);
  Check(recordsAfter!=recordsBefore||Directory.GetFiles(Path.GetFullPath("Temp/Report072CondSave"),"*",SearchOption.AllDirectories).Any(f=>File.GetLastWriteTime(f)>DateTime.Now.AddMinutes(-2)&&f.Contains("record")),$"{t}/{w}: race record saved normally in the same category '{category}' (files: {string.Join(", ",boardsFile)})");
  if(pilot){pilot.enabled=false;Destroy(pilot);}car.GetComponent<VehicleInput>().enabled=true;
  flow.QuitRace();yield return Settle();
  Check(WorldLook.Current.Mode=="Menu"&&WorldLook.Current.Preset.name=="Clear Day",$"{t}/{w}: back in the menu the look is Clear Day again ('{WorldLook.Current.Preset.name}')");
  flow.Save.Settings.timeOfDay=0;flow.Save.Settings.weather=0;flow.Save.SaveSettings();}

 // ---------- Free Roam cycle ----------
 IEnumerator Cycle(float fromHour,float minutes){
  if(flow.State!=RaceFlow.Stage.Ready){flow.Pause();flow.QuitRace();yield return Settle();}
  flow.OpenGarage();flow.SelectVehicle("moto");flow.CloseGarage();flow.StartFreeRoam();float t0=Time.realtimeSinceStartup;while(flow.State!=RaceFlow.Stage.Racing&&Time.realtimeSinceStartup-t0<30)yield return null;
  yield return null;var look=WorldLook.Current;Check(look.Mode=="Free Roam"&&Mathf.Abs(look.Hour-WorldLook.FreeRoamStartHour)<.1f,$"Free Roam starts at {look.Clock} (mode {look.Mode}, preset {look.Preset.name})");
  var hud=flow.Activities?.Hud??"";Check(hud.Contains(look.Clock),$"clock in the Free Roam HUD line: '{hud.Split('\n')[0]}'");
  typeof(WorldLook).GetProperty("Hour").SetValue(look,fromHour);
  race.vehicle.Body.isKinematic=true;Time.timeScale=4;yield return null;yield return null;yield return null;// the look follows the new hour first
  var sun=RenderSettings.sun;float lastInt=sun.intensity;Quaternion lastRot=sun.transform.rotation;float maxStepInt=0,maxStepAngle=0,lastHour=look.Hour;string lastName="";var names=new List<string>();float lampOnHour=-1;int frames=0;
  using(var log=new StreamWriter(output+"/cycle.csv")){log.WriteLine("hour,preset,sunElevation,sunAzimuth,intensity,shadow,lights,fogEnd,exposure");
   float tEnd=Time.realtimeSinceStartup+minutes*60/4;
   while(Time.realtimeSinceStartup<tEnd){yield return null;AudioListener.volume=0;frames++;var p=look.Preset;
    // change per game minute (the batch editor renders few frames; a pop is a jump far above the steady rate)
    float gameMinutes=Mathf.Max(1/60f,Mathf.Repeat(look.Hour-lastHour,24)*60);
    maxStepInt=Mathf.Max(maxStepInt,Mathf.Abs(sun.intensity-lastInt)/gameMinutes);maxStepAngle=Mathf.Max(maxStepAngle,Quaternion.Angle(lastRot,sun.transform.rotation)/gameMinutes*(Mathf.Max(sun.intensity,lastInt)>.05f?1:0));lastInt=sun.intensity;lastRot=sun.transform.rotation;lastHour=look.Hour;
    if(p.name!=lastName){names.Add($"{look.Clock} {p.name}");lastName=p.name;}
    if(lampOnHour<0&&VehicleLights.Level>.3f)lampOnHour=look.Hour;
    if(frames%10==0)log.WriteLine($"{look.Hour:F3},{p.name},{p.sunElevation:F2},{p.sunAzimuth:F2},{sun.intensity:F3},{sun.shadowStrength:F3},{p.lights:F2},{p.fogEnd:F0},{p.postExposure:F2}");}}
  Time.timeScale=1;race.vehicle.Body.isKinematic=false;
  Check(names.Count>=2,$"cycle {fromHour:00.0}h -> {look.Clock}: presets passed {string.Join(" -> ",names)}");
  Check(maxStepInt<.06f&&maxStepAngle<.6f,$"no pops: largest change of the main light per game minute {maxStepInt:F4} intensity, {maxStepAngle:F3} degrees while lit (sun / moon move 0.25 / 0.25 degrees per game minute; {frames} frames)");
  Check(lampOnHour>0,$"lamps switch on automatically at {(lampOnHour>0?$"{Mathf.FloorToInt(lampOnHour):00}:{Mathf.FloorToInt(lampOnHour*60)%60:00}":"never")}");
  flow.Pause();flow.QuitRace();yield return Settle();}
 // ---------- weather state probe ----------
 IEnumerator WeatherProbe(){
  if(flow.State!=RaceFlow.Stage.Ready){flow.Pause();flow.QuitRace();yield return Settle();}
  race.opponents=true;flow.StartRace();yield return new WaitForSecondsRealtime(2);
  foreach(var (t,w) in new[]{(TimeOfDay.Day,Weather.Rain),(TimeOfDay.Night,Weather.Snow),(TimeOfDay.Night,Weather.Clear)}){
   WorldLook.Current.Pin(LookPresets.Compose(t,w));yield return new WaitForSecondsRealtime(3);
   var fx=WorldLook.Current.GetComponent<WeatherEffects>();var F=typeof(WeatherEffects).GetFields(BindingFlags.NonPublic|BindingFlags.Instance);string Get(string n)=>F.First(f=>f.Name==n).GetValue(fx)?.ToString();
   var ps=fx?fx.GetComponentsInChildren<ParticleSystem>(true):new ParticleSystem[0];
   Note($"{t}/{w}: effects {(fx?"present":"MISSING")} covered {Get("covered")} rain {Get("rainLevel")} snow {Get("snowLevel")} star {Get("starLevel")}; camera {Camera.main?.name} at {Camera.main?.transform.position}; systems: "+string.Join(", ",ps.Select(p=>$"{p.name} playing {p.isPlaying} count {p.particleCount} at {p.transform.position:F0} mat {p.GetComponent<ParticleSystemRenderer>().sharedMaterial?.name} shader {p.GetComponent<ParticleSystemRenderer>().sharedMaterial?.shader.name} renderer {p.GetComponent<ParticleSystemRenderer>().enabled}")));}
  WorldLook.Current.Pin(null);}
 // ---------- debug report metadata ----------
 IEnumerator DebugMeta(){
  if(flow.State!=RaceFlow.Stage.Ready){flow.Pause();flow.QuitRace();yield return Settle();}
  flow.Save.Settings.timeOfDay=2;flow.Save.Settings.weather=2;flow.Save.SaveSettings();race.opponents=false;flow.StartRace();yield return new WaitForSecondsRealtime(2);
  var root=Path.GetFullPath("Temp/Report072DebugReports");if(Directory.Exists(root))Directory.Delete(root,true);var session=new DebugReportSession(root);
  var bug=new DebugReportSession.Bug{id=session.NextId,timestamp=DateTimeOffset.Now.ToString("o"),course=race.courseName,mode="Race",scene="check",viewpoint="Vehicle",screenshot="Screenshots/BUG-001.png",comment="0.72 conditions check",conditions=WorldLook.Current.Conditions};
  File.WriteAllBytes(Path.Combine(session.DirectoryPath,bug.screenshot),new byte[]{0});session.Save(bug);
  var md=File.ReadAllText(Path.Combine(session.DirectoryPath,"BUG_REPORT.md"));var json=File.ReadAllText(Path.Combine(session.DirectoryPath,"bugs.json"));
  Check(md.Contains("Conditions: Night / Snow")&&json.Contains("Night / Snow"),$"bug report records the conditions: Markdown '{md.Split('\n').FirstOrDefault(l=>l.StartsWith("Conditions"))}', JSON {(json.Contains("Night / Snow")?"has":"lacks")} conditions");
  flow.Pause();flow.QuitRace();yield return Settle();flow.Save.Settings.timeOfDay=0;flow.Save.Settings.weather=0;flow.Save.SaveSettings();}
}
}
#endif
