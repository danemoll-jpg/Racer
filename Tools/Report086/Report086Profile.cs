using System;using System.IO;using System.Linq;using System.Text;using System.Collections.Generic;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;using Racer;
// 0.86 Part D (read only): the driving surface along a branch's centre line: PROFILE="title,from,to,step[,lateral]".
// Every step: branch point (authored y), the surface at the centre and at -lat / +lat (nearest the authored height),
// collider names, grade over +-2 m and vertical curvature over +-6 m (dip > 0). Also the main's nearest station.
public static class Report086Profile {
 static bool Skip(Collider c)=>c.isTrigger||c.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0||c.name.IndexOf("tree",StringComparison.OrdinalIgnoreCase)>=0||(c.attachedRigidbody&&!c.attachedRigidbody.isKinematic);
 public static float Surf(Vector3 p,out Collider what){what=null;float best=float.NaN,bd=float.MaxValue;foreach(var h in Physics.RaycastAll(new Vector3(p.x,p.y+15,p.z),Vector3.down,40,~0,QueryTriggerInteraction.Ignore)){if(Skip(h.collider))continue;float d=Mathf.Abs(h.point.y-p.y);if(h.point.y>p.y+4)d+=100;if(d<bd){bd=d;best=h.point.y;what=h.collider;}}return best;}
 public static void Run(){var o=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(o);var a=Environment.GetEnvironmentVariable("PROFILE").Split(',');var inv=System.Globalization.CultureInfo.InvariantCulture;
  string title=a[0];float from=float.Parse(a[1],inv),to=float.Parse(a[2],inv),step=float.Parse(a[3],inv),lat=a.Length>4?float.Parse(a[4],inv):2;
  foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){var s=EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();var roots=s.GetRootGameObjects();
   var rd=roots.SelectMany(g=>g.GetComponentsInChildren<RaceDirector>(true)).First().road;rd.Initialize();
   var b=roots.SelectMany(g=>g.GetComponentsInChildren<WoodlandRoute>(true)).First(x=>x.title==title);b.Initialize();
   var sb=new StringBuilder($"{scene} '{title}' length {b.Length:F1} hw {b.halfWidth} entry {b.entryRoad:F1} exit {b.exitRoad:F1}\n s,x,z,authY,centre,left,right,grade%,curv,collider\n");var names=new Dictionary<Collider,int>();
   float S(float st){var p=b.At(st,out _);return Surf(p,out _);}
   for(float st=from;st<=to+.001f;st+=step){var p=b.At(st,out var f);f.y=0;f.Normalize();var r=Vector3.Cross(Vector3.up,f);float c=Surf(p,out var col);float l=Surf(p-r*lat,out _),rr=Surf(p+r*lat,out _);
    float g=(S(st+2)-S(st-2))/4f*100;float k=(S(st+6)+S(st-6)-2*c)/36f;if(col&&!names.ContainsKey(col))names[col]=names.Count;
    sb.AppendLine($"{st:F1},{p.x:F2},{p.z:F2},{p.y:F2},{c:F2},{l:F2},{rr:F2},{g:F1},{k:F3},{(col?names[col]:-1)}");}
   foreach(var kv in names)sb.AppendLine($"collider {kv.Value}: {kv.Key.transform.parent?.name}/{kv.Key.name} {(kv.Key is MeshCollider mc&&mc.sharedMesh?AssetDatabase.GetAssetPath(mc.sharedMesh)+" shared-by-renderer "+(kv.Key.GetComponent<MeshFilter>()?.sharedMesh==mc.sharedMesh):kv.Key.GetType().Name)}");
   File.WriteAllText($"{o}/profile-{scene}-{title.Replace(' ','_')}.csv",sb.ToString());}
  EditorApplication.Exit(0);}
}
