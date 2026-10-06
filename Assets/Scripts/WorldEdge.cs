using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Racer
{
    // 0.81 Part 0.2 (BUG-002, "the end of the world"): the ground used to stop at a straight edge with empty haze beyond.
    // Built at load in every scene (the same world everywhere; nothing is saved into a scene):
    //  - Look: beyond the playable edge, a ring of rolling wooded hills (vertex-coloured, drawn with the ground's own
    //    material so it takes the time of day, weather and snow like the ground) rising from the edge to ridgelines up to
    //    ~1.8 km out, so every view has a horizon; a band of simple cone trees along the near hills. No collision.
    //  - Edge: just outside the edge the ground rises as a steep earth bank (visual and collidable), and from the bank
    //    top an invisible wall stands 150 m high behind it, so nobody rides off; the 0.68 fall reset stays the backstop.
    // "Outside" is found from the ground colliders themselves: a 12 m grid over the world is ray cast; a point with no
    // ground under it counts as outside only when it is reachable from beyond the world without crossing ground and is
    // within 30 m of the outer bounds of the big ground pieces, so ravines, flight gaps and other holes INSIDE the
    // world are never filled (section 5A). In the course scenes no collider is built within 25 m of a race line or
    // shortcut (listed in Report); the visual ring is everywhere.
    public sealed class WorldEdge : MonoBehaviour
    {
        public const float Cell = 12, Reach = 1800, BankRise = 10, BankRun = 12, WallHeight = 150, ColliderBand = 48;
        public static readonly List<string> Report = new();
        public int Trees { get; private set; }
        public int Triangles { get; private set; }

        public static WorldEdge Attach(GameObject host) => host.GetComponent<WorldEdge>() ?? host.AddComponent<WorldEdge>();
        void Start() { var w = System.Diagnostics.Stopwatch.StartNew(); Build(gameObject.scene); Report.Add($"built in {w.ElapsedMilliseconds} ms"); Debug.Log($"World edge ({gameObject.scene.name}):\n" + string.Join("\n", Report)); }

        static float Noise(float x, float z)
        {
            // smooth value noise, two octaves, 0..1
            float n = 0, a = .65f;
            for (int o = 0; o < 3; o++) { n += a * Mathf.PerlinNoise(x * (1 << o) + 17.3f * o, z * (1 << o) - 9.1f * o); a *= .5f; }
            return n / 1.1375f;
        }

        public void Build(Scene scene)
        {
            Report.Clear();
            // the big ground pieces: their bounds give the world's outline; their material draws the ring
            var big = new List<Bounds>(); Material ground = null;
            foreach (var c in scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Collider>()))
            {
                if (!c.enabled || c.isTrigger || c.attachedRigidbody || !(c is MeshCollider || c is TerrainCollider)) continue;
                var b = c.bounds; if (b.size.x < 100 && b.size.z < 100) continue;
                big.Add(b);
                if (!ground && c.name.StartsWith("Ground") && c.TryGetComponent<MeshRenderer>(out var r) && r.sharedMaterial && r.sharedMaterial.shader && r.sharedMaterial.shader.name == "Racer/MarkedGround") ground = r.sharedMaterial;
            }
            if (big.Count == 0 || !ground) { Report.Add("no ground pieces here: nothing built"); return; }
            var all = big[0]; foreach (var b in big) all.Encapsulate(b);
            bool InsideBig(float x, float z) { foreach (var b in big) if (x >= b.min.x && x <= b.max.x && z >= b.min.z && z <= b.max.z) return true; return false; }
            bool NearOutside(float x, float z)
            {
                if (!InsideBig(x, z)) return true;
                for (int k = 0; k < 8; k++) { var o = Quaternion.Euler(0, k * 45, 0) * Vector3.forward * 30; if (!InsideBig(x + o.x, z + o.z)) return true; }
                return false;
            }
            // grid lines: 12 m over the world (+2 cells), then growing outward to Reach
            float[] Axis(float lo, float hi)
            {
                var inner = new List<float>(); for (float v = lo - 2 * Cell; v <= hi + 2 * Cell + .01f; v += Cell) inner.Add(v);
                var before = new List<float>(); float step = Cell, at = inner[0];
                while (at > lo - Reach) { step = Mathf.Min(step * 1.13f, 140); at -= step; before.Add(at); }
                var after = new List<float>(); step = Cell; at = inner[^1];
                while (at < hi + Reach) { step = Mathf.Min(step * 1.13f, 140); at += step; after.Add(at); }
                before.Reverse(); return before.Concat(inner).Concat(after).ToArray();
            }
            var X = Axis(all.min.x, all.max.x); var Z = Axis(all.min.z, all.max.z); int nx = X.Length, nz = Z.Length, n = nx * nz;
            int I(int i, int j) => i * nz + j;
            var covered = new bool[n]; var height = new float[n]; var colour = new Color[n]; var candidate = new bool[n];
            for (int i = 0; i < nx; i++) for (int j = 0; j < nz; j++)
                {
                    int k = I(i, j); float x = X[i], z = Z[j];
                    if (x >= all.min.x - 1 && x <= all.max.x + 1 && z >= all.min.z - 1 && z <= all.max.z + 1 && Ground(x, z, out var hit))
                    { covered[k] = true; height[k] = hit.point.y; colour[k] = Tint(hit); }
                    else candidate[k] = NearOutside(x, z);
                }
            // outside: flood from the grid's border through candidate points
            var outside = new bool[n]; var queue = new Queue<int>();
            for (int i = 0; i < nx; i++) for (int j = 0; j < nz; j++) if ((i == 0 || j == 0 || i == nx - 1 || j == nz - 1) && candidate[I(i, j)]) { outside[I(i, j)] = true; queue.Enqueue(I(i, j)); }
            while (queue.Count > 0)
            {
                int k = queue.Dequeue(), i = k / nz, j = k % nz;
                for (int di = -1; di <= 1; di++) for (int dj = -1; dj <= 1; dj++)
                    {
                        int a = i + di, b = j + dj; if (a < 0 || b < 0 || a >= nx || b >= nz) continue; int m = I(a, b);
                        if (!outside[m] && candidate[m]) { outside[m] = true; queue.Enqueue(m); }
                    }
            }
            // sources: ground points next to the outside; distance and the source's height spread outward (Dijkstra)
            var dist = new float[n]; var baseY = new float[n]; var baseC = new Color[n]; var source = new bool[n];
            for (int k = 0; k < n; k++) dist[k] = float.MaxValue;
            var heap = new SortedSet<(float d, int k)>();
            for (int i = 0; i < nx; i++) for (int j = 0; j < nz; j++)
                {
                    int k = I(i, j); if (!covered[k]) continue;
                    bool edge = false;
                    for (int di = -1; di <= 1 && !edge; di++) for (int dj = -1; dj <= 1; dj++) { int a = i + di, b = j + dj; if (a >= 0 && b >= 0 && a < nx && b < nz && outside[I(a, b)]) { edge = true; break; } }
                    if (!edge) continue;
                    source[k] = true; dist[k] = 0; baseY[k] = height[k]; baseC[k] = colour[k]; heap.Add((0, k));
                }
            if (heap.Count == 0) { Report.Add("no open edge found: nothing built"); return; }
            while (heap.Count > 0)
            {
                var (d, k) = heap.Min; heap.Remove(heap.Min); if (d > dist[k]) continue; int i = k / nz, j = k % nz;
                for (int di = -1; di <= 1; di++) for (int dj = -1; dj <= 1; dj++)
                    {
                        int a = i + di, b = j + dj; if ((di == 0 && dj == 0) || a < 0 || b < 0 || a >= nx || b >= nz) continue; int m = I(a, b); if (!outside[m]) continue;
                        float nd = d + new Vector2(X[a] - X[i], Z[b] - Z[j]).magnitude;
                        if (nd < dist[m]) { if (dist[m] < float.MaxValue) heap.Remove((dist[m], m)); dist[m] = nd; baseY[m] = baseY[k]; baseC[m] = baseC[k]; heap.Add((nd, m)); }
                    }
            }
            // the base height spreads straight out from each edge point, so one odd edge point (a ramp top at the edge)
            // would draw a ridge to the horizon: away from the edge the base is smoothed over its neighbours
            for (int pass = 0; pass < 40; pass++)
            {
                var next = (float[])baseY.Clone();
                for (int i = 1; i < nx - 1; i++) for (int j = 1; j < nz - 1; j++)
                    {
                        int k = I(i, j); if (!outside[k] || dist[k] == float.MaxValue) continue;
                        float sum = 0; int cnt = 0;
                        for (int di = -1; di <= 1; di++) for (int dj = -1; dj <= 1; dj++) { int m = I(i + di, j + dj); if ((outside[m] && dist[m] < float.MaxValue) || source[m]) { sum += baseY[m]; cnt++; } }
                        next[k] = Mathf.Lerp(baseY[k], sum / cnt, Mathf.Clamp01((dist[k] - BankRun) / 60));
                    }
                baseY = next;
            }
            // heights and colours of the ring
            var forest = new Color(.15f, .25f, .11f); var meadow = new Color(.33f, .43f, .20f);
            var Y = new float[n]; var C = new Color[n]; var Yc = new float[n];
            for (int i = 0; i < nx; i++) for (int j = 0; j < nz; j++)
                {
                    int k = I(i, j); float x = X[i], z = Z[j];
                    if (source[k]) { Y[k] = height[k] - .3f; Yc[k] = Y[k]; C[k] = colour[k]; continue; }
                    if (!outside[k] || dist[k] == float.MaxValue) continue;
                    float d = dist[k], hill = Noise(x * .0021f, z * .0021f), ridge = 1 - Mathf.Abs(Noise(x * .0009f + 3, z * .0009f - 5) * 2 - 1);
                    float bank = BankRise * Mathf.SmoothStep(0, 1, d / BankRun);
                    float amp = 280 * Mathf.SmoothStep(0, 1, Mathf.InverseLerp(30, 1300, d));
                    Y[k] = baseY[k] + bank + amp * (.35f + .45f * hill + .35f * ridge) + 6 * Noise(x * .02f, z * .02f) * Mathf.Clamp01(d / 60);
                    Yc[k] = d <= BankRun + .01f ? Y[k] : Y[k] + WallHeight;
                    float wood = Mathf.Clamp01(Noise(x * .006f + 11, z * .006f + 4) * 1.6f - .35f);
                    var far = Color.Lerp(meadow, forest, wood) * (.9f + .2f * Noise(x * .03f, z * .03f));
                    C[k] = Color.Lerp(baseC[k], far, Mathf.SmoothStep(0, 1, d / 70));
                }
            // race lines (course scenes only): no collider near them (section 5A)
            var lines = new List<Vector3>();
            if (scene.name != RaceFlow.RoamScene)
                foreach (var g in scene.GetRootGameObjects())
                {
                    foreach (var r in g.GetComponentsInChildren<RaceDirector>(true)) if (r.road && r.road.points != null) lines.AddRange(r.road.points);
                    foreach (var w in g.GetComponentsInChildren<WoodlandRoute>(true)) if (w.points != null) lines.AddRange(w.points);
                }
            bool NearLine(Vector3 p) { foreach (var q in lines) if (new Vector2(q.x - p.x, q.z - p.z).sqrMagnitude < 25 * 25 && Mathf.Abs(q.y - p.y) < 30) return true; return false; }
            // meshes in chunks of 40 x 40 cells (culling), trees added to the chunk they stand in
            var root = new GameObject("World edge (0.81)"); SceneManager.MoveGameObjectToScene(root, scene);
            int chunk = 40, skippedNearLines = 0, wallCells = 0;
            var rng = new System.Random(81);
            for (int ci = 0; ci < nx - 1; ci += chunk) for (int cj = 0; cj < nz - 1; cj += chunk)
                {
                    var v = new List<Vector3>(); var col = new List<Color>(); var t = new List<int>(); var cv = new List<Vector3>(); var ct = new List<int>();
                    var map = new Dictionary<int, int>(); var cmap = new Dictionary<int, int>();
                    int Vert(int k) { if (!map.TryGetValue(k, out var x)) { x = v.Count; map[k] = x; v.Add(new Vector3(X[k / nz], Y[k], Z[k % nz])); col.Add(C[k]); } return x; }
                    int CVert(int k) { if (!cmap.TryGetValue(k, out var x)) { x = cv.Count; cmap[k] = x; cv.Add(new Vector3(X[k / nz], Yc[k], Z[k % nz])); } return x; }
                    for (int i = ci; i < Mathf.Min(ci + chunk, nx - 1); i++) for (int j = cj; j < Mathf.Min(cj + chunk, nz - 1); j++)
                        {
                            int a = I(i, j), b = I(i + 1, j), c = I(i + 1, j + 1), d = I(i, j + 1);
                            bool Ring(int k) => source[k] || (outside[k] && dist[k] < float.MaxValue);
                            if (!Ring(a) || !Ring(b) || !Ring(c) || !Ring(d)) continue;
                            if (source[a] && source[b] && source[c] && source[d]) continue;
                            t.AddRange(new[] { Vert(a), Vert(d), Vert(c), Vert(a), Vert(c), Vert(b) });
                            float near = Mathf.Min(Mathf.Min(dist[a], dist[b]), Mathf.Min(dist[c], dist[d]));
                            if (near <= ColliderBand)
                            {
                                var mid = new Vector3((X[i] + X[i + 1]) * .5f, (Y[a] + Y[c]) * .5f, (Z[j] + Z[j + 1]) * .5f);
                                if (lines.Count > 0 && NearLine(mid)) { skippedNearLines++; continue; }
                                ct.AddRange(new[] { CVert(a), CVert(d), CVert(c), CVert(a), CVert(c), CVert(b) }); wallCells++;
                            }
                            // trees on the near hills (cones, no collider)
                            float dc = (dist[a] + dist[c]) * .5f;
                            if (dc > 16 && dc < 110 && X[i + 1] - X[i] < Cell * 1.5f && Z[j + 1] - Z[j] < Cell * 1.5f)
                                for (int s = 0; s < 2; s++)
                                {
                                    float u = (float)rng.NextDouble(), w = (float)rng.NextDouble();
                                    float px = Mathf.Lerp(X[i], X[i + 1], u), pz = Mathf.Lerp(Z[j], Z[j + 1], w);
                                    float py = Mathf.Lerp(Mathf.Lerp(Y[a], Y[b], u), Mathf.Lerp(Y[d], Y[c], u), w);
                                    Cone(v, col, t, new Vector3(px, py - .5f, pz), 2.2f + 1.4f * (float)rng.NextDouble(), 8 + 7 * (float)rng.NextDouble(), (float)rng.NextDouble()); Trees++;
                                }
                        }
                    if (t.Count == 0) continue;
                    var go = new GameObject("World edge hills " + ci + "_" + cj); go.transform.SetParent(root.transform, false);
                    var mesh = new Mesh { name = go.name, indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 }; mesh.SetVertices(v); mesh.SetColors(col); mesh.SetTriangles(t, 0);
                    mesh.SetUVs(1, Enumerable.Repeat(Vector4.zero, v.Count).ToList()); mesh.RecalculateNormals(); mesh.RecalculateBounds();
                    go.AddComponent<MeshFilter>().sharedMesh = mesh; var mr = go.AddComponent<MeshRenderer>(); mr.sharedMaterial = ground;
                    mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; mr.receiveShadows = true;
                    go.AddComponent<VehicleMeshLifetime>().owned = mesh; Triangles += t.Count / 3;
                    if (ct.Count > 0)
                    {
                        var cm = new Mesh { name = go.name + " bank and wall", indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 }; cm.SetVertices(cv); cm.SetTriangles(ct, 0); cm.RecalculateBounds();
                        var cg = new GameObject("World edge bank and wall"); cg.transform.SetParent(go.transform, false); cg.AddComponent<MeshCollider>().sharedMesh = cm; cg.AddComponent<VehicleMeshLifetime>().owned = cm;
                    }
                }
            cache.Clear();
            Report.Add($"grid {nx} x {nz}; outside points {outside.Count(o => o)}, edge points {source.Count(s => s)}; ring {Triangles} triangles with {Trees} trees; bank/wall cells {wallCells}; cells left without a collider near a race line {skippedNearLines}");
        }

        static readonly Color Pine = new(.10f, .19f, .09f);
        static void Cone(List<Vector3> v, List<Color> c, List<int> t, Vector3 at, float r, float h, float turn)
        {
            int k = v.Count; var tint = Pine * (.85f + .3f * turn);
            v.Add(at + Vector3.up * h); c.Add(tint * 1.15f);
            for (int s = 0; s < 5; s++) { float a = (s + turn) * Mathf.PI * 2 / 5; v.Add(at + new Vector3(Mathf.Cos(a) * r, 0, Mathf.Sin(a) * r)); c.Add(tint); }
            for (int s = 0; s < 5; s++) t.AddRange(new[] { k, k + 1 + (s + 1) % 5, k + 1 + s });
        }

        // the highest drawn ground surface (a mesh or terrain collider, not a vehicle, trigger, barrier, tree or prop) under (x, z)
        static bool Ground(float x, float z, out RaycastHit ground)
        {
            ground = default; bool found = false; float best = float.NegativeInfinity;
            foreach (var h in Physics.RaycastAll(new Vector3(x, 1200, z), Vector3.down, 1600, ~0, QueryTriggerInteraction.Ignore))
                if (!h.rigidbody && (h.collider is MeshCollider || h.collider is TerrainCollider) && h.point.y > best && !h.collider.GetComponentInParent<WorldEdge>() && Visible(h.collider)) { best = h.point.y; ground = h; found = true; }
            return found;
        }
        // a drawn surface (a Ground piece, whose renderer the New scenery may hide, or anything with a visible renderer):
        // invisible barriers and walls are not ground
        static bool Visible(Collider c) => c is TerrainCollider || c.name.StartsWith("Ground") || (c.TryGetComponent<MeshRenderer>(out var r) && r.enabled);
        // the ground's own vertex colour where the ray met it (grass at the edge), else a plain grass tone
        static readonly Dictionary<Mesh, (Color[] c, int[] t)> cache = new();
        static Color Tint(RaycastHit h)
        {
            var fallback = new Color(.33f, .43f, .20f);
            if (h.collider is not MeshCollider mc || !mc.sharedMesh || h.triangleIndex < 0) return fallback;
            var m = mc.sharedMesh; if (!m.isReadable) return fallback;
            if (!cache.TryGetValue(m, out var e)) { e = (m.colors, m.triangles); cache[m] = e; }
            if (e.c == null || e.c.Length == 0 || h.triangleIndex * 3 + 2 >= e.t.Length) return fallback;
            var b = h.barycentricCoordinate; int i = h.triangleIndex * 3;
            var col = e.c[e.t[i]] * b.x + e.c[e.t[i + 1]] * b.y + e.c[e.t[i + 2]] * b.z;
            // keep it off the road colours (blue-ish greys are read as pavement by the ground shader)
            return col.b > col.r ? fallback : col;
        }
    }
}
