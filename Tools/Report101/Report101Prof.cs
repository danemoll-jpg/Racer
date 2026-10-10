using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
public static class Report101Prof { public static void Run(){var log=new List<string>();
 EditorSceneManager.OpenScene("Assets/Scenes/"+(Environment.GetEnvironmentVariable("PROBE_SCENES")??"DansBackyardForward")+".unity");log.Add(GameObject.Find("Memory loop - north is +Z/Ground_480_400").GetComponent<MeshFilter>().sharedMesh.name);Physics.SyncTransforms();
 var lip=new Vector3(224.61f,74.15f,80.95f);var d=new Vector3(Mathf.Sin(82.82f*Mathf.Deg2Rad),0,Mathf.Cos(82.82f*Mathf.Deg2Rad));var n=new Vector3(-d.z,0,d.x);
 foreach(float l in new[]{-1.5f,0f,1.5f}){log.Add($"lane {l}");float py=float.NaN,ps=float.NaN;
  for(float s=-4;s<=12;s+=.25f){var p=lip+d*(s-44.72f)+n*l;var hits=Physics.RaycastAll(new Vector3(p.x,200,p.z),Vector3.down,400,~0,QueryTriggerInteraction.Ignore).Where(h=>!h.collider.attachedRigidbody).OrderBy(h=>h.distance).ToList();var h0=hits.FirstOrDefault();
   float slope=float.IsNaN(py)?0:(h0.point.y-py)/.25f;log.Add($"  s {s,5:F2} y {h0.point.y:F3} slope {slope:F3} dslope {(float.IsNaN(ps)?0:slope-ps):+0.000;-0.000} [{h0.collider?.name}]");py=h0.point.y;ps=slope;}}
 File.WriteAllLines(Environment.GetEnvironmentVariable("PROBE_OUT")+"/prof.txt",log);EditorApplication.Exit(0);}}
