using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.SceneManagement;
using Racer;
using Object=UnityEngine.Object;
public static class CheckLaurelProfileSpeeds
{
 public static string Main(){EditorSceneManager.OpenScene("Assets/Scenes/StreetLoopReverse.unity");Physics.SyncTransforms();var z=Object.FindObjectsByType<JumpRecoveryExclusion>().Single(x=>x.name.StartsWith("Laurel straight"));var axis=Vector3.ProjectOnPlane(z.end-z.start,Vector3.up).normalized;var lip=z.end-axis*2;var rows=new List<string>();
 foreach(var profile in VehicleProfile.All){float speed=profile.Speed;bool landed=false;Vector3 contact=default;for(float t=.1f;t<2.5f;t+=.005f){var p=lip+axis*(speed*t)+Vector3.up*(speed*.052f*t+.5f*Physics.gravity.y*t*t);var hits=Physics.RaycastAll(p+Vector3.up*20,Vector3.down,60,1,QueryTriggerInteraction.Ignore).Where(h=>!h.rigidbody&&h.collider.name=="Ground_Laurel continuous driving surface").OrderBy(h=>h.distance).ToArray();if(hits.Length>0&&p.y<=hits[0].point.y+.02f){landed=true;contact=hits[0].point;break;}}rows.Add($"{profile.Id}: speed={speed}; landing={landed}; contact={contact}");}
 File.WriteAllLines("Docs/LocalRecovery/profile-speed-envelope.txt",rows);return string.Join("\n",rows);}
}
