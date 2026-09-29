using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using Racer;
public static class InspectShortcutRevision {
 public static string Main(){
 var root=GameObject.Find("Backyard optional forest shortcuts");var rows=new List<string>();
 foreach(var b in UnityEngine.Object.FindObjectsByType<WoodlandRoute>())for(float s=0;s<b.Length;s+=5){var p=b.At(s,out var f);float ground=Physics.RaycastAll(p+Vector3.up*200,Vector3.down,400,1).Where(h=>h.collider.name.StartsWith("Ground_")).First().point.y;rows.Add($"{b.title} {s:F0} {p} ground={ground:F2} grade={f.y/Mathf.Max(.01f,new Vector2(f.x,f.z).magnitude):F2}");}
 foreach(Transform t in root.transform)if(!t.name.Contains("plank")&&!t.name.Contains("chevron"))rows.Add($"OBJ {t.name} pos={t.position} scale={t.localScale} colliders={t.GetComponentsInChildren<Collider>().Length}");
 Directory.CreateDirectory("Docs/ShortcutRevision");File.WriteAllLines("Docs/ShortcutRevision/baseline.txt",rows);return string.Join("\n",rows);
 }
}
