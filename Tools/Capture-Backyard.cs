EditorApplication.delayCall+=()=>{try{
foreach(string name in new[]{"DansBackyard","DansBackyardReverse"}){
UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/"+name+".unity");
foreach(var view in new[]{("property",new Vector3(440,106,42),new Vector3(420,80,9)),("dump",new Vector3(310,125,35),new Vector3(294,75,12)),("gully",new Vector3(112,71,-140),new Vector3(94,38,-100)),("return",new Vector3(404,120,167),new Vector3(417,83,85))}){
var go=new GameObject("Temporary inspection camera");var cam=go.AddComponent<Camera>();cam.CopyFrom(Camera.main);cam.enabled=false;cam.transform.SetPositionAndRotation(view.Item2,Quaternion.LookRotation(view.Item3-view.Item2));var rt=new RenderTexture(1200,760,24);cam.targetTexture=rt;cam.Render();var prev=RenderTexture.active;RenderTexture.active=rt;var tex=new Texture2D(1200,760,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1200,760),0,0);tex.Apply();System.IO.File.WriteAllBytes("Docs/Backyard/"+name+"-"+view.Item1+".png",tex.EncodeToPNG());RenderTexture.active=prev;cam.targetTexture=null;UnityEngine.Object.DestroyImmediate(tex);UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(go);
}}
System.IO.File.WriteAllText("Docs/Backyard/capture-done.txt","Four local views of each direction captured");
}catch(Exception e){System.IO.File.WriteAllText("Docs/Backyard/capture-error.txt",e.ToString());}};return "Scheduled local course views";
