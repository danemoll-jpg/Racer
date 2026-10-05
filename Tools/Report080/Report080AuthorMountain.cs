using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using Racer;using Object=UnityEngine.Object;
// 0.80 Part A.6-7: BUG-009 edge seam cover and BUG-010 deck embankments (Mountain Loop Reverse). See Report080Author.
public static partial class Report080Author {
 static bool IsNew(Collider c)=>c&&c.name.StartsWith("Ground_Report080");
 // the first static, upward-facing solid surface below p (within depth) for which ok() holds
 static bool Surface(Vector3 p,float depth,Func<Collider,bool> ok,out RaycastHit hit){hit=default;var o=p;
  for(int k=0;k<40&&depth>0;k++){if(!Physics.Raycast(o,Vector3.down,out var h,depth,~0,QueryTriggerInteraction.Ignore))return false;if(h.normal.y>0&&!h.collider.attachedRigidbody&&ok(h.collider)){hit=h;return true;}depth-=h.distance+.02f;o=h.point+Vector3.down*.02f;}return false;}
 static Material Mat(string rendererName){var r=Object.FindObjectsByType<MeshRenderer>().FirstOrDefault(x=>x.name==rendererName);return r?r.sharedMaterial:null;}

 // ---------- BUG-009: at the start of the Reverse Downhill Ridge Cut the earth banks meet the trail's edges with
 // 10-50 cm notches and steps. A smooth collidable strip covers each edge: from 0.4 m inside the edge (tucked 3 cm under
 // the trail) out to 2.5 m, flush at the edge and then straight to the bank. The trail surface is not edited; the
 // stretch stops before the jump run-up (s 25+), so the jump is untouched.
 static void SeamCover(string title,float s0,float s1,bool dry){
  var wr=Object.FindObjectsByType<WoodlandRoute>().FirstOrDefault(b=>b.title==title);if(!wr){Note($"BUG-009: no '{title}' in {Scene}");return;}
  var v=new List<Vector3>();var t=new List<int>();float worstBefore=0;var rows=new List<string>();
  var runs=new List<(int start,int n)>();
  foreach(int side in new[]{-1,1}){int start=v.Count,n=0;
   for(float s=s0;s<=s1+.01f;s+=.5f){var c=wr.At(s,out var f);f.y=0;f.Normalize();var r=Vector3.Cross(Vector3.up,f)*side;
    if(!Surface(c+Vector3.up*1.5f,4,IsDrive,out var ch))break;float py=ch.point.y,w=0;
    for(;w<10;w+=.05f){if(!Surface(c+r*(w+.05f)+Vector3.up*(py-c.y+1.2f),2.5f,IsDrive,out var eh))break;py=eh.point.y;}
    // no edge here (the trail opens into the main road's junction): end this run
    if(w>=9.9f){if(n>1)runs.Add((start,n));start=v.Count;n=0;rows.Add($"{(side<0?"left":"right")} s {s:F1}: no edge within 10 m (junction), none");continue;}
    var e=c+r*w;e.y=py;
    // before: the lowest point within 2 m outside the edge, relative to the edge
    float lowest=0;for(float o=.1f;o<=2;o+=.1f)if(Surface(e+r*o+Vector3.up*1.5f,5,x=>!IsNew(x)&&!IsDrive(x),out var bh))lowest=Mathf.Min(lowest,bh.point.y-py);worstBefore=Mathf.Min(worstBefore,lowest);
    var outer=e+r*2.5f;float oy=Surface(outer+Vector3.up*2,6,x=>!IsNew(x),out var oh)?oh.point.y:py-.3f;
    var inner=e-r*.4f;float iy=Surface(inner+Vector3.up*1.5f,3,IsDrive,out var ih)?ih.point.y:py;
    v.Add(new Vector3(inner.x,iy-.03f,inner.z));v.Add(new Vector3(e.x,py-.005f,e.z));v.Add(new Vector3(outer.x,oy,outer.z));
    rows.Add($"{(side<0?"left":"right")} s {s:F1}: edge {w:F2} m out at y {py:F2}, deepest notch within 2 m {lowest:F2} m, bank at 2.5 m {oy-py:+0.00;-0.00} m");n++;}
   if(n>1)runs.Add((start,n));}
  foreach(var (start,n) in runs){
   for(int i=0;i+1<n;i++){int a=start+i*3,b=a+3;for(int k=0;k<2;k++){int p0=a+k,p1=a+k+1,q0=b+k,q1=b+k+1;
     foreach(var tr in new[]{new[]{p0,q0,p1},new[]{p1,q0,q1}}){var nn=Vector3.Cross(v[tr[1]]-v[tr[0]],v[tr[2]]-v[tr[0]]);if(nn.y<0)t.AddRange(new[]{tr[0],tr[2],tr[1]});else t.AddRange(tr);}}}}
  File.WriteAllLines($"Docs/Report080/b009-{Scene}-edges.txt",rows);
  Note($"BUG-009 {Scene}: {title} s {s0}-{s1}, both edges ({v.Count/3} stations); deepest notch/step within 2 m of the edge before: {worstBefore:F2} m");
  if(dry||t.Count==0)return;
  var m=new Mesh{indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};m.SetVertices(v);m.SetTriangles(t,0);m.RecalculateNormals();m.RecalculateBounds();
  var g=Make("Ground_Report080 BUG-009 edge seam cover",Store("BUG-009 edge seam cover",m),Mat("Ground_CR133 mountain earth banks"));
  Note($"BUG-009 {Scene}: built '{g.name}' with a mesh collider, earth-bank material");}

 // ---------- BUG-010: driving-surface edges with open air under them. For each boundary vertex of the CR133 driving
 // surface inside the box, the gap to the first surface under a point 0.3 m outside the edge; where it is 0.4-8 m an
 // earth embankment runs from 5 cm under the edge outward and down at 1:1 to the ground (0.4 m into it). Never inside
 // a jump's recovery exclusion zone, a committed flight stretch, above another route or under a roof (section 5A);
 // gaps over 8 m (ravines, flight gaps, bridges) are listed and left.
 static void Embank(string label,Vector3 lo,Vector3 hi,bool dry){
  var mf=Object.FindObjectsByType<MeshFilter>().FirstOrDefault(m=>m.name=="Ground_CR133 mountain driving surface");if(!mf){Note($"{label}: no driving surface in {Scene}");return;}
  var race=Race;race.road.Initialize();var flights=Object.FindAnyObjectByType<MountainFlights>(FindObjectsInactive.Include);var excl=Object.FindObjectsByType<JumpRecoveryExclusion>(FindObjectsInactive.Include);var routes=Routes();
  var mesh=mf.sharedMesh;var w=mesh.vertices.Select(x=>mf.transform.TransformPoint(x)).ToArray();var tris=mesh.triangles;
  var id=new Dictionary<(int,int,int),int>();int[] wid=new int[w.Length];for(int i=0;i<w.Length;i++){var k=(Mathf.RoundToInt(w[i].x*100),Mathf.RoundToInt(w[i].y*100),Mathf.RoundToInt(w[i].z*100));if(!id.TryGetValue(k,out var x)){x=id.Count;id[k]=x;}wid[i]=x;}
  var pos=new Vector3[id.Count];for(int i=0;i<w.Length;i++)pos[wid[i]]=w[i];
  var edges=new Dictionary<(int,int),(int count,int third)>();
  for(int i=0;i<tris.Length;i+=3)for(int e=0;e<3;e++){int a=wid[tris[i+e]],b=wid[tris[i+(e+1)%3]],c=wid[tris[i+(e+2)%3]];if(a==b)continue;var key=a<b?(a,b):(b,a);edges[key]=edges.TryGetValue(key,out var x)?(x.count+1,x.third):(1,c);}
  var outn=new Dictionary<int,Vector3>();var bedges=new List<(int a,int b)>();
  foreach(var kv in edges){if(kv.Value.count!=1)continue;int a=kv.Key.Item1,b=kv.Key.Item2;var pa=pos[a];var pb=pos[b];var mid=(pa+pb)*.5f;
   if(mid.x<lo.x||mid.x>hi.x||mid.z<lo.z||mid.z>hi.z)continue;var along=pb-pa;along.y=0;if(along.sqrMagnitude<1e-6f)continue;var o=Vector3.Cross(Vector3.up,along).normalized;var th=pos[kv.Value.third];if(Vector3.Dot(o,new Vector3(th.x-mid.x,0,th.z-mid.z))>0)o=-o;
   bedges.Add((a,b));outn[a]=(outn.TryGetValue(a,out var na)?na:Vector3.zero)+o;outn[b]=(outn.TryGetValue(b,out var nb)?nb:Vector3.zero)+o;}
  // a flight's lip, flight path and landing (30 m around the lip-to-landing-end segment); its long run-up is not
  float Seg(Vector3 p,Vector3 a,Vector3 b){var q=new Vector2(p.x,p.z);var A=new Vector2(a.x,a.z);var ab=new Vector2(b.x,b.z)-A;float t2=ab.sqrMagnitude<1e-6f?0:Mathf.Clamp01(Vector2.Dot(q-A,ab)/ab.sqrMagnitude);return (A+ab*t2-q).magnitude;}
  string Protected(Vector3 p){foreach(var z in excl)if(z.Contains(p))return "jump exclusion "+z.name;
   if(flights)foreach(var f in flights.flights)if(Seg(p,f.lip,f.landingEnd)<30)return "flight "+f.name;
   foreach(var (n,pts,hw) in routes)foreach(var c in pts){float dy=c.y-p.y;if(dy<-1.5f&&dy>-60&&new Vector2(c.x-p.x,c.z-p.z).magnitude<hw+3)return "above "+n;}
   if(Physics.Raycast(p+Vector3.up*.5f,Vector3.up,out var up,12,~0,QueryTriggerInteraction.Ignore)&&!up.collider.attachedRigidbody)return "under a roof";return null;}
  var slope=new Dictionary<int,(Vector3 top,Vector3 bottom,float gap)>();int deep=0,prot=0;var listed=new List<string>();
  foreach(var kv in outn){var p=pos[kv.Key];var o=kv.Value;o.y=0;if(o.sqrMagnitude<1e-6f)continue;o.Normalize();
   if(!Surface(p+o*.3f+Vector3.up*.2f,40,x=>!IsDrive(x)&&!IsNew(x),out var gh))continue;float gap=p.y-gh.point.y;if(gap<.4f)continue;
   if(gap>8){deep++;if(listed.Count<400)listed.Add($"deep {gap:F1} m at ({p.x:F1}, {p.y:F1}, {p.z:F1}): left");continue;}
   var why=Protected(p);if(why!=null){prot++;if(listed.Count<400)listed.Add($"gap {gap:F1} m at ({p.x:F1}, {p.y:F1}, {p.z:F1}): {why}, left");continue;}
   var top=p+Vector3.down*.05f;Vector3 bottom=top;bool met=false;
   for(float d=.5f;d<=10;d+=.5f){var q=top+o*d+Vector3.down*d;if(Surface(new Vector3(q.x,top.y+.5f,q.z),40,x=>!IsDrive(x)&&!IsNew(x),out var bh)&&bh.point.y>=q.y-.3f){bottom=new Vector3(q.x,bh.point.y-.4f,q.z);met=true;break;}}
   if(!met){if(listed.Count<400)listed.Add($"gap {gap:F1} m at ({p.x:F1}, {p.y:F1}, {p.z:F1}): slope does not meet the ground within 10 m, left");continue;}
   slope[kv.Key]=(top,bottom,gap);}
  // one welded surface (smooth shading, no facets): the foot of the slope is smoothed along the edge (the outward
  // distance averaged over the neighbouring edge vertices, then re-seated 0.4 m into the ground there), so the bank
  // reads as one continuous slope, not a row of teeth
  var nbr=new Dictionary<int,List<int>>();foreach(var (a,b) in bedges){if(!slope.ContainsKey(a)||!slope.ContainsKey(b))continue;if(!nbr.TryGetValue(a,out var la))nbr[a]=la=new();la.Add(b);if(!nbr.TryGetValue(b,out var lb))nbr[b]=lb=new();lb.Add(a);}
  var dist=slope.ToDictionary(kv=>kv.Key,kv=>{var d=kv.Value.bottom-kv.Value.top;d.y=0;return d.magnitude;});
  for(int pass=0;pass<4;pass++){var nd=new Dictionary<int,float>();foreach(var kv in dist){float sum=kv.Value;int n=1;if(nbr.TryGetValue(kv.Key,out var l))foreach(var o in l){sum+=dist[o];n++;}nd[kv.Key]=Mathf.Max(kv.Value,sum/n);}dist=nd;}
  var v=new List<Vector3>();var t=new List<int>();var topI=new Dictionary<int,int>();var botI=new Dictionary<int,int>();int quads=0;float worst=0;
  foreach(var kv in slope){var top=kv.Value.top;var o=outn[kv.Key];o.y=0;o.Normalize();var q=top+o*dist[kv.Key];
   float gy=Surface(new Vector3(q.x,top.y+.5f,q.z),40,x=>!IsDrive(x)&&!IsNew(x),out var bh)?bh.point.y:kv.Value.bottom.y+.4f;
   topI[kv.Key]=v.Count;v.Add(top);botI[kv.Key]=v.Count;v.Add(new Vector3(q.x,Mathf.Min(gy-.4f,top.y-.3f),q.z));worst=Mathf.Max(worst,kv.Value.gap);}
  foreach(var (a,b) in bedges){if(!topI.ContainsKey(a)||!topI.ContainsKey(b))continue;int ta=topI[a],tb=topI[b],ba=botI[a],bb=botI[b];
   var n=Vector3.Cross(v[tb]-v[ta],v[bb]-v[ta]);if(n.y<0)t.AddRange(new[]{ta,bb,tb,ta,ba,bb});else t.AddRange(new[]{ta,tb,bb,ta,bb,ba});quads++;}
  File.WriteAllLines($"Docs/Report080/{label.Replace(' ','-')}-{Scene}.txt",new[]{$"{label}: {outn.Count} edge vertices in the box; embanked {slope.Count} (gap 0.4-8 m, worst {worst:F1} m) as {quads} strips; deep gaps left {deep}; protected left {prot}"}.Concat(listed));
  Note($"{label} {Scene}: {outn.Count} driving-surface edge vertices in the box; {slope.Count} with 0.4-8 m of open air under the edge embanked ({quads} strips, worst {worst:F1} m); {deep} deeper gaps and {prot} protected places left (listed)");
  if(dry||quads==0)return;
  var m=new Mesh{indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};m.SetVertices(v);m.SetTriangles(t,0);m.RecalculateNormals();m.RecalculateBounds();
  var g=Make("Ground_Report080 "+label+" embankment",Store(label+" embankment",m),Mat("Ground_CR133 mountain earth banks"));
  Note($"{label} {Scene}: built '{g.name}' with a mesh collider, earth-bank material");}
}
public static partial class Report080Author {
 // ---------- BUG-008 (second part): beside the crest outcrop a V-shaped trench (between the outcrop's flank and the
 // shoulders) is walled in on every side; a vehicle that drops in cannot get out. On a 0.5 m grid of the top surface
 // over the box, every depression is filled to its spill level (priority flood); within 'radius' of 'centre' the
 // filled cells become one collidable earth surface (edges tucked 5 cm under the ground they meet). Never over a
 // driving surface or in a route corridor.
 static void FillPit(string label,Vector3 lo,Vector3 hi,Vector3 centre,float radius,bool dry){
  const float st=.5f;int nx=Mathf.RoundToInt((hi.x-lo.x)/st)+1,nz=Mathf.RoundToInt((hi.z-lo.z)/st)+1;var H=new float[nx,nz];var drive=new bool[nx,nz];
  for(int i=0;i<nx;i++)for(int j=0;j<nz;j++){var p=new Vector3(lo.x+i*st,600,lo.z+j*st);if(Surface(p,900,x=>!IsNew(x)||x.name.Contains("skirt"),out var h)){H[i,j]=h.point.y;drive[i,j]=IsDrive(h.collider);}else H[i,j]=float.NaN;}
  var F=(float[,])H.Clone();var done=new bool[nx,nz];var pq=new SortedSet<(float h,int i,int j)>();
  for(int i=0;i<nx;i++)for(int j=0;j<nz;j++)if(i==0||j==0||i==nx-1||j==nz-1||float.IsNaN(H[i,j])){done[i,j]=true;if(!float.IsNaN(H[i,j]))pq.Add((H[i,j],i,j));}
  while(pq.Count>0){var c=pq.Min;pq.Remove(c);foreach(var (di,dj) in new[]{(1,0),(-1,0),(0,1),(0,-1)}){int a=c.i+di,b=c.j+dj;if(a<0||b<0||a>=nx||b>=nz||done[a,b])continue;done[a,b]=true;F[a,b]=Mathf.Max(H[a,b],c.h);pq.Add((F[a,b],a,b));}}
  // narrow slots (a V too tight to drive out of) are raised to their lower rim: a morphological closing (dilate, then
  // erode) with a 3 m disk, which never lowers the ground and leaves anything wider than ~6 m as it is
  int R=6;var off=new List<(int,int)>();for(int a2=-R;a2<=R;a2++)for(int b2=-R;b2<=R;b2++)if(a2*a2+b2*b2<=R*R)off.Add((a2,b2));
  var D=new float[nx,nz];for(int i=0;i<nx;i++)for(int j=0;j<nz;j++){float mx=float.MinValue;foreach(var (a2,b2) in off){int x=i+a2,z=j+b2;if(x<0||z<0||x>=nx||z>=nz||float.IsNaN(H[x,z]))continue;mx=Mathf.Max(mx,H[x,z]);}D[i,j]=mx;}
  for(int i=0;i<nx;i++)for(int j=0;j<nz;j++){if(float.IsNaN(H[i,j]))continue;float mn=float.MaxValue;foreach(var (a2,b2) in off){int x=i+a2,z=j+b2;if(x<0||z<0||x>=nx||z>=nz||float.IsNaN(H[x,z]))continue;mn=Mathf.Min(mn,D[x,z]);}F[i,j]=Mathf.Max(F[i,j],mn);}
  var routes=Routes();var fill=new bool[nx,nz];int cells=0;float deepest=0;
  for(int i=0;i<nx;i++)for(int j=0;j<nz;j++){float d=F[i,j]-H[i,j];var q=new Vector3(lo.x+i*st,F[i,j],lo.z+j*st);
   if(d>.1f&&new Vector2(q.x-centre.x,q.z-centre.z).magnitude<radius&&!drive[i,j]&&Corridor(routes,q,1)==null){fill[i,j]=true;cells++;deepest=Mathf.Max(deepest,d);}}
  Note($"{label} {Scene}: {cells} grid cells ({cells*st*st:F0} m²) in walled-in hollows and narrow slots within {radius} m of ({centre.x}, {centre.z}), deepest {deepest:F2} m");
  if(dry||cells==0)return;
  var v=new List<Vector3>();var t=new List<int>();var idx=new Dictionary<(int,int),int>();
  int V(int i,int j){if(idx.TryGetValue((i,j),out var k))return k;k=v.Count;bool f=fill[i,j];v.Add(new Vector3(lo.x+i*st,(f?F[i,j]:H[i,j]-.05f),lo.z+j*st));idx[(i,j)]=k;return k;}
  for(int i=0;i+1<nx;i++)for(int j=0;j+1<nz;j++){if(!(fill[i,j]||fill[i+1,j]||fill[i,j+1]||fill[i+1,j+1]))continue;if(float.IsNaN(H[i,j])||float.IsNaN(H[i+1,j])||float.IsNaN(H[i,j+1])||float.IsNaN(H[i+1,j+1]))continue;
   int a=V(i,j),b=V(i+1,j),c=V(i+1,j+1),d=V(i,j+1);t.AddRange(new[]{a,d,c,a,c,b});}
  var m=new Mesh{indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};m.SetVertices(v);m.SetTriangles(t,0);m.RecalculateNormals();m.RecalculateBounds();
  var g=Make("Ground_Report080 "+label,Store(label,m),Mat("Ground_Report069 flush shoulders"));
  Note($"{label} {Scene}: built '{g.name}' ({t.Count/6} cells) with a mesh collider, shoulder material");}
}
