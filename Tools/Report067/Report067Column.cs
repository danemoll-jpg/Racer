using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Object=UnityEngine.Object;
// Read-only column debug: every collider surface and every renderer bounds column at given XZ points.
// PROBE_COLUMNS="scene:x,z;x,z|scene2:x,z"
public static class Report067Column {
 public static void Run(){var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");var rows=new List<string>();
  foreach(var job in Environment.GetEnvironmentVariable("PROBE_COLUMNS").Split('|')){var parts=job.Split(':');EditorSceneManager.OpenScene("Assets/Scenes/"+parts[0]+".unity");Physics.SyncTransforms();Physics.queriesHitBackfaces=true;rows.Add("SCENE "+parts[0]);
   var rends=Object.FindObjectsByType<MeshRenderer>();
   foreach(var xz in parts[1].Split(';')){var v=xz.Split(',').Select(float.Parse).ToArray();rows.Add($"COLUMN {v[0]},{v[1]}");
    var o=new Vector3(v[0],400,v[1]);for(int k=0;k<40&&Physics.Raycast(o,Vector3.down,out var h,600,~0,QueryTriggerInteraction.Collide);k++){rows.Add($"  hit {h.point.y:F2} n{h.normal.y:F2} {h.collider.name} ({h.collider.GetType().Name} trig={h.collider.isTrigger} rb={(bool)h.collider.attachedRigidbody} enabled={h.collider.enabled})");o=h.point+Vector3.down*.02f;}
    // Render-only geometry: every renderer triangle covering this XZ, with its height there.
    foreach(var r in rends.Where(r=>r.bounds.min.x<=v[0]&&r.bounds.max.x>=v[0]&&r.bounds.min.z<=v[1]&&r.bounds.max.z>=v[1])){var mf=r.GetComponent<MeshFilter>();if(!mf||!mf.sharedMesh||!mf.sharedMesh.isReadable)continue;var m=mf.sharedMesh;var vs=m.vertices;
     for(int sm=0;sm<m.subMeshCount;sm++){var t=m.GetTriangles(sm);for(int i=0;i<t.Length;i+=3){var a=mf.transform.TransformPoint(vs[t[i]]);var b=mf.transform.TransformPoint(vs[t[i+1]]);var c=mf.transform.TransformPoint(vs[t[i+2]]);
      float d=(b.z-c.z)*(a.x-c.x)+(c.x-b.x)*(a.z-c.z);if(Mathf.Abs(d)<1e-6f)continue;float l1=((b.z-c.z)*(v[0]-c.x)+(c.x-b.x)*(v[1]-c.z))/d,l2=((c.z-a.z)*(v[0]-c.x)+(a.x-c.x)*(v[1]-c.z))/d,l3=1-l1-l2;if(l1<0||l2<0||l3<0)continue;
      float y=l1*a.y+l2*b.y+l3*c.y;var n=Vector3.Cross(b-a,c-a).normalized;rows.Add($"  tri {y:F2} n{n.y:F2} {r.name} tri#{i/3} v=[{a.ToString("F2")} {b.ToString("F2")} {c.ToString("F2")}] sub{sm} mat={(sm<r.sharedMaterials.Length&&r.sharedMaterials[sm]?r.sharedMaterials[sm].name:"-")} mesh={AssetDatabase.GetAssetPath(m)}");}}}
    foreach(var r in rends.Where(r=>r.bounds.min.x<=v[0]&&r.bounds.max.x>=v[0]&&r.bounds.min.z<=v[1]&&r.bounds.max.z>=v[1]&&r.bounds.size.y<200))rows.Add($"  rend {r.name} y=[{r.bounds.min.y:F1},{r.bounds.max.y:F1}] col={(r.GetComponent<Collider>()?r.GetComponent<Collider>().GetType().Name+(r.GetComponent<Collider>().enabled?"":"(off)"):"none")} mesh={(r.GetComponent<MeshFilter>()&&r.GetComponent<MeshFilter>().sharedMesh?AssetDatabase.GetAssetPath(r.GetComponent<MeshFilter>().sharedMesh):"")}");}}
  File.WriteAllLines(outDir+"/columns.txt",rows);EditorApplication.Exit(0);}
}
