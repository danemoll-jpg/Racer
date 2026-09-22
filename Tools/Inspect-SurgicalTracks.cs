UnityEditor.EditorApplication.delayCall += () => { try {
var rows=new List<string>();
foreach(var name in new[]{"StreetLoopGreybox","StreetLoopReverse","MountainLoopReverse"}) {
 UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/"+name+".unity");Physics.SyncTransforms();
 var race=UnityEngine.Object.FindAnyObjectByType<Racer.RaceDirector>(); var road=race.road;road.Initialize();
 rows.Add("SCENE "+name);
 if(name.StartsWith("Street")) {
  foreach(var mf in UnityEngine.Object.FindObjectsByType<MeshFilter>().Where(m=>m.GetComponent<Renderer>() && m.GetComponent<Renderer>().bounds.Intersects(new Bounds(new Vector3(495,75,-150),new Vector3(220,100,260))) && m.GetComponent<Collider>())) rows.Add($"MESH {mf.name}: {AssetDatabase.GetAssetPath(mf.sharedMesh)} bounds={mf.GetComponent<Renderer>().bounds}");
  float station=road.Project(new Vector3(500,88,-101),out _);rows.Add("station="+station);
  for(float s=station-150;s<station+150;s+=5){var p=road.At(s,out _);var hits=Physics.RaycastAll(p+Vector3.up*5,Vector3.down,100,1,QueryTriggerInteraction.Ignore).Where(h=>!h.rigidbody).OrderBy(h=>h.distance).Take(3);rows.Add($"ROAD {s:F2} {p}: "+string.Join(";",hits.Select(h=>$"{h.collider.name} {h.point.y:F3}")));}
 } else {
  var arrows=UnityEngine.Object.FindObjectsByType<MeshFilter>().Where(m=>m.name=="Main teal trail arrow").OrderBy(m=>road.Project(m.GetComponent<Renderer>().bounds.center,out _)).Take(4);
  foreach(var a in arrows){var p=a.GetComponent<Renderer>().bounds.center;float s=road.Project(p,out _);rows.Add($"ARROW {s} {p} colliders={a.GetComponentsInChildren<Collider>(true).Length}");}
  foreach(float lane in new[]{-4f,0,4}) {Vector3 prior=Vector3.up;float py=0;for(float s=10;s<250;s+=.25f){var p=road.At(s,out var f)+Vector3.Cross(Vector3.up,f).normalized*lane;var hits=Physics.RaycastAll(p+Vector3.up*5,Vector3.down,15,1,QueryTriggerInteraction.Ignore).Where(h=>!h.rigidbody).OrderBy(h=>h.distance).ToArray();if(hits.Length==0){rows.Add($"GAP {s} lane={lane}");continue;}var h=hits[0];float angle=Vector3.Angle(prior,h.normal);if(s>10&&(angle>3||Math.Abs(h.point.y-py)>.2f||hits.Count(x=>Math.Abs(x.point.y-h.point.y)<.15f)>1)) rows.Add($"SURFACE {s} lane={lane} step={h.point.y-py:F4} angle={angle:F3}: "+string.Join(";",hits.Select(x=>$"{x.collider.name} y={x.point.y:F4} n={x.normal}")));prior=h.normal;py=h.point.y;}}
 }
}
System.IO.Directory.CreateDirectory("Docs/SurgicalTracks");System.IO.File.WriteAllLines("Docs/SurgicalTracks/before.txt",rows);


} catch(System.Exception e) { System.IO.Directory.CreateDirectory("Docs/SurgicalTracks");System.IO.File.WriteAllText("Docs/SurgicalTracks/inspection-error.txt",e.ToString()); }}; return "Scheduled structural inspection";
