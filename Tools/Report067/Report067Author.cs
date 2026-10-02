using System;using System.IO;using System.Linq;using System.Reflection;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
// 0.67 local repairs for Dan's 20-report session 2026-10-02_03-04-39-449_547472 (PROJECT_TODO "CURRENT").
// Every change is local and logged to Docs/Report067/author-notes.txt. Route/navigation data is changed only for
// BUG-020 (run-up alignment); no other route, checkpoint, physics, AI or recovery data is touched.
// Section 5A: protected volumes (lower main route, South Face ramp underside and flight corridor, other routes)
// are never filled; the 0.64 Author-SolidReportTerrain regeneration is NOT re-run or extended.
public static class Report067Author {
 const string Folder="Assets/Track/Report067";
 static readonly List<string> notes=new();
 static void Note(string s){notes.Add(s);Debug.Log("REPORT067 "+s);}
 static string Scene=>UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
 static Color Earth=new(.39f,.48f,.29f),Rock=new(.34f,.36f,.29f);
 static bool IsDrive(Collider c)=>c&&c.name.Contains("driving surface");
 static bool IsTerrain(Collider c)=>c is MeshCollider&&!c.isTrigger&&!c.attachedRigidbody&&c.name.StartsWith("Ground");
 // First upward-facing surface accepted by ok, at or below 'from'.
 static bool Surface(float x,float z,float from,Func<Collider,bool> ok,out RaycastHit hit,float depth=260){var o=new Vector3(x,from,z);hit=default;
  for(int k=0;k<60&&depth>0;k++){if(!Physics.Raycast(o,Vector3.down,out var h,depth,~0,QueryTriggerInteraction.Ignore))return false;if(h.normal.y>0&&ok(h.collider)){hit=h;return true;}depth-=h.distance+.02f;o=h.point+Vector3.down*.02f;}return false;}
 static List<RaycastHit> Column(float x,float z,float from=400){var hs=new List<RaycastHit>();var o=new Vector3(x,from,z);for(int k=0;k<60&&Physics.Raycast(o,Vector3.down,out var h,700,~0,QueryTriggerInteraction.Ignore);k++){hs.Add(h);o=h.point+Vector3.down*.02f;}return hs;}
 static Mesh Store(string name,Mesh mesh){Directory.CreateDirectory(Folder);string path=$"{Folder}/{Scene}-{name}.asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(old){EditorUtility.CopySerialized(mesh,old);Object.DestroyImmediate(mesh);return old;}AssetDatabase.CreateAsset(mesh,path);return mesh;}
 static Mesh Build(List<Vector3> v,List<Color> c,List<int> t){var m=new Mesh{indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};m.SetVertices(v);m.SetColors(c);m.SetTriangles(t,0);m.RecalculateNormals();m.RecalculateBounds();return m;}
 // Flat-shaded triangle facing 'want' (rock colour on steep faces).
 static void Tri(List<Vector3> v,List<Color> c,List<int> t,Vector3 a,Vector3 b,Vector3 d,Vector3 want){var n=Vector3.Cross(b-a,d-a);if(n.sqrMagnitude<1e-8f)return;if(Vector3.Dot(n,want)<0){var x=b;b=d;d=x;n=-n;}var col=n.normalized.y>.72f?Earth:Rock;int k=v.Count;v.AddRange(new[]{a,b,d});c.AddRange(new[]{col,col,col});t.AddRange(new[]{k,k+1,k+2});}
 static GameObject Make(string name,Mesh mesh,Material mat){var g=new GameObject("Ground_Report067 "+name,typeof(MeshFilter),typeof(MeshRenderer),typeof(MeshCollider));g.GetComponent<MeshFilter>().sharedMesh=mesh;g.GetComponent<MeshRenderer>().sharedMaterial=mat;g.GetComponent<MeshCollider>().sharedMesh=mesh;GameObjectUtility.SetStaticEditorFlags(g,StaticEditorFlags.BatchingStatic);return g;}
 static Material GroundMaterial(){var r=Object.FindObjectsByType<MeshRenderer>().FirstOrDefault(x=>x.name.Contains("continuous solid 0"))??Object.FindObjectsByType<MeshRenderer>().First(x=>x.name.StartsWith("Ground_Mountain"));return r.sharedMaterial;}
 static void Save(){var s=UnityEngine.SceneManagement.SceneManager.GetActiveScene();EditorSceneManager.MarkSceneDirty(s);EditorSceneManager.SaveScene(s);AssetDatabase.SaveAssets();}
 static void Refresh(MeshFilter mf){if(mf.TryGetComponent<MeshCollider>(out var mc)){mc.sharedMesh=null;mc.sharedMesh=mf.sharedMesh;}EditorUtility.SetDirty(mf.sharedMesh);Physics.SyncTransforms();}
 static RaceDirector Race=>Object.FindAnyObjectByType<RaceDirector>(FindObjectsInactive.Include);

 public static void Run(){
  if(Application.isPlaying)throw new Exception("Edit mode required");Directory.CreateDirectory("Docs/Report067");Physics.queriesHitBackfaces=true;
  try{
   foreach(var scene in new[]{"StreetLoopGreybox","StreetLoopReverse","LakeWoods","ForestLoopReverse","DansBackyardForward","DansBackyardReverse","MountainLoop","MountainLoopReverse"}){
    EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();Note("SCENE "+scene);
    SharedWorld();
    if(scene=="MountainLoopReverse")Reverse();
    if(scene=="MountainLoop")Forward();
    if(scene=="DansBackyardForward")BackyardForward();
    if(scene.StartsWith("Mountain"))Mountain();
    Save();}
  }finally{File.WriteAllLines("Docs/Report067/author-notes.txt",notes);}
  EditorApplication.Exit(0);}

 // ---------- shared-world objects (present in every course scene) ----------
 static void SharedWorld(){
  // BUG-001: woodland-08 acorn-clue cairn floats where later junction work lowered the ground.
  foreach(var g in Object.FindObjectsByType<Transform>().Where(t=>t.name=="Acorn clue / woodland-08").ToArray())SeatGroup(g,"BUG-001 cairn woodland-08");
  // BUG-005: SUMMIT CAMP sign is one physical sign of the shared world; remove it wherever it exists.
  RemoveSigns(t=>t.Contains("SUMMIT CAMP")&&t.Contains("Look for the cairns"),"BUG-005");
  if(Scene=="StreetLoopGreybox")PalePatch(new Vector3(311.8f,8.85f,536.5f));
 }
 static void SeatGroup(Transform g,string label){var rs=g.GetComponentsInChildren<Renderer>();if(rs.Length==0)return;float foot=rs.Min(r=>r.bounds.min.y);var c=rs.Aggregate(rs[0].bounds,(b,r)=>{b.Encapsulate(r.bounds);return b;}).center;
  if(!Surface(c.x,c.z,foot+3,IsTerrain,out var h)){Note(label+": no ground");return;}float dy=h.point.y-.04f-foot;if(Mathf.Abs(dy)<.06f){Note($"{label}: already grounded (gap {-dy:F2})");return;}g.position+=Vector3.up*dy;Note($"{label}: {g.name} moved {dy:F2} m to ground {h.point.y:F2} ({h.collider.name})");}
 static void RemoveSigns(Func<string,bool> match,string label){foreach(var t in Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include).ToArray()){if(!t||!match(t.text))continue;var sign=t.GetComponentInParent<PhysicalSign>(true);var root=sign?sign.gameObject:t.transform.parent.gameObject;Note($"{label}: removed sign '{t.text.Replace('\n','/')}' with post at {root.transform.position}");Object.DestroyImmediate(root);}}
 // BUG-001 pale rectangle: leftover marked-ground vertex colour beside the cairn; blend to the surrounding grass.
 static void PalePatch(Vector3 at){var mf=Object.FindObjectsByType<MeshFilter>().Where(m=>m.name.StartsWith("Ground_")&&m.sharedMesh&&m.GetComponent<Renderer>().bounds.Contains(new Vector3(at.x,m.GetComponent<Renderer>().bounds.center.y,at.z))).OrderBy(m=>m.GetComponent<Renderer>().bounds.size.x).FirstOrDefault();
  if(!mf){Note("BUG-001 patch: no ground mesh");return;}var m=mf.sharedMesh;var v=m.vertices;var c=m.colors;if(c.Length!=v.Length){Note("BUG-001 patch: ground has no vertex colours; material "+mf.GetComponent<Renderer>().sharedMaterial.name);return;}
  var ring=new List<Color>();var inner=new List<int>();for(int i=0;i<v.Length;i++){var w=mf.transform.TransformPoint(v[i]);float d=Vector2.Distance(new(w.x,w.z),new(at.x,at.z));if(d<9)inner.Add(i);else if(d<17)ring.Add(c[i]);}
  if(ring.Count==0){Note("BUG-001 patch: no ring samples");return;}var sorted=new[]{ring.Select(x=>x.r).OrderBy(x=>x).ToArray(),ring.Select(x=>x.g).OrderBy(x=>x).ToArray(),ring.Select(x=>x.b).OrderBy(x=>x).ToArray()};var med=new Color(sorted[0][sorted[0].Length/2],sorted[1][sorted[1].Length/2],sorted[2][sorted[2].Length/2],1);
  int changed=0;var distinct=inner.Select(i=>c[i]).Distinct().Take(8).ToArray();foreach(var i in inner){var x=c[i];if(Mathf.Max(Mathf.Abs(x.r-med.r),Mathf.Abs(x.g-med.g),Mathf.Abs(x.b-med.b))>.05f){c[i]=med;changed++;}}
  Note($"BUG-001 patch: {mf.name} ring median {med}; inner colours {string.Join(" ",distinct.Select(x=>x.ToString()))}; recoloured {changed}/{inner.Count}");if(changed>0){m.colors=c;EditorUtility.SetDirty(m);}}

 // ---------- Mountain scenes (both directions) ----------
 static void Mountain(){
  var race=Race;race.road.Initialize();
  // BUG-002 (and the same cause at BUG-007/008): terrain faces surfacing through the pavement.
  Intrusions();
  // Part C: shoulders under every unsupported pavement edge (after all other geometry changes).
  Banks();
  // BUG-007 (and Part C gaps of the same kind): narrow see-through seams between the pavement edge and the ground.
  Seams();
  // BUG-011: Summit natural edge boulders were plain spheres; replace with faceted rock, seat on the final ground.
  Boulders();
  // BUG-005 (and the same object family): natural edge rocks lifted off the ground.
  foreach(var mf in Object.FindObjectsByType<MeshFilter>().Where(m=>m.name.Contains("natural edge")&&m.name.StartsWith("Ground_ReportCleanup")).ToArray())SeatRock(mf,"BUG-005 natural edge");
  foreach(var g in Object.FindObjectsByType<Transform>().Where(t=>t.name=="Acorn clue / woodland-19").ToArray())SeatGroup(g,"BUG-005 cairn woodland-19");
  // BUG-003: SUMMIT trail sign sunk below the road; raise the board to a readable grounded height.
  foreach(var g in Object.FindObjectsByType<Transform>().Where(t=>t.name.StartsWith("SUMMIT\n")).ToArray())RaiseTrailSign(g,"BUG-003");
  // BUG-014: MOUNTAIN TRAILS sign standing on the driving surface; move it off the pavement.
  foreach(var g in Object.FindObjectsByType<Transform>().Where(t=>t.name.StartsWith("MOUNTAIN TRAILS\n")).ToArray())MoveOffPavement(g,"BUG-014");
 }
 static void RaiseTrailSign(Transform g,string label){var post=g.Find("Trail sign post");var board=g.Find("Trail sign board");if(!post||!board){Note(label+": sign parts missing");return;}
  var br=board.GetComponent<Renderer>().bounds;var pp=post.position;if(!Surface(pp.x,pp.z,br.max.y+4,IsTerrain,out var h)){Note(label+": no ground");return;}
  // Readable from the road: the board bottom must clear both the local ground and the adjacent pavement.
  float ground=h.point.y;var road=Race.road;var rp=road.At(road.Project(pp,out _),out _);float roadY=Surface(rp.x,rp.z,rp.y+3,IsDrive,out var rh,8)?rh.point.y:rp.y;if(Mathf.Abs(roadY-ground)>6||Vector2.Distance(new(rp.x,rp.z),new(pp.x,pp.z))>15)roadY=ground;float reference=Mathf.Max(ground,roadY),dy=reference+1.55f-br.min.y;if(dy<.25f&&dy>-3f){Note($"{label}: '{g.name.Replace('\n','/')}' already readable (board bottom {br.min.y-reference:F2} m above ground/road)");return;}
  foreach(Transform ch in g)if(ch!=post)ch.position+=Vector3.up*dy;float top=br.center.y+dy,bottom=ground-.3f;post.position=new Vector3(pp.x,(top+bottom)/2,pp.z);var ls=post.localScale;ls.y=(top-bottom)/Mathf.Max(.0001f,post.parent.lossyScale.y);post.localScale=ls;
  Note($"{label}: '{g.name.Replace('\n','/')}' board raised {dy:F2} m; board bottom now 1.55 m above max(ground {ground:F2}, road {roadY:F2}); post {bottom:F2}..{top:F2}");}
 static void MoveOffPavement(Transform g,string label){var post=g.Find("Trail sign post");if(!post)return;var drive=Object.FindObjectsByType<MeshCollider>().Where(IsDrive).ToArray();
  bool On(Vector3 p)=>drive.Any(d=>d.Raycast(new Ray(new Vector3(p.x,p.y+6,p.z),Vector3.down),out _,14));var p0=post.position;if(!On(p0)){Note($"{label}: '{g.name.Replace('\n','/')}' not on pavement in this scene");return;}
  var race=Race;Vector3 centre=race.road.At(race.road.Project(p0,out float best),out _);foreach(var b in Object.FindObjectsByType<WoodlandRoute>()){b.Initialize();float s=b.Project(p0,out float lat);if(lat<best){best=lat;centre=b.At(s,out _);}}
  var dir=Vector3.ProjectOnPlane(p0-centre,Vector3.up);dir=dir.sqrMagnitude<.01f?Vector3.right:dir.normalized;var q=p0;for(int i=0;i<60&&On(q);i++)q+=dir*.5f;q+=dir*2.5f;
  if(!Surface(q.x,q.z,q.y+8,c=>IsTerrain(c)&&!IsDrive(c),out var h)){Note(label+": no ground at new spot");return;}float foot=post.GetComponent<Renderer>().bounds.min.y;var shift=new Vector3(q.x-p0.x,h.point.y-.3f-foot,q.z-p0.z);g.position+=shift;
  Note($"{label}: '{g.name.Replace('\n','/')}' moved {new Vector2(shift.x,shift.z).magnitude:F1} m off the pavement to {post.position}, grounded on {h.collider.name}");}
 static Mesh[] rocks;
 static Mesh RockMesh(int seed){var rnd=new System.Random(seed);var t=(1+Mathf.Sqrt(5))/2;var verts=new List<Vector3>{new(-1,t,0),new(1,t,0),new(-1,-t,0),new(1,-t,0),new(0,-1,t),new(0,1,t),new(0,-1,-t),new(0,1,-t),new(t,0,-1),new(t,0,1),new(-t,0,-1),new(-t,0,1)};
  var faces=new List<int[]>{new[]{0,11,5},new[]{0,5,1},new[]{0,1,7},new[]{0,7,10},new[]{0,10,11},new[]{1,5,9},new[]{5,11,4},new[]{11,10,2},new[]{10,7,6},new[]{7,1,8},new[]{3,9,4},new[]{3,4,2},new[]{3,2,6},new[]{3,6,8},new[]{3,8,9},new[]{4,9,5},new[]{2,4,11},new[]{6,2,10},new[]{8,6,7},new[]{9,8,1}};
  var mid=new Dictionary<long,int>();int Mid(int a,int b){long k=Math.Min(a,b)*1000L+Math.Max(a,b);if(!mid.TryGetValue(k,out int i)){i=verts.Count;verts.Add((verts[a]+verts[b])/2);mid[k]=i;}return i;}
  var sub=new List<int[]>();foreach(var f in faces){int a=Mid(f[0],f[1]),b=Mid(f[1],f[2]),e=Mid(f[2],f[0]);sub.AddRange(new[]{new[]{f[0],a,e},new[]{f[1],b,a},new[]{f[2],e,b},new[]{a,b,e}});}
  for(int i=0;i<verts.Count;i++){var d=verts[i].normalized;float r=.5f*(.78f+.3f*(float)rnd.NextDouble());var p=d*r;if(p.y<-.3f)p.y=-.3f-(p.y+.3f)*.25f;verts[i]=p;}
  var v=new List<Vector3>();var c=new List<Color>();var tr=new List<int>();foreach(var f in sub){var col=Color.Lerp(new Color(.36f,.37f,.33f),new Color(.47f,.47f,.42f),(float)rnd.NextDouble());int k=v.Count;v.AddRange(new[]{verts[f[0]],verts[f[1]],verts[f[2]]});c.AddRange(new[]{col,col,col});var n=Vector3.Cross(verts[f[1]]-verts[f[0]],verts[f[2]]-verts[f[0]]);tr.AddRange(Vector3.Dot(n,verts[f[0]])>=0?new[]{k,k+1,k+2}:new[]{k,k+2,k+1});}
  var m=Build(v,c,tr);m.name="Report067 faceted rock "+seed;return m;}
 static void Boulders(){var list=Object.FindObjectsByType<MeshFilter>().Where(m=>m.name=="Summit natural edge boulder").ToArray();if(list.Length==0)return;
  rocks??=new Mesh[3];for(int i=0;i<3;i++){var path=$"{Folder}/faceted-rock-{i}.asset";rocks[i]=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(!rocks[i]){Directory.CreateDirectory(Folder);rocks[i]=RockMesh(67+i);AssetDatabase.CreateAsset(rocks[i],path);}}
  var rockMat=Object.FindObjectsByType<MeshRenderer>().FirstOrDefault(r=>r.name.Contains("natural outcrop")||r.name.Contains("natural edge"))?.sharedMaterial;int n=0;
  foreach(var mf in list){var mesh=rocks[n%3];mf.sharedMesh=mesh;if(rockMat)mf.GetComponent<MeshRenderer>().sharedMaterial=rockMat;var sc=mf.GetComponent<SphereCollider>();if(sc)Object.DestroyImmediate(sc);if(!mf.TryGetComponent<MeshCollider>(out var mc))mc=mf.gameObject.AddComponent<MeshCollider>();mc.sharedMesh=mesh;mc.convex=true;
   mf.transform.rotation=Quaternion.Euler(0,(n*67)%360,0);n++;Physics.SyncTransforms();SeatRock(mf,"BUG-011 boulder",.25f,true);}
  Note($"BUG-011: {n} 'Summit natural edge boulder' spheres replaced by faceted rock meshes (3 variants) with matching convex mesh colliders; material {(rockMat?rockMat.name:"unchanged")}");}
 // Seat a rock/boulder whose underside floats: shift it down until its low vertices are embedded.
 static void SeatRock(MeshFilter mf,string label,float embed=.25f,bool both=false){var self=mf.GetComponent<Collider>();bool terrainOnly=mf.name.Contains("natural edge");var vs=mf.sharedMesh.vertices.Select(v=>mf.transform.TransformPoint(v)).ToArray();float lo=vs.Min(v=>v.y),hi=vs.Max(v=>v.y);
  var gaps=new List<float>();foreach(var v in vs.Where(v=>v.y<lo+(hi-lo)*.3f)){if(Surface(v.x,v.z,hi+1,c=>IsTerrain(c)&&c!=self&&!(terrainOnly&&IsDrive(c)),out var h))gaps.Add(v.y-h.point.y);}
  if(gaps.Count<3){Note($"{label}: {mf.name} no ground samples");return;}gaps.Sort();float p25=gaps[gaps.Count/4],p50=gaps[gaps.Count/2];
  // A rock on a slope can touch on its uphill side while its downhill half hangs in the air: use the median then.
  if(!both&&terrainOnly&&p50>.3f)p25=p50-embed+.1f;
  if(both?Mathf.Abs(p25+embed)<.1f:p25<.12f)return;float dy=-(p25+embed);mf.transform.position+=Vector3.up*dy;Physics.SyncTransforms();Note($"{label}: {mf.name} at {mf.transform.position} moved {dy:F2} m (underside gap p25 {p25:F2})");}



 // ---------- BUG-002 / BUG-007-008: terrain surfacing through the pavement ----------
 // 0.64 voxel terrain sits 0.1 m under the pavement; where it rises through it, the vehicle body strikes a
 // steep face and is launched/rolled (drive contact logs: solid 1 at main s 134-138, solid 0 at s 1576).
 // Scan every route every 0.5 m across the real pavement width; lower only the intruding terrain vertices that
 // lie under the pavement to 0.15 m below it (second pass: edge vertices to 0.05 m below the pavement edge).
 // Pavement, routes, rocks/outcrops/barriers and all other geometry are unchanged.
 static void Intrusions(){var race=Race;var road=race.road;road.Initialize();var branches=Object.FindObjectsByType<WoodlandRoute>().Where(b=>b.gameObject.activeInHierarchy).ToArray();foreach(var b in branches)b.Initialize();
  var routes=new List<(string n,Func<float,Vector3> at,Func<float,Vector3> fw,float len)>{("Main",x=>road.At(x,out _),x=>{road.At(x,out var f);return f;},road.Length)};
  foreach(var b in branches){var bb=b;routes.Add((b.title,x=>bb.At(x,out _),x=>{bb.At(x,out var f);return f;},b.Length));}
  bool Barrier(Collider c)=>c.name.Contains("natural edge")||c.name.Contains("outcrop")||c.name.Contains("boulder")||c.name.Contains("vault")||c.name.Contains("mountain cap")||c.name.Contains("tunnel ceiling");
  bool Pave(Vector3 q,float near,out float y){y=0;if(!Surface(q.x,q.z,near+2.5f,IsDrive,out var h,5))return false;y=h.point.y;return true;}
  bool PaveBelow(Vector3 q,out float y){y=0;if(!Surface(q.x,q.z,q.y+1f,IsDrive,out var h,14))return false;y=h.point.y;return true;}
  List<(RaycastHit h,string where)> Scan(){var found=new List<(RaycastHit,string)>();
   foreach(var r in routes)for(float st=0;st<r.len;st+=.5f){var c=r.at(st);var f=Vector3.ProjectOnPlane(r.fw(st),Vector3.up).normalized;var right=Vector3.Cross(Vector3.up,f);
    for(float o=-9;o<=9;o+=.5f){var q=c+right*o;if(!Pave(q,c.y,out float py))continue;var org=new Vector3(q.x,py+2.5f,q.z);float depth=2.5f-.02f;
     for(int k=0;k<8&&depth>0&&Physics.Raycast(org,Vector3.down,out var h,depth,~0,QueryTriggerInteraction.Ignore);k++){if(IsTerrain(h.collider)&&!IsDrive(h.collider)&&h.point.y>py-.02f){found.Add((h,$"{r.n} s={st:F1} o={o:F1}"));break;}depth-=h.distance+.01f;org=h.point+Vector3.down*.01f;}}}
   return found;}
  var log=new List<string>();int lowered=0;var hits=Scan();
  foreach(var g in hits.GroupBy(x=>x.h.collider.name+"|"+x.where.Split(' ')[0]+"|"+Mathf.Floor(float.Parse(x.where.Split('=')[1].Split(' ')[0])/10)))log.Add($"INTRUSION {g.Key} n={g.Count()} first={g.First().where} at={g.First().h.point.ToString("F2")}");
  for(int pass=0;pass<4&&hits.Count>0;pass++){
   foreach(var g in hits.GroupBy(x=>x.h.collider).ToArray()){var col=g.Key as MeshCollider;if(Barrier(col)){log.Add($"BARRIER left unchanged (pass {pass+1}): {col.name} ({g.Count()} samples, e.g. {g.First().where})");continue;}
    var mf=col.GetComponent<MeshFilter>();if(!mf||mf.sharedMesh!=col.sharedMesh){log.Add($"collider/mesh mismatch: {col.name}");continue;}
    var m=mf.sharedMesh;var v=m.vertices;var tri=m.triangles;bool any=false;var done=new HashSet<int>();
    foreach(var (h,_) in g){if(h.triangleIndex<0)continue;for(int k=0;k<3;k++){int vi=tri[h.triangleIndex*3+k];if(!done.Add(vi))continue;var w=mf.transform.TransformPoint(v[vi]);
      if(Pave(w,w.y,out float py)||PaveBelow(w,out py)){if(w.y>py-.15f){w.y=py-.15f;v[vi]=mf.transform.InverseTransformPoint(w);lowered++;any=true;}continue;}
      float edge=float.NaN;for(float r=.25f;r<=3f&&float.IsNaN(edge);r+=.25f)foreach(var dir in new[]{Vector3.forward,Vector3.back,Vector3.left,Vector3.right,new Vector3(.71f,0,.71f),new Vector3(.71f,0,-.71f),new Vector3(-.71f,0,.71f),new Vector3(-.71f,0,-.71f)})if(Pave(w+dir*r,w.y,out float ey)||PaveBelow(w+dir*r,out ey))edge=float.IsNaN(edge)?ey:Mathf.Min(edge,ey);
      if(!float.IsNaN(edge)&&w.y>edge-.05f){w.y=edge-.05f;v[vi]=mf.transform.InverseTransformPoint(w);lowered++;any=true;}}}
    if(any){m.vertices=v;m.RecalculateNormals();m.RecalculateBounds();Refresh(mf);}}
   hits=Scan();log.Add($"after pass {pass+1}: remaining intrusion samples={hits.Count}");}
  foreach(var g in hits.GroupBy(x=>x.h.collider.name))log.Add($"REMAINING {g.Key}: {g.Count()} e.g. {g.First().where} {g.First().h.point.ToString("F2")}");
  // Body clearance: the vehicle body collider is not supported by the pavement (wheels are raycasts) and dips up
  // to ~0.5 m below it under suspension compression; 0.64 voxel terrain lying within 1.2 m under the pavement is
  // struck from below (contact logs: solid 1 at s 134-138, solid 0 at s 1576, impulses up to 1418). Every terrain
  // vertex with pavement directly above it within 1.2 m is lowered to 1.2 m below that pavement (hidden by it).
  int deep=0,meshes=0;var corridor=new List<Bounds>();foreach(var r in routes)for(float st=0;st<r.len;st+=10){var b=new Bounds(r.at(st),new Vector3(30,12,30));corridor.Add(b);}
  foreach(var mf in Object.FindObjectsByType<MeshFilter>().Where(m=>m.sharedMesh&&m.GetComponent<MeshCollider>()&&IsTerrain(m.GetComponent<MeshCollider>())&&!IsDrive(m.GetComponent<Collider>())&&!Barrier(m.GetComponent<Collider>())).ToArray()){var mb=mf.GetComponent<Renderer>().bounds;if(!corridor.Any(b=>b.Intersects(mb)))continue;
   var m=mf.sharedMesh;var v=m.vertices;bool any=false;
   for(int i=0;i<v.Length;i++){var w=mf.transform.TransformPoint(v[i]);if(!corridor.Any(b=>b.Contains(w)))continue;var o=w;float up=1.2f;
    for(int k=0;k<4&&up>0&&Physics.Raycast(o,Vector3.up,out var h,up,~0,QueryTriggerInteraction.Ignore);k++){if(IsDrive(h.collider)){float target=h.point.y-1.2f;if(w.y>target){w.y=target;v[i]=mf.transform.InverseTransformPoint(w);deep++;any=true;}break;}up-=h.distance+.01f;o=h.point+Vector3.up*.01f;}}
   if(any){m.vertices=v;m.RecalculateNormals();m.RecalculateBounds();Refresh(mf);meshes++;}}
  log.Add($"BODY CLEARANCE: {deep} terrain vertices in {meshes} meshes lowered to 1.2 m under the pavement above them");
  File.WriteAllLines($"Docs/Report067/intrusions-{Scene}.txt",log);Note($"BUG-002/007/008 intrusions {Scene}: terrain vertices lowered under/at the pavement={lowered}; remaining samples={hits.Count}; body-clearance vertices={deep} (see intrusions-{Scene}.txt)");}
 // ---------- Part C: edge shoulders ----------
 // A pavement edge is unsupported when ground 0.8 m outside it is >1.2 m lower (or a back face) and no
 // front-facing bank meets it (same rule as Tools/Report067/Report067Sweep.cs). Each unsupported edge gets a
 // local shoulder from just inside the pavement edge down to existing ground (37 deg, then 54 deg). Shoulders
 // stop outside other routes' corridors and never enter a flight system (approach..landing) or its corridor.
 static void Banks(){foreach(var old in Object.FindObjectsByType<MeshFilter>().Where(m=>m.name=="Ground_Report067 edge shoulders").ToArray())Object.DestroyImmediate(old.gameObject);Physics.SyncTransforms();
  var race=Race;var road=race.road;road.Initialize();var mat=GroundMaterial();
  var branches=Object.FindObjectsByType<WoodlandRoute>().Where(b=>b.gameObject.activeInHierarchy).ToArray();foreach(var b in branches)b.Initialize();
  var flights=Object.FindAnyObjectByType<MountainFlights>(FindObjectsInactive.Include);
  var routes=new List<(string n,Func<float,Vector3> at,Func<float,Vector3> fw,float len,Func<float,float> hw)>{("Main",x=>road.At(x,out _),x=>{road.At(x,out var f);return f;},road.Length,x=>road.HalfWidth(x))};
  foreach(var b in branches){var bb=b;routes.Add((b.title,x=>bb.At(x,out _),x=>{bb.At(x,out var f);return f;},b.Length,x=>bb.halfWidth));}
  bool FlightStation(string r,float st){if(r!="Main"||!flights)return false;foreach(var f in flights.flights)if(st>=f.approachStation-40&&st<=f.endStation)return true;return false;}
  bool Corridor(Vector3 p){if(!flights)return false;foreach(var f in flights.flights){var ax=Vector3.ProjectOnPlane(f.landingEnd-f.start,Vector3.up).normalized;float along=Vector3.Dot(p-f.start,ax),tot=Vector3.Dot(f.landingEnd-f.start,ax),lat=Mathf.Abs(Vector3.Dot(p-f.start,Vector3.Cross(Vector3.up,ax)));if(along>-5&&along<tot+5&&lat<26)return true;}return false;}
  // Another route (or a distant part of the main route) whose corridor contains p below the bank's pavement.
  bool Other(Vector3 p,float py,string self,float st,out float otherY){otherY=0;
   for(int i=0;i<road.points.Length-1;i++){var a=road.points[i];var v=road.points[i+1]-a;float t=Mathf.Clamp01(Vector3.Dot(p-a,v)/v.sqrMagnitude);var q=a+v*t;if(Vector2.Distance(new(q.x,q.z),new(p.x,p.z))>9.5f||q.y>py-1)continue;if(self=="Main"&&Mathf.Abs(road.Project(q,out _)-st)<40)continue;otherY=q.y;return true;}
   foreach(var b in branches){if(b.title==self)continue;float bs=b.Project(p,out float bl);var q=b.At(bs,out _);if(bl<b.halfWidth+2.5f&&q.y<py-1){otherY=q.y;return true;}}return false;}
  bool OnPave(Vector3 q,float near,out float y){y=0;if(!Surface(q.x,q.z,near+2.5f,IsDrive,out var h,5))return false;y=h.point.y;return true;}
  // Supported: ground within 0.35 m of the pavement 0.8 m outside the edge, or a sloped embankment (its face at 0.6 m
  // below the pavement lies at least 0.45 m outside the edge). BUG-007: a near-vertical face straight under a jagged
  // edge, stepping 0.35-1.2 m down to the ground, reads as a sawtooth hole and is not support.
  bool Supported(Vector3 c,Vector3 right,int side,float w,float py){var outside=c+right*side*(w+.8f);
   if(Physics.Raycast(new Vector3(outside.x,py+2,outside.z),Vector3.down,out var dh,80,~0,QueryTriggerInteraction.Ignore)&&IsTerrain(dh.collider)&&dh.normal.y>0&&py-dh.point.y<.35f)return true;
   var from=c+right*side*(w+3f);from.y=py-.6f;return Physics.Raycast(from,-right*side,out var wh,3.4f,~0,QueryTriggerInteraction.Ignore)&&IsTerrain(wh.collider)&&Vector3.Dot(wh.normal,right*side)>0&&Vector3.Dot(wh.point-c,right*side)>=w+.45f;}
  // Sawtooth pavement edge: pavement pieces continue past the first gap; returns how far (m) or -1.
  float Notch(Vector3 c,Vector3 right,int side,float w,float py){float far=-1;for(float d=.25f;d<=3f;d+=.25f){var q=c+right*side*(w+d);if(OnPave(q,py,out float y)&&Mathf.Abs(y-py)<.6f)far=d;}return far<0?-1:far+.3f;}
  float[] D={0,.25f,.5f,1,1.6f,2.4f,3.4f,4.6f,6,8,10.5f,13.5f,17,21,26,32,39,47};
  var v=new List<Vector3>();var col=new List<Color>();var tri=new List<int>();var log=new List<string>();int runs=0,stations=0,skippedFlight=0;
  foreach(var r in routes)foreach(var side in new[]{-1,1}){
   var flagged=new List<float>();
   // Any pavement gap on this route is a jump: its approach (60 m), flight and landing (+10 m) are protected (5A.5).
   var jumps=new List<(float a,float b)>();{float gap=-1;for(float st=0;st<r.len;st+=1){var c0=r.at(st);bool on=OnPave(c0,c0.y,out _);if(!on&&gap<0)gap=st;if(on&&gap>=0){if(st-gap>=2)jumps.Add((gap-60,st+10));gap=-1;}}}
   bool InJump(float st)=>jumps.Any(j=>st>=j.a&&st<=j.b);
   List<Vector3> Profile(float st){var c=r.at(st);var f=Vector3.ProjectOnPlane(r.fw(st),Vector3.up).normalized;var right=Vector3.Cross(Vector3.up,f);float hw=r.hw(st);var e=c+right*side*(hw-.3f);if(!OnPave(e,c.y,out float py))return null;
    float w=hw-.3f;for(;w<hw+6;w+=.25f){if(!OnPave(c+right*side*(w+.25f),py,out float ny)||Mathf.Abs(ny-py)>.6f)break;py=ny;}
    float notch=Notch(c,right,side,w,py);
    float G(float d){var q=c+right*side*(w+d);return Surface(q.x,q.z,py+.5f,x=>IsTerrain(x)&&!IsDrive(x),out var gh)?gh.point.y:float.NegativeInfinity;}
    // Gutter: a shallow trough between the pavement edge and a wall rising within 8 m is filled flush.
    float wallAt=-1;for(float d=.5f;d<=8;d+=.5f){float g=G(d);if(g<py-3){wallAt=-1;break;}if(g>py-.3f){wallAt=d;break;}}
    // Starts only 0.15 m inside the pavement edge: the vehicle body dips below the pavement and must not meet it.
    var pts=new List<Vector3>{new Vector3(0,0,0)};var p0=c+right*side*(w-.15f);p0.y=py-.12f;pts[0]=p0;bool toe=false;
    foreach(var d in D){var q=c+right*side*(w+d);float flat=Mathf.Max(notch,wallAt);float y=d<=flat?py-.12f:py-.12f-(d-Mathf.Max(0,flat)<=4?.75f*(d-Mathf.Max(0,flat)):3+1.4f*(d-Mathf.Max(0,flat)-4));q.y=y;
     if(Corridor(q)){var last=pts[^1];if(Surface(last.x,last.z,last.y,x=>IsTerrain(x)&&!IsDrive(x),out var gh))pts.Add(new Vector3(last.x,gh.point.y-.3f,last.z));toe=true;break;}
     if(Other(q,py,r.n,st,out _)){toe=true;break;}
     if(Surface(q.x,q.z,py+.5f,x=>IsTerrain(x)&&!IsDrive(x),out var g)&&y<=g.point.y){q.y=g.point.y-.3f;pts.Add(q);toe=true;break;}pts.Add(q);}
    if(!toe){var last=pts[^1];if(Surface(last.x,last.z,last.y,x=>IsTerrain(x)&&!IsDrive(x),out var gh))pts.Add(new Vector3(last.x,gh.point.y-.3f,last.z));}
    // Any shoulder point under any pavement (merges, notch underlays, the edge strip) sits 1.2 m below it,
    // the same body clearance as the terrain (the vehicle body dips below the pavement surface).
    // Exception: the flat underlay beneath short sawtooth notches (<=1.2 m, outside the driven width) stays just under
    // the surface so the notch gaps show ground, not pits.
    for(int k=0;k<pts.Count;k++){var q=pts[k];float lat=Vector3.Dot(q-c,right*side);if(notch>0&&notch<=1.2f&&lat>w-.01f&&lat<=w+notch+.01f)continue;if(Surface(q.x,q.z,q.y+1.6f,IsDrive,out var ph,1.6f)&&ph.point.y>=q.y-.01f){q.y=Mathf.Min(q.y,ph.point.y-1.2f);pts[k]=q;}}
    while(pts.Count<D.Length+2)pts.Add(pts[^1]);return pts;}
   for(float st=0;st<r.len;st+=2){var c=r.at(st);var f=Vector3.ProjectOnPlane(r.fw(st),Vector3.up).normalized;var right=Vector3.Cross(Vector3.up,f);float hw=r.hw(st);var e=c+right*side*(hw-.3f);if(!OnPave(e,c.y,out float py))continue;
    float w=hw-.3f;for(;w<hw+6;w+=.25f){if(!OnPave(c+right*side*(w+.25f),py,out float ny)||Mathf.Abs(ny-py)>.6f)break;py=ny;}
    if(Supported(c,right,side,w,py)&&Notch(c,right,side,w,py)<0)continue;
    {var probe=c+right*side*(w+1.5f);if(Other(probe,py,r.n,st,out _)){skippedFlight++;log.Add($"PROTECTED {r.n} s={st} side={(side<0?"L":"R")} p={c.ToString("F1")} (above another route: multi-level clearance, not filled)");continue;}}if(FlightStation(r.n,st)||InJump(st)){skippedFlight++;log.Add($"PROTECTED {r.n} s={st} side={(side<0?"L":"R")} p={c.ToString("F1")} (jump/flight system approach..landing; not changed)");continue;}flagged.Add(st);}
   // Runs of flagged stations, each padded by one station at both ends so the shoulder tapers out.
   var set=new SortedSet<float>();foreach(var st in flagged){set.Add(st);if(st-2>=0)set.Add(st-2);if(st+2<r.len)set.Add(st+2);}
   List<Vector3> prev=null;float prevS=-99;int runLen=0;float runStart=0;
   foreach(var st in set){var pr=Profile(st);if(pr==null){prev=null;continue;}
    if(prev!=null&&st-prevS<=2.01f){for(int k=0;k+1<pr.Count;k++){var want=Vector3.up;Tri(v,col,tri,prev[k],prev[k+1],pr[k+1],want);Tri(v,col,tri,prev[k],pr[k+1],pr[k],want);}runLen++;}
    else{if(runLen>0)log.Add($"{r.n} side={(side<0?"L":"R")} s={runStart}-{prevS} ({runLen*2} m) start={r.at(runStart).ToString("F1")}");runs++;runStart=st;runLen=0;}
    prev=pr;prevS=st;stations++;}
   if(runLen>0)log.Add($"{r.n} side={(side<0?"L":"R")} s={runStart}-{prevS} ({runLen*2} m) start={r.at(runStart).ToString("F1")}");}
  File.WriteAllLines($"Docs/Report067/partC-{Scene}.txt",log.Prepend($"{Scene}: shoulder runs={runs}, stations={stations}, protected flight-system edges left unchanged={skippedFlight}"));
  if(tri.Count==0){Note($"Part C: {Scene} no unsupported pavement edges");return;}
  var bank=Make("edge shoulders",Store("edge-shoulders",Build(v,col,tri)),mat);Physics.SyncTransforms();var bc=bank.GetComponent<MeshCollider>();
  // Dependent objects: nothing may end up buried by a new shoulder.
  bool Buried(Collider c){var b=c.bounds;return bc.Raycast(new Ray(new Vector3(b.center.x,b.min.y+40,b.center.z),Vector3.down),out var h,80)&&h.point.y>b.min.y+.3f;}
  int trees=Object.FindObjectsByType<Collider>().Count(c=>c.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0&&Buried(c));if(trees>0)TreesHelper(c=>false,Buried);
  int signs=0;foreach(var post in Object.FindObjectsByType<Renderer>().Where(r=>r.name.IndexOf("post",StringComparison.OrdinalIgnoreCase)>=0).ToArray()){var b=post.bounds;if(!bc.Raycast(new Ray(new Vector3(b.center.x,b.max.y+40,b.center.z),Vector3.down),out var h,90)||h.point.y<b.min.y+.3f)continue;var root=post.transform.parent?post.transform.parent:post.transform;float dy=h.point.y-.2f-b.min.y;root.position+=Vector3.up*dy;signs++;Note($"Part C: raised '{root.name.Replace('\n','/')}' {dy:F2} m onto new shoulder");}
  Note($"Part C: {Scene} edge shoulders runs={runs} stations={stations} tris={tri.Count/3}; protected flight-system edges unchanged={skippedFlight}; trees reseated={trees}; sign posts raised={signs}");}
 // BUG-007: the hole is a 0.25-0.5 m see-through seam between the jagged pavement edge and the earth bank, which
 // sits at road level just beyond it (only the terrain 1.3 m below shows through); at BUG-008 the pavement's
 // sawtooth fringe leaves pits (the shoulder underlay 0.1-0.5 m down) between its teeth. Anything more than 0.12 m
 // below the pavement counts as open. Every such seam along the Mountain routes (a gap at the edge
 // that is closed again by ground or pavement at road level within 3 m) gets a thin earth-coloured cover 4-6 cm
 // under the surrounding surfaces. Visual only: no collider, so driving, wheel contact and jump systems are unchanged.
 static void Seams(){foreach(var old in Object.FindObjectsByType<MeshFilter>().Where(m=>m.name=="Ground_Report067 edge seams").ToArray())Object.DestroyImmediate(old.gameObject);Physics.SyncTransforms();
  var road=Race.road;road.Initialize();var branches=Object.FindObjectsByType<WoodlandRoute>().Where(b=>b.gameObject.activeInHierarchy).ToArray();foreach(var b in branches)b.Initialize();
  var flights=Object.FindAnyObjectByType<MountainFlights>(FindObjectsInactive.Include);
  var routes=new List<(string n,Func<float,Vector3> at,Func<float,Vector3> fw,float len,Func<float,float> hw)>{("Main",x=>road.At(x,out _),x=>{road.At(x,out var f);return f;},road.Length,x=>road.HalfWidth(x))};
  foreach(var b in branches){var bb=b;routes.Add((b.title,x=>bb.At(x,out _),x=>{bb.At(x,out var f);return f;},b.Length,x=>bb.halfWidth));}
  bool Solid(Collider c)=>!c.isTrigger&&!c.attachedRigidbody&&(c is MeshCollider||c is TerrainCollider);
  bool Pave(Vector3 q,float near,out float y){y=0;if(!Surface(q.x,q.z,near+2.5f,IsDrive,out var h,5))return false;y=h.point.y;return true;}
  bool Top(Vector3 q,float py,out float y){y=0;if(!Physics.Raycast(new Vector3(q.x,py+.6f,q.z),Vector3.down,out var h,.72f,~0,QueryTriggerInteraction.Ignore)||h.normal.y<=.3f||!Solid(h.collider))return false;y=h.point.y;return true;}
  // Fixed lateral grid (0.1 m) per route side, stations every 0.25 m: each sample is pavement/ground at road level
  // (covered), a seam sample (open, but closed again at road level within 1 m), or outside (open beyond).
  const int K=64;const float step=.25f;var v=new List<Vector3>();var col=new List<Color>();var tri=new List<int>();var log=new List<string>();int seams=0,stations=0;
  foreach(var r in routes)foreach(var side in new[]{-1,1}){
   var gaps=new List<(float a,float b)>();{float g=-1;for(float st=0;st<r.len;st+=1){var c0=r.at(st);bool on=Pave(c0,c0.y,out _);if(!on&&g<0)g=st;if(on&&g>=0){if(st-g>=2)gaps.Add((g-60,st+10));g=-1;}}}
   Vector3[] pv=null;int[] ps=null;float runStart=-1,last=-1;
   for(float st=0;st<r.len;st+=step){Vector3[] cv=null;int[] cs=null;
    bool flight=false;if(r.n=="Main"&&flights)foreach(var f in flights.flights)if(st>=f.approachStation-40&&st<=f.endStation)flight=true;
    var c=r.at(st);var fwd=Vector3.ProjectOnPlane(r.fw(st),Vector3.up).normalized;var right=Vector3.Cross(Vector3.up,fwd);float hw=r.hw(st);
    if(!flight&&!gaps.Any(j=>st>=j.a&&st<=j.b)&&Pave(c+right*side*(hw-.3f),c.y,out float py)){
     cv=new Vector3[K+1];cs=new int[K+1];// 1 covered, 2 seam, 0 outside
     for(int k=0;k<=K;k++){var q=c+right*side*(hw-.3f+k*.1f);if(Top(q,py,out float ty)){cs[k]=1;q.y=ty-.05f;py=Mathf.Lerp(py,ty,.5f);}cv[k]=q;}
     int lastCov=0;for(int k=1;k<=K;k++){if(cs[k]!=1)continue;if(k-lastCov>1&&k-lastCov<=11){for(int j=lastCov+1;j<k;j++){cs[j]=2;cv[j].y=Mathf.Lerp(cv[lastCov].y,cv[k].y,(j-lastCov)/(float)(k-lastCov));}}else if(k-lastCov>11)break;lastCov=k;}
     for(int k=lastCov+1;k<=K;k++)if(cs[k]==1)cs[k]=0;
     if(!cs.Contains(2)){cv=null;cs=null;}}
    if(cv!=null){stations++;if(last<0||st-last>step+.01f){runStart=st;seams++;}last=st;
     if(pv!=null)for(int k=0;k<K;k++){var q=new[]{ps[k],ps[k+1],cs[k+1],cs[k]};if(q.Contains(0)||!q.Contains(2))continue;Tri(v,col,tri,pv[k],pv[k+1],cv[k+1],Vector3.up);Tri(v,col,tri,pv[k],cv[k+1],cv[k],Vector3.up);}}
    else if(pv!=null)log.Add($"{r.n} side={(side<0?"L":"R")} s={runStart:F2}-{last:F2} at {r.at(runStart).ToString("F1")}");
    pv=cv;ps=cs;}
   if(pv!=null)log.Add($"{r.n} side={(side<0?"L":"R")} s={runStart:F2}-{last:F2} at {r.at(runStart).ToString("F1")}");}
  for(int i=0;i<col.Count;i++)col[i]=Earth;
  File.AppendAllLines($"Docs/Report067/partC-{Scene}.txt",log.Prepend($"SEAMS (visual cover, no collider): runs={seams} stations(0.25 m)={stations}"));
  if(tri.Count==0){Note($"BUG-007 seams: {Scene} none");return;}
  var g2=Make("edge seams",Store("edge-seams",Build(v,col,tri)),GroundMaterial());Object.DestroyImmediate(g2.GetComponent<MeshCollider>());
  Note($"BUG-007 seams: {Scene} runs={seams} stations={stations} tris={tri.Count/3} (visual cover, no collider)");}
 // ---------- MountainLoopReverse ----------
 static void Reverse(){var race=Race;var road=race.road;road.Initialize();
  RemoveSigns(t=>t.Contains("LEFT TO FULL RUN-UP"),"BUG-004");
  RemoveSigns(t=>t.Contains("CLEAR THE MAIN ROAD"),"BUG-013");
  Fern();
  SmoothGrade(road,1602,1625,"BUG-008");
  SouthFaceFill(road);
  SeatArrows(road,1495,1632,"BUG-006");
 }
 static void Fern(){var site=Object.FindObjectsByType<ActivitySite>(FindObjectsInactive.Include).FirstOrDefault(a=>a.title=="Fern Creek Leap");if(site){var r=Race.road;float s=r.Project(site.transform.position,out float lat);Note($"Fern Creek Leap activity at {site.transform.position} main s={s:F1} lateral={lat:F1}; BUG-008 window 1602-1625 is approach-only");}}
 static float Centre(RaceRoad road,float s){var p=road.At(s,out _);return Surface(p.x,p.z,p.y+3,IsDrive,out var h,8)?h.point.y:float.NaN;}
 // BUG-008: the shelf is a 0.9 m flat followed by a ~45 deg, 0.9 m step in the pavement. Route stations are degenerate
 // here (the navigation polyline climbs almost vertically: s 1614-1617 covers <1 m horizontally), so the grade is
 // rebuilt on horizontal distance u along the road: a Hermite profile per lateral offset between the unchanged ends.
 // Only the top pavement layer is placed on the target; lower layers and nearby terrain move with it.
 static void SmoothGrade(RaceRoad road,float a,float b,string label){
  // Main pavement only (|o| <= 7.5 m); nothing within a branch corridor (Summit Traverse merges beside this window).
  var branches=Object.FindObjectsByType<WoodlandRoute>().Where(x=>x.gameObject.activeInHierarchy).ToArray();foreach(var x in branches)x.Initialize();
  int i0=Mathf.Max(0,Array.FindIndex(road.points,p=>road.Project(p,out _)>=a-40)),i1=Math.Min(road.points.Length-1,Array.FindLastIndex(road.points,p=>road.Project(p,out _)<=b+40));
  var P=new List<Vector2>();for(int i=i0;i<=i1;i++){var q=new Vector2(road.points[i].x,road.points[i].z);if(P.Count==0||Vector2.Distance(P[^1],q)>.05f)P.Add(q);}
  var U=new float[P.Count];for(int i=1;i<P.Count;i++)U[i]=U[i-1]+Vector2.Distance(P[i-1],P[i]);
  float Proj(Vector2 q,out float o){float best=float.MaxValue,bu=0;o=0;for(int i=0;i+1<P.Count;i++){var v=P[i+1]-P[i];float t=Mathf.Clamp01(Vector2.Dot(q-P[i],v)/v.sqrMagnitude);var c=P[i]+v*t;float d=(q-c).sqrMagnitude;if(d<best){best=d;bu=U[i]+t*v.magnitude;var dir=v.normalized;o=Vector2.Dot(q-c,new Vector2(dir.y,-dir.x));}}return bu;}
  Vector2 At(float u,out Vector2 right){int i=0;while(i+2<P.Count&&U[i+1]<u)i++;var v=P[i+1]-P[i];var dir=v.normalized;right=new Vector2(dir.y,-dir.x);return P[i]+dir*(u-U[i]);}
  Vector2 XZ(Vector3 w)=>new(w.x,w.z);
  float ua=Proj(XZ(road.At(a,out _)),out _),ub=Proj(XZ(road.At(b,out _)),out _),sa=ua-2.5f,sb=ub+2.5f;int ns=(int)((sb-sa)/.25f)+1,no=41;var Y=new float[ns,no];float O(int j)=>-10+j*.5f;
  float guess=road.At(a-2,out _).y+3;
  for(int i=0;i<ns;i++){var c=At(sa+i*.25f,out var r);for(int j=0;j<no;j++){var q=c+r*O(j);Y[i,j]=Surface(q.x,q.y,guess,IsDrive,out var h,7)?h.point.y:float.NaN;}
   if(!float.IsNaN(Y[i,no/2]))guess=Y[i,no/2]+3;
   for(int j=no/2;j<no;j++)if(float.IsNaN(Y[i,j])&&j>0)Y[i,j]=Y[i,j-1];for(int j=no/2;j>=0;j--)if(float.IsNaN(Y[i,j])&&j<no-1)Y[i,j]=Y[i,j+1];}
  float Orig(float u,float o){float fi=Mathf.Clamp((u-sa)/.25f,0,ns-1.001f),fj=Mathf.Clamp((o+10)/.5f,0,no-1.001f);int i=(int)fi,j=(int)fj;float t=fi-i,w=fj-j;return Mathf.Lerp(Mathf.Lerp(Y[i,j],Y[i+1,j],t),Mathf.Lerp(Y[i,j+1],Y[i+1,j+1],t),w);}
  float Target(float u,float o){float ya=Orig(ua,o),yb=Orig(ub,o),ma=(ya-Orig(ua-2,o))/2,mb=(Orig(ub+2,o)-yb)/2,t=(u-ua)/(ub-ua),L=ub-ua;float h00=2*t*t*t-3*t*t+1,h10=t*t*t-2*t*t+t,h01=-2*t*t*t+3*t*t,h11=t*t*t-t*t;return h00*ya+h10*L*ma+h01*yb+h11*L*mb;}
  float Delta(float u,float o){if(u<=ua||u>=ub)return 0;float d=Target(u,o)-Orig(u,o);return float.IsNaN(d)?0:d;}
  var c0=At(ua,out _);var c1=At(ub,out _);var box=new Bounds(new Vector3(c0.x,Orig(ua,0),c0.y),Vector3.zero);box.Encapsulate(new Vector3(c1.x,Orig(ub,0),c1.y));box.Expand(new Vector3(32,12,32));
  float maxD=0;for(float u=ua;u<=ub;u+=.25f)for(float o=-8;o<=8;o+=1)maxD=Mathf.Max(maxD,Mathf.Abs(Delta(u,o)));int moved=0,meshes=0;
  foreach(var mf in Object.FindObjectsByType<MeshFilter>().Where(m=>m.sharedMesh&&m.GetComponent<MeshCollider>()&&IsTerrain(m.GetComponent<MeshCollider>())).ToArray()){if(!mf.GetComponent<Renderer>().bounds.Intersects(box))continue;
   var m=mf.sharedMesh;var v=m.vertices;bool any=false;bool drive=IsDrive(mf.GetComponent<Collider>());
   for(int i=0;i<v.Length;i++){var w=mf.transform.TransformPoint(v[i]);if(!box.Contains(w))continue;float u=Proj(XZ(w),out float o);if(u<=ua||u>=ub||Mathf.Abs(o)>(drive?7.5f:12))continue;if(branches.Any(br=>{br.Project(w,out float bl);return bl<br.halfWidth+3;}))continue;float oc=Mathf.Clamp(o,-10,10),orig=Orig(u,oc);if(float.IsNaN(orig)||Mathf.Abs(w.y-orig)>4)continue;
    float target=Target(u,oc),ny;if(float.IsNaN(target))continue;
    if(drive)ny=Mathf.Abs(w.y-orig)<.3f?target:w.y+Delta(u,oc);else{ny=w.y+Delta(u,oc)*Mathf.Clamp01((12-Mathf.Abs(o))/4f);if(Surface(w.x,w.z,ny+3,IsDrive,out var ph,6)&&Mathf.Abs(ph.point.y-target)<1.5f)ny=Mathf.Min(ny,target-.15f);}
    if(Mathf.Abs(ny-w.y)<.002f)continue;w.y=ny;v[i]=mf.transform.InverseTransformPoint(w);moved++;any=true;}
   if(any){m.vertices=v;m.RecalculateNormals();m.RecalculateBounds();Refresh(mf);meshes++;}}
  Note($"{label}: grade s {a}-{b} (horizontal {ub-ua:F1} m) per-offset Hermite (centre y {Orig(ua,0):F2}->{Orig(ub,0):F2}); max change {maxD:F2} m; {moved} vertices in {meshes} meshes");}
 // BUG-006: legacy teal arrows hover above the regraded pavement; duplicates of Route Atlas arrows are hidden,
 // every other arrow on the stretch is draped onto the final pavement.
 static void SeatArrows(RaceRoad road,float a,float b,string label){var atlasRoot=GameObject.Find("Route atlas direction guidance");var atlas=atlasRoot?atlasRoot.GetComponentsInChildren<MeshRenderer>():new MeshRenderer[0];
  bool OnStretch(Renderer r){float s=road.ProjectNear(r.bounds.center,(a+b)/2,(b-a)/2+5,out float lat);return s>=a&&s<=b&&lat<12;}
  foreach(var r in Object.FindObjectsByType<MeshRenderer>().Where(r=>!r.GetComponent<Collider>()&&!r.GetComponent<TextMesh>()&&(r.name.Contains("arrow")||r.name.Contains("handoff")||r.name.Contains("continuation")||r.name.StartsWith("Main teal")||r.name.StartsWith("Optional gold"))&&OnStretch(r)).ToArray()){
   bool isAtlas=atlasRoot&&r.transform.IsChildOf(atlasRoot.transform);
   if(!isAtlas&&atlas.Any(x=>Overlap(x.bounds,r.bounds))){r.gameObject.SetActive(false);Note($"{label}: hid legacy duplicate '{r.name}' at {r.bounds.center} (overlaps Route Atlas arrow)");continue;}
   Drape(r,isAtlas?.075f:.07f,label);}}
 static bool Overlap(Bounds x,Bounds y)=>x.min.x<y.max.x+.3f&&x.max.x>y.min.x-.3f&&x.min.z<y.max.z+.3f&&x.max.z>y.min.z-.3f;
 static void Drape(Renderer r,float lift,string label){var mf=r.GetComponent<MeshFilter>();var v=mf.sharedMesh.vertices;float before=0,moved=0;var nv=new Vector3[v.Length];
  for(int i=0;i<v.Length;i++){var w=mf.transform.TransformPoint(v[i]);if(!Surface(w.x,w.z,w.y+4,c=>IsDrive(c)||IsTerrain(c),out var h,12)){Note($"{label}: '{r.name}' at {r.bounds.center} unsupported vertex; left as is");return;}before=Mathf.Max(before,Mathf.Abs(w.y-h.point.y-lift));w.y=h.point.y+lift;nv[i]=mf.transform.InverseTransformPoint(w);}
  if(before<.05f)return;var m=Object.Instantiate(mf.sharedMesh);m.vertices=nv;m.RecalculateBounds();m.RecalculateNormals();mf.sharedMesh=Store($"draped-arrow-{Mathf.RoundToInt(r.bounds.center.x)}-{Mathf.RoundToInt(r.bounds.center.z)}",m);
  Note($"{label}: draped '{r.name}' at {r.bounds.center} onto pavement (max vertex correction {before:F2} m)");}

 // BUG-009/010: the 0.64 regeneration left no ground under the South Face receiving deck. Fill the slot up to
 // just under the deck, around a tunnel for the lower main route (floor flush with the pavement, ceiling 6.8 m).
 static void SouthFaceFill(RaceRoad road){
  const float x0=944,x1=1024,z0=-216,z1=-78,step=1;int nx=(int)((x1-x0)/step)+1,nz=(int)((z1-z0)/step)+1;
  foreach(var old in Object.FindObjectsByType<MeshFilter>().Where(m=>m.name.StartsWith("Ground_Report067 South Face")).ToArray())Object.DestroyImmediate(old.gameObject);Physics.SyncTransforms();
  var mat=GroundMaterial();var H=new float[nx,nz];var kind=new int[nx,nz];// 0 outside,1 boundary,2 free,3 floor,4 deck
  var deck=new float[nx,nz];var floorY=new float[nx,nz];var lowLat=new float[nx,nz];var ceilingY=new float[nx,nz];
  // Lower main route stations and real pavement half widths.
  float lowS0=1660,lowS1=1850;var pave=new Dictionary<int,(float y,float w)>();
  for(float s=lowS0;s<=lowS1;s+=1){var pc=road.At(s,out var f);var right=Vector3.Cross(Vector3.up,Vector3.ProjectOnPlane(f,Vector3.up).normalized);if(!Surface(pc.x,pc.z,pc.y+2.5f,IsDrive,out var h,5))continue;float w=0;for(;w<6.5f;w+=.25f){var q=pc+right*(w+.25f);var ql=pc-right*(w+.25f);if(!Surface(q.x,q.z,h.point.y+1.5f,IsDrive,out var hq,3)||!Surface(ql.x,ql.z,h.point.y+1.5f,IsDrive,out var hl,3))break;}pave[(int)s]=(h.point.y,Mathf.Max(w,3.6f));}
  Note($"BUG-009/010: lower-route pavement half-width min {pave.Values.Min(x=>x.w):F1} max {pave.Values.Max(x=>x.w):F1} mean {pave.Values.Average(x=>x.w):F1}");
  bool Sheet(Collider c)=>IsDrive(c)||c.name.Contains("receiving bank");
  for(int i=0;i<nx;i++)for(int j=0;j<nz;j++){float x=x0+i*step,z=z0+j*step;var col=Column(x,z);var drv=col.Where(h=>IsDrive(h.collider)).Select(h=>h.point.y).ToList();
   var ground=col.Where(h=>h.normal.y>0&&IsTerrain(h.collider)&&!Sheet(h.collider)&&!(h.collider.name.Contains("continuous solid 2")&&drv.Any(d=>d-h.point.y>=0&&d-h.point.y<=.6f))).Select(h=>h.point.y).ToList();
   float s=road.ProjectNear(new Vector3(x,130,z),1755,95,out _);var rp0=road.At(s,out _);s=road.ProjectNear(new Vector3(x,rp0.y,z),s,25,out _);rp0=road.At(s,out _);float lat=Vector2.Distance(new(x,z),new(rp0.x,rp0.z));var key=Mathf.Clamp((int)s,(int)lowS0,(int)lowS1);bool hasLow=pave.TryGetValue(key,out var lp);if(!hasLow||s<lowS0||s>lowS1)lat=999;lowLat[i,j]=lat;floorY[i,j]=hasLow?lp.y-.12f:float.NaN;
   deck[i,j]=drv.Where(d=>!hasLow||lat>10||d>lp.y+4).DefaultIfEmpty(float.NaN).Max();
   bool onPave=hasLow&&lat<16&&drv.Any(d=>Mathf.Abs(d-lp.y)<1.5f);
   if(ground.Count>0&&!onPave){kind[i,j]=1;H[i,j]=ground.Max()-.15f;continue;}
   bool hasDeck=!float.IsNaN(deck[i,j]);if(!hasDeck&&lat>12){kind[i,j]=0;continue;}
   float w=hasLow?lp.w:3.6f;
   var here=hasLow&&lat<16?drv.Where(d=>Mathf.Abs(d-lp.y)<1.5f).ToList():new List<float>();
   if(lat<=w+1.5f||here.Count>0){kind[i,j]=3;
    if(here.Count>0)H[i,j]=here.Max()-.15f;else{float edge=float.NaN;for(float r=.5f;r<=2.5f&&float.IsNaN(edge);r+=.5f)foreach(var dir in new[]{Vector3.forward,Vector3.back,Vector3.left,Vector3.right})if(Surface(x+dir.x*r,z+dir.z*r,lp.y+1.5f,IsDrive,out var eh,3)){edge=float.IsNaN(edge)?eh.point.y:Mathf.Min(edge,eh.point.y);}
     H[i,j]=float.IsNaN(edge)?floorY[i,j]-.3f:edge-.08f;}}
   else if(hasDeck&&lat>=w+3.5f){kind[i,j]=4;H[i,j]=deck[i,j]-.35f;}
   else kind[i,j]=2;
   ceilingY[i,j]=hasDeck&&(lat<=w+3f||here.Count>0)&&hasLow&&deck[i,j]-floorY[i,j]>=8.5f?floorY[i,j]+6.8f:float.NaN;}
  // Boundary cells only matter next to the filled region.
  bool Fill(int i,int j)=>i>=0&&j>=0&&i<nx&&j<nz&&kind[i,j]>=2;
  for(int i=0;i<nx;i++)for(int j=0;j<nz;j++)if(kind[i,j]==1&&!(Fill(i-1,j)||Fill(i+1,j)||Fill(i,j-1)||Fill(i,j+1)||Fill(i-1,j-1)||Fill(i+1,j+1)||Fill(i-1,j+1)||Fill(i+1,j-1)))kind[i,j]=0;
  int free=0;double sum=0;int fixedCount=0;for(int i=0;i<nx;i++)for(int j=0;j<nz;j++){if(kind[i,j]==2)free++;else if(kind[i,j]>0){sum+=H[i,j];fixedCount++;}}
  float mean=(float)(sum/Math.Max(1,fixedCount));for(int i=0;i<nx;i++)for(int j=0;j<nz;j++)if(kind[i,j]==2)H[i,j]=mean;
  for(int it=0;it<4000;it++)for(int i=0;i<nx;i++)for(int j=0;j<nz;j++){if(kind[i,j]!=2)continue;float acc=0;int n=0;foreach(var (di,dj) in new[]{(1,0),(-1,0),(0,1),(0,-1)}){int a=i+di,b=j+dj;if(a<0||b<0||a>=nx||b>=nz||kind[a,b]==0)continue;acc+=H[a,b];n++;}if(n>0)H[i,j]=acc/n;}
  for(int i=0;i<nx;i++)for(int j=0;j<nz;j++)if(kind[i,j]>=2&&!float.IsNaN(deck[i,j]))H[i,j]=Mathf.Min(H[i,j],deck[i,j]-.35f);
  // Surface mesh (cells whose four nodes are in the fill or its boundary ring, with at least one fill node).
  var v=new List<Vector3>();var c=new List<Color>();var t=new List<int>();Vector3 P(int i,int j)=>new(x0+i*step,H[i,j],z0+j*step);
  for(int i=0;i+1<nx;i++)for(int j=0;j+1<nz;j++){if(kind[i,j]==0||kind[i+1,j]==0||kind[i,j+1]==0||kind[i+1,j+1]==0)continue;if(!(Fill(i,j)||Fill(i+1,j)||Fill(i,j+1)||Fill(i+1,j+1)))continue;
   Tri(v,c,t,P(i,j),P(i,j+1),P(i+1,j+1),Vector3.up);Tri(v,c,t,P(i,j),P(i+1,j+1),P(i+1,j),Vector3.up);}
  int cleared=0;for(int k=0;k<v.Count;k++){var q=v[k];if(Surface(q.x,q.z,q.y+.1f,IsDrive,out var ph,14)&&q.y-ph.point.y>-.1f&&q.y-ph.point.y<6.5f){q.y=ph.point.y-.15f;v[k]=q;cleared++;continue;}
   // Edge vertices beside the lower route only: never above the adjacent pavement edge (within 1 m).
   int ni=Mathf.Clamp(Mathf.RoundToInt((q.x-x0)/step),0,nx-1),nj=Mathf.Clamp(Mathf.RoundToInt((q.z-z0)/step),0,nz-1);if(lowLat[ni,nj]>pave.Values.Max(x=>x.w)+3)continue;
   float edge=float.NaN;foreach(var dir in new[]{Vector3.forward,Vector3.back,Vector3.left,Vector3.right})for(float r=.25f;r<=1f;r+=.25f)if(Surface(q.x+dir.x*r,q.z+dir.z*r,q.y+.1f,IsDrive,out var eh,6.5f)&&q.y-eh.point.y<6.5f&&Mathf.Abs(eh.point.y-floorY[ni,nj])<2f)edge=float.IsNaN(edge)?eh.point.y:Mathf.Min(edge,eh.point.y);
   if(!float.IsNaN(edge)&&q.y>edge-.05f){q.y=edge-.05f;v[k]=q;cleared++;}}
  var hill=Make("South Face hillside",Store("south-face-hillside",Build(v,c,t)),mat);
  // Tunnel ceiling (faces down) and portal headwalls up to the deck where the ceiling ends.
  v=new();c=new();t=new();bool Ceil(int i,int j)=>i>=0&&j>=0&&i<nx&&j<nz&&!float.IsNaN(ceilingY[i,j]);Vector3 C(int i,int j)=>new(x0+i*step,ceilingY[i,j],z0+j*step);int portal=0;
  for(int i=0;i+1<nx;i++)for(int j=0;j+1<nz;j++){if(!(Ceil(i,j)&&Ceil(i+1,j)&&Ceil(i,j+1)&&Ceil(i+1,j+1)))continue;Tri(v,c,t,C(i,j),C(i,j+1),C(i+1,j+1),Vector3.down);Tri(v,c,t,C(i,j),C(i+1,j+1),C(i+1,j),Vector3.down);
   foreach(var (ai,aj,bi,bj,oi,oj) in new[]{(i,j,i,j+1,-1,0),(i+1,j,i+1,j+1,1,0),(i,j,i+1,j,0,-1),(i,j+1,i+1,j+1,0,1)}){int ni=i+(oi>0?1:oi),nj=j+(oj>0?1:oj);if(ni<0||nj<0||ni+1>=nx||nj+1>=nz)continue;if(Ceil(ni,nj)&&Ceil(ni+1,nj)&&Ceil(ni,nj+1)&&Ceil(ni+1,nj+1))continue;if(lowLat[ni,nj]>pave.Values.Max(x=>x.w)+1.5f&&lowLat[ni+1,nj+1]>pave.Values.Max(x=>x.w)+1.5f)continue;
    float ta=deck[ai,aj]-.35f,tb=deck[bi,bj]-.35f;if(float.IsNaN(ta)||float.IsNaN(tb))continue;var A=C(ai,aj);var B=C(bi,bj);var out_=new Vector3(oi,0,oj);Tri(v,c,t,A,B,new Vector3(B.x,tb,B.z),out_);Tri(v,c,t,A,new Vector3(B.x,tb,B.z),new Vector3(A.x,ta,A.z),out_);portal++;}}
  for(int k=0;k<v.Count;k++){var q=v[k];if(Surface(q.x,q.z,q.y+.1f,IsDrive,out var ph,14)&&q.y-ph.point.y<6.5f){q.y=ph.point.y+6.6f;v[k]=q;cleared++;}}
  GameObject ceiling=null;if(t.Count>0)ceiling=Make("South Face tunnel ceiling",Store("south-face-tunnel-ceiling",Build(v,c,t)),mat);
  Physics.SyncTransforms();
  // Old curtain/sheet faces inside the tunnel/cut clearance are removed (0.66 method, same volume rules).
  int cut=0;foreach(var mf in Object.FindObjectsByType<MeshFilter>().Where(m=>m.name.Contains("receiving bank")||m.name.Contains("continuous solid 2")).ToArray()){var m=mf.sharedMesh;var mv=m.vertices;var tri=m.triangles;var keep=new List<int>();
   for(int k=0;k<tri.Length;k+=3){var ce=mf.transform.TransformPoint((mv[tri[k]]+mv[tri[k+1]]+mv[tri[k+2]])/3);bool inside=false;if(ce.x>x0&&ce.x<x1&&ce.z>z0&&ce.z<z1){float s=road.ProjectNear(ce,1755,95,out float lat);if(s>=lowS0&&s<=lowS1&&pave.TryGetValue((int)s,out var lp)){float rel=ce.y-lp.y;inside=lat<=lp.w+3f&&rel>.2f&&rel<7.2f;}}if(inside){cut++;continue;}keep.AddRange(new[]{tri[k],tri[k+1],tri[k+2]});}
   if(keep.Count!=tri.Length){m.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;m.SetTriangles(keep,0);m.RecalculateNormals();m.RecalculateBounds();Refresh(mf);}}
  // Complete trees whose trunks hung in the empty slot are removed; none can stand inside the new mass.
  bool InFill(Vector3 p){int i=Mathf.RoundToInt((p.x-x0)/step),j=Mathf.RoundToInt((p.z-z0)/step);return i>=0&&j>=0&&i<nx&&j<nz&&kind[i,j]>=2;}
  int trees=TreesHelper(c0=>InFill(c0.bounds.center),c0=>false);
  int nFill=0,nFloor=0,nDeck=0;for(int i=0;i<nx;i++)for(int j=0;j<nz;j++){if(kind[i,j]>=2)nFill++;if(kind[i,j]==3)nFloor++;if(kind[i,j]==4)nDeck++;}
  Note($"BUG-009/010: South Face hillside fill nodes={nFill} (floor {nFloor}, under-deck {nDeck}, Laplace {free}); surface tris={hill.GetComponent<MeshFilter>().sharedMesh.triangles.Length/3}; tunnel ceiling tris={(ceiling?ceiling.GetComponent<MeshFilter>().sharedMesh.triangles.Length/3:0)} portal quads={portal}; old curtain/sheet tris removed from tunnel clearance={cut}; hanging trees removed={trees}; clearance-corrected vertices={cleared}; lower-route pavement stations sampled={pave.Count}");
  File.WriteAllLines("Docs/Report067/south-face-fill-map.txt",Enumerable.Range(0,nz).Reverse().Where(j=>j%3==0).Select(j=>$"{z0+j*step,6} "+string.Concat(Enumerable.Range(0,nx).Where(i=>i%2==0).Select(i=>".BFTD"[kind[i,j]]))));}
 static int TreesHelper(Func<Collider,bool> remove,Func<Collider,bool> seat){int before=Object.FindObjectsByType<Collider>().Count(c=>c.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0&&remove(c));
  if(before==0&&!Object.FindObjectsByType<Collider>().Any(c=>c.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0&&seat(c)))return 0;
  typeof(Racer.Editor.BackyardReverseAuthoring).GetMethod("Trees",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{remove,seat});return before;}

 // ---------- MountainLoop (Forward) ----------
 static void Forward(){LayerDedupe(new Vector3(731.4f,79.7f,-26.6f),17,"BUG-018");}
 // BUG-018: up to four coincident ground sheets z-fight at the lakeside junction. Remove a sheet triangle only
 // where another ground surface lies 0..0.6 m above all of its sample points (no holes; topmost surface kept).
 static void LayerDedupe(Vector3 at,float radius,string label){var road=Race.road;var meshes=Object.FindObjectsByType<MeshFilter>().Where(m=>m.sharedMesh&&m.GetComponent<MeshCollider>()&&IsTerrain(m.GetComponent<MeshCollider>())&&!IsDrive(m.GetComponent<Collider>())&&m.GetComponent<Renderer>().bounds.Intersects(new Bounds(at,new Vector3(radius*2,30,radius*2)))).ToArray();
  var plan=new Dictionary<MeshFilter,HashSet<int>>();int total=0;
  var rank=meshes.Select(m=>m.GetComponent<Collider>()).OrderBy(x=>x.name).ToList();
  // Covered: another ground surface 0..0.6 m above p. Exactly coincident sheets (within 4 mm) of another mesh
  // count only if that mesh ranks higher, so one of a coincident pair always survives (no holes).
  bool Covered(Vector3 p,Collider self){var o=p+Vector3.up*.6f;float depth=.61f;for(int k=0;k<10&&depth>0;k++){if(!Physics.Raycast(o,Vector3.down,out var h,depth,~0,QueryTriggerInteraction.Ignore))return false;
    if(h.normal.y>0&&IsTerrain(h.collider)&&!IsDrive(h.collider)){float dy=h.point.y-p.y;if(dy>.004f)return true;if(h.collider!=self&&rank.IndexOf(h.collider)>rank.IndexOf(self))return true;}
    depth-=h.distance+.002f;o=h.point+Vector3.down*.002f;}return false;}
  foreach(var mf in meshes){var m=mf.sharedMesh;var v=m.vertices;var tri=m.triangles;var col=mf.GetComponent<Collider>();var gone=new HashSet<int>();
   for(int k=0;k<tri.Length;k+=3){var a=mf.transform.TransformPoint(v[tri[k]]);var b=mf.transform.TransformPoint(v[tri[k+1]]);var d=mf.transform.TransformPoint(v[tri[k+2]]);var ce=(a+b+d)/3;if(Vector2.Distance(new(ce.x,ce.z),new(at.x,at.z))>radius||Mathf.Abs(ce.y-at.y)>6)continue;
    if(Surface(ce.x,ce.z,ce.y+1.5f,IsDrive,out _,3))continue;var samples=new[]{ce,Vector3.Lerp(a,ce,.2f),Vector3.Lerp(b,ce,.2f),Vector3.Lerp(d,ce,.2f)};if(samples.All(p=>Covered(p,col))){gone.Add(k);}}
   if(gone.Count>0){plan[mf]=gone;total+=gone.Count;}}
  foreach(var kv in plan){var m=kv.Key.sharedMesh;var tri=m.triangles;var keep=new List<int>();for(int k=0;k<tri.Length;k+=3)if(!kv.Value.Contains(k))keep.AddRange(new[]{tri[k],tri[k+1],tri[k+2]});m.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;m.SetTriangles(keep,0);m.RecalculateBounds();Refresh(kv.Key);Note($"{label}: {kv.Key.name}: removed {kv.Value.Count} buried duplicate triangles");}
  Note($"{label}: layer dedupe radius {radius} m at {at}: {total} duplicate triangles removed; pavement, gates and road lines untouched");}

 // ---------- DansBackyardForward ----------
 static void BackyardForward(){var race=Race;var road=race.road;road.Initialize();
  // BUG-019: a Street Loop Route Atlas arrow inherited by this scene points along Street Loop's direction.
  foreach(var r in Object.FindObjectsByType<MeshRenderer>().Where(r=>r.TryGetComponent<MeshFilter>(out var rf)&&rf.sharedMesh&&rf.sharedMesh.name.StartsWith("StreetLoopGreybox-arrow")).ToArray()){
   float d=Vector3.Distance(r.bounds.center,new Vector3(472.8f,82.4f,-5.8f));if(d<8){Note($"BUG-019: removed inherited Street Loop arrow '{r.name}' ({r.GetComponent<MeshFilter>().sharedMesh.name}) at {r.bounds.center}");Object.DestroyImmediate(r.gameObject);}
   else Note($"BUG-019 info: other inherited Street Loop arrow '{r.name}' at {r.bounds.center} left unchanged (not reported)");}
  // BUG-020: route points on the straight raised run-up (lip s~762) were a curve 74..95 deg; arrow and reset
  // heading both come from them. Align s 736..762 to the run-up/landing axis and blend s 718..736.
  float L0=road.Length;var pts=road.points.ToArray();float[] dist=new float[pts.Length];for(int i=1;i<pts.Length;i++)dist[i]=dist[i-1]+Vector3.Distance(pts[i-1],pts[i]);
  int I(float s)=>Array.FindIndex(dist,x=>x>=s);int ia=I(718),ib=I(736),ic=I(762);float axisZ=pts[ic].z;var A=pts[ia];var B=new Vector3(pts[ib].x,pts[ib].y,axisZ);
  var ta=(pts[ia+1]-pts[ia-1]);ta.y=0;ta=ta.normalized*Vector3.Distance(A,B);var tb=Vector3.right*Vector3.Distance(A,B);
  for(int i=ib;i<=ic;i++)pts[i].z=axisZ;
  for(int i=ia+1;i<ib;i++){float t=(dist[i]-dist[ia])/(dist[ib]-dist[ia]);float h00=2*t*t*t-3*t*t+1,h10=t*t*t-2*t*t+t,h01=-2*t*t*t+3*t*t,h11=t*t*t-t*t;var p=h00*A+h10*ta+h01*B+h11*tb;pts[i].x=p.x;pts[i].z=p.z;}
  var old=road.points;road.points=pts;typeof(RaceRoad).GetField("distance",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(road,null);road.Initialize();float dL=road.Length-L0;
  Note($"BUG-020: route points {ia}..{ic} (s {dist[ia]:F1}..{dist[ic]:F1}) aligned to run-up axis z={axisZ:F2}; max lateral change {Enumerable.Range(ia,ic-ia+1).Max(i=>Vector3.Distance(old[i],pts[i])):F2} m; length change {dL:F3} m");
  if(Mathf.Abs(dL)>.005f){var layout=Object.FindAnyObjectByType<ForestLayout>();if(layout){for(int i=0;i<layout.jumpStarts.Length;i++){if(layout.jumpStarts[i]>dist[ic])layout.jumpStarts[i]+=dL;if(layout.jumpEnds[i]>dist[ic])layout.jumpEnds[i]+=dL;}EditorUtility.SetDirty(layout);}
   foreach(var b in Object.FindObjectsByType<WoodlandRoute>(FindObjectsInactive.Include)){if(b.entryRoad>dist[ic])b.entryRoad+=dL;if(b.exitRoad>dist[ic])b.exitRoad+=dL;EditorUtility.SetDirty(b);}
   if(road.bypassStart>dist[ic])road.bypassStart+=dL;if(road.bypassEnd>dist[ic])road.bypassEnd+=dL;Note($"BUG-020: downstream stations shifted by {dL:F3} m");}
  EditorUtility.SetDirty(road);
  var arrow=Object.FindObjectsByType<MeshRenderer>().Where(r=>r.name=="Main teal trail arrow"&&Vector3.Distance(r.transform.position,new Vector3(70.54f,45.10f,61.06f))<1.5f).FirstOrDefault();
  if(arrow){float s=road.Project(arrow.transform.position,out _);var p=road.At(s,out var f);float before=arrow.transform.eulerAngles.y;arrow.transform.position=new Vector3(p.x,arrow.transform.position.y,p.z);arrow.transform.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(f,Vector3.up));Physics.SyncTransforms();Drape(arrow,.06f,"BUG-020");
   Note($"BUG-020: arrow at s {s:F1} heading {before:F1} -> {arrow.transform.eulerAngles.y:F1} (route tangent)");}
  for(float s=730;s<=762;s+=4){road.At(s,out var f);Note($"BUG-020 route heading s {s}: {Mathf.Atan2(f.x,f.z)*Mathf.Rad2Deg:F1}");}}
}
