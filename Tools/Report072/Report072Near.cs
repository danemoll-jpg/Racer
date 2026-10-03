using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Object=UnityEngine.Object;
// 0.72 read-only: every mesh (renderer or collider) with a triangle within r (horizontal) of a point, with the height range of
// those triangles; then a column scan (all collider hits, front and back faces) on a grid around the point.
// PROBE_NEAR="scene|x,y,z|r|grid;..."
public static class Report072Near {
 static string Path(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n.Replace('\n','/');}
 public static void Run(){var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(outDir);var rows=new List<string>();string open=null;
  foreach(var spec in Environment.GetEnvironmentVariable("PROBE_NEAR").Split(';')){var a=spec.Split('|');if(a[0]!=open){EditorSceneManager.OpenScene("Assets/Scenes/"+a[0]+".unity");open=a[0];Physics.SyncTransforms();}
   var v=a[1].Split(',').Select(float.Parse).ToArray();var p=new Vector3(v[0],v[1],v[2]);float r=float.Parse(a[2]);float g=a.Length>3?float.Parse(a[3]):0;rows.Add($"NEAR {a[0]} {p} r {r}");
   foreach(var mf in Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include,FindObjectsSortMode.None)){var m=mf.sharedMesh;if(!m||!m.isReadable)continue;var rend=mf.GetComponent<Renderer>();var col=mf.GetComponent<Collider>();
    var bb=rend?rend.bounds:(col?col.bounds:new Bounds(mf.transform.position,Vector3.zero));var flat=new Bounds(new Vector3(bb.center.x,p.y,bb.center.z),new Vector3(bb.size.x,1,bb.size.z));if(Mathf.Sqrt(flat.SqrDistance(p))>r)continue;
    var vs=m.vertices;var tr=mf.transform.localToWorldMatrix;int n=0;float lo=1e9f,hi=-1e9f;var tris=m.triangles;
    for(int i=0;i<tris.Length;i+=3){var A=tr.MultiplyPoint3x4(vs[tris[i]]);var B=tr.MultiplyPoint3x4(vs[tris[i+1]]);var C=tr.MultiplyPoint3x4(vs[tris[i+2]]);var c=(A+B+C)/3;
     if(new Vector2(c.x-p.x,c.z-p.z).magnitude>r)continue;n++;lo=Mathf.Min(lo,Mathf.Min(A.y,Mathf.Min(B.y,C.y)));hi=Mathf.Max(hi,Mathf.Max(A.y,Mathf.Max(B.y,C.y)));}
    if(n==0)continue;var mc=mf.GetComponent<MeshCollider>();
    rows.Add($"{(mf.gameObject.activeInHierarchy?"A":"-")}{(rend&&rend.enabled?"R":"-")}{(col&&col.enabled?"C":"-")}{(mc&&mc.sharedMesh!=m?"*":"")} tris {n,5} y {lo,7:F2}..{hi,7:F2} {Path(mf.transform)} | {AssetDatabase.GetAssetPath(m)}:{m.name} | mat {(rend?string.Join(",",rend.sharedMaterials.Select(x=>x?x.name:"null")):"")}");}
   if(g>0){Physics.queriesHitBackfaces=true;for(float dz=-r;dz<=r+.01f;dz+=g)for(float dx=-r;dx<=r+.01f;dx+=g){var o=new Vector3(p.x+dx,p.y+40,p.z+dz);
     var hits=Physics.RaycastAll(o,Vector3.down,80,~0,QueryTriggerInteraction.Ignore).Where(h=>!h.collider.attachedRigidbody).OrderBy(h=>h.distance).ToArray();
     rows.Add($"  col {o.x:F1},{o.z:F1}: "+string.Join(" | ",hits.Select(h=>$"{h.point.y:F2} n{h.normal.y:F2} {h.collider.name}")));}Physics.queriesHitBackfaces=false;}}
  File.WriteAllLines(outDir+"/near.txt",rows);EditorApplication.Exit(0);}
}
