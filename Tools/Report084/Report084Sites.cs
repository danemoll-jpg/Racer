using System;using System.IO;using System.Linq;using System.Text;using System.Collections.Generic;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;using Racer;using Object=UnityEngine.Object;
// 0.84 Part C: the Forest opening jump and Forest speed traps 1 and 2 lived on race-only Forest trails, so Free Roam never had
// them. They get homes in FreeRoamWorld on ground that is already there, where a roaming player sees them (no new terrain or
// ramps). Their course-scene sites are unchanged (and keep their results there); in Free Roam the ids jump-01 / speed-0 /
// speed-1 are already the Trickum sites, so these are forest-jump-01 / forest-speed-0 / forest-speed-1. Each also becomes a
// map destination (found within 45 m, like the others). SITES_DRY=1 reports only. Targets: SITES_TARGETS="j:b,s,g;s0:...;s1:..."
public static class Report084Sites {
 const string Root="0.84 relocated activity sites";
 static string V(Vector3 v)=>$"({v.x:F1}, {v.y:F2}, {v.z:F1})";
 static Vector3 Ground(Vector3 p){var hits=Physics.RaycastAll(new Vector3(p.x,p.y+40,p.z),Vector3.down,120,~0,QueryTriggerInteraction.Ignore).Where(h=>!h.collider.attachedRigidbody&&h.collider.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)<0).OrderBy(h=>Mathf.Abs(h.point.y-p.y)).ToArray();return hits.Length>0?hits[0].point:p;}
 static void Sign(Transform root,Vector3 p,Vector3 f,string words){
  p=Ground(p);var sign=new GameObject(words).transform;sign.SetParent(root);sign.SetPositionAndRotation(p,Quaternion.LookRotation(Vector3.ProjectOnPlane(f,Vector3.up)));sign.gameObject.AddComponent<PhysicalSign>();
  var mat=AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Phase4Ramp.mat");
  void Box(Vector3 pos,Vector3 size){var b=GameObject.CreatePrimitive(PrimitiveType.Cube);b.transform.SetParent(sign,false);b.transform.localPosition=pos;b.transform.localScale=size;b.GetComponent<Renderer>().sharedMaterial=mat;Object.DestroyImmediate(b.GetComponent<Collider>());}
  Box(new(0,2.8f,0),new(7,1.6f,.14f));Box(new(0,1.4f,.08f),new(.18f,2.8f,.18f));
  var label=new GameObject("Mounted direction lettering").AddComponent<TextMesh>();label.transform.SetParent(sign,false);label.transform.localPosition=new(0,2.8f,-.09f);label.text=words;label.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");label.fontSize=64;label.anchor=TextAnchor.MiddleCenter;label.alignment=TextAlignment.Center;label.color=Color.white;
  label.characterSize=Mathf.Min(.1f,1.5f/words.Split('\n').Max(line=>line.Length));
  label.GetComponent<Renderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Environment/Phase8/Depth tested world lettering 0.mat");}
 public static void Run(){bool dry=Environment.GetEnvironmentVariable("SITES_DRY")=="1";var o=Environment.GetEnvironmentVariable("PROBE_OUT");var sb=new StringBuilder();
  var targets=(Environment.GetEnvironmentVariable("SITES_TARGETS")??"").Split(';').Where(x=>x.Contains(':')).ToDictionary(x=>x.Split(':')[0],x=>x.Split(':')[1].Split(',').Select(v=>float.Parse(v,System.Globalization.CultureInfo.InvariantCulture)).ToArray());
  float[] T(string k,float b,float s,float g)=>targets.TryGetValue(k,out var t)?t:new[]{b,s,g};
  try{var scene=EditorSceneManager.OpenScene("Assets/Scenes/FreeRoamWorld.unity");Physics.SyncTransforms();
   var old=GameObject.Find(Root);if(old)Object.DestroyImmediate(old);var root=new GameObject(Root).transform;
   var roads=Object.FindObjectsByType<RaceRoad>(FindObjectsSortMode.None).Where(r=>r.points!=null&&r.points.Length>1).ToList();foreach(var r in roads)r.Initialize();
   var street=roads.Single(r=>r.name=="Phase 3 - Race Systems");var hwy=roads.Single(r=>r.name=="CR113 highway through route");
   ActivitySite Site(string id,string title,ActivitySite.Kind kind,Vector3 p,Vector3 f,float radius,float[] t){
    var site=new GameObject(title).AddComponent<ActivitySite>();site.transform.SetParent(root);site.transform.position=p;site.id=id;site.title=title;site.kind=kind;site.forward=f;site.radius=radius;site.bronze=t[0];site.silver=t[1];site.gold=t[2];site.bothDirections=kind==ActivitySite.Kind.Speed;site.legacyRecords=new string[0];
    sb.AppendLine($"SITE {id} \"{title}\" {kind} at {V(p)} forward {V(f)} radius {radius:F1} targets {t[0]}/{t[1]}/{t[2]} {(kind==ActivitySite.Kind.Speed?"m/s":"m")}");return site;}
   var dests=new List<ExplorationMap.Destination>();
   // speed trap 1: S Cherokee Ln, half way down the straight 200 m hill north of Dan's house (both directions)
   {float s=150;var p=street.At(s,out var f);f.y=0;f.Normalize();var site=Site("forest-speed-0","S Cherokee hill speed trap",ActivitySite.Kind.Speed,p,f,street.HalfWidth(s)+1,T("s0",18,26,34));site.Seconds=45;
    var side=Vector3.Cross(Vector3.up,f);Sign(root,p+side*(site.radius+2),f,"SPEED CAMERA\nBOTH DIRECTIONS");Sign(root,p-side*(site.radius+2),-f,"SPEED CAMERA\nBOTH DIRECTIONS");dests.Add(new ExplorationMap.Destination{id="forest-speed-0",title="S Cherokee speed trap",position=p,yaw=Mathf.Atan2(f.x,f.z)*Mathf.Rad2Deg});}
   // speed trap 2: Hwy 92 east of the S Cherokee junction, on the long straight (both directions)
   {float s=hwy.Project(new Vector3(481,19,561),out _);var p=hwy.At(s,out var f);f.y=0;f.Normalize();var site=Site("forest-speed-1","Hwy 92 east speed trap",ActivitySite.Kind.Speed,p,f,hwy.HalfWidth(s)+1,T("s1",20,28,36));site.Seconds=45;
    var side=Vector3.Cross(Vector3.up,f);Sign(root,p+side*(site.radius+2),f,"SPEED CAMERA\nBOTH DIRECTIONS");Sign(root,p-side*(site.radius+2),-f,"SPEED CAMERA\nBOTH DIRECTIONS");dests.Add(new ExplorationMap.Destination{id="forest-speed-1",title="Hwy 92 speed trap",position=p,yaw=Mathf.Atan2(f.x,f.z)*Mathf.Rad2Deg});}
   // jump: the Backyard pool-house flight beside Dan's house (Dan's Backyard Forward launch, flying east)
   {var course=Object.FindAnyObjectByType<BackyardForwardCourse>();var trail=roads.Single(r=>r.name=="Reverse navigation only - no pavement");var lip=trail.At(course.launchStations[2],out var f);f.y=0;f.Normalize();
    var site=Site("forest-jump-01","Pool-house jump",ActivitySite.Kind.Jump,lip,f,32,T("j",12,22,32));site.Seconds=180;
    Sign(root,lip-f*38+Vector3.Cross(Vector3.up,f)*8,f,"POOL-HOUSE JUMP\nLAND CLEAN / PAUSE FOR TARGETS");dests.Add(new ExplorationMap.Destination{id="forest-jump-01",title="Pool-house jump",position=lip,yaw=Mathf.Atan2(f.x,f.z)*Mathf.Rad2Deg});}
   var map=Object.FindAnyObjectByType<ExplorationMap>();map.destinations=map.destinations.Where(d=>!d.id.StartsWith("forest-")).Concat(dests).ToArray();EditorUtility.SetDirty(map);
   sb.AppendLine("map destinations now: "+string.Join(", ",map.destinations.Select(d=>d.title)));
   if(!dry){EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);}else sb.AppendLine("(dry run: nothing saved)");
  }catch(Exception e){sb.AppendLine("ERROR "+e);}
  Directory.CreateDirectory(o);File.WriteAllText(o+"/C-sites.txt",sb.ToString());EditorApplication.Exit(0);}
}
