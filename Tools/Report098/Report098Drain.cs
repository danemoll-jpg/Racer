using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// Temporary 0.98 tool (copied into Assets/Editor/Report098Temp only while it runs): the Free Roam storm drain.
public static class Report098Drain {
 static string Out=>Environment.GetEnvironmentVariable("PROBE_OUT");
 static string Pairs=>Environment.GetEnvironmentVariable("MESH_PAIRS");
 static string P(Transform t){var s=t.name;while(t.parent){t=t.parent;s=t.name+"/"+s;}return s;}
 // height of a mesh (local = world, the tiles sit at identity) at x,z: the highest triangle covering the point
 sealed class Surf{public Vector3[] v;public int[] t;Dictionary<(int,int),List<int>> g=new();const float C=8;
  public Surf(Mesh m,Matrix4x4 mat){v=m.vertices.Select(x=>mat.MultiplyPoint3x4(x)).ToArray();t=m.triangles;for(int i=0;i<t.Length;i+=3){float x0=Mathf.Min(v[t[i]].x,v[t[i+1]].x,v[t[i+2]].x),x1=Mathf.Max(v[t[i]].x,v[t[i+1]].x,v[t[i+2]].x),z0=Mathf.Min(v[t[i]].z,v[t[i+1]].z,v[t[i+2]].z),z1=Mathf.Max(v[t[i]].z,v[t[i+1]].z,v[t[i+2]].z);
    for(int gx=Mathf.FloorToInt(x0/C);gx<=Mathf.FloorToInt(x1/C);gx++)for(int gz=Mathf.FloorToInt(z0/C);gz<=Mathf.FloorToInt(z1/C);gz++){if(!g.TryGetValue((gx,gz),out var l))g[(gx,gz)]=l=new List<int>();l.Add(i);}}}
  public float H(float x,float z){float best=float.NaN;if(!g.TryGetValue((Mathf.FloorToInt(x/C),Mathf.FloorToInt(z/C)),out var l))return best;
   foreach(int i in l){var a=v[t[i]];var b=v[t[i+1]];var c=v[t[i+2]];float den=(b.z-c.z)*(a.x-c.x)+(c.x-b.x)*(a.z-c.z);if(Mathf.Abs(den)<1e-9f)continue;float w1=((b.z-c.z)*(x-c.x)+(c.x-b.x)*(z-c.z))/den,w2=((c.z-a.z)*(x-c.x)+(a.x-c.x)*(z-c.z))/den,w3=1-w1-w2;if(w1<-1e-4f||w2<-1e-4f||w3<-1e-4f)continue;float y=w1*a.y+w2*b.y+w3*c.y;if(float.IsNaN(best)||y>best)best=y;}return best;}}
 static Mesh Load(string guid){var path=AssetDatabase.GUIDToAssetPath(guid);return string.IsNullOrEmpty(path)?null:AssetDatabase.LoadAssetAtPath<Mesh>(path);}
 // what each 0.96 swap did: old mesh vs new mesh, vertices and the largest height difference of the new vertices against the old surface
 public static void Analyse(){
  var log=new List<string>();var sc=EditorSceneManager.OpenScene("Assets/Scenes/FreeRoamWorld.unity");
  var pairs=File.ReadAllLines(Pairs).Select(l=>l.Split(' ')).ToList();
  var users=new Dictionary<Mesh,List<MeshFilter>>();foreach(var mf in UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include,FindObjectsSortMode.None)){if(!mf.sharedMesh)continue;if(!users.TryGetValue(mf.sharedMesh,out var l))users[mf.sharedMesh]=l=new List<MeshFilter>();l.Add(mf);}
  foreach(var pr in pairs){var o=Load(pr[0]);var n=Load(pr[1]);if(!o||!n){log.Add($"missing mesh {pr[0]} {pr[1]} {(o?o.name:"-")} {(n?n.name:"-")}");continue;}
   var mf=users.TryGetValue(n,out var us)?us[0]:null;var m=mf?mf.transform.localToWorldMatrix:Matrix4x4.identity;
   var so=new Surf(o,m);float maxUp=0,maxDown=0;Vector3 at=default;int cnt=0,missing=0;foreach(var w in n.vertices.Select(x=>m.MultiplyPoint3x4(x))){float h=so.H(w.x,w.z);if(float.IsNaN(h)){missing++;continue;}float d=w.y-h;if(d>maxUp){maxUp=d;at=w;}if(-d>maxDown)maxDown=-d;cnt++;}
   var b=n.bounds;var wb=mf?mf.GetComponent<Renderer>().bounds:b;
   log.Add($"{(mf?mf.name:"?")}: old '{o.name}' v{o.vertexCount} -> new '{n.name}' v{n.vertexCount}; new vertices above the old surface by up to {maxUp:F2} m (at {at.x:F1},{at.y:F1},{at.z:F1}), below by up to {maxDown:F2} m, {missing} outside it; bounds centre ({wb.center.x:F0},{wb.center.y:F0},{wb.center.z:F0}) size ({wb.size.x:F0},{wb.size.z:F0})");}
  foreach(var g in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(t=>{var nm=t.name.ToLower();return nm.Contains("culvert")||nm.Contains("storm");}))log.Add($"object {P(g)} at {g.position:F1} active {g.gameObject.activeInHierarchy}");
  Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/drain-analyse.txt",log);EditorApplication.Exit(0);}
 // Put the storm drain back to 0.95: the lid goes; the culvert shell and floor and every tile the 0.96 crack repair changed near the drain go back to their 0.95 meshes
 // (colliders too). Tiles away from the drain keep the crack repair unless they deviate (they are listed by the analysis).
 public static void Revert(){
  var log=new List<string>();var sc=EditorSceneManager.OpenScene("Assets/Scenes/FreeRoamWorld.unity");
  var centre=new Vector3(146,60,65);float radius=float.Parse(Environment.GetEnvironmentVariable("DRAIN_RADIUS")??"90",System.Globalization.CultureInfo.InvariantCulture);float maxDev=float.Parse(Environment.GetEnvironmentVariable("DRAIN_MAXDEV")??"0.25",System.Globalization.CultureInfo.InvariantCulture);
  var lid=GameObject.Find("Ground_Report096 culvert lid");if(lid){log.Add("removed "+lid.name);UnityEngine.Object.DestroyImmediate(lid);}
  var pairs=File.ReadAllLines(Pairs).Select(l=>l.Split(' ')).ToList();
  foreach(var pr in pairs){var o=Load(pr[0]);var n=Load(pr[1]);if(!o||!n)continue;
   var users=UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(f=>f.sharedMesh==n).ToList();
   foreach(var mf in users){var wb=mf.GetComponent<Renderer>().bounds;float dist=new Vector2(Mathf.Max(wb.min.x-centre.x,0,centre.x-wb.max.x),Mathf.Max(wb.min.z-centre.z,0,centre.z-wb.max.z)).magnitude;
    var m=mf.transform.localToWorldMatrix;var so=new Surf(o,m);float maxUp=0,maxDown=0;foreach(var w in n.vertices.Select(x=>m.MultiplyPoint3x4(x))){float h=so.H(w.x,w.z);if(float.IsNaN(h))continue;maxUp=Mathf.Max(maxUp,w.y-h);maxDown=Mathf.Max(maxDown,h-w.y);}
    bool shell=n.name.Contains("culvert-shell"),floor=mf.name.StartsWith("Ground_Culvert");bool back=shell||floor||dist<radius||Mathf.Max(maxUp,maxDown)>maxDev;
    log.Add($"{mf.name}: {(back?"BACK to 0.95":"kept")} (distance {dist:F0} m, new vertices {maxUp:F2} above / {maxDown:F2} below the old surface)");
    if(!back)continue;
    foreach(var mc in mf.GetComponents<MeshCollider>())if(mc.sharedMesh==n)mc.sharedMesh=o;mf.sharedMesh=o;}}
  EditorSceneManager.MarkSceneDirty(sc);EditorSceneManager.SaveScene(sc);Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/drain-revert.txt",log);EditorApplication.Exit(0);}
}
public static class Report098Probe {
 public static void Holes(){try{Report096Holes.Run();}catch(Exception e){File.WriteAllText(Environment.GetEnvironmentVariable("PROBE_OUT")+"/holes-error.txt",e.ToString());}EditorApplication.Exit(0);}
 public static void Seams(){try{Report096Seams.Run();}catch(Exception e){File.WriteAllText(Environment.GetEnvironmentVariable("PROBE_OUT")+"/seams-error.txt",e.ToString());}EditorApplication.Exit(0);}
 public static void Box(){try{Report096Box.Scan();}catch(Exception e){File.WriteAllText(Environment.GetEnvironmentVariable("PROBE_OUT")+"/box-error.txt",e.ToString());}EditorApplication.Exit(0);}
 public static void Flip(){try{Report096Flip.Scan();}catch(Exception e){File.WriteAllText(Environment.GetEnvironmentVariable("PROBE_OUT")+"/flip-error.txt",e.ToString());}EditorApplication.Exit(0);}
}
// A ground-coloured sheet 12 cm under the topmost ground surface (tiles and the culvert floor, never the concrete shell) over the strip round the box and the entrance,
// so a sliver between the concrete and the ground, or between two tiles, shows ground and not sky. Not collidable; nothing above it moves. Cells with no ground at all
// (holes) are filled at the mean height of their neighbours. Region: x 118..190, z 38..104.
public static class Report098Underlay {
 static string Out=>Environment.GetEnvironmentVariable("PROBE_OUT");
 static float Top(float x,float z,out Collider col){col=null;float best=float.NaN;foreach(var h in Physics.RaycastAll(new Vector3(x,200,z),Vector3.down,400,~0,QueryTriggerInteraction.Ignore)){if(!(h.collider is MeshCollider))continue;var n=h.collider.name;if(!n.StartsWith("Ground_")||n.StartsWith("Ground_Edge apron"))continue;if(float.IsNaN(best)||h.point.y>best){best=h.point.y;col=h.collider;}}return best;}
 public static void Run(){
  var sc=EditorSceneManager.OpenScene("Assets/Scenes/FreeRoamWorld.unity");Physics.SyncTransforms();var log=new List<string>();
  var old=GameObject.Find("Ground_Report098 seam underlay");if(old)UnityEngine.Object.DestroyImmediate(old);
  const float cell=.3f,x0=118,x1=190,z0=38,z1=104;int nx=Mathf.CeilToInt((x1-x0)/cell)+1,nz=Mathf.CeilToInt((z1-z0)/cell)+1;
  var h=new float[nx,nz];var mats=new Dictionary<Material,int>();int holes=0;
  for(int i=0;i<nx;i++)for(int j=0;j<nz;j++){h[i,j]=Top(x0+i*cell,z0+j*cell,out var col);if(float.IsNaN(h[i,j]))holes++;else if(col&&(i+j)%17==0){var mr=col.GetComponent<MeshRenderer>();if(mr&&mr.sharedMaterial){mats.TryGetValue(mr.sharedMaterial,out var q);mats[mr.sharedMaterial]=q+1;}}}
  var excl=new bool[nx,nz];
  { // the tunnel's interior: every cell under a ceiling triangle of the shell (the sheet never goes inside it)
   var shell=UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Exclude,FindObjectsSortMode.None).First(m=>m.name.StartsWith("Long storm culvert"));var mm=shell.transform.localToWorldMatrix;var sv=shell.sharedMesh.vertices.Select(v=>mm.MultiplyPoint3x4(v)).ToArray();var st=shell.sharedMesh.triangles;
   for(int q=0;q<st.Length;q+=3){var A=sv[st[q]];var B=sv[st[q+1]];var C=sv[st[q+2]];var tn=Vector3.Cross(B-A,C-A).normalized;if(tn.y>-.4f)continue;
    int ix0=Mathf.Max(0,Mathf.FloorToInt((Mathf.Min(A.x,Mathf.Min(B.x,C.x))-x0)/cell)-1),ix1=Mathf.Min(nx-1,Mathf.CeilToInt((Mathf.Max(A.x,Mathf.Max(B.x,C.x))-x0)/cell)+1),iz0=Mathf.Max(0,Mathf.FloorToInt((Mathf.Min(A.z,Mathf.Min(B.z,C.z))-z0)/cell)-1),iz1=Mathf.Min(nz-1,Mathf.CeilToInt((Mathf.Max(A.z,Mathf.Max(B.z,C.z))-z0)/cell)+1);
    float den=(B.z-C.z)*(A.x-C.x)+(C.x-B.x)*(A.z-C.z);if(Mathf.Abs(den)<1e-6f)continue;
    for(int i=ix0;i<=ix1;i++)for(int j=iz0;j<=iz1;j++){float px=x0+i*cell,pz=z0+j*cell;float w1=((B.z-C.z)*(px-C.x)+(C.x-B.x)*(pz-C.z))/den,w2=((C.z-A.z)*(px-C.x)+(A.x-C.x)*(pz-C.z))/den,w3=1-w1-w2;if(w1<-.02f||w2<-.02f||w3<-.02f)continue;excl[i,j]=true;}}
  }
  for(int i=0;i<nx;i++)for(int j=0;j<nz;j++){foreach(var hh in Physics.RaycastAll(new Vector3(x0+i*cell,200,z0+j*cell),Vector3.down,400,~0,QueryTriggerInteraction.Ignore))if(hh.collider.name.StartsWith("Long storm culvert")){excl[i,j]=true;break;}}
  for(int i=0;i<nx;i++)for(int j=0;j<nz;j++)if(excl[i,j]){bool near=false;for(int di=-1;di<=1&&!near;di++)for(int dj=-1;dj<=1;dj++){int a=i+di,b=j+dj;if(a>=0&&b>=0&&a<nx&&b<nz&&!excl[a,b]){near=true;break;}}}
  var filled=new bool[nx,nz];
  for(int pass=0;pass<40;pass++){bool any=false;for(int i=0;i<nx;i++)for(int j=0;j<nz;j++){if(!float.IsNaN(h[i,j]))continue;float s=0;int c=0;for(int di=-1;di<=1;di++)for(int dj=-1;dj<=1;dj++){int a=i+di,b=j+dj;if(a<0||b<0||a>=nx||b>=nz||float.IsNaN(h[a,b]))continue;s+=h[a,b];c++;}if(c>0){h[i,j]=s/c;filled[i,j]=true;any=true;}}if(!any)break;}
  var verts=new List<Vector3>();var idx=new int[nx,nz];for(int i=0;i<nx;i++)for(int j=0;j<nz;j++){idx[i,j]=verts.Count;verts.Add(new Vector3(x0+i*cell,(float.IsNaN(h[i,j])?60:h[i,j])-.12f,z0+j*cell));}
  var tris=new List<int>();for(int i=0;i+1<nx;i++)for(int j=0;j+1<nz;j++){int a=idx[i,j],b=idx[i+1,j],c=idx[i,j+1],d=idx[i+1,j+1];if(excl[i,j]||excl[i+1,j]||excl[i,j+1]||excl[i+1,j+1])continue;float lo=Mathf.Min(Mathf.Min(h[i,j],h[i+1,j]),Mathf.Min(h[i,j+1],h[i+1,j+1])),hi=Mathf.Max(Mathf.Max(h[i,j],h[i+1,j]),Mathf.Max(h[i,j+1],h[i+1,j+1]));if(hi-lo>.6f)continue; /* a step or a wall: no sheet across it */ tris.AddRange(new[]{a,c,b,b,c,d});}
  var mesh=new Mesh{name="FreeRoamWorld-Report098-seam-underlay",indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};mesh.SetVertices(verts);mesh.SetTriangles(tris,0);mesh.RecalculateNormals();mesh.RecalculateBounds();mesh.SetColors(verts.Select(_=>new Color32(89,102,77,255)).ToList());
  Directory.CreateDirectory("Assets/Scenery/Report098");AssetDatabase.CreateAsset(mesh,"Assets/Scenery/Report098/"+mesh.name+".asset");
  var go=new GameObject("Ground_Report098 seam underlay");go.AddComponent<MeshFilter>().sharedMesh=mesh;var mr2=go.AddComponent<MeshRenderer>();var mat=mats.OrderByDescending(m=>m.Value).Select(m=>m.Key).FirstOrDefault();if(mat)mr2.sharedMaterial=mat;mr2.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
  UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go,sc);
  log.Add($"grid {nx}x{nz} cells of {cell} m, {holes} cells with no ground, filled at the neighbours' mean; material {(mat?mat.name:"none")}; {tris.Count/3} triangles");
  EditorSceneManager.MarkSceneDirty(sc);EditorSceneManager.SaveScene(sc);AssetDatabase.SaveAssets();Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/underlay.txt",log);EditorApplication.Exit(0);}
}
public static class Report098Dbg {
 public static void Rays(){var sc=EditorSceneManager.OpenScene("Assets/Scenes/FreeRoamWorld.unity");Physics.SyncTransforms();var log=new List<string>();
  foreach(var p in new[]{new Vector2(141,50),new Vector2(141,70),new Vector2(146,40),new Vector2(150,60),new Vector2(160,75)}){log.Add("ray at "+p);foreach(var h in Physics.RaycastAll(new Vector3(p.x,200,p.y),Vector3.down,400,~0,QueryTriggerInteraction.Ignore).OrderByDescending(x=>x.point.y))log.Add($"   y {h.point.y:F2} {h.collider.GetType().Name} '{h.collider.name}'");}
  Directory.CreateDirectory(Environment.GetEnvironmentVariable("PROBE_OUT"));File.WriteAllLines(Environment.GetEnvironmentVariable("PROBE_OUT")+"/rays.txt",log);EditorApplication.Exit(0);}
}
