using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Racer
{
    // 0.89 campaign, round 1: the campaign save and its rules. Its own file (campaign-v1.json beside the other saves),
    // written atomically and versioned; losing or resetting it never touches records, acorns, settings or Free Roam.
    // No file = a new player: money 0, the two starters, Street Loop - Forward, chapter 1 (nothing is written until the
    // first event or purchase). Testing ("Unlock everything" in Settings) opens every course and vehicle in Race and Free
    // Roam and never writes this save (the F6 debug entries do write it: they are explicit campaign tools).
    public static class Campaign
    {
        public const string File = "campaign-v1.json";
        [Serializable] public sealed class Result
        {
            public string id; public int runs; public bool passed, won, bonusPaid;
            public int bestPlace, field, bestMedal; public double bestTime; public float bestScore;
        }
        [Serializable] public sealed class State
        {
            public int version = 1; public int money; public int chapter = 1;
            public List<string> owned = new(), courses = new(); public List<Result> results = new();
            public string started;
        }
        public static State Current { get; private set; } = NewState();
        public static bool Exists { get; private set; }
        public static string Error { get; private set; }
        public static bool Testing;
        // The event highlighted on the campaign screen (the F6 "mark event won" target).
        public static string Selected;
        static string path; static bool readFailed;

        static State NewState() => new() { owned = CampaignData.Starters.ToList(), courses = new() { RacePlaylists.Scenes[0] } };
        public static void Load(string root)
        {
            path = Path.Combine(root, File); Error = null; Exists = false; readFailed = false; Current = NewState();
            try
            {
                if (!System.IO.File.Exists(path)) return;
                var data = JsonUtility.FromJson<State>(System.IO.File.ReadAllText(path));
                if (data == null || data.version != 1) throw new IOException("unknown campaign save version");
                data.owned ??= new(); data.courses ??= new(); data.results ??= new();
                foreach (var s in CampaignData.Starters) if (!data.owned.Contains(s)) data.owned.Add(s);
                if (!data.courses.Contains(RacePlaylists.Scenes[0])) data.courses.Add(RacePlaylists.Scenes[0]);
                data.chapter = Mathf.Clamp(data.chapter, 1, CampaignData.Chapters.Length); data.money = Mathf.Max(0, data.money);
                Current = data; Exists = true;
            }
            catch (Exception e) { readFailed = true; Exists = true; Error = "Campaign save could not be read (" + e.Message + "); it is left as it is. New Campaign replaces it."; }
        }
        // force: the F6 debug entries write even while Testing is on.
        public static bool Save(bool force = false)
        {
            if ((Testing && !force) || path == null || readFailed) return false;
            try { if (string.IsNullOrEmpty(Current.started)) Current.started = DateTime.UtcNow.ToString("o"); AtomicSave.Write(path, JsonUtility.ToJson(Current, true)); Exists = true; Error = null; return true; }
            catch (Exception e) { Error = "Campaign could not be saved: " + e.Message; return false; }
        }
        // New Campaign (after its confirmation) and the F6 reset: a fresh campaign, written at once over the old one.
        public static void Reset() { Current = NewState(); Selected = null; readFailed = false; Error = null; if (path != null) { try { AtomicSave.Write(path, JsonUtility.ToJson(Current, true)); Exists = true; } catch (Exception e) { Error = "Campaign could not be saved: " + e.Message; } } }
        public static bool HasProgress => Exists && (readFailed || Current.results.Count > 0 || Current.money > 0 || Current.owned.Count > CampaignData.Starters.Length);

        // ---------- what is open ----------
        public static bool Owns(string id) => Current.owned.Contains(id);
        // A player vehicle locked by the campaign (the mower is governed by the acorn reward instead).
        public static bool VehicleLocked(VehicleProfile p) => p != null && !Testing && !p.Reward && !Owns(p.Id);
        public static bool CourseOpen(int course) => Testing || (course >= 0 && course < RacePlaylists.Scenes.Length && Current.courses.Contains(RacePlaylists.Scenes[course]));
        public static bool CourseOpen(string scene) => CourseOpen(Array.IndexOf(RacePlaylists.Scenes, scene));
        public static int FirstOpenCourse => Enumerable.Range(0, RacePlaylists.Scenes.Length).FirstOrDefault(CourseOpen);
        public static bool ChapterOpen(int chapter) => chapter <= Current.chapter;
        public static Result ResultOf(string id) => Current.results.FirstOrDefault(r => r.id == id);
        public static bool Passed(CampaignEvent e) => ResultOf(e.Id)?.passed == true;
        public static bool Available(CampaignEvent e)
        {
            if (!ChapterOpen(e.Chapter)) return false;
            var list = CampaignData.InChapter(e.Chapter); int i = Array.IndexOf(list, e);
            return i <= 0 || Passed(list[i - 1]);
        }
        public static CampaignEvent Next => CampaignData.Events.FirstOrDefault(e => Available(e) && !Passed(e)) ?? CampaignData.Events.LastOrDefault(Available);
        public static string EventLock(CampaignEvent e)
        {
            if (!ChapterOpen(e.Chapter)) return "Chapter " + e.Chapter + " is locked";
            var list = CampaignData.InChapter(e.Chapter); int i = Array.IndexOf(list, e);
            return i > 0 && !Passed(list[i - 1]) ? $"Pass \"{list[i - 1].Name}\" first" : "";
        }
        public static string CourseHowTo(int course)
        {
            var e = CampaignData.Events.FirstOrDefault(x => x.Course == course);
            if (e != null) return $"Campaign: reach it in \"{e.Name}\" (chapter {e.Chapter})";
            var c = CampaignData.Chapters.FirstOrDefault(x => x.Courses.Contains(course));
            return c != null ? $"Campaign: reach it in chapter {c.Number}, {c.Name}" : "Campaign";
        }
        public static int Price(string id) => CampaignData.Prices.FirstOrDefault(p => p.id == id).price;
        public static int PriceChapter(string id) => CampaignData.Prices.FirstOrDefault(p => p.id == id).chapter;
        public static CampaignEvent PrizeEvent(string id) => CampaignData.Events.FirstOrDefault(e => e.Prize == id);
        // One line: how to get a vehicle the player does not own.
        public static string HowToGet(VehicleProfile p)
        {
            if (p.Reward) return VehicleUnlocks.LockedText;
            int price = Price(p.Id);
            if (price > 0) return ChapterOpen(PriceChapter(p.Id)) ? $"Buy it in the Shop for {Money(price)}" : $"In the Shop from chapter {PriceChapter(p.Id)}: {Money(price)}";
            var prize = PrizeEvent(p.Id); if (prize != null) return $"Prize: win \"{prize.Name}\" (campaign chapter {prize.Chapter})";
            var later = CampaignData.LaterPrizes.FirstOrDefault(l => l.id == p.Id); if (later.id != null) return "Prize from " + later.from;
            return "Campaign";
        }
        public static string Money(int dollars) => "$" + dollars.ToString("N0", System.Globalization.CultureInfo.InvariantCulture);

        // ---------- the shop ----------
        public static bool CanBuy(VehicleProfile p, out string why)
        {
            why = "";
            if (Testing) { why = "Testing mode is on: buying is off (everything is unlocked)."; return false; }
            if (Owns(p.Id)) { why = "Owned"; return false; }
            int price = Price(p.Id); if (price <= 0) { why = HowToGet(p); return false; }
            if (!ChapterOpen(PriceChapter(p.Id))) { why = $"Locked: in the Shop from chapter {PriceChapter(p.Id)}"; return false; }
            if (Current.money < price) { why = $"Not enough money: {Money(price)} needed, you have {Money(Current.money)}"; return false; }
            return true;
        }
        public static bool Buy(VehicleProfile p)
        {
            if (!CanBuy(p, out _)) return false;
            var keep = JsonUtility.ToJson(Current);
            Current.money -= Price(p.Id); Current.owned.Add(p.Id);
            if (Save()) return true;
            Current = JsonUtility.FromJson<State>(keep); return false;
        }

        // ---------- results and payouts ----------
        public sealed class Outcome
        {
            public CampaignEvent Event; public string Vehicle;
            public int Place, Field, Medal; public bool Dnf, Passed, Won, Replay, Saved, Testing, Debug; public double Time; public float Score;
            public int Pay, Bonus; public readonly List<string> Unlocked = new();
            public string Headline => Debug ? "DEBUG RUN — no result or payout" : Event.Kind == CampaignEventKind.Race
                ? (Dnf ? "Did not finish" : $"{Ordinal(Place)} of {Field}")
                : (Dnf ? "No result" : new[] { "No medal", "BRONZE", "SILVER", "GOLD" }[Medal] + "  ·  " + Measure(Event, Event.Kind == CampaignEventKind.TimeTrial ? (float)Time : Score));
        }
        public static string Ordinal(int n) => n + (n % 100 is >= 11 and <= 13 ? "th" : (n % 10) switch { 1 => "st", 2 => "nd", 3 => "rd", _ => "th" });
        public static string Measure(CampaignEvent e, float value) => e.Kind == CampaignEventKind.TimeTrial ? RaceHud.FormatTime(value) : e.Kind == CampaignEventKind.SpeedTrap ? DisplayUnits.Speed(value) : RaceHud.FormatTime(value);
        public static int MedalFor(CampaignEvent e, float value)
        {
            if (e.Targets == null || e.Targets.Length != 3 || value <= 0) return 0;
            if (e.Kind == CampaignEventKind.TimeTrial) return value <= e.Targets[2] ? 3 : value <= e.Targets[1] ? 2 : value <= e.Targets[0] ? 1 : 0;
            return value >= e.Targets[2] ? 3 : value >= e.Targets[1] ? 2 : value >= e.Targets[0] ? 1 : 0;
        }
        public static string TargetsText(CampaignEvent e) => e.Targets == null ? "" : $"Bronze {Measure(e, e.Targets[0])} · silver {Measure(e, e.Targets[1])} · gold {Measure(e, e.Targets[2])}";
        public static string PayText(CampaignEvent e) => e.Kind == CampaignEventKind.Race
            ? $"1st {Money(e.Pay)}, then {string.Join(", ", Enumerable.Range(1, e.Rivals.Length).Select(i => Money(Share(e.Pay, CampaignData.PlaceShare[Mathf.Min(i, CampaignData.PlaceShare.Length - 1)]))))}" + $" · first win +{Money(e.Bonus)}"
            : $"Gold {Money(e.Pay)}, silver {Money(Share(e.Pay, CampaignData.MedalShare[2]))}, bronze {Money(Share(e.Pay, CampaignData.MedalShare[1]))} · first gold +{Money(e.Bonus)}";
        static int Share(int pay, float share) => Mathf.RoundToInt(pay * share / 10f) * 10;
        public static string BestText(CampaignEvent e)
        {
            var r = ResultOf(e.Id); if (r == null || r.runs == 0) return "Not run yet";
            if (e.Kind == CampaignEventKind.Race) return r.bestPlace > 0 ? $"Best {Ordinal(r.bestPlace)} of {r.field}" + (r.bestTime > 0 ? " · " + RaceHud.FormatTime(r.bestTime) : "") : "Best: did not finish";
            return r.bestMedal > 0 || r.bestScore > 0 || r.bestTime > 0 ? new[] { "No medal", "Bronze", "Silver", "Gold" }[r.bestMedal] + " · " + Measure(e, e.Kind == CampaignEventKind.TimeTrial ? (float)r.bestTime : r.bestScore) : "Best: no result";
        }

        // Applies a finished run: money, the event's best, passing, the first-win bonus, the prize, the next event and
        // chapter. Called once per run. While Testing it is worked out and shown, but nothing is saved.
        public static Outcome Commit(Outcome o)
        {
            var e = o.Event; o.Testing = Testing;
            if (o.Debug) return o;
            if (e.Kind == CampaignEventKind.Race) { o.Passed = !o.Dnf && o.Place <= 3; o.Won = !o.Dnf && o.Place == 1; }
            else { o.Medal = o.Dnf ? 0 : MedalFor(e, e.Kind == CampaignEventKind.TimeTrial ? (float)o.Time : o.Score); o.Passed = o.Medal >= 1; o.Won = o.Medal == 3; }
            var keep = JsonUtility.ToJson(Current);
            var r = ResultOf(e.Id); if (r == null) { r = new Result { id = e.Id }; Current.results.Add(r); }
            o.Replay = r.runs > 0;
            float share = o.Dnf ? 0 : e.Kind == CampaignEventKind.Race ? CampaignData.PlaceShare[Mathf.Clamp(o.Place - 1, 0, CampaignData.PlaceShare.Length - 1)] : CampaignData.MedalShare[o.Medal];
            o.Pay = Share(e.Pay, share * (o.Replay ? CampaignData.ReplayShare : 1));
            if (o.Won && !r.bonusPaid) { o.Bonus = e.Bonus; r.bonusPaid = true; }
            int nextBefore = CampaignData.Events.Count(Available); int chapterBefore = Current.chapter;
            r.runs++; r.passed |= o.Passed; r.won |= o.Won;
            if (e.Kind == CampaignEventKind.Race)
            {
                if (!o.Dnf && (r.bestPlace == 0 || o.Place < r.bestPlace || (o.Place == r.bestPlace && (r.bestTime <= 0 || o.Time < r.bestTime)))) { r.bestPlace = o.Place; r.field = o.Field; r.bestTime = o.Time; }
            }
            else if (!o.Dnf)
            {
                r.bestMedal = Mathf.Max(r.bestMedal, o.Medal);
                if (e.Kind == CampaignEventKind.TimeTrial) { if (r.bestTime <= 0 || o.Time < r.bestTime) r.bestTime = o.Time; }
                else if (o.Score > r.bestScore) r.bestScore = o.Score;
            }
            Current.money += o.Pay + o.Bonus;
            if (o.Won && e.Prize != null && !Owns(e.Prize)) { Current.owned.Add(e.Prize); o.Unlocked.Add("Vehicle: " + VehicleProfile.Find(e.Prize).Name); }
            if (o.Passed && e.Final && Current.chapter == e.Chapter && e.Chapter < CampaignData.Chapters.Length) Current.chapter = e.Chapter + 1;
            if (Current.chapter > chapterBefore) { var c = CampaignData.Chapters[Current.chapter - 1]; o.Unlocked.Add($"Chapter {c.Number}: {c.Name}"); }
            if (CampaignData.Events.Count(Available) > nextBefore) { var n = CampaignData.Events.FirstOrDefault(x => Available(x) && ResultOf(x.Id) == null); if (n != null) o.Unlocked.Add("Next event: " + n.Name); }
            if (Testing) { Current = JsonUtility.FromJson<State>(keep); return o; }
            o.Saved = Save();
            return o;
        }
        // The campaign puts the player on a course: from then on it is open in Race and the playlists.
        public static string OpenCourse(int course)
        {
            string scene = RacePlaylists.Scenes[course];
            if (Testing || Current.courses.Contains(scene)) return null;
            Current.courses.Add(scene); Save(); return RacePlaylists.Titles[course];
        }

        // ---------- F6 debug entries (debug mode only; they write even while Testing is on) ----------
        public static void DebugAddMoney(int dollars) { Current.money += dollars; Save(true); }
        public static void DebugUnlockAll()
        {
            foreach (var p in VehicleProfile.All) if (!p.Reward && !Owns(p.Id)) Current.owned.Add(p.Id);
            foreach (var s in RacePlaylists.Scenes) if (!Current.courses.Contains(s)) Current.courses.Add(s);
            Current.chapter = CampaignData.Chapters.Length;
            foreach (var e in CampaignData.Events) { var r = ResultOf(e.Id); if (r == null) Current.results.Add(r = new Result { id = e.Id }); r.passed = true; }
            Save(true);
        }
        public static string DebugMarkWon()
        {
            var e = CampaignData.Find(Selected) ?? Next; if (e == null) return "No event";
            var r = ResultOf(e.Id); if (r == null) Current.results.Add(r = new Result { id = e.Id });
            r.passed = r.won = true; r.runs = Mathf.Max(1, r.runs);
            if (e.Kind == CampaignEventKind.Race) { r.bestPlace = 1; r.field = e.Rivals.Length + 1; } else r.bestMedal = 3;
            if (e.Prize != null && !Owns(e.Prize)) Current.owned.Add(e.Prize);
            string scene = RacePlaylists.Scenes[e.Course]; if (!Current.courses.Contains(scene)) Current.courses.Add(scene);
            if (e.Final && Current.chapter == e.Chapter && e.Chapter < CampaignData.Chapters.Length) Current.chapter++;
            Save(true); return e.Name;
        }
    }
}
