using System;using System.IO;using System.Linq;using System.Text;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
// 0.76 Part B quality bar (FreeRoamWorld only): the north end of the long storm culvert stands up to ~4 m out of the
// ground (z 44-64). An earth cover is laid over it: 0.6 m over the culvert's outside where it is near or above the ground,
// falling away at 24 degrees into the terrain. Kept clear: the north mouth (8 m), every road / route within 5 m where it
// runs above ground, the tunnel inside (the cover sits on the outside only). Trees on raised ground are lifted with their
// pieces. COVER_DRY=1 reports only. Notes: PROBE_OUT/cover-notes.txt.
public static class Report076Cover {
 static readonly StringBuilder notes=new();static void Note(string s){notes.AppendLine(s);Debug.Log("REPORT076 "+s);}
 static string P(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n;}
 const float X0=118,X1=176,Z0=24,Z1=72,Step=1,Lift=.6f,Slope=.45f,Reach=10;
 static readonly Vector3 Mouth=new(166.8f,69.5f,73f);
 public static void Run(){bool dry=Environment.GetEnvironmentVariable("COVER_DRY")=="1";
  var scene=EditorSceneManager.OpenScene("Assets/Scenes/FreeRoamWorld.unity");Physics.SyncTransforms();Physics.queriesHitBackfaces=true;
  try{
  var box=Object.FindObjectsByType<MeshCollider>(FindObjectsSortMode.None).First(c=>c.name=="Long storm culvert walls and ceiling");
  bool Terrain(Collider c)=>c&&c.name.StartsWith("Ground_")&&!c.name.Contains("Culvert")&&!c.name.Contains("Free Roam culvert cover");
  float Ground(float x,float z){foreach(var h in Physics.RaycastAll(new Vector3(x,200,z),Vector3.down,300,~0,QueryTriggerInteraction.Ignore).OrderBy(h=>h.distance))if(Terrain(h.collider))return h.point.y;return float.NaN;}
  float CulvertTop(float x,float z)=>box.Raycast(new Ray(new Vector3(x,200,z),Vector3.down),out var h,300)?h.point.y:float.NaN;
  // paths that run above ground nearby (roads, branch lines outside their underground part)
  var keep=new List<Vector3>();
  foreach(var road in Object.FindObjectsByType<RaceRoad>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(road.points!=null)keep.AddRange(road.points.Where(p=>p.x>X0-10&&p.x<X1+10&&p.z>Z0-10&&p.z<Z1+10));
  foreach(var w in Object.FindObjectsByType<WoodlandRoute>(FindObjectsInactive.Include,FindObjectsSortMode.None)){if(w.points==null)continue;float s=0;var pts=w.points.ToArray();for(int i=0;i<pts.Length;i++){if(i>0)s+=Vector3.Distance(pts[i-1],pts[i]);bool under=w.undergroundEnd>w.undergroundStart&&s>=w.undergroundStart&&s<=w.undergroundEnd;if(!under&&pts[i].x>X0-10&&pts[i].x<X1+10&&pts[i].z>Z0-10&&pts[i].z<Z1+10)keep.Add(pts[i]);}}
  Note($"path points kept clear: {keep.Count}");
  int nx=Mathf.RoundToInt((X1-X0)/Step)+1,nz=Mathf.RoundToInt((Z1-Z0)/Step)+1;var g=new float[nx,nz];var top=new float[nx,nz];
  for(int i=0;i<nx;i++)for(int j=0;j<nz;j++){float x=X0+i*Step,z=Z0+j*Step;g[i,j]=Ground(x,z);float t=CulvertTop(x,z);top[i,j]=!float.IsNaN(t)&&!float.IsNaN(g[i,j])&&t>g[i,j]-.3f?t+Lift:float.NaN;}
  var H=new float[nx,nz];int raised=0;float most=0;int r=Mathf.CeilToInt(Reach/Step);
  for(int i=0;i<nx;i++)for(int j=0;j<nz;j++){H[i,j]=g[i,j];if(float.IsNaN(g[i,j]))continue;float x=X0+i*Step,z=Z0+j*Step;
   float want=float.MinValue;for(int a=-r;a<=r;a++)for(int b=-r;b<=r;b++){int ii=i+a,jj=j+b;if(ii<0||jj<0||ii>=nx||jj>=nz||float.IsNaN(top[ii,jj]))continue;want=Mathf.Max(want,top[ii,jj]-Slope*Mathf.Sqrt(a*a+b*b)*Step);}
   if(want<=g[i,j]+.03f)continue;
   var here=new Vector3(x,0,z);float clear=Mathf.Min(new Vector2(x-Mouth.x,z-Mouth.z).magnitude-8,keep.Count>0?keep.Min(p=>new Vector2(p.x-x,p.z-z).magnitude)-5:99);
   if(clear<0)continue;want=Mathf.Lerp(g[i,j],want,Mathf.Clamp01(clear/4));// eases down to the ground near the mouth and the paths
   if(want<=g[i,j]+.03f)continue;H[i,j]=want;raised++;most=Mathf.Max(most,want-g[i,j]);}
  Note($"cover: {raised} cells of {Step} m raised, up to {most:F2} m over the ground");
  for(int j=nz-1;j>=0;j-=2){var sb=new StringBuilder($"{Z0+j*Step,5:F0} ");for(int i=0;i<nx;i++){float d=H[i,j]-g[i,j];sb.Append(!float.IsNaN(top[i,j])?'C':d>.03f?(char)('0'+Mathf.Clamp(Mathf.CeilToInt(d),0,9)):'.');}Note(sb.ToString());}
  // trees standing on the raised ground
  var trunks=Object.FindObjectsByType<Collider>(FindObjectsSortMode.None).Where(c=>c.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0).Where(c=>{var p=c.bounds.center;int i=Mathf.RoundToInt((p.x-X0)/Step),j=Mathf.RoundToInt((p.z-Z0)/Step);return i>=0&&j>=0&&i<nx&&j<nz&&H[i,j]-g[i,j]>.1f;}).ToArray();
  Note($"trees on raised ground: {trunks.Length} {string.Join("; ",trunks.Select(t=>P(t.transform)+" "+t.bounds.center.ToString("F1")))}");
  if(dry)return;
  // mesh over the raised cells (a cell is drawn when any corner is raised)
  var v=new List<Vector3>();var tri=new List<int>();var col=new List<Color>();Color earth=new(.39f,.48f,.29f);
  bool Up(int i,int j)=>H[i,j]-g[i,j]>.03f;
  for(int i=0;i+1<nx;i++)for(int j=0;j+1<nz;j++){if(!(Up(i,j)||Up(i+1,j)||Up(i,j+1)||Up(i+1,j+1)))continue;if(float.IsNaN(H[i,j]+H[i+1,j]+H[i,j+1]+H[i+1,j+1]))continue;
   Vector3 Q(int a,int b)=>new(X0+a*Step,H[a,b]+(Up(a,b)?0:.02f),Z0+b*Step);int k=v.Count;v.AddRange(new[]{Q(i,j),Q(i,j+1),Q(i+1,j+1),Q(i+1,j)});col.AddRange(new[]{earth,earth,earth,earth});tri.AddRange(new[]{k,k+1,k+2,k,k+2,k+3});}
  var mesh=new Mesh{name="FreeRoamWorld culvert cover"};mesh.SetVertices(v);mesh.SetColors(col);mesh.SetTriangles(tri,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
  Directory.CreateDirectory("Assets/Track/FreeRoamWorld");const string path="Assets/Track/FreeRoamWorld/FreeRoamWorld-culvert-cover.asset";if(AssetDatabase.LoadAssetAtPath<Mesh>(path))AssetDatabase.DeleteAsset(path);AssetDatabase.CreateAsset(mesh,path);
  foreach(var old in Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Where(m=>m.name=="Ground_Free Roam culvert cover").ToArray())Object.DestroyImmediate(old.gameObject);
  var go=new GameObject("Ground_Free Roam culvert cover",typeof(MeshFilter),typeof(MeshRenderer),typeof(MeshCollider));go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<MeshCollider>().sharedMesh=mesh;
  var tile=Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).First(m=>m.name=="Ground_400_400");go.GetComponent<MeshRenderer>().sharedMaterial=tile.sharedMaterial;go.layer=tile.gameObject.layer;GameObjectUtility.SetStaticEditorFlags(go,StaticEditorFlags.BatchingStatic);
  Physics.SyncTransforms();Note($"cover mesh: {tri.Count/3} triangles, material {tile.sharedMaterial.name}");
  // lift trees: trunk collider and its batched pieces (connected parts within 2.2 m of the trunk)
  foreach(var t in trunks){var p=t.bounds.center;int i=Mathf.RoundToInt((p.x-X0)/Step),j=Mathf.RoundToInt((p.z-Z0)/Step);float dy=H[i,j]-g[i,j];
   foreach(var mf in Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Where(m=>m.sharedMesh&&!m.GetComponent<Collider>()&&(m.name.Contains("tree")||m.name.Contains("canopy")||m.name.Contains("woodland")))){
    var m=mf.sharedMesh;var trs=m.triangles;var par=Enumerable.Range(0,m.vertexCount).ToArray();int F(int x){while(par[x]!=x){par[x]=par[par[x]];x=par[x];}return x;}
    for(int q=0;q<trs.Length;q+=3){int a=F(trs[q]),b=F(trs[q+1]);par[b]=a;int c=F(trs[q+2]);par[c]=F(a);}
    var w=m.vertices;bool any=false;
    foreach(var grp in new HashSet<int>(trs).GroupBy(F)){var vs=grp.ToArray();var c=Vector3.zero;foreach(int x in vs)c+=mf.transform.TransformPoint(w[x]);c/=vs.Length;if(new Vector2(c.x-p.x,c.z-p.z).magnitude>2.2f)continue;
     if(!AssetDatabase.GetAssetPath(m).StartsWith("Assets/Track/FreeRoamWorld/")){var copy=Object.Instantiate(m);copy.name=m.name;var dst="Assets/Track/FreeRoamWorld/FreeRoamWorld-cover-"+mf.name.Replace(' ','-')+"-"+Mathf.RoundToInt(p.x)+"-"+Mathf.RoundToInt(p.z)+".asset";AssetDatabase.CreateAsset(copy,dst);mf.sharedMesh=copy;m=copy;w=m.vertices;}
     foreach(int x in vs){var q=mf.transform.TransformPoint(w[x]);q.y+=dy;w[x]=mf.transform.InverseTransformPoint(q);}any=true;}
    if(any){m.vertices=w;m.RecalculateBounds();EditorUtility.SetDirty(m);}}
   t.transform.position+=Vector3.up*dy;Note($"tree {P(t.transform)} at {p:F1} lifted {dy:F2} m with its pieces");}
  EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
  }finally{File.WriteAllText(Path.Combine(Environment.GetEnvironmentVariable("PROBE_OUT"),"cover-notes.txt"),notes.ToString());}
  EditorApplication.Exit(0);}
}
public static class Report076Uncover {
 // Removes the culvert cover again: the cover object and its mesh go; the two lifted trees (trunks and their batched
 // pieces) take back exactly the objects / meshes the Backyard Reverse scene has for them.
 static string P(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n;}
 public static void Run(){var sb=new StringBuilder();
  var world=EditorSceneManager.OpenScene("Assets/Scenes/FreeRoamWorld.unity");var src=EditorSceneManager.OpenScene("Assets/Scenes/DansBackyardReverse.unity",OpenSceneMode.Additive);
  try{
   foreach(var g in world.GetRootGameObjects().Where(g=>g.name=="Ground_Free Roam culvert cover").ToArray()){Object.DestroyImmediate(g);sb.AppendLine("cover object removed");}
   var srcAll=src.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).GroupBy(P).ToDictionary(g=>g.Key,g=>g.ToList());
   int idx(Transform t)=>t.GetSiblingIndex();
   foreach(var mf in world.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<MeshFilter>(true)).Where(m=>m.sharedMesh&&AssetDatabase.GetAssetPath(m.sharedMesh).Contains("FreeRoamWorld-cover-")).ToArray()){
    var twin=srcAll.TryGetValue(P(mf.transform),out var l)?l.FirstOrDefault(t=>idx(t)==idx(mf.transform)):null;var orig=twin?twin.GetComponent<MeshFilter>()?.sharedMesh:null;
    sb.AppendLine($"{P(mf.transform)}: {AssetDatabase.GetAssetPath(mf.sharedMesh)} -> {(orig?AssetDatabase.GetAssetPath(orig):"NOT FOUND")}");if(orig){mf.sharedMesh=orig;EditorUtility.SetDirty(mf);}}
   foreach(var at in new[]{new Vector3(140.0f,62.0f,69.9f),new Vector3(138.5f,60.1f,53.1f)}){
    var trunk=world.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Collider>(true)).Where(c=>c.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0).OrderBy(c=>new Vector2(c.bounds.center.x-at.x,c.bounds.center.z-at.z).magnitude).First();
    var twin=srcAll[P(trunk.transform)].FirstOrDefault(t=>idx(t)==idx(trunk.transform));sb.AppendLine($"trunk {P(trunk.transform)} {trunk.transform.position:F2} -> {(twin?twin.position.ToString("F2"):"NOT FOUND")}");if(twin){trunk.transform.position=twin.position;EditorUtility.SetDirty(trunk.transform);}}
   EditorSceneManager.MarkSceneDirty(world);EditorSceneManager.SaveScene(world);
  }finally{EditorSceneManager.CloseScene(src,true);File.WriteAllText(Path.Combine(Environment.GetEnvironmentVariable("PROBE_OUT"),"uncover-notes.txt"),sb.ToString());}
  foreach(var p in AssetDatabase.FindAssets("FreeRoamWorld-cover-",new[]{"Assets/Track/FreeRoamWorld"}).Select(AssetDatabase.GUIDToAssetPath).Concat(new[]{"Assets/Track/FreeRoamWorld/FreeRoamWorld-culvert-cover.asset"}))AssetDatabase.DeleteAsset(p);
  AssetDatabase.SaveAssets();EditorApplication.Exit(0);}
}
