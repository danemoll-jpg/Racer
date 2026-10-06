using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Object=UnityEngine.Object;
// 0.81 Part 0.3 (BUG-003, Mountain Loop Reverse at (997.8, 152.5, 319.2)): the 0.80 deck embankment was also built at
// seams INSIDE the CR133 driving surface (two deck pieces meeting: an open mesh edge with more road beyond it), where its
// strip runs from 5 cm under the road down under the other piece. Its top edge meets the road plane and catches the
// wheels / body (a vertical contact): bikes flip. This removes exactly those strips from the embankment mesh asset: a
// strip whose ground beyond its top edge (0.6 m outward, toward its foot) is still a driving surface within 0.35 m of the
// edge's height. Every other strip, and every other collider, route and scene object, is unchanged. AUTHOR_DRY=1 measures only.
public static class Report081Author {
 const string Asset="Assets/Track/Report080/MountainLoopReverse-mountain decks embankment.asset";
 public static void Run(){
  bool dry=Environment.GetEnvironmentVariable("AUTHOR_DRY")=="1";var notes=new List<string>();
  try{
   EditorSceneManager.OpenScene("Assets/Scenes/MountainLoopReverse.unity");Physics.SyncTransforms();Physics.queriesHitBackfaces=false;
   var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(Asset);var g=Object.FindObjectsByType<MeshFilter>().First(m=>m.sharedMesh==mesh);var col=g.GetComponent<MeshCollider>();
   var v=mesh.vertices.Select(x=>g.transform.TransformPoint(x)).ToArray();var t=mesh.triangles;var keep=new List<int>();int removed=0;var listed=new List<string>();
   bool Drive(Vector3 p,float y){foreach(var h in Physics.RaycastAll(new Vector3(p.x,y+1.5f,p.z),Vector3.down,3,~0,QueryTriggerInteraction.Ignore))if(h.collider!=col&&h.collider.name.Contains("driving surface")&&Mathf.Abs(h.point.y-y)<.35f)return true;return false;}
   // strips were written as 6 indices: (ta,tb,bb,ta,bb,ba) or (ta,bb,tb,ta,ba,bb); tops are the even vertices
   for(int i=0;i+5<t.Length;i+=6){
    var q=t.Skip(i).Take(6).Distinct().ToArray();var tops=q.Where(k=>k%2==0).ToArray();var bots=q.Where(k=>k%2==1).ToArray();
    if(tops.Length!=2||bots.Length!=2){keep.AddRange(t.Skip(i).Take(6));continue;}
    var top=(v[tops[0]]+v[tops[1]])*.5f;var foot=(v[bots[0]]+v[bots[1]])*.5f;var o=foot-top;o.y=0;if(o.sqrMagnitude<1e-6f){keep.AddRange(t.Skip(i).Take(6));continue;}o.Normalize();
    float ey=top.y+.05f;bool inside=Drive(top+o*.6f,ey)&&Drive(top+o*1.2f,ey);
    if(inside){removed++;if(listed.Count<300)listed.Add($"strip under the road at ({top.x:F1}, {ey:F2}, {top.z:F1}): removed");}else keep.AddRange(t.Skip(i).Take(6));}
   notes.Add($"BUG-003: embankment strips {t.Length/6}; under a continuing driving surface (removed) {removed}; kept {keep.Count/6}");notes.AddRange(listed);
   if(!dry&&removed>0){var m=Object.Instantiate(mesh);m.name=mesh.name;m.SetTriangles(keep,0);m.RecalculateNormals();m.RecalculateBounds();EditorUtility.CopySerialized(m,mesh);Object.DestroyImmediate(m);EditorUtility.SetDirty(mesh);AssetDatabase.SaveAssets();notes.Add("saved "+Asset);}
  }catch(Exception e){notes.Add("ERROR "+e);}
  finally{Directory.CreateDirectory("Docs/Report081");File.WriteAllLines("Docs/Report081/BUG-003-embankment.txt",notes);}
  EditorApplication.Exit(0);}
}
