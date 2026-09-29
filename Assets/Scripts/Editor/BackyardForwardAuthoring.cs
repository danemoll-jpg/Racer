using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Object=UnityEngine.Object;
namespace Racer.Editor {
public static class BackyardForwardAuthoring {
public const string Folder="Assets/Track/BackyardForward", Evidence="Docs/BackyardForward", ScenePath="Assets/Scenes/DansBackyardForward.unity";
public static readonly Vector3[] Anchors={new(463.6f,80.2f,8),new(422.7f,83.3f,9),new(409.5f,81.9f,9.3f),new(391.9f,79.9f,9.7f),new(309.2f,75.7f,15.7f),new(258.6f,68.2f,8.2f),new(96.1f,38.8f,-108.6f),new(107.7f,50.7f,60.7f),new(456.4f,81.8f,67.4f)};
static RaceRoad road,street;static Transform root;static Vector3[] path;static float[] stations;static readonly List<string> report=new();
static readonly Vector3 dumpAxis=new Vector3(-50.6f,0,-7.5f).normalized;
static Vector3[] lips;static float[] lipS,landS;static Vector3[] directions;
static float S(float a,float b,float t)=>Mathf.SmoothStep(0,1,Mathf.InverseLerp(a,b,t));
static float Ground(Vector3 p)=>Physics.RaycastAll(new Vector3(p.x,250,p.z),Vector3.down,500,1,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name.StartsWith("Ground_")||h.collider.name=="Decorative Road pavement").OrderBy(h=>h.distance).First().point.y;
static float Near(Vector3 p,Vector3[] line,out Vector3 at,out int index){float best=float.MaxValue;at=default;index=0;for(int i=1;i<line.Length;i++){var a=line[i-1];var v=line[i]-a;v.y=0;var q=p-a;q.y=0;float t=Mathf.Clamp01(Vector3.Dot(q,v)/Mathf.Max(.001f,v.sqrMagnitude));float d=(q-v*t).sqrMagnitude;if(d<best){best=d;at=Vector3.Lerp(a,line[i],t);index=i-1;}}return Mathf.Sqrt(best);}
static Mesh Store(Mesh mesh,string name){var file=Folder+"/"+name+".asset";if(AssetDatabase.LoadAssetAtPath<Mesh>(file))throw new Exception("Fresh asset required: "+file);AssetDatabase.CreateAsset(mesh,file);return mesh;}
static Vector3[] Curve(Vector3[] knots){var points=new List<Vector3>();for(int i=0;i<knots.Length;i++){var a=knots[(i+knots.Length-1)%knots.Length];var b=knots[i];var c=knots[(i+1)%knots.Length];var d=knots[(i+2)%knots.Length];int n=Mathf.CeilToInt(Vector3.Distance(b,c)/.75f);for(int j=0;j<n;j++){float t=j/(float)n;points.Add(.5f*(2*b+(-a+c)*t+(2*a-5*b+4*c-d)*t*t+(-a+3*b-3*c+d)*t*t*t));}}return points.ToArray();}
static float GullyX(float z)=>96.1f+(z+108.6f)*(11.6f/169.3f);
static float GullyDepth(Vector3 p){float width=Mathf.Lerp(19,15,S(-90,60,p.z));float cross=1-S(5,width,Mathf.Abs(p.x-GullyX(p.z)));return Mathf.Lerp(12,10,S(-80,60,p.z))*cross*S(-172,-135,p.z)*(1-S(88,130,p.z));}
static float DumpDepth(Vector3 p){var q=p-Anchors[4];float along=Vector3.Dot(q,dumpAxis),side=Mathf.Abs(Vector3.Dot(q,Vector3.Cross(Vector3.up,dumpAxis)));return 3.66f*S(1,8,along)*(1-S(39,51,along))*(1-S(11,24,side));}
static float TrailWidth(Vector3 p)=>p.x>265&&p.z>-3&&p.z<30?3.5f:2.65f;
static bool AirGap(float s)=>Enumerable.Range(0,lipS.Length).Any(i=>s>lipS[i]+.5f&&s<landS[i]-1);
static float Shape(Vector3 p,out float paint){paint=0;if((p.x>=391.9f&&p.z<42)||p.x>470||p.x<5||p.z<-175||p.z>145)return p.y;
float protectedRoad=Near(p,street.points,out _,out _);if(protectedRoad<10)return p.y;
float original=p.y,depth=Mathf.Max(GullyDepth(p),DumpDepth(p));p.y-=depth;
float d=Near(p,path,out var at,out int k);float s=stations[k];float width=TrailWidth(p);
if(d<width+2&&s>70&&s<road.Length-110&&!AirGap(s)){p.y=Mathf.Lerp(p.y,at.y,1-S(width,width+2,d));paint=1-S(width-.35f,width+.65f,d);}
// One continuous surface: the flight corridor is the hollow itself, never a deck.
if(depth>.05f)paint=Mathf.Max(paint,Mathf.Clamp01(depth/3));
return Mathf.Lerp(original,p.y,S(7,11,protectedRoad));}
public static void Build(){
if(Application.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Saved edit mode required");
if(File.Exists(ScenePath))throw new Exception("Forward scene already exists; inspect rather than rebuilding over it");
Directory.CreateDirectory(Folder);Directory.CreateDirectory(Evidence);AssetDatabase.Refresh();report.Clear();
var scene=EditorSceneManager.OpenScene(StreetLoopBuilder.ScenePath);EditorSceneManager.SaveScene(scene,ScenePath);var race=Object.FindAnyObjectByType<RaceDirector>();street=race.road;street.Initialize();
root=new GameObject("Dan's Backyard Forward - terrain trail").transform;
var oldGates=race.gates;var startTemplate=oldGates[0];var cpTemplate=oldGates[1];
var markers=GameObject.Find("TEMPORARY Backyard anchor validation - no route");if(markers)Object.DestroyImmediate(markers);
// Keep physical established shortcuts; remove only their race entitlement from this new course.
foreach(var branch in Object.FindObjectsByType<WoodlandRoute>())Object.DestroyImmediate(branch);
foreach(var f in Object.FindObjectsByType<ForestLayout>())Object.DestroyImmediate(f);
var knots=new List<Vector3>{new(470,0,7.82f),Anchors[0],new(440,0,8.57f),Anchors[1],Anchors[2],Anchors[3],new(374,0,11),new(355,0,19),Anchors[4]-dumpAxis*32,Anchors[4]-dumpAxis*18,Anchors[4],Anchors[5],Anchors[5]+dumpAxis*23,new(213,0,-15),new(236,0,-46),new(221,0,-76),new(183,0,-95),new(151,0,-108.6f),new(119,0,-108.6f),Anchors[6],new(73,0,-108.6f),new(44,0,-108.6f),new(22,0,-86),new(27,0,-47),new(47,0,-16),new(36,0,19),new(48,0,49),new(69,0,60.7f),new(91,0,60.7f),Anchors[7],new(126,0,60.7f),new(158,0,60.7f),new(186,0,80),new(224,0,112),new(269,0,123),new(304,0,103),new(325,0,72),new(362,0,62),new(398,0,80),new(428,0,83),Anchors[8],new(466,0,66)};
var join=street.Project(new Vector3(468,80,63),out _);var exit=street.Project(new Vector3(474,79,14),out _);if(exit<join)throw new Exception("Unexpected South Cherokee direction");for(float s=join;s<=exit;s+=4)knots.Add(street.At(s,out _));knots.Add(new Vector3(473,0,9));
for(int i=0;i<knots.Count;i++){var p=knots[i];p.y=Ground(p);knots[i]=p;}path=Curve(knots.ToArray());
for(int i=0;i<path.Length;i++){var p=path[i];p.y=Ground(p);path[i]=p;}
road=new GameObject("Forward navigation only - no road mesh").AddComponent<RaceRoad>();road.transform.SetParent(root);road.forestTrail=true;road.points=path;road.Initialize();
stations=new float[path.Length];for(int i=1;i<path.Length;i++)stations[i]=stations[i-1]+Vector3.Distance(path[i-1],path[i]);
lips=new[]{Anchors[4],new Vector3(119,0,-108.6f),new Vector3(91,0,60.7f)};directions=new[]{dumpAxis,Vector3.left,Vector3.right};var landings=new[]{Anchors[5],new Vector3(73,0,-108.6f),new Vector3(126,0,60.7f)};
lipS=lips.Select(p=>{p.y=Ground(p);return road.Project(p,out _);}).ToArray();landS=landings.Select(p=>{p.y=Ground(p);return road.Project(p,out _);}).ToArray();
// Smooth dirt takeoffs with flat tangency at their starts; long aligned runouts.
for(int j=0;j<lips.Length;j++){float run=j==0?30:26,h=j==0?5.2f:j==1?3.6f:2.3f;float lipY=Ground(lips[j])+h;float landingY=Ground(landings[j]);lips[j].y=lipY;
for(int i=0;i<path.Length;i++){float s=stations[i],q=s-lipS[j];if(q>=-run&&q<=0)path[i].y+=h*Mathf.Pow((q+run)/run,2);else if(s>lipS[j]&&s<=landS[j])path[i].y=Mathf.Lerp(lipY,landingY,Mathf.InverseLerp(lipS[j],landS[j],s));}}
// Four readable extra crests, keeping the parking and property entirely unchanged.
var extraLocations=new[]{new Vector3(367,0,14),new Vector3(224,0,-71),new Vector3(29,0,-46),new Vector3(285,0,115)};
var extraS=extraLocations.Select(p=>{p.y=Ground(p);return road.Project(p,out _);}).ToArray();
foreach(float crest in extraS)for(int i=0;i<path.Length;i++){float q=stations[i]-crest;if(q>=-12&&q<=0)path[i].y+=1.7f*Mathf.Pow((q+12)/12,2);else if(q>0&&q<13)path[i].y+=1.7f*(1-S(0,13,q));}
// Route vertices are now final vertical guidance. Recreate cache after shaping.
Object.DestroyImmediate(road);road=root.Find("Forward navigation only - no road mesh").gameObject.AddComponent<RaceRoad>();road.forestTrail=true;road.points=path;road.Initialize();
stations[0]=0;for(int i=1;i<path.Length;i++)stations[i]=stations[i-1]+Vector3.Distance(path[i-1],path[i]);
lipS=lips.Select(p=>road.Project(p,out _)).ToArray();landS=landings.Select(p=>{p.y=Ground(p);return road.Project(p,out _);}).ToArray();
var sourceTrees=Object.FindObjectsByType<Collider>().Where(IsTree).ToArray();var oldBases=sourceTrees.ToDictionary(c=>c,c=>Ground(c.bounds.center));
SculptTerrain();Physics.SyncTransforms();PreserveTrees(sourceTrees,oldBases);
race.ambientRoad=street;race.road=road;race.courseId="backyard-forward-v2-terrain";race.courseName="Dan's Backyard - Forward";race.reverseCourse=false;race.opponentRoster=new[]{"moto","atv","moto"};
var course=race.gameObject.AddComponent<BackyardForwardCourse>();course.anchors=Anchors;course.launchStations=lipS;course.landingStations=landS;course.flightStarts=lips;course.flightDirections=directions;
var layout=root.gameObject.AddComponent<ForestLayout>();layout.jumpStarts=lipS.Concat(extraS).ToArray();layout.jumpEnds=landS.Concat(extraS.Select(s=>s+14)).ToArray();layout.jumpNames=new[]{"Old dump","Big ravine","Ravine return","Dirt crest","Downhill kicker","West woods crest","Return hill jump"};layout.approachLead=28;
var gates=new List<RaceGate>();var gateTargets=new[]{new Vector3(459.6f,0,8.1f),new Vector3(354,0,19),new Vector3(235,0,4),new Vector3(182,0,-95),new Vector3(26,0,-47),new Vector3(155,0,60.7f),new Vector3(269,0,123),new Vector3(398,0,80),new Vector3(470,0,49)};
for(int i=0;i<gateTargets.Length;i++){var p=gateTargets[i];p.y=Ground(p);float s=road.Project(p,out _);p=road.At(s,out var f);p.y=Ground(p);var g=Object.Instantiate(i==0?startTemplate:cpTemplate,root);g.name=i==0?"Backyard START FINISH":"Backyard blue CP "+i;g.transform.SetPositionAndRotation(p+Vector3.up*1.5f,Quaternion.LookRotation(Vector3.ProjectOnPlane(f,Vector3.up)));g.halfWidth=i==0?2.6f:3.3f;g.upperHeight=18;
foreach(var col in g.GetComponentsInChildren<Collider>())Object.DestroyImmediate(col);
foreach(Transform t in g.transform){if(t.name.Contains("marking")){var lp=t.localPosition;var scale=t.localScale;if(scale.y>1){lp.x=Mathf.Sign(lp.x)*(g.halfWidth+.125f);t.localPosition=lp;}else if(scale.x>5){scale.x=g.halfWidth*2+.5f;t.localScale=scale;}else{Object.DestroyImmediate(t.gameObject);}}}
if(i==0){var black=AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/RaceBlack.mat");var white=AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/RaceWhite.mat");for(int row=0;row<2;row++)for(int col=0;col<8;col++){var square=GameObject.CreatePrimitive(PrimitiveType.Cube);square.name="Non-colliding finish paint";square.transform.SetParent(g.transform);var at=p+g.transform.right*((col-3.5f)*.6f)+g.transform.forward*(row-.5f)*.65f;at.y=Ground(at)+.02f;square.transform.SetPositionAndRotation(at,g.transform.rotation);square.transform.localScale=new(.6f,.012f,.65f);Object.DestroyImmediate(square.GetComponent<Collider>());square.GetComponent<Renderer>().sharedMaterial=(row+col)%2==0?white:black;}}
gates.Add(g);}
foreach(var g in oldGates)Object.DestroyImmediate(g.gameObject);race.gates=gates.ToArray();
var spawn=race.vehicle.GetComponent<VehicleRespawn>().spawnPoint;var startS=road.Project(gates[0].transform.position,out _);var startP=road.At(startS-7,out var startF);spawn.SetPositionAndRotation(startP+Vector3.up*.55f,Quaternion.LookRotation(Vector3.ProjectOnPlane(startF,Vector3.up)));race.vehicle.transform.SetPositionAndRotation(spawn.position,spawn.rotation);
AddDumpJunk();EditorUtility.SetDirty(race);EditorBuildSettings.scenes=EditorBuildSettings.scenes.Where(s=>!s.path.Contains("DansBackyard")).Append(new EditorBuildSettingsScene(ScenePath,true)).ToArray();PlayerSettings.bundleVersion="0.38.0-review1";
EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();report.Add($"Forward {road.Length:F1}m; 9 anchors; {gates.Count-1} blue checkpoints; 3 major + 4 extra terrain jumps; no Reverse, shortcuts or new road meshes");File.WriteAllLines(Evidence+"/authoring.txt",report);Export();
}
static void SculptTerrain(){int count=0;foreach(var mf in GameObject.Find("Memory loop - north is +Z").GetComponentsInChildren<MeshFilter>()){
if(!System.Text.RegularExpressions.Regex.IsMatch(mf.name,@"^Ground_\d+_\d+$"))continue;var bounds=mf.GetComponent<Renderer>().bounds;if(bounds.min.x>470||bounds.max.x<5||bounds.min.z>145||bounds.max.z<-175)continue;
var source=mf.sharedMesh;var v=source.vertices;var c=source.colors;var uv=source.uv;var uv2=new List<Vector4>();source.GetUVs(1,uv2);var tris=source.triangles;var verts=new List<Vector3>();var colors=new List<Color>();var tex=new List<Vector2>();var roadUv=new List<Vector4>();var indices=new List<int>();int changed=0;
for(int t=0;t<tris.Length;t+=3){int a=tris[t],b=tris[t+1],d=tris[t+2];var center=mf.transform.TransformPoint((v[a]+v[b]+v[d])/3);float distance=Near(center,path,out _,out _);bool detail=distance<10||GullyDepth(center)>.001f||DumpDepth(center)>.001f;int n=detail?4:1;
void Add(float u,float w){var local=v[a]+(v[b]-v[a])*u+(v[d]-v[a])*w;var p=mf.transform.TransformPoint(local);float y=Shape(p,out float paint);if(Mathf.Abs(p.y-y)>.001f)changed++;p.y=y;indices.Add(verts.Count);verts.Add(mf.transform.InverseTransformPoint(p));Color color=c.Length==v.Length?c[a]+(c[b]-c[a])*u+(c[d]-c[a])*w:Color.white;colors.Add(Color.Lerp(color,new Color(.38f,.255f,.135f),paint*.93f));tex.Add(uv.Length==v.Length?uv[a]+(uv[b]-uv[a])*u+(uv[d]-uv[a])*w:Vector2.zero);roadUv.Add(uv2.Count==v.Length?uv2[a]+(uv2[b]-uv2[a])*u+(uv2[d]-uv2[a])*w:Vector4.zero);}
for(int j=0;j<n;j++)for(int k=0;k<n-j;k++){Add(j/(float)n,k/(float)n);Add((j+1)/(float)n,k/(float)n);Add(j/(float)n,(k+1)/(float)n);if(j+k<n-1){Add((j+1)/(float)n,k/(float)n);Add((j+1)/(float)n,(k+1)/(float)n);Add(j/(float)n,(k+1)/(float)n);}}}
if(changed==0)continue;var mesh=new Mesh{indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};mesh.SetVertices(verts);mesh.SetColors(colors);mesh.SetUVs(0,tex);mesh.SetUVs(1,roadUv);mesh.SetTriangles(indices,0);mesh.RecalculateNormals();mesh.RecalculateBounds();mf.sharedMesh=Store(mesh,mf.name);mf.GetComponent<MeshCollider>().sharedMesh=mf.sharedMesh;count++;report.Add(mf.name+": changed "+changed+" samples, single visible/collision terrain mesh");}report.Add("Terrain copies="+count);}
static bool IsTree(Collider c)=>c.name.IndexOf("tree",StringComparison.OrdinalIgnoreCase)>=0||c.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0;
static void PreserveTrees(Collider[] trees,Dictionary<Collider,float> oldBases){
var removed=new List<Vector3>();var shifts=new List<(Vector3 p,float dy)>();int retained=0;
foreach(var tree in trees){var p=tree.bounds.center;float d=Near(p,path,out _,out _);bool cut=(p.x<391.9f||p.z>42)&&p.x<468&&p.z>-172&&p.z<145&&(d<TrailWidth(p)+.7f||Mathf.Abs(p.x-GullyX(p.z))<5&&GullyDepth(p)>1||DumpDepth(p)>2.8f&&Mathf.Abs(Vector3.Dot(p-Anchors[4],Vector3.Cross(Vector3.up,dumpAxis)))<10);
if(cut){removed.Add(p);Object.DestroyImmediate(tree.gameObject);}else{float dy=Ground(p)-oldBases[tree];if(Mathf.Abs(dy)>.025f){tree.transform.position+=Vector3.up*dy;shifts.Add((p,dy));}retained++;}}
// Edit only connected crown/trunk components belonging to affected individual trees.
int batch=0;foreach(var mf in Object.FindObjectsByType<MeshFilter>().Where(m=>m.sharedMesh&&(m.name=="Forest detail batch"||m.transform.GetComponentsInParent<Transform>().Any(t=>t.name.IndexOf("woods",StringComparison.OrdinalIgnoreCase)>=0||t.name.Contains("Forest tree")))).ToArray()){
var mesh=mf.sharedMesh;var v=mesh.vertices;var tris=mesh.triangles;var parent=Enumerable.Range(0,v.Length).ToArray();int Find(int n){while(parent[n]!=n){parent[n]=parent[parent[n]];n=parent[n];}return n;}void Union(int a,int b){parent[Find(a)]=Find(b);}
var weld=new Dictionary<Vector3Int,int>();for(int i=0;i<v.Length;i++){var key=Vector3Int.RoundToInt(v[i]*1000);if(weld.TryGetValue(key,out int other))Union(i,other);else weld[key]=i;}for(int i=0;i<tris.Length;i+=3){Union(tris[i],tris[i+1]);Union(tris[i],tris[i+2]);}
var groups=new Dictionary<int,Bounds>();for(int i=0;i<v.Length;i++){int id=Find(i);if(!groups.TryGetValue(id,out var b))b=new Bounds(v[i],Vector3.zero);b.Encapsulate(v[i]);groups[id]=b;}
var cuts=new HashSet<int>();var moves=new Dictionary<int,float>();foreach(var pair in groups){var p=mf.transform.TransformPoint(pair.Value.center);if(removed.Any(t=>Vector2.Distance(new(t.x,t.z),new(p.x,p.z))<1.8f)){cuts.Add(pair.Key);continue;}var near=shifts.OrderBy(t=>Vector2.Distance(new(t.p.x,t.p.z),new(p.x,p.z))).FirstOrDefault();if(shifts.Count>0&&Vector2.Distance(new(near.p.x,near.p.z),new(p.x,p.z))<1.8f)moves[pair.Key]=near.dy;}
if(cuts.Count==0&&moves.Count==0)continue;var kept=new List<int>();for(int i=0;i<tris.Length;i+=3)if(!cuts.Contains(Find(tris[i])))kept.AddRange(new[]{tris[i],tris[i+1],tris[i+2]});for(int i=0;i<v.Length;i++)if(moves.TryGetValue(Find(i),out var dy))v[i]+=mf.transform.InverseTransformVector(Vector3.up*dy);var copy=Object.Instantiate(mesh);copy.vertices=v;copy.SetTriangles(kept,0);copy.RecalculateBounds();mf.sharedMesh=Store(copy,"Trees-"+batch++);}
report.Add($"Trees retained={retained}; individually removed={removed.Count}; grounded={shifts.Count}; existing batch copies edited={batch}; normal usable trail 5.3m wide");}
static void AddDumpJunk(){var mat=new Material(Shader.Find("Universal Render Pipeline/Lit")){color=new Color(.27f,.20f,.13f)};AssetDatabase.CreateAsset(mat,Folder+"/Old rust.mat");for(int i=0;i<9;i++){var p=Anchors[4]+dumpAxis*(13+i*3)+Vector3.Cross(Vector3.up,dumpAxis)*(i%2==0?9:-10);p.y=Ground(p);var g=GameObject.CreatePrimitive(i%3==0?PrimitiveType.Cylinder:PrimitiveType.Cube);g.name=i%3==0?"Old dump rusted drum":"Old dump discarded timber";g.transform.SetParent(root);g.transform.position=p+Vector3.up*.25f;g.transform.rotation=Quaternion.Euler(i%3==0?83:4,i*39,8);g.transform.localScale=i%3==0?new Vector3(.8f,.55f,.8f):new Vector3(1.9f,.35f,.6f);g.GetComponent<Renderer>().sharedMaterial=mat;Object.DestroyImmediate(g.GetComponent<Collider>());}}
public static void Export(){var r=Object.FindAnyObjectByType<RaceDirector>();var data=new{r.courseName,route=r.road.points,anchors=Anchors,gates=r.gates.Select(g=>new{g.name,position=g.transform.position,g.halfWidth}),flights=r.GetComponent<BackyardForwardCourse>(),gully=new{south=new Vector3(GullyX(-172),0,-172),north=new Vector3(GullyX(130),0,130),depth=12,firstWidth=38,secondWidth=30},dump=new{launch=Anchors[4],landing=Anchors[5],depression=3.66f}};
// Components themselves are not serialized: export only their authored data.
File.WriteAllText(Evidence+"/geometry.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{data.courseName,data.route,data.anchors,data.gates,data.gully,data.dump,launchStations=data.flights.launchStations,landingStations=data.flights.landingStations},new Newtonsoft.Json.JsonSerializerSettings{ContractResolver=new ForwardJsonResolver()}));}
}}
