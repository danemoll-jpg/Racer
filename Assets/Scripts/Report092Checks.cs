#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Racer {
// 0.92 targeted checks, added to the 0.80 runner (muted, isolated save).
//  unlock92                          Testing on (every vehicle and course), for the rides below
//  surface92:Scene:from:to:halfWidth  the main's driving surface sampled every 0.25 m across and 0.5 m along: height steps,
//                                     kinks and collider seams that could throw a bike (lists the worst, CSV of all)
public sealed partial class Report080Checks {
 IEnumerator Run092(string[] a)=>a[0] switch{"unlock92"=>Unlock092(),"surface92"=>Surface092(a[1],F(a[2]),F(a[3]),F(a[4])),"around92"=>Around092(a[1],a[2]),_=>Walk092(a)};
 // around92:Scene:x,y,z,r  every collider and renderer whose bounds come within r metres of the point (dependents check)
 IEnumerator Around092(string scene,string point){
  yield return Load(scene);yield return Menu();var v=point.Split(',').Select(F).ToArray();var c=new Vector3(v[0],v[1],v[2]);float r=v[3];
  var list=new List<string>();
  foreach(var col in FindObjectsByType<Collider>(FindObjectsSortMode.None)){if(!col.enabled||col.bounds.SqrDistance(c)>r*r)continue;list.Add($"collider {Path092(col.transform)} [{col.GetType().Name}{(col.isTrigger?", trigger":"")}] bounds {col.bounds.min:F1}..{col.bounds.max:F1}");}
  foreach(var ren in FindObjectsByType<Renderer>(FindObjectsSortMode.None)){if(!ren.enabled||ren.bounds.SqrDistance(c)>r*r||ren.bounds.size.magnitude>300)continue;list.Add($"renderer {Path092(ren.transform)} bounds {ren.bounds.min:F1}..{ren.bounds.max:F1}");}
  File.WriteAllLines($"{output}/around-{scene}-{v[0]:F0}-{v[2]:F0}.txt",list.OrderBy(x=>x));Note($"{scene} around {c} r {r}: {list.Count} colliders / renderers (list in around-{scene}-{v[0]:F0}-{v[2]:F0}.txt)");yield return Menu();}
 static string Path092(Transform t){var s=t.name;for(var p=t.parent;p&&s.Length<160;p=p.parent)s=p.name+"/"+s;return s;}
 IEnumerator Unlock092(){flow.Save.Settings.unlockEverything=true;Campaign.Testing=true;flow.Save.Settings.hints=false;flow.Save.SaveSettings();Note("Testing on: every vehicle and course");yield break;}
 IEnumerator Surface092(string scene,float from,float to,float half){
  yield return Load(scene);yield return Menu();var rd=race.road;rd.Initialize();to=Mathf.Min(to,rd.Length);
  int mask=~0;var vehicles=new HashSet<Collider>(FindObjectsByType<ArcadeVehicle>(FindObjectsSortMode.None).SelectMany(v=>v.GetComponentsInChildren<Collider>()));
  var csv=new StringBuilder("station,lateral,x,y,z,nx,ny,nz,collider\n");var finds=new List<(float score,string text)>();
  int lanes=Mathf.RoundToInt(half*2/.25f)+1;float[] prevY=new float[lanes],prev2Y=new float[lanes];string[] prevC=new string[lanes];Vector3[] prevN=new Vector3[lanes];bool[] have=new bool[lanes];
  for(float s=from;s<=to;s+=.5f){var c=rd.At(s,out var f);f.y=0;f.Normalize();var right=Vector3.Cross(Vector3.up,f);
   float[] rowY=new float[lanes];string[] rowC=new string[lanes];
   for(int i=0;i<lanes;i++){float lat=-half+i*.25f;var o=c+right*lat+Vector3.up*6;
    var hits=Physics.RaycastAll(o,Vector3.down,14,mask,QueryTriggerInteraction.Ignore).Where(h=>!vehicles.Contains(h.collider)).OrderBy(h=>h.distance).ToArray();
    if(hits.Length==0){have[i]=false;rowC[i]=null;continue;}var h0=hits[0];
    csv.AppendLine($"{s:F1},{lat:F2},{h0.point.x:F2},{h0.point.y:F3},{h0.point.z:F2},{h0.normal.x:F3},{h0.normal.y:F3},{h0.normal.z:F3},{h0.collider.name}");
    rowY[i]=h0.point.y;rowC[i]=h0.collider.name;
    // a second surface within 8 cm under the first: a buried sheet or a seam overlap
    var under=hits.Skip(1).FirstOrDefault(h=>h.collider!=h0.collider&&h.point.y>h0.point.y-.08f);
    if(under.collider)finds.Add((.5f,$"overlap at s {s:F1} lat {lat:+0.00;-0.00}: {h0.collider.name} over {under.collider.name} by {(h0.point.y-under.point.y)*100:F1} cm at {h0.point:F2}"));
    if(have[i]){
     // along the line: a step against the run of the slope (second difference over 0.5 m)
     float bump=Mathf.Abs((h0.point.y-prevY[i])-(prevY[i]-prev2Y[i]));
     if(bump>.05f)finds.Add((bump,$"kink {bump*100:F1} cm at s {s:F1} lat {lat:+0.00;-0.00} ({h0.point:F2}) on {h0.collider.name}{(prevC[i]!=h0.collider.name?" (seam from "+prevC[i]+")":"")}"));
     float tilt=Vector3.Angle(prevN[i],h0.normal);if(tilt>12)finds.Add((tilt/100f,$"normal turns {tilt:F0}° at s {s:F1} lat {lat:+0.00;-0.00} ({h0.point:F2}) on {h0.collider.name}"));}
    prev2Y[i]=have[i]?prevY[i]:h0.point.y;prevY[i]=h0.point.y;prevN[i]=h0.normal;prevC[i]=h0.collider.name;have[i]=true;}
   // across the road: a step between neighbouring samples
   for(int i=1;i<lanes;i++)if(rowC[i]!=null&&rowC[i-1]!=null){float step=Mathf.Abs(rowY[i]-rowY[i-1]);if(step>.06f)finds.Add((step,$"step across {step*100:F1} cm at s {s:F1} lat {-half+i*.25f:+0.00;-0.00} ({rowC[i-1]} / {rowC[i]})"));}}
  File.WriteAllText($"{output}/surface-{scene}-{from:F0}-{to:F0}.csv",csv.ToString());
  var worst=finds.OrderByDescending(x=>x.score).Take(40).ToList();
  File.WriteAllLines($"{output}/surface-{scene}-{from:F0}-{to:F0}-finds.txt",finds.OrderByDescending(x=>x.score).Select(x=>x.text));
  Note($"{scene} main {from}-{to} m, ±{half} m: {finds.Count} finds; worst: "+string.Join(" | ",worst.Take(15).Select(x=>x.text)));
  yield return Menu();}
}
}
#endif
