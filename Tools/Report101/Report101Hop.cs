using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
public static class Report101Hop { public static void Run(){var log=new List<string>();
 EditorSceneManager.OpenScene("Assets/Scenes/DansBackyardForward.unity");Physics.SyncTransforms();
 var lip=new Vector3(224.61f,74.15f,80.95f);var d=new Vector3(Mathf.Sin(82.82f*Mathf.Deg2Rad),0,Mathf.Cos(82.82f*Mathf.Deg2Rad));var n=new Vector3(-d.z,0,d.x);
 for(float s=10;s<=18;s+=.5f)foreach(float l in new[]{-1f,0f,1f}){var p=lip+d*(s-44.72f)+n*l;var hits=Physics.RaycastAll(new Vector3(p.x,200,p.z),Vector3.down,400,~0,QueryTriggerInteraction.Collide).OrderBy(h=>h.distance).Take(4);log.Add($"s {s:F1} l {l}: "+string.Join(" | ",hits.Select(h=>$"{h.collider.name} y{h.point.y:F2} trig {h.collider.isTrigger} layer {h.collider.gameObject.layer}")));}
 // any collider in a box over the run-up s 10..18
 var c=lip+d*(14-44.72f);foreach(var col in Physics.OverlapBox(c+Vector3.up*69.5f-Vector3.up*74.15f+Vector3.up*0,new Vector3(4,3,5),Quaternion.LookRotation(d),~0,QueryTriggerInteraction.Collide))log.Add("overlap "+col.name+" "+col.GetType().Name+" trig "+col.isTrigger+" "+col.bounds);
 File.WriteAllLines(Environment.GetEnvironmentVariable("PROBE_OUT")+"/hop.txt",log);EditorApplication.Exit(0);}}
