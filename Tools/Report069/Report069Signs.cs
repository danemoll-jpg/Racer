using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
// 0.69 read-only: hierarchy of the Part D signs (and the Fence Line Smash activity) in every course scene.
public static class Report069Signs {
 public static readonly string[] Scenes={"StreetLoopGreybox","StreetLoopReverse","LakeWoods","ForestLoopReverse","DansBackyardForward","DansBackyardReverse","MountainLoop","MountainLoopReverse"};
 static string Path(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n.Replace('\n','/');}
 public static bool Match(string s)=>s.Contains("FENCE LINE SMASH")||(s.Contains("ANDERSON'S")&&s.Contains("MOUNTAIN TRAILS"))||s.Contains("BOTH TRAILS RETURN HOME");
 public static void Run(){var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(outDir);var rows=new List<string>();
  foreach(var scene in Scenes){EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");rows.Add("== "+scene);
   foreach(var t in Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include)){if(!Match(t.text))continue;var sign=t.GetComponentInParent<PhysicalSign>(true);var root=sign?sign.transform:t.transform.parent;
    rows.Add($"TEXT '{t.text.Replace('\n','/')}' at {t.transform.position:F2} path {Path(t.transform)} | root {Path(root)} physicalSign={(bool)sign} active={root.gameObject.activeInHierarchy}");
    foreach(var c in root.GetComponentsInChildren<Transform>(true)){var r=c.GetComponent<Renderer>();rows.Add($"   {Path(c)} comps={string.Join(",",c.GetComponents<Component>().Where(x=>x&&!(x is Transform)).Select(x=>x.GetType().Name))} {(r?$"b={r.bounds.center:F2} sz={r.bounds.size:F2}":"")}");}
    var rp=root.parent;rows.Add($"   parent {(rp?Path(rp):"-")} comps={(rp?string.Join(",",rp.GetComponents<Component>().Where(x=>x&&!(x is Transform)).Select(x=>x.GetType().Name)):"-")} children={(rp?rp.childCount:0)}");}
   foreach(var a in Object.FindObjectsByType<ActivitySite>(FindObjectsInactive.Include).Where(a=>a.title!=null&&a.title.ToUpper().Contains("FENCE")))rows.Add($"ACTIVITY '{a.title}' {Path(a.transform)} at {a.transform.position:F2}");}
  File.WriteAllLines(outDir+"/signs.txt",rows);EditorApplication.Exit(0);}
}
