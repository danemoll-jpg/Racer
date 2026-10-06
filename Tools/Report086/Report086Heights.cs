using System;using System.IO;using System.Linq;using System.Text;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;
// 0.86 (read only): bare ground heights (topmost collider, ignoring the House 3 pool group, trees, triggers) on a grid:
// HGRID="x0,z0,x1,z1,cell". Writes heights-<scene>.csv (rows z descending, x ascending) and a text table every 1 m.
public static class Report086Heights {
 static bool Skip(Collider c)=>c.isTrigger||c.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0||c.name.IndexOf("tree",StringComparison.OrdinalIgnoreCase)>=0||(c.attachedRigidbody&&!c.attachedRigidbody.isKinematic)||(Environment.GetEnvironmentVariable("HGRID_POOL")!="1"&&c.transform.parent&&c.transform.parent.name=="House 3 pool and lake");
 public static float Top(float x,float z,float from=200){float best=float.NaN;foreach(var h in Physics.RaycastAll(new Vector3(x,from,z),Vector3.down,from+100,~0,QueryTriggerInteraction.Ignore)){if(Skip(h.collider))continue;if(float.IsNaN(best)||h.point.y>best)best=h.point.y;}return best;}
 public static void Run(){var o=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(o);var f=Environment.GetEnvironmentVariable("HGRID").Split(',').Select(x=>float.Parse(x,System.Globalization.CultureInfo.InvariantCulture)).ToArray();
  foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();
   var csv=new StringBuilder();var tab=new StringBuilder("      x:"+string.Concat(Enumerable.Range(0,(int)((f[2]-f[0])/1)+1).Select(i=>$"{f[0]+i,6:F0}"))+"\n");
   for(float z=f[3];z>=f[1]-.001f;z-=f[4]){var row=new StringBuilder();for(float x=f[0];x<=f[2]+.001f;x+=f[4]){float y=Top(x,z);row.Append(float.IsNaN(y)?"":y.ToString("F2")).Append(',');}csv.AppendLine($"{z:F2},"+row);}
   for(float z=f[3];z>=f[1]-.001f;z-=1){tab.Append($"{z,7:F0}:");for(float x=f[0];x<=f[2]+.001f;x+=1){float y=Top(x,z);tab.Append(float.IsNaN(y)?"     -":$"{y,6:F2}");}tab.AppendLine();}
   File.WriteAllText($"{o}/heights-{scene}.csv",$"# x0 {f[0]} x1 {f[2]} cell {f[4]}\n"+csv);File.WriteAllText($"{o}/heights-{scene}.txt",tab.ToString());}
  EditorApplication.Exit(0);}
}
