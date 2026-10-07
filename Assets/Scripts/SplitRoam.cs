using System.Linq;
using UnityEngine;

namespace Racer
{
    // 0.94 Part B (split-screen stage 3): Free Roam for two in FreeRoamWorld. Player 1 is the scene's own vehicle; player 2's
    // vehicle is made here (beside player 1, with player 2's vehicle and colour), driven by player 2's device, or cruising the
    // roads with the traffic driver when player 2 is the AI. Each player has their own reset (nearest safe point), their own
    // jump and speed-trap results (shown in their half), the direction and distance to the other player, and both are on
    // each minimap. Nothing is recorded: no acorns, activity records or discovery (ExplorationCollection, ExplorationMap and
    // ArcadeActivities check SplitScreen.Active). The world map's travel brings both players (BringBeside).
    public sealed class SplitRoam : MonoBehaviour
    {
        public static SplitRoam Current { get; private set; }
        RaceFlow flow; RaceDirector race;
        public ArcadeVehicle Car { get; private set; }
        public ArcadeActivities Activities2 { get; private set; }
        public RoadDriver Cruiser { get; private set; }
        public float StartedAt { get; private set; }

        public void Initialize(RaceFlow owner) { flow = owner; race = owner.Race; Current = this; }
        void OnDestroy() { Clear(); if (Current == this) Current = null; }

        // RaceDirector.RestartRace (Free Roam): player 2's vehicle, made again beside player 1
        public ArcadeVehicle CreatePlayerTwo(string vehicle, int colour)
        {
            Clear();
            var clone = Instantiate(race.vehicle.gameObject); clone.name = "PLAYER 2";
            foreach (var c in clone.GetComponents<ActivityLandingContact>()) Destroy(c);
            foreach (var d in clone.GetComponents<RoadDriver>()) { d.enabled = false; Destroy(d); }
            var sound = clone.GetComponent<VehicleAudio>(); if (sound) { sound.enabled = false; DestroyImmediate(sound); }
            foreach (var s in clone.GetComponents<AudioSource>()) DestroyImmediate(s);
            var car = clone.GetComponent<ArcadeVehicle>(); var config = clone.GetComponent<VehicleConfiguration>();
            config.riderLook = RiderLook.Field(DriverVariation.Seed, 3, RiderLook.Player)[1]; config.classicVisual = false;
            config.Apply(vehicle); config.SetBodyColor(colour);
            clone.AddComponent<VehicleAudio>();
            Car = car;
            var input = clone.GetComponent<VehicleInput>(); var respawn = clone.GetComponent<VehicleRespawn>();
            input.Bind(SplitScreen.P2Ai ? null : SplitScreen.P2Device); input.enabled = !SplitScreen.P2Ai; respawn.enabled = true;
            car.enabled = true; car.Body.isKinematic = false;
            Place(car, race.vehicle.transform, 4.5f);
            if (SplitScreen.P2Ai) Cruise();
            Activities2 = gameObject.AddComponent<ArcadeActivities>(); Activities2.Initialize(race, flow.Save.DirectoryPath, car); Activities2.NewSession();
            StartedAt = Time.unscaledTime;
            return car;
        }
        // the AI player 2 cruises the roads (the traffic driver on the nearest road, without traffic's recycling)
        // pace > 1: faster than traffic (the Police Chase runner, 1.25); direction along the road (1 = with it)
        public void Cruise(float pace = 1.02f, int direction = 1, bool place = true)
        {
            if (!Car) return; if (Cruiser) { DestroyImmediate(Cruiser); Cruiser = null; }
            Car.GetComponent<VehicleInput>().enabled = false;
            Cruiser = Car.gameObject.AddComponent<RoadDriver>(); Cruiser.Initialize(race, Car, false, direction, pace);
            if (place) { var road = Cruiser.DriveRoad; float s = road.Project(Car.Body.position, out _); Cruiser.Place(s, road.TrafficLane(s, direction)); }
        }
        public void Clear()
        {
            if (Activities2) { Destroy(Activities2); Activities2 = null; }
            if (Car) { Car.gameObject.SetActive(false); Destroy(Car.gameObject); }
            Car = null; Cruiser = null;
        }
        // a vehicle set on the ground beside (side metres to the right of) another, facing the same way; behind it if blocked
        static void Place(ArcadeVehicle car, Transform beside, float side)
        {
            var rot = Quaternion.Euler(0, beside.eulerAngles.y, 0); var respawn = car.GetComponent<VehicleRespawn>();
            foreach (var offset in new[] { rot * new Vector3(side, 0, 0), rot * new Vector3(-side, 0, 0), rot * new Vector3(0, 0, -9), rot * new Vector3(0, 0, 9), rot * new Vector3(side, 0, -9) })
                if (respawn.TryFastTravel(beside.position + offset + Vector3.up, rot)) return;
            car.Body.position = beside.position + rot * new Vector3(side, 1, 0); car.Body.rotation = rot; car.transform.SetPositionAndRotation(car.Body.position, rot);
        }
        // the world map's travel: player 2 to player 1's new place
        public void BringBeside(Vector3 at, Quaternion facing)
        {
            if (!Car) return; Place(Car, race.vehicle.transform, 4.5f);
            Activities2?.NewSession(); if (SplitScreen.P2Ai) Cruise();
        }
        void Update()
        {
            if (!Car || !flow) return;
            bool driving = flow.State == RaceFlow.Stage.Racing;
            if (!SplitScreen.P2Ai) { var input = Car.GetComponent<VehicleInput>(); if (input.enabled != driving) input.enabled = driving; }
        }
        // "→ Kyle  240 m": the direction (from this player's view) and distance to the other player
        public static string Toward(Transform from, Transform viewer, Transform to, string name)
        {
            if (!from || !to) return "";
            var d = to.position - from.position; d.y = 0; float dist = d.magnitude;
            var f = viewer ? Vector3.ProjectOnPlane(viewer.forward, Vector3.up).normalized : from.forward;
            float a = Vector3.SignedAngle(f, d, Vector3.up); string[] arrows = { "↑", "↗", "→", "↘", "↓", "↙", "←", "↖" };
            string arrow = arrows[Mathf.RoundToInt(Mathf.Repeat(a, 360) / 45) % 8];
            return $"{arrow}  {name}  {DisplayUnits.Distance(dist)}";
        }
    }
}
