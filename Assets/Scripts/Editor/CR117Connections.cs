using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Object=UnityEngine.Object;
namespace Racer.Editor
{
    public static partial class DiscoveryAuthoring
    {
        public static void CR117Connections()
        {
            foreach(var scene in CR112Scenes.Where(s=>s.Contains("MountainLoop"))){
                EditorSceneManager.OpenScene(scene);owner=Object.FindAnyObjectByType<RaceDirector>();var road=owner.road;road.Initialize();
                if(GameObject.Find("CR117 smooth route connections"))continue;worldRoot=new GameObject("CR117 smooth route connections").transform;
                if(!owner.reverseCourse){
                    var old=GameObject.Find("Ground_CR117 gully return");var mesh=old.GetComponent<MeshFilter>().sharedMesh;var v=mesh.vertices;var first=(v[0]+v[1])*.5f-Vector3.up*.04f;var last=(v[^2]+v[^1])*.5f-Vector3.up*.04f;
                    float a=road.Project(first,out _),b=road.Project(last,out _);var replacement=Curve(new[]{first,new Vector3(1385,103,-210),new(1370,117,-140),new(1280,136,-65),new(1140,146,-58),new(1060,142,-118),new(990,132,-125),new(991,133,-98),last});
                    var before=road.points.Where(p=>road.Project(p,out _)<=a+.05f).ToArray();var after=road.points.Where(p=>road.Project(p,out _)>=b-.05f).ToArray();road.points=before.Concat(replacement.Skip(1).SkipLast(1)).Concat(after).ToArray();old.SetActive(false);MountainSurface("CR117 tangent gully return",replacement,10);
                    // Deliberate rejoin approaches the uphill road in its direction of
                    // travel, instead of demanding a near-180-degree turn at the edge.
                }
                // A rounded closure corner replaces the sharp first/last tangent change.
                road.Initialize();float begin=road.Length-65,end=45;var pa=road.At(begin,out var fa);var pb=road.At(end,out var fb);float span=Vector3.Distance(pa,pb);var bend=new List<Vector3>();
                for(int i=0;i<=60;i++){float t=i/60f,t2=t*t,t3=t2*t;bend.Add((2*t3-3*t2+1)*pa+(t3-2*t2+t)*fa*span+(-2*t3+3*t2)*pb+(t3-t2)*fb*span);}
                var middle=Enumerable.Range(0,(int)((begin-end)/2)+1).Select(i=>road.At(end+i*2,out _)).Append(pa).ToArray();road.points=middle.Concat(bend.Skip(1).SkipLast(1)).ToArray();road.Initialize();MountainSurface("CR117 rounded start turn",bend.ToArray(),9);
                foreach(var gate in owner.gates){float s=road.Project(gate.transform.position-Vector3.up*1.6f,out _);gate.transform.SetPositionAndRotation(road.At(s,out var f)+Vector3.up*1.6f,Quaternion.LookRotation(Vector3.ProjectOnPlane(f,Vector3.up)));}
                foreach(var branch in Object.FindObjectsByType<WoodlandRoute>()){branch.entryRoad=road.Project(branch.points[0],out _);branch.exitRoad=road.Project(branch.points[^1],out _);branch.bypassedGates=Enumerable.Range(1,owner.gates.Length-1).Where(i=>road.Relative(road.Project(owner.gates[i].transform.position,out _),branch.entryRoad)<road.Relative(branch.exitRoad,branch.entryRoad)).ToArray();}
                foreach(var f in owner.GetComponent<MountainFlights>().flights){f.approachStation=road.Project(f.start,out _);f.endStation=road.Project(f.landingEnd,out _);}
                var points=road.points.Where((p,i)=>i%8==0).Append(road.points[^1]).ToArray();ClearCompleteTrees(p=>Near(p,points,out _)<20);var guidance=GameObject.Find("CR117 selected-direction guidance");foreach(var gate in owner.gates)gate.transform.SetParent(worldRoot,true);if(guidance)Object.DestroyImmediate(guidance);Save();CR117Overview();
            }
            CR117Polish();File.WriteAllText("Docs/CR112-118/connections-done.txt","Tangent gully rejoin and rounded closure corner authored");
        }
        public static void CR117RestoreGuidanceTemplate()
        {
            var target=EditorSceneManager.OpenScene("Assets/Scenes/MountainLoop.unity");var race=Object.FindAnyObjectByType<RaceDirector>();if(race.gates.Any(g=>g))throw new Exception("Existing gates must be retained");
            var reference=EditorSceneManager.OpenScene("Assets/Scenes/MountainLoopReverse.unity",OpenSceneMode.Additive);var other=reference.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<RaceDirector>()).Single();
            var gate=Object.Instantiate(other.gates[0]);UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(gate.gameObject,target);gate.transform.SetParent(target.GetRootGameObjects().Single(g=>g.name=="CR117 smooth route connections").transform,true);var spawn=race.vehicle.GetComponent<VehicleRespawn>().spawnPoint;float s=race.road.Project(spawn.position,out _)+35;gate.transform.SetPositionAndRotation(race.road.At(s,out var f)+Vector3.up*1.6f,Quaternion.LookRotation(Vector3.ProjectOnPlane(f,Vector3.up)));race.gates=new[]{gate};EditorSceneManager.CloseScene(reference,true);UnityEngine.SceneManagement.SceneManager.SetActiveScene(target);Save();
        }
    }
}
