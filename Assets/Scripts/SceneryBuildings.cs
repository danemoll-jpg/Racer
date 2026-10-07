using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Racer
{
    // 0.78 Part A3: the new buildings. Visual only: every new building is generated from its own colliders, so what is
    // drawn is where it collides — the walls are the wall collider, each pitched roof is drawn on its roof collider's
    // own shape (overhang, gable ends and soffit included), the chimney, flat roof and parapet on theirs, the entrance
    // steps on theirs. Detail is added on the surfaces: lap siding or brick courses, corner boards, base trim, window
    // frames with sills, mullions and shutters, doors, fascia, gutters and downspouts, ridge caps, chimney caps, porch
    // brackets, shop fascias and awnings, and windows that light up at night (Racer/Building).
    // Dan's house is modelled on the real one: it keeps its own model; only its windows light at night. Kyle's house
    // ("Friend across street") keeps its own model too and gains detail only (gutters, downspouts, ridge cap, corner
    // boards, lit windows) until Dan supplies photos. Fox Gully (a drive-through house with ramps) is left as it is.
    // Every other house and business gets a new design (style from its name: siding, colours, shutters, porch).
    // The old geometry is in "Phase 6 - architectural render batches" (merged per material across the town): those are
    // replaced by copies without the triangles of the redesigned buildings, so everything else in them stays drawn.
    public sealed class SceneryBuildings : MonoBehaviour
    {
        public int Redesigned { get; private set; }
        public int Detailed { get; private set; }
        public readonly List<string> Names = new();
        static Material material;
        readonly List<GameObject> shown = new();
        readonly List<(Renderer r, Material classic, Material lit)> glass = new();
        static readonly Color Warm = new(1f, .74f, .38f);

        public void SetShown(bool on)
        {
            foreach (var g in shown) if (g) g.SetActive(on);
            foreach (var (r, classic, lit) in glass) if (r) r.sharedMaterial = on ? lit : classic;
        }
        void LateUpdate()
        {
            float level = Mathf.Clamp01(VehicleLights.Level);
            Shader.SetGlobalFloat("_RacerWindowGlow", level);
            foreach (var (r, _, lit) in glass) if (lit) { lit.SetColor("_EmissionColor", Warm * (level * 1.4f)); }
        }

        // ---------------------------------------------------------------- mesh building
        sealed class Builder
        {
            public readonly List<Vector3> v = new(); public readonly List<Vector3> n = new(); public readonly List<Color> c = new(); public readonly List<int> t = new();
            public void Tri(Vector3 a, Vector3 b, Vector3 d, Color col)
            {
                var nn = Vector3.Cross(b - a, d - a); if (nn.sqrMagnitude < 1e-10f) return; nn.Normalize();
                int i = v.Count; v.Add(a); v.Add(b); v.Add(d); n.Add(nn); n.Add(nn); n.Add(nn); c.Add(col); c.Add(col); c.Add(col); t.Add(i); t.Add(i + 1); t.Add(i + 2);
            }
            // a quad facing `outward` (vertex order is fixed so its normal points that way)
            public void Quad(Vector3 a, Vector3 b, Vector3 d, Vector3 e, Color col, Vector3 outward)
            {
                if (Vector3.Dot(Vector3.Cross(b - a, d - a), outward) < 0) { (b, e) = (e, b); }
                Tri(a, b, d, col); Tri(a, d, e, col);
            }
            // an axis-aligned box (in the building frame) between min and max
            public void Box(Vector3 min, Vector3 max, Color col, bool bottom = false)
            {
                Vector3 p(float x, float y, float z) => new(x == 0 ? min.x : max.x, y == 0 ? min.y : max.y, z == 0 ? min.z : max.z);
                Quad(p(0, 0, 1), p(1, 0, 1), p(1, 1, 1), p(0, 1, 1), col, Vector3.forward);
                Quad(p(1, 0, 0), p(0, 0, 0), p(0, 1, 0), p(1, 1, 0), col, Vector3.back);
                Quad(p(1, 0, 1), p(1, 0, 0), p(1, 1, 0), p(1, 1, 1), col, Vector3.right);
                Quad(p(0, 0, 0), p(0, 0, 1), p(0, 1, 1), p(0, 1, 0), col, Vector3.left);
                Quad(p(0, 1, 1), p(1, 1, 1), p(1, 1, 0), p(0, 1, 0), col, Vector3.up);
                if (bottom) Quad(p(0, 0, 0), p(1, 0, 0), p(1, 0, 1), p(0, 0, 1), col, Vector3.down);
            }
            // a box along a wall: u along the wall, w out of it (from the wall face), y up
            public void WallBox(Vector3 origin, Vector3 u, Vector3 w, float u0, float u1, float y0, float y1, float w0, float w1, Color col)
            {
                Vector3 P(float a, float y, float b) => origin + u * a + Vector3.up * y + w * b;
                var q = new[] { P(u0, y0, w1), P(u1, y0, w1), P(u1, y1, w1), P(u0, y1, w1) };
                Quad(q[0], q[1], q[2], q[3], col, w);
                Quad(P(u0, y1, w0), P(u0, y1, w1), P(u1, y1, w1), P(u1, y1, w0), col, Vector3.up);
                Quad(P(u0, y0, w0), P(u1, y0, w0), P(u1, y0, w1), P(u0, y0, w1), col, Vector3.down);
                Quad(P(u0, y0, w0), P(u0, y0, w1), P(u0, y1, w1), P(u0, y1, w0), col, -u);
                Quad(P(u1, y0, w1), P(u1, y0, w0), P(u1, y1, w0), P(u1, y1, w1), col, u);
            }
            public Mesh Mesh(string name)
            {
                var m = new Mesh { name = name, indexFormat = v.Count > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16 };
                m.SetVertices(v); m.SetNormals(n); m.SetColors(c); m.SetTriangles(t, 0); m.RecalculateBounds(); m.UploadMeshData(true); return m;
            }
        }

        // ---------------------------------------------------------------- styles
        struct Style { public Color wall, trim, roof, shutter, door, accent; public bool brick, shutters, porchBrackets; }
        static readonly Color[] Sidings = { new(.86f, .85f, .80f), new(.86f, .80f, .63f), new(.85f, .77f, .52f), new(.56f, .62f, .49f), new(.46f, .56f, .63f), new(.62f, .63f, .62f), new(.70f, .61f, .47f), new(.62f, .31f, .25f), new(.53f, .23f, .18f) };
        static readonly Color[] Roofs = { new(.20f, .22f, .24f), new(.31f, .23f, .18f), new(.21f, .30f, .25f), new(.34f, .37f, .41f), new(.45f, .17f, .13f), new(.26f, .26f, .27f) };
        static readonly Color[] Shutters = { new(.08f, .09f, .10f), new(.14f, .26f, .17f), new(.13f, .18f, .30f), new(.36f, .12f, .11f), new(.30f, .22f, .15f) };
        static readonly Color[] Doors = { new(.32f, .21f, .14f), new(.45f, .12f, .10f), new(.12f, .22f, .34f), new(.16f, .27f, .19f), new(.86f, .84f, .78f) };
        static readonly Color[] Accents = { new(.15f, .39f, .37f), new(.64f, .22f, .16f), new(.66f, .46f, .19f), new(.20f, .32f, .55f), new(.35f, .22f, .40f) };
        static Style StyleFor(string name, bool business)
        {
            var r = new System.Random(name.Aggregate(17, (h, ch) => unchecked(h * 31 + ch)));
            int wall = r.Next(Sidings.Length); bool brick = wall >= 7 || (business && r.NextDouble() < .35);
            var s = new Style
            {
                wall = brick && wall < 7 ? Sidings[7 + r.Next(2)] : Sidings[wall], brick = brick,
                trim = wall == 0 || wall == 1 ? new Color(.95f, .94f, .90f) * (r.NextDouble() < .5 ? 1 : .9f) : r.NextDouble() < .75 ? new Color(.93f, .92f, .87f) : new Color(.25f, .22f, .19f),
                roof = Roofs[r.Next(Roofs.Length)], shutter = Shutters[r.Next(Shutters.Length)], door = Doors[r.Next(Doors.Length)], accent = Accents[r.Next(Accents.Length)],
                shutters = !business && r.NextDouble() < .55, porchBrackets = r.NextDouble() < .7
            };
            return s;
        }
        static Color Shade(Color c, float k) { var o = c * k; o.a = 1; return o; }

        // ---------------------------------------------------------------- reading a site
        sealed class Site
        {
            public Transform root, arch; public Matrix4x4 toLocal, toWorld; public Bounds wall; public bool business;
            public readonly List<(Vector3[] tris, bool porch)> roofs = new(); public readonly List<Bounds> boxes = new(), steps = new(); public Bounds? chimney, flatRoof; public readonly List<Bounds> parapets = new();
            public float door;
        }
        static Bounds LocalBox(BoxCollider b, Matrix4x4 toLocal)
        {
            var t = b.transform; var e = b.size * .5f; Bounds? r = null;
            for (int i = 0; i < 8; i++)
            {
                var p = toLocal.MultiplyPoint3x4(t.TransformPoint(b.center + Vector3.Scale(e, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1))));
                if (r == null) r = new Bounds(p, Vector3.zero); else { var x = r.Value; x.Encapsulate(p); r = x; }
            }
            return r.Value;
        }
        static Site Read(Transform site)
        {
            var arch = site.Find("Phase 6 architecture"); if (!arch) return null;
            var wallCol = arch.GetComponentsInChildren<BoxCollider>(true).FirstOrDefault(b => b.name == "Wall collision"); if (!wallCol) return null;
            var s = new Site { root = site, arch = arch, business = site.name.Contains("business") };
            s.toWorld = Matrix4x4.TRS(arch.position, arch.rotation, Vector3.one); s.toLocal = s.toWorld.inverse;
            s.wall = LocalBox(wallCol, s.toLocal);
            foreach (var mc in arch.GetComponentsInChildren<MeshCollider>(true).Where(m => m.name == "Pitched roof" && m.sharedMesh && m.sharedMesh.isReadable))
            {
                var mv = mc.sharedMesh.vertices; var mt = mc.sharedMesh.triangles; var pts = new Vector3[mt.Length];
                for (int i = 0; i < mt.Length; i++) pts[i] = s.toLocal.MultiplyPoint3x4(mc.transform.TransformPoint(mv[mt[i]]));
                var lo = pts.Min(p => p.y); s.roofs.Add((pts, lo < s.wall.max.y - .5f));
            }
            foreach (var b in arch.GetComponentsInChildren<BoxCollider>(true))
            {
                var lb = LocalBox(b, s.toLocal);
                if (b.name == "Chimney") s.chimney = lb; else if (b.name == "Flat roof collision") s.flatRoof = lb; else if (b.name.Contains("parapet")) s.parapets.Add(lb);
            }
            foreach (Transform c in site) if (c.name.StartsWith("Entrance step") && c.GetComponent<BoxCollider>() is BoxCollider sb) s.steps.Add(LocalBox(sb, s.toLocal));
            s.door = s.steps.Count > 0 ? s.steps.Average(b => b.center.x) : s.wall.center.x;
            return s;
        }

        // ---------------------------------------------------------------- building geometry
        // The four walls of the wall collider with siding courses (or brick), corner boards, base trim and the openings.
        static void Walls(Builder m, Site s, Style st, bool windows)
        {
            var w = s.wall; float y0 = w.min.y, y1 = w.max.y;
            var faces = new[] { (o: new Vector3(w.min.x, 0, w.max.z), u: Vector3.right, out_: Vector3.forward, len: w.size.x, front: true),
                                (o: new Vector3(w.max.x, 0, w.min.z), u: Vector3.left, out_: Vector3.back, len: w.size.x, front: false),
                                (o: new Vector3(w.max.x, 0, w.max.z), u: Vector3.back, out_: Vector3.right, len: w.size.z, front: false),
                                (o: new Vector3(w.min.x, 0, w.min.z), u: Vector3.forward, out_: Vector3.left, len: w.size.z, front: false) };
            float course = st.brick ? .24f : .30f; int courses = Mathf.Max(1, Mathf.RoundToInt((y1 - y0) / course));
            foreach (var f in faces)
            {
                for (int i = 0; i < courses; i++)
                {
                    float a = y0 + (y1 - y0) * i / courses, b = y0 + (y1 - y0) * (i + 1) / courses;
                    var col = Shade(st.wall, st.brick ? .94f + .1f * Mathf.PerlinNoise(i * .7f, f.len) : (i % 2 == 0 ? 1 : .955f));
                    Vector3 P(float u, float y) => f.o + f.u * u + Vector3.up * y;
                    m.Quad(P(0, a), P(f.len, a), P(f.len, b), P(0, b), col, f.out_);
                    if (!st.brick && i > 0) m.WallBox(f.o, f.u, f.out_, 0, f.len, a - .018f, a + .012f, 0, .022f, Shade(st.wall, .82f)); // the lap shadow line
                }
                m.WallBox(f.o, f.u, f.out_, -.04f, f.len + .04f, y0, y0 + .38f, 0, .045f, Shade(st.brick ? new Color(.58f, .56f, .52f) : st.trim, .92f));
                m.WallBox(f.o, f.u, f.out_, -.04f, .14f, y0, y1, 0, .035f, st.trim); m.WallBox(f.o, f.u, f.out_, f.len - .14f, f.len + .04f, y0, y1, 0, .035f, st.trim);
                if (!windows) continue;
                bool shop = s.business;
                float height = shop ? 1.6f : 1.35f, width = shop ? 1.4f : 1.15f, sill = y0 + (shop ? 1.1f : 1.75f);
                if (sill + height > y1 - .5f) sill = y1 - .5f - height;
                int count = Mathf.Max(1, Mathf.FloorToInt((f.len - 1.2f) / (shop ? 4.2f : 3.3f)));
                float doorU = f.front ? (s.door - w.min.x) : -100;
                if (f.front && shop) { ShopFront(m, s, st, f.o, f.u, f.out_, f.len, doorU); continue; }
                for (int k = 0; k < count; k++)
                {
                    float u = f.len * (k + .5f) / count; if (Mathf.Abs(u - doorU) < 1.6f) continue;
                    Window(m, st, f.o, f.u, f.out_, u, sill, width, height, Hash(s.root.name, k + (int)(f.len * 10)));
                }
                if (f.front) Door(m, st, f.o, f.u, f.out_, doorU, y0, shop);
            }
        }
        static float Hash(string s, int k) { unchecked { int h = k * 7919; foreach (char ch in s) h = h * 31 + ch; return (h & 0xffff) / 65535f; } }
        static Color Glass(float lit) { var g = new Color(.17f, .25f, .30f); g.a = lit > .45f ? .1f + (1 - lit) * .4f : 1; return g; } // alpha < 1: glows at night
        static void Window(Builder m, Style st, Vector3 o, Vector3 u, Vector3 w, float c, float sill, float width, float height, float lit)
        {
            float a = c - width * .5f, b = c + width * .5f;
            m.WallBox(o, u, w, a, b, sill, sill + height, 0, .03f, Glass(lit));
            m.WallBox(o, u, w, a - .1f, b + .1f, sill + height, sill + height + .12f, 0, .08f, st.trim);        // head
            m.WallBox(o, u, w, a - .14f, b + .14f, sill - .1f, sill, 0, .12f, st.trim);                          // sill
            m.WallBox(o, u, w, a - .1f, a, sill, sill + height, 0, .07f, st.trim); m.WallBox(o, u, w, b, b + .1f, sill, sill + height, 0, .07f, st.trim);
            m.WallBox(o, u, w, c - .03f, c + .03f, sill, sill + height, 0, .06f, st.trim);                       // mullion
            m.WallBox(o, u, w, a, b, sill + height * .5f - .03f, sill + height * .5f + .03f, 0, .06f, st.trim);  // transom bar
            if (st.shutters) { m.WallBox(o, u, w, a - .62f, a - .14f, sill, sill + height, 0, .05f, st.shutter); m.WallBox(o, u, w, b + .14f, b + .62f, sill, sill + height, 0, .05f, st.shutter); }
        }
        static void Door(Builder m, Style st, Vector3 o, Vector3 u, Vector3 w, float c, float y0, bool shop)
        {
            float width = shop ? 2.2f : 1.0f, height = shop ? 2.7f : 2.2f;
            m.WallBox(o, u, w, c - width * .5f, c + width * .5f, y0, y0 + height, 0, .04f, shop ? Glass(.8f) : st.door);
            m.WallBox(o, u, w, c - width * .5f - .14f, c - width * .5f, y0, y0 + height + .14f, 0, .09f, st.trim);
            m.WallBox(o, u, w, c + width * .5f, c + width * .5f + .14f, y0, y0 + height + .14f, 0, .09f, st.trim);
            m.WallBox(o, u, w, c - width * .5f - .14f, c + width * .5f + .14f, y0 + height, y0 + height + .16f, 0, .11f, st.trim);
            if (!shop) { m.WallBox(o, u, w, c - .22f, c + .22f, y0 + height - .7f, y0 + height - .25f, 0, .05f, Glass(.7f)); m.WallBox(o, u, w, c + .3f, c + .38f, y0 + 1.0f, y0 + 1.1f, 0, .09f, new Color(.75f, .65f, .3f)); }
        }
        static void ShopFront(Builder m, Site s, Style st, Vector3 o, Vector3 u, Vector3 w, float len, float doorU)
        {
            float y0 = s.wall.min.y, y1 = s.wall.max.y;
            float pane = len * 7.1f / 24, gap = len * 7 / 24;
            foreach (float c in new[] { len * .5f - gap, len * .5f + gap })
            {
                m.WallBox(o, u, w, c - pane * .5f, c + pane * .5f, y0 + .8f, y0 + 3.3f, 0, .03f, Glass(Hash(s.root.name, (int)c) * .5f + .5f));
                m.WallBox(o, u, w, c - pane * .5f - .12f, c + pane * .5f + .12f, y0 + .55f, y0 + .8f, 0, .14f, st.trim);
                m.WallBox(o, u, w, c - pane * .5f - .12f, c + pane * .5f + .12f, y0 + 3.3f, y0 + 3.45f, 0, .09f, st.trim);
                for (int k = 0; k <= 3; k++) { float x = c - pane * .5f + pane * k / 3; m.WallBox(o, u, w, x - .05f, x + .05f, y0 + .8f, y0 + 3.3f, 0, .07f, st.trim); }
            }
            Door(m, st, o, u, w, doorU, y0, true);
            // fascia band (the storefront name sits in front of it) and an awning over the windows
            m.WallBox(o, u, w, -.05f, len + .05f, y1 - .95f, y1 - .1f, 0, .42f, st.accent);
            m.WallBox(o, u, w, -.05f, len + .05f, y1 - 1.0f, y1 - .95f, 0, .46f, st.trim);
            for (int k = 0; k < 2; k++)
            {
                float c = len * .5f + (k == 0 ? -gap : gap), a = c - pane * .5f - .3f, b = c + pane * .5f + .3f;
                for (int sIdx = 0; sIdx < 8; sIdx++)
                {
                    float sa = a + (b - a) * sIdx / 8, sb = a + (b - a) * (sIdx + 1) / 8; var col = sIdx % 2 == 0 ? st.accent : new Color(.92f, .90f, .84f);
                    Vector3 P(float x, float y, float z) => o + u * x + Vector3.up * y + w * z;
                    float top = y0 + 3.75f, bot = y0 + 3.35f;
                    m.Quad(P(sa, top, .02f), P(sb, top, .02f), P(sb, bot, 1.3f), P(sa, bot, 1.3f), col, (Vector3.up + w).normalized);
                    m.Quad(P(sa, bot, 1.3f), P(sb, bot, 1.3f), P(sb, bot - .25f, 1.3f), P(sa, bot - .25f, 1.3f), col, w);
                }
            }
        }
        // A pitched roof drawn on its collider: sloped faces in courses, vertical faces (gable ends) as siding with a
        // vent, the flat underside as the soffit; then fascia, gutters, downspouts and a ridge cap.
        static void Roof(Builder m, Site s, Style st, Vector3[] tri, bool porch, bool gutters)
        {
            float lo = tri.Min(p => p.y), hi = tri.Max(p => p.y);
            var eaves = new List<(Vector3 a, Vector3 b)>();
            for (int i = 0; i < tri.Length; i += 3)
            {
                Vector3 a = tri[i], b = tri[i + 1], c = tri[i + 2]; var nn = Vector3.Cross(b - a, c - a).normalized;
                if (nn.y > .2f)
                {
                    // shingle courses: thin bands across the slope, alternating a touch in shade
                    int bands = Mathf.Clamp(Mathf.RoundToInt((hi - lo) / .35f), 2, 12);
                    for (int k = 0; k < bands; k++)
                    {
                        float y0 = lo + (hi - lo) * k / bands, y1 = lo + (hi - lo) * (k + 1) / bands; var col = Shade(st.roof, k % 2 == 0 ? 1 : .93f);
                        Clip(m, a, b, c, y0, y1, col, nn * .01f);
                    }
                    foreach (var (p, q) in new[] { (a, b), (b, c), (c, a) }) if (Mathf.Abs(p.y - lo) < .02f && Mathf.Abs(q.y - lo) < .02f) eaves.Add((p, q));
                }
                else if (nn.y < -.2f) m.Tri(a, b, c, Shade(st.trim, .85f));
                else { m.Tri(a, b, c, st.wall); }
            }
            // gable-end trim band and a little vent on the gable ends
            for (int i = 0; i < tri.Length; i += 3)
            {
                Vector3 a = tri[i], b = tri[i + 1], c = tri[i + 2]; var nn = Vector3.Cross(b - a, c - a).normalized; if (Mathf.Abs(nn.y) > .2f || porch) continue;
                var mid = (a + b + c) / 3; var side = Vector3.Cross(Vector3.up, nn).normalized;
                m.WallBox(new Vector3(mid.x, 0, mid.z), side, nn, -.25f, .25f, mid.y - .15f, mid.y + .2f, 0, .04f, Shade(st.trim, .9f));
            }
            foreach (var (a, b) in eaves)
            {
                var along = (b - a).normalized; var outward = Vector3.Cross(along, Vector3.up); if (Vector3.Dot(outward, (a + b) * .5f - s.wall.center) < 0) outward = -outward;
                m.WallBox(new Vector3(a.x, 0, a.z), along, outward, 0, (b - a).magnitude, lo - .2f, lo + .02f, -.02f, .04f, st.trim);       // fascia
                if (gutters)
                {
                    var gutter = new Color(.80f, .80f, .78f);
                    m.WallBox(new Vector3(a.x, 0, a.z), along, outward, -.05f, (b - a).magnitude + .05f, lo - .26f, lo - .1f, .04f, .18f, gutter);
                }
            }
            // ridge cap along the top edge
            var top = tri.Where(p => hi - p.y < .02f).Distinct().ToList();
            if (top.Count >= 2) { var a = top[0]; var b = top.OrderByDescending(p => (p - a).sqrMagnitude).First(); if ((b - a).sqrMagnitude > .01f) { var along = (b - a).normalized; var side = Vector3.Cross(Vector3.up, along); m.WallBox(new Vector3(a.x, 0, a.z) - side * .14f, along, side, -.1f, (b - a).magnitude + .1f, hi - .02f, hi + .1f, 0, .28f, Shade(st.roof, .8f)); } }
        }
        // Part of triangle abc between heights y0 and y1 (a slab of a sloped face), pushed out by `lift`.
        static void Clip(Builder m, Vector3 a, Vector3 b, Vector3 c, float y0, float y1, Color col, Vector3 lift)
        {
            var poly = new List<Vector3> { a, b, c };
            poly = Cut(poly, y0, true); poly = Cut(poly, y1, false);
            for (int i = 1; i + 1 < poly.Count; i++) m.Tri(poly[0] + lift, poly[i] + lift, poly[i + 1] + lift, col);
        }
        static List<Vector3> Cut(List<Vector3> poly, float y, bool keepAbove)
        {
            var o = new List<Vector3>();
            for (int i = 0; i < poly.Count; i++)
            {
                var p = poly[i]; var q = poly[(i + 1) % poly.Count]; bool pin = keepAbove ? p.y >= y : p.y <= y, qin = keepAbove ? q.y >= y : q.y <= y;
                if (pin) o.Add(p);
                if (pin != qin) o.Add(Vector3.Lerp(p, q, (y - p.y) / (q.y - p.y)));
            }
            return o;
        }
        static void Downspouts(Builder m, Site s, Style st)
        {
            var w = s.wall; var col = new Color(.78f, .78f, .76f);
            foreach (var (x, z) in new[] { (w.min.x, w.max.z), (w.max.x, w.max.z), (w.min.x, w.min.z), (w.max.x, w.min.z) })
            {
                float sx = Mathf.Sign(x - w.center.x), sz = Mathf.Sign(z - w.center.z);
                var c = new Vector3(x - sx * .25f, 0, z + sz * .02f);
                float foot = Mathf.Min(w.min.y + .15f, GroundAt(s, c.x, c.z + sz * .05f) - .05f); // down to the ground (0.79)
                m.Box(new Vector3(c.x - .05f, foot, Mathf.Min(c.z, c.z + sz * .1f)), new Vector3(c.x + .05f, w.max.y - .1f, Mathf.Max(c.z, c.z + sz * .1f)), col, true);
            }
        }
        static void Chimney(Builder m, Bounds b, Color brick)
        {
            m.Box(b.min, b.max, brick);
            m.Box(new Vector3(b.min.x - .08f, b.max.y - .22f, b.min.z - .08f), new Vector3(b.max.x + .08f, b.max.y - .1f, b.max.z + .08f), Shade(brick, .8f));
            m.Box(new Vector3(b.min.x - .1f, b.max.y, b.min.z - .1f), new Vector3(b.max.x + .1f, b.max.y + .12f, b.max.z + .1f), new Color(.45f, .45f, .44f));
        }
        static void PorchBrackets(Builder m, Site s, Style st, Vector3[] tri)
        {
            var b = new Bounds(tri[0], Vector3.zero); foreach (var p in tri) b.Encapsulate(p);
            float wallZ = s.wall.max.z; if (b.max.z <= wallZ + .2f) return;
            foreach (float x in new[] { b.min.x + .25f, b.max.x - .25f })
            {
                // a diagonal timber from the wall up to the porch roof's front edge
                var p0 = new Vector3(x, b.min.y - .9f, wallZ); var p1 = new Vector3(x, b.min.y - .05f, b.max.z - .2f); var d = (p1 - p0).normalized; var side = Vector3.right * .06f; var up = Vector3.Cross(d, Vector3.right).normalized * .06f;
                Vector3[] q = { p0 - side - up, p0 + side - up, p0 + side + up, p0 - side + up, p1 - side - up, p1 + side - up, p1 + side + up, p1 - side + up };
                m.Quad(q[0], q[1], q[5], q[4], st.trim, -up); m.Quad(q[3], q[7], q[6], q[2], st.trim, up); m.Quad(q[0], q[4], q[7], q[3], st.trim, -side); m.Quad(q[1], q[2], q[6], q[5], st.trim, side);
            }
        }
        static void Steps(Builder m, Site s, Bounds b)
        {
            m.Box(b.min, b.max, new Color(.66f, .64f, .60f));
            m.Box(new Vector3(b.min.x - .02f, b.max.y - .04f, b.max.z - .05f), new Vector3(b.max.x + .02f, b.max.y + .005f, b.max.z + .02f), new Color(.58f, .56f, .53f));
            // 0.79: the step stands on a block that reaches the ground (never hovering)
            float ground = LowestGround(s, b.min.x, b.max.x, b.min.z, b.max.z);
            if (ground < b.min.y + .01f) m.Box(new Vector3(b.min.x, ground - FoundationMargin, b.min.z), new Vector3(b.max.x, b.min.y + .01f, b.max.z), new Color(.60f, .58f, .55f), true);
        }

        // ---------------------------------------------------------------- 0.79 Part D: standing on the ground
        // The walls are drawn from the wall collider, whose floor is level; on a slope the downhill side would hang in the
        // air. Every generated building stands on a foundation from its floor down to below the lowest ground under its
        // footprint (sampled at every corner and every ~0.75 m along the edges), within the wall footprint; steps, an
        // exterior chimney and the downspouts reach the ground the same way. Nothing here touches a collider.
        public const float FoundationMargin = .3f;
        // The ground's height (building frame) under a point of the building frame: the first static surface below the
        // floor that is not part of this building.
        static readonly RaycastHit[] hits = new RaycastHit[32];
        static float GroundAt(Site s, float x, float z)
        {
            var from = s.toWorld.MultiplyPoint3x4(new Vector3(x, s.wall.min.y + 2.5f, z));
            int n = Physics.RaycastNonAlloc(from, Vector3.down, hits, 80, ~0, QueryTriggerInteraction.Ignore);
            System.Array.Sort(hits, 0, n, SceneryTrees.ByDistance);
            for (int k = 0; k < n; k++)
            {
                var h = hits[k];
                if (h.collider.transform.IsChildOf(s.root) || (h.collider.attachedRigidbody && !h.collider.attachedRigidbody.isKinematic)) continue;
                return s.toLocal.MultiplyPoint3x4(h.point).y;
            }
            return s.wall.min.y;
        }
        static float LowestGround(Site s, float x0, float x1, float z0, float z1)
        {
            float lo = float.MaxValue; int nx = Mathf.Max(1, Mathf.CeilToInt((x1 - x0) / 1f)), nz = Mathf.Max(1, Mathf.CeilToInt((z1 - z0) / 1f));
            for (int i = 0; i <= nx; i++) { float x = Mathf.Lerp(x0, x1, (float)i / nx); lo = Mathf.Min(lo, Mathf.Min(GroundAt(s, x, z0), GroundAt(s, x, z1))); }
            for (int k = 0; k <= nz; k++) { float z = Mathf.Lerp(z0, z1, (float)k / nz); lo = Mathf.Min(lo, Mathf.Min(GroundAt(s, x0, z), GroundAt(s, x1, z))); }
            lo = Mathf.Min(lo, GroundAt(s, (x0 + x1) * .5f, (z0 + z1) * .5f));
            return lo;
        }
        // Brick under a brick house, otherwise concrete block, in courses; a touch proud of the wall so the base trim
        // still reads above it.
        static void Foundation(Builder m, Site s, Style st)
        {
            var w = s.wall; float bottom = LowestGround(s, w.min.x, w.max.x, w.min.z, w.max.z) - FoundationMargin, top = w.min.y + .02f;
            var block = st.brick ? Shade(st.wall, .78f) : new Color(.57f, .56f, .53f);
            float course = st.brick ? .16f : .2f; const float proud = .03f;
            var min = new Vector3(w.min.x - proud, bottom, w.min.z - proud); var max = new Vector3(w.max.x + proud, top, w.max.z + proud);
            int n = Mathf.Max(1, Mathf.CeilToInt((top - bottom) / course));
            for (int i = 0; i < n; i++)
            {
                float a = Mathf.Max(bottom, top - (i + 1) * course), b = top - i * course;
                var col = Shade(block, i % 2 == 0 ? 1 : .93f);
                m.Box(new Vector3(min.x, a, min.z), new Vector3(max.x, b, max.z), col, i == n - 1);
            }
        }

        // ---------------------------------------------------------------- the town
        public void Build(Scene scene, List<Renderer> hidden)
        {
            material ??= Resources.Load<Material>("Scenery/Building"); if (!material) { Debug.LogWarning("Scenery: building material missing"); return; }
            var root = scene.GetRootGameObjects().FirstOrDefault(g => g.name == "Remembered houses and approximate buildings"); if (!root) return;
            var replaced = new List<Site>();
            foreach (Transform site in root.transform)
            {
                bool dan = site.name.StartsWith("Dan"), kyle = site.name.StartsWith("Friend"), fox = site.name.StartsWith("Fox Gully");
                if (fox) continue;
                if (dan) { LightGlass(site, hidden); continue; }
                var s = Read(site); if (s == null) continue;
                var st = StyleFor(site.name, s.business); var m = new Builder();
                // 0.87: Kyle's house is its own model in the scene (Assets/Scenery/KylesHouse, from Dan's photo), with its
                // gutters, trim and lit windows; nothing is added or hidden here, in either scenery setting.
                if (kyle && site.Find("Kyle's house (0.87)")) continue;
                if (kyle)
                {
                    // detail only: the house keeps its own model and colours
                    var own = Shade(new Color(.53f, .30f, .23f), 1); st.trim = new Color(.88f, .85f, .72f);
                    foreach (var (tri, porch) in s.roofs) RoofDetail(m, s, st, tri);
                    Downspouts(m, s, st); Corners(m, s, st);
                    LightGlass(site, hidden); Emit(m, s, "Kyle's house detail"); Detailed++; Names.Add(site.name + " (detail)"); continue;
                }
                Foundation(m, s, st); Walls(m, s, st, true);
                foreach (var (tri, porch) in s.roofs) { Roof(m, s, st, tri, porch, !porch); if (porch && st.porchBrackets) PorchBrackets(m, s, st, tri); }
                if (s.roofs.Any(r => !r.porch)) Downspouts(m, s, st);
                if (s.chimney is Bounds ch)
                {
                    var brickColour = st.brick ? Shade(st.wall, .9f) : new Color(.53f, .30f, .23f); Chimney(m, ch, brickColour);
                    // an exterior chimney (starting near the floor) goes down to the ground
                    if (ch.min.y < s.wall.min.y + 1) { float g = LowestGround(s, ch.min.x, ch.max.x, ch.min.z, ch.max.z); if (g < ch.min.y) m.Box(new Vector3(ch.min.x, g - FoundationMargin, ch.min.z), new Vector3(ch.max.x, ch.min.y + .01f, ch.max.z), Shade(brickColour, .92f), true); }
                }
                if (s.flatRoof is Bounds fr) { m.Box(fr.min, fr.max, Shade(st.roof, .85f)); m.Box(new Vector3(fr.min.x, fr.max.y, fr.min.z), new Vector3(fr.max.x, fr.max.y + .12f, fr.min.z + .25f), st.trim); }
                foreach (var p in s.parapets) { m.Box(p.min, p.max, st.wall); m.Box(new Vector3(p.min.x - .05f, p.max.y, p.min.z - .05f), new Vector3(p.max.x + .05f, p.max.y + .12f, p.max.z + .05f), st.trim); }
                foreach (var b in s.steps) Steps(m, s, b);
                Emit(m, s, "New " + site.name);
                // the old model of this building (its own enabled renderers: the porch steps, House 2's unbatched parts)
                foreach (var r in s.arch.GetComponentsInChildren<Renderer>(true)) if (r.enabled && r is MeshRenderer && !r.GetComponent<TextMesh>()) hidden.Add(r);
                foreach (Transform c in site) if (c.name.StartsWith("Entrance step") && c.GetComponent<MeshRenderer>() is MeshRenderer sr && sr.enabled) hidden.Add(sr);
                replaced.Add(s); Redesigned++; Names.Add(site.name);
            }
            TrimBatches(scene, replaced, hidden);
        }
        static void Corners(Builder m, Site s, Style st)
        {
            var w = s.wall;
            foreach (var (x, z) in new[] { (w.min.x, w.max.z), (w.max.x, w.max.z), (w.min.x, w.min.z), (w.max.x, w.min.z) })
            {
                float sx = Mathf.Sign(x - w.center.x), sz = Mathf.Sign(z - w.center.z);
                m.Box(new Vector3(Mathf.Min(x, x + sx * .04f), w.min.y + .3f, Mathf.Min(z - sz * .16f, z + sz * .04f)), new Vector3(Mathf.Max(x, x + sx * .04f), w.max.y - .05f, Mathf.Max(z - sz * .16f, z + sz * .04f)), st.trim);
            }
        }
        static void RoofDetail(Builder m, Site s, Style st, Vector3[] tri)
        {
            float lo = tri.Min(p => p.y), hi = tri.Max(p => p.y);
            for (int i = 0; i < tri.Length; i += 3)
            {
                Vector3 a = tri[i], b = tri[i + 1], c = tri[i + 2]; var nn = Vector3.Cross(b - a, c - a).normalized; if (nn.y <= .2f) continue;
                foreach (var (p, q) in new[] { (a, b), (b, c), (c, a) })
                    if (Mathf.Abs(p.y - lo) < .02f && Mathf.Abs(q.y - lo) < .02f)
                    {
                        var along = (q - p).normalized; var outward = Vector3.Cross(along, Vector3.up); if (Vector3.Dot(outward, (p + q) * .5f - s.wall.center) < 0) outward = -outward;
                        m.WallBox(new Vector3(p.x, 0, p.z), along, outward, -.05f, (q - p).magnitude + .05f, lo - .26f, lo - .1f, .04f, .18f, new Color(.80f, .80f, .78f));
                    }
            }
            var top = tri.Where(p => hi - p.y < .02f).Distinct().ToList();
            if (top.Count >= 2) { var a = top[0]; var b = top.OrderByDescending(p => (p - a).sqrMagnitude).First(); if ((b - a).sqrMagnitude > .01f) { var along = (b - a).normalized; var side = Vector3.Cross(Vector3.up, along); m.WallBox(new Vector3(a.x, 0, a.z) - side * .14f, along, side, -.1f, (b - a).magnitude + .1f, hi - .02f, hi + .1f, 0, .28f, new Color(.16f, .17f, .18f)); } }
        }
        void Emit(Builder m, Site s, string name)
        {
            var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer)); go.transform.SetParent(transform, false);
            go.transform.SetPositionAndRotation(s.toWorld.GetColumn(3), s.arch.rotation);
            go.GetComponent<MeshFilter>().sharedMesh = m.Mesh(name); var r = go.GetComponent<MeshRenderer>(); r.sharedMaterial = material; r.shadowCastingMode = ShadowCastingMode.On;
            shown.Add(go);
        }
        // Lit windows for a house that keeps its own model: its glass renderers get a copy of their material that glows.
        void LightGlass(Transform site, List<Renderer> hidden)
        {
            foreach (var r in site.GetComponentsInChildren<MeshRenderer>(true))
            {
                var mat = r.sharedMaterial; if (!r.enabled || r.GetComponent<TextMesh>() || !mat || mat.name.IndexOf("glass", System.StringComparison.OrdinalIgnoreCase) < 0) continue;
                var lit = new Material(mat) { name = mat.name + " (lit at night)" }; lit.EnableKeyword("_EMISSION"); lit.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None; lit.SetColor("_EmissionColor", Color.black);
                glass.Add((r, mat, lit));
            }
        }
        // The town's merged batches without the redesigned buildings' triangles.
        void TrimBatches(Scene scene, List<Site> sites, List<Renderer> hidden)
        {
            var batches = scene.GetRootGameObjects().FirstOrDefault(g => g.name == "Phase 6 - architectural render batches"); if (!batches || sites.Count == 0) return;
            var regions = sites.Select(s => { var b = s.wall; b.Expand(new Vector3(3.4f, 0, 5.4f)); b.max += Vector3.up * 8; b.min -= Vector3.up * 1.5f; return (s.toLocal, b); }).ToList();
            foreach (var mr in batches.GetComponentsInChildren<MeshRenderer>(true))
            {
                var mf = mr.GetComponent<MeshFilter>(); if (!mr.enabled || !mf || !mf.sharedMesh || !mf.sharedMesh.isReadable) continue;
                var mesh = mf.sharedMesh; var v = mesh.vertices; var keep = new List<int>(); var toWorld = mr.transform.localToWorldMatrix; int removed = 0;
                for (int sm = 0; sm < mesh.subMeshCount; sm++)
                {
                    var t = mesh.GetTriangles(sm);
                    for (int i = 0; i < t.Length; i += 3)
                    {
                        var c = toWorld.MultiplyPoint3x4((v[t[i]] + v[t[i + 1]] + v[t[i + 2]]) / 3); bool inside = false;
                        foreach (var (toLocal, b) in regions) if (b.Contains(toLocal.MultiplyPoint3x4(c))) { inside = true; break; }
                        if (inside) removed++; else { keep.Add(t[i]); keep.Add(t[i + 1]); keep.Add(t[i + 2]); }
                    }
                }
                if (removed == 0) continue;
                var copy = new Mesh { name = mesh.name + " (kept)", indexFormat = mesh.indexFormat };
                copy.SetVertices(v); if (mesh.normals.Length == v.Length) copy.SetNormals(mesh.normals); if (mesh.uv.Length == v.Length) copy.uv = mesh.uv; if (mesh.colors.Length == v.Length) copy.colors = mesh.colors;
                copy.SetTriangles(keep, 0); copy.RecalculateBounds(); copy.UploadMeshData(true);
                var go = new GameObject(mr.name + " (kept)", typeof(MeshFilter), typeof(MeshRenderer)); go.transform.SetParent(transform, false);
                go.transform.SetPositionAndRotation(mr.transform.position, mr.transform.rotation); go.transform.localScale = mr.transform.lossyScale;
                go.GetComponent<MeshFilter>().sharedMesh = copy; var r = go.GetComponent<MeshRenderer>(); r.sharedMaterial = mr.sharedMaterial; r.shadowCastingMode = mr.shadowCastingMode;
                hidden.Add(mr); shown.Add(go);
            }
        }
    }
}
