using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.SceneManagement;
namespace Racer.Editor
{
    public static class CR101Audit
    {
        public static void Run(){foreach(var scene in ReverseReviewRelease.Scenes){EditorSceneManager.OpenScene(scene);var root=GameObject.Find("CR094 summit launch").transform;var rows=new List<string>{"station,side,ground"};for(int s=-5;s<=520;s+=2)foreach(float side in new[]{-5f,0,5f}){var p=root.TransformPoint(new Vector3(side,0,s));float h=Physics.RaycastAll(new(p.x,400,p.z),Vector3.down,800).Where(h=>h.collider.name.StartsWith("Ground_")).Select(h=>h.point.y).DefaultIfEmpty(-999).Max();rows.Add($"{s},{side},{h:F3}");}File.WriteAllLines("Docs/CR101-104/geometry-copy-"+root.gameObject.scene.name+".csv",rows);var map=UnityEngine.Object.FindAnyObjectByType<ExplorationMap>();var d=map.destinations.Single(d=>d.id=="summit-homeward");var site=root.GetComponentInChildren<ActivitySite>();File.WriteAllText("Docs/CR101-104/local-dependencies-"+root.gameObject.scene.name+".txt",$"Safe summit destination {d.position:F3} yaw {d.yaw}; launch activity {site.transform.position:F3} forward {site.forward:F6}; path root {root.position:F3}; property kennel {GameObject.Find("Separate downhill garage kennel").transform.position:F3}\n");}}
    }
}

