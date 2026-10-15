using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 0.118 (read-only, temporary; scenes never saved): what stands on Kyle's bank (Leviathan's driveway link) in MountainLoop / MountainLoopReverse.
// PROBE_SCENES "MountainLoop;MountainLoopReverse" -> bank-<scene>.txt: every collider / renderer whose bounds meet the box, the ground meshes'
// vertices inside it (local spacing), and a 1 m height grid (raycast top ground) -> bankgrid-<scene>.txt
public static class Report118Bank {
 static string P(Transform t){var s=t.name;while(t.parent){t=t.parent;s=t.name+"/"+s;}return s;}
 static string V(Vector3 v)=>FormattableString.Invariant($"({v.x:F2},{v.y:F2},{v.z:F2})");
 public static void Run(){
  var o=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(o);
  var box=new Bounds();box.SetMinMax(new Vector3(490,60,-40),new Vector3(540,110,8));
  foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(';')){
   var log=new List<string>();
   try{
    EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();
    foreach(var c in UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(c=>c.bounds.Intersects(box)).OrderBy(c=>P(c.transform)))
     log.Add($"COL {c.GetType().Name} {(c.enabled&&c.gameObject.activeInHierarchy?"on":"off")} {P(c.transform)} bounds {V(c.bounds.min)}-{V(c.bounds.max)}"+(c is MeshCollider mc&&mc.sharedMesh?$" mesh {mc.sharedMesh.name} v{mc.sharedMesh.vertexCount} asset '{AssetDatabase.GetAssetPath(mc.sharedMesh)}'":""));
    foreach(var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(r=>r.bounds.Intersects(box)).OrderBy(r=>P(r.transform))){
     var mf=r.GetComponent<MeshFilter>();log.Add($"REN {(r.enabled&&r.gameObject.activeInHierarchy?"on":"off")} {P(r.transform)} pos {V(r.transform.position)} bounds {V(r.bounds.min)}-{V(r.bounds.max)}"+(mf&&mf.sharedMesh?$" mesh {mf.sharedMesh.name} v{mf.sharedMesh.vertexCount} asset '{AssetDatabase.GetAssetPath(mf.sharedMesh)}' readable {mf.sharedMesh.isReadable}":"")+$" mat {string.Join(",",r.sharedMaterials.Select(m=>m?m.name:"null"))}");}
    // ground meshes: vertices inside the box
    foreach(var mc in UnityEngine.Object.FindObjectsByType<MeshCollider>(FindObjectsSortMode.None).Where(c=>c.name.StartsWith("Ground_")&&c.bounds.Intersects(box))){
     var m=mc.sharedMesh;if(!m)continue;var vs=m.vertices;var t=mc.transform;int n=0;var xs=new SortedSet<float>();var zs=new SortedSet<float>();
     for(int i=0;i<vs.Length;i++){var w=t.TransformPoint(vs[i]);if(w.x>=box.min.x&&w.x<=box.max.x&&w.z>=box.min.z&&w.z<=box.max.z){n++;xs.Add(Mathf.Round(w.x*100)/100);zs.Add(Mathf.Round(w.z*100)/100);}}
     log.Add($"GROUND {P(t)} pos {V(t.position)} rot {V(t.eulerAngles)} scale {V(t.lossyScale)} verts in box {n} of {vs.Length}; xs {string.Join(" ",xs.Take(40))} | zs {string.Join(" ",zs.Take(40))}");
     var mf=t.GetComponent<MeshFilter>();log.Add($"   render mesh same as collider: {(mf&&mf.sharedMesh==m)} tris {m.triangles.Length/3} subMeshes {m.subMeshCount} uv {m.uv.Length} colors {m.colors.Length}");}
    var g=new List<string>();
    for(int z=-40;z<=8;z++)for(int x=490;x<=540;x++){
     var hits=Physics.RaycastAll(new Vector3(x,300,z),Vector3.down,400,~0,QueryTriggerInteraction.Ignore).Where(h=>!h.collider.attachedRigidbody&&!h.collider.isTrigger).OrderBy(h=>h.distance).ToArray();
     var gr=hits.Where(h=>h.collider.name.StartsWith("Ground_")).Select(h=>h.point.y).DefaultIfEmpty(float.NaN).First();
     var top=hits.Length>0?hits[0]:default;g.Add(FormattableString.Invariant($"{x} {z} {gr:F3} {(hits.Length>0?top.point.y:float.NaN):F3} {(hits.Length>0?top.collider.name.Replace(' ','_'):"-")}"));}
    File.WriteAllLines(Path.Combine(o,"bankgrid-"+scene+".txt"),g);
   }catch(Exception e){log.Add("ERROR "+e);}
   File.WriteAllLines(Path.Combine(o,"bank-"+scene+".txt"),log);}
  EditorApplication.Exit(0);}
}
