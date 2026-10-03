using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Object=UnityEngine.Object;
// 0.71 read-only: top surface along lines. PROBE_LINES="scene|x1,z1|x2,z2|step;..." -> y, normal, collider, mesh asset per sample.
public static class Report071Line {
 static string Path(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n.Replace('\n','/');}
 public static void Run(){var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(outDir);var rows=new List<string>();string open=null;
  foreach(var spec in Environment.GetEnvironmentVariable("PROBE_LINES").Split(';')){var a=spec.Split('|');if(a[0]!=open){EditorSceneManager.OpenScene("Assets/Scenes/"+a[0]+".unity");open=a[0];Physics.SyncTransforms();}
   var p=a[1].Split(',').Select(float.Parse).ToArray();var q=a[2].Split(',').Select(float.Parse).ToArray();float step=float.Parse(a[3]);
   var A=new Vector2(p[0],p[1]);var B=new Vector2(q[0],q[1]);float len=(B-A).magnitude;rows.Add($"LINE {a[0]} {A}->{B} len {len:F1}");
   for(float d=0;d<=len+.01f;d+=step){var c=Vector2.Lerp(A,B,d/len);var o=new Vector3(c.x,600,c.y);var hits=new List<string>();
    for(int k=0;k<6&&Physics.Raycast(o,Vector3.down,out var h,1200,~0,QueryTriggerInteraction.Ignore);k++){var mf=h.collider.GetComponent<MeshFilter>();var mc=h.collider as MeshCollider;
     hits.Add($"{h.point.y:F2} n{h.normal.y:F2} {Path(h.collider.transform)} [{(mc&&mc.sharedMesh?AssetDatabase.GetAssetPath(mc.sharedMesh)+"#"+h.triangleIndex:"")}]");o=h.point+Vector3.down*.05f;}
    rows.Add($"{d,6:F1} ({c.x:F1},{c.y:F1}) "+string.Join(" || ",hits));}}
  File.WriteAllLines(outDir+"/lines.txt",rows);EditorApplication.Exit(0);}
}
