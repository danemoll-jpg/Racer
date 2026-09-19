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
    public static partial class ReverseReviewRelease
    {
        sealed class PathIndex
        {
            public Vector3[] points;public float[] distances;public float Length=>distances[^1];
            readonly Dictionary<Vector2Int,List<int>> bins=new();
            public PathIndex(Vector3[] path,bool closed=false)
            {
                points=closed?path.Append(path[0]).ToArray():path;distances=new float[points.Length];
                for(int i=1;i<points.Length;i++)
                {
                    distances[i]=distances[i-1]+Vector3.Distance(points[i-1],points[i]);
                    var a=points[i-1];var b=points[i];
                    for(int x=Mathf.FloorToInt(Mathf.Min(a.x,b.x)/24)-4;x<=Mathf.FloorToInt(Mathf.Max(a.x,b.x)/24)+4;x++)
                    for(int z=Mathf.FloorToInt(Mathf.Min(a.z,b.z)/24)-4;z<=Mathf.FloorToInt(Mathf.Max(a.z,b.z)/24)+4;z++)
                    {var key=new Vector2Int(x,z);if(!bins.TryGetValue(key,out var list))bins[key]=list=new();list.Add(i);}
                }
            }
            public float Near(Vector3 p,out Vector3 at,out float station)
            {
                at=p;station=0;float best=float.MaxValue;
                if(!bins.TryGetValue(new(Mathf.FloorToInt(p.x/24),Mathf.FloorToInt(p.z/24)),out var list))return best;
                foreach(int i in list)
                {var a=points[i-1];var v=points[i]-a;var q=p-a;v.y=q.y=0;float t=Mathf.Clamp01(Vector3.Dot(v,q)/Mathf.Max(.001f,v.sqrMagnitude));float d=(q-v*t).sqrMagnitude;if(d>=best)continue;best=d;at=Vector3.Lerp(a,points[i],t);station=Mathf.Lerp(distances[i-1],distances[i],t);}
                return Mathf.Sqrt(best);
            }
        }
        static float Ease(float a,float b,float v)=>Mathf.SmoothStep(0,1,Mathf.InverseLerp(a,b,v));
        static Vector3[] Curve(IEnumerable<Vector3> controls)
        {
            var knots=controls.ToArray();var result=new List<Vector3>();
            for(int i=0;i<knots.Length-1;i++)
            {var a=knots[Math.Max(0,i-1)];var b=knots[i];var c=knots[i+1];var d=knots[Math.Min(knots.Length-1,i+2)];int steps=Mathf.CeilToInt(Vector3.Distance(b,c)/1.5f);for(int j=0;j<steps;j++){float t=j/(float)steps;result.Add(.5f*(2*b+(-a+c)*t+(2*a-5*b+4*c-d)*t*t+(-a+3*b-3*c+d)*t*t*t));}}
            result.Add(knots[^1]);return result.ToArray();
        }
        static Mesh StoreMesh(Mesh mesh,string name)
        {
            string path=Folder+"/"+name+".asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(old){EditorUtility.CopySerialized(mesh,old);Object.DestroyImmediate(mesh);return old;}
            AssetDatabase.CreateAsset(mesh,path);return mesh;
        }
        static Material Material(string name,Color color)
        {string path=Folder+"/"+name+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);if(!mat){mat=new Material(Shader.Find("Universal Render Pipeline/Lit")){color=color,enableInstancing=true};AssetDatabase.CreateAsset(mat,path);}return mat;}
        static float Jump(float x,float run,float height,float gap,float landing)
        {
            if(x<0)return 0;if(x<run)return height*Mathf.Pow(x/run,2);
            if(x<run+5)return Mathf.Lerp(height,-2.8f,Ease(run,run+5,x));
            return -2.8f*(1-Ease(run+gap,run+gap+landing,x));
        }
        public static void AuthorReverse()
        {
            Guard();AssetDatabase.Refresh();var report=new List<string>();
            for(int course=0;course<2;course++)
            {
                var scene=EditorSceneManager.OpenScene(Scenes[course]);bool forest=course==1;
                EditorSceneManager.SaveScene(scene,Scenes[course+2],true);scene=EditorSceneManager.OpenScene(Scenes[course+2]);
                var race=Object.FindAnyObjectByType<RaceDirector>();var original=race.road;original.Initialize();
                var streetProfile=forest&&race.ambientRoad?new PathIndex(race.ambientRoad.points,true):null;
                var source=(Vector3[])original.points.Clone();var sourceIndex=new PathIndex(source,true);float length=original.Length;
                var root=new GameObject("Reverse course authoring").transform;
                var road=new GameObject("Reverse main route").AddComponent<RaceRoad>();road.transform.SetParent(root);road.forestTrail=forest;
                road.points=new Vector3[source.Length];road.geometryStations=new float[source.Length];road.geometryLength=length;
                var windows=forest?new[]{(325f,480f),(595f,765f),(885f,1150f),(1295f,1460f),(1525f,1686f)}:Array.Empty<(float,float)>();
                for(int i=0;i<source.Length;i++)
                {
                    int index=(source.Length-i)%source.Length;float s=sourceIndex.distances[index];var p=source[index];road.geometryStations[i]=s;
                    foreach(var w in windows)if(s>=w.Item1&&s<=w.Item2)
                    {
                        float span=w.Item2-w.Item1,x=w.Item2-s,t=x/span;var a=original.At(w.Item2,out var fa);var b=original.At(w.Item1,out var fb);
                        // Hermite endpoint slopes preserve support at both transitions.
                        float ma=-fa.y*span,mb=-fb.y*span;
                        p.y=(2*t*t*t-3*t*t+1)*a.y+(t*t*t-2*t*t+t)*ma+(-2*t*t*t+3*t*t)*b.y+(t*t*t-t*t)*mb;
                        float run=Mathf.Clamp(span-95,30,48);p.y+=Jump(x-12,run,3.1f,12,span-run-24);
                    }
                    road.points[i]=p;
                }
                road.Initialize();race.road=road;race.reverseCourse=true;race.courseName=forest?"Forest Loop Reverse":"Street Loop Reverse";race.courseId=forest?"forest-reverse-v1":"street-reverse-v1";
                // Civilian traffic retains the original street geometry and direction.
                if(!forest)race.ambientRoad=original;
                foreach(var branch in Object.FindObjectsByType<WoodlandRoute>())Object.DestroyImmediate(branch);
                var layout=Object.FindAnyObjectByType<ForestLayout>();
                if(layout)
                {
                    layout.approachLead=110;
                    layout.jumpStarts=windows.Reverse().Select(w=>road.Project(original.At(w.Item2,out _),out _)).ToArray();
                    layout.jumpEnds=windows.Reverse().Select(w=>road.Project(original.At(w.Item1,out _),out _)).ToArray();
                    layout.jumpNames=new[]{"Reverse homeward gully","Reverse ridge crossing","Reverse linked descent","Reverse deep gully","Reverse creek crossing"};
                    EditorUtility.SetDirty(layout);
                }
                if(!forest){ReverseRoadworks(root,original,report);ReversePavementApron(root,road);}
                var branches=new List<WoodlandRoute>();
                WoodlandRoute Add(string title,float from,float to,float speed,float width,params Vector3[] inner)
                {
                    var a=road.At(road.Project(original.At(from,out _),out _),out var reverseA);var b=road.At(road.Project(original.At(to,out _),out _),out var reverseB);var af=-reverseA;var bf=-reverseB;
                    var branch=new GameObject(title).AddComponent<WoodlandRoute>();branch.transform.SetParent(root);branch.title=title;branch.halfWidth=width;branch.recommendedSpeed=speed;
                    branch.points=Curve(new[]{a,a-af*24}.Concat(inner).Concat(new[]{b+bf*28,b}));
                    if(title=="Granite Creek Cut")
                    {
                        int entryEnd=Enumerable.Range(0,branch.points.Length).OrderBy(i=>(branch.points[i]-inner[0]).sqrMagnitude).First();
                        var entry=branch.points[entryEnd];var entryHeading=(branch.points[entryEnd+1]-entry).normalized;
                        var entryC1=a-af*20;var entryC2=entry-entryHeading*20;var entryCurve=new List<Vector3>();
                        for(int i=0;i<=60;i++){float t=i/60f,u=1-t;entryCurve.Add(u*u*u*a+3*u*u*t*entryC1+3*u*t*t*entryC2+t*t*t*entry);}
                        branch.points=entryCurve.Concat(branch.points.Skip(entryEnd+1)).ToArray();
                        // A tangent-matched rejoin avoids the old Catmull overshoot past
                        // the far pavement edge without changing the mouth or gate set.
                        int start=Enumerable.Range(0,branch.points.Length).OrderBy(i=>(branch.points[i]-inner[^1]).sqrMagnitude).First();
                        var q=branch.points[start];var h=(q-branch.points[start-1]).normalized;
                        var c1=q+h*35;var c2=b+bf*35;var smooth=branch.points.Take(start+1).ToList();
                        for(int i=1;i<=80;i++){float t=i/80f,u=1-t;smooth.Add(u*u*u*q+3*u*u*t*c1+3*u*t*t*c2+t*t*t*b);}branch.points=smooth.ToArray();
                    }
                    {var mainProfile=new PathIndex(road.points,true);for(int k=0;k<branch.points.Length;k++){float distance=mainProfile.Near(branch.points[k],out var shared,out _);branch.points[k].y=Mathf.Lerp(shared.y,branch.points[k].y,Ease(5,forest?65:40,distance));if(streetProfile!=null){float sd=streetProfile.Near(branch.points[k],out var streetAt,out _);branch.points[k].y=Mathf.Lerp(streetAt.y,branch.points[k].y,Ease(9,45,sd));}}}
                    branch.entryRoad=road.Project(a,out _);branch.exitRoad=road.Project(b,out _);branch.entryInset=24;branch.entryMargin=1.5f;for(float inset=24;inset<Mathf.Min(100,branch.Length*.25f);inset+=2){var at=branch.At(inset,out _);float mainStation=road.Project(at,out float distance);if(distance>road.HalfWidth(mainStation)+branch.halfWidth+3){branch.entryInset=inset;break;}}if(title=="Granite Saddle"){branch.entrySpeed=24;branch.entrySpeedDistance=100;}if(title=="Laurel Switchbacks"){branch.entrySpeed=20;branch.entrySpeedDistance=75;}branch.aiValidated=true;branches.Add(branch);return branch;
                }
                if(forest)
                {
                    Add("Fern Gully",1480,750,32,3.7f,new(154,39,-113),new(92,34,-15),new(17,31,90),new(-12,30,165));
                    Add("Granite Saddle",1980,1480,44,3.8f,new(550,44,-208),new(470,43,-201),new(375,40,-195),new(290,40,-199));
                }
                else
                {
                    Add("Laurel Switchbacks",1610,710,28,4.0f,new(251,49,-290),new(321,64,-255),new(353,71,-182),new(414,79,-212),new(475,84,-157));
                    Add("Granite Creek Cut",4270,3230,52,4.2f,new(-103,9,470),new(-149,13,382),new(-274,16,295),new(-427,12,181),new(-556,9,104));
                }
                // The second branch gets a deliberate gap on a straight interior approach.
                var gapBranch=branches[1];var idx=new PathIndex(gapBranch.points);float takeoff=idx.Length*.50f;
                for(int i=0;i<gapBranch.points.Length;i++)gapBranch.points[i].y+=Jump(idx.distances[i]-(takeoff-34),34,3.2f,forest?11:14,65);
                var gate0=race.gates[0];float origin=road.Project(gate0.transform.position,out _);
                race.gates=race.gates.OrderBy(g=>road.Relative(road.Project(g.transform.position,out _),origin)).ToArray();
                for(int i=0;i<race.gates.Length;i++)
                {
                    var gate=race.gates[i];float s=road.Project(gate.transform.position,out _);
                    foreach(var branch in branches)
                    {
                        if(Mathf.Abs(s-branch.entryRoad)<65)s=branch.entryRoad-70;
                        if(Mathf.Abs(s-branch.exitRoad)<75)s=branch.exitRoad+80;
                    }
                    var p=road.At(s,out var f);gate.transform.SetPositionAndRotation(p+Vector3.up*1.6f,Quaternion.LookRotation(Vector3.ProjectOnPlane(f,Vector3.up)));gate.name=i==0?race.courseName+" START FINISH":race.courseName+" CP "+i;
                }
                race.gates=race.gates.OrderBy(g=>road.Relative(road.Project(g.transform.position,out _),origin)).ToArray();
                foreach(var branch in branches)
                {
                    branch.bypassedGates=race.gates.Select((g,i)=>(s:road.Project(g.transform.position,out _),i)).Where(x=>x.i>0&&x.s>branch.entryRoad&&x.s<branch.exitRoad).Select(x=>x.i).ToArray();
                    report.Add(race.courseName+" / "+branch.title+": entry "+branch.entryRoad+" exit "+branch.exitRoad+" normal "+(branch.exitRoad-branch.entryRoad)+" branch "+branch.Length+" bypass="+string.Join(",",branch.bypassedGates));
                }
                SculptReverseTerrain(race,sourceIndex,new PathIndex(road.points,true),branches,forest,report);
                if(forest)ReverseCaveCrossing(root,branches.First(b=>b.title=="Fern Gully"),report);
                var spawn=race.vehicle.GetComponent<VehicleRespawn>().spawnPoint;spawn.position=road.At(origin-32,out var direction)+Vector3.up*.7f;spawn.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(direction,Vector3.up));race.vehicle.transform.SetPositionAndRotation(spawn.position,spawn.rotation);
                foreach(var b in branches)
                {
                    Marker(root,b.At(28,out var f),f,b.title+" / OPTIONAL",b.halfWidth);
                    Marker(root,b.At(b.Length-30,out f),f,"REJOIN / "+race.courseName,b.halfWidth);
                }
                Marker(root,gapBranch.At(takeoff-15,out var jumpForward),jumpForward,"GAP / STRAIGHT LANDING",gapBranch.halfWidth);
                EditorUtility.SetDirty(race);EditorUtility.SetDirty(road);Physics.SyncTransforms();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
                File.WriteAllText(Evidence+"/route-"+scene.name+".json",JsonUtility.ToJson(new Inventory{name=scene.name,road=road.points,gates=race.gates.Select(g=>g.transform.position).ToArray(),branches=branches.Select(b=>new Branch{title=b.title,points=b.points,entry=b.entryRoad,exit=b.exitRoad,gates=b.bypassedGates}).ToArray()},true));
            }
            File.WriteAllLines(Evidence+"/reverse-authoring.txt",report);
        }
        static void Marker(Transform root,Vector3 p,Vector3 f,string text,float width)
        {
            var right=Vector3.Cross(Vector3.up,f).normalized;var mat=Material("Reverse trail amber",new(.83f,.42f,.055f));
            foreach(int side in new[]{-1,1})
            {
                var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name="Reverse branch entrance marker";go.transform.SetParent(root);go.transform.position=p+right*side*(width+1)+Vector3.up;go.transform.localScale=new(.24f,2,.24f);go.GetComponent<Renderer>().sharedMaterial=mat;Object.DestroyImmediate(go.GetComponent<Collider>());
            }
            var sign=new GameObject(text).transform;sign.SetParent(root);sign.SetPositionAndRotation(p-right*(width+3)+Vector3.up*2.2f,Quaternion.LookRotation(Vector3.ProjectOnPlane(f,Vector3.up)));sign.gameObject.AddComponent<PhysicalSign>();
            var board=GameObject.CreatePrimitive(PrimitiveType.Cube);board.transform.SetParent(sign,false);board.transform.localScale=new(5.5f,1.1f,.12f);board.GetComponent<Renderer>().sharedMaterial=Material("Reverse sign dark",new(.025f,.06f,.045f));Object.DestroyImmediate(board.GetComponent<Collider>());
            var label=new GameObject("Reverse physical sign text").AddComponent<TextMesh>();label.transform.SetParent(sign,false);label.transform.localPosition=new(0,0,-.08f);label.text=text;label.characterSize=.075f;label.fontSize=64;label.anchor=TextAnchor.MiddleCenter;label.alignment=TextAlignment.Center;label.color=new(1,.86f,.35f);FitMarkerLabel(label);
        }
        static void FitMarkerLabel(TextMesh label)
        {
            float width=label.GetComponent<Renderer>().localBounds.size.x;
            if(width>5.1f)label.characterSize*=5.1f/width;
        }
        static void ReverseCaveCrossing(Transform root,WoodlandRoute branch,List<string> report)
        {
            // New transverse grotto passage, only in this copied reverse scene.
            // Keep the cave roof, existing longitudinal tunnel and forward scene intact.
            var path=new PathIndex(branch.points);int cut=0,rocks=0;float crossing=-1;
            foreach(var mf in Object.FindObjectsByType<MeshFilter>().Where(f=>f.name=="Echo Cave enclosed rock"))
            {
                var mesh=mf.sharedMesh;var vertices=mesh.vertices;var triangles=mesh.triangles;var kept=new List<int>();
                for(int i=0;i<triangles.Length;i+=3)
                {
                    var a=mf.transform.TransformPoint(vertices[triangles[i]]);var b=mf.transform.TransformPoint(vertices[triangles[i+1]]);var c=mf.transform.TransformPoint(vertices[triangles[i+2]]);var mid=(a+b+c)/3;
                    float distance=path.Near(mid,out var floor,out var station);
                    if(distance<branch.halfWidth+3&&Mathf.Min(a.y,b.y,c.y)<floor.y+5.2f){cut++;crossing=station;continue;}
                    kept.AddRange(new[]{triangles[i],triangles[i+1],triangles[i+2]});
                }
                var copy=Object.Instantiate(mesh);copy.triangles=kept.ToArray();copy.RecalculateBounds();copy=StoreMesh(copy,"Fern transverse cave passage");mf.sharedMesh=copy;mf.GetComponent<MeshCollider>().sharedMesh=copy;
            }
            foreach(var collider in Object.FindObjectsByType<Renderer>().Where(c=>c.name=="Cave rock buttress").ToArray())
            {if(path.Near(collider.bounds.center,out _,out _) < branch.halfWidth+5){rocks++;Object.DestroyImmediate(collider.gameObject);}}
            if(crossing>0){var at=branch.At(crossing-18,out var f);Marker(root,at,f,"FERN GROTTO / THROUGH PASSAGE",branch.halfWidth);var lamp=new GameObject("Fern grotto amber light").AddComponent<Light>();lamp.transform.SetParent(root);lamp.transform.position=branch.At(crossing,out _)+Vector3.up*3.8f;lamp.type=LightType.Point;lamp.range=18;lamp.intensity=3;lamp.color=new(1,.8f,.42f);lamp.shadows=LightShadows.None;}
            report.Add("Fern reverse transverse grotto: "+cut+" wall triangles opened below 5.2m headroom; "+rocks+" intersecting buttresses removed; roof retained.");
        }
        static void ReversePavementApron(Transform root,RaceRoad road)
        {
            // The west continuation has a 15 cm box-pavement edge across Hwy 92.
            // Its forward drop becomes an exposed collision step in reverse.
            float seam=road.Project(new Vector3(-500,8,540.192f),out _);var vertices=new List<Vector3>();var tris=new List<int>();
            for(int i=0;i<=32;i++)
            {
                float s=seam-8+i*.5f;var at=road.At(s,out var f);var right=Vector3.Cross(Vector3.up,f).normalized;
                float height=Mathf.Lerp(at.y+.006f,8.097f,Ease(seam-8,seam+.1f,s));
                foreach(float side in new[]{-8.25f,8.25f}){var p=at+right*side;p.y=height;vertices.Add(p);}
                if(i<32){int a=i*2;tris.AddRange(new[]{a,a+2,a+1,a+1,a+2,a+3});}
            }
            var mesh=new Mesh();mesh.SetVertices(vertices);mesh.SetTriangles(tris,0);mesh.RecalculateNormals();mesh.RecalculateBounds();mesh=StoreMesh(mesh,"Reverse west pavement apron");
            var go=new GameObject("Reverse flush west pavement apron",typeof(MeshFilter),typeof(MeshRenderer),typeof(MeshCollider));go.transform.SetParent(root);go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<MeshCollider>().sharedMesh=mesh;go.GetComponent<Renderer>().sharedMaterial=Material("Reverse asphalt",new(.19f,.22f,.24f));
        }
        static void ReverseRoadworks(Transform root,RaceRoad source,List<string> report)
        {
            var original=GameObject.Find(Phase4Setup.RootName).GetComponentsInChildren<MeshCollider>().First(c=>c.name.StartsWith("Takeoff"));
            var parent=original.transform.parent;var vertices=new List<Vector3>();var tris=new List<int>();
            // Remove only the directional takeoff surface; the accepted signs stay.
            original.enabled=false;original.GetComponent<MeshRenderer>().enabled=false;
            for(int i=0;i<=140;i++)
            {
                float x=i*.5f;float z=90-x;float h=x<42?3.2f*Mathf.Pow(x/42,2):x<47?3.2f*(1-Ease(42,47,x)):0;
                foreach(float side in new[]{-4.5f,1.5f}){var p=parent.TransformPoint(new(side,0,z));p.y=source.At(source.Project(p,out _),out _).y+h+.018f;vertices.Add(p);}
                if(i<140){int a=i*2;tris.AddRange(new[]{a,a+1,a+2,a+1,a+3,a+2});}
            }
            var mesh=new Mesh();mesh.SetVertices(vertices);mesh.SetTriangles(tris,0);mesh.RecalculateNormals();mesh.RecalculateBounds();mesh=StoreMesh(mesh,"Street reverse raised pavement");
            var go=new GameObject("Reverse supported roadworks transition",typeof(MeshFilter),typeof(MeshRenderer),typeof(MeshCollider));go.transform.SetParent(root);go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<MeshCollider>().sharedMesh=mesh;go.GetComponent<Renderer>().sharedMaterial=original.GetComponent<Renderer>().sharedMaterial;
            report.Add("Street roadworks: reverse approach +90m to +20m, 42m quadratic run / 3.2m rise; original takeoff collider/render disabled only in reverse scene. Ground lane remains viable.");
        }
        static bool ReverseLanding(float station,ForestLayout layout){if(!layout)return false;for(int i=0;i<layout.jumpStarts.Length;i++)if(station>=layout.jumpStarts[i]+45&&station<=Mathf.Min(layout.jumpStarts[i]+160,layout.jumpEnds[i]+10))return true;return false;}
        static void SculptReverseTerrain(RaceDirector race,PathIndex source,PathIndex current,List<WoodlandRoute> branches,bool forest,List<string> report)
        {
            var streetProfile=forest&&race.ambientRoad?new PathIndex(race.ambientRoad.points,true):null;var mainLayout=Object.FindAnyObjectByType<ForestLayout>();var indices=branches.Select(b=>new PathIndex(b.points)).ToArray();var buildings=GameObject.Find("Remembered houses and approximate buildings").transform.Cast<Transform>().Select(t=>t.position).ToArray();
            int changed=0,tiles=0;string prefix=forest?"Forest":"Street";
            foreach(var mf in GameObject.Find("Memory loop - north is +Z").GetComponentsInChildren<MeshFilter>())
            {
                var mesh=mf.sharedMesh;var vertices=mesh.vertices;var colors=mesh.colors;bool touched=false;
                for(int i=0;i<vertices.Length;i++)
                {
                    var p=mf.transform.TransformPoint(vertices[i]);var original=p;float d=current.Near(p,out var main,out _);
                    if(forest&&d<17){float oldDistance=source.Near(p,out var old,out _);float ms=race.road.Project(main,out _);bool landing=ReverseLanding(ms,mainLayout);if(oldDistance<17&&(Mathf.Abs(main.y-old.y)>.001f||landing))p.y=Mathf.Lerp(p.y,main.y,1-Ease(landing?10:race.road.HalfWidth(ms)+1,17,d));}
                    for(int b=0;b<indices.Length;b++)
                    {
                        float bd=indices[b].Near(p,out var at,out _);if(bd>16||bd>=d)continue;
                        float property=buildings.Min(h=>Vector2.Distance(new(p.x,p.z),new(h.x,h.z)));
                        float streetDistance=streetProfile==null?float.MaxValue:streetProfile.Near(p,out _,out _);float preserve=Ease(25,38,property)*Ease(5,10,d)*Ease(8,18,streetDistance);float w=branches[b].halfWidth;
                        p.y=Mathf.Lerp(p.y,at.y,(1-Ease(w+1,16,bd))*preserve);
                        if(colors.Length==vertices.Length)colors[i]=Color.Lerp(colors[i],new(.40f,.28f,.14f),(1-Ease(w-.3f,w+1,bd))*preserve);
                        touched=true;
                    }
                    if(Mathf.Abs(p.y-original.y)>.001f){touched=true;changed++;}vertices[i]=mf.transform.InverseTransformPoint(p);
                }
                if(!touched)continue;
                var copy=Object.Instantiate(mesh);copy.vertices=vertices;copy.colors=colors;copy.RecalculateNormals();copy.RecalculateBounds();copy=StoreMesh(copy,prefix+"-"+mf.name);mf.sharedMesh=copy;mf.GetComponent<MeshCollider>().sharedMesh=copy;tiles++;
            }
            Physics.SyncTransforms();
            // Retire only trunks inside the new driving cores. Each affected forest batch
            // is rebuilt from surviving trunk authoring data, using copied mesh assets.
            var removed=new List<Vector3>();var woods=GameObject.Find("Woods replacing later subdivisions");
            var trees=(woods?woods.GetComponentsInChildren<Collider>():Array.Empty<Collider>()).Concat(Object.FindObjectsByType<Collider>().Where(c=>c.name=="Forest trunk collision")).Distinct().ToArray();
            foreach(var tree in trees)
            {
                var p=tree.bounds.center;
                if(indices.Select((index,b)=>index.Near(p,out _,out _) < branches[b].halfWidth+2).Any(x=>x)||(forest&&current.Near(p,out _,out float landingS)<11&&ReverseLanding(landingS,mainLayout))){removed.Add(p);Object.DestroyImmediate(tree.gameObject);}
            }
            if(removed.Count>0)TrimTreeMeshes(removed,prefix);
            report.Add(prefix+" reverse terrain: "+tiles+" copied tiles, "+changed+" height vertices, "+removed.Count+" scoped corridor trunks; properties protected within 25m.");
        }
        static void TrimTreeMeshes(List<Vector3> removed,string prefix)
        {
            int count=0;
            foreach(var mf in Object.FindObjectsByType<MeshFilter>())
            {
                var renderer=mf.GetComponent<Renderer>();if(!renderer||!renderer.sharedMaterial||!AssetDatabase.GetAssetPath(renderer.sharedMaterial).Contains("Vegetation"))continue;
                if(!removed.Any(p=>{var b=renderer.bounds;return p.x>=b.min.x&&p.x<=b.max.x&&p.z>=b.min.z&&p.z<=b.max.z;}))continue;
                var mesh=mf.sharedMesh;var vs=mesh.vertices;var triangles=mesh.triangles;var parent=Enumerable.Range(0,vs.Length).ToArray();
                int Find(int a){while(parent[a]!=a){parent[a]=parent[parent[a]];a=parent[a];}return a;}
                for(int i=0;i<triangles.Length;i+=3){int a=Find(triangles[i]);parent[Find(triangles[i+1])]=a;parent[Find(triangles[i+2])]=a;}
                var centers=new Dictionary<int,(Vector3 sum,int count)>();
                for(int i=0;i<vs.Length;i++){int k=Find(i);centers.TryGetValue(k,out var c);centers[k]=(c.sum+mf.transform.TransformPoint(vs[i]),c.count+1);}
                var retire=new HashSet<int>();foreach(var pair in centers){var p=pair.Value.sum/pair.Value.count;if(removed.Any(r=>Vector2.Distance(new(r.x,r.z),new(p.x,p.z))<1.6f))retire.Add(pair.Key);}
                if(retire.Count==0)continue;var kept=new List<int>();for(int i=0;i<triangles.Length;i+=3)if(!retire.Contains(Find(triangles[i])))kept.AddRange(new[]{triangles[i],triangles[i+1],triangles[i+2]});
                var copy=Object.Instantiate(mesh);copy.triangles=kept.ToArray();copy.RecalculateBounds();mf.sharedMesh=StoreMesh(copy,prefix+"-Scoped-tree-clearance-"+count++);
            }
        }
    }
}
