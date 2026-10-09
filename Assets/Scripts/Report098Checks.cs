#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Racer {
// 0.98 targeted checks (muted, isolated save), added in front of the 0.97 chain:
//   setup98                       controller-only walk of the police setups: no Start-at row, no solo Runner role, the Getaway description
//   tunnel98:vehicle:dir:day|night  the Free Roam storm drain driven end to end by a waypoint driver along the Storm Drain trail (dir 1 = down the drain, -1 = back up)
//   chase98:kind:vehicle:seed[:minutes]   Getaway on Normal: kind roadhalf (a human on the main road at half throttle) or roadfast (flat out); a per-second log, shots
//   radio98                       the longest police radio line at 3840 x 2160, then in a split-screen half
public sealed partial class Report080Checks {
 IEnumerator Run098(string[] a) => a[0] switch {
  "setup98" => Setup098(), "tunnel98" => Tunnel098(a[1], int.Parse(a[2]), a.Length > 3 ? a[3] : "day"), "chase98" => Chase98(a[1], a[2], int.Parse(a[3]), a.Length > 4 ? int.Parse(a[4]) : 5), "radio98" => Radio098(), _ => Run097(a) };

 IEnumerator PressUntil098(Func<bool> done, GamepadButton b, int max = 6) { for (int i = 0; i < max && !done(); i++) yield return Press091(b); }
 IEnumerator Setup098() {
  yield return DanCopy095(); Pads090(); SplitScreen.Solo = true; SplitScreen.P2Device = null; SplitScreen.P2AiRunner = false; SplitScreen.PoliceGame = SplitScreen.Game.CopRunner; SplitScreen.SoloRole = 2; // an old session left on the Runner role
  yield return Choose091("police");
  Check(Page90 == "police" && SplitScreen.SoloRole == 1 && !Button90("police-role") && !Button90("split-course"), $"Cop vs Runner, 1 player: no role row, no 'Start at' row, role forced to cop ({SplitScreen.SoloRole}); rows [{string.Join(", ", Rows091().Select(r => r.name))}]; {Details095}");
  yield return Late(() => Shot4k("98-setup-copvsrunner")); yield return Walk091("Cop vs Runner setup (1 player)", true, () => Page90 == "");
  yield return Choose091("police"); yield return Goto091("police-game"); yield return PressUntil098(() => SplitScreen.PoliceGame == SplitScreen.Game.Getaway, GamepadButton.DpadRight);
  var rows = string.Join(", ", Rows091().Select(r => r.name)); string det = Details095;
  Check(SplitScreen.PoliceGame == SplitScreen.Game.Getaway && !Button90("split-course") && det.Contains("Escape the police. More cops join the longer you stay free."), $"Getaway setup: no 'Start at' row; description starts with the required line; rows [{rows}]; {det}");
  yield return Late(() => Shot4k("98-setup-getaway")); yield return Walk091("Getaway setup (1 player)", true, () => Page90 == "");
  yield return Choose091("police"); yield return Goto091("police-game"); yield return PressUntil098(() => SplitScreen.PoliceGame == SplitScreen.Game.SpeedPatrol, GamepadButton.DpadRight, 8);
  Check(SplitScreen.PoliceGame == SplitScreen.Game.SpeedPatrol && !Button90("split-course"), $"Speed Patrol setup: no 'Start at' row; rows [{string.Join(", ", Rows091().Select(r => r.name))}]");
  yield return Late(() => Shot4k("98-setup-patrol")); yield return Walk091("Speed Patrol setup (1 player)", true, () => Page90 == "");
  SplitScreen.PoliceGame = SplitScreen.Game.CopRunner; SplitScreen.Solo = false; Menus089.Show(); yield return Settle091(); yield return Choose091("police");
  Check(Page90 == "police" && !Button90("split-course"), $"Cop vs Runner, 2 players: no 'Start at' row; rows [{string.Join(", ", Rows091().Select(r => r.name))}]");
  SplitScreen.Solo = true;
 }

 // ---- the storm drain, driven end to end ----
 IEnumerator Tunnel098(string vehicle, int dir, string light) {
  yield return EnterRoam("StreetLoopGreybox", vehicle); AudioListener.volume = 0;
  WorldLook.Current?.Pin(LookPresets.Compose(light == "night" ? TimeOfDay.Night : TimeOfDay.Day, Weather.Clear));
  var wr = FindObjectsByType<WoodlandRoute>(FindObjectsSortMode.None).FirstOrDefault(w => w.title != null && w.title.Contains("Storm Drain")); if (!wr) { Check(false, "no Storm Drain trail in Free Roam"); yield break; }
  wr.Initialize(); var pts = new List<Vector3>(); for (float s = 0; s < wr.Length; s += 5) pts.Add(wr.At(s, out _)); pts.Add(wr.At(wr.Length - .1f, out _));
  // the culvert part only: points inside the long box's footprint (x 105..175, z -75..80), with 30 m of approach each side
  int first = pts.FindIndex(p => p.z < 82 && p.z > -80 && p.x > 100 && p.x < 180), last = pts.FindLastIndex(p => p.z < 82 && p.z > -80 && p.x > 100 && p.x < 180);
  if (first < 0) { Check(false, "the trail does not pass the drain: " + string.Join(" ", pts.Take(6).Select(V))); yield break; }
  first = Mathf.Max(0, first - 6); last = Mathf.Min(pts.Count - 1, last + 6); var path = pts.GetRange(first, last - first + 1); if (dir < 0) path.Reverse();
  Note($"drain path: {path.Count} points from {V(path[0])} to {V(path[^1])}, lowest {path.Min(p => p.y):F1}");
  var car = race.vehicle; car.GetComponent<VehicleInput>().enabled = false; var d0 = path[1] - path[0]; d0.y = 0;
  CapPut(path[0], Quaternion.LookRotation(d0).eulerAngles.y, 0);
  var resp = car.GetComponent<VehicleRespawn>(); int resets = 0; Action onR = () => resets++; resp.Respawned += onR;
  var pilot = car.gameObject.AddComponent<Pilot097>(); pilot.Car = car; pilot.Path = path; pilot.Speed = vehicle == "moto" ? 14 : 12; pilot.Look = 9;
  float t0 = Time.time, minUp = 1, minSpeed = 99; float stuck = 0; Vector3 lastPos = car.Body.position; float lastMove = Time.time; float below = 0;
  var cam = Camera.main; bool shotIn = false;
  while (!pilot.Done && Time.time - t0 < 90) { AudioListener.volume = 0; yield return null; minUp = Mathf.Min(minUp, car.transform.up.y); if (Time.time - t0 > 3) minSpeed = Mathf.Min(minSpeed, car.ForwardSpeed);
   if (Vector3.Distance(car.Body.position, lastPos) > 1.5f) { lastPos = car.Body.position; lastMove = Time.time; } if (Time.time - lastMove > 6) { stuck = Time.time - lastMove; break; }
   if (!shotIn && car.Body.position.z < 50 && dir > 0 || !shotIn && car.Body.position.z > 40 && dir < 0 && Time.time - t0 > 3) { shotIn = true; yield return Late(() => Shot($"tunnel98-{vehicle}-{(dir > 0 ? "down" : "up")}-{light}-inside")); } }
  resp.Respawned -= onR; pilot.enabled = false; car.enabled = true;
  Check(pilot.Done && resets == 0 && stuck == 0, $"{vehicle} {(dir > 0 ? "down" : "up")} the storm drain, {light}: {(pilot.Done ? "reached the far end" : "DID NOT reach the end")} in {Time.time - t0:F1} s, resets {resets}, stuck {stuck:F0} s, lowest up {minUp:F2}, slowest {minSpeed:F1} m/s, end at {V(car.Body.position)}");
  flow.QuitRace(); yield return null;
 }

 // ---- Getaway in the Free Roam world: a human-like runner on the main road ----
 IEnumerator Chase98(string kind, string vehicle, int seed, int minutes) {
  UnityEngine.Random.InitState(seed * 7919 + 13); mode96 = "road";
  yield return StartGetaway096(vehicle, 1, minutes); var g = GetawayChase.Current; var car = race.vehicle; var runner = g.Runners[0];
  float t0 = Time.time; var wait0 = Time.realtimeSinceStartup;
  Check(g.Cops.Count >= 1, $"at the start: {g.Cops.Count} cops placed; nearest {g.Cops.Min(c => Vector3.Distance(c.car.Body.position, runner.body.position)):F0} m behind; siren on {g.Cops.All(c => c.lights.Siren)}; frozen {g.Cops.Count(c => c.driver.Frozen)}");
  while (g.State == GetawayChase.Phase.Starting && Time.time - t0 < 20) yield return null;
  var human = car.gameObject.AddComponent<Runner097>(); human.G = g; human.Car = car; human.Human = false; human.Trails = false; human.Cap = kind == "roadhalf" ? car.topSpeed * .5f : 0; car.GetComponent<VehicleInput>().enabled = false;
  var csv = new StringBuilder("t,heat,alert,cops,near150,near600,nearest,runnerSpeed,seen,escape,bust,heli,heliDist,heliOnScreen,heliLOS,heliAudio,heliVol,radio\n");
  Time.timeScale = 3; float real0 = Time.realtimeSinceStartup; int lastHeat = 1; float nextLog = 0; float heliAt = -1; var heatAt = new Dictionary<int, float> { { 1, 0 } }; int radioSeen = 0; bool shotH4 = false, shotAlertSeq = false; float seqNext = -1; int seq = 0;
  var cam = Camera.main; int maxNear = 0; var shotsAt = new HashSet<int>();
  while (g.State != GetawayChase.Phase.Over && g.State != GetawayChase.Phase.Done && flow.State == RaceFlow.Stage.Racing && Time.realtimeSinceStartup - real0 < 900) {
   AudioListener.volume = 0; yield return null;
   if (g.Heat != lastHeat) { lastHeat = g.Heat; heatAt[g.Heat] = g.Clock; Note($"   [{g.Clock:F0}s] HEAT {g.Heat}: alert '{g.Alert}', cops {g.Cops.Count(c => !c.block)}, near600 {NearCount098(g, runner)}");
    if (g.Heat >= 2 && g.Heat <= 5 && !shotsAt.Contains(g.Heat)) { shotsAt.Add(g.Heat); Time.timeScale = 1; yield return new WaitForSecondsRealtime(.4f); int h = g.Heat; yield return Late(() => Shot($"chase98-{kind}-{vehicle}-heat{h}-alert")); Time.timeScale = 3; } }
   if (g.Heli && heliAt < 0) { heliAt = g.Clock; Note($"   [{g.Clock:F0}s] HELICOPTER spawned at {V(g.Heli.transform.position)}, {Vector3.Distance(g.Heli.transform.position, runner.body.position):F0} m from the runner"); seqNext = Time.realtimeSinceStartup + 1.5f; Time.timeScale = 1; }
   if (g.Heli && seq < 5 && Time.realtimeSinceStartup >= seqNext) { int n = ++seq; seqNext = Time.realtimeSinceStartup + 1.6f; yield return Late(() => Shot($"chase98-{kind}-{vehicle}-heli-seq{n}")); if (n == 3) yield return Late(() => Shot4k($"chase98-{kind}-{vehicle}-heli-4k")); if (seq == 5) Time.timeScale = 3; }
   maxNear = Mathf.Max(maxNear, NearCount098(g, runner));
   int sec = Mathf.FloorToInt(g.Clock);
   if (g.Clock >= nextLog) { nextLog = g.Clock + 1;
    var hp = g.Heli ? g.Heli.transform.position : Vector3.zero; bool onScreen = false, los = false; float vol = 0; bool playing = false;
    if (g.Heli) { var v = cam.WorldToViewportPoint(hp); onScreen = v.z > 0 && v.x > 0 && v.x < 1 && v.y > 0 && v.y < 1; los = !Physics.Linecast(cam.transform.position, hp, out var hit, ~0, QueryTriggerInteraction.Ignore) || hit.collider.GetComponentInParent<PoliceHelicopter>(); var src = g.Heli.GetComponent<AudioSource>(); playing = src && src.isPlaying; vol = src ? src.volume : 0; }
    csv.AppendLine($"{g.Clock:F0},{g.Heat},\"{(g.AlertShown ? g.Alert : "")}\",{g.Cops.Count(c => !c.block)},{g.Cops.Count(c => !c.block && Vector3.Distance(c.car.Body.position, runner.body.position) < 150)},{NearCount098(g, runner)},{(g.Cops.Count > 0 ? g.Cops.Min(c => Vector3.Distance(c.car.Body.position, runner.body.position)) : -1):F0},{runner.body.linearVelocity.magnitude:F1},{runner.seen},{runner.escape:F2},{runner.bust:F2},{(g.Heli ? "1" : "0")},{(g.Heli ? Vector3.Distance(hp, runner.body.position) : -1):F0},{onScreen},{los},{playing},{vol:F1},\"{g.Radio}\"");
    File.WriteAllText(Path.Combine(output, $"chase98-{kind}-{vehicle}-{seed}.csv"), csv.ToString()); }
   if (g.RadioLog.Count > radioSeen) { for (int q = radioSeen; q < g.RadioLog.Count; q++) Note($"   [{g.Clock:F0}s] radio: {g.RadioLog[q]}"); radioSeen = g.RadioLog.Count; }
  }
  Time.timeScale = 1; var r = g.Runners[0];
  Note($"chase98 {kind} {vehicle} seed {seed}: {r.outcome} at {r.freeSeconds:F0} s, top heat {r.topHeat}, heat reached at [{string.Join(", ", heatAt.OrderBy(k => k.Key).Select(k => "H" + k.Key + "@" + k.Value.ToString("F0") + "s"))}], helicopter {(heliAt >= 0 ? "spawned at " + heliAt.ToString("F0") + " s" : "never")}, most cops within 600 m {maxNear}");
  Check(r.Done, $"{kind}: round ended ({r.outcome})");
  csv.AppendLine(); File.WriteAllText(Path.Combine(output, $"chase98-{kind}-{vehicle}-{seed}.csv"), csv.ToString());
  if (g.State == GetawayChase.Phase.Over) { float w0 = Time.realtimeSinceStartup; while (flow.State != RaceFlow.Stage.Results && Time.realtimeSinceStartup - w0 < 30) yield return null; }
  flow.QuitSplit(false); yield return WaitScene095(() => !SplitScreen.Active && flow.State == RaceFlow.Stage.Ready);
 }
 static int NearCount098(GetawayChase g, GetawayChase.Runner r) => g.Cops.Count(c => c.car && !c.block && Vector3.Distance(c.car.Body.position, r.body.position) <= 600);

 // ---- the radio line: the longest one, at 4K and in a split half ----
 IEnumerator Radio098() {
  foreach (bool two in new[] { false, true }) {
   mode96 = "road"; yield return StartGetaway096("moto", 1, 5, two); var g = GetawayChase.Current;
   float t0 = Time.time; while (g.State == GetawayChase.Phase.Starting && Time.time - t0 < 20) yield return null;
   var names = Enumerable.Range(0, g.Net.Roads.Count).Select(i => g.Net.RoadNames[i]).Distinct().OrderByDescending(n => n.Length).Take(2).ToArray();
   string longest = $"4 units posted at the exits near {names[0]} and {names[1]}"; var say = typeof(GetawayChase).GetMethod("Say", BindingFlags.NonPublic | BindingFlags.Instance);
   say.Invoke(g, new object[] { longest, 0 }); yield return new WaitForSecondsRealtime(.5f);
   var fieldAlert = typeof(GetawayChase).GetMethod("BumpHeat"); fieldAlert.Invoke(g, new object[] { "more units on the way" }); yield return new WaitForSecondsRealtime(.3f);
   say.Invoke(g, new object[] { longest, 0 }); yield return new WaitForSecondsRealtime(.4f);
   Note($"radio line ({longest.Length} characters): {longest}");
   yield return Late(() => Shot4k("98-radio-" + (two ? "split" : "full")));
   foreach (var rt in FindObjectsByType<RectTransform>(FindObjectsSortMode.None).Where(r => r.name.EndsWith(" HUD") && r.gameObject.activeInHierarchy)) Note($"   {rt.name}: rect {rt.rect.width:F0} x {rt.rect.height:F0} units, anchors {rt.anchorMin}-{rt.anchorMax}; screen {Screen.width} x {Screen.height}; canvas scale {rt.lossyScale.x:F3}");
   flow.QuitSplit(false); yield return WaitScene095(() => !SplitScreen.Active && flow.State == RaceFlow.Stage.Ready);
  }
 }
}
}
#endif
