using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace Racer
{
    // 0.90 Part D: the split-screen setup screen (main menu > SPLIT SCREEN), its pause menu and its results. Player 1 is the
    // device that opened the screen; player 2 joins by pressing Start on another controller or Enter on the keyboard (or
    // is the race AI with "Player 2: AI driver"). Either player can move through these menus; once paused, the pause menu
    // answers only the device that paused.
    public sealed partial class RaceMenus
    {
        public void OpenSplitSetup()
        {
            var keyboard = (InputDevice)Keyboard.current; var opener = MenuInput.Controller && MenuInput.Pad != null ? MenuInput.Pad : keyboard;
            if (SplitScreen.P1Device == null || !SplitScreen.P1Device.added) SplitScreen.P1Device = opener;
            if (SplitScreen.P2Device != null && (!SplitScreen.P2Device.added || SplitScreen.P2Device == SplitScreen.P1Device)) SplitScreen.P2Device = null;
            if (!SplitScreen.Vehicles.Any(v => v.Id == SplitScreen.P1Vehicle)) SplitScreen.P1Vehicle = "original";
            if (!SplitScreen.Vehicles.Any(v => v.Id == SplitScreen.P2Vehicle)) SplitScreen.P2Vehicle = "atv";
            if (page != "split") Navigate("split"); else Show();
        }
        // Player 2 joins with Start (a controller that is not player 1's) or Enter (the keyboard, when player 1 is not on it).
        void UpdateSplitJoin()
        {
            if (flow.State != RaceFlow.Stage.Ready || page != "split" || modalConfirm != null) return;
            foreach (var pad in Gamepad.all) if (pad != SplitScreen.P1Device && pad != SplitScreen.P2Device && pad.startButton.wasPressedThisFrame) { Join(pad); return; }
            var k = Keyboard.current; if (k != null && SplitScreen.P1Device != k && SplitScreen.P2Device != k && (k.enterKey.wasPressedThisFrame || k.numpadEnterKey.wasPressedThisFrame)) Join(k);
            if (SplitScreen.P2Device != null && !SplitScreen.P2Device.added) { SplitScreen.P2Device = null; Show(); }
        }
        void Join(InputDevice d) { SplitScreen.P2Device = d; SplitScreen.P2Ai = false; MenuInput.ConsumeThroughRelease(); flow.Click(); flow.Notify("Player 2 joined: " + SplitScreen.DeviceName(d), 3); Show(); }
        void RenderSplit()
        {
            ClearCore("SPLIT SCREEN", "Two players, one race on this PC: everything is unlocked and every vehicle is stock.\nPlayer 2 joins by pressing Start on another controller, or Enter on the keyboard.");
            details.fontSize = 18; details.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 54;
            var vehicles = SplitScreen.Vehicles; int n = 0;
            string Vehicle(string id) { var p = VehicleProfile.Find(id); return $"{p.Name} ({p.Class})"; }
            string Next(string id, int d) { int i = System.Array.FindIndex(vehicles, v => v.Id == id); return vehicles[((i < 0 ? 0 : i) + d + vehicles.Length) % vehicles.Length].Id; }
            int Colour(int c, int d) => (c + d + VehiclePaint.Colors.Length) % VehiclePaint.Colors.Length;
            Row(n++, "split-p1", "Player 1:   " + SplitScreen.DeviceName(SplitScreen.P1Device) + "   ✓ joined", () => { });
            Row(n++, "split-p2", SplitScreen.P2Ai ? "Player 2:   the race AI drives" : SplitScreen.P2Device != null ? "Player 2:   " + SplitScreen.DeviceName(SplitScreen.P2Device) + "   ✓ joined" : "Player 2:   press Start on another controller, or Enter on the keyboard", () => { });
            Row(n++, "split-ai", "Player 2: AI driver:   " + (SplitScreen.P2Ai ? "On" : "Off") + "   (play alone and test)", () => { SplitScreen.P2Ai = !SplitScreen.P2Ai; flow.Click(); Show(); });
            if (SplitScreen.P2Device != null && !SplitScreen.P2Ai) Row(n++, "split-swap", "Swap the players' devices", () => { (SplitScreen.P1Device, SplitScreen.P2Device) = (SplitScreen.P2Device, SplitScreen.P1Device); flow.Click(); Show(); });
            Step(n++, "split-v1", "Player 1 vehicle:   " + Vehicle(SplitScreen.P1Vehicle), d => { SplitScreen.P1Vehicle = Next(SplitScreen.P1Vehicle, d); flow.Click(); Show(); });
            Step(n++, "split-c1", "Player 1 colour:   " + VehiclePaint.Names[SplitScreen.P1Color], d => { SplitScreen.P1Color = Colour(SplitScreen.P1Color, d); flow.Click(); Show(); });
            Step(n++, "split-v2", "Player 2 vehicle:   " + Vehicle(SplitScreen.P2Vehicle), d => { SplitScreen.P2Vehicle = Next(SplitScreen.P2Vehicle, d); flow.Click(); Show(); });
            Step(n++, "split-c2", "Player 2 colour:   " + VehiclePaint.Names[SplitScreen.P2Color], d => { SplitScreen.P2Color = Colour(SplitScreen.P2Color, d); flow.Click(); Show(); });
            var order = RacePlaylists.DisplayOrder.ToArray();
            Step(n++, "split-course", "Course:   " + RacePlaylists.Titles[SplitScreen.Course].Replace(" - ", " — "), d => { int i = System.Array.IndexOf(order, SplitScreen.Course); SplitScreen.Course = order[((i < 0 ? 0 : i) + d + order.Length) % order.Length]; flow.Click(); Show(); });
            Step(n++, "split-laps", "Laps:   " + SplitScreen.Laps, d => { SplitScreen.Laps = Mathf.Clamp(SplitScreen.Laps + d, 1, 5); flow.Click(); Show(); });
            Step(n++, "split-layout", "Screen:   " + (SplitScreen.LeftRight ? "Left / right" : "Top / bottom (player 1 on top)"), d => { flow.Save.Settings.splitLeftRight = !flow.Save.Settings.splitLeftRight; flow.Save.SaveSettings(); flow.Click(); Show(); });
            Row(n, "split-start", SplitScreen.Ready ? "START" : "START   (waiting for player 2)", flow.StartSplit);
            var colors = buttons[n].colors; colors.normalColor = new(.1f, .38f, .35f); buttons[n].colors = colors; buttons[n].GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 54; buttons[n++].interactable = SplitScreen.Ready;
            Row(n, "split-back", "Back", () => BackPage());
            buttons[0].GetComponentInChildren<UnityEngine.UI.Text>(true).color = new Color(.3f, .95f, .81f);
            buttons[1].GetComponentInChildren<UnityEngine.UI.Text>(true).color = SplitScreen.P2Ai || SplitScreen.P2Device != null ? new Color(1, .74f, .25f) : new Color(.75f, .75f, .72f);
        }
        void ResetSplitRowColours() { foreach (var b in buttons) b.GetComponentInChildren<UnityEngine.UI.Text>(true).color = Color.white; }
        // Split-screen pause: resume (not while a controller is missing), restart, settings, end the race.
        void RenderSplitPause()
        {
            var split = SplitScreen.Race; string missing = split ? split.MissingText : null;
            ClearCore("SPLIT SCREEN / PAUSED", missing ?? $"Paused by {(split && split.PausedBy != null ? SplitScreen.DeviceName(split.PausedBy) : "a player")}: only that device works this menu.");
            Row(0, "resume", "RESUME", flow.Resume); buttons[0].interactable = missing == null;
            Row(1, "restart", "RESTART RACE", () => Confirm("RESTART RACE?", "Both players go back to the grid.", flow.StartRace));
            Row(2, "settings", "SETTINGS", flow.OpenSettings);
            Row(3, "return", "END RACE / RETURN TO MENU", () => Confirm("END THE SPLIT-SCREEN RACE?", "Both players return to the menu.", flow.QuitRace));
            Row(4, "quit", "Quit Game", ConfirmQuit);
        }
        // Both players: place, total time and best lap; Rematch / Change setup / Main menu.
        void RenderSplitResults()
        {
            var race = flow.Race; var split = SplitScreen.Race;
            ClearCore("SPLIT SCREEN RESULTS", race.courseName + "  ·  " + race.laps + " lap" + (race.laps == 1 ? "" : "s"));
            int n = 0; var w = new[] { .1f, .4f, .26f, .24f };
            TableRow(n++, "header", new[] { "Place", "Player", "Total time", "Best lap" }, w, () => { }); int rank = 0;
            foreach (var r in race.Ordered(true))
            {
                int place = ++rank; bool p1 = r == race.Racers[0]; var laps = r.Progress.LapTimes;
                string who = (p1 ? "Player 1" : SplitScreen.P2Ai ? "Player 2 (AI)" : "Player 2") + " · " + VehicleProfile.Find(r.Car.GetComponent<VehicleConfiguration>().profileId).Name;
                TableRow(n++, "split-standing-" + place, new[] { place.ToString(), who, r.Dnf || !r.Classified ? "DNF" : RaceHud.FormatTime(r.ClassifiedTime(race.Clock)), laps.Count > 0 ? RaceHud.FormatTime(laps.Min()) : "—" }, w, () => { }, place == 1);
            }
            Row(n, "rematch", "REMATCH", flow.StartRace); var colors = buttons[n].colors; colors.normalColor = new(.1f, .38f, .35f); buttons[n].colors = colors; buttons[n].GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 54; n++;
            Row(n++, "change-setup", "Change setup", () => flow.QuitSplit(true));
            Row(n, "menu", "Main menu", () => flow.QuitSplit(false));
        }
        // Once paused, only the device that paused works the menu (the mouse too when it is the keyboard).
        public void RestrictMenuDevices(InputDevice device)
        {
            ReadOnlyArray<InputDevice>? only = null;
            if (device != null) { var list = new System.Collections.Generic.List<InputDevice> { device }; if (device is Keyboard && Mouse.current != null) list.Add(Mouse.current); only = new ReadOnlyArray<InputDevice>(list.ToArray()); }
            if (ownedUiActions) ownedUiActions.devices = only;
            if (menuActions) menuActions.devices = only;
        }
    }
}
