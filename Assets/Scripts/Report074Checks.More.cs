#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Racer {
// 0.74 checks, continued:
//  storm:seconds:hour            Free Roam in Rain at that hour: every strike (time, gap, distance, flash peak, bolt), distant
//                                rumbles, thunder; then "Lightning flashes: Off" for 60 s (no screen flash; bolts and thunder stay).
//  clock                         the Free Roam clock: saved and resumed through each exit (race start, return to menu, quit),
//                                midnight advances the day and the moon phase, day 30 wraps to day 1, a corrupt value falls back.
//  waypoint                      map waypoint: placed with the cursor (controller path) and with a mouse click (also in an
//                                unexplored area), replaced, cleared; the beam and HUD line show; reaching it clears it.
//  shots:label|ex,ey,ez|tx,ty,tz|hour|weather|...   Free Roam screenshots at those views, hour and weather.
public sealed partial class Report074Checks {
 partial void More(string[] a,string spec,ref IEnumerator run){
  switch(a[0]){
   case "storm":run=Storm(float.Parse(a[1]),float.Parse(a[2]));break;
   case "clock":run=Clock();break;
   case "waypoint":run=WaypointCase();break;
   case "shots":run=Shots(spec.Substring(6));break;
   case "bolt":run=Bolt(float.Parse(a[1]));break;
  }}
 WorldLook Look=>WorldLook.Current;
 IEnumerator Storm(float seconds,float hour){
  flow.Save.Settings.lightningFlashes=true;yield return Roam("moto",Weather.Rain);Look.SetClock(10,hour);var fx=Look.GetComponent<WeatherEffects>();
  var car=Car;car.Body.isKinematic=true;// parked: the storm only
  var strikes=new List<(float t,float d,float peak,int bolts)>();int last=fx.Strikes,rumbles0=fx.Rumbles,thunder0=fx.ThunderPlayed;float t0=Time.time;float peakStart=-1;float peak=0;
  while(Time.time-t0<seconds){yield return null;Look.SetClock(10,hour);
   if(fx.Strikes!=last){last=fx.Strikes;strikes.Add((Time.time-t0,fx.LastStrikeDistance,0,fx.BoltsShown));peakStart=Time.time;peak=0;}
   if(peakStart>0&&Time.time-peakStart<.5f){peak=Mathf.Max(peak,fx.FlashLevel);var s=strikes[^1];strikes[^1]=(s.t,s.d,peak,s.bolts);}}
  var gaps=strikes.Zip(strikes.Skip(1),(x,y)=>y.t-x.t).ToList();
  Note($"STORM hour {hour:00} ({Look.Preset.name}) {seconds:F0} s: strikes {strikes.Count}, bolts shown {fx.BoltsShown}, thunder {fx.ThunderPlayed-thunder0}, distant rumbles {fx.Rumbles-rumbles0}");
  Note("  strikes (t s, distance m, flash peak): "+string.Join("  ",strikes.Select(s=>$"{s.t:F1}/{s.d:F0}/{s.peak:F2}")));
  Note($"  gaps s: {string.Join(" ",gaps.Select(g=>g.ToString("F1")))}; near (<600 m) {strikes.Count(s=>s.d<600)}, far {strikes.Count(s=>s.d>=600)}; close pairs (<5 s) {gaps.Count(g=>g<5)}");
  Check(strikes.Count>=Mathf.FloorToInt(seconds/25f),$"storm: at least one strike per 25 s on average ({strikes.Count} in {seconds:F0} s)");
  Check(gaps.Count==0||gaps.Max()<=26,$"storm: no gap longer than ~25 s (longest {(gaps.Count>0?gaps.Max():0):F1} s)");
  Check(strikes.All(s=>s.peak>.3f),$"storm: every strike flashes clearly (lowest peak {(strikes.Count>0?strikes.Min(s=>s.peak):0):F2})");
  // the Off setting
  flow.Save.Settings.lightningFlashes=false;int b0=fx.BoltsShown,s0=fx.Strikes,th0=fx.ThunderPlayed;float maxFlash=0;t0=Time.time;
  while(Time.time-t0<60){yield return null;maxFlash=Mathf.Max(maxFlash,fx.FlashLevel);}
  Check(fx.Strikes>s0&&maxFlash==0&&fx.BoltsShown>b0&&fx.ThunderPlayed>th0,$"storm Off: {fx.Strikes-s0} strikes in 60 s, screen flash peak {maxFlash:F2}, bolts {fx.BoltsShown-b0}, thunder {fx.ThunderPlayed-th0}");
  flow.Save.Settings.lightningFlashes=true;car.Body.isKinematic=false;}

 IEnumerator Clock(){
  var s=flow.Save.Settings;
  // first ever start: no value saved (leave Free Roam first, so the session's own clock is not written over the reset)
  yield return ToMenu();yield return null;s.roamHour=8;s.roamDay=1;flow.Save.SaveSettings();
  yield return Roam("moto",Weather.Clear);Check(Look.Day==1&&Mathf.Abs(Look.Hour-8)<.1f,$"clock: first start Day {Look.Day} {Look.Clock}");
  // 1. start a race
  Look.SetClock(3,21.5f);yield return new WaitForSecondsRealtime(.5f);float h=Look.Hour;flow.Pause();flow.StartRace();yield return new WaitForSecondsRealtime(2);
  var disk=ReadDisk();Check(disk.roamDay==3&&Mathf.Abs(disk.roamHour-h)<.05f,$"clock: starting a race saved Day {disk.roamDay} {disk.roamHour:F2} (was Day 3 {h:F2})");
  float raceHour=Look.Hour;yield return new WaitForSecondsRealtime(3);
  yield return Roam("moto",Weather.Clear);Check(Look.Day==3&&Mathf.Abs(Look.Hour-h)<.05f,$"clock: resumed after the race at Day {Look.Day} {Look.Clock} (time did not advance during the race)");
  // 2. return to the menu
  Look.SetClock(9,13.25f);yield return new WaitForSecondsRealtime(.5f);h=Look.Hour;flow.Pause();yield return new WaitForSecondsRealtime(1);
  disk=ReadDisk();Check(disk.roamDay==9&&Mathf.Abs(disk.roamHour-h)<.05f,$"clock: opening the menu (Free Roam pause = main menu) saved Day {disk.roamDay} {disk.roamHour:F2}");
  yield return new WaitForSecondsRealtime(3);yield return Roam("moto",Weather.Clear);Check(Look.Day==9&&Mathf.Abs(Look.Hour-h)<.05f,$"clock: resumed after the menu at Day {Look.Day} {Look.Clock}");
  // 3. quit the game (the quit path: settings saved by RaceFlow.Quit and WorldLook's application-quit save)
  Look.SetClock(17,4.75f);yield return new WaitForSecondsRealtime(.5f);h=Look.Hour;
  typeof(WorldLook).GetMethod("OnApplicationQuit",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(Look,null);
  disk=ReadDisk();Check(disk.roamDay==17&&Mathf.Abs(disk.roamHour-h)<.05f,$"clock: quitting saved Day {disk.roamDay} {disk.roamHour:F2} (read back from settings.json)");
  // 4. midnight: the day and the moon advance
  Look.SetClock(14,23.98f);float ph0=Look.MoonPhase;float w0=Time.time;while(Look.Day==14&&Time.time-w0<10)yield return null;
  Check(Look.Day==15&&Look.Hour<1,$"clock: midnight Day 14 -> Day {Look.Day} {Look.Clock}; moon phase {ph0:F3} -> {Look.MoonPhase:F3} (lit {WorldLook.Illumination(Look.MoonPhase):F2})");
  Look.SetClock(30,23.98f);w0=Time.time;while(Look.Day==30&&Time.time-w0<10)yield return null;
  Check(Look.Day==1,$"clock: Day 30 wraps to Day {Look.Day} {Look.Clock}; moon phase {Look.MoonPhase:F3} (new)");
  Check(Look.Conditions.Contains("Day 1 ")&&flow.Activities.Hud.Contains("Day 1 "),$"clock: HUD \"{flow.Activities.Hud.Split('\n')[0]}\"; conditions \"{Look.Conditions}\"");
  // 5. the night light by phase (moon up at midnight near full; dim sky glow at new moon)
  foreach(int day in new[]{1,8,15,22}){var p=WorldLook.Cycle(0.5f,Weather.Clear,WorldLook.PhaseOf(day,.5f));Note($"  night 00:30 day {day}: light {p.sunIntensity:F2} at elevation {p.sunElevation:F0} az {p.sunAzimuth:F0}, ambient x{p.ambientIntensity:F2}");}
  // 6. a corrupt value falls back to day 1, 08:00
  flow.Pause();flow.StartRace();yield return new WaitForSecondsRealtime(2);yield return ToMenu();s.roamHour=float.NaN;s.roamDay=77;yield return Roam("moto",Weather.Clear);Check(Look.Day==1&&Mathf.Abs(Look.Hour-8)<.1f,$"clock: unreadable value -> Day {Look.Day} {Look.Clock}");
  yield return ToMenu();}
 RacerSave.Options ReadDisk(){var p=Path.Combine(flow.Save.DirectoryPath,"settings.json");var o=new RacerSave.Options();if(File.Exists(p))JsonUtility.FromJsonOverwrite(File.ReadAllText(p),o);return o;}

 IEnumerator WaypointCase(){
  yield return Roam("moto",Weather.Clear);var map=race.GetComponent<ExplorationMap>();if(!map)map=FindAnyObjectByType<ExplorationMap>();var car=Car;
  map.Open();yield return null;
  var center=typeof(ExplorationMap).GetField("center",BindingFlags.NonPublic|BindingFlags.Instance);
  var toggle=typeof(ExplorationMap).GetMethod("ToggleWaypoint",BindingFlags.NonPublic|BindingFlags.Instance);
  var clear=typeof(ExplorationMap).GetMethod("ClearWaypoint",BindingFlags.NonPublic|BindingFlags.Instance);
  var mapUV=typeof(ExplorationMap).GetMethod("MapNormalized",BindingFlags.NonPublic|BindingFlags.Instance);
  // controller: move the cursor 220 m east of the player, place
  var target=car.Body.position+new Vector3(220,0,0);center.SetValue(map,(Vector2)mapUV.Invoke(map,new object[]{target}));toggle.Invoke(map,null);
  Check(map.Waypoint.HasValue&&new Vector2(map.Waypoint.Value.x-target.x,map.Waypoint.Value.z-target.z).magnitude<3,$"waypoint: placed with the cursor at {map.Waypoint:F0}");
  // controller: replace (another spot) and clear
  var t2=car.Body.position+new Vector3(-150,0,90);center.SetValue(map,(Vector2)mapUV.Invoke(map,new object[]{t2}));toggle.Invoke(map,null);
  Check(map.Waypoint.HasValue&&new Vector2(map.Waypoint.Value.x-t2.x,map.Waypoint.Value.z-t2.z).magnitude<3,"waypoint: replaced by a new one");
  clear.Invoke(map,null);Check(!map.Waypoint.HasValue,"waypoint: cleared with the clear button");
  // mouse: a click in an unexplored area (the far corner of the world) puts the waypoint there
  var pic=(UnityEngine.UI.RawImage)typeof(ExplorationMap).GetField("picture",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(map);
  center.SetValue(map,new Vector2(.08f,.1f));typeof(ExplorationMap).GetMethod("Draw",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(map,null);yield return null;
  var rt=pic.rectTransform;var screen=RectTransformUtility.WorldToScreenPoint(null,rt.TransformPoint(new Vector3(40,-30,0)));
  var e=new PointerEventData(EventSystem.current){position=screen,button=PointerEventData.InputButton.Left};map.OnPointerClick(e);
  bool fog=map.Waypoint.HasValue&&!map.Visited(map.Waypoint.Value);
  Check(map.Waypoint.HasValue,$"waypoint: placed with a mouse click at {map.Waypoint:F0} (unexplored: {fog})");
  var e2=new PointerEventData(EventSystem.current){position=screen,button=PointerEventData.InputButton.Right};map.OnPointerClick(e2);Check(!map.Waypoint.HasValue,"waypoint: cleared with a right-click");
  // drive to one: placed 60 m ahead; the beam and HUD show; reaching it clears it with a note
  var ahead=car.Body.position+Vector3.ProjectOnPlane(car.transform.forward,Vector3.up).normalized*60;map.SetWaypoint(ahead);map.Close();yield return new WaitForSecondsRealtime(.5f);
  var guide=race.GetComponent<WaypointGuide>();if(!guide)guide=FindAnyObjectByType<WaypointGuide>();
  var beam=guide.GetComponentsInChildren<LineRenderer>().FirstOrDefault(l=>l.name=="Waypoint beam");
  Check(guide.Active&&beam&&beam.enabled&&guide.Hud.StartsWith("WAYPOINT"),$"waypoint: beam shown, HUD \"{guide.Hud}\" bearing {guide.Bearing:F0} deg");
  yield return Shot2("waypoint-beam-day");
  Look.SetClock(10,23);yield return new WaitForSecondsRealtime(1);yield return Shot2("waypoint-beam-night");Look.SetClock(10,12);
  Put(map.Waypoint.Value+Vector3.up*1.5f+Vector3.ProjectOnPlane(car.transform.forward,Vector3.up).normalized*-12,car.transform.rotation,Vector3.zero);
  float w0=Time.time;while(map.Waypoint.HasValue&&Time.time-w0<5)yield return null;
  Check(!map.Waypoint.HasValue&&guide.Arrivals==1,$"waypoint: reached (within {WaypointGuide.ArriveDistance} m) -> cleared, \"Destination reached\" shown ({guide.Arrivals})");
  // races: nothing shown
  map.SetWaypoint(car.Body.position+new Vector3(100,0,0));yield return ToMenu();flow.StartRace();yield return new WaitForSecondsRealtime(3);
  Check(!guide||!guide.Active,"waypoint: not shown in a race");yield return ToMenu();}
 IEnumerator Shot2(string name){yield return null;yield return null;// (no WaitForEndOfFrame: it never fires in batch mode)
 var cam=Camera.main;var rt=new RenderTexture(1600,900,24);cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;var tex=new Texture2D(1600,900,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1600,900),0,0);tex.Apply();File.WriteAllBytes($"{output}/{name}.png",tex.EncodeToPNG());cam.targetTexture=null;RenderTexture.active=null;rt.Release();Destroy(tex);}

 // a forced strike ahead of the camera, held lit, screenshot (evidence that the bolt itself is drawn)
 IEnumerator Bolt(float hour){flow.Save.Settings.lightningFlashes=true;yield return Roam("moto",Weather.Rain);Look.SetClock(10,hour);var fx=Look.GetComponent<WeatherEffects>();Car.Body.isKinematic=true;
  var cam=Camera.main;foreach(var c in FindObjectsByType<ChaseCamera>(FindObjectsSortMode.None))c.enabled=false;var eye=Car.Body.position+Vector3.up*3;var fwd=Vector3.ProjectOnPlane(Car.transform.forward,Vector3.up).normalized;
  cam.transform.SetPositionAndRotation(eye,Quaternion.LookRotation(fwd+Vector3.up*.25f));yield return new WaitForSeconds(1);
  WeatherEffects.TestBoltAzimuth=cam.transform.eulerAngles.y;int before=fx.Strikes;typeof(WeatherEffects).GetField("nextStrike",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(fx,Time.time);
  float w0=Time.realtimeSinceStartup;while(fx.Strikes==before&&Time.realtimeSinceStartup-w0<5){Look.SetClock(10,hour);yield return null;}
  LightningBolts.TestHold=true;yield return null;var bolts=fx.GetComponentInChildren<LightningBolts>();
  Note($"BOLT hour {hour}: strike distance {fx.LastStrikeDistance:F0} m, camera {cam.name} mask {cam.cullingMask}, {bolts?.Describe()}");
  yield return Shot2($"bolt-{hour:00}");Check(bolts&&bolts.Describe().Contains("on=True"),$"bolt drawn at hour {hour}");
  LightningBolts.TestHold=false;WeatherEffects.TestBoltAzimuth=float.NaN;foreach(var c in FindObjectsByType<ChaseCamera>(FindObjectsSortMode.None))c.enabled=true;Car.Body.isKinematic=false;}
 IEnumerator Shots(string spec){
  foreach(var v in spec.Split('|').Select((x,i)=>(x,i)).GroupBy(p=>p.i/5).Select(g=>g.Select(p=>p.x).ToArray()).Where(g=>g.Length==5)){
   Vector3 V(string s){var q=s.Split(',').Select(float.Parse).ToArray();return new Vector3(q[0],q[1],q[2]);}
   var w=(Weather)Enum.Parse(typeof(Weather),v[4]);if(Look==null||Look.Mode!="Free Roam"||Look.RoamWeather!=w)yield return Roam("moto",w);
   Look.SetClock(10,float.Parse(v[3]));Car.Body.isKinematic=true;var cam=Camera.main;foreach(var c in FindObjectsByType<ChaseCamera>(FindObjectsSortMode.None))c.enabled=false;
   var eye=V(v[1]);cam.transform.SetPositionAndRotation(eye,Quaternion.LookRotation(V(v[2])-eye));yield return new WaitForSeconds(1.5f);Look.SetClock(10,float.Parse(v[3]));
   yield return Shot2(v[0]);Note($"SHOT {v[0]} hour {v[3]} {w}");foreach(var c in FindObjectsByType<ChaseCamera>(FindObjectsSortMode.None))c.enabled=true;Car.Body.isKinematic=false;}}
}
}
#endif
