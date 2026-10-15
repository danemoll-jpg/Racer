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
    // 0.106 evidence only (like Report105PlayerCheck): `-p106check <dir> -racerTestSave <dir>` in the built player. Never active without both arguments.
    //   -p106kind views     Free Roam: driver's views of the stunt park from Hwy 92 and its drive, views inside it, a top-down shot with the 0.104 plot outline
    //   -p106kind features  Free Roam: every feature driven by a virtual controller with -p106vehicle (stuntbuggy / stuntbike / original ...): the line,
    //                       the speed, every flight (air time, distance, rotation), the stunt line and points, the loop, bails; and set tricks
    //   -p106kind event     the Stunt event (-p106players 1|2), a bot driving the park's circuit for the whole 10 minutes, a pause and resume in the middle;
    //                       the clock, the points, the results, the medals and the Top 10 (test save)
    //   -p106kind menus     the Stunt event's setup page with the controller only (every row reachable, B back), the Records' STUNTS tab
    //   -p106kind perf      worst-view frame time at the park (full screen and split-screen), -p106off: the world without the park (before)
    //   -p106kind lap       a Street Loop lap (-p106course 0 Forward / 1 Reverse) with rivals and traffic, the player's car driven by the road AI: resets, touches of the park
    public sealed class Report106Contacts : MonoBehaviour
    {
        public readonly List<string> list = new(); public string who;
        void OnCollisionEnter(Collision c)
        {
            if (c.collider.name.StartsWith("Ground_") || c.collider.name == "Stunt park floor") return;
            list.Add($"{who} touched '{c.collider.name}' at {c.GetContact(0).point:F1}, impulse {c.impulse.magnitude:F0}");
        }
    }
    public sealed partial class Report106PlayerCheck : MonoBehaviour
    {
        string outDir; readonly List<string> rows = new();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            var a = Environment.GetCommandLineArgs(); int i = Array.IndexOf(a, "-p106check");
            if (i < 0 || i + 1 >= a.Length || Array.IndexOf(a, "-racerTestSave") < 0 || FindAnyObjectByType<Report106PlayerCheck>()) return;
            Application.runInBackground = true; InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus; // check only
            if (Array.IndexOf(a, "-p106off") >= 0) StuntPark.Disabled = true;
            if (Array.IndexOf(a, "-p113off") >= 0) Hwy92Roadside.TrickumOff = true; // 0.113 Part C: the "before" (checks only)
            var g = new GameObject("Report106 player check"); DontDestroyOnLoad(g); g.AddComponent<Report106PlayerCheck>().outDir = a[i + 1];
        }
        static string Arg(string key, string fallback) { var a = Environment.GetCommandLineArgs(); int i = Array.IndexOf(a, key); return i >= 0 && i + 1 < a.Length ? a[i + 1] : fallback; }
        static float F(string s) => float.Parse(s, System.Globalization.CultureInfo.InvariantCulture);
        static string V(Vector3 v) => $"{v.x:F1},{v.y:F2},{v.z:F1}";
        void Note(string s) { rows.Add(s); File.WriteAllLines(Path.Combine(outDir, "p106-check.txt"), rows); Debug.Log("P106CHECK " + s); }
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
        static Vector3 Flat(Vector3 v) { v.y = 0; return v; }
        static float Yaw(Vector3 d) => Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg;
        static float TopAt(Vector3 p)
        {
            float best = float.NaN;
            foreach (var h in Physics.RaycastAll(new Vector3(p.x, p.y + 30, p.z), Vector3.down, 80, ~0, QueryTriggerInteraction.Ignore))
            { if (h.collider.attachedRigidbody || h.collider.isTrigger) continue; if (float.IsNaN(best) || h.point.y > best) best = h.point.y; }
            return float.IsNaN(best) ? p.y : best;
        }
        void Put(ArcadeVehicle car, Vector3 pos, float yaw, float speed)
        {
            var rot = Quaternion.Euler(0, yaw, 0); pos.y = TopAt(pos) + .7f;
            car.Body.position = pos; car.Body.rotation = rot; car.transform.SetPositionAndRotation(pos, rot); car.Body.linearVelocity = rot * Vector3.forward * speed; car.Body.angularVelocity = Vector3.zero; car.ClearSteering();
            car.GetComponent<VehicleRespawn>()?.CancelRecovery(); Physics.SyncTransforms();
        }
        void BindPad() { var car = race.vehicle; var vi = car.GetComponent<VehicleInput>(); vi.enabled = true; vi.Bind(pad); InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus; }
        // the pad: steering toward a target at a speed cap, plus air inputs (tip: + forward / - back; spin: + right)
        void Drive(ArcadeVehicle car, Vector3 target, float cap, float tip = 0, float roll = 0, float spin = 0, float steerGain = 25)
        {
            bool air = car.GroundedWheels == 0;
            float ang = Vector3.SignedAngle(Flat(car.transform.forward), Flat(target - car.Body.position), Vector3.up); float speed = Vector3.Dot(car.Body.linearVelocity, car.transform.forward); float steer = air ? roll : Mathf.Clamp(ang / steerGain, -1, 1);
            InputSystem.QueueStateEvent(pad, new GamepadState { leftStick = new Vector2(Mathf.Abs(steer) < .02f ? 0 : steer, air ? tip : 0), rightStick = new Vector2(air ? spin : 0, 0), rightTrigger = speed < cap ? 1 : 0, leftTrigger = speed > cap + 1.5f ? Mathf.Clamp01((speed - cap) / 5) : 0 });
        }
        void Release() => InputSystem.QueueStateEvent(pad, new GamepadState());

        IEnumerator Start()
        {
            Directory.CreateDirectory(outDir); AudioListener.volume = 0;
            string kind = Arg("-p106kind", "views");
            Screen.SetResolution(int.Parse(Arg("-p106w", "1920")), int.Parse(Arg("-p106h", "1080")), Arg("-p106full", "0") == "1" ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed); yield return null; yield return null;
            pad = InputSystem.AddDevice<Gamepad>("Report106Pad");
            yield return WaitFlow();
            Note($"screen={Screen.width}x{Screen.height} kind={kind} version={Application.version} park={(StuntPark.Disabled ? "off" : "on")}");
            switch (kind)
            {
                case "views": yield return Views(); break;
                case "features": yield return Features(); break;
                case "event": yield return Event(); break;
                case "menus": yield return Menus(); break;
                case "perf": yield return Perf(); break;
                case "lap": yield return Lap(); break;
                case "landings": yield return Landings(); break; // 0.107 (Report106PlayerCheck.Round107.cs)
                case "event107": yield return Event107(); break;
                case "overtime": yield return Overtime(); break; // 0.108 (Report106PlayerCheck.Round108.cs)
                case "unlocks": yield return Unlocks(); break;
                case "combos": yield return Combos(); break;
                case "mainmenu": yield return MainMenuStunt(); break; // 0.109 (Report106PlayerCheck.Round109.cs)
                case "results": yield return ResultsTop10(); break;
                case "balance": yield return BalanceLogCheck(); break;
                case "stunt110": yield return Stunt110(); break; // 0.110 (Report106PlayerCheck.Round110.cs)
                case "race110": yield return Race110(); break;
                case "vehicle110": yield return VehicleMemory(); break;
                case "vehicle110b": yield return VehicleMemoryAfterRestart(); break;
                case "fresh110": yield return Fresh110(); break;
                case "stats111": yield return Stats111(); break; // 0.111 (Report106PlayerCheck.Round111.cs)
                case "smash111": yield return Smash111(); break;
                case "prize111": yield return Prize111(); break;
                case "cups111": yield return Cups111(); break;
                case "pace112": yield return Pace112(); break; // 0.112 (Report106PlayerCheck.Round112.cs)
                case "jump112": yield return Jump112(); break;
                case "shots112": yield return ToStreetLoop(); yield return Shots112(); break;
                case "shop112": yield return Shop112(); break;
                case "race112": yield return Race112(); break;
                case "ai113": yield return Ai113(); break; // 0.113 (Report106PlayerCheck.Round113.cs)
                case "shots113": yield return Shots113(); break;
                case "perf113": yield return Perf113(); break;
                case "audio113": yield return Audio113(); break;
                case "retry113": yield return Retry113(); break;
                case "shots114a": yield return Shots114a(); break; // 0.114 (Report106PlayerCheck.Round114.cs)
                case "lev115": yield return Lev115(); break; // 0.115 (Report106PlayerCheck.Round115.cs)
                case "drive115": yield return Drive115(); break;
                case "split115": yield return Split115(); break;
                case "turns116": yield return Turns116(); break; // 0.116 (Report106PlayerCheck.Round116.cs)
                case "short116": yield return Short116(); break;
                case "debug116": yield return Debug116(); break;
                case "audit117": yield return Audit117(); break; // 0.117 (Report106PlayerCheck.Round117.cs)
                case "lap117": yield return Lap117(); break;
                case "reset117": yield return Reset117(); break;
                case "skip117": yield return Skip117(); break;
                case "song117": yield return Song117(); break;
                default: Note("unknown kind " + kind); break;
            }
            File.WriteAllText(Path.Combine(outDir, "done.txt"), "done"); Application.Quit();
        }

        // ---------------------------------------------------------------- Part E 1: views
        IEnumerator Free(string name, Vector3 eye, Vector3 look, float fov = 60, int w = 1920, int h = 1080, bool ortho = false, float size = 100)
        {
            var go = new GameObject("free camera"); var cam = go.AddComponent<Camera>(); cam.CopyFrom(Camera.main); cam.fieldOfView = fov; cam.nearClipPlane = .3f; cam.farClipPlane = 4000;
            cam.orthographic = ortho; cam.orthographicSize = size;
            go.transform.position = eye; go.transform.rotation = ortho ? Quaternion.Euler(90, 0, 0) : Quaternion.LookRotation(look - eye);
            var rt = new RenderTexture(w, h, 24); cam.targetTexture = rt; cam.aspect = (float)w / h; yield return null; cam.Render();
            var prev = RenderTexture.active; RenderTexture.active = rt; var tex = new Texture2D(w, h, TextureFormat.RGB24, false); tex.ReadPixels(new Rect(0, 0, w, h), 0, 0); tex.Apply(); RenderTexture.active = prev;
            File.WriteAllBytes(Path.Combine(outDir, name + ".png"), tex.EncodeToPNG()); Destroy(tex); cam.targetTexture = null; rt.Release(); Destroy(go);
        }
        static Vector3 P(float u, float v, float y = 0) => StuntPark.P(u, v, y);
        IEnumerator Views()
        {
            yield return ToFreeRoam(Arg("-p106vehicle", "original")); var car = race.vehicle;
            if (StuntPark.Current) Note("park: " + string.Join("; ", StuntPark.Current.Report) + "; cleared: " + string.Join("; ", StuntPark.Cleared.Skip(Math.Max(0, StuntPark.Cleared.Count - 1))));
            foreach (var c in FindObjectsByType<Canvas>(FindObjectsSortMode.None)) c.enabled = false;
            // driver's views: on Hwy 92 eastbound before the drive, westbound after it, at the drive's mouth looking up it
            foreach (var (name, pos, yaw) in new[] { ("E1-hwy92-eastbound", new Vector3(60, 9, 537), 80f), ("E1-hwy92-westbound", new Vector3(190, 9, 552), 265f), ("E1-drive-mouth", new Vector3(120, 9, 551), 0f), ("E1-drive-top", new Vector3(120, 9, 598), 0f) })
            {
                Put(car, pos, yaw, 0); FindAnyObjectByType<ChaseCamera>()?.Snap(); yield return new WaitForSeconds(2); car.Body.linearVelocity = Vector3.zero; yield return Snap(name); Note($"{name}: vehicle at {V(car.Body.position)} heading {car.transform.eulerAngles.y:F0}");
            }
            // inside: the start line, each line, from a few metres up (a person's eye height on a vehicle)
            foreach (var (name, eye, look) in new[] {
                ("E1-start-line", P(156, 20, 2.5f), P(100, 20, 1)), ("E1-kicker-line", P(125, 20, 2.5f), P(40, 20, 1)), ("E1-car-gap", P(22, 36, 2.5f), P(22, 85, 1)),
                ("E1-deck-and-loop", P(25, 138, 3f), P(115, 138, 4)), ("E1-loop", P(95, 128, 3f), P(112, 136, 5)), ("E1-half-pipe", P(146.5f, 132, 3f), P(146.5f, 40, 1)),
                ("E1-huge-table", P(22, 46, 3f), P(90, 46, 3)), ("E1-barrel-roll", P(30, 70, 3f), P(80, 74, 2)), ("E1-bus-gap", P(28, 103, 3f), P(80, 100, 2)), ("E1-overview-south-west", P(-20, -15, 25), P(70, 80, 0)), ("E1-overview-north-east", P(175, 175, 30), P(70, 70, 0)) })
            { yield return Free(name, eye, look, 62); Note($"{name}: camera at {V(eye)} looking at {V(look)}"); }
            // top-down with the 0.104 plot outline (yellow) and the park's floor outline (white)
            var plot = new[] { new Vector2(-52.1f, 605.3f), new Vector2(-57.4f, 611.4f), new Vector2(-62.5f, 630.6f), new Vector2(-60.9f, 636.1f), new Vector2(-18.2f, 802.8f), new Vector2(22.2f, 848.0f), new Vector2(312.1f, 848.0f), new Vector2(321.0f, 638.7f), new Vector2(170.8f, 644.4f), new Vector2(169.5f, 570.6f), new Vector2(27.5f, 567.3f) };
            var lines = new List<GameObject>();
            LineRenderer Outline(IEnumerable<Vector3> pts, Color c, float width)
            {
                var g = new GameObject("outline"); lines.Add(g); var lr = g.AddComponent<LineRenderer>(); var p = pts.ToArray(); lr.positionCount = p.Length; lr.SetPositions(p); lr.loop = true; lr.widthMultiplier = width;
                lr.material = new Material(Shader.Find("Universal Render Pipeline/Unlit")) { color = c }; lr.alignment = LineAlignment.TransformZ; g.transform.rotation = Quaternion.Euler(90, 0, 0); return lr;
            }
            Outline(plot.Select(q => new Vector3(q.x, 40, q.y)), new Color(1, .85f, .1f), 2.5f);
            Outline(StuntPark.Outline.Select(q => new Vector3(q.x, 40, q.z)), Color.white, 1.2f);
            yield return Free("E1-top-down-with-plot", new Vector3(130, 300, 700), new Vector3(130, 0, 700), 60, 1600, 1600, true, 175);
            foreach (var g in lines) Destroy(g);
            yield return Free("E1-top-down-park", new Vector3(70, 300, 683), new Vector3(70, 0, 683), 60, 1600, 1600, true, 90);
            // the two stunt vehicles on the start line (the chase camera, and a close side view)
            Campaign.Testing = true; flow.Save.Settings.unlockEverything = true; flow.Save.SaveSettings();
            foreach (var v in new[] { "stuntbuggy", "stuntbike" })
            {
                yield return Vehicle(v); car = race.vehicle; foreach (var c in FindObjectsByType<Canvas>(FindObjectsSortMode.None)) c.enabled = false;
                Put(car, P(140, 30), 270, 0); FindAnyObjectByType<ChaseCamera>()?.Snap(); yield return new WaitForSeconds(2.5f); car.Body.linearVelocity = Vector3.zero;
                yield return Snap("E1-vehicle-" + v + "-chase"); var at = car.Body.position;
                yield return Free("E1-vehicle-" + v + "-side", at + car.transform.right * -4.2f + car.transform.forward * 2.2f + Vector3.up * .9f, at + Vector3.up * .25f, 42, 1600, 1000);
                Note($"{VehicleProfile.Find(v).Name}: config {car.GetComponent<VehicleConfiguration>().profileId}, resting at {V(car.Body.position)} ({car.Body.position.y - StuntPark.FloorY:F2} m above the floor), grounded wheels {car.GroundedWheels}");
            }
            foreach (var c in FindObjectsByType<Canvas>(FindObjectsSortMode.None)) c.enabled = true;
        }
    }
}
