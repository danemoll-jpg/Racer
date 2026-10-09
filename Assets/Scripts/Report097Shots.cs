#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine;

namespace Racer {
// 0.97 Part C: a plain waypoint driver for the trailer shots that are not on a road the game's AI knows (the lawn, the fence line, the tunnel).
public sealed class Pilot097 : MonoBehaviour {
 public ArcadeVehicle Car; public List<Vector3> Path = new(); public float Speed = 20, Look = 12, Finish = 0; public bool Done; int seg;
 void FixedUpdate() {
  if (!Car || Path.Count < 2 || Car.Body.isKinematic) return; Car.enabled = false; float dt = Time.fixedDeltaTime; var p = Car.Body.position; float speed = Car.ForwardSpeed;
  while (seg + 1 < Path.Count - 1) { var a = Path[seg]; var b = Path[seg + 1]; var ab = b - a; ab.y = 0; var ap = p - a; ap.y = 0; if (Vector3.Dot(ap, ab) > ab.sqrMagnitude) seg++; else break; }
  float left = Look; int i = seg; var cur = p; Vector3 target = Path[Path.Count - 1];
  while (i + 1 < Path.Count) { var end = Path[i + 1]; float len = Vector3.Distance(new Vector3(cur.x, 0, cur.z), new Vector3(end.x, 0, end.z)); if (len >= left) { var d = end - cur; d.y = 0; target = cur + d.normalized * left; break; } left -= len; cur = end; i++; }
  float toEnd = Vector3.Distance(new Vector3(p.x, 0, p.z), new Vector3(Path[^1].x, 0, Path[^1].z)); Done = toEnd < 4;
  float want = Done ? Finish : Speed; var local = Car.transform.InverseTransformPoint(target); float angle = Mathf.Atan2(local.x, local.z);
  float maxAngle = Mathf.Lerp(Car.slowSteerAngle, Car.fastSteerAngle, Mathf.Clamp01(Mathf.Abs(speed) / Car.topSpeed)) * Mathf.Deg2Rad;
  float steering = Mathf.Clamp(Mathf.Atan(2 * Car.wheelbase * Mathf.Sin(angle) / Look) / maxAngle, -1, 1);
  float throttle = speed < want ? Mathf.Clamp01((want - speed) * .5f) : 0, brake = speed > want + .5f ? Mathf.Clamp01((speed - want) * .3f) : 0;
  Car.Simulate(throttle, brake, steering, dt);
 }
}

public sealed partial class Report080Checks {
 static readonly FieldInfo plantField = typeof(CameraViews).GetField("plant", BindingFlags.Instance | BindingFlags.NonPublic);
 static Vector3 Ground(Vector3 p, float lift = 0) {
  var hits = Physics.RaycastAll(p + Vector3.up * 60, Vector3.down, 140, ~0, QueryTriggerInteraction.Ignore); float best = float.NaN;
  foreach (var h in hits) { var n = h.collider.name; if (h.collider is TerrainCollider || n.StartsWith("Ground") || n.IndexOf("road", StringComparison.OrdinalIgnoreCase) >= 0 || n.IndexOf("terrain", StringComparison.OrdinalIgnoreCase) >= 0 || n.IndexOf("pavement", StringComparison.OrdinalIgnoreCase) >= 0) { if (float.IsNaN(best) || h.point.y > best) best = h.point.y; } }
  if (float.IsNaN(best) && hits.Length > 0) best = hits.Min(h => h.point.y);
  if (!float.IsNaN(best)) p.y = best; return p + Vector3.up * lift; }
 void CapPut(Vector3 pos, float yawDeg, float speed) {
  var car = race.vehicle; var at = Ground(pos, 1.1f); var rot = Quaternion.Euler(0, yawDeg, 0);
  car.Body.position = at; car.Body.rotation = rot; car.transform.SetPositionAndRotation(at, rot); car.Body.linearVelocity = rot * Vector3.forward * speed; car.Body.angularVelocity = Vector3.zero; car.ClearSteering();
 }
 void CapPlant(Vector3 spot) { var v = CameraViews.Current; v.SelectTrailerCamera(CameraViews.Shot.Fixed); plantField.SetValue(v, spot); }
 // Dawn and so on: a shot on the ambient road near a world point. The camera is chosen by `cam`; for Fixed the camera is planted `plantAhead` m ahead of the car (along its heading), `plantSide` m to its right.
 IEnumerator CapRoadShot(string name, string vehicle, float hour, Weather weather, CameraViews.Shot cam, float seconds, Vector3 near, float pace, float plantAhead = 0, float plantSide = 5, float plantUp = 1.4f, float moon = -1, int dir = -1, float warm = 1.5f) {
  yield return CapRoam(vehicle, -1, null, hour, weather, moon);
  var car = race.vehicle; var pilot = car.gameObject.AddComponent<RoadDriver>(); pilot.Initialize(race, car, false, dir, pace); car.GetComponent<VehicleInput>().enabled = false;
  var road = pilot.DriveRoad; float s = road.Project(near, out _); pilot.Place(s, road.TrafficLane(s, dir));
  yield return new WaitForSecondsRealtime(.7f);
  var v = CameraViews.Current;
  if (cam == CameraViews.Shot.Fixed) { var vel = car.Body.linearVelocity; vel.y = 0; var f = vel.sqrMagnitude > 1 ? vel.normalized : car.transform.forward; var spot = Ground(car.transform.position + f * plantAhead + Vector3.Cross(Vector3.up, f) * -plantSide); spot.y += plantUp; CapPlant(spot); }
  else v.SelectTrailerCamera(cam);
  yield return new WaitForSecondsRealtime(warm);
  yield return CapRecord(name, seconds);
  Destroy(pilot); flow.QuitRace(); yield return null;
 }
 IEnumerator CapPathShot(string name, string vehicle, float hour, Weather weather, CameraViews.Shot cam, float seconds, List<Vector3> path, float speed, float startSpeed = 0, Vector3? plant = null, float moon = -1, float warm = 1.5f, float look = 12, int color = -1, bool slowFlight = false) {
  yield return CapRoam(vehicle, color, null, hour, weather, moon);
  var car = race.vehicle; car.GetComponent<VehicleInput>().enabled = false; var d = path[1] - path[0]; d.y = 0;
  CapPut(path[0], Quaternion.LookRotation(d).eulerAngles.y, startSpeed);
  var pilot = car.gameObject.AddComponent<Pilot097>(); pilot.Car = car; pilot.Path = path; pilot.Speed = speed; pilot.Look = look;
  yield return new WaitForSecondsRealtime(.3f);
  if (cam == CameraViews.Shot.Fixed && plant.HasValue) CapPlant(plant.Value); else CameraViews.Current.SelectTrailerCamera(cam);
  yield return new WaitForSecondsRealtime(warm);
  float air = 0; bool slow = false; float baseFixed = Time.fixedDeltaTime;
  yield return CapRecord(name, seconds, slowFlight ? t => { air = car.GroundedWheels == 0 ? air + Time.unscaledDeltaTime : 0; bool want = air > .12f && car.Body.linearVelocity.magnitude > 12; if (want != slow) { slow = want; Time.timeScale = want ? .3f : 1f; Time.fixedDeltaTime = baseFixed * (want ? .3f : 1f); } } : null);
  Time.timeScale = 1; Time.fixedDeltaTime = baseFixed;
  Destroy(pilot); flow.QuitRace(); yield return null;
 }
 RaceRoad CapNamedRoad(string part) { var r = FindObjectsByType<RaceRoad>(FindObjectsSortMode.None).FirstOrDefault(x => x.name.Contains(part)); if (r) r.Initialize(); return r; }
 static List<Vector3> RoadPath(RaceRoad r, float s0, float s1, float step = 6, float side = 0) { var l = new List<Vector3>(); for (float s = s0; s <= s1; s += step) { var p = r.At(s, out var f); l.Add(p + Vector3.Cross(Vector3.up, f) * side); } return l; }

 // shot number -> the clip. Shots on the ambient road use the Dan's-house end of the loop (near the houses and fences) unless noted.
 IEnumerator CapShot(int n) {
  var house = new Vector3(476, 87, -45); var woods = new Vector3(300, 40, 400);
  switch (n) {
   case 1: return CapRoadShot("01-empty-road", "moto", 6.1f, Weather.Clear, CameraViews.Shot.Fixed, 9, woods, 1.05f, plantAhead: 75, plantSide: 6, plantUp: 1.3f, warm: 0.5f);
   case 5: return CapRoadShot("05-fast-past-houses", "moto", 12f, Weather.Clear, CameraViews.Shot.Chase, 6, house, 1.15f);
   case 6: return CapRoadShot("06-first-person", "moto", 12f, Weather.Clear, CameraViews.Shot.FirstPerson, 5, woods, 1.15f);
   case 12: return CapRoadShot("12-wet-road", "original", 18.2f, Weather.Rain, CameraViews.Shot.Chase, 5, woods, 1.1f);
   case 7: return CapPathShot("07-fence-smash", "atv", 12f, Weather.Clear, CameraViews.Shot.Side, 6, new List<Vector3> { Ground(new Vector3(488, 0, -15.3f)), Ground(new Vector3(461.5f, 0, -15.3f)), Ground(new Vector3(436, 0, -15.3f)) }, 24, startSpeed: 16);
   case 10: return CapKyle();
   case 8: return CapTunnel();
   case 9: { var c = new Vector3(880.2f, 115.5f, -84.9f); var f = new Vector3(.9f, 0, .3f).normalized; return CapPathShot("09-creek-leap", "moto", 18.4f, Weather.Clear, CameraViews.Shot.Chase, 8, new List<Vector3> { Ground(c - f * 70), Ground(c), Ground(c + f * 70) }, 24, startSpeed: 15, slowFlight: true); }
   case 11: { var p0 = new Vector3(891.5f, 150.4f, 152.6f); var f = Quaternion.Euler(0, 262, 0) * Vector3.forward; var path = new List<Vector3> { Ground(p0), Ground(p0 + f * 40), Ground(p0 + f * 90) }; return CapLightning("11-lightning", path, p0 + Vector3.Cross(Vector3.up, f) * -9 + f * 22); }
   case 13: { var lane = new[] { new Vector3(520.5f, 0, -173.1f), new Vector3(534.1f, 0, -207.1f) }; var d = (lane[1] - lane[0]).normalized; var side = Vector3.Cross(Vector3.up, d); var a = lane[0] - d * 40 + side * 9; var b = lane[1] + d * 40 + side * 9; return CapPathShot("13-sledders", "moto", 12f, Weather.Snow, CameraViews.Shot.Fixed, 6, new List<Vector3> { Ground(a), Ground(b) }, 14, startSpeed: 10, plant: Ground(lane[0] + d * 22 + side * 16, 1.6f)); }
   case 14: return CapHockey();
   case 17: { var site = new Vector3(980.4f, 177.9f, 147.3f); var f = new Vector3(-1, 0, -.3f).normalized; var from = new Vector3(1187.2f, 152f, 206f); return CapPathShot("17-giant-jump", "atv", 19.0f, Weather.Clear, CameraViews.Shot.Side, 12, new List<Vector3> { Ground(from), Ground(site - f * 60), Ground(site), Ground(site + f * 100) }, 32, startSpeed: 20, moon: .5f, look: 16, slowFlight: true); }
   case 4: return CapRaceStart();
   case 15: return CapPolice();
   case 16: return CapSplit();
   case 2: return CapRider();
   case 3: return CapGarageVehicles();
   default: return null;
  }
 }
 IEnumerator CapKyle() {
  var path = new List<Vector3> { new Vector3(476f, 0, -12f), new Vector3(480f, 0, -18f), new Vector3(492f, 0, -27f), new Vector3(504f, 0, -29f) };
  for (int i = 0; i < path.Count; i++) path[i] = Ground(path[i]);
  yield return CapPathShot("10-mower-lawn", "mower", 12f, Weather.Clear, CameraViews.Shot.Side, 5, path, 14, startSpeed: 9);
 }
 IEnumerator CapTunnel() {
  var e = new Vector3(182.5f, 0, 83.5f); var f = Quaternion.Euler(0, 239, 0) * Vector3.forward;
  yield return CapPathShot("08-storm-drain", "moto", 23f, Weather.Clear, CameraViews.Shot.Chase, 6, new List<Vector3> { Ground(e - f * 30), Ground(e), Ground(e + f * 30), Ground(e + f * 70) }, 10, startSpeed: 6, look: 8);
 }
 IEnumerator CapLightning(string name, List<Vector3> path, Vector3 plant) {
  yield return CapRoam("moto", -1, null, 22f, Weather.Rain, .5f);
  var car = race.vehicle; car.GetComponent<VehicleInput>().enabled = false; var d = path[1] - path[0]; d.y = 0; CapPut(path[0], Quaternion.LookRotation(d).eulerAngles.y, 8);
  var pilot = car.gameObject.AddComponent<Pilot097>(); pilot.Car = car; pilot.Path = path; pilot.Speed = 9;
  yield return new WaitForSecondsRealtime(.3f); CapPlant(Ground(plant, 2.2f)); yield return new WaitForSecondsRealtime(1.5f);
  bool flashed = false;
  yield return CapRecord(name, 6, t => { if (!flashed && t > 2.2f) { flashed = true; TrailerMode.Instance.LightningNow(); } });
  Destroy(pilot); flow.QuitRace(); yield return null;
 }
 IEnumerator CapHockey() {
  yield return CapRoam("moto", -1, null, 12f, Weather.Snow);
  var car = race.vehicle; car.GetComponent<VehicleInput>().enabled = false; car.enabled = false; CapPut(new Vector3(404f, 80f, -12.5f), 270, 0); car.Body.isKinematic = true;
  CameraViews.Current.SelectTrailerCamera(CameraViews.Shot.Orbit); yield return new WaitForSecondsRealtime(1.5f);
  yield return CapRecord("14-broom-hockey", 6);
  car.Body.isKinematic = false; flow.QuitRace(); yield return null;
 }


 // the editor's Game view at a fixed render size (so the garage and the split-screen are recorded at a real resolution, not the 732 x 290 window)
 static bool capViewSet;
 bool CapGameView(int w, int h) {
  if (capViewSet) return true;
  try {
   var asm = typeof(UnityEditor.Editor).Assembly; var sizesType = asm.GetType("UnityEditor.GameViewSizes"); var singleType = typeof(UnityEditor.ScriptableSingleton<>).MakeGenericType(sizesType);
   var instance = singleType.GetProperty("instance").GetValue(null); var group = sizesType.GetMethod("GetGroup").Invoke(instance, new object[] { 0 });
   var gvsType = asm.GetType("UnityEditor.GameViewSize"); var typeEnum = asm.GetType("UnityEditor.GameViewSizeType"); var ctor = gvsType.GetConstructor(new[] { typeEnum, typeof(int), typeof(int), typeof(string) });
   var size = ctor.Invoke(new object[] { Enum.ToObject(typeEnum, 1), w, h, "trailer " + w + "x" + h }); group.GetType().GetMethod("AddCustomSize").Invoke(group, new[] { size });
   int index = (int)group.GetType().GetMethod("GetTotalCount").Invoke(group, null) - 1;
   var gvType = asm.GetType("UnityEditor.GameView"); var gv = UnityEditor.EditorWindow.GetWindow(gvType); gvType.GetMethod("SizeSelectionCallback", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Invoke(gv, new object[] { index, size });
   gv.Repaint(); capViewSet = true; return true;
  } catch (Exception e) { CapNote("game view resize failed: " + e.Message); return false; }
 }
 // shot 4: the pack leaving the grid, side tracking low, the player under the racing AI
 IEnumerator CapRaceStart() {
  yield return Load("StreetLoopGreybox"); yield return Menu(); var st = flow.Save.Settings; st.master = 1; st.ambience = 1; st.radioOn = false; st.weather = 0; st.timeOfDay = 0; st.hints = false; st.unlockEverything = true; Campaign.Testing = true; VehicleUnlocks.Set(24, true); st.estimateAiFinishes = false; flow.Save.SaveSettings();
  FixColors97(); flow.OpenGarage(); flow.SelectVehicle("moto"); flow.CloseGarage(); race.opponents = true; race.traffic = false; race.laps = 1; race.difficulty = 1; race.opponentRoster = new[] { "moto", "atv", "original" };
  flow.StartRace(); float t0 = Time.realtimeSinceStartup; while (flow.State != RaceFlow.Stage.Countdown && flow.State != RaceFlow.Stage.Racing && Time.realtimeSinceStartup - t0 < 60) yield return null;
  var car = race.vehicle; var pilot = car.gameObject.AddComponent<RoadDriver>(); pilot.Initialize(race, car, true, 1, 1); pilot.Racer = race.Racers[0]; car.GetComponent<VehicleInput>().enabled = false;
  TrailerMode.Instance.Begin(); yield return null; yield return null; var c = WorldLook.Current.Trailer; if (c != null) { c.hour = 12; c.paused = true; }
  CameraViews.Current.SelectTrailerCamera(CameraViews.Shot.Side); AudioListener.volume = 1; CapNote($"race start: state {flow.State}");
  yield return CapRecord("04-race-start", 9);
  Destroy(pilot); flow.QuitRace(); yield return null;
 }
 // shot 15: Getaway at night, the cruisers' lights and the helicopter's spotlight on the runner
 IEnumerator CapPolice() {
  mode96 = "road"; yield return StartGetaway096Night("moto"); var g = GetawayChase.Current;
  float t0 = Time.time; while (g.State == GetawayChase.Phase.Starting && Time.time - t0 < 20) yield return null;
  var car = race.vehicle; var human = car.gameObject.AddComponent<Runner097>(); human.G = g; human.Car = car; human.Human = false; human.Trails = false; car.GetComponent<VehicleInput>().enabled = false;
  typeof(GetawayChase).GetProperty("Heat").SetValue(g, 5);
  float w0 = Time.realtimeSinceStartup; while (Time.realtimeSinceStartup - w0 < 25 && !(g.Heli && g.Cops.Count(c => !c.block && Vector3.Distance(c.car.Body.position, car.Body.position) < 60) >= 2)) { AudioListener.volume = 1; yield return null; }
  CapNote($"police: heli {(g.Heli ? "yes" : "no")}, cops {g.Cops.Count}, near {g.Cops.Count(c => !c.block && Vector3.Distance(c.car.Body.position, car.Body.position) < 60)}, state {g.State}");
  var tm = TrailerMode.Instance; if (tm) { tm.Begin(); yield return null; yield return null; }
  CameraViews.Current.SelectTrailerCamera(CameraViews.Shot.Chase); AudioListener.volume = 1; var cams = CameraViews.Current;
  yield return CapRecord("15-police", 10, t => { if (t > 5 && cams.TrailerCamera != CameraViews.Shot.Flyover) cams.SelectTrailerCamera(CameraViews.Shot.Flyover); g.Runners[0].bust = 0; g.Runners[0].escape = 0; });
  flow.QuitSplit(false); yield return null;
 }
 // shot 16: split-screen, two riders on the same road (the whole window is recorded)
 IEnumerator CapSplit() {
  CapGameView(1920, 1080); yield return null; yield return null; yield return DanCopy095(); Pads090(); SplitScreen.Solo = false; SplitScreen.P1Device = pad1; SplitScreen.P2Device = null; SplitScreen.P2Ai = true; SplitScreen.P2AiRunner = false; SplitScreen.Mode = SplitScreen.Kind.Race; SplitScreen.Course = 0; SplitScreen.Laps = 1; SplitScreen.Time = TimeOfDay.Day; SplitScreen.Weather = Weather.Clear; SplitScreen.Traffic = false; SplitScreen.Rivals = 0; SplitScreen.P1Vehicle = "moto"; SplitScreen.P2Vehicle = "atv";
  var st = flow.Save.Settings; st.master = 1; st.radioOn = false; st.hints = false; st.splitLeftRight = true; flow.Save.SaveSettings();
  flow.StartSplit(); float t0 = Time.realtimeSinceStartup; while (!(SplitScreen.Active && flow.State == RaceFlow.Stage.Racing) && Time.realtimeSinceStartup - t0 < 90) yield return null;
  var rd = flow.Race; var car = rd.vehicle; var pilot = car.gameObject.AddComponent<RoadDriver>(); pilot.Initialize(rd, car, true, 1, 1); pilot.Racer = rd.Racers[0]; car.GetComponent<VehicleInput>().enabled = false;
  CapNote($"split: window {Screen.width}x{Screen.height}"); yield return new WaitForSecondsRealtime(4); AudioListener.volume = 1;
  yield return CapRecord("16-split-screen", 6, screen: true);
  Destroy(pilot); flow.QuitSplit(false); yield return null;
 }
 RenderTexture GaragePreview() { var menus = FindAnyObjectByType<RaceMenus>(); var ri = (UnityEngine.UI.RawImage)typeof(RaceMenus).GetField("preview", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(menus); return ri.texture as RenderTexture; }
 IEnumerator CapGarageStart() {
  CapGameView(1920, 1080); yield return null; yield return null; yield return Load("StreetLoopGreybox"); yield return Menu(); var st = flow.Save.Settings; st.master = 1; st.radioOn = false; st.hints = false; st.unlockEverything = true; Campaign.Testing = true; VehicleUnlocks.Set(24, true); flow.Save.SaveSettings(); FixColors97();
  flow.OpenGarage(); yield return new WaitForSecondsRealtime(1.5f);
 }
 // a press and release of a pad button over two frames (the hook runs once a frame)
 int padPhase; void PadTap(GamepadButton b) { UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad91, new GamepadState().WithButton(b)); padPhase = 2; }
 void PadRelease() { if (padPhase > 0 && --padPhase == 0) UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad91, new GamepadState()); }
 // shot 3: every vehicle turning in the garage, one after another (the right D-pad on the vehicle row, the garage turning each one)
 IEnumerator CapGarageVehicles() {
  yield return CapGarageStart(); Pad091(); var menus = FindAnyObjectByType<RaceMenus>(); int count = VehicleProfile.All.Length; CapNote($"garage: window {Screen.width}x{Screen.height}, {count} vehicles");
  float each = .34f; int last = 0;
  yield return CapRecord("03-garage-vehicles", each * count, t => { PadRelease(); int i = Mathf.Min(count - 1, (int)(t / each)); if (i != last) { last = i; PadTap(GamepadButton.DpadRight); } menus.PreviewYaw = 200 + t * 150; }, null, null, true);
  flow.CloseGarage(); yield return null;
 }
 // shot 2: the rider page, a row changed every 0.8 s (hat, shirt, hair ...)
 IEnumerator CapRider() {
  yield return CapGarageStart(); Pad091(); var menus = FindAnyObjectByType<RaceMenus>(); flow.SelectVehicle("moto");
  typeof(RaceMenus).GetMethod("Navigate", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(menus, new object[] { "rider" }); yield return new WaitForSecondsRealtime(.8f);
  var buttons = (List<UnityEngine.UI.Button>)typeof(RaceMenus).GetField("buttons", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(menus);
  var rows = buttons.Where(b => b && b.gameObject.activeInHierarchy && b.interactable).ToList(); CapNote($"rider page: window {Screen.width}x{Screen.height}, {rows.Count} rows");
  float each = .8f; int last = -1;
  yield return CapRecord("02-rider-looks", each * 4, t => { PadRelease(); int i = (int)(t / each); if (i != last && rows.Count > 0) { last = i; UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(rows[Mathf.Min(rows.Count - 1, i * 2 + 1)].gameObject); PadTap(GamepadButton.DpadRight); } menus.PreviewYaw = 215 + t * 40; }, null, null, true);
  menus.BackPage(); flow.CloseGarage(); yield return null;
 }
}
}
#endif
