using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Racer
{
    // 0.101 evidence only (like Report100PlayerCheck): `-p101check <dir> -racerTestSave <dir>` in the built player. Never active without both arguments.
    //   -p101kind flights   Dan's Backyard Forward: Needle 600, Street Classic, Trail Four, Turf Rocket at 24/28/31/34 m/s up the Cabin run-up (speed held, centre line)
    //   -p101kind attempts  -p101scene race|roam: from the main road, driven with a (virtual) controller like a player: three normal attempts with
    //                       different lines and one flat out (-p101vehicle)
    //   -p101kind ai        race: the race AI sent down the Cabin Jump (Street Classic, Needle 600)
    //   -p101kind reset     race and Free Roam: a reset from inside the brush
    //   -p101kind views     -p101scene race|roam -p101views "name,x,z,heading;..." the vehicle placed there, the chase camera's view
    //   -p101kind lap       one lap of Dan's Backyard Forward by the road AI
    public sealed class Report101Hits : MonoBehaviour
    {
        public readonly List<string> hits = new();
        void OnCollisionEnter(Collision c) { if (c.impulse.magnitude > 300 && !c.collider.attachedRigidbody) hits.Add($"{c.collider.name} ({c.impulse.magnitude:F0} N s)"); }
    }
    public sealed class Report101PlayerCheck : MonoBehaviour
    {
        string outDir; readonly List<string> rows = new();
        const BindingFlags NonPublic = BindingFlags.NonPublic | BindingFlags.Instance;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            var a = Environment.GetCommandLineArgs(); int i = Array.IndexOf(a, "-p101check");
            if (i < 0 || i + 1 >= a.Length || Array.IndexOf(a, "-racerTestSave") < 0 || FindAnyObjectByType<Report101PlayerCheck>()) return;
            Application.runInBackground = true; // the check must not pause when its window loses focus (the game itself is unchanged)
            var g = new GameObject("Report101 player check"); DontDestroyOnLoad(g); g.AddComponent<Report101PlayerCheck>().outDir = a[i + 1];
        }
        static string Arg(string key, string fallback) { var a = Environment.GetCommandLineArgs(); int i = Array.IndexOf(a, key); return i >= 0 && i + 1 < a.Length ? a[i + 1] : fallback; }
        static float F(string s) => float.Parse(s, System.Globalization.CultureInfo.InvariantCulture);
        static string V(Vector3 v) => $"{v.x:F1},{v.y:F2},{v.z:F1}";
        void Note(string s) { rows.Add(s); File.WriteAllLines(Path.Combine(outDir, "p101-check.txt"), rows); Debug.Log("P101CHECK " + s); }
        IEnumerator Snap(string name) { yield return new WaitForEndOfFrame(); ScreenCapture.CaptureScreenshot(Path.Combine(outDir, name + ".png")); yield return null; yield return null; yield return new WaitForSecondsRealtime(.3f); }
        RaceFlow flow; RaceDirector race; Gamepad pad;
        void Bind() { flow = FindAnyObjectByType<RaceFlow>(); race = flow ? flow.Race : null; }
        IEnumerator WaitFlow(float limit = 120)
        {
            float t0 = Time.realtimeSinceStartup;
            while (true) { Bind(); if (flow && flow.Started && !LoadingScreen.Holding && flow.Save != null) break; yield return null; if (Time.realtimeSinceStartup - t0 > limit) break; }
            var title = FindAnyObjectByType<StartupTitle>(); if (title) { typeof(StartupTitle).GetField("completed", BindingFlags.Static | BindingFlags.NonPublic)?.SetValue(null, true); Destroy(title.gameObject); }
            if (flow.State == RaceFlow.Stage.Title) flow.EnterMenuAfterTitle();
            yield return null;
        }
        void Conditions() { var st = flow.Save.Settings; st.timeOfDay = 0; st.weather = 0; st.hints = false; st.vsync = true; st.estimateAiFinishes = false; flow.Save.SaveSettings(); flow.Save.ApplySettings(); }
        IEnumerator ToBackyard()
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "DansBackyardForward") yield break;
            Campaign.Testing = true; float t0 = Time.realtimeSinceStartup;
            for (int attempt = 0; attempt < 4 && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "DansBackyardForward"; attempt++)
            {
                Note($"  to Dan's Backyard Forward (try {attempt + 1}): state {flow.State}, scene {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}, timeScale {Time.timeScale}");
                if (flow.State == RaceFlow.Stage.Title) flow.EnterMenuAfterTitle();
                flow.OpenCourses(); yield return null; flow.SelectBackyardForward(); yield return null;
                float t1 = Time.realtimeSinceStartup; while (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "DansBackyardForward" && Time.realtimeSinceStartup - t1 < 30) yield return null;
            }
            yield return WaitFlow();
        }
        IEnumerator StartRace(string vehicle)
        {
            yield return ToBackyard(); Conditions();
            flow.OpenGarage(); flow.SelectVehicle(vehicle); flow.CloseGarage();
            race.opponents = false; race.traffic = false; race.laps = 1; race.difficulty = 1;
            flow.StartRace(); float t0 = Time.realtimeSinceStartup;
            while (flow.State != RaceFlow.Stage.Racing && Time.realtimeSinceStartup - t0 < 60) { AudioListener.volume = 0; yield return null; }
            yield return new WaitForSecondsRealtime(4); // the countdown
        }
        IEnumerator ToFreeRoam(string vehicle)
        {
            flow.Save.Settings.vehicleId = vehicle; flow.Save.Settings.hints = false; flow.Save.SaveSettings(); Conditions();
            flow.StartFreeRoam(); float t0 = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - t0 < 120 && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != RaceFlow.RoamScene) yield return null;
            Bind(); t0 = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - t0 < 120) { Bind(); if (flow && race && flow.Started && !LoadingScreen.Holding && flow.State == RaceFlow.Stage.Racing) break; yield return null; }
            AudioListener.volume = 0; yield return new WaitForSecondsRealtime(1f);
        }
        static float GroundAt(Vector3 p) { float best = float.NaN; foreach (var h in Physics.RaycastAll(new Vector3(p.x, 400, p.z), Vector3.down, 800, ~0, QueryTriggerInteraction.Ignore)) { if (h.collider.attachedRigidbody) continue; if (float.IsNaN(best) || h.point.y > best) best = h.point.y; } return best; }
        void Put(Vector3 pos, float yaw, float speed = 0, float lift = 1.0f, float pitch = 0)
        {
            var car = race.vehicle; pos.y = GroundAt(pos) + lift; var rot = Quaternion.Euler(-pitch, yaw, 0);
            car.Body.position = pos; car.Body.rotation = rot; car.transform.SetPositionAndRotation(pos, rot); car.Body.linearVelocity = rot * Vector3.forward * speed; car.Body.angularVelocity = Vector3.zero; car.ClearSteering();
            car.GetComponent<VehicleRespawn>()?.CancelRecovery(); FindAnyObjectByType<ChaseCamera>()?.Snap();
        }
        // the Cabin line: the race's branch, or Free Roam's brush reset line
        WoodlandRoute Line() { foreach (var u in FindObjectsByType<ShortcutUndergrowth>(FindObjectsSortMode.None)) if (u.clearRoute && u.clearRoute.title == "Abandoned Cabin Jump") { u.clearRoute.Initialize(); return u.clearRoute; } return null; }
        ShortcutUndergrowth Brush() => FindObjectsByType<ShortcutUndergrowth>(FindObjectsSortMode.None).FirstOrDefault(u => u.clearRoute && u.clearRoute.title == "Abandoned Cabin Jump");
        RaceRoad Main() => FindObjectsByType<RaceRoad>(FindObjectsSortMode.None).First(r => r.name == "Forward navigation only - no road mesh");
        float lastBush, lipS;
        void Measure()
        {
            var u = Brush(); var line = u.clearRoute; lipS = u.brushFrom; var mf = u.GetComponent<MeshFilter>(); var M = u.transform.localToWorldMatrix; lastBush = 0;
            var verts = mf.sharedMesh.vertices; foreach (var i in new HashSet<int>(mf.sharedMesh.triangles)) { var w = M.MultiplyPoint3x4(verts[i]); float s = line.Project(w, out float lat); if (s >= lipS && lat < u.clearHalfWidth && s < u.clearFrom + 6) lastBush = Mathf.Max(lastBush, s); }
            Note($"{UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}: Cabin line {line.name}, length {line.Length:F1}, lip at s {lipS:F1}, brush far edge {u.clearFrom:F1}, the last bush reaches s {lastBush:F1}");
        }
        void PadState(float steer, float throttle, float brake)
        {
            float sx = Mathf.Abs(steer) < .01f ? 0 : Mathf.Sign(steer) * (.12f + Mathf.Min(1, Mathf.Abs(steer)) * .83f);
            float th = throttle < .01f ? 0 : .1f + Mathf.Min(1, throttle) * .9f, br = brake < .01f ? 0 : .1f + Mathf.Min(1, brake) * .9f;
            InputSystem.QueueStateEvent(pad, new GamepadState { leftStick = new Vector2(sx, 0), rightTrigger = th, leftTrigger = br });
        }
        // one run: speed held at `target` (0 = flat out); on the main road until `mainUntil` (main station; -1 = on the line from the start), then the Cabin line
        // (aimed `offset` m to its left); records the take-off, the landing, any air before the ramp, and where it ends up
        IEnumerator Run(string tag, float target, float mainUntil, float offset, List<string> table, RaceRoad approach = null, float dir = 1)
        {
            var car = race.vehicle; var line = Line(); var main = approach ? approach : Main(); main.Initialize(); line.At(10, out var lf); var n = -Vector3.Cross(Vector3.up, lf).normalized; // left of the line
            bool onLine = mainUntil < 0, airborne = false, touched = false; float airStart = 0, airS = 0, airV = 0, t0 = Time.time, maxLatAtFoot = 0, minUp = 1, endAt = -1;
            var events = new List<(float s0, float s1, float dt, float v, float lat, Vector3 p)>(); var trace = new List<string>(); bool tracing = Array.IndexOf(Environment.GetCommandLineArgs(), "-p101trace") >= 0;
            var hitLog = car.GetComponent<Report101Hits>() ?? car.gameObject.AddComponent<Report101Hits>(); hitLog.hits.Clear();
            float footS = lipS - 19f; int rec0 = race.Racers.Count > 0 ? race.Racers[0].Recoveries : 0;
            while (Time.time - t0 < 30)
            {
                yield return new WaitForFixedUpdate(); AudioListener.volume = 0;
                var p = car.Body.position; float v = car.ForwardSpeed; float s = line.Project(p, out float lat);
                if (!onLine && (dir > 0 ? main.Project(p, out _) >= mainUntil : main.Project(p, out _) <= mainUntil)) onLine = true;
                Vector3 aim; if (!onLine) aim = main.At(main.Project(p, out _) + 12 * dir, out _); else aim = line.At(Mathf.Max(s + 10, 8), out _) + (s < lipS - 4 ? n * offset * Mathf.Clamp01((lipS - 4 - s) / 10f) : Vector3.zero);
                var local = car.transform.InverseTransformPoint(aim); float steer = Mathf.Clamp(Mathf.Atan2(local.x, local.z) * 2.2f, -1, 1); if (onLine && s > lipS - 3) steer = 0;
                float th = target <= 0 ? 1 : v < target - .4f ? 1 : v < target + .6f ? .35f : 0, br = target > 0 && v > target + 1.5f ? .3f : 0;
                if (endAt > 0) { th = 0; br = 1; steer = 0; }
                PadState(steer, th, br);
                if (tracing && onLine && s > 6 && s < 60) { float gy = GroundAt(p); trace.Add($"{Time.time - t0:F3} th {th:F2} br {br:F2} st {steer:F2} wiped {car.GetComponent<VehicleConfiguration>()?.WipedOut} s {s:F2} lat {lat:F2} y {p.y:F3} ground {gy:F3} h {p.y - gy:F3} wheels {car.GroundedWheels} vy {car.Body.linearVelocity.y:F2} v {v:F1} pitch {-car.transform.eulerAngles.x:F1} roll {car.transform.eulerAngles.z:F1} lift {car.SuspensionLift:F1} touching {string.Join("/", Physics.OverlapBox(p, new Vector3(1.2f, 1.0f, 2.0f), car.transform.rotation, ~0, QueryTriggerInteraction.Ignore).Where(x => !x.attachedRigidbody).Select(x => x.name).Distinct())}"); }
                bool ground = car.GroundedWheels > 0; if (!touched) { if (ground) touched = true; else continue; }
                if (onLine && Mathf.Abs(s - footS) < .6f) maxLatAtFoot = Mathf.Max(maxLatAtFoot, lat);
                if (!ground && !airborne) { airborne = true; airStart = Time.time; airS = s; airV = car.Body.linearVelocity.magnitude; }
                if (ground && airborne) { airborne = false; float dt = Time.time - airStart; if (dt > .1f) events.Add((airS, s, dt, airV, lat, p)); if (s > lipS + 5 && dt > .4f && endAt < 0) endAt = Time.time; }
                if (endAt > 0) minUp = Mathf.Min(minUp, car.transform.up.y);
                if (endAt > 0 && Time.time - endAt > 3f) break;
                if (endAt < 0 && onLine && s > lipS + 25 && ground) break;
            }
            PadState(0, 0, 1);
            int rec = race.Racers.Count > 0 ? race.Racers[0].Recoveries - rec0 : 0;
            var jump = events.Where(e => e.s1 > lipS + 5 && e.dt > .4f).OrderByDescending(e => e.dt).FirstOrDefault();
            string landOn = "-"; if (jump.dt > 0) foreach (var h in Physics.RaycastAll(jump.p + Vector3.up, Vector3.down, 5, ~0, QueryTriggerInteraction.Ignore).OrderBy(x => x.distance)) if (!h.collider.attachedRigidbody) { landOn = h.collider.name; break; }
            var early = events.Where(e => e.s0 < footS && e.dt > .12f).ToList();
            string clear = jump.dt <= 0 ? "NO JUMP" : jump.s1 >= lastBush + 3 ? $"clear by {jump.s1 - lastBush:F1} m" : jump.s1 >= lastBush ? $"past the last bush by only {jump.s1 - lastBush:F1} m" : $"IN the brush ({lastBush - jump.s1:F1} m short)";
            string row = $"{tag}: take-off s {jump.s0:F1} at {jump.v:F1} m/s, {jump.dt:F2} s in the air, landing s {jump.s1:F1} ({jump.lat:F1} m off the line) on {landOn}: {clear}; off the line at the ramp foot {maxLatAtFoot:F1} m; air before the ramp: {(early.Count == 0 ? "none" : string.Join(", ", early.Select(e => $"s {e.s0:F1}-{e.s1:F1} {e.dt:F2} s")))}; other hops {events.Count - early.Count - (jump.dt > 0 ? 1 : 0)}; lowest up after landing {minUp:F2}; resets {rec}; struck: {(hitLog.hits.Count == 0 ? "nothing" : string.Join(", ", hitLog.hits.Distinct()))}";
            Note(row); table?.Add(row); if (tracing) File.WriteAllLines(Path.Combine(outDir, "trace-" + string.Concat(tag.Where(char.IsLetterOrDigit)) + ".txt"), trace);
        }

        IEnumerator Start()
        {
            Directory.CreateDirectory(outDir); AudioListener.volume = 0;
            string kind = Arg("-p101kind", "flights");
            int w = int.Parse(Arg("-p101w", "1920")), h = int.Parse(Arg("-p101h", "1080")); Screen.SetResolution(w, h, FullScreenMode.Windowed); yield return null; yield return null;
            Note($"screen={Screen.width}x{Screen.height} kind={kind} version={Application.version}");
            pad = InputSystem.AddDevice<Gamepad>("Report101Pad");
            yield return WaitFlow();
            switch (kind)
            {
                case "flights": yield return Flights(); break;
                case "attempts": yield return Attempts(); break;
                case "ai": yield return Ai(); break;
                case "reset": yield return Resets(); break;
                case "views": yield return Views(); break;
                case "lap": yield return Lap(); break;
                default: Note("unknown kind " + kind); break;
            }
            File.WriteAllText(Path.Combine(outDir, "done.txt"), "done"); Application.Quit();
        }

        IEnumerator Flights()
        {
            yield return StartRace("original"); Measure(); var line = Line(); var table = new List<string>();
            string only = Arg("-p101only", ""); // "id:speed" = that one flight
            foreach (var id in new[] { "moto", "original", "atv", "mower" })
            {
                if (only.Length > 0 && !only.StartsWith(id + ":")) continue;
                race.vehicle.GetComponent<VehicleConfiguration>().Apply(id); yield return new WaitForFixedUpdate();
                foreach (var speed in new[] { 24f, 28f, 31f, 34f })
                {
                    if (only.Length > 0 && only != $"{id}:{speed}") continue;
                    var p = line.At(12, out var f); var ahead = line.At(14, out _); float g0 = GroundAt(p), g1 = GroundAt(ahead); float pitch = Mathf.Atan2(g1 - g0, Vector2.Distance(new Vector2(p.x, p.z), new Vector2(ahead.x, ahead.z))) * Mathf.Rad2Deg;
                    Put(p, Mathf.Atan2(f.x, f.z) * Mathf.Rad2Deg, speed, .42f, pitch); yield return new WaitForFixedUpdate();
                    yield return Run($"{VehicleProfile.Find(id).Name} at {speed} m/s", speed, -1, 0, table);
                }
            }
            Note("TABLE"); foreach (var r in table) Note("  " + r);
        }
        IEnumerator Attempts()
        {
            string vehicle = Arg("-p101vehicle", "mower"); bool roam = Arg("-p101scene", "race") == "roam";
            if (roam) yield return ToFreeRoam(vehicle); else yield return StartRace(vehicle);
            Measure(); var line = Line();
            // the race: from the main road (Forward); Free Roam: that stretch of the main is closed by a timber barricade there, so from the
            // straight Reverse trail that leads east to the cabin, driven eastwards
            var main = roam ? FindObjectsByType<RaceRoad>(FindObjectsSortMode.None).First(r => r.name.StartsWith("Reverse navigation only")) : Main(); main.Initialize();
            float dir = roam ? -1 : 1; var lineStart = line.At(roam ? 4 : 0, out _); float cross = main.Project(lineStart, out _); float back = roam ? 40 : 70;
            Note($"{(roam ? "Free Roam" : "race")}, {VehicleProfile.Find(vehicle).Name}: start on {(roam ? "the Reverse trail heading east" : "the main road")} {back:F0} m before the turn-off ({main.name} s {cross - back * dir:F0}) at 18 m/s, controller input; onto the run-up from 8 m before it");
            int k = 0;
            foreach (var (speed, offset) in new[] { (0f, 0f), (0f, 1.0f), (0f, -1.0f), (26f, 0f) })
            {
                var p = main.At(cross - back * dir, out var f); f *= dir; Put(p, Mathf.Atan2(f.x, f.z) * Mathf.Rad2Deg, 18); yield return new WaitForFixedUpdate();
                yield return Run($"{(speed > 0 ? $"eased off (held near {speed} m/s, on the line)" : $"attempt {++k} (full throttle, aiming {offset:+0.0;-0.0;0} m off the line)")}", speed, cross - (roam ? 2 : 8) * dir, offset, null, main, dir);
                yield return new WaitForSecondsRealtime(.5f);
            }
        }
        IEnumerator Ai()
        {
            // a real race from the grid (the gates in order, so the branch entry counts), the race AI driving the player's vehicle and sent down the Cabin Jump
            foreach (var id in new[] { "original", "moto", "atv" })
            {
                if (id == "original") yield return StartRace(id);
                else { race.vehicle.GetComponent<VehicleConfiguration>().Apply(id); flow.StartRace(); float t1 = Time.realtimeSinceStartup; yield return null; while ((flow.State != RaceFlow.Stage.Racing || LoadingScreen.Holding) && Time.realtimeSinceStartup - t1 < 60) yield return null; yield return new WaitForSecondsRealtime(4); }
                Measure(); var line = Line(); var main = Main(); main.Initialize();
                var car = race.vehicle; car.GetComponent<VehicleInput>().enabled = false;
                var pilot = car.gameObject.AddComponent<RoadDriver>(); pilot.Initialize(race, car, true, 1, 1); pilot.Racer = race.Racers[0];
                typeof(RoadDriver).GetField("plannedBranch", NonPublic).SetValue(pilot, line);
                Note($"  start: state {flow.State}, timeScale {Time.timeScale}, holding {LoadingScreen.Holding}"); float real0 = Time.realtimeSinceStartup;
                Time.timeScale = 2; float t0 = Time.time, best = 0, takeoff = -1, land = -1, air = 0, landLat = 0, airS = 0; bool entered = false, inAir = false; int rec0 = race.Racers[0].Recoveries, recSeen = rec0; var hops = new List<string>(); var recs = new List<string>();
                while (Time.time - t0 < 150 && Time.realtimeSinceStartup - real0 < 150)
                {
                    yield return null; if (Time.timeScale < 1.9f && flow.State == RaceFlow.Stage.Racing) Time.timeScale = 2; AudioListener.volume = 0; float s = line.Project(car.Body.position, out float lat);
                    if (race.Racers[0].Branch.Route == line) { entered = true; best = Mathf.Max(best, s); }
                    if (entered && car.GroundedWheels == 0 && !inAir) { inAir = true; air = Time.time; airS = s; }
                    if (entered && car.GroundedWheels > 0 && inAir) { inAir = false; if (Time.time - air > .12f) hops.Add($"s {airS:F1}-{s:F1} {Time.time - air:F2} s"); if (s > lipS + 5 && Time.time - air > .4f && land < 0) { takeoff = airS; land = s; landLat = lat; } }
                    if (race.Racers[0].Recoveries > recSeen) { recSeen = race.Racers[0].Recoveries; recs.Add($"at {V(car.Body.position)} (line s {s:F1}, main s {main.Project(car.Body.position, out _):F0}) '{car.GetComponent<VehicleRespawn>()?.RecoveryDiagnostic}'"); }
                    if (entered && (s > line.Length - 6 || race.Racers[0].Branch.Route == null && best > line.Length - 10)) break;
                    if (!entered && main.Project(car.Body.position, out _) > line.exitRoad + 20) break;
                }
                Time.timeScale = 1; int rec = race.Racers[0].Recoveries - rec0; Note($"  end: state {flow.State}, game {Time.time - t0:F0} s, real {Time.realtimeSinceStartup - real0:F0} s");
                Note($"race AI in the {VehicleProfile.Find(id).Name}, from the grid: took the Cabin Jump {entered}, take-off s {takeoff:F1}, landing s {land:F1} {landLat:F1} m off the line ({(land >= lastBush + 3 ? $"clear by {land - lastBush:F1} m" : land < 0 ? "no landing" : $"NOT clear ({land - lastBush:F1} m from the last bush)")}), furthest s {best:F1} of {line.Length:F1}, resets {rec}; air on the branch: {string.Join(", ", hops)}");
                foreach (var r in recs) Note("  reset " + r);
                Destroy(pilot); car.GetComponent<VehicleInput>().enabled = true; yield return null;
            }
        }
        IEnumerator ResetIn(string where)
        {
            Measure(); var line = Line(); var u = Brush(); var car = race.vehicle; var resp = car.GetComponent<VehicleRespawn>();
            float s0 = (lipS + u.clearFrom) / 2; var c = line.At(s0, out var f); var n = Vector3.Cross(Vector3.up, f).normalized; var p = c + n * 1.5f;
            Put(p, Mathf.Atan2(f.x, f.z) * Mathf.Rad2Deg + 20, 0); yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate();
            // a teleport of more than 10 m sends the racer back to the grid and clears its branch: the branch is begun after it, as if driven there
            if (!race.FreeRoam) { race.Racers[0].Branch.Clear(); race.Racers[0].Branch.Begin(line); }
            yield return new WaitForSeconds(1.5f);
            var before = car.Body.position; float sb = line.Project(before, out float lb);
            resp.ResetVehicle(); float t0 = Time.time; while (Time.time - t0 < 3 && resp.Pending) yield return null; yield return new WaitForSeconds(.5f);
            var after = car.Body.position; float sa = line.Project(after, out float la); line.At(sa, out var fa);
            bool clear = !Physics.OverlapBox(after + Vector3.up * .8f + fa * 3, new Vector3(1.2f, .6f, 2.5f), Quaternion.LookRotation(fa)).Any(x => !x.attachedRigidbody && !x.name.StartsWith("Ground_"));
            Note($"{where}: stopped in the brush at s {sb:F1} ({lb:F1} m off the line) -> reset to s {sa:F1} ({la:F1} m off), facing {Vector3.Angle(Vector3.ProjectOnPlane(car.transform.forward, Vector3.up), Vector3.ProjectOnPlane(fa, Vector3.up)):F0} deg from the line, speed {car.Body.linearVelocity.magnitude:F1}; '{resp.LastRecovery}'; far edge {u.clearFrom:F1}, last bush {lastBush:F1}: {(sa > lastBush && sa >= u.clearFrom ? "past the brush" : "NOT past the brush")}, nothing solid 3 m ahead {clear}");
            yield return Snap($"reset-{where.Replace(' ', '-')}");
        }
        IEnumerator Resets()
        {
            yield return StartRace(Arg("-p101vehicle", "mower")); yield return ResetIn("race");
            flow.Pause(); yield return null; yield return ToFreeRoam(Arg("-p101vehicle", "mower")); yield return ResetIn("Free Roam");
        }
        IEnumerator Views()
        {
            bool roam = Arg("-p101scene", "roam") == "roam"; string vehicle = Arg("-p101vehicle", "mower");
            if (roam) yield return ToFreeRoam(vehicle); else yield return StartRace(vehicle);
            foreach (var c in FindObjectsByType<Canvas>(FindObjectsSortMode.None)) c.enabled = false;
            foreach (var spec in Arg("-p101views", "").Split(';').Where(x => x.Length > 0))
            {
                var a = spec.Split(','); Put(new Vector3(F(a[1]), 0, F(a[2])), F(a[3])); race.vehicle.Body.isKinematic = false;
                yield return new WaitForSeconds(2.0f); race.vehicle.Body.linearVelocity = Vector3.zero;
                yield return Snap($"view-{(roam ? "roam" : "race")}-{a[0]}"); Note($"view {a[0]}: vehicle at {V(race.vehicle.Body.position)} heading {race.vehicle.transform.eulerAngles.y:F1}, camera {V(Camera.main.transform.position)}");
            }
        }
        IEnumerator Lap()
        {
            yield return StartRace("original");
            var car = race.vehicle; var pilot = car.gameObject.AddComponent<RoadDriver>(); pilot.Initialize(race, car, true, 1, 1); pilot.Racer = race.Racers[0]; car.GetComponent<VehicleInput>().enabled = false;
            Time.timeScale = 3; float t0 = Time.time, real0 = Time.realtimeSinceStartup; float lowestUp = 1;
            int lastRec = 0; var where = new List<string>(); var resp = car.GetComponent<VehicleRespawn>(); Vector3 prev = car.Body.position;
            while (flow.State == RaceFlow.Stage.Racing && Time.realtimeSinceStartup - real0 < 900) { AudioListener.volume = 0; yield return new WaitForFixedUpdate(); lowestUp = Mathf.Min(lowestUp, car.transform.up.y); if (race.Racers[0].Recoveries > lastRec) { lastRec = race.Racers[0].Recoveries; where.Add($"from {V(prev)} (main s {race.road.Project(prev, out _):F0}, branch {(race.Racers[0].Branch.Route ? race.Racers[0].Branch.Route.title : "-")}): {resp?.RecoveryDiagnostic}"); } prev = car.Body.position; }
            foreach (var w in where) Note("  recovery " + w);
            Time.timeScale = 1;
            Note($"Dan's Backyard Forward lap by the road AI (Street Classic): state {flow.State}, game time {Time.time - t0:F0} s, recoveries {race.Racers[0].Recoveries}, lowest up-vector {lowestUp:F2}, finished {flow.State == RaceFlow.Stage.Results}");
        }
    }
}
