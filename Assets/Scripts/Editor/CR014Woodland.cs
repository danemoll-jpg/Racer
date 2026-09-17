using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;
namespace Racer.Editor {
public static class CR014Woodland {
 const string Woods="Woods replacing later subdivisions", Prefix="CR014 trunk ";
 [Serializable] public class Placement { public Vector3 position; public float height; }
 [Serializable] public class Layout { public Placement[] trees; }
 static readonly Dictionary<Vector2Int,List<Vector3>> bins=new();
 static Vector2Int Key(Vector3 p)=>new(Mathf.FloorToInt(p.x/8),Mathf.FloorToInt(p.z/8));
 static void Add(Vector3 p){var k=Key(p);if(!bins.TryGetValue(k,out var a))bins[k]=a=new();a.Add(p);}
 static bool Near(Vector3 p,float radius){var k=Key(p);int n=Mathf.CeilToInt(radius/8);for(int z=-n;z<=n;z++)for(int x=-n;x<=n;x++)if(bins.TryGetValue(k+new Vector2Int(x,z),out var a))foreach(var q in a)if(new Vector2(p.x-q.x,p.z-q.z).sqrMagnitude<radius*radius)return true;return false;}
 static bool Ground(Vector3 p,out Vector3 surface){surface=p;foreach(var h in Physics.RaycastAll(new Vector3(p.x,350,p.z),Vector3.down,500,1,QueryTriggerInteraction.Ignore).OrderBy(h=>h.distance))if(h.collider.transform.root.name=="Memory loop - north is +Z"){surface=h.point;return h.normal.y>.8f;}return false;}
 public static bool SiteReserved(Vector3 p){
  var root=GameObject.Find(Phase6Review.Root);if(root)foreach(Transform t in root.transform){var d=p-t.position;d.y=0;if(d.magnitude<28)return true;}
  // Retain House 3's accepted downhill view/access across its sloped site.
  var h3=CR015Neighborhood.AuthoredPosition("Original house 3",new Vector3(429,0,-220));var current=GameObject.Find("Original house 3");if(current)h3=current.transform.position;h3.y=0;var road=StreetLoopBuilder.Route();StreetLoopBuilder.Nearest(h3,road,out var r);r.y=0;var v=h3-r;p.y=0;return (p-r-v*Mathf.Clamp01(Vector3.Dot(p-r,v)/v.sqrMagnitude)).magnitude<18;
 }
 public static void Plan(){
  if(Application.isPlaying||SceneManager.GetActiveScene().isDirty)throw new Exception("Saved scene outside Play required");Physics.SyncTransforms();bins.Clear();
  if(GameObject.Find(Woods).GetComponentsInChildren<BoxCollider>().Any(c=>c.name.StartsWith(Prefix)))throw new Exception("CR-014 layout already applied. Refresh visuals from authored trunks; do not overwrite its placement plan.");
  foreach(var c in GameObject.Find(Woods).GetComponentsInChildren<BoxCollider>())Add(c.bounds.center);
  var road=StreetLoopBuilder.Route();var cut=Phase5Setup.Path();var rng=new System.Random(1401978);var result=new List<Placement>();
  for(float z=-660;z<610;z+=8.5f)for(float x=-730;x<750;x+=8.5f){
   var p=new Vector3(x+((float)rng.NextDouble()-.5f)*6,0,z+((float)rng.NextDouble()-.5f)*6);
   // Broad connected remembered woodland, softened by two scales of irregular edges/glades.
   bool central=p.x>-565&&p.x<350&&p.z>-460&&p.z<440;
   bool east=p.x>470&&p.z>-620&&p.z<500;
   bool south=p.z<-440&&p.x>-540&&p.x<700;
   if(!central&&!east&&!south)continue;
   float patch=Mathf.PerlinNoise((p.x+1800)/145,(p.z+1800)/145),edge=Mathf.PerlinNoise((p.x+313)/37,(p.z+917)/37);
   if(patch<.29f||edge<.17f||Near(p,6.8f)||Phase6Vegetation.Reserved(p,road,cut)||SiteReserved(p))continue;
   if(!Ground(p,out p))continue;Add(p);result.Add(new Placement{position=p,height=15+(float)rng.NextDouble()*5});
  }
  // Deterministic shuffle makes half-stage representative of every region.
  result=result.OrderBy(_=>rng.Next()).ToList();Directory.CreateDirectory("Docs/CR014");File.WriteAllText("Docs/CR014/layout.json",JsonUtility.ToJson(new Layout{trees=result.ToArray()},true));
  var routes=new List<WoodlandBenchmark.Route>();int i=Phase5Setup.Closest(road,new Vector3(470,0,30));routes.Add(new(){name="wooded-road",points=Enumerable.Range(0,220).Select(j=>road[(i+j)%road.Count]).ToArray(),speed=18});
  routes.Add(new(){name="central-forest",points=Path(new(-300,0,260),new(-100,0,200)),speed=7});
  routes.Add(new(){name="southwest-forest",points=Path(new(-430,0,-200),new(-260,0,-270)),speed=7});
  routes.Add(new(){name="east-forest",points=Path(new(590,0,100),new(650,0,-90)),speed=7});
  routes.Add(new(){name="dense-view",points=new[]{routes[1].points[0],routes[1].points[20]},speed=0,fixedView=true});
  File.WriteAllText("Docs/CR014/routes.json",JsonUtility.ToJson(new WoodlandBenchmark.Plan{routes=routes.ToArray()},true));
  File.WriteAllText("Docs/CR014/plan.txt",$"Existing 6102; proposed additions {result.Count}; minimum added trunk center spacing 6.8m (including existing trees). Added boxes 0.75m wide. Broad central, eastern and southern woodland; patch noise preserves glades. Plan routes avoid all proposed trunks; routes are test trajectories, not authored clearings or shortcuts. Terrain sampled from accepted live MeshColliders.\n");
 }
 static Vector3[] Path(Vector3 from,Vector3 to){
  const int n=111;const float step=3;var origin=(from+to)*.5f-new Vector3(n*step*.5f,0,n*step*.5f);
  Vector3 Point(int k)=>origin+new Vector3(k%n*step,0,k/n*step);
  int Cell(Vector3 p)=>Mathf.Clamp(Mathf.RoundToInt((p.z-origin.z)/step),0,n-1)*n+Mathf.Clamp(Mathf.RoundToInt((p.x-origin.x)/step),0,n-1);
  bool[] valid=new bool[n*n];Vector3[] surface=new Vector3[n*n];for(int k=0;k<valid.Length;k++)valid[k]=!Near(Point(k),3.0f)&&Ground(Point(k),out surface[k]);
  int Nearest(int k)=>Enumerable.Range(0,n*n).Where(j=>valid[j]).OrderBy(j=>(Point(k)-Point(j)).sqrMagnitude).First();
  int start=Nearest(Cell(from)),end=Nearest(Cell(to));var cost=Enumerable.Repeat(float.MaxValue,n*n).ToArray();var prev=Enumerable.Repeat(-1,n*n).ToArray();var open=new SortedSet<(float,int)>();cost[start]=0;open.Add((0,start));
  while(open.Count>0){var item=open.Min;open.Remove(item);int k=item.Item2;if(k==end)break;foreach(int dz in new[]{-1,0,1})foreach(int dx in new[]{-1,0,1}){int x=k%n+dx,z=k/n+dz;if(x<0||x>=n||z<0||z>=n||(dx==0&&dz==0))continue;int j=z*n+x;if(!valid[j]||!valid[k+dx]||!valid[k+dz*n]||Near((Point(k)+Point(j))*.5f,3))continue;float c=cost[k]+Vector3.Distance(surface[k],surface[j]);if(c>=cost[j])continue;open.Remove((cost[j],j));cost[j]=c;prev[j]=k;open.Add((c,j));}}
  if(prev[end]<0)throw new Exception("No connected forest route "+from);var indices=new List<int>();for(int k=end;k!=-1;k=prev[k])indices.Add(k);indices.Reverse();
  // Resample at ~2m for the same follower lookahead used on roads.
  var path=new List<Vector3>();for(int j=0;j<indices.Count-1;j++){var a=surface[indices[j]];var b=surface[indices[j+1]];int count=Mathf.CeilToInt(Vector3.Distance(a,b)/2);for(int k=0;k<count;k++)path.Add(Vector3.Lerp(a,b,k/(float)count));}path.Add(surface[end]);return path.ToArray();
 }
 public static void Apply(int stage){
  var scene=SceneManager.GetActiveScene();if(Application.isPlaying||scene.isDirty||scene.path!=StreetLoopBuilder.ScenePath)throw new Exception("Saved StreetLoopGreybox outside Play required");
  var layout=JsonUtility.FromJson<Layout>(File.ReadAllText("Docs/CR014/layout.json"));var woods=GameObject.Find(Woods).transform;
  int count=stage==1?layout.trees.Length/2:layout.trees.Length;
  var names=new HashSet<string>(woods.Cast<Transform>().Select(t=>t.name));for(int i=0;i<count;i++){string name=Prefix+i.ToString("D5");if(names.Contains(name))continue;var p=layout.trees[i];var go=new GameObject(name);go.transform.SetParent(woods);go.transform.position=p.position+Vector3.up*p.height*.25f;go.AddComponent<BoxCollider>().size=new Vector3(.75f,p.height*.5f,.75f);}
  EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);Phase6Vegetation.Refresh($"Docs/CR014/vegetation-stage{stage}.txt");
  File.WriteAllText($"Docs/CR014/stage{stage}.txt",$"Added {count}; total {woods.GetComponentsInChildren<BoxCollider>().Length}; render batches {woods.GetComponentsInChildren<MeshFilter>().Length}; triangles {woods.GetComponentsInChildren<MeshFilter>().Sum(f=>f.sharedMesh.triangles.Length/3)}\n");
 }
 public static void Capture(string label){
  var cam=Camera.main;var p=cam.transform.position;var r=cam.transform.rotation;var chase=cam.GetComponent<ChaseCamera>();bool ce=chase.enabled;bool ortho=cam.orthographic;float os=cam.orthographicSize;
  var plan=JsonUtility.FromJson<WoodlandBenchmark.Plan>(File.ReadAllText("Docs/CR014/routes.json"));
  try{chase.enabled=false;for(int i=0;i<6;i++){
   if(i==0){cam.orthographic=true;cam.orthographicSize=790;cam.transform.SetPositionAndRotation(new Vector3(0,1000,0),Quaternion.Euler(90,0,0));}
   else if(i==5){cam.orthographic=true;cam.orthographicSize=155;cam.transform.SetPositionAndRotation(new Vector3(389,500,53),Quaternion.Euler(90,0,0));}
   else{cam.orthographic=false;var path=plan.routes[i-1].points;cam.transform.SetPositionAndRotation(path[0]+Vector3.up*3.5f,Quaternion.LookRotation(path[15]-path[0]));}
   var rt=new RenderTexture(1440,900,24);var old=cam.targetTexture;var active=RenderTexture.active;try{cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;var tex=new Texture2D(1440,900,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1440,900),0,0);tex.Apply();File.WriteAllBytes($"Docs/CR014/{label}-{i}.png",tex.EncodeToPNG());Object.DestroyImmediate(tex);}finally{cam.targetTexture=old;RenderTexture.active=active;Object.DestroyImmediate(rt);}
  }}finally{cam.transform.SetPositionAndRotation(p,r);cam.orthographic=ortho;cam.orthographicSize=os;chase.enabled=ce;}
 }
 public static void Build(string label){
  // URP 17.6 calls Create even on inactive features; stripped inactive SSAO resources
  // otherwise crash the Player renderer. Exclude only inactive feature references
  // for this benchmark build, then restore the accepted renderer configuration.
  var renderer=AssetDatabase.LoadAssetAtPath<UnityEngine.Rendering.Universal.UniversalRendererData>("Assets/Settings/PC_Renderer.asset");
  var features=renderer.rendererFeatures.ToArray();int removed=renderer.rendererFeatures.RemoveAll(f=>f&&!f.isActive);
  try{EditorUtility.SetDirty(renderer);AssetDatabase.SaveAssetIfDirty(renderer);
   var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{StreetLoopBuilder.ScenePath},locationPathName=$"Builds/CR014-{label}/Racer.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
   File.WriteAllText($"Docs/CR014/build-{label}.txt",$"{result.summary.result}; errors={result.summary.totalErrors}; warnings={result.summary.totalWarnings}; bytes={result.summary.totalSize}; duration={result.summary.totalTime}; inactive renderer features omitted for benchmark={removed}; restored after build.\n");
  }finally{renderer.rendererFeatures.Clear();renderer.rendererFeatures.AddRange(features);EditorUtility.SetDirty(renderer);AssetDatabase.SaveAssetIfDirty(renderer);}
 }
 public static void Coverage(string label){
  // Exact horizontal triangle footprints sampled on a fixed 2m grid, not tree-count inference.
  const int n=800;const float step=2;var covered=new bool[n*n];
  float Cross(Vector2 a,Vector2 b)=>a.x*b.y-a.y*b.x;
  foreach(var f in GameObject.Find(Woods).GetComponentsInChildren<MeshFilter>()){
   var vertices=f.sharedMesh.vertices.Select(v=>f.transform.TransformPoint(v)).Select(v=>new Vector2((v.x+800)/step,(v.z+750)/step)).ToArray();var t=f.sharedMesh.triangles;
   for(int i=0;i<t.Length;i+=3){var a=vertices[t[i]];var b=vertices[t[i+1]];var c=vertices[t[i+2]];float area=Cross(b-a,c-a);if(Mathf.Abs(area)<.0001f)continue;
    int x0=Mathf.Clamp(Mathf.FloorToInt(Mathf.Min(a.x,Mathf.Min(b.x,c.x))),0,n-1),x1=Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(a.x,Mathf.Max(b.x,c.x))),0,n-1);
    int z0=Mathf.Clamp(Mathf.FloorToInt(Mathf.Min(a.y,Mathf.Min(b.y,c.y))),0,n-1),z1=Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(a.y,Mathf.Max(b.y,c.y))),0,n-1);
    for(int z=z0;z<=z1;z++)for(int x=x0;x<=x1;x++){var p=new Vector2(x+.5f,z+.5f);float u=Cross(b-p,c-p)/area,v=Cross(c-p,a-p)/area;if(u>=0&&v>=0&&u+v<=1)covered[z*n+x]=true;}
   }
  }
  var lines=new List<string>();void Region(string name,float x0,float x1,float z0,float z1){int total=0,count=0;for(int z=0;z<n;z++)for(int x=0;x<n;x++){float px=-800+(x+.5f)*step,pz=-750+(z+.5f)*step;if(px<x0||px>x1||pz<z0||pz>z1)continue;total++;if(covered[z*n+x])count++;}lines.Add($"{name}: projected tree footprint {count*4}m2 / {total*4}m2 = {100.0*count/total:F2}%");}
  Region("whole supported terrain",-800,800,-750,850);Region("central removed subdivisions",-565,350,-460,440);Region("eastern woods",470,750,-620,500);Region("southern woods",-540,700,-660,-440);
  File.WriteAllLines($"Docs/CR014/coverage-{label}.txt",lines);
 }
 public static void Validate(){
  Physics.SyncTransforms();var boxes=GameObject.Find(Woods).GetComponentsInChildren<BoxCollider>();var log=new List<string>();int fail=0;
  void Check(bool ok,string message){log.Add((ok?"PASS ":"FAIL ")+message);if(!ok)fail++;}
  var road=StreetLoopBuilder.Route();var cut=Phase5Setup.Path();int yard=0,blocked=0;float rd=999,cd=999;bins.Clear();foreach(var c in boxes)if(!c.name.StartsWith(Prefix))Add(c.bounds.center);
  int tooNear=0,unsupported=0,reserved=0;foreach(var c in boxes){var p=c.bounds.center;if(Phase6Buildings.YardClear(p))yard++;rd=Mathf.Min(rd,StreetLoopBuilder.Nearest(p,road,out _)-.71f);cd=Mathf.Min(cd,Phase5Setup.Distance(p,cut,out _)-.71f);
   if(c.name.StartsWith(Prefix)){if(Near(p,6.79f))tooNear++;Add(p);if(!Ground(p,out var ground)||Mathf.Abs(ground.y-c.bounds.min.y)>.02f)unsupported++;if(Phase6Vegetation.Reserved(p,road,cut))reserved++;}}
  Check(yard==0,$"Expanded yard trunks={yard}");Check(tooNear==0,$"Added trunks violating 6.8m spacing={tooNear}");Check(unsupported==0,$"Added trunks not aligned with supported ground={unsupported}");Check(reserved==0,$"Added trees in reserved sites/corridors={reserved}");Check(rd>=16&&cd>=7.6f,$"Minimum conservative trunk surface clearance road={rd:F2}m shortcut={cd:F2}m");
  int probes=0;foreach(var path in new[]{road,cut})for(int i=0;i<path.Count;i+=5){var side=Vector3.Cross(Vector3.up,path[Mathf.Min(i+1,path.Count-1)]-path[Math.Max(0,i-1)]).normalized;foreach(float offset in new[]{-9f,0,9f}){probes++;foreach(var c in Physics.OverlapBox(path[i]+side*offset+Vector3.up*1.5f,new Vector3(.8f,1.2f,.8f),Quaternion.identity,1,QueryTriggerInteraction.Ignore))if(c.transform.root.name==Woods)blocked++;}}
  Check(blocked==0,$"Road/shoulder/shortcut forest obstacles={blocked}, vehicle-volume probes={probes}");
  Check(boxes.Count(c=>!c.name.StartsWith(Prefix))==6102,"All original 6102 trunks retained (full transform verification separate)");
  log.Add($"RESULT failures={fail}; colliders always enabled, no distance activation or pass-through trees. No terrain modifications.");File.WriteAllLines("Docs/CR014/clearances.txt",log);
 }
 public static void ExplorationPlan(){
  bins.Clear();foreach(var c in GameObject.Find(Woods).GetComponentsInChildren<BoxCollider>())Add(c.bounds.center);
  var source=JsonUtility.FromJson<WoodlandBenchmark.Plan>(File.ReadAllText("Docs/CR014/routes.json"));var routes=new List<WoodlandBenchmark.Route>();
  foreach(var route in source.routes.Skip(1).Take(3)){
   var p=route.points.ToArray();for(int pass=0;pass<30;pass++)for(int i=1;i<p.Length-1;i++){
    var q=Vector3.Lerp(p[i],(p[i-1]+p[i+1])*.5f,.4f);if(!Near(q,2.5f)&&Ground(q,out q))p[i]=q;
   }
   routes.Add(new(){name=route.name+"-slow",points=p,speed=3.5f,lookAhead=5});
  }
  File.WriteAllText("Docs/CR014/exploration-routes.json",JsonUtility.ToJson(new WoodlandBenchmark.Plan{routes=routes.ToArray(),seconds=40},true));
 }
}
}

