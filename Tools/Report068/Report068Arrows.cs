using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
// 0.68 Part F: redundant ground arrows (all courses). ARROWS_APPLY=1 deletes the arrows classified REMOVE and saves;
// otherwise dry run. Every arrow is listed with its classification in PROBE_OUT/arrows-<scene>.csv.
// Rules (PROJECT_TODO 0.68 Part F): remove arrows from another course / off this course, wrong-way arrows, duplicates
// of another arrow for the same instruction within 12 m, and repeated arrows on stretches with no turn or fork
// (one reassurance arrow is kept per 150 m). Keep arrows before turns, forks and junctions, gold shortcut entry/rejoin
// arrows and the main arrow at each fork, and arrows Dan asked for (0.67 BUG-020 straight arrow).
public static class Report068Arrows {
 public static readonly string[] Scenes={"StreetLoopGreybox","StreetLoopReverse","LakeWoods","ForestLoopReverse","DansBackyardForward","DansBackyardReverse","MountainLoop","MountainLoopReverse"};
 static string Path(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n.Replace('\n','/');}
 static float Heading(Vector3 f)=>Mathf.Atan2(f.x,f.z)*Mathf.Rad2Deg;
 sealed class Arrow{public MeshRenderer r;public string path,mesh,why="";public Vector3 c,dir;public bool gold,atlas,keepForced,remove;public string route;public float s,lat,dot,len;public int prio;}
 sealed class Route{public string n;public Func<float,Vector3> at;public Func<float,Vector3> fw;public float len,hw;public bool loop;public List<float> forks=new();}
 public static void Run(){var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(outDir);bool apply=Environment.GetEnvironmentVariable("ARROWS_APPLY")=="1";
  var summary=new List<string>();var only=Environment.GetEnvironmentVariable("PROBE_SCENES");
  foreach(var scene in Scenes.Where(x=>string.IsNullOrEmpty(only)||only.Split(',').Contains(x))){EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();
   var race=Object.FindAnyObjectByType<RaceDirector>(FindObjectsInactive.Include);var road=race.road;road.Initialize();
   var routes=new List<Route>{new(){n="Main",at=x=>road.At(x,out _),fw=x=>{road.At(x,out var f);return f;},len=road.Length,hw=road.HalfWidth(0),loop=!road.openHighway}};
   foreach(var b in Object.FindObjectsByType<WoodlandRoute>().Where(b=>b.gameObject.activeInHierarchy)){b.Initialize();var bb=b;routes.Add(new(){n=b.title,at=x=>bb.At(x,out _),fw=x=>{bb.At(x,out var f);return f;},len=b.Length,hw=b.halfWidth,forks={0,b.Length}});
    routes[0].forks.Add(b.entryRoad);routes[0].forks.Add(b.exitRoad);}
   var arrows=new List<Arrow>();
   foreach(var mr in Object.FindObjectsByType<MeshRenderer>()){if(!mr.enabled||mr.GetComponent<TextMesh>())continue;var mf=mr.GetComponent<MeshFilter>();if(!mf||!mf.sharedMesh)continue;var path=Path(mr.transform);
    string mesh=mf.sharedMesh.name;bool isArrow=mr.name.IndexOf("arrow",StringComparison.OrdinalIgnoreCase)>=0||path.StartsWith("Route atlas direction guidance")||mesh.IndexOf("arrow",StringComparison.OrdinalIgnoreCase)>=0||mesh.Contains("seated-marking");
    if(!isArrow||path.Contains("Gate")||mf.sharedMesh.vertexCount>16)continue;
    var vs=mf.sharedMesh.vertices.Select(v=>mf.transform.TransformPoint(v)).ToArray();var c=vs.Aggregate(Vector3.zero,(a,v)=>a+v)/vs.Length;
    // Principal horizontal axis; the head is the end with the wider lateral spread.
    float xx=0,xz=0,zz=0;foreach(var v in vs){float dx=v.x-c.x,dz=v.z-c.z;xx+=dx*dx;xz+=dx*dz;zz+=dz*dz;}float ang=.5f*Mathf.Atan2(2*xz,xx-zz);var axis=new Vector3(Mathf.Cos(ang),0,Mathf.Sin(ang));var side=Vector3.Cross(Vector3.up,axis);
    // The tip is the narrow extreme (one point); the tail end is the shaft's full width.
    var t=vs.Select(v=>Vector3.Dot(v-c,axis)).ToArray();float tmin=t.Min(),tmax=t.Max(),band=(tmax-tmin)*.1f;
    var lats=vs.Select(v=>Vector3.Dot(v-c,side)).ToArray();
    float Spread(Func<float,bool> at){var l=lats.Where((x,i)=>at(t[i])).ToArray();return l.Length==0?0:l.Max()-l.Min();}
    float wPos=Spread(x=>x>tmax-band),wNeg=Spread(x=>x<tmin+band);
    var dir=wPos<=wNeg?axis:-axis;string mat=mr.sharedMaterial?mr.sharedMaterial.name:"";
    bool gold=(path+mat).IndexOf("gold",StringComparison.OrdinalIgnoreCase)>=0||(path+mat).IndexOf("orange",StringComparison.OrdinalIgnoreCase)>=0||path.Contains("Optional");
    arrows.Add(new Arrow{r=mr,path=path,mesh=mesh,c=c,dir=dir,gold=gold,atlas=path.StartsWith("Route atlas"),len=tmax-tmin});}
   // Route association: best-aligned nearby route.
   foreach(var a in arrows){float bestLat=float.MaxValue;foreach(var r in routes){float s=Project(r,a.c,out float lat,out float dy);if(dy>5)continue;var f=Vector3.ProjectOnPlane(r.fw(s),Vector3.up).normalized;float dot=Vector3.Dot(a.dir,f);
     float score=lat-(lat<r.hw+4?dot*3:0);if(score<bestLat){bestLat=score;a.route=r.n;a.s=s;a.lat=lat;a.dot=dot;}}
    a.keepForced=scene=="DansBackyardForward"&&Vector2.Distance(new(a.c.x,a.c.z),new(70.54f,61.06f))<2.5f;
    a.prio=a.keepForced?3:a.atlas?2:a.path.Contains("Report067")||a.mesh.Contains("draped")||a.mesh.Contains("seated")?1:0;}
   string[] others=Scenes.Where(x=>x!=scene).ToArray();
   bool Inherited(Arrow a)=>others.Any(o=>a.mesh.StartsWith(o+"-")&&!a.mesh.StartsWith(scene+"-"));
   foreach(var a in arrows){var r=routes.FirstOrDefault(x=>x.n==a.route);
    if(r==null||a.lat>r.hw+10){a.remove=true;a.why="not on this course";continue;}
    // Wrong way: points backwards for every route it lies on.
    bool anyForward=routes.Any(x=>{float s=Project(x,a.c,out float l,out float dy);return dy<5&&l<x.hw+4&&Vector3.Dot(a.dir,Vector3.ProjectOnPlane(x.fw(s),Vector3.up).normalized)>-.3f;});
    if(!anyForward){a.remove=true;a.why="points the wrong way";continue;}
    if(Inherited(a))a.why="inherited";}
   bool Decision(Route r,float s){foreach(var f in r.forks){float d=r.loop?Mathf.Repeat(f-s+r.len/2,r.len)-r.len/2:f-s;if(d>-40&&d<80)return true;}
    for(float a=-10;a<=60;a+=5){float s0=s+a,s1=s+a+40;if(!r.loop&&(s0<0||s1>r.len))continue;if(Mathf.Abs(Mathf.DeltaAngle(Heading(r.fw(s0)),Heading(r.fw(s1))))>20)return true;}return false;}
   // Duplicates (same route, same instruction, within 12 m): keep the highest priority, then own-course over inherited.
   foreach(var g in arrows.Where(a=>!a.remove).GroupBy(a=>a.route)){var list=g.OrderByDescending(a=>a.prio).ThenBy(a=>a.why=="inherited"?1:0).ThenBy(a=>a.s).ToList();var r=routes.First(x=>x.n==g.Key);
    for(int i=0;i<list.Count;i++){if(list[i].remove)continue;for(int j=i+1;j<list.Count;j++){var b=list[j];if(b.remove||b.keepForced)continue;float d=r.loop?Mathf.Abs(Mathf.Repeat(b.s-list[i].s+r.len/2,r.len)-r.len/2):Mathf.Abs(b.s-list[i].s);
      if(d<12&&Vector3.Dot(b.dir,list[i].dir)>.8f&&b.gold==list[i].gold){b.remove=true;b.why=$"duplicate of {list[i].path} ({d:F0} m)";}}}}
   // Strings on stretches with no decision: one reassurance arrow per 150 m.
   foreach(var g in arrows.Where(a=>!a.remove).GroupBy(a=>a.route)){var r=routes.First(x=>x.n==g.Key);float lastKept=float.NegativeInfinity;var ordered=g.OrderBy(a=>a.s).ToList();
    foreach(var a in ordered){bool named=a.atlas&&(a.path.Contains("fork")||a.path.Contains("rejoin")||a.path.Contains("before"));bool keepAnyway=a.keepForced||named||Decision(r,a.s)||(a.gold&&r.n=="Main");
     if(keepAnyway){a.why=(a.why==""?"":a.why+"; ")+(a.keepForced?"Dan-requested":named?"fork/rejoin arrow":a.gold&&r.n=="Main"?"shortcut guidance at fork":"turn/fork ahead");lastKept=a.s;continue;}
     if(a.why=="inherited"){a.remove=true;a.why="inherited, no decision here";continue;}
     if(a.s-lastKept>=150){a.why="reassurance (long straight)";lastKept=a.s;continue;}
     a.remove=true;a.why=$"repeated on straight ({a.s-lastKept:F0} m after kept arrow)";}}
   // Flatness of kept arrows (0.67 BUG-006 rule): gap between arrow centre and the surface beneath it.
   var rows=new List<string>{"action,route,station,lateral,x,y,z,heading,gold,surfaceGap,reason,object,mesh"};int removed=0;
   foreach(var a in arrows.OrderBy(a=>a.route).ThenBy(a=>a.s)){float gap=float.NaN;var col=a.r.GetComponent<Collider>();
    var hits=Physics.RaycastAll(a.c+Vector3.up*2,Vector3.down,6,~0,QueryTriggerInteraction.Ignore).Where(h=>h.collider!=col).OrderBy(h=>h.distance).ToArray();if(hits.Length>0)gap=a.c.y-hits[0].point.y;
    rows.Add($"{(a.remove?"REMOVE":"keep")},{a.route},{a.s:F1},{a.lat:F1},{a.c.x:F2},{a.c.y:F2},{a.c.z:F2},{Heading(a.dir):F0},{a.gold},{gap:F2},\"{a.why}\",\"{a.path}\",{a.mesh}");if(a.remove)removed++;}
   File.WriteAllLines($"{outDir}/arrows-{scene}.csv",rows);
   var flagged=arrows.Where(a=>!a.remove).Count(a=>{var col=a.r.GetComponent<Collider>();var h=Physics.RaycastAll(a.c+Vector3.up*2,Vector3.down,6,~0,QueryTriggerInteraction.Ignore).Where(x=>x.collider!=col).OrderBy(x=>x.distance).FirstOrDefault();return h.collider&&Mathf.Abs(a.c.y-h.point.y)>.2f;});
   summary.Add($"{scene}: arrows before={arrows.Count} after={arrows.Count-removed} removed={removed} (not on course {arrows.Count(a=>a.remove&&a.why.StartsWith("not on"))}, wrong way {arrows.Count(a=>a.remove&&a.why.StartsWith("points"))}, duplicate {arrows.Count(a=>a.remove&&a.why.StartsWith("duplicate"))}, inherited {arrows.Count(a=>a.remove&&a.why.StartsWith("inherited"))}, repeated {arrows.Count(a=>a.remove&&a.why.StartsWith("repeated"))}); kept arrows >0.2 m off the surface: {flagged}");
   if(apply)foreach(var a in arrows.Where(a=>!a.remove)){var col=a.r.GetComponent<Collider>();var h=Physics.RaycastAll(a.c+Vector3.up*2,Vector3.down,6,~0,QueryTriggerInteraction.Ignore).Where(x=>x.collider!=col).OrderBy(x=>x.distance).FirstOrDefault();if(h.collider&&Mathf.Abs(a.c.y-h.point.y)>.2f)summary.Add("   "+Drape(a.r,.06f,scene));}
   if(apply&&removed>0){foreach(var a in arrows.Where(a=>a.remove))Object.DestroyImmediate(a.r.gameObject);var sc=UnityEngine.SceneManagement.SceneManager.GetActiveScene();EditorSceneManager.MarkSceneDirty(sc);EditorSceneManager.SaveScene(sc);}}
  File.WriteAllLines($"{outDir}/arrows-summary.txt",summary);EditorApplication.Exit(0);}
 static bool IsGround(Collider c)=>c&&!c.isTrigger&&!c.attachedRigidbody&&(c.name.Contains("driving surface")||c.name.StartsWith("Ground"));
 // Seat every vertex of a kept arrow on the surface beneath it (0.67 BUG-006 rule: arrows sit flat on the pavement).
 static string Drape(Renderer r,float lift,string scene){var mf=r.GetComponent<MeshFilter>();var v=mf.sharedMesh.vertices;var nv=new Vector3[v.Length];float worst=0;
  for(int i=0;i<v.Length;i++){var w=mf.transform.TransformPoint(v[i]);var hs=Physics.RaycastAll(w+Vector3.up*3,Vector3.down,8,~0,QueryTriggerInteraction.Ignore).Where(x=>IsGround(x.collider)&&x.normal.y>.5f).OrderBy(x=>Mathf.Abs(x.point.y-w.y)).ToArray();if(hs.Length==0)return $"drape skipped (unsupported vertex) '{r.name}' at {r.bounds.center:F1}";worst=Mathf.Max(worst,Mathf.Abs(w.y-hs[0].point.y-lift));w.y=hs[0].point.y+lift;nv[i]=mf.transform.InverseTransformPoint(w);}
  var m=Object.Instantiate(mf.sharedMesh);m.vertices=nv;m.RecalculateBounds();m.RecalculateNormals();Directory.CreateDirectory("Assets/Track/Report068");var path=$"Assets/Track/Report068/{scene}-draped-arrow-{Mathf.RoundToInt(r.bounds.center.x)}-{Mathf.RoundToInt(r.bounds.center.z)}.asset";AssetDatabase.CreateAsset(m,path);mf.sharedMesh=m;
  return $"draped kept arrow '{r.name}' at {r.bounds.center:F1} (max vertex correction {worst:F2} m) -> {path}";}
 static float Project(Route r,Vector3 p,out float lat,out float dy){float best=float.MaxValue,bs=0;for(float s=0;s<=r.len;s+=1){var q=r.at(s);float d=new Vector2(q.x-p.x,q.z-p.z).sqrMagnitude;if(Mathf.Abs(q.y-p.y)>6)d+=10000;if(d<best){best=d;bs=s;}}
  lat=Mathf.Sqrt(best%10000>0&&best>=10000?best-10000:best);var at=r.at(bs);dy=Mathf.Abs(at.y-p.y);lat=new Vector2(at.x-p.x,at.z-p.z).magnitude;return bs;}
}
