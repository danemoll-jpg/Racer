using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
// 0.68 rule-5 check (read-only): compares the 0.66 ground meshes (pre-0.67 copies imported from Tools/Report068/old)
// with the current ones along every Mountain route corridor. Finds holes/voids the 0.67 edits created.
public static class Report068Compare {
 const int Old=30;
 public static void Run(){var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(outDir);AssetDatabase.Refresh();
  var olds=AssetDatabase.FindAssets("t:Mesh",new[]{"Assets/Editor/Report068Temp/Old"}).Select(AssetDatabase.GUIDToAssetPath).ToDictionary(p=>Path.GetFileName(p),p=>AssetDatabase.LoadAssetAtPath<Mesh>(p));
  foreach(var scene in (Environment.GetEnvironmentVariable("PROBE_SCENES")??"MountainLoop,MountainLoopReverse").Split(',')){
   EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.queriesHitBackfaces=false;
   var rows=new List<string>();var changed=new List<MeshCollider>();var ghosts=new List<GameObject>();
   foreach(var mc in Object.FindObjectsByType<MeshCollider>()){if(!mc.sharedMesh)continue;var name=Path.GetFileName(AssetDatabase.GetAssetPath(mc.sharedMesh));if(!olds.TryGetValue(name,out var om)||!om)continue;
    changed.Add(mc);var g=new GameObject("old:"+mc.name);g.layer=Old;g.transform.SetPositionAndRotation(mc.transform.position,mc.transform.rotation);g.transform.localScale=mc.transform.lossyScale;g.AddComponent<MeshCollider>().sharedMesh=om;ghosts.Add(g);rows.Add($"changed collider {mc.name} <- {name} (old v{om.vertexCount}/t{om.triangles.Length/3}, new v{mc.sharedMesh.vertexCount}/t{mc.sharedMesh.triangles.Length/3})");}
   Physics.SyncTransforms();
   var race=Object.FindAnyObjectByType<RaceDirector>(FindObjectsInactive.Include);var road=race.road;road.Initialize();
   var routes=new List<(string n,Func<float,Vector3> at,Func<float,Vector3> fw,float len)>{("Main",x=>road.At(x,out _),x=>{road.At(x,out var f);return f;},road.Length)};
   foreach(var b in Object.FindObjectsByType<WoodlandRoute>().Where(b=>b.gameObject.activeInHierarchy)){b.Initialize();var bb=b;routes.Add((b.title,x=>bb.At(x,out _),x=>{bb.At(x,out var f);return f;},b.Length));}
   List<RaycastHit> Col(Vector3 top,float depth,int mask){var hs=new List<RaycastHit>();var o=top;for(int k=0;k<30&&depth>0&&Physics.Raycast(o,Vector3.down,out var h,depth,mask,QueryTriggerInteraction.Ignore);k++){if(!h.collider.attachedRigidbody)hs.Add(h);depth-=h.distance+.02f;o=h.point+Vector3.down*.02f;}return hs;}
   var seen=new HashSet<(int,int)>();var samples=new List<(Vector3 p,string where)>();
   foreach(var r in routes)for(float s=0;s<r.len;s+=2){var c=r.at(s);var f=Vector3.ProjectOnPlane(r.fw(s),Vector3.up).normalized;var right=Vector3.Cross(Vector3.up,f);
    for(float o=-30;o<=30;o+=2){var p=c+right*o;var key=(Mathf.RoundToInt(p.x/2),Mathf.RoundToInt(p.z/2));if(!seen.Add(key))continue;samples.Add((p,$"{r.n} s={s:F0} o={o:F0}"));}}
   var newCols=new List<List<RaycastHit>>();foreach(var (p,_) in samples)newCols.Add(Col(p+Vector3.up*10,80,~(1<<Old)));
   foreach(var mc in changed)mc.enabled=false;Physics.SyncTransforms();
   var oldCols=new List<List<RaycastHit>>();foreach(var (p,_) in samples)oldCols.Add(Col(p+Vector3.up*10,80,~0));
   foreach(var mc in changed)mc.enabled=true;
   int voidNew=0,voidBoth=0,underNew=0;var list=new List<string>();
   for(int i=0;i<samples.Count;i++){var n=newCols[i];var o=oldCols[i];var (p,w)=samples[i];
    if(n.Count==0&&o.Count>0){voidNew++;list.Add($"VOID-NEW {p.x:F0},{p.z:F0} routeY {p.y:F1} old top {o[0].point.y:F1} {o[0].collider.name} | {w}");continue;}
    if(n.Count==0){voidBoth++;list.Add($"VOID-BOTH {p.x:F0},{p.z:F0} routeY {p.y:F1} | {w}");continue;}
    // Anything below the top surface (support beneath a deck/pavement) that the old world had and the new does not.
    if(n.Count==1&&o.Count>1&&Mathf.Abs(n[0].point.y-o[0].point.y)<.5f){underNew++;list.Add($"UNDER-NEW {p.x:F0},{p.z:F0} top {n[0].point.y:F1} {n[0].collider.name}; old below {o[1].point.y:F1} {o[1].collider.name} | {w}");}}
   rows.Insert(0,$"{scene}: samples={samples.Count} void created by 0.67={voidNew} void before and after={voidBoth} support-under-top removed by 0.67={underNew}");
   foreach(var pt in new[]{new Vector3(724.51f,79f,-6.11f),new Vector3(728.22f,80.69f,-10.35f),new Vector3(684.53f,85f,71.83f)}){
    rows.Add($"column {pt.x},{pt.z} NEW: "+string.Join(" | ",Col(pt+Vector3.up*40,200,~(1<<Old)).Select(h=>$"{h.point.y:F2} {h.collider.name}")));
    foreach(var mc in changed)mc.enabled=false;Physics.SyncTransforms();rows.Add($"column {pt.x},{pt.z} OLD: "+string.Join(" | ",Col(pt+Vector3.up*40,200,~0).Select(h=>$"{h.point.y:F2} {h.collider.name}")));foreach(var mc in changed)mc.enabled=true;Physics.SyncTransforms();}
   File.WriteAllLines($"{outDir}/{scene}-compare.txt",rows.Concat(list));foreach(var g in ghosts)Object.DestroyImmediate(g);}
  EditorApplication.Exit(0);}
}
