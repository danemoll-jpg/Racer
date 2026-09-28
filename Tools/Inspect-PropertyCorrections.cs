using System;using System.IO;using System.Linq;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
public static class InspectPropertyCorrections {
 public static string Main(){
 if(Application.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Saved edit mode required");
 Directory.CreateDirectory("Docs/PropertyCorrections");EditorSceneManager.OpenScene("Assets/Scenes/StreetLoopReverse.unity");
 var home=GameObject.Find("Dan - blue X").transform;
 var rows=home.GetComponentsInChildren<Transform>().Where(t=>t.name.Contains("garage")||t.name.Contains("house")||t.name.Contains("Kennel")).Select(t=>t.name+" local="+home.InverseTransformPoint(t.position)+" world="+t.position).ToList();
 foreach(var name in new[]{"Dan - blue X","Original house 2","Original house 3","House 3 / Rocky Way Acres entrance"}){var t=GameObject.Find(name).transform;rows.Add(name+" "+t.position+" yaw="+t.eulerAngles);}
 var sign=GameObject.Find("House 3 / Rocky Way Acres entrance").transform;rows.AddRange(sign.GetComponentsInChildren<Transform>().Select(t=>t.name+" local="+t.localPosition+" scale="+t.localScale));
 File.WriteAllLines("Docs/PropertyCorrections/before.txt",rows);
 Shot("property-overhead-before",home.TransformPoint(new Vector3(0,100,-6)),home.TransformPoint(new Vector3(0,0,-6)),home.forward);
 Shot("first-gate-before",home.TransformPoint(new Vector3(-20,3,24)),home.TransformPoint(new Vector3(-25,-3,-16)),Vector3.up);
 Shot("wrong-gate-before",home.TransformPoint(new Vector3(20,3,24)),home.TransformPoint(new Vector3(10,0,0)),Vector3.up);
 Shot("sign-before",sign.position+sign.forward*-27+Vector3.up*4,sign.position+Vector3.up*2,Vector3.up);
 return string.Join("\n",rows);
 }
 public static void Shot(string n,Vector3 p,Vector3 q,Vector3 up){var go=new GameObject("Inspection camera");var c=go.AddComponent<Camera>();c.CopyFrom(Camera.main);c.enabled=false;c.transform.SetPositionAndRotation(p,Quaternion.LookRotation(q-p,up));var rt=new RenderTexture(1200,800,24);c.targetTexture=rt;c.Render();var old=RenderTexture.active;RenderTexture.active=rt;var t=new Texture2D(1200,800,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,1200,800),0,0);t.Apply();File.WriteAllBytes("Docs/PropertyCorrections/"+n+".png",t.EncodeToPNG());RenderTexture.active=old;c.targetTexture=null;Object.DestroyImmediate(t);Object.DestroyImmediate(rt);Object.DestroyImmediate(go);}
}
