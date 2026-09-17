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
            return $"LAP {Mathf.Min(p.CompletedLaps + 1, p.TargetLaps)} / {p.TargetLaps}    Completed: {p.CompletedLaps}    {speed:0} km/h\n" +
                $"Race  {FormatTime(p.RaceTime(race.Clock))}    Lap  {FormatTime(p.LapTime(race.Clock))}\n" +
                $"Last  {FormatTime(p.LastLap)}    Best  {FormatTime(p.BestLap)}\n" +
                $"Next: {next}\n{p.Status}\nEnter / Start: restart race    R / Y: reset car";
        }
        void LateUpdate() { if (race && race.Progress != null && display) display.text = BuildText(); }
    }
}
