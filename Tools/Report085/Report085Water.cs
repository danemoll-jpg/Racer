using System;using System.IO;using System.Linq;using System.Text;using System.Collections.Generic;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;using Racer;
// 0.85 Part F census (read only): every ShallowWater in the scenes (PROBE_SCENES=a,b). For each: footprint, surface, the
// bed under it (deepest point) and the easiest way out: on a 0.5 m height grid of the drivable ground (highest collider
// below surface + 2.5 m; trees and loose props ignored) over the footprint plus 14 m, the path from the deepest point to
// dry ground (outside the water, ground at or above the surface) whose steepest single climb is least (minimax).
// "need" is that steepest climb as a grade (rise/run): about 0.6 or more is a lip or wall a vehicle cannot climb in water.
public static class Report085Water {
 static string P(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n;}
 static bool Skip(Collider c)=>c.isTrigger||c.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0||c.name.IndexOf("tree",StringComparison.OrdinalIgnoreCase)>=0||(c.attachedRigidbody&&!c.attachedRigidbody.isKinematic);
 public static float Ground(Vector3 p,float top){float best=float.NaN;foreach(var h in Physics.RaycastAll(new Vector3(p.x,top,p.z),Vector3.down,top-p.y+60,~0,QueryTriggerInteraction.Ignore)){if(Skip(h.collider))continue;if(float.IsNaN(best)||h.point.y>best)best=h.point.y;}return best;}
 static List<ShallowWater> all=new();
 // Wet: inside any water of the scene whose surface is above the ground there.
 static bool Wet(Vector3 p,float ground){if(float.IsNaN(ground))return true;foreach(var x in all)if(x.Contains(p)&&x.Surface>ground+.02f)return true;return false;}
 // Shore profile: 16 rays from the water's centre; on each, the steepest climb (over 1 m) between leaving this water's bed
 // and reaching dry ground. "-" = no dry ground within reach.
 public static string Shores(ShallowWater w){var t=w.transform;float R=Mathf.Max(t.lossyScale.x,t.lossyScale.z)*.75f+16;var sb=new StringBuilder();string[] names={"N","NNE","NE","ENE","E","ESE","SE","SSE","S","SSW","SW","WSW","W","WNW","NW","NNW"};
  for(int d=0;d<16;d++){float a=d*22.5f*Mathf.Deg2Rad;var dir=new Vector3(Mathf.Sin(a),0,Mathf.Cos(a));float worst=0;bool dry=false,started=false;float prev=float.NaN;
   for(float r=0;r<R;r+=.5f){var p=t.position+dir*r;float g=Ground(p,w.Surface+30);if(float.IsNaN(g)){prev=g;continue;}
    if(!Wet(p,g)){if(started){if(!float.IsNaN(prev))worst=Mathf.Max(worst,(g-prev)/.5f);dry=true;break;}}else started=true;
    if(started&&!float.IsNaN(prev))worst=Mathf.Max(worst,(g-prev)/.5f);prev=g;}
   sb.Append($"{names[d]}:{(dry?worst.ToString("F2"):"-")} ");}
  return sb.ToString().Trim();}
 public struct Exit{public float need,depth;public Vector3 deep,dry;public string dir;}
 public static Exit Escape(ShallowWater w){var t=w.transform;var b=new Bounds(t.position,Vector3.zero);
  foreach(var c in new[]{new Vector3(-.5f,0,-.5f),new Vector3(.5f,0,-.5f),new Vector3(-.5f,0,.5f),new Vector3(.5f,0,.5f)})b.Encapsulate(t.TransformPoint(c));
  b.Expand(new Vector3(28,0,28));const float cell=.5f;int nx=Mathf.CeilToInt(b.size.x/cell),nz=Mathf.CeilToInt(b.size.z/cell);float S=w.Surface,top=S+30f;
  var h=new float[nx,nz];var wet=new bool[nx,nz];Vector3 At(int i,int k)=>new Vector3(b.min.x+(i+.5f)*cell,S,b.min.z+(k+.5f)*cell);
  int di=-1,dk=-1;float deepest=float.PositiveInfinity;
  for(int i=0;i<nx;i++)for(int k=0;k<nz;k++){var p=At(i,k);h[i,k]=Ground(p,top);wet[i,k]=Wet(p,h[i,k]);bool mine=w.Contains(p);if(mine&&!float.IsNaN(h[i,k])&&h[i,k]<deepest){deepest=h[i,k];di=i;dk=k;}}
  var r=new Exit{need=float.NaN,depth=S-deepest};if(di<0)return r;r.deep=new Vector3(At(di,dk).x,deepest,At(di,dk).z);
  var cost=new float[nx,nz];for(int i=0;i<nx;i++)for(int k=0;k<nz;k++)cost[i,k]=float.PositiveInfinity;cost[di,dk]=0;
  var open=new SortedSet<(float c,int i,int k)>();open.Add((0,di,dk));int[] ox={1,-1,0,0,1,1,-1,-1},oz={0,0,1,-1,1,-1,1,-1};
  while(open.Count>0){var cur=open.Min;open.Remove(cur);if(cur.c>cost[cur.i,cur.k])continue;
   if(!wet[cur.i,cur.k]){r.need=cur.c;r.dry=new Vector3(At(cur.i,cur.k).x,h[cur.i,cur.k],At(cur.i,cur.k).z);var d=r.dry-r.deep;r.dir=$"{(Mathf.Atan2(d.x,d.z)*Mathf.Rad2Deg+360)%360:F0}deg";return r;}
   for(int n=0;n<8;n++){int a=cur.i+ox[n],c=cur.k+oz[n];if(a<0||c<0||a>=nx||c>=nz||float.IsNaN(h[a,c]))continue;float run=cell*(n<4?1:1.4142f);
    float g=Mathf.Max(cur.c,Mathf.Max(0,(h[a,c]-h[cur.i,cur.k])/run));if(g<cost[a,c]){cost[a,c]=g;open.Add((g,a,c));}}}
  return r;}
 public static void Run(){var o=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(o);var sb=new StringBuilder("scene\tpath\tactive\tround\tcentre\tsize\tsurface\tdepth\tdeepest\tneed\texitDir\tdryAt\tshores\n");
  foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){var s=EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();
   all=s.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<ShallowWater>(true)).Where(x=>x.gameObject.activeInHierarchy).ToList();
   foreach(var w in s.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<ShallowWater>(true))){var t=w.transform;var e=Escape(w);
    sb.Append($"{scene}\t{P(t)}\t{w.gameObject.activeInHierarchy}\t{w.round}\t{t.position.x:F1},{t.position.z:F1}\t{t.lossyScale.x:F1}x{t.lossyScale.z:F1} rot {t.eulerAngles.y:F0}\t{w.Surface:F2}\t{e.depth:F2}\t{e.deep.x:F1},{e.deep.y:F2},{e.deep.z:F1}\t{e.need:F2}\t{e.dir}\t{e.dry.x:F1},{e.dry.y:F2},{e.dry.z:F1}\t{(t.lossyScale.x*t.lossyScale.z>40?Shores(w):"")}\n");}
   Debug.Log("REPORT085 water "+scene);}
  File.WriteAllText(o+"/water-census.tsv",sb.ToString());EditorApplication.Exit(0);}
}
