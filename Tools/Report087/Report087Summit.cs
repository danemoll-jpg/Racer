using System;using System.IO;using System.Linq;using System.Text;using System.Collections.Generic;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;using Racer;using Object=UnityEngine.Object;
// 0.87 Part B: the Summit Climb, Forest Loop Forward's second shortcut (LakeWoods only). Centre line and profile from
// Tools/Report087/Design-Summit.py (summit-points.txt: x y z of the trail surface every ~0.65 m).
//  - Ground: the two ground tiles under the line are copied to LakeWoods-only assets (Assets/Track/SummitClimb) and sculpted:
//    trail surface across the half-width (+0.4 m), then a 1:1 cut batter / 1:1.25 fill batter back to the hillside; never
//    within the main's half-width + 0.5 m (blended over 3 m beyond). Dirt vertex colour as the other forest trails. The
//    rough middle (s 72-114) gets low mounds (0.10-0.17 m) staggered left and right.
//  - Trees: every tree (trunk collider, or bark box) standing on the trail (half-width + 0.7 m) is removed with its drawn
//    pieces; also those on the sight lines from the top of the run-out to the main's hook. Trees and small objects on
//    changed ground are re-seated. Edge trees are planted (copies of nearby complete trees, drawn and with a trunk
//    collider) where the forest leaves a gap of more than ~7 m along the chute, and on the inside of each kink.
//  - Kink 1 boulder (collider), two exposed rock slabs (colliders) and roots (visual only) in the rough middle.
//  - WoodlandRoute "Summit Climb" (bypasses CP4, AI on), ReverseShortcutGuidance, sign at the fork (copy of the Echo Cave
//    sign), gold arrows on the branch, teal arrows on the main at the fork and rejoin, gold entrance and edge markers.
//  - Course id lake-v8-summit-climb. SUMMIT_DRY=1 reports only.
public static class Report087Summit {
 const string F="Assets/Track/SummitClimb";const float HW=2.75f,Core=HW+.4f,RoughFrom=85,RoughTo=125;
 static readonly StringBuilder rep=new();static int assetN;
 static string V(Vector3 v)=>$"{v.x:F2},{v.y:F2},{v.z:F2}";
 static string P(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n;}
 static float Flat(Vector3 a,Vector3 b)=>Vector2.Distance(new(a.x,a.z),new(b.x,b.z));
 static float H(int k,int salt)=>Mathf.Repeat(Mathf.Sin(k*12.9898f+salt*78.233f)*43758.5453f,1f);
 static Vector3[] pts;static float[] st;
 // branch-local coordinates: station, signed lateral (+ right of travel), centre point
 static float Local(Vector3 p,out float lat,out Vector3 c){float best=float.MaxValue,s=0;lat=0;c=pts[0];
  for(int i=1;i<pts.Length;i++){var a=pts[i-1];var b=pts[i];var v=b-a;v.y=0;var q=p-a;q.y=0;float t=Mathf.Clamp01(Vector3.Dot(q,v)/Mathf.Max(1e-6f,v.sqrMagnitude));var d=q-v*t;float dd=d.sqrMagnitude;
   if(dd<best){best=dd;s=Mathf.Lerp(st[i-1],st[i],t);c=Vector3.Lerp(a,b,t);var r=Vector3.Cross(Vector3.up,v).normalized;lat=Vector3.Dot(d,r);}}
  return s;}
 static Vector3 At(float s,out Vector3 f){s=Mathf.Clamp(s,0,st[^1]);int i=1;while(i<st.Length-1&&st[i]<s)i++;f=(pts[i]-pts[i-1]).normalized;return Vector3.Lerp(pts[i-1],pts[i],Mathf.InverseLerp(st[i-1],st[i],s));}
 static Vector3 Right(Vector3 f){f.y=0;return Vector3.Cross(Vector3.up,f.normalized);}
 // the main, near this stretch only
 static RaceRoad main;
 static float MainLat(Vector3 p){float best=float.MaxValue;for(float s=1790;s<=2150;s+=1){var q=main.At(s,out _);best=Mathf.Min(best,Flat(p,q));}return best;}
 static float MainW(Vector3 p)=>Mathf.Clamp01((MainLat(p)-5.5f)/3f);
 // rough middle: low mounds staggered left / right
 static readonly List<(float s,float l,float a,float r)> mounds=new();
 // exposed slabs: flat rock 0.10 m proud of the trail, its edges bevelled over 0.6 m (in the ground: no step a wheel or the AI's
 // obstacle sense reads as a wall)
 static readonly (float s,float l,float hs,float hl)[] Slabs={(97f,-1.15f,1.3f,.95f),(114f,1.2f,1.3f,.95f)};
 static float SlabH(float s,float l){float b=0;foreach(var q in Slabs){float es=Mathf.Abs(s-q.s)-q.hs,el=Mathf.Abs(l-q.l)-q.hl;float e=Mathf.Max(es,el);if(e<.6f)b=Mathf.Max(b,.10f*(e<=0?1:1-Mathf.SmoothStep(0,1,e/.6f)));}return b;}
 static float Bump(float s,float l){if(s<RoughFrom-3||s>RoughTo+3)return 0;float b=0;foreach(var m in mounds){float ds=s-m.s,dl=l-m.l;b+=m.a*Mathf.Exp(-(ds*ds+dl*dl)/(m.r*m.r));}return b+SlabH(s,l);}
 static float Surface(float s,float l,Vector3 c)=>c.y+Bump(s,l);
 static float Ground(Vector3 p,float near){float best=float.NaN,bd=1e9f;foreach(var h in Physics.RaycastAll(new Vector3(p.x,near+40,p.z),Vector3.down,120,~0,QueryTriggerInteraction.Ignore)){if(!h.collider.name.StartsWith("Ground"))continue;float d=Mathf.Abs(h.point.y-near);if(d<bd){bd=d;best=h.point.y;}}return best;}
 static bool TreeName(string n){n=n.ToLowerInvariant();return n.Contains("trunk")||n.Contains("tree");}
 static bool Veg(Renderer r){var m=r.sharedMaterial;return m&&m.shader&&m.shader.name=="Racer/GreyboxGround"&&m.HasProperty("_Vegetation")&&m.GetFloat("_Vegetation")>.5f;}
 static Mesh Store(Mesh m,string n){string path=$"{F}/{n}-{assetN++}.asset";var prior=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(prior){EditorUtility.CopySerialized(m,prior);return prior;}AssetDatabase.CreateAsset(m,path);return m;}
 // vegetation pieces (connected parts of the batch meshes)
 sealed class Piece{public MeshFilter mf;public int[] idx;public Bounds b;public bool bark;public int owner=-1;}
 sealed class Site{public Vector3 bottom;public Collider col;public float r,h;public bool remove;public float dy;public List<Piece> pieces=new();}
 public static void Run(){bool dry=Environment.GetEnvironmentVariable("SUMMIT_DRY")=="1";var o=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(o);
  try{
   var scene=EditorSceneManager.OpenScene("Assets/Scenes/LakeWoods.unity");Physics.SyncTransforms();var roots=scene.GetRootGameObjects();
   var race=roots.SelectMany(g=>g.GetComponentsInChildren<RaceDirector>(true)).First();main=race.road;main.Initialize();
   if(roots.SelectMany(g=>g.GetComponentsInChildren<WoodlandRoute>(true)).Any(b=>b.title=="Summit Climb"))throw new Exception("Summit Climb already exists");
   var inv=System.Globalization.CultureInfo.InvariantCulture;
   pts=File.ReadAllLines("Tools/Report087/summit-points.txt").Where(l=>!l.StartsWith("#")&&l.Trim().Length>0).Select(l=>{var a=l.Split(' ').Select(x=>float.Parse(x,inv)).ToArray();return new Vector3(a[0],a[1],a[2]);}).ToArray();
   st=new float[pts.Length];for(int i=1;i<pts.Length;i++)st[i]=st[i-1]+Flat(pts[i-1],pts[i]);float L=st[^1];
   rep.AppendLine($"Summit Climb: {pts.Length} points, length {L:F1} m, {V(pts[0])} -> {V(pts[^1])}; main station of the fork {main.Project(pts[0],out _):F1}, rejoin {main.Project(pts[^1],out _):F1}");
   for(int k=0;k<15;k++){float s=RoughFrom+2+k*2.8f;if(s>RoughTo-1)break;if(Mathf.Abs(s-97)<2.2f||Mathf.Abs(s-114)<2.2f)continue;mounds.Add((s,(k%2==0?-1:1)*(.6f+.7f*H(k,1)),.05f+.04f*H(k,2),1.4f+.5f*H(k,3)));}
   var corridor=new Bounds();corridor.SetMinMax(new Vector3(pts.Min(p=>p.x)-12,-100,pts.Min(p=>p.z)-12),new Vector3(pts.Max(p=>p.x)+12,400,pts.Max(p=>p.z)+12));
   if(!dry&&!AssetDatabase.IsValidFolder(F))AssetDatabase.CreateFolder("Assets/Track","SummitClimb");

   // ---- 1. ground
   var tiles=roots.SelectMany(g=>g.GetComponentsInChildren<MeshCollider>(true)).Where(c=>c.name.StartsWith("Ground_")&&c.sharedMesh&&c.transform.parent&&c.transform.parent.name.StartsWith("Memory loop")&&c.bounds.Intersects(corridor)).ToList();
   var changeAt=new List<(Vector3 p,float dy)>();float cut=0,fill=0,maxCut=0,maxFill=0;int nv=0;
   var dirt=new Color(.39f,.265f,.135f);var rock=new Color(.43f,.40f,.35f);
   foreach(var mc in tiles){var mesh=mc.sharedMesh;var path=AssetDatabase.GetAssetPath(mesh);var t=mc.transform;var v=mesh.vertices;var col=mesh.colors;bool any=false;var changed=new bool[v.Length];
    for(int i=0;i<v.Length;i++){var w=t.TransformPoint(v[i]);if(!corridor.Contains(w))continue;float s=Local(w,out float lat,out var c);float d=Mathf.Abs(lat);
     if(s<.3f||s>L-.3f||d>HW+9)continue;float mw=MainW(w);if(mw<=0)continue;
     float ts=Surface(s,lat,c);float target;
     if(d<=Core)target=ts;else{float e=d-Core;target=w.y>ts?Mathf.Min(w.y,ts+e*1.0f):Mathf.Max(w.y,ts-e*.8f);}
     // ends: fade in over the first / last 4 m of the line as well
     mw*=Mathf.Clamp01(s/4f)*Mathf.Clamp01((L-s)/4f);
     float ny=Mathf.Lerp(w.y,target,mw);float dy=ny-w.y;
     float dirtW=(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(HW-.6f,HW+.9f,d)))*mw;
     if(Mathf.Abs(dy)<.004f&&dirtW<.01f)continue;
     if(Mathf.Abs(dy)>=.004f){changed[i]=true;nv++;if(dy>0){fill+=dy;maxFill=Mathf.Max(maxFill,dy);}else{cut-=dy;maxCut=Mathf.Max(maxCut,-dy);}changeAt.Add((w,dy));}
     if(!dry){v[i]=t.InverseTransformPoint(new Vector3(w.x,ny,w.z));if(col.Length==v.Length){var dc=dirt;float bump=Bump(s,lat);if(bump>.04f)dc=Color.Lerp(dirt,rock,Mathf.Clamp01((bump-.04f)/.05f));col[i]=Color.Lerp(col[i],dc,dirtW);}}any=true;}
    rep.AppendLine($"ground tile {P(t)} {path}: {(any?"edited":"unchanged")}");
    if(!dry&&any){var copy=Object.Instantiate(mesh);copy.name="LakeWoods-summit-"+mc.name;var oldN=mesh.normals;copy.vertices=v;if(col.Length==v.Length)copy.colors=col;copy.RecalculateNormals();var nn=copy.normals;var near=new HashSet<int>();var tr=copy.triangles;
     for(int k=0;k<tr.Length;k+=3)if(changed[tr[k]]||changed[tr[k+1]]||changed[tr[k+2]]){near.Add(tr[k]);near.Add(tr[k+1]);near.Add(tr[k+2]);}
     for(int k=0;k<nn.Length;k++)if(!near.Contains(k))nn[k]=oldN[k];copy.normals=nn;copy.RecalculateBounds();
     string np=$"{F}/LakeWoods-summit-{mc.name}.asset";if(AssetDatabase.LoadAssetAtPath<Mesh>(np))throw new Exception("exists "+np);AssetDatabase.CreateAsset(copy,np);
     var mf=mc.GetComponent<MeshFilter>();if(mf&&mf.sharedMesh==mesh)mf.sharedMesh=copy;mc.sharedMesh=null;mc.sharedMesh=copy;rep.AppendLine($"  -> {np} (LakeWoods only; the original asset is unchanged)");}}
   rep.AppendLine($"ground vertices changed {nv} (1 m grid): cut {cut:F0} m3 (deepest {maxCut:F2} m), fill {fill:F0} m3 (highest {maxFill:F2} m)");
   Physics.SyncTransforms();
   float DyAt(Vector3 p){float best=0,bd=1.2f;foreach(var c in changeAt){float d=Flat(c.p,p);if(d<bd){bd=d;best=c.dy;}}return best;}

   // ---- 2. trees: sites (trunk colliders, and bark boxes without one) with their drawn pieces
   var vegs=roots.SelectMany(g=>g.GetComponentsInChildren<MeshRenderer>(true)).Where(r=>Veg(r)&&r.bounds.Intersects(corridor)).Select(r=>r.GetComponent<MeshFilter>()).Where(m=>m&&m.sharedMesh&&m.sharedMesh.isReadable).ToList();
   var pieces=new List<Piece>();
   foreach(var mf in vegs){var m=mf.sharedMesh;var wv=m.vertices.Select(x=>mf.transform.TransformPoint(x)).ToArray();var par=Enumerable.Range(0,wv.Length).ToArray();int Find(int x){while(par[x]!=x){par[x]=par[par[x]];x=par[x];}return x;}void U(int a,int c){a=Find(a);c=Find(c);if(a!=c)par[a]=c;}
    var tri=m.triangles;for(int k=0;k<tri.Length;k+=3){U(tri[k],tri[k+1]);U(tri[k],tri[k+2]);}var byPos=new Dictionary<Vector3Int,int>();for(int i=0;i<wv.Length;i++){var key=Vector3Int.RoundToInt(wv[i]*200);if(byPos.TryGetValue(key,out var j))U(i,j);else byPos[key]=i;}
    var used=new HashSet<int>(tri);foreach(var g in Enumerable.Range(0,wv.Length).Where(used.Contains).GroupBy(Find)){var idx=g.ToArray();var b=new Bounds(wv[idx[0]],Vector3.zero);foreach(var i in idx)b.Encapsulate(wv[i]);
     var c2=new Vector3(b.center.x,0,b.center.z);if(!corridor.Contains(new Vector3(c2.x,0,c2.z)))continue;pieces.Add(new Piece{mf=mf,idx=idx,b=b,bark=b.size.x<1.3f&&b.size.z<1.3f&&b.size.y>1.8f});}}
   var sites=new List<Site>();
   foreach(var c in roots.SelectMany(g=>g.GetComponentsInChildren<Collider>(true))){if(!TreeName(c.name)||c.isTrigger||!c.bounds.Intersects(corridor))continue;var b=c.bounds;sites.Add(new Site{bottom=new Vector3(b.center.x,b.min.y,b.center.z),col=c,r=Mathf.Max(b.extents.x,b.extents.z),h=b.size.y});}
   foreach(var p in pieces.Where(p=>p.bark)){var bt=new Vector3(p.b.center.x,p.b.min.y,p.b.center.z);if(sites.Any(s=>s.col&&Flat(s.bottom,bt)<.6f))continue;if(sites.Any(s=>!s.col&&Flat(s.bottom,bt)<.3f))continue;sites.Add(new Site{bottom=bt,r=Mathf.Max(p.b.extents.x,p.b.extents.z),h=p.b.size.y});}
   foreach(var p in pieces){var c=new Vector3(p.b.center.x,p.b.min.y,p.b.center.z);int best=-1;float bd=p.bark?.8f:6f;for(int i=0;i<sites.Count;i++){var s=sites[i];if(!(s.bottom.y+s.h*1.6f+1>=p.b.min.y&&s.bottom.y<p.b.max.y+1))continue;float d=Flat(s.bottom,c);if(d<bd){bd=d;best=i;}}p.owner=best;if(best>=0)sites[best].pieces.Add(p);}
   // which go: on the trail; on the merge sight lines
   var sight=new List<(Vector3 a,Vector3 b)>();foreach(float a in new[]{178f,188f,198f})foreach(float m in new[]{2045f,2055f,2065f,2075f}){sight.Add((At(a,out _),main.At(m,out _)));}
   float SegD(Vector3 p,Vector3 a,Vector3 b){var ab=new Vector2(b.x-a.x,b.z-a.z);var ap=new Vector2(p.x-a.x,p.z-a.z);float t=Mathf.Clamp01(Vector2.Dot(ap,ab)/ab.sqrMagnitude);return (ap-ab*t).magnitude;}
   int onTrail=0,onSight=0,seated=0;
   foreach(var s in sites){float ss=Local(s.bottom,out float lat,out var c);bool trail=ss>.5f&&ss<L-.5f&&Mathf.Abs(lat)<HW+.7f+s.r&&Mathf.Abs(s.bottom.y-c.y)<4;bool sl=MainLat(s.bottom)>6.5f&&sight.Any(q=>SegD(s.bottom,q.a,q.b)<1.8f+s.r);
    if(trail||sl){s.remove=true;if(trail)onTrail++;else onSight++;rep.AppendLine($"  remove tree {(s.col?P(s.col.transform):"(bark box, no collider)")} at {V(s.bottom)} branch s {ss:F1} lateral {lat:+0.0;-0.0} ({(trail?"on the trail":"merge sight line")}), drawn pieces {s.pieces.Count}");continue;}
    float dy=DyAt(s.bottom);if(Mathf.Abs(dy)>.03f){s.dy=dy;seated++;rep.AppendLine($"  re-seat tree {(s.col?P(s.col.transform):"(bark box)")} at {V(s.bottom)} by {dy:+0.00;-0.00} m (ground changed under it), pieces {s.pieces.Count}");}}
   rep.AppendLine($"trees removed on the trail {onTrail}, on the merge sight lines {onSight}; re-seated {seated}");
   // kink 2 tree (the existing one the upper offset bend goes round) and the obstacle report
   foreach(var s in sites.Where(x=>!x.remove&&x.col)){float ss=Local(s.bottom,out float lat,out _);if(ss>10&&ss<L-10&&Mathf.Abs(lat)<HW+2.2f)rep.AppendLine($"  edge tree (kept) {P(s.col.transform)} s {ss:F1} lateral {lat:+0.0;-0.0} (edge gap {Mathf.Abs(lat)-HW-s.r:F2} m)");}
   // small objects on changed ground (props, ferns, signs): re-seat
   foreach(var r in roots.SelectMany(g=>g.GetComponentsInChildren<Renderer>(true))){if(Veg(r)||r.name.StartsWith("Ground")||!r.bounds.Intersects(corridor)||r.bounds.size.x>8||r.bounds.size.z>8)continue;var b=r.bounds;var foot=new Vector3(b.center.x,b.min.y,b.center.z);float dy=DyAt(foot);if(Mathf.Abs(dy)<.03f)continue;
    rep.AppendLine($"  re-seat object {P(r.transform)} at {V(foot)} by {dy:+0.00;-0.00}");if(!dry)r.transform.position+=Vector3.up*dy;}

   // ---- 3. apply tree removal / re-seating to the drawn pieces and colliders
   if(!dry){var byMesh=pieces.GroupBy(p=>p.mf);
    foreach(var g in byMesh){var cutP=g.Where(p=>p.owner>=0&&sites[p.owner].remove).ToList();var move=g.Where(p=>p.owner>=0&&!sites[p.owner].remove&&sites[p.owner].dy!=0).ToList();
     // bushes and crown-only clumps (no trunk of their own) standing on changed ground move with it
     var looseMove=new Dictionary<Piece,float>();foreach(var p in g.Where(p=>p.owner<0)){var bt=new Vector3(p.b.center.x,p.b.min.y,p.b.center.z);float dy=DyAt(bt);if(Mathf.Abs(dy)>.03f)looseMove[p]=dy;}
     // bark boxes / crowns standing on the trail with no site match (crown-only clumps on the trail)
     var loose=g.Where(p=>p.owner<0).Where(p=>{float ss=Local(new Vector3(p.b.center.x,0,p.b.center.z),out float lat,out var c);return ss>.5f&&ss<L-.5f&&Mathf.Abs(lat)<HW+.7f&&p.b.min.y<c.y+3;}).ToList();cutP.AddRange(loose);
     foreach(var p in cutP)looseMove.Remove(p);
     if(cutP.Count+move.Count+looseMove.Count==0)continue;var mf=g.Key;var m=Object.Instantiate(mf.sharedMesh);var vv=m.vertices;var tr=mf.transform;var cutSet=new HashSet<int>(cutP.SelectMany(p=>p.idx));
     foreach(var p in move)foreach(var i in p.idx)vv[i]=tr.InverseTransformPoint(tr.TransformPoint(vv[i])+Vector3.up*sites[p.owner].dy);
     foreach(var kv in looseMove)foreach(var i in kv.Key.idx)vv[i]=tr.InverseTransformPoint(tr.TransformPoint(vv[i])+Vector3.up*kv.Value);
     var tri=m.triangles;var keep=new List<int>();for(int k=0;k<tri.Length;k+=3)if(!cutSet.Contains(tri[k]))keep.AddRange(new[]{tri[k],tri[k+1],tri[k+2]});
     m.vertices=vv;m.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;m.SetTriangles(keep,0);m.RecalculateBounds();rep.AppendLine($"  vegetation {P(tr)} [{AssetDatabase.GetAssetPath(mf.sharedMesh)}]: pieces cut {cutP.Count} (loose {loose.Count}), moved {move.Count}, bushes / clumps moved with the ground {looseMove.Count}");mf.sharedMesh=Store(m,"LakeWoods-summit-vegetation");}
    foreach(var s in sites){if(!s.col)continue;if(s.remove){var go=s.col.gameObject;if(go.GetComponents<Component>().Length<=2&&go.transform.childCount==0)Object.DestroyImmediate(go);else Object.DestroyImmediate(s.col);}else if(s.dy!=0)s.col.transform.position+=Vector3.up*s.dy;}}
   Physics.SyncTransforms();

   // ---- 4. objects of the shortcut
   var forest=roots.First(g=>g.name=="CR056 Forest Loop").transform;var root=new GameObject("Summit Climb shortcut").transform;root.SetParent(forest);
   Material Mat(string name){var g=AssetDatabase.FindAssets(name+" t:Material").Select(AssetDatabase.GUIDToAssetPath).FirstOrDefault(p=>Path.GetFileNameWithoutExtension(p)==name);if(g==null)throw new Exception("material "+name);return AssetDatabase.LoadAssetAtPath<Material>(g);}
   var granite=Mat("Cave granite");var bark=Mat("Tree bark");var gold=AssetDatabase.LoadAssetAtPath<Material>("Assets/Track/Discovery/CR117 alternate gold.mat");var teal=AssetDatabase.LoadAssetAtPath<Material>("Assets/Track/Discovery/CR117 main teal.mat");
   GameObject Part(string n,Mesh m,Material mat,bool solid,bool convex=true){var g=new GameObject(n,typeof(MeshFilter),typeof(MeshRenderer));g.transform.SetParent(root);m.RecalculateNormals();m.RecalculateBounds();if(!dry)m=Store(m,"LakeWoods-summit-part");g.GetComponent<MeshFilter>().sharedMesh=m;g.GetComponent<MeshRenderer>().sharedMaterial=mat;if(solid){var c=g.AddComponent<MeshCollider>();c.convex=convex;c.sharedMesh=m;}return g;}
   // faceted rock: a jittered octa-sphere, flat shaded
   Mesh Rock(Vector3 centre,float rx,float ry,float rz,int seed){var dirs=new List<Vector3>();int rings=5,seg=9;for(int i=0;i<=rings;i++){float th=Mathf.PI*i/rings;for(int j=0;j<seg;j++){if((i==0||i==rings)&&j>0)break;float ph=2*Mathf.PI*j/seg+(i%2)*.35f;dirs.Add(new Vector3(Mathf.Sin(th)*Mathf.Cos(ph),Mathf.Cos(th),Mathf.Sin(th)*Mathf.Sin(ph)));}}
    var pv=dirs.Select((d,k)=>centre+Vector3.Scale(d*(.82f+.3f*H(k+seed*31,7)),new Vector3(rx,ry,rz))).ToList();var tri=new List<int>();
    int Idx(int i,int j)=>i==0?0:i==rings?pv.Count-1:1+(i-1)*seg+((j%seg)+seg)%seg;
    for(int j=0;j<seg;j++)tri.AddRange(new[]{0,Idx(1,j+1),Idx(1,j)});for(int i=1;i<rings-1;i++)for(int j=0;j<seg;j++){tri.AddRange(new[]{Idx(i,j),Idx(i,j+1),Idx(i+1,j)});tri.AddRange(new[]{Idx(i,j+1),Idx(i+1,j+1),Idx(i+1,j)});}
    for(int j=0;j<seg;j++)tri.AddRange(new[]{pv.Count-1,Idx(rings-1,j),Idx(rings-1,j+1)});
    var fv=new List<Vector3>();var ft=new List<int>();for(int k=0;k<tri.Count;k+=3){ft.Add(fv.Count);fv.Add(pv[tri[k]]);ft.Add(fv.Count);fv.Add(pv[tri[k+1]]);ft.Add(fv.Count);fv.Add(pv[tri[k+2]]);}
    return new Mesh{vertices=fv.ToArray(),triangles=ft.ToArray()};}
   // kink 1 boulder: on the line the climb comes up on, beside the trail where the lower offset bend has moved it right
   {var b=new Vector3(537.55f,0,-195.60f);float g=Ground(b,48);var m=Rock(new Vector3(b.x,g+.85f,b.z),2.3f,1.75f,2.0f,1);Part("Summit Climb kink boulder",m,granite,true);float s=Local(b,out float lat,out _);rep.AppendLine($"kink 1 boulder at {V(new Vector3(b.x,g,b.z))}: branch s {s:F1}, lateral {lat:+0.0;-0.0}, its face {Mathf.Abs(lat)-2.3f*.82f-HW:F1}-{Mathf.Abs(lat)-2.3f*1.12f-HW:F1} m from the trail edge");}
   // rough middle: two exposed slabs (solid, low) and roots (drawn only)
   foreach(var q in Slabs){var c=At(q.s,out var f);var r=Right(f);var vs=new List<Vector3>();var tri=new List<int>();int n=0;
    for(int i=0;i<=4;i++)for(int j=0;j<=4;j++){float ds=(i/4f-.5f)*2*(q.hs+.1f),dl=(j/4f-.5f)*2*(q.hl+.1f);float wob=.12f*(H(i*5+j,(int)q.s)-.5f);var w=c+f.normalized*ds+r*(q.l+dl+wob);float ss=Local(w,out float ll,out var cc);w.y=Surface(ss,ll,cc)+.025f;vs.Add(w);}
    for(int i=0;i<4;i++)for(int j=0;j<4;j++){int a0=i*5+j;tri.AddRange(new[]{a0,a0+1,a0+5,a0+1,a0+6,a0+5});}
    var fv=new List<Vector3>();var ft=new List<int>();foreach(var ix in tri){ft.Add(fv.Count);fv.Add(vs[ix]);}Part("Summit Climb exposed rock slab",new Mesh{vertices=fv.ToArray(),triangles=ft.ToArray()},granite,false);n++;
    rep.AppendLine($"rock slab at branch s {q.s}, lateral {q.l:+0.0;-0.0}: the ground there is raised 0.10 m with 0.6 m bevels; drawn rock on it, no collider of its own");}
   for(int k=0;k<8;k++){float s=RoughFrom+3+k*4.8f+1.5f*H(k,11);var c=At(s,out var f);var r=Right(f);float l=(H(k,12)-.5f)*1.4f;float yaw=(k%2==0?1:-1)*(55+20*H(k,13));float len=2.6f+.9f*H(k,14);
    var dir=Quaternion.AngleAxis(yaw,Vector3.up)*new Vector3(f.x,0,f.z).normalized;var a=c+r*l-dir*len*.5f;var b2=c+r*l+dir*len*.5f;float sa=Local(a,out float la,out var ca),sb=Local(b2,out float lb,out var cb);a.y=Surface(sa,la,ca)+.03f;b2.y=Surface(sb,lb,cb)+.03f;
    var g=GameObject.CreatePrimitive(PrimitiveType.Cylinder);g.name="Summit Climb exposed root";g.transform.SetParent(root);Object.DestroyImmediate(g.GetComponent<Collider>());g.transform.position=(a+b2)*.5f;g.transform.up=(b2-a).normalized;g.transform.localScale=new Vector3(.16f,(b2-a).magnitude*.5f,.12f);g.GetComponent<Renderer>().sharedMaterial=bark;}

   // ---- 5. planted edge trees (copies of nearby complete trees: drawn pieces + a trunk collider)
   var templates=sites.Where(s=>!s.remove&&s.col&&s.col is BoxCollider&&s.pieces.Count>=2&&s.pieces.Any(p=>p.bark)&&s.pieces.Any(p=>!p.bark)&&s.h>4).GroupBy(s=>s.pieces[0].mf).OrderByDescending(g=>g.Count()).First().Take(12).ToList();
   var tmf=templates[0].pieces[0].mf;rep.AppendLine($"planting templates: {templates.Count} trees from {P(tmf.transform)}");
   var keepSites=sites.Where(s=>!s.remove).Select(s=>s.bottom).ToList();var plant=new List<(Vector3 p,string why)>();
   bool Free(Vector3 p,float r)=>!keepSites.Any(q=>Flat(q,p)<r)&&!plant.Any(q=>Flat(q.p,p)<r);
   // inside of each kink half: the curvature peaks in s 38-70 and 110-142
   foreach(var (a0,a1) in new[]{(55f,90f),(125f,158f)}){var peaks=new List<(float s,float k)>();for(float s=a0;s<=a1;s+=.5f){At(s-2,out var f0);At(s+2,out var f1);float k=Vector3.SignedAngle(new Vector3(f0.x,0,f0.z),new Vector3(f1.x,0,f1.z),Vector3.up);peaks.Add((s,k));}
    foreach(int sign in new[]{-1,1}){var pk=peaks.Where(x=>Mathf.Sign(x.k)==sign).OrderByDescending(x=>Mathf.Abs(x.k)).FirstOrDefault();if(pk.k==0)continue;var c=At(pk.s,out var f);var p=c+Right(f)*sign*(HW+1.25f);p.y=Ground(p,c.y);if(Free(p,2.2f)&&MainW(p)>=1)plant.Add((p,$"inside of kink at s {pk.s:F1}"));}}
   for(float s=30;s<=158;s+=7f)foreach(int side in new[]{-1,1}){var c=At(s+side*1.7f,out var f);var p=c+Right(f)*side*(HW+1.35f+.5f*H((int)s,side+20));p.y=Ground(p,c.y);if(float.IsNaN(p.y)||MainW(p)<1||!Free(p,4.6f))continue;plant.Add((p,$"edge gap at s {s:F0}"));}
   if(!dry){var tm=tmf.sharedMesh;var tv=tm.vertices;var tc=tm.colors;var tt=tm.triangles;var nvv=new List<Vector3>();var ncc=new List<Color>();var ntt=new List<int>();var trunks=new GameObject("Summit Climb edge tree trunks").transform;trunks.SetParent(root);
    for(int k=0;k<plant.Count;k++){var tpl=templates[k%templates.Count];var off=plant[k].p-tpl.bottom;var map=new Dictionary<int,int>();
     foreach(var pc in tpl.pieces.Where(p=>p.mf==tmf)){foreach(var i in pc.idx){map[i]=nvv.Count;nvv.Add(tmf.transform.TransformPoint(tv[i])+off);ncc.Add(tc.Length==tv.Length?tc[i]:Color.white);}}
     for(int i=0;i<tt.Length;i+=3)if(map.ContainsKey(tt[i])&&map.ContainsKey(tt[i+1])&&map.ContainsKey(tt[i+2]))ntt.AddRange(new[]{map[tt[i]],map[tt[i+1]],map[tt[i+2]]});
     var b=(BoxCollider)tpl.col;var go=new GameObject("Tree trunk");go.transform.SetParent(trunks);go.layer=b.gameObject.layer;go.transform.SetPositionAndRotation(b.transform.position+off,b.transform.rotation);go.transform.localScale=b.transform.lossyScale;var nb=go.AddComponent<BoxCollider>();nb.center=b.center;nb.size=b.size;
     float ss=Local(plant[k].p,out float lat,out _);rep.AppendLine($"  plant tree at {V(plant[k].p)} (branch s {ss:F1}, lateral {lat:+0.0;-0.0}; {plant[k].why}) from the tree at {V(tpl.bottom)}");}
    var pm=new Mesh{indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};pm.SetVertices(nvv);pm.SetColors(ncc);pm.SetTriangles(ntt,0);pm.RecalculateNormals();pm.RecalculateBounds();pm=Store(pm,"LakeWoods-summit-edge-trees");
    var pg=new GameObject("Summit Climb edge trees",typeof(MeshFilter),typeof(MeshRenderer));pg.transform.SetParent(root);pg.GetComponent<MeshFilter>().sharedMesh=pm;var tr0=tmf.GetComponent<MeshRenderer>();var pr=pg.GetComponent<MeshRenderer>();pr.sharedMaterial=tr0.sharedMaterial;pr.shadowCastingMode=tr0.shadowCastingMode;}
   rep.AppendLine($"edge trees planted {plant.Count}");

   // ---- 6. the branch
   var go2=new GameObject("Summit Climb route");go2.transform.SetParent(root);var br=go2.AddComponent<WoodlandRoute>();br.title="Summit Climb";br.points=pts.ToArray();br.halfWidth=HW;br.recommendedSpeed=30;br.entryInset=24;br.entryMargin=1.5f;
   br.entryRoad=main.Project(pts[0],out _);br.exitRoad=main.Project(pts[^1],out _);br.bypassedGates=Enumerable.Range(1,race.gates.Length-1).Where(i=>{float s=main.Project(race.gates[i].transform.position,out _);return s>br.entryRoad&&s<br.exitRoad;}).ToArray();br.aiValidated=true;
   go2.AddComponent<ReverseShortcutGuidance>().lookAhead=8;br.Initialize();
   rep.AppendLine($"branch Summit Climb: entryRoad {br.entryRoad:F1}, exitRoad {br.exitRoad:F1}, length {br.Length:F1}, bypasses gate(s) {string.Join(",",br.bypassedGates)}, halfWidth {HW}, recommendedSpeed 30, AI on");

   // ---- 7. sign at the fork: a copy of the Echo Cave sign, at the same place relative to its own fork
   var echo=roots.SelectMany(g=>g.GetComponentsInChildren<WoodlandRoute>(true)).First(b=>b.title=="Echo Cave");
   var echoSign=roots.SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).First(t=>t.name.StartsWith("Echo Cave <")&&t.GetComponent<PhysicalSign>());
   {var ea=main.At(echo.entryRoad,out var ef);var ma=main.At(br.entryRoad,out var mfw);var eq=Quaternion.LookRotation(new Vector3(ef.x,0,ef.z));var mq=Quaternion.LookRotation(new Vector3(mfw.x,0,mfw.z));
    var relR=Quaternion.Inverse(eq)*echoSign.rotation;float eg=Ground(echoSign.position,echoSign.position.y);
    // on the left of the main 6 m before the fork, 8.5 m from the centre line, turned to the main as the Echo Cave sign is to its own
    var left=-Vector3.Cross(Vector3.up,new Vector3(mfw.x,0,mfw.z).normalized);var np=main.At(br.entryRoad-6,out _)+left*8.5f;np.y=Ground(np,ma.y)+(echoSign.position.y-eg);
    var sign=Object.Instantiate(echoSign.gameObject,echoSign.parent);sign.name="Summit Climb <\n\nOPTIONAL / FORWARD";sign.transform.SetPositionAndRotation(np,mq*relR);
    foreach(var t in sign.GetComponentsInChildren<TextMesh>(true))t.text=t.text.Replace("Echo Cave","SUMMIT CLIMB").Replace("ECHO CAVE","SUMMIT CLIMB");
    rep.AppendLine($"sign: copy of {P(echoSign)} at {V(np)} (relative to the fork as the Echo Cave sign is to its fork, on the left); text: {string.Join(" | ",sign.GetComponentsInChildren<TextMesh>(true).Select(t=>t.text.Replace("\n","/")))}");}

   // ---- 8. arrows (decals as RouteAtlasPass.Guidance), entrance and edge markers
   var guide=roots.First(g=>g.name=="Route atlas direction guidance").transform;var placed=guide.GetComponentsInChildren<Renderer>(true).Select(r=>r.bounds.center).ToList();int arrows=0;
   bool Paint(Vector3 p,Vector3 heading,bool optional,string label){if(placed.Any(q=>Vector3.Distance(q,p)<(optional?9:18)))return false;var f=Vector3.ProjectOnPlane(heading,Vector3.up).normalized;var right=Vector3.Cross(Vector3.up,f);float w=optional?1:1.5f;
    var shape=new[]{new Vector2(-w*.4f,-3),new Vector2(w*.4f,-3),new Vector2(w*.4f,0),new Vector2(w,0),new Vector2(0,3),new Vector2(-w,0),new Vector2(-w*.4f,0)};var vs=new List<Vector3>();
    foreach(var v in shape){var q=p+right*v.x+f*v.y;var hits=Physics.RaycastAll(q+Vector3.up*5,Vector3.down,10,~0,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name.StartsWith("Ground")).OrderBy(h=>Math.Abs(h.point.y-q.y)).ToArray();if(hits.Length==0)return false;q.y=hits[0].point.y+.075f;vs.Add(q);}
    var mesh=new Mesh{vertices=vs.ToArray(),triangles=new[]{0,6,1,1,6,2,6,5,4,6,4,2,2,4,3}};mesh.RecalculateNormals();mesh.RecalculateBounds();if(!dry)mesh=Store(mesh,"LakeWoods-summit-arrow");
    var g=new GameObject((optional?"Optional gold / ":"Main teal / ")+label,typeof(MeshFilter),typeof(MeshRenderer));g.transform.SetParent(guide);g.GetComponent<MeshFilter>().sharedMesh=mesh;g.GetComponent<Renderer>().sharedMaterial=optional?gold:teal;placed.Add(p);arrows++;rep.AppendLine($"  arrow {g.name} at {V(p)}");return true;}
   foreach(var (s,l) in new[]{(br.entryRoad-24,"before Summit Climb fork"),(br.entryRoad+22,"main at Summit Climb fork"),(br.exitRoad+24,"Summit Climb rejoin")}){var p=main.At(s,out var f);Paint(p,f,false,l);}
   foreach(float s in new[]{30f,80f,118f,150f,br.Length-22}){var p=br.At(s,out var f);Paint(p,f,true,"Summit Climb");}
   GameObject Post(string n,Vector3 p,float h){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=n;g.transform.SetParent(root);Object.DestroyImmediate(g.GetComponent<Collider>());g.transform.position=p+Vector3.up*h*.5f;g.transform.localScale=new Vector3(.2f,h,.2f);g.GetComponent<Renderer>().sharedMaterial=gold;return g;}
   int posts=0;foreach(float s in new[]{27f,31f}){var c=At(s,out var f);foreach(int side in new[]{-1,1}){var p=c+Right(f)*side*(HW+.9f);p.y=Ground(p,c.y);if(MainW(p)<1)continue;Post("Summit Climb shortcut entrance marker",p,2);posts++;}}
   for(float s=50;s<=158;s+=27){var c=At(s,out var f);foreach(int side in new[]{-1,1}){var p=c+Right(f)*side*(HW+.75f);p.y=Ground(p,c.y);Post("Summit Climb shortcut edge marker",p,.9f);posts++;}}
   rep.AppendLine($"arrows painted {arrows}; markers {posts} (no colliders)");

   // ---- 9. course id
   rep.AppendLine($"course id {race.courseId} -> lake-v8-summit-climb");race.courseId="lake-v8-summit-climb";
   if(!dry){EditorUtility.SetDirty(race);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();rep.AppendLine("saved");}else rep.AppendLine("DRY RUN: nothing saved");
  }catch(Exception e){rep.AppendLine("ERROR "+e);}
  File.WriteAllText(o+"/B-summit-author.txt",rep.ToString());EditorApplication.Exit(0);}
}
