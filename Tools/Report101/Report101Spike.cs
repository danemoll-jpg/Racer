using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 0.101 Part B follow-up: the triangles that formed the spikes, once flattened, lie on the paving they stood out of (a sliver z-fights).
// Remove them if the paving under them is whole without them.
public static class Report101Spike { public static void Run(){var log=new List<string>();
 var sc=EditorSceneManager.OpenScene("Assets/Scenes/FreeRoamWorld.unity");var g=GameObject.Find("Ground_Dan beige concrete descent and parking");var mf=g.GetComponent<MeshFilter>();var m=mf.sharedMesh;var v=m.vertices;var t=m.triangles;
 var spikeTris=new List<int>();for(int k=0;k<t.Length;k+=3)if(t[k]==8815||t[k]==8816||t[k+1]==8815||t[k+1]==8816||t[k+2]==8815||t[k+2]==8816)spikeTris.Add(k);
 log.Add($"{AssetDatabase.GetAssetPath(m)}: {spikeTris.Count} triangles use the two spike vertices");
 foreach(var k in spikeTris)log.Add($"  tri {k/3}: {v[t[k]]} {v[t[k+1]]} {v[t[k+2]]} (vertices {t[k]},{t[k+1]},{t[k+2]})");
 var keep=new List<int>();for(int k=0;k<t.Length;k+=3)if(!spikeTris.Contains(k))keep.AddRange(new[]{t[k],t[k+1],t[k+2]});
 // is every spike triangle's area still covered by another paving triangle?
 var test=UnityEngine.Object.Instantiate(m);test.SetTriangles(keep,0);var go=new GameObject("probe");var mc=go.AddComponent<MeshCollider>();mc.sharedMesh=test;Physics.SyncTransforms();int covered=0,samples=0;
 foreach(var k in spikeTris)for(float a=.1f;a<.9f;a+=.2f)for(float b=.1f;a+b<.95f;b+=.2f){var p=v[t[k]]+(v[t[k+1]]-v[t[k]])*a+(v[t[k+2]]-v[t[k]])*b;samples++;if(mc.Raycast(new Ray(p+Vector3.up*2,Vector3.down),out var h,4))covered++;}
 UnityEngine.Object.DestroyImmediate(go);log.Add($"  without them, {covered}/{samples} sample points still on paving");
 if(samples>0&&covered==samples){m.SetTriangles(keep,0);m.RecalculateBounds();EditorUtility.SetDirty(m);var col=g.GetComponent<MeshCollider>();col.sharedMesh=null;col.sharedMesh=m;EditorSceneManager.MarkSceneDirty(sc);EditorSceneManager.SaveScene(sc);AssetDatabase.SaveAssets();log.Add("  removed");}
 else log.Add("  kept (they cover paving nothing else does)");
 File.WriteAllLines(Environment.GetEnvironmentVariable("PROBE_OUT")+"/spike.txt",log);EditorApplication.Exit(0);}}
