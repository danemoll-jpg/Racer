using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Racer
{
    // 0.108 evidence only, on the 0.106 check (same switches: -p106check <dir> -racerTestSave <dir>; never active without both):
    //   -p106kind overtime  the Stunt event (-p106players 1|2, -p106limit seconds, shortened): the next-medal line at the start, at bronze, between bronze
    //                       and silver, at gold and after it (player 1's points set just before a real trick: a test shortcut); then a chain of backflips
    //                       started about 3 s before 0:00 and kept going past it, then the 5 s left to run out (-p106variant bail: a roof landing in
    //                       overtime instead). In split-screen player 2 does nothing (done at 0:00).
    //   -p106kind unlocks   -p106phase dan: Dan's save copy at the main menu; earn: an empty save, the stunt motorcycle's Gold and the patrol car at the
    //                       same time at the end of a Stunt event, then the patrol cycle (20 speeders) and a campaign prize in Free Roam (all test
    //                       shortcuts for the setup), then one raised in the event and the game closed before it shows; restart: that save again.
    //   -p106kind combos    Dan's save copy: Records > COMBOS by controller (the list started from the old save), three combos of different sizes in the
    //                       Stunt event, one in Free Roam, one by player 2 in split-screen, Records > COMBOS again
    public sealed partial class Report106PlayerCheck
    {
        IEnumerator StartStunt(int players, float limit, Gamepad pad2 = null)
        {
            StuntEvent.TestLimit = limit; Conditions(); flow.Save.Settings.hints = false; flow.Save.SaveSettings();
            SplitScreen.Mode = SplitScreen.Kind.Stunt; SplitScreen.Solo = players == 1; SplitScreen.P1Device = pad; SplitScreen.P2Device = pad2; SplitScreen.P2Ai = players == 1;
            SplitScreen.P1Vehicle = "stuntbuggy"; SplitScreen.P2Vehicle = "stuntbuggy"; SplitScreen.FromRoam = false; SplitScreen.Time = TimeOfDay.Day; SplitScreen.Weather = Weather.Clear;
            if (players > 1) { SplitScreen.P1Name = "Player One"; SplitScreen.P2Pick = "Player Two"; }
            flow.StartSplit(); float r0 = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - r0 < 150) { Bind(); if (flow && StuntEvent.Current && StuntEvent.Current.State == StuntEvent.Phase.Running) break; yield return null; }
            AudioListener.volume = 0; WorldLook.Current?.SetClock(WorldLook.Current.Day, 12);
            Note($"Stunt event: {players} player(s), round shortened to {limit} s (StuntEvent.TestLimit, checks only); running: {(StuntEvent.Current ? StuntEvent.Current.State.ToString() : "no event")}");
        }
        IEnumerator WaitMenu(float limit = 150)
        {
            float t0 = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - t0 < limit) { Bind(); if (flow && flow.Started && !LoadingScreen.Holding && flow.State == RaceFlow.Stage.Ready) break; yield return null; }
            yield return new WaitForSecondsRealtime(1.5f);
        }
        string Texts(params string[] starts) => string.Join(" | ", FindObjectsByType<UnityEngine.UI.Text>(FindObjectsSortMode.None).Where(t => t.isActiveAndEnabled && starts.Any(s => t.text.StartsWith(s))).Select(t => t.text.Replace("\n", " / ")));
        RaceMenus Menu => FindAnyObjectByType<RaceMenus>();
        string PanelText => Texts("CONGRATULATIONS", "You unlocked");
        // a flight from where the vehicle is: re-aimed toward `aim` where it stands (no teleport: a teleport counts as a recovery and banks the combo),
        // then launched upward as by a kicker, turned by `turn`; waits for the landing (or the bail)
        IEnumerator Hop(Gamepad g, ArcadeVehicle car, Vector3 aim, Vector3 turn, List<string> notes, string name)
        {
            var acts = ActsOf(car); var dir = Flat(aim - car.Body.position).normalized; if (dir.sqrMagnitude < .5f) dir = Vector3.forward;
            car.Body.rotation = Quaternion.LookRotation(dir, Vector3.up); car.Body.linearVelocity = dir * 10; car.Body.angularVelocity = Vector3.zero;
            yield return new WaitForFixedUpdate();
            int bails = acts.Bails, links = acts.ComboLinks; float left = acts.ComboLeft, at = StuntEvent.Current ? StuntEvent.Current.Clock : 0; var tilt = new float[2];
            yield return Launch(car, 10, turn, tilt);
            float t0 = Time.realtimeSinceStartup; while (Time.realtimeSinceStartup - t0 < 5 && flow.State == RaceFlow.Stage.Racing) { yield return null; Neutral(g); if (!acts.StuntBusy && (acts.Bails > bails || acts.ComboLinks != links)) break; }
            float settle = Time.realtimeSinceStartup; while (Time.realtimeSinceStartup - settle < .5f && flow.State == RaceFlow.Stage.Racing) { yield return null; Neutral(g); }
            var ev = StuntEvent.Current;
            notes.Add($"{name}: took off {(ev ? $"at {at:F1} s ({(at >= StuntEvent.TestLimit ? "after 0:00" : $"{StuntEvent.TestLimit - at:F1} s before 0:00")})" : "in Free Roam")} with {left:F1} s of chain time left -> {(acts.Bails > bails ? $"BAILED ({acts.LastBail})" : $"{acts.LastStunt.Replace("\n", " ")} {acts.LastStuntPoints} pts")}; combo x{acts.ComboLinks} {acts.ComboTotal:N0}");
        }
        // the first flight of a chain: from a standing start at `from` (a teleport), then the launch
        IEnumerator FirstFlight(Gamepad g, ArcadeVehicle car, Vector3 from, float yaw, Vector3 turn, List<string> notes, string name)
        {
            var acts = ActsOf(car); yield return Stand(g, car, from, yaw, 10, .4f); int links = acts.ComboLinks, bails = acts.Bails; var tilt = new float[2];
            yield return Launch(car, 10, turn, tilt);
            float t0 = Time.realtimeSinceStartup; while (Time.realtimeSinceStartup - t0 < 5 && flow.State == RaceFlow.Stage.Racing) { yield return null; Neutral(g); if (!acts.StuntBusy && (acts.Bails > bails || acts.ComboLinks != links)) break; }
            float settle = Time.realtimeSinceStartup; while (Time.realtimeSinceStartup - settle < .5f && flow.State == RaceFlow.Stage.Racing) { yield return null; Neutral(g); }
            notes.Add($"{name}: {(acts.Bails > bails ? $"BAILED ({acts.LastBail})" : $"{acts.LastStunt.Replace("\n", " ")} {acts.LastStuntPoints} pts")}; combo x{acts.ComboLinks} {acts.ComboTotal:N0}");
        }
        // a combo of `links` backflips (the first from a standing start), then left to run out (banked after 5 s on the ground)
        IEnumerator Combo(Gamepad g, ArcadeVehicle car, Vector3 from, int links, List<string> notes, string name, bool snap = false)
        {
            var acts = ActsOf(car);
            yield return FirstFlight(g, car, from, 0, new Vector3(-360, 0, 0), notes, name + " flight 1");
            for (int i = 1; i < links; i++) yield return Hop(g, car, i % 2 == 1 ? from : from + Vector3.forward * 40, new Vector3(-360, 0, 0), notes, name + " flight " + (i + 1));
            float t0 = Time.realtimeSinceStartup; string shown = "";
            while (Time.realtimeSinceStartup - t0 < 8 && acts.ComboLinks > 0 && flow.State == RaceFlow.Stage.Racing) { yield return null; Neutral(g); }
            yield return null; shown = (acts.StuntFeedback ?? "").Replace("\n", " / ");
            notes.Add($"{name}: banked x{acts.LastComboLinks} [{acts.LastComboTricks}] -> Top 10 combo place {(acts.LastComboPlace > 0 ? "#" + acts.LastComboPlace : "none")}; shown '{shown}'");
            if (snap) yield return Snap(name.Replace(" ", "-"));
        }

        // ---------------------------------------------------------------- Parts A and B: combo overtime, the goal on the HUD
        IEnumerator Overtime()
        {
            int players = int.Parse(Arg("-p106players", "1")); bool bail = Arg("-p106variant", "") == "bail"; float limit = F(Arg("-p106limit", "45"));
            var pad2 = players > 1 ? InputSystem.AddDevice<Gamepad>("Report108Pad2") : null;
            yield return StartStunt(players, limit, pad2); var ev = StuntEvent.Current; if (!ev) yield break;
            string tag = $"{players}p-{Screen.width}x{Screen.height}"; var c1 = race.vehicle; var r1 = ev.Of(1); var notes = new List<string>();
            Note($"screen {Screen.width}x{Screen.height}");
            string Goal(int p) => Texts("<b>BRONZE", "<b>SILVER", "<b>GOLD");
            yield return new WaitForSeconds(1.2f); Note($"B at the start: goal line '{Goal(1)}'"); yield return Snap("B-goal-start-" + tag);
            if (players > 1) { ev.Of(2).points = 31200; Note("player 2's points set to 31,200 (a test shortcut: between bronze and silver)"); }
            // bronze: 19,500 set (shortcut), then a real backflip banked crosses 20,000
            r1.points = 19500; Note("player 1's points set to 19,500 (a test shortcut), then a real backflip");
            yield return FirstFlight(pad, c1, P(112, 40), 0, new Vector3(-360, 0, 0), notes, "P1 backflip toward bronze");
            float t0 = Time.realtimeSinceStartup; while (Time.realtimeSinceStartup - t0 < 8 && r1.points < 20000) yield return null;
            yield return new WaitForSecondsRealtime(.25f); Note($"B bronze reached ({r1.points:N0}): '{Goal(1)}'"); yield return Snap("B-goal-bronze-moment-" + tag);
            yield return new WaitForSeconds(3f); Note($"B between bronze and silver: '{Goal(1)}'"); yield return Snap("B-goal-between-" + tag);
            r1.points = 89500; Note("player 1's points set to 89,500 (a test shortcut), then a real backflip");
            yield return FirstFlight(pad, c1, P(112, 40), 0, new Vector3(-360, 0, 0), notes, "P1 backflip toward gold");
            t0 = Time.realtimeSinceStartup; while (Time.realtimeSinceStartup - t0 < 8 && r1.points < 90000) yield return null;
            yield return new WaitForSecondsRealtime(.25f); Note($"B gold reached ({r1.points:N0}): '{Goal(1)}'"); yield return Snap("B-goal-gold-moment-" + tag);
            yield return new WaitForSeconds(3f); Note($"B after gold: '{Goal(1)}'"); yield return Snap("B-goal-after-gold-" + tag);
            foreach (var n in notes) Note(n); notes.Clear();
            // Part A: the chain starts so that its first takeoff is about 3 s before 0:00
            while (ev.Left > 5.8f) yield return null;
            Note($"Part A: chain started at {ev.Clock:F1} s ({ev.Left:F1} s left)"); int before = r1.points;
            yield return FirstFlight(pad, c1, P(112, 40), 0, new Vector3(-360, 0, 0), notes, "P1 flight 1");
            int hops = bail ? 3 : 4; bool shot = false;
            for (int i = 1; i < hops; i++)
            {
                bool roof = bail && i == hops - 1;
                yield return Hop(pad, c1, i % 2 == 1 ? P(112, 20) : P(112, 90), roof ? new Vector3(-180, 0, 0) : new Vector3(-360, 0, 0), notes, $"P1 flight {i + 1}{(roof ? " (half backflip: onto the roof)" : "")}");
                if (!shot && ev.Overtime && !roof) { shot = true; Note($"HUD in overtime: '{Texts("<color=#FFC747>").Replace("<color=#FFC747>", "").Replace("</color>", "")}'"); yield return Snap("A-overtime-" + tag); }
            }
            foreach (var n in notes) Note(n); notes.Clear();
            if (players > 1) { yield return new WaitForSeconds(.3f); Note($"split-screen: player 2's half: '{Texts("<size=28>WAITING")}'"); yield return Snap("A-waiting-" + tag); }
            float o0 = Time.realtimeSinceStartup; while (ev.State != StuntEvent.Phase.Done && Time.realtimeSinceStartup - o0 < 20) { yield return null; Neutral(pad); }
            Note($"event done {(ev.State == StuntEvent.Phase.Done ? "" : "NOT ")}{Time.realtimeSinceStartup - o0:F1} s after the last flight; player 1 {before:N0} -> {r1.points:N0}");
            float w0 = Time.realtimeSinceStartup; while (Time.realtimeSinceStartup - w0 < 15 && flow.State != RaceFlow.Stage.Results) yield return null;
            yield return new WaitForSecondsRealtime(1.5f);
            Note($"results: state {flow.State}");
            foreach (var r in ev.Riders) { Note($"  {SplitScreen.NameOf(r.player)}: {r.points:N0} pts, medal {r.medal}, combos {r.combos}, best {r.best:N0}, tricks {r.acts.Tricks}, bails {r.acts.Bails}"); foreach (var l in r.log.Skip(Mathf.Max(0, r.log.Count - 10))) Note("      " + l); }
            yield return Snap("A-results-" + tag);
        }

        // ---------------------------------------------------------------- Part C: a congratulations for every vehicle unlock
        IEnumerator PanelLoop(string tag, int max = 4)
        {
            for (int i = 0; i < max; i++)
            {
                float t0 = Time.realtimeSinceStartup; while (Time.realtimeSinceStartup - t0 < 4 && !Menu.UnlockOpen) yield return null;
                if (!Menu.UnlockOpen) { Note($"{tag}: no (more) panels; state {flow.State}, page '{Page}'"); yield break; }
                yield return new WaitForSecondsRealtime(1.2f);
                Note($"{tag}: panel {i + 1}: '{PanelText}' ({UnlockNotice.Showing?.vehicle}); state {flow.State}; still waiting: {string.Join(",", UnlockNotice.Pending.Select(n => n.id))}");
                yield return Snap($"C-{tag}-panel-{i + 1}-{UnlockNotice.Showing?.id}");
                yield return Press(GamepadButton.East); yield return new WaitForSecondsRealtime(.5f); Note($"{tag}: B on the panel: still open {Menu.UnlockOpen}");
                yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(1f);
                Note($"{tag}: A: open {Menu.UnlockOpen}; state {flow.State}, page '{Page}'; seen [{string.Join(",", flow.Save.Settings.unlocksSeen ?? new string[0])}], pending in settings [{string.Join(",", flow.Save.Settings.unlocksPending ?? new string[0])}]");
            }
        }
        IEnumerator Unlocks()
        {
            string phase = Arg("-p106phase", "earn"); Conditions();
            Note($"phase {phase}; save {flow.Save.DirectoryPath}; seen [{string.Join(",", flow.Save.Settings.unlocksSeen ?? new string[0])}], pending in settings [{string.Join(",", flow.Save.Settings.unlocksPending ?? new string[0])}]; gold {StuntProgress.GoldEarned}, patrol car {PoliceProgress.CarEarned}, patrol cycle {PoliceProgress.BikeEarned}");
            if (phase == "dan" || phase == "restart")
            {
                Note($"main menu: state {flow.State}, page '{Page}', queued [{string.Join(",", UnlockNotice.Pending.Select(n => n.id))}], showing {UnlockNotice.Showing?.id}");
                yield return PanelLoop(phase + "-menu");
                yield return ToFreeRoam("stuntbuggy"); yield return new WaitForSecondsRealtime(4);
                Note($"Free Roam (a scene load, same save): panel open {Menu.UnlockOpen}, state {flow.State}, queued [{string.Join(",", UnlockNotice.Pending.Select(n => n.id))}]");
                yield break;
            }
            // earn: the Stunt event's Gold (player 1's points set: a shortcut) and the patrol car (its goal's own Earn: a shortcut), at the same moment
            yield return StartStunt(1, 14); var ev = StuntEvent.Current; if (!ev) yield break;
            while (ev.Clock < 8) yield return null;
            ev.Of(1).points = 95000; typeof(PoliceProgress).GetMethod("Earn", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).Invoke(null, new object[] { true });
            Note($"at {ev.Clock:F1} s: player 1's points set to 95,000 and the patrol car's goal met (shortcuts); queued [{string.Join(",", UnlockNotice.Pending.Select(n => n.id))}], pending in settings [{string.Join(",", flow.Save.Settings.unlocksPending ?? new string[0])}]");
            float w0 = Time.realtimeSinceStartup; while (Time.realtimeSinceStartup - w0 < 30 && flow.State != RaceFlow.Stage.Results) yield return null;
            yield return new WaitForSecondsRealtime(2f);
            Note($"results: state {flow.State}, page '{Page}', panel open {Menu.UnlockOpen}, gold earned {StuntProgress.GoldEarned}, queued [{string.Join(",", UnlockNotice.Pending.Select(n => n.id))}]; title '{Texts("STUNT EVENT RESULTS")}'");
            yield return Snap("C-earn-results-before");
            yield return FocusRow("menu"); Note($"results: focus '{Focus}'; A");
            yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(.5f);
            yield return PanelLoop("earn-results");
            yield return WaitMenu(); Note($"after the panels the results' choice went ahead: state {flow.State}, page '{Page}', split-screen {SplitScreen.Active}");
            // the patrol cycle: 20 speeders (its goal's own count: a shortcut) while driving in Free Roam
            yield return ToFreeRoam("stuntbuggy"); for (int i = 0; i < PoliceProgress.SpeedersNeeded; i++) PoliceProgress.SpeederCaught(1);
            Note($"Free Roam: {PoliceProgress.SpeedersCaught} speeders caught (shortcut); patrol cycle earned {PoliceProgress.BikeEarned}");
            yield return PanelLoop("earn-roam-cycle");
            Note($"after A: state {flow.State} (driving again: {flow.State == RaceFlow.Stage.Racing})");
            // a campaign prize (raised as Campaign.Commit raises it on passing a chapter final: a shortcut)
            var finals = CampaignData.Events.Where(e => e.Prize != null).ToArray();
            UnlockNotice.Raise(flow.Save, UnlockNotice.For(UnlockNotice.Prize + finals[0].Prize)); Note($"campaign prize raised: {finals[0].Name} -> {finals[0].Prize}");
            yield return PanelLoop("earn-roam-prize");
            // one raised during a Stunt event (held back by split-screen), and the game closed before it shows
            yield return StartStunt(1, 60); yield return new WaitForSeconds(1);
            UnlockNotice.Raise(flow.Save, UnlockNotice.For(UnlockNotice.Prize + finals[1].Prize));
            yield return new WaitForSecondsRealtime(2); Note($"in the event: campaign prize {finals[1].Prize} raised; panel open {Menu.UnlockOpen}; pending in settings [{string.Join(",", flow.Save.Settings.unlocksPending ?? new string[0])}]; settings.json on disk has it: {File.ReadAllText(Path.Combine(flow.Save.DirectoryPath, "settings.json")).Contains(UnlockNotice.Prize + finals[1].Prize)}; closing the game now");
        }

        // ---------------------------------------------------------------- Part D: Top 10 best combos
        IEnumerator RecordsCombos(string tag)
        {
            yield return FocusRow("records"); yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(1f);
            for (int i = 0; i < 8 && !Shown("✓ COMBOS"); i++) yield return Press(GamepadButton.RightShoulder);
            Note($"Records ({tag}): COMBOS tab {(Shown("✓ COMBOS") ? "shown (by RB)" : "NOT shown")}; '{Texts("TOP 10 BEST COMBOS")}'");
            var cells = FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None).Where(b => b.isActiveAndEnabled && b.name.StartsWith("combo-top")).OrderBy(b => b.name == "combo-top-header" ? -1 : int.TryParse(b.name.Substring(10), out int k) ? k : 99)
                .Select(b => string.Join(" | ", b.GetComponentsInChildren<UnityEngine.UI.Text>().Where(t => t.name == "Cell").Select(t => t.text)));
            foreach (var c in cells) Note("    " + c);
            yield return Snap("D-records-combos-" + tag);
            yield return Press(GamepadButton.East); yield return new WaitForSecondsRealtime(.6f); Note($"B: state {flow.State}, page '{Page}'");
        }
        IEnumerator Combos()
        {
            Conditions(); var notes = new List<string>();
            yield return PanelLoop("combos-start"); // Dan's save copy: any panel waiting is closed first
            Note($"the list from the old save: {string.Join("; ", StuntProgress.Combos.Select(c => $"{c.points:N0} {c.vehicle} {(string.IsNullOrEmpty(c.name) ? "(no name: the player's)" : c.name)} x{c.links}"))}");
            yield return RecordsCombos("seeded");
            yield return StartStunt(1, 75); var ev = StuntEvent.Current; if (!ev) yield break; var c1 = race.vehicle;
            yield return Combo(pad, c1, P(112, 40), 1, notes, "event combo x1", true);
            yield return Combo(pad, c1, P(112, 40), 2, notes, "event combo x2", true);
            yield return Combo(pad, c1, P(112, 40), 3, notes, "event combo x3", true);
            foreach (var n in notes) Note(n); notes.Clear();
            float w0 = Time.realtimeSinceStartup; while (Time.realtimeSinceStartup - w0 < 90 && flow.State != RaceFlow.Stage.Results) yield return null;
            Note($"event over: {string.Join("; ", ev.Of(1).log)}");
            flow.QuitSplit(false); yield return WaitMenu(); yield return PanelLoop("after-event");
            yield return ToFreeRoam("stuntbuggy"); yield return new WaitForSeconds(1);
            yield return Combo(pad, race.vehicle, P(112, 40), 2, notes, "Free Roam combo x2", true);
            foreach (var n in notes) Note(n); notes.Clear();
            flow.Pause(); yield return null; flow.QuitRace(); yield return WaitMenu();
            var pad2 = InputSystem.AddDevice<Gamepad>("Report108Pad2");
            yield return StartStunt(2, 45, pad2); ev = StuntEvent.Current; if (!ev) yield break;
            var c2 = SplitRoam.Current ? SplitRoam.Current.Car : null; Note($"split-screen: player 2 '{SplitScreen.NameOf(2)}' in {c2?.GetComponent<VehicleConfiguration>().profileId}");
            if (c2) yield return Combo(pad2, c2, P(120, 90), 2, notes, "split player 2 combo x2", true);
            foreach (var n in notes) Note(n); notes.Clear();
            w0 = Time.realtimeSinceStartup; while (Time.realtimeSinceStartup - w0 < 60 && flow.State != RaceFlow.Stage.Results) yield return null;
            flow.QuitSplit(false); yield return WaitMenu(); yield return PanelLoop("after-split");
            yield return RecordsCombos("after");
            var file = Path.Combine(flow.Save.DirectoryPath, StuntProgress.File); Note($"test save {file}: combos in the file {(File.Exists(file) ? File.ReadAllText(file).Split(new[] { "\"combos\"" }, System.StringSplitOptions.None).Length > 1 : false)}; bestCombo kept {StuntProgress.BestCombo:N0}");
        }
    }
}
