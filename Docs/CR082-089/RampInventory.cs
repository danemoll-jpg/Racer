if(UnityEditor.EditorApplication.isPlaying || UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty) throw new System.Exception("Saved edit mode required");
var initial=UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
foreach(var scene in Racer.Editor.ReverseReviewRelease.Scenes)
{
 UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scene);
 var race=UnityEngine.Object.FindAnyObjectByType<Racer.RaceDirector>();
 var frame=UnityEngine.GameObject.Find("Phase 4 - Connector Jump").transform;
 var rows=new System.Collections.Generic.List<string>{"Scene="+scene,"Ramp frame="+frame.position+" rotation="+frame.rotation};
 foreach(var collider in UnityEngine.Object.FindObjectsByType<UnityEngine.Collider>(UnityEngine.FindObjectsInactive.Include,UnityEngine.FindObjectsSortMode.None))
 {
  var local=frame.InverseTransformPoint(collider.transform.position);
  var mesh=collider as UnityEngine.MeshCollider;
  var bounds=collider.bounds;
  var center=frame.InverseTransformPoint(bounds.center);
  if(collider.name.Contains("roadworks")||collider.name.StartsWith("Takeoff")||collider.name.Contains("shoulder")||(System.Math.Abs(center.x)<35&&center.z>-65&&center.z<170))
  {
   string path=collider.name;for(var parent=collider.transform.parent;parent!=null;parent=parent.parent)path=parent.name+"/"+path;
   rows.Add("COLLIDER "+path+" enabled="+collider.enabled+" active="+collider.gameObject.activeInHierarchy+" trigger="+collider.isTrigger+" bounds="+bounds+" mesh="+(mesh?UnityEditor.AssetDatabase.GetAssetPath(mesh.sharedMesh):"box/other")+" readable="+(mesh&&mesh.sharedMesh?mesh.sharedMesh.isReadable:false));
  }
 }
 for(float z=-40;z<=130;z+=2)
 foreach(float x in new[]{-4.7f,-3f,-1.5f,0f,1.7f})
 {
  var start=frame.TransformPoint(new UnityEngine.Vector3(x,50,z));
  foreach(var hit in UnityEngine.Physics.RaycastAll(start,UnityEngine.Vector3.down,80,~0,UnityEngine.QueryTriggerInteraction.Collide))
   rows.Add("SUPPORT x="+x+" z="+z+" hit="+hit.collider.name+" y="+hit.point.y.ToString("F4")+" normal="+hit.normal.ToString("F4")+" face="+hit.triangleIndex+" trigger="+hit.collider.isTrigger);
 }
 System.IO.File.WriteAllLines("Docs/CR082-089/inventory-"+race.gameObject.scene.name+".txt",rows);
}
UnityEditor.SceneManagement.EditorSceneManager.OpenScene(initial);
return "Baseline collider and support inventories saved; no scenes changed";
