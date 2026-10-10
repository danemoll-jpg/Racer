using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 0.103 Part A (editor only): the Abandoned Cabin Jump joint where the leaning run-up boards meet the roof deck, Dan's Backyard Forward and Free Roam.
//  - the takeoff collider (0.101) had the boards' last row (5.0 m wide) and the roof's first row (9.6 m) at the same station: two zero-area
//    triangles across the whole joint, so the boards' end edge and the roof's front edge were not joined. Rebuilt as one strip: the 5.0 m
//    centre strip runs unbroken from the board foot to the lip, and the roof's eaves are side strips from the joint to the lip sharing the
//    strip's edge vertices. Every row keeps its station and height (same surface, same line, same roof angle, same lip).
//  - the visible roof planks were level boxes on the 0.32 roof, each front edge 9 cm above the roof: laid along the roof's slope, top flush.
// WORLD_DRY=1: report only.
public static class Report103Fix {
 static string Out=>Environment.GetEnvironmentVariable("PROBE_OUT");
 static bool Dry=>Environment.GetEnvironmentVariable("WORLD_DRY")=="1";
 const string Asset="Assets/Scenery/Report103/Takeoff - Leaning boards through cabin roof (0.103).asset";
 static readonly Vector3 lip=new(224.61f,74.15f,80.95f);const float heading=82.82f;
 static Vector3 d=>new(Mathf.Sin(heading*Mathf.Deg2Rad),0,Mathf.Cos(heading*Mathf.Deg2Rad));static Vector3 n=>new(-d.z,0,d.x);
 static float A(Vector3 p){p-=lip;p.y=0;return Vector3.Dot(p,d);}static float L(Vector3 p){p-=lip;p.y=0;return Vector3.Dot(p,n);}
 static string V(Vector3 v)=>$"({v.x:F3},{v.y:F3},{v.z:F3})";
 static int Degenerate(Mesh m,Matrix4x4 w){var v=m.vertices;var t=m.triangles;int k=0;for(int i=0;i<t.Length;i+=3)if(Vector3.Cross(w.MultiplyPoint3x4(v[t[i+1]])-w.MultiplyPoint3x4(v[t[i]]),w.MultiplyPoint3x4(v[t[i+2]])-w.MultiplyPoint3x4(v[t[i]])).magnitude<1e-4f)k++;return k;}
 public static void Run(){var log=new List<string>();
  try{
   Directory.CreateDirectory("Assets/Scenery/Report103");Mesh built=null;
   foreach(var sn in new[]{"DansBackyardForward","FreeRoamWorld"}){
    var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+sn+".unity");Physics.SyncTransforms();log.Add("===== "+sn);
    var root=GameObject.Find("Backyard optional forest shortcuts").transform;
    var take=root.GetComponentsInChildren<MeshCollider>(true).First(c=>c.name=="Takeoff - Leaning boards through cabin roof");
    var w=take.transform.localToWorldMatrix;var wi=take.transform.worldToLocalMatrix;var old=take.sharedMesh;
    log.Add($"takeoff collider {old.name} ({AssetDatabase.GetAssetPath(old)}): {old.vertexCount} vertices, {old.triangles.Length/3} triangles, {Degenerate(old,w)} of zero area");
    // the rows: station, centre point (the row is level across), half width
    var rows=old.vertices.Select(v=>w.MultiplyPoint3x4(v)).GroupBy(p=>Mathf.Round(A(p)*1000)).Select(g=>{var ps=g.ToList();float a=A(ps[0]);float hw=ps.Max(p=>Mathf.Abs(L(p)));var c=ps.Aggregate(Vector3.zero,(x,y)=>x+y)/ps.Count;return (a,c,hw,ys:ps.Select(p=>p.y).ToList());}).OrderBy(r=>r.a).ToList();
    foreach(var r in rows)if(r.ys.Max()-r.ys.Min()>1e-4f)throw new Exception($"row at along {r.a:F2} is not level across ({r.ys.Min():F4}..{r.ys.Max():F4})");
    float joint=rows.Where(r=>r.hw>3).Min(r=>r.a),board=rows.Where(r=>r.hw<3).Max(r=>r.hw),roof=rows.Max(r=>r.hw);
    log.Add($"rows: {rows.Count} stations from along {rows[0].a:F2} to {rows[^1].a:F2}; boards {board*2:F2} m wide to the joint at along {joint:F2} (line s {joint+44.72f:F2}), roof {roof*2:F2} m from there to the lip");
    if(built==null){
     var vs=new List<Vector3>();var ts=new List<int>();int prevC=-1,prevE=-1;
     foreach(var r in rows){var c=r.c;int ci=vs.Count;vs.Add(c-n*board);vs.Add(c+n*board);
      if(prevC>=0)ts.AddRange(new[]{prevC,ci,prevC+1,prevC+1,ci,ci+1});
      if(r.a>=joint-1e-4f){int ei=vs.Count;vs.Add(c-n*roof);vs.Add(c+n*roof);
       if(prevE>=0){ts.AddRange(new[]{prevE,ei,prevC,prevC,ei,ci});ts.AddRange(new[]{prevC+1,ci+1,prevE+1,prevE+1,ci+1,ei+1});}
       prevE=ei;}
      prevC=ci;}
     for(int i=0;i<vs.Count;i++)vs[i]=wi.MultiplyPoint3x4(vs[i]);
     var m=new Mesh{name="Takeoff - Leaning boards through cabin roof (0.103)"};m.SetVertices(vs);m.SetTriangles(ts,0);m.RecalculateNormals();m.RecalculateBounds();
     // every face up
     var nv=m.vertices;var nt=m.triangles;int down=0;for(int i=0;i<nt.Length;i+=3){var fn=Vector3.Cross(w.MultiplyPoint3x4(nv[nt[i+1]])-w.MultiplyPoint3x4(nv[nt[i]]),w.MultiplyPoint3x4(nv[nt[i+2]])-w.MultiplyPoint3x4(nv[nt[i]]));if(fn.y<0){(nt[i+1],nt[i+2])=(nt[i+2],nt[i+1]);down++;}}
     m.SetTriangles(nt,0);m.RecalculateNormals();
     if(!Dry){var ex=AssetDatabase.LoadAssetAtPath<Mesh>(Asset);if(ex){EditorUtility.CopySerialized(m,ex);m=ex;}else AssetDatabase.CreateAsset(m,Asset);}
     built=m;log.Add($"new collider: {m.vertexCount} vertices, {m.triangles.Length/3} triangles ({down} turned face-up), {Degenerate(m,w)} of zero area; the centre strip {board*2:F2} m unbroken from the foot to the lip, eaves {roof-board:F2} m each side from the joint");
     // the same surface: every old vertex lies on the new mesh at the same height
     float worst=0;foreach(var v in old.vertices){var p=w.MultiplyPoint3x4(v);var r=rows.OrderBy(x=>Mathf.Abs(x.a-A(p))).First();worst=Mathf.Max(worst,Mathf.Abs(p.y-r.c.y));}log.Add($"  old vertices against their row height: worst {worst*1000:F2} mm");}
    if(!Dry){take.sharedMesh=null;take.sharedMesh=built;EditorUtility.SetDirty(take);}
    // the roof planks: along the roof's slope at their own station, top flush with the roof
    float Y(float a){for(int i=1;i<rows.Count;i++)if(rows[i].a>=a)return Mathf.Lerp(rows[i-1].c.y,rows[i].c.y,Mathf.InverseLerp(rows[i-1].a,rows[i].a,a));return rows[^1].c.y;}
    int k=0;foreach(Transform t in root){if(t.name!="Cabin weathered roof board")continue;var p=t.position;float a=A(p),l=L(p),half=t.lossyScale.z/2,th=t.lossyScale.y;
     float slope=(Y(a+half)-Y(a-half))/(2*half);var dir=(d+Vector3.up*slope).normalized;var rot=Quaternion.LookRotation(dir,Vector3.up);var up=rot*Vector3.up;
     var top=lip+d*a+n*l;top.y=Y(a);var np=top-up*(th/2);
     float frontBefore=p.y+th/2-Y(a-half),frontAfter=(np+up*(th/2)-dir*half).y-Y(a-half);
     if(Mathf.Abs(frontBefore)>.2f){log.Add($"  plank at {V(p)} along {a:F2}: not on the roof deck (its front edge {frontBefore*100:+0.0;-0.0} cm against the roof surface there); left as it is");continue;}
     log.Add($"  plank along {a:F2} (line s {a+44.72f:F2}): slope {slope:F3}; front edge top {frontBefore*100:+0.0;-0.0} cm -> {frontAfter*100:+0.0;-0.0} cm against the roof; moved {(np-p).magnitude*100:F1} cm");
     if(!Dry){Undo.RecordObject(t,"plank");t.SetPositionAndRotation(np,rot);EditorUtility.SetDirty(t);}k++;}
    log.Add($"planks: {k}");
    if(!Dry){EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);}}
   if(!Dry)AssetDatabase.SaveAssets();
  }catch(Exception e){log.Add("FAILED "+e);}
  Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/fix.txt",log);EditorApplication.Exit(0);}
}
