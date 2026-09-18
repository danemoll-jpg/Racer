using UnityEngine;

namespace Racer
{
    public sealed class RacerState
    {
        public readonly string Name;
        public readonly ArcadeVehicle Car;
        public readonly RaceProgress Progress;
        public bool Dnf, FinishArmed;
        public Vector3 Previous;
        public double PreviousTime;
        public float RoadPosition, Travel, VerifiedRoad;
        public int Recoveries;
        public float RecoveryStart = float.NaN;
        public RacerState(string name, ArcadeVehicle car, int gates, int laps)
        {
            Name = name;
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
