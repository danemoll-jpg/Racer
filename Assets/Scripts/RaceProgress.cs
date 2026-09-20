using System;
using System.Collections.Generic;

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
        public double PenaltySeconds { get; private set; }
        public int MissedGates { get; private set; }
        readonly List<string> penalties = new();
        public IReadOnlyList<string> Penalties => penalties;
        public sealed class PenaltyEntry
        {
            public int Lap, Checkpoint;
            public string Reason, Branch;
            public double Seconds, Time;
        }
        readonly List<PenaltyEntry> ledger = new();
        public IReadOnlyList<PenaltyEntry> Ledger => ledger;
        double lapPenalty;
        public double CurrentLapPenalty => lapPenalty;
        public double AdjustedTime(double now) => RaceTime(now) + PenaltySeconds;
        public bool MissFinish(double now)
        {
            if (!LapActive || !LapValid || Finished || NextGate != 0 || now <= lapStart) return false;
            ledger.Add(new PenaltyEntry { Lap=CompletedLaps+1, Checkpoint=0, Reason="missed finish gate", Branch="none", Seconds=5, Time=now });
            PenaltySeconds += 5; lapPenalty += 5; MissedGates=ledger.Count;
            penalties.Add($"L{CompletedLaps+1} FINISH +5s / missed finish gate");
            return true;
        }
        public bool Miss(int gate, double seconds, string reason="missed gate", string branch="none", double time=0)
        {
            if (!LapActive || !LapValid || Finished || gate == 0 || gate != NextGate) return false;
            seconds = 5; // One authoritative charge per ordered gate, regardless of caller.
            ledger.Add(new PenaltyEntry { Lap=CompletedLaps+1, Checkpoint=gate, Reason=reason, Branch=branch, Seconds=seconds, Time=time });
            PenaltySeconds += seconds; lapPenalty += seconds; MissedGates=ledger.Count;
            penalties.Add($"L{CompletedLaps + 1} CP{gate:00} +5s / {reason}"+(branch=="none"?"":" / "+branch));
            NextGate = gate == CheckpointCount ? 0 : gate + 1;
            Status = $"Checkpoint missed: +{seconds:0.0}s";
            return true;
        }
        readonly List<double> lapTimes = new();
        public IReadOnlyList<double> LapTimes => lapTimes;
        double raceStart, lapStart, finishTime;

        public RaceProgress(int checkpoints, int laps)
        {
            if (checkpoints < 1 || laps < 1) throw new ArgumentOutOfRangeException();
            CheckpointCount = checkpoints; TargetLaps = laps; Restart();
        }
        public void BeginTiming(double now) { if (!Started) { Started = true; raceStart = now; } }
        public double RaceTime(double now) => Started ? Math.Max(0, (Finished ? finishTime : now) - raceStart) : 0;
        public double LapTime(double now) => LapActive && !Finished ? Math.Max(0, now - lapStart) : 0;
        public void Restart()
        {
            CompletedLaps = NextGate = 0; Started = LapActive = LapValid = false;
            lapTimes.Clear();
            penalties.Clear(); ledger.Clear(); PenaltySeconds = lapPenalty = 0; MissedGates = 0;
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
            if (!forward) { Status = "Wrong way - turn around"; return; }
            if (gate == 0)
            {
                if (LapActive && LapValid && NextGate == 0 && now > lapStart)
                {
                    CompletedLaps++; LastLap = now - lapStart + lapPenalty;
                    lapTimes.Add(LastLap);
                    if (BestLap == 0 || LastLap < BestLap) BestLap = LastLap;
                    if (Finished) { finishTime = now; LapActive = false; Status = "Race complete"; return; }
                }
                if (LapActive && NextGate != 0) { Status = "Continue to the required checkpoint"; return; }
                if (!Started) { Started = true; raceStart = now; }
                lapStart = now; lapPenalty = 0; LapActive = LapValid = true; NextGate = 1;
                Status = "Follow the numbered gates";
                return;
            }
            if (!LapActive) { Status = "Cross START before checkpoints"; return; }
            if (!LapValid) return;
            if (gate != NextGate) return;
            NextGate = gate == CheckpointCount ? 0 : gate + 1;
            Status = NextGate == 0 ? "All checkpoints passed - cross FINISH" : "Checkpoint accepted";
        }
    }
}
