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
        // 0.90: version 2 adds upgrades, championships, completion, time and money earned. A version 1 save (0.89) loads and
        // carries on (money, vehicles, results, opened courses kept); it is written as version 2 from the next save. The
        // file keeps its name.
        // 0.92: version 3 adds the vehicles last driven in the campaign (newest first) and grants the chapter-final prize
        // vehicles of finals already passed (refunding a vehicle bought before it became a prize); both said once.
        public const int Version = 3;
        [Serializable] public sealed class Upgrade { public string id; public int[] levels = new int[4]; }
        [Serializable] public sealed class CupRace { public int[] places = new int[6]; public double time; }
        [Serializable] public sealed class Cup
        {
            public string id; public bool active; public string vehicle; public List<CupRace> races = new();
            public int runs, bestPosition; public bool finished, won, bonusPaid;
        }
        [Serializable] public sealed class State
        {
            public int version = Version; public int money; public int chapter = 1;
            public List<string> owned = new(), courses = new(); public List<Result> results = new();
            public string started;
            public List<Upgrade> upgrades = new(); public List<Cup> cups = new();
            public bool complete; public double secondsRacing; public int earned;
            public List<string> driven = new();
        }
        // 0.92 Part D: what loading a save granted (prize vehicles, refunds), shown once on the menu; null when nothing.
        public static string LoadNotice;
        // The player has seen it: written now, so it is not said again.
        public static void NoticeSeen() { LoadNotice = null; Save(); }
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
            path = Path.Combine(root, File); Error = null; Exists = false; readFailed = false; Current = NewState(); LoadNotice = null;
            try
            {
                if (!System.IO.File.Exists(path)) return;
                var data = JsonUtility.FromJson<State>(System.IO.File.ReadAllText(path));
                if (data == null || data.version < 1 || data.version > Version) throw new IOException("unknown campaign save version");
                data.owned ??= new(); data.courses ??= new(); data.results ??= new(); data.upgrades ??= new(); data.cups ??= new(); data.driven ??= new();
                int loadedVersion = data.version;
                foreach (var u in data.upgrades) if (u.levels == null || u.levels.Length != 4) u.levels = new int[4];
                foreach (var c in data.cups) { c.races ??= new(); foreach (var r in c.races) if (r.places == null || r.places.Length != 6) r.places = new int[6]; }
                if (data.version == 1)
                {
                    // 0.89 save: what it has earned so far is its money plus what it spent in the Shop; its racing time is
                    // estimated from its best times (the 0.89 save did not keep it).
                    data.earned = data.money + data.owned.Sum(Price);
                    data.secondsRacing = data.results.Sum(r => r.bestTime);
                    data.version = Version;
                }
                foreach (var s in CampaignData.Starters) if (!data.owned.Contains(s)) data.owned.Add(s);
                if (!data.courses.Contains(RacePlaylists.Scenes[0])) data.courses.Add(RacePlaylists.Scenes[0]);
                data.chapter = Mathf.Clamp(data.chapter, 1, CampaignData.Chapters.Length); data.money = Mathf.Max(0, data.money);
                Current = data; Exists = true;
                // 0.92 Part D: a save from before the prize change: a vehicle bought that is now a prize is refunded (it stays
                // owned), and every prize of a final already passed is granted. Loading never writes: the menu says so once and
                // the save is written when the player has seen it (NoticeSeen), or with the next result.
                var notes = new List<string>();
                if (loadedVersion < 3) foreach (var (id, price) in CampaignData.FormerlySold) if (data.owned.Contains(id)) { data.money += price; notes.Add($"{VehicleProfile.Find(id).Name} is now a prize: the {Money(price)} you paid is refunded"); }
                foreach (var e in CampaignData.Events) if (e.Prize != null && Passed(e) && !data.owned.Contains(e.Prize)) { data.owned.Add(e.Prize); notes.Add($"Prize for passing the {e.Name}: the {VehicleProfile.Find(e.Prize).Name}"); }
                data.version = Version;
                if (notes.Count > 0) LoadNotice = string.Join("\n", notes);
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
            var prize = PrizeEvent(p.Id); if (prize != null) return $"Prize: pass \"{prize.Name}\" (campaign chapter {prize.Chapter})";
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
            // 0.90: the run's racing time (campaign total) and, for a championship round, the six finishing places
            // (index 0 the player, 1-5 the rivals in roster order; 0 = did not finish), whether it ended the championship
            // and the final position.
            public double RaceSeconds; public int[] Places; public bool CupFinished, Forfeit; public int CupPosition;
            // 0.92 Part D: the prize vehicle this run won (shown revealed on the results), or the champion's paint scheme.
            public string PrizeWon; public bool ChampionPaint;
            public string Headline => Debug ? "DEBUG RUN — no result or payout" : Event.Kind == CampaignEventKind.Race
                ? (Dnf ? "Did not finish" : $"{Ordinal(Place)} of {Field}")
                : (Dnf ? "No result" : new[] { "No medal", "BRONZE", "SILVER", "GOLD" }[Medal] + "  ·  " + Measure(Event, Event.Kind == CampaignEventKind.TimeTrial ? (float)Time : Score));
        }
        public static string Ordinal(int n) => n + (n % 100 is >= 11 and <= 13 ? "th" : (n % 10) switch { 1 => "st", 2 => "nd", 3 => "rd", _ => "th" });
        public static string Measure(CampaignEvent e, float value) => e.Kind == CampaignEventKind.TimeTrial ? RaceHud.FormatTime(value) : e.Kind == CampaignEventKind.SpeedTrap ? DisplayUnits.Speed(value) : e.Kind == CampaignEventKind.Jump ? DisplayUnits.Jump(value) : e.Kind == CampaignEventKind.Smash ? value.ToString("0") + " props" : RaceHud.FormatTime(value);
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
            Current.money += o.Pay + o.Bonus; Current.earned += o.Pay + o.Bonus; Current.secondsRacing += o.RaceSeconds;
            if (o.Passed && e.Final) foreach (var cup in CampaignData.CupsAfter(e.Chapter)) if (CupResult(cup.Id) == null && !o.Unlocked.Contains("Championship: " + cup.Name)) o.Unlocked.Add("Championship: " + cup.Name);
            if (o.Passed && e.Prize != null && !Owns(e.Prize)) { Current.owned.Add(e.Prize); o.Unlocked.Add("Vehicle: " + VehicleProfile.Find(e.Prize).Name); o.PrizeWon = e.Prize; }
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

        // ---------- 0.92 Part B: the campaign's vehicle defaults to the last one driven ----------
        public static bool ChampionPaint => Testing || CupResult("cup-grand")?.won == true;
        public static void Drove(string id)
        {
            if (Testing || string.IsNullOrEmpty(id)) return;
            Current.driven.Remove(id); Current.driven.Insert(0, id); if (Current.driven.Count > 12) Current.driven.RemoveRange(12, Current.driven.Count - 12);
        }
        // The default for an event's (or championship's) list of allowed owned vehicles: the most recently driven one in it,
        // else the first.
        public static string DefaultVehicle(VehicleProfile[] allowed)
        {
            if (allowed == null || allowed.Length == 0) return null;
            foreach (var id in Current.driven) if (allowed.Any(p => p.Id == id)) return id;
            return allowed[0].Id;
        }

        // ---------- 0.90 upgrades (campaign only: Race, Free Roam, split-screen and the Top 10 boards stay stock) ----------
        public static int[] Levels(string id) => Current.upgrades.FirstOrDefault(u => u.id == id)?.levels ?? new int[4];
        public static float Multiplier(string id, int stat) => 1 + CampaignData.UpgradeStep[stat] * Mathf.Clamp(Levels(id)[stat], 0, CampaignData.UpgradeLevels);
        public static bool Upgraded(string id) => Levels(id).Any(l => l > 0);
        public static string UpgradeSummary(string id) { var l = Levels(id); return l.All(x => x == 0) ? "stock" : string.Join(", ", Enumerable.Range(0, 4).Where(i => l[i] > 0).Select(i => CampaignData.UpgradeNames[i] + " " + l[i])); }
        public static bool CanUpgrade(VehicleProfile p, int stat, out string why, out int cost)
        {
            why = ""; int level = Levels(p.Id)[stat]; cost = CampaignData.UpgradeCost(p.Id, level + 1);
            if (Testing) { why = "Testing mode is on: upgrades are off"; return false; }
            if (p.Reward || CampaignData.VehicleValue(p.Id) <= 0) { why = "No upgrades for this vehicle"; return false; }
            if (!Owns(p.Id)) { why = "Buy or win it first"; return false; }
            if (level >= CampaignData.UpgradeLevels) { why = "Fully upgraded"; return false; }
            if (Current.money < cost) { why = $"Not enough money: {Money(cost)} needed"; return false; }
            return true;
        }
        public static bool BuyUpgrade(VehicleProfile p, int stat)
        {
            if (!CanUpgrade(p, stat, out _, out int cost)) return false;
            var keep = JsonUtility.ToJson(Current);
            var u = Current.upgrades.FirstOrDefault(x => x.id == p.Id); if (u == null) Current.upgrades.Add(u = new Upgrade { id = p.Id });
            u.levels[stat]++; Current.money -= cost;
            if (Save()) return true;
            Current = JsonUtility.FromJson<State>(keep); return false;
        }
        public static void ApplyUpgrades(VehicleConfiguration configuration)
        {
            if (!configuration || Testing) return; string id = configuration.profileId;
            configuration.ApplyUpgrades(Multiplier(id, 0), Multiplier(id, 1), Multiplier(id, 2), Multiplier(id, 3));
        }

        // ---------- 0.90 championships ----------
        public static Cup CupResult(string id) => Current.cups.FirstOrDefault(c => c.id == id);
        public static bool CupOpen(CampaignCup c) { var final = CampaignData.InChapter(c.AfterChapter).FirstOrDefault(e => e.Final); return final != null && Passed(final); }
        public static Cup ActiveCup(CampaignCup c) { var r = CupResult(c.Id); return r != null && r.active ? r : null; }
        // Starting (or restarting) a championship: a new table, the chosen vehicle kept for every round. Saved at once.
        public static Cup StartCup(CampaignCup c, string vehicle)
        {
            var r = CupResult(c.Id); if (r == null) Current.cups.Add(r = new Cup { id = c.Id });
            r.active = true; r.vehicle = vehicle; r.races.Clear(); Save(); return r;
        }
        public static void AbandonCup(CampaignCup c) { var r = CupResult(c.Id); if (r == null) return; r.active = false; r.races.Clear(); Save(); }
        // Points per driver (0 the player, 1-5 the rivals), ordered: most points, then most wins, then the better place in the
        // last race (did not finish counts as last).
        public static List<(int driver, int points, int wins)> Standings(Cup r)
        {
            var list = Enumerable.Range(0, 6).Select(d => (driver: d, points: r.races.Sum(x => x.places[d] > 0 ? CampaignData.CupPoints[Mathf.Min(x.places[d], 6) - 1] : 0), wins: r.races.Count(x => x.places[d] == 1))).ToList();
            int Last(int d) { var x = r.races.LastOrDefault(); return x == null || x.places[d] <= 0 ? 7 : x.places[d]; }
            return list.OrderByDescending(s => s.points).ThenByDescending(s => s.wins).ThenBy(s => Last(s.driver)).ThenBy(s => s.driver).ToList();
        }
        public static string DriverName(int driver) => driver == 0 ? PlayerNames.Player : CampaignData.RivalNames[driver - 1];
        // 0.94 Part A: a championship's driver (0 the player, 1-5 its roster slots) by the cast member in that slot
        public static string DriverName(Cup r, int driver) { var cup = r == null ? null : CampaignData.FindCup(r.id); if (driver == 0 || cup == null) return DriverName(driver); var cast = CampaignData.CastFor(cup.Rivals); return driver - 1 < cast.Length ? CampaignData.RivalNames[cast[driver - 1]] : DriverName(driver); }
        // A round's result: saved at once. The last round ends the championship: paid by final position (half on a replay),
        // the first win pays its bonus and marks the trophy; winning the Grand Championship completes the campaign.
        public static Outcome CommitCupRace(Outcome o)
        {
            var cup = o.Event.Cup; o.Testing = Testing; if (o.Debug || Testing) return o;
            var r = CupResult(cup.Id); if (r == null || !r.active) return o;
            r.races.Add(new CupRace { places = (int[])o.Places.Clone(), time = o.Time });
            Current.secondsRacing += o.RaceSeconds;
            if (r.races.Count >= cup.Rounds.Length)
            {
                var table = Standings(r); int position = table.FindIndex(s => s.driver == 0) + 1;
                o.CupFinished = true; o.CupPosition = position; o.Replay = r.runs > 0; o.Won = position == 1;
                o.Pay = Share(cup.Pay, CampaignData.PlaceShare[Mathf.Clamp(position - 1, 0, CampaignData.PlaceShare.Length - 1)] * (o.Replay ? CampaignData.ReplayShare : 1));
                if (o.Won && !r.bonusPaid) { o.Bonus = cup.Bonus; r.bonusPaid = true; }
                r.runs++; r.finished = true; r.won |= o.Won; r.bestPosition = r.bestPosition == 0 ? position : Mathf.Min(r.bestPosition, position); r.active = false;
                Current.money += o.Pay + o.Bonus; Current.earned += o.Pay + o.Bonus;
                if (cup.Grand && o.Won && !Current.complete) { Current.complete = true; o.Unlocked.Add("The campaign: you are the Champion of Woodstock"); }
                if (cup.Grand && o.Won && !o.Replay) { o.ChampionPaint = true; o.Unlocked.Add("Paint: the champion's scheme (gold with a number roundel), for any owned vehicle"); }
            }
            o.Saved = Save();
            return o;
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
