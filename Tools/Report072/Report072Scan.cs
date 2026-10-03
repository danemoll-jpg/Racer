using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Object=UnityEngine.Object;
// 0.72 read-only (Part A): scan the 0.71 reshaped box (launch frame of CR094 summit launch, s 316-545, |x| <= 50) plus a margin
// on a 1 m grid for tears: the top collision surface seen from behind (fold), terrain poking through the ribbon, a step of
// more than 0.45 m to the next grid point, slopes steeper than 50 deg, holes, and places where the visible surface differs
// from the collision surface by more than 0.25 m. Anomalies are clustered (4 m) and listed with world coordinates.
// PROBE_SCENES scenes; PROBE_PAD margin (default 14).
public static class Report072Scan {
 const int Ghost=31;
 static string Path(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n.Replace('\n','/');}
 static bool Veg(string p)=>p.Contains("tree")||p.Contains("Tree")||p.Contains("foliage")||p.Contains("canopy")||p.Contains("trunk")||p.Contains("Woods")||p.Contains("woodland")||p.Contains("Forest")||p.Contains("timber")||p.Contains("Acorn")||p.Contains("sign")||p.Contains("Sign")||p.Contains("Wildlife")||p.Contains("ghost:");
 public static void Run(){var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(outDir);float pad=float.TryParse(Environment.GetEnvironmentVariable("PROBE_PAD"),out var pp)?pp:14;
  foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");var rows=new List<string>{"SCENE "+scene};
   var launch=Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).FirstOrDefault(t=>t.name=="CR094 summit launch");
   if(!launch||!launch.gameObject.activeInHierarchy){rows.Add("no active launch");File.WriteAllLines($"{outDir}/scan-{scene}.txt",rows);continue;}
   float S0=316-pad,S1=545+pad,XW=50+pad;var corners=new[]{launch.TransformPoint(-XW,0,S0),launch.TransformPoint(XW,0,S0),launch.TransformPoint(-XW,0,S1),launch.TransformPoint(XW,0,S1)};
   var area=new Bounds(corners[0],Vector3.zero);foreach(var c in corners)area.Encapsulate(c);area.Expand(new Vector3(4,2000,4));
   var ghosts=new List<GameObject>();
   foreach(var mr in Object.FindObjectsByType<MeshRenderer>()){if(!mr.enabled||!mr.bounds.Intersects(area))continue;var mf=mr.GetComponent<MeshFilter>();if(!mf||!mf.sharedMesh)continue;var path=Path(mr.transform);if(Veg(path))continue;
    var go=new GameObject("ghost:"+path);go.layer=Ghost;go.transform.SetPositionAndRotation(mr.transform.position,mr.transform.rotation);go.transform.localScale=mr.transform.lossyScale;go.AddComponent<MeshCollider>().sharedMesh=mf.sharedMesh;ghosts.Add(go);}
   Physics.SyncTransforms();Physics.queriesHitBackfaces=true;
   List<(Vector3 p,string kind,string info)> Grid(bool visual,out float[,] top){
   int ns=(int)(S1-S0)+1,nx=(int)(2*XW)+1;top=new float[ns,nx];var tp=top;var ny=new float[ns,nx];var name=new string[ns,nx];var anomalies=new List<(Vector3 p,string kind,string info)>();
   for(int i=0;i<ns;i++)for(int j=0;j<nx;j++){var w=launch.TransformPoint(-XW+j,0,S0+i);var o=new Vector3(w.x,600,w.z);top[i,j]=float.NaN;
    var hits=Physics.RaycastAll(o,Vector3.down,1200,~(1<<Ghost),QueryTriggerInteraction.Ignore).Where(h=>!h.collider.attachedRigidbody&&!Veg(Path(h.collider.transform))).OrderBy(h=>h.distance).ToArray();
    if(hits.Length==0){anomalies.Add((new Vector3(w.x,0,w.z),"hole",""));continue;}
    var h0=hits[0];top[i,j]=h0.point.y;ny[i,j]=h0.normal.y;name[i,j]=h0.collider.name;
    if(h0.normal.y<0)anomalies.Add((h0.point,"fold",$"{h0.collider.name} n{h0.normal.y:F2}"));
    else if(h0.normal.y<.5f)anomalies.Add((h0.point,"steep",$"{h0.collider.name} n{h0.normal.y:F2}"));
    if(hits.Length>1&&h0.collider.name.StartsWith("Ground_")&&!h0.collider.name.Contains("CR103")){var under=hits.Skip(1).FirstOrDefault(h=>h.collider.name.Contains("CR103")&&h0.point.y-h.point.y<.6f);if(under.collider)anomalies.Add((h0.point,"through",$"{h0.collider.name} {h0.point.y-under.point.y:F2} above {under.collider.name}"));}
    if(visual&&Physics.Raycast(o,Vector3.down,out var g,1200,1<<Ghost,QueryTriggerInteraction.Collide)){float d=g.point.y-h0.point.y;if(Mathf.Abs(d)>.25f)anomalies.Add((g.point,d>0?"visual-above":"visual-below",$"{d:F2} vis {g.collider.name.Substring(6)} / col {h0.collider.name}"));
     else if(g.normal.y<0)anomalies.Add((g.point,"visual-fold",g.collider.name.Substring(6)));}}
   for(int i=0;i<ns;i++)for(int j=0;j<nx;j++){if(float.IsNaN(top[i,j]))continue;float m=0;foreach(var (di,dj) in new[]{(1,0),(0,1)}){int a=i+di,b=j+dj;if(a>=ns||b>=nx||float.IsNaN(top[a,b]))continue;m=Mathf.Max(m,Mathf.Abs(top[a,b]-top[i,j]));}
    float kink=0;if(i>0&&i<ns-1&&!float.IsNaN(top[i-1,j])&&!float.IsNaN(top[i+1,j]))kink=Mathf.Abs(top[i+1,j]-2*top[i,j]+top[i-1,j]);if(j>0&&j<nx-1&&!float.IsNaN(top[i,j-1])&&!float.IsNaN(top[i,j+1]))kink=Mathf.Max(kink,Mathf.Abs(top[i,j+1]-2*top[i,j]+top[i,j-1]));
    if(m>1.2f||kink>.6f){var w=launch.TransformPoint(-XW+j,0,S0+i);anomalies.Add((new Vector3(w.x,top[i,j],w.z),m>1.2f?"step":"kink",$"{m:F2}/{kink:F2} {name[i,j]}"));}}
    return anomalies;}
   var now=Grid(true,out var topNow);
   // the same scan with every 0.71-changed mesh collider put back to its 0.70 mesh (Assets/Report072Old = git a5b2ebaa, or the
   // original of an "-own-" copy).
   var swapped=new List<(MeshCollider c,Mesh m)>();var moved=0;
   foreach(var mc in Object.FindObjectsByType<MeshCollider>(FindObjectsInactive.Exclude,FindObjectsSortMode.None)){var m=mc.sharedMesh;if(!m||!mc.bounds.Intersects(area))continue;var ap=AssetDatabase.GetAssetPath(m);
    var on=System.IO.Path.GetFileNameWithoutExtension(ap);int ix=on.IndexOf("-own-");if(ix>=0)on=on.Substring(ix+5);else if(!File.Exists("Assets/Report072Old/"+on+".asset"))continue;
    var op=File.Exists("Assets/Report072Old/"+on+".asset")?"Assets/Report072Old/"+on+".asset":AssetDatabase.FindAssets(on+" t:Mesh").Select(AssetDatabase.GUIDToAssetPath).FirstOrDefault(x=>System.IO.Path.GetFileNameWithoutExtension(x)==on);
    if(op==null)continue;var om=AssetDatabase.LoadAssetAtPath<Mesh>(op);if(!om)continue;swapped.Add((mc,m));mc.sharedMesh=om;moved++;}
   rows.Add($"colliders put back to 0.70 for the comparison: {moved} ({string.Join(", ",swapped.Select(s=>s.c.name))})");
   var old=Grid(false,out var topOld);foreach(var s in swapped)s.c.sharedMesh=s.m;
   string Cls(string k)=>k=="step"||k=="kink"||k=="steep"?"shape":k;
   var anomalies=now.Select(a=>(a.p,a.kind,a.info+(old.Any(o=>Cls(o.kind)==Cls(a.kind)&&new Vector2(o.p.x-a.p.x,o.p.z-a.p.z).sqrMagnitude<2.3f)?" [0.70 too]":" [NEW]"))).ToList();
   // cluster
   var left=anomalies.ToList();int k=0;
   while(left.Count>0){var seed=left[0];var cl=new List<(Vector3 p,string kind,string info)>{seed};left.RemoveAt(0);bool grew=true;
    while(grew){grew=false;for(int q=left.Count-1;q>=0;q--)if(cl.Any(c=>new Vector2(c.p.x-left[q].p.x,c.p.z-left[q].p.z).magnitude<=4.1f)){cl.Add(left[q]);left.RemoveAt(q);grew=true;}}
    var c0=cl.Aggregate(Vector3.zero,(s,c)=>s+c.p)/cl.Count;var lq=launch.InverseTransformPoint(c0);
    rows.Add($"CLUSTER {++k} n {cl.Count} centre ({c0.x:F1}, {c0.y:F1}, {c0.z:F1}) launch s {lq.z:F0} x {lq.x:F0} kinds {string.Join(",",cl.GroupBy(c=>c.kind).Select(gr=>gr.Key+":"+gr.Count()))} new {cl.Count(c=>c.info.EndsWith("[NEW]"))}");
    foreach(var c in cl.Where(c=>c.info.EndsWith("[NEW]")).Take(30).Concat(cl.Where(c=>!c.info.EndsWith("[NEW]")).Take(4)))rows.Add($"   {c.kind} ({c.p.x:F1}, {c.p.y:F2}, {c.p.z:F1}) {c.info}");}
   File.WriteAllLines($"{outDir}/scan-{scene}.txt",rows);foreach(var go in ghosts)Object.DestroyImmediate(go);Physics.queriesHitBackfaces=false;}
  EditorApplication.Exit(0);}
}
