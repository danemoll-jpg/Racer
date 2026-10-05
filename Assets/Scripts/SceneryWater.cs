using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Racer
{
    // 0.78 Part A6: lake and creek shores. Around every open body of water (ShallowWater with a renderer; pools and the
    // storm drain excluded) a bank is laid on the ground: a strip from just inside the water's edge to 1.8 m outside it,
    // each vertex dropped onto the terrain (3 cm above it, never below the water surface) and coloured from wet mud at
    // the water to the ground's own colour at its outer edge, so the water no longer ends in a hard line. Reed clumps
    // and a few pebbles stand where the ground meets the water. Nothing has a collider; the water and its Snow ice are
    // not touched. Reeds sway and take snow (Racer/Foliage); the bank takes snow and wet (Racer/Building).
    public sealed class SceneryWater : MonoBehaviour
    {
        public int Shores { get; private set; }
        public int Reeds { get; private set; }
        readonly List<GameObject> shown = new();
        public void SetShown(bool on) { foreach (var g in shown) if (g) g.SetActive(on); }

        static bool Ground(Vector3 above, out RaycastHit ground)
        {
            ground = default; float best = float.MaxValue; bool found = false;
            foreach (var h in Physics.RaycastAll(above, Vector3.down, 40, ~0, QueryTriggerInteraction.Ignore))
            {
                if (h.collider.attachedRigidbody && !h.collider.attachedRigidbody.isKinematic) continue;
                if (h.collider.GetComponentInParent<ShallowWater>()) continue;
                if (h.distance < best) { best = h.distance; ground = h; found = true; }
            }
            return found;
        }
        static Color GroundColor(RaycastHit h, Color fallback)
        {
            if (h.collider is MeshCollider mc && mc.sharedMesh && mc.sharedMesh.isReadable && h.triangleIndex >= 0)
            {
                int i = h.triangleIndex * 3;
                if (SceneryGround.Terrain(mc.sharedMesh, out var colors, out var tris) && i + 2 < tris.Length)
                {
                    var b = h.barycentricCoordinate; var c = colors[tris[i]] * b.x + colors[tris[i + 1]] * b.y + colors[tris[i + 2]] * b.z; c.a = 1; return c;
                }
            }
            return fallback;
        }

        public void Build(Scene scene)
        {
            var building = Resources.Load<Material>("Scenery/Building"); var foliage = Resources.Load<Material>("Scenery/Foliage"); if (!building || !foliage) return;
            foreach (var water in scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<ShallowWater>(true)))
            {
                var r = water.GetComponent<MeshRenderer>(); string n = water.name.ToLowerInvariant();
                if (!r || !water.gameObject.activeInHierarchy || n.Contains("pool") || n.Contains("drain") || n.Contains("ripple") || n.Contains("footprint")) continue;
                var t = water.transform; var s = t.lossyScale; if (Mathf.Max(s.x, s.z) < 3) continue;
                float level = water.Surface;
                // the water's outline (local x, z), about every 0.8 m
                var outline = new List<Vector3>();
                if (water.round) { float per = Mathf.PI * (s.x + s.z) * .5f; int count = Mathf.Clamp(Mathf.CeilToInt(per / .8f), 24, 400); for (int i = 0; i < count; i++) { float a = i * Mathf.PI * 2 / count; outline.Add(new Vector3(Mathf.Cos(a) * .5f, .5f, Mathf.Sin(a) * .5f)); } }
                else
                {
                    var corners = new[] { new Vector3(-.5f, .5f, -.5f), new Vector3(.5f, .5f, -.5f), new Vector3(.5f, .5f, .5f), new Vector3(-.5f, .5f, .5f) };
                    for (int k = 0; k < 4; k++) { var a = corners[k]; var b = corners[(k + 1) % 4]; float len = Vector3.Distance(t.TransformPoint(a), t.TransformPoint(b)); int steps = Mathf.Max(1, Mathf.CeilToInt(len / .8f)); for (int i = 0; i < steps; i++) outline.Add(Vector3.Lerp(a, b, (float)i / steps)); }
                }
                var world = outline.Select(p => t.TransformPoint(p)).ToList(); var centre = t.position;
                var bank = new Builder(); var reeds = new Builder(); var rnd = new System.Random(water.name.GetHashCode());
                Vector3 Out(int i) { var p = world[i]; var prev = world[(i + world.Count - 1) % world.Count]; var next = world[(i + 1) % world.Count]; var along = next - prev; var o = Vector3.Cross(Vector3.up, along).normalized; if (Vector3.Dot(o, p - centre) < 0) o = -o; o.y = 0; return o.normalized; }
                var mud = new Color(.24f, .20f, .15f); var damp = new Color(.33f, .30f, .21f);
                const int rings = 4; float[] offsets = { -.6f, 0, .7f, 1.8f };
                var grid = new Vector3[world.Count, rings]; var colors = new Color[world.Count, rings]; var ok = new bool[world.Count, rings];
                for (int i = 0; i < world.Count; i++)
                {
                    var o = Out(i);
                    for (int k = 0; k < rings; k++)
                    {
                        var p = world[i] + o * offsets[k];
                        if (!Ground(new Vector3(p.x, level + 15, p.z), out var hit)) continue;
                        float y = Mathf.Max(hit.point.y + .03f, level + .012f); if (hit.point.y > level + 2.5f) continue; // a steep high bank: leave it
                        grid[i, k] = new Vector3(p.x, y, p.z); ok[i, k] = true;
                        var g = GroundColor(hit, damp);
                        colors[i, k] = k == 0 ? mud : k == 1 ? Color.Lerp(mud, damp, .5f) : k == 2 ? Color.Lerp(damp, g, .55f) : g;
                    }
                    // reeds where the ground meets the water
                    if (ok[i, 1] && Mathf.Abs(grid[i, 1].y - level) < .45f && rnd.NextDouble() < .38) { Reed(reeds, grid[i, 1] - o * (float)rnd.NextDouble() * .5f, rnd); Reeds++; }
                    if (ok[i, 1] && rnd.NextDouble() < .12) Pebble(bank, grid[i, 1] + o * (float)(rnd.NextDouble() * .6), rnd);
                }
                for (int i = 0; i < world.Count; i++)
                {
                    int j = (i + 1) % world.Count; if (!water.round && j == 0 && false) continue;
                    for (int k = 0; k + 1 < rings; k++)
                        if (ok[i, k] && ok[j, k] && ok[i, k + 1] && ok[j, k + 1])
                        {
                            bank.Vertex(grid[i, k], colors[i, k]); bank.Vertex(grid[i, k + 1], colors[i, k + 1]); bank.Vertex(grid[j, k + 1], colors[j, k + 1]); bank.Face(Vector3.up);
                            bank.Vertex(grid[i, k], colors[i, k]); bank.Vertex(grid[j, k + 1], colors[j, k + 1]); bank.Vertex(grid[j, k], colors[j, k]); bank.Face(Vector3.up);
                        }
                }
                Emit(bank, building, "Shore of " + water.name, false); Emit(reeds, foliage, "Reeds of " + water.name, true); Shores++;
            }
        }
        static void Reed(Builder m, Vector3 at, System.Random rnd)
        {
            int blades = 6 + rnd.Next(6);
            for (int b = 0; b < blades; b++)
            {
                float a = (float)rnd.NextDouble() * Mathf.PI * 2, d = (float)rnd.NextDouble() * .22f, h = .7f + (float)rnd.NextDouble() * .8f, lean = .12f + (float)rnd.NextDouble() * .2f;
                var root = at + new Vector3(Mathf.Cos(a) * d, -.03f, Mathf.Sin(a) * d); var dir = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)); var side = Vector3.Cross(Vector3.up, dir) * .025f;
                var tip = root + Vector3.up * h + dir * lean;
                var green = Color.Lerp(new Color(.30f, .38f, .16f), new Color(.46f, .44f, .22f), (float)rnd.NextDouble());
                m.Vertex(root - side, green, 0); m.Vertex(root + side, green, 0); m.Vertex(tip, green, 1); m.Face(Vector3.Cross(side, tip - root));
                m.Vertex(root + side, green, 0); m.Vertex(root - side, green, 0); m.Vertex(tip, green, 1); m.Face(-Vector3.Cross(side, tip - root));
                if (b % 4 == 0)
                {   // a cattail head
                    var head = root + Vector3.up * (h * .82f) + dir * lean * .8f; var brown = new Color(.30f, .19f, .11f);
                    for (int q = 0; q < 4; q++) { float qa = q * Mathf.PI * .5f; var o1 = new Vector3(Mathf.Cos(qa), 0, Mathf.Sin(qa)) * .03f; var o2 = new Vector3(Mathf.Cos(qa + Mathf.PI * .5f), 0, Mathf.Sin(qa + Mathf.PI * .5f)) * .03f; m.Vertex(head + o1, brown, 1); m.Vertex(head + o2, brown, 1); m.Vertex(head + Vector3.up * .16f + (o1 + o2) * .5f, brown, 1); m.Face(o1 + o2); }
                }
            }
        }
        static void Pebble(Builder m, Vector3 at, System.Random rnd)
        {
            float r = .06f + (float)rnd.NextDouble() * .12f; var c = Color.Lerp(new Color(.42f, .41f, .38f), new Color(.55f, .52f, .46f), (float)rnd.NextDouble());
            var top = at + Vector3.up * r * .7f; var ring = Enumerable.Range(0, 5).Select(i => at + new Vector3(Mathf.Cos(i * 1.2566f + r * 9) * r, .01f, Mathf.Sin(i * 1.2566f + r * 9) * r * .8f)).ToArray();
            for (int i = 0; i < 5; i++) { m.Vertex(ring[i], c); m.Vertex(top, c); m.Vertex(ring[(i + 1) % 5], c); m.Face(Vector3.up); }
        }
        void Emit(Builder m, Material mat, string name, bool sway)
        {
            if (m.Count == 0) return;
            var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer)); go.transform.SetParent(transform, false);
            go.GetComponent<MeshFilter>().sharedMesh = m.Mesh(name, sway); var r = go.GetComponent<MeshRenderer>(); r.sharedMaterial = mat; r.shadowCastingMode = ShadowCastingMode.Off;
            shown.Add(go);
        }

        // Triangles with flat normals that face a given way; colours; for reeds the sway weight (alpha) and leaf flag (uv).
        sealed class Builder
        {
            readonly List<Vector3> v = new(); readonly List<Color> c = new(); readonly List<float> sway = new(); readonly List<Vector3> n = new();
            public int Count => v.Count;
            public void Vertex(Vector3 p, Color col, float s = 0) { v.Add(p); c.Add(col); sway.Add(s); }
            public void Face(Vector3 facing)
            {
                int i = v.Count - 3; var nn = Vector3.Cross(v[i + 1] - v[i], v[i + 2] - v[i]);
                if (Vector3.Dot(nn, facing) < 0) { (v[i + 1], v[i + 2]) = (v[i + 2], v[i + 1]); (c[i + 1], c[i + 2]) = (c[i + 2], c[i + 1]); (sway[i + 1], sway[i + 2]) = (sway[i + 2], sway[i + 1]); nn = -nn; }
                nn.Normalize(); n.Add(nn); n.Add(nn); n.Add(nn);
            }
            public Mesh Mesh(string name, bool foliage)
            {
                var m = new Mesh { name = name, indexFormat = v.Count > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16 };
                m.SetVertices(v); m.SetNormals(n);
                m.SetColors(c.Select((col, i) => new Color(col.r, col.g, col.b, foliage ? sway[i] : 1)).ToList());
                if (foliage) m.SetUVs(0, Enumerable.Repeat(new Vector2(0, 0), v.Count).ToList());
                m.SetTriangles(Enumerable.Range(0, v.Count).ToArray(), 0); m.RecalculateBounds(); m.UploadMeshData(true); return m;
            }
        }
    }
}
