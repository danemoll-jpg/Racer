using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Racer
{
    // 0.72 time-of-day / weather evidence (command-line opt-in only): "-conditionsBench <folder>" with "-racerTestSave
    // <folder>" (isolated storage; refuses to run without it so Dan's saves are never touched). For each course family it
    // starts a race (the rivals stand on the grid with their lamps), holds the 0.71 fixed viewpoint and saves a screenshot of
    // all 9 time-of-day x weather combinations; the Forest cave at night; the Free Roam cycle at 08, 12, 18, 21, 00 and 05;
    // and GPU frame times at the three 0.71 views for Day/Clear, Night/Clear, Day/Rain and Night/Snow. Muted, vsync and frame
    // cap off. "-conditionsFpsOnly" skips the screenshots. Quits when done.
    // 0.73: the clouds show in every view; Night/Rain is measured too (lightning held so it never lands in a fixed view);
    // Free Roam dusk to night (18:00-23:00); and the motorcycle model: full-screen garage (New, Classic), the race grid at Day
    // and Night from the chase camera and a close side view, and GPU frame times on the grid with New and with Classic.
    public sealed class ConditionsBench : MonoBehaviour
    {
        string outDir;
        static readonly (string scene, float s, float up, bool fps, string label)[] Views =
        {
            ("StreetLoopGreybox", 12, 3.2f, true, "street"),
            ("LakeWoods", 12, 3.2f, true, "forest"),
            ("DansBackyardForward", 12, 3.2f, false, "backyard"),
            ("MountainLoop", 400, 3.6f, true, "mountain"),
        };
        // 0.74: Dawn/Clear, and Night/Rain with a lightning strike every 2 s during the measurement ("Night/Rain strikes").
        static readonly (TimeOfDay t, Weather w)[] Measured = { (TimeOfDay.Day, Weather.Clear), (TimeOfDay.Dawn, Weather.Clear), (TimeOfDay.Night, Weather.Clear), (TimeOfDay.Day, Weather.Rain), (TimeOfDay.Night, Weather.Rain), (TimeOfDay.Night, Weather.Snow) };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            var a = Environment.GetCommandLineArgs(); int i = Array.IndexOf(a, "-conditionsBench");
            if (i < 0 || i + 1 >= a.Length || Array.IndexOf(a, "-racerTestSave") < 0 || FindAnyObjectByType<ConditionsBench>()) return;
            var g = new GameObject("Conditions bench"); DontDestroyOnLoad(g); g.AddComponent<ConditionsBench>().outDir = a[i + 1];
        }

        IEnumerator Start()
        {
            Directory.CreateDirectory(outDir);
            var rows = new List<string> { $"screen={Screen.width}x{Screen.height} gpu={SystemInfo.graphicsDeviceName} quality={QualitySettings.names[QualitySettings.GetQualityLevel()]}" };
            Screen.SetResolution(3840, 2160, FullScreenMode.FullScreenWindow); yield return null; yield return null;
            WeatherEffects.HoldStrikes = true;
            bool fpsOnly = Array.IndexOf(Environment.GetCommandLineArgs(), "-conditionsFpsOnly") >= 0;
            foreach (var v in Views)
            {
                if (fpsOnly && !v.fps) continue;
                SceneManager.LoadScene(v.scene); yield return null; yield return null;
                var race = Prepare(); yield return Grid(race);
                race.road.Initialize();
                var p = race.road.At(v.s, out var f); f = Vector3.ProjectOnPlane(f, Vector3.up).normalized;
                var cam = Camera.main; foreach (var c in FindObjectsByType<ChaseCamera>(FindObjectsSortMode.None)) c.enabled = false;
                var eye = p - f * 9 + Vector3.up * v.up; cam.transform.SetPositionAndRotation(eye, Quaternion.LookRotation(p + f * 30 + Vector3.up * .5f - eye));
                if (!fpsOnly)
                    foreach (TimeOfDay t in Enum.GetValues(typeof(TimeOfDay))) foreach (Weather w in Enum.GetValues(typeof(Weather)))
                    { WorldLook.Current.Pin(LookPresets.Compose(t, w)); yield return Hold(3); yield return Shot($"{v.label}-{t}-{w}.jpg".ToLowerInvariant()); rows.Add($"VIEW {v.label} {t} {w}"); }
                if (v.fps)
                    foreach (var (t, w) in Measured)
                    {
                        WorldLook.Current.Pin(LookPresets.Compose(t, w)); yield return Hold(3);
                        bool strikes = t == TimeOfDay.Night && w == Weather.Rain; if (strikes) { WeatherEffects.HoldStrikes = false; WeatherEffects.TestStrikeInterval = 2; }
                        var gpu = new List<float>(); var frames = new List<float>(); var ft = new FrameTiming[1]; float t0 = Time.unscaledTime;
                        while (Time.unscaledTime - t0 < 10)
                        {
                            yield return null; Keep(); frames.Add(Time.unscaledDeltaTime);
                            FrameTimingManager.CaptureFrameTimings(); if (FrameTimingManager.GetLatestTimings(1, ft) > 0 && ft[0].gpuFrameTime > 0) gpu.Add((float)ft[0].gpuFrameTime);
                        }
                        gpu.Sort(); frames.Sort(); float med = gpu.Count > 0 ? gpu[gpu.Count / 2] : float.NaN, p95 = gpu.Count > 0 ? gpu[(int)(gpu.Count * .95f)] : float.NaN;
                        if (strikes) { WeatherEffects.HoldStrikes = true; WeatherEffects.TestStrikeInterval = 0; }
                        rows.Add($"FPS {v.label} {t}/{w}{(strikes ? " strikes every 2 s" : "")}: GPU median {med:F2} ms (95th {p95:F2}) = {1000 / med:F0} fps GPU-limited; wall-clock median frame {frames[frames.Count / 2] * 1000:F2} ms; {gpu.Count} timed frames; vehicles with lamps {FindObjectsByType<VehicleLights>(FindObjectsSortMode.None).Length}; screen {Screen.width}x{Screen.height}");
                    }
                File.WriteAllLines(Path.Combine(outDir, "conditions.txt"), rows);
                if (!fpsOnly && v.label == "forest")
                {
                    // Inside the Forest cave at night: no rain may fall there.
                    var ce = new Vector3(59.7f, 37.4f, 87.0f); cam.transform.SetPositionAndRotation(ce, Quaternion.LookRotation(new Vector3(87.4f, 40.4f, 157.5f) - ce));
                    foreach (var w in new[] { Weather.Clear, Weather.Rain })
                    {
                        WorldLook.Current.Pin(LookPresets.Compose(TimeOfDay.Night, w)); yield return Hold(3); yield return Shot($"forest-cave-night-{w}.jpg".ToLowerInvariant());
                        var fx = WorldLook.Current.GetComponent<WeatherEffects>(); var covered = typeof(WeatherEffects).GetField("covered", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(fx);
                        int falling = fx ? fx.GetComponentsInChildren<ParticleSystem>().Where(p => p.name == "Rain" || p.name == "Snow").Sum(p => p.particleCount) : -1;
                        rows.Add($"VIEW forest cave interior Night {w}: covered {covered}, falling particles {falling}");
                    }
                }
                if (!fpsOnly && v.label == "street")
                {
                    // The Free Roam day-night cycle (the same function Free Roam uses), Clear weather.
                    foreach (float h in new[] { 5f, 5.5f, 6f, 6.5f, 7f, 8f, 12f, 18f, 19f, 20f, 21f, 22f, 23f, 0f }) { WorldLook.Current.Pin(WorldLook.Cycle(h, Weather.Clear)); yield return Hold(2); yield return Shot($"freeroam-cycle-{Mathf.FloorToInt(h):00}{Mathf.RoundToInt(h % 1 * 60):00}.jpg"); rows.Add($"VIEW free roam cycle {h:00}:00 ({WorldLook.Current.Preset.name}), clouds {FindAnyObjectByType<SkyClouds>()?.Visible}"); }
                }
                if (!fpsOnly && v.label == "street")
                {
                    // 0.74 moon phases: the sky at night on days 1, 4, 8, 15, 22 and 27, looking at the moon (fixed at 40 degrees, south).
                    foreach (int day in new[] { 1, 4, 8, 15, 22, 27 })
                    {
                        float ph = WorldLook.PhaseOf(day, 0); WeatherEffects.TestMoonPhase = ph; WeatherEffects.TestMoonElevation = 40; WeatherEffects.TestMoonAzimuth = 180;
                        WorldLook.Current.Pin(WorldLook.Cycle(0, Weather.Clear, ph)); var to = -(Quaternion.Euler(40, 180, 0) * Vector3.forward); cam.transform.rotation = Quaternion.LookRotation(to);
                        yield return Hold(2); yield return Shot($"moon-day{day:00}.jpg"); rows.Add($"VIEW moon day {day}: phase {ph:F2}, lit {WorldLook.Illumination(ph):F2}");
                    }
                    WeatherEffects.TestMoonPhase = -1; cam.transform.SetPositionAndRotation(eye, Quaternion.LookRotation(p + f * 30 + Vector3.up * .5f - eye));
                    // lightning by day and by night (a strike forced at the start of the hold; the screenshot near its first pulse)
                    foreach (var t in new[] { TimeOfDay.Day, TimeOfDay.Night })
                    {
                        WorldLook.Current.Pin(LookPresets.Compose(t, Weather.Rain)); yield return Hold(2);
                        WeatherEffects.HoldStrikes = false; WeatherEffects.TestStrikeInterval = 30; WeatherEffects.TestBoltAzimuth = cam.transform.eulerAngles.y + 8; var fx = WorldLook.Current.GetComponent<WeatherEffects>(); int before = fx.Strikes;
                        typeof(WeatherEffects).GetField("nextStrike", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(fx, Time.time);
                        float w0 = Time.unscaledTime; while (fx.Strikes == before && Time.unscaledTime - w0 < 3) yield return null;
                        yield return null; yield return null; yield return Shot($"lightning-{t}.jpg".ToLowerInvariant());
                        WeatherEffects.HoldStrikes = true; WeatherEffects.TestStrikeInterval = 0; WeatherEffects.TestBoltAzimuth = float.NaN; yield return Hold(1);
                        rows.Add($"VIEW lightning {t}/Rain: strike {fx.Strikes - before}, flash {fx.PeakFlash:F2}, bolts shown {fx.BoltsShown}");
                    }
                }
                WorldLook.Current.Pin(null);
                File.WriteAllLines(Path.Combine(outDir, "conditions.txt"), rows);
            }
            if (!fpsOnly || Array.IndexOf(Environment.GetCommandLineArgs(), "-conditionsMoto") >= 0) yield return Motorcycle(rows);
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-conditionsModels") >= 0) yield return Models(rows);
            File.WriteAllLines(Path.Combine(outDir, "conditions.txt"), rows);
            Application.Quit();
        }
        // 0.75 evidence ("-conditionsModels"): a mixed grid (player Street Classic; AI Longroof GT, Needle 600, Trail Four)
        // from the chase camera at Day and Night, New vs Classic models: full-screen shots and GPU frame times.
        IEnumerator Models(List<string> rows)
        {
            SceneManager.LoadScene("StreetLoopGreybox"); yield return null; yield return null;
            var race = Prepare(); var flow = race.Flow; var s = flow.Save.Settings;
            s.opponentChoices = new[] { "tourer", "moto", "atv" }; s.opponentRoster = new[] { "tourer", "moto", "atv" }; race.opponentRoster = new[] { "tourer", "moto", "atv" };
            foreach (var t in new[] { TimeOfDay.Day, TimeOfDay.Night })
                foreach (bool model in new[] { true, false })
                {
                    VehicleVisual.NewModels = s.newMotorcycle = model; s.timeOfDay = (int)t; s.weather = 0;
                    flow.OpenGarage(); flow.SelectVehicle("original"); flow.SetColor(1); flow.CloseGarage();
                    yield return Grid(race); yield return Hold(3);
                    string tag = $"{t}-{(model ? "new" : "classic")}".ToLowerInvariant();
                    yield return Shot($"models-grid-{tag}-chase.jpg");
                    var gpu = new List<float>(); var ft = new FrameTiming[1]; float t0 = Time.unscaledTime;
                    while (Time.unscaledTime - t0 < 10) { yield return null; Keep(); FrameTimingManager.CaptureFrameTimings(); if (FrameTimingManager.GetLatestTimings(1, ft) > 0 && ft[0].gpuFrameTime > 0) gpu.Add((float)ft[0].gpuFrameTime); }
                    gpu.Sort(); float med = gpu.Count > 0 ? gpu[gpu.Count / 2] : float.NaN;
                    rows.Add($"FPS mixed grid {t}/Clear chase, models {(model ? "New" : "Classic")} (Street Classic + Longroof GT, Needle 600, Trail Four AI; traffic): GPU median {med:F2} ms = {1000 / med:F0} fps; {gpu.Count} timed frames; screen {Screen.width}x{Screen.height}");
                    File.WriteAllLines(Path.Combine(outDir, "conditions.txt"), rows);
                    flow.Pause(); flow.QuitRace(); yield return Hold(1);
                }
            VehicleVisual.NewModels = s.newMotorcycle = true; s.timeOfDay = 0;
        }
        // 0.73 Part D evidence: the Needle 600 model in the garage and on the race grid; frame time New vs Classic.
        IEnumerator Motorcycle(List<string> rows)
        {
            SceneManager.LoadScene("StreetLoopGreybox"); yield return null; yield return null;
            var race = Prepare(); var flow = race.Flow; var s = flow.Save.Settings;
            foreach (bool model in new[] { true, false })
            {
                VehicleVisual.NewModels = s.newMotorcycle = model;
                flow.OpenGarage(); flow.SelectVehicle("moto"); flow.SetColor(3); yield return Hold(2);
                yield return Full($"moto-garage-{(model ? "new" : "classic")}.jpg");
                if (model) { flow.SetColor(6); yield return Hold(1); yield return Full("moto-garage-new-black.jpg"); flow.SetColor(3); yield return Hold(.5f); }
                flow.CloseGarage(); yield return Hold(.5f);
            }
            VehicleVisual.NewModels = s.newMotorcycle = true;
            s.opponentChoices = new[] { "moto", "moto", "moto" }; s.opponentRoster = new[] { "moto", "moto", "moto" }; race.opponentRoster = new[] { "moto", "moto", "moto" };
            foreach (var t in new[] { TimeOfDay.Day, TimeOfDay.Night })
                foreach (bool model in new[] { true, false })
                {
                    VehicleVisual.NewModels = s.newMotorcycle = model; s.timeOfDay = (int)t; s.weather = 0;
                    flow.OpenGarage(); flow.SelectVehicle("moto"); flow.SetColor(3); flow.CloseGarage();
                    yield return Grid(race); yield return Hold(3);
                    string tag = $"{t}-{(model ? "new" : "classic")}".ToLowerInvariant();
                    yield return Full($"moto-race-{tag}-chase.jpg");
                    var cam = Camera.main; var chase = FindObjectsByType<ChaseCamera>(FindObjectsSortMode.None); foreach (var c in chase) c.enabled = false;
                    var car = race.vehicle.transform; var eye = car.position + car.right * 2.6f + car.forward * .4f + Vector3.up * .55f;
                    var old = (cam.transform.position, cam.transform.rotation); cam.transform.SetPositionAndRotation(eye, Quaternion.LookRotation(car.position + Vector3.up * .45f - eye));
                    yield return Hold(1); yield return Full($"moto-race-{tag}-side.jpg");
                    cam.transform.SetPositionAndRotation(old.Item1, old.Item2); foreach (var c in chase) c.enabled = true; yield return Hold(1);
                    if (t == TimeOfDay.Day)
                    {
                        var gpu = new List<float>(); var ft = new FrameTiming[1]; float t0 = Time.unscaledTime;
                        while (Time.unscaledTime - t0 < 10) { yield return null; Keep(); FrameTimingManager.CaptureFrameTimings(); if (FrameTimingManager.GetLatestTimings(1, ft) > 0 && ft[0].gpuFrameTime > 0) gpu.Add((float)ft[0].gpuFrameTime); }
                        gpu.Sort(); float med = gpu.Count > 0 ? gpu[gpu.Count / 2] : float.NaN;
                        rows.Add($"FPS moto grid Day/Clear chase, model {(model ? "New" : "Classic")} (player + 3 AI motorcycles): GPU median {med:F2} ms = {1000 / med:F0} fps; {gpu.Count} timed frames; screen {Screen.width}x{Screen.height}");
                        File.WriteAllLines(Path.Combine(outDir, "conditions.txt"), rows);
                    }
                    flow.Pause(); flow.QuitRace(); yield return Hold(1);
                }
            VehicleVisual.NewModels = s.newMotorcycle = true;
        }
        // Full screen (UI included) at the screen resolution.
        IEnumerator Full(string name)
        {
            yield return new WaitForEndOfFrame();
            var tex = ScreenCapture.CaptureScreenshotAsTexture(); File.WriteAllBytes(Path.Combine(outDir, name), tex.EncodeToJPG(88)); Destroy(tex);
        }
        RaceDirector Prepare()
        {
            var race = FindAnyObjectByType<RaceDirector>();
            var title = FindAnyObjectByType<StartupTitle>(); if (title) Destroy(title.gameObject);
            typeof(StartupTitle).GetField("completed", BindingFlags.Static | BindingFlags.NonPublic)?.SetValue(null, true);
            race.Flow.EnterMenuAfterTitle(); Keep(); return race;
        }
        // Start a race so the rivals stand on the grid, then hold every vehicle where it is.
        IEnumerator Grid(RaceDirector race)
        {
            race.opponents = true; race.Flow.StartRace(); yield return Hold(1.5f);
            foreach (var d in FindObjectsByType<RoadDriver>(FindObjectsSortMode.None)) { d.enabled = false; if (d.Car) d.Car.Body.isKinematic = true; }
            race.vehicle.Body.isKinematic = true; var input = race.vehicle.GetComponent<VehicleInput>(); if (input) input.enabled = false;
        }
        static void Keep() { AudioListener.volume = 0; QualitySettings.vSyncCount = 0; Application.targetFrameRate = -1; }
        static IEnumerator Hold(float seconds) { float t0 = Time.unscaledTime; while (Time.unscaledTime - t0 < seconds) { Keep(); yield return null; } }
        IEnumerator Shot(string name)
        {
            yield return new WaitForEndOfFrame();
            var cam = Camera.main; var rt = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32) { antiAliasing = 2 };
            var old = cam.targetTexture; cam.targetTexture = rt; cam.Render(); cam.targetTexture = old;
            RenderTexture.active = rt; var tex = new Texture2D(1920, 1080, TextureFormat.RGB24, false); tex.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0); tex.Apply(); RenderTexture.active = null; rt.Release();
            File.WriteAllBytes(Path.Combine(outDir, name), tex.EncodeToJPG(90)); Destroy(tex);
        }
    }
}
