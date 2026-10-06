using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Racer
{
    // 0.82 Part C stutter evidence (command-line opt-in only): "-hitchBench <folder>" with "-racerTestSave <folder>"
    // (isolated storage; never Dan's saves). At the current screen size, muted, vsync and frame cap off, it records every
    // frame of (1) a Street Loop race start with AI and traffic (the player held on an autopilot along the race line) and
    // (2) a 75 s Free Roam drive with traffic along Hwy 92 and S Cherokee Ln (autopilot along the Street Loop line from
    // Hwy 92 west of S Cherokee), and lists every frame over 16.7 ms with what the main thread was doing (profiler markers;
    // the markers only report in a development player, the frame times in any player). Quits when done.
    public sealed class HitchBench : MonoBehaviour
    {
        string outDir;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            var a = Environment.GetCommandLineArgs(); int i = Array.IndexOf(a, "-hitchBench");
            if (i < 0 || i + 1 >= a.Length || Array.IndexOf(a, "-racerTestSave") < 0 || FindAnyObjectByType<HitchBench>()) return;
            var g = new GameObject("Hitch bench"); DontDestroyOnLoad(g); g.AddComponent<HitchBench>().outDir = a[i + 1];
        }

        static readonly (ProfilerCategory cat, string name)[] Markers =
        {
            (ProfilerCategory.Internal, "Main Thread"), (ProfilerCategory.Scripts, "GC.Collect"), (ProfilerCategory.Render, "Shader.CreateGPUProgram"),
            (ProfilerCategory.Scripts, "Instantiate"), (ProfilerCategory.Scripts, "BehaviourUpdate"), (ProfilerCategory.Scripts, "CoroutinesDelayedCalls"),
            (ProfilerCategory.Scripts, "FixedBehaviourUpdate"), (ProfilerCategory.Physics, "Physics.Simulate"), (ProfilerCategory.Physics, "Physics.BakeCollisionMeshes"),
            (ProfilerCategory.Scripts, "LateBehaviourUpdate"), (ProfilerCategory.Render, "Camera.Render"), (ProfilerCategory.Render, "Gfx.WaitForPresentOnGfxThread"),
            (ProfilerCategory.Loading, "Loading.ReadObject"), (ProfilerCategory.Render, "Mesh.UploadMeshData"), (ProfilerCategory.Animation, "Animator.Update"),
        };
        readonly List<ProfilerRecorder> recorders = new();
        ProfilerRecorder gcBytes;
        readonly List<string> frames = new(); string phase = "-"; bool recording; float phaseStart; int gc0;
        ArcadeVehicle drivenCar; Vector3[] path; int pathIndex; float targetSpeed = 22;

        IEnumerator Start()
        {
            Directory.CreateDirectory(outDir); Application.runInBackground = true;
            foreach (var (cat, name) in Markers) recorders.Add(ProfilerRecorder.StartNew(cat, name, 1));
            gcBytes = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame", 1);
            var rows = new List<string> { $"screen={Screen.width}x{Screen.height} gpu={SystemInfo.graphicsDeviceName} cpu={SystemInfo.processorType} development={Debug.isDebugBuild} version={Application.version}" };
            Keep(); yield return null; yield return null;
            // (1) Street Loop race start with AI and traffic
            if (SceneManager.GetActiveScene().name != "StreetLoopGreybox") { SceneManager.LoadScene("StreetLoopGreybox"); yield return null; yield return null; }
            { float w0 = Time.realtimeSinceStartup; while (Time.realtimeSinceStartup - w0 < 60) { var r = FindAnyObjectByType<RaceDirector>(); if (r && r.Flow != null && r.Flow.Started && !LoadingScreen.Holding) break; Keep(); yield return null; } }
            var race = Prepare(); var flow = race.Flow; flow.Save.Settings.vehicleId = "original"; race.opponents = true; race.traffic = true; race.laps = 1;
            yield return Hold(2);
            Begin("race-start"); StartCoroutine(Capture("race")); yield return null; flow.StartRace();
            float t0 = Time.unscaledTime; while (flow.State != RaceFlow.Stage.Racing && Time.unscaledTime - t0 < 20) { Keep(); yield return null; }
            race.road.Initialize(); var rp = new List<Vector3>(); for (float s = 0; s < race.road.Length; s += 4) rp.Add(race.road.At(s, out _));
            Drive(race.vehicle, rp.ToArray(), 0, 22); yield return Hold(25); Stop(rows);
            // (2) Free Roam along Hwy 92 and S Cherokee Ln with traffic
            flow.Pause(); flow.QuitRace(); yield return Hold(1);
            race = FindAnyObjectByType<RaceDirector>(); race.traffic = true; Begin("free-roam-load"); StartCoroutine(Capture("load", 14)); race.Flow.StartFreeRoam();
            t0 = Time.unscaledTime;
            while (Time.unscaledTime - t0 < 60) { Keep(); yield return null; race = FindAnyObjectByType<RaceDirector>(); if (SceneManager.GetActiveScene().name == RaceFlow.RoamScene && race && race.Flow != null && race.Flow.State == RaceFlow.Stage.Racing && !LoadingScreen.Holding) break; }
            yield return Hold(3); Stop(rows);
            var loop = CoursePreviewCatalog.Courses.First(c => c.scene == "StreetLoopGreybox").main;
            int from = Nearest(loop, new Vector3(-150, 0, 528)), to = Nearest(loop, new Vector3(315, 0, 520));
            int fwd = (to - from + loop.Length) % loop.Length, back = (from - to + loop.Length) % loop.Length;
            var ordered = Enumerable.Range(0, loop.Length).Select(k => loop[((fwd <= back ? from + k : from - k) % loop.Length + loop.Length) % loop.Length]).ToArray();
            var car = race.vehicle; Put(car, ordered[0], ordered[1]); yield return Hold(1);
            Begin("free-roam-drive"); Drive(car, ordered, 0, 24); yield return Hold(5); yield return Capture("roam"); yield return Hold(64); Stop(rows);
            rows.Add($"free roam drive ended at ({car.transform.position.x:F0}, {car.transform.position.z:F0}), path point {pathIndex} of {ordered.Length}");
            File.WriteAllLines(Path.Combine(outDir, "hitches.txt"), rows);
            File.WriteAllLines(Path.Combine(outDir, "frames.csv"), frames);
            Application.Quit();
        }

        // "-hitchProfile": 6 s of a full profiler capture (development players) to <folder>/<name>.raw, read by the editor
        IEnumerator Capture(string name, float seconds = 6)
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-hitchProfile") < 0 || !Debug.isDebugBuild) yield break;
            UnityEngine.Profiling.Profiler.maxUsedMemory = 512 * 1024 * 1024; UnityEngine.Profiling.Profiler.logFile = Path.Combine(outDir, name);
            UnityEngine.Profiling.Profiler.enableBinaryLog = true; UnityEngine.Profiling.Profiler.enabled = true;
            yield return Hold(seconds);
            UnityEngine.Profiling.Profiler.enabled = false; UnityEngine.Profiling.Profiler.enableBinaryLog = false; UnityEngine.Profiling.Profiler.logFile = "";
        }
        void Begin(string name) { phase = name; phaseStart = Time.unscaledTime; recording = true; gc0 = lastGc = GC.CollectionCount(0); frameRows.Clear(); }
        readonly List<(float t, float ms, string detail)> frameRows = new();
        void Stop(List<string> rows)
        {
            recording = false; drivenCar = null;
            var all = frameRows; var slow = all.Where(f => f.ms > 16.7f).ToList();
            rows.Add($"== {phase}: {all.Count} frames over {(all.Count > 0 ? all[^1].t : 0):F1} s, median {Median(all.Select(f => f.ms)):F2} ms, frames over 16.7 ms: {slow.Count}, worst {(all.Count > 0 ? all.Max(f => f.ms) : 0):F1} ms, GC collections {GC.CollectionCount(0) - gc0}");
            foreach (var f in slow) rows.Add($"   t {f.t:F2} s  {f.ms:F1} ms  {f.detail}");
            File.WriteAllLines(Path.Combine(outDir, "hitches.txt"), rows);
        }
        static float Median(IEnumerable<float> v) { var l = v.OrderBy(x => x).ToList(); return l.Count == 0 ? 0 : l[l.Count / 2]; }

        int lastGc;
        void Update()
        {
            if (!recording) return;
            float ms = Time.unscaledDeltaTime * 1000; int gc = GC.CollectionCount(0); int gcNow = gc - lastGc; lastGc = gc;
            var parts = new List<string>(); string csv = "";
            for (int i = 0; i < recorders.Count; i++) { float v = recorders[i].Valid ? recorders[i].LastValue / 1e6f : 0; csv += $",{v:F2}"; if (v >= .5f && i != 0) parts.Add($"{Markers[i].name} {v:F1}"); }
            long alloc = gcBytes.Valid ? gcBytes.LastValue : 0;
            string detail = $"main {(recorders[0].Valid ? recorders[0].LastValue / 1e6f : 0):F1} ms; " + string.Join(", ", parts) + (gcNow > 0 ? $"; GC collections {gcNow}" : "") + $"; alloc {alloc / 1024} KB; scene {SceneManager.GetActiveScene().name}";
            frameRows.Add((Time.unscaledTime - phaseStart, ms, detail));
            if (frames.Count == 0) frames.Add("phase,t,ms,gcCollections,allocKB" + string.Concat(Markers.Select(m => "," + m.name)));
            frames.Add($"{phase},{Time.unscaledTime - phaseStart:F3},{ms:F2},{gcNow},{alloc / 1024}{csv}");
        }

        // a simple autopilot along a polyline (look-ahead 12 m), holding the target speed
        void Drive(ArcadeVehicle car, Vector3[] pts, int start, float speed)
        {
            drivenCar = car; path = pts; pathIndex = start; targetSpeed = speed;
            car.GetComponent<VehicleInput>().enabled = false; car.enabled = false;
        }
        void FixedUpdate()
        {
            var car = drivenCar; if (!car || path == null) return; Keep();
            var p = car.Body.position;
            while (pathIndex < path.Length - 1 && Flat(path[pathIndex] - p).magnitude < 12) pathIndex++;
            var to = Flat(path[pathIndex] - p);
            float ang = Vector3.SignedAngle(Flat(car.transform.forward), to, Vector3.up);
            float v = car.ForwardSpeed; float target = Mathf.Abs(ang) > 25 ? Mathf.Min(targetSpeed, 12) : targetSpeed;
            car.Simulate(v < target ? 1 : 0, v > target + 3 ? .6f : 0, Mathf.Clamp(ang / 25f, -1, 1), Time.fixedDeltaTime);
        }
        static Vector3 Flat(Vector3 v) { v.y = 0; return v; }
        static int Nearest(Vector3[] pts, Vector3 q) { int best = 0; float d = float.MaxValue; for (int i = 0; i < pts.Length; i++) { float e = Flat(pts[i] - q).sqrMagnitude; if (e < d) { d = e; best = i; } } return best; }
        static void Put(ArcadeVehicle car, Vector3 at, Vector3 next)
        {
            if (Physics.Raycast(at + Vector3.up * 50, Vector3.down, out var h, 120, ~0, QueryTriggerInteraction.Ignore)) at = h.point + Vector3.up * .8f;
            var rot = Quaternion.LookRotation(Flat(next - at).normalized);
            car.Body.position = at; car.Body.rotation = rot; car.transform.SetPositionAndRotation(at, rot); car.Body.linearVelocity = Vector3.zero; car.Body.angularVelocity = Vector3.zero;
            car.GetComponent<VehicleRespawn>()?.SeedCoursePosition(at);
        }
        RaceDirector Prepare()
        {
            var race = FindAnyObjectByType<RaceDirector>();
            var title = FindAnyObjectByType<StartupTitle>(); if (title) Destroy(title.gameObject);
            typeof(StartupTitle).GetField("completed", BindingFlags.Static | BindingFlags.NonPublic)?.SetValue(null, true);
            race.Flow.EnterMenuAfterTitle(); Keep(); return race;
        }
        static void Keep() { AudioListener.volume = 0; QualitySettings.vSyncCount = 0; Application.targetFrameRate = -1; }
        static IEnumerator Hold(float seconds) { float t0 = Time.unscaledTime; while (Time.unscaledTime - t0 < seconds) { Keep(); yield return null; } }
        void OnDestroy() { foreach (var r in recorders) r.Dispose(); gcBytes.Dispose(); }
    }
}
