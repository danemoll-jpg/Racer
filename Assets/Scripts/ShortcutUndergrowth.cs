using UnityEngine;

namespace Racer
{
    // Authored only in the two Backyard shortcut footprints. Bushes resist ground
    // travel smoothly; the actual terrain remains the sole collision surface.
    [DefaultExecutionOrder(100)]
    public sealed class ShortcutUndergrowth : MonoBehaviour
    {
        public Vector3[] centres;
        public float radius = 17;
        public RaceRoad protectedMain;
        ArcadeVehicle[] vehicles;
        float nextScan;

        public float Coverage(Vector3 position)
        {
            if (protectedMain)
            {
                protectedMain.Project(position, out float lateral);
                if (lateral < 4) return 0;
            }
            float closest = float.MaxValue;
            foreach (var p in centres)
                closest = Mathf.Min(closest, new Vector2(position.x-p.x, position.z-p.z).magnitude);
            return 1-Mathf.SmoothStep(0, 1, Mathf.InverseLerp(radius-3, radius, closest));
        }

        void FixedUpdate()
        {
            if (vehicles == null || Time.time >= nextScan)
            {
                vehicles = FindObjectsByType<ArcadeVehicle>(FindObjectsSortMode.None);
                nextScan = Time.time+1;
            }
            foreach (var car in vehicles)
            {
                if (!car || !car.gameObject.activeInHierarchy || car.gameObject.scene != gameObject.scene
                    || !car.Body || car.Body.isKinematic || car.GroundedWheels < 2) continue;
                float amount = Coverage(car.Body.position);
                if (amount <= 0 || !Physics.Raycast(car.Body.position, Vector3.down, out var hit,
                    car.suspensionLength+.5f, 1, QueryTriggerInteraction.Ignore)
                    || !hit.collider.name.StartsWith("Ground_")) continue;
                var horizontal = Vector3.ProjectOnPlane(car.Body.linearVelocity, Vector3.up);
                car.Body.AddForce(-horizontal*Mathf.Min(6*amount, .8f/Time.fixedDeltaTime), ForceMode.Acceleration);
            }
        }
    }
}
