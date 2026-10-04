using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Object=UnityEngine.Object;
// 0.73 read-only (Part A): for every water volume, the bed depth inside and the ground just outside its edge (a frozen
// surface must meet the banks without a step), and where course routes (RaceRoad / WoodlandRoute points) cross water.
public static class Report073Water {
 static string P(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n;}
 static float? Top(Vector3 at,float below){foreach(var h in Physics.RaycastAll(new Vector3(at.x,below+40,at.z),Vector3.down,80,1,QueryTriggerInteraction.Ignore).OrderBy(h=>h.distance)){if(h.collider.attachedRigidbody)continue;return h.point.y;}return null;}
 public static void Run(){var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(outDir);
  foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");var rows=new List<string>{"SCENE "+scene};Physics.SyncTransforms();
   var waters=Object.FindObjectsByType<Racer.ShallowWater>(FindObjectsSortMode.None);
   foreach(var w in waters){var t=w.transform;float s=w.Surface;
    // bed inside: grid over local -0.5..0.5
    var depths=new List<float>();for(int i=0;i<=20;i++)for(int j=0;j<=20;j++){var lp=new Vector3(-.5f+i/20f,0,-.5f+j/20f);if(!w.Contains(t.TransformPoint(lp)))continue;var wp=t.TransformPoint(lp);var y=Top(wp,s);if(y.HasValue)depths.Add(s-y.Value);}
    // outline: 64 points, just inside (95%) and just outside (+0.6 m)
    var edge=new List<string>();int steps=0,ledges=0;float worstStep=0,worstLedge=0;
    for(int k=0;k<64;k++){Vector3 lp;if(w.round){float a=k*Mathf.PI*2/64;lp=new Vector3(Mathf.Cos(a)*.5f,0,Mathf.Sin(a)*.5f);}else{float u=k/16f;int side=k/16;float f=(k%16)/16f-.5f;lp=side==0?new Vector3(f,0,-.5f):side==1?new Vector3(.5f,0,f):side==2?new Vector3(-f,0,.5f):new Vector3(-.5f,0,-f);}
     var inside=t.TransformPoint(lp*.95f);var outside=t.TransformPoint(lp);var dir=(outside-t.position);dir.y=0;outside+=dir.normalized*.6f;
     var yi=Top(inside,s);var yo=Top(outside,s);if(!yo.HasValue)continue;float d=yo.Value-s;// + = bank above ice (wall), - = drop off ice edge
     if(d<-.12f){ledges++;worstLedge=Mathf.Min(worstLedge,d);}if(yi.HasValue&&yi.Value-s>.12f){steps++;worstStep=Mathf.Max(worstStep,yi.Value-s);}
     edge.Add($"{d:F2}");}
    rows.Add($"WATER {P(t)} surface={s:F2} round={w.round} size={t.lossyScale.x:F1}x{t.lossyScale.z:F1} depth n={depths.Count} min={(depths.Count>0?depths.Min():0):F2} max={(depths.Count>0?depths.Max():0):F2} mean={(depths.Count>0?depths.Average():0):F2} outsideDrops(<-.12)={ledges} worst={worstLedge:F2} insideAbove={steps} worst={worstStep:F2}");
    rows.Add("  outside-minus-surface: "+string.Join(" ",edge));}
   var routes=Object.FindObjectsByType<Racer.RaceRoad>(FindObjectsInactive.Include,FindObjectsSortMode.None).Select(r=>(t:r.transform,pts:r.points)).Concat(Object.FindObjectsByType<Racer.WoodlandRoute>(FindObjectsInactive.Include,FindObjectsSortMode.None).Select(r=>(t:r.transform,pts:r.points)));
   foreach(var (rt,pts) in routes){var road=(transform:rt,points:pts);if(road.points==null||road.points.Length<2)continue;float st=0;var hits=new Dictionary<string,(float a,float b)>();
    for(int i=0;i<road.points.Length;i++){var a=road.points[i];var b=road.points[(i+1)%road.points.Length];float len=Vector3.Distance(a,b);for(float f=0;f<len;f+=1){var p=Vector3.Lerp(a,b,f/len);foreach(var w in waters)if(w.Contains(p)&&w.transform.lossyScale.x>4){var key=w.name;if(!hits.TryGetValue(key,out var r))r=(st+f,st+f);hits[key]=(Mathf.Min(r.a,st+f),Mathf.Max(r.b,st+f));}}st+=len;}
    foreach(var h in hits)rows.Add($"ROUTE {P(road.transform)} crosses {h.Key} at path m {h.Value.a:F0}-{h.Value.b:F0}");}
   File.WriteAllLines($"{outDir}/water-{scene}.txt",rows);}
  EditorApplication.Exit(0);}
}
