using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Racer
{
    // 0.95 Part B: Police Chase has its own setup screen (main menu > POLICE CHASE, after SPLIT SCREEN; the Free Roam page and
    // the Free Roam pause menu; the Split Screen setup's Mode row sends Police Chase here). Players: 1 (full screen against the
    // AI, one role kept, one round; Part A) or 2 (split-screen, the roles swap). The runner's vehicle is chosen in the garage
    // view; the patrol car turns on this screen with its stat bars (it stays out of the normal garage). Started from inside
    // Free Roam it asks once and the menu afterwards goes back to Free Roam.
    public sealed partial class RaceMenus
    {
        public void OpenPoliceSetup(bool keepPlayers = false)
        {
            if (!keepPlayers && flow.InRoamWorld && !SplitScreen.Active)
            {
                Confirm("POLICE CHASE", "Leave Free Roam and start a Police Chase?\nYou come back to Free Roam afterwards.", () => { SplitScreen.FromRoam = true; SplitScreen.RoamReturn = RaceFlow.RoamCourse; PolicePage(false); }, "POLICE CHASE");
                return;
            }
            if (!keepPlayers) SplitScreen.FromRoam = false;
            PolicePage(keepPlayers);
        }
        void PolicePage(bool keepPlayers)
        {
            var keyboard = (InputDevice)Keyboard.current; var opener = MenuInput.Controller && MenuInput.Pad != null ? MenuInput.Pad : keyboard;
            if (!keepPlayers || SplitScreen.P1Device == null || !SplitScreen.P1Device.added) SplitScreen.P1Device = opener;
            if (SplitScreen.P2Device != null && (!SplitScreen.P2Device.added || SplitScreen.P2Device == SplitScreen.P1Device)) SplitScreen.P2Device = null;
            SplitScreen.Mode = SplitScreen.Kind.Police; SplitScreen.P2Ai = SplitScreen.Solo;
            if (!SplitScreen.Vehicles.Any(v => v.Id == SplitScreen.P1Vehicle)) SplitScreen.P1Vehicle = "original";
            if (!SplitScreen.Vehicles.Any(v => v.Id == SplitScreen.P2Vehicle)) SplitScreen.P2Vehicle = "atv";
            if (page != "police") Navigate("police"); else Show();
        }
        // The Split Screen setup's Mode row lands on Police Chase: two players, this screen.
        void SplitToPolice() { SplitScreen.Solo = false; PolicePage(true); }
        void RenderPolice()
        {
            if (SplitScreen.PoliceGame == SplitScreen.Game.SpeedPatrol) { RenderPatrolSetup(); return; } // 0.95 Part G
            bool solo = SplitScreen.Solo; SplitScreen.Mode = SplitScreen.Kind.Police; SplitScreen.P2Ai = solo;
            if (solo) { SplitScreen.SoloRole = 1; SplitScreen.CopFirst = 1; } // the runner needs the chasing AI (coming later)
            ClearCore("POLICE CHASE", (solo
                ? "One player against the AI on the full screen. You keep your role for the one round."
                : "Two players, split-screen: one is the cop in the patrol car, the other runs; then the roles swap.")
                + "\nNothing is recorded." + (SplitScreen.FromRoam ? "  Afterwards you go back to Free Roam." : ""));
            details.fontSize = 18; details.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 54;
            int n = 0; GameRow(n++);
            Step(n++, "police-players", "Players:   " + (solo ? "1   (full screen, against the AI)" : "2   (split screen)"), d => { SplitScreen.Solo = !SplitScreen.Solo; SplitScreen.P2Ai = SplitScreen.Solo; flow.Click(); Show(); });
            if (solo)
            {
                // Part A: one role, kept; the runner role is greyed until the chasing AI exists
                Step(n++, "police-role", "Your role:   Cop   ·   Runner: coming later (the AI as the cop)", d => { flow.Notify("Runner against an AI cop is coming later: for now you are the cop", 3); flow.Click(); });
            }
            else
            {
                string Joins(InputDevice dev) => dev is Gamepad ? "   (A on another controller joins)" : "";
                Step(n++, "split-p1", "Player 1:   " + (SplitScreen.P1Device == null ? "press A on a controller, or Enter" : SplitScreen.DeviceName(SplitScreen.P1Device) + "   ✓"), d => CycleSlot(1, d));
                Step(n++, "split-p2", "Player 2:   " + (SplitScreen.P2Device != null ? SplitScreen.DeviceName(SplitScreen.P2Device) + "   ✓" : "press A on another controller, or Enter" + Joins(SplitScreen.P1Device)), d => CycleSlot(2, d));
                if (SplitScreen.P2Device != null) Row(n++, "split-swap", "Swap the players' devices", () => { (SplitScreen.P1Device, SplitScreen.P2Device) = (SplitScreen.P2Device, SplitScreen.P1Device); flow.Click(); Show(); });
                if (string.IsNullOrEmpty(SplitScreen.P1Name)) SplitScreen.P1Name = PlayerNames.Player;
                Step(n++, "split-p1-name", "Player 1 name:   " + SplitScreen.NameOf(1) + "   (A: new name)", d => { SplitScreen.P1Name = NextKnownName(SplitScreen.P1Name, d, SplitScreen.P2Pick); flow.Click(); Show(); },
                    () => OpenNameEntry("", v => { PlayerNames.Remember(v); flow.Save.SaveSettings(); SplitScreen.P1Name = v; return true; }, null, "PLAYER 1'S NAME"));
                if (string.IsNullOrEmpty(SplitScreen.P2Pick) || SplitScreen.P2Pick == SplitScreen.NameOf(1)) SplitScreen.P2Pick = PlayerNames.Known.FirstOrDefault(k => k != SplitScreen.NameOf(1)) ?? "";
                Step(n++, "split-p2-name", "Player 2 name:   " + (string.IsNullOrEmpty(SplitScreen.P2Pick) ? "press A to enter it" : SplitScreen.P2Pick + "   (A: new name)"), d => { SplitScreen.P2Pick = NextKnownName(SplitScreen.P2Pick, d, SplitScreen.NameOf(1)); flow.Click(); Show(); },
                    () => OpenNameEntry("", v => { if (v == SplitScreen.NameOf(1)) { keyboardError = "Player 1 already has that name."; return false; } PlayerNames.Remember(v); flow.Save.SaveSettings(); SplitScreen.P2Pick = v; return true; }, null, "PLAYER 2'S NAME"));
                Step(n++, "police-cop", "Cop first:   " + SplitScreen.NameOf(SplitScreen.CopFirst) + "   (then the roles swap)", d => { SplitScreen.CopFirst = 3 - SplitScreen.CopFirst; flow.Click(); Show(); });
            }
            // the runner's vehicle in the garage view; the patrol car shown turning with its stat bars
            Row(n, "police-vehicles", solo ? "RUNNER'S VEHICLE…   (the AI runs in it)" : "RUNNERS' VEHICLES…   (each player's garage)", () => OpenSplitPick(solo)); int vehiclesRow = n++;
            var strip = PreviewStrip("Police Chase vehicles", buttons[vehiclesRow].transform.GetSiblingIndex() + 1, 170);
            PreviewCard(strip, VehicleProfile.Police, -1, false, "The cop: Patrol Car", new Color(.5f, .7f, 1), 250);
            var stats = Rect("Patrol car stats", strip); stats.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().preferredWidth = 230;
            var v = stats.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>(); v.padding = new RectOffset(4, 4, 14, 4); v.spacing = 4; v.childControlWidth = v.childControlHeight = true; v.childForceExpandHeight = false;
            StatBars(stats, VehicleProfile.Police);
            string Vehicle(string id) { var p = VehicleProfile.Find(id); return $"{p.Name} ({p.Class})"; }
            PreviewCard(strip, VehicleProfile.Find(SplitScreen.P2Vehicle), SplitScreen.P2Color, false, (solo ? "The runner (AI): " : SplitScreen.NameOf(2) + " runs in: ") + Vehicle(SplitScreen.P2Vehicle), new Color(1, .74f, .25f), solo ? 250 : 200);
            if (!solo) PreviewCard(strip, VehicleProfile.Find(SplitScreen.P1Vehicle), SplitScreen.P1Color, false, SplitScreen.NameOf(1) + " runs in: " + Vehicle(SplitScreen.P1Vehicle), new Color(.3f, .95f, .81f), 200);
            var order = RacePlaylists.DisplayOrder.ToArray();
            Step(n++, "split-course", "Start at:   " + RacePlaylists.Titles[SplitScreen.Course].Replace(" - ", " — "), d => { int i = System.Array.IndexOf(order, SplitScreen.Course); SplitScreen.Course = order[((i < 0 ? 0 : i) + d + order.Length) % order.Length]; flow.Click(); Show(); });
            var times = LookPresets.MenuOrder;
            Step(n++, "split-time", "Time of day:   " + SplitScreen.Time, d => { int i = System.Array.IndexOf(times, SplitScreen.Time); SplitScreen.Time = times[((i < 0 ? 1 : i) + d + times.Length) % times.Length]; flow.Click(); Show(); });
            Step(n++, "split-weather", "Weather:   " + SplitScreen.Weather, d => { SplitScreen.Weather = (Weather)(((int)SplitScreen.Weather + d + 3) % 3); flow.Click(); Show(); });
            Toggle(n++, "split-traffic", "Traffic", SplitScreen.Traffic, () => { SplitScreen.Traffic = !SplitScreen.Traffic; flow.Click(); Show(); });
            int[] limits = { 3, 5, 8 };
            Step(n++, "police-limit", "Round limit:   " + SplitScreen.PoliceMinutes + " minutes", d => { int i = System.Array.IndexOf(limits, SplitScreen.PoliceMinutes); SplitScreen.PoliceMinutes = limits[Mathf.Clamp((i < 0 ? 1 : i) + d, 0, 2)]; flow.Click(); Show(); });
            if (!solo) Step(n++, "split-layout", "Screen:   " + (SplitScreen.LeftRight ? "Left / right" : "Top / bottom (player 1 on top)"), d => { flow.Save.Settings.splitLeftRight = !flow.Save.Settings.splitLeftRight; flow.Save.SaveSettings(); flow.Click(); Show(); });
            Row(n, "police-start", SplitScreen.Ready ? "START CHASE" : "START CHASE   (waiting for player 2)", flow.StartSplit);
            var colors = buttons[n].colors; colors.normalColor = new(.1f, .38f, .35f); buttons[n].colors = colors; buttons[n].GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 54; buttons[n++].interactable = SplitScreen.Ready;
            Row(n, "police-back", "Back", () => BackPage());
            if (solo) buttons[2].GetComponentInChildren<UnityEngine.UI.Text>(true).color = new Color(.78f, .8f, .78f);
            else { DeviceGlyph(buttons[2], SplitScreen.P1Device); DeviceGlyph(buttons[3], SplitScreen.P2Device); buttons[2].GetComponentInChildren<UnityEngine.UI.Text>(true).color = new Color(.3f, .95f, .81f); buttons[3].GetComponentInChildren<UnityEngine.UI.Text>(true).color = SplitScreen.P2Device != null ? new Color(1, .74f, .25f) : new Color(.75f, .75f, .72f); }
        }
    }
}
