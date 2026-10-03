using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
// 0.71 read-only inspection (never saves a scene): every object near each reported coordinate, the surface column
// under it and a top-surface layer map (0.5 m cells) around it. PROBE_SCENES limits the scenes.
public static class Report071Probe {
 public static readonly (string id,string scene,Vector3 p,float heading)[] Reports={
  ("BUG-001","MountainLoop",new(767.34f,88.23f,-107.26f),15.89f),
  ("BUG-002","MountainLoop",new(753.74f,87.67f,-120.91f),38.79f),
  ("BUG-003","MountainLoop",new(998.48f,137.80f,-57.98f),55.94f),
  ("FR-001","StreetLoopGreybox",new(895.79f,151.05f,154.34f),230.79f),
  ("FR-002","StreetLoopGreybox",new(761.93f,109.29f,80.45f),76.14f)};
 static string Path(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n.Replace('\n','/');}
 public static void Run(){var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(outDir);Physics.queriesHitBackfaces=false;
  var only=(Environment.GetEnvironmentVariable("PROBE_IDS")??"").Split(',',StringSplitOptions.RemoveEmptyEntries);
  foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){
   EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();
   var race=Object.FindAnyObjectByType<RaceDirector>(FindObjectsInactive.Include);var road=race?race.road:null;if(road)road.Initialize();
   var branches=Object.FindObjectsByType<WoodlandRoute>(FindObjectsInactive.Include);foreach(var b in branches)b.Initialize();
   var all=Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None);
   foreach(var r in Reports.Where(x=>x.scene==scene&&(only.Length==0||only.Contains(x.id)))){var rows=new List<string>{$"{r.id} {r.scene} p={r.p} heading={r.heading}"};
    if(road){float s=road.Project(r.p,out float lat);var rp=road.At(s,out var f);rows.Add($"main s={s:F1} lat={lat:F2} roadPt={rp:F2} fwd={f:F2} hw={road.HalfWidth(s):F2}");}
    foreach(var b in branches){float bs=b.Project(r.p,out float bl);if(bl<40)rows.Add($"branch {b.title} active={b.gameObject.activeInHierarchy} s={bs:F1} lat={bl:F2} len={b.Length:F0} hw={b.halfWidth}");}
    Column(rows,r.p,"column");
    foreach(var t in all){var rend=t.GetComponent<Renderer>();var col=t.GetComponent<Collider>();var tm0=t.GetComponent<TextMesh>();if(!rend&&!col&&!tm0)continue;
     var bb=rend?rend.bounds:col?col.bounds:new Bounds(t.position,Vector3.zero);float dist=Mathf.Sqrt(bb.SqrDistance(r.p));if(dist>30)continue;bool big=bb.size.x>120||bb.size.z>120;if(big&&dist>2)continue;
     var mf=t.GetComponent<MeshFilter>();string mesh=mf&&mf.sharedMesh?AssetDatabase.GetAssetPath(mf.sharedMesh)+":"+mf.sharedMesh.name+" v"+mf.sharedMesh.vertexCount:"";
     string mat=rend&&rend.sharedMaterial?rend.sharedMaterial.name:"";
     var comps=string.Join(",",t.GetComponents<Component>().Where(c=>c&&!(c is Transform)&&!(c is MeshFilter)).Select(c=>c.GetType().Name));
     rows.Add($"{dist,6:F1} {(t.gameObject.activeInHierarchy?"A":"-")} {Path(t)} | pos={t.position:F2} rot={t.eulerAngles:F0} scl={t.lossyScale:F2} | b={bb.center:F1} sz={bb.size:F1} | {comps} | {mesh} | mat={mat}{(tm0?" | TEXT="+tm0.text.Replace('\n','/'):"")}");}
    File.WriteAllLines($"{outDir}/{scene}-{r.id}.txt",rows);
    Layers(outDir,scene,r.id,r.p,14);}}
  EditorApplication.Exit(0);}
 static void Column(List<string> rows,Vector3 p,string label){var o=new Vector3(p.x,p.y+60,p.z);rows.Add($"{label} at {p.x:F1},{p.z:F1}:");
  for(int k=0;k<20&&Physics.Raycast(o,Vector3.down,out var h,1000,~0,QueryTriggerInteraction.Collide);k++){rows.Add($"  {h.point.y:F2} n{h.normal.y:F2} {Path(h.collider.transform)} trig={h.collider.isTrigger}");o=h.point+Vector3.down*.02f;}}
 // Top-surface map: letter = topmost upward collider surface within +-4 m of the report height; '.' = none.
 public static void Layers(string outDir,string scene,string id,Vector3 p,float rad){var names=new Dictionary<string,char>();char Code(string n){if(!names.TryGetValue(n,out var c)){c=(char)('A'+names.Count);names[n]=c;}return c;}
  var map=new List<string>();var hmap=new List<string>();
  for(float z=p.z+rad;z>=p.z-rad;z-=.5f){var line=new System.Text.StringBuilder($"{z,8:F1} ");var hl=new System.Text.StringBuilder($"{z,8:F1} ");
   for(float x=p.x-rad;x<=p.x+rad;x+=.5f){var o=new Vector3(x,p.y+4,z);RaycastHit top=default;bool any=false;float depth=8;
    for(int k=0;k<10&&depth>0&&Physics.Raycast(o,Vector3.down,out var h,depth,~0,QueryTriggerInteraction.Ignore);k++){if(h.normal.y>0){top=h;any=true;break;}depth-=h.distance+.01f;o=h.point+Vector3.down*.01f;}
    line.Append(any?Code(top.collider.name):'.');hl.Append(any?((int)Mathf.Clamp(Mathf.Round((top.point.y-p.y)*5)+5,0,9)).ToString():".");}
   map.Add(line.ToString());hmap.Add(hl.ToString());}
  File.WriteAllLines($"{outDir}/layers-{scene}-{id}.txt",new[]{$"{scene} {id} around {p} r={rad}, 0.5 m cells, x left->right (west->east), z top->bottom (north->south)"}.Concat(map).Concat(names.Select(kv=>$"{kv.Value} = {kv.Key}")).Concat(new[]{"HEIGHT (digit = round((y-py)*5)+5, i.e. 0.2 m steps, 5 = report height)"}).Concat(hmap));}
}
