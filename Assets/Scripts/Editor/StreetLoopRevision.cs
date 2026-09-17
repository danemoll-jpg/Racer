using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;
namespace Racer.Editor {
// Only environment authoring. One continuous height field is both visible ground and collision.
public static class StreetLoopRevision {
 const string Folder="Assets/Track/StreetLoop";
 public const float Cell=2, ShoulderEnd=16, ForestClearance=23;
 const int N=801, Tile=80;
 static readonly Vector3 Origin=new(-800,0,-750);
 static List<Vector3> route;
 static float house3Height, friendHeight;
 static Vector3 house3Road;
 static float[,] heights;
 static float[,] distances;
 static Dictionary<Vector2Int,List<int>> bins;
 static Vector4[] bounds;
 static Vector3 Map(float x,float y)=>new((x-650)*1.1f,0,(950-y)*1.1f);
 static readonly Vector3 House3=Map(1040,1150), Friend=Map(1115,982);
 static void Prepare() {
  route=StreetLoopBuilder.Route(); bins=new();bounds=new Vector4[(route.Count+15)/16];
  for(int k=0;k<bounds.Length;k++){var b=new Vector4(float.MaxValue,float.MaxValue,float.MinValue,float.MinValue);for(int j=k*16;j<=Mathf.Min(route.Count,k*16+16);j++){var p=route[j%route.Count];b.x=Mathf.Min(b.x,p.x);b.y=Mathf.Min(b.y,p.z);b.z=Mathf.Max(b.z,p.x);b.w=Mathf.Max(b.w,p.z);}bounds[k]=b;}
  for(int i=0;i<route.Count;i++){var p=route[i];var k=new Vector2Int(Mathf.FloorToInt(p.x/64),Mathf.FloorToInt(p.z/64));if(!bins.TryGetValue(k,out var list))bins[k]=list=new();list.Add(i);} Near(House3,out var h3);house3Height=h3.y;house3Road=h3;Near(Friend,out var fr);friendHeight=fr.y;
 }
 static float Near(Vector3 p,out Vector3 nearest) {
  var key=new Vector2Int(Mathf.FloorToInt(p.x/64),Mathf.FloorToInt(p.z/64));float best=float.MaxValue;Vector3 found=default;
  void Segment(int i){var a=route[i];var b=route[(i+1)%route.Count];var v=b-a;v.y=0;var q=p-a;q.y=0;float t=Mathf.Clamp01(Vector3.Dot(q,v)/v.sqrMagnitude);var pt=Vector3.Lerp(a,b,t);float d=(p.x-pt.x)*(p.x-pt.x)+(p.z-pt.z)*(p.z-pt.z);if(d<best){best=d;found=pt;}}
  for(int z=-1;z<=1;z++)for(int x=-1;x<=1;x++)if(bins.TryGetValue(key+new Vector2Int(x,z),out var list))foreach(int i in list)Segment(i);
  // A local candidate beyond 64m does not guarantee the global nearest segment.
  if(best>64*64)for(int k=0;k<bounds.Length;k++){var b=bounds[k];float dx=Mathf.Max(Mathf.Max(b.x-p.x,0),p.x-b.z),dz=Mathf.Max(Mathf.Max(b.y-p.z,0),p.z-b.w);if(dx*dx+dz*dz>best)continue;for(int i=k*16;i<Mathf.Min(route.Count,k*16+16);i++)Segment(i);}
  nearest=found;return Mathf.Sqrt(best);
 }
 static float Height(Vector3 p,out float distance) {
  distance=Near(p,out var road);float sum=0,w=0;
  for(int i=0;i<route.Count;i+=35){var q=p-route[i];q.y=0;float a=1/Mathf.Pow(q.sqrMagnitude+1600,2);sum+=route[i].y*a;w+=a;}
  float blend=Mathf.SmoothStep(0,1,Mathf.InverseLerp(ShoulderEnd,110,distance));
  float ground=Mathf.Lerp(road.y,sum/w,blend)-.45f*Mathf.SmoothStep(0,1,Mathf.InverseLerp(6,ShoulderEnd,distance));
  ground+=blend*(Mathf.PerlinNoise((p.x+1000)/160,(p.z+1000)/160)*12-6);
  // Site bowls start outside normal shoulders. Flat interior supports the visible foundation.
  ground=Site(p,House3,26,64,28,ground,distance);
  ground=Site(p,Friend,12,27,3,ground,distance);
  // A downhill sightline to House 3: a local sloped outer shoulder begins at 8m, then descends
  // continuously toward its site instead of retaining an intervening interpolated ridge.
  var axis=House3-house3Road;axis.y=0;var siteDelta=p-house3Road;siteDelta.y=0;
  float along=Vector3.Dot(siteDelta,axis.normalized),across=(siteDelta-axis.normalized*along).magnitude;
  if(along>8 && along<axis.magnitude+20){float blendAcross=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(18,45,across));float t=Mathf.Clamp01((along-8)/(axis.magnitude-8-20));float target=house3Height-.45f-27.55f*t;target=Mathf.Max(target,road.y-.45f-Mathf.Max(0,distance-8)*.4f);ground=Mathf.Lerp(ground,Mathf.Min(ground,target),blendAcross*Mathf.SmoothStep(0,1,Mathf.InverseLerp(8,14,distance)));}
  return ground;
 }
 static float Site(Vector3 p,Vector3 site,float inner,float outer,float below,float ground,float distance){float siteHeight=site==House3?house3Height:friendHeight;float d=Vector2.Distance(new(p.x,p.z),new(site.x,site.z));float b=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(inner,outer,d));b*=Mathf.SmoothStep(0,1,Mathf.InverseLerp(ShoulderEnd,24,distance));return Mathf.Lerp(ground,siteHeight-below,b);}
 public static float Surface(Vector3 p) {
  float x=(p.x-Origin.x)/Cell,z=(p.z-Origin.z)/Cell;int ix=Mathf.Clamp((int)x,0,N-2),iz=Mathf.Clamp((int)z,0,N-2);float u=x-ix,v=z-iz;
  return u+v<=1?heights[ix,iz]+u*(heights[ix+1,iz]-heights[ix,iz])+v*(heights[ix,iz+1]-heights[ix,iz]):heights[ix+1,iz+1]+(1-u)*(heights[ix,iz+1]-heights[ix+1,iz+1])+(1-v)*(heights[ix+1,iz]-heights[ix+1,iz+1]);
 }
 static Color ColorAt(float d){var road=new Color(.24f,.25f,.26f);var shoulder=new Color(.46f,.45f,.39f);var grass=new Color(.35f,.40f,.30f);return d<6?Color.Lerp(road,shoulder,Mathf.SmoothStep(0,1,Mathf.InverseLerp(4,6,d))):Color.Lerp(shoulder,grass,Mathf.SmoothStep(0,1,Mathf.InverseLerp(9,16,d)));}
 static Mesh SaveMesh(Mesh mesh,string path){mesh.name=Path.GetFileNameWithoutExtension(path);var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(old){old.Clear();old.name=mesh.name;old.indexFormat=mesh.indexFormat;old.vertices=mesh.vertices;old.triangles=mesh.triangles;old.colors=mesh.colors;old.normals=mesh.normals;old.RecalculateBounds();old.UploadMeshData(false);Object.DestroyImmediate(mesh);EditorUtility.SetDirty(old);return old;}AssetDatabase.CreateAsset(mesh,path);return mesh;}
 static GameObject MeshGO(string name,Mesh mesh,Material mat,Transform parent,bool collider){var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(parent,false);go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<MeshRenderer>().sharedMaterial=mat;if(collider)go.AddComponent<MeshCollider>().sharedMesh=mesh;return go;}
 [MenuItem("Racer/Rebuild Phase 2 Feedback Environment")]
 public static void Rebuild(){
  if(Application.isPlaying || SceneManager.GetActiveScene().path!=StreetLoopBuilder.ScenePath || SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Open saved StreetLoopGreybox and exit Play mode before regenerating environment.");
  Prepare();heights=new float[N,N];distances=new float[N,N];
  System.Threading.Tasks.Parallel.For(0,N,new System.Threading.Tasks.ParallelOptions { MaxDegreeOfParallelism=4 },z=>{for(int x=0;x<N;x++){var p=Origin+new Vector3(x*Cell,0,z*Cell);heights[x,z]=Height(p,out distances[x,z]);}});
  foreach(string name in new[]{"Memory loop - north is +Z","Remembered houses and approximate buildings","Woods replacing later subdivisions"}){var old=GameObject.Find(name);if(old)Object.DestroyImmediate(old);}
  var root=new GameObject("Memory loop - north is +Z").transform;
  var mat=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/ContinuousGround.mat");if(!mat){mat=new Material(Shader.Find("Racer/GreyboxGround"));AssetDatabase.CreateAsset(mat,Folder+"/ContinuousGround.mat");}
  for(int tz=0;tz<N-1;tz+=Tile)for(int tx=0;tx<N-1;tx+=Tile){int size=Tile+1;var v=new Vector3[size*size];var colors=new Color[v.Length];var normals=new Vector3[v.Length];var tri=new List<int>();
   for(int z=0;z<size;z++)for(int x=0;x<size;x++){int gx=tx+x,gz=tz+z,k=z*size+x;v[k]=Origin+new Vector3(gx*Cell,heights[gx,gz],gz*Cell);colors[k]=ColorAt(distances[gx,gz]);normals[k]=new Vector3(heights[Mathf.Max(0,gx-1),gz]-heights[Mathf.Min(N-1,gx+1),gz],2*Cell,heights[gx,Mathf.Max(0,gz-1)]-heights[gx,Mathf.Min(N-1,gz+1)]).normalized;if(x<Tile&&z<Tile)tri.AddRange(new[]{k,k+size,k+1,k+1,k+size,k+size+1});}
   var mesh=new Mesh{name=$"Continuous ground {tx}_{tz}"};mesh.vertices=v;mesh.triangles=tri.ToArray();mesh.colors=colors;mesh.normals=normals;mesh.RecalculateBounds();mesh=SaveMesh(mesh,$"{Folder}/Ground_{tx}_{tz}.asset");MeshGO(mesh.name,mesh,mat,root,true);
  }
  var buildings=new GameObject("Remembered houses and approximate buildings").transform;
  Material M(string n)=>AssetDatabase.LoadAssetAtPath<Material>(Folder+"/"+n+".mat");
  void Cube(string name,Transform parent,Vector3 position,Vector3 size,Material material){var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=position;go.transform.localScale=size;go.GetComponent<Renderer>().sharedMaterial=material;}
  void House(string name,Vector3 p,bool key=false,float width=13,float depth=10){p.y=Surface(p);Near(p,out var near);var t=new GameObject(name).transform;t.SetParent(buildings);t.position=p;t.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(near-p,Vector3.up));float low=p.y,high=p.y;foreach(float x in new[]{-width/2,width/2})foreach(float z in new[]{-depth/2,depth/2}){float y=Surface(t.TransformPoint(new Vector3(x,0,z)));low=Mathf.Min(low,y);high=Mathf.Max(high,y);}float top=high-p.y+.5f;Cube("Foundation",t,new(0,(low-p.y+top)*.5f,0),new(width+1,top-(low-p.y)+.2f,depth+1),M("Shoulder"));Cube("House mass",t,new(0,top+2.5f,0),new(width,5,depth),M(key?"Landmark":"House"));Cube("Roof mass",t,new(0,top+5.4f,0),new(width+1,.8f,depth+1),M("Roof"));}
  House("Dan - blue X",Map(970,854),true);House("Original house 1",Map(1028,951),true);House("Original house 2",Map(1049,1051),true);House("Original house 3",House3,true);House("Friend across street - blue circle",Friend,true);House("Remembered house behind southern hairpin",Map(1090,1492),true);
  var rng=new System.Random(1978);
  foreach(var p in new[]{new Vector2(45,660),new Vector2(113,820),new Vector2(43,1015),new Vector2(118,1130),new Vector2(43,1290),new Vector2(730,1260),new Vector2(690,1360),new Vector2(530,1441),new Vector2(367,1425),new Vector2(116,725),new Vector2(42,890),new Vector2(121,970),new Vector2(41,1170),new Vector2(126,1350),new Vector2(594,1415),new Vector2(615,1310),new Vector2(780,1170),new Vector2(872,1325),new Vector2(1000,1410),new Vector2(1180,1340)})House("Approximate older residence",Map(p.x,p.y),false,11+(float)rng.NextDouble()*5,9+(float)rng.NextDouble()*3);
  for(float x=165;x<=855;x+=67)foreach(int side in new[]{-1,1}){var p=Map(x,460);Near(p,out var road);p=road+Vector3.forward*side*(30+(float)rng.NextDouble()*8);House("Approximate main road business "+side,p,false,20+(float)rng.NextDouble()*11,15+(float)rng.NextDouble()*5);}
  var woods=new GameObject("Woods replacing later subdivisions").transform;
  // Batched low-poly tree masses; trunk colliders only. Canopies never cover normal shoulders.
  var batches=new Dictionary<Vector2Int,List<CombineInstance>>();var primitive=GameObject.CreatePrimitive(PrimitiveType.Cube);var cube=primitive.GetComponent<MeshFilter>().sharedMesh;Object.DestroyImmediate(primitive);int count=0;
  for(int i=0;i<7000;i++){var p=new Vector3(Mathf.Lerp(-740,735,(float)rng.NextDouble()),0,Mathf.Lerp(-680,525,(float)rng.NextDouble()));if(Near(p,out _)<ForestClearance)continue;bool clear=false;foreach(Transform b in buildings)if(Vector2.Distance(new(p.x,p.z),new(b.position.x,b.position.z))<22){clear=true;break;}
   var sight=House3-house3Road;sight.y=0;var offset=p-house3Road;offset.y=0;float st=Mathf.Clamp01(Vector3.Dot(offset,sight)/sight.sqrMagnitude);if((offset-sight*st).magnitude<16)clear=true;
   if(clear)continue;p.y=Surface(p);float h=9+(float)rng.NextDouble()*9;var key=new Vector2Int(Mathf.FloorToInt(p.x/160),Mathf.FloorToInt(p.z/160));if(!batches.TryGetValue(key,out var list))batches[key]=list=new();
   list.Add(new CombineInstance{mesh=cube,transform=Matrix4x4.TRS(p+Vector3.up*h*.7f,Quaternion.Euler(0,(float)rng.NextDouble()*180,0),new Vector3(h*.55f,h*.65f,h*.55f))});
   list.Add(new CombineInstance{mesh=cube,transform=Matrix4x4.TRS(p+Vector3.up*h*.25f,Quaternion.identity,new Vector3(1,h*.5f,1))});
   var trunk=new GameObject("Tree trunk");trunk.transform.SetParent(woods);trunk.transform.position=p+Vector3.up*h*.25f;var box=trunk.AddComponent<BoxCollider>();box.size=new(1,h*.5f,1);count++;
  }
  foreach(var entry in batches){var mesh=new Mesh{name="Forest batch",indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};mesh.CombineMeshes(entry.Value.ToArray(),true,true);mesh=SaveMesh(mesh,$"{Folder}/Forest_{entry.Key.x}_{entry.Key.y}.asset");MeshGO("Forest canopy and trunk batch",mesh,M("Woods"),woods,false);}
  var spawn=GameObject.Find("Fixed roadside respawn - north entrance").transform;var sp=route[10];sp.y=Surface(sp)+1.1f;spawn.position=sp;
  var car=Object.FindAnyObjectByType<ArcadeVehicle>();car.transform.SetPositionAndRotation(sp,spawn.rotation);PrefabUtility.RecordPrefabInstancePropertyModifications(car.transform);Object.FindAnyObjectByType<ChaseCamera>().Snap();
  EditorSceneManager.SaveScene(SceneManager.GetActiveScene());AssetDatabase.SaveAssets();
  File.WriteAllText("Docs/PHASE2_REVISION_BUILD.txt",$"Terrain: {N}x{N} at {Cell}m; 100 tiles, one shared visible/collision heightfield.\nTrees: {count}; forest renderer batches: {batches.Count}; building sites: {buildings.childCount}.\nHouse3: {House3}, ground {Surface(House3):F2}; friend: {Friend}, ground {Surface(Friend):F2}.\n");
 }
}
}







