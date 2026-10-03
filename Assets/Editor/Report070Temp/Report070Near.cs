using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Object=UnityEngine.Object;
// 0.70 read-only: every renderer/collider whose bounds come within r of a point. PROBE_NEAR="scene|x,y,z|r;..."
public static class Report070Near {
 static string Path(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n.Replace('\n','/');}
 public static void Run(){var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(outDir);var rows=new List<string>();string open=null;
  foreach(var spec in Environment.GetEnvironmentVariable("PROBE_NEAR").Split(';')){var a=spec.Split('|');if(a[0]!=open){EditorSceneManager.OpenScene("Assets/Scenes/"+a[0]+".unity");open=a[0];}
   var v=a[1].Split(',').Select(float.Parse).ToArray();var p=new Vector3(v[0],v[1],v[2]);float r=float.Parse(a[2]);rows.Add($"NEAR {a[0]} {p} r {r}");
   foreach(var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None)){var rend=t.GetComponent<Renderer>();var col=t.GetComponent<Collider>();if(!rend&&!col)continue;var bb=rend?rend.bounds:col.bounds;
    float d=Mathf.Sqrt(bb.SqrDistance(p));if(d>r||bb.size.x>150||bb.size.z>150)continue;var mf=t.GetComponent<MeshFilter>();
    rows.Add($"{d,6:F1} {(t.gameObject.activeInHierarchy?"A":"-")}{(rend&&rend.enabled?"R":"-")}{(col&&col.enabled?"C":"-")} {Path(t)} | pos={t.position:F2} | b={bb.center:F1} sz={bb.size:F1} | {(mf&&mf.sharedMesh?AssetDatabase.GetAssetPath(mf.sharedMesh)+":"+mf.sharedMesh.name:"")}");}}
  File.WriteAllLines(outDir+"/near.txt",rows);EditorApplication.Exit(0);}
}
