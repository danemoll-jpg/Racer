using System;using System.IO;using System.Linq;using System.Text;using System.Collections.Generic;using UnityEditor;using UnityEngine;
// 0.89 Part F (as 0.88 Part B) (read only): z-fighting candidates in the vehicle models. For every FBX in Resources/VehicleModels: all
// triangles in the model's own space, grouped by plane; two triangles facing the same way on the same plane (within 2 mm)
// whose areas overlap are reported per pair of parts (object names '<group>__<slot>'), with the overlapping area.
public static class Report089Coplanar {
 public static void Run(){var o=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(o);var sb=new StringBuilder();var tag=Environment.GetEnvironmentVariable("COPLANAR_TAG")??"";
  foreach(var path in Directory.GetFiles("Assets/Resources/VehicleModels","*.fbx").OrderBy(p=>p)){var root=AssetDatabase.LoadAssetAtPath<GameObject>(path.Replace('\\','/'));if(!root)continue;
   var tris=new List<(Vector3 a,Vector3 b,Vector3 c,Vector3 n,float d,string part)>();
   foreach(var mf in root.GetComponentsInChildren<MeshFilter>(true)){var m=mf.sharedMesh;if(!m)continue;var mx=root.transform.worldToLocalMatrix*mf.transform.localToWorldMatrix;var v=m.vertices.Select(x=>mx.MultiplyPoint3x4(x)).ToArray();var t=m.triangles;
    for(int k=0;k<t.Length;k+=3){var a=v[t[k]];var b=v[t[k+1]];var c=v[t[k+2]];var n=Vector3.Cross(b-a,c-a);float ar=n.magnitude*.5f;if(ar<1e-5f)continue;n.Normalize();tris.Add((a,b,c,n,Vector3.Dot(n,a),mf.name));}}
   var buckets=new Dictionary<(int,int,int,int),List<int>>();
   for(int i=0;i<tris.Count;i++){var q=tris[i];var key=(Mathf.RoundToInt(q.n.x*50),Mathf.RoundToInt(q.n.y*50),Mathf.RoundToInt(q.n.z*50),Mathf.RoundToInt(q.d/.004f));if(!buckets.TryGetValue(key,out var l))buckets[key]=l=new List<int>();l.Add(i);}
   var pairs=new Dictionary<string,(float area,Vector3 at,int n)>();
   static bool In(Vector3 p,Vector3 a,Vector3 b,Vector3 c,Vector3 n){float s1=Vector3.Dot(Vector3.Cross(b-a,p-a),n),s2=Vector3.Dot(Vector3.Cross(c-b,p-b),n),s3=Vector3.Dot(Vector3.Cross(a-c,p-c),n);return (s1>1e-6f&&s2>1e-6f&&s3>1e-6f);}
   foreach(var kv in buckets){var (nx,ny,nz,dd)=kv.Key;var cand=new List<int>();for(int e=-1;e<=1;e++)if(buckets.TryGetValue((nx,ny,nz,dd+e),out var l2))cand.AddRange(l2);
    foreach(var i in kv.Value)foreach(var j in cand){if(j<=i)continue;var A=tris[i];var B=tris[j];if(Vector3.Dot(A.n,B.n)<.9995f||Mathf.Abs(A.d-B.d)>.002f)continue;
     // overlap: sample A on a 6x6 barycentric lattice, count points strictly inside B (and the other way)
     int hit=0,tot=0;for(int u=1;u<6;u++)for(int w=1;w<6-u;w++){var p=A.a+(A.b-A.a)*(u/6f)+(A.c-A.a)*(w/6f);tot++;if(In(p,B.a,B.b,B.c,B.n))hit++;}
     if(hit==0){for(int u=1;u<6;u++)for(int w=1;w<6-u;w++){var p=B.a+(B.b-B.a)*(u/6f)+(B.c-B.a)*(w/6f);if(In(p,A.a,A.b,A.c,A.n)){hit++;break;}}if(hit==0)continue;}
     float area=Vector3.Cross(A.b-A.a,A.c-A.a).magnitude*.5f*hit/Math.Max(1,tot);var key=string.CompareOrdinal(A.part,B.part)<=0?A.part+" | "+B.part:B.part+" | "+A.part;
     pairs.TryGetValue(key,out var acc);pairs[key]=(acc.area+area,acc.n==0?(A.a+A.b+A.c)/3:acc.at,acc.n+1);}}
   var name=Path.GetFileNameWithoutExtension(path);float total=pairs.Values.Sum(p=>p.area);
   sb.AppendLine($"{name}: {tris.Count} triangles; same-facing coplanar overlaps {pairs.Values.Sum(p=>p.n)} triangle pairs, about {total*1e4:F0} cm2");
   foreach(var kv in pairs.OrderByDescending(p=>p.Value.area).Take(8))sb.AppendLine($"  {kv.Key}: {kv.Value.n} pairs, about {kv.Value.area*1e4:F0} cm2, e.g. at {kv.Value.at.x:F2},{kv.Value.at.y:F2},{kv.Value.at.z:F2}");}
  File.WriteAllText($"{o}/F-coplanar{tag}.txt",sb.ToString());EditorApplication.Exit(0);}
}
