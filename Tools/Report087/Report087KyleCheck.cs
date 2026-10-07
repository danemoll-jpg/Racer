using System;using System.IO;using System.Linq;using System.Text;using System.Collections.Generic;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;using Racer;
// 0.87 Part A (read only): rule 4 around Kyle's house in each scene. In the house frame (lower floor = the pad = 0 after the
// 2.45 m lift): the ground just outside every wall, the porch slab, the chimney, the screened porch skirt, the deck posts,
// the stairs' foot, the screen-door steps and the garage doors against the bottom of what stands there (gap = bottom -
// ground: > 0.05 floats, buried only where the part should meet the ground at its foot); the new drive leg and the kept
// old drive against the ground; every tree trunk and bark box within 30 m against the ground under it.
public static class Report087KyleCheck {
 static string V(Vector3 v)=>$"{v.x:F2},{v.y:F2},{v.z:F2}";
 public static void Run(){var o=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(o);var sb=new StringBuilder();
  foreach(var sn in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+sn+".unity");Physics.SyncTransforms();var roots=scene.GetRootGameObjects();
   var site=roots.SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).FirstOrDefault(t=>t.name=="Friend across street - blue circle");if(!site){sb.AppendLine($"{sn}: no house");continue;}
   float G(float x,float z){var w=site.TransformPoint(new Vector3(x,0,z));float best=float.NaN;foreach(var h in Physics.RaycastAll(new Vector3(w.x,w.y+40,w.z),Vector3.down,90,~0,QueryTriggerInteraction.Ignore)){if(!h.collider.name.StartsWith("Ground_")||h.collider.name.Contains("garage drive")||h.collider.name.Contains("descending drive"))continue;if(float.IsNaN(best)||h.point.y>best)best=h.point.y;}return best-site.position.y;}
   float Surf(string name,float x,float z){var w=site.TransformPoint(new Vector3(x,0,z));foreach(var h in Physics.RaycastAll(new Vector3(w.x,w.y+40,w.z),Vector3.down,90,~0,QueryTriggerInteraction.Ignore))if(h.collider.name.Contains(name))return h.point.y-site.position.y;return float.NaN;}
   int checks=0,floats=0,buried=0;var issues=new List<string>();
   void Edge(string what,float x0,float z0,float x1,float z1,float bottom,float nx,float nz,bool mustMeet){float len=Mathf.Sqrt((x1-x0)*(x1-x0)+(z1-z0)*(z1-z0));for(float d=0;d<=len;d+=.5f){float x=x0+(x1-x0)*d/len+nx*.35f,z=z0+(z1-z0)*d/len+nz*.35f;float g=G(x,z);checks++;float gap=bottom-g;
     if(gap>.05f){floats++;issues.Add($"  FLOATS {gap:F2} m: {what} at local ({x:F1}, {z:F1}), bottom {bottom:F2}, ground {g:F2}");}else if(mustMeet&&gap<-.6f){buried++;issues.Add($"  BURIED {-gap:F2} m: {what} at local ({x:F1}, {z:F1})");}}}
   // walls and lower-level foundations: bottom at the pad (0); the ground may stand higher (front terrace, banks)
   Edge("back wall",-7.6f,-4.6f,7.6f,-4.6f,0,0,-1,false);Edge("left end wall",7.6f,-4.6f,7.6f,6.4f,0,1,0,false);Edge("wing front",2.2f,6.4f,7.6f,6.4f,0,0,1,false);Edge("right end wall (behind the screened porch)",-7.6f,-4.6f,-7.6f,-4.2f,0,-1,0,false);
   Edge("screened porch skirt (outer side)",-11.4f,-4.2f,-11.4f,1.45f,0,-1,0,false);Edge("screened porch skirt (back)",-11.4f,-4.2f,-7.6f,-4.2f,0,0,-1,false);Edge("screened porch skirt (front)",-11.4f,1.45f,-8.6f,1.45f,0,0,1,false);
   Edge("chimney base",-8.55f,1.75f,-8.55f,3.35f,0,-1,0,false);Edge("porch slab (front)",-7.6f,6.5f,.4f,6.5f,1.85f,0,1,false);Edge("porch slab (right end)",-7.6f,4.6f,-7.6f,6.5f,1.85f,-1,0,false);Edge("front wall by the wing",.4f,4.6f,2.2f,4.6f,0,0,1,false);
   Edge("garage doors (apron)",7.6f,-4.25f,7.6f,1.05f,0,1,0,true);
   // deck posts, stairs' foot, screen-door steps
   foreach(var (x,z) in new[]{(-4.8f,-8.3f),(-1.5f,-8.3f),(1.8f,-8.3f),(-4.8f,-4.8f),(1.8f,-4.8f)}){float g=G(x,z);checks++;float gap=-.2f-g;if(gap>.0f){floats++;issues.Add($"  FLOATS {gap:F2} m: deck post at local ({x}, {z}), ground {g:F2}");}else if(g>.6f){buried++;issues.Add($"  BURIED {g:F2} m: deck post foot at ({x}, {z})");}}
   {float g=G(-9.1f,-7.85f);checks++;if(Mathf.Abs(g)>.25f){(g<0?ref floats:ref buried)++;issues.Add($"  STAIRS' FOOT: ground {g:F2} at local (-9.1, -7.85), the last tread at 0");}}
   {float g=G(-9.75f,2.4f);checks++;float stepBottom=2.9f-.45f-.12f;if(stepBottom-g>.05f||g-stepBottom>.35f){issues.Add($"  SCREEN-DOOR STEPS: lowest step bottom {stepBottom:F2}, ground {g:F2} at (-9.75, 2.4)");if(stepBottom>g)floats++;else buried++;}}
   // the drives
   float worstLeg=0;Vector3 worstAt=default;for(float x=8f;x<=22f;x+=.5f)for(float z=-5f;z<=8f;z+=.5f){float s=Surf("garage drive",x,z);if(float.IsNaN(s))continue;float g=G(x,z);float gap=s-g;checks++;if(Mathf.Abs(gap)>Mathf.Abs(worstLeg)){worstLeg=gap;worstAt=new Vector3(x,0,z);}if(gap>.12f){floats++;issues.Add($"  FLOATS {gap:F2} m: new drive leg / apron at local ({x}, {z})");}else if(gap<-.05f){buried++;issues.Add($"  UNDER GROUND {-gap:F2} m: new drive leg / apron at local ({x}, {z})");}}
   float worstOld=0;for(float x=22.5f;x<=34f;x+=.5f)for(float z=2f;z<=14f;z+=.5f){float s=Surf("descending drive",x,z);if(float.IsNaN(s))continue;float g=G(x,z);float gap=s-g;checks++;if(Mathf.Abs(gap)>Mathf.Abs(worstOld))worstOld=gap;if(gap>.15f){floats++;issues.Add($"  FLOATS {gap:F2} m: kept old drive at local ({x}, {z})");}}
   // trees
   int trees=0;foreach(var c in roots.SelectMany(g=>g.GetComponentsInChildren<BoxCollider>(true))){if(c.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)<0||!c.enabled)continue;var b=c.bounds;var l=site.InverseTransformPoint(new Vector3(b.center.x,b.min.y,b.center.z));if(Mathf.Abs(l.x-4)>30||Mathf.Abs(l.z)>30)continue;trees++;float g=G(l.x,l.z);float gap=l.y-g;checks++;
    if(gap>.15f){floats++;issues.Add($"  FLOATS {gap:F2} m: trunk {c.name} at local {V(l)}");}else if(gap<-.6f){buried++;issues.Add($"  BURIED {-gap:F2} m: trunk {c.name} at local {V(l)}");}}
   sb.AppendLine($"{sn}: {checks} points checked ({trees} trunk colliders within 30 m): floating {floats}, buried {buried}; new drive leg vs ground worst {worstLeg:+0.00;-0.00} m at local {V(worstAt)}, kept old drive worst {worstOld:+0.00;-0.00} m");foreach(var i in issues.Where(i=>!i.Contains("drive")).Take(30).Concat(issues.Where(i=>i.Contains("drive")).Take(10)))sb.AppendLine(i);}
  File.WriteAllText(o+"/A-grounding.txt",sb.ToString());EditorApplication.Exit(0);}
}
