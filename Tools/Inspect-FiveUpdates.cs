using System; using System.IO; using System.Linq; using UnityEngine; using UnityEditor; using UnityEditor.SceneManagement; using Racer; using Object=UnityEngine.Object;
public static class InspectFiveUpdates {
 public static string Main(){
  if(Application.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Saved edit mode required");
  Directory.CreateDirectory("Docs/FiveUpdates");
  EditorSceneManager.OpenScene("Assets/Scenes/StreetLoopReverse.unity");
  var race=Object.FindAnyObjectByType<RaceDirector>(); race.road.Initialize();
  var atlas=JsonUtility.FromJson<InspectRouteAtlas.Atlas>(File.ReadAllText("Docs/ForestWaterJump/routes-current.json"));
  foreach(var route in atlas.courses[0].routes.Where(r=>r.kind=="shortcut"&&r.name!="Existing Southwest Cut")){
   var end=route.points[^1]; float s=race.road.Project(end,out _); var pos=race.road.At(s-25,out var f);
   Shot(route.name.Replace(" ","-"),pos+Vector3.up*3,pos+Vector3.ProjectOnPlane(f,Vector3.up)*35+Vector3.up*2);
  }
  var home=GameObject.Find("Dan - blue X").transform;
  File.WriteAllText("Docs/FiveUpdates/property-before.txt", "Home="+home.position+" yaw="+home.eulerAngles+" scale="+home.lossyScale+"\n"+string.Join("\n",home.GetComponentsInChildren<Transform>().Where(t=>t.name.Contains("house")||t.name.Contains("garage")||t.name.Contains("Drive")||t.name.Contains("Park")).Select(t=>t.name+" local="+home.InverseTransformPoint(t.position)+" world="+t.position)));
  Shot("Property-before", home.TransformPoint(new Vector3(0,65,-5)),home.position+Vector3.forward*.1f);
  return "Candidate exit views and property inventory saved";
 }
 public static void Shot(string name,Vector3 p,Vector3 target){
  var go=new GameObject("Temporary inspection camera");var cam=go.AddComponent<Camera>();cam.CopyFrom(Camera.main);cam.enabled=false;cam.transform.SetPositionAndRotation(p,Quaternion.LookRotation(target-p));
  var rt=new RenderTexture(1100,700,24);cam.targetTexture=rt;cam.Render();var old=RenderTexture.active;RenderTexture.active=rt;var tex=new Texture2D(1100,700,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1100,700),0,0);tex.Apply();File.WriteAllBytes("Docs/FiveUpdates/"+name+".png",tex.EncodeToPNG());RenderTexture.active=old;cam.targetTexture=null;Object.DestroyImmediate(tex);Object.DestroyImmediate(rt);Object.DestroyImmediate(go);
 }
}
