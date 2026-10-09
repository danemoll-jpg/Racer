using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// Temporary 0.99 world tool (copied into Assets/Editor/Report099Temp only while it runs).
public static class Report099World {
 static string Out=>Environment.GetEnvironmentVariable("PROBE_OUT");
 // Part A: the Ridge Cut sign's lettering used the TextMesh default font material (GUI text shader: ZTest Always, no culling),
 // so it drew over terrain and objects and showed on both faces. Give it the project's depth-tested, back-face-culled lettering material.
 public static void SignText(){
  var log=new List<string>();var mat=AssetDatabase.LoadAssetAtPath<Material>("Assets/Environment/Phase8/Depth tested world lettering 0.mat");
  log.Add("material "+(mat?mat.name+" shader "+mat.shader.name:"MISSING"));
  foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){
   var sc=EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");bool dirty=false;
   foreach(var tm in UnityEngine.Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include,FindObjectsSortMode.None)){
    var r=tm.GetComponent<MeshRenderer>();string sh=r&&r.sharedMaterial?r.sharedMaterial.shader.name:"none";
    bool ours=tm.name=="Road closed lettering";
    if(ours){r.sharedMaterial=mat;dirty=true;log.Add($"{scene}: FIXED '{tm.text.Replace('\n',' ')}' at {tm.transform.position:F2} (was {sh}) board {tm.transform.parent.Find("Sign backing").lossyScale}");}
    else if(sh.StartsWith("GUI")||sh.Contains("Text Shader")||sh.Contains("Font"))log.Add($"{scene}: default-font text '{tm.text.Replace('\n',' ')}' {tm.name} at {tm.transform.position:F1} shader {sh}");}
   if(dirty){EditorSceneManager.MarkSceneDirty(sc);EditorSceneManager.SaveScene(sc);}}
  Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/signtext.txt",log);EditorApplication.Exit(0);}
}
public static class Report099Pits {
 static string Out=>Environment.GetEnvironmentVariable("PROBE_OUT");
 // PROBE_SPOTS="name,x,y,z,R|..." : lists the report-096 fills in the scene and prints an ASCII height map (step 2 m) of every spot: each cell = ground (top hit) minus the spot's y, in metres
 public static void Map(){
  var log=new List<string>();
  foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){
   var sc=EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();log.Add("==== "+scene);
   foreach(var mf in UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(m=>m.name.Contains("Report096")))log.Add($"  {mf.name} bounds {mf.GetComponent<Renderer>().bounds.center:F0} size {mf.GetComponent<Renderer>().bounds.size:F0} active {mf.gameObject.activeInHierarchy}");
   foreach(var spot in Environment.GetEnvironmentVariable("PROBE_SPOTS").Split('|')){
    var a=spot.Split(',');string name=a[0];float x=F(a[1]),y=F(a[2]),z=F(a[3]),R=F(a[4]);log.Add($"-- {name} ({x},{y},{z}) R {R}; rows = z from {z+R} down to {z-R}, cols = x from {x-R} to {x+R}; cell = ground - {y:F1}, in dm (n = none, '+' >9, '-' < -9)");
    for(float zz=z+R;zz>=z-R;zz-=2){var sb=new System.Text.StringBuilder();sb.Append($"{zz,7:F0} ");
     for(float xx=x-R;xx<=x+R;xx+=2){string c="n";if(Physics.Raycast(new Vector3(xx,y+80,zz),Vector3.down,out var h,300,~0,QueryTriggerInteraction.Ignore)){float d=h.point.y-y;c=d>.5f?"^":d>-.5f?".":d<-9.5f?"#":Mathf.RoundToInt(-d).ToString();}sb.Append(c);}
     log.Add(sb.ToString());}}}
  Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/pitmap.txt",log);EditorApplication.Exit(0);}
 static float F(string s)=>float.Parse(s,System.Globalization.CultureInfo.InvariantCulture);
}
public static class Report099Shots {
 static string Out=>Environment.GetEnvironmentVariable("PROBE_OUT");
 // PROBE_VIEWS="name,x,y,z,yaw,pitch|..." in each PROBE_SCENES scene: renders the saved scene (authored ground, roads, objects; not the runtime-built trees) from each eye point
 public static void Run(){
  Directory.CreateDirectory(Out);
  foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){
   EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");
   var go=new GameObject("shotcam");var cam=go.AddComponent<Camera>();cam.fieldOfView=60;cam.nearClipPlane=.3f;cam.farClipPlane=1500;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.55f,.7f,.9f);
   var data=go.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
   foreach(var v in Environment.GetEnvironmentVariable("PROBE_VIEWS").Split('|')){var a=v.Split(',');var inv=System.Globalization.CultureInfo.InvariantCulture;
    go.transform.SetPositionAndRotation(new Vector3(float.Parse(a[1],inv),float.Parse(a[2],inv),float.Parse(a[3],inv)),Quaternion.Euler(float.Parse(a[5],inv),float.Parse(a[4],inv),0));
    var rt=new RenderTexture(1600,900,24);cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;var tex=new Texture2D(1600,900,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1600,900),0,0);tex.Apply();RenderTexture.active=null;
    File.WriteAllBytes($"{Out}/{scene}-{a[0]}.png",tex.EncodeToPNG());}
   UnityEngine.Object.DestroyImmediate(go);}
  EditorApplication.Exit(0);}
}

public static class Report099Pit {
 static string Out=>Environment.GetEnvironmentVariable("PROBE_OUT");
 static float GroundBelow(Vector3 o,out Collider col){col=null;float best=float.NaN;foreach(var h in Physics.RaycastAll(o,Vector3.down,400,~0,QueryTriggerInteraction.Ignore)){if(!(h.collider is MeshCollider)||h.collider.name.StartsWith("Ground_Report099"))continue;if(float.IsNaN(best)||h.point.y>best){best=h.point.y;col=h.collider;}}return best;}
 // WORLD_PITS="name,cx,cz,R|..." (PROBE_SCENES): each pit is filled with a ground mesh "Ground_Report099 fill <name>" over what is there (the 0.96 fills included); WORLD_DRY=1 only reports
 public static void Run(){
  var log=new List<string>();bool dry=Environment.GetEnvironmentVariable("WORLD_DRY")=="1";
  foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){
   var sc=EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();
   if(!dry)foreach(var g in UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(m=>m.name.StartsWith("Ground_Report099 fill")).Select(m=>m.gameObject).ToList())UnityEngine.Object.DestroyImmediate(g);
   Physics.SyncTransforms();
   foreach(var spec in Environment.GetEnvironmentVariable("WORLD_PITS").Split('|')){var pa=spec.Split(',');var inv=System.Globalization.CultureInfo.InvariantCulture;Pit(sc,pa[0],float.Parse(pa[1],inv),float.Parse(pa[2],inv),float.Parse(pa[3],inv),dry,log,scene);}
   if(!dry){Physics.SyncTransforms();EditorSceneManager.MarkSceneDirty(sc);EditorSceneManager.SaveScene(sc);}}
  Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/pit.txt",log);AssetDatabase.SaveAssets();EditorApplication.Exit(0);}
 static void Pit(UnityEngine.SceneManagement.Scene sc,string name,float cx,float cz,float R,bool dry,List<string> log,string scene){
  const float step=1f;int n=Mathf.CeilToInt(2*R/step)+1;var g=new float[n,n];var cols=new Collider[n,n];var inside=new bool[n,n];var rim=new bool[n,n];float y0=0;
  var probe=GroundBelow(new Vector3(cx,2000,cz),out _);y0=float.IsNaN(probe)?100:probe;
  for(int i=0;i<n;i++)for(int j=0;j<n;j++){float x=cx-R+i*step,z=cz-R+j*step;float d=Mathf.Sqrt((x-cx)*(x-cx)+(z-cz)*(z-cz));inside[i,j]=d<=R;rim[i,j]=inside[i,j]&&d>R-2.2f;g[i,j]=GroundBelow(new Vector3(x,y0+80,z),out cols[i,j]);if(float.IsNaN(g[i,j]))g[i,j]=y0-30;}
  var H=(float[,])g.Clone();float sum=0;int cnt=0;for(int i=0;i<n;i++)for(int j=0;j<n;j++)if(rim[i,j]){sum+=g[i,j];cnt++;}for(int i=0;i<n;i++)for(int j=0;j<n;j++)if(inside[i,j]&&!rim[i,j])H[i,j]=cnt>0?sum/cnt:y0;
  for(int it=0;it<1200;it++){var N=(float[,])H.Clone();for(int i=1;i<n-1;i++)for(int j=1;j<n-1;j++){if(!inside[i,j]||rim[i,j])continue;N[i,j]=(H[i-1,j]+H[i+1,j]+H[i,j-1]+H[i,j+1])/4;}H=N;}
  var fills=new bool[n,n];float deepest=0;int fcount=0;for(int i=0;i<n;i++)for(int j=0;j<n;j++){if(!inside[i,j]||rim[i,j])continue;if(H[i,j]>g[i,j]+.15f){fills[i,j]=true;fcount++;deepest=Mathf.Max(deepest,H[i,j]-g[i,j]);}}
  log.Add($"{scene} pit {name} at ({cx:F0},{cz:F0}) r {R:F0}: {fcount} cells to fill, the deepest {deepest:F1} m");
  if(dry||fcount==0)return;
  var verts=new List<Vector3>();var idx=new int[n,n];var mats=new Dictionary<Material,int>();
  for(int i=0;i<n;i++)for(int j=0;j<n;j++){idx[i,j]=-1;if(!inside[i,j])continue;idx[i,j]=verts.Count;float x=cx-R+i*step,z=cz-R+j*step;verts.Add(new Vector3(x,fills[i,j]?H[i,j]:g[i,j]+.025f,z));if(cols[i,j]&&(i+j)%3==0){var mr=cols[i,j].GetComponent<MeshRenderer>();if(mr&&mr.sharedMaterial){mats.TryGetValue(mr.sharedMaterial,out var q);mats[mr.sharedMaterial]=q+1;}}}
  var tris=new List<int>();for(int i=0;i+1<n;i++)for(int j=0;j+1<n;j++){int a=idx[i,j],b=idx[i+1,j],c=idx[i,j+1],d=idx[i+1,j+1];if(a<0||b<0||c<0||d<0)continue;if(!(fills[i,j]||fills[i+1,j]||fills[i,j+1]||fills[i+1,j+1]))continue;tris.AddRange(new[]{a,c,b,b,c,d});}
  var mesh=new Mesh{name=$"{scene}-Report099-{name}",indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};mesh.SetVertices(verts);mesh.SetTriangles(tris,0);mesh.RecalculateNormals();mesh.RecalculateBounds();mesh.SetColors(verts.Select(_=>new Color32(89,102,77,255)).ToList());
  Directory.CreateDirectory("Assets/Scenery/Report099");AssetDatabase.CreateAsset(mesh,$"Assets/Scenery/Report099/{mesh.name}.asset");
  var go=new GameObject($"Ground_Report099 fill {name}");go.AddComponent<MeshFilter>().sharedMesh=mesh;var mrr=go.AddComponent<MeshRenderer>();var mat=mats.OrderByDescending(m=>m.Value).Select(m=>m.Key).FirstOrDefault();if(mat)mrr.sharedMaterial=mat;go.AddComponent<MeshCollider>().sharedMesh=mesh;UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go,sc);
 }
}
public static class Report099Section {
 static string Out=>Environment.GetEnvironmentVariable("PROBE_OUT");
 // PROBE_SPOTS="name,x,z|..." : cross-sections across the race road, 8 m apart over 160 m around the nearest point; columns = lateral offset -60..60 step 3 (left = negative);
 // a cell = how far the ground there is below the road surface (. within 0.5 m, ^ above, 1-9 metres, # more), R = the road surface itself
 public static void Run(){
  var log=new List<string>();var inv=System.Globalization.CultureInfo.InvariantCulture;
  foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){
   EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();var road=UnityEngine.Object.FindAnyObjectByType<Racer.RaceDirector>().road;road.Initialize();
   foreach(var spot in Environment.GetEnvironmentVariable("PROBE_SPOTS").Split('|')){var a=spot.Split(',');float x=float.Parse(a[1],inv),z=float.Parse(a[2],inv);
    float s0=0,bd=1e9f;for(float s=0;s<road.Length;s++){var q=road.At(s,out _);float d=new Vector2(q.x-x,q.z-z).magnitude;if(d<bd){bd=d;s0=s;}}
    log.Add($"== {scene} {a[0]}: road station {s0:F0} is {bd:F0} m away; left = negative lateral; columns -60..60 step 3");
    for(float s=s0-80;s<=s0+80;s+=8){var c=road.At(s,out var f);var right=Vector3.Cross(Vector3.up,Vector3.ProjectOnPlane(f,Vector3.up).normalized);float hw=road.HalfWidth(s);var sb=new System.Text.StringBuilder($"s{s:F0} ({c.x:F0},{c.y:F0},{c.z:F0}) hw{hw:F0} ");
     for(float l=-60;l<=60;l+=3){var p=c+right*l;string ch="n";if(Physics.Raycast(new Vector3(p.x,c.y+80,p.z),Vector3.down,out var h,300,~0,QueryTriggerInteraction.Ignore)){float d=c.y-h.point.y;ch=Mathf.Abs(l)<=hw?"R":d<-.5f?"^":d<.5f?".":d>9.5f?"#":Mathf.RoundToInt(d).ToString();}sb.Append(ch);}
     log.Add(sb.ToString());}}}
  Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/sections.txt",log);EditorApplication.Exit(0);}
}
public static class Report099Strip {
 static string Out=>Environment.GetEnvironmentVariable("PROBE_OUT");
 static float GroundAt(float x,float y,float z,out Collider col){col=null;float best=float.NaN;foreach(var h in Physics.RaycastAll(new Vector3(x,y+80,z),Vector3.down,400,~0,QueryTriggerInteraction.Ignore)){if(!(h.collider is MeshCollider)||!h.collider.name.StartsWith("Ground_")||h.collider.name.StartsWith("Ground_Report099"))continue;if(float.IsNaN(best)||h.point.y>best){best=h.point.y;col=h.collider;}}return best;}
 // WORLD_STRIPS="name,s0,s1,lat0,lat1,drop|..." on the scene's race road (lat negative = left): the ground in that band of the road's side is brought up to the road's verge
 // (road surface - drop), blending back into the existing ground over 5 m at the band's edges and wherever the ground is already higher; drawn and collidable.
 public static void Run(){
  var log=new List<string>();var inv=System.Globalization.CultureInfo.InvariantCulture;bool dry=Environment.GetEnvironmentVariable("WORLD_DRY")=="1";
  foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){
   var sc=EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();var road=UnityEngine.Object.FindAnyObjectByType<Racer.RaceDirector>().road;road.Initialize();
   if(!dry)foreach(var g in UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(m=>m.name.StartsWith("Ground_Report099 fill")).Select(m=>m.gameObject).ToList())UnityEngine.Object.DestroyImmediate(g);
   Physics.SyncTransforms();
   var others=new List<(Func<Vector3,float> lat,float hw,Func<Vector3,float> ry,string nm)>();var excl=new Dictionary<string,int>();
   foreach(var w in UnityEngine.Object.FindObjectsByType<Racer.WoodlandRoute>(FindObjectsInactive.Exclude,FindObjectsSortMode.None)){if(w.points==null||w.points.Length<4)continue;w.Initialize();var ww=w;others.Add((p=>{ww.Project(p,out float l);return l-ww.halfWidth-1f;},0f,p=>{float t=ww.Project(p,out _);return ww.At(t,out _).y;},"trail "+ww.title));}
   foreach(var rr in UnityEngine.Object.FindObjectsByType<Racer.RaceRoad>(FindObjectsInactive.Exclude,FindObjectsSortMode.None)){if(rr==road||rr.points==null||rr.points.Length<4)continue;rr.Initialize();var r2=rr;others.Add((p=>{float t=r2.Project(p,out float l);return l-r2.HalfWidth(t)+.5f;},0f,p=>{float t=r2.Project(p,out _);return r2.At(t,out _).y;},"road "+rr.name));}
   foreach(var spec in Environment.GetEnvironmentVariable("WORLD_STRIPS").Split('|')){var a=spec.Split(',');string name=a[0];float s0=float.Parse(a[1],inv),s1=float.Parse(a[2],inv),l0=float.Parse(a[3],inv),l1=float.Parse(a[4],inv),drop=float.Parse(a[5],inv);
    const float step=1f;int ns=Mathf.CeilToInt((s1-s0)/step)+1,nl=Mathf.CeilToInt((l1-l0)/step)+1;
    var P=new Vector3[ns,nl];var G=new float[ns,nl];var W=new float[ns,nl];var fill=new bool[ns,nl];var cols=new Collider[ns,nl];var mats=new Dictionary<Material,int>();int cells=0,skipped=0;float deepest=0;
    for(int i=0;i<ns;i++){float s=s0+i*step;var c=road.At(s,out var f);var right=Vector3.Cross(Vector3.up,Vector3.ProjectOnPlane(f,Vector3.up).normalized);
     for(int j=0;j<nl;j++){float l=l0+j*step;var p=c+right*l;float g=GroundAt(p.x,c.y,p.z,out cols[i,j]);if(float.IsNaN(g))g=c.y-30;
      bool innerHigh=Mathf.Abs(l1)<Mathf.Abs(l0);float outerDist=innerHigh?j*step:(nl-1-j)*step; // the edge next to the road is the verge itself: full weight there
      float edge=Mathf.Min(Mathf.Min(i*step,(ns-1-i)*step),outerDist);float w=Mathf.SmoothStep(0,1,Mathf.Clamp01(edge/8f));
      float plane=c.y-drop;float H=Mathf.Lerp(g,Mathf.Max(g,plane),w);bool fl=H>g+.15f;
      foreach(var (lat,hw,ry,nm) in others){if(lat(p)<hw&&Mathf.Abs(ry(p)-g)<3f){fl=false;skipped++;excl.TryGetValue(nm,out var q);excl[nm]=q+1;break;}}
      P[i,j]=new Vector3(p.x,fl?H:g+.025f,p.z);G[i,j]=g;fill[i,j]=fl;if(fl){cells++;deepest=Mathf.Max(deepest,H-g);if(cols[i,j]&&(i+j)%3==0){var mr=cols[i,j].GetComponent<MeshRenderer>();if(mr&&mr.sharedMaterial){mats.TryGetValue(mr.sharedMaterial,out var q);mats[mr.sharedMaterial]=q+1;}}}}}
    log.Add($"{scene} strip {name}: s {s0:F0}-{s1:F0}, lateral {l0:F0}..{l1:F0}, {cells} cells filled (deepest {deepest:F1} m), {skipped} cells left because another route is there");
    foreach(var kv in excl.OrderByDescending(k=>k.Value))log.Add($"   cells left for {kv.Key}: {kv.Value}");
    if(dry||cells==0)continue;
    var verts=new List<Vector3>();var idx=new int[ns,nl];for(int i=0;i<ns;i++)for(int j=0;j<nl;j++){idx[i,j]=verts.Count;verts.Add(P[i,j]);}
    var tris=new List<int>();for(int i=0;i+1<ns;i++)for(int j=0;j+1<nl;j++){if(!(fill[i,j]||fill[i+1,j]||fill[i,j+1]||fill[i+1,j+1]))continue;int A=idx[i,j],B=idx[i+1,j],C=idx[i,j+1],D=idx[i+1,j+1];
      void T(int p,int q,int r){var n=Vector3.Cross(verts[q]-verts[p],verts[r]-verts[p]);if(n.y<0){int t=q;q=r;r=t;}tris.AddRange(new[]{p,q,r});}T(A,B,C);T(B,D,C);}
    var mesh=new Mesh{name=$"{scene}-Report099-{name}",indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};mesh.SetVertices(verts);mesh.SetTriangles(tris,0);mesh.RecalculateNormals();mesh.RecalculateBounds();mesh.SetColors(verts.Select(_=>new Color32(89,102,77,255)).ToList());
    Directory.CreateDirectory("Assets/Scenery/Report099");AssetDatabase.CreateAsset(mesh,$"Assets/Scenery/Report099/{mesh.name}.asset");
    var go=new GameObject($"Ground_Report099 fill {name}");go.AddComponent<MeshFilter>().sharedMesh=mesh;var mrr=go.AddComponent<MeshRenderer>();var mat=mats.OrderByDescending(m=>m.Value).Select(m=>m.Key).FirstOrDefault();if(mat)mrr.sharedMaterial=mat;go.AddComponent<MeshCollider>().sharedMesh=mesh;UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go,sc);
    log.Add($"   mesh {mesh.name}: {verts.Count} vertices, {tris.Count/3} triangles, material {(mat?mat.name:"none")}");}
   if(!dry){Physics.SyncTransforms();EditorSceneManager.MarkSceneDirty(sc);EditorSceneManager.SaveScene(sc);}}
  Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/strip.txt",log);AssetDatabase.SaveAssets();EditorApplication.Exit(0);}
}
public static class Report099Dump {
 static string Out=>Environment.GetEnvironmentVariable("PROBE_OUT");
 // the map destinations (the places the radio names) and the race roads' names in each scene
 public static void Places(){
  var log=new List<string>();
  foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){
   EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");log.Add("==== "+scene);
   foreach(var m in UnityEngine.Object.FindObjectsByType<Racer.ExplorationMap>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(m.destinations!=null)foreach(var d in m.destinations)log.Add($"  place '{d.title}' id {d.id} at ({d.position.x:F0},{d.position.y:F0},{d.position.z:F0})");
   foreach(var r in UnityEngine.Object.FindObjectsByType<Racer.RaceRoad>(FindObjectsInactive.Exclude,FindObjectsSortMode.None))if(r.points!=null&&r.points.Length>3){r.Initialize();log.Add($"  road '{r.name}' length {r.Length:F0} highway {r.openHighway} halfwidth {r.HalfWidth(0):F1}");}}
  Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/places.txt",log);EditorApplication.Exit(0);}
}
