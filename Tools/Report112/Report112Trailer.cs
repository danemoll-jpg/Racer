using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// 0.112 Part D (editor tool, temporary): in every scene with the Phase 4 Trickum pavement jump,
//  1. the "Breakable - Jump approach sign" stands upright on two posts, grounded, no longer breakable (it was knocked flat by passing vehicles),
//     lettering "TRAILER JUMP / 78 - 96 mph / OPEN LANE ON RIGHT" on the depth-tested lettering material;
//  2. the jump is dressed as a parked flatbed trailer: deck material, side rails with reflectors, loading-ramp (gate) plates and hinge bars at both ends,
//     tie-down straps, tandem wheels at the sides, a hitch tongue. All dressing is visual only (no colliders); the drivable mesh and its collider are untouched.
// PROBE_OUT: report folder. Run: Run-Unity.ps1 -Method Report112Trailer.Main -NoGraphics
public static class Report112Trailer
{
    static readonly string[] Scenes = { "StreetLoopGreybox", "StreetLoopReverse", "DansBackyardForward", "DansBackyardReverse", "FreeRoamWorld", "ForestLoopReverse", "LakeWoods", "MountainLoop", "MountainLoopReverse" };
    const string Folder = "Assets/Track/ArcadeReview/Trailer";
    static readonly List<string> report = new();
    public static void Main()
    {
        string outDir = System.Environment.GetEnvironmentVariable("PROBE_OUT") ?? Path.GetTempPath();
        try
        {
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Track/ArcadeReview", "Trailer");
            foreach (var name in Scenes)
            {
                var scene = EditorSceneManager.OpenScene("Assets/Scenes/" + name + ".unity", OpenSceneMode.Single);
                string before = Census(); int changed = 0;
                foreach (var root in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(t => t.name == "Phase 4 - Connector Jump").ToArray())
                {
                    foreach (var mf in root.GetComponentsInChildren<MeshFilter>(true).Where(m => m.name.StartsWith("Takeoff -")).ToArray()) changed += Dress(name, root, mf);
                    foreach (var holder in root.GetComponentsInChildren<Transform>(true).Where(t => t.name == "Breakable - Jump approach sign").ToArray()) changed += Sign(name, holder);
                }
                foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(t => t.name == "Reverse supported roadworks transition").ToArray())
                { var mf = t.GetComponent<MeshFilter>(); var root = t.parent ? t.parent : t; if (mf) changed += Dress(name, root, mf); }
                string after = Census();
                report.Add($"{name}: changes {changed}; colliders before [{before}] after [{after}]");
                if (changed > 0) EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets();
        }
        catch (System.Exception e) { report.Add("ERROR " + e); }
        File.WriteAllLines(Path.Combine(outDir, "trailer112.txt"), report); EditorApplication.Exit(0);
    }
    // solid colliders (not triggers): count and a position sum, to show nothing solid moved
    static string Census()
    {
        var all = Object.FindObjectsByType<Collider>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        var solid = all.Where(c => !c.isTrigger).ToArray(); var sum = Vector3.zero; foreach (var c in solid) sum += c.bounds.center;
        return $"solid {solid.Length} (sum {sum.x:F1},{sum.y:F1},{sum.z:F1}), triggers {all.Length - solid.Length}";
    }
    static Material Mat(string name, Color c, float smooth = .3f, float metal = 0)
    {
        string path = Folder + "/" + name + ".mat"; var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (!m) { m = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(m, path); }
        m.color = c; if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smooth); if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", metal); EditorUtility.SetDirty(m); return m;
    }
    static Material Asphalt => AssetDatabase.FindAssets("t:Material Raised roadworks asphalt").Select(g => AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(g))).FirstOrDefault(m => m && m.name == "Raised roadworks asphalt");
    static float GroundY(Vector3 p, float fallback)
    {
        var hits = Physics.RaycastAll(p + Vector3.up * 30, Vector3.down, 70, ~0, QueryTriggerInteraction.Ignore).Where(h => h.collider.name.StartsWith("Ground_") || h.collider.GetComponent<Terrain>()).OrderBy(h => h.distance).ToArray();
        return hits.Length > 0 ? hits[0].point.y : fallback;
    }

    // ------------------------------------------------------------------ the sign
    static int Sign(string scene, Transform holder)
    {
        var board = holder.Find("Jump approach sign"); if (!board) { report.Add($"  {scene}: sign holder without board"); return 0; }
        var text = holder.GetComponentInChildren<TextMesh>(true);
        string was = $"holder {holder.position:F2} rot {holder.eulerAngles:F1}, board {board.position:F2} rot {board.eulerAngles:F1}";
        foreach (var b in holder.GetComponents<Racer.BreakableProp>()) Object.DestroyImmediate(b, true);
        foreach (var c in holder.GetComponents<Collider>()) Object.DestroyImmediate(c, true);
        foreach (var c in board.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(c, true);
        holder.name = "Jump approach sign (posts)";
        // upright, facing back along the jump (toward the traffic it is for); pivot on the ground
        var root = holder.parent; var up = Quaternion.LookRotation(Vector3.ProjectOnPlane(root.forward, Vector3.up));
        var p = holder.position; p.y = GroundY(p, p.y); holder.SetPositionAndRotation(p, up);
        board.localRotation = Quaternion.identity; board.localPosition = new Vector3(0, 3.1f, 0);
        var mat = board.GetComponent<Renderer>().sharedMaterial;
        foreach (var old in holder.Cast<Transform>().Where(t => t.name == "Sign post").ToArray()) Object.DestroyImmediate(old.gameObject);
        foreach (float x in new[] { -2.8f, 2.8f })
        {
            var post = GameObject.CreatePrimitive(PrimitiveType.Cube); post.name = "Sign post"; Object.DestroyImmediate(post.GetComponent<Collider>());
            post.transform.SetParent(holder, false); post.transform.localPosition = new Vector3(x, 1.6f, .2f); post.transform.localScale = new Vector3(.18f, 3.2f, .18f);
            post.GetComponent<Renderer>().sharedMaterial = Mat("Sign post steel", new Color(.42f, .43f, .45f), .4f, .6f);
        }
        if (!holder.GetComponent<Racer.PhysicalSign>()) holder.gameObject.AddComponent<Racer.PhysicalSign>();
        if (text)
        {
            text.text = "TRAILER JUMP\n78 - 96 mph\nOPEN LANE ON RIGHT";
            text.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Environment/Phase8/Depth tested world lettering 0.mat");
            // fit inside the 8 x 3.5 board with a margin (the text is a child of the scaled board: measure in world units)
            text.transform.localRotation = Quaternion.identity;
            var b = text.GetComponent<Renderer>().bounds.size; float w = Mathf.Max(.01f, Mathf.Abs(Vector3.Dot(b, holder.right))), h = Mathf.Max(.01f, b.y);
            text.characterSize *= Mathf.Min(7f / w, 2.8f / h);
        }
        report.Add($"  {scene}: sign was {was}; now upright at {holder.position:F2} heading {holder.eulerAngles.y:F1}, board centre {board.position:F2}, active {holder.gameObject.activeInHierarchy}, text '{text?.text.Replace("\n", " / ")}' size {text?.GetComponent<Renderer>().bounds.size:F2}");
        return 1;
    }

    // ------------------------------------------------------------------ the trailer
    sealed class Parts { public readonly List<CombineInstance> list = new(); }
    static void Box(Dictionary<Material, Parts> parts, Material m, Matrix4x4 frame, Vector3 a, Vector3 b, Vector3 side, float width, float height)
    {
        // a box from a to b (frame space), `width` along `side`, `height` along the normal of (b-a) x side
        var along = b - a; var up = Vector3.Cross(along.normalized, side).normalized; if (up.y < 0) up = -up;
        var rot = Quaternion.LookRotation(along.normalized, up);
        var mesh = Cube(); var t = Matrix4x4.TRS((a + b) * .5f, rot, new Vector3(width, height, along.magnitude));
        if (!parts.TryGetValue(m, out var p)) parts[m] = p = new Parts();
        p.list.Add(new CombineInstance { mesh = mesh, transform = frame * t });
    }
    static void Cylinder(Dictionary<Material, Parts> parts, Material m, Matrix4x4 frame, Vector3 centre, float radius, float width)
    {
        var mesh = Cyl(); var t = Matrix4x4.TRS(centre, Quaternion.Euler(0, 0, 90), new Vector3(radius * 2, width * .5f, radius * 2));
        if (!parts.TryGetValue(m, out var p)) parts[m] = p = new Parts();
        p.list.Add(new CombineInstance { mesh = mesh, transform = frame * t });
    }
    static Mesh cube, cyl;
    static Mesh Cube() { if (!cube) { var g = GameObject.CreatePrimitive(PrimitiveType.Cube); cube = g.GetComponent<MeshFilter>().sharedMesh; Object.DestroyImmediate(g); } return cube; }
    static Mesh Cyl() { if (!cyl) { var g = GameObject.CreatePrimitive(PrimitiveType.Cylinder); cyl = g.GetComponent<MeshFilter>().sharedMesh; Object.DestroyImmediate(g); } return cyl; }

    static int Dress(string scene, Transform root, MeshFilter mf)
    {
        var mesh = mf.GetComponent<MeshCollider>() && mf.GetComponent<MeshCollider>().sharedMesh ? mf.GetComponent<MeshCollider>().sharedMesh : mf.sharedMesh; if (!mesh || mesh.vertexCount % 4 != 0 || mesh.vertexCount < 40) { report.Add($"  {scene}: {mf.name} mesh {(mesh ? mesh.vertexCount.ToString() : "none")} vertices: not the Trickum strip, skipped"); return 0; }
        var holder = mf.transform.Find("Trailer dressing"); if (holder) Object.DestroyImmediate(holder.gameObject);
        // the strip in its own (renderer) space: row r = vertices 4r..4r+3 across (base, top edge, top edge, base)
        var v = mesh.vertices; int rows = v.Length / 4;
        Vector3 V(int r, int j) => v[r * 4 + j];
        float H(int r) => (V(r, 1).y + V(r, 2).y) * .5f - (V(r, 0).y + V(r, 3).y) * .5f;
        if (!Enumerable.Range(0, rows).Any(r => H(r) > .6f)) { report.Add($"  {scene}: {mf.name} ({mf.sharedMesh.name}, {rows} rows): no raised hump in this strip (flat), skipped"); return 0; }
        float maxH = Enumerable.Range(0, rows).Max(H); int peak = Enumerable.Range(0, rows).First(r => H(r) == maxH);
        int first = Enumerable.Range(0, rows).First(r => H(r) > .12f), last = Enumerable.Range(0, rows).Last(r => H(r) > .12f);
        // the deck = where the surface is above 60% of the peak; the loading ramps (gates) = the rest of the hump on each side
        int deckA = Enumerable.Range(0, rows).First(r => H(r) > .6f * maxH), deckB = Enumerable.Range(0, rows).Last(r => H(r) > .6f * maxH);
        var steel = Mat("Trailer steel", new Color(.16f, .17f, .19f), .35f, .55f); var plate = Mat("Trailer ramp plate", new Color(.62f, .64f, .66f), .55f, .8f);
        var tire = Mat("Trailer tire", new Color(.05f, .05f, .05f), .15f); var hub = Mat("Trailer hub", new Color(.7f, .7f, .72f), .6f, .8f);
        var red = Mat("Trailer reflector red", new Color(.85f, .06f, .05f), .7f); var amber = Mat("Trailer reflector amber", new Color(1f, .55f, .05f), .7f); var strap = Mat("Trailer strap", new Color(.95f, .78f, .08f), .2f);
        var deckMat = Mat("Trailer deck", new Color(.34f, .29f, .23f), .2f);
        var parts = new Dictionary<Material, Parts>(); var frame = Matrix4x4.identity;
        Vector3 side = (V(0, 3) - V(0, 0)).normalized; // across the strip, base 0 -> base 3
        Vector3 Top(int r, int j, float up = 0, float outward = 0) => V(r, j) + Vector3.up * up + side * (j == 1 ? -outward : outward);
        // side rails along both top edges, with reflectors (red toward the approach, amber along the sides)
        for (int j = 1; j <= 2; j++)
            for (int r = first; r < last; r += 2)
            {
                int n = Mathf.Min(r + 2, last); Box(parts, steel, frame, Top(r, j, .16f, .06f), Top(n, j, .16f, .06f), side, .1f, .22f);
                if ((r - first) % 12 == 0) { var at = Top(r, j, .16f, .13f); Box(parts, r < peak ? red : amber, frame, at, at + (V(n, j) - V(r, j)).normalized * .16f, side, .03f, .1f); }
            }
        // the loading ramps (gates down): two plate tracks on each ramp and a hinge bar where each meets the deck
        float wTop = Vector3.Distance(V(0, 1), V(0, 2));
        foreach (var (a, b) in new[] { (first, deckA), (deckB, last) })
            foreach (float f in new[] { .22f, .78f })
                for (int r = a; r < b; r += 2) { int n = Mathf.Min(r + 2, b); Vector3 P(int q) => Vector3.Lerp(V(q, 1), V(q, 2), f) + Vector3.up * .02f; Box(parts, plate, frame, P(r), P(n), side, wTop * .22f, .02f); }
        foreach (int r in new[] { deckA, deckB }) { int n = Mathf.Min(r + 1, rows - 1); var c = (V(r, 1) + V(r, 2)) * .5f + Vector3.up * .03f; var dir = (V(n, 1) - V(r, 1)).normalized; Box(parts, steel, frame, V(r, 1) + Vector3.up * .03f, V(r, 2) + Vector3.up * .03f, dir, .25f, .04f); }
        // two tie-down straps across the deck either side of the peak
        foreach (int r in new[] { Mathf.Max(deckA, peak - 8), Mathf.Min(deckB, peak + 8) }) Box(parts, strap, frame, V(r, 1) + Vector3.up * .025f, V(r, 2) + Vector3.up * .025f, (V(r + 1, 1) - V(r, 1)).normalized, .09f, .025f);
        // tandem wheels at the foot of each side under the deck (inside the bevel, outside the driving surface)
        var mid = (deckA + deckB) / 2; float spacing = Vector3.Distance(V(mid, 0), V(mid + 1, 0));
        int gap = Mathf.Max(1, Mathf.RoundToInt(1.3f / Mathf.Max(.05f, spacing)));
        foreach (int r in new[] { mid - gap, mid + gap })
            foreach (int j in new[] { 0, 3 })
            {
                var foot = V(r, j); var inward = (j == 0 ? side : -side); var c = foot + inward * .12f + Vector3.up * .5f;
                Cylinder(parts, tire, frame, c, .5f, .34f); Cylinder(parts, hub, frame, c - inward * .02f, .26f, .36f);
            }
        // the hitch tongue: a low A-frame on the road beyond the far end of the hump, and its coupler
        int end = Mathf.Min(rows - 1, last + 2); var e1 = V(end, 1); var e2 = V(end, 2); var ahead = (V(end, 1) - V(end - 1, 1)); ahead.y = 0; ahead.Normalize();
        var tip = (e1 + e2) * .5f + ahead * 2.6f; tip.y = (V(end, 0).y + V(end, 3).y) * .5f + .12f; var l = e1; l.y = tip.y; var rr = e2; rr.y = tip.y;
        Box(parts, steel, frame, l + side * .4f, tip, Vector3.up, .1f, .1f); Box(parts, steel, frame, rr - side * .4f, tip, Vector3.up, .1f, .1f); Box(parts, steel, frame, tip, tip + ahead * .35f, side, .16f, .14f);
        // build: one renderer per material under the takeoff (no colliders), meshes saved per scene
        holder = new GameObject("Trailer dressing").transform; holder.SetParent(mf.transform, false);
        int count = 0;
        foreach (var kv in parts)
        {
            var m = new Mesh { name = "Trailer " + kv.Key.name, indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 }; m.CombineMeshes(kv.Value.list.ToArray(), true, true); m.RecalculateBounds();
            string path = $"{Folder}/{scene}-{mf.name.Split(' ')[0]}-{kv.Key.name.Replace(' ', '-')}.asset"; var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing) { EditorUtility.CopySerialized(m, existing); m = existing; } else AssetDatabase.CreateAsset(m, path);
            var g = new GameObject(kv.Key.name); g.transform.SetParent(holder, false); g.AddComponent<MeshFilter>().sharedMesh = m; g.AddComponent<MeshRenderer>().sharedMaterial = kv.Key; count += kv.Value.list.Count;
        }
        // the look only: a copy of the strip with the deck (top) and the sides (bevels) as two parts, deck boards on top and steel sides; the collider keeps the original mesh
        var collider = mf.GetComponent<MeshCollider>(); var source = collider && collider.sharedMesh ? collider.sharedMesh : mesh;
        // the flat lead-in and run-out (no rise) keep the road's raised-asphalt look
        var tris = source.triangles; var top = new List<int>(); var sides = new List<int>(); var flat = new List<int>();
        for (int k = 0; k < tris.Length / 6; k++) { int row = k / 3; bool level = H(row) < .12f && H(Mathf.Min(row + 1, rows - 1)) < .12f; (level ? flat : k % 3 == 1 ? top : sides).AddRange(tris.Skip(k * 6).Take(6)); }
        var visual = new Mesh { name = source.name + " (trailer look)" }; visual.SetVertices(source.vertices); visual.subMeshCount = 3; visual.SetTriangles(top, 0); visual.SetTriangles(sides, 1); visual.SetTriangles(flat, 2); visual.RecalculateNormals(); visual.RecalculateBounds();
        string vpath = $"{Folder}/{scene}-{mf.name.Split(' ')[0]}-look.asset"; var vexisting = AssetDatabase.LoadAssetAtPath<Mesh>(vpath);
        if (vexisting) { EditorUtility.CopySerialized(visual, vexisting); visual = vexisting; } else AssetDatabase.CreateAsset(visual, vpath);
        var rend = mf.GetComponent<MeshRenderer>(); string oldMat = rend.sharedMaterial ? rend.sharedMaterial.name : "none"; mf.sharedMesh = visual; rend.sharedMaterials = new[] { deckMat, Mat("Trailer side", new Color(.30f, .33f, .37f), .4f, .5f), Asphalt };
        report.Add($"  {scene}: {mf.name} under '{root.name}' ({rows} rows, peak {maxH:F2} m at row {peak}, hump rows {first}-{last}, deck rows {deckA}-{deckB}): {count} dressing pieces in {parts.Count} meshes, no colliders; deck material {oldMat} -> {deckMat.name}; renderer mesh -> {visual.name}; collider mesh {collider?.sharedMesh?.name} ({collider?.sharedMesh?.vertexCount} vertices) unchanged; active {mf.gameObject.activeInHierarchy}");
        return 1;
    }
}
