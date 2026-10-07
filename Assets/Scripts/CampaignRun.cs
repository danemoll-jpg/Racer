using System.Linq;
using UnityEngine;

namespace Racer
{
    // 0.89 campaign, round 1: the event being run. Starting an event goes through the normal loading screen into the
    // event's course scene, where RaceFlow applies the event's settings (laps, rivals, difficulty, traffic, time of day,
    // weather, the chosen vehicle) and starts it through the normal race flow. Nothing the player has set for Race
    // is changed: QuitRace / Continue put Race back on the saved settings. While an event runs no Top 10 board, personal
    // best, ghost or activity record is written (campaign results go to the campaign save only).
    // 0.90: championship rounds run the same way (Active is the round, Active.Cup the championship); the player's
    // campaign upgrades are applied to their vehicle for campaign runs only; jump events.
    public static class CampaignRun
    {
        public static CampaignEvent Active { get; private set; }
        public static string Vehicle { get; private set; }
        public static bool PendingStart;
        public static Campaign.Outcome Last { get; private set; }
        public static string OpenedCourse { get; private set; }
        public static bool Done => Last != null;
        public static CampaignCup Cup => Active?.Cup;

        public static void Begin(CampaignEvent e, string vehicle)
        {
            RacePlaylists.Quit(); Active = e; Vehicle = vehicle; Last = null; PendingStart = true; Campaign.Selected = e.Id;
            OpenedCourse = Campaign.OpenCourse(e.Course);
        }
        // A championship round: the next one of the championship in progress (its saved vehicle).
        public static CampaignEvent RoundEvent(CampaignCup cup, int round)
        {
            var r = cup.Rounds[round];
            return new CampaignEvent { Id = $"{cup.Id}-r{round + 1}", Name = $"{cup.Name}: round {round + 1} of {cup.Rounds.Length}", Chapter = cup.AfterChapter, Kind = CampaignEventKind.Race,
                Course = r.Course, Laps = r.Laps, Rivals = cup.Rivals, Difficulty = cup.Difficulty, Time = r.Time, Weather = r.Weather, Cup = cup, Round = round, Entry = "Your championship vehicle" };
        }
        public static bool BeginCupRound(CampaignCup cup)
        {
            var progress = Campaign.ActiveCup(cup); if (progress == null || progress.races.Count >= cup.Rounds.Length) return false;
            Begin(RoundEvent(cup, progress.races.Count), progress.vehicle); Campaign.Selected = cup.Id; return true;
        }
        public static void End() { Active = null; Vehicle = null; Last = null; PendingStart = false; OpenedCourse = null; }
        // A retry: the same event again (a new run; its result counts as a replay once one has been recorded). Never for a
        // championship round.
        public static void Retry() { if (Cup == null) Last = null; }
        // The named rival for a championship round's roster slot (null outside a championship).
        public static string RivalName(int slot) => Cup != null && slot >= 0 && slot < CampaignData.RivalNames.Length ? CampaignData.RivalNames[slot] : null;

        public static void Configure(RaceDirector race)
        {
            var e = Active; if (e == null) return;
            race.opponents = e.Kind == CampaignEventKind.Race && e.Rivals.Length > 0;
            race.opponentRoster = race.opponents ? (string[])e.Rivals.Clone() : new[] { "tourer", "moto", "atv" };
            race.laps = e.Laps; race.difficulty = e.Difficulty; race.traffic = e.Traffic;
            var configuration = race.vehicle.GetComponent<VehicleConfiguration>();
            configuration.Apply(Vehicle);
            Campaign.ApplyUpgrades(configuration); // 0.90: campaign runs only
        }
        // Where a timed event starts: RunUp metres of road before the trap / jump or the start line (null = the normal start).
        public static bool StartStation(RaceDirector race, out float station)
        {
            station = 0; var e = Active; if (e == null || !race.road || e.Kind == CampaignEventKind.Race || e.RunUp <= 0) return false;
            if (e.Kind == CampaignEventKind.SpeedTrap || e.Kind == CampaignEventKind.Jump || e.Kind == CampaignEventKind.Smash)
            {
                var site = Site(race); if (!site) return false;
                station = race.road.Project(site.transform.position, out _) - e.RunUp;
            }
            else station = race.Origin - e.RunUp;
            return true;
        }
        public static ActivitySite Site(RaceDirector race) => Active?.Site == null ? null : race.Flow?.Activities?.Sites?.FirstOrDefault(s => s && s.id == Active.Site);

        // The run's result: a race when the classification is final, a time trial at its finish, a speed trap or jump at
        // the scored crossing / landing (score) or when its time runs out (no result). Committed once.
        public static Campaign.Outcome Finish(RaceFlow flow, float score = 0)
        {
            if (Active == null || Last != null) return Last;
            var race = flow.Race; var e = Active; var me = race.Racers[0];
            var o = new Campaign.Outcome { Event = e, Vehicle = race.vehicle.GetComponent<VehicleConfiguration>().profileId, Debug = flow.DebugMovementUsed, RaceSeconds = race.Progress.RaceTime(race.Clock) };
            if (e.Kind == CampaignEventKind.Race)
            {
                var order = race.Ordered(true); o.Field = order.Count; o.Place = order.IndexOf(me) + 1; o.Dnf = me.Dnf || !me.Classified;
                o.Time = o.Dnf ? 0 : me.ClassifiedTime(race.Clock);
                if (e.Cup != null) { o.Places = Places(race, order); Last = Campaign.CommitCupRace(o); return Last; }
            }
            else if (e.Kind == CampaignEventKind.TimeTrial) { o.Dnf = !me.Progress.Finished || me.Progress.LapTimes.Count == 0; o.Time = o.Dnf ? 0 : me.Progress.LapTimes[0]; }
            else { o.Dnf = score <= 0; o.Score = score; }
            Last = Campaign.Commit(o);
            return Last;
        }
        // Finishing places by driver (0 the player, 1-5 the rivals in roster order); 0 = did not finish.
        static int[] Places(RaceDirector race, System.Collections.Generic.List<RacerState> order)
        {
            var places = new int[6];
            for (int i = 0; i < order.Count; i++) { int driver = race.Racers.IndexOf(order[i]); if (driver >= 0 && driver < 6 && !order[i].Dnf && order[i].Classified) places[driver] = i + 1; }
            return places;
        }
        // Ending a championship round from the pause menu: it counts as did not finish for the player (no retries inside a
        // championship); the rivals keep their places from the running order. The championship goes on from the next race.
        public static Campaign.Outcome Forfeit(RaceFlow flow)
        {
            if (Cup == null || Last != null) return Last;
            var race = flow.Race; var order = race.Ordered(false).Where(r => r != race.Racers[0]).ToList();
            var places = new int[6]; for (int i = 0; i < order.Count; i++) { int driver = race.Racers.IndexOf(order[i]); if (driver > 0 && driver < 6) places[driver] = i + 1; }
            var o = new Campaign.Outcome { Event = Active, Vehicle = Vehicle, Dnf = true, Forfeit = true, Field = race.Racers.Count, Places = places, Debug = flow.DebugMovementUsed, RaceSeconds = race.Progress.RaceTime(race.Clock) };
            Last = Campaign.CommitCupRace(o); return Last;
        }
    }

    // Ends a speed-trap or jump event: the first scored crossing of the event's trap, or the first scored jump from its
    // site (shown for a moment, then the results), or no result when the time limit runs out. Added to the race flow
    // object while such an event runs.
    public sealed class CampaignTrapWatch : MonoBehaviour
    {
        RaceFlow flow; int awards; float endAt = -1, score; bool smashStarted;
        public void Initialize(RaceFlow owner) { flow = owner; awards = flow.Activities ? flow.Activities.Awards : 0; endAt = -1; score = 0; smashStarted = false; }
        void Update()
        {
            var e = CampaignRun.Active;
            if (!flow || e == null || (e.Kind != CampaignEventKind.SpeedTrap && e.Kind != CampaignEventKind.Jump && e.Kind != CampaignEventKind.Smash) || CampaignRun.Done) { Destroy(this); return; }
            if (flow.State != RaceFlow.Stage.Racing) return;
            var a = flow.Activities;
            // 0.90: a smash event: the site's attempt runs from GO for the time limit; its count (or all of them) ends it
            if (e.Kind == CampaignEventKind.Smash)
            {
                if (!smashStarted) { smashStarted = true; a.BeginCampaignSmash(CampaignRun.Site(flow.Race), e.TimeLimit); awards = a.Awards; }
                if (endAt < 0 && a.Awards != awards) { awards = a.Awards; score = a.LastSmashScore; endAt = Time.unscaledTime + 1.6f; }
                if (endAt < 0 && flow.Race.Progress.RaceTime(flow.Race.Clock) > e.TimeLimit + 1) { score = 0; endAt = Time.unscaledTime; flow.Notify("Time is up — nothing smashed", 3); }
                if (endAt >= 0 && Time.unscaledTime >= endAt) { CampaignRun.Finish(flow, score); flow.CompleteResults(); Destroy(this); }
                return;
            }
            if (endAt < 0 && a && a.Awards != awards)
            {
                awards = a.Awards;
                if (a.LastAwardSite && a.LastAwardSite.id == e.Site) { score = e.Kind == CampaignEventKind.Jump ? a.LastJumpAward : a.LastSpeed; endAt = Time.unscaledTime + 1.6f; }
            }
            if (endAt < 0 && flow.Race.Progress.RaceTime(flow.Race.Clock) > e.TimeLimit) { score = 0; endAt = Time.unscaledTime; flow.Notify(e.Kind == CampaignEventKind.Jump ? "Time is up — no scored jump" : "Time is up — no trap speed recorded", 3); }
            if (endAt >= 0 && Time.unscaledTime >= endAt) { CampaignRun.Finish(flow, score); flow.CompleteResults(); Destroy(this); }
        }
    }
}
