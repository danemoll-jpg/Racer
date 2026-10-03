using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Object=UnityEngine.Object;
// 0.72 read-only: dump one scene object's mesh in world space (vertices, triangles with world normal y and area).
// PROBE_MESH="scene|object name"
public static class Report072Mesh {
 public static void Run(){var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(outDir);var rows=new List<string>();
  foreach(var spec in Environment.GetEnvironmentVariable("PROBE_MESH").Split(';')){var a=spec.Split('|');EditorSceneManager.OpenScene("Assets/Scenes/"+a[0]+".unity");
   foreach(var mf in Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(m=>m.name==a[1])){var m=mf.sharedMesh;var tr=mf.transform.localToWorldMatrix;
    rows.Add($"MESH {a[0]} {mf.name} {AssetDatabase.GetAssetPath(m)} v {m.vertexCount} t {m.triangles.Length/3} sub {m.subMeshCount} uv {m.uv.Length} col {m.colors.Length} pos {mf.transform.position} rot {mf.transform.eulerAngles} scale {mf.transform.lossyScale}");
    var vs=m.vertices.Select(v=>tr.MultiplyPoint3x4(v)).ToArray();var uv=m.uv;for(int i=0;i<vs.Length;i++)rows.Add($"v {i} {vs[i].x:F2} {vs[i].y:F2} {vs[i].z:F2}"+(uv.Length>0?$" uv {uv[i].x:F3} {uv[i].y:F3}":""));
    var t=m.triangles;for(int i=0;i<t.Length;i+=3){var n=Vector3.Cross(vs[t[i+1]]-vs[t[i]],vs[t[i+2]]-vs[t[i]]);rows.Add($"t {i/3} {t[i]} {t[i+1]} {t[i+2]} ny {(n.magnitude>0?n.normalized.y:0):F2} area {n.magnitude/2:F2}");}}}
  File.WriteAllLines(outDir+"/mesh.txt",rows);EditorApplication.Exit(0);}
}
