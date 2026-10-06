using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Racer
{
    // 0.80 Part A.1 (BUG-001-003): no post, bollard, marker, sign post or prop stands on a paved road or driveway.
    // Run at load in every scene, before the scenery is built. A candidate is a small upright object (footprint at most
    // 1.2 m, 0.2-6 m tall) whose base is on a paved surface (SceneryTrees.OnDrivable: the terrain's road colour or a
    // paved road / driveway piece; trails are not paved). FreeRoamWorld: every one is removed with its colliders.
    // Course scenes (section 5A): a race marker (shortcut edge posts, gate markings) or anything with a collider within
    // 12 m of a race line or shortcut stays and is listed in Kept; the rest is removed as in FreeRoamWorld.
    public static class RoadPosts
    {
        public static readonly List<string> Removed = new(), Kept = new();
        static readonly string[] Skip = { "ground", "road", "lane", "median", "line", "dash", "yellow", "paving", "pavement", "driveway", "curb", "kerb", "shoulder", "seam", "arrow", "deck", "ramp", "landing", "bridge" };
        static readonly string[] RaceMarker = { "trail edge", "path edge", "gate", "checkpoint", "marker", "finish", "start", "arrow", "guidance", "shortcut", "optional", "sign", "course authoring" };
        // a paved road or driveway (the terrain's road colour, or a road / driveway / asphalt piece); not a drain or a deck
        static readonly string[] RoadSurface = { "paved road", "road", "driveway", "asphalt", "street", "hwy", "highway", "pavement", "parking", "lane" };
        static bool Has(string n, string[] words) => words.Any(w => n.IndexOf(w, System.StringComparison.OrdinalIgnoreCase) >= 0);

        public static void Clear(Scene scene)
        {
            Removed.Clear(); Kept.Clear();
            bool roam = scene.name == RaceFlow.RoamScene;
            List<Vector3[]> lines = null;
            var done = new HashSet<Transform>();
            foreach (var r in scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<MeshRenderer>()))
            {
                if (!r.enabled || r.GetComponent<TextMesh>() || !r.TryGetComponent<MeshFilter>(out var f) || !f.sharedMesh) continue;
                var b = r.bounds; if (b.size.x > 1.2f || b.size.z > 1.2f || b.size.y < .2f || b.size.y > 6) continue;
                if (Has(r.name, Skip) || r.name.StartsWith("Ground")) continue;
                if (r.GetComponentInParent<ArcadeVehicle>() || r.GetComponentInParent<AmbientVehicle>() || r.GetComponentInParent<RaceGate>() || r.GetComponentInParent<ActivitySite>()
                    || r.GetComponentInParent<SceneryBuildings>() || r.GetComponentInParent<StreetSigns>()) continue;
                // people, animals and tree parts are not posts
                var path = Path(r.transform);
                if (Has(path, new[] { "resident", "wildlife", "turkey", "deer", "rider", "tree", "trunk", "canopy", "foliage", "house", "building", "acorn" })) continue;
                // the object that stands there: a sign's post and board go together
                var obj = r.transform; if (obj.parent && obj.parent.childCount <= 6 && Has(obj.parent.name, new[] { "sign", "post", "mailbox", "bollard", "marker" })) obj = obj.parent;
                if (done.Contains(obj)) continue;
                var cols = obj.GetComponentsInChildren<Collider>().Where(c => c.enabled).ToArray();
                foreach (var c in cols) c.enabled = false;
                var bottom = new Vector3(b.center.x, b.min.y + .05f, b.center.z);
                bool paved = SceneryTrees.OnDrivable(bottom, null, out var what);
                // FreeRoamWorld has no shortcut racing: an old shortcut's edge marker within 4 m of the pavement (on its
                // dirt verge, beside the road) goes too (BUG-002/003: Dan saw these as posts in the road)
                if (roam && !(paved && Has(what ?? "", RoadSurface)) && Has(Path(obj), new[] { "trail edge", "path edge" }))
                    for (float rad = 1; rad <= 4 && !(paved && Has(what ?? "", RoadSurface)); rad += 1)
                        for (int k = 0; k < 8 && !(paved && Has(what ?? "", RoadSurface)); k++)
                        {
                            var o = Quaternion.Euler(0, k * 45, 0) * Vector3.forward * rad;
                            paved = SceneryTrees.OnDrivable(bottom + o, null, out what);
                            if (paved && Has(what ?? "", RoadSurface)) what += $" ({rad:F0} m away)";
                        }
                foreach (var c in cols) c.enabled = true;
                if (!paved || !Has(what ?? "", RoadSurface)) continue;
                done.Add(obj);
                string line = $"{Path(obj)} at ({bottom.x:F1}, {b.min.y:F2}, {bottom.z:F1}) on {what}{(cols.Length > 0 ? ", " + cols.Length + " collider(s)" : ", no collider")}";
                if (!roam)
                {
                    if (Has(Path(obj), RaceMarker)) { Kept.Add(line + ": race marker, kept"); continue; }
                    if (cols.Length > 0)
                    {
                        lines ??= RaceLines(scene);
                        float near = lines.Count == 0 ? 999 : lines.Min(l => Distance(l, bottom));
                        if (near < 12) { Kept.Add(line + $": collider {near:F1} m from a race line, kept"); continue; }
                    }
                }
                Removed.Add(line);
                obj.gameObject.SetActive(false); Object.Destroy(obj.gameObject);
            }
            if (Removed.Count + Kept.Count > 0) Debug.Log($"Road posts ({scene.name}): removed {Removed.Count}, kept {Kept.Count}\n" + string.Join("\n", Removed.Concat(Kept)));
        }
        // 0.81 Part 0.1 (BUG-001): the old dark road-name boards on a post ("Trickum Road" at Hwy 92, "South Cherokee Lane"
        // at Hwy 92, "TO Jamerson Road" at S Cherokee / Trickum) are replaced by the 0.79 green street signs. They are
        // breakable props whose only collider is their smash trigger (a vehicle drives through them), so removing them
        // changes no race contact in any scene; one used by a Free Roam smash activity would be kept and listed.
        public static readonly List<string> Boards = new();
        // only the road-name boards; the shortcut speed boards ("Creek Leap > 76 mph" ...) stay
        static readonly string[] NameBoards = { "Breakable sign - Hwy 92", "Breakable sign - Hwy 92 South Cherokee Lane", "Breakable sign - Jamerson Rd" };
        public static void RetireNameBoards(Scene scene)
        {
            Boards.Clear();
            var used = new HashSet<BreakableProp>(scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<ActivitySite>(true)).SelectMany(s => s.props ?? new BreakableProp[0]).Where(p => p));
            foreach (var text in scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<TextMesh>(true)))
            {
                if (text.name != "Road lettering") continue;
                var board = text.GetComponentInParent<BreakableProp>(true); if (!board || System.Array.IndexOf(NameBoards, board.name) < 0) continue;
                if (!board.gameObject.activeSelf) continue;
                var cols = board.GetComponentsInChildren<Collider>(true);
                string line = $"{Path(board.transform)} \"{text.text.Replace("\n", " ")}\" at ({board.transform.position.x:F1}, {board.transform.position.y:F1}, {board.transform.position.z:F1}); colliders: {cols.Length} ({string.Join(", ", cols.Select(c => c.isTrigger ? "trigger" : "solid"))})";
                if (used.Contains(board) || cols.Any(c => !c.isTrigger)) { Boards.Add(line + ": kept (activity prop or solid collider)"); continue; }
                Boards.Add(line + ": removed");
                board.gameObject.SetActive(false); Object.Destroy(board.gameObject);
            }
            if (Boards.Count > 0) Debug.Log($"Road-name boards ({scene.name}):\n" + string.Join("\n", Boards));
        }
        static List<Vector3[]> RaceLines(Scene scene)
        {
            var lines = new List<Vector3[]>();
            foreach (var g in scene.GetRootGameObjects())
            {
                foreach (var d in g.GetComponentsInChildren<RaceDirector>()) if (d.road && d.road.points != null) lines.Add(d.road.points);
                foreach (var w in g.GetComponentsInChildren<WoodlandRoute>()) if (w.points != null && w.points.Length > 1) lines.Add(w.points);
            }
            return lines;
        }
        static float Distance(Vector3[] l, Vector3 p)
        {
            float best = float.MaxValue; var q = new Vector2(p.x, p.z);
            for (int i = 0; i + 1 < l.Length; i++)
            {
                Vector2 a = new(l[i].x, l[i].z), c = new(l[i + 1].x, l[i + 1].z), ab = c - a; float t = ab.sqrMagnitude < 1e-6f ? 0 : Mathf.Clamp01(Vector2.Dot(q - a, ab) / ab.sqrMagnitude);
                best = Mathf.Min(best, (a + ab * t - q).magnitude);
            }
            return best;
        }
        static string Path(Transform t) { var n = t.name; while (t.parent) { t = t.parent; n = t.name + "/" + n; } return n; }
    }
}
