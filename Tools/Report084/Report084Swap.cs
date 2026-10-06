using System;using System.IO;using System.Linq;using System.Text;using System.Collections.Generic;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;using Racer;using Object=UnityEngine.Object;
// 0.84 Part J (Dan, 2026-10-06: option 1): in ForestLoopReverse McFadden Cut becomes the main between the L04 fork and the L05
// rejoin, Granite Saddle (with the House 3 pool-and-lake jump) the optional line. Route data, gates, jumps, guidance and
// signs only: no mesh, terrain or collider changes. Survey(): read-only inventory.
public static class Report084Swap {
 static string P(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n.Replace('\n','/');}
 static string V(Vector3 v)=>$"{v.x:F1},{v.y:F1},{v.z:F1}";
 static float Dist(Vector3[] line,Vector3 q){float best=1e9f;for(int i=0;i+1<line.Length;i++){var a=line[i];var b=line[i+1];var ab=new Vector2(b.x-a.x,b.z-a.z);var aq=new Vector2(q.x-a.x,q.z-a.z);float t=Mathf.Clamp01(Vector2.Dot(aq,ab)/Mathf.Max(1e-6f,ab.sqrMagnitude));best=Mathf.Min(best,(aq-ab*t).magnitude);}return best;}
 public static void Survey(){var o=Environment.GetEnvironmentVariable("PROBE_OUT");var sb=new StringBuilder();
  try{EditorSceneManager.OpenScene("Assets/Scenes/ForestLoopReverse.unity");var race=Object.FindAnyObjectByType<RaceDirector>();var road=race.road;road.Initialize();
   var mcf=Object.FindObjectsByType<WoodlandRoute>(FindObjectsSortMode.None).Single(b=>b.title=="McFadden Cut");
   var granite=road.points.Where(p=>{float s=road.Project(p,out _);return s>=mcf.entryRoad-1&&s<=mcf.exitRoad+1;}).ToArray();var mc=mcf.points;
   sb.AppendLine($"McFadden {P(mcf.transform)} entry {mcf.entryRoad:F1} exit {mcf.exitRoad:F1} inset {mcf.entryInset} margin {mcf.entryMargin} hw {mcf.halfWidth} speed {mcf.recommendedSpeed} entrySpeed {mcf.entrySpeed}/{mcf.entrySpeedDistance} bypass [{string.Join(",",mcf.bypassedGates)}] ai {mcf.aiValidated} hb {mcf.entryHeightBelow}/{mcf.entryHeightAbove}; granite points {granite.Length}");
   bool Near(Vector3 p)=>Dist(granite,p)<25||Dist(mc,p)<25;
   foreach(var t in Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include,FindObjectsSortMode.None)){var p=t.transform.position;if(!Near(p)&&t.text.IndexOf("granite",StringComparison.OrdinalIgnoreCase)<0&&t.text.IndexOf("mcfadden",StringComparison.OrdinalIgnoreCase)<0&&t.text.IndexOf("detour",StringComparison.OrdinalIgnoreCase)<0)continue;
    sb.AppendLine($"TEXT \"{t.text.Replace("\n"," / ")}\" {P(t.transform)} at {V(p)} active {t.gameObject.activeInHierarchy} dG {Dist(granite,p):F1} dM {Dist(mc,p):F1} color {t.color}");}
   foreach(var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include,FindObjectsSortMode.None)){var n=r.name.ToLowerInvariant();var m=r.sharedMaterial?r.sharedMaterial.name:"";if(!(n.Contains("arrow")||n.Contains("chevron")||n.Contains("teal")||n.Contains("gold")||m.Contains("teal")||m.Contains("gold")))continue;var p=r.bounds.center;if(!Near(p))continue;
    sb.AppendLine($"ARROW {P(r.transform)} mat {m} at {V(p)} active {r.gameObject.activeInHierarchy&&r.enabled} dG {Dist(granite,p):F1} dM {Dist(mc,p):F1}");}
   foreach(var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include,FindObjectsSortMode.None)){var n=r.name.ToLowerInvariant();if(!(n.Contains("sign")||n.Contains("board")||n.Contains("barrier")||n.Contains("marker")))continue;var p=r.bounds.center;if(!Near(p))continue;
    sb.AppendLine($"SIGNLIKE {P(r.transform)} at {V(p)} dG {Dist(granite,p):F1} dM {Dist(mc,p):F1}");}
  }catch(Exception e){sb.AppendLine("ERROR "+e);}
  Directory.CreateDirectory(o);File.WriteAllText(o+"/J-survey.txt",sb.ToString());EditorApplication.Exit(0);}
 public static void Run(){bool dry=Environment.GetEnvironmentVariable("SWAP_DRY")=="1";var o=Environment.GetEnvironmentVariable("PROBE_OUT");var sb=new StringBuilder();
  try{var scene=EditorSceneManager.OpenScene("Assets/Scenes/ForestLoopReverse.unity");var race=Object.FindAnyObjectByType<RaceDirector>();var road=race.road;road.Initialize();Physics.SyncTransforms();
   var mcf=Object.FindObjectsByType<WoodlandRoute>(FindObjectsSortMode.None).Single(b=>b.title=="McFadden Cut");float entry=mcf.entryRoad,exit=mcf.exitRoad;
   var oldPoints=road.points.ToArray();var oldSt=new float[oldPoints.Length];for(int i=1;i<oldPoints.Length;i++)oldSt[i]=oldSt[i-1]+Vector3.Distance(oldPoints[i-1],oldPoints[i]);
   float OldGeo(int i)=>road.geometryStations!=null&&road.geometryStations.Length==oldPoints.Length?road.geometryStations[i]:oldSt[i];
   // the Granite Saddle line as it is now (its ends exactly at the two junctions)
   var granite=new[]{road.At(entry,out _)}.Concat(oldPoints.Where((p,i)=>oldSt[i]>entry&&oldSt[i]<exit)).Concat(new[]{road.At(exit,out _)}).ToArray();
   var mc=mcf.points.ToArray();
   var gatePos=race.gates.Select(g=>g.transform.position).ToArray();var gateSt=race.gates.Select(g=>road.Project(g.transform.position,out _)).ToArray();
   var others=Object.FindObjectsByType<WoodlandRoute>(FindObjectsSortMode.None).Where(b=>b!=mcf).ToArray();var otherEnds=others.Select(b=>(a:road.At(b.entryRoad,out _),b:road.At(b.exitRoad,out _))).ToArray();
   var layout=Object.FindAnyObjectByType<ForestLayout>();var jumps=layout.jumpStarts.Select((s,i)=>(a:s,b:layout.jumpEnds[i],name:layout.jumpNames[i],start:road.At(s,out _),end:road.At(layout.jumpEnds[i],out _))).ToArray();
   sb.AppendLine($"before: main {oldPoints.Length} points, length {road.Length:F1}; McFadden {mc.Length} points {mcf.Length:F1} m, stations {entry:F1}..{exit:F1}; Granite {granite.Length} points; course {race.courseId}");
   // 1. the main: McFadden between the junctions
   var pts=new List<Vector3>();var geo=new List<float>();float gl=road.geometryLength>0?road.geometryLength:road.Length;
   for(int i=0;i<oldPoints.Length;i++)if(oldSt[i]<entry){pts.Add(oldPoints[i]);geo.Add(OldGeo(i));}
   int first=Array.FindIndex(oldSt,x=>x>=entry),last=Array.FindIndex(oldSt,x=>x>=exit);float ga=OldGeo(first),gb=OldGeo(last);
   for(int i=0;i<mc.Length;i++){pts.Add(mc[i]);geo.Add(Mathf.Repeat(ga+Mathf.DeltaAngle(ga/gl*360,gb/gl*360)/360*gl*i/(mc.Length-1),gl));}
   for(int i=0;i<oldPoints.Length;i++)if(oldSt[i]>exit){pts.Add(oldPoints[i]);geo.Add(OldGeo(i));}
   road.points=pts.ToArray();road.geometryStations=geo.ToArray();road.geometryLength=gl;typeof(RaceRoad).GetField("distance",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)?.SetValue(road,null);road.Initialize();
   float nEntry=road.Project(mc[0],out _),nExit=road.Project(mc[^1],out _);
   sb.AppendLine($"after: main {road.points.Length} points, length {road.Length:F1}; McFadden now stations {nEntry:F1}..{nExit:F1}");
   // 2. gates on the swapped stretch go to the same fraction of the new main there; the order stays
   for(int i=1;i<race.gates.Length;i++)if(gateSt[i]>entry&&gateSt[i]<exit){float s=Mathf.Lerp(nEntry,nExit,Mathf.InverseLerp(entry,exit,gateSt[i]));var p=road.At(s,out var f);
     sb.AppendLine($"gate {i} {race.gates[i].name}: {V(gatePos[i])} (old station {gateSt[i]:F1}) -> {V(p+Vector3.up*1.6f)} (station {s:F1}), same order");
     race.gates[i].transform.SetPositionAndRotation(p+Vector3.up*1.6f,Quaternion.LookRotation(Vector3.ProjectOnPlane(f,Vector3.up)));}
   // 3. Granite Saddle becomes the optional branch (the same component, renamed); AI never takes it
   var g=mcf;g.gameObject.name="Granite Saddle";g.title="Granite Saddle";g.points=granite;g.aiValidated=false;
   typeof(WoodlandRoute).GetField("lengths",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)?.SetValue(g,null);g.Initialize();
   g.entryRoad=road.Project(granite[0],out _);g.exitRoad=road.Project(granite[^1],out _);g.entryInset=24;
   for(float s=24;s<120;s+=2){var p=g.At(s,out _);float ms=road.Project(p,out float d);if(d>road.HalfWidth(ms)+g.halfWidth+4){g.entryInset=s;break;}}
   for(int k=0;k<others.Length;k++){others[k].entryRoad=road.Project(otherEnds[k].a,out _);others[k].exitRoad=road.Project(otherEnds[k].b,out _);}
   foreach(var b in others.Concat(new[]{g})){
    b.bypassedGates=Enumerable.Range(1,race.gates.Length-1).Where(k=>road.Relative(road.Project(race.gates[k].transform.position,out _),b.entryRoad)<road.Relative(b.exitRoad,b.entryRoad)).ToArray();EditorUtility.SetDirty(b);
    sb.AppendLine($"branch {b.title}: entry {b.entryRoad:F1} exit {b.exitRoad:F1} length {b.Length:F1} inset {b.entryInset} bypasses [{string.Join(",",b.bypassedGates)}] ai {b.aiValidated}");}
   // 4. jumps: the pool-and-lake flight is on Granite now (not the main); the rest remapped
   var kept=jumps.Where(j=>!(j.a>entry&&j.a<exit)).ToArray();layout.jumpStarts=kept.Select(j=>road.Project(j.start,out _)).ToArray();layout.jumpEnds=kept.Select(j=>road.Project(j.end,out _)).ToArray();layout.jumpNames=kept.Select(j=>j.name).ToArray();EditorUtility.SetDirty(layout);
   sb.AppendLine("main jumps: "+string.Join("; ",kept.Select((j,i)=>$"{j.name} {layout.jumpStarts[i]:F1}->{layout.jumpEnds[i]:F1}"))+$"; moved off the main: {string.Join(", ",jumps.Where(j=>j.a>entry&&j.a<exit).Select(j=>j.name))}");
   // 5. guidance: arrows only on Granite (more than 3 m from McFadden) gold; the McFadden gold arrows teal
   var teal=AssetDatabase.LoadAssetAtPath<Material>("Assets/Track/Discovery/CR117 main teal.mat");var gold=AssetDatabase.LoadAssetAtPath<Material>("Assets/Track/Discovery/CR117 alternate gold.mat");
   foreach(var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include,FindObjectsSortMode.None)){var c=r.bounds.center;
    if(r.sharedMaterial==teal&&r.name.StartsWith("Main teal")&&Dist(granite,c)<1.5f&&Dist(mc,c)>3){sb.AppendLine($"arrow {P(r.transform)} at {V(c)}: teal -> gold (Granite Saddle)");r.sharedMaterial=gold;r.gameObject.name="Optional gold / Granite Saddle";EditorUtility.SetDirty(r.gameObject);}
    else if(r.sharedMaterial==gold&&r.transform.parent&&r.transform.parent.name=="House 3 Detour optional visual guidance"){sb.AppendLine($"arrow {P(r.transform)} at {V(c)}: gold -> teal (McFadden Cut main)");r.sharedMaterial=teal;r.gameObject.name="Main teal / McFadden Cut";EditorUtility.SetDirty(r.gameObject);}}
   var guide=GameObject.Find("House 3 Detour optional visual guidance");if(guide)guide.name="McFadden Cut main guidance";
   // 6. signs
   void Text(string from,string to){foreach(var t in Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(t=>t.text==from)){sb.AppendLine($"sign {P(t.transform)}: \"{from.Replace("\n"," / ")}\" -> \"{to.Replace("\n"," / ")}\"");t.text=to;EditorUtility.SetDirty(t);}}
   Text("McFADDEN CUT\nOPTIONAL","McFADDEN CUT\nMAIN COURSE");Text("REVERSE\nMAIN COURSE","GRANITE SADDLE\nOPTIONAL");foreach(var t in Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(t=>t.text.Contains("Granite Saddle")&&t.text.Contains("MAIN ROUTE"))){sb.AppendLine($"sign {P(t.transform)}: \"{t.text.Replace("\n"," / ")}\": MAIN ROUTE -> OPTIONAL");t.text=t.text.Replace("MAIN ROUTE","OPTIONAL");EditorUtility.SetDirty(t);}Text("MAIN COURSE\nGRANITE SADDLE","MAIN COURSE\nMcFADDEN CUT");
   foreach(var t in Object.FindObjectsByType<TextMesh>(FindObjectsSortMode.None).Where(t=>t.text=="^"&&Dist(granite,t.transform.position)<2&&Dist(mc,t.transform.position)>4).ToList()){var board=t.transform.parent?t.transform.parent.gameObject:t.gameObject;sb.AppendLine($"cue {P(board.transform)} at {V(board.transform.position)}: MAIN ^ on the Granite side, hidden");board.SetActive(false);EditorUtility.SetDirty(board);}
   // 7. records
   sb.AppendLine($"course id {race.courseId} -> forest-reverse-v8-mcfadden-main");race.courseId="forest-reverse-v8-mcfadden-main";EditorUtility.SetDirty(race);EditorUtility.SetDirty(road);
   if(!dry){EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();}else sb.AppendLine("(dry run: nothing saved)");
  }catch(Exception e){sb.AppendLine("ERROR "+e);}
  Directory.CreateDirectory(o);File.WriteAllText(o+"/J-swap.txt",sb.ToString());EditorApplication.Exit(0);}
}
