using System.Linq;
using UnityEngine;

namespace Racer
{
    // 0.96 Part E: the Getaway game of the Police Chase setup (every human runs, the cops are AI) and the results of Getaway and of a
    // solo Runner against the AI cop. Players 1 / 2 (the second may be the AI, which runs too), the vehicles (garage view), where
    // it starts, time of day, weather, traffic, Difficulty (Easy / Normal / Hard: heat, escape time and numbers; the cops are as fast as you), round limit.
    public sealed partial class RaceMenus
    {
        void RenderGetawaySetup()
        {
            bool solo = SplitScreen.Solo; SplitScreen.Mode = SplitScreen.Kind.Police; SplitScreen.P2Ai = solo || SplitScreen.P2AiRunner;
            ClearCore("GETAWAY", "Escape the police. More cops join the longer you stay free."
                + (solo ? "\nYou run on the full screen." : "\nTwo runners, split-screen: the one who stays free longest wins.")
                + "\nA cop is already behind you when it starts. Stay out of every cop's sight to escape; the round limit counts as escaped. Nothing is recorded but the Getaway Top 10." + (SplitScreen.FromRoam ? "  Afterwards you go back to Free Roam." : ""));
            details.fontSize = 17; details.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 84;
            int n = 0; GameRow(n++);
            Step(n++, "police-players", "Players:   " + (solo ? "1   (full screen)" : "2   (split screen)"), d => { SplitScreen.Solo = !SplitScreen.Solo; SplitScreen.P2Ai = SplitScreen.Solo || SplitScreen.P2AiRunner; flow.Click(); Show(); });
            int devices = -1;
            if (!solo)
            {
                devices = n;
                Step(n++, "split-p1", "Player 1:   " + (SplitScreen.P1Device == null ? "press A on a controller, or Enter" : SplitScreen.DeviceName(SplitScreen.P1Device) + "   ✓"), d => CycleSlot(1, d));
                Step(n++, "split-p2", "Player 2:   " + (SplitScreen.P2AiRunner ? "the AI runs too   (left / right: a person)" : SplitScreen.P2Device != null ? SplitScreen.DeviceName(SplitScreen.P2Device) + "   ✓   (left / right: the AI)" : "press A on another controller, or Enter   (left / right: the AI)"),
                    d => { if (SplitScreen.P2AiRunner) SplitScreen.P2AiRunner = false; else if (SplitScreen.P2Device == null || d != 0) SplitScreen.P2AiRunner = true; SplitScreen.P2Ai = SplitScreen.P2AiRunner; flow.Click(); Show(); });
                if (string.IsNullOrEmpty(SplitScreen.P1Name)) SplitScreen.P1Name = PlayerNames.Player;
                Step(n++, "split-p1-name", "Player 1 name:   " + SplitScreen.NameOf(1) + "   (A: new name)", d => { SplitScreen.P1Name = NextKnownName(SplitScreen.P1Name, d, SplitScreen.P2Pick); flow.Click(); Show(); },
                    () => OpenNameEntry("", v => { PlayerNames.Remember(v); flow.Save.SaveSettings(); SplitScreen.P1Name = v; return true; }, null, "PLAYER 1'S NAME"));
                if (!SplitScreen.P2AiRunner)
                {
                    if (string.IsNullOrEmpty(SplitScreen.P2Pick) || SplitScreen.P2Pick == SplitScreen.NameOf(1)) SplitScreen.P2Pick = PlayerNames.Known.FirstOrDefault(k => k != SplitScreen.NameOf(1)) ?? "";
                    Step(n++, "split-p2-name", "Player 2 name:   " + (string.IsNullOrEmpty(SplitScreen.P2Pick) ? "press A to enter it" : SplitScreen.P2Pick + "   (A: new name)"), d => { SplitScreen.P2Pick = NextKnownName(SplitScreen.P2Pick, d, SplitScreen.NameOf(1)); flow.Click(); Show(); },
                        () => OpenNameEntry("", v => { if (v == SplitScreen.NameOf(1)) { keyboardError = "Player 1 already has that name."; return false; } PlayerNames.Remember(v); flow.Save.SaveSettings(); SplitScreen.P2Pick = v; return true; }, null, "PLAYER 2'S NAME"));
                }
            }
            Row(n, "police-vehicles", solo ? "YOUR VEHICLE…   (you run in it)" : "RUNNERS' VEHICLES…   (each player's garage)", () => OpenSplitPick(false, solo ? 1 : 0)); int vehiclesRow = n++;
            var strip = PreviewStrip("Getaway vehicles", buttons[vehiclesRow].transform.GetSiblingIndex() + 1, 170);
            string Vehicle(string id) { var p = VehicleProfile.Find(id); return $"{p.Name} ({p.Class})"; }
            PreviewCard(strip, VehicleProfile.Find(SplitScreen.P1Vehicle), SplitScreen.P1Color, false, (solo ? "You run in: " : SplitScreen.NameOf(1) + " runs in: ") + Vehicle(SplitScreen.P1Vehicle), new Color(.3f, .95f, .81f), 250);
            if (!solo) PreviewCard(strip, VehicleProfile.Find(SplitScreen.P2Vehicle), SplitScreen.P2Color, false, SplitScreen.NameOf(2) + " runs in: " + Vehicle(SplitScreen.P2Vehicle), new Color(1, .74f, .25f), 200);
            PreviewCard(strip, VehicleProfile.Police, -1, false, "The cops: Patrol Car", new Color(.5f, .7f, 1), 200);
            var times = LookPresets.MenuOrder;
            Step(n++, "split-time", "Time of day:   " + SplitScreen.Time, d => { int i = System.Array.IndexOf(times, SplitScreen.Time); SplitScreen.Time = times[((i < 0 ? 1 : i) + d + times.Length) % times.Length]; flow.Click(); Show(); });
            Step(n++, "split-weather", "Weather:   " + SplitScreen.Weather, d => { SplitScreen.Weather = (Weather)(((int)SplitScreen.Weather + d + 3) % 3); flow.Click(); Show(); });
            Toggle(n++, "split-traffic", "Traffic", SplitScreen.Traffic, () => { SplitScreen.Traffic = !SplitScreen.Traffic; flow.Click(); Show(); });
            string[] names = { "Easy   (heat every 45 s, escape in 20 s, at most 4 cops)", "Normal   (heat every 30 s, escape in 30 s)", "Hard   (heat every 20 s, escape in 40 s)" };
            Step(n++, "police-difficulty", "Difficulty:   " + names[SplitScreen.PoliceDifficulty], d => { SplitScreen.PoliceDifficulty = Mathf.Clamp(SplitScreen.PoliceDifficulty + d, 0, 2); flow.Click(); Show(); });
            int[] limits = { 3, 5, 8 };
            Step(n++, "police-limit", "Round limit:   " + SplitScreen.PoliceMinutes + " minutes", d => { int i = System.Array.IndexOf(limits, SplitScreen.PoliceMinutes); SplitScreen.PoliceMinutes = limits[Mathf.Clamp((i < 0 ? 1 : i) + d, 0, 2)]; flow.Click(); Show(); });
            if (!solo) Step(n++, "split-layout", "Screen:   " + (SplitScreen.LeftRight ? "Left / right" : "Top / bottom (player 1 on top)"), d => { flow.Save.Settings.splitLeftRight = !flow.Save.Settings.splitLeftRight; flow.Save.SaveSettings(); flow.Click(); Show(); });
            Row(n, "police-start", SplitScreen.Ready ? "START GETAWAY" : "START GETAWAY   (waiting for player 2)", flow.StartSplit);
            var colors = buttons[n].colors; colors.normalColor = new(.1f, .38f, .35f); buttons[n].colors = colors; buttons[n].GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 54; buttons[n++].interactable = SplitScreen.Ready;
            Row(n, "police-back", "Back", () => BackPage());
            if (devices >= 0) { DeviceGlyph(buttons[devices], SplitScreen.P1Device); DeviceGlyph(buttons[devices + 1], SplitScreen.P2Device); buttons[devices].GetComponentInChildren<UnityEngine.UI.Text>(true).color = new Color(.3f, .95f, .81f); }
        }

        void RenderGetawayResults()
        {
            var g = GetawayChase.Current; bool ga = g.Mode == GetawayChase.Variant.Getaway; int w = g.Winner; var humans = g.Runners.Where(r => r.human).ToList();
            string verdict = humans.Count < 2 ? (humans.Count > 0 ? g.Verdict(humans[0]) : "") : w == 0 ? "A DRAW" : SplitScreen.NameOf(w).ToUpperInvariant() + " STAYED FREE LONGEST";
            ClearCore(ga ? "GETAWAY RESULTS" : "POLICE CHASE RESULTS", verdict + "   ·   " + SplitScreen.PoliceMinutes + "-minute round   ·   " + new[] { "Easy", "Normal", "Hard" }[SplitScreen.PoliceDifficulty] + "   ·   " + SplitScreen.Time + " / " + SplitScreen.Weather
                + (humans.Count < 2 && !ga ? "\nOne player keeps one role: change it in the setup. Rematch plays it again." : ""));
            int n = 0; var widths = ga ? new[] { .2f, .26f, .14f, .12f, .14f, .14f } : new[] { .24f, .36f, .2f, .2f };
            TableRow(n++, "header", ga ? new[] { "Runner", "Result", "Time free", "Top heat", "Dodged", "Blocks passed" } : new[] { "Runner", "Result", "Time", "Cops dodged" }, widths, () => { });
            foreach (var r in humans)
            {
                var cells = ga ? new[] { SplitScreen.NameOf(r.player), g.Verdict(r), RaceHud.FormatTime(r.freeSeconds).Substring(0, 5), r.topHeat.ToString(), r.dodged.ToString(), r.blocks.ToString() }
                    : new[] { SplitScreen.NameOf(r.player), g.Verdict(r), RaceHud.FormatTime(r.freeSeconds).Substring(0, 5), r.dodged.ToString() };
                TableRow(n++, "getaway-run-" + r.player, cells, widths, () => { }, humans.Count == 1 || r.player == w);
            }
            if (ga)
            {
                var top = GetawayChase.Top(SplitScreen.PoliceMinutes); var tw = new[] { .12f, .34f, .2f, .16f, .18f };
                TableRow(n++, "top-header", new[] { "Top 10", SplitScreen.PoliceMinutes + " min", "Time free", "Heat", "Date" }, tw, () => { });
                for (int i = 0; i < top.Count; i++) { var e = top[i]; bool mine = g.Places.Contains(i + 1); TableRow(n++, "getaway-top-" + i, new[] { "#" + (i + 1), e.name, RaceHud.FormatTime(e.seconds).Substring(0, 5), e.heat.ToString(), e.date }, tw, () => { }, mine); }
            }
            Row(n, "rematch", "REMATCH", g.Rematch); var colors = buttons[n].colors; colors.normalColor = new(.1f, .38f, .35f); buttons[n].colors = colors; buttons[n].GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 54; n++;
            Row(n++, "change-setup", "Change setup", () => flow.QuitSplit(true));
            Row(n, "menu", SplitScreen.FromRoam ? "Back to Free Roam" : "Main menu", () => flow.QuitSplit(false));
        }
    }
}
