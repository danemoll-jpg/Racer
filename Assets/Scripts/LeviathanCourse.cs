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
        [Serializable] public sealed class Data
        {
            public int version; public string id, title, scene; public bool reverse; public float[] pts, geometryStations, bypass, mountain, narrow, spawn; public float geometryLength, spawnYaw, length; public int highwayTraffic;
            public Gate[] gates; public Branch[] branches; public MountainFlights.Flight[] flights;
            public string[] fills; // ground patches added for this course only (Resources/Leviathan): Forward's creek gully under the lake path
            public Vector3[] Points => V(pts);
            public bool In(float[] spans, float s) { if (spans == null) return false; for (int i = 0; i + 1 < spans.Length; i += 2) if (s >= spans[i] && s <= spans[i + 1]) return true; return false; }
        }
        static Vector3[] V(float[] f) { var v = new Vector3[(f?.Length ?? 0) / 3]; for (int i = 0; i < v.Length; i++) v[i] = new Vector3(f[3 * i], f[3 * i + 1], f[3 * i + 2]); return v; }
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
            var swap = race.gameObject.AddComponent<LeviathanSwap>(); swap.Data = data; swap.Branches = branches.ToArray(); swap.GateHints = data.gates.Select(g => g.station).ToArray();
            race.road = road; race.gates = gates.ToArray(); race.courseId = data.id; race.courseName = data.title; race.reverseCourse = data.reverse; race.highwayTrafficCount = data.highwayTraffic;
            Debug.Log($"Leviathan: {data.title} in {race.gameObject.scene.name}: line {road.Length:F0} m ({road.points.Length} points), {gates.Count} gates ({gates.Count(g => g.transform.parent != root)} the scene's own), {branches.Count} shortcuts, {data.flights?.Length ?? 0} flight(s)");
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
                if (k < 0 || best > (data.In(data.narrow, s[k]) ? 4.5f : 3.5f) || data.In(data.mountain, s[k]) || !OnLake(data, s[k])) continue;
                var size = Vector3.Scale(box.size, t.lossyScale);
                SceneryTrees.Cleared.Add((bottom, Mathf.Min(Mathf.Abs(size.x), Mathf.Abs(size.z)), Mathf.Abs(size.y), data.title + " lake trail (0.115)", t.name));
                box.enabled = false; if (t.GetComponents<Component>().Length == 2 && t.childCount == 0) t.gameObject.SetActive(false); cleared++; names.Add($"{t.name} at {bottom.x:F1},{bottom.z:F1} ({best:F1} m from the line, s {s[k]:F0})");
            }
            Debug.Log($"Leviathan: {cleared} tree trunk(s) standing on the lake trails cleared in {scene.name}" + (names.Count > 0 ? ":\n" + string.Join("\n", names) : ""));
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
}
