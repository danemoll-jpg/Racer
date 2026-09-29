EditorApplication.delayCall+=()=>{try{
if(Application.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Saved edit mode required");
var original=UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/DansBackyardForward.unity");
var race=UnityEngine.Object.FindAnyObjectByType<Racer.RaceDirector>();
var roads=UnityEngine.Object.FindObjectsByType<Racer.RaceRoad>(FindObjectsSortMode.None);
System.IO.File.WriteAllText("Docs/BackyardForward/map-scene.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{
 scene="Assets/Scenes/DansBackyardForward.unity",route=race.road.points.Select(p=>new {x=p.x,y=p.y,z=p.z}),
 branches=UnityEngine.Object.FindObjectsByType<Racer.WoodlandRoute>().Select(b=>new{b.title,b.entryRoad,b.exitRoad,b.entryInset,b.bypassedGates,points=b.points.Select(p=>new{x=p.x,y=p.y,z=p.z})}), roads=roads.Select(r=>new{name=r.name,trail=r.forestTrail,points=r.points.Select(p=>new{x=p.x,y=p.y,z=p.z})}),
 gates=UnityEngine.Object.FindObjectsByType<Racer.RaceGate>(FindObjectsSortMode.None).Select(g=>new{name=g.name,x=g.transform.position.x,y=g.transform.position.y,z=g.transform.position.z}),
 landmarks=UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Where(t=>t.name.Contains("Dan")||t.name.Contains("Kyle")||t.name.Contains("Cherokee")||t.name.Contains("ravine")).Select(t=>new{name=t.name,x=t.position.x,z=t.position.z})
}));
var go=new GameObject("Temporary documentation camera");go.hideFlags=HideFlags.HideAndDontSave;
var cam=go.AddComponent<Camera>();cam.CopyFrom(Camera.main);cam.enabled=false;cam.orthographic=true;
cam.orthographicSize=213.333333f;cam.aspect=1.5f;cam.nearClipPlane=.1f;cam.farClipPlane=1200;
cam.transform.SetPositionAndRotation(new Vector3(280,700,-6.666667f),Quaternion.LookRotation(Vector3.down,Vector3.forward));
var rt=new RenderTexture(4800,3200,24);var previous=RenderTexture.active;
try {cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;var tex=new Texture2D(4800,3200,TextureFormat.RGB24,false);try{tex.ReadPixels(new Rect(0,0,4800,3200),0,0);tex.Apply();System.IO.File.WriteAllBytes("Docs/BackyardForward/SHORTCUT_OVERHEAD.png",tex.EncodeToPNG());}finally{UnityEngine.Object.DestroyImmediate(tex);}}
finally{RenderTexture.active=previous;cam.targetTexture=null;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(go);}
// Reload without saving: documentation must never persist temporary scene state.
UnityEditor.SceneManagement.EditorSceneManager.OpenScene(original);
System.IO.File.WriteAllText("Docs/BackyardForward/shortcut-map-capture-done.txt","4800x3200 orthographic capture; X=-40..600, Z=-220..206.666667; saved approved scene; no scene save");
}catch(Exception e){System.IO.File.WriteAllText("Docs/BackyardForward/shortcut-map-capture-error.txt",e.ToString());}};return "Scheduled read-only approved world capture";

