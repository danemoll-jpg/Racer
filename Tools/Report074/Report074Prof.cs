using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Object=UnityEngine.Object;
// 0.74 read-only (BUG-003): the driving surface along the House 3 driveway road every 0.5 m (top solid hit, no trees):
// height, 2 m grade, and every place where the grade changes by more than 8 points within 3 m (crest / dip).
public static class Report074Prof {
 public static void Run(){var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(outDir);var rows=new List<string>();
  foreach(var sc in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){EditorSceneManager.OpenScene("Assets/Scenes/"+sc+".unity");Physics.SyncTransforms();
   var road=Object.FindObjectsByType<Racer.RaceRoad>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(r=>r.name=="House 3 valley driveway");var p=road.points;
   var pts=new List<Vector3>();float acc=0;pts.Add(p[0]);for(int i=0;i+1<p.Length;i++){float l=new Vector2(p[i+1].x-p[i].x,p[i+1].z-p[i].z).magnitude;for(float t=.5f-acc;t<=l;t+=.5f)pts.Add(Vector3.Lerp(p[i],p[i+1],t/l));acc=(l-(.5f-acc))%.5f;}
   var y=pts.Select(q=>{foreach(var h in Physics.RaycastAll(new Vector3(q.x,q.y+20,q.z),Vector3.down,60,~0,QueryTriggerInteraction.Ignore).OrderBy(h=>h.distance)){var n=h.collider.name;if(h.collider.isTrigger||n.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0||n.IndexOf("tree",StringComparison.OrdinalIgnoreCase)>=0)continue;return (h.point.y,n);}return (float.NaN,"none");}).ToArray();
   rows.Add($"SCENE {sc} {pts.Count} samples");float gmax=0,gat=0;
   for(int i=4;i+4<pts.Count;i++){float g1=(y[i].Item1-y[i-4].Item1)/2,g2=(y[i+4].Item1-y[i].Item1)/2;if(Mathf.Abs(g2)>gmax){gmax=Mathf.Abs(g2);gat=i*.5f;}
    if(Mathf.Abs(g2-g1)>.08f)rows.Add($"  s {i*.5f,6:F1} ({pts[i].x:F1},{pts[i].z:F1}) y {y[i].Item1:F2} grade in {g1*100:F0}% out {g2*100:F0}% ({(g2-g1<0?"CREST":"dip")}) on {y[i].Item2}");}
   rows.Add($"  steepest 2 m grade {gmax*100:F0}% at s {gat:F1}");
   for(int i=0;i<pts.Count;i+=20)rows.Add($"  p s {i*.5f,6:F1} y {y[i].Item1:F2} {y[i].Item2}");}
  File.WriteAllLines(outDir+"/profile.txt",rows);EditorApplication.Exit(0);}
}
