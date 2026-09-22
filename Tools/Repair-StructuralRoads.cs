using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Racer;
using Object=UnityEngine.Object;

// Explicit editor authoring, never runs in a player. All coordinates are world space.
public static class StructuralRoads {
 const string Folder="Assets/Track/StructuralRoads";
 sealed class Strip {public string name; public Vector3[] p; public float[] w; public bool bank=true; public Strip(string n,Vector3[] a,float width){name=n;p=a;w=Enumerable.Repeat(width,a.Length).ToArray();}}
 sealed class Tri {public Vector3 a,b,c; public Color ca=Color.white,cb=Color.white,cc=Color.white; public Tri(Vector3 x,Vector3 y,Vector3 z){a=x;b=y;c=z;} public Vector3 Center=>(a+b+c)/3;}
 static Color ColorAt(Tri t,Vector3 p){float d=Cross(t.b-t.a,t.c-t.a);if(Math.Abs(d)<.00001f)return t.ca;float u=Cross(p-t.a,t.c-t.a)/d,v=Cross(t.b-t.a,p-t.a)/d;return t.ca*(1-u-v)+t.cb*u+t.cc*v;}
 static float Cross(Vector3 a,Vector3 b)=>a.x*b.z-a.z*b.x;
 static float Flat(Vector3 a,Vector3 b)=>Vector2.Distance(new(a.x,a.z),new(b.x,b.z));
 static float Smooth(float x)=>Mathf.SmoothStep(0,1,Mathf.Clamp01(x));
 static Vector3 Side(Vector3 f)=>Vector3.Cross(Vector3.up,f).normalized;
 static float Height(Tri t,Vector3 p){float d=Cross(t.b-t.a,t.c-t.a);return t.a.y+(Cross(p-t.a,t.c-t.a)*(t.b.y-t.a.y)+Cross(t.b-t.a,p-t.a)*(t.c.y-t.a.y))/d;}
 static Vector3 Closest(Vector3 p,Vector3 a,Vector3 b){var v=b-a;v.y=0;var q=p-a;q.y=0;return Vector3.Lerp(a,b,Mathf.Clamp01(Vector3.Dot(q,v)/Mathf.Max(.00001f,v.sqrMagnitude)));}
 static float Distance(Tri t,Vector3 p){var ab=Cross(t.b-t.a,p-t.a);var bc=Cross(t.c-t.b,p-t.b);var ca=Cross(t.a-t.c,p-t.c);if((ab>=0&&bc>=0&&ca>=0)||(ab<=0&&bc<=0&&ca<=0))return 0;return Mathf.Min(Flat(p,Closest(p,t.a,t.b)),Flat(p,Closest(p,t.b,t.c)),Flat(p,Closest(p,t.c,t.a)));}
 sealed class Surface {
  public List<Tri> triangles=new(); Dictionary<Vector2Int,List<Tri>> bins=new();
  public IEnumerable<Tri> Near(Vector3 p,float radius=0){var found=new HashSet<Tri>();for(int x=Mathf.FloorToInt((p.x-radius)/16);x<=Mathf.FloorToInt((p.x+radius)/16);x++)for(int z=Mathf.FloorToInt((p.z-radius)/16);z<=Mathf.FloorToInt((p.z+radius)/16);z++)if(bins.TryGetValue(new(x,z),out var ts))foreach(var t in ts)if(found.Add(t))yield return t;}
  public void Add(Tri t){if(Math.Abs(Cross(t.b-t.a,t.c-t.a))<.000001f)return;triangles.Add(t);for(int x=Mathf.FloorToInt(Mathf.Min(t.a.x,t.b.x,t.c.x)/16);x<=Mathf.FloorToInt(Mathf.Max(t.a.x,t.b.x,t.c.x)/16);x++)for(int z=Mathf.FloorToInt(Mathf.Min(t.a.z,t.b.z,t.c.z)/16);z<=Mathf.FloorToInt(Mathf.Max(t.a.z,t.b.z,t.c.z)/16);z++){var k=new Vector2Int(x,z);if(!bins.TryGetValue(k,out var list))bins[k]=list=new();list.Add(t);}}
  public Tri NearestLevel(Vector3 p,float radius,out float distance){Tri best=null;distance=float.MaxValue;float score=float.MaxValue;foreach(var t in Near(p,radius)){float d=Distance(t,p);float next=d+Math.Abs(Height(t,p)-p.y)*2;if(d<=radius&&next<score){score=next;distance=d;best=t;}}return best;}
  public Tri Nearest(Vector3 p,float radius,out float distance){Tri best=null;distance=float.MaxValue;foreach(var t in Near(p,radius)){float d=Distance(t,p);if(d<distance){distance=d;best=t;}}return best;}
 }
 static List<Vector3> Clip(List<Vector3> poly,Vector3 a,Vector3 b,float sign,bool inside){var result=new List<Vector3>();for(int i=0;i<poly.Count;i++){var p=poly[i];var q=poly[(i+1)%poly.Count];float u=Cross(b-a,p-a)*sign,v=Cross(b-a,q-a)*sign;bool ip=inside?u>=0:u<=0,iq=inside?v>=0:v<=0;if(ip)result.Add(p);if(ip!=iq)result.Add(Vector3.Lerp(p,q,u/(u-v)));}return result;}
 // Subtract actual triangles, rather than a guessed road half-width. The new
 // boundary is seated on the retained triangle's plane, including bank grade.
 static float Area(List<Vector3> p){float a=0;for(int i=1;i+1<p.Count;i++)a+=Math.Abs(Cross(p[i]-p[0],p[i+1]-p[0]));return a*.5f;}
 static List<List<Vector3>> Subtract(List<Vector3> polygon,Tri t,bool seat){
  if(polygon.Max(p=>p.x)<=Mathf.Min(t.a.x,t.b.x,t.c.x)+.00001f||polygon.Min(p=>p.x)>=Mathf.Max(t.a.x,t.b.x,t.c.x)-.00001f||polygon.Max(p=>p.z)<=Mathf.Min(t.a.z,t.b.z,t.c.z)+.00001f||polygon.Min(p=>p.z)>=Mathf.Max(t.a.z,t.b.z,t.c.z)-.00001f)return new(){polygon};
  float sign=Mathf.Sign(Cross(t.b-t.a,t.c-t.a));var edges=new[]{t.a,t.b,t.c};var intersection=polygon;for(int i=0;i<3&&intersection.Count>2;i++)intersection=Clip(intersection,edges[i],edges[(i+1)%3],sign,true);
  if(Area(intersection)<.00001f)return new(){polygon};
  var outside=new List<List<Vector3>>();var remaining=polygon;for(int i=0;i<3&&remaining.Count>2;i++){var part=Clip(remaining,edges[i],edges[(i+1)%3],sign,false);if(Area(part)>.00001f)outside.Add(part);remaining=Clip(remaining,edges[i],edges[(i+1)%3],sign,true);}if(seat)foreach(var poly in outside)for(int i=0;i<poly.Count;i++)if(Distance(t,poly[i])<.001f){var p=poly[i];p.y=Height(t,p);poly[i]=p;}return outside;
 }
 static void Union(Surface target,IEnumerable<Tri> input,bool differentLevels=false,bool seat=true){foreach(var t in input){var pieces=new List<List<Vector3>>{new(){t.a,t.b,t.c}};float radius=Mathf.Max(Flat(t.Center,t.a),Flat(t.Center,t.b),Flat(t.Center,t.c));foreach(var old in target.Near(t.Center,radius).ToArray()){if(differentLevels&&Math.Abs(Height(old,t.Center)-t.Center.y)>2)continue;var next=new List<List<Vector3>>();foreach(var p in pieces)next.AddRange(Subtract(p,old,seat));pieces=next;if(pieces.Count==0)break;}foreach(var p in pieces)for(int i=1;i+1<p.Count;i++)target.Add(new(p[0],p[i],p[i+1]));}}
 static List<Tri> Ribbon(Strip s,Vector3? first=null,Vector3? last=null){var v=new List<Vector3>();for(int i=0;i<s.p.Length;i++){var f=i==0&&first.HasValue?first.Value:i==s.p.Length-1&&last.HasValue?last.Value:s.p[Math.Min(i+1,s.p.Length-1)]-s.p[Math.Max(0,i-1)];var side=Side(f)*s.w[i];v.Add(s.p[i]-side);v.Add(s.p[i]+side);}var t=new List<Tri>();for(int i=2;i<v.Count;i+=2){t.Add(new(v[i-2],v[i],v[i-1]));t.Add(new(v[i-1],v[i],v[i+1]));}return t;}
 static Strip Read(string name){var mf=GameObject.Find(name).GetComponent<MeshFilter>();var v=mf.sharedMesh.vertices;var s=new Strip(name,Enumerable.Range(0,v.Length/2).Select(i=>mf.transform.TransformPoint((v[2*i]+v[2*i+1])*.5f)).ToArray(),7);s.w=Enumerable.Range(0,v.Length/2).Select(i=>Flat(mf.transform.TransformPoint(v[2*i]),mf.transform.TransformPoint(v[2*i+1]))*.5f).ToArray();return s;}
 static Mesh Store(string name,IEnumerable<Tri> tris){
  var vertices=new List<Vector3>();var colors=new List<Color>();var indices=new List<int>();var weld=new Dictionary<(int,int,int),int>();
  foreach(var t in tris){
   if(Math.Abs(Cross(t.b-t.a,t.c-t.a))<.001f)continue;
   var points=Cross(t.b-t.a,t.c-t.a)<0?new[]{t.a,t.b,t.c}:new[]{t.a,t.c,t.b};var face=new int[3];
   for(int j=0;j<3;j++){var p=points[j];var k=(Mathf.RoundToInt(p.x*10000),Mathf.RoundToInt(p.y*10000),Mathf.RoundToInt(p.z*10000));if(!weld.TryGetValue(k,out int ix)){ix=vertices.Count;vertices.Add(p);colors.Add(ColorAt(t,p));weld[k]=ix;}face[j]=ix;}
   if(face.Distinct().Count()!=3||Math.Abs(Cross(vertices[face[1]]-vertices[face[0]],vertices[face[2]]-vertices[face[0]]))<.001f)continue;
   indices.AddRange(face);
  }
  var mesh=new Mesh{indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};mesh.SetVertices(vertices);mesh.SetColors(colors);mesh.SetTriangles(indices,0);mesh.RecalculateNormals();mesh.RecalculateBounds();string path=Folder+"/"+name+".asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(old){EditorUtility.CopySerialized(mesh,old);Object.DestroyImmediate(mesh);return old;}AssetDatabase.CreateAsset(mesh,path);return mesh;
 }
 static GameObject Make(Transform root,string name,IEnumerable<Tri> triangles,Material mat){var mesh=Store(root.gameObject.scene.name+"-"+name,triangles);var go=new GameObject("Ground_"+name,typeof(MeshFilter),typeof(MeshRenderer),typeof(MeshCollider));go.transform.SetParent(root);go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<MeshCollider>().sharedMesh=mesh;go.GetComponent<MeshRenderer>().sharedMaterial=mat;return go;}
 static Vector3[] Hermite(Vector3 a,Vector3 b,Vector3 fa,Vector3 fb){float l=Flat(a,b);int n=Mathf.CeilToInt(l/.5f);return Enumerable.Range(0,n+1).Select(i=>{float t=(float)i/n,t2=t*t,t3=t2*t;return (2*t3-3*t2+1)*a+(t3-2*t2+t)*fa*l+(-2*t3+3*t2)*b+(t3-t2)*fb*l;}).ToArray();}
 static void Refresh(RaceRoad road){typeof(RaceRoad).GetField("distance",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).SetValue(road,null);road.Initialize();EditorUtility.SetDirty(road);}
 static void Refresh(WoodlandRoute road){typeof(WoodlandRoute).GetField("lengths",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).SetValue(road,null);road.Initialize();EditorUtility.SetDirty(road);}
 static Collider[] ground;
 static float Ground(Vector3 p){float best=float.NegativeInfinity;foreach(var c in ground){if(p.x<c.bounds.min.x||p.x>c.bounds.max.x||p.z<c.bounds.min.z||p.z>c.bounds.max.z)continue;if(c.Raycast(new Ray(new(p.x,500,p.z),Vector3.down),out var hit,1000))best=Mathf.Max(best,hit.point.y);}return float.IsNegativeInfinity(best)?p.y-10:best;}
 static List<Tri> Banks(Strip s,MountainFlights.Flight[] flights){var tris=new List<Tri>();for(int side=-1;side<=1;side+=2){var rows=new List<Vector3[]>();for(int i=0;i<s.p.Length;i++){var p=s.p[i];var right=Side(s.p[Math.Min(i+1,s.p.Length-1)]-s.p[Math.Max(0,i-1)])*side;var edge=p+right*s.w[i];float span=Mathf.Clamp(Math.Abs(edge.y-Ground(edge))*1.5f+8,10,55);var row=new Vector3[9];for(int j=0;j<9;j++){float t=j/8f;var q=edge+right*span*t;float terrain=Ground(q);q.y=Mathf.Lerp(edge.y,terrain-.04f,Smooth(t));row[j]=q;}rows.Add(row);}
 for(int i=1;i<rows.Count;i++){var p=(s.p[i-1]+s.p[i])*.5f;bool deliberate=flights.Any(f=>{float a=Vector3.Dot(p-f.lip,f.forward);return a> -65&&a<170&&Math.Abs(Vector3.Dot(p-f.lip,Side(f.forward)))<32;});if(deliberate)continue;for(int j=1;j<9;j++){tris.Add(new(rows[i-1][j-1],rows[i][j-1],rows[i-1][j]));tris.Add(new(rows[i-1][j],rows[i][j-1],rows[i][j]));}}}return tris;}
 static void Init(){Directory.CreateDirectory("Docs/CR133-137");if(!AssetDatabase.IsValidFolder(Folder))AssetDatabase.CreateFolder("Assets/Track","StructuralRoads");}
 public static string MountainForward()=>Mountain(false);
 public static string MountainReverse()=>Mountain(true);
 public static string FinishForwardFork(){EditorSceneManager.OpenScene("Assets/Scenes/MountainLoop.unity");var race=Object.FindAnyObjectByType<RaceDirector>();var road=race.road;var branch=Object.FindObjectsByType<WoodlandRoute>().Single(b=>b.title=="Climbing Ridge Cut");float station=road.Project(branch.points[0],out _);var plane=new Tri(road.At(station-18,out _)+Vector3.up*.04f,road.At(station+25,out _)+Vector3.up*.04f,branch.At(25,out _)+Vector3.up*.04f);var center=branch.points[0];
  Vector3 Map(Vector3 p){float d=Flat(p,center);if(d>=65)return p;float y=Height(plane,p);if(Math.Abs(y-p.y)>12)return p;p.y=Mathf.Lerp(p.y,y,1-Smooth((d-25)/40));return p;}
  foreach(var mf in Object.FindObjectsByType<MeshFilter>().Where(m=>m.name.StartsWith("Ground_CR133")||m.name=="Main teal trail arrow"||m.name=="CR133 shortcut gold arrow")){var mesh=Object.Instantiate(mf.sharedMesh);mesh.vertices=mesh.vertices.Select(p=>mf.transform.InverseTransformPoint(Map(mf.transform.TransformPoint(p)))).ToArray();mesh.RecalculateNormals();mesh.RecalculateBounds();string path=Folder+"/MountainLoop-fork-"+mf.GetEntityId().ToString().Replace(':','-')+".asset";AssetDatabase.CreateAsset(mesh,path);mf.sharedMesh=mesh;if(mf.TryGetComponent<MeshCollider>(out var collider)){collider.sharedMesh=null;collider.sharedMesh=mesh;}}
  road.points=road.points.Select(p=>Map(p+Vector3.up*.04f)-Vector3.up*.04f).ToArray();Refresh(road);foreach(var b in Object.FindObjectsByType<WoodlandRoute>()){b.points=b.points.Select(p=>Map(p+Vector3.up*.04f)-Vector3.up*.04f).ToArray();Refresh(b);b.entryRoad=road.Project(b.points[0],out _);b.exitRoad=road.Project(b.points[^1],out _);}foreach(var f in race.GetComponent<MountainFlights>().flights){f.approachStation=road.Project(f.start,out _);f.endStation=road.Project(f.landingEnd,out _);}foreach(var gate in race.gates)gate.transform.position=Map(gate.transform.position-Vector3.up*1.6f)+Vector3.up*1.6f;
  var spawn=race.vehicle.GetComponent<VehicleRespawn>().spawnPoint;if(spawn)spawn.position=Map(spawn.position-Vector3.up*.7f)+Vector3.up*.7f;
  Physics.SyncTransforms();EditorSceneManager.MarkSceneDirty(race.gameObject.scene);EditorSceneManager.SaveScene(race.gameObject.scene);AssetDatabase.SaveAssets();File.WriteAllText("Docs/CR133-137/forward-fork-grade.txt",$"Unified junction plane through feeder {plane.a}, regular exit {plane.b}, shortcut exit {plane.c}; full-width core radius 25m, continuous fade to 65m. Navigation, gate and spawn x/z retained.");return "Rebuilt fork cross-section as one continuous grade across both route choices.";
 }
 public static string FinishReverseTransition(){EditorSceneManager.OpenScene("Assets/Scenes/MountainLoopReverse.unity");var branch=Object.FindObjectsByType<WoodlandRoute>().Single(b=>b.title=="Downhill Ridge Cut");float s=branch.Project(new Vector3(774,88,-176),out _);var a=branch.At(s-10,out _);var b=branch.At(s+10,out _);var center=(a+b)*.5f;var f=b-a;var flat=Vector3.ProjectOnPlane(f,Vector3.up);var gradient=flat*f.y/flat.sqrMagnitude;foreach(var mf in Object.FindObjectsByType<MeshFilter>().Where(m=>m.name.StartsWith("Ground_CR133"))){var mesh=Object.Instantiate(mf.sharedMesh);var v=mesh.vertices;for(int i=0;i<v.Length;i++){float d=Flat(v[i],center);if(d>=20||Math.Abs(v[i].y-center.y)>10)continue;float y=center.y+Vector3.Dot(v[i]-center,gradient);v[i].y=Mathf.Lerp(v[i].y,y,1-Smooth((d-6)/14));}mesh.vertices=v;mesh.RecalculateNormals();mesh.RecalculateBounds();EditorUtility.CopySerialized(mesh,mf.sharedMesh);Object.DestroyImmediate(mesh);var collider=mf.GetComponent<MeshCollider>();collider.sharedMesh=null;collider.sharedMesh=mf.sharedMesh;EditorUtility.SetDirty(mf.sharedMesh);}EditorSceneManager.MarkSceneDirty(branch.gameObject.scene);EditorSceneManager.SaveScene(branch.gameObject.scene);AssetDatabase.SaveAssets();return SeatReverseNavigation();}
 public static string SeatReverseNavigation(){EditorSceneManager.OpenScene("Assets/Scenes/MountainLoopReverse.unity");Physics.SyncTransforms();foreach(var b in Object.FindObjectsByType<WoodlandRoute>()){var points=new List<Vector3>();for(int i=1;i<b.points.Length;i++){var a=b.points[i-1];var z=b.points[i];int n=Vector3.Distance(a,z)>10?1:Math.Max(1,Mathf.CeilToInt(Flat(a,z)/.25f));for(int j=0;j<n;j++)points.Add(Vector3.Lerp(a,z,(float)j/n));}points.Add(b.points[^1]);for(int i=0;i<points.Count;i++){var p=points[i];var hits=Physics.RaycastAll(p+Vector3.up*4,Vector3.down,8,1,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name=="Ground_CR133 mountain driving surface").OrderBy(h=>Math.Abs(h.point.y-p.y)).ToArray();if(hits.Length>0)p.y=hits[0].point.y-.04f;points[i]=p;}b.points=points.ToArray();Refresh(b);}EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());return "Seated branch navigation on final physical surfaces at 0.25m intervals; flight gaps retained.";}
 public static string FinishForwardMouths(){EditorSceneManager.OpenScene("Assets/Scenes/MountainLoop.unity");GradeMouths();return "Full-width junction height fields applied to meshes, colliders and navigation.";}
 static void ClearCorridor(WoodlandRoute branch){var corridor=new Surface();foreach(var t in Ribbon(new Strip("clearance",branch.points,branch.halfWidth+2)))corridor.Add(t);Func<Vector3,bool> remove=p=>{var t=corridor.Nearest(p,0,out float d);return t!=null&&d<.01f&&Math.Abs(Height(t,p)-p.y)<20;};var flags=System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static;typeof(Racer.Editor.DiscoveryAuthoring).GetField("owner",flags).SetValue(null,Object.FindAnyObjectByType<RaceDirector>());typeof(Racer.Editor.DiscoveryAuthoring).GetMethod("ClearCompleteTrees",flags).Invoke(null,new object[]{remove});}
 public static string ClearForwardCorridor(){EditorSceneManager.OpenScene("Assets/Scenes/MountainLoop.unity");ClearCorridor(Object.FindObjectsByType<WoodlandRoute>().Single(b=>b.title=="Summit Traverse"));EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();return "Cleared complete trees intersecting the revised Summit Traverse only.";}
 static void GradeMouths(){
  var objects=Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include);
  var main=new Surface();foreach(var mf in objects.Where(m=>m.name.StartsWith("Ground_CR122 main section")&&m.sharedMesh)){var v=mf.sharedMesh.vertices;var ix=mf.sharedMesh.triangles;for(int i=0;i<ix.Length;i+=3)main.Add(new(mf.transform.TransformPoint(v[ix[i]]),mf.transform.TransformPoint(v[ix[i+1]]),mf.transform.TransformPoint(v[ix[i+2]])));}
  var branches=Object.FindObjectsByType<WoodlandRoute>();
  Vector3 Map(Vector3 p){if(main.Near(p).Any(t=>Distance(t,p)<.001f&&Math.Abs(Height(t,p)-p.y)<.06f))return p;foreach(var b in branches){float s=b.Project(p,out float lateral);float mouthLimit=b.title=="Downhill Ridge Cut"?55:100;if(lateral>b.halfWidth+2||Mathf.Min(s,b.Length-s)>mouthLimit)continue;var t=main.NearestLevel(p,32,out float d);if(t==null||d>32)continue;float y=Height(t,p);if(Math.Abs(y-p.y)>3)continue;float weight=(1-Smooth(d/32))*Smooth((mouthLimit-Mathf.Min(s,b.Length-s))/20);p.y=Mathf.Lerp(p.y,y,weight);}return p;}
  foreach(var mf in objects.Where(m=>m.gameObject.activeInHierarchy&&m.name.StartsWith("Ground_CR133"))){var mesh=Object.Instantiate(mf.sharedMesh);mesh.vertices=mesh.vertices.Select(v=>mf.transform.InverseTransformPoint(Map(mf.transform.TransformPoint(v)))).ToArray();mesh.RecalculateNormals();mesh.RecalculateBounds();EditorUtility.CopySerialized(mesh,mf.sharedMesh);Object.DestroyImmediate(mesh);var c=mf.GetComponent<MeshCollider>();c.sharedMesh=null;c.sharedMesh=mf.sharedMesh;EditorUtility.SetDirty(mf.sharedMesh);}
  foreach(var b in branches){b.points=b.points.Select(p=>Map(p+Vector3.up*.04f)-Vector3.up*.04f).ToArray();Refresh(b);}
  Physics.SyncTransforms();EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();
 }
 static void GradeReverseJunctions(){
  var race=Object.FindAnyObjectByType<RaceDirector>();var road=race.road;road.Initialize();var branches=Object.FindObjectsByType<WoodlandRoute>();var flights=race.GetComponent<MountainFlights>().flights;
  var mouths=new List<(Vector3 p,Vector3 gradient,float core,float outer)>();foreach(var b in branches)foreach(var endpoint in new[]{b.points[0],b.points[^1]}){float s=road.Project(endpoint,out _);var p=road.At(s,out var f)+Vector3.up*.04f;var flat=Vector3.ProjectOnPlane(f,Vector3.up);mouths.Add((p,flat*f.y/Mathf.Max(.01f,flat.sqrMagnitude),b.title=="Downhill Ridge Cut"?25:35,b.title=="Downhill Ridge Cut"?55:70));}
  Vector3 Map(Vector3 p){
   foreach(var f in flights){float length=Vector3.Dot(f.lip-f.start,f.forward),s=Vector3.Dot(p-f.start,f.forward);float u=Mathf.Clamp01((s-length+60)/60);float height=f.start.y+(f.lip.y-f.start.y)*u*u+.04f;if(s>=0&&s<=length&&Math.Abs(Vector3.Dot(p-f.start,Side(f.forward)))<16&&Math.Abs(p.y-height)<2)return p;}
   foreach(var j in mouths){float d=Flat(p,j.p);if(d>=j.outer)continue;float y=j.p.y+Vector3.Dot(p-j.p,j.gradient);if(Math.Abs(y-p.y)>12)continue;p.y=Mathf.Lerp(p.y,y,1-Smooth((d-j.core)/(j.outer-j.core)));}return p;
  }
  foreach(var mf in Object.FindObjectsByType<MeshFilter>().Where(m=>m.name.StartsWith("Ground_CR133"))){var copy=Object.Instantiate(mf.sharedMesh);copy.vertices=copy.vertices.Select(p=>Map(p)).ToArray();copy.RecalculateNormals();copy.RecalculateBounds();EditorUtility.CopySerialized(copy,mf.sharedMesh);Object.DestroyImmediate(copy);var collider=mf.GetComponent<MeshCollider>();collider.sharedMesh=null;collider.sharedMesh=mf.sharedMesh;EditorUtility.SetDirty(mf.sharedMesh);}
  road.points=road.points.Select(p=>Map(p+Vector3.up*.04f)-Vector3.up*.04f).ToArray();Refresh(road);foreach(var b in branches){b.points=b.points.Select(p=>Map(p+Vector3.up*.04f)-Vector3.up*.04f).ToArray();Refresh(b);b.entryRoad=road.Project(b.points[0],out _);b.exitRoad=road.Project(b.points[^1],out _);}foreach(var f in flights){f.approachStation=road.Project(f.start,out _);f.endStation=road.Project(f.landingEnd,out _);}
  Physics.SyncTransforms();EditorSceneManager.MarkSceneDirty(race.gameObject.scene);EditorSceneManager.SaveScene(race.gameObject.scene);AssetDatabase.SaveAssets();
 }
 public static string Laurel(){Init();EditorSceneManager.OpenScene("Assets/Scenes/StreetLoopReverse.unity");if(GameObject.Find("CR133 straight Laurel"))throw new Exception("Already authored");
  var branch=Object.FindObjectsByType<WoodlandRoute>().Single(b=>b.title=="Laurel Switchbacks");Refresh(branch);var takeoff=Read("Ground_CR122 Laurel smooth takeoff");var catchRoad=Read("Ground_CR122 Laurel supported catch");
  var lip=takeoff.p[^1];var land=catchRoad.p[0];var axis=Vector3.ProjectOnPlane(land-lip,Vector3.up).normalized;
  var start=lip-axis*100;float startStation=branch.Project(start,out _);start.y=lip.y-3.8f;
  float joinStation=Mathf.Max(0,startStation-100);var join=branch.At(joinStation,out var joinDir)+Vector3.up*.04f;
  var entry=Hermite(join,start,joinDir,axis);float rise=lip.y-start.y;
  if(rise<=0||rise>18)throw new Exception("Unexpected Laurel ramp rise "+rise);
  var run=Enumerable.Range(0,201).Select(i=>{float s=i*.5f;float u=Mathf.Clamp01((s-72)/28);return start+axis*s+Vector3.up*(rise*u*u);}).ToArray();
  var before=new List<Vector3>();for(float s=0;s<joinStation;s+=.5f)before.Add(branch.At(s,out _)+Vector3.up*.04f);
  var approach=new Strip("Laurel approach",before.Concat(entry).Concat(run.Skip(1)).ToArray(),5);
  float landEnd=branch.Project(catchRoad.p[^1],out _);var after=new List<Vector3>();for(float s=landEnd+.5f;s<branch.Length;s+=.5f)after.Add(branch.At(s,out _)+Vector3.up*.04f);after.Add(branch.points[^1]+Vector3.up*.04f);
  // C1 landing joins the retained normal shortcut direction over the full catch.
  var end=catchRoad.p[^1];branch.At(landEnd,out var endDir);var landing=new Strip("Laurel landing",Hermite(land,end,(axis-Vector3.up*.10f).normalized,endDir).Concat(after).ToArray(),6);
  var mat=GameObject.Find("Ground_CR122 Laurel smooth takeoff").GetComponent<Renderer>().sharedMaterial;
  GameObject.Find("Ground_CR122 Laurel smooth takeoff").SetActive(false);GameObject.Find("Ground_CR122 Laurel supported catch").SetActive(false);
  var root=new GameObject("CR133 straight Laurel").transform;var surface=new Surface();Union(surface,Ribbon(approach,last:axis));Union(surface,Ribbon(landing,first:axis));Make(root,"CR133 Laurel driving surface",surface.triangles,mat);
  // Remove the former terrain driving surface under the replacement shortcut.
  // Clip at the exact new ribbon edges; no stacked collider remains at entry.
  ground=GameObject.Find("Memory loop - north is +Z").GetComponentsInChildren<Collider>();
  TrimTerrain(surface,"Laurel");
  foreach(var zone in Object.FindObjectsByType<JumpRecoveryExclusion>().Where(z=>z.transform.IsChildOf(GameObject.Find("CR122 Laurel local jump").transform)))Object.DestroyImmediate(zone.gameObject);
  var exclusion=new GameObject("Laurel runway no recovery").AddComponent<JumpRecoveryExclusion>();exclusion.transform.SetParent(root);exclusion.start=start-axis*12;exclusion.end=lip;exclusion.halfWidth=8;
  branch.points=approach.p.Concat(landing.p).Select(p=>p-Vector3.up*.04f).ToArray();Refresh(branch);ClearCorridor(branch);
  Physics.SyncTransforms();EditorSceneManager.MarkSceneDirty(branch.gameObject.scene);EditorSceneManager.SaveScene(branch.gameObject.scene);AssetDatabase.SaveAssets();string report=$"Laurel: straight length=100m; level run-up=72m; quadratic ramp=28m; rise={rise:F3}; lip={lip}; landing={land}; horizontal axis={axis}; matching visible/collision mesh. Terrain clipped at actual road edges.";File.WriteAllText("Docs/CR133-137/Laurel-geometry.txt",report);return report;
 }
 static void TrimTerrain(Surface surface,string label){foreach(var collider in ground.OfType<MeshCollider>()){var mf=collider.GetComponent<MeshFilter>();if(!mf||!mf.sharedMesh)continue;var v=mf.sharedMesh.vertices;var originalColors=mf.sharedMesh.colors;var ix=mf.sharedMesh.triangles;var output=new List<Tri>();bool changed=false;for(int n=0;n<ix.Length;n+=3){var t=new Tri(mf.transform.TransformPoint(v[ix[n]]),mf.transform.TransformPoint(v[ix[n+1]]),mf.transform.TransformPoint(v[ix[n+2]]));if(originalColors.Length==v.Length){t.ca=originalColors[ix[n]];t.cb=originalColors[ix[n+1]];t.cc=originalColors[ix[n+2]];}var pieces=new List<List<Vector3>>{new(){t.a,t.b,t.c}};float radius=Mathf.Max(Flat(t.Center,t.a),Flat(t.Center,t.b),Flat(t.Center,t.c));foreach(var road in surface.Near(t.Center,radius)){if(Math.Abs(Height(t,road.Center)-road.Center.y)>16)continue;var next=new List<List<Vector3>>();foreach(var p in pieces)next.AddRange(Subtract(p,road,false));if(next.Count!=pieces.Count||next.Count==0||next.Any(p=>p.Count!=3))changed=true;pieces=next;if(pieces.Count==0)break;}foreach(var p in pieces)for(int i=1;i+1<p.Count;i++)output.Add(new(mf.transform.InverseTransformPoint(p[0]),mf.transform.InverseTransformPoint(p[i]),mf.transform.InverseTransformPoint(p[i+1])){ca=ColorAt(t,p[0]),cb=ColorAt(t,p[i]),cc=ColorAt(t,p[i+1])});}if(!changed)continue;foreach(var t in output){Vector3 Lower(Vector3 local){var p=mf.transform.TransformPoint(local);var r=surface.Nearest(p,.2f,out float d);if(r!=null&&d<.1f&&Math.Abs(Height(r,p)-p.y)<16)p.y=Mathf.Min(p.y,Height(r,p)-.12f);return mf.transform.InverseTransformPoint(p);}t.a=Lower(t.a);t.b=Lower(t.b);t.c=Lower(t.c);}mf.sharedMesh=Store(mf.gameObject.scene.name+"-"+label+"-"+mf.name,output);collider.sharedMesh=mf.sharedMesh;EditorUtility.SetDirty(mf);EditorUtility.SetDirty(collider);}}
 static string Mountain(bool reverse){Init();string scene="MountainLoop"+(reverse?"Reverse":"");EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");if(GameObject.Find("CR133 structural mountain roads"))throw new Exception("Already authored");var race=Object.FindAnyObjectByType<RaceDirector>();var road=race.road;road.Initialize();var flights=race.GetComponent<MountainFlights>().flights;var main=Enumerable.Range(0,3).Select(i=>Read("Ground_CR122 main section "+i)).ToArray();var branches=Object.FindObjectsByType<WoodlandRoute>();
 var mat=GameObject.Find("Ground_CR122 main section 0").GetComponent<Renderer>().sharedMaterial;
 ground=GameObject.Find("Memory loop - north is +Z").GetComponentsInChildren<Collider>().Concat(Object.FindObjectsByType<Collider>().Where(c=>c.name=="Ground_CR117 outer ravine")).ToArray();
 // Close the lap join with the SAME cross-section tangent on both sides.
 var seam=(main[0].p[1]-main[2].p[^2]).normalized;var surface=new Surface();for(int i=0;i<3;i++)Union(surface,Ribbon(main[i],i==0?seam:null,i==2?seam:null),differentLevels:true);
 var strips=new List<Strip>(main);var extra=new List<Strip>();
 foreach(var branch in branches){Refresh(branch);
 if(reverse&&branch.title=="Summit Traverse"){float endStation=100;var end=branch.At(endStation,out var tangent);float y0=branch.points[0].y;var points=branch.points.ToArray();for(int k=0;k<points.Length;k++){float s0=branch.Project(points[k],out _);if(s0>=endStation)continue;float t=s0/endStation,t2=t*t,t3=t2*t;points[k].y=(2*t3-3*t2+1)*y0+(t3-2*t2+t)*(-.1f)*endStation+(-2*t3+3*t2)*end.y+(t3-t2)*(tangent.y/Mathf.Max(.1f,Vector3.ProjectOnPlane(tangent,Vector3.up).magnitude))*endStation;}branch.points=points;Refresh(branch);}
  if(!reverse&&branch.title=="Summit Traverse"){
   float mouth=road.Project(new Vector3(990,163.43f,115),out _);var join=road.At(mouth-12,out var entryForward)+Vector3.up*.04f;
   // The old shortcut crossed underneath the high catch road as well as
   // starting in the flight gap. Keep its summit-to-lower-route function, but
   // route it south of the airborne corridor and reconnect at the same exit.
   var controls=new[]{join,new Vector3(960,155,88),new Vector3(895,140,58),new Vector3(825,119,40),new Vector3(750,95,28),branch.points[^1]+Vector3.up*.04f};
   var connect=new List<Vector3>();for(int i=1;i<controls.Length;i++){var fa=i==1?(entryForward-Side(entryForward)*.6f).normalized:(controls[i]-controls[i-2]).normalized;var fb=i==controls.Length-1?road.At(road.Project(controls[i],out _),out var ef)*0+ef:(controls[i+1]-controls[i-1]).normalized;connect.AddRange(Hermite(controls[i-1],controls[i],fa,fb).Skip(i==1?0:1));}
   branch.points=connect.Select(p=>p-Vector3.up*.04f).ToArray();Refresh(branch);
  }
  if(reverse&&branch.title=="Downhill Ridge Cut"){
   // Preserve the explicitly authored over-road flight and its far-side catch.
   var gap=Enumerable.Range(1,branch.points.Length-1).OrderByDescending(i=>Flat(branch.points[i-1],branch.points[i])).First();
   extra.Add(new("reverse shortcut runway",branch.points.Take(gap).Select(p=>p+Vector3.up*.04f).ToArray(),4.2f){bank=false});extra.Add(new("reverse shortcut catch",branch.points.Skip(gap).Select(p=>p+Vector3.up*.04f).ToArray(),5){bank=false});continue;
  }
  var p=branch.points.Select(x=>x+Vector3.up*.04f).ToArray();
  // At mouths blend onto the actual main triangle plane, not nearest 3D
  // centreline distance (which incorrectly treats elevation as road width).
  for(int i=0;i<p.Length;i++){var t=surface.NearestLevel(p[i],38,out float d);if(!(reverse&&branch.title=="Summit Traverse")&&t!=null&&d<32&&(branch.Project(p[i],out _)<65||branch.Project(p[i],out _)>branch.Length-65)){float target=Height(t,p[i]);target=Mathf.Clamp(target,p[i].y-12,p[i].y+12);{float station=branch.Project(p[i],out _);float endWeight=Smooth((65-Mathf.Min(station,branch.Length-station))/20);p[i].y=Mathf.Lerp(p[i].y,target,(1-Smooth(d/32))*endWeight);}}}
  branch.points=p.Select(x=>x-Vector3.up*.04f).ToArray();Refresh(branch);var s=new Strip(branch.title,p,branch.halfWidth);strips.Add(s);Union(surface,Ribbon(s),differentLevels:true);
 }
 foreach(var s in extra)Union(surface,Ribbon(s),differentLevels:true);
 var old=Object.FindObjectsByType<MeshCollider>().Where(m=>m.name.StartsWith("Ground_CR122")||m.name=="Ground_Summit authored launch").ToArray();foreach(var m in old)m.gameObject.SetActive(false);
 var root=new GameObject("CR133 structural mountain roads").transform;Make(root,"CR133 mountain driving surface",surface.triangles,mat);
 
 // Support each ordinary road with visible, collidable earth banks whose inner
 // edge is the road edge. Clip bank overlaps against the road footprint.
 var combined=new Surface();foreach(var t in surface.triangles)combined.Add(t);int count=combined.triangles.Count;foreach(var s in strips)Union(combined,Banks(s,flights),seat:false);
 var bankMat=new Material(mat){name="CR133 roadside earth",color=new Color(.31f,.37f,.19f)};string mp=Folder+"/"+scene+"-earth.mat";var savedMat=AssetDatabase.LoadAssetAtPath<Material>(mp);if(savedMat){Object.DestroyImmediate(bankMat);bankMat=savedMat;}else AssetDatabase.CreateAsset(bankMat,mp);Make(root,"CR133 mountain earth banks",combined.triangles.Skip(count),bankMat);
 TrimTerrain(surface,"mountain-subgrade");Physics.SyncTransforms();foreach(var b in branches){b.entryRoad=road.Project(b.points[0],out _);b.exitRoad=road.Project(b.points[^1],out _);b.bypassedGates=Enumerable.Range(1,race.gates.Length-1).Where(i=>road.Relative(road.Project(race.gates[i].transform.position,out _),b.entryRoad)<road.Relative(b.exitRoad,b.entryRoad)).ToArray();EditorUtility.SetDirty(b);}
 race.courseId=reverse?"mountain-reverse-v5-supported":"mountain-forward-v5-supported";EditorUtility.SetDirty(race);if(reverse)GradeReverseJunctions();else GradeMouths();var report=$"{scene}: union road triangles={surface.triangles.Count}; earth bank triangles={combined.triangles.Count-count}; intentional flight gaps retained";File.WriteAllText("Docs/CR133-137/"+scene+"-geometry.txt",report);return report;
 }
}











