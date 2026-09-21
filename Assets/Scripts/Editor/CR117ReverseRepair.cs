using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.SceneManagement;
using Object=UnityEngine.Object;
namespace Racer.Editor
{
    public static partial class DiscoveryAuthoring
    {
        // Local correction for the first authoring pass: the legacy reverse summit
        // and ridge branch intervals overlap. Splice at the summit rejoin instead.
        public static void CR117ReverseSequence()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/MountainLoopReverse.unity");owner=Object.FindAnyObjectByType<RaceDirector>();worldRoot=GameObject.Find("CR117 main flights and alternatives").transform;
            var old=GameObject.Find("Mountain racing line").GetComponent<RaceRoad>();old.Initialize();var current=owner.road;
            var rejoin=old.At(old.Project(new(820,98,-125),out _),out _);var end=old.At(old.Project(new(747,86,-120),out _),out _);
            int prefix=Array.FindIndex(current.points,p=>Vector3.Distance(p,rejoin)<.3f);if(prefix<0)throw new Exception("Reverse summit rejoin not found");
            var entry=Curve(new[]{rejoin,new Vector3(875,111,-100),new(980,131,-100),new(1150,139,-70),new(1250,124,-140),new(1280,110,-255),new(1270,110,-292),new(1225,110,-300)});
            var previous=GameObject.Find("Ground_CR117 gully approach");if(previous)Object.DestroyImmediate(previous);MountainSurface("CR117 gully approach corrected",entry,7);
            Vector3[] Centers(string name){var mf=GameObject.Find(name).GetComponent<MeshFilter>();var v=mf.sharedMesh.vertices;return Enumerable.Range(0,v.Length/2).Select(i=>mf.transform.TransformPoint((v[i*2]+v[i*2+1])*.5f)-Vector3.up*.04f).ToArray();}
            var points=current.points.Take(prefix+1).Concat(entry.Skip(1)).Concat(Centers("Ground_CR117 gully takeoff").Skip(1)).Concat(Centers("Ground_CR117 gully catch")).Concat(Centers("Ground_CR117 gully return").Skip(1)).ToList();
            float finish=old.Project(end,out _);for(float s=finish+2;s<old.Length;s+=2)points.Add(old.At(s,out _));
            var go=current.gameObject;Object.DestroyImmediate(current);var road=go.AddComponent<RaceRoad>();road.forestTrail=true;road.points=points.Where((p,i)=>i==0||(p-points[i-1]).sqrMagnitude>.0001f).ToArray();road.Initialize();owner.road=road;
            var ridge=Object.FindObjectsByType<WoodlandRoute>().Single(b=>b.title.Contains("Ridge Cut"));ridge.points=Curve(new[]{rejoin,new Vector3(775,94,-160),end});MountainSurface("CR117 reverse ridge shortcut",ridge.points,3.6f);
            var branches=Object.FindObjectsByType<WoodlandRoute>();foreach(var b in branches){b.entryRoad=road.Project(b.points[0],out _);b.exitRoad=road.Project(b.points[^1],out _);}
            foreach(var f in owner.GetComponent<MountainFlights>().flights){f.approachStation=road.Project(f.start,out _);f.endStation=road.Project(f.landingEnd,out _);}
            var stations=new List<float>{road.Project(owner.gates[0].transform.position,out _)};foreach(var b in branches){stations.Add(Mathf.Repeat(b.entryRoad-35,road.Length));stations.Add(Mathf.Repeat(b.exitRoad+40,road.Length));}
            var sorted=stations.OrderBy(s=>road.Relative(s,stations[0])).ToArray();for(int i=0;i<owner.gates.Length;i++){var p=road.At(sorted[i],out var forward);owner.gates[i].transform.SetPositionAndRotation(p+Vector3.up*1.6f,Quaternion.LookRotation(Vector3.ProjectOnPlane(forward,Vector3.up)));}
            foreach(var b in branches)b.bypassedGates=Enumerable.Range(1,owner.gates.Length-1).Where(i=>road.Relative(road.Project(owner.gates[i].transform.position,out _),b.entryRoad)<road.Relative(b.exitRoad,b.entryRoad)).ToArray();
            ClearCompleteTrees(p=>Near(p,entry,out _)<17||Near(p,ridge.points,out _)<8);Save();CR117Overview();File.WriteAllText("Docs/CR112-118/reverse-sequence-correction.txt","Removed legacy overlapping branch-interval wrap before first playable tests; ridge now enters at summit rejoin.");
        }
    }
}
