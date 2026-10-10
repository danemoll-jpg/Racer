using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 0.101 Part A, the run-up's driving surface: the 2 m faceted ground under the run-up has small creases every 2 m that kick a vehicle into
// the air at speed. A smooth surface (0.25 m lanes, 0.5 m rows, collider named Ground_ like the ground) is laid on each lane's profile
// (Report101Grade's: from the main road's verge with the ground's own height and slope, flattening evenly to the ramp foot), 6 m wide,
// from the verge to just under the boards, with short skirts at its sides. The ground under it is kept 6 cm below it. It replaces the
// run-up decal (same material). Both scenes.
public static class Report101Surface {
 static string Out=>Environment.GetEnvironmentVariable("PROBE_OUT");
 static readonly Vector3 lip=new(224.61f,74.15f,80.95f);const float heading=82.82f,aC=-44.72f,aF=-19.00f,yFoot=71.10f,Half=3.0f;
 static Vector3 d=>new(Mathf.Sin(heading*Mathf.Deg2Rad),0,Mathf.Cos(heading*Mathf.Deg2Rad));static Vector3 n=>new(-d.z,0,d.x);
 static Vector3 Flat(Vector3 v){v.y=0;return v;}static float A(Vector3 p)=>Vector3.Dot(Flat(p-lip),d);static float L(Vector3 p)=>Vector3.Dot(Flat(p-lip),n);
 static float Smooth(float e0,float e1,float x)=>Mathf.SmoothStep(0,1,Mathf.InverseLerp(e0,e1,x));
 public static void Run(){var log=new List<string>();
  try{
  foreach(var (sn,origPath) in new[]{("DansBackyardForward","Assets/Track/ReverseCorrection/Local bore mouths Ground_480_400-804.asset"),("FreeRoamWorld","Assets/Scenery/Report096/FreeRoamWorld-Ground_480_400-conformed.asset")}){
   var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+sn+".unity");log.Add("===== "+sn);
   var ground=GameObject.Find("Memory loop - north is +Z/Ground_480_400");var gf=ground.GetComponent<MeshFilter>();var gc=ground.GetComponent<MeshCollider>();var cur=gf.sharedMesh;
   var orig=AssetDatabase.LoadAssetAtPath<Mesh>(origPath);var probe=new GameObject("orig probe (not ground)");probe.transform.SetPositionAndRotation(ground.transform.position,ground.transform.rotation);var pc=probe.AddComponent<MeshCollider>();pc.sharedMesh=orig;Physics.SyncTransforms();
   float Orig(Vector3 p)=>pc.Raycast(new Ray(new Vector3(p.x,200,p.z),Vector3.down),out var h,400)?h.point.y:float.NaN;
   var roads=UnityEngine.Object.FindObjectsByType<Racer.RaceRoad>(FindObjectsSortMode.None).Where(r=>r.points!=null&&r.points.Length>3).ToList();foreach(var r in roads)r.Initialize();var main=roads.First(r=>r.name=="Forward navigation only - no road mesh");
   float Lat(Racer.RaceRoad r,Vector3 p,out float hw){var q=p;float s=r.Project(q,out _);q.y=r.At(s,out _).y;s=r.Project(q,out _);var c=r.At(s,out _);hw=r.HalfWidth(s);return Mathf.Abs(c.y-p.y)>6?999:new Vector2(p.x-c.x,p.z-c.z).magnitude;}
   bool RightOfMain(Vector3 p){float s=main.Project(p,out _);if(Mathf.Abs(s-857.4f)>45)return A(p)>aC;var c=main.At(s,out var f);return Vector3.Dot(Flat(p-c),Vector3.Cross(Vector3.up,Flat(f)))>0;}
   float OtherRoads(Vector3 p){float w=1;foreach(var r in roads){if(r==main)continue;w*=Smooth(.8f,1.6f,Lat(r,p,out _));}return w;}
   var lanes=new Dictionary<int,(float ae,float ye,float me,float c)>();
   (float ae,float ye,float me,float c) Lane(float l){int k=Mathf.RoundToInt(l*4);if(lanes.TryGetValue(k,out var v))return v;float ll=k/4f;float ae=aC-3-Mathf.Max(0,-ll)*1.6f;
    while(ae<aF-4){var p=lip+d*ae+n*ll;p.y=Orig(p);if(float.IsNaN(p.y)){ae+=.05f;continue;}float lat=Lat(main,p,out float hw);if(RightOfMain(p)&&lat>=hw+.8f)break;ae+=.05f;}
    var pe=lip+d*ae+n*ll;float ye=Orig(pe),y1=Orig(pe+d*1.5f),me=(y1-ye)/1.5f;float Lr=aF-ae;float c=(yFoot-ye-me*Lr)/(Lr*Lr);v=(ae,ye,me,c);lanes[k]=v;return v;}
   float Prof(float l,float a){var ln=Lane(l);float u=Mathf.Clamp(a-ln.ae,0,aF+.5f-ln.ae);return ln.ye+ln.me*u+ln.c*u*u;}
   // the surface: lanes -Half..Half every 0.25 m; rows from each lane's verge to aF+0.5, every 0.5 m (24 rows per lane-strip, resampled per lane)
   int cols=Mathf.RoundToInt(Half*2/.25f)+1;const int rows=60;var vs=new List<Vector3>();var ts=new List<int>();var uv=new List<Vector2>();
   for(int c=0;c<cols;c++){float l=-Half+c*.25f;var ln=Lane(l);float a0=ln.ae,a1=aF+.5f;for(int r=0;r<rows;r++){float a=Mathf.Lerp(a0,a1,r/(rows-1f));float u=a-a0;float y=Prof(l,a)+Mathf.Lerp(-.03f,.0f,Smooth(0,1.2f,u));var p=lip+d*a+n*l;p.y=y;vs.Add(p);uv.Add(new Vector2(l*.3f,a*.3f));}}
   for(int c=1;c<cols;c++)for(int r=1;r<rows;r++){int a=(c-1)*rows+r-1,b=a+1,e=c*rows+r-1,f=e+1;ts.AddRange(new[]{a,b,e,b,f,e});}
   // skirts: both sides, 15 cm down
   foreach(int c in new[]{0,cols-1}){int b0=vs.Count;for(int r=0;r<rows;r++){var p=vs[c*rows+r];vs.Add(p);vs.Add(p-Vector3.up*.15f);uv.Add(Vector2.zero);uv.Add(Vector2.zero);}for(int r=1;r<rows;r++){int k=b0+(r-1)*2;if(c==0)ts.AddRange(new[]{k,k+2,k+1,k+1,k+2,k+3});else ts.AddRange(new[]{k,k+1,k+2,k+1,k+3,k+2});}}
   var m=new Mesh{name="Cabin run-up surface (0.101) "+sn};m.SetVertices(vs);m.SetUVs(0,uv);m.SetTriangles(ts,0);m.RecalculateNormals();
   if(m.normals[rows/2].y<0){for(int k=0;k<ts.Count;k+=3)(ts[k+1],ts[k+2])=(ts[k+2],ts[k+1]);m.SetTriangles(ts,0);m.RecalculateNormals();}m.RecalculateBounds();
   var path="Assets/Scenery/Report101/"+sn+"-Cabin run-up surface.asset";var ex=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(ex){EditorUtility.CopySerialized(m,ex);m=ex;}else AssetDatabase.CreateAsset(m,path);
   // crest check along each lane: the steepest change of slope over 0.5 m and its curvature
   float worstK=0;for(int c=0;c<cols;c+=2){float l=-Half+c*.25f;var ln=Lane(l);worstK=Mathf.Max(worstK,-2*ln.c);}
   log.Add($"surface: {cols} lanes x {rows} rows, {ts.Count/3} triangles; lanes leave the main's verge between along {lanes.Values.Min(x=>x.ae):F1} and {lanes.Values.Max(x=>x.ae):F1}; the sharpest crest curvature {worstK:F4}/m (fully grounded up to {Mathf.Sqrt(9.81f/Mathf.Max(worstK,1e-4f)):F0} m/s)");
   // the ground under it: 6 cm below the surface (where another road does not own it)
   var v=cur.vertices;var M=ground.transform.localToWorldMatrix;var Mi=ground.transform.worldToLocalMatrix;int lowered=0;float most=0;var changedSet=new HashSet<int>();
   for(int i=0;i<v.Length;i++){var w=M.MultiplyPoint3x4(v[i]);float a=A(w),l=L(w);if(Mathf.Abs(l)>Half+.4f||a>aF+.6f||a<aC-8||!RightOfMain(w))continue;var ln=Lane(Mathf.Clamp(l,-Half,Half));if(a<ln.ae)continue;
    float top=Prof(Mathf.Clamp(l,-Half,Half),a)+Mathf.Lerp(-.03f,0,Smooth(0,1.2f,a-ln.ae))-.06f;if(w.y<=top)continue;if(OtherRoads(w)<.5f)continue;most=Mathf.Max(most,w.y-top);w.y=top;v[i]=Mi.MultiplyPoint3x4(w);lowered++;changedSet.Add(i);}
   if(v.Any(q=>float.IsNaN(q.x+q.y+q.z)))throw new Exception("NaN");
   cur.vertices=v;{var tri=cur.triangles;var touch=new HashSet<int>();for(int t=0;t<tri.Length;t+=3)if(changedSet.Contains(tri[t])||changedSet.Contains(tri[t+1])||changedSet.Contains(tri[t+2])){touch.Add(tri[t]);touch.Add(tri[t+1]);touch.Add(tri[t+2]);}
    var nr=cur.normals;var acc=touch.ToDictionary(k=>k,k=>Vector3.zero);for(int t=0;t<tri.Length;t+=3){int a=tri[t],b=tri[t+1],c=tri[t+2];if(!acc.ContainsKey(a)&&!acc.ContainsKey(b)&&!acc.ContainsKey(c))continue;var fn=Vector3.Cross(v[b]-v[a],v[c]-v[a]);foreach(var k in new[]{a,b,c})if(acc.ContainsKey(k))acc[k]+=fn;}
    foreach(var kv in acc)if(kv.Value.sqrMagnitude>1e-12f)nr[kv.Key]=kv.Value.normalized;cur.normals=nr;}cur.RecalculateBounds();EditorUtility.SetDirty(cur);gc.sharedMesh=null;gc.sharedMesh=cur;UnityEngine.Object.DestroyImmediate(probe);Physics.SyncTransforms();
   if(!gc.Raycast(new Ray(new Vector3(190,200,76.6f),Vector3.down),out var th,400))throw new Exception("ground lost its collision");
   log.Add($"ground: {lowered} vertices kept 6 cm under the surface (at most {most:F2} m lowered)");
   // replace the decal object by the surface
   var root=GameObject.Find("Backyard optional forest shortcuts").transform;var old=root.Find("Cabin wooded approach");var mat=old.GetComponent<MeshRenderer>().sharedMaterial;UnityEngine.Object.DestroyImmediate(old.gameObject);
   var prev=root.Find("Ground_Cabin run-up surface (0.101)");if(prev)UnityEngine.Object.DestroyImmediate(prev.gameObject);
   var go=new GameObject("Ground_Cabin run-up surface (0.101)");go.transform.SetParent(root,false);go.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);go.AddComponent<MeshFilter>().sharedMesh=m;go.AddComponent<MeshRenderer>().sharedMaterial=mat;go.AddComponent<MeshCollider>().sharedMesh=m;go.layer=ground.layer;
   Physics.SyncTransforms();
   // re-seat posts and run-up chevrons on the surface
   float Top(Vector3 p){float best=float.NaN;foreach(var h in Physics.RaycastAll(new Vector3(p.x,200,p.z),Vector3.down,400,~0,QueryTriggerInteraction.Ignore))if(h.collider.name.StartsWith("Ground_")&&(float.IsNaN(best)||h.point.y>best))best=h.point.y;return best;}
   foreach(Transform t in root){if(t.name=="Cabin run-up lead-in post"){var p=t.position;t.position=new Vector3(p.x,Top(p)+.55f,p.z);}if(t.name=="Gold optional route chevron"&&A(t.position)<aF&&A(t.position)>aC){var p=t.position;t.position=new Vector3(p.x,Top(p)+.12f,p.z);}}
   // fine profile along three lanes on what a wheel now meets
   foreach(float l in new[]{-1.5f,0f,1.5f}){float py=float.NaN,ps=float.NaN,worst=0,at=0;var ln=Lane(l);for(float a=ln.ae+.5f;a<=aF;a+=.25f){var p=lip+d*a+n*l;float y=Top(p);float sl=float.IsNaN(py)?float.NaN:(y-py)/.25f;if(!float.IsNaN(ps)&&!float.IsNaN(sl)&&sl-ps<worst){worst=sl-ps;at=a-aC;}py=y;ps=sl;}log.Add($"  lane {l:+0.0;-0.0;0}: the largest drop in slope between 0.25 m steps {worst:F3} (at s {at:F1})");}
   EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);}
  AssetDatabase.SaveAssets();}catch(Exception e){log.Add("FAILED "+e);}
  Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/surface.txt",log);EditorApplication.Exit(0);}
}
