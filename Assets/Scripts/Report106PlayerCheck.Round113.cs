using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Racer
{
    // 0.113 evidence only, on the 0.106 check (same switches: -p106check <dir> -racerTestSave <dir>; never active without both):
    //   -p106kind ai113     AI-only races, the player's vehicle parked out of the race (as 0.112's pace112):
    //                       -p113race "c1-wrong-way" (a campaign event; -p112set overrides as in 0.112) or "quick:<course>:<rivals a+b+c>:<laps>:<difficulty>"
    //                       (a quick race from the menus on RacePlaylists.Scenes[course]); -p113runs 5; -p113force "Laurel Switchbacks|..." makes every
    //                       rival take those shortcuts (ShortcutStrategy.Forced, checks only); -p113trace 1: each rival's line on a shortcut, every 0.5 s.
    //                       One line per rival per run: vehicle, time, resets (and where), shortcuts taken.
    public sealed partial class Report106PlayerCheck
    {
        string Rp(RacerState r) => r.Car.GetComponent<VehicleConfiguration>().profileId;
        IEnumerator QuickTo(int course)
        {
            Campaign.Testing = true;
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == RacePlaylists.Scenes[course]) yield break;
            if (flow.State == RaceFlow.Stage.Title) flow.EnterMenuAfterTitle();
            flow.OpenCourses(); yield return null; flow.SelectCourseEntry(course);
            float t1 = Time.realtimeSinceStartup; while (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != RacePlaylists.Scenes[course] && Time.realtimeSinceStartup - t1 < 60) yield return null;
            yield return WaitMenu(); Bind();
        }
        // a quick race with these rivals (the player's choice of vehicle kept); from the menu, or `retry` = the results screen's Retry
        void QuickStart(string[] rivals, int laps, int difficulty)
        {
            bool traffic = Arg("-p113traffic", "0") == "1"; // with traffic (0.113 Part C laps)
            var st = flow.Save.Settings; st.opponents = true; st.traffic = traffic; st.opponentRoster = (string[])rivals.Clone(); st.laps = laps; st.difficulty = difficulty; st.estimateAiFinishes = false; flow.Save.SaveSettings();
            race.opponents = true; race.traffic = traffic; race.opponentRoster = (string[])rivals.Clone(); race.laps = laps; race.difficulty = difficulty;
            flow.StartRace();
        }
        // parks the player's vehicle out of the race and times the rivals to the line; returns per-rival rows
        IEnumerator RunField(List<string> outRows, string tag, bool trace, float firstSeconds = 10)
        {
            yield return new WaitForSecondsRealtime(.5f); yield return WaitRacing(); Bind();
            var me = race.vehicle; me.Body.isKinematic = true; foreach (var c in me.GetComponentsInChildren<Collider>()) c.enabled = false;
            var vi = me.GetComponent<VehicleInput>(); if (vi) vi.enabled = false; var respawn = me.GetComponent<VehicleRespawn>(); if (respawn) respawn.enabled = false;
            race.finishGraceSeconds = 100000;
            var ai = race.Racers.Where(r => r.IsAi).ToList(); var reset0 = ai.ToDictionary(r => r, r => r.Recoveries); var resetAt = ai.ToDictionary(r => r, r => new List<string>());
            var shortcuts = ai.ToDictionary(r => r, r => new List<string>()); var on = ai.ToDictionary(r => r, r => (WoodlandRoute)null); var early = ai.ToDictionary(r => r, r => 0);
            var traces = ai.ToDictionary(r => r, r => new List<string>()); var nextTrace = ai.ToDictionary(r => r, r => 0f);
            // -p113watch "x,z,r": every rival within r m of (x, z), every 0.25 s (position, speed, target, branch, steering hint)
            var watchArg = Arg("-p113watch", ""); Vector3 watch = default; float watchR = 0; if (watchArg.Length > 0) { var w3 = watchArg.Split(','); watch = new Vector3(F(w3[0]), 0, F(w3[1])); watchR = F(w3[2]); }
            var watched = ai.ToDictionary(r => r, r => new List<string>()); var nextWatch = ai.ToDictionary(r => r, r => 0f);
            Time.timeScale = 6; float t0 = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - t0 < 1500 && flow.State == RaceFlow.Stage.Racing && !ai.All(r => r.Progress.Finished || r.Dnf))
            {
                AudioListener.volume = 0; yield return null; if (Time.timeScale == 0) Time.timeScale = 6;
                float clock = (float)race.Progress.RaceTime(race.Clock);
                foreach (var r in ai)
                {
                    if (r.Recoveries - reset0[r] > resetAt[r].Count) { resetAt[r].Add($"[{r.Car.GetComponent<RoadDriver>()?.LastRecoveryWhy}] {V(r.Car.Body.position)} {(r.Branch.Route ? r.Branch.Route.title + " s " + r.Branch.Route.Project(r.Car.Body.position, out _).ToString("F0") : "road " + r.RoadPosition.ToString("F0"))} t {clock:F0}"); if (clock < firstSeconds) early[r]++; }
                    if (r.Branch.Route != on[r]) { on[r] = r.Branch.Route; if (on[r]) shortcuts[r].Add(on[r].title + " lap " + (r.Progress.CompletedLaps + 1)); }
                    if (watchR > 0 && Time.time >= nextWatch[r] && Vector3.Distance(Flat(r.Car.Body.position), watch) < watchR && watched[r].Count < 400)
                    {
                        nextWatch[r] = Time.time + .25f; var d = r.Car.GetComponent<RoadDriver>();
                        watched[r].Add(string.Format(CultureInfo.InvariantCulture, "t{0:F1} {1} v{2:F1} tgt{3:F1} thr{4:F2} brk{5:F2} {6} obst[{7}] road {8:F0} hdg {9:F0}", clock, V(r.Car.Body.position), r.Car.ForwardSpeed, d ? d.TargetSpeed : -1, d ? d.LastThrottle : -1, d ? d.LastBrake : -1, r.Branch.Route ? r.Branch.Route.title : "main", d ? d.LastObstacle : "", r.RoadPosition, Yaw(r.Car.transform.forward)));
                    }
                    if (trace && r.Branch.Route && Time.time >= nextTrace[r])
                    {
                        nextTrace[r] = Time.time + .5f; var d = r.Car.GetComponent<RoadDriver>(); float bs = r.Branch.Route.Project(r.Car.Body.position, out float lat);
                        traces[r].Add(string.Format(CultureInfo.InvariantCulture, "s{0:F0} y{1:F1} lat{2:F1} v{3:F1} tgt{4:F1} w{5} up{6:F2}", bs, r.Car.Body.position.y - r.Branch.Route.At(bs, out _).y, lat, r.Car.ForwardSpeed, d ? d.TargetSpeed : -1, r.Car.GroundedWheels, r.Car.transform.up.y));
                    }
                }
            }
            Time.timeScale = 1;
            foreach (var r in ai.OrderBy(r => r.Progress.Finished ? r.ClassifiedTime(race.Clock) : 1e9))
            {
                outRows.Add(string.Format(CultureInfo.InvariantCulture, "  {0} {1,-10} {2} resets {3} (first {4:0} s: {5}) shortcuts [{6}]{7}", tag, Rp(r),
                    r.Progress.Finished ? r.ClassifiedTime(race.Clock).ToString("0.0", CultureInfo.InvariantCulture) : (r.Dnf ? "DNF" : "unfinished"), r.Recoveries - reset0[r], firstSeconds, early[r], string.Join("; ", shortcuts[r]),
                    resetAt[r].Count > 0 ? " | resets at " + string.Join("; ", resetAt[r].Take(12)) + (resetAt[r].Count > 12 ? " ..." : "") : ""));
                if (trace && traces[r].Count > 0) outRows.Add("      trace " + Rp(r) + ": " + string.Join(" | ", traces[r].Take(160)));
                if (watched[r].Count > 0) outRows.Add("      watch " + Rp(r) + ": " + string.Join(" | ", watched[r].Take(200)));
            }
            race.finishGraceSeconds = 0; float w = Time.realtimeSinceStartup; while (flow.State != RaceFlow.Stage.Results && Time.realtimeSinceStartup - w < 30) yield return null;
        }
        // ---------------------------------------------------------------- Part C: Trickum Rd and S Cherokee Ln (Free Roam)
        //   -p106kind shots113  driver's-eye views along both stretches both ways (1.4 m up, in the right lane, 40 m ahead) and top-down tiles;
        //                       -p113tag before|after; -p113off hides the Trickum plan (Hwy92Roadside.TrickumOff, checks only) for the "before"
        //   -p106kind perf113   worst-view frame time at the views (-p106split 1: split-screen Free Roam), -p113off for the before
        static readonly (string name, Vector2 at, Vector2 f)[] TrickumViews = {
            ("T150a", new Vector2(-627.5f, 443.4f), new Vector2(-0.200f, -0.980f)), ("T150b", new Vector2(-624.2f, 442.7f), new Vector2(0.200f, 0.980f)),
            ("T300a", new Vector2(-634.4f, 293.4f), new Vector2(0.016f, -1.000f)), ("T300b", new Vector2(-631.0f, 293.5f), new Vector2(-0.016f, 1.000f)),
            ("T450a", new Vector2(-632.2f, 143.5f), new Vector2(0.016f, -1.000f)), ("T450b", new Vector2(-628.8f, 143.5f), new Vector2(-0.016f, 1.000f)),
            ("T600a", new Vector2(-629.5f, -6.5f), new Vector2(0.018f, -1.000f)), ("T600b", new Vector2(-626.1f, -6.5f), new Vector2(-0.018f, 1.000f)),
            ("T750a", new Vector2(-627.2f, -156.5f), new Vector2(0.014f, -1.000f)), ("T750b", new Vector2(-623.8f, -156.5f), new Vector2(-0.014f, 1.000f)),
            ("T900a", new Vector2(-624.8f, -306.5f), new Vector2(0.021f, -1.000f)), ("T900b", new Vector2(-621.4f, -306.4f), new Vector2(-0.021f, 1.000f)),
            ("T1050a", new Vector2(-621.5f, -456.5f), new Vector2(0.031f, -1.000f)), ("T1050b", new Vector2(-618.1f, -456.4f), new Vector2(-0.031f, 1.000f)),
            ("C60a", new Vector2(-556.3f, -552.9f), new Vector2(0.999f, 0.045f)), ("C60b", new Vector2(-556.5f, -549.5f), new Vector2(-0.999f, -0.045f)),
            ("C160a", new Vector2(-456.6f, -554.2f), new Vector2(0.997f, -0.078f)), ("C160b", new Vector2(-456.4f, -550.8f), new Vector2(-0.997f, 0.078f)),
            ("C260a", new Vector2(-357.2f, -564.0f), new Vector2(0.993f, -0.116f)), ("C260b", new Vector2(-356.8f, -560.6f), new Vector2(-0.993f, 0.116f)),
            ("C330a", new Vector2(-288.0f, -574.3f), new Vector2(0.987f, -0.159f)), ("C330b", new Vector2(-287.5f, -571.0f), new Vector2(-0.987f, 0.159f)) };
        IEnumerator Shots113()
        {
            yield return ToFreeRoam("original"); string tag = Arg("-p113tag", Hwy92Roadside.TrickumOff ? "before" : "after");
            var plan = FindObjectsByType<Hwy92Roadside>(FindObjectsSortMode.None).FirstOrDefault(r => r.resource == Hwy92Roadside.Trickum);
            Note($"Trickum plan {(Hwy92Roadside.TrickumOff ? "off (before)" : "on")}: {(plan ? string.Join("; ", plan.Report) : "no component")}; removed {Hwy92Roadside.Removed.Count(r => r.StartsWith("Approximate"))} old placeholder houses");
            foreach (var c in FindObjectsByType<Canvas>(FindObjectsSortMode.None)) c.enabled = false;
            race.vehicle.Body.isKinematic = true; race.vehicle.transform.position += Vector3.down * 200;
            foreach (var (name, at, f) in TrickumViews)
            {
                var eye = new Vector3(at.x, TopAt(new Vector3(at.x, 40, at.y)) + 1.4f, at.y); var look = eye + new Vector3(f.x, 0, f.y) * 40 + Vector3.down * 1.2f;
                yield return Free($"C-{name}-{tag}", eye, look, 62, 1600, 900);
            }
            foreach (var (x, z) in new[] { (-625f, 330f), (-625f, 110f), (-622f, -110f), (-618f, -330f), (-600f, -520f), (-420f, -560f), (-300f, -565f) })
                yield return Free($"C-top-{x:0}_{z:0}-{tag}", new Vector3(x, 420, z), new Vector3(x, 0, z), 60, 1400, 1400, true, 115);
            foreach (var c in FindObjectsByType<Canvas>(FindObjectsSortMode.None)) c.enabled = true;
        }
        IEnumerator Perf113()
        {
            bool split = Arg("-p106split", "0") == "1"; var pad2 = split ? InputSystem.AddDevice<Gamepad>("Report113Pad2") : null;
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
            foreach (var (name, at, f) in TrickumViews.Where((v, i) => i % 2 == 0 || v.name.StartsWith("C")))
            {
                var car = race.vehicle; float yaw = Yaw(new Vector3(f.x, 0, f.y)); var pos = new Vector3(at.x, 0, at.y); Put(car, pos, yaw, 0); car.Body.isKinematic = true; FindAnyObjectByType<ChaseCamera>()?.Snap();
                if (split && SplitRoam.Current && SplitRoam.Current.Car) { var c2 = SplitRoam.Current.Car; Put(c2, pos + Quaternion.Euler(0, yaw, 0) * new Vector3(3, 0, -8), yaw, 0); c2.Body.isKinematic = true; }
                yield return new WaitForSecondsRealtime(2);
                int frames = 0; float t0 = Time.realtimeSinceStartup; while (Time.realtimeSinceStartup - t0 < 3) { frames++; yield return null; }
                float ms = (Time.realtimeSinceStartup - t0) * 1000 / frames; results.Add((name, ms)); Note($"{name}: {ms:F2} ms per frame");
                car.Body.isKinematic = false; if (split && SplitRoam.Current && SplitRoam.Current.Car) SplitRoam.Current.Car.Body.isKinematic = false;
            }
            var worst = results.OrderByDescending(r => r.Item2).First();
            Note($"worst view: {worst.Item1} {worst.Item2:F2} ms; all views {results.Average(r => r.Item2):F2} ms ({(split ? "split-screen" : "full screen")} {Screen.width}x{Screen.height}, Trickum plan {(Hwy92Roadside.TrickumOff ? "off (before)" : "on")})");
        }
        // ---------------------------------------------------------------- Part E: other vehicles' engines
        //   -p106kind audio113  four short clips (frames at 10 per second + the listener's mix as a WAV, joined later with ffmpeg): a Street Loop
        //                       Forward race where the player first passes the rivals then is passed by them, Free Roam traffic beside Hwy 92, a
        //                       Getaway at heat 3, a split-screen race; every 0.5 s the voices playing (car, distance, side, level, pitch). Then the
        //                       frame time with and without the other engines (-p106w 3840 -p106h 2160 for 4K; split-screen in the last clip).
        //                       The listener volume stays 0 (nothing reaches the speakers) unless the capture comes back empty: then 0.03, scaled.
        string Who(ArcadeVehicle c) => (c.GetComponent<AmbientVehicle>() ? "traffic" : c.GetComponent<CopDriver>() ? "cop" : "rival") + ":" + c.GetComponent<VehicleConfiguration>()?.profileId;
        IEnumerator Clip(string name, float seconds, ArcadeVehicle me)
        {
            var listener = FindAnyObjectByType<AudioListener>(); float vol = 0; string result = "";
            for (int attempt = 0; attempt < 2; attempt++)
            {
                AudioListener.volume = vol; var cap = listener.gameObject.AddComponent<CorrectionAudioCapture>(); cap.Begin(seconds + 1);
                var dir = System.IO.Path.Combine(outDir, name); System.IO.Directory.CreateDirectory(dir); int frame = 0; float next = 0, nextLog = 0, t0 = Time.realtimeSinceStartup; var lines = new List<string>();
                while (Time.realtimeSinceStartup - t0 < seconds)
                {
                    yield return new WaitForEndOfFrame(); AudioListener.volume = vol; float t = Time.realtimeSinceStartup - t0;
                    if (t >= next) { next += .1f; ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(dir, $"f{frame++:0000}.png")); }
                    if (t >= nextLog && OtherEngines.Current)
                    {
                        nextLog += .5f; var cam = Camera.main ? Camera.main.transform : listener.transform;
                        lines.Add($"t{t:F1} playing {OtherEngines.Current.Playing} [{OtherEngines.Current.Info}]: " + string.Join("; ", OtherEngines.Current.Voices.Select(v => { var d = v.car.Body.position - cam.position; return $"{Who(v.car)} {d.magnitude:F0} m {(Vector3.Dot(d, cam.right) > 0 ? "right" : "left")} level {v.level:F2} vol {v.idle + v.load:F3} pitch {v.pitch:F2}{(v.spatial < .5f ? " 2D" : "")}"; })));
                    }
                }
                result = cap.Finish(System.IO.Path.Combine(outDir, name + ".wav")); Destroy(cap);
                if (!result.Contains("peak=0.000000")) { foreach (var l in lines) Note("    " + l); break; }
                Note($"  {name}: capture empty at listener volume 0, again at 0.03 (scaled when joined)"); vol = .03f; foreach (var f in System.IO.Directory.GetFiles(System.IO.Path.Combine(outDir, name))) System.IO.File.Delete(f);
            }
            AudioListener.volume = 0; Note($"  clip {name}: {seconds:F0} s, listener volume {vol}, wav {result}");
        }
        IEnumerator FrameTime(string what, float seconds = 4)
        {
            QualitySettings.vSyncCount = 0; Application.targetFrameRate = -1;
            foreach (bool off in new[] { true, false })
            {
                OtherEngines.Disabled = off; yield return new WaitForSecondsRealtime(1); int frames = 0; float t0 = Time.realtimeSinceStartup;
                while (Time.realtimeSinceStartup - t0 < seconds) { frames++; yield return null; }
                Note($"  frame time {what} ({Screen.width}x{Screen.height}), other engines {(off ? "off" : "on")}: {(Time.realtimeSinceStartup - t0) * 1000 / frames:F2} ms");
            }
            OtherEngines.Disabled = false;
        }
        IEnumerator Audio113()
        {
            Campaign.Testing = true; Conditions(); var st = flow.Save.Settings; st.vehicle = .75f; flow.Save.SaveSettings();
            string clips = Arg("-p113clips", "A,B,C,D"); ArcadeVehicle me = null;
            // A: a race with rivals on Street Loop Forward: the player (driven by the road AI) passes them, then slows and they pass it
            if (clips.Contains("A")) {
            yield return QuickTo(0); Conditions(); flow.OpenGarage(); flow.SelectVehicle("original"); flow.CloseGarage();
            QuickStart(new[] { "pebble", "moto", "atv", "tourer" }, 1, 1); yield return WaitRacing(); Bind();
            me = race.vehicle; var pilot = me.gameObject.AddComponent<RoadDriver>(); pilot.Initialize(race, me, true, 1, 1.25f); pilot.Racer = race.Racers[0]; me.GetComponent<VehicleInput>().enabled = false;
            Note("A: Street Loop Forward race, 4 rivals (Pebble Coupe, Needle 600, Trail Four, Longroof GT), the player passing them (pace 1.25)");
            yield return Clip("E-A1-race-passing", 18, me);
            Destroy(pilot); yield return null; pilot = me.gameObject.AddComponent<RoadDriver>(); pilot.Initialize(race, me, true, 1, .55f); pilot.Racer = race.Racers[0];
            Note("A: then slowing (pace 0.55) so they pass"); yield return Clip("E-A2-race-passed", 18, me);
            yield return FrameTime("race, chase view");
            flow.QuitRace(); yield return WaitMenu(); }
            // B: Free Roam traffic, parked on the Hwy 92 shoulder (traffic on: the quick race above saved it off in the test save)
            flow.Save.Settings.traffic = true; flow.Save.SaveSettings(); if (race) race.traffic = true;
            yield return ToFreeRoam("original"); me = race.vehicle; Put(me, new Vector3(-120, 0, 548), 90, 0); me.Body.isKinematic = true; FindAnyObjectByType<ChaseCamera>()?.Snap();
            Note("B: Free Roam, parked on the Hwy 92 shoulder at (-120, 548) facing east"); yield return Clip("E-B-traffic", 20, me);
            me.Body.isKinematic = false;
            // C: a Getaway at heat 3 (the runner driven by the road AI)
            SplitScreen.P1Device = pad; SplitScreen.Solo = true; SplitScreen.P2Ai = true; SplitScreen.Mode = SplitScreen.Kind.Police; SplitScreen.PoliceGame = SplitScreen.Game.Getaway;
            SplitScreen.PoliceDifficulty = 1; SplitScreen.PoliceMinutes = 5; SplitScreen.Course = 0; SplitScreen.Time = TimeOfDay.Day; SplitScreen.Weather = Weather.Clear; SplitScreen.Traffic = true; SplitScreen.P1Vehicle = "original";
            flow.StartSplit(); float w0 = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - w0 < 150) { Bind(); if (flow && flow.Started && !LoadingScreen.Holding && flow.State == RaceFlow.Stage.Racing && GetawayChase.Current && GetawayChase.Current.Runners.Count > 0) break; yield return null; }
            var g = GetawayChase.Current; w0 = Time.time; while (g && g.State == GetawayChase.Phase.Starting && Time.time - w0 < 20) yield return null;
            if (g)
            {
                me = race.vehicle; var runner = me.gameObject.AddComponent<RoadDriver>(); runner.Initialize(race, me, false, 1, 1.4f); me.GetComponent<VehicleInput>().enabled = false;
                for (int i = 0; i < 2 && g.Heat < 3; i++) typeof(GetawayChase).GetMethod("BumpHeat").Invoke(g, new object[] { "more units on the way" });
                yield return new WaitForSecondsRealtime(6);
                Note($"C: Getaway, heat {g.Heat}, {g.Cops.Count} cops (nearest {g.Cops.Min(c => Vector3.Distance(c.car.Body.position, me.Body.position)):F0} m), sirens {g.Cops.Count(c => c.lights.Siren && !c.lights.Silent)} sounding");
                yield return Clip("E-C-getaway-heat3", 20, me);
                Note($"   cops' own VehicleAudio left on clones: {g.Cops.Count(c => c.car.GetComponent<VehicleAudio>())}");
                flow.QuitSplit(false); yield return new WaitForSecondsRealtime(3); yield return WaitMenu();
            }
            else Note("C: no Getaway started");
            // D: a split-screen race (player 2 driven by the AI), each player's nearest cars
            Bind(); SplitScreen.Mode = SplitScreen.Kind.Race; SplitScreen.Solo = false; SplitScreen.P1Device = pad; SplitScreen.P2Ai = true; SplitScreen.Course = 0; SplitScreen.Laps = 1; SplitScreen.Rivals = 3; SplitScreen.RivalDifficulty = 1; SplitScreen.Traffic = false;
            SplitScreen.P1Vehicle = "original"; SplitScreen.P2Vehicle = "atv"; flow.StartSplit(); w0 = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - w0 < 150) { Bind(); if (flow && flow.Started && !LoadingScreen.Holding && flow.State == RaceFlow.Stage.Racing && SplitScreen.Race && SplitScreen.Race.P2Car) break; yield return null; }
            if (SplitScreen.Race && SplitScreen.Race.P2Car)
            {
                me = race.vehicle; var p1 = me.gameObject.AddComponent<RoadDriver>(); p1.Initialize(race, me, true, 1, .9f); p1.Racer = race.Racers[0]; me.GetComponent<VehicleInput>().enabled = false;
                Note($"D: split-screen race, {race.Racers.Count - 2} rivals, player 2 AI ({SplitScreen.Race.P2Car.GetComponent<VehicleConfiguration>().profileId})"); yield return Clip("E-D-split-race", 20, me);
                yield return FrameTime("split-screen race");
                flow.QuitSplit(false); yield return new WaitForSecondsRealtime(3);
            }
            else Note("D: no split-screen race started");
        }
        // ---------------------------------------------------------------- Part B: Retry from the results
        //   -p106kind retry113  -p113events "c1-first-lap,c4-first-ascent" -p113runs 5: each run a fresh start from the menu (the scene loaded), the
        //                       player's vehicle driven to the finish by the road AI (nothing parked or switched off), then Retry event on the results
        //                       (RaceFlow.RetryCampaignEvent, what the results row calls) and the same race again; per rival: time, resets in the
        //                       first 10 s and in all, and whether its body collider is on (the 0.112 retries copied a parked player's car)
        IEnumerator DriveToResults(List<string> rows, string tag)
        {
            yield return WaitRacing(); Bind();
            // -p113park 1: the player's car parked out of the race as 0.112's measuring did (colliders off), the race ending 1 s after the rivals finish
            if (Arg("-p113park", "0") == "1") { var pc = race.vehicle; pc.Body.isKinematic = true; foreach (var c in pc.GetComponentsInChildren<Collider>()) c.enabled = false; var pvi = pc.GetComponent<VehicleInput>(); if (pvi) pvi.enabled = false; var prs = pc.GetComponent<VehicleRespawn>(); if (prs) prs.enabled = false; race.finishGraceSeconds = 100000; }
            else Pilot(race.vehicle, race.Racers[0]);
            var ai = race.Racers.Where(r => r.IsAi).ToList(); var reset0 = ai.ToDictionary(r => r, r => r.Recoveries); var early = ai.ToDictionary(r => r, r => 0);
            string cols = string.Join(",", ai.Select(r => r.Car.GetComponent<BoxCollider>() is var b && b && b.enabled ? "on" : "OFF"));
            Time.timeScale = 6; float t0 = Time.realtimeSinceStartup;
            while (flow.State == RaceFlow.Stage.Racing && Time.realtimeSinceStartup - t0 < 900)
            {
                AudioListener.volume = 0; yield return null; if (Time.timeScale == 0) Time.timeScale = 6;
                float clock = (float)race.Progress.RaceTime(race.Clock); foreach (var r in ai) if (clock < 10 && r.Recoveries - reset0[r] > early[r]) early[r] = r.Recoveries - reset0[r];
                if (race.finishGraceSeconds > 1000 && ai.All(r => r.Progress.Finished || r.Dnf)) race.finishGraceSeconds = 0;
            }
            Time.timeScale = 1; yield return new WaitForSecondsRealtime(1.5f);
            var me = race.Racers[0];
            rows.Add(string.Format(CultureInfo.InvariantCulture, "  {0}: player {1} s; colliders [{2}]; ", tag, me.Classified ? me.ClassifiedTime(race.Clock).ToString("0.0", CultureInfo.InvariantCulture) : "unfinished", cols) +
                string.Join("; ", ai.Select(r => string.Format(CultureInfo.InvariantCulture, "{0} {1} resets {2} (first 10 s {3})", Rp(r), r.Classified ? r.ClassifiedTime(race.Clock).ToString("0.0", CultureInfo.InvariantCulture) + (r.Estimated ? " est" : "") : "-", r.Recoveries - reset0[r], early[r]))));
        }
        IEnumerator Retry113()
        {
            Campaign.Testing = true; DriverVariation.Seed = System.Environment.TickCount;
            RaceDirector.LiveCopies = Arg("-p113live", "0") == "1"; Note("rivals copied from " + (RaceDirector.LiveCopies ? "the player's car as it is (the 0.112 way: before)" : "the clean copy kept from the scene's first start (after)"));
            foreach (var id in Arg("-p113events", "c1-first-lap,c4-first-ascent").Split(','))
            {
                var e = CampaignData.Find(id); Note($"== {e.Id} '{e.Name}' on {RacePlaylists.Scenes[e.Course]}, rivals {string.Join(",", e.Rivals)}");
                for (int run = 1; run <= int.Parse(Arg("-p113runs", "5")); run++)
                {
                    var rows = new List<string>();
                    Conditions(); flow.StartCampaignEvent(e, "atv"); yield return new WaitForSecondsRealtime(1);
                    yield return DriveToResults(rows, $"run {run} fresh");
                    if (flow.State == RaceFlow.Stage.Results) { flow.RetryCampaignEvent(); yield return new WaitForSecondsRealtime(1); yield return DriveToResults(rows, $"run {run} retry"); }
                    else rows.Add($"  run {run}: no results screen (state {flow.State})");
                    foreach (var r in rows) Note(r);
                    flow.QuitRace(); yield return WaitMenu();
                }
            }
        }
        IEnumerator Ai113()
        {
            Campaign.Testing = true; DriverVariation.Seed = System.Environment.TickCount; Conditions();
            ShortcutStrategy.Forced = Arg("-p113force", "").Split('|').Where(x => x.Length > 0).ToArray();
            if (Arg("-p112cuts", null) != null) CampaignRun.RivalShortcutsUnvalidated = Arg("-p112cuts", "").Split('|');
            foreach (var spec in Arg("-p112set", "").Split(';').Where(x => x.Contains("=")))
            { var e = CampaignData.Find(spec.Split('=')[0]); if (e != null) { Override(e, spec.Split('=')[1]); Note($"override {e.Id}: {spec.Split('=')[1]}"); } }
            string what = Arg("-p113race", "c1-wrong-way"); int runs = int.Parse(Arg("-p113runs", "5")); bool trace = Arg("-p113trace", "0") == "1";
            Note($"race {what}, {runs} runs, forced shortcuts [{string.Join(", ", ShortcutStrategy.Forced)}], chapter 4 unvalidated allowed [{string.Join(", ", CampaignRun.RivalShortcutsUnvalidated)}]");
            for (int run = 1; run <= runs; run++)
            {
                var rows = new List<string>();
                if (what.StartsWith("quick:"))
                {
                    var q = what.Split(':'); int course = int.Parse(q[1]); yield return QuickTo(course); Conditions();
                    flow.OpenGarage(); flow.SelectVehicle("atv"); flow.CloseGarage();
                    // rivals "a+b+c" or several line-ups "a+b|c+d" taken in turn, run by run
                    var lineups = q[2].Split('|'); var lineup = lineups[(run - 1) % lineups.Length];
                    if (run == 1) Note($"== quick race {RacePlaylists.Scenes[course]} rivals {q[2]} laps {q[3]} difficulty {q[4]}");
                    QuickStart(lineup.Split('+'), int.Parse(q[3]), int.Parse(q[4]));
                }
                else
                {
                    var e = CampaignData.Find(what); if (run == 1) Note($"== {e.Id} '{e.Name}' course {RacePlaylists.Scenes[e.Course]} laps {e.Laps} {e.Conditions} difficulty {e.Difficulty} pace {CampaignRunPace(e):0.000} rivals {string.Join(",", e.Rivals)}");
                    Conditions(); flow.StartCampaignEvent(e, "atv"); yield return new WaitForSecondsRealtime(1);
                }
                yield return RunField(rows, "run " + run, trace); foreach (var r in rows) Note(r);
                flow.QuitRace(); yield return WaitMenu();
            }
            ShortcutStrategy.Forced = new string[0];
        }
    }
}
