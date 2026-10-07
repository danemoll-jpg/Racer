using System;using System.IO;using System.Linq;using System.Text;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;using Racer;
// 0.87 Part B: the Summit Climb branch's points become the AI line (Tools/Report087/summit-line.txt, Racing-Line.py), each set
// on the carved ground under it; recommendedSpeed 33. Nothing else changes (the trail, trees and markers stay).
public static class Report087Line {
 public static void Run(){var o=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(o);var sb=new StringBuilder();
  try{var s=EditorSceneManager.OpenScene("Assets/Scenes/LakeWoods.unity");Physics.SyncTransforms();var roots=s.GetRootGameObjects();
   var br=roots.SelectMany(g=>g.GetComponentsInChildren<WoodlandRoute>(true)).First(b=>b.title=="Summit Climb");var inv=System.Globalization.CultureInfo.InvariantCulture;var old=br.points;
   var xz=File.ReadAllLines("Tools/Report087/summit-line.txt").Where(l=>!l.StartsWith("#")&&l.Trim().Length>0).Select(l=>l.Split(' ').Select(x=>float.Parse(x,inv)).ToArray()).ToArray();
   if(xz.Length!=old.Length)throw new Exception($"line has {xz.Length} points, branch {old.Length}");
   var pts=new Vector3[xz.Length];float maxDy=0;
   for(int i=0;i<pts.Length;i++){var p=new Vector3(xz[i][0],old[i].y,xz[i][1]);float best=float.NaN,bd=1e9f;foreach(var h in Physics.RaycastAll(p+Vector3.up*6,Vector3.down,12,~0,QueryTriggerInteraction.Ignore)){if(!h.collider.name.StartsWith("Ground"))continue;float d=Mathf.Abs(h.point.y-p.y);if(d<bd){bd=d;best=h.point.y;}}
    if(!float.IsNaN(best)){maxDy=Mathf.Max(maxDy,Mathf.Abs(best-p.y));p.y=best;}pts[i]=p;}
   br.points=pts;br.recommendedSpeed=33;EditorUtility.SetDirty(br);br.Initialize();
   sb.AppendLine($"Summit Climb branch points -> AI line ({pts.Length} points, largest sideways move {Enumerable.Range(0,pts.Length).Max(i=>Vector2.Distance(new(pts[i].x,pts[i].z),new(old[i].x,old[i].z))):F2} m, ground under it within {maxDy:F2} m of the centre profile); length {br.Length:F1}; recommendedSpeed 33");
   EditorSceneManager.MarkSceneDirty(s);EditorSceneManager.SaveScene(s);sb.AppendLine("saved");
  }catch(Exception e){sb.AppendLine("ERROR "+e);}File.WriteAllText(o+"/B-line.txt",sb.ToString());EditorApplication.Exit(0);}
}
