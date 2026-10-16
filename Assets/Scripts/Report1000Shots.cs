#if UNITY_EDITOR
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Racer {
// 1.0 Part B: the trailer's shots (Docs/Trailer/shot-list.md), "cap100:<shot>[:take]" (Report1000Trailer.cs). Editor only, capture only.
public sealed partial class Report080Checks {
 const BindingFlags Any100 = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
 // ---------------------------------------------------------------- the look: Dan's rider and colors (his settings.json, 2026-10-16), the same in every shot
 static readonly int[] DanColors = { 1, 1, 1, 2, 2, 6, -1, 1, 1, 3, 1, -1, -1, 1, -1, -1, -1, -1, -1, -1 };
 static RiderLook DanRider => new RiderLook { body = 0, skin = 1, hair = 5, hairColor = 0, hat = 0, hatColor = 4, shirt = 1, shirtColor = 6, pants = 0, pantsColor = 6 };
 void Look100() {
  if (!flow || flow.Save == null) return; var st = flow.Save.Settings; st.rider = DanRider; RiderLook.Player = st.rider;
  FixColors97(); for (int i = 0; i < st.bodyColors.Length && i < DanColors.Length; i++) st.bodyColors[i] = DanColors[i];
  st.master = 1; st.ambience = 1; st.radioOn = false; st.hints = false; st.timeOfDay = 0; st.weather = 0; st.unlockEverything = true; Campaign.Testing = true; VehicleUnlocks.Set(24, true); st.estimateAiFinishes = false; flow.Save.SaveSettings();
 }
 // the 0.97 shot helpers start in Free Roam from the menu: the look goes into the save first
 IEnumerator Look0() { yield return Load("StreetLoopGreybox"); yield return Menu(); Look100(); }
 // Free Roam for a shot: the look, the vehicle, then the conditions (Trailer Mode on, unless `trailer` is false: then the plain clock)
 IEnumerator Roam100(string vehicle, float hour, Weather weather, float moon = -1, bool trailer = true) {
  yield return Look0();
  if (!trailer && TrailerMode.Active && TrailerMode.Instance) TrailerMode.Instance.End(); // Trailer Mode carries over between scenes; the hidden cops sleep in it
  if (trailer) { yield return CapRoam(vehicle, -1, DanRider, hour, weather, moon); yield break; }
  var st = flow.Save.Settings; st.roamWeather = (int)weather; st.hiddenPolice = true; flow.Save.SaveSettings(); flow.OpenGarage(); flow.SelectVehicle(vehicle); flow.CloseGarage();
  flow.StartFreeRoam(); float t0 = Time.realtimeSinceStartup;
  while (Time.realtimeSinceStartup - t0 < 120) { yield return null; Bind(); if (flow.State == RaceFlow.Stage.Racing && race.gameObject.scene.name == RaceFlow.RoamScene) break; }
  yield return new WaitForSecondsRealtime(3); Bind(); WorldLook.Current?.SetClock(WorldLook.Current.Day, hour); AudioListener.volume = 1;
 }
 // a race for a shot: the course, the look, the field; returns when racing (the player under the racing AI unless `drive` is false)
 IEnumerator Race100(int course, string vehicle, string[] rivals, int difficulty, float hour, Weather weather, bool stunts = false, int laps = 1, bool drive = true) {
  yield return Course100(course); Look100(); Bind();
  if (race.stunts != stunts) { flow.StepRaceType(1); yield return null; yield return Menu(); Bind(); }
  flow.OpenGarage(); flow.SelectVehicle(vehicle); flow.CloseGarage();
  var st = flow.Save.Settings; st.opponents = rivals.Length > 0; st.traffic = false; st.laps = laps; st.difficulty = difficulty; if (rivals.Length > 0) st.opponentRoster = rivals; flow.Save.SaveSettings();
  race.opponents = rivals.Length > 0; race.traffic = false; race.laps = laps; race.difficulty = difficulty; if (rivals.Length > 0) race.opponentRoster = (string[])rivals.Clone();
  flow.StartRace(); float t0 = Time.realtimeSinceStartup; while (flow.State != RaceFlow.Stage.Countdown && flow.State != RaceFlow.Stage.Racing && Time.realtimeSinceStartup - t0 < 90) yield return null;
  Bind(); race.finishGraceSeconds = 100000;
  if (drive) { var car = race.vehicle; var pilot = car.gameObject.AddComponent<RoadDriver>(); pilot.Initialize(race, car, true, 1, 1); pilot.Racer = race.Racers[0]; car.GetComponent<VehicleInput>().enabled = false; }
  TrailerMode.Instance.Begin(); yield return null; yield return null; var c = WorldLook.Current.Trailer; if (c != null) { c.hour = hour; c.weather = weather; c.paused = true; }
  while (flow.State == RaceFlow.Stage.Countdown && Time.realtimeSinceStartup - t0 < 120) yield return null;
  AudioListener.volume = 1; CapNote($"race: {race.courseName} ({race.courseId}), stunts {race.stunts}, {race.Racers.Count} racers: {string.Join(", ", race.Racers.Select(r => r.Name + " " + r.Car.GetComponent<VehicleConfiguration>().profileId))}");
 }
 // signed distance along the course from `origin` to `s` (RaceRoad.Relative wraps to 0..length)
 float Rel100(float s, float origin) { float r = race.road.Relative(s, origin); return r > race.road.Length / 2 ? r - race.road.Length : r; }
 void EndRace100() { Time.timeScale = 1; Time.fixedDeltaTime = .02f; if (TrailerMode.Instance) TrailerMode.Instance.End(); flow.Pause(); flow.QuitRace(); }
 // fast-forward (not recorded) until `until`, at 3x
 IEnumerator Skip100(Func<bool> until, float realSeconds = 300) {
  Time.timeScale = 3; float r0 = Time.realtimeSinceStartup;
  while (Time.realtimeSinceStartup - r0 < realSeconds && !until()) { AudioListener.volume = 0; yield return null; if (Time.timeScale == 0) Time.timeScale = 3; }
  Time.timeScale = 1; AudioListener.volume = 1;
 }
 // slow motion while any of `targets` is in the air: real slow motion on the fixed step, never stretched
 Action<float> SlowAir100(Func<IEnumerable<ArcadeVehicle>> targets, float scale = .3f) {
  float air = 0; bool slow = false;
  return t => { bool any = targets().Any(c => c && c.GroundedWheels == 0 && c.Body.linearVelocity.magnitude > 8); air = any ? air + Time.unscaledDeltaTime : 0; bool want = air > .1f; if (want != slow) { slow = want; Time.timeScale = want ? scale : 1f; Time.fixedDeltaTime = .02f * (want ? scale : 1f); } };
 }
 void Distance100(CameraViews.Shot shot, float scale, float up = 0) {
  var v = CameraViews.Current; ((float[])typeof(CameraViews).GetField("distance", Any100).GetValue(v))[(int)shot] = scale; ((float[])typeof(CameraViews).GetField("height", Any100).GetValue(v))[(int)shot] = up;
 }

 // ---------------------------------------------------------------- the composite recorder: every camera and the screen-space UI into one render texture
 // (batch mode has no screen): split-screen views keep their viewport rects; overlay canvases are drawn by a UI camera over them, for the clip only.
 void Still100(RenderTexture rt, string name) { var old = RenderTexture.active; RenderTexture.active = rt; var t = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false); t.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0); t.Apply(); RenderTexture.active = old; System.IO.File.WriteAllBytes(System.IO.Path.Combine(capOut, name + ".png"), t.EncodeToPNG()); Destroy(t); }
 IEnumerator Composite100(string name, float seconds, Action<float> hook = null, int w = 2560, int h = 1440) {
  if (TrailerMode.Active && TrailerMode.Instance) TrailerMode.Instance.End(); // its panel and its hidden HUD carry over between scenes
  yield return null;
  var rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB) { name = "Trailer composite" }; rt.Create();
  // the UI draws into a transparent texture of its own and is laid over the cameras' picture (a depth-only camera here cleared the picture)
  var urt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB) { name = "Trailer UI" }; urt.Create(); var over = new Material(Shader.Find("UI/Default"));
  var ui = new GameObject("Trailer UI camera").AddComponent<Camera>(); ui.enabled = false; ui.clearFlags = CameraClearFlags.SolidColor; ui.backgroundColor = new Color(0, 0, 0, 0); ui.cullingMask = 1 << 31; ui.targetTexture = urt; ui.nearClipPlane = .01f; ui.farClipPlane = 10;
  var canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c => c.isRootCanvas && c.renderMode == RenderMode.ScreenSpaceOverlay && c.enabled).ToList(); var layers = new Dictionary<Transform, int>();
  foreach (var c in canvases) { foreach (var t in c.GetComponentsInChildren<Transform>(true)) { layers[t] = t.gameObject.layer; t.gameObject.layer = 31; } c.renderMode = RenderMode.ScreenSpaceCamera; c.worldCamera = ui; c.planeDistance = 1; }
  List<Camera> Cams() => Camera.allCameras.Where(c => c.targetTexture == null && c != ui).OrderBy(c => c.depth).ToList();
  CapNote($"{name}: composite of {Cams().Count} cameras ({string.Join(", ", Cams().Select(c => c.name + " " + c.rect))}) and {canvases.Count} canvases ({string.Join(", ", canvases.Select(c => c.name))})");
  // each frame every on-screen camera is drawn into the clip's texture in its own viewport (the split-screen code owns the cameras, so they are
  // borrowed for one render and given back), then the UI over them
  int frame = 0;
  void Draw() { var old = RenderTexture.active; RenderTexture.active = rt; GL.Clear(true, true, Color.black); RenderTexture.active = old; foreach (var c in Cams()) { var mask = c.cullingMask; c.cullingMask &= ~(1 << 31); c.targetTexture = rt; c.Render(); c.targetTexture = null; c.cullingMask = mask; } ui.Render(); Graphics.Blit(urt, rt, over); }
  yield return CapRecord(name, seconds, _ => { hook?.Invoke(frame / 60f); frame++; Draw(); }, null, rt);
  foreach (var c in canvases) if (c) { c.renderMode = RenderMode.ScreenSpaceOverlay; c.worldCamera = null; } foreach (var kv in layers) if (kv.Key) kv.Key.gameObject.layer = kv.Value;
  Destroy(ui.gameObject); rt.Release(); Destroy(rt); urt.Release(); Destroy(urt); Destroy(over);
 }

 // ---------------------------------------------------------------- the stunt park: a run along a line on a pad, a trick in the air (0.106's feature drives)
 sealed class Line100 { public Vector3 from; public Vector3[] path; public float cap, start; }
 static Line100 Park100(string line) => line switch {
  "K4" => new Line100 { from = StuntPark.P(17, 46), path = new[] { StuntPark.P(105, 46) }, cap = 18, start = 16 },
  "K3" => new Line100 { from = StuntPark.P(90, 20), path = new[] { StuntPark.P(14, 20) }, cap = 16, start = 12 },
  "loop" => new Line100 { from = StuntPark.P(64, 138, 5), path = new[] { StuntPark.P(150, 138) }, cap = 18, start = 14 },
  "barrel" => new Line100 { from = StuntPark.P(14, 74), path = new[] { StuntPark.P(100, 72) }, cap = 20, start = 17 },
  _ => null };
 sealed class Stunt100 : MonoBehaviour {
  public ArcadeVehicle Car; public Gamepad Pad; public Line100 Line; public string Trick = ""; public bool Done; public float Air; int wp; bool air; float airStart, estimate; Vector3 turn, dir;
  static Vector3 Flat(Vector3 v) { v.y = 0; return v; }
  // driven directly (Simulate with the AI air input, as the rivals' tricks are), not through a pad: Trailer Mode owns the player's input
 void Start() { dir = Flat(Line.path[0] - Line.from).normalized; Car.enabled = false; }
 void OnDestroy() { if (Car) { Car.AiAir = false; Car.enabled = true; } }
  void FixedUpdate() {
   if (!Car || Done) return; var p = Car.Body.position;
   if (wp < Line.path.Length - 1 && Vector3.Distance(Flat(p), Flat(Line.path[wp])) < 6) wp++;
   var target = Line.path[wp]; if (Vector3.Dot(Flat(target - p), dir) < 0 && wp == Line.path.Length - 1) { Done = true; Car.AiAir = false; Car.enabled = true; return; }
   bool off = Car.GroundedWheels == 0; float tip = 0, roll = 0, spin = 0;
   if (off && !air) { air = true; airStart = Time.time; turn = Vector3.zero; float vy = Car.Body.linearVelocity.y, drop = Mathf.Max(0, p.y - (StuntPark.FloorY + 1.6f)); estimate = (vy + Mathf.Sqrt(Mathf.Max(0, vy * vy + 2 * 9.81f * drop))) / 9.81f; }
   if (!off && air && Car.GroundedWheels >= 2) { air = false; Air = Mathf.Max(Air, Time.time - airStart); }
   if (air) {
    turn += Car.transform.InverseTransformDirection(Car.Body.angularVelocity) * (Time.fixedDeltaTime * Mathf.Rad2Deg);
    float left = Mathf.Max(.12f, estimate * .82f - (Time.time - airStart)); float rate = ArcadeVehicle.AirTipRate * Mathf.Rad2Deg; if (estimate < 1.1f) rate = float.MaxValue;
    float Pace(float done, float goal) => Mathf.Clamp((goal - done) / left / rate, -1, 1);
    if (Trick == "back") tip = -Pace(-turn.x, 360); if (Trick == "roll") roll = Pace(-turn.z, 360); if (Trick == "spin") spin = Pace(turn.y, 360);
   }
   float ang = Vector3.SignedAngle(Flat(Car.transform.forward), Flat(target - p), Vector3.up); float speed = Vector3.Dot(Car.Body.linearVelocity, Car.transform.forward); float steer = off ? roll : Mathf.Clamp(ang / 25, -1, 1);
   Car.AiAir = off && Trick != ""; Car.AiAirInput = new Vector3(tip, spin, roll);
   Car.Simulate(speed < Line.cap ? 1 : 0, speed > Line.cap + 1.5f ? Mathf.Clamp01((speed - Line.cap) / 5) : 0, off ? 0 : steer, Time.fixedDeltaTime);
  }
 }
 Stunt100 StuntRun100(ArcadeVehicle car, Gamepad pad, string line, string trick) {
  var l = Park100(line); var d = l.path[0] - l.from; d.y = 0; car.GetComponent<VehicleInput>().enabled = false;
  var at = l.from + Vector3.up * .7f; var rot = Quaternion.LookRotation(d); car.Body.position = at; car.Body.rotation = rot; car.transform.SetPositionAndRotation(at, rot); car.Body.linearVelocity = rot * Vector3.forward * l.start; car.Body.angularVelocity = Vector3.zero; car.ClearSteering(); car.GetComponent<VehicleRespawn>()?.CancelRecovery(); Physics.SyncTransforms();
  var old = car.GetComponent<Stunt100>(); if (old) Destroy(old); var s = car.gameObject.AddComponent<Stunt100>(); s.Car = car; s.Pad = pad; s.Line = l; s.Trick = trick; return s;
 }
 IEnumerator Park100Shot(string name, string vehicle, string line, string trick, CameraViews.Shot cam, Vector3? plant, float seconds, float hour = 12f) {
  yield return Roam100(vehicle, hour, Weather.Clear); Pads090(); Pad091(); var car = race.vehicle;
  var run = StuntRun100(car, pad1, line, trick); FindAnyObjectByType<ChaseCamera>()?.Snap(); yield return new WaitForSecondsRealtime(.2f);
  if (plant.HasValue) CapPlant(plant.Value); else CameraViews.Current.SelectTrailerCamera(cam);
  yield return CapRecord(name, seconds, SlowAir100(() => new[] { car }, .35f), () => run.Done && car.GroundedWheels >= 2 && Time.timeScale == 1);
  CapNote($"{name}: longest flight {run.Air:F2} s (game time), done {run.Done}, trick {trick}");
  Time.timeScale = 1; Time.fixedDeltaTime = .02f; Destroy(run); flow.QuitRace(); yield return null;
 }

 // ---------------------------------------------------------------- the shots ("take" for the shots with a jump)
 IEnumerator Shot100(string id, int take) {
  string T(string n) => take > 1 ? $"{n}-take{take}" : n; DriverVariation.Seed = 1000 + take * 77;
  var woods = new Vector3(300, 40, 400);
  switch (id) {
   case "01": yield return Look0(); yield return CapRoadShot("01-dawn-road", "moto", 6.1f, Weather.Clear, CameraViews.Shot.Fixed, 9, woods, 1.05f, plantAhead: 75, plantSide: 6, plantUp: 1.3f, warm: .5f); break;
   case "02": yield return Rider100(); break;
   case "03": yield return Garage100(); break;
   case "04": yield return Look0(); yield return CapRaceStart(); break;
   case "05": yield return Hwy100(); break;
   case "06": yield return RiderClose100(take); break;
   case "07": yield return Look0(); yield return CapPathShot("07-fence-smash", "atv", 12f, Weather.Clear, CameraViews.Shot.Side, 6, new List<Vector3> { Ground(new Vector3(488, 0, -15.3f)), Ground(new Vector3(461.5f, 0, -15.3f)), Ground(new Vector3(436, 0, -15.3f)) }, 24, startSpeed: 16); break;
   case "08": { yield return Look0(); var path = new List<Vector3> { new(476f, 0, -12f), new(480f, 0, -18f), new(492f, 0, -27f), new(504f, 0, -29f) }; for (int i = 0; i < path.Count; i++) path[i] = Ground(path[i]); yield return CapPathShot("08-mower-lawn", "mower", 12f, Weather.Clear, CameraViews.Shot.Side, 5, path, 14, startSpeed: 9); break; }
   case "09": yield return Summit100(T("09-leviathan-summit"), take); break;
   case "10": yield return Cabin100(T("10-cabin-jump"), take); break;
   case "11": { yield return Look0(); var p0 = new Vector3(891.5f, 150.4f, 152.6f); var f = Quaternion.Euler(0, 262, 0) * Vector3.forward; yield return CapLightning("11-lightning", new List<Vector3> { Ground(p0), Ground(p0 + f * 40), Ground(p0 + f * 90) }, p0 + Vector3.Cross(Vector3.up, f) * -9 + f * 22); break; }
   case "12": yield return StuntRace100(T("12-stunt-race-tricks"), take); break;
   case "13a": { yield return Look0(); var lane = new[] { new Vector3(520.5f, 0, -173.1f), new Vector3(534.1f, 0, -207.1f) }; var d = (lane[1] - lane[0]).normalized; var side = Vector3.Cross(Vector3.up, d); yield return CapPathShot("13a-sledders", "moto", 12f, Weather.Snow, CameraViews.Shot.Fixed, 6, new List<Vector3> { Ground(lane[0] - d * 40 + side * 9), Ground(lane[1] + d * 40 + side * 9) }, 14, startSpeed: 10, plant: Ground(lane[0] + d * 22 + side * 16, 1.6f)); break; }
   case "13b": yield return Look0(); yield return CapHockey(); break;
   case "14": yield return HiddenCop100(); break;
   case "15": yield return Look0(); yield return CapPolice(); break;
   case "S1": yield return SplitRace100(); break;
   case "S2": yield return SplitPolice100(); break;
   case "S3": yield return SplitStunt100(); break;
   case "16": yield return Reveal100(); break;
   case "17": yield return Park100Shot(T("17-buggy-backflip"), "stuntbuggy", "K4", "back", CameraViews.Shot.Fixed, take == 2 ? StuntPark.P(60, 60, 2f) : take == 3 ? StuntPark.P(84, 34, 4f) : StuntPark.P(70, 68, 3f), 9); break;
   case "18a": yield return Park100Shot(T("18a-bike-loop"), "stuntbike", "loop", "", take == 2 ? CameraViews.Shot.Chase : take == 3 ? CameraViews.Shot.Orbit : CameraViews.Shot.Side, null, 7, 18.2f); break;
   case "18b": yield return Park100Shot(T("18b-bike-barrel-roll"), "stuntbike", "barrel", "roll", take == 2 ? CameraViews.Shot.Side : take == 3 ? CameraViews.Shot.Chase : CameraViews.Shot.Orbit, null, 7, 18.2f); break;
   case "19": { yield return Look0(); var site = new Vector3(980.4f, 177.9f, 147.3f); var f = new Vector3(-1, 0, -.3f).normalized; var from = new Vector3(1187.2f, 152f, 206f); yield return CapPathShot(T("19-giant-jump"), "atv", 19.0f, Weather.Clear, take == 2 ? CameraViews.Shot.Chase : take == 3 ? CameraViews.Shot.Front : CameraViews.Shot.Side, 12, new List<Vector3> { Ground(from), Ground(site - f * 60), Ground(site), Ground(site + f * 100) }, 32, startSpeed: 20, moon: .5f, look: 16, slowFlight: true); break; }
   case "20": yield return Cover100(T("20-cover"), take); break;
   default: CapNote($"no shot {id}"); break;
  }
  Time.timeScale = 1; Time.fixedDeltaTime = .02f;
 }

 // 02: the rider page, a row changed every 0.8 s (0.97 shot 2, composite)
 IEnumerator Rider100() {
  yield return Look0(); flow.OpenGarage(); yield return new WaitForSecondsRealtime(1.5f); Pad091(); Pads090(); pad91 = pad1; var menus = FindAnyObjectByType<RaceMenus>(); flow.SelectVehicle("moto");
  typeof(RaceMenus).GetMethod("Navigate", Any100).Invoke(menus, new object[] { "rider" }); yield return new WaitForSecondsRealtime(.8f);
  var buttons = (List<UnityEngine.UI.Button>)typeof(RaceMenus).GetField("buttons", Any100).GetValue(menus);
  var rows = buttons.Where(b => b && b.gameObject.activeInHierarchy && b.interactable).ToList(); CapNote($"rider page: {rows.Count} rows: {string.Join(", ", rows.Select(r => r.name))}");
  // hair, hair color, hat, shirt, shirt color: each row stepped right on the pad, one every 0.8 s
  var pick = new[] { "rider-2", "rider-3", "rider-4", "rider-6", "rider-7" };
  float each = .8f; int last = -1; string before = JsonUtility.ToJson(flow.Save.Settings.rider);
  yield return Composite100("02-rider-looks", each * pick.Length + .5f, t => { PadRelease(); int i = Mathf.Min(pick.Length - 1, (int)(t / each)); if (i != last) { last = i; var row = buttons.FirstOrDefault(b => b && b.name == pick[i] && b.gameObject.activeInHierarchy); if (row) { UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(row.gameObject); PadTap(GamepadButton.DpadRight); } } menus.PreviewYaw = 215 + t * 40; }, 1920, 1080);
  CapNote($"02: rider before {before}, after {JsonUtility.ToJson(flow.Save.Settings.rider)}");
  menus.BackPage(); flow.CloseGarage(); Look100(); yield return null; // the look back as it was (the tapped rows changed it)
 }
 // 03: the vehicles turning one after another, ending on the stunt vehicles and the patrol car (composite)
 IEnumerator Garage100() {
  yield return Look0(); flow.OpenGarage(); yield return new WaitForSecondsRealtime(1.5f); Pad091(); Pads090(); pad91 = pad1; var menus = FindAnyObjectByType<RaceMenus>();
  int count = VehicleProfile.All.Length; float each = .36f; int last = 0; var seen = new List<string>();
  yield return Composite100("03-garage-vehicles", each * count + .6f, t => { PadRelease(); int i = Mathf.Min(count - 1, (int)(t / each)); if (i != last) { last = i; PadTap(GamepadButton.DpadRight); seen.Add(flow.PlayerVehicleId); } menus.PreviewYaw = 200 + t * 150; }, 1920, 1080);
  CapNote($"03: vehicles shown {string.Join(", ", seen)}");
  flow.CloseGarage(); Look100(); yield return null;
 }
 // 05: Hwy 92 at speed, the stores and lots going by (the through road, the right-hand lane)
 IEnumerator Hwy100() {
  yield return Roam100("moto", 12f, Weather.Clear); var road = race.throughRoad; road.Initialize();
  float s0 = road.Project(new Vector3(-260, 8, 530), out _), s1 = road.Project(new Vector3(420, 12, 545), out _); var path = RoadPath(road, Mathf.Min(s0, s1), Mathf.Max(s0, s1), 8, s1 > s0 ? 2.2f : -2.2f); if (s1 < s0) path.Reverse();
  var car = race.vehicle; car.GetComponent<VehicleInput>().enabled = false; var d = path[1] - path[0]; d.y = 0; CapPut(path[0], Quaternion.LookRotation(d).eulerAngles.y, 28);
  var pilot = car.gameObject.AddComponent<Pilot097>(); pilot.Car = car; pilot.Path = path; pilot.Speed = 36; pilot.Look = 18;
  yield return new WaitForSecondsRealtime(1.5f); CameraViews.Current.SelectTrailerCamera(CameraViews.Shot.Chase); Distance100(CameraViews.Shot.Chase, .85f, -.6f); yield return new WaitForSecondsRealtime(1f);
  CapNote($"05: highway '{road.name}' s {s0:F0} to {s1:F0}"); yield return CapRecord("05-hwy92", 8);
  Distance100(CameraViews.Shot.Chase, 1); Destroy(pilot); flow.QuitRace(); yield return null;
 }
 // 06 (added 1.0): the rider close: Dan's look (the mohawk, the hands on the bars) from a close side camera on the road loop
 IEnumerator RiderClose100(int take) {
  yield return Roam100("moto", 17.5f, Weather.Clear); var car = race.vehicle; var pilot = car.gameObject.AddComponent<RoadDriver>(); pilot.Initialize(race, car, false, -1, 1.0f); car.GetComponent<VehicleInput>().enabled = false;
  var road = pilot.DriveRoad; float s = road.Project(take == 2 ? new Vector3(300, 40, 400) : new Vector3(476, 87, -45), out _); pilot.Place(s, road.TrafficLane(s, -1));
  yield return new WaitForSecondsRealtime(2f); CameraViews.Current.SelectTrailerCamera(CameraViews.Shot.Side); Distance100(CameraViews.Shot.Side, .45f, -.5f); yield return new WaitForSecondsRealtime(1.5f);
  yield return CapRecord(take > 1 ? $"06-rider-close-take{take}" : "06-rider-close", 6);
  Distance100(CameraViews.Shot.Side, 1); Destroy(pilot); flow.QuitRace(); yield return null;
 }
 // 09 (added 1.0): Leviathan Forward, the summit flight with the field: side tracking, slow motion in the air
 IEnumerator Summit100(string name, int take) {
  yield return Race100(8, "moto", new[] { "moto", "atv", "scrambler", "drifter", "fastback" }, 2, 17.8f, Weather.Clear);
  var flight = race.GetComponent<MountainFlights>().flights.First(f => f.name.Contains("Summit")); var me = race.Racers[0];
  yield return Skip100(() => Rel100(me.RoadPosition, flight.approachStation) > -120 && Rel100(me.RoadPosition, flight.approachStation) < 200);
  CapNote($"{name}: at road {me.RoadPosition:F0} (flight from {flight.approachStation:F0}), field {string.Join(", ", race.Racers.Select(r => r.Name + " " + r.RoadPosition.ToString("F0")))}");
  CameraViews.Current.SelectTrailerCamera(take == 2 ? CameraViews.Shot.Chase : CameraViews.Shot.Side); yield return new WaitForSecondsRealtime(.5f);
  var car = race.vehicle;
  yield return CapRecord(name, 16, SlowAir100(() => new[] { car }, .35f), () => Time.timeScale == 1 && Rel100(me.RoadPosition, flight.endStation) > 30);
  EndRace100(); yield return null;
 }
 // 10: the Cabin Jump in Dan's Backyard (the racing AI sent down it), dusk, side tracking, slow motion from take-off
 IEnumerator Cabin100(string name, int take) {
  yield return Race100(6, take == 3 ? "atv" : "moto", new string[0], 1, 18.6f, Weather.Clear, drive: false);
  var line = FindObjectsByType<ShortcutUndergrowth>(FindObjectsSortMode.None).Select(u => u.clearRoute).FirstOrDefault(r => r && r.title == "Abandoned Cabin Jump");
  if (!line) { CapNote($"{name}: no Cabin Jump route here"); EndRace100(); yield break; }
  line.Initialize(); var car = race.vehicle; car.GetComponent<VehicleInput>().enabled = false; var pilot = car.gameObject.AddComponent<RoadDriver>(); pilot.Initialize(race, car, true, 1, 1); pilot.Racer = race.Racers[0];
  typeof(RoadDriver).GetField("plannedBranch", Any100).SetValue(pilot, line);
  var start = line.At(0, out _);
  yield return Skip100(() => Vector3.Distance(car.Body.position, start) < 90);
  CameraViews.Current.SelectTrailerCamera(take == 2 ? CameraViews.Shot.Chase : CameraViews.Shot.Side); yield return new WaitForSecondsRealtime(.3f);
  float flew = 0; var slow = SlowAir100(() => new[] { car }, .35f);
  yield return CapRecord(name, 12, t => { slow(t); if (car.GroundedWheels == 0) flew += Time.deltaTime; }, () => Time.timeScale == 1 && line.Project(car.Body.position, out _) > line.Length - 10);
  CapNote($"{name}: air {flew:F2} s game time, at branch s {line.Project(car.Body.position, out _):F0} of {line.Length:F0}");
  EndRace100(); yield return null;
 }
 // 14: a hidden cop in a Hwy 92 lot clocks the rider going by and lights up; the chase begins. No Trailer Mode (it keeps the cops asleep);
 // the clip is the camera's picture, so no HUD.
 IEnumerator HiddenCop100() {
  yield return Roam100("moto", 18.4f, Weather.Clear, trailer: false); var hp = HiddenPolice.Current;
  if (!hp) { CapNote("14: no hidden police in this Free Roam"); flow.QuitRace(); yield break; }
  var spot = hp.All.OrderBy(s => Vector3.Distance(s.pos, new Vector3(85.1f, 8.6f, 523.2f))).First(); if (!hp.Active.Contains(spot)) hp.Active.Add(spot); hp.AttachedAt = Time.time - 1000; hp.LastEnd = -9999;
  var road = race.throughRoad; road.Initialize(); float sc = road.Project(spot.pos, out _), s0 = road.Project(spot.pos + Vector3.left * 300, out _); int sign = s0 < sc ? 1 : -1;
  var path = RoadPath(road, Mathf.Min(s0, sc + sign * 250), Mathf.Max(s0, sc + sign * 250), 8, 2.2f * sign); if (sign < 0) path.Reverse();
  var car = race.vehicle; car.GetComponent<VehicleInput>().enabled = false; var d = path[1] - path[0]; d.y = 0; CapPut(path[0], Quaternion.LookRotation(d).eulerAngles.y, 30);
  var pilot = car.gameObject.AddComponent<Pilot097>(); pilot.Car = car; pilot.Path = path; pilot.Speed = 40; pilot.Look = 18;
  FindAnyObjectByType<ChaseCamera>()?.Snap(); yield return new WaitForSecondsRealtime(1.5f); bool tripped = false; float at = -1;
  yield return CapRecord("14-hidden-cop", 13, t => { if (!tripped && hp && hp.ChaseRunning) { tripped = true; at = t; } });
  CapNote($"14: spot {V(spot.pos)}, chase started {tripped} at {at:F1} s; radio line '{HiddenPolice.Line}'");
  Destroy(pilot); flow.Pause(); flow.QuitRace(); yield return null;
 }
 // S1-S3: split screen, both views and the HUD (composite)
 IEnumerator Split100(SplitScreen.Kind mode, Action setup) {
  yield return Look0(); yield return DanCopy095(); Look100(); Pads090();
  SplitScreen.Solo = false; SplitScreen.P1Device = pad1; SplitScreen.P2Device = null; SplitScreen.P2Ai = true; SplitScreen.Mode = mode; SplitScreen.Time = TimeOfDay.Day; SplitScreen.Weather = Weather.Clear; SplitScreen.Traffic = false; SplitScreen.P1Color = 1; SplitScreen.P2Color = 4;
  setup();
  var st = flow.Save.Settings; st.master = 1; st.radioOn = false; st.hints = false; st.splitLeftRight = true; flow.Save.SaveSettings();
  flow.StartSplit(); float t0 = Time.realtimeSinceStartup; while (!(SplitScreen.Active && flow.State == RaceFlow.Stage.Racing) && Time.realtimeSinceStartup - t0 < 120) { yield return null; Bind(); }
  Bind(); AudioListener.volume = 1; CapNote($"split {mode}: active {SplitScreen.Active}, state {flow.State}, views {SplitScreen.Views.Count}, scene {Scene}");
 }
 IEnumerator SplitRace100() {
  yield return Split100(SplitScreen.Kind.Race, () => { SplitScreen.Course = 0; SplitScreen.Laps = 1; SplitScreen.Rivals = 2; SplitScreen.P1Vehicle = "moto"; SplitScreen.P2Vehicle = "atv"; SplitScreen.StuntRace = false; });
  var rd = flow.Race; var car = rd.vehicle; var pilot = car.gameObject.AddComponent<RoadDriver>(); pilot.Initialize(rd, car, true, 1, 1); pilot.Racer = rd.Racers[0]; car.GetComponent<VehicleInput>().enabled = false;
  yield return Composite100("S1-split-race", 7);
  Destroy(pilot); flow.QuitSplit(false); yield return null;
 }
 // S2: Cop vs Runner on the streets at dusk: player 1 in the patrol car (lights on) after the AI runner, player 2's view running
 IEnumerator SplitPolice100() {
  yield return Split100(SplitScreen.Kind.Police, () => { SplitScreen.PoliceGame = SplitScreen.Game.CopRunner; SplitScreen.P2AiRunner = true; SplitScreen.CopFirst = 1; SplitScreen.Time = TimeOfDay.Dusk; SplitScreen.P1Vehicle = "moto"; SplitScreen.P2Vehicle = "atv"; SplitScreen.CopVehicle = "police"; });
  float w0 = Time.realtimeSinceStartup; while (Time.realtimeSinceStartup - w0 < 25 && GetawayChase.Current && GetawayChase.Current.State == GetawayChase.Phase.Starting) yield return null;
  var g = GetawayChase.Current; var rd = flow.Race; var car = rd.vehicle; car.GetComponent<VehicleInput>().enabled = false;
  var runner = g ? g.Runners.Select(r => r.car).FirstOrDefault(c => c && c != car) : null;
  if (runner) { var chaser = car.gameObject.AddComponent<Pilot097>(); chaser.Car = car; chaser.Speed = 30; chaser.Look = 12; chaser.Path = new List<Vector3> { car.Body.position, runner.Body.position }; StartCoroutine(Chase100(chaser, runner)); }
  else { var pilot = car.gameObject.AddComponent<RoadDriver>(); pilot.Initialize(rd, car, false, 1, 1.2f); }
  yield return new WaitForSecondsRealtime(3);
  CapNote($"S2: chase {(g ? g.State.ToString() : "none")}, runners {(g ? g.Runners.Count : 0)}, chasing {(runner ? runner.name : "nobody (the road AI drives)")}, cop {car.GetComponent<VehicleConfiguration>().profileId}");
  yield return Composite100("S2-split-police", 8, t => { if (g) foreach (var r in g.Runners) r.bust = 0; });
  flow.QuitSplit(false); yield return null;
 }
 IEnumerator Chase100(Pilot097 p, ArcadeVehicle target) { while (p && target) { var v = target.Body.linearVelocity; p.Path = new List<Vector3> { p.Car.Body.position, target.Body.position + v * .5f, target.Body.position + v * 1.5f + (v.sqrMagnitude > 1 ? v.normalized * 10 : Vector3.zero) }; yield return new WaitForSeconds(.25f); } }
 // S3: the split-screen Stunt event: both riders in the air at once (each on their own pad, two kicker lines)
 IEnumerator SplitStunt100() {
  yield return Split100(SplitScreen.Kind.Stunt, () => { SplitScreen.P2Ai = false; SplitScreen.P2Device = pad2; SplitScreen.P1Vehicle = "stuntbuggy"; SplitScreen.P2Vehicle = "stuntbike"; SplitScreen.FromRoam = false; });
  float w0 = Time.realtimeSinceStartup; while (Time.realtimeSinceStartup - w0 < 60 && !(StuntEvent.Current && StuntEvent.Current.State == StuntEvent.Phase.Running)) yield return null;
  var rd = flow.Race; var c1 = rd.vehicle; var c2 = SplitScreen.Race ? SplitScreen.Race.P2Car : null; yield return new WaitForSecondsRealtime(1);
  CapNote($"S3: event {(StuntEvent.Current ? StuntEvent.Current.State.ToString() : "none")}, p1 {(c1 ? c1.name : "-")}, p2 {(c2 ? c2.name : "-")}");
  StuntRun100(c1, pad1, "K4", "back"); if (c2) StuntRun100(c2, pad2, "K3", "spin");
  yield return Composite100("S3-split-stunt", 8, SlowAir100(() => new[] { c1, c2 }, .5f));
  Time.timeScale = 1; Time.fixedDeltaTime = .02f; flow.QuitSplit(false); yield return null;
 }
 // 16: the stunt park from the Hwy 92 entrance: riding in, flyover
 IEnumerator Reveal100() {
  yield return Roam100("stuntbuggy", 11f, Weather.Clear); var car = race.vehicle; car.GetComponent<VehicleInput>().enabled = false;
  var path = new List<Vector3> { Ground(StuntPark.Entrance + new Vector3(0, 0, -40)), Ground(StuntPark.Entrance), Ground(StuntPark.P(110, 30)), Ground(StuntPark.P(80, 60)) };
  CapPut(path[0], 0, 8); var pilot = car.gameObject.AddComponent<Pilot097>(); pilot.Car = car; pilot.Path = path; pilot.Speed = 12;
  yield return new WaitForSecondsRealtime(.5f); CameraViews.Current.SelectTrailerCamera(CameraViews.Shot.Flyover); yield return new WaitForSecondsRealtime(.5f);
  yield return CapRecord("16-park-reveal", 7); Destroy(pilot); flow.QuitRace(); yield return null;
 }
 // 12 (added 1.0): a Stunt Race on Mountain Loop Forward, rivals over the Eastbound Gully Flight turning BACKFLIP + ROLL (Hard; for this clip every
 // entry of the rivals' trick list is that trick, in memory only), a fixed camera beside the landing, half-speed slow motion
 IEnumerator StuntRace100(string name, int take) {
  var menu = (Array)typeof(RivalTricks).GetField("Menu", Any100).GetValue(null); var keep = menu.Cast<object>().ToArray();
  var only = keep.First(m => (string)m.GetType().GetField("name", Any100).GetValue(m) == "BACKFLIP + ROLL");
  yield return Race100(4, "stuntbuggy", new[] { "stuntbuggy", "stuntbike", "stuntbuggy", "stuntbike", "stuntbuggy" }, 2, 13f, Weather.Clear, stunts: true);
  for (int i = 0; i < menu.Length; i++) menu.SetValue(only, i);
  var flight = race.GetComponent<MountainFlights>().flights.First(f => f.name.Contains("Gully")); var ai = race.Racers.Where(r => r.IsAi).ToList();
  // until a rival is about 110 m from the lip on its way in (the run-up winds, so distance, not stations)
  float ToLip(RacerState r) { var d = r.Car.Body.position - flight.lip; d.y = 0; return d.magnitude; }
  bool Coming(RacerState r) => ToLip(r) < 110 && Vector3.Dot(r.Car.Body.position - flight.lip, flight.forward) < 0;
  yield return Skip100(() => ai.Any(Coming), 240);
  // the camera rides with the leading rival still before the lip (Trailer Mode's cameras view CameraViews.Target): side, chase or orbit by take
  var lead = ai.Where(Coming).OrderBy(ToLip).FirstOrDefault() ?? ai[0]; CameraViews.Current.Target = lead.Car;
  CameraViews.Current.SelectTrailerCamera(take == 2 ? CameraViews.Shot.Chase : take == 3 ? CameraViews.Shot.Orbit : CameraViews.Shot.Side); var plant = lead.Car.Body.position;
  CapNote($"{name}: camera {V(plant)}; field {string.Join(", ", race.Racers.Select(r => r.Name + " " + Rel100(r.RoadPosition, flight.approachStation).ToString("F0")))}");
  yield return CapRecord(name, 14, t => { Time.timeScale = .5f; Time.fixedDeltaTime = .01f; });
  Time.timeScale = 1; Time.fixedDeltaTime = .02f;
  CapNote($"{name}: rivals' flights: {string.Join(" | ", ai.Select(r => r.Car.GetComponent<RivalTricks>()).Where(x => x).Select(x => x.Log.Replace("\n", "; ")))}");
  CameraViews.Current.Target = null; for (int i = 0; i < menu.Length; i++) menu.SetValue(keep[i], i); EndRace100(); yield return null;
 }
 // 20: the cover shot: a Street Loop race over the crest south of Dan's house (road s about 767), the blue car and an ATV field, a camera low past
 // the crest looking back up it, golden light, slow motion through the crest
 IEnumerator Cover100(string name, int take) {
  yield return Race100(0, "original", new[] { "atv", "atv", "atv" }, take == 3 ? 2 : 1, 18.9f, Weather.Clear);
  var crest = new Vector3(515.7f, 81.3f, -137.6f); float sc = race.road.Project(crest, out _); var me = race.Racers[0];
  yield return Skip100(() => Rel100(me.RoadPosition, sc) > -140 && Rel100(me.RoadPosition, sc) < 0);
  var at = race.road.At(sc + 10 + take * 3, out var f); var plant = Ground(at + Vector3.Cross(Vector3.up, f).normalized * (take == 2 ? -3.5f : 3.5f), 1.3f); CapPlant(plant);
  CapNote($"{name}: crest s {sc:F0}, camera {V(plant)}, field {string.Join(", ", race.Racers.Select(r => r.Name + " " + Rel100(r.RoadPosition, sc).ToString("F0")))}");
  var cars = race.Racers.Select(r => r.Car).ToArray();
  yield return CapRecord(name, 11, t => { bool near = cars.Any(c => c && Vector3.Distance(c.Body.position, crest) < 45); Time.timeScale = near ? .35f : 1; Time.fixedDeltaTime = .02f * Time.timeScale; }, () => Time.timeScale == 1 && Rel100(me.RoadPosition, sc) > 60);
  EndRace100(); yield return null;
 }
}
}
#endif
