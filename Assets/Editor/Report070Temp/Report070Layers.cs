using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Object=UnityEngine.Object;
// 0.70 read-only: surface layer map around a point. Each 0.5 m cell lists every upward surface within +-4 m of the
// reference height (y, collider) so stacked pavement ribbons / terrain over pavement can be read. PROBE_LAYERS="scene|x,y,z|radius;..."
public static class Report070Layers {
 public static void Run(){var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(outDir);
  foreach(var spec in Environment.GetEnvironmentVariable("PROBE_LAYERS").Split(';')){var a=spec.Split('|');var v=a[1].Split(',').Select(float.Parse).ToArray();var p=new Vector3(v[0],v[1],v[2]);float rad=float.Parse(a[2]);
   EditorSceneManager.OpenScene("Assets/Scenes/"+a[0]+".unity");Physics.queriesHitBackfaces=false;var rows=new List<string>{$"{a[0]} around {p} r={rad}: x,z: surfaces top->down (y name normalY)"};
   var names=new Dictionary<string,char>();char Code(string n){if(!names.TryGetValue(n,out var c)){c=(char)('A'+names.Count);names[n]=c;}return c;}
   var map=new List<string>();
   for(float z=p.z+rad;z>=p.z-rad;z-=.5f){var line=new System.Text.StringBuilder($"{z,8:F1} ");
    for(float x=p.x-rad;x<=p.x+rad;x+=.5f){var o=new Vector3(x,p.y+float.Parse(Environment.GetEnvironmentVariable("PROBE_DEPTH")??"8")/2,z);var hs=new List<RaycastHit>();float depth=float.Parse(Environment.GetEnvironmentVariable("PROBE_DEPTH")??"8");for(int k=0;k<10&&depth>0&&Physics.Raycast(o,Vector3.down,out var h,depth,~0,QueryTriggerInteraction.Ignore);k++){if(h.normal.y>0)hs.Add(h);depth-=h.distance+.01f;o=h.point+Vector3.down*.01f;}
     rows.Add($"{x:F1},{z:F1}: "+string.Join(" | ",hs.Select(h=>$"{h.point.y:F2} {h.collider.name} n{h.normal.y:F2}")));
     line.Append(hs.Count==0?' ':Code(hs[0].collider.name));}
    map.Add(line.ToString());}
   foreach(var w in Object.FindObjectsByType<Racer.ShallowWater>())rows.Add($"WATER {w.name} pos {w.transform.position} scale {w.transform.lossyScale} surface {w.Surface:F2}");
   File.WriteAllLines($"{outDir}/layers-{a[0]}-{p.x:F0}_{p.z:F0}.txt",rows.Concat(new[]{"TOP MAP (x left->right, z top->bottom):"}).Concat(map).Concat(names.Select(kv=>$"{kv.Value} = {kv.Key}")));}
  EditorApplication.Exit(0);}
}
