using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Racer
{
    // 0.106 Part D: the Stunt event's setup (main menu > FREE ROAM > STUNT EVENT…, and the Free Roam menu at the stunt park), its results and the
    // Records' STUNTS tab. Players 1 / 2 (split-screen), each player's vehicle (the stunt vehicles and the garage's own, left / right), time of day,
    // weather; the medals' points shown before the start (as the other events). Started from inside Free Roam it asks once and the menu afterwards goes
    // back to Free Roam (as Police Chase).
    public sealed partial class RaceMenus
    {
        public void OpenStuntSetup(bool keepPlayers = false)
        {
            if (!keepPlayers && flow.InRoamWorld && !SplitScreen.Active)
            {
                Confirm("STUNT EVENT", "Leave Free Roam and start the Stunt event at " + StuntPark.Title + "?\nYou come back to Free Roam afterwards.", () => { SplitScreen.FromRoam = true; SplitScreen.RoamReturn = RaceFlow.RoamCourse; StuntPage(false); }, "STUNT EVENT");
                return;
            }
            if (!keepPlayers) SplitScreen.FromRoam = false;
            StuntPage(keepPlayers);
        }
        static VehicleProfile[] StuntVehicles => SplitScreen.Vehicles.Where(v => !v.IsPolice).ToArray(); // 0.120 Part E: the stunt vehicles only
        // 0.120 Part E: player 1's stunt vehicle is saved as the stunt choice (as a Stunt Race's garage saves it) and is the default next time
        void RememberStunt(string id) { var p = VehicleProfile.Find(id); if (p.Id == id && p.Stunt && flow.Save.Settings.stuntVehicleId != id) { flow.Save.Settings.stuntVehicleId = id; flow.Save.SaveSettings(); } }
        static string NextStuntVehicle(string id, int d) { var list = StuntVehicles; int i = System.Array.FindIndex(list, v => v.Id == id); return list[((i < 0 ? 0 : i) + d + list.Length) % list.Length].Id; }
        void StuntPage(bool keepPlayers)
        {
            var keyboard = (InputDevice)Keyboard.current; var opener = MenuInput.Controller && MenuInput.Pad != null ? MenuInput.Pad : keyboard;
            if (!keepPlayers || SplitScreen.P1Device == null || !SplitScreen.P1Device.added) SplitScreen.P1Device = opener;
            if (SplitScreen.P2Device != null && (!SplitScreen.P2Device.added || SplitScreen.P2Device == SplitScreen.P1Device)) SplitScreen.P2Device = null;
            SplitScreen.Mode = SplitScreen.Kind.Stunt; SplitScreen.P2Ai = SplitScreen.Solo;
            // 0.110 Part D: each player's newest driven vehicle the event allows; the stunt buggy for a player who has driven nothing yet
            if (!keepPlayers) { SplitScreen.P1Vehicle = VehicleProfile.StuntBuggy.Id; SplitScreen.P2Vehicle = VehicleProfile.StuntBuggy.Id; PreselectVehicles(StuntVehicles); if (StuntVehicles.Any(v => v.Id == flow.Save.Settings.stuntVehicleId)) SplitScreen.P1Vehicle = flow.Save.Settings.stuntVehicleId; }
            if (!StuntVehicles.Any(v => v.Id == SplitScreen.P1Vehicle)) SplitScreen.P1Vehicle = VehicleProfile.StuntBuggy.Id;
            if (!StuntVehicles.Any(v => v.Id == SplitScreen.P2Vehicle)) SplitScreen.P2Vehicle = VehicleProfile.StuntBuggy.Id;
            if (page != "stunt") { pageMemory[RaceFlow.Stage.Ready + "/stunt"] = ("stunt-players", 1f); Navigate("stunt"); } else Show(); // opens on its first row, scrolled to the top (the description and medals in view)
        }
        static string[] StuntTargetStrings => new[] { StuntProgress.Targets[2].ToString("N0") + " pts", StuntProgress.Targets[1].ToString("N0") + " pts", StuntProgress.Targets[0].ToString("N0") + " pts" };
        void RenderStuntSetup()
        {
            bool solo = SplitScreen.Solo; SplitScreen.Mode = SplitScreen.Kind.Stunt; SplitScreen.P2Ai = solo;
            ClearCore("STUNT EVENT", StuntPark.Title + ", across Hwy 92 from S Cherokee Ln: " + StuntProgress.Minutes + " minutes to score as many points as you can."
                + " Jumps, flips, rolls, spins, the loop and the barrel roll; chain them within " + ArcadeActivities.ComboGap.ToString("0.#") + " s on the ground for a combo multiplier. Land on your wheels: a bail loses that jump's tricks."
                + (solo ? "" : " Two players, split-screen: the most points wins.") + (SplitScreen.FromRoam ? "  Afterwards you go back to Free Roam." : ""));
            details.fontSize = 17; details.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 78;
            var medals = LaterGroup("Stunt medals", content, false, 40); medals.SetSiblingIndex(details.transform.GetSiblingIndex() + 1);
            MedalUi.TargetsRow(medals, details.font, StuntTargetStrings, 22, 30, 0, Color.white, "MEDALS");
            int n = 0;
            Step(n++, "stunt-players", "Players:   " + (solo ? "1   (full screen)" : "2   (split screen)"), d => { SplitScreen.Solo = !SplitScreen.Solo; SplitScreen.P2Ai = SplitScreen.Solo; flow.Click(); Show(); });
            int devices = -1;
            if (!solo)
            {
                devices = n;
                Step(n++, "split-p1", "Player 1:   " + (SplitScreen.P1Device == null ? "press A on a controller, or Enter" : SplitScreen.DeviceName(SplitScreen.P1Device) + "   ✓"), d => CycleSlot(1, d));
                Step(n++, "split-p2", "Player 2:   " + (SplitScreen.P2Device != null && !SplitScreen.P2Ai ? SplitScreen.DeviceName(SplitScreen.P2Device) + "   ✓" : "press A on another controller, or Enter"), d => CycleSlot(2, d));
                if (string.IsNullOrEmpty(SplitScreen.P1Name)) SplitScreen.P1Name = PlayerNames.Player;
                Step(n++, "split-p1-name", "Player 1 name:   " + SplitScreen.NameOf(1) + "   (A: new name)", d => { SplitScreen.P1Name = NextKnownName(SplitScreen.P1Name, d, SplitScreen.P2Pick); flow.Click(); Show(); },
                    () => OpenNameEntry("", v => { PlayerNames.Remember(v); flow.Save.SaveSettings(); SplitScreen.P1Name = v; return true; }, null, "PLAYER 1'S NAME"));
                if (string.IsNullOrEmpty(SplitScreen.P2Pick) || SplitScreen.P2Pick == SplitScreen.NameOf(1)) SplitScreen.P2Pick = PlayerNames.Known.FirstOrDefault(k => k != SplitScreen.NameOf(1)) ?? "";
                Step(n++, "split-p2-name", "Player 2 name:   " + (string.IsNullOrEmpty(SplitScreen.P2Pick) ? "press A to enter it" : SplitScreen.P2Pick + "   (A: new name)"), d => { SplitScreen.P2Pick = NextKnownName(SplitScreen.P2Pick, d, SplitScreen.NameOf(1)); flow.Click(); Show(); },
                    () => OpenNameEntry("", v => { if (v == SplitScreen.NameOf(1)) { keyboardError = "Player 1 already has that name."; return false; } PlayerNames.Remember(v); flow.Save.SaveSettings(); SplitScreen.P2Pick = v; return true; }, null, "PLAYER 2'S NAME"));
            }
            string Vehicle(string id) { var p = VehicleProfile.Find(id); return $"{p.Name} ({(p.Stunt ? "stunt " + p.Class.ToLowerInvariant() : p.Class)})"; }
            Step(n++, "stunt-p1-vehicle", (solo ? "Vehicle:   " : SplitScreen.NameOf(1) + ":   ") + Vehicle(SplitScreen.P1Vehicle), d => { SplitScreen.P1Vehicle = NextStuntVehicle(SplitScreen.P1Vehicle, d); RememberStunt(SplitScreen.P1Vehicle); flow.Click(); Show(); });
            int vehicleRow = n - 1;
            if (!solo) { Step(n++, "stunt-p2-vehicle", SplitScreen.NameOf(2) + ":   " + Vehicle(SplitScreen.P2Vehicle), d => { SplitScreen.P2Vehicle = NextStuntVehicle(SplitScreen.P2Vehicle, d); flow.Click(); Show(); }); vehicleRow = n - 1; }
            var strip = PreviewStrip("Stunt event vehicles", buttons[vehicleRow].transform.GetSiblingIndex() + 1, 170);
            PreviewCard(strip, VehicleProfile.Find(SplitScreen.P1Vehicle), SplitScreen.P1Color, false, (solo ? "You: " : SplitScreen.NameOf(1) + ": ") + VehicleProfile.Find(SplitScreen.P1Vehicle).Name, new Color(.3f, .95f, .81f), 250);
            if (!solo) PreviewCard(strip, VehicleProfile.Find(SplitScreen.P2Vehicle), SplitScreen.P2Color, false, SplitScreen.NameOf(2) + ": " + VehicleProfile.Find(SplitScreen.P2Vehicle).Name, new Color(1, .74f, .25f), 250);
            var locked = VehicleProfile.StuntBike; if (VehicleUnlocks.Locked(locked)) { var note = Label("Stunt motorcycle goal", strip, 17, 0); note.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().preferredWidth = 260; note.alignment = TextAnchor.MiddleCenter; note.color = new Color(1, .84f, .35f); note.text = "The " + locked.Name + " stunt motorcycle: " + StuntProgress.BikeGoal; tableCells.Add(note.gameObject); }
            var times = LookPresets.MenuOrder;
            Step(n++, "split-time", "Time of day:   " + SplitScreen.Time, d => { int i = System.Array.IndexOf(times, SplitScreen.Time); SplitScreen.Time = times[((i < 0 ? 1 : i) + d + times.Length) % times.Length]; flow.Click(); Show(); });
            Step(n++, "split-weather", "Weather:   " + SplitScreen.Weather, d => { SplitScreen.Weather = (Weather)(((int)SplitScreen.Weather + d + 3) % 3); flow.Click(); Show(); });
            if (!solo) Step(n++, "split-layout", "Screen:   " + (SplitScreen.LeftRight ? "Left / right" : "Top / bottom (player 1 on top)"), d => { flow.Save.Settings.splitLeftRight = !flow.Save.Settings.splitLeftRight; flow.Save.SaveSettings(); flow.Click(); Show(); });
            Row(n, "stunt-start", SplitScreen.Ready ? "START STUNT EVENT" : "START STUNT EVENT   (waiting for player 2)", flow.StartSplit);
            var colors = buttons[n].colors; colors.normalColor = new(.1f, .38f, .35f); buttons[n].colors = colors; buttons[n].GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 54; buttons[n++].interactable = SplitScreen.Ready;
            Row(n, "stunt-back", "Back", () => BackPage());
            if (devices >= 0) { DeviceGlyph(buttons[devices], SplitScreen.P1Device); DeviceGlyph(buttons[devices + 1], SplitScreen.P2Ai ? null : SplitScreen.P2Device); buttons[devices].GetComponentInChildren<UnityEngine.UI.Text>(true).color = new Color(.3f, .95f, .81f); buttons[devices + 1].GetComponentInChildren<UnityEngine.UI.Text>(true).color = SplitScreen.P2Device != null && !SplitScreen.P2Ai ? new Color(1, .74f, .25f) : new Color(.75f, .75f, .72f); }
        }
        // 0.109 Part B: the results show the Top 10 Best Combos first (this event's entries highlighted, "NEW #n"); RB / LB or the tabs: the Stunt event Top 10
        int stuntResultTab, stuntTabRound; StuntEvent stuntTabEvent;
        public bool StuntResultsShown => flow.State == RaceFlow.Stage.Results && SplitScreen.Active && StuntEvent.Current && CampaignRun.Active == null;
        public void NextStuntResultTab(int d) { stuntResultTab = (stuntResultTab + d + 2) % 2; Show(); }
        void RenderStuntResults()
        {
            var e = StuntEvent.Current; int w = e.Winner;
            if (stuntTabEvent != e || stuntTabRound != e.Round) { stuntTabEvent = e; stuntTabRound = e.Round; stuntResultTab = 0; }
            string verdict = e.Riders.Count < 2 ? "SCORE  " + (e.Riders.Count > 0 ? e.Riders[0].points.ToString("N0") : "0") : w == 0 ? "A DRAW" : SplitScreen.NameOf(w).ToUpperInvariant() + " WINS";
            ClearCore("STUNT EVENT RESULTS", verdict + "   ·   " + StuntProgress.Minutes + " minutes at " + StuntPark.Title + "   ·   " + SplitScreen.Time + " / " + SplitScreen.Weather);
            bool two = e.Riders.Count > 1; // two players: slightly shorter rows, so the whole Top 10 still fits at 1280x720
            var medals = LaterGroup("Stunt medals", content, false, two ? 34 : 40); medals.SetSiblingIndex(details.transform.GetSiblingIndex() + 1);
            MedalUi.TargetsRow(medals, details.font, StuntTargetStrings, two ? 20 : 22, two ? 26 : 30, e.Riders.Count > 0 ? e.Riders.Max(r => r.medal) : 0, Color.white, "MEDALS");
            int fresh = e.Riders.Sum(r => r.entered.Count(c => StuntProgress.Combos.Contains(c)));
            TabRow(new[] { "TOP 10 BEST COMBOS" + (fresh > 0 ? "  (" + fresh + " new)" : ""), "STUNT EVENT TOP 10" }, stuntResultTab, i => { stuntResultTab = i; Show(); });
            int n = 2; var widths = new[] { .26f, .14f, .12f, .18f, .14f, .16f };
            TableRow(n++, "header", new[] { "Player", "Points", "Medal", "Best combo", "Tricks", "Bails" }, widths, () => { });
            foreach (var r in e.Riders)
            {
                TableRow(n++, "stunt-player-" + r.player, new[] { SplitScreen.NameOf(r.player) + (r.place > 0 ? $"  (Top 10 #{r.place})" : ""), r.points.ToString("N0"), "", r.best > 0 ? r.best.ToString("N0") : "—", r.acts ? r.acts.Tricks.ToString() : "0", r.acts ? r.acts.Bails.ToString() : "0" }, widths, () => { }, r.player == w || e.Riders.Count == 1);
                CellBadge(n - 1, widths, 2, r.medal, 22);
            }
            Table(2, n, two ? 25 : 28, two ? 17 : 18); // compact, so the whole Top 10 fits under it at 1280x720
            int first = n;
            n = stuntResultTab == 0 ? EventCombos(n, e) : StuntTop(n, e.Riders.Where(r => r.place > 0).Select(r => r.place).ToArray());
            Table(first, n, two ? 22 : 25, two ? 16 : 17);
            Row(n, "rematch", "REMATCH", e.Rematch); var colors = buttons[n].colors; colors.normalColor = new(.1f, .38f, .35f); buttons[n].colors = colors; buttons[n].GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 54; n++;
            Row(n++, "change-setup", "Change setup", () => flow.QuitSplit(true));
            Row(n, "menu", SplitScreen.FromRoam ? "Back to Free Roam" : "Main menu", () => flow.QuitSplit(false));
            for (int k = n - 2; k <= n; k++) buttons[k].transform.SetAsLastSibling();
        }
        // 0.109 Part B: the Top 10 Best Combos on the results (rows from n): this event's entries highlighted with their place ("NEW #3"), in split-screen
        // in that player's color (their name in the Name column)
        int EventCombos(int n, StuntEvent e)
        {
            var top = StuntProgress.Combos; var w = new[] { .09f, .11f, .07f, .31f, .15f, .14f, .13f };
            TableRow(n++, "combo-top-header", new[] { "#", "Points", "Links", "Tricks", "Vehicle", "Name", "Date" }, w, () => { });
            for (int i = 0; i < top.Count; i++)
            {
                var c = top[i]; var by = e.Riders.FirstOrDefault(r => r.entered.Contains(c)); string tricks = string.IsNullOrEmpty(c.tricks) ? "—" : c.tricks.Length > 36 ? c.tricks.Substring(0, 35) + "…" : c.tricks;
                TableRow(n++, "combo-top-" + i, new[] { (by != null ? "NEW #" : "#") + (i + 1), c.points.ToString("N0"), c.links > 0 ? "×" + c.links : "—", tricks, VehicleProfile.Find(c.vehicle).Name, string.IsNullOrEmpty(c.name) ? PlayerNames.Player : c.name, c.date ?? "—" }, w, () => { }, by != null);
                if (by != null && e.Riders.Count > 1) foreach (var cell in buttons[n - 1].GetComponentsInChildren<UnityEngine.UI.Text>(true)) if (cell.name == "Cell") cell.color = by.player == 1 ? new Color(.3f, .95f, .81f) : new Color(1, .74f, .25f);
            }
            if (top.Count == 0) TableRow(n++, "combo-top-none", new[] { "", "", "", "No combos yet", "", "", "" }, w, () => { });
            return n;
        }
        // the Stunt event Top 10 (rows from n); mine: the places to highlight
        int StuntTop(int n, int[] mine, System.Collections.Generic.IReadOnlyList<StuntProgress.Entry> list = null) // 1.2: list = the HIGH board (Records > HIGH)
        {
            var top = list ?? StuntProgress.Top; var tw = new[] { .1f, .26f, .16f, .12f, .2f, .16f };
            TableRow(n++, "stunt-top-header", new[] { "Top 10", "Name", "Points", "Medal", "Vehicle", "Date" }, tw, () => { });
            for (int i = 0; i < top.Count; i++)
            {
                var t = top[i]; TableRow(n++, "stunt-top-" + i, new[] { "#" + (i + 1), t.name, t.points.ToString("N0"), "", VehicleProfile.Find(t.vehicle).Name, t.date }, tw, () => { }, mine.Contains(i + 1));
                CellBadge(n - 1, tw, 3, t.medal);
            }
            if (top.Count == 0) TableRow(n++, "stunt-top-none", new[] { "", "No Stunt event results yet", "", "", "", "" }, tw, () => { });
            return n;
        }
        // Records > STUNTS: the Stunt event Top 10 and Free Roam's best single combo
        void RenderStuntRecords(int row, int backRow)
        {
            details.gameObject.SetActive(true); details.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 56;
            details.text = "STUNT EVENT · " + StuntProgress.Minutes + " minutes at " + StuntPark.Title + "\nFree Roam's best single combo: " + (StuntProgress.BestCombo > 0 ? $"{StuntProgress.BestCombo:N0} pts ({VehicleProfile.Find(StuntProgress.BestComboVehicle).Name}, {StuntProgress.BestComboDate})" : "none yet");
            int first = row; row = StuntTop(row, new int[0]); Table(first, row); buttons[backRow].transform.SetAsLastSibling();
        }
        // 0.108 Part D: Records > COMBOS, beside STUNTS: the Top 10 Best Combos (the Stunt event and Free Roam in the park, every player)
        void RenderComboRecords(int row, int backRow)
        {
            details.gameObject.SetActive(true); details.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 56;
            details.text = "TOP 10 BEST COMBOS · the Stunt event and Free Roam at " + StuntPark.Title + "\nBF backflip · FF frontflip · R roll · BR barrel roll · 360 spin · LOOP · J jump";
            var top = StuntProgress.Combos; var w = new[] { .06f, .11f, .07f, .33f, .15f, .14f, .14f }; int first = row;
            TableRow(row++, "combo-top-header", new[] { "#", "Points", "Links", "Tricks", "Vehicle", "Name", "Date" }, w, () => { });
            for (int i = 0; i < top.Count; i++)
            {
                var c = top[i]; string tricks = string.IsNullOrEmpty(c.tricks) ? "—" : c.tricks.Length > 38 ? c.tricks.Substring(0, 37) + "…" : c.tricks;
                TableRow(row++, "combo-top-" + i, new[] { "#" + (i + 1), c.points.ToString("N0"), c.links > 0 ? "×" + c.links : "—", tricks, VehicleProfile.Find(c.vehicle).Name, string.IsNullOrEmpty(c.name) ? PlayerNames.Player : c.name, c.date ?? "—" }, w, () => { });
            }
            if (top.Count == 0) TableRow(row++, "combo-top-none", new[] { "", "", "", "No combos yet: chain tricks at the stunt park", "", "", "" }, w, () => { });
            Table(first, row); buttons[backRow].transform.SetAsLastSibling();
        }
    }
}
