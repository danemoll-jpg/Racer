using System;using System.IO;using System.Linq;using System.Text;using System.Collections.Generic;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;using Racer;using Object=UnityEngine.Object;
// 0.88 Part A: Kyle's place ("Friend across street - blue circle", the same site in all nine scenes). House frame = the
// site's transform (x right = north, the house's left; z forward = the front, west, toward the street). The house model is
// not touched. In each scene:
//  - one straight gravel driveway from the street edge at Dan's BUG-002 position to the 0.87 garage apron (7.6 x 6.8 m at
//    the pad), one width, an even grade with short rounded ends; the ground under it cut / filled, batters 1:1.5 / 1:2;
//  - every other drive on the property removed (the old drive ribbon, the 0.87 garage leg, the CR113 gravel edge join) and
//    the ground under the old drive and the gravel smoothed to grass (a harmonic fill from the ground round it);
//  - a flat lawn in front of the house two steps (2 x 0.17 m) below the porch floor, filling the hollow at the foot of the
//    bank up to where the bank rises above it; one broad step between lawn and porch;
//  - trees in the drive's line re-planted beside it, more trees planted on its left (north) where the old drive was;
//    trees and objects on changed ground re-seated;
//  - a mailbox (the CR-015 roadside mailbox, breakaway trigger) at the mouth on the verge, left of the drive;
//  - the drive's route points and the map's "Anderson's" arrival follow; the Dan-and-Kyle vignette moves off the drive line
//    to the verge beside the mouth.
// Ground tiles shared between scenes are edited once; later scenes sample the original ground from a kept copy.
// KYLE_DRY=1 reports only. KYLE_SCENES=a,b limits the scenes.
public static class Report088KyleBuild {
 const string Site="Friend across street - blue circle",Dir="Assets/Scenery/KylesHouse";
 static readonly string[] Scenes={"FreeRoamWorld","DansBackyardReverse","DansBackyardForward","StreetLoopGreybox","StreetLoopReverse","LakeWoods","ForestLoopReverse","MountainLoop","MountainLoopReverse"};
 const float YL=2.31f,HW=2.4f,CORE=HW+1.6f,CURVE=1.8f,ST=.5f,GX0=-30,GX1=48,GZ0=-16,GZ1=44;const int NX=(int)((GX1-GX0)/ST)+1,NZ=(int)((GZ1-GZ0)/ST)+1;
 static readonly StringBuilder rep=new();static readonly HashSet<string> edited=new();static readonly Dictionary<string,(Vector3[] v,int[] t)> originals=new();
 static string V(Vector3 v)=>$"{v.x:F2},{v.y:F2},{v.z:F2}";
 static string P(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n;}
 static bool TreeName(string n){n=n.ToLowerInvariant();return n.Contains("trunk")||n.Contains("tree");}
 static bool Veg(Renderer r){var m=r.sharedMaterial;return m&&m.shader&&m.shader.name=="Racer/GreyboxGround"&&m.HasProperty("_Vegetation")&&m.GetFloat("_Vegetation")>.5f;}
 static float Out(float x,float z,(float x0,float x1,float z0,float z1) r){float dx=Mathf.Max(r.x0-x,0,x-r.x1),dz=Mathf.Max(r.z0-z,0,z-r.z1);return Mathf.Sqrt(dx*dx+dz*dz);}
 static readonly (float x0,float x1,float z0,float z1)[] Footprint={(-7.6f,7.6f,-4.6f,4.6f),(2.2f,7.6f,4.6f,6.4f),(-7.6f,.4f,4.6f,6.5f),(-11.4f,-7.6f,-4.2f,1.45f),(-8.6f,-7.6f,1.75f,3.35f),(-4.9f,1.9f,-8.4f,-4.6f),(-9.3f,-4.9f,-8.4f,-7.3f)};
 static readonly (float x0,float x1,float z0,float z1) Apron=(7.6f,15.2f,-5.2f,1.6f),T1=(-12.6f,8.6f,4.6f,9.6f),T2=(-12.6f,-7.6f,1.6f,4.6f),Lawn=(-13f,12f,6.5f,24f),Step=(-5.3f,-1.3f,6.5f,6.95f);
 // the drive (house frame): mouth M at the street edge, end E at the apron's front edge; profile in metres above the pad
 static Vector3 M,E,F,Left;static float Ld,yM,grade,cross0;
 // the drive surface: the profile on the centre line, plus the street edge's cross-fall at the mouth fading out over 8 m
 static float Surf(float s,float lat){float f=1-Mathf.SmoothStep(0,1,Mathf.Clamp01(s/8f));return Prof(s)+lat*cross0*f;}
 static float Prof(float s){s=Mathf.Clamp(s,0,Ld);if(s<CURVE)return yM-grade*s*s/(2*CURVE);if(s>Ld-CURVE){float u=Ld-s;return grade*u*u/(2*CURVE);}return yM-grade*CURVE*.5f-grade*(s-CURVE);}
 static float DriveLocal(float x,float z,out float lat){var d=new Vector3(x-M.x,0,z-M.z);float s=Vector3.Dot(d,F);lat=Vector3.Dot(d,Left);return s;}
 static float[,] streetGap;static bool[,] zold;static float[,] H0,H1,D;
 static int IX(float x)=>Mathf.RoundToInt((x-GX0)/ST);static int IZ(float z)=>Mathf.RoundToInt((z-GZ0)/ST);static float CX(int i)=>GX0+i*ST;static float CZ(int k)=>GZ0+k*ST;
 static float Bil(float[,] g,float x,float z){float fx=(x-GX0)/ST,fz=(z-GZ0)/ST;int i=Mathf.FloorToInt(fx),k=Mathf.FloorToInt(fz);if(i<0||k<0||i>=NX-1||k>=NZ-1)return 0;float u=fx-i,w=fz-k;return Mathf.Lerp(Mathf.Lerp(g[i,k],g[i+1,k],u),Mathf.Lerp(g[i,k+1],g[i+1,k+1],u),w);}
 static bool InFootprint(float x,float z,float m){foreach(var r in Footprint)if(Out(x,z,r)<m)return true;return false;}
 public static void Run(){bool dry=Environment.GetEnvironmentVariable("KYLE_DRY")=="1";var o=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(o);
  foreach(var kv in (Environment.GetEnvironmentVariable("KYLE_ORIG")??"").Split(';').Where(x=>x.Contains('='))){var a=kv.Split('=');var m=AssetDatabase.LoadAssetAtPath<Mesh>(a[1]);if(!m)throw new Exception("no original "+a[1]);originals[a[0]]=(m.vertices,m.triangles);edited.Add(a[0]);rep.AppendLine($"original of {a[0]} (already edited): {a[1]}, {m.vertexCount} vertices");}
  try{var only=Environment.GetEnvironmentVariable("KYLE_SCENES");foreach(var sn in only!=null?only.Split(','):Scenes)Scene(sn,dry);}
  catch(Exception e){rep.AppendLine("ERROR "+e);}
  File.WriteAllText(o+"/A-kyle-build.txt",rep.ToString());EditorApplication.Exit(0);}
 sealed class Piece{public MeshFilter mf;public int[] idx;public Bounds b;public bool bark;public int owner=-1;}
 sealed class Tree{public Vector3 bottom;public Collider col;public float r,h;public bool remove;public Vector3 move;public float dy;public List<Piece> pieces=new();public string why;}
 static void Scene(string sn,bool dry){
  rep.AppendLine($"== {sn}");var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+sn+".unity");Physics.SyncTransforms();var roots=scene.GetRootGameObjects();var all=roots.SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).ToList();
  var site=all.FirstOrDefault(t=>t.name==Site);if(!site){rep.AppendLine("  no house");return;}
  if(Vector3.Distance(site.position,new Vector3(511.5f,78.66718f,-35.2f))>.01f)throw new Exception("site moved in "+sn);
  if(all.Any(t=>t.name=="Ground_Kyle driveway (0.88)"))throw new Exception("already built in "+sn);
  Vector3 L(Vector3 w)=>site.InverseTransformPoint(w);Vector3 W(Vector3 l)=>site.TransformPoint(l);
  var regionW=new Bounds(W(new Vector3(9,0,14)),new Vector3(95,80,95));
  // ---- what goes: the drive ribbons and the gravel edge join
  var driveRoad=roots.SelectMany(g=>g.GetComponentsInChildren<RaceRoad>(true)).FirstOrDefault(r=>r.name=="Kyle descending driveway");if(!driveRoad)throw new Exception("no Kyle driveway road in "+sn);
  var gone=roots.SelectMany(g=>g.GetComponentsInChildren<MeshCollider>(true)).Where(c=>c.name=="Ground_Kyle descending drive"||c.name=="Ground_Kyle garage drive (0.87)"||c.name=="Ground_CR113 Kyle driveway edge join").ToList();
  rep.AppendLine($"  removed surfaces: {string.Join("; ",gone.Select(c=>$"{P(c.transform)} [{AssetDatabase.GetAssetPath(c.sharedMesh)}] {c.GetComponent<Renderer>()?.sharedMaterial?.name}"))}");
  var gravelMat=gone.Select(c=>c.GetComponent<MeshRenderer>()?.sharedMaterial).FirstOrDefault(m=>m&&m.name=="Driveway gravel")??AssetDatabase.FindAssets("Driveway gravel t:Material").Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<Material>).First();
  var driveLayer=gone.Count>0?gone[0].gameObject.layer:site.gameObject.layer;bool driveStatic=gone.Count>0&&gone[0].gameObject.isStatic;
  // ---- the street (its surface is the ground tile here); gap = distance outside its half width
  var street=roots.SelectMany(g=>g.GetComponentsInChildren<RaceRoad>(true)).First(r=>r.name=="Phase 3 - Race Systems");street.Initialize();
  var ss=new List<(Vector2 p,float hw)>();for(float st=0;st<street.Length;st+=.5f){var q=street.At(st,out _);if(Vector2.Distance(new(q.x,q.z),new(site.position.x,site.position.z))<110)ss.Add((new Vector2(q.x,q.z),street.HalfWidth(st)));}
  float Gap(Vector3 l){var w=W(l);var p=new Vector2(w.x,w.z);float best=1e9f,hw=4.5f;for(int i=1;i<ss.Count;i++){var a=ss[i-1].p;var ab=ss[i].p-a;if(ab.sqrMagnitude<1e-6f||ab.sqrMagnitude>4)continue;float t=Mathf.Clamp01(Vector2.Dot(p-a,ab)/ab.sqrMagnitude);float d=(p-a-ab*t).magnitude;if(d<best){best=d;hw=ss[i].hw;}}return best-hw;}
  streetGap=new float[NX,NZ];for(int i=0;i<NX;i++)for(int k=0;k<NZ;k++)streetGap[i,k]=Gap(new Vector3(CX(i),0,CZ(k)));
  // ---- ground tiles (the "Memory loop" tiles in the region); sampling uses each edited asset's original mesh
  var tiles=roots.SelectMany(g=>g.GetComponentsInChildren<MeshCollider>(true)).Where(c=>c.name.StartsWith("Ground_")&&c.sharedMesh&&c.transform.parent&&c.transform.parent.name.StartsWith("Memory loop")&&c.bounds.Intersects(regionW)).ToList();
  // sampling: each tile's own triangles (the original mesh for a tile already edited for an earlier scene)
  var samplers=tiles.Select(c=>{var p=AssetDatabase.GetAssetPath(c.sharedMesh);return originals.TryGetValue(p,out var om)?new Sampler(om.v,om.t,c.transform,site):new Sampler(c.sharedMesh.vertices,c.sharedMesh.triangles,c.transform,site);}).ToList();int fromOrig=tiles.Count(c=>originals.ContainsKey(AssetDatabase.GetAssetPath(c.sharedMesh)));
  float Tile(Vector3 l){float best=float.NaN;foreach(var sm in samplers){float y=sm.Height(l.x,l.z);if(!float.IsNaN(y)&&(float.IsNaN(best)||y>best))best=y;}return best;}
  H0=new float[NX,NZ];int missing=0;for(int i=0;i<NX;i++)for(int k=0;k<NZ;k++){H0[i,k]=Tile(new Vector3(CX(i),0,CZ(k)));if(float.IsNaN(H0[i,k]))missing++;}
  // drive geometry
  var B=L(new Vector3(477.65f,0,-21.06f));B.y=0;var A=new Vector3(11.4f,0,-1.8f);F=(A-B).normalized;Left=new Vector3(-F.z,0,F.x);
  float tE=(Apron.z1-B.z)/F.z;E=B+F*tE;float tM=0;if(Gap(B)<0)throw new Exception("BUG-002 point on the street");for(float t=0;t>-14;t-=.01f){if(Gap(B+F*t)<0){tM=t+.01f;break;}}M=B+F*tM;Ld=tE-tM;yM=Tile(M);grade=yM/(Ld-CURVE);
  {Vector3 Edge(float side){var q=M+Left*side*HW;for(float b=-3;b<3;b+=.02f)if(Gap(q+F*b)>=0)return q+F*b;return q;}var eL=Edge(1);var eR=Edge(-1);cross0=(Tile(eL)-Tile(eR))/(2*HW);rep.AppendLine($"  street edge at the mouth: left {Tile(eL):F2}, right {Tile(eR):F2}: the drive takes a cross-fall of {cross0*100:F1} % there, level by 8 m");}
  rep.AppendLine($"  ground cells without a tile hit: {missing}; tiles: {string.Join("; ",tiles.Select(c=>$"{c.name} [{AssetDatabase.GetAssetPath(c.sharedMesh)}]"))}{(fromOrig>0?$" (sampled from the original of {fromOrig} tile(s) already edited for an earlier scene)":"")}");
  rep.AppendLine($"  drive: mouth M local {V(M)} (world {V(W(M))}, street gap {Gap(M):F2}) at {yM:F2} m above the pad; end E local {V(E)} at the apron edge; length {Ld:F1} m, width {2*HW} m, heading {Mathf.Atan2(W(E).x-W(M).x,W(E).z-W(M).z)*Mathf.Rad2Deg:F0} deg; grade {grade*100:F1} % ({Mathf.Atan(grade)*Mathf.Rad2Deg:F1} deg) with {CURVE} m rounded ends");
  // gravel mask (before removal) and the old drive line
  var oldPts=driveRoad.points.Select(L).ToArray();
  float OldD(float x,float z){float best=1e9f;for(int i=1;i<oldPts.Length;i++){var a=oldPts[i-1];var b=oldPts[i];var ab=new Vector2(b.x-a.x,b.z-a.z);if(ab.sqrMagnitude<1e-4f)continue;var ap=new Vector2(x-a.x,z-a.z);float t=Mathf.Clamp01(Vector2.Dot(ap,ab)/ab.sqrMagnitude);best=Mathf.Min(best,(ap-ab*t).magnitude);}return best;}
  var gravel=new bool[NX,NZ];for(int i=0;i<NX;i++)for(int k=0;k<NZ;k++){var w=W(new Vector3(CX(i),0,CZ(k)));var ray=new Ray(new Vector3(w.x,w.y+70,w.z),Vector3.down);foreach(var c in gone)if(c.Raycast(ray,out _,160)){gravel[i,k]=true;break;}}
  zold=new bool[NX,NZ];int nz=0;int dil=Mathf.CeilToInt(2.5f/ST);
  for(int i=0;i<NX;i++)for(int k=0;k<NZ;k++){float x=CX(i),z=CZ(k);if(streetGap[i,k]<1f||InFootprint(x,z,.3f)||float.IsNaN(H0[i,k]))continue;bool g=OldD(x,z)<=6.5f;
   if(!g)for(int a=-dil;a<=dil&&!g;a++)for(int b=-dil;b<=dil&&!g;b++){int ii=i+a,kk=k+b;if(ii<0||kk<0||ii>=NX||kk>=NZ||a*a+b*b>dil*dil)continue;if(gravel[ii,kk])g=true;}
   if(g){zold[i,k]=true;nz++;}}
  // ---- target ground
  H1=(float[,])H0.Clone();
  var lapG=(float[,])H0.Clone();{var lap=lapG;for(int it=0;it<4000;it++)for(int i=1;i<NX-1;i++)for(int k=1;k<NZ-1;k++){if(!zold[i,k])continue;float avg=(lap[i-1,k]+lap[i+1,k]+lap[i,k-1]+lap[i,k+1])*.25f;if(float.IsNaN(avg))continue;lap[i,k]+=1.8f*(avg-lap[i,k]);}
   for(int i=0;i<NX;i++)for(int k=0;k<NZ;k++)if(zold[i,k])H1[i,k]=lap[i,k];}
  float Target(float x,float z,float y){
   // lawn: the old terrace rectangles exactly at YL; the lawn box filled up to YL; the terrace batter (fill only) beyond
   if(Out(x,z,T1)<=0||Out(x,z,T2)<=0)y=YL;else if(Out(x,z,Lawn)<=0)y=Mathf.Max(y,YL);else{float dT=Mathf.Min(Out(x,z,T1),Out(x,z,T2));if(dT<5.2f&&YL-.5f*dT>y)y=YL-.5f*dT;}
   // apron at the pad
   float dA=Out(x,z,Apron)-1f;if(dA<=0)y=0;else if(dA<7){float hi=.67f*dA,lo=-.5f*dA;y=Mathf.Clamp(y,lo,hi);}
   // the drive
   float s=DriveLocal(x,z,out float lat);if(s>=-.5f&&s<=Ld+.5f){float al=Mathf.Abs(lat);float ys=Surf(s,Mathf.Clamp(lat,-CORE,CORE))-.03f;if(al<=CORE)y=ys;else if(al<CORE+7){float e=al-CORE;y=Mathf.Clamp(y,ys-.5f*e,ys+.67f*e);}}
   return y;}
  float TargetAt(float x,float z,float y0){int i=IX(x),k=IZ(z);if(i<0||k<0||i>=NX||k>=NZ)return y0;if(streetGap[i,k]<.5f)return y0;float y=zold[i,k]?Bil(lapG,x,z):y0;return Target(x,z,y);}
  for(int i=0;i<NX;i++)for(int k=0;k<NZ;k++){if(float.IsNaN(H0[i,k])||streetGap[i,k]<.5f)continue;H1[i,k]=Target(CX(i),CZ(k),H1[i,k]);}
  D=new float[NX,NZ];float cut=0,fill=0,mx=0;for(int i=0;i<NX;i++)for(int k=0;k<NZ;k++){if(float.IsNaN(H0[i,k])){D[i,k]=0;continue;}D[i,k]=H1[i,k]-H0[i,k];if(Mathf.Abs(D[i,k])<.004f)D[i,k]=0;if(D[i,k]>0)fill+=D[i,k]*ST*ST;else cut-=D[i,k]*ST*ST;mx=Mathf.Max(mx,Mathf.Abs(D[i,k]));}
  rep.AppendLine($"  ground: cleaned zone (old drive and gravel, smoothed) {nz*ST*ST:F0} m2; fill {fill:F0} m3, cut {cut:F0} m3, largest change {mx:F2} m");
  {var sb=new StringBuilder("  drive profile (s: drive / ground before):");for(float s=0;s<=Ld;s+=3){var q=M+F*s;sb.Append($" {s:F0}:{Prof(s):F2}/{Bil(H0,q.x,q.z):F2}");}rep.AppendLine(sb.ToString());}
  {var sb=new StringBuilder("  lawn (z: ground before -> after at x -4 / 4):");for(float z=7;z<=24;z+=2)sb.Append($" {z:F0}:{Bil(H0,-4,z):F2}->{Bil(H1,-4,z):F2}/{Bil(H0,4,z):F2}->{Bil(H1,4,z):F2}");rep.AppendLine(sb.ToString());}
  // ---- race data near: gates, branches, activities
  foreach(var d in roots.SelectMany(g=>g.GetComponentsInChildren<RaceDirector>(true))){float gmin=1e9f;string gn="";foreach(var g in d.gates){float dd=Vector2.Distance(new(g.transform.position.x,g.transform.position.z),new(W(M).x,W(M).z));if(dd<gmin){gmin=dd;gn=g.name;}}
   rep.AppendLine($"  course {d.courseId}: nearest gate to the mouth {gmin:F1} m ({gn}); street gap at the mouth {Gap(M):F2} m");}
  foreach(var b in roots.SelectMany(g=>g.GetComponentsInChildren<WoodlandRoute>(true))){b.Initialize();float bb=1e9f;foreach(var q in b.points){var l=L(q);float s=DriveLocal(l.x,l.z,out float lat);s=Mathf.Clamp(s,0,Ld);var c=M+F*s;bb=Mathf.Min(bb,Vector2.Distance(new(l.x,l.z),new(c.x,c.z)));}if(bb<60)rep.AppendLine($"  branch {b.title} passes {bb:F1} m from the drive line");}
  foreach(var a in roots.SelectMany(g=>g.GetComponentsInChildren<ActivitySite>(true))){var l=L(a.transform.position);float s=DriveLocal(l.x,l.z,out float lat);s=Mathf.Clamp(s,0,Ld);var c=M+F*s;float dd=Vector2.Distance(new(l.x,l.z),new(c.x,c.z));if(dd<120)rep.AppendLine($"  activity '{a.title}' centre {dd:F1} m from the drive line (radius {a.radius}); props nearest {(a.props==null||a.props.Length==0?"-":a.props.Where(p=>p).Min(p=>{var pl=L(p.transform.position);float ps=Mathf.Clamp(DriveLocal(pl.x,pl.z,out _),0,Ld);var pc=M+F*ps;return Vector2.Distance(new(pl.x,pl.z),new(pc.x,pc.z));}).ToString("F1"))} m");}
  var otherRoads=roots.SelectMany(g=>g.GetComponentsInChildren<RaceRoad>(true)).Where(r=>r!=driveRoad&&r!=street).Select(r=>r.points.Select(L).ToArray()).Concat(roots.SelectMany(g=>g.GetComponentsInChildren<WoodlandRoute>(true)).Select(b=>b.points.Select(L).ToArray())).ToList();
  float RoadD(Vector3 l){float best=1e9f;foreach(var pts in otherRoads)for(int i=1;i<pts.Length;i++){var a=pts[i-1];var b=pts[i];if(Mathf.Abs(a.x-l.x)>60&&Mathf.Abs(b.x-l.x)>60)continue;var ab=new Vector2(b.x-a.x,b.z-a.z);if(ab.sqrMagnitude<1e-4f)continue;var ap=new Vector2(l.x-a.x,l.z-a.z);float t=Mathf.Clamp01(Vector2.Dot(ap,ab)/ab.sqrMagnitude);best=Mathf.Min(best,(ap-ab*t).magnitude);}return best;}
  // ---- mailbox and vignette spots (left of the mouth, on the verge)
  Vector3 mail=M+Left*(HW+1.3f);for(float s=0;s<4;s+=.05f){var q=M+F*s+Left*(HW+1.3f);if(Gap(q)>=1.0f){mail=q;break;}}
  var vigA=M+F*4.2f+Left*(HW+4.6f);var vigB=M+F*5.6f+Left*(HW+6.8f);
  rep.AppendLine($"  mailbox at local {V(mail)} (street gap {Gap(mail):F2} m, {HW+1.3f-HW:F1} m off the drive edge); vignette spots local {V(vigA)} / {V(vigB)}");
  // ---- trees: sites with their drawn pieces
  Func<Vector3,float> Gf=l=>Bil(H1,l.x,l.z);Func<Vector3,float> Dy=l=>Bil(D,l.x,l.z);
  var vegs=roots.SelectMany(g=>g.GetComponentsInChildren<MeshRenderer>(true)).Where(r=>Veg(r)&&r.bounds.Intersects(regionW)).Select(r=>r.GetComponent<MeshFilter>()).Where(m=>m&&m.sharedMesh&&m.sharedMesh.isReadable).ToList();
  var pieces=new List<Piece>();
  foreach(var mf in vegs){var m=mf.sharedMesh;var wv=m.vertices.Select(x=>mf.transform.TransformPoint(x)).ToArray();var par=Enumerable.Range(0,wv.Length).ToArray();int Fd(int x){while(par[x]!=x){par[x]=par[par[x]];x=par[x];}return x;}void U(int a,int c){a=Fd(a);c=Fd(c);if(a!=c)par[a]=c;}
   var tri=m.triangles;for(int k=0;k<tri.Length;k+=3){U(tri[k],tri[k+1]);U(tri[k],tri[k+2]);}var byPos=new Dictionary<Vector3Int,int>();for(int i=0;i<wv.Length;i++){var key=Vector3Int.RoundToInt(wv[i]*200);if(byPos.TryGetValue(key,out var j))U(i,j);else byPos[key]=i;}
   var used=new HashSet<int>(tri);foreach(var g in Enumerable.Range(0,wv.Length).Where(used.Contains).GroupBy(Fd)){var idx=g.ToArray();var b=new Bounds(wv[idx[0]],Vector3.zero);foreach(var i in idx)b.Encapsulate(wv[i]);var l=L(b.center);if(l.x<GX0+2||l.x>GX1-2||l.z<GZ0+2||l.z>GZ1-2)continue;pieces.Add(new Piece{mf=mf,idx=idx,b=b,bark=b.size.x<1.3f&&b.size.z<1.3f&&b.size.y>1.8f});}}
  var trees=new List<Tree>();
  foreach(var c in roots.SelectMany(g=>g.GetComponentsInChildren<Collider>(true))){if(!TreeName(c.name)||c.isTrigger||!c.bounds.Intersects(regionW))continue;var b=c.bounds;var l=L(new Vector3(b.center.x,b.min.y,b.center.z));if(l.x<GX0+2||l.x>GX1-2||l.z<GZ0+2||l.z>GZ1-2)continue;trees.Add(new Tree{bottom=new Vector3(b.center.x,b.min.y,b.center.z),col=c,r=Mathf.Max(b.extents.x,b.extents.z),h=b.size.y});}
  foreach(var p in pieces.Where(p=>p.bark)){var bt=new Vector3(p.b.center.x,p.b.min.y,p.b.center.z);if(trees.Any(s=>s.col&&new Vector2(s.bottom.x-bt.x,s.bottom.z-bt.z).magnitude<.6f))continue;trees.Add(new Tree{bottom=bt,r=Mathf.Max(p.b.extents.x,p.b.extents.z),h=p.b.size.y});}
  foreach(var p in pieces){var c=new Vector3(p.b.center.x,p.b.min.y,p.b.center.z);int best=-1;float bd=p.bark?.8f:6f;for(int i=0;i<trees.Count;i++){var s=trees[i];if(!(s.bottom.y+s.h*1.6f+1>=p.b.min.y&&s.bottom.y<p.b.max.y+1))continue;float d=new Vector2(s.bottom.x-c.x,s.bottom.z-c.z).magnitude;if(d<bd){bd=d;best=i;}}p.owner=best;if(best>=0)trees[best].pieces.Add(p);}
  bool InDrive(Vector3 l,float m){float s=DriveLocal(l.x,l.z,out float lat);return s>-1.5f&&s<Ld+1&&Mathf.Abs(lat)<HW+m;}
  bool Blocked(Vector3 l,float m)=>InFootprint(l.x,l.z,m)||Out(l.x,l.z,Apron)<m||Out(l.x,l.z,Lawn)<m*.5f||Out(l.x,l.z,T2)<m||InDrive(l,m+1.2f)||streetGap[Mathf.Clamp(IX(l.x),0,NX-1),Mathf.Clamp(IZ(l.z),0,NZ-1)]<2.5f||RoadD(l)<2.5f+m*.5f||new Vector2(l.x-mail.x,l.z-mail.z).magnitude<3||new Vector2(l.x-vigA.x,l.z-vigA.z).magnitude<3||new Vector2(l.x-vigB.x,l.z-vigB.z).magnitude<3||l.x<GX0+4||l.x>GX1-4||l.z<GZ0+4||l.z>GZ1-4;
  int relocated=0,seated=0;var spots=new List<Vector3>();
  foreach(var s in trees){var l=L(s.bottom);if(InDrive(l,1.6f+s.r)||Out(l.x,l.z,Apron)<1.2f+s.r||InFootprint(l.x,l.z,1.0f+s.r)){s.remove=true;s.why="in the drive's line";}}
  foreach(var s in trees.Where(t=>!t.remove))spots.Add(L(s.bottom));
  foreach(var s in trees.Where(t=>t.remove).OrderBy(t=>{var l=L(t.bottom);return DriveLocal(l.x,l.z,out _);})){var l=L(s.bottom);float s0=Mathf.Clamp(DriveLocal(l.x,l.z,out float lat0),2,Ld-4);bool found=false;
   foreach(float ds in new[]{0f,2.5f,-2.5f,5f,-5f,7.5f,-7.5f})foreach(float side in lat0>=0?new[]{1f,-1f}:new[]{-1f,1f})foreach(float off in new[]{HW+3.4f,HW+5f,HW+6.8f}){var q=M+F*(s0+ds)+Left*side*off;q.y=0;if(found||Blocked(q,.8f)||spots.Any(p=>new Vector2(p.x-q.x,p.z-q.z).magnitude<3.6f))continue;
    q.y=Gf(q);if(float.IsNaN(q.y))continue;s.move=q-l;found=true;spots.Add(q);relocated++;rep.AppendLine($"  re-plant tree {(s.col?P(s.col.transform):"(bark box)")} from local {V(l)} (drive s {s0:F1}, lateral {lat0:+0.0;-0.0}) to {V(q)} (lateral {side*off:+0.0;-0.0}), pieces {s.pieces.Count}");}
   if(!found){s.move=Vector3.zero;rep.AppendLine($"  REMOVE tree {(s.col?P(s.col.transform):"(bark box)")} at local {V(l)}: no free spot beside the drive, pieces {s.pieces.Count}");}}
  foreach(var s in trees.Where(t=>!t.remove)){float dy=Dy(L(s.bottom));if(Mathf.Abs(dy)>.03f){s.dy=dy;seated++;}}
  rep.AppendLine($"  trees: {trees.Count} sites within the region; in the drive's line {trees.Count(t=>t.remove)} (re-planted beside it {relocated}); re-seated on changed ground {seated}");
  // more trees on the left of the drive (the old drive and gravel, now grass), a jittered 4.6 m grid
  var plant=new List<Vector3>();
  for(float s=-1;s<=Ld+8;s+=4.6f)for(float lat=HW+3.4f;lat<=34;lat+=4.6f){float js=1.4f*(Mathf.PerlinNoise(s*.37f,lat*.21f)-.5f)*2,jl=1.4f*(Mathf.PerlinNoise(lat*.33f+7,s*.19f)-.5f)*2;var q=M+F*(s+js)+Left*(lat+jl);q.y=0;int i=IX(q.x),k=IZ(q.z);if(i<0||k<0||i>=NX||k>=NZ)continue;
   bool inZone=zold[i,k]||lat<HW+9;if(!inZone||Blocked(q,.8f)||spots.Any(p=>new Vector2(p.x-q.x,p.z-q.z).magnitude<4.2f)||plant.Any(p=>new Vector2(p.x-q.x,p.z-q.z).magnitude<4.2f))continue;q.y=Gf(q);if(float.IsNaN(q.y))continue;plant.Add(q);}
  var templates=trees.Where(s=>!s.remove&&s.col is BoxCollider&&s.pieces.Count>=2&&s.pieces.Any(p=>p.bark)&&s.pieces.Any(p=>!p.bark)&&s.h>6&&s.pieces.Select(p=>p.mf).Distinct().Count()==1).ToList();
  rep.AppendLine($"  planted on the left of the drive: {plant.Count} (templates: {templates.Count} nearby trees)");foreach(var q in plant){float s=DriveLocal(q.x,q.z,out float lat);rep.AppendLine($"    plant at local {V(q)} (drive s {s:F1}, lateral {lat:+0.0;-0.0})");}
  if(templates.Count==0&&plant.Count>0)throw new Exception("no planting template in "+sn);
  // small objects on changed ground
  var objs=new List<(Renderer r,float dy)>();
  foreach(var r in roots.SelectMany(g=>g.GetComponentsInChildren<Renderer>(true))){if(Veg(r)||r.name.StartsWith("Ground")||r.transform.IsChildOf(site)||gone.Any(c=>r.transform.IsChildOf(c.transform))||!r.bounds.Intersects(regionW)||r.bounds.size.x>8||r.bounds.size.z>8)continue;var b=r.bounds;var l=L(new Vector3(b.center.x,b.min.y,b.center.z));float dy=Dy(l);if(Mathf.Abs(dy)<.03f)continue;
   objs.Add((r,dy));rep.AppendLine($"  re-seat object {P(r.transform)} at local {V(l)} by {dy:+0.00;-0.00}");}
  if(dry){rep.AppendLine("  DRY: nothing saved");WriteGrid(sn);return;}

  // ================= apply
  // ground tiles
  foreach(var mc in tiles){var mesh=mc.sharedMesh;var path=AssetDatabase.GetAssetPath(mesh);
   string users=string.Join(",",AssetDatabase.FindAssets("t:Scene",new[]{"Assets/Scenes"}).Select(AssetDatabase.GUIDToAssetPath).Where(sp=>AssetDatabase.GetDependencies(sp,false).Contains(path)).Select(Path.GetFileNameWithoutExtension));
   if(users.Split(',').Any(u=>!Scenes.Contains(u)))throw new Exception($"{path} is also used by {users}");
   if(edited.Contains(path)){rep.AppendLine($"  ground {P(mc.transform)} [{path}] (scenes {users}): already edited for an earlier scene");continue;}
   var t=mc.transform;var v=mesh.vertices;var changed=new bool[v.Length];int n=0;float mxv=0;
   for(int i=0;i<v.Length;i++){var l=L(t.TransformPoint(v[i]));if(l.x<GX0||l.x>GX1-ST||l.z<GZ0||l.z>GZ1-ST)continue;if(streetGap[Mathf.Clamp(IX(l.x),0,NX-1),Mathf.Clamp(IZ(l.z),0,NZ-1)]<.3f)continue;float d=TargetAt(l.x,l.z,l.y)-l.y;if(Mathf.Abs(d)<.003f)continue;v[i]=t.InverseTransformPoint(W(l+Vector3.up*d));changed[i]=true;n++;mxv=Mathf.Max(mxv,Mathf.Abs(d));}
   if(n==0){rep.AppendLine($"  ground {P(t)} [{path}] (scenes {users}): no vertex in the changed area");continue;}
   originals[path]=(mesh.vertices,mesh.triangles);edited.Add(path);
   var oldN=mesh.normals;mesh.vertices=v;mesh.RecalculateNormals();var nn=mesh.normals;var near=new HashSet<int>();var tr=mesh.triangles;for(int k=0;k<tr.Length;k+=3)if(changed[tr[k]]||changed[tr[k+1]]||changed[tr[k+2]]){near.Add(tr[k]);near.Add(tr[k+1]);near.Add(tr[k+2]);}
   for(int k=0;k<nn.Length;k++)if(!near.Contains(k))nn[k]=oldN[k];mesh.normals=nn;mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);
   rep.AppendLine($"  ground {P(t)} [{path}] (scenes {users}): {n} vertices moved, largest {mxv:F2} m");}
  foreach(var mc in tiles){var m=mc.sharedMesh;mc.sharedMesh=null;mc.sharedMesh=m;}
  // from here on, heights are read from the edited tiles themselves
  var now=tiles.Select(c=>new Sampler(c.sharedMesh.vertices,c.sharedMesh.triangles,c.transform,site)).ToList();float Now(Vector3 l){float best=float.NaN;foreach(var sm in now){float y=sm.Height(l.x,l.z);if(!float.IsNaN(y)&&(float.IsNaN(best)||y>best))best=y;}return best;}
  float Was(Vector3 l){float best=float.NaN;foreach(var sm in samplers){float y=sm.Height(l.x,l.z);if(!float.IsNaN(y)&&(float.IsNaN(best)||y>best))best=y;}return best;}
  Gf=l=>Now(l);Dy=l=>{float a=Was(l),b=Now(l);return float.IsNaN(a)||float.IsNaN(b)?0:b-a;};
  foreach(var s in trees.Where(t=>!t.remove)){float dy=Dy(L(s.bottom));s.dy=Mathf.Abs(dy)>.03f?dy:0;}
  for(int k=0;k<plant.Count;k++)plant[k]=new Vector3(plant[k].x,Gf(plant[k]),plant[k].z);
  foreach(var s in trees.Where(t=>t.remove&&t.move!=Vector3.zero)){var l=L(s.bottom);var q=l+s.move;q.y=Gf(q);s.move=q-l;}
  for(int k=0;k<objs.Count;k++){var r=objs[k].r;var b=r.bounds;objs[k]=(r,Dy(L(new Vector3(b.center.x,b.min.y,b.center.z))));}
  // the old surfaces
  foreach(var c in gone){var go=c.gameObject;if(go.transform.childCount==0)Object.DestroyImmediate(go);else{Object.DestroyImmediate(c);var r=go.GetComponent<MeshRenderer>();if(r)Object.DestroyImmediate(r);}}
  Physics.SyncTransforms();
  // trees: pieces
  var root=new GameObject("Kyle's place (0.88)").transform;root.SetParent(site.parent,false);root.gameObject.layer=site.gameObject.layer;
  var plantedV=new Dictionary<MeshFilter,(List<Vector3> v,List<Color> c,List<int> t)>();
  void Copy(MeshFilter mf,IEnumerable<Piece> ps,Vector3 off){if(!plantedV.TryGetValue(mf,out var acc)){acc=(new List<Vector3>(),new List<Color>(),new List<int>());plantedV[mf]=acc;}var m=mf.sharedMesh;var tv=m.vertices;var tc=m.colors;var tt=m.triangles;var map=new Dictionary<int,int>();
   foreach(var pc in ps)foreach(var i in pc.idx){if(map.ContainsKey(i))continue;map[i]=acc.v.Count;acc.v.Add(mf.transform.TransformPoint(tv[i])+off);acc.c.Add(tc.Length==tv.Length?tc[i]:Color.white);}
   for(int i=0;i<tt.Length;i+=3)if(map.ContainsKey(tt[i])&&map.ContainsKey(tt[i+1])&&map.ContainsKey(tt[i+2]))acc.t.AddRange(new[]{map[tt[i]],map[tt[i+1]],map[tt[i+2]]});}
  // relocated trees: their pieces copied to the new spot (by the offset plus the ground there), the originals cut
  foreach(var s in trees.Where(t=>t.remove&&t.move!=Vector3.zero)){var off=W(L(s.bottom)+s.move)-s.bottom;foreach(var g in s.pieces.GroupBy(p=>p.mf))Copy(g.Key,g,off);}
  // planted trees from templates
  for(int k=0;k<plant.Count;k++){var tpl=templates[(k*7)%templates.Count];var off=W(plant[k])-tpl.bottom;Copy(tpl.pieces[0].mf,tpl.pieces,off);}
  foreach(var g in pieces.GroupBy(p=>p.mf)){var cutP=g.Where(p=>p.owner>=0&&trees[p.owner].remove).ToList();var move=g.Where(p=>p.owner>=0&&!trees[p.owner].remove&&trees[p.owner].dy!=0).ToList();
   var loose=new Dictionary<Piece,float>();foreach(var p in g.Where(p=>p.owner<0)){var l=L(new Vector3(p.b.center.x,p.b.min.y,p.b.center.z));if((InDrive(l,1f)||Out(l.x,l.z,Apron)<.8f)&&p.b.min.y<site.position.y+Gf(l)+4){cutP.Add(p);continue;}float dy=Dy(l);if(Mathf.Abs(dy)>.03f)loose[p]=dy;}
   if(cutP.Count+move.Count+loose.Count==0)continue;var mf=g.Key;var m=Object.Instantiate(mf.sharedMesh);var vv=m.vertices;var tr=mf.transform;var cutSet=new HashSet<int>(cutP.SelectMany(p=>p.idx));
   foreach(var p in move)foreach(var i in p.idx)vv[i]=tr.InverseTransformPoint(tr.TransformPoint(vv[i])+Vector3.up*trees[p.owner].dy);foreach(var kv in loose)foreach(var i in kv.Key.idx)vv[i]=tr.InverseTransformPoint(tr.TransformPoint(vv[i])+Vector3.up*kv.Value);
   var tri=m.triangles;var keep=new List<int>();for(int k=0;k<tri.Length;k+=3)if(!cutSet.Contains(tri[k]))keep.AddRange(new[]{tri[k],tri[k+1],tri[k+2]});m.vertices=vv;m.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;m.SetTriangles(keep,0);m.RecalculateBounds();
   string np=$"{Dir}/{sn}-kyle-vegetation-0.88-{mf.name.Replace('/','_')}-{Math.Abs(AssetDatabase.GetAssetPath(mf.sharedMesh).GetHashCode())%100000}.asset";AssetDatabase.CreateAsset(m,np);rep.AppendLine($"  vegetation {P(tr)}: pieces cut {cutP.Count}, moved {move.Count}, bushes moved {loose.Count} -> {np}");mf.sharedMesh=m;}
  int pn=0;foreach(var kv in plantedV){var pm=new Mesh{indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};pm.SetVertices(kv.Value.v);pm.SetColors(kv.Value.c);pm.SetTriangles(kv.Value.t,0);pm.RecalculateNormals();pm.RecalculateBounds();string np=$"{Dir}/{sn}-kyle-planted-trees-0.88-{pn}.asset";AssetDatabase.CreateAsset(pm,np);
   var pg=new GameObject($"Kyle's place trees {pn++}",typeof(MeshFilter),typeof(MeshRenderer));pg.transform.SetParent(root,true);pg.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);pg.layer=kv.Key.gameObject.layer;pg.isStatic=kv.Key.gameObject.isStatic;pg.GetComponent<MeshFilter>().sharedMesh=pm;var r0=kv.Key.GetComponent<MeshRenderer>();var pr=pg.GetComponent<MeshRenderer>();pr.sharedMaterials=r0.sharedMaterials;pr.shadowCastingMode=r0.shadowCastingMode;}
  // trees: colliders
  var trunks=new GameObject("Kyle's place tree trunks").transform;trunks.SetParent(root,true);
  void Trunk(Collider c,Vector3 off){if(c is BoxCollider b){var go=new GameObject("Tree trunk");go.transform.SetParent(trunks);go.layer=b.gameObject.layer;go.isStatic=b.gameObject.isStatic;go.transform.SetPositionAndRotation(b.transform.position+off,b.transform.rotation);go.transform.localScale=b.transform.lossyScale;var nb=go.AddComponent<BoxCollider>();nb.center=b.center;nb.size=b.size;}else rep.AppendLine($"  WARNING trunk collider {P(c.transform)} is {c.GetType().Name}; not copied");}
  for(int k=0;k<plant.Count;k++){var tpl=templates[(k*7)%templates.Count];Trunk(tpl.col,W(plant[k])-tpl.bottom);}
  foreach(var s in trees){if(!s.col)continue;if(s.remove){if(s.move!=Vector3.zero)Trunk(s.col,W(L(s.bottom)+s.move)-s.bottom);var go=s.col.gameObject;if(go.GetComponents<Component>().Length<=2&&go.transform.childCount==0)Object.DestroyImmediate(go);else Object.DestroyImmediate(s.col);}else if(s.dy!=0)s.col.transform.position+=Vector3.up*s.dy;}
  foreach(var (r,dy) in objs)if(r)r.transform.position+=Vector3.up*dy;
  // ---- the new drive: one ribbon mouth -> apron edge and the apron, gravel, with a collider
  {var vs=new List<Vector3>();var ts=new List<int>();int rows=Mathf.CeilToInt(Ld/.5f);
   for(int i=0;i<=rows;i++){float s=Ld*i/rows;var c=M+F*s;float lift=Mathf.Lerp(.006f,.03f,Mathf.Clamp01(s/1.2f));foreach(float w in new[]{-HW,HW}){var q=c+Left*w;if(i==0){for(float b=0;b<3;b+=.02f){if(Gap(q-F*b)<0){q-=F*Mathf.Max(0,b-.02f);break;}}}q.y=Surf(s,w)+lift;vs.Add(W(q));}if(i>0){int a=(i-1)*2;ts.AddRange(new[]{a,a+2,a+1,a+1,a+2,a+3});}}
   int b0=vs.Count;int ax=8,az=7;for(int i=0;i<=ax;i++)for(int k=0;k<=az;k++)vs.Add(W(new Vector3(Mathf.Lerp(Apron.x0,Apron.x1,i/(float)ax),.03f,Mathf.Lerp(Apron.z0,Apron.z1,k/(float)az))));for(int i=0;i<ax;i++)for(int k=0;k<az;k++){int a=b0+i*(az+1)+k;ts.AddRange(new[]{a,a+1,a+az+1,a+1,a+az+2,a+az+1});}
   var m=new Mesh{indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};m.SetVertices(vs);m.SetTriangles(ts,0);m.uv=vs.Select(p=>new Vector2(p.x,p.z)*.2f).ToArray();m.RecalculateNormals();
   {var nrm=m.normals;var t2=m.triangles;bool flip=false;for(int k=0;k<t2.Length;k+=3){var n=Vector3.Cross(vs[t2[k+1]]-vs[t2[k]],vs[t2[k+2]]-vs[t2[k]]);if(n.y<0){(t2[k+1],t2[k+2])=(t2[k+2],t2[k+1]);flip=true;}}if(flip){m.triangles=t2;m.RecalculateNormals();}}m.RecalculateBounds();
   string np=$"{Dir}/{sn}-kyle-driveway-0.88.asset";AssetDatabase.CreateAsset(m,np);
   var g=new GameObject("Ground_Kyle driveway (0.88)",typeof(MeshFilter),typeof(MeshRenderer));g.transform.SetParent(driveRoad.transform,true);g.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);g.transform.localScale=Vector3.one;if(g.transform.lossyScale!=Vector3.one)throw new Exception("drive parent scaled");
   g.GetComponent<MeshFilter>().sharedMesh=m;g.GetComponent<MeshRenderer>().sharedMaterial=gravelMat;g.AddComponent<MeshCollider>().sharedMesh=m;g.layer=driveLayer;g.isStatic=driveStatic;rep.AppendLine($"  new drive and apron: {np}, gravel '{gravelMat.name}'");}
  // ---- the step between lawn and porch: one broad step, concrete like the porch slab (Building shader, vertex colour)
  {var building=AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/Scenery/Building.mat");float y0=YL-.35f,y1=(YL+2.65f)*.5f;var c=new Color(.62f,.60f,.57f,1);
   var m=Box(new Vector3(Step.x0,y0,Step.z0),new Vector3(Step.x1,y1,Step.z1),c);string np=$"{Dir}/kyle-front-step-0.88.asset";var prior=AssetDatabase.LoadAssetAtPath<Mesh>(np);if(prior)m=prior;else AssetDatabase.CreateAsset(m,np);
   var g=new GameObject("Kyle's front step (0.88)",typeof(MeshFilter),typeof(MeshRenderer));g.transform.SetParent(site,false);g.layer=site.gameObject.layer;g.isStatic=site.gameObject.isStatic;g.GetComponent<MeshFilter>().sharedMesh=m;g.GetComponent<MeshRenderer>().sharedMaterial=building;
   var bc=g.AddComponent<BoxCollider>();bc.center=new Vector3((Step.x0+Step.x1)*.5f,(y0+y1)*.5f,(Step.z0+Step.z1)*.5f);bc.size=new Vector3(Step.x1-Step.x0,y1-y0,Step.z1-Step.z0);rep.AppendLine($"  front step: local x {Step.x0}..{Step.x1}, z {Step.z0}..{Step.z1}, top {y1:F2} (lawn {YL:F2}, porch 2.65)");}
  // ---- mailbox
  {var dan=all.FirstOrDefault(t=>t.name=="Mailbox - Dan - blue X");var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Buildings/Roadside/Mailbox.prefab");var g=(GameObject)PrefabUtility.InstantiatePrefab(prefab,dan?dan.parent:root);g.name="Mailbox - Kyle";
   var p=W(new Vector3(mail.x,Gf(mail),mail.z));var face=site.TransformDirection(-F);face.y=0;g.transform.SetPositionAndRotation(p,Quaternion.LookRotation(face.normalized));
   if(dan){var src=dan.GetComponent<BreakableProp>();var dst=g.GetComponent<BreakableProp>();if(src&&dst){var so=new SerializedObject(dst);var sp=new SerializedObject(src).FindProperty("surface");if(sp!=null){so.FindProperty("surface").intValue=sp.intValue;so.ApplyModifiedPropertiesWithoutUndo();}}}
   PrefabUtility.RecordPrefabInstancePropertyModifications(g.transform);rep.AppendLine($"  mailbox at {V(p)} yaw {g.transform.eulerAngles.y:F0} (door to the street), trigger collider + breakaway as the other mailboxes");}
  // ---- route points, map arrival, vignette
  {var before=string.Join(" ",driveRoad.points.Select(V));var pts=new List<Vector3>{W(new Vector3(M.x,Prof(0)+.03f,M.z)),W(new Vector3((M+F*(Ld*.5f)).x,Prof(Ld*.5f)+.03f,(M+F*(Ld*.5f)).z)),W(new Vector3(E.x,.03f,E.z)),W(new Vector3(12.2f,.03f,-1.6f))};
   driveRoad.points=pts.ToArray();EditorUtility.SetDirty(driveRoad);rep.AppendLine($"  drive route points {before} -> {string.Join(" ",pts.Select(V))}");
   var map=roots.SelectMany(g=>g.GetComponentsInChildren<ExplorationMap>(true)).FirstOrDefault();var d=map?map.destinations.FirstOrDefault(x=>x.id=="property-anderson"):null;
   if(d!=null){var was=d.position;d.position=pts[^1];d.yaw=Quaternion.LookRotation(Vector3.ProjectOnPlane(pts[^2]-pts[^1],Vector3.up)).eulerAngles.y;EditorUtility.SetDirty(map);rep.AppendLine($"  map destination property-anderson {V(was)} -> {V(d.position)} yaw {d.yaw:F0}");}
   foreach(var al in roots.SelectMany(g=>g.GetComponentsInChildren<AmbientLife>(true))){if(al.smoking==null||al.smoking.Length<2)continue;var was=string.Join(" ",al.smoking.Select(V));al.smoking[0]=W(new Vector3(vigA.x,Gf(vigA),vigA.z));al.smoking[1]=W(new Vector3(vigB.x,Gf(vigB),vigB.z));EditorUtility.SetDirty(al);rep.AppendLine($"  Dan-and-Kyle vignette {was} -> {string.Join(" ",al.smoking.Select(V))}");}}
  EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();WriteGrid(sn);rep.AppendLine("  saved");}
 // heights from a mesh's triangles in the house frame (region only), bucketed by 2 m
 sealed class Sampler{readonly Vector3[] v;readonly int[] t;readonly Dictionary<(int,int),List<int>> b=new();
  public Sampler(Vector3[] mv,int[] tris,Transform tr,Transform site){v=new Vector3[mv.Length];for(int i=0;i<mv.Length;i++)v[i]=site.InverseTransformPoint(tr.TransformPoint(mv[i]));t=tris;
   for(int k=0;k<t.Length;k+=3){var a=v[t[k]];var c=v[t[k+1]];var d=v[t[k+2]];float x0=Mathf.Min(a.x,Mathf.Min(c.x,d.x)),x1=Mathf.Max(a.x,Mathf.Max(c.x,d.x)),z0=Mathf.Min(a.z,Mathf.Min(c.z,d.z)),z1=Mathf.Max(a.z,Mathf.Max(c.z,d.z));if(x1<GX0-2||x0>GX1+2||z1<GZ0-2||z0>GZ1+2)continue;
    for(int i=Mathf.FloorToInt(x0/2);i<=Mathf.FloorToInt(x1/2);i++)for(int j=Mathf.FloorToInt(z0/2);j<=Mathf.FloorToInt(z1/2);j++){if(!b.TryGetValue((i,j),out var l))b[(i,j)]=l=new List<int>();l.Add(k);}}}
  public float Height(float x,float z){if(!b.TryGetValue((Mathf.FloorToInt(x/2),Mathf.FloorToInt(z/2)),out var l))return float.NaN;float best=float.NaN;
   foreach(var k in l){var a=v[t[k]];var c=v[t[k+1]];var d=v[t[k+2]];float den=(c.z-d.z)*(a.x-d.x)+(d.x-c.x)*(a.z-d.z);if(Mathf.Abs(den)<1e-9f)continue;float w1=((c.z-d.z)*(x-d.x)+(d.x-c.x)*(z-d.z))/den,w2=((d.z-a.z)*(x-d.x)+(a.x-d.x)*(z-d.z))/den,w3=1-w1-w2;if(w1<-1e-4f||w2<-1e-4f||w3<-1e-4f)continue;float y=w1*a.y+w2*c.y+w3*d.y;if(float.IsNaN(best)||y>best)best=y;}return best;}}
 static Mesh Box(Vector3 a,Vector3 b,Color c){var v=new List<Vector3>();var t=new List<int>();void Q(Vector3 p0,Vector3 p1,Vector3 p2,Vector3 p3){int i=v.Count;v.AddRange(new[]{p0,p1,p2,p3});t.AddRange(new[]{i,i+1,i+2,i,i+2,i+3});}
  Vector3 P(float x,float y,float z)=>new(x==0?a.x:b.x,y==0?a.y:b.y,z==0?a.z:b.z);
  Q(P(0,1,0),P(0,1,1),P(1,1,1),P(1,1,0));Q(P(0,0,1),P(1,0,1),P(1,1,1),P(0,1,1));Q(P(1,0,0),P(0,0,0),P(0,1,0),P(1,1,0));Q(P(0,0,0),P(0,0,1),P(0,1,1),P(0,1,0));Q(P(1,0,1),P(1,0,0),P(1,1,0),P(1,1,1));
  var m=new Mesh{name="Kyle's front step (0.88)"};m.SetVertices(v);m.SetTriangles(t,0);m.SetColors(Enumerable.Repeat(c,v.Count).ToList());m.RecalculateNormals();m.RecalculateBounds();return m;}
 static void WriteGrid(string sn){var o=Environment.GetEnvironmentVariable("PROBE_OUT");var sb=new StringBuilder("# house frame, 1 m: before -> after (z rows, x columns -20..40)\n");
  for(float z=40;z>=-12;z-=2){sb.Append($"{z,4}:");for(float x=-20;x<=40;x+=2){float a=Bil(H0,x,z),b=Bil(H1,x,z);sb.Append(Mathf.Abs(b-a)<.01f?$"{a,6:F2} ":$"{a,6:F2}>{b:F2}");sb.Append(zold[IX(x),IZ(z)]?"z":" ");}sb.AppendLine();}
  File.WriteAllText($"{o}/A-grid-{sn}.txt",sb.ToString());}
}
