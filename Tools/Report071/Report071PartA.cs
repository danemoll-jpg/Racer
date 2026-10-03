using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
// 0.71 read-only (Part A): the Summit Homeward Flight launch and its catch landing in each scene. Hierarchy, the site and map
// destination, cross-sections of the top surface in the launch frame (with and without the catch-landing collider), and the
// top surface along the straight Free Roam path between the two 0.70 report positions.
public static class Report071PartA {
 static string Path(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n.Replace('\n','/');}
 static bool Top(Vector3 p,out RaycastHit hit){var o=new Vector3(p.x,600,p.z);hit=default;for(int k=0;k<10&&Physics.Raycast(o,Vector3.down,out var h,1200,~0,QueryTriggerInteraction.Ignore);k++){if(h.normal.y>0&&!h.collider.attachedRigidbody){hit=h;return true;}o=h.point+Vector3.down*.05f;}return false;}
 public static void Run(){var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(outDir);Physics.queriesHitBackfaces=false;
  foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();var rows=new List<string>{"SCENE "+scene};
   var root=Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).FirstOrDefault(t=>t.name=="CR094 summit launch");
   if(!root){rows.Add("no CR094 summit launch");File.WriteAllLines($"{outDir}/partA-{scene}.txt",rows);continue;}
   rows.Add($"root {Path(root)} active={root.gameObject.activeInHierarchy} pos={root.position:F2} rot={root.eulerAngles:F2} fwd={root.forward:F3}");
   foreach(var t in root.GetComponentsInChildren<Transform>(true)){var mf=t.GetComponent<MeshFilter>();var c=t.GetComponent<Collider>();var r=t.GetComponent<Renderer>();
    rows.Add($"  {(t.gameObject.activeInHierarchy?"A":"-")} {Path(t)} local={root.InverseTransformPoint(t.position):F1} | {string.Join(",",t.GetComponents<Component>().Where(x=>x&&!(x is Transform)).Select(x=>x.GetType().Name))} | {(mf&&mf.sharedMesh?AssetDatabase.GetAssetPath(mf.sharedMesh):"")} {(r?"b="+r.bounds.center.ToString("F1")+" sz="+r.bounds.size.ToString("F1"):"")}");}
   foreach(var a in Object.FindObjectsByType<ActivitySite>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(a=>a.id=="summit-homeward"))rows.Add($"SITE {Path(a.transform)} active={a.gameObject.activeInHierarchy} pos={a.transform.position:F2} local={root.InverseTransformPoint(a.transform.position):F1} fwd={a.forward:F3} r={a.radius} bronze={a.bronze} silver={a.silver} gold={a.gold} kind={a.kind}");
   var map=Object.FindAnyObjectByType<ExplorationMap>(FindObjectsInactive.Include);if(map)foreach(var d in map.destinations.Where(d=>d.id.Contains("summit")))rows.Add($"DEST {d.id} '{d.title}' {d.position:F1} yaw {d.yaw}");
   var col=Object.FindAnyObjectByType<ExplorationCollection>(FindObjectsInactive.Include);if(col)foreach(var r in col.routes){r.Initialize();var near=r.points.Select((p,i)=>(p,i)).Where(x=>{var q=root.InverseTransformPoint(x.p);return q.z>150&&q.z<700&&Mathf.Abs(q.x)<80;}).ToArray();if(near.Length>0||r.name.Contains("Summit"))rows.Add($"ROUTE '{r.name}' pts={r.points.Length} near={near.Length}: "+string.Join(" ",near.Select(x=>$"[{x.i}]{x.p:F0}/l{root.InverseTransformPoint(x.p):F0}")));}
   // signs / text near the landing
   foreach(var tm in Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include,FindObjectsSortMode.None)){var q=root.InverseTransformPoint(tm.transform.position);if(q.z>-40&&q.z<650&&Mathf.Abs(q.x)<90)rows.Add($"TEXT {Path(tm.transform)} local={q:F0} '{tm.text.Replace('\n','/')}'");}
   var landing=root.Find("Ground_CR103 smooth landing");var lc=landing?landing.GetComponent<Collider>():null;
   rows.Add("SECTIONS local s, x: top y [collider] / without the catch landing");
   for(float s=150;s<=640;s+=10){var line=new System.Text.StringBuilder($"s {s,4}: ");foreach(float x in new[]{-60f,-40,-24,-12,0,12,24,40,60}){var p=root.TransformPoint(new Vector3(x,0,s));
     string a=Top(p,out var h)?$"{h.point.y:F1}{(h.collider==lc?"*":"")}":"-";string b="";if(lc&&lc.enabled){lc.enabled=false;Physics.SyncTransforms();b=Top(p,out var h2)?$"/{h2.point.y:F1}":"/-";lc.enabled=true;Physics.SyncTransforms();}line.Append($"x{x}:{a}{b}  ");}
    rows.Add(line.ToString());}
   // Straight path between the 0.70 report positions (BUG-002 -> BUG-001), extended 40 m each way.
   var A=new Vector3(761.93f,0,80.45f);var B=new Vector3(895.79f,0,154.34f);var dir=(B-A).normalized;rows.Add($"PATH 0.70 BUG-002 -> BUG-001, len {(B-A).magnitude:F1}; samples every 4 m with the local launch frame position");
   for(float d=-40;d<=(B-A).magnitude+40;d+=4){var p=A+dir*d;string a=Top(p,out var h)?$"{h.point.y:F2} {h.collider.name}":"-";var q=root.InverseTransformPoint(new Vector3(p.x,h.point.y,p.z));rows.Add($"  d {d,6:F0} ({p.x:F1},{p.z:F1}) local({q.x:F0},{q.z:F0}) top {a}");}
   // Objects standing in the catch region
   foreach(var c in Object.FindObjectsByType<Collider>(FindObjectsInactive.Exclude,FindObjectsSortMode.None)){var b=c.bounds;if(b.size.x>40||b.size.z>40)continue;var q=root.InverseTransformPoint(b.center);if(q.z>300&&q.z<560&&Mathf.Abs(q.x)<60)rows.Add($"OBJ {Path(c.transform)} local={q:F1} size={b.size:F1} trig={c.isTrigger}");}
   File.WriteAllLines($"{outDir}/partA-{scene}.txt",rows);}
  EditorApplication.Exit(0);}
}
