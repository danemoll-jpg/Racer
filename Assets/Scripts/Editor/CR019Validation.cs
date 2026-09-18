using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEditor;
using Object=UnityEngine.Object;
namespace Racer.Editor {
public static class CR019Validation {
 static string Hash(byte[] b){using var s=SHA256.Create();return BitConverter.ToString(s.ComputeHash(b));}
 static string Shape(Mesh m){using var stream=new MemoryStream();using(var w=new BinaryWriter(stream,System.Text.Encoding.UTF8,true)){foreach(var p in m.vertices){w.Write(p.x);w.Write(p.y);w.Write(p.z);}foreach(var p in m.normals){w.Write(p.x);w.Write(p.y);w.Write(p.z);}foreach(int i in m.triangles)w.Write(i);var uv=new List<Vector4>();m.GetUVs(1,uv);foreach(var p in uv){w.Write(p.x);w.Write(p.y);w.Write(p.z);w.Write(p.w);}}return Hash(stream.ToArray());}
 public static void Preservation(string label){
  Directory.CreateDirectory(CR019Commercial.Dir);var rows=new List<string>();
  foreach(Transform house in GameObject.Find(Phase6Review.Root).transform)if(!house.name.Contains("business"))foreach(var t in house.GetComponentsInChildren<Transform>())rows.Add("HOUSE|"+house.GetSiblingIndex()+"|"+t.name+"|"+t.localPosition.ToString("F6")+"|"+t.localRotation.ToString("F6")+"|"+t.localScale.ToString("F6"));
  foreach(var f in GameObject.Find("Memory loop - north is +Z").GetComponentsInChildren<MeshFilter>().OrderBy(f=>f.name))rows.Add("TERRAIN|"+f.name+"|"+Shape(f.sharedMesh));
  foreach(var p in Object.FindObjectsByType<BreakableProp>().OrderBy(p=>p.name).ThenBy(p=>p.transform.position.x))rows.Add("PROP|"+p.name+"|"+p.transform.position.ToString("F6")+"|"+p.transform.rotation.ToString("F6"));
  foreach(var c in GameObject.Find("Woods replacing later subdivisions").GetComponentsInChildren<BoxCollider>().OrderBy(c=>c.name).ThenBy(c=>c.transform.position.x))rows.Add("TREE|"+c.name+"|"+c.transform.position.ToString("F6")+"|"+c.transform.rotation.ToString("F6")+"|"+c.transform.lossyScale.ToString("F6"));
  File.WriteAllLines(CR019Commercial.Dir+"/"+label+"-preservation.txt",rows);
 }
 public static void Check(string label){
  Physics.SyncTransforms();var shops=CR019Commercial.Shops();var logs=new List<string>();int failed=0;
  void Test(bool pass,string message){logs.Add((pass?"PASS ":"FAIL ")+message);if(!pass)failed++;}
  Test(shops.Length==22,"22 businesses retained");
  string Variant(Transform t)=>PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(t.Find("Phase 6 architecture").gameObject);
  var south=shops.Where(t=>t.name.StartsWith("CR019 S")).OrderBy(t=>t.position.x).ToArray();var north=shops.Where(t=>t.name.StartsWith("CR019 N")).OrderBy(t=>t.position.x).ToArray();
  Test(south.Length==11&&north.Length==11,"11 sites per side");
  float minSame=float.PositiveInfinity,minOpp=float.PositiveInfinity,minGap=float.PositiveInfinity,minRoad=float.PositiveInfinity;int overlaps=0,accessObstacles=0;float maxFoundationGap=0;
  float HalfX(Transform t){var f=t.Find("Foundation");return Mathf.Abs(t.right.x)*f.localScale.x*.5f+Mathf.Abs(t.forward.x)*f.localScale.z*.5f;}
  foreach(var s in south)foreach(var n in north){float delta=Mathf.Abs(s.position.x-n.position.x);minOpp=Mathf.Min(minOpp,delta);if(Variant(s)==Variant(n))minSame=Mathf.Min(minSame,delta-HalfX(s)-HalfX(n));}
  Test(minSame>0,$"Matching opposite storefront projected footprints separated by at least {minSame:F2}m along X");
  logs.Add($"Closest opposing center stagger={minOpp:F2}m.");
  foreach(var row in new[]{south,north}){var spacing=row.Skip(1).Select((t,i)=>t.position.x-row[i].position.x).ToArray();logs.Add((row==south?"South":"North")+" center spacing(m): "+string.Join(", ",spacing.Select(x=>x.ToString("F2"))));for(int i=1;i<row.Length;i++)minGap=Mathf.Min(minGap,row[i].position.x-row[i-1].position.x-HalfX(row[i])-HalfX(row[i-1]));}
  Test(minGap>10,$"Smallest same-row building footprint gap {minGap:F2}m");
  foreach(var s in shops){var foundation=s.Find("Foundation");var f=foundation.GetComponent<BoxCollider>();
   foreach(float x in new[]{-.5f,0,.5f})foreach(float z in new[]{-.5f,0,.5f}){var p=foundation.TransformPoint(new Vector3(x,-.5f,z));maxFoundationGap=Mathf.Max(maxFoundationGap,p.y-Phase6Buildings.Ground(p));minRoad=Mathf.Min(minRoad,StreetLoopBuilder.Nearest(p,StreetLoopBuilder.Route(),out _));}
   foreach(var other in shops)if(string.CompareOrdinal(s.name,other.name)<0&&Physics.ComputePenetration(f,f.transform.position,f.transform.rotation,other.Find("Foundation").GetComponent<BoxCollider>(),other.Find("Foundation").position,other.Find("Foundation").rotation,out _,out _))overlaps++;
   // Roadward central access corridor: terrain and the site's own steps are intentional.
   for(float z=foundation.localScale.z*.5f+2.5f;z<28;z+=2){var p=s.TransformPoint(new(0,0,z));if(StreetLoopBuilder.Nearest(p,StreetLoopBuilder.Route(),out _)<=10)continue;p.y=Phase6Buildings.Ground(p)+1;accessObstacles+=Physics.OverlapBox(p,new Vector3(1.65f,.7f,.7f),s.rotation,~0,QueryTriggerInteraction.Ignore).Count(c=>!c.transform.IsChildOf(s)&&!c.transform.IsChildOf(GameObject.Find("Memory loop - north is +Z").transform));}
   var a=s.Find("Phase 6 architecture");Test(a.GetComponentsInChildren<TextMesh>().Length==1&&a.GetComponentsInChildren<Collider>().Length>=2,$"{s.name}: attached sign and prefab colliders");
  }
  Test(overlaps==0,$"Site foundation overlaps={overlaps}");Test(maxFoundationGap<=.02f,$"Foundation bottom maximum gap={maxFoundationGap:F3}m");Test(minRoad>16,$"Minimum building foundation clearance from road center={minRoad:F2}m");Test(accessObstacles==0,$"Central driveway corridor obstacle samples={accessObstacles}");
  var sourceFilters=GameObject.Find(Phase6Review.Root).GetComponentsInChildren<MeshFilter>();var batch=GameObject.Find("Phase 6 - architectural render batches").GetComponentsInChildren<MeshFilter>();
  Test(sourceFilters.All(f=>!f.GetComponent<Renderer>().enabled),"Source renderers disabled; batched visuals only");
  foreach(var group in sourceFilters.GroupBy(f=>f.GetComponent<Renderer>().sharedMaterial)){var b=batch.Single(f=>f.GetComponent<Renderer>().sharedMaterial==group.Key);var points=group.SelectMany(f=>f.sharedMesh.vertices.Select(p=>f.transform.TransformPoint(p))).ToArray();var actual=b.sharedMesh.vertices;Test(points.Length==actual.Length&&points.Zip(actual,(p,q)=>(p-q).sqrMagnitude<.000001f).All(x=>x),"Batched world vertices match sources: "+group.Key.name);}
  logs.Add($"RESULT failures={failed}; {batch.Length} architectural material batches; {Object.FindObjectsByType<Renderer>().Length} scene renderers.");File.WriteAllLines(CR019Commercial.Dir+"/"+label+"-validation.txt",logs);
 }
}
}
