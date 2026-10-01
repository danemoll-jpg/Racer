using System;using System.IO;using System.Linq;using UnityEngine;using UnityEditor;using Racer;using Object=UnityEngine.Object;
public static class ViewsCaveRestore{
 public static string Main(){var b=Object.FindObjectsByType<WoodlandRoute>().Single(x=>x.title=="Echo Cave");var g=new GameObject("Temporary cave review",typeof(Camera));var cam=g.GetComponent<Camera>();cam.CopyFrom(Camera.main);cam.enabled=false;
 void Shot(string name,Vector3 eye,Vector3 target){cam.fieldOfView=70;cam.transform.SetPositionAndRotation(eye,Quaternion.LookRotation(target-eye));var rt=new RenderTexture(1100,700,24);var prior=RenderTexture.active;cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;var tex=new Texture2D(1100,700,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1100,700),0,0);tex.Apply();File.WriteAllBytes("Docs/CaveRestore/"+name+".png",tex.EncodeToPNG());cam.targetTexture=null;RenderTexture.active=prior;rt.Release();Object.DestroyImmediate(tex);Object.DestroyImmediate(rt);}
 foreach(float s in new[]{40f,110f,180f,195f,203f,270f,430f,492f})Shot("view-"+s,b.At(s,out _)+Vector3.up*2.3f,b.At(s+(s>480?-30:28),out _)+Vector3.up*1.5f);
 foreach(float s in new[]{100f,270f,400f}){var p=b.At(s,out var f);var side=Vector3.Cross(Vector3.up,f).normalized;var q=p+side*45;float ground=Physics.RaycastAll(new(q.x,200,q.z),Vector3.down,300).Where(h=>h.collider.name.StartsWith("Ground_")).Select(h=>h.point.y).DefaultIfEmpty(p.y).Max();q.y=ground+2;Shot("outside-"+s,q,p+Vector3.up*5);}
 Object.DestroyImmediate(g);return "Eight views saved";}
}


