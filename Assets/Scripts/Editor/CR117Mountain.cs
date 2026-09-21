using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Object=UnityEngine.Object;
namespace Racer.Editor
{
    public static partial class DiscoveryAuthoring
    {
        public static void CR117Geometry()
        {
            foreach(var path in CR112Scenes.Where(p=>p.Contains("MountainLoop"))){
                EditorSceneManager.OpenScene(path);owner=Object.FindAnyObjectByType<RaceDirector>();
                if(owner.courseId.Contains("v2"))throw new Exception("CR117 geometry already authored; revise the affected surface locally");
                var old=owner.road;old.Initialize();bool reverse=owner.reverseCourse;
                worldRoot=new GameObject("CR117 main flights and alternatives").transform;
                var branches=Object.FindObjectsByType<WoodlandRoute>();var ridge=branches.Single(b=>b.title.Contains("Ridge Cut"));var summit=branches.Single(b=>b.title.Contains("Summit Flight"));
                Vector3[] Segment(float a,float b){float len=old.Relative(b,a);return Enumerable.Range(0,(int)(len/2)+1).Select(i=>old.At(a+Math.Min(i*2,len),out _)).Append(old.At(b,out _)).ToArray();}
                var summitBypass=Segment(summit.entryRoad,summit.exitRoad);
                float gullyEntry=reverse?summit.exitRoad:ridge.entryRoad;
                Vector3 origin=reverse?new(1225,110,-300):new(850,110,-275);Vector3 direction=reverse?Vector3.left:Vector3.right;
                Vector3 P(float s,float h)=>origin+direction*s+Vector3.up*(h-110);
                float Ramp(float s)=>110+(s<160?0:.006f*(s-160)*(s-160));
                float Catch(float s){float d=s-320;return 124-.30f*d+.0009f*d*d;}
                Vector3[] entry=reverse?Curve(new[]{old.At(gullyEntry,out _),new Vector3(875,111,-100),new(980,131,-100),new(1150,139,-70),new(1250,124,-140),new(1280,110,-255),new(1270,110,-292),P(0,110)}):Curve(new[]{old.At(ridge.entryRoad,out _),new Vector3(755,93,-180),new(780,106,-238),new(810,110,-268),P(0,110)});
                var runway=Enumerable.Range(0,441).Select(i=>P(i*.5f,Ramp(i*.5f))).ToArray();
                var landing=Enumerable.Range(0,361).Select(i=>P(320+i*.5f,Catch(320+i*.5f))).ToArray();
                Vector3[] exit=reverse?Curve(new[]{landing[^1],new Vector3(700,99,-275),new(688,91,-213),new(720,87,-145),old.At(ridge.exitRoad,out _)}):Curve(new[]{landing[^1],new Vector3(1385,103,-210),new(1370,117,-140),new(1280,136,-65),new(1110,141,-30),old.At(ridge.exitRoad,out _)});
                MountainSurface("CR117 gully approach",entry,7);MountainSurface("CR117 gully takeoff",runway,12);MountainSurface("CR117 gully catch",landing,19);MountainSurface("CR117 gully return",exit,7);
                Terrain("CR117 gully air clearance",p=>{var q=p-origin;float s=Vector3.Dot(q,direction),side=Mathf.Abs(Vector3.Dot(q,Vector3.Cross(Vector3.up,direction)));if(s>222&&s<320&&side<30)p.y=Mathf.Min(p.y,100-12*Mathf.Sin(Mathf.PI*(s-222)/98));return p;});
                var gully=entry.Concat(runway.Skip(1)).Concat(landing).Concat(exit.Skip(1)).ToArray();
                var replacements=new[]{(start:gullyEntry,end:ridge.exitRoad,points:gully),(start:summit.entryRoad,end:summit.exitRoad,points:summit.points)}.OrderBy(x=>x.start).ToArray();
                if(reverse){ridge.points=Curve(new[]{old.At(gullyEntry,out _),new Vector3(775,94,-160),old.At(ridge.exitRoad,out _)});MountainSurface("CR117 reverse ridge shortcut",ridge.points,3.6f);}
                var main=new List<Vector3>();float at=0;
                foreach(var r in replacements){main.AddRange(Segment(at,r.start).Skip(main.Count>0?1:0));main.AddRange(r.points.Skip(1));at=r.end;}
                main.AddRange(Segment(at,old.Length-.1f).Skip(1));
                var road=new GameObject("CR117 mandatory two-flight racing line").AddComponent<RaceRoad>();road.transform.SetParent(worldRoot);road.points=main.Where((p,i)=>i==0||(p-main[i-1]).sqrMagnitude>.0001f).ToArray();road.forestTrail=true;road.Initialize();owner.road=road;
                owner.courseId=reverse?"mountain-reverse-v2-main-flights":"mountain-forward-v2-main-flights";
                // The previous ridge remains the narrow early alternative. The former
                // summit main trail becomes a clearly signed faster technical traverse.
                var alternate=new GameObject("Summit Traverse / technical shortcut").AddComponent<WoodlandRoute>();alternate.transform.SetParent(worldRoot);alternate.points=summitBypass;alternate.halfWidth=5;alternate.title="Summit Traverse";alternate.recommendedSpeed=23;alternate.entrySpeed=18;alternate.entrySpeedDistance=45;
                Object.DestroyImmediate(summit.gameObject);
                foreach(var b in new[]{ridge,alternate}){b.entryRoad=road.Project(b.points[0],out _);b.exitRoad=road.Project(b.points[^1],out _);b.entryInset=15;b.entryMargin=3;b.aiValidated=false;b.bypassedGates=Array.Empty<int>();}
                var metadata=owner.gameObject.AddComponent<MountainFlights>();var summitRoot=GameObject.Find(reverse?"CR110 reverse summit launch":"CR094 summit launch").transform;
                var summitEnd=summitRoot.TransformPoint(new Vector3(0,LandingHeight(480)-152,480));
                metadata.flights=new[]{new MountainFlights.Flight{name=reverse?"Westbound Gully Flight":"Eastbound Gully Flight",start=origin,lip=runway[^1],landingEnd=landing[^1],forward=direction},new MountainFlights.Flight{name=reverse?"South Face Summit Flight":"Homeward Summit Flight",start=summitRoot.position,lip=summitRoot.TransformPoint(new Vector3(0,HomewardHeight(220)-152,220)),landingEnd=summitEnd,forward=summitRoot.forward}};
                foreach(var f in metadata.flights){f.approachStation=road.Project(f.start,out _);f.endStation=road.Project(f.landingEnd,out _);}
                var template=owner.gates[0].gameObject;var originals=owner.gates.Select(g=>g.gameObject).ToArray();
                var stations=new List<float>{road.Project(owner.gates[0].transform.position,out _)};
                foreach(var b in new[]{ridge,alternate}){stations.Add(Mathf.Repeat(b.entryRoad-35,road.Length));stations.Add(Mathf.Repeat(b.exitRoad+40,road.Length));}
                var ordered=stations.Distinct().OrderBy(s=>road.Relative(s,stations[0])).ToArray();var gates=new List<RaceGate>();
                foreach(float s in ordered){var go=Object.Instantiate(template,worldRoot);go.name="CR117 "+(gates.Count==0?"START FINISH":"CP "+gates.Count);var g=go.GetComponent<RaceGate>();g.transform.SetPositionAndRotation(road.At(s,out var f)+Vector3.up*1.6f,Quaternion.LookRotation(Vector3.ProjectOnPlane(f,Vector3.up)));g.halfWidth=7;g.halfHeight=5;gates.Add(g);}
                foreach(var go in originals)Object.DestroyImmediate(go);owner.gates=gates.ToArray();
                foreach(var b in new[]{ridge,alternate})b.bypassedGates=Enumerable.Range(1,gates.Count-1).Where(i=>road.Relative(road.Project(gates[i].transform.position,out _),b.entryRoad)<road.Relative(b.exitRoad,b.entryRoad)).ToArray();
                var spawn=owner.vehicle.GetComponent<VehicleRespawn>().spawnPoint;spawn.SetPositionAndRotation(road.At(stations[0]-35,out var heading)+Vector3.up*.7f,Quaternion.LookRotation(Vector3.ProjectOnPlane(heading,Vector3.up)));owner.vehicle.transform.SetPositionAndRotation(spawn.position,spawn.rotation);
                var obsolete=GameObject.Find("CR110 selected direction signs");if(obsolete)obsolete.SetActive(false);
                var mainArray=main.ToArray();ClearCompleteTrees(p=>Near(p,mainArray,out _)<17||Near(p,gully,out _)<24);
                Save();CR117Overview();
            }
        }
        static void ClearCompleteTrees(Func<Vector3,bool> remove)
        {
            foreach(var c in Object.FindObjectsByType<Collider>().Where(c=>c.name.IndexOf("tree",StringComparison.OrdinalIgnoreCase)>=0||c.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0).ToArray())if(remove(c.bounds.center))Object.DestroyImmediate(c.gameObject);
            foreach(var mf in Object.FindObjectsByType<MeshFilter>().Where(m=>m.sharedMesh&&(m.name=="Forest detail batch"||m.transform.GetComponentsInParent<Transform>().Any(t=>t.name.IndexOf("woods",StringComparison.OrdinalIgnoreCase)>=0||t.name.Contains("Forest tree")))).ToArray()){
                var mesh=mf.sharedMesh;var v=mesh.vertices;var tris=mesh.triangles;var parent=Enumerable.Range(0,v.Length).ToArray();
                int Root(int n){while(parent[n]!=n){parent[n]=parent[parent[n]];n=parent[n];}return n;}
                void Union(int a,int b){parent[Root(a)]=Root(b);}
                var welded=new Dictionary<Vector3Int,int>();for(int i=0;i<v.Length;i++){var key=Vector3Int.RoundToInt(v[i]*1000);if(welded.TryGetValue(key,out int other))Union(i,other);else welded[key]=i;}
                for(int i=0;i<tris.Length;i+=3){Union(tris[i],tris[i+1]);Union(tris[i],tris[i+2]);}
                var cut=new HashSet<int>();for(int i=0;i<v.Length;i++)if(remove(mf.transform.TransformPoint(v[i])))cut.Add(Root(i));
                if(cut.Count==0)continue;var kept=new List<int>();for(int i=0;i<tris.Length;i+=3)if(!cut.Contains(Root(tris[i])))kept.AddRange(new[]{tris[i],tris[i+1],tris[i+2]});
                var copy=Object.Instantiate(mesh);copy.SetTriangles(kept,0);copy.RecalculateBounds();mf.sharedMesh=MeshAsset(copy,owner.gameObject.scene.name+"-CR117-whole-crowns-"+mf.GetEntityId().ToString().Replace(':','-'));if(mf.TryGetComponent<MeshCollider>(out var collider))collider.sharedMesh=mf.sharedMesh;
            }
        }
        public static void CR117Overview()
        {
            var race=Object.FindAnyObjectByType<RaceDirector>();var road=race.road;var bounds=new Bounds(road.points[0],Vector3.zero);foreach(var p in road.points)bounds.Encapsulate(p);
            float scale=Math.Min(1050/bounds.size.x,640/bounds.size.z);Vector2 XY(Vector3 p)=>new(70+(p.x-bounds.min.x)*scale,720-(p.z-bounds.min.z)*scale);
            string Path(Vector3[] points)=>string.Join(" ",points.Select(p=>{var q=XY(p);return $"{q.x:F1},{q.y:F1}";}));
            var svg=new StringBuilder("<svg xmlns='http://www.w3.org/2000/svg' width='1280' height='820'><rect width='1280' height='820' fill='#102a2d'/><style>text{fill:white;font:16px sans-serif}</style><defs><marker id='arrow' markerWidth='8' markerHeight='8' refX='7' refY='3' orient='auto'><path d='M0,0 L7,3 L0,6' fill='#63efd2'/></marker></defs>");
            svg.Append($"<text x='30' y='28'>{race.courseName} — actual CR117 geometry / first playable</text><text x='30' y='52'>Teal: main line · Amber: optional routes · Pink: mandatory BIG flights</text><polyline points='{Path(road.points.Append(road.points[0]).ToArray())}' fill='none' stroke='#63efd2' stroke-width='5'/>");
            foreach(var b in Object.FindObjectsByType<WoodlandRoute>()){svg.Append($"<polyline points='{Path(b.points)}' fill='none' stroke='#ffcb67' stroke-width='4'/>");var a=XY(b.points[0]);var z=XY(b.points[^1]);svg.Append($"<text x='{a.x}' y='{a.y-8}'>{b.title} ENTRANCE</text><text x='{z.x}' y='{z.y+18}'>REJOIN</text>");}
            foreach(var f in race.GetComponent<MountainFlights>().flights){var p=XY(f.lip);svg.Append($"<circle cx='{p.x}' cy='{p.y}' r='9' fill='#fa86c4'/><text x='{p.x+12}' y='{p.y}'>{f.name}</text>");}
            var start=XY(race.gates[0].transform.position);svg.Append($"<circle cx='{start.x}' cy='{start.y}' r='10' fill='white'/><text x='{start.x+12}' y='{start.y}'>START / FINISH</text>");
            for(float s=70;s<road.Length;s+=180){var a=XY(road.At(s,out _));var b=XY(road.At(s+22,out _));svg.Append($"<path d='M{a.x},{a.y} L{b.x},{b.y}' stroke='#63efd2' stroke-width='3' marker-end='url(#arrow)'/>");}
            svg.Append("</svg>");Directory.CreateDirectory("Docs/CR112-118");File.WriteAllText("Docs/CR112-118/route-"+race.gameObject.scene.name+".svg",svg.ToString());
        }
    }
}
