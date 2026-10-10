using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 0.101 Part A, final shape of the run-up and the boards (both scenes). The ground falls from the main road's verge at about 0.27 to a ramp
// foot that sits too low, so the run-up had to flatten hard and threw a vehicle at ~30 m/s 15 m through the air onto the boards. The foot of
// the leaning boards is raised by RAISE (default 0.6 m) on a small earth fill: each lane of the run-up now leaves the main road's verge with
// the slope the vehicle already has there and flattens evenly to the raised foot, and the boards curve up from the run-up's slope to the
// roof's 0.32 at the roof edge. The roof, the lip, its height and its launch angle are unchanged. Rebuilds: the run-up surface (with sides
// down to the ground), the boards' collider and visuals, the route's / Free Roam line's heights, the ground kept under the surface.
public static class Report101Ramp {
 static string Out=>Environment.GetEnvironmentVariable("PROBE_OUT");
 static readonly Vector3 lip=new(224.61f,74.15f,80.95f);const float heading=82.82f,aC=-44.72f,aF=-19.00f,aRoof=-9.00f,yFoot0=71.10f,yRoof=74.10f,Half=3.0f,BoardWidth=5.0f,RoofWidth=9.6f;
 static Vector3 d=>new(Mathf.Sin(heading*Mathf.Deg2Rad),0,Mathf.Cos(heading*Mathf.Deg2Rad));static Vector3 n=>new(-d.z,0,d.x);
 static Vector3 Flat(Vector3 v){v.y=0;return v;}static float A(Vector3 p)=>Vector3.Dot(Flat(p-lip),d);static float L(Vector3 p)=>Vector3.Dot(Flat(p-lip),n);
 static float Smooth(float e0,float e1,float x)=>Mathf.SmoothStep(0,1,Mathf.InverseLerp(e0,e1,x));
 static float Raise=>float.Parse(Environment.GetEnvironmentVariable("RAISE")??"0.6",System.Globalization.CultureInfo.InvariantCulture);
 static float Herm(float x,float a,float b,float ya,float yb,float ma,float mb){float t=Mathf.InverseLerp(a,b,x),t2=t*t,t3=t2*t,h=b-a;return (2*t3-3*t2+1)*ya+(t3-2*t2+t)*h*ma+(-2*t3+3*t2)*yb+(t3-t2)*h*mb;}
 public static void Run(){var log=new List<string>();
  try{
  float mFoot=0;
  foreach(var (sn,origPath) in new[]{("DansBackyardForward","Assets/Track/ReverseCorrection/Local bore mouths Ground_480_400-804.asset"),("FreeRoamWorld","Assets/Scenery/Report096/FreeRoamWorld-Ground_480_400-conformed.asset")}){
   var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+sn+".unity");log.Add("===== "+sn);
   var ground=GameObject.Find("Memory loop - north is +Z/Ground_480_400");var gc=ground.GetComponent<MeshCollider>();var cur=ground.GetComponent<MeshFilter>().sharedMesh;
   var orig=AssetDatabase.LoadAssetAtPath<Mesh>(origPath);var probe=new GameObject("orig probe (not ground)");probe.transform.SetPositionAndRotation(ground.transform.position,ground.transform.rotation);var pc=probe.AddComponent<MeshCollider>();pc.sharedMesh=orig;Physics.SyncTransforms();
   float Orig(Vector3 p)=>pc.Raycast(new Ray(new Vector3(p.x,200,p.z),Vector3.down),out var h,400)?h.point.y:float.NaN;
   var roads=UnityEngine.Object.FindObjectsByType<Racer.RaceRoad>(FindObjectsSortMode.None).Where(r=>r.points!=null&&r.points.Length>3).ToList();foreach(var r in roads)r.Initialize();var main=roads.First(r=>r.name=="Forward navigation only - no road mesh");
   float Lat(Racer.RaceRoad r,Vector3 p,out float hw){var q=p;float s=r.Project(q,out _);q.y=r.At(s,out _).y;s=r.Project(q,out _);var c=r.At(s,out _);hw=r.HalfWidth(s);return Mathf.Abs(c.y-p.y)>6?999:new Vector2(p.x-c.x,p.z-c.z).magnitude;}
   bool RightOfMain(Vector3 p){float s=main.Project(p,out _);if(Mathf.Abs(s-857.4f)>45)return A(p)>aC;var c=main.At(s,out var f);return Vector3.Dot(Flat(p-c),Vector3.Cross(Vector3.up,Flat(f)))>0;}
   float OtherRoads(Vector3 p){float w=1;foreach(var r in roads){if(r==main)continue;w*=Smooth(.8f,1.6f,Lat(r,p,out _));}return w;}
   float yF=yFoot0+Raise;bool ballistic=Environment.GetEnvironmentVariable("BALLISTIC")=="1";const float Vd=34,Rebound=.04f;float gk=9.81f/(2*Vd*Vd);
   var lanes=new Dictionary<int,(float ae,float ye,float me,float c)>();
   (float ae,float ye,float me,float c) Lane(float l){int k=Mathf.RoundToInt(l*4);if(lanes.TryGetValue(k,out var v))return v;float ll=k/4f;float ae=aC-3-Mathf.Max(0,-ll)*1.6f;
    while(ae<aF-4){var p=lip+d*ae+n*ll;p.y=Orig(p);if(float.IsNaN(p.y)){ae+=.05f;continue;}float lat=Lat(main,p,out float hw);if(RightOfMain(p)&&lat>=hw+.8f)break;ae+=.05f;}
    var pe=lip+d*ae+n*ll;float ye=Orig(pe);
    // the slope the vehicle has as it reaches the verge: the steeper of the ground just before and just after it
    float mBefore=(ye-Orig(pe-d*1.5f))/1.5f,mAfter=(Orig(pe+d*1.5f)-ye)/1.5f,me=Mathf.Max(mBefore,mAfter);if(ballistic)me+=Rebound;
    float Lr=aF-ae;float target=Mathf.Abs(ll)<=2.6f?yF:Mathf.Lerp(yF,Orig(lip+d*aF+n*ll),Smooth(2.6f,Half,Mathf.Abs(ll)));float c=(target-ye-me*Lr)/(Lr*Lr);v=(ae,ye,me,c);lanes[k]=v;return v;}
   if(ballistic){var l0r=Lane(0);float L0=aF-l0r.ae;yF=l0r.ye+l0r.me*L0-gk*L0*L0;lanes.Clear();log.Add($"  design: each lane leaves the verge with the ground's slope +{Rebound} (the lift a vehicle carries off the main) and falls no faster than a {Vd} m/s flight: the ramp foot at {yF:F2} ({yF-yFoot0:+0.00} m)");}
   float Prof(float l,float a){var ln=Lane(l);float u=Mathf.Clamp(a-ln.ae,0,aF-ln.ae);float y=ln.ye+ln.me*u+ln.c*u*u;if(a>aF){float mf=ln.me+2*ln.c*(aF-ln.ae);y+=mf*(a-aF);}return y;}
   var l0=Lane(0);mFoot=l0.me+2*l0.c*(aF-l0.ae);
   float Board(float a){if(!ballistic&&Raise<.01f){float so=22+(a-aF)/(0-aF)*19;if(so<25)return Herm(so,22,25,yFoot0,yFoot0+.76f,.15f,.32f);return yFoot0+.76f+(so-25)*.32f;}return a<=aRoof?Herm(a,aF,aRoof,yF,yRoof,mFoot,.32f):yRoof+.32f*(a-aRoof);}
   foreach(var k in new[]{-10,-6,-2,0,2,6,10}){var ln=Lane(k/4f);float curv=-2*ln.c;log.Add($"  lane {k/4f,5:F1}: verge at along {ln.ae:F1} ({ln.ye:F2} m, slope {ln.me:F3}) -> foot {Prof(k/4f,aF):F2} slope {ln.me+2*ln.c*(aF-ln.ae):F3}; curvature {curv:F4}/m (grounded to {(curv>0?Mathf.Sqrt(9.81f/curv):999):F0} m/s)");}
   {float worst=0;for(float a=aF;a<aRoof;a+=.25f){float s0=(Board(a+.25f)-Board(a))/.25f,s1=(Board(a+.5f)-Board(a+.25f))/.25f;worst=Mathf.Min(worst,s1-s0);}log.Add($"  boards: foot raised {Raise:F2} m to {yF:F2}, from slope {mFoot:F3} to 0.320 at the roof edge ({yRoof:F2}); the largest slope drop on the boards {worst:F4} per 0.25 m (none = never convex)");}
   // ---- run-up surface (to 1.5 m under the boards), with sides down into the ground ----
   int cols=Mathf.RoundToInt(Half*2/.25f)+1;const int rows=64;var vs=new List<Vector3>();var ts=new List<int>();var uv=new List<Vector2>();
   for(int c=0;c<cols;c++){float l=-Half+c*.25f;var ln=Lane(l);float a0=ln.ae,a1=aF+1.5f;for(int r=0;r<rows;r++){float a=Mathf.Lerp(a0,a1,r/(rows-1f));float u=a-a0;float y=Prof(l,a)+Mathf.Lerp(-.03f,0,Smooth(0,1.2f,u));if(ballistic){float tb=Smooth(aF-1.5f,aF,a);float yb=a<=aF?Board(aF)+mFoot*(a-aF):Board(a)-.002f;y=Mathf.Lerp(y,yb,tb);}else if(a>aF)y=Mathf.Min(y,Board(a)-.04f);var p=lip+d*a+n*l;p.y=y;vs.Add(p);uv.Add(new Vector2(l*.3f,a*.3f));}}
   for(int c=1;c<cols;c++)for(int r=1;r<rows;r++){int a=(c-1)*rows+r-1,b=a+1,e=c*rows+r-1,f=e+1;ts.AddRange(new[]{a,b,e,b,f,e});}
   float GroundBelow(Vector3 p){float g=Orig(p);return float.IsNaN(g)?p.y-.3f:Mathf.Min(g,p.y)-.12f;}
   // the sides: a 1:4 earth slope down to the ground (drivable from the side; a vertical edge acted as a kicker)
   foreach(int c in new[]{0,cols-1}){int b0=vs.Count;var outward=c==0?-n:n;for(int r=0;r<rows;r++){var p=vs[c*rows+r];vs.Add(p);float g0=Orig(p);float drop=float.IsNaN(g0)?.3f:Mathf.Max(.05f,p.y-g0);var q=p+outward*Mathf.Max(.4f,drop*4);float gq=Orig(q);q.y=(float.IsNaN(gq)?p.y-drop:Mathf.Min(gq,p.y))-.05f;if(A(p)>aF+.05f)q=new Vector3(p.x,GroundBelow(p),p.z);vs.Add(q);uv.Add(Vector2.zero);uv.Add(new Vector2(0,1));}for(int r=1;r<rows;r++){int k=b0+(r-1)*2;if(c==0)ts.AddRange(new[]{k,k+2,k+1,k+1,k+2,k+3});else ts.AddRange(new[]{k,k+1,k+2,k+1,k+3,k+2});}}
   {int b0=vs.Count;for(int c=0;c<cols;c++){var p=vs[c*rows+rows-1];vs.Add(p);var q=p;q.y=GroundBelow(p);vs.Add(q);uv.Add(Vector2.zero);uv.Add(Vector2.zero);}for(int c=1;c<cols;c++){int k=b0+(c-1)*2;ts.AddRange(new[]{k,k+1,k+2,k+1,k+3,k+2});}}
   var m=new Mesh{name="Cabin run-up surface (0.101) "+sn};m.SetVertices(vs);m.SetUVs(0,uv);m.SetTriangles(ts,0);m.RecalculateNormals();
   if(m.normals[rows/2].y<0){for(int k=0;k<ts.Count;k+=3)(ts[k+1],ts[k+2])=(ts[k+2],ts[k+1]);m.SetTriangles(ts,0);m.RecalculateNormals();}m.RecalculateBounds();
   var path="Assets/Scenery/Report101/"+sn+"-Cabin run-up surface.asset";var ex=AssetDatabase.LoadAssetAtPath<Mesh>(path);EditorUtility.CopySerialized(m,ex);m=ex;
   var root=GameObject.Find("Backyard optional forest shortcuts").transform;var surf=root.Find("Ground_Cabin run-up surface (0.101)");surf.GetComponent<MeshFilter>().sharedMesh=m;var smc=surf.GetComponent<MeshCollider>();smc.sharedMesh=null;smc.sharedMesh=m;
   // ---- the ground under it: from the original (Grade/Surface), kept 6 cm below the surface ----
   {var v=cur.vertices;var ov=orig.vertices;var M=ground.transform.localToWorldMatrix;var Mi=ground.transform.worldToLocalMatrix;int lowered=0;var changed=new HashSet<int>();
    for(int i=0;i<v.Length;i++){var w=M.MultiplyPoint3x4(v[i]);float a=A(w),l=L(w);if(Mathf.Abs(l)>Half+.4f||a>aF+1.6f||a<aC-8||!RightOfMain(w))continue;var ln=Lane(Mathf.Clamp(l,-Half,Half));if(a<ln.ae)continue;
     float top=Prof(Mathf.Clamp(l,-Half,Half),a)+Mathf.Lerp(-.03f,0,Smooth(0,1.2f,a-ln.ae))-.06f-Mathf.Max(0,Mathf.Abs(l)-Half)*.25f;if(a>aF)top=Mathf.Min(top,Board(a)-.1f);if(w.y<=top)continue;if(OtherRoads(w)<.5f)continue;w.y=top;v[i]=Mi.MultiplyPoint3x4(w);lowered++;changed.Add(i);}
    if(v.Any(q=>float.IsNaN(q.x+q.y+q.z)))throw new Exception("NaN");cur.vertices=v;
    var tri=cur.triangles;var touch=new HashSet<int>();for(int t=0;t<tri.Length;t+=3)if(changed.Contains(tri[t])||changed.Contains(tri[t+1])||changed.Contains(tri[t+2])){touch.Add(tri[t]);touch.Add(tri[t+1]);touch.Add(tri[t+2]);}
    var nr=cur.normals;var acc=touch.ToDictionary(k=>k,k=>Vector3.zero);for(int t=0;t<tri.Length;t+=3){int a=tri[t],b=tri[t+1],c=tri[t+2];if(!acc.ContainsKey(a)&&!acc.ContainsKey(b)&&!acc.ContainsKey(c))continue;var fn=Vector3.Cross(v[b]-v[a],v[c]-v[a]);foreach(var k in new[]{a,b,c})if(acc.ContainsKey(k))acc[k]+=fn;}
    foreach(var kv in acc)if(kv.Value.sqrMagnitude>1e-12f)nr[kv.Key]=kv.Value.normalized;cur.normals=nr;cur.RecalculateBounds();EditorUtility.SetDirty(cur);gc.sharedMesh=null;gc.sharedMesh=cur;log.Add($"  ground: {lowered} more vertices kept under the surface");}
   UnityEngine.Object.DestroyImmediate(probe);Physics.SyncTransforms();
   // ---- boards: collider strip (boards then roof) and the 9 visual boards ----
   {var bv=new List<Vector3>();var bt=new List<int>();void Row(float a,float w){var p=lip+d*a;p.y=Board(a);bv.Add(p-n*w/2);bv.Add(p+n*w/2);if(bv.Count>2){int k=bv.Count-4;bt.AddRange(new[]{k,k+1,k+2,k+1,k+3,k+2});}}
    // the collider starts 1.5 m before the boards, flush on the run-up (no lip where the run-up meets the boards)
    for(float a=aF;a<aRoof;a+=.4f)Row(a,BoardWidth);Row(aRoof,BoardWidth);for(float a=aRoof;a<0;a+=.4f)Row(a,RoofWidth);Row(0,RoofWidth);
    var cm=new Mesh();cm.SetVertices(bv);cm.SetTriangles(bt,0);cm.RecalculateNormals();if(cm.normals[0].y<0){bt.Clear();for(int k=0;k+3<bv.Count;k+=2)bt.AddRange(new[]{k,k+2,k+1,k+1,k+2,k+3});cm.SetTriangles(bt,0);cm.RecalculateNormals();}cm.RecalculateBounds();
    var cp="Assets/Track/BackyardShortcuts/Takeoff - Leaning boards through cabin roof (0.101).asset";var cex=AssetDatabase.LoadAssetAtPath<Mesh>(cp);cm.name=cex.name;EditorUtility.CopySerialized(cm,cex);var col=root.Find("Takeoff - Leaning boards through cabin roof").GetComponent<MeshCollider>();col.sharedMesh=null;col.sharedMesh=cex;}
   for(int i=0;i<9;i++){float off=(i-4)*.55f;var mv=new List<Vector3>();var mt=new List<int>();int j=0;for(float a=aF;;a=Mathf.Min(aRoof,a+.25f)){var p=lip+d*a;p.y=Board(a);mv.Add(p+n*(off-.26f));mv.Add(p+n*(off+.26f));if(j++>0){int k=mv.Count-4;mt.AddRange(new[]{k,k+2,k+1,k+1,k+2,k+3});}if(a>=aRoof)break;}
    var bm=new Mesh();bm.SetVertices(mv);bm.SetTriangles(mt,0);bm.RecalculateNormals();if(bm.normals[0].y<0){mt.Clear();for(int k=0;k+3<mv.Count;k+=2)mt.AddRange(new[]{k,k+1,k+2,k+1,k+3,k+2});bm.SetTriangles(mt,0);bm.RecalculateNormals();}bm.RecalculateBounds();
    var bp="Assets/Track/BackyardShortcuts/Leaning weathered board (0.101) "+i+".asset";var bex=AssetDatabase.LoadAssetAtPath<Mesh>(bp);bm.name=bex.name;EditorUtility.CopySerialized(bm,bex);}
   // ---- the line's heights (the race's route / Free Roam's reset line) ----
   {var line=UnityEngine.Object.FindObjectsByType<Racer.ShortcutUndergrowth>(FindObjectsSortMode.None).First(u=>u.clearRoute&&u.clearRoute.title=="Abandoned Cabin Jump").clearRoute;var pts=line.points;int moved=0;
    for(int i=0;i<pts.Length;i++){float a=A(pts[i]);if(a>.01f||a<aC-.01f)continue;float y=a<=aF?Prof(0,a):Board(a);if(a<l0.ae)y=pts[i].y;if(Mathf.Abs(pts[i].y-y)>.001f){pts[i].y=y;moved++;}}
    line.points=pts;if(line.gameObject.activeInHierarchy){line.entrySpeed=25;line.entrySpeedDistance=8;}EditorUtility.SetDirty(line);log.Add($"  line {line.name}: {moved} point heights set to the new run-up and boards");}
   // ---- the boards' chevron pair, the run-up chevrons and posts, re-seated ----
   float Top(Vector3 p){float best=float.NaN;foreach(var h in Physics.RaycastAll(new Vector3(p.x,200,p.z),Vector3.down,400,~0,QueryTriggerInteraction.Ignore))if((h.collider.name.StartsWith("Ground_")||h.collider.name.StartsWith("Takeoff"))&&(float.IsNaN(best)||h.point.y>best))best=h.point.y;return best;}
   Physics.SyncTransforms();
   foreach(Transform t in root){if(t.name=="Cabin run-up lead-in post"){var p=t.position;t.position=new Vector3(p.x,Top(p)+.55f,p.z);}if(t.name=="Gold optional route chevron"&&A(t.position)<aRoof&&A(t.position)>aC){var p=t.position;t.position=new Vector3(p.x,Top(p)+.12f,p.z);}}
   EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);}
  AssetDatabase.SaveAssets();}catch(Exception e){log.Add("FAILED "+e);}
  Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/ramp.txt",log);EditorApplication.Exit(0);}
}
