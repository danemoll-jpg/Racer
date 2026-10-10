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
        // 0.93 (BUG-002): an optional cleared corridor along a shortcut, from clearFrom (m along clearRoute) to its end, within
        // clearHalfWidth of its centre line: no bushes are drawn there and nothing is slowed (the Abandoned Cabin Jump's
        // landing and run-out to the rejoin). Unset = as before.
        // 0.94 Part D: the Abandoned Cabin Jump's brush is back from the lip to a far edge (clearFrom = that edge, set so a
        // well-hit jump clears it); brushFrom = the lip. A reset by a vehicle inside that restored brush (on clearRoute from
        // brushFrom to clearFrom, within clearHalfWidth of its centre) puts it on the clear ground just past the far edge,
        // on the centre line, facing along the route. 0 = no such reset (Tree-Top Trail).
        public WoodlandRoute clearRoute;
        public float clearFrom, clearHalfWidth, brushFrom;
        public const float ResetPastEdge = 4; // metres past the far edge: the last bushes (up to 1.2 m reach) and half a long car
        // The station a reset from inside a restored brush goes to (false = not inside one: the normal reset).
        public static bool ResetPast(WoodlandRoute route, Vector3 position, out float station)
        {
            station = 0; if (!route) return false;
            foreach (var u in FindObjectsByType<ShortcutUndergrowth>(FindObjectsSortMode.None))
                if (u.clearRoute == route && u.InBrush(position, out station)) return true;
            return false;
        }
        // 0.101 Part A: Free Roam has no Cabin branch; its brush keeps the race's line on an inactive route object (never a
        // registered branch), and a reset from inside that brush goes past the far edge the same way.
        public static bool ResetPastAny(Vector3 position, out WoodlandRoute route, out float station)
        {
            route = null; station = 0;
            foreach (var u in FindObjectsByType<ShortcutUndergrowth>(FindObjectsSortMode.None))
                if (u.clearRoute && u.InBrush(position, out station)) { route = u.clearRoute; return true; }
            return false;
        }
        bool InBrush(Vector3 position, out float station)
        {
            station = 0; var route = clearRoute;
            if (!route || brushFrom <= 0 || clearFrom <= brushFrom) return false;
            float s = route.Project(position, out _); var centre = route.At(s, out _);
            if (s < brushFrom || s >= clearFrom || new Vector2(position.x - centre.x, position.z - centre.z).magnitude > clearHalfWidth) return false;
            station = Mathf.Min(route.Length, clearFrom + ResetPastEdge); return true;
        }
        ArcadeVehicle[] vehicles;
        float nextScan;

        public float Coverage(Vector3 position)
        {
            float closest = float.MaxValue;
            foreach (var p in centres)
                closest = Mathf.Min(closest, new Vector2(position.x-p.x, position.z-p.z).magnitude);
            // 0.82 Part C: away from the bush footprints the answer is 0 whatever the main road says, so the (costly) projection
            // onto the main road is only made near them; same result as before.
            if (closest >= radius) return 0;
            if (Cleared(position, 0, position.y + 1)) return 0; // not under a raised stretch: a vehicle that fell off it is still slowed
            if (protectedMain)
            {
                protectedMain.Project(position, out float lateral);
                if (lateral < 4) return 0;
            }
            return 1-Mathf.SmoothStep(0, 1, Mathf.InverseLerp(radius-3, radius, closest));
        }

        // whether a point (with a bush's reach) lies in the cleared corridor
        public bool Cleared(Vector3 position, float reach, float top = float.MaxValue)
        {
            if (!clearRoute || clearHalfWidth <= 0) return false;
            float s = clearRoute.Project(position, out _); var centre = clearRoute.At(s, out _);
            return s >= clearFrom && top > centre.y - .5f && new Vector2(position.x - centre.x, position.z - centre.z).magnitude - reach < clearHalfWidth;
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
