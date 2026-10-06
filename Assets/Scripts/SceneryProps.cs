using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Racer
{
    // 0.78 Part A2 and A4: rocks and small props. Their meshes are swapped IN PLACE on their own renderers (same
    // object, same material, same collider), so breakable fences, signs and mailboxes break and come back exactly as
    // before, and Classic swaps the old meshes back.
    //  - Rocks, boulders, outcrops, cairns and fire-ring stones become faceted, weathered rocks: the old shape pushed
    //    OUTWARD only (never inward, at most 12 cm), so the new rock always covers the old rock's collider; flat-shaded.
    //    Cave, vault and portal rock (enclosing a road or a cave) is left as it is.
    //  - Boards, posts, rails, fence sections, mailboxes and the like built from plain boxes get clean bevelled edges
    //    (inside the same box): the same shape and text, cleaner.
    public sealed class SceneryProps : MonoBehaviour
    {
        public int Rocks { get; private set; }
        public int Bevelled { get; private set; }
        readonly List<(MeshFilter f, Mesh classic, Mesh fresh)> swaps = new();
        public IEnumerable<MeshFilter> Swapped => swaps.Select(x => x.f);
        static readonly Dictionary<(Mesh, Vector3Int), Mesh> bevelled = new();
        static readonly string[] RockWords = { "rock", "boulder", "outcrop", "stone", "cairn" };
        static readonly string[] NotRock = { "vault", "portal", "cave", "enclosed", "sign", "letter", "wall", "rocky way", "acres", "step", "edge stone", "coping" };
        static readonly string[] PropWords = { "sign board", "sign post", "post", "board", "rail", "mailbox", "crossbuck", "barricade", "bench", "table", "fence", "gate", "picket", "beam", "plank" };
        static readonly string[] NotProp = { "treehouse", "ramp", "landing", "deck", "bridge", "road", "lane", "ground", "trail", "porch", "stair", "step" };

        public void SetShown(bool on)
        {
            foreach (var (f, classic, fresh) in swaps) if (f) f.sharedMesh = on ? fresh : classic;
            foreach (var (r, classic, fresh) in materialSwaps) if (r) r.sharedMaterials = on ? fresh : classic;
        }
        readonly List<(Renderer r, Material[] classic, Material[] fresh)> materialSwaps = new();
        public int Fences { get; private set; }
        public int FencePostsGrounded { get; private set; }

        static bool Has(string n, string[] words) => words.Any(w => n.IndexOf(w, System.StringComparison.OrdinalIgnoreCase) >= 0);
        public void Build(Scene scene, HashSet<Renderer> skip)
        {
            foreach (var r in scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<MeshRenderer>(true)))
            {
                if (skip.Contains(r) || !r.TryGetComponent<MeshFilter>(out var f) || !f.sharedMesh || !f.sharedMesh.isReadable) continue;
                string n = r.name; var mesh = f.sharedMesh;
                if (r.GetComponentInParent<SceneryBuildings>() || r.GetComponentInParent<RaceGate>() || r.GetComponentInParent<ArcadeVehicle>()) continue;
                if (Has(n, CaveWords) && Has(n, CaveRockWords) && !Has(n, NotCaveRock) && !r.GetComponentInChildren<Collider>(true) && !r.GetComponentInParent<Collider>() && mesh.vertexCount < 6000)
                {
                    var fresh = CaveRock(mesh, r.transform, Mathf.Abs(Mathf.Sin(r.transform.position.x * .37f + r.transform.position.z * .71f)));
                    swaps.Add((f, mesh, fresh)); CaveRocks++;
                }
                else if (Has(n, RockWords) && !Has(n, NotRock) && mesh.vertexCount < 6000)
                {
                    var fresh = Rock(mesh, r.transform, Mathf.Abs(Mathf.Sin(r.transform.position.x * .37f + r.transform.position.z * .71f)), Cairn(r.transform));
                    swaps.Add((f, mesh, fresh)); Rocks++;
                }
                else if (Has(n, FenceWords) && !Has(n, NotFence) && mesh.vertexCount > 24 && mesh.vertexCount % 24 == 0 && Fence(r, f, mesh)) { }
                else if (Has(n, PropWords) && !Has(n, NotProp) && mesh.vertexCount == 24 && (mesh.name.StartsWith("Cube") || mesh.bounds.size.x > 0))
                {
                    var sc = r.transform.lossyScale; var key = (mesh, new Vector3Int(Mathf.RoundToInt(sc.x * 50), Mathf.RoundToInt(sc.y * 50), Mathf.RoundToInt(sc.z * 50)));
                    Mesh fresh;
                    // 0.79 Part D: a post on a slope reaches the ground at its lowest corner (its own, longer mesh)
                    float down = n.IndexOf("post", System.StringComparison.OrdinalIgnoreCase) >= 0 ? PostGap(r) : 0;
                    if (down > .01f) { fresh = Bevel(mesh, sc, (down + .05f) / Mathf.Max(1e-4f, Mathf.Abs(sc.y))); PostsGrounded++; }
                    else if (!bevelled.TryGetValue(key, out fresh) || !fresh) bevelled[key] = fresh = Bevel(mesh, sc);
                    swaps.Add((f, mesh, fresh)); Bevelled++;
                }
            }
            SeatCairns();
        }

        // 0.84 Part E: fence sections (one merged mesh of plain boxes each: the wood crossbucks with their X braces, the
        // neighbours' board fence, the roadside chain-link) get the same treatment as the other props: every box rebuilt with
        // chamfered edges inside its own outline; posts (upright boxes) reach the ground under them (no daylight under a post
        // on a slope); wooden fences get three slightly different wood tones, piece by piece. The lines, heights, rails and
        // braces stay where they are, and the collider is not touched (visual only; Classic shows the original mesh). A mesh
        // whose blocks are not all plain boxes is left as it is.
        static readonly string[] FenceWords = { "fence", "crossbuck", "chain-link", "railing" };
        static readonly string[] NotFence = { "treehouse", "ramp", "landing", "deck", "bridge", "porch", "stair", "step" }; // ("Grounded ...", "roadside ..." fences are fences)
        static readonly Dictionary<(Material, int), Material> tints = new();
        static Material Tint(Material m, int k)
        {
            if (k == 0 || !m) return m;
            if (tints.TryGetValue((m, k), out var t) && t) return t;
            t = new Material(m) { name = m.name + (k == 1 ? " (weathered)" : " (fresh)") };
            var mul = k == 1 ? new Color(.90f, .89f, .86f) : new Color(1.04f, 1.02f, .97f);
            if (t.HasProperty("_BaseColor")) t.SetColor("_BaseColor", t.GetColor("_BaseColor") * mul);
            if (t.HasProperty("_Color")) t.SetColor("_Color", t.GetColor("_Color") * mul);
            return tints[(m, k)] = t;
        }
        bool Fence(Renderer r, MeshFilter f, Mesh mesh)
        {
            if (!mesh.isReadable || mesh.subMeshCount != 1) return false;
            var v = mesh.vertices; var tr = r.transform; int boxes = v.Length / 24;
            var parts = new List<(Vector3 c, Vector3[] ax)>();
            for (int b = 0; b < boxes; b++) { if (!Box(v, b * 24, out var c, out var ax)) return false; parts.Add((c, ax)); }
            bool wood = r.sharedMaterial && r.sharedMaterial.name.IndexOf("galvan", System.StringComparison.OrdinalIgnoreCase) < 0;
            var verts = new List<Vector3>(); var normals = new List<Vector3>(); var subs = new[] { new List<int>(), new List<int>(), new List<int>() };
            foreach (var (c0, ax0) in parts)
            {
                var c = c0; var ax = (Vector3[])ax0.Clone();
                // the longest axis; an upright one makes this box a post
                int l = 0; for (int i = 1; i < 3; i++) if (ax[i].magnitude > ax[l].magnitude) l = i;
                var up = tr.TransformVector(ax[l]);
                bool post = Mathf.Abs(up.normalized.y) > .9f && up.magnitude > Mathf.Max(tr.TransformVector(ax[(l + 1) % 3]).magnitude, tr.TransformVector(ax[(l + 2) % 3]).magnitude) * 1.8f;
                if (post)
                {
                    var down = up.y > 0 ? -ax[l] : ax[l]; var bottom = tr.TransformPoint(c + down); var top = tr.TransformPoint(c - down);
                    float ground = float.MinValue;
                    foreach (var hit in Physics.RaycastAll(top + Vector3.up * .5f, Vector3.down, (top - bottom).magnitude + 3, ~0, QueryTriggerInteraction.Ignore))
                        if (!hit.collider.transform.IsChildOf(tr) && hit.collider.transform != tr.parent && !hit.collider.GetComponentInParent<BreakableProp>() && !(hit.collider.attachedRigidbody && !hit.collider.attachedRigidbody.isKinematic) && hit.point.y < bottom.y + .3f) ground = Mathf.Max(ground, hit.point.y);
                    float gap = ground == float.MinValue ? 0 : bottom.y - ground;
                    if (gap > .02f && gap < 1.5f)
                    {
                        // lengthen the post downward to 5 cm into the ground (mesh units along its own axis)
                        float extra = (gap + .05f) / Mathf.Max(1e-4f, Mathf.Abs(tr.TransformVector(down.normalized).y));
                        c += down.normalized * extra * .5f; ax[l] = ax[l].normalized * (ax[l].magnitude + extra * .5f); FencePostsGrounded++;
                    }
                }
                int k = wood ? Mathf.Abs(Mathf.RoundToInt(Mathf.Sin(c0.x * 12.9898f + c0.y * 78.233f + c0.z * 37.719f) * 43758.5453f)) % 3 : 0;
                Chamfer(verts, normals, subs[k], c, ax, tr);
            }
            var fresh = new Mesh { name = mesh.name + " (0.84 fence)" };
            if (verts.Count > 65000) fresh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            fresh.SetVertices(verts); fresh.SetNormals(normals);
            var used = Enumerable.Range(0, 3).Where(i => subs[i].Count > 0).ToArray(); fresh.subMeshCount = used.Length;
            for (int i = 0; i < used.Length; i++) fresh.SetTriangles(subs[used[i]], i);
            fresh.RecalculateBounds(); fresh.UploadMeshData(false);
            var classic = r.sharedMaterials; var mats = used.Select(i => Tint(classic[0], i)).ToArray();
            swaps.Add((f, mesh, fresh)); if (used.Length > 1 || used[0] != 0) materialSwaps.Add((r, classic, mats)); Fences++;
            return true;
        }
        // the 24 vertices of one plain box: its centre and three half-axis vectors (mesh space)
        static bool Box(Vector3[] v, int at, out Vector3 c, out Vector3[] ax)
        {
            c = default; ax = null; var pts = new List<Vector3>();
            for (int i = at; i < at + 24; i++) if (!pts.Any(q => (q - v[i]).sqrMagnitude < 1e-8f)) pts.Add(v[i]);
            if (pts.Count != 8) return false;
            c = pts.Aggregate(Vector3.zero, (a, b) => a + b) / 8;
            var d = pts.Skip(1).Select(q => q - pts[0]).ToArray();
            for (int i = 0; i < 7; i++) for (int j = i + 1; j < 7; j++) for (int k = j + 1; k < 7; k++)
                    {
                        var a = d[i]; var b = d[j]; var e = d[k]; float la = a.magnitude, lb = b.magnitude, le = e.magnitude;
                        if (la < 1e-5f || lb < 1e-5f || le < 1e-5f) continue;
                        if (Mathf.Abs(Vector3.Dot(a, b)) > 1e-3f * la * lb || Mathf.Abs(Vector3.Dot(a, e)) > 1e-3f * la * le || Mathf.Abs(Vector3.Dot(b, e)) > 1e-3f * lb * le) continue;
                        if (((pts[0] + a + b + e) - (2 * c - pts[0])).sqrMagnitude > 1e-6f) continue;
                        ax = new[] { a * .5f, b * .5f, e * .5f }; return true;
                    }
            return false;
        }
        // a box with its edges chamfered (inside the original outline), flat shaded
        static void Chamfer(List<Vector3> verts, List<Vector3> normals, List<int> tris, Vector3 c, Vector3[] ax, Transform tr)
        {
            var u = ax.Select(a => a.normalized).ToArray(); var h = ax.Select(a => a.magnitude).ToArray();
            // about 1.2 cm in the world, never more than a quarter of the thinnest side
            float scale = Mathf.Max(1e-4f, tr.lossyScale.magnitude / 1.732f), bv = Mathf.Min(.012f / scale, h.Min() * .25f);
            Vector3 P(float x, float y, float z) => c + u[0] * x + u[1] * y + u[2] * z;
            void Poly(Vector3 n, params Vector3[] p)
            {
                int b = verts.Count; verts.AddRange(p); for (int i = 0; i < p.Length; i++) normals.Add(n);
                var cross = Vector3.Cross(p[1] - p[0], p[2] - p[0]); bool flip = Vector3.Dot(cross, n) < 0;
                for (int i = 1; i + 1 < p.Length; i++) { tris.Add(b); tris.Add(flip ? b + i + 1 : b + i); tris.Add(flip ? b + i : b + i + 1); }
            }
            for (int i = 0; i < 3; i++) for (int s = -1; s <= 1; s += 2)
                {
                    int j = (i + 1) % 3, k = (i + 2) % 3; var n = u[i] * s;
                    Vector3 Q(float a, float b) { var x = new float[3]; x[i] = s * h[i]; x[j] = a * (h[j] - bv); x[k] = b * (h[k] - bv); return P(x[0], x[1], x[2]); }
                    Poly(n, Q(-1, -1), Q(1, -1), Q(1, 1), Q(-1, 1));
                    // the chamfer strips between this face and the faces of axis j (each of the 12 edges exactly once)
                    for (int t = -1; t <= 1; t += 2)
                    {
                        Vector3 E(float b, bool onI) { var x = new float[3]; x[i] = s * (onI ? h[i] : h[i] - bv); x[j] = t * (onI ? h[j] - bv : h[j]); x[k] = b * (h[k] - bv); return P(x[0], x[1], x[2]); }
                        Poly((u[i] * s + u[j] * t).normalized, E(-1, true), E(1, true), E(1, false), E(-1, false));
                    }
                }
            for (int x = -1; x <= 1; x += 2) for (int y = -1; y <= 1; y += 2) for (int z = -1; z <= 1; z += 2)
                        Poly((u[0] * x + u[1] * y + u[2] * z).normalized, P(x * h[0], y * (h[1] - bv), z * (h[2] - bv)), P(x * (h[0] - bv), y * h[1], z * (h[2] - bv)), P(x * (h[0] - bv), y * (h[1] - bv), z * h[2]));
        }

        // 0.79 Part D: a clue cairn (three stacked stones, no collider) whose ground was raised or lowered after it was
        // placed is drawn sitting on today's ground: the whole stack moves together, its bottom 2 cm into the ground.
        public int CairnsSeated { get; private set; }
        void SeatCairns()
        {
            foreach (var group in swaps.Select((s, i) => (s, i)).Where(x => x.s.f && Cairn(x.s.f.transform) && x.s.fresh.isReadable).GroupBy(x => x.s.f.transform.parent).ToList())
            {
                var clue = group.Key; float bottom = float.MaxValue;
                foreach (var (s, _) in group) { var m = s.fresh; var t = s.f.transform; foreach (var v in m.vertices) bottom = Mathf.Min(bottom, t.TransformPoint(v).y); }
                float ground = float.MinValue;
                foreach (var h in Physics.RaycastAll(clue.position + Vector3.up * 12, Vector3.down, 24, ~0, QueryTriggerInteraction.Ignore).OrderBy(h => h.distance))
                    if (!h.collider.transform.IsChildOf(clue) && !(h.collider.attachedRigidbody && !h.collider.attachedRigidbody.isKinematic)) { ground = h.point.y; break; }
                float dy = ground == float.MinValue ? 0 : ground - .02f - bottom; bool seat = Mathf.Abs(dy) >= .03f;
                foreach (var (s, _) in group)
                {
                    var m = s.fresh;
                    if (seat) { var shift = s.f.transform.InverseTransformVector(Vector3.up * dy); var v = m.vertices; for (int k = 0; k < v.Length; k++) v[k] += shift; m.vertices = v; m.RecalculateBounds(); m.name += " (seated)"; }
                    m.UploadMeshData(true);
                }
                if (seat) CairnsSeated++;
            }
        }

        // A faceted rock: vertices welded by position, pushed outward along their normal by a little noise (never inward),
        // then flat-shaded (each face its own vertices).
        static bool Cairn(Transform t) => t.parent && t.parent.name.StartsWith("Acorn clue / ");
        static Mesh Rock(Mesh old, Transform t, float seed, bool keepReadable = false)
        {
            var v = old.vertices; var nrm = old.normals; var tri = old.triangles; var b = old.bounds;
            var scale = t.lossyScale; float world = Mathf.Min(Mathf.Abs(b.size.x * scale.x), Mathf.Abs(b.size.y * scale.y), Mathf.Abs(b.size.z * scale.z));
            float push = Mathf.Min(.12f, world * .07f);
            // welded positions share one offset so the surface stays closed
            var offset = new Dictionary<Vector3, Vector3>();
            var moved = new Vector3[v.Length];
            for (int i = 0; i < v.Length; i++)
            {
                var key = new Vector3(Mathf.Round(v[i].x * 1000), Mathf.Round(v[i].y * 1000), Mathf.Round(v[i].z * 1000));
                if (!offset.TryGetValue(key, out var o))
                {
                    var n = nrm.Length == v.Length ? nrm[i] : (v[i] - b.center).normalized;
                    // world-space amount, converted back to the mesh's (possibly scaled) space
                    float k = push * (.35f + .65f * Mathf.PerlinNoise(v[i].x * 3.3f + seed * 10, v[i].z * 3.3f + v[i].y * 2.1f));
                    var wn = Vector3.Scale(n, new Vector3(1 / Mathf.Max(1e-4f, Mathf.Abs(scale.x)), 1 / Mathf.Max(1e-4f, Mathf.Abs(scale.y)), 1 / Mathf.Max(1e-4f, Mathf.Abs(scale.z))));
                    o = wn.normalized * k / Mathf.Max(1e-4f, Vector3.Scale(wn.normalized, scale).magnitude);
                    offset[key] = o;
                }
                moved[i] = v[i] + o;
            }
            var fv = new List<Vector3>(tri.Length); var ft = new List<int>(tri.Length);
            for (int i = 0; i < tri.Length; i++) { fv.Add(moved[tri[i]]); ft.Add(i); }
            var m = new Mesh { name = old.name + " (faceted rock)", indexFormat = fv.Count > 65000 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16 };
            m.SetVertices(fv); m.SetTriangles(ft, 0); m.RecalculateNormals(); m.RecalculateBounds(); m.UploadMeshData(!keepReadable);
            return m;
        }

        // 0.84 Part F: the cave decoration rock (fallen stones, hanging formations, buttresses, shoulder outcrops, ceiling stones)
        // was left as it was in 0.78. Only pieces with no collider of their own are restyled, drawn in place of the old mesh
        // (Scenery: New; Classic shows the original): the same faceted look, but every point is pulled INWARD toward the
        // piece's centre (never outward), at most 12 cm, so the new rock stays inside the old outline: it cannot narrow an
        // opening or hang further into the driving space. Cave colliders, triggers, routes, floors, the rock vault, the portal
        // outcrops and the collidable cave-edge boulders are not touched.
        public int CaveRocks { get; private set; }
        static readonly string[] CaveWords = { "cave" };
        static readonly string[] CaveRockWords = { "stone", "formation", "buttress", "outcrop", "slab", "boulder" };
        static readonly string[] NotCaveRock = { "patch", "puddle", "gravel", "root", "sign", "post", "board", "floor", "vault", "portal" };
        static Mesh CaveRock(Mesh old, Transform t, float seed)
        {
            var v = old.vertices; var tri = old.triangles; var b = old.bounds; var scale = t.lossyScale;
            float world = Mathf.Min(Mathf.Abs(b.size.x * scale.x), Mathf.Abs(b.size.y * scale.y), Mathf.Abs(b.size.z * scale.z));
            float pull = Mathf.Min(.12f, world * .12f);
            var offset = new Dictionary<Vector3, Vector3>(); var moved = new Vector3[v.Length];
            for (int i = 0; i < v.Length; i++)
            {
                var key = new Vector3(Mathf.Round(v[i].x * 1000), Mathf.Round(v[i].y * 1000), Mathf.Round(v[i].z * 1000));
                if (!offset.TryGetValue(key, out var o))
                {
                    var toCentre = b.center - v[i]; var worldToCentre = Vector3.Scale(toCentre, scale); float dist = worldToCentre.magnitude;
                    float k = pull * (.25f + .75f * Mathf.PerlinNoise(v[i].x * 3.3f + seed * 10, v[i].z * 3.3f + v[i].y * 2.1f));
                    o = dist > 1e-4f ? toCentre * Mathf.Min(.45f, k / dist) : Vector3.zero;
                    offset[key] = o;
                }
                moved[i] = v[i] + o;
            }
            var fv = new List<Vector3>(tri.Length); var ft = new List<int>(tri.Length);
            for (int i = 0; i < tri.Length; i++) { fv.Add(moved[tri[i]]); ft.Add(i); }
            var m = new Mesh { name = old.name + " (faceted cave rock)", indexFormat = fv.Count > 65000 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16 };
            m.SetVertices(fv); m.SetTriangles(ft, 0); m.RecalculateNormals(); m.RecalculateBounds(); m.UploadMeshData(true);
            return m;
        }

        public int PostsGrounded { get; private set; }
        // how far an upright post's bottom stands above the lowest ground under its four bottom corners
        static float PostGap(Renderer r)
        {
            if (Vector3.Dot(r.transform.up, Vector3.up) < .95f) return 0;
            var b = r.bounds; float lo = float.MaxValue;
            foreach (var (x, z) in new[] { (b.min.x, b.min.z), (b.max.x, b.min.z), (b.min.x, b.max.z), (b.max.x, b.max.z) })
                foreach (var h in Physics.RaycastAll(new Vector3(x, b.max.y + 1, z), Vector3.down, b.size.y + 4, ~0, QueryTriggerInteraction.Ignore))
                    if (!h.collider.transform.IsChildOf(r.transform) && h.collider.transform != r.transform.parent && !(h.collider.attachedRigidbody && !h.collider.attachedRigidbody.isKinematic) && h.point.y < b.min.y + .3f) { lo = Mathf.Min(lo, h.point.y); }
            return lo == float.MaxValue ? 0 : Mathf.Clamp(b.min.y - lo, 0, 1.5f);
        }
        // A box mesh with bevelled edges inside the original box (a plain 24-vertex box / Unity cube); extraDown lengthens
        // it downward (mesh units).
        static Mesh Bevel(Mesh old, Vector3 scale, float extraDown = 0)
        {
            var b = old.bounds; b.min -= new Vector3(0, extraDown, 0); var s = Vector3.Scale(b.size, new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z)));
            float bevel = Mathf.Min(.035f, Mathf.Min(s.x, Mathf.Min(s.y, s.z)) * .18f);
            var e = new Vector3(bevel / Mathf.Max(1e-4f, Mathf.Abs(scale.x)), bevel / Mathf.Max(1e-4f, Mathf.Abs(scale.y)), bevel / Mathf.Max(1e-4f, Mathf.Abs(scale.z)));
            Vector3 lo = b.min, hi = b.max; var pts = new List<Vector3>();
            // the 24 corner points of a chamfered box
            foreach (int sx in new[] { -1, 1 }) foreach (int sy in new[] { -1, 1 }) foreach (int sz in new[] { -1, 1 })
                    {
                        var c = new Vector3(sx < 0 ? lo.x : hi.x, sy < 0 ? lo.y : hi.y, sz < 0 ? lo.z : hi.z);
                        pts.Add(c - new Vector3(sx * e.x, 0, 0)); pts.Add(c - new Vector3(0, sy * e.y, 0)); pts.Add(c - new Vector3(0, 0, sz * e.z));
                    }
            var hull = Hull(pts);
            var v = new List<Vector3>(); var t = new List<int>();
            foreach (var (a, c, d) in hull) { int i = v.Count; v.Add(a); v.Add(c); v.Add(d); t.Add(i); t.Add(i + 1); t.Add(i + 2); }
            var m = new Mesh { name = old.name + " (bevelled)" }; m.SetVertices(v); m.SetTriangles(t, 0); m.RecalculateNormals(); m.RecalculateBounds();
            var uv = v.Select(p => new Vector2(p.x + p.z, p.y)).ToList(); m.SetUVs(0, uv); m.UploadMeshData(true);
            return m;
        }
        // Convex hull of a small point set (brute force: every triangle of points with all others on one side).
        static List<(Vector3, Vector3, Vector3)> Hull(List<Vector3> p)
        {
            var o = new List<(Vector3, Vector3, Vector3)>(); var centre = p.Aggregate(Vector3.zero, (a, b) => a + b) / p.Count;
            var faces = new HashSet<string>();
            for (int i = 0; i < p.Count; i++) for (int j = i + 1; j < p.Count; j++) for (int k = j + 1; k < p.Count; k++)
                    {
                        var n = Vector3.Cross(p[j] - p[i], p[k] - p[i]); if (n.sqrMagnitude < 1e-14f) continue; n.Normalize();
                        float d = Vector3.Dot(n, p[i]); bool pos = false, neg = false;
                        for (int q = 0; q < p.Count && !(pos && neg); q++) { float x = Vector3.Dot(n, p[q]) - d; if (x > 1e-6f) pos = true; else if (x < -1e-6f) neg = true; }
                        if (pos && neg) continue;
                        if (Vector3.Dot(n, centre) - d > 0) n = -n;
                        // one triangle fan per plane: collect all coplanar points, sort round, fan
                        string key = $"{Mathf.Round(n.x * 1000)},{Mathf.Round(n.y * 1000)},{Mathf.Round(n.z * 1000)}"; if (!faces.Add(key)) continue;
                        float dd = Vector3.Dot(n, p[i]); var on = p.Where(q => Mathf.Abs(Vector3.Dot(n, q) - dd) < 1e-5f).Distinct().ToList();
                        var c = on.Aggregate(Vector3.zero, (a, b) => a + b) / on.Count; var u = (on[0] - c).normalized; var w = Vector3.Cross(n, u);
                        on = on.OrderBy(q => Mathf.Atan2(Vector3.Dot(q - c, w), Vector3.Dot(q - c, u))).ToList();
                        for (int f = 1; f + 1 < on.Count; f++) o.Add((on[0], on[f], on[f + 1]));
                    }
            return o;
        }
    }
}
