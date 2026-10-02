using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Object=UnityEngine.Object;
// 0.69 before/after views at each reported coordinate, framed like the chase camera along the reported heading.
// Each view also writes a pick grid (what visible object each of 12x7 screen rays meets) so the image can be read.
// PROBE_TAG names the set; PROBE_IDS limits views (prefix match); PROBE_EXTRA adds "id|scene|ex,ey,ez|tx,ty,tz;..."
public static class Report069Views {
 const int Ghost=31;
 static string Path(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n.Replace('\n','/');}
 public static void Run(){var tag=Environment.GetEnvironmentVariable("PROBE_TAG")??"view";var outDir=Environment.GetEnvironmentVariable("PROBE_OUT")+"/views";Directory.CreateDirectory(outDir);
  var only=Environment.GetEnvironmentVariable("PROBE_IDS");bool Want(string id)=>string.IsNullOrEmpty(only)||only.Split(',').Any(o=>id.StartsWith(o));
  Vector3 V(string s){var a=s.Split(',').Select(float.Parse).ToArray();return new Vector3(a[0],a[1],a[2]);}
  var extra=(Environment.GetEnvironmentVariable("PROBE_EXTRA")??"").Split(';').Where(x=>x.Contains('|')).Select(x=>{var a=x.Split('|');return(id:a[0],scene:a[1],eye:V(a[2]),target:V(a[3]));});
  var shots=Report069Probe.Reports.Where(r=>Want(r.id)).Select(r=>{var f=Quaternion.Euler(0,r.heading,0)*Vector3.forward;var p=r.p.y<0?new Vector3(r.p.x,86,r.p.z):r.p;return(r.id,r.scene,eye:p-f*6.5f+Vector3.up*2.6f,target:p+f*12+Vector3.up*.5f);})
   .Concat(extra.Where(e=>Want(e.id))).GroupBy(v=>v.scene);
  foreach(var g in shots){EditorSceneManager.OpenScene("Assets/Scenes/"+g.Key+".unity");Physics.queriesHitBackfaces=true;
   var ghosts=new List<GameObject>();
   foreach(var mr in Object.FindObjectsByType<MeshRenderer>()){if(!mr.enabled)continue;var mf=mr.GetComponent<MeshFilter>();if(!mf||!mf.sharedMesh)continue;
    var go=new GameObject("ghost:"+Path(mr.transform));go.layer=Ghost;go.transform.SetPositionAndRotation(mr.transform.position,mr.transform.rotation);go.transform.localScale=mr.transform.lossyScale;go.AddComponent<MeshCollider>().sharedMesh=mf.sharedMesh;ghosts.Add(go);}
   Physics.SyncTransforms();
   var cam=new GameObject("view",typeof(Camera)).GetComponent<Camera>();if(Camera.main)cam.CopyFrom(Camera.main);cam.enabled=false;cam.farClipPlane=2500;cam.nearClipPlane=.1f;cam.fieldOfView=60;cam.cullingMask&=~(1<<Ghost);
   {var first=g.First();cam.transform.SetPositionAndRotation(first.eye,Quaternion.LookRotation(first.target-first.eye));var wrt=new RenderTexture(64,36,24);cam.targetTexture=wrt;cam.Render();cam.Render();cam.targetTexture=null;wrt.Release();}
   foreach(var v in g){cam.transform.SetPositionAndRotation(v.eye,Quaternion.LookRotation(v.target-v.eye));var rt=new RenderTexture(1280,720,24);cam.targetTexture=rt;cam.aspect=16f/9;cam.Render();RenderTexture.active=rt;var tex=new Texture2D(1280,720,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1280,720),0,0);tex.Apply();File.WriteAllBytes($"{outDir}/{v.id}-{tag}.png",tex.EncodeToPNG());cam.targetTexture=null;RenderTexture.active=null;rt.Release();Object.DestroyImmediate(tex);
    var picks=new List<string>{$"{v.id} eye={v.eye:F1} target={v.target:F1}; rows top->bottom, cols left->right (viewport x,y)"};
    for(int yi=6;yi>=0;yi--)for(int xi=0;xi<12;xi++){float x=(xi+.5f)/12,y=(yi+.5f)/7;var ray=cam.ViewportPointToRay(new Vector3(x,y,0));
     picks.Add(Physics.Raycast(ray,out var h,2500,1<<Ghost,QueryTriggerInteraction.Collide)?$"({x:F2},{y:F2}) {h.distance,6:F1}m {h.point:F1} n{h.normal.y:F2} {h.collider.name.Substring(6)}":$"({x:F2},{y:F2}) sky");}
    var fine=Environment.GetEnvironmentVariable("PROBE_FINE");if(!string.IsNullOrEmpty(fine)){var q=fine.Split(',').Select(float.Parse).ToArray();int n=(int)q[4];
     for(int yi=n-1;yi>=0;yi--)for(int xi=0;xi<n;xi++){float x=Mathf.Lerp(q[0],q[1],xi/(n-1f)),y=Mathf.Lerp(q[2],q[3],yi/(n-1f));var ray=cam.ViewportPointToRay(new Vector3(x,y,0));
      picks.Add(Physics.Raycast(ray,out var h,2500,1<<Ghost,QueryTriggerInteraction.Collide)?$"F({x:F3},{y:F3}) {h.distance,6:F1}m {h.point:F2} n{h.normal.y:F2} {h.collider.name.Substring(6)} tri{h.triangleIndex}":$"F({x:F3},{y:F3}) sky");}}
    File.WriteAllLines($"{outDir}/{v.id}-{tag}-picks.txt",picks);}
   Object.DestroyImmediate(cam.gameObject);foreach(var go in ghosts)Object.DestroyImmediate(go);}
  EditorApplication.Exit(0);}
}
