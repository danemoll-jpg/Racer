using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Racer
{
    // 1.1 evidence only (the 0.106 check component: -p106check <dir> -racerTestSave <dir>; never active without both):
    //   -p106kind replay1100 -p1100part main     Street Loop Forward, 3 laps, 3 rivals, Normal traffic, the player driven by the road AI and steered by a
    //                                            virtual pad into a mailbox or two. Finish, then: the replay against the race (each racer's position at the
    //                                            same race time), the breaks on time, every viewer control by the pad and by keys, a shot per camera, the
    //                                            finish screen and the REPLAYS page by the controller only, the race saved and watched from the page.
    //   -p1100part best     #1s in the test save: a 1-lap race on an empty board (#1 lap and race kept), a slower run (left alone), a faster run (replaced)
    //   -p1100part stunt    a Stunt Race (Mountain Loop Forward, 1 lap) replay watched briefly: the tricks play
    //   -p1100part split    a split-screen race (Street Loop Forward, 1 lap, player 2 the AI, 2 rivals) replay: full screen, LB / RB through everyone
    //   -p1100part lev      Leviathan Forward driven about 2.5 km, left, its replay: every TV camera checked (not inside anything, a clear line to the
    //                       racer's path) and one TV shot on the mountain stretch
    public sealed class ReplayProbe1100 : MonoBehaviour
    {
        // the live racers' positions at the recording's clock, every 5th physics step (after the recorder samples)
        public readonly List<(float t, Vector3[] p)> samples = new List<(float, Vector3[])>(); public RaceDirector race; int step;
        void FixedUpdate()
        {
            var rec = ReplayRecorder.Instance; if (!rec || !rec.Recording || !race || ++step % 5 != 0) return;
            samples.Add((rec.Clock, race.Racers.Select(r => r.Car ? r.Car.Body.position : Vector3.zero).ToArray()));
        }
    }
    public sealed partial class Report106PlayerCheck
    {
        Keyboard keys1100; Vector2 base1100;
        IEnumerator Press1100(GamepadButton b, float hold = .08f)
        {
            InputSystem.QueueStateEvent(pad, new GamepadState().WithButton(b)); yield return null; yield return new WaitForSecondsRealtime(hold);
            InputSystem.QueueStateEvent(pad, new GamepadState()); yield return null; yield return null; yield return new WaitForSecondsRealtime(.3f);
        }
        IEnumerator HoldPad(GamepadState s, float seconds) { InputSystem.QueueStateEvent(pad, s); yield return new WaitForSecondsRealtime(seconds); InputSystem.QueueStateEvent(pad, new GamepadState()); yield return null; yield return new WaitForSecondsRealtime(.3f); }
        IEnumerator Tap(Key k, float hold = .08f)
        {
            keys1100 ??= InputSystem.AddDevice<Keyboard>("Report1100Keys");
            InputSystem.QueueStateEvent(keys1100, new KeyboardState(k)); yield return null; yield return new WaitForSecondsRealtime(hold);
            InputSystem.QueueStateEvent(keys1100, new KeyboardState()); yield return null; yield return null; yield return new WaitForSecondsRealtime(.3f);
        }
        string Selected => EventSystem.current && EventSystem.current.currentSelectedGameObject ? EventSystem.current.currentSelectedGameObject.name : "(none)";
        // the controller only: D-pad down until the row is focused (each step's focus noted), then A
        IEnumerator PadTo(string row, List<string> path, int max = 40)
        {
            for (int i = 0; i < max && Selected != row; i++) { path.Add(Selected); yield return Press1100(GamepadButton.DpadDown); }
            path.Add(Selected);
        }
        IEnumerator WaitState(RaceFlow.Stage s, float limit)
        {
            float t0 = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - t0 < limit && flow.State != s) { AudioListener.volume = 0; yield return null; if (Time.timeScale == 0 && flow.State == RaceFlow.Stage.Racing) Time.timeScale = 3; }
        }
        // a race to its results: the player driven by the road AI (optionally its top speed scaled), x3 time
        IEnumerator RaceToResults(float speedScale = 1, System.Func<ArcadeVehicle, IEnumerator> each = null)
        {
            yield return WaitRacing(); Bind(); var me = race.Racers[0]; var car = race.vehicle; Pilot(car, me);
            if (base1100.x <= 0) base1100 = new Vector2(car.topSpeed, car.acceleration); car.topSpeed = base1100.x * speedScale; car.acceleration = base1100.y * speedScale;   // checks only: a slower or faster run (the next race applies the profile again)
            race.finishGraceSeconds = 100000; Time.timeScale = 3; float t0 = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - t0 < 1500 && flow.State != RaceFlow.Stage.Results)
            {
                AudioListener.volume = 0; if (Time.timeScale <= 0 || flow.State != RaceFlow.Stage.Racing) yield return null; else if (each != null) yield return each(car); else yield return null;
                if (Time.timeScale == 0 && flow.State == RaceFlow.Stage.Racing) Time.timeScale = 3;
            }
            Time.timeScale = 0; yield return new WaitForSecondsRealtime(2.5f);
        }
        static string Kb(string path) => File.Exists(path) ? $"{new FileInfo(path).Length / 1024.0:N0} KB" : "missing";
        static string Hash1100(string path) { if (!File.Exists(path)) return "none"; using var sha = System.Security.Cryptography.SHA256.Create(); return System.BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))).Replace("-", "").Substring(0, 12); }

        IEnumerator Replay1100()
        {
            string part = Arg("-p1100part", "main"); Campaign.Testing = true; DriverVariation.Seed = 1100; Conditions();
            Note($"replay check part {part}, save {flow.SaveRoot}, replays in {ReplayData.Folder}");
            switch (part)
            {
                case "main": yield return Main1100(); break;
                case "best": yield return Best1100(); break;
                case "stunt": yield return Stunt1100(); break;
                case "split": yield return Split1100(); break;
                case "lev": yield return Lev1100(); break;
            }
        }

        // ---------------------------------------------------------------- the main check
        IEnumerator Main1100()
        {
            yield return QuickTo115(0); Conditions();
            var st = flow.Save.Settings; st.opponents = true; st.traffic = true; st.laps = 3; st.difficulty = 1; st.estimateAiFinishes = false; st.nameTags = 2; flow.Save.SaveSettings();
            race.opponents = true; race.traffic = true; race.laps = 3; race.difficulty = 1;
            flow.StartRace(); yield return WaitRacing(); Bind();
            var probe = flow.gameObject.AddComponent<ReplayProbe1100>(); probe.race = race;
            Note($"== {race.courseName} ({race.courseId}), {race.laps} laps, {race.Racers.Count - 1} rivals ({race.RosterLabel}), {race.DifficultyName}, traffic {race.Drivers.Count(d => d && d.GetComponent<AmbientVehicle>())} cars; player {race.vehicle.GetComponent<VehicleConfiguration>().profileId}");
            // the mailboxes (and other breakable props) near the course: the pad steers the player into the next one ahead
            var props = FindObjectsByType<BreakableProp>(FindObjectsSortMode.None).Where(p => p.name.StartsWith("Mailbox") || p.GetComponentInParent<Transform>().name.StartsWith("Mailbox")).ToList();
            if (props.Count == 0) props = FindObjectsByType<BreakableProp>(FindObjectsSortMode.None).ToList();
            var smashed = new List<string>(); BreakableProp target = null; float targetSince = 0; int smashes = 0;
            BindPad(); var vi = race.vehicle.GetComponent<VehicleInput>(); vi.enabled = false;
            IEnumerator Each(ArcadeVehicle car)
            {
                yield return null;
                if (smashes >= 2 || race.Racers[0].Progress.Finished || race.Racers[0].Progress.CompletedLaps < 1) yield break;   // on lap 2, when the field has spread
                var pilot = car.GetComponent<RoadDriver>();
                if (!target)
                {
                    var fwd = Flat(car.transform.forward).normalized;
                    foreach (var p in props)
                    {
                        if (p.IsBroken) continue; var to = Flat(p.Home - car.Body.position); float ahead = Vector3.Dot(to, fwd), side = Vector3.Cross(fwd, to).magnitude;
                        if (ahead > 12 && ahead < 40 && side < 14) { target = p; targetSince = Time.time; if (pilot) pilot.enabled = false; car.enabled = true; vi.enabled = true; break; }
                    }
                }
                if (target)
                {
                    Drive(car, target.Home, 16, steerGain: 18);
                    if (target.IsBroken || Time.time - targetSince > 4)
                    {
                        if (target.IsBroken) { smashes++; smashed.Add($"'{target.name}' at {V(target.Home)}, race clock {ReplayRecorder.Instance.Clock:F2} s"); }
                        target = null; Release(); vi.enabled = false; Pilot(car, race.Racers[0]);
                    }
                }
            }
            yield return RaceToResults(1, Each);
            Note($"results: state {flow.State}; {race.Standings().Replace("\n", " / ")}"); Note("smashed: " + (smashed.Count > 0 ? string.Join("; ", smashed) : "none"));
            Note("recorder: " + ReplayRecorder.Note);
            var d = flow.Replays.Last; if (d == null) { Note("NO REPLAY"); yield break; }
            Note($"replay: {d.Title}, {d.racers.Count} racers, {d.traffic.Count} traffic, {d.events.Count} events ({d.events.Count(e => e.kind == ReplayData.Kind.Prop)} props), {d.t0:F1} to {d.t1:F1} s; laps of player {string.Join(", ", d.racers[0].laps.Select(l => $"{l.lap}: {l.start:F2}-{l.end:F2}"))}");
            // positions: the replay against the live race at the same race time
            var errs = new List<float>(); var worst = new float[d.racers.Count];
            foreach (var (t, ps) in probe.samples) for (int i = 0; i < ps.Length && i < d.racers.Count; i++) { float e = Vector3.Distance(ReplayData.At(d.racers[i].track, t).p, ps[i]); errs.Add(e); worst[i] = Mathf.Max(worst[i], e); }
            errs.Sort(); Note($"position error over {probe.samples.Count} samples x {d.racers.Count} racers: median {errs[errs.Count / 2]:F3} m, 99th {errs[(int)(errs.Count * .99f)]:F3} m, max {errs.Last():F3} m; worst per racer {string.Join(", ", worst.Select((w, i) => d.racers[i].name + " " + w.ToString("F2")))}");
            Destroy(probe);
            // the finish screen: its rows and the focus
            yield return Snap("finish-screen-buttons"); Note($"finish screen rows: {string.Join(" | ", FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None).Where(b => b.gameObject.activeInHierarchy && (b.name.Contains("replay") || b.name == "primary")).Select(b => b.name + ": " + b.GetComponentInChildren<UnityEngine.UI.Text>(true).text))}");
            // ---- the pad: X watches
            yield return Press1100(GamepadButton.West); yield return new WaitForSecondsRealtime(1.5f);
            var v = ReplayViewer.Current; Note($"pad X: viewer {(v ? "open" : "NOT OPEN")}, menu canvas hidden {!(FindAnyObjectByType<RaceHud>().GetComponent<Canvas>().enabled)}, camera {v?.View}, TV cameras {v?.TvCount}");
            if (!v) yield break;
            // breaks on time: just before the first prop event, and after it
            var prop = d.events.FirstOrDefault(e => e.kind == ReplayData.Kind.Prop);
            if (prop.kind == ReplayData.Kind.Prop)
            {
                var bp = FindObjectsByType<BreakableProp>(FindObjectsSortMode.None).OrderBy(p => (p.Home - prop.p).sqrMagnitude).First();
                v.Seek(prop.t - 1.5f); yield return null; bool before = bp.IsBroken;
                float t0 = Time.realtimeSinceStartup; while (v.T < prop.t + .3f && Time.realtimeSinceStartup - t0 < 5) yield return null; bool after = bp.IsBroken;
                v.FocusOn(0); v.SetView(ReplayViewer.Cam.Chase); v.Seek(prop.t - .6f); yield return new WaitForSecondsRealtime(.9f); yield return Snap("break-in-replay");
                Note($"break on time: '{bp.name}' recorded at {prop.t:F2} s: broken at {prop.t - 1.5f:F2} s {before}, at {v.T:F2} s {after}");
                v.SetView(ReplayViewer.Cam.TV);
            }
            // the cameras: one shot each (on lap 2, mid-field)
            float mid = d.racers[0].laps.Count > 1 ? d.racers[0].laps[0].end + 20 : d.t0 + 60; v.Seek(mid); yield return new WaitForSecondsRealtime(2.5f);
            yield return Snap("camera-tv"); Note($"TV camera {v.TvIndex} at {(v.TvIndex >= 0 ? V(v.TvSpot(v.TvIndex)) : "fallback chase")}, racer at {V(v.FocusCar.transform.position)}");
            // ---- every control by the pad
            var log = new List<string>();
            yield return Press1100(GamepadButton.South); log.Add($"A: paused {v.Paused}"); yield return Press1100(GamepadButton.South); log.Add($"A: paused {v.Paused}");
            float tA = v.T; yield return HoldPad(new GamepadState { rightTrigger = 1 }, 1f); log.Add($"RT 1 s: +{v.T - tA:F1} s");
            tA = v.T; yield return HoldPad(new GamepadState { leftTrigger = 1 }, 1f); log.Add($"LT 1 s: {v.T - tA:F1} s");
            yield return Press1100(GamepadButton.West); log.Add($"X: rate {v.Rate}"); yield return Press1100(GamepadButton.West); log.Add($"X: rate {v.Rate}"); yield return Press1100(GamepadButton.West); log.Add($"X: rate {v.Rate}");
            yield return Press1100(GamepadButton.North); log.Add($"Y: {v.View}"); yield return new WaitForSecondsRealtime(1.2f); yield return Snap("camera-chase");
            yield return Press1100(GamepadButton.RightShoulder); log.Add($"RB: racer {v.Focus} ({d.racers[v.Focus].name})"); yield return new WaitForSecondsRealtime(1f); yield return Snap("camera-chase-other-racer");
            yield return Press1100(GamepadButton.LeftShoulder); log.Add($"LB: racer {v.Focus}");
            yield return Press1100(GamepadButton.North); log.Add($"Y: {v.View}");
            var camAt = Camera.main.transform.position; yield return HoldPad(new GamepadState { leftStick = new Vector2(0, 1), rightStick = new Vector2(.4f, 0) }, 1.2f); log.Add($"free: sticks moved the camera {Vector3.Distance(camAt, Camera.main.transform.position):F1} m");
            camAt = Camera.main.transform.position; yield return HoldPad(new GamepadState().WithButton(GamepadButton.RightShoulder), .8f); log.Add($"free: RB up {Camera.main.transform.position.y - camAt.y:F1} m");
            yield return new WaitForSecondsRealtime(.5f); yield return Snap("camera-free");
            yield return Press1100(GamepadButton.North); log.Add($"Y: {v.View}");
            float tL = v.T; yield return Press1100(GamepadButton.DpadRight); log.Add($"D-pad right: {tL:F1} -> {v.T:F1} s"); tL = v.T; yield return Press1100(GamepadButton.DpadLeft); log.Add($"D-pad left: {tL:F1} -> {v.T:F1} s");
            yield return Press1100(GamepadButton.DpadUp); log.Add($"D-pad up: restart at {v.T:F1} s (start {d.t0:F1})");
            yield return Press1100(GamepadButton.DpadDown); log.Add($"D-pad down: HUD {v.HudOn}"); v.Seek(mid + 10); yield return new WaitForSecondsRealtime(1.5f); yield return Snap("camera-tv-hud");
            yield return Press1100(GamepadButton.DpadDown); log.Add($"D-pad down: HUD {v.HudOn}");
            string savedBefore = ReplayData.LastSaved; yield return Press1100(GamepadButton.Select); log.Add($"View: '{v.Message}' -> {Path.GetFileName(ReplayData.LastSaved)} ({Kb(ReplayData.LastSaved)}){(ReplayData.LastSaved != savedBefore ? "" : " NOT SAVED")}");
            yield return Press1100(GamepadButton.Select); log.Add($"View again: '{v.Message}'");
            yield return Press1100(GamepadButton.East); yield return new WaitForSecondsRealtime(1f); log.Add($"B: viewer {(ReplayViewer.Active ? "still open" : "closed")}, state {flow.State}, focus {Selected}, time scale {Time.timeScale}");
            Note("pad: " + string.Join("; ", log)); log.Clear();
            // ---- the same by keys (V watches)
            yield return Tap(Key.V); yield return new WaitForSecondsRealtime(1.5f); v = ReplayViewer.Current; log.Add($"V: viewer {(v ? "open" : "NOT OPEN")}");
            if (v)
            {
                v.Seek(mid);
                yield return Tap(Key.Space); log.Add($"Space: paused {v.Paused}"); yield return Tap(Key.Space); log.Add($"Space: paused {v.Paused}");
                tA = v.T; yield return Tap(Key.RightArrow, 1f); log.Add($"Right 1 s: +{v.T - tA:F1} s"); tA = v.T; yield return Tap(Key.LeftArrow, 1f); log.Add($"Left 1 s: {v.T - tA:F1} s");
                yield return Tap(Key.S); log.Add($"S: rate {v.Rate}"); yield return Tap(Key.S); yield return Tap(Key.S); log.Add($"S S: rate {v.Rate}");
                yield return Tap(Key.C); log.Add($"C: {v.View}"); yield return Tap(Key.E); log.Add($"E: racer {v.Focus}"); yield return Tap(Key.Q); log.Add($"Q: racer {v.Focus}");
                yield return Tap(Key.C); log.Add($"C: {v.View}"); camAt = Camera.main.transform.position; yield return Tap(Key.W, 1f); log.Add($"W 1 s: camera moved {Vector3.Distance(camAt, Camera.main.transform.position):F1} m");
                yield return Tap(Key.C); log.Add($"C: {v.View}");
                tL = v.T; yield return Tap(Key.Period); log.Add($". : {tL:F1} -> {v.T:F1} s"); tL = v.T; yield return Tap(Key.Comma); log.Add($", : {tL:F1} -> {v.T:F1} s");
                yield return Tap(Key.Home); log.Add($"Home: {v.T:F1} s"); yield return Tap(Key.H); log.Add($"H: HUD {v.HudOn}"); yield return Tap(Key.H);
                yield return Tap(Key.K); log.Add($"K: '{v.Message}'");
                yield return Tap(Key.Escape); yield return new WaitForSecondsRealtime(1f); log.Add($"Esc: viewer {(ReplayViewer.Active ? "still open" : "closed")}, state {flow.State}");
            }
            yield return Tap(Key.K); log.Add($"K on the finish screen: {FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None).FirstOrDefault(b => b.name == "save-replay")?.GetComponentInChildren<UnityEngine.UI.Text>(true).text}");
            Note("keys: " + string.Join("; ", log));
            // ---- the race saved from the finish screen, found on the REPLAYS page, played from there (controller only)
            yield return Press1100(GamepadButton.North); var saved = ReplayData.LastSaved; Note($"pad Y on the finish screen: saved {Path.GetFileName(saved)} ({Kb(saved)}) — one 3-lap replay");
            var steps = new List<string>();
            yield return Press1100(GamepadButton.East); yield return WaitMenu(); steps.Add($"B: state {flow.State}");
            yield return PadTo("replays", steps); yield return Press1100(GamepadButton.South); yield return new WaitForSecondsRealtime(1f);
            steps.Add($"A: page rows {string.Join(", ", FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None).Where(b => b.gameObject.activeInHierarchy).Select(b => b.name))}");
            yield return Snap("replays-page");
            yield return PadTo(FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None).Where(b => b.gameObject.activeInHierarchy && b.name.StartsWith("saved-")).Select(b => b.name).FirstOrDefault() ?? "saved-1", steps);
            yield return Press1100(GamepadButton.South); yield return new WaitForSecondsRealtime(1f); steps.Add($"A: saved replay page, focus {Selected}"); yield return Snap("replays-saved-one");
            yield return Press1100(GamepadButton.South); yield return new WaitForSecondsRealtime(3f); v = ReplayViewer.Current; steps.Add($"A WATCH: viewer {(v ? $"open, {v.Data.kind}, {v.Data.racers.Count} racers" : "NOT OPEN")}");
            if (v) { v.Seek(mid); yield return new WaitForSecondsRealtime(2f); yield return Snap("watched-from-page"); }
            yield return Press1100(GamepadButton.East); yield return new WaitForSecondsRealtime(1f); steps.Add($"B: viewer {(ReplayViewer.Active ? "open" : "closed")}, page focus {Selected}");
            yield return Press1100(GamepadButton.East); steps.Add($"B: focus {Selected}"); yield return Press1100(GamepadButton.East); steps.Add($"B: focus {Selected}");
            Note("page by the controller: " + string.Join("; ", steps));
        }

        // ---------------------------------------------------------------- #1s
        IEnumerator Best1100()
        {
            yield return QuickTo115(0); Conditions();
            var st = flow.Save.Settings; st.opponents = true; st.traffic = false; st.laps = 1; st.difficulty = 0; st.estimateAiFinishes = true; st.opponentRoster = new[] { "mower", "mower", "atv" }; flow.Save.SaveSettings();
            race.opponents = true; race.traffic = false; race.laps = 1; race.difficulty = 0; race.opponentRoster = (string[])st.opponentRoster.Clone(); flow.SelectRecords(race.Category);
            string cat = race.Category, lapPath = ReplayData.BestPath(RecordBoards.LapCategory(cat), false), racePath = ReplayData.BestPath(cat, true);
            Note($"board {cat}: race entries {flow.Boards.Board(cat, true).Count}, lap entries {flow.Boards.Board(cat, false).Count}");
            string Head(string p) { try { var h = ReplayData.Read(p, true); return $"{h.kind} {RaceHud.FormatTime(h.bestTime)} ({Kb(p)}, {Hash1100(p)})"; } catch { return "none"; } }
            foreach (var (name, scale) in new[] { ("first run", .8f), ("slower run", .6f), ("faster run", 1f) })
            {
                flow.StartRace(); yield return RaceToResults(scale);
                Note($"{name} (top speed x{scale}): lap {RaceHud.FormatTime(race.Progress.LastLap)} rank #{flow.LapRank}, race {RaceHud.FormatTime(race.Progress.AdjustedTime(race.Clock))} rank #{flow.RaceRank}; recorder: {ReplayRecorder.Note}");
                Note($"   kept: lap {Head(lapPath)}; race {Head(racePath)}");
                if (name == "faster run") { var d = ReplayData.Read(lapPath); Note($"   the #1 lap's replay: {d.t0:F1} to {d.t1:F1} s ({d.Duration:F1} s), the race had {race.Progress.LastLap:F1} s laps"); }
            }
            flow.QuitRace(); yield return WaitMenu();
        }

        // ---------------------------------------------------------------- a Stunt Race replay
        IEnumerator Stunt1100()
        {
            yield return StuntQuick(4, false, 1, 1);
            yield return WaitRacing(); Bind(); var steer = race.vehicle.gameObject.AddComponent<RivalTricks>(); steer.Initialize(race, 9); steer.Bind(race.Racers[0]); steer.SteerOnly = true;
            yield return RaceToResults();
            Note($"Stunt Race results: {race.Standings().Replace("\n", " / ")}; recorder: {ReplayRecorder.Note}");
            Menu.WatchLastReplay(); yield return new WaitForSecondsRealtime(1.5f); var v = ReplayViewer.Current; if (!v) { Note("NO VIEWER"); yield break; }
            // the tricks play: a puppet upside down or turned past 90 degrees from level in the air
            v.SetView(ReplayViewer.Cam.Chase); int flips = 0; bool shot = false; float t0 = Time.realtimeSinceStartup; var seen = new HashSet<int>();
            while (Time.realtimeSinceStartup - t0 < 60 && v.T < v.Data.t1 - .1f)
            {
                InputSystem.QueueStateEvent(pad, new GamepadState { rightTrigger = 1 }); yield return null;
                for (int i = 0; i < v.Data.racers.Count; i++) { var p = v.PuppetOf(i); if (p && p.transform.up.y < 0 && seen.Add(i * 10000 + Mathf.FloorToInt(v.T))) flips++; }
                if (!shot && v.FocusCar && v.FocusCar.transform.up.y < .2f) { InputSystem.QueueStateEvent(pad, new GamepadState()); shot = true; yield return Snap("stunt-replay-trick"); }
            }
            InputSystem.QueueStateEvent(pad, new GamepadState()); yield return null;
            Note($"Stunt Race replay: {v.Data.racers.Count} racers, played {v.Data.t0:F0} to {v.T:F0} s at x4: racers upside down in {flips} racer-seconds; shot {(shot ? "taken" : "none (no flip of the chased racer)")}");
            v.Close(); yield return new WaitForSecondsRealtime(1);
        }

        // ---------------------------------------------------------------- a split-screen race replay
        IEnumerator Split1100()
        {
            SplitScreen.Mode = SplitScreen.Kind.Race; SplitScreen.Solo = false; SplitScreen.P1Device = pad; SplitScreen.P2Device = null; SplitScreen.P2Ai = true; SplitScreen.Course = 0; SplitScreen.Laps = 1; SplitScreen.Rivals = 2;
            SplitScreen.P1Vehicle = "original"; SplitScreen.P2Vehicle = "atv"; SplitScreen.P1Name = "Player One"; SplitScreen.P2Pick = "Player Two"; flow.StartSplit();
            yield return new WaitForSecondsRealtime(2); float t1 = Time.realtimeSinceStartup; while (RacePlaylists.CourseOf(UnityEngine.SceneManagement.SceneManager.GetActiveScene()) != 0 && Time.realtimeSinceStartup - t1 < 90) yield return null;
            yield return RaceToResults();
            Note($"split-screen results: {race.Standings().Replace("\n", " / ")}; recorder: {ReplayRecorder.Note}");
            Menu.WatchLastReplay(); yield return new WaitForSecondsRealtime(1.5f); var v = ReplayViewer.Current; if (!v) { Note("NO VIEWER"); yield break; }
            var cam = Camera.main; var others = FindObjectsByType<Camera>(FindObjectsSortMode.None).Where(c => c != cam && c.enabled && c.targetTexture == null).Select(c => c.name).ToList();
            v.Seek(v.Data.t0 + 40); v.SetView(ReplayViewer.Cam.Chase); var names = new List<string>();
            for (int i = 0; i < v.Data.racers.Count + 1; i++) { names.Add($"{v.Focus} {v.Data.racers[v.Focus].name}{(v.Data.racers[v.Focus].human ? " (player " + v.Data.racers[v.Focus].seat + ")" : "")}"); yield return Press1100(GamepadButton.RightShoulder); }
            yield return new WaitForSecondsRealtime(1); yield return Snap("split-replay-full-screen");
            Note($"split-screen replay: camera rect {cam.rect}, other cameras drawing: {(others.Count == 0 ? "none" : string.Join(", ", others))}; RB through {string.Join(" -> ", names)}");
            yield return Press1100(GamepadButton.East); yield return new WaitForSecondsRealtime(1);
            Note($"left: camera rect {cam.rect}, player 2's camera {(FindObjectsByType<Camera>(FindObjectsSortMode.None).Any(c => c != cam && c.enabled && c.targetTexture == null) ? "drawing" : "off")}");
            flow.QuitSplit(false); yield return WaitMenu();
        }

        // ---------------------------------------------------------------- Leviathan: the TV cameras
        IEnumerator Lev1100()
        {
            yield return QuickTo115(8); Conditions();
            var st = flow.Save.Settings; st.opponents = true; st.traffic = false; st.laps = 1; st.difficulty = 1; flow.Save.SaveSettings(); race.opponents = true; race.traffic = false; race.laps = 1; race.difficulty = 1;
            if (race.stunts) { flow.StepRaceType(1); yield return WaitMenu(); Bind(); }
            flow.StartRace(); yield return WaitRacing(); Bind(); var me = race.Racers[0]; Pilot(race.vehicle, me);
            float stop = float.Parse(Arg("-p1100road", "2600"), System.Globalization.CultureInfo.InvariantCulture); Time.timeScale = 3; float t0 = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - t0 < 600 && !(me.Progress.LapActive && me.RoadPosition > stop && me.RoadPosition < race.road.Length - 200)) { AudioListener.volume = 0; yield return null; if (Time.timeScale == 0) Time.timeScale = 3; }
            Time.timeScale = 1; Note($"Leviathan Forward ({race.courseId}) driven to road {me.RoadPosition:F0} m; left the race");
            flow.Pause(); flow.QuitRace(); yield return WaitMenu();
            Note("recorder: " + ReplayRecorder.Note);
            var v = ReplayViewer.Open(flow.Replays.Last); yield return new WaitForSecondsRealtime(1); if (!v) { Note("NO VIEWER"); yield break; }
            var frames = v.Data.racers[0].track.frames; int bad = 0, onPath = 0;
            for (int i = 0; i < v.TvCount; i++)
            {
                var at = v.TvSpot(i); if (v.TvOnPath(i)) onPath++;
                var near = frames.OrderBy(f => (f.p - at).sqrMagnitude).First().p + Vector3.up * .8f;
                bool inside = Physics.CheckSphere(at, .3f, ~0, QueryTriggerInteraction.Ignore) || Physics.RaycastAll(near, (at - near).normalized, (at - near).magnitude, ~0, QueryTriggerInteraction.Ignore).Any(h => !h.collider.attachedRigidbody);
                bool under = Physics.Raycast(at, Vector3.down, out var down, 200, ~0, QueryTriggerInteraction.Ignore) == false;
                if (inside || under) { bad++; Note($"  TV camera {i} at {V(at)}: {(inside ? "blocked from the path" : "")}{(under ? " no ground under it" : "")}"); }
            }
            // the mountain stretch: the highest point of the player's way
            var top = frames.OrderByDescending(f => f.p.y).First();
            v.SetView(ReplayViewer.Cam.TV); v.Seek(top.t - 3); yield return new WaitForSecondsRealtime(3.5f);
            Note($"TV cameras along Leviathan: {v.TvCount} ({onPath} on the way itself), {bad} inside or under anything; mountain shot at race clock {v.T:F1} s, racer at {V(v.FocusCar.transform.position)}, camera {(v.TvIndex >= 0 ? "TV " + v.TvIndex + " at " + V(v.TvSpot(v.TvIndex)) : "the chase fallback")} {V(Camera.main.transform.position)}");
            yield return Snap("leviathan-tv-mountain");
            v.Close(); yield return new WaitForSecondsRealtime(1);
        }
    }
}
