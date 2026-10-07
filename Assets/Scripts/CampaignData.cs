using System.Linq;

namespace Racer
{
    // 0.89 campaign, round 1: the chapters, their events, vehicle prices and prizes (data only; rules in Campaign.cs, the
    // running event in CampaignRun.cs). Ids are save identities: never rename one. Course numbers are RacePlaylists.Scenes.
    // 0.90 (round 2): chapters 2-4, the championships (one opens when each chapter's final is passed), jump events, class
    // entry rules and the upgrade tables.
    public enum CampaignEventKind { Race, TimeTrial, SpeedTrap, Jump, Smash }
    public sealed class CampaignEvent
    {
        public string Id, Name; public int Chapter; public CampaignEventKind Kind; public int Course, Laps = 1;
        public string[] Rivals = new string[0]; public int Difficulty; public TimeOfDay Time; public Weather Weather; public bool Traffic;
        // SpeedTrap / Jump: the activity site id; SpeedTrap, Jump and TimeTrial: metres of road before the site / the start
        // line where the player starts (a flying lap), and the run's time limit after GO.
        public string Site; public float RunUp, TimeLimit;
        // Bronze / silver / gold: seconds for a time trial (lower is better), m/s for a speed trap, metres for a jump, props
        // for a smash.
        public float[] Targets;
        // Pay = first place / gold; Bonus = paid once, the first time it is won / golded; Prize = vehicle id (0.92: awarded when the event is passed: every chapter final has one).
        public int Pay, Bonus; public string Prize, Entry = "Any owned vehicle";
        // 0.90: vehicle classes allowed (null = any): "Two Wheels Only" takes motorcycles and the ATV.
        public string[] Classes;
        public bool Final;
        // 0.90: a championship round run as an event (CampaignRun builds it; never in Events).
        public CampaignCup Cup; public int Round;
        public bool Timed => Kind != CampaignEventKind.Race;
        public bool Allows(VehicleProfile p) => Classes == null || Classes.Contains(p.Class);
        public string KindLabel => Kind switch { CampaignEventKind.Race => $"Race · {Laps} lap{(Laps == 1 ? "" : "s")} · {Rivals.Length} rivals ({new[] { "Easy", "Normal", "Hard" }[Difficulty]})", CampaignEventKind.TimeTrial => "Time trial · 1 flying lap", CampaignEventKind.Jump => "Jump · run-up start", CampaignEventKind.Smash => "Smash · as many props as you can", _ => "Speed trap · standing start" };
        public string Conditions => $"{Time} / {Weather}" + (Traffic ? " / traffic" : "");
        public string CourseTitle => RacePlaylists.Titles[Course].Replace(" - ", " — ");
    }
    public sealed class CampaignChapter { public int Number; public string Name, Opens; public int[] Courses; }
    // 0.90: a championship. Rounds on the courses opened so far, the same five named rivals in every round, points per
    // place (CampaignData.CupPoints), paid by final position (Pay = first, then PlaceShare), a first-win bonus.
    public sealed class CampaignCupRound { public int Course, Laps = 2; public TimeOfDay Time; public Weather Weather; public string Conditions => $"{Time} / {Weather}"; }
    public sealed class CampaignCup
    {
        public string Id, Name; public int AfterChapter; public CampaignCupRound[] Rounds; public string[] Rivals; public int Difficulty, Pay, Bonus; public bool Grand;
        public string Opens => $"Pass the chapter {AfterChapter} final";
    }

    public static class CampaignData
    {
        public static readonly CampaignChapter[] Chapters =
        {
            new() { Number = 1, Name = "Street Loop", Courses = new[] { 0, 1 }, Opens = "Open from the start" },
            new() { Number = 2, Name = "Forest Loop", Courses = new[] { 2, 3 }, Opens = "Finish top three in the Street Loop Final" },
            new() { Number = 3, Name = "Dan's Backyard", Courses = new[] { 6, 7 }, Opens = "Pass the Forest Final (chapter 2)" },
            new() { Number = 4, Name = "Mountain Loop", Courses = new[] { 4, 5 }, Opens = "Pass the Backyard Final (chapter 3)" },
        };
        static readonly string[] TwoWheels = { "Motorcycle", "ATV" };
        // Each chapter in order; each event needs the one before it passed (top three in a race, bronze or better otherwise).
        // Chapter 1 targets (Docs/Report089/Lists/campaign-targets.txt): the trap from full-throttle runs of the starters from
        // the event's start (Street Classic 41.1, Trail Four 46.1 m/s); the time trial from flying laps by the race AI.
        // Chapters 2-4 targets: Docs/Report090/Lists/campaign-targets.txt (measured the same way with vehicles a player could
        // own by then: the starters, the Needle 600 and a bought car).
        public static readonly CampaignEvent[] Events =
        {
            new() { Id = "c1-first-lap", Name = "First Lap", Chapter = 1, Kind = CampaignEventKind.Race, Course = 0, Laps = 2, Rivals = new[] { "original", "pebble", "roadster" }, Difficulty = 0, Pay = 1200, Bonus = 600 },
            new() { Id = "c1-hwy92-trap", Name = "Hwy 92 Speed Trap", Chapter = 1, Kind = CampaignEventKind.SpeedTrap, Course = 0, Site = "speed-0", RunUp = 200, TimeLimit = 45, Targets = new[] { 34f, 39f, 44f }, Pay = 1000, Bonus = 500 },
            new() { Id = "c1-against-clock", Name = "Against the Clock", Chapter = 1, Kind = CampaignEventKind.TimeTrial, Course = 0, RunUp = 150, Targets = new[] { 150f, 140f, 134f }, Pay = 1000, Bonus = 500 },
            new() { Id = "c1-night-shift", Name = "Night Shift", Chapter = 1, Kind = CampaignEventKind.Race, Course = 0, Laps = 3, Rivals = new[] { "roadster", "tourer", "atv" }, Difficulty = 0, Time = TimeOfDay.Night, Traffic = true, Pay = 1400, Bonus = 700 },
            new() { Id = "c1-wrong-way", Name = "Wrong Way Round", Chapter = 1, Kind = CampaignEventKind.Race, Course = 1, Laps = 2, Rivals = new[] { "pebble", "skyfin", "atv" }, Difficulty = 1, Time = TimeOfDay.Dusk, Weather = Weather.Rain, Pay = 1400, Bonus = 700 },
            new() { Id = "c1-street-final", Name = "Street Loop Final", Chapter = 1, Kind = CampaignEventKind.Race, Course = 0, Laps = 3, Rivals = new[] { "atv", "tourer", "roadster", "pebble", "drifter" }, Difficulty = 1, Pay = 2500, Bonus = 1500, Prize = "moto", Final = true },

            // Chapter 2, Forest Loop (Normal rivals). 0.91: the jump events' targets and time limits re-set from flat-out runs now
            // that a jump counts when the vehicle survives it (several attempts per run, the best counts).
            new() { Id = "c2-into-woods", Name = "Into the Woods", Chapter = 2, Kind = CampaignEventKind.Race, Course = 2, Laps = 2, Rivals = new[] { "tourer", "pebble", "atv" }, Difficulty = 1, Pay = 1500, Bonus = 750 },
            new() { Id = "c2-cave-run", Name = "Cave Run", Chapter = 2, Kind = CampaignEventKind.TimeTrial, Course = 2, RunUp = 150, Targets = new[] { 76f, 69f, 64f }, Pay = 1300, Bonus = 650 },
            new() { Id = "c2-opening-jump", Name = "The Opening Jump", Chapter = 2, Kind = CampaignEventKind.Jump, Course = 2, Site = "jump-01", RunUp = 150, TimeLimit = 120, Targets = new[] { 30f, 60f, 110f }, Pay = 1300, Bonus = 650 },
            new() { Id = "c2-rain-pines", Name = "Rain in the Pines", Chapter = 2, Kind = CampaignEventKind.Race, Course = 2, Laps = 3, Rivals = new[] { "skyfin", "drifter", "atv" }, Difficulty = 1, Time = TimeOfDay.Dusk, Weather = Weather.Rain, Pay = 1700, Bonus = 850 },
            new() { Id = "c2-long-way-back", Name = "The Long Way Back", Chapter = 2, Kind = CampaignEventKind.Race, Course = 3, Laps = 2, Rivals = new[] { "roadster", "drifter", "atv" }, Difficulty = 1, Pay = 1700, Bonus = 850 },
            new() { Id = "c2-forest-final", Name = "Forest Final", Chapter = 2, Kind = CampaignEventKind.Race, Course = 2, Laps = 3, Rivals = new[] { "drifter", "tourer", "atv", "moto", "skyfin" }, Difficulty = 1, Pay = 2800, Bonus = 1400, Prize = "pebble", Final = true },

            // Chapter 3, Dan's Backyard (Normal, then Hard)
            new() { Id = "c3-backyard-dash", Name = "Backyard Dash", Chapter = 3, Kind = CampaignEventKind.Race, Course = 6, Laps = 2, Rivals = new[] { "moto", "atv", "drifter" }, Difficulty = 1, Pay = 1800, Bonus = 900 },
            new() { Id = "c3-backyard-clock", Name = "Round the Yard", Chapter = 3, Kind = CampaignEventKind.TimeTrial, Course = 6, RunUp = 150, Targets = new[] { 72f, 64f, 59f }, Pay = 1600, Bonus = 800 },
            new() { Id = "c3-two-wheels", Name = "Two Wheels Only", Chapter = 3, Kind = CampaignEventKind.Race, Course = 7, Laps = 2, Rivals = new[] { "moto", "drifter", "atv" }, Difficulty = 1, Classes = TwoWheels, Entry = "Motorcycles and the ATV only", Pay = 2000, Bonus = 1000 },
            new() { Id = "c3-snow-day", Name = "Snow Day", Chapter = 3, Kind = CampaignEventKind.Race, Course = 6, Laps = 2, Rivals = new[] { "atv", "moto", "tourer" }, Difficulty = 2, Weather = Weather.Snow, Pay = 2000, Bonus = 1000 },
            new() { Id = "c3-fence-smash", Name = "Fence Line Smash", Chapter = 3, Kind = CampaignEventKind.Smash, Course = 6, Site = "smash-01", RunUp = 60, TimeLimit = 45, Targets = new[] { 3f, 6f, 10f }, Pay = 1600, Bonus = 800 },
            new() { Id = "c3-backyard-final", Name = "Backyard Final", Chapter = 3, Kind = CampaignEventKind.Race, Course = 6, Laps = 3, Rivals = new[] { "moto", "drifter", "atv", "tourer", "fastback" }, Difficulty = 2, Pay = 3400, Bonus = 1700, Prize = "scrambler", Final = true },

            // Chapter 4, Mountain Loop (Hard: the campaign's hardest)
            new() { Id = "c4-first-ascent", Name = "First Ascent", Chapter = 4, Kind = CampaignEventKind.Race, Course = 4, Laps = 2, Rivals = new[] { "scrambler", "atv", "tourer" }, Difficulty = 2, Time = TimeOfDay.Dawn, Pay = 2200, Bonus = 1100 },
            new() { Id = "c4-summit-clock", Name = "Summit Clock", Chapter = 4, Kind = CampaignEventKind.TimeTrial, Course = 4, RunUp = 150, Targets = new[] { 134f, 124f, 118f }, Pay = 2000, Bonus = 1000 },
            new() { Id = "c4-downhill", Name = "Downhill", Chapter = 4, Kind = CampaignEventKind.Race, Course = 5, Laps = 2, Rivals = new[] { "moto", "scrambler", "drifter" }, Difficulty = 2, Pay = 2400, Bonus = 1200 },
            new() { Id = "c4-whiteout", Name = "Whiteout", Chapter = 4, Kind = CampaignEventKind.Race, Course = 4, Laps = 2, Rivals = new[] { "atv", "scrambler", "fastback" }, Difficulty = 2, Time = TimeOfDay.Night, Weather = Weather.Snow, Pay = 2600, Bonus = 1300 },
            new() { Id = "c4-summit-flight", Name = "Summit Homeward Flight", Chapter = 4, Kind = CampaignEventKind.Jump, Course = 4, Site = "summit-homeward", RunUp = 250, TimeLimit = 150, Targets = new[] { 90f, 140f, 200f }, Pay = 2000, Bonus = 1000 },
            new() { Id = "c4-summit-final", Name = "Summit Final", Chapter = 4, Kind = CampaignEventKind.Race, Course = 4, Laps = 3, Rivals = new[] { "moto", "scrambler", "drifter", "fastback", "atv" }, Difficulty = 2, Pay = 4200, Bonus = 2100, Prize = "fastback", Final = true },
        };
        // Share of Pay by finishing place (every finisher earns something) and by medal (none, bronze, silver, gold).
        public static readonly float[] PlaceShare = { 1f, .6f, .4f, .25f, .15f, .1f };
        public static readonly float[] MedalShare = { 0f, .4f, .65f, 1f };
        public const float ReplayShare = .5f;
        public static readonly string[] Starters = { "original", "atv" };
        // Shop prices (dollars) by how good the vehicle is; Chapter = the chapter that must be open before it can be bought.
        // Prize vehicles are not sold; the mower is the acorn reward, outside the shop and the campaign.
        // 0.92 Part D: every chapter final awards a vehicle, so the Pebble Coupe (chapter 2's prize) left the Shop; four are
        // bought: Sundown Roadster, Skyfin Cruiser, Longroof GT, Drifter Twin.
        public static readonly (string id, int price, int chapter)[] Prices =
        {
            ("roadster", 7000, 1), ("skyfin", 8000, 1), ("tourer", 10000, 1), ("drifter", 16000, 2),
        };
        // 0.92 Part D: the vehicles that were sold before and are now prizes, with what a save that bought one paid (refunded).
        public static readonly (string id, int price)[] FormerlySold = { ("pebble", 6000) };
        public static CampaignEvent Find(string id) => Events.FirstOrDefault(e => e.Id == id);
        public static CampaignEvent[] InChapter(int chapter) => Events.Where(e => e.Chapter == chapter).ToArray();

        // ---------- 0.90 championships ----------
        // The same five named rivals in every championship (in roster order).
        public static readonly string[] RivalNames = { "Rusty Vance", "Mia Torres", "Big Ed Kowalski", "June Park", "Hollis Gray" };
        // Points for places 1-6 (a did-not-finish scores nothing).
        public static readonly int[] CupPoints = { 10, 7, 5, 3, 2, 1 };
        static CampaignCupRound R(int course, int laps, TimeOfDay time = TimeOfDay.Day, Weather weather = Weather.Clear) => new() { Course = course, Laps = laps, Time = time, Weather = weather };
        public static readonly CampaignCup[] Cups =
        {
            new() { Id = "cup-street", Name = "Street Cup", AfterChapter = 1, Difficulty = 1, Pay = 4000, Bonus = 2000,
                Rivals = new[] { "pebble", "roadster", "skyfin", "tourer", "atv" },
                Rounds = new[] { R(0, 2), R(1, 2, TimeOfDay.Dusk), R(0, 3) } },
            new() { Id = "cup-woodland", Name = "Woodland Cup", AfterChapter = 2, Difficulty = 1, Pay = 6000, Bonus = 3000,
                Rivals = new[] { "tourer", "drifter", "atv", "roadster", "skyfin" },
                Rounds = new[] { R(0, 2), R(2, 2), R(1, 2, TimeOfDay.Night), R(3, 2, TimeOfDay.Dusk, Weather.Rain) } },
            new() { Id = "cup-backyard", Name = "Backyard Cup", AfterChapter = 3, Difficulty = 2, Pay = 8000, Bonus = 4000,
                Rivals = new[] { "moto", "drifter", "atv", "tourer", "scrambler" },
                Rounds = new[] { R(0, 2), R(2, 2, TimeOfDay.Dawn), R(6, 2), R(1, 2, TimeOfDay.Night), R(3, 2, TimeOfDay.Day, Weather.Rain), R(7, 2, TimeOfDay.Day, Weather.Snow) } },
            new() { Id = "cup-grand", Name = "Woodstock Grand Championship", AfterChapter = 4, Difficulty = 2, Pay = 12000, Bonus = 6000, Grand = true,
                Rivals = new[] { "moto", "drifter", "fastback", "scrambler", "atv" },
                Rounds = new[] { R(0, 2), R(2, 2), R(6, 2, TimeOfDay.Dusk), R(4, 2, TimeOfDay.Dawn), R(1, 2, TimeOfDay.Night, Weather.Rain), R(3, 2, TimeOfDay.Day, Weather.Snow), R(7, 2), R(5, 2, TimeOfDay.Dusk) } },
        };
        public static CampaignCup FindCup(string id) => Cups.FirstOrDefault(c => c.Id == id);
        public static CampaignCup[] CupsAfter(int chapter) => Cups.Where(c => c.AfterChapter == chapter).ToArray();

        // ---------- 0.90 upgrades ----------
        // Three levels each; a level adds Step of the stock value (Top speed +3%, Acceleration +6%, Grip +5%, Handling +6%),
        // so a fully upgraded starter is competitive late on but never beats the best vehicles fully upgraded (they gain the
        // same share). Level n costs CostShare[n-1] of the vehicle's value (its price; the starters and prizes have values).
        public static readonly string[] UpgradeNames = { "Top speed", "Acceleration", "Grip", "Handling" };
        public static readonly float[] UpgradeStep = { .03f, .06f, .05f, .06f };
        public static readonly float[] UpgradeCostShare = { .10f, .18f, .28f };
        public const int UpgradeLevels = 3;
        static readonly (string id, int value)[] Values = { ("original", 4000), ("atv", 5000), ("pebble", 6000), ("moto", 15000), ("scrambler", 13000), ("fastback", 14000) };
        public static int VehicleValue(string id) { var p = Prices.FirstOrDefault(x => x.id == id); if (p.id != null) return p.price; var v = Values.FirstOrDefault(x => x.id == id); return v.id != null ? v.value : 0; }
        public static int UpgradeCost(string id, int level) => level < 1 || level > UpgradeLevels ? 0 : (int)(System.Math.Round(VehicleValue(id) * UpgradeCostShare[level - 1] / 50.0) * 50);
    }
}
