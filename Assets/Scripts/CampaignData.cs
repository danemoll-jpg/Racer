using System.Linq;

namespace Racer
{
    // 0.89 campaign, round 1: the chapters, their events, vehicle prices and prizes (data only; rules in Campaign.cs, the
    // running event in CampaignRun.cs). Ids are save identities: never rename one. Course numbers are RacePlaylists.Scenes.
    // Round 2 adds the events of chapters 2-4 and upgrades; the fields for them are here already.
    public enum CampaignEventKind { Race, TimeTrial, SpeedTrap }
    public sealed class CampaignEvent
    {
        public string Id, Name; public int Chapter; public CampaignEventKind Kind; public int Course, Laps = 1;
        public string[] Rivals = new string[0]; public int Difficulty; public TimeOfDay Time; public Weather Weather; public bool Traffic;
        // SpeedTrap: the activity site id; SpeedTrap and TimeTrial: metres of road before the trap / the start line where
        // the player starts (a flying lap), and the run's time limit after GO.
        public string Site; public float RunUp, TimeLimit;
        // Bronze / silver / gold: seconds for a time trial (lower is better), m/s for a speed trap.
        public float[] Targets;
        // Pay = first place / gold; Bonus = paid once, the first time it is won / golded; Prize = vehicle id (won with first).
        public int Pay, Bonus; public string Prize, Entry = "Any owned vehicle";
        public bool Final;
        public bool Timed => Kind != CampaignEventKind.Race;
        public string KindLabel => Kind switch { CampaignEventKind.Race => $"Race · {Laps} lap{(Laps == 1 ? "" : "s")} · {Rivals.Length} rivals ({new[] { "Easy", "Normal", "Hard" }[Difficulty]})", CampaignEventKind.TimeTrial => "Time trial · 1 flying lap", _ => "Speed trap · standing start" };
        public string Conditions => $"{Time} / {Weather}" + (Traffic ? " / traffic" : "");
        public string CourseTitle => RacePlaylists.Titles[Course].Replace(" - ", " — ");
    }
    public sealed class CampaignChapter { public int Number; public string Name, Opens; public int[] Courses; }

    public static class CampaignData
    {
        public static readonly CampaignChapter[] Chapters =
        {
            new() { Number = 1, Name = "Street Loop", Courses = new[] { 0, 1 }, Opens = "Open from the start" },
            new() { Number = 2, Name = "Forest Loop", Courses = new[] { 2, 3 }, Opens = "Finish top three in the Street Loop Final" },
            new() { Number = 3, Name = "Dan's Backyard", Courses = new[] { 6, 7 }, Opens = "Pass the Forest Loop final (chapter 2)" },
            new() { Number = 4, Name = "Mountain Loop", Courses = new[] { 4, 5 }, Opens = "Pass the Dan's Backyard final (chapter 3)" },
        };
        // Chapter 1, in order; each needs the one before it passed (top three in a race, bronze or better otherwise).
        // Targets (Docs/Report089/Lists/campaign-targets.txt): the trap from full-throttle runs of the starters from the
        // event's start (Street Classic 41.1, Trail Four 46.1 m/s): gold needs the Trail Four or better flat out; the time
        // trial from flying laps by the race AI (Hard: Street Classic 2:13.5, Trail Four 2:12.2; Normal 2:18.5 / 2:16.4).
        public static readonly CampaignEvent[] Events =
        {
            new() { Id = "c1-first-lap", Name = "First Lap", Chapter = 1, Kind = CampaignEventKind.Race, Course = 0, Laps = 2, Rivals = new[] { "original", "pebble", "roadster" }, Difficulty = 0, Pay = 1200, Bonus = 600 },
            new() { Id = "c1-hwy92-trap", Name = "Hwy 92 Speed Trap", Chapter = 1, Kind = CampaignEventKind.SpeedTrap, Course = 0, Site = "speed-0", RunUp = 200, TimeLimit = 45, Targets = new[] { 34f, 39f, 44f }, Pay = 1000, Bonus = 500 },
            new() { Id = "c1-against-clock", Name = "Against the Clock", Chapter = 1, Kind = CampaignEventKind.TimeTrial, Course = 0, RunUp = 150, Targets = new[] { 150f, 140f, 134f }, Pay = 1000, Bonus = 500 },
            new() { Id = "c1-night-shift", Name = "Night Shift", Chapter = 1, Kind = CampaignEventKind.Race, Course = 0, Laps = 3, Rivals = new[] { "roadster", "tourer", "atv" }, Difficulty = 0, Time = TimeOfDay.Night, Traffic = true, Pay = 1400, Bonus = 700 },
            new() { Id = "c1-wrong-way", Name = "Wrong Way Round", Chapter = 1, Kind = CampaignEventKind.Race, Course = 1, Laps = 2, Rivals = new[] { "pebble", "skyfin", "atv" }, Difficulty = 1, Time = TimeOfDay.Dusk, Weather = Weather.Rain, Pay = 1400, Bonus = 700 },
            new() { Id = "c1-street-final", Name = "Street Loop Final", Chapter = 1, Kind = CampaignEventKind.Race, Course = 0, Laps = 3, Rivals = new[] { "atv", "tourer", "roadster", "pebble", "drifter" }, Difficulty = 1, Pay = 2500, Bonus = 1500, Prize = "moto", Final = true },
        };
        // Share of Pay by finishing place (every finisher earns something) and by medal (none, bronze, silver, gold).
        public static readonly float[] PlaceShare = { 1f, .6f, .4f, .25f, .15f, .1f };
        public static readonly float[] MedalShare = { 0f, .4f, .65f, 1f };
        public const float ReplayShare = .5f;
        public static readonly string[] Starters = { "original", "atv" };
        // Shop prices (dollars) by how good the vehicle is; Chapter = the chapter that must be open before it can be bought.
        // Prize vehicles are not sold; the mower is the acorn reward, outside the shop and the campaign.
        public static readonly (string id, int price, int chapter)[] Prices =
        {
            ("pebble", 6000, 1), ("roadster", 7000, 1), ("skyfin", 8000, 1), ("tourer", 10000, 1), ("drifter", 16000, 2),
        };
        // Prize vehicles of later chapters (their events arrive in round 2): defined now so the shop can say where they come from.
        public static readonly (string id, string from)[] LaterPrizes = { ("scrambler", "the chapter 3 (Dan's Backyard) events"), ("fastback", "the chapter 4 (Mountain Loop) events") };
        public static CampaignEvent Find(string id) => Events.FirstOrDefault(e => e.Id == id);
        public static CampaignEvent[] InChapter(int chapter) => Events.Where(e => e.Chapter == chapter).ToArray();
    }
}
