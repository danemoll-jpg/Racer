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
            foreach (var visual in visuals) visual.enabled = true;
            sensor.enabled = true; IsBroken = false; PendingRestore = false; age = 0;
        }
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

