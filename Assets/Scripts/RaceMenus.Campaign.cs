using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Racer
{
    // 0.89 campaign, round 1: the campaign screen (main menu > CAMPAIGN), the event page and the results-and-payout page.
    // Campaign screen: money at the top; Continue / Shop / New Campaign / Back; the chapters down the left of the list
    // (locked ones say what opens them) and the selected chapter's events beside them; the course map panel (as on the
    // Tracks screen) shows the highlighted event's route, with its type, course, conditions, entry rule, payout and best.
    public sealed partial class RaceMenus
    {
        int campaignChapter; string campaignEvent, campaignVehicle;
        // Tracks opened from the Free Roam page picks where Free Roam starts: the whole world is open there.
        public bool RoamTrackPick => stagePages.TryGetValue(RaceFlow.Stage.Ready, out var from) && from == "roam";
        public void OpenCampaign() { page = "campaign"; pages.Clear(); stagePages[RaceFlow.Stage.Ready] = page; modalConfirm = null; Show(); }
        void ShowCampaignEvent(CampaignEvent e)
        {
            if (e == null || !courseView) return;
            ShowCourseOnMap(e.Course); Campaign.Selected = e.Id;
            courseCaption.fontSize = 16;
            courseCaption.text = $"{e.Name}  ·  {e.CourseTitle}\n{e.KindLabel}  ·  {e.Conditions}\n{e.Entry}  ·  {Campaign.PayText(e)}\n{Campaign.BestText(e)}";
        }
        static string ShortKind(CampaignEvent e) => e.Kind switch { CampaignEventKind.Race => $"Race, {e.Laps} laps", CampaignEventKind.TimeTrial => "Time trial", _ => "Speed trap" };
        // the event row's best: place or medal only (the full best is in the caption beside the map)
        static string ShortBest(CampaignEvent e) { var r = Campaign.ResultOf(e.Id); if (r == null || r.runs == 0) return "not run yet"; return e.Kind == CampaignEventKind.Race ? (r.bestPlace > 0 ? "best " + Campaign.Ordinal(r.bestPlace) : "best: DNF") : "best: " + new[] { "no medal", "bronze", "silver", "gold" }[r.bestMedal]; }
        void RenderCampaign()
        {
            var st = Campaign.Current; var chapters = CampaignData.Chapters;
            if (campaignChapter < 1 || campaignChapter > chapters.Length) campaignChapter = Mathf.Clamp(Campaign.Next?.Chapter ?? st.chapter, 1, chapters.Length);
            int passed = CampaignData.Events.Count(Campaign.Passed);
            ClearCore("CAMPAIGN   ·   " + Campaign.Money(st.money), Campaign.Error ?? (Campaign.Testing
                ? "Testing mode is on (Settings): events can be run, but nothing is saved or paid."
                : $"Chapter {st.chapter} of {chapters.Length}  ·  {passed} of {CampaignData.Events.Length} events passed  ·  {st.owned.Count} vehicles owned"));
            details.fontSize = 18; details.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 28;
            var actions = LaterGroup("Campaign actions", content, true); actions.SetSiblingIndex(1); int n = 0;
            void ActionButton(string id, string label, System.Action action) { Row(n, id, label, action); var t = buttons[n].GetComponentInChildren<UnityEngine.UI.Text>(true); t.alignment = TextAnchor.MiddleCenter; t.fontSize = 18; buttons[n++].transform.SetParent(actions, false); }
            var next = Campaign.Next;
            ActionButton("continue", Campaign.Exists ? "Continue" : "Start", () => { if (next != null) { campaignChapter = next.Chapter; campaignEvent = next.Id; Navigate("campaign-event"); } });
            ActionButton("shop", "Shop", OpenCampaignShop);
            ActionButton("new-campaign", "New Campaign", () => { if (Campaign.HasProgress) Confirm("START A NEW CAMPAIGN?", "This replaces your campaign: money, vehicles bought or won, results and opened courses.\nRecords, acorns, settings and Free Roam are not touched.", NewCampaign, "NEW CAMPAIGN"); else NewCampaign(); });
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
            if (events.Length == 0)
            {
                var c = chapters[campaignChapter - 1];
                Row(n, "chapter-empty", Campaign.ChapterOpen(c.Number) ? $"{c.Name}: its events arrive in the next update." : $"{c.Name} is locked.\n{c.Opens}.", () => { });
                buttons[n].GetComponentInChildren<UnityEngine.UI.Text>(true).fontSize = 18; buttons[n].GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 70; buttons[n].transform.SetParent(right, false); rightPaneButtons.Add(buttons[n++]);
                EnterCourseView(.4f); ShowCourseOnMap(c.Courses[0]); courseCaption.fontSize = 18; courseCaption.text = $"Chapter {c.Number}: {c.Name}\n{(Campaign.ChapterOpen(c.Number) ? "Open" : "Locked: " + c.Opens)}";
            }
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
            if (events.Length > 0)
            {
                EnterCourseView(.4f);
                var shownEvent = events.FirstOrDefault(x => x.Id == Campaign.Selected) ?? events.FirstOrDefault(x => x == next) ?? events[0];
                ShowCampaignEvent(shownEvent);
            }
        }
        void NewCampaign() { Campaign.Reset(); campaignChapter = 1; campaignEvent = null; flow.Notify(Campaign.Error ?? "New campaign started", 4); Show(); }
        void OpenCampaignShop() { shopFromCampaign = true; flow.OpenGarage(); page = "shop"; pages.Clear(); Show(); }
        // The vehicles the player may enter in an event: owned (all of them while Testing), never the mower.
        VehicleProfile[] EventVehicles(CampaignEvent e) => VehicleProfile.All.Where(p => !p.Reward && (Campaign.Testing || Campaign.Owns(p.Id)) && (CarAccess.CourseAllowsCars(RacePlaylists.Scenes[e.Course]) || p.Small)).ToArray();
        void RenderCampaignEvent()
        {
            var e = CampaignData.Find(campaignEvent); if (e == null) { BackPage(); return; }
            bool available = Campaign.Available(e) || Campaign.Testing;
            var list = EventVehicles(e);
            if (list.Length > 0 && !list.Any(p => p.Id == campaignVehicle)) campaignVehicle = list.Any(p => p.Id == flow.Race.vehicle.GetComponent<VehicleConfiguration>().profileId) ? flow.Race.vehicle.GetComponent<VehicleConfiguration>().profileId : list[0].Id;
            var lines = new System.Collections.Generic.List<string> {
                $"Chapter {e.Chapter}  ·  {e.CourseTitle}", e.KindLabel, "Conditions: " + e.Conditions, "Entry: " + e.Entry, "Pays: " + Campaign.PayText(e) };
            if (e.Timed) lines.Add(Campaign.TargetsText(e));
            if (e.Kind == CampaignEventKind.SpeedTrap) lines.Add($"Standing start {DisplayUnits.Distance(e.RunUp)} before the trap; {e.TimeLimit:0} s to reach it.");
            if (e.Kind == CampaignEventKind.TimeTrial) lines.Add("Flying start: the lap clock starts at the START line.");
            lines.Add(e.Kind == CampaignEventKind.Race ? "Pass: finish in the top three" : "Pass: bronze or better");
            if (e.Prize != null) lines.Add($"Win it: the {VehicleProfile.Find(e.Prize).Name}" + (e.Final ? $"; top three opens chapter {e.Chapter + 1}" : ""));
            lines.Add(Campaign.BestText(e));
            if (!Campaign.Available(e)) lines.Add("LOCKED: " + Campaign.EventLock(e) + (Campaign.Testing ? " (Testing mode: can be run, nothing is saved)" : ""));
            ClearCore(e.Name.ToUpperInvariant(), string.Join("\n", lines));
            details.fontSize = 18; details.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 24 * lines.Count + 8;
            int at = System.Array.FindIndex(list, p => p.Id == campaignVehicle);
            void StepVehicle(int d) { if (list.Length == 0) return; campaignVehicle = list[((at < 0 ? 0 : at) + d + list.Length) % list.Length].Id; flow.Click(); Show(); }
            var v = VehicleProfile.Find(campaignVehicle);
            Step(0, "event-vehicle", list.Length == 0 ? "No owned vehicle fits this event" : $"Vehicle: {v.Name}  ({v.Class}, {at + 1} of {list.Length})", StepVehicle);
            Row(1, "start-event", "START EVENT", () => flow.StartCampaignEvent(e, campaignVehicle));
            var startColors = buttons[1].colors; startColors.normalColor = new(.1f, .38f, .35f); buttons[1].colors = startColors; buttons[1].GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 54;
            buttons[1].interactable = available && list.Length > 0;
            Row(2, "event-back", "Back", () => BackPage());
            EnterCourseView(.4f); ShowCampaignEvent(e);
        }
        // Results of a campaign run: place or medal, money earned, bonus, anything unlocked; Continue returns to the campaign.
        void RenderCampaignResults()
        {
            var e = CampaignRun.Active; var o = CampaignRun.Last;
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
    }
}
