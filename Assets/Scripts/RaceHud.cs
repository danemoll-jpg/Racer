using UnityEngine;

namespace Racer
{
    public sealed class RaceHud : MonoBehaviour
    {
        public RaceDirector race;
        public UnityEngine.UI.Text display;
        public static string FormatTime(double seconds)
        { int ms = (int)(seconds * 1000); return $"{ms / 60000:00}:{ms / 1000 % 60:00}.{ms % 1000:000}"; }
        public string BuildText()
        {
            var p = race.Progress;
            string next = p.NextGate == 0 ? "START / FINISH" : $"CP {p.NextGate:00} / {p.CheckpointCount:00}";
            float speed = race.vehicle ? Mathf.Abs(race.vehicle.ForwardSpeed) * 3.6f : 0;
            return race.ModeLabel + "\n" + $"LAP {Mathf.Min(p.CompletedLaps + 1, p.TargetLaps)} / {p.TargetLaps}    Completed: {p.CompletedLaps}    {speed:0} km/h\n" +
                $"Adjusted {FormatTime(p.AdjustedTime(race.Clock))}  Driving {FormatTime(p.RaceTime(race.Clock))}  +{p.PenaltySeconds:0.0}s ({p.MissedGates} misses)\n" +
                $"Last  {FormatTime(p.LastLap)}    Best  {FormatTime(p.BestLap)}\n" +
                $"Track position {race.PlayerPosition}/{race.Racers.Count}  Next: {next}\n{p.Status}\nEnter / Esc / Start: pause    R / Y: reset car" +
                (race.Flow && race.Flow.Save != null ? $"\nPersonal best  {FormatTime(race.Flow.Save.Best.lap)}    {race.Flow.Save.Error}" : "");
        }
        void LateUpdate() { if (race && race.Progress != null && display) display.text = BuildText(); }
    }
}
