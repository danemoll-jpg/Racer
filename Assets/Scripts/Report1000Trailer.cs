#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Racer {
// 1.0 Part B: the trailer, filmed by the 0.97 capture mode (Report097Trailer.cs, TrailerRecorder) in the editor's play mode of the 1.0.0
// source, through Tools/Report1000/Trailer/Run-Trailer.ps1 (PROBE_CASES "cap100:<shot>[:take]"). Never in a release build or its menus.
// Every shot uses Dan's rider look and vehicle colors (from the copy of his save) and the game's scenes as they are: nothing in the
// world is moved for filming.
//   cap100:survey      where things are: the roads of Free Roam, the hidden-police places, the flights of the mountain courses,
//                      the shortcut routes, and where a car leaves the ground on the Street Loop (crest candidates for the cover shot)
public sealed partial class Report080Checks {
 IEnumerator Run100(string[] a) => a[1] switch {
  "survey" => Survey100(),
  _ => Shot100(a[1], a.Length > 2 ? int.Parse(a[2]) : 1),
 };
 IEnumerator Course100(int course) {
  yield return Menu(); Campaign.Testing = true;
  if (RacePlaylists.CourseOf(UnityEngine.SceneManagement.SceneManager.GetActiveScene()) != course) {
   flow.OpenCourses(); yield return null; flow.SelectCourseEntry(course); float t0 = Time.realtimeSinceStartup; yield return new WaitForSecondsRealtime(1);
   while (Time.realtimeSinceStartup - t0 < 120) { Bind(); if (flow && RacePlaylists.CourseOf(UnityEngine.SceneManagement.SceneManager.GetActiveScene()) == course && flow.Started && !LoadingScreen.Holding && flow.Save != null) break; yield return null; }
  }
  yield return new WaitForSecondsRealtime(1); Bind(); yield return Menu();
 }
 void Routes100(string where) {
  foreach (var w in FindObjectsByType<WoodlandRoute>(FindObjectsSortMode.None)) { w.Initialize(); CapNote($"{where} route '{w.title}' length {w.Length:F0} from {V(w.At(0, out _))} to {V(w.At(w.Length, out _))}"); }
  var f = race.GetComponent<MountainFlights>(); if (f) foreach (var x in f.flights) CapNote($"{where} flight '{x.name}' start {V(x.start)} lip {V(x.lip)} landing end {V(x.landingEnd)} forward {V(x.forward)} stations {x.approachStation:F0}-{x.endStation:F0}");
 }
 // the player's vehicle under the racing AI for a lap: every flight (where, how long, how fast)
 IEnumerator Flights100(string where, string vehicle, float seconds) {
  race.opponents = false; race.traffic = false; race.laps = 1; flow.OpenGarage(); flow.SelectVehicle(vehicle); flow.CloseGarage(); flow.StartRace();
  float t0 = Time.realtimeSinceStartup; while (flow.State != RaceFlow.Stage.Racing && Time.realtimeSinceStartup - t0 < 60) yield return null; Bind();
  var car = race.vehicle; var pilot = car.gameObject.AddComponent<RoadDriver>(); pilot.Initialize(race, car, true, 1, 1); pilot.Racer = race.Racers[0]; car.GetComponent<VehicleInput>().enabled = false;
  Time.timeScale = 3; float air = 0; Vector3 from = default; float r0 = Time.realtimeSinceStartup;
  while (Time.realtimeSinceStartup - r0 < seconds && !race.Racers[0].Progress.Finished) {
   yield return new WaitForFixedUpdate(); AudioListener.volume = 0; if (Time.timeScale == 0) Time.timeScale = 3;
   if (car.GroundedWheels == 0) { if (air == 0) from = car.Body.position; air += Time.fixedDeltaTime; }
   else { if (air > .35f) CapNote($"{where} {vehicle} flight {air:F2} s from {V(from)} to {V(car.Body.position)} speed {car.Body.linearVelocity.magnitude:F1} heading {car.transform.eulerAngles.y:F0} road s {race.road.Project(from, out _):F0}"); air = 0; }
  }
  Time.timeScale = 1; Destroy(pilot); flow.Pause(); flow.QuitRace(); yield return null;
 }
 IEnumerator Survey100() {
  yield return CapRoam("moto", -1, null, 12, Weather.Clear);
  foreach (var r in FindObjectsByType<RaceRoad>(FindObjectsSortMode.None)) { r.Initialize(); CapNote($"roam road '{r.name}' length {r.Length:F0} from {V(r.At(0, out _))} to {V(r.At(r.Length, out _))}"); }
  var hp = HiddenPolice.Current; CapNote($"hidden police: {(hp ? hp.All.Count + " places, active " + hp.Active.Count : "none (trailer mode)")}");
  if (hp) foreach (var s in hp.All) CapNote($"  police place {s.road} at {V(s.pos)} facing {V(s.forward)}{(hp.Active.Contains(s) ? " ACTIVE" : "")}");
  foreach (var (pos, fwd) in Hwy92Roadside.PoliceSpots(race.gameObject.scene)) CapNote($"  Hwy 92 lot place at {V(pos)} facing {V(fwd)}");
  CapNote($"stunt park entrance {V(StuntPark.Entrance)} start {V(StuntPark.StartPos)} loop top {V(StuntPark.LoopTop)}");
  flow.QuitRace(); yield return null;
  yield return Course100(0); Routes100("Street Loop F"); yield return Flights100("Street Loop F", "original", 120);
  yield return Course100(6); Routes100("Backyard F");
  yield return Course100(4); Routes100("Mountain Loop F");
  yield return Course100(8); Routes100("Leviathan F");
 }
}
}
#endif
