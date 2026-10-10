using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
public static class Report101Bush { public static void Run(){var log=new List<string>();
 EditorSceneManager.OpenScene("Assets/Scenes/DansBackyardForward.unity");var u=UnityEngine.Object.FindObjectsByType<Racer.ShortcutUndergrowth>(FindObjectsSortMode.None).First(x=>x.clearRoute&&x.clearRoute.title=="Abandoned Cabin Jump");var line=u.clearRoute;line.Initialize();
 var m=u.GetComponent<MeshFilter>().sharedMesh;var M=u.transform.localToWorldMatrix;var v=m.vertices;var t=m.triangles;log.Add($"mesh {m.name} {AssetDatabase.GetAssetPath(m)} verts {v.Length} tris {t.Length/3} readable {m.isReadable} transform {u.transform.position} {u.transform.eulerAngles}");
 var used=new HashSet<int>(t);int unusedIn=0;float lastUsed=0,lastAny=0;
 for(int i=0;i<v.Length;i++){var w=M.MultiplyPoint3x4(v[i]);float s=line.Project(w,out float lat);if(s>=u.brushFrom&&lat<u.clearHalfWidth&&s<u.clearFrom+6){lastAny=Mathf.Max(lastAny,s);if(used.Contains(i))lastUsed=Mathf.Max(lastUsed,s);else unusedIn++;}}
 log.Add($"in the corridor: last vertex of any bush s {lastAny:F1}; last vertex still drawn (used by a triangle) s {lastUsed:F1}; {unusedIn} vertices of removed bushes still in the vertex list");
 File.WriteAllLines(Environment.GetEnvironmentVariable("PROBE_OUT")+"/bush.txt",log);EditorApplication.Exit(0);}}
