using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Racer
{
    // 0.115: Leviathan, Forward and Reverse. The Street Loop joined to the Mountain Loop at J (the Lake shore connection), raced in the Mountain
    // Loop scenes on roads they already have: Forward in MountainLoop.unity (Climbing Ridge Cut as the main road, the Homeward summit flight),
    // Reverse in MountainLoopReverse.unity (Downhill Ridge Cut, the South Face summit flight). The scenes are not changed: when a Leviathan course
    // is loaded, its line, gates, the Street Loop's shortcuts and the summit flight's stations (Resources/Leviathan/*.json, composed by
    // Tools/Report115/py/leviathan.py) replace the scene's own course on its RaceDirector before anything reads it. Loading the scene
    // any other way gives the Mountain Loop exactly as before.
    public static class LeviathanCourse
    {
        public const string Forward = "Leviathan", Reverse = "LeviathanReverse"; // course keys (RacePlaylists.Scenes)
        // the course key of the scene being loaded / raced (null: the scene's own course)
        public static string Active { get; private set; }
        public static Data Current { get; private set; }
        public static bool Is(string key) => key == Forward || key == Reverse;
        public static string SceneOf(string key) => key == Forward ? "MountainLoop" : key == Reverse ? "MountainLoopReverse" : key;
        // the course a Mountain Loop course opens with (the campaign: Leviathan opens with the Mountain Loop of its direction)
        public static string BaseCourse(string key) => SceneOf(key);
        // every scene load goes through here (LoadingScreen.LoadScene): a Leviathan key loads its Mountain scene with the course selected
        public static string Resolve(string key) { Active = Is(key) ? key : null; Current = null; return SceneOf(key); }
        // the course raced in this scene: its key (Leviathan when selected), else the scene name
        public static string KeyOf(Scene scene) => Active != null && scene.name == SceneOf(Active) ? Active : scene.name;

        [Serializable] public sealed class Gate { public string name, reuse; public float[] pos, rot; public float halfWidth, halfHeight, upperHeight, station; public bool mandatory; }
        [Serializable] public sealed class Branch
        {
            public string title, from; public float[] pts, aiPts; public float entryRoad, exitRoad, halfWidth, recommendedSpeed, undergroundStart, undergroundEnd, entryHeightBelow, entryHeightAbove, entrySpeed, entrySpeedDistance, entryInset, entryMargin;
            public int[] bypassedGates; public bool aiValidated;
        }
        // 0.116 Part A: a turn indicator: "arrow" (a big arrow board before the turn) or "chevron" (a board on the outside of the turn);
        // dir -1 left, +1 right; pos x, z, the road height there (seated on the ground nearest it in the race); yaw: the heading of the traffic it faces
        [Serializable] public sealed class Sign { public string kind, text; public int dir; public float[] pos, alt; public float yaw; public string at; } // alt: other spots (x, z, road height triples); text: a "board" (a shortcut's entry / rejoin)
        // 0.117 Part A: a way off the course closed for the race: "barricade" (timber rails across it, half: its half width, with a one-sided ROAD
        // CLOSED sign facing the traffic that would turn into it; yaw: that traffic's heading) or "noentry" (Street Loop Reverse's own block at
        // Pine Ridge's end: its concrete lip, mesh, and its NO ENTRY sign). pos: x, z, the road's height there.
        [Serializable] public sealed class Block { public string kind, at, mesh; public float[] pos; public float yaw, half, station; public bool both; } // both: closed from each end (a sign facing each way, side by side)
        // 0.117 Part C: a kicker on a shortcut (race only, on the existing ground): an earth wedge rising height m over length m to its lip at pos
        // (x, z), width m wide, facing yaw (the way it is driven)
        [Serializable] public sealed class Kicker { public string at; public float[] pos; public float yaw, width, length, height; }
        // 0.117 Part B item 4: a scene sign's words changed for this race (a TextMesh whose words contain find)
        [Serializable] public sealed class Relabel { public string find, text; }
        // 0.118 Part A2: a scene ground tile (a MeshCollider named tile, on a 1 m vertex grid) given new heights in this race only: verts x, z, y
        [Serializable] public sealed class Reshape { public string tile, at; public float[] verts; }
        [Serializable] public sealed class Data
        {
            public int version; public string id, title, scene; public bool reverse; public float[] pts, geometryStations, bypass, mountain, narrow, spawn; public float geometryLength, spawnYaw, length; public int highwayTraffic;
            public Gate[] gates; public Branch[] branches; public MountainFlights.Flight[] flights;
            public string[] fills; // ground patches added for this course only (Resources/Leviathan): Forward's creek gully under the lake path
            public Sign[] signs; // 0.116 Part A: turn indicators at the course's decision points (Tools/Report116/py/signs.py)
            public float[] clear; // 0.116 Part B: x, z, radius: tree trunks cleared for this race only (on its shortcut lines)
            public Block[] blocks; public Kicker[] kickers; public Relabel[] relabel; // 0.117 (Tools/Report117/py)
            public Reshape[] reshape; // 0.118 Part A2: Kyle's bank (Tools/Report118/py/bank.py)
            public Vector3[] Points => V(pts);
            public bool In(float[] spans, float s) { if (spans == null) return false; for (int i = 0; i + 1 < spans.Length; i += 2) if (s >= spans[i] && s <= spans[i + 1]) return true; return false; }
        }
        static Vector3[] V(float[] f) { var v = new Vector3[(f?.Length ?? 0) / 3]; for (int i = 0; i < v.Length; i++) v[i] = new Vector3(f[3 * i], f[3 * i + 1], f[3 * i + 2]); return v; }
        // 0.116 Part B: Leviathan Reverse's shortcuts Dan switched off (settings.json "leviathanShortcutsOff", the F6 debug menu); set by RaceFlow
        public static string[] ShortcutsOff = Array.Empty<string>();
        public static bool ShortcutOn(string title) => Array.IndexOf(ShortcutsOff ?? Array.Empty<string>(), title) < 0;
        public static Data Load(string key)
        {
            string name = key == Reverse ? "LeviathanReverse" : "LeviathanForward";
            // checks only (Report106PlayerCheck -p117data <dir>: the 0.116 course files, for the before shots); never set in play
            if (!string.IsNullOrEmpty(DataOverride) && System.IO.File.Exists(System.IO.Path.Combine(DataOverride, name + ".json"))) return JsonUtility.FromJson<Data>(System.IO.File.ReadAllText(System.IO.Path.Combine(DataOverride, name + ".json")));
            var a = Resources.Load<TextAsset>("Leviathan/" + name); return a ? JsonUtility.FromJson<Data>(a.text) : null;
        }
        public static string DataOverride; public static bool Before117; // checks only: the 0.116 way (no gate seating, no relabelled signs)

        // RaceDirector.Awake, before it reads its road and gates
        public static void Apply(RaceDirector race)
        {
            if (Active == null || race.gameObject.scene.name != SceneOf(Active) || race.GetComponent<LeviathanSwap>()) return;
            var data = Current = Load(Active); if (data == null) { Debug.LogError("Leviathan: course data missing for " + Active); Active = null; return; }
            var root = new GameObject(data.title + " (course)").transform; SceneManager.MoveGameObjectToScene(root.gameObject, race.gameObject.scene);
            // Leviathan's own ground patches (Dan, 2026-10-14: for this race only, nowhere else): Forward fills the creek gully that crosses the lake
            // path's south shore in MountainLoop.unity (Free Roam has flat ground there), drawn with the scene's own ground material
            foreach (var rs in data.reshape ?? Array.Empty<Reshape>()) ReshapeGround(rs, race.gameObject.scene); // 0.118 Part A2
            var groundMaterial = race.gameObject.scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<MeshRenderer>(true)).FirstOrDefault(r => r.name == "Ground_640_240")?.sharedMaterial;
            foreach (var name in data.fills ?? Array.Empty<string>())
            {
                var mesh = Resources.Load<Mesh>("Leviathan/" + name); if (!mesh) { Debug.LogError("Leviathan: ground patch missing: " + name); continue; }
                var fill = new GameObject("Ground_" + mesh.name); fill.transform.SetParent(root, false);
                fill.AddComponent<MeshFilter>().sharedMesh = mesh; var mr = fill.AddComponent<MeshRenderer>(); if (groundMaterial) mr.sharedMaterial = groundMaterial; fill.AddComponent<MeshCollider>().sharedMesh = mesh;
            }
            Physics.SyncTransforms(); // the patches' colliders, for the gates, blocks and kickers seated on the ground below
            BuildKickers(data, root, groundMaterial); BuildBlocks(data, root); RelabelSigns(data, race.gameObject.scene); // 0.117
            // the line
            var road = new GameObject(data.title + " line").AddComponent<RaceRoad>(); road.transform.SetParent(root, false);
            road.points = data.Points; road.forestTrail = false; road.geometryStations = data.geometryStations; road.geometryLength = data.geometryLength;
            road.bypassStart = data.bypass[0]; road.bypassEnd = data.bypass[1]; road.tracked = true; road.Initialize();
            // the gates: the scene's own Mountain Loop gates where the course passes them, the others built like them
            var own = race.gates.Where(g => g).ToArray(); var gates = new List<RaceGate>(); var marking = own.SelectMany(g => g.GetComponentsInChildren<MeshRenderer>(true)).FirstOrDefault()?.sharedMaterial;
            var oldStart = race.GetComponentsInChildren<RaceGate>(true).FirstOrDefault(g => !g.gameObject.activeSelf && g.name == "START FINISH" && g.transform.childCount > 3);
            foreach (var g in data.gates)
            {
                RaceGate gate = !string.IsNullOrEmpty(g.reuse) ? own.FirstOrDefault(o => Path(o.transform).EndsWith(g.reuse)) : null;
                if (!gate) gate = gates.Count == 0 && oldStart && Mathf.Abs(g.halfWidth - 6) < .01f ? Clone(oldStart, root) : Build(g, marking, root);
                var at = new Vector3(g.pos[0], g.pos[1], g.pos[2]);
                // 0.117 (BUG-003): a gate built here stands on the ground of its spot (its posts reach 1.5 m below its origin, like the Mountain
                // Loop's): the ground nearest its height, the course's own ground patches included (Forward's lake-shore gate stood 1.3 m into the fill)
                if (string.IsNullOrEmpty(g.reuse) && gates.Count > 0 && !Before117 && SeatGround(road, at, g.pos[1], out float gy)) { if (Mathf.Abs(gy + 1.5f - at.y) > .05f) Debug.Log($"Leviathan: {g.name} seated on its ground: {at.y:F2} -> {gy + 1.5f:F2}"); at.y = gy + 1.5f; }
                gate.name = g.name; gate.transform.SetPositionAndRotation(at, Quaternion.Euler(g.rot[0], g.rot[1], g.rot[2]));
                gate.halfWidth = g.halfWidth; gate.halfHeight = g.halfHeight; gate.upperHeight = g.upperHeight; gate.gameObject.SetActive(true); gates.Add(gate);
                if (string.IsNullOrEmpty(g.reuse) && gates.Count > 1) PostsToGround(gate); // 0.118
            }
            foreach (var g in own) if (!gates.Contains(g)) g.gameObject.SetActive(false); // the Mountain Loop's gates not on this course
            // the Street Loop's shortcuts on this course (Forward); the scene's own Mountain shortcuts are not part of it (they stay in the world)
            var branches = new List<WoodlandRoute>();
            foreach (var b in data.branches ?? Array.Empty<Branch>())
            {
                if (data.reverse && !ShortcutOn(b.title)) { Debug.Log("Leviathan: shortcut switched off: " + b.title); continue; } // 0.116 Part B
                var w = new GameObject(b.title).AddComponent<WoodlandRoute>(); w.transform.SetParent(root, false);
                w.title = b.title; w.points = V(b.pts); w.aiPoints = V(b.aiPts); w.entryRoad = b.entryRoad; w.exitRoad = b.exitRoad; w.halfWidth = b.halfWidth; w.recommendedSpeed = b.recommendedSpeed;
                w.bypassedGates = b.bypassedGates; w.aiValidated = b.aiValidated; w.undergroundStart = b.undergroundStart; w.undergroundEnd = b.undergroundEnd; w.entryHeightBelow = b.entryHeightBelow; w.entryHeightAbove = b.entryHeightAbove;
                w.entrySpeed = b.entrySpeed; w.entrySpeedDistance = b.entrySpeedDistance; w.entryInset = b.entryInset; w.entryMargin = b.entryMargin; branches.Add(w);
            }
            // the summit flight at its stations on this line (the scene's other flight is not on the course)
            var flights = race.GetComponent<MountainFlights>(); if (flights) flights.flights = data.flights;
            // the start: the Street Loop's (its gate, grid and spawn)
            var spawn = new GameObject(data.title + " start").transform; spawn.SetParent(root, false); spawn.SetPositionAndRotation(new Vector3(data.spawn[0], data.spawn[1], data.spawn[2]), Quaternion.Euler(0, data.spawnYaw, 0));
            var respawn = race.vehicle ? race.vehicle.GetComponent<VehicleRespawn>() : null; if (respawn) respawn.spawnPoint = spawn;
            if (race.vehicle) { race.vehicle.transform.SetPositionAndRotation(spawn.position, spawn.rotation); if (race.vehicle.Body) { race.vehicle.Body.position = spawn.position; race.vehicle.Body.rotation = spawn.rotation; } }
            BuildSigns(data, root); // 0.116 Part A
            if (data.reverse) HideStrayArrows(race.gameObject.scene, road, branches, root); // 1.3.0-review5 Part D
            race.gameObject.AddComponent<LeviathanGuide>(); // 0.116 Part A: only the next gate shown, the course ahead on the minimap, the next CP on the HUD
            var swap = race.gameObject.AddComponent<LeviathanSwap>(); swap.Data = data; swap.Branches = branches.ToArray(); swap.GateHints = data.gates.Select(g => g.station).ToArray();
            race.road = road; race.gates = gates.ToArray(); race.courseId = data.id; race.courseName = data.title; race.reverseCourse = data.reverse; race.highwayTrafficCount = data.highwayTraffic;
            Debug.Log($"Leviathan: {data.title} in {race.gameObject.scene.name}: line {road.Length:F0} m ({road.points.Length} points), {gates.Count} gates ({gates.Count(g => g.transform.parent != root)} the scene's own), {branches.Count} shortcuts, {data.flights?.Length ?? 0} flight(s)");
        }
        // ---------------------------------------------------------------- 1.3.0-review5 Part D: the scene's route arrows
        // (Dan: "it needs to be easy to understand where to go"): Mountain Loop Reverse's painted teal and gold arrows stayed in Leviathan Reverse
        // and pointed Mountain Loop's way where Leviathan goes another (up the mountain again from the lake, into its shortcuts). In this race an
        // arrow is kept only where it lies on Leviathan's line (or one of its shortcuts) and points along it, on a stretch the course drives once;
        // where the course drives the same road twice (the lake out-and-back and its ends) an arrow is right for one pass and wrong for the
        // other, so it goes too (the gates and the HUD guide lead there). Race only: the scene is loaded fresh for every other race.
        public static readonly List<string> ArrowReport = new();
        static void HideStrayArrows(Scene scene, RaceRoad road, List<WoodlandRoute> branches, Transform root)
        {
            ArrowReport.Clear(); int kept = 0, hidden = 0;
            var samples = new List<(Vector3 p, Vector3 f, float s, int line)>();
            for (float s = 0; s < road.Length; s += 3) { var p = road.At(s, out var f); samples.Add((p, Vector3.ProjectOnPlane(f, Vector3.up).normalized, s, 0)); }
            for (int b = 0; b < branches.Count; b++) for (float s = 0; s < branches[b].Length; s += 3) { var p = branches[b].At(s, out var f); samples.Add((p, Vector3.ProjectOnPlane(f, Vector3.up).normalized, s, b + 1)); }
            // where the course meets another pass of itself (the out-and-back's shared road and its two ends)
            var meets = new List<Vector3>(); var main = samples.Where(q => q.line == 0).ToList();
            for (int i = 0; i < main.Count; i += 2) for (int j = i + 1; j < main.Count; j += 2) { float ds = main[j].s - main[i].s; if (ds > 150 && ds < road.Length - 150 && (main[i].p - main[j].p).sqrMagnitude < 15 * 15) { meets.Add(main[i].p); break; } }
            foreach (var g in scene.GetRootGameObjects())
                foreach (var r in g.GetComponentsInChildren<MeshRenderer>(true))
                {
                    if (!r.enabled || r.transform.IsChildOf(root) || !r.name.EndsWith("arrow", StringComparison.OrdinalIgnoreCase)) continue;
                    var mf = r.GetComponent<MeshFilter>(); var mesh = mf ? mf.sharedMesh : null; if (!mesh || !mesh.isReadable || mesh.vertexCount < 3) continue;
                    var v = mesh.vertices; var c = Vector3.zero; foreach (var x in v) c += x; c /= v.Length;
                    var tip = v.OrderByDescending(x => (x - c).sqrMagnitude).First();
                    var at = r.transform.TransformPoint(c); var dir = Vector3.ProjectOnPlane(r.transform.TransformPoint(tip) - at, Vector3.up).normalized;
                    bool along = false, twice = false; float alongS = -1;
                    foreach (var q in samples)
                    {
                        var d = at - q.p; if (Mathf.Abs(d.y) > 4 || new Vector2(d.x, d.z).sqrMagnitude > 7 * 7) continue;
                        if (Vector3.Dot(dir, q.f) > .8f) { along = true; if (q.line == 0) alongS = q.s; }
                    }
                    foreach (var m in meets) { var d = at - m; if (Mathf.Abs(d.y) < 6 && new Vector2(d.x, d.z).sqrMagnitude < 60 * 60) { twice = true; break; } }
                    if (along && !twice) { kept++; continue; }
                    r.enabled = false; hidden++; ArrowReport.Add($"{r.name} at ({at.x:F0}, {at.z:F0}): {(twice ? "where the course passes twice" : "not along the course")}");
                }
            Debug.Log($"Leviathan: the scene's route arrows: {kept} kept, {hidden} hidden ({string.Join("; ", ArrowReport)})");
        }
        // ---------------------------------------------------------------- 0.116 Part A: turn indicators
        // Boards like a road's: a big arrow before the turn and chevrons on its outside, facing the traffic that turns there. Plain grey
        // backs (one-sided: the other pass of a twice-driven road sees the back), standard depth-tested materials, posts seated on the
        // ground, no colliders. Built only in a Leviathan race.
        static Material signBlack, signYellow, signGrey, lettering; static Font letteringFont;
        static Material Unlit(Color c) { var m = new Material(Shader.Find("Universal Render Pipeline/Unlit")); m.SetColor("_BaseColor", c); m.color = c; return m; }
        static void BuildSigns(Data data, Transform root)
        {
            if (data.signs == null || data.signs.Length == 0 || LeviathanGuide.Disabled) return;
            if (!signBlack) signBlack = Unlit(new Color(.05f, .05f, .06f)); if (!signYellow) signYellow = Unlit(new Color(1f, .78f, .08f));
            if (!signGrey) signGrey = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = new Color(.42f, .43f, .44f) };
            var holder = new GameObject("Leviathan turn signs (0.116)").transform; holder.SetParent(root, false);
            int built = 0;
            foreach (var sg in data.signs)
            {
                var rot = Quaternion.Euler(0, sg.yaw, 0); // +z along the traffic: the face (-z) looks back at it
                bool arrow = sg.kind == "arrow", board = sg.kind == "board"; float w = arrow ? 3.2f : board ? 3.6f : 1.0f, h = arrow ? 1.6f : board ? 1.2f : 1.25f, lift = arrow ? 1.3f : board ? 1.4f : 1.0f;
                var right = rot * Vector3.right; float y0 = float.MaxValue, yGround = float.MinValue; Vector3 at = default; bool any = false;
                // on this side of the road, else (a bank, the lake) on the other: ground under both posts near the road's height there
                var spots = new List<float[]> { sg.pos }; for (int k = 0; sg.alt != null && k + 2 < sg.alt.Length; k += 3) spots.Add(new[] { sg.alt[k], sg.alt[k + 1], sg.alt[k + 2] });
                foreach (var spot in spots)
                {
                    at = new Vector3(spot[0], 0, spot[1]); y0 = float.MaxValue; yGround = float.MinValue; int hits = 0;
                    foreach (float side in new[] { -w * .38f, 0, w * .38f })
                        if (Ground(at + right * side, spot.Length > 2 ? spot[2] : float.NaN, out float gy)) { y0 = Mathf.Min(y0, gy); yGround = Mathf.Max(yGround, gy); hits++; }
                    if (hits == 3 && yGround - y0 < 2.5f) { any = true; break; }
                }
                if (!any) { Debug.LogWarning($"Leviathan: no ground under the turn sign at {at} ({sg.at})"); continue; }
                var sign = new GameObject(board ? $"Leviathan shortcut board '{sg.text}' ({sg.at})" : $"Leviathan {(arrow ? "turn arrow board" : "chevron board")} {(sg.dir < 0 ? "left" : "right")} ({sg.at})").transform;
                sign.SetParent(holder, false); sign.SetPositionAndRotation(new Vector3(at.x, yGround, at.z), rot); sign.gameObject.AddComponent<PhysicalSign>();
                float drop = yGround - y0 + .5f; // the posts reach into the lower ground
                foreach (float side in arrow || board ? new[] { -w * .38f, w * .38f } : new[] { 0f })
                    Box(sign, "Post", new Vector3(side, (lift - drop) * .5f, .06f), new Vector3(.12f, lift + drop, .12f), signGrey);
                Box(sign, "Board", new Vector3(0, lift + h * .5f, .06f), new Vector3(w, h, .08f), signGrey); // the back and edges: plain grey
                var v = new List<Vector3>(); var black = new List<int>(); var yellow = new List<int>();
                void Tri(List<int> into, Vector3 a, Vector3 b, Vector3 c)
                {
                    int i = v.Count; v.Add(a); v.Add(b); v.Add(c);
                    // every triangle faces -z (the traffic)
                    if (Vector3.Cross(b - a, c - a).z > 0) into.AddRange(new[] { i, i + 2, i + 1 }); else into.AddRange(new[] { i, i + 1, i + 2 });
                }
                void Quad(List<int> into, Vector3 a, Vector3 b, Vector3 c, Vector3 d) { Tri(into, a, b, c); Tri(into, a, c, d); }
                float bx = w * .5f - .04f, by0 = lift + .04f, by1 = lift + h - .04f, f = -.01f, g = -.02f;
                Quad(black, new Vector3(-bx, by0, f), new Vector3(-bx, by1, f), new Vector3(bx, by1, f), new Vector3(bx, by0, f));
                float dd = sg.dir < 0 ? -1 : 1, cy = lift + h * .5f;
                if (board)
                {
                    // a shortcut's name (its entry) or REJOIN ROAD (its end): lettering fitted inside the board, on its face only
                    if (!letteringFont) letteringFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    if (!lettering) { lettering = Resources.Load<Material>("Scenery/SignLettering"); if (lettering && letteringFont && letteringFont.material && letteringFont.material.mainTexture) lettering.mainTexture = letteringFont.material.mainTexture; }
                    if (lettering && letteringFont)
                    {
                        var tm = new GameObject("Shortcut lettering").AddComponent<TextMesh>(); tm.transform.SetParent(sign, false); tm.transform.localPosition = new Vector3(0, lift + h * .5f, .06f - .04f - .03f);
                        tm.font = letteringFont; tm.text = sg.text; tm.anchor = TextAnchor.MiddleCenter; tm.alignment = TextAlignment.Center; tm.fontSize = 96; tm.characterSize = .1f; tm.color = new Color(1f, .78f, .08f);
                        var tr = tm.GetComponent<MeshRenderer>(); tr.sharedMaterial = lettering; tr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                        var fit = tm.gameObject.AddComponent<LeviathanLettering>(); fit.width = w - .3f; fit.height = h - .3f; fit.Fit();
                    }
                }
                else if (arrow)
                {
                    // a shaft and a head pointing the way to turn
                    float sx0 = Mathf.Min(-1.15f * dd, .35f * dd), sx1 = Mathf.Max(-1.15f * dd, .35f * dd);
                    Quad(yellow, new Vector3(sx0, cy - .2f, g), new Vector3(sx0, cy + .2f, g), new Vector3(sx1, cy + .2f, g), new Vector3(sx1, cy - .2f, g));
                    Tri(yellow, new Vector3(.25f * dd, cy + .62f, g), new Vector3(1.35f * dd, cy, g), new Vector3(.25f * dd, cy - .62f, g));
                }
                else
                {
                    // a chevron: two strokes meeting at the point (the way to turn)
                    float px = .28f * dd, back = -.22f * dd, th = .17f;
                    void Stroke(Vector3 a, Vector3 b) { var dir = (b - a).normalized; var n = new Vector3(-dir.y, dir.x, 0) * th; Quad(yellow, a - n, a + n, b + n, b - n); }
                    Stroke(new Vector3(back, cy + .42f, g), new Vector3(px, cy, g)); Stroke(new Vector3(px, cy, g), new Vector3(back, cy - .42f, g));
                }
                var face = new Mesh { name = "Sign face" }; face.SetVertices(v); face.subMeshCount = 2; face.SetTriangles(black, 0); face.SetTriangles(yellow, 1); face.RecalculateNormals(); face.RecalculateBounds();
                var fo = new GameObject("Sign face (one-sided)"); fo.transform.SetParent(sign, false); fo.transform.localPosition = new Vector3(0, 0, .06f - .04f);
                fo.AddComponent<MeshFilter>().sharedMesh = face; var fr = fo.AddComponent<MeshRenderer>(); fr.sharedMaterials = new[] { signBlack, signYellow }; fr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                built++;
            }
            Debug.Log($"Leviathan: {built} sign(s) built ({data.signs.Count(x => x.kind == "arrow")} arrows, {data.signs.Count(x => x.kind == "chevron")} chevrons, {data.signs.Count(x => x.kind == "board")} shortcut boards)");
        }
        // ---------------------------------------------------------------- 0.117 Part A: closed ways
        // A barricade like the Mountain Loop's own (Climbing Ridge Cut's "Road closed barrier 4"): timber posts and two rails, solid, in panels
        // of up to 3 m each seated on its own ground (a panel over a bank or a hole is left out), and a one-sided ROAD CLOSED sign in the middle
        // facing the traffic that would turn in (red face with lettering, plain grey back). Built only in a Leviathan race.
        static Material timber, closedRed, concrete, amber;
        static Material Lit(Color c) { var m = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = c }; m.SetColor("_BaseColor", c); return m; }
        internal static void BuildBlocks(Data data, Transform root) // 1.3.0-review3: also the Mountain Loop Stunt Races' closed shortcuts (StuntShortcuts)
        {
            if (data.blocks == null || data.blocks.Length == 0) return;
            if (!timber) timber = Lit(new Color(.42f, .29f, .17f)); if (!closedRed) closedRed = Unlit(new Color(.55f, .08f, .06f)); if (!signGrey) signGrey = Lit(new Color(.42f, .43f, .44f));
            var holder = new GameObject("Leviathan closed ways (0.117)").transform; holder.SetParent(root, false); int built = 0, panels = 0;
            foreach (var b in data.blocks)
            {
                var rot = Quaternion.Euler(0, b.yaw, 0); var right = rot * Vector3.right; var at = new Vector3(b.pos[0], 0, b.pos[1]);
                if (b.kind == "noentry") { NoEntry(b, holder); built++; continue; }
                if (!Ground(at, b.pos[2], out float y0)) { Debug.LogWarning($"Leviathan: no ground under the block at {at} ({b.at})"); continue; }
                var block = new GameObject($"Leviathan road closed ({b.at})").transform; block.SetParent(holder, false); block.SetPositionAndRotation(new Vector3(at.x, y0, at.z), rot);
                int n = Mathf.Max(1, Mathf.CeilToInt(2 * b.half / 3f)); float w = 2 * b.half / n;
                for (int k = 0; k < n; k++)
                {
                    float c = -b.half + w * (k + .5f); var p = at + right * c;
                    if (!Ground(p, y0, out float py) || Mathf.Abs(py - y0) > 2.5f) continue;
                    var panel = new GameObject("Barricade panel").transform; panel.SetParent(block, false); panel.position = new Vector3(p.x, py, p.z); panel.rotation = rot;
                    Box(panel, "Post", new Vector3(-w * .5f + .15f, .45f, 0), new Vector3(.25f, 1.6f, .25f), timber); Box(panel, "Post", new Vector3(w * .5f - .15f, .45f, 0), new Vector3(.25f, 1.6f, .25f), timber);
                    Box(panel, "Rail low", new Vector3(0, .5f, 0), new Vector3(w, .2f, .1f), timber); Box(panel, "Rail high", new Vector3(0, 1.0f, 0), new Vector3(w, .2f, .1f), timber);
                    var hit = new GameObject("Barricade collision"); hit.transform.SetParent(panel, false); hit.transform.localPosition = new Vector3(0, .6f, 0);
                    hit.AddComponent<BoxCollider>().size = new Vector3(w, 2.0f, .35f); panels++;
                }
                // the sign: one-sided, its face (-z) toward the traffic, in front of the middle panel; closed from both ends: one each way, side by side
                float apart = b.both ? Mathf.Min(1.4f, b.half * .5f) : 0;
                foreach (int way in b.both ? new[] { 1, -1 } : new[] { 1 })
                {
                    var sign = new GameObject("Road closed sign").transform; sign.SetParent(block, false); sign.localPosition = new Vector3(-apart * way, 0, -.35f * way); sign.localRotation = Quaternion.Euler(0, way > 0 ? 0 : 180, 0); sign.gameObject.AddComponent<PhysicalSign>();
                    Box(sign, "Sign post", new Vector3(-.8f, 1.1f, .05f), new Vector3(.1f, 2.2f, .1f), timber); Box(sign, "Sign post", new Vector3(.8f, 1.1f, .05f), new Vector3(.1f, 2.2f, .1f), timber);
                    Box(sign, "Sign back", new Vector3(0, 2.0f, .06f), new Vector3(1.9f, 1.2f, .06f), signGrey);
                    Face(sign, new Vector3(0, 2.0f, .02f), 1.86f, 1.16f, closedRed); Lettering(sign, "ROAD\nCLOSED", new Vector3(0, 2.0f, 0f), 1.6f, .95f, Color.white);
                }
                built++; Debug.Log($"Leviathan: road closed at {block.position} facing {b.yaw:F0}: {block.GetComponentsInChildren<BoxCollider>().Length} panels{(b.both ? ", signs both ways" : "")} ({b.at})");
            }
            Debug.Log($"Leviathan: {built} closed way(s) built ({panels} barricade panels)");
        }
        // Street Loop Reverse's block at Pine Ridge's end, as it is there: its concrete exit lip (the same mesh) and its NO ENTRY board
        static void NoEntry(Block b, Transform holder)
        {
            if (!concrete) concrete = Lit(new Color(.48f, .44f, .34f)); if (!amber) amber = Lit(new Color(.32f, .23f, .15f));
            var mesh = string.IsNullOrEmpty(b.mesh) ? null : Resources.Load<Mesh>("Leviathan/" + b.mesh);
            if (mesh) { var lip = new GameObject("Ground_" + b.mesh); lip.transform.SetParent(holder, false); lip.AddComponent<MeshFilter>().sharedMesh = mesh; lip.AddComponent<MeshRenderer>().sharedMaterial = concrete; lip.AddComponent<MeshCollider>().sharedMesh = mesh; }
            var sign = new GameObject($"NO ENTRY ({b.at})").transform; sign.SetParent(holder, false); sign.SetPositionAndRotation(new Vector3(b.pos[0], b.pos[2], b.pos[1]), Quaternion.Euler(0, b.yaw, 0)); sign.localScale = Vector3.one * .7f; sign.gameObject.AddComponent<PhysicalSign>();
            Box(sign, "Board", new Vector3(0, 2.5f, .02f), new Vector3(6f, 2f, .16f), amber); Box(sign, "Post", new Vector3(-2f, 1.3f, .1f), new Vector3(.2f, 2.6f, .2f), amber); Box(sign, "Post", new Vector3(2f, 1.3f, .1f), new Vector3(.2f, 2.6f, .2f), amber);
            Lettering(sign, "NO ENTRY", new Vector3(0, 2.5f, -.08f), 5.4f, 1.5f, new Color(1f, .86f, .3f));
        }
        static void Face(Transform parent, Vector3 at, float w, float h, Material m)
        {
            var mesh = new Mesh { name = "Sign face" }; float x = w * .5f, y = h * .5f;
            mesh.vertices = new[] { new Vector3(-x, -y, 0), new Vector3(-x, y, 0), new Vector3(x, y, 0), new Vector3(x, -y, 0) }; mesh.triangles = new[] { 0, 1, 2, 0, 2, 3 }; mesh.RecalculateNormals(); mesh.RecalculateBounds(); // faces -z only
            var f = new GameObject("Sign face (one-sided)"); f.transform.SetParent(parent, false); f.transform.localPosition = at;
            f.AddComponent<MeshFilter>().sharedMesh = mesh; var r = f.AddComponent<MeshRenderer>(); r.sharedMaterial = m; r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        // lettering on a sign's face only (depth-tested, back-face culled: the signs rule), fitted inside width x height
        static void Lettering(Transform parent, string text, Vector3 at, float width, float height, Color colour)
        {
            if (!letteringFont) letteringFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (!lettering) { lettering = Resources.Load<Material>("Scenery/SignLettering"); if (lettering && letteringFont && letteringFont.material && letteringFont.material.mainTexture) lettering.mainTexture = letteringFont.material.mainTexture; }
            if (!lettering || !letteringFont) return;
            var tm = new GameObject("Sign lettering").AddComponent<TextMesh>(); tm.transform.SetParent(parent, false); tm.transform.localPosition = at - new Vector3(0, 0, .02f);
            tm.font = letteringFont; tm.text = text; tm.anchor = TextAnchor.MiddleCenter; tm.alignment = TextAlignment.Center; tm.fontSize = 96; tm.characterSize = .1f; tm.color = colour;
            var tr = tm.GetComponent<MeshRenderer>(); tr.sharedMaterial = lettering; tr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            var fit = tm.gameObject.AddComponent<LeviathanLettering>(); fit.width = width; fit.height = height; fit.Fit();
        }
        // ---------------------------------------------------------------- 0.118 Part A2: Kyle's bank, one even slope (Dan: "smooth out Kyle's hill")
        // The tile's mesh is copied and its vertices at the listed grid points set to their new heights (drawn and collided: the same copy); the
        // normals are recomputed around them only. The tree trunks standing on the changed ground move with it (the new scenery kit builds each
        // tree on its trunk afterwards, so the trees stand on the new ground). The scene's own mesh is not changed: Free Roam and the other
        // courses keep their ground.
        static void ReshapeGround(Reshape rs, Scene scene)
        {
            if (rs.verts == null || rs.verts.Length < 3) return;
            var heights = new Dictionary<(int, int), float>(); for (int i = 0; i + 2 < rs.verts.Length; i += 3) heights[(Mathf.RoundToInt(rs.verts[i]), Mathf.RoundToInt(rs.verts[i + 1]))] = rs.verts[i + 2];
            var roots = scene.GetRootGameObjects(); var moved = new Dictionary<(int, int), float>();
            foreach (var mc in roots.SelectMany(g => g.GetComponentsInChildren<MeshCollider>(true)).Where(c => c.name == rs.tile))
            {
                var src = mc.sharedMesh; if (!src || !src.isReadable) { Debug.LogWarning($"Leviathan: ground tile {rs.tile} not readable: {rs.at} left as it is"); continue; }
                var t = mc.transform; var v = src.vertices; var hit = new bool[v.Length]; int n = 0;
                for (int i = 0; i < v.Length; i++)
                {
                    var w = t.TransformPoint(v[i]); var key = (Mathf.RoundToInt(w.x), Mathf.RoundToInt(w.z));
                    if (Mathf.Abs(w.x - key.Item1) > .01f || Mathf.Abs(w.z - key.Item2) > .01f || !heights.TryGetValue(key, out float y)) continue;
                    moved[key] = y - w.y; w.y = y; v[i] = t.InverseTransformPoint(w); hit[i] = true; n++;
                }
                if (n == 0) continue;
                var mesh = UnityEngine.Object.Instantiate(src); mesh.name = src.name + " (Leviathan: " + rs.at + ")"; var before = src.normals;
                mesh.vertices = v; mesh.RecalculateNormals(); mesh.RecalculateBounds();
                // the normals recomputed only on the changed ground and the ring round it (the rest of the tile is shaded as before)
                var after = mesh.normals; var ring = new HashSet<(int, int)>(); foreach (var k in moved.Keys) for (int dx = -1; dx <= 1; dx++) for (int dz = -1; dz <= 1; dz++) ring.Add((k.Item1 + dx, k.Item2 + dz));
                if (before.Length == after.Length) for (int i = 0; i < v.Length; i++) { var w = t.TransformPoint(v[i]); if (!ring.Contains((Mathf.RoundToInt(w.x), Mathf.RoundToInt(w.z)))) after[i] = before[i]; }
                mesh.normals = after;
                var mf = mc.GetComponent<MeshFilter>(); if (mf && mf.sharedMesh == src) mf.sharedMesh = mesh; mc.sharedMesh = mesh;
                Debug.Log($"Leviathan: {rs.at}: {n} vertices of {t.name} set (raised up to {moved.Values.Max():F2} m, lowered up to {-moved.Values.Min():F2} m)");
            }
            if (moved.Count == 0) return;
            float Dy(Vector3 p)
            {
                int x0 = Mathf.FloorToInt(p.x), z0 = Mathf.FloorToInt(p.z); float fx = p.x - x0, fz = p.z - z0;
                float D(int x, int z) => moved.TryGetValue((x, z), out float d) ? d : 0;
                return Mathf.Lerp(Mathf.Lerp(D(x0, z0), D(x0 + 1, z0), fx), Mathf.Lerp(D(x0, z0 + 1), D(x0 + 1, z0 + 1), fx), fz);
            }
            foreach (var box in roots.SelectMany(g => g.GetComponentsInChildren<BoxCollider>(true)).Where(b => b.name.IndexOf("trunk", StringComparison.OrdinalIgnoreCase) >= 0))
            {
                var tr = box.transform; var bottom = tr.TransformPoint(box.center - Vector3.up * box.size.y * .5f); float dy = Dy(bottom);
                if (Mathf.Abs(dy) < .01f) continue;
                tr.position += Vector3.up * dy; Debug.Log($"Leviathan: {rs.at}: tree trunk at {bottom.x:F1},{bottom.z:F1} set on the new ground ({dy:+0.00;-0.00} m)");
            }
            Physics.SyncTransforms();
        }
        // ---------------------------------------------------------------- 0.117 Part C: kickers on the shortcuts
        // An earth wedge on the existing ground (drawn with the scene's ground material, solid), rising to its lip; the ground beyond is unchanged.
        static void BuildKickers(Data data, Transform root, Material ground)
        {
            if (data.kickers == null) return;
            foreach (var k in data.kickers) BuildKicker(k, root, ground, "Leviathan");
            Physics.SyncTransforms();
        }
        // 0.120 Part C: also the Stunt Races' bumps (StuntBumps). The lip's own ground (at height hint pos[2] when given); null if there is none.
        // 0.120: colour (with a vertex-colour material such as Scenery/Building): the top that colour, the sides a shade darker
        public static GameObject BuildKicker(Kicker k, Transform root, Material ground, string owner, Color? colour = null)
        {
            var rot = Quaternion.Euler(0, k.yaw, 0); var f = rot * Vector3.forward; var r = rot * Vector3.right; var lip = new Vector3(k.pos[0], 0, k.pos[1]); var start = lip - f * k.length;
            float hint = k.pos.Length > 2 ? k.pos[2] : float.NaN;
            if (!Ground(lip, hint, out float yl) || !Ground(start, hint, out float ys)) { Debug.LogWarning($"{owner}: no ground under the kicker {k.at}"); return null; }
            float hw = k.width * .5f, top = yl + k.height, bottom = Mathf.Min(yl, ys) - .6f;
            Vector3 V(Vector3 p, float side, float y) { var q = p + r * side; return new Vector3(q.x, y, q.z); }
            var v = new List<Vector3> {
                V(start, -hw, ys - .05f), V(start, hw, ys - .05f), V(lip, hw, top), V(lip, -hw, top), // the ramp
                V(lip, -hw, top), V(lip, hw, top), V(lip, hw, bottom), V(lip, -hw, bottom),          // the lip's face
                V(start, -hw, ys - .05f), V(lip, -hw, top), V(lip, -hw, bottom), V(start, -hw, bottom), // left side
                V(start, hw, ys - .05f), V(start, hw, bottom), V(lip, hw, bottom), V(lip, hw, top) };  // right side
            var tris = new List<int>(); for (int q = 0; q < 4; q++) { int i = q * 4; tris.AddRange(new[] { i, i + 3, i + 2, i, i + 2, i + 1 }); }
            var mesh = new Mesh { name = owner + " kicker " + k.at }; mesh.SetVertices(v); mesh.SetTriangles(tris, 0); mesh.RecalculateNormals(); mesh.RecalculateBounds();
            // every face outward (the winding above is checked against the face's centre; flipped where it points in)
            var t = mesh.triangles; var c = mesh.bounds.center; var vv = mesh.vertices;
            for (int i = 0; i < t.Length; i += 3) { var n = Vector3.Cross(vv[t[i + 1]] - vv[t[i]], vv[t[i + 2]] - vv[t[i]]); var m = (vv[t[i]] + vv[t[i + 1]] + vv[t[i + 2]]) / 3; if (Vector3.Dot(n, m - c) < 0) { (t[i + 1], t[i + 2]) = (t[i + 2], t[i + 1]); } }
            mesh.triangles = t; mesh.RecalculateNormals();
            if (colour is Color paint) mesh.SetColors(Enumerable.Range(0, v.Count).Select(i => i < 4 ? paint : new Color(paint.r * .82f, paint.g * .82f, paint.b * .82f, 1)).ToList());
            var go = new GameObject("Ground_" + owner + " kicker (" + k.at + ")"); go.transform.SetParent(root, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh; var mr = go.AddComponent<MeshRenderer>(); if (ground) mr.sharedMaterial = ground; go.AddComponent<MeshCollider>().sharedMesh = mesh;
            Debug.Log($"{owner}: kicker {k.at}: lip {top - yl:F2} m above its ground at ({lip.x:F1}, {lip.z:F1}), {k.length:F1} m long, {k.width:F1} m wide");
            return go;
        }
        // 0.117 Part B item 4: Climbing Ridge Cut (Forward) / Downhill Ridge Cut (Reverse) are Leviathan's main road: their scene signs lose
        // "optional shortcut" / "follow gold" in this race only
        static void RelabelSigns(Data data, Scene scene)
        {
            if (data.relabel == null || Before117) return;
            foreach (var tm in scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<TextMesh>(true)))
                foreach (var r in data.relabel)
                    if (tm.text != null && tm.text.IndexOf(r.find, StringComparison.OrdinalIgnoreCase) >= 0) { Debug.Log($"Leviathan: sign '{tm.text.Replace('\n', '/')}' reads '{r.text.Replace('\n', '/')}' in this race"); tm.text = r.text; var fit = tm.GetComponent<LeviathanLettering>(); if (fit) fit.Fit(); }
        }
        static void Box(Transform parent, string name, Vector3 at, Vector3 size, Material m)
        {
            var g = GameObject.CreatePrimitive(PrimitiveType.Cube); g.name = name; UnityEngine.Object.Destroy(g.GetComponent<Collider>());
            g.transform.SetParent(parent, false); g.transform.localPosition = at; g.transform.localScale = size; g.GetComponent<Renderer>().sharedMaterial = m;
        }
        // 0.117: the top ground surface under a point between heights lo and hi (a gate on the course's own ground patch stands on the patch)
        static bool GroundTop(Vector3 p, float lo, float hi, out float y)
        {
            y = float.MinValue;
            foreach (var h in Physics.RaycastAll(new Vector3(p.x, hi + 1, p.z), Vector3.down, hi - lo + 1, ~0, QueryTriggerInteraction.Ignore))
                if (!h.collider.attachedRigidbody && !h.collider.isTrigger && h.normal.y >= .5f && h.collider.name.IndexOf("trunk", StringComparison.OrdinalIgnoreCase) < 0) y = Mathf.Max(y, h.point.y);
            return y > float.MinValue;
        }
        // the ground under a point (not trunks, vehicles or triggers) nearest the road's height there (yRef; NaN: the top surface), within 8 m
        static bool Ground(Vector3 p, float yRef, out float y)
        {
            y = 0; float best = float.MaxValue; bool found = false;
            foreach (var h in Physics.RaycastAll(new Vector3(p.x, 600, p.z), Vector3.down, 1200, ~0, QueryTriggerInteraction.Ignore))
            {
                if (h.collider.attachedRigidbody || h.collider.isTrigger || h.normal.y < .5f || h.collider.name.IndexOf("trunk", StringComparison.OrdinalIgnoreCase) >= 0) continue;
                float score = float.IsNaN(yRef) ? h.distance : Mathf.Abs(h.point.y - yRef); if (!float.IsNaN(yRef) && score > 8) continue;
                if (score < best) { best = score; y = h.point.y; found = true; }
            }
            return found;
        }

        // 0.118 (0.117's seating took the top surface within 3 m above the gate: on the Street Loop's west road something stands 2.3 m over the
        // road, and Reverse CP 8 was seated on it, too high for the cars under it to cross): the ground nearest the course line's own height
        // there (the line runs on Forward's gully fill, so its lake-shore gate still stands on the fill)
        static bool SeatGround(RaceRoad road, Vector3 at, float y, out float gy)
        {
            float lineY = road ? road.At(road.Project(at, out _), out _).y : y - 1.5f;
            return Ground(new Vector3(at.x, 0, at.z), lineY, out gy) || GroundTop(at, y - 6, y + 3, out gy);
        }
        // 0.118: a built gate's posts reach down to the ground under each (on a cross slope the ground beside the road is lower than at its
        // middle, where the gate is seated: Kyle's driveway gate's posts stood up to 0.85 m above it)
        static void PostsToGround(RaceGate gate)
        {
            foreach (Transform post in gate.transform)
            {
                if (post.name != "Non-colliding gate marking" || Mathf.Abs(post.localScale.y - 4) > .01f) continue;
                var foot = post.TransformPoint(Vector3.down * .5f); // the post's foot (a unit cube scaled to its height)
                if (!GroundTop(foot, foot.y - 4, foot.y + .5f, out float gy) || foot.y - gy < .05f) continue;
                float gap = foot.y - gy; post.localScale += new Vector3(0, gap, 0); post.localPosition -= new Vector3(0, gap * .5f, 0);
            }
        }
        static string Path(Transform t) { var s = t.name; while (t.parent) { t = t.parent; s = t.name + "/" + s; } return s; }
        static RaceGate Clone(RaceGate template, Transform parent) { var g = UnityEngine.Object.Instantiate(template, parent); return g; }
        // a gate like the Mountain Loop's: two posts and a bar, no collider (sized to the opening)
        static RaceGate Build(Gate g, Material marking, Transform parent)
        {
            var gate = new GameObject(g.name).AddComponent<RaceGate>(); gate.transform.SetParent(parent, false);
            void Part(Vector3 at, Vector3 scale)
            {
                var p = GameObject.CreatePrimitive(PrimitiveType.Cube); p.name = "Non-colliding gate marking"; UnityEngine.Object.Destroy(p.GetComponent<Collider>());
                p.transform.SetParent(gate.transform, false); p.transform.localPosition = at; p.transform.localScale = scale; if (marking) p.GetComponent<Renderer>().sharedMaterial = marking;
            }
            float x = g.halfWidth + .125f; Part(new Vector3(-x, .5f, 0), new Vector3(.25f, 4, .25f)); Part(new Vector3(x, .5f, 0), new Vector3(.25f, 4, .25f)); Part(new Vector3(0, 2.5f, 0), new Vector3(2 * g.halfWidth + .5f, .25f, .25f));
            return gate;
        }

        // Scenery (before the new kit): tree trunks standing on Leviathan's lake trails, as Free Roam clears them from its drivable trails (its
        // stem and shore paths are those trails; the Mountain scenes kept their trunks, nothing raced there). Only on the line, only in a Leviathan race.
        public static void ClearTrunks(Scene scene)
        {
            var data = Current; if (data == null || Active == null || scene.name != SceneOf(Active)) return;
            var line = data.Points; var s = new float[line.Length]; for (int i = 1; i < line.Length; i++) s[i] = s[i - 1] + Vector3.Distance(line[i - 1], line[i]);
            int cleared = 0; var names = new List<string>();
            foreach (var box in scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<BoxCollider>(true)))
            {
                if (!box || !box.enabled || box.name.IndexOf("trunk", StringComparison.OrdinalIgnoreCase) < 0) continue;
                var t = box.transform; var bottom = t.TransformPoint(box.center - Vector3.up * box.size.y * .5f);
                int k = -1; float best = float.MaxValue;
                for (int i = 0; i + 1 < line.Length; i++)
                {
                    var a = line[i]; var b = line[i + 1]; var ab = new Vector2(b.x - a.x, b.z - a.z); float tt = Mathf.Clamp01(Vector2.Dot(new Vector2(bottom.x - a.x, bottom.z - a.z), ab) / Mathf.Max(1e-4f, ab.sqrMagnitude));
                    var c = Vector3.Lerp(a, b, tt); float d = new Vector2(bottom.x - c.x, bottom.z - c.z).magnitude; if (d < best && Mathf.Abs(bottom.y - c.y) < 2.5f) { best = d; k = i; }
                }
                // a car's width either side of its lane; on the stem and at J, where rivals swing wide out of the hairpin, a little more
                bool lake = k >= 0 && best <= (data.In(data.narrow, s[k]) ? 4.5f : 3.5f) && !data.In(data.mountain, s[k]) && OnLake(data, s[k]);
                // 0.116 Part B: and on Leviathan Reverse's own shortcut lines (the ones switched on), 3.5 m either side
                string onShortcut = lake ? null : ShortcutAt(data, bottom);
                if (!lake && onShortcut == null) continue;
                var size = Vector3.Scale(box.size, t.lossyScale);
                SceneryTrees.Cleared.Add((bottom, Mathf.Min(Mathf.Abs(size.x), Mathf.Abs(size.z)), Mathf.Abs(size.y), data.title + (onShortcut != null ? " shortcut " + onShortcut + " (0.116)" : " lake trail (0.115)"), t.name));
                box.enabled = false; if (t.GetComponents<Component>().Length == 2 && t.childCount == 0) t.gameObject.SetActive(false); cleared++; names.Add(onShortcut != null ? $"{t.name} at {bottom.x:F1},{bottom.z:F1} on {onShortcut}" : $"{t.name} at {bottom.x:F1},{bottom.z:F1} ({best:F1} m from the line, s {s[k]:F0})");
            }
            Debug.Log($"Leviathan: {cleared} tree trunk(s) standing on the lake trails cleared in {scene.name}" + (names.Count > 0 ? ":\n" + string.Join("\n", names) : ""));
        }
        // 0.116 Part B: the Leviathan Reverse shortcut (switched on) whose line passes within 3.5 m of a point (same ground level), or null
        static string ShortcutAt(Data d, Vector3 p)
        {
            if (!d.reverse || d.branches == null) return null;
            foreach (var b in d.branches)
            {
                if (!ShortcutOn(b.title)) continue; var q = V(b.pts);
                for (int i = 0; i + 1 < q.Length; i++)
                {
                    var a = q[i]; var ab = new Vector2(q[i + 1].x - a.x, q[i + 1].z - a.z); float tt = Mathf.Clamp01(Vector2.Dot(new Vector2(p.x - a.x, p.z - a.z), ab) / Mathf.Max(1e-4f, ab.sqrMagnitude));
                    var c = Vector3.Lerp(a, q[i + 1], tt); if (new Vector2(p.x - c.x, p.z - c.z).magnitude < 3.5f && Mathf.Abs(p.y - c.y) < 2.5f) return b.title;
                }
            }
            return null;
        }
        // the parts of the line on the Lake shore connection, its stem and the strip (from the composer's part names)
        static bool OnLake(Data d, float s) => s > LakeFrom(d) && s < LakeTo(d);
        static float LakeFrom(Data d) { float lo = float.MaxValue; for (int i = 0; i + 1 < d.narrow.Length; i += 2) lo = Mathf.Min(lo, d.narrow[i]); return lo; }
        static float LakeTo(Data d) { float hi = 0; for (int i = 0; i + 1 < d.narrow.Length; i += 2) hi = Mathf.Max(hi, d.narrow[i + 1]); return hi; }
    }

    // On a RaceDirector racing Leviathan: the course's own shortcuts, its gate stations, the spans where the Mountain AI rules and the
    // narrow stem apply, and its gate rule (the lake and mountain gates must be passed in order: crossing a later gate does not skip them).
    public sealed class LeviathanSwap : MonoBehaviour
    {
        public LeviathanCourse.Data Data; public WoodlandRoute[] Branches; public float[] GateHints;
        public bool Mountain(float s) => Data.In(Data.mountain, s);
        public bool Narrow(float s) => Data.In(Data.narrow, s);
        public bool Mandatory(int gate) => gate >= 0 && gate < Data.gates.Length && Data.gates[gate].mandatory;
    }

    // 0.116 Part A (Leviathan only, Forward and Reverse): the gates of later parts of the lap are hidden until they are next (Leviathan drives
    // its lake roads and the J twice, so a later gate is often in view first): each player's next gate is shown, and the start / finish.
    // RaceHud and RacingMiniMap read the course ahead from here: the next gate, the distance to it along the course, and a point ahead.
    public sealed class LeviathanGuide : MonoBehaviour
    {
        RaceDirector race; Renderer[][] gateRenderers; bool[] shown;
        public static bool Disabled; // checks only (Report106PlayerCheck lev116 -p116before: the 0.115 way, for the before shots); never set in play
        // 1.3.0-review5 Part D (Dan: "it needs to be easy to understand where to go"; "Why would a CP I already hit light up again"): a wrong
        // turn is said at once, big (RaceHud: WRONG TURN / TURN BACK, the arrow at the next gate): the player drove on through a gate already
        // passed (RaceDirector: Recrossed), or is 30 m or more off this pass of the course and getting further from the next gate for 1.5 s.
        // It goes when the next gate is passed, or the player is back on this pass heading along the course. Only the next gate is ever drawn.
        public int TurnBackGate { get; private set; } = -1; // the gate to turn back to (-1: none)
        public string TurnBackWhy { get; private set; } = "";
        int turnFrom = -1; float awaySeconds, onSeconds, lastGap = -1, heldUntil;
        public void Recrossed(RacerState r, int gate) { if (r != race.Racers[0]) return; Set(r.Progress.NextGate, "CP " + gate + " already passed"); heldUntil = Time.time + 2; }
        void Set(int gate, string why) { if (TurnBackGate != gate) Debug.Log($"Leviathan: TURN BACK to CP {gate} ({why})"); TurnBackGate = gate; TurnBackWhy = why; turnFrom = gate; onSeconds = 0; }
        void FixedUpdate()
        {
            if (!race || race.FreeRoam || Disabled || race.Racers.Count == 0) return;
            var me = race.Racers[0]; var car = me.Car; int next = me.Progress.NextGate;
            if (!car || !me.Progress.LapActive || me.Progress.Finished || race.Flow == null || race.Flow.State != RaceFlow.Stage.Racing || next <= 0) { TurnBackGate = -1; lastGap = -1; awaySeconds = 0; return; }
            if (TurnBackGate >= 0 && next != turnFrom) { TurnBackGate = -1; Debug.Log("Leviathan: TURN BACK cleared: CP " + turnFrom + " passed"); }
            float dt = Time.fixedDeltaTime; var p = car.Body.position;
            float gap = Vector3.ProjectOnPlane(race.GatePosition(next) - p, Vector3.up).magnitude;
            var ahead = Ahead(race, me, 12, 6); float off = Vector3.ProjectOnPlane(ahead[0] - p, Vector3.up).magnitude;
            var along = Vector3.ProjectOnPlane(ahead[ahead.Count - 1] - ahead[0], Vector3.up).normalized;
            bool free = me.Branch.Route == null && !me.InFlight;
            awaySeconds = free && off > 30 && lastGap >= 0 && gap > lastGap + .02f ? awaySeconds + dt : 0; lastGap = gap;
            if (TurnBackGate < 0 && awaySeconds >= 1.5f) Set(next, $"{off:0} m off the course, going away from CP {next}");
            if (TurnBackGate >= 0 && Time.time > heldUntil)
            {
                onSeconds = off < 15 && Vector3.Dot(car.Body.linearVelocity, along) > 2 ? onSeconds + dt : 0;
                if (onSeconds >= .5f) { TurnBackGate = -1; Debug.Log("Leviathan: TURN BACK cleared: back on the course"); }
            }
        }
        void Start() { race = GetComponent<RaceDirector>(); gateRenderers = race.gates.Select(g => g ? g.GetComponentsInChildren<Renderer>(true) : new Renderer[0]).ToArray(); shown = new bool[gateRenderers.Length]; }
        void LateUpdate()
        {
            if (!race || race.FreeRoam || gateRenderers == null) return;
            for (int i = 0; i < shown.Length; i++) shown[i] = Disabled || i == 0;
            foreach (var r in race.Racers) if (!r.IsAi && r.Progress.NextGate >= 0 && r.Progress.NextGate < shown.Length) shown[r.Progress.NextGate] = true;
            for (int i = 0; i < shown.Length; i++) foreach (var rd in gateRenderers[i]) if (rd && rd.enabled != shown[i]) rd.enabled = shown[i];
        }
        // the racer's station on the line (absolute), on the main road (a racer on a shortcut: where the shortcut rejoins, less what is left of it)
        public static float Station(RaceDirector race, RacerState r) => race.Origin + r.RoadPosition;
        // the course from a racer onwards, sampled every step metres for distance metres (its shortcut first when it is on one)
        public static List<Vector3> Ahead(RaceDirector race, RacerState r, float distance, float step = 8)
        {
            var list = new List<Vector3>(); float left = distance; float s;
            var b = r.Branch.Route;
            if (b)
            {
                float at = b.Project(r.Car.Body.position, out _); for (float x = at; x <= b.Length && left > 0; x += step, left -= step) list.Add(b.At(x, out _));
                s = b.exitRoad;
            }
            else s = Station(race, r);
            for (float x = 0; x <= left; x += step) list.Add(race.road.At(s + x, out _));
            return list;
        }
        // the distance along the course to the racer's next gate (the finish when it is 0)
        public static float ToNextGate(RaceDirector race, RacerState r)
        {
            int next = r.Progress.NextGate; float gate = next > 0 ? race.GateStation(next) : race.road.Length;
            return Mathf.Max(0, gate - r.RoadPosition);
        }
    }

    // 0.116: a shortcut board's lettering scaled to fit inside its board (the text mesh's size is known once it is built: at once, or the first frame)
    public sealed class LeviathanLettering : MonoBehaviour
    {
        public float width, height;
        public bool Fit() { var b = GetComponent<MeshRenderer>().localBounds; if (b.size.x < 1e-4f) return false; transform.localScale = Vector3.one * Mathf.Min(height / b.size.y, width / b.size.x); enabled = false; return true; }
        void LateUpdate() => Fit();
    }
}
