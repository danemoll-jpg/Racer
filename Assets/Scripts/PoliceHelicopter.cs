using System.Collections.Generic;
using UnityEngine;

namespace Racer
{
    // 0.97 Part A: the police helicopter (Getaway, from heat 4; one at most). It follows the runner from above at a delay of
    // about two seconds, so a fast runner can outrun its spotlight, and lights a circle of ground. A runner inside the circle is
    // SEEN (GetawayChase counts the helicopter like a cop that sees), unless something is between: dense tree canopy over the runner,
    // a tunnel or cave roof, a tree-top board or any other solid thing. It cannot catch, only see.
    public sealed class PoliceHelicopter : MonoBehaviour
    {
        public const float Delay = 2.4f, Altitude = 58, ConeRadius = 21, Speed = 62;
        GetawayChase boss; GetawayChase.Runner target; Transform rotor, tailRotor, beam; Light spot; AudioSource thump;
        readonly Queue<(float t, Vector3 p)> trail = new(); Vector3 aim, velocity; bool seesCache, started;
        public Vector3 Aim => aim; public bool Covered { get; private set; } public string Why { get; private set; } = "";
        static AudioClip thumpClip;

        public void Begin(GetawayChase owner, GetawayChase.Runner runner)
        {
            boss = owner; target = runner; var p = runner.body.position; var back = -runner.car.transform.forward; back.y = 0;
            aim = p; transform.position = p + back.normalized * 260 + Vector3.up * (Altitude + 20); started = true;
            Build();
        }
        Renderer Part(string name, PrimitiveType type, Vector3 pos, Vector3 scale, Color colour, Transform parent = null)
        {
            var go = GameObject.CreatePrimitive(type); go.name = name; var c = go.GetComponent<Collider>(); if (c) Destroy(c);
            go.transform.SetParent(parent ? parent : transform, false); go.transform.localPosition = pos; go.transform.localScale = scale;
            var r = go.GetComponent<Renderer>(); r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; r.material.color = colour; return r;
        }
        void Build()
        {
            var dark = new Color(.05f, .06f, .08f); Part("Body", PrimitiveType.Capsule, Vector3.zero, new Vector3(2.4f, 2.2f, 2.8f), dark).transform.localRotation = Quaternion.Euler(90, 0, 0);
            Part("Cockpit", PrimitiveType.Sphere, new Vector3(0, .25f, 1.9f), new Vector3(2f, 1.8f, 2.4f), new Color(.1f, .14f, .2f));
            Part("Tail boom", PrimitiveType.Cube, new Vector3(0, .35f, -4.6f), new Vector3(.45f, .5f, 6.4f), dark);
            Part("Fin", PrimitiveType.Cube, new Vector3(0, 1.2f, -7.4f), new Vector3(.2f, 1.7f, 1.1f), dark);
            Part("Mast", PrimitiveType.Cylinder, new Vector3(0, 1.45f, 0), new Vector3(.25f, .35f, .25f), dark);
            rotor = new GameObject("Rotor").transform; rotor.SetParent(transform, false); rotor.localPosition = new Vector3(0, 1.85f, 0);
            Part("Blade A", PrimitiveType.Cube, Vector3.zero, new Vector3(11f, .06f, .35f), new Color(.1f, .1f, .12f), rotor); Part("Blade B", PrimitiveType.Cube, Vector3.zero, new Vector3(.35f, .06f, 11f), new Color(.1f, .1f, .12f), rotor);
            tailRotor = new GameObject("Tail rotor").transform; tailRotor.SetParent(transform, false); tailRotor.localPosition = new Vector3(.3f, 1.2f, -7.5f);
            Part("Tail blade", PrimitiveType.Cube, Vector3.zero, new Vector3(.06f, 2.1f, .2f), new Color(.1f, .1f, .12f), tailRotor);
            var lamp = new GameObject("Spotlight"); lamp.transform.SetParent(transform, false); lamp.transform.localPosition = new Vector3(0, -1.1f, 1.6f);
            spot = lamp.AddComponent<Light>(); spot.type = LightType.Spot; spot.range = Altitude * 2.4f; spot.spotAngle = 2 * Mathf.Atan(ConeRadius / Altitude) * Mathf.Rad2Deg; spot.intensity = 40; spot.color = new Color(1f, .96f, .85f); spot.shadows = LightShadows.None;
            var beamGo = new GameObject("Beam"); beamGo.transform.SetParent(lamp.transform, false); beam = beamGo.transform;
            var mf = beamGo.AddComponent<MeshFilter>(); mf.sharedMesh = ConeMesh(); var mr = beamGo.AddComponent<MeshRenderer>(); mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            var shader = Shader.Find("Sprites/Default"); if (shader) { var m = new Material(shader); m.color = new Color(1, .97f, .85f, .06f); mr.sharedMaterial = m; } else mr.enabled = false;
            thump = gameObject.AddComponent<AudioSource>(); thump.clip = Thump(); thump.loop = true; thump.spatialBlend = 1; thump.minDistance = 25; thump.maxDistance = 420; thump.volume = .8f; thump.Play();
        }
        static Mesh ConeMesh()
        {
            var v = new List<Vector3> { Vector3.zero }; var t = new List<int>(); int n = 18;
            for (int i = 0; i < n; i++) { float a = i * Mathf.PI * 2 / n; v.Add(new Vector3(Mathf.Cos(a) * ConeRadius, Mathf.Sin(a) * ConeRadius, Altitude)); }
            for (int i = 0; i < n; i++) { t.Add(0); t.Add(1 + (i + 1) % n); t.Add(1 + i); t.Add(0); t.Add(1 + i); t.Add(1 + (i + 1) % n); }
            var mesh = new Mesh { name = "Helicopter beam" }; mesh.SetVertices(v); mesh.SetTriangles(t, 0); mesh.RecalculateBounds(); return mesh;
        }
        static AudioClip Thump()
        {
            if (thumpClip) return thumpClip; const int rate = 22050; int n = rate; var d = new float[n]; var rng = new System.Random(5); float lp = 0;
            for (int i = 0; i < n; i++) { float t = (float)i / rate; float beat = Mathf.Pow(Mathf.Max(0, Mathf.Sin(2 * Mathf.PI * 11 * t)), 6); float noise = (float)rng.NextDouble() * 2 - 1; lp += (noise - lp) * .06f; d[i] = (lp * 2.2f + Mathf.Sin(2 * Mathf.PI * 70 * t) * .4f) * beat * .55f; }
            thumpClip = AudioClip.Create("Helicopter", n, 1, rate, false); thumpClip.SetData(d, 0); return thumpClip;
        }

        void Update()
        {
            if (!started || target == null || target.body == null) return;
            if (target.Done) { var next = boss.Runners.Find(x => !x.Done && x.human); if (next != null) target = next; }
            float now = Time.time; trail.Enqueue((now, target.body.position)); while (trail.Count > 1 && now - trail.Peek().t > Delay + 1.5f) trail.Dequeue();
            Vector3 want = target.body.position; foreach (var (t, p) in trail) if (now - t <= Delay) { want = p; break; }
            aim = Vector3.MoveTowards(aim, want, Speed * 1.4f * Time.deltaTime);
            var lead = target.body.linearVelocity; lead.y = 0; var side = lead.sqrMagnitude > 4 ? -lead.normalized * 22 : Vector3.zero;
            var hover = aim + side + Vector3.up * Altitude; var ground = Physics.Raycast(aim + Vector3.up * 400, Vector3.down, out var hit, 800, ~0, QueryTriggerInteraction.Ignore) ? hit.point.y : aim.y; hover.y = Mathf.Max(hover.y, ground + Altitude);
            transform.position = Vector3.SmoothDamp(transform.position, hover, ref velocity, .9f, Speed);
            var flat = velocity; flat.y = 0; if (flat.sqrMagnitude > 4) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(flat) * Quaternion.Euler(Mathf.Clamp(flat.magnitude * .25f, 0, 10), 0, 0), 2f * Time.deltaTime);
            rotor.Rotate(0, 1500 * Time.deltaTime, 0); tailRotor.Rotate(1800 * Time.deltaTime, 0, 0);
            if (spot) { var from = spot.transform.position; spot.transform.rotation = Quaternion.LookRotation(aim - from); if (beam) beam.localScale = Vector3.one * (Vector3.Distance(from, aim) / Altitude); }
        }

        // from GetawayChase.Tick: does the beam on the ground hold this runner, with nothing between (recompute = a fresh test)
        public bool Sees(GetawayChase.Runner r, bool recompute)
        {
            if (!started || r == null || r.body == null) return false;
            if (!recompute) return seesCache && r == target;
            var to = r.body.position - aim; to.y = 0; bool seen = to.magnitude <= ConeRadius; Why = ""; Covered = false;
            if (seen)
            {
                var eye = transform.position; var at = r.body.position + Vector3.up * 1.1f; var dir = at - eye; float dist = dir.magnitude;
                var hits = Physics.RaycastAll(eye, dir / dist, dist - .5f, ~0, QueryTriggerInteraction.Ignore);
                foreach (var h in hits) { if (h.collider.GetComponentInParent<ArcadeVehicle>() || h.collider.GetComponentInParent<PoliceHelicopter>()) continue; seen = false; Covered = true; Why = "solid: " + h.collider.name; break; }
                if (seen && Canopy.Dense(r.body.position)) { seen = false; Covered = true; Why = "dense canopy"; }
                if (seen) { var fx = WorldLook.Current ? WorldLook.Current.GetComponent<WeatherEffects>() : null; if (fx && fx.CoveredAt(r.body.position)) { seen = false; Covered = true; Why = "tunnel / cave roof"; } }
            }
            seesCache = seen && r == target; return seen;
        }

        // dense tree canopy over a point: the crowns of the placed trees (SceneryTrees.Placements) in a coarse grid; under two crowns,
        // or one big one, that start above head height
        static class Canopy
        {
            static Dictionary<Vector2Int, List<SceneryTrees.Placement>> grid; static SceneryTrees source; const float Cell = 16;
            public static bool Dense(Vector3 p)
            {
                if (grid == null || !source) { source = Object.FindAnyObjectByType<SceneryTrees>(); if (!source) return false; grid = new(); foreach (var pl in source.Placements) { if (pl.variant >= 5 || pl.radius < 2.2f) continue; var k = new Vector2Int(Mathf.FloorToInt(pl.bottom.x / Cell), Mathf.FloorToInt(pl.bottom.z / Cell)); if (!grid.TryGetValue(k, out var l)) grid[k] = l = new(); l.Add(pl); } }
                int cx = Mathf.FloorToInt(p.x / Cell), cz = Mathf.FloorToInt(p.z / Cell); int covering = 0; float big = 0;
                for (int x = cx - 1; x <= cx + 1; x++) for (int z = cz - 1; z <= cz + 1; z++)
                    if (grid.TryGetValue(new Vector2Int(x, z), out var l))
                        foreach (var pl in l)
                        {
                            var d = pl.bottom - p; d.y = 0; if (d.magnitude > pl.radius * .85f) continue;
                            if (pl.bottom.y + pl.height * .45f < p.y + 2.5f) continue; covering++; big = Mathf.Max(big, pl.radius);
                        }
                return covering >= 2 || big >= 4.5f;
            }
        }
    }
}
