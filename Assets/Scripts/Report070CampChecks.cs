#if UNITY_EDITOR
using System;using System.IO;using System.Linq;using System.Collections;using System.Collections.Generic;using System.Reflection;using UnityEngine;
namespace Racer {
// 0.70 Part D check (muted, isolated save): the Free Roam "Campsite" landmark is an ordinary map destination - discovered
// within 45 m, then free-roam fast travel arrives beside the camp on supported ground. The player's real map save is never touched.
public sealed class Report070CampChecks:MonoBehaviour {
 readonly List<string> rows=new();string output;
 void Check(bool ok,string label){rows.Add((ok?"PASS ":"FAIL ")+label);File.WriteAllLines(output+"/camp-checks.txt",rows);Debug.Log("REPORT070 "+rows[^1]);}
 IEnumerator Start(){AudioListener.volume=0;output=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(output);Application.runInBackground=true;var race=FindAnyObjectByType<RaceDirector>();yield return null;yield return null;
  var flow=race.Flow;flow.UseValidationSave(Path.GetFullPath("Temp/Report070Save"));
  var title=FindAnyObjectByType<StartupTitle>();if(title)Destroy(title.gameObject);typeof(StartupTitle).GetField("completed",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,true);flow.EnterMenuAfterTitle();yield return null;
  if(flow.State!=RaceFlow.Stage.Ready){flow.Pause();flow.QuitRace();yield return null;}
  flow.StartFreeRoam();while(flow.State!=RaceFlow.Stage.Racing){AudioListener.volume=0;yield return null;}yield return new WaitForSeconds(.5f);
  // The scene's map was initialized against the player's real save before validation storage could be selected, so it is
  // disabled here (no reveal, no save) and an isolated fixture map with the same destinations is used instead.
  var real=race.GetComponent<ExplorationMap>();real.enabled=false;var acorns=race.GetComponent<ExplorationCollection>();if(acorns)acorns.enabled=false;var root=Path.GetFullPath("Temp/Report070Save/isolated-map");if(Directory.Exists(root))Directory.Delete(root,true);Directory.CreateDirectory(root);
  var map=new GameObject("Report070 isolated map").AddComponent<ExplorationMap>();map.destinations=real.destinations;map.Initialize(race,root);int i=Array.FindIndex(map.destinations,d=>d.id=="campsite");
  Check(i>=0&&map.destinations[i].title=="Campsite",$"Campsite is a map destination ({map.destinations.Length} destinations)");
  var camp=GameObject.Find("Permanent mountainside camp / two seated guys");Check(camp&&Vector3.Distance(camp.transform.position,new Vector3(989f,163.81f,75f))<.01f,$"Free Roam camp not moved ({(camp?camp.transform.position.ToString("F2"):"missing")})");
  Check(!map.Discovered("campsite"),"Not discovered before visiting");
  map.Reveal(map.destinations[i].position+new Vector3(30,0,0));Check(map.Discovered("campsite"),"Discovered within 45 m (exploration reveal)");
  bool ok=map.Travel(i);yield return new WaitForSeconds(1f);var p=race.vehicle.Body.position;bool ground=Physics.Raycast(p+Vector3.up,Vector3.down,out var h,4,~0,QueryTriggerInteraction.Ignore);
  Check(ok&&Vector3.Distance(new Vector3(p.x,0,p.z),new Vector3(982,0,76))<4&&ground,$"Fast travel to Campsite arrives at {p:F1} on '{(ground?h.collider.name:"nothing")}', {Vector3.Distance(p,camp.transform.position):F1} m from the camp, up {race.vehicle.transform.up.y:F2}");
  File.WriteAllLines(output+"/done.txt",rows);UnityEditor.EditorApplication.Exit(rows.Any(r=>r.StartsWith("FAIL"))?1:0);}
}
}
#endif
