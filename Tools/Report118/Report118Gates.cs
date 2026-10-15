using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 0.118 Part B (read-only, temporary; scenes never saved): the gates Leviathan needs so that from every gate the next one is in view as you
// leave it, and every turn the course makes off a road has a gate in or just past it. Per course (PROBE_COURSES "LeviathanForward;LeviathanReverse"):
// the scene opened with the course's own ground (Forward's gully fill, Kyle's bank) and its line and gates from Tools/Report118/py/stage1-<course>.json,
// the off-road turns from turns118.json. Writes gates118-<course>.json (the gates to add) and gates118-<course>.txt (every pair checked, before / after).
// In view: from the driver's eye 8 m past the gate (1.5 m up, looking along the course there), the next gate's bar or posts reachable by sight
// (nothing solid between: the ground, buildings, trunks, rocks), within 250 m and 50 degrees of straight ahead. Tree crowns are not solid, so the recording shows the woods.
public static class Report118Gates {
 [Serializable] sealed class Turn { public string at; public float station; public bool corner; }
 [Serializable] sealed class Turns { public Turn[] LeviathanForward, LeviathanReverse; }
 [Serializable] public sealed class NewGate { public float station; public float[] pos; public float yaw, halfWidth; public bool mandatory; public string reason; }
 [Serializable] sealed class Out { public NewGate[] gates; }
 const float Range = 250, Cone = 50, Eye = 1.5f, Lead = 8;
 static Vector3[] P; static float[] S; static float L;
 static Vector3 At(float s){s=Mathf.Repeat(s,L);int lo=0,hi=S.Length-1;while(hi-lo>1){int m=(lo+hi)/2;if(S[m]<=s)lo=m;else hi=m;}float t=(s-S[lo])/Mathf.Max(1e-5f,S[hi]-S[lo]);return Vector3.Lerp(P[lo],P[hi],t);}
 static Vector3 Dir(float a,float b){var d=At(b)-At(a);d.y=0;return d.normalized;}
 static float Project(Vector3 q,float near,float window){float best=float.MaxValue,bs=near;for(int i=0;i+1<P.Length;i++){float rel=Mathf.Repeat(S[i]-near+L*.5f,L)-L*.5f;if(Mathf.Abs(rel)>window)continue;var a=P[i];var b=P[i+1];var ab=new Vector2(b.x-a.x,b.z-a.z);float t=Mathf.Clamp01(Vector2.Dot(new Vector2(q.x-a.x,q.z-a.z),ab)/Mathf.Max(1e-5f,ab.sqrMagnitude));var c=Vector3.Lerp(a,b,t);float d=new Vector2(q.x-c.x,q.z-c.z).magnitude;if(d<best){best=d;bs=S[i]+(S[i+1]-S[i])*t;}}return bs;}
 static bool Clear(Vector3 a,Vector3 b){foreach(var h in Physics.RaycastAll(a,(b-a).normalized,(b-a).magnitude,~0,QueryTriggerInteraction.Ignore))if(!h.collider.attachedRigidbody)return false;return true;}
 // whether a gate at station b (half width hw) is in view from a gate at station a; why: the reason when not
 static bool InView(float a,float b,float hw,out string why){
  var eye=At(a+Lead)+Vector3.up*Eye;var look=Dir(a+Lead,a+Lead+14);var g=At(b);var side=Vector3.Cross(Vector3.up,Dir(b-3,b+3)).normalized;
  float dist=Vector3.Distance(eye,g);if(dist>Range){why=$"{dist:F0} m away";return false;}
  var to=g-eye;to.y=0;float ang=Vector3.Angle(look,to);if(ang>Cone&&dist>25){why=$"{ang:F0} deg off ahead";return false;}
  var pts=new[]{g+Vector3.up*4f,g+Vector3.up*4f+side*hw,g+Vector3.up*4f-side*hw,g+Vector3.up*2.5f+side*hw,g+Vector3.up*2.5f-side*hw};
  int seen=pts.Count(q=>Clear(eye,q));why=seen>=2?"":$"hidden ({seen}/5 points seen, {dist:F0} m)";return seen>=2;}
 static bool In(float[] spans,float s){if(spans==null)return false;for(int i=0;i+1<spans.Length;i+=2)if(s>=spans[i]&&s<=spans[i+1])return true;return false;}
 public static void Run(){
  var o=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(o);
  var turns=JsonUtility.FromJson<Turns>(File.ReadAllText("Tools/Report118/py/turns118.json"));
  foreach(var name in Environment.GetEnvironmentVariable("PROBE_COURSES").Split(';')){
   var log=new List<string>();
   try{
    var d=JsonUtility.FromJson<Racer.LeviathanCourse.Data>(File.ReadAllText($"Tools/Report118/py/stage1-{name}.json"));
    var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+d.scene+".unity");
    foreach(var f in d.fills??new string[0]){var mesh=AssetDatabase.LoadAssetAtPath<Mesh>($"Assets/Resources/Leviathan/{f}.asset");if(mesh){var g=new GameObject("Ground_fill (probe)");g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshCollider>().sharedMesh=mesh;}}
    var reshape=typeof(Racer.LeviathanCourse).GetMethod("ReshapeGround",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static);
    foreach(var rs in d.reshape??new Racer.LeviathanCourse.Reshape[0])reshape.Invoke(null,new object[]{rs,scene});
    Physics.SyncTransforms();
    P=d.Points;S=new float[P.Length];for(int i=1;i<P.Length;i++)S[i]=S[i-1]+Vector3.Distance(P[i-1],P[i]);L=S[^1]+Vector3.Distance(P[^1],P[0]);
    // no gate in a flight (take-off to the landing's end) or on a kicker's run
    var none=new List<float>();foreach(var f in d.flights??new Racer.MountainFlights.Flight[0]){none.Add(f.approachStation);none.Add(f.endStation);}
    var gates=d.gates.Select((g,i)=>(s:Project(new Vector3(g.pos[0],g.pos[1],g.pos[2]),g.station,40),hw:g.halfWidth,m:g.mandatory,name:g.name,added:false,reason:"")).ToList();
    log.Add($"== {d.title}: line {L:F1} m, {gates.Count} gates; no gate in {string.Join(", ",Enumerable.Range(0,none.Count/2).Select(i=>$"{none[2*i]:F0}-{none[2*i+1]:F0}"))}");
    // a lap in course order: the start / finish (gate 0) first, by its station relative to it
    float s0=gates[0].s;float Rel(float s)=>Mathf.Repeat(s-s0,L);
    float Hw(float s)=>In(d.narrow,s)?5:In(d.mountain,s)?7:6;
    bool Mand(float s){var r=Rel(s);var before=gates.Where(g=>Rel(g.s)<r).OrderBy(g=>Rel(g.s)).LastOrDefault();var after=gates.Where(g=>Rel(g.s)>r).OrderBy(g=>Rel(g.s)).FirstOrDefault();return before.m&&after.m;}
    float Free(float s){for(int i=0;i+1<none.Count;i+=2)if(s>=none[i]&&s<=none[i+1])return none[i+1]+5;return s;}
    // a flight between two gates, or the first one in a flight (Downhill Ridge Cut's gate is on its drop): the next one is past its landing
    // (you are in the air): not checked
    bool Flies(float a,float b){for(int i=0;i+1<none.Count;i+=2){float r=Mathf.Repeat(none[i]-a,L);if(r>0&&r<Mathf.Repeat(b-a,L))return true;if(a>=none[i]&&a<=none[i+1])return true;}return false;}
    bool Near(float s){return gates.Any(g=>Mathf.Abs(Mathf.Repeat(g.s-s+L*.5f,L)-L*.5f)<10);}
    // 1. the turns off a road: a gate in or just past each (one already from 15 m before to 45 m after it does)
    foreach(var t in (name=="LeviathanForward"?turns.LeviathanForward:turns.LeviathanReverse).Where(t=>!t.corner)){
     var have=gates.FirstOrDefault(g=>{float r=Mathf.Repeat(g.s-t.station+L*.5f,L)-L*.5f;return r>=-15&&r<=45;});
     if(have.name!=null){log.Add($"  turn '{t.at}' (s {t.station:F0}): {have.name} at s {have.s:F0} already");continue;}
     float s=Free(t.station+15);gates.Add((s,Hw(s),Mand(s),"",true,$"just past the turn '{t.at}'"));log.Add($"  turn '{t.at}' (s {t.station:F0}): a gate added at s {s:F0}");}
    // 2. from every gate the next one in view: when not, a gate added as far on as can be seen, until it is
    var excused=new HashSet<float>();
    for(int guard=0;guard<200;guard++){
     var order=gates.OrderBy(g=>Rel(g.s)).ToList();bool changed=false;
     for(int i=0;i<order.Count;i++){
      var a=order[i];var b=order[(i+1)%order.Count];float ra=Rel(a.s),rb=i+1<order.Count?Rel(b.s):L;
      if(excused.Contains(a.s)||Flies(a.s,b.s)||InView(a.s,b.s,b.hw,out _))continue;
      // halfway when both halves are in view (the gates evenly spaced), else as far on as can be seen
      float best=-1;{float mid=Mathf.Repeat(ra+(rb-ra)*.5f+s0,L);if(!In(none.ToArray(),mid)&&InView(a.s,mid,Hw(mid),out _)&&InView(mid,b.s,b.hw,out _))best=mid;}
      if(best<0)for(float r=rb-20;r>=ra+30;r-=5){float s=Mathf.Repeat(r+s0,L);if(In(none.ToArray(),s))continue;if(InView(a.s,s,Hw(s),out _)){best=s;break;}}
      if(best<0){float r=ra+Mathf.Min(70,(rb-ra)*.5f);best=Free(Mathf.Repeat(r+s0,L));}
      if(Near(best)){log.Add($"  {(a.name??"the gate added at s "+a.s.ToString("F0"))} -> s {b.s:F0}: no place for a gate between that can be seen (left as it is)");excused.Add(a.s);continue;}
      gates.Add((best,Hw(best),Mand(best),"",true,$"the next gate in view from {(a.name??"the gate added at s "+a.s.ToString("F0"))}"));changed=true;break;}
     if(!changed)break;}
    // 3. an added gate within 30 m of the next one is left out when the gate before it sees that next one anyway
    for(bool dropped=true;dropped;){dropped=false;var order=gates.OrderBy(g=>Rel(g.s)).ToList();
     for(int i=0;i<order.Count;i++){var g=order[i];if(!g.added||g.reason.StartsWith("just past"))continue;var prev=order[(i-1+order.Count)%order.Count];var next=order[(i+1)%order.Count];
      if(Mathf.Repeat(next.s-g.s,L)<30&&(Flies(prev.s,next.s)||InView(prev.s,next.s,next.hw,out _))){log.Add($"  the gate added at s {g.s:F0} ({g.reason}) left out: {Mathf.Repeat(next.s-g.s,L):F0} m before the next, which is in view from the one before");gates.Remove(g);dropped=true;break;}}}
    var final=gates.OrderBy(g=>Rel(g.s)).ToList();
    // the report: every pair, in view or not, with the 0.117 gates only and with the added ones
    var old=final.Where(g=>!g.added).ToList();
    log.Add("  0.117 gates, each to the next:");
    for(int i=0;i<old.Count;i++){var a=old[i];var b=old[(i+1)%old.Count];bool ok=InView(a.s,b.s,b.hw,out string why);if(Flies(a.s,b.s)){ok=true;why="";}log.Add($"    {a.name,-44} s {a.s,7:F0} -> {b.name,-44} {Mathf.Repeat(b.s-a.s,L),5:F0} m: {(ok?"in view":"NOT in view: "+why)}");}
    log.Add($"  with the gates added ({final.Count(g=>g.added)}), each to the next:");
    for(int i=0;i<final.Count;i++){var a=final[i];var b=final[(i+1)%final.Count];bool ok=InView(a.s,b.s,b.hw,out string why);string fl=Flies(a.s,b.s)?" (a flight between: not checked)":"";log.Add($"    {(a.added?"NEW "+a.reason:a.name),-60} s {a.s,7:F0} -> {Mathf.Repeat(b.s-a.s,L),5:F0} m: {(ok?"in view":"NOT in view: "+why)}{fl}");}
    var outList=final.Where(g=>g.added).Select(g=>{var p=At(g.s);var f=Dir(g.s-3,g.s+3);return new NewGate{station=g.s,pos=new[]{p.x,p.y+1.5f,p.z},yaw=Mathf.Repeat(Mathf.Atan2(f.x,f.z)*Mathf.Rad2Deg,360),halfWidth=g.hw,mandatory=g.m,reason=g.reason};}).ToArray();
    File.WriteAllText(Path.Combine(o,$"gates118-{name}.json"),JsonUtility.ToJson(new Out{gates=outList},true));
   }catch(Exception e){log.Add("ERROR "+e);}
   File.WriteAllLines(Path.Combine(o,$"gates118-{name}.txt"),log);}
  EditorApplication.Exit(0);}
}
