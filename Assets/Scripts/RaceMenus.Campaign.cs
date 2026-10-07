using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Racer
{
    // 0.89 campaign, round 1: the campaign screen (main menu > CAMPAIGN), the event page and the results-and-payout page.
    // Campaign screen: money at the top; Continue / Shop / New Campaign / Back; the chapters down the left of the list
    // (locked ones say what opens them) and the selected chapter's events beside them; the course map panel (as on the
    // Tracks screen) shows the highlighted event's route, with its type, course, conditions, entry rule, payout and best.
    // 0.90: each chapter's championship is a row after its final (trophy, "Round n of m" while in progress); its own page
    // (start with a vehicle / resume / restart / abandon); a round's results show the race and the standings table, then
    // Next race; winning the Grand Championship shows the Champion of Woodstock screen.
    public sealed partial class RaceMenus
    {
        int campaignChapter; string campaignEvent, campaignVehicle, campaignCup;
        // Tracks opened from the Free Roam page picks where Free Roam starts: the whole world is open there.
        public bool RoamTrackPick => stagePages.TryGetValue(RaceFlow.Stage.Ready, out var from) && from == "roam";
        // chapter: the chapter to show (the one of the event or championship just run); 0 keeps the current one.
        public void OpenCampaign(int chapter = 0) { if (chapter > 0) campaignChapter = chapter; page = "campaign"; pages.Clear(); stagePages[RaceFlow.Stage.Ready] = page; modalConfirm = null; Show(); }
        void ShowCampaignEvent(CampaignEvent e)
        {
            if (e == null || !courseView) return;
            ShowCourseOnMap(e.Course); Campaign.Selected = e.Id;
            courseCaption.fontSize = 16;
            courseCaption.text = $"{e.Name}  ·  {e.CourseTitle}\n{e.KindLabel}  ·  {e.Conditions}\n{e.Entry}  ·  {Campaign.PayText(e)}\n{Campaign.BestText(e)}";
        }
        void ShowCampaignCup(CampaignCup c)
        {
            if (c == null || !courseView) return;
            var progress = Campaign.ActiveCup(c); int round = progress != null ? Mathf.Min(progress.races.Count, c.Rounds.Length - 1) : 0;
            ShowCourseOnMap(c.Rounds[round].Course); Campaign.Selected = c.Id;
            courseCaption.fontSize = 16;
            courseCaption.text = $"{c.Name}  ·  championship, {c.Rounds.Length} races\n{(progress != null ? $"Next: round {round + 1}, {RacePlaylists.Titles[c.Rounds[round].Course].Replace(" - ", " — ")}" : "Points 10 / 7 / 5 / 3 / 2 / 1 per race")}\nWins {Campaign.Money(c.Pay)} · first win +{Campaign.Money(c.Bonus)}\n{CupBest(c)}";
        }
        static string ShortKind(CampaignEvent e) => e.Kind switch { CampaignEventKind.Race => $"Race, {e.Laps} laps", CampaignEventKind.TimeTrial => "Time trial", CampaignEventKind.Jump => "Jump", CampaignEventKind.Smash => "Smash", _ => "Speed trap" };
        // the event row's best: place or medal only (the full best is in the caption beside the map)
        static string ShortBest(CampaignEvent e) { var r = Campaign.ResultOf(e.Id); if (r == null || r.runs == 0) return "not run yet"; return e.Kind == CampaignEventKind.Race ? (r.bestPlace > 0 ? "best " + Campaign.Ordinal(r.bestPlace) : "best: DNF") : "best: " + new[] { "no medal", "bronze", "silver", "gold" }[r.bestMedal]; }
        // The trophy mark: the best final position (★ = won).
        static string CupBest(CampaignCup c) { var r = Campaign.CupResult(c.Id); if (r == null || !r.finished) return "No trophy yet"; return r.won ? "★ TROPHY: won" : "Best: " + Campaign.Ordinal(r.bestPosition) + " overall"; }
        static string CupState(CampaignCup c)
        {
            if (!Campaign.CupOpen(c) && !Campaign.Testing) return "Locked: " + c.Opens;
            var active = Campaign.ActiveCup(c); if (active != null) return $"Round {active.races.Count + 1} of {c.Rounds.Length}  ·  Resume";
            var r = Campaign.CupResult(c.Id); return r != null && r.finished ? (r.won ? "★ Won" : "Best " + Campaign.Ordinal(r.bestPosition)) : "Open";
        }
        void RenderCampaign()
        {
            var st = Campaign.Current; var chapters = CampaignData.Chapters;
            if (campaignChapter < 1 || campaignChapter > chapters.Length) campaignChapter = Mathf.Clamp(Campaign.Next?.Chapter ?? st.chapter, 1, chapters.Length);
            int passed = CampaignData.Events.Count(Campaign.Passed), trophies = st.cups.Count(c => c.won);
            ClearCore("CAMPAIGN   ·   " + Campaign.Money(st.money), Campaign.Error ?? (Campaign.Testing
                ? "Testing mode is on (Settings): events can be run, but nothing is saved or paid."
                : (st.complete ? "★ CHAMPION OF WOODSTOCK  ·  " : $"Chapter {st.chapter} of {chapters.Length}  ·  ") + $"{passed} of {CampaignData.Events.Length} events passed  ·  {trophies} of {CampaignData.Cups.Length} trophies  ·  {st.owned.Count} vehicles owned"));
            details.fontSize = 18; details.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 28;
            var actions = LaterGroup("Campaign actions", content, true); actions.SetSiblingIndex(1); int n = 0;
            void ActionButton(string id, string label, System.Action action) { Row(n, id, label, action); var t = buttons[n].GetComponentInChildren<UnityEngine.UI.Text>(true); t.alignment = TextAnchor.MiddleCenter; t.fontSize = 18; buttons[n++].transform.SetParent(actions, false); }
            var next = Campaign.Next;
            // Continue: a championship in progress first (resume), else the next event
            var resume = CampaignData.Cups.FirstOrDefault(c => Campaign.ActiveCup(c) != null);
            ActionButton("continue", Campaign.Exists ? "Continue" : "Start", () => { if (resume != null) { campaignChapter = resume.AfterChapter; campaignCup = resume.Id; Navigate("campaign-cup"); } else if (next != null) { campaignChapter = next.Chapter; campaignEvent = next.Id; Navigate("campaign-event"); } });
            ActionButton("shop", "Shop", OpenCampaignShop);
            ActionButton("new-campaign", "New Campaign", () => { if (Campaign.HasProgress) Confirm("START A NEW CAMPAIGN?", "This replaces your campaign: money, vehicles bought or won, upgrades, results, trophies and opened courses.\nRecords, acorns, settings and Free Roam are not touched.", NewCampaign, "NEW CAMPAIGN"); else NewCampaign(); });
            ActionButton("back", "Back", () => BackPage());
            var split = LaterGroup("Chapters and events", content, true, 400); split.SetSiblingIndex(2);
            var left = Pane("Chapters", split, 200); var right = Pane("Events", split, 430);
            left.parent.parent.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 400; right.parent.parent.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 400;
            foreach (var c in chapters)
            {
                int number = c.Number; bool open = Campaign.ChapterOpen(number);
                Row(n, "chapter-" + number, $"{(campaignChapter == number ? "› " : "")}{number}  {c.Name}\n{(open ? "Open" : "Locked: " + c.Opens)}", () => { campaignChapter = number; Show(); if (rightPaneButtons.Count > 0) EventSystem.current.SetSelectedGameObject(rightPaneButtons[0].gameObject); });
                var b = buttons[n]; b.transform.SetParent(left, false); var label = b.GetComponentInChildren<UnityEngine.UI.Text>(true); label.fontSize = 17;
                b.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = open ? 58 : 92; leftPaneButtons.Add(buttons[n++]);
            }
            var events = CampaignData.InChapter(campaignChapter);
            for (int i = 0; i < events.Length; i++)
            {
                var e = events[i]; bool available = Campaign.Available(e); var result = Campaign.ResultOf(e.Id);
                string mark = result?.passed == true ? "✓ " : "";
                Row(n, "cev-" + e.Id, $"{i + 1}. {mark}{e.Name}{(e.Course % 2 == 1 ? "  (Reverse)" : "")}\n{ShortKind(e)}  ·  {(available ? ShortBest(e) : "Locked")}", () => { campaignEvent = e.Id; Navigate("campaign-event"); });
                var b = buttons[n]; b.transform.SetParent(right, false); b.GetComponentInChildren<UnityEngine.UI.Text>(true).fontSize = 18; b.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 62;
                if (!available) { var colors = b.colors; colors.normalColor = new(.07f, .11f, .14f); b.colors = colors; }
                if (!b.GetComponent<CourseRowHover>()) b.gameObject.AddComponent<CourseRowHover>();
                rightPaneButtons.Add(buttons[n++]);
            }
            // 0.90: the chapter's championship after its final
            foreach (var cup in CampaignData.CupsAfter(campaignChapter))
            {
                var c = cup; bool open = Campaign.CupOpen(c) || Campaign.Testing;
                Row(n, "ccup-" + c.Id, $"★  {c.Name}\nChampionship, {c.Rounds.Length} races  ·  {CupState(c)}", () => { campaignCup = c.Id; Navigate("campaign-cup"); });
                var b = buttons[n]; b.transform.SetParent(right, false); b.GetComponentInChildren<UnityEngine.UI.Text>(true).fontSize = 18; b.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 62;
                var colors = b.colors; colors.normalColor = open ? new(.16f, .22f, .12f) : new(.07f, .11f, .14f); b.colors = colors;
                if (!b.GetComponent<CourseRowHover>()) b.gameObject.AddComponent<CourseRowHover>();
                rightPaneButtons.Add(buttons[n++]);
            }
            EnterCourseView(.4f);
            var shownCup = CampaignData.CupsAfter(campaignChapter).FirstOrDefault(x => x.Id == Campaign.Selected);
            if (shownCup != null) ShowCampaignCup(shownCup);
            else if (events.Length > 0) ShowCampaignEvent(events.FirstOrDefault(x => x.Id == Campaign.Selected) ?? events.FirstOrDefault(x => x == next) ?? events[0]);
        }
        // 0.90 Part A: the main menu's line under CAMPAIGN: chapter, its events passed (the championship counts), money (or
        // that Testing is on).
        public static string CampaignStatusLine
        {
            get
            {
                if (Campaign.Testing) return "TESTING: everything unlocked";
                if (Campaign.Error != null) return "Campaign save could not be read";
                var st = Campaign.Current; var c = CampaignData.Chapters[Mathf.Clamp(st.chapter, 1, CampaignData.Chapters.Length) - 1];
                if (st.complete) return $"★ Champion of Woodstock  ·  {Campaign.Money(st.money)}";
                var events = CampaignData.InChapter(c.Number); var cups = CampaignData.CupsAfter(c.Number);
                int done = events.Count(Campaign.Passed) + cups.Count(x => Campaign.CupResult(x.Id)?.finished == true);
                return $"Chapter {c.Number}: {c.Name}  ·  {done} of {events.Length + cups.Length} events  ·  {Campaign.Money(st.money)}";
            }
        }
        // 0.90 Part B: a new campaign offers the new-player hints again (when some have been shown).
        void NewCampaign() { Campaign.Reset(); campaignChapter = 1; campaignEvent = null; flow.Notify(Campaign.Error ?? "New campaign started", 4); if (Hints.AnySeen) Confirm("SHOW THE HINTS AGAIN?", "The short new-player hints (controls, shortcuts, the Shop) can be shown again for this campaign.", Hints.Reset, "SHOW HINTS"); else Show(); }
        void OpenCampaignShop() { shopFromCampaign = true; flow.OpenGarage(); page = "shop"; pages.Clear(); Show(); }
        // The vehicles the player may enter: owned (all of them while Testing) that the event's class rule allows, never the mower.
        VehicleProfile[] EventVehicles(CampaignEvent e) => VehicleProfile.All.Where(p => !p.Reward && (Campaign.Testing || Campaign.Owns(p.Id)) && e.Allows(p) && (CarAccess.CourseAllowsCars(RacePlaylists.Scenes[e.Course]) || p.Small)).ToArray();
        // A championship: owned vehicles allowed on every one of its courses.
        VehicleProfile[] CupVehicles(CampaignCup c) => VehicleProfile.All.Where(p => !p.Reward && (Campaign.Testing || Campaign.Owns(p.Id)) && (c.Rounds.All(r => CarAccess.CourseAllowsCars(RacePlaylists.Scenes[r.Course])) || p.Small)).ToArray();
        string PickVehicle(VehicleProfile[] list)
        {
            if (list.Length > 0 && !list.Any(p => p.Id == campaignVehicle)) campaignVehicle = list.Any(p => p.Id == flow.Race.vehicle.GetComponent<VehicleConfiguration>().profileId) ? flow.Race.vehicle.GetComponent<VehicleConfiguration>().profileId : list[0].Id;
            return campaignVehicle;
        }
        void VehicleStepper(int row, VehicleProfile[] list, string empty)
        {
            int at = System.Array.FindIndex(list, p => p.Id == campaignVehicle);
            void StepVehicle(int d) { if (list.Length == 0) return; campaignVehicle = list[((at < 0 ? 0 : at) + d + list.Length) % list.Length].Id; flow.Click(); Show(); }
            var v = VehicleProfile.Find(campaignVehicle);
            Step(row, "event-vehicle", list.Length == 0 ? empty : $"Vehicle: {v.Name}  ({v.Class}, {at + 1} of {list.Length})" + (Campaign.Upgraded(v.Id) ? "  ·  upgraded" : ""), StepVehicle);
        }
        void RenderCampaignEvent()
        {
            var e = CampaignData.Find(campaignEvent); if (e == null) { BackPage(); return; }
            bool available = Campaign.Available(e) || Campaign.Testing;
            var list = EventVehicles(e); PickVehicle(list);
            var lines = new System.Collections.Generic.List<string> {
                $"Chapter {e.Chapter}  ·  {e.CourseTitle}", e.KindLabel, "Conditions: " + e.Conditions, "Entry: " + e.Entry, "Pays: " + Campaign.PayText(e) };
            if (e.Timed) lines.Add(Campaign.TargetsText(e));
            if (e.Kind == CampaignEventKind.SpeedTrap) lines.Add($"Standing start {DisplayUnits.Distance(e.RunUp)} before the trap; {e.TimeLimit:0} s to reach it.");
            if (e.Kind == CampaignEventKind.Jump) lines.Add($"Start {DisplayUnits.Distance(e.RunUp)} before the jump; jump as often as you like in {e.TimeLimit:0} s: your best counts. Reset takes you back to the run-up; Pause > End event keeps your best. Land it on your wheels to score.");
            if (e.Kind == CampaignEventKind.Smash) lines.Add($"Leave the road for the fence line beside it: {e.TimeLimit:0} s to smash as many different props as you can.");
            if (e.Kind == CampaignEventKind.TimeTrial) lines.Add("Flying start: the lap clock starts at the START line.");
            lines.Add(e.Kind == CampaignEventKind.Race ? "Pass: finish in the top three" : "Pass: bronze or better");
            if (e.Prize != null) lines.Add($"Win it: the {VehicleProfile.Find(e.Prize).Name}" + (e.Final ? $"; top three opens chapter {e.Chapter + 1}" : ""));
            else if (e.Final && e.Chapter < CampaignData.Chapters.Length) lines.Add($"Top three opens chapter {e.Chapter + 1}" + (e.Bonus >= 4000 ? $"; winning pays a {Campaign.Money(e.Bonus)} bonus" : ""));
            if (e.Final) lines.Add("Passing it also opens the " + string.Join(", ", CampaignData.CupsAfter(e.Chapter).Select(c => c.Name)));
            lines.Add(Campaign.BestText(e));
            if (list.Length > 0) lines.Add("Your " + VehicleProfile.Find(campaignVehicle).Name + ": " + Campaign.UpgradeSummary(campaignVehicle));
            if (!Campaign.Available(e)) lines.Add("LOCKED: " + Campaign.EventLock(e) + (Campaign.Testing ? " (Testing mode: can be run, nothing is saved)" : ""));
            ClearCore(e.Name.ToUpperInvariant(), string.Join("\n", lines));
            details.fontSize = 18; details.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 24 * lines.Count + 8;
            VehicleStepper(0, list, "No owned vehicle fits this event");
            Row(1, "start-event", "START EVENT", () => flow.StartCampaignEvent(e, campaignVehicle));
            var startColors = buttons[1].colors; startColors.normalColor = new(.1f, .38f, .35f); buttons[1].colors = startColors; buttons[1].GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 54;
            buttons[1].interactable = available && list.Length > 0;
            Row(2, "event-back", "Back", () => BackPage());
            EnterCourseView(.4f); ShowCampaignEvent(e);
        }
        // 0.90: a championship's page: its rounds, rules and pay; start (choose the vehicle) / resume / restart / abandon.
        void RenderCampaignCup()
        {
            var c = CampaignData.FindCup(campaignCup); if (c == null) { BackPage(); return; }
            bool open = Campaign.CupOpen(c); var progress = Campaign.ActiveCup(c);
            var lines = new System.Collections.Generic.List<string> { $"Championship after chapter {c.AfterChapter}  ·  {c.Rounds.Length} races  ·  {new[] { "Easy", "Normal", "Hard" }[c.Difficulty]} rivals" };
            for (int i = 0; i < c.Rounds.Length; i++) { var r = c.Rounds[i]; string done = progress != null && i < progress.races.Count ? $"   ✓ {(progress.races[i].places[0] > 0 ? Campaign.Ordinal(progress.races[i].places[0]) : "DNF")}" : progress != null && i == progress.races.Count ? "   ‹ next" : ""; lines.Add($"{i + 1}. {RacePlaylists.Titles[r.Course].Replace(" - ", " — ")} · {r.Laps} laps · {r.Conditions}{done}"); }
            lines.Add("Rivals: " + string.Join(", ", c.Rivals.Select((v, i) => CampaignData.RivalNames[i] + " (" + VehicleProfile.Find(v).Name + ")")));
            lines.Add("Points per race: 10 / 7 / 5 / 3 / 2 / 1; ties: most wins, then the better last race.");
            lines.Add($"Pays by final position: 1st {Campaign.Money(c.Pay)}, then " + string.Join(", ", Enumerable.Range(1, 5).Select(i => Campaign.Money(Mathf.RoundToInt(c.Pay * CampaignData.PlaceShare[i] / 10f) * 10))) + $" · first win +{Campaign.Money(c.Bonus)} and the trophy" + (c.Grand ? " · the campaign's final event" : ""));
            lines.Add("One vehicle for the whole championship (with its upgrades). No retries: a race left early counts as did not finish.");
            lines.Add(CupBest(c));
            if (!open) lines.Add("LOCKED: " + c.Opens);
            if (Campaign.Testing) lines.Add("Testing mode is on: championships need it off (nothing is saved while it is on).");
            ClearCore(c.Name.ToUpperInvariant(), string.Join("\n", lines));
            details.fontSize = 17; details.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 22 * lines.Count + 30;
            int n = 0;
            if (progress != null)
            {
                Row(n++, "cup-resume", $"RESUME   ·   round {progress.races.Count + 1} of {c.Rounds.Length}   ·   {VehicleProfile.Find(progress.vehicle).Name}", () => flow.StartCupRound(c));
                var colors = buttons[0].colors; colors.normalColor = new(.1f, .38f, .35f); buttons[0].colors = colors; buttons[0].GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 54;
                if (progress.races.Count > 0)
                {
                    TableRow(n++, "cup-header", new[] { "Pos", "Driver", "Points", "Wins" }, new[] { .1f, .5f, .2f, .2f }, () => { }); int pos = 0;
                    foreach (var s in Campaign.Standings(progress)) { pos++; TableRow(n++, "cup-standing-" + pos, new[] { pos.ToString(), Campaign.DriverName(s.driver), s.points.ToString(), s.wins.ToString() }, new[] { .1f, .5f, .2f, .2f }, () => { }, s.driver == 0); }
                }
                Row(n++, "cup-restart", "Restart the championship…", () => Confirm("RESTART THE " + c.Name.ToUpperInvariant() + "?", "The points so far are cleared and it starts again from round 1 (you can choose the vehicle again).", () => { Campaign.AbandonCup(c); Show(); }, "RESTART"));
                Row(n++, "cup-abandon", "Abandon it…", () => Confirm("ABANDON THE " + c.Name.ToUpperInvariant() + "?", "The points so far are cleared. You can start it again at any time.", () => { Campaign.AbandonCup(c); Show(); }, "ABANDON"));
            }
            else
            {
                var list = CupVehicles(c); PickVehicle(list);
                VehicleStepper(n++, list, "No owned vehicle fits this championship");
                Row(n, "cup-start", "START CHAMPIONSHIP", () => { Campaign.StartCup(c, campaignVehicle); flow.StartCupRound(c); });
                var colors = buttons[n].colors; colors.normalColor = new(.1f, .38f, .35f); buttons[n].colors = colors; buttons[n].GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 54;
                buttons[n++].interactable = open && list.Length > 0 && !Campaign.Testing;
            }
            Row(n, "cup-back", "Back", () => BackPage());
            EnterCourseView(.4f); ShowCampaignCup(c);
        }
        // Results of a campaign run: place or medal, money earned, bonus, anything unlocked; Continue returns to the campaign.
        void RenderCampaignResults()
        {
            var e = CampaignRun.Active; var o = CampaignRun.Last;
            if (e.Cup != null) { RenderCupResults(e, o); return; }
            var lines = new System.Collections.Generic.List<string>();
            if (o == null) lines.Add("Result pending");
            else
            {
                lines.Add(o.Headline);
                if (!o.Debug)
                {
                    lines.Add($"Earned {Campaign.Money(o.Pay)}" + (o.Replay ? "  (replay: half pay)" : ""));
                    if (o.Bonus > 0) lines.Add($"First {(e.Kind == CampaignEventKind.Race ? "win" : "gold")} bonus: +{Campaign.Money(o.Bonus)}");
                    foreach (var u in o.Unlocked) lines.Add("Unlocked: " + u);
                    if (CampaignRun.OpenedCourse != null) lines.Add("Now open in Race: " + CampaignRun.OpenedCourse);
                    lines.Add(o.Passed ? (e.Final ? "Chapter passed" : "Passed") : e.Kind == CampaignEventKind.Race ? "Not passed: finish in the top three to go on" : "Not passed: earn bronze or better to go on");
                    lines.Add(o.Testing ? "Testing mode: nothing was saved or paid." : "Money: " + Campaign.Money(Campaign.Current.money) + (o.Saved ? "" : "   (" + (Campaign.Error ?? "not saved") + ")"));
                }
            }
            // 0.90 Part B: the first money earned and the first thing opened each get a hint
            if (o != null && !o.Debug && !o.Testing) { if (o.Unlocked.Count > 0 || CampaignRun.OpenedCourse != null) Hints.Unlocked(); if (o.Pay + o.Bonus > 0) Hints.Money(); }
            ClearCore("CAMPAIGN RESULT", e.Name + "  ·  " + e.CourseTitle);
            var head = Label("Campaign payout", content, 22, 0); laterLayouts.Add(head.gameObject); head.transform.SetSiblingIndex(1);
            head.text = string.Join("\n", lines); head.color = new(.3f, .95f, .81f); head.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 28 * lines.Count + 6;
            int n = 0;
            Row(n++, "campaign-continue", "CONTINUE", flow.QuitRace);
            var colors = buttons[0].colors; colors.normalColor = new(.1f, .38f, .35f); buttons[0].colors = colors;
            Row(n++, "campaign-retry", "Retry event", flow.RetryCampaignEvent);
            if (e.Kind == CampaignEventKind.Race)
            {
                TableRow(n++, "header", new[] { "Place", "Driver", "Time", "Status" }, new[] { .1f, .32f, .30f, .28f }, () => { }); int rank = 0;
                foreach (var r in flow.Race.Ordered(true)) { int place = ++rank; TableRow(n++, "standing-" + place, new[] { place.ToString(), r.IsAi ? r.Car.name + " · " + VehicleProfile.Find(r.Car.GetComponent<VehicleConfiguration>().profileId).Name : "You", r.Dnf ? "—" : RaceHud.FormatTime(r.ClassifiedTime(flow.Race.Clock)), r.Dnf ? "DNF" : r.Estimated ? "Estimated" : "Measured" }, new[] { .1f, .32f, .30f, .28f }, () => { }, !r.IsAi); }
            }
            else if (e.Kind == CampaignEventKind.TimeTrial && flow.Race.Progress.MissedGates > 0) Row(n++, "penalties", "Penalty Details · " + flow.Race.Progress.MissedGates + " missed / +" + flow.Race.Progress.PenaltySeconds.ToString("0") + "s", () => Navigate("penalties"));
        }
        // 0.90: a championship round: the race, then the standings table; Next race (or the final standings and payout).
        void RenderCupResults(CampaignEvent e, Campaign.Outcome o)
        {
            var cup = e.Cup; var progress = Campaign.CupResult(cup.Id);
            if (page == "champion") { RenderChampion(cup, progress); return; }
            var lines = new System.Collections.Generic.List<string>();
            if (o == null) lines.Add("Result pending");
            else if (o.Debug) lines.Add("DEBUG RUN — not counted; this round can be run again");
            else if (o.Testing) lines.Add("Testing mode: nothing was saved.");
            else
            {
                lines.Add($"Round {e.Round + 1} of {cup.Rounds.Length}:  " + o.Headline + (o.Places != null && o.Places[0] > 0 ? $"  ·  +{CampaignData.CupPoints[Mathf.Min(o.Places[0], 6) - 1]} points" : "  ·  no points"));
                if (o.CupFinished)
                {
                    lines.Add($"FINAL: {Campaign.Ordinal(o.CupPosition)} overall" + (o.Won ? "  ·  ★ TROPHY" : ""));
                    lines.Add($"Earned {Campaign.Money(o.Pay)}" + (o.Replay ? "  (replay: half pay)" : "") + (o.Bonus > 0 ? $"  ·  first win bonus +{Campaign.Money(o.Bonus)}" : ""));
                    foreach (var u in o.Unlocked) lines.Add("Unlocked: " + u);
                    lines.Add("Money: " + Campaign.Money(Campaign.Current.money) + (o.Saved ? "" : "   (" + (Campaign.Error ?? "not saved") + ")"));
                }
                else lines.Add("Progress saved: resume from the campaign screen at any time.");
            }
            if (o != null && o.CupFinished && o.Pay + o.Bonus > 0) Hints.Money();
            ClearCore(cup.Name.ToUpperInvariant(), e.CourseTitle + "  ·  " + e.Conditions);
            var head = Label("Championship result", content, 22, 0); laterLayouts.Add(head.gameObject); head.transform.SetSiblingIndex(1);
            head.text = string.Join("\n", lines); head.color = new(.3f, .95f, .81f); head.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 28 * lines.Count + 6;
            int n = 0;
            bool more = progress != null && progress.active && progress.races.Count < cup.Rounds.Length;
            bool champion = o != null && o.CupFinished && cup.Grand && o.Won;
            if (champion) Row(n++, "champion", "★  CHAMPION OF WOODSTOCK  ★", () => Navigate("champion"));
            else if (more) Row(n++, "next-race", $"NEXT RACE   ·   round {progress.races.Count + 1}: {RacePlaylists.Titles[cup.Rounds[progress.races.Count].Course].Replace(" - ", " — ")}", flow.NextCupRace);
            Row(n++, "campaign-continue", more ? "Campaign screen (resume later)" : "CONTINUE", flow.QuitRace);
            var colors = buttons[0].colors; colors.normalColor = new(.1f, .38f, .35f); buttons[0].colors = colors; buttons[0].GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 54;
            var w = new[] { .1f, .36f, .27f, .27f };
            TableRow(n++, "header", new[] { "Place", "Driver", "Time", "Status" }, w, () => { }); int rank = 0;
            foreach (var r in flow.Race.Ordered(true)) { int place = ++rank; TableRow(n++, "standing-" + place, new[] { place.ToString(), r.IsAi ? r.Name + " · " + VehicleProfile.Find(r.Car.GetComponent<VehicleConfiguration>().profileId).Name : "You", r.Dnf ? "—" : RaceHud.FormatTime(r.ClassifiedTime(flow.Race.Clock)), r.Dnf ? "DNF" : r.Estimated ? "Estimated" : "Measured" }, w, () => { }, !r.IsAi); }
            if (progress != null && progress.races.Count > 0) CupTable(ref n, progress, o != null && o.CupFinished ? "FINAL STANDINGS" : "CHAMPIONSHIP STANDINGS");
        }
        void CupTable(ref int n, Campaign.Cup progress, string heading)
        {
            var w = new[] { .1f, .5f, .2f, .2f };
            TableRow(n++, "cup-header", new[] { "Pos", heading, "Points", "Wins" }, w, () => { }); int pos = 0;
            foreach (var s in Campaign.Standings(progress)) { pos++; TableRow(n++, "cup-standing-" + pos, new[] { pos.ToString(), Campaign.DriverName(s.driver), s.points.ToString(), s.wins.ToString() }, w, () => { }, s.driver == 0); }
        }
        // 0.90: the ending: the poster, the final table, time racing and money earned; the campaign stays replayable.
        GameObject championArt;
        void RenderChampion(CampaignCup cup, Campaign.Cup progress)
        {
            var st = Campaign.Current; var t = System.TimeSpan.FromSeconds(st.secondsRacing);
            ClearCore("CHAMPION OF WOODSTOCK", $"You won the {cup.Name}: the campaign is complete.\nTime racing in the campaign: {(int)t.TotalHours}h {t.Minutes:00}m  ·  Money earned: {Campaign.Money(st.earned)}\nEverything stays open to replay: events, championships and the Shop.");
            details.fontSize = 20; details.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 96;
            var poster = Resources.Load<Texture2D>("LoadingPoster");
            if (poster)
            {
                var r = Rect("Champion poster", content); laterLayouts.Add(r.gameObject); r.SetSiblingIndex(1);
                r.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 260;
                var art = Rect("Poster", r); art.anchorMin = art.anchorMax = art.pivot = new(.5f, .5f); art.sizeDelta = new(260 * poster.width / (float)poster.height, 260);
                art.gameObject.AddComponent<UnityEngine.UI.RawImage>().texture = poster; art.GetComponent<UnityEngine.UI.RawImage>().raycastTarget = false;
            }
            int n = 0;
            Row(n++, "campaign-continue", "CONTINUE", flow.QuitRace);
            var colors = buttons[0].colors; colors.normalColor = new(.1f, .38f, .35f); buttons[0].colors = colors;
            if (progress != null) CupTable(ref n, progress, "FINAL STANDINGS");
        }
    }
}
