using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 0.101 Part A (editor only, copied into Assets/Editor/Report101Temp while it runs): Abandoned Cabin Jump, Dan's Backyard Forward and Free Roam.
//  - one straight run-up square to the ramp, from where the main road crosses the ramp's axis to the lip; the ground under it graded to a
//    smooth profile that only flattens gently (no crest), never touching any road; the old approach decal replaced along the new line
//  - the leaning boards widened 3.3 -> 5.0 m (9 boards), the roof lip widened 6.4 -> 9.6 m (eaves), one takeoff collider for both
//  - lead-in posts at the run-up and the gold chevrons moved onto the new line (run-up, boards, roof); the trail sign moved off the line
//  - the brush: the original 0.43 mesh cleared within 7 m of the line from the far edge (FAR_EDGE, new stations) to the rejoin, and
//    nothing on the run-up or ramp; the same mesh in both scenes; the reset past the far edge in the race and (inactive line) in Free Roam
// WORLD_DRY=1: report only.
public static class Report101Cabin {
 static string Out=>Environment.GetEnvironmentVariable("PROBE_OUT");
 static bool Dry=>Environment.GetEnvironmentVariable("WORLD_DRY")=="1";
 const string Dir="Assets/Track/BackyardShortcuts/",NewDir="Assets/Scenery/Report101/",Original="91b6615e142686f4f88dd9a12cac700e";
 const float BoardWidth=5.0f,RoofWidth=9.6f,ClearHalf=7f;
 static List<string> log=new();
 static Vector3 lip,d,n;static float aC,aF,aRoof,yM,lipS;static Vector3[] newPoints;static float[] oldS;static Vector3[] oldP;static float oldLip=41,oldFoot=22,oldRoof=32;
 static float F(string s)=>float.Parse(s,System.Globalization.CultureInfo.InvariantCulture);
 static float A(Vector3 p)=>Vector3.Dot(Flat(p-lip),d);static float L(Vector3 p)=>Vector3.Dot(Flat(p-lip),n);static Vector3 Flat(Vector3 v){v.y=0;return v;}
 static float Hermite(float x,float a,float b,float ya,float yb,float ma,float mb){float t=Mathf.InverseLerp(a,b,x),t2=t*t,t3=t2*t;return (2*t3-3*t2+1)*ya+(t3-2*t2+t)*(b-a)*ma+(-2*t3+3*t2)*yb+(t3-t2)*(b-a)*mb;}
 static float OldY(float s){for(int i=1;i<oldS.Length;i++)if(oldS[i]>=s)return Mathf.Lerp(oldP[i-1].y,oldP[i].y,Mathf.InverseLerp(oldS[i-1],oldS[i],s));return oldP[^1].y;}
 // the height of the new line at along a (<= 0: run-up then the ramp as it was)
 static float Y(float a){
  if(a<=aF){float L=aF-aC,u=Mathf.Clamp(a-aC,0,L),yF=OldY(oldFoot),s0=2*(yF-yM)/L-.15f;return yM+s0*u-(s0-.15f)*u*u/(2*L);}
  return OldY(oldFoot+(a-aF)/(0-aF)*(oldLip-oldFoot));}
 static Vector3 OnLine(float a,float l=0){var p=lip+d*a+n*l;p.y=Y(a);return p;}
 static float Smooth(float e0,float e1,float x)=>Mathf.SmoothStep(0,1,Mathf.InverseLerp(e0,e1,x));
 static float TopGround(Vector3 p){float best=float.NaN;foreach(var h in Physics.RaycastAll(new Vector3(p.x,p.y+40,p.z),Vector3.down,120,~0,QueryTriggerInteraction.Ignore))if(h.collider.name.StartsWith("Ground_")&&(float.IsNaN(best)||h.point.y>best))best=h.point.y;return best;}
 static string V(Vector3 v)=>$"({v.x:F2},{v.y:F2},{v.z:F2})";

 public static void Run(){
  try{
   float far=F(Environment.GetEnvironmentVariable("FAR_EDGE")??"87.8");
   Directory.CreateDirectory(NewDir);
   // ---------- Dan's Backyard Forward: the line ----------
   var sc=EditorSceneManager.OpenScene("Assets/Scenes/DansBackyardForward.unity");Physics.SyncTransforms();
   var route=UnityEngine.Object.FindObjectsByType<Racer.WoodlandRoute>(FindObjectsSortMode.None).First(w=>w.title=="Abandoned Cabin Jump");route.Initialize();
   oldP=route.points.ToArray();oldS=new float[oldP.Length];for(int i=1;i<oldP.Length;i++)oldS[i]=oldS[i-1]+Vector2.Distance(new(oldP[i].x,oldP[i].z),new(oldP[i-1].x,oldP[i-1].z));
   var foot=route.At(oldFoot,out _);lip=route.At(oldLip,out _);d=Flat(lip-foot).normalized;n=new Vector3(-d.z,0,d.x);aF=A(foot);aRoof=A(route.At(oldRoof,out _));
   var race=UnityEngine.Object.FindAnyObjectByType<Racer.RaceDirector>();var main=race.road;main.Initialize();
   float sx=float.NaN;for(float s=route.entryRoad-40;s<route.entryRoad+10;s+=.05f){var a=main.At(s,out _);var b=main.At(s+.05f,out _);if(Mathf.Sign(L(a))!=Mathf.Sign(L(b))){sx=s;break;}}
   var cross=main.At(sx,out var mf);aC=A(cross);yM=cross.y;lipS=-aC;
   log.Add($"axis heading {Mathf.Atan2(d.x,d.z)*Mathf.Rad2Deg:F2}; lip {V(lip)}; foot along {aF:F2}; roof edge along {aRoof:F2}; main crosses the axis at main s {sx:F2} along {aC:F2} {V(cross)} heading {Mathf.Atan2(mf.x,mf.z)*Mathf.Rad2Deg:F1} (crossing angle {Vector3.Angle(Flat(mf),d):F1} deg) hw {main.HalfWidth(sx):F1}");
   log.Add($"old route: entryRoad {route.entryRoad:F1}, length {route.Length:F1}, heading at s0 {Mathf.Atan2(oldP[1].x-oldP[0].x,oldP[1].z-oldP[0].z)*Mathf.Rad2Deg:F1}, at s20 {H(route,20):F1}, at the lip {H(route,40):F1}; main heading at the old fork {H(main,route.entryRoad):F1}; old fork to ramp foot {oldFoot} m, to the lip {oldLip} m; boards 3.3 m wide, roof 6.4 m");
   var pts=new List<Vector3>();for(float a=aC;a<0;a+=.5f)pts.Add(OnLine(a));pts.Add(OnLine(0));for(int i=0;i<oldP.Length;i++)if(oldS[i]>oldLip+.3f)pts.Add(oldP[i]);newPoints=pts.ToArray();
   for(float a=aC;a<=aF;a+=2){float t=TopGround(OnLine(a));log.Add($"  profile along {a,6:F1} (new s {a-aC,5:F1}): line {Y(a):F2}, ground now {t:F2} (cut {t-Y(a):+0.00;-0.00})");}
   float offset=lipS-oldLip;log.Add($"new route: straight {lipS:F1} m from the main's centre line to the lip ({lipS-main.HalfWidth(sx)/Mathf.Sin(Vector3.Angle(Flat(mf),d)*Mathf.Deg2Rad):F1} m from its right edge on the centre line), ramp foot at new s {aF-aC:F1}; stations past the lip = old + {offset:F2}; far edge {far:F1} (old-station equivalent {far-offset:F1})");
   AuthorScene(sc,route,far,true);
   // ---------- Free Roam ----------
   var fr=EditorSceneManager.OpenScene("Assets/Scenes/FreeRoamWorld.unity");Physics.SyncTransforms();AuthorScene(fr,null,far,false);
   if(!Dry)AssetDatabase.SaveAssets();
   Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/cabin.txt",log);EditorApplication.Exit(0);
  }catch(Exception e){log.Add("FAILED "+e);Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/cabin.txt",log);EditorApplication.Exit(1);}}
 static float H(Racer.WoodlandRoute r,float s){r.At(s,out var f);return Mathf.Atan2(f.x,f.z)*Mathf.Rad2Deg;}
 static float H(Racer.RaceRoad r,float s){r.At(s,out var f);return Mathf.Atan2(f.x,f.z)*Mathf.Rad2Deg;}

 static void AuthorScene(UnityEngine.SceneManagement.Scene scene,Racer.WoodlandRoute route,float far,bool forward){
  string sn=scene.name;log.Add("===== "+sn);
  var root=GameObject.Find("Backyard optional forest shortcuts").transform;log.Add($"shortcut root at {V(root.position)} rot {V(root.eulerAngles)} scale {V(root.lossyScale)}");
  var roads=UnityEngine.Object.FindObjectsByType<Racer.RaceRoad>(FindObjectsSortMode.None).Where(r=>r.points!=null&&r.points.Length>3).ToList();foreach(var r in roads)r.Initialize();
  // the race's main road (in both scenes) is untouched out to 4 m past its edge; any other road (Free Roam's copy of the Reverse road runs
  // 4-5 m beside the run-up) keeps its centre 3 m strip and lets its outer edge blend into the run-up
  float RoadWeight(Vector3 p,bool mainOnly=false){float w=1;foreach(var r in roads){bool isMain=r.name=="Forward navigation only - no road mesh";if(mainOnly&&!isMain)continue;var q=p;float s=r.Project(q,out _);q.y=r.At(s,out _).y;s=r.Project(q,out _);var c=r.At(s,out _);if(Mathf.Abs(c.y-p.y)>6)continue;float lat=new Vector2(p.x-c.x,p.z-c.z).magnitude,hw=r.HalfWidth(s);w*=isMain?Smooth(hw+.3f,hw+4f,lat):Smooth(1.5f,3f,lat);}return w;}
  var trunks=UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsSortMode.None).Where(c=>c.name.ToLower().Contains("trunk")&&Vector3.Distance(c.bounds.center,OnLine((aC+aRoof)/2))<45).ToList();
  float TrunkWeight(Vector3 p){float w=1;foreach(var t in trunks){var b=t.bounds;float dx=Mathf.Max(0,Mathf.Abs(p.x-b.center.x)-b.extents.x),dz=Mathf.Max(0,Mathf.Abs(p.z-b.center.z)-b.extents.z);w=Mathf.Min(w,Smooth(.6f,2f,Mathf.Sqrt(dx*dx+dz*dz)));}return w;}
  // ---- ground ----
  var ground=GameObject.Find("Memory loop - north is +Z/Ground_480_400");var gf=ground.GetComponent<MeshFilter>();var gc=ground.GetComponent<MeshCollider>();
  log.Add($"ground mesh {gf.sharedMesh.name} {AssetDatabase.GetAssetPath(gf.sharedMesh)} verts {gf.sharedMesh.vertexCount}; collider mesh same {gc.sharedMesh==gf.sharedMesh}; transform {V(ground.transform.position)} {V(ground.transform.eulerAngles)}");
  var mesh=UnityEngine.Object.Instantiate(gf.sharedMesh);var v=mesh.vertices;var M=ground.transform.localToWorldMatrix;var Mi=ground.transform.worldToLocalMatrix;
  var changed=new HashSet<int>();float maxCut=0,maxFill=0;
  var fwdMain=roads.First(r=>r.name=="Forward navigation only - no road mesh");
  bool RightOfMain(Vector3 p){float s=fwdMain.Project(p,out _);var c=fwdMain.At(s,out var f);return Vector3.Dot(Flat(p-c),Vector3.Cross(Vector3.up,Flat(f))) >0;}
  for(int i=0;i<v.Length;i++){var w=M.MultiplyPoint3x4(v[i]);float a=A(w),l=Mathf.Abs(L(w));if(a<aC-4||a>aRoof+.5f||l>8||!RightOfMain(w))continue;
   float target,wl;
   if(a<=aF+.5f){target=Y(Mathf.Min(a,aF));wl=1-Smooth(3.5f,7.5f,l);}
   else{target=Mathf.Min(w.y,Y(a)-.08f);wl=1-Smooth(3.2f,6f,l);}
   float wr=RoadWeight(w)*TrunkWeight(w);float ny=w.y+(target-w.y)*wl*wr;if(Mathf.Abs(ny-w.y)<.002f)continue;
   maxCut=Mathf.Max(maxCut,w.y-ny);maxFill=Mathf.Max(maxFill,ny-w.y);w.y=ny;v[i]=Mi.MultiplyPoint3x4(w);changed.Add(i);}
  log.Add($"ground: {changed.Count} vertices moved, deepest cut {maxCut:F2} m, highest fill {maxFill:F2} m");
  mesh.vertices=v;
  {var tri=mesh.triangles;var touch=new HashSet<int>();for(int t=0;t<tri.Length;t+=3)if(changed.Contains(tri[t])||changed.Contains(tri[t+1])||changed.Contains(tri[t+2])){touch.Add(tri[t]);touch.Add(tri[t+1]);touch.Add(tri[t+2]);}
   var nr=mesh.normals;var acc=new Dictionary<int,Vector3>();foreach(var k in touch)acc[k]=Vector3.zero;
   for(int t=0;t<tri.Length;t+=3){int a=tri[t],b=tri[t+1],c=tri[t+2];if(!touch.Contains(a)&&!touch.Contains(b)&&!touch.Contains(c))continue;var fn=Vector3.Cross(v[b]-v[a],v[c]-v[a]);foreach(var k in new[]{a,b,c})if(acc.ContainsKey(k))acc[k]+=fn;}
   if(nr!=null&&nr.Length==v.Length){foreach(var kv in acc)if(kv.Value.sqrMagnitude>1e-12f)nr[kv.Key]=kv.Value.normalized;mesh.normals=nr;}
   mesh.RecalculateBounds();log.Add($"  normals recomputed on {touch.Count} vertices of the touched triangles (flat-shaded mesh: {tri.Length==v.Length})");}
  // underlays drawn under the ground (no collider): kept below the new surface
  var newMeshPath=NewDir+sn+"-Ground_480_400-cabin-runup.asset";
  if(!Dry){if(AssetDatabase.LoadAssetAtPath<Mesh>(newMeshPath))AssetDatabase.DeleteAsset(newMeshPath);mesh.name=Path.GetFileNameWithoutExtension(newMeshPath);AssetDatabase.CreateAsset(mesh,newMeshPath);gf.sharedMesh=mesh;gc.sharedMesh=null;gc.sharedMesh=mesh;EditorUtility.SetDirty(gf);EditorUtility.SetDirty(gc);Physics.SyncTransforms();}
  foreach(var r in UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r=>r.name.StartsWith("Ground_")&&!r.GetComponent<Collider>()&&r.bounds.Intersects(new Bounds(OnLine((aC+aF)/2),new Vector3(60,30,60))))){
   var um=r.GetComponent<MeshFilter>().sharedMesh;var uv=um.vertices;var UM=r.transform.localToWorldMatrix;var UMi=r.transform.worldToLocalMatrix;int moved=0;
   for(int i=0;i<uv.Length;i++){var w=UM.MultiplyPoint3x4(uv[i]);float a=A(w),l=Mathf.Abs(L(w));if(a<aC-4||a>aRoof+.5f||l>8)continue;float g=TopGround(w+Vector3.up*2);if(!float.IsNaN(g)&&w.y>g-.05f){w.y=g-.12f;uv[i]=UMi.MultiplyPoint3x4(w);moved++;}}
   log.Add($"  underlay {r.name} ({um.name}): {moved} vertices pushed below the new ground");
   if(moved>0&&!Dry){var copy=UnityEngine.Object.Instantiate(um);copy.vertices=uv;copy.RecalculateBounds();var p=NewDir+sn+"-"+r.name.Replace(' ','-')+"-cabin-runup.asset";if(AssetDatabase.LoadAssetAtPath<Mesh>(p))AssetDatabase.DeleteAsset(p);copy.name=Path.GetFileNameWithoutExtension(p);AssetDatabase.CreateAsset(copy,p);r.GetComponent<MeshFilter>().sharedMesh=copy;EditorUtility.SetDirty(r.GetComponent<MeshFilter>());}}
  // what stands on the graded ground
  var zone=new Bounds(OnLine((aC+aRoof)/2),new Vector3(70,40,70));
  foreach(var c in UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsSortMode.None).Where(c=>!c.name.StartsWith("Ground_")&&c.bounds.Intersects(zone))){var b=c.bounds;var p=b.center;float a=A(p),l=Mathf.Abs(L(p));if(a<aC-4||a>aRoof||l>8+b.extents.magnitude*.5f)continue;float g=TopGround(new Vector3(p.x,b.max.y,p.z));log.Add($"  stands in the graded area: {c.name} at {V(p)} along {a:F1} lateral {L(p):F1} bottom {b.min.y:F2} ground {g:F2}");}
  // ---- the route (Forward) / the Free Roam reset line ----
  Racer.WoodlandRoute line=route;
  if(forward){
   if(!Dry){route.points=newPoints;var ms=UnityEngine.Object.FindAnyObjectByType<Racer.RaceDirector>().road;route.entryRoad=ms.Project(newPoints[0],out _);route.entrySpeedDistance=lipS+5;EditorUtility.SetDirty(route);}
   log.Add($"route: {newPoints.Length} points, entryRoad {route.entryRoad:F1}, exitRoad {route.exitRoad:F1}, entrySpeed {route.entrySpeed} over {route.entrySpeedDistance:F1} m, inset {route.entryInset}");}
  else{
   var old=root.Find("Abandoned Cabin Jump (Free Roam brush reset line)");if(old&&!Dry)UnityEngine.Object.DestroyImmediate(old.gameObject);
   var g=new GameObject("Abandoned Cabin Jump (Free Roam brush reset line)");g.transform.SetParent(root,false);line=g.AddComponent<Racer.WoodlandRoute>();line.title="Abandoned Cabin Jump";line.points=newPoints;line.halfWidth=3.2f;g.SetActive(false);
   log.Add("Free Roam: inactive reset line (not a branch; the brush's reset only)");}
  line.Initialize();
  // ---- takeoff collider: boards then roof ----
  {var vs=new List<Vector3>();var ts=new List<int>();void Row(float a,float w){var p=OnLine(a);vs.Add(p-n*w/2);vs.Add(p+n*w/2);if(vs.Count>2){int k=vs.Count-4;ts.AddRange(new[]{k,k+1,k+2,k+1,k+3,k+2});}}
   for(float a=aF;a<aRoof;a+=.4f)Row(a,BoardWidth);Row(aRoof,BoardWidth);for(float a=aRoof;a<0;a+=.4f)Row(a,RoofWidth);Row(0,RoofWidth);
   var m=new Mesh{name="Takeoff - Leaning boards through cabin roof (0.101)"};m.SetVertices(vs);m.SetTriangles(ts,0);m.RecalculateNormals();m.RecalculateBounds();
   // winding: top faces up
   if(m.normals[0].y<0){ts.Clear();for(int k=0;k+3<vs.Count;k+=2)ts.AddRange(new[]{k,k+2,k+1,k+1,k+2,k+3});m.SetTriangles(ts,0);m.RecalculateNormals();}
   m=Save(m,Dir+"Takeoff - Leaning boards through cabin roof (0.101).asset");
   var c=root.Find("Takeoff - Leaning boards through cabin roof").GetComponent<MeshCollider>();if(!Dry){c.sharedMesh=null;c.sharedMesh=m;EditorUtility.SetDirty(c);}
   log.Add($"takeoff collider: boards {BoardWidth} m from along {aF:F1} to {aRoof:F1}, roof {RoofWidth} m to the lip; normal y {m.normals[0].y:F2}");}
  // ---- visuals: boards, roof, roof planks ----
  Material dark=null,wood=null;
  foreach(Transform t in root.Cast<Transform>().ToArray()){if(t.name.StartsWith("Long loose leaning board")){var mr=t.GetComponent<MeshRenderer>();if(t.name.EndsWith("0"))dark=mr.sharedMaterial;if(t.name.EndsWith("1"))wood=mr.sharedMaterial;if(!Dry)UnityEngine.Object.DestroyImmediate(t.gameObject);}}
  if(!dark||!wood)throw new Exception("board materials not found in "+sn);
  if(!Dry)for(int i=0;i<9;i++){float off=(i-4)*.55f;var vs=new List<Vector3>();var ts=new List<int>();int j=0;for(float a=aF;;a=Mathf.Min(aRoof,a+.25f)){var p=OnLine(a);vs.Add(p+n*(off-.26f));vs.Add(p+n*(off+.26f));if(j++>0){int k=vs.Count-4;ts.AddRange(new[]{k,k+2,k+1,k+1,k+2,k+3});}if(a>=aRoof)break;}
   var m=new Mesh{name="Leaning weathered board (0.101) "+i};m.SetVertices(vs);m.SetTriangles(ts,0);m.RecalculateNormals();if(m.normals[0].y<0){ts.Clear();for(int k=0;k+3<vs.Count;k+=2)ts.AddRange(new[]{k,k+1,k+2,k+1,k+3,k+2});m.SetTriangles(ts,0);m.RecalculateNormals();}m.RecalculateBounds();
   m=Save(m,Dir+"Leaning weathered board (0.101) "+i+".asset");var g=new GameObject("Long loose leaning board "+i);g.transform.SetParent(root,false);g.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);g.AddComponent<MeshFilter>().sharedMesh=m;g.AddComponent<MeshRenderer>().sharedMaterial=i%2==0?dark:wood;}
  log.Add($"boards: 9 leaning boards across {BoardWidth} m (were 6 across 3.3 m)");
  {var roof=root.Find("Abandoned cabin modest shed roof");var mr=roof.GetComponent<MeshRenderer>();var vs=new List<Vector3>();var ts=new List<int>();int j=0;for(float a=aRoof;;a=Mathf.Min(0,a+.4f)){var p=OnLine(a);vs.Add(p-n*RoofWidth/2);vs.Add(p+n*RoofWidth/2);if(j++>0){int k=vs.Count-4;ts.AddRange(new[]{k,k+1,k+2,k+1,k+3,k+2});}if(a>=0)break;}
   var m=new Mesh{name="Abandoned cabin modest shed roof (0.101)"};m.SetVertices(vs);m.SetTriangles(ts,0);m.RecalculateNormals();if(m.normals[0].y<0){ts.Clear();for(int k=0;k+3<vs.Count;k+=2)ts.AddRange(new[]{k,k+2,k+1,k+1,k+2,k+3});m.SetTriangles(ts,0);m.RecalculateNormals();}m.RecalculateBounds();
   m=Save(m,Dir+"Abandoned cabin modest shed roof (0.101).asset");if(!Dry){roof.GetComponent<MeshFilter>().sharedMesh=m;roof.position=Vector3.zero;roof.rotation=Quaternion.identity;EditorUtility.SetDirty(roof.gameObject);}}
  {var planks=root.Cast<Transform>().Where(t=>t.name=="Cabin weathered roof board").ToList();Material pd=null,pw=null;foreach(var p in planks){var m=p.GetComponent<Renderer>().sharedMaterial;if(m==dark)pd=m;else pw=m;}pd??=dark;pw??=wood;
   if(!Dry){foreach(var p in planks)UnityEngine.Object.DestroyImmediate(p.gameObject);int i=0;for(float a=aRoof+.27f;a<0;a+=.57f){var p=OnLine(a);var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name="Cabin weathered roof board";UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());g.transform.SetParent(root,true);g.transform.SetPositionAndRotation(p-Vector3.up*.055f,Quaternion.LookRotation(d));g.transform.localScale=new(RoofWidth+(i%3)*.07f,.11f,.53f);g.GetComponent<Renderer>().sharedMaterial=i++%4==0?pd:pw;}}
   log.Add($"roof: {RoofWidth} m wide (eaves {(RoofWidth-6.0f)/2:F1} m past the walls each side), {planks.Count} planks replaced");}
  // ---- markers: chevrons and lead-in posts ----
  {var chev=root.Cast<Transform>().Where(t=>t.name=="Gold optional route chevron"&&Vector3.Distance(t.position,lip)<60).OrderBy(t=>A(t.position)).ToList();bool active=chev.Count>0&&chev[0].gameObject.activeSelf;
   log.Add($"chevrons near the cabin: {chev.Count} (active {active}) at along {string.Join(", ",chev.Select(c=>A(c.position).ToString("F0")))}");
   var at=new[]{-32f,aF+4f,-4f};
   if(!Dry)for(int k=0;k<chev.Count&&k<6;k++){float a=at[k/2];float sign=k%2==0?-1:1;var p=OnLine(a)+Vector3.up*.12f;if(a<aF){float g=TopGround(p+Vector3.up);if(!float.IsNaN(g))p.y=g+.12f;}var s0=p-d*.9f+n*.7f*sign;var s1=p+d*.4f;var t=chev[k];t.SetPositionAndRotation((s0+s1)/2,Quaternion.LookRotation(s1-s0));t.localScale=new(.11f,.11f,Vector3.Distance(s0,s1));}
   var gold=chev.Count>0?chev[0].GetComponent<Renderer>().sharedMaterial:AssetDatabase.LoadAssetAtPath<Material>(Dir+"Shortcut gold.mat");
   foreach(var t in root.Cast<Transform>().Where(t=>t.name.StartsWith("Cabin run-up lead-in post")).ToArray())if(!Dry)UnityEngine.Object.DestroyImmediate(t.gameObject);
   if(!Dry)foreach(float sign in new[]{-1f,1f}){var p=OnLine(-27,3.6f*sign);float g=TopGround(p+Vector3.up*3);var post=GameObject.CreatePrimitive(PrimitiveType.Cube);post.name="Cabin run-up lead-in post";UnityEngine.Object.DestroyImmediate(post.GetComponent<Collider>());post.transform.SetParent(root,true);post.transform.SetPositionAndRotation(new Vector3(p.x,g+.55f,p.z),Quaternion.LookRotation(d));post.transform.localScale=new(.18f,1.7f,.18f);post.GetComponent<Renderer>().sharedMaterial=dark;
    var cap=GameObject.CreatePrimitive(PrimitiveType.Cube);cap.name="Cabin run-up lead-in post gold band";UnityEngine.Object.DestroyImmediate(cap.GetComponent<Collider>());cap.transform.SetParent(post.transform,true);cap.transform.SetPositionAndRotation(new Vector3(p.x,g+1.2f,p.z),Quaternion.LookRotation(d));cap.transform.localScale=new(.22f,.3f,.22f);cap.GetComponent<Renderer>().sharedMaterial=gold;}
   log.Add("lead-in posts: a pair at along -27 (new s "+(-27-aC).ToString("F1")+"), 3.6 m each side, dark timber with a gold band, no collider");}
  // ---- the trail sign: off the line, to its right ----
  {var names=new[]{"Abandoned Cabin Jump grounded signpost","Abandoned Cabin Jump timber sign","Abandoned Cabin Jump optional sign"};var parts=names.Select(x=>root.Find(x)).Where(t=>t).ToList();var post=root.Find(names[0]);
   if(post){var oldBase=post.GetComponent<Renderer>().bounds.min;float og=TopGround(oldBase+Vector3.up*3);var target=OnLine(-31,-8.5f);float ng=TopGround(target+Vector3.up*5);var delta=new Vector3(target.x-post.position.x,ng-og,target.z-post.position.z);
    log.Add($"sign: was at {V(post.position)} along {A(post.position):F1} lateral {L(post.position):F1} (its 5.5 m board reached {L(post.position)+2.75f:F1} m, on the new line); moved by {V(delta)} to along -31, 8.5 m right of the line (board from {-8.5f-2.75f:F1} to {-8.5f+2.75f:F1} m), base on the ground {ng:F2}");
    if(!Dry)foreach(var t in parts){t.position+=delta;EditorUtility.SetDirty(t.gameObject);}}}
  // ---- the approach decal ----
  {var old=root.Find("Cabin wooded approach");Material mat=old?old.GetComponent<MeshRenderer>().sharedMaterial:AssetDatabase.LoadAssetAtPath<Material>(Dir+"Wooded earth.mat");
   if(!mat){var any=AssetDatabase.FindAssets("Wooded earth t:Material");if(any.Length>0)mat=AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(any[0]));}
   var vs=new List<Vector3>();var ts=new List<int>();int cols=13;int rows=0;for(float a=aC;a<=aF+.01f;a+=.5f){for(int c=0;c<cols;c++){float l=-2.8f+c*(5.6f/(cols-1));var p=OnLine(a,l);float g=TopGround(p+Vector3.up*2);p.y=(float.IsNaN(g)?p.y:g)+.03f;vs.Add(p);}rows++;}
   for(int r=1;r<rows;r++)for(int c=1;c<cols;c++){int a=(r-1)*cols+c-1,b=a+1,e=r*cols+c-1,f=e+1;var mid=(vs[a]+vs[f])/2;if(RoadWeight(mid,true)<.999f)continue;ts.AddRange(new[]{a,e,b,b,e,f});}
   var m=new Mesh{name="Cabin wooded approach (0.101) "+sn};m.SetVertices(vs);m.SetTriangles(ts,0);m.RecalculateNormals();if(m.normals.Length>0&&m.normals[ts[0]].y<0){for(int k=0;k<ts.Count;k+=3)(ts[k+1],ts[k+2])=(ts[k+2],ts[k+1]);m.SetTriangles(ts,0);m.RecalculateNormals();}m.RecalculateBounds();
   m=Save(m,NewDir+sn+"-Cabin wooded approach.asset");
   if(!Dry){if(old)UnityEngine.Object.DestroyImmediate(old.gameObject);var g=new GameObject("Cabin wooded approach");g.transform.SetParent(root,false);g.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);g.AddComponent<MeshFilter>().sharedMesh=m;g.AddComponent<MeshRenderer>().sharedMaterial=mat;}
   log.Add($"approach decal: {ts.Count/3} triangles along the run-up, 5.6 m wide, off every road ({(old?"replaces the old one":"new")}, material {(mat?mat.name:"none")})");}
  // ---- the brush ----
  {var g=root.Find("Abandoned Cabin Jump traversable dense undergrowth").gameObject;var under=g.GetComponent<Racer.ShortcutUndergrowth>();var filter=g.GetComponent<MeshFilter>();
   var source=AssetDatabase.LoadAssetAtPath<Mesh>(AssetDatabase.GUIDToAssetPath(Original));if(!source)throw new Exception("original 0.43 brush mesh not found");
   if(!Dry){under.clearRoute=line;under.clearFrom=far;under.clearHalfWidth=ClearHalf;under.brushFrom=lipS;EditorUtility.SetDirty(under);}
   var path=Dir+"Abandoned Cabin Jump dense brush (0.101).asset";
   if(forward||!AssetDatabase.LoadAssetAtPath<Mesh>(path)||Dry){
    var bv=source.vertices;var bt=source.triangles;var world=g.transform.localToWorldMatrix;var parent=Enumerable.Range(0,bv.Length).ToArray();int Find(int i){while(parent[i]!=i){parent[i]=parent[parent[i]];i=parent[i];}return i;}
    for(int i=0;i<bt.Length;i+=3){int a=Find(bt[i]),b=Find(bt[i+1]),c=Find(bt[i+2]);parent[b]=a;parent[Find(c)]=a;}
    var pieces=Enumerable.Range(0,bv.Length).GroupBy(Find).ToList();var drop=new HashSet<int>();int cleared=0,onRamp=0,inBrush=0;float last=0;
    foreach(var piece in pieces){var ps=piece.Select(i=>world.MultiplyPoint3x4(bv[i])).ToList();var c=ps.Aggregate(Vector3.zero,(x,y)=>x+y)/ps.Count;float reach=ps.Max(p=>new Vector2(p.x-c.x,p.z-c.z).magnitude);
     float s=line.Project(c,out _);var q=line.At(s,out _);float off=new Vector2(c.x-q.x,c.z-q.z).magnitude;bool above=c.y>q.y-.5f;
     if(s>=far&&off-reach<ClearHalf&&above){cleared++;foreach(var i in piece)drop.Add(i);}
     else if(s<lipS&&off-reach<3.5f&&ps.Max(p=>p.y)>q.y-.3f){onRamp++;foreach(var i in piece)drop.Add(i);}
     else if(s>=lipS&&off-reach<ClearHalf&&above){inBrush++;last=Mathf.Max(last,s+reach);}}
    var tri=new List<int>();for(int i=0;i<bt.Length;i+=3)if(!drop.Contains(bt[i]))tri.AddRange(new[]{bt[i],bt[i+1],bt[i+2]});
    var m=UnityEngine.Object.Instantiate(source);m.name="Abandoned Cabin Jump dense brush (0.101)";m.SetTriangles(tri,0);m.RecalculateBounds();m=Save(m,path);
    log.Add($"brush (original 0.43 mesh, {pieces.Count} bushes): {cleared} removed within {ClearHalf} m of the line from the far edge (new s {far:F1}) to the rejoin, {onRamp} removed from the run-up and ramp, {inBrush} kept in the corridor from the lip (new s {lipS:F1}) to the far edge; the last one reaches s {last:F1}");
    if(!Dry){filter.sharedMesh=m;EditorUtility.SetDirty(filter);}}
   else if(!Dry){filter.sharedMesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);EditorUtility.SetDirty(filter);log.Add("brush: the same 0.101 mesh as the race");}
   log.Add($"undergrowth: clearRoute {(under.clearRoute?under.clearRoute.name:"-")} brushFrom {under.brushFrom:F1} clearFrom {under.clearFrom:F1} hw {under.clearHalfWidth}");}
  if(!Dry){EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);}
 }
 static Mesh Save(Mesh m,string path){if(Dry)return m;var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(old){EditorUtility.CopySerialized(m,old);return old;}AssetDatabase.CreateAsset(m,path);return m;}
}
