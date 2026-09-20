if(UnityEditor.EditorApplication.isPlaying || UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty) throw new System.Exception("Saved edit mode required");
var initial=UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
foreach(var path in Racer.Editor.ReverseReviewRelease.Scenes)
{
 UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path);
 var race=UnityEngine.Object.FindAnyObjectByType<Racer.RaceDirector>();var road=race.road;road.Initialize();
 var spawn=race.vehicle.GetComponent<Racer.VehicleRespawn>().spawnPoint;
 float origin=road.Project(race.gates[0].transform.position,out _);
 var rows=new System.Collections.Generic.List<string>{"origin="+origin+" roadLength="+road.Length,"spawn="+spawn.position+" forward="+spawn.forward+" station="+road.Project(spawn.position,out _),"road0="+road.points[0]};
 for(int i=0;i<race.gates.Length;i++)
 {
  var g=race.gates[i];float station=road.Project(g.transform.position,out _);
  rows.Add("GATE "+i+" name="+g.name+" station="+station+" relative="+road.Relative(station,origin)+" position="+g.transform.position+" forward="+g.transform.forward+" width="+g.halfWidth+" scale="+g.transform.lossyScale);
  if(i==0)foreach(UnityEngine.Transform child in g.transform)rows.Add("GATE VISUAL "+child.name+" local="+child.localPosition+" size="+child.localScale);
 }
 foreach(var b in UnityEngine.Object.FindObjectsByType<Racer.WoodlandRoute>(UnityEngine.FindObjectsSortMode.None))
 {
  rows.Add("BRANCH "+b.title+" entry="+b.entryRoad+" exit="+b.exitRoad+" inset="+b.entryInset+" entrypos="+b.points[0]+" exitpos="+b.points[b.points.Length-1]+" bypass="+string.Join(",",b.bypassedGates));
  if(race.reverseCourse)Racer.LivingWorldValidation.Capture("Docs/CR082-089/before-"+race.gameObject.scene.name+"-"+b.title.Replace(' ','-')+".png",road.At(b.entryRoad-24,out var f)+UnityEngine.Vector3.up*2.6f,road.At(b.entryRoad+15,out _)+UnityEngine.Vector3.up*1.5f);
 }
 foreach(var f in UnityEngine.Object.FindObjectsByType<UnityEngine.MeshFilter>(UnityEngine.FindObjectsSortMode.None))
 {
  var r=f.GetComponent<UnityEngine.Renderer>();if(r&&r.sharedMaterial&&UnityEditor.AssetDatabase.GetAssetPath(r.sharedMaterial).Contains("Vegetation"))rows.Add("TREE BATCH "+f.name+" mesh="+UnityEditor.AssetDatabase.GetAssetPath(f.sharedMesh)+" vertices="+f.sharedMesh.vertexCount);
 }
 System.IO.File.WriteAllLines("Docs/CR082-089/route-before-"+race.gameObject.scene.name+".txt",rows);
 if(race.reverseCourse)Racer.LivingWorldValidation.Capture("Docs/CR082-089/before-"+race.gameObject.scene.name+"-grid.png",spawn.position-spawn.forward*6+UnityEngine.Vector3.up*3,spawn.position+spawn.forward*25+UnityEngine.Vector3.up);
}
UnityEditor.SceneManagement.EditorSceneManager.OpenScene(initial);
return "Saved route inventories and initial approach views; no scene edits";
