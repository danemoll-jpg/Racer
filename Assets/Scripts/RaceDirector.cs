using UnityEngine;

namespace Racer
{
    public sealed class RaceDirector : MonoBehaviour
    {
        public ArcadeVehicle vehicle;
        [Tooltip("Start/finish first, followed by required gates in driving order. No prescribed path between gates.")]
        public RaceGate[] gates;
        [Min(1)] public int laps = 3;
        public RaceProgress Progress { get; private set; }
        public double Clock { get; private set; }
        VehicleRespawn respawn;
        public RaceFlow Flow { get; private set; }
        Vector3 previous;
        double previousTime;

        void Awake()
        {
            if (!vehicle || gates == null || gates.Length < 2 || System.Array.Exists(gates, g => !g))
            { Debug.LogError("RaceDirector needs a vehicle and an ordered course.", this); enabled = false; return; }
            Progress = new RaceProgress(gates.Length - 1, laps);
            respawn = vehicle.GetComponent<VehicleRespawn>();
            Flow = GetComponent<RaceFlow>();
        }
        void OnEnable()
        {
            if (Progress == null) return;
            respawn.Respawned += OnRespawn;
            ResetSampling(vehicle.transform.position, Time.timeAsDouble);
        }
        void OnDisable() { if (respawn) respawn.Respawned -= OnRespawn; }
        void FixedUpdate() => Sample(vehicle.Body.position, vehicle.transform.forward, Time.fixedTimeAsDouble);
        public void ResetSampling(Vector3 position, double now)
        { previous = position; previousTime = Clock = now; }
        void OnRespawn()
        { Progress.ResetToGrid(); ResetSampling(vehicle.Body.position, Time.timeAsDouble); if (Flow) Flow.ResetFeedback(); }
        public void RestartRace()
        {
            if (Flow) Flow.PrepareRestart();
            respawn.ResetVehicle(); Progress.Restart();
            BreakableProp.RestoreRace();
            ResetSampling(vehicle.Body.position, Time.timeAsDouble);
            if (Flow) Flow.BeginCountdown();
        }
        // Also used by manual PhysX validation; times are simulation seconds, not wall-clock time.
        public void Sample(Vector3 position, Vector3 heading, double now)
        {
            if (Flow && Flow.State != RaceFlow.Stage.Racing) return;
            if (Progress.Finished) return;
            int completed = Progress.CompletedLaps;
            Clock = now;
            if ((position - previous).sqrMagnitude > 100)
                Progress.Invalidate("Position discontinuity");
            else
                for (int i = 0; i < gates.Length; i++)
                    if (gates[i].TryCross(previous, position, out bool forward, out float fraction))
                        Progress.Cross(i, forward && Vector3.Dot(heading, gates[i].transform.forward) > .25f,
                            previousTime + (now - previousTime) * fraction);
            previous = position; previousTime = now;
            if (Flow && Progress.CompletedLaps > completed) Flow.LapCompleted();
        }
    }
}
