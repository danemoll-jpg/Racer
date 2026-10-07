using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 0.93 Part B (editor only, copied into Assets/Editor/Report093Temp while it runs): Dan's Backyard Forward, Abandoned Cabin
// Jump. The 0.43 "traversable dense undergrowth" (collider-free bushes under and beside the stunt, centred on the branch from
// 53 to 69 m, radius 18 m) covers the landing and run-out: every measured flight lands in it (61-106 m). This scene's
// brush gets its own copy of the mesh (Reverse and Free Roam keep the original) without every bush whose footprint reaches
// within ClearHalfWidth of the branch centre line from ClearFrom (the lip) on, and the component's matching cleared corridor
// (no slowing there). Bushes beside the corridor and every tree stay. Nothing else in the scene changes.
// ClearTreeTop: the same for Tree-Top Trail (its brush is centred on 39-83 m, radius 17 m), whose ground-level entry
// (20-24 m) and exit (88-96 m) reset points had brush on them: every bush within 3.5 m of its centre line whose top rises
// above the trail there (the ones under the raised boards stay).
public static class Report093Author {
 const string Scene="Assets/Scenes/DansBackyardForward.unity",Brush="Abandoned Cabin Jump traversable dense undergrowth",
  NewMesh="Assets/Track/BackyardShortcuts/Abandoned Cabin Jump dense brush (Forward, landing cleared).asset";
 const float ClearFrom=41,ClearHalfWidth=7;
 public static void ClearTreeTop(){
  string report="C:/Users/danmo/Racer/Docs/Report093/Lists/B-author-tree-top.txt";var lines=new List<string>();
  try{
   EditorSceneManager.OpenScene(Scene);
   var g=GameObject.Find("Tree-Top Trail traversable dense undergrowth");var under=g.GetComponent<Racer.ShortcutUndergrowth>();var filter=g.GetComponent<MeshFilter>();var source=filter.sharedMesh;
   var route=UnityEngine.Object.FindObjectsByType<Racer.WoodlandRoute>(FindObjectsSortMode.None).First(w=>w.title=="Tree-Top Trail");route.Initialize();
   under.clearRoute=route;under.clearFrom=0;under.clearHalfWidth=3.5f;
   var v=source.vertices;var t=source.triangles;var world=g.transform.localToWorldMatrix;
   var parent=Enumerable.Range(0,v.Length).ToArray();int Find(int i){while(parent[i]!=i){parent[i]=parent[parent[i]];i=parent[i];}return i;}
   for(int i=0;i<t.Length;i+=3){int a=Find(t[i]),b=Find(t[i+1]),c=Find(t[i+2]);parent[b]=a;parent[Find(c)]=a;}
   var used=new HashSet<int>(t);var pieces=Enumerable.Range(0,v.Length).Where(used.Contains).GroupBy(Find).ToList();var drop=new HashSet<int>();int kept=0;
   foreach(var piece in pieces){var pts=piece.Select(i=>world.MultiplyPoint3x4(v[i])).ToList();var c=pts.Aggregate(Vector3.zero,(x,y)=>x+y)/pts.Count;
    float reach=pts.Max(p=>new Vector2(p.x-c.x,p.z-c.z).magnitude),top=pts.Max(p=>p.y);
    if(under.Cleared(c,reach,top)){foreach(var i in piece)drop.Add(i);float s=route.Project(c,out float lat);lines.Add($"removed bush at {c.x:F1},{c.y:F1},{c.z:F1} reach {reach:F1} top {top:F1} (s {s:F1}, {lat:F1} m from the centre, trail at {route.At(s,out _).y:F1})");}else kept++;}
   var tri=new List<int>();for(int i=0;i<t.Length;i+=3)if(!drop.Contains(t[i]))tri.AddRange(new[]{t[i],t[i+1],t[i+2]});
   var mesh=UnityEngine.Object.Instantiate(source);mesh.name="Tree-Top Trail dense brush (Forward, reset points cleared)";mesh.SetTriangles(tri,0);mesh.RecalculateBounds();
   const string path="Assets/Track/BackyardShortcuts/Tree-Top Trail dense brush (Forward, reset points cleared).asset";AssetDatabase.CreateAsset(mesh,path);filter.sharedMesh=mesh;
   EditorUtility.SetDirty(under);EditorUtility.SetDirty(filter);EditorSceneManager.MarkSceneDirty(g.scene);EditorSceneManager.SaveScene(g.scene);AssetDatabase.SaveAssets();
   lines.Insert(0,$"{Scene}: Tree-Top Trail brush: {pieces.Count} bushes, {pieces.Count-kept} removed (within 3.5 m of the centre line and rising above the trail), {kept} kept; triangles {t.Length/3} -> {tri.Count/3}; mesh {path}");
   File.WriteAllLines(report,lines);EditorApplication.Exit(0);
  }catch(Exception e){lines.Add("FAILED "+e);File.WriteAllLines(report,lines);EditorApplication.Exit(1);}}
 public static void ClearLanding(){
  string report="C:/Users/danmo/Racer/Docs/Report093/Lists/B-author.txt";var lines=new List<string>();
  try{
   EditorSceneManager.OpenScene(Scene);
   var g=GameObject.Find(Brush);var under=g.GetComponent<Racer.ShortcutUndergrowth>();var filter=g.GetComponent<MeshFilter>();var source=filter.sharedMesh;
   var route=UnityEngine.Object.FindObjectsByType<Racer.WoodlandRoute>(FindObjectsSortMode.None).First(w=>w.title=="Abandoned Cabin Jump");route.Initialize();
   under.clearRoute=route;under.clearFrom=ClearFrom;under.clearHalfWidth=ClearHalfWidth;
   var v=source.vertices;var t=source.triangles;var world=g.transform.localToWorldMatrix;
   // the bushes = connected pieces of the mesh (union of triangle vertices)
   var parent=Enumerable.Range(0,v.Length).ToArray();int Find(int i){while(parent[i]!=i){parent[i]=parent[parent[i]];i=parent[i];}return i;}
   for(int i=0;i<t.Length;i+=3){int a=Find(t[i]),b=Find(t[i+1]),c=Find(t[i+2]);parent[b]=a;parent[Find(c)]=a;}
   var pieces=Enumerable.Range(0,v.Length).GroupBy(Find).ToList();var drop=new HashSet<int>();int kept=0;
   foreach(var piece in pieces){var pts=piece.Select(i=>world.MultiplyPoint3x4(v[i])).ToList();var c=pts.Aggregate(Vector3.zero,(x,y)=>x+y)/pts.Count;
    float reach=pts.Max(p=>new Vector2(p.x-c.x,p.z-c.z).magnitude);
    if(under.Cleared(c,reach)){foreach(var i in piece)drop.Add(i);float s=route.Project(c,out float lat);lines.Add($"removed bush at {c.x:F1},{c.y:F1},{c.z:F1} reach {reach:F1} (s {s:F1}, {lat:F1} m from the centre)");}else kept++;}
   var tri=new List<int>();for(int i=0;i<t.Length;i+=3)if(!drop.Contains(t[i]))tri.AddRange(new[]{t[i],t[i+1],t[i+2]});
   var mesh=UnityEngine.Object.Instantiate(source);mesh.name="Abandoned Cabin Jump dense brush (Forward, landing cleared)";mesh.SetTriangles(tri,0);mesh.RecalculateBounds();
   AssetDatabase.CreateAsset(mesh,NewMesh);filter.sharedMesh=mesh;
   EditorUtility.SetDirty(under);EditorUtility.SetDirty(filter);EditorSceneManager.MarkSceneDirty(g.scene);EditorSceneManager.SaveScene(g.scene);AssetDatabase.SaveAssets();
   lines.Insert(0,$"{Scene}: {Brush}: {pieces.Count} bushes, {pieces.Count-kept} removed within {ClearHalfWidth} m of the Abandoned Cabin Jump centre line from {ClearFrom} m to its end (rejoin), {kept} kept; triangles {t.Length/3} -> {tri.Count/3}; mesh {NewMesh}");
   Directory.CreateDirectory(Path.GetDirectoryName(report));File.WriteAllLines(report,lines);EditorApplication.Exit(0);
  }catch(Exception e){lines.Add("FAILED "+e);Directory.CreateDirectory(Path.GetDirectoryName(report));File.WriteAllLines(report,lines);EditorApplication.Exit(1);}}
}
