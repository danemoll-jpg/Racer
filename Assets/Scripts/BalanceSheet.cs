using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Racer
{
    // 0.109 Part D: BalanceLog/balance-sheet.md, every current tuning value in plain tables and where it lives in code. Rewritten at each game start
    // (and when a scene with activity sites is first loaded: those targets are scene data, kept in BalanceLog/activity-targets.json as scenes are seen).
    // Read only: values come from the code itself where they are fields; values written inline in code are copied here with their place, so a
    // change there needs this file changed too (marked "inline").
    public static class BalanceSheet
    {
        [Serializable] sealed class Site { public string scene, id, title, kind; public float seconds, bronze, silver, gold; public float[] vehicleBronze, vehicleSilver, vehicleGold; }
        [Serializable] sealed class Sites { public List<Site> list = new(); }
        const string SitesFile = "activity-targets.json";
        static bool written;

        // RaceFlow.Start, once the scene's activity sites are known
        public static void NoteScene(string scene, ActivitySite[] sites)
        {
            if (ResultsLog.Folder == null) return;
            try
            {
                Directory.CreateDirectory(ResultsLog.Folder);
                string path = Path.Combine(ResultsLog.Folder, SitesFile); var data = new Sites();
                try { if (File.Exists(path)) data = JsonUtility.FromJson<Sites>(File.ReadAllText(path)) ?? new Sites(); } catch (Exception) { data = new Sites(); }
                data.list ??= new List<Site>();
                var now = (sites ?? new ActivitySite[0]).Where(s => s).Select(s => new Site { scene = scene, id = s.id, title = s.title, kind = s.kind.ToString(), seconds = s.Seconds, bronze = s.bronze, silver = s.silver, gold = s.gold,
                    vehicleBronze = s.vehicleBronze?.Length == 4 ? s.vehicleBronze : null, vehicleSilver = s.vehicleSilver?.Length == 4 ? s.vehicleSilver : null, vehicleGold = s.vehicleGold?.Length == 4 ? s.vehicleGold : null }).ToList();
                string before = JsonUtility.ToJson(new Sites { list = data.list.Where(x => x.scene == scene).ToList() });
                data.list = data.list.Where(x => x.scene != scene).Concat(now).OrderBy(x => x.scene).ThenBy(x => x.id).ToList();
                bool changed = before != JsonUtility.ToJson(new Sites { list = now });
                if (changed) AtomicSave.Write(path, JsonUtility.ToJson(data, true));
                if (!written || changed) { written = true; AtomicSave.Write(Path.Combine(ResultsLog.Folder, ResultsLog.SheetName), Build(data.list)); }
            }
            catch (Exception e) { Debug.LogWarning("Balance sheet not written: " + e.Message); }
        }

        static string N(float v) => v.ToString("0.###", CultureInfo.InvariantCulture);
        static string L(IEnumerable<float> v) => string.Join(" / ", v.Select(N));
        static readonly string[] Diff = { "Easy", "Normal", "Hard" };

        static string Build(List<Site> sites)
        {
            var b = new StringBuilder();
            void Line(string s = "") => b.Append(s).Append('\n');
            void Table(string[] head, IEnumerable<string[]> rows) { Line("| " + string.Join(" | ", head) + " |"); Line("|" + string.Concat(head.Select(_ => "---|"))); foreach (var r in rows) Line("| " + string.Join(" | ", r.Select(c => (c ?? "").Replace("|", "/"))) + " |"); Line(); }
            Line("# Woodstock Rush balance sheet"); Line();
            Line($"Version {Application.version}, written {DateTime.Now:yyyy-MM-dd HH:mm}. Every current tuning value and where it lives (paths under Assets/Scripts). Read only: nothing here changes the game.");
            Line("\"inline\" = a number written inside the code at that place (copied here); the rest are read from the code's own fields."); Line();

            // ---------------------------------------------------------------- rival AI
            Line("## Rival AI pace and skill per difficulty"); Line();
            var t = RoadDriver.Tuning;
            Table(new[] { "Value", "Easy", "Normal", "Hard", "Where" }, new[]
            {
                new[] { "Corner speed use (share of grip)", N(t.corner[0]), N(t.corner[1]), N(t.corner[2]), "RoadDriver.cs cornerUse" },
                new[] { "Brake use", N(t.brake[0]), N(t.brake[1]), N(t.brake[2]), "RoadDriver.cs brakeUse" },
                new[] { "Top speed use", N(t.speed[0]), N(t.speed[1]), N(t.speed[2]), "RoadDriver.cs speedUse" },
                new[] { "Hill target speed (m/s)", N(t.hill[0]), N(t.hill[1]), N(t.hill[2]), "RoadDriver.cs hillTargets" },
                new[] { "Consistency lift (target speed dips up to)", "7 %", "1.2 %", "none", "RoadDriver.cs (inline, TargetSpeed *= 1 - ...)" },
                new[] { "Crest speed constant", "6.5", "7.6", "8.2", "RoadDriver.cs (inline, hillSpeed)" },
                new[] { "Throttle gain", "0.5", "0.8", "0.8", "RoadDriver.cs (inline, throttle)" },
                new[] { "Judgment error length (s)", "3.2", "2.6", "2", "DriverVariation.cs (inline)" },
                new[] { "Judgment error strength", "1", "0.65", "0.4", "DriverVariation.cs (inline)" },
                new[] { "Seconds between errors (+ 0-25 random)", "18", "30", "48", "DriverVariation.cs (inline)" },
                new[] { "Estimated finish skill factor", "0.80", "1", "1.08", "AiFinishEstimate.cs (inline)" },
            });
            Line("- Each rival's own pace: 0.94 + 0.025 × its slot (slot 0 = 0.94 … slot 4 = 1.04), RaceDirector.cs (inline, driver.Initialize). The results log records each rival's pace.");
            Line("- Error effects: wide line 0.65, jump line 0.28, early braking −30 % (DriverVariation.cs, inline). Estimated finish pace: profile speed × 0.36 (forest) / 0.46, AiFinishEstimate.cs (inline).");
            Line("- **AI catch-up / rubber band in races: none** (RoadDriver: no speed multipliers or positional driving). Only the shortcut choice depends on place and gap: probability = clamp(0.12 + 0.34 × rank share + 0.12 × clamp01(gap / 200 m) + risk ±0.04, 0.06, 0.65), ShortcutStrategy.cs (inline).");
            Line($"- Race timeouts: {N(1200)} s × max(1, laps / 3) for the race, {N(90)} s after the first finisher (RaceDirector.cs). Missed gate: +{RaceDirector.OrdinaryMissPenalty} s.");
            Line($"- Weather grip (share of dry grip; car / tourer / moto / ATV ... as WeatherGrip.cs): rain {L(WeatherGrip.Rain)}, snow {L(WeatherGrip.Snow)}."); Line();

            // ---------------------------------------------------------------- campaign
            Line("## Campaign: rivals and difficulty per chapter"); Line();
            foreach (var ch in CampaignData.Chapters)
            {
                Line($"### Chapter {ch.Number}: {ch.Name}"); Line();
                Table(new[] { "Event", "Id", "Kind", "Course", "Laps", "Rivals", "Difficulty", "Conditions", "Time limit (s)", "Targets bronze / silver / gold", "Pay (1st / gold)", "Medal pay b / s / g", "Bonus", "Prize" },
                    CampaignData.InChapter(ch.Number).Select(e => new[] { e.Name, e.Id, e.Kind.ToString(), RacePlaylists.Titles[e.Course], e.Laps.ToString(), e.Rivals.Length == 0 ? "—" : CampaignData.CastLine(e.Rivals), Diff[Mathf.Clamp(e.Difficulty, 0, 2)], e.Conditions,
                        e.TimeLimit > 0 ? N(e.TimeLimit) : "—", e.Targets != null ? L(e.Targets) : "— (race: top 3 passes, 1st wins)", "$" + e.Pay, e.Kind == CampaignEventKind.Race ? "—" : $"${Campaign.MedalPay(e, 1)} / ${Campaign.MedalPay(e, 2)} / ${Campaign.MedalPay(e, 3)}", "$" + e.Bonus, e.Prize ?? "" }));
            }
            Line("### Championships"); Line();
            Table(new[] { "Cup", "Id", "After chapter", "Rounds", "Rivals", "Difficulty", "Pay", "Bonus" }, CampaignData.Cups.Select(c => new[] { c.Name, c.Id, c.AfterChapter.ToString(), string.Join("; ", c.Rounds.Select(r => RacePlaylists.Titles[r.Course] + " ×" + r.Laps + " " + r.Conditions)), CampaignData.CastLine(c.Rivals), Diff[Mathf.Clamp(c.Difficulty, 0, 2)], "$" + c.Pay, "$" + c.Bonus }));
            Line($"- Pay by place: {L(CampaignData.PlaceShare)} of first; medal shares {L(CampaignData.MedalShare)}; replays pay {N(CampaignData.ReplayShare)}; cup points per place {string.Join(" / ", CampaignData.CupPoints)} (CampaignData.cs / Campaign.cs).");
            Line($"- Pass rules: a race passes in the top 3 and wins in 1st; other events pass at bronze and win at gold (Campaign.cs Commit). Time trials: lower is better."); Line();
            Line("## Practice laps"); Line();
            Line("Practice laps are ordinary campaign time trials (one flying lap from the run-up) named \"Practice: …\" (ids contain -practice-), with lower pay and generous targets; required in the chapter order, optional under the legacy order (Campaign.cs LegacyAvailable). They are listed in the chapter tables above:"); Line();
            Table(new[] { "Practice", "Chapter", "Course", "Run-up (m)", "Targets (s)", "Pay" }, CampaignData.Events.Where(e => e.Id.Contains("-practice-")).Select(e => new[] { e.Name, e.Chapter.ToString(), RacePlaylists.Titles[e.Course], N(e.RunUp), L(e.Targets), "$" + e.Pay }));

            // ---------------------------------------------------------------- medals elsewhere
            Line("## Medal targets outside the campaign"); Line();
            Line("- Quick races, playlists and split-screen races: no medals (records and Top 10 only). Getaway, Cop vs Runner and Speed Patrol: no medals.");
            Line($"- **Stunt event** ({StuntProgress.Minutes} minutes): bronze / silver / gold {string.Join(" / ", StuntProgress.Targets.Select(x => x.ToString("N0")))} points (StuntProgress.cs Targets). Gold earns the {VehicleProfile.StuntBike.Name}.");
            Line($"  Trick points (ArcadeActivities.Stunts.cs): flip {ArcadeActivities.FlipPoints}, roll {ArcadeActivities.RollPoints}, spin {ArcadeActivities.SpinPoints}, loop {ArcadeActivities.LoopPoints}, barrel roll {ArcadeActivities.BarrelPoints}; combo = points × links (at most {ArcadeActivities.MaxLinks}); chain within {N(ArcadeActivities.ComboGap)} s; a trick needs {N(ArcadeActivities.MinStuntAir)} s air and {N(ArcadeActivities.MinStuntDistance)} m; landing recovery window {N(ArcadeActivities.RecoverWindow)} s.");
            Line(); Line("### Free Roam activities (scene data on each ActivitySite; per-vehicle targets for car / tourer / motorcycle / ATV where set)"); Line();
            if (sites.Count == 0) Line("No scene with activities loaded yet; this table fills in as scenes load.");
            else Table(new[] { "Scene", "Site", "Kind", "Bronze / silver / gold", "Per vehicle (car / tourer / moto / ATV)", "Attempt seconds" }, sites.Select(s => new[] { s.scene, s.title + " (" + s.id + ")", s.kind, $"{N(s.bronze)} / {N(s.silver)} / {N(s.gold)}" + (s.kind == "Speed" ? " m/s" : s.kind == "Jump" ? " m" : " props"),
                s.vehicleBronze?.Length == 4 && s.vehicleSilver?.Length == 4 && s.vehicleGold?.Length == 4 ? $"b {L(s.vehicleBronze)}; s {L(s.vehicleSilver)}; g {L(s.vehicleGold)}" : "—", s.kind == "Speed" ? "— (always armed)" : N(s.seconds) }));
            Line("Authored in the editor scripts (Assets/Editor: ArcadeReviewRelease.cs, DiscoveryAuthoring.cs, CR110Mountain.cs, ExplorationAuthoring.cs) and the scene files."); Line();

            // ---------------------------------------------------------------- unlocks
            Line("## Vehicle unlock goals"); Line();
            string How(VehicleProfile p) => CampaignData.Starters.Contains(p.Id) ? "owned from the start" : p.Id == VehicleProfile.StuntBuggy.Id ? "free (Free Roam and the Stunt event only)" : p.Id == VehicleProfile.StuntBike.Id ? StuntProgress.BikeGoal + " (Free Roam and the Stunt event only)" : Campaign.HowToGet(p);
            string Where(VehicleProfile p) => CampaignData.Starters.Contains(p.Id) ? "CampaignData.Starters" : Campaign.Price(p.Id) > 0 ? $"CampaignData.Prices (${Campaign.Price(p.Id):N0}, chapter {Campaign.PriceChapter(p.Id)})" : Campaign.PrizeEvent(p.Id) != null ? "CampaignData.Events Prize (" + Campaign.PrizeEvent(p.Id).Id + ")"
                : p.Id == "mower" ? "VehicleUnlocks.AcornsNeeded" : p.IsPolice ? "PoliceProgress (HeatNeeded, SpeedersNeeded)" : p.Id == VehicleProfile.StuntBike.Id ? "StuntProgress.Targets (gold)" : p.Id == VehicleProfile.StuntBuggy.Id ? "VehicleUnlocks (always earned)" : "";
            Table(new[] { "Vehicle", "Id", "Class", "How to get it", "Where" }, VehicleProfile.All.Select(p => new[] { p.Name, p.Id, p.Class, How(p), Where(p) }));
            Line($"- Acorns: {VehicleUnlocks.AcornsNeeded} (VehicleUnlocks.cs). Patrol car: escape at heat {PoliceProgress.HeatNeeded}+ on Normal or Hard (PoliceProgress.cs; the difficulty rule inline). Patrol cycle: {PoliceProgress.SpeedersNeeded} speeders caught. Stunt motorcycle: Stunt event gold ({StuntProgress.Targets[2]:N0}).");
            Line($"- Upgrades: {CampaignData.UpgradeLevels} levels; step per level {string.Join(", ", CampaignData.UpgradeNames.Select((n, i) => n + " " + N(CampaignData.UpgradeStep[i])))}; cost share of the vehicle's value {L(CampaignData.UpgradeCostShare)} (CampaignData.cs)."); Line();

            // ---------------------------------------------------------------- police
            Line("## Police chase pacing (Getaway; GetawayChase.cs)"); Line();
            Table(new[] { "Value", "Easy", "Normal", "Hard", "Where" }, new[]
            {
                new[] { "Most cops at once", GetawayChase.CopCap[0].ToString(), GetawayChase.CopCap[1].ToString(), GetawayChase.CopCap[2].ToString(), "CopCap" },
                new[] { "Seconds per heat level while chased", N(GetawayChase.HeatEvery[0]), N(GetawayChase.HeatEvery[1]), N(GetawayChase.HeatEvery[2]), "HeatEvery" },
                new[] { "Seconds unseen to escape", N(GetawayChase.EscapeBy[0]), N(GetawayChase.EscapeBy[1]), N(GetawayChase.EscapeBy[2]), "EscapeBy" },
                new[] { "Bust meter rate", N(GetawayChase.BustRate[0]), N(GetawayChase.BustRate[1]), N(GetawayChase.BustRate[2]), "BustRate" },
                new[] { "Extra heat for a ram / roadblock run / 10 s off-road seen", "no", "yes", "yes", "inline" },
            });
            Table(new[] { "Heat level", "What happens" }, new[]
            {
                new[] { "1", "start: min(most cops, 2) cops 62-78 m and 120 m+ behind (inline); backup checks every 4 s: wanted near cops = min(most cops, heat + 1), placed 250-450 m ahead; total cap most cops + 4 (inline)" },
                new[] { "2", "as above with one more wanted cop" },
                new[] { "3", "roadblocks every 38 s (at most 2, 240-420 m ahead, lifted after 80 s); every third new unit a police bike (inline). Heat 3+ escape earns the patrol car" },
                new[] { "4-5", "helicopter (from heat 4, inline); heat 5 is the top" },
            });
            Line($"- Constants: sight {N(GetawayChase.SightRange)} m, near cops {N(GetawayChase.NearCops)} m, catch within {N(GetawayChase.CatchDistance)} m under {N(GetawayChase.CatchSpeed)} m/s, box speed {N(GetawayChase.BoxSpeed)}, contact speed {N(GetawayChase.ContactSpeed)}, pin {N(GetawayChase.PinDistance)} m, held {N(GetawayChase.HeldSpeed)} m/s within {N(GetawayChase.HeldDistance)} m, runner release {N(GetawayChase.RunnerRelease)} s, end pause {N(GetawayChase.EndPause)} s, reset hold {N(GetawayChase.ResetHold)} s.");
            Line("- Bust meter fill per second by situation: 1/3, 1/6, 1/9, 1/6, 1/12, 1/6; drain 1/3 (inline). Round lengths 3 / 5 / 8 minutes (RaceMenus.Getaway.cs); a hidden-police chase 300 s.");
            Line("- Cop speed: every cop copies the runner's top speed and acceleration (at least its grip and braking); catch-up boost beyond 150 m: 1 + 0.3 × clamp01((distance − 80) / 220) until within 80 m, at most 1.3 (inline: GetawayChase Boosts, CopDriver). Cop corner grip min(grip × 0.62, 14), braking max(6, braking × 0.75) (CopDriver.cs, inline).");
            Line($"- Helicopter: delay {N(PoliceHelicopter.Delay)} s, altitude {N(PoliceHelicopter.Altitude)} m, cone {N(PoliceHelicopter.ConeRadius)} m, speed {N(PoliceHelicopter.Speed)} m/s (PoliceHelicopter.cs).");
            Line($"- Hidden police in Free Roam: sees {N(HiddenPolice.SeeRange)} m, over the limit by {N(HiddenPolice.OverBy)} mph, quiet {N(HiddenPolice.QuietStart)} s at start and {N(HiddenPolice.QuietAfter)} s after a chase (HiddenPolice.cs).");
            Line($"- Cop vs Runner: catch within {N(PoliceChase.CatchDistance)} m under {N(PoliceChase.CatchSpeed)} m/s for {N(PoliceChase.FillSeconds)} s; the cop starts {N(PoliceChase.CopDelay)} s later, {N(PoliceChase.CopBehind)} m behind (PoliceChase.cs).");
            Line($"- Speed Patrol: radar {N(SpeedPatrol.RadarRange)} m / {N(SpeedPatrol.RadarCone)}°, pursuit {N(SpeedPatrol.PursuitRange)} m, speeders get away after {N(SpeedPatrol.GetAwaySeconds)} s, at most {SpeedPatrol.MaxSpeeders} at once; points: catch {SpeedPatrol.CatchPoints} + {SpeedPatrol.PerMph} per mph over, hit −{SpeedPatrol.HitPenalty}, no violation −{SpeedPatrol.NoViolationPenalty} (SpeedPatrol.cs); speeders 10-40 mph over, one every 20-30 s (inline).");
            Line(); Line("## Vehicles"); Line();
            Table(new[] { "Vehicle", "Id", "Class", "Speed", "Acceleration", "Grip", "Response", "Mass" }, VehicleProfile.All.Select(p => new[] { p.Name, p.Id, p.Class, N(p.Speed), N(p.Acceleration), N(p.Grip), N(p.Response), N(p.Mass) }));
            Line("(VehicleProfile.cs)");
            return b.ToString();
        }
    }
}
