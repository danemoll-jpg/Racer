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
    // 0.100 evidence only (like Report099PlayerCheck): `-p100check <dir> -racerTestSave <dir>` in the built player. Never active without both arguments.
    //   -p100kind views     -p100scene MountainLoop -p100views "name,x,y,z,yaw,pitch;name,@station,yawOffset,pitch;..." -p100time -p100weather  [-p100car x,y,z,heading]
    //   -p100kind lap       one lap of -p100scene by the road AI in the player's vehicle (recoveries, time)
    //   -p100kind previews  every menu with a vehicle preview, a car and a bike: on-screen picture size vs its texture (squish), shots
    //   -p100kind moon      the moon in a night race (Night / Clear) and in Free Roam (full and crescent nights, new moon), from the driving view
    //   -p100kind calendar  the Free Roam day a new save starts on
    //   -p100kind hidden    two hiding places, each passed over the limit in both directions
    //   -p100kind freelook  the right stick / R3 / right mouse button free look in chase and first person, then split-screen (virtual pads)
    //   -p100kind drain     Free Roam storm drain: onto and across the top from three sides, then through the tunnel both ways (-p100vehicle)
    public sealed partial class Report100PlayerCheck : MonoBehaviour
    {
        string outDir; readonly List<string> rows = new();
        const BindingFlags NonPublic = BindingFlags.NonPublic | BindingFlags.Instance;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            var a = Environment.GetCommandLineArgs(); int i = Array.IndexOf(a, "-p100check");
            if (i < 0 || i + 1 >= a.Length || Array.IndexOf(a, "-racerTestSave") < 0 || FindAnyObjectByType<Report100PlayerCheck>()) return;
            var g = new GameObject("Report100 player check"); DontDestroyOnLoad(g); g.AddComponent<Report100PlayerCheck>().outDir = a[i + 1];
        }
        static string Arg(string key, string fallback) { var a = Environment.GetCommandLineArgs(); int i = Array.IndexOf(a, key); return i >= 0 && i + 1 < a.Length ? a[i + 1] : fallback; }
        static float F(string s) => float.Parse(s, System.Globalization.CultureInfo.InvariantCulture);
        static string V(Vector3 v) => $"{v.x:F1},{v.y:F2},{v.z:F1}";
        void Note(string s) { rows.Add(s); File.WriteAllLines(Path.Combine(outDir, "p100-check.txt"), rows); Debug.Log("P100CHECK " + s); }
        IEnumerator Snap(string name) { yield return new WaitForEndOfFrame(); ScreenCapture.CaptureScreenshot(Path.Combine(outDir, name + ".png")); yield return null; yield return null; yield return new WaitForSecondsRealtime(.3f); }
        RaceFlow flow; RaceDirector race;
        RaceMenus Menus => (RaceMenus)typeof(RaceFlow).GetField("menus", NonPublic).GetValue(flow);
        void Bind() { flow = FindAnyObjectByType<RaceFlow>(); race = flow ? flow.Race : null; }
        IEnumerator WaitFlow(float limit = 120)
        {
            float t0 = Time.realtimeSinceStartup;
            while (true) { Bind(); if (flow && flow.Started && !LoadingScreen.Holding && flow.Save != null) break; yield return null; if (Time.realtimeSinceStartup - t0 > limit) break; }
            var title = FindAnyObjectByType<StartupTitle>(); if (title) { typeof(StartupTitle).GetField("completed", BindingFlags.Static | BindingFlags.NonPublic)?.SetValue(null, true); Destroy(title.gameObject); }
            if (flow.State == RaceFlow.Stage.Title) flow.EnterMenuAfterTitle();
            yield return null;
        }
        IEnumerator GoMountain(bool reverse)
        {
            string want = reverse ? "MountainLoopReverse" : "MountainLoop";
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != want)
            {
                Campaign.Testing = true; flow.OpenCourses(); yield return null; flow.SelectMountain(reverse);
                float t0 = Time.realtimeSinceStartup; yield return null;
                while (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != want && Time.realtimeSinceStartup - t0 < 120) yield return null;
                yield return WaitFlow();
            }
        }
        void Conditions(int time, int weather)
        {
            var st = flow.Save.Settings; st.timeOfDay = time; st.weather = weather; st.hints = false; st.vsync = true; st.estimateAiFinishes = false; flow.Save.SaveSettings(); flow.Save.ApplySettings();
        }
        IEnumerator StartRace(string player, string[] roster, int laps)
        {
            flow.OpenGarage(); flow.SelectVehicle(player); flow.CloseGarage();
            race.opponents = roster != null && roster.Length > 0; race.traffic = false; race.laps = laps; race.difficulty = 1; if (roster != null && roster.Length > 0) race.opponentRoster = roster;
            flow.StartRace(); float t0 = Time.realtimeSinceStartup;
            while (flow.State != RaceFlow.Stage.Racing && Time.realtimeSinceStartup - t0 < 60) { AudioListener.volume = 0; yield return null; }
        }
        IEnumerator ToFreeRoam(string vehicle)
        {
            flow.Save.Settings.vehicleId = vehicle; flow.Save.Settings.hints = false; flow.Save.SaveSettings(); Conditions(0, 0);
            flow.StartFreeRoam(); float t0 = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - t0 < 120 && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != RaceFlow.RoamScene) yield return null;
            Bind(); t0 = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - t0 < 120) { Bind(); if (flow && race && flow.Started && !LoadingScreen.Holding && flow.State == RaceFlow.Stage.Racing) break; yield return null; }
            AudioListener.volume = 0; yield return new WaitForSecondsRealtime(1f);
        }
        void HideHud(bool hide) { foreach (var c in FindObjectsByType<Canvas>(FindObjectsSortMode.None)) c.enabled = !hide; }
        static Vector3 Ground(Vector3 p, float lift = 0)
        {
            float best = float.NaN; foreach (var h in Physics.RaycastAll(p + Vector3.up * 60, Vector3.down, 140, ~0, QueryTriggerInteraction.Ignore)) { if (h.collider.attachedRigidbody) continue; if (float.IsNaN(best) || h.point.y > best) best = h.point.y; }
            if (!float.IsNaN(best)) p.y = best; return p + Vector3.up * lift;
        }
        void Put(Vector3 pos, float yaw, float speed = 0)
        {
            var car = race.vehicle; var at = Ground(pos, 1.1f); var rot = Quaternion.Euler(0, yaw, 0);
            car.Body.position = at; car.Body.rotation = rot; car.transform.SetPositionAndRotation(at, rot); car.Body.linearVelocity = rot * Vector3.forward * speed; car.Body.angularVelocity = Vector3.zero; car.ClearSteering();
        }

        IEnumerator Start()
        {
            Directory.CreateDirectory(outDir); AudioListener.volume = 0;
            string kind = Arg("-p100kind", "views");
            int w = int.Parse(Arg("-p100w", "1920")), h = int.Parse(Arg("-p100h", "1080")); Screen.SetResolution(w, h, FullScreenMode.Windowed); yield return null; yield return null;
            Note($"screen={Screen.width}x{Screen.height} gpu={SystemInfo.graphicsDeviceName} kind={kind} version={Application.version}");
            yield return WaitFlow();
            switch (kind)
            {
                case "views": yield return Views(); break;
                case "lap": yield return Lap(); break;
                case "previews": yield return Previews(); break;
                case "moon": yield return Moon(); break;
                case "calendar": yield return Calendar(); break;
                case "hidden": yield return Hidden(); break;
                case "freelook": yield return FreeLook(); break;
                case "drain": yield return Drain(); break;
                case "controls": yield return Controls(); break;
                default: Note("unknown kind " + kind); break;
            }
            File.WriteAllText(Path.Combine(outDir, "done.txt"), "done"); Application.Quit();
        }

        // ---------- Part A ----------
        IEnumerator Views()
        {
            string scene = Arg("-p100scene", "MountainLoop"); yield return GoMountain(scene.EndsWith("Reverse"));
            Conditions(int.Parse(Arg("-p100time", "0")), int.Parse(Arg("-p100weather", "0")));
            yield return StartRace(Arg("-p100vehicle", "original"), null, 1);
            yield return new WaitForSecondsRealtime(2);
            var cam = Camera.main; var chase = FindAnyObjectByType<ChaseCamera>(); if (chase) chase.enabled = false; var cv = CameraViews.Current; if (cv) cv.enabled = false;
            var carSpec = Arg("-p100car", ""); if (carSpec.Length > 0) { var c = carSpec.Split(','); Put(new Vector3(F(c[0]), F(c[1]), F(c[2])), F(c[3])); }
            race.vehicle.Body.isKinematic = true; HideHud(true);
            var road = race.road; road.Initialize();
            foreach (var spec in Arg("-p100views", "").Split(';').Where(x => x.Length > 0))
            {
                var f = spec.Split(','); Vector3 p; float yaw, pitch;
                if (f[1].StartsWith("@")) { float s = F(f[1].Substring(1)); var c = road.At(s, out var fw); float head = Mathf.Atan2(fw.x, fw.z) * Mathf.Rad2Deg; yaw = head + F(f[2]); pitch = F(f[3]); var dir = Quaternion.Euler(0, yaw, 0) * Vector3.forward; p = c + Vector3.up * 3.6f - dir * 7.5f; }
                else { p = new Vector3(F(f[1]), F(f[2]), F(f[3])); yaw = F(f[4]); pitch = F(f[5]); }
                cam.transform.SetPositionAndRotation(p, Quaternion.Euler(pitch, yaw, 0)); yield return null; yield return null; yield return new WaitForSecondsRealtime(.5f);
                yield return Snap($"view-{scene}-{f[0]}"); Note($"view {f[0]} eye {p.x:F2},{p.y:F2},{p.z:F2} yaw {yaw:F1} pitch {pitch:F1}");
            }
        }
        IEnumerator Lap()
        {
            string scene = Arg("-p100scene", "MountainLoop"); yield return GoMountain(scene.EndsWith("Reverse")); Conditions(0, 0);
            yield return StartRace(Arg("-p100vehicle", "original"), null, 1);
            var car = race.vehicle; var pilot = car.gameObject.AddComponent<RoadDriver>(); pilot.Initialize(race, car, true, 1, 1); pilot.Racer = race.Racers[0]; car.GetComponent<VehicleInput>().enabled = false;
            Time.timeScale = float.Parse(Arg("-p100scale", "3")); float t0 = Time.time, real0 = Time.realtimeSinceStartup; var road = race.road; float lowestUp = 1; int passed = 0; float lastS = -1;
            while (flow.State == RaceFlow.Stage.Racing && Time.realtimeSinceStartup - real0 < 900)
            {
                AudioListener.volume = 0; yield return new WaitForFixedUpdate(); lowestUp = Mathf.Min(lowestUp, car.transform.up.y);
                float s = road.Project(car.Body.position, out _); if (lastS >= 0 && s >= 1600 && lastS < 1600 || lastS >= 0 && s >= 1850 && lastS < 1850) { passed++; Note($"passed main {(s < 1700 ? 1600 : 1850)} m at {Time.time - t0:F0} s, speed {car.Body.linearVelocity.magnitude:F1} m/s, recoveries {race.Racers[0].Recoveries}"); }
                lastS = s;
            }
            Time.timeScale = 1;
            Note($"{scene} lap by the road AI ({car.GetComponent<VehicleConfiguration>().profileId}): state {flow.State}, game time {Time.time - t0:F0} s, recoveries {race.Racers[0].Recoveries}, lowest up-vector {lowestUp:F2}, finished {flow.State == RaceFlow.Stage.Results}");
        }
    }
}
