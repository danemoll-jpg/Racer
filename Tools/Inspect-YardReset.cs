if(Application.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Saved edit mode required");
UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/StreetLoopGreybox.unity");
var rows=new List<string>();
string PathOf(Transform t)=>t.parent?PathOf(t.parent)+"/"+t.name:t.name;
foreach(var r in UnityEngine.Object.FindObjectsByType<Renderer>().Where(r=>r.bounds.Intersects(new Bounds(new Vector3(431,81,12),new Vector3(90,25,65)))&&!r.name.Contains("Tree")&&!r.name.Contains("trunk")&&!r.name.StartsWith("Ground_")))rows.Add(PathOf(r.transform)+" pos="+r.transform.position.ToString("F3")+" bounds="+r.bounds+" scale="+r.transform.localScale);
var mf=UnityEngine.Object.FindObjectsByType<MeshFilter>().Single(m=>m.name=="Ground_Dan beige concrete descent and parking");
rows.Add("DRIVE bounds="+mf.GetComponent<Renderer>().bounds);
for(float x=390;x<=474;x+=2){var hits=Physics.RaycastAll(new Vector3(x,200,9),Vector3.down,300,1,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name.StartsWith("Ground_")||h.collider.name.Contains("pavement"));rows.Add("PROFILE "+x+" "+string.Join(" / ",hits.Select(h=>h.collider.name+"="+h.point.y.ToString("F3"))));}
System.IO.Directory.CreateDirectory("Docs/YardReset");System.IO.File.WriteAllLines("Docs/YardReset/before.txt",rows);
return "Saved dependency inventory and driveway profile: "+rows.Count;
