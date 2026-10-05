using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Racer
{
    // 0.79 Part G: standard US street-name signs at the junctions of the world's three named roads (Dan, 2026-10-05):
    // S Cherokee Ln (Dan's road, all the way to Trickum Rd: no Jamerson signs), Hwy 92 (the four-lane road) and Trickum
    // Rd. Each sign is a steel post at a corner of the junction with two green blades with white lettering at right
    // angles, each blade parallel to the road it names, lettered on both faces. Built at load in every scene (the same
    // world everywhere), whatever the Scenery setting. No collider. Placement reads the route data first (section 5A):
    // the junction is found from the Street Loop route (the loop runs on exactly these three roads), the post stands on
    // the verge 2.5 m beyond the paved edge measured on the spot, at the corner that is off every drivable surface,
    // clear of every course's race lines and shortcuts and of anything else standing there.
    public sealed class StreetSigns : MonoBehaviour
    {
        public const string Cherokee = "S Cherokee Ln", Hwy92 = "Hwy 92", Trickum = "Trickum Rd";
        // where the Street Loop turns from one named road onto the other (forward direction), roughly
        // and which of the two is the side road meeting the other at a T (the post goes on its corners): S Cherokee Ln
        // leaves Hwy 92 southward; Trickum Rd meets Hwy 92 from the south; Trickum Rd leaves S Cherokee Ln (which runs on
        // west as the Jamerson continuation) northward
        static readonly (Vector3 near, string incoming, string outgoing, bool stemOutgoing)[] Junctions =
        {
            (new Vector3(315, 0, 550), Hwy92, Cherokee, true),
            (new Vector3(-605, 0, 540), Trickum, Hwy92, false),
            (new Vector3(-614, 0, -544), Cherokee, Trickum, true),
        };
        public const float Verge = 2.5f, PostHeight = 3.5f, BladeLength = 1.9f, BladeHeight = .4f, LineClearance = 6.5f;
        public readonly List<string> Report = new();
        public readonly List<(Vector3 post, string lower, string upper, Vector3 centre)> Placed = new();
        static Material body, lettering;

        public static StreetSigns Attach(GameObject host) => host.GetComponent<StreetSigns>() ?? host.AddComponent<StreetSigns>();
        void Start() => Build(gameObject.scene);

        public void Build(Scene scene)
        {
            body ??= Resources.Load<Material>("Scenery/Building"); lettering ??= Resources.Load<Material>("Scenery/SignLettering");
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (!body || !lettering || !font) { Debug.LogWarning("Street signs: material or font missing"); return; }
            if (font.material && font.material.mainTexture) lettering.mainTexture = font.material.mainTexture;
            var loop = CoursePreviewCatalog.Courses.FirstOrDefault(c => c.scene == "StreetLoopGreybox")?.main;
            if (loop == null || loop.Length < 10) { Report.Add("no Street Loop route data"); return; }
            var lines = CoursePreviewCatalog.Courses.SelectMany(c => new[] { c.main }.Concat((c.branches ?? new CoursePreviewCatalog.Path[0]).Select(b => b.points))).Where(l => l != null && l.Length > 1).ToList();
            var root = new GameObject("Street name signs (0.79)"); SceneManager.MoveGameObjectToScene(root, scene);
            var route = new Polyline(loop);
            var highway = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<RaceRoad>()).FirstOrDefault(r => r.openHighway && !r.forestTrail && r.points != null && r.points.Length > 1);
            if (highway) highway.Initialize();
            foreach (var (near, incoming, outgoing, stemOutgoing) in Junctions)
            {
                float s0 = route.Nearest(near);
                // the two roads as straight lines well before and after the corner (the race line keeps to a lane and
                // starts its turn early), each moved onto its paved centre measured on the spot, and where they cross
                Vector3 pA = route.At(s0 - 90), dA = Flat(route.At(s0 - 60) - route.At(s0 - 120)), pB = route.At(s0 + 90), dB = Flat(route.At(s0 + 120) - route.At(s0 + 60));
                var (shiftA, wA) = Paved(pA, dA); var (shiftB, wB) = Paved(pB, dB);
                Vector3 nA = new(-dA.z, 0, dA.x), nB = new(-dB.z, 0, dB.x); pA += nA * shiftA; pB += nB * shiftB;
                // Hwy 92: its own centreline (the through route), not the paved width, which includes wide seam pieces
                if (highway) { if (incoming == Hwy92) Centre(highway, route.At(s0 - 90), ref pA, ref dA, ref wA); if (outgoing == Hwy92) Centre(highway, route.At(s0 + 90), ref pB, ref dB, ref wB); nA = new(-dA.z, 0, dA.x); nB = new(-dB.z, 0, dB.x); }
                if (!Cross(pA, dA, pB, dB, out var centre)) { Report.Add($"{incoming} / {outgoing}: roads parallel, no sign"); continue; }
                centre.y = route.At(s0).y;
                var stem = stemOutgoing ? dB : -dA;
                (Vector3 p, float clear, bool side)? best = null; var tried = new List<string>();
                foreach (int sa in new[] { 1, -1 }) foreach (int sb in new[] { 1, -1 })
                    {
                        // the corner just off both roads; stepped further out while it is still on a paved piece
                        Vector3 c = default; string why = null; Vector3 ground = default; float clear = 0;
                        foreach (float extra in new[] { 0f, 1.5f, 3f, 4.5f, 6f, 8f })
                        {
                            c = centre + nA * sa * (wA + Verge + extra) + nB * sb * (wB + Verge + extra);
                            why = Unsuitable(c, lines, out ground, out clear); if (why == null || !why.StartsWith("on a drivable")) break;
                        }
                        bool side = Vector3.Dot(c - centre, stem) > 0; // a corner of the side road
                        tried.Add($"corner ({c.x:F1}, {c.z:F1}){(side ? " by the side road" : "")}: {why ?? $"ok, {clear:F1} m from the nearest race line"}");
                        if (why == null && (best == null || (side && !best.Value.side) || (side == best.Value.side && clear > best.Value.clear))) best = (ground, clear, side);
                    }
                if (best == null) { Report.Add($"{incoming} / {outgoing} at ({centre.x:F0}, {centre.z:F0}): no suitable corner, no sign ({string.Join("; ", tried)})"); continue; }
                var post = best.Value.p;
                Make(root.transform, post, incoming, dA, outgoing, dB, font);
                Placed.Add((post, incoming, outgoing, centre));
                Report.Add($"{incoming} / {outgoing}: junction ({centre.x:F1}, {centre.z:F1}), paved half-widths {wA:F1} / {wB:F1} m; post at ({post.x:F1}, {post.y:F2}, {post.z:F1}), {best.Value.clear:F1} m from the nearest race line ({string.Join("; ", tried)})");
            }
        }
        static void Centre(RaceRoad road, Vector3 near, ref Vector3 p, ref Vector3 d, ref float half)
        {
            float s = road.Project(near, out _); var q = road.At(s, out var f); f = Flat(f); if (Vector3.Dot(f, d) < 0) f = -f;
            p = q; d = f; half = road.HalfWidth(s);
        }
        static Vector3 Flat(Vector3 v) { v.y = 0; return v.normalized; }
        static bool Cross(Vector3 p, Vector3 d, Vector3 q, Vector3 e, out Vector3 x)
        {
            float den = d.x * e.z - d.z * e.x; x = default; if (Mathf.Abs(den) < .2f) return false;
            float t = ((q.x - p.x) * e.z - (q.z - p.z) * e.x) / den; x = p + d * t; return true;
        }
        // the paved surface across a road, measured on the spot from a point on it: how far its centre is from that point
        // (along the left normal) and its half-width
        static (float shift, float half) Paved(Vector3 at, Vector3 along)
        {
            var n = new Vector3(-along.z, 0, along.x); float left = 0, right = 0;
            for (float o = 0; o < 20; o += .25f) { right = o; if (!Paved(at + n * o)) break; }
            for (float o = 0; o < 20; o += .25f) { left = o; if (!Paved(at - n * o)) break; }
            return ((right - left) * .5f, Mathf.Max(3.5f, (right + left) * .5f));
        }
        static bool Paved(Vector3 p)
        {
            if (!SurfaceBelow(p, out var h)) return false;
            return SceneryTrees.OnDrivable(h.point + Vector3.up * .05f, null, out _);
        }
        static bool SurfaceBelow(Vector3 p, out RaycastHit hit)
        {
            foreach (var h in Physics.RaycastAll(new Vector3(p.x, p.y + 60, p.z), Vector3.down, 200, ~0, QueryTriggerInteraction.Ignore).OrderBy(h => h.distance))
                if (!(h.collider.attachedRigidbody && !h.collider.attachedRigidbody.isKinematic)) { hit = h; return true; }
            hit = default; return false;
        }
        // why a post cannot stand here (null: it can): not on any drivable surface (1 m round), not near any course's race
        // line or shortcut, nothing else standing within 1.5 m (fences, poles, props), on open ground.
        static string Unsuitable(Vector3 c, List<Vector3[]> lines, out Vector3 ground, out float clear)
        {
            ground = c; clear = 0;
            if (!SurfaceBelow(c, out var h)) return "no ground";
            ground = h.point;
            foreach (var o in new[] { Vector3.zero, Vector3.right, Vector3.left, Vector3.forward, Vector3.back })
                if (SceneryTrees.OnDrivable(ground + o + Vector3.up * .05f, null, out var what)) return "on a drivable surface (" + what + ")";
            var g0 = ground; clear = lines.Min(l => Distance(l, g0));
            if (clear < LineClearance) return $"{clear:F1} m from a race line";
            foreach (var col in Physics.OverlapCapsule(ground + Vector3.up * .3f, ground + Vector3.up * PostHeight, 1.5f, ~0, QueryTriggerInteraction.Ignore))
                if (col.bounds.size.x < 120 && col.bounds.size.z < 120 && !(col.attachedRigidbody && !col.attachedRigidbody.isKinematic)) return "next to " + col.name;
            return null;
        }
        static float Distance(Vector3[] l, Vector3 p)
        {
            float best = float.MaxValue; var q = new Vector2(p.x, p.z);
            for (int i = 0; i + 1 < l.Length; i++)
            {
                Vector2 a = new(l[i].x, l[i].z), b = new(l[i + 1].x, l[i + 1].z), ab = b - a; float t = ab.sqrMagnitude < 1e-6f ? 0 : Mathf.Clamp01(Vector2.Dot(q - a, ab) / ab.sqrMagnitude);
                best = Mathf.Min(best, (a + ab * t - q).magnitude);
            }
            return best;
        }

        // ---------------------------------------------------------------- the sign
        static readonly Color Green = new(.02f, .36f, .20f), White = new(.94f, .95f, .93f), Steel = new(.56f, .58f, .59f);
        void Make(Transform root, Vector3 ground, string lower, Vector3 lowerAlong, string upper, Vector3 upperAlong, Font font)
        {
            var sign = new GameObject($"Street sign - {lower} / {upper}").transform; sign.SetParent(root, false); sign.position = ground;
            var v = new List<Vector3>(); var n = new List<Vector3>(); var c = new List<Color>(); var t = new List<int>();
            void Box(Vector3 centre, Vector3 size, Quaternion r, Color col)
            {
                var e = size * .5f;
                foreach (var f in new[] { Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.forward, Vector3.back })
                {
                    var u = Mathf.Abs(f.y) > .5f ? Vector3.right : Vector3.up; var w = Vector3.Cross(f, u);
                    Vector3 P(float a, float b) => centre + r * Vector3.Scale(f + u * a + w * b, e);
                    int i = v.Count; v.Add(P(-1, -1)); v.Add(P(1, -1)); v.Add(P(1, 1)); v.Add(P(-1, 1));
                    for (int k = 0; k < 4; k++) { n.Add(r * f); c.Add(col); }
                    t.AddRange(new[] { i, i + 1, i + 2, i, i + 2, i + 3 });
                }
            }
            Box(new Vector3(0, PostHeight * .5f - .3f, 0), new Vector3(.075f, PostHeight + .6f, .075f), Quaternion.identity, Steel);   // set .6 m into the ground
            Box(new Vector3(0, PostHeight + .02f, 0), new Vector3(.1f, .05f, .1f), Quaternion.identity, Steel);                         // cap
            float y0 = PostHeight - 2 * BladeHeight - .06f;
            foreach (var (along, y) in new[] { (lowerAlong, y0), (upperAlong, y0 + BladeHeight + .05f) })
            {
                var r = Quaternion.LookRotation(Vector3.Cross(Vector3.up, along)); // blade's long axis = road direction (local x)
                var mid = new Vector3(0, y + BladeHeight * .5f, 0);
                Box(mid, new Vector3(BladeLength, BladeHeight, .03f), r, Green);
                // white border on both faces
                foreach (float z in new[] { -.017f, .017f })
                {
                    Box(mid + r * new Vector3(0, BladeHeight * .5f - .025f, z), new Vector3(BladeLength - .04f, .02f, .004f), r, White);
                    Box(mid + r * new Vector3(0, -BladeHeight * .5f + .025f, z), new Vector3(BladeLength - .04f, .02f, .004f), r, White);
                    Box(mid + r * new Vector3(BladeLength * .5f - .03f, 0, z), new Vector3(.02f, BladeHeight - .04f, .004f), r, White);
                    Box(mid + r * new Vector3(-BladeLength * .5f + .03f, 0, z), new Vector3(.02f, BladeHeight - .04f, .004f), r, White);
                }
                Box(new Vector3(0, y + BladeHeight * .5f, 0), new Vector3(.06f, BladeHeight + .04f, .06f), Quaternion.identity, Steel); // bracket
            }
            var mesh = new Mesh { name = "Street sign" }; mesh.SetVertices(v); mesh.SetNormals(n); mesh.SetColors(c); mesh.SetTriangles(t, 0); mesh.RecalculateBounds(); mesh.UploadMeshData(true);
            var go = new GameObject("Post and blades", typeof(MeshFilter), typeof(MeshRenderer)); go.transform.SetParent(sign, false);
            go.GetComponent<MeshFilter>().sharedMesh = mesh; go.GetComponent<MeshRenderer>().sharedMaterial = body;
            // lettering, both faces of each blade
            foreach (var (name, along, y) in new[] { (lower, lowerAlong, y0), (upper, upperAlong, y0 + BladeHeight + .05f) })
                foreach (int face in new[] { 1, -1 })
                {
                    var r = Quaternion.LookRotation(Vector3.Cross(Vector3.up, along) * -face);
                    var label = new GameObject("Blade lettering - " + name).AddComponent<TextMesh>(); label.transform.SetParent(sign, false);
                    label.font = font; label.text = name; label.anchor = TextAnchor.MiddleCenter; label.alignment = TextAlignment.Center; label.fontSize = 96; label.characterSize = .1f; label.color = White;
                    var mr = label.GetComponent<MeshRenderer>(); mr.sharedMaterial = lettering; mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    label.transform.localRotation = r; label.transform.localPosition = new Vector3(0, y + BladeHeight * .5f, 0) + r * new Vector3(0, 0, -.022f);
                    labels.Add(mr);
                }
        }
        // fit the lettering once TextMesh has built it: capitals about 0.26 m tall, the name within the blade
        readonly List<MeshRenderer> labels = new();
        void LateUpdate()
        {
            for (int i = labels.Count - 1; i >= 0; i--)
            {
                var mr = labels[i]; if (!mr) { labels.RemoveAt(i); continue; }
                var b = mr.localBounds; if (b.size.x < 1e-4f) continue;
                mr.transform.localScale = Vector3.one * Mathf.Min(.26f / b.size.y, (BladeLength - .2f) / b.size.x); labels.RemoveAt(i);
            }
            if (labels.Count == 0) enabled = false;
        }

        // a route as a polyline with stations (wraps round: the Street Loop is a loop)
        sealed class Polyline
        {
            readonly Vector3[] p; readonly float[] s; public readonly float Length;
            public Polyline(Vector3[] points) { p = points; s = new float[p.Length]; for (int i = 1; i < p.Length; i++) s[i] = s[i - 1] + Vector3.Distance(p[i - 1], p[i]); Length = s[^1]; }
            public Vector3 At(float x)
            {
                x = Mathf.Repeat(x, Length); int i = System.Array.BinarySearch(s, x); if (i < 0) i = ~i - 1; i = Mathf.Clamp(i, 0, p.Length - 2);
                return Vector3.Lerp(p[i], p[i + 1], Mathf.InverseLerp(s[i], s[i + 1], x));
            }
            public float Nearest(Vector3 q)
            {
                int best = 0; float bd = float.MaxValue; for (int i = 0; i < p.Length; i++) { float d = (p[i].x - q.x) * (p[i].x - q.x) + (p[i].z - q.z) * (p[i].z - q.z); if (d < bd) { bd = d; best = i; } }
                return s[best];
            }
        }
    }
}
