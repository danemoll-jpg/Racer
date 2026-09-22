UnityEditor.EditorApplication.delayCall += () => { try {
UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/StreetLoopReverse.unity");
var replacement=GameObject.Find("Laurel separate shortcut ramp");if(replacement)UnityEngine.Object.DestroyImmediate(replacement);
var baseline=Newtonsoft.Json.Linq.JArray.Parse(System.IO.File.ReadAllText("Docs/SurgicalTracks/baseline-meshes.json"));
var report=new List<string>();
foreach(var item in baseline){string name=(string)item["name"];var mf=GameObject.Find(name).GetComponent<MeshFilter>();if(!UnityEditor.AssetDatabase.GetAssetPath(mf.sharedMesh).Contains("LocalRecovery"))continue;
string path=UnityEditor.AssetDatabase.GUIDToAssetPath((string)item["guid"]);var mesh=UnityEditor.AssetDatabase.LoadAssetAtPath<Mesh>(path);if(!mesh)throw new Exception("Missing baseline "+name);mf.sharedMesh=mesh;mf.GetComponent<MeshCollider>().sharedMesh=mesh;report.Add(name+" => "+path);}
// Keep every recovery component and its serialized configuration intact.
var root=GameObject.Find("Local Laurel replacement");
foreach(var mf in root.GetComponentsInChildren<MeshFilter>())UnityEngine.Object.DestroyImmediate(mf.gameObject);
var branch=UnityEngine.Object.FindObjectsByType<Racer.WoodlandRoute>().Single(b=>b.title=="Laurel Switchbacks");
var source=System.IO.File.ReadAllText("Docs/SurgicalTracks/baseline-branch.txt");
branch.points=System.Text.RegularExpressions.Regex.Matches(source,@"- \{x: (.*?), y: (.*?), z: (.*?)\}").Cast<System.Text.RegularExpressions.Match>().Select(m=>new Vector3(float.Parse(m.Groups[1].Value,System.Globalization.CultureInfo.InvariantCulture),float.Parse(m.Groups[2].Value,System.Globalization.CultureInfo.InvariantCulture),float.Parse(m.Groups[3].Value,System.Globalization.CultureInfo.InvariantCulture))).ToArray();
typeof(Racer.WoodlandRoute).GetField("lengths",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).SetValue(branch,null);branch.Initialize();
Physics.SyncTransforms();UnityEditor.EditorUtility.SetDirty(branch);var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);UnityEditor.AssetDatabase.SaveAssets();
System.IO.File.WriteAllLines("Docs/SurgicalTracks/restored.txt",report);
}catch(Exception e){System.IO.File.WriteAllText("Docs/SurgicalTracks/restore-error.txt",e.ToString());}};return "Scheduled surgical baseline restoration";
