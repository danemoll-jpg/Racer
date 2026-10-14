using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 0.115 (temporary; the scenes are never saved): seats Leviathan's blended / lane points (course "seat" list) and its new gates on the ground of the
// course's own scene, then surveys the whole line: ground under every point (gap / step), and a sweep between points at wheel height for anything
// solid (not a driving surface). Writes the seated course back (Assets/Resources/Leviathan) and the survey to PROBE_OUT/survey-<name>.txt.
public static class Report115Snap {
 static string P(Transform t){var s=t.name;while(t.parent){t=t.parent;s=t.name+"/"+s;}return s;}
 [Serializable] class Gate{public string name;public float[] pos,rot;public float halfWidth,halfHeight,upperHeight;public string reuse;public bool mandatory,seat;public float station;}
 static bool Ground(Vector3 p,float above,float below,out RaycastHit hit){
  var hits=Physics.RaycastAll(p+Vector3.up*above,Vector3.down,above+below,~0,QueryTriggerInteraction.Ignore).Where(h=>h.normal.y>.5f&&!h.collider.attachedRigidbody).OrderBy(h=>Mathf.Abs(h.point.y-p.y)).ToArray();
  hit=hits.FirstOrDefault();return hits.Length>0;}
 public static void Run(){
  var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(outDir);
  try{
   foreach(var name in new[]{"LeviathanForward","LeviathanReverse"}){
    var path="Assets/Resources/Leviathan/"+name+".json";var text=File.ReadAllText(path);
    var json=MiniJson.Parse(text) as Dictionary<string,object>;var sceneName=(string)json["scene"];
    EditorSceneManager.OpenScene("Assets/Scenes/"+sceneName+".unity");
    // the course's own ground patches, as LeviathanCourse adds them in the race (the scene is not saved)
    if(json.ContainsKey("fills"))foreach(var f in (List<object>)json["fills"]){var m=Resources.Load<Mesh>("Leviathan/"+(string)f);if(m){var g=new GameObject("Ground_"+m.name);g.AddComponent<MeshCollider>().sharedMesh=m;}}
    Physics.SyncTransforms();
    var flat=((List<object>)json["pts"]).Select(Convert.ToSingle).ToArray();var pts=new Vector3[flat.Length/3];for(int k=0;k<pts.Length;k++)pts[k]=new Vector3(flat[3*k],flat[3*k+1],flat[3*k+2]);
    var seat=new HashSet<int>(((List<object>)json["seat"]).Select(Convert.ToInt32));
    var o=new List<string>();int seated=0,unseated=0;
    // seated on the top driving surface within 12 m above / 8 m below (a course patch can stand above the line's old ground)
    foreach(int i in seat.OrderBy(x=>x)){var top=Physics.RaycastAll(pts[i]+Vector3.up*12,Vector3.down,20,~0,QueryTriggerInteraction.Ignore).Where(x=>x.normal.y>.5f&&!x.collider.attachedRigidbody&&x.collider.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)<0).OrderBy(x=>x.distance).ToArray();
     if(top.Length>0){pts[i].y=top[0].point.y;seated++;}else{unseated++;o.Add($"UNSEATED point {i} {pts[i]}");}}
    // smooth the seated heights a little (a lane line can cross a kerb or ditch edge)
    var gates=(List<object>)json["gates"];
    foreach(Dictionary<string,object> g in gates){if(!g.ContainsKey("seat")||!(bool)g["seat"])continue;var pos=(List<object>)g["pos"];var p=new Vector3(Convert.ToSingle(pos[0]),Convert.ToSingle(pos[1])-1.5f,Convert.ToSingle(pos[2]));
     if(Ground(p,5,8,out var h)){pos[1]=(double)(h.point.y+1.5f);o.Add($"gate {g["name"]} seated at {h.point.y+1.5f:F2} on {P(h.collider.transform)}");}else o.Add($"gate {g["name"]} NOT SEATED");}
    json["pts"]=pts.SelectMany(p=>new object[]{Math.Round(p.x,3),Math.Round(p.y,3),Math.Round(p.z,3)}).ToList();
    File.WriteAllText(path,MiniJson.Serialize(json));
    o.Insert(0,$"{name} in {sceneName}: {pts.Length} points, {seated} seated, {unseated} not found");
    // survey: ground under each point, height step to the next, and solids between consecutive points at 0.6-1.6 m above the line
    float s=0;
    var flights=((List<object>)json["flights"]).Cast<Dictionary<string,object>>().Select(f=>(Convert.ToSingle(f["approachStation"]),Convert.ToSingle(f["endStation"]))).ToArray();
    int gaps=0,solids=0;string lastSolid="";
    for(int i=0;i<pts.Length;i++){
     var a=pts[i];var b=pts[(i+1)%pts.Length];float seg=Vector3.Distance(a,b);
     bool flight=flights.Any(f=>s>f.Item1&&s<f.Item2);
     if(!flight){
      if(!Ground(a,3,3,out var h)){gaps++;o.Add($"s {s:F1} NO GROUND within 3 m at {a}");}
      else if(Mathf.Abs(h.point.y-a.y)>.6f)o.Add($"s {s:F1} ground {h.point.y-a.y:+0.00;-0.00} m from the line at {a} ({P(h.collider.transform)})");
      var side=Vector3.Cross(Vector3.up,(b-a).normalized).normalized;
      foreach(var lat in new[]{0f,-1f,1f})foreach(var lift in new[]{.7f,1.4f}){
       var from=a+Vector3.up*lift+side*lat;var dir=(b+Vector3.up*lift+side*lat)-from;
       foreach(var hit in Physics.SphereCastAll(from,.45f,dir.normalized,dir.magnitude,~0,QueryTriggerInteraction.Ignore)){
        if(hit.normal.y>.55f||hit.collider.attachedRigidbody||hit.distance==0)continue;
        var who=P(hit.collider.transform);if(who==lastSolid)continue;lastSolid=who;solids++;
        o.Add($"s {s:F1} SOLID at {hit.point} (lift {lift}, side {lat:+0;-0;0} m, normal {hit.normal.y:F2}): {who}");}
      }
     }
     s+=seg;}
    o.Insert(1,$"length {s:F1} m, no-ground points {gaps}, solids crossed {solids}");
    File.WriteAllLines(Path.Combine(outDir,"survey-"+name+".txt"),o);
   }
   EditorApplication.Exit(0);
  }catch(Exception e){File.WriteAllText(Path.Combine(outDir,"snap-error.txt"),e.ToString());EditorApplication.Exit(1);}
 }
}
// a small JSON reader / writer (numbers as double, objects as Dictionary, arrays as List) for the course files
public static class MiniJson {
 public static object Parse(string s){int i=0;return Value(s,ref i);}
 static void Ws(string s,ref int i){while(i<s.Length&&char.IsWhiteSpace(s[i]))i++;}
 static object Value(string s,ref int i){Ws(s,ref i);char c=s[i];
  if(c=='{'){var d=new Dictionary<string,object>();i++;Ws(s,ref i);if(s[i]=='}'){i++;return d;}
   while(true){Ws(s,ref i);var k=Str(s,ref i);Ws(s,ref i);i++;d[k]=Value(s,ref i);Ws(s,ref i);if(s[i]==','){i++;continue;}i++;return d;}}
  if(c=='['){var l=new List<object>();i++;Ws(s,ref i);if(s[i]==']'){i++;return l;}
   while(true){l.Add(Value(s,ref i));Ws(s,ref i);if(s[i]==','){i++;continue;}i++;return l;}}
  if(c=='"')return Str(s,ref i);
  if(s.Substring(i).StartsWith("true")){i+=4;return true;}if(s.Substring(i).StartsWith("false")){i+=5;return false;}if(s.Substring(i).StartsWith("null")){i+=4;return null;}
  int j=i;while(j<s.Length&&"+-0123456789.eE".IndexOf(s[j])>=0)j++;var n=double.Parse(s.Substring(i,j-i),System.Globalization.CultureInfo.InvariantCulture);i=j;return n;}
 static string Str(string s,ref int i){i++;var b=new System.Text.StringBuilder();while(s[i]!='"'){if(s[i]=='\\'){i++;char e=s[i];b.Append(e=='n'?'\n':e=='t'?'\t':e=='u'?(char)Convert.ToInt32(s.Substring(i+1,4),16):e);if(e=='u')i+=4;}else b.Append(s[i]);i++;}i++;return b.ToString();}
 public static string Serialize(object o){var b=new System.Text.StringBuilder();W(o,b);return b.ToString();}
 static void W(object o,System.Text.StringBuilder b){
  switch(o){case null:b.Append("null");break;case string s:b.Append('"').Append(s.Replace("\\","\\\\").Replace("\"","\\\"")).Append('"');break;case bool v:b.Append(v?"true":"false");break;
   case Dictionary<string,object> d:b.Append('{');bool first=true;foreach(var kv in d){if(!first)b.Append(',');first=false;W(kv.Key,b);b.Append(':');W(kv.Value,b);}b.Append('}');break;
   case List<object> l:b.Append('[');for(int k=0;k<l.Count;k++){if(k>0)b.Append(',');W(l[k],b);}b.Append(']');break;
   default:b.Append(Convert.ToDouble(o).ToString("R",System.Globalization.CultureInfo.InvariantCulture));break;}}
}
