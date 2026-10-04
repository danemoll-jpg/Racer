using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Racer
{
    // 0.77 evidence (command-line opt-in only): "-trailerShots <folder>" with "-racerTestSave <folder>". In the real window
    // it starts Free Roam, measures the frame time with the 0.77 camera components running (Trailer Mode off, Chase view)
    // against the same components switched off, then turns Trailer Mode on and saves full-screen captures (everything that
    // is drawn, overlays included) and the game's own screenshot key output at Day, Night/Rain and Dusk/Snow. Quits when done.
    public sealed class TrailerShots : MonoBehaviour
    {
        string outDir; readonly List<string> rows = new();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            var a = Environment.GetCommandLineArgs(); int i = Array.IndexOf(a, "-trailerShots");
            if (i < 0 || i + 1 >= a.Length || Array.IndexOf(a, "-racerTestSave") < 0 || FindAnyObjectByType<TrailerShots>()) return;
            var g = new GameObject("Trailer shots"); DontDestroyOnLoad(g); g.AddComponent<TrailerShots>().outDir = a[i + 1];
        }
        void Log(string s) { rows.Add(s); File.WriteAllLines(Path.Combine(outDir, $"trailer-{Screen.width}x{Screen.height}.txt"), rows); }
        IEnumerator Shot(string name) { yield return null; yield return new WaitForEndOfFrame(); ScreenCapture.CaptureScreenshot(Path.Combine(outDir, $"{Screen.width}x{Screen.height}-{name}.png")); yield return null; yield return null; }
        IEnumerator Frames(string label, bool on, List<float> all)
        {
            CameraViews.Current.enabled = on; TrailerMode.Instance.enabled = on;
            yield return new WaitForSecondsRealtime(1.5f);
            var t = new List<float>(); for (int i = 0; i < 360; i++) { yield return null; t.Add(Time.unscaledDeltaTime * 1000); }
            t.Sort(); all.Add(t.Average());
            Log($"{label}: mean {t.Average():F2} ms, median {t[t.Count / 2]:F2} ms, 95th {t[(int)(t.Count * .95f)]:F2} ms over {t.Count} frames");
        }
        IEnumerator Start()
        {
            Application.runInBackground = true; Directory.CreateDirectory(outDir); AudioListener.volume = 0;
            yield return null; yield return null;
            var race = FindAnyObjectByType<RaceDirector>(); var flow = race.Flow;
            typeof(StartupTitle).GetField("completed", BindingFlags.Static | BindingFlags.NonPublic)?.SetValue(null, true);
            var title = FindAnyObjectByType<StartupTitle>(); if (title) Destroy(title.gameObject);
            if (flow.State == RaceFlow.Stage.Title) flow.EnterMenuAfterTitle();
            flow.Save.Settings.master = 0; flow.Save.Settings.cameraView = 0; flow.Save.Settings.roamWeather = 0; flow.Save.ApplySettings();
            yield return new WaitForSecondsRealtime(1); flow.StartFreeRoam();
            float t0 = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - t0 < 90) { yield return null; race = FindAnyObjectByType<RaceDirector>(); if (race && race.gameObject.scene.name == RaceFlow.RoamScene && race.Flow && race.Flow.State == RaceFlow.Stage.Racing) break; }
            flow = race.Flow; AudioListener.volume = 0; yield return new WaitForSecondsRealtime(4);
            Log($"Free Roam in {race.gameObject.scene.name}, {Screen.width}x{Screen.height}, vsync {QualitySettings.vSyncCount}, frame cap {Application.targetFrameRate}");
            // Frame time with the mode off: 0.77 components running vs switched off (twice each, alternating).
            QualitySettings.vSyncCount = 0; Application.targetFrameRate = -1;
            var on = new List<float>(); var off = new List<float>();
            for (int i = 0; i < 2; i++) { yield return Frames("0.77 cameras running, Trailer Mode off", true, on); yield return Frames("0.77 cameras switched off", false, off); }
            CameraViews.Current.enabled = TrailerMode.Instance.enabled = true;
            Log($"Mode off vs components off: {on.Average():F2} ms vs {off.Average():F2} ms ({on.Average() - off.Average():+0.00;-0.00} ms)");
            flow.Save.ApplySettings();
            yield return Shot("hud-normal-day");
            var mode = TrailerMode.Instance; var views = CameraViews.Current;
            mode.Begin(); typeof(TrailerMode).GetField("hintUntil", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(mode, Time.unscaledTime + 30);
            yield return new WaitForSecondsRealtime(1); yield return Shot("trailer-first-time-hint");
            typeof(TrailerMode).GetField("hintUntil", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(mode, 0f);
            views.SelectTrailerCamera(CameraViews.Shot.Orbit); mode.CycleTimeOfDay();
            foreach (var (tag, hour, w) in new[] { ("day", 12f, Weather.Clear), ("night-rain", 23.5f, Weather.Rain), ("dusk-snow", 19.2f, Weather.Snow) })
            {
                var c = WorldLook.Current.Trailer; c.hour = hour; c.weather = w; c.paused = true;
                yield return new WaitForSecondsRealtime(3);
                yield return Shot("trailer-hud-hidden-" + tag);
                mode.Screenshot(); yield return null; yield return null; yield return new WaitForSecondsRealtime(4);
                var file = mode.LastScreenshot; var tex = new Texture2D(2, 2);
                bool ok = file != null && File.Exists(file) && tex.LoadImage(File.ReadAllBytes(file));
                if (ok) File.Copy(file, Path.Combine(outDir, $"{Screen.width}x{Screen.height}-screenshot-key-{tag}.png"), true);
                Log($"{tag}: look {WorldLook.Current.Preset.name} rain {WorldLook.Current.Preset.rain:F1} snow {WorldLook.Current.Preset.snowfall:F1}; HUD canvas drawn {FindAnyObjectByType<RaceHud>().GetComponent<Canvas>().enabled}; screenshot key -> {(ok ? $"{Path.GetFileName(file)} {tex.width}x{tex.height} (window {Screen.width}x{Screen.height}) {new FileInfo(file).Length:N0} bytes" : "MISSING")}");
                Destroy(tex);
            }
            mode.End(); yield return new WaitForSecondsRealtime(1);
            Log("done"); Application.Quit();
        }
    }
}
