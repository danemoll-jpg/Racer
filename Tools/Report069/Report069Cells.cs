using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Object=UnityEngine.Object;
// 0.69 read-only: every upward collider surface (y, name) in 0.5 m cells of PROBE_CELLS="scene|x,y,z|radius;..." between
// y-6 and y+6, plus whether a visible renderer without collider covers the cell. -> cells-<scene>-<x>_<z>.csv
public static class Report069Cells {
 public static void Run(){var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(outDir);Physics.queriesHitBackfaces=false;
  foreach(var g in Environment.GetEnvironmentVariable("PROBE_CELLS").Split(';').Select(x=>x.Split('|')).GroupBy(a=>a[0])){EditorSceneManager.OpenScene("Assets/Scenes/"+g.Key+".unity");Physics.SyncTransforms();
   foreach(var a in g){var c=a[1].Split(',').Select(float.Parse).ToArray();float r=float.Parse(a[2]);var rows=new List<string>{"x,z,surfaces(y:name:ny|...)"};
    for(float x=c[0]-r;x<=c[0]+r;x+=.5f)for(float z=c[2]-r;z<=c[2]+r;z+=.5f){var o=new Vector3(x,c[1]+6,z);float depth=12;var hs=new List<string>();
     for(int k=0;k<12&&depth>0&&Physics.Raycast(o,Vector3.down,out var h,depth,~0,QueryTriggerInteraction.Ignore);k++){hs.Add($"{h.point.y:F2}:{h.collider.name.Replace(',',' ')}:{h.normal.y:F2}");depth-=h.distance+.01f;o=h.point+Vector3.down*.01f;}
     rows.Add($"{x:F1},{z:F1},{string.Join("|",hs)}");}
    File.WriteAllLines($"{outDir}/cells-{g.Key}-{c[0]:F0}_{c[2]:F0}.csv",rows);}}
  EditorApplication.Exit(0);}
}
