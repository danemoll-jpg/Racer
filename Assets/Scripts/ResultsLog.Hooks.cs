using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Racer
{
    // 0.109 Part C: what the results log records. An attempt opens when a race, campaign event, split-screen race, Getaway, Cop vs Runner, Speed Patrol
    // or Stunt event starts (RaceDirector.RestartRace), and is written once: finished at RaceFlow.CompleteResults (every one of those modes ends there),
    // or abandoned when it is left before its result (quit, restart, a scene change, quitting the game, a championship round forfeited). Free Roam
    // activities (jumps, speed traps, smash attempts) and hidden-police chases are written as they end. Split-screen: one line per player.
    // Read only: nothing here changes a result, a record or a tuning value.
    public static class BalanceLog
    {
        sealed class Base { public int resets, wipeouts, impacts, big; }
        static bool open; static string openMode, openKey; static float openedAt;
        static readonly Dictionary<ArcadeVehicle, Base> bases = new();
        public static bool Open => open;

        static string ModeOf(RaceFlow flow)
        {
            var race = flow.Race;
            if (CampaignRun.Active != null) return CampaignRun.Cup != null ? "campaign-championship-round" : CampaignRun.Active.Id.Contains("-practice-") ? "campaign-practice-lap" : "campaign-" + CampaignRun.Active.Kind.ToString().ToLowerInvariant();
            if (SplitScreen.Active && GetawayChase.Current && !GetawayChase.Current.Embedded) return GetawayChase.Current.Mode == GetawayChase.Variant.Getaway ? "getaway" : "cop-vs-runner";
            if (SplitScreen.Active && PoliceChase.Current) return "cop-vs-runner";
            if (SplitScreen.Active && SpeedPatrol.Current) return "speed-patrol";
            if (SplitScreen.Active && StuntEvent.Current) return "stunt-event";
            if (race.FreeRoam) return null; // Free Roam (alone or for two) is not an attempt
            if (SplitScreen.Active) return "split-screen-race";
            return RacePlaylists.Active != null ? "playlist-race" : "quick-race";
        }
        static string KeyOf(RaceFlow flow, string mode)
        {
            var race = flow.Race;
            if (CampaignRun.Active != null) return "campaign:" + (CampaignRun.Cup != null ? CampaignRun.Cup.Id + "/round" + CampaignRun.Active.Round : CampaignRun.Active.Id);
            switch (mode)
            {
                case "getaway": return $"getaway:d{SplitScreen.PoliceDifficulty}-{SplitScreen.PoliceMinutes}min";
                case "cop-vs-runner": return $"cop-vs-runner:{SplitScreen.PoliceMinutes}min";
                case "speed-patrol": return $"speed-patrol:{SplitScreen.PoliceMinutes}min";
                case "stunt-event": return "stunt-event";
                default: return "race:" + race.Category;
            }
        }

        // RaceDirector.RestartRace (every start and restart): opens the attempt (a still open one was left: abandoned)
        public static void Begin(RaceFlow flow)
        {
            if (!flow || ResultsLog.Folder == null) return;
            if (PoliceChase.Current && PoliceChase.Current.Round > 0 && open) return; // Cop vs Runner's second round is the same attempt
            if (open) Abandon(flow, "restarted");
            var mode = ModeOf(flow); if (mode == null) return;
            open = true; openMode = mode; openKey = KeyOf(flow, mode); openedAt = Time.unscaledTime; bases.Clear();
            foreach (var car in Cars(flow)) Remember(car);
        }
        static IEnumerable<ArcadeVehicle> Cars(RaceFlow flow)
        {
            yield return flow.Race.vehicle;
            if (SplitScreen.Race && SplitScreen.Race.P2Car) yield return SplitScreen.Race.P2Car;
            if (SplitRoam.Current && SplitRoam.Current.Car) yield return SplitRoam.Current.Car;
        }
        static void Remember(ArcadeVehicle car)
        {
            if (!car || bases.ContainsKey(car)) return;
            bases[car] = new Base { resets = car.GetComponent<VehicleRespawn>()?.Resets ?? 0, wipeouts = car.GetComponent<VehicleConfiguration>()?.Wipeouts ?? 0, impacts = car.GetComponent<VehicleAudio>()?.ImpactCount ?? 0, big = car.GetComponent<VehicleAudio>()?.BigImpactCount ?? 0 };
        }
        // the player 2 vehicle may be made after the start (split-screen modes): counted from the first time it is seen
        static ResultsLog.Json Incidents(ArcadeVehicle car)
        {
            var j = new ResultsLog.Json(); if (!car) return j;
            bases.TryGetValue(car, out var b); b ??= new Base();
            var audio = car.GetComponent<VehicleAudio>();
            return j.Add("resets", (car.GetComponent<VehicleRespawn>()?.Resets ?? 0) - b.resets).Add("wipeouts", (car.GetComponent<VehicleConfiguration>()?.Wipeouts ?? 0) - b.wipeouts)
                    .Add("collisions", audio ? audio.ImpactCount - b.impacts : float.NaN).Add("bigCollisions", audio ? audio.BigImpactCount - b.big : float.NaN).Add("bigCollisionAtLeastMps", VehicleAudio.BigImpact);
        }

        // RaceFlow.CompleteResults' last step: the attempt finished
        public static void Finished(RaceFlow flow) { if (open) Write(flow, true, null); }
        // left before its result: quit, restart, retry, a scene change, quitting the game, a championship round forfeited
        public static void Abandon(RaceFlow flow, string how)
        {
            if (!open || !flow) return;
            if (flow.State == RaceFlow.Stage.Results) { open = false; return; } // already written as finished
            if (CampaignRun.Cup != null && how == "quit") how = "championship round forfeited (counts as did not finish)";
            Write(flow, false, how);
        }

        static void Write(RaceFlow flow, bool finished, string how)
        {
            open = false;
            try
            {
                var (number, retry) = ResultsLog.Attempt(openKey);
                var lines = new List<ResultsLog.Json>();
                switch (openMode)
                {
                    case "getaway": case "cop-vs-runner": if (GetawayChase.Current) Getaway(flow, lines, number, retry, finished, how); else CopRunner(flow, lines, number, retry, finished, how); break;
                    case "speed-patrol": Patrol(flow, lines, number, retry, finished, how); break;
                    case "stunt-event": Stunt(flow, lines, number, retry, finished, how); break;
                    default: Race(flow, lines, number, retry, finished, how); break;
                }
                foreach (var j in lines) ResultsLog.Write(j);
            }
            catch (System.Exception e) { Debug.LogWarning("Results log line not made: " + e.Message); }
        }
        static ResultsLog.Json Head(RaceFlow flow, int number, bool retry, bool finished, string how, int player, string name)
        {
            var race = flow.Race; var look = WorldLook.Current; bool split = SplitScreen.Active;
            var j = ResultsLog.Line(openMode, openKey, number, retry);
            j.Add("player", player).Add("name", name).Add("finished", finished).Add("endedBy", finished ? "result" : how).Add("secondsPlayed", Time.unscaledTime - openedAt)
             .Add("course", race.gameObject.scene.name).Add("courseName", !race.FreeRoam ? race.courseName : openMode == "stunt-event" ? StuntPark.Title : "Free Roam world").Add("direction", race.FreeRoam ? null : race.reverseCourse ? "reverse" : "forward")
             .Add("timeOfDay", split ? SplitScreen.Time.ToString() : look ? (look.Mode == "Free Roam" ? $"Free Roam {look.Hour:0.0} h" : look.RaceTime.ToString()) : null)
             .Add("weather", split ? SplitScreen.Weather.ToString() : look ? (look.Mode == "Free Roam" ? look.RoamWeather.ToString() : look.RaceWeather.ToString()) : null)
             .Add("debugMovementUsed", flow.DebugMovementUsed);
            return j;
        }
        static string VehicleOf(ArcadeVehicle car) => car ? car.GetComponent<VehicleConfiguration>()?.profileId : null;
        // the medal earned, every target, and how far from the next medal (lowerIsBetter: a time)
        static void Medal(ResultsLog.Json j, int medal, float value, float[] bronzeSilverGold, bool lowerIsBetter, string units)
        {
            j.Add("medal", MedalName(medal)).Add("medalValue", value).Add("medalUnits", units).Add("targets", new ResultsLog.Json().Add("bronze", bronzeSilverGold[0]).Add("silver", bronzeSilverGold[1]).Add("gold", bronzeSilverGold[2]));
            if (medal < 3) j.Add("nextMedal", MedalName(medal + 1)).Add("toNextMedal", lowerIsBetter ? value - bronzeSilverGold[medal] : bronzeSilverGold[medal] - value);
            else j.Add("nextMedal", (string)null).Add("overGold", lowerIsBetter ? bronzeSilverGold[2] - value : value - bronzeSilverGold[2]);
        }
        static string MedalName(int m) => new[] { "none", "bronze", "silver", "gold" }[Mathf.Clamp(m, 0, 3)];

        // ---------------------------------------------------------------- races (quick, playlist, split-screen, campaign)
        static void Race(RaceFlow flow, List<ResultsLog.Json> lines, int number, bool retry, bool finished, string how)
        {
            var race = flow.Race; var order = race.Ordered(finished); var e = CampaignRun.Active; var o = CampaignRun.Last;
            int humans = SplitScreen.Active && !SplitScreen.P2Ai && race.Racers.Count > 1 ? 2 : 1;
            for (int p = 0; p < humans; p++)
            {
                var me = race.Racers[p]; string name = SplitScreen.Active ? SplitScreen.NameOf(p + 1) : PlayerNames.Player;
                var j = Head(flow, number, retry, finished, how, p + 1, name);
                j.Add("laps", race.laps).Add("difficulty", race.DifficultyName).Add("rivalsCount", race.RivalCount).Add("traffic", race.traffic).Add("vehicle", VehicleOf(me.Car));
                if (e != null)
                {
                    j.Add("campaign", new ResultsLog.Json().Add("eventId", e.Id).Add("eventName", e.Name).Add("chapter", e.Chapter).Add("kind", e.Kind.ToString()).Add("practiceLap", e.Id.Contains("-practice-"))
                        .Add("final", e.Final).Add("championship", e.Cup?.Id).Add("round", e.Cup != null ? e.Round : 0).Add("runsBefore", Campaign.ResultOf(e.Id)?.runs ?? 0)
                        .Add("attemptsThisRun", CampaignRun.Attempts.Count).Add("passed", o?.Passed ?? false).Add("won", o?.Won ?? false).Add("pay", o?.Pay ?? 0).Add("bonus", o?.Bonus ?? 0)
                        .Add("unlocked", o != null ? o.Unlocked : new List<string>()).Add("cupFinished", o?.CupFinished ?? false).Add("cupPosition", o?.CupPosition ?? 0)
                        .Add("slot", Campaign.Fresh ? "fresh" : "main").Add("run", Replay(e, o) ? "replay" : "firstRun") // 0.110 Part E: balancing uses fresh first runs
                        .Add("garage", Campaign.Current.owned).Add("moneyAfter", Campaign.Current.money).Add("upgrades", Upgrades(CampaignRun.Vehicle ?? VehicleOf(me.Car)))); // 0.110 Part D
                }
                var prog = me.Progress;
                j.Add("result", new ResultsLog.Json().Add("place", order.IndexOf(me) + 1).Add("field", race.Racers.Count).Add("classified", me.Classified).Add("dnf", me.Dnf).Add("estimated", me.Estimated)
                    .Add("totalTime", me.Classified ? (float)me.ClassifiedTime(race.Clock) : float.NaN).Add("raceTime", (float)prog.RaceTime(race.Clock)).Add("penaltySeconds", (float)prog.PenaltySeconds).Add("missedGates", prog.MissedGates)
                    .Add("bestLap", prog.BestLap > 0 ? (float)prog.BestLap : float.NaN).Add("lapTimes", prog.LapTimes.Select(t => (float)t)).Add("completedLaps", prog.CompletedLaps).Add("targetLaps", prog.TargetLaps)
                    .Add("remainingMeters", prog.Finished ? 0 : race.RemainingDistance(me)));
                j.Add("rivals", race.Racers.Where((r, i) => r.IsAi && !(SplitScreen.Active && i == 1 && !SplitScreen.P2Ai)).Select(r => Rival(race, r, me, order)));
                j.Add("aiCatchUp", "none: race AI has no catch-up or rubber band (RoadDriver); only the shortcut choice depends on its place and gap (ShortcutStrategy)");
                if (e != null && e.Kind != CampaignEventKind.Race && e.Targets != null && o != null && !o.Dnf) Medal(j, o.Medal, e.Kind == CampaignEventKind.TimeTrial ? (float)o.Time : o.Score, e.Targets, e.Kind == CampaignEventKind.TimeTrial, e.Kind.ToString());
                else if (e != null && e.Kind != CampaignEventKind.Race && e.Targets != null) j.Add("medal", "none").Add("targets", new ResultsLog.Json().Add("bronze", e.Targets[0]).Add("silver", e.Targets[1]).Add("gold", e.Targets[2]));
                else j.Add("medal", (string)null).Add("medalNote", e != null ? "campaign races have no medals (top 3 passes, 1st wins)" : "races have no medals");
                j.Add("incidents", Incidents(me.Car));
                lines.Add(j);
            }
        }
        // 0.110 Part E: a replay = the event (or, for a championship round, that championship) was run to a result before this attempt
        static bool Replay(CampaignEvent e, Campaign.Outcome o)
        {
            if (e.Cup != null) return o != null && o.CupFinished ? o.Replay : (Campaign.CupResult(e.Cup.Id)?.runs ?? 0) > 0;
            return o != null && !o.Debug ? o.Replay : (Campaign.ResultOf(e.Id)?.runs ?? 0) > 0;
        }
        // 0.110 Part D: the campaign upgrades on the vehicle used (level per stat)
        static ResultsLog.Json Upgrades(string vehicle)
        {
            var j = new ResultsLog.Json().Add("vehicle", vehicle); var levels = Campaign.Levels(vehicle);
            for (int i = 0; i < levels.Length && i < CampaignData.UpgradeNames.Length; i++) j.Add(CampaignData.UpgradeNames[i], levels[i]);
            return j;
        }
        static ResultsLog.Json Rival(RaceDirector race, RacerState r, RacerState me, List<RacerState> order)
        {
            var driver = race.Drivers.FirstOrDefault(d => d && d.Racer == r);
            var j = new ResultsLog.Json().Add("name", r.Name).Add("vehicle", VehicleOf(r.Car)).Add("place", order.IndexOf(r) + 1).Add("classified", r.Classified).Add("dnf", r.Dnf).Add("estimated", r.Estimated)
                .Add("finishTime", r.Classified ? (float)r.ClassifiedTime(race.Clock) : float.NaN);
            if (r.Classified && me.Classified) j.Add("behindPlayerSeconds", (float)(r.ClassifiedTime(race.Clock) - me.ClassifiedTime(race.Clock))); // negative: ahead of the player
            else j.Add("behindPlayerMeters", race.RemainingDistance(r) - (me.Progress.Finished ? 0 : race.RemainingDistance(me)));
            return j.Add("pace", driver ? driver.Pace : float.NaN).Add("skill", race.DifficultyName).Add("recoveries", r.Recoveries);
        }

        // ---------------------------------------------------------------- Getaway (and its Cop vs Runner variant)
        static void Getaway(RaceFlow flow, List<ResultsLog.Json> lines, int number, bool retry, bool finished, string how)
        {
            var g = GetawayChase.Current;
            foreach (var r in g.Runners.Where(x => x.human))
            {
                var j = Head(flow, number, retry, finished, how, r.player, SplitScreen.NameOf(r.player)).Add("vehicle", VehicleOf(r.car)).Add("difficulty", new[] { "Easy", "Normal", "Hard" }[g.Difficulty]);
                j.Add("getaway", new ResultsLog.Json().Add("roundLimitSeconds", g.Limit).Add("durationSeconds", r.Done ? r.freeSeconds : g.Clock).Add("maxHeat", r.topHeat).Add("heatAtEnd", g.Heat)
                    .Add("copsAtPeak", g.PeakCops).Add("outcome", r.caught ? "busted" : r.escaped ? (r.limit ? "escaped (time limit)" : "escaped") : "not decided").Add("roadblocksPassed", r.blocks).Add("dodged", r.dodged).Add("helicopter", g.Heli != null));
                j.Add("medal", (string)null).Add("medalNote", "Getaway has no medals"); j.Add("incidents", Incidents(r.car)); lines.Add(j);
            }
        }
        static void CopRunner(RaceFlow flow, List<ResultsLog.Json> lines, int number, bool retry, bool finished, string how)
        {
            var c = PoliceChase.Current; if (!c) return;
            for (int p = 1; p <= (SplitScreen.P2Ai ? 1 : 2); p++)
            {
                var j = Head(flow, number, retry, finished, how, p, SplitScreen.NameOf(p)).Add("vehicle", c.VehicleFor(p));
                j.Add("copVsRunner", new ResultsLog.Json().Add("roundLimitSeconds", c.Limit).Add("runs", c.Runs.Select(r => new ResultsLog.Json().Add("runner", SplitScreen.NameOf(r.runner)).Add("caught", r.caught).Add("seconds", r.seconds).Add("runnerVehicle", r.runnerVehicle)))
                    .Add("winner", c.Winner == 0 ? "draw" : SplitScreen.NameOf(c.Winner)).Add("runnerResets", c.RunnerResets));
                j.Add("incidents", Incidents(c.CarOf(p))); lines.Add(j);
            }
        }
        // ---------------------------------------------------------------- Speed Patrol
        static void Patrol(RaceFlow flow, List<ResultsLog.Json> lines, int number, bool retry, bool finished, string how)
        {
            var s = SpeedPatrol.Current; if (!s) return;
            foreach (var cop in s.Cops.Where(x => x.player == 1 || !SplitScreen.P2Ai))
            {
                var j = Head(flow, number, retry, finished, how, cop.player, SplitScreen.NameOf(cop.player)).Add("vehicle", VehicleOf(cop.car));
                j.Add("speedPatrol", new ResultsLog.Json().Add("roundLimitSeconds", s.Limit).Add("secondsDriven", s.Clock).Add("speedersCaught", cop.catches).Add("speedersEscaped", s.GotAway).Add("speedersClocked", s.Clocked)
                    .Add("points", cop.points).Add("penalties", cop.penalties).Add("hits", cop.hits).Add("noViolationStops", cop.noViolations).Add("fastestMphOver", cop.fastest).Add("winner", s.Winner == 0 ? "draw" : SplitScreen.NameOf(s.Winner)));
                j.Add("medal", (string)null).Add("medalNote", "Speed Patrol has no medals"); j.Add("incidents", Incidents(cop.car)); lines.Add(j);
            }
        }
        // ---------------------------------------------------------------- Stunt event
        static void Stunt(RaceFlow flow, List<ResultsLog.Json> lines, int number, bool retry, bool finished, string how)
        {
            var e = StuntEvent.Current; if (!e) return;
            foreach (var r in e.Riders.Where(x => x.player == 1 || !SplitScreen.P2Ai))
            {
                var j = Head(flow, number, retry, finished, how, r.player, SplitScreen.NameOf(r.player)).Add("vehicle", r.vehicle);
                j.Add("stunt", new ResultsLog.Json().Add("roundSeconds", e.Limit).Add("testLimitSeconds", StuntEvent.TestLimit).Add("clockAtEnd", e.Clock).Add("score", r.points).Add("bestCombo", r.best).Add("combos", r.combos)
                    .Add("outsideThePark", r.outside).Add("tricks", r.acts ? r.acts.Tricks : 0).Add("bails", r.acts ? r.acts.Bails : 0).Add("top10Place", r.place).Add("newTop10Combos", r.entered.Count));
                Medal(j, StuntProgress.MedalFor(r.points), r.points, StuntProgress.Targets.Select(t => (float)t).ToArray(), false, "points");
                j.Add("incidents", Incidents(r.car)); lines.Add(j);
            }
        }

        // ---------------------------------------------------------------- Free Roam: activities and hidden-police chases (written as they end)
        public static bool RoamActivity(RaceDirector race) => race && race.FreeRoam && CampaignRun.Active == null && !SplitScreen.Active && !DeveloperLocationHud.Inspecting && ResultsLog.Folder != null;
        public static void Activity(ArcadeActivities acts, ActivitySite site, string vehicle, float value, string units, string outcome, bool timed, float personalBest)
        {
            if (!site) return;
            try
            {
                var race = acts.GetComponent<RaceDirector>() ?? Object.FindAnyObjectByType<RaceDirector>(); string key = "activity:" + acts.Key(site);
                var (number, retry) = ResultsLog.Attempt(key); var look = WorldLook.Current;
                var j = ResultsLog.Line("free-roam-" + site.kind.ToString().ToLowerInvariant(), key, number, retry).Add("player", 1).Add("name", PlayerNames.Player).Add("finished", outcome == "scored").Add("endedBy", outcome)
                    .Add("course", race ? race.gameObject.scene.name : null).Add("site", site.id).Add("siteName", site.title).Add("timedAttempt", timed).Add("vehicle", vehicle)
                    .Add("timeOfDay", look ? $"Free Roam {look.Hour:0.0} h" : null).Add("weather", look ? look.RoamWeather.ToString() : null).Add("personalBestBefore", personalBest);
                site.Targets(vehicle, out float b, out float s, out float g);
                if (outcome == "scored") Medal(j, site.Medal(value, vehicle), value, new[] { b, s, g }, false, units);
                else j.Add("medal", "none").Add("targets", new ResultsLog.Json().Add("bronze", b).Add("silver", s).Add("gold", g));
                if (site.kind == ActivitySite.Kind.Jump) j.Add("airtime", acts.LastAirtime);
                ResultsLog.Write(j);
            }
            catch (System.Exception e) { Debug.LogWarning("Results log line not made: " + e.Message); }
        }
        public static void HiddenPolice(GetawayChase g)
        {
            if (!g || ResultsLog.Folder == null) return;
            try
            {
                var r = g.Runners.FirstOrDefault(); const string key = "hidden-police"; var (number, retry) = ResultsLog.Attempt(key); var look = WorldLook.Current;
                var j = ResultsLog.Line("free-roam-hidden-police", key, number, retry).Add("player", 1).Add("name", PlayerNames.Player).Add("finished", true).Add("endedBy", "result")
                    .Add("vehicle", r != null ? VehicleOf(r.car) : null).Add("difficulty", new[] { "Easy", "Normal", "Hard" }[g.Difficulty])
                    .Add("timeOfDay", look ? $"Free Roam {look.Hour:0.0} h" : null).Add("weather", look ? look.RoamWeather.ToString() : null)
                    .Add("getaway", new ResultsLog.Json().Add("roundLimitSeconds", g.Limit).Add("durationSeconds", r != null && r.Done ? r.freeSeconds : g.Clock).Add("maxHeat", r?.topHeat ?? 0).Add("copsAtPeak", g.PeakCops)
                        .Add("outcome", r == null ? "not decided" : r.caught ? "busted" : r.limit ? "escaped (time limit)" : "escaped"));
                ResultsLog.Write(j);
            }
            catch (System.Exception e) { Debug.LogWarning("Results log line not made: " + e.Message); }
        }
    }
}
