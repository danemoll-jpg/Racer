using System;using System.IO;using System.Linq;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
public static class CaptureFiveUpdates {
 public static string Main(){
 if(Application.isPlaying)throw new Exception("Edit mode required");
 EditorSceneManager.OpenScene("Assets/Scenes/StreetLoopReverse.unity");var race=Object.FindAnyObjectByType<RaceDirector>();var p=new Vector3(-624,7.5f,-220);var s=race.road.Project(p,out _);p=race.road.At(s,out var f);Shot("Exit-reverse",p+Vector3.up*2.7f,p+f*35+Vector3.up*2);
 var home=GameObject.Find("Dan - blue X").transform;Shot("Property-after",home.TransformPoint(new Vector3(0,65,-5)),home.position+Vector3.forward*.1f);
 EditorSceneManager.OpenScene("Assets/Scenes/ForestLoopReverse.unity");var b=Object.FindObjectsByType<WoodlandRoute>().Single(b=>b.title=="McFadden Cut");p=b.At(b.entryInset-15,out f);Shot("McFadden-sign",p+Vector3.up*2.5f,b.At(b.entryInset+10,out _)+Vector3.up*1.5f);
 // Name-only atlas verification: exact main/shortcut control points and gates remain unchanged.
 var atlas=JsonUtility.FromJson<InspectRouteAtlas.Atlas>(File.ReadAllText("Docs/ForestWaterJump/routes-current.json"));race=Object.FindAnyObjectByType<RaceDirector>();var c=atlas.courses.Single(c=>c.scene=="ForestLoopReverse");if(!c.routes.Single(r=>r.name=="McFadden Cut").points.SequenceEqual(b.points)||!c.routes.Single(r=>r.kind=="main").points.SequenceEqual(race.road.points)||!c.gates.SequenceEqual(race.gates.Select(g=>g.transform.position)))throw new Exception("Route geometry changed");
 File.WriteAllText("Docs/FiveUpdates/preservation.txt","PASS: Forest main and McFadden Cut points and all checkpoint positions match the previous delivered atlas exactly. Existing sign helper reused; unsupported label removed.\n");return "Captured exit/sign/property; route preservation passed";
 }
 static void Shot(string n,Vector3 p,Vector3 q){var go=new GameObject("Inspection camera");var c=go.AddComponent<Camera>();c.CopyFrom(Camera.main);c.enabled=false;c.transform.SetPositionAndRotation(p,Quaternion.LookRotation(q-p));var rt=new RenderTexture(1100,700,24);c.targetTexture=rt;c.Render();var old=RenderTexture.active;RenderTexture.active=rt;var t=new Texture2D(1100,700,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,1100,700),0,0);t.Apply();File.WriteAllBytes("Docs/FiveUpdates/"+n+".png",t.EncodeToPNG());RenderTexture.active=old;c.targetTexture=null;Object.DestroyImmediate(t);Object.DestroyImmediate(rt);Object.DestroyImmediate(go);}
}
