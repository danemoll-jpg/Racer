UnityEditor.EditorApplication.delayCall += () => {try {
UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/StreetLoopReverse.unity");
var rows=new List<string>();
foreach(var mf in UnityEngine.Object.FindObjectsByType<MeshFilter>().Where(m=>m.GetComponent<Renderer>() && m.GetComponent<Renderer>().bounds.Intersects(new Bounds(new Vector3(440,75,-200),new Vector3(240,90,220))) && (m.name.StartsWith("Ground")||m.name.Contains("House")))) rows.Add(mf.name+" | "+mf.GetComponent<Renderer>().bounds+" | "+AssetDatabase.GetAssetPath(mf.sharedMesh));
var branch=UnityEngine.Object.FindObjectsByType<Racer.WoodlandRoute>().Single(b=>b.title=="Laurel Switchbacks");for(float s=0;s<branch.Length;s+=20)rows.Add("BRANCH "+s+" "+branch.At(s,out var f)+" "+f);
System.IO.File.WriteAllLines("Docs/SimpleLaurel/before.txt",rows);
var go=new GameObject("Inspection camera",typeof(Camera));var cam=go.GetComponent<Camera>();cam.transform.SetPositionAndRotation(new Vector3(440,400,-200),Quaternion.Euler(90,0,0));cam.orthographic=true;cam.orthographicSize=140;cam.farClipPlane=1000;
var rt=new RenderTexture(1200,1200,24);var tex=new Texture2D(1200,1200,TextureFormat.RGB24,false);var prior=RenderTexture.active;cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1200,1200),0,0);tex.Apply();System.IO.File.WriteAllBytes("Docs/SimpleLaurel/before-map.png",tex.EncodeToPNG());RenderTexture.active=prior;UnityEngine.Object.DestroyImmediate(tex);UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(go);
}catch(Exception e){System.IO.File.WriteAllText("Docs/SimpleLaurel/error.txt",e.ToString());}};return "Scheduled inspection";
