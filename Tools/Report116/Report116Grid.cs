using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 0.116 (read-only, temporary; scene never saved): a height grid of MountainLoopReverse.unity for planning Leviathan Reverse's shortcut lines.
// PROBE_GRID "name:x0,z0,x1,z1,step;..." -> grid-<name>.txt, one line per cell: x z y normalY blocked(0/1) surface solid
// y: the highest driving surface (normal > .5; tree trunks and loose props not counted); blocked: a solid 0.9-2.2 m above it within 1.2 m
public static class Report116Grid {
 public static void Run(){
  var o=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(o);
  try{
   EditorSceneManager.OpenScene("Assets/Scenes/"+(Environment.GetEnvironmentVariable("PROBE_SCENE")??"MountainLoopReverse")+".unity");Physics.SyncTransforms();
   var inv=System.Globalization.CultureInfo.InvariantCulture;
   foreach(var spec in Environment.GetEnvironmentVariable("PROBE_GRID").Split(';',StringSplitOptions.RemoveEmptyEntries)){
    var parts=spec.Split(':');var f=parts[1].Split(',').Select(x=>float.Parse(x,inv)).ToArray();var lines=new List<string>();
    for(float z=f[1];z<=f[3];z+=f[4])for(float x=f[0];x<=f[2];x+=f[4]){
     var hits=Physics.RaycastAll(new Vector3(x,400,z),Vector3.down,600,~0,QueryTriggerInteraction.Ignore).Where(h=>!h.collider.attachedRigidbody&&h.normal.y>.5f&&h.collider.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)<0&&!h.collider.isTrigger).OrderBy(h=>h.distance).ToArray();
     if(hits.Length==0){lines.Add(FormattableString.Invariant($"{x:F1} {z:F1} nan 0 1 none -"));continue;}
     var h0=hits[0];var solid=Physics.OverlapBox(new Vector3(x,h0.point.y+1.55f,z),new Vector3(1.2f,.65f,1.2f),Quaternion.identity,~0,QueryTriggerInteraction.Ignore).Where(c=>!c.attachedRigidbody&&!c.isTrigger&&!c.name.StartsWith("Ground_")).Select(c=>c.name).FirstOrDefault();
     lines.Add(FormattableString.Invariant($"{x:F1} {z:F1} {h0.point.y:F2} {h0.normal.y:F2} {(solid!=null?1:0)} {h0.collider.name.Replace(' ','_')} {(solid??"-").Replace(' ','_')}"));}
    File.WriteAllLines(Path.Combine(o,"grid-"+parts[0]+".txt"),lines);}
  }catch(Exception e){File.WriteAllText(Path.Combine(o,"grid-error.txt"),e.ToString());}
  EditorApplication.Exit(0);}
}
