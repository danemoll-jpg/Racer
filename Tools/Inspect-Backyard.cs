if(Application.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Saved edit mode required");
var anchors=new Vector3[]{new(478.3f,82,-1.9f),new(463.6f,80.2f,8),new(422.7f,83.3f,9),new(409.5f,81.9f,9.3f),new(391.9f,79.9f,9.7f),new(331.4f,78.5f,16.2f),new(258.6f,68.2f,8.2f),new(96.1f,38.8f,-108.6f),new(456.4f,81.8f,67.4f)};
Physics.SyncTransforms();var rows=new List<string>();
foreach(var p in anchors){rows.Add("ANCHOR "+p);foreach(var h in Physics.RaycastAll(new Vector3(p.x,400,p.z),Vector3.down,800,~0,QueryTriggerInteraction.Ignore).OrderBy(h=>h.distance))rows.Add(h.collider.name+" at "+h.point+" mesh="+(h.collider is MeshCollider m?AssetDatabase.GetAssetPath(m.sharedMesh):""));}
rows.Add("LOCAL OBJECTS");foreach(var c in UnityEngine.Object.FindObjectsByType<Collider>().Where(c=>c.bounds.Intersects(new Bounds(new Vector3(445,82,10),new Vector3(115,25,145)))&&!c.name.Contains("Tree")))rows.Add(c.name+" pos="+c.transform.position+" bounds="+c.bounds);
System.IO.Directory.CreateDirectory("Docs/Backyard");System.IO.File.WriteAllLines("Docs/Backyard/inspection.txt",rows);
return string.Join("\n",rows.Take(65));
