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
    // 0.103 evidence only (like Report102PlayerCheck): `-p103check <dir> -racerTestSave <dir>` in the built player. Never active without both arguments.
    //   -p103kind joint   Part A: the Abandoned Cabin Jump driven from the main road by a virtual controller, flat out, on the centre and 0.5 m
    //                     left / right of it (-p103runs per lane, default 5), -p103vehicle id (default mower). Speed every 0.05 s from 3 m before
    //                     the joint where the run-up boards meet the roof deck to the lip; every physics step there: wheels, suspension, contacts,
    //                     brush drag. -p103frames 1: a frame sequence of the first run across the joint. -p103scene roam: Free Roam.
    public sealed class Report103Contacts : MonoBehaviour
    {
        public readonly List<(float t, string name, float impulse, Vector3 normal, Vector3 point)> list = new();
        void OnCollisionEnter(Collision c) => Add(c); void OnCollisionStay(Collision c) => Add(c);
        void Add(Collision c) { if (c.collider.attachedRigidbody) return; var p = c.contactCount > 0 ? c.GetContact(0) : default; list.Add((Time.fixedTime, c.collider.name, c.impulse.magnitude, p.normal, p.point)); if (list.Count > 4000) list.RemoveRange(0, 2000); }
    }
    public sealed class Report103PlayerCheck : MonoBehaviour
    {
        string outDir; readonly List<string> rows = new();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            var a = Environment.GetCommandLineArgs(); int i = Array.IndexOf(a, "-p103check");
            if (i < 0 || i + 1 >= a.Length || Array.IndexOf(a, "-racerTestSave") < 0 || FindAnyObjectByType<Report103PlayerCheck>()) return;
            Application.runInBackground = true; // the check must not pause when its window loses focus (the game itself is unchanged)
            // nor lose its virtual controller: by default the Input System disables such a device while the window is not focused (check only)
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            var g = new GameObject("Report103 player check"); DontDestroyOnLoad(g); g.AddComponent<Report103PlayerCheck>().outDir = a[i + 1];
        }
        static string Arg(string key, string fallback) { var a = Environment.GetCommandLineArgs(); int i = Array.IndexOf(a, key); return i >= 0 && i + 1 < a.Length ? a[i + 1] : fallback; }
        static string V(Vector3 v) => $"{v.x:F2},{v.y:F2},{v.z:F2}";
        void Note(string s) { rows.Add(s); File.WriteAllLines(Path.Combine(outDir, "p103-check.txt"), rows); Debug.Log("P103CHECK " + s); }
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
        void Conditions() { var st = flow.Save.Settings; st.timeOfDay = 0; st.weather = 0; st.hints = false; st.vsync = true; st.estimateAiFinishes = false; flow.Save.SaveSettings(); flow.Save.ApplySettings(); }
        IEnumerator StartRace(string id)
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "DansBackyardForward")
            {
                Campaign.Testing = true;
                for (int attempt = 0; attempt < 4 && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "DansBackyardForward"; attempt++)
                {
                    if (flow.State == RaceFlow.Stage.Title) flow.EnterMenuAfterTitle();
                    flow.OpenCourses(); yield return null; flow.SelectBackyardForward(); yield return null;
                    float t1 = Time.realtimeSinceStartup; while (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "DansBackyardForward" && Time.realtimeSinceStartup - t1 < 30) yield return null;
                }
                yield return WaitFlow();
            }
            Conditions();
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
        void Put(Vector3 pos, float yaw, float speed)
        {
            var car = race.vehicle; var rot = Quaternion.Euler(0, yaw, 0); pos.y += .6f; // the route's own surface height (the runtime trees differ each session; a crown above the road is not the road)
            car.Body.position = pos; car.Body.rotation = rot; car.transform.SetPositionAndRotation(pos, rot); car.Body.linearVelocity = rot * Vector3.forward * speed; car.Body.angularVelocity = Vector3.zero; car.ClearSteering();
            car.GetComponent<VehicleRespawn>()?.CancelRecovery(); FindAnyObjectByType<ChaseCamera>()?.Snap(); Physics.SyncTransforms();
        }
        static float Yaw(Vector3 f) => Mathf.Atan2(f.x, f.z) * Mathf.Rad2Deg;
        void PadState(float steer, float throttle)
        {
            float sx = Mathf.Abs(steer) < .01f ? 0 : Mathf.Sign(steer) * (.12f + Mathf.Min(1, Mathf.Abs(steer)) * .83f);
            InputSystem.QueueStateEvent(pad, new GamepadState { leftStick = new Vector2(sx, 0), rightTrigger = throttle < .01f ? 0 : .1f + Mathf.Min(1, throttle) * .9f, leftTrigger = 0 });
        }
        RaceRoad Main() { var m = FindObjectsByType<RaceRoad>(FindObjectsSortMode.None).First(r => r.name == "Forward navigation only - no road mesh"); m.Initialize(); return m; }
        WoodlandRoute Route(string title) { var w = FindObjectsByType<WoodlandRoute>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault(r => r.title == title); if (w) w.Initialize(); return w; }

        IEnumerator Start()
        {
            Directory.CreateDirectory(outDir); AudioListener.volume = 0;
            string kind = Arg("-p103kind", "joint"); vehicle = Arg("-p103vehicle", "mower");
            Screen.SetResolution(1920, 1080, FullScreenMode.Windowed); yield return null; yield return null;
            Note($"screen={Screen.width}x{Screen.height} kind={kind} vehicle={vehicle} version={Application.version}");
            pad = InputSystem.AddDevice<Gamepad>("Report103Pad");
            yield return WaitFlow();
            if (kind == "joint") yield return Joint(); else Note("unknown kind " + kind);
            File.WriteAllText(Path.Combine(outDir, "done.txt"), "done"); Application.Quit();
        }

        // where the run-up boards meet the roof deck: the first row of the takeoff collider wider than the boards (along the line)
        float JointStation(WoodlandRoute line)
        {
            var take = FindObjectsByType<MeshCollider>(FindObjectsSortMode.None).First(c => c.name == "Takeoff - Leaning boards through cabin roof");
            var m = take.transform.localToWorldMatrix; float best = float.MaxValue;
            foreach (var v in take.sharedMesh.vertices) { var w = m.MultiplyPoint3x4(v); float s = line.Project(w, out float lat); if (lat > 3.2f) best = Mathf.Min(best, s); }
            return best;
        }

        IEnumerator Joint()
        {
            bool roam = Arg("-p103scene", "race") == "roam"; int runs = int.Parse(Arg("-p103runs", "5")); bool frames = Arg("-p103frames", "0") == "1";
            if (roam) yield return ToFreeRoam(vehicle); else yield return StartRace(vehicle);
            var line = Route("Abandoned Cabin Jump"); var main = Main(); var car = race.vehicle;
            { float tw = Time.realtimeSinceStartup; while ((flow.State != RaceFlow.Stage.Racing || car.Body.isKinematic) && Time.realtimeSinceStartup - tw < 60) yield return null; yield return new WaitForSeconds(1); }
            // a real controller connected to this PC takes the throttle action from the virtual pad when it sends events: the check binds the
            // vehicle's input to the virtual pad (the game's own split-screen binding; still the real input path)
            car.GetComponent<VehicleInput>().Bind(pad);
            Note($"  ready: gamepads {string.Join(", ", Gamepad.all.Select(g => g.displayName))}; state {flow.State}, kinematic {car.Body.isKinematic}, at {V(car.Body.position)}");
            float joint = JointStation(line), lip = line.Project(new Vector3(224.61f, 74.15f, 80.95f), out _);
            var contacts = car.GetComponent<Report103Contacts>() ?? car.gameObject.AddComponent<Report103Contacts>();
            Note($"Part A, {VehicleProfile.Find(vehicle).Name}, {(roam ? "Free Roam" : "Dan's Backyard Forward race")}: the joint (boards meet the roof deck) at line s {joint:F2}, the lip at s {lip:F2}; from the main road 70-86 m before the turn-off (4 m further back each run), flat out, onto the line 8 m before it; {runs} runs per lane");
            float cross = main.Project(line.At(0, out _), out _);
            var trace = new List<string>(); var worst = (drop: 0f, run: ""); int unclean = 0;
            foreach (var lane in new[] { 0f, -.5f, .5f })
                for (int run = 0, tries = 0; run < runs; run++)
                {
                    string tag = $"{(lane == 0 ? "centre" : lane < 0 ? "0.5 m left" : "0.5 m right")} run {run + 1}";
                    // the Free Roam approach is along the Reverse trail (the Forward main is closed there): start on the line's own extension instead
                    if (roam) { var p0 = line.At(2, out var f0); Put(p0, Yaw(f0), 28); } // Free Roam: a flying start on the run-up itself (the main road is closed before the turn-off there)
                    else { var p = main.At(cross - 70 - 4 * run, out var f); Put(p, Yaw(f), 18); } // each run starts 4 m further back: five different arrivals
                    yield return new WaitForFixedUpdate(); var placed = car.Body.position; var path = new List<string>();
                    bool onLine = roam; if (!roam) race.Racers[0].Branch.Clear();
                    float nextLog = -1, maxV = 0, drop = 0, dropAt = 0, takeoff = -1, takeV = 0, air = -1, lastS = 0; var speeds = new List<string>(); var hit = new List<string>(); int shot = 0; float nextShot = 0; contacts.list.Clear();
                    yield return Drive(q =>
                    {
                        float ms = main.Project(q, out _);
                        if (!onLine && ms >= cross - 8) { onLine = true; race.Racers[0].Branch.Clear(); race.Racers[0].Branch.Begin(line); }
                        if (!onLine) return main.At(ms + 12, out _);
                        float s = line.Project(q, out _); var c = line.At(Mathf.Max(s + 10, 4), out var ff); return c + Vector3.Cross(Vector3.up, new Vector3(ff.x, 0, ff.z).normalized) * lane;
                    }, () =>
                    {
                        var q = car.Body.position; if (path.Count < 40 && Time.frameCount % 10 == 0) path.Add($"{V(q)} v{car.Body.linearVelocity.magnitude:F0} w{car.GroundedWheels}"); float s = line.Project(q, out _); var c = line.At(s, out var ff); float lat = Vector3.Dot(q - c, Vector3.Cross(Vector3.up, new Vector3(ff.x, 0, ff.z).normalized)); lastS = s;
                        float v = car.Body.linearVelocity.magnitude;
                        if (onLine && s >= 15 && s < joint - 3) trace.Add($"{tag} {s:F3} lat {lat:+0.00;-0.00} v {v:F2} vy {car.Body.linearVelocity.y:F2} wheels {car.GroundedWheels} lift {car.SuspensionLift:F1} pitch {-car.transform.eulerAngles.x:F1} ground {GroundAt(q, q.y + 2):F3} contacts {string.Join("; ", contacts.list.Where(x => Mathf.Abs(x.t - Time.fixedTime) < 1e-4f).Select(x => $"{x.name} {x.impulse:F1}"))}");
                        if (onLine && s >= joint - 3 && s <= lip + .5f)
                        {
                            if (Time.time >= nextLog) { speeds.Add($"{s:F2}:{v:F2}"); nextLog = (nextLog < 0 ? Time.time : nextLog) + .05f; }
                            if (s <= joint + 3) { maxV = Mathf.Max(maxV, v); if (maxV - v > drop) { drop = maxV - v; dropAt = s; } }
                            var now = contacts.list.Where(x => Mathf.Abs(x.t - Time.fixedTime) < 1e-4f).ToList();
                            foreach (var x in now) if (x.impulse > 1) hit.Add($"{x.name} {x.impulse:F0} N s at s {line.Project(x.point, out _):F2} normal {V(x.normal)}");
                            trace.Add($"{tag} {s:F3} lat {lat:+0.00;-0.00} v {v:F2} fwd {car.ForwardSpeed:F2} vy {car.Body.linearVelocity.y:F2} wheels {car.GroundedWheels} lift {car.SuspensionLift:F1} pitch {-car.transform.eulerAngles.x:F1} ground {GroundAt(q, q.y + 2):F3} brush {Brush(car):F2} contacts {string.Join("; ", now.Select(x => $"{x.name} {x.impulse:F1}"))}");
                            if (frames && run == 0 && lane == 0 && s >= joint - 2 && shot < 12 && Time.time >= nextShot) { nextShot = Time.time + .06f; ScreenCapture.CaptureScreenshot(Path.Combine(outDir, $"frame-{shot:00}-s{s:F1}.png")); shot++; }
                        }
                        if (onLine && s > lip - 3) { if (car.GroundedWheels == 0) { if (air < 0) { air = Time.time; takeoff = s; takeV = v; } } else air = -1; }
                        return (!onLine || s < lip + 6) && (air < 0 || Time.time - air < 3);
                    }, 25);
                    if (!onLine && tries++ < 3) { Note($"  ({tag}: never reached the line, state {flow.State}, kinematic {car.Body.isKinematic}, at {V(car.Body.position)}; placed at {V(placed)} (main s {cross - 70 - 4 * run:F1}, cross {cross:F1}); path {string.Join(" > ", path)}; again)"); run--; yield return new WaitForSeconds(2); continue; }
                    bool clean = takeoff > lip - 1.5f && takeV > 0 && car.transform.up.y > .7f;
                    if (!clean) unclean++;
                    if (drop > worst.drop) worst = (drop, $"{tag} at s {dropAt:F2}");
                    Note($"  {tag}: {speeds.FirstOrDefault()?.Split(':')[1]} m/s at s {joint - 3:F1}; largest speed drop within 3 m of the joint {drop:F2} m/s (at s {dropAt:F2}); take-off at s {takeoff:F2}, {takeV:F2} m/s, {(clean ? "clean" : "NOT CLEAN")}; struck: {(hit.Count == 0 ? "nothing" : string.Join(" | ", hit.Distinct().Take(6)))}; ended at {V(car.Body.position)} line s {lastS:F1}");
                    Note($"     speed every 0.05 s (s:m/s): {string.Join(" ", speeds)}");
                    yield return new WaitForSecondsRealtime(.3f);
                }
            Note($"WORST speed drop at the joint: {worst.drop:F2} m/s ({worst.run}); runs not taking off clean: {unclean}");
            File.WriteAllLines(Path.Combine(outDir, "trace-joint.txt"), trace);
        }
        static float Brush(ArcadeVehicle car) { float a = 0; foreach (var u in FindObjectsByType<ShortcutUndergrowth>(FindObjectsSortMode.None)) a = Mathf.Max(a, u.Coverage(car.Body.position)); return a; }

        IEnumerator Drive(Func<Vector3, Vector3> aim, Func<bool> observe, float limit)
        {
            var car = race.vehicle; float t0 = Time.time;
            while (Time.time - t0 < limit)
            {
                yield return new WaitForFixedUpdate(); AudioListener.volume = 0;
                var local = car.transform.InverseTransformPoint(aim(car.Body.position));
                PadState(Mathf.Clamp(Mathf.Atan2(local.x, local.z) * 2.2f, -1, 1), 1);
                if (!observe()) break;
            }
            PadState(0, 0);
        }
    }
}
