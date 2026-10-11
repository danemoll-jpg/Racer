using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 0.105 Part A sweep: the drivable surface of Hwy 92 (x -800..800) in each scene, the roadside built as at load.
//  along each line (-8..8 m every 0.5 m) every 5 cm: the top collider (anything but the road's own Ground_ meshes = "poke"),
//  and steps (a 5 cm height change differing from the local slope by more than 1 cm); across the road every 1 m at 10 cm.
public static class Report105Sweep {
 static readonly RaycastHit[] hits=new RaycastHit[32];
 static bool Top(Vector3 p,out RaycastHit top){int n=Physics.RaycastNonAlloc(p+Vector3.up*1.5f,Vector3.down,hits,5,~0,QueryTriggerInteraction.Ignore);top=default;bool any=false;for(int i=0;i<n;i++){if(!any||hits[i].point.y>top.point.y){top=hits[i];any=true;}}return any;}
 static float Median(List<float> l){if(l.Count==0)return 0;var s=l.OrderBy(x=>x).ToList();return s[s.Count/2];}
 public static void Run(){
  var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");bool roadside=Environment.GetEnvironmentVariable("PROBE_PLAY")!="off";
  foreach(var sn in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){
   var log=new List<string>();var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+sn+".unity");Physics.SyncTransforms();
   if(roadside){Racer.Hwy92Roadside.Clear(scene);new GameObject("check host").AddComponent<Racer.Hwy92Roadside>().Build(scene);Physics.SyncTransforms();}
   var roads=UnityEngine.Object.FindObjectsByType<Racer.RaceRoad>(FindObjectsSortMode.None).Where(r=>r.openHighway&&r.points!=null&&r.points.Length>1).ToList();
   log.Add($"{sn}: roadside {(roadside?"built":"off")}, open-highway roads {roads.Count}");
   var flags=new Dictionary<(int,int),(float res,string what)>();var pokes=new Dictionary<string,(int n,Vector3 at,float h)>();
   foreach(var road in roads){road.Initialize();
    var offs=new List<float>();for(float o=-8;o<=8.001f;o+=.5f)offs.Add(o);
    var prev=new float[offs.Count];var dh=new List<float>[offs.Count];for(int k=0;k<offs.Count;k++){prev[k]=float.NaN;dh[k]=new List<float>();}
    int stations=0;
    for(float s=0;s<road.Length;s+=.05f){
     var p=road.At(s,out var fwd);if(p.x<-800||p.x>800||p.z<380||p.z>850){for(int k=0;k<offs.Count;k++)prev[k]=float.NaN;continue;}stations++;
     fwd.y=0;fwd.Normalize();var right=Vector3.Cross(Vector3.up,fwd);
     for(int k=0;k<offs.Count;k++){
      var q=p+right*offs[k];if(!Top(q,out var t)){prev[k]=float.NaN;continue;}
      var c=t.collider;if(!c.name.StartsWith("Ground_")&&!c.GetComponentInParent<Racer.ArcadeVehicle>()){var key=c.name+" @"+c.transform.parent?.name;var e=pokes.TryGetValue(key,out var v)?v:(0,t.point,0f);pokes[key]=(e.Item1+1,e.Item2,Mathf.Max(e.Item3,t.point.y));}
      float y=t.point.y;if(!float.IsNaN(prev[k])){float d=y-prev[k];var l=dh[k];l.Add(d);if(l.Count>40)l.RemoveAt(0);float res=Mathf.Abs(d-Median(l));if(res>.01f){var key=(Mathf.RoundToInt(t.point.x),Mathf.RoundToInt(t.point.z));if(!flags.TryGetValue(key,out var f)||f.res<res)flags[key]=(res,$"along, lane offset {offs[k]:+0.0;-0.0}, {c.name}");}}
      prev[k]=y;
     }
     // across every 1 m at 10 cm
     if(Mathf.Repeat(s,1f)<.05f){var ys=new List<float>();for(float o=-8;o<=8.001f;o+=.1f){if(Top(p+right*o,out var t))ys.Add(t.point.y);else ys.Add(float.NaN);}
      var d=new List<float>();for(int i=1;i<ys.Count;i++)d.Add(ys[i]-ys[i-1]);
      for(int i=0;i<d.Count;i++){if(float.IsNaN(d[i]))continue;var win=d.Skip(Mathf.Max(0,i-10)).Take(21).Where(x=>!float.IsNaN(x)).ToList();float res=Mathf.Abs(d[i]-Median(win));if(res>.01f){var q=p+right*(-8+.1f*(i+1));var key=(Mathf.RoundToInt(q.x),Mathf.RoundToInt(q.z));if(!flags.TryGetValue(key,out var f)||f.res<res)flags[key]=(res,$"across at offset {-8+.1f*(i+1):+0.0;-0.0}");}}}
    }
    log.Add($"  road {road.name}: length {road.Length:F0} m, {stations} stations in the stretch");
   }
   log.Add($"== colliders other than the road on top of the drivable width: {pokes.Count}");foreach(var kv in pokes.OrderByDescending(k=>k.Value.n))log.Add($"  {kv.Key}: {kv.Value.n} samples, first at {kv.Value.at:F1}, top {kv.Value.h:F2}");
   log.Add($"== steps over 1 cm: {flags.Count} (1 m cells)");foreach(var kv in flags.OrderBy(k=>k.Key.Item1))log.Add($"  ({kv.Key.Item1},{kv.Key.Item2}) {kv.Value.res*100:F1} cm {kv.Value.what}");
   File.WriteAllLines(Path.Combine(outDir,"sweep-"+sn+".txt"),log);
  }
  EditorApplication.Exit(0);}
}
