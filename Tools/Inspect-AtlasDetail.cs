UnityEditor.EditorApplication.delayCall += () => {try {
var data=Newtonsoft.Json.Linq.JObject.Parse(System.IO.File.ReadAllText("Docs/RouteAtlas/routes-before.json"));
var courses=(Newtonsoft.Json.Linq.JArray)data["courses"];
var granite=courses.Single(c=>(string)c["scene"]=="ForestLoopReverse")["routes"].Single(r=>(string)r["name"]=="Granite Saddle")["points"].ToObject<Vector3[]>();
var output=new List<object>();
foreach(var scene in new[]{"StreetLoopReverse","LakeWoods","ForestLoopReverse"}) {
UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();
var race=UnityEngine.Object.FindAnyObjectByType<Racer.RaceDirector>();
var samples=new List<object>();
for(int i=0;i<granite.Length;i+=8){var p=granite[i];var hits=Physics.RaycastAll(new Vector3(p.x,180,p.z),Vector3.down,230,~0,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name.StartsWith("Ground")||h.collider.name.Contains("pavement")).OrderBy(h=>Math.Abs(h.point.y-p.y)).Take(3).Select(h=>new {name=h.collider.name,y=h.point.y,asset=h.collider is MeshCollider m?AssetDatabase.GetAssetPath(m.sharedMesh):""}).ToArray();samples.Add(new{point=p,hits});}
output.Add(new{scene,graniteSamples=samples,roads=UnityEngine.Object.FindObjectsByType<Racer.RaceRoad>().Select(r=>new{name=r.name,points=r.points}).ToArray(),zones=UnityEngine.Object.FindObjectsByType<Racer.JumpRecoveryExclusion>().Select(z=>new{name=z.name,start=z.start,end=z.end,width=z.halfWidth}).ToArray(),signs=UnityEngine.Object.FindObjectsByType<TextMesh>().Where(t=>t.GetComponentInParent<Racer.PhysicalSign>()).Select(t=>new{name=t.transform.parent.name,text=t.text,position=t.transform.position}).ToArray(),localSurfaces=UnityEngine.Object.FindObjectsByType<MeshCollider>().Where(m=>m.bounds.Intersects(new Bounds(new Vector3(420,65,-170),new Vector3(500,220,440)))).Select(m=>new{name=m.name,bounds=m.bounds,asset=AssetDatabase.GetAssetPath(m.sharedMesh)}).ToArray()});
}
System.IO.File.WriteAllText("Docs/RouteAtlas/detail-before.json",Newtonsoft.Json.JsonConvert.SerializeObject(output,new Newtonsoft.Json.JsonSerializerSettings{ReferenceLoopHandling=Newtonsoft.Json.ReferenceLoopHandling.Ignore}));
}catch(Exception e){System.IO.File.WriteAllText("Docs/RouteAtlas/detail-error.txt",e.ToString());}};return "Scheduled detail export";
