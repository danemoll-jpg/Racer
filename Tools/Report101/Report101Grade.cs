using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 0.101 Part A, the run-up's ground, second version (replaces Report101Cabin's grading; everything else it made stays): re-graded from the
// ORIGINAL ground meshes. Each lane across the run-up (every 0.25 m of lateral offset) leaves the main road's verge with the ground's own
// height and slope there and flattens evenly (one constant curvature) to the ramp foot: the gentlest crest the place allows, starting
// 0.8 m past the main road's edge (the road itself untouched). Under the boards: cut only, below the boards. Then the decal is re-draped,
// and the lead-in posts and the run-up chevrons re-seated.
public static class Report101Grade {
 static string Out=>Environment.GetEnvironmentVariable("PROBE_OUT");
 static readonly Vector3 lip=new(224.61f,74.15f,80.95f);const float heading=82.82f,aC=-44.72f,aF=-19.00f,aRoof=-9.00f,yFoot=71.10f;
 static Vector3 d=>new(Mathf.Sin(heading*Mathf.Deg2Rad),0,Mathf.Cos(heading*Mathf.Deg2Rad));static Vector3 n=>new(-d.z,0,d.x);
 static Vector3 Flat(Vector3 v){v.y=0;return v;}static float A(Vector3 p)=>Vector3.Dot(Flat(p-lip),d);static float L(Vector3 p)=>Vector3.Dot(Flat(p-lip),n);
 static float Smooth(float e0,float e1,float x)=>Mathf.SmoothStep(0,1,Mathf.InverseLerp(e0,e1,x));
 public static void Run(){var log=new List<string>();
  try{
  foreach(var (sn,origPath) in new[]{("DansBackyardForward","Assets/Track/ReverseCorrection/Local bore mouths Ground_480_400-804.asset"),("FreeRoamWorld","Assets/Scenery/Report096/FreeRoamWorld-Ground_480_400-conformed.asset")}){
   var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+sn+".unity");log.Add("===== "+sn);
   var ground=GameObject.Find("Memory loop - north is +Z/Ground_480_400");var gf=ground.GetComponent<MeshFilter>();var gc=ground.GetComponent<MeshCollider>();var cur=gf.sharedMesh;
   if(!AssetDatabase.GetAssetPath(cur).Contains("Report101"))throw new Exception("expected the 0.101 ground in "+sn);
   var orig=AssetDatabase.LoadAssetAtPath<Mesh>(origPath);if(!orig||orig.vertexCount!=cur.vertexCount)throw new Exception("original ground not found / different");
   // a probe collider of the original ground, to read its height
   var probe=new GameObject("orig probe");probe.transform.SetPositionAndRotation(ground.transform.position,ground.transform.rotation);var pc=probe.AddComponent<MeshCollider>();pc.sharedMesh=orig;probe.name="orig probe (not ground)";Physics.SyncTransforms();
   float Orig(Vector3 p){return pc.Raycast(new Ray(new Vector3(p.x,200,p.z),Vector3.down),out var h,400)?h.point.y:float.NaN;}
   var roads=UnityEngine.Object.FindObjectsByType<Racer.RaceRoad>(FindObjectsSortMode.None).Where(r=>r.points!=null&&r.points.Length>3).ToList();foreach(var r in roads)r.Initialize();var main=roads.First(r=>r.name=="Forward navigation only - no road mesh");
   float Lat(Racer.RaceRoad r,Vector3 p,out float hw){var q=p;float s=r.Project(q,out _);q.y=r.At(s,out _).y;s=r.Project(q,out _);var c=r.At(s,out _);hw=r.HalfWidth(s);return Mathf.Abs(c.y-p.y)>6?999:new Vector2(p.x-c.x,p.z-c.z).magnitude;}
   bool RightOfMain(Vector3 p){float s=main.Project(p,out _);if(Mathf.Abs(s-857.4f)>45)return Vector3.Dot(Flat(p-lip),d)>aC;var c=main.At(s,out var f);return Vector3.Dot(Flat(p-c),Vector3.Cross(Vector3.up,Flat(f)))>0;}
   float OtherRoads(Vector3 p){float w=1;foreach(var r in roads){if(r==main)continue;float lat=Lat(r,p,out _);w*=Smooth(.8f,1.6f,lat);}return w;}
   var trunks=UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsSortMode.None).Where(c=>c.name.ToLower().Contains("trunk")&&Vector3.Distance(c.bounds.center,lip+d*-25)<45).ToList();
   float TrunkWeight(Vector3 p){float w=1;foreach(var t in trunks){var b=t.bounds;float dx=Mathf.Max(0,Mathf.Abs(p.x-b.center.x)-b.extents.x),dz=Mathf.Max(0,Mathf.Abs(p.z-b.center.z)-b.extents.z);w=Mathf.Min(w,Smooth(.6f,2f,Mathf.Sqrt(dx*dx+dz*dz)));}return w;}
   // per lane: where the main road's verge is (0.8 m past its edge), the original ground's height and slope there, and the curve to the foot
   var lanes=new Dictionary<int,(float ae,float ye,float me,float c)>();
   (float ae,float ye,float me,float c) Lane(float l){int k=Mathf.RoundToInt(l*4);if(lanes.TryGetValue(k,out var v))return v;float ll=k/4f;float ae=aC-3-Mathf.Max(0,-ll)*1.6f;
    while(ae<aF-4){var p=lip+d*ae+n*ll;p.y=Orig(p);if(float.IsNaN(p.y)){ae+=.05f;continue;}float lat=Lat(main,p,out float hw);if(RightOfMain(p)&&lat>=hw+.8f)break;ae+=.05f;}
    var pe=lip+d*ae+n*ll;float ye=Orig(pe),y1=Orig(pe+d*1.5f),me=(y1-ye)/1.5f;float Lr=aF-ae;float yF=Mathf.Abs(ll)<=2.6f?yFoot:Orig(lip+d*aF+n*ll);float c=(yF-ye-me*Lr)/(Lr*Lr);
    v=(ae,ye,me,c);lanes[k]=v;return v;}
   var v=cur.vertices;var ov=orig.vertices;var M=ground.transform.localToWorldMatrix;var Mi=ground.transform.worldToLocalMatrix;
   var changed=new HashSet<int>();float maxCut=0,maxFill=0;int restored=0;
   for(int i=0;i<v.Length;i++){var wo=M.MultiplyPoint3x4(ov[i]);float a=A(wo),l=L(wo),al=Mathf.Abs(l);
    var wc=M.MultiplyPoint3x4(v[i]);float ny=wo.y;
    if(a>=aC-6&&a<=aRoof+.5f&&al<=7.5f&&RightOfMain(wo)){
     float w0=TrunkWeight(wo)*OtherRoads(wo);
     if(a<=aF+.5f){var ln=Lane(l);float u=a-ln.ae;if(u>0){float uu=Mathf.Min(u,aF-ln.ae);float target=ln.ye+ln.me*uu+ln.c*uu*uu;if(float.IsNaN(target))throw new Exception($"no profile for lane {l:F2} at along {a:F1}");float w=(1-Smooth(3.0f,7.0f,al))*Smooth(0,1.2f,u)*w0;ny=wo.y+(target-wo.y)*w;}}
     else{float boardY=Racer_BoardY(a);float target=Mathf.Min(wo.y,boardY-.08f);float w=(1-Smooth(3.2f,6f,al))*w0;ny=wo.y+(target-wo.y)*w;}}
    if(Mathf.Abs(ny-wc.y)>.0005f){changed.Add(i);if(Mathf.Abs(ny-wo.y)<.0005f)restored++;}
    maxCut=Mathf.Max(maxCut,wo.y-ny);maxFill=Mathf.Max(maxFill,ny-wo.y);var wn=wo;wn.y=ny;v[i]=Mi.MultiplyPoint3x4(wn);}
   foreach(var kv in lanes.OrderBy(k=>k.Key))if(Mathf.Abs(kv.Key/4f)<=2.5f&&kv.Key%2==0){var ln=kv.Value;float curv=-2*ln.c;log.Add($"  lane {kv.Key/4f,5:F1} m: leaves the main's verge at along {ln.ae:F1} ({ln.ye:F2} m, slope {ln.me:F3}), reaches the foot with slope {ln.me+2*ln.c*(aF-ln.ae):F3}; crest curvature {curv:F4}/m (grounded up to {(curv>0?Mathf.Sqrt(9.81f/curv):999):F0} m/s)");}
   log.Add($"ground: {changed.Count} vertices changed from the first grading ({restored} back to the original), deepest cut {maxCut:F2} m, highest fill {maxFill:F2} m from the original");
   {int bad=v.Count(q=>float.IsNaN(q.x+q.y+q.z));if(bad>0)throw new Exception($"{bad} NaN vertices computed");}
   cur.vertices=v;
   {var tri=cur.triangles;var touch=new HashSet<int>();for(int t=0;t<tri.Length;t+=3)if(changed.Contains(tri[t])||changed.Contains(tri[t+1])||changed.Contains(tri[t+2])){touch.Add(tri[t]);touch.Add(tri[t+1]);touch.Add(tri[t+2]);}
    var nr=cur.normals;var acc=touch.ToDictionary(k=>k,k=>Vector3.zero);for(int t=0;t<tri.Length;t+=3){int a=tri[t],b=tri[t+1],c=tri[t+2];if(!acc.ContainsKey(a)&&!acc.ContainsKey(b)&&!acc.ContainsKey(c))continue;var fn=Vector3.Cross(v[b]-v[a],v[c]-v[a]);foreach(var k in new[]{a,b,c})if(acc.ContainsKey(k))acc[k]+=fn;}
    foreach(var kv in acc)if(kv.Value.sqrMagnitude>1e-12f)nr[kv.Key]=kv.Value.normalized;cur.normals=nr;cur.RecalculateBounds();EditorUtility.SetDirty(cur);}
   UnityEngine.Object.DestroyImmediate(probe);gc.sharedMesh=null;gc.sharedMesh=cur;EditorUtility.SetDirty(gc);Physics.SyncTransforms();{var test=new Vector3(190,200,76.6f);if(!gc.Raycast(new Ray(test,Vector3.down),out var th,400))throw new Exception("the regraded ground does not collide");log.Add($"  ground collides again: {th.point.y:F2} at (190, 76.6)");}
   // centre-line profile after
   for(float a=aC;a<=aF;a+=2){var p=lip+d*a;float best=float.NaN;foreach(var h in Physics.RaycastAll(new Vector3(p.x,200,p.z),Vector3.down,400,~0,QueryTriggerInteraction.Ignore))if(h.collider.name.StartsWith("Ground_")&&(float.IsNaN(best)||h.point.y>best))best=h.point.y;log.Add($"  centre along {a,6:F1} (s {a-aC,5:F1}): ground {best:F2} (original {Orig2(orig,ground.transform,p):F2})");}
   // underlays kept below (Free Roam)
   foreach(var r in UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r=>r.name.StartsWith("Ground_")&&!r.GetComponent<Collider>()&&r.bounds.Intersects(new Bounds(lip+d*-30,new Vector3(60,40,60))))){
    var um=r.GetComponent<MeshFilter>().sharedMesh;var uv=um.vertices;var UM=r.transform.localToWorldMatrix;var UMi=r.transform.worldToLocalMatrix;int moved=0;
    for(int i=0;i<uv.Length;i++){var w=UM.MultiplyPoint3x4(uv[i]);float a=A(w),l=Mathf.Abs(L(w));if(a<aC-14||a>aRoof+.5f||l>8)continue;if(!gc.Raycast(new Ray(new Vector3(w.x,200,w.z),Vector3.down),out var h,400))continue;if(w.y>h.point.y-.05f){w.y=h.point.y-.12f;uv[i]=UMi.MultiplyPoint3x4(w);moved++;}}
    if(moved>0){if(!AssetDatabase.GetAssetPath(um).Contains("Report101")){um=UnityEngine.Object.Instantiate(um);var p=$"Assets/Scenery/Report101/{sn}-{r.name.Replace(' ','-')}-cabin-runup.asset";if(AssetDatabase.LoadAssetAtPath<Mesh>(p))AssetDatabase.DeleteAsset(p);AssetDatabase.CreateAsset(um,p);r.GetComponent<MeshFilter>().sharedMesh=um;}um.vertices=uv;um.RecalculateBounds();EditorUtility.SetDirty(um);}
    log.Add($"  underlay {r.name}: {moved} vertices kept below the new ground");}
   // re-seat: the decal, the lead-in posts, the run-up chevrons
   var root=GameObject.Find("Backyard optional forest shortcuts").transform;
   float Top(Vector3 p){float best=float.NaN;foreach(var h in Physics.RaycastAll(new Vector3(p.x,200,p.z),Vector3.down,400,~0,QueryTriggerInteraction.Ignore))if(h.collider.name.StartsWith("Ground_")&&(float.IsNaN(best)||h.point.y>best))best=h.point.y;return best;}
   {var dm=root.Find("Cabin wooded approach").GetComponent<MeshFilter>().sharedMesh;var dv=dm.vertices;for(int i=0;i<dv.Length;i++){float g=Top(dv[i]);if(float.IsNaN(g))throw new Exception("no ground under the decal");dv[i].y=g+.03f;}dm.vertices=dv;dm.RecalculateNormals();dm.RecalculateBounds();EditorUtility.SetDirty(dm);}
   foreach(Transform t in root){if(t.name=="Cabin run-up lead-in post"){var p=t.position;float g=Top(p);if(float.IsNaN(g))throw new Exception("no ground under a post");t.position=new Vector3(p.x,g+.55f,p.z);EditorUtility.SetDirty(t);log.Add($"  post re-seated on {g:F2}");}
    if(t.name=="Gold optional route chevron"&&A(t.position)<aF&&A(t.position)>aC){var p=t.position;float g=Top(p);if(float.IsNaN(g))throw new Exception("no ground under a chevron");t.position=new Vector3(p.x,g+.12f,p.z);EditorUtility.SetDirty(t);}}
   EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);}
  AssetDatabase.SaveAssets();}catch(Exception e){log.Add("FAILED "+e);}
  Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/grade.txt",log);EditorApplication.Exit(0);}
 static float Orig2(Mesh m,Transform t,Vector3 p){var go=new GameObject("p");go.transform.SetPositionAndRotation(t.position,t.rotation);var c=go.AddComponent<MeshCollider>();c.sharedMesh=m;Physics.SyncTransforms();float y=c.Raycast(new Ray(new Vector3(p.x,200,p.z),Vector3.down),out var h,400)?h.point.y:float.NaN;UnityEngine.Object.DestroyImmediate(go);return y;}
 // the boards' height along the axis (the ramp as built: 71.10 at the foot, a 3 m Hermite to +0.76, then 0.32 per metre)
 static float Racer_BoardY(float a){float s=22+(a-aF)/(0-aF)*19;float foot=yFoot;if(s<25){float t=Mathf.InverseLerp(22,25,s),t2=t*t,t3=t2*t;return (2*t3-3*t2+1)*foot+(t3-2*t2+t)*3*.15f+(-2*t3+3*t2)*(foot+.76f)+(t3-t2)*3*.32f;}return foot+.76f+(s-25)*.32f;}
}
