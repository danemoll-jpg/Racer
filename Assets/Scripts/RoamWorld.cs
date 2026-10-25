using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Racer
{
    // 1.3 (Dan, 2026-10-24/25: "more big ramps spread across the map and stuff to drive people from the edge of map"; "it is ok to have ones that jump
    // over or through houses"; "the blocked roads that they circle around or they have a large ramp that flips you high in the air but spins you back to
    // the proper direction"): the Free Roam world's additions, in FreeRoamWorld only and shown only while Free Roam runs (one or two players, and the
    // chases and stunts played in it); races never see them. Every item is numbered (its id, Resources/RoamWorld.json) so Dan can say "remove 4".
    //   kicker   an earth kicker onto open ground                 table   a kicker, a long table and a landing slope (the huge ones)
    //   gap      a kicker, open ground, a landing ramp            house   a kicker, an existing house (flown over), a landing ramp beyond it
    //   through  a kicker, a barn with breakable plank ends and a deck inside (fly in one end, out the other), a landing slope
    //   smash    a cluster of hay bales, crates, cones and boxes (BreakableProp) that scatters and resets when everyone has left
    //   loop     a road that ended at the world's edge ends in a banked turnaround that circles you back
    //   flip     a road that ended at the world's edge ends in a flip ramp: thrown high, spun round in the air (assisted), landed upright facing back
    // Before the trees are fitted, the trunks where an item, its run-up and its landing are go (Clear, as StuntPark.Clear). The trees themselves break
    // in Free Roam (SceneryTrees.Break, TreeBreaker).
    public sealed class RoamWorld : MonoBehaviour
    {
        [Serializable] public sealed class Item { public string id, kind, title, house; public float[] pos; public float yaw, height, angle, width, gap, runup, radius; public int count; }
        [Serializable] sealed class Data { public Item[] items; }
        public static RoamWorld Current { get; private set; }
        public static bool Disabled; // checks only (-p1300off: the world before this round); never set in play
        public static string Override; // checks only (-p1300data <file>: items read from that file instead of the built-in one)
        public static Item[] Load()
        {
            string text = !string.IsNullOrEmpty(Override) && System.IO.File.Exists(Override) ? System.IO.File.ReadAllText(Override) : Resources.Load<TextAsset>("RoamWorld")?.text;
            return string.IsNullOrEmpty(text) ? Array.Empty<Item>() : JsonUtility.FromJson<Data>(text)?.items ?? Array.Empty<Item>();
        }
        static bool IsRoam(Scene scene) => scene.name == RaceFlow.RoamScene;
        static Vector3 Dir(Item i) => Quaternion.Euler(0, i.yaw, 0) * Vector3.forward;
        static Vector3 At(Item i) => new(i.pos[0], 0, i.pos[1]);
        public enum Mark { Ramp, Smash, Flip }
        // the map's and minimap's icons: where, what, the item's number
        public readonly List<(Vector3 at, Mark mark, string id)> Icons = new();
        public readonly List<string> Report = new();
        public Item[] Items { get; private set; } = Array.Empty<Item>();

        // ---------------------------------------------------------------- sizes (metres)
        const float LandSlope = 18;
        static float Lip(Item i) => i.height / (1 - Mathf.Cos(i.angle * Mathf.Deg2Rad)) * Mathf.Sin(i.angle * Mathf.Deg2Rad); // the kicker's length to its lip
        static float H(Item i, float fallback) => i.height > 0 ? i.height : fallback;
        public static void Defaults(Item i)
        {
            switch (i.kind)
            {
                case "kicker": i.height = H(i, 2.4f); i.angle = i.angle > 0 ? i.angle : 26; i.width = i.width > 0 ? i.width : 9; break;
                case "table": i.height = H(i, 5.5f); i.angle = i.angle > 0 ? i.angle : 32; i.width = i.width > 0 ? i.width : 12; i.gap = i.gap > 0 ? i.gap : 26; break;
                case "gap": case "house": i.height = H(i, 3.6f); i.angle = i.angle > 0 ? i.angle : 30; i.width = i.width > 0 ? i.width : 10; i.gap = i.gap > 0 ? i.gap : 30; break;
                case "through": i.height = H(i, 2.6f); i.angle = i.angle > 0 ? i.angle : 15; i.width = i.width > 0 ? i.width : 9; i.gap = i.gap > 0 ? i.gap : 5; break;
                case "flip": i.height = H(i, 4.5f); i.angle = i.angle > 0 ? i.angle : 56; i.width = i.width > 0 ? i.width : 9; break;
                case "loop": i.radius = i.radius > 0 ? i.radius : 13; break;
                case "smash": i.count = i.count > 0 ? i.count : 16; break;
            }
            if (i.runup <= 0) i.runup = i.kind is "smash" or "loop" or "flip" ? 0 : 60;
        }
        public const float BarnLength = 16, BarnDeck = 2.0f;
        // the ground an item covers, its run-up and landing included (along: from the run-up's start to past the landing; half: across)
        static bool Covers(Item i, float x, float z, float margin)
        {
            var d = new Vector3(x, 0, z) - At(i); d.y = 0;
            if (i.kind is "smash") return d.magnitude < 9 + margin;
            if (i.kind is "loop") return d.magnitude < i.radius + 6 + margin;
            var f = Dir(i); float along = Vector3.Dot(d, f), across = Mathf.Abs(Vector3.Dot(d, Vector3.Cross(Vector3.up, f)));
            float back = Lip(i) + i.runup, ahead = i.kind switch { "table" => i.gap + 60, "gap" => i.gap + 60, "house" => i.gap + 60, "through" => i.gap + BarnLength + 50, "flip" => 2, _ => 45 };
            float half = i.width * .5f + (along < -Lip(i) ? 1 : 3);
            if (i.kind == "house" && along > 4 && along < i.gap - 6) return false; // the house itself is left as it is
            return along > -back - margin && along < ahead + margin && across < half + margin;
        }

        // ---------------------------------------------------------------- 1. before the scenery is fitted: the trees where the items are
        public static readonly List<string> Cleared = new();
        public static void Clear(Scene scene)
        {
            Cleared.Clear(); if (Disabled || !IsRoam(scene)) return;
            var items = Load(); foreach (var i in items) Defaults(i); if (items.Length == 0) return;
            bool Covered(float x, float z) { foreach (var i in items) if (Covers(i, x, z, 0)) return true; return false; }
            int trunks = 0, pieces = 0;
            foreach (var box in scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<BoxCollider>(true)))
            {
                if (!box || !box.enabled || box.name.IndexOf("trunk", StringComparison.OrdinalIgnoreCase) < 0) continue;
                var t = box.transform; var bottom = t.TransformPoint(box.center - Vector3.up * box.size.y * .5f);
                if (!Covered(bottom.x, bottom.z)) continue;
                var size = Vector3.Scale(box.size, t.lossyScale);
                SceneryTrees.Cleared.Add((bottom, Mathf.Min(Mathf.Abs(size.x), Mathf.Abs(size.z)), Mathf.Abs(size.y), "Free Roam world item (1.3)", t.name));
                box.enabled = false; if (t.GetComponents<Component>().Length == 2 && t.childCount == 0) t.gameObject.SetActive(false); trunks++;
            }
            foreach (var r in scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<MeshRenderer>(true)))
            {
                if (!SceneryTrees.IsOldVegetation(r)) continue;
                var b = r.bounds; if (!items.Any(i => { var a = At(i); return b.min.x < a.x + 200 && b.max.x > a.x - 200 && b.min.z < a.z + 200 && b.max.z > a.z - 200; })) continue;
                pieces += Hwy92Roadside.TrimPieces(r.GetComponent<MeshFilter>(), Covered);
            }
            Cleared.Add($"{trunks} tree trunks and {pieces} old vegetation pieces where the {items.Length} items, their run-ups and landings are");
            Debug.Log("Free Roam world: removed " + Cleared[^1]);
        }

        // ---------------------------------------------------------------- 2. the items
        public static RoamWorld Attach(GameObject host) => host.GetComponent<RoamWorld>() ?? host.AddComponent<RoamWorld>();
        RaceDirector race; Transform root; static Material building;
        void Start() { if (!Disabled && IsRoam(gameObject.scene)) Build(gameObject.scene); }
        void OnDestroy() { if (Current == this) Current = null; }
        static readonly Color Dirt = StuntPark.DirtColour, DirtSide = StuntPark.DirtColour * .85f, Ply = new(.74f, .60f, .42f), PlyDark = new(.55f, .43f, .29f), Asphalt = new(.33f, .33f, .34f),
            Berm = new(.56f, .55f, .52f), Red = new(.72f, .14f, .12f), White = new(.90f, .89f, .86f), Siding = new(.70f, .66f, .58f), Roof = new(.30f, .22f, .18f), Hay = new(.86f, .70f, .34f),
            Crate = new(.62f, .45f, .26f), Cone = new(1f, .42f, .08f), Box = new(.70f, .56f, .38f);
        static void Set(Color c, ref Color o) { o = c; c.a = 1; }

        // the top static surface under a point (not trunks, triggers, vehicles, the world edge or this round's own items)
        float Ground(float x, float z, float fallback = float.NaN)
        {
            float best = float.NaN;
            foreach (var h in Physics.RaycastAll(new Vector3(x, 600, z), Vector3.down, 1200, ~0, QueryTriggerInteraction.Ignore))
            {
                var c = h.collider; if (c.isTrigger || c.attachedRigidbody || h.normal.y < .45f) continue;
                if (c.name.IndexOf("trunk", StringComparison.OrdinalIgnoreCase) >= 0 || c.name.StartsWith("World edge")) continue;
                if (root && c.transform.IsChildOf(root)) continue;
                if (float.IsNaN(best) || h.point.y > best) best = h.point.y;
            }
            return float.IsNaN(best) ? (float.IsNaN(fallback) ? 0 : fallback) : best;
        }

        public void Build(Scene scene)
        {
            building ??= Resources.Load<Material>("Scenery/Building"); if (!building) { Debug.LogWarning("Free Roam world: material missing"); return; }
            var watch = System.Diagnostics.Stopwatch.StartNew(); Physics.SyncTransforms(); Current = this; race = FindAnyObjectByType<RaceDirector>();
            var go = new GameObject("Free Roam world items (1.3)"); SceneManager.MoveGameObjectToScene(go, scene); root = go.transform;
            Items = Load(); foreach (var i in Items) Defaults(i);
            foreach (var i in Items)
            {
                try
                {
                    switch (i.kind)
                    {
                        case "kicker": case "table": case "gap": case "house": BuildRamp(i); Icons.Add((At(i), Mark.Ramp, i.id)); break;
                        case "through": BuildThrough(i); Icons.Add((At(i), Mark.Ramp, i.id)); break;
                        case "flip": BuildFlip(i); Icons.Add((At(i), Mark.Flip, i.id)); break;
                        case "loop": BuildLoop(i); break;
                        case "smash": BuildSmash(i); Icons.Add((At(i), Mark.Smash, i.id)); break;
                        default: Report.Add($"{i.id}: unknown kind {i.kind}"); break;
                    }
                }
                catch (Exception e) { Report.Add($"{i.id} {i.kind}: not built ({e.Message})"); Debug.LogException(e); }
            }
            Physics.SyncTransforms(); Shown(race && race.FreeRoam);
            Report.Add($"built {Items.Length} items in {watch.ElapsedMilliseconds} ms");
            Debug.Log("Free Roam world: " + string.Join("; ", Report));
        }
        bool shown = true;
        void Shown(bool on) { if (root && shown != on) { shown = on; root.gameObject.SetActive(on); } }
        void Update()
        {
            if (!root) return;
            if (!race) race = FindAnyObjectByType<RaceDirector>();
            bool roam = race && race.FreeRoam; Shown(roam); if (!roam) return;
            // the players' vehicles break trees (one or two players)
            TreeBreaker.Ensure(race.vehicle, race, 1); if (SplitScreen.Active && SplitScreen.Race) TreeBreaker.Ensure(SplitScreen.Race.P2Car, race, 2);
        }
        // 1.3: traffic on an open highway keeps this much further from an end that has a flip ramp or turnaround (Free Roam only), so it never stops in
        // the turnaround, on a flip ramp's run-in or where it lands you (RoadDriver: stop, recycle and respawn points all move back by it)
        public const float TrafficMargin = 60;
        public static float EndMargin(RaceRoad road, bool atEnd)
        {
            var w = Current; if (!w || !w.shown || !road || road.points == null || road.points.Length < 2) return 0;
            var end = atEnd ? road.points[^1] : road.points[0];
            foreach (var i in w.Items) if ((i.kind == "flip" || i.kind == "loop") && Vector3.Distance(Flat(At(i)), Flat(end)) < 90) return TrafficMargin;
            return 0;
        }
        // the players now (for the smash spots' and trees' reset)
        public static IEnumerable<Vector3> Players()
        {
            var race = Current ? Current.race : null; if (!race) race = UnityEngine.Object.FindAnyObjectByType<RaceDirector>(); if (!race) yield break;
            if (race.vehicle) yield return race.vehicle.Body.position;
            if (SplitScreen.Active && SplitScreen.Race && SplitScreen.Race.P2Car) yield return SplitScreen.Race.P2Car.Body.position;
        }
        public static float Nearest(Vector3 p) { float d = float.MaxValue; foreach (var q in Players()) d = Mathf.Min(d, Vector3.Distance(Flat(p), Flat(q))); return d; }
        static Vector3 Flat(Vector3 v) { v.y = 0; return v; }

        // ---------------------------------------------------------------- shapes
        static StuntPark.Prof Kicker(float h, float th) { float r = h / (1 - Mathf.Cos(th * Mathf.Deg2Rad)); return new StuntPark.Prof().Arc(r, th); }
        // a profile (s along dir from origin, y up) extruded across `width`, its base following the ground from the profile's start to its end;
        // the sides reach down below the ground under each edge, so nothing floats on a slope
        void Extrude(StuntPark.Prof prof, Vector3 origin, Vector3 dir, float width, Color top, Color side, StuntPark.Mb vis, StuntPark.Mb col, out float baseStart, out float baseEnd)
        {
            dir.y = 0; dir.Normalize(); var right = Vector3.Cross(Vector3.up, dir); var pts = prof.pts; float s0 = pts[0].x, s1 = pts[^1].x;
            baseStart = Ground(origin.x + dir.x * s0, origin.z + dir.z * s0); baseEnd = Ground(origin.x + dir.x * s1, origin.z + dir.z * s1);
            float b0 = baseStart, b1 = baseEnd;
            float Base(float s) => Mathf.Lerp(b0, b1, Mathf.InverseLerp(s0, s1, s));
            Vector3 Edge(int i, float sign) { var p = pts[i]; return origin + dir * p.x + right * (sign * width * .5f) + Vector3.up * (Base(p.x) + p.y); }
            Vector3 Foot(Vector3 p) => new(p.x, Mathf.Min(Ground(p.x, p.z, p.y), Base(Vector3.Dot(p - origin, dir))) - .8f, p.z);
            for (int i = 0; i + 1 < pts.Count; i++)
            {
                Vector3 l0 = Edge(i, -1), r0 = Edge(i, 1), l1 = Edge(i + 1, -1), r1 = Edge(i + 1, 1);
                var ds = pts[i + 1] - pts[i]; var outward = dir * -ds.y + Vector3.up * ds.x; if (outward.sqrMagnitude < 1e-8f) continue;
                vis.Quad(l0, r0, r1, l1, top, outward); col.Quad(l0, r0, r1, l1, top, outward);
                vis.Quad(Foot(r0), Foot(r1), r1, r0, side, right); col.Quad(Foot(r0), Foot(r1), r1, r0, side, right);
                vis.Quad(Foot(l0), Foot(l1), l1, l0, side, -right); col.Quad(Foot(l0), Foot(l1), l1, l0, side, -right);
            }
            void End(int i, Vector3 facing) { var l = Edge(i, -1); var r = Edge(i, 1); vis.Quad(Foot(l), Foot(r), r, l, side, facing); col.Quad(Foot(l), Foot(r), r, l, side, facing); }
            End(0, -dir); End(pts.Count - 1, dir);
        }
        GameObject Piece(string name, StuntPark.Mb vis, StuntPark.Mb col, bool shadows = true)
        {
            var g = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer)); g.transform.SetParent(root, false);
            g.GetComponent<MeshFilter>().sharedMesh = vis.Mesh(name); var r = g.GetComponent<MeshRenderer>(); r.sharedMaterial = building; r.shadowCastingMode = shadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
            if (col != null && !col.Empty) { var c = new GameObject("Ground_" + name + " surface"); c.transform.SetParent(g.transform, false); c.AddComponent<MeshCollider>().sharedMesh = col.Mesh(name + " collider", true); }
            return g;
        }
        public readonly Dictionary<string, (Vector3 lip, Vector3 dir, float lipY, Vector3 land)> Ramps = new(); // checks: each ramp's lip and landing

        void BuildRamp(Item i)
        {
            var f = Dir(i); var lip = At(i); float lipS = Lip(i); var vis = new StuntPark.Mb(); var col = new StuntPark.Mb();
            bool ply = i.kind == "kicker";
            var k = Kicker(i.height, i.angle);
            if (i.kind == "table") k.Flat(i.gap).Landing(LandSlope + 2, 4, 14); else k.Drop();
            Extrude(k, lip - f * lipS, f, i.width, ply ? Ply : Dirt, ply ? PlyDark : DirtSide, vis, col, out float b0, out _);
            Vector3 land = lip + f * (i.kind == "table" ? i.gap + 6 : 14);
            if (i.kind is "gap" or "house")
            {
                // the landing: up to 85% of the kicker's height, a short table, then the slope down
                var l = new StuntPark.Prof().Up(i.height * .85f); l.Flat(2).Landing(LandSlope, 3, 12);
                Extrude(l, lip + f * i.gap, f, i.width + 4, Dirt, DirtSide, vis, col, out _, out _); land = lip + f * (i.gap + 4);
            }
            Piece($"Free Roam ramp {i.id} ({i.kind})", vis, col);
            Ramps[i.id] = (lip, f, b0 + i.height, land);
            Report.Add($"{i.id} {i.kind} at {lip.x:F0},{lip.z:F0} facing {i.yaw:F0}: lip {i.height:F1} m at {i.angle:F0} degrees, {i.width:F0} m wide{(i.gap > 0 && i.kind != "kicker" ? $", {i.gap:F0} m to the landing" : "")}");
        }

        // the barn: a kicker, then a barn whose deck starts at its front; plank walls (breakable) across both open ends; out the back onto a landing slope
        void BuildThrough(Item i)
        {
            var f = Dir(i); var right = Vector3.Cross(Vector3.up, f); var lip = At(i); var vis = new StuntPark.Mb(); var col = new StuntPark.Mb();
            var k = Kicker(i.height, i.angle); k.Drop(); Extrude(k, lip - f * Lip(i), f, i.width, Ply, PlyDark, vis, col, out _, out _);
            var front = lip + f * i.gap; float w = i.width + 2;
            var deck = new StuntPark.Prof().Up(BarnDeck); deck.Flat(BarnLength + .5f).Landing(14, 3, 10);
            Extrude(deck, front, f, w - .2f, Ply, PlyDark, vis, col, out float b0, out _);
            Piece($"Free Roam ramp {i.id} (through, kicker and deck)", vis, col);
            // the shell: two side walls, a pitched roof, gable walls beside and above the openings (solid); the openings closed by planks (breakable)
            var shell = new StuntPark.Mb(); var solid = new StuntPark.Mb(); float y0 = b0 - .6f, wallTop = b0 + BarnDeck + 5.2f, ridge = wallTop + 2.6f, open = 6.4f, openTop = b0 + BarnDeck + 4.0f;
            var mid = front + f * (BarnLength * .5f);
            foreach (int side in new[] { -1, 1 })
            {
                var c = mid + right * (side * w * .5f); var bottom = new Vector3(c.x, y0, c.z);
                shell.Box(bottom, f, new Vector3(.3f, wallTop - y0, BarnLength), Siding); solid.Box(bottom, f, new Vector3(.3f, wallTop - y0, BarnLength), Siding);
            }
            foreach (int end in new[] { 0, 1 })
            {
                var e = front + f * (end * BarnLength); var facing = end == 0 ? -f : f;
                float pier = (w - open) * .5f;
                foreach (int side in new[] { -1, 1 })
                {
                    var c = e + right * (side * (open * .5f + pier * .5f)); var b = new Vector3(c.x, y0, c.z);
                    shell.Box(b, f, new Vector3(pier, wallTop - y0, .3f), Siding); solid.Box(b, f, new Vector3(pier, wallTop - y0, .3f), Siding);
                }
                var above = new Vector3(e.x, openTop, e.z); shell.Box(above, f, new Vector3(open, wallTop - openTop, .3f), Siding); solid.Box(above, f, new Vector3(open, wallTop - openTop, .3f), Siding);
                var under = new Vector3(e.x, y0, e.z); if (end == 1) { shell.Box(under, f, new Vector3(open, b0 + BarnDeck - y0, .3f), Siding); }
                // the gable
                Vector3 gl = e - right * (w * .5f) + Vector3.up * wallTop, gr = e + right * (w * .5f) + Vector3.up * wallTop, gt = e + Vector3.up * ridge;
                gl.y = wallTop; gr.y = wallTop; gt.y = ridge; shell.TriOut(gl, gr, gt, Siding, facing); shell.TriOut(gl, gr, gt, Siding, -facing);
            }
            foreach (int side in new[] { -1, 1 })
            {
                Vector3 a = front - f * .6f + right * (side * (w * .5f + .5f)), b = front + f * (BarnLength + .6f) + right * (side * (w * .5f + .5f));
                a.y = b.y = wallTop - .3f; Vector3 ta = front - f * .6f, tb = front + f * (BarnLength + .6f); ta.y = tb.y = ridge + .1f;
                shell.Quad(a, b, tb, ta, Roof, Vector3.up + right * side); shell.Quad(a, b, tb, ta, Roof, -(Vector3.up + right * side));
                solid.Quad(a, b, tb, ta, Roof, Vector3.up + right * side);
            }
            Piece($"Free Roam barn {i.id}", shell, solid);
            // the planks: three panels at each end, from the deck to the opening's top (the front ones from the kicker's flight height down to the deck)
            foreach (int end in new[] { 0, 1 })
            {
                var e = front + f * (end * BarnLength) + (end == 0 ? -f : f) * .05f;
                for (int p = 0; p < 3; p++)
                {
                    var c = e + right * ((p - 1) * open / 3f); float yb = b0 + BarnDeck - (end == 0 ? .6f : 0), yt = openTop;
                    Plank(c, f, open / 3f - .05f, yb, yt, $"Free Roam barn {i.id} plank wall");
                }
            }
            Ramps[i.id] = (lip, f, Ground(lip.x, lip.z) + i.height, front + f * (BarnLength + 6));
            Report.Add($"{i.id} through at {lip.x:F0},{lip.z:F0} facing {i.yaw:F0}: kicker {i.height:F1} m, barn {BarnLength:F0} m long (deck {BarnDeck:F1} m), plank walls both ends");
        }
        void Plank(Vector3 centre, Vector3 f, float width, float yb, float yt, string name)
        {
            var g = new GameObject(name); g.transform.SetParent(root, false); g.transform.SetPositionAndRotation(new Vector3(centre.x, yb, centre.z), Quaternion.LookRotation(f));
            var m = new StuntPark.Mb(); int n = Mathf.Max(3, Mathf.RoundToInt((yt - yb) / .32f)); float step = (yt - yb) / n;
            for (int k = 0; k < n; k++) m.Box(new Vector3(0, k * step + .02f, 0), Vector3.forward, new Vector3(width, step - .04f, .08f), k % 2 == 0 ? Ply : PlyDark);
            m.Box(new Vector3(-width * .5f + .1f, 0, -.06f), Vector3.forward, new Vector3(.12f, yt - yb, .06f), PlyDark); m.Box(new Vector3(width * .5f - .1f, 0, -.06f), Vector3.forward, new Vector3(.12f, yt - yb, .06f), PlyDark);
            Visual(g.transform, m, name);
            var box = g.AddComponent<BoxCollider>(); box.center = new Vector3(0, (yt - yb) * .5f, 0); box.size = new Vector3(width, yt - yb, .8f);
            var bp = g.AddComponent<BreakableProp>(); bp.surface = SmashAudio.Surface.Wood; props.Add(bp);
        }
        static void Visual(Transform parent, StuntPark.Mb m, string name)
        {
            var v = new GameObject(name + " visual", typeof(MeshFilter), typeof(MeshRenderer)); v.transform.SetParent(parent, false);
            v.GetComponent<MeshFilter>().sharedMesh = m.Mesh(name); v.GetComponent<MeshRenderer>().sharedMaterial = building;
        }

        // ---------------------------------------------------------------- the dead ends
        public readonly List<FlipRamp> Flips = new();
        void BuildFlip(Item i)
        {
            var f = Dir(i); var lip = At(i); var vis = new StuntPark.Mb(); var col = new StuntPark.Mb();
            var k = Kicker(i.height, i.angle); k.Flat(.8f).Drop(); Extrude(k, lip - f * Lip(i), f, i.width, Ply, Red, vis, col, out float b0, out _);
            // red and white chevrons on the lead-in so it reads from down the road
            var right = Vector3.Cross(Vector3.up, f); var stripes = new StuntPark.Mb();
            for (int c = 0; c < 5; c++)
            {
                var p = lip - f * (Lip(i) + 6 + c * 3.2f); float y = Ground(p.x, p.z) + .03f; p.y = y;
                var a = p - right * (i.width * .45f); var b = p + right * (i.width * .45f);
                stripes.Quad(a, b, b + f * 1.4f, a + f * 1.4f, c % 2 == 0 ? Red : White, Vector3.up);
            }
            var g = Piece($"Free Roam flip ramp {i.id}", vis, col); var sg = Piece($"Free Roam flip ramp {i.id} stripes", stripes, null, false); sg.transform.SetParent(g.transform, true);
            var trigger = new GameObject("Flip ramp launch"); trigger.transform.SetParent(g.transform, false);
            // the launch zone: the whole ramp, from its foot to its lip and 6 m over it (a vehicle that reaches the ramp heading up it is launched at once)
            trigger.transform.SetPositionAndRotation(lip - f * (Lip(i) * .5f - .5f) + Vector3.up * (b0 + i.height * .5f + 2), Quaternion.LookRotation(f));
            var box = trigger.AddComponent<BoxCollider>(); box.isTrigger = true; box.size = new Vector3(i.width + 2, i.height + 6, Lip(i) + 1.5f);
            var flip = trigger.AddComponent<FlipRamp>(); flip.forward = f; flip.landing = lip - f * (Lip(i) + 18); flip.landing.y = Ground(flip.landing.x, flip.landing.z); flip.id = i.id; Flips.Add(flip);
            Ramps[i.id] = (lip, f, b0 + i.height, flip.landing);
            Report.Add($"{i.id} flip at {lip.x:F0},{lip.z:F0} facing {i.yaw:F0}: {i.height:F1} m at {i.angle:F0} degrees, lands you {Lip(i) + 18:F0} m back facing the other way");
        }
        // a banked turnaround: a paved disc draped on the ground, a bank round its rim rising 2.8 m over 6 m, open toward the road coming in
        void BuildLoop(Item i)
        {
            var f = Dir(i); var c = At(i); float R = i.radius; var vis = new StuntPark.Mb(); var col = new StuntPark.Mb();
            const int seg = 48; const float bank = 6, rise = 2.8f, opening = 34;
            float inAngle = Mathf.Atan2(-f.x, -f.z) * Mathf.Rad2Deg; // where the road comes in (behind the loop's centre)
            Vector3 P(float a, float r, float lift) { var d = new Vector3(Mathf.Sin(a * Mathf.Deg2Rad), 0, Mathf.Cos(a * Mathf.Deg2Rad)); var p = c + d * r; p.y = Ground(p.x, p.z) + .05f + lift; return p; }
            float Height(float a) { float off = Mathf.Abs(Mathf.DeltaAngle(a, inAngle)); return rise * Mathf.SmoothStep(0, 1, Mathf.InverseLerp(opening, opening + 28, off)); }
            int rings = Mathf.CeilToInt(R / 2.5f);
            for (int s = 0; s < seg; s++)
            {
                float a0 = s * 360f / seg, a1 = (s + 1) * 360f / seg;
                for (int r = 0; r < rings; r++)
                {
                    float r0 = R * r / rings, r1 = R * (r + 1) / rings;
                    Vector3 p00 = P(a0, r0, 0), p01 = P(a0, r1, 0), p10 = P(a1, r0, 0), p11 = P(a1, r1, 0);
                    vis.Quad(p00, p01, p11, p10, Asphalt, Vector3.up); col.Quad(p00, p01, p11, p10, Asphalt, Vector3.up);
                }
                // the bank: a concave curve from the disc's rim up to its top, then down its back to the ground
                float h0 = Height(a0), h1 = Height(a1); if (h0 < .05f && h1 < .05f) continue;
                const int steps = 4;
                for (int k = 0; k < steps; k++)
                {
                    float t0 = k / (float)steps, t1 = (k + 1) / (float)steps;
                    Vector3 q00 = P(a0, R + bank * t0, h0 * t0 * t0), q01 = P(a0, R + bank * t1, h0 * t1 * t1), q10 = P(a1, R + bank * t0, h1 * t0 * t0), q11 = P(a1, R + bank * t1, h1 * t1 * t1);
                    vis.Quad(q00, q01, q11, q10, Berm, Vector3.up - (q00 - c).normalized); col.Quad(q00, q01, q11, q10, Berm, Vector3.up - (q00 - c).normalized);
                }
                Vector3 t0p = P(a0, R + bank, h0), t1p = P(a1, R + bank, h1), b0p = P(a0, R + bank + 1.2f, h0), b1p = P(a1, R + bank + 1.2f, h1);
                vis.Quad(t0p, b0p, b1p, t1p, Berm, Vector3.up); col.Quad(t0p, b0p, b1p, t1p, Berm, Vector3.up);
                Vector3 g0 = P(a0, R + bank + 1.2f + h0 * 1.5f, -.5f), g1 = P(a1, R + bank + 1.2f + h1 * 1.5f, -.5f);
                vis.Quad(b0p, g0, g1, b1p, DirtSide, (b0p - c).normalized + Vector3.up); col.Quad(b0p, g0, g1, b1p, DirtSide, (b0p - c).normalized + Vector3.up);
            }
            Piece($"Free Roam turnaround {i.id}", vis, col, false);
            Report.Add($"{i.id} loop at {c.x:F0},{c.z:F0} (road coming in heading {i.yaw:F0}): a {R * 2:F0} m paved turnaround, banked {rise:F1} m round its rim");
        }

        // ---------------------------------------------------------------- smash spots
        readonly List<BreakableProp> props = new();
        public readonly List<(Item item, List<BreakableProp> props)> Spots = new();
        void BuildSmash(Item i)
        {
            var f = Dir(i); var right = Vector3.Cross(Vector3.up, f); var c = At(i); var list = new List<BreakableProp>();
            var rng = new System.Random(i.id.Aggregate(7, (h, ch) => h * 31 + ch));
            float R() => (float)rng.NextDouble();
            // rows across the spot: a hay bale wall in the middle, crate stacks and boxes either side, cones in a chicane line on the way in
            // a hay bale wall across the middle (two high in the centre), crate stacks just either side, boxes strewn in the way in, a cone chicane before it
            for (int n = 0; n < 5; n++) list.Add(Prop("hay", c + right * ((n - 2) * 1.9f), f, 0));
            for (int n = 0; n < 3; n++) list.Add(Prop("hay", c + right * ((n - 1) * 1.9f), f, 1));
            foreach (int side in new[] { -1, 1 })
            {
                var s = c + right * (side * 5.6f) + f * (R() - .5f);
                list.Add(Prop("crate", s, f, 0)); list.Add(Prop("crate", s + right * side * 1.1f, f, 0)); list.Add(Prop("crate", s + right * side * .55f, f, 1));
            }
            for (int b = 0; b < 8; b++) list.Add(Prop("box", c - f * (3 + R() * 3) + right * ((b - 3.5f) * 1.1f + (R() - .5f) * .4f), f, 0));
            for (int n = 0; n < 6; n++) list.Add(Prop("cone", c - f * (9 + n * 2.2f) + right * ((n % 2 == 0 ? -1 : 1) * 1.1f), f, 0));
            Spots.Add((i, list)); props.AddRange(list);
            Report.Add($"{i.id} smash at {c.x:F0},{c.z:F0}: {list.Count} props (hay bales, crates, boxes, cones)");
        }
        BreakableProp Prop(string kind, Vector3 at, Vector3 f, int level)
        {
            float y = Ground(at.x, at.z); var m = new StuntPark.Mb(); Vector3 size; SmashAudio.Surface surface = SmashAudio.Surface.Wood;
            switch (kind)
            {
                case "hay": size = new Vector3(1.8f, 1.5f, 1.5f); m.Cyl(new Vector3(-.9f, .75f, 0), new Vector3(.9f, .75f, 0), .75f, Hay, 10, Hay * .85f); break;
                case "crate": size = new Vector3(1.05f, 1.05f, 1.05f); m.Box(Vector3.zero, Vector3.forward, size, Crate); m.Box(new Vector3(0, .45f, 0), Vector3.forward, new Vector3(1.09f, .12f, 1.09f), Crate * .8f); break;
                case "box": size = new Vector3(.7f, .6f, .7f); m.Box(Vector3.zero, Vector3.forward, size, Box); m.Box(new Vector3(0, .58f, 0), Vector3.forward, new Vector3(.72f, .04f, .2f), Box * .75f); surface = SmashAudio.Surface.Sign; break;
                default: size = new Vector3(.5f, .75f, .5f); m.Box(Vector3.zero, Vector3.forward, new Vector3(.5f, .05f, .5f), Cone * .5f); m.Cyl(new Vector3(0, .05f, 0), new Vector3(0, .75f, 0), .17f, Cone, 8, White); surface = SmashAudio.Surface.Sign; break;
            }
            var g = new GameObject("Free Roam smash " + kind); g.transform.SetParent(root, false);
            g.transform.SetPositionAndRotation(new Vector3(at.x, y + level * size.y, at.z), Quaternion.LookRotation(f));
            Visual(g.transform, m, "Free Roam smash " + kind);
            var box = g.AddComponent<BoxCollider>(); box.center = new Vector3(0, size.y * .5f, 0); box.size = size + new Vector3(.2f, .1f, .2f);
            var bp = g.AddComponent<BreakableProp>(); bp.surface = surface; bp.impactSpeed = 1.5f; return bp;
        }
        // a spot (and a barn's planks) is put back once nobody is within 70 m of it and nothing of it is still flying
        float nextReset;
        void LateUpdate()
        {
            if (!root || !shown || Time.time < nextReset) return; nextReset = Time.time + 1;
            foreach (var p in props) if (p && (p.IsBroken || p.PendingRestore) && Nearest(p.Home) > 70) p.Restore();
        }
    }

    // A flip ramp's launch: a player vehicle reaching its lip heading up it is thrown high (to 13 m above the lip) toward a point back down the road, and
    // turned round in the air (assisted: its rotation is set each step), so it lands upright facing back the way it came. Any vehicle; both modes.
    public sealed class FlipRamp : MonoBehaviour
    {
        public Vector3 forward, landing; public string id; public const float Apex = 13;
        public static int Launches { get; private set; } public static string Last { get; private set; } // checks
        readonly Dictionary<ArcadeVehicle, float> recent = new();
        void OnTriggerEnter(Collider other) => Try(other);
        void OnTriggerStay(Collider other) => Try(other);
        void Try(Collider other)
        {
            var car = other.attachedRigidbody ? other.attachedRigidbody.GetComponent<ArcadeVehicle>() : null;
            if (!car || car.Body.isKinematic || car.Puppet || car.GetComponent<FlipFlight>()) return;
            if (recent.TryGetValue(car, out float t) && Time.time - t < 3) return;
            var v = car.Body.linearVelocity; if (Vector3.Dot(v, forward) < 4) return;
            recent[car] = Time.time; car.gameObject.AddComponent<FlipFlight>().Begin(car, forward, landing); Launches++;
        }
    }
    public sealed class FlipFlight : MonoBehaviour
    {
        ArcadeVehicle car; Vector3 dir; Quaternion start; float t, flight, sign;
        public void Begin(ArcadeVehicle vehicle, Vector3 forward, Vector3 landing)
        {
            car = vehicle; dir = Vector3.ProjectOnPlane(forward, Vector3.up).normalized; start = car.Body.rotation; sign = UnityEngine.Random.value < .5f ? -1 : 1;
            float g = -Physics.gravity.y, vy = Mathf.Sqrt(2 * g * FlipRamp.Apex), drop = car.Body.position.y - (landing.y + .8f);
            flight = (vy + Mathf.Sqrt(Mathf.Max(0, vy * vy + 2 * g * drop))) / g;
            var flat = landing - car.Body.position; flat.y = 0;
            car.Body.linearVelocity = flat / flight + Vector3.up * vy; car.Body.angularVelocity = Vector3.zero;
        }
        void FixedUpdate()
        {
            if (!car) { Destroy(this); return; }
            t += Time.fixedDeltaTime; float k1 = Mathf.SmoothStep(0, 1, Mathf.Clamp01(t / (flight * .35f))), k2 = Mathf.SmoothStep(0, 1, Mathf.Clamp01((t - flight * .1f) / (flight * .65f)));
            var level = Quaternion.LookRotation(dir, Vector3.up);
            var rot = Quaternion.AngleAxis(sign * 180 * k2, Vector3.up) * Quaternion.Slerp(start, level, k1);
            car.Body.MoveRotation(rot); car.Body.angularVelocity = Vector3.zero;
            if ((t > flight * .6f && car.GroundedWheels >= 2) || t > flight + 2 || car.Body.isKinematic) { var last = Quaternion.AngleAxis(sign * 180, Vector3.up) * level; car.Body.rotation = last; car.Body.angularVelocity = Vector3.zero; Destroy(this); }
        }
    }

    // On a player's vehicle in Free Roam: a tree trunk just ahead is broken before the vehicle reaches it when the vehicle is fast enough for that trunk
    // (thin ones easily, big trunks need real speed); breaking it costs speed (less in HIGH MODE). Slower, the trunk stands and stops you as before.
    public sealed class TreeBreaker : MonoBehaviour
    {
        ArcadeVehicle car; RaceDirector race; int player = 1;
        static readonly RaycastHit[] hits = new RaycastHit[16];
        public static int Broken { get; private set; } public static string Last { get; private set; } // checks
        public static void Ensure(ArcadeVehicle car, RaceDirector race, int player)
        {
            if (!car) return; var b = car.GetComponent<TreeBreaker>(); if (!b) b = car.gameObject.AddComponent<TreeBreaker>();
            b.car = car; b.race = race; b.player = player;
        }
        // the speed a trunk of this width needs (m/s): about 16 mph for the thinnest, about 54 mph for a trunk a metre thick
        public static float Needed(float width) => Mathf.Lerp(7, 24, Mathf.InverseLerp(.5f, 1f, width)); // the trunks are 0.52-1.0 m wide
        void FixedUpdate()
        {
            if (!car || !race || !race.FreeRoam || !Scenery.New || car.Body.isKinematic) return;
            var trees = SceneryWorld.Current ? SceneryWorld.Current.Trees : null; if (!trees || !trees.Breakable) return;
            var v = car.Body.linearVelocity; var flat = Vector3.ProjectOnPlane(v, Vector3.up); float speed = flat.magnitude; if (speed < 5.5f) return;
            var dir = flat / speed; float reach = speed * Time.fixedDeltaTime * 2 + 2.6f;
            int n = Physics.SphereCastNonAlloc(car.Body.position + Vector3.up * .5f, .9f, dir, hits, reach, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < n; i++)
            {
                if (!(hits[i].collider is BoxCollider box) || !trees.IsTree(box, out float width)) continue;
                float need = Needed(width); if (speed < need) continue;
                if (!trees.Break(box, dir, speed)) continue;
                float size = Mathf.InverseLerp(.5f, 1f, width), loss = (.15f + .3f * size) * (race.HighFor(player) ? .5f : 1);
                car.Body.linearVelocity = v - flat * loss; Broken++; Last = $"player {player} broke a trunk {width:F2} m wide at {speed:F1} m/s (needs {need:F1}), speed -{loss * 100:F0}%";
            }
        }
    }
}
