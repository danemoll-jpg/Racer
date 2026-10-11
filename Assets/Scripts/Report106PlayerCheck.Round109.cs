using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Racer
{
    // 0.109 evidence only, on the 0.106 check (same switches: -p106check <dir> -racerTestSave <dir>; never active without both):
    //   -p106kind mainmenu   Part A by controller only: the main menu's rows in order; STUNT EVENT from the main menu solo (a short round, -p106limit) and
    //                        back to the main menu; the same in split-screen (a second virtual pad joins); from Free Roam far from the park: the question,
    //                        the event, back to Free Roam
    //   -p106kind results    Part B: a test save holding a full Top 10 Best Combos list; an event (-p106players 1|2) that enters two combos (player 2's in
    //                        split-screen); the results screen (combos first, RB to the event Top 10)
    //   -p106kind balance    Parts C and D: a campaign race finished and one quit halfway (one lap: a test shortcut; the player's car driven by the road AI at 3x),
    //                        a Free Roam speed trap and a timed jump attempt canceled, a Getaway (1-minute round: a test shortcut), a Stunt event with the test
    //                        limit, a split-screen race (both cars driven by the road AI); then the log lines and the balance sheet (test log path)
    public sealed partial class Report106PlayerCheck
    {
        // ---------------------------------------------------------------- Parts C and D: the results log and the balance sheet
        IEnumerator WaitRacing(float limit = 150)
        {
            float t0 = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - t0 < limit) { Bind(); if (flow && flow.Started && !LoadingScreen.Holding && (flow.State == RaceFlow.Stage.Racing || flow.State == RaceFlow.Stage.Countdown)) break; yield return null; }
            while (Time.realtimeSinceStartup - t0 < limit && flow && flow.State == RaceFlow.Stage.Countdown) yield return null;
            AudioListener.volume = 0; yield return new WaitForSecondsRealtime(.5f);
        }
        void Pilot(ArcadeVehicle car, RacerState state)
        {
            if (!car) return;
            var pilot = car.GetComponent<RoadDriver>() ?? car.gameObject.AddComponent<RoadDriver>(); pilot.Initialize(race, car, true, 1, 1); pilot.Racer = state; pilot.enabled = true; var vi = car.GetComponent<VehicleInput>(); if (vi) vi.enabled = false;
        }
        IEnumerator Drive(float realSeconds, System.Func<bool> until)
        {
            Time.timeScale = 3; float t0 = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - t0 < realSeconds && !until()) { AudioListener.volume = 0; yield return null; if (Time.timeScale == 0) Time.timeScale = 3; }
            Time.timeScale = 1;
        }
        int logLines;
        void NewLines(string what)
        {
            var path = ResultsLog.FilePath; var all = path != null && File.Exists(path) ? File.ReadAllLines(path) : new string[0];
            Note($"{what}: {all.Length - logLines} new line(s) in {path}"); foreach (var l in all.Skip(logLines)) Note("    " + l); logLines = all.Length;
        }
        IEnumerator BalanceLogCheck()
        {
            Conditions(); Campaign.Testing = true;
            Note($"results log folder {ResultsLog.Folder} (test save {flow.Save.DirectoryPath}); the real log folder is {Path.Combine(Application.persistentDataPath, "BalanceLog")}");
            NewLines("before");
            // a campaign race: one lap (shortcut), finished
            var e = CampaignData.Events.First(x => x.Kind == CampaignEventKind.Race && x.Chapter == 1);
            flow.StartCampaignEvent(e, "original"); yield return new WaitForSecondsRealtime(2); yield return WaitRacing();
            race.laps = 1; flow.StartRace(); yield return WaitRacing(); Note($"campaign race {e.Id} ({e.Name}): laps set to 1 and restarted (a test shortcut; the restart is itself logged as abandoned)");
            Pilot(race.vehicle, race.Racers[0]); yield return Drive(400, () => flow.State == RaceFlow.Stage.Results);
            yield return new WaitForSecondsRealtime(1); Note($"state {flow.State}"); NewLines("campaign race finished");
            // the same event again, quit halfway
            flow.RetryCampaignEvent(); yield return WaitRacing(); Pilot(race.vehicle, race.Racers[0]);
            yield return Drive(25, () => flow.State != RaceFlow.Stage.Racing); flow.Pause(); yield return null; flow.QuitRace(); yield return new WaitForSecondsRealtime(1.5f);
            NewLines("campaign race quit halfway");
            // Free Roam: a speed trap and a timed jump attempt canceled
            yield return ToFreeRoam("original"); var acts = flow.Activities;
            var trap = acts.Sites.FirstOrDefault(x => x && x.kind == ActivitySite.Kind.Speed);
            if (trap)
            {
                var f = Flat(trap.forward).normalized; int awards = acts.Awards;
                for (int k = 0; k < 2 && acts.Awards == awards; k++) { Put(race.vehicle, trap.transform.position - f * 150, Yaw(f), 30); float t0 = Time.realtimeSinceStartup; while (Time.realtimeSinceStartup - t0 < 9 && acts.Awards == awards) { yield return new WaitForFixedUpdate(); var v = race.vehicle.Body.linearVelocity; race.vehicle.Body.linearVelocity = f * 30 + Vector3.up * v.y; } } // carried straight through at 30 m/s (a test shortcut)
                Release(); Note($"speed trap {trap.title} (placed 150 m before it, carried through at 30 m/s): scored {acts.Awards > awards} ({acts.LastSpeed:F1} m/s)");
            }
            var jump = acts.Sites.FirstOrDefault(x => x && x.kind == ActivitySite.Kind.Jump);
            if (jump) { for (int k = 0; k < 40 && acts.Selected != jump; k++) acts.Cycle(); acts.BeginAttempt(); yield return new WaitForSecondsRealtime(1); acts.Cancel(); Note($"timed jump attempt at {jump.title}: started, then canceled"); }
            NewLines("Free Roam activities");
            flow.Pause(); yield return null; flow.QuitRace(); yield return WaitMenu();
            // a Getaway (one-minute round: shortcut)
            SplitScreen.Mode = SplitScreen.Kind.Police; SplitScreen.PoliceGame = SplitScreen.Game.Getaway; SplitScreen.Solo = true; SplitScreen.P2Ai = true; SplitScreen.P1Device = pad; SplitScreen.P2Device = null; SplitScreen.PoliceMinutes = 1; SplitScreen.PoliceDifficulty = 1;
            SplitScreen.P1Vehicle = "original"; SplitScreen.FromRoam = false; SplitScreen.Time = TimeOfDay.Day; SplitScreen.Weather = Weather.Clear; flow.StartSplit();
            yield return new WaitForSecondsRealtime(2); yield return WaitRacing(); Note($"Getaway running: {(GetawayChase.Current ? GetawayChase.Current.State.ToString() : "none")} (round shortened to 1 minute: SplitScreen.PoliceMinutes, a test shortcut)");
            yield return Drive(60, () => flow.State == RaceFlow.Stage.Results); yield return new WaitForSecondsRealtime(1); NewLines("Getaway");
            flow.QuitSplit(false); yield return WaitMenu(); SplitScreen.PoliceMinutes = 5;
            // a Stunt event with the test limit (one backflip combo)
            yield return StartStunt(1, 20); var notes = new List<string>(); if (StuntEvent.Current) yield return Combo(pad, race.vehicle, P(112, 40), 2, notes, "stunt combo x2");
            foreach (var n in notes) Note(n); yield return WaitResults(60); NewLines("Stunt event");
            yield return FocusRow("menu"); yield return Press(GamepadButton.South); yield return WaitMenu(); yield return PanelLoop("balance-after-stunt");
            // a split-screen race: Street Loop, one lap, two rivals; both players' cars driven by the road AI
            var pad2 = InputSystem.AddDevice<Gamepad>("Report109Pad2");
            SplitScreen.Mode = SplitScreen.Kind.Race; SplitScreen.Solo = false; SplitScreen.P1Device = pad; SplitScreen.P2Device = pad2; SplitScreen.P2Ai = false; SplitScreen.Course = 0; SplitScreen.Laps = 1; SplitScreen.Rivals = 2; SplitScreen.RivalDifficulty = 1; SplitScreen.Traffic = false;
            SplitScreen.P1Vehicle = "original"; SplitScreen.P2Vehicle = "atv"; SplitScreen.P1Name = "Player One"; SplitScreen.P2Pick = "Player Two"; flow.StartSplit();
            yield return new WaitForSecondsRealtime(2); yield return WaitRacing(); Note($"split-screen race: {race.Racers.Count} racers, scene {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
            Pilot(race.vehicle, race.Racers[0]); if (SplitScreen.Race && SplitScreen.Race.P2Car) Pilot(SplitScreen.Race.P2Car, race.Racers[1]);
            yield return Drive(400, () => flow.State == RaceFlow.Stage.Results); yield return new WaitForSecondsRealtime(1); NewLines("split-screen race");
            flow.QuitSplit(false); yield return WaitMenu(); InputSystem.RemoveDevice(pad2);
            // the balance sheet
            var sheet = Path.Combine(ResultsLog.Folder, ResultsLog.SheetName);
            if (File.Exists(sheet)) { File.Copy(sheet, Path.Combine(outDir, ResultsLog.SheetName), true); Note($"balance sheet {sheet}: {File.ReadAllLines(sheet).Length} lines, sections: {string.Join(" | ", File.ReadAllLines(sheet).Where(l => l.StartsWith("## ")).Select(l => l.Substring(3)))}"); }
            else Note("balance sheet MISSING");
            if (File.Exists(ResultsLog.FilePath)) File.Copy(ResultsLog.FilePath, Path.Combine(outDir, ResultsLog.FileName), true);
        }
        string MainRows() => string.Join(", ", FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None).Where(b => b.isActiveAndEnabled)
            .Where(b => new[] { "resume", "trailer", "campaign", "race", "roam", "split", "police", "stunt", "garage", "records", "exploration", "settings", "quit", "camera-view" }.Contains(b.name))
            .OrderBy(b => b.transform.GetSiblingIndex()).Select(b => b.name));
        // A on the focused row; then waits for the event to run (or the setup page)
        IEnumerator WaitEvent(float limit = 150)
        {
            float r0 = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - r0 < limit) { Bind(); if (flow && StuntEvent.Current && StuntEvent.Current.State == StuntEvent.Phase.Running) break; yield return null; }
            AudioListener.volume = 0;
        }
        IEnumerator WaitResults(float limit = 120)
        {
            float w0 = Time.realtimeSinceStartup; while (Time.realtimeSinceStartup - w0 < limit && !(flow && flow.State == RaceFlow.Stage.Results)) { Bind(); yield return null; }
            yield return new WaitForSecondsRealtime(1.5f);
        }
        IEnumerator MainMenuStunt()
        {
            float limit = F(Arg("-p106limit", "8")); StuntEvent.TestLimit = limit; Conditions(); flow.Save.Settings.hints = true; flow.Save.SaveSettings(); yield return new WaitForSecondsRealtime(1.5f);
            Note($"main menu rows top to bottom: {MainRows()}; hint '{Hints.Text}'"); yield return Snap("A-main-menu");
            // solo from the main menu
            yield return FocusRow("stunt"); Note($"focus '{Focus}'"); yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(1f);
            Note($"A: page '{Page}', focus '{Focus}', players {(SplitScreen.Solo ? 1 : 2)}"); yield return Snap("A-setup-from-main-menu");
            if (!SplitScreen.Solo) { yield return FocusRow("stunt-players"); yield return Press(GamepadButton.DpadRight); }
            yield return FocusRow("stunt-start"); Note($"focus '{Focus}' (round shortened to {limit} s: StuntEvent.TestLimit, checks only)"); yield return Press(GamepadButton.South);
            yield return WaitEvent(); Note($"event: scene {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}, state {(StuntEvent.Current ? StuntEvent.Current.State.ToString() : "none")}, players {StuntEvent.Current?.Riders.Count}, from Free Roam {SplitScreen.FromRoam}");
            yield return WaitResults(); Note($"results: state {flow.State}"); yield return FocusRow("menu"); Note($"focus '{Focus}' ('{Texts("Main menu", "Back to Free Roam")}')"); yield return Press(GamepadButton.South);
            yield return WaitMenu(); Note($"back: state {flow.State}, scene {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}, in Free Roam {flow.InRoamWorld}, page '{Page}'"); yield return Snap("A-back-at-main-menu-solo");
            // split-screen from the main menu: a second pad presses A on the setup page
            yield return FocusRow("stunt"); yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(1f);
            if (SplitScreen.Solo) { yield return FocusRow("stunt-players"); yield return Press(GamepadButton.DpadRight); }
            var pad2 = InputSystem.AddDevice<Gamepad>("Report109Pad2"); yield return new WaitForSecondsRealtime(.3f);
            InputSystem.QueueStateEvent(pad2, new GamepadState().WithButton(GamepadButton.South)); yield return new WaitForSecondsRealtime(.15f); InputSystem.QueueStateEvent(pad2, new GamepadState()); yield return new WaitForSecondsRealtime(.6f);
            if (string.IsNullOrEmpty(SplitScreen.P2Pick)) SplitScreen.P2Pick = "Player Two";
            Note($"split setup: players {(SplitScreen.Solo ? 1 : 2)}, P1 {SplitScreen.DeviceName(SplitScreen.P1Device)}, P2 {(SplitScreen.P2Device != null ? SplitScreen.DeviceName(SplitScreen.P2Device) : "none")}, ready {SplitScreen.Ready}"); yield return Snap("A-setup-split-from-main-menu");
            yield return FocusRow("stunt-start", 30); yield return Press(GamepadButton.South);
            yield return WaitEvent(); Note($"event: scene {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}, players {StuntEvent.Current?.Riders.Count}, from Free Roam {SplitScreen.FromRoam}");
            yield return WaitResults(); yield return FocusRow("menu"); yield return Press(GamepadButton.South);
            yield return WaitMenu(); Note($"back: state {flow.State}, scene {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}, in Free Roam {flow.InRoamWorld}, page '{Page}'");
            InputSystem.RemoveDevice(pad2);
            // from Free Roam, far from the park
            yield return ToFreeRoam("original"); var car = race.vehicle; Put(car, new Vector3(-400, 0, -300), 0, 0); yield return new WaitForSeconds(1.5f);
            float far = StuntPark.Current ? Vector3.Distance(car.Body.position, StuntPark.StartPos) : -1; Note($"Free Roam: vehicle at {V(car.Body.position)}, {far:F0} m from the park's start line");
            yield return Press(GamepadButton.Start); yield return new WaitForSecondsRealtime(1f);
            Note($"pause menu rows: {MainRows()}"); yield return Snap("A-free-roam-menu");
            yield return FocusRow("stunt"); yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(.8f);
            Note($"A: '{Texts("Leave Free Roam")}'"); yield return Snap("A-free-roam-question");
            yield return FocusRow("confirm", 3); yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(1f);
            Note($"confirmed: page '{Page}'"); if (!SplitScreen.Solo) { yield return FocusRow("stunt-players"); yield return Press(GamepadButton.DpadRight); }
            yield return FocusRow("stunt-start"); yield return Press(GamepadButton.South);
            yield return WaitEvent(); Note($"event: players {StuntEvent.Current?.Riders.Count}, from Free Roam {SplitScreen.FromRoam}");
            yield return WaitResults(); yield return FocusRow("menu"); Note($"focus '{Focus}' ('{Texts("Main menu", "Back to Free Roam")}')"); yield return Press(GamepadButton.South);
            float b0 = Time.realtimeSinceStartup; while (Time.realtimeSinceStartup - b0 < 120) { Bind(); if (flow && flow.Started && !LoadingScreen.Holding && flow.State == RaceFlow.Stage.Racing && !SplitScreen.Active) break; yield return null; }
            yield return new WaitForSecondsRealtime(1.5f); Note($"back: state {flow.State}, in Free Roam {flow.InRoamWorld}, split-screen {SplitScreen.Active}"); yield return Snap("A-back-in-free-roam");
            if (ResultsLog.FilePath != null && File.Exists(ResultsLog.FilePath)) Note($"(results log at {ResultsLog.FilePath}: {File.ReadAllLines(ResultsLog.FilePath).Length} lines)");
        }

        // ---------------------------------------------------------------- Part B: the Top 10 Best Combos on the results
        string ResultRows() => string.Join("\n", FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None).Where(b => b.isActiveAndEnabled && (b.name.StartsWith("combo-top") || b.name.StartsWith("stunt-top") || b.name.StartsWith("stunt-player") || b.name == "header"))
            .OrderBy(b => b.transform.parent.GetSiblingIndex() * 100 + b.transform.GetSiblingIndex()).Select(b => "    " + b.name + ": " + string.Join(" | ", b.GetComponentsInChildren<UnityEngine.UI.Text>().Where(t => t.name == "Cell").Select(t => t.text))));
        IEnumerator ResultsTop10()
        {
            int players = int.Parse(Arg("-p106players", "1")); Conditions(); var notes = new List<string>();
            yield return PanelLoop("results-start");
            // a full list: small filler combos added to the test save until there are 10 (a test shortcut; this event's combos then enter it)
            for (int k = 0; StuntProgress.Combos.Count < 10 && k < 10; k++) StuntProgress.RecordComboTop("Filler", "stuntbuggy", 600 + 50 * k, 1, "J");
            Note($"test save's Top 10 Best Combos before: {string.Join("; ", StuntProgress.Combos.Select(c => $"{c.points:N0} x{c.links} {c.name}"))}");
            var pad2 = players > 1 ? InputSystem.AddDevice<Gamepad>("Report109Pad2") : null;
            yield return StartStunt(players, 60, pad2); var ev = StuntEvent.Current; if (!ev) yield break;
            yield return Combo(pad, race.vehicle, P(112, 40), 3, notes, "P1 combo x3");
            if (players > 1 && SplitRoam.Current) yield return Combo(pad2, SplitRoam.Current.Car, P(120, 90), 3, notes, "P2 combo x3");
            else yield return Combo(pad, race.vehicle, P(112, 40), 4, notes, "P1 combo x4");
            foreach (var n in notes) Note(n); notes.Clear();
            yield return WaitResults(90);
            Note($"results ({players}p, {Screen.width}x{Screen.height}): state {flow.State}; tabs '{Texts("✓ ")}'; this event's entries: {string.Join(", ", ev.Riders.SelectMany(r => r.entered.Select(c => $"{SplitScreen.NameOf(r.player)} {c.points:N0} -> #{StuntProgress.Combos.ToList().IndexOf(c) + 1}")))}");
            Note(ResultRows()); yield return Snap($"B-results-combos-{players}p-{Screen.width}x{Screen.height}");
            yield return Press(GamepadButton.RightShoulder); yield return new WaitForSecondsRealtime(.6f);
            Note($"RB: tabs '{Texts("✓ ")}'"); Note(ResultRows()); yield return Snap($"B-results-event-top10-{players}p-{Screen.width}x{Screen.height}");
            yield return Press(GamepadButton.LeftShoulder); yield return new WaitForSecondsRealtime(.6f); Note($"LB: tabs '{Texts("✓ ")}'");
            yield return FocusRow("menu"); yield return Press(GamepadButton.South); yield return WaitMenu(); yield return PanelLoop("results-after");
            Note($"back: state {flow.State}");
        }
    }
}
