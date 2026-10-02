using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
// 0.67 inspection: every object near each reported coordinate (read-only).
public static class Report067Probe {
 public static readonly (string id,string scene,Vector3 p,float heading)[] Reports={
  ("BUG-001","StreetLoopGreybox",new(314.69f,9.14f,538.04f),236.5f),
  ("BUG-002","MountainLoopReverse",new(746.32f,90.97f,64.20f),89.2f),
  ("BUG-003","MountainLoopReverse",new(921.90f,156.89f,155.56f),354.2f),
  ("BUG-004","MountainLoopReverse",new(948.03f,165.14f,147.70f),116.2f),
  ("BUG-005","MountainLoopReverse",new(999.75f,161.76f,89.38f),102.5f),
  ("BUG-006","MountainLoopReverse",new(815.51f,96.64f,-148.82f),0f),
  ("BUG-007","MountainLoopReverse",new(825.20f,99.93f,-117.54f),78.5f),
  ("BUG-008","MountainLoopReverse",new(846.63f,102.07f,-110.01f),60.1f),
  ("BUG-009","MountainLoopReverse",new(985.35f,131.97f,-106.06f),95.6f),
  ("BUG-010","MountainLoopReverse",new(987.86f,132.40f,-92.78f),37.1f),
  ("BUG-011","MountainLoopReverse",new(1046.89f,150.85f,24.40f),208.9f),
  ("BUG-012","MountainLoopReverse",new(1006.88f,137.03f,-63.03f),225.4f),
  ("BUG-013","MountainLoopReverse",new(785.35f,96.00f,-184.02f),287.8f),
  ("BUG-014","MountainLoopReverse",new(701.91f,82.64f,-104.54f),142.2f),
  ("BUG-015","MountainLoop",new(1371.13f,101.15f,-236.74f),32.1f),
  ("BUG-016","MountainLoop",new(731.26f,92.21f,32.30f),120.9f),
  ("BUG-017","MountainLoop",new(963.81f,154.28f,97.12f),198.3f),
  ("BUG-018","MountainLoop",new(731.44f,79.66f,-26.64f),263.3f),
  ("BUG-019","DansBackyardForward",new(469.23f,80.94f,3.13f),199.0f),
  ("BUG-020","DansBackyardForward",new(68.18f,45.08f,61.54f),75.1f)};
 static string Path(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n;}
 public static void Run(){var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(outDir);
  var only=Environment.GetEnvironmentVariable("PROBE_IDS");float radius=float.Parse(Environment.GetEnvironmentVariable("PROBE_RADIUS")??"30");
  foreach(var g in Reports.Where(r=>string.IsNullOrEmpty(only)||only.Split(',').Contains(r.id)).GroupBy(r=>r.scene)){
   EditorSceneManager.OpenScene("Assets/Scenes/"+g.Key+".unity");Physics.SyncTransforms();Physics.queriesHitBackfaces=true;
   var race=Object.FindAnyObjectByType<RaceDirector>(FindObjectsInactive.Include);var road=race?race.road:null;if(road)road.Initialize();
   var branches=Object.FindObjectsByType<WoodlandRoute>(FindObjectsInactive.Include);foreach(var b in branches)b.Initialize();
   var all=Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None);
   foreach(var r in g){var rows=new List<string>{$"{r.id} {r.scene} p={r.p} heading={r.heading}"};
    if(road){float s=road.Project(r.p,out float lat);var rp=road.At(s,out var f);rows.Add($"main s={s:F1} lat={lat:F2} roadPt={rp.ToString("F2")} fwd={f.ToString("F2")}");}
    foreach(var b in branches){float bs=b.Project(r.p,out float bl);if(Mathf.Abs(bl)<25)rows.Add($"branch {b.title} s={bs:F1} lat={bl:F2} len={b.Length:F0}");}
    foreach(var h in Physics.RaycastAll(r.p+Vector3.up*40,Vector3.down,120,~0,QueryTriggerInteraction.Collide).OrderByDescending(h=>h.point.y))rows.Add($" ray@p {h.point.y:F2} n{h.normal.y:F2} {Path(h.collider.transform)} trig={h.collider.isTrigger}");
    foreach(var t in all){var rend=t.GetComponent<Renderer>();var col=t.GetComponent<Collider>();if(!rend&&!col)continue;
     var b=rend?rend.bounds:col.bounds;float dist=Mathf.Sqrt(b.SqrDistance(r.p));if(dist>radius)continue;
     bool big=b.size.x>120||b.size.z>120;if(big&&dist>2)continue;
     var mf=t.GetComponent<MeshFilter>();string mesh=mf&&mf.sharedMesh?AssetDatabase.GetAssetPath(mf.sharedMesh)+":"+mf.sharedMesh.name+" v"+mf.sharedMesh.vertexCount:"";
     var comps=string.Join(",",t.GetComponents<Component>().Where(c=>c&&!(c is Transform)&&!(c is MeshFilter)).Select(c=>c.GetType().Name));
     rows.Add($"{dist,6:F1} {(t.gameObject.activeInHierarchy?"A":"-")} {Path(t)} | pos={t.position.ToString("F2")} rot={t.eulerAngles.ToString("F0")} scl={t.lossyScale.ToString("F2")} | b={b.center.ToString("F1")} sz={b.size.ToString("F1")} | {comps} | {mesh}");}
    File.WriteAllLines(outDir+"/"+r.id+".txt",rows);}}
  EditorApplication.Exit(0);}
}
