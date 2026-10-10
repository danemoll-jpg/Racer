using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Racer
{
    // 0.104 Part B: Hwy 92's roadside as it is today (Dan, 2026-10-10: "make that look more like it does today"), the same in every scene.
    // Planned offline from OpenStreetMap and the USGS NAIP aerial image (Tools/Report104/py/gen.py, placement only; every name is invented)
    // into Resources/Hwy92/Hwy92Roadside.json, in game x/z: buildings of the right kind at about the real footprint and orientation (scaled
    // with the game's stretch, 0.76 of the real length), fuel canopies with pumps, paved lots with stall lines and islands, entrances where
    // the real ones are, today's side streets, sidewalks, street lights, trees and shrubs, pylon signs, traffic lights at the real signalled
    // junctions with stop bars, crosswalks and lane arrows. Heights come from the ground at load: the flat layers lie on it, each building
    // stands level on its highest ground with a foundation down to its lowest (nothing floats or is buried).
    // Paint, lots, sidewalks and streets are drawn only (no collider): the drivable surface, the race lines, gates, AI lines and traffic
    // lanes are unchanged. Buildings, canopy columns, pumps, posts and poles are solid, all at least 10 m from every race line that leaves
    // the highway and 6 m beyond its edge. The plot of the subdivision across from S Cherokee Ln is left empty for the stunt course.
    // Clear() (before the scenery is fitted) removes the old placeholder roadside (the CR019 businesses outside the plot) and the trees
    // that stood where the lots and buildings are now.
    public sealed class Hwy92Roadside : MonoBehaviour
    {
        [Serializable] public sealed class Part { public float[] pts; public int[] fronts; public float[] roof; }
        [Serializable] public sealed class Building { public string kind, name; public float h; public string[] tenants; public Part[] parts; public long osm; }
        [Serializable] public sealed class Canopy { public string kind, name; public float[] pts; public float h; }
        [Serializable] public sealed class Flat { public float[] asphalt, street, sidewalk, island, white, yellow; }
        [Serializable] public sealed class Pylon { public float x, z, yaw; public string label, style, kind; }
        [Serializable] public sealed class Signal { public float[] pole, arm, face, heads; }
        [Serializable] public sealed class Police { public float[] pos, forward; }
        [Serializable] public sealed class ClearMask { public float x0, z0; public int w, h; public string bits; }
        [Serializable] public sealed class Data
        {
            public string version, source; public Building[] buildings; public Canopy[] canopies; public Flat flat; public float[] lights, trees, shrubs, plot;
            public Pylon[] pylons; public Signal[] signals; public Police[] police; public string[] remove, keepInPlot; public ClearMask clear;
        }

        static Data data;
        public static Data Plan { get { if (data == null) { var t = Resources.Load<TextAsset>("Hwy92/Hwy92Roadside"); data = t ? JsonUtility.FromJson<Data>(t.text) : new Data(); } return data; } }
        // the hidden-police places on Hwy 92 (in a lot, nose to the road), for HiddenPolice
        public static IEnumerable<(Vector3 pos, Vector3 forward)> PoliceSpots(Scene scene)
        {
            if (Disabled) yield break;
            foreach (var p in Plan.police ?? Array.Empty<Police>())
            {
                float y = GroundY(p.pos[0], p.pos[1]); if (float.IsNaN(y)) continue;
                yield return (new Vector3(p.pos[0], y, p.pos[1]), new Vector3(p.forward[0], 0, p.forward[1]).normalized);
            }
        }
        public readonly List<string> Report = new();
        public static readonly List<string> Removed = new();
        public static bool Disabled; // evidence only (Report104PlayerCheck -p104off: the world before this round); never set in play
        public int Solids { get; private set; }
        static bool IsWorld(Scene scene) => scene.GetRootGameObjects().Any(g => g.name == "Remembered houses and approximate buildings");

        // ---------------------------------------------------------------- the ground
        static readonly RaycastHit[] hits = new RaycastHit[32];
        static readonly Dictionary<long, float> cache = new();
        static float GroundY(float x, float z)
        {
            long key = ((long)Mathf.RoundToInt(x * 50) << 32) ^ (uint)Mathf.RoundToInt(z * 50);
            if (cache.TryGetValue(key, out var y)) return y;
            int n = Physics.RaycastNonAlloc(new Vector3(x, 600, z), Vector3.down, hits, 1000, ~0, QueryTriggerInteraction.Ignore); float best = float.NaN;
            for (int i = 0; i < n; i++) { var c = hits[i].collider; if (!c || !c.name.StartsWith("Ground_")) continue; if (float.IsNaN(best) || hits[i].point.y > best) best = hits[i].point.y; }
            cache[key] = best; return best;
        }
        float lastGood = 8;
        float Ground(float x, float z) { var y = GroundY(x, z); if (float.IsNaN(y)) return lastGood; lastGood = y; return y; }

        // ---------------------------------------------------------------- 1. before the scenery is fitted: the old roadside out
        public static void Clear(Scene scene)
        {
            Removed.Clear(); cache.Clear();
            var plan = Plan; if (Disabled || plan.remove == null || !IsWorld(scene)) return;
            var root = scene.GetRootGameObjects().First(g => g.name == "Remembered houses and approximate buildings");
            var regions = new List<Bounds>();
            foreach (var name in plan.remove)
            {
                var site = root.transform.Find(name); if (!site || !site.gameObject.activeSelf) continue;
                Bounds? b = null;
                foreach (var r in site.GetComponentsInChildren<Renderer>(true)) { if (b == null) b = r.bounds; else { var x = b.Value; x.Encapsulate(r.bounds); b = x; } }
                foreach (var c in site.GetComponentsInChildren<Collider>(true)) { if (b == null) b = c.bounds; else { var x = b.Value; x.Encapsulate(c.bounds); b = x; } }
                if (b is Bounds bb) { bb.Expand(new Vector3(1, 2, 1)); regions.Add(bb); }
                site.gameObject.SetActive(false); Removed.Add(name);
            }
            // their triangles out of the town's merged render batches (the copies stay readable for SceneryBuildings)
            var batches = scene.GetRootGameObjects().FirstOrDefault(g => g.name == "Phase 6 - architectural render batches");
            if (batches && regions.Count > 0)
                foreach (var mf in batches.GetComponentsInChildren<MeshFilter>(true)) Trim(mf, (a, b, c) => { var m = (a + b + c) / 3; foreach (var r in regions) if (r.Contains(m)) return true; return false; }, false);
            // the trees standing where the lots and buildings are now: trunk colliders, and the old vegetation's crowns and bark (whole pieces)
            var mask = Mask(plan.clear); if (mask == null) return;
            int trunks = 0, pieces = 0;
            foreach (var box in scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<BoxCollider>(true)))
            {
                if (!box || box.name.IndexOf("trunk", StringComparison.OrdinalIgnoreCase) < 0) continue;
                var t = box.transform; var bottom = t.TransformPoint(box.center - Vector3.up * box.size.y * .5f);
                if (!mask(bottom.x, bottom.z)) continue;
                var size = Vector3.Scale(box.size, t.lossyScale);
                SceneryTrees.Cleared.Add((bottom, Mathf.Min(Mathf.Abs(size.x), Mathf.Abs(size.z)), Mathf.Abs(size.y), "Hwy 92 roadside (0.104)", t.name));
                box.enabled = false; if (t.GetComponents<Component>().Length == 2 && t.childCount == 0) t.gameObject.SetActive(false); trunks++;
            }
            var st = plan.clear;
            foreach (var r in scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<MeshRenderer>(true)))
            {
                if (!SceneryTrees.IsOldVegetation(r)) continue;
                var bd = r.bounds; if (bd.max.x < st.x0 || bd.min.x > st.x0 + st.w || bd.max.z < st.z0 || bd.min.z > st.z0 + st.h) continue;
                pieces += TrimPieces(r.GetComponent<MeshFilter>(), mask);
            }
            Debug.Log($"Hwy 92 roadside: removed {Removed.Count} old placeholder businesses, {trunks} tree trunks and {pieces} old vegetation pieces where the lots are");
        }
        static Func<float, float, bool> Mask(ClearMask m)
        {
            if (m == null || string.IsNullOrEmpty(m.bits)) return null;
            byte[] packed;
            using (var src = new System.IO.MemoryStream(Convert.FromBase64String(m.bits)))
            {
                // zlib: a 2-byte header, then deflate
                src.ReadByte(); src.ReadByte();
                using var z = new System.IO.Compression.DeflateStream(src, System.IO.Compression.CompressionMode.Decompress); using var o = new System.IO.MemoryStream(); z.CopyTo(o); packed = o.ToArray();
            }
            float x0 = m.x0, z0 = m.z0; int w = m.w, h = m.h;
            return (x, z) =>
            {
                int i = Mathf.FloorToInt(x - x0), j = Mathf.FloorToInt(z - z0); if (i < 0 || j < 0 || i >= w || j >= h) return false;
                int k = j * w + i; return (packed[k >> 3] & (0x80 >> (k & 7))) != 0;
            };
        }
        // a copy of the mesh without the triangles `drop` names (world positions); returns how many went
        static int Trim(MeshFilter mf, Func<Vector3, Vector3, Vector3, bool> drop, bool _)
        {
            if (!mf || !mf.sharedMesh || !mf.sharedMesh.isReadable) return 0;
            var mesh = mf.sharedMesh; var v = mesh.vertices; var w = mf.transform.localToWorldMatrix; int removed = 0;
            var subs = new List<int[]>();
            for (int sm = 0; sm < mesh.subMeshCount; sm++)
            {
                var t = mesh.GetTriangles(sm); var keep = new List<int>(t.Length);
                for (int i = 0; i < t.Length; i += 3)
                    if (drop(w.MultiplyPoint3x4(v[t[i]]), w.MultiplyPoint3x4(v[t[i + 1]]), w.MultiplyPoint3x4(v[t[i + 2]]))) removed++; else { keep.Add(t[i]); keep.Add(t[i + 1]); keep.Add(t[i + 2]); }
                subs.Add(keep.ToArray());
            }
            if (removed == 0) return 0;
            var copy = UnityEngine.Object.Instantiate(mesh); copy.name = mesh.name + " (Hwy 92 roadside 0.104)";
            for (int sm = 0; sm < subs.Count; sm++) copy.SetTriangles(subs[sm], sm);
            mf.sharedMesh = copy; return removed;
        }
        // whole connected pieces (by shared position) whose middle lies in the mask
        static int TrimPieces(MeshFilter mf, Func<float, float, bool> mask)
        {
            if (!mf || !mf.sharedMesh || !mf.sharedMesh.isReadable) return 0;
            var mesh = mf.sharedMesh; var v = mesh.vertices; var w = mf.transform.localToWorldMatrix;
            var parent = new int[v.Length]; for (int i = 0; i < parent.Length; i++) parent[i] = i;
            int Find(int x) { while (parent[x] != x) { parent[x] = parent[parent[x]]; x = parent[x]; } return x; }
            var byPos = new Dictionary<Vector3Int, int>();
            for (int i = 0; i < v.Length; i++) { var k = Vector3Int.RoundToInt(v[i] * 200); if (byPos.TryGetValue(k, out var j)) parent[Find(i)] = Find(j); else byPos[k] = i; }
            var all = new List<int[]>(); for (int sm = 0; sm < mesh.subMeshCount; sm++) all.Add(mesh.GetTriangles(sm));
            foreach (var t in all) for (int i = 0; i < t.Length; i += 3) { int a = Find(t[i]); parent[Find(t[i + 1])] = a; parent[Find(t[i + 2])] = a; }
            var lo = new Dictionary<int, Vector3>(); var hi = new Dictionary<int, Vector3>();
            for (int i = 0; i < v.Length; i++) { int r = Find(i); var p = w.MultiplyPoint3x4(v[i]); if (lo.TryGetValue(r, out var a)) { lo[r] = Vector3.Min(a, p); hi[r] = Vector3.Max(hi[r], p); } else { lo[r] = p; hi[r] = p; } }
            var gone = new HashSet<int>(); foreach (var r in lo.Keys) { var c = (lo[r] + hi[r]) * .5f; if (mask(c.x, c.z)) gone.Add(r); }
            if (gone.Count == 0) return 0;
            var copy = UnityEngine.Object.Instantiate(mesh); copy.name = mesh.name + " (Hwy 92 roadside 0.104)";
            for (int sm = 0; sm < all.Count; sm++) { var t = all[sm]; var keep = new List<int>(t.Length); for (int i = 0; i < t.Length; i += 3) if (!gone.Contains(Find(t[i]))) { keep.Add(t[i]); keep.Add(t[i + 1]); keep.Add(t[i + 2]); } copy.SetTriangles(keep, sm); }
            mf.sharedMesh = copy; return gone.Count;
        }

        // ---------------------------------------------------------------- 2. the new roadside
        public static Hwy92Roadside Attach(GameObject host) => host.GetComponent<Hwy92Roadside>() ?? host.AddComponent<Hwy92Roadside>();
        void Start() { if (!Disabled && IsWorld(gameObject.scene)) Build(gameObject.scene); }

        sealed class Mb
        {
            public readonly List<Vector3> v = new(); public readonly List<Vector3> n = new(); public readonly List<Color> c = new(); public readonly List<int> t = new();
            public void Tri(Vector3 a, Vector3 b, Vector3 d, Color col)
            {
                var nn = Vector3.Cross(b - a, d - a); if (nn.sqrMagnitude < 1e-12f) return; nn.Normalize();
                int i = v.Count; v.Add(a); v.Add(b); v.Add(d); n.Add(nn); n.Add(nn); n.Add(nn); c.Add(col); c.Add(col); c.Add(col); t.Add(i); t.Add(i + 1); t.Add(i + 2);
            }
            public void Quad(Vector3 a, Vector3 b, Vector3 d, Vector3 e, Color col, Vector3 outward)
            {
                if (Vector3.Dot(Vector3.Cross(b - a, d - a), outward) < 0) (b, e) = (e, b);
                Tri(a, b, d, col); Tri(a, d, e, col);
            }
            // an oriented box: centre bottom, its forward (horizontal), size (x across, y up, z along forward)
            public void Box(Vector3 bottom, Vector3 forward, Vector3 size, Color col, Color? top = null)
            {
                forward.y = 0; forward.Normalize(); var right = Vector3.Cross(Vector3.up, forward);
                Vector3 P(float x, float y, float z) => bottom + right * (x * size.x * .5f) + Vector3.up * (y * size.y) + forward * (z * size.z * .5f);
                Quad(P(-1, 0, 1), P(1, 0, 1), P(1, 1, 1), P(-1, 1, 1), col, forward);
                Quad(P(1, 0, -1), P(-1, 0, -1), P(-1, 1, -1), P(1, 1, -1), col, -forward);
                Quad(P(1, 0, 1), P(1, 0, -1), P(1, 1, -1), P(1, 1, 1), col, right);
                Quad(P(-1, 0, -1), P(-1, 0, 1), P(-1, 1, 1), P(-1, 1, -1), col, -right);
                Quad(P(-1, 1, 1), P(1, 1, 1), P(1, 1, -1), P(-1, 1, -1), top ?? col, Vector3.up);
                Quad(P(-1, 0, -1), P(1, 0, -1), P(1, 0, 1), P(-1, 0, 1), col, Vector3.down);
            }
            public void Blob(Vector3 centre, Vector3 radius, Color col, int seed)
            {
                // a low-poly crown: an icosahedron, a little irregular
                float g = (1 + Mathf.Sqrt(5)) / 2; var r = new System.Random(seed);
                var p = new[] { new Vector3(-1, g, 0), new Vector3(1, g, 0), new Vector3(-1, -g, 0), new Vector3(1, -g, 0), new Vector3(0, -1, g), new Vector3(0, 1, g), new Vector3(0, -1, -g), new Vector3(0, 1, -g), new Vector3(g, 0, -1), new Vector3(g, 0, 1), new Vector3(-g, 0, -1), new Vector3(-g, 0, 1) };
                for (int i = 0; i < p.Length; i++) { p[i] = p[i].normalized * (.85f + .3f * (float)r.NextDouble()); p[i] = centre + Vector3.Scale(p[i], radius); }
                int[] f = { 0, 11, 5, 0, 5, 1, 0, 1, 7, 0, 7, 10, 0, 10, 11, 1, 5, 9, 5, 11, 4, 11, 10, 2, 10, 7, 6, 7, 1, 8, 3, 9, 4, 3, 4, 2, 3, 2, 6, 3, 6, 8, 3, 8, 9, 4, 9, 5, 2, 4, 11, 6, 2, 10, 8, 6, 7, 9, 8, 1 };
                for (int i = 0; i < f.Length; i += 3) { var shade = .88f + .24f * (float)r.NextDouble(); var cc = col * shade; cc.a = 1; Tri(p[f[i]], p[f[i + 2]], p[f[i + 1]], cc); }
            }
            public bool Empty => t.Count == 0;
            public Mesh Mesh(string name, bool keepReadable = false)
            {
                var m = new Mesh { name = name, indexFormat = v.Count > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16 };
                m.SetVertices(v); m.SetNormals(n); m.SetColors(c); m.SetTriangles(t, 0); m.RecalculateBounds(); m.UploadMeshData(!keepReadable); return m;
            }
        }
        static Material building, asphalt, street, lettering; static Font font;
        Transform root; readonly Dictionary<(int chunk, string layer), Mb> layers = new();
        Mb Layer(float x, string layer) { int k = Mathf.FloorToInt((x + 800) / 200); if (!layers.TryGetValue((k, layer), out var m)) layers[(k, layer)] = m = new Mb(); return m; }
        GameObject solids;
        void Solid(Vector3 bottom, Vector3 forward, Vector3 size)
        {
            var g = new GameObject("Solid"); g.transform.SetParent(solids.transform, false); forward.y = 0;
            g.transform.SetPositionAndRotation(bottom + Vector3.up * size.y * .5f, Quaternion.LookRotation(forward.sqrMagnitude > 1e-6f ? forward : Vector3.forward));
            g.AddComponent<BoxCollider>().size = size; Solids++;
        }

        public void Build(Scene scene)
        {
            var plan = Plan; if (plan.buildings == null) { Report.Add("no plan"); return; }
            building ??= Resources.Load<Material>("Scenery/Building"); lettering ??= Resources.Load<Material>("Scenery/SignLettering");
            font ??= Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (!asphalt)
            {
                var source = Resources.Load<Material>("Scenery/Paved");
                if (source) { asphalt = new Material(source) { name = "Hwy 92 lots (0.104)" }; asphalt.SetVector("_Asphalt", SceneryPaving.Driveway); street = new Material(source) { name = "Hwy 92 side streets (0.104)" }; street.SetVector("_Asphalt", SceneryPaving.Road); }
            }
            if (!building || !asphalt || !lettering || !font) { Debug.LogWarning("Hwy 92 roadside: material or font missing"); return; }
            if (font.material && font.material.mainTexture) lettering.mainTexture = font.material.mainTexture;
            var watch = System.Diagnostics.Stopwatch.StartNew(); Physics.SyncTransforms();
            var go = new GameObject("Hwy 92 roadside like today (0.104)"); SceneManager.MoveGameObjectToScene(go, scene); root = go.transform;
            solids = new GameObject("Hwy 92 roadside solids"); solids.transform.SetParent(root, false);
            // flat layers (draped on the ground's own 2 m grid)
            Drape(plan.flat.asphalt, "asphalt", .05f, Color.white);
            Drape(plan.flat.street, "street", .05f, Color.white);
            Drape(plan.flat.sidewalk, "flat", .075f, new Color(.66f, .65f, .62f));
            Drape(plan.flat.island, "flat", .14f, new Color(.30f, .42f, .20f));
            Drape(plan.flat.white, "flat", .095f, new Color(.86f, .86f, .84f));
            Drape(plan.flat.yellow, "flat", .095f, new Color(.86f, .66f, .16f));
            foreach (var b in plan.buildings) foreach (var p in b.parts) BuildingPart(b, p);
            foreach (var c in plan.canopies) CanopyAt(c);
            for (int i = 0; i + 2 < plan.lights.Length; i += 3) Light(plan.lights[i], plan.lights[i + 1], plan.lights[i + 2]);
            for (int i = 0; i + 3 < plan.trees.Length; i += 4) Tree(plan.trees[i], plan.trees[i + 1], plan.trees[i + 2], plan.trees[i + 3] > .5f, i);
            for (int i = 0; i + 2 < plan.shrubs.Length; i += 3) { float x = plan.shrubs[i], z = plan.shrubs[i + 1], s = plan.shrubs[i + 2]; Layer(x, "solid").Blob(new Vector3(x, Ground(x, z) + .35f * s, z), new Vector3(.75f, .5f, .75f) * s, new Color(.20f, .33f, .14f), i); }
            foreach (var p in plan.pylons) PylonAt(p);
            foreach (var s in plan.signals) SignalAt(s);
            // the meshes: buildings, props and paint in the building material (props cast shadows; flat layers do not), lots and streets paved
            int tris = 0;
            foreach (var kv in layers.OrderBy(k => k.Key.chunk))
            {
                if (kv.Value.Empty) continue; tris += kv.Value.t.Count / 3;
                var g = new GameObject($"Hwy 92 {kv.Key.layer} {kv.Key.chunk}", typeof(MeshFilter), typeof(MeshRenderer)); g.transform.SetParent(root, false);
                g.GetComponent<MeshFilter>().sharedMesh = kv.Value.Mesh(g.name); var r = g.GetComponent<MeshRenderer>();
                r.sharedMaterial = kv.Key.layer == "asphalt" ? asphalt : kv.Key.layer == "street" ? street : building;
                r.shadowCastingMode = kv.Key.layer == "solid" ? ShadowCastingMode.On : ShadowCastingMode.Off;
            }
            layers.Clear();
            Report.Add($"built in {watch.ElapsedMilliseconds} ms: {plan.buildings.Length} buildings, {plan.canopies.Length} canopies, {plan.lights.Length / 3} lights, {plan.trees.Length / 4} trees, {plan.pylons.Length} pylon signs, {plan.signals.Length} signal poles; {tris} triangles, {Solids} solids, {labels.Count} lettered faces");
            Debug.Log("Hwy 92 roadside: " + Report[^1]);
        }

        // ---------------------------------------------------------------- flat layers
        static readonly List<Vector2> clipA = new(), clipB = new();
        void Drape(float[] tri, string layer, float lift, Color col)
        {
            if (tri == null) return;
            for (int i = 0; i + 5 < tri.Length; i += 6)
            {
                var a = new Vector2(tri[i], tri[i + 1]); var b = new Vector2(tri[i + 2], tri[i + 3]); var c = new Vector2(tri[i + 4], tri[i + 5]);
                int i0 = Mathf.FloorToInt(Mathf.Min(a.x, Mathf.Min(b.x, c.x)) / 2), i1 = Mathf.FloorToInt(Mathf.Max(a.x, Mathf.Max(b.x, c.x)) / 2);
                int j0 = Mathf.FloorToInt(Mathf.Min(a.y, Mathf.Min(b.y, c.y)) / 2), j1 = Mathf.FloorToInt(Mathf.Max(a.y, Mathf.Max(b.y, c.y)) / 2);
                var m = Layer((a.x + b.x + c.x) / 3, layer);
                for (int ci = i0; ci <= i1; ci++)
                    for (int cj = j0; cj <= j1; cj++)
                    {
                        clipA.Clear(); clipA.Add(a); clipA.Add(b); clipA.Add(c);
                        ClipAxis(clipA, clipB, 0, ci * 2f, true); ClipAxis(clipB, clipA, 0, ci * 2f + 2, false); ClipAxis(clipA, clipB, 1, cj * 2f, true); ClipAxis(clipB, clipA, 1, cj * 2f + 2, false);
                        if (clipA.Count < 3) continue;
                        var p0 = new Vector3(clipA[0].x, Ground(clipA[0].x, clipA[0].y) + lift, clipA[0].y);
                        for (int k = 1; k + 1 < clipA.Count; k++)
                        {
                            var p1 = new Vector3(clipA[k].x, Ground(clipA[k].x, clipA[k].y) + lift, clipA[k].y); var p2 = new Vector3(clipA[k + 1].x, Ground(clipA[k + 1].x, clipA[k + 1].y) + lift, clipA[k + 1].y);
                            // clockwise from above (Unity's front face): the face points up
                            if (Vector3.Cross(p1 - p0, p2 - p0).y > 0) m.Tri(p0, p1, p2, col); else m.Tri(p0, p2, p1, col);
                        }
                    }
            }
        }
        static void ClipAxis(List<Vector2> src, List<Vector2> dst, int axis, float at, bool keepAbove)
        {
            dst.Clear(); int n = src.Count; if (n == 0) return;
            for (int i = 0; i < n; i++)
            {
                var p = src[i]; var q = src[(i + 1) % n]; float pv = axis == 0 ? p.x : p.y, qv = axis == 0 ? q.x : q.y;
                bool pin = keepAbove ? pv >= at : pv <= at, qin = keepAbove ? qv >= at : qv <= at;
                if (pin) dst.Add(p);
                if (pin != qin) { float t = (at - pv) / (qv - pv); dst.Add(Vector2.Lerp(p, q, t)); }
            }
        }

        // ---------------------------------------------------------------- buildings
        static Color C(float r, float g, float b, float a = 1) => new(r, g, b, a);
        static Color Glass(float glow) => C(.16f, .23f, .28f, glow);  // alpha below 1: lit at night (Racer/Building)
        static Color Pick(string seed, params Color[] cs) { int h = seed.Aggregate(23, (a, ch) => unchecked(a * 31 + ch)); return cs[Mathf.Abs(h) % cs.Length]; }
        static readonly Color[] Stucco = { C(.80f, .74f, .62f), C(.74f, .70f, .63f), C(.84f, .81f, .74f), C(.66f, .62f, .55f) };
        static readonly Color[] Brick = { C(.55f, .30f, .23f), C(.48f, .27f, .21f), C(.62f, .42f, .32f) };
        static readonly Color[] Panel = { C(.70f, .71f, .70f), C(.58f, .60f, .62f), C(.78f, .76f, .70f) };
        static readonly Color[] Accent = { C(.62f, .16f, .14f), C(.14f, .30f, .52f), C(.16f, .42f, .30f), C(.74f, .52f, .14f), C(.40f, .22f, .46f), C(.12f, .44f, .46f), C(.20f, .20f, .22f) };
        static readonly Color[] HouseWalls = { C(.84f, .82f, .74f), C(.70f, .74f, .78f), C(.78f, .70f, .56f), C(.62f, .66f, .58f), C(.86f, .86f, .82f) };
        static readonly Color[] HouseRoofs = { C(.24f, .24f, .26f), C(.32f, .26f, .22f), C(.36f, .38f, .40f) };

        void BuildingPart(Building b, Part p)
        {
            int n = p.pts.Length / 2; if (n < 3) return;
            var pts = new Vector2[n]; for (int i = 0; i < n; i++) pts[i] = new Vector2(p.pts[2 * i], p.pts[2 * i + 1]);
            // level on the highest ground under it, a foundation down to the lowest
            float hi = float.MinValue, lo = float.MaxValue; var min = pts[0]; var max = pts[0];
            foreach (var q in pts) { float gy = Ground(q.x, q.y); hi = Mathf.Max(hi, gy); lo = Mathf.Min(lo, gy); min = Vector2.Min(min, q); max = Vector2.Max(max, q); }
            for (float x = min.x + 1.5f; x < max.x; x += 3) for (float z = min.y + 1.5f; z < max.y; z += 3) if (Inside(pts, new Vector2(x, z))) { float gy = Ground(x, z); hi = Mathf.Max(hi, gy); lo = Mathf.Min(lo, gy); }
            float floor = hi + .15f, h = b.h, top = floor + h; bool house = b.kind == "house";
            string seed = b.osm + b.kind; var m = Layer(pts[0].x, "solid");
            Color wall = house ? Pick(seed, HouseWalls) : b.kind is "office" or "civic" or "senior" or "apartments" or "bank" or "church" ? Pick(seed, Brick.Concat(Stucco).ToArray()) : b.kind is "storage" or "auto" or "fitness" ? Pick(seed, Panel) : Pick(seed, Stucco.Concat(Brick).ToArray());
            Color trim = Pick(seed + "t", C(.90f, .89f, .85f), C(.30f, .28f, .26f), C(.82f, .78f, .70f)), accent = Pick(seed + "a", Accent), foundation = C(.50f, .49f, .47f), roof = C(.84f, .84f, .82f); // flat roofs light, like the real ones from above
            float parapet = house ? 0 : .8f, eave = top;
            for (int i = 0; i < n; i++)
            {
                var a = pts[i]; var c = pts[(i + 1) % n]; var e = c - a; float len = e.magnitude; if (len < .05f) continue;
                var outward = new Vector3(e.y, 0, -e.x) / len; Vector3 A(float y) => new(a.x, y, a.y); Vector3 B(float y) => new(c.x, y, c.y);
                m.Quad(A(lo - .4f), B(lo - .4f), B(floor), A(floor), foundation, outward);
                if (house) m.Quad(A(floor), B(floor), B(top), A(top), wall, outward);
                else
                {
                    m.Quad(A(floor), B(floor), B(top - 1.1f), A(top - 1.1f), wall, outward);
                    m.Quad(A(top - 1.1f), B(top - 1.1f), B(top + parapet), A(top + parapet), b.kind is "strip" or "retail" or "grocery" or "fastfood" or "pharmacy" or "convenience" or "liquor" ? accent : trim, outward);
                    m.Quad(A(top + parapet), B(top + parapet), B(top), A(top), trim * .9f, -outward); // the parapet's inner face
                }
                Facade(m, b, a, c, outward, floor, top, p.fronts != null && i < p.fronts.Length ? p.fronts[i] : 0, wall, accent, trim, i);
            }
            // roof
            if (house) eave = GableRoof(m, pts, top, Pick(seed + "r", HouseRoofs));
            else for (int i = 0; i + 5 < p.roof.Length; i += 6) m.Tri(new Vector3(p.roof[i], top, p.roof[i + 1]), new Vector3(p.roof[i + 4], top, p.roof[i + 5]), new Vector3(p.roof[i + 2], top, p.roof[i + 3]), roof);
            if (b.kind == "church") Steeple(m, pts, p.fronts, top);
            // one solid: the walls from the foundation to the roof
            var col = new Mb();
            for (int i = 0; i < n; i++) { var a = pts[i]; var c = pts[(i + 1) % n]; var e = c - a; if (e.sqrMagnitude < .0025f) continue; col.Quad(new Vector3(a.x, lo - .4f, a.y), new Vector3(c.x, lo - .4f, c.y), new Vector3(c.x, top, c.y), new Vector3(a.x, top, a.y), Color.white, new Vector3(e.y, 0, -e.x)); }
            for (int i = 0; i + 5 < p.roof.Length; i += 6) col.Tri(new Vector3(p.roof[i], top, p.roof[i + 1]), new Vector3(p.roof[i + 4], top, p.roof[i + 5]), new Vector3(p.roof[i + 2], top, p.roof[i + 3]), Color.white);
            var g = new GameObject(string.IsNullOrEmpty(b.name) ? b.kind : b.name); g.transform.SetParent(solids.transform, false);
            g.AddComponent<MeshCollider>().sharedMesh = col.Mesh(g.name + " collider", true); Solids++;
            Buildings.Add((g.name, b.kind, new Bounds(new Vector3((min.x + max.x) / 2, (lo + top) / 2, (min.y + max.y) / 2), new Vector3(max.x - min.x, top - lo, max.y - min.y)), floor - hi, lo));
        }
        public readonly List<(string name, string kind, Bounds bounds, float lift, float lowest)> Buildings = new();
        static bool Inside(Vector2[] poly, Vector2 p)
        {
            bool inside = false;
            for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i++)
                if ((poly[i].y > p.y) != (poly[j].y > p.y) && p.x < (poly[j].x - poly[i].x) * (p.y - poly[i].y) / (poly[j].y - poly[i].y) + poly[i].x) inside = !inside;
            return inside;
        }
        void Facade(Mb m, Building b, Vector2 a2, Vector2 c2, Vector3 outward, float floor, float top, int front, Color wall, Color accent, Color trim, int edge)
        {
            var a = new Vector3(a2.x, 0, a2.y); var c = new Vector3(c2.x, 0, c2.y); var u = (c - a).normalized; float len = Vector3.Distance(a, c);
            Vector3 P(float along, float y, float out_) => a + u * along + Vector3.up * y + outward * out_;
            void Panel(float u0, float u1, float y0, float y1, float d, Color col) { m.Quad(P(u0, y0, d), P(u1, y0, d), P(u1, y1, d), P(u0, y1, d), col, outward); }
            void Slab(float u0, float u1, float y0, float y1, float d0, float d1, Color col)
            {
                m.Quad(P(u0, y0, d1), P(u1, y0, d1), P(u1, y1, d1), P(u0, y1, d1), col, outward); m.Quad(P(u0, y1, d0), P(u0, y1, d1), P(u1, y1, d1), P(u1, y1, d0), col, Vector3.up);
                m.Quad(P(u0, y0, d0), P(u1, y0, d0), P(u1, y0, d1), P(u0, y0, d1), col, Vector3.down);
                m.Quad(P(u0, y0, d0), P(u0, y0, d1), P(u0, y1, d1), P(u0, y1, d0), col, -u); m.Quad(P(u1, y0, d1), P(u1, y0, d0), P(u1, y1, d0), P(u1, y1, d1), col, u);
            }
            bool shop = b.kind is "strip" or "retail" or "grocery" or "fastfood" or "pharmacy" or "convenience" or "liquor" or "vet" or "venue";
            if (b.kind == "house")
            {
                for (float x = 1.2f; x + 1.0f < len - .6f; x += 3.2f) Panel(x, x + 1.0f, floor + 1.0f, floor + 2.3f, .03f, Glass(.6f));
                if (front == 2 && len > 5) Panel(len / 2 - .5f, len / 2 + .5f, floor, floor + 2.1f, .03f, C(.30f, .20f, .14f));
                return;
            }
            if (shop && front > 0 && len > 4)
            {
                // storefront: glass between slim mullions, doors, a sign band with an awning over the glass on strip shops
                float y0 = floor + .35f, y1 = floor + Mathf.Min(3.2f, top - floor - 1.6f);
                int panes = Mathf.Max(1, Mathf.FloorToInt((len - 1.2f) / 2.4f)); float pw = (len - 1.2f) / panes;
                for (int k = 0; k < panes; k++) { float s = .6f + k * pw; bool door = k == panes / 2; Panel(s + .06f, s + pw - .06f, door ? floor + .02f : y0, y1, .04f, door ? C(.10f, .12f, .14f, .5f) : Glass(.35f)); }
                Slab(.4f, len - .4f, y1 + .05f, y1 + .25f, 0, .1f, trim);
                if (b.kind is "strip" && b.tenants != null && b.tenants.Length > 0 && front == 2)
                {
                    int t = b.tenants.Length; float sw = len / t;
                    for (int k = 0; k < t; k++)
                    {
                        var col = Accent[(edge * 3 + k * 5 + b.tenants[k].Length) % Accent.Length];
                        Slab(k * sw + .3f, (k + 1) * sw - .3f, y1 + .45f, y1 + 1.35f, 0, .12f, col);
                        Slab(k * sw + .5f, (k + 1) * sw - .5f, y1 - .25f, y1 + .3f, 0, 1.3f, col * .85f);
                        Label(b.tenants[k], P(k * sw + sw / 2, y1 + .9f, .14f), outward, sw - 1.2f, .7f, Color.white);
                    }
                }
                else if (front == 2 || (front == 1 && edge == 0))
                {
                    Slab(1, len - 1, y1 + .45f, y1 + 1.5f, 0, .12f, accent);
                    if (!string.IsNullOrEmpty(b.name)) Label(b.name, P(len / 2, y1 + .98f, .14f), outward, Mathf.Min(len - 3, 14), .85f, Color.white);
                }
                return;
            }
            if (b.kind == "auto" && front > 0)
            {
                for (float x = 1; x + 3.2f < len - .6f; x += 4) Panel(x, x + 3.2f, floor + .02f, floor + 3.6f, .04f, C(.78f, .78f, .76f));
                if (front == 2 && !string.IsNullOrEmpty(b.name)) Label(b.name, P(len / 2, top - .55f, .06f), outward, len - 2, .7f, Color.white);
                return;
            }
            // offices and the rest: rows of windows, a floor every 3.5 m, an entrance on the front
            int floors = Mathf.Max(1, Mathf.FloorToInt((top - floor - .6f) / 3.5f));
            for (int f = 0; f < floors; f++)
            {
                float y = floor + f * 3.5f;
                if (b.kind == "storage") { Slab(0, len, y + 2.6f, y + 3.1f, 0, .04f, Pick(b.name + "s", C(.80f, .45f, .16f), C(.20f, .40f, .62f))); continue; }
                for (float x = 1.0f; x + 1.4f < len - .6f; x += 2.8f) Panel(x, x + 1.4f, y + 1.0f, y + 2.5f, .03f, Glass(f == 0 ? .5f : .7f));
            }
            if (front == 2 && len > 6)
            {
                Slab(len / 2 - 2, len / 2 + 2, floor + 2.9f, floor + 3.2f, 0, 1.8f, trim);
                Panel(len / 2 - 1, len / 2 + 1, floor + .02f, floor + 2.6f, .04f, C(.10f, .12f, .14f, .5f));
                if (!string.IsNullOrEmpty(b.name) && b.kind != "apartments") Label(b.name, P(len / 2, top - .55f, .06f), outward, Mathf.Min(len - 3, 12), .7f, b.kind == "storage" ? Color.white : C(.95f, .95f, .92f));
            }
        }
        float GableRoof(Mb m, Vector2[] pts, float eave, Color col)
        {
            // over the footprint's oriented box (the smallest of the boxes along its edges), the ridge along the long side, gable ends
            float best = float.MaxValue; Vector2 bu = Vector2.right, bv = Vector2.up, lo = default, hi = default;
            void Extents(Vector2 u, Vector2 v, out Vector2 l, out Vector2 h)
            {
                l = new Vector2(float.MaxValue, float.MaxValue); h = new Vector2(float.MinValue, float.MinValue);
                foreach (var p in pts) { var q = new Vector2(Vector2.Dot(p, u), Vector2.Dot(p, v)); l = Vector2.Min(l, q); h = Vector2.Max(h, q); }
            }
            for (int i = 0; i < pts.Length; i++)
            {
                var e = pts[(i + 1) % pts.Length] - pts[i]; if (e.sqrMagnitude < .01f) continue; var u = e.normalized; var v = new Vector2(-u.y, u.x);
                Extents(u, v, out var l, out var h); float area = (h.x - l.x) * (h.y - l.y);
                if (area < best) { best = area; bu = u; bv = v; lo = l; hi = h; }
            }
            if (hi.x - lo.x < hi.y - lo.y) { (bu, bv) = (bv, -bu); Extents(bu, bv, out lo, out hi); }
            const float over = .45f; lo -= Vector2.one * over; hi += Vector2.one * over;
            float rise = Mathf.Clamp((hi.y - lo.y) * .35f, 1.4f, 3.2f), mid = (lo.y + hi.y) / 2;
            Vector3 W(float a, float b, float y) { var q = bu * a + bv * b; return new Vector3(q.x, y, q.y); }
            var up = Vector3.up; var vb = new Vector3(bv.x, 0, bv.y); var ub = new Vector3(bu.x, 0, bu.y);
            m.Quad(W(lo.x, lo.y, eave - .1f), W(hi.x, lo.y, eave - .1f), W(hi.x, mid, eave + rise), W(lo.x, mid, eave + rise), col, -vb + up);
            m.Quad(W(lo.x, hi.y, eave - .1f), W(hi.x, hi.y, eave - .1f), W(hi.x, mid, eave + rise), W(lo.x, mid, eave + rise), col, vb + up);
            // gable ends in the wall colour of the roof's shade
            var gc = col * 1.6f; gc.a = 1;
            m.Tri(W(lo.x + over, lo.y + over, eave), W(lo.x + over, hi.y - over, eave), W(lo.x + over, mid, eave + rise - .2f), gc); m.Tri(W(lo.x + over, lo.y + over, eave), W(lo.x + over, mid, eave + rise - .2f), W(lo.x + over, hi.y - over, eave), gc);
            m.Tri(W(hi.x - over, lo.y + over, eave), W(hi.x - over, hi.y - over, eave), W(hi.x - over, mid, eave + rise - .2f), gc); m.Tri(W(hi.x - over, lo.y + over, eave), W(hi.x - over, mid, eave + rise - .2f), W(hi.x - over, hi.y - over, eave), gc);
            return eave + rise;
        }
        void Steeple(Mb m, Vector2[] pts, int[] fronts, float top)
        {
            int best = 0; float bl = 0;
            for (int i = 0; i < pts.Length; i++) { float l = (pts[(i + 1) % pts.Length] - pts[i]).magnitude; if ((fronts != null && i < fronts.Length && fronts[i] == 2 ? 100 : 0) + l > bl) { bl = (fronts != null && i < fronts.Length && fronts[i] == 2 ? 100 : 0) + l; best = i; } }
            var a = pts[best]; var c = pts[(best + 1) % pts.Length]; var e = (c - a).normalized; var inward = new Vector2(-e.y, e.x); var mid = (a + c) / 2 + inward * 2.2f;
            var bottom = new Vector3(mid.x, top - .5f, mid.y); var f = new Vector3(e.x, 0, e.y);
            m.Box(bottom, f, new Vector3(3.2f, 6.5f, 3.2f), C(.93f, .92f, .88f));
            var apex = bottom + Vector3.up * 13.5f; var r = Vector3.Cross(Vector3.up, f);
            Vector3[] q = { bottom + Vector3.up * 6.5f + (f + r) * 1.7f, bottom + Vector3.up * 6.5f + (f - r) * 1.7f, bottom + Vector3.up * 6.5f + (-f - r) * 1.7f, bottom + Vector3.up * 6.5f + (-f + r) * 1.7f };
            for (int i = 0; i < 4; i++) m.Tri(q[i], apex, q[(i + 1) % 4], C(.30f, .30f, .32f));
            Solid(bottom, f, new Vector3(3.2f, 6.5f, 3.2f));
        }

        // ---------------------------------------------------------------- canopies (fuel: pumps under), lights, trees
        void CanopyAt(Canopy c)
        {
            if (c.pts == null || c.pts.Length < 8) return;
            var p = new Vector3[4]; for (int i = 0; i < 4; i++) p[i] = new Vector3(c.pts[2 * i], 0, c.pts[2 * i + 1]);
            var centre = (p[0] + p[1] + p[2] + p[3]) / 4; var e1 = p[1] - p[0]; var e2 = p[2] - p[1]; bool longFirst = e1.magnitude >= e2.magnitude;
            var along = (longFirst ? e1 : e2).normalized; float L = (longFirst ? e1 : e2).magnitude, W = (longFirst ? e2 : e1).magnitude;
            float g = float.MinValue; foreach (var q in p) g = Mathf.Max(g, Ground(q.x, q.z)); g = Mathf.Max(g, Ground(centre.x, centre.z));
            float y = g + c.h; var m = Layer(centre.x, "solid"); var across = Vector3.Cross(Vector3.up, along);
            m.Box(new Vector3(centre.x, y, centre.z), along, new Vector3(W, .85f, L), C(.92f, .92f, .90f), C(.70f, .70f, .70f));
            m.Box(new Vector3(centre.x, y + .3f, centre.z), along, new Vector3(W + .06f, .22f, L + .06f), Pick(c.name ?? "x", Accent));
            Solid(new Vector3(centre.x, y, centre.z), along, new Vector3(W, .85f, L));
            bool fuel = c.kind == "canopy_fuel";
            int cols = fuel ? Mathf.Max(2, Mathf.RoundToInt(L / 7.5f)) : 2;
            for (int i = 0; i < cols; i++)
            {
                float t = cols == 1 ? 0 : -L / 2 + 2 + i * (L - 4) / (cols - 1);
                foreach (float s in fuel ? new[] { 0f } : new[] { -W / 2 + 1.2f, W / 2 - 1.2f })
                {
                    var at = centre + along * t + across * s; float gg = Ground(at.x, at.z);
                    m.Box(new Vector3(at.x, gg, at.z), along, new Vector3(.55f, y - gg, .55f), C(.90f, .90f, .88f)); Solid(new Vector3(at.x, gg, at.z), along, new Vector3(.55f, y - gg, .55f));
                    if (!fuel) continue;
                    // a pump island across the canopy at each column, a pump each side of the column
                    float il = Mathf.Min(W - 3, 6.5f);
                    m.Box(new Vector3(at.x, gg, at.z), across, new Vector3(1.1f, .2f, il), C(.70f, .69f, .66f)); Solid(new Vector3(at.x, gg, at.z), across, new Vector3(1.1f, .2f, il));
                    foreach (float k in new[] { -il / 3, il / 3 })
                    {
                        var pp = at + across * k; m.Box(new Vector3(pp.x, gg + .2f, pp.z), across, new Vector3(.75f, 1.75f, .55f), C(.86f, .86f, .84f), Pick(c.name ?? "x", Accent));
                        m.Box(new Vector3(pp.x, gg + 1.0f, pp.z) + along * .29f, across, new Vector3(.45f, .35f, .02f), C(.10f, .12f, .14f, .4f));
                        m.Box(new Vector3(pp.x, gg + 1.0f, pp.z) - along * .29f, across, new Vector3(.45f, .35f, .02f), C(.10f, .12f, .14f, .4f));
                        Solid(new Vector3(pp.x, gg + .2f, pp.z), across, new Vector3(.75f, 1.75f, .55f));
                    }
                }
            }
        }
        void Light(float x, float z, float yaw)
        {
            var f = Quaternion.Euler(0, yaw, 0) * Vector3.forward; float g = Ground(x, z); var m = Layer(x, "solid"); var b = new Vector3(x, g, z);
            var grey = C(.52f, .53f, .52f);
            m.Box(b, f, new Vector3(.24f, 9.2f, .24f), grey); m.Box(b + Vector3.up * 8.95f + f * 1.2f, f, new Vector3(.14f, .14f, 2.4f), grey);
            m.Box(b + Vector3.up * 8.75f + f * 2.55f, f, new Vector3(.38f, .22f, .85f), grey, grey); m.Box(b + Vector3.up * 8.72f + f * 2.55f, f, new Vector3(.3f, .04f, .7f), C(.95f, .92f, .80f, .05f));
            Solid(b, f, new Vector3(.24f, 9.2f, .24f));
        }
        void Tree(float x, float z, float s, bool solid, int seed)
        {
            float g = Ground(x, z); var m = Layer(x, "solid"); var b = new Vector3(x, g, z);
            var leaf = Pick(seed.ToString(), C(.24f, .37f, .16f), C(.20f, .33f, .14f), C(.30f, .40f, .17f), C(.18f, .30f, .16f));
            m.Box(b, Vector3.forward, new Vector3(.32f, 3.0f, .32f) * s, C(.30f, .23f, .16f));
            m.Blob(b + Vector3.up * 4.1f * s, new Vector3(2.0f, 2.2f, 2.0f) * s, leaf, seed); m.Blob(b + Vector3.up * 5.3f * s + new Vector3(.4f, 0, -.3f) * s, new Vector3(1.4f, 1.5f, 1.4f) * s, leaf * 1.08f, seed + 1);
            if (solid) Solid(b, Vector3.forward, new Vector3(.32f, 3f, .32f) * s);
        }

        // ---------------------------------------------------------------- pylon signs (one face lettered), traffic lights
        void PylonAt(Pylon p)
        {
            var f = Quaternion.Euler(0, p.yaw, 0) * Vector3.forward; var r = Vector3.Cross(Vector3.up, f); float g = Ground(p.x, p.z); var b = new Vector3(p.x, g, p.z);
            var m = Layer(p.x, "solid"); var body = C(.30f, .30f, .32f); var accent = Pick(p.label ?? "x", Accent);
            if (p.style == "low")
            {
                m.Box(b - Vector3.up * .3f, f, new Vector3(3.0f, .9f, .6f), C(.58f, .52f, .46f)); m.Box(b + Vector3.up * .6f, f, new Vector3(2.7f, 1.3f, .4f), accent, body);
                Solid(b - Vector3.up * .3f, f, new Vector3(3.0f, 2.2f, .6f));
                Label(p.label, b + Vector3.up * 1.25f + f * .21f, f, 2.4f, .55f, Color.white);
                return;
            }
            bool gas = p.style == "gas"; float w = gas ? 2.4f : 3.4f, h0 = gas ? 2.6f : 4.2f, h1 = gas ? 6.4f : 6.8f;
            foreach (float s in gas ? new[] { 0f } : new[] { -1.1f, 1.1f }) { m.Box(b + r * s - Vector3.up * .3f, f, new Vector3(.35f, h0 + .4f, .35f), body); Solid(b + r * s - Vector3.up * .3f, f, new Vector3(.35f, h0 + .4f, .35f)); }
            m.Box(b + Vector3.up * h0, f, new Vector3(w, h1 - h0, .55f), body);
            m.Box(b + Vector3.up * (h1 - 1.35f), f, new Vector3(w - .2f, 1.2f, .6f), accent);
            Label(p.label, b + Vector3.up * (h1 - .75f) + f * .31f, f, w - .5f, .75f, Color.white);
            if (gas)
            {
                m.Box(b + Vector3.up * (h0 + .25f), f, new Vector3(w - .3f, 1.9f, .6f), C(.08f, .09f, .10f));
                Label("REGULAR 3.19", b + Vector3.up * (h0 + 1.55f) + f * .31f, f, w - .6f, .5f, C(1f, .78f, .25f));
                Label("DIESEL 3.69", b + Vector3.up * (h0 + .8f) + f * .31f, f, w - .6f, .5f, C(1f, .78f, .25f));
            }
            else
            {
                m.Box(b + Vector3.up * (h0 + .2f), f, new Vector3(w - .2f, h1 - h0 - 1.75f, .6f), C(.90f, .89f, .86f));
                if (p.kind == "strip" || p.kind == "retail") Label("SHOPS", b + Vector3.up * (h0 + (h1 - h0 - 1.35f) * .5f) + f * .31f, f, w - .8f, .5f, C(.20f, .20f, .22f));
            }
            Solid(b + Vector3.up * h0, f, new Vector3(w, h1 - h0, .55f));
        }
        void SignalAt(Signal s)
        {
            var b = new Vector3(s.pole[0], Ground(s.pole[0], s.pole[1]), s.pole[1]); var arm = new Vector3(s.arm[0], 0, s.arm[1]).normalized; var face = new Vector3(s.face[0], 0, s.face[1]).normalized;
            var m = Layer(b.x, "solid"); var grey = C(.42f, .43f, .43f); float reach = s.heads.Max() + 1.2f;
            m.Box(b, arm, new Vector3(.42f, 7.6f, .42f), grey); Solid(b, arm, new Vector3(.42f, 7.6f, .42f));
            m.Box(b + Vector3.up * 6.7f + arm * (reach / 2), arm, new Vector3(.22f, .22f, reach), grey);
            foreach (var d in s.heads)
            {
                var at = b + arm * d + Vector3.up * 5.25f;
                m.Box(at, face, new Vector3(.46f, 1.3f, .34f), C(.10f, .10f, .10f)); m.Box(at + Vector3.up * 1.3f, face, new Vector3(.06f, .15f, .06f), grey);
                m.Box(at + Vector3.up * .97f + face * .18f, face, new Vector3(.24f, .24f, .04f), C(.25f, .06f, .05f));
                m.Box(at + Vector3.up * .57f + face * .18f, face, new Vector3(.24f, .24f, .04f), C(.25f, .20f, .04f));
                m.Box(at + Vector3.up * .17f + face * .18f, face, new Vector3(.24f, .24f, .04f), C(.25f, 1f, .45f, .2f)); // green: Hwy 92 has the light
            }
        }

        // ---------------------------------------------------------------- lettering: one face, inside its board (signs rule, 0.99)
        readonly List<(MeshRenderer mr, float w, float h)> labels = new();
        void Label(string text, Vector3 at, Vector3 facing, float w, float h, Color col)
        {
            if (string.IsNullOrEmpty(text) || w < .5f) return;
            var t = new GameObject("Lettering - " + text).AddComponent<TextMesh>(); t.transform.SetParent(root, false);
            t.font = font; t.text = text; t.anchor = TextAnchor.MiddleCenter; t.alignment = TextAlignment.Center; t.fontSize = 96; t.characterSize = .1f; t.color = col;
            t.transform.SetPositionAndRotation(at, Quaternion.LookRotation(-facing));
            var mr = t.GetComponent<MeshRenderer>(); mr.sharedMaterial = lettering; mr.shadowCastingMode = ShadowCastingMode.Off; mr.receiveShadows = false;
            var lod = t.gameObject.AddComponent<LODGroup>(); lod.SetLODs(new[] { new LOD(.012f, new Renderer[] { mr }) }); // not drawn when it would be a few pixels
            labels.Add((mr, w, h));
        }
        void LateUpdate() => FitLabels();
        public void FitLabels()
        {
            for (int i = labels.Count - 1; i >= 0; i--)
            {
                var (mr, w, h) = labels[i]; if (!mr) { labels.RemoveAt(i); continue; }
                var b = mr.localBounds; if (b.size.x < 1e-4f) continue;
                mr.transform.localScale = Vector3.one * Mathf.Min(h / b.size.y, w / b.size.x); mr.GetComponent<LODGroup>().RecalculateBounds(); labels.RemoveAt(i);
            }
            if (labels.Count == 0) enabled = false;
        }
    }
}
