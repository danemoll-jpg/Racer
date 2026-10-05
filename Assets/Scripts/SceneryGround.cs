using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Racer
{
    // 0.78 Part A5: sparse ground detail near the camera, with no collision: grass tufts and a few small flowers on grass,
    // leaf litter under tree crowns. Only on the open terrain mesh where its colour is grass (never on a road, trail,
    // driveway or other dirt surface, so roads, trails, arrows and gates stay as readable as before). Placed once per
    // 12 m cell as the camera nears it (a few raycasts each), kept for nearby cells, drawn instanced (Racer/Foliage:
    // tufts sway and take snow).
    public sealed class SceneryGround : MonoBehaviour
    {
        public const float CellSize = 12, Radius = 54; const int PerCell = 26;
        static Mesh tuft, flower, litter; static Material material;
        readonly Dictionary<Vector2Int, (List<Matrix4x4> tufts, List<Matrix4x4> flowers, List<Matrix4x4> litter)> cells = new();
        readonly List<Matrix4x4> tufts = new(), flowers = new(), leaves = new();
        public SceneryTrees Trees;
        public int Placed { get; private set; }

        static Mesh Make(string name, System.Action<List<Vector3>, List<Color>> build)
        {
            var v = new List<Vector3>(); var c = new List<Color>(); build(v, c);
            var t = new List<int>(); for (int i = 0; i < v.Count; i++) t.Add(i);
            var m = new Mesh { name = name }; m.SetVertices(v); m.SetColors(c); m.SetTriangles(t, 0);
            var uv = new List<Vector2>(); foreach (var col in c) uv.Add(new Vector2(col.g > col.r * 1.15f ? 1 : 0, 0)); m.SetUVs(0, uv);
            m.RecalculateNormals(); m.RecalculateBounds(); m.UploadMeshData(true); return m;
        }
        static void Blade(List<Vector3> v, List<Color> c, Vector3 root, Vector3 tip, float w, Color col)
        {
            var side = Vector3.Cross(Vector3.up, tip - root).normalized * w; if (side.sqrMagnitude < 1e-8f) side = Vector3.right * w;
            Color b = col * .75f; b.a = 0; Color top = col; top.a = 1;
            v.Add(root - side); v.Add(tip); v.Add(root + side); c.Add(b); c.Add(top); c.Add(b);
            v.Add(root + side); v.Add(tip); v.Add(root - side); c.Add(b); c.Add(top); c.Add(b);
        }
        static void Kit()
        {
            if (material) return;
            material = Resources.Load<Material>("Scenery/Foliage");
            tuft = Make("Scenery grass tuft", (v, c) =>
            {
                var r = new System.Random(5);
                for (int i = 0; i < 7; i++) { float a = i * 0.9f, d = (float)r.NextDouble() * .12f; var root = new Vector3(Mathf.Cos(a) * d, 0, Mathf.Sin(a) * d); var tip = root * 2.4f + Vector3.up * (.75f + (float)r.NextDouble() * .5f); Blade(v, c, root, tip, .035f, new Color(.30f, .40f, .17f)); }
            });
            flower = Make("Scenery flower", (v, c) =>
            {
                for (int i = 0; i < 3; i++) { float a = i * 2.1f; var root = new Vector3(Mathf.Cos(a) * .05f, 0, Mathf.Sin(a) * .05f); Blade(v, c, root, root * 1.5f + Vector3.up * .7f, .02f, new Color(.28f, .40f, .16f)); }
                var head = Vector3.up * .75f;
                for (int p = 0; p < 5; p++) { float a = p * 1.2566f; var o = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)) * .09f; var o2 = new Vector3(Mathf.Cos(a + 1.2566f), 0, Mathf.Sin(a + 1.2566f)) * .09f; var petal = new Color(.92f, .86f, .40f, 1); v.Add(head); v.Add(head + o2); v.Add(head + o); c.Add(petal); c.Add(petal); c.Add(petal); }
            });
            litter = Make("Scenery leaf litter", (v, c) =>
            {
                var r = new System.Random(9);
                for (int i = 0; i < 9; i++)
                {
                    var p = new Vector3((float)r.NextDouble() - .5f, .02f, (float)r.NextDouble() - .5f) * 1.6f; float a = (float)r.NextDouble() * 6.28f; var u = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)) * .09f; var w = new Vector3(-u.z, 0, u.x) * .6f;
                    var col = Color.Lerp(new Color(.42f, .27f, .12f), new Color(.55f, .42f, .18f), (float)r.NextDouble()); col.a = 0;
                    v.Add(p - u); v.Add(p + w); v.Add(p + u); v.Add(p - u); v.Add(p + u); v.Add(p - w); for (int k = 0; k < 6; k++) c.Add(col);
                }
            });
        }

        // Terrain colours and triangles, read once per mesh (a terrain mesh is large; copying it per sample would stall).
        static readonly Dictionary<Mesh, (Color[] colors, int[] tris)> terrain = new();
        public static void ForgetTerrain() => terrain.Clear();
        void OnDestroy() => ForgetTerrain();
        public static bool Terrain(Mesh m, out Color[] colors, out int[] tris)
        {
            if (!terrain.TryGetValue(m, out var e)) { e = m.isReadable ? (m.colors, m.triangles) : (new Color[0], new int[0]); terrain[m] = e; }
            colors = e.colors; tris = e.tris; return colors.Length == m.vertexCount && tris.Length > 0;
        }
        static bool Grass(RaycastHit h)
        {
            if (!(h.collider is MeshCollider mc) || !mc.sharedMesh || !mc.sharedMesh.isReadable || h.triangleIndex < 0) return false;
            if (!h.collider.TryGetComponent<MeshRenderer>(out var r) || !r.sharedMaterial || r.sharedMaterial.shader.name != "Racer/GreyboxGround") return false;
            var m = mc.sharedMesh; if (!Terrain(m, out var colors, out var tris)) return false;
            int i = h.triangleIndex * 3; if (i + 2 >= tris.Length) return false;
            var b = h.barycentricCoordinate; var col = colors[tris[i]] * b.x + colors[tris[i + 1]] * b.y + colors[tris[i + 2]] * b.z;
            // all three corners must be grass (well away from any road or trail colour)
            foreach (int k in new[] { tris[i], tris[i + 1], tris[i + 2] }) { var cc = colors[k]; if (cc.g - Mathf.Max(cc.r, cc.b) < .03f || cc.b - cc.r > .0f) return false; }
            return col.g - Mathf.Max(col.r, col.b) > .04f && h.normal.y > .75f;
        }
        (List<Matrix4x4>, List<Matrix4x4>, List<Matrix4x4>) Fill(Vector2Int key)
        {
            var t = new List<Matrix4x4>(); var f = new List<Matrix4x4>(); var l = new List<Matrix4x4>();
            var rnd = new System.Random(key.x * 73856093 ^ key.y * 19349663);
            for (int i = 0; i < PerCell; i++)
            {
                var p = new Vector3((key.x + (float)rnd.NextDouble()) * CellSize, 0, (key.y + (float)rnd.NextDouble()) * CellSize);
                if (!Physics.Raycast(new Vector3(p.x, 700, p.z), Vector3.down, out var h, 1400, 1, QueryTriggerInteraction.Ignore) || !Grass(h)) continue;
                var rot = Quaternion.FromToRotation(Vector3.up, h.normal) * Quaternion.Euler(0, (float)rnd.NextDouble() * 360, 0); float s = .45f + (float)rnd.NextDouble() * .4f;
                if (Trees && Trees.UnderCrown(h.point)) { l.Add(Matrix4x4.TRS(h.point, rot, Vector3.one * (1.2f + s))); continue; }
                if (rnd.NextDouble() < .1) f.Add(Matrix4x4.TRS(h.point, rot, Vector3.one * s)); else t.Add(Matrix4x4.TRS(h.point, rot, new Vector3(s, s * (.8f + (float)rnd.NextDouble() * .5f), s)));
                Placed++;
            }
            return (t, f, l);
        }

        void LateUpdate()
        {
            Kit(); var cam = Camera.main; if (!cam || !material) return;
            var eye = cam.transform.position; int reach = Mathf.CeilToInt(Radius / CellSize); var c0 = new Vector2Int(Mathf.FloorToInt(eye.x / CellSize), Mathf.FloorToInt(eye.z / CellSize));
            tufts.Clear(); flowers.Clear(); leaves.Clear(); int made = 0;
            for (int x = -reach; x <= reach; x++) for (int z = -reach; z <= reach; z++)
                {
                    var k = new Vector2Int(c0.x + x, c0.y + z); var centre = new Vector3((k.x + .5f) * CellSize, eye.y, (k.y + .5f) * CellSize);
                    if ((new Vector2(centre.x - eye.x, centre.z - eye.z)).sqrMagnitude > (Radius + CellSize) * (Radius + CellSize)) continue;
                    if (!cells.TryGetValue(k, out var cell)) { if (made >= 6) continue; cells[k] = cell = Fill(k); made++; } // a few new cells per frame
                    tufts.AddRange(cell.tufts); flowers.AddRange(cell.flowers); leaves.AddRange(cell.litter);
                }
            if (cells.Count > 600) { var drop = new List<Vector2Int>(); foreach (var k in cells.Keys) if (Mathf.Abs(k.x - c0.x) > reach + 3 || Mathf.Abs(k.y - c0.y) > reach + 3) drop.Add(k); foreach (var k in drop) cells.Remove(k); }
            var rp = new RenderParams(material) { shadowCastingMode = ShadowCastingMode.Off, receiveShadows = true, layer = 0, worldBounds = new Bounds(eye, Vector3.one * (Radius * 2 + 40)) };
            if (tufts.Count > 0) SceneryTrees.Draw(rp, tuft, tufts);
            if (flowers.Count > 0) SceneryTrees.Draw(rp, flower, flowers);
            if (leaves.Count > 0) SceneryTrees.Draw(rp, litter, leaves);
        }
    }
}
