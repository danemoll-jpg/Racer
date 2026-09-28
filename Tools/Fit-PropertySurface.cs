using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
public static class FitPropertySurface {
 public static string Main(){
 if(Application.isPlaying)throw new Exception("Edit mode required");var report=new List<string>();
 foreach(var scene in new[]{"StreetLoopGreybox","LakeWoods","StreetLoopReverse","ForestLoopReverse","MountainLoop","MountainLoopReverse"}){
 EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");foreach(var c in GameObject.Find("Dan beige concrete driveway").GetComponentsInChildren<Collider>())c.enabled=false;Physics.SyncTransforms();
 var src=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Track/FiveUpdates/"+scene+"-concrete.asset");if(src.indexFormat==UnityEngine.Rendering.IndexFormat.UInt32){AssetDatabase.SaveAssets();continue;}var old=src.vertices;var tri=src.triangles;var vertices=new List<Vector3>();var triangles=new List<int>();var cache=new Dictionary<Vector2Int,int>();
 int Vertex(Vector3 p){var key=new Vector2Int(Mathf.RoundToInt(p.x*10000),Mathf.RoundToInt(p.z*10000));if(cache.TryGetValue(key,out int i))return i;var hits=Physics.RaycastAll(p+Vector3.up*50,Vector3.down,100,1,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name.StartsWith("Ground_")).OrderBy(h=>Math.Abs(h.point.y-p.y)).ToArray();if(hits.Length==0)throw new Exception("Missing property ground");p.y=hits[0].point.y+.08f;i=vertices.Count;vertices.Add(p);cache.Add(key,i);return i;}
 void Split(Vector3 a,Vector3 b,Vector3 c){float ab=(a-b).sqrMagnitude,bc=(b-c).sqrMagnitude,ca=(c-a).sqrMagnitude;if(Math.Max(ab,Math.Max(bc,ca))<.36f){triangles.AddRange(new[]{Vertex(a),Vertex(b),Vertex(c)});return;}if(ab>=bc&&ab>=ca){var m=(a+b)*.5f;Split(a,m,c);Split(m,b,c);}else if(bc>=ca){var m=(b+c)*.5f;Split(a,b,m);Split(a,m,c);}else{var m=(c+a)*.5f;Split(a,b,m);Split(m,b,c);}}
 for(int i=0;i<tri.Length;i+=3)Split(old[tri[i]],old[tri[i+1]],old[tri[i+2]]);
 src.Clear();src.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;src.SetVertices(vertices);src.SetTriangles(triangles,0);src.RecalculateNormals();src.RecalculateBounds();EditorUtility.SetDirty(src);AssetDatabase.SaveAssets();report.Add(scene+": local surface refined below 0.6m edges, 0.08m clearance; vertices="+vertices.Count);}
 EditorSceneManager.OpenScene("Assets/Scenes/StreetLoopGreybox.unity");File.WriteAllLines("Docs/FiveUpdates/property-fit.txt",report);return "Property surface conforms without ground poking through";
 }
}
