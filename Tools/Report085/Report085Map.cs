using System;using System.IO;using System.Linq;using System.Text;using System.Collections.Generic;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;using Racer;
// 0.85 (read only): top-down height map of a region of a scene as a PNG (topmost drivable collider; trees and triggers
// ignored), with water footprints (blue), the main route (white), branches (gold), gates (red) and a 10 m grid.
// PROBE_SCENES=scene, MAP="x0,z0,x1,z1,cell[,yLow,yHigh]", MAP_TAG=name. Also writes the heights as CSV.
public static class Report085Map {
 static bool Skip(Collider c)=>c.isTrigger||c.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0||c.name.IndexOf("tree",StringComparison.OrdinalIgnoreCase)>=0||(c.attachedRigidbody&&!c.attachedRigidbody.isKinematic);
 public static float Top(float x,float z){float best=float.NaN;foreach(var h in Physics.RaycastAll(new Vector3(x,400,z),Vector3.down,800,~0,QueryTriggerInteraction.Ignore)){if(Skip(h.collider))continue;if(float.IsNaN(best)||h.point.y>best)best=h.point.y;}return best;}
 public static void Run(){var o=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(o);var tag=Environment.GetEnvironmentVariable("MAP_TAG")??"map";
  var f=Environment.GetEnvironmentVariable("MAP").Split(',').Select(x=>float.Parse(x,System.Globalization.CultureInfo.InvariantCulture)).ToArray();
  foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){var s=EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();var roots=s.GetRootGameObjects();
   float x0=f[0],z0=f[1],x1=f[2],z1=f[3],cell=f[4];int W=Mathf.CeilToInt((x1-x0)/cell),H=Mathf.CeilToInt((z1-z0)/cell);var hs=new float[W,H];float lo=float.MaxValue,hi=float.MinValue;
   var csv=new StringBuilder();
   for(int k=0;k<H;k++){for(int i=0;i<W;i++){float y=Top(x0+(i+.5f)*cell,z0+(k+.5f)*cell);hs[i,k]=y;if(!float.IsNaN(y)){lo=Mathf.Min(lo,y);hi=Mathf.Max(hi,y);}csv.Append(float.IsNaN(y)?"":y.ToString("F2")).Append(i<W-1?",":"\n");}}
   if(f.Length>6){lo=f[5];hi=f[6];}
   int sc=f.Length>7?(int)f[7]:Mathf.Max(1,Mathf.RoundToInt(2/cell));var tex=new Texture2D(W*sc,H*sc,TextureFormat.RGBA32,false);
   for(int i=0;i<W;i++)for(int k=0;k<H;k++){float y=hs[i,k];Color c;if(float.IsNaN(y))c=Color.black;else{float t=Mathf.InverseLerp(lo,hi,y);c=Color.HSVToRGB(.75f*(1-t),.55f,.55f+.45f*((Mathf.Floor(y)%2+2)%2));}
    for(int a=0;a<sc;a++)for(int b=0;b<sc;b++)tex.SetPixel(i*sc+a,k*sc+b,c);}
   void Dot(Vector3 p,Color c,int r=1){int px=Mathf.RoundToInt((p.x-x0)/cell*sc),pz=Mathf.RoundToInt((p.z-z0)/cell*sc);for(int a=-r;a<=r;a++)for(int b=-r;b<=r;b++){int u=px+a,v=pz+b;if(u>=0&&v>=0&&u<W*sc&&v<H*sc)tex.SetPixel(u,v,c);}}
   void Line(Vector3 a,Vector3 b,Color c,int r=1){float d=Vector3.Distance(new Vector3(a.x,0,a.z),new Vector3(b.x,0,b.z));int n=Mathf.Max(1,Mathf.CeilToInt(d/(cell*.5f)));for(int j=0;j<=n;j++)Dot(Vector3.Lerp(a,b,j/(float)n),c,r);}
   for(float gx=Mathf.Ceil(x0/10)*10;gx<x1;gx+=10)Line(new Vector3(gx,0,z0),new Vector3(gx,0,z1),new Color(0,0,0,1),0);
   for(float gz=Mathf.Ceil(z0/10)*10;gz<z1;gz+=10)Line(new Vector3(x0,0,gz),new Vector3(x1,0,gz),new Color(0,0,0,1),0);
   foreach(var w in roots.SelectMany(g=>g.GetComponentsInChildren<ShallowWater>(true))){var t=w.transform;if(w.round){for(int j=0;j<96;j++){float a=j*Mathf.PI*2/96,b=(j+1)*Mathf.PI*2/96;Line(t.TransformPoint(new Vector3(Mathf.Cos(a)*.5f,0,Mathf.Sin(a)*.5f)),t.TransformPoint(new Vector3(Mathf.Cos(b)*.5f,0,Mathf.Sin(b)*.5f)),Color.blue);}}
    else{var cs=new[]{new Vector3(-.5f,0,-.5f),new Vector3(.5f,0,-.5f),new Vector3(.5f,0,.5f),new Vector3(-.5f,0,.5f)}.Select(t.TransformPoint).ToArray();for(int j=0;j<4;j++)Line(cs[j],cs[(j+1)%4],Color.blue);}}
   var mainRoad=roots.SelectMany(g=>g.GetComponentsInChildren<RaceDirector>(true)).Select(d=>d.road).FirstOrDefault();foreach(var rd in roots.SelectMany(g=>g.GetComponentsInChildren<RaceRoad>(true)))for(int j=0;j+1<rd.points.Length;j++)Line(rd.points[j],rd.points[j+1],rd==mainRoad?Color.white:new Color(.35f,.35f,.35f),rd==mainRoad?1:0);
   foreach(var br in roots.SelectMany(g=>g.GetComponentsInChildren<WoodlandRoute>(true)))for(int j=0;j+1<br.points.Length;j++)Line(br.points[j],br.points[j+1],new Color(1,.8f,0));
   foreach(var d in roots.SelectMany(g=>g.GetComponentsInChildren<RaceDirector>(true)))foreach(var g in d.gates)Dot(g.transform.position,Color.red,3);
   File.WriteAllBytes($"{o}/map-{tag}-{scene}.png",tex.EncodeToPNG());File.WriteAllText($"{o}/map-{tag}-{scene}.csv",$"# x0 {x0} z0 {z0} cell {cell} W {W} H {H} rows=z ascending, cols=x ascending; colour range {lo:F1}..{hi:F1}\n"+csv);}
  EditorApplication.Exit(0);}
}
