using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Racer.Editor
{
    // 0.114 Part A: every generated roadside building (the Hwy 92 and Trickum Rd plans, Hwy92Roadside) is built in each world scene in the
    // build and every drawn and collider triangle on its outside is checked to face away from the building (a face turned inward is culled
    // from outside: the 0.113 "half built" buildings). Run before release: menu Racer > Checks > Generated building faces, or batch mode
    // -executeMethod Racer.Editor.GeneratedBuildingFacesCheck.Batch (BUILDING_FACES_OUT = the report file). Scenes are opened, never saved.
    public static class GeneratedBuildingFacesCheck
    {
        [MenuItem("Racer/Checks/Generated building faces")]
        public static void Menu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var lines = Run(out bool ok);
            Debug.Log(string.Join("\n", lines));
            EditorUtility.DisplayDialog("Generated building faces", ok ? "PASS: every generated building faces outward." : "FAIL: see the Console.", "OK");
        }

        public static void Batch()
        {
            int code = 1;
            try
            {
                var lines = Run(out bool ok); code = ok ? 0 : 2;
                var output = Environment.GetEnvironmentVariable("BUILDING_FACES_OUT");
                if (string.IsNullOrEmpty(output)) output = "Temp/generated-building-faces.txt";
                Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output)));
                File.WriteAllLines(output, lines);
            }
            catch (Exception e) { Debug.LogException(e); }
            EditorApplication.Exit(code);
        }

        public static List<string> Run(out bool ok)
        {
            var lines = new List<string> { $"Generated building faces check, {DateTime.Now:yyyy-MM-dd HH:mm}" };
            ok = true; int worlds = 0;
            // the release build's scenes (as Tools/Report113/Build-Report113.cs.txt)
            var scenes = ReverseReviewRelease.Scenes.Concat(new[] { "Assets/Scenes/MountainLoop.unity", "Assets/Scenes/MountainLoopReverse.unity", "Assets/Scenes/DansBackyardForward.unity", "Assets/Scenes/DansBackyardReverse.unity", "Assets/Scenes/FreeRoamWorld.unity" }).Distinct();
            foreach (var path in scenes)
            {
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                if (!scene.GetRootGameObjects().Any(g => g.name == "Remembered houses and approximate buildings")) continue;
                worlds++;
                Hwy92Roadside.Clear(scene); // as at load: the old roadside out, the ground cache fresh for this scene
                var host = new GameObject("Generated building faces check");
                foreach (var plan in Hwy92Roadside.Plans)
                {
                    Hwy92Roadside.FaceFailures = new List<string>(); Hwy92Roadside.FacesChecked = Hwy92Roadside.PartsChecked = 0;
                    try { Hwy92Roadside.Attach(host, plan).Build(scene); }
                    finally
                    {
                        var failures = Hwy92Roadside.FaceFailures; Hwy92Roadside.FaceFailures = null;
                        bool pass = failures.Count == 0 && Hwy92Roadside.PartsChecked > 0; ok &= pass;
                        lines.Add($"{(pass ? "PASS" : "FAIL")} {Path.GetFileNameWithoutExtension(path)} {plan}: {Hwy92Roadside.PartsChecked} building parts, {Hwy92Roadside.FacesChecked} triangles, {failures.Count} parts with inward faces");
                        lines.AddRange(failures.Select(f => "  " + f));
                    }
                }
            }
            if (worlds == 0) { ok = false; lines.Add("FAIL: no world scene in the build"); }
            lines.Add(ok ? "RESULT PASS" : "RESULT FAIL");
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single); // leave nothing built in an open scene
            return lines;
        }
    }
}
