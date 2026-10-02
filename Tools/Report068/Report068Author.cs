using System;using System.IO;using System.Linq;using System.Reflection;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
// 0.68 local repairs for Dan's debug session 2026-10-02_13-02-35-687_63de1f (PROJECT_TODO "CURRENT" Parts A-C, E).
// Every change is local and logged to Docs/Report068/author-notes.txt. No route, checkpoint, physics or AI data is changed.
// Section 5A: lower routes, tunnels, flight corridors and other routes' corridors are never filled or obstructed.
public static class Report068Author {
 const string Folder="Assets/Track/Report068";
 static readonly List<string> notes=new();
 static void Note(string s){notes.Add(s);Debug.Log("REPORT068 "+s);}
 static string Scene=>UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
 static Color Earth=new(.39f,.48f,.29f),Rock=new(.34f,.36f,.29f);
 static bool IsDrive(Collider c)=>c&&c.name.Contains("driving surface");
 static bool IsTerrain(Collider c)=>c is MeshCollider&&!c.isTrigger&&!c.attachedRigidbody&&c.name.StartsWith("Ground");
 static bool Surface(float x,float z,float from,Func<Collider,bool> ok,out RaycastHit hit,float depth=260){var o=new Vector3(x,from,z);hit=default;
  for(int k=0;k<60&&depth>0;k++){if(!Physics.Raycast(o,Vector3.down,out var h,depth,~0,QueryTriggerInteraction.Ignore))return false;if(h.normal.y>0&&ok(h.collider)){hit=h;return true;}depth-=h.distance+.02f;o=h.point+Vector3.down*.02f;}return false;}
 static bool Pave(Vector3 q,float near,out float y){y=0;if(!Surface(q.x,q.z,near+2.5f,IsDrive,out var h,5))return false;y=h.point.y;return true;}
 static Mesh Store(string name,Mesh mesh){Directory.CreateDirectory(Folder);string path=$"{Folder}/{Scene}-{name}.asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(old){EditorUtility.CopySerialized(mesh,old);Object.DestroyImmediate(mesh);return old;}AssetDatabase.CreateAsset(mesh,path);return mesh;}
 static Mesh Build(List<Vector3> v,List<Color> c,List<int> t){var m=new Mesh{indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};m.SetVertices(v);m.SetColors(c);m.SetTriangles(t,0);m.RecalculateNormals();m.RecalculateBounds();return m;}
 static void Tri(List<Vector3> v,List<Color> c,List<int> t,Vector3 a,Vector3 b,Vector3 d,Vector3 want){var n=Vector3.Cross(b-a,d-a);if(n.sqrMagnitude<1e-8f)return;if(Vector3.Dot(n,want)<0){var x=b;b=d;d=x;n=-n;}var col=n.normalized.y>.72f?Earth:Rock;int k=v.Count;v.AddRange(new[]{a,b,d});c.AddRange(new[]{col,col,col});t.AddRange(new[]{k,k+1,k+2});}
 static GameObject Make(string name,Mesh mesh,Material mat){var g=new GameObject("Ground_Report068 "+name,typeof(MeshFilter),typeof(MeshRenderer),typeof(MeshCollider));g.GetComponent<MeshFilter>().sharedMesh=mesh;g.GetComponent<MeshRenderer>().sharedMaterial=mat;g.GetComponent<MeshCollider>().sharedMesh=mesh;GameObjectUtility.SetStaticEditorFlags(g,StaticEditorFlags.BatchingStatic);return g;}
 static Material GroundMaterial(){var r=Object.FindObjectsByType<MeshRenderer>().FirstOrDefault(x=>x.name.Contains("continuous solid 0"))??Object.FindObjectsByType<MeshRenderer>().First(x=>x.name.StartsWith("Ground_Mountain"));return r.sharedMaterial;}
 static void Save(){var s=UnityEngine.SceneManagement.SceneManager.GetActiveScene();EditorSceneManager.MarkSceneDirty(s);EditorSceneManager.SaveScene(s);AssetDatabase.SaveAssets();}
 static void Refresh(MeshFilter mf){if(mf.TryGetComponent<MeshCollider>(out var mc)){mc.sharedMesh=null;mc.sharedMesh=mf.sharedMesh;}EditorUtility.SetDirty(mf.sharedMesh);Physics.SyncTransforms();}
 static RaceDirector Race=>Object.FindAnyObjectByType<RaceDirector>(FindObjectsInactive.Include);
 static string Path(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n.Replace('\n','/');}
 // Routes of the open scene (main + active branches) for corridor checks.
 sealed class R{public string n;public Func<float,Vector3> at;public Func<float,Vector3> fw;public float len;public Func<float,float> hw;}
 static List<R> Routes(){var road=Race.road;road.Initialize();var list=new List<R>{new(){n="Main",at=x=>road.At(x,out _),fw=x=>{road.At(x,out var f);return f;},len=road.Length,hw=x=>road.HalfWidth(x)}};
  foreach(var b in Object.FindObjectsByType<WoodlandRoute>().Where(b=>b.gameObject.activeInHierarchy)){b.Initialize();var bb=b;list.Add(new(){n=b.title,at=x=>bb.At(x,out _),fw=x=>{bb.At(x,out var f);return f;},len=b.Length,hw=x=>bb.halfWidth});}return list;}
 static float Near(R r,Vector3 p,out float lat,out float dy){float best=float.MaxValue,bs=0;for(float s=0;s<=r.len;s+=1){var q=r.at(s);float d=new Vector2(q.x-p.x,q.z-p.z).sqrMagnitude;if(d<best){best=d;bs=s;}}
  for(float s=Mathf.Max(0,bs-1);s<=Mathf.Min(r.len,bs+1);s+=.1f){var q=r.at(s);float d=new Vector2(q.x-p.x,q.z-p.z).sqrMagnitude;if(d<best){best=d;bs=s;}}lat=Mathf.Sqrt(best);dy=p.y-r.at(bs).y;return bs;}

 public static void Run(){
  if(Application.isPlaying)throw new Exception("Edit mode required");Directory.CreateDirectory("Docs/Report068");Physics.queriesHitBackfaces=true;
  var parts=(Environment.GetEnvironmentVariable("AUTHOR_PARTS")??"all").Split(',');bool Do(string p)=>parts.Contains("all")||parts.Contains(p);
  try{
   foreach(var scene in new[]{"MountainLoop","MountainLoopReverse"}){
    EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();Note("SCENE "+scene);
    if(Do("sheets"))ShoulderSheets();
    if(Do("seams"))SeamSupport();
    if(scene=="MountainLoop"&&Do("spawn"))SpawnMarker();
    if(scene=="MountainLoopReverse"){if(Do("b004"))RemoveSigns(t=>t.Contains("OLD CUT CLOSED"),"BUG-004");if(Do("b005"))Crease();if(Do("b006"))CapShoulder();}
    if(scene=="MountainLoop"&&Do("landing"))Landing();
    if(Do("barriers"))Barriers();
    Save();}
  }finally{File.AppendAllLines("Docs/Report068/author-notes.txt",notes);}
  EditorApplication.Exit(0);}

 // ---------- BUG-003 (and the same defect elsewhere): 0.67 edge-shoulder sheets lying ON the pavement ----------
 // 0.67 tucked shoulder points under pavement that lay ABOVE them; points lying 0-0.6 m above the pavement (a shoulder
 // built from a neighbouring route's edge) stayed visible across the driving surface. Tuck those the same way (1.2 m under).
 static void ShoulderSheets(){var mf=Object.FindObjectsByType<MeshFilter>().FirstOrDefault(m=>m.name=="Ground_Report067 edge shoulders");if(!mf){Note("sheets: no 0.67 shoulder mesh");return;}
  var m=mf.sharedMesh;var v=m.vertices;int moved=0;var where=new List<string>();var col=mf.GetComponent<Collider>();
  for(int i=0;i<v.Length;i++){var w=mf.transform.TransformPoint(v[i]);
   var hs=Physics.RaycastAll(w+Vector3.up*.05f,Vector3.down,.65f,~0,QueryTriggerInteraction.Ignore).Where(h=>IsDrive(h.collider)&&h.normal.y>.3f).ToArray();if(hs.Length==0)continue;
   float pave=hs.Max(h=>h.point.y);if(w.y<pave-.02f)continue;w.y=pave-1.2f;v[i]=mf.transform.InverseTransformPoint(w);moved++;if(where.Count<400)where.Add($"{w.x:F1},{w.z:F1}");}
  if(moved>0){m.vertices=v;m.RecalculateNormals();m.RecalculateBounds();Refresh(mf);}
  var cells=where.Select(s=>s.Split(',')).GroupBy(a=>$"{Mathf.Floor(float.Parse(a[0])/20)*20},{Mathf.Floor(float.Parse(a[1])/20)*20}").Select(g=>$"{g.Key}({g.Count()})");
  Note($"BUG-003 sheets {Scene}: {moved} 0.67 shoulder vertices that lay on top of the pavement tucked 1.2 m under it; 20 m cells: {string.Join(" ",cells)}");}

 // ---------- BUG-007: 0.67 seam covers over holes ----------
 // The 0.67 seam covers are visual only. Where nothing solid lies within 2.5 m below a cover (bottomless or deep slot
 // between pavement and ground), a motorcycle could drop through what looks like ground. Those triangles get a matching
 // collider (same surface, no extra renderer), so the visible surface is solid.
 static void SeamSupport(){foreach(var old in Object.FindObjectsByType<MeshFilter>().Where(x=>x.name=="Ground_Report068 seam support").ToArray())Object.DestroyImmediate(old.gameObject);Physics.SyncTransforms();
  var mf=Object.FindObjectsByType<MeshFilter>().FirstOrDefault(x=>x.name=="Ground_Report067 edge seams");if(!mf){Note("seams: none");return;}
  var m=mf.sharedMesh;var v=m.vertices;var t=m.triangles;var nv=new List<Vector3>();var nc=new List<Color>();var nt=new List<int>();int voids=0,deep=0;var log=new List<string>();
  for(int i=0;i<t.Length;i+=3){var a=mf.transform.TransformPoint(v[t[i]]);var b=mf.transform.TransformPoint(v[t[i+1]]);var c=mf.transform.TransformPoint(v[t[i+2]]);bool bad=false;
   foreach(var p in new[]{(a+b+c)/3,Vector3.Lerp(a,(a+b+c)/3,.3f),Vector3.Lerp(b,(a+b+c)/3,.3f),Vector3.Lerp(c,(a+b+c)/3,.3f)}){
    if(!Physics.Raycast(p+Vector3.up*.02f,Vector3.down,out var h,600,~0,QueryTriggerInteraction.Ignore)){bad=true;voids++;log.Add($"VOID {p:F2}");break;}
    if(p.y-h.point.y>2.5f){bad=true;deep++;log.Add($"DEEP {p:F2} floor {h.point.y:F2} ({h.collider.name})");break;}}
   if(bad){int k=nv.Count;nv.AddRange(new[]{a,b,c});nc.AddRange(new[]{Earth,Earth,Earth});nt.AddRange(new[]{k,k+1,k+2});}}
  File.WriteAllLines($"Docs/Report068/seam-support-{Scene}.txt",log.Prepend($"{Scene}: seam triangles {t.Length/3}; given a collider {nt.Count/3} (void beneath {voids}, >2.5 m deep {deep})"));
  if(nt.Count==0){Note($"BUG-007 seams {Scene}: no seam cover over a hole");return;}
  var g=Make("seam support",Store("seam-support",Build(nv,nc,nt)),GroundMaterial());Object.DestroyImmediate(g.GetComponent<MeshRenderer>());
  Note($"BUG-007 seams {Scene}: {nt.Count/3} of {t.Length/3} 0.67 seam-cover triangles lay over a hole (void {voids}, deep {deep}); they now have a matching collider (Ground_Report068 seam support, collider only; the 0.67 cover still draws it)");}

 // ---------- BUG-009: Mountain Forward start marker below the pavement ----------
 static void SpawnMarker(){var race=Race;var spawn=race.vehicle.GetComponent<VehicleRespawn>().spawnPoint;if(!spawn){Note("spawn: none");return;}var p=spawn.position;
  if(!Surface(p.x,p.z,p.y+6,c=>IsDrive(c)||IsTerrain(c),out var h,12)){Note("spawn: no surface");return;}float y=h.point.y+.7f;if(Mathf.Abs(y-p.y)<.05f){Note("spawn already seated");return;}
  spawn.position=new Vector3(p.x,y,p.z);race.vehicle.transform.SetPositionAndRotation(spawn.position,Quaternion.Euler(0,spawn.eulerAngles.y,0));EditorUtility.SetDirty(spawn);EditorUtility.SetDirty(race.vehicle.transform);
  Note($"BUG-009: start marker '{spawn.name}' was {h.point.y-p.y:F2} m BELOW the pavement ({h.collider.name} at {h.point.y:F2}) with no ground under it; raised to 0.7 m above it ({p.y:F2} -> {y:F2}); scene vehicle moved with it");}

 static void RemoveSigns(Func<string,bool> match,string label){foreach(var t in Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include).ToArray()){if(!t||!match(t.text))continue;var sign=t.GetComponentInParent<PhysicalSign>(true);var root=sign?sign.gameObject:t.transform.parent.gameObject;Note($"{label}: removed sign '{t.text.Replace('\n','/')}' with post at {root.transform.position}");Object.DestroyImmediate(root);}}

 // ---------- BUG-005: crease across the Reverse pavement at main s 2966 ----------
 // A 0.3-0.6 m step along z~-115 joins the climb (north) to a flatter plateau (south). Blend the pavement vertices across
 // a 5 m band (z -117.5..-112.5) linearly between the band edges, faded out over 2 m at the band's west/east ends.
 static void Crease(){var mf=Object.FindObjectsByType<MeshFilter>().FirstOrDefault(x=>x.name=="Ground_CR133 mountain driving surface");if(!mf){Note("BUG-005: no pavement");return;}
  // Smooth vertical curve: the plateau (south, ~2% grade) and the climb (north, ~12%) are joined by a cubic Hermite
  // transition over 20 m (z -126..-106) whose end heights/grades come from the untouched surface just outside the band.
  // At 35 m/s the grade change then needs < 0.7 g, so a full-throttle bike stays on the surface (a 5 m blend still launched it).
  string path=AssetDatabase.GetAssetPath(mf.sharedMesh);const float zN=-106f,zS=-126f,x0=739,x1=757;
  float Sample(float x,float z){return Surface(x,z,100,IsDrive,out var h,30)?h.point.y:float.NaN;}
  var m=mf.sharedMesh;var v=m.vertices;int moved=0;float worst=0;var before=new List<string>();var after=new List<string>();
  string Line(float x){var parts=new List<string>();for(float z=zS-2;z<=zN+2;z+=2)parts.Add(Sample(x,z).ToString("F2"));return $"x {x}: z {zS-2}..{zN+2} step 2: "+string.Join(" ",parts);}
  for(float x=x0;x<=x1;x+=3)before.Add(Line(x));
  var ends=new Dictionary<int,(float yS,float gS,float yN,float gN)>();
  (float yS,float gS,float yN,float gN)? Ends(float x){float a=Sample(x,zS-2),b=Sample(x,zS),c=Sample(x,zN),d=Sample(x,zN+2);if(float.IsNaN(a)||float.IsNaN(b)||float.IsNaN(c)||float.IsNaN(d))return null;return(b,(b-a)/2,c,(d-c)/2);}
  var targets=new Dictionary<int,float>();
  for(int i=0;i<v.Length;i++){var w=mf.transform.TransformPoint(v[i]);if(w.x<x0-2||w.x>x1+2||w.z>zN||w.z<zS||w.y<80||w.y>92)continue;
   var e=Ends(w.x);if(e==null)continue;var(yS,gS,yN,gN)=e.Value;float L=zN-zS,t=(w.z-zS)/L;
   float h00=2*t*t*t-3*t*t+1,h10=t*t*t-2*t*t+t,h01=-2*t*t*t+3*t*t,h11=t*t*t-t*t;float target=h00*yS+h10*L*gS+h01*yN+h11*L*gN;
   float fade=Mathf.Clamp01(Mathf.Min(w.x-(x0-2),(x1+2)-w.x)/2);targets[i]=Mathf.Lerp(w.y,target,fade);}
  foreach(var kv in targets){var w=mf.transform.TransformPoint(v[kv.Key]);worst=Mathf.Max(worst,Mathf.Abs(kv.Value-w.y));w.y=kv.Value;v[kv.Key]=mf.transform.InverseTransformPoint(w);moved++;}
  if(moved>0){m.vertices=v;m.RecalculateNormals();m.RecalculateBounds();Refresh(mf);}
  for(float x=x0;x<=x1;x+=3)after.Add(Line(x));
  File.WriteAllLines("Docs/Report068/bug005-crease.txt",new[]{$"mesh {path}; vertices on the 20 m vertical curve {moved}; max vertex change this pass {worst:F2} m","BEFORE (heights every 2 m from south to north)"}.Concat(before).Concat(new[]{"AFTER"}).Concat(after));
  Note($"BUG-005: {moved} pavement vertices ({path}) set on a smooth 20 m vertical curve z -126..-106, x {x0}..{x1}; max change {worst:F2} m (profiles in bug005-crease.txt)");
  foreach(var r in Object.FindObjectsByType<MeshRenderer>().Where(r=>r.bounds.center.z>zS-2&&r.bounds.center.z<zN+2&&r.bounds.center.x>x0-3&&r.bounds.center.x<x1+3&&r.GetComponent<MeshFilter>()&&r.GetComponent<MeshFilter>().sharedMesh.vertexCount<=16&&(r.name.ToLower().Contains("arrow")||Path(r.transform).StartsWith("Route atlas"))).ToArray())Drape(r,.06f,"BUG-005");}
 static void Drape(Renderer r,float lift,string label){var mf=r.GetComponent<MeshFilter>();var v=mf.sharedMesh.vertices;float worst=0;var nv=new Vector3[v.Length];
  for(int i=0;i<v.Length;i++){var w=mf.transform.TransformPoint(v[i]);if(!Surface(w.x,w.z,w.y+4,c=>IsDrive(c)||IsTerrain(c),out var h,12)){Note($"{label}: '{r.name}' unsupported vertex; left as is");return;}worst=Mathf.Max(worst,Mathf.Abs(w.y-h.point.y-lift));w.y=h.point.y+lift;nv[i]=mf.transform.InverseTransformPoint(w);}
  if(worst<.03f)return;var m=Object.Instantiate(mf.sharedMesh);m.vertices=nv;m.RecalculateBounds();m.RecalculateNormals();mf.sharedMesh=Store($"draped-arrow-{Mathf.RoundToInt(r.bounds.center.x)}-{Mathf.RoundToInt(r.bounds.center.z)}",m);
  Note($"{label}: arrow '{Path(r.transform)}' reseated flat on the pavement (max vertex correction {worst:F2} m)");}

 // ---------- Generic strip along a route side: one cross-section per metre, joined into one collidable mesh ----------
 // section(s, edge point, pavement y at edge, outward unit vector, taper 0..1) returns the outward profile points or null.
 static int Strip(R r,float s0,float s1,int side,string label,List<Vector3> v,List<Color> c,List<int> t,Func<float,Vector3,float,Vector3,float,List<Vector3>> section,List<string> log){
  var routes=Routes();List<Vector3> prev=null;int made=0;
  for(float s=s0;s<=s1;s+=1){var ctr=r.at(s);var f=Vector3.ProjectOnPlane(r.fw(s),Vector3.up).normalized;var right=Vector3.Cross(Vector3.up,f);var outv=right*side;
   if(!Pave(ctr,ctr.y,out float py)){prev=null;log.Add($"{label} s {s}: no pavement at centre");continue;}
   float w=0;for(;w<16;w+=.1f){if(!Pave(ctr+outv*(w+.1f),py,out float ny)||Mathf.Abs(ny-py)>.6f)break;py=ny;}
   var edge=ctr+outv*w;edge.y=py;float taper=Mathf.Clamp01(Mathf.Min(s-s0,s1-s)/8f);
   var pts=section(s,edge,py,outv,taper);if(pts==null){prev=null;continue;}
   // Never into another route's corridor (shortcut entrances/exits, lower routes, other parts of the lap).
   string clash=null;foreach(var p in pts)foreach(var o in routes){float os=Near(o,p,out float lat,out float dy);if(o.n==r.n&&Mathf.Abs(os-s)<40)continue;if(lat<o.hw(os)+1.5f&&dy>-1.5f&&dy<6){clash=o.n;break;}}
   if(clash!=null){prev=null;log.Add($"{label} s {s}: would enter {clash} corridor; skipped");continue;}
   if(prev!=null&&prev.Count==pts.Count)for(int k=0;k+1<pts.Count;k++){Tri(v,c,t,prev[k],prev[k+1],pts[k+1],Vector3.up+outv*.01f);Tri(v,c,t,prev[k],pts[k+1],pts[k],Vector3.up+outv*.01f);}
   prev=pts;made++;}
  return made;}
 static float GroundAt(Vector3 q,float from,float depth=40)=>Surface(q.x,q.z,from,x=>IsTerrain(x)&&!IsDrive(x)&&!x.name.StartsWith("Ground_Report068"),out var h,depth)?h.point.y:float.NegativeInfinity;

 // ---------- Part C: Mountain Forward Homeward Summit Flight landing (BUG-008) ----------
 // The 32-degree lip lands only ~33-36 m/s riders on the 17 m flat deck; faster riders land on the steep descent
 // (s 2416-2473, often well off-centre) and reach the left bend at s 2483 too fast. Approach, ramp, lip and deck are
 // unchanged. The landing zone is widened along the descent (to 9 m either side of the centre line) and given a runout
 // on the outside of the bend (to 15 m), both flush with the pavement edge, falling 3% outward, on a supported earth skirt.
 static void Landing(){foreach(var old in Object.FindObjectsByType<MeshFilter>().Where(x=>x.name.StartsWith("Ground_Report068 landing")).ToArray())Object.DestroyImmediate(old.gameObject);Physics.SyncTransforms();
  var main=Routes()[0];var v=new List<Vector3>();var c=new List<Color>();var t=new List<int>();var log=new List<string>();
  List<Vector3> Apron(float s,Vector3 edge,float py,Vector3 outv,float taper,float full){var ctr=main.at(s);float edgeLat=Vector3.Dot(edge-ctr,outv);float target=Mathf.Lerp(edgeLat,full,taper);
   var pts=new List<Vector3>{edge+Vector3.down*.03f};if(target-edgeLat<.3f){log.Add($"s {s} side {(Vector3.Dot(outv,Vector3.Cross(Vector3.up,Vector3.ProjectOnPlane(main.fw(s),Vector3.up).normalized))>0?"R":"L")}: pavement already {edgeLat:F1} m from the centre line (target {full} m); nothing to add");return null;}
   for(float d=.5f;;d+=.5f){bool last=d>=target-edgeLat;if(last)d=target-edgeLat;var q=edge+outv*d;q.y=py-.03f-.03f*d;float g=GroundAt(q,q.y+3);
    if(g>q.y){q.y=g-.03f;pts.Add(q);return Pad(pts,64);}pts.Add(q);if(last)break;}
   // Earth skirt: 1:1.5 down to the ground (bounded), so the apron edge is supported, never a floating sheet.
   var top=pts[^1];for(float d=1;d<=12;d+=1){var q=top+outv*d;q.y=top.y-d/1.5f;float g=GroundAt(q,top.y+1);if(float.IsNegativeInfinity(g))break;if(q.y<=g){q.y=g-.05f;pts.Add(q);return Pad(pts,64);}pts.Add(q);}
   var end=pts[^1];float ge=GroundAt(end,end.y+1,60);if(!float.IsNegativeInfinity(ge))pts.Add(new Vector3(end.x,ge-.05f,end.z));return Pad(pts,64);}
  int a=Strip(main,2318,2482,-1,"landing L",v,c,t,(s,e,py,o,tp)=>Apron(s,e,py,o,tp,9),log);
  int b=Strip(main,2318,2482,1,"landing R",v,c,t,(s,e,py,o,tp)=>Apron(s,e,py,o,tp,9),log);
  int rr=Strip(main,2474,2545,1,"runout R",v,c,t,(s,e,py,o,tp)=>Apron(s,e,py,o,tp,15),log);
  File.WriteAllLines("Docs/Report068/partC-landing.txt",log.Prepend($"stations built: descent left {a}, descent right {b}, bend runout {rr}"));
  if(t.Count==0){Note("Part C: nothing built");return;}
  for(int i=0;i<c.Count;i++)c[i]=c[i]==Rock?Rock:Earth;
  Make("landing zone",Store("landing-zone",Build(v,c,t)),GroundMaterial());Physics.SyncTransforms();
  Note($"Part C: Homeward Summit Flight landing widened to 9 m each side along the descent s 2318-2482 ({a}+{b} stations) and a 15 m runout outside the s 2474-2545 bend ({rr} stations); {t.Count/3} tris, collidable earth (Ground_Report068 landing zone); approach/ramp/lip/deck unchanged");
  Dependents(Object.FindObjectsByType<MeshCollider>().First(x=>x.name=="Ground_Report068 landing zone"),"Part C");}
 static List<Vector3> Pad(List<Vector3> p,int n){while(p.Count<n)p.Add(p[^1]);return p;}
 // Grounded objects inside a new surface's footprint: trees/posts that would be buried are raised onto it, rocks noted.
 static void Dependents(MeshCollider mc,string label){int signs=0,trees=0;
  foreach(var col in Object.FindObjectsByType<Collider>().Where(x=>x!=mc&&!x.isTrigger&&!x.attachedRigidbody).ToArray()){var b=col.bounds;if(b.size.x>8||b.size.z>8)continue;
   bool trunk=col.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0;var sign=col.GetComponentInParent<PhysicalSign>();bool post=col.name.IndexOf("post",StringComparison.OrdinalIgnoreCase)>=0;if(!trunk&&!sign&&!post)continue;
   if(!mc.Raycast(new Ray(new Vector3(b.center.x,b.max.y+40,b.center.z),Vector3.down),out var h,120)||h.point.y<b.min.y+.25f||h.point.y>b.max.y)continue;
   // Trees are batched visuals with separate trunk colliders: a base inside new earth reads as a tree on the bank (reported).
   if(trunk&&!sign){trees++;Note($"{label}: tree trunk '{Path(col.transform)}' base now {h.point.y-b.min.y:F2} m inside the new surface (tree stands on it)");continue;}
   var root=sign?sign.transform:col.transform.parent?col.transform.parent:col.transform;float dy=h.point.y-.2f-b.min.y;root.position+=Vector3.up*dy;signs++;Note($"{label}: raised sign/post '{Path(root)}' {dy:F2} m onto the new surface");}
  Note($"{label}: signs/posts raised {signs}; tree bases inside the new surface {trees}");}

 // ---------- Part E: barriers on the outside of turns right after Mountain jumps (Docs/Report068/BARRIERS.md) ----------
 // An earth berm (natural formation) along the outside edge: inner toe 0.6 m outside the pavement (1.4 m on the narrow
 // Downhill Ridge Cut), near-vertical (85 deg) rock inner face up to 1.8 m above the pavement edge (a 65 deg face let an
 // ATV climb and vault it in verification), 0.8 m crown, outer face back down to the ground. One continuous face deflects
 // along the road instead of stopping or launching; the ends taper over 8 m. Never in the driving width, a flight corridor,
 // a shortcut entrance/exit or another route's corridor (stations that would be are left open and logged).
 // High Ridge Drop (Summit Traverse s 8-60) was built and removed: it is the shortcut entrance and the berm touched the
 // full-throttle line and stopped an ATV in the verification pass. It stays a suggestion in BARRIERS.md.
 static void Barriers(){foreach(var old in Object.FindObjectsByType<MeshFilter>().Where(x=>x.name.StartsWith("Ground_Report068 barrier")).ToArray())Object.DestroyImmediate(old.gameObject);Physics.SyncTransforms();
  var sites=Scene=="MountainLoop"?new[]{("Homeward Summit Flight bend","Main",2476f,2545f,1)}
   :new[]{("South Face Summit Flight bend","Main",1279f,1350f,-1),("Downhill Ridge Cut jumps","Downhill Ridge Cut",112f,192f,-1)};
  var routes=Routes();var log=new List<string>();
  foreach(var (name,route,s0,s1,side) in sites){var r=routes.FirstOrDefault(x=>x.n==route);if(r==null){Note($"Part E {name}: route {route} missing");continue;}
   var v=new List<Vector3>();var c=new List<Color>();var t=new List<int>();
   List<Vector3> Berm(float s,Vector3 edge,float py,Vector3 outv,float taper){
    // Start outside any apron/runout surface flush with the road (Part C) so the berm bounds the runout.
    float extra=0;for(float d=.2f;d<16;d+=.2f){var q=edge+outv*d;if(Surface(q.x,q.z,py+1,x=>x.name.StartsWith("Ground_Report068 landing"),out var lh,1.6f)&&lh.point.y>py-1.2f)extra=d;else if(d>extra+.5f)break;}
    float gap=route=="Downhill Ridge Cut"?1.4f:.6f;var b0=edge+outv*(extra+gap);float baseY=extra>0&&Surface(b0.x,b0.z,py+1,x=>x.name.StartsWith("Ground_Report068 landing"),out var bh,3)?bh.point.y:py;
    float gIn=GroundAt(b0,baseY+2,20);if(!float.IsNegativeInfinity(gIn)&&gIn>baseY+1.2f){log.Add($"{name} s {s}: natural bank already {gIn-baseY:F1} m high; no berm");return null;}
    float hgt=(route=="Main"&&Scene=="MountainLoop"?2.4f:1.8f)*Mathf.Max(.15f,taper);var p0=b0;p0.y=Mathf.Min(baseY,float.IsNegativeInfinity(gIn)?baseY:Mathf.Max(gIn,baseY-.6f))-.05f;
    var p1=edge+outv*(extra+gap+hgt/Mathf.Tan(85*Mathf.Deg2Rad));p1.y=baseY+hgt;var p2=p1+outv*.8f;
    var pts=new List<Vector3>{p0,p1,p2};
    for(float d=1;d<=10;d+=1){var q=p2+outv*d;q.y=p2.y-d;float g=GroundAt(q,p2.y+1,30);if(float.IsNegativeInfinity(g)){pts.Add(q);continue;}if(q.y<=g){q.y=g-.1f;pts.Add(q);return Pad(pts,16);}pts.Add(q);}
    var end=pts[^1];float ge=GroundAt(end,end.y+1,80);if(!float.IsNegativeInfinity(ge))pts.Add(new Vector3(end.x,ge-.1f,end.z));return Pad(pts,16);}
   int made=Strip(r,s0,s1,side,name,v,c,t,Berm,log);
   if(t.Count==0){Note($"Part E {name}: nothing built");continue;}
   Make("barrier "+name,Store("barrier-"+name.Split(' ')[0].ToLower()+"-"+route.Replace(' ','-').ToLower(),Build(v,c,t)),GroundMaterial());Physics.SyncTransforms();
   Note($"Part E {Scene} {name}: earth berm on the {(side>0?"right":"left")} (outside) of {route} s {s0}-{s1}: {made} stations, {t.Count/3} tris, {(route=="Main"&&Scene=="MountainLoop"?2.4f:1.8f)} m above the pavement edge, near-vertical (85 deg) inner face");
   Dependents(Object.FindObjectsByType<MeshCollider>().First(x=>x.name=="Ground_Report068 barrier "+name),"Part E "+name);}
  File.AppendAllLines("Docs/Report068/partE-barriers-log.txt",log.Prepend("SCENE "+Scene));}

 // ---------- BUG-006: Reverse main s ~430-485, pavement above the mountain cap ----------
 // The pavement edge stands 0.5-1 m above the MountainCut cap with a dark gap under it and portal-outcrop shards poking up.
 // A collidable earth shoulder runs from each exposed pavement edge (at the edge, 5 cm under it) 1 m flat, then down
 // 1:2.5 to the ground. It stops at other routes' corridors (Summit Traverse entry) and never goes below the cap
 // (lower main route tunnel untouched). Outcrop/terrain vertices that poke above the new shoulder are lowered under it.
 static void CapShoulder(){foreach(var old in Object.FindObjectsByType<MeshFilter>().Where(x=>x.name=="Ground_Report068 BUG-006 shoulder").ToArray())Object.DestroyImmediate(old.gameObject);Physics.SyncTransforms();
  var road=Race.road;road.Initialize();var routes=Routes();var v=new List<Vector3>();var c=new List<Color>();var t=new List<int>();var log=new List<string>();
  bool OtherRoute(Vector3 p){foreach(var r in routes){float s=Near(r,p,out float lat,out float dy);if(r.n=="Main"){float s0=road.Project(p,out _);if(Mathf.Abs(s-457)<80)continue;}if(lat<r.hw(s)+1.5f&&Mathf.Abs(dy)<6)return true;}return false;}
  float[] D={0,.5f,1f,1.6f,2.4f,3.4f,4.6f,6f};
  foreach(int side in new[]{-1,1}){List<Vector3> prev=null;
   for(float s=428;s<=488;s+=1){var ctr=road.At(s,out var f);f=Vector3.ProjectOnPlane(f,Vector3.up).normalized;var right=Vector3.Cross(Vector3.up,f);
    if(!Pave(ctr,ctr.y,out float py)){prev=null;continue;}float w=0;for(;w<14;w+=.1f){if(!Pave(ctr+right*side*(w+.1f),py,out float ny)||Mathf.Abs(ny-py)>.6f)break;py=ny;}
    var edge=ctr+right*side*w;var outside=edge+right*side*1.2f;if(!Surface(outside.x,outside.z,py+.5f,x=>IsTerrain(x)&&!IsDrive(x),out var gh,8)||py-gh.point.y<.25f){prev=null;continue;}
    if(OtherRoute(outside)){prev=null;log.Add($"s {s} side {side}: other route corridor, skipped");continue;}
    var pts=new List<Vector3>();bool toe=false;
    foreach(var d in D){var q=edge+right*side*d;float y=d<=1?py-.05f:py-.05f-(d-1)/2.5f;q.y=y;
     if(Surface(q.x,q.z,py+.3f,x=>IsTerrain(x)&&!IsDrive(x),out var gg,6)&&y<=gg.point.y+.02f){q.y=gg.point.y-.05f;pts.Add(q);toe=true;break;}pts.Add(q);}
    if(!toe){var last=pts[^1];if(Surface(last.x,last.z,last.y+.2f,x=>IsTerrain(x)&&!IsDrive(x),out var g2,6))pts.Add(new Vector3(last.x,g2.point.y-.05f,last.z));}
    while(pts.Count<D.Length+1)pts.Add(pts[^1]);
    if(prev!=null)for(int k=0;k+1<pts.Count;k++){Tri(v,c,t,prev[k],prev[k+1],pts[k+1],Vector3.up);Tri(v,c,t,prev[k],pts[k+1],pts[k],Vector3.up);}
    log.Add($"s {s} side {(side<0?"L":"R")}: edge {edge:F2} pavement {py:F2} ground 1.2 m out {gh.point.y:F2} (step {py-gh.point.y:F2})");prev=pts;}}
  File.WriteAllLines("Docs/Report068/bug006-shoulder.txt",log);if(t.Count==0){Note("BUG-006: no exposed edge found");return;}
  var g=Make("BUG-006 shoulder",Store("bug006-shoulder",Build(v,c,t)),GroundMaterial());Physics.SyncTransforms();var gc=g.GetComponent<MeshCollider>();int shards=0;
  foreach(var mf in Object.FindObjectsByType<MeshFilter>().Where(x=>x.sharedMesh&&x.GetComponent<MeshCollider>()&&IsTerrain(x.GetComponent<MeshCollider>())&&!IsDrive(x.GetComponent<Collider>())&&x.gameObject!=g&&x.GetComponent<Renderer>()&&x.GetComponent<Renderer>().bounds.Intersects(gc.bounds)).ToArray()){
   var m=mf.sharedMesh;var mv=m.vertices;bool any=false;for(int i=0;i<mv.Length;i++){var w=mf.transform.TransformPoint(mv[i]);if(!gc.bounds.Contains(new Vector3(w.x,gc.bounds.center.y,w.z)))continue;
    if(!gc.Raycast(new Ray(new Vector3(w.x,w.y+30,w.z),Vector3.down),out var h,60))continue;if(w.y>h.point.y-.15f&&w.y<h.point.y+1.5f){w.y=h.point.y-.15f;mv[i]=mf.transform.InverseTransformPoint(w);shards++;any=true;}}
   if(any){if(AssetDatabase.GetAssetPath(m).StartsWith("Assets/")){m.vertices=mv;m.RecalculateNormals();m.RecalculateBounds();Refresh(mf);}else Note($"BUG-006: {mf.name} mesh not an asset; shards left");}}
  Note($"BUG-006: collidable earth shoulder along the exposed pavement edges at Reverse main s 428-488 ({t.Count/3} tris, log bug006-shoulder.txt); {shards} terrain/outcrop vertices poking above it lowered under it");}
}
