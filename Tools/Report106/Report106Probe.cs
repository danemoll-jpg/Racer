using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 0.106 editor probes (batch mode). PROBE_SCENES (comma list), PROBE_OUT.
public static class Report106Probe {
 public static void Compile(){EditorApplication.Exit(0);}
 static string P(Transform t){var s=t.name;while(t.parent){t=t.parent;s=t.name+"/"+s;}return s;}
 static readonly string[] World={"FreeRoamWorld","StreetLoopGreybox","StreetLoopReverse","DansBackyardForward","DansBackyardReverse","LakeWoods","ForestLoopReverse","MountainLoop","MountainLoopReverse"};
 // the 0.104 plot outline (game x,z), clipped at the world's north edge
 public static readonly Vector2[] Plot={new(-52.1f,605.3f),new(-57.4f,611.4f),new(-62.5f,630.6f),new(-60.9f,636.1f),new(-18.2f,802.8f),new(22.2f,848.0f),new(312.1f,848.0f),new(321.0f,638.7f),new(170.8f,644.4f),new(169.5f,570.6f),new(27.5f,567.3f)};
 static bool Inside(Vector2[] poly,Vector2 p){bool inside=false;for(int i=0,j=poly.Length-1;i<poly.Length;j=i++)if((poly[i].y>p.y)!=(poly[j].y>p.y)&&p.x<(poly[j].x-poly[i].x)*(p.y-poly[i].y)/(poly[j].y-poly[i].y)+poly[i].x)inside=!inside;return inside;}
 static float Ground(float x,float z,out string name){name="";float best=float.NaN;foreach(var h in Physics.RaycastAll(new Vector3(x,600,z),Vector3.down,1000,~0,QueryTriggerInteraction.Ignore)){if(!h.collider.name.StartsWith("Ground_"))continue;if(float.IsNaN(best)||h.point.y>best){best=h.point.y;name=h.collider.name;}}return best;}
 public static void Plot106(){
  var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");var list=Environment.GetEnvironmentVariable("PROBE_SCENES");var scenes=string.IsNullOrEmpty(list)||list=="all"?World:list.Split(',');
  foreach(var sn in scenes){
   var log=new List<string>();var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+sn+".unity");Physics.SyncTransforms();
   bool world=scene.GetRootGameObjects().Any(g=>g.name=="Remembered houses and approximate buildings");log.Add($"scene {sn} world={world}");
   // ground heights every 5 m inside the plot's box
   var grid=new List<string>{"x,z,inPlot,ground,collider"};float lo=float.MaxValue,hi=float.MinValue;var tiles=new Dictionary<string,int>();
   for(float x=-70;x<=330;x+=5)for(float z=560;z<=850;z+=5){var y=Ground(x,z,out var nm);bool inp=Inside(Plot,new Vector2(x,z));grid.Add($"{x},{z},{(inp?1:0)},{y:F2},{nm}");if(inp&&!float.IsNaN(y)){lo=Mathf.Min(lo,y);hi=Mathf.Max(hi,y);tiles[nm]=tiles.TryGetValue(nm,out var c)?c+1:1;}}
   File.WriteAllLines(Path.Combine(outDir,"ground-"+sn+".csv"),grid);
   log.Add($"ground in plot {lo:F2}..{hi:F2}; tiles: "+string.Join(", ",tiles.Select(k=>k.Key+" x"+k.Value)));
   // every collider (not ground) whose bounds overlap the plot
   var box=new Bounds(new Vector3(130,0,707),new Vector3(400,2000,300));
   foreach(var c in scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Collider>(true))){if(c.name.StartsWith("Ground_"))continue;var b=c.bounds;if(!b.Intersects(box))continue;
    bool any=Inside(Plot,new Vector2(b.center.x,b.center.z))||Inside(Plot,new Vector2(b.min.x,b.min.z))||Inside(Plot,new Vector2(b.max.x,b.max.z))||Inside(Plot,new Vector2(b.min.x,b.max.z))||Inside(Plot,new Vector2(b.max.x,b.min.z));if(!any)continue;
    log.Add($"COLLIDER {P(c.transform)} {c.GetType().Name} en={c.enabled&&c.gameObject.activeInHierarchy} trig={c.isTrigger} {b.min:F1}..{b.max:F1}");}
   // renderers by top-level root, inside the plot
   var rr=new Dictionary<string,int>();foreach(var r in scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Renderer>(true))){var b=r.bounds;if(b.size.x>300||b.size.z>300)continue;if(!Inside(Plot,new Vector2(b.center.x,b.center.z)))continue;var t=r.transform;var path=P(t);var key=string.Join("/",path.Split('/').Take(3));rr[key]=rr.TryGetValue(key,out var n)?n+1:1;}
   foreach(var k in rr)log.Add($"RENDERERS {k.Key} x{k.Value}");
   // roads, race lines, acorns, activity sites, gates
   foreach(var road in scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Racer.RaceRoad>(true))){if(road.points==null)continue;int inside=road.points.Count(p=>Inside(Plot,new Vector2(p.x,p.z)));if(inside>0)log.Add($"ROAD {P(road.transform)} {inside}/{road.points.Length} points inside");}
   foreach(var ec in scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Racer.ExplorationCollection>(true))){log.Add($"ACORNS {P(ec.transform)} {ec.sites.Length} sites; approaches "+string.Join(",",ec.sites.Select(s=>s.approach).Distinct()));foreach(var s in ec.sites)if(Inside(Plot,new Vector2(s.position.x,s.position.z)))log.Add($"  ACORN IN PLOT {s.id} {s.position}");}
   foreach(var a in scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Racer.ActivitySite>(true)))if(Vector3.Distance(a.transform.position,new Vector3(130,10,700))<350)log.Add($"SITE {a.id} {a.kind} {a.transform.position}");
   foreach(var g in scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Racer.RaceGate>(true)))if(Inside(Plot,new Vector2(g.transform.position.x,g.transform.position.z)))log.Add($"GATE IN PLOT {P(g.transform)}");
   // Hwy 92 near the plot: the through road's centre and height at x 0..200
   var dir=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Racer.RaceDirector>(true)).FirstOrDefault();
   if(dir){foreach(var r in new[]{dir.throughRoad,dir.ambientRoad,dir.road}){if(!r||r.points==null)continue;foreach(var p in r.points)if(p.x>-80&&p.x<340&&p.z>480&&p.z<640)log.Add($"ROADPT {r.name} {p:F1}");}}
   File.WriteAllLines(Path.Combine(outDir,"plot-"+sn+".txt"),log);
  }
  EditorApplication.Exit(0);}

 // the park built in each world scene (edit mode): the trees cleared; Hwy 92 where the drive meets it (no park collider over the road, the step at the edge);
 // the park's banks inside the 0.104 plot
 public static void Park(){
  var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");var list=Environment.GetEnvironmentVariable("PROBE_SCENES");var scenes=string.IsNullOrEmpty(list)||list=="all"?World:list.Split(',');
  var log=new List<string>();
  foreach(var sn in scenes){
   var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+sn+".unity");Physics.SyncTransforms();
   Racer.SceneryTrees.Cleared.Clear();Racer.Hwy92Roadside.Clear(scene);Racer.StuntPark.Clear(scene);
   var host=new GameObject("check host");host.AddComponent<Racer.Hwy92Roadside>().Build(scene);var park=host.AddComponent<Racer.StuntPark>();park.Build(scene);Physics.SyncTransforms();
   log.Add($"== {sn}: {string.Join("; ",park.Report)}; cleared: {string.Join("; ",Racer.StuntPark.Cleared)}");
   // Hwy 92 at the drive: every 0.25 m across, every 0.05 m along, from the road's middle to 3 m past its edge
   float edge=float.Parse(park.Report[0].Split(new[]{"z "},StringSplitOptions.None)[1].Split(',')[0],System.Globalization.CultureInfo.InvariantCulture);
   int over=0;float worst=0;string worstAt="";
   for(float x=Racer.StuntPark.DriveX-8;x<=Racer.StuntPark.DriveX+8.01f;x+=.25f){
    float prev=float.NaN;
    for(float z=545;z<=edge+3;z+=.05f){
     var hits=Physics.RaycastAll(new Vector3(x,60,z),Vector3.down,120,~0,QueryTriggerInteraction.Ignore).Where(h=>!h.collider.isTrigger).OrderByDescending(h=>h.point.y).ToArray();if(hits.Length==0)continue;var top=hits[0];
     if(z<edge-.02f&&top.collider.name.StartsWith("Stunt park"))over++;
     if(!float.IsNaN(prev)&&Mathf.Abs(x-Racer.StuntPark.DriveX)<=Racer.StuntPark.DriveHalf-.3f&&z>edge-.5f&&z<edge+.5f){float d=Mathf.Abs(top.point.y-prev);if(d>worst){worst=d;worstAt=$"x {x:F2} z {z:F2} ({top.collider.name})";}}
     prev=top.point.y;}}
   log.Add($"  Hwy 92 under the drive's mouth: park colliders above the road surface: {over}; largest height step between 5 cm samples across the road's edge on the drive: {worst*100:F1} cm at {worstAt}");
   // the banks (the height field's vertices above the ground) inside the plot
   var mc=GameObject.Find("Stunt park ground (banks and drive)")?.GetComponent<MeshCollider>();int outside=0,total=0;
   if(mc){foreach(var v in mc.sharedMesh.vertices.Distinct()){var w=mc.transform.TransformPoint(v);if(w.z<572&&Mathf.Abs(w.x-Racer.StuntPark.DriveX)<12)continue;float g=float.NaN;foreach(var h in Physics.RaycastAll(new Vector3(w.x,600,w.z),Vector3.down,1000,~0,QueryTriggerInteraction.Ignore))if(h.collider.name.StartsWith("Ground_")&&(float.IsNaN(g)||h.point.y>g))g=h.point.y;if(float.IsNaN(g)||w.y<g+.02f)continue;total++;if(!Inside(Plot,new Vector2(w.x,w.z)))outside++;}}
   log.Add($"  banks: {total} height-field points above the ground, {outside} of them outside the 0.104 plot (the drive's own run from Hwy 92 to the plot excluded)");
   UnityEngine.Object.DestroyImmediate(host);
  }
  File.WriteAllLines(Path.Combine(outDir,"park-probe.txt"),log);EditorApplication.Exit(0);}
}
