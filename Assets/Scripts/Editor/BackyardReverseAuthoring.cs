using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Object=UnityEngine.Object;
namespace Racer.Editor {
public static partial class BackyardReverseAuthoring {
public const string Folder="Assets/Track/BackyardReverse", Evidence="Docs/BackyardReverse";
static int asset;
static readonly List<string> report=new();
static object P(Vector3 p)=>new{x=p.x,y=p.y,z=p.z};
static float Smooth(float a,float b,float t)=>Mathf.SmoothStep(0,1,Mathf.InverseLerp(a,b,t));
public static float Ground(Vector3 p)=>Physics.RaycastAll(new(p.x,500,p.z),Vector3.down,1000,1,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name.StartsWith("Ground_")||h.collider.name=="Decorative Road pavement").OrderBy(h=>h.distance).First().point.y;
static float Terrain(Vector3 p)=>Physics.RaycastAll(new(p.x,500,p.z),Vector3.down,1000,1,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name.StartsWith("Ground_")).OrderBy(h=>h.distance).First().point.y;
static Mesh Store(Mesh m,string name){string file=AssetDatabase.GenerateUniqueAssetPath(Folder+"/"+UnityEngine.SceneManagement.SceneManager.GetActiveScene().name+"-"+name+"-"+(asset++)+".asset");AssetDatabase.CreateAsset(m,file);return m;}
static bool Tree(Collider c)=>c.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0||c.name.IndexOf("tree",StringComparison.OrdinalIgnoreCase)>=0;
static float Distance(Vector3 a,Vector3 b)=>Vector2.Distance(new(a.x,a.z),new(b.x,b.z));
// All authoring callers test local distances <= 10m. Index expanded segment bounds
// so unrelated terrain never scans the entire kilometre-long course.
static readonly Dictionary<Vector3[],Dictionary<Vector2Int,List<int>>> localSegments=new();
static float Near(Vector3 p,Vector3[] line,out Vector3 at,out int segment){
if(!localSegments.TryGetValue(line,out var cells)){cells=new();for(int i=1;i<line.Length;i++){int x0=Mathf.FloorToInt((Mathf.Min(line[i-1].x,line[i].x)-12)/16),x1=Mathf.FloorToInt((Mathf.Max(line[i-1].x,line[i].x)+12)/16),z0=Mathf.FloorToInt((Mathf.Min(line[i-1].z,line[i].z)-12)/16),z1=Mathf.FloorToInt((Mathf.Max(line[i-1].z,line[i].z)+12)/16);for(int x=x0;x<=x1;x++)for(int z=z0;z<=z1;z++){var key=new Vector2Int(x,z);if(!cells.TryGetValue(key,out var list))cells[key]=list=new();list.Add(i);}}localSegments[line]=cells;}
float best=float.MaxValue;at=default;segment=0;if(!cells.TryGetValue(new(Mathf.FloorToInt(p.x/16),Mathf.FloorToInt(p.z/16)),out var nearby))return best;
foreach(int i in nearby){var v=line[i]-line[i-1];v.y=0;var q=p-line[i-1];q.y=0;float t=Mathf.Clamp01(Vector3.Dot(q,v)/Mathf.Max(.001f,v.sqrMagnitude));float d=(q-v*t).sqrMagnitude;if(d<best){best=d;at=Vector3.Lerp(line[i-1],line[i],t);segment=i-1;}}return Mathf.Sqrt(best);}
static void Save(){var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();}
// Copy only changed meshes. Shared originals (including the approved Forward assets) stay intact.
static void Sculpt(Func<Vector3,bool> region,Func<Vector3,Vector3> shape,string label){
foreach(var mf in Object.FindObjectsByType<MeshFilter>().Where(m=>m.name.StartsWith("Ground_")&&m.GetComponent<MeshCollider>()).ToArray()){
var src=mf.sharedMesh;var vs=src.vertices;var ts=src.triangles;var cs=src.colors;var uv=src.uv;var uv1=new List<Vector4>();src.GetUVs(1,uv1);
bool candidate=vs.Any(v=>region(mf.transform.TransformPoint(v)));if(!candidate)continue;
var verts=new List<Vector3>();var cols=new List<Color>();var tex=new List<Vector2>();var tex1=new List<Vector4>();var indices=new List<int>();int changed=0;
for(int t=0;t<ts.Length;t+=3){int a=ts[t],b=ts[t+1],c=ts[t+2];var pa=mf.transform.TransformPoint(vs[a]);var pb=mf.transform.TransformPoint(vs[b]);var pc=mf.transform.TransformPoint(vs[c]);bool inside=region(pa)||region(pb)||region(pc)||region((pa+pb+pc)/3);int n=inside?Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(Vector3.Distance(pa,pb),Vector3.Distance(pa,pc))),1,8):1;
void Add(float u,float v){var p=pa+(pb-pa)*u+(pc-pa)*v;var original=p;if(region(p))p=shape(p);if(Mathf.Abs(p.y-original.y)>.0001f)changed++;indices.Add(verts.Count);verts.Add(mf.transform.InverseTransformPoint(p));cols.Add(cs.Length==vs.Length?cs[a]+(cs[b]-cs[a])*u+(cs[c]-cs[a])*v:Color.white);tex.Add(uv.Length==vs.Length?uv[a]+(uv[b]-uv[a])*u+(uv[c]-uv[a])*v:Vector2.zero);tex1.Add(uv1.Count==vs.Length?uv1[a]+(uv1[b]-uv1[a])*u+(uv1[c]-uv1[a])*v:Vector4.zero);}
for(int j=0;j<n;j++)for(int k=0;k<n-j;k++){Add(j/(float)n,k/(float)n);Add((j+1)/(float)n,k/(float)n);Add(j/(float)n,(k+1)/(float)n);if(j+k<n-1){Add((j+1)/(float)n,k/(float)n);Add((j+1)/(float)n,(k+1)/(float)n);Add(j/(float)n,(k+1)/(float)n);}}}
if(changed==0)continue;var m=new Mesh{indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};m.SetVertices(verts);m.SetColors(cols);m.SetUVs(0,tex);m.SetUVs(1,tex1);m.SetTriangles(indices,0);m.RecalculateNormals();m.RecalculateBounds();mf.sharedMesh=Store(m,label);mf.GetComponent<MeshCollider>().sharedMesh=m;report.Add(label+" "+mf.name+" changed samples "+changed);
}Physics.SyncTransforms();}
// Match connected render components to trunk axes, including the separate crown in combined meshes.
static void Trees(Func<Collider,bool> remove,Func<Collider,bool> seat){
var originalTreePositions=Object.FindObjectsByType<Collider>().Where(Tree).Select(c=>c.bounds.center).ToArray();var changes=new List<(Vector3 p,float dy,bool cut)>();foreach(var c in Object.FindObjectsByType<Collider>().Where(Tree).ToArray()){
var p=c.bounds.center;bool cut=remove(c);float dy=seat(c)?Terrain(p)-c.bounds.min.y:0;if(!cut&&Mathf.Abs(dy)<.02f)continue;changes.Add((p,dy,cut));if(cut)Object.DestroyImmediate(c.gameObject);else c.transform.position+=Vector3.up*dy;}
if(changes.Count==0)return;
int groupsEdited=0;foreach(var mf in Object.FindObjectsByType<MeshFilter>().Where(m=>m.sharedMesh&&m.GetComponent<Renderer>()&&(m.name.Contains("batch")||m.name.Contains("canopy")||m.name.Contains("Crown")||m.GetComponentsInParent<Transform>().Any(t=>t.name.IndexOf("woods",StringComparison.OrdinalIgnoreCase)>=0||t.name.Contains("Forest tree")))).ToArray()){
var bound=mf.GetComponent<Renderer>().bounds;if(!changes.Any(c=>c.p.x>bound.min.x-2&&c.p.x<bound.max.x+2&&c.p.z>bound.min.z-2&&c.p.z<bound.max.z+2))continue;
var mesh=mf.sharedMesh;var v=mesh.vertices;var tri=mesh.triangles;var parents=Enumerable.Range(0,v.Length).ToArray();int Find(int i){while(parents[i]!=i){parents[i]=parents[parents[i]];i=parents[i];}return i;}void Join(int a,int b){parents[Find(a)]=Find(b);}
var weld=new Dictionary<Vector3Int,int>();for(int i=0;i<v.Length;i++){var key=Vector3Int.RoundToInt(v[i]*1000);if(weld.TryGetValue(key,out int other))Join(i,other);else weld[key]=i;}for(int i=0;i<tri.Length;i+=3){Join(tri[i],tri[i+1]);Join(tri[i],tri[i+2]);}
var bounds=new Dictionary<int,Bounds>();for(int i=0;i<v.Length;i++){int id=Find(i);if(!bounds.TryGetValue(id,out var b))b=new Bounds(v[i],Vector3.zero);b.Encapsulate(v[i]);bounds[id]=b;}
var cutGroups=new HashSet<int>();var moves=new Dictionary<int,float>();foreach(var pair in bounds){var center=mf.transform.TransformPoint(pair.Value.center);var owner=originalTreePositions.OrderBy(p=>Distance(p,center)).First();if(Distance(owner,center)>6)continue;var matches=changes.Where(c=>Vector3.Distance(c.p,owner)<.001f).ToArray();if(matches.Length==0)continue;var match=matches[0];if(match.cut)cutGroups.Add(pair.Key);else moves[pair.Key]=match.dy;}
if(cutGroups.Count+moves.Count==0)continue;groupsEdited+=cutGroups.Count+moves.Count;var keep=new List<int>();for(int i=0;i<tri.Length;i+=3)if(!cutGroups.Contains(Find(tri[i])))keep.AddRange(new[]{tri[i],tri[i+1],tri[i+2]});for(int i=0;i<v.Length;i++)if(moves.TryGetValue(Find(i),out float dy))v[i]+=mf.transform.InverseTransformVector(Vector3.up*dy);var copy=Object.Instantiate(mesh);copy.vertices=v;copy.triangles=keep.ToArray();copy.RecalculateBounds();mf.sharedMesh=Store(copy,"trees");if(mf.GetComponent<MeshCollider>())mf.GetComponent<MeshCollider>().sharedMesh=copy;
}Physics.SyncTransforms();report.Add($"Complete trees removed={changes.Count(c=>c.cut)}, grounded={changes.Count(c=>!c.cut)}, render components={groupsEdited}");}
static void Signs(){int count=0;var mats=new Dictionary<Material,Material>();foreach(var t in Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include).Where(t=>!SceneryText.IsFloating(t))){var r=t.GetComponent<Renderer>();var old=r.sharedMaterial;if(!old)continue;if(!mats.TryGetValue(old,out var m)){m=new Material(old){shader=Shader.Find("Racer/SummitText"),name="Physical depth tested lettering"};m.renderQueue=3000;AssetDatabase.CreateAsset(m,Folder+"/letters-"+(asset++)+".mat");mats[old]=m;}r.sharedMaterial=m;EditorUtility.SetDirty(r);PrefabUtility.RecordPrefabInstancePropertyModifications(r);count++;}report.Add("Physical lettering with depth testing and back-face culling="+count);}
public static void World(){
if(Application.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Saved edit mode required");Directory.CreateDirectory(Folder);Directory.CreateDirectory(Evidence);AssetDatabase.Refresh();report.Clear();
EditorSceneManager.OpenScene("Assets/Scenes/ForestLoopReverse.unity");var water=PrefabUtility.SaveAsPrefabAsset(GameObject.Find("House 3 pool and lake"),Folder+"/Permanent McFadden water.prefab");
try{foreach(var name in RacePlaylists.Scenes.Where(n=>n!="DansBackyardReverse")){
File.AppendAllText(Evidence+"/world-progress.txt",name+" starting\n");EditorSceneManager.OpenScene("Assets/Scenes/"+name+".unity");Physics.SyncTransforms();report.Add("SCENE "+name);
var house=GameObject.Find("Original house 2").transform;var old=house.position;var target=new Vector3(440,0,-82);target.y=Terrain(target);var houseBounds=house.GetComponentsInChildren<Collider>().Select(c=>c.bounds).ToArray();house.position=target;Physics.SyncTransforms();
// Supported footprint only, fading into Roger's existing lawn. Public road and boundaries stay put.
var supports=house.GetComponentsInChildren<Collider>().Where(c=>c.name.Contains("Foundation")||c.name.Contains("Wall")||c.name.Contains("Body")).Select(c=>c.bounds).ToArray();if(supports.Length==0)supports=house.GetComponentsInChildren<Collider>().Select(c=>c.bounds).ToArray();var footprint=supports[0];foreach(var b in supports)footprint.Encapsulate(b);float floor=supports.Min(b=>b.min.y);
Sculpt(p=>Math.Abs(p.x-target.x)<21&&Math.Abs(p.z-target.z)<23,p=>{float dx=Math.Max(0,Math.Max(footprint.min.x-p.x,p.x-footprint.max.x));float dz=Math.Max(0,Math.Max(footprint.min.z-p.z,p.z-footprint.max.z));p.y=Mathf.Lerp(p.y,floor,1-Smooth(0,5,Math.Max(dx,dz)));return p;},"roger-support");
var map=Object.FindAnyObjectByType<ExplorationMap>();foreach(var d in map.destinations.Where(d=>d.id=="property-roger")){d.position=target+house.forward*18;d.position.y=Ground(d.position);d.yaw=house.eulerAngles.y;}EditorUtility.SetDirty(map);
var collection=Object.FindAnyObjectByType<ExplorationCollection>();foreach(var s in collection.sites.Where(s=>s.title=="Roger's approach")){var delta=target-old;s.position+=delta;s.position.y=Ground(s.position)+.7f;s.access+=delta;s.access.y=Ground(s.access);}EditorUtility.SetDirty(collection);
report.Add("Roger existing house "+old+" -> "+target+"; local footprint support floor="+floor);
var existing=GameObject.Find("House 3 pool and lake");if(!existing){existing=Object.Instantiate(water);existing.name="House 3 pool and lake";existing.hideFlags=HideFlags.None;UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(existing,UnityEngine.SceneManagement.SceneManager.GetActiveScene());}
// Preserve the accepted water footprint/elevation; close the floating rim with a local basin and shell.
Sculpt(p=>p.x>360&&p.x<430&&p.z>-211&&p.z<-178,p=>{float pool=Math.Max(Math.Abs(p.x-416)/9.6f,Math.Abs(p.z+197)/8.6f);if(pool<1.25f){float y=pool<.92f?33.35f:34.65f;p.y=Mathf.Lerp(p.y,y,1-Smooth(1,1.25f,pool));}float lake=Mathf.Sqrt(Mathf.Pow((p.x-384)/19,2)+Mathf.Pow((p.z+194.1f)/11.5f,2));if(lake<1.2f){float y=lake<.90f?32.85f:33.4f;p.y=Mathf.Lerp(p.y,y,1-Smooth(1,1.2f,lake));}return p;},"permanent-water-support");
foreach(var edge in existing.GetComponentsInChildren<Transform>().Where(t=>t.name=="Pool edge").ToArray()){if(!edge.GetComponent<BoxCollider>())edge.gameObject.AddComponent<BoxCollider>();var shell=GameObject.CreatePrimitive(PrimitiveType.Cube);shell.name="Supported pool shell";shell.transform.SetParent(existing.transform);var size=edge.localScale;size.y=1.45f;shell.transform.localScale=size;shell.transform.position=new(edge.position.x,33.975f,edge.position.z);shell.GetComponent<Renderer>().sharedMaterial=edge.GetComponent<Renderer>().sharedMaterial;}
bool Water(Vector3 p)=>Math.Abs(p.x-416)<10.7f&&Math.Abs(p.z+197)<9.7f||Mathf.Pow((p.x-384)/20.5f,2)+Mathf.Pow((p.z+194.1f)/13,2)<1;
var rampStart=new Vector3(334.05f,0,-212);var axis=new Vector3(.35f,0,1).normalized;bool Ramp(Collider c){if(name!="StreetLoopReverse")return false;var q=c.bounds.center-rampStart;float along=Vector3.Dot(q,axis),side=Math.Abs(Vector3.Dot(q,Vector3.Cross(Vector3.up,axis)));return along>-42&&along<100&&side<10&&c.bounds.max.y>55;}
Trees(c=>Water(c.bounds.center)||Distance(c.bounds.center,target)<15||Ramp(c),c=>Distance(c.bounds.center,new(370.3f,0,12.7f))<65||Distance(c.bounds.center,target)<28||(c.bounds.center.x>360&&c.bounds.center.x<430&&c.bounds.center.z>-211&&c.bounds.center.z<-178));
Signs();Save();File.AppendAllText(Evidence+"/world-progress.txt",name+" saved\n");}
File.WriteAllLines(Evidence+"/world-changes.txt",report);
}finally{EditorSceneManager.OpenScene(BackyardForwardAuthoring.ScenePath);}
}
}
}
