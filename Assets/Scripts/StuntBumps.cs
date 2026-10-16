using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Racer
{
    // 0.120 Part C (Dan: "perhaps we can add some bumps on the stunt tracks in order to be able to get some extra points"): kickers and bumps on the
    // four Stunt Race courses, in Stunt Races only (never a normal race or Free Roam): earth wedges on the existing ground beside or on the edge of
    // the course line (LeviathanCourse.Kicker: solid, in the stunt park's dirt colour, the ground beyond unchanged), so there is air to trick
    // in between the big jumps; you choose to hit one or not. Each lands on a down-slope; none on or just before a big jump, on a gate or across the
    // whole road. Placed by Tools/Report120/py/bumps.py from the course probes (Resources/StuntBumps.json, keyed by the start of the course id).
    // Built at each race start (RaceDirector.RestartRace) when the race is a Stunt Race, removed otherwise.
    // 0.121 (BUG-005): Leviathan Forward's Kicker 6 moved from Hwy 92 east (s 6756, 85 m before CP 44) to Hwy 92 west (s 6510, 191 m before CP 43):
    // at Hard its flights carried 100 m and more, over and beside CP 44 (Tools/Report121/py/bump6.py lists the places 0.120's rules allow).
    public static class StuntBumps
    {
        [Serializable] public sealed class Course { public string course; public LeviathanCourse.Kicker[] bumps; }
        [Serializable] sealed class Data { public Course[] courses; }
        const string RootName = "Stunt Race bumps";
        // checks only (Report106PlayerCheck -p120bumps <file>): bumps read from that file instead of the built-in one; never set in play
        public static string Override;
        public static int Built { get; private set; }
        public static Course For(string courseId)
        {
            string text = !string.IsNullOrEmpty(Override) && System.IO.File.Exists(Override) ? System.IO.File.ReadAllText(Override) : Resources.Load<TextAsset>("StuntBumps")?.text;
            var data = string.IsNullOrEmpty(text) ? null : JsonUtility.FromJson<Data>(text);
            return data?.courses?.FirstOrDefault(c => !string.IsNullOrEmpty(courseId) && courseId.StartsWith(c.course));
        }
        public static void Apply(RaceDirector race)
        {
            var scene = race.gameObject.scene; Built = 0;
            foreach (var old in scene.GetRootGameObjects().Where(g => g.name == RootName)) { old.SetActive(false); UnityEngine.Object.Destroy(old); }
            if (!race.stunts || race.FreeRoam) return;
            var course = For(race.courseId); if (course?.bumps == null || course.bumps.Length == 0) return;
            var root = new GameObject(RootName).transform; SceneManager.MoveGameObjectToScene(root.gameObject, scene);
            // the stunt park's dirt (its vertex-coloured material and colour), so they read as dirt kickers on any ground
            var dirt = Resources.Load<Material>("Scenery/Building");
            foreach (var b in course.bumps) if (LeviathanCourse.BuildKicker(b, root, dirt, "Stunt Race", StuntPark.DirtColour)) Built++;
            Physics.SyncTransforms();
        }
    }
}
