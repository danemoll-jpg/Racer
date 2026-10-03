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
        static readonly (TimeOfDay t, Weather w)[] Measured = { (TimeOfDay.Day, Weather.Clear), (TimeOfDay.Night, Weather.Clear), (TimeOfDay.Day, Weather.Rain), (TimeOfDay.Night, Weather.Snow) };

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
                        var gpu = new List<float>(); var frames = new List<float>(); var ft = new FrameTiming[1]; float t0 = Time.unscaledTime;
                        while (Time.unscaledTime - t0 < 10)
                        {
                            yield return null; Keep(); frames.Add(Time.unscaledDeltaTime);
                            FrameTimingManager.CaptureFrameTimings(); if (FrameTimingManager.GetLatestTimings(1, ft) > 0 && ft[0].gpuFrameTime > 0) gpu.Add((float)ft[0].gpuFrameTime);
                        }
                        gpu.Sort(); frames.Sort(); float med = gpu.Count > 0 ? gpu[gpu.Count / 2] : float.NaN, p95 = gpu.Count > 0 ? gpu[(int)(gpu.Count * .95f)] : float.NaN;
                        rows.Add($"FPS {v.label} {t}/{w}: GPU median {med:F2} ms (95th {p95:F2}) = {1000 / med:F0} fps GPU-limited; wall-clock median frame {frames[frames.Count / 2] * 1000:F2} ms; {gpu.Count} timed frames; vehicles with lamps {FindObjectsByType<VehicleLights>(FindObjectsSortMode.None).Length}; screen {Screen.width}x{Screen.height}");
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
                    foreach (float h in new[] { 8f, 12f, 18f, 21f, 0f, 5f }) { WorldLook.Current.Pin(WorldLook.Cycle(h, Weather.Clear)); yield return Hold(2); yield return Shot($"freeroam-cycle-{h:00}00.jpg"); rows.Add($"VIEW free roam cycle {h:00}:00 ({WorldLook.Current.Preset.name})"); }
                }
                WorldLook.Current.Pin(null);
                File.WriteAllLines(Path.Combine(outDir, "conditions.txt"), rows);
            }
            Application.Quit();
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
