using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
// 0.71 read-only: top-down area dump. PROBE_AREA="scene|x0,z0,x1,z1|cell". Writes area-grid.csv (x,z,topY,collider) and
// area-routes.csv (route,s,x,y,z,rightX,rightZ,halfWidth) for every route sample inside the box.
public static class Report071Area {
 public static void Run(){var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(outDir);var a=Environment.GetEnvironmentVariable("PROBE_AREA").Split('|');
  EditorSceneManager.OpenScene("Assets/Scenes/"+a[0]+".unity");Physics.SyncTransforms();var b=a[1].Split(',').Select(float.Parse).ToArray();float cell=float.Parse(a[2]);
  var g=new List<string>{"x,z,y,collider"};
  for(float x=b[0];x<=b[2];x+=cell)for(float z=b[1];z<=b[3];z+=cell){var o=new Vector3(x,600,z);for(int k=0;k<8&&Physics.Raycast(o,Vector3.down,out var h,1200,~0,QueryTriggerInteraction.Ignore);k++){if(h.normal.y>0&&!h.collider.attachedRigidbody){g.Add($"{x:F1},{z:F1},{h.point.y:F2},{h.collider.name.Replace(',',' ')}");break;}o=h.point+Vector3.down*.05f;}}
  File.WriteAllLines(outDir+"/area-grid.csv",g);
  var race=Object.FindAnyObjectByType<RaceDirector>(FindObjectsInactive.Include);var road=race.road;road.Initialize();var r=new List<string>{"route,s,x,y,z,rx,rz,hw"};
  void Add(string n,float len,Func<float,(Vector3 p,Vector3 f)> at,Func<float,float> hw){for(float s=0;s<=len;s+=1){var (p,f)=at(s);if(p.x<b[0]||p.x>b[2]||p.z<b[1]||p.z>b[3])continue;var rt=Vector3.Cross(Vector3.up,Vector3.ProjectOnPlane(f,Vector3.up).normalized);r.Add($"{n},{s},{p.x:F2},{p.y:F2},{p.z:F2},{rt.x:F3},{rt.z:F3},{hw(s):F2}");}}
  Add("Main",road.Length,s=>{var p=road.At(s,out var f);return(p,f);},s=>road.HalfWidth(s));
  foreach(var w in Object.FindObjectsByType<WoodlandRoute>().Where(w=>w.gameObject.activeInHierarchy)){w.Initialize();var ww=w;Add(w.title,w.Length,s=>{var p=ww.At(s,out var f);return(p,f);},s=>ww.halfWidth);}
  File.WriteAllLines(outDir+"/area-routes.csv",r);EditorApplication.Exit(0);}
}
