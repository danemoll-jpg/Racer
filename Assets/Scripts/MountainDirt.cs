using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Racer
{
    // 0.80 Part A.3-4 (BUG-006, BUG-007): the summit launch's three road meshes - the giant-jump landing and return
    // ("Ground_CR103 supported return"), the inward connection ("Ground_CR103 supported connection") and the run-up and
    // lip ("Ground_Summit authored launch") - were drawn with the URP Lit "Summit packed earth" material, a dark, shiny
    // red-brown slab unlike every other mountain road, which is dirt painted into the terrain. Here they are drawn like
    // that terrain: the ground shader (ContinuousGround) with the mountain trails' own dirt colour, blending over the
    // outer 3 m into the colour of the terrain underneath, so they sit in the hillside. LOOK ONLY: same shape, same
    // collider (the collider keeps its own mesh), so the landing and the jump are unchanged. Every scene where these
    // objects are active (they belong to the shared world); the race scenes' own mountain roads are not touched.
    public static class MountainDirt
    {
        static readonly string[] Names = { "Ground_CR103 supported return", "Ground_CR103 supported connection", "Ground_Summit authored launch" };
        public static readonly Color Dirt = new(.46f, .33f, .18f);
        public const float Band = 3f;
        public static readonly List<string> Report = new();
        static readonly Dictionary<Mesh, Mesh> made = new();
        static Material early;

        public static void Apply(Scene scene)
        {
            Report.Clear();
            var roots = scene.GetRootGameObjects();
            var ground = roots.SelectMany(g => g.GetComponentsInChildren<MeshRenderer>()).FirstOrDefault(r => r.name.StartsWith("Ground_") && r.sharedMaterial && r.sharedMaterial.shader && r.sharedMaterial.shader.name == "Racer/GreyboxGround" && !(r.sharedMaterial.HasProperty("_Vegetation") && r.sharedMaterial.GetFloat("_Vegetation") > .5f));
            if (!ground) return;
            foreach (var r in roots.SelectMany(g => g.GetComponentsInChildren<MeshRenderer>()))
            {
                if (!Names.Contains(r.name) || !r.sharedMaterial || !r.sharedMaterial.name.StartsWith("Summit packed earth")) continue;
                var f = r.GetComponent<MeshFilter>(); if (!f || !f.sharedMesh || !f.sharedMesh.isReadable) { Report.Add(r.name + ": mesh not readable, left"); continue; }
                if (!made.TryGetValue(f.sharedMesh, out var mesh) || !mesh) made[f.sharedMesh] = mesh = Coloured(f.sharedMesh, r.transform, r.GetComponentsInChildren<Collider>());
                // drawn before the terrain (the same material, an earlier queue), so the terrain hidden under these large
                // meshes is rejected by the depth test instead of being shaded twice (the summit is the worst 4K view)
                if (!early || early.shader != ground.sharedMaterial.shader) early = new Material(ground.sharedMaterial) { name = ground.sharedMaterial.name + " (0.80 drawn first)", renderQueue = 1990 };
                f.sharedMesh = mesh; r.sharedMaterial = early;
                Report.Add($"{r.name}: drawn as mountain dirt ({mesh.vertexCount} vertices), collider unchanged");
            }
        }

        // a copy of the mesh with vertex colours: dirt, blending at the outer edge into the terrain colour beneath
        static Mesh Coloured(Mesh src, Transform t, Collider[] own)
        {
            var mesh = Object.Instantiate(src); mesh.name = src.name + " (0.80 dirt)";
            var v = src.vertices; var tris = src.triangles; var w = v.Select(p => t.TransformPoint(p)).ToArray();
            // boundary edges (welded by position, 2 cm) in the horizontal plane
            var id = new Dictionary<(int, int, int), int>(); var wid = new int[w.Length];
            for (int i = 0; i < w.Length; i++) { var k = (Mathf.RoundToInt(w[i].x * 50), Mathf.RoundToInt(w[i].y * 50), Mathf.RoundToInt(w[i].z * 50)); if (!id.TryGetValue(k, out var n)) { n = id.Count; id[k] = n; } wid[i] = n; }
            var pos = new Vector3[id.Count]; for (int i = 0; i < w.Length; i++) pos[wid[i]] = w[i];
            var count = new Dictionary<(int, int), int>();
            for (int i = 0; i < tris.Length; i += 3) for (int e = 0; e < 3; e++) { int a = wid[tris[i + e]], b = wid[tris[i + (e + 1) % 3]]; if (a == b) continue; var key = a < b ? (a, b) : (b, a); count[key] = count.TryGetValue(key, out var c) ? c + 1 : 1; }
            var edges = count.Where(kv => kv.Value == 1).Select(kv => (new Vector2(pos[kv.Key.Item1].x, pos[kv.Key.Item1].z), new Vector2(pos[kv.Key.Item2].x, pos[kv.Key.Item2].z))).ToList();
            // a 4 m grid of the edges for the distance queries
            var cells = new Dictionary<(int, int), List<int>>();
            for (int i = 0; i < edges.Count; i++)
            {
                var (a, b) = edges[i]; var lo = Vector2.Min(a, b); var hi = Vector2.Max(a, b);
                for (int x = Mathf.FloorToInt(lo.x / 4); x <= Mathf.FloorToInt(hi.x / 4); x++) for (int z = Mathf.FloorToInt(lo.y / 4); z <= Mathf.FloorToInt(hi.y / 4); z++) { if (!cells.TryGetValue((x, z), out var l)) cells[(x, z)] = l = new(); l.Add(i); }
            }
            float EdgeDistance(Vector2 q)
            {
                float best = Band; int cx = Mathf.FloorToInt(q.x / 4), cz = Mathf.FloorToInt(q.y / 4);
                for (int x = cx - 1; x <= cx + 1; x++) for (int z = cz - 1; z <= cz + 1; z++) if (cells.TryGetValue((x, z), out var l))
                            foreach (int i in l) { var (a, b) = edges[i]; var ab = b - a; float s = ab.sqrMagnitude < 1e-6f ? 0 : Mathf.Clamp01(Vector2.Dot(q - a, ab) / ab.sqrMagnitude); best = Mathf.Min(best, (a + ab * s - q).magnitude); }
                return best;
            }
            foreach (var c in own) c.enabled = false;
            var colours = new Color[v.Length];
            // a strip (a road ribbon: nearly every vertex on its two edges) has no inside to blend over: all dirt
            var dist = w.Select(p => EdgeDistance(new Vector2(p.x, p.z))).ToArray();
            bool strip = dist.Count(d => d < .05f) > dist.Length * .8f;
            for (int i = 0; i < v.Length; i++)
            {
                float d = strip ? Band : dist[i];
                float k = Mathf.SmoothStep(0, 1, d / Band);
                colours[i] = k >= 1 ? Dirt : Color.Lerp(Beneath(w[i]), Dirt, k);
            }
            foreach (var c in own) c.enabled = true;
            mesh.colors = colours; mesh.UploadMeshData(false);
            return mesh;
        }
        static readonly Color Grass = new(.22f, .32f, .13f);
        // the terrain's own colour under a point (or grass when it cannot be read)
        static Color Beneath(Vector3 p)
        {
            foreach (var h in Physics.RaycastAll(p + Vector3.up * 2, Vector3.down, 30, ~0, QueryTriggerInteraction.Ignore).OrderBy(h => h.distance))
            {
                if (h.collider.attachedRigidbody && !h.collider.attachedRigidbody.isKinematic) continue;
                if (h.collider is MeshCollider mc && mc.sharedMesh && h.triangleIndex >= 0 && SceneryGround.Terrain(mc.sharedMesh, out var cs, out var tr))
                {
                    int i = h.triangleIndex * 3; if (i + 2 >= tr.Length) break; var b = h.barycentricCoordinate;
                    var c = cs[tr[i]] * b.x + cs[tr[i + 1]] * b.y + cs[tr[i + 2]] * b.z; c.a = 1; return c;
                }
            }
            return Grass;
        }
    }
}
