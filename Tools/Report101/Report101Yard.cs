using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 0.101 Parts B and C (editor only, copied into Assets/Editor/Report101Temp while it runs). Dan's house, every scene that has it.
//  B  Free Roam: the two paving vertices pulled up into spikes beside the garage are put back on the paving.
//  C  the garage/kennel turned so its three doors face the parking area (south), its west end against the fence; the parking area
//     paved up to the doors; the driveway strip along the old door side removed; a 1 m concrete sidewalk along its east end; the
//     white crossbuck fence around the yard (CR101 local boundary extension) replaced by galvanized chain-link on the same line,
//     each segment keeping its own collider.
// YARD_DRY=1: report only.
public static class Report101Yard {
 static string Out=>Environment.GetEnvironmentVariable("PROBE_OUT");
 static bool Dry=>Environment.GetEnvironmentVariable("YARD_DRY")=="1";
 const string NewDir="Assets/Scenery/Report101/";
 static readonly string[] Scenes={"FreeRoamWorld","DansBackyardForward","DansBackyardReverse","ForestLoopReverse","LakeWoods","MountainLoop","MountainLoopReverse","StreetLoopGreybox","StreetLoopReverse"};
 static readonly Vector3 OldC=new(398.89f,79.32f,24.51f);const float OldYaw=91.67f,NewYaw=181.67f;
 static Vector3 NewC;const float HalfLen=8.5f,HalfDepth=5f,DoorZ=16.0f;
 static readonly List<string> log=new();
 static string V(Vector3 v)=>$"({v.x:F2},{v.y:F2},{v.z:F2})";
 static string P(Transform t){var s=t.name;while(t.parent){t=t.parent;s=t.name+"/"+s;}return s;}
 static bool InGarage(Vector3 w,float pad=1.2f){var l=Quaternion.Inverse(Quaternion.Euler(0,OldYaw,0))*(w-OldC);return Mathf.Abs(l.x)<=HalfLen+pad&&Mathf.Abs(l.z)<=HalfDepth+pad&&l.y>-1.5f&&l.y<8f;}
 static float Terrain(Vector3 p,Func<Collider,bool> ok=null){float best=float.NaN;foreach(var h in Physics.RaycastAll(new Vector3(p.x,p.y+40,p.z),Vector3.down,120,~0,QueryTriggerInteraction.Ignore)){if(!h.collider.name.StartsWith("Ground_"))continue;if(ok!=null&&!ok(h.collider))continue;if(float.IsNaN(best)||h.point.y>best)best=h.point.y;}return best;}
 static Mesh SaveMesh(Mesh m,string path){if(Dry)return m;var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(old){EditorUtility.CopySerialized(m,old);return old;}AssetDatabase.CreateAsset(m,path);return m;}
 static bool IsSpur(Vector3 c)=>c.x>404.2f&&c.x<411.6f&&c.z>12.45f&&c.z<32.5f;

 public static void Run(){
  try{
   Directory.CreateDirectory(NewDir);
   // the new pose: doors (local +z) at z = DoorZ, west end (local +x) 0.12 m east of the fence line x = 389.17 + 0.0287 z
   var R=Quaternion.Euler(0,NewYaw,0);var dz=R*new Vector3(0,0,HalfDepth);float cz=DoorZ-dz.z;var dx=R*new Vector3(HalfLen,0,0);float zw=cz+dx.z;float cx=389.17f+.0287f*zw+.12f-dx.x;NewC=new Vector3(cx,OldC.y,cz);
   log.Add($"garage: from {V(OldC)} yaw {OldYaw} to {V(NewC)} yaw {NewYaw}; door face centre {V(NewC+dz)}, west end centre {V(NewC+dx)}, east end centre {V(NewC-dx)}");
   Mesh garageVisual=null;Material[] garageMats=null;var chain=Materials();var fenceMeshes=new Dictionary<string,Mesh>();var paveBySource=new Dictionary<string,string>();
   foreach(var sn in Scenes){
    var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+sn+".unity");Physics.SyncTransforms();log.Add("===== "+sn);
    var g=GameObject.Find("Separate downhill garage kennel");if(!g){log.Add("no garage here");continue;}
    bool already=Mathf.Abs(Mathf.DeltaAngle(g.transform.eulerAngles.y,NewYaw))<.1f;
    if(already)log.Add("garage already turned (re-run): pose kept");
    // ---- B: spikes (any scene's own paving; Free Roam's was the one Dan saw) ----
    var beige=UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).FirstOrDefault(r=>r.name=="Ground_Dan beige concrete descent and parking");
    var beigeMesh=beige.GetComponent<MeshFilter>().sharedMesh;var beigeCol=beige.GetComponent<MeshCollider>();
    log.Add($"paving {AssetDatabase.GetAssetPath(beigeMesh)} ({beigeMesh.vertexCount} vertices), collider mesh same {beigeCol&&beigeCol.sharedMesh==beigeMesh}");
    var bv=beigeMesh.vertices;var BM=beige.transform.localToWorldMatrix;var BMi=beige.transform.worldToLocalMatrix;var spikes=new List<int>();
    var nonBeige=new Func<Collider,bool>(c=>!c.name.Contains("beige"));
    for(int i=0;i<bv.Length;i++){var w=BM.MultiplyPoint3x4(bv[i]);float t=Terrain(w,nonBeige);if(!float.IsNaN(t)&&w.y-t>.6f)spikes.Add(i);}
    foreach(var i in spikes){var w=BM.MultiplyPoint3x4(bv[i]);log.Add($"  B: paving vertex {i} at {V(w)} stands {w.y-Terrain(w,nonBeige):F2} m above the ground {(Vector2.Distance(new(w.x,w.z),new(406.92f,11.88f))<6?"(Dan's spot)":"(elsewhere: for Dan)")}");}
    // ---- C: the paving: spur out, spikes flattened (one new asset per source asset) ----
    string src=AssetDatabase.GetAssetPath(beigeMesh);if(src.StartsWith(NewDir))throw new Exception("paving already replaced: run on a clean tree");
    if(!paveBySource.TryGetValue(src,out var pavePath)){pavePath=NewDir+(paveBySource.Count==0?"":paveBySource.Count+"-")+Path.GetFileNameWithoutExtension(src)+" (0.101).asset";paveBySource[src]=pavePath;}
    Mesh newPave=Dry?null:AssetDatabase.LoadAssetAtPath<Mesh>(pavePath);
    if(!newPave){
     var m=UnityEngine.Object.Instantiate(beigeMesh);var v=m.vertices;
     foreach(var i in spikes){var w=BM.MultiplyPoint3x4(v[i]);if(Vector2.Distance(new(w.x,w.z),new(406.92f,11.88f))>6)continue;var near=Enumerable.Range(0,v.Length).Where(j=>!spikes.Contains(j)).Select(j=>BM.MultiplyPoint3x4(v[j])).Where(q=>Vector2.Distance(new(q.x,q.z),new(w.x,w.z))<1.6f).ToList();float y=near.Count>0?near.Average(q=>q.y):Terrain(w,nonBeige)+.05f;log.Add($"  B: vertex {i} {w.y:F2} -> {y:F2} (the paving round it, {near.Count} vertices)");w.y=y;v[i]=BMi.MultiplyPoint3x4(w);}
     m.vertices=v;var t=m.triangles;var keep=new List<int>();int cut=0;for(int k=0;k<t.Length;k+=3){var c=BM.MultiplyPoint3x4((v[t[k]]+v[t[k+1]]+v[t[k+2]])/3);if(IsSpur(c)){cut++;continue;}keep.AddRange(new[]{t[k],t[k+1],t[k+2]});}
     m.SetTriangles(keep,0);m.RecalculateBounds();m.name=Path.GetFileNameWithoutExtension(pavePath);
     log.Add($"  C: paving: {cut} triangles of the strip along the old door side removed (x 404.2-411.6, z 12.45-32.5); mesh {pavePath}");newPave=SaveMesh(m,pavePath);}
    else log.Add($"  C: paving: the 0.101 mesh {pavePath} (already made)");
    if(!Dry){beige.GetComponent<MeshFilter>().sharedMesh=newPave;if(beigeCol){beigeCol.sharedMesh=null;beigeCol.sharedMesh=newPave;}EditorUtility.SetDirty(beige.gameObject);Physics.SyncTransforms();}
    // what used the strip: roads or trails along it
    foreach(var r in UnityEngine.Object.FindObjectsByType<Racer.RaceRoad>(FindObjectsSortMode.None)){if(r.points==null)continue;foreach(var p in r.points)if(IsSpur(p)&&p.z>16.5f){log.Add($"  WARNING road {P(r.transform)} has a point on the strip at {V(p)}");break;}}
    // ---- C: the garage: visual out of the house batches into its own child, the building turned ----
    var house=g.transform.parent;
    foreach(var r in house.GetComponentsInChildren<MeshRenderer>(true)){var mf=r.GetComponent<MeshFilter>();if(!mf||!mf.sharedMesh||r.transform.IsChildOf(g.transform))continue;var m=mf.sharedMesh;var mv=m.vertices;var M=r.transform.localToWorldMatrix;
     for(int sm=0;sm<m.subMeshCount;sm++){var t=m.GetTriangles(sm);int inside=0;for(int k=0;k<t.Length;k+=3)if(InGarage(M.MultiplyPoint3x4((mv[t[k]]+mv[t[k+1]]+mv[t[k+2]])/3)))inside++;if(inside>0)log.Add($"  garage triangles in the house batch {r.name} ({AssetDatabase.GetAssetPath(m)}) sub {sm}: {inside}");}}
    if(garageVisual==null){garageVisual=AssetDatabase.LoadAssetAtPath<Mesh>(NewDir+"Dan garage kennel (0.101).asset");garageMats=null;}
    if(!already){
     var parts=new List<(Material mat,List<Vector3> v,List<Vector3> nrm,List<Vector2> uv,List<Color> col)>();var Gi=Matrix4x4.TRS(OldC,Quaternion.Euler(0,OldYaw,0),Vector3.one).inverse;
     foreach(var r in house.GetComponentsInChildren<MeshRenderer>(true)){var mf=r.GetComponent<MeshFilter>();if(!mf||!mf.sharedMesh||r.transform.IsChildOf(g.transform))continue;var m=mf.sharedMesh;var mv=m.vertices;var mn=m.normals;var mu=m.uv;var mc=m.colors;var M=r.transform.localToWorldMatrix;bool any=false;
      var newSubs=new List<int[]>();
      for(int sm=0;sm<m.subMeshCount;sm++){var t=m.GetTriangles(sm);var keep=new List<int>();var smat=r.sharedMaterials[Mathf.Min(sm,r.sharedMaterials.Length-1)];var part=parts.FirstOrDefault(p=>p.mat==smat);if(part.mat==null){part=(smat,new(),new(),new(),new());parts.Add(part);}
       for(int k=0;k<t.Length;k+=3){if(!InGarage(M.MultiplyPoint3x4((mv[t[k]]+mv[t[k+1]]+mv[t[k+2]])/3))){keep.AddRange(new[]{t[k],t[k+1],t[k+2]});continue;}any=true;
        foreach(var q in new[]{t[k],t[k+1],t[k+2]}){part.v.Add(Gi.MultiplyPoint3x4(M.MultiplyPoint3x4(mv[q])));part.nrm.Add(Gi.MultiplyVector(M.MultiplyVector(mn.Length>q?mn[q]:Vector3.up)).normalized);part.uv.Add(mu.Length>q?mu[q]:Vector2.zero);part.col.Add(mc.Length>q?mc[q]:Color.white);}}
       newSubs.Add(keep.ToArray());}
      if(!any)continue;
      // the house batch is shared by every scene: strip the garage out of the asset once
      var copy=UnityEngine.Object.Instantiate(m);for(int sm=0;sm<newSubs.Count;sm++)copy.SetTriangles(newSubs[sm],sm);copy.RecalculateBounds();
      if(!Dry){EditorUtility.CopySerialized(copy,m);EditorUtility.SetDirty(m);}log.Add($"  garage removed from the shared house batch {AssetDatabase.GetAssetPath(m)}");}
     parts=parts.Where(p=>p.v.Count>0).ToList();
     if(parts.Count>0&&!Dry){var gm=new Mesh{name="Dan garage kennel (0.101)"};var allV=new List<Vector3>();var allN=new List<Vector3>();var allU=new List<Vector2>();var allC=new List<Color>();gm.subMeshCount=parts.Count;var subs=new List<int[]>();
      foreach(var p in parts){int b=allV.Count;allV.AddRange(p.v);allN.AddRange(p.nrm);allU.AddRange(p.uv);allC.AddRange(p.col);subs.Add(Enumerable.Range(b,p.v.Count).ToArray());}
      gm.SetVertices(allV);gm.SetNormals(allN);gm.SetUVs(0,allU);gm.SetColors(allC);for(int i=0;i<subs.Count;i++)gm.SetTriangles(subs[i],i);gm.RecalculateBounds();garageVisual=SaveMesh(gm,NewDir+"Dan garage kennel (0.101).asset");garageMats=parts.Select(p=>p.mat).ToArray();
      log.Add($"  garage visual: {allV.Count/3} triangles in {parts.Count} materials ({string.Join(", ",parts.Select(p=>p.mat.name+" "+p.v.Count/3))}), local bounds {V(gm.bounds.min)}-{V(gm.bounds.max)}");}}
    if(!Dry){
     var vis=g.transform.Find("Garage visual (0.101)");
     if(!vis){if(garageMats==null||!garageVisual)throw new Exception("garage visual not built (run on a clean tree)");
      var go=new GameObject("Garage visual (0.101)");go.transform.SetParent(g.transform,false);go.AddComponent<MeshFilter>().sharedMesh=garageVisual;go.AddComponent<MeshRenderer>().sharedMaterials=garageMats;}
     g.transform.SetPositionAndRotation(NewC,Quaternion.Euler(0,NewYaw,0));EditorUtility.SetDirty(g);Physics.SyncTransforms();}
    var walls=g.GetComponentInChildren<BoxCollider>();if(walls)log.Add($"  garage walls collider now {V(walls.bounds.min)}-{V(walls.bounds.max)}");
    // ground under the new footprint (base 79.32)
    {float lo=float.MaxValue,hi=float.MinValue;var w=Quaternion.Euler(0,NewYaw,0);foreach(float lx in new[]{-HalfLen,0,HalfLen})foreach(float lz in new[]{-HalfDepth,0,HalfDepth}){var p=NewC+w*new Vector3(lx,0,lz);float t=Terrain(p+Vector3.up*3,c=>!c.name.Contains("parking to the garage"));lo=Mathf.Min(lo,t);hi=Mathf.Max(hi,t);}log.Add($"  ground round the turned building: {lo:F2}..{hi:F2} (base {OldC.y:F2}: nothing floats; the back west corner sits {hi-OldC.y:F2} m into the bank)");}
    foreach(var c in UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsSortMode.None)){if(c.transform.IsChildOf(g.transform)||c.name.StartsWith("Ground_")||c.name.Contains("crossbuck"))continue;var b=c.bounds;var l=Quaternion.Inverse(Quaternion.Euler(0,NewYaw,0))*(b.center-NewC);if(Mathf.Abs(l.x)<HalfLen+b.extents.x+1.2f&&Mathf.Abs(l.z)<HalfDepth+b.extents.z+1.2f&&b.max.y>79&&b.min.y<84)log.Add($"  WARNING something in the new footprint: {P(c.transform)} {V(b.center)}");}
    // ---- C: parking pad up to the doors, and the sidewalk ----
    var mat=beige.sharedMaterial;
    Mesh Slab(string name,Func<float,float,Vector3> at,int nu,int nv,Func<Vector3,float> height){var vs=new List<Vector3>();var ts=new List<int>();for(int i=0;i<=nu;i++)for(int j=0;j<=nv;j++){var p=at((float)i/nu,(float)j/nv);p.y=height(p);vs.Add(p);}
     for(int i=1;i<=nu;i++)for(int j=1;j<=nv;j++){int a=(i-1)*(nv+1)+j-1,b=a+1,c=i*(nv+1)+j-1,e=c+1;ts.AddRange(new[]{a,b,c,b,e,c});}
     var m=new Mesh{name=name};m.SetVertices(vs);m.SetTriangles(ts,0);m.RecalculateNormals();if(m.normals[0].y<0){for(int k=0;k<ts.Count;k+=3)(ts[k+1],ts[k+2])=(ts[k+2],ts[k+1]);m.SetTriangles(ts,0);m.RecalculateNormals();}
     var uvs=vs.Select(p=>new Vector2(p.x*.25f,p.z*.25f)).ToList();m.SetUVs(0,uvs);m.RecalculateBounds();return m;}
    bool NotNew(Collider c)=>!c.name.Contains("(0.101)");
    float PadHeight(Vector3 p){float b=float.NaN;foreach(var h in Physics.RaycastAll(new Vector3(p.x,p.y+40,p.z),Vector3.down,120,~0,QueryTriggerInteraction.Ignore))if(h.collider==beigeCol&&(float.IsNaN(b)||h.point.y>b))b=h.point.y;float t=Terrain(p,c=>!c.name.Contains("beige")&&NotNew(c));return float.IsNaN(b)?Mathf.Max(t+.08f,79.40f):b+.015f;}
    {var doorLine=NewC+dz;float x0=391.4f,x1=410.75f,z0=11.85f;
     var pad=Slab("Dan parking to the garage doors (0.101) "+sn,(u,v)=>new Vector3(Mathf.Lerp(x0,x1,u),79.4f,Mathf.Lerp(z0,doorLine.z+(Mathf.Lerp(x0,x1,u)-doorLine.x)*-(dz.x/dz.z)+.3f,v)),40,10,PadHeight);pad=SaveMesh(pad,NewDir+sn+"-Dan parking to the garage doors.asset");
     var swR=Quaternion.Euler(0,NewYaw,0);var sw=Slab("Dan garage sidewalk (0.101) "+sn,(u,v)=>NewC+swR*new Vector3(-HalfLen-.03f-1.0f*u,0,Mathf.Lerp(HalfDepth-.3f,-HalfDepth-.05f,v)),2,20,p=>Terrain(p,c=>!c.name.Contains("beige")&&NotNew(c))+.10f);sw=SaveMesh(sw,NewDir+sn+"-Dan garage sidewalk.asset");
     if(!Dry)foreach(var (n,m) in new[]{("Ground_Dan beige parking to the garage doors (0.101)",pad),("Ground_Dan garage sidewalk (0.101)",sw)}){var old=beige.transform.parent.Find(n);if(old)UnityEngine.Object.DestroyImmediate(old.gameObject);var go=new GameObject(n);go.transform.SetParent(beige.transform.parent,false);go.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);go.AddComponent<MeshFilter>().sharedMesh=m;go.AddComponent<MeshRenderer>().sharedMaterial=mat;go.AddComponent<MeshCollider>().sharedMesh=m;}
     log.Add($"  parking pad x {x0}-{x1}, z {z0} to the door line (+0.3 m under the doors), {pad.bounds.min.y:F2}..{pad.bounds.max.y:F2}; sidewalk 1.0 m along the east end, {sw.bounds.min.y:F2}..{sw.bounds.max.y:F2}");}
    // the strip's ground now open: what is under it
    {int n=0;float worst=0;for(float x=405f;x<=410.5f;x+=1)for(float z=17;z<=31;z+=1){var p=new Vector3(x,80,z);float t=Terrain(p,c=>!c.name.Contains("beige")&&NotNew(c));if(float.IsNaN(t)){n++;continue;}worst=Mathf.Max(worst,79.46f-t);}log.Add($"  ground under the removed strip: {n} holes, lowest {worst:F2} m below the old paving");}
    // ---- C: chain-link fence ----
    var fences=UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Where(f=>f.name=="Grounded wood crossbuck"&&f.transform.parent&&f.transform.parent.name=="CR101 local boundary extension").ToList();
    log.Add($"  fence: {fences.Count} white crossbuck segments in 'CR101 local boundary extension'");
    var ends=fences.Select(f=>(f,a:f.transform.Find("Fence endpoint A").position,b:f.transform.Find("Fence endpoint B").position)).ToList();
    foreach(var (f,a,b) in ends){bool postB=!ends.Any(o=>o.f!=f&&Vector3.Distance(o.a,b)<.15f);string key=(f.sharedMesh?f.sharedMesh.name:f.name)+$"|{a.x:F2},{a.y:F2},{a.z:F2}|{b.x:F2},{b.y:F2},{b.z:F2}|{postB}";
     if(!fenceMeshes.TryGetValue(key,out var cm)){cm=ChainLink(f.transform,a,b,postB);cm=SaveMesh(cm,NewDir+$"Chain-link fence (0.101) {fenceMeshes.Count:D2}.asset");fenceMeshes[key]=cm;}
     if(!Dry){f.sharedMesh=cm;f.GetComponent<MeshRenderer>().sharedMaterials=new[]{chain.frame,chain.fabric};f.name="Grounded chain-link fence";EditorUtility.SetDirty(f.gameObject);}
     var bc=f.GetComponent<BoxCollider>();if(!bc)log.Add($"  WARNING fence segment without a collider at {V(a)}");}
    log.Add($"  fence: replaced by chain-link (posts, top rail, fabric); colliders kept; {fenceMeshes.Count} distinct segment meshes so far");
    if(!Dry){EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);}
   }
   if(!Dry)AssetDatabase.SaveAssets();
  }catch(Exception e){log.Add("FAILED "+e);}
  Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/yard.txt",log);EditorApplication.Exit(0);}



 static (Material frame,Material fabric) Materials(){
  string fp=NewDir+"Galvanized chain-link frame.mat",cp=NewDir+"Galvanized chain-link fabric.mat",tp=NewDir+"Chain-link fabric.png";
  var lit=Shader.Find("Universal Render Pipeline/Lit");
  if(!AssetDatabase.LoadAssetAtPath<Texture2D>(tp)&&!Dry){const int S=128;var tex=new Texture2D(S,S,TextureFormat.RGBA32,false);var px=new Color32[S*S];
   // 4 x 4 diamonds per tile: wires along both diagonals, about 2 px thick
   for(int y=0;y<S;y++)for(int x=0;x<S;x++){int u=(x+y)%32,v=((x-y)%32+32)%32;bool wire=u<2||v<2;px[y*S+x]=wire?new Color32(200,204,208,255):new Color32(200,204,208,0);}
   tex.SetPixels32(px);File.WriteAllBytes(tp,tex.EncodeToPNG());AssetDatabase.ImportAsset(tp);
   var imp=(TextureImporter)AssetImporter.GetAtPath(tp);imp.alphaIsTransparency=true;imp.mipmapEnabled=true;imp.mipMapsPreserveCoverage=true;imp.alphaTestReferenceValue=.4f;imp.wrapMode=TextureWrapMode.Repeat;imp.anisoLevel=4;imp.SaveAndReimport();}
  Material Make(string path,Color c,float metal,float smooth,bool fabric){var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(m)return m;m=new Material(lit);m.SetColor("_BaseColor",c);m.SetFloat("_Metallic",metal);m.SetFloat("_Smoothness",smooth);
   if(fabric){m.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(tp));m.SetFloat("_AlphaClip",1);m.SetFloat("_Cutoff",.4f);m.SetFloat("_Cull",2);m.EnableKeyword("_ALPHATEST_ON");m.renderQueue=2450;m.SetOverrideTag("RenderType","TransparentCutout");}
   if(!Dry)AssetDatabase.CreateAsset(m,path);return m;}
  return (Make(fp,new Color(.60f,.62f,.64f),.6f,.45f,false),Make(cp,new Color(.78f,.80f,.82f),.5f,.4f,true));}

 // one segment in the fence object's own space: a post at A (and at B where no neighbour starts), a top rail, the fabric (both faces)
 static Mesh ChainLink(Transform t,Vector3 a,Vector3 b,bool postB){
  const float H=1.55f,PostR=.035f,RailR=.022f;var sub0=new List<int>();var sub1=new List<int>();var vs=new List<Vector3>();var ns=new List<Vector3>();var uv=new List<Vector2>();
  void Tube(Vector3 p0,Vector3 p1,float r,int sides){var ax=(p1-p0).normalized;var u=Vector3.Cross(ax,Mathf.Abs(ax.y)>.9f?Vector3.right:Vector3.up).normalized;var w=Vector3.Cross(ax,u);int b0=vs.Count;
   for(int i=0;i<=sides;i++){float ang=i*Mathf.PI*2/sides;var dir=u*Mathf.Cos(ang)+w*Mathf.Sin(ang);foreach(var e in new[]{p0,p1}){vs.Add(t.InverseTransformPoint(e+dir*r));ns.Add(t.InverseTransformDirection(dir));uv.Add(Vector2.zero);}}
   for(int i=0;i<sides;i++){int k=b0+i*2;sub0.AddRange(new[]{k,k+1,k+2,k+1,k+3,k+2});}
   // caps
   foreach(var (e,s) in new[]{(p1,1),(p0,0)}){int c=vs.Count;vs.Add(t.InverseTransformPoint(e));ns.Add(t.InverseTransformDirection(s==1?ax:-ax));uv.Add(Vector2.zero);int r0=vs.Count;for(int i=0;i<=sides;i++){float ang=i*Mathf.PI*2/sides;vs.Add(t.InverseTransformPoint(e+(u*Mathf.Cos(ang)+w*Mathf.Sin(ang))*r));ns.Add(t.InverseTransformDirection(s==1?ax:-ax));uv.Add(Vector2.zero);}for(int i=0;i<sides;i++)sub0.AddRange(s==1?new[]{c,r0+i+1,r0+i}:new[]{c,r0+i,r0+i+1});}}
  Tube(a-Vector3.up*.15f,a+Vector3.up*(H+.05f),PostR,8);if(postB)Tube(b-Vector3.up*.15f,b+Vector3.up*(H+.05f),PostR,8);
  Tube(a+Vector3.up*H,b+Vector3.up*H,RailR,6);
  // fabric: from 3 cm above the ground to the rail, following the ground at each end
  float len=Vector3.Distance(new Vector3(a.x,0,a.z),new Vector3(b.x,0,b.z));var side=Vector3.Cross(Vector3.up,(b-a).normalized).normalized;
  foreach(var face in new[]{1,-1}){int k=vs.Count;var q=new[]{a+Vector3.up*.03f,b+Vector3.up*.03f,b+Vector3.up*(H-.02f),a+Vector3.up*(H-.02f)};var quv=new[]{new Vector2(0,0),new Vector2(len/.25f,0),new Vector2(len/.25f,(H-.05f)/.25f),new Vector2(0,(H-.05f)/.25f)};
   for(int i=0;i<4;i++){vs.Add(t.InverseTransformPoint(q[i]+side*.005f*face));ns.Add(t.InverseTransformDirection(side*face));uv.Add(quv[i]);}
   sub1.AddRange(face==1?new[]{k,k+1,k+2,k,k+2,k+3}:new[]{k,k+2,k+1,k,k+3,k+2});}
  var m=new Mesh{name="Chain-link fence segment (0.101)"};m.SetVertices(vs);m.SetNormals(ns);m.SetUVs(0,uv);m.subMeshCount=2;m.SetTriangles(sub0,0);m.SetTriangles(sub1,1);m.RecalculateBounds();return m;}
}
