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
    // 0.98 Part C evidence only (like SplitBench): `-getawayCheck <dir> -racerTestSave <dir>` in the built player starts a Getaway on Normal and drives the
    // runner with the road AI, writing a per-second log (heat, alert, cops near, helicopter existence / distance / on screen / audio) and screenshots.
    //   -gcKind roadhalf | roadfast | radio    -gcVehicle original | moto    -gcMinutes 5    -gcSeed 1
    // roadhalf = the main road at about half the vehicle's speed; roadfast = the road AI at pace 2.0 (flat out on the Needle 600); radio = the longest radio line, full screen then split.
    // Never active without both arguments.
    public sealed class GetawayPlayerCheck : MonoBehaviour
    {
        string outDir; readonly List<string> rows = new();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            var a = Environment.GetCommandLineArgs(); int i = Array.IndexOf(a, "-getawayCheck");
            if (i < 0 || i + 1 >= a.Length || Array.IndexOf(a, "-racerTestSave") < 0 || FindAnyObjectByType<GetawayPlayerCheck>()) return;
            var g = new GameObject("Getaway player check"); DontDestroyOnLoad(g); g.AddComponent<GetawayPlayerCheck>().outDir = a[i + 1];
        }
        static string Arg(string key, string fallback) { var a = Environment.GetCommandLineArgs(); int i = Array.IndexOf(a, key); return i >= 0 && i + 1 < a.Length ? a[i + 1] : fallback; }
        void Note(string s) { rows.Add(s); File.WriteAllLines(Path.Combine(outDir, "getaway-check.txt"), rows); Debug.Log("GETAWAYCHECK " + s); }
        IEnumerator Snap(string name) { ScreenCapture.CaptureScreenshot(Path.Combine(outDir, name + ".png")); yield return null; yield return null; }
        IEnumerator Start()
        {
            Directory.CreateDirectory(outDir); AudioListener.volume = 0; string kind = Arg("-gcKind", "roadfast"), vehicle = Arg("-gcVehicle", "moto"); int minutes = int.Parse(Arg("-gcMinutes", "5"));
            UnityEngine.Random.InitState(int.Parse(Arg("-gcSeed", "1")) * 7919 + 13);
            Screen.SetResolution(3840, 2160, FullScreenMode.Windowed); yield return null; yield return null;
            Note($"screen={Screen.width}x{Screen.height} gpu={SystemInfo.graphicsDeviceName} kind={kind} vehicle={vehicle}");
            foreach (bool two in kind == "radio" ? new[] { false, true } : new[] { false })
            {
                RaceFlow flow = null; float t0 = Time.realtimeSinceStartup;
                while ((flow = FindAnyObjectByType<RaceFlow>()) == null || !flow.Started || LoadingScreen.Holding) { yield return null; if (Time.realtimeSinceStartup - t0 > 120) break; }
                var title = FindAnyObjectByType<StartupTitle>(); if (title) { typeof(StartupTitle).GetField("completed", BindingFlags.Static | BindingFlags.NonPublic)?.SetValue(null, true); Destroy(title.gameObject); }
                if (flow.State == RaceFlow.Stage.Title) flow.EnterMenuAfterTitle();
                flow.Save.Settings.hints = false; flow.Save.Settings.vsync = true; flow.Save.ApplySettings();
                SplitScreen.P1Device = Keyboard.current; SplitScreen.Solo = !two; SplitScreen.P2AiRunner = two; SplitScreen.P2Ai = true; SplitScreen.Mode = SplitScreen.Kind.Police; SplitScreen.PoliceGame = SplitScreen.Game.Getaway;
                SplitScreen.PoliceDifficulty = 1; SplitScreen.PoliceMinutes = minutes; SplitScreen.Course = 0; SplitScreen.Time = TimeOfDay.Day; SplitScreen.Weather = Weather.Clear; SplitScreen.Traffic = true; SplitScreen.P1Vehicle = vehicle; SplitScreen.P2Vehicle = "atv";
                flow.StartSplit(); yield return null;
                t0 = Time.realtimeSinceStartup;
                while ((flow = FindAnyObjectByType<RaceFlow>()) == null || !flow.Started || LoadingScreen.Holding || flow.State != RaceFlow.Stage.Racing || GetawayChase.Current == null || GetawayChase.Current.Runners.Count == 0) { yield return null; if (Time.realtimeSinceStartup - t0 > 150) break; }
                var g = GetawayChase.Current; var race = flow.Race; var car = race.vehicle; var runner = g.Runners[0];
                Note($"start: {g.Cops.Count} cops, nearest {g.Cops.Min(c => Vector3.Distance(c.car.Body.position, runner.body.position)):F0} m behind, sirens {g.Cops.All(c => c.lights.Siren)}, state {g.State}");
                t0 = Time.time; while (g.State == GetawayChase.Phase.Starting && Time.time - t0 < 20) yield return null;
                if (kind == "radio")
                {
                    var names = Enumerable.Range(0, g.Net.Roads.Count).Select(i => g.Net.RoadNames[i]).Distinct().OrderByDescending(n => n.Length).Take(2).ToArray();
                    string longest = $"4 units posted at the exits near {names[0]} and {names[1]}"; var say = typeof(GetawayChase).GetMethod("Say", BindingFlags.NonPublic | BindingFlags.Instance);
                    say.Invoke(g, new object[] { longest, 0 }); typeof(GetawayChase).GetMethod("BumpHeat").Invoke(g, new object[] { "more units on the way" }); say.Invoke(g, new object[] { longest, 0 });
                    yield return new WaitForSecondsRealtime(.6f); Note($"radio line ({longest.Length} characters): {longest}; screen {Screen.width}x{Screen.height}"); yield return Snap("radio-" + (two ? "split" : "full"));
                    flow.QuitSplit(false); yield return new WaitForSecondsRealtime(3); continue;
                }
                var pilot = car.gameObject.AddComponent<RoadDriver>(); pilot.Initialize(race, car, false, 1, kind == "roadhalf" ? .8f : 2f); car.GetComponent<VehicleInput>().enabled = false;
                var csv = new StringBuilder("t,heat,alert,cops,near150,near600,nearest,runnerSpeed,seen,escape,bust,heli,heliDist,heliOnScreen,heliLOS,heliAudio,heliVol,radio\n"); var cam = Camera.main;
                int lastHeat = 1; float next = 0, heliAt = -1, seqNext = -1; int seq = 0, maxNear = 0; var heatAt = new Dictionary<int, float> { { 1, 0 } }; int radioSeen = 0; var shots = new HashSet<int>(); float real0 = Time.realtimeSinceStartup;
                while (g.State != GetawayChase.Phase.Over && g.State != GetawayChase.Phase.Done && flow.State == RaceFlow.Stage.Racing && Time.realtimeSinceStartup - real0 < minutes * 60 + 30)
                {
                    yield return null;
                    if (g.Heat != lastHeat) { lastHeat = g.Heat; heatAt[g.Heat] = g.Clock; Note($"[{g.Clock:F0}s] HEAT {g.Heat}: alert '{g.Alert}', cops {g.Cops.Count(c => !c.block)}"); if (g.Heat >= 2 && shots.Add(g.Heat)) { yield return new WaitForSecondsRealtime(.4f); yield return Snap($"{kind}-heat{g.Heat}-alert"); } }
                    if (g.Heli && heliAt < 0) { heliAt = g.Clock; seqNext = Time.realtimeSinceStartup + 2f; Note($"[{g.Clock:F0}s] HELICOPTER spawned at {g.Heli.transform.position}, {Vector3.Distance(g.Heli.transform.position, runner.body.position):F0} m from the runner"); }
                    if (g.Heli && seq < 5 && Time.realtimeSinceStartup >= seqNext) { seq++; seqNext = Time.realtimeSinceStartup + 1.5f; yield return Snap($"{kind}-heli-seq{seq}"); }
                    int near600 = g.Cops.Count(c => c.car && !c.block && Vector3.Distance(c.car.Body.position, runner.body.position) <= 600); maxNear = Mathf.Max(maxNear, near600);
                    if (g.Clock >= next)
                    {
                        next = g.Clock + 1; var hp = g.Heli ? g.Heli.transform.position : Vector3.zero; bool onScreen = false, los = false, playing = false; float vol = 0;
                        if (g.Heli) { var v = cam.WorldToViewportPoint(hp); onScreen = v.z > 0 && v.x > 0 && v.x < 1 && v.y > 0 && v.y < 1; los = !Physics.Linecast(cam.transform.position, hp, out var hit, ~0, QueryTriggerInteraction.Ignore) || hit.collider.GetComponentInParent<PoliceHelicopter>(); var src = g.Heli.GetComponent<AudioSource>(); playing = src && src.isPlaying; vol = src ? src.volume : 0; }
                        csv.AppendLine($"{g.Clock:F0},{g.Heat},\"{(g.AlertShown ? g.Alert : "")}\",{g.Cops.Count(c => !c.block)},{g.Cops.Count(c => !c.block && Vector3.Distance(c.car.Body.position, runner.body.position) < 150)},{near600},{g.Cops.Min(c => Vector3.Distance(c.car.Body.position, runner.body.position)):F0},{runner.body.linearVelocity.magnitude:F1},{runner.seen},{runner.escape:F2},{runner.bust:F2},{(g.Heli ? 1 : 0)},{(g.Heli ? Vector3.Distance(hp, runner.body.position) : -1):F0},{onScreen},{los},{playing},{vol:F1},\"{g.Radio}\"");
                        File.WriteAllText(Path.Combine(outDir, $"{kind}-{vehicle}.csv"), csv.ToString());
                    }
                    for (; radioSeen < g.RadioLog.Count; radioSeen++) Note($"[{g.Clock:F0}s] radio: {g.RadioLog[radioSeen]}");
                }
                Note($"{kind} {vehicle}: {runner.outcome} at {runner.freeSeconds:F0} s, top heat {runner.topHeat}, heat reached at [{string.Join(", ", heatAt.OrderBy(k => k.Key).Select(k => "H" + k.Key + "@" + k.Value.ToString("F0") + "s"))}], helicopter {(heliAt >= 0 ? "spawned at " + heliAt.ToString("F0") + " s" : "never")}, most cops within 600 m {maxNear}");
            }
            File.WriteAllText(Path.Combine(outDir, "done.txt"), "done"); Application.Quit();
        }
    }
}
