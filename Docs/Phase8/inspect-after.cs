var dir="Docs/Phase8";
string PathOf(Transform t) => t.parent ? PathOf(t.parent)+"/"+t.name : t.name;
var rows=new System.Collections.Generic.List<string>();
foreach(var f in GameObject.Find("Memory loop - north is +Z").GetComponentsInChildren<MeshFilter>()){
var m=f.sharedMesh;var data=Newtonsoft.Json.JsonConvert.SerializeObject(new {v=m.vertices.Select(v=>new[]{v.x,v.y,v.z}),n=m.normals.Select(v=>new[]{v.x,v.y,v.z}),t=m.triangles,c=m.colors.Select(c=>new[]{c.r,c.g,c.b,c.a})});using var hash=System.Security.Cryptography.SHA256.Create();rows.Add(f.name+"|"+System.BitConverter.ToString(hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(data))));}
System.IO.File.WriteAllLines(dir+"/terrain-after.txt",rows.OrderBy(x=>x));
var colliders=UnityEngine.Object.FindObjectsByType<Collider>().Select(c=>PathOf(c.transform)+"|"+c.transform.position.ToString("F6")+"|"+c.transform.rotation.ToString("F6")+"|"+c.bounds.ToString("F6")+"|"+c.enabled+"|"+c.isTrigger).OrderBy(x=>x);
System.IO.File.WriteAllLines(dir+"/colliders-after.txt",colliders);
System.IO.File.WriteAllLines(dir+"/sites-after.txt",GameObject.Find(Racer.Editor.Phase6Review.Root).transform.Cast<Transform>().Select(t=>t.name+"|"+t.position.ToString("F6")+"|"+t.rotation.ToString("F6")).OrderBy(x=>x));
var rp=(UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset)UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
var inventory=new {scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene().path,rp.name,rp.shadowDistance,rp.mainLightShadowmapResolution,rp.shadowCascadeCount,rp.supportsSoftShadows,rp.msaaSampleCount,rp.renderScale, renderers=UnityEngine.Object.FindObjectsByType<Renderer>().Where(r=>r.enabled).GroupBy(r=>r.sharedMaterial?r.sharedMaterial.shader.name:"none").Select(g=>new{shader=g.Key,count=g.Count(),triangles=g.Sum(r=>r.GetComponent<MeshFilter>()?r.GetComponent<MeshFilter>().sharedMesh.triangles.Length/3:0)}),lights=UnityEngine.Object.FindObjectsByType<Light>().Select(l=>new{l.name,l.intensity,l.shadows}),car=UnityEngine.Object.FindAnyObjectByType<Racer.ArcadeVehicle>().GetComponentsInChildren<Transform>().Select(t=>new{t.name,position=t.localPosition.ToString(),scale=t.localScale.ToString()})};
System.IO.File.WriteAllText(dir+"/inventory-after.json",Newtonsoft.Json.JsonConvert.SerializeObject(inventory,Newtonsoft.Json.Formatting.Indented));return inventory;

