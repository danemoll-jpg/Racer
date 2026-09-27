// One-off local authoring, executed in the connected Editor. No runtime systems added.
if(Application.isPlaying||EditorApplication.isCompiling||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty||UnityEngine.SceneManagement.SceneManager.GetActiveScene().name!="ForestLoopReverse")throw new Exception("Saved Forest Reverse edit mode required");
const string folder="Assets/Track/ForestWaterJump", evidence="Docs/ForestWaterJump";
System.IO.Directory.CreateDirectory(folder);System.IO.Directory.CreateDirectory(evidence);
if(GameObject.Find("House 3 pool and lake"))throw new Exception("Already authored");
var race=UnityEngine.Object.FindAnyObjectByType<Racer.RaceDirector>();var road=race.road;
var originalPoints=road.points.ToArray();var gatePositions=race.gates.Select(g=>g.transform.position).ToArray();
var branches=UnityEngine.Object.FindObjectsByType<Racer.WoodlandRoute>();var branchEnds=branches.Select(b=>new[]{road.At(b.entryRoad,out _),road.At(b.exitRoad,out _)}).ToArray();
var layout=UnityEngine.Object.FindAnyObjectByType<Racer.ForestLayout>();var jumpEnds=layout.jumpStarts.Select((s,i)=>new[]{road.At(s,out _),road.At(layout.jumpEnds[i],out _)}).ToArray();
var mf=GameObject.Find("Ground_560_240").GetComponent<MeshFilter>();var collider=mf.GetComponent<MeshCollider>();var originalMesh=mf.sharedMesh;
var start=new Vector3(474,0,-201.3f);var axis=new Vector3(-1,0,.08f).normalized;var side=Vector3.Cross(Vector3.up,axis);const float length=40;
float Ground(Vector3 p){if(!collider.Raycast(new Ray(p+Vector3.up*200,Vector3.down),out var h,400))throw new Exception("Missing ground");return h.point.y;}
float Smooth(float x){x=Mathf.Clamp01(x);return x*x*(3-2*x);}
float height=Ground(start);float grade=(Ground(start+axis)-Ground(start-axis))*.5f;
var lanes=Enumerable.Range(0,81).Select(i=>Ground(start+side*(i*.25f-10))-height).ToArray();
float Profile(float s,float lateral){float k=Mathf.Clamp((lateral+10)*4,0,79.999f);float bank=Mathf.Lerp(lanes[(int)k],lanes[(int)k+1],k-(int)k);return height+grade*s+(.40f-grade)*s*s/(2*length)+bank*(1-Smooth(s/14));}
// The two disconnected obsolete driveway pieces match the screenshot positions.
// Keep the third component at the public-road mouth exactly as saved.
var oldDrive=GameObject.Find("Ground_House3 supported valley driveway").GetComponent<MeshFilter>();var dm=oldDrive.sharedMesh;var dv=dm.vertices;var dt=dm.triangles;var keep=new List<int>();int removed=0;
for(int i=0;i<dt.Length;i+=3){var centre=(oldDrive.transform.TransformPoint(dv[dt[i]])+oldDrive.transform.TransformPoint(dv[dt[i+1]])+oldDrive.transform.TransformPoint(dv[dt[i+2]]))/3;if(centre.z< -175&&centre.x<480){removed++;continue;}keep.AddRange(new[]{dt[i],dt[i+1],dt[i+2]});}
var retained=UnityEngine.Object.Instantiate(dm);retained.name="House 3 retained road mouth only";retained.triangles=keep.ToArray();retained.RecalculateBounds();AssetDatabase.CreateAsset(retained,folder+"/driveway-road-mouth.asset");oldDrive.sharedMesh=retained;oldDrive.GetComponent<MeshCollider>().sharedMesh=retained;
// One metre authored terrain resolution: preserve X/Z, triangle topology and all outside vertices.
// The ramp is a straight extension of the incoming road; its far edge falls into the existing valley.
var mesh=UnityEngine.Object.Instantiate(originalMesh);var vertices=mesh.vertices;int rampEdits=0,poolEdits=0;
var pool=new Vector3(416,34.65f,-197);var lake=new Vector3(384,33.35f,-194.1f);
for(int i=0;i<vertices.Length;i++){
 var p=mf.transform.TransformPoint(vertices[i]);var d=p-start;d.y=0;float s=Vector3.Dot(d,axis),t=Vector3.Dot(d,side);float y=p.y;
 if(s>0&&s<=length&&Math.Abs(t)<10){float w=(1-Smooth((Math.Abs(t)-5)/5))*Smooth(s/4);y=Mathf.Lerp(y,Profile(s,t),w);rampEdits++;}
 // A modest shallow basin below the pool; no terrain changes outside this water footprint.
 float poolD=Math.Max(Math.Abs(p.x-pool.x)/9,Math.Abs(p.z-pool.z)/8);
 if(poolD<1.18f){float w=1-Smooth((poolD-.88f)/.30f);y=Mathf.Lerp(y,33.35f,w);poolEdits++;}
 if(Math.Abs(y-p.y)>.00001f){p.y=y;vertices[i]=mf.transform.InverseTransformPoint(p);}
}
mesh.vertices=vertices;mesh.RecalculateNormals();mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,folder+"/Forest-ramp-pool-terrain.asset");mf.sharedMesh=mesh;collider.sharedMesh=mesh;Physics.SyncTransforms();
var root=new GameObject("House 3 pool and lake").transform;
Material Mat(string name,Color color){var m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.name=name;m.color=color;AssetDatabase.CreateAsset(m,folder+"/"+name+".mat");return m;}
var poolMat=Mat("Pool turquoise",new Color(.10f,.64f,.76f));var lakeMat=Mat("Lake blue green",new Color(.12f,.37f,.43f));var rimMat=Mat("Pool pale coping",new Color(.80f,.79f,.69f));
GameObject Box(string name,Vector3 p,Vector3 size,Material material){var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(root);go.transform.position=p;go.transform.localScale=size;go.GetComponent<Renderer>().sharedMaterial=material;UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());return go;}
var water=Box("House 3 swimming pool",pool,new Vector3(18,.08f,16),poolMat);water.AddComponent<Racer.ShallowWater>();
foreach(int sign in new[]{-1,1}){Box("Pool edge",pool+new Vector3(sign*9.3f,.12f,0),new Vector3(.6f,.24f,17.2f),rimMat);Box("Pool edge",pool+new Vector3(0,.12f,sign*8.3f),new Vector3(18,.24f,.6f),rimMat);}
var lakeGo=GameObject.CreatePrimitive(PrimitiveType.Cylinder);lakeGo.name="House 3 lake";lakeGo.transform.SetParent(root);lakeGo.transform.position=lake;lakeGo.transform.localScale=new Vector3(38,.04f,23);lakeGo.GetComponent<Renderer>().sharedMaterial=lakeMat;UnityEngine.Object.DestroyImmediate(lakeGo.GetComponent<Collider>());lakeGo.AddComponent<Racer.ShallowWater>().round=true;
// Main route X/Z is invariant. Update only support heights through the local ramp.
for(int i=0;i<road.points.Length;i++){var p=road.points[i];var d=p-start;d.y=0;float s=Vector3.Dot(d,axis);if(s>=0&&s<=length&&Math.Abs(Vector3.Dot(d,side))<10){p.y=Ground(p);road.points[i]=p;}}
typeof(Racer.RaceRoad).GetField("distance",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).SetValue(road,null);road.Initialize();
for(int i=0;i<branches.Length;i++){branches[i].entryRoad=road.Project(branchEnds[i][0],out _);branches[i].exitRoad=road.Project(branchEnds[i][1],out _);EditorUtility.SetDirty(branches[i]);}
layout.jumpStarts=jumpEnds.Select(p=>road.Project(p[0],out _)).Concat(new[]{road.Project(new Vector3(start.x,height,start.z),out _)}).ToArray();layout.jumpEnds=jumpEnds.Select(p=>road.Project(p[1],out _)).Concat(new[]{road.Project(new Vector3(270,46,-195),out _)}).ToArray();layout.jumpNames=layout.jumpNames.Concat(new[]{"House 3 pool and lake flight"}).ToArray();
EditorUtility.SetDirty(road);EditorUtility.SetDirty(layout);
var lip=start+axis*length;lip.y=Profile(length,0);
System.IO.File.WriteAllText(evidence+"/geometry.json",Newtonsoft.Json.JsonConvert.SerializeObject(new {checkpoint="480a279fc23afd0d1bee0a3c218f4a0ac6ecb5c9",start=new[]{start.x,height,start.z},axis=new[]{axis.x,axis.y,axis.z},lip=new[]{lip.x,lip.y,lip.z},length,grade,launchGrade=.40f,pool=new[]{pool.x,pool.y,pool.z},poolSize=new[]{18,16},lake=new[]{lake.x,lake.y,lake.z},lakeSize=new[]{38,23},removedDrivewayTriangles=removed,rampEdits,poolEdits,originalTerrain=AssetDatabase.GetAssetPath(originalMesh)},Newtonsoft.Json.Formatting.Indented));
if(!originalPoints.Select(p=>new Vector2(p.x,p.z)).SequenceEqual(road.points.Select(p=>new Vector2(p.x,p.z))))throw new Exception("Main alignment changed");
if(!gatePositions.SequenceEqual(race.gates.Select(g=>g.transform.position)))throw new Exception("Gate positions changed");
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(race.gameObject.scene);UnityEditor.SceneManagement.EditorSceneManager.SaveScene(race.gameObject.scene);AssetDatabase.SaveAssets();return $"Local ramp/water authored; removed {removed} triangles from two orphan driveway pieces. Main X/Z and gates invariant. Lip {lip}";
