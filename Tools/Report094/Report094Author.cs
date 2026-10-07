using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 0.94 Part D (editor only, copied into Assets/Editor/Report094Temp while it runs): Dan's Backyard Forward, Abandoned Cabin
// Jump. 0.93 removed every bush within 7 m of the branch from the lip (41 m) to the rejoin. Dan: put them back, but only as
// far as a well-hit jump clears. This scene's brush gets a new copy of the ORIGINAL 0.43 mesh without the bushes whose
// footprint reaches within ClearHalfWidth of the centre line from FarEdge to the rejoin (the bushes from the lip to the far
// edge are back at their original positions); the component's cleared corridor starts at FarEdge (slowing restored before
// it) and brushFrom = the lip (the reset from inside the brush). Reverse, Free Roam and Tree-Top Trail unchanged.
public static class Report094Author {
 const string Scene="Assets/Scenes/DansBackyardForward.unity",Brush="Abandoned Cabin Jump traversable dense undergrowth",
  Original="91b6615e142686f4f88dd9a12cac700e",Old="Assets/Track/BackyardShortcuts/Abandoned Cabin Jump dense brush (Forward, landing cleared).asset",
  NewMesh="Assets/Track/BackyardShortcuts/Abandoned Cabin Jump dense brush (Forward, cleared past the far edge).asset";
 const float Lip=41,ClearHalfWidth=7;
 static float FarEdge=>float.Parse(Environment.GetEnvironmentVariable("FAR_EDGE")??"84",System.Globalization.CultureInfo.InvariantCulture);
 public static void PartClear(){
  string report="C:/Users/danmo/Racer/Docs/Report094/Lists/D-author.txt";var lines=new List<string>();
  try{
   EditorSceneManager.OpenScene(Scene);
   var g=GameObject.Find(Brush);var under=g.GetComponent<Racer.ShortcutUndergrowth>();var filter=g.GetComponent<MeshFilter>();
   var source=AssetDatabase.LoadAssetAtPath<Mesh>(AssetDatabase.GUIDToAssetPath(Original));if(!source)throw new Exception("original 0.43 brush mesh not found");
   var route=UnityEngine.Object.FindObjectsByType<Racer.WoodlandRoute>(FindObjectsSortMode.None).First(w=>w.title=="Abandoned Cabin Jump");route.Initialize();
   under.clearRoute=route;under.clearFrom=FarEdge;under.clearHalfWidth=ClearHalfWidth;under.brushFrom=Lip;
   var v=source.vertices;var t=source.triangles;var world=g.transform.localToWorldMatrix;
   var parent=Enumerable.Range(0,v.Length).ToArray();int Find(int i){while(parent[i]!=i){parent[i]=parent[parent[i]];i=parent[i];}return i;}
   for(int i=0;i<t.Length;i+=3){int a=Find(t[i]),b=Find(t[i+1]),c=Find(t[i+2]);parent[b]=a;parent[Find(c)]=a;}
   var pieces=Enumerable.Range(0,v.Length).GroupBy(Find).ToList();var drop=new HashSet<int>();int kept=0,restored=0;float last=0;
   foreach(var piece in pieces){var pts=piece.Select(i=>world.MultiplyPoint3x4(v[i])).ToList();var c=pts.Aggregate(Vector3.zero,(x,y)=>x+y)/pts.Count;
    float reach=pts.Max(p=>new Vector2(p.x-c.x,p.z-c.z).magnitude);float s=route.Project(c,out float lat);var q=route.At(s,out _);float off=new Vector2(c.x-q.x,c.z-q.z).magnitude;
    if(under.Cleared(c,reach)){foreach(var i in piece)drop.Add(i);lines.Add($"removed bush at {c.x:F1},{c.y:F1},{c.z:F1} reach {reach:F1} (s {s:F1}, {lat:F1} m from the centre)");}
    else{kept++;if(s>=Lip&&off-reach<ClearHalfWidth&&c.y>q.y-.5f){restored++;last=Mathf.Max(last,s+reach);}}}
   var tri=new List<int>();for(int i=0;i<t.Length;i+=3)if(!drop.Contains(t[i]))tri.AddRange(new[]{t[i],t[i+1],t[i+2]});
   var mesh=UnityEngine.Object.Instantiate(source);mesh.name="Abandoned Cabin Jump dense brush (Forward, cleared past the far edge)";mesh.SetTriangles(tri,0);mesh.RecalculateBounds();
   AssetDatabase.CreateAsset(mesh,NewMesh);filter.sharedMesh=mesh;
   EditorUtility.SetDirty(under);EditorUtility.SetDirty(filter);EditorSceneManager.MarkSceneDirty(g.scene);EditorSceneManager.SaveScene(g.scene);
   if(AssetDatabase.LoadAssetAtPath<Mesh>(Old))AssetDatabase.DeleteAsset(Old);AssetDatabase.SaveAssets();
   lines.Insert(0,$"{Scene}: {Brush}: from the original 0.43 mesh ({pieces.Count} bushes): {pieces.Count-kept} removed within {ClearHalfWidth} m of the Abandoned Cabin Jump centre line from the far edge {FarEdge} m to its end (rejoin), {kept} kept, of which {restored} in the corridor from the lip ({Lip} m) to the far edge; the last bush reaches s {last:F1}; triangles {t.Length/3} -> {tri.Count/3}; mesh {NewMesh} (0.93's {Old} deleted)");
   Directory.CreateDirectory(Path.GetDirectoryName(report));File.WriteAllLines(report,lines);EditorApplication.Exit(0);
  }catch(Exception e){lines.Add("FAILED "+e);Directory.CreateDirectory(Path.GetDirectoryName(report));File.WriteAllLines(report,lines);EditorApplication.Exit(1);}}
}
