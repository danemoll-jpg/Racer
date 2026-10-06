using System;using System.IO;using System.Linq;using System.Text;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;using Racer;
// 0.86 Part C (read only): every solid thing in a region that is not ground: colliders (not triggers, not the big ground
// meshes) with bounds, kind (trunk / building / prop / ground), plus every ShallowWater footprint (corners, surface) and
// every WoodlandRoute's points. OBST="x0,z0,x1,z1". Writes obstacles-<scene>.tsv, water-<scene>.tsv, branches-<scene>.tsv.
public static class Report086Obstacles {
 static string P(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n;}
 public static void Run(){var o=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(o);var f=Environment.GetEnvironmentVariable("OBST").Split(',').Select(x=>float.Parse(x,System.Globalization.CultureInfo.InvariantCulture)).ToArray();
  foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){var s=EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");var roots=s.GetRootGameObjects();
   var sb=new StringBuilder("kind\tpath\tcx\tcy\tcz\tsx\tsy\tsz\tminY\n");
   foreach(var c in roots.SelectMany(g=>g.GetComponentsInChildren<Collider>(true))){if(c.isTrigger||!c.enabled||!c.gameObject.activeInHierarchy)continue;var b=c.bounds;if(b.max.x<f[0]||b.min.x>f[2]||b.max.z<f[1]||b.min.z>f[3])continue;
    string n=c.name.ToLowerInvariant();string kind=n.Contains("trunk")||n.Contains("tree")?"trunk":c is MeshCollider&&(b.size.x>40||b.size.z>40)?"ground":n.StartsWith("ground")?"ground":b.size.y>2.2f&&(b.size.x>3||b.size.z>3)?"building":"prop";
    sb.Append($"{kind}\t{P(c.transform)}\t{b.center.x:F1}\t{b.center.y:F1}\t{b.center.z:F1}\t{b.size.x:F1}\t{b.size.y:F1}\t{b.size.z:F1}\t{b.min.y:F1}\n");}
   File.WriteAllText($"{o}/obstacles-{scene}.tsv",sb.ToString());
   var w=new StringBuilder("name\tround\tsurface\tcorners\n");
   foreach(var x in roots.SelectMany(g=>g.GetComponentsInChildren<ShallowWater>(true))){var t=x.transform;var cs=new[]{new Vector3(-.5f,0,-.5f),new Vector3(.5f,0,-.5f),new Vector3(.5f,0,.5f),new Vector3(-.5f,0,.5f)}.Select(t.TransformPoint);
    w.Append($"{P(t)}\t{x.round}\t{x.Surface:F2}\t{string.Join(" ",cs.Select(p=>$"{p.x:F1},{p.z:F1}"))}\t{t.position.x:F1},{t.position.z:F1}\t{t.lossyScale.x:F1}x{t.lossyScale.z:F1}\t{t.eulerAngles.y:F0}\n");}
   File.WriteAllText($"{o}/water-{scene}.tsv",w.ToString());
   var br=new StringBuilder();foreach(var b in roots.SelectMany(g=>g.GetComponentsInChildren<WoodlandRoute>(true))){b.Initialize();br.Append(b.title+"\t"+string.Join(" ",b.points.Select(p=>$"{p.x:F1},{p.y:F1},{p.z:F1}"))+"\n");}
   var rd=roots.SelectMany(g=>g.GetComponentsInChildren<RaceDirector>(true)).First().road;rd.Initialize();br.Append("Main\t"+string.Join(" ",Enumerable.Range(0,(int)(rd.Length/2)).Select(i=>{var p=rd.At(i*2,out _);return $"{p.x:F1},{p.y:F1},{p.z:F1}";}))+"\n");
   File.WriteAllText($"{o}/branches-{scene}.tsv",br.ToString());}
  EditorApplication.Exit(0);}
}
