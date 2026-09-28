using System;using System.IO;using System.Linq;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
public static class InspectExitProperty {
 public static string Main(){
 EditorSceneManager.OpenScene("Assets/Scenes/StreetLoopGreybox.unity");var race=Object.FindAnyObjectByType<RaceDirector>();race.road.Initialize();var b=Object.FindObjectsByType<WoodlandRoute>().Single(b=>b.title=="Pine Ridge");
 File.WriteAllLines("Docs/FiveUpdates/pine-tail.txt",Enumerable.Range(0,31).Select(i=>{var p=b.At(b.Length-150+i*5,out var f);var s=race.road.Project(p,out float d);return $"{b.Length-150+i*5:F1} p={p:F2} f={f:F2} main={s:F1} lateral={d:F2}";}));
 var home=GameObject.Find("Dan - blue X").transform;
 File.WriteAllLines("Docs/FiveUpdates/property-pieces.txt",home.GetComponentsInChildren<Renderer>().Select(r=>r.name+" center="+home.InverseTransformPoint(r.bounds.center).ToString("F2")+" size="+r.bounds.size.ToString("F2")));
 return "Exit tail and property bounds recorded";
 }
}
