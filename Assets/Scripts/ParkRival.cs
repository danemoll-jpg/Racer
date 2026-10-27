using UnityEngine;

namespace Racer
{
    // 1.3.0-review3 (Dan: "It should start with needing to reach a high score on the stunt course then a stunt course competition"): a rival in the
    // Stunt chapter's Park Competition (StuntEvent with the campaign event's rivals). A copy of the player's vehicle (as split-screen player 2 is
    // made) in the event's stunt vehicle, ridden round Red Clay Stunt Park on the park's circuit (the line the Stunt event's check bot rode for
    // 10 minutes since 0.106: the kicker line, round the bowl end, back along the big ramps), turning tricks on its flights (RivalTricks, SteerOnly:
    // more and bigger tricks on Hard) and scored by its own ArcadeActivities, the player's own scoring. Each rival starts at its own point of the
    // circuit so they are spread round the park. Stopped or on its roof for 2.5 s: put back on the circuit at the nearest point (its combo ends).
    public sealed class ParkRival : MonoBehaviour
    {
        // park metres (StuntPark.P) and the speed cap there (m/s)
        static readonly (float u, float v, float cap)[] Circuit =
        {
            (140, 20, 15), (110, 20, 15), (85, 20, 15), (55, 20, 16), (22, 22, 12), (11, 34, 11), (10, 50, 16), (10, 90, 15),
            (12, 118, 12), (22, 134, 11), (34, 138, 15), (70, 138, 17), (100, 138, 18), (118, 136, 18), (128, 131.5f, 14), (140, 128, 11),
            (146.5f, 118, 13), (146.5f, 50, 14), (146.5f, 34, 11), (150, 22, 10),
        };
        public static string NameFor(int slot) => CampaignData.RivalNames[slot % CampaignData.RivalNames.Length];
        public ArcadeVehicle Car { get; private set; }
        public ArcadeActivities Acts { get; private set; }
        public int Slot { get; private set; }
        public int Resets { get; private set; } // checks
        RaceFlow flow; VehicleConfiguration config; int wp, lastReset = -1; float slowSince = -1, waitUntil, pace = 1, kick, lastResetAt = -99;

        public static ParkRival Create(RaceFlow flow, string vehicle, int slot, int difficulty)
        {
            var race = flow.Race; var clone = Instantiate(race.vehicle.gameObject); clone.name = "PARK RIVAL " + (slot + 1) + " " + NameFor(slot);
            foreach (var c in clone.GetComponents<ActivityLandingContact>()) Destroy(c);
            foreach (var d in clone.GetComponents<RoadDriver>()) { d.enabled = false; Destroy(d); }
            foreach (var p in clone.GetComponents<PoliceLights>()) Destroy(p);
            foreach (var t in clone.GetComponents<TreeBreaker>()) Destroy(t);
            foreach (var t in clone.GetComponents<RivalTricks>()) Destroy(t);
            var sound = clone.GetComponent<VehicleAudio>(); if (sound) { sound.enabled = false; DestroyImmediate(sound); }
            foreach (var s in clone.GetComponents<AudioSource>()) DestroyImmediate(s);
            var car = clone.GetComponent<ArcadeVehicle>(); var config = clone.GetComponent<VehicleConfiguration>();
            config.riderLook = RiderLook.Field(DriverVariation.Seed + 11, 6, RiderLook.Player)[1 + slot % 5]; config.classicVisual = false;
            config.Apply(vehicle); config.SetBodyColor(slot + 2);
            clone.AddComponent<VehicleAudio>();
            var input = clone.GetComponent<VehicleInput>(); input.Bind(null); input.enabled = false;
            var respawn = clone.GetComponent<VehicleRespawn>(); if (respawn) respawn.enabled = false;
            car.enabled = false; car.Body.isKinematic = false; // driven here (Simulate), as the road AI drives
            var me = clone.AddComponent<ParkRival>(); me.flow = flow; me.Car = car; me.config = config; me.Slot = slot; me.pace = new[] { .9f, 1f, 1.08f }[Mathf.Clamp(difficulty, 0, 2)]; me.kick = new[] { 2f, 3f, 5f }[Mathf.Clamp(difficulty, 0, 2)];
            me.PutAt((slot * 4) % Circuit.Length);
            var tricks = clone.AddComponent<RivalTricks>(); tricks.SteerOnly = true; tricks.Park = true; tricks.Difficulty = difficulty; tricks.Initialize(race, slot + 1);
            me.Acts = clone.AddComponent<ArcadeActivities>(); me.Acts.Initialize(race, flow.Save.DirectoryPath, car); me.Acts.NewSession();
            return me;
        }
        // on the circuit at point k (on the ground there; at the start a little to the side by slot so two never share a spot), facing the next point
        void PutAt(int k, bool side = true)
        {
            var a = Circuit[k]; var b = Circuit[(k + 1) % Circuit.Length]; var from = StuntPark.P(a.u, a.v); var to = StuntPark.P(b.u, b.v);
            var f = Vector3.ProjectOnPlane(to - from, Vector3.up).normalized; var rot = Quaternion.LookRotation(f);
            var p = from + Vector3.Cross(Vector3.up, f) * (side ? (Slot % 3 - 1) * 2.5f : 0);
            p.y = Physics.Raycast(new Vector3(p.x, StuntPark.FloorY + 30, p.z), Vector3.down, out var hit, 60, Car.groundMask, QueryTriggerInteraction.Ignore) ? hit.point.y + .7f : StuntPark.FloorY + .7f;
            Car.Body.position = p; Car.Body.rotation = rot; transform.SetPositionAndRotation(p, rot);
            Car.Body.linearVelocity = Car.Body.angularVelocity = Vector3.zero; Car.ClearSteering(); wp = (k + 1) % Circuit.Length;
        }
        void FixedUpdate()
        {
            if (!Car || !flow || flow.State != RaceFlow.Stage.Racing || Car.Body.isKinematic) return;
            float dt = Time.fixedDeltaTime; var p = Car.Body.position;
            if (Time.time < waitUntil) { Car.Simulate(0, 1, 0, dt); return; }
            var w = Circuit[wp]; var target = StuntPark.P(w.u, w.v);
            if (Vector3.Distance(Flat(p), Flat(target)) < 7) { wp = (wp + 1) % Circuit.Length; w = Circuit[wp]; target = StuntPark.P(w.u, w.v); }
            bool air = Car.GroundedWheels == 0;
            float ang = Vector3.SignedAngle(Flat(Car.transform.forward), Flat(target - p), Vector3.up), speed = Vector3.Dot(Car.Body.linearVelocity, Car.transform.forward);
            // the kicker line (K1, K2, K3 on the way to points 1-3) faster, for flights long enough to turn a trick (Easy +2 m/s, Normal +3, Hard +5)
            float cap = (Mathf.Abs(ang) > 50 ? Mathf.Min(w.cap, 8) : w.cap + (wp is >= 1 and <= 3 ? kick : 0)) * pace;
            Car.Simulate(speed < cap ? 1 : 0, speed > cap + 1.5f ? Mathf.Clamp01((speed - cap) / 5) : 0, air ? 0 : Mathf.Clamp(ang / 25, -1, 1), dt);
            // stopped or on its roof: back on the circuit at the nearest point
            bool stuck = Car.Body.linearVelocity.magnitude < 1.5f || config.WipedOut;
            if (!stuck) slowSince = -1; else if (slowSince < 0) slowSince = Time.time;
            if (slowSince > 0 && Time.time - slowSince > 2.5f)
            {
                slowSince = -1; Resets++; Acts.EndStuntCombo(); int best = 0; float d = float.MaxValue;
                for (int i = 0; i < Circuit.Length; i++) { float x = Vector3.Distance(Flat(p), Flat(StuntPark.P(Circuit[i].u, Circuit[i].v))); if (x < d) { d = x; best = i; } }
                // stuck again at the same point straight after a reset: on to the next point instead
                if (best == lastReset && Time.time - lastResetAt < 10) best = (best + 1) % Circuit.Length;
                lastReset = best; lastResetAt = Time.time; PutAt(best, false); waitUntil = Time.time + .6f;
            }
        }
        static Vector3 Flat(Vector3 v) { v.y = 0; return v; }
    }
}
