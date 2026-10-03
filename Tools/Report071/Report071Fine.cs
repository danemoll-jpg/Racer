using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Object=UnityEngine.Object;
// 0.71 read-only: fine top-surface scan. PROBE_FINE2="scene|x0,z0,x1,z1|step|yRef". Lists cells where the topmost upward
// surface is NOT the driving surface while a driving surface exists within 0.3 m below/above, and the height of every layer.
public static class Report071Fine {
 public static void Run(){var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(outDir);var a=Environment.GetEnvironmentVariable("PROBE_FINE2").Split('|');
  EditorSceneManager.OpenScene("Assets/Scenes/"+a[0]+".unity");Physics.SyncTransforms();Physics.queriesHitBackfaces=true;var b=a[1].Split(',').Select(float.Parse).ToArray();float step=float.Parse(a[2]),yr=float.Parse(a[3]);
  var rows=new List<string>();var counts=new Dictionary<string,int>();
  for(float x=b[0];x<=b[2];x+=step)for(float z=b[1];z<=b[3];z+=step){var o=new Vector3(x,yr+3,z);var hs=new List<RaycastHit>();float depth=6;
   for(int k=0;k<8&&depth>0&&Physics.Raycast(o,Vector3.down,out var h,depth,~0,QueryTriggerInteraction.Ignore);k++){hs.Add(h);depth-=h.distance+.005f;o=h.point+Vector3.down*.005f;}
   if(hs.Count==0)continue;var top=hs[0];var pave=hs.FirstOrDefault(h=>h.collider.name.Contains("driving surface"));
   if(pave.collider&&top.collider!=pave.collider&&top.point.y>pave.point.y+.005f){string k=top.collider.name;counts[k]=counts.TryGetValue(k,out int n)?n+1:1;
    rows.Add($"{x:F2},{z:F2}: ABOVE pavement by {top.point.y-pave.point.y:F3} m: {top.collider.name} n{top.normal.y:F2} tri{top.triangleIndex} | pave {pave.point.y:F3}");}}
  // Char map: P pavement on top, S seam support on top (pavement absent above it), other letter = other surface, . none.
  var map=new List<string>();var legend=new Dictionary<string,char>();for(float z=b[3];z>=b[1];z-=step){var sb=new System.Text.StringBuilder($"{z,8:F2} ");
   for(float x=b[0];x<=b[2];x+=step){var o=new Vector3(x,yr+3,z);if(!Physics.Raycast(o,Vector3.down,out var h,6,~0,QueryTriggerInteraction.Ignore)){sb.Append('.');continue;}var n=h.collider.name;
    char c=n.Contains("driving surface")?'P':n.Contains("seam support")?'S':legend.TryGetValue(n,out var lc)?lc:(legend[n]=(char)('a'+legend.Count));sb.Append(c);}map.Add(sb.ToString());}
  File.WriteAllLines(outDir+"/finemap.txt",map.Concat(legend.Select(kv=>kv.Value+" = "+kv.Key)));
  File.WriteAllLines(outDir+"/fine.txt",new[]{$"{a[0]} box {a[1]} step {step}: cells where something stands above the pavement: "+string.Join(", ",counts.Select(kv=>kv.Key+" "+kv.Value))}.Concat(rows));EditorApplication.Exit(0);}
}
