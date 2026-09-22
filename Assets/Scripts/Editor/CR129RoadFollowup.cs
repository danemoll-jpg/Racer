using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Object=UnityEngine.Object;
namespace Racer.Editor {
 public static partial class DiscoveryAuthoring {
  const string CR129Dir="Docs/CR129-132";
  static void ResetRoad129(RaceRoad road){typeof(RaceRoad).GetField("distance",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).SetValue(road,null);road.Initialize();EditorUtility.SetDirty(road);}
  static void Store129(MeshFilter mf,Mesh mesh,string suffix){mesh.RecalculateNormals();mesh.RecalculateBounds();mf.sharedMesh=MeshAsset(mesh,owner.gameObject.scene.name+"-CR129-"+suffix);if(mf.TryGetComponent<MeshCollider>(out var c)){c.sharedMesh=null;c.sharedMesh=mf.sharedMesh;}}
  static float HighwayWeight129(float x)=>Smooth(205,280,x)*(1-Smooth(350,445,x));
  // Correct the CR113 elevated intersection itself, then cut its subgrade at
  // the pavement edge. The approach is terrain, not another stacked ramp.
  public static void CR129Highway(){
   Directory.CreateDirectory(CR129Dir);var rows=new List<string>();
   foreach(var scene in CR112Scenes){EditorSceneManager.OpenScene(scene);owner=Object.FindAnyObjectByType<RaceDirector>();
    if(GameObject.Find("CR129 aligned Highway 92 junction"))throw new Exception("Junction already repaired");
    var highway=GameObject.Find("Ground_CR113 east four lane seam").GetComponent<MeshFilter>();
    var original=highway.sharedMesh.vertices;var centers=Enumerable.Range(0,original.Length/2).Select(i=>highway.transform.TransformPoint((original[i*2]+original[i*2+1])*.5f)).ToArray();
    float Height(Vector3 p){Near(p,centers,out var q);return Mathf.Lerp(q.y,8,HighwayWeight129(p.x));}
    var filters=GameObject.Find("CR113 continuous highway seams").GetComponentsInChildren<MeshFilter>();int meshId=0;
    foreach(var mf in filters){var v=mf.sharedMesh.vertices;bool changed=false;for(int i=0;i<v.Length;i++){var p=mf.transform.TransformPoint(v[i]);float d=Near(p,centers,out var at);if(p.x<205||p.x>445||d>9)continue;p.y+=Height(p)-at.y;v[i]=mf.transform.InverseTransformPoint(p);changed=true;}if(changed){var mesh=Object.Instantiate(mf.sharedMesh);mesh.vertices=v;Store129(mf,mesh,"highway-"+meshId++);}}
    Physics.SyncTransforms();
    var terrain=GameObject.Find("Memory loop - north is +Z").GetComponentsInChildren<MeshFilter>();int terrainCount=0;
    foreach(var mf in terrain){var v=mf.sharedMesh.vertices;bool changed=false;for(int i=0;i<v.Length;i++){var p=mf.transform.TransformPoint(v[i]);if(p.x<205||p.x>445)continue;float d=Near(p,centers,out var at);if(d>44)continue;
      // Blend into the retained street profile over 35m, with zero grade
      // difference at the highway. Outside the pavement preserve local relief.
      float weight=HighwayWeight129(p.x)*(1-Smooth(8.2f,43.2f,d));float y=Mathf.Lerp(p.y,Height(p),weight);if(Math.Abs(y-p.y)<.0001f)continue;p.y=y;v[i]=mf.transform.InverseTransformPoint(p);changed=true;}
     if(!changed)continue;
     float Field(Vector3 p){float d=Near(p,centers,out _);return Math.Max(d-8.2f,Math.Max(205-p.x,p.x-445));}
     Vector3 Seat(Vector3 p){if(p.x>205.01f&&p.x<444.99f){p.y=Height(p);}return p;}
     var output=new List<Vector3>();var colors=new List<Color>();var indices=new List<int>();var tris=mf.sharedMesh.triangles;var oldColors=mf.sharedMesh.colors;
     for(int t=0;t<tris.Length;t+=3){var poly=new List<(Vector3 p,Color c)>();for(int j=0;j<3;j++){int ia=tris[t+j],ib=tris[t+(j+1)%3];var a=mf.transform.TransformPoint(v[ia]);var b=mf.transform.TransformPoint(v[ib]);var ca=oldColors.Length==v.Length?oldColors[ia]:Color.white;var cb=oldColors.Length==v.Length?oldColors[ib]:Color.white;float fa=Field(a),fb=Field(b);if(fa>=0)poly.Add((a,ca));if((fa>=0)!=(fb>=0)){float blend=fa/(fa-fb);poly.Add((Seat(Vector3.Lerp(a,b,blend)),Color.Lerp(ca,cb,blend)));}}int n=output.Count;output.AddRange(poly.Select(q=>mf.transform.InverseTransformPoint(q.p)));colors.AddRange(poly.Select(q=>q.c));for(int j=1;j+1<poly.Count;j++)indices.AddRange(new[]{n,n+j,n+j+1});}
     var copy=new Mesh{indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};copy.SetVertices(output);copy.SetColors(colors);copy.SetTriangles(indices,0);Store129(mf,copy,"junction-"+mf.name);terrainCount++;
    }
    Physics.SyncTransforms();
    // Navigation and spawn use the same repaired physical elevation. Retain
    // every x/z coordinate, gate, grid station and geometry-station mapping.
    foreach(var road in Object.FindObjectsByType<RaceRoad>()){var p=road.points.ToArray();bool changed=false;for(int i=0;i<p.Length;i++){if(p[i].x<205||p[i].x>445||Near(p[i],centers,out _)>44)continue;var hits=Physics.RaycastAll(p[i]+Vector3.up*20,Vector3.down,40,1,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name.StartsWith("Ground_")).ToArray();if(hits.Length==0)throw new Exception("Missing junction support");p[i].y=hits.Max(h=>h.point.y);changed=true;}if(changed){road.points=p;ResetRoad129(road);}}
    if(scene.EndsWith("StreetLoopGreybox.unity")){var road=owner.road;float origin=road.Project(owner.gates[0].transform.position,out _);var p=road.At(origin-32,out var f)-Vector3.Cross(Vector3.up,f).normalized*2.2f;var support=Physics.RaycastAll(p+Vector3.up*10,Vector3.down,20,1,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name.StartsWith("Ground_")).OrderByDescending(h=>h.point.y).First();rows.Add($"Forward grid x/z retained; road={p}; collider={support.point}; height error={support.point.y-p.y:F4}");var spawn=owner.vehicle.GetComponent<VehicleRespawn>().spawnPoint;var old=spawn.position;old.y=CR105Authoring.Ground(old)+.7f;spawn.position=old;owner.vehicle.transform.SetPositionAndRotation(spawn.position,spawn.rotation);}
    new GameObject("CR129 aligned Highway 92 junction");Save();rows.Add(scene+" terrain tiles="+terrainCount);
   }File.WriteAllLines(CR129Dir+"/highway.txt",rows);
  }
  sealed class Ribbon129 {public List<Vector3> p;public List<float> w;public MeshFilter mf;}
  static Ribbon129 Read129(string name){var mf=GameObject.Find(name).GetComponent<MeshFilter>();var v=mf.sharedMesh.vertices;return new(){mf=mf,p=Enumerable.Range(0,v.Length/2).Select(i=>mf.transform.TransformPoint((v[2*i]+v[2*i+1])*.5f)-Vector3.up*.04f).ToList(),w=Enumerable.Range(0,v.Length/2).Select(i=>Vector3.Distance(v[2*i],v[2*i+1])*.5f).ToList()};}
  static void Replace129(Ribbon129 ribbon,RaceRoad road,float a,float b,Vector3[] replacement){var stations=ribbon.p.Select(p=>road.Project(p,out _)).ToArray();int first=Array.FindIndex(stations,s=>s>=a),last=Array.FindLastIndex(stations,s=>s<=b);if(first<0||last<first)throw new Exception("Invalid ribbon interval");float wa=ribbon.w[first],wb=ribbon.w[last];ribbon.p.RemoveRange(first,last-first+1);ribbon.w.RemoveRange(first,last-first+1);ribbon.p.InsertRange(first,replacement);ribbon.w.InsertRange(first,Enumerable.Range(0,replacement.Length).Select(i=>Mathf.Lerp(wa,wb,Smooth(0,1,(float)i/(replacement.Length-1)))));}
  static void Write129(Ribbon129 r,string name){var v=new List<Vector3>();var t=new List<int>();for(int i=0;i<r.p.Count;i++){var f=r.p[Math.Min(i+1,r.p.Count-1)]-r.p[Math.Max(0,i-1)];var side=Vector3.Cross(Vector3.up,f).normalized*r.w[i];v.Add(r.mf.transform.InverseTransformPoint(r.p[i]-side+Vector3.up*.04f));v.Add(r.mf.transform.InverseTransformPoint(r.p[i]+side+Vector3.up*.04f));if(i>0){int n=(i-1)*2;t.AddRange(new[]{n,n+2,n+1,n+1,n+2,n+3});}}var m=new Mesh{indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};m.SetVertices(v);m.SetTriangles(t,0);Store129(r.mf,m,name);}
  public static void CR129Mountain(){
   EditorSceneManager.OpenScene("Assets/Scenes/MountainLoop.unity");owner=Object.FindAnyObjectByType<RaceDirector>();if(GameObject.Find("CR129 forward straight approaches"))throw new Exception("Already repaired");var road=owner.road;road.Initialize();worldRoot=GameObject.Find("CR122 continuous mountain support").transform;repairStrips.Clear();var r0=Read129("Ground_CR122 main section 0");var r1=Read129("Ground_CR122 main section 1");var r2=Read129("Ground_CR122 main section 2");
   var rows=new List<string>();
   // Smooth the complete cross-section, including plan-view tangent changes
   // that folded adjacent quads over one another at the reported seam.
   foreach(var range in new[]{(1374f,1424f),(1570f,1640f)}){var a=road.At(range.Item1,out var fa);var b=road.At(range.Item2,out var fb);Replace129(r1,road,range.Item1,range.Item2,RepairHermite(a,b,fa,fb));}
   var flights=owner.GetComponent<MountainFlights>().flights;
   foreach(var flight in flights){bool gully=flight.name.Contains("Gully");var r=gully?r0:r1;float begin=gully?125:1740;float end=road.Project(flight.lip,out _);var join=road.At(begin,out var tangent);var straightStart=flight.start-flight.forward*(gully?40:70);straightStart.y=flight.start.y;
    var entry=RepairHermite(join,straightStart,tangent,flight.forward);float length=Vector3.Dot(flight.lip-straightStart,flight.forward);float rise=flight.lip.y-straightStart.y;float ramp=60;
    int count=Mathf.CeilToInt(length/.5f);var run=Enumerable.Range(0,count+1).Select(i=>{float s=length*i/count;float u=Mathf.Clamp01((s-(length-ramp))/ramp);return straightStart+flight.forward*s+Vector3.up*(rise*u*u);}).ToArray();
    Replace129(r,road,begin,end+.1f,entry.Concat(run.Skip(1)).ToArray());flight.start=straightStart;rows.Add($"{flight.name}: straight run-up+ramp {length:F1}m, ramp {ramp}m, rise {rise:F2}m; lip/destination retained.");ExcludeRecovery(entry[0],run[^1],22);
   }
   // This pre-CR122 launch survived the last replacement and remained a second
   // driving collider directly under/through the new summit runway.
   var duplicate=GameObject.Find("Ground_Summit authored launch");if(duplicate)duplicate.SetActive(false);
   Write129(r0,"forward-main-0");Write129(r1,"forward-main-1");Physics.SyncTransforms();
   road.points=r0.p.Concat(r1.p).Concat(r2.p.Take(r2.p.Count-1)).ToArray();ResetRoad129(road);
   foreach(var r in new[]{r0,r1})repairStrips.Add(new RepairStrip(r.p.ToArray(),7){widths=r.w.ToArray()});
   // Regenerate only forward branch mouths to meet the changed main edges.
   foreach(var branch in Object.FindObjectsByType<WoodlandRoute>()){
    var p=branch.points.ToArray();for(int i=0;i<p.Length;i++){float s=road.Project(p[i],out float d);var at=road.At(s,out _);if(Math.Abs(p[i].y-at.y)<6)p[i].y=Mathf.Lerp(at.y,p[i].y,Smooth(7,16,d));}branch.points=p;branch.Initialize();GameObject.Find("Ground_CR122 "+branch.title).SetActive(false);RepairRibbon(branch.title,new(p,branch.halfWidth),true);branch.entryRoad=road.Project(p[0],out _);branch.exitRoad=road.Project(p[^1],out _);
   }
   RepairSubgrade();Physics.SyncTransforms();
   foreach(var flight in flights){flight.approachStation=road.Project(flight.start,out _);flight.endStation=road.Project(flight.landingEnd,out _);}
   foreach(var gate in owner.gates){var p=gate.transform.position-Vector3.up*1.6f;float s=road.Project(p,out _);gate.transform.SetPositionAndRotation(road.At(s,out var f)+Vector3.up*1.6f,Quaternion.LookRotation(Vector3.ProjectOnPlane(f,Vector3.up)));}
   foreach(var branch in Object.FindObjectsByType<WoodlandRoute>())branch.bypassedGates=Enumerable.Range(1,owner.gates.Length-1).Where(i=>road.Relative(road.Project(owner.gates[i].transform.position,out _),branch.entryRoad)<road.Relative(branch.exitRoad,branch.entryRoad)).ToArray();
   // Re-seat guidance on the authored road; arrows have no physical colliders.
   foreach(var mf in Object.FindObjectsByType<MeshFilter>().Where(m=>m.name=="Main teal trail arrow")){float s=road.Project(mf.transform.position,out float d);if(d>15){mf.gameObject.SetActive(false);continue;}var p=road.At(s,out var f);mf.transform.SetPositionAndRotation(p+Vector3.up*.10f,Quaternion.LookRotation(f));}
   ClearCompleteTrees(p=>flights.Any(f=>{var q=p-f.start;float s=Vector3.Dot(q,f.forward);return s>-80&&s<Vector3.Dot(f.lip-f.start,f.forward)&&Math.Abs(Vector3.Dot(q,Vector3.Cross(Vector3.up,f.forward)))<19;}));
   owner.courseId="mountain-forward-v4-straight-smooth";new GameObject("CR129 forward straight approaches");Save();File.WriteAllLines(CR129Dir+"/mountain.txt",rows);PlayerSettings.bundleVersion="0.23.0-review1";AssetDatabase.SaveAssets();
  }
 }
}
