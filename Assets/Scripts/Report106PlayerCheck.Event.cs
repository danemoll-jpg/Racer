using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Racer
{
    // 0.106 evidence only: the Stunt event with a bot, the setup page with the controller only, the frame times (Report106PlayerCheck)
    public sealed partial class Report106PlayerCheck
    {
        // ---------------------------------------------------------------- Part E 3: the Stunt event, a bot on the park's circuit for the whole 10 minutes
        static readonly (float u, float v, float cap, string trick)[] Circuit =
        {
            (140, 20, 15, ""), (110, 20, 15, ""), (85, 20, 15, ""), (55, 20, 16, "back"), (22, 22, 12, ""), (11, 34, 11, ""), (10, 50, 16, ""), (10, 90, 15, ""),
            (12, 118, 12, ""), (22, 134, 11, ""), (34, 138, 15, ""), (70, 138, 17, ""), (100, 138, 18, ""), (118, 136, 18, ""), (128, 131.5f, 14, ""), (140, 128, 11, ""),
            (146.5f, 118, 13, ""), (146.5f, 50, 14, ""), (146.5f, 34, 11, ""), (150, 22, 10, ""),
        };
        readonly List<string> resetLog = new();
        sealed class Bot { public Gamepad pad; public ArcadeVehicle car; public int wp = 1, resets; public float slowSince = -1, waitUntil, airStart, airEstimate = 1; public bool air; public Vector3 turn; public string trick = ""; }
        void BotStep(Bot b)
        {
            var car = b.car; if (!car) return; var p = car.Body.position;
            if (Time.time < b.waitUntil) { InputSystem.QueueStateEvent(b.pad, new GamepadState()); return; }
            var w = Circuit[b.wp]; var target = PP(w.u, w.v);
            if (Vector3.Distance(Flat(p), Flat(target)) < 7) { b.trick = w.trick; b.wp = (b.wp + 1) % Circuit.Length; w = Circuit[b.wp]; target = PP(w.u, w.v); }
            bool off = car.GroundedWheels == 0;
            if (off && !b.air) { b.air = true; b.airStart = Time.time; b.turn = Vector3.zero; float vy = car.Body.linearVelocity.y, drop = Mathf.Max(0, p.y - (StuntPark.FloorY + 1.6f)); b.airEstimate = (vy + Mathf.Sqrt(Mathf.Max(0, vy * vy + 2 * 9.81f * drop))) / 9.81f; }
            if (!off && car.GroundedWheels >= 2 && b.air) { b.air = false; b.trick = ""; }
            float tip = 0;
            if (b.air)
            {
                b.turn += car.transform.InverseTransformDirection(car.Body.angularVelocity) * (Time.deltaTime * Mathf.Rad2Deg);
                if (b.trick == "back" && b.airEstimate > 1.3f) { float left = Mathf.Max(.12f, b.airEstimate * .82f - (Time.time - b.airStart)); tip = -Mathf.Clamp((360 + b.turn.x) / left / (ArcadeVehicle.AirTipRate * Mathf.Rad2Deg), -1, 1); }
            }
            float ang = Vector3.SignedAngle(Flat(car.transform.forward), Flat(target - p), Vector3.up); float speed = Vector3.Dot(car.Body.linearVelocity, car.transform.forward);
            float cap = Mathf.Abs(ang) > 50 ? Mathf.Min(w.cap, 8) : w.cap;
            float steer = off ? 0 : Mathf.Clamp(ang / 25, -1, 1);
            InputSystem.QueueStateEvent(b.pad, new GamepadState { leftStick = new Vector2(steer, off ? tip : 0), rightTrigger = speed < cap ? 1 : 0, leftTrigger = speed > cap + 1.5f ? Mathf.Clamp01((speed - cap) / 5) : 0 });
            // stuck or on its roof: the reset button, then on from the nearest point of the circuit
            bool stuck = car.Body.linearVelocity.magnitude < 1.5f || car.GetComponent<VehicleConfiguration>().WipedOut;
            if (!stuck) b.slowSince = -1; else if (b.slowSince < 0) b.slowSince = Time.time;
            if (b.slowSince > 0 && Time.time - b.slowSince > 2.5f)
            {
                b.slowSince = -1; b.resets++; b.waitUntil = Time.time + 1.6f; resetLog.Add($"reset at {V(p)} (park u {p.x - StuntPark.X0:F0}, v {p.z - StuntPark.Z0:F0}), heading to point {b.wp}, wiped out {car.GetComponent<VehicleConfiguration>().WipedOut}");
                InputSystem.QueueStateEvent(b.pad, new GamepadState().WithButton(GamepadButton.North));
                StartCoroutine(AfterReset(b));
            }
        }
        IEnumerator AfterReset(Bot b)
        {
            yield return new WaitForSeconds(1.5f);
            var p = b.car.Body.position; int best = 0; float d = float.MaxValue;
            for (int i = 0; i < Circuit.Length; i++) { float x = Vector3.Distance(Flat(p), Flat(PP(Circuit[i].u, Circuit[i].v))); if (x < d) { d = x; best = i; } }
            b.wp = (best + 1) % Circuit.Length;
        }
        IEnumerator Event()
        {
            int players = int.Parse(Arg("-p106players", "1")); float limit = F(Arg("-p106limit", "-1")); if (limit > 0) StuntEvent.TestLimit = limit;
            var pad2 = players > 1 ? InputSystem.AddDevice<Gamepad>("Report106Pad2") : null;
            Conditions(); flow.Save.Settings.hints = false; flow.Save.SaveSettings();
            SplitScreen.Mode = SplitScreen.Kind.Stunt; SplitScreen.Solo = players == 1; SplitScreen.P1Device = pad; SplitScreen.P2Device = pad2; SplitScreen.P2Ai = players == 1;
            SplitScreen.P1Vehicle = Arg("-p106vehicle", "stuntbuggy"); SplitScreen.P2Vehicle = "stuntbuggy"; SplitScreen.FromRoam = false; SplitScreen.Time = TimeOfDay.Day; SplitScreen.Weather = Weather.Clear;
            if (players > 1) { SplitScreen.P1Name = "Player One"; SplitScreen.P2Pick = "Player Two"; }
            Note($"Stunt event: {players} player(s), {VehicleProfile.Find(SplitScreen.P1Vehicle).Name}{(players > 1 ? " and " + VehicleProfile.Find(SplitScreen.P2Vehicle).Name : "")}, round {(StuntEvent.TestLimit > 0 ? StuntEvent.TestLimit + " s (shortened)" : StuntProgress.Minutes + " minutes")}; medal targets {string.Join(" / ", StuntProgress.Targets)}");
            flow.StartSplit(); float t0 = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - t0 < 150) { Bind(); if (flow && StuntEvent.Current && StuntEvent.Current.State == StuntEvent.Phase.Running) break; yield return null; }
            var ev = StuntEvent.Current; if (!ev) { Note("the event did not start"); yield break; }
            AudioListener.volume = 0; WorldLook.Current?.SetClock(WorldLook.Current.Day, 12);
            var bots = new List<Bot> { new() { pad = pad, car = race.vehicle } };
            if (players > 1 && SplitRoam.Current && SplitRoam.Current.Car) bots.Add(new Bot { pad = pad2, car = SplitRoam.Current.Car, waitUntil = Time.time + 6 });
            Note($"running: player 1 at {V(race.vehicle.Body.position)}{(bots.Count > 1 ? ", player 2 at " + V(bots[1].car.Body.position) : "")}; screen {(SplitScreen.OneView ? "one view" : "split")}");
            yield return Snap("E3-event-start-" + players + "p");
            bool paused = false; float nextLog = 60, shot = 120;
            while (ev && ev.State != StuntEvent.Phase.Done)
            {
                yield return null; // per frame (the results stop the physics steps, so never wait for one)
                AudioListener.volume = 0; if (flow.State == RaceFlow.Stage.Racing) foreach (var b in bots) BotStep(b);
                if (ev.Clock >= nextLog) { nextLog += 60; Note($"  {(int)(ev.Clock / 60)}:{(int)(ev.Clock % 60):00}  " + string.Join("   ", ev.Riders.Select(r => $"P{r.player} {r.points:N0} pts, {r.combos} combos, best {r.best:N0}, tricks {r.acts.Tricks}, bails {r.acts.Bails}, loops {r.acts.Loops}")) + "   resets " + string.Join("/", bots.Select(b => b.resets))); }
                if (ev.Clock >= shot) { shot = float.MaxValue; yield return Snap("E3-event-driving-" + players + "p"); }
                if (!paused && ev.Clock >= (StuntEvent.TestLimit > 0 ? StuntEvent.TestLimit / 2 : 300))
                {
                    paused = true; float c0 = ev.Clock; var pts0 = ev.Riders.Select(r => r.points).ToArray();
                    foreach (var b in bots) InputSystem.QueueStateEvent(b.pad, new GamepadState());
                    flow.Pause(); yield return new WaitForSecondsRealtime(6); yield return Snap("E3-event-paused-" + players + "p");
                    float c1 = ev.Clock; var pts1 = ev.Riders.Select(r => r.points).ToArray(); string state = flow.State.ToString();
                    flow.Resume(); yield return new WaitForSecondsRealtime(.5f);
                    Note($"pause at {c0:F2} s for 6 s (state {state}): clock after {c1:F2} s, points {string.Join("/", pts0)} -> {string.Join("/", pts1)}: {(Mathf.Abs(c1 - c0) < .01f && pts0.SequenceEqual(pts1) ? "kept" : "CHANGED")}; resumed, state {flow.State}");
                }
            }
            foreach (var b in bots) InputSystem.QueueStateEvent(b.pad, new GamepadState());
            t0 = Time.realtimeSinceStartup; while (Time.realtimeSinceStartup - t0 < 15 && flow.State != RaceFlow.Stage.Results) yield return null;
            yield return new WaitForSecondsRealtime(1.5f);
            Note($"finished at {ev.Clock:F1} s; state {flow.State}; winner {(ev.Winner == 0 ? (ev.Riders.Count > 1 ? "a draw" : "solo") : SplitScreen.NameOf(ev.Winner))}");
            foreach (var r in ev.Riders) Note($"  {SplitScreen.NameOf(r.player)}: {r.points:N0} pts, medal {new[] { "none", "Bronze", "Silver", "Gold" }[r.medal]}, Top 10 #{r.place}, combos {r.combos}, best combo {r.best:N0}, outside the park {r.outside}, tricks {r.acts.Tricks}, bails {r.acts.Bails}, loops {r.acts.Loops}");
            foreach (var r in ev.Riders) File.WriteAllLines(Path.Combine(outDir, $"event-{players}p-p{r.player}-combos.txt"), r.log);
            File.WriteAllLines(Path.Combine(outDir, $"event-{players}p-bot-resets.txt"), resetLog);
            yield return Snap("E3-event-results-" + players + "p");
            var texts = FindObjectsByType<UnityEngine.UI.Text>(FindObjectsSortMode.None).Where(t => t.isActiveAndEnabled && !string.IsNullOrWhiteSpace(t.text)).Select(t => t.text.Replace("\n", " | ")).ToList();
            File.WriteAllLines(Path.Combine(outDir, $"event-{players}p-results-texts.txt"), texts);
            var file = Path.Combine(flow.Save.DirectoryPath, StuntProgress.File); Note($"test save {file}: {(File.Exists(file) ? File.ReadAllText(file).Replace("\n", " ").Replace("  ", "") : "missing")}");
            Note($"stunt motorcycle earned: {StuntProgress.GoldEarned}; the save is the test copy ({flow.Save.DirectoryPath})");
            yield return new WaitForSecondsRealtime(1);
        }

        // ---------------------------------------------------------------- Part D: the setup page with the controller only, the Records tab
        IEnumerator Press(GamepadButton button, int times = 1)
        {
            for (int i = 0; i < times; i++) { InputSystem.QueueStateEvent(pad, new GamepadState().WithButton(button)); yield return new WaitForSecondsRealtime(.12f); InputSystem.QueueStateEvent(pad, new GamepadState()); yield return new WaitForSecondsRealtime(.3f); }
        }
        static string Focus => UnityEngine.EventSystems.EventSystem.current?.currentSelectedGameObject?.name ?? "(none)";
        string Page => FindAnyObjectByType<RaceMenus>()?.PageName ?? "?";
        IEnumerator FocusRow(string id, int max = 24) { for (int i = 0; i < max && Focus != id; i++) yield return Press(GamepadButton.DpadDown); }
        bool Shown(string start) => FindObjectsByType<UnityEngine.UI.Text>(FindObjectsSortMode.None).Any(t => t.isActiveAndEnabled && t.text.StartsWith(start));
        IEnumerator Menus()
        {
            Conditions(); yield return new WaitForSecondsRealtime(1.5f);
            Note($"start: state {flow.State}, page '{Page}', focus '{Focus}'");
            yield return FocusRow("roam"); Note($"main menu: focus '{Focus}'"); yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(.6f);
            yield return FocusRow("stunt"); Note($"Free Roam page '{Page}': focus '{Focus}'"); yield return Snap("D-free-roam-page");
            yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(1f); Note($"after A: page '{Page}', focus '{Focus}'"); yield return Snap("D-stunt-setup");
            // every row by D-pad down, round to the first again
            var seen = new List<string>(); string first = Focus;
            for (int i = 0; i < 30; i++) { yield return Press(GamepadButton.DpadDown); if (Focus == first) break; seen.Add(Focus); }
            var active = FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None).Where(b => b.isActiveAndEnabled && b.interactable && b.navigation.mode == UnityEngine.UI.Navigation.Mode.Explicit).Select(b => b.name).Distinct().ToList();
            var all = seen.Prepend(first).ToList();
            Note($"stunt setup rows reached by D-pad down from '{first}': {string.Join(", ", all)}; interactable rows on the page: {string.Join(", ", active)}; not reached: {string.Join(", ", active.Except(all))}");
            yield return FocusRow("stunt-players"); string v0 = SplitScreen.Solo ? "1" : "2"; yield return Press(GamepadButton.DpadRight); Note($"Players row: right -> {(SplitScreen.Solo ? "1" : "2")} player(s) (was {v0})"); yield return Snap("D-stunt-setup-two-players");
            yield return FocusRow("stunt-players"); yield return Press(GamepadButton.DpadRight); Note($"Players row: right again -> {(SplitScreen.Solo ? "1" : "2")}");
            yield return FocusRow("stunt-p1-vehicle"); string veh = SplitScreen.P1Vehicle; yield return Press(GamepadButton.DpadRight); Note($"Vehicle row: right -> {SplitScreen.P1Vehicle} (was {veh})"); yield return Snap("D-stunt-setup-vehicle"); yield return Press(GamepadButton.DpadLeft); Note($"Vehicle row: left -> {SplitScreen.P1Vehicle}");
            yield return Press(GamepadButton.East); yield return new WaitForSecondsRealtime(.6f); Note($"after B: page '{Page}', focus '{Focus}'");
            yield return Press(GamepadButton.East); yield return new WaitForSecondsRealtime(.6f); Note($"after B: page '{Page}', focus '{Focus}'");
            // Records > STUNTS (RB steps the tabs)
            yield return FocusRow("records"); yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(1f);
            for (int i = 0; i < 6 && !Shown("✓ STUNTS"); i++) yield return Press(GamepadButton.RightShoulder);
            Note($"Records: STUNTS tab {(Shown("✓ STUNTS") ? "shown" : "NOT shown")}"); yield return Snap("D-records-stunts");
            yield return Press(GamepadButton.East); yield return new WaitForSecondsRealtime(.6f); Note($"after B: state {flow.State}, page '{Page}'");
            // in Free Roam at the park: the garage's stunt vehicles, the prompt, and the pause menu's STUNT EVENT row
            yield return ToFreeRoam("stuntbuggy"); var car = race.vehicle;
            // the garage: the buggy chosen; then right along the vehicle row to the stunt motorcycle (locked: its goal shown)
            flow.Pause(); flow.OpenGarage(); flow.SelectVehicle("stuntbuggy"); yield return new WaitForSecondsRealtime(1.2f);
            string Title() => FindObjectsByType<UnityEngine.UI.Text>(FindObjectsSortMode.None).Where(t => t.isActiveAndEnabled && (t.text.Contains(" / Buggy") || t.text.Contains(" / Motorcycle") || t.text.Contains(" / Car") || t.text.Contains(" / ATV") || t.text.Contains(" / Mower"))).Select(t => t.text).FirstOrDefault() ?? "";
            Note($"garage: the stunt buggy -> config {race.vehicle.GetComponent<VehicleConfiguration>().profileId}; title '{Title()}'"); yield return Snap("D-garage-stuntbuggy");
            for (int i = 0; i < 16 && !Title().StartsWith("Skywhip"); i++) yield return Press(GamepadButton.DpadRight);
            Note($"garage: right to '{Title()}': '{FindObjectsByType<UnityEngine.UI.Text>(FindObjectsSortMode.None).Where(t => t.isActiveAndEnabled && t.text.StartsWith("LOCKED")).Select(t => t.text.Replace("\n", " | ")).FirstOrDefault()}'"); yield return Snap("D-garage-stuntbike-locked");
            flow.CloseGarage(); flow.Resume(); yield return new WaitForSecondsRealtime(.6f);
            yield return Vehicle("stuntbuggy"); car = race.vehicle; Put(car, StuntPark.Entrance, 270, 0); yield return new WaitForSeconds(1.5f);
            Note($"at the entrance, the prompt: '{flow.Activities.Hud.Replace("\n", " | ")}'"); yield return Snap("D-entrance-prompt");
            yield return Press(GamepadButton.Start); yield return new WaitForSecondsRealtime(1f); Note($"Start: state {flow.State}, page '{Page}', focus '{Focus}'");
            yield return FocusRow("stunt"); Note($"pause menu focus '{Focus}'"); yield return Snap("D-roam-menu-stunt-row");
            yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(.6f); Note($"A: page '{Page}', focus '{Focus}'"); yield return Snap("D-roam-confirm");
            yield return FocusRow("confirm", 3); yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(.8f); Note($"A on '{Focus}': page '{Page}'"); yield return Snap("D-roam-stunt-setup");
            yield return Press(GamepadButton.East); yield return new WaitForSecondsRealtime(.6f); Note($"B: page '{Page}', state {flow.State}");
        }

        // ---------------------------------------------------------------- Part E 4: a Street Loop lap each way past the park (rivals, traffic)
        IEnumerator Lap()
        {
            int course = int.Parse(Arg("-p106course", "0")); string scene = course == 0 ? "StreetLoopGreybox" : "StreetLoopReverse"; Campaign.Testing = true;
            for (int attempt = 0; attempt < 4 && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != scene; attempt++)
            {
                if (flow.State == RaceFlow.Stage.Title) flow.EnterMenuAfterTitle();
                flow.OpenCourses(); yield return null; flow.SelectCourse(false, course == 1);
                float t1 = Time.realtimeSinceStartup; while (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != scene && Time.realtimeSinceStartup - t1 < 40) yield return null;
            }
            yield return WaitFlow(); Conditions();
            flow.OpenGarage(); flow.SelectVehicle(Arg("-p106vehicle", "original")); flow.CloseGarage();
            race.opponents = true; race.traffic = true; race.laps = 1; race.difficulty = 1;
            flow.StartRace(); float t0 = Time.realtimeSinceStartup;
            while (flow.State != RaceFlow.Stage.Racing && Time.realtimeSinceStartup - t0 < 60) { AudioListener.volume = 0; yield return null; }
            yield return new WaitForSecondsRealtime(4);
            Note($"{scene}: park built {(StuntPark.Current ? StuntPark.Current.Report.LastOrDefault() : "no")}; cleared {StuntPark.Cleared.LastOrDefault()}");
            var car = race.vehicle; var pilot = car.gameObject.AddComponent<RoadDriver>(); pilot.Initialize(race, car, true, 1, 1); pilot.Racer = race.Racers[0]; car.GetComponent<VehicleInput>().enabled = false;
            var contacts = new List<Report106Contacts>();
            foreach (var r in race.Racers) { var c = r.Car.gameObject.AddComponent<Report106Contacts>(); c.who = r.Name + (r.IsAi ? " (AI)" : " (player)"); contacts.Add(c); }
            foreach (var d in race.Drivers.Where(d => d && d.GetComponent<AmbientVehicle>())) { var c = d.gameObject.AddComponent<Report106Contacts>(); c.who = "traffic " + d.name; contacts.Add(c); }
            var recov = race.Racers.ToDictionary(r => r, r => r.Recoveries); var where = new List<string>();
            Time.timeScale = 3; float real0 = Time.realtimeSinceStartup;
            while (flow.State == RaceFlow.Stage.Racing && Time.realtimeSinceStartup - real0 < 900)
            {
                AudioListener.volume = 0; yield return null; if (Time.timeScale == 0) Time.timeScale = 3;
                foreach (var r in race.Racers) if (r.Recoveries > recov[r]) { recov[r] = r.Recoveries; where.Add($"reset: {r.Name}{(r.IsAi ? " (AI)" : "")} at {V(r.Car.Body.position)}"); }
            }
            Time.timeScale = 1;
            var park = contacts.SelectMany(c => c ? c.list : new List<string>()).Where(t => t.Contains("'Stunt park")).ToList();
            Note($"{scene} lap: rivals {race.Racers.Count - 1}, traffic {race.traffic} ({race.Drivers.Count(d => d && d.GetComponent<AmbientVehicle>())} cars); finished {flow.State == RaceFlow.Stage.Results}; resets {race.Racers.Sum(r => r.Recoveries)}; touches of the stunt park by racers or traffic: {park.Count}");
            foreach (var w in where) Note("  " + w); foreach (var t in park.Take(20)) Note("    " + t);
            yield return new WaitForSecondsRealtime(3); yield return Snap($"E4-results-{scene}");
        }

        // ---------------------------------------------------------------- Part A 6: frame times at the park (worst view), full screen and split-screen
        static readonly (string name, Vector3 at, float yaw)[] PerfViews =
        {
            ("drive mouth, looking up the drive", new Vector3(120, 9, 551), 0), ("start line, looking west", StuntPark.P(152, 20), 270), ("north-west corner, across the park", StuntPark.P(16, 146), 135),
            ("by the loop, looking west", StuntPark.P(132, 140), 270), ("half-pipe north end, looking south", StuntPark.P(146.5f, 128), 180), ("Hwy 92 west of the drive, eastbound", new Vector3(40, 9, 533), 80),
        };
        IEnumerator Perf()
        {
            bool split = Arg("-p106split", "0") == "1"; var pad2 = split ? InputSystem.AddDevice<Gamepad>("Report106Pad2") : null;
            Conditions(); QualitySettings.vSyncCount = 0; Application.targetFrameRate = -1;
            if (split)
            {
                SplitScreen.Mode = SplitScreen.Kind.FreeRoam; SplitScreen.Solo = false; SplitScreen.P1Device = pad; SplitScreen.P2Device = pad2; SplitScreen.P2Ai = false; SplitScreen.P1Vehicle = "original"; SplitScreen.P2Vehicle = "atv";
                SplitScreen.Time = TimeOfDay.Day; SplitScreen.Weather = Weather.Clear; SplitScreen.Traffic = true; flow.StartSplit(); float t0 = Time.realtimeSinceStartup;
                while (Time.realtimeSinceStartup - t0 < 150) { Bind(); if (flow && SplitRoam.Current && SplitRoam.Current.Car && flow.State == RaceFlow.Stage.Racing) break; yield return null; }
            }
            else yield return ToFreeRoam("original");
            QualitySettings.vSyncCount = 0; Application.targetFrameRate = -1; AudioListener.volume = 0; WorldLook.Current?.SetClock(WorldLook.Current.Day, 12);
            var results = new List<(string, float)>();
            foreach (var (name, at, yaw) in PerfViews)
            {
                var car = race.vehicle; Put(car, at, yaw, 0); car.Body.isKinematic = true; FindAnyObjectByType<ChaseCamera>()?.Snap();
                if (split && SplitRoam.Current && SplitRoam.Current.Car) { var c2 = SplitRoam.Current.Car; Put(c2, at + Quaternion.Euler(0, yaw, 0) * new Vector3(4, 0, -6), yaw, 0); c2.Body.isKinematic = true; }
                yield return new WaitForSecondsRealtime(2);
                int frames = 0; float t0 = Time.realtimeSinceStartup; while (Time.realtimeSinceStartup - t0 < 3) { frames++; yield return null; }
                float ms = (Time.realtimeSinceStartup - t0) * 1000 / frames; results.Add((name, ms));
                Note($"{name}: {ms:F2} ms per frame ({frames} frames in 3 s)"); if (name.StartsWith("north-west")) yield return Snap("A6-perf-" + (split ? "split" : "full") + (StuntPark.Disabled ? "-before" : "-after"));
                car.Body.isKinematic = false; if (split && SplitRoam.Current && SplitRoam.Current.Car) SplitRoam.Current.Car.Body.isKinematic = false;
            }
            var worst = results.OrderByDescending(r => r.Item2).First();
            Note($"worst view: {worst.Item1} {worst.Item2:F2} ms; all views {results.Average(r => r.Item2):F2} ms ({(split ? "split-screen" : "full screen")} {Screen.width}x{Screen.height}, park {(StuntPark.Disabled ? "off (before)" : "on")})");
        }
    }
}
