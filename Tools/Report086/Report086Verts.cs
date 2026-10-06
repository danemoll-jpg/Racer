using System;using System.IO;using System.Linq;using System.Text;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;
// 0.86 (read only): every renderer vertex inside a box VBOX="x0,y0,z0,x1,y1,z1" (world), per renderer, with its triangle count there.
public static class Report086Verts {
 static string P(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n;}
 public static void Run(){var o=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(o);var f=Environment.GetEnvironmentVariable("VBOX").Split(',').Select(x=>float.Parse(x,System.Globalization.CultureInfo.InvariantCulture)).ToArray();
  var box=new Bounds();box.SetMinMax(new Vector3(f[0],f[1],f[2]),new Vector3(f[3],f[4],f[5]));var sb=new StringBuilder();
  foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){var s=EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");sb.AppendLine("== "+scene);
   foreach(var r in s.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<MeshRenderer>(true))){if(!r.bounds.Intersects(box))continue;var mf=r.GetComponent<MeshFilter>();if(!mf||!mf.sharedMesh||!mf.sharedMesh.isReadable)continue;var m=mf.sharedMesh;
    var w=m.vertices.Select(v=>r.transform.TransformPoint(v)).ToArray();var inside=Enumerable.Range(0,w.Length).Where(i=>box.Contains(w[i])).ToList();if(inside.Count==0)continue;
    var set=inside.ToHashSet();var tris=m.triangles;var tv=new System.Collections.Generic.HashSet<int>();for(int k=0;k<tris.Length;k+=3)if(set.Contains(tris[k])||set.Contains(tris[k+1])||set.Contains(tris[k+2])){tv.Add(tris[k]);tv.Add(tris[k+1]);tv.Add(tris[k+2]);}
    if(tv.Count==0){sb.AppendLine($"  {P(r.transform)} {AssetDatabase.GetAssetPath(m)}: {inside.Count} vertices in the box, used by NO triangle (not drawn)");continue;}var b=new Bounds(w[tv.First()],Vector3.zero);foreach(var i in tv)b.Encapsulate(w[i]);
    sb.AppendLine($"  {P(r.transform)} {AssetDatabase.GetAssetPath(m)} on {r.enabled&&r.gameObject.activeInHierarchy}: {inside.Count} vertices in the box; their triangles span {b.min.x:F2},{b.min.y:F2},{b.min.z:F2} .. {b.max.x:F2},{b.max.y:F2},{b.max.z:F2} ({tv.Count} vertices)");}}
  File.WriteAllText(o+"/verts.txt",sb.ToString());EditorApplication.Exit(0);}
}
