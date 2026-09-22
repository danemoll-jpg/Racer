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
public static partial class LocalCorrectiveRoads {
 const string Folder="Assets/Track/LocalRecovery";
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
}
