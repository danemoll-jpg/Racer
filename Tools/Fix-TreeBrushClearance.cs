using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using Racer;
public static class FixTreeBrushClearance {
 static float Distance(Vector3 p,Vector3[] route){float best=float.MaxValue;var q=new Vector2(p.x,p.z);for(int i=0;i<route.Length;i++){var a=new Vector2(route[i].x,route[i].z);var b=route[(i+1)%route.Length];var v=new Vector2(b.x,b.z)-a;best=Mathf.Min(best,(q-a-v*Mathf.Clamp01(Vector2.Dot(q-a,v)/v.sqrMagnitude)).sqrMagnitude);}return Mathf.Sqrt(best);}
 public static string Main(){
 if(Application.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Saved edit mode required");
 var race=UnityEngine.Object.FindAnyObjectByType<RaceDirector>();if(race.gameObject.scene.name!="DansBackyardForward")throw new Exception("Open Backyard first");
 var field=UnityEngine.Object.FindObjectsByType<ShortcutUndergrowth>(FindObjectsSortMode.None).Single(x=>x.name.StartsWith("Tree-Top Trail"));
 var mesh=field.GetComponent<MeshFilter>().sharedMesh;int count=44; // Three eight-vertex shrubs and five four-vertex fern fronds per authored plant.
 var vertices=mesh.vertices;var triangles=mesh.triangles;if(vertices.Length%count!=0)throw new Exception("Unexpected combined bush topology");
 var keep=new bool[vertices.Length/count];float minimum=float.MaxValue;int removed=0;for(int b=0;b<keep.Length;b++){float clearance=float.MaxValue;for(int j=0;j<count;j++)clearance=Mathf.Min(clearance,Distance(field.transform.TransformPoint(vertices[b*count+j]),race.road.points));keep[b]=clearance>=4.25f;if(keep[b])minimum=Mathf.Min(minimum,clearance);else removed++;}
 var remap=new int[vertices.Length];var v=new List<Vector3>();var n=new List<Vector3>();var uv=new List<Vector2>();var normals=mesh.normals;var uvs=mesh.uv;for(int i=0;i<vertices.Length;i++){remap[i]=-1;if(!keep[i/count])continue;remap[i]=v.Count;v.Add(vertices[i]);n.Add(normals[i]);if(uvs.Length==vertices.Length)uv.Add(uvs[i]);}
 var t=new List<int>();for(int i=0;i<triangles.Length;i+=3)if(remap[triangles[i]]>=0){t.Add(remap[triangles[i]]);t.Add(remap[triangles[i+1]]);t.Add(remap[triangles[i+2]]);}
 mesh.Clear();mesh.SetVertices(v);mesh.SetNormals(n);mesh.SetUVs(0,uv);mesh.SetTriangles(t,0);mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);AssetDatabase.SaveAssets();
 Directory.CreateDirectory("Docs/WorldMap");string report=$"Removed {removed}/{keep.Length} complete Tree-Top bushes intersecting 4.25m planar main-route clearance; retained {keep.Count(x=>x)}; closest retained foliage {minimum:F3}m. Trail half-width 2.65m; shoulder >=1.60m. Resistance field, scene, structures and cabin unchanged.";File.WriteAllText("Docs/WorldMap/bush-clearance.txt",report);return report;
 }
}

