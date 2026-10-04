using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Object=UnityEngine.Object;
// 0.74 read-only: tree pieces (connected parts of batched tree meshes) and trunk/tree colliders within r of a point, with the
// ground name under each. PROBE_TREES="scene|x,y,z|r;..."
public static class Report074Trees {
 static string Path(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n.Replace('\n','/');}
 public static List<(MeshFilter mf,List<int> tris,Bounds b)> Pieces(Vector3 p,float r){var res=new List<(MeshFilter,List<int>,Bounds)>();
  foreach(var mf in Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None)){var path=Path(mf.transform);if(!(path.Contains("tree")||path.Contains("Tree")||path.Contains("foliage")||path.Contains("canopy")||path.Contains("Woods")||path.Contains("woodland")))continue;
   var m=mf.sharedMesh;if(!m||!m.isReadable)continue;var rd=mf.GetComponent<Renderer>();if(!rd||!rd.enabled||!mf.gameObject.activeInHierarchy)continue;
   var bb=rd.bounds;if(Mathf.Sqrt(new Bounds(new Vector3(bb.center.x,p.y,bb.center.z),new Vector3(bb.size.x,1,bb.size.z)).SqrDistance(p))>r)continue;
   var vs=m.vertices.Select(v=>mf.transform.TransformPoint(v)).ToArray();var t=m.triangles;
   var key=new Dictionary<Vector3Int,int>();var parent=new List<int>();int Find(int a){while(parent[a]!=a){parent[a]=parent[parent[a]];a=parent[a];}return a;}
   var vid=new int[vs.Length];for(int i=0;i<vs.Length;i++){var k=Vector3Int.RoundToInt(vs[i]*100);if(!key.TryGetValue(k,out var id)){id=parent.Count;parent.Add(id);key[k]=id;}vid[i]=id;}
   for(int i=0;i<t.Length;i+=3){int a=Find(vid[t[i]]),b=Find(vid[t[i+1]]),c=Find(vid[t[i+2]]);parent[b]=a;parent[Find(c)]=a;}
   var groups=new Dictionary<int,List<int>>();for(int i=0;i<t.Length;i+=3){int g=Find(vid[t[i]]);if(!groups.TryGetValue(g,out var l))groups[g]=l=new();l.Add(i/3);}
   foreach(var g in groups.Values){var b=new Bounds(vs[t[3*g[0]]],Vector3.zero);foreach(var q in g)for(int j=0;j<3;j++)b.Encapsulate(vs[t[3*q+j]]);
    if(new Vector2(b.center.x-p.x,b.center.z-p.z).magnitude>r)continue;res.Add((mf,g,b));}}
  return res;}
 public static void Run(){var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(outDir);var rows=new List<string>();string open=null;
  foreach(var spec in Environment.GetEnvironmentVariable("PROBE_TREES").Split(';')){var a=spec.Split('|');if(a[0]!=open){EditorSceneManager.OpenScene("Assets/Scenes/"+a[0]+".unity");open=a[0];Physics.SyncTransforms();}
   var v=a[1].Split(',').Select(float.Parse).ToArray();var p=new Vector3(v[0],v[1],v[2]);float r=float.Parse(a[2]);rows.Add($"TREES {a[0]} {p} r {r}");
   foreach(var x in Pieces(p,r))rows.Add($"PIECE {Path(x.mf.transform)} tris {x.tris.Count} min {x.b.min:F2} max {x.b.max:F2} c {x.b.center:F2}");
   foreach(var c in Object.FindObjectsByType<Collider>(FindObjectsSortMode.None)){var b=c.bounds;if(new Vector2(b.center.x-p.x,b.center.z-p.z).magnitude>r)continue;if(b.size.x>4||b.size.z>4)continue;
    rows.Add($"COL {Path(c.transform)} {c.GetType().Name} {(c.enabled?"on":"off")} c {b.center:F2} s {b.size:F2}");}
   for(float dz=-r;dz<=r;dz+=2)for(float dx=-r;dx<=r;dx+=2){var o=new Vector3(p.x+dx,p.y+40,p.z+dz);if(Physics.Raycast(o,Vector3.down,out var h,90,~0,QueryTriggerInteraction.Ignore))rows.Add($"  g {o.x:F0},{o.z:F0} {h.point.y:F2} {h.collider.name}");}}
  File.WriteAllLines(outDir+"/trees.txt",rows);EditorApplication.Exit(0);}
}
