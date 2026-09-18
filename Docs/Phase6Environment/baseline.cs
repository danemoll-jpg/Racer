var dir="Docs/Phase6Environment";
var r=Racer.Editor.StreetLoopBuilder.Route();
var woods=JsonUtility.FromJson<Racer.WoodlandBenchmark.Plan>(System.IO.File.ReadAllText("Docs/CR014/routes.json")).routes[0];
int k=Racer.Editor.Phase5Setup.Closest(r,new Vector3(-400,0,530));
var commercial=new Racer.WoodlandBenchmark.Route{name="commercial",speed=18,points=Enumerable.Range(0,300).Select(i=>r[(k+i)%r.Count]).ToArray()};
System.IO.File.WriteAllText(dir+"/routes.json",JsonUtility.ToJson(new Racer.WoodlandBenchmark.Plan{seconds=24,routes=new[]{woods,commercial}},true));
var sites=GameObject.Find(Racer.Editor.Phase6Review.Root).transform.Cast<Transform>().Select(t=>t.name+"|"+t.position.ToString("F6")+"|"+t.rotation.ToString("F6")).ToArray();System.IO.File.WriteAllLines(dir+"/sites-before.txt",sites);
return new {scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene().path,dirty=UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty,ground=GameObject.Find("Memory loop - north is +Z").GetComponentsInChildren<MeshCollider>().Length};
