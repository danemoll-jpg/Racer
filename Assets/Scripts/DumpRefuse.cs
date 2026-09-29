using UnityEngine;

namespace Racer
{
    // Only authored on the Backyard dump. No contact bodies, upward force or motor changes.
    [DefaultExecutionOrder(100)]
    public sealed class DumpRefuse : MonoBehaviour
    {
        public Vector3 origin = new(309.2f, 0, 15.7f);
        public Vector3 axis = new Vector3(-50.6f, 0, -7.5f).normalized;
        ArcadeVehicle[] vehicles;
        float nextScan;

        public float Amount(ArcadeVehicle car)
        {
            if (!car || !car.Body || car.Body.isKinematic || car.GroundedWheels < 2) return 0;
            Vector3 delta = car.Body.position - origin;
            float a = (Vector3.Dot(delta, axis) - 25.5f) / 24.5f;
            float s = Vector3.Dot(delta, Vector3.Cross(Vector3.up, axis)) / 23f;
            float radius = Mathf.Sqrt(a * a + s * s);
            if (radius >= .86f) return 0;
            // Require actual terrain contact below the rider, so overflying jumps are unaffected.
            if (!Physics.Raycast(car.Body.position, Vector3.down, out var hit, car.suspensionLength + .5f, 1, QueryTriggerInteraction.Ignore)
                || !hit.collider.name.StartsWith("Ground_")) return 0;
            return 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.52f, .86f, radius));
        }

        void FixedUpdate()
        {
            if (vehicles == null || Time.time >= nextScan)
            {
                vehicles = FindObjectsByType<ArcadeVehicle>(FindObjectsSortMode.None);
                nextScan = Time.time + 1;
            }
            foreach (var car in vehicles)
            {
                if (!car || !car.gameObject.activeInHierarchy || car.gameObject.scene != gameObject.scene) continue;
                float amount = Amount(car);
                if (amount <= 0) continue;
                var velocity = Vector3.ProjectOnPlane(car.Body.linearVelocity, Vector3.up);
                // Smooth dissipative resistance at the centre of mass; zero at rest so either
                // direction can always pull away. Fade out on the slopes for a forgiving exit.
                car.Body.AddForce(-velocity * Mathf.Min(5f * amount, .8f / Time.fixedDeltaTime), ForceMode.Acceleration);
            }
        }
    }
}
