using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Racer.Editor
{
    // 0.76: regenerates Resources/WorldMaps/CoursePreviews.json from each course scene's own route data (main route,
    // shortcuts, gates, start pose, vehicle rule), so the map and Free Roam can use every course without loading its scene.
    // Run it again whenever a course changes (menu Racer/Export course routes, or -executeMethod ... .Run in batch mode).
    public static class CourseRouteExport
    {
        public const string Output = "Assets/Resources/WorldMaps/CoursePreviews.json";
        // Dan, 2026-10-04: the Forest Loop trails and the Mountain race roads are race-only geometry (not in FreeRoamWorld).
        static bool RaceOnly(string scene) => scene == "LakeWoods" || scene == "ForestLoopReverse" || scene.StartsWith("MountainLoop");
        [MenuItem("Racer/Export course routes")]
        public static void Export()
        {
            if (EditorApplication.isPlaying || EditorSceneManager.GetActiveScene().isDirty) throw new System.Exception("Saved edit mode required");
            var original = EditorSceneManager.GetActiveScene().path; var courses = new List<CoursePreviewCatalog.Course>();
            foreach (string name in RacePlaylists.Scenes)
            {
                var scene = EditorSceneManager.OpenScene("Assets/Scenes/" + name + ".unity", OpenSceneMode.Single);
                var roots = scene.GetRootGameObjects(); var race = roots.SelectMany(g => g.GetComponentsInChildren<RaceDirector>(true)).Single();
                var respawn = race.vehicle.GetComponent<VehicleRespawn>(); var start = respawn && respawn.spawnPoint ? respawn.spawnPoint : race.vehicle.transform;
                courses.Add(new CoursePreviewCatalog.Course
                {
                    scene = name, id = race.courseId, main = race.road.points.ToArray(), gates = race.gates.Select(g => g.transform.position).ToArray(),
                    branches = roots.SelectMany(g => g.GetComponentsInChildren<WoodlandRoute>()).Where(b => b.gameObject.activeInHierarchy).Select(b => new CoursePreviewCatalog.Path { points = b.points.ToArray(), undergroundStart = b.undergroundStart, undergroundEnd = b.undergroundEnd }).ToArray(),
                    start = start.position, startYaw = start.eulerAngles.y, forest = race.Forest, raceOnly = RaceOnly(name)
                });
            }
            File.WriteAllText(Output, JsonUtility.ToJson(new CoursePreviewCatalog { courses = courses.ToArray() }, true)); AssetDatabase.ImportAsset(Output);
            if (!string.IsNullOrEmpty(original)) EditorSceneManager.OpenScene(original);
            Debug.Log("Course routes exported: " + string.Join(", ", courses.Select(c => $"{c.scene} main={c.main.Length} branches={c.branches.Length} gates={c.gates.Length} forest={c.forest} raceOnly={c.raceOnly}")));
        }
        public static void Run() { Export(); EditorApplication.Exit(0); }
    }
}
