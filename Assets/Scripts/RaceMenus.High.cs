using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Racer
{
    // 1.2 HIGH MODE in the menus (the switch: RaceFlow.SetHighMode; the mode: HighMode.cs), and Free Roam easy to find (both modes).
    // - HIGH MODE: ON / OFF is the main menu's top row in both modes (A, or left / right), and a row of a race's pause menu (a race keeps the
    //   mode it started with, so it asks "Restart in HIGH MODE?").
    // - HIGH MODE's main menu: HIGH MODE, (RESUME DRIVING), FREE ROAM, RACE, CHASES & STUNTS, VEHICLE, TIME & WEATHER, REPLAYS, SETTINGS, QUIT.
    //   RACE is the track list (map beside it), then go. No campaign; the records are reached from a race's results and REPLAYS.
    // - FREE ROAM asks 1 PLAYER / 2 PLAYERS (normal mode: 1 is the Free Roam page as before, 2 the split-screen setup on Free Roam; HIGH MODE:
    //   1 drives at once, 2 is one join row and START).
    // - VEHICLE: one scrolling row of every vehicle with its picture and three stat bars, left / right then A drives it (colors and rider as
    //   last used); also on a HIGH MODE race's results. TIME & WEATHER: two rows. CHASES & STUNTS: 1 / 2 players and four choices, each starts
    //   at once with its defaults (Easy, 5 minutes, traffic on, HIGH MODE's time and weather, the vehicle chosen in VEHICLE).
    public sealed partial class RaceMenus
    {
        string HighLabel => "HIGH MODE:   " + (HighMode.On ? "ON" : "OFF");
        void HighRow(int index)
        {
            Step(index, "high-mode", HighLabel, d => SwitchHigh(), SwitchHigh); listRows.Remove(index); // A switches it too (not a list)
            var label = buttons[index].GetComponentInChildren<UnityEngine.UI.Text>(true); label.alignment = TextAnchor.MiddleCenter;
            label.color = HighMode.On ? HighMode.Colour : new Color(.82f, .88f, .88f);
            var c = buttons[index].colors; c.normalColor = HighMode.On ? new Color(.30f, .16f, .05f) : new Color(.10f, .20f, .25f); buttons[index].colors = c;
        }
        // 1.2.0-review2: a split-screen player's own HIGH MODE (A or left / right); in a race it asks to restart (or applies from the next race)
        void PlayerHighRow(int index, int player)
        {
            bool on = player == 1 ? SplitScreen.P1High : SplitScreen.P2High;
            Step(index, "high-p" + player, SplitScreen.NameOf(player) + ":   HIGH MODE " + (on ? "ON   (bumpers, no wrecks)" : "OFF"), d => SwitchPlayerHigh(player), () => SwitchPlayerHigh(player)); listRows.Remove(index);
            buttons[index].GetComponentInChildren<UnityEngine.UI.Text>(true).color = on ? HighMode.Colour : new Color(.82f, .88f, .88f);
            var c = buttons[index].colors; c.normalColor = on ? new Color(.30f, .16f, .05f) : new Color(.10f, .20f, .25f); buttons[index].colors = c;
        }
        int PlayerHighRows(int n) { PlayerHighRow(n++, 1); if (!SplitScreen.P2Ai) PlayerHighRow(n++, 2); return n; }
        void SwitchPlayerHigh(int player)
        {
            bool on = !(player == 1 ? SplitScreen.P1High : SplitScreen.P2High), midRace = SplitScreen.Active && flow.TrackBrowsingLocked;
            flow.SetPlayerHigh(player, on); string who = SplitScreen.NameOf(player) + ": HIGH MODE " + (on ? "on" : "off");
            if (midRace) Confirm("RESTART THE RACE?", who + ". Restart this race now, or keep racing as it is: the change applies from the next race.", flow.RestartForMode, "RESTART", () => flow.Notify(who + " from the next race", 4), "KEEP RACING");
            else Show();
        }
        static void PlayersHighDefault() { SplitScreen.P1High = SplitScreen.P2High = HighMode.On; }
        void SwitchHigh()
        {
            bool on = !HighMode.On, midRace = flow.TrackBrowsingLocked;
            flow.SetHighMode(on);
            if (midRace) Confirm(on ? "RESTART IN HIGH MODE?" : "RESTART IN NORMAL MODE?", (on ? "HIGH MODE is on." : "HIGH MODE is off.") + " Restart this race now, or keep racing as it is: the change applies from the next race.",
                flow.RestartForMode, "RESTART", () => flow.Notify((on ? "HIGH MODE" : "Normal mode") + " from the next race", 4), "KEEP RACING");
            else { flow.Notify(on ? "HIGH MODE is on" : "HIGH MODE is off", 3); Show(); }
        }
        // Part F: the question (RaceFlow.AskHighMode pauses first); B or NO is No
        public void AskHighMode()
        {
            string what = CampaignRun.Active != null ? "\nYes leaves this campaign event and races the same track in HIGH MODE (your campaign is untouched)."
                : flow.Race.FreeRoam ? "\nYes carries on in Free Roam in HIGH MODE." : "\nYes restarts this race in HIGH MODE.";
            Confirm("WOULD YOU LIKE TO PLAY HIGH MODE?", "HIGH MODE is the simple, forgiving way to play: bumpers along the race courses, no wrecks, everything unlocked." + what,
                flow.AcceptHighMode, "YES", flow.DeclineHighMode, "NO");
        }
        bool HighPlayersPage => page == "high-chases" || page == "high-two";
        bool RenderHighPages()
        {
            if (page.StartsWith("high-") || page == "roam-players" || page == "race-players") { if (renderedKey != PageKey) selections[flow.State] = 0; } // a page opens on its first row
            switch (page)
            {
                case "roam-players": RenderRoamPlayers(); return true;
                case "race-players": RenderRacePlayers(); return true;
                case "high-two": RenderHighTwo(); return true;
                case "high-chases": RenderHighChases(); return true;
                case "high-vehicle": RenderHighVehicle(); return true;
                case "high-time": RenderHighTime(); return true;
            }
            return false;
        }
        static string TimeWeatherLine => HighMode.TimeNames[HighMode.TimeChoice] + " · " + HighMode.WeatherNames[HighMode.WeatherChoice];
        void RenderHighMain()
        {
            ClearCore("WOODSTOCK RUSH   ·   HIGH MODE", "Version " + Application.version + "   ·   simple and forgiving: everything unlocked, bumpers on the race courses");
            int n = 0; HighRow(n++);
            if (flow.RoamMenu) Row(n++, "resume", "RESUME DRIVING", flow.Resume);
            Row(n++, "roam", "FREE ROAM", () => Navigate("roam-players"));
            Row(n++, "race", "RACE", () => Navigate("race-players"));
            Row(n++, "high-chases", "CHASES & STUNTS", OpenHighChases);
            Row(n++, "high-vehicle", "VEHICLE   ·   " + VehicleProfile.Find(flow.Save.Settings.vehicleId).Name, () => Navigate("high-vehicle"));
            Row(n++, "high-time", "TIME & WEATHER   ·   " + TimeWeatherLine, () => Navigate("high-time"));
            Row(n++, "replays", "REPLAYS", () => Navigate("replays"));
            Row(n++, "settings", "SETTINGS", flow.OpenSettings);
            Row(n++, "quit", "QUIT GAME", ConfirmQuit);
        }
        // ---------- FREE ROAM: 1 or 2 players (both modes) ----------
        void RenderRoamPlayers()
        {
            ClearCore("FREE ROAM", "Drive the whole world: one player on the full screen, or two players in split screen.");
            Row(0, "roam-1p", "1 PLAYER", () => { if (!HighMode.On) Navigate("roam"); else if (flow.RoamMenu) flow.Resume(); else flow.StartFreeRoam(); });
            Row(1, "roam-2p", "2 PLAYERS   (split screen)", () => { if (HighMode.On) { PrepareHighPlayers(); SplitScreen.Mode = SplitScreen.Kind.FreeRoam; SplitScreen.Solo = false; SplitScreen.P2Ai = false; Navigate("high-two"); } else { SplitScreen.Mode = SplitScreen.Kind.FreeRoam; OpenSplitSetup(); } });
            Row(2, "back", "Back", () => BackPage());
        }
        // 1.2.0-review2: HIGH MODE's RACE: 1 player (the track list, then go) or 2 (the split-screen race setup, each player's HIGH MODE on its own row)
        void RenderRacePlayers()
        {
            ClearCore("RACE", "One player: pick a track and go. Two players: split screen, and each player chooses HIGH MODE or not.");
            Row(0, "race-1p", "1 PLAYER", flow.OpenCourses);
            Row(1, "race-2p", "2 PLAYERS   (split screen)", () => { SplitScreen.Mode = SplitScreen.Kind.Race; OpenSplitSetup(); });
            Row(2, "back", "Back", () => BackPage());
        }
        // ---------- HIGH MODE: two players ----------
        void PrepareHighPlayers()
        {
            var keyboard = (InputDevice)Keyboard.current; SplitScreen.P1Device = MenuInput.Controller && MenuInput.Pad != null ? MenuInput.Pad : keyboard;
            if (SplitScreen.P2Device != null && (!SplitScreen.P2Device.added || SplitScreen.P2Device == SplitScreen.P1Device)) SplitScreen.P2Device = null;
            if (HighMode.WeatherChoice == 3) HighMode.Roll();
            SplitScreen.Time = HighMode.RaceTime; SplitScreen.Weather = HighMode.Weather; SplitScreen.Traffic = true; SplitScreen.P1Name = ""; PlayersHighDefault();
            int here = RacePlaylists.CourseOf(flow.gameObject.scene); SplitScreen.Course = Mathf.Clamp(flow.InRoamWorld ? RaceFlow.RoamCourse : here >= 0 ? here : 0, 0, RacePlaylists.Scenes.Length - 1);
        }
        // each player's vehicle for the game chosen: player 1's from VEHICLE (the stunt choice for the Stunt event), player 2's last driven
        void HighVehicles()
        {
            var allowed = SplitScreen.Vehicles; if (allowed.Length == 0) return; PreselectVehicles(allowed);
            string mine = SplitScreen.Mode == SplitScreen.Kind.Stunt ? flow.Save.Settings.stuntVehicleId : flow.Save.Settings.vehicleId;
            string Fix(string id, string fallback) => allowed.Any(v => v.Id == id) ? id : allowed.Any(v => v.Id == fallback) ? fallback : allowed[0].Id;
            SplitScreen.P1Vehicle = Fix(mine, SplitScreen.P1Vehicle); SplitScreen.P1Vehicle = Fix(SplitScreen.P1Vehicle, "original"); SplitScreen.P2Vehicle = Fix(SplitScreen.P2Vehicle, "atv");
            int scheme = SchemeOf(SplitScreen.P1Vehicle); if (scheme >= 0 && scheme < VehiclePaint.Count) SplitScreen.P1Color = scheme;
        }
        string JoinLine(InputDevice d) => d != null ? SplitScreen.DeviceName(d) + "   ✓" : MenuInput.JoinHint(true) + " to join";
        void RenderHighTwo()
        {
            ClearCore("FREE ROAM   ·   2 PLAYERS", "Split screen, both players in HIGH MODE. Player 2: " + MenuInput.JoinHint(true) + ".");
            Step(0, "split-p1", "Player 1:   " + JoinLine(SplitScreen.P1Device), d => CycleSlot(1, d));
            Step(1, "split-p2", "Player 2:   " + (SplitScreen.P2Ai ? "the AI drives   (A on another controller joins)" : JoinLine(SplitScreen.P2Device)), d => CycleSlot(2, d));
            int n = PlayerHighRows(2);
            Row(n, "high-two-start", SplitScreen.Ready ? "START FREE ROAM" : "START FREE ROAM   (waiting for player 2)", () => { SplitScreen.Mode = SplitScreen.Kind.FreeRoam; HighVehicles(); flow.StartSplit(); });
            StartColours(n++, SplitScreen.Ready);
            Row(n, "back", "Back", () => BackPage());
            DeviceGlyph(buttons[0], SplitScreen.P1Device); DeviceGlyph(buttons[1], SplitScreen.P2Ai ? null : SplitScreen.P2Device);
        }
        void StartColours(int i, bool ready)
        {
            var colors = buttons[i].colors; colors.normalColor = new(.1f, .38f, .35f); buttons[i].colors = colors; buttons[i].GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 54; buttons[i].interactable = ready;
        }
        // ---------- HIGH MODE: CHASES & STUNTS ----------
        void OpenHighChases() { PrepareHighPlayers(); SplitScreen.Solo = true; SplitScreen.P2Ai = true; SplitScreen.P2AiRunner = false; Navigate("high-chases"); }
        void RenderHighChases()
        {
            bool solo = SplitScreen.Solo; SplitScreen.P2Ai = solo || SplitScreen.P2Ai;
            bool ready = SplitScreen.P1Device != null && (solo || (!SplitScreen.P2Ai && SplitScreen.P2Device != null));
            ClearCore("CHASES & STUNTS", (solo ? "One player on the full screen." : "Two players, split screen.") + "\nPick one and it starts: Easy, 5 minutes, your HIGH MODE vehicle, time and weather.");
            int n = 0;
            Step(n++, "high-players", "Players:   " + (solo ? "1   (full screen)" : "2   (split screen)"), d => { SplitScreen.Solo = !SplitScreen.Solo; SplitScreen.P2Ai = SplitScreen.Solo; flow.Click(); Show(); });
            int p2 = -1;
            if (!solo) { p2 = n; Step(n++, "split-p2", "Player 2:   " + (SplitScreen.P2Ai ? MenuInput.JoinHint(true) + " to join" : JoinLine(SplitScreen.P2Device)), d => CycleSlot(2, d)); n = PlayerHighRows(n); }
            string Wait(string s) => ready ? s : s + "   (waiting for player 2)";
            var games = new (string id, string label)[] { ("high-getaway", "RUN FROM THE COPS   ·   the police chase you"), ("high-cop", solo ? "BE THE COP   ·   catch the runner" : "BE THE COP   ·   one chases, one runs, then swap"),
                ("high-patrol", "CATCH SPEEDERS   ·   clock them, pull them over"), ("high-stunt", "STUNT EVENT   ·   flips and jumps at " + StuntPark.Title) };
            for (int g = 0; g < games.Length; g++) { int game = g; Row(n, games[g].id, Wait(games[g].label), () => StartHighChase(game)); buttons[n].interactable = ready; n++; }
            Row(n, "back", "Back", () => BackPage());
            if (p2 >= 0) DeviceGlyph(buttons[p2], SplitScreen.P2Ai ? null : SplitScreen.P2Device);
        }
        void StartHighChase(int game)
        {
            bool solo = SplitScreen.Solo; SplitScreen.P2Ai = solo; SplitScreen.P2AiRunner = false;
            if (flow.InRoamWorld && !SplitScreen.Active) { SplitScreen.FromRoam = true; SplitScreen.RoamReturn = RaceFlow.RoamCourse; } else SplitScreen.FromRoam = false;
            SplitScreen.PoliceMinutes = 5; SplitScreen.PoliceDifficulty = 0; SplitScreen.CopVehicle = VehicleProfile.Police.Id; SplitScreen.SoloRole = 1; SplitScreen.CopFirst = 1;
            if (game == 3) SplitScreen.Mode = SplitScreen.Kind.Stunt;
            else { SplitScreen.Mode = SplitScreen.Kind.Police; SplitScreen.PoliceGame = game == 0 ? SplitScreen.Game.Getaway : game == 1 ? SplitScreen.Game.CopRunner : SplitScreen.Game.SpeedPatrol; }
            HighVehicles();
            if (!SplitScreen.Ready) { flow.Notify("Waiting for player 2: press A on another controller", 3); flow.Click(); return; }
            flow.StartSplit();
        }
        // ---------- HIGH MODE: VEHICLE ----------
        string highPick;
        static VehicleProfile[] HighVehicleList => VehicleProfile.All.Where(p => !VehicleUnlocks.Locked(p) && !Campaign.VehicleLocked(p)).ToArray();
        void RenderHighVehicle()
        {
            var list = HighVehicleList; if (list.Length == 0) { BackPage(); return; }
            if (renderedKey != PageKey || !list.Any(v => v.Id == highPick)) highPick = (list.FirstOrDefault(v => v.Id == flow.Save.Settings.vehicleId) ?? list[0]).Id;
            int at = System.Array.FindIndex(list, v => v.Id == highPick); var p = list[at];
            ClearCore("VEHICLE", "Left / right: every vehicle.   A: drive it.   Colors and rider stay as you last set them.");
            var strip = PreviewStrip("Every vehicle", details.transform.GetSiblingIndex() + 1, 262);
            strip.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;
            for (int k = -2; k <= 2; k++) HighCard(strip, list[((at + k) % list.Length + list.Length) % list.Length], k == 0);
            Step(0, "high-vehicle-pick", $"{p.Name}  ({p.Class})   ·   {at + 1} / {list.Length}", d => { highPick = list[(at + d + list.Length) % list.Length].Id; flow.Click(); Show(); },
                () => { flow.ChooseVehicle(highPick); flow.Notify("Driving the " + VehicleProfile.Find(highPick).Name, 3); BackPage(); });
            buttons[0].GetComponentInChildren<UnityEngine.UI.Text>(true).alignment = TextAnchor.MiddleCenter; // A drives it (the footer shows A as well as left / right)
            Row(1, "back", "Back", () => BackPage());
        }
        static string Where(VehicleProfile p) => p.Stunt ? "Free Roam and the Stunt event" : p.IsPolice ? "a patrol vehicle" : p.Small ? "every track" : "not on the forest tracks";
        void HighCard(RectTransform strip, VehicleProfile p, bool centre)
        {
            float width = centre ? 236 : 160;
            var cell = Rect("Vehicle " + p.Id, strip); cell.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().preferredWidth = width;
            cell.gameObject.AddComponent<UnityEngine.UI.Image>().color = centre ? new Color(.12f, .32f, .35f) : new Color(.06f, .1f, .13f);
            var v = cell.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>(); v.padding = new RectOffset(6, 6, 6, 6); v.spacing = 3; v.childControlWidth = v.childControlHeight = true; v.childForceExpandHeight = false;
            var pic = Rect("Picture", cell); pic.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 128;
            var raw = MiniPreview(pic, p, SchemeOf(p.Id), false, Mathf.RoundToInt(width * 1.5f), 190); var r = raw.rectTransform; r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero;
            var name = Label("Name", cell, centre ? 18 : 16, 24); name.alignment = TextAnchor.MiddleCenter; name.text = p.Name; name.color = centre ? new Color(.3f, .95f, .81f) : Color.white;
            StatBars(cell, p, false, 18, 14, .5f, new[] { 0, 1, 3 }); // top speed, acceleration, handling
            var where = Label("Where", cell, 13, 18); where.alignment = TextAnchor.MiddleCenter; where.text = Where(p); where.color = new Color(.66f, .78f, .8f);
            tableCells.Add(cell.gameObject);
        }
        // ---------- HIGH MODE: TIME & WEATHER ----------
        void RenderHighTime()
        {
            ClearCore("TIME & WEATHER", "For Free Roam and races in HIGH MODE. Left / right changes a row.");
            var s = flow.Save.Settings;
            Step(0, "high-time-of-day", "Time of day:   " + HighMode.TimeNames[HighMode.TimeChoice], d => { s.highTime = (HighMode.TimeChoice + d + 5) % 5; flow.Save.SaveSettings(); flow.Click(); Show(); });
            Step(1, "high-weather", "Weather:   " + HighMode.WeatherNames[HighMode.WeatherChoice], d => { s.highWeather = (HighMode.WeatherChoice + d + 4) % 4; if (s.highWeather == 3) HighMode.Roll(); flow.Save.SaveSettings(); flow.Click(); Show(); });
            Row(2, "back", "Back", () => BackPage());
        }
    }
}
