using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Racer.Editor
{
    public static class IntegratedRaceSetup
    {
        public static void Apply()
        {
            var scene = SceneManager.GetActiveScene();
            if (Application.isPlaying || scene.isDirty || scene.path != StreetLoopBuilder.ScenePath)
                throw new InvalidOperationException("Open saved StreetLoopGreybox outside play mode.");
            var race = UnityEngine.Object.FindAnyObjectByType<RaceDirector>();
            var road = race.GetComponent<RaceRoad>();
            if (!road)
                road = race.gameObject.AddComponent<RaceRoad>();
            road.points = StreetLoopBuilder.Route().ToArray();
            race.road = road;
            road.Initialize();
            var jump = GameObject.Find(Phase4Setup.RootName).transform;
            road.bypassStart = road.Project(jump.position - jump.forward * 95, out _);
            road.bypassEnd = road.Project(jump.position + jump.forward * 130, out _);
            race.vehicle.topSpeed = 49;
            race.vehicle.acceleration = 14.5f;
            PrefabUtility.RecordPrefabInstancePropertyModifications(race.vehicle);
            PlayerSettings.bundleVersion = "0.3.0-review1";
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }
    }
}
