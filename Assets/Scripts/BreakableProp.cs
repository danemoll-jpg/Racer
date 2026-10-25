using System.Collections.Generic;
using UnityEngine;

namespace Racer
{
    // Lightweight scenery yields without transferring solver impulses to the car.
    // The original visual is the debris: no clones, rigidbodies or collision fragments.
    [RequireComponent(typeof(BoxCollider))]
    public sealed class BreakableProp : MonoBehaviour
    {
        public static event System.Action<BreakableProp,ArcadeVehicle> BrokenByVehicle;
        public const int MaximumMovingDebris = 24;
        public const float DebrisLifetime = 4f;
        [Min(0.01f)] public float impactSpeed = .2f;
        public bool IsBroken { get; private set; }
        public bool PendingRestore { get; private set; }
        public int BreakCount { get; private set; }
        public SmashAudio.Surface surface;
        static readonly List<BreakableProp> all = new();
        static readonly List<BreakableProp> moving = new();
        public static int MovingDebrisCount => moving.Count;
        BoxCollider sensor;
        Renderer[] visuals;
        bool[] initialVisualEnabled;
        Vector3 initialPosition, initialScale, flight, spinAxis;
        Quaternion initialRotation;
        Vector3 initialSensorCenter, initialSensorHalf;
        readonly Collider[] restoreOverlaps = new Collider[32];
        float age, floor;
        bool initialized;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ClearRegistry() { all.Clear(); moving.Clear(); BrokenByVehicle=null; }
        void Awake()
        {
            sensor = GetComponent<BoxCollider>(); sensor.isTrigger = true;
            visuals = GetComponentsInChildren<Renderer>();
            initialVisualEnabled = new bool[visuals.Length];
            for(int i=0;i<visuals.Length;i++)initialVisualEnabled[i]=visuals[i].enabled;
            initialPosition = transform.position; initialRotation = transform.rotation;
            initialScale = transform.localScale;
            initialSensorCenter = transform.TransformPoint(sensor.center);
            initialSensorHalf = Vector3.Scale(sensor.size * .5f, transform.lossyScale);
            floor = initialPosition.y;
            foreach (var hit in Physics.RaycastAll(initialPosition + Vector3.up * 12, Vector3.down, 80, 1, QueryTriggerInteraction.Ignore))
                if (hit.collider is MeshCollider && hit.point.y <= initialPosition.y + .3f)
                    floor = Mathf.Min(floor, hit.point.y + .12f);
            initialized = true;
        }
        void OnEnable() { if (!all.Contains(this)) all.Add(this); }
        void OnDisable() { all.Remove(this); moving.Remove(this); }
        void OnTriggerEnter(Collider other) => Contact(other);
        void OnTriggerStay(Collider other) => Contact(other);
        void Contact(Collider other)
        {
            if (IsBroken || PendingRestore) return;
            var vehicle = other.attachedRigidbody ? other.attachedRigidbody.GetComponent<ArcadeVehicle>() : null;
            if (vehicle && vehicle.Body.linearVelocity.magnitude >= impactSpeed)
            {
                Yield(vehicle.Body.linearVelocity);
                BrokenByVehicle?.Invoke(this,vehicle);
                SmashAudio.Play(initialPosition,vehicle.Body.linearVelocity.magnitude,surface);
            }
        }
        void Yield(Vector3 velocity)
        {
            if (IsBroken) return;
            IsBroken = true; BreakCount++; sensor.enabled = false; age = 0;
            var direction = Vector3.ProjectOnPlane(velocity, Vector3.up).normalized;
            flight = direction * Mathf.Clamp(velocity.magnitude * .14f, .6f, 4f);
            flight.y = Mathf.Clamp(velocity.magnitude * .035f, .4f, 1.4f);
            spinAxis = Vector3.Cross(Vector3.up, direction);
            if (spinAxis.sqrMagnitude < .1f) spinAxis = transform.right;
            while (moving.Count >= MaximumMovingDebris) moving[0].HideDebris();
            moving.Add(this);
        }
        void HideDebris()
        {
            foreach (var visual in visuals) visual.enabled = false;
            moving.Remove(this);
        }
        void Update()
        {
            if (PendingRestore) { TryRestore(); return; }
            if (!IsBroken || !moving.Contains(this)) return;
            float dt = Time.deltaTime; age += dt;
            flight.y -= 7f * dt;
            var p = transform.position + flight * dt;
            if (p.y <= floor) { p.y = floor; flight = Vector3.MoveTowards(flight, Vector3.zero, 10f * dt); flight.y = 0; }
            transform.position = p;
            transform.rotation = Quaternion.AngleAxis(Mathf.Min(85, age * 190), spinAxis) * initialRotation;
            if (age > DebrisLifetime - .3f) transform.localScale = initialScale * Mathf.Clamp01((DebrisLifetime - age) / .3f);
            if (age >= DebrisLifetime) HideDebris();
        }
        bool VehicleOverlaps()
        {
            // Use the original oriented sensor, not its broad AABB: diagonal wide glass
            // otherwise stays hidden while a car is still metres outside the doorway.
            int count = Physics.OverlapBoxNonAlloc(initialSensorCenter, initialSensorHalf,
                restoreOverlaps, initialRotation, ~0, QueryTriggerInteraction.Ignore);
            if (count == restoreOverlaps.Length) return true;
            for (int i=0; i<count; i++)
                if (restoreOverlaps[i].attachedRigidbody &&
                    restoreOverlaps[i].attachedRigidbody.GetComponent<ArcadeVehicle>()) return true;
            return false;
        }
        void TryRestore()
        {
            if (VehicleOverlaps()) return;
            transform.SetPositionAndRotation(initialPosition, initialRotation);
            transform.localScale = initialScale;
            for(int i=0;i<visuals.Length;i++)visuals[i].enabled=initialVisualEnabled[i];
            sensor.enabled = true; IsBroken = false; PendingRestore = false; age = 0;
        }
        // 1.1 (from Chastain Rush's FenceChunk.ReplayBreak): a race replay breaks the prop that stood at 'at' (the one recorded), as a vehicle at 'vel'
        // would; quiet (going back in time puts the world as it was): hidden, no flight, no sound. BrokenByVehicle is not raised.
        public Vector3 Home => initialPosition;
        public static bool ReplayBreak(Vector3 at, Vector3 vel, bool show)
        {
            BreakableProp found = null; float best = .25f;
            foreach (var prop in all) { if (!prop || !prop.initialized) continue; float d = (prop.initialPosition - at).sqrMagnitude; if (d < best) { best = d; found = prop; } }
            if (!found || found.IsBroken) return found;
            found.PendingRestore = false;
            if (show) { found.Yield(vel); SmashAudio.Play(found.initialPosition, vel.magnitude, found.surface); }
            else { found.IsBroken = true; found.sensor.enabled = false; found.HideDebris(); }
            return true;
        }
        // a replay going back in time: every prop as it stood, at once (no vehicle can block it: the replay's vehicles have no colliders)
        public static void ReplayRestoreAll()
        {
            foreach (var prop in all.ToArray())
            {
                if (!prop || !prop.initialized || (!prop.IsBroken && !prop.PendingRestore)) continue;
                prop.HideDebris(); prop.transform.SetPositionAndRotation(prop.initialPosition, prop.initialRotation); prop.transform.localScale = prop.initialScale;
                for (int i = 0; i < prop.visuals.Length; i++) prop.visuals[i].enabled = prop.initialVisualEnabled[i];
                prop.sensor.enabled = true; prop.IsBroken = false; prop.PendingRestore = false; prop.age = 0;
            }
        }
        // 1.3: this one prop back where it stood (a Free Roam smash spot nobody is near any more); waits while a vehicle is in its place
        public void Restore() { if (!initialized || (!IsBroken && !PendingRestore)) return; HideDebris(); PendingRestore = true; TryRestore(); }
        public static void RestoreRace()
        {
            Physics.SyncTransforms();
            foreach (var prop in all.ToArray())
            {
                if (!prop || !prop.initialized) continue;
                prop.sensor.enabled = false; prop.HideDebris(); prop.PendingRestore = true;
                prop.TryRestore();
            }
        }
    }
}

