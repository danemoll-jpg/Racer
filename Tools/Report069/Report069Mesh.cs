using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Object=UnityEngine.Object;
// 0.69 read-only: world-space triangles of every collider/renderer mesh whose name matches PROBE_MESH inside boxes
// PROBE_BOX="scene|x,z|radius;..." -> tris-<scene>-<x>_<z>.txt (name;ax,ay,az;bx,by,bz;cx,cy,cz). Plotted outside Unity.
public static class Report069Mesh {
 public static void Run(){var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(outDir);var pat=(Environment.GetEnvironmentVariable("PROBE_MESH")??"driving surface").Split(',');
  foreach(var g in Environment.GetEnvironmentVariable("PROBE_BOX").Split(';').Select(x=>x.Split('|')).GroupBy(a=>a[0])){EditorSceneManager.OpenScene("Assets/Scenes/"+g.Key+".unity");
   var mfs=Object.FindObjectsByType<MeshFilter>().Where(m=>m.sharedMesh&&pat.Any(p=>m.name.Contains(p))).ToArray();
   foreach(var a in g){var c=a[1].Split(',').Select(float.Parse).ToArray();float r=float.Parse(a[2]);var rows=new List<string>();
    foreach(var mf in mfs){var b=mf.GetComponent<Renderer>()?mf.GetComponent<Renderer>().bounds:new Bounds(mf.transform.position,Vector3.one*1e5f);if(b.max.x<c[0]-r||b.min.x>c[0]+r||b.max.z<c[1]-r||b.min.z>c[1]+r)continue;
     var v=mf.sharedMesh.vertices.Select(x=>mf.transform.TransformPoint(x)).ToArray();var t=mf.sharedMesh.triangles;
     for(int i=0;i<t.Length;i+=3){var p=v[t[i]];var q=v[t[i+1]];var s=v[t[i+2]];var m=(p+q+s)/3;if(Mathf.Abs(m.x-c[0])>r||Mathf.Abs(m.z-c[1])>r)continue;
      rows.Add($"{mf.name};{p.x:F3},{p.y:F3},{p.z:F3};{q.x:F3},{q.y:F3},{q.z:F3};{s.x:F3},{s.y:F3},{s.z:F3}");}}
    File.WriteAllLines($"{outDir}/tris-{g.Key}-{c[0]:F0}_{c[1]:F0}.txt",rows);}}
  EditorApplication.Exit(0);}
}
