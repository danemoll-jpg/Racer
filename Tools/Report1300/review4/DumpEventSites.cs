using System.IO;using System.Linq;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// Temporary (1.3.0-review4): writes Resources/WorldMaps/EventSites.json, the one-spot campaign events' sites as saved in their course scenes.
public static class DumpEventSites { public static void Main(){
 var want=new[]{("StreetLoopGreybox","speed-0"),("LakeWoods","jump-01"),("DansBackyardForward","smash-01"),("MountainLoop","summit-homeward")};
 var rows=new System.Collections.Generic.List<string>();
 foreach(var (scene,id) in want){EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");var s=Object.FindObjectsByType<Racer.ActivitySite>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(x=>x.id==id);var p=s.transform.position;var f=s.transform.TransformDirection(s.forward);
  rows.Add($"{{\"scene\":\"{scene}\",\"id\":\"{id}\",\"title\":\"{s.title}\",\"kind\":\"{s.kind}\",\"position\":{{\"x\":{p.x:0.##},\"y\":{p.y:0.##},\"z\":{p.z:0.##}}},\"forward\":{{\"x\":{f.x:0.###},\"y\":{f.y:0.###},\"z\":{f.z:0.###}}},\"local\":{{\"x\":{s.forward.x:0.###},\"y\":{s.forward.y:0.###},\"z\":{s.forward.z:0.###}}},\"both\":{(s.bothDirections?"true":"false")}}}");}
 File.WriteAllText(System.Environment.GetEnvironmentVariable("PROBE_OUT")+"/sites.json","{\"sites\":[\n"+string.Join(",\n",rows)+"\n]}");EditorApplication.Exit(0);}}
