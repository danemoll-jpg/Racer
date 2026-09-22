UnityEditor.EditorApplication.delayCall += () => {try {
System.IO.Directory.CreateDirectory("Docs/ForestHill");
var b=UnityEngine.Object.FindObjectsByType<Racer.WoodlandRoute>().Single(b=>b.title=="Granite Saddle");
for(int i=0;i<2;i++){
var p=b.At(i==0?140:120,out var f);var hits=Physics.RaycastAll(p+Vector3.up*150,Vector3.down,300,1,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name.StartsWith("Ground")).OrderByDescending(h=>h.point.y).ToArray();p.y=hits[0].point.y;
var go=new GameObject("Temporary inspection camera",typeof(Camera));var cam=go.GetComponent<Camera>();cam.transform.position=p+Vector3.up*(i==0?2:60);cam.transform.LookAt(b.At(175,out _)+Vector3.up*20);cam.farClipPlane=1500;
var rt=new RenderTexture(1200,760,24);var tex=new Texture2D(1200,760,TextureFormat.RGB24,false);var prior=RenderTexture.active;cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1200,760),0,0);tex.Apply();System.IO.File.WriteAllBytes("Docs/ForestHill/before-"+i+".png",tex.EncodeToPNG());RenderTexture.active=prior;UnityEngine.Object.DestroyImmediate(tex);UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(go);
}
}catch(Exception e){System.IO.File.WriteAllText("Docs/ForestHill/error.txt",e.ToString());}};return "Scheduled hill reference views";
