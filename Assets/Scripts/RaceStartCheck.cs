using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

namespace Racer
{
    // 0.93 Part A evidence only: `-raceStartCheck <dir> -racerTestSave <dir> -raceStartCases "a;b;..."` starts races in the
    // built player (title skipped) and watches the start: the grid (any two vehicles' colliders overlapping), then from GO
    // the player's throttle held through an emulated controller for 3 s (the real input path), then the race AI driving it
    // to 10 s; every vehicle's distance moved at 3 s and 10 s and any reset (a jump of over 6 m in one frame). Cases:
    //   event:<campaign event id>:<vehicle>     a campaign event, as from its page
    //   setup:<course index>:<vehicle>:<rival,rival,rival>   a Race Setup race (3 laps) with those rivals
    //   split:<course index>:<vehicle>:<rivals>  split-screen, player 2 the AI driver, that many AI rivals (Mixed)
    // Never active without both arguments.
    public sealed class RaceStartCheck : MonoBehaviour
    {
        string outDir; Gamepad pad; readonly List<string> log = new(); int failures, shot;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            var a = Environment.GetCommandLineArgs(); int i = Array.IndexOf(a, "-raceStartCheck");
            if (i < 0 || i + 1 >= a.Length || Array.IndexOf(a, "-racerTestSave") < 0 || FindAnyObjectByType<RaceStartCheck>()) return;
            var g = new GameObject("Race start check"); DontDestroyOnLoad(g); g.AddComponent<RaceStartCheck>().outDir = a[i + 1];
        }
        void Note(string line) { log.Add(line); Debug.Log("RACESTART " + line); File.WriteAllLines(Path.Combine(outDir, "racestart.txt"), log); }
        static RaceFlow Flow => FindAnyObjectByType<RaceFlow>();
        IEnumerator Until(Func<bool> condition, float seconds) { float t = Time.realtimeSinceStartup + seconds; while (!condition() && Time.realtimeSinceStartup < t) yield return null; }
        IEnumerator Wait(float s) { float t = Time.realtimeSinceStartup + s; while (Time.realtimeSinceStartup < t) yield return null; }
        void Throttle(float value) { using (StateEvent.From(pad, out var ptr)) { pad.rightTrigger.WriteValueIntoEvent(value, ptr); pad.leftStick.WriteValueIntoEvent(Vector2.zero, ptr); InputSystem.QueueEvent(ptr); } }
        void Shot(string label) { ScreenCapture.CaptureScreenshot(Path.Combine(outDir, $"{shot++:00}-{label}.png")); }
        static string V(Vector3 v) => $"({v.x:F1}, {v.y:F1}, {v.z:F1})";

        IEnumerator Start()
        {
            Directory.CreateDirectory(outDir); AudioListener.volume = 0; Application.runInBackground = true;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            pad = InputSystem.AddDevice<Gamepad>("RaceStartCheck pad");
            Note($"version {Application.version}, screen {Screen.width}x{Screen.height}");
            yield return Until(() => Flow && Flow.Started && !LoadingScreen.Holding, 120); yield return Wait(1);
            var args = Environment.GetCommandLineArgs(); int c = Array.IndexOf(args, "-raceStartCases");
            foreach (var spec in (c >= 0 && c + 1 < args.Length ? args[c + 1] : "").Split(';').Where(s => s.Length > 0))
            {
                Note("---- " + spec); var a = spec.Split(':');
                if (a[0] == "event")
                {
                    var e = CampaignData.Find(a[1]); bool open = Campaign.Available(e);
                    if (!open) Campaign.Testing = true;
                    Note($"{e.Name}: {e.CourseTitle}, {e.Rivals?.Length ?? 0} rivals, available on this save {open}");
                    Flow.StartCampaignEvent(e, a[2]);
                }
                else if (a[0] == "setup")
                {
                    string scene = RacePlaylists.Scenes[int.Parse(a[1])];
                    if (SceneManager.GetActiveScene().name != scene) { SceneManager.LoadScene(scene); yield return null; yield return null; }
                    yield return Until(() => Flow && Flow.Started && !LoadingScreen.Holding && Flow.State == RaceFlow.Stage.Ready, 120); yield return Wait(1);
                    var race = Flow.Race; race.opponents = true; race.traffic = false; race.laps = 3; race.opponentRoster = a[3].Split(',');
                    race.vehicle.GetComponent<VehicleConfiguration>().Apply(a[2]);
                    Flow.StartRace();
                }
                else if (a[0] == "split")
                {
                    SplitScreen.P1Device = pad; SplitScreen.P2Ai = true; SplitScreen.P1Vehicle = a[2]; SplitScreen.P2Vehicle = "atv";
                    SplitScreen.Course = int.Parse(a[1]); SplitScreen.Laps = 2; SplitScreen.Rivals = int.Parse(a[3]); SplitScreen.RivalsRandom = false; SplitScreen.Traffic = false;
                    SplitScreen.Time = TimeOfDay.Day; SplitScreen.Weather = Weather.Clear;
                    Flow.StartSplit();
                }
                yield return Watch(spec.Replace(':', '-').Replace(',', '-'));
                if (Flow && Flow.State != RaceFlow.Stage.Ready) { Flow.Pause(); yield return null; if (SplitScreen.Active) Flow.QuitSplit(false); else Flow.QuitRace(); }
                yield return Wait(1.5f); Campaign.Testing = Flow && Flow.Save.Settings.unlockEverything;
            }
            Note(failures == 0 ? "ALL PASS" : $"FAILURES {failures}");
            File.WriteAllText(Path.Combine(outDir, "done.txt"), failures.ToString());
            yield return Wait(1); Application.Quit();
        }

        IEnumerator Watch(string label)
        {
            yield return Until(() => Flow && Flow.Started && !LoadingScreen.Holding && (Flow.State == RaceFlow.Stage.Countdown || Flow.State == RaceFlow.Stage.Racing), 120);
            var race = Flow.Race; var racers = race.Racers.ToList();
            if (Flow.State != RaceFlow.Stage.Countdown && Flow.State != RaceFlow.Stage.Racing) { Note($"FAIL {label}: no race (state {Flow.State})"); failures++; yield break; }
            yield return new WaitForFixedUpdate();
            // the grid: every vehicle, and any two whose colliders overlap
            Note($"{SceneManager.GetActiveScene().name}: {racers.Count} on the grid (origin {race.Origin:F1} m)");
            foreach (var r in racers) Note($"  {r.Name} [{r.Car.GetComponent<VehicleConfiguration>().profileId}] at {V(r.Car.Body.position)}, station {race.road.Project(r.Car.Body.position, out float lat):F1} m, side {lat:+0.0;-0.0} m");
            int overlaps = 0;
            for (int i = 0; i < racers.Count; i++) for (int j = i + 1; j < racers.Count; j++)
                {
                    var x = racers[i].Car.GetComponent<BoxCollider>(); var y = racers[j].Car.GetComponent<BoxCollider>();
                    if (x && y && Physics.ComputePenetration(x, x.transform.position, x.transform.rotation, y, y.transform.position, y.transform.rotation, out _, out float depth))
                    { overlaps++; Note($"  OVERLAP {racers[i].Name} / {racers[j].Name} by {depth:F2} m"); }
                }
            Note($"  grid overlaps: {overlaps}");
            // anything solid (not the ground under the wheels) inside a vehicle's box
            foreach (var r in racers)
            {
                var box = r.Car.GetComponent<BoxCollider>(); if (!box) continue;
                var others = Physics.OverlapBox(box.transform.TransformPoint(box.center), Vector3.Scale(box.size, box.transform.lossyScale) / 2, box.transform.rotation, ~0, QueryTriggerInteraction.Ignore)
                    .Where(o => !o.GetComponentInParent<ArcadeVehicle>()).Select(o => o.name).Distinct().ToArray();
                if (others.Length > 0) Note($"  {r.Name}'s box touches: {string.Join(", ", others)}");
            }
            yield return Until(() => Flow.State == RaceFlow.Stage.Racing, 30);
            var input = race.vehicle.GetComponent<VehicleInput>(); Throttle(1);
            var start = racers.Select(r => r.Car.Body.position).ToArray(); var last = start.ToArray(); var resets = new int[racers.Count]; var at3 = new float[racers.Count];
            int playerResets = 0; Action onReset = () => playerResets++; var respawn = race.vehicle.GetComponent<VehicleRespawn>(); if (respawn) respawn.Respawned += onReset;
            float t0 = Time.time, throttleRead = 0; bool shotTaken = false; RoadDriver pilot = null;
            while (Time.time - t0 < 10 && Flow.State == RaceFlow.Stage.Racing)
            {
                yield return new WaitForFixedUpdate();
                for (int i = 0; i < racers.Count; i++)
                {
                    var p = racers[i].Car.Body.position; if ((p - last[i]).magnitude > 6) resets[i]++; last[i] = p;
                    if (Time.time - t0 <= 3) at3[i] = Vector3.Distance(p, start[i]);
                }
                if (Time.time - t0 < 1) throttleRead = Mathf.Max(throttleRead, input ? input.Throttle : 0);
                if (!shotTaken && Time.time - t0 > .6f) { shotTaken = true; Shot(label + "-GO"); }
                if (!pilot && Time.time - t0 > 3)
                {
                    Throttle(0); input.enabled = false; pilot = race.vehicle.gameObject.AddComponent<RoadDriver>(); pilot.Initialize(race, race.vehicle, true, 1, 1); pilot.Racer = race.Racers[0];
                }
            }
            if (respawn) respawn.Respawned -= onReset; Throttle(0);
            if (pilot) Destroy(pilot); if (input) input.enabled = true;
            bool ok = true;
            for (int i = 0; i < racers.Count; i++)
            {
                float d = Vector3.Distance(racers[i].Car.Body.position, start[i]); int n = resets[i] + (i == 0 ? playerResets : 0);
                bool moved = at3[i] > 5 && d > 25 && n == 0; ok &= moved;
                Note($"  {(moved ? "moved" : "STUCK / RESET")} {racers[i].Name}: {at3[i]:F1} m in 3 s, {d:F1} m in 10 s, resets {n}");
            }
            Note($"  the player's throttle as read from the controller in the first second: {throttleRead:F2}");
            ok &= overlaps == 0; if (!ok) failures++;
            Note($"{(ok ? "PASS" : "FAIL")} {label}: {racers.Count} vehicles, grid overlaps {overlaps}, all moved at GO with no reset in 10 s: {ok}");
        }
    }
}
