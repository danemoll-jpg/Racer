using System;using System.IO;using System.Linq;using System.Text;using System.Collections.Generic;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;using Racer;using Object=UnityEngine.Object;
// 0.87 Part A: Kyle's house from Dan's photo, in every scene that has it (the same site, "Friend across street - blue
// circle", at the same place in all nine). House frame = the site's transform: x right (the photo's left, north), y up,
// z forward (the front, west). The site stands on a flat pad dug into the slope (front / street side rising 1-9 m, a
// 4-6 m bank 3-5 m behind the back wall). So:
//  - the model (Report087KyleModel) and its colliders are lifted 2.45 m: the walk-out lower floor (garage, back yard,
//    deck stairs) is on the existing pad, the main floor one storey up;
//  - the ground in front is filled to the front grade (2.45: a terrace in front of the porch and the front of the right
//    side, 1:2 down to the existing ground), so from the front the house is one storey, the porch one low step up;
//  - behind, the bank is cut back to the pad for the deck and its stairs (1:1.5 up to the existing ground);
//  - the driveway's last leg to the front is taken out and it continues to a gravel apron at the two garage doors on the
//    left side (its route data and the map's "Anderson's" arrival point follow);
//  - the old house (House 3 prefab instance, foundation, steps) is removed; trees standing in the new footprint, the apron
//    or the drive are removed, those on changed ground and small objects are re-seated.
// Ground tiles shared between scenes (all of them among these nine, all getting the same house) are edited once.
// KYLE_DRY=1 reports only.
public static class Report087KyleBuild {
 const float OFF=2.45f,FRONT=2.45f;const string Site="Friend across street - blue circle";
 static readonly string[] Scenes={"FreeRoamWorld","LakeWoods","ForestLoopReverse","MountainLoop","MountainLoopReverse","StreetLoopGreybox","StreetLoopReverse","DansBackyardForward","DansBackyardReverse"};
 static readonly StringBuilder rep=new();static readonly HashSet<string> edited=new();
 // the ground changes made to each tile asset (house frame), so a scene sharing an already-edited tile re-seats the same things
 static readonly Dictionary<string,List<(Vector3 l,float dy)>> changesByAsset=new();
 static string V(Vector3 v)=>$"{v.x:F2},{v.y:F2},{v.z:F2}";
 static string P(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n;}
 static bool TreeName(string n){n=n.ToLowerInvariant();return n.Contains("trunk")||n.Contains("tree");}
 static bool Veg(Renderer r){var m=r.sharedMaterial;return m&&m.shader&&m.shader.name=="Racer/GreyboxGround"&&m.HasProperty("_Vegetation")&&m.GetFloat("_Vegetation")>.5f;}
 // rectangles in the house frame (x0,x1,z0,z1)
 static float Out(float x,float z,float x0,float x1,float z0,float z1){float dx=Mathf.Max(x0-x,0,x-x1),dz=Mathf.Max(z0-z,0,z-z1);return Mathf.Sqrt(dx*dx+dz*dz);}
 static readonly (float x0,float x1,float z0,float z1)[] Footprint={(-7.6f,7.6f,-4.6f,4.6f),(2.2f,7.6f,4.6f,6.4f),(-7.6f,.4f,4.6f,6.5f),(-11.4f,-7.6f,-4.2f,1.45f),(-8.6f,-7.6f,1.75f,3.35f),(-4.9f,1.9f,-8.4f,-4.6f),(-9.3f,-4.9f,-8.4f,-7.3f)};
 static readonly (float x0,float x1,float z0,float z1) Apron=(7.6f,15.2f,-5.2f,1.6f),Yard=(-10.4f,8.6f,-9.3f,-4.6f);
 // the drive's new leg: from the existing drive (local x 21.5) to the apron; y set per scene from the existing drive surface
 static Vector3[] spur;const float SpurHW=2.6f;
 static float SpurLocal(float x,float z,out float lat,out float y){float best=1e9f,s=0,acc=0;lat=0;y=0;for(int i=1;i<spur.Length;i++){var a=spur[i-1];var b=spur[i];var ab=new Vector2(b.x-a.x,b.z-a.z);var ap=new Vector2(x-a.x,z-a.z);float t=Mathf.Clamp01(Vector2.Dot(ap,ab)/ab.sqrMagnitude);float d=(ap-ab*t).magnitude;if(d<best){best=d;s=acc+ab.magnitude*t;y=Mathf.Lerp(a.y,b.y,t);}acc+=ab.magnitude;}lat=best;return s;}
 // the kept part of the old drive (house frame): the ground under its ribbon is not touched
 static Vector3[] oldDrive=new Vector3[0];const float Trim=22.5f;
 static bool UnderOldDrive(float x,float z){if(x<Trim)return false;for(int i=1;i<oldDrive.Length;i++){var a=oldDrive[i-1];var b=oldDrive[i];var ab=new Vector2(b.x-a.x,b.z-a.z);if(ab.sqrMagnitude<1e-4f)continue;var ap=new Vector2(x-a.x,z-a.z);float t=Mathf.Clamp01(Vector2.Dot(ap,ab)/ab.sqrMagnitude);if((ap-ab*t).magnitude<4.2f)return true;}return false;}
 static float Target(float x,float z,float y0,out bool any){any=false;float y=y0;if(UnderOldDrive(x,z))return y0;
  // 1. the front terrace (fill only)
  float dT=Mathf.Min(Out(x,z,-12.6f,8.6f,4.6f,9.6f),Out(x,z,-12.6f,-7.6f,1.6f,4.6f));if(dT<5.2f){float t=FRONT-.5f*dT;if(t>y){y=t;any=true;}}
  // 2. the back yard behind the deck (cut only, 1:1 back up to the bank)
  float dB=Out(x,z,Yard.x0,Yard.x1,Yard.z0,Yard.z1);{float t=1f*dB;if(dB<9&&t<y){y=t;any=true;}}
  // 3. the garage apron: exactly the pad, batters out
  float dA=Out(x,z,Apron.x0,Apron.x1,Apron.z0,Apron.z1)-1f;if(dA<=0){y=0;any=true;}else if(dA<7){float hi=.67f*dA,lo=-.5f*dA;if(y>hi){y=hi;any=true;}else if(y<lo){y=lo;any=true;}}
  // 4. the drive's new leg
  SpurLocal(x,z,out float lat,out float ys);float core=SpurHW+1.2f;if(lat<=core){y=ys-.03f;any=true;}else if(lat<core+7){float e=lat-core;float hi=ys-.03f+.67f*e,lo=ys-.03f-.5f*e;if(y>hi){y=hi;any=true;}else if(y<lo){y=lo;any=true;}}
  return y;}
 public static void Run(){bool dry=Environment.GetEnvironmentVariable("KYLE_DRY")=="1";var o=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(o);
  try{var model=Report087KyleModel.Build(rep);var cols=Report087KyleModel.Colliders(rep);var building=AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/Scenery/Building.mat");if(!building)throw new Exception("Building.mat");
   var only=Environment.GetEnvironmentVariable("KYLE_SCENES");foreach(var sn in only!=null?only.Split(','):Scenes)Scene(sn,model,cols,building,dry);}
  catch(Exception e){rep.AppendLine("ERROR "+e);}
  File.WriteAllText(o+"/A-kyle-build.txt",rep.ToString());EditorApplication.Exit(0);}
 sealed class Piece{public MeshFilter mf;public int[] idx;public Bounds b;public bool bark;public int owner=-1;}
 sealed class Tree{public Vector3 bottom;public Collider col;public float r,h;public bool remove;public float dy;public List<Piece> pieces=new();}
 static void Scene(string sn,Mesh model,List<(string kind,string name,Vector3 c,Vector3 s,Mesh m)> cols,Material building,bool dry){
  rep.AppendLine($"== {sn}");var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+sn+".unity");Physics.SyncTransforms();var roots=scene.GetRootGameObjects();
  var site=roots.SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).FirstOrDefault(t=>t.name==Site);if(!site){rep.AppendLine("  no house");return;}
  if(site.Find("Kyle's house (0.87)"))throw new Exception("already built in "+sn);
  if(Vector3.Distance(site.position,new Vector3(511.5f,78.66718f,-35.2f))>.01f)throw new Exception("site moved in "+sn);
  Vector3 L(Vector3 w)=>site.InverseTransformPoint(w);Vector3 W(Vector3 l)=>site.TransformPoint(l);
  var region=new Bounds(W(new Vector3(6,0,0)),new Vector3(70,60,70));
  // ---- the old house
  var arch=site.Find("Phase 6 architecture");int layer=site.gameObject.layer;bool isStatic=site.gameObject.isStatic;
  var oldCols=site.GetComponentsInChildren<Collider>(true).Select(c=>$"{P(c.transform).Substring(P(site).Length+1)} {c.GetType().Name} {V(c.bounds.min)} - {V(c.bounds.max)}").ToList();
  if(arch){var wc=arch.GetComponentsInChildren<Collider>(true).FirstOrDefault();if(wc){layer=wc.gameObject.layer;isStatic=wc.gameObject.isStatic;}}
  rep.AppendLine($"  old colliders: {oldCols.Count}");foreach(var c in oldCols)rep.AppendLine("    "+c);
  // ---- driveway: the existing drive (route + ribbon) and the new leg's height where it leaves it
  var driveRoad=roots.SelectMany(g=>g.GetComponentsInChildren<RaceRoad>(true)).FirstOrDefault(r=>r.name=="Kyle descending driveway");
  var ribbon=roots.SelectMany(g=>g.GetComponentsInChildren<MeshCollider>(true)).FirstOrDefault(c=>c.name=="Ground_Kyle descending drive");
  float DriveY(Vector3 lp){if(ribbon){var w=W(lp);if(ribbon.Raycast(new Ray(w+Vector3.up*20,Vector3.down),out var h,40))return L(h.point).y;}float best=float.NaN;foreach(var h in Physics.RaycastAll(W(lp)+Vector3.up*20,Vector3.down,40,~0,QueryTriggerInteraction.Ignore))if(h.collider.name.StartsWith("Ground")&&(float.IsNaN(best)||h.point.y>best))best=h.point.y;return float.IsNaN(best)?0:best-site.position.y;}
  oldDrive=driveRoad?driveRoad.points.Select(L).ToArray():new Vector3[0];
  // where the old ribbon is cut (a straight line across the drive at local x = Trim): its two ends
  Vector3 cutA=new Vector3(Trim,0,4.2f),cutB=new Vector3(Trim,0,11.8f);
  if(ribbon){var rm=ribbon.sharedMesh;var rv=rm.vertices.Select(x=>L(ribbon.transform.TransformPoint(x))).ToArray();var rt=rm.triangles;var hitsZ=new List<Vector3>();
   for(int k=0;k<rt.Length;k+=3)for(int e=0;e<3;e++){var a=rv[rt[k+e]];var b=rv[rt[k+(e+1)%3]];if((a.x-Trim)*(b.x-Trim)<0){float f=(Trim-a.x)/(b.x-a.x);hitsZ.Add(Vector3.Lerp(a,b,f));}}
   if(hitsZ.Count>=2){cutA=hitsZ.OrderBy(h=>h.z).First();cutB=hitsZ.OrderBy(h=>h.z).Last();}}
  var cutMid=(cutA+cutB)*.5f;float y0=cutMid.y;
  spur=new[]{cutMid,new Vector3(18.6f,Mathf.Lerp(y0,0,.5f),4.8f),new Vector3(15.6f,0,1.0f),new Vector3(14.6f,0,-.6f)};
  rep.AppendLine($"  drive: the new leg leaves the existing drive at local ({Trim}, {y0:F2}, 8.0) and descends to the apron at the pad (0) over {Vector3.Distance(spur[0],spur[^1]):F1} m");
  // ---- ground
  var changes=new List<(Vector3 l,float dy)>();
  foreach(var mc in roots.SelectMany(g=>g.GetComponentsInChildren<MeshCollider>(true)).Where(c=>c.name.StartsWith("Ground_")&&c.sharedMesh&&c.transform.parent&&(c.transform.parent.name.StartsWith("Memory loop")||c.name=="Ground_CR113 Kyle driveway edge join")&&c.bounds.Intersects(region))){
   var mesh=mc.sharedMesh;var path=AssetDatabase.GetAssetPath(mesh);var t=mc.transform;var v=mesh.vertices;var changed=new bool[v.Length];int n=0;float cut=0,fill=0,mx=0;
   if(changesByAsset.TryGetValue(path,out var prior)){changes.AddRange(prior);rep.AppendLine($"  ground {P(t)} [{path}]: already edited for an earlier scene ({prior.Count} changed vertices; their changes are used here to re-seat trees and objects)");continue;}
   var mine=new List<(Vector3 l,float dy)>();
   for(int i=0;i<v.Length;i++){var w=t.TransformPoint(v[i]);var l=L(w);if(l.x<-26||l.x>34||l.z<-22||l.z>22)continue;float ny=Target(l.x,l.z,l.y,out bool any);if(!any||Mathf.Abs(ny-l.y)<.004f)continue;
    changes.Add((new Vector3(l.x,ny,l.z),ny-l.y));mine.Add((new Vector3(l.x,ny,l.z),ny-l.y));changed[i]=true;n++;if(ny>l.y)fill+=ny-l.y;else cut+=l.y-ny;mx=Mathf.Max(mx,Mathf.Abs(ny-l.y));if(!edited.Contains(path))v[i]=t.InverseTransformPoint(W(new Vector3(l.x,ny,l.z)));}
   string users=string.Join(",",AssetDatabase.FindAssets("t:Scene",new[]{"Assets/Scenes"}).Select(AssetDatabase.GUIDToAssetPath).Where(sp=>AssetDatabase.GetDependencies(sp,false).Contains(path)).Select(Path.GetFileNameWithoutExtension));
   if(users.Split(',').Any(u=>!Scenes.Contains(u)))throw new Exception($"{path} is also used by {users}");
   rep.AppendLine($"  ground {P(t)} [{path}] (scenes {users}): {n} vertices, raised {fill:F0} / lowered {cut:F0} (vertex-metres), largest {mx:F2} m{(edited.Contains(path)?"; asset already edited for an earlier scene":"")}");
   changesByAsset[path]=mine;
   if(dry||n==0||edited.Contains(path))continue;edited.Add(path);
   var oldN=mesh.normals;mesh.vertices=v;mesh.RecalculateNormals();var nn=mesh.normals;var near=new HashSet<int>();var tr=mesh.triangles;for(int k=0;k<tr.Length;k+=3)if(changed[tr[k]]||changed[tr[k+1]]||changed[tr[k+2]]){near.Add(tr[k]);near.Add(tr[k+1]);near.Add(tr[k+2]);}
   for(int k=0;k<nn.Length;k++)if(!near.Contains(k))nn[k]=oldN[k];mesh.normals=nn;mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);}
  if(!dry)foreach(var mc in roots.SelectMany(g=>g.GetComponentsInChildren<MeshCollider>(true)).Where(c=>c.name.StartsWith("Ground_")&&c.sharedMesh&&edited.Contains(AssetDatabase.GetAssetPath(c.sharedMesh)))){var m=mc.sharedMesh;mc.sharedMesh=null;mc.sharedMesh=m;}
  Physics.SyncTransforms();
  // the change of the ground under an object's base: the ground rule at its own place, its base taken as the ground it stood on
  float After(Vector3 l){var w=W(new Vector3(l.x,0,l.z));float best=float.NaN;foreach(var h in Physics.RaycastAll(new Vector3(w.x,w.y+40,w.z),Vector3.down,90,~0,QueryTriggerInteraction.Ignore)){if(!h.collider.name.StartsWith("Ground_")||h.collider.name.Contains("drive"))continue;if(float.IsNaN(best)||h.point.y>best)best=h.point.y;}return best-site.position.y;}
  float DyAt(Vector3 l){if(l.x<-26||l.x>34||l.z<-22||l.z>22)return 0;Target(l.x,l.z,l.y,out bool any);if(!any||dry)return any?Target(l.x,l.z,l.y,out _)-l.y:0;float g=After(l);return float.IsNaN(g)?0:g-l.y;}
  bool InFootprint(Vector3 l,float m){foreach(var r in Footprint)if(Out(l.x,l.z,r.x0,r.x1,r.z0,r.z1)<m)return true;if(Out(l.x,l.z,Apron.x0,Apron.x1,Apron.z0,Apron.z1)<m)return true;SpurLocal(l.x,l.z,out float lat,out _);return lat<SpurHW+m;}
  // ---- trees: sites with their drawn pieces
  var vegs=roots.SelectMany(g=>g.GetComponentsInChildren<MeshRenderer>(true)).Where(r=>Veg(r)&&r.bounds.Intersects(region)).Select(r=>r.GetComponent<MeshFilter>()).Where(m=>m&&m.sharedMesh&&m.sharedMesh.isReadable).ToList();
  var pieces=new List<Piece>();
  foreach(var mf in vegs){var m=mf.sharedMesh;var wv=m.vertices.Select(x=>mf.transform.TransformPoint(x)).ToArray();var par=Enumerable.Range(0,wv.Length).ToArray();int Fd(int x){while(par[x]!=x){par[x]=par[par[x]];x=par[x];}return x;}void U(int a,int c){a=Fd(a);c=Fd(c);if(a!=c)par[a]=c;}
   var tri=m.triangles;for(int k=0;k<tri.Length;k+=3){U(tri[k],tri[k+1]);U(tri[k],tri[k+2]);}var byPos=new Dictionary<Vector3Int,int>();for(int i=0;i<wv.Length;i++){var key=Vector3Int.RoundToInt(wv[i]*200);if(byPos.TryGetValue(key,out var j))U(i,j);else byPos[key]=i;}
   var used=new HashSet<int>(tri);foreach(var g in Enumerable.Range(0,wv.Length).Where(used.Contains).GroupBy(Fd)){var idx=g.ToArray();var b=new Bounds(wv[idx[0]],Vector3.zero);foreach(var i in idx)b.Encapsulate(wv[i]);var l=L(b.center);if(l.x<-30||l.x>38||l.z<-26||l.z>26)continue;pieces.Add(new Piece{mf=mf,idx=idx,b=b,bark=b.size.x<1.3f&&b.size.z<1.3f&&b.size.y>1.8f});}}
  var trees=new List<Tree>();
  foreach(var c in roots.SelectMany(g=>g.GetComponentsInChildren<Collider>(true))){if(!TreeName(c.name)||c.isTrigger||!c.bounds.Intersects(region))continue;var b=c.bounds;var l=L(new Vector3(b.center.x,b.min.y,b.center.z));if(l.x<-30||l.x>38||l.z<-26||l.z>26)continue;trees.Add(new Tree{bottom=new Vector3(b.center.x,b.min.y,b.center.z),col=c,r=Mathf.Max(b.extents.x,b.extents.z),h=b.size.y});}
  foreach(var p in pieces.Where(p=>p.bark)){var bt=new Vector3(p.b.center.x,p.b.min.y,p.b.center.z);if(trees.Any(s=>s.col&&new Vector2(s.bottom.x-bt.x,s.bottom.z-bt.z).magnitude<.6f))continue;trees.Add(new Tree{bottom=bt,r=Mathf.Max(p.b.extents.x,p.b.extents.z),h=p.b.size.y});}
  foreach(var p in pieces){var c=new Vector3(p.b.center.x,p.b.min.y,p.b.center.z);int best=-1;float bd=p.bark?.8f:6f;for(int i=0;i<trees.Count;i++){var s=trees[i];if(!(s.bottom.y+s.h*1.6f+1>=p.b.min.y&&s.bottom.y<p.b.max.y+1))continue;float d=new Vector2(s.bottom.x-c.x,s.bottom.z-c.z).magnitude;if(d<bd){bd=d;best=i;}}p.owner=best;if(best>=0)trees[best].pieces.Add(p);}
  int removed=0,seated=0;foreach(var s in trees){var l=L(s.bottom);if(InFootprint(l,1.2f+s.r)){s.remove=true;removed++;rep.AppendLine($"  remove tree {(s.col?P(s.col.transform):"(bark box)")} at local {V(l)} (in the new footprint / apron / drive), pieces {s.pieces.Count}");continue;}
   float dy=DyAt(l);if(Mathf.Abs(dy)>.03f){s.dy=dy;seated++;rep.AppendLine($"  re-seat tree {(s.col?P(s.col.transform):"(bark box)")} at local {V(l)} by {dy:+0.00;-0.00}");}}
  if(!dry){foreach(var g in pieces.GroupBy(p=>p.mf)){var cutP=g.Where(p=>p.owner>=0&&trees[p.owner].remove).ToList();var move=g.Where(p=>p.owner>=0&&!trees[p.owner].remove&&trees[p.owner].dy!=0).ToList();
     var loose=new Dictionary<Piece,float>();foreach(var p in g.Where(p=>p.owner<0)){var l=L(new Vector3(p.b.center.x,p.b.min.y,p.b.center.z));if(InFootprint(l,.8f)&&p.b.min.y<site.position.y+OFF+4){cutP.Add(p);continue;}float dy=DyAt(l);if(Mathf.Abs(dy)>.03f)loose[p]=dy;}
     if(cutP.Count+move.Count+loose.Count==0)continue;var mf=g.Key;var m=Object.Instantiate(mf.sharedMesh);var vv=m.vertices;var tr=mf.transform;var cutSet=new HashSet<int>(cutP.SelectMany(p=>p.idx));
     foreach(var p in move)foreach(var i in p.idx)vv[i]=tr.InverseTransformPoint(tr.TransformPoint(vv[i])+Vector3.up*trees[p.owner].dy);foreach(var kv in loose)foreach(var i in kv.Key.idx)vv[i]=tr.InverseTransformPoint(tr.TransformPoint(vv[i])+Vector3.up*kv.Value);
     var tri=m.triangles;var keep=new List<int>();for(int k=0;k<tri.Length;k+=3)if(!cutSet.Contains(tri[k]))keep.AddRange(new[]{tri[k],tri[k+1],tri[k+2]});m.vertices=vv;m.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;m.SetTriangles(keep,0);m.RecalculateBounds();
     string np=$"{Report087KyleModel.Dir}/{sn}-kyle-vegetation-{mf.name.Replace('/','_')}-{Math.Abs(AssetDatabase.GetAssetPath(mf.sharedMesh).GetHashCode())%100000}.asset";AssetDatabase.CreateAsset(m,np);rep.AppendLine($"  vegetation {P(tr)}: pieces cut {cutP.Count}, moved {move.Count}, bushes moved {loose.Count} -> {np}");mf.sharedMesh=m;}
   foreach(var s in trees){if(!s.col)continue;if(s.remove){var go=s.col.gameObject;if(go.GetComponents<Component>().Length<=2&&go.transform.childCount==0)Object.DestroyImmediate(go);else Object.DestroyImmediate(s.col);}else if(s.dy!=0)s.col.transform.position+=Vector3.up*s.dy;}}
  rep.AppendLine($"  trees removed {removed}, re-seated {seated}");
  // small objects on changed ground (not the house, not ground, not the drive)
  foreach(var r in roots.SelectMany(g=>g.GetComponentsInChildren<Renderer>(true))){if(Veg(r)||r.name.StartsWith("Ground")||r.transform.IsChildOf(site)||!r.bounds.Intersects(region)||r.bounds.size.x>8||r.bounds.size.z>8)continue;var b=r.bounds;var l=L(new Vector3(b.center.x,b.min.y,b.center.z));if(l.x<-26||l.x>34||l.z<-22||l.z>22)continue;float dy=DyAt(l);if(Mathf.Abs(dy)<.03f)continue;
   rep.AppendLine($"  re-seat object {P(r.transform)} at local {V(l)} by {dy:+0.00;-0.00}");if(!dry)r.transform.position+=Vector3.up*dy;}
  if(dry){rep.AppendLine("  DRY: nothing saved");return;}
  // ---- old house out, new house in
  if(arch){if(PrefabUtility.IsPartOfPrefabInstance(arch.gameObject))PrefabUtility.UnpackPrefabInstance(PrefabUtility.GetOutermostPrefabInstanceRoot(arch.gameObject),PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);Object.DestroyImmediate(arch.gameObject);}
  foreach(var n in new[]{"Foundation","Entrance step 1","Entrance step 2"}){var c=site.Find(n);if(c)Object.DestroyImmediate(c.gameObject);}
  var house=new GameObject("Kyle's house (0.87)",typeof(MeshFilter),typeof(MeshRenderer));house.transform.SetParent(site,false);house.transform.localPosition=new Vector3(0,OFF,0);house.layer=layer;house.isStatic=isStatic;
  house.GetComponent<MeshFilter>().sharedMesh=model;var mr=house.GetComponent<MeshRenderer>();mr.sharedMaterial=building;mr.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.On;
  var colRoot=new GameObject("Phase 6 architecture").transform;colRoot.SetParent(site,false);colRoot.localPosition=new Vector3(0,OFF,0);colRoot.gameObject.layer=layer;colRoot.gameObject.isStatic=isStatic;
  foreach(var c in cols){var g=new GameObject(c.name);g.transform.SetParent(colRoot,false);g.layer=layer;g.isStatic=isStatic;if(c.kind=="box"){g.transform.localPosition=c.c;g.AddComponent<BoxCollider>().size=c.s;}else{var m=g.AddComponent<MeshCollider>();m.convex=true;m.sharedMesh=c.m;}}
  Physics.SyncTransforms();
  rep.AppendLine($"  new colliders: {colRoot.GetComponentsInChildren<Collider>().Length}");foreach(var c in colRoot.GetComponentsInChildren<Collider>())rep.AppendLine($"    {c.name} {c.GetType().Name} {V(c.bounds.min)} - {V(c.bounds.max)}");
  // ---- the driveway: the old leg to the front out, the new leg and apron in
  Material gravel=null;
  if(ribbon){var mf=ribbon.GetComponent<MeshFilter>();gravel=ribbon.GetComponent<MeshRenderer>()?.sharedMaterial;var src=ribbon.sharedMesh;var tf=ribbon.transform;var sv=src.vertices;var st=src.triangles;
   var nv=new List<Vector3>();var nt=new List<int>();int clipped=0,dropped=0;
   for(int k=0;k<st.Length;k+=3){var poly=new List<Vector3>{sv[st[k]],sv[st[k+1]],sv[st[k+2]]};float D(Vector3 q)=>L(tf.TransformPoint(q)).x-Trim;
    if(poly.All(q=>D(q)>=0)){int bi=nv.Count;nv.AddRange(poly);nt.AddRange(new[]{bi,bi+1,bi+2});continue;}if(poly.All(q=>D(q)<=0)){dropped++;continue;}
    var outp=new List<Vector3>();for(int e=0;e<3;e++){var a2=poly[e];var b2=poly[(e+1)%3];float da=D(a2),db=D(b2);if(da>=0)outp.Add(a2);if(da*db<0)outp.Add(Vector3.Lerp(a2,b2,da/(da-db)));}
    int b0=nv.Count;nv.AddRange(outp);for(int e=1;e+1<outp.Count;e++)nt.AddRange(new[]{b0,b0+e,b0+e+1});clipped++;}
   var m=new Mesh{indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};m.SetVertices(nv);m.SetTriangles(nt,0);m.uv=nv.Select(q=>new Vector2(q.x,q.z)*.2f).ToArray();m.RecalculateNormals();m.RecalculateBounds();
   string np=$"{Report087KyleModel.Dir}/{sn}-kyle-driveway-0.87.asset";AssetDatabase.CreateAsset(m,np);if(mf&&mf.sharedMesh==src)mf.sharedMesh=m;ribbon.sharedMesh=m;rep.AppendLine($"  old drive ribbon: cut along local x = {Trim} (the leg to the front removed: {dropped} triangles, {clipped} clipped) -> {np}; cut ends {V(cutA)} / {V(cutB)}");}
  gravel??=AssetDatabase.FindAssets("Driveway gravel t:Material").Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<Material>).FirstOrDefault();
  {var vs=new List<Vector3>();var ts=new List<int>();float len=0;for(int i=1;i<spur.Length;i++)len+=Vector2.Distance(new(spur[i].x,spur[i].z),new(spur[i-1].x,spur[i-1].z));
   Vector3 SP(float d,out Vector3 f){float acc=0;for(int i=1;i<spur.Length;i++){float sl=Vector2.Distance(new(spur[i].x,spur[i].z),new(spur[i-1].x,spur[i-1].z));if(d<=acc+sl||i==spur.Length-1){f=(spur[i]-spur[i-1]).normalized;return Vector3.Lerp(spur[i-1],spur[i],Mathf.Clamp01((d-acc)/sl));}acc+=sl;}f=Vector3.forward;return spur[^1];}
   int rows=Mathf.CeilToInt(len/.5f);for(int i=0;i<=rows;i++){var c=SP(len*i/rows,out var f);var r=Vector3.Cross(Vector3.up,new Vector3(f.x,0,f.z).normalized);float hw0=(cutB-cutA).magnitude*.5f;float hw=Mathf.Lerp(hw0,SpurHW,Mathf.SmoothStep(0,1,len*i/rows/7f));if(i==0){var e1=cutA;var e2=cutB;if(Vector3.Dot(e1-c,r)>0)(e1,e2)=(e2,e1);vs.Add(W(e1+Vector3.up*.005f));vs.Add(W(e2+Vector3.up*.005f));}else foreach(float w in new[]{-hw,hw}){var q=c+r*w;q.y=c.y+.03f;vs.Add(W(q));}if(i>0){int a=(i-1)*2;ts.AddRange(new[]{a,a+2,a+1,a+1,a+2,a+3});}}
   int b0=vs.Count;foreach(var q in new[]{new Vector3(Apron.x0,.03f,Apron.z0),new Vector3(Apron.x1,.03f,Apron.z0),new Vector3(Apron.x1,.03f,Apron.z1),new Vector3(Apron.x0,.03f,Apron.z1)})vs.Add(W(q));ts.AddRange(new[]{b0,b0+2,b0+1,b0,b0+3,b0+2});
   var m=new Mesh{vertices=vs.ToArray(),triangles=ts.ToArray()};m.uv=vs.Select(p=>new Vector2(p.x,p.z)*.2f).ToArray();m.RecalculateNormals();m.RecalculateBounds();
   // faces up
   if(m.normals.Average(n=>n.y)<0){var t2=m.triangles;for(int k=0;k<t2.Length;k+=3)(t2[k+1],t2[k+2])=(t2[k+2],t2[k+1]);m.triangles=t2;m.RecalculateNormals();}
   string np=$"{Report087KyleModel.Dir}/{sn}-kyle-garage-drive.asset";AssetDatabase.CreateAsset(m,np);
   var parent=driveRoad?driveRoad.transform:site.parent;var g=new GameObject("Ground_Kyle garage drive (0.87)",typeof(MeshFilter),typeof(MeshRenderer));g.transform.SetParent(parent,true);g.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);g.transform.localScale=Vector3.one;
   if(g.transform.lossyScale!=Vector3.one)throw new Exception("drive parent scaled");g.GetComponent<MeshFilter>().sharedMesh=m;g.GetComponent<MeshRenderer>().sharedMaterial=gravel;g.AddComponent<MeshCollider>().sharedMesh=m;g.layer=ribbon?ribbon.gameObject.layer:layer;g.isStatic=ribbon?ribbon.gameObject.isStatic:isStatic;
   rep.AppendLine($"  new drive leg and apron: {np}, gravel '{gravel?.name}', apron local x {Apron.x0}..{Apron.x1}, z {Apron.z0}..{Apron.z1} at the pad");}
  if(driveRoad){var pts=driveRoad.points.ToList();var before=string.Join(" ",pts.Select(V));while(pts.Count>2&&L(pts[^1]).x<Trim)pts.RemoveAt(pts.Count-1);
   foreach(var q in new[]{spur[0],spur[1],spur[2],new Vector3(12.2f,0,-1.6f)})pts.Add(W(q+Vector3.up*.03f));driveRoad.points=pts.ToArray();EditorUtility.SetDirty(driveRoad);rep.AppendLine($"  drive route points {before} -> {string.Join(" ",pts.Select(V))}");
   var map=roots.SelectMany(g=>g.GetComponentsInChildren<ExplorationMap>(true)).FirstOrDefault();var d=map?map.destinations.FirstOrDefault(x=>x.id=="property-anderson"):null;
   if(d!=null){var was=d.position;d.position=pts[^1];d.yaw=Quaternion.LookRotation(Vector3.ProjectOnPlane(pts[^2]-pts[^1],Vector3.up)).eulerAngles.y;EditorUtility.SetDirty(map);rep.AppendLine($"  map destination property-anderson {V(was)} -> {V(d.position)} yaw {d.yaw:F0}");}}
  EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();rep.AppendLine("  saved");}
}
