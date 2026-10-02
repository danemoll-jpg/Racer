using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
public static class RegressionMembrane {
 public static void Run(){var rows=new List<string>();EditorSceneManager.OpenScene("Assets/Scenes/MountainLoopReverse.unity");
  var road=Object.FindAnyObjectByType<RaceDirector>(FindObjectsInactive.Include).road;road.Initialize();
  foreach(var name in new[]{"Ground_ReportCleanup MountainLoopReverse continuous solid 2"}){var mf=GameObject.Find(name).GetComponent<MeshFilter>();var m=mf.sharedMesh;var v=m.vertices;var t=m.triangles;rows.Add(name+" tris="+t.Length/3+" pos="+mf.transform.position+" rot="+mf.transform.rotation.eulerAngles);
   for(float s=1700;s<=1780;s+=1){var p=road.At(s,out var f);var r=Vector3.Cross(Vector3.up,f).normalized;int n=0;float ymin=999,ymax=-999,lmin=999,lmax=-999;
    for(int i=0;i<t.Length;i+=3){var c=mf.transform.TransformPoint((v[t[i]]+v[t[i+1]]+v[t[i+2]])/3);var d=c-p;float along=Vector3.Dot(Vector3.ProjectOnPlane(d,Vector3.up),Vector3.ProjectOnPlane(f,Vector3.up).normalized);if(Mathf.Abs(along)>.5f)continue;float lat=Vector3.Dot(d,r);if(Mathf.Abs(lat)>14)continue;if(d.y<-.3f)continue;n++;ymin=Mathf.Min(ymin,d.y);ymax=Mathf.Max(ymax,d.y);lmin=Mathf.Min(lmin,lat);lmax=Mathf.Max(lmax,lat);}
    rows.Add($"s={s} p={p.ToString("F1")} tris_in_slice={n} relY=[{ymin:F1},{ymax:F1}] lat=[{lmin:F1},{lmax:F1}]");}}
  File.WriteAllLines(Environment.GetEnvironmentVariable("PROBE_OUT")+"/membrane.txt",rows);EditorApplication.Exit(0);}
}
