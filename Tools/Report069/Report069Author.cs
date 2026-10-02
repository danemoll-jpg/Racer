using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
// 0.69 local repairs for Dan's debug session 2026-10-02_17-29-13-319_325918 (PROJECT_TODO "CURRENT" Parts A-D).
// Every change is local and logged to Docs/Report069/author-notes.txt. No route, checkpoint, physics or AI data changes.
// Section 5A: jump ramps/lips/flights/landings, multi-level routes, tunnels, junctions and the 0.68 berms are never edited.
public static class Report069Author {
 const string Folder="Assets/Track/Report069";
 static readonly List<string> notes=new();
 static void Note(string s){notes.Add(s);Debug.Log("REPORT069 "+s);}
 static string Scene=>UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
 static Color Earth=new(.39f,.48f,.29f),Rock=new(.34f,.36f,.29f);
 static bool IsDrive(Collider c)=>c&&c.name.Contains("driving surface");
 static bool IsTerrain(Collider c)=>c is MeshCollider&&!c.isTrigger&&!c.attachedRigidbody&&c.name.StartsWith("Ground");
 // Natural barriers, rock formations and structural roofs: sampled as ground but never lowered or built over.
 static bool IsBarrier(Collider c)=>c&&(c.name.Contains("natural edge")||c.name.Contains("outcrop")||c.name.Contains("boulder")||c.name.Contains("vault")||c.name.Contains("barrier")||c.name.Contains("portal")||c.name.Contains("rock"));
 static bool IsRoof(Collider c)=>c&&(c.name.Contains("mountain cap")||c.name.Contains("tunnel ceiling")||c.name.Contains("South Face hillside"));
 static bool Solid(Collider c)=>c&&!c.isTrigger&&!c.attachedRigidbody;
 static bool Surface(float x,float z,float from,Func<Collider,bool> ok,out RaycastHit hit,float depth=260){var o=new Vector3(x,from,z);hit=default;
  for(int k=0;k<60&&depth>0;k++){if(!Physics.Raycast(o,Vector3.down,out var h,depth,~0,QueryTriggerInteraction.Ignore))return false;if(h.normal.y>0&&ok(h.collider)){hit=h;return true;}depth-=h.distance+.02f;o=h.point+Vector3.down*.02f;}return false;}
 static bool Pave(Vector3 q,float near,out float y){y=0;if(!Surface(q.x,q.z,near+.6f,IsDrive,out var h,1.2f))return false;y=h.point.y;return true;}
 // Ground beside the road: first solid upward non-pavement surface below 'from' (excluding this round's new meshes).
 static bool Ground(Vector3 q,float from,float depth,out RaycastHit hit)=>Surface(q.x,q.z,from,c=>IsTerrain(c)&&!IsDrive(c)&&!c.name.StartsWith("Ground_Report069"),out hit,depth);
 static Mesh Store(string name,Mesh mesh){Directory.CreateDirectory(Folder);string path=$"{Folder}/{Scene}-{name}.asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(old){EditorUtility.CopySerialized(mesh,old);Object.DestroyImmediate(mesh);return old;}AssetDatabase.CreateAsset(mesh,path);return mesh;}
 static Mesh Build(List<Vector3> v,List<Color> c,List<int> t){var m=new Mesh{indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};m.SetVertices(v);m.SetColors(c);m.SetTriangles(t,0);m.RecalculateNormals();m.RecalculateBounds();return m;}
 static void Tri(List<Vector3> v,List<Color> c,List<int> t,Vector3 a,Vector3 b,Vector3 d,Vector3 want){var n=Vector3.Cross(b-a,d-a);if(n.sqrMagnitude<1e-8f)return;if(Vector3.Dot(n,want)<0){var x=b;b=d;d=x;n=-n;}var col=n.normalized.y>.72f?Earth:Rock;int k=v.Count;v.AddRange(new[]{a,b,d});c.AddRange(new[]{col,col,col});t.AddRange(new[]{k,k+1,k+2});}
 static GameObject Make(string name,Mesh mesh,Material mat){foreach(var old in Object.FindObjectsByType<MeshFilter>().Where(x=>x.name=="Ground_Report069 "+name).ToArray())Object.DestroyImmediate(old.gameObject);
  var g=new GameObject("Ground_Report069 "+name,typeof(MeshFilter),typeof(MeshRenderer),typeof(MeshCollider));g.GetComponent<MeshFilter>().sharedMesh=mesh;g.GetComponent<MeshRenderer>().sharedMaterial=mat;g.GetComponent<MeshCollider>().sharedMesh=mesh;GameObjectUtility.SetStaticEditorFlags(g,StaticEditorFlags.BatchingStatic);Physics.SyncTransforms();return g;}
 static Material GroundMaterial(){var r=Object.FindObjectsByType<MeshRenderer>().FirstOrDefault(x=>x.name.Contains("continuous solid 0"))??Object.FindObjectsByType<MeshRenderer>().First(x=>x.name.StartsWith("Ground_Mountain"));return r.sharedMaterial;}
 static void Save(){var s=UnityEngine.SceneManagement.SceneManager.GetActiveScene();EditorSceneManager.MarkSceneDirty(s);EditorSceneManager.SaveScene(s);AssetDatabase.SaveAssets();}
 static void Refresh(MeshFilter mf){if(mf.TryGetComponent<MeshCollider>(out var mc)){mc.sharedMesh=null;mc.sharedMesh=mf.sharedMesh;}EditorUtility.SetDirty(mf.sharedMesh);Physics.SyncTransforms();}
 static RaceDirector Race=>Object.FindAnyObjectByType<RaceDirector>(FindObjectsInactive.Include);
 static string Path(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n.Replace('\n','/');}

 // ---------- routes, a spatial index of route samples, and the protection rule ----------
 sealed class R{public string n;public Func<float,Vector3> at;public Func<float,Vector3> fw;public float len;public Func<float,float> hw;public ReverseShortcutGuidance guide;}
 static List<R> routes;static Dictionary<(int,int),List<(int ri,float s,Vector3 p)>> index;
 static void LoadRoutes(){var road=Race.road;road.Initialize();routes=new List<R>{new(){n="Main",at=x=>road.At(x,out _),fw=x=>{road.At(x,out var f);return f;},len=road.Length,hw=x=>road.HalfWidth(x)}};
  foreach(var b in Object.FindObjectsByType<WoodlandRoute>().Where(b=>b.gameObject.activeInHierarchy)){b.Initialize();var bb=b;routes.Add(new(){n=b.title,at=x=>bb.At(x,out _),fw=x=>{bb.At(x,out var f);return f;},len=b.Length,hw=x=>bb.halfWidth,guide=b.GetComponent<ReverseShortcutGuidance>()});}
  index=new();for(int i=0;i<routes.Count;i++)for(float s=0;s<=routes[i].len;s+=1){var p=routes[i].at(s);var k=((int)Mathf.Floor(p.x/10),(int)Mathf.Floor(p.z/10));if(!index.TryGetValue(k,out var l))index[k]=l=new();l.Add((i,s,p));}
  edgeCache=new();}
 // Nearest route sample (horizontal) with |dy| < maxDy; returns route index or -1.
 static int Nearest(Vector3 p,float maxDy,out float s,out float d){int best=-1;s=0;d=float.MaxValue;int cx=(int)Mathf.Floor(p.x/10),cz=(int)Mathf.Floor(p.z/10);
  for(int i=-1;i<=1;i++)for(int j=-1;j<=1;j++)if(index.TryGetValue((cx+i,cz+j),out var l))foreach(var (ri,ss,q) in l){if(Mathf.Abs(q.y-p.y)>maxDy)continue;float dd=new Vector2(q.x-p.x,q.z-p.z).magnitude;if(dd<d){d=dd;best=ri;s=ss;}}
  if(best>=0){var r=routes[best];for(float t=Mathf.Max(0,s-1);t<=Mathf.Min(r.len,s+1);t+=.1f){var q=r.at(t);float dd=new Vector2(q.x-p.x,q.z-p.z).magnitude;if(dd<d){d=dd;s=t;}}}return best;}
 // Is p inside another route's corridor (another route, or a part of the same route more than 40 m away)?
 static string OtherCorridor(Vector3 p,int self,float selfS,float pad=1.5f){int cx=(int)Mathf.Floor(p.x/10),cz=(int)Mathf.Floor(p.z/10);
  for(int i=-1;i<=1;i++)for(int j=-1;j<=1;j++)if(index.TryGetValue((cx+i,cz+j),out var l))foreach(var (ri,ss,q) in l){if(ri==self&&Mathf.Abs(ss-selfS)<40)continue;float dy=p.y-q.y;if(dy<-1.5f||dy>6)continue;if(new Vector2(q.x-p.x,q.z-p.z).magnitude<routes[ri].hw(ss)+pad)return routes[ri].n;}return null;}
 static Dictionary<(int,int,int),(float w,float py,Vector3 e)> edgeCache;
 // Pavement edge: walked out from the centre line in 0.05 m steps (NaN where the centre is unpaved).
 static float EdgeW(int ri,float s,int side,out float py,out Vector3 e){var key=(ri,side,Mathf.RoundToInt(s*2));if(edgeCache.TryGetValue(key,out var c)){py=c.py;e=c.e;return c.w;}
  var r=routes[ri];var ctr=r.at(s);var right=Right(r,s)*side;e=ctr;float w=float.NaN;py=0;
  if(Surface(ctr.x,ctr.z,ctr.y+2f,IsDrive,out var ch,4f)){py=ch.point.y;w=0;for(;w<16;w+=.05f){if(!Pave(ctr+right*(w+.05f),py,out float ny))break;py=ny;}e=ctr+right*w;e.y=py;}
  edgeCache[key]=(w,py,e);return w;}
 static Vector3 Right(R r,float s)=>Vector3.Cross(Vector3.up,Vector3.ProjectOnPlane(r.fw(s),Vector3.up).normalized);
 // Protected: jump/flight systems (game exclusion zones, branch flight windows, whole flight systems from the approach,
 // activity jumps), multi-level
 // routes above/below, covered roads (tunnel/cut roof overhead), and the 0.68 berms.
 static string Protected(int ri,float s,Vector3 e,Vector3 outv){var r=routes[ri];
  foreach(var z in exclusions)if(z.Contains(e))return "J";
  if(r.guide&&r.guide.takeoff>=0&&s>=r.guide.takeoff-15&&s<=r.guide.landing+10)return "J";
  // Whole flight systems, approach included (5A.5): approach station -40 m to the end of the landing.
  if(r.n=="Main"&&flights!=null)foreach(var f in flights.flights)if(s>=f.approachStation-40&&s<=f.endStation)return "F";
  // Activity jumps (Fern Creek Leap, High Ridge Drop, ...): 40 m either side of the site along any route within 25 m.
  foreach(var a in jumps){var ap=a.transform.position;var q0=r.at(s);if(new Vector2(ap.x-q0.x,ap.z-q0.z).magnitude<40&&Mathf.Abs(ap.y-q0.y)<12)return "J";}
  var probe=e+outv*1.5f;int cx=(int)Mathf.Floor(probe.x/10),cz=(int)Mathf.Floor(probe.z/10);
  for(int i=-1;i<=1;i++)for(int j=-1;j<=1;j++)if(index.TryGetValue((cx+i,cz+j),out var l))foreach(var (oi,os,q) in l){if(oi==ri&&Mathf.Abs(os-s)<40)continue;float dy=Mathf.Abs(probe.y-q.y);if(dy>1.5f&&dy<40&&new Vector2(q.x-probe.x,q.z-probe.z).magnitude<routes[oi].hw(os)+3)return "M";}
  if(Physics.Raycast(e+Vector3.up*.3f,Vector3.up,out var up,10,~0,QueryTriggerInteraction.Ignore)&&Solid(up.collider))return "T";
  for(float d=.3f;d<=1.5f;d+=.4f)if(Surface(e.x+outv.x*d,e.z+outv.z*d,e.y+2.5f,Solid,out var bh,5)&&bh.collider.name.Contains("Report068 barrier"))return "B";
  return null;}
 static JumpRecoveryExclusion[] exclusions;static MountainFlights flights;static ActivitySite[] jumps;

 public static void Run(){
  if(Application.isPlaying)throw new Exception("Edit mode required");Directory.CreateDirectory("Docs/Report069");Physics.queriesHitBackfaces=false;
  var parts=(Environment.GetEnvironmentVariable("AUTHOR_PARTS")??"all").Split(',');bool Do(string p)=>parts.Contains("all")||parts.Contains(p);
  var scenes=(Environment.GetEnvironmentVariable("AUTHOR_SCENES")??"StreetLoopGreybox,StreetLoopReverse,LakeWoods,ForestLoopReverse,DansBackyardForward,DansBackyardReverse,MountainLoop,MountainLoopReverse").Split(',');
  try{
   foreach(var scene in scenes){
    EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();Note("SCENE "+scene);
    if(Do("signs")){RemoveSigns(t=>t.Contains("FENCE LINE SMASH"),"BUG-002");RemoveSigns(t=>t.Contains("ANDERSON'S")&&t.Contains("MOUNTAIN TRAILS"),"BUG-002");
     foreach(var t in Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include).Where(t=>t.text.Contains("BOTH TRAILS RETURN HOME")).ToArray())ReseatBoard(t.GetComponentInParent<PhysicalSign>(true).transform,"BUG-003");}
    if(scene.StartsWith("Mountain")){LoadRoutes();exclusions=Object.FindObjectsByType<JumpRecoveryExclusion>(FindObjectsInactive.Include);flights=Object.FindAnyObjectByType<MountainFlights>(FindObjectsInactive.Include);jumps=Object.FindObjectsByType<ActivitySite>(FindObjectsInactive.Include).Where(a=>a.kind==ActivitySite.Kind.Jump).ToArray();
     if(Do("outline"))SmoothOutline();
     if(scene=="MountainLoopReverse"&&Do("b006"))Patch("BUG-006 gore",new Vector3(729f,99.1f,-286.5f),9f);
     if(scene=="MountainLoop"&&Do("b007"))Patch("BUG-007 junction",new Vector3(761.5f,88f,-119.5f),8.5f);
     if(Do("shoulders"))Shoulders();
     if(Do("barriers"))Barriers();}
    Save();}
  }finally{File.AppendAllLines("Docs/Report069/author-notes.txt",notes);}
  EditorApplication.Exit(0);}

 // ---------- Part D ----------
 static void RemoveSigns(Func<string,bool> match,string label){foreach(var t in Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include).ToArray()){if(!t||!match(t.text))continue;var sign=t.GetComponentInParent<PhysicalSign>(true);if(!sign){Note($"{label}: '{t.text.Replace('\n','/')}' has no PhysicalSign root; left");continue;}Note($"{label}: removed sign '{t.text.Replace('\n','/')}' with post ({Path(sign.transform)}) at {sign.transform.position}");Object.DestroyImmediate(sign.gameObject);}}
 // BUG-003: the board must clear the hillside under its whole width; the post reaches from the ground to the board.
 static void ReseatBoard(Transform g,string label){var post=g.Find("Trail sign post");var board=g.Find("Trail sign board");if(!post||!board){Note(label+": sign parts missing in "+Path(g));return;}
  var br=board.GetComponent<Renderer>().bounds;var pp=post.position;bool ok(Collider c)=>Solid(c)&&c.transform!=post&&!c.transform.IsChildOf(g);
  if(!Surface(pp.x,pp.z,br.max.y+8,ok,out var ph,30)){Note(label+": no ground at the post");return;}
  float gmax=ph.point.y;var across=board.right;float half=board.lossyScale.x/2;for(int i=-4;i<=4;i++){var q=board.position+across*half*i/4f;if(Surface(q.x,q.z,br.max.y+8,ok,out var h,30))gmax=Mathf.Max(gmax,h.point.y);}
  float target=Mathf.Max(ph.point.y+1.5f,gmax+.4f),dy=target-br.min.y;if(dy<.05f){Note($"{label}: '{Path(g)}' already clear (board bottom {br.min.y:F2}, ground under board up to {gmax:F2})");return;}
  foreach(Transform ch in g)if(ch!=post)ch.position+=Vector3.up*dy;float top=br.center.y+dy,bottom=ph.point.y-.3f;post.position=new Vector3(pp.x,(top+bottom)/2,pp.z);var ls=post.localScale;ls.y=(top-bottom)/Mathf.Max(.0001f,post.parent.lossyScale.y);post.localScale=ls;
  Note($"{label}: '{Path(g)}' board raised {dy:F2} m: bottom now {target:F2} (post ground {ph.point.y:F2}, highest ground under the board {gmax:F2}); post {bottom:F2}..{top:F2}");}

 // ---------- Part A1: sawtooth pavement outline ----------
 // The pavement is a 0.5 m world-axis grid in places, so its outline steps where the road runs diagonally. Every
 // boundary vertex on a road side (not a lip, junction or protected system) moves sideways onto the local mean edge
 // line (+-1 m along the road); heights follow the cross slope. Centre line, width, grade and banking are unchanged.
 static void SmoothOutline(){
  foreach(var mc in Object.FindObjectsByType<MeshCollider>().Where(IsDrive).ToArray()){var mf=mc.GetComponent<MeshFilter>();var m=mc.sharedMesh;string path=AssetDatabase.GetAssetPath(m);
   if(!mf||mf.sharedMesh!=m||!path.StartsWith("Assets/")){Note($"outline: {mc.name} skipped (mesh {path})");continue;}
   var lv=m.vertices;var w=lv.Select(x=>mf.transform.TransformPoint(x)).ToArray();var key=w.Select(x=>(Mathf.RoundToInt(x.x*100),Mathf.RoundToInt(x.y*100),Mathf.RoundToInt(x.z*100))).ToArray();var t=m.triangles;
   var ec=new Dictionary<((int,int,int),(int,int,int)),int>();for(int i=0;i<t.Length;i+=3)for(int k=0;k<3;k++){var a=key[t[i+k]];var b=key[t[i+(k+1)%3]];var e=a.CompareTo(b)<0?(a,b):(b,a);ec[e]=ec.TryGetValue(e,out int n)?n+1:1;}
   var boundary=new HashSet<(int,int,int)>();foreach(var kv in ec)if(kv.Value==1){boundary.Add(kv.Key.Item1);boundary.Add(kv.Key.Item2);}
   var first=new Dictionary<(int,int,int),int>();for(int i=0;i<key.Length;i++)if(boundary.Contains(key[i])&&!first.ContainsKey(key[i]))first[key[i]]=i;
   var cand=new List<(int ri,int side,float s,float lat,(int,int,int) k)>();var why=new Dictionary<string,int>();void Why(string x){why[x]=why.TryGetValue(x,out int n)?n+1:1;}
   foreach(var kv in first){var p=w[kv.Value];int ri=Nearest(p,3,out float s,out _);if(ri<0){Why("no route");continue;}var r=routes[ri];var c=r.at(s);var right=Right(r,s);float lat=Vector3.Dot(p-c,right);int side=lat<0?-1:1;
    float W=EdgeW(ri,s,side,out _,out var e);if(float.IsNaN(W)){Why("centre unpaved");continue;}if(Mathf.Abs(Mathf.Abs(lat)-W)>.8f){Why("not a side edge");continue;}
    if(Pave(e+right*side*.9f,e.y,out _)||Pave(e+right*side*1.6f,e.y,out _)){Why("junction/notch");continue;}
    // Where the edge itself flares (junction mouths, deck ends, widening) it is not a stair: leave it.
    float wa=EdgeW(ri,s-2,side,out _,out _),wb=EdgeW(ri,s+2,side,out _,out _),wc=EdgeW(ri,s-1,side,out _,out _),wd=EdgeW(ri,s+1,side,out _,out _);
    if(float.IsNaN(wa)||float.IsNaN(wb)||float.IsNaN(wc)||float.IsNaN(wd)||Mathf.Abs(wa-wb)>.6f||Mathf.Abs(wc-wd)>.6f){Why("flaring edge");continue;}
    var pr=Protected(ri,s,e,right*side);if(pr!=null){Why("protected "+pr);continue;}cand.Add((ri,side,s,Mathf.Abs(lat),kv.Key));}
   var move=new Dictionary<(int,int,int),Vector3>();float worst=0;
   // Smooth edge line: local least-squares line of the boundary laterals over +-1.25 m along the road, iterated 3 times.
   foreach(var g in cand.GroupBy(x=>(x.ri,x.side))){var L=g.OrderBy(x=>x.s).ToArray();var cur=L.Select(x=>x.lat).ToArray();
    for(int it=0;it<3;it++){var nxt=new float[L.Length];int lo=0;for(int i=0;i<L.Length;i++){while(L[lo].s<L[i].s-1.25f)lo++;double n=0,sx=0,sy=0,sxx=0,sxy=0;
      for(int j=lo;j<L.Length&&L[j].s<=L[i].s+1.25f;j++){double x=L[j].s-L[i].s;n++;sx+=x;sy+=cur[j];sxx+=x*x;sxy+=x*cur[j];}
      double den=n*sxx-sx*sx;nxt[i]=(float)(Math.Abs(den)<1e-6?sy/n:(sy*sxx-sx*sxy)/den);}cur=nxt;}
    for(int i=0;i<L.Length;i++){float d=cur[i]-L[i].lat;if(Mathf.Abs(d)<.015f||Mathf.Abs(d)>.4f)continue;
     var r=routes[L[i].ri];var right=Right(r,L[i].s)*L[i].side;var c=r.at(L[i].s);var p=w[first[L[i].k]];float slope=0;
     if(Pave(c+right*(L[i].lat-.6f),p.y,out float y1)&&Pave(c+right*(L[i].lat-1.6f),p.y,out float y2))slope=Mathf.Clamp(y1-y2,-.3f,.3f);
     move[L[i].k]=p+right*d+Vector3.up*slope*d;worst=Mathf.Max(worst,Mathf.Abs(d));}}
   // Never flip or collapse a triangle: revert moves that would.
   int reverted=0;for(int pass=0;pass<4;pass++){var bad=new HashSet<(int,int,int)>();
    for(int i=0;i<t.Length;i+=3){var k0=key[t[i]];var k1=key[t[i+1]];var k2=key[t[i+2]];if(!move.ContainsKey(k0)&&!move.ContainsKey(k1)&&!move.ContainsKey(k2))continue;
     Vector3 P(int idx,(int,int,int) k)=>move.TryGetValue(k,out var q)?q:w[idx];var n0=Vector3.Cross(w[t[i+1]]-w[t[i]],w[t[i+2]]-w[t[i]]);var n1=Vector3.Cross(P(t[i+1],k1)-P(t[i],k0),P(t[i+2],k2)-P(t[i],k0));
     if(n0.sqrMagnitude<1e-10f)continue;if(Vector3.Dot(n0,n1)<=0||n1.magnitude<n0.magnitude*.15f){foreach(var k in new[]{k0,k1,k2})if(move.ContainsKey(k))bad.Add(k);}}
    if(bad.Count==0)break;foreach(var k in bad){move.Remove(k);reverted++;}}
   if(move.Count==0){Note($"outline {Scene} {mc.name} ({path}): boundary vertices {first.Count}; nothing to move ({string.Join(", ",why.Select(x=>x.Key+" "+x.Value))})");continue;}
   // Do no harm: the edge straightness (deviation of the walked edge from its local straight line, +-1.5 m) is measured
   // before and after at every 0.5 m station near a moved vertex; moves within 1.5 m of a station that got worse are undone.
   var at=cand.ToDictionary(x=>x.k,x=>(x.ri,x.side,x.s));var orig=(Vector3[])lv.Clone();m=Own(mf);
   float Resid(int ri,int side,float s){float w0=EdgeW(ri,s,side,out _,out _);if(float.IsNaN(w0))return 0;double n=0,sx=0,sy=0,sxx=0,sxy=0;
    for(float d=-1.5f;d<=1.51f;d+=.5f){float w=EdgeW(ri,s+d,side,out _,out _);if(float.IsNaN(w))continue;n++;sx+=d;sy+=w;sxx+=d*d;sxy+=d*w;}double den=n*sxx-sx*sx;if(n<4)return 0;return Mathf.Abs(w0-(float)(Math.Abs(den)<1e-9?sy/n:(sy*sxx-sx*sxy)/den));}
   var stations=new HashSet<(int,int,int)>();foreach(var k in move.Keys){var (ri,side,s)=at[k];for(float d=-1.5f;d<=1.51f;d+=.5f)stations.Add((ri,side,Mathf.RoundToInt((s+d)*2)));}
   var before=stations.ToDictionary(x=>x,x=>Resid(x.Item1,x.Item2,x.Item3/2f));int undone=0;
   for(int pass=0;pass<4;pass++){for(int i=0;i<lv.Length;i++)lv[i]=move.TryGetValue(key[i],out var q)?mf.transform.InverseTransformPoint(q):orig[i];
    m.vertices=lv;m.RecalculateBounds();Refresh(mf);edgeCache.Clear();if(pass==3)break;
    var worse=stations.Where(x=>Resid(x.Item1,x.Item2,x.Item3/2f)>Mathf.Max(before[x],.05f)+.02f).ToList();if(worse.Count==0)break;
    foreach(var k in move.Keys.ToArray()){var (ri,side,s)=at[k];if(worse.Any(ww=>ww.Item1==ri&&ww.Item2==side&&Mathf.Abs(ww.Item3/2f-s)<=1.5f)){move.Remove(k);undone++;}}}
   edgeCache.Clear();
   Note($"A1 outline {Scene} {mc.name} ({path}): boundary vertices {first.Count}, side-edge candidates {cand.Count}, moved {move.Count} onto the smooth edge line (max {worst:F2} m; {reverted} reverted to avoid flipped triangles, {undone} undone where the edge did not get straighter); skipped: {string.Join(", ",why.Select(x=>x.Key+" "+x.Value))}");}}

 // ---------- Part A2: flush shoulders ----------
 // Where the ground beside an open pavement edge is not flush (0.67 shoulders start 0.12 m under the edge and fall at
 // 37 deg), a collidable shoulder starts 0.3 m under the pavement, meets the edge flush, runs 1 m out at -4%, then
 // follows 1:2 (then 1:1.33, 1:0.7 below 4 m / 8 m) to the existing ground, or rises 1:2 to meet a bank. Terrain
 // left above the new verge is lowered under it. Jump/flight systems, multi-level routes, covered roads, junctions,
 // natural barriers/berms, other routes' corridors and the flight corridors are left as they are.
 // Trees are batched visuals with separate trunk colliders and cannot be moved: where a shoulder would bury a trunk
 // base more than 1 m, that station falls 1:0.7 straight after the verge; if that still buries it, it is left as it was.
 static void Shoulders(){var log=new List<string>();var skip=new Dictionary<string,int>();void Skip(string x){skip[x]=skip.TryGetValue(x,out int n)?n+1:1;}
  const int K=40;
  bool FlightCorridor(Vector3 p){if(flights==null)return false;foreach(var f in flights.flights){var ax=Vector3.ProjectOnPlane(f.landingEnd-f.start,Vector3.up).normalized;float along=Vector3.Dot(p-f.start,ax),tot=Vector3.Dot(f.landingEnd-f.start,ax),lat=Mathf.Abs(Vector3.Dot(p-f.start,Vector3.Cross(Vector3.up,ax)));if(along>-5&&along<tot+5&&lat<26)return true;}return false;}
  // One station's cross-section (null + reason where nothing is built). level 0 normal, 1 steep below the verge, 2 none.
  List<Vector3> Profile(int ri,int side,float s,int level,out string reason){var r=routes[ri];reason=null;
   float W=EdgeW(ri,s,side,out float py,out var e);var outv=Right(r,s)*side;
   if(float.IsNaN(W)){reason="unpaved";return null;}
   if(Pave(e+outv*.6f,py,out _)||Pave(e+outv*1.4f,py,out _)){reason="junction";return null;}
   if(Surface(e.x+outv.x*.6f,e.z+outv.z*.6f,py+2,c=>c.name.StartsWith("Ground_Report069 patch"),out _,5)){reason="patch";return null;}
   reason=Protected(ri,s,e,outv);if(reason!=null)return null;
   if(level>=2){reason="tree";return null;}
   RaycastHit gh;float G(float d)=>Ground(e+outv*d,py+6f,44,out gh)?gh.point.y-py:float.NaN;
   float g03=G(.3f),g10=G(1f);for(float bd=.3f;bd<=1.5f;bd+=.4f)if(Ground(e+outv*bd,py+2.5f,6,out var bh)&&IsBarrier(bh.collider)){reason="barrier";return null;}
   if(!float.IsNaN(g03)&&g03>1.5f){reason="wall";return null;}
   if(!float.IsNaN(g03)&&Mathf.Abs(g03)<=.06f&&!float.IsNaN(g10)&&Mathf.Abs(g10)<=.12f){reason="flush";return null;}
   float Grade(float nd)=>level==1?1.4f:nd<=4?.5f:nd<=8?.75f:1.4f;
   var pts=new List<Vector3>();var p0=e-outv*.3f;p0.y=py-.10f;pts.Add(p0);float y=py-.012f,d=0;bool met=false;
   for(;d<=1.001f;d+=.5f){var q=e+outv*d;q.y=py-.012f-.038f*d;pts.Add(q);y=q.y;}d=1f;
   for(int k=0;k<40&&!met;k++){float nd=d+.5f;float gd=G(nd);float ny;
    if(float.IsNaN(gd))ny=y-.5f*Grade(nd);else{float gy=py+gd;ny=gy<y?Mathf.Max(gy,y-.5f*Grade(nd)):Mathf.Min(gy,y+.25f);if(Mathf.Abs(ny-gy)<.005f){ny=gy-.03f;met=true;}}
    var q=e+outv*nd;q.y=ny;pts.Add(q);d=nd;y=ny;if(nd>=16)break;}
   if(!met){var last=pts[^1];if(Ground(last,last.y+.5f,80,out var dh))pts.Add(new Vector3(last.x,dh.point.y-.05f,last.z));}
   foreach(var q in pts){var oc=OtherCorridor(q,ri,s);if(oc!=null){reason="corridor "+oc;return null;}if(FlightCorridor(q)){reason="flight corridor";return null;}}
   while(pts.Count<K)pts.Add(pts[^1]);if(pts.Count>K)pts.RemoveRange(K,pts.Count-K);return pts;}
  var st=new List<(int ri,int side,float s,int level,List<Vector3> pts,string reason)>();
  for(int ri=0;ri<routes.Count;ri++)foreach(int side in new[]{-1,1})for(float s=0;s<=routes[ri].len;s+=.5f){var pts=Profile(ri,side,s,0,out var why);st.Add((ri,side,s,0,pts,why));}
  var trunks=Object.FindObjectsByType<Collider>().Where(c=>!c.isTrigger&&c.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0&&c.bounds.size.x<8&&c.bounds.size.z<8).ToArray();
  GameObject g=null;MeshCollider gc=null;int buried=0;
  for(int pass=0;pass<3;pass++){
   var v=new List<Vector3>();var col=new List<Color>();var tri=new List<int>();
   for(int i=1;i<st.Count;i++){var a=st[i-1];var b=st[i];if(a.pts==null||b.pts==null||a.ri!=b.ri||a.side!=b.side||b.s-a.s>.51f)continue;
    for(int k=0;k+1<K;k++){Tri(v,col,tri,a.pts[k],a.pts[k+1],b.pts[k+1],Vector3.up);Tri(v,col,tri,a.pts[k],b.pts[k+1],b.pts[k],Vector3.up);}}
   if(tri.Count==0){Note($"A2 {Scene}: no shoulder needed");return;}
   for(int i=0;i<col.Count;i++)col[i]=col[i]==Rock?Rock:Earth;
   g=Make("flush shoulders",Store("flush-shoulders",Build(v,col,tri)),GroundMaterial());gc=g.GetComponent<MeshCollider>();
   var hit=new List<Vector3>();foreach(var c in trunks){var bb=c.bounds;if(gc.Raycast(new Ray(new Vector3(bb.center.x,bb.max.y+40,bb.center.z),Vector3.down),out var h,120)&&h.point.y>bb.min.y+1f&&h.point.y<bb.max.y)hit.Add(new Vector3(bb.center.x,0,bb.center.z));}
   buried=hit.Count;if(hit.Count==0||pass==2)break;int changed=0;
   for(int i=0;i<st.Count;i++){var x=st[i];if(x.pts==null||x.level>=2)continue;bool near=false;foreach(var p in x.pts)foreach(var t in hit)if(new Vector2(p.x-t.x,p.z-t.z).sqrMagnitude<4){near=true;break;}
    if(!near)continue;int lv=x.level+1;var pts=Profile(x.ri,x.side,x.s,lv,out var why);st[i]=(x.ri,x.side,x.s,lv,pts,why);changed++;}
   Note($"A2 {Scene} pass {pass+1}: {hit.Count} tree trunks would stand more than 1 m inside the shoulder; {changed} stations made steeper/left as they were");}
  int built=0,flush=0;float runStart=-1,prevS=-9;int pr=-1,ps=0;
  foreach(var x in st){if(x.pts==null){Skip(x.reason??"?");if(x.reason=="flush")flush++;if(pr>=0)log.Add($"{routes[pr].n} {(ps<0?"L":"R")} s {runStart:F1}-{prevS:F1}");pr=-1;continue;}
   built++;if(pr!=x.ri||ps!=x.side||x.s-prevS>.51f){if(pr>=0)log.Add($"{routes[pr].n} {(ps<0?"L":"R")} s {runStart:F1}-{prevS:F1}");runStart=x.s;}pr=x.ri;ps=x.side;prevS=x.s;}
  if(pr>=0)log.Add($"{routes[pr].n} {(ps<0?"L":"R")} s {runStart:F1}-{prevS:F1}");
  int steep=st.Count(x=>x.level==1&&x.pts!=null);
  File.WriteAllLines($"Docs/Report069/shoulders-{Scene}.txt",log.Prepend($"{Scene}: stations with a new flush shoulder {built} (0.5 m; {steep} steep beside trees); already flush {flush}; skipped: {string.Join(", ",skip.OrderByDescending(x=>x.Value).Select(x=>x.Key+" "+x.Value))}"));
  // Terrain is lowered only under the 1 m verge (beyond it the shoulder ends where it meets the ground, so nothing stands
  // above it there); lowering wider would leave dents at run ends.
  var vv=new List<Vector3>();var vc=new List<Color>();var vt=new List<int>();
  for(int i=1;i<st.Count;i++){var a=st[i-1];var b=st[i];if(a.pts==null||b.pts==null||a.ri!=b.ri||a.side!=b.side||b.s-a.s>.51f)continue;for(int k=0;k<3;k++){Tri(vv,vc,vt,a.pts[k],a.pts[k+1],b.pts[k+1],Vector3.up);Tri(vv,vc,vt,a.pts[k],b.pts[k+1],b.pts[k],Vector3.up);}}
  var verge=new GameObject("Report069 verge (temporary)",typeof(MeshCollider));var vmesh=Build(vv,vc,vt);verge.GetComponent<MeshCollider>().sharedMesh=vmesh;Physics.SyncTransforms();
  int lowered=LowerUnder(verge.GetComponent<MeshCollider>(),1.6f,"A2");Object.DestroyImmediate(verge);Object.DestroyImmediate(vmesh);Physics.SyncTransforms();
  Note($"A2 flush shoulders {Scene}: {built} stations (0.5 m) in {log.Count} runs ({steep} steeper beside trees; trunks still >1 m inside: {buried}); already flush {flush}; terrain vertices lowered under the new verge {lowered}; skipped {string.Join(", ",skip.OrderByDescending(x=>x.Value).Select(x=>x.Key+" "+x.Value))} (runs in shoulders-{Scene}.txt)");
  Dependents(gc,"A2");}
 // Terrain (not pavement, barriers, roofs or this round's meshes) whose vertices stand above a new surface inside its
 // footprint (up to 'reach' above it) is lowered 0.1 m under it, never toward a road below (5A.4).
 static int LowerUnder(MeshCollider top,float reach,string label,Vector3 centre=default,float radius=0){int n=0;var tb=top.bounds;
  foreach(var mf in Object.FindObjectsByType<MeshFilter>().Where(x=>x.sharedMesh&&((x.TryGetComponent<MeshCollider>(out var c)&&IsTerrain(c)&&!IsDrive(c)&&!IsBarrier(c)&&!IsRoof(c))||x.name=="Ground_Report067 edge seams")&&!x.name.StartsWith("Ground_Report069")&&x.GetComponent<Renderer>()&&x.GetComponent<Renderer>().bounds.Intersects(tb)).ToArray()){
   var m=mf.sharedMesh;var path=AssetDatabase.GetAssetPath(m);var lv=m.vertices;bool any=false;
   for(int i=0;i<lv.Length;i++){var w=mf.transform.TransformPoint(lv[i]);if(w.x<tb.min.x||w.x>tb.max.x||w.z<tb.min.z||w.z>tb.max.z)continue;
    if(radius>0&&new Vector2(w.x-centre.x,w.z-centre.z).magnitude>radius)continue;
    if(!top.Raycast(new Ray(new Vector3(w.x,w.y+reach+.5f,w.z),Vector3.down),out var h,reach+40))continue;float target=h.point.y-.1f;if(w.y<=target+.04f||w.y>h.point.y+reach)continue;
    if(Physics.Raycast(new Vector3(w.x,target-.05f,w.z),Vector3.down,out var below,6,~0,QueryTriggerInteraction.Ignore)&&IsDrive(below.collider))continue;
    w.y=target;lv[i]=mf.transform.InverseTransformPoint(w);n++;any=true;}
   if(any){if(!path.StartsWith("Assets/")){Note($"{label}: {mf.name} mesh is not a project asset ({path}); vertices left");continue;}m=Own(mf);m.vertices=lv;m.RecalculateNormals();m.RecalculateBounds();Refresh(mf);}}
  return n;}
 // A mesh asset also used by another scene is cloned for this scene before it is edited (the other scene is unchanged).
 static Dictionary<string,HashSet<string>> deps;
 static Mesh Own(MeshFilter mf){var m=mf.sharedMesh;var path=AssetDatabase.GetAssetPath(m);
  if(deps==null){deps=new();foreach(var sc in AssetDatabase.FindAssets("t:Scene",new[]{"Assets/Scenes"}).Select(AssetDatabase.GUIDToAssetPath))deps[System.IO.Path.GetFileNameWithoutExtension(sc)]=new HashSet<string>(AssetDatabase.GetDependencies(sc,false));}
  var users=deps.Where(kv=>kv.Key!=Scene&&kv.Value.Contains(path)).Select(kv=>kv.Key).ToArray();if(users.Length==0)return m;
  var copy=Object.Instantiate(m);copy.name=m.name;var own=Store("own-"+System.IO.Path.GetFileNameWithoutExtension(path),copy);mf.sharedMesh=own;if(mf.TryGetComponent<MeshCollider>(out var mc)&&mc.sharedMesh==m)mc.sharedMesh=own;
  Note($"{mf.name}: mesh {path} is shared with {string.Join(",",users)}; this scene now uses its own copy {AssetDatabase.GetAssetPath(own)}");return own;}
 // Grounded objects inside a new surface's footprint: posts/signs that would be buried are raised onto it, trees noted.
 static void Dependents(MeshCollider mc,string label){int signs=0,trees=0;
  foreach(var c in Object.FindObjectsByType<Collider>().Where(x=>x!=mc&&!x.isTrigger&&!x.attachedRigidbody).ToArray()){var b=c.bounds;if(b.size.x>8||b.size.z>8)continue;
   bool trunk=c.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0;var sign=c.GetComponentInParent<PhysicalSign>();bool post=c.name.IndexOf("post",StringComparison.OrdinalIgnoreCase)>=0;if(!trunk&&!sign&&!post)continue;
   if(!mc.Raycast(new Ray(new Vector3(b.center.x,b.max.y+40,b.center.z),Vector3.down),out var h,120)||h.point.y<b.min.y+.25f||h.point.y>b.max.y)continue;
   if(trunk&&!sign){trees++;Note($"{label}: tree trunk '{Path(c.transform)}' base {h.point.y-b.min.y:F2} m inside the new surface (tree stands on it)");continue;}
   var root=sign?sign.transform:c.transform.parent?c.transform.parent:c.transform;float dy=h.point.y-.2f-b.min.y;root.position+=Vector3.up*dy;signs++;Note($"{label}: raised sign/post '{Path(root)}' {dy:F2} m onto the new surface");}
  Note($"{label}: signs/posts raised {signs}; tree bases inside the new surface {trees}");}

 // ---------- BUG-006 / BUG-007: one smooth surface where lumpy/torn terrain sheets meet the road ----------
 // 0.5 m grid in a circle: cells over pavement that border open ground are pinned 3 cm under the pavement (flush,
 // tucked under its edge); the rim is pinned 2 cm under the existing ground; everything between is the smooth
 // (harmonic) surface joining them. Terrain sheets above it inside the circle are lowered under it. Collidable.
 static void Patch(string label,Vector3 c,float R){var mat=Surface(c.x,c.z,c.y+3,x=>IsTerrain(x)&&!IsDrive(x),out var mh,8)&&mh.collider.GetComponent<Renderer>()?mh.collider.GetComponent<Renderer>().sharedMaterial:GroundMaterial();
  int N=Mathf.CeilToInt(R/.5f);int S=2*N+1;var H=new float[S,S];var kind=new int[S,S];// 0 out,1 pave-pinned,2 rim-pinned,3 free,4 deep pavement
  for(int i=0;i<S;i++)for(int j=0;j<S;j++){float x=c.x+(i-N)*.5f,z=c.z+(j-N)*.5f;float r=new Vector2(x-c.x,z-c.z).magnitude;if(r>R){kind[i,j]=0;continue;}
   if(Surface(x,z,c.y+3,Solid,out var top,7)&&IsDrive(top.collider)){kind[i,j]=4;H[i,j]=top.point.y-.03f;continue;}
   if(!Ground(new Vector3(x,0,z),c.y+3,8,out var gh)||IsBarrier(gh.collider)){kind[i,j]=0;continue;}
   H[i,j]=gh.point.y;kind[i,j]=r>R-.75f?2:3;if(kind[i,j]==2)H[i,j]-=.02f;}
  for(int i=0;i<S;i++)for(int j=0;j<S;j++)if(kind[i,j]==4){bool edge=false;for(int a=-1;a<=1;a++)for(int b=-1;b<=1;b++){int ii=i+a,jj=j+b;if(ii>=0&&jj>=0&&ii<S&&jj<S&&(kind[ii,jj]==2||kind[ii,jj]==3))edge=true;}if(edge)kind[i,j]=1;}
  for(int it=0;it<1500;it++){var nh=(float[,])H.Clone();for(int i=0;i<S;i++)for(int j=0;j<S;j++){if(kind[i,j]!=3)continue;float sum=0;int n=0;foreach(var (a,b) in new[]{(1,0),(-1,0),(0,1),(0,-1)}){int ii=i+a,jj=j+b;if(ii<0||jj<0||ii>=S||jj>=S)continue;var k=kind[ii,jj];if(k==1||k==2||k==3){sum+=H[ii,jj];n++;}}if(n>0)nh[i,j]=sum/n;}H=nh;}
  var v=new List<Vector3>();var col=new List<Color>();var tri=new List<int>();bool In(int i,int j)=>kind[i,j]==1||kind[i,j]==2||kind[i,j]==3;
  Vector3 P(int i,int j)=>new(c.x+(i-N)*.5f,H[i,j],c.z+(j-N)*.5f);
  for(int i=0;i+1<S;i++)for(int j=0;j+1<S;j++){if(!In(i,j)||!In(i+1,j)||!In(i,j+1)||!In(i+1,j+1))continue;if(kind[i,j]==1&&kind[i+1,j]==1&&kind[i,j+1]==1&&kind[i+1,j+1]==1)continue;
   Tri(v,col,tri,P(i,j),P(i+1,j),P(i+1,j+1),Vector3.up);Tri(v,col,tri,P(i,j),P(i+1,j+1),P(i,j+1),Vector3.up);}
  for(int i=0;i<col.Count;i++)col[i]=Earth;
  var cellsFree=0;foreach(var k in kind)if(k==3)cellsFree++;
  var g=Make("patch "+label,Store("patch-"+label.Split(' ')[0].ToLower(),Build(v,col,tri)),mat);var gc=g.GetComponent<MeshCollider>();
  int lowered=LowerUnder(gc,3f,label,c,R-1.25f);
  Note($"{label}: smooth collidable surface r {R} m around {c} ({cellsFree} free 0.5 m cells, {tri.Count/3} tris, material {mat.name}); pinned flush 3 cm under the pavement edge and 2 cm under the ground at the rim; terrain vertices lowered under it {lowered}");
  Dependents(gc,label);}

 // ---------- Part B: berms at the two crest-then-bend places (0.68 Part E design) ----------
 // Earth berm on the OUTSIDE of the bend: inner toe 0.6 m outside the pavement edge, 85 deg rock inner face 1.8 m above
 // the pavement edge, 0.8 m crown, outer face down to the ground; ends taper over 8 m. Stations whose berm would enter
 // another route's corridor (the Summit Traverse entrance) are left open and logged.
 static void Barriers(){var sites=Scene=="MountainLoop"?new[]{("Climbing Ridge Cut rejoin (BUG-008)","Main",1390f,1434f,1)}:new[]{("Summit crest bend (BUG-005)","Main",446f,486f,1)};var log=new List<string>{"SCENE "+Scene};
  foreach(var (name,route,s0,s1,side) in sites){int ri=routes.FindIndex(x=>x.n==route);var r=routes[ri];var v=new List<Vector3>();var c=new List<Color>();var t=new List<int>();List<Vector3> prev=null;int made=0;
   for(float s=s0;s<=s1;s+=1){var outv=Right(r,s)*side;float W=EdgeW(ri,s,side,out float py,out var e);if(float.IsNaN(W)){prev=null;log.Add($"{name} s {s}: no pavement");continue;}
    float taper=Mathf.Clamp01(Mathf.Min(s-s0,s1-s)/8f);float hgt=1.8f*Mathf.Max(.15f,taper);var b0=e+outv*.6f;
    float gIn=Ground(b0,py+2,20,out var gh)?gh.point.y:float.NegativeInfinity;if(!float.IsNegativeInfinity(gIn)&&gIn>py+1.2f){prev=null;log.Add($"{name} s {s}: natural bank already {gIn-py:F1} m high; no berm");continue;}
    var p0=b0;p0.y=(float.IsNegativeInfinity(gIn)?py:Mathf.Min(py,Mathf.Max(gIn,py-.6f)))-.05f;var p1=e+outv*(.6f+hgt/Mathf.Tan(85*Mathf.Deg2Rad));p1.y=py+hgt;var p2=p1+outv*.8f;var pts=new List<Vector3>{p0,p1,p2};
    for(float d=1;d<=10;d+=1){var q=p2+outv*d;q.y=p2.y-d;float gq=Ground(q,p2.y+1,30,out var g2)?g2.point.y:float.NegativeInfinity;if(float.IsNegativeInfinity(gq)){pts.Add(q);continue;}if(q.y<=gq){q.y=gq-.1f;pts.Add(q);break;}pts.Add(q);}
    var end=pts[^1];if(Ground(end,end.y+1,80,out var eh)&&eh.point.y<end.y-.05f)pts.Add(new Vector3(end.x,eh.point.y-.1f,end.z));
    string clash=null;foreach(var p in pts){clash=OtherCorridor(p,ri,s);if(clash!=null)break;}if(clash!=null){prev=null;log.Add($"{name} s {s}: would enter the {clash} corridor; left open");continue;}
    while(pts.Count<16)pts.Add(pts[^1]);
    if(prev!=null)for(int k=0;k+1<pts.Count;k++){Tri(v,c,t,prev[k],prev[k+1],pts[k+1],Vector3.up+outv*.01f);Tri(v,c,t,prev[k],pts[k+1],pts[k],Vector3.up+outv*.01f);}
    prev=pts;made++;}
   if(t.Count==0){Note($"Part B {name}: nothing built");continue;}
   var g=Make("barrier "+name,Store("barrier-"+name.Split(' ')[0].ToLower(),Build(v,c,t)),GroundMaterial());
   Note($"Part B {Scene} {name}: earth berm on the {(side>0?"right":"left")} (outside) of {route} s {s0}-{s1}: {made} stations, {t.Count/3} tris, 1.8 m above the pavement edge, 85 deg inner face");
   Dependents(g.GetComponent<MeshCollider>(),"Part B "+name);}
  File.AppendAllLines("Docs/Report069/partB-barriers-log.txt",log);}
}
