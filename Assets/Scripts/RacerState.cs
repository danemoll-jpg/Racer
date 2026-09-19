using UnityEngine;

namespace Racer
{
    public sealed class RacerState
    {
        public readonly string Name;
        public readonly ArcadeVehicle Car;
        public readonly RaceProgress Progress;
        public bool Dnf, FinishArmed;
        public readonly bool IsAi;
        public bool Estimated { get; private set; }
        public double EstimatedPhysicalTime { get; private set; }
        public bool Classified => Progress.Finished || Estimated;
        public readonly AiFinishEstimate Estimate = new();
        public double ClassifiedTime(double now) => Estimated ? EstimatedPhysicalTime + Progress.PenaltySeconds : Progress.AdjustedTime(now);
        public bool FinalizeEstimate(double now,double duration)
        {
            if(!IsAi || Dnf || Classified || double.IsNaN(duration) || double.IsInfinity(duration)) return false;
            EstimatedPhysicalTime=Progress.RaceTime(now)+System.Math.Max(.02,duration); Estimated=true; return true;
        }
        public Vector3 Previous;
        public double PreviousTime;
        public float RoadPosition, Travel, VerifiedRoad;
        public int Recoveries;
        public readonly BranchProgress Branch=new();
        public float RecoveryStart = float.NaN;
        public RacerState(string name, ArcadeVehicle car, int gates, int laps, bool isAi=false)
        {
            Name = name;
            IsAi = isAi;
            Car = car;
            Progress = new RaceProgress(gates, laps);
        }

        public void SampleOrigin(double now)
        {
            Previous = Car.Body.position;
            PreviousTime = now;
        }
    }
}
