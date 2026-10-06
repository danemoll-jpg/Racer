using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Object=UnityEngine.Object;
// 0.83 Parts A/B: Mountain Loop Forward driving surface (Assets/Track/ReportCleanup/forward-bend-final.asset, used only by
// MountainLoop, visual and collider). Only vertex heights change; no vertex moves sideways, no triangle is added or removed.
//  A (BUG-001, 1253.0, 152.5, 226.3): on the summit deck pairs of vertices a few cm apart sit 1-3 cm apart in height, so the
//    triangles between them are slivers tilted up to 26 degrees. VehicleSurfaceContacts gives a body contact on a face that
//    face's own normal, so a bike touching a sliver is pushed sideways and up (normal 0.26/0.96, impulse 349): it flips.
//    Within 45 m of the spot each such cluster (within 0.15 m across, under 6 cm apart in height) gets one height, its mean.
//  B (BUG-002, 989.1, 132.0, -115.6): where the north road piece meets the south piece its first row is up to 10.5 cm lower
//    at the inner (east) end, leaving a 10 cm wall across the road and three tilted fins at the inner corner (the climbing piece meets both at one point, 20 cm higher). The north
//    piece's first row is closed onto the south piece's edge, its next three rows' east ends raised 7.9, 5.3 and 2.6 cm, the
//    fins put level with the road.
// AUTHOR_DRY=1 measures only. Writes Docs/Report083/Lists/AB-surface.txt.
public static class Report083Author {
 const string Asset="Assets/Track/ReportCleanup/forward-bend-final.asset";
 static readonly List<string> notes=new();static void Note(string s){notes.Add(s);Debug.Log("REPORT083 "+s);}
 public static void Run(){bool dry=Environment.GetEnvironmentVariable("AUTHOR_DRY")=="1";
  try{EditorSceneManager.OpenScene("Assets/Scenes/MountainLoop.unity");
   var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(Asset);var mf=Object.FindObjectsByType<MeshFilter>().Single(m=>m.sharedMesh==mesh);var tr=mf.transform;
   var local=mesh.vertices;var w=local.Select(x=>tr.TransformPoint(x)).ToArray();var t=mesh.triangles;var before=(Vector3[])w.Clone();
   Note($"{Asset}: {w.Length} vertices, {t.Length/3} triangles; object {mf.name}");
   // ---- A
   var a=new Vector3(1256,0,231);var near=Enumerable.Range(0,w.Length).Where(i=>Flat(w[i]-a).magnitude<45).ToList();var done=new HashSet<int>();int clusters=0,moved=0;float most=0;
   foreach(int i in near){if(done.Contains(i))continue;var c=near.Where(j=>!done.Contains(j)&&Flat(w[j]-w[i]).magnitude<.15f&&Mathf.Abs(w[j].y-w[i].y)<.06f).ToList();foreach(var j in c)done.Add(j);
    if(c.Count<2)continue;float lo=c.Min(j=>w[j].y),hi=c.Max(j=>w[j].y);if(hi-lo<.0005f)continue;float y=c.Average(j=>w[j].y);clusters++;
    foreach(var j in c){most=Mathf.Max(most,Mathf.Abs(w[j].y-y));w[j].y=y;moved++;}}
   Note($"A: {clusters} vertex clusters within 45 m given one height ({moved} vertices, largest change {most*100:F1} cm)");
   Tilts("A before",before,t,a,45);Tilts("A after",w,t,a,45);
   // ---- B
   void Set(Vector3 at,float dy,bool absolute=false){int k=Enumerable.Range(0,w.Length).OrderBy(i=>(w[i]-at).sqrMagnitude).First();if((w[k]-at).magnitude>.02f)throw new Exception($"B: no vertex at {at}");
    float y=absolute?dy:w[k].y+dy;Note($"B: vertex {k} at ({w[k].x:F3}, {w[k].z:F3}) {w[k].y:F3} -> {y:F3}");w[k].y=y;}
   // The crack: the south piece's top edge runs to vertex 3681 (990.805, -117.637), the north piece's bottom edge to 3682
   // (990.993, -117.639), 19 cm east and 10.5 cm lower, with no triangle between the two edges: a wedge-shaped gap from
   // nothing at the outer edge to 19 cm at the inner edge, the ground showing through. 3682 is put on 3681.
   {int k=Enumerable.Range(0,w.Length).OrderBy(i=>(w[i]-new Vector3(990.993f,131.404f,-117.639f)).sqrMagnitude).First();int s1=Enumerable.Range(0,w.Length).OrderBy(i=>(w[i]-new Vector3(990.805f,131.509f,-117.637f)).sqrMagnitude).First();
    Note($"B: vertex {k} ({w[k].x:F3}, {w[k].y:F3}, {w[k].z:F3}) put on vertex {s1} ({w[s1].x:F3}, {w[s1].y:F3}, {w[s1].z:F3})");w[k]=w[s1];}Set(new Vector3(991.000f,131.426f,-117.072f),.07875f);Set(new Vector3(991.006f,131.453f,-116.825f),.0525f);Set(new Vector3(991.077f,131.490f,-116.442f),.02625f);
   foreach(var p in new[]{new Vector3(991.003f,131.384f,-117.703f),new Vector3(991.005f,131.384f,-117.712f),new Vector3(990.958f,131.383f,-117.652f)})Set(p,131.509f,true);
   if(Environment.GetEnvironmentVariable("AUTHOR_DUMP")=="1"){var k0=new Vector3(990.95f,0,-117.75f);for(int i=0;i<t.Length;i+=3){if(!new[]{t[i],t[i+1],t[i+2]}.Any(j=>Flat(w[j]-k0).magnitude<1.3f))continue;var nn=Vector3.Cross(w[t[i+1]]-w[t[i]],w[t[i+2]]-w[t[i]]).normalized;Note($"  knot tri {i/3} n.y {Mathf.Abs(nn.y):F3}: "+string.Join(" | ",new[]{t[i],t[i+1],t[i+2]}.Select(j=>$"{j} ({w[j].x:F3}, {w[j].y:F3}, {w[j].z:F3})")));}}
   var b=new Vector3(986,0,-118);Tilts("B before",before,t,b,8);Tilts("B after",w,t,b,8);
   int changed=Enumerable.Range(0,w.Length).Count(i=>w[i]!=before[i]);float sideways=Enumerable.Range(0,w.Length).Max(i=>Flat(w[i]-before[i]).magnitude);
   Note($"vertices changed {changed}, largest sideways movement {sideways:F3} m");
   if(!dry){var nl=w.Select(x=>tr.InverseTransformPoint(x)).ToArray();for(int i=0;i<nl.Length;i++){if(w[i]==before[i])nl[i]=local[i];}
    mesh.vertices=nl;mesh.RecalculateNormals();mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);AssetDatabase.SaveAssets();Note("saved "+Asset);}
  }catch(Exception e){Note("ERROR "+e);}
  finally{Directory.CreateDirectory("Docs/Report083/Lists");File.WriteAllLines("Docs/Report083/Lists/AB-surface.txt",notes);}
  EditorApplication.Exit(0);}
 static Vector3 Flat(Vector3 v){v.y=0;return v;}
 static void Tilts(string tag,Vector3[] w,int[] t,Vector3 c,float r){int n=0;float worst=1;
  for(int i=0;i<t.Length;i+=3){var p=w[t[i]];var q=w[t[i+1]];var s=w[t[i+2]];if(Flat((p+q+s)/3-c).magnitude>r)continue;var nn=Vector3.Cross(q-p,s-p).normalized;if(nn.y<0)nn=-nn;if(nn.y<.99f){n++;worst=Mathf.Min(worst,nn.y);if(tag.EndsWith("after"))Note($"   tilted face at ({((p+q+s)/3).x:F2}, {((p+q+s)/3).y:F2}, {((p+q+s)/3).z:F2}) normal y {nn.y:F3}, heights {p.y:F3} {q.y:F3} {s.y:F3}");}}
  Note($"{tag}: faces within {r} m tilted more than 8 degrees: {n}, steepest normal y {worst:F3}");}
}
