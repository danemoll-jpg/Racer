using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 0.92 Part E (editor only, copied into Assets/Editor/Report092Temp while it runs): Forest Loop Reverse, the main at about
// 1508-1510 m (175, 48, 290): the ground mesh steps up about 24 cm across the whole road where its gradient changes
// (0.113 before, 0.080 after), which throws a bike at race speed (1.1-1.3 s in the air). The road band of that one ground
// chunk (Ground_480_480, used by this scene only) is re-shaped between 1500 and 1518 m as one smooth curve joining the
// surface before and after it with their own slopes; the band fades back to the untouched ground from 8 to 11 m off the
// centre line. Nothing outside the band moves; normals are recomputed only for the vertices that moved and their neighbours.
public static class Report092Author {
 const string Scene="Assets/Scenes/ForestLoopReverse.unity",Centre="C:/Users/danmo/Racer/Docs/Report092/Lists/forest-reverse-main-centre-1470-1550.csv";
 const float S0=1500,S1=1518,Inner=8,Outer=11;
 public static void Lip(){
  string report="C:/Users/danmo/Racer/Docs/Report092/Lists/E-author.txt";var lines=new List<string>();
  try{
   EditorSceneManager.OpenScene(Scene);Physics.SyncTransforms();
   var centre=File.ReadAllLines(Centre).Skip(1).Select(l=>l.Split(',')).Select(c=>(s:float.Parse(c[0]),p:new Vector3(float.Parse(c[1]),float.Parse(c[2]),float.Parse(c[3])))).ToList();
   var go=GameObject.Find("Ground_480_480");var mf=go.GetComponent<MeshFilter>();var mc=go.GetComponent<MeshCollider>();var mesh=mf.sharedMesh;
   if(mc.sharedMesh!=mesh)throw new Exception("collider and renderer meshes differ");
   lines.Add($"{go.name}: mesh {AssetDatabase.GetAssetPath(mesh)}, {mesh.vertexCount} vertices, transform {go.transform.position} {go.transform.lossyScale}");
   // station and signed lateral (+ = right of travel) of a world point, from the centre line
   (float s,float l,float d) Project(Vector3 w){float best=1e9f,bs=0,bl=0;for(int i=0;i+1<centre.Count;i++){var a=centre[i].p;var b=centre[i+1].p;var ab=new Vector2(b.x-a.x,b.z-a.z);var ap=new Vector2(w.x-a.x,w.z-a.z);float t=Mathf.Clamp01(Vector2.Dot(ap,ab)/ab.sqrMagnitude);var q=new Vector2(a.x,a.z)+ab*t;float d=(new Vector2(w.x,w.z)-q).magnitude;if(d<best){best=d;bs=Mathf.Lerp(centre[i].s,centre[i+1].s,t);var f=ab.normalized;bl=f.x*ap.y-f.y*ap.x;bl=-bl;}}return(bs,bl,best);}
   Vector3 At(float s,float l){int i=Mathf.Clamp(centre.FindIndex(c=>c.s>=s)-1,0,centre.Count-2);var a=centre[i].p;var b=centre[i+1].p;float t=(s-centre[i].s)/(centre[i+1].s-centre[i].s);var p=Vector3.Lerp(a,b,t);var f=(b-a);f.y=0;f.Normalize();return p+Vector3.Cross(Vector3.up,f)*l;}
   float Height(float s,float l){var p=At(s,l);return mc.Raycast(new Ray(p+Vector3.up*20,Vector3.down),out var h,60)?h.point.y:float.NaN;}
   var local=mesh.vertices;var world=local.Select(v=>go.transform.TransformPoint(v)).ToArray();var moved=new List<int>();var newY=new Dictionary<int,float>();
   // the band's own end heights and slopes, per lateral, from the surface as it is (sampled before anything moves)
   var cache=new Dictionary<int,(float y0,float m0,float y1,float m1)>();
   (float,float,float,float) Ends(float l){int k=Mathf.RoundToInt(l*4);if(cache.TryGetValue(k,out var e))return e;float q=k/4f;
    float a0=Height(S0-1,q),b0=Height(S0,q),a1=Height(S1,q),b1=Height(S1+1,q);e=(b0,b0-a0,a1,b1-a1);cache[k]=e;return e;}
   for(int i=0;i<world.Length;i++){var w=world[i];if(Mathf.Abs(w.x-176)>40||Mathf.Abs(w.z-290)>40)continue;var(s,l,d)=Project(w);
    if(s<=S0||s>=S1||Mathf.Abs(l)>=Outer)continue;var(y0,m0,y1,m1)=Ends(l);if(float.IsNaN(y0)||float.IsNaN(y1)||float.IsNaN(m0)||float.IsNaN(m1))continue;
    float L=S1-S0,t=(s-S0)/L,t2=t*t,t3=t2*t;float h=(2*t3-3*t2+1)*y0+(t3-2*t2+t)*m0*L+(-2*t3+3*t2)*y1+(t3-t2)*m1*L;
    float wgt=Mathf.Abs(l)<=Inner?1:1-(Mathf.Abs(l)-Inner)/(Outer-Inner);float y=Mathf.Lerp(w.y,h,wgt);
    if(Mathf.Abs(y-w.y)<.002f)continue;newY[i]=y;moved.Add(i);}
   float maxUp=0,maxDown=0;foreach(var i in moved){float dy=newY[i]-world[i].y;maxUp=Mathf.Max(maxUp,dy);maxDown=Mathf.Min(maxDown,dy);var w=world[i];w.y=newY[i];local[i]=go.transform.InverseTransformPoint(w);}
   lines.Add($"band {S0}-{S1} m, ±{Inner} m (fading to ±{Outer} m): {moved.Count} vertices moved, raised up to {maxUp*100:F1} cm, lowered up to {-maxDown*100:F1} cm");
   if(moved.Count==0)throw new Exception("nothing to move");
   // normals: only for the moved vertices and the vertices sharing a triangle with them
   var tris=mesh.triangles;var touched=new HashSet<int>(moved);var near=new HashSet<int>(touched);for(int t=0;t<tris.Length;t+=3)if(touched.Contains(tris[t])||touched.Contains(tris[t+1])||touched.Contains(tris[t+2])){near.Add(tris[t]);near.Add(tris[t+1]);near.Add(tris[t+2]);}
   var oldNormals=mesh.normals;mesh.vertices=local;var copy=UnityEngine.Object.Instantiate(mesh);copy.RecalculateNormals();var fresh=copy.normals;UnityEngine.Object.DestroyImmediate(copy);
   var normals=(Vector3[])oldNormals.Clone();foreach(var i in near)normals[i]=fresh[i];mesh.normals=normals;mesh.RecalculateBounds();
   EditorUtility.SetDirty(mesh);AssetDatabase.SaveAssets();mc.sharedMesh=null;mc.sharedMesh=mesh;Physics.SyncTransforms();
   lines.Add($"normals recomputed for {near.Count} vertices; the rest of the chunk keeps its normals");
   // the centre line after: height every metre
   lines.Add("after, centre line: "+string.Join(", ",Enumerable.Range(1496,26).Select(s=>$"{s}:{Height(s,0):F2}")));
   File.WriteAllLines(report,lines);EditorApplication.Exit(0);
  }catch(Exception e){lines.Add("ERROR "+e);File.WriteAllLines(report,lines);EditorApplication.Exit(1);}
 }
}
