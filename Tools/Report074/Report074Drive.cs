using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using Racer;using Object=UnityEngine.Object;
// 0.74 BUG-003: the House 3 driveway. The straight driveway (0.30) drops 48 m in about 80 m through a slot cut into the
// valley wall (a 60-degree lip at the top, then ~29 degrees). The original winding driveway (DiscoveryCorrections, still
// the driveway in Street Loop Reverse, Laurel scene, untouched) runs along the natural slope and its ground is intact in
// every other scene. This part:
//  1. fills the 0.30 slot back to the hillside (each slot vertex goes up to the line between the ground 11.5 m either side,
//     blended exactly like 0.30 cut it: full within 4 m of the straight line, nothing beyond 10 m; never within 15 m of an
//     authored road or branch, never lowered);
//  2. lays the driveway on the original winding line again: the centre profile is the ground along it, smoothed over
//     +-8 m (rounded top and bottom), the terrain within 9 m is fitted to that profile, and a 7 m gravel surface follows it;
//  3. removes the straight driveway surface and points the "House 3 valley driveway" road at the winding line;
//  4. re-grounds what rests on the changed ground (fences, trees, props) and lists anything standing in the new driving width.
// Forest Loop Reverse: the Forest Reverse main route's gap-jump embankment covers the original line between s 81 and 114,
// so there the driveway crosses that trail at grade further east and comes down the embankment's south flank diagonally;
// nothing within 15 m of the trail is reshaped and no gravel is laid on the trail.
public static partial class Report074Author {
 static readonly string[] DriveScenes={"StreetLoopGreybox","LakeWoods","ForestLoopReverse","DansBackyardForward","DansBackyardReverse","MountainLoop","MountainLoopReverse"};
 const string RibbonName="Ground_House3 gentle winding driveway";
 static Collider skipGround;
 static float GroundT(float x,float z){foreach(var h in Physics.RaycastAll(new Vector3(x,300,z),Vector3.down,500,~0,QueryTriggerInteraction.Ignore).OrderBy(h=>h.distance)){var c=h.collider;if(c.isTrigger||c.attachedRigidbody||!c.enabled)continue;var n=c.name;
   if(n.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0||n.IndexOf("tree",StringComparison.OrdinalIgnoreCase)>=0)continue;if(!n.StartsWith("Ground"))continue;if(c==skipGround||n==RibbonName)continue;return h.point.y;}return float.NaN;}
 static float SegDist(Vector2 p,Vector2 a,Vector2 b){var ab=b-a;float t=ab.sqrMagnitude<1e-6f?0:Mathf.Clamp01(Vector2.Dot(p-a,ab)/ab.sqrMagnitude);return (a+ab*t-p).magnitude;}
 static List<Vector3> CatmullResample(List<Vector3> pts,float step){var dense=new List<Vector3>();for(int i=0;i+1<pts.Count;i++){var p0=pts[Mathf.Max(0,i-1)];var p1=pts[i];var p2=pts[i+1];var p3=pts[Mathf.Min(pts.Count-1,i+2)];
   int n=Mathf.Max(2,Mathf.CeilToInt(Vector3.Distance(p1,p2)/.25f));for(int k=0;k<n;k++){float t=k/(float)n,t2=t*t,t3=t2*t;dense.Add(.5f*((2*p1)+(-p0+p2)*t+(2*p0-5*p1+4*p2-p3)*t2+(-p0+3*p1-3*p2+p3)*t3));}}dense.Add(pts[^1]);
  var res=new List<Vector3>{dense[0]};float acc=0;for(int i=1;i<dense.Count;i++){var a=new Vector2(dense[i-1].x,dense[i-1].z);var b=new Vector2(dense[i].x,dense[i].z);acc+=(b-a).magnitude;if(acc>=step){res.Add(dense[i]);acc=0;}}
  if(Vector3.Distance(res[^1],dense[^1])>.5f)res.Add(dense[^1]);else res[^1]=dense[^1];return res;}

 static float GroundOld(Renderer[] rs,Dictionary<Renderer,float> old){var lowest=rs.OrderBy(r=>r.bounds.min.y).First();return old[lowest];}
 static void Bug003Drive(bool dry){
  if(!DriveScenes.Contains(Scene)){Note($"BUG-003 {Scene}: not changed (this scene keeps the original winding driveway; its straightening was never applied because of the protected Laurel route)");return;}
  var drive=Object.FindObjectsByType<RaceRoad>(FindObjectsInactive.Include,FindObjectsSortMode.None).FirstOrDefault(r=>r.name=="House 3 valley driveway");
  var straight=drive?drive.transform.Find("Ground_House3 straight descending property driveway"):null;
  if(!straight){Note($"BUG-003 {Scene}: straight driveway not found; not changed");return;}
  var pad=drive.transform.Find("Ground_House3 supported valley driveway");var mat=pad.GetComponent<MeshRenderer>().sharedMaterial;
  var sCol=straight.GetComponent<MeshCollider>();
  // ---- before: steepest grade along the straight driveway (its own collider, 2 m windows) ----
  var a0=drive.points[0];var a1=drive.points[^1];var axis=a1-a0;axis.y=0;float L=axis.magnitude;var fwd=axis/L;var right=Vector3.Cross(Vector3.up,fwd);
  float SurfY(Collider c,float x,float z)=>c.Raycast(new Ray(new Vector3(x,300,z),Vector3.down),out var hh,500)?hh.point.y:float.NaN;
  float steepBefore=0,steepAt=0;for(float s=0;s+2<=L;s+=.5f){var p=a0+fwd*s;var q=a0+fwd*(s+2);float y0=SurfY(sCol,p.x,p.z),y1=SurfY(sCol,q.x,q.z);if(float.IsNaN(y0)||float.IsNaN(y1))continue;float gg=Mathf.Abs(y1-y0)/2;if(gg>steepBefore){steepBefore=gg;steepAt=s;}}
  Note($"BUG-003 {Scene}: straight driveway {a0:F1} -> {a1:F1}, {L:F1} m plan, drop {a0.y-a1.y:F1} m; steepest 2 m grade {steepBefore*100:F0}% ({Mathf.Atan(steepBefore)*Mathf.Rad2Deg:F0} deg) at s {steepAt:F1}");
  // ---- protected routes: every other road and branch ----
  var bbox=new Rect(340,-262,200,150);
  var prot=new List<(Vector2 a,Vector2 b,string n)>();
  void AddLine(Vector3[] pts,string n){if(pts==null)return;for(int i=0;i+1<pts.Length;i++){var a=new Vector2(pts[i].x,pts[i].z);var b=new Vector2(pts[i+1].x,pts[i+1].z);if(!bbox.Contains(a)&&!bbox.Contains(b))continue;prot.Add((a,b,n));}}
  foreach(var r in Object.FindObjectsByType<RaceRoad>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(r!=drive)AddLine(r.points,r.name);
  foreach(var w in Object.FindObjectsByType<WoodlandRoute>(FindObjectsInactive.Include,FindObjectsSortMode.None))AddLine(w.points,"W:"+w.title);
  float Prot(float x,float z){var p=new Vector2(x,z);float d=1e9f;foreach(var s in prot)d=Mathf.Min(d,SegDist(p,s.a,s.b));return d;}
  string ProtName(float x,float z){var p=new Vector2(x,z);float d=1e9f;string n="";foreach(var s in prot){float e=SegDist(p,s.a,s.b);if(e<d){d=e;n=s.n;}}return n;}
  // ---- centre line ----
  var orig=File.ReadAllLines("Docs/Report074/house3-original-winding-points.txt").Select(l=>l.Split(' ')).Select(a=>new Vector3(float.Parse(a[1]),float.Parse(a[2]),float.Parse(a[3]))).ToList();
  List<Vector3> ctrl;
  // Forest Loop Reverse: cross the trail at a shallow angle where its south shoulder is lowest (x ~ 471), slide off it
  // westward along the strip of plateau south of it, and rejoin the original line at s ~ 116.
  if(Scene=="ForestLoopReverse")ctrl=orig.Take(50).Concat(new[]{new Vector3(471.2f,0,-189f),new Vector3(472.6f,0,-196.5f),new Vector3(471.6f,0,-203f),new Vector3(466.5f,0,-206.6f),new Vector3(458f,0,-208.8f),new Vector3(448.5f,0,-210.3f)}).Concat(orig.Skip(81)).ToList();
  else ctrl=orig.ToList();
  // On the valley floor the original line runs through the west end of the House 3 lake (added in 0.31 on top of it):
  // there the driveway goes round the lake's west tip and along its north shore instead (s 214-268 of the original).
  {var os=new float[orig.Count];for(int i=1;i<orig.Count;i++)os[i]=os[i-1]+new Vector2(orig[i].x-orig[i-1].x,orig[i].z-orig[i-1].z).magnitude;
   var from=orig[Array.FindLastIndex(os,v=>v<=214)];var to=orig[Array.FindIndex(os,v=>v>=300)];int ia=ctrl.IndexOf(from),ib=ctrl.IndexOf(to);
   // ... and keeps 8 m from the pool's north wall (z -188.7) past the pool
   var around=new[]{new Vector3(366.8f,0,-207f),new Vector3(364.6f,0,-200f),new Vector3(364.9f,0,-195.5f),new Vector3(366.6f,0,-189.6f),new Vector3(370.6f,0,-185.1f),new Vector3(377f,0,-182.1f),new Vector3(386f,0,-181.1f),new Vector3(396f,0,-181f),new Vector3(406f,0,-180.6f),new Vector3(416f,0,-180.5f),new Vector3(426f,0,-180.8f),new Vector3(431.5f,0,-181.2f)};
   if(ia>0&&ib>ia)ctrl=ctrl.Take(ia+1).Concat(around).Concat(ctrl.Skip(ib)).ToList();else Note($"BUG-003 {Scene}: lake detour not applied (indices {ia},{ib})");}
  var C=CatmullResample(ctrl,2f);int N=C.Count;var S=new float[N];for(int i=1;i<N;i++)S[i]=S[i-1]+new Vector2(C[i].x-C[i-1].x,C[i].z-C[i-1].z).magnitude;
  var P=new float[N];for(int i=0;i<N;i++)P[i]=Prot(C[i].x,C[i].z);
  // terrain within 15 m of an authored road or branch is not reshaped (0.30 rule); in Forest Loop Reverse the crossing of
  // the Forest Reverse trail is reshaped only from 5.5 m off its centre line (outside its driving surface).
  bool flr=Scene=="ForestLoopReverse";float Lim(float s)=>s<20?5.5f:flr&&s>60&&s<130?5.5f:flr?7f:15f;// s<20: the gate pad beyond the main road edge
  (float s,float d) Nearest(float x,float z){var p=new Vector2(x,z);float best=1e9f,bs=0;for(int i=0;i+1<N;i++){var a=new Vector2(C[i].x,C[i].z);var b=new Vector2(C[i+1].x,C[i+1].z);var ab=b-a;float t=Mathf.Clamp01(Vector2.Dot(p-a,ab)/Mathf.Max(1e-6f,ab.sqrMagnitude));float e=(a+ab*t-p).magnitude;if(e<best){best=e;bs=S[i]+t*(S[i+1]-S[i]);}}return (bs,best);}
  float H(float[] h,float s){int i=Mathf.Clamp(Array.FindLastIndex(S,v=>v<=s),0,N-2);float t=Mathf.InverseLerp(S[i],S[i+1],s);return Mathf.Lerp(h[i],h[i+1],t);}
  // ---- dependents: record their ground before anything changes ----
  skipGround=sCol;
  var area=new Bounds(new Vector3(bbox.center.x,100,bbox.center.y),new Vector3(bbox.width,400,bbox.height));
  bool NearWork(Vector3 w){float st=Vector3.Dot(new Vector3(w.x-a0.x,0,w.z-a0.z),fwd),lat=Vector3.Dot(new Vector3(w.x-a0.x,0,w.z-a0.z),right);if(st>-2&&st<L+2&&Mathf.Abs(lat)<12)return true;return Nearest(w.x,w.z).d<11;}
  var trunks=Object.FindObjectsByType<Collider>(FindObjectsSortMode.None).Where(c=>!c.isTrigger&&c.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0&&c.bounds.size.x<3&&NearWork(c.bounds.center)).ToArray();
  var trunkOld=trunks.ToDictionary(c=>c,c=>GroundT(c.bounds.center.x,c.bounds.center.z));
  bool IsTreeMesh(string p)=>p.Contains("tree")||p.Contains("Tree")||p.Contains("canopy")||p.Contains("woodland")||p.Contains("Woods")||p.Contains("foliage");
  var props=Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None).Where(r=>r.enabled&&r.gameObject.activeInHierarchy&&!(r is ParticleSystemRenderer)&&!r.name.StartsWith("Ground")&&!IsTreeMesh(Path(r.transform))&&!Path(r.transform).StartsWith("House 3 pool and lake")&&r.transform!=straight&&r.bounds.size.x<40&&r.bounds.size.z<40&&NearWork(r.bounds.center)).ToArray();
  var propOld=new Dictionary<Renderer,float>();var fenceOld=new Dictionary<MeshFilter,float[]>();
  foreach(var r in props){var b=r.bounds;propOld[r]=GroundT(b.center.x,b.center.z);
   var mf=r.GetComponent<MeshFilter>();if(mf&&mf.sharedMesh&&(r.name.Contains("fence")||r.name.Contains("crossbuck")||Path(r.transform).Contains("fence")||Path(r.transform).Contains("crossbuck"))&&mf.sharedMesh.isReadable)fenceOld[mf]=mf.sharedMesh.vertices.Select(v=>{var w=mf.transform.TransformPoint(v);return GroundT(w.x,w.z);}).ToArray();}
  // ---- 1. fill the slot ----
  var tiles=Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Where(m=>System.Text.RegularExpressions.Regex.IsMatch(m.name,@"^Ground_\d+_\d+$")&&m.GetComponent<MeshCollider>()&&m.sharedMesh).ToArray();
  var slotBox=new Bounds((a0+a1)/2,new Vector3(Mathf.Abs(a0.x-a1.x)+30,400,Mathf.Abs(a0.z-a1.z)+30));
  int filled=0;float fillMax=0;var newVerts=new Dictionary<MeshFilter,Vector3[]>();
  var newCols=new Dictionary<MeshFilter,Color[]>();
  foreach(var mf in tiles){if(!mf.GetComponent<MeshCollider>().bounds.Intersects(slotBox))continue;var lv=mf.sharedMesh.vertices;bool any=false;
   // ground colour beside the slot (10.5-15 m out) per 3 m of station, used to repaint the filled ground (no dirt stripe left)
   var tcol=mf.sharedMesh.colors;var band=new Dictionary<int,(Color c,int n)>();
   if(tcol.Length==lv.Length)for(int i=0;i<lv.Length;i++){var w=mf.transform.TransformPoint(lv[i]);var rel=new Vector3(w.x-a0.x,0,w.z-a0.z);float st=Vector3.Dot(rel,fwd),la=Mathf.Abs(Vector3.Dot(rel,right));if(st<-3||st>L+3||la<10.5f||la>15)continue;int k=Mathf.FloorToInt(st/3);band.TryGetValue(k,out var e);band[k]=(e.c+tcol[i],e.n+1);}
   Color Rim(float st){for(int r=0;r<6;r++)foreach(int k in new[]{Mathf.FloorToInt(st/3)+r,Mathf.FloorToInt(st/3)-r})if(band.TryGetValue(k,out var e)&&e.n>0)return e.c/e.n;return new Color(.4f,.5f,.3f,1);}
   for(int i=0;i<lv.Length;i++){var w=mf.transform.TransformPoint(lv[i]);var rel=new Vector3(w.x-a0.x,0,w.z-a0.z);float st=Vector3.Dot(rel,fwd),lat=Vector3.Dot(rel,right);if(st<0||st>L||Mathf.Abs(lat)>=10)continue;if(Prot(w.x,w.z)<=15)continue;
    var pl=a0+fwd*st-right*11.5f;var pr=a0+fwd*st+right*11.5f;float yl=GroundT(pl.x,pl.z),yr=GroundT(pr.x,pr.z);if(float.IsNaN(yl)||float.IsNaN(yr))continue;
    float yf=Mathf.Lerp(yl,yr,(lat+11.5f)/23f);float wgt=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(4,10,Mathf.Abs(lat)));float ny=Mathf.Lerp(w.y,yf,wgt);if(ny<=w.y+.005f)continue;
    fillMax=Mathf.Max(fillMax,ny-w.y);w.y=ny;lv[i]=mf.transform.InverseTransformPoint(w);filled++;any=true;if(tcol.Length==lv.Length)tcol[i]=Color.Lerp(tcol[i],Rim(st),wgt);}
   if(any){newVerts[mf]=lv;if(tcol.Length==lv.Length)newCols[mf]=tcol;}}
  // the filled ground is relaxed (5 neighbour-average passes, weighted like the fill) so no ridges or facets remain
  foreach(var kv in newVerts){var mf=kv.Key;var lv=kv.Value;var t=mf.sharedMesh.triangles;var nb=new Dictionary<int,HashSet<int>>();var wts=new Dictionary<int,float>();
   for(int i=0;i<lv.Length;i++){var w=mf.transform.TransformPoint(lv[i]);var rel=new Vector3(w.x-a0.x,0,w.z-a0.z);float st=Vector3.Dot(rel,fwd),la=Mathf.Abs(Vector3.Dot(rel,right));if(st<0||st>L||la>=10||Prot(w.x,w.z)<=15)continue;wts[i]=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(6,10,la));}
   for(int k=0;k<t.Length;k+=3)for(int a=0;a<3;a++){int u=t[k+a];if(!wts.ContainsKey(u))continue;if(!nb.TryGetValue(u,out var set))nb[u]=set=new HashSet<int>();set.Add(t[k+(a+1)%3]);set.Add(t[k+(a+2)%3]);}
   for(int it=0;it<5;it++){var ny=new Dictionary<int,float>();foreach(var e in nb){float sum=0;foreach(int q in e.Value)sum+=lv[q].y;float avg=sum/e.Value.Count;ny[e.Key]=Mathf.Lerp(lv[e.Key].y,avg,.6f*wts[e.Key]);}foreach(var e in ny)lv[e.Key].y=e.Value;}}
  void Apply(Dictionary<MeshFilter,Vector3[]> nv){foreach(var kv in nv){var m=Own(kv.Key);m.vertices=kv.Value;if(newCols.TryGetValue(kv.Key,out var cc)&&cc.Length==kv.Value.Length)m.colors=cc;m.RecalculateNormals();m.RecalculateBounds();Refresh(kv.Key);}Physics.SyncTransforms();}
  Note($"BUG-003 {Scene}: slot fill: {filled} terrain vertices raised (max +{fillMax:F1} m) in {newVerts.Count} tiles ({string.Join(", ",newVerts.Keys.Select(k=>k.name))})");
  if(!dry)Apply(newVerts);
  // ---- 2. profile along the winding line ----
  var g=new float[N];for(int i=0;i<N;i++)g[i]=GroundT(C[i].x,C[i].z);
  {// inside the old slot the centre follows the original authored height of the winding driveway (the hillside before 0.30)
   for(int i=0;i<N;i++){var rel=new Vector3(C[i].x-a0.x,0,C[i].z-a0.z);float st=Vector3.Dot(rel,fwd),lat=Vector3.Dot(rel,right);if(st>=0&&st<=L&&Mathf.Abs(lat)<10){int k=0;float best=1e9f;for(int j=0;j<orig.Count;j++){float e=new Vector2(orig[j].x-C[i].x,orig[j].z-C[i].z).magnitude;if(e<best){best=e;k=j;}}if(best<3)g[i]=dry?Mathf.Max(g[i],orig[k].y):orig[k].y;}}}
  var h=new float[N];for(int i=0;i<N;i++){if(P[i]<Lim(S[i])){h[i]=g[i];continue;}float sw=0,sy=0;for(int j=0;j<N;j++){float ds=Mathf.Abs(S[j]-S[i]);if(ds>8)continue;float wv=1-ds/8.5f;sw+=wv;sy+=wv*g[j];}h[i]=sy/sw;}
  // blend smoothly into the fixed (protected) stretches
  for(int i=0;i<N;i++){float lim=Lim(S[i]);if(P[i]>=lim&&P[i]<lim+6)h[i]=Mathf.Lerp(g[i],h[i],Mathf.InverseLerp(lim,lim+6,P[i]));}
  // rounded top: between the gate pad and s 60 the profile relaxes to a smooth curve (no crest to launch over)
  // smooth (cubic Hermite) between the last fixed sample and s = sEnd, matching the grades at both ends: no kinks
  void Ease(float sStart,float sEnd,Collider anchor=null,float startShare=float.NaN){int a=-1,b=-1;for(int i=0;i<N;i++){if(S[i]>=sStart&&P[i]>=Lim(S[i])&&a<0)a=Mathf.Max(0,i-1);if(S[i]<=sEnd)b=i;}if(a<0||b<=a+2)return;
   float G(int i,int dir){int j=i;while(j+dir>=0&&j+dir<N&&Mathf.Abs(S[j+dir]-S[i])<4)j+=dir;return j==i?0:(h[j]-h[i])/(S[j]-S[i]);}
   float L0=S[b]-S[a],y0=h[a],y1=h[b],m0=G(a,-1)*L0,m1=G(b,1)*L0;
   // at the gate the curve starts level on the pad surface itself (the terrain beside the pad stands a little higher)
   if(anchor){m0=0;y0=g[0];h[a]=y0;}// level with the main road at the gate (the pad and the ground beside it are uneven by up to 0.5 m)
   // optionally start already descending at a share of the average grade (lower peak grade over the window)
   if(!float.IsNaN(startShare))m0=(y1-y0)*startShare;for(int i=a+1;i<b;i++){float t=(S[i]-S[a])/L0,t2=t*t,t3=t2*t;h[i]=(2*t3-3*t2+1)*y0+(t3-2*t2+t)*m0+(-2*t3+3*t2)*y1+(t3-t2)*m1;}}
  Ease(0,60,pad.GetComponent<MeshCollider>());
  // and the same through the Forest Reverse crossing flank (between the trail edge and s 135)
  if(flr)
  {// before and after the trail crossing (kept at the trail's own height), each eased to it: no step where the gravel meets the trail
   int i1=-1,i2=-1;for(int i=0;i<N;i++)if(S[i]>60&&S[i]<130&&P[i]<Lim(S[i])){if(i1<0)i1=i;i2=i;}
   void Range(int a,int b,float m0Sign){if(a<0||b<=a+2)return;float G(int i,int dir){int j=i;while(j+dir>=0&&j+dir<N&&Mathf.Abs(S[j+dir]-S[i])<4)j+=dir;return j==i?0:(h[j]-h[i])/(S[j]-S[i]);}
    float L0=S[b]-S[a],y0=h[a],y1=h[b],m0=G(a,-1)*L0*m0Sign,m1=G(b,1)*L0;for(int i=a+1;i<b;i++){float t=(S[i]-S[a])/L0,t2=t*t,t3=t2*t;h[i]=(2*t3-3*t2+1)*y0+(t3-2*t2+t)*m0+(-2*t3+3*t2)*y1+(t3-t2)*m1;}}
   int ia=Array.FindIndex(S,v=>v>=70),ib=Array.FindLastIndex(S,v=>v<=190);
   if(i1>0){Range(ia,i1,1);Range(i2,ib,0);}// leaving the trail level, then down to the original line
  }
  float gradeMax=0,gradeAt=0,gradeOld=0;for(int i=0;i+1<N;i++){float gr=Mathf.Abs(h[i+1]-h[i])/Mathf.Max(.1f,S[i+1]-S[i]);if(gr>gradeMax){gradeMax=gr;gradeAt=S[i];}float g2=Mathf.Abs(g[i+1]-g[i])/Mathf.Max(.1f,S[i+1]-S[i]);gradeOld=Mathf.Max(gradeOld,g2);}
  float crest=0,crestAt=0;for(int i=1;i+1<N;i++){float k=((h[i+1]-h[i])/(S[i+1]-S[i])-(h[i]-h[i-1])/(S[i]-S[i-1]))/((S[i+1]-S[i-1])/2);if(Mathf.Abs(k)>crest){crest=Mathf.Abs(k);crestAt=S[i];}}
  Note($"BUG-003 {Scene}: winding line {N} samples, {S[^1]:F0} m, {h[0]:F1} -> {h[^1]:F1} m; ground along it before smoothing max grade {gradeOld*100:F0}%; centre profile max grade {gradeMax*100:F0}% ({Mathf.Atan(gradeMax)*Mathf.Rad2Deg:F0} deg) at s {gradeAt:F0}; sharpest vertical curvature {crest:F3}/m (radius {1/Mathf.Max(crest,1e-4f):F0} m) at s {crestAt:F0}");
  var prof=(Environment.GetEnvironmentVariable("AUTHOR_PROFILE")??"").Split(',');if(prof.Length==2)for(int i=0;i<N;i++)if(S[i]>=float.Parse(prof[0])&&S[i]<=float.Parse(prof[1]))Note($"   p s {S[i],5:F1} ({C[i].x:F1},{C[i].z:F1}) g {g[i]:F2} h {h[i]:F2} P {P[i]:F1}");
  for(int i=0;i<N;i+=10)Note($"   s {S[i],5:F0} ({C[i].x:F1},{C[i].z:F1}) ground {g[i]:F2} profile {h[i]:F2} protected-dist {P[i]:F1} {(P[i]<20?ProtName(C[i].x,C[i].z):"")}");
  // ---- 3. fit the terrain: within 9 m every vertex moves by the profile correction (profile - ground at the centre) ----
  var g0=new float[N];for(int i=0;i<N;i++)g0[i]=GroundT(C[i].x,C[i].z);
  var nv2=new Dictionary<MeshFilter,Vector3[]>();int fitted=0;float fitUp=0,fitDown=0;var lineBox=new Bounds(C[0],Vector3.zero);foreach(var c in C)lineBox.Encapsulate(c);lineBox.Expand(new Vector3(20,400,20));
  foreach(var mf in tiles){if(!mf.GetComponent<MeshCollider>().bounds.Intersects(lineBox))continue;var lv=newVerts.TryGetValue(mf,out var nvv)&&!dry?mf.sharedMesh.vertices:mf.sharedMesh.vertices;bool any=false;
   for(int i=0;i<lv.Length;i++){var w=mf.transform.TransformPoint(lv[i]);var (s,d)=Nearest(w.x,w.z);if(d>=9)continue;float pd=Prot(w.x,w.z);if(pd<=Lim(s))continue;float corr=H(h,s)-H(g0,s);float wgt=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(4.5f,9,d));float ny=w.y+corr*wgt;if(Mathf.Abs(ny-w.y)<.005f)continue;
    fitUp=Mathf.Max(fitUp,ny-w.y);fitDown=Mathf.Min(fitDown,ny-w.y);w.y=ny;lv[i]=mf.transform.InverseTransformPoint(w);fitted++;any=true;}
   if(any)nv2[mf]=lv;}
  Note($"BUG-003 {Scene}: terrain fitted to the profile: {fitted} vertices ({fitDown:+0.00;-0.00} .. {fitUp:+0.00;-0.00} m) in {nv2.Count} tiles");
  // ---- obstructions in the new driving width ----
  var inWidth=Object.FindObjectsByType<Collider>(FindObjectsSortMode.None).Where(c=>!c.isTrigger&&!c.name.StartsWith("Ground")&&c.bounds.size.x<10&&c.bounds.size.z<10&&Nearest(c.bounds.center.x,c.bounds.center.z).d<4.6f&&c.bounds.center.y<H(h,Nearest(c.bounds.center.x,c.bounds.center.z).s)+6&&c.bounds.max.y>H(h,Nearest(c.bounds.center.x,c.bounds.center.z).s)-1).ToArray();
  foreach(var c in inWidth)Note($"BUG-003 {Scene}: in the driving width: {Path(c.transform)} {c.GetType().Name} at {c.bounds.center:F1} (s {Nearest(c.bounds.center.x,c.bounds.center.z).s:F0}, {Nearest(c.bounds.center.x,c.bounds.center.z).d:F1} m from centre)");
  if(dry){Note($"BUG-003 {Scene}: dry run; dependents considered: {trunks.Length} trunks, {props.Length} props ({fenceOld.Count} fence meshes)");return;}
  Apply(nv2);
  // ---- 4. the gravel surface ----
  var v=new List<Vector3>();var tri=new List<int>();const int cols=9;const float half=3.5f;int rows=0;var rowOk=new List<bool>();
  for(int i=0;i<N;i++){var f=C[Mathf.Min(N-1,i+1)]-C[Mathf.Max(0,i-1)];f.y=0;var sd=Vector3.Cross(Vector3.up,f).normalized;
   for(int j=0;j<cols;j++){var p=C[i]+sd*(half*(j/4f-1));v.Add(new Vector3(p.x,h[i]+.05f,p.z));}// level across, on the smooth profile
   rowOk.Add(P[i]>=(S[i]<20?5.5f:Scene=="ForestLoopReverse"?5.5f:12f));rows++;}
  for(int i=0;i+1<rows;i++){if(!rowOk[i]||!rowOk[i+1])continue;for(int j=0;j+1<cols;j++){int n=i*cols+j;tri.AddRange(new[]{n,n+cols,n+1,n+1,n+cols,n+cols+1});}}
  var mesh=new Mesh{indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};mesh.SetVertices(v);mesh.SetTriangles(tri,0);mesh.RecalculateNormals();mesh.RecalculateBounds();var stored=Store("house3-winding-drive",mesh);
  foreach(var old in drive.GetComponentsInChildren<Transform>(true).Where(t=>t.name==RibbonName).ToArray())Object.DestroyImmediate(old.gameObject);
  var go=new GameObject(RibbonName,typeof(MeshFilter),typeof(MeshRenderer),typeof(MeshCollider));go.transform.SetParent(drive.transform,false);go.GetComponent<MeshFilter>().sharedMesh=stored;go.GetComponent<MeshCollider>().sharedMesh=stored;go.GetComponent<MeshRenderer>().sharedMaterial=mat;go.layer=pad.gameObject.layer;
  GameObjectUtility.SetStaticEditorFlags(go,GameObjectUtility.GetStaticEditorFlags(pad.gameObject));
  Note($"BUG-003 {Scene}: gravel surface {tri.Count/3} tris, 7 m wide, rows {rowOk.Count(x=>x)}/{rows} (rows within 5.5 m of the main road centre are the road and the existing gate pad mouth{(Scene=="ForestLoopReverse"?"; rows within 5.5 m of the Forest Reverse trail centre are the trail itself, where the driveway crosses it":"")})");
  // ---- the ground is benched to the gravel: under it 10 cm below, blending back to the natural ground 3.6-8 m out ----
  {Physics.SyncTransforms();var rc=go.GetComponent<MeshCollider>();var nv3=new Dictionary<MeshFilter,Vector3[]>();int benched=0;float bUp=0,bDown=0;
   var targets=tiles;var bb=rc.bounds;bb.Expand(new Vector3(18,0,18));
   var waters=Object.FindObjectsByType<ShallowWater>(FindObjectsSortMode.None).Where(wv=>wv.transform.lossyScale.x>3).ToArray();// lake and pool shapes are kept
   var poolParts=Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None).Where(r=>Path(r.transform).StartsWith("House 3 pool and lake")).Select(r=>{var b=r.bounds;b.Expand(new Vector3(4,0,4));return b;}).ToArray();
   bool NearWater(Vector3 w){foreach(var wv in waters){var q=wv.transform.InverseTransformPoint(w);if(wv.round?q.x*q.x+q.z*q.z<.25f*1.25f:Mathf.Abs(q.x)<.56f&&Mathf.Abs(q.z)<.56f)return true;}foreach(var b in poolParts)if(w.x>b.min.x&&w.x<b.max.x&&w.z>b.min.z&&w.z<b.max.z)return true;return false;}
   foreach(var mf in targets){var col=mf.GetComponent<Collider>();if(!col||!col.bounds.Intersects(new Bounds(bb.center,new Vector3(bb.size.x,1000,bb.size.z))))continue;var lv=mf.sharedMesh.vertices;bool any=false;
    for(int i=0;i<lv.Length;i++){var w=mf.transform.TransformPoint(lv[i]);var (s,d)=Nearest(w.x,w.z);if(d>=8)continue;int k=Mathf.Clamp(Array.FindLastIndex(S,q=>q<=s),0,N-2);if(!rowOk[k]||!rowOk[k+1])continue;
     float pd=Prot(w.x,w.z);if(pd<=Lim(s))continue;if(NearWater(w))continue;float rib=H(h,s)+.05f;float ny=d<3.6f?rib-.1f:Mathf.Lerp(rib-.1f,w.y,Mathf.SmoothStep(0,1,Mathf.InverseLerp(3.6f,8,d)));
     ny=Mathf.Clamp(ny,w.y-3f,w.y+3.5f);// no deep cuts or tall fills beside the drive
     if(Mathf.Abs(ny-w.y)<.005f)continue;bUp=Mathf.Max(bUp,ny-w.y);bDown=Mathf.Min(bDown,ny-w.y);w.y=ny;lv[i]=mf.transform.InverseTransformPoint(w);any=true;benched++;}
    if(any)nv3[mf]=lv;}
   // the gate pad (render and collider meshes differ): where the gravel covers it, it goes 8 cm under the gravel
   {var pmf=pad.GetComponent<MeshFilter>();var pmc=pad.GetComponent<MeshCollider>();int pn=0;
    Mesh Lower(Mesh src,string tag){var lv=src.vertices;bool any=false;for(int i=0;i<lv.Length;i++){var w=pad.TransformPoint(lv[i]);var (s,d)=Nearest(w.x,w.z);if(d>=3.7f)continue;int k=Mathf.Clamp(Array.FindLastIndex(S,q=>q<=s),0,N-2);if(!rowOk[k]||!rowOk[k+1])continue;float rib=H(h,s)+.05f;if(w.y<rib-.06f)continue;w.y=rib-.08f;lv[i]=pad.InverseTransformPoint(w);any=true;pn++;}
     if(!any)return src;var c=Object.Instantiate(src);c.name=src.name;c.vertices=lv;c.RecalculateNormals();c.RecalculateBounds();return Store(tag,c);}
    var nr=Lower(pmf.sharedMesh,"house3-pad-render");var nc=pmc.sharedMesh==pmf.sharedMesh?nr:Lower(pmc.sharedMesh,"house3-pad-collider");pmf.sharedMesh=nr;pmc.sharedMesh=null;pmc.sharedMesh=nc;EditorUtility.SetDirty(pmf);EditorUtility.SetDirty(pmc);
    Note($"BUG-003 {Scene}: gate pad vertices put under the gravel: {pn}");}
   foreach(var kv in nv3){var m=Own(kv.Key);m.vertices=kv.Value;m.RecalculateNormals();m.RecalculateBounds();Refresh(kv.Key);}Physics.SyncTransforms();
   Note($"BUG-003 {Scene}: ground benched to the gravel: {benched} vertices ({bDown:+0.00;-0.00} .. {bUp:+0.00;-0.00} m) in {nv3.Count} meshes ({string.Join(", ",nv3.Keys.Select(x=>x.name))})");}
  // ---- 5. the old straight surface goes; the road follows the winding line ----
  Object.DestroyImmediate(straight.gameObject);skipGround=null;
  drive.points=C.Select((c,i)=>new Vector3(c.x,(float.IsNaN(GroundT(c.x,c.z))?h[i]:GroundT(c.x,c.z))+.05f,c.z)).ToArray();drive.Initialize();EditorUtility.SetDirty(drive);Physics.SyncTransforms();
  // ---- 6. dependents ----
  var moves=new List<(Collider tr,float baseRef,float dy)>();
  foreach(var tr in trunks){var b=tr.bounds;float og=trunkOld[tr],ng=GroundT(b.center.x,b.center.z);if(float.IsNaN(og)||float.IsNaN(ng))continue;float dy=ng-og;if(Mathf.Abs(dy)<.1f)continue;bool buried=b.min.y<og-1.5f;moves.Add((tr,buried?b.min.y+.3f:og,dy));}
  var inWidthTrunks=new HashSet<Collider>(inWidth.Where(c=>c.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0));
  moves.RemoveAll(m=>inWidthTrunks.Contains(m.tr));
  ShiftTrees(moves,area,"BUG-003 "+Scene);
  // trees standing in the new driving width move out to 6.5 m from the centre, on their own side, onto the new ground
  foreach(var tr in inWidthTrunks){var b=tr.bounds;var (s,d)=Nearest(b.center.x,b.center.z);var cpt=new Vector3(0,0,0);{int i=Mathf.Clamp(Array.FindLastIndex(S,v=>v<=s),0,N-2);float t=Mathf.InverseLerp(S[i],S[i+1],s);cpt=Vector3.Lerp(C[i],C[i+1],t);}
   var side=new Vector3(b.center.x-cpt.x,0,b.center.z-cpt.z);if(side.magnitude<.3f){var f=C[Mathf.Min(N-1,Array.FindLastIndex(S,v=>v<=s)+1)]-cpt;side=Vector3.Cross(Vector3.up,new Vector3(f.x,0,f.z));}side.Normalize();
   var dest=cpt+side*6.5f;float og=trunkOld.TryGetValue(tr,out var o)?o:GroundT(b.center.x,b.center.z);float ng=GroundT(dest.x,dest.z);
   var clash=Object.FindObjectsByType<Collider>(FindObjectsSortMode.None).Where(c=>c!=tr&&!c.isTrigger&&!c.name.StartsWith("Ground")&&c.bounds.size.x<8&&c.bounds.size.z<8&&new Vector2(c.bounds.center.x-dest.x,c.bounds.center.z-dest.z).magnitude<1.5f).Select(c=>c.name).ToArray();
   if(clash.Length>0){dest=cpt+side*8f;ng=GroundT(dest.x,dest.z);}
   MoveTree(tr,new Vector3(dest.x-b.center.x,(float.IsNaN(og)||float.IsNaN(ng))?0:ng-og,dest.z-b.center.z),$"BUG-003 {Scene} (was {d:F1} m from the driveway centre)");}
  int fences=0,moved=0;
  foreach(var kv in fenceOld){var mf=kv.Key;var m0=mf.sharedMesh;var lv=m0.vertices;bool any=false;for(int i=0;i<lv.Length;i++){var w=mf.transform.TransformPoint(lv[i]);float ng=GroundT(w.x,w.z),og=kv.Value[i];if(float.IsNaN(ng)||float.IsNaN(og))continue;float dy=ng-og;if(Mathf.Abs(dy)<.03f)continue;w.y+=dy;lv[i]=mf.transform.InverseTransformPoint(w);any=true;}
   if(any){var m=Own(mf);m.vertices=lv;m.RecalculateNormals();m.RecalculateBounds();Refresh(mf);fences++;Note($"BUG-003 {Scene}: fence {Path(mf.transform)} re-seated on the new ground");}}
  // props: parts of one small object (siblings under a parent spanning < 6 m) move together by the change under the object
  var groups=props.Where(r=>{var mf=r.GetComponent<MeshFilter>();return !(mf&&fenceOld.ContainsKey(mf));}).GroupBy(r=>{var par=r.transform.parent;if(!par)return r.transform;var sib=par.GetComponentsInChildren<Renderer>();if(sib.Length>12)return r.transform;var bb=sib[0].bounds;foreach(var x in sib)bb.Encapsulate(x.bounds);return bb.size.x<14&&bb.size.z<14?par:r.transform;});
  foreach(var grp in groups){var rs=grp.ToArray();var bb=rs[0].bounds;foreach(var x in rs)bb.Encapsulate(x.bounds);var low=rs.OrderBy(r=>r.bounds.min.y).First();float og=propOld[low],ng=GroundT(low.bounds.center.x,low.bounds.center.z);if(float.IsNaN(og)||float.IsNaN(ng))continue;float dy=ng-og;if(Mathf.Abs(dy)<.05f)continue;
   if(bb.min.y>og+1.2f||bb.min.y<og-2f){Note($"BUG-003 {Scene}: not moved (not resting on the ground): {Path(grp.Key)} base {bb.min.y:F2} old ground {og:F2} new {ng:F2}");continue;}
   foreach(var r in rs){r.transform.position+=Vector3.up*dy;EditorUtility.SetDirty(r.transform);}moved++;Note($"BUG-003 {Scene}: re-grounded {Path(grp.Key)} ({rs.Length} parts) {dy:+0.00;-0.00} m");}
  Physics.SyncTransforms();Note($"BUG-003 {Scene}: dependents: trees {moves.Count}, fences {fences}, props {moved}");}
}
