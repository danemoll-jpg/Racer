using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Racer
{
    // 0.110 evidence only, on the 0.106 check (same switches: -p106check <dir> -racerTestSave <dir>; never active without both):
    //   -p106kind stunt110    Part B: the Stunt setup page's chain-time text (controller) and the event's start caption; Part A: two split-screen events
    //                         on a full Stunt event Top 10, the players' points set so they land next to each other, then far apart (a test shortcut):
    //                         the results' "(Top 10 #n)" and the list; Part C: the divider on the Stunt results
    //   -p106kind race110     Part C: the divider in a split-screen race (driving, paused, resumed, results) and a two-player Getaway's results
    //   -p106kind vehicle110  Part D by controller: campaign event 1 driven on X and left, events 2, 3 and a championship preselect X; Y driven in one,
    //                         the next shows Y; Police Chase, Stunt event, split-screen and Race preselect Y; player 2's vehicle in split-screen
    //   -p106kind vehicle110b Part D after a restart on the same test save: still Y
    //   -p106kind fresh110    Part E by controller: MY CAMPAIGN as it is; FRESH RUN from chapter 1 with the starters only; a vehicle bought in the
    //                         fresh run's Shop; back to MY CAMPAIGN unchanged; Race garage unchanged; log lines tagged; Restart fresh run; Settings rows
    public sealed partial class Report106PlayerCheck
    {
        static GameObject Divider => Resources.FindObjectsOfTypeAll<RectTransform>().FirstOrDefault(r => r && r.name == "Split-screen divider" && r.gameObject.scene.IsValid())?.gameObject;
        string DividerState => Divider ? (Divider.activeInHierarchy ? "SHOWN" : "hidden") : "none";
        UnityEngine.UI.Button ActiveButton(string id) => FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None).FirstOrDefault(b => b.isActiveAndEnabled && b.name == id);
        string RowText(string id) { var b = ActiveButton(id); return b ? b.GetComponentInChildren<UnityEngine.UI.Text>(true).text.Replace("\n", " / ") : "(no row " + id + ")"; }
        // the controller's focus put on a row (the campaign's two panes), then A on the pad
        IEnumerator Choose(string id)
        {
            var b = ActiveButton(id); if (!b) { Note($"  (row {id} not shown; page '{Page}')"); yield break; }
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(b.gameObject); yield return null;
            yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(.8f);
        }
        IEnumerator Back() { yield return Press(GamepadButton.East); yield return new WaitForSecondsRealtime(.8f); }
        string Title => FindAnyObjectByType<RaceMenus>() ? Texts("CAMPAIGN", "SHOP", "SETTINGS", "STUNT EVENT", "POLICE", "SPLIT") : "";

        // ---------------------------------------------------------------- Parts A, B, C: the Stunt event
        IEnumerator Stunt110()
        {
            Conditions(); flow.Save.Settings.hints = false; flow.Save.SaveSettings(); yield return PanelLoop("stunt110-start");
            // Part B: the setup page by controller
            yield return FocusRow("stunt"); yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(1f);
            Note($"Part B: page '{Page}'; ComboGap {ArcadeActivities.ComboGap} s; setup text: '{Texts(StuntPark.Title)}'"); yield return Snap("B-stunt-setup");
            yield return Back(); Note($"B: page '{Page}'");
            // a full Stunt event Top 10 (fillers added to the test save: a test shortcut)
            for (int k = 0; k < 10; k++) StuntProgress.Finish(2, "Filler " + (k + 1), "stuntbuggy", 95000 - 10000 * k, 1000);
            Note("Stunt event Top 10 before: " + string.Join("; ", StuntProgress.Top.Select((t, i) => $"#{i + 1} {t.points:N0} {t.name}")));
            var pad2 = InputSystem.AddDevice<Gamepad>("Report110Pad2");
            for (int run = 0; run < 2; run++)
            {
                var top = StuntProgress.Top.Select(t => t.points).ToList();
                int p1 = run == 0 ? top[3] - 1 : top[0] + 1, p2 = run == 0 ? top[3] - 2 : top[6] - 1; // next to each other just under #4; far apart: above #1 and under #7
                StuntEvent.TestLimit = 6; SplitScreen.Mode = SplitScreen.Kind.Stunt; SplitScreen.Solo = false; SplitScreen.P1Device = pad; SplitScreen.P2Device = pad2; SplitScreen.P2Ai = false;
                SplitScreen.P1Vehicle = "stuntbuggy"; SplitScreen.P2Vehicle = "stuntbuggy"; SplitScreen.FromRoam = false; SplitScreen.P1Name = "Player One"; SplitScreen.P2Pick = "Player Two";
                flow.StartSplit(); string caption = ""; float r0 = Time.realtimeSinceStartup;
                while (Time.realtimeSinceStartup - r0 < 150) { Bind(); if (flow && StuntEvent.Current && StuntEvent.Current.State == StuntEvent.Phase.Starting && caption == "") caption = Texts("<size=26>STUNT EVENT"); if (flow && StuntEvent.Current && StuntEvent.Current.State == StuntEvent.Phase.Running) break; yield return null; }
                AudioListener.volume = 0; var ev = StuntEvent.Current; if (!ev || ev.Riders.Count < 2) { Note("no two-player event"); break; }
                if (run == 0) Note($"Part B: the event's start caption: '{caption}'");
                ev.Riders[0].points = p1; ev.Riders[1].points = p2;
                Note($"Part A run {run + 1} ({(run == 0 ? "next to each other" : "far apart")}): points set to P1 {p1:N0}, P2 {p2:N0} (a test shortcut); round {StuntEvent.TestLimit} s; divider while driving {DividerState}");
                yield return WaitResults(90);
                Note($"  results: {RowText("stunt-player-1")} || {RowText("stunt-player-2")}; riders' places P1 #{ev.Riders[0].place}, P2 #{ev.Riders[1].place}");
                Note("  Records (Stunt event Top 10 now): " + string.Join("; ", StuntProgress.Top.Select((t, i) => $"#{i + 1} {t.points:N0} {t.name}")));
                Note($"  the list agrees: P1 #{StuntProgress.Top.ToList().FindIndex(t => t.name == "Player One" && t.points == p1) + 1}, P2 #{StuntProgress.Top.ToList().FindIndex(t => t.name == "Player Two" && t.points == p2) + 1}");
                Note($"  Part C: divider on the Stunt results: {DividerState}"); yield return Snap($"AC-stunt-results-2p-run{run + 1}");
                yield return FocusRow("menu"); yield return Press(GamepadButton.South); yield return WaitMenu(); yield return PanelLoop("stunt110-after-" + run);
            }
            InputSystem.RemoveDevice(pad2);
        }

        // ---------------------------------------------------------------- Part C: a split-screen race and a two-player Getaway
        IEnumerator Race110()
        {
            Conditions(); var pad2 = InputSystem.AddDevice<Gamepad>("Report110Pad2");
            SplitScreen.Mode = SplitScreen.Kind.Race; SplitScreen.Solo = false; SplitScreen.P1Device = pad; SplitScreen.P2Device = pad2; SplitScreen.P2Ai = false; SplitScreen.Course = 0; SplitScreen.Laps = 1; SplitScreen.Rivals = 1; SplitScreen.Traffic = false;
            SplitScreen.P1Vehicle = "original"; SplitScreen.P2Vehicle = "atv"; SplitScreen.P1Name = "Player One"; SplitScreen.P2Pick = "Player Two"; flow.StartSplit();
            yield return new WaitForSecondsRealtime(2); yield return WaitRacing(); yield return new WaitForSecondsRealtime(1);
            Note($"split-screen race: state {flow.State}, divider while driving: {DividerState}"); yield return Snap("C-race-driving");
            flow.Pause(); yield return new WaitForSecondsRealtime(1); Note($"paused: state {flow.State}, divider: {DividerState}"); yield return Snap("C-race-paused");
            flow.Resume(); yield return new WaitForSecondsRealtime(1); Note($"resumed: state {flow.State}, divider: {DividerState}");
            Pilot(race.vehicle, race.Racers[0]); if (SplitScreen.Race && SplitScreen.Race.P2Car) Pilot(SplitScreen.Race.P2Car, race.Racers[1]);
            yield return Drive(400, () => flow.State == RaceFlow.Stage.Results); yield return new WaitForSecondsRealtime(1.5f);
            Note($"race results: state {flow.State}, divider: {DividerState}"); yield return Snap("C-race-results");
            flow.QuitSplit(false); yield return WaitMenu();
            // a two-player Getaway (1-minute round: a test shortcut)
            SplitScreen.Mode = SplitScreen.Kind.Police; SplitScreen.PoliceGame = SplitScreen.Game.Getaway; SplitScreen.Solo = false; SplitScreen.P1Device = pad; SplitScreen.P2Device = pad2; SplitScreen.P2Ai = false;
            SplitScreen.PoliceMinutes = 1; SplitScreen.PoliceDifficulty = 1; SplitScreen.P1Vehicle = "original"; SplitScreen.P2Vehicle = "atv"; SplitScreen.FromRoam = false; SplitScreen.Time = TimeOfDay.Day; SplitScreen.Weather = Weather.Clear; flow.StartSplit();
            yield return new WaitForSecondsRealtime(2); yield return WaitRacing(); yield return new WaitForSecondsRealtime(1);
            Note($"Getaway (2 players): state {flow.State}, divider while driving: {DividerState}"); yield return Snap("C-getaway-driving");
            yield return Drive(90, () => flow.State == RaceFlow.Stage.Results); yield return new WaitForSecondsRealtime(1.5f);
            Note($"Getaway results: state {flow.State}, divider: {DividerState}"); yield return Snap("C-getaway-results");
            flow.QuitSplit(false); yield return WaitMenu(); SplitScreen.PoliceMinutes = 5; InputSystem.RemoveDevice(pad2);
        }

        // ---------------------------------------------------------------- Part D: remember the vehicle
        static readonly string[] Events110 = { "c1-first-lap", "c1-night-shift", "c1-wrong-way" };
        static VehicleProfile[] Allowed(CampaignEvent e) => VehicleProfile.All.Where(p => !p.Reward && Campaign.Owns(p.Id) && e.Allows(p) && (CarAccess.CourseAllowsCars(RacePlaylists.Scenes[e.Course]) || p.Small)).ToArray();
        string Recent => $"recent P1 [{string.Join(",", flow.Save.Settings.recentVehicles ?? new string[0])}], P2 [{string.Join(",", flow.Save.Settings.recentVehiclesP2 ?? new string[0])}], Race vehicle '{flow.Save.Settings.vehicleId}', campaign driven [{string.Join(",", Campaign.Current.driven)}]";
        IEnumerator OpenEvent(string id) { yield return Choose("cev-" + id); Note($"  event '{id}': page '{Page}', {RowText("event-vehicle")}"); }
        IEnumerator StepTo(string vehicle)
        {
            var row = ActiveButton("event-vehicle"); if (!row) yield break; UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(row.gameObject); yield return null;
            for (int i = 0; i < 12 && !RowText("event-vehicle").Contains("Vehicle:   " + VehicleProfile.Find(vehicle).Name + " "); i++) yield return Press(GamepadButton.DpadRight);
        }
        IEnumerator DriveAndLeave(string what)
        {
            yield return Choose("start-event"); yield return new WaitForSecondsRealtime(2); yield return WaitRacing(); yield return new WaitForSecondsRealtime(2);
            Note($"  {what}: driving {race.vehicle.GetComponent<VehicleConfiguration>().profileId} in {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}; then Start and quit");
            yield return Press(GamepadButton.Start); yield return new WaitForSecondsRealtime(.6f); flow.QuitRace(); yield return WaitMenu(); Note($"  back: page '{Page}'; {Recent}");
        }
        IEnumerator VehicleMemory()
        {
            Conditions(); yield return PanelLoop("vehicle110-start");
            Note($"before (copy of Dan's save): {Recent}");
            var e1 = CampaignData.Find(Events110[0]); var allowed = Allowed(e1); var cup = CampaignData.Cups[0];
            yield return FocusRow("campaign"); yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(1f); Note($"campaign page: '{Title}'");
            yield return Choose("chapter-1"); Note($"A on chapter 1: focus '{Focus}'");
            yield return OpenEvent(e1.Id);
            string shown = Campaign.Current.driven.FirstOrDefault();
            string x = allowed.Select(p => p.Id).First(id => !RowText("event-vehicle").Contains(VehicleProfile.Find(id).Name)); // one not preselected now
            string y = allowed.Select(p => p.Id).First(id => id != x && !RowText("event-vehicle").Contains(VehicleProfile.Find(id).Name));
            Note($"X = {x}, Y = {y} (allowed in {e1.Id}: {string.Join(",", allowed.Select(p => p.Id))})");
            yield return StepTo(x); Note($"  stepped: {RowText("event-vehicle")}"); yield return DriveAndLeave("event 1 on X");
            foreach (var id in Events110.Skip(1)) { yield return OpenEvent(id); yield return Back(); }
            yield return Choose("ccup-" + cup.Id); Note($"  championship '{cup.Id}': page '{Page}', {RowText("event-vehicle")}"); yield return Snap("D-cup-preselect-X"); yield return Back();
            yield return OpenEvent(Events110[1]); yield return StepTo(y); Note($"  stepped: {RowText("event-vehicle")}"); yield return DriveAndLeave("event 2 on Y");
            yield return OpenEvent(Events110[2]); yield return Snap("D-event3-preselect-Y"); yield return Back();
            yield return Back(); Note($"main menu: page '{Page}'");
            // the other setups
            yield return FocusRow("police"); yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(1); Note($"Police Chase setup: page '{Page}', game {SplitScreen.PoliceGame}, player 1's vehicle {SplitScreen.P1Vehicle}"); yield return Back();
            yield return FocusRow("stunt"); yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(1); Note($"Stunt event setup: page '{Page}', {RowText("stunt-p1-vehicle")} ({SplitScreen.P1Vehicle})"); yield return Snap("D-stunt-preselect-Y"); yield return Back();
            yield return FocusRow("split"); yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(1); Note($"Split-screen setup: page '{Page}', player 1 {SplitScreen.P1Vehicle}, player 2 {SplitScreen.P2Vehicle}"); yield return Back();
            Note($"Race: settings vehicle '{flow.Save.Settings.vehicleId}', Race uses '{flow.PlayerVehicleId}'");
            // player 2: a two-player Stunt event with player 2 on the moto (player 1 on Y), left at once; the split-screen setup then preselects it
            var pad2 = InputSystem.AddDevice<Gamepad>("Report110Pad2");
            StuntEvent.TestLimit = 4; SplitScreen.Mode = SplitScreen.Kind.Stunt; SplitScreen.Solo = false; SplitScreen.P1Device = pad; SplitScreen.P2Device = pad2; SplitScreen.P2Ai = false;
            SplitScreen.P1Vehicle = y; SplitScreen.P2Vehicle = "atv"; SplitScreen.FromRoam = false; flow.StartSplit(); yield return WaitEvent(); Note($"two-player Stunt event: P1 {SplitScreen.P1Vehicle}, P2 {SplitScreen.P2Vehicle}");
            yield return WaitResults(60); yield return FocusRow("menu"); yield return Press(GamepadButton.South); yield return WaitMenu(); yield return PanelLoop("vehicle110-after-stunt");
            SplitScreen.P1Vehicle = "original"; SplitScreen.P2Vehicle = "atv";
            yield return FocusRow("split"); yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(1); Note($"Split-screen setup again: player 1 {SplitScreen.P1Vehicle}, player 2 {SplitScreen.P2Vehicle}"); yield return Back();
            InputSystem.RemoveDevice(pad2);
            Note($"end: {Recent}");
        }
        IEnumerator VehicleMemoryAfterRestart()
        {
            Conditions(); yield return PanelLoop("vehicle110b-start"); Note($"after a restart: {Recent}");
            yield return FocusRow("campaign"); yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(1f);
            yield return Choose("chapter-1"); yield return OpenEvent(Events110[0]); yield return Snap("D-after-restart-event1"); yield return Back(); yield return Back();
            yield return FocusRow("police"); yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(1); Note($"Police Chase setup: player 1's vehicle {SplitScreen.P1Vehicle}"); yield return Back();
        }

        // ---------------------------------------------------------------- Part E: the fresh run
        string Hash(string file) { if (!File.Exists(file)) return "missing"; using var sha = System.Security.Cryptography.SHA256.Create(); return System.BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(file))).Replace("-", "").Substring(0, 16); }
        string Slot => $"slot {Campaign.SlotName}: chapter {Campaign.Current.chapter}, {Campaign.Money(Campaign.Current.money)}, owns [{string.Join(",", Campaign.Current.owned)}], courses {Campaign.Current.courses.Count}, results {Campaign.Current.results.Count}";
        string Locks => string.Join(",", VehicleProfile.All.Where(p => Campaign.VehicleLocked(p)).Select(p => p.Id)) + " | courses open in Race: " + Enumerable.Range(0, RacePlaylists.Scenes.Length).Count(i => Campaign.CourseOpen(i));
        IEnumerator StartAndQuit(string id, string vehicle, string what)
        {
            yield return Choose("chapter-1"); yield return OpenEvent(id); yield return StepTo(vehicle); yield return Choose("start-event"); yield return new WaitForSecondsRealtime(2); yield return WaitRacing(); yield return new WaitForSecondsRealtime(2);
            yield return Press(GamepadButton.Start); yield return new WaitForSecondsRealtime(.6f); flow.QuitRace(); yield return WaitMenu();
            var path = ResultsLog.FilePath; var last = path != null && File.Exists(path) ? File.ReadAllLines(path).LastOrDefault() ?? "" : "";
            int at = last.IndexOf("\"slot\""); Note($"  {what}: log line {(at >= 0 ? last.Substring(at, Mathf.Min(260, last.Length - at)) : "(no campaign block) " + last.Substring(0, Mathf.Min(200, last.Length)))}");
        }
        IEnumerator Fresh110()
        {
            Conditions(); yield return PanelLoop("fresh110-start");
            string dir = flow.Save.DirectoryPath, main = Path.Combine(dir, Campaign.File), fresh = Path.Combine(dir, Campaign.FreshFile);
            string mainHash = Hash(main), locks = Locks; Note($"start: main file {mainHash}, fresh file {Hash(fresh)}; {Slot}; Race locks: {locks}");
            yield return FocusRow("campaign"); yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(1f);
            Note($"CAMPAIGN page: '{Title}' / {Texts("Chapter ")}; slot row '{RowText("campaign-slot")}'"); yield return Choose("chapter-1"); yield return Snap("E-my-campaign");
            yield return Choose("campaign-slot"); Note($"A on the slot row: '{Title}' / {Texts("Chapter ")}; {Slot}"); yield return Snap("E-fresh-run");
            yield return OpenEvent("c1-first-lap"); var offered = new List<string>();
            var row = ActiveButton("event-vehicle"); UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(row.gameObject); yield return null;
            for (int i = 0; i < 8; i++) { var t = RowText("event-vehicle"); if (offered.Contains(t)) break; offered.Add(t); yield return Press(GamepadButton.DpadRight); }
            Note($"  vehicles offered in the fresh run: {string.Join(" || ", offered)}"); Note($"  details: {Texts("FRESH RUN")}"); yield return Snap("E-fresh-event-page"); yield return Back();
            // the fresh run's Shop: $7,000 put in the fresh run (a test shortcut: it starts at $0), the Roadster bought by controller
            Campaign.Current.money = 7000; yield return Choose("shop"); Note($"Shop: '{Title}'");
            var shopRow = ActiveButton("shop-vehicle"); UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(shopRow.gameObject); yield return null;
            for (int i = 0; i < 14 && !RowText("shop-vehicle").StartsWith(VehicleProfile.Find("roadster").Name); i++) yield return Press(GamepadButton.DpadRight);
            Note($"  {RowText("shop-vehicle")}; {RowText("buy")}"); yield return Choose("buy"); Note($"  confirm: '{Texts("BUY THE")}' {Texts("$7,000 of")}"); yield return FocusRow("confirm", 3); yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(1);
            Note($"  bought: {Slot}; fresh file {Hash(fresh)} owns roadster: {File.Exists(fresh) && File.ReadAllText(fresh).Contains("\"roadster\"")}; main file {Hash(main)} (unchanged: {Hash(main) == mainHash})"); yield return Snap("E-fresh-shop-bought");
            yield return Back(); Note($"back: page '{Page}'");
            yield return StartAndQuit("c1-against-clock", "original", "fresh run, Against the Clock (its first event) started and quit");
            yield return Choose("campaign-slot"); Note($"switched back: '{Title}'; {Slot}; main file unchanged: {Hash(main) == mainHash}; Race locks unchanged: {Locks == locks} ({Locks})"); yield return Snap("E-back-to-my-campaign");
            yield return StartAndQuit("c1-first-lap", "moto", "main campaign, First Lap started and quit");
            Note($"  main file after its own event start: {Hash(main)} (written by the main campaign itself: its driven list)");
            // Restart fresh run
            yield return Choose("campaign-slot"); Note($"fresh again: {Slot}");
            yield return Choose("restart-fresh"); Note($"  '{Texts("RESTART THE FRESH RUN")}' {Texts("The fresh run starts")}"); yield return Snap("E-restart-confirm"); yield return FocusRow("confirm", 3); yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(1);
            Note($"  restarted: {Slot}"); yield return Choose("campaign-slot"); Note($"  back to: {Slot}"); yield return Back();
            // Settings: no "Unlock everything" in a release build; an old settings.json with it on is off
            yield return FocusRow("settings"); yield return Press(GamepadButton.South); yield return new WaitForSecondsRealtime(1);
            Note($"Settings (development build: {Debug.isDebugBuild}): rows {string.Join(", ", FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None).Where(b => b.isActiveAndEnabled).OrderBy(b => b.transform.GetSiblingIndex()).Select(b => b.name))}"); yield return Snap("E-settings-gameplay"); yield return Back();
            flow.Save.Settings.unlockEverything = true; flow.Save.SaveSettings(); yield return ToFreeRoam("original");
            Note($"settings.json unlockEverything=true, then a scene load: Campaign.Testing {Campaign.Testing}, RaceFlow.UnlockEverything {flow.UnlockEverything}; Race locks {Locks}");
            flow.Save.Settings.unlockEverything = false; flow.Save.SaveSettings(); flow.Pause(); yield return null; flow.QuitRace(); yield return WaitMenu();
        }
    }
}
