using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace Racer
{
    // 0.90 Part D: the split-screen setup screen (main menu > SPLIT SCREEN), its pause menu and its results. Player 1 is the
    // device that opened the screen; player 2 joins by pressing A / Start on another controller or Enter on the keyboard (or
    // is the race AI with "Player 2: AI driver"). 0.91: each player's row changes their device; the setup screen answers
    // player 1's device; the pause menu and the results answer either player's device.
    public sealed partial class RaceMenus
    {
        // 0.91 Part B: whoever opens the screen from the main menu is player 1 on the device they used (a controller, or the
        // keyboard / mouse); "Change setup" from the results keeps the players as they were. Player 2 with no device is the
        // race AI, so one player can start at once.
        public void OpenSplitSetup(bool keepPlayers = false)
        {
            var keyboard = (InputDevice)Keyboard.current; var opener = MenuInput.Controller && MenuInput.Pad != null ? MenuInput.Pad : keyboard;
            if (!keepPlayers || SplitScreen.P1Device == null || !SplitScreen.P1Device.added) SplitScreen.P1Device = opener;
            if (SplitScreen.P2Device != null && (!SplitScreen.P2Device.added || SplitScreen.P2Device == SplitScreen.P1Device)) SplitScreen.P2Device = null;
            if (SplitScreen.P2Device == null) SplitScreen.P2Ai = true;
            if (!SplitScreen.Vehicles.Any(v => v.Id == SplitScreen.P1Vehicle)) SplitScreen.P1Vehicle = "original";
            if (!SplitScreen.Vehicles.Any(v => v.Id == SplitScreen.P2Vehicle)) SplitScreen.P2Vehicle = "atv";
            if (page != "split") Navigate("split"); else Show();
        }
        // The devices a player slot can take: the keyboard and every connected controller, never the other slot's.
        static InputDevice[] FreeDevices(InputDevice other) => new InputDevice[] { Keyboard.current }.Concat(Gamepad.all).Where(d => d != null && d.added && d != other).ToArray();
        static bool Assigned(InputDevice d) => d == SplitScreen.P1Device || (d == SplitScreen.P2Device && !SplitScreen.P2Ai);
        // A / Start on a controller, or Enter on the keyboard, that no player holds joins it to the first free slot (player 2
        // when it is the AI). Player 2's own device works only player 2's choices: left / right its vehicle, up / down its
        // colour (D-pad or arrow keys). Everything else on the screen answers player 1's device only.
        void UpdateSplitJoin()
        {
            if (flow.State != RaceFlow.Stage.Ready || page != "split" || modalConfirm != null) return;
            if (SplitScreen.P1Device != null && !SplitScreen.P1Device.added) { SplitScreen.P1Device = null; Show(); return; }
            if (SplitScreen.P2Device != null && !SplitScreen.P2Device.added) { SplitScreen.P2Device = null; SplitScreen.P2Ai = true; Show(); return; }
            foreach (var pad in Gamepad.all) if (!Assigned(pad) && (pad.buttonSouth.wasPressedThisFrame || pad.startButton.wasPressedThisFrame)) { Join(pad); return; }
            var k = Keyboard.current; if (k != null && !Assigned(k) && (k.enterKey.wasPressedThisFrame || k.numpadEnterKey.wasPressedThisFrame)) { Join(k); return; }
            // 0.92 Part B: the vehicles and colours are chosen in the players' garages (Vehicles and colours…)
        }
        void Join(InputDevice d)
        {
            if (SplitScreen.P1Device == null) SplitScreen.P1Device = d; else { SplitScreen.P2Device = d; SplitScreen.P2Ai = false; }
            MenuInput.ConsumeThroughRelease(); flow.Click(); flow.Notify((SplitScreen.P1Device == d ? "Player 1" : "Player 2") + " joined: " + SplitScreen.DeviceName(d), 3); Show();
        }
        static string NextSplitVehicle(string id, int d) { var vehicles = SplitScreen.Vehicles; int i = System.Array.FindIndex(vehicles, v => v.Id == id); return vehicles[((i < 0 ? 0 : i) + d + vehicles.Length) % vehicles.Length].Id; }
        // A slot's device row: the next device it can take (player 2 also cycles through the AI driver).
        void CycleSlot(int player, int d)
        {
            var other = player == 1 ? (SplitScreen.P2Ai ? null : SplitScreen.P2Device) : SplitScreen.P1Device;
            var choices = FreeDevices(other).ToList(); if (player == 2) choices.Insert(0, null); // null = the AI driver
            var current = player == 1 ? SplitScreen.P1Device : SplitScreen.P2Ai ? null : SplitScreen.P2Device;
            if (choices.Count < 2) { flow.Notify("No other device: connect another controller", 3); flow.Click(); Show(); return; }
            var next = choices[(Mathf.Max(0, choices.IndexOf(current)) + d + choices.Count) % choices.Count];
            if (player == 1) SplitScreen.P1Device = next; else { SplitScreen.P2Device = next; SplitScreen.P2Ai = next == null; }
            flow.Click(); Show();
        }
        // While the setup screen is up only player 1's device (and the mouse with the keyboard) works it.
        bool splitSetupDevices;
        public bool SplitSetupOpen => flow.State == RaceFlow.Stage.Ready && (page == "split" || page == "split-garage") && modalConfirm == null;
        void SplitSetupDevices()
        {
            bool setup = flow.State == RaceFlow.Stage.Ready && (page == "split" || page == "split-garage");
            if (setup) { var p1 = SplitScreen.P1Device; RestrictMenuDevices(p1 != null && p1.added ? p1 : null); splitSetupDevices = true; }
            else if (splitSetupDevices) { splitSetupDevices = false; if (!SplitScreen.Race) RestrictMenuDevices(); }
        }
        void RenderSplit()
        {
            ClearCore("SPLIT SCREEN", "Two players, one race on this PC: everything is unlocked and every vehicle is stock.\nLeft / right on a player's row changes their device; A or Start on another controller, or Enter, joins it.");
            if (SplitScreen.P1Color > VehiclePaint.Count - 1) SplitScreen.P1Color = 2; if (SplitScreen.P2Color > VehiclePaint.Count - 1) SplitScreen.P2Color = 2;
            details.fontSize = 18; details.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 54;
            var vehicles = SplitScreen.Vehicles; int n = 0;
            string Vehicle(string id) { var p = VehicleProfile.Find(id); return $"{p.Name} ({p.Class})"; }
            string Joins(InputDevice d) => d is Gamepad ? "   (A on another controller joins it)" : "";
            Step(n++, "split-p1", "Player 1:   " + (SplitScreen.P1Device == null ? "press A on a controller, or Enter" : SplitScreen.DeviceName(SplitScreen.P1Device) + "   ✓"), d => CycleSlot(1, d));
            Step(n++, "split-p2", "Player 2:   " + (SplitScreen.P2Ai ? "the race AI drives" + Joins(SplitScreen.P1Device) : SplitScreen.P2Device != null ? SplitScreen.DeviceName(SplitScreen.P2Device) + "   ✓" : "press A on another controller, or Enter on the keyboard"), d => CycleSlot(2, d));
            Toggle(n++, "split-ai", "Player 2: AI driver", SplitScreen.P2Ai, () => { SplitScreen.P2Ai = !SplitScreen.P2Ai; flow.Click(); Show(); });
            if (SplitScreen.P2Device != null && !SplitScreen.P2Ai) Row(n++, "split-swap", "Swap the players' devices", () => { (SplitScreen.P1Device, SplitScreen.P2Device) = (SplitScreen.P2Device, SplitScreen.P1Device); flow.Click(); Show(); });
            // 0.92 Part B: the vehicles and colours are chosen in each player's garage view; both are shown here
            Row(n, "split-vehicles", "VEHICLES AND COLOURS…   (each player's garage)", OpenSplitPick); int vehiclesRow = n++;
            var strip = PreviewStrip("Players' vehicles", buttons[vehiclesRow].transform.GetSiblingIndex() + 1);
            PreviewCard(strip, VehicleProfile.Find(SplitScreen.P1Vehicle), SplitScreen.P1Color, false, "Player 1: " + Vehicle(SplitScreen.P1Vehicle) + " · " + VehiclePaint.Name(SplitScreen.P1Color), new Color(.3f, .95f, .81f), 300);
            PreviewCard(strip, VehicleProfile.Find(SplitScreen.P2Vehicle), SplitScreen.P2Color, false, (SplitScreen.P2Ai ? "Player 2 (AI): " : "Player 2: ") + Vehicle(SplitScreen.P2Vehicle) + " · " + VehiclePaint.Name(SplitScreen.P2Color), new Color(1, .74f, .25f), 300);
            var order = RacePlaylists.DisplayOrder.ToArray();
            Step(n++, "split-course", "Course:   " + RacePlaylists.Titles[SplitScreen.Course].Replace(" - ", " — "), d => { int i = System.Array.IndexOf(order, SplitScreen.Course); SplitScreen.Course = order[((i < 0 ? 0 : i) + d + order.Length) % order.Length]; flow.Click(); Show(); });
            Step(n++, "split-laps", "Laps:   " + SplitScreen.Laps, d => { SplitScreen.Laps = Mathf.Clamp(SplitScreen.Laps + d, 1, 5); flow.Click(); Show(); });
            Step(n++, "split-layout", "Screen:   " + (SplitScreen.LeftRight ? "Left / right" : "Top / bottom (player 1 on top)"), d => { flow.Save.Settings.splitLeftRight = !flow.Save.Settings.splitLeftRight; flow.Save.SaveSettings(); flow.Click(); Show(); });
            Row(n, "split-start", SplitScreen.Ready ? "START" : "START   (waiting for player 2)", flow.StartSplit);
            var colors = buttons[n].colors; colors.normalColor = new(.1f, .38f, .35f); buttons[n].colors = colors; buttons[n].GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 54; buttons[n++].interactable = SplitScreen.Ready;
            Row(n, "split-back", "Back", () => BackPage());
            DeviceGlyph(buttons[0], SplitScreen.P1Device); DeviceGlyph(buttons[1], SplitScreen.P2Ai ? null : SplitScreen.P2Device);
            buttons[0].GetComponentInChildren<UnityEngine.UI.Text>(true).color = new Color(.3f, .95f, .81f);
            buttons[1].GetComponentInChildren<UnityEngine.UI.Text>(true).color = SplitScreen.P2Ai || SplitScreen.P2Device != null ? new Color(1, .74f, .25f) : new Color(.75f, .75f, .72f);
        }
        // 0.91 Part B: a player row's device as a glyph at its left: a controller's stick with its number, or the keyboard.
        void DeviceGlyph(UnityEngine.UI.Button row, InputDevice device)
        {
            if (device == null) return;
            var label = row.GetComponentInChildren<UnityEngine.UI.Text>(true); label.rectTransform.offsetMin = new(86, 0);
            var icon = Rect("Device glyph", row.transform); icon.anchorMin = icon.anchorMax = new(0, .5f); icon.pivot = new(0, .5f); icon.anchoredPosition = new(12, 0); icon.sizeDelta = new(62, 34); tableCells.Add(icon.gameObject);
            var glyph = icon.gameObject.AddComponent<MenuGlyph>(); glyph.raycastTarget = false; glyph.SetPath(device is Gamepad ? "<Gamepad>/leftStick" : "<Keyboard>/k");
            var text = Label("Device", icon, 16, 0); Stretch(text.rectTransform, 0, 0, 0, 0); text.alignment = TextAnchor.MiddleCenter;
            text.text = device is Gamepad g ? (Gamepad.all.ToList().IndexOf(g) + 1).ToString() : "KB";
        }
        void ResetSplitRowColours() { foreach (var b in buttons) b.GetComponentInChildren<UnityEngine.UI.Text>(true).color = Color.white; }
        // Split-screen pause: resume (not while a controller is missing), restart, settings, end the race.
        void RenderSplitPause()
        {
            var split = SplitScreen.Race; string missing = split ? split.MissingText : null;
            ClearCore("SPLIT SCREEN / PAUSED", missing ?? $"Paused by {(split && split.PausedBy != null ? SplitScreen.DeviceName(split.PausedBy) : "a player")}: either player's device works this menu.");
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
        // 0.91 Part B: the menus answer only these devices (the mouse too with the keyboard); none = every device.
        public void RestrictMenuDevices(params InputDevice[] devices)
        {
            ReadOnlyArray<InputDevice>? only = null;
            var list = (devices ?? new InputDevice[0]).Where(d => d != null && d.added).Distinct().ToList();
            if (list.Count > 0) { if (list.Any(d => d is Keyboard) && Mouse.current != null) list.Add(Mouse.current); only = new ReadOnlyArray<InputDevice>(list.ToArray()); }
            menuDevices = only?.ToArray();
            if (ownedUiActions) ownedUiActions.devices = only;
            if (menuActions) menuActions.devices = only;
        }
    }
}
