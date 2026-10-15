using System.Linq;
using UnityEngine;

namespace Racer
{
    // 0.119 Part B: Stunt Races in the menus. Race Setup: "Race type" (Normal race / Stunt Race, shown once a stunt vehicle is owned: the Clay Hopper
    // is from the start) and its "Scoring" (Trick bonus / Split score); the rivals' row says they ride the stunt vehicles. Tracks lists the four Stunt
    // Race courses while it is on; the garage offers the stunt vehicles (RaceDirector.stunts). Records: the four Stunt Race tracks after the
    // normal ones (their own boards and best combo). Results: the trick bonus or the split score beside the times.
    public sealed partial class RaceMenus
    {
        void StuntSetupRows()
        {
            if (!StuntRace.Offered) return;
            bool stunt = flow.Race.stunts;
            Step(12, "race-type", "Race type:   " + (stunt ? "Stunt Race   (stunt vehicles only)" : "Normal race"), d => flow.StepRaceType(d));
            buttons[12].transform.SetSiblingIndex(buttons[1].transform.GetSiblingIndex());
            if (!stunt) return;
            Step(13, "stunt-scoring", "Scoring:   " + (flow.Race.splitScore ? "Split score   (finish + stunt points)" : "Trick bonus   (time off for tricks)"), d => { flow.StepStuntScoring(d); Show(); });
            buttons[13].transform.SetSiblingIndex(buttons[12].transform.GetSiblingIndex() + 1);
            Row(3, "opponents", "Rivals:   on stunt vehicles (" + string.Join(", ", StuntRace.All.Where(p => !p.CupEdition).Select(p => p.Name)) + ")", () => { flow.Notify("In a Stunt Race every rival rides a stunt vehicle", 3); flow.Click(); });
            details.text += "\n" + StuntRace.Rules + "\n" + StuntRace.ScoringText(flow.Race.splitScore);
            details.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = Mathf.Max(details.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight, 190);
        }
        // ---------- Records: the Stunt Race tracks after the normal ones ----------
        static string StuntTitle(int course) => "Stunt Race · " + RacePlaylists.Titles[course];
        static string RecordTitle(int i) => i < RacePlaylists.Titles.Length ? RacePlaylists.Titles[i] : StuntTitle(StuntRace.Courses[i - RacePlaylists.Titles.Length]);
        static int RecordTracks => RacePlaylists.Titles.Length + StuntRace.Courses.Length;
        static System.Collections.Generic.List<int> RecordOrder => RacePlaylists.DisplayOrder.Concat(Enumerable.Range(RacePlaylists.Titles.Length, StuntRace.Courses.Length)).ToList();
        static int RecordIndex(string title) { for (int i = 0; i < RecordTracks; i++) if (RecordTitle(i) == title) return i; return -1; }
        static bool StuntTrack(int i) => i >= RacePlaylists.Titles.Length;
        string StuntRecordLine(int i)
        {
            int course = StuntRace.Courses[i - RacePlaylists.Titles.Length]; string key = StuntRace.RecordPrefix + CourseIdOf(course);
            var best = key == null ? null : StuntRaceRecords.Best(key);
            return best == null ? "\nBest combo: none yet" : $"\nBest combo: {best.points:N0} pts  ·  {best.name}  ·  {VehicleProfile.Find(best.vehicle).Name}  ·  {RecordDate(best.date)}";
        }
        // the course id a track races under (its records): the scene's for the Mountain Loops, the course file's for Leviathan
        static string CourseIdOf(int course)
        {
            string scene = RacePlaylists.Scenes[course];
            if (LeviathanCourse.Is(scene)) return LeviathanCourse.Load(scene)?.id;
            return scene == "MountainLoop" ? "mountain-forward-v5-supported" : scene == "MountainLoopReverse" ? "mountain-reverse-v4-natural-summit-merge" : scene;
        }
        // ---------- Results ----------
        // single player and split screen: the trick bonus (or the split score) beside each racer's time
        int StuntStandings(int n, bool split)
        {
            var race = flow.Race; var score = StuntRaceScore.Current;
            string Who(RacerState r) { bool p1 = r == race.Racers[0], p2 = SplitScreen.Race && r == SplitScreen.Race.P2; return (split ? p1 ? SplitScreen.NameOf(1) : p2 ? SplitScreen.P2Name : r.Name : r.IsAi ? (CampaignRun.Active != null ? r.Name : "Rival " + race.Racers.IndexOf(r)) : PlayerNames.Player) + " · " + VehicleProfile.Find(r.Car.GetComponent<VehicleConfiguration>().profileId).Name; }
            bool Mine(RacerState r) => !r.IsAi || split && SplitScreen.Race && r == SplitScreen.Race.P2;
            if (race.splitScore && score)
            {
                var w = new[] { .08f, .34f, .2f, .2f, .18f };
                TableRow(n++, "header", new[] { "Place", "Driver", "Finish", "Stunt points", "Points" }, w, () => { });
                int place = 0;
                foreach (var l in score.SplitTable(true))
                {
                    place++; var r = l.racer;
                    string fin = l.finish > 0 ? Campaign.Ordinal(l.finish) + "  " + RaceHud.FormatTime(r.ClassifiedTime(race.Clock)) : "DNF";
                    TableRow(n++, "standing-" + place, new[] { place.ToString(), Who(r), fin + $"  +{StuntRaceScore.Points(l.finish)}", $"{r.Progress.StuntPoints:N0}  ({Campaign.Ordinal(l.stunt)})  +{StuntRaceScore.Points(l.stunt)}", l.total.ToString() }, w, () => { }, Mine(r));
                }
                return n;
            }
            var widths = new[] { .08f, .32f, .2f, .22f, .18f };
            TableRow(n++, "header", new[] { "Place", "Driver", "Time", "Tricks", "Status" }, widths, () => { }); int rank = 0;
            foreach (var r in race.Ordered(true))
            {
                int place = ++rank; var p = r.Progress;
                TableRow(n++, "standing-" + place, new[] { place.ToString(), Who(r), r.Dnf ? "—" : RaceHud.FormatTime(r.ClassifiedTime(race.Clock)), p.StuntPoints > 0 ? $"−{p.BonusSeconds:0.0} s · {p.StuntPoints:N0} pts" : "—", r.Dnf ? "DNF" : r.Estimated ? "Estimated" : "Measured" }, widths, () => { }, Mine(r));
            }
            return n;
        }
    }
}
