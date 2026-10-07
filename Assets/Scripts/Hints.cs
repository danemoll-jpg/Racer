using System.Linq;
using UnityEngine;

namespace Racer
{
    // 0.90 Part B: new-player hints. Each hint is shown once (its id is remembered in settings.json, hintsSeen), small and
    // timed, never blocking; Settings > Gameplay has Hints On / Off and Show hints again. Never in Trailer Mode or split-
    // screen. The welcome panel and the controls card are RaceMenus pages driven from here; the in-play hints are watched
    // by HintWatch on the race flow object.
    public static class Hints
    {
        public static RaceFlow Flow;
        public static string Text { get; private set; }
        public static float Until { get; private set; }
        static RacerSave.Options Settings => Flow && Flow.Save != null ? Flow.Save.Settings : null;
        public static bool Enabled => Settings != null && Settings.hints && !TrailerMode.Active && !SplitScreen.Active;
        public static bool Seen(string id) => Settings?.hintsSeen != null && Settings.hintsSeen.Contains(id);
        public static void MarkSeen(string id)
        {
            var s = Settings; if (s == null || Seen(id)) return;
            s.hintsSeen = (s.hintsSeen ?? new string[0]).Append(id).ToArray(); Flow.Save.SaveSettings();
        }
        static readonly System.Collections.Generic.Queue<(string text, float seconds)> queue = new();
        public static void Show(string id, string text, float seconds = 7)
        {
            if (!Enabled || Seen(id)) return;
            MarkSeen(id);
            if (Text != null && Time.unscaledTime < Until) { queue.Enqueue((text, seconds)); return; }
            Text = text; Until = Time.unscaledTime + seconds;
        }
        // One at a time: the next waiting hint follows when the shown one ends.
        public static void Tick() { if (queue.Count > 0 && (Text == null || Time.unscaledTime >= Until)) { var (t, s) = queue.Dequeue(); Text = t; Until = Time.unscaledTime + s; } }
        public static bool Visible => Text != null && Time.unscaledTime < Until && !TrailerMode.Active && !SplitScreen.Active;
        public static void Reset() { var s = Settings; if (s == null) return; s.hintsSeen = new string[0]; Flow.Save.SaveSettings(); Text = null; queue.Clear(); }
        public static bool AnySeen => Settings?.hintsSeen != null && Settings.hintsSeen.Length > 0;

        // A new player: no campaign progress yet (Dan's save has progress, so he never sees it unless he asks again).
        public static bool WelcomeDue => Enabled && !Seen("welcome") && !Campaign.HasProgress && !Campaign.Testing;
        // Before the first event a new player starts (a player who has already run campaign events is not new).
        public static bool ControlsDue => Enabled && !Seen("controls") && !Flow.Race.FreeRoam && !Campaign.Current.results.Any(r => r.runs > 0);

        public static void LockedItem() => Show("locked", "Locked: each one says how to get it. Win campaign events to open tracks and prizes, and earn money to buy vehicles in the Shop.");
        public static void Money() => Show("money", "You earned money. Spend it in the Shop (Campaign > Shop, or Garage > Shop) on vehicles and upgrades.");
        public static void Unlocked() => Show("unlocked", "Something new is open: new tracks are in Race > Tracks, new vehicles in the Garage.");
        public static void FreeRoam()
        {
            var map = Flow.GetComponent<ExplorationMap>(); string m = MenuInput.Controller ? "View" : "M", j = MenuInput.Controller ? "B" : "J";
            Show("roam", $"Free Roam: the whole world is yours. {m}: map and waypoints  ·  {j}: minimap on / off  ·  drive through a speed trap or land a jump to score.", 10);
        }
    }

    // The in-play hints that need watching: stuck or off the course for a few seconds, and the first shortcut ahead.
    public sealed class HintWatch : MonoBehaviour
    {
        RaceFlow flow; float stuck; string resetLabel;
        public void Initialize(RaceFlow owner) { flow = owner; }
        void Update()
        {
            if (!flow || !Hints.Enabled || flow.State != RaceFlow.Stage.Racing || flow.Race.FreeRoam || flow.Race.Progress.Finished) { stuck = 0; return; }
            var race = flow.Race; var car = race.vehicle; if (!car) return;
            if (Time.unscaledTime - flow.SessionStartedAt < 8) return;
            float lateral = 0; if (race.road) { float s = race.road.Project(car.Body.position, out lateral); lateral -= race.road.HalfWidth(s); }
            bool wrong = race.GetComponent<WrongWayGuidance>()?.Visible == true;
            bool off = race.Racers[0].Branch.Route == null && lateral > 14;
            bool slow = Mathf.Abs(car.ForwardSpeed) < 1.5f && race.Progress.LapActive;
            stuck = wrong || off || slow ? stuck + Time.deltaTime : 0;
            if (stuck > 4)
            {
                var input = car.GetComponent<VehicleInput>(); resetLabel = input ? input.ResetControlLabel : "R";
                Hints.Show("stuck", $"Stuck or off the course? Press {resetLabel} to go back to the nearest point of the track.");
            }
            if (!Hints.Seen("shortcut") && race.Branches != null)
                foreach (var b in race.Branches)
                {
                    if (!b || !b.isActiveAndEnabled || b.points == null || b.points.Length < 2) continue;
                    var entry = b.points[0]; var to = entry - car.Body.position; to.y = 0;
                    if (to.magnitude < 70 && Vector3.Dot(to, car.transform.forward) > 0)
                    { Hints.Show("shortcut", "A shortcut is coming up: the OPTIONAL SHORTCUT signs mark it, and the minimap shows it in gold. Take it if you dare."); break; }
                }
        }
    }
}
