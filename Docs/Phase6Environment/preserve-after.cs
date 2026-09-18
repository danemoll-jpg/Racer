var dir="Docs/Phase6Environment";
var rows=new System.Collections.Generic.List<string>();
foreach(var f in GameObject.Find("Memory loop - north is +Z").GetComponentsInChildren<MeshFilter>()){
var m=f.sharedMesh;var data=Newtonsoft.Json.JsonConvert.SerializeObject(new {v=m.vertices.Select(v=>new[]{v.x,v.y,v.z}),n=m.normals.Select(v=>new[]{v.x,v.y,v.z}),t=m.triangles});var hash=System.Security.Cryptography.SHA256.Create();rows.Add(f.name+"|"+System.BitConverter.ToString(hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(data))));hash.Dispose();}
System.IO.File.WriteAllLines(dir+"/terrain-after.txt",rows);
var colliders=UnityEngine.Object.FindObjectsByType<Collider>().OrderBy(c=>c.GetEntityId()).Select(c=>c.GetEntityId()+"|"+c.name+"|"+c.transform.position.ToString("F6")+"|"+c.transform.rotation.ToString("F6")+"|"+c.bounds.ToString("F6")+"|"+c.enabled+"|"+c.isTrigger);
System.IO.File.WriteAllLines(dir+"/colliders-after.txt",colliders);
return rows.Count;

