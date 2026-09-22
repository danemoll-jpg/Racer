using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Racer;
using Object=UnityEngine.Object;
public static class FinishStructuralGuidance {
 public static string Forward()=>Finish("MountainLoop","Summit Traverse");
 public static string Laurel()=>Finish("StreetLoopReverse","Laurel Switchbacks");
 public static string CaptureForward(){EditorSceneManager.OpenScene("Assets/Scenes/MountainLoop.unity");Shot("Mountain-junction",new Vector3(726,100,-99),new Vector3(757,89,-137));return "Captured finalized junction.";}
 public static string CaptureLaurel(){EditorSceneManager.OpenScene("Assets/Scenes/StreetLoopReverse.unity");var zone=Object.FindObjectsByType<JumpRecoveryExclusion>().Single(z=>z.name=="Laurel runway no recovery");var axis=Vector3.ProjectOnPlane(zone.end-zone.start,Vector3.up).normalized;Shot("Laurel-driver-view",zone.start+axis*30+Vector3.up*3,zone.end);return "Captured straight driver approach.";}
 static string Finish(string scene,string title){EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");var branch=Object.FindObjectsByType<WoodlandRoute>().Single(b=>b.title==title);var root=new GameObject("CR133 rerouted shortcut guidance").transform;bool laurel=title.StartsWith("Laurel");
  foreach(var mf in Object.FindObjectsByType<MeshFilter>().Where(m=>m.name.IndexOf("gold",StringComparison.OrdinalIgnoreCase)>=0)){var r=mf.GetComponent<Renderer>();if(!r)continue;var p=r.bounds.center;if(laurel?p.x>180&&p.x<510&&p.z< -70&&p.z> -350:p.x>670&&p.x<1100&&p.z> -20&&p.z<210)mf.gameObject.SetActive(false);}
  Physics.SyncTransforms();var mat=AssetDatabase.LoadAssetAtPath<Material>("Assets/Track/Discovery/CR117 alternate gold.mat");int count=0;
  for(float s=12;s<branch.Length-5;s+=14){var p=branch.At(s,out var f);var right=Vector3.Cross(Vector3.up,f).normalized;f=Vector3.ProjectOnPlane(f,Vector3.up).normalized;var shape=new[]{new Vector2(-.44f,-3),new(.44f,-3),new(.44f,0),new(1.1f,0),new(0,3),new(-1.1f,0),new(-.44f,0)};var vertices=new List<Vector3>();foreach(var v in shape){var q=p+right*v.x+f*v.y;var hits=Physics.RaycastAll(q+Vector3.up*3,Vector3.down,6,1,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name.StartsWith("Ground_CR133")&&h.collider.name.Contains("driving surface")).OrderBy(h=>Math.Abs(h.point.y-q.y)).ToArray();if(hits.Length==0)break;q.y=hits[0].point.y+.08f;vertices.Add(q);}if(vertices.Count!=7)continue;
   var mesh=new Mesh{vertices=vertices.ToArray(),triangles=new[]{0,6,1,1,6,2,6,5,4,6,4,2,2,4,3}};mesh.RecalculateNormals();mesh.RecalculateBounds();string path=$"Assets/Track/StructuralRoads/{scene}-gold-{count}.asset";AssetDatabase.CreateAsset(mesh,path);var go=new GameObject("CR133 shortcut gold arrow",typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(root);go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<Renderer>().sharedMaterial=mat;count++;
  }
  if(!laurel)foreach(var sign in Object.FindObjectsByType<PhysicalSign>()){var text=sign.GetComponentInChildren<TextMesh>();if(!text||!text.text.Contains("SUMMIT TRAVERSE"))continue;var p=branch.At(7,out var f)+Vector3.Cross(Vector3.up,f).normalized*9;var hits=Physics.RaycastAll(p+Vector3.up*8,Vector3.down,16,1,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name.StartsWith("Ground_")).OrderBy(h=>Math.Abs(h.point.y-p.y)).ToArray();if(hits.Length>0)p.y=hits[0].point.y;sign.transform.SetPositionAndRotation(p,Quaternion.LookRotation(Vector3.ProjectOnPlane(f,Vector3.up)));}
  EditorSceneManager.MarkSceneDirty(branch.gameObject.scene);EditorSceneManager.SaveScene(branch.gameObject.scene);AssetDatabase.SaveAssets();
  if(laurel){var zone=Object.FindObjectsByType<JumpRecoveryExclusion>().Single(z=>z.name=="Laurel runway no recovery");var axis=Vector3.ProjectOnPlane(zone.end-zone.start,Vector3.up).normalized;Shot("Laurel-straight",zone.start-axis*8+Vector3.up*12,zone.end);}
  else{Shot("Mountain-junction",new Vector3(726,100,-99),new Vector3(757,89,-137));Shot("Mountain-supported",new Vector3(1020,205,55),new Vector3(931,145,56));}
  return $"{scene}: {count} non-colliding guidance arrows aligned to the changed shortcut.";
 }
 static void Shot(string name,Vector3 position,Vector3 target){var go=new GameObject("Temporary structural review camera",typeof(Camera));var cam=go.GetComponent<Camera>();cam.transform.SetPositionAndRotation(position,Quaternion.LookRotation(target-position));cam.fieldOfView=65;cam.farClipPlane=1800;var rt=new RenderTexture(1440,900,24);var tex=new Texture2D(1440,900,TextureFormat.RGB24,false);var old=RenderTexture.active;try{cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1440,900),0,0);tex.Apply();File.WriteAllBytes("Docs/CR133-137/"+name+".png",tex.EncodeToPNG());}finally{RenderTexture.active=old;Object.DestroyImmediate(tex);Object.DestroyImmediate(rt);Object.DestroyImmediate(go);}}
}
