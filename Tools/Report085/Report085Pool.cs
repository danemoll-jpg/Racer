using System;using System.IO;using System.Linq;using System.Text;using System.Collections.Generic;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;using Racer;
// 0.85 Part F: the House 3 swimming pool (18 x 16 m, 1.3 m deep, a raised box: 1.5 m walls inside, its coping 1.9 m above
// the basin floor outside) cannot be driven out of. In every scene that has it: a beach entry across the full width of the
// north end (the pool floor rises at about 18 % to the coping) and outside it a paved apron down to the basin floor at about
// 19 %, both solid (no gap under them), in the pool's pale coping. Water, walls, coping and everything else unchanged.
// PROBE_SCENES=a,b ; POOL_DRY=1 lists only. Re-running replaces the pieces it made.
public static class Report085Pool {
 static string V(Vector3 v)=>$"{v.x:F2},{v.y:F2},{v.z:F2}";
 static string P(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n;}
 static bool Skip(Collider c)=>c.isTrigger||c.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0||c.name.IndexOf("tree",StringComparison.OrdinalIgnoreCase)>=0||(c.attachedRigidbody&&!c.attachedRigidbody.isKinematic);
 static float Ground(Vector3 p,float top,Transform ignore){float best=float.NaN;foreach(var h in Physics.RaycastAll(new Vector3(p.x,top,p.z),Vector3.down,top-p.y+30,~0,QueryTriggerInteraction.Ignore)){if(Skip(h.collider)||h.collider.transform.IsChildOf(ignore))continue;if(float.IsNaN(best)||h.point.y>best)best=h.point.y;}return best;}
 // A solid sloped block: top face from (z0,y0) to (z1,y1) across x0..x1, reaching down to 'bottom' under its high end.
 static GameObject Wedge(string name,Transform parent,Material mat,float x0,float x1,float z0,float y0,float z1,float y1,float bottom){
  var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(parent,true);
  float run=z1-z0,rise=y1-y0,len=Mathf.Sqrt(run*run+rise*rise),ang=Mathf.Atan2(rise,run)*Mathf.Rad2Deg;float thick=Mathf.Max(y0,y1)-bottom+.3f;
  var rot=Quaternion.Euler(-ang,0,0);var topMid=new Vector3((x0+x1)*.5f,(y0+y1)*.5f,(z0+z1)*.5f);var centre=topMid-rot*Vector3.up*(thick*.5f);
  go.transform.SetPositionAndRotation(centre,rot);go.transform.localScale=new Vector3(x1-x0,thick,len);
  go.GetComponent<MeshRenderer>().sharedMaterial=mat;GameObjectUtility.SetStaticEditorFlags(go,GameObjectUtility.GetStaticEditorFlags(parent.gameObject));go.layer=parent.gameObject.layer;return go;}
 public static void Run(){bool dry=Environment.GetEnvironmentVariable("POOL_DRY")=="1";var o=Environment.GetEnvironmentVariable("PROBE_OUT");var sb=new StringBuilder();
  try{foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){var s=EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();
    var pool=s.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<ShallowWater>(true)).FirstOrDefault(w=>w.name=="House 3 swimming pool");
    if(!pool){sb.AppendLine($"{scene}: no House 3 swimming pool");continue;}
    var group=pool.transform.parent;foreach(Transform old in group.Cast<Transform>().ToList())if(old.name.StartsWith("Pool beach entry")||old.name.StartsWith("Pool deck apron")){sb.AppendLine($"{scene}: replacing {old.name}");if(!dry)UnityEngine.Object.DestroyImmediate(old.gameObject);}
    Physics.SyncTransforms();
    var edges=group.GetComponentsInChildren<BoxCollider>(true).Where(c=>c.name=="Pool edge").ToList();var shells=group.GetComponentsInChildren<BoxCollider>(true).Where(c=>c.name=="Supported pool shell").ToList();
    var north=edges.OrderByDescending(c=>c.bounds.center.z).First();var west=shells.OrderBy(c=>c.bounds.center.x).First();var east=shells.OrderByDescending(c=>c.bounds.center.x).First();var northShell=shells.OrderByDescending(c=>c.bounds.center.z).First();
    float coping=north.bounds.max.y,inner=northShell.bounds.min.z,outer=Mathf.Max(northShell.bounds.max.z,north.bounds.max.z),xi0=west.bounds.max.x,xi1=east.bounds.min.x,xo0=west.bounds.min.x,xo1=east.bounds.max.x;
    var mat=north.GetComponent<MeshRenderer>().sharedMaterial;
    float floor=Ground(new Vector3(pool.transform.position.x,0,pool.transform.position.z),pool.Surface,group);
    const float g1=.185f,g2=.19f;float z0=inner-(coping-floor)/g1;
    // Basin floor beyond the apron: the highest ground under its far edge.
    float far=outer+10;float ground=float.NegativeInfinity;for(int k=0;k<6;k++){var zz=outer+6+k*1.5f;float lowest=float.PositiveInfinity;for(float x=xo0;x<=xo1;x+=1){float gy=Ground(new Vector3(x,0,zz),coping+5,group);if(!float.IsNaN(gy))lowest=Mathf.Min(lowest,gy);}ground=Mathf.Max(ground,lowest);}
    far=outer+(coping-ground)/g2;
    sb.AppendLine($"{scene}: pool {V(pool.transform.position)} surface {pool.Surface:F2} floor {floor:F2} coping {coping:F2}; inner x {xi0:F2}..{xi1:F2}, outer x {xo0:F2}..{xo1:F2}; north wall z {inner:F2}..{outer:F2}; basin floor north {ground:F2}");
    sb.AppendLine($"  beach entry: top from z {z0:F2} (y {floor:F2}) to z {inner:F2} (y {coping:F2}), {(coping-floor)/(inner-z0)*100:F1} %, width {xi1-xi0:F2} m; dry beach above the water from z {inner-(coping-pool.Surface)/g1:F2}");
    sb.AppendLine($"  apron: top from z {outer:F2} (y {coping:F2}) down to z {far:F2} (y {ground:F2}), {(coping-ground)/(far-outer)*100:F1} %, width {xo1-xo0:F2} m");
    // Anything standing where the apron goes (it would be buried or float).
    var box=new Bounds(new Vector3((xo0+xo1)*.5f,(coping+ground)*.5f+1,(outer+far)*.5f),new Vector3(xo1-xo0,coping-ground+4,far-outer));
    foreach(var c in s.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Collider>(true)))if(c.bounds.Intersects(box)&&c.bounds.size.x<60&&!c.transform.IsChildOf(group))sb.AppendLine($"  in apron area: collider {P(c.transform)} b {V(c.bounds.center)} s {V(c.bounds.size)}");
    foreach(var r in s.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Renderer>(true)))if(r.bounds.Intersects(box)&&r.bounds.size.x<60&&!r.transform.IsChildOf(group))sb.AppendLine($"  in apron area: renderer {P(r.transform)} b {V(r.bounds.center)} s {V(r.bounds.size)}");
    if(dry)continue;
    Wedge("Pool beach entry (0.85)",group,mat,xi0,xi1,z0,floor,inner+.02f,coping,floor-.05f);
    Wedge("Pool deck apron (0.85)",group,mat,xo0,xo1,outer-.02f,coping,far,ground,ground-.2f);
    EditorSceneManager.MarkSceneDirty(s);EditorSceneManager.SaveScene(s);sb.AppendLine($"{scene}: saved");}
  }catch(Exception e){sb.AppendLine("ERROR "+e);}
  Directory.CreateDirectory(o);File.WriteAllText(o+"/F-pool.txt",sb.ToString());EditorApplication.Exit(0);}
}
