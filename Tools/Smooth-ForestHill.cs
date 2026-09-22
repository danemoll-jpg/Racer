using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using Object=UnityEngine.Object;
public static class SmoothForestHill {
 public static string Main(){
  if(Application.isPlaying||EditorApplication.isCompiling||UnityEngine.SceneManagement.SceneManager.GetActiveScene().name!="ForestLoopReverse")throw new Exception("Forest Loop Reverse edit mode required");
  var branch=Object.FindObjectsByType<Racer.WoodlandRoute>().Single(b=>b.title=="Granite Saddle");
  var filters=new[]{"Ground_560_240","Ground_640_240"}.Select(n=>GameObject.Find(n).GetComponent<MeshFilter>()).ToArray();
  // Always evaluate from the checkpoint, including when refining this one-off edit.
  foreach(var mf in filters){var text=File.ReadAllText("Temp/ForestHill-original-"+mf.name+".txt");var hex=System.Text.RegularExpressions.Regex.Match(text,@"_typelessdata: ([0-9a-fA-F]+)").Groups[1].Value;var bytes=Enumerable.Range(0,hex.Length/2).Select(i=>Convert.ToByte(hex.Substring(i*2,2),16)).ToArray();var v=mf.sharedMesh.vertices;int stride=bytes.Length/v.Length;for(int i=0;i<v.Length;i++)v[i]=new Vector3(BitConverter.ToSingle(bytes,i*stride),BitConverter.ToSingle(bytes,i*stride+4),BitConverter.ToSingle(bytes,i*stride+8));mf.sharedMesh.vertices=v;mf.GetComponent<MeshCollider>().sharedMesh=null;mf.GetComponent<MeshCollider>().sharedMesh=mf.sharedMesh;}
  Physics.SyncTransforms();
  var main=Object.FindAnyObjectByType<Racer.RaceDirector>().road;
  var original=filters.Select(m=>m.sharedMesh.vertices).ToArray();
  var colliders=filters.Select(m=>m.GetComponent<MeshCollider>()).ToArray();
  Vector3 Point(float s,float t){var p=branch.At(s,out var f);return p+Vector3.Cross(Vector3.up,f).normalized*t;}
  float Ground(float s,float t){var p=Point(s,t);var ray=new Ray(p+Vector3.up*200,Vector3.down);float y=float.NegativeInfinity;foreach(var c in colliders)if(c.Raycast(ray,out var h,400))y=Math.Max(y,h.point.y);if(float.IsInfinity(y))throw new Exception("Missing original ground");return y;}
  float Smooth(float u){u=Mathf.Clamp01(u);return u*u*u*(u*(u*6-15)+10);}
  // The upper boundary follows the diagonal driveway edge and stops before it.
  // Match each lane's original endpoint height and grade; only the old terrain vertices move.
  var profiles=new List<float[]>();
  float startSlope=Ground(55.5f,0)-Ground(54.5f,0),endSlope=Ground(162.5f,0)-Ground(161.5f,0);
  for(int i=-60;i<=60;i++){float t=i*.2f,a=55,b=162-.9f*t;profiles.Add(new[]{Ground(a,t),Ground(b,t),startSlope,endSlope});}
  float Target(float s,float t,float old){float a=55,b=162-.9f*t,u=(s-a)/(b-a);float ix=(t+12)*5;int k=Mathf.Clamp((int)ix,0,profiles.Count-2);float f=ix-k;var q=Enumerable.Range(0,4).Select(j=>Mathf.Lerp(profiles[k][j],profiles[k+1][j],f)).ToArray();float y=(2*u*u*u-3*u*u+1)*q[0]+(u*u*u-2*u*u+u)*(b-a)*q[2]+(-2*u*u*u+3*u*u)*q[1]+(u*u*u-u*u)*(b-a)*q[3];float w=(1-Smooth((Math.Abs(t)-6)/6))*Smooth((s-a)/5)*Smooth((b-s)/5);return Mathf.Lerp(old,y,w);}
  var rows=new List<string>();var before=new List<string>{"station,lateral,height"};
  for(float s=50;s<=180;s+=.5f)for(float t=-4;t<=4;t+=2)before.Add($"{s},{t},{Ground(s,t):F6}");File.WriteAllLines("Docs/ForestHill/before-profile.csv",before);
  var outputs=new Vector3[filters.Length][];int moved=0;
  for(int m=0;m<filters.Length;m++){var v=original[m].ToArray();int n=0;for(int i=0;i<v.Length;i++){var p=filters[m].transform.TransformPoint(v[i]);float s=branch.Project(p,out float d);if(d>=12||s<=55||s>=174)continue;var at=branch.At(s,out var f);float t=Vector3.Dot(p-at,Vector3.Cross(Vector3.up,f).normalized);if(s>=162-.9f*t)continue;float y=Target(s,t,p.y);main.Project(p,out float mainDistance);y=Mathf.Lerp(p.y,y,Smooth((mainDistance-6)/4));if(Math.Abs(y-p.y)<.00001f)continue;p.y=y;v[i]=filters[m].transform.InverseTransformPoint(p);n++;}outputs[m]=v;moved+=n;rows.Add(filters[m].name+": changed "+n+" of "+v.Length+" vertices; topology and x/z unchanged");}
  for(int m=0;m<filters.Length;m++){var mesh=filters[m].sharedMesh;mesh.vertices=outputs[m];mesh.RecalculateNormals();mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);colliders[m].sharedMesh=null;colliders[m].sharedMesh=mesh;}
  AssetDatabase.SaveAssets();Physics.SyncTransforms();
  rows.Add("Only existing ForestLoopReverse-specific terrain assets edited. No scene, route, driveway, physics, AI or reset components edited.");
  File.WriteAllLines("Docs/ForestHill/implementation.txt",rows);return string.Join("\n",rows);
 }
}



