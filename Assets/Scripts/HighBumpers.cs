using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Racer
{
    // 1.2 HIGH MODE Part D1: gutter bumpers, like a bowling alley's. In a HIGH MODE race (never a normal race or Free Roam) a low rounded red
    // and white bumper (about 1 m, with an unseen collision fence above its road side up to 2.2 m so nothing rides up and over it) runs along
    // both edges of the course line and of every shortcut, so a vehicle that drifts to the edge bounces back onto the road (most important on the mountain courses' cliff edges and hanging roads). Built once per course scene at the first HIGH MODE race
    // start from the race's own lines (RaceRoad / WoodlandRoute, nothing authored), shown only while the race is in HIGH MODE.
    // - It stands at the road's edge: the first point out from the centre where the ground drops away (a cliff, a hanging road), else just
    //   past the paved width (HalfWidth + 1.2 m).
    // - Openings are left wherever another drivable line crosses or joins (a shortcut's mouth, a junction, the same course passing again,
    //   the rivals' own lines, the traffic roads), so no road, shortcut or crossing is ever closed.
    // - None over a gap (no road under the line: a jump's flight), so a jump flies as before.
    // - 1.3.0-review2 (Dan: "The lanes on the landing are too narrow and users end up outside the track"): on a jump's landing the bumpers stand
    //   wider apart, at least LandingHalf from the line on each side for the first LandingFull metres past the gap, closing back to the road's edge over
    //   the next LandingTaper metres (a funnel), so a vehicle that comes down off the line lands between them. Where the ground drops away nearer
    //   than that, the bumper stays at the drop as before.
    // - On the "Ignore Raycast" layer: the suspension, the cameras and the reset search do not take it for ground.
    public static class HighBumpers
    {
        const string RootName = "HIGH MODE bumpers";
        public const int Layer = 2;
        public static int Samples { get; private set; } // checks: bumper stations built
        public static int Openings { get; private set; } // checks: stations left open for a crossing line
        public static int LandingStations { get; private set; } // checks: stations on a landing, set wider apart
        static PhysicsMaterial bounce;
        public static GameObject Root(Scene scene) => scene.GetRootGameObjects().FirstOrDefault(g => g.name == RootName);
        public static void Apply(RaceDirector race)
        {
            var root = Root(race.gameObject.scene);
            if (!race.high || race.FreeRoam || !race.road) { if (root && root.activeSelf) { root.SetActive(false); Physics.SyncTransforms(); } return; }
            if (!root) root = Build(race);
            root.SetActive(true); PerPlayer(race, root); Physics.SyncTransforms();
        }
        // 1.2.0-review2: split screen with one player in HIGH MODE: the bumpers are drawn only in that player's half (the split views' own
        // layers) and the other player's vehicle passes through them (its race is unchanged); one player, or both in HIGH MODE: as before
        public static void PerPlayer(RaceDirector race, GameObject root)
        {
            bool split = SplitScreen.Active, h1 = race.HighFor(1), h2 = race.HighFor(2);
            int layer = !split || (h1 && h2) ? Layer : h1 ? SplitRace.View1Layer : SplitRace.View2Layer;
            foreach (Transform t in root.transform) t.gameObject.layer = layer;
            var bumpers = root.GetComponentsInChildren<MeshCollider>(true);
            void Pass(ArcadeVehicle car, bool through) { if (!car) return; foreach (var c in car.GetComponentsInChildren<Collider>(true)) foreach (var b in bumpers) Physics.IgnoreCollision(c, b, through); }
            Pass(race.vehicle, split && !h1); Pass(SplitScreen.Race ? SplitScreen.Race.P2Car : null, split && !h2);
        }
        public static bool IsBumper(Collider c) => c && c.transform.parent && c.transform.parent.name == RootName;

        sealed class Line { public Func<float, (Vector3 p, Vector3 f)> at; public Func<float, float> half; public float length; public bool loop; public bool walls; }
        struct Band { public Vector3 p; public float half, s; public int line; }
        const float Cell = 16, Step = 2;
        public const float LandingHalf = 11, LandingFull = 35, LandingTaper = 20;
        static long Key(int x, int z) => ((long)x << 32) ^ (uint)z;

        static GameObject Build(RaceDirector race)
        {
            var scene = race.gameObject.scene; Samples = Openings = LandingStations = 0;
            var root = new GameObject(RootName) { layer = Layer }; SceneManager.MoveGameObjectToScene(root, scene);
            var lines = new List<Line>();
            RaceRoad Road(RaceRoad r) { r.Initialize(); return r; }
            var main = Road(race.road);
            lines.Add(new Line { at = s => { var p = main.At(s, out var f); return (p, f); }, half = main.HalfWidth, length = main.Length, loop = !main.openHighway, walls = true });
            foreach (var b in race.Branches ?? Array.Empty<WoodlandRoute>())
            {
                if (!b || !b.isActiveAndEnabled || b.points == null || b.points.Length < 2) continue; var route = b;
                lines.Add(new Line { at = s => { var p = route.At(s, out var f); return (p, f); }, half = _ => route.halfWidth, length = route.Length, walls = true });
                if (route.HasAiLine) lines.Add(new Line { at = s => { var p = route.AtFor(true, s, out var f); return (p, f); }, half = _ => route.halfWidth, length = route.LengthFor(true) });
            }
            foreach (var other in new[] { race.ambientRoad, race.throughRoad })
                if (other && other != race.road && other.points != null && other.points.Length >= 4) { var r = Road(other); lines.Add(new Line { at = s => { var p = r.At(s, out var f); return (p, f); }, half = r.HalfWidth, length = r.Length, loop = !r.openHighway }); }

            // every line's driven band (centre, half width), for the openings
            var grid = new Dictionary<long, List<Band>>();
            for (int li = 0; li < lines.Count; li++)
            {
                var l = lines[li]; if (l.length <= 0) continue;
                for (float s = 0; s < l.length; s += Step)
                {
                    var (p, _) = l.at(s); long k = Key(Mathf.FloorToInt(p.x / Cell), Mathf.FloorToInt(p.z / Cell));
                    if (!grid.TryGetValue(k, out var list)) grid[k] = list = new List<Band>(); list.Add(new Band { p = p, half = l.half(s), s = s, line = li });
                }
            }
            bool Open(Vector3 w, int line, float s, Vector3 centre, Vector3 outward)
            {
                var l = lines[line]; int cx = Mathf.FloorToInt(w.x / Cell), cz = Mathf.FloorToInt(w.z / Cell);
                for (int x = cx - 1; x <= cx + 1; x++) for (int z = cz - 1; z <= cz + 1; z++)
                    {
                        if (!grid.TryGetValue(Key(x, z), out var list)) continue;
                        foreach (var b in list)
                        {
                            if (Mathf.Abs(b.p.y - w.y) > 4) continue;
                            float dx = b.p.x - w.x, dz = b.p.z - w.z; if (dx * dx + dz * dz > (b.half - .3f) * (b.half - .3f)) continue;
                            // another line along the course's own centre (the traffic on the course road) is the same road: no opening
                            if (b.line != line) { if (Mathf.Abs(Vector3.Dot(Vector3.ProjectOnPlane(b.p - centre, Vector3.up), outward)) < 1.5f) continue; return true; }
                            float along = Mathf.Abs(b.s - s); if (l.loop) along = Mathf.Min(along, l.length - along);
                            if (along > 30) return true; // the same line passing this way again (a hairpin's other leg, a second pass)
                        }
                    }
                return false;
            }

            int mask = race.vehicle ? race.vehicle.groundMask.value : 1;
            var hits = new RaycastHit[16];
            bool Surface(Vector3 q, float nearY, out float y)
            {
                y = 0; int n = scene.GetPhysicsScene().Raycast(new Vector3(q.x, nearY + 2.5f, q.z), Vector3.down, hits, 6, mask, QueryTriggerInteraction.Ignore);
                float best = float.MaxValue;
                for (int i = 0; i < n; i++) { var h = hits[i]; if (h.collider.attachedRigidbody || h.normal.y < .5f || h.collider.gameObject.layer == Layer) continue; float d = Mathf.Abs(h.point.y - nearY); if (d < best) { best = d; y = h.point.y; } }
                return best < float.MaxValue;
            }
            // a bumper already standing here from another line or another pass of this one (the same road driven twice): not built again
            var placed = new Dictionary<long, List<(Vector3 w, int owner, float s)>>();
            bool Duplicate(Vector3 w, int owner, float s, float length)
            {
                int cx = Mathf.FloorToInt(w.x / 4), cz = Mathf.FloorToInt(w.z / 4);
                for (int x = cx - 1; x <= cx + 1; x++) for (int z = cz - 1; z <= cz + 1; z++)
                        if (placed.TryGetValue(Key(x, z), out var l)) foreach (var o in l)
                            {
                                if ((o.w - w).sqrMagnitude >= 1.2f) continue;
                                float along = Mathf.Abs(o.s - s); along = Mathf.Min(along, length - along);
                                if (o.owner != owner || along > 30) return true;
                            }
                return false;
            }
            void Place(Vector3 w, int owner, float s) { long k = Key(Mathf.FloorToInt(w.x / 4), Mathf.FloorToInt(w.z / 4)); if (!placed.TryGetValue(k, out var l)) placed[k] = l = new List<(Vector3, int, float)>(); l.Add((w, owner, s)); }

            // each walled line's jump gaps (no road under the line for 4 m or more): how far past the last gap's end each station is
            var pastLanding = new Dictionary<int, float[]>();
            for (int li = 0; li < lines.Count; li++)
            {
                var l = lines[li]; if (!l.walls || l.length <= 0) continue; int count = Mathf.FloorToInt(l.length / Step) + 1; var past = new float[count]; var gap = new bool[count];
                for (int i = 0; i < count; i++) { var (p, _) = l.at(Mathf.Min(i * Step, l.length - .01f)); gap[i] = !Surface(p, p.y, out float gy) || Mathf.Abs(gy - p.y) > 1.6f; }
                float since = float.MaxValue; int run = 0;
                for (int pass = 0; pass < (l.loop ? 2 : 1); pass++)
                    for (int i = 0; i < count; i++)
                    {
                        if (gap[i]) { run++; since = float.MaxValue; continue; }
                        if (run * Step >= 4) since = 0; else if (since < float.MaxValue) since += Step;
                        run = 0; past[i] = since;
                    }
                pastLanding[li] = past;
            }
            float Widen(int line, float s)
            {
                if (!pastLanding.TryGetValue(line, out var past)) return 0; int i = Mathf.Clamp(Mathf.FloorToInt(s / Step), 0, past.Length - 1); float d = past[i];
                return d <= LandingFull ? 1 : d <= LandingFull + LandingTaper ? 1 - (d - LandingFull) / LandingTaper : 0;
            }
            var material = Resources.Load<Material>("Scenery/Building");
            if (!bounce) bounce = new PhysicsMaterial("HIGH MODE bumper") { bounciness = .3f, bounceCombine = PhysicsMaterialCombine.Maximum, dynamicFriction = .05f, staticFriction = .05f, frictionCombine = PhysicsMaterialCombine.Minimum };
            int chunk = 0;
            for (int li = 0; li < lines.Count; li++)
            {
                var l = lines[li]; if (!l.walls || l.length <= 0) continue;
                for (int side = -1; side <= 1; side += 2)
                {
                    var strip = new List<(Vector3 w, Vector3 o)>(); int count = Mathf.FloorToInt(l.length / Step);
                    for (int i = 0; i <= count + (l.loop ? 1 : 0); i++)
                    {
                        float s = l.loop ? Mathf.Repeat(i * Step, l.length) : Mathf.Min(i * Step, l.length - .01f);
                        int owner = li * 2 + (side > 0 ? 1 : 0);
                        var ok = Sample(l, li, s, side, i > count, owner, out var w, out var o);
                        if (ok && strip.Count > 0 && (strip[^1].w - w).magnitude > 7) ok = false;
                        if (ok) { strip.Add((w, o)); if (i <= count) { Place(w, owner, s); Samples++; } }
                        if ((!ok || strip.Count >= 40 || i == count + (l.loop ? 1 : 0)) && strip.Count >= 2) { Emit(root.transform, strip, chunk++, material); var last = strip[^1]; strip.Clear(); if (ok) strip.Add(last); }
                        else if (!ok) strip.Clear();
                    }
                }
            }
            return root;

            bool Sample(Line l, int line, float s, int side, bool wrap, int owner, out Vector3 w, out Vector3 outward)
            {
                w = outward = default; var (p, f) = l.at(s); f = Vector3.ProjectOnPlane(f, Vector3.up); if (f.sqrMagnitude < 1e-4f) return false; f.Normalize();
                if (!Surface(p, p.y, out float roadY) || Mathf.Abs(roadY - p.y) > 1.6f) return false; // no road under the line: a jump's gap
                outward = Vector3.Cross(Vector3.up, f) * side; float half = l.half(s), wide = Widen(line, s), reach = Mathf.Lerp(half + 1.2f, Mathf.Max(half + 1.2f, LandingHalf), wide), edge = reach;
                if (reach > half + 1.25f) LandingStations++;
                for (float d = 2.5f; d <= reach; d += .5f)
                    if (!Surface(p + outward * d, roadY, out float y) || y < roadY - 1f) { edge = Mathf.Max(2.2f, d - .3f); break; } // the ground drops away here
                w = p + outward * edge; w.y = roadY;
                if (Open(w, line, s, p, outward)) { Openings++; return false; }
                return wrap || !Duplicate(w, owner, s, l.length);
            }
        }
        // the rounded profile (out from the road's edge, height above the road), buried below the road so it meets uneven ground
        static readonly Vector2[] Profile = { new(0, -1.4f), new(0, .5f), new(.1f, .76f), new(.28f, .92f), new(.5f, .98f), new(.72f, .92f), new(.9f, .76f), new(1f, .5f), new(1f, -1.4f) };
        // the collision also has an unseen fence on the road side up to this height, so a vehicle hitting at an angle cannot ride up and over
        const float Fence = 2.2f;
        static readonly Color Red = new(.86f, .16f, .12f), White = new(.95f, .94f, .9f);
        static void Emit(Transform root, List<(Vector3 w, Vector3 o)> strip, int index, Material material)
        {
            var go = new GameObject("Bumper " + index) { layer = Layer }; go.transform.SetParent(root, false);
            var origin = strip[0].w; var verts = new List<Vector3>(); var colors = new List<Color>(); var tris = new List<int>();
            int P = Profile.Length;
            for (int i = 0; i + 1 < strip.Count; i++)
            {
                var c = (index * 40 + i) / 3 % 2 == 0 ? Red : White; int start = verts.Count;
                for (int k = 0; k < 2; k++) { var (w, o) = strip[i + k]; foreach (var q in Profile) { verts.Add(w + o * q.x + Vector3.up * q.y - origin); colors.Add(c); } }
                var (w0, o0) = strip[i]; var centre = w0 + o0 * .5f + Vector3.up * -.3f - origin; // inside the profile
                for (int j = 0; j + 1 < P; j++)
                {
                    int a = start + j, b = start + j + 1, a2 = start + P + j, b2 = start + P + j + 1;
                    // each face wound to face out of the bumper (the road side, the top, the outside)
                    var n = Vector3.Cross(verts[b] - verts[a], verts[a2] - verts[a]); var m = (verts[a] + verts[b]) * .5f - centre;
                    if (Vector3.Dot(n, m) >= 0) tris.AddRange(new[] { a, b, a2, b, b2, a2 }); else tris.AddRange(new[] { a, a2, b, b, a2, b2 });
                }
            }
            // end caps (both faces), so a vehicle running along the edge meets a solid end after a gap, never the hollow inside
            foreach (int end in new[] { 0, strip.Count - 1 })
            {
                var (w, o) = strip[end];
                for (int face = 0; face < 2; face++)
                {
                    int start = verts.Count; verts.Add(w + o * .5f + Vector3.up * -.2f - origin); colors.Add(White);
                    foreach (var q in Profile) { verts.Add(w + o * q.x + Vector3.up * q.y - origin); colors.Add(White); }
                    for (int j = 0; j < P; j++) { int a = start + 1 + j, b = start + 1 + (j + 1) % P; if (face == 0) tris.AddRange(new[] { start, a, b }); else tris.AddRange(new[] { start, b, a }); }
                }
            }
            go.transform.position = origin;
            var mesh = new Mesh { name = "HIGH MODE bumper" }; mesh.indexFormat = verts.Count > 65000 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
            mesh.SetVertices(verts); mesh.SetColors(colors); mesh.SetTriangles(tris, 0); mesh.RecalculateNormals(); mesh.RecalculateBounds();
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>(); if (material) renderer.sharedMaterial = material; renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            // the collision: the bumper itself and the unseen fence above its road-side face (both sides solid)
            var cv = new List<Vector3>(verts); var ct = new List<int>(tris);
            for (int i = 0; i + 1 < strip.Count; i++)
            {
                int k = cv.Count; var (wa, _) = strip[i]; var (wb, _) = strip[i + 1];
                cv.Add(wa + Vector3.up * .3f - origin); cv.Add(wa + Vector3.up * Fence - origin); cv.Add(wb + Vector3.up * .3f - origin); cv.Add(wb + Vector3.up * Fence - origin);
                ct.AddRange(new[] { k, k + 1, k + 2, k + 2, k + 1, k + 3, k, k + 2, k + 1, k + 2, k + 3, k + 1 });
            }
            var solid = new Mesh { name = "HIGH MODE bumper collision" }; solid.indexFormat = cv.Count > 65000 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
            solid.SetVertices(cv); solid.SetTriangles(ct, 0); solid.RecalculateBounds();
            var collider = go.AddComponent<MeshCollider>(); collider.sharedMesh = solid; collider.sharedMaterial = bounce;
        }
    }
}
