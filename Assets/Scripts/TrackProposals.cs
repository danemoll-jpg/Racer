using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Racer
{
    // 0.114 Part B: two combined-course proposals (Street Loop + Mountain) marked in FreeRoamWorld for Dan to drive. The centrelines come
    // from Tools/Report114/py/proposals.py (roads that exist in FreeRoamWorld only) in Resources/TrackProposals/TrackProposals.json.
    // Markers, visual only (no collider, no shadows): a tall post every 25 m on the right of the course with an arrow on top pointing the
    // way and a board facing the driver, A bright orange, B bright cyan; a taller post with a numbered board at each corner; a checkered
    // arch at the start; a yellow post and arrow at each shortcut entrance. The minimap and the big map draw the lines (RacingMiniMap,
    // ExplorationMap). Settings > Display > "Show track proposals" (default on) hides them all. Nothing in the world is changed.
    public sealed class TrackProposals : MonoBehaviour
    {
        [Serializable] public sealed class Corner { public int n, i; public float turn; }
        [Serializable] public sealed class Shortcut { public string title; public float[] pts; }
        [Serializable] public sealed class Proposal { public string id, title, notes; public float[] color, pts, start; public float length; public Corner[] corners; public Shortcut[] shortcuts; }
        [Serializable] public sealed class Data { public string version, source; public Proposal[] proposals; }

        public static TrackProposals Current { get; private set; }
        public static readonly Color Yellow = new(1f, .9f, .1f);
        static Data data;
        public static Data Plan { get { if (data == null) { var t = Resources.Load<TextAsset>("TrackProposals/TrackProposals"); data = t ? JsonUtility.FromJson<Data>(t.text) : new Data(); } return data; } }
        public static Color ColorOf(Proposal p) => new(p.color[0], p.color[1], p.color[2]);
        public static Vector3[] Points(float[] f) { var r = new Vector3[f.Length / 3]; for (int i = 0; i < r.Length; i++) r[i] = new Vector3(f[3 * i], f[3 * i + 1], f[3 * i + 2]); return r; }
        // the maps: the lines to draw (each proposal in its colour, the shortcuts in yellow) while the markers are shown
        // the maps' side-by-side offset: a unit normal that does not flip with the direction of travel (B drives the Street Loop backwards)
        public static Vector2 Beside(Vector2 d) { var n = new Vector2(-d.y, d.x).normalized; return n.x < 0 || (n.x == 0 && n.y < 0) ? -n : n; }
        public static bool Showing => Current && Current.root && Current.root.activeSelf;
        public readonly List<(Vector3[] pts, Color color, bool shortcut, float side)> Lines = new(); // side: drawn beside each other where A and B share a road (-1 A, +1 B, 0 shortcuts)
        public readonly List<string> Report = new();
        public int Posts { get; private set; }

        public static TrackProposals Attach(GameObject host) => host.GetComponent<TrackProposals>() ?? host.AddComponent<TrackProposals>();
        GameObject root; RaceFlow flow;
        static Material lettering; static Font font;
        readonly Dictionary<Color, (List<Vector3> v, List<int> t)> meshes = new();
        readonly List<(MeshRenderer mr, float w, float h)> labels = new();

        void Start()
        {
            if (gameObject.scene.name != RaceFlow.RoamScene || Plan.proposals == null) { enabled = false; return; }
            Current = this; flow = FindAnyObjectByType<RaceFlow>(); var watch = System.Diagnostics.Stopwatch.StartNew(); Physics.SyncTransforms();
            lettering ??= Resources.Load<Material>("Scenery/SignLettering"); font ??= Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (lettering && font && font.material && font.material.mainTexture) lettering.mainTexture = font.material.mainTexture;
            root = new GameObject("Track proposals (0.114)"); SceneManager.MoveGameObjectToScene(root, gameObject.scene);
            foreach (var p in Plan.proposals) Build(p);
            foreach (var kv in meshes)
            {
                var g = new GameObject("Proposal markers " + ColorUtility.ToHtmlStringRGB(kv.Key), typeof(MeshFilter), typeof(MeshRenderer)); g.transform.SetParent(root.transform, false);
                var m = new Mesh { name = g.name, indexFormat = kv.Value.v.Count > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16 }; m.SetVertices(kv.Value.v); m.SetTriangles(kv.Value.t, 0); m.RecalculateNormals(); m.RecalculateBounds();
                g.GetComponent<MeshFilter>().sharedMesh = m; var r = g.GetComponent<MeshRenderer>();
                var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit")) { name = g.name }; mat.SetColor("_BaseColor", kv.Key); r.sharedMaterial = mat;
                r.shadowCastingMode = ShadowCastingMode.Off; r.receiveShadows = false;
            }
            meshes.Clear();
            Report.Add($"built in {watch.ElapsedMilliseconds} ms: {Plan.proposals.Length} proposals, {Posts} posts, {labels.Count} numbered boards");
            Debug.Log("Track proposals: " + Report[^1]);
            Apply();
        }
        void OnDestroy() { if (Current == this) Current = null; }
        bool Wanted => !flow || flow.Save == null || flow.Save.Settings.showTrackProposals;
        void Update() { if (root && root.activeSelf != Wanted) Apply(); }
        void Apply() { if (root) root.SetActive(Wanted); }
        void LateUpdate()
        {
            for (int i = labels.Count - 1; i >= 0; i--)
            {
                var (mr, w, h) = labels[i]; if (!mr) { labels.RemoveAt(i); continue; }
                var b = mr.localBounds; if (b.size.x < 1e-4f) continue;
                mr.transform.localScale = Vector3.one * Mathf.Min(h / b.size.y, w / b.size.x); labels.RemoveAt(i);
            }
        }

        // ---------------------------------------------------------------- the markers
        void Build(Proposal p)
        {
            var col = ColorOf(p); var pts = Points(p.pts); var s = Stations(pts);
            Lines.Add((pts, col, false, p.id == "A" ? -1 : 1)); foreach (var sc in p.shortcuts ?? Array.Empty<Shortcut>()) Lines.Add((Points(sc.pts), Yellow, true, 0));
            var cornerAt = new HashSet<int>(); foreach (var c in p.corners ?? Array.Empty<Corner>()) cornerAt.Add(c.i);
            // a post every 25 m (none within 12 m of a numbered corner, which has its own)
            for (float d = 10; d < s[^1] - 5; d += 25)
            {
                int i = Index(s, d); bool near = false; foreach (var c in p.corners ?? Array.Empty<Corner>()) if (Mathf.Abs(s[c.i] - d) < 12) near = true;
                if (!near) Post(pts, i, col, 6.5f, null);
            }
            foreach (var c in p.corners ?? Array.Empty<Corner>()) Post(pts, c.i, col, 9f, p.id + c.n);
            // the start: a checkered arch across the course
            Arch(pts, p.id == "A" ? 0 : 2, col); // B's 4 m on, so the two arches at the shared start do not overlap
            // each shortcut's entrance: a yellow post with its arrow into the shortcut, on the side it leaves
            foreach (var sc in p.shortcuts ?? Array.Empty<Shortcut>())
            {
                var q = Points(sc.pts); if (q.Length < 3) continue;
                int k = 0; float best = float.MaxValue; for (int i = 0; i < pts.Length; i++) { float dd = (Flat(pts[i]) - Flat(q[0])).sqrMagnitude; if (dd < best) { best = dd; k = i; } }
                var main = Dir(pts, k); var into = Flat(q[Mathf.Min(q.Length - 1, 6)] - q[0]).normalized; float side = Vector3.Dot(into, Vector3.Cross(Vector3.up, main)) >= 0 ? 1 : -1;
                var at = Flat(pts[k]) + Vector3.Cross(Vector3.up, main) * side * Off(pts[k]);
                Mast(at, Ground(at, pts[k].y), into.sqrMagnitude > .1f ? into : main, Yellow, 8f, sc.title);
            }
        }
        void Post(Vector3[] pts, int i, Color col, float h, string number)
        {
            var f = Dir(pts, i); var right = Vector3.Cross(Vector3.up, f); var at = Flat(pts[i]) + right * Off(pts[i]);
            Mast(at, Ground(at, pts[i].y), f, col, h, number);
        }
        // a post h high at `at`, an arrow on top pointing `f`, a board facing the oncoming driver (with lettering if given)
        void Mast(Vector3 at, float g, Vector3 f, Color col, float h, string text)
        {
            f.y = 0; f.Normalize(); var right = Vector3.Cross(Vector3.up, f);
            Box(new Vector3(at.x, g - .3f, at.z), f, new Vector3(.22f, h + .3f, .22f), col);
            // the arrow: a flat chevron 3 m long, 0.25 m thick, at the top
            var top = new Vector3(at.x, g + h, at.z); Arrow(top, f, col);
            // the board, 1.2 m below the top, facing back down the course
            var bc = top - Vector3.up * 1.6f; float bw = text == null ? 1.0f : 1.8f, bh = text == null ? .7f : 1.3f;
            Box(bc - Vector3.up * bh * .5f - f * .14f, f, new Vector3(bw, bh, .1f), col);
            if (text != null && lettering && font)
            {
                Box(bc - Vector3.up * bh * .5f + Vector3.up * .08f - f * .2f, f, new Vector3(bw - .2f, bh - .16f, .02f), Color.white);
                var t = new GameObject("Proposal " + text).AddComponent<TextMesh>(); t.transform.SetParent(root.transform, false);
                t.font = font; t.text = text; t.anchor = TextAnchor.MiddleCenter; t.alignment = TextAlignment.Center; t.fontSize = 96; t.characterSize = .1f; t.color = new Color(.05f, .05f, .06f);
                t.transform.SetPositionAndRotation(bc - f * .23f, Quaternion.LookRotation(f)); // read from behind the post, by the driver coming up to it
                var mr = t.GetComponent<MeshRenderer>(); mr.sharedMaterial = lettering; mr.shadowCastingMode = ShadowCastingMode.Off; mr.receiveShadows = false;
                labels.Add((mr, bw - .4f, bh - .4f));
            }
            Posts++;
        }
        void Arrow(Vector3 c, Vector3 f, Color col)
        {
            var r = Vector3.Cross(Vector3.up, f); var u = Vector3.up * .25f;
            Vector3[] q = { c + f * 1.6f, c - f * 1.4f + r * 1.1f, c - f * .6f, c - f * 1.4f - r * 1.1f };
            // top and bottom faces (two triangles each), and the four sides
            Tri(q[0] + u, q[1] + u, q[2] + u, col, Vector3.up); Tri(q[0] + u, q[2] + u, q[3] + u, col, Vector3.up);
            Tri(q[0], q[1], q[2], col, Vector3.down); Tri(q[0], q[2], q[3], col, Vector3.down);
            for (int k = 0; k < 4; k++) { var a = q[k]; var b = q[(k + 1) % 4]; Quad(a, b, b + u, a + u, col); }
        }
        void Arch(Vector3[] pts, int i, Color col)
        {
            var f = Dir(pts, i); var right = Vector3.Cross(Vector3.up, f); var c = Flat(pts[i]); float half = 7.5f;
            float gl = Ground(c - right * half, pts[i].y), gr = Ground(c + right * half, pts[i].y), top = Mathf.Max(gl, gr) + 6f;
            Box(new Vector3(c.x, gl - .3f, c.z) - right * half, f, new Vector3(.4f, top - gl + .3f, .4f), col);
            Box(new Vector3(c.x, gr - .3f, c.z) + right * half, f, new Vector3(.4f, top - gr + .3f, .4f), col);
            // a checkered banner: 2 rows of 0.9 m squares across the course, both faces
            int n = Mathf.RoundToInt(2 * half / .9f); float w = 2 * half / n;
            for (int row = 0; row < 2; row++) for (int k = 0; k < n; k++)
                {
                    var mid = c + right * (-half + (k + .5f) * w); var bottom = new Vector3(mid.x, top - .9f * (row + 1), mid.z);
                    Box(bottom, f, new Vector3(w, .9f, .08f), (row + k) % 2 == 0 ? Color.white : new Color(.04f, .04f, .05f));
                }
            Box(new Vector3(c.x, top, c.z), f, new Vector3(2 * half + .4f, .3f, .2f), col);
        }

        // ---------------------------------------------------------------- helpers
        static Vector3 Flat(Vector3 v) { v.y = 0; return v; }
        static float Off(Vector3 p) => p.z > 520 ? 11.5f : 7.5f; // from the centreline: clear of the road (Hwy 92 is wider)
        static float[] Stations(Vector3[] p) { var s = new float[p.Length]; for (int i = 1; i < p.Length; i++) s[i] = s[i - 1] + Vector3.Distance(Flat(p[i]), Flat(p[i - 1])); return s; }
        static int Index(float[] s, float d) { int lo = 0, hi = s.Length - 1; while (lo + 1 < hi) { int m = (lo + hi) / 2; if (s[m] <= d) lo = m; else hi = m; } return lo; }
        static Vector3 Dir(Vector3[] p, int i) { var d = Flat(p[Mathf.Min(p.Length - 1, i + 3)] - p[Mathf.Max(0, i - 3)]); return d.sqrMagnitude > 1e-4f ? d.normalized : Vector3.forward; }
        static readonly RaycastHit[] hits = new RaycastHit[32];
        // the ground under a marker: the highest terrain within 25 m of the course's height there (never a roof or a tree)
        static float Ground(Vector3 p, float y)
        {
            int n = Physics.RaycastNonAlloc(new Vector3(p.x, y + 25, p.z), Vector3.down, hits, 60, ~0, QueryTriggerInteraction.Ignore); float best = float.NaN;
            for (int i = 0; i < n; i++)
            {
                var c = hits[i].collider; if (!c || c.attachedRigidbody) continue;
                if (!c.name.StartsWith("Ground_")) continue;
                if (float.IsNaN(best) || hits[i].point.y > best) best = hits[i].point.y;
            }
            return float.IsNaN(best) ? y : best;
        }
        (List<Vector3> v, List<int> t) M(Color c) { if (!meshes.TryGetValue(c, out var m)) meshes[c] = m = (new List<Vector3>(), new List<int>()); return m; }
        void Tri(Vector3 a, Vector3 b, Vector3 d, Color col, Vector3 facing)
        {
            var m = M(col); if (Vector3.Dot(Vector3.Cross(b - a, d - a), facing) < 0) (b, d) = (d, b);
            int i = m.v.Count; m.v.Add(a); m.v.Add(b); m.v.Add(d); m.t.Add(i); m.t.Add(i + 1); m.t.Add(i + 2);
        }
        // a quad seen from both sides (thin parts: the arrow's edges)
        void Quad(Vector3 a, Vector3 b, Vector3 d, Vector3 e, Color col)
        {
            var m = M(col); int i = m.v.Count; m.v.Add(a); m.v.Add(b); m.v.Add(d); m.v.Add(e);
            m.t.AddRange(new[] { i, i + 1, i + 2, i, i + 2, i + 3, i, i + 2, i + 1, i, i + 3, i + 2 });
        }
        void Box(Vector3 bottom, Vector3 forward, Vector3 size, Color col)
        {
            forward.y = 0; forward.Normalize(); var right = Vector3.Cross(Vector3.up, forward); var m = M(col);
            Vector3 P(float x, float y, float z) => bottom + right * (x * size.x * .5f) + Vector3.up * (y * size.y) + forward * (z * size.z * .5f);
            void F(Vector3 a, Vector3 b, Vector3 d, Vector3 e, Vector3 o)
            {
                if (Vector3.Dot(Vector3.Cross(b - a, d - a), o) < 0) (b, e) = (e, b);
                int i = m.v.Count; m.v.Add(a); m.v.Add(b); m.v.Add(d); m.v.Add(e); m.t.AddRange(new[] { i, i + 1, i + 2, i, i + 2, i + 3 });
            }
            F(P(-1, 0, 1), P(1, 0, 1), P(1, 1, 1), P(-1, 1, 1), forward); F(P(1, 0, -1), P(-1, 0, -1), P(-1, 1, -1), P(1, 1, -1), -forward);
            F(P(1, 0, 1), P(1, 0, -1), P(1, 1, -1), P(1, 1, 1), right); F(P(-1, 0, -1), P(-1, 0, 1), P(-1, 1, 1), P(-1, 1, -1), -right);
            F(P(-1, 1, 1), P(1, 1, 1), P(1, 1, -1), P(-1, 1, -1), Vector3.up); F(P(-1, 0, -1), P(1, 0, -1), P(1, 0, 1), P(-1, 0, 1), Vector3.down);
        }
    }
}
