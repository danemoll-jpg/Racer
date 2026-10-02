using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
// 0.68 read-only inspection (never saves a scene).
// - every object near each reported coordinate;
// - course metadata: road length/origin, spawn, gates, flights, branches, exclusions;
// - "looks solid but no collider" map: every renderer without a collider gets a temporary MeshCollider on a
//   private layer; cells where the top visible surface has no physical collider within 2 m below are holes;
// - 0.67 seam-cover audit: any seam-cover triangle with no physical collider within 2.5 m beneath.
public static class Report068Probe {
 public static readonly (string id,string scene,Vector3 p,float heading)[] Reports={
  ("BUG-003","MountainLoopReverse",new(814.16f,95.49f,-171.39f),176.09f),
  ("BUG-004","MountainLoopReverse",new(810.26f,95.43f,-159.19f),346.79f),
  ("BUG-005","MountainLoopReverse",new(744.70f,85.84f,-113.38f),148.39f),
  ("BUG-006","MountainLoopReverse",new(1005.11f,162.65f,121.37f),6.84f),
  ("BUG-007","MountainLoop",new(684.53f,-334.58f,71.83f),270.90f),
  ("BUG-008","MountainLoop",new(802.79f,177.67f,90.57f),249.87f),
  ("BUG-009","MountainLoop",new(728.22f,80.69f,-10.35f),84.97f)};
 static string Path(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n.Replace('\n','/');}
 const int Ghost=31;
 public static void Run(){var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(outDir);Physics.queriesHitBackfaces=false;
  foreach(var scene in (Environment.GetEnvironmentVariable("PROBE_SCENES")??"MountainLoop,MountainLoopReverse").Split(',')){
   EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();
   var race=Object.FindAnyObjectByType<RaceDirector>(FindObjectsInactive.Include);var road=race.road;road.Initialize();
   var branches=Object.FindObjectsByType<WoodlandRoute>(FindObjectsInactive.Include);foreach(var b in branches)b.Initialize();
   var meta=new List<string>{$"{scene} road length={road.Length:F1} origin={race.Origin:F1} points={road.points.Length}"};
   var spawn=race.vehicle.GetComponent<VehicleRespawn>().spawnPoint;if(spawn){float ss=road.Project(spawn.position,out float sl);meta.Add($"spawn {spawn.position:F2} main s={ss:F1} lat={sl:F2}");Column(meta,spawn.position,"spawn column");}
   for(int g=0;g<race.gates.Length;g++){float gs=road.Project(race.gates[g].transform.position,out _);meta.Add($"gate {g} {race.gates[g].name} s={gs:F1} rel={road.Relative(gs,race.Origin):F1} at {race.gates[g].transform.position:F1}");}
   var flights=Object.FindAnyObjectByType<MountainFlights>(FindObjectsInactive.Include);
   if(flights)foreach(var f in flights.flights)meta.Add($"flight '{f.name}' start={f.start:F1} lip={f.lip:F1} landingEnd={f.landingEnd:F1} fwd={f.forward:F2} approachS={f.approachStation:F1} endS={f.endStation:F1} lipS={road.Project(f.lip,out _):F1} landS={road.Project(f.landingEnd,out _):F1}");
   foreach(var b in branches){var gd=b.GetComponent<ReverseShortcutGuidance>();meta.Add($"branch '{b.title}' active={b.gameObject.activeInHierarchy} len={b.Length:F1} entryRoad={b.entryRoad:F1} exitRoad={b.exitRoad:F1} hw={b.halfWidth} start={b.points[0]:F1} end={b.points[^1]:F1} takeoff={(gd?gd.takeoff:-1):F1} landing={(gd?gd.landing:-1):F1}");}
   foreach(var z in Object.FindObjectsByType<JumpRecoveryExclusion>(FindObjectsInactive.Include))meta.Add($"exclusion {Path(z.transform)} {z.start:F1}->{z.end:F1} hw={z.halfWidth}");
   foreach(var c in Object.FindObjectsByType<Collider>().Where(c=>c.name.StartsWith("Takeoff -")||c.name.StartsWith("Landing -")))meta.Add($"jump collider {Path(c.transform)} bounds {c.bounds.center:F1} sz {c.bounds.size:F1} s={road.Project(c.bounds.center,out _):F1}");
   // Reports in this scene.
   var all=Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None);
   foreach(var r in Reports.Where(x=>x.scene==scene)){var rows=new List<string>{$"{r.id} {r.scene} p={r.p} heading={r.heading}"};
    float s=road.Project(r.p,out float lat);var rp=road.At(s,out var f);rows.Add($"main s={s:F1} lat={lat:F2} roadPt={rp:F2} fwd={f:F2}");
    foreach(var b in branches){float bs=b.Project(r.p,out float bl);if(bl<30)rows.Add($"branch {b.title} s={bs:F1} lat={bl:F2} len={b.Length:F0}");}
    var probeP=r.p.y<0?new Vector3(r.p.x,rp.y,r.p.z):r.p;Column(rows,probeP,"column");
    foreach(var t in all){var rend=t.GetComponent<Renderer>();var col=t.GetComponent<Collider>();if(!rend&&!col)continue;
     var bb=rend?rend.bounds:col.bounds;float dist=Mathf.Sqrt(bb.SqrDistance(probeP));if(dist>25)continue;bool big=bb.size.x>120||bb.size.z>120;if(big&&dist>2)continue;
     var mf=t.GetComponent<MeshFilter>();string mesh=mf&&mf.sharedMesh?AssetDatabase.GetAssetPath(mf.sharedMesh)+":"+mf.sharedMesh.name+" v"+mf.sharedMesh.vertexCount:"";
     var comps=string.Join(",",t.GetComponents<Component>().Where(c=>c&&!(c is Transform)&&!(c is MeshFilter)).Select(c=>c.GetType().Name));var tm=t.GetComponent<TextMesh>();
     rows.Add($"{dist,6:F1} {(t.gameObject.activeInHierarchy?"A":"-")} {Path(t)} | pos={t.position:F2} rot={t.eulerAngles:F0} scl={t.lossyScale:F2} | b={bb.center:F1} sz={bb.size:F1} | {comps} | {mesh}{(tm?" | TEXT="+tm.text.Replace('\n','/'):"")}");}
    File.WriteAllLines($"{outDir}/{scene}-{r.id}.txt",rows);}
   // Temporary ghost colliders for every visible mesh that has no collider (scene is never saved).
   var ghosts=new List<MeshCollider>();
   foreach(var mr in Object.FindObjectsByType<MeshRenderer>()){if(!mr.enabled||mr.GetComponent<Collider>())continue;var mf=mr.GetComponent<MeshFilter>();if(!mf||!mf.sharedMesh)continue;
    if(mr.name.Contains("Water")||mr.name.Contains("water")||mr.name.Contains("lake")||mr.name.Contains("Lake")||mr.name.Contains("arrow")||mr.name.Contains("Arrow"))continue;
    var go=new GameObject("ghost:"+Path(mr.transform));go.layer=Ghost;go.transform.SetPositionAndRotation(mr.transform.position,mr.transform.rotation);go.transform.localScale=mr.transform.lossyScale;var mc=go.AddComponent<MeshCollider>();mc.sharedMesh=mf.sharedMesh;ghosts.Add(mc);}
   Physics.SyncTransforms();int physMask=~(1<<Ghost);
   var lo=road.points.Aggregate(new Vector2(1e9f,1e9f),(a,p)=>Vector2.Min(a,new(p.x,p.z)))-new Vector2(120,120);var hi=road.points.Aggregate(new Vector2(-1e9f,-1e9f),(a,p)=>Vector2.Max(a,new(p.x,p.z)))+new Vector2(120,120);
   var holes=new List<string>{$"{scene} visible-without-collider cells (2 m grid) x {lo.x:F0}..{hi.x:F0} z {lo.y:F0}..{hi.y:F0}: x,z,visibleTop,visibleName,physTop,physName"};
   var noFloor=new List<string>{$"{scene} cells with NO physical collider at all (2 m grid)"};
   for(float x=lo.x;x<=hi.x;x+=2)for(float z=lo.y;z<=hi.y;z+=2){var o=new Vector3(x,600,z);
    bool vis=Physics.Raycast(o,Vector3.down,out var vh,1200,1<<Ghost,QueryTriggerInteraction.Ignore);bool phys=Physics.Raycast(o,Vector3.down,out var ph,1200,physMask,QueryTriggerInteraction.Ignore);
    if(!phys){noFloor.Add($"{x:F0},{z:F0},{(vis?vh.point.y.ToString("F1")+" "+vh.collider.name:"-")}");continue;}
    if(vis&&vh.point.y>ph.point.y+2&&vh.normal.y>.5f)holes.Add($"{x:F0},{z:F0},{vh.point.y:F1},{vh.collider.name},{ph.point.y:F1},{Path(ph.collider.transform)}");}
   File.WriteAllLines($"{outDir}/{scene}-visible-no-collider.txt",holes);File.WriteAllLines($"{outDir}/{scene}-no-floor.txt",noFloor);
   // Seam-cover audit.
   var seam=Object.FindObjectsByType<MeshFilter>().FirstOrDefault(m=>m.name=="Ground_Report067 edge seams");var seamRows=new List<string>();
   if(seam){var m=seam.sharedMesh;var v=m.vertices;var t=m.triangles;int open=0,deep=0;
    for(int i=0;i<t.Length;i+=3){var c=seam.transform.TransformPoint((v[t[i]]+v[t[i+1]]+v[t[i+2]])/3);
     if(!Physics.Raycast(c+Vector3.up*.02f,Vector3.down,out var h,400,physMask,QueryTriggerInteraction.Ignore)){open++;seamRows.Add($"VOID {c:F2}");continue;}
     if(c.y-h.point.y>2.5f){deep++;seamRows.Add($"DEEP {c:F2} floor {h.point.y:F2} {h.collider.name} drop {c.y-h.point.y:F1}");}}
    seamRows.Insert(0,$"{scene} seam tris={t.Length/3} void={open} deeper-than-2.5m={deep}");}
   else seamRows.Add($"{scene}: no seam cover");
   File.WriteAllLines($"{outDir}/{scene}-seam-audit.txt",seamRows);
   // Other 0.67 collider-free Report067 objects.
   foreach(var mr in Object.FindObjectsByType<MeshRenderer>().Where(x=>x.name.Contains("Report067")&&!x.GetComponent<Collider>()))meta.Add($"collider-free 0.67 object {Path(mr.transform)} bounds {mr.bounds.center:F1} sz {mr.bounds.size:F1}");
   File.WriteAllLines($"{outDir}/{scene}-meta.txt",meta);
   foreach(var g in ghosts)Object.DestroyImmediate(g.gameObject);}
  EditorApplication.Exit(0);}
 static void Column(List<string> rows,Vector3 p,string label){var o=new Vector3(p.x,p.y+60,p.z);rows.Add($"{label} at {p.x:F1},{p.z:F1}:");
  for(int k=0;k<20&&Physics.Raycast(o,Vector3.down,out var h,1000,~0,QueryTriggerInteraction.Collide);k++){rows.Add($"  {h.point.y:F2} n{h.normal.y:F2} {Path(h.collider.transform)} trig={h.collider.isTrigger}");o=h.point+Vector3.down*.02f;}}
}
