using System;using System.IO;using System.Linq;using System.Text;using System.Collections.Generic;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;using Racer;
// 0.86 Part A (BUG-001): the House 3 swimming pool becomes truly in-ground. In every scene that has it (PROBE_SCENES=a,b):
//  - the 0.85 beach entry and outside deck apron are removed;
//  - the pool (water, coping, shell) is lowered by D so the coping top sits 2 cm above the level basin floor just north of
//    it (D about 1.86 m); position, outline, size, depth and materials unchanged;
//  - the ground mesh under the pool (the pool's floor and the wall band) is lowered by the same D;
//  - outside the coping, the ground is brought down to the coping where it stood above it: within 0.4 m of the coping it
//    sits at the coping (flush); further out, ground that the old pool's skirt or the lake side raised above the coping
//    is lowered to it, and the hillside foot on the south / east / west is lowered by D fading to nothing 3 m out, so the
//    hill keeps its shape and meets the coping. Ground below the coping (the basin floor) is never touched;
//  - inside, a slope across the full width of the north end rises from the floor to the coping top at SLOPE (15 %): the
//    shallow end runs out to nothing, in the pool's own pale finish, nothing above the coping.
// Each ground mesh asset is edited once (DansBackyardForward/Reverse share one): a mesh whose pool floor is already low is
// left alone. Normals are recomputed only near the change. POOL_DRY=1 lists only (and what stands on changed ground).
public static class Report086Pool {
 const float Slope=.15f,Flush=.4f,Fade=3f;
 static string V(Vector3 v)=>$"{v.x:F2},{v.y:F2},{v.z:F2}";
 static string P(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n;}
 static bool Skip(Collider c)=>c.isTrigger||c.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0||c.name.IndexOf("tree",StringComparison.OrdinalIgnoreCase)>=0||(c.attachedRigidbody&&!c.attachedRigidbody.isKinematic);
 static float Ground(float x,float z,Transform ignore){float best=float.NaN;foreach(var h in Physics.RaycastAll(new Vector3(x,120,z),Vector3.down,200,~0,QueryTriggerInteraction.Ignore)){if(Skip(h.collider)||h.collider.transform.IsChildOf(ignore))continue;if(float.IsNaN(best)||h.point.y>best)best=h.point.y;}return best;}
 static readonly HashSet<string> edited=new();
 // Distance outside a rectangle (0 inside).
 static float Out(Vector3 p,Rect r){float dx=Mathf.Max(r.xMin-p.x,0,p.x-r.xMax),dz=Mathf.Max(r.yMin-p.z,0,p.z-r.yMax);return Mathf.Sqrt(dx*dx+dz*dz);}
 // New height of a ground point (world) at old height y.
 static float Target(Vector3 p,Rect outer,float D,float C){float d=Out(p,outer);
  if(d<=0)return p.y-D;
  if(d<=Flush)return Mathf.Min(p.y,C);
  if(d>Flush+Fade)return p.y;
  float fade=1-(d-Flush)/Fade;return Mathf.Min(p.y,Mathf.Max(p.y-D*fade,C));}
 static GameObject Wedge(string name,Transform parent,Material mat,float x0,float x1,float z0,float y0,float z1,float y1,float bottom){
  var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(parent,true);
  float run=z1-z0,rise=y1-y0,len=Mathf.Sqrt(run*run+rise*rise),ang=Mathf.Atan2(rise,run)*Mathf.Rad2Deg;float thick=Mathf.Max(y0,y1)-bottom+.3f;
  var rot=Quaternion.Euler(-ang,0,0);var topMid=new Vector3((x0+x1)*.5f,(y0+y1)*.5f,(z0+z1)*.5f);var centre=topMid-rot*Vector3.up*(thick*.5f);
  go.transform.SetPositionAndRotation(centre,rot);go.transform.localScale=new Vector3(x1-x0,thick,len);
  go.GetComponent<MeshRenderer>().sharedMaterial=mat;GameObjectUtility.SetStaticEditorFlags(go,GameObjectUtility.GetStaticEditorFlags(parent.gameObject));go.layer=parent.gameObject.layer;return go;}
 public static void Run(){bool dry=Environment.GetEnvironmentVariable("POOL_DRY")=="1";var o=Environment.GetEnvironmentVariable("PROBE_OUT");var sb=new StringBuilder();
  try{foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){var s=EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();var roots=s.GetRootGameObjects();
    var pool=roots.SelectMany(g=>g.GetComponentsInChildren<ShallowWater>(true)).FirstOrDefault(w=>w.name=="House 3 swimming pool");
    if(!pool){sb.AppendLine($"{scene}: no House 3 swimming pool");continue;}
    var group=pool.transform.parent;
    foreach(Transform old in group.Cast<Transform>().ToList())if(old.name.StartsWith("Pool beach entry (0.85)")||old.name.StartsWith("Pool deck apron (0.85)")){sb.AppendLine($"{scene}: removing {old.name}");if(!dry)UnityEngine.Object.DestroyImmediate(old.gameObject);}
    Physics.SyncTransforms();
    var edges=group.GetComponentsInChildren<BoxCollider>(true).Where(c=>c.name=="Pool edge").ToList();var shells=group.GetComponentsInChildren<BoxCollider>(true).Where(c=>c.name=="Supported pool shell").ToList();
    var pieces=new List<Transform>{pool.transform};pieces.AddRange(edges.Select(c=>c.transform));pieces.AddRange(shells.Select(c=>c.transform));
    float copingOld=edges.Max(c=>c.bounds.max.y);
    var outerB=edges.Concat(shells).Select(c=>c.bounds).Aggregate((a,b)=>{a.Encapsulate(b);return a;});var outer=Rect.MinMaxRect(outerB.min.x,outerB.min.z,outerB.max.x,outerB.max.z);
    var north=edges.OrderByDescending(c=>c.bounds.center.z).First();var west=shells.OrderBy(c=>c.bounds.center.x).First();var east=shells.OrderByDescending(c=>c.bounds.center.x).First();var northShell=shells.OrderByDescending(c=>c.bounds.center.z).First();
    float inner=northShell.bounds.min.z,xi0=west.bounds.max.x,xi1=east.bounds.min.x;
    // Basin floor just north of the coping (median), the reference for the new coping top.
    var basin=new List<float>();for(float x=outer.xMin+1.5f;x<=outer.xMax-1.5f;x+=1)foreach(var z in new[]{outer.yMax+3.5f,outer.yMax+4.5f,outer.yMax+5.5f}){float g=Ground(x,z,group);if(!float.IsNaN(g))basin.Add(g);}
    basin.Sort();float basinY=basin[basin.Count/2];
    bool moved=copingOld<basinY+.5f;float copingNew=moved?copingOld:basinY+.02f;float D=moved?0:copingOld-copingNew;float C=copingNew-.02f;
    float floorOld=Ground(pool.transform.position.x,pool.transform.position.z,group);
    sb.AppendLine($"{scene}: pool {V(pool.transform.position)} surface {pool.Surface:F2} coping top {copingOld:F2}; outer x {outer.xMin:F2}..{outer.xMax:F2} z {outer.yMin:F2}..{outer.yMax:F2}; inner x {xi0:F2}..{xi1:F2}, north inner face z {inner:F2}; floor {floorOld:F2}; basin floor north {basinY:F2} ({basin.Count} samples {basin.First():F2}..{basin.Last():F2})");
    if(moved)sb.AppendLine("  pool already lowered: pieces not moved");
    else sb.AppendLine($"  lower by D {D:F2}: coping top {copingNew:F2}, water surface {pool.Surface-D:F2}, ground at the coping {C:F2}");
    // Ground meshes under / around the pool.
    var zone=new Bounds(new Vector3(outer.center.x,40,outer.center.y),new Vector3(outer.width+2*(Flush+Fade)+1,200,outer.height+2*(Flush+Fade)+1));
    float Dg=0;
    foreach(var mc in roots.SelectMany(g=>g.GetComponentsInChildren<MeshCollider>(true)).Where(c=>c.sharedMesh&&c.bounds.Intersects(zone)&&!c.transform.IsChildOf(group))){
     var mesh=mc.sharedMesh;var path=AssetDatabase.GetAssetPath(mesh);var t=mc.transform;var v=mesh.vertices;
     var inside=Enumerable.Range(0,v.Length).Where(i=>{var w=t.TransformPoint(v[i]);return Out(w,outer)<=0;}).ToList();
     if(inside.Count==0){sb.AppendLine($"  {P(t)}: no vertex under the pool; left alone");continue;}
     // The pool floor of this mesh: the most common height under the inner footprint.
     var floorYs=inside.Select(i=>t.TransformPoint(v[i])).Where(w=>w.x>xi0+.5f&&w.x<xi1-.5f&&w.z<inner-.5f&&w.z>outer.yMin+1.1f).Select(w=>Mathf.Round(w.y*20)/20).GroupBy(y=>y).OrderByDescending(g=>g.Count()).FirstOrDefault();
     float floorMesh=floorYs!=null?floorYs.Key:float.NaN;
     bool done=edited.Contains(path)||floorMesh<copingOld-2.6f;
     float Dm=moved?(copingOld+1.86f-copingOld):D; // when re-run after the pool moved, the mesh drop equals the recorded D
     if(moved){var rec=Environment.GetEnvironmentVariable("POOL_D");Dm=rec!=null?float.Parse(rec,System.Globalization.CultureInfo.InvariantCulture):1.86f;}
     int lowered=0;float maxCut=0,maxFlush=0;var changed=new bool[v.Length];var oldY=new float[v.Length];
     for(int i=0;i<v.Length;i++){var w=t.TransformPoint(v[i]);oldY[i]=w.y;if(!zone.Contains(new Vector3(w.x,40,w.z)))continue;float ny=Target(w,outer,Dm,C);if(ny<w.y-.005f){changed[i]=true;lowered++;float d=Out(w,outer);if(d>0)maxCut=Mathf.Max(maxCut,w.y-ny);if(!done)v[i]=t.InverseTransformPoint(new Vector3(w.x,ny,w.z));}}
     sb.AppendLine($"  ground {P(t)} [{path}#{mesh.name}] floor {floorMesh:F2}; {(done?"ALREADY EDITED (left alone)":"")} vertices lowered {lowered} (under the pool {inside.Count}), largest cut outside the coping {maxCut:F2} m");
     if(!done&&!dry){var oldN=mesh.normals;mesh.vertices=v;mesh.RecalculateNormals();var n=mesh.normals;
      // keep authored normals away from the change
      var near=new HashSet<int>();var tris=mesh.triangles;for(int k=0;k<tris.Length;k+=3)if(changed[tris[k]]||changed[tris[k+1]]||changed[tris[k+2]]){near.Add(tris[k]);near.Add(tris[k+1]);near.Add(tris[k+2]);}
      if(oldN!=null&&oldN.Length==n.Length){for(int i=0;i<n.Length;i++)if(!near.Contains(i))n[i]=oldN[i];mesh.normals=n;}
      mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);mc.sharedMesh=null;mc.sharedMesh=mesh;edited.Add(path);}
     else if(done)edited.Add(path);
     Dg=Dm;}
    // What stands on changed ground outside the pool group (would float): renderers / colliders whose bottom is within
    // 1.5 m of the old ground at a point where the ground is lowered by more than 5 cm.
    foreach(var r in roots.SelectMany(g=>g.GetComponentsInChildren<Renderer>(true))){if(r.transform.IsChildOf(group))continue;var b=r.bounds;if(!b.Intersects(zone))continue;
     if(r.GetComponent<MeshCollider>()&&r.GetComponent<MeshCollider>().sharedMesh&&b.size.x>60)continue;
     if(b.size.x<60&&b.size.z<60){var foot=new Vector3(b.center.x,b.min.y,b.center.z);float g0=foot.y;var lowerBy=foot.y-Target(new Vector3(foot.x,g0,foot.z),outer,Dg,C);
      float gNow=Ground(foot.x,foot.z,group);if(Out(foot,outer)<=Flush+Fade&&Mathf.Abs(foot.y-gNow)<1.5f)sb.AppendLine($"  stands in the changed zone: {P(r.transform)} bottom {V(foot)} ground {gNow:F2} d {Out(foot,outer):F2} (ground there drops by {(gNow-Target(new Vector3(foot.x,gNow,foot.z),outer,Dg,C)):F2})");}
     else{var mf=r.GetComponent<MeshFilter>();if(!mf||!mf.sharedMesh||!mf.sharedMesh.isReadable)continue;int cnt=0;float worst=0;var at=new List<string>();
      foreach(var lv in mf.sharedMesh.vertices){var w=r.transform.TransformPoint(lv);float d=Out(w,outer);if(d>Flush+Fade)continue;float g=Ground(w.x,w.z,group);if(float.IsNaN(g)||w.y-g>3||w.y<g-.3f)continue;float drop=g-Target(new Vector3(w.x,g,w.z),outer,Dg,C);if(drop>.05f){cnt++;worst=Mathf.Max(worst,drop);if(at.Count<14)at.Add($"{w.x:F1},{w.y:F2},{w.z:F1} g {g:F2}");}}
      if(cnt>0)sb.AppendLine($"  batch {P(r.transform)}: {cnt} vertices near the ground in the changed zone (ground drops up to {worst:F2}): {string.Join(" ; ",at)}");}}
    if(dry)continue;
    if(!moved)foreach(var p in pieces)p.position+=Vector3.down*D;
    Physics.SyncTransforms();
    var mat=northShell.GetComponent<MeshRenderer>().sharedMaterial;float floor=Ground(pool.transform.position.x,pool.transform.position.z,group);
    float cop=edges.Max(c=>c.bounds.max.y);float z0=inner-(cop-floor)/Slope;
    foreach(Transform old in group.Cast<Transform>().ToList())if(old.name.StartsWith("Pool shallow-end slope (0.86)"))UnityEngine.Object.DestroyImmediate(old.gameObject);
    Wedge("Pool shallow-end slope (0.86)",group,mat,xi0,xi1,z0,floor,inner+.02f,cop,floor-.05f);
    sb.AppendLine($"  now: coping top {cop:F2}, water surface {pool.Surface:F2}, floor {floor:F2}; shallow-end slope from z {z0:F2} (y {floor:F2}) to z {inner:F2} (y {cop:F2}), {(cop-floor)/(inner-z0)*100:F1} %, width {xi1-xi0:F2} m; dry above the water from z {inner-(cop-pool.Surface)/Slope:F2}");
    EditorSceneManager.MarkSceneDirty(s);EditorSceneManager.SaveScene(s);AssetDatabase.SaveAssets();sb.AppendLine($"{scene}: saved");}
  }catch(Exception e){sb.AppendLine("ERROR "+e);}
  Directory.CreateDirectory(o);File.WriteAllText(o+"/A-pool.txt",sb.ToString());EditorApplication.Exit(0);}
}
