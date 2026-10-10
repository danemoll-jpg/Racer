using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 0.103 Part A probe (editor only): the Abandoned Cabin Jump joint where the leaning run-up boards meet the roof deck, both scenes.
// Samples the drivable surface every 2 cm along the ramp axis at lateral 0, +-0.5, +-1.0, +-1.5 m from JOINT-2 to JOINT+2 m:
//  pass "colliders": the top non-rigidbody collider (what the wheels touch), its name and normal;
//  pass "visible":   the same with every collider off and temporary mesh colliders on the visible renderers (never saved).
// Also fires rays parallel to the surface at 3/8/15/30 cm above it toward the lip, to find any face a wheel or the nose can strike.
// Lists every collider and renderer near the joint. Writes joint-<tag>.txt (TAG). Never saves a scene.
public static class Report103Joint {
 static string Out=>Environment.GetEnvironmentVariable("PROBE_OUT");
 static readonly Vector3 lip=new(224.61f,74.15f,80.95f);const float heading=82.82f;
 static Vector3 d=>new(Mathf.Sin(heading*Mathf.Deg2Rad),0,Mathf.Cos(heading*Mathf.Deg2Rad));static Vector3 n=>new(-d.z,0,d.x);
 static float A(Vector3 p){p-=lip;p.y=0;return Vector3.Dot(p,d);}static float L(Vector3 p){p-=lip;p.y=0;return Vector3.Dot(p,n);}
 static string P(Transform t){var s=t.name;while(t.parent){t=t.parent;s=t.name+"/"+s;}return s;}
 static string V(Vector3 v)=>$"({v.x:F3},{v.y:F3},{v.z:F3})";
 static bool Top(Vector3 p,out RaycastHit best){best=default;bool any=false;foreach(var h in Physics.RaycastAll(new Vector3(p.x,p.y+6,p.z),Vector3.down,12,~0,QueryTriggerInteraction.Ignore)){if(h.collider.attachedRigidbody)continue;if(!any||h.point.y>best.point.y){best=h;any=true;}}return any;}
 public static void Run(){var log=new List<string>();
  try{
  foreach(var sn in (Environment.GetEnvironmentVariable("PROBE_SCENES")??"DansBackyardForward,FreeRoamWorld").Split(',')){
   EditorSceneManager.OpenScene("Assets/Scenes/"+sn+".unity");Physics.SyncTransforms();log.Add("===== "+sn);
   var root=GameObject.Find("Backyard optional forest shortcuts").transform;
   var take=root.GetComponentsInChildren<MeshCollider>(true).First(c=>c.name=="Takeoff - Leaning boards through cabin roof");
   // the joint: the first row of the takeoff collider that is roof width (wider than the boards)
   var tm=take.transform.localToWorldMatrix;var tv=take.sharedMesh.vertices.Select(v=>tm.MultiplyPoint3x4(v)).ToArray();
   float joint=tv.Where(v=>Mathf.Abs(L(v))>3f).Min(v=>A(v));float foot=tv.Min(v=>A(v));
   log.Add($"takeoff collider {take.sharedMesh.name} ({AssetDatabase.GetAssetPath(take.sharedMesh)}), {tv.Length} vertices; foot along {foot:F2}, joint (roof starts) along {joint:F2}, lip along 0; route s = along + 44.72");
   var rows=tv.Select(v=>(a:A(v),y:v.y)).Where(x=>Mathf.Abs(x.a-joint)<3).GroupBy(x=>Mathf.Round(x.a*100)).OrderBy(g=>g.Key);
   float pa=float.NaN,py=0,ps=float.NaN;foreach(var g in rows){float a=g.First().a,y=g.Average(x=>x.y);string s="";if(!float.IsNaN(pa)){float sl=(y-py)/(a-pa);s=$" slope {sl:F3}"+(float.IsNaN(ps)?"":$" (change {sl-ps:+0.000;-0.000})");ps=sl;}log.Add($"  collider row along {a:F2} y {y:F3}{s}");pa=a;py=y;}
   var zone=new Bounds(lip+d*joint+Vector3.up*(take.bounds.center.y-lip.y),new Vector3(10,6,10));
   log.Add("--- colliders near the joint (within 5 m, either side of the axis)");
   foreach(var c in UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(c=>c.bounds.Intersects(zone)&&!c.attachedRigidbody).OrderBy(c=>P(c.transform))){var b=c.bounds;
    log.Add($"   C {P(c.transform)} {c.GetType().Name} en {c.enabled&&c.gameObject.activeInHierarchy} b {V(b.min)}-{V(b.max)} along {A(b.min):F2}..{A(b.max):F2} {(c is BoxCollider bc?"box c "+V(bc.center)+" s "+V(bc.size)+" rot "+V(c.transform.eulerAngles):"")}{(c is MeshCollider mc&&mc.sharedMesh?mc.sharedMesh.name:"")}");}
   log.Add("--- renderers near the joint");
   var rends=UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r=>r.bounds.Intersects(zone)&&r.bounds.size.magnitude<80).OrderBy(r=>P(r.transform)).ToList();
   foreach(var r in rends){var mf=r.GetComponent<MeshFilter>();log.Add($"   R {P(r.transform)} pos {V(r.transform.position)} rot {V(r.transform.eulerAngles)} sc {V(r.transform.lossyScale)} b {V(r.bounds.min)}-{V(r.bounds.max)} along {A(r.bounds.min):F2}..{A(r.bounds.max):F2} mesh {(mf&&mf.sharedMesh?mf.sharedMesh.name:"-")} col {(r.GetComponent<Collider>()?"yes":"-")}");}
   Sample(log,"colliders",joint);
   // visible: every collider off, temporary mesh colliders on the renderers near the joint (scene never saved)
   var all=UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsSortMode.None).Where(c=>c.enabled).ToList();foreach(var c in all)c.enabled=false;
   var temp=new List<MeshCollider>();foreach(var r in rends){var mf=r.GetComponent<MeshFilter>();if(!mf||!mf.sharedMesh)continue;var mc=r.gameObject.AddComponent<MeshCollider>();mc.sharedMesh=mf.sharedMesh;temp.Add(mc);}
   Physics.SyncTransforms();Sample(log,"visible",joint);
   foreach(var mc in temp)UnityEngine.Object.DestroyImmediate(mc);foreach(var c in all)c.enabled=true;}
  }catch(Exception e){log.Add("FAILED "+e);}
  Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/joint-"+(Environment.GetEnvironmentVariable("TAG")??"before")+".txt",log);EditorApplication.Exit(0);}

 static void Sample(List<string> log,string pass,float joint){
  log.Add($"--- {pass}: top surface every 2 cm from along {joint-2:F2} to {joint+2:F2} (step = height change over 2 cm minus the expected from the local slope; flagged over 1 cm, slope change over 0.05, name change, no hit)");
  foreach(float l in new[]{0f,-.5f,.5f,-1f,1f,-1.5f,1.5f}){
   log.Add($"  lateral {l:+0.0;-0.0;0.0} (+ = right of the axis looking at the lip)");
   var pts=new List<(float a,float y,string name,Vector3 nrm,bool ok)>();
   for(int i=0;i<=200;i++){float a=joint-2+i*.02f;var p=lip+d*a+n*l;p.y=lip.y+2;bool ok=Top(p,out var h);pts.Add((a,ok?h.point.y:float.NaN,ok?h.collider.name:"NONE",ok?h.normal:Vector3.zero,ok));}
   int flags=0;float maxStep=0,minSl=9,maxSl=-9;
   for(int i=0;i<pts.Count;i++){var q=pts[i];string f="";
    if(!q.ok){f=" NO SURFACE";flags++;}
    else if(i>=2&&pts[i-1].ok&&pts[i-2].ok){float s1=(pts[i-1].y-pts[i-2].y)/.02f,s2=(q.y-pts[i-1].y)/.02f;float step=q.y-pts[i-1].y-s1*.02f;minSl=Mathf.Min(minSl,s2);maxSl=Mathf.Max(maxSl,s2);
     if(Mathf.Abs(step)>Mathf.Abs(maxStep))maxStep=step;
     if(Mathf.Abs(step)>.01f)f+=$" STEP {step*100:+0.0;-0.0} cm";else if(Mathf.Abs(s2-s1)>.05f)f+=$" SLOPE {s1:F3}->{s2:F3}";if(s2<-.01f)f+=" REVERSED";}
    if(i>0&&q.name!=pts[i-1].name)f+=$" NAME {pts[i-1].name} -> {q.name}";
    if(f.Length>0&&i>0)flags++;
    if(i%10==0||f.Length>0)log.Add($"    along {q.a,6:F2} y {q.y,8:F3} slope-normal {(q.ok?Mathf.Atan2(Mathf.Sqrt(q.nrm.x*q.nrm.x+q.nrm.z*q.nrm.z),q.nrm.y):0):F3} {q.name}{f}");}
   log.Add($"    summary lateral {l:+0.0;-0.0;0.0}: {flags} flagged, largest step {maxStep*100:+0.00;-0.00} cm, slope over 2 cm {minSl:F3}..{maxSl:F3}");
   // rays parallel to the surface toward the lip: any face standing up in the way
   foreach(float h in new[]{.03f,.08f,.15f,.30f}){var s0=pts[0];var s1=pts[10];if(!s0.ok||!s1.ok)continue;float sl=(s1.y-s0.y)/.2f;var dir=(d+Vector3.up*sl).normalized;var o=lip+d*s0.a+n*l;o.y=s0.y+h;
    var hits=Physics.RaycastAll(o,dir,4.2f,~0,QueryTriggerInteraction.Ignore).Where(x=>!x.collider.attachedRigidbody).OrderBy(x=>x.distance).ToList();
    log.Add($"    parallel ray {h*100:F0} cm above, slope {sl:F3}: "+(hits.Count==0?"clear for 4.2 m":string.Join("; ",hits.Take(3).Select(x=>$"hits {x.collider.name} at along {A(x.point):F2} y {x.point.y:F3} normal {V(x.normal)}"))));}}
 }
}
