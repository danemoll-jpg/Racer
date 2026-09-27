EditorApplication.delayCall += () => {try {
var refs=Newtonsoft.Json.Linq.JObject.Parse(System.IO.File.ReadAllText("Docs/RouteAtlas/baseline-mesh-references.json"));var report=new List<string>();
foreach(var scene in new[]{"StreetLoopGreybox","LakeWoods","ForestLoopReverse","MountainLoop","MountainLoopReverse"}){
 UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();int samples=0,edits=0;float max=0;var roads=UnityEngine.Object.FindObjectsByType<Racer.RaceRoad>().Where(r=>!r.name.Contains("House 3")).SelectMany(r=>r.points).Concat(UnityEngine.Object.FindObjectsByType<Racer.WoodlandRoute>().SelectMany(b=>b.points)).ToArray();
 foreach(var mf in UnityEngine.Object.FindObjectsByType<MeshFilter>().Where(m=>m.sharedMesh&&AssetDatabase.GetAssetPath(m.sharedMesh).Contains("-property-Ground_"))){
  string id=GlobalObjectId.GetGlobalObjectIdSlow(mf).targetObjectId.ToString();string guid=(string)refs[scene][id];if(guid==null)throw new Exception("Original mesh reference missing "+id);var baseline=AssetDatabase.LoadAssetAtPath<Mesh>(AssetDatabase.GUIDToAssetPath(guid));var before=baseline.vertices;var after=mf.sharedMesh.vertices;
  if(before.Length!=after.Length)throw new Exception("Terrain topology changed");
  for(int i=0;i<before.Length;i++)if(before[i]!=after[i]){if(before[i].x!=after[i].x||before[i].z!=after[i].z)throw new Exception("Terrain horizontal geometry changed");edits++;}
  var go=new GameObject("Temporary atlas baseline collider");go.layer=31;go.transform.SetPositionAndRotation(mf.transform.position,mf.transform.rotation);go.transform.localScale=mf.transform.lossyScale;var old=go.AddComponent<MeshCollider>();old.sharedMesh=baseline;var now=mf.GetComponent<MeshCollider>();Physics.SyncTransforms();
  try{foreach(var p in roads.Where(p=>p.x>420&&p.x<545&&p.z> -195&&p.z< -105))foreach(var offset in new[]{Vector3.zero,Vector3.right*4,Vector3.left*4,Vector3.forward*4,Vector3.back*4}){var q=p+offset;var ray=new Ray(new Vector3(q.x,200,q.z),Vector3.down);bool a=old.Raycast(ray,out var ah,260),b=now.Raycast(ray,out var bh,260);if(a!=b)throw new Exception("Race-road support disappeared");if(a){samples++;max=Math.Max(max,Math.Abs(ah.point.y-bh.point.y));}}}finally{UnityEngine.Object.DestroyImmediate(go);}
 }
 if(max>.001f)throw new Exception(scene+" race-road support changed "+max);
 report.Add($"{scene}: {edits} vertical-only property edits; {samples} race-road/branch support comparisons including 4m margins; maximum before/after difference {max:F6}m. House and sign transforms retained.");
}
System.IO.File.WriteAllLines("Docs/RouteAtlas/terrain-preservation.txt",report);
}catch(Exception e){System.IO.File.WriteAllText("Docs/RouteAtlas/terrain-error.txt",e.ToString());}};return "Scheduled local race-road support invariance check";
