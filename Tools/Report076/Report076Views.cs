using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Object=UnityEngine.Object;
// 0.76 editor views (from the 0.74 tool) (authored look): Dan's two report positions framed like the chase camera, plus top-down views of each
// area with a pick grid, used to place the snow scenes. PROBE_EXTRA adds "id|scene|ex,ey,ez|tx,ty,tz[|ortho]" views.
public static class Report076Views {
 static string Path(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n.Replace('\n','/');}
 public static void Run(){var tag=Environment.GetEnvironmentVariable("PROBE_TAG")??"view";var outDir=Environment.GetEnvironmentVariable("PROBE_OUT")+"/views";Directory.CreateDirectory(outDir);
  Vector3 V(string s){var a=s.Split(',').Select(float.Parse).ToArray();return new Vector3(a[0],a[1],a[2]);}
  var list=new List<(string id,string scene,Vector3 eye,Vector3 target,float ortho)>();
  void Chase(string id,string scene,Vector3 p,float heading){var f=Quaternion.Euler(0,heading,0)*Vector3.forward;list.Add((id,scene,p-f*6.5f+Vector3.up*2.6f,p+f*12+Vector3.up*.5f,0));}
  if(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROBE_NODEFAULT"))){
  Chase("BUG-001","DansBackyardReverse",new Vector3(519.21f,74.77f,-153.47f),171.92f);
  Chase("BUG-002","DansBackyardReverse",new Vector3(400.78f,80.17f,-5.15f),196.6f);
  list.Add(("TOP-sled","DansBackyardReverse",new Vector3(510,200,-180),new Vector3(510,0,-180.01f),40));
  list.Add(("TOP-pool","DansBackyardReverse",new Vector3(405,200,-10),new Vector3(405,0,-10.01f),22));}
  foreach(var x in (Environment.GetEnvironmentVariable("PROBE_EXTRA")??"").Split(';').Where(x=>x.Contains('|'))){var a=x.Split('|');list.Add((a[0],a[1],V(a[2]),V(a[3]),a.Length>4?float.Parse(a[4]):0));}
  foreach(var g in list.GroupBy(v=>v.scene)){EditorSceneManager.OpenScene("Assets/Scenes/"+g.Key+".unity");Physics.queriesHitBackfaces=true;
   const int Ghost=31;var ghosts=new List<GameObject>();
   foreach(var mr in Object.FindObjectsByType<MeshRenderer>()){if(!mr.enabled)continue;var mf=mr.GetComponent<MeshFilter>();if(!mf||!mf.sharedMesh)continue;
    var go=new GameObject("ghost:"+Path(mr.transform));go.layer=Ghost;go.transform.SetPositionAndRotation(mr.transform.position,mr.transform.rotation);go.transform.localScale=mr.transform.lossyScale;go.AddComponent<MeshCollider>().sharedMesh=mf.sharedMesh;ghosts.Add(go);}
   Physics.SyncTransforms();
   var cam=new GameObject("view",typeof(Camera)).GetComponent<Camera>();if(Camera.main)cam.CopyFrom(Camera.main);cam.enabled=false;cam.farClipPlane=2500;cam.nearClipPlane=.1f;cam.fieldOfView=60;cam.cullingMask&=~(1<<Ghost);
   foreach(var v in g){cam.orthographic=v.ortho>0;cam.orthographicSize=v.ortho;cam.transform.SetPositionAndRotation(v.eye,Quaternion.LookRotation(v.target-v.eye,v.ortho>0?Vector3.forward:Vector3.up));
    var rt=new RenderTexture(1280,720,24);cam.targetTexture=rt;cam.aspect=16f/9;cam.Render();cam.Render();RenderTexture.active=rt;var tex=new Texture2D(1280,720,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1280,720),0,0);tex.Apply();File.WriteAllBytes($"{outDir}/{v.id}-{tag}.png",tex.EncodeToPNG());cam.targetTexture=null;RenderTexture.active=null;rt.Release();Object.DestroyImmediate(tex);
    var picks=new List<string>{$"{v.id} eye={v.eye:F1} target={v.target:F1} ortho={v.ortho}; rows top->bottom"};
    for(int yi=8;yi>=0;yi--)for(int xi=0;xi<16;xi++){float x=(xi+.5f)/16,y=(yi+.5f)/9;var ray=cam.ViewportPointToRay(new Vector3(x,y,0));
     picks.Add(Physics.Raycast(ray,out var h,2500,1<<Ghost,QueryTriggerInteraction.Collide)?$"({x:F2},{y:F2}) {h.distance,6:F1}m {h.point:F1} n{h.normal.y:F2} {h.collider.name.Substring(6)}":$"({x:F2},{y:F2}) sky");}
    File.WriteAllLines($"{outDir}/{v.id}-{tag}-picks.txt",picks);}
   Object.DestroyImmediate(cam.gameObject);foreach(var go in ghosts)Object.DestroyImmediate(go);}
  EditorApplication.Exit(0);}
}
