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
        [Serializable] public sealed class Data
        {
            public int version; public string id, title, scene; public bool reverse; public float[] pts, geometryStations, bypass, mountain, narrow, spawn; public float geometryLength, spawnYaw, length; public int highwayTraffic;
            public Gate[] gates; public Branch[] branches; public MountainFlights.Flight[] flights;
            public string[] fills; // ground patches added for this course only (Resources/Leviathan): Forward's creek gully under the lake path
            public Sign[] signs; // 0.116 Part A: turn indicators at the course's decision points (Tools/Report116/py/signs.py)
            public float[] clear; // 0.116 Part B: x, z, radius: tree trunks cleared for this race only (on its shortcut lines)
            public Vector3[] Points => V(pts);
            public bool In(float[] spans, float s) { if (spans == null) return false; for (int i = 0; i + 1 < spans.Length; i += 2) if (s >= spans[i] && s <= spans[i + 1]) return true; return false; }
        }
        static Vector3[] V(float[] f) { var v = new Vector3[(f?.Length ?? 0) / 3]; for (int i = 0; i < v.Length; i++) v[i] = new Vector3(f[3 * i], f[3 * i + 1], f[3 * i + 2]); return v; }
        // 0.116 Part B: Leviathan Reverse's shortcuts Dan switched off (settings.json "leviathanShortcutsOff", the F6 debug menu); set by RaceFlow
        public static string[] ShortcutsOff = Array.Empty<string>();
        public static bool ShortcutOn(string title) => Array.IndexOf(ShortcutsOff ?? Array.Empty<string>(), title) < 0;
        public static Data Load(string key) { var a = Resources.Load<TextAsset>("Leviathan/" + (key == Reverse ? "LeviathanReverse" : "LeviathanForward")); return a ? JsonUtility.FromJson<Data>(a.text) : null; }

        // RaceDirector.Awake, before it reads its road and gates
        public static void Apply(RaceDirector race)
        {
            if (Active == null || race.gameObject.scene.name != SceneOf(Active) || race.GetComponent<LeviathanSwap>()) return;
            var data = Current = Load(Active); if (data == null) { Debug.LogError("Leviathan: course data missing for " + Active); Active = null; return; }
            var root = new GameObject(data.title + " (course)").transform; SceneManager.MoveGameObjectToScene(root.gameObject, race.gameObject.scene);
            // Leviathan's own ground patches (Dan, 2026-10-14: for this race only, nowhere else): Forward fills the creek gully that crosses the lake
            // path's south shore in MountainLoop.unity (Free Roam has flat ground there), drawn with the scene's own ground material
            var groundMaterial = race.gameObject.scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<MeshRenderer>(true)).FirstOrDefault(r => r.name == "Ground_640_240")?.sharedMaterial;
            foreach (var name in data.fills ?? Array.Empty<string>())
            {
                var mesh = Resources.Load<Mesh>("Leviathan/" + name); if (!mesh) { Debug.LogError("Leviathan: ground patch missing: " + name); continue; }
                var fill = new GameObject("Ground_" + mesh.name); fill.transform.SetParent(root, false);
                fill.AddComponent<MeshFilter>().sharedMesh = mesh; var mr = fill.AddComponent<MeshRenderer>(); if (groundMaterial) mr.sharedMaterial = groundMaterial; fill.AddComponent<MeshCollider>().sharedMesh = mesh;
            }
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
                gate.name = g.name; gate.transform.SetPositionAndRotation(new Vector3(g.pos[0], g.pos[1], g.pos[2]), Quaternion.Euler(g.rot[0], g.rot[1], g.rot[2]));
                gate.halfWidth = g.halfWidth; gate.halfHeight = g.halfHeight; gate.upperHeight = g.upperHeight; gate.gameObject.SetActive(true); gates.Add(gate);
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
            race.gameObject.AddComponent<LeviathanGuide>(); // 0.116 Part A: only the next gate shown, the course ahead on the minimap, the next CP on the HUD
            var swap = race.gameObject.AddComponent<LeviathanSwap>(); swap.Data = data; swap.Branches = branches.ToArray(); swap.GateHints = data.gates.Select(g => g.station).ToArray();
            race.road = road; race.gates = gates.ToArray(); race.courseId = data.id; race.courseName = data.title; race.reverseCourse = data.reverse; race.highwayTrafficCount = data.highwayTraffic;
            Debug.Log($"Leviathan: {data.title} in {race.gameObject.scene.name}: line {road.Length:F0} m ({road.points.Length} points), {gates.Count} gates ({gates.Count(g => g.transform.parent != root)} the scene's own), {branches.Count} shortcuts, {data.flights?.Length ?? 0} flight(s)");
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
        static void Box(Transform parent, string name, Vector3 at, Vector3 size, Material m)
        {
            var g = GameObject.CreatePrimitive(PrimitiveType.Cube); g.name = name; UnityEngine.Object.Destroy(g.GetComponent<Collider>());
            g.transform.SetParent(parent, false); g.transform.localPosition = at; g.transform.localScale = size; g.GetComponent<Renderer>().sharedMaterial = m;
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
