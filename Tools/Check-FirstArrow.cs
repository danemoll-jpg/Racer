using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
public static class CheckFirstArrow { public static object Main(){
UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/MountainLoopReverse.unity");
Physics.SyncTransforms();var road=UnityEngine.Object.FindAnyObjectByType<Racer.RaceDirector>().road;var rows=new List<string>();
foreach(float lane in new[]{-4f,-2,0,2,4}){Vector3 prior=Vector3.up;float py=0,maxStep=0,maxNormal=0,worstS=0;string worst="";
for(float s=40;s<=100;s+=.1f){var p=road.At(s,out var f)+Vector3.Cross(Vector3.up,f).normalized*lane;var hits=Physics.RaycastAll(p+Vector3.up*4,Vector3.down,8,1,QueryTriggerInteraction.Ignore).Where(h=>!h.rigidbody).OrderBy(h=>h.distance).ToArray();if(hits.Length==0){rows.Add($"MISSING {s} lane={lane}");continue;}var h=hits[0];float change=Vector3.Angle(prior,h.normal);if(s>40&&change>maxNormal){maxNormal=change;worstS=s;worst=string.Join(";",hits.Select(x=>$"{x.collider.name} y={x.point.y:F4} normal={x.normal}"));}if(s>40)maxStep=Mathf.Max(maxStep,Math.Abs(h.point.y-py));prior=h.normal;py=h.point.y;}
rows.Add($"lane={lane} maximum .1m step={maxStep:F4} normal change={maxNormal:F3} at={worstS:F2}: {worst}");}
System.IO.Directory.CreateDirectory("Docs/LocalRecovery");System.IO.File.WriteAllLines("Docs/LocalRecovery/arrow-after.txt",rows);return string.Join("\n",rows);

}}
