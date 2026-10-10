using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Racer
{
    // 0.102 evidence only (like Report101PlayerCheck): `-p102check <dir> -racerTestSave <dir>` in the built player. Never active without both arguments.
    // All driving is by a virtual controller, at racing speed, in Dan's Backyard Forward (and Free Roam where said). -p102vehicle id (default mower).
    //   -p102kind crest   Part A: main s 40 -> past the crest -> the dirt jump at s 164, three runs: air time and how far off the trail line
    //   -p102kind edge    Part B: the Tree-Top Trail dirt jump on its far left, centre and far right: speed in and out, any snag
    //   -p102kind gate    Part C: along the storm drain side close to the edge both ways, at the grate head-on and at an angle, a reset inside
    //   -p102kind reset   Part D: ten resets from inside the Cabin brush (the first after a real landing in it), three in Free Roam
    //   -p102kind boards  Part E: onto the Cabin boards straight, 15 deg from the left and from the right at the run-up speed
    //   -p102kind views   -p102scene race|roam -p102views "name,x,z,heading;..." the vehicle placed there, the chase camera's view
    //   -p102kind lap     one lap of Dan's Backyard Forward by the road AI
    public sealed class Report102Hits : MonoBehaviour { public readonly List<string> list = new(); void OnCollisionEnter(Collision c) { if (c.impulse.magnitude > 300 && !c.collider.attachedRigidbody) list.Add($"{c.collider.name} ({c.impulse.magnitude:F0} N s)"); } }
    public sealed class Report102PlayerCheck : MonoBehaviour
    {
        string outDir; readonly List<string> rows = new();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            var a = Environment.GetCommandLineArgs(); int i = Array.IndexOf(a, "-p102check");
            if (i < 0 || i + 1 >= a.Length || Array.IndexOf(a, "-racerTestSave") < 0 || FindAnyObjectByType<Report102PlayerCheck>()) return;
            Application.runInBackground = true; // the check must not pause when its window loses focus (the game itself is unchanged)
            var g = new GameObject("Report102 player check"); DontDestroyOnLoad(g); g.AddComponent<Report102PlayerCheck>().outDir = a[i + 1];
        }
        static string Arg(string key, string fallback) { var a = Environment.GetCommandLineArgs(); int i = Array.IndexOf(a, key); return i >= 0 && i + 1 < a.Length ? a[i + 1] : fallback; }
        static float F(string s) => float.Parse(s, System.Globalization.CultureInfo.InvariantCulture);
        static string V(Vector3 v) => $"{v.x:F1},{v.y:F2},{v.z:F1}";
        void Note(string s) { rows.Add(s); File.WriteAllLines(Path.Combine(outDir, "p102-check.txt"), rows); Debug.Log("P102CHECK " + s); }
        IEnumerator Snap(string name) { yield return new WaitForEndOfFrame(); ScreenCapture.CaptureScreenshot(Path.Combine(outDir, name + ".png")); yield return null; yield return null; yield return new WaitForSecondsRealtime(.3f); }
        RaceFlow flow; RaceDirector race; Gamepad pad; string vehicle;
        void Bind() { flow = FindAnyObjectByType<RaceFlow>(); race = flow ? flow.Race : null; }
        IEnumerator WaitFlow(float limit = 120)
        {
            float t0 = Time.realtimeSinceStartup;
            while (true) { Bind(); if (flow && flow.Started && !LoadingScreen.Holding && flow.Save != null) break; yield return null; if (Time.realtimeSinceStartup - t0 > limit) break; }
            var title = FindAnyObjectByType<StartupTitle>(); if (title) { typeof(StartupTitle).GetField("completed", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)?.SetValue(null, true); Destroy(title.gameObject); }
            if (flow.State == RaceFlow.Stage.Title) flow.EnterMenuAfterTitle();
            yield return null;
        }
        void Conditions() { var st = flow.Save.Settings; st.timeOfDay = Arg("-p102dusk", "0") == "1" ? 2 : 0; st.weather = 0; st.hints = false; st.vsync = true; st.estimateAiFinishes = false; flow.Save.SaveSettings(); flow.Save.ApplySettings(); }
        IEnumerator ToBackyard()
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "DansBackyardForward") yield break;
            Campaign.Testing = true;
            for (int attempt = 0; attempt < 4 && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "DansBackyardForward"; attempt++)
            {
                if (flow.State == RaceFlow.Stage.Title) flow.EnterMenuAfterTitle();
                flow.OpenCourses(); yield return null; flow.SelectBackyardForward(); yield return null;
                float t1 = Time.realtimeSinceStartup; while (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "DansBackyardForward" && Time.realtimeSinceStartup - t1 < 30) yield return null;
            }
            yield return WaitFlow();
        }
        IEnumerator StartRace(string id)
        {
            yield return ToBackyard(); Conditions();
            flow.OpenGarage(); flow.SelectVehicle(id); flow.CloseGarage();
            race.opponents = false; race.traffic = false; race.laps = 1; race.difficulty = 1;
            flow.StartRace(); float t0 = Time.realtimeSinceStartup;
            while (flow.State != RaceFlow.Stage.Racing && Time.realtimeSinceStartup - t0 < 60) { AudioListener.volume = 0; yield return null; }
            yield return new WaitForSecondsRealtime(4);
        }
        IEnumerator ToFreeRoam(string id)
        {
            flow.Save.Settings.vehicleId = id; flow.Save.Settings.hints = false; flow.Save.SaveSettings(); Conditions();
            flow.StartFreeRoam(); float t0 = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - t0 < 120 && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != RaceFlow.RoamScene) yield return null;
            Bind(); t0 = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - t0 < 120) { Bind(); if (flow && race && flow.Started && !LoadingScreen.Holding && flow.State == RaceFlow.Stage.Racing) break; yield return null; }
            AudioListener.volume = 0; yield return new WaitForSecondsRealtime(1f);
        }
        static float GroundAt(Vector3 p, float from = 400) { float best = float.NaN; foreach (var h in Physics.RaycastAll(new Vector3(p.x, from, p.z), Vector3.down, 800, ~0, QueryTriggerInteraction.Ignore)) { if (h.collider.attachedRigidbody) continue; if (float.IsNaN(best) || h.point.y > best) best = h.point.y; } return best; }
        void PutExact(Vector3 pos, float yaw, float speed)
        {
            var car = race.vehicle; var rot = Quaternion.Euler(0, yaw, 0);
            car.Body.position = pos; car.Body.rotation = rot; car.transform.SetPositionAndRotation(pos, rot); car.Body.linearVelocity = rot * Vector3.forward * speed; car.Body.angularVelocity = Vector3.zero; car.ClearSteering();
            car.GetComponent<VehicleRespawn>()?.CancelRecovery(); FindAnyObjectByType<ChaseCamera>()?.Snap(); Physics.SyncTransforms();
        }
        void Put(Vector3 pos, float yaw, float speed = 0, float lift = .6f) { pos.y = GroundAt(pos) + lift; PutExact(pos, yaw, speed); }
        static float Yaw(Vector3 f) => Mathf.Atan2(f.x, f.z) * Mathf.Rad2Deg;
        void PadState(float steer, float throttle, float brake)
        {
            float sx = Mathf.Abs(steer) < .01f ? 0 : Mathf.Sign(steer) * (.12f + Mathf.Min(1, Mathf.Abs(steer)) * .83f);
            float th = throttle < .01f ? 0 : .1f + Mathf.Min(1, throttle) * .9f, br = brake < .01f ? 0 : .1f + Mathf.Min(1, brake) * .9f;
            InputSystem.QueueStateEvent(pad, new GamepadState { leftStick = new Vector2(sx, 0), rightTrigger = th, leftTrigger = br });
        }
        RaceRoad Main() { var m = FindObjectsByType<RaceRoad>(FindObjectsSortMode.None).First(r => r.name == "Forward navigation only - no road mesh"); m.Initialize(); return m; }
        WoodlandRoute Route(string title) { var w = FindObjectsByType<WoodlandRoute>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault(r => r.title == title); if (w) w.Initialize(); return w; }
        ShortcutUndergrowth Brush() => FindObjectsByType<ShortcutUndergrowth>(FindObjectsSortMode.None).FirstOrDefault(u => u.clearRoute && u.clearRoute.title == "Abandoned Cabin Jump");
        int Recov() => race.Racers.Count > 0 ? race.Racers[0].Recoveries : 0;

        // One controlled drive: each physics step `aim` gives the point to steer at and the speed to hold (0 = flat out); `observe` sees every step and
        // returns false to stop. Returns nothing; the caller records what it needs.
        IEnumerator Drive(Func<Vector3, (Vector3 aim, float speed)> aim, Func<bool> observe, float limit = 30)
        {
            var car = race.vehicle; float t0 = Time.time;
            while (Time.time - t0 < limit)
            {
                yield return new WaitForFixedUpdate(); AudioListener.volume = 0;
                var (target, speed) = aim(car.Body.position); var local = car.transform.InverseTransformPoint(target);
                float steer = Mathf.Clamp(Mathf.Atan2(local.x, local.z) * 2.2f, -1, 1); float v = car.ForwardSpeed;
                float th = speed <= 0 ? 1 : v < speed - .4f ? 1 : v < speed + .6f ? .35f : 0, br = speed > 0 && v > speed + 1.5f ? .3f : 0;
                PadState(steer, th, br);
                if (!observe()) break;
            }
            PadState(0, 0, 1);
        }

        IEnumerator Start()
        {
            Directory.CreateDirectory(outDir); AudioListener.volume = 0;
            string kind = Arg("-p102kind", "crest"); vehicle = Arg("-p102vehicle", "mower");
            int w = int.Parse(Arg("-p102w", "1920")), h = int.Parse(Arg("-p102h", "1080")); Screen.SetResolution(w, h, FullScreenMode.Windowed); yield return null; yield return null;
            Note($"screen={Screen.width}x{Screen.height} kind={kind} vehicle={vehicle} version={Application.version}");
            pad = InputSystem.AddDevice<Gamepad>("Report102Pad");
            yield return WaitFlow();
            switch (kind)
            {
                case "crest": yield return Crest(); break;
                case "edge": yield return Edge(); break;
                case "gate": yield return Gate(); break;
                case "reset": yield return Resets(); break;
                case "boards": yield return Boards(); break;
                case "views": yield return Views(); break;
                case "lap": yield return Lap(); break;
                default: Note("unknown kind " + kind); break;
            }
            File.WriteAllText(Path.Combine(outDir, "done.txt"), "done"); Application.Quit();
        }

        void Toggles()
        {
            foreach (var c in FindObjectsByType<Collider>(FindObjectsSortMode.None))
            {
                if (Arg("-p102nosides", "0") == "1" && c.name == "Dirt jump grounded sides") { c.enabled = false; Note("  (test: the dirt jump's side skirt collider off)"); }
                if (Arg("-p102noshoulders", "0") == "1" && c.name.Contains("shoulders (0.102)")) { c.gameObject.SetActive(false); Note("  (test: " + c.name + " off)"); }
            }
        }
        string Tr(float s, float lat, string tag) { var car = race.vehicle; var q = car.Body.position; var under = Physics.Raycast(q + Vector3.up, Vector3.down, out var h, 4, ~0, QueryTriggerInteraction.Ignore) ? h.collider.name : "-"; return $"{tag} s {s:F2} lat {lat:F2} v {car.Body.linearVelocity.magnitude:F2} fwd {car.ForwardSpeed:F2} wheels {car.GroundedWheels} up {car.transform.up.y:F2} under {under} touching {string.Join("/", Physics.OverlapBox(q, new Vector3(1.0f, .8f, 1.6f), car.transform.rotation, ~0, QueryTriggerInteraction.Ignore).Where(x => !x.attachedRigidbody).Select(x => x.name).Distinct())}"; }
        // ---------- A ----------
        IEnumerator Crest()
        {
            yield return StartRace(vehicle); var main = Main(); var car = race.vehicle; var trace = new List<string>();
            Note($"Part A, {VehicleProfile.Find(vehicle).Name}: from main s 40 at 15 m/s, flat out along the trail centre (aim 12 m ahead) to the dirt jump at s 164");
            for (int run = 0; run < 3; run++)
            {
                float lead = new[] { 0f, -1.2f, 1.2f }[run]; // centre, a little left, a little right of the line
                var p = main.At(40, out var f); Put(p, Yaw(f), 15); yield return new WaitForFixedUpdate();
                float airFrom = 0, air = 0, airMax = 0, airAt = 0, maxLat = 0, vCrest = 0, latAt160 = float.NaN, airStart = -1; int hops = 0; var hits = car.GetComponent<Report102Hits>() ?? car.gameObject.AddComponent<Report102Hits>(); hits.list.Clear(); int r0 = Recov();
                yield return Drive(q => { float s = main.Project(q, out _); var c = main.At(s + 12, out var ff); var right = Vector3.Cross(Vector3.up, new Vector3(ff.x, 0, ff.z).normalized); return (c + right * lead, 0); }, () =>
                {
                    var q = car.Body.position; float s = main.Project(q, out _); var c = main.At(s, out var ff); var right = Vector3.Cross(Vector3.up, new Vector3(ff.x, 0, ff.z).normalized); float lat = Vector3.Dot(q - c, right);
                    if (s > 95 && s < 140) { maxLat = Mathf.Max(maxLat, Mathf.Abs(lat - lead)); if (s > 104 && s < 106) vCrest = car.ForwardSpeed; }
                    if (s > 95 && s < 163) { if (car.GroundedWheels == 0) { if (airStart < 0 && s < 140) { airStart = Time.time; airFrom = s; } } else if (airStart >= 0) { float dt = Time.time - airStart; air += dt; if (dt > .08f) hops++; if (dt > airMax) { airMax = dt; airAt = airFrom; } airStart = -1; } }
                    if (s > 159 && s < 161 && float.IsNaN(latAt160)) latAt160 = lat;
                    if (s > 90 && s < 140) trace.Add($"{run} s {s:F2} lat {lat:F2} y {q.y:F3} ground {GroundAt(q, q.y + 3):F3} vy {car.Body.linearVelocity.y:F2} v {car.ForwardSpeed:F1} wheels {car.GroundedWheels} pitch {-car.transform.eulerAngles.x:F1}");
                    return s < 163;
                });
                Note($"  run {run + 1} (aimed {lead:+0.0;-0.0;0} m from the centre): {vCrest:F1} m/s over the crest; in the air over s 95-140 {air:F2} s in total, longest {airMax:F2} s (leaving the ground at s {airAt:F1}), {hops} hops over 0.08 s; furthest off its line {maxLat:F1} m; at s 160 (the jump run-in) {latAt160:F1} m from the centre; resets {Recov() - r0}; struck: {(hits.list.Count == 0 ? "nothing" : string.Join(", ", hits.list.Distinct()))}");
                yield return new WaitForSecondsRealtime(.3f);
            }
            File.WriteAllLines(Path.Combine(outDir, "trace-crest.txt"), trace);
        }

        // ---------- B ----------
        IEnumerator Edge()
        {
            yield return StartRace(vehicle); var tree = Route("Tree-Top Trail"); var car = race.vehicle; Toggles(); var trace = new List<string>();
            Note($"Part B, {VehicleProfile.Find(vehicle).Name}: the Tree-Top Trail from s 0 at 16 m/s, flat out; held on a line offset from the trail centre through the dirt jump (s 15-29)");
            foreach (var off in new[] { -1.9f, 0f, 1.9f })
            {
                race.Racers[0].Branch.Clear(); race.Racers[0].Branch.Begin(tree);
                var p = tree.At(1, out var f); var right0 = Vector3.Cross(Vector3.up, new Vector3(f.x, 0, f.z).normalized); Put(p + right0 * off, Yaw(f), 16); yield return new WaitForFixedUpdate();
                float vIn = 0, vOut = 0, vMin = 99, latMax = 0; var hits = car.GetComponent<Report102Hits>() ?? car.gameObject.AddComponent<Report102Hits>(); hits.list.Clear(); int r0 = Recov(); float minUp = 1;
                yield return Drive(q => { float s = tree.Project(q, out _); var c = tree.At(s + 8, out var ff); var right = Vector3.Cross(Vector3.up, new Vector3(ff.x, 0, ff.z).normalized); return (c + right * off, 0); }, () =>
                {
                    var q = car.Body.position; float s = tree.Project(q, out _); var c = tree.At(s, out var ff); var right = Vector3.Cross(Vector3.up, new Vector3(ff.x, 0, ff.z).normalized); float lat = Vector3.Dot(q - c, right);
                    float v = car.Body.linearVelocity.magnitude; if (s > 10 && s < 30) trace.Add(Tr(s, lat, $"{off:+0.0;-0.0;0}")); if (s > 11.5f && s < 12.5f) vIn = v; if (s > 13 && s < 28) { vMin = Mathf.Min(vMin, v); latMax = Mathf.Max(latMax, Mathf.Abs(lat - off)); minUp = Mathf.Min(minUp, car.transform.up.y); } if (s > 27.5f && s < 28.5f) vOut = v;
                    return s < 29;
                }, 12);
                Note($"  {(off < 0 ? "far left" : off > 0 ? "far right" : "centre")} ({off:+0.0;-0.0;0} m): {vIn:F1} m/s at s 12, slowest on the jump {vMin:F1}, {vOut:F1} m/s at s 28 (the lip at s 29); off its line by up to {latMax:F1} m; lowest up-vector {minUp:F2}; resets {Recov() - r0}; struck: {(hits.list.Count == 0 ? "nothing" : string.Join(", ", hits.list.Distinct()))}");
                yield return new WaitForSecondsRealtime(.5f);
            }
            File.WriteAllLines(Path.Combine(outDir, "trace-edge.txt"), trace);
        }

        // ---------- C ----------
        IEnumerator Gate()
        {
            yield return StartRace(vehicle); var main = Main(); var car = race.vehicle;
            var lintel = GameObject.Find("Culvert mouth lintel").transform; var shell = GameObject.Find("Long storm culvert walls and ceiling").GetComponent<Renderer>();
            var axisIn = Vector3.ProjectOnPlane(lintel.forward, Vector3.up).normalized; if (Vector3.Dot(shell.bounds.center - lintel.position, axisIn) < 0) axisIn = -axisIn;
            var grate = GameObject.Find("Forward closed drainage grate"); Note($"Part C, {VehicleProfile.Find(vehicle).Name}: the grate {(grate && grate.activeInHierarchy ? "is closed (shown, solid)" : "IS OPEN")} in this race");
            float floorY = lintel.position.y - 6.5f;
            bool InTunnel(Vector3 q) => Vector3.Dot(Vector3.ProjectOnPlane(q - lintel.position, Vector3.up), axisIn) > .3f && q.y < lintel.position.y - .5f && Physics.Raycast(q + Vector3.up * .5f, Vector3.up, 12, ~0, QueryTriggerInteraction.Ignore);
            // along the side, close to the edge, both ways
            foreach (var (dir, lat, speed) in new[] { (1, -3.4f, 22f), (1, -4.4f, 18f), (-1, -3.4f, 22f), (-1, -4.4f, 18f) })
            {
                float s0 = dir > 0 ? 805 : 880, s1 = dir > 0 ? 870 : 815; var p = main.At(s0, out var f); var right0 = Vector3.Cross(Vector3.up, new Vector3(f.x, 0, f.z).normalized);
                Put(p + right0 * lat, Yaw(f * dir), speed); yield return new WaitForFixedUpdate();
                bool inside = false; float lowest = 99, minS = 9999; var hits = car.GetComponent<Report102Hits>() ?? car.gameObject.AddComponent<Report102Hits>(); hits.list.Clear(); int r0 = Recov();
                yield return Drive(q => { float s = main.Project(q, out _); var c = main.At(s + 10 * dir, out var ff); var right = Vector3.Cross(Vector3.up, new Vector3(ff.x, 0, ff.z).normalized); return (c + right * lat, speed); }, () =>
                {
                    var q = car.Body.position; float s = main.Project(q, out _); var c = main.At(s, out _); if (s > 830 && s < 860) lowest = Mathf.Min(lowest, q.y - c.y); if (InTunnel(q)) inside = true;
                    return dir > 0 ? s < s1 : s > s1;
                }, 15);
                Note($"  along the drain side {(dir > 0 ? "forward" : "back the other way (retrying the Cabin)")}, {Mathf.Abs(lat):F1} m left of the trail centre (forward sense), {speed} m/s: {(inside ? "FELL INTO THE DRAIN" : "stayed out")}, lowest {lowest:+0.00;-0.00} m against the trail centre over s 830-860; resets {Recov() - r0}; struck: {(hits.list.Count == 0 ? "nothing" : string.Join(", ", hits.list.Distinct()))}; ended at {V(car.Body.position)}");
                yield return new WaitForSecondsRealtime(.3f);
            }
            // at the grate: head-on and at 30 degrees, from 25 m out in front of the mouth
            foreach (var angle in new[] { 0f, 30f, -30f })
            {
                var mouth = lintel.position - axisIn * .3f; mouth.y = GroundAt(lintel.position - axisIn * 2, lintel.position.y - 1);
                var dirv = Quaternion.Euler(0, angle, 0) * axisIn; var start = mouth - dirv * 22; Put(start, Yaw(dirv), 14); yield return new WaitForFixedUpdate();
                bool inside = false; float deepest = -99; var hits = car.GetComponent<Report102Hits>() ?? car.gameObject.AddComponent<Report102Hits>(); hits.list.Clear(); float t0 = Time.time;
                yield return Drive(q => (mouth + axisIn * 6, 14), () => { var q = car.Body.position; deepest = Mathf.Max(deepest, Vector3.Dot(Vector3.ProjectOnPlane(q - lintel.position, Vector3.up), axisIn)); if (InTunnel(q)) inside = true; return Time.time - t0 < 5; }, 6);
                Note($"  at the grate {(angle == 0 ? "head-on" : $"at {angle:+0;-0} deg")}, 14 m/s from 22 m out: {(inside ? "GOT THROUGH" : "stopped")}; the vehicle's centre came to {deepest:+0.00;-0.00} m of the grate line (+ = inside); struck: {(hits.list.Count == 0 ? "nothing" : string.Join(", ", hits.list.Distinct()))}");
                yield return Snap($"C-grate-{(angle == 0 ? "head-on" : angle > 0 ? "angle-right" : "angle-left")}");
                yield return new WaitForSecondsRealtime(.3f);
            }
            // a reset from inside the drain behind the grate (Dan's BUG-003 spot, on the drain floor under the new top)
            {
                var spot = new Vector3(167.25f, 0, 69.03f); float floor = float.NaN;
                foreach (var hh in Physics.RaycastAll(new Vector3(spot.x, lintel.position.y - 1, spot.z), Vector3.down, 20, ~0, QueryTriggerInteraction.Ignore).OrderBy(x => x.distance)) if (!hh.collider.attachedRigidbody) { floor = hh.point.y; break; }
                PutExact(new Vector3(spot.x, floor + .6f, spot.z), 66, 0); yield return new WaitForSeconds(1.0f);
                var before = car.Body.position; var resp = car.GetComponent<VehicleRespawn>(); resp.ResetVehicle(); float t0 = Time.time; while (Time.time - t0 < 3 && resp.Pending) yield return null; yield return new WaitForSeconds(.5f);
                var after = car.Body.position; float s = main.Project(after, out _); var c = main.At(s, out var ff); var right = Vector3.Cross(Vector3.up, new Vector3(ff.x, 0, ff.z).normalized);
                Note($"  reset from inside the drain at {V(before)} (inside {InTunnel(before)}): to {V(after)}, main s {s:F1}, {Vector3.Dot(after - c, right):+0.0;-0.0} m from the centre, facing {Vector3.Angle(Vector3.ProjectOnPlane(car.transform.forward, Vector3.up), Vector3.ProjectOnPlane(ff, Vector3.up)):F0} deg from the route, inside the drain {InTunnel(after)}; '{resp.LastRecovery}'");
            }
        }

        // ---------- D ----------
        string ResetLine(string where, WoodlandRoute line, ShortcutUndergrowth u, VehicleRespawn resp, Vector3 before)
        {
            var car = race.vehicle; var after = car.Body.position; float sb = line.Project(before, out float lb), sa = line.Project(after, out float la); line.At(sa, out var fa);
            float lastBush = LastBush(u, line); bool past = sa > lastBush && sa >= u.clearFrom && la < 2.5f;
            return $"{where}: in the brush at s {sb:F1}, {lb:F1} m off the line -> s {sa:F1}, {la:F1} m off, facing {Vector3.Angle(Vector3.ProjectOnPlane(car.transform.forward, Vector3.up), Vector3.ProjectOnPlane(fa, Vector3.up)):F0} deg from the line, {car.Body.linearVelocity.magnitude:F1} m/s; '{resp.LastRecovery}': {(past ? "PAST THE BRUSH" : "NOT past the brush")}";
        }
        float LastBush(ShortcutUndergrowth u, WoodlandRoute line)
        {
            var mf = u.GetComponent<MeshFilter>(); var M = u.transform.localToWorldMatrix; float last = 0; var verts = mf.sharedMesh.vertices;
            foreach (var i in new HashSet<int>(mf.sharedMesh.triangles)) { var w = M.MultiplyPoint3x4(verts[i]); float s = line.Project(w, out float lat); if (s >= u.brushFrom && lat < u.clearHalfWidth && s < u.clearFrom + 6) last = Mathf.Max(last, s); }
            return last;
        }
        IEnumerator ResetAt(string where, WoodlandRoute line, ShortcutUndergrowth u, float s, float off, float yawOff, float speed)
        {
            var car = race.vehicle; var resp = car.GetComponent<VehicleRespawn>(); var c = line.At(s, out var f); var n = Vector3.Cross(Vector3.up, new Vector3(f.x, 0, f.z).normalized);
            Put(c + n * off, Yaw(f) + yawOff, speed); yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate();
            if (!race.FreeRoam && race.Racers[0].Branch.Route != line) { race.Racers[0].Branch.Clear(); race.Racers[0].Branch.Begin(line); }
            yield return new WaitForSeconds(speed > 0 ? .35f : 1.2f);
            var before = car.Body.position; resp.ResetVehicle(); float t0 = Time.time; while (Time.time - t0 < 3 && resp.Pending) yield return null; yield return new WaitForSeconds(.4f);
            Note("  " + ResetLine(where, line, u, resp, before));
        }
        IEnumerator Resets()
        {
            yield return StartRace(vehicle); var line = Route("Abandoned Cabin Jump"); var u = Brush(); var main = Main(); var car = race.vehicle; var resp = car.GetComponent<VehicleRespawn>();
            Note($"Part D, {VehicleProfile.Find(vehicle).Name}: brush from the lip s {u.brushFrom:F1} to the far edge s {u.clearFrom:F1}, the last bush reaches s {LastBush(u, line):F1}; footprint radius {u.radius}");
            // 1: the first reset after a real landing in the brush: off the main road onto the run-up, eased off so it lands short, aimed 3 m left
            {
                float cross = main.Project(line.At(0, out _), out _); var p = main.At(cross - 70, out var f); Put(p, Yaw(f), 18); yield return new WaitForFixedUpdate(); bool onLine = false; float landed = -1, airStart = -1, landS = 0, landLat = 0;
                yield return Drive(q => { float ms = main.Project(q, out _); if (!onLine && ms >= cross - 8) { onLine = true; race.Racers[0].Branch.Clear(); race.Racers[0].Branch.Begin(line); } if (!onLine) return (main.At(ms + 12, out _), 0); float s = line.Project(q, out _); var c = line.At(Mathf.Max(s + 10, 8), out var ff); var n = -Vector3.Cross(Vector3.up, new Vector3(ff.x, 0, ff.z).normalized); return (c + (s < 40 ? n * 3.5f * Mathf.Clamp01((40 - s) / 12f) : Vector3.zero), 23); },
                    () => { var q = car.Body.position; float s = line.Project(q, out float lat); if (onLine && s > u.brushFrom - 2) { if (car.GroundedWheels == 0) { if (airStart < 0) airStart = Time.time; } else if (airStart >= 0 && Time.time - airStart > .4f && landed < 0) { landed = Time.time; landS = s; landLat = lat; } else if (car.GroundedWheels > 0) airStart = -1; } return landed < 0 || Time.time - landed < 1.0f; }, 30);
                Note($"  the real jump: landed at s {landS:F1}, {landLat:F1} m off the line; it settles in the brush (1 s)");
                var before = car.Body.position; resp.ResetVehicle(); float t0 = Time.time; while (Time.time - t0 < 3 && resp.Pending) yield return null; yield return new WaitForSeconds(.4f);
                Note("  1 (the first reset after landing) " + ResetLine("", line, u, resp, before));
            }
            int k = 2;
            foreach (var (s, off, yaw, speed) in new[] { (52f, 0f, 20f, 0f), (60f, -5f, -40f, 0f), (66f, 9f, 60f, 0f), (72f, -12f, 120f, 0f), (78f, 4f, 0f, 6f), (58f, 13f, -90f, 0f), (70f, -8f, 30f, 4f), (82f, -10f, 160f, 0f), (62f, 6f, 10f, 0f) })
                yield return ResetAt($"{k++} (s {s}, {off:+0;-0;0} m, turned {yaw:+0;-0;0} deg, {speed} m/s)", line, u, s, off, yaw, speed);
            flow.Pause(); yield return null; yield return ToFreeRoam(vehicle); line = Route("Abandoned Cabin Jump"); u = Brush();
            Note("Free Roam:");
            foreach (var (s, off, yaw) in new[] { (60f, -9f, 0f), (70f, 11f, 45f), (56f, 0f, -30f) }) yield return ResetAt($"Free Roam (s {s}, {off:+0;-0;0} m)", line, u, s, off, yaw, 0);
        }

        // ---------- E ----------
        IEnumerator Boards()
        {
            yield return StartRace(vehicle); var line = Route("Abandoned Cabin Jump"); var u = Brush(); var car = race.vehicle; float lip = u.brushFrom, lastBush = LastBush(u, line); Toggles(); var trace = new List<string>();
            float hold = vehicle == "mower" ? 33 : 32;
            Note($"Part E, {VehicleProfile.Find(vehicle).Name}: onto the boards (their foot at s 25.8, the lip at s {lip:F1}, the last bush s {lastBush:F1}) at {hold} m/s held; an angled entry holds its angle to the foot of the boards, then lines up");
            foreach (var ang in new[] { 0f, 15f, -15f })
            {
                // start at s 18, offset so the angled path reaches the line at the board foot (s 25.8)
                var cf = line.At(25.8f, out var ff); var fl = new Vector3(ff.x, 0, ff.z).normalized; var n = Vector3.Cross(Vector3.up, fl);
                var dirv = Quaternion.Euler(0, ang, 0) * fl; var start = cf - dirv * 7.8f; Put(start, Yaw(dirv), hold); yield return new WaitForFixedUpdate();
                float vBefore = 0, vAfter = 0, vMin = 99, take = -1, land = -1, landLat = 0, airStart = -1, takeV = 0; var hits = car.GetComponent<Report102Hits>() ?? car.gameObject.AddComponent<Report102Hits>(); hits.list.Clear(); int r0 = Recov(); bool angled = ang != 0;
                yield return Drive(q => { float s = line.Project(q, out _); if (angled && s < 25.8f) return (q + dirv * 10, hold); return (line.At(Mathf.Max(s + 10, 30), out _), hold); }, () =>
                {
                    var q = car.Body.position; float s = line.Project(q, out float lat); float v = car.Body.linearVelocity.magnitude; if (s > 18 && s < 48) trace.Add(Tr(s, lat, $"{ang:+0;-0;0}"));
                    if (s > 21.5f && s < 22.5f) vBefore = v; if (s > 23 && s < 34) vMin = Mathf.Min(vMin, v); if (s > 31.5f && s < 32.5f) vAfter = v;
                    if (s > lip - 4) { if (car.GroundedWheels == 0) { if (airStart < 0) { airStart = Time.time; take = s; takeV = v; } } else if (airStart >= 0) { if (Time.time - airStart > .4f && land < 0) { land = s; landLat = lat; } airStart = -1; } }
                    return land < 0 && s < lip + 70;
                }, 12);
                Note($"  {(ang == 0 ? "straight" : ang > 0 ? "15 deg from the left" : "15 deg from the right")}: {vBefore:F1} m/s before the boards (s 22), slowest over s 23-34 {vMin:F1}, {vAfter:F1} m/s on the boards (s 32); take-off s {take:F1} at {takeV:F1} m/s, landing s {land:F1} ({landLat:F1} m off): {(land >= lastBush + 3 ? $"clear of the brush by {land - lastBush:F1} m" : land < 0 ? "no landing seen" : $"IN the brush ({lastBush - land:F1} m short)")}; resets {Recov() - r0}; struck: {(hits.list.Count == 0 ? "nothing" : string.Join(", ", hits.list.Distinct()))}");
                yield return new WaitForSecondsRealtime(.5f);
            }
            File.WriteAllLines(Path.Combine(outDir, "trace-boards.txt"), trace);
        }

        IEnumerator Views()
        {
            bool roam = Arg("-p102scene", "race") == "roam";
            if (roam) yield return ToFreeRoam(vehicle); else yield return StartRace(vehicle);
            foreach (var c in FindObjectsByType<Canvas>(FindObjectsSortMode.None)) c.enabled = false;
            foreach (var spec in Arg("-p102views", "").Split(';').Where(x => x.Length > 0))
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
