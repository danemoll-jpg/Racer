using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor.SceneManagement;
using Object=UnityEngine.Object;
namespace Racer.Editor
{
    public static partial class DiscoveryAuthoring
    {
        public static void CR117ClimbSubgrade()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/MountainLoop.unity");owner=Object.FindAnyObjectByType<RaceDirector>();owner.road.Initialize();
            var summit=owner.GetComponent<MountainFlights>().flights.Single(f=>f.name.Contains("Summit"));
            float begin=owner.road.Project(new Vector3(1020,139,-60),out _)-10,end=owner.road.Project(summit.lip,out _)+5;
            var path=owner.road.points.Where(p=>{float s=owner.road.Project(p,out _);return s>=begin&&s<=end;}).ToArray();
            float minX=path.Min(p=>p.x)-15,maxX=path.Max(p=>p.x)+15,minZ=path.Min(p=>p.z)-15,maxZ=path.Max(p=>p.z)+15;
            Terrain("CR117 climb subgrade",p=>{if(p.x<minX||p.x>maxX||p.z<minZ||p.z>maxZ)return p;float d=Near(p,path,out var at);if(d<12){float edge=1-Smooth(7,12,d);float ends=Smooth(0,16,Math.Min(Vector3.Distance(at,path[0]),Vector3.Distance(at,path[^1])));p.y=Math.Min(p.y,at.y-1.5f*edge*ends);}return p;});
            Save();File.WriteAllText("Docs/CR112-118/climb-subgrade-done.txt","Lowered underlying coarse terrain below the existing smooth supported climb ribbon; route, road mesh and vehicle forces retained.");
        }
        public static void CR117SummitReturn()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/MountainLoop.unity");owner=Object.FindAnyObjectByType<RaceDirector>();var road=owner.road;road.Initialize();
            if(GameObject.Find("CR117 downhill summit return"))throw new Exception("Summit return already authored");
            worldRoot=new GameObject("CR117 downhill summit return").transform;var launch=GameObject.Find("CR094 summit launch").transform;
            Vector3 P(float s)=>launch.TransformPoint(new Vector3(0,LandingHeight(s)-152,s));
            float a=road.Project(P(475),out _),b=road.Project(new Vector3(724,79,-45),out _);var endpoint=road.At(b,out _);
            var curve=Curve(new[]{road.At(a,out _),P(500),new Vector3(685,87,35),new(690,83,-5),new(730,80,-12),new(742,79,-30),endpoint});
            road.points=road.points.Where(p=>road.Project(p,out _)<=a).Concat(curve.Skip(1).SkipLast(1)).Concat(road.points.Where(p=>road.Project(p,out _)>=b)).ToArray();road.Initialize();
            var old=launch.Find("Ground_CR103 supported return");if(old)old.gameObject.SetActive(false);
            MountainSurface("CR117 downhill summit return",curve,10);
            var alternate=Object.FindObjectsByType<WoodlandRoute>().Single(r=>r.title=="Summit Traverse");
            var join=curve.OrderBy(p=>Vector3.Distance(p,new Vector3(685,86,20))).First();var extension=Curve(new[]{alternate.points[^1],new Vector3(770,102,50),new(742,91,40),new(707,88,25),join});
            alternate.points=alternate.points.Concat(extension.Skip(1)).ToArray();alternate.Initialize();MountainSurface("CR117 Summit Traverse rejoin",extension,6);
            foreach(var branch in Object.FindObjectsByType<WoodlandRoute>()){branch.entryRoad=road.Project(branch.points[0],out _);branch.exitRoad=road.Project(branch.points[^1],out _);}
            foreach(var flight in owner.GetComponent<MountainFlights>().flights){flight.approachStation=road.Project(flight.start,out _);flight.endStation=road.Project(flight.landingEnd,out _);}
            ClearCompleteTrees(p=>Near(p,curve,out _)<23||Near(p,extension,out _)<12);
            var guidance=GameObject.Find("CR117 selected-direction guidance");foreach(var gate in owner.gates)gate.transform.SetParent(worldRoot,true);if(guidance)Object.DestroyImmediate(guidance);Save();
            // Rebuild only this direction's gate/arrow source through the common pass.
            CR117Polish(true);File.WriteAllText("Docs/CR112-118/summit-return-done.txt","Forward summit return rebuilt as supported downhill curve; optional traverse rejoined before its downhill bend.");
        }
    }
}
