using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace Racer.Editor {
public static class Phase6Validation {
 public static void Geometry(){
  Physics.SyncTransforms();var log=new List<string>();int fail=0;
  void Check(bool ok,string message){log.Add((ok?"PASS ":"FAIL ")+message);if(!ok)fail++;}
  var root=GameObject.Find(Phase6Review.Root).transform;var ground=GameObject.Find("Memory loop - north is +Z").transform;
  Check(!root.Find("Original house 1"),"House 1 object, child foundation, wall and roof colliders absent");
  var before=File.ReadAllLines("Docs/PHASE6_BEFORE_PLACEMENTS.txt").Where(l=>!l.Contains("|Original house 1|")).Select(l=>l.Substring(l.IndexOf('|')+1)).ToArray();
  var after=root.Cast<Transform>().Select(t=>$"{t.name}|{t.position.ToString("F6")}|{t.rotation.ToString("F6")}|{t.localScale.ToString("F6")}").ToArray();
  Check(before.SequenceEqual(after),"All 47 surviving building names, world positions, rotations and scales exactly match baseline (6 decimals); labels 2/3 retained");
  int n=0,missing=0,blocked=0;float slope=0;
  for(float z=-25;z<133;z+=2)for(float x=323;x<447;x+=2){var p=new Vector3(x,0,z);if(Phase6Buildings.YardDistance(p)>22||(p-Phase6Buildings.Dan).magnitude<15)continue;n++;
   if(!Physics.Raycast(new(x,300,z),Vector3.down,out var hit,600,1)){missing++;continue;}if(!hit.collider.transform.IsChildOf(ground))blocked++;slope=Mathf.Max(slope,Vector3.Angle(hit.normal,Vector3.up));
  }
  Check(missing==0&&blocked==0,$"Expanded-yard {n} vertical support/obstacle probes: missing {missing}, non-terrain hits {blocked}; max slope {slope:F2} degrees");
  int seams=0;var coords=new Dictionary<Vector2,float>();foreach(var f in ground.GetComponentsInChildren<MeshFilter>())foreach(var v in f.sharedMesh.vertices){var key=new Vector2(v.x,v.z);if(coords.TryGetValue(key,out float y)&&Mathf.Abs(y-v.y)>.00001f)seams++;coords[key]=v.y;}
  Check(seams==0,$"Terrain tile boundary duplicates: {seams} height mismatches; continuous collision uses the same visible mesh");
  Check(ground.GetComponentsInChildren<MeshFilter>().All(f=>f.GetComponent<MeshCollider>().sharedMesh==f.sharedMesh),"Every terrain tile renders and collides with the same mesh");
  int floats=0;float maxGap=0;var road=StreetLoopBuilder.Route();float clearance=999;
  foreach(Transform t in root){
   var foundation=t.Find("Foundation");foreach(float x in new[]{-.5f,.5f})foreach(float z in new[]{-.5f,.5f}){
    var p=foundation.TransformPoint(new Vector3(x,-.5f,z));float gap=p.y-Phase6Buildings.Ground(p);maxGap=Mathf.Max(maxGap,gap);if(gap>.25f)floats++;
   }
   foreach(var box in t.GetComponentsInChildren<BoxCollider>()){
    foreach(float x in new[]{-.5f,.5f})foreach(float z in new[]{-.5f,.5f}){var p=box.transform.TransformPoint(box.center+Vector3.Scale(box.size,new Vector3(x,0,z)));clearance=Mathf.Min(clearance,StreetLoopBuilder.Nearest(p,road,out _));}
   }
   Check(t.Find("Phase 6 architecture")!=null,t.name+" has reusable architecture");
  }
  Check(floats==0,$"Foundation corner grounding: {floats} corners >0.25m above terrain; maximum gap {maxGap:F3}m");
  Check(clearance>=9,$"Minimum sampled building/step box-collider setback from road center {clearance:F2}m (9m road/shoulder envelope)");
  int roofMismatch=0;foreach(var c in root.GetComponentsInChildren<MeshCollider>())if(!c.convex||!c.sharedMesh)roofMismatch++;
  Check(roofMismatch==0,$"Simple convex roof collision present: {root.GetComponentsInChildren<MeshCollider>().Length} roofs; invalid {roofMismatch}");
  Check(GameObject.Find(Phase5Setup.RootName)!=null,"Accepted shortcut root retained");
  log.Add("Legacy rebuild guard inspected in source only; rebuild intentionally not invoked.");
  log.Add($"RESULT: {fail} failed checks. Geometric sampling is not an exhaustive collision proof; virtual driving and visual inspection recorded separately.");File.WriteAllLines("Docs/PHASE6_GEOMETRY.txt",log);
 }
}
}
