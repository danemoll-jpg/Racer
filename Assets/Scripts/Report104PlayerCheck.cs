using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Racer
{
    // 0.104 evidence only (like Report103PlayerCheck): `-p104check <dir> -racerTestSave <dir>` in the built player. Never active without both arguments.
    //   -p104off            the Hwy 92 roadside not built (the "before" world: old placeholder businesses, old trees) for before/after comparisons
    //   -p104kind views     -p104scene race|roam -p104views "name,x,z,yaw;..." the vehicle placed there, the chase camera's view
    //   -p104kind top       roam: top-down tiles of the stretch (orthographic, 2 px per m, x -800..800, z 380..850), stitched by the tools
    //   -p104kind perf      -p104split 0|1: the same stations as views, 3840x2160 full screen, frame time (mean over 3 s) at each; the worst
    //   -p104kind lap       -p104course 0|1 (Street Loop Forward / Reverse): one lap with rivals and traffic, the player driven by the road AI;
    //                       every reset (who, where), every touch of the new roadside, which rivals took Granite Creek Cut; the results screen
    //   -p104kind shortcut  Street Loop Reverse: Granite Creek Cut from Hwy 92 at racing speed by a virtual controller, -p104vehicles a,b, -p104runs n
    //   -p104kind menus     Part A: the garage (its Color row), Settings, a campaign event page and the split-screen setup, reached and left with
    //                       a virtual controller; every text on screen checked for British spellings
    //   -p104kind police    roam: the hidden-police places on Hwy 92
    public sealed class Report104Contacts : MonoBehaviour
    {
        public readonly List<string> list = new(); public string who;
        void OnCollisionEnter(Collision c)
        {
            var t = c.collider.transform; bool roadside = false; while (t) { if (t.name == "Hwy 92 roadside solids") { roadside = true; break; } t = t.parent; }
            if (roadside) list.Add($"{who} touched roadside '{c.collider.name}' at {c.GetContact(0).point:F1}, impulse {c.impulse.magnitude:F0}");
        }
    }
    public sealed class Report104PlayerCheck : MonoBehaviour
    {
        string outDir; readonly List<string> rows = new();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Early() { if (Array.IndexOf(Environment.GetCommandLineArgs(), "-p104check") >= 0 && Array.IndexOf(Environment.GetCommandLineArgs(), "-p104off") >= 0) Hwy92Roadside.Disabled = true; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            var a = Environment.GetCommandLineArgs(); int i = Array.IndexOf(a, "-p104check");
            if (i < 0 || i + 1 >= a.Length || Array.IndexOf(a, "-racerTestSave") < 0 || FindAnyObjectByType<Report104PlayerCheck>()) return;
            Application.runInBackground = true; InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus; // check only
            var g = new GameObject("Report104 player check"); DontDestroyOnLoad(g); g.AddComponent<Report104PlayerCheck>().outDir = a[i + 1];
        }
        static string Arg(string key, string fallback) { var a = Environment.GetCommandLineArgs(); int i = Array.IndexOf(a, key); return i >= 0 && i + 1 < a.Length ? a[i + 1] : fallback; }
        static float F(string s) => float.Parse(s, System.Globalization.CultureInfo.InvariantCulture);
        static string V(Vector3 v) => $"{v.x:F1},{v.y:F1},{v.z:F1}";
        void Note(string s) { rows.Add(s); File.WriteAllLines(Path.Combine(outDir, "p104-check.txt"), rows); Debug.Log("P104CHECK " + s); }
        IEnumerator Snap(string name) { yield return new WaitForEndOfFrame(); ScreenCapture.CaptureScreenshot(Path.Combine(outDir, name + ".png")); yield return null; yield return null; yield return new WaitForSecondsRealtime(.3f); }
        RaceFlow flow; RaceDirector race; Gamepad pad;
        void Bind() { flow = FindAnyObjectByType<RaceFlow>(); race = flow ? flow.Race : null; }
        IEnumerator WaitFlow(float limit = 150)
        {
            float t0 = Time.realtimeSinceStartup;
            while (true) { Bind(); if (flow && flow.Started && !LoadingScreen.Holding && flow.Save != null) break; yield return null; if (Time.realtimeSinceStartup - t0 > limit) break; }
            var title = FindAnyObjectByType<StartupTitle>(); if (title) { typeof(StartupTitle).GetField("completed", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)?.SetValue(null, true); Destroy(title.gameObject); }
            if (flow.State == RaceFlow.Stage.Title) flow.EnterMenuAfterTitle();
            yield return null;
        }
        void Conditions(bool unlimited = false)
        {
            var st = flow.Save.Settings; st.timeOfDay = 0; st.weather = 0; st.hints = false; st.vsync = !unlimited; st.estimateAiFinishes = false; flow.Save.SaveSettings(); flow.Save.ApplySettings();
            if (unlimited) { QualitySettings.vSyncCount = 0; Application.targetFrameRate = -1; } // check only: frame time unclamped
        }
        IEnumerator Load(string scene)
        {
            Campaign.Testing = true;
            for (int attempt = 0; attempt < 4 && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != scene; attempt++)
            {
                if (flow.State == RaceFlow.Stage.Title) flow.EnterMenuAfterTitle();
                flow.OpenCourses(); yield return null;
                if (scene == "StreetLoopReverse") flow.SelectCourse(false, true); else if (scene == "StreetLoopGreybox") flow.SelectCourse(false, false);
                float t1 = Time.realtimeSinceStartup; while (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != scene && Time.realtimeSinceStartup - t1 < 40) yield return null;
            }
            yield return WaitFlow();
        }
        IEnumerator StartRace(string scene, string vehicle, bool opponents, bool traffic, int laps)
        {
            yield return Load(scene); Conditions();
            flow.OpenGarage(); flow.SelectVehicle(vehicle); flow.CloseGarage();
            race.opponents = opponents; race.traffic = traffic; race.laps = laps; race.difficulty = 1;
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
        static float GroundAt(Vector3 p) { float best = float.NaN; foreach (var h in Physics.RaycastAll(new Vector3(p.x, 400, p.z), Vector3.down, 800, ~0, QueryTriggerInteraction.Ignore)) { if (h.collider.attachedRigidbody || !h.collider.name.StartsWith("Ground_")) continue; if (float.IsNaN(best) || h.point.y > best) best = h.point.y; } return float.IsNaN(best) ? p.y : best; }
        void Put(ArcadeVehicle car, Vector3 pos, float yaw, float speed)
        {
            var rot = Quaternion.Euler(0, yaw, 0); pos.y = GroundAt(pos) + .6f;
            car.Body.position = pos; car.Body.rotation = rot; car.transform.SetPositionAndRotation(pos, rot); car.Body.linearVelocity = rot * Vector3.forward * speed; car.Body.angularVelocity = Vector3.zero; car.ClearSteering();
            car.GetComponent<VehicleRespawn>()?.CancelRecovery(); Physics.SyncTransforms();
        }
        IEnumerable<(string name, Vector3 pos, float yaw)> Stations()
        {
            foreach (var spec in Arg("-p104views", "").Split(';').Where(x => x.Length > 0)) { var a = spec.Split(','); yield return (a[0], new Vector3(F(a[1]), 0, F(a[2])), F(a[3])); }
        }

        IEnumerator Start()
        {
            Directory.CreateDirectory(outDir); AudioListener.volume = 0;
            string kind = Arg("-p104kind", "views");
            bool full = kind == "perf";
            if (full) Screen.SetResolution(3840, 2160, FullScreenMode.FullScreenWindow); else Screen.SetResolution(1920, 1080, FullScreenMode.Windowed);
            yield return null; yield return null;
            pad = InputSystem.AddDevice<Gamepad>("Report104Pad");
            yield return WaitFlow();
            Note($"screen={Screen.width}x{Screen.height} kind={kind} roadside={(Hwy92Roadside.Disabled ? "OFF (before)" : "on")} version={Application.version}");
            switch (kind)
            {
                case "views": yield return Views(); break;
                case "top": yield return Top(); break;
                case "perf": yield return Perf(); break;
                case "lap": yield return Lap(); break;
                case "shortcut": yield return Shortcut(); break;
                case "menus": yield return Menus(); break;
                case "police": yield return Police(); break;
                default: Note("unknown kind " + kind); break;
            }
            File.WriteAllText(Path.Combine(outDir, "done.txt"), "done"); Application.Quit();
        }
        void Roadside()
        {
            var r = FindAnyObjectByType<Hwy92Roadside>();
            Note(r ? "roadside: " + string.Join(" | ", r.Report) + $"; removed {Hwy92Roadside.Removed.Count} old businesses" : "roadside: not built");
        }

        // ---------------------------------------------------------------- views, top-down, frame time
        IEnumerator Views()
        {
            bool roam = Arg("-p104scene", "roam") == "roam";
            if (roam) yield return ToFreeRoam(Arg("-p104vehicle", "original")); else yield return StartRace(Arg("-p104race", "StreetLoopGreybox"), Arg("-p104vehicle", "original"), false, false, 1);
            Roadside(); foreach (var c in FindObjectsByType<Canvas>(FindObjectsSortMode.None)) c.enabled = false;
            foreach (var (name, pos, yaw) in Stations())
            {
                Put(race.vehicle, pos, yaw, 0); FindAnyObjectByType<ChaseCamera>()?.Snap(); race.vehicle.Body.isKinematic = false;
                yield return new WaitForSeconds(2.0f); race.vehicle.Body.linearVelocity = Vector3.zero;
                yield return Snap($"view-{name}"); Note($"view {name}: vehicle at {V(race.vehicle.Body.position)} heading {race.vehicle.transform.eulerAngles.y:F0}");
            }
        }
        IEnumerator Top()
        {
            yield return ToFreeRoam("original"); Roadside(); yield return new WaitForSeconds(2);
            race.vehicle.Body.isKinematic = true; race.vehicle.transform.position = new Vector3(0, -500, 0);
            var go = new GameObject("top camera"); var cam = go.AddComponent<Camera>(); cam.orthographic = true; cam.orthographicSize = 235; cam.aspect = 200f / 470f; cam.nearClipPlane = 1; cam.farClipPlane = 1000;
            cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = Color.black;
            for (int x = -800; x < 800; x += 200)
            {
                go.transform.SetPositionAndRotation(new Vector3(x + 100, 95, 615), Quaternion.Euler(90, 0, 0)); // below the clouds
                var rt = new RenderTexture(400, 940, 24); cam.targetTexture = rt; yield return null; cam.Render();
                var prev = RenderTexture.active; RenderTexture.active = rt; var tex = new Texture2D(400, 940, TextureFormat.RGB24, false); tex.ReadPixels(new Rect(0, 0, 400, 940), 0, 0); tex.Apply(); RenderTexture.active = prev;
                File.WriteAllBytes(Path.Combine(outDir, $"top-{x + 800:0000}.png"), tex.EncodeToPNG()); Destroy(tex); cam.targetTexture = null; rt.Release();
            }
            Note("top-down tiles: x -800..800 in 200 m tiles, z 380..850, 2 px per m");
        }
        IEnumerator Perf()
        {
            bool split = Arg("-p104split", "0") == "1"; ArcadeVehicle p2 = null;
            if (split)
            {
                yield return Load("StreetLoopGreybox"); Conditions(true);
                SplitScreen.P1Device = pad; SplitScreen.P2Device = null; SplitScreen.P2Ai = true; SplitScreen.Mode = SplitScreen.Kind.Race; SplitScreen.Course = 0; SplitScreen.Laps = 1; SplitScreen.Rivals = 0;
                SplitScreen.Time = TimeOfDay.Day; SplitScreen.Weather = Weather.Clear; SplitScreen.Traffic = false; SplitScreen.P1Vehicle = "original"; SplitScreen.P2Vehicle = "original";
                flow.StartSplit(); float t0 = Time.realtimeSinceStartup; yield return null;
                while ((flow == null || !flow.Started || LoadingScreen.Holding || flow.State != RaceFlow.Stage.Racing) && Time.realtimeSinceStartup - t0 < 150) { yield return null; Bind(); AudioListener.volume = 0; }
                yield return new WaitForSecondsRealtime(4); p2 = SplitScreen.Race ? SplitScreen.Race.P2Car : null;
                if (p2 && p2.GetComponent<RoadDriver>()) p2.GetComponent<RoadDriver>().enabled = false;
                Note($"split-screen race running: {SplitScreen.Active}, player 2 car {(p2 ? p2.name : "none")}");
            }
            else yield return ToFreeRoam("original");
            Conditions(true); Roadside();
            race.vehicle.GetComponent<VehicleInput>().enabled = false;
            var results = new List<(string name, float mean, float p95)>();
            foreach (var (name, pos, yaw) in Stations())
            {
                Put(race.vehicle, pos, yaw, 0); if (p2) Put(p2, pos + Quaternion.Euler(0, yaw, 0) * new Vector3(-4.1f, 0, 8), yaw, 0);
                foreach (var c in FindObjectsByType<ChaseCamera>(FindObjectsSortMode.None)) c.Snap();
                yield return new WaitForSecondsRealtime(1.5f);
                var times = new List<float>(); float t0 = Time.realtimeSinceStartup; while (Time.realtimeSinceStartup - t0 < 3) { yield return null; times.Add(Time.unscaledDeltaTime * 1000); }
                times.Sort(); float mean = times.Average(), p95 = times[(int)(times.Count * .95f)]; results.Add((name, mean, p95));
                Note($"  {name}: mean {mean:F2} ms ({1000 / mean:F0} fps), 95th percentile {p95:F2} ms, {times.Count} frames");
            }
            var worst = results.OrderByDescending(r => r.mean).First();
            Note($"WORST VIEW {(split ? "split-screen" : "full screen")} {Screen.width}x{Screen.height}: {worst.name} mean {worst.mean:F2} ms ({1000 / worst.mean:F0} fps), 95th {worst.p95:F2} ms; all stations mean {results.Average(r => r.mean):F2} ms");
            Put(race.vehicle, Stations().First(s => s.name == worst.name).pos, Stations().First(s => s.name == worst.name).yaw, 0); yield return new WaitForSecondsRealtime(1); yield return Snap("perf-worst-" + (split ? "split" : "full"));
        }

        // ---------------------------------------------------------------- a lap with rivals and traffic; the shortcut
        static readonly Regex British = new(@"\b(colou?rs?\b(?<=colour|colours)|grey|greys|centre|centres|metre|metres|favourite|honour|licence|tyre|tyres|kerb|theatre|travelling|travelled|cancelled|cancelling|practise|organis\w*|realis\w*|recognis\w*|behaviour|neighbour|programme|catalogue|whilst|storey|customis\w*|personalis\w*|optimis\w*)\b", RegexOptions.IgnoreCase);
        void TextScan(string where)
        {
            var texts = FindObjectsByType<UnityEngine.UI.Text>(FindObjectsSortMode.None).Where(t => t.isActiveAndEnabled && !string.IsNullOrWhiteSpace(t.text)).Select(t => t.text).ToList();
            var bad = texts.SelectMany(t => British.Matches(t).Cast<Match>().Select(m => m.Value)).Distinct().ToList();
            File.AppendAllLines(Path.Combine(outDir, "texts.txt"), new[] { "==== " + where }.Concat(texts.Select(t => "  " + t.Replace("\n", " / "))));
            Note($"{where}: {texts.Count} texts on screen, British spellings: {(bad.Count == 0 ? "none" : string.Join(", ", bad))}");
        }
        static List<Vector3> Branch(string scene, int index) { var c = CoursePreviewCatalog.Courses.First(x => x.scene == scene); return c.branches[index].points.ToList(); }
        IEnumerator Lap()
        {
            int course = int.Parse(Arg("-p104course", "0")); string scene = course == 0 ? "StreetLoopGreybox" : "StreetLoopReverse";
            yield return StartRace(scene, Arg("-p104vehicle", "original"), true, true, 1); Roadside();
            var car = race.vehicle; var pilot = car.gameObject.AddComponent<RoadDriver>(); pilot.Initialize(race, car, true, 1, 1); pilot.Racer = race.Racers[0]; car.GetComponent<VehicleInput>().enabled = false;
            var contacts = new List<Report104Contacts>();
            foreach (var r in race.Racers) { var c = r.Car.gameObject.AddComponent<Report104Contacts>(); c.who = r.Name + (r.IsAi ? " (AI)" : " (player)"); contacts.Add(c); }
            var shortcut = Branch("StreetLoopReverse", 1); var mid = shortcut[shortcut.Count / 2]; var tookShortcut = new HashSet<string>();
            var recov = race.Racers.ToDictionary(r => r, r => r.Recoveries); var where = new List<string>();
            Time.timeScale = 3; float real0 = Time.realtimeSinceStartup;
            float lastNote = 0;
            while (flow.State == RaceFlow.Stage.Racing && Time.realtimeSinceStartup - real0 < 900)
            {
                AudioListener.volume = 0; yield return null;
                if (Time.realtimeSinceStartup - lastNote > 30) { lastNote = Time.realtimeSinceStartup; Note($"  ... {Time.realtimeSinceStartup - real0:F0} s: stage {flow.State}, timeScale {Time.timeScale}, player lap {race.Racers[0].Progress.CompletedLaps}, at {V(car.Body.position)}"); }
                if (Time.timeScale == 0) { Time.timeScale = 3; }
                foreach (var r in race.Racers)
                {
                    if (r.Recoveries > recov[r]) { recov[r] = r.Recoveries; where.Add($"reset: {r.Name}{(r.IsAi ? " (AI)" : "")} at {V(r.Car.Body.position)}"); }
                    var p = r.Car.Body.position; p.y = mid.y; if ((p - mid).sqrMagnitude < 15 * 15) tookShortcut.Add(r.Name + (r.IsAi ? " (AI)" : " (player)"));
                }
            }
            Time.timeScale = 1;
            Note($"ended in stage {flow.State} after {Time.realtimeSinceStartup - real0:F0} s");
            Note($"{scene} lap: rivals {race.Racers.Count - 1}, traffic {race.traffic}; finished {flow.State == RaceFlow.Stage.Results}; resets {race.Racers.Sum(r => r.Recoveries)}");
            foreach (var w in where) Note("  " + w);
            var touches = contacts.SelectMany(c => c ? c.list : new List<string>()).ToList(); Note($"  touches of the new roadside: {touches.Count}"); foreach (var t in touches.Take(20)) Note("    " + t);
            if (course == 1) Note($"  took Granite Creek Cut: {(tookShortcut.Count == 0 ? "nobody" : string.Join(", ", tookShortcut))}");
            yield return new WaitForSecondsRealtime(3); yield return Snap($"results-{scene}"); TextScan($"results screen ({scene})");
        }
        IEnumerator Shortcut()
        {
            var vehicles = Arg("-p104vehicles", "mower,original").Split(','); int runs = int.Parse(Arg("-p104runs", "2"));
            yield return StartRace("StreetLoopReverse", vehicles[0], false, false, 1); Roadside();
            var branch = Branch("StreetLoopReverse", 1); var entry = branch[0]; var end = branch[^1];
            foreach (var v in vehicles)
            {
                if (VehicleProfile.Find(v) == null) { Note("no vehicle " + v); continue; }
                if (race.vehicle.GetComponent<VehicleConfiguration>().profileId != v) { flow.Pause(); flow.OpenGarage(); flow.SelectVehicle(v); flow.CloseGarage(); flow.Resume(); yield return new WaitForSecondsRealtime(1); }
                var car = race.vehicle; car.GetComponent<VehicleInput>().enabled = true; InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                car.GetComponent<VehicleInput>().Bind(pad); // the game's own split-screen binding (a real controller on this PC would otherwise take over)
                for (int run = 0; run < runs; run++)
                {
                    // on Hwy 92 west-bound in the race line's lane, 110 m before the cut's entrance, at racing speed
                    var start = entry + new Vector3(110 + run * 30, 0, 0); var path = new List<Vector3> { start, entry }; path.AddRange(branch);
                    Put(car, start, 270, 26); FindAnyObjectByType<ChaseCamera>()?.Snap();
                    int r0 = race.Racers[0].Recoveries; var hit = car.gameObject.GetComponent<Report104Contacts>() ?? car.gameObject.AddComponent<Report104Contacts>(); hit.who = v; hit.list.Clear();
                    var solidHits = new List<string>(); float vmin = 99, vEntry = 0; int seg = 0; float t0 = Time.time; bool reached = false, frames = run == 0;
                    int shot = 0;
                    while (Time.time - t0 < 60)
                    {
                        yield return new WaitForFixedUpdate(); AudioListener.volume = 0;
                        var p = car.Body.position; while (seg < path.Count - 1 && Flat(path[seg + 1] - p).magnitude < 12) seg++;
                        var target = path[Mathf.Min(seg + 1, path.Count - 1)]; var to = Flat(target - p); float ang = Vector3.SignedAngle(Flat(car.transform.forward), to, Vector3.up);
                        float speed = car.Body.linearVelocity.magnitude; float steer = Mathf.Clamp(ang / 25f, -1, 1);
                        // a racing line through the cut: flat out on the highway, braking to 24 m/s for the turn-off, then by the bend ahead
                        float ahead = 0; for (int k = seg + 1; k < Mathf.Min(seg + 5, path.Count - 1); k++) ahead = Mathf.Max(ahead, Vector3.Angle(Flat(path[k] - path[k - 1]), Flat(path[k + 1] - path[k])));
                        float cap = Flat(entry - p).magnitude < 70 && seg == 0 ? 24 : Mathf.Lerp(30, 14, Mathf.InverseLerp(5, 40, ahead + Mathf.Abs(ang) * .5f));
                        float throttle = speed < cap ? 1 : 0, brake = speed > cap + 2 ? Mathf.Clamp01((speed - cap) / 6) : 0;
                        InputSystem.QueueStateEvent(pad, new GamepadState { leftStick = new Vector2(Mathf.Abs(steer) < .02f ? 0 : steer, 0), rightTrigger = throttle, leftTrigger = brake });
                        if (seg >= 2) vmin = Mathf.Min(vmin, speed); if (seg == 1 || seg == 2) vEntry = Mathf.Max(vEntry, speed);
                        if (frames && seg >= 1 && shot < 6 && Time.time - t0 > shot * 1.6f) { yield return Snap($"shortcut-{v}-{shot:00}"); shot++; }
                        if (Flat(end - p).magnitude < 15) { reached = true; break; }
                        if (race.Racers[0].Recoveries > r0) break;
                    }
                    InputSystem.QueueStateEvent(pad, new GamepadState());
                    foreach (var t in hit.list) Note("    " + t);
                    Note($"{VehicleProfile.Find(v).Name} run {run + 1}: {(reached ? "through Granite Creek Cut to Trickum Rd" : "did NOT get through")} in {Time.time - t0:F1} s, {vEntry:F1} m/s at the entrance, slowest {vmin:F1} m/s in the cut, resets {race.Racers[0].Recoveries - r0}, touches of the new roadside {hit.list.Count}");
                    yield return new WaitForSecondsRealtime(.5f);
                }
            }
        }
        static Vector3 Flat(Vector3 v) { v.y = 0; return v; }

        // ---------------------------------------------------------------- Part A screens with a controller
        IEnumerator Press(Func<GamepadState> state, string what, int times = 1)
        {
            for (int i = 0; i < times; i++) { InputSystem.QueueStateEvent(pad, state()); yield return new WaitForSecondsRealtime(.12f); InputSystem.QueueStateEvent(pad, new GamepadState()); yield return new WaitForSecondsRealtime(.25f); }
        }
        static GamepadState Btn(GamepadButton b) => new GamepadState().WithButton(b);
        IEnumerator Menus()
        {
            yield return Load("StreetLoopGreybox"); Conditions(); yield return new WaitForSecondsRealtime(1);
            TextScan("main menu"); yield return Snap("A-menu");
            flow.OpenGarage(); yield return new WaitForSecondsRealtime(1);
            for (int i = 0; i < 12; i++)
            {
                var hl = FindObjectsByType<UnityEngine.UI.Text>(FindObjectsSortMode.None).FirstOrDefault(t => t.isActiveAndEnabled && t.text.StartsWith("Color:"));
                if (hl) break; yield return Press(() => Btn(GamepadButton.DpadDown), "down");
            }
            yield return Snap("A-garage"); TextScan("garage");
            var colorRow = FindObjectsByType<UnityEngine.UI.Text>(FindObjectsSortMode.None).FirstOrDefault(t => t.isActiveAndEnabled && t.text.StartsWith("Color"));
            Note($"garage Color row: '{(colorRow ? colorRow.text : "not found")}'");
            yield return Press(() => Btn(GamepadButton.East), "B"); yield return new WaitForSecondsRealtime(.5f);
            Note($"after B: state {flow.State}");
            flow.OpenSettings(); yield return new WaitForSecondsRealtime(1); yield return Snap("A-settings"); TextScan("settings");
            for (int page = 1; page <= 3; page++) { yield return Press(() => Btn(GamepadButton.DpadDown), "down", 8); yield return Snap($"A-settings-{page}"); TextScan($"settings, after {page * 8} presses down"); }
            yield return Press(() => Btn(GamepadButton.East), "B"); yield return new WaitForSecondsRealtime(.5f); Note($"after B: state {flow.State}");
            FindAnyObjectByType<RaceMenus>().OpenCampaign(); yield return new WaitForSecondsRealtime(1.5f); yield return Snap("A-campaign"); TextScan("campaign");
            yield return Press(() => Btn(GamepadButton.South), "A"); yield return new WaitForSecondsRealtime(1.5f); yield return Snap("A-campaign-event"); TextScan("campaign event page");
            yield return Press(() => Btn(GamepadButton.East), "B", 2); yield return new WaitForSecondsRealtime(.5f); Note($"after B: state {flow.State}");
            FindAnyObjectByType<RaceMenus>().OpenSplitSetup(); yield return new WaitForSecondsRealtime(1.5f); yield return Snap("A-split-setup"); TextScan("split-screen setup");
            yield return Press(() => Btn(GamepadButton.DpadDown), "down", 3); yield return Snap("A-split-setup-2"); TextScan("split-screen setup, 3 down");
            yield return Press(() => Btn(GamepadButton.East), "B"); yield return new WaitForSecondsRealtime(.5f); Note($"after B: state {flow.State}");
        }

        // ---------------------------------------------------------------- hidden police on Hwy 92
        IEnumerator Police()
        {
            yield return ToFreeRoam("original"); Roadside();
            float t0 = Time.realtimeSinceStartup; while (!HiddenPolice.Current && Time.realtimeSinceStartup - t0 < 20) yield return null;
            var hp = HiddenPolice.Current; if (!hp) { Note("no HiddenPolice"); yield break; }
            foreach (var s in hp.All) Note($"hiding place {s.road}: {V(s.pos)} facing {V(s.forward)}");
            foreach (var s in hp.All.Where(s => s.road == "Hwy 92").Take(2))
            {
                Put(race.vehicle, s.pos + s.forward * 34 + Vector3.Cross(Vector3.up, s.forward) * 6, Quaternion.LookRotation(-s.forward).eulerAngles.y, 0);
                var prop = GetawayChase.MakePatrolProp(race, s.pos, s.forward); yield return new WaitForSeconds(2); FindAnyObjectByType<ChaseCamera>()?.Snap(); yield return new WaitForSeconds(.5f);
                foreach (var c in FindObjectsByType<Canvas>(FindObjectsSortMode.None)) c.enabled = false;
                yield return Snap($"police-{s.pos.x:F0}"); if (prop) Destroy(prop.gameObject);
            }
        }
    }
}
