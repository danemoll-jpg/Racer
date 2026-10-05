using System;using System.IO;using System.Linq;using System.Text;using System.Collections.Generic;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;
// Temporary 0.79 census (copied into Assets/Editor/Report079Temp while it runs): for each scene in PROBE_SCENES (comma
// list) writes <scene>-renderers.tsv (every MeshRenderer: path, mesh, vertices, material, shader, bounds, collider on the
// same object) and <scene>-colliders.tsv (every collider: path, type, enabled, trigger, layer, world bounds), so the
// scenery can be classified and the colliders compared before / after.
public static class Report079Census {
 static string Path(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n;}
 static string V(Vector3 v)=>$"{v.x:F2},{v.y:F2},{v.z:F2}";
 public static void Run(){var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(outDir);
  foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){
   var s=EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");var roots=s.GetRootGameObjects();
   var r=new StringBuilder("path\tactive\tmesh\tverts\tmaterial\tshader\tcenter\tsize\tcollider\n");
   foreach(var mr in roots.SelectMany(g=>g.GetComponentsInChildren<MeshRenderer>(true))){var mf=mr.GetComponent<MeshFilter>();var m=mf?mf.sharedMesh:null;var mat=mr.sharedMaterial;
    r.Append($"{Path(mr.transform)}\t{mr.gameObject.activeInHierarchy&&mr.enabled}\t{(m?m.name:"")}\t{(m?m.vertexCount:0)}\t{(mat?mat.name:"")}\t{(mat&&mat.shader?mat.shader.name:"")}\t{V(mr.bounds.center)}\t{V(mr.bounds.size)}\t{(mr.GetComponent<Collider>()?mr.GetComponent<Collider>().GetType().Name:"")}\n");}
   File.WriteAllText(outDir+"/"+scene+"-renderers.tsv",r.ToString());
   var c=new StringBuilder("path\ttype\tenabled\ttrigger\tlayer\tcenter\tsize\n");
   foreach(var col in roots.SelectMany(g=>g.GetComponentsInChildren<Collider>(true)).OrderBy(x=>Path(x.transform)))
    c.Append($"{Path(col.transform)}\t{col.GetType().Name}\t{col.enabled&&col.gameObject.activeInHierarchy}\t{col.isTrigger}\t{col.gameObject.layer}\t{V(col.bounds.center)}\t{V(col.bounds.size)}\n");
   File.WriteAllText(outDir+"/"+scene+"-colliders.tsv",c.ToString());
   Debug.Log("REPORT079 census "+scene);}
  EditorApplication.Exit(0);}
}
