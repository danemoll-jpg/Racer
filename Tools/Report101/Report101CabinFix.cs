using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 0.101 Part A follow-up steps after Report101Cabin (the ground is already graded; nothing here touches it):
//  CABIN_PART=decal  the run-up decal with a clean edge along the main road's verge (both scenes)
//  CABIN_PART=brush  the brush mesh and the undergrowth's far edge (FAR_EDGE, stations of the new line) in both scenes
public static class Report101CabinFix {
 static string Out=>Environment.GetEnvironmentVariable("PROBE_OUT");
 const string Dir="Assets/Track/BackyardShortcuts/",NewDir="Assets/Scenery/Report101/",Original="91b6615e142686f4f88dd9a12cac700e";
 static readonly Vector3 lip=new(224.61f,74.15f,80.95f);static readonly float heading=82.82f,aC=-44.72f,aF=-19.00f,lipS=44.72f,ClearHalf=7f;
 static Vector3 d=>new(Mathf.Sin(heading*Mathf.Deg2Rad),0,Mathf.Cos(heading*Mathf.Deg2Rad));static Vector3 n=>new(-d.z,0,d.x);
 static float TopGround(Vector3 p){float best=float.NaN;foreach(var h in Physics.RaycastAll(new Vector3(p.x,p.y+40,p.z),Vector3.down,120,~0,QueryTriggerInteraction.Ignore))if(h.collider.name.StartsWith("Ground_")&&(float.IsNaN(best)||h.point.y>best))best=h.point.y;return best;}
 public static void Run(){
  var log=new List<string>();string part=Environment.GetEnvironmentVariable("CABIN_PART");float far=float.Parse(Environment.GetEnvironmentVariable("FAR_EDGE")??"87.8",System.Globalization.CultureInfo.InvariantCulture);
  try{
  foreach(var sn in new[]{"DansBackyardForward","FreeRoamWorld"}){
   var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+sn+".unity");Physics.SyncTransforms();log.Add("===== "+sn);var root=GameObject.Find("Backyard optional forest shortcuts").transform;
   var main=UnityEngine.Object.FindObjectsByType<Racer.RaceRoad>(FindObjectsSortMode.None).First(r=>r.name=="Forward navigation only - no road mesh");main.Initialize();
   float MainLat(Vector3 p){var q=p;float s=main.Project(q,out _);q.y=main.At(s,out _).y;s=main.Project(q,out _);var c=main.At(s,out _);return new Vector2(p.x-c.x,p.z-c.z).magnitude-main.HalfWidth(s);}
   if(part=="decal"){
    var old=root.Find("Cabin wooded approach");var mat=old.GetComponent<MeshRenderer>().sharedMaterial;
    var vs=new List<Vector3>();var ts=new List<int>();const int cols=15,rows=50;
    for(int c=0;c<cols;c++){float l=-2.8f+c*(5.6f/(cols-1));float a0=aC;while(a0<aF-2&&MainLat(lip+d*a0+n*l)<.25f)a0+=.05f;
     for(int r=0;r<rows;r++){float a=Mathf.Lerp(a0,aF,r/(rows-1f));var p=lip+d*a+n*l;float g=TopGround(p);if(float.IsNaN(g))throw new Exception("no ground under the decal at "+p);p.y=g+.03f;vs.Add(p);}}
    for(int c=1;c<cols;c++)for(int r=1;r<rows;r++){int a=(c-1)*rows+r-1,b=a+1,e=c*rows+r-1,f=e+1;ts.AddRange(new[]{a,b,e,b,f,e});}
    var m=new Mesh{name="Cabin wooded approach (0.101) "+sn};m.SetVertices(vs);m.SetTriangles(ts,0);m.RecalculateNormals();if(m.normals[0].y<0){for(int k=0;k<ts.Count;k+=3)(ts[k+1],ts[k+2])=(ts[k+2],ts[k+1]);m.SetTriangles(ts,0);m.RecalculateNormals();}m.RecalculateBounds();
    var path=NewDir+sn+"-Cabin wooded approach.asset";var ex=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(ex){EditorUtility.CopySerialized(m,ex);m=ex;}else AssetDatabase.CreateAsset(m,path);
    old.GetComponent<MeshFilter>().sharedMesh=m;EditorUtility.SetDirty(old.gameObject);log.Add($"decal: {ts.Count/3} triangles, each lateral column starting at the main's verge (+0.25 m), material {mat.name}");}
   if(part=="brush"){
    var g=root.Find("Abandoned Cabin Jump traversable dense undergrowth").gameObject;var under=g.GetComponent<Racer.ShortcutUndergrowth>();var line=under.clearRoute;line.Initialize();
    under.clearFrom=far;EditorUtility.SetDirty(under);var path=Dir+"Abandoned Cabin Jump dense brush (0.101).asset";
    var source=AssetDatabase.LoadAssetAtPath<Mesh>(AssetDatabase.GUIDToAssetPath(Original));var bv=source.vertices;var bt=source.triangles;var world=g.transform.localToWorldMatrix;var parent=Enumerable.Range(0,bv.Length).ToArray();int Find(int i){while(parent[i]!=i){parent[i]=parent[parent[i]];i=parent[i];}return i;}
    for(int i=0;i<bt.Length;i+=3){int a=Find(bt[i]),b=Find(bt[i+1]),c=Find(bt[i+2]);parent[b]=a;parent[Find(c)]=a;}
    var pieces=Enumerable.Range(0,bv.Length).GroupBy(Find).ToList();var drop=new HashSet<int>();int cleared=0,onRamp=0,inBrush=0;float last=0;
    foreach(var piece in pieces){var ps=piece.Select(i=>world.MultiplyPoint3x4(bv[i])).ToList();var c=ps.Aggregate(Vector3.zero,(x,y)=>x+y)/ps.Count;float reach=ps.Max(p=>new Vector2(p.x-c.x,p.z-c.z).magnitude);
     float s=line.Project(c,out _);var q=line.At(s,out _);float off=new Vector2(c.x-q.x,c.z-q.z).magnitude;bool above=c.y>q.y-.5f;
     if(s>=far&&off-reach<ClearHalf){cleared++;foreach(var i in piece)drop.Add(i);}
     else if(s<lipS&&off-reach<3.5f&&ps.Max(p=>p.y)>q.y-.3f){onRamp++;foreach(var i in piece)drop.Add(i);}
     else if(s>=lipS&&off-reach<ClearHalf&&above){inBrush++;last=Mathf.Max(last,s+reach);}}
    if(sn=="DansBackyardForward"){var tri=new List<int>();for(int i=0;i<bt.Length;i+=3)if(!drop.Contains(bt[i]))tri.AddRange(new[]{bt[i],bt[i+1],bt[i+2]});
     var m=UnityEngine.Object.Instantiate(source);m.name="Abandoned Cabin Jump dense brush (0.101)";m.SetTriangles(tri,0);m.RecalculateBounds();var ex=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(ex){EditorUtility.CopySerialized(m,ex);m=ex;}else AssetDatabase.CreateAsset(m,path);}
    g.GetComponent<MeshFilter>().sharedMesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);EditorUtility.SetDirty(g);
    log.Add($"brush: {pieces.Count} bushes in the original; {cleared} removed within {ClearHalf} m of the line from the far edge s {far:F1} to the rejoin, {onRamp} off the run-up and ramp, {inBrush} kept between the lip (s {lipS:F1}) and the far edge; the last reaches s {last:F1}; reset to s {far+Racer.ShortcutUndergrowth.ResetPastEdge:F1}");}
   EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);}
  AssetDatabase.SaveAssets();}catch(Exception e){log.Add("FAILED "+e);}
  Directory.CreateDirectory(Out);File.WriteAllLines(Out+"/cabinfix.txt",log);EditorApplication.Exit(0);}
}
