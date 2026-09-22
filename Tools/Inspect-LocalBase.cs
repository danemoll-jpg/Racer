UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/StreetLoopReverse.unity");
var rows=new List<string>();
var probes=new[]{new Vector3(251,49,-290),new Vector3(300,55,-260),new Vector3(345,65,-230),new Vector3(390,74,-210),new Vector3(440,80,-190),new Vector3(480,85,-170),new Vector3(491,87,-140)};
foreach(var tile in new[]{"Ground_480_160","Ground_480_240","Ground_560_160","Ground_560_240","Ground_640_240"}){
 var source=GameObject.Find(tile);foreach(var folder in new[]{"StreetLoop/","ReverseReview/Street-","Exploration/StreetLoopReverse-course-protected-","Discovery/StreetLoopReverse-cr101-property-"}){
 var path="Assets/Track/"+folder+tile+".asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(!mesh)continue;
 var go=new GameObject("Temporary baseline probe");go.transform.SetPositionAndRotation(source.transform.position,source.transform.rotation);go.transform.localScale=source.transform.lossyScale;var c=go.AddComponent<MeshCollider>();c.sharedMesh=mesh;
 foreach(var p in probes)if(c.Raycast(new Ray(p+Vector3.up*100,Vector3.down),out var h,200))rows.Add($"{path} xz=({p.x},{p.z}) y={h.point.y:F2}");
 UnityEngine.Object.DestroyImmediate(go);
 }}
return string.Join("\n",rows);
