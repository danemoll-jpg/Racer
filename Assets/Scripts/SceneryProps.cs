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
        static readonly Dictionary<(Mesh, Vector3Int), Mesh> bevelled = new();
        static readonly string[] RockWords = { "rock", "boulder", "outcrop", "stone", "cairn" };
        static readonly string[] NotRock = { "vault", "portal", "cave", "enclosed", "sign", "letter", "wall", "rocky way", "acres", "step", "edge stone", "coping" };
        static readonly string[] PropWords = { "sign board", "sign post", "post", "board", "rail", "mailbox", "crossbuck", "barricade", "bench", "table", "fence", "gate", "picket", "beam", "plank" };
        static readonly string[] NotProp = { "treehouse", "ramp", "landing", "deck", "bridge", "road", "lane", "ground", "trail", "porch", "stair", "step" };

        public void SetShown(bool on) { foreach (var (f, classic, fresh) in swaps) if (f) f.sharedMesh = on ? fresh : classic; }

        static bool Has(string n, string[] words) => words.Any(w => n.IndexOf(w, System.StringComparison.OrdinalIgnoreCase) >= 0);
        public void Build(Scene scene, HashSet<Renderer> skip)
        {
            foreach (var r in scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<MeshRenderer>(true)))
            {
                if (skip.Contains(r) || !r.TryGetComponent<MeshFilter>(out var f) || !f.sharedMesh || !f.sharedMesh.isReadable) continue;
                string n = r.name; var mesh = f.sharedMesh;
                if (r.GetComponentInParent<SceneryBuildings>() || r.GetComponentInParent<RaceGate>() || r.GetComponentInParent<ArcadeVehicle>()) continue;
                if (Has(n, RockWords) && !Has(n, NotRock) && mesh.vertexCount < 6000)
                {
                    var fresh = Rock(mesh, r.transform, Mathf.Abs(Mathf.Sin(r.transform.position.x * .37f + r.transform.position.z * .71f)));
                    swaps.Add((f, mesh, fresh)); Rocks++;
                }
                else if (Has(n, PropWords) && !Has(n, NotProp) && mesh.vertexCount == 24 && (mesh.name.StartsWith("Cube") || mesh.bounds.size.x > 0))
                {
                    var sc = r.transform.lossyScale; var key = (mesh, new Vector3Int(Mathf.RoundToInt(sc.x * 50), Mathf.RoundToInt(sc.y * 50), Mathf.RoundToInt(sc.z * 50)));
                    if (!bevelled.TryGetValue(key, out var fresh) || !fresh) bevelled[key] = fresh = Bevel(mesh, sc);
                    swaps.Add((f, mesh, fresh)); Bevelled++;
                }
            }
        }

        // A faceted rock: vertices welded by position, pushed outward along their normal by a little noise (never inward),
        // then flat-shaded (each face its own vertices).
        static Mesh Rock(Mesh old, Transform t, float seed)
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
            m.SetVertices(fv); m.SetTriangles(ft, 0); m.RecalculateNormals(); m.RecalculateBounds(); m.UploadMeshData(true);
            return m;
        }

        // A box mesh with bevelled edges inside the original box (a plain 24-vertex box / Unity cube).
        static Mesh Bevel(Mesh old, Vector3 scale)
        {
            var b = old.bounds; var s = Vector3.Scale(b.size, new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z)));
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
