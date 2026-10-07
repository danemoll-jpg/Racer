using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Racer
{
    // 0.90 Part D evidence only (like ConditionsBench): `-splitBench <dir> -racerTestSave <dir>` runs a split-screen race
    // with "Player 2: AI driver" on each listed course (-splitCourses 0,2,4; default all three loops forward), player 1 left
    // on the grid and then driven by the race AI, and records GPU frame times (frame-timing stats build) for 40 s per
    // layout. Never active without both arguments.
    public sealed class SplitBench : MonoBehaviour
    {
        string outDir;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            var a = Environment.GetCommandLineArgs(); int i = Array.IndexOf(a, "-splitBench");
            if (i < 0 || i + 1 >= a.Length || Array.IndexOf(a, "-racerTestSave") < 0 || FindAnyObjectByType<SplitBench>()) return;
            var g = new GameObject("Split-screen bench"); DontDestroyOnLoad(g); g.AddComponent<SplitBench>().outDir = a[i + 1];
        }
        static string Arg(string key, string fallback) { var a = Environment.GetCommandLineArgs(); int i = Array.IndexOf(a, key); return i >= 0 && i + 1 < a.Length ? a[i + 1] : fallback; }
        IEnumerator Start()
        {
            Directory.CreateDirectory(outDir); AudioListener.volume = 0;
            var rows = new List<string> { $"screen={Screen.width}x{Screen.height} gpu={SystemInfo.graphicsDeviceName}" };
            Screen.SetResolution(3840, 2160, FullScreenMode.FullScreenWindow); yield return null; yield return null;
            foreach (var course in Arg("-splitCourses", "0,2,4").Split(',').Select(int.Parse))
                foreach (bool leftRight in new[] { false, true })
                {
                    RaceFlow flow = null; float t0 = Time.realtimeSinceStartup;
                    while ((flow = FindAnyObjectByType<RaceFlow>()) == null || !flow.Started || LoadingScreen.Holding) { yield return null; if (Time.realtimeSinceStartup - t0 > 120) break; }
                    var title = FindAnyObjectByType<StartupTitle>(); if (title) { typeof(StartupTitle).GetField("completed", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)?.SetValue(null, true); Destroy(title.gameObject); }
                    if (flow.State == RaceFlow.Stage.Title) flow.EnterMenuAfterTitle();
                    flow.Save.Settings.splitLeftRight = leftRight; flow.Save.Settings.hints = false; flow.Save.Settings.vsync = false; flow.Save.Settings.frameLimit = 120; flow.Save.ApplySettings();
                    SplitScreen.P1Device = Keyboard.current; SplitScreen.P2Ai = true; SplitScreen.Course = course; SplitScreen.Laps = 3;
                    SplitScreen.P1Vehicle = "moto"; SplitScreen.P2Vehicle = "tourer";
                    flow.StartSplit(); yield return null;
                    t0 = Time.realtimeSinceStartup;
                    while ((flow = FindAnyObjectByType<RaceFlow>()) == null || !flow.Started || LoadingScreen.Holding || flow.State != RaceFlow.Stage.Racing) { yield return null; if (Time.realtimeSinceStartup - t0 > 120) break; }
                    QualitySettings.vSyncCount = 0; Application.targetFrameRate = 300;
                    // player 1 driven by the race AI too, so both views move through the course
                    var race = flow.Race; var car = race.vehicle; car.GetComponent<VehicleInput>().enabled = false;
                    var pilot = car.gameObject.AddComponent<RoadDriver>(); pilot.Initialize(race, car, true, 1, 1); pilot.Racer = race.Racers[0];
                    var gpu = new List<float>(); var frames = new List<float>(); var ft = new FrameTiming[1]; t0 = Time.unscaledTime;
                    while (Time.unscaledTime - t0 < 40 && flow.State == RaceFlow.Stage.Racing)
                    {
                        yield return null; frames.Add(Time.unscaledDeltaTime);
                        FrameTimingManager.CaptureFrameTimings(); if (FrameTimingManager.GetLatestTimings(1, ft) > 0 && ft[0].gpuFrameTime > 0) gpu.Add((float)ft[0].gpuFrameTime);
                    }
                    gpu.Sort(); frames.Sort();
                    float med = gpu.Count > 0 ? gpu[gpu.Count / 2] : float.NaN, p95 = gpu.Count > 0 ? gpu[(int)(gpu.Count * .95f)] : float.NaN, worst = gpu.Count > 0 ? gpu[gpu.Count - 1] : float.NaN;
                    rows.Add($"SPLIT {RacePlaylists.Titles[course]} {(leftRight ? "left/right" : "top/bottom")}: GPU median {med:F2} ms (95th {p95:F2}, max {worst:F2}) = {1000 / med:F0} fps median, {1000 / p95:F0} fps 95th; wall-clock median {frames[frames.Count / 2] * 1000:F2} ms; {gpu.Count} frames");
                    ScreenCapture.CaptureScreenshot(Path.Combine(outDir, $"split-{course}-{(leftRight ? "lr" : "tb")}.png"));
                    yield return null; yield return null;
                    File.WriteAllLines(Path.Combine(outDir, "split.txt"), rows);
                    Destroy(pilot); flow.Pause(); yield return null; flow.QuitRace(); yield return null;
                }
            File.WriteAllLines(Path.Combine(outDir, "split.txt"), rows); File.WriteAllText(Path.Combine(outDir, "done.txt"), "done");
            Application.Quit();
        }
    }
}
