using System;using System.IO;using System.Linq;using System.Text;using System.Collections.Generic;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;using Racer;
// 0.85 Part C (BUG-001, Mountain Loop Forward road edge at 740.0, 86.9, -132.5). Two older sheets lie under the smooth road
// / trail-edge patch / shoulder layers here: "mountain earth banks" and "supported shoulders". Under the patch they sit up
// to 0.8 m (shoulders up to 3 m) lower, then climb steeply and break through at the patch's edge as steep facets (normals
// 40-55 degrees from vertical); a vehicle running 2 m wide hits them and is thrown into the air (impulse up to 2,500).
// Within 100 m of the spot along the main (s0 +- 100, up to half-width + 14 m off the centre line), every vertex of those two
// sheets that a covering layer (the CR133 driving surface, the 0.67-0.71 patches and shoulders) lies over (up to 3 m above
// it) or under (up to 0.6 m below it) is set 3 cm under that layer, so nothing steep can come up through it and the smooth
// layer alone carries a vehicle. Where nothing covers a vertex it is not moved. Only these two mesh assets (MountainLoop
// only) change. LIP_DRY=1 lists only.
public static class Report085Lip {
 static readonly string[] Sheets={"Ground_CR133 mountain earth banks","Ground_MountainPolish supported shoulders"};
 static bool Cover(Collider c)=>!c.isTrigger&&c is MeshCollider&&(c.name=="Ground_CR133 mountain driving surface"||c.name.StartsWith("Ground_Report06")||c.name.StartsWith("Ground_Report07"));
 public static void Run(){bool dry=Environment.GetEnvironmentVariable("LIP_DRY")=="1";var o=Environment.GetEnvironmentVariable("PROBE_OUT");var sb=new StringBuilder();
  try{var s=EditorSceneManager.OpenScene("Assets/Scenes/MountainLoop.unity");Physics.SyncTransforms();var roots=s.GetRootGameObjects();
   var rd=roots.SelectMany(g=>g.GetComponentsInChildren<RaceDirector>(true)).First().road;rd.Initialize();float s0=29.5f;
   foreach(var name in Sheets){var sheet=roots.SelectMany(g=>g.GetComponentsInChildren<MeshCollider>(true)).First(c=>c.name==name);
    var mesh=sheet.sharedMesh;var mf=sheet.GetComponent<MeshFilter>();sb.AppendLine($"{name}: {AssetDatabase.GetAssetPath(mesh)} ({mesh.vertexCount} vertices); renderer shares it: {(mf&&mf.sharedMesh==mesh)}");
    var t=sheet.transform;var v=mesh.vertices;int lowered=0,raised=0;float maxDown=0,maxUp=0;var by=new Dictionary<string,int>();
    for(int i=0;i<v.Length;i++){var p=t.TransformPoint(v[i]);float st=rd.Project(p,out float lat);float rel=Mathf.Repeat(st-s0+rd.Length*.5f,rd.Length)-rd.Length*.5f;
     if(Mathf.Abs(rel)>100||lat>rd.HalfWidth(st)+14)continue;
     float best=float.NaN;string what=null;
     foreach(var h in Physics.RaycastAll(p+Vector3.up*3.05f,Vector3.down,3.7f,~0,QueryTriggerInteraction.Ignore)){if(!Cover(h.collider))continue;float d=h.point.y-p.y;if(d<-.6f||d>3f)continue;if(float.IsNaN(best)||Mathf.Abs(h.point.y-p.y)<Mathf.Abs(best-p.y)){best=h.point.y;what=h.collider.name;}}
     if(float.IsNaN(best))continue;float target=best-.03f,dy=target-p.y;if(Mathf.Abs(dy)<.005f)continue;
     if(dy<0){lowered++;maxDown=Mathf.Max(maxDown,-dy);}else{raised++;maxUp=Mathf.Max(maxUp,dy);}by[what]=by.TryGetValue(what,out var n)?n+1:1;
     v[i]=t.InverseTransformPoint(new Vector3(p.x,target,p.z));}
    sb.AppendLine($"  lowered {lowered} (largest {maxDown:F2} m), raised {raised} (largest {maxUp:F2} m) to 3 cm under: {string.Join(", ",by.Select(k=>k.Key+" "+k.Value))}{(dry?" (dry run)":"")}");
    if(!dry&&lowered+raised>0){mesh.vertices=v;mesh.RecalculateNormals();mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);sheet.sharedMesh=null;sheet.sharedMesh=mesh;}}
   if(!dry){AssetDatabase.SaveAssets();sb.AppendLine("saved");}
  }catch(Exception e){sb.AppendLine("ERROR "+e);}
  Directory.CreateDirectory(o);File.WriteAllText(o+"/C-lip.txt",sb.ToString());EditorApplication.Exit(0);}
}
