using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Racer
{
    // 0.106 Part A: Red Clay Stunt Park, a local dirt-lot stunt park on the plot kept free in 0.104 (the real Weatherstone subdivision's footprint across
    // Hwy 92 from S Cherokee Ln), built at load in every world scene like the Hwy 92 roadside (scenery in the race scenes; scored in Free Roam and the Stunt
    // event). Inspired by the stunt arenas of old racing games, copied from none: our own layout and look.
    //   Where: one level packed-dirt floor (red clay) 160 x 156 m at y 11.1 (x -10..150, z 605..761, the plot's flattest part; the ground under it is
    //   6.7-10.9 m), on a pad whose earth banks run down to the ground all round (1 in 1.8), with a gravel drive up from Hwy 92 at x 120 (from the
    //   road's edge at its height, so no lip; the road itself is not touched). A tire wall round the floor (a gap at the drive) keeps everyone off
    //   the highway; the sign by the drive is one-sided and depth tested (signs rule).
    //   Features (one circuit, counter-clockwise, plus three lines across the middle):
    //     south, westbound   K1 small plywood kicker (1 m), K2 medium dirt table (2 m), K3 big dirt table (3 m), the west quarter-pipe
    //     west, northbound   the junk-car gap (five wrecks side by side) onto a dirt landing
    //     north, eastbound   a kicker up onto the two-level deck (5 m), the drop off its far end onto a landing ramp, the loop
    //     east, southbound   the half-pipe (two plywood quarter-pipes, 85 m long)
    //     middle, eastbound  the huge dirt table (5 m), the barrel-roll ramp (a twisting kicker), the school-bus gap
    //   Every jump lands on a down-slope sized to its launch (ballistic sizing: Tools/Report106/py/size.py); every ramp is one mesh drawn from one
    //   continuous profile (no seams, no joints that catch; 0.101-0.103 lessons). Shipping containers, junk cars, light poles (0.107 Part F: no hay bales).
    //   Clear() removes the trees standing where the park, its banks and the drive are; nothing else in the plot is changed.
    public sealed class StuntPark : MonoBehaviour
    {
        public const string Title = "Red Clay Stunt Park";
        public const float X0 = -10, Z0 = 605, W = 160, D = 156, FloorY = 11.1f, Bank = 1.8f, DriveX = 120, DriveHalf = 4.5f;
        public static StuntPark Current { get; private set; }
        public static bool Disabled; // evidence only (the world before this round); never set in play
        // park space: u east, v north (metres from the floor's south-west corner), y above the floor
        public static Vector3 P(float u, float v, float y = 0) => new(X0 + u, FloorY + y, Z0 + v);
        public static bool Inside(Vector3 p, float margin = 0) => p.x >= X0 - margin && p.x <= X0 + W + margin && p.z >= Z0 - margin && p.z <= Z0 + D + margin;
        public static bool OnPark(Vector3 p) => Inside(p) && p.y > FloorY - 3;
        public static readonly Vector3[] Outline = { P(0, 0), P(W, 0), P(W, D), P(0, D) };
        public static Vector3 Centre => P(W / 2, D / 2);
        // where the Stunt event starts (the start line, facing the kicker line), and the entrance (the Free Roam prompt)
        public static Vector3 StartPos => P(152, 20, .7f);
        public static Quaternion StartRot => Quaternion.LookRotation(Vector3.left);
        public static Vector3 Entrance => P(136, 14);
        public const float EntranceRadius = 26;
        // the drive's mouth on Hwy 92 (the roadside's sidewalk and planting stop there)
        public static bool InDriveMouth(float x, float z) => Mathf.Abs(x - DriveX) < DriveHalf + 1.5f && z > 540 && z < Z0 + 1;
        // the loop and the barrel-roll ramp, for the stunt scoring
        static readonly HashSet<Collider> loops = new();
        public static bool Loop(Collider c) => loops.Count > 0 && c && loops.Contains(c);
        public static Vector3 LoopOrigin, LoopForward, LoopSide; public const float LoopRadius = 5f, LoopShift = 6.5f, LoopWidth = 5.6f;
        public static Vector3 BarrelLip; public static Vector3 BarrelForward;
        // the loop's line at a point (its ribbon moves sideways as it goes round, so the way out passes beside the way in): the direction along
        // the ribbon there and how far the point is from its centre line, sideways (ArcadeVehicle steers a vehicle on it along that line)
        public static Vector3 LoopTangent(Vector3 p, out float sideways)
        {
            var rel = p - LoopOrigin; float r = LoopRadius, fwd = Vector3.Dot(rel, LoopForward), up = rel.y, lat = Vector3.Dot(rel, LoopSide);
            float phi = Mathf.Atan2(fwd, r - up); if (phi < 0) phi += 2 * Mathf.PI;
            if (phi < .6f && lat > LoopShift * .5f) phi = 2 * Mathf.PI; else if (phi > 2 * Mathf.PI - .6f && lat < LoopShift * .5f) phi = 0;
            float t = phi / (2 * Mathf.PI), sm = t * t * (3 - 2 * t), dsm = 6 * t * (1 - t) / (2 * Mathf.PI);
            sideways = lat - LoopShift * sm;
            return (LoopForward * (r * Mathf.Cos(phi)) + Vector3.up * (r * Mathf.Sin(phi)) + LoopSide * (LoopShift * dsm)).normalized;
        }
        // three acorns on the hardest spots: the top deck, the school bus's roof, the top of the loop
        public static readonly (string id, string title, Vector3 at)[] Acorns =
        {
            ("stunt-deck", "Stunt park: the top deck", P(72, 138, 5.9f)),
            ("stunt-bus", "Stunt park: the school bus's roof", P(63, 100, 4.2f)),
            ("stunt-loop", "Stunt park: the top of the loop", new Vector3(0, 0, 0)), // set from the loop below (LoopTop)
        };
        public const string AcornArea = "Stunt park";
        public static Vector3 LoopTop => LoopOrigin + Vector3.up * (2 * LoopRadius - 1.1f) + LoopSide * (LoopShift * .5f);
        public static Vector3 AcornAt(int i) => i == 2 ? LoopTop : Acorns[i].at;

        static StuntPark() { Layout(); }
        // the fixed parts of the layout the scoring and the acorns need before the park is built
        static void Layout()
        {
            LoopOrigin = P(118, 138); LoopForward = Vector3.right; LoopSide = Vector3.back;
            BarrelLip = P(56, 72, 4.7f); BarrelForward = Vector3.right;
        }

        // ---------------------------------------------------------------- the ground
        static readonly Dictionary<long, float> cache = new();
        static float GroundY(float x, float z)
        {
            long key = ((long)Mathf.RoundToInt(x * 20) << 32) ^ (uint)Mathf.RoundToInt(z * 20);
            if (cache.TryGetValue(key, out var y)) return y;
            float best = float.NaN;
            foreach (var h in Physics.RaycastAll(new Vector3(x, 600, z), Vector3.down, 1000, ~0, QueryTriggerInteraction.Ignore))
            { var c = h.collider; if (!c || !c.name.StartsWith("Ground_")) continue; if (float.IsNaN(best) || h.point.y > best) best = h.point.y; }
            if (float.IsNaN(best)) best = 8; cache[key] = best; return best;
        }
        static bool IsWorld(Scene scene) => scene.GetRootGameObjects().Any(g => g.name == "Remembered houses and approximate buildings");

        // ---------------------------------------------------------------- 1. before the scenery is fitted: the trees where the park goes
        public static readonly List<string> Cleared = new();
        // the area the park, its banks and the drive cover
        static bool Covered(float x, float z)
        {
            if (x > X0 - 14 && x < X0 + W + 14 && z > Z0 - 14 && z < Z0 + D + 14) return true;
            return Mathf.Abs(x - DriveX) < DriveHalf + 9 && z > 548 && z <= Z0;
        }
        public static void Clear(Scene scene)
        {
            Cleared.Clear(); cache.Clear();
            if (Disabled || !IsWorld(scene)) return;
            int trunks = 0, pieces = 0;
            foreach (var box in scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<BoxCollider>(true)))
            {
                if (!box || !box.enabled || box.name.IndexOf("trunk", StringComparison.OrdinalIgnoreCase) < 0) continue;
                var t = box.transform; var bottom = t.TransformPoint(box.center - Vector3.up * box.size.y * .5f);
                if (!Covered(bottom.x, bottom.z)) continue;
                var size = Vector3.Scale(box.size, t.lossyScale);
                SceneryTrees.Cleared.Add((bottom, Mathf.Min(Mathf.Abs(size.x), Mathf.Abs(size.z)), Mathf.Abs(size.y), "Red Clay Stunt Park (0.106)", t.name));
                box.enabled = false; if (t.GetComponents<Component>().Length == 2 && t.childCount == 0) t.gameObject.SetActive(false); trunks++;
                Cleared.Add($"tree trunk '{t.name}' at {bottom.x:F1},{bottom.y:F2},{bottom.z:F1}");
            }
            foreach (var r in scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<MeshRenderer>(true)))
            {
                if (!SceneryTrees.IsOldVegetation(r)) continue;
                var b = r.bounds; if (b.max.x < X0 - 14 || b.min.x > X0 + W + 14 || b.max.z < 548 || b.min.z > Z0 + D + 14) continue;
                pieces += Hwy92Roadside.TrimPieces(r.GetComponent<MeshFilter>(), Covered);
            }
            Cleared.Add($"{trunks} tree trunks and {pieces} old vegetation pieces");
            Debug.Log($"Stunt park: removed {trunks} tree trunks and {pieces} old vegetation pieces where the park, its banks and the drive are");
        }

        // ---------------------------------------------------------------- 2. the park
        public static StuntPark Attach(GameObject host) => host.GetComponent<StuntPark>() ?? host.AddComponent<StuntPark>();
        void Start() { if (!Disabled && IsWorld(gameObject.scene)) Build(gameObject.scene); }
        void OnDestroy() { if (Current == this) { Current = null; loops.Clear(); } }

        sealed class Mb
        {
            public readonly List<Vector3> v = new(); public readonly List<Vector3> n = new(); public readonly List<Color> c = new(); public readonly List<int> t = new();
            public void Tri(Vector3 a, Vector3 b, Vector3 d, Color col)
            {
                var nn = Vector3.Cross(b - a, d - a); if (nn.sqrMagnitude < 1e-12f) return; nn.Normalize();
                int i = v.Count; v.Add(a); v.Add(b); v.Add(d); n.Add(nn); n.Add(nn); n.Add(nn); c.Add(col); c.Add(col); c.Add(col); t.Add(i); t.Add(i + 1); t.Add(i + 2);
            }
            // a quad (a b c d round its edge) facing `outward`
            public void Quad(Vector3 a, Vector3 b, Vector3 d, Vector3 e, Color col, Vector3 outward)
            {
                if (Vector3.Dot(Vector3.Cross(b - a, d - a), outward) < 0) (b, e) = (e, b);
                Tri(a, b, d, col); Tri(a, d, e, col);
            }
            public void Box(Vector3 bottom, Vector3 forward, Vector3 size, Color col, Color? top = null)
            {
                forward.y = 0; forward.Normalize(); var right = Vector3.Cross(Vector3.up, forward);
                Vector3 Q(float x, float y, float z) => bottom + right * (x * size.x * .5f) + Vector3.up * (y * size.y) + forward * (z * size.z * .5f);
                Quad(Q(-1, 0, 1), Q(1, 0, 1), Q(1, 1, 1), Q(-1, 1, 1), col, forward);
                Quad(Q(1, 0, -1), Q(-1, 0, -1), Q(-1, 1, -1), Q(1, 1, -1), col, -forward);
                Quad(Q(1, 0, 1), Q(1, 0, -1), Q(1, 1, -1), Q(1, 1, 1), col, right);
                Quad(Q(-1, 0, -1), Q(-1, 0, 1), Q(-1, 1, 1), Q(-1, 1, -1), col, -right);
                Quad(Q(-1, 1, 1), Q(1, 1, 1), Q(1, 1, -1), Q(-1, 1, -1), top ?? col, Vector3.up);
                Quad(Q(-1, 0, -1), Q(1, 0, -1), Q(1, 0, 1), Q(-1, 0, 1), col, Vector3.down);
            }
            public void TriOut(Vector3 a, Vector3 b, Vector3 d, Color col, Vector3 outward) { if (Vector3.Dot(Vector3.Cross(b - a, d - a), outward) < 0) (b, d) = (d, b); Tri(a, b, d, col); }
            // a cylinder between two points (the sides, the end cap at b, and the one at a unless open)
            public void Cyl(Vector3 a, Vector3 b, float r, Color col, int sides = 8, Color? cap = null, bool openA = false)
            {
                var ax = (b - a).normalized; var p = Vector3.Cross(ax, Mathf.Abs(ax.y) < .9f ? Vector3.up : Vector3.right).normalized; var q = Vector3.Cross(ax, p);
                for (int i = 0; i < sides; i++)
                {
                    float a0 = i * Mathf.PI * 2 / sides, a1 = (i + 1) * Mathf.PI * 2 / sides;
                    var o0 = (p * Mathf.Cos(a0) + q * Mathf.Sin(a0)) * r; var o1 = (p * Mathf.Cos(a1) + q * Mathf.Sin(a1)) * r;
                    Quad(a + o0, a + o1, b + o1, b + o0, col, (o0 + o1).normalized);
                    TriOut(b, b + o0, b + o1, cap ?? col, ax); if (!openA) TriOut(a, a + o1, a + o0, cap ?? col, -ax);
                }
            }
            public bool Empty => t.Count == 0;
            public Mesh Mesh(string name, bool keepReadable = false)
            {
                var m = new Mesh { name = name, indexFormat = v.Count > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16 };
                m.SetVertices(v); m.SetNormals(n); m.SetColors(c); m.SetTriangles(t, 0); m.RecalculateBounds(); m.UploadMeshData(!keepReadable); return m;
            }
        }

        // ---------------------------------------------------------------- profiles: a turtle in (along, up), so every ramp is one smooth line
        sealed class Prof
        {
            public readonly List<Vector2> pts = new(); public float s, y, a; // a: heading in degrees (0 flat, + up)
            public Prof(float s0 = 0, float y0 = 0) { s = s0; y = y0; pts.Add(new Vector2(s, y)); }
            Prof Add() { pts.Add(new Vector2(s, y)); return this; }
            // turn by da degrees on a radius r (+ curving up, - curving over)
            public Prof Arc(float r, float da)
            {
                int n = Mathf.Max(2, Mathf.CeilToInt(Mathf.Abs(da) / 3)); float a0 = a * Mathf.Deg2Rad;
                Vector2 c = da > 0 ? new Vector2(s - r * Mathf.Sin(a0), y + r * Mathf.Cos(a0)) : new Vector2(s + r * Mathf.Sin(a0), y - r * Mathf.Cos(a0));
                for (int i = 1; i <= n; i++)
                {
                    float a1 = (a + da * i / n) * Mathf.Deg2Rad;
                    var p = da > 0 ? c + new Vector2(r * Mathf.Sin(a1), -r * Mathf.Cos(a1)) : c + new Vector2(-r * Mathf.Sin(a1), r * Mathf.Cos(a1));
                    s = p.x; y = p.y; Add();
                }
                a += da; return this;
            }
            public Prof Line(float len) { s += len * Mathf.Cos(a * Mathf.Deg2Rad); y += len * Mathf.Sin(a * Mathf.Deg2Rad); return Add(); }
            public Prof Flat(float len) { a = 0; s += len; return Add(); }
            public Prof Up(float to) { y = to; return Add(); }     // a vertical face up (the front of a landing or a deck)
            public Prof Drop() { y = 0; a = 0; return Add(); }    // a vertical face down to the floor (the back of a kicker)
            // over a knuckle (radius rk) onto a slope of phi degrees, down to the floor with a transition of radius rb: lands on the floor flat
            public Prof Landing(float phi, float rk, float rb)
            {
                a = 0; Arc(rk, -phi); Line(Mathf.Max(0, (y - rb * (1 - Mathf.Cos(phi * Mathf.Deg2Rad))) / Mathf.Sin(phi * Mathf.Deg2Rad))); Arc(rb, phi); y = 0; pts[^1] = new Vector2(s, 0);
                return this;
            }
        }

        // ---------------------------------------------------------------- materials and colours
        static Material building, lettering; static Font font;
        static Color C(float r, float g, float b, float a = 1) => new(r, g, b, a);
        static readonly Color Clay = C(.58f, .35f, .22f), ClayDark = C(.49f, .29f, .18f), Dirt = C(.50f, .33f, .22f), Ply = C(.74f, .60f, .42f), PlyDark = C(.55f, .43f, .29f),
            Gravel = C(.50f, .48f, .45f), BankGrass = C(.36f, .42f, .22f), Tire = C(.07f, .07f, .075f), Steel = C(.70f, .70f, .68f), Red = C(.70f, .14f, .12f), White = C(.90f, .89f, .86f),
            Yellow = C(.93f, .70f, .10f), Black = C(.08f, .08f, .08f), Lamp = C(1f, .95f, .80f, .1f);
        static float Noise(float x, float z) { unchecked { int h = (int)(x * 73856093) ^ (int)(z * 19349663); h = (h ^ (h >> 13)) * 1274126177; return ((h & 0xffff) / 65535f) - .5f; } }
        static Color Vary(Color c, float x, float z, float amount = .07f) { float k = 1 + Noise(x, z) * amount * 2; return new Color(c.r * k, c.g * k, c.b * k, c.a); }

        Transform root; readonly Dictionary<string, Mb> layers = new();
        Mb Layer(string name) { if (!layers.TryGetValue(name, out var m)) layers[name] = m = new Mb(); return m; }
        public readonly List<string> Report = new();
        public int Solids { get; private set; }
        public int Triangles { get; private set; }
        void Solid(Mb shape, string name, bool loop = false)
        {
            var g = new GameObject(name); g.transform.SetParent(root, false);
            var mc = g.AddComponent<MeshCollider>(); mc.sharedMesh = shape.Mesh(name + " collider", true); Solids++;
            if (loop) loops.Add(mc);
        }
        void SolidBox(Vector3 bottom, Vector3 forward, Vector3 size, string name)
        {
            var g = new GameObject(name); g.transform.SetParent(root, false); forward.y = 0;
            g.transform.SetPositionAndRotation(bottom + Vector3.up * size.y * .5f, Quaternion.LookRotation(forward.sqrMagnitude > 1e-6f ? forward.normalized : Vector3.forward));
            g.AddComponent<BoxCollider>().size = size; Solids++;
        }

        public void Build(Scene scene)
        {
            building ??= Resources.Load<Material>("Scenery/Building"); lettering ??= Resources.Load<Material>("Scenery/SignLettering");
            font ??= Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (!building || !lettering || !font) { Debug.LogWarning("Stunt park: material or font missing"); return; }
            if (font.material && font.material.mainTexture) lettering.mainTexture = font.material.mainTexture;
            var watch = System.Diagnostics.Stopwatch.StartNew(); Physics.SyncTransforms(); Current = this; loops.Clear();
            var go = new GameObject("Red Clay Stunt Park (0.106)"); SceneManager.MoveGameObjectToScene(go, scene); root = go.transform;
            Floor(); Banks(); TireWall();
            KickerLine(); CarGap(); DeckAndLoop(); HalfPipe(); Middle(); Props(); Sign();
            foreach (var kv in layers)
            {
                if (kv.Value.Empty) continue; Triangles += kv.Value.t.Count / 3;
                var g = new GameObject("Stunt park " + kv.Key, typeof(MeshFilter), typeof(MeshRenderer)); g.transform.SetParent(root, false);
                g.GetComponent<MeshFilter>().sharedMesh = kv.Value.Mesh(g.name); var r = g.GetComponent<MeshRenderer>(); r.sharedMaterial = building;
                r.shadowCastingMode = kv.Key == "flat" ? ShadowCastingMode.Off : ShadowCastingMode.On;
            }
            layers.Clear();
            Report.Add($"built in {watch.ElapsedMilliseconds} ms: {Triangles} triangles, {Solids} solids, {labels.Count} lettered faces");
            Debug.Log("Stunt park: " + Report[^1]);
        }

        // ---------------------------------------------------------------- floor, banks, drive, tire wall
        void Floor()
        {
            var m = Layer("flat"); const float cell = 8;
            for (float u = 0; u < W - .01f; u += cell)
                for (float v = 0; v < D - .01f; v += cell)
                {
                    float u1 = Mathf.Min(W, u + cell), v1 = Mathf.Min(D, v + cell);
                    Vector3 a = P(u, v), b = P(u1, v), c = P(u1, v1), d = P(u, v1);
                    m.Quad(a, b, c, d, Vary(Clay, u, v, .025f), Vector3.up);
                }
            // tyre tracks and darker packed lanes: a few long strips
            foreach (var (u0, v0, u1, v1) in new[] { (8f, 17f, 150f, 23f), (19f, 30f, 25f, 125f), (30f, 135f, 130f, 141f), (140f, 30f, 152f, 128f) })
                m.Quad(P(u0, v0, .02f), P(u1, v0, .02f), P(u1, v1, .02f), P(u0, v1, .02f), ClayDark, Vector3.up);
            // the start line
            m.Quad(P(147.5f, 13, .03f), P(148.5f, 13, .03f), P(148.5f, 27, .03f), P(147.5f, 27, .03f), White, Vector3.up);
            var g = new GameObject("Stunt park floor"); g.transform.SetParent(root, false); g.transform.position = new Vector3(X0 + W / 2, FloorY - .5f, Z0 + D / 2);
            g.AddComponent<BoxCollider>().size = new Vector3(W, 1, D); Solids++;
        }
        // the road's edge and height where the drive meets Hwy 92 (from the through route: the drive starts exactly at its edge, at its height)
        float driveStartZ = 553.8f, driveStartY = 9.1f;
        void FindRoadEdge()
        {
            var race = FindAnyObjectByType<RaceDirector>(); var road = race ? race.throughRoad : null;
            if (road && road.points != null && road.points.Length > 1)
            {
                var probe = new Vector3(DriveX, 9, 560); float s = road.Project(probe, out _); var c = road.At(s, out var f);
                float half = road.HalfWidth(s); var side = Vector3.Cross(Vector3.up, f.normalized); if (side.z < 0) side = -side;
                var edge = c + side * half; driveStartZ = edge.z + (DriveX - edge.x) * 0f;
            }
            // the road's own surface just inside its edge
            float best = float.NaN;
            foreach (var h in Physics.RaycastAll(new Vector3(DriveX, 60, driveStartZ - .25f), Vector3.down, 120, ~0, QueryTriggerInteraction.Ignore))
                if (h.collider && h.collider.name.StartsWith("Ground_") && (float.IsNaN(best) || h.point.y > best)) best = h.point.y;
            if (!float.IsNaN(best)) driveStartY = best;
            Report.Add($"drive meets Hwy 92's edge at z {driveStartZ:F2}, height {driveStartY:F3}");
        }
        float DriveHeight(float z)
        {
            float t = Mathf.Clamp01((z - driveStartZ) / (Z0 - driveStartZ)); t = t * t * (3 - 2 * t); // flat where it leaves the road and where it meets the floor
            return Mathf.Lerp(driveStartY, FloorY, t);
        }
        // the pad's banks and the drive as one height field on a 2 m grid round the floor, falling to the ground (1 in 1.8) and tucked under it beyond
        void Banks()
        {
            FindRoadEdge();
            var vis = Layer("ground"); var col = new Mb(); const float step = 2;
            float gx0 = X0 - 14, gx1 = X0 + W + 14, gz0 = Z0 - 58, gz1 = Z0 + D + 14;
            int nx = Mathf.RoundToInt((gx1 - gx0) / step);
            // rows every 2 m, the first at the road's edge (nothing is built over the road)
            var zs = new List<float> { driveStartZ }; for (float z = gz0; z <= gz1 + .01f; z += step) if (z > driveStartZ + .5f) zs.Add(z);
            int nz = zs.Count - 1;
            var h = new float[nx + 1, nz + 1]; var above = new bool[nx + 1, nz + 1]; var drive = new bool[nx + 1, nz + 1];
            for (int i = 0; i <= nx; i++)
                for (int j = 0; j <= nz; j++)
                {
                    float x = gx0 + i * step, z = zs[j];
                    float dx = Mathf.Max(0, Mathf.Max(X0 - x, x - (X0 + W))), dz = Mathf.Max(0, Mathf.Max(Z0 - z, z - (Z0 + D)));
                    float bank = FloorY - Mathf.Sqrt(dx * dx + dz * dz) / Bank;
                    float lat = Mathf.Abs(x - DriveX), dh = float.MinValue;
                    if (z >= driveStartZ - .01f && z <= Z0) dh = DriveHeight(z) - Mathf.Max(0, lat - DriveHalf) / Bank;
                    float top = Mathf.Max(bank, dh), ground = GroundY(x, z);
                    above[i, j] = top >= ground - .02f; drive[i, j] = lat <= DriveHalf + .01f && z <= Z0;
                    h[i, j] = above[i, j] ? Mathf.Max(top, ground) : ground - .12f;
                }
            for (int i = 0; i < nx; i++)
                for (int j = 0; j < nz; j++)
                {
                    float x = gx0 + i * step, z = zs[j], z2 = zs[j + 1];
                    if (x >= X0 - .01f && x + step <= X0 + W + .01f && z >= Z0 - .01f && z2 <= Z0 + D + .01f) continue; // the floor
                    if (!(above[i, j] || above[i + 1, j] || above[i, j + 1] || above[i + 1, j + 1])) continue;
                    Vector3 a = new(x, h[i, j], z), b = new(x + step, h[i + 1, j], z), c = new(x + step, h[i + 1, j + 1], z2), d = new(x, h[i, j + 1], z2);
                    bool onDrive = drive[i, j] && drive[i + 1, j] && drive[i, j + 1] && drive[i + 1, j + 1];
                    var colour = onDrive ? Vary(Gravel, x, z, .04f) : Color.Lerp(Vary(BankGrass, x, z, .08f), Dirt, Mathf.Clamp01((a.y - GroundY(x, z)) / 4) * .6f);
                    vis.Quad(a, b, c, d, colour, Vector3.up); col.Quad(a, b, c, d, colour, Vector3.up);
                }
            Solid(col, "Stunt park ground (banks and drive)");
        }
        void TireWall()
        {
            var m = Layer("props"); float r = .34f, hgt = 1.0f;
            void Run(Vector3 a, Vector3 b, string name)
            {
                float len = Vector3.Distance(a, b); int n = Mathf.Max(1, Mathf.RoundToInt(len / (2 * r + .02f))); var f = (b - a).normalized;
                for (int i = 0; i < n; i++)
                {
                    var c = a + f * ((i + .5f) * len / n);
                    m.Cyl(c, c + Vector3.up * hgt, r, (i % 2 == 0) ? Tire : Tire * 1.25f, 8, C(.03f, .03f, .03f), true);
                }
                for (float s = 0; s < len - .01f; s += 10) { float e = Mathf.Min(len, s + 10); SolidBox(a + f * ((s + e) / 2) - Vector3.up * .02f, f, new Vector3(2 * r + .1f, hgt + .02f, e - s), name); }
            }
            float i = .45f;
            Run(P(i, i), P(125.5f, i), "Stunt park tire wall"); Run(P(134.5f, i), P(W - i, i), "Stunt park tire wall");
            Run(P(W - i, i), P(W - i, D - i), "Stunt park tire wall"); Run(P(W - i, D - i), P(i, D - i), "Stunt park tire wall"); Run(P(i, D - i), P(i, i), "Stunt park tire wall");
        }

        // ---------------------------------------------------------------- ramps: a profile drawn along a lane, closed below (one mesh, one collider)
        // origin: park point where the profile's s = 0 meets the lane's centre; dir: the lane's direction; width across it. bank (optional): the
        // barrel-roll ramp's roll (degrees, raising the left edge about the right) at each point
        void Ramp(Prof prof, Vector3 origin, Vector3 dir, float width, Color top, Color side, string name, Func<float, float> bank = null, bool loop = false)
        {
            var m = Layer(top == Ply || top == PlyDark ? "ply" : "dirt"); var col = new Mb();
            dir.y = 0; dir.Normalize(); var right = Vector3.Cross(Vector3.up, dir); var pts = prof.pts;
            Vector3 Edge(int i, float sideSign)
            {
                var p = pts[i]; float b = bank != null ? bank(p.x) * Mathf.Deg2Rad : 0;
                if (b == 0) return origin + dir * p.x + right * (sideSign * width * .5f) + Vector3.up * p.y;
                // rolled about the centre line (the left edge up, the right down), lifted so the low edge never goes below the floor
                float lift = Mathf.Max(0, Mathf.Sin(b) * width * .5f - p.y);
                return origin + dir * p.x + right * (sideSign * Mathf.Cos(b) * width * .5f) + Vector3.up * (p.y + lift - sideSign * Mathf.Sin(b) * width * .5f);
            }
            Vector3 Foot(Vector3 p) => new(p.x, origin.y, p.z);
            for (int i = 0; i + 1 < pts.Count; i++)
            {
                Vector3 l0 = Edge(i, -1), r0 = Edge(i, 1), l1 = Edge(i + 1, -1), r1 = Edge(i + 1, 1);
                var ds = pts[i + 1] - pts[i]; var outward = dir * -ds.y + Vector3.up * ds.x; if (outward.sqrMagnitude < 1e-8f) continue;
                var shade = Vary(top, pts[i].x + origin.x, origin.z + pts[i].y, .05f);
                if (bank != null) outward = Vector3.Cross(l1 - l0, r0 - l0).y > 0 ? Vector3.Cross(l1 - l0, r0 - l0) : -Vector3.Cross(l1 - l0, r0 - l0);
                m.Quad(l0, r0, r1, l1, shade, outward); col.Quad(l0, r0, r1, l1, shade, outward);
                // sides down to the floor
                if (pts[i].y > .001f || pts[i + 1].y > .001f || bank != null)
                {
                    m.Quad(Foot(r0), Foot(r1), r1, r0, side, right); col.Quad(Foot(r0), Foot(r1), r1, r0, side, right);
                    m.Quad(Foot(l0), Foot(l1), l1, l0, side, -right); col.Quad(Foot(l0), Foot(l1), l1, l0, side, -right);
                }
            }
            // end faces (a profile that ends or starts above the floor)
            void End(int i, Vector3 facing)
            {
                var l = Edge(i, -1); var r = Edge(i, 1); if (l.y - origin.y < .01f && r.y - origin.y < .01f) return;
                m.Quad(Foot(l), Foot(r), r, l, side, facing); col.Quad(Foot(l), Foot(r), r, l, side, facing);
            }
            End(0, -dir); End(pts.Count - 1, dir);
            Solid(col, "Stunt park " + name, loop);
        }
        static Prof Kicker(float h, float th) { float r = h / (1 - Mathf.Cos(th * Mathf.Deg2Rad)); return new Prof().Arc(r, th); }

        // ---------------------------------------------------------------- the south line (westbound): K1, K2, K3; the west quarter-pipe
        void KickerLine()
        {
            var west = Vector3.left;
            Ramp(Kicker(1.0f, 20).Drop(), P(128, 20), west, 7, Ply, PlyDark, "K1 small kicker");
            var k2 = Kicker(2.0f, 25); k2.Flat(14).Landing(18, 3, 10); Ramp(k2, P(104, 20), west, 9, Dirt, Dirt * .85f, "K2 medium table");
            var k3 = Kicker(3.0f, 30); k3.Flat(18).Landing(20, 3, 12); Ramp(k3, P(64, 20), west, 10, Dirt, Dirt * .85f, "K3 big table");
            // the west quarter-pipe: up toward the wall (radius 6.5 to 65 degrees), a 1.5 m deck, then the wall
            var qp = new Prof().Arc(6.5f, 65); qp.Flat(1.4f).Drop();
            Ramp(qp, P(8, 22), west, 36, Ply, PlyDark, "west quarter-pipe");
        }
        // ---------------------------------------------------------------- the west line (northbound): the junk-car gap
        void CarGap()
        {
            var north = Vector3.forward;
            Ramp(Kicker(2.0f, 22).Drop(), P(10, 46), north, 7, Ply, PlyDark, "car gap takeoff");
            float lip = 46 + 2.0f / (1 - Mathf.Cos(22 * Mathf.Deg2Rad)) * Mathf.Sin(22 * Mathf.Deg2Rad);
            for (int k = 0; k < 5; k++) JunkCar(P(10, lip + 3 + k * 2.05f), Vector3.right, k);
            var land = new Prof().Up(2.0f); land.Flat(1.2f).Landing(16, 3, 10);
            Ramp(land, P(10, lip + 13.6f), north, 10, Dirt, Dirt * .85f, "car gap landing");
        }
        // ---------------------------------------------------------------- the north line (eastbound): kicker up onto the deck, the drop, the loop
        void DeckAndLoop()
        {
            var east = Vector3.right;
            var kick = Kicker(3.2f, 34); kick.Drop(); Ramp(kick, P(38, 138), east, 8, Ply, PlyDark, "deck launch");
            float lip = 38 + 3.2f / (1 - Mathf.Cos(34 * Mathf.Deg2Rad)) * Mathf.Sin(34 * Mathf.Deg2Rad);
            // the deck: 5 m up, 36.5 m long, 16 m wide; plywood top on a timber frame
            float d0 = lip + 8, d1 = d0 + 30f, top = 5f;
            var deck = new Prof().Up(top); deck.Flat(d1 - d0).Drop(); Ramp(deck, P(d0, 138), east, 16, Ply, PlyDark, "two-level deck");
            var frame = Layer("props");
            for (float u = d0 + 1; u < d1; u += 5.5f) foreach (float v in new[] { 129.75f, 146.25f }) frame.Box(P(u, v, 0), east, new Vector3(.45f, top, .45f), PlyDark * .8f);
            for (float u = d0; u + 3 <= d1 + .01f; u += 3) foreach (float v in new[] { 129.82f, 146.18f }) frame.Box(P(u + 1.5f, v, 3.3f), east, new Vector3(3.1f, .25f, .12f), PlyDark * .9f);
            // the drop's landing: a ramp from 3.2 m (its front 6 m beyond the deck's edge)
            var land = new Prof().Up(3.6f); land.Flat(.5f).Landing(16, 3, 10); Ramp(land, P(d1 + 5, 138), east, 14, Dirt, Dirt * .85f, "deck drop landing");
            BuildLoop(LoopOrigin, east, Vector3.back);
        }
        // the loop: a ribbon round a circle (radius 5 m) that moves 6.5 m sideways on the way round, so the way out passes beside the way in;
        // its inner face is the road (the wheels hold it all the way round: ArcadeVehicle asks Loop()), rails on both edges, two towers outside
        void BuildLoop(Vector3 o, Vector3 f, Vector3 side)
        {
            LoopOrigin = o; LoopForward = f; LoopSide = side;
            float r = LoopRadius, w = LoopWidth, shift = LoopShift, thick = .3f, rail = .7f; int n = 72;
            var vis = Layer("loop"); var road = new Mb(); var rails = new Mb();
            Vector3 Centre(float phi) { float t = phi / (2 * Mathf.PI); float sm = t * t * (3 - 2 * t); return o + f * (r * Mathf.Sin(phi)) + Vector3.up * (r - r * Mathf.Cos(phi)) + side * (shift * sm); }
            Vector3 In(float phi) => (-f * Mathf.Sin(phi) + Vector3.up * Mathf.Cos(phi)).normalized; // toward the circle's centre
            // the lead-in and lead-out: 6 m of the same ribbon on the floor either side
            for (int i = 0; i < n; i++)
            {
                float a0 = i * 2 * Mathf.PI / n, a1 = (i + 1) * 2 * Mathf.PI / n;
                Vector3 c0 = Centre(a0), c1 = Centre(a1), n0 = In(a0), n1 = In(a1);
                Vector3 l0 = c0 - side * (w / 2), r0 = c0 + side * (w / 2), l1 = c1 - side * (w / 2), r1 = c1 + side * (w / 2);
                var tone = (i % 6 < 3) ? Steel : Steel * .92f;
                vis.Quad(l0, r0, r1, l1, tone, (n0 + n1) * .5f); road.Quad(l0, r0, r1, l1, tone, (n0 + n1) * .5f);
                // the underside (outer face) and the ribbon's edges
                Vector3 lo0 = l0 - n0 * thick, ro0 = r0 - n0 * thick, lo1 = l1 - n1 * thick, ro1 = r1 - n1 * thick;
                vis.Quad(lo0, ro0, ro1, lo1, PlyDark, -(n0 + n1)); road.Quad(lo0, ro0, ro1, lo1, PlyDark, -(n0 + n1));
                vis.Quad(l0, l1, lo1, lo0, Red, -side); vis.Quad(r0, r1, ro1, ro0, Red, side);
                // rails on the way round (not at the bottom, where the way in and out lie on the floor)
                if (a0 > .5f && a1 < 2 * Mathf.PI - .5f)
                    foreach (var (e0, e1, s) in new[] { (l0, l1, -side), (r0, r1, side) })
                    {
                        var t0 = e0 + n0 * rail; var t1 = e1 + n1 * rail; var stripe = i % 2 == 0 ? Red : White;
                        vis.Quad(e0, e1, t1, t0, stripe, -s); vis.Quad(e0, e1, t1, t0, stripe * .8f, s);
                        rails.Quad(e0, e1, t1, t0, stripe, -s); rails.Quad(e0, e1, t1, t0, stripe, s);
                    }
            }
            Solid(road, "Stunt park loop", true); Solid(rails, "Stunt park loop rails");
            // two towers outside both lanes, a beam over the top
            var props = Layer("props"); float lo = -w / 2 - 1.6f, hi = shift + w / 2 + 1.6f, topY = 2 * r + .5f;
            foreach (float s in new[] { lo, hi })
            {
                var at = o + side * s; props.Box(at, f, new Vector3(.5f, topY + .4f, .5f), Steel * .85f); SolidBox(at, f, new Vector3(.5f, topY + .4f, .5f), "Stunt park loop tower");
                props.Box(at - f * 1.2f, f, new Vector3(.3f, topY * .7f, .3f), Steel * .8f); props.Box(at + f * 1.2f, f, new Vector3(.3f, topY * .7f, .3f), Steel * .8f);
            }
            var mid = o + side * ((lo + hi) / 2) + Vector3.up * topY; props.Box(mid, side, new Vector3(.4f, .4f, hi - lo + .5f), Red);
        }
        // ---------------------------------------------------------------- the east line: the half-pipe (southbound)
        void HalfPipe()
        {
            // two quarter-pipes facing each other across an 8 m flat (the floor), 85 m long: radius 6 m to 65 degrees (3.5 m), a 1.5 m deck
            foreach (var (u, dir) in new[] { (142.5f, Vector3.left), (150.5f, Vector3.right) })
            {
                var qp = new Prof().Arc(6, 65); qp.Flat(1.5f).Drop();
                Ramp(qp, P(u, 82.5f), dir, 85, Ply, PlyDark, "half-pipe");
            }
        }
        // ---------------------------------------------------------------- the middle lines (eastbound): huge table, barrel roll, bus gap
        void Middle()
        {
            var east = Vector3.right;
            var huge = Kicker(5.0f, 34); huge.Flat(22).Landing(22, 4, 14); Ramp(huge, P(36, 46), east, 11, Dirt, Dirt * .85f, "K4 huge table");
            // the barrel-roll ramp: a 3 m kicker (28 degrees) whose surface rolls up to 50 degrees by the lip (the left edge rises), so it throws you into a roll
            var barrel = Kicker(3.0f, 28); barrel.Drop(); float len = barrel.pts[^2].x;
            Ramp(barrel, P(44, 74), east, 6, Ply, PlyDark, "barrel-roll ramp", s => 50 * Mathf.SmoothStep(0, 1, Mathf.Clamp01((s - 1) / (len - 1))));
            BarrelLip = P(44 + len, 74, 3.0f); BarrelForward = east;
            // its landing: wide, and a little to the right (the ramp throws you that way)
            // its landing: a table 1.2 m past the lip (a short flight lands on the table), then a long slope; wide, and a little to the right (the ramp throws you that way)
            // its front is a rounded step-up (no wall for a vehicle that leaves the ramp's low side): up 22 degrees to 1.6 m (well under a rolling
            // vehicle's flight), a short table, then the slope
            var bl = new Prof().Arc(3, 22); bl.Line((1.6f - 2 * 3 * (1 - Mathf.Cos(22 * Mathf.Deg2Rad))) / Mathf.Sin(22 * Mathf.Deg2Rad)).Arc(3, -22); bl.Flat(2).Landing(12, 3, 10);
            Ramp(bl, P(44 + len + 1.2f, 72.5f), east, 15, Dirt, Dirt * .85f, "barrel-roll landing");
            // the school-bus gap: over the bus lengthwise
            var bk = Kicker(3.4f, 26); bk.Drop(); Ramp(bk, P(40, 100), east, 7, Ply, PlyDark, "bus gap takeoff");
            float blip = 40 + 3.4f / (1 - Mathf.Cos(26 * Mathf.Deg2Rad)) * Mathf.Sin(26 * Mathf.Deg2Rad);
            Bus(P(blip + 2.3f, 100), east);
            var bland = new Prof().Up(3.4f); bland.Flat(1).Landing(18, 3, 12); Ramp(bland, P(blip + 16.5f, 100), east, 11, Dirt, Dirt * .85f, "bus gap landing");
        }

        // ---------------------------------------------------------------- props
        void JunkCar(Vector3 centre, Vector3 along, int k)
        {
            var m = Layer("props"); var rust = new[] { C(.45f, .24f, .14f), C(.30f, .34f, .36f), C(.52f, .44f, .30f), C(.36f, .20f, .18f), C(.24f, .30f, .24f) }[k % 5];
            var right = Vector3.Cross(Vector3.up, along);
            m.Box(centre + Vector3.up * .3f, along, new Vector3(1.75f, .62f, 4.3f), rust);
            m.Box(centre + Vector3.up * .92f - along * .2f, along, new Vector3(1.55f, .5f, 2.1f), rust * .9f);
            m.Box(centre + Vector3.up * .97f - along * .2f, along, new Vector3(1.57f, .36f, 1.9f), C(.12f, .14f, .15f));
            foreach (float a in new[] { -1.35f, 1.35f }) foreach (float s in new[] { -.82f, .82f }) m.Cyl(centre + along * a + right * s + Vector3.up * .32f - right * .1f * Mathf.Sign(s), centre + along * a + right * s + Vector3.up * .32f + right * .12f * Mathf.Sign(s), .32f, Tire, 8);
            SolidBox(centre, along, new Vector3(1.75f, 1.42f, 4.3f), "Stunt park junk car");
        }
        void Bus(Vector3 back, Vector3 along)
        {
            var m = Layer("props"); var right = Vector3.Cross(Vector3.up, along); float len = 12, wid = 2.5f, h = 3.2f;
            var c = back + along * (len / 2);
            m.Box(c + Vector3.up * .55f, along, new Vector3(wid, h - .55f, len), Yellow, Yellow * .95f);
            foreach (float s in new[] { -1f, 1f })
            {
                m.Box(c + right * (s * (wid / 2 + .005f)) + Vector3.up * 1.95f, along, new Vector3(.02f, .85f, len - 2.2f), C(.12f, .14f, .16f));
                foreach (float y in new[] { 1.45f, 1.0f }) m.Box(c + right * (s * (wid / 2 + .01f)) + Vector3.up * y, along, new Vector3(.02f, .08f, len - .4f), Black);
            }
            m.Box(c + along * (len / 2 + .005f) + Vector3.up * 2.0f, along, new Vector3(wid - .4f, .9f, .02f), C(.12f, .14f, .16f));
            m.Box(c + along * (len / 2 + .1f) + Vector3.up * .45f, along, new Vector3(wid, .3f, .2f), Black);
            m.Box(c - along * (len / 2 + .1f) + Vector3.up * .45f, along, new Vector3(wid, .3f, .2f), Black);
            foreach (float a in new[] { -len / 2 + 2.2f, len / 2 - 2.6f }) foreach (float s in new[] { -1f, 1f })
                    m.Cyl(c + along * a + right * (s * (wid / 2 - .25f)) + Vector3.up * .5f, c + along * a + right * (s * (wid / 2 + .02f)) + Vector3.up * .5f, .5f, Tire, 10);
            SolidBox(c, along, new Vector3(wid, h, len), "Stunt park school bus");
        }
        void Container(Vector3 bottom, Vector3 along, Color col)
        {
            var m = Layer("props"); var right = Vector3.Cross(Vector3.up, along);
            m.Box(bottom, along, new Vector3(2.44f, 2.59f, 6.06f), col, col * .9f);
            for (float a = -2.8f; a <= 2.81f; a += .4f) foreach (float s in new[] { -1f, 1f }) m.Box(bottom + along * a + right * (s * 1.23f) + Vector3.up * .1f, along, new Vector3(.03f, 2.4f, .12f), col * .82f);
            SolidBox(bottom, along, new Vector3(2.44f, 2.59f, 6.06f), "Stunt park container");
        }
        void LightPole(Vector3 at, Vector3 facing)
        {
            var m = Layer("props"); m.Box(at, facing, new Vector3(.3f, 11, .3f), Steel * .8f);
            m.Box(at + Vector3.up * 11 + facing * .5f, facing, new Vector3(2.2f, .5f, .9f), Steel * .7f);
            m.Box(at + Vector3.up * 10.95f + facing * .5f, facing, new Vector3(2.0f, .06f, .7f), Lamp);
            SolidBox(at, facing, new Vector3(.3f, 11, .3f), "Stunt park light pole");
        }
        void Props()
        {
            // containers along the north edge (some two high) and by the entrance; junk piled in the north-west corner
            var cols = new[] { C(.55f, .20f, .14f), C(.18f, .32f, .52f), C(.22f, .42f, .26f), C(.50f, .50f, .48f), C(.62f, .44f, .16f) };
            int k = 0;
            for (float u = 8; u < 150; u += 7.4f, k++)
            {
                if (u > 30 && u < 36) continue;
                Container(P(u, 152.4f), Vector3.right, cols[k % cols.Length]);
                if (k % 3 == 1) Container(P(u, 152.4f, 2.6f), Vector3.right, cols[(k + 2) % cols.Length]);
            }
            Container(P(144, 3.2f), Vector3.right, cols[1]); Container(P(152, 3.2f), Vector3.right, cols[3]); Container(P(148, 3.2f, 2.6f), Vector3.right, cols[0]);
            JunkCar(P(6, 146), Vector3.forward, 1); JunkCar(P(10, 146), Vector3.forward, 3);
            // lamps (their heads glow at night) and the start flag
            foreach (var (u, v) in new[] { (3f, 3f), (157f, 3f), (157f, 153f), (3f, 153f), (80f, 3f), (80f, 153f), (3f, 80f), (126f, 80f) }) LightPole(P(u, v), (Centre - P(u, v)).normalized);
            var props = Layer("props"); props.Box(P(148, 10.5f), Vector3.forward, new Vector3(.12f, 4, .12f), Steel);
            for (int i = 0; i < 4; i++) for (int j = 0; j < 3; j++) props.Box(P(148.1f, 10.5f + .05f + i * .4f - .2f, 3.2f + j * .3f - .3f), Vector3.right, new Vector3(.02f, .3f, .4f), (i + j) % 2 == 0 ? Black : White);
            SolidBox(P(148, 10.5f), Vector3.forward, new Vector3(.15f, 4, .15f), "Stunt park start flag");
        }
        // ---------------------------------------------------------------- the sign by the drive (one-sided, its lettering depth tested)
        void Sign()
        {
            var m = Layer("props"); var at = new Vector3(DriveX + DriveHalf + 4.5f, 0, driveStartZ + 6); at.y = GroundY(at.x, at.z);
            var face = Vector3.back; var right = Vector3.Cross(Vector3.up, face);
            foreach (float s in new[] { -2.4f, 2.4f }) { m.Box(at + right * s - Vector3.up * .4f, face, new Vector3(.25f, 5.0f, .25f), Steel * .75f); SolidBox(at + right * s - Vector3.up * .4f, face, new Vector3(.25f, 5.0f, .25f), "Stunt park sign post"); }
            m.Box(at + Vector3.up * 2.6f, face, new Vector3(6.2f, 1.9f, .22f), Red, Red * .9f);
            m.Box(at + Vector3.up * 2.75f + face * .118f, face, new Vector3(5.9f, 1.6f, .02f), C(.95f, .92f, .84f));
            SolidBox(at + Vector3.up * 2.6f, face, new Vector3(6.2f, 1.9f, .22f), "Stunt park sign");
            Label("RED CLAY", at + Vector3.up * 3.95f + face * .135f, face, 5.4f, .55f, Red);
            Label("STUNT PARK", at + Vector3.up * 3.38f + face * .135f, face, 5.4f, .55f, C(.15f, .15f, .16f));
            Label("JUMPS  ·  LOOP  ·  HALF-PIPE", at + Vector3.up * 2.93f + face * .135f, face, 5.0f, .26f, C(.25f, .22f, .2f));
        }
        readonly List<(MeshRenderer mr, float w, float h)> labels = new();
        void Label(string text, Vector3 at, Vector3 facing, float w, float h, Color col)
        {
            var t = new GameObject("Lettering - " + text).AddComponent<TextMesh>(); t.transform.SetParent(root, false);
            t.font = font; t.text = text; t.anchor = TextAnchor.MiddleCenter; t.alignment = TextAlignment.Center; t.fontSize = 96; t.characterSize = .1f; t.color = col;
            t.transform.SetPositionAndRotation(at, Quaternion.LookRotation(-facing));
            var mr = t.GetComponent<MeshRenderer>(); mr.sharedMaterial = lettering; mr.shadowCastingMode = ShadowCastingMode.Off; mr.receiveShadows = false;
            var lod = t.gameObject.AddComponent<LODGroup>(); lod.SetLODs(new[] { new LOD(.012f, new Renderer[] { mr }) });
            labels.Add((mr, w, h));
        }
        void LateUpdate()
        {
            for (int i = labels.Count - 1; i >= 0; i--)
            {
                var (mr, w, h) = labels[i]; if (!mr) { labels.RemoveAt(i); continue; }
                var b = mr.localBounds; if (b.size.x < 1e-4f) continue;
                mr.transform.localScale = Vector3.one * Mathf.Min(h / b.size.y, w / b.size.x); mr.GetComponent<LODGroup>().RecalculateBounds(); labels.RemoveAt(i);
            }
        }

        // ---------------------------------------------------------------- recovery inside the park (Free Roam): the nearest clear spot on the floor
        public static readonly Vector3[] ResetSpots = { P(150, 20), P(128, 30), P(100, 30), P(110, 60), P(110, 88), P(100, 115), P(30, 34), P(30, 90), P(30, 118), P(60, 120), P(125, 126), P(146, 140) };
        public static IEnumerable<(Vector3 at, Quaternion facing)> ResetsNear(Vector3 from)
        {
            foreach (var p in ResetSpots.OrderBy(p => (p - from).sqrMagnitude))
            {
                var toward = Vector3.ProjectOnPlane(Centre - p, Vector3.up); if (toward.sqrMagnitude < 1) toward = Vector3.left;
                yield return (p + Vector3.up * .7f, Quaternion.LookRotation(toward.normalized));
            }
        }
    }
}
