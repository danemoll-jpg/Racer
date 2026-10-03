using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Object=UnityEngine.Object;
// 0.72 read-only: top surfaces along the Summit Homeward flight line (launch frame of CR094 summit launch). PROBE_LINE="s0,s1,step,x;x;..."
public static class Report072Line {
 public static void Run(){var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(outDir);var rows=new List<string>();var a=Environment.GetEnvironmentVariable("PROBE_LINE").Split(',');float s0=float.Parse(a[0]),s1=float.Parse(a[1]),st=float.Parse(a[2]);var xs=a[3].Split(';').Select(float.Parse).ToArray();
  foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();rows.Add("SCENE "+scene);
   var launch=Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(t=>t.name=="CR094 summit launch");
   // PROBE_HEAD=1: the 0.71 surfaces (git HEAD copies in Assets/Report072Head, and the 0.71 ribbon) replace the current colliders.
   if(Environment.GetEnvironmentVariable("PROBE_HEAD")=="1"){foreach(var mc in Object.FindObjectsByType<MeshCollider>(FindObjectsSortMode.None)){if(!mc.sharedMesh)continue;var n=System.IO.Path.GetFileName(AssetDatabase.GetAssetPath(mc.sharedMesh));
     var hp="Assets/Report072Head/"+n;if(File.Exists(hp)){mc.sharedMesh=AssetDatabase.LoadAssetAtPath<Mesh>(hp);rows.Add("HEAD "+mc.name);}
     if(mc.name=="Ground_CR103 supported return"){mc.sharedMesh=AssetDatabase.LoadAssetAtPath<Mesh>($"Assets/Track/Report071/{scene}-own-cr103-return.asset");rows.Add("HEAD ribbon");}}Physics.SyncTransforms();}
   foreach(var x in xs)for(float s=s0;s<=s1+1e-3f;s+=st){var w=launch.TransformPoint(x,0,s);var hits=Physics.RaycastAll(new Vector3(w.x,600,w.z),Vector3.down,1200,~0,QueryTriggerInteraction.Ignore).Where(h=>!h.collider.attachedRigidbody&&h.normal.y>0).OrderBy(h=>h.distance).Take(3).ToArray();
    rows.Add($"x {x,5:F1} s {s,6:F2} ({w.x:F2},{w.z:F2}): "+string.Join(" | ",hits.Select(h=>$"{h.point.y:F3} n{h.normal.y:F2} {h.collider.name}")));}}
  File.WriteAllLines(outDir+"/line.txt",rows);EditorApplication.Exit(0);}
}
