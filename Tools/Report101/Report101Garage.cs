using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// Temporary 0.101 probe: per scene, the triangles of every renderer inside Dan's garage volume, the fence segments and beige paving around it, and paving spikes.
public static class Report101Garage {
 static string Out=>Environment.GetEnvironmentVariable("PROBE_OUT");
 static string P(Transform t){var s=t.name;while(t.parent){t=t.parent;s=t.name+"/"+s;}return s;}
 static string V(Vector3 v)=>$"({v.x:F2},{v.y:F2},{v.z:F2})";
 public static readonly Vector3 C=new(398.89f,79.32f,24.51f);public const float Yaw=91.67f;
 public static bool InGarage(Vector3 w,float pad=1.2f){var l=Quaternion.Inverse(Quaternion.Euler(0,Yaw,0))*(w-C);return Mathf.Abs(l.x)<=8.5f+pad&&Mathf.Abs(l.z)<=5f+pad&&l.y>-1.5f&&l.y<8f;}
 public static void Run(){
  var log=new List<string>();
  foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){
   EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();log.Add("===== "+scene);
   var g=GameObject.Find("Separate downhill garage kennel");log.Add(g?$"garage {P(g.transform)} pos {V(g.transform.position)} rot {V(g.transform.eulerAngles)}":"garage NOT FOUND");
   var area=new Bounds(C,new Vector3(30,20,30));
   foreach(var r in UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(r=>r.bounds.Intersects(area))){
    var mf=r.GetComponent<MeshFilter>();if(!mf||!mf.sharedMesh)continue;var m=mf.sharedMesh;if(!m.isReadable){log.Add($"  unreadable {P(r.transform)} {m.name}");continue;}
    var v=m.vertices;var M=r.transform.localToWorldMatrix;
    for(int sm=0;sm<m.subMeshCount;sm++){var t=m.GetTriangles(sm);int n=0;var mn=Vector3.one*1e9f;var mx=-mn;
     for(int i=0;i<t.Length;i+=3){var a=M.MultiplyPoint3x4(v[t[i]]);var b=M.MultiplyPoint3x4(v[t[i+1]]);var c=M.MultiplyPoint3x4(v[t[i+2]]);var ce=(a+b+c)/3;if(!InGarage(ce))continue;n++;mn=Vector3.Min(mn,Vector3.Min(a,Vector3.Min(b,c)));mx=Vector3.Max(mx,Vector3.Max(a,Vector3.Max(b,c)));}
     if(n>0)log.Add($"  TRI {n} in {P(r.transform)} sub {sm} mat {(sm<r.sharedMaterials.Length&&r.sharedMaterials[sm]?r.sharedMaterials[sm].name:"?")} mesh {m.name} {AssetDatabase.GetAssetPath(m)} tris {t.Length/3} range {V(mn)}-{V(mx)}");}}
   foreach(var c in UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(c=>InGarage(c.bounds.center,2)))log.Add($"  COL {P(c.transform)} {c.GetType().Name} b {V(c.bounds.min)}-{V(c.bounds.max)}");
   // fences in the yard
   var fbox=new Bounds(new Vector3(415,80,10),new Vector3(60,20,60));
   foreach(var r in UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(r=>r.name.Contains("crossbuck")&&fbox.Contains(r.bounds.center)).OrderBy(r=>r.bounds.center.x+r.bounds.center.z*.01f))
    {var mf=r.GetComponent<MeshFilter>();log.Add($"  FENCE {P(r.transform)} b {V(r.bounds.min)}-{V(r.bounds.max)} mesh {(mf&&mf.sharedMesh?mf.sharedMesh.name+" "+AssetDatabase.GetAssetPath(mf.sharedMesh):"-")} kids {r.transform.childCount} col {(r.GetComponent<BoxCollider>()?V(r.GetComponent<BoxCollider>().bounds.size):"-")}");
     foreach(Transform k in r.transform)log.Add($"     kid {k.name} {V(k.position)} {(k.GetComponent<Renderer>()?"R":"")}{(k.GetComponent<Collider>()?"C":"")}");}
   // beige paving
   foreach(var r in UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(r=>r.name.Contains("beige")||r.name.Contains("paved-to-dirt"))){
    var mf=r.GetComponent<MeshFilter>();log.Add($"  PAVE {P(r.transform)} {mf.sharedMesh.name} {AssetDatabase.GetAssetPath(mf.sharedMesh)} v{mf.sharedMesh.vertexCount} b {V(r.bounds.min)}-{V(r.bounds.max)} col {(r.GetComponent<MeshCollider>()&&r.GetComponent<MeshCollider>().sharedMesh?r.GetComponent<MeshCollider>().sharedMesh.name:"-")}");
    if(!mf.sharedMesh.isReadable)continue;var v=mf.sharedMesh.vertices;var M=r.transform.localToWorldMatrix;
    // spikes: a vertex well above the terrain at its spot (terrain = Ground_ colliders other than this one)
    for(int i=0;i<v.Length;i++){var w=M.MultiplyPoint3x4(v[i]);if(w.x<392||w.x>416||w.z<0||w.z>36)continue;float tg=float.NaN;foreach(var h in Physics.RaycastAll(new Vector3(w.x,w.y+30,w.z),Vector3.down,80,~0,QueryTriggerInteraction.Ignore))if(h.collider.name.StartsWith("Ground_")&&!h.collider.name.Contains("beige")&&(float.IsNaN(tg)||h.point.y>tg))tg=h.point.y;
     if(float.IsNaN(tg)||w.y-tg>.35f||w.y-tg<-.6f)log.Add($"     vertex {i} {V(w)} terrain {tg:F2} d {w.y-tg:F2}");}}
  }
  Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/garage.txt",log);EditorApplication.Exit(0);}
}
