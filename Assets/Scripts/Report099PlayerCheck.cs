using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Racer
{
    // 0.99 evidence only (like GetawayPlayerCheck): `-p99check <dir> -racerTestSave <dir>` in the built player. Never active without both arguments.
    //   -p99kind views    -p99scene MountainLoop  -p99views "name,x,y,z,yaw,pitch;..."  -p99time 0|1|2  -p99weather 0|1|2   shots of the scene from those eye points (HUD and chase camera off)
    //   -p99kind aijump   -p99scene MountainLoop|MountainLoopReverse  -p99rivals original,fastback,pebble,tourer  -p99time  -p99weather  -p99laps 1  -p99scale 3
    //                     a normal race with AI rivals; every rival's speed at the lip of the Mountain Loop summit flight and where it landed
    // The same component carries the later checks (hidden police, radio, unlocks) under other -p99kind values.
    public sealed partial class Report099PlayerCheck : MonoBehaviour
    {
        string outDir; readonly List<string> rows = new();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            var a = Environment.GetCommandLineArgs(); int i = Array.IndexOf(a, "-p99check");
            if (i < 0 || i + 1 >= a.Length || Array.IndexOf(a, "-racerTestSave") < 0 || FindAnyObjectByType<Report099PlayerCheck>()) return;
            var g = new GameObject("Report099 player check"); DontDestroyOnLoad(g); g.AddComponent<Report099PlayerCheck>().outDir = a[i + 1];
        }
        static string Arg(string key, string fallback) { var a = Environment.GetCommandLineArgs(); int i = Array.IndexOf(a, key); return i >= 0 && i + 1 < a.Length ? a[i + 1] : fallback; }
        static float F(string s) => float.Parse(s, System.Globalization.CultureInfo.InvariantCulture);
        void Note(string s) { rows.Add(s); File.WriteAllLines(Path.Combine(outDir, "p99-check.txt"), rows); Debug.Log("P99CHECK " + s); }
        IEnumerator Snap(string name) { ScreenCapture.CaptureScreenshot(Path.Combine(outDir, name + ".png")); yield return null; yield return null; yield return new WaitForSecondsRealtime(.3f); }
        RaceFlow flow; RaceDirector race;
        void Bind() { flow = FindAnyObjectByType<RaceFlow>(); race = flow ? flow.Race : null; }
        IEnumerator WaitFlow(float limit = 120)
        {
            float t0 = Time.realtimeSinceStartup;
            while (true) { Bind(); if (flow && flow.Started && !LoadingScreen.Holding && flow.Save != null) break; yield return null; if (Time.realtimeSinceStartup - t0 > limit) break; }
            var title = FindAnyObjectByType<StartupTitle>(); if (title) { typeof(StartupTitle).GetField("completed", BindingFlags.Static | BindingFlags.NonPublic)?.SetValue(null, true); Destroy(title.gameObject); }
            if (flow.State == RaceFlow.Stage.Title) flow.EnterMenuAfterTitle();
            yield return null;
        }
        // from the menu of whatever scene is loaded: go to a Mountain Loop scene (through the course page, as the menu does) and wait until it is loaded
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

        IEnumerator Start()
        {
            Directory.CreateDirectory(outDir); AudioListener.volume = 0;
            string kind = Arg("-p99kind", "views");
            int w = int.Parse(Arg("-p99w", "1920")), h = int.Parse(Arg("-p99h", "1080")); Screen.SetResolution(w, h, FullScreenMode.Windowed); yield return null; yield return null;
            Note($"screen={Screen.width}x{Screen.height} gpu={SystemInfo.graphicsDeviceName} kind={kind}");
            yield return WaitFlow();
            yield return Run99(kind);
            File.WriteAllText(Path.Combine(outDir, "done.txt"), "done"); Application.Quit();
        }
        IEnumerator Run99(string kind)
        {
            switch (kind)
            {
                case "views": yield return Views(); break;
                case "aijump": yield return AiJump(); break;
                default: yield return RunMore99(kind); break;
            }
        }

        IEnumerator Views()
        {
            string scene = Arg("-p99scene", "MountainLoop"); yield return GoMountain(scene.EndsWith("Reverse"));
            Conditions(int.Parse(Arg("-p99time", "0")), int.Parse(Arg("-p99weather", "0")));
            yield return StartRace(Arg("-p99vehicle", "original"), null, 1);
            yield return new WaitForSecondsRealtime(2);
            var cam = Camera.main; var chase = FindAnyObjectByType<ChaseCamera>(); if (chase) chase.enabled = false; var cv = CameraViews.Current; if (cv) cv.enabled = false;
            var canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None); foreach (var c in canvases) c.enabled = false;
            race.vehicle.Body.isKinematic = true;
            foreach (var spec in Arg("-p99views", "").Split(';').Where(x => x.Length > 0))
            {
                var f = spec.Split(','); var p = new Vector3(F(f[1]), F(f[2]), F(f[3]));
                cam.transform.SetPositionAndRotation(p, Quaternion.Euler(F(f[5]), F(f[4]), 0)); yield return null; yield return null; yield return new WaitForSecondsRealtime(.4f);
                yield return Snap($"view-{scene}-{f[0]}"); Note($"view {f[0]} at {p}");
            }
        }

        // a normal race, rivals in cars: every rival at the Mountain Loop summit flight (the one whose lip is near (966, 190, 142))
        IEnumerator AiJump()
        {
            string scene = Arg("-p99scene", "MountainLoop"); yield return GoMountain(scene.EndsWith("Reverse"));
            int time = int.Parse(Arg("-p99time", "0")), weather = int.Parse(Arg("-p99weather", "0")); Conditions(time, weather);
            var roster = Arg("-p99rivals", "original,fastback,pebble,tourer").Split(',');
            yield return StartRace(Arg("-p99vehicle", "moto"), roster, int.Parse(Arg("-p99laps", "1")));
            var car = race.vehicle; var pilot = car.gameObject.AddComponent<RoadDriver>(); pilot.Initialize(race, car, true, 1, 1); pilot.Racer = race.Racers[0]; car.GetComponent<VehicleInput>().enabled = false;
            var flights = race.GetComponent<MountainFlights>(); var road = race.road; road.Initialize();
            var flight = flights.flights.OrderBy(f => Vector3.Distance(f.lip, new Vector3(966, 190, 142))).First();
            Note($"{scene} time {time} weather {weather}: flight '{flight.name}' approach {flight.approachStation:F0} end {flight.endStation:F0} lip {flight.lip} landing end {flight.landingEnd} aiEntrySpeed {flight.aiEntrySpeed} aiTakeoffSpeed {flight.aiTakeoffSpeed}; rivals {string.Join(",", roster)}");
            float scale = float.Parse(Arg("-p99scale", "3")); Time.timeScale = scale;
            var state = new Dictionary<RacerState, (bool air, float airStart, float lastSpeed, float takeoff, bool took, bool landed, float maxIn, float lipSpeed, float lipDist)>(); var done = new HashSet<RacerState>(); var report = new List<string>(); var trace = new List<string>(); var rec0 = new Dictionary<RacerState, int>(); var minUp = new Dictionary<RacerState, float>(); var landedAt = new Dictionary<RacerState, float>(); var traceAt = new Dictionary<RacerState, float>();
            float t0 = Time.time, real0 = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - real0 < 900 && flow.State == RaceFlow.Stage.Racing && done.Count < race.Racers.Count)
            {
                AudioListener.volume = 0; yield return new WaitForFixedUpdate();
                foreach (var r in race.Racers)
                {
                    if (done.Contains(r) || !r.Car) continue;
                    float s = road.Project(r.Car.Body.position, out _); bool inWindow = road.Relative(s, flight.approachStation) < road.Relative(flight.endStation, flight.approachStation);
                    if (inWindow && !done.Contains(r)) { float dl0 = Vector3.Distance(r.Car.Body.position, flight.lip); if (dl0 < 420 && Time.time >= (traceAt.TryGetValue(r, out var ta) ? ta : 0)) { traceAt[r] = Time.time + 1.5f; var rd = r.Car.GetComponent<RoadDriver>(); trace.Add($"{r.Name} {r.Car.GetComponent<VehicleConfiguration>().profileId} d_lip {dl0:F0} speed {r.Car.Body.linearVelocity.magnitude:F1} target {(rd ? rd.TargetSpeed : -1):F1} top {r.Car.topSpeed:F0} gripScale {r.Car.GripScale:F2} obstacle '{(rd ? rd.LastObstacle : "")}' wheels {r.Car.GroundedWheels}"); } }
                    state.TryGetValue(r, out var st); bool grounded = r.Car.GroundedWheels >= 2; float speed = r.Car.Body.linearVelocity.magnitude;
                    if (inWindow)
                    {
                        st.maxIn = Mathf.Max(st.maxIn, speed); float dl = Vector3.Distance(r.Car.Body.position, flight.lip);
                        if (grounded && dl < 12) { st.lipSpeed = speed; st.lipDist = dl; } // the last grounded sample near the lip
                        if (!st.air && !grounded && speed > 8 && dl < 60) { st.air = true; st.airStart = Time.time; st.takeoff = st.lastSpeed; st.took = true; rec0[r] = r.Recoveries; minUp[r] = 1; }
                        else if (st.air && grounded && Time.time - st.airStart > .4f && !st.landed)
                        {
                            st.landed = true; st.air = false; landedAt[r] = Time.time; var p = r.Car.Body.position; float ls = road.Project(p, out _); string veh = r.Car.GetComponent<VehicleConfiguration>().profileId; float along = Vector3.Dot(p - flight.landingEnd, flight.forward);
                            report.Add($"{r.Name} ({veh}): speed at the lip {st.lipSpeed:F1} m/s ({st.lipSpeed * 2.237f:F0} mph), took off at {st.takeoff:F1}, air {Time.time - st.airStart:F2} s, landed at {p.x:F0},{p.y:F0},{p.z:F0} (station {ls:F0}, end {flight.endStation:F0}), {Vector3.Distance(p, flight.landingEnd):F0} m {(along > 0 ? "beyond" : "short of")} the landing end, recoveries so far {r.Recoveries}");
                        }
                        if (!st.air && grounded) st.lastSpeed = speed;
                    }
                    else { if (st.took && st.landed) { done.Add(r); Note($"{r.Name}: out of the window {Time.time - landedAt[r]:F1} s after landing, recoveries during the flight and landing {r.Recoveries - rec0[r]}, lowest up-vector {minUp[r]:F2}, speed {speed:F1} m/s"); } if (grounded) st.lastSpeed = speed; }
                    if (st.took && minUp.ContainsKey(r)) minUp[r] = Mathf.Min(minUp[r], r.Car.transform.up.y);
                    state[r] = st;
                }
            }
            Time.timeScale = 1;
            File.WriteAllLines(Path.Combine(outDir, "aijump-trace.txt"), trace);
            foreach (var l in report) Note(l);
            foreach (var r in race.Racers) { state.TryGetValue(r, out var st); string veh = r.Car ? r.Car.GetComponent<VehicleConfiguration>().profileId : "?"; if (!st.took) Note($"{r.Name} ({veh}): did not take off in the window (max speed there {st.maxIn:F1} m/s, recoveries {r.Recoveries})"); }
            Note($"race clock {race.Clock:F0}s, window done for {done.Count}/{race.Racers.Count}");
            File.WriteAllLines(Path.Combine(outDir, "aijump.txt"), rows);
            Destroy(pilot); flow.Pause(); yield return null; flow.QuitRace();
        }
    }
}
