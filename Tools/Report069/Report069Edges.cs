using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
// 0.69 read-only Part A sweep: for every Mountain route (main + active branches), both sides, every 0.5 m, the real
// pavement edge (walked out from the centre line), the ground just outside it and protection flags. Analysed by
// edge_stats.py (sawtooth = edge position jumps along the road; step = ground not flush with the pavement edge).
// Flags: J jump window (pavement gap -60..+10 m), F flight system, M another route above/below (multi-level),
// T covered (tunnel/cut cap overhead), B 0.68 berm beside the edge.
public static class Report069Edges {
 static bool IsDrive(Collider c)=>c&&c.name.Contains("driving surface");
 static bool Solid(Collider c)=>c&&!c.isTrigger&&!c.attachedRigidbody;
 public sealed class R{public string n;public Func<float,Vector3> at;public Func<float,Vector3> fw;public float len;public Func<float,float> hw;}
 public static List<R> Routes(){var race=Object.FindAnyObjectByType<RaceDirector>(FindObjectsInactive.Include);var road=race.road;road.Initialize();
  var list=new List<R>{new(){n="Main",at=x=>road.At(x,out _),fw=x=>{road.At(x,out var f);return f;},len=road.Length,hw=x=>road.HalfWidth(x)}};
  foreach(var b in Object.FindObjectsByType<WoodlandRoute>().Where(b=>b.gameObject.activeInHierarchy)){b.Initialize();var bb=b;list.Add(new(){n=b.title,at=x=>bb.At(x,out _),fw=x=>{bb.At(x,out var f);return f;},len=b.Length,hw=x=>bb.halfWidth});}return list;}
 public static bool Pave(Vector3 q,float near,out float y){y=0;var o=new Vector3(q.x,near+.6f,q.z);float depth=1.2f;
  for(int k=0;k<6&&depth>0&&Physics.Raycast(o,Vector3.down,out var h,depth,~0,QueryTriggerInteraction.Ignore);k++){if(IsDrive(h.collider)&&h.normal.y>.3f){y=h.point.y;return true;}depth-=h.distance+.01f;o=h.point+Vector3.down*.01f;}return false;}
 // First solid upward non-pavement surface below 'from' (ground beside the road).
 public static bool Ground(Vector3 q,float from,float depth,out RaycastHit hit){var o=new Vector3(q.x,from,q.z);hit=default;
  for(int k=0;k<12&&depth>0&&Physics.Raycast(o,Vector3.down,out var h,depth,~0,QueryTriggerInteraction.Ignore);k++){if(Solid(h.collider)&&!IsDrive(h.collider)&&h.normal.y>.05f){hit=h;return true;}depth-=h.distance+.01f;o=h.point+Vector3.down*.01f;}return false;}
 // Distance from the centre line to the last pavement sample walking outward (0.05 m steps); NaN if the centre is unpaved.
 public static float EdgeW(R r,float s,int side,out float py,out Vector3 edge){var c=r.at(s);var f=Vector3.ProjectOnPlane(r.fw(s),Vector3.up).normalized;var right=Vector3.Cross(Vector3.up,f)*side;edge=c;
  py=0;if(!Physics.Raycast(new Vector3(c.x,c.y+2,c.z),Vector3.down,out var ch,4,~0,QueryTriggerInteraction.Ignore)||!IsDrive(ch.collider)){if(!Pave(c,c.y,out py))return float.NaN;}else py=ch.point.y;float w=0;for(;w<16;w+=.05f){if(!Pave(c+right*(w+.05f),py,out float ny))break;py=ny;}edge=c+right*w;edge.y=py;return w;}
 public static void Run(){var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(outDir);Physics.queriesHitBackfaces=false;var tag=Environment.GetEnvironmentVariable("PROBE_TAG")??"edges";
  foreach(var scene in (Environment.GetEnvironmentVariable("PROBE_SCENES")??"MountainLoop,MountainLoopReverse").Split(',')){
   EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();
   var race=Object.FindAnyObjectByType<RaceDirector>(FindObjectsInactive.Include);var exclusions=Object.FindObjectsByType<JumpRecoveryExclusion>(FindObjectsInactive.Include);var actJumps=Object.FindObjectsByType<ActivitySite>(FindObjectsInactive.Include).Where(a=>a.kind==ActivitySite.Kind.Jump).ToArray();var routes=Routes();var flights=Object.FindAnyObjectByType<MountainFlights>(FindObjectsInactive.Include);var rows=new List<string>{"route,side,s,x,y,z,w,hw,g03,g10,g20,g30,flags,g03name"};
   foreach(var r in routes){
    var guide=r.n=="Main"?null:Object.FindObjectsByType<WoodlandRoute>().First(b=>b.title==r.n).GetComponent<ReverseShortcutGuidance>();
    foreach(int side in new[]{-1,1})for(float s=0;s<r.len;s+=.5f){float w=EdgeW(r,s,side,out float py,out var e);if(float.IsNaN(w))continue;
     var c=r.at(s);var f=Vector3.ProjectOnPlane(r.fw(s),Vector3.up).normalized;var right=Vector3.Cross(Vector3.up,f)*side;
     // Same protection rule as Report069Author.Protected (game jump exclusions, branch flight windows, flight lip..end).
     string flags="";if(exclusions.Any(z=>z.Contains(e))||(guide&&guide.takeoff>=0&&s>=guide.takeoff-15&&s<=guide.landing+10))flags+="J";
     if(r.n=="Main"&&flights)foreach(var fl in flights.flights)if(s>=fl.approachStation-40&&s<=fl.endStation){flags+="F";break;}
     {var q0=r.at(s);if(actJumps.Any(a=>new Vector2(a.transform.position.x-q0.x,a.transform.position.z-q0.z).magnitude<40&&Mathf.Abs(a.transform.position.y-q0.y)<12)&&!flags.Contains("J"))flags+="J";}
     var probe=e+right*1.5f;foreach(var o in routes){float os=Near(o,probe,out float lat,out float dy);if(o.n==r.n&&Mathf.Abs(os-s)<40)continue;if(lat<o.hw(os)+3&&Mathf.Abs(dy)>1.5f&&Mathf.Abs(dy)<40){flags+="M";break;}}
     if(Physics.Raycast(e+Vector3.up*.3f,Vector3.up,out var up,10,~0,QueryTriggerInteraction.Ignore)&&Solid(up.collider))flags+="T";
     if(Pave(e+right*.6f,py,out _)||Pave(e+right*1.4f,py,out _))flags+="X";
     float G(float d,out string nm){nm="";if(Ground(e+right*d,py+2.5f,30,out var h)){nm=h.collider.name;if(nm.Contains("Report068 barrier"))nm="BERM";return h.point.y-py;}return float.NaN;}
     float g03=G(.3f,out var n03),g10=G(1f,out var n10),g20=G(2f,out _),g30=G(3f,out _);if(n03=="BERM"||n10=="BERM")flags+="B";
     rows.Add($"{r.n},{side},{s:F1},{e.x:F2},{e.y:F2},{e.z:F2},{w:F2},{r.hw(s):F2},{g03:F3},{g10:F3},{g20:F3},{g30:F3},{flags},{n03.Replace(',',' ')}");}}
   File.WriteAllLines($"{outDir}/{tag}-{scene}.csv",rows);
   var ms=new List<string>();foreach(var mc in Object.FindObjectsByType<MeshCollider>().Where(IsDrive)){var m=mc.sharedMesh;var mf=mc.GetComponent<MeshFilter>();
    var v=m.vertices;var t=m.triangles;var key=v.Select(x=>(Mathf.RoundToInt(x.x*100),Mathf.RoundToInt(x.y*100),Mathf.RoundToInt(x.z*100))).ToArray();var ec=new Dictionary<((int,int,int),(int,int,int)),int>();
    for(int i=0;i<t.Length;i+=3)for(int k=0;k<3;k++){var a=key[t[i+k]];var b=key[t[i+(k+1)%3]];var e=a.CompareTo(b)<0?(a,b):(b,a);ec[e]=ec.TryGetValue(e,out int n)?n+1:1;}
    ms.Add($"{mc.name} | {AssetDatabase.GetAssetPath(m)} | verts {v.Length} unique {key.Distinct().Count()} tris {t.Length/3} boundary edges {ec.Count(x=>x.Value==1)} | renderer mesh same={(mf&&mf.sharedMesh==m)} | mat={(mc.GetComponent<Renderer>()?mc.GetComponent<Renderer>().sharedMaterial.name:"-")} colours={m.colors.Length}");}
   File.WriteAllLines($"{outDir}/{tag}-{scene}-meshes.txt",ms);}
  EditorApplication.Exit(0);}
 public static float Near(R r,Vector3 p,out float lat,out float dy){float best=float.MaxValue,bs=0;for(float s=0;s<=r.len;s+=2){var q=r.at(s);float d=new Vector2(q.x-p.x,q.z-p.z).sqrMagnitude;if(d<best){best=d;bs=s;}}
  for(float s=Mathf.Max(0,bs-2);s<=Mathf.Min(r.len,bs+2);s+=.25f){var q=r.at(s);float d=new Vector2(q.x-p.x,q.z-p.z).sqrMagnitude;if(d<best){best=d;bs=s;}}lat=Mathf.Sqrt(best);dy=p.y-r.at(bs).y;return bs;}
}
