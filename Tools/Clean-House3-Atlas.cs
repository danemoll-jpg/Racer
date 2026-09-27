EditorApplication.delayCall += () => {try {
var report=new List<string>();
foreach(var sceneName in new[]{"StreetLoopGreybox","LakeWoods","ForestLoopReverse","MountainLoop","MountainLoopReverse"}) {
 UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/"+sceneName+".unity");Physics.SyncTransforms();
 var house=GameObject.Find("Original house 3").transform;var housePosition=house.position;
 var drive=GameObject.Find("House 3 valley driveway").GetComponent<Racer.RaceRoad>();var surface=drive.GetComponentsInChildren<MeshFilter>().Single(m=>m.name=="Ground_House3 supported valley driveway");
 var start=drive.points[0];var end=drive.points[^1];var axis=end-start;axis.y=0;float length=axis.magnitude;var forward=axis.normalized;var right=Vector3.Cross(Vector3.up,forward);
 var roads=UnityEngine.Object.FindObjectsByType<Racer.RaceRoad>().Where(r=>r!=drive).SelectMany(r=>r.points).Where(p=>p.x>start.x-160&&p.x<start.x+70&&p.z>start.z-130&&p.z<start.z+100).ToArray();
 var branches=UnityEngine.Object.FindObjectsByType<Racer.WoodlandRoute>().SelectMany(b=>b.points).Where(p=>p.x>start.x-160&&p.x<start.x+70&&p.z>start.z-130&&p.z<start.z+100).ToArray();var protectedPoints=roads.Concat(branches).ToArray();
 float RoadDistance(Vector3 p){float d=float.MaxValue;foreach(var q in protectedPoints){float dx=p.x-q.x,dz=p.z-q.z;d=Math.Min(d,dx*dx+dz*dz);}return Mathf.Sqrt(d);}
 float Station(Vector3 p)=>Vector3.Dot(new Vector3(p.x-start.x,0,p.z-start.z),forward);
 float Near(Vector3 p){float s=Mathf.Clamp(Station(p),0,length);var q=start+forward*s;return new Vector2(p.x-q.x,p.z-q.z).magnitude;}
 float Ground(Vector3 p){var hits=Physics.RaycastAll(new Vector3(p.x,200,p.z),Vector3.down,260,~0,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name.StartsWith("Ground")||h.collider.name.Contains("pavement")).OrderBy(h=>Math.Abs(h.point.y-p.y)).ToArray();if(hits.Length==0)throw new Exception("Missing property support at "+p);return hits[0].point.y;}
 var line=new List<Vector3>();int rows=Mathf.CeilToInt(length/2)+1;
 for(int i=0;i<rows;i++){var p=Vector3.Lerp(start,end,i/(float)(rows-1));float d=RoadDistance(p);if(d<24)p.y=Mathf.Lerp(Ground(p),p.y,Mathf.SmoothStep(0,1,Mathf.InverseLerp(15,24,d)));line.Add(p);}
 // First keep road-side driveway triangles exactly as authored. Their road overlap is protected.
 var old=surface.sharedMesh;var oldV=old.vertices;var oldT=old.triangles;var keep=new List<int>();
 for(int i=0;i<oldT.Length;i+=3)if(Enumerable.Range(0,3).Any(j=>RoadDistance(surface.transform.TransformPoint(oldV[oldT[i+j]]))<15)){keep.Add(oldT[i]);keep.Add(oldT[i+1]);keep.Add(oldT[i+2]);}
 var retained=UnityEngine.Object.Instantiate(old);retained.name=sceneName+" preserved race-road driveway overlaps";retained.triangles=keep.ToArray();retained.RecalculateBounds();AssetDatabase.CreateAsset(retained,"Assets/Track/RouteAtlas/"+sceneName+"-retained-driveway-overlaps.asset");surface.sharedMesh=retained;surface.GetComponent<MeshCollider>().sharedMesh=retained;
 int changed=0;var changedPaths=new List<string>();
 foreach(var mf in UnityEngine.Object.FindObjectsByType<MeshFilter>().Where(m=>System.Text.RegularExpressions.Regex.IsMatch(m.name,@"^Ground_\d+_\d+$")&&m.GetComponent<MeshCollider>()).ToArray()){
  var collider=mf.GetComponent<MeshCollider>();var bounds=collider.bounds;if(!bounds.Intersects(new Bounds((start+end)*.5f,new Vector3(Math.Abs(start.x-end.x)+24,200,Math.Abs(start.z-end.z)+24))))continue;
  var mesh=UnityEngine.Object.Instantiate(mf.sharedMesh);var vertices=mesh.vertices;int edits=0;
  for(int i=0;i<vertices.Length;i++){var p=mf.transform.TransformPoint(vertices[i]);float s=Station(p),d=Near(p);if(s<0||s>length||d>=10||RoadDistance(p)<=15)continue;
   float t=s/length;float target=Vector3.Lerp(start,end,t).y;float rd=RoadDistance(p);float weight=(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(4,10,d)))*Mathf.SmoothStep(0,1,Mathf.InverseLerp(15,24,rd));float y=Mathf.Lerp(p.y,target,weight);if(Math.Abs(y-p.y)<.00001f)continue;p.y=y;vertices[i]=mf.transform.InverseTransformPoint(p);edits++;
  }
  if(edits==0){UnityEngine.Object.DestroyImmediate(mesh);continue;}mesh.vertices=vertices;mesh.RecalculateNormals();mesh.RecalculateBounds();string path="Assets/Track/RouteAtlas/"+sceneName+"-property-"+mf.name+".asset";AssetDatabase.CreateAsset(mesh,path);mf.sharedMesh=mesh;collider.sharedMesh=mesh;changed+=edits;changedPaths.Add(path);
 }
 Physics.SyncTransforms();
 var v=new List<Vector3>();var triangles=new List<int>();
 for(int i=0;i<line.Count;i++)for(int j=0;j<9;j++){var p=line[i]+right*(j-4)*.75f;p.y=Ground(p)+.035f;v.Add(p);if(i<line.Count-1&&j<8&&RoadDistance(line[i])>=15){int n=i*9+j;triangles.AddRange(new[]{n,n+9,n+1,n+1,n+9,n+10});}}
 var ribbon=new Mesh{vertices=v.ToArray(),triangles=triangles.ToArray()};ribbon.RecalculateNormals();ribbon.RecalculateBounds();AssetDatabase.CreateAsset(ribbon,"Assets/Track/RouteAtlas/"+sceneName+"-straight-house3-drive.asset");
 var go=new GameObject("Ground_House3 straight descending property driveway",typeof(MeshFilter),typeof(MeshRenderer),typeof(MeshCollider));go.transform.SetParent(drive.transform);go.GetComponent<MeshFilter>().sharedMesh=ribbon;go.GetComponent<MeshRenderer>().sharedMaterial=surface.GetComponent<MeshRenderer>().sharedMaterial;go.GetComponent<MeshCollider>().sharedMesh=ribbon;
 drive.points=line.ToArray();drive.openHighway=true;EditorUtility.SetDirty(drive);Physics.SyncTransforms();
 if(house.position!=housePosition)throw new Exception("House moved");
 var slopes=new List<float>();float lastY=float.PositiveInfinity;int increases=0;for(int i=0;i<line.Count;i++){var p=line[i];p.y=Ground(p);if(i>0&&p.y>lastY+.4f)increases++;lastY=p.y;}
 UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(drive.gameObject.scene);UnityEditor.SceneManagement.EditorSceneManager.SaveScene(drive.gameObject.scene);AssetDatabase.SaveAssets();
 report.Add($"{sceneName}: straight property driveway {start} -> {end}; {length:F1}m plan length; {start.y-end.y:F1}m descent. House pose unchanged. {changed} terrain vertices changed within 10m of straight driveway and more than 15m from every authored road/branch. Existing road-overlap driveway triangles preserved exactly. Centre support rises >0.4m between 2m samples: {increases}. New path has no race gates/branch entitlement.");
}
report.Add("StreetLoopReverse: House 3 cleanup stopped entirely at Laurel protection boundary. The direct road-to-house line crosses the existing Laurel route, so its driveway, terrain and colliders remain unchanged.");
System.IO.File.WriteAllLines("Docs/RouteAtlas/house3-cleanup.txt",report);
}catch(Exception e){System.IO.File.WriteAllText("Docs/RouteAtlas/house3-error.txt",e.ToString());}};return "Scheduled property-only cleanup outside race corridors; Laurel scene excluded";
