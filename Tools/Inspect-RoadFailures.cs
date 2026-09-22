using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
public static class InspectRoadFailures {
 public static string Main(){var m=GameObject.Find("Ground_CR133 mountain earth banks").GetComponent<MeshCollider>();var mesh=m.sharedMesh;var v=mesh.vertices;var ix=mesh.triangles;return string.Join("\n",new[]{390,450,480,42360,48300,50010}.Select(n=>{var a=v[ix[n*3]];var b=v[ix[n*3+1]];var c=v[ix[n*3+2]];var p=(a+b+c)/3;var normal=Vector3.Cross(b-a,c-a);bool hit=m.Raycast(new Ray(p+Vector3.up*10,Vector3.down),out var h,20);return $"triangle {n} a={a} b={b} c={c}; area={normal.magnitude*.5f:F8} normal={normal.normalized}; hit={hit} offset={h.point.y-p.y}";}));}
}
