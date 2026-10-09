using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Racer
{
    // 0.95 Part G: the Police Chase setup's Speed Patrol game (Game: Cop vs Runner / Speed Patrol): players 1 / 2, where it
    // starts, time of day, weather, round length 3 / 5 / 8 minutes; traffic is always on. Its results: each cop's points,
    // catches, fastest speeder caught, penalties, and the Speed Patrol Top 10 for that round length.
    public sealed partial class RaceMenus
    {
        void GameRow(int n) => Step(n, "police-game", "Game:   " + (SplitScreen.PoliceGame == SplitScreen.Game.SpeedPatrol ? "Speed Patrol   (catch speeders for points)" : SplitScreen.PoliceGame == SplitScreen.Game.Getaway ? "Getaway   (you run; the cops are AI)" : "Cop vs Runner"), d => { SplitScreen.PoliceGame = (SplitScreen.Game)(((int)SplitScreen.PoliceGame + d + 3) % 3); flow.Click(); Show(); });
        void RenderPatrolSetup()
        {
            bool solo = SplitScreen.Solo; SplitScreen.Mode = SplitScreen.Kind.Police; SplitScreen.P2Ai = solo;
            ClearCore("SPEED PATROL", (solo ? "One cop on the full screen." : "Two cops, split-screen: the most points wins.")
                + " Point the radar at a car to clock it; lights on (RB / H) near a clocked speeder to chase it; stay close to fill the pull-over meter."
                + " Catch: 100 + 10 per mph over. Hitting other traffic −50; stopping a car that was not speeding −25. Traffic is always on." + (SplitScreen.FromRoam ? "  Afterwards you go back to Free Roam." : ""));
            details.fontSize = 17; details.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 78;
            int n = 0; GameRow(n++);
            Step(n++, "police-players", "Players:   " + (solo ? "1   (full screen)" : "2   (split screen)"), d => { SplitScreen.Solo = !SplitScreen.Solo; SplitScreen.P2Ai = SplitScreen.Solo; flow.Click(); Show(); });
            int devices = -1;
            if (!solo)
            {
                devices = n;
                Step(n++, "split-p1", "Player 1:   " + (SplitScreen.P1Device == null ? "press A on a controller, or Enter" : SplitScreen.DeviceName(SplitScreen.P1Device) + "   ✓"), d => CycleSlot(1, d));
                Step(n++, "split-p2", "Player 2:   " + (SplitScreen.P2Device != null ? SplitScreen.DeviceName(SplitScreen.P2Device) + "   ✓" : "press A on another controller, or Enter"), d => CycleSlot(2, d));
                if (string.IsNullOrEmpty(SplitScreen.P1Name)) SplitScreen.P1Name = PlayerNames.Player;
                Step(n++, "split-p1-name", "Player 1 name:   " + SplitScreen.NameOf(1) + "   (A: new name)", d => { SplitScreen.P1Name = NextKnownName(SplitScreen.P1Name, d, SplitScreen.P2Pick); flow.Click(); Show(); },
                    () => OpenNameEntry("", v => { PlayerNames.Remember(v); flow.Save.SaveSettings(); SplitScreen.P1Name = v; return true; }, null, "PLAYER 1'S NAME"));
                if (string.IsNullOrEmpty(SplitScreen.P2Pick) || SplitScreen.P2Pick == SplitScreen.NameOf(1)) SplitScreen.P2Pick = PlayerNames.Known.FirstOrDefault(k => k != SplitScreen.NameOf(1)) ?? "";
                Step(n++, "split-p2-name", "Player 2 name:   " + (string.IsNullOrEmpty(SplitScreen.P2Pick) ? "press A to enter it" : SplitScreen.P2Pick + "   (A: new name)"), d => { SplitScreen.P2Pick = NextKnownName(SplitScreen.P2Pick, d, SplitScreen.NameOf(1)); flow.Click(); Show(); },
                    () => OpenNameEntry("", v => { if (v == SplitScreen.NameOf(1)) { keyboardError = "Player 1 already has that name."; return false; } PlayerNames.Remember(v); flow.Save.SaveSettings(); SplitScreen.P2Pick = v; return true; }, null, "PLAYER 2'S NAME"));
            }
            var strip = PreviewStrip("Speed Patrol cars", buttons[n - 1].transform.GetSiblingIndex() + 1, 170);
            PreviewCard(strip, VehicleProfile.Police, -1, false, solo ? "Your patrol car" : SplitScreen.NameOf(1) + ": Patrol Car (black)", new Color(.5f, .7f, 1), 250);
            var stats = Rect("Patrol car stats", strip); stats.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().preferredWidth = 230;
            var v = stats.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>(); v.padding = new RectOffset(4, 4, 14, 4); v.spacing = 4; v.childControlWidth = v.childControlHeight = true; v.childForceExpandHeight = false;
            StatBars(stats, VehicleProfile.Police);
            if (!solo) { var note = Label("Second livery", strip, 17, 0); note.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().preferredWidth = 220; note.alignment = TextAnchor.MiddleCenter; note.color = new Color(1, .74f, .25f); note.text = SplitScreen.NameOf(2) + "'s patrol car is dark blue"; tableCells.Add(note.gameObject); }
            var times = LookPresets.MenuOrder;
            Step(n++, "split-time", "Time of day:   " + SplitScreen.Time, d => { int i = System.Array.IndexOf(times, SplitScreen.Time); SplitScreen.Time = times[((i < 0 ? 1 : i) + d + times.Length) % times.Length]; flow.Click(); Show(); });
            Step(n++, "split-weather", "Weather:   " + SplitScreen.Weather, d => { SplitScreen.Weather = (Weather)(((int)SplitScreen.Weather + d + 3) % 3); flow.Click(); Show(); });
            int[] limits = { 3, 5, 8 };
            Step(n++, "police-limit", "Round length:   " + SplitScreen.PoliceMinutes + " minutes", d => { int i = System.Array.IndexOf(limits, SplitScreen.PoliceMinutes); SplitScreen.PoliceMinutes = limits[Mathf.Clamp((i < 0 ? 1 : i) + d, 0, 2)]; flow.Click(); Show(); });
            if (!solo) Step(n++, "split-layout", "Screen:   " + (SplitScreen.LeftRight ? "Left / right" : "Top / bottom (player 1 on top)"), d => { flow.Save.Settings.splitLeftRight = !flow.Save.Settings.splitLeftRight; flow.Save.SaveSettings(); flow.Click(); Show(); });
            Row(n, "police-start", SplitScreen.Ready ? "START PATROL" : "START PATROL   (waiting for player 2)", flow.StartSplit);
            var colors = buttons[n].colors; colors.normalColor = new(.1f, .38f, .35f); buttons[n].colors = colors; buttons[n].GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 54; buttons[n++].interactable = SplitScreen.Ready;
            Row(n, "police-back", "Back", () => BackPage());
            if (devices >= 0) { DeviceGlyph(buttons[devices], SplitScreen.P1Device); DeviceGlyph(buttons[devices + 1], SplitScreen.P2Device); buttons[devices].GetComponentInChildren<UnityEngine.UI.Text>(true).color = new Color(.3f, .95f, .81f); buttons[devices + 1].GetComponentInChildren<UnityEngine.UI.Text>(true).color = SplitScreen.P2Device != null ? new Color(1, .74f, .25f) : new Color(.75f, .75f, .72f); }
        }
        void RenderPatrolResults()
        {
            var p = SpeedPatrol.Current; int w = p.Winner;
            string verdict = p.Cops.Count < 2 ? "SCORE  " + (p.Cops.Count > 0 ? p.Cops[0].points : 0) : w == 0 ? "A DRAW" : SplitScreen.NameOf(w).ToUpperInvariant() + " WINS";
            ClearCore("SPEED PATROL RESULTS", verdict + "   ·   " + SplitScreen.PoliceMinutes + "-minute round   ·   " + SplitScreen.Time + " / " + SplitScreen.Weather + "   ·   speeders that got away: " + p.GotAway);
            int n = 0; var widths = new[] { .26f, .14f, .14f, .22f, .24f };
            TableRow(n++, "header", new[] { "Cop", "Points", "Catches", "Fastest caught", "Penalties" }, widths, () => { });
            for (int i = 0; i < p.Cops.Count; i++)
            {
                var c = p.Cops[i]; int place = i < p.Places.Count ? p.Places[i] : 0;
                TableRow(n++, "patrol-cop-" + c.player, new[] { SplitScreen.NameOf(c.player) + (place > 0 ? $"  (Top 10 #{place})" : ""), c.points.ToString(), c.catches.ToString(), c.catches > 0 ? $"{c.fastest:0} mph over" : "—", c.penalties == 0 ? "none" : $"{c.hits} hit, {c.noViolations} no violation" }, widths, () => { }, c.player == w || p.Cops.Count == 1);
            }
            var top = SpeedPatrol.Top(SplitScreen.PoliceMinutes); var tw = new[] { .12f, .34f, .18f, .16f, .2f };
            TableRow(n++, "top-header", new[] { "Top 10", SplitScreen.PoliceMinutes + " min", "Points", "Catches", "Date" }, tw, () => { });
            for (int i = 0; i < top.Count; i++) { var e = top[i]; bool mine = p.Places.Contains(i + 1); TableRow(n++, "patrol-top-" + i, new[] { "#" + (i + 1), e.name, e.points.ToString(), e.catches.ToString(), e.date }, tw, () => { }, mine); }
            Row(n, "rematch", "REMATCH", p.Rematch); var colors = buttons[n].colors; colors.normalColor = new(.1f, .38f, .35f); buttons[n].colors = colors; buttons[n].GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 54; n++;
            Row(n++, "change-setup", "Change setup", () => flow.QuitSplit(true));
            Row(n, "menu", SplitScreen.FromRoam ? "Back to Free Roam" : "Main menu", () => flow.QuitSplit(false));
        }
    }
}
