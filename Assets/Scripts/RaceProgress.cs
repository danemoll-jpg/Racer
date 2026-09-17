using System;

namespace Racer
{
    /// <summary>Ordered required gates only: paths between gates are deliberately unconstrained.</summary>
    public sealed class RaceProgress
    {
        public int CheckpointCount { get; }
        public int TargetLaps { get; }
        public int CompletedLaps { get; private set; }
        public int NextGate { get; private set; }
        public bool Started { get; private set; }
        public bool LapActive { get; private set; }
        public bool LapValid { get; private set; }
        public bool Finished => CompletedLaps == TargetLaps;
        public double LastLap { get; private set; }
        public double BestLap { get; private set; }
        public string Status { get; private set; }
        double raceStart, lapStart, finishTime;

        public RaceProgress(int checkpoints, int laps)
        {
            if (checkpoints < 1 || laps < 1) throw new ArgumentOutOfRangeException();
            CheckpointCount = checkpoints; TargetLaps = laps; Restart();
        }
        public double RaceTime(double now) => Started ? Math.Max(0, (Finished ? finishTime : now) - raceStart) : 0;
        public double LapTime(double now) => LapActive && !Finished ? Math.Max(0, now - lapStart) : 0;
        public void Restart()
        {
            CompletedLaps = NextGate = 0; Started = LapActive = LapValid = false;
            LastLap = BestLap = raceStart = lapStart = finishTime = 0;
            Status = "Cross START in the arrow direction";
        }
        public void Invalidate(string reason)
        {
            if (Finished) return;
            LapValid = false; Status = reason + " - return to START for a fresh lap";
        }
        public void ResetToGrid()
        {
            if (Finished) return;
            LapActive = LapValid = false; NextGate = 0;
            Status = "Vehicle reset - cross START for a fresh lap";
        }
        public void Cross(int gate, bool forward, double now)
        {
            if (Finished || gate < 0 || gate > CheckpointCount) return;
            if (!forward) { Invalidate("Wrong way"); return; }
            if (gate == 0)
            {
                if (LapActive && LapValid && NextGate == 0 && now > lapStart)
                {
                    CompletedLaps++; LastLap = now - lapStart;
                    if (BestLap == 0 || LastLap < BestLap) BestLap = LastLap;
                    if (Finished) { finishTime = now; LapActive = false; Status = "Race complete - Enter / Start to restart"; return; }
                }
                if (!Started) { Started = true; raceStart = now; }
                lapStart = now; LapActive = LapValid = true; NextGate = 1;
                Status = "Follow the numbered gates";
                return;
            }
            if (!LapActive) { Status = "Cross START before checkpoints"; return; }
            if (!LapValid) return;
            if (gate != NextGate) { Invalidate("Checkpoint out of order"); return; }
            NextGate = gate == CheckpointCount ? 0 : gate + 1;
            Status = NextGate == 0 ? "All checkpoints passed - cross FINISH" : "Checkpoint accepted";
        }
    }
}
