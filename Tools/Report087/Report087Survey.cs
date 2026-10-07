using System;using System.IO;using System.Linq;using System.Text;using System.Collections.Generic;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;using Racer;
// 0.87 Part B (read only): the Summit Climb hillside in LakeWoods. SURVEY="x0,z0,x1,z1". Writes:
//  heights-<scene>.csv  ground (Ground_* colliders) on a 1 m grid
//  trunks-<scene>.tsv   tree/trunk colliders (centre, size, bottom)
//  drawn-<scene>.tsv    drawn tree pieces in batch meshes (base, size, renderer)
//  solids-<scene>.tsv   every other collider / renderer in the region (not ground, not trees)
//  main-<scene>.tsv     main every 2 m (station, point, half-width), gates, branches
//  tiles-<scene>.txt    ground tiles in the region, asset, vertex spacing, scenes sharing the asset
public static class Report087Survey {
 static string P(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n;}
 static bool Tree(string n){n=n.ToLowerInvariant();return n.Contains("trunk")||n.Contains("tree");}
 public static void Run(){var o=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(o);var inv=System.Globalization.CultureInfo.InvariantCulture;
  var f=Environment.GetEnvironmentVariable("SURVEY").Split(',').Select(x=>float.Parse(x,inv)).ToArray();var zone=new Bounds();zone.SetMinMax(new Vector3(f[0],-100,f[1]),new Vector3(f[2],400,f[3]));
  try{foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){var s=EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();var roots=s.GetRootGameObjects();
   var h=new StringBuilder($"x0 {f[0]} z0 {f[1]} cell 1 w {(int)(f[2]-f[0])} h {(int)(f[3]-f[1])}\n");
   for(float z=f[1];z<f[3];z+=1){var row=new List<string>();for(float x=f[0];x<f[2];x+=1){float best=float.NaN;foreach(var hit in Physics.RaycastAll(new Vector3(x+.5f,400,z+.5f),Vector3.down,800,~0,QueryTriggerInteraction.Ignore)){if(!hit.collider.name.StartsWith("Ground"))continue;if(float.IsNaN(best)||hit.point.y>best)best=hit.point.y;}row.Add(float.IsNaN(best)?"":best.ToString("F2",inv));}h.AppendLine(string.Join(",",row));}
   File.WriteAllText($"{o}/heights-{scene}.csv",h.ToString());
   var tr=new StringBuilder("path\tcx\tcy\tcz\tsx\tsy\tsz\tbottom\ttype\n");var so=new StringBuilder("kind\tpath\tcx\tcy\tcz\tsx\tsy\tsz\tminY\tlayer\n");
   foreach(var c in roots.SelectMany(g=>g.GetComponentsInChildren<Collider>(true))){var b=c.bounds;if(!b.Intersects(zone))continue;
    if(Tree(c.name)){tr.Append($"{P(c.transform)}\t{b.center.x:F2}\t{b.center.y:F2}\t{b.center.z:F2}\t{b.size.x:F2}\t{b.size.y:F2}\t{b.size.z:F2}\t{b.min.y:F2}\t{c.GetType().Name}{(c.enabled&&c.gameObject.activeInHierarchy?"":" (off)")}\n");continue;}
    if(c.name.StartsWith("Ground")&&(b.size.x>60||b.size.z>60))continue;
    so.Append($"collider{(c.isTrigger?" trigger":"")}\t{P(c.transform)}\t{b.center.x:F1}\t{b.center.y:F1}\t{b.center.z:F1}\t{b.size.x:F1}\t{b.size.y:F1}\t{b.size.z:F1}\t{b.min.y:F1}\t{c.gameObject.layer}\n");}
   var dr=new StringBuilder("renderer\tasset\tverts\tbx\tby\tbz\tsx\tsy\tsz\n");
   foreach(var r in roots.SelectMany(g=>g.GetComponentsInChildren<MeshRenderer>(true))){if(!r.bounds.Intersects(zone))continue;var mf=r.GetComponent<MeshFilter>();if(!mf||!mf.sharedMesh)continue;var m=mf.sharedMesh;if(r.name.StartsWith("Ground"))continue;
    if(r.bounds.size.x<40&&r.bounds.size.z<40){var b=r.bounds;so.Append($"renderer\t{P(r.transform)}\t{b.center.x:F1}\t{b.center.y:F1}\t{b.center.z:F1}\t{b.size.x:F1}\t{b.size.y:F1}\t{b.size.z:F1}\t{b.min.y:F1}\t{AssetDatabase.GetAssetPath(m)}\n");continue;}
    if(!m.isReadable){so.Append($"big unreadable\t{P(r.transform)}\t{AssetDatabase.GetAssetPath(m)}\n");continue;}
    var wv=m.vertices.Select(x=>r.transform.TransformPoint(x)).ToArray();var par=Enumerable.Range(0,wv.Length).ToArray();int Find(int x){while(par[x]!=x){par[x]=par[par[x]];x=par[x];}return x;}void U(int a,int c){a=Find(a);c=Find(c);if(a!=c)par[a]=c;}
    var tri=m.triangles;for(int k=0;k<tri.Length;k+=3){U(tri[k],tri[k+1]);U(tri[k],tri[k+2]);}
    var byPos=new Dictionary<Vector3Int,int>();for(int i=0;i<wv.Length;i++){var key=Vector3Int.RoundToInt(wv[i]*200);if(byPos.TryGetValue(key,out var j))U(i,j);else byPos[key]=i;}
    var used=new HashSet<int>(tri);
    foreach(var grp in Enumerable.Range(0,wv.Length).Where(used.Contains).GroupBy(Find)){var idx=grp.ToArray();var ext=new Bounds(wv[idx[0]],Vector3.zero);foreach(var i in idx)ext.Encapsulate(wv[i]);
     if(!zone.Contains(new Vector3(ext.center.x,0,ext.center.z)))continue;dr.Append($"{P(r.transform)}\t{AssetDatabase.GetAssetPath(m)}\t{idx.Length}\t{ext.center.x:F2}\t{ext.min.y:F2}\t{ext.center.z:F2}\t{ext.size.x:F2}\t{ext.size.y:F2}\t{ext.size.z:F2}\n");}}
   File.WriteAllText($"{o}/trunks-{scene}.tsv",tr.ToString());File.WriteAllText($"{o}/solids-{scene}.tsv",so.ToString());File.WriteAllText($"{o}/drawn-{scene}.tsv",dr.ToString());
   var race=roots.SelectMany(g=>g.GetComponentsInChildren<RaceDirector>(true)).First();var rd=race.road;rd.Initialize();var mn=new StringBuilder($"course {race.courseId} length {rd.Length:F1}\n");
   for(float st=0;st<rd.Length;st+=2){var p=rd.At(st,out _);mn.Append($"M\t{st:F0}\t{p.x:F2}\t{p.y:F2}\t{p.z:F2}\t{rd.HalfWidth(st):F2}\n");}
   for(int i=0;i<race.gates.Length;i++){var g=race.gates[i];var gg=g.GetComponent<RaceGate>();mn.Append($"G\t{i}\t{g.transform.position.x:F2}\t{g.transform.position.y:F2}\t{g.transform.position.z:F2}\t{(gg?gg.halfWidth:0):F2}\t{rd.Project(g.transform.position,out _):F1}\t{g.transform.eulerAngles.y:F0}\n");}
   foreach(var b in roots.SelectMany(g=>g.GetComponentsInChildren<WoodlandRoute>(true))){b.Initialize();mn.Append($"B\t{b.title}\t{b.entryRoad:F1}\t{b.exitRoad:F1}\t{b.halfWidth}\t{b.Length:F1}\t{string.Join(",",b.bypassedGates??new int[0])}\t{b.aiValidated}\t{P(b.transform)}\n");}
   var fl=roots.SelectMany(g=>g.GetComponentsInChildren<ForestLayout>(true)).FirstOrDefault();if(fl)mn.Append($"J\t{string.Join(" ",fl.jumpStarts)}\t{string.Join(" ",fl.jumpEnds)}\t{string.Join("|",fl.jumpNames)}\n");
   File.WriteAllText($"{o}/main-{scene}.tsv",mn.ToString());
   var ti=new StringBuilder();foreach(var mc in roots.SelectMany(g=>g.GetComponentsInChildren<MeshCollider>(true))){if(!mc.name.StartsWith("Ground_")||!mc.sharedMesh||!mc.bounds.Intersects(zone))continue;var m=mc.sharedMesh;var path=AssetDatabase.GetAssetPath(m);
    var xs=m.vertices.Select(v=>Mathf.Round(mc.transform.TransformPoint(v).x*100)/100).Distinct().OrderBy(x=>x).ToArray();float sp=xs.Length>1?xs.Zip(xs.Skip(1),(a,b)=>b-a).OrderBy(d=>d).ElementAt(xs.Length/2):0;
    var mf=mc.GetComponent<MeshFilter>();var mr=mc.GetComponent<MeshRenderer>();
    ti.AppendLine($"{P(mc.transform)} {path} verts {m.vertexCount} bounds {mc.bounds.min}-{mc.bounds.max} median x spacing {sp:F2} renderer-shares {(mf&&mf.sharedMesh==m)} material {(mr?mr.sharedMaterial?.name:"")} colors {m.colors.Length>0} readable {m.isReadable} scenes {string.Join(",",AssetDatabase.FindAssets("t:Scene",new[]{"Assets/Scenes"}).Select(AssetDatabase.GUIDToAssetPath).Where(sp2=>AssetDatabase.GetDependencies(sp2,false).Contains(path)).Select(Path.GetFileNameWithoutExtension))}");}
   File.WriteAllText($"{o}/tiles-{scene}.txt",ti.ToString());}
  }catch(Exception e){File.WriteAllText(o+"/survey-error.txt",e.ToString());}
  EditorApplication.Exit(0);}
}
