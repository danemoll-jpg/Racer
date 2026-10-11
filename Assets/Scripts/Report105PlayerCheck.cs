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
    // 0.105 evidence only (like Report104PlayerCheck): `-p105check <dir> -racerTestSave <dir>` in the built player. Never active without both arguments.
    //   -p105kind bump      Free Roam: the mower and the Street Classic driven by a virtual controller along Hwy 92 through Dan's bump spot (and the two
    //                       seam ends), both directions; the body's vertical speed every physics step
    //   -p105kind entrance  Free Roam: Dan's sidewalk view, top-down shots of three entrances (-p105mouths "x,z;x,z;x,z"), a drive in and out of one
    //   -p105kind trees     Free Roam: driver's views at Dan's two tree spots, and the Pine Ridge trail driven end to end both ways (every contact)
    //   -p105kind police    Free Roam: every hiding place, every 0.2 s tick of a 70 mph pass in each lane, both facings, the mower and the Street
    //                       Classic, through the game's own test (HiddenPolice.Sees); then real passes at three places and one inside the quiet period
    //   -p105kind moon      Free Roam at night: a moonset behind a ridge (touching, half hidden, gone) and a moonrise
    public sealed class Report105Contacts : MonoBehaviour
    {
        public readonly List<string> list = new(); public string who;
        void OnCollisionEnter(Collision c)
        {
            if (c.collider.name.StartsWith("Ground_")) return;
            list.Add($"{who} touched '{c.collider.name}' ({c.collider.transform.parent?.name}) at {c.GetContact(0).point:F1}, impulse {c.impulse.magnitude:F0}");
        }
    }
    public sealed class Report105PlayerCheck : MonoBehaviour
    {
        string outDir; readonly List<string> rows = new();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            var a = Environment.GetCommandLineArgs(); int i = Array.IndexOf(a, "-p105check");
            if (i < 0 || i + 1 >= a.Length || Array.IndexOf(a, "-racerTestSave") < 0 || FindAnyObjectByType<Report105PlayerCheck>()) return;
            Application.runInBackground = true; InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus; // check only
            var g = new GameObject("Report105 player check"); DontDestroyOnLoad(g); g.AddComponent<Report105PlayerCheck>().outDir = a[i + 1];
        }
        static string Arg(string key, string fallback) { var a = Environment.GetCommandLineArgs(); int i = Array.IndexOf(a, key); return i >= 0 && i + 1 < a.Length ? a[i + 1] : fallback; }
        static float F(string s) => float.Parse(s, System.Globalization.CultureInfo.InvariantCulture);
        static string V(Vector3 v) => $"{v.x:F1},{v.y:F2},{v.z:F1}";
        void Note(string s) { rows.Add(s); File.WriteAllLines(Path.Combine(outDir, "p105-check.txt"), rows); Debug.Log("P105CHECK " + s); }
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
        void Conditions(int time = 0)
        {
            var st = flow.Save.Settings; st.timeOfDay = time; st.weather = 0; st.hints = false; st.estimateAiFinishes = false; flow.Save.SaveSettings(); flow.Save.ApplySettings();
        }
        IEnumerator ToFreeRoam(string id)
        {
            flow.Save.Settings.vehicleId = id; flow.Save.Settings.hints = false; flow.Save.Settings.roamWeather = 0; flow.Save.SaveSettings(); Conditions();
            flow.StartFreeRoam(); float t0 = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - t0 < 120 && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != RaceFlow.RoamScene) yield return null;
            Bind(); t0 = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - t0 < 120) { Bind(); if (flow && race && flow.Started && !LoadingScreen.Holding && flow.State == RaceFlow.Stage.Racing) break; yield return null; }
            AudioListener.volume = 0; yield return new WaitForSecondsRealtime(1f);
            WorldLook.Current?.SetClock(WorldLook.Current.Day, 12);
        }
        IEnumerator Vehicle(string id)
        {
            if (race.vehicle.GetComponent<VehicleConfiguration>().profileId == id) yield break;
            flow.Pause(); flow.OpenGarage(); flow.SelectVehicle(id); flow.CloseGarage(); flow.Resume(); yield return new WaitForSecondsRealtime(1);
        }
        static float GroundAt(Vector3 p) { float best = float.NaN; foreach (var h in Physics.RaycastAll(new Vector3(p.x, p.y + 30, p.z), Vector3.down, 80, ~0, QueryTriggerInteraction.Ignore)) { if (h.collider.attachedRigidbody || !h.collider.name.StartsWith("Ground_")) continue; if (float.IsNaN(best) || h.point.y > best) best = h.point.y; } return float.IsNaN(best) ? p.y : best; }
        void Put(ArcadeVehicle car, Vector3 pos, float yaw, float speed)
        {
            var rot = Quaternion.Euler(0, yaw, 0); pos.y = GroundAt(pos) + .6f;
            car.Body.position = pos; car.Body.rotation = rot; car.transform.SetPositionAndRotation(pos, rot); car.Body.linearVelocity = rot * Vector3.forward * speed; car.Body.angularVelocity = Vector3.zero; car.ClearSteering();
            car.GetComponent<VehicleRespawn>()?.CancelRecovery(); Physics.SyncTransforms();
        }
        static Vector3 Flat(Vector3 v) { v.y = 0; return v; }
        static float Yaw(Vector3 d) => Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg;
        void Drive(ArcadeVehicle car, Vector3 target, float cap, float steerGain = 25)
        {
            float ang = Vector3.SignedAngle(Flat(car.transform.forward), Flat(target - car.Body.position), Vector3.up); float speed = car.Body.linearVelocity.magnitude; float steer = Mathf.Clamp(ang / steerGain, -1, 1);
            InputSystem.QueueStateEvent(pad, new GamepadState { leftStick = new Vector2(Mathf.Abs(steer) < .02f ? 0 : steer, 0), rightTrigger = speed < cap ? 1 : 0, leftTrigger = speed > cap + 1.5f ? Mathf.Clamp01((speed - cap) / 5) : 0 });
        }
        void BindPad() { var car = race.vehicle; var vi = car.GetComponent<VehicleInput>(); vi.enabled = true; vi.Bind(pad); InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus; }
        RaceRoad Highway => FindObjectsByType<RaceRoad>(FindObjectsSortMode.None).FirstOrDefault(r => r.openHighway && r.name.Contains("CR113"));

        IEnumerator Start()
        {
            Directory.CreateDirectory(outDir); AudioListener.volume = 0;
            string kind = Arg("-p105kind", "bump");
            Screen.SetResolution(1920, 1080, FullScreenMode.Windowed); yield return null; yield return null;
            pad = InputSystem.AddDevice<Gamepad>("Report105Pad");
            yield return WaitFlow();
            Note($"screen={Screen.width}x{Screen.height} kind={kind} version={Application.version}");
            switch (kind)
            {
                case "bump": yield return Bump(); break;
                case "entrance": yield return Entrance(); break;
                case "trees": yield return Trees(); break;
                case "police": yield return Police(); break;
                case "moon": yield return Moon(); break;
                default: Note("unknown kind " + kind); break;
            }
            File.WriteAllText(Path.Combine(outDir, "done.txt"), "done"); Application.Quit();
        }

        // ---------------------------------------------------------------- Part A: the bump
        // follows Hwy 92 at the lateral offset of the spot, from 120 m before to 40 m after it; vertical speed of the body every physics step
        IEnumerator Pass(string vehicle, RaceRoad road, Vector3 spot, int dir, string label, float cap)
        {
            var car = race.vehicle; float s0 = road.Project(spot, out _); var at = road.At(s0, out var f); var right = Vector3.Cross(Vector3.up, Flat(f).normalized); float off = Vector3.Dot(Flat(spot - at), right);
            Vector3 P(float s) { var q = road.At(s, out var ff); return q + Vector3.Cross(Vector3.up, Flat(ff).normalized) * off; }
            float start = s0 - dir * 120; Put(car, P(start), Yaw(P(start + dir * 5) - P(start)), cap * .8f); FindAnyObjectByType<ChaseCamera>()?.Snap();
            var hit = car.GetComponent<Report105Contacts>() ?? car.gameObject.AddComponent<Report105Contacts>(); hit.who = vehicle; hit.list.Clear();
            var vy = new List<(float d, float vy, float speed)>(); float t0 = Time.time;
            while (Time.time - t0 < 25)
            {
                yield return new WaitForFixedUpdate(); AudioListener.volume = 0;
                float s = road.Project(car.Body.position, out _); float d = (s - s0) * dir; Drive(car, P(s + dir * 14), cap);
                if (d > -15 && d < 15) vy.Add((d, car.Body.linearVelocity.y, car.Body.linearVelocity.magnitude));
                if (d > 40) break;
            }
            InputSystem.QueueStateEvent(pad, new GamepadState());
            if (vy.Count < 3) { Note($"{label} {vehicle}: did not reach the spot"); yield break; }
            float maxStep = 0, at2 = 0; for (int i = 1; i < vy.Count; i++) { float dv = Mathf.Abs(vy[i].vy - vy[i - 1].vy); if (dv > maxStep) { maxStep = dv; at2 = vy[i].d; } }
            var sorted = vy.Select(x => x.vy).OrderBy(x => x).ToList(); float median = sorted[sorted.Count / 2]; float dev = vy.Max(x => Mathf.Abs(x.vy - median));
            Note($"{label} {vehicle} {(dir > 0 ? "eastbound" : "westbound")} at {vy.Average(x => x.speed):F1} m/s ({vy.Average(x => x.speed) * 2.237f:F0} mph): vertical speed over 30 m around the spot: largest change between physics steps {maxStep:F3} m/s (at {at2:+0.0;-0.0} m), largest departure from its median {dev:F3} m/s (median {median:F3}); contacts {hit.list.Count}");
            foreach (var c in hit.list.Take(5)) Note("    " + c);
        }
        IEnumerator Bump()
        {
            yield return ToFreeRoam("mower"); BindPad(); var road = Highway; road.Initialize();
            var spots = new[] { ("Dan's spot BUG-001 (250.3, 557.1)", new Vector3(250.33f, 9.16f, 557.08f)), ("east seam end (161, 549)", new Vector3(161.5f, 9f, 549f)), ("west seam end (-505.6, 538)", new Vector3(-505.6f, 8f, 538.5f)) };
            foreach (var v in new[] { "mower", "original" })
            {
                yield return Vehicle(v); BindPad();
                foreach (var (label, spot) in spots) foreach (int dir in new[] { 1, -1 }) yield return Pass(v, road, spot, dir, label, 26);
            }
        }

        // ---------------------------------------------------------------- Part B: the entrances
        IEnumerator Entrance()
        {
            yield return ToFreeRoam("mower"); var car = race.vehicle;
            foreach (var c in FindObjectsByType<Canvas>(FindObjectsSortMode.None)) c.enabled = false;
            Put(car, new Vector3(-236.68f, 9.07f, 542.31f), 198.06f, 0); FindAnyObjectByType<ChaseCamera>()?.Snap(); yield return new WaitForSeconds(2); car.Body.linearVelocity = Vector3.zero;
            yield return Snap("partB-dan-view-after"); Note($"Dan's BUG-002 view: vehicle at {V(car.Body.position)} heading {car.transform.eulerAngles.y:F0}");
            WorldLook.Current?.SetClock(WorldLook.Current.Day, 21.85f); yield return new WaitForSeconds(1.5f); yield return Snap("partB-dan-view-after-night"); WorldLook.Current?.SetClock(WorldLook.Current.Day, 12);
            var mouths = Arg("-p105mouths", "").Split(';').Where(x => x.Length > 0).Select(x => { var a = x.Split(','); return new Vector3(F(a[0]), 0, F(a[1])); }).ToList();
            car.Body.isKinematic = true; car.transform.position = new Vector3(0, -500, 0);
            var go = new GameObject("top camera"); var cam = go.AddComponent<Camera>(); cam.orthographic = true; cam.orthographicSize = 22; cam.aspect = 1; cam.nearClipPlane = 1; cam.farClipPlane = 300;
            int k = 0;
            foreach (var m in mouths)
            {
                go.transform.SetPositionAndRotation(new Vector3(m.x, GroundAt(m) + 60, m.z), Quaternion.Euler(90, 0, 0));
                var rt = new RenderTexture(900, 900, 24); cam.targetTexture = rt; yield return null; cam.Render();
                var prev = RenderTexture.active; RenderTexture.active = rt; var tex = new Texture2D(900, 900, TextureFormat.RGB24, false); tex.ReadPixels(new Rect(0, 0, 900, 900), 0, 0); tex.Apply(); RenderTexture.active = prev;
                File.WriteAllBytes(Path.Combine(outDir, $"partB-top-entrance-{++k}-{m.x:F0}_{m.z:F0}.png"), tex.EncodeToPNG()); Destroy(tex); cam.targetTexture = null; rt.Release();
                Note($"top-down of the entrance at {m.x:F1},{m.z:F1} (44 m square, north up)");
            }
            Destroy(go); car.Body.isKinematic = false;
            // drive in and out of the first entrance at speed: from the highway lane, through the mouth 30 m into the lot, then back out across it
            if (mouths.Count > 0)
            {
                BindPad(); var road = Highway; road.Initialize(); var m = mouths[0]; float s0 = road.Project(m, out _); var at = road.At(s0, out var f); var nrm = Vector3.Cross(Vector3.up, Flat(f).normalized); float side = Mathf.Sign(Vector3.Dot(Flat(m - at), nrm));
                var inLot = at + nrm * side * 40;
                foreach (var (from, to, label) in new[] { (at - nrm * side * 6, inLot, "in"), (inLot, at - nrm * side * 6, "out") })
                {
                    Put(race.vehicle, from - (to - from).normalized * 12, Yaw(to - from), 15); FindAnyObjectByType<ChaseCamera>()?.Snap();
                    var vy = new List<(float d, float vy, float sp)>(); float t0 = Time.time; var c2 = race.vehicle;
                    while (Time.time - t0 < 10) { yield return new WaitForFixedUpdate(); Drive(c2, to, 16); float d = Vector3.Dot(Flat(c2.Body.position - from), Flat(to - from).normalized); vy.Add((d, c2.Body.linearVelocity.y, c2.Body.linearVelocity.magnitude)); if (d > Vector3.Distance(Flat(from), Flat(to)) - 2) break; }
                    InputSystem.QueueStateEvent(pad, new GamepadState());
                    float maxStep = 0, atD = 0; for (int i = 1; i < vy.Count; i++) { float dv = Mathf.Abs(vy[i].vy - vy[i - 1].vy); if (dv > maxStep) { maxStep = dv; atD = vy[i].d; } }
                    Note($"drive {label} of the entrance at {m.x:F0},{m.z:F0} at {vy.Average(x => x.sp):F1} m/s ({vy.Average(x => x.sp) * 2.237f:F0} mph): largest vertical speed change between physics steps {maxStep:F3} m/s, {atD:F1} m from the start line (the road centre side is 0; the road edge is at about {6 + 8.2f:F0} m), over {vy.Count} steps; top of the steps: {string.Join(", ", vy.Select((x, k) => (k == 0 ? 0 : Mathf.Abs(x.vy - vy[k - 1].vy), x.d)).OrderByDescending(x => x.Item1).Take(4).Select(x => $"{x.Item1:F3} at {x.d:F1} m"))}");
                    yield return new WaitForSeconds(.5f);
                }
            }
        }

        // ---------------------------------------------------------------- Part C: the trail
        IEnumerator Trees()
        {
            yield return ToFreeRoam("mower"); var car = race.vehicle;
            foreach (var c in FindObjectsByType<Canvas>(FindObjectsSortMode.None)) c.enabled = false;
            foreach (var (name, pos, yaw, hour) in new[] { ("BUG-003", new Vector3(-578.59f, 11.52f, -273.13f), 309.6f, 12f), ("BUG-004", new Vector3(-303.76f, 25.81f, -524.46f), 308.5f, 12f) })
            {
                WorldLook.Current?.SetClock(WorldLook.Current.Day, hour);
                Put(car, pos, yaw, 0); FindAnyObjectByType<ChaseCamera>()?.Snap(); yield return new WaitForSeconds(2); car.Body.linearVelocity = Vector3.zero; yield return Snap($"partC-{name}-after"); Note($"{name} view: vehicle at {V(car.Body.position)} heading {car.transform.eulerAngles.y:F0}");
            }
            foreach (var c in FindObjectsByType<Canvas>(FindObjectsSortMode.None)) c.enabled = true;
            var line = CoursePreviewCatalog.Courses.First(x => x.scene == "StreetLoopGreybox").branches[2].points.ToList();
            foreach (var v in new[] { "mower", "original" })
            {
                yield return Vehicle(v); BindPad(); car = race.vehicle;
                foreach (bool back in new[] { false, true })
                {
                    var path = back ? Enumerable.Reverse(line).ToList() : line; int seg = 0;
                    Put(car, path[0], Yaw(path[3] - path[0]), 8); FindAnyObjectByType<ChaseCamera>()?.Snap();
                    var hit = car.GetComponent<Report105Contacts>() ?? car.gameObject.AddComponent<Report105Contacts>(); hit.who = v; hit.list.Clear();
                    int r0 = race.Racers[0].Recoveries; float t0 = Time.time; bool done = false; float vmax = 0;
                    while (Time.time - t0 < 120)
                    {
                        yield return new WaitForFixedUpdate(); AudioListener.volume = 0; var p = car.Body.position;
                        while (seg < path.Count - 1 && Flat(path[seg + 1] - p).magnitude < 9) seg++;
                        float ahead = 0; for (int k = seg + 1; k < Mathf.Min(seg + 6, path.Count - 1); k++) ahead = Mathf.Max(ahead, Vector3.Angle(Flat(path[k] - path[k - 1]), Flat(path[k + 1] - path[k])));
                        Drive(car, path[Mathf.Min(seg + 2, path.Count - 1)], Mathf.Lerp(26, 12, Mathf.InverseLerp(5, 35, ahead)), 20); vmax = Mathf.Max(vmax, car.Body.linearVelocity.magnitude);
                        if (seg >= path.Count - 2 && Flat(path[^1] - p).magnitude < 10) { done = true; break; }
                    }
                    InputSystem.QueueStateEvent(pad, new GamepadState());
                    var trees = hit.list.Where(x => x.IndexOf("trunk", StringComparison.OrdinalIgnoreCase) >= 0 || x.IndexOf("tree", StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                    Note($"Pine Ridge trail {(back ? "west to east" : "east to west")} with the {v}: {(done ? "end to end" : $"stopped at point {seg}/{path.Count} at {V(car.Body.position)}")} in {Time.time - t0:F0} s (top {vmax * 2.237f:F0} mph), resets {race.Racers[0].Recoveries - r0}, tree hits {trees.Count}, other contacts {hit.list.Count - trees.Count}");
                    foreach (var x in hit.list.Take(8)) Note("    " + x);
                }
            }
        }

        // ---------------------------------------------------------------- Part D: hidden police
        IEnumerator Police()
        {
            yield return ToFreeRoam("mower");
            float t0 = Time.realtimeSinceStartup; while (!HiddenPolice.Current && Time.realtimeSinceStartup - t0 < 20) yield return null;
            var hp = HiddenPolice.Current; if (!hp) { Note("no HiddenPolice"); yield break; }
            Note($"Free Roam roads for the limit: through road {(race.throughRoad ? race.throughRoad.name : "none")}, ambient road {(race.ambientRoad ? race.ambientRoad.name : "none")}, main road {(race.road ? race.road.name : "none")}; hiding places {hp.All.Count}; Settings hidden police {(hp.Enabled ? "on" : "off")}, difficulty {hp.Difficulty}");
            hp.enabled = false; foreach (var s in hp.All) if (s.prop) { Destroy(s.prop.gameObject); s.prop = null; }
            yield return null;
            foreach (var s in hp.All) s.prop = GetawayChase.MakePatrolProp(race, s.pos, s.forward);
            yield return new WaitForFixedUpdate(); Physics.SyncTransforms();
            var net = hp.Net; var car = race.vehicle; car.GetComponent<VehicleInput>().enabled = false;
            foreach (var v in new[] { "mower", "original" })
            {
                if (v != "mower") { hp.enabled = false; yield return Vehicle(v); car = race.vehicle; car.GetComponent<VehicleInput>().enabled = false; foreach (var s in hp.All) { if (s.prop) Destroy(s.prop.gameObject); s.prop = GetawayChase.MakePatrolProp(race, s.pos, s.forward); } yield return new WaitForFixedUpdate(); }
                car.Body.isKinematic = true;
                foreach (var s in hp.All)
                {
                    int n = net.Nearest(s.pos, out _, 80); if (n < 0) { Note($"  {s.road} {V(s.pos)}: no road near"); continue; }
                    var road = net.Roads[net.Road[n]]; float sAt = road.Project(s.pos, out _);
                    var lanes = new[] { road.TrafficLane(sAt, 1, true), road.TrafficLane(sAt, 1), road.TrafficLane(sAt, -1, true), road.TrafficLane(sAt, -1) }.Select(x => Mathf.Round(x * 100) / 100).Distinct().ToList();
                    var parts = new List<string>(); var fails = new Dictionary<string, int>();
                    foreach (float lane in lanes) foreach (int facing in new[] { 1, -1 })
                        {
                            int inRange = 0, trips = 0;
                            for (float st = sAt - 90; st <= sAt + 90; st += 6.26f) // 70 mph x 0.2 s
                            {
                                var q = road.At(Mathf.Repeat(st, road.Length), out var f); f = Flat(f).normalized; var pos = q + Vector3.Cross(Vector3.up, f) * lane;
                                var rot = Quaternion.LookRotation(f * facing); pos.y = GroundAt(pos) + .6f; car.Body.position = pos; car.Body.rotation = rot; car.transform.SetPositionAndRotation(pos, rot); Physics.SyncTransforms();
                                var me = car.Body.position; var toCop = me - s.prop.Body.position; toCop.y = 0; if (toCop.magnitude > HiddenPolice.SeeRange) continue; inRange++;
                                int limit = hp.LimitAt(me, out bool onRoad);
                                if (hp.Sees(s, me, 70, limit, onRoad, car.transform, out var why)) trips++; else { var key = why.StartsWith("view blocked by") ? why : why + (why == "not fast enough" ? $" (limit {limit})" : ""); fails[key] = fails.TryGetValue(key, out var c) ? c + 1 : 1; }
                            }
                            parts.Add($"lane {lane:+0.00;-0.00} facing {(facing > 0 ? "+" : "-")}: {trips}/{inRange}");
                        }
                    Note($"  {v} | {s.road} {V(s.pos)} on '{road.name}' (limit {SpeedPatrol.LimitAt(race, road.At(sAt, out _), out _)}): {string.Join("; ", parts)}{(fails.Count > 0 ? " | not seen: " + string.Join(", ", fails.Select(kv => $"{kv.Key} x{kv.Value}")) : "")}");
                }
                car.Body.isKinematic = false;
            }
            foreach (var s in hp.All) if (s.prop) { Destroy(s.prop.gameObject); s.prop = null; }
            // real passes at about 70 mph with the virtual controller: three places, both directions, both lanes
            yield return Vehicle("mower"); car = race.vehicle; BindPad(); hp.enabled = true;
            var buildings = FindAnyObjectByType<Hwy92Roadside>()?.Buildings;
            var hwy = hp.All.Where(s => s.road == "Hwy 92").ToList();
            var byBuilding = buildings != null && hwy.Count > 0 ? hwy.OrderBy(s => buildings.Min(b => Vector2.Distance(new Vector2(b.bounds.center.x, b.bounds.center.z), new Vector2(s.pos.x, s.pos.z)) - b.bounds.extents.magnitude * .5f)).First() : null;
            var picks = new List<(HiddenPolice.Spot s, string why)>();
            if (byBuilding != null) picks.Add((byBuilding, "Hwy 92, beside a 0.104 building"));
            var other92 = hwy.FirstOrDefault(s => s != byBuilding); if (other92 != null) picks.Add((other92, "Hwy 92"));
            var street = hp.All.FirstOrDefault(s => s.road == "Lake road") ?? hp.All.FirstOrDefault(s => s.road != "Hwy 92"); if (street != null) picks.Add((street, street.road));
            int passes = 0, tripped = 0;
            foreach (var (s, why) in picks)
            {
                int n = net.Nearest(s.pos, out _, 80); var road = net.Roads[net.Road[n]]; float sAt = road.Project(s.pos, out _);
                foreach (int dir in new[] { 1, -1 }) foreach (bool inner in new[] { true, false })
                    {
                        float lane = road.TrafficLane(sAt, dir, inner); // the right-hand lanes for this direction of travel (inner and outer)
                        Vector3 P(float st) { var q = road.At(Mathf.Repeat(st, road.Length), out var f); return q + Vector3.Cross(Vector3.up, Flat(f).normalized) * lane; }
                        hp.Active.Clear(); hp.Active.Add(s); foreach (var o in hp.All) if (o != s && o.prop) { Destroy(o.prop.gameObject); o.prop = null; }
                        hp.AttachedAt = Time.time - 1000; hp.LastEnd = -9999; HiddenPolice.FlashAt = -99;
                        float start = sAt - dir * 160; Put(car, P(start), Yaw(P(start + dir * 5) - P(start)), 28); FindAnyObjectByType<ChaseCamera>()?.Snap();
                        float tStart = Time.time; bool trip = false; float closest = 999, speedAt = 0; string line = "";
                        while (Time.time - tStart < 18)
                        {
                            yield return new WaitForFixedUpdate(); AudioListener.volume = 0;
                            float st = road.Project(car.Body.position, out _); Drive(car, P(st + dir * 16), 31.3f);
                            if (s.prop) { float d = Flat(car.Body.position - s.prop.Body.position).magnitude; if (d < closest) { closest = d; speedAt = car.Body.linearVelocity.magnitude; line = hp.DebugLine; } }
                            if (hp.ChaseRunning) { trip = true; break; }
                            if ((st - sAt) * dir > 110) break;
                        }
                        InputSystem.QueueStateEvent(pad, new GamepadState()); passes++; if (trip) tripped++;
                        Note($"pass at {why} {V(s.pos)}: {(dir > 0 ? "forward" : "backward")} along '{road.name}', {(inner ? "inner" : "outer")} lane ({lane:+0.0;-0.0} m), {speedAt * 2.237f:F0} mph at the nearest point ({closest:F0} m): {(trip ? "TRIPPED (" + HiddenPolice.Flash + ")" : "not tripped")} | {line}");
                        if (trip) { var g = GetawayChase.Current; if (g) Destroy(g); yield return null; }
                        yield return new WaitForSeconds(.3f);
                    }
            }
            Note($"real passes: {tripped}/{passes} tripped");
            // a pass inside the quiet period right after a chase ends
            if (picks.Count > 0)
            {
                var (s, why) = picks[0]; int n = net.Nearest(s.pos, out _, 80); var road = net.Roads[net.Road[n]]; float sAt = road.Project(s.pos, out _); float lane = road.TrafficLane(sAt, 1);
                Vector3 P(float st) { var q = road.At(Mathf.Repeat(st, road.Length), out var f); return q + Vector3.Cross(Vector3.up, Flat(f).normalized) * lane; }
                hp.Active.Clear(); hp.Active.Add(s); hp.AttachedAt = Time.time - 1000; HiddenPolice.Ended(false); hp.Active.Clear(); hp.Active.Add(s); // a chase has just ended
                Put(car, P(sAt - 160), Yaw(P(sAt - 155) - P(sAt - 160)), 28); float tStart = Time.time; bool trip = false; string line = ""; float closest = 999;
                while (Time.time - tStart < 18)
                {
                    yield return new WaitForFixedUpdate(); float st = road.Project(car.Body.position, out _); Drive(car, P(st + 16), 31.3f);
                    if (s.prop) { float d = Flat(car.Body.position - s.prop.Body.position).magnitude; if (d < closest) { closest = d; line = hp.DebugLine; } }
                    if (hp.ChaseRunning) { trip = true; break; } if (st - sAt > 110) break;
                }
                InputSystem.QueueStateEvent(pad, new GamepadState());
                Note($"pass inside the quiet period ({HiddenPolice.QuietAfter:F0} s after a chase) at {why}: {(trip ? "TRIPPED" : "not tripped; radio and screen quiet: radio line '" + (Time.unscaledTime - HiddenPolice.LineAt < 30 ? HiddenPolice.Line : "") + "', flash '" + (Time.unscaledTime - HiddenPolice.FlashAt < 30 ? HiddenPolice.Flash : "") + "'")} | {line}");
                if (trip && GetawayChase.Current) Destroy(GetawayChase.Current);
            }
        }

        // ---------------------------------------------------------------- Part E: the moon
        static float Horizon(Vector3 eye, float az, out float dist)
        {
            float best = -5; dist = 0; var dirH = Quaternion.Euler(0, az, 0) * Vector3.forward;
            for (float e = -2; e <= 14; e += .1f)
            {
                var d = Quaternion.AngleAxis(-e, Vector3.Cross(Vector3.up, dirH)) * dirH;
                foreach (var h in Physics.RaycastAll(eye, d, 3000, ~0, QueryTriggerInteraction.Ignore)) { if (h.distance <= 120 || h.collider.attachedRigidbody) continue; var mr = h.collider.GetComponent<MeshRenderer>(); if (!h.collider.name.StartsWith("Ground_") && !(mr && mr.enabled)) continue; best = e; dist = h.distance; break; } // ground and drawn things only (not invisible walls)
            }
            return best;
        }
        // where the moon disc actually is, seen from the camera (elevation, azimuth); NaN when it is not shown
        static (float elev, float az) DiscDir(Camera cam)
        {
            var md = FindObjectsByType<MoonDisc>(FindObjectsSortMode.None).FirstOrDefault(m => m.Visible); if (!md) return (float.NaN, float.NaN);
            var to = md.transform.Find("Moon disc").position - cam.transform.position; return (Mathf.Asin(to.y / to.magnitude) * Mathf.Rad2Deg, Mathf.Repeat(Mathf.Atan2(to.x, to.z) * Mathf.Rad2Deg, 360));
        }
        IEnumerator Moon()
        {
            yield return ToFreeRoam("original"); var car = race.vehicle; var look = WorldLook.Current;
            foreach (var c in FindObjectsByType<Canvas>(FindObjectsSortMode.None)) c.enabled = false;
            // a free camera above the treetops (the trees' crowns have no colliders, so the ridge finder sees only the ground), the vehicle out of the way
            car.Body.isKinematic = true; car.transform.position = new Vector3(0, -400, 0); var chase = FindAnyObjectByType<ChaseCamera>(); if (chase) chase.enabled = false;
            var cam = Camera.main; cam.fieldOfView = 40;
            var roads = FindObjectsByType<RaceRoad>(FindObjectsSortMode.None).Where(r => r.points != null && r.points.Length > 3).ToList();
            float disc = 2.58f; // the disc is 2.6 degrees wide (0.100)
            foreach (var (label, setting, day, from0, to) in new[] { ("moonset", true, 13, 0.5f, 5f), ("moonrise", false, 24, 20.5f, 30f) })
            {
                // the moon's path through the night (its direction does not depend on where the camera is), then a place above the trees whose ridge it
                // sinks behind (rises from behind) within an hour and a half, before dawn (after dusk)
                float from = from0; var traj = new List<(float hh, float e, float a)>();
                for (float hh = from; hh < to; hh += .1f) { look.SetClock(hh >= 24 ? day + 1 : day, Mathf.Repeat(hh, 24)); yield return null; var (e0, a0) = DiscDir(cam); if (!float.IsNaN(e0)) traj.Add((hh, e0, a0)); }
                Note($"{label}: the moon's path on day {day} {from0:F1}-{to:F1} h: {traj.Count} samples" + (traj.Count > 0 ? $", from {traj[0].e:F1} deg up at {traj[0].a:F0} to {traj[^1].e:F1} deg up at {traj[^1].a:F0}" : ""));
                if (!setting) { traj = traj.Where(t => t.hh < from0 + 3).ToList(); traj.Reverse(); } // the rise (its first 3 h) read backwards is a set
                var rej = new Dictionary<string, int>(); Vector3 eye = default; float bestSpan = 99, startHour = from, azWanted = 0;
                foreach (var r in roads) { r.Initialize(); for (float s = 0; s < r.Length; s += 40)
                {
                    foreach (float up in setting ? new[] { 26f } : new[] { 26f, 16f }) { var p = r.At(s, out _) + Vector3.up * up; int i1 = -1, i3 = -1;
                    string why = "never low"; for (int n = 0; n < traj.Count; n++) { if (traj[n].e < -1.5f) { why = "below the horizon before hidden"; break; } float g = traj[n].e - Horizon(p, traj[n].a, out var dd); if (i1 < 0 && g > disc * .6f) continue; if (dd < (setting ? 400 : 100)) { why = "ridge too near"; break; } if (i1 < 0) { if (n == 0) { why = "hidden from the start"; break; } i1 = n - 1; } if (g < -disc * .75f) { i3 = n; break; } } rej[why] = rej.TryGetValue(why, out var rc) ? rc + 1 : 1;
                    if (i1 < 0 || i3 < 0) continue; float span = Mathf.Abs(traj[i3].hh - traj[i1].hh); if (span < bestSpan) { bestSpan = span; eye = p; startHour = traj[Mathf.Max(0, i1 - 1)].hh; azWanted = traj[(i1 + i3) / 2].a; } }
                } }
                if (bestSpan > 2f) { Note($"{label}: no place found where the moon goes behind a ridge within 2 h; " + string.Join(", ", rej.Select(kv => kv.Key + " " + kv.Value))); continue; }
                from = setting ? startHour : Mathf.Max(from0, startHour - bestSpan - .3f);
                float ridgeFace = Horizon(eye, azWanted, out var rdist); cam.transform.SetPositionAndRotation(eye, Quaternion.Euler(-ridgeFace + 1, azWanted, 0));
                Note($"{label}: free camera at {V(eye)} (above the trees), looking {azWanted:F0}° and {ridgeFace - 1:F1}° up, field of view 40°, far clip {cam.farClipPlane:F0} m; the ridge that way is {ridgeFace:F1}° up, {rdist:F0} m off; the moon crosses it in about {bestSpan * 60:F0} minutes");
                // step the clock; the three moments: the disc's centre half a disc above the ridge, level with it, 0.7 of a disc below it
                var wanted = new List<(string moment, float offset)> { ("touching", disc * .5f), ("half hidden", 0f), ("gone", -disc * .7f) }; if (!setting) wanted.Reverse();
                float prevGap = float.NaN; int k = 0;
                for (float hh = from; hh < to && k < wanted.Count; hh += .004f)
                {
                    look.SetClock(hh >= 24 ? day + 1 : day, Mathf.Repeat(hh, 24)); cam.transform.SetPositionAndRotation(eye, Quaternion.Euler(-ridgeFace + 1, azWanted, 0)); yield return null;
                    var (e, a) = DiscDir(cam); if (float.IsNaN(e)) { prevGap = float.NaN; continue; }
                    float rAt = Horizon(eye, a, out var rDist); float gap = e - rAt - wanted[k].offset; if (Mathf.Repeat(hh, .1f) < .004f) File.AppendAllText(Path.Combine(outDir, "moon-trace.txt"), $"{label} {hh:F3} e {e:F2} az {a:F1} ridge {rAt:F2} gap {gap:F2} k {k}" + Environment.NewLine);
                    if (!float.IsNaN(prevGap) && (setting ? prevGap >= 0 && gap < 0 : prevGap <= 0 && gap > 0))
                    {
                        var vp = cam.WorldToViewportPoint(FindObjectsByType<MoonDisc>(FindObjectsSortMode.None).First(m => m.Visible).transform.Find("Moon disc").position);
                        Note($"  {wanted[k].moment}: day {look.Day} {look.Clock}: moon disc centre {e:F1}° up at {a:F0}°, the ridge there {rAt:F1}° up ({rDist:F0} m off; centre {e - rAt:+0.0;-0.0}° from it, the disc {disc:F1}° wide); screen ({vp.x:F2},{vp.y:F2})");
                        yield return Snap($"partE-{label}-{wanted[k].moment.Replace(' ', '-')}"); k++; prevGap = float.NaN; continue;
                    }
                    prevGap = gap;
                }
            }
            if (chase) chase.enabled = true; car.Body.isKinematic = false;
        }
    }
}
