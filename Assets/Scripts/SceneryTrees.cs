using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Racer
{
    // 0.78 Part A1: the new trees and bushes. The old forest is drawn by big combined vertex-colour meshes (the
    // "Racer/GreyboxGround" shader with _Vegetation = 1: Phase6Vegetation, CR014, the mountain woods, ...): each old tree
    // is a crown (one or a few closed blobs, green) over a bark box; the trunk colliders are separate invisible boxes.
    // Build() reads those meshes once, finds every crown and bark box (connected pieces of the mesh), matches the crowns
    // to their trunk (the trunk collider, or the bark box of a tree that never had one), and fits a tree from the Blender
    // kit (Tools/Blender/scenery_trees.py) to each: the trunk exactly over the old trunk box (same place, height and
    // width), the crown within the old crown's bounds. Crowns without a trunk become bushes (near the ground) or crown-only
    // clumps. The old meshes are then hidden by SceneryWorld; no collider is touched.
    // Drawing: instanced (Racer/Foliage), in 96 m cells; near cells use the detailed kit and cast shadows, far cells the
    // simplified kit without shadows; cells beyond the draw distance are skipped.
    public sealed class SceneryTrees : MonoBehaviour
    {
        public const float Cell = 96, FarCell = 256, NearDistance = 90, DrawDistance = 1100, FarTrunks = 450;
        static readonly string[] Variants = { "Round", "Oak", "Maple", "Pine", "Poplar", "Young", "Bush", "Shrub" };
        const int Round = 0, Oak = 1, Maple = 2, Pine = 3, Poplar = 4, Young = 5, Bush = 6, Shrub = 7;
        static readonly Color[] Leaf = { new(.25f, .37f, .17f), new(.21f, .32f, .14f), new(.31f, .38f, .16f), new(.13f, .25f, .15f), new(.30f, .41f, .18f), new(.33f, .45f, .20f), new(.22f, .33f, .15f), new(.26f, .35f, .17f) };
        static readonly Color[] Bark = { new(.27f, .21f, .15f), new(.24f, .19f, .14f), new(.29f, .22f, .16f), new(.26f, .18f, .13f), new(.36f, .33f, .29f), new(.30f, .24f, .17f), new(.27f, .21f, .15f), new(.27f, .21f, .15f) };

        // ---------------------------------------------------------------- the kit
        static Mesh[] crownNear, crownFar, trunkNear, trunkFar; static Material material;
        static bool LoadKit()
        {
            if (material) return true;
            var asset = Resources.Load<GameObject>("Scenery/SceneryTrees"); material = Resources.Load<Material>("Scenery/Foliage");
            if (!asset || !material) { Debug.LogWarning("Scenery: tree kit or foliage material missing"); return false; }
            Mesh Part(string name) => asset.GetComponentsInChildren<MeshFilter>(true).FirstOrDefault(f => f.name == name)?.sharedMesh;
            int n = Variants.Length; crownNear = new Mesh[n]; crownFar = new Mesh[n]; trunkNear = new Mesh[n]; trunkFar = new Mesh[n];
            for (int v = 0; v < n; v++)
            {
                crownNear[v] = Prepare(Variants[v] + " crown", v, Part(Variants[v] + "_Crown__leaf"), Part(Variants[v] + "_Crown__bark"), true);
                crownFar[v] = Prepare(Variants[v] + " crown far", v, Part(Variants[v] + "_CrownFar__leaf"), Part(Variants[v] + "_CrownFar__bark"), true);
                trunkNear[v] = Prepare(Variants[v] + " trunk", v, null, Part(Variants[v] + "_Trunk__bark"), false);
                trunkFar[v] = Prepare(Variants[v] + " trunk far", v, null, Part(Variants[v] + "_TrunkFar__bark"), false);
            }
            return true;
        }
        // One mesh per part: per-face colours (leaf facets vary a little in shade so clumps read; bark is the species'
        // bark), alpha = sway weight (leaves by height in the crown, branches half, trunks none), uv.x = 1 on leaves.
        static Mesh Prepare(string name, int variant, Mesh leaf, Mesh bark, bool crown)
        {
            if (!leaf && !bark) return null;
            var v = new List<Vector3>(); var nrm = new List<Vector3>(); var col = new List<Color>(); var uv = new List<Vector2>(); var tri = new List<int>();
            void Add(Mesh m, bool isLeaf)
            {
                if (!m) return;
                var mv = m.vertices; var mn = m.normals; var mt = m.triangles;
                for (int i = 0; i < mt.Length; i += 3)
                {
                    Vector3 a = mv[mt[i]], b = mv[mt[i + 1]], c = mv[mt[i + 2]], centre = (a + b + c) / 3;
                    float shade = isLeaf ? .86f + .28f * Mathf.PerlinNoise(centre.x * 3.1f + variant, centre.z * 3.1f + centre.y * 2.3f) : .9f + .2f * Mathf.PerlinNoise(centre.y * 4, variant);
                    var baseColor = (isLeaf ? Leaf[variant] : Bark[variant]) * shade; baseColor.a = 1;
                    foreach (int k in new[] { mt[i], mt[i + 1], mt[i + 2] })
                    {
                        var p = mv[k]; float sway = crown ? (isLeaf ? Mathf.Clamp01(.35f + p.y * .65f) : Mathf.Clamp01(p.y) * .5f) : 0;
                        var c4 = baseColor; c4.a = sway;
                        tri.Add(v.Count); v.Add(p); nrm.Add(mn != null && mn.Length > k ? mn[k] : Vector3.up); col.Add(c4); uv.Add(new Vector2(isLeaf ? 1 : 0, 0));
                    }
                }
            }
            Add(leaf, true); Add(bark, false);
            var mesh = new Mesh { name = "Scenery " + name, indexFormat = v.Count > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16 };
            mesh.SetVertices(v); mesh.SetNormals(nrm); mesh.SetColors(col); mesh.SetUVs(0, uv); mesh.SetTriangles(tri, 0); mesh.RecalculateBounds(); mesh.UploadMeshData(true);
            return mesh;
        }

        // ---------------------------------------------------------------- instances
        sealed class CellData
        {
            public Bounds bounds; public bool any;
            public readonly List<Matrix4x4>[] crowns = Enumerable.Range(0, Variants.Length).Select(_ => new List<Matrix4x4>()).ToArray();
            public readonly List<Matrix4x4>[] trunks = Enumerable.Range(0, Variants.Length).Select(_ => new List<Matrix4x4>()).ToArray();
        }
        readonly Dictionary<Vector2Int, CellData> cells = new();
        // far away every tree is one of three simple shapes (broadleaf crown, conifer crown, trunk) in big cells
        sealed class FarGroup { public Bounds bounds; public bool any; public readonly List<Matrix4x4> broad = new(), conifer = new(), trunks = new(), sb = new(), sc = new(), st = new(); }
        readonly Dictionary<Vector2Int, FarGroup> far = new();
        FarGroup FarAt(Vector3 p) { var k = new Vector2Int(Mathf.FloorToInt(p.x / FarCell), Mathf.FloorToInt(p.z / FarCell)); if (!far.TryGetValue(k, out var c)) far[k] = c = new FarGroup(); return c; }
        public int TreeCount { get; private set; }
        public int TrunkOnly { get; private set; }
        public int CrownOnly { get; private set; }
        public int Bushes { get; private set; }
        public int DrivableSkipped { get; private set; }
        public int[] PerVariant { get; } = new int[Variants.Length];
        public string Summary => $"{TreeCount} trees ({string.Join(", ", Variants.Select((n, i) => n + " " + PerVariant[i]))}), {Bushes} bushes, {CrownOnly} crown-only, {TrunkOnly} trunks without crown";
        public int DrawCalls { get; private set; }
        // every placed tree, bush and clump (for the checks): where it stands, how wide, and what it was made from
        public struct Placement { public Vector3 bottom; public float radius, height; public int variant; public string source; }
        public readonly List<Placement> Placements = new();
        public int TrunksAdded { get; private set; }
        public int BarkSeated { get; private set; }
        public List<string> TrunksNear(Vector3 p, float r) => Placements.Where(x => { var d = x.bottom - p; d.y = 0; return d.magnitude < r; })
            .Select(x => $"{Variants[x.variant]} {x.source} at {x.bottom.x:F1},{x.bottom.y:F2},{x.bottom.z:F1} r {x.radius:F2} h {x.height:F1}").ToList();
        public static string KitInfo => material ? $"kit: crown {crownNear?[0]?.vertexCount} verts, trunk {trunkNear?[0]?.vertexCount} verts, material {material.shader.name} supported {material.shader.isSupported} instancing {material.enableInstancing}" : "kit not loaded";
        public static Mesh DebugCrown => crownNear?[0]; public static Material DebugMaterial => material;
        public Bounds FirstCellBounds => cells.Values.FirstOrDefault(c => c.any)?.bounds ?? default;

        CellData CellAt(Vector3 p)
        {
            var key = new Vector2Int(Mathf.FloorToInt(p.x / Cell), Mathf.FloorToInt(p.z / Cell));
            if (!cells.TryGetValue(key, out var c)) cells[key] = c = new CellData();
            return c;
        }
        void Grow(CellData c, Bounds b) { if (c.any) c.bounds.Encapsulate(b); else { c.bounds = b; c.any = true; } }
        void AddCrown(int variant, Vector3 bottom, float radius, float height, float yaw)
        {
            var m = Matrix4x4.TRS(bottom, Quaternion.Euler(0, yaw, 0), new Vector3(radius, height, radius));
            var c = CellAt(bottom); c.crowns[variant].Add(m); var b = new Bounds(bottom + Vector3.up * height * .5f, new Vector3(radius * 2.4f, height * 1.2f, radius * 2.4f));
            Grow(c, b); var f = FarAt(bottom); (variant == Pine ? f.conifer : f.broad).Add(m); if (f.any) f.bounds.Encapsulate(b); else { f.bounds = b; f.any = true; }
        }
        void AddTrunk(int variant, Vector3 bottom, Vector3 up, float width, float height, float yaw)
        {
            var m = Matrix4x4.TRS(bottom, Quaternion.FromToRotation(Vector3.up, up) * Quaternion.Euler(0, yaw, 0), new Vector3(width, height, width));
            var c = CellAt(bottom); c.trunks[variant].Add(m); var b = new Bounds(bottom + up * height * .5f, new Vector3(width * 2, height, width * 2));
            Grow(c, b); var f = FarAt(bottom); f.trunks.Add(m); if (f.any) f.bounds.Encapsulate(b); else { f.bounds = b; f.any = true; }
        }

        // ---------------------------------------------------------------- reading the old forest
        struct Piece { public Bounds b; public Color c; public int verts; public bool bark; }
        struct Trunk { public Vector3 bottom, up; public float width, height; public bool collider, cleared; public Bounds crown; public bool hasCrown; }
        public static bool IsOldVegetation(Renderer r)
        {
            // never the ground: some terrain tiles share the vegetation shading (CR117 outer ravine); a crown batch has no collider
            if (r.GetComponent<Collider>() || r.name.StartsWith("Ground", System.StringComparison.OrdinalIgnoreCase)) return false;
            var m = r.sharedMaterial;
            return m && m.shader && m.shader.name == "Racer/GreyboxGround" && m.HasProperty("_Vegetation") && m.GetFloat("_Vegetation") > .5f
                && r.TryGetComponent<MeshFilter>(out var f) && f.sharedMesh && f.sharedMesh.isReadable;
        }
        // The mountain woods (ExplorationAuthoring) are drawn on URP Lit materials: one batch of sphere crowns, one of cube
        // trunks per "Mountain woods" group (Dan's house has batches of the same names, which stay as they are).
        static int MountainKind(Renderer r)
        {
            if (!r.transform.parent || !r.transform.parent.name.StartsWith("Mountain woods")) return 0;
            if (!r.TryGetComponent<MeshFilter>(out var f) || !f.sharedMesh || !f.sharedMesh.isReadable) return 0;
            return r.name == "Batched Mountain foliage" ? 1 : r.name == "Batched Weathered timber" ? 2 : 0;
        }
        static bool IsTrunkCollider(BoxCollider b) => b.name.IndexOf("trunk", System.StringComparison.OrdinalIgnoreCase) >= 0;

        // ---------------------------------------------------------------- 0.79 Part E: nothing grows on a drivable surface
        // A tree, bush or clump "stands on" what is directly under its base (within 1.5 m). Drivable: paved terrain (the
        // road colour), asphalt / gravel / concrete surfaces, road / driveway / parking / lane / trail meshes, and dirt
        // terrain inside a trail's width. Verges, shoulders and open ground are not.
        static readonly string[] PavedWords = { "asphalt", "gravel", "concrete", "pavement", "paving", "paved", "tarmac" };
        static readonly string[] RoadWords = { "road", "driveway", "parking", "lane", "street", "highway", "hwy", "trail" };
        static readonly string[] NotRoad = { "verge", "shoulder", "embankment", "bank", "kerb", "curb" };
        static bool Has(string n, string[] words) => words.Any(w => n.IndexOf(w, System.StringComparison.OrdinalIgnoreCase) >= 0);
        static readonly RaycastHit[] hits = new RaycastHit[32];
        public static readonly IComparer<RaycastHit> ByDistance = Comparer<RaycastHit>.Create((a, b) => a.distance.CompareTo(b.distance));
        public static bool OnDrivable(Vector3 bottom, IList<RaceRoad> trails, out string what)
        {
            what = null;
            if (trails != null && OnRaceLine(bottom, out what)) return true;
            int n = Physics.RaycastNonAlloc(bottom + Vector3.up * 2.5f, Vector3.down, hits, 6, ~0, QueryTriggerInteraction.Ignore);
            System.Array.Sort(hits, 0, n, ByDistance);
            for (int k = 0; k < n; k++)
            {
                var h = hits[k];
                var c = h.collider; if (c is BoxCollider b && IsTrunkCollider(b)) continue; if (c.attachedRigidbody && !c.attachedRigidbody.isKinematic) continue;
                if (bottom.y - h.point.y > 1.5f) return false; // above the ground: not standing on it
                var r = c.GetComponent<Renderer>(); var mat = r ? r.sharedMaterial : null;
                if (mat && mat.shader && mat.shader.name.StartsWith("Racer/") && c is MeshCollider mc && mc.sharedMesh && h.triangleIndex >= 0 && SceneryGround.Terrain(mc.sharedMesh, out var cs, out var tr))
                {
                    int i = h.triangleIndex * 3; if (i + 2 >= tr.Length) return false; var bc = h.barycentricCoordinate; var col = cs[tr[i]] * bc.x + cs[tr[i + 1]] * bc.y + cs[tr[i + 2]] * bc.z;
                    if (col.b - col.r > .005f) { what = "paved road (terrain)"; return true; }
                    if (col.r > col.g + .02f && trails != null)
                        foreach (var t in trails) { float s = t.Project(h.point, out _); var q = t.At(s, out _); var d = q - h.point; d.y = 0; if (d.magnitude < t.HalfWidth(s) - .5f) { what = "trail " + t.name; return true; } }
                    return false;
                }
                string names = c.name + "/" + (c.transform.parent ? c.transform.parent.name : "");
                var at = MaterialAt(h) ?? mat;
                if (at && Has(at.name, NotRoad)) return false;
                if (at && Has(at.name, PavedWords)) { what = at.name; return true; }
                if (Has(names, RoadWords) && !Has(names, NotRoad)) { what = names; return true; }
                return false;
            }
            return false;
        }
        // 0.85 Part A: the driving surface of every optional line of a course scene (0.79 only knew the main, whose trail
        // surface its dirt-colour test recognises, so 0.84's grounded trees could stand on Forest Reverse's optional lines),
        // with a margin each side so a trail reads as open: within the line's own half-width + LineMargin of its centre line
        // and 2.5 m of its height. Set when the trees are built; used wherever trees and clumps are tested (trails given),
        // never for posts, signs or paint.
        public const float LineMargin = 1.5f;
        static readonly List<WoodlandRoute> raceLines = new();
        static Scene raceLineScene;
        public static void SetRaceLines(Scene scene)
        {
            raceLines.Clear(); raceLineScene = scene; if (scene.name == RaceFlow.RoamScene) return;
            foreach (var g in scene.GetRootGameObjects())
                foreach (var w in g.GetComponentsInChildren<WoodlandRoute>(true)) if (w.gameObject.activeInHierarchy && w.points != null && w.points.Length > 1) { w.Initialize(); raceLines.Add(w); }
        }
        public static bool OnRaceLine(Vector3 p, out string what, float margin = LineMargin)
        {
            what = null; if (raceLines.Count == 0 || !raceLineScene.IsValid() || !raceLineScene.isLoaded) return false;
            foreach (var branch in raceLines)
            {
                float s = branch.Project(p, out _); var d = p - branch.At(s, out _); float lateral = new Vector2(d.x, d.z).magnitude;
                if (lateral < branch.halfWidth + margin && Mathf.Abs(d.y) < 2.5f) { what = $"optional line {branch.title} (s {s:F0}, {lateral:F1} m from the centre)"; return true; }
            }
            return false;
        }
        // the material of the hit triangle (a mesh may carry the road and its verge as two materials)
        static Material MaterialAt(RaycastHit h)
        {
            if (!(h.collider is MeshCollider mc) || !mc.sharedMesh || !mc.sharedMesh.isReadable || h.triangleIndex < 0) return null;
            var r = h.collider.GetComponent<Renderer>(); if (!r) return null; var mats = r.sharedMaterials; var m = mc.sharedMesh;
            if (mats.Length < 2 || m.subMeshCount < 2 || !r.TryGetComponent<MeshFilter>(out var f) || f.sharedMesh != m) return null;
            int index = h.triangleIndex * 3;
            for (int i = 0; i < m.subMeshCount && i < mats.Length; i++) { var sm = m.GetSubMesh(i); if (index >= sm.indexStart && index < sm.indexStart + sm.indexCount) return mats[i]; }
            return null;
        }
        // The race lines of a course scene (main route and its branches): a tree near one keeps its visual (a hidden
        // trunk with a live collider would be an invisible obstacle in a race).
        static List<Vector3[]> RaceLines(Scene scene)
        {
            var lines = new List<Vector3[]>();
            foreach (var g in scene.GetRootGameObjects())
            {
                foreach (var d in g.GetComponentsInChildren<RaceDirector>(true)) if (d.road && d.road.points != null) lines.Add(d.road.points);
                foreach (var w in g.GetComponentsInChildren<WoodlandRoute>(true)) if (w.gameObject.activeInHierarchy && w.points != null) lines.Add(w.points);
            }
            return lines;
        }
        static float LineDistance(List<Vector3[]> lines, Vector3 p)
        {
            float best = float.MaxValue; var q = new Vector2(p.x, p.z);
            foreach (var l in lines) for (int i = 0; i + 1 < l.Length; i++)
                {
                    Vector2 a = new(l[i].x, l[i].z), b = new(l[i + 1].x, l[i + 1].z), ab = b - a; float t = ab.sqrMagnitude < 1e-6f ? 0 : Mathf.Clamp01(Vector2.Dot(q - a, ab) / ab.sqrMagnitude);
                    best = Mathf.Min(best, (a + ab * t - q).magnitude);
                }
            return best;
        }
        public const float RaceLineClearance = 12;
        // what the rule found and did, per tree (for the checks)
        public readonly List<string> DrivableReport = new();
        // FreeRoamWorld only (any Scenery setting): trunk colliders standing on a drivable surface are removed; their
        // places are remembered so the new scenery drops their crowns too.
        public static readonly List<(Vector3 bottom, float width, float height, string what, string name)> Cleared = new();
        public static void ClearFreeRoamTrunks(Scene scene)
        {
            Cleared.Clear(); if (scene.name != RaceFlow.RoamScene) return;
            var trails = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<RaceRoad>()).Where(t => t.forestTrail && t.points != null && t.points.Length > 1).ToList();
            foreach (var t in trails) t.Initialize();
            foreach (var box in scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<BoxCollider>()))
            {
                if (!IsTrunkCollider(box) || !box.enabled) continue;
                var t = box.transform; var bottom = t.TransformPoint(box.center - Vector3.up * box.size.y * .5f);
                if (!OnDrivable(bottom, trails, out var what)) continue;
                var size = Vector3.Scale(box.size, t.lossyScale);
                Cleared.Add((bottom, Mathf.Min(Mathf.Abs(size.x), Mathf.Abs(size.z)), Mathf.Abs(size.y), what, t.name));
                // a trunk object holds just its collider: remove the object; otherwise only the collider
                if (t.GetComponents<Component>().Length == 2 && t.childCount == 0) Destroy(t.gameObject); else Destroy(box);
                box.enabled = false;
            }
            if (Cleared.Count > 0) Debug.Log($"Scenery: removed {Cleared.Count} tree trunk(s) standing on drivable surfaces in {scene.name}");
        }

        // 0.84 Part K (BUG-001 "floating trees", ForestLoopReverse): later edits lowered the ground under some trees (the 0.31
        // pool basin and lake-exit bank, the 0.29 hill smoothing, the cuttings). The trunk colliders were lowered onto the
        // ground in the scene itself (Tools/Report084/Report084Trees.cs); what is only drawn is grounded here: a visual-only
        // trunk (bark box) hanging over the ground is set down on it, a crown-only clump hanging more than 0.5 m gets a trunk
        // down to the ground (a bush is set down), and one hanging over a trail or road is left out.
        // 0.95 Part E (BUG-007, Free Roam's floating trees): Free Roam is grounded the same way (its trunk colliders were set
        // down in the scene by Tools/Report095/Report095World.cs)
        static readonly HashSet<string> GroundedScenes = new() { "ForestLoopReverse", RaceFlow.RoamScene, "MountainLoop", "MountainLoopReverse" }; // 0.96 Part D: the Mountain scenes too (BUG-008)
        static bool TreeLike(Collider c) => c.name.IndexOf("trunk", System.StringComparison.OrdinalIgnoreCase) >= 0 || c.name.IndexOf("tree", System.StringComparison.OrdinalIgnoreCase) >= 0;
        static bool GroundBelow(Vector3 p, out float y)
        {
            y = 0; float best = float.MaxValue; bool any = false;
            foreach (var h in Physics.RaycastAll(p + Vector3.up * .5f, Vector3.down, 80, ~0, QueryTriggerInteraction.Ignore))
                if (!TreeLike(h.collider) && !h.collider.attachedRigidbody && h.distance < best) { best = h.distance; y = h.point.y; any = true; }
            return any;
        }
        public void Build(Scene scene, List<Renderer> hidden)
        {
            if (!LoadKit()) { enabled = false; return; }
            var roots = scene.GetRootGameObjects();
            // 1. old vegetation meshes, split into connected pieces (world bounds, mean colour)
            var pieces = new List<Piece>();
            foreach (var r in roots.SelectMany(g => g.GetComponentsInChildren<MeshRenderer>(true)))
            {
                if (!r.gameObject.activeInHierarchy || !r.enabled) continue;
                int mountain = MountainKind(r); if (mountain == 0 && !IsOldVegetation(r)) continue;
                var mesh = r.GetComponent<MeshFilter>().sharedMesh; Pieces(mesh, r.transform.localToWorldMatrix, pieces, mountain); hidden.Add(r);
            }
            // 2. bark boxes: a primitive cube's six faces are separate pieces; merge the touching ones into boxes
            var barkBoxes = MergeTouching(pieces.Where(p => p.bark).Select(p => p.b).ToList());
            // 3. trunks: every trunk collider, plus bark boxes that have no collider (visual-only trees stay visual-only)
            var trunks = new List<Trunk>();
            foreach (var box in roots.SelectMany(g => g.GetComponentsInChildren<BoxCollider>(true)))
            {
                if (!IsTrunkCollider(box) || !box.gameObject.activeInHierarchy || !box.enabled) continue;
                var t = box.transform; var size = Vector3.Scale(box.size, t.lossyScale); var up = t.up;
                trunks.Add(new Trunk { bottom = t.TransformPoint(box.center - Vector3.up * box.size.y * .5f), up = up, width = Mathf.Min(Mathf.Abs(size.x), Mathf.Abs(size.z)), height = Mathf.Abs(size.y), collider = true });
            }
            // trunks removed from FreeRoamWorld's drivable surfaces still take their crowns (which then go too)
            foreach (var c in Cleared) trunks.Add(new Trunk { bottom = c.bottom, up = Vector3.up, width = c.width, height = c.height, collider = true, cleared = true });
            var trunkGrid = new Dictionary<Vector2Int, List<int>>();
            Vector2Int G(Vector3 p, float s) => new(Mathf.FloorToInt(p.x / s), Mathf.FloorToInt(p.z / s));
            void Index(int i) { var k = G(trunks[i].bottom, 8); if (!trunkGrid.TryGetValue(k, out var l)) trunkGrid[k] = l = new(); l.Add(i); }
            for (int i = 0; i < trunks.Count; i++) Index(i);
            int Nearest(Vector3 p, float max, System.Func<Trunk, bool> ok)
            {
                int best = -1; float bd = max * max; var k = G(p, 8); int reach = Mathf.CeilToInt(max / 8);
                for (int x = -reach; x <= reach; x++) for (int z = -reach; z <= reach; z++)
                        if (trunkGrid.TryGetValue(new Vector2Int(k.x + x, k.y + z), out var l))
                            foreach (int i in l) { var d = trunks[i].bottom - p; d.y = 0; if (d.sqrMagnitude < bd && ok(trunks[i])) { bd = d.sqrMagnitude; best = i; } }
                return best;
            }
            foreach (var b in barkBoxes)
            {
                var bottom = new Vector3(b.center.x, b.min.y, b.center.z);
                if (Nearest(bottom, Mathf.Max(.4f, b.extents.x), _ => true) >= 0) continue;
                float h = b.size.y;
                // 0.84 Part K: a visual-only trunk hanging over lowered ground is set down on it (and reaches as high as before)
                if (GroundedScenes.Contains(scene.name) && GroundBelow(bottom, out float ground) && bottom.y - ground > .3f) { h += bottom.y - ground; bottom.y = ground; BarkSeated++; }
                trunks.Add(new Trunk { bottom = bottom, up = Vector3.up, width = Mathf.Min(b.size.x, b.size.z), height = h }); Index(trunks.Count - 1);
            }
            // 4. crowns to the nearest trunk whose top reaches into or near them
            var loose = new List<Bounds>();
            foreach (var p in pieces)
            {
                if (p.bark) continue;
                var c = new Vector3(p.b.center.x, p.b.min.y, p.b.center.z);
                int i = Nearest(c, Mathf.Max(2.5f, p.b.extents.x * .9f), t => t.bottom.y + t.height * 1.6f + 1 >= p.b.min.y && t.bottom.y < p.b.max.y);
                if (i < 0) { loose.Add(p.b); continue; }
                var t = trunks[i]; if (t.hasCrown) t.crown.Encapsulate(p.b); else { t.crown = p.b; t.hasCrown = true; }
                trunks[i] = t;
            }
            // 5. place the kit, except on drivable surfaces (0.79 Part E) and the race lines (0.85 Part A)
            SetRaceLines(scene);
            var trails = roots.SelectMany(g => g.GetComponentsInChildren<RaceRoad>()).Where(t => t.forestTrail && t.points != null && t.points.Length > 1).ToList();
            foreach (var t in trails) t.Initialize();
            bool roam = scene.name == RaceFlow.RoamScene; var lines = roam ? null : RaceLines(scene);
            // whether to leave a tree out: FreeRoamWorld's were already removed (collider too); in a course scene the
            // collider stays and only the visual goes, unless a race line passes near (then it stays and is reported)
            bool Skip(Vector3 bottom, bool collider, string kind, bool cleared, string clearedWhat)
            {
                string what = clearedWhat; if (!cleared && !OnDrivable(bottom, trails, out what)) return false;
                string at = $"{kind} at {bottom.x:F1},{bottom.y:F2},{bottom.z:F1} on {what}";
                if (cleared) { DrivableReport.Add("REMOVED (collider and visual) " + at); return true; }
                if (roam || !collider) { DrivableReport.Add("REMOVED (visual; no collider) " + at); return true; }
                float d = LineDistance(lines, bottom);
                if (d < RaceLineClearance) { DrivableReport.Add($"KEPT (collider {d:F1} m from a race line; course scene) " + at); return false; }
                DrivableReport.Add($"HIDDEN (visual only; collider kept, nearest race line {d:F0} m) " + at); return true;
            }
            foreach (var t in trunks)
            {
                float hash = Hash(t.bottom), yaw = hash * 360;
                string clearedWhat = t.cleared ? Cleared.FirstOrDefault(c => (c.bottom - t.bottom).sqrMagnitude < .01f).what : null;
                if (Skip(t.bottom, t.collider, t.collider ? "tree (trunk collider)" : "tree (bark box)", t.cleared, clearedWhat)) { DrivableSkipped++; continue; }
                if (!t.hasCrown) { AddTrunk(Young, t.bottom, t.up, t.width, t.height, yaw); TrunkOnly++; Placements.Add(new Placement { bottom = t.bottom, radius = t.width * .5f, height = t.height, variant = Young, source = t.collider ? "trunk collider, no crown" : "bark box, no crown" }); continue; }
                int v = Species(t, hash); PerVariant[v]++; TreeCount++;
                var cb = t.crown; float radius = Mathf.Max(cb.extents.x, cb.extents.z);
                AddTrunk(v, t.bottom, t.up, t.width, Mathf.Max(t.height, cb.min.y - t.bottom.y + cb.size.y * .25f), yaw);
                AddCrown(v, new Vector3(cb.center.x, cb.min.y, cb.center.z), radius, cb.size.y, yaw);
                Placements.Add(new Placement { bottom = t.bottom, radius = t.width * .5f, height = cb.max.y - t.bottom.y, variant = v, source = t.collider ? "trunk collider" : "bark box" });
            }
            // each crown without a trunk on its own (merging touching ones joined whole hillsides of overlapping mountain
            // foliage into one giant crown); a bush or clump down on a drivable surface is left out
            foreach (var b in loose)
            {
                float hash = Hash(b.center), radius = Mathf.Max(b.extents.x, b.extents.z);
                bool bush = b.size.y < 3.5f; var bottom = new Vector3(b.center.x, b.min.y, b.center.z);
                if (Skip(bottom, false, bush ? "bush" : "crown-only clump", false, null)) { DrivableSkipped++; continue; }
                int v = bush ? (hash < .5f ? Bush : Shrub) : Round; if (bush) Bushes++; else CrownOnly++;
                string source = bush ? "bush" : "crown-only clump";
                // 0.84 Part K: hanging more than 0.5 m above the ground (not over a drivable surface): a bush is set down on
                // the ground, a clump gets a trunk down to it
                float ground = 0; bool hanging = GroundedScenes.Contains(scene.name) && GroundBelow(bottom, out ground) && bottom.y - ground > .5f;
                // hanging over a trail or road (the ground under it was cut away): left out, as on a drivable surface
                if (hanging && OnDrivable(new Vector3(bottom.x, ground, bottom.z), trails, out var over)) { DrivableReport.Add($"REMOVED (visual; hanging {bottom.y - ground:F1} m over {over}) {(bush ? "bush" : "crown-only clump")} at {bottom.x:F1},{bottom.y:F2},{bottom.z:F1}"); DrivableSkipped++; if (bush) Bushes--; else CrownOnly--; continue; }
                if (hanging)
                {
                    if (bush) { bottom.y = ground; source = "bush (set down on the ground)"; }
                    else { AddTrunk(v, new Vector3(bottom.x, ground, bottom.z), Vector3.up, Mathf.Clamp(radius * .14f, .25f, .6f), bottom.y - ground + b.size.y * .25f, hash * 360); TrunksAdded++; source = "crown-only clump (trunk added)"; }
                }
                AddCrown(v, bottom, radius, b.size.y, hash * 360);
                Placements.Add(new Placement { bottom = bottom, radius = radius, height = b.size.y, variant = v, source = source });
            }
        }

        // Natural-looking mix: conifers where the old crowns were tall and narrow or on the mountain, oaks for the biggest
        // spreading crowns, young trees for small ones; the rest broadleaves, in patches so species cluster.
        static int Species(Trunk t, float hash)
        {
            var cb = t.crown; float radius = Mathf.Max(cb.extents.x, cb.extents.z), tall = cb.size.y / Mathf.Max(.1f, radius * 2);
            float patch = Mathf.PerlinNoise((t.bottom.x + 311) / 140, (t.bottom.z - 97) / 140);
            if (cb.max.y - t.bottom.y < 5.5f) return Young;
            if (tall > 1.25f) return patch < .38f || hash < .15f ? Pine : hash < .6f ? Poplar : Round;
            if (t.bottom.y > 60 && patch < .45f) return Pine;
            if (radius > 4.2f && hash < .55f) return Oak;
            return patch < .3f ? (hash < .4f ? Pine : Round) : patch > .62f ? (hash < .6f ? Maple : Oak) : (hash < .5f ? Round : Maple);
        }
        static float Hash(Vector3 p) => Mathf.Repeat(Mathf.Sin(p.x * 12.9898f + p.z * 78.233f) * 43758.5453f, 1);

        // Connected pieces of a mesh (triangles sharing vertex indices), with world bounds and mean vertex colour.
        // kind: 0 = by colour (bark brown, crowns green), 1 = all crowns, 2 = all bark.
        static void Pieces(Mesh mesh, Matrix4x4 toWorld, List<Piece> output, int kind = 0)
        {
            var verts = mesh.vertices; var colors = mesh.colors; var tris = mesh.triangles;
            var parent = new int[verts.Length]; for (int i = 0; i < parent.Length; i++) parent[i] = i;
            int Find(int x) { while (parent[x] != x) { parent[x] = parent[parent[x]]; x = parent[x]; } return x; }
            for (int i = 0; i < tris.Length; i += 3) { int a = Find(tris[i]), b = Find(tris[i + 1]), c = Find(tris[i + 2]); parent[b] = a; parent[Find(c)] = a; }
            var index = new Dictionary<int, int>(); var acc = new List<(Bounds b, Color c, int n)>();
            var used = new bool[verts.Length]; foreach (int t in tris) used[t] = true;
            for (int i = 0; i < verts.Length; i++)
            {
                if (!used[i]) continue;
                int root = Find(i); var w = toWorld.MultiplyPoint3x4(verts[i]); var col = colors.Length == verts.Length ? colors[i] : Color.gray;
                if (!index.TryGetValue(root, out int k)) { index[root] = k = acc.Count; acc.Add((new Bounds(w, Vector3.zero), col, 1)); }
                else { var e = acc[k]; e.b.Encapsulate(w); e.c += col; e.n++; acc[k] = e; }
            }
            foreach (var e in acc)
            {
                var c = e.c / e.n;
                output.Add(new Piece { b = e.b, c = c, verts = e.n, bark = kind == 2 || (kind == 0 && c.r > c.g * 1.05f) });
            }
        }
        // Unions boxes that touch (1 cm tolerance).
        static List<Bounds> MergeTouching(List<Bounds> boxes)
        {
            var parent = Enumerable.Range(0, boxes.Count).ToArray();
            int Find(int x) { while (parent[x] != x) { parent[x] = parent[parent[x]]; x = parent[x]; } return x; }
            var grid = new Dictionary<Vector2Int, List<int>>();
            for (int i = 0; i < boxes.Count; i++)
            {
                var b = boxes[i]; b.Expand(.02f);
                for (int x = Mathf.FloorToInt(b.min.x / 4); x <= Mathf.FloorToInt(b.max.x / 4); x++)
                    for (int z = Mathf.FloorToInt(b.min.z / 4); z <= Mathf.FloorToInt(b.max.z / 4); z++)
                    {
                        var k = new Vector2Int(x, z); if (!grid.TryGetValue(k, out var l)) grid[k] = l = new();
                        foreach (int j in l) { var o = boxes[j]; o.Expand(.02f); if (o.Intersects(b)) parent[Find(j)] = Find(i); }
                        l.Add(i);
                    }
            }
            var merged = new Dictionary<int, Bounds>();
            for (int i = 0; i < boxes.Count; i++) { int r = Find(i); if (merged.TryGetValue(r, out var m)) { m.Encapsulate(boxes[i]); merged[r] = m; } else merged[r] = boxes[i]; }
            return merged.Values.ToList();
        }

        // ---------------------------------------------------------------- drawing
        readonly HashSet<Vector2Int> nearKeys = new();
        // instanced draws in batches of at most 1000
        public static int Draw(RenderParams rp, Mesh mesh, List<Matrix4x4> list)
        {
            int n = 0; for (int start = 0; start < list.Count; start += 1000) { Graphics.RenderMeshInstanced(rp, mesh, 0, list, Mathf.Min(1000, list.Count - start), start); n++; }
            return n;
        }
        // Whether p is under a tree crown (for the ground detail: leaf litter instead of grass).
        public bool UnderCrown(Vector3 p)
        {
            var key = new Vector2Int(Mathf.FloorToInt(p.x / Cell), Mathf.FloorToInt(p.z / Cell)); if (!cells.TryGetValue(key, out var c)) return false;
            for (int v = 0; v < Bush; v++) foreach (var m in c.crowns[v]) { var q = m.GetColumn(3); float r = m.GetColumn(0).magnitude * .8f; float dx = q.x - p.x, dz = q.z - p.z; if (dx * dx + dz * dz < r * r) return true; }
            return false;
        }
        bool Overlaps(Vector2Int farKey)
        {
            foreach (var k in nearKeys) if (Mathf.FloorToInt(k.x * Cell / FarCell) == farKey.x && Mathf.FloorToInt(k.y * Cell / FarCell) == farKey.y) return true;
            return false;
        }
        // the far cell the camera is in: its trees except those the near cells drew
        void FarExcept(RenderParams rp, Vector2Int farKey, ref int calls)
        {
            var f = far[farKey]; var scratchBroad = f.sb; var scratchConifer = f.sc; var scratchTrunks = f.st; scratchBroad.Clear(); scratchConifer.Clear(); scratchTrunks.Clear();
            bool Skip(Matrix4x4 m) { var p = m.GetColumn(3); return nearKeys.Contains(new Vector2Int(Mathf.FloorToInt(p.x / Cell), Mathf.FloorToInt(p.z / Cell))); }
            foreach (var m in f.broad) if (!Skip(m)) scratchBroad.Add(m);
            foreach (var m in f.conifer) if (!Skip(m)) scratchConifer.Add(m);
            foreach (var m in f.trunks) if (!Skip(m)) scratchTrunks.Add(m);
            if (scratchBroad.Count > 0) { calls += Draw(rp, crownFar[Round], scratchBroad); }
            if (scratchConifer.Count > 0) { calls += Draw(rp, crownFar[Pine], scratchConifer); }
            if (scratchTrunks.Count > 0) { calls += Draw(rp, trunkFar[Round], scratchTrunks); }
        }
        void LateUpdate()
        {
            var cam = Camera.main; if (!cam || !material) return;
            var eye = cam.transform.position; float draw = Mathf.Min(DrawDistance, cam.farClipPlane + 100); int calls = 0;
            // 0.90 Part D: in split-screen the near detail and the far trees follow both views (the nearer one counts)
            float Distance(Bounds b) { if (SplitScreen.Eyes.Count == 0) return Mathf.Sqrt(b.SqrDistance(eye)); float d = float.MaxValue; foreach (var e in SplitScreen.Eyes) d = Mathf.Min(d, Mathf.Sqrt(b.SqrDistance(e))); return d; }
            var near = new RenderParams(material) { shadowCastingMode = ShadowCastingMode.On, receiveShadows = true, layer = 0 };
            var farParams = new RenderParams(material) { shadowCastingMode = ShadowCastingMode.Off, receiveShadows = true, layer = 0 };
            // near: the detailed kit per 96 m cell; the big far cells skip the trees those near cells already drew
            nearKeys.Clear();
            foreach (var pair in cells)
            {
                var c = pair.Value; if (!c.any || Distance(c.bounds) >= NearDistance) continue;
                nearKeys.Add(pair.Key); var rp = near; rp.worldBounds = c.bounds;
                for (int v = 0; v < Variants.Length; v++)
                {
                    if (c.crowns[v].Count > 0 && crownNear[v] is Mesh cm) { calls += Draw(rp, cm, c.crowns[v]); }
                    if (c.trunks[v].Count > 0 && trunkNear[v] is Mesh tm) { calls += Draw(rp, tm, c.trunks[v]); }
                }
            }
            foreach (var pair in far)
            {
                var f = pair.Value; float fd = Distance(f.bounds); if (!f.any || fd > draw) continue;
                var rp = farParams; rp.worldBounds = f.bounds; bool trunks = fd < FarTrunks; // far away the crowns hide the trunks
                if (nearKeys.Count == 0 || !Overlaps(pair.Key))
                {
                    if (f.broad.Count > 0) { calls += Draw(rp, crownFar[Round], f.broad); }
                    if (f.conifer.Count > 0) { calls += Draw(rp, crownFar[Pine], f.conifer); }
                    if (trunks && f.trunks.Count > 0) { calls += Draw(rp, trunkFar[Round], f.trunks); }
                }
                else FarExcept(rp, pair.Key, ref calls);
            }
            DrawCalls = calls;
        }
    }
}
