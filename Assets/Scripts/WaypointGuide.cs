using UnityEngine;

namespace Racer
{
    // 0.74 Free Roam custom waypoint (set on the exploration map anywhere in the world): a tall beam in the world at the
    // waypoint (additive and fog-free, so it reads by day and by night from far away) with a ring on the ground, and the HUD
    // line (RaceHud) with the distance and a direction arrow. Within 15 m: "Destination reached", and the waypoint clears.
    // Straight-line guidance only; no effect in races (nothing is shown there); not saved.
    public sealed class WaypointGuide : MonoBehaviour
    {
        public const float ArriveDistance = 15;
        ExplorationMap map; RaceDirector race; LineRenderer beam, ring; Material material;
        public bool Active { get; private set; }
        public float Distance { get; private set; }
        public float Bearing { get; private set; }// degrees, relative to the vehicle's heading (0 ahead, + right)
        public int Arrivals { get; private set; }
        void Awake()
        {
            map = GetComponent<ExplorationMap>(); race = GetComponent<RaceDirector>(); if (!race) race = FindAnyObjectByType<RaceDirector>();
            var asset = Resources.Load<Material>("LightningBolt");
            material = asset ? new Material(asset) { name = "Waypoint beam" } : new Material(Shader.Find("Sprites/Default"));
            material.SetFloat("_Intensity", 1.6f);
            beam = Line("Waypoint beam", 2, false, 1.8f); ring = Line("Waypoint ring", 40, true, .45f);
        }
        LineRenderer Line(string name, int count, bool loop, float width)
        {
            var go = new GameObject(name); go.transform.SetParent(transform, false);
            var l = go.AddComponent<LineRenderer>(); l.sharedMaterial = material; l.useWorldSpace = true; l.positionCount = count; l.loop = loop; l.widthMultiplier = width;
            l.alignment = LineAlignment.View; l.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; l.receiveShadows = false; l.enabled = false;
            var c = new Color(1f, .82f, .2f, 1); l.startColor = c; l.endColor = new Color(1f, .82f, .2f, loop ? 1 : .25f);
            return l;
        }
        void LateUpdate()
        {
            Active = map && race && race.FreeRoam && map.Waypoint.HasValue && race.vehicle;
            beam.enabled = ring.enabled = Active; if (!Active) return;
            var w = map.Waypoint.Value; var car = race.vehicle.Body.position;
            var delta = Vector3.ProjectOnPlane(w - car, Vector3.up); Distance = delta.magnitude;
            var fwd = Vector3.ProjectOnPlane(race.vehicle.transform.forward, Vector3.up);
            Bearing = Vector3.SignedAngle(fwd.sqrMagnitude > 1e-4f ? fwd : Vector3.forward, delta.sqrMagnitude > 1e-4f ? delta : fwd, Vector3.up);
            beam.SetPosition(0, w); beam.SetPosition(1, w + Vector3.up * 160);
            float pulse = 2.6f + .5f * Mathf.Sin(Time.unscaledTime * 3);
            for (int i = 0; i < ring.positionCount; i++) { float a = i * Mathf.PI * 2 / ring.positionCount; ring.SetPosition(i, w + new Vector3(Mathf.Cos(a) * pulse, .25f, Mathf.Sin(a) * pulse)); }
            if (Distance <= ArriveDistance && race.Flow.State == RaceFlow.Stage.Racing)
            {
                Arrivals++; map.ClearWaypointFromGuide(); race.Flow.Notify("Destination reached", 3); beam.enabled = ring.enabled = Active = false;
            }
        }
        // The HUD line: "WAYPOINT 0.42 mi" (the arrow is drawn beside it, rotated by Bearing).
        public string Hud => Active ? $"WAYPOINT  {DisplayUnits.Distance(Distance)}" : "";
        void OnDestroy() { if (material) Destroy(material); }
    }
}
