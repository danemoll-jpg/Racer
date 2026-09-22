using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.SceneManagement;
using Racer;
using Object=UnityEngine.Object;
public static class CheckLaurelRunway {
 public static string Main(){EditorSceneManager.OpenScene("Assets/Scenes/StreetLoopReverse.unity");Physics.SyncTransforms();var zone=Object.FindObjectsByType<JumpRecoveryExclusion>().Single(z=>z.name=="Laurel runway no recovery");var axis=Vector3.ProjectOnPlane(zone.end-zone.start,Vector3.up).normalized;var start=zone.end-axis*100;start.y=zone.end.y-3.8f;int samples=0;float error=0,normalChange=0;Vector3 previous=Vector3.up;var failures=new List<string>();
  for(float s=0;s<=100;s+=.25f){float u=Mathf.Clamp01((s-72)/28);var p=start+axis*s+Vector3.up*(3.8f*u*u);foreach(float lane in new[]{-3f,0,3f}){var q=p+Vector3.Cross(Vector3.up,axis)*lane;var hits=Physics.RaycastAll(q+Vector3.up*70,Vector3.down,100,1,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name.StartsWith("Ground_")).OrderByDescending(h=>h.point.y).ToArray();samples++;if(hits.Length==0||Math.Abs(hits[0].point.y-q.y)>.02f){if(failures.Count<25)failures.Add($"Runway {s:F2} lane={lane}: "+string.Join(";",hits.Take(3).Select(h=>$"{h.collider.name} y={h.point.y:F3} expected={q.y:F3}")));continue;}error=Mathf.Max(error,Math.Abs(hits[0].point.y-q.y));if(lane==0){if(s>0)normalChange=Mathf.Max(normalChange,Vector3.Angle(previous,hits[0].normal));previous=hits[0].normal;}}}
  var b=Object.FindObjectsByType<WoodlandRoute>().Single(x=>x.title=="Laurel Switchbacks");var rows=new List<string>{$"Straight runway samples={samples}; highest-surface error={error:F5}m; max adjacent normal change={normalChange:F3}deg; failures={failures.Count}"};rows.AddRange(failures);for(float s=0;s<b.Length;s+=40)rows.Add($"branch {s}: {b.At(s,out _)}");File.WriteAllLines("Docs/CR133-137/Laurel-runway-checks.txt",rows);return string.Join("\n",rows);
 }
}
