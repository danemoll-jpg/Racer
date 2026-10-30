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
            string key = Key(race); // 1.3.0-review8: built again when the course line or its jumps change (a Stunt Race's kickers, Leviathan in a Mountain Loop scene)
            if (root && root.GetComponent<BuildKey>()?.key != key) { root.SetActive(false); UnityEngine.Object.Destroy(root); root = null; }
            if (!root) { root = Build(race); root.AddComponent<BuildKey>().key = key; }
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
        sealed class BuildKey : MonoBehaviour { public string key; }
        static string Key(RaceDirector race) => $"{race.road.name}|{race.courseId}|{race.stunts}|{string.Join(",", (race.Branches ?? Array.Empty<WoodlandRoute>()).Where(b => b).Select(b => b.name))}";
        public static readonly List<JumpRooms.Room> Rooms = new(); // 1.3.0-review8: the jumps' rooms of the last build (checks)
        public static int RoomStations { get; private set; } // checks: stations left out for a jump
        public static int Ends { get; private set; } // checks: run ends flared and rounded
        public static bool IsBumper(Collider c) => c && c.transform.parent && c.transform.parent.name == RootName;

        sealed class Line { public Func<float, (Vector3 p, Vector3 f)> at; public Func<float, float> half; public float length; public bool loop; public bool walls; }
        struct Band { public Vector3 p; public float half, s; public int line; }
        const float Cell = 16, Step = 2;
        public const float LandingHalf = 11, LandingFull = 35, LandingTaper = 20;
        static long Key(int x, int z) => ((long)x << 32) ^ (uint)z;

        static GameObject Build(RaceDirector race)
        {
            var scene = race.gameObject.scene; Samples = Openings = LandingStations = RoomStations = Ends = 0; Physics.SyncTransforms();
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
            // 1.3.0-review8 Part B: each walled line's jumps and the room each needs (JumpRooms): no bumper in one
            Rooms.Clear();
            if (!JumpRooms.Off) for (int li = 0; li < lines.Count; li++) { var l = lines[li]; if (l.walls && l.length > 0) Rooms.AddRange(JumpRooms.Find(scene.GetPhysicsScene(), mask, l.at, l.half, l.length, l.loop, li, c => GuideWalls.Is(c))); }
            if (!JumpRooms.Off) Rooms.AddRange(JumpRooms.Flights(race));
            bool OnLineRoom(int line, float s) { foreach (var r in Rooms) if (r.OnLine(line, s)) return true; return false; }
            bool InRoom(Vector3 w) { foreach (var r in Rooms) if (r.Contains(w)) return true; return false; }
            var material = Resources.Load<Material>("Scenery/Building");
            if (!bounce) bounce = new PhysicsMaterial("HIGH MODE bumper") { bounciness = 0f, bounceCombine = PhysicsMaterialCombine.Minimum, dynamicFriction = 0f, staticFriction = 0f, frictionCombine = PhysicsMaterialCombine.Minimum }; // 1.3.0-review8: WallGlance turns a vehicle off it (was a 0.3 bounce, a little friction)
            int chunk = 0;
            for (int li = 0; li < lines.Count; li++)
            {
                var l = lines[li]; if (!l.walls || l.length <= 0) continue;
                for (int side = -1; side <= 1; side += 2)
                {
                    // 1.3.0-review8: the unbroken runs first (a run ends at a jump, an opening, a drop), so each run's two ends can be shaped
                    var runs = new List<List<Pt>>(); List<Pt> run = null; int count = Mathf.FloorToInt(l.length / Step); bool firstAtZero = false, lastWrap = false;
                    for (int i = 0; i <= count + (l.loop ? 1 : 0); i++)
                    {
                        float s = l.loop ? Mathf.Repeat(i * Step, l.length) : Mathf.Min(i * Step, l.length - .01f);
                        int owner = li * 2 + (side > 0 ? 1 : 0);
                        var ok = Sample(l, li, s, side, i > count, owner, out var w, out var o, out var c);
                        if (ok && run != null && run.Count > 0 && (run[^1].w - w).magnitude > 7) run = null;
                        if (!ok) { run = null; lastWrap = false; continue; }
                        if (run == null) lastWrap = false;
                        if (i <= count) { Place(w, owner, s); Samples++; }
                        if (run == null) { run = new List<Pt>(); runs.Add(run); if (i == 0) firstAtZero = true; }
                        run.Add(new Pt { w = w, o = o, c = c, s = s }); if (i > count) lastWrap = true;
                    }
                    // a loop's last run that comes round to its first: one run
                    if (l.loop && runs.Count > 1 && firstAtZero && lastWrap)
                    { var last = runs[^1]; last.RemoveAt(last.Count - 1); last.AddRange(runs[0]); runs.RemoveAt(0); }
                    bool ring = l.loop && runs.Count == 1 && firstAtZero && lastWrap;
                    foreach (var r in runs)
                    {
                        if (r.Count < 2) continue;
                        if (!ring && !JumpRooms.Off) Flare(r, li);
                        for (int at = 0; at < r.Count - 1; at += 39)
                        {
                            int to = Mathf.Min(r.Count - 1, at + 39); var strip = r.GetRange(at, to - at + 1);
                            Emit(root.transform, strip, chunk++, material, !ring && at == 0, !ring && to == r.Count - 1);
                        }
                    }
                }
            }
            return root;

            // 1.3.0-review8 Part B item 2: where a run starts or stops (a jump, a shortcut's mouth, anywhere) its last FlareLength metres bend away from
            // the road, up to FlareOut at its tip (less where the ground or an opening does not allow it), so a vehicle that meets the end is turned back
            // onto the course along it; Emit rounds the tip itself and lowers the unseen fence to the bumper over the last 3 m
            void Flare(List<Pt> r, int line)
            {
                float total = (r.Count - 1) * Step; Ends += 2;
                for (int j = 0; j < r.Count; j++)
                {
                    var pt = r[j]; float d = Mathf.Min(j * Step, total - j * Step); pt.low = 1 - Mathf.Clamp01(d / 3f);
                    if (d < FlareLength)
                    {
                        float want = FlareOut * (1 - d / FlareLength) * (1 - d / FlareLength);
                        for (float push = want; push > .05f; push *= .5f)
                        {
                            var q = pt.w + pt.o * push;
                            if (!Surface(q, pt.w.y, out float y) || y < pt.w.y - 1f || Open(q, line, pt.s, pt.c, pt.o) || InRoom(q)) continue;
                            pt.w = q; break;
                        }
                    }
                    r[j] = pt;
                }
            }

            bool Sample(Line l, int line, float s, int side, bool wrap, int owner, out Vector3 w, out Vector3 outward, out Vector3 centre)
            {
                w = outward = centre = default; var (p, f) = l.at(s); centre = p; f = Vector3.ProjectOnPlane(f, Vector3.up); if (f.sqrMagnitude < 1e-4f) return false; f.Normalize();
                if (!Surface(p, p.y, out float roadY) || Mathf.Abs(roadY - p.y) > 1.6f) return false; // no road under the line: a jump's gap
                outward = Vector3.Cross(Vector3.up, f) * side; float half = l.half(s), wide = Widen(line, s), reach = Mathf.Lerp(half + 1.2f, Mathf.Max(half + 1.2f, LandingHalf), wide), edge = reach;
                if (reach > half + 1.25f) LandingStations++;
                for (float d = 2.5f; d <= reach; d += .5f)
                    if (!Surface(p + outward * d, roadY, out float y) || y < roadY - 1f) { edge = Mathf.Max(2.2f, d - .3f); break; } // the ground drops away here
                w = p + outward * edge; w.y = roadY;
                if (Open(w, line, s, p, outward)) { Openings++; return false; }
                if (InRoom(w) || OnLineRoom(line, s)) { RoomStations++; return false; } // 1.3.0-review8: a jump's flight or landing, or the road on from it to the end of its run-out
                return wrap || !Duplicate(w, owner, s, l.length);
            }
        }
        struct Pt { public Vector3 w, o, c; public float s, low; } // low: the unseen fence lowered toward the bumper at a run end (0: full height)
        public const float FlareLength = 8, FlareOut = 2.5f;
        // the rounded profile (out from the road's edge, height above the road), buried below the road so it meets uneven ground
        static readonly Vector2[] Profile = { new(0, -1.4f), new(0, .5f), new(.1f, .76f), new(.28f, .92f), new(.5f, .98f), new(.72f, .92f), new(.9f, .76f), new(1f, .5f), new(1f, -1.4f) };
        // the collision also has an unseen fence on the road side up to this height, so a vehicle hitting at an angle cannot ride up and over
        const float Fence = 2.2f;
        static readonly Color Red = new(.86f, .16f, .12f), White = new(.95f, .94f, .9f);
        // 1.3.0-review8 Part B item 2: a run's real end (noseStart / noseEnd) is rounded: the profile carried on past the last station in Nose rings
        // that close in on its middle, so the tip is a rounded nose, not a square end
        static readonly float[] NoseAt = { .25f, .45f, .6f }, NoseScale = { .8f, .5f, .15f };
        static void Emit(Transform root, List<Pt> strip, int index, Material material, bool noseStart, bool noseEnd)
        {
            var go = new GameObject("Bumper " + index) { layer = Layer }; go.transform.SetParent(root, false);
            var origin = strip[0].w; var verts = new List<Vector3>(); var colors = new List<Color>(); var tris = new List<int>();
            int P = Profile.Length;
            // the rings along the strip (station, outward, scale about the profile's middle), the noses' rings before and after
            var rings = new List<(Vector3 w, Vector3 o, float k)>();
            Vector3 Along(int i, int j) { var d = strip[i].w - strip[j].w; d.y = 0; return d.sqrMagnitude > 1e-4f ? d.normalized : Vector3.zero; }
            if (noseStart) for (int k = NoseAt.Length - 1; k >= 0; k--) rings.Add((strip[0].w + Along(0, 1) * NoseAt[k], strip[0].o, NoseScale[k]));
            foreach (var pt in strip) rings.Add((pt.w, pt.o, 1));
            if (noseEnd) for (int k = 0; k < NoseAt.Length; k++) rings.Add((strip[^1].w + Along(strip.Count - 1, strip.Count - 2) * NoseAt[k], strip[^1].o, NoseScale[k]));
            Vector3 Ring(int r, Vector2 q) { var (w, o, k) = rings[r]; var mid = new Vector2(.5f, .1f); var e = mid + (q - mid) * k; return w + o * e.x + Vector3.up * e.y; }
            for (int i = 0; i + 1 < rings.Count; i++)
            {
                var c = (index * 40 + i) / 3 % 2 == 0 ? Red : White; int start = verts.Count;
                for (int k = 0; k < 2; k++) foreach (var q in Profile) { verts.Add(Ring(i + k, q) - origin); colors.Add(c); }
                var centre = Ring(i, new Vector2(.5f, -.3f)) - origin; // inside the profile
                for (int j = 0; j + 1 < P; j++)
                {
                    int a = start + j, b = start + j + 1, a2 = start + P + j, b2 = start + P + j + 1;
                    // each face wound to face out of the bumper (the road side, the top, the outside)
                    var n = Vector3.Cross(verts[b] - verts[a], verts[a2] - verts[a]); var m = (verts[a] + verts[b]) * .5f - centre;
                    if (Vector3.Dot(n, m) >= 0) tris.AddRange(new[] { a, b, a2, b, b2, a2 }); else tris.AddRange(new[] { a, a2, b, b, a2, b2 });
                }
            }
            // end caps (both faces), so a vehicle running along the edge meets a solid end after a gap, never the hollow inside
            foreach (int end in new[] { 0, rings.Count - 1 })
            {
                var mid = Ring(end, new Vector2(.5f, -.2f));
                for (int face = 0; face < 2; face++)
                {
                    int start = verts.Count; verts.Add(mid - origin); colors.Add(White);
                    foreach (var q in Profile) { verts.Add(Ring(end, q) - origin); colors.Add(White); }
                    for (int j = 0; j < P; j++) { int a = start + 1 + j, b = start + 1 + (j + 1) % P; if (face == 0) tris.AddRange(new[] { start, a, b }); else tris.AddRange(new[] { start, b, a }); }
                }
            }
            go.transform.position = origin;
            var mesh = new Mesh { name = "HIGH MODE bumper" }; mesh.indexFormat = verts.Count > 65000 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
            mesh.SetVertices(verts); mesh.SetColors(colors); mesh.SetTriangles(tris, 0); mesh.RecalculateNormals(); mesh.RecalculateBounds();
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>(); if (material) renderer.sharedMaterial = material; renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            // the collision: the bumper itself and the unseen fence above its road-side face (both sides solid), down to its buried base
            // (1.3.0-review8, Chastain Round 85: it began 0.3 m up, and where the ground beside the road dips a body went under it and stuck);
            // at a run's end it comes down to the bumper's top over the last 3 m
            var cv = new List<Vector3>(verts); var ct = new List<int>(tris);
            for (int i = 0; i + 1 < strip.Count; i++)
            {
                int k = cv.Count; var pa = strip[i]; var pb = strip[i + 1];
                float ha = Mathf.Lerp(Fence, Profile[4].y, pa.low), hb = Mathf.Lerp(Fence, Profile[4].y, pb.low);
                cv.Add(pa.w + Vector3.up * Profile[0].y - origin); cv.Add(pa.w + Vector3.up * ha - origin); cv.Add(pb.w + Vector3.up * Profile[0].y - origin); cv.Add(pb.w + Vector3.up * hb - origin);
                ct.AddRange(new[] { k, k + 1, k + 2, k + 2, k + 1, k + 3, k, k + 2, k + 1, k + 2, k + 3, k + 1 });
            }
            var solid = new Mesh { name = "HIGH MODE bumper collision" }; solid.indexFormat = cv.Count > 65000 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
            solid.SetVertices(cv); solid.SetTriangles(ct, 0); solid.RecalculateBounds();
            var collider = go.AddComponent<MeshCollider>(); collider.sharedMesh = solid; collider.sharedMaterial = bounce;
        }
    }
}
