using System;using System.IO;using System.Linq;using System.Text;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;
// 0.85 (read only): every collider and renderer within r of points in a scene, plus the down-ray stack at each point.
// PROBE_SCENES=scene, NEAR="x,y,z,r|x,y,z,r"
public static class Report085Near {
 static string P(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n;}
 static string V(Vector3 v)=>$"{v.x:F2},{v.y:F2},{v.z:F2}";
 public static void Run(){var o=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(o);
  foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){var s=EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();var sb=new StringBuilder();
   var roots=s.GetRootGameObjects();
   foreach(var spec in Environment.GetEnvironmentVariable("NEAR").Split('|')){var f=spec.Split(',').Select(x=>float.Parse(x,System.Globalization.CultureInfo.InvariantCulture)).ToArray();var p=new Vector3(f[0],f[1],f[2]);float r=f[3];
    sb.AppendLine($"== {scene} point {V(p)} r {r}");
    foreach(var c in roots.SelectMany(g=>g.GetComponentsInChildren<Collider>(true))){var b=c.bounds;if(b.SqrDistance(p)>r*r||b.size.x>600||b.size.z>600)continue;
     string extra=c is BoxCollider bc?$" box c {V(bc.center)} s {V(bc.size)} pos {V(c.transform.position)} rot {V(c.transform.eulerAngles)} scale {V(c.transform.lossyScale)}":c is MeshCollider mc&&mc.sharedMesh?$" mesh {mc.sharedMesh.name} v{mc.sharedMesh.vertexCount} asset {AssetDatabase.GetAssetPath(mc.sharedMesh)}":"";
     sb.AppendLine($"  C {P(c.transform)} [{c.GetType().Name}] en {c.enabled&&c.gameObject.activeInHierarchy} trig {c.isTrigger} b {V(b.center)} s {V(b.size)}{extra}");}
    foreach(var rr in roots.SelectMany(g=>g.GetComponentsInChildren<Renderer>(true))){var b=rr.bounds;if(b.SqrDistance(p)>r*r||b.size.x>600||b.size.z>600)continue;var mf=rr.GetComponent<MeshFilter>();
     sb.AppendLine($"  R {P(rr.transform)} on {rr.enabled&&rr.gameObject.activeInHierarchy} b {V(b.center)} s {V(b.size)} mesh {(mf&&mf.sharedMesh?mf.sharedMesh.name+" v"+mf.sharedMesh.vertexCount+" "+AssetDatabase.GetAssetPath(mf.sharedMesh):"-")} mat {(rr.sharedMaterial?rr.sharedMaterial.name:"-")}");}
    foreach(var h in Physics.RaycastAll(p+Vector3.up*40,Vector3.down,120,~0,QueryTriggerInteraction.Collide).OrderBy(h=>h.distance))sb.AppendLine($"  down-hit {P(h.collider.transform)} y {h.point.y:F2} n {V(h.normal)} trig {h.collider.isTrigger}");}
   File.WriteAllText(o+"/near-"+scene+".txt",sb.ToString());}
  EditorApplication.Exit(0);}
}
