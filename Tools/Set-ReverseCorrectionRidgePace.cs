using System;using System.Linq;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;
public static class SetReverseCorrectionRidgePace {
 public static string Main(){EditorSceneManager.OpenScene("Assets/Scenes/DansBackyardReverse.unity");var b=UnityEngine.Object.FindObjectsByType<WoodlandRoute>().First(b=>b.title.StartsWith("Abandoned"));b.entrySpeed=b.recommendedSpeed=18;EditorSceneManager.MarkSceneDirty(b.gameObject.scene);EditorSceneManager.SaveScene(b.gameObject.scene);AssetDatabase.Refresh();return "Local technical ridge recommendation 18m/s; normal motor unchanged";}
}
