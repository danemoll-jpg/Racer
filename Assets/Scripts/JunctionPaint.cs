using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Racer
{
    // 0.80 Part A.2 (BUG-004/005): Hwy 92 lane paint as one clean layout. Paint only: no road, collider or route changes.
    // Two old paint sets overlapped at the side-road junctions:
    //  - the 0.6 set ("CR034-039 Woodland routes": amber median, white edges, broken lane lines) follows the Street Loop
    //    race line, so where the loop turns between Hwy 92 and Trickum Rd / S Cherokee Ln its lines curve across the lanes;
    //  - the CR113 set (double yellow, white lane dashes) follows Hwy 92 itself (the through route) west and east of them.
    // At load (every scene: the same world) this keeps the CR113 set and every old piece that lies on Hwy 92's own lanes,
    // hides the old pieces that are off them or duplicate CR113, repaints any stretch left bare (double yellow, dashes),
    // gives Hwy 92 continuous white edge lines (broken only at the two side-road mouths, turning into the side road with
    // a curve) and a stop line across each side road's lane into Hwy 92. The old median is drawn in the CR113 yellow.
    public sealed class JunctionPaint : MonoBehaviour
    {
        public const float Lane = 4.1f, Edge = 8.0f, Fillet = 9f;
        static readonly (Vector3 near, string side)[] Junctions = { (new Vector3(-605, 0, 540), StreetSigns.Trickum), (new Vector3(315, 0, 550), StreetSigns.Cherokee) };
        public readonly List<string> Report = new();
        public int Hidden, Recoloured, Kept;

        public static JunctionPaint Attach(GameObject host) => host.GetComponent<JunctionPaint>() ?? host.AddComponent<JunctionPaint>();
        void Start() => Build(gameObject.scene);

        public void Build(Scene scene)
        {
            var roots = scene.GetRootGameObjects();
            var through = roots.SelectMany(g => g.GetComponentsInChildren<RaceRoad>()).FirstOrDefault(r => r.openHighway && !r.forestTrail && r.points != null && r.points.Length > 1);
            var loop = CoursePreviewCatalog.Courses.FirstOrDefault(c => c.scene == "StreetLoopGreybox")?.main;
            var renderers = roots.SelectMany(g => g.GetComponentsInChildren<MeshRenderer>()).Where(r => r.enabled).ToList();
            var yellowR = renderers.FirstOrDefault(r => r.name.Contains("CR113") && r.name.Contains("double yellow"));
            var whiteR = renderers.FirstOrDefault(r => r.name.Contains("CR113") && r.name.Contains("white lane dash"));
            if (!through || loop == null || !yellowR || !whiteR) { Report.Add("no Hwy 92 through route or CR113 paint here"); return; }
            through.Initialize();
            Material yellow = yellowR.sharedMaterial, white = whiteR.sharedMaterial;
            // CR113 coverage: stations of the through route where its double yellow runs (vertices every ~2 m)
            var cover = new HashSet<int>();
            foreach (var r in renderers.Where(r => r.name.Contains("CR113") && r.name.Contains("double yellow")))
            {
                var f = r.GetComponent<MeshFilter>(); if (!f || !f.sharedMesh || !f.sharedMesh.isReadable) continue;
                foreach (var v in f.sharedMesh.vertices) { var p = r.transform.TransformPoint(v); float s = through.Project(p, out float lat); if (lat < 2) cover.Add(Mathf.RoundToInt(s / 2)); }
            }
            bool Covered(float s) => cover.Contains(Mathf.RoundToInt(s / 2)) || cover.Contains(Mathf.RoundToInt(s / 2) - 1) || cover.Contains(Mathf.RoundToInt(s / 2) + 1);
            // horizontal signed offset from Hwy 92's centre (right positive)
            float Signed(Vector3 p, out float s) { s = through.Project(p, out _); var c = through.At(s, out var fw); var d = p - c; d.y = 0; return Vector3.Dot(d, Right(Flat(fw))); }

            // the old 0.6 pieces: keep those on Hwy 92's own lines (median 0.12, lane 4.1, edge ~8.0) outside CR113 coverage
            var keptEdge = new HashSet<(int, int)>(); var keptCentre = new HashSet<int>();
            foreach (var r in renderers.Where(r => (r.name == "Highway edge / median" || r.name == "Broken lane line") && r.transform.parent && r.transform.parent.name.Contains("Woodland routes")))
            {
                var p = r.transform.position; float off = Signed(p, out float s); through.At(s, out var fw);
                // an "edge / median" piece belongs at 0.12 m or at the edge; a "broken lane line" at 4.1 m
                float a = Mathf.Abs(off), expected = r.name == "Broken lane line" ? Lane : a < 2 ? .12f : Edge;
                bool along = Vector3.Angle(Flat(r.transform.forward), Flat(fw)) < 4 || Vector3.Angle(Flat(r.transform.forward), -Flat(fw)) < 4;
                bool aligned = along && Mathf.Abs(a - expected) < (expected == Edge ? .5f : .3f);
                // a piece duplicates CR113 only where CR113 covers both of its ends (a piece half over the end of CR113 stays,
                // so the lines run on without a gap)
                float half = r.transform.lossyScale.z * .5f; var fwd = Flat(r.transform.forward);
                bool covered = Covered(through.Project(p + fwd * half, out _)) && Covered(through.Project(p - fwd * half, out _));
                if (!aligned || covered) { r.gameObject.SetActive(false); Hidden++; continue; }
                Kept++;
                if (expected == Edge) keptEdge.Add((Mathf.RoundToInt(s / 2), off > 0 ? 1 : -1)); else keptCentre.Add(Mathf.RoundToInt(s / 2));
                if (expected < 1 && r.sharedMaterial && r.sharedMaterial.name.Contains("amber")) { r.sharedMaterial = yellow; Recoloured++; }
            }
            bool Near(HashSet<int> set, float s, int span) { int k = Mathf.RoundToInt(s / 2); for (int i = -span; i <= span; i++) if (set.Contains(k + i)) return true; return false; }
            bool NearEdge(float s, int side) { int k = Mathf.RoundToInt(s / 2); for (int i = -2; i <= 2; i++) if (keptEdge.Contains((k + i, side))) return true; return false; }

            // side roads: centre line, direction into Hwy 92, half-width, the junction point and the mouth on Hwy 92
            var mouths = new List<(int side, float s0, float s1)>(); var extra = new List<(Vector3[] line, float half, Material m)>();
            var poly = new List<Vector3>(loop);
            foreach (var (near, name) in Junctions)
            {
                // loop points on the side road 12-50 m from Hwy 92's centre, near this junction
                var pts = poly.Where(p => Hz(p - near) < 90).Select(p => (p, d: Mathf.Abs(Signed(p, out _)))).Where(x => x.d > 14 && x.d < 50).ToList();
                if (pts.Count < 4) { Report.Add($"{name}: side road not found"); continue; }
                var nearP = pts.OrderBy(x => x.d).First().p; var farP = pts.OrderBy(x => x.d).Last().p;
                var din = Flat(nearP - farP);
                var probe = farP + din * (Hz(nearP - farP) * .5f);
                var (shift, half) = Paved(probe, din); var nrm = Right(din); var c = probe + nrm * shift;
                // walk the side centre line toward Hwy 92 until it reaches the through route's centre
                var J = c; for (int i = 0; i < 400; i++) { if (Mathf.Abs(Signed(J, out _)) < .25f) break; J += din * .25f; }
                float sJ = through.Project(J, out _); var tJ = Flat(Dir(through, sJ)); var m = Flat(-(din - tJ * Vector3.Dot(din, tJ))); // Hwy normal toward the side road
                int hwySide = Vector3.Dot(m, Right(tJ)) > 0 ? 1 : -1;
                float sMin = float.MaxValue, sMax = float.MinValue;
                foreach (int sd in new[] { -1, 1 })
                {
                    // corner curve between Hwy 92's edge (offset Edge toward the side road) and this side edge of the side road
                    Vector2 A0 = V2(J + m * (Edge + Fillet)), Ad = V2(tJ), B0 = V2(c + nrm * sd * (half - .2f + Fillet)), Bd = V2(din);
                    if (!Cross(A0, Ad, B0, Bd, out var C2)) continue;
                    // the centre must be on the outer side of both lines (away from both roads)
                    var C = new Vector3(C2.x, J.y, C2.y); J.y = through.At(through.Project(J, out _), out _).y;
                    var tA = C - m * Fillet; var foot = c + nrm * sd * (half - .2f); var tB = foot + din * Vector3.Dot(C - foot, din);
                    var arc = new List<Vector3>(); float a0 = Mathf.Atan2(tA.z - C.z, tA.x - C.x), a1 = Mathf.Atan2(tB.z - C.z, tB.x - C.x);
                    float da = Mathf.DeltaAngle(a0 * Mathf.Rad2Deg, a1 * Mathf.Rad2Deg) * Mathf.Deg2Rad;
                    for (int i = 0; i <= 12; i++) { float ang = a0 + da * i / 12f; arc.Add(new Vector3(C.x + Mathf.Cos(ang) * Fillet, J.y, C.z + Mathf.Sin(ang) * Fillet)); }
                    for (float back = 1; back <= 30; back += 1) arc.Add(tB - din * back);
                    extra.Add((arc.ToArray(), .06f, white));
                    float st = through.Project(tA, out _); sMin = Mathf.Min(sMin, st); sMax = Mathf.Max(sMax, st);
                }
                mouths.Add((hwySide, sMin, sMax));
                // stop line across the side road's lane into Hwy 92 (the right-hand half, facing Hwy 92), 2 m before the edge
                float back2 = Mathf.Abs(Vector3.Dot(J - c, din)) - Edge - 2f; var sl = c + din * back2;
                extra.Add((new[] { sl + nrm * .1f, sl + nrm * (half - .4f) }, .22f, white));
                Report.Add($"{name} / {StreetSigns.Hwy92}: junction ({J.x:F1}, {J.z:F1}), side road half-width {half:F1} m, mouth stations {sMin:F0}-{sMax:F0}, stop line at ({sl.x:F1}, {sl.z:F1})");
            }

            // the terrain's own two-lane markings (Racer/MarkedGround: edge lines at 3.65 m and centre dashes, drawn from the
            // vertices' road coordinates) also show on Hwy 92 where no highway mesh covers the terrain; they are switched off
            // on Hwy 92's pavement (within 8.6 m of its centre) in a copy of the drawn mesh (the collider keeps its mesh)
            int unmarked = 0;
            foreach (var r in renderers.Where(r => r.sharedMaterial && r.sharedMaterial.shader && r.sharedMaterial.shader.name == "Racer/MarkedGround"))
            {
                var f = r.GetComponent<MeshFilter>(); if (!f || !f.sharedMesh || !f.sharedMesh.isReadable) continue;
                var road = new List<Vector4>(); f.sharedMesh.GetUVs(1, road); if (road.Count != f.sharedMesh.vertexCount) continue;
                var verts = f.sharedMesh.vertices; bool changed = false;
                for (int i = 0; i < verts.Length; i++)
                {
                    if (road[i].z <= 0 && road[i].w <= 0) continue;
                    var p = r.transform.TransformPoint(verts[i]); float off = Signed(p, out float st); var c = through.At(st, out _);
                    if (Mathf.Abs(off) > Edge + .6f || Mathf.Abs(p.y - c.y) > 2 || st <= .5f || st >= through.Length - .5f) continue;
                    var q = road[i]; q.z = 0; q.w = 0; road[i] = q; changed = true; unmarked++;
                }
                if (!changed) continue;
                var copy = Instantiate(f.sharedMesh); copy.name = f.sharedMesh.name + " (0.80 Hwy 92 unmarked)"; copy.SetUVs(1, road); f.sharedMesh = copy;
            }
            Report.Add($"terrain markings switched off on Hwy 92's pavement at {unmarked} vertices");

            // new paint along the through route
            var lines = new List<(List<Vector3> pts, float half, Material m)>();
            void Run(List<Vector3> cur, float half, Material m) { if (cur.Count > 1) lines.Add((new List<Vector3>(cur), half, m)); cur.Clear(); }
            var edge = new[] { new List<Vector3>(), new List<Vector3>() }; var centreL = new[] { new List<Vector3>(), new List<Vector3>() };
            int dashes = 0;
            for (float s = 0; s <= through.Length; s += 2)
            {
                var p = through.At(s, out var fw); var rt = Right(Flat(fw));
                for (int k = 0; k < 2; k++)
                {
                    int sd = k == 0 ? -1 : 1;
                    bool mouth = mouths.Any(mo => mo.side == sd && s > mo.s0 && s < mo.s1);
                    if (mouth || NearEdge(s, sd)) Run(edge[k], .06f, white); else edge[k].Add(p + rt * sd * Edge);
                }
                bool bare = !Covered(s) && !Near(keptCentre, s, 3);
                for (int k = 0; k < 2; k++) { if (bare) centreL[k].Add(p + rt * (k == 0 ? -.16f : .16f)); else Run(centreL[k], .065f, yellow); }
                if (bare && Mathf.RoundToInt(s / 2) % 7 == 0)
                    foreach (int sd in new[] { -1, 1 }) { var q0 = p + rt * sd * Lane; var q1 = through.At(s + 4, out _) + rt * sd * Lane; lines.Add((new List<Vector3> { q0, Vector3.Lerp(q0, q1, .5f), q1 }, .075f, white)); dashes++; }
            }
            for (int k = 0; k < 2; k++) { Run(edge[k], .06f, white); Run(centreL[k], .065f, yellow); }
            foreach (var (l, h, m) in extra) lines.Add((l.ToList(), h, m));
            var root = new GameObject("Hwy 92 junction paint (0.80)"); SceneManager.MoveGameObjectToScene(root, scene);
            foreach (var group in lines.GroupBy(l => l.m))
            {
                var v = new List<Vector3>(); var t = new List<int>(); var n = new List<Vector3>();
                foreach (var (pts, half, _) in group) Ribbon(pts, half, v, t, n);
                var mesh = new Mesh { name = "Hwy 92 junction paint", indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 }; mesh.SetVertices(v); mesh.SetNormals(n); mesh.SetTriangles(t, 0); mesh.RecalculateBounds();
                var go = new GameObject("Paint " + group.Key.name, typeof(MeshFilter), typeof(MeshRenderer)); go.transform.SetParent(root.transform, false);
                go.GetComponent<MeshFilter>().sharedMesh = mesh; var mr = go.GetComponent<MeshRenderer>(); mr.sharedMaterial = group.Key; mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            foreach (var (pts, half, m) in lines) Report.Add($"line {m.name} half {half:F3}: {pts.Count} points ({pts[0].x:F1}, {pts[0].z:F1}) -> ({pts[^1].x:F1}, {pts[^1].z:F1})");
            Report.Add($"old 0.6 pieces: kept {Kept} on Hwy 92's own lines, hid {Hidden} off them or duplicating CR113, recoloured {Recoloured} median pieces yellow; new: {lines.Count} lines, {dashes} lane dashes");
        }

        static Vector3 Flat(Vector3 v) { v.y = 0; return v.normalized; }
        static float Hz(Vector3 v) => new Vector2(v.x, v.z).magnitude;
        static Vector3 Right(Vector3 f) => Vector3.Cross(Vector3.up, f).normalized;
        static Vector2 V2(Vector3 v) => new(v.x, v.z);
        static Vector3 Dir(RaceRoad r, float s) { r.At(s, out var f); return f; }
        static bool Cross(Vector2 p, Vector2 d, Vector2 q, Vector2 e, out Vector2 x)
        {
            float den = d.x * e.y - d.y * e.x; x = default; if (Mathf.Abs(den) < .15f) return false;
            float t = ((q.x - p.x) * e.y - (q.y - p.y) * e.x) / den; x = p + d * t; return true;
        }
        // the paved surface across a road from a point on it: its centre's offset (along the right normal) and half-width
        static (float shift, float half) Paved(Vector3 at, Vector3 along)
        {
            var n = Right(along); float right = 0, left = 0;
            for (float o = 0; o < 20; o += .25f) { right = o; if (!Paved(at + n * o)) break; }
            for (float o = 0; o < 20; o += .25f) { left = o; if (!Paved(at - n * o)) break; }
            return ((right - left) * .5f, Mathf.Max(3f, (right + left) * .5f));
        }
        static bool Paved(Vector3 p) => Ground(p, out var y) && SceneryTrees.OnDrivable(new Vector3(p.x, y + .05f, p.z), null, out _);
        static bool Ground(Vector3 p, out float y, float above = 60, float depth = 200)
        {
            y = 0; float best = float.MaxValue;
            foreach (var h in Physics.RaycastAll(new Vector3(p.x, p.y + above, p.z), Vector3.down, depth, ~0, QueryTriggerInteraction.Ignore))
                if (!(h.collider.attachedRigidbody && !h.collider.attachedRigidbody.isKinematic) && h.distance < best) { best = h.distance; y = h.point.y; }
            return best < float.MaxValue;
        }
        // a flat strip along the points, 3 cm above the road
        static void Ribbon(List<Vector3> pts, float half, List<Vector3> v, List<int> t, List<Vector3> n)
        {
            int start = v.Count;
            for (int i = 0; i < pts.Count; i++)
            {
                var f = Flat(pts[Mathf.Min(i + 1, pts.Count - 1)] - pts[Mathf.Max(i - 1, 0)]); var side = Right(f) * half;
                var p = pts[i]; if (Ground(p, out var y, 2.5f, 6)) p.y = y; p.y += .03f;
                v.Add(p - side); v.Add(p + side); n.Add(Vector3.up); n.Add(Vector3.up);
                if (i > 0) { int k = start + (i - 1) * 2; t.AddRange(new[] { k, k + 2, k + 1, k + 1, k + 2, k + 3 }); }
            }
        }
    }
}
