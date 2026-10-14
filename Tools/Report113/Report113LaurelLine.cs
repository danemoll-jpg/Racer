using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 0.113 Part A: the rivals' line on Laurel Switchbacks (StreetLoopReverse), WoodlandRoute.aiPoints only (no geometry, no other field):
// the route's own points up the valley to a leave station, a smooth connector to the House 3 valley driveway's foot, up the driveway
// (its own centre line, the ground's height), and back onto the route's own points where the driveway meets them near the top.
// Candidates (leave station x join point) are scored on the ground: the steepest grade, the tightest bend, and clearance from every
// solid that is not a Ground_ surface (fences, trees, walls) within 2.6 m of the line at 0.3-1.6 m above the ground. PROBE_PLAY=write saves.
public static class Report113LaurelLine {
 static string V(Vector3 v)=>$"{v.x:F1},{v.y:F2},{v.z:F1}";
 static float Ground(Vector3 p,out string what){what="-";var hs=Physics.RaycastAll(new Vector3(p.x,p.y+12,p.z),Vector3.down,30,~0,QueryTriggerInteraction.Ignore).Where(h=>!h.collider.attachedRigidbody&&h.collider.name.StartsWith("Ground_")).OrderBy(h=>Mathf.Abs(h.point.y-p.y)).ToArray();if(hs.Length==0)return float.NaN;what=hs[0].collider.name;return hs[0].point.y;}
 static List<Vector3> Resample(List<Vector3> pts,float step){var o=new List<Vector3>{pts[0]};float carry=0;for(int i=1;i<pts.Count;i++){var a=pts[i-1];var b=pts[i];float d=Vector2.Distance(new(a.x,a.z),new(b.x,b.z));float t=step-carry;while(t<=d){o.Add(Vector3.Lerp(a,b,t/d));t+=step;}carry=d-(t-step);}if(Vector3.Distance(o[^1],pts[^1])>.2f)o.Add(pts[^1]);return o;}
 // a cubic Bezier between two points with their headings (tangent lengths k)
 static List<Vector3> Connector(Vector3 a,Vector3 fa,Vector3 b,Vector3 fb){float d=Vector2.Distance(new(a.x,a.z),new(b.x,b.z));float k=d*.4f;var o=new List<Vector3>();for(int i=0;i<=40;i++){float t=i/40f,u=1-t;var p=u*u*u*a+3*u*u*t*(a+fa*k)+3*u*t*t*(b-fb*k)+t*t*t*b;o.Add(p);}return o;}
 static (float grade,float radius,float clear,string blocker,int gaps) Score(List<Vector3> line){
  float g=0,r=1e9f,c=99;string who="";int gaps=0;
  for(int i=1;i<line.Count;i++){float d=Vector2.Distance(new(line[i].x,line[i].z),new(line[i-1].x,line[i-1].z));if(d>.01f)g=Mathf.Max(g,Mathf.Abs(line[i].y-line[i-1].y)/d);}
  for(int i=2;i<line.Count-2;i++){var a=line[i-2];var b=line[i];var e=line[i+2];var u=new Vector2(b.x-a.x,b.z-a.z);var v=new Vector2(e.x-b.x,e.z-b.z);float ang=Vector2.Angle(u,v)*Mathf.Deg2Rad;float len=(u.magnitude+v.magnitude)*.5f;if(ang>.001f)r=Mathf.Min(r,len/ang);}
  foreach(var p in line){var cs=Physics.OverlapCapsule(p+Vector3.up*.3f,p+Vector3.up*1.6f,2.6f,~0,QueryTriggerInteraction.Ignore).Where(x=>!x.attachedRigidbody&&!x.name.StartsWith("Ground_")).ToArray();
   foreach(var x in cs){var q=x.ClosestPoint(p+Vector3.up*.9f);float dd=Vector2.Distance(new(q.x,q.z),new(p.x,p.z));if(dd<c){c=dd;who=x.name+"@"+V(q);}}
   if(float.IsNaN(p.y))gaps++;}
  return (g,r,c,who,gaps);
 }
 public static void Run(){
  var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");var log=new List<string>();bool write=Environment.GetEnvironmentVariable("PROBE_PLAY")=="write";
  var scene=EditorSceneManager.OpenScene("Assets/Scenes/StreetLoopReverse.unity");Physics.SyncTransforms();
  var route=UnityEngine.Object.FindObjectsByType<Racer.WoodlandRoute>(FindObjectsSortMode.None).Single(b=>b.title=="Laurel Switchbacks");route.Initialize();
  var drive=UnityEngine.Object.FindObjectsByType<Racer.RaceRoad>(FindObjectsSortMode.None).Single(r=>r.name=="House 3 valley driveway");
  var dp=drive.points.ToList();var dl=new List<float>{0};for(int i=1;i<dp.Count;i++)dl.Add(dl[^1]+Vector2.Distance(new(dp[i].x,dp[i].z),new(dp[i-1].x,dp[i-1].z)));
  Vector3 DAt(float L,out Vector3 f){int i=1;while(i<dl.Count-1&&dl[i]<L)i++;f=(dp[i]-dp[i-1]);f.y=0;f.Normalize();return Vector3.Lerp(dp[i-1],dp[i],Mathf.InverseLerp(dl[i-1],dl[i],L));}
  log.Add($"Laurel length {route.Length:F0}; driveway {dp.Count} pts length {dl[^1]:F0}; aiPoints before {(route.aiPoints==null?0:route.aiPoints.Length)}");
  // the top: where the driveway, climbing (decreasing L), comes within 2 m of the route
  float topL=-1,topS=-1;for(float L=60;L>=0;L-=.5f){var p=DAt(L,out _);float s=route.Project(p,out float lat);if(lat<2){topL=L;topS=s;break;}}
  log.Add($"top join: driveway L {topL:F1} -> route s {topS:F1}");
  var best=(score:float.MaxValue,line:(List<Vector3>)null,desc:"");
  foreach(float leave in new[]{150f,160f,170f,180f,190f,200f,210f}) foreach(float joinL in new[]{160f,165f,170f,175f,180f,185f}){
   var a=route.At(leave,out var fa);fa.y=0;fa.Normalize();var b=DAt(joinL,out var fb);fb=-fb; // climbing = decreasing L
   var pts=new List<Vector3>();for(float s=0;s<leave;s+=2)pts.Add(route.At(s,out _));
   pts.AddRange(Connector(a,fa,b,fb));
   for(float L=joinL-2;L>topL;L-=2)pts.Add(DAt(L,out _));
   for(float s=topS+2;s<=route.Length;s+=2)pts.Add(route.At(s,out _));pts.Add(route.At(route.Length,out _));
   var line=Resample(pts,2);for(int i=0;i<line.Count;i++){float y=Ground(line[i],out _);line[i]=new Vector3(line[i].x,float.IsNaN(y)?float.NaN:y,line[i].z);}
   // score the connector and the climb only (the route's own parts are as they are)
   int i0=Mathf.Max(0,(int)(leave/2)-3);var part=line.Skip(i0).Take(Mathf.Min(line.Count-i0,(int)((dl[(int)0]+joinL-topL+60)/2)+12)).ToList();
   var sc=Score(part);float cost=(sc.gaps>0?1000:0)+(sc.clear<1.2f?500:0)+Mathf.Max(0,sc.grade-.36f)*400+Mathf.Max(0,18-sc.radius)*10+(2.6f-Mathf.Min(2.6f,sc.clear))*20;
   string d=$"leave s {leave} join L {joinL}: steepest {sc.grade:F2} tightest {sc.radius:F1} m clearance {sc.clear:F1} m ({sc.blocker}) gaps {sc.gaps} cost {cost:F1}";log.Add(d);
   if(cost<best.score)best=(cost,line,d);
  }
  log.Add("BEST "+best.desc);
  var bl=best.line;float total=0;for(int i=1;i<bl.Count;i++)total+=Vector2.Distance(new(bl[i].x,bl[i].z),new(bl[i-1].x,bl[i-1].z));
  log.Add($"rivals' line {bl.Count} points, {total:F0} m (route {route.Length:F0} m); start {V(bl[0])} (route {V(route.points[0])}), end {V(bl[^1])} (route {V(route.points[^1])})");
  for(int i=0;i<bl.Count;i+=5){string w;Ground(bl[i],out w);float g=i>0?(bl[i].y-bl[i-5<0?0:i-5].y)/Mathf.Max(.1f,Vector2.Distance(new(bl[i].x,bl[i].z),new(bl[Math.Max(0,i-5)].x,bl[Math.Max(0,i-5)].z))):0;log.Add($"  {i*2,4} m {V(bl[i])} grade {g:+0.00;-0.00} {w}");}
  if(write){
   var so=new SerializedObject(route);var prop=so.FindProperty("aiPoints");prop.arraySize=bl.Count;for(int i=0;i<bl.Count;i++)prop.GetArrayElementAtIndex(i).vector3Value=bl[i];so.ApplyModifiedPropertiesWithoutUndo();
   EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);log.Add("written to the scene");
  }
  File.WriteAllLines(Path.Combine(outDir,"laurel-line.txt"),log);EditorApplication.Exit(0);
 }
}
