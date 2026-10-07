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
            RacePlaylists.Quit(); Active = e; Vehicle = vehicle; Last = null; PendingStart = true; Campaign.Selected = e.Id; Campaign.Drove(vehicle);
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
        // The named rival for a campaign race's roster slot (null outside the campaign). 0.94 Part A: every campaign race
        // (0.90-0.93: championships only), from the one cast (CampaignData.CastFor); RivalMember = the cast member (colour).
        public static int RivalMember(int slot) { var e = Active; if (e == null || e.Kind != CampaignEventKind.Race || slot < 0 || slot >= e.Rivals.Length) return -1; return CampaignData.CastFor(e.Rivals)[slot]; }
        public static string RivalName(int slot) { int m = RivalMember(slot); return m < 0 ? null : CampaignData.RivalNames[m]; }

        public static void Configure(RaceDirector race)
        {
            var e = Active; if (e == null) return;
            race.opponents = e.Kind == CampaignEventKind.Race && e.Rivals.Length > 0;
            race.opponentRoster = race.opponents ? (string[])e.Rivals.Clone() : new[] { "tourer", "moto", "atv" };
            // 0.91 Part C: a jump event has no laps (the player may go round and jump again until the time limit)
            race.laps = e.Kind == CampaignEventKind.Jump ? 0 : e.Laps; race.difficulty = e.Difficulty; race.traffic = e.Traffic;
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
        // 0.91 Part C: in a jump event the reset button puts the vehicle back at the run-up start, facing the jump.
        public static bool RunUpStart(RaceDirector race, out Vector3 position, out Quaternion rotation)
        {
            position = default; rotation = Quaternion.identity;
            if (Active?.Kind != CampaignEventKind.Jump || Done || !StartStation(race, out float station)) return false;
            var at = race.road.At(station, out var heading); var car = race.vehicle;
            position = at + Vector3.up * Mathf.Max(.4f, car.suspensionLength - Physics.gravity.magnitude / car.springStrength);
            rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(heading, Vector3.up)); return true;
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

    // Ends a speed-trap event at the first scored crossing of its trap (shown for a moment, then the results), or with no
    // result when the time limit runs out. 0.91 Part C: a jump event goes on until its time limit (or End event in the pause
    // menu), however many jumps the player makes: the best scored jump counts, a failed one never ends it. Added to the race
    // flow object while such an event runs.
    public sealed class CampaignTrapWatch : MonoBehaviour
    {
        RaceFlow flow; int awards, jumps; float endAt = -1, score; bool smashStarted;
        public float Best { get; private set; }
        public void Initialize(RaceFlow owner) { flow = owner; awards = flow.Activities ? flow.Activities.Awards : 0; endAt = -1; score = 0; smashStarted = false; Best = 0; jumps = 0; }
        // The jump event's HUD line: targets, best so far, time left, how to go again.
        public string Hud
        {
            get
            {
                var e = CampaignRun.Active; if (!flow || e == null || e.Kind != CampaignEventKind.Jump || CampaignRun.Done || flow.State != RaceFlow.Stage.Racing) return null;
                float left = Mathf.Max(0, e.TimeLimit - (float)flow.Race.Progress.RaceTime(flow.Race.Clock));
                string reset = MenuInput.Binding(flow.Race.vehicle.GetComponent<VehicleInput>().CurrentBindings[3]);
                return $"{e.Name.ToUpperInvariant()}  ·  {Campaign.TargetsText(e)}\nBest: {(Best > 0 ? Campaign.Measure(e, Best) + " " + new[] { "(no medal)", "BRONZE", "SILVER", "GOLD" }[Campaign.MedalFor(e, Best)] : "none yet")}  ·  {left:0} s left  ·  {MenuGlyph.Label(reset)}: back to the run-up";
            }
        }
        // Pause menu > End event: the best scored jump counts (none: no result).
        public void EndNow() { if (CampaignRun.Done) return; CampaignRun.Finish(flow, Best); flow.CompleteResults(); Destroy(this); }
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
                if (a.LastAwardSite && a.LastAwardSite.id == e.Site)
                {
                    if (e.Kind == CampaignEventKind.Jump) { jumps++; Best = Mathf.Max(Best, a.LastJumpAward); }
                    else { score = a.LastSpeed; endAt = Time.unscaledTime + 1.6f; }
                }
            }
            if (endAt < 0 && flow.Race.Progress.RaceTime(flow.Race.Clock) > e.TimeLimit)
            {
                score = e.Kind == CampaignEventKind.Jump ? Best : 0; endAt = Time.unscaledTime + (score > 0 ? 1.6f : 0);
                flow.Notify(e.Kind == CampaignEventKind.Jump ? (Best > 0 ? "Time is up — your best jump counts: " + Campaign.Measure(e, Best) : "Time is up — no scored jump") : "Time is up — no trap speed recorded", 3);
            }
            if (endAt >= 0 && Time.unscaledTime >= endAt) { CampaignRun.Finish(flow, score); flow.CompleteResults(); Destroy(this); }
        }
    }
}
