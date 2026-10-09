#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Racer {
// 0.97 Part C: the trailer capture mode (editor only; PROBE_CASES "cap97:..." through Tools/Report097/Run-Trailer.ps1). The game plays each
// shot of Docs/Trailer/shot-list.md by itself: it sets the course or Free Roam position, time of day, weather, moon, vehicle, colour and rider
// look, drives the vehicle (the game's own AI on the road, or the racing AI for the jumps), places the Trailer Mode camera and records
// offline at a fixed 60 frames a second (TrailerRecorder). Clips go to PROBE_TRAILER (the project's Trailer/ folder), one per shot.
//   cap97:test            a 3 s clip at 1280x720: the recorder, the sound and ffmpeg
public sealed partial class Report080Checks {
 string capOut => Environment.GetEnvironmentVariable("PROBE_TRAILER") ?? Path.Combine(output, "Trailer");
 string capFfmpeg => Environment.GetEnvironmentVariable("PROBE_FFMPEG");
 int capW => int.TryParse(Environment.GetEnvironmentVariable("PROBE_CAPW"), out var w) ? w : 3840;
 int capH => int.TryParse(Environment.GetEnvironmentVariable("PROBE_CAPH"), out var h) ? h : 2160;
 TrailerRecorder rec;
 readonly List<string> capLog = new();
 void CapNote(string s) { capLog.Add(s); Note(s); try { File.WriteAllLines(Path.Combine(capOut, "capture-log.txt"), capLog); } catch { } }

 // the world for a Free Roam shot: vehicle, colour, rider look, time of day, weather; returns once driving
 IEnumerator CapRoam(string vehicle, int color, RiderLook rider, float hour, Weather weather, float moon = -1, bool pause = true) {
  yield return Load("StreetLoopGreybox"); yield return Menu();
  var st = flow.Save.Settings; st.master = 1; st.ambience = 1; st.radioOn = false; st.roamWeather = (int)weather; st.hints = false; st.weatherGrip = true; st.unlockEverything = true; Campaign.Testing = true; VehicleUnlocks.Set(24, true); flow.Save.SaveSettings();
  if (rider != null) { flow.Save.Settings.rider = rider; RiderLook.Player = rider; }
  FixColors97();flow.OpenGarage(); flow.SelectVehicle(vehicle); if (color >= 0) flow.SetColor(color); flow.CloseGarage();
  flow.StartFreeRoam(); float t0 = Time.realtimeSinceStartup;
  while (Time.realtimeSinceStartup - t0 < 120) { yield return null; Bind(); if (flow.State == RaceFlow.Stage.Racing && race.gameObject.scene.name == RaceFlow.RoamScene) break; }
  yield return new WaitForSecondsRealtime(3); AudioListener.volume = 1; Bind();
  { var cfg = race.vehicle.GetComponent<VehicleConfiguration>(); if (cfg != null && cfg.profileId != vehicle) cfg.Apply(vehicle); }
  TrailerMode.Instance.Begin(); yield return null; yield return null;
  var c = WorldLook.Current.Trailer; if (c != null) { c.hour = hour; c.weather = weather; if (moon >= 0) c.phase = moon; c.paused = pause; }
  yield return new WaitForSecondsRealtime(1.5f);
 }
 // record `seconds` of game time (the game steps 1/60 s a frame); hook(t) runs every frame before it renders
 IEnumerator CapRecord(string name, float seconds, Action<float> hook = null, Func<bool> stop = null, RenderTexture source = null, bool screen = false, bool noCamera = false) {
  rec ??= new TrailerRecorder { Ffmpeg = capFfmpeg }; string file = Path.Combine(capOut, "Clips", name + ".mp4"); int w = capW, h = capH;
  if (source != null) { w = source.width; h = source.height; } if (screen) { w = Screen.width; h = Screen.height; }
  var cam0 = source != null || screen || noCamera ? null : Camera.main;
  if (!rec.Begin(cam0, file, w, h, source, screen)) { if (source != null || screen) { CapNote(name + ": no recording"); yield break; } w = 2560; h = 1440; CapNote($"{name}: {capW}x{capH} would not render, falling back to {w}x{h}"); if (!rec.Begin(cam0, file, w, h)) { CapNote(name + ": no recording"); yield break; } }
  AudioListener.volume = 1; UnityEditor.EditorUtility.audioMasterMute = false; float elapsed = 0; long frames = (long)(seconds * rec.Fps); var realStart = Time.realtimeSinceStartup;
  for (long i = 0; i < frames; i++) { if (i == 30) CapNote($"audio diag: listener volume {AudioListener.volume} pause {AudioListener.pause} listeners {FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Length} sources playing {FindObjectsByType<AudioSource>(FindObjectsSortMode.None).Count(a => a.isPlaying)} of {FindObjectsByType<AudioSource>(FindObjectsSortMode.None).Length}, rendered samples/frame {AudioRenderer.GetSampleCountForCaptureFrame()}, timeScale {Time.timeScale}, master {flow.Save.Settings.master}"); hook?.Invoke(elapsed); yield return null; if (screen) yield return new WaitForEndOfFrame(); rec.Grab(); elapsed += Time.deltaTime; if (stop != null && stop()) break; }
  rec.End(); UnityEditor.EditorUtility.audioMasterMute = true; var (peak, rms) = TrailerRecorder.PcmLevel(Path.ChangeExtension(file, ".pcm"));
  CapNote($"{name}: {rec.Frames} frames {w}x{h} @ {rec.Fps} fps = {rec.Frames / (float)rec.Fps:F1} s of clip, {Time.realtimeSinceStartup - realStart:F0} s to render; sound peak {peak:F2} rms {rms:F3}; {(File.Exists(file) ? new FileInfo(file).Length / 1048576f : 0):F1} MB");
  try { File.Delete(Path.ChangeExtension(file, ".pcm")); } catch { }
 }
 IEnumerator CapTest() {
  rec = new TrailerRecorder { Ffmpeg = capFfmpeg };
  yield return CapRoam("moto", -1, null, 12, Weather.Clear);
  var car = race.vehicle; var views = CameraViews.Current; views.SelectTrailerCamera(CameraViews.Shot.Chase);
  CapNote($"cameras: {Camera.allCamerasCount} ({string.Join(", ", Camera.allCameras.Select(c => c.name + (c.targetTexture ? "/RT" : "")))}); audio {AudioSettings.outputSampleRate} Hz {AudioSettings.speakerMode}; listener volume {AudioListener.volume}");
  var pilot = car.gameObject.AddComponent<RoadDriver>(); pilot.Initialize(race, car, false, 1, 1.1f); car.GetComponent<VehicleInput>().enabled = false;
  yield return new WaitForSecondsRealtime(4);
  Environment.SetEnvironmentVariable("PROBE_CAPW", "1280"); Environment.SetEnvironmentVariable("PROBE_CAPH", "720");
  yield return CapRecord("00-test", 3);
  Check(File.Exists(Path.Combine(capOut, "Clips", "00-test.mp4")), "test clip written");
  Destroy(pilot); flow.QuitRace(); yield return null;
 }
 IEnumerator CapSurvey() {
  rec = new TrailerRecorder { Ffmpeg = capFfmpeg };
  yield return CapRoam("moto", -1, null, 12, Weather.Snow);
  var map = flow.GetComponent<ExplorationMap>();
  if (map != null) foreach (var d in map.destinations) CapNote($"destination {d.id} '{d.title}' at {V(d.position)} yaw {d.yaw:F0}");
  foreach (var s in FindObjectsByType<ActivitySite>(FindObjectsSortMode.None)) CapNote($"site {s.id} '{s.title}' {s.kind} at {V(s.transform.position)} forward {V(s.forward)} radius {s.radius}");
  foreach (var w in FindObjectsByType<ShallowWater>(FindObjectsSortMode.None)) CapNote($"water {w.name} surface {w.Surface:F1} ice {(w.Ice ? V(w.Ice.bounds.center) + " size " + V(w.Ice.bounds.size) : "none")}");
  foreach (var r in FindObjectsByType<RaceRoad>(FindObjectsSortMode.None)) { r.Initialize(); CapNote($"road '{r.name}' length {r.Length:F0} trail {r.forestTrail} open {r.openHighway} start {V(r.At(0, out _))}"); }
  foreach (var t in FindObjectsByType<Transform>(FindObjectsSortMode.None)) { string n = t.name.ToLowerInvariant(); if (n.Contains("sled") || n.Contains("hockey") || n.Contains("broom") || n.Contains("mailbox") || n.Contains("storm") || n.Contains("drain") || n.Contains("dump")) CapNote($"object '{t.name}' at {V(t.position)}"); }
  var car = race.vehicle; CapNote($"player vehicle at {V(car.transform.position)} heading {car.transform.eulerAngles.y:F0}; ambient road '{(race.ambientRoad ? race.ambientRoad.name : "none")}'; road '{(race.road ? race.road.name : "none")}'");
  flow.QuitRace(); yield return null;
 }
 IEnumerator CapSurvey2() {
  yield return CapRoam("moto", -1, null, 12, Weather.Clear);
  var site = new Vector3(481.3f, 88.1f, -89.7f);
  foreach (var b in FindObjectsByType<BreakableProp>(FindObjectsSortMode.None)) if (Vector3.Distance(b.transform.position, site) < 110) CapNote($"breakable '{b.name}' at {V(b.transform.position)} parent '{(b.transform.parent ? b.transform.parent.name : "")}'");
  foreach (var t in FindObjectsByType<Transform>(FindObjectsSortMode.None)) { var n = t.name; if (n.IndexOf("kyle", StringComparison.OrdinalIgnoreCase) >= 0 || n.IndexOf("lawn", StringComparison.OrdinalIgnoreCase) >= 0) CapNote($"kyle/lawn '{n}' at {V(t.position)}"); }
  var kd = CapNamedRoad("Kyle descending"); if (kd) for (float s = 0; s <= kd.Length; s += 10) CapNote($"kyle road s {s:F0}: {V(kd.At(s, out _))}");
  var amb = race.ambientRoad; CapNote($"ambient road '{amb.name}' length {amb.Length:F0}");
  foreach (var (nm, pt) in new[] { ("home", new Vector3(433.8f, 83.7f, -13.5f)), ("roger", new Vector3(458, 88, -82.5f)), ("anderson", new Vector3(510.5f, 78.7f, -22.9f)), ("mcfadden", new Vector3(434.6f, 32.75f, -153.1f)), ("kyle mailbox", new Vector3(477.9f, 84.5f, -17.4f)), ("market", new Vector3(25, 8, 530)), ("storm drain", new Vector3(182.5f, 67.2f, 83.5f)) }) { float s = amb.Project(pt, out float lat); CapNote($"ambient road nearest to {nm}: s {s:F0} lateral {lat:F1} m at {V(amb.At(s, out _))}"); }
  foreach (var t in FindObjectsByType<Transform>(FindObjectsSortMode.None)) { var n = t.name; if (n.StartsWith("Long storm culvert") || n.StartsWith("Storm Drain") || n.IndexOf("entrance", StringComparison.OrdinalIgnoreCase) >= 0 && n.IndexOf("drain", StringComparison.OrdinalIgnoreCase) >= 0) { var rs = t.GetComponentsInChildren<Renderer>(); if (rs.Length == 0) { CapNote($"drain '{n}' no renderers"); continue; } var bb = rs[0].bounds; foreach (var r in rs) bb.Encapsulate(r.bounds); CapNote($"drain '{n}' children {rs.Length} bounds center {V(bb.center)} size {V(bb.size)}"); } }
  foreach (var r in FindObjectsByType<RaceRoad>(FindObjectsSortMode.None)) if (r.name.IndexOf("drain", StringComparison.OrdinalIgnoreCase) >= 0 || r.name.IndexOf("storm", StringComparison.OrdinalIgnoreCase) >= 0) { r.Initialize(); CapNote($"drain road '{r.name}' length {r.Length:F0} from {V(r.At(0, out _))} to {V(r.At(r.Length, out _))}"); }
  flow.QuitRace(); yield return null;
 }
 IEnumerator Run097c(string[] a) {
  if (!a[0].StartsWith("cap97")) return null;
  Directory.CreateDirectory(Path.Combine(capOut, "Clips"));
  return a[0] switch { "cap97" => a[1] switch { "test" => CapTest(), "survey" => CapSurvey(), "survey2" => CapSurvey2(), "shot" => CapShot(int.Parse(a[2])), _ => null }, _ => null };
 }
}
}
#endif
