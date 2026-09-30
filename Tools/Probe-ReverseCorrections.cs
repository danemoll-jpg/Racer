using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using Racer;using Object=UnityEngine.Object;
public static class ProbeReverseCorrections {
 public static string Main(){var r=Object.FindAnyObjectByType<RaceDirector>();r.road.Initialize();var rows=new List<object>();object P(Vector3 p)=>new{x=p.x,y=p.y,z=p.z};
 foreach(var p in new[]{new Vector3(416.6f,84.7f,82.1f),new(172.2f,46.3f,-98.9f),new(235,60.7f,-31.3f)}){float station=r.road.Project(p,out _);var samples=new List<object>();for(float d=-20;d<=20;d+=.5f)for(float side=-4;side<=4;side+=.5f){var q=r.road.At(station+d,out var f);q+=Vector3.Cross(Vector3.up,f).normalized*side;var h=Physics.RaycastAll(new(q.x,250,q.z),Vector3.down,500,~0,QueryTriggerInteraction.Ignore).Where(h=>!h.collider.isTrigger).OrderBy(h=>h.distance).FirstOrDefault();if(h.collider)samples.Add(new{d,side,p=P(h.point),name=h.collider.name,normal=P(h.normal)});}rows.Add(new{reference=P(p),samples});}
 var guide=Object.FindObjectsByType<MeshRenderer>().Where(m=>m.name.IndexOf("arrow",StringComparison.OrdinalIgnoreCase)>=0||m.name.IndexOf("guidance",StringComparison.OrdinalIgnoreCase)>=0).Select(m=>new{m.name,p=P(m.transform.position),material=AssetDatabase.GetAssetPath(m.sharedMaterial)}).ToArray();
 File.WriteAllText("Docs/BackyardReverseCorrections/probes.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{rows,guide},Newtonsoft.Json.Formatting.Indented));return "Cross-corridor support and active navigation captured";}
}

