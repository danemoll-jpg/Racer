using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Object=UnityEngine.Object;
// 0.72 read-only (Part A): batched tree/foliage meshes around the 0.71 box, split into connected pieces. For each piece: its
// height range, the ground under its lowest point, and the nearest trunk collider. Lists pieces with no trunk within 3 m
// (orphans), pieces whose bottom is buried more than 1 m or that hang more than 1.5 m above the ground with no trunk.
public static class Report072Trees {
 static string Path(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n.Replace('\n','/');}
 public static void Run(){var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(outDir);float pad=float.TryParse(Environment.GetEnvironmentVariable("PROBE_PAD"),out var pp)?pp:20;
  foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();var rows=new List<string>{"SCENE "+scene};
   var launch=Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).FirstOrDefault(t=>t.name=="CR094 summit launch");
   bool InBox(Vector3 w){var q=launch.InverseTransformPoint(w);return q.z>=316-pad&&q.z<=545+pad&&Mathf.Abs(q.x)<=50+pad;}
   var trunks=Object.FindObjectsByType<Collider>(FindObjectsSortMode.None).Where(c=>c.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0&&InBox(c.bounds.center)).Select(c=>c.bounds).ToList();
   float Ground(Vector3 p){foreach(var h in Physics.RaycastAll(new Vector3(p.x,600,p.z),Vector3.down,1200,~0,QueryTriggerInteraction.Ignore).OrderBy(h=>h.distance))if(h.normal.y>0&&!h.collider.attachedRigidbody&&h.collider.name.StartsWith("Ground"))return h.point.y;return float.NaN;}
   foreach(var mf in Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None)){var path=Path(mf.transform);if(!(path.Contains("tree")||path.Contains("Tree")||path.Contains("foliage")||path.Contains("canopy")||path.Contains("Woods")||path.Contains("woodland")))continue;
    var m=mf.sharedMesh;if(!m||!m.isReadable)continue;var r=mf.GetComponent<Renderer>();if(!r||!r.enabled||!mf.gameObject.activeInHierarchy)continue;
    var vs=m.vertices.Select(v=>mf.transform.TransformPoint(v)).ToArray();var t=m.triangles;
    // union-find over vertex positions
    var key=new Dictionary<Vector3Int,int>();var parent=new List<int>();int Find(int a){while(parent[a]!=a){parent[a]=parent[parent[a]];a=parent[a];}return a;}
    var vid=new int[vs.Length];for(int i=0;i<vs.Length;i++){var k=Vector3Int.RoundToInt(vs[i]*100);if(!key.TryGetValue(k,out var id)){id=parent.Count;parent.Add(id);key[k]=id;}vid[i]=id;}
    for(int i=0;i<t.Length;i+=3){int a=Find(vid[t[i]]),b=Find(vid[t[i+1]]),c=Find(vid[t[i+2]]);parent[b]=a;parent[Find(c)]=a;}
    var groups=new Dictionary<int,List<int>>();for(int i=0;i<t.Length;i+=3){if(!InBox(vs[t[i]]))continue;int g=Find(vid[t[i]]);if(!groups.TryGetValue(g,out var l))groups[g]=l=new();l.Add(i/3);}
    foreach(var g in groups.Values){var pts=g.SelectMany(q=>new[]{vs[t[3*q]],vs[t[3*q+1]],vs[t[3*q+2]]}).ToArray();var b=new Bounds(pts[0],Vector3.zero);foreach(var p in pts)b.Encapsulate(p);
     var low=pts.OrderBy(p=>p.y).First();float gr=Ground(b.center);float near=trunks.Count==0?999:trunks.Min(tb=>new Vector2(tb.center.x-b.center.x,tb.center.z-b.center.z).magnitude);
     bool trunkLike=b.size.y>b.size.x*1.6f&&b.size.x<1.6f;string why=null;
     if(!trunkLike&&near>3)why="ORPHAN (no trunk within 3 m)";else if(b.min.y<gr-1)why="BURIED";else if(!trunkLike&&b.min.y>gr+6&&near>3)why="FLOATING";
     if(why!=null)rows.Add($"{why} {path} tris {g.Count} bounds min {b.min:F1} max {b.max:F1} ground {gr:F2} nearest trunk {near:F1} m launch {launch.InverseTransformPoint(b.center):F0}");}}
   File.WriteAllLines($"{outDir}/trees-{scene}.txt",rows);}
  EditorApplication.Exit(0);}
}
