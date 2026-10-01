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
public static class MountainPolish {
 const string Folder="Assets/Track/MountainPolish";
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

 static readonly List<string> log=new();
 static List<Tri> ReadMesh(MeshFilter mf){var v=mf.sharedMesh.vertices.Select(mf.transform.TransformPoint).ToArray();var ix=mf.sharedMesh.triangles;var cs=mf.sharedMesh.colors;return Enumerable.Range(0,ix.Length/3).Select(i=>new Tri(v[ix[i*3]],v[ix[i*3+1]],v[ix[i*3+2]]){ca=cs.Length==v.Length?cs[ix[i*3]]:Color.white,cb=cs.Length==v.Length?cs[ix[i*3+1]]:Color.white,cc=cs.Length==v.Length?cs[ix[i*3+2]]:Color.white}).ToList();}
 static void Assign(MeshFilter mf,List<Tri> tris,string label){var mesh=Store(mf.gameObject.scene.name+"-"+label,tris);mf.sharedMesh=mesh;if(mf.TryGetComponent<MeshCollider>(out var c)){c.sharedMesh=null;c.sharedMesh=mesh;}EditorUtility.SetDirty(mf);}
 static void Save(){var s=UnityEngine.SceneManagement.SceneManager.GetActiveScene();EditorSceneManager.MarkSceneDirty(s);EditorSceneManager.SaveScene(s);AssetDatabase.SaveAssets();}
 static bool Tree(Collider c)=>c.name.IndexOf("tree",StringComparison.OrdinalIgnoreCase)>=0||c.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0;
 static float Support(Vector3 p)=>Physics.RaycastAll(new(p.x,p.y+25,p.z),Vector3.down,150,1,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name.StartsWith("Ground_")).Select(h=>h.point.y).DefaultIfEmpty(p.y).Max();
 static void SeatTrees(Func<Collider,bool> local,Func<Collider,bool> remove=null){var trees=Object.FindObjectsByType<Collider>().Where(Tree).Where(local).ToArray();typeof(Racer.Editor.BackyardReverseAuthoring).GetMethod("Trees",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static).Invoke(null,new object[]{remove??((Func<Collider,bool>)(c=>false)),(Func<Collider,bool>)(c=>trees.Contains(c))});log.Add("Tree patch inspected="+trees.Length);}
 public static string Forest(){Directory.CreateDirectory(Folder);Directory.CreateDirectory("Docs/MountainPolish");EditorSceneManager.OpenScene("Assets/Scenes/LakeWoods.unity");Physics.SyncTransforms();var p=new Vector3(174.8f,48.9f,279.2f);var trees=Object.FindObjectsByType<Collider>().Where(Tree).Where(c=>Flat(c.bounds.center,p)<35).ToArray();File.WriteAllLines("Docs/MountainPolish/forest-trees-before.txt",trees.Select(c=>$"{c.name} {c.bounds.center:F3} foot={c.bounds.min.y:F3} ground={Support(c.bounds.center):F3}"));SeatTrees(c=>trees.Contains(c));Physics.SyncTransforms();File.WriteAllLines("Docs/MountainPolish/forest-trees-after.txt",trees.Select(c=>$"{c.name} {c.bounds.center:F3} foot={c.bounds.min.y:F3} ground={Support(c.bounds.center):F3}"));Save();return "Grounded local Forest tree patch; cave untouched";}
 public static string Reverse(){return Author(true);}
 public static string Forward(){return Author(false);}
 static string Author(bool reverse){
 Directory.CreateDirectory(Folder);Directory.CreateDirectory("Docs/MountainPolish");log.Clear();string scene=reverse?"MountainLoopReverse":"MountainLoop";EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();var race=Object.FindAnyObjectByType<RaceDirector>();var road=race.road;road.Initialize();var branch=Object.FindObjectsByType<WoodlandRoute>().Single(b=>b.title=="Summit Traverse");branch.Initialize();
 var mf=GameObject.Find("Ground_CR133 mountain driving surface").GetComponent<MeshFilter>();var surface=new Surface();foreach(var t in ReadMesh(mf))surface.Add(t);var original=branch.points.ToArray();float oldLength=branch.Length;
 if(reverse){
 const float cut=245;var a=branch.At(cut,out var fa);var exit=road.Project(new Vector3(925,122,-100),out _);var end=road.At(exit,out var fe);
 var route=new List<Vector3>();for(float s=0;s<cut;s+=.5f)route.Add(branch.At(s,out _));
 var knots=new[]{a,new Vector3(934,125,-48),new Vector3(887,116,-48),new Vector3(863,111,-73),new Vector3(888,114,-99),end};
 var tangents=new[]{fa,new Vector3(-1,-.16f,0),new Vector3(-1,-.18f,0),new Vector3(0,0,-1),new Vector3(1,.22f,0),fe};
 for(int i=1;i<knots.Length;i++){var segment=Hermite(knots[i-1],knots[i],tangents[i-1],tangents[i]);route.AddRange(segment.Skip(i==1?0:1));}
 var oldTail=new Surface();var tail=Enumerable.Range(0,Mathf.CeilToInt((oldLength-cut)/2)+1).Select(i=>branch.At(Mathf.Min(oldLength,cut+i*2),out _)).ToArray();foreach(var t in Ribbon(new Strip("old tail",tail,9)))oldTail.Add(t);
 var retained=new Surface();int retired=0;foreach(var t in surface.triangles){road.Project(t.Center,out float rd);var main=road.At(road.Project(t.Center,out _),out _);if(rd<9&&Math.Abs(t.Center.y-main.y)<1){retained.Add(t);continue;}var pieces=new List<List<Vector3>>{new(){t.a,t.b,t.c}};float radius=Mathf.Max(Flat(t.Center,t.a),Flat(t.Center,t.b),Flat(t.Center,t.c));foreach(var old in oldTail.Near(t.Center,radius)){if(Math.Abs(Height(old,t.Center)-t.Center.y)>1)continue;var next=new List<List<Vector3>>();foreach(var p in pieces)next.AddRange(Subtract(p,old,false));pieces=next;}if(pieces.Count!=1||pieces[0].Count!=3)retired++;foreach(var p in pieces)for(int j=1;j+1<p.Count;j++)retained.Add(new(p[0],p[j],p[j+1]));}
 surface=retained;branch.points=route.ToArray();Refresh(branch);branch.exitRoad=exit;branch.aiValidated=true;branch.recommendedSpeed=20;race.courseId="mountain-reverse-v4-natural-summit-merge";EditorUtility.SetDirty(race);EditorUtility.SetDirty(branch);
 Union(surface,Ribbon(new Strip("rejoin",route.Where(p=>branch.Project(p,out _)>=cut-.5f).Select(p=>p+Vector3.up*.04f).ToArray(),6)),true);log.Add($"Final rejoin starts at {a}; merges at {end}, exit={exit:F2}. Old length={oldLength:F2}, new={branch.Length:F2}, replaced triangles={retired}. Remaining main distance skipped={exit-branch.entryRoad:F2}m.");
 // Remove obsolete guidance only in the replaced tail.
 foreach(var text in Object.FindObjectsByType<TextMesh>().Where(t=>t.text.IndexOf("HAIRPIN",StringComparison.OrdinalIgnoreCase)>=0||t.text.IndexOf("TURN BACK ONTO TEAL",StringComparison.OrdinalIgnoreCase)>=0).ToArray()){var sign=text.GetComponentInParent<PhysicalSign>();if(sign)Object.DestroyImmediate(sign.gameObject);else Object.DestroyImmediate(text.transform.parent.gameObject);}
 var oldObj=new GameObject("Temporary old branch").AddComponent<WoodlandRoute>();oldObj.points=original;
 foreach(var arrow in Object.FindObjectsByType<MeshFilter>().Where(m=>m.name.Contains("gold")||m.name.Contains("Gold")).ToArray()){if(!arrow.TryGetComponent<Renderer>(out var r))continue;float s=oldObj.Project(r.bounds.center,out float d);if(s>cut-10&&d<15)Object.DestroyImmediate(arrow.gameObject);}Object.DestroyImmediate(oldObj.gameObject);
 }
 // Connect the existing entry apron to the main road; retained pavement wins overlap.
 if(reverse){var start=road.At(branch.entryRoad-7,out var f);var p=branch.At(12,out var bf);var entry=Hermite(start,p,f,bf);Union(surface,Ribbon(new Strip("entry apron",entry.Select(p=>p+Vector3.up*.04f).ToArray(),6)),true);}
 Assign(mf,surface.triangles,"supported-driving");Physics.SyncTransforms();
 // Exposed boundary edges receive actual earth support down to surrounding terrain.
 SupportEdges(mf,scene);
 Physics.SyncTransforms();
 SeatProps(branch,road);
 if(reverse){SeatTrees(c=>{float s=branch.Project(c.bounds.center,out float d);return s>230&&d<20;},c=>{float s=branch.Project(c.bounds.center,out float d);return s>245&&d<7&&Math.Abs(c.bounds.min.y-branch.At(s,out _).y)<15;});AddArrows(branch);var exclusion=new GameObject("Summit merge no recovery").AddComponent<JumpRecoveryExclusion>();exclusion.start=branch.At(branch.Length-20,out _);exclusion.end=road.At(branch.exitRoad+12,out _);exclusion.halfWidth=9;}
 Save();File.WriteAllLines("Docs/MountainPolish/"+scene+"-authoring.txt",log);return string.Join("\n",log);
 }
 static void SupportEdges(MeshFilter mf,string scene){
 var faces=ReadMesh(mf);var pavement=new Surface();foreach(var t in faces)pavement.Add(t);var edges=new Dictionary<(Vector3Int,Vector3Int),(Vector3 a,Vector3 b,int count)>();Vector3Int Key(Vector3 p)=>Vector3Int.RoundToInt(p*1000);
 foreach(var t in faces){var p=new[]{t.a,t.b,t.c};for(int i=0;i<3;i++){var a=p[i];var b=p[(i+1)%3];var k=(Key(a),Key(b));var flip=(k.Item2,k.Item1);if(edges.TryGetValue(flip,out var old))edges[flip]=(old.a,old.b,old.count+1);else if(edges.TryGetValue(k,out old))edges[k]=(old.a,old.b,old.count+1);else edges[k]=(a,b,1);}}
 var baseGround=Object.FindObjectsByType<Collider>().Where(c=>c.name.StartsWith("Ground_")&&!c.name.Contains("driving surface")&&!c.name.Contains("earth banks")).ToArray();
 float Below(Vector3 p){float y=float.NegativeInfinity;foreach(var c in baseGround){if(c.Raycast(new Ray(p+Vector3.up*.1f,Vector3.down),out var h,160))y=Mathf.Max(y,h.point.y);}return float.IsNegativeInfinity(y)?p.y-12:y;}
 var tris=new List<Tri>();int count=0;foreach(var e in edges.Values.Where(e=>e.count==1)){var mid=(e.a+e.b)*.5f;if(Flat(e.a,e.b)<.02f||Flat(e.a,e.b)>25)continue;float ya=Below(e.a),yb=Below(e.b);if(e.a.y-ya<.15f&&e.b.y-yb<.15f)continue;
 // Intentional flight lips remain open. Only support the road's lengthwise edges.
 var road=Object.FindAnyObjectByType<RaceRoad>();float rs=road.Project(mid,out float rd);road.At(rs,out var f);if(rd<15&&Math.Abs(Vector3.Dot((e.b-e.a).normalized,f))<.5f)continue;
 var outward=-Side(e.b-e.a);var probe=mid+outward*.15f;if(pavement.Near(probe).Any(t=>Distance(t,probe)<.001f&&Math.Abs(Height(t,probe)-probe.y)<.3f))continue;var a=e.a;var b=e.b;var aa=a+outward*4;var bb=b+outward*4;aa.y=Mathf.Min(a.y-.1f,Below(aa));bb.y=Mathf.Min(b.y-.1f,Below(bb));
 tris.Add(new(a,b,aa));tris.Add(new(b,bb,aa));var va=a;var vb=b;va.y=ya-.1f;vb.y=yb-.1f;tris.Add(new(a,va,b));tris.Add(new(b,va,vb));count++;}
 foreach(var t in tris)t.ca=t.cb=t.cc=new Color(.39f,.48f,.29f);var mat=Object.FindObjectsByType<MeshRenderer>().First(r=>r.name.StartsWith("Ground_Mountain_")).sharedMaterial;var root=new GameObject("Mountain corridor earth support").transform;Make(root,"MountainPolish supported shoulders",tris,mat);log.Add("Supported exposed corridor edges="+count);
 }
 public static string RefineReverse(){EditorSceneManager.OpenScene("Assets/Scenes/MountainLoopReverse.unity");log.Clear();Object.DestroyImmediate(GameObject.Find("Mountain corridor earth support"));var branch=Object.FindObjectsByType<WoodlandRoute>().Single(b=>b.title=="Summit Traverse");var ribbon=new Surface();var points=Enumerable.Range(0,Mathf.CeilToInt((branch.Length-245)/2)+1).Select(i=>branch.At(Mathf.Min(branch.Length,245+i*2),out _)+Vector3.up*.02f).ToArray();foreach(var t in Ribbon(new Strip("cut",points,7)))ribbon.Add(t);
 foreach(var mf in Object.FindObjectsByType<MeshFilter>().Where(m=>m.name.StartsWith("Ground_")&&!m.name.Contains("driving surface")&&m.sharedMesh).ToArray()){var tris=ReadMesh(mf);var result=new List<Tri>();int changed=0;foreach(var t in tris){var pieces=new List<List<Vector3>>{new(){t.a,t.b,t.c}};float radius=Mathf.Max(Flat(t.Center,t.a),Flat(t.Center,t.b),Flat(t.Center,t.c));foreach(var road in ribbon.Near(t.Center,radius)){if(t.Center.y<Height(road,t.Center)-.1f)continue;var next=new List<List<Vector3>>();foreach(var p in pieces)next.AddRange(Subtract(p,road,false));pieces=next;}if(pieces.Count!=1||pieces[0].Count!=3)changed++;foreach(var p in pieces)for(int j=1;j+1<p.Count;j++)result.Add(new(p[0],p[j],p[j+1]){ca=ColorAt(t,p[0]),cb=ColorAt(t,p[j]),cc=ColorAt(t,p[j+1])});}if(changed>0){Assign(mf,result,"trim-"+mf.name);log.Add(mf.name+" locally trimmed faces="+changed);}}
 Physics.SyncTransforms();SupportEdges(GameObject.Find("Ground_CR133 mountain driving surface").GetComponent<MeshFilter>(),"MountainLoopReverse");Physics.SyncTransforms();SeatProps(branch,Object.FindAnyObjectByType<RaceRoad>());SeatTrees(c=>{branch.Project(c.bounds.center,out float d);return d<22;});Save();File.WriteAllLines("Docs/MountainPolish/reverse-refinement.txt",log);return string.Join("\n",log);}
 static void SeatProps(WoodlandRoute branch,RaceRoad road){int signs=0,cairns=0;
 foreach(var sign in Object.FindObjectsByType<PhysicalSign>()){var rs=sign.GetComponentsInChildren<Renderer>();if(rs.Length==0)continue;var p=sign.transform.position;branch.Project(p,out float d);road.Project(p,out float rd);if(Math.Min(d,rd)>30)continue;var posts=rs.Where(r=>r.name.IndexOf("post",StringComparison.OrdinalIgnoreCase)>=0).ToArray();if(posts.Length==0)continue;float foot=posts.Min(r=>r.bounds.min.y),ground=Support(new Vector3(p.x,foot,p.z));float dy=ground-foot;if(Math.Abs(dy)>.08f&&Math.Abs(dy)<20){sign.transform.position+=Vector3.up*dy;signs++;}}
 foreach(var group in Object.FindObjectsByType<Transform>().Where(t=>t.name.StartsWith("Acorn clue /"))){var rs=group.GetComponentsInChildren<Renderer>();if(rs.Length==0)continue;var p=rs[0].bounds.center;branch.Project(p,out float d);road.Project(p,out float rd);if(Math.Min(d,rd)>30)continue;float foot=rs.Min(r=>r.bounds.min.y);float dy=Support(new(p.x,foot,p.z))-foot;if(Math.Abs(dy)>.08f&&Math.Abs(dy)<20){group.position+=Vector3.up*dy;cairns++;}}
 log.Add($"Grounded signs={signs}, cairns={cairns}");}
 static void AddArrows(WoodlandRoute b){var mat=AssetDatabase.LoadAssetAtPath<Material>("Assets/Track/Discovery/CR117 alternate gold.mat");for(float s=245;s<b.Length-5;s+=18){var p=b.At(s,out var f);var side=Side(f);f=Vector3.ProjectOnPlane(f,Vector3.up).normalized;var shape=new[]{new Vector2(-.44f,-3),new(.44f,-3),new(.44f,0),new(1.1f,0),new(0,3),new(-1.1f,0),new(-.44f,0)};var v=shape.Select(q=>{var x=p+side*q.x+f*q.y;var hits=Physics.RaycastAll(x+Vector3.up*2,Vector3.down,4).Where(h=>h.collider.name.Contains("driving surface")).ToArray();if(hits.Length>0)x.y=hits.OrderBy(h=>Math.Abs(h.point.y-p.y)).First().point.y+.08f;return x;}).ToArray();var mesh=new Mesh{vertices=v,triangles=new[]{0,6,1,1,6,2,6,5,4,6,4,2,2,4,3}};mesh.RecalculateNormals();mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,Folder+"/reverse-merge-arrow-"+s+".asset");var go=new GameObject("Summit merge gold arrow",typeof(MeshFilter),typeof(MeshRenderer));go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<MeshRenderer>().sharedMaterial=mat;}}
}
