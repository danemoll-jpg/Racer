using System.Linq;
using UnityEngine;

namespace Racer
{
    // 0.89 campaign, round 1: the event being run. Starting an event goes through the normal loading screen into the
    // event's course scene, where RaceFlow applies the event's settings (laps, rivals, difficulty, traffic, time of day,
    // weather, the chosen vehicle, stock) and starts it through the normal race flow. Nothing the player has set for Race
    // is changed: QuitRace / Continue put Race back on the saved settings. While an event runs no Top 10 board, personal
    // best, ghost or activity record is written (campaign results go to the campaign save only).
    public static class CampaignRun
    {
        public static CampaignEvent Active { get; private set; }
        public static string Vehicle { get; private set; }
        public static bool PendingStart;
        public static Campaign.Outcome Last { get; private set; }
        public static string OpenedCourse { get; private set; }
        public static bool Done => Last != null;

        public static void Begin(CampaignEvent e, string vehicle)
        {
            RacePlaylists.Quit(); Active = e; Vehicle = vehicle; Last = null; PendingStart = true; Campaign.Selected = e.Id;
            OpenedCourse = Campaign.OpenCourse(e.Course);
        }
        public static void End() { Active = null; Vehicle = null; Last = null; PendingStart = false; OpenedCourse = null; }
        // A retry: the same event again (a new run; its result counts as a replay once one has been recorded).
        public static void Retry() { Last = null; }

        public static void Configure(RaceDirector race)
        {
            var e = Active; if (e == null) return;
            race.opponents = e.Kind == CampaignEventKind.Race && e.Rivals.Length > 0;
            race.opponentRoster = race.opponents ? (string[])e.Rivals.Clone() : new[] { "tourer", "moto", "atv" };
            race.laps = e.Laps; race.difficulty = e.Difficulty; race.traffic = e.Traffic;
            var configuration = race.vehicle.GetComponent<VehicleConfiguration>();
            if (configuration.profileId != Vehicle) configuration.Apply(Vehicle);
        }
        // Where a timed event starts: RunUp metres of road before the trap or the start line (null = the normal start).
        public static bool StartStation(RaceDirector race, out float station)
        {
            station = 0; var e = Active; if (e == null || !race.road || e.Kind == CampaignEventKind.Race || e.RunUp <= 0) return false;
            if (e.Kind == CampaignEventKind.SpeedTrap)
            {
                var site = Site(race); if (!site) return false;
                station = race.road.Project(site.transform.position, out _) - e.RunUp;
            }
            else station = race.Origin - e.RunUp;
            return true;
        }
        public static ActivitySite Site(RaceDirector race) => Active?.Site == null ? null : race.Flow?.Activities?.Sites?.FirstOrDefault(s => s && s.id == Active.Site);

        // The run's result: a race when the classification is final, a time trial at its finish, a speed trap at the
        // crossing (score) or when its time runs out (no result). Committed once.
        public static Campaign.Outcome Finish(RaceFlow flow, float trapSpeed = 0)
        {
            if (Active == null || Last != null) return Last;
            var race = flow.Race; var e = Active; var me = race.Racers[0];
            var o = new Campaign.Outcome { Event = e, Vehicle = race.vehicle.GetComponent<VehicleConfiguration>().profileId, Debug = flow.DebugMovementUsed };
            if (e.Kind == CampaignEventKind.Race)
            {
                var order = race.Ordered(true); o.Field = order.Count; o.Place = order.IndexOf(me) + 1; o.Dnf = me.Dnf || !me.Classified;
                o.Time = o.Dnf ? 0 : me.ClassifiedTime(race.Clock);
            }
            else if (e.Kind == CampaignEventKind.TimeTrial) { o.Dnf = !me.Progress.Finished || me.Progress.LapTimes.Count == 0; o.Time = o.Dnf ? 0 : me.Progress.LapTimes[0]; }
            else { o.Dnf = trapSpeed <= 0; o.Score = trapSpeed; }
            Last = Campaign.Commit(o);
            return Last;
        }
    }

    // Ends a speed-trap event: the first scored crossing of the event's trap (shown for a moment, then the results), or no
    // result when the time limit runs out. Added to the race flow object while a speed-trap event runs.
    public sealed class CampaignTrapWatch : MonoBehaviour
    {
        RaceFlow flow; int awards; float endAt = -1, speed;
        public void Initialize(RaceFlow owner) { flow = owner; awards = flow.Activities ? flow.Activities.Awards : 0; endAt = -1; speed = 0; }
        void Update()
        {
            var e = CampaignRun.Active;
            if (!flow || e == null || e.Kind != CampaignEventKind.SpeedTrap || CampaignRun.Done) { Destroy(this); return; }
            if (flow.State != RaceFlow.Stage.Racing) return;
            var a = flow.Activities;
            if (endAt < 0 && a && a.Awards != awards)
            {
                awards = a.Awards;
                if (a.LastAwardSite && a.LastAwardSite.id == e.Site) { speed = a.LastSpeed; endAt = Time.unscaledTime + 1.6f; }
            }
            if (endAt < 0 && flow.Race.Progress.RaceTime(flow.Race.Clock) > e.TimeLimit) { speed = 0; endAt = Time.unscaledTime; flow.Notify("Time is up — no trap speed recorded", 3); }
            if (endAt >= 0 && Time.unscaledTime >= endAt) { CampaignRun.Finish(flow, speed); flow.CompleteResults(); Destroy(this); }
        }
    }
}
