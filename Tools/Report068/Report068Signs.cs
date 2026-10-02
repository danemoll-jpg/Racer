using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
// 0.68 read-only: every sign board (renderer named *board*/*backing* or PhysicalSign) within 90 m of a point, with its
// text, facing and whether its text side faces a camera at the reported position. PROBE_SIGNS="scene|x,y,z|heading;..."
public static class Report068Signs {
 static string Path(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n.Replace('\n','/');}
 public static void Run(){var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(outDir);var rows=new List<string>();
  foreach(var spec in Environment.GetEnvironmentVariable("PROBE_SIGNS").Split(';')){var a=spec.Split('|');var v=a[1].Split(',').Select(float.Parse).ToArray();var p=new Vector3(v[0],v[1],v[2]);float hdg=float.Parse(a[2]);
   EditorSceneManager.OpenScene("Assets/Scenes/"+a[0]+".unity");var cam=p-Quaternion.Euler(0,hdg,0)*Vector3.forward*6.5f+Vector3.up*2.6f;var look=Quaternion.Euler(0,hdg,0)*Vector3.forward;rows.Add($"== {a[0]} {p} heading {hdg}");
   foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include)){var n=r.name.ToLower();if(!(n.Contains("board")||n.Contains("backing")||n.Contains("sign")))continue;var b=r.bounds;float d=Vector3.Distance(b.center,p);if(d>90)continue;
    var root=r.transform.parent;var texts=root?root.GetComponentsInChildren<TextMesh>(true):new TextMesh[0];string text=string.Join(" | ",texts.Select(t=>t.text.Replace('\n','/')));
    var toCam=Vector3.ProjectOnPlane(cam-b.center,Vector3.up).normalized;string face=texts.Length>0?string.Join(",",texts.Select(t=>Vector3.Dot(-t.transform.forward,toCam).ToString("F2"))):"-";
    float ahead=Vector3.Dot(b.center-cam,look);
    rows.Add($"{d,5:F1}m {(r.gameObject.activeInHierarchy?"A":"-")} {Path(r.transform)} centre {b.center:F1} size {b.size:F1} ahead {ahead:F0} | text '{text}' | textFacesCamera {face}");}}
  File.WriteAllLines(outDir+"/signs.txt",rows);EditorApplication.Exit(0);}
}
