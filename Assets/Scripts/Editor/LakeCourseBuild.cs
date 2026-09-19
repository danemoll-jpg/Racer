using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;
namespace Racer.Editor
{
    public static class LakeCourseBuild
    {
        public const string ScenePath="Assets/Scenes/LakeWoods.unity", Folder="Assets/Track/LakeWoods", Evidence="Docs/CR040-054-055";
        static RaceRoad road;
        static WoodlandRoute shortcut;
        static Material amber,water;
        static float Blend(float a,float b,float v)=>Mathf.SmoothStep(0,1,Mathf.InverseLerp(a,b,v));
        static Vector3[] Curve(IReadOnlyList<Vector3> knots,bool closed)
        {
            var result=new List<Vector3>();int n=knots.Count;
            Vector3 K(int i)=>knots[closed?(i+n)%n:Mathf.Clamp(i,0,n-1)];
            for(int i=0;i<(closed?n:n-1);i++)
            {
                var a=K(i-1);var b=K(i);var c=K(i+1);var d=K(i+2);int count=Mathf.CeilToInt(Vector3.Distance(b,c)/2);
                for(int j=0;j<count;j++){float t=j/(float)count;result.Add(.5f*(2*b+(-a+c)*t+(2*a-5*b+4*c-d)*t*t+(-a+3*b-3*c+d)*t*t*t));}
            }
            if(!closed)result.Add(knots[^1]);return result.ToArray();
        }
        static float Near(Vector3 p,Vector3[] points,out Vector3 at)
        {
            float best=float.MaxValue;at=default;
            for(int i=1;i<points.Length;i++)
            {
                var v=points[i]-points[i-1];v.y=0;var q=p-points[i-1];q.y=0;
                float t=Mathf.Clamp01(Vector3.Dot(q,v)/Mathf.Max(.001f,v.sqrMagnitude));float d=(q-v*t).sqrMagnitude;
                if(d<best){best=d;at=Vector3.Lerp(points[i-1],points[i],t);}
            }
            return Mathf.Sqrt(best);
        }
        static Material Mat(string name,Color color)
        {
            var m=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=name,color=color,enableInstancing=true};AssetDatabase.CreateAsset(m,Folder+"/"+name+".mat");return m;
        }
        static void Marker(Transform root,Vector3 p,Vector3 f,string text)
        {
            var go=new GameObject(text);go.transform.SetParent(root);go.transform.SetPositionAndRotation(p,Quaternion.LookRotation(Vector3.ProjectOnPlane(f,Vector3.up)));
            var label=new GameObject("Trail sign").AddComponent<TextMesh>();label.transform.SetParent(go.transform,false);label.transform.localPosition=new(0,3,0);label.text=text;label.fontSize=48;label.characterSize=.16f;label.anchor=TextAnchor.MiddleCenter;
            label.GetComponent<Renderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Track/Woodland/Lettering.mat");
            var board=GameObject.CreatePrimitive(PrimitiveType.Cube);board.transform.SetParent(go.transform,false);board.transform.localPosition=new(0,3,.08f);board.transform.localScale=new(9,1.4f,.1f);board.GetComponent<Renderer>().sharedMaterial=amber;Object.DestroyImmediate(board.GetComponent<Collider>());
        }
        public static void Apply()
        {
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if(Application.isPlaying||scene.isDirty||scene.path!=StreetLoopBuilder.ScenePath||File.Exists(ScenePath))throw new InvalidOperationException("Saved original scene and unapplied new course required");
            Directory.CreateDirectory(Folder);Directory.CreateDirectory(Evidence);
            EditorSceneManager.SaveScene(scene,ScenePath,true);scene=EditorSceneManager.OpenScene(ScenePath);
            var race=Object.FindAnyObjectByType<RaceDirector>();var original=race.road;original.Initialize();
            var sites=GameObject.Find("Remembered houses and approximate buildings").transform.Cast<Transform>().ToArray();
            File.WriteAllLines(Evidence+"/sites-before.txt",sites.Select(t=>t.name+" "+t.position.ToString("F5")+" "+t.rotation.ToString("F5")));
            var root=new GameObject("Lake & Woods circuit").transform;
            road=new GameObject("Lake course AI and recovery route").AddComponent<RaceRoad>();road.transform.SetParent(root);
            // Reverse the accepted neighborhood road, passing friend and Dan, then a broad wooded loop.
            var knots=new List<Vector3>();for(float s=820;s>=480;s-=10)knots.Add(original.At(s,out _));
            knots.AddRange(new[]{new Vector3(375,81,140),new(315,70,210),new(200,54,270),new(50,36,230),new(-100,20,120),new(-90,20,-70),new(60,33,-145),new(170,46,-145),new(270,53,-200),new(365,63,-260),new(460,49,-250)});
            road.points=Curve(knots,true);road.Initialize();race.road=road;race.courseId="lake-v1";race.courseName="Lake & Woods";
            // Supported dirt kickers: steep takeoff, a low gully floor, long graded landing.
            var jumps=new[]{(s:620f,run:30f,h:4.5f,gap:14f),(s:1020f,run:42f,h:6f,gap:22f),(s:1390f,run:34f,h:5f,gap:18f)};
            float distance=0;var before=(Vector3[])road.points.Clone();
            for(int i=1;i<road.points.Length;i++)
            {
                distance+=Vector3.Distance(before[i-1],before[i]);
                foreach(var j in jumps)
                {
                    float x=distance-j.s;if(x<0||x>j.run+j.gap+65)continue;
                    float y=x<j.run?j.h*Mathf.Pow(x/j.run,2):x<j.run+6?Mathf.Lerp(j.h,-3,Blend(j.run,j.run+6,x)):-3*(1-Blend(j.run+j.gap,j.run+j.gap+65,x));
                    road.points[i].y+=y;
                }
            }
            // New route instance rebuilds distances after the sculpt.
            var finalPoints=road.points;Object.DestroyImmediate(road);road=root.GetChild(0).gameObject.AddComponent<RaceRoad>();road.points=finalPoints;road.Initialize();race.road=road;
            race.highwayTrafficCount=0; // No highway on this course; original population/difficulty stays intact.
            foreach(var branch in Object.FindObjectsByType<WoodlandRoute>())Object.DestroyImmediate(branch);
            shortcut=new GameObject("Birch Hollow shortcut").AddComponent<WoodlandRoute>();shortcut.transform.SetParent(root);shortcut.title="Birch Hollow";shortcut.entryRoad=740;shortcut.exitRoad=1250;shortcut.halfWidth=10;shortcut.recommendedSpeed=29;
            var a=road.At(shortcut.entryRoad,out var af);var b=road.At(shortcut.exitRoad,out var bf);
            shortcut.points=Curve(new[]{a,a+af*30,Vector3.Lerp(a,b,.45f)+new Vector3(15,-5,10),b-bf*40,b},false);
            // Preserve original gate objects as inactive scenery data; the director only receives new gates.
            var template=race.gates[1];var gates=new List<RaceGate>();
            for(float s=110;s<road.Length-90;s+=210)
            {
                if(Mathf.Abs(s-shortcut.entryRoad)<100||Mathf.Abs(s-shortcut.exitRoad)<110||jumps.Any(j=>s>j.s&&s<j.s+j.run+j.gap+110))continue;
                var g=Object.Instantiate(template,root);int index=gates.Count;g.name=index==0?"Lake START FINISH":$"Lake CP {index:00}";
                var p=road.At(s,out var f);g.transform.SetPositionAndRotation(p+Vector3.up*1.6f,Quaternion.LookRotation(Vector3.ProjectOnPlane(f,Vector3.up)));g.halfWidth=12;g.upperHeight=30;
                foreach(var t in g.GetComponentsInChildren<TextMesh>())if(t.name=="Gate label")t.text=index==0?"LAKE START / FINISH >":$"CP {index:00} >";
                gates.Add(g);
            }
            foreach(var g in race.gates)g.gameObject.SetActive(false);race.gates=gates.ToArray();
            shortcut.bypassedGates=Enumerable.Range(1,gates.Count-1).Where(i=>{float s=road.Project(gates[i].transform.position,out _);return s>shortcut.entryRoad&&s<shortcut.exitRoad;}).ToArray();
            if(shortcut.bypassedGates.Length==0)throw new InvalidOperationException("Shortcut needs explicit bypass gates");
            var spawn=race.vehicle.GetComponent<VehicleRespawn>().spawnPoint;spawn.position=road.At(75,out var facing)+Vector3.up;spawn.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(facing,Vector3.up));race.vehicle.transform.SetPositionAndRotation(spawn.position,spawn.rotation);
            amber=Mat("Lake trail amber",new(.24f,.12f,.035f));water=Mat("Lake water",new(.08f,.34f,.42f));
            var terrain=GameObject.Find("Memory loop - north is +Z").GetComponentsInChildren<MeshFilter>();int changed=0;
            var closed=road.points.Append(road.points[0]).ToArray();Vector3 lake=new(642,78,-20);
            foreach(var mf in terrain)
            {
                var mesh=Object.Instantiate(mf.sharedMesh);var vertices=mesh.vertices;var colors=mesh.colors;bool edited=false;
                for(int i=0;i<vertices.Length;i++)
                {
                    var p=vertices[i];if(p.x<-145||p.x>740||p.z<-305||p.z>320)continue;
                    float d=Near(p,closed,out var at),bd=shortcut.Project(p,out _);shortcut.Project(p,out float branchDistance);
                    if(branchDistance<d){d=branchDistance;at=shortcut.At(bd,out _);}
                    float originalDistance=Near(p,original.points,out _);
                    float preserve=Blend(12,24,originalDistance);
                    preserve*=Blend(42,65,sites.Min(t=>Vector2.Distance(new(p.x,p.z),new(t.position.x,t.position.z))));
                    if(d<25&&preserve>0)
                    {
                        p.y=Mathf.Lerp(p.y,at.y,(1-Blend(13,25,d))*preserve);
                        if(colors.Length==vertices.Length)colors[i]=Color.Lerp(colors[i],new Color(.43f,.30f,.17f),(1-Blend(9,14,d))*preserve);
                    }
                    float ld=Vector2.Distance(new(p.x,p.z),new(lake.x,lake.z));
                    if(ld<85&&preserve>0){p.y=Mathf.Lerp(p.y,74,1-Blend(56,85,ld));if(colors.Length==vertices.Length)colors[i]=Color.Lerp(colors[i],new Color(.36f,.34f,.23f),1-Blend(57,83,ld));}
                    if(p!=vertices[i]){vertices[i]=p;edited=true;}
                }
                if(!edited){Object.DestroyImmediate(mesh);continue;}
                mesh.vertices=vertices;mesh.colors=colors;mesh.RecalculateNormals();mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,Folder+"/Terrain-"+changed+++".asset");mf.sharedMesh=mesh;
                if(mf.TryGetComponent<MeshCollider>(out var collider))collider.sharedMesh=mesh;
            }
            Physics.SyncTransforms();
            var surface=GameObject.CreatePrimitive(PrimitiveType.Cylinder);surface.name="Friend's lake";surface.transform.SetParent(root);surface.transform.position=lake;surface.transform.localScale=new(126,.035f,126);surface.GetComponent<Renderer>().sharedMaterial=water;Object.DestroyImmediate(surface.GetComponent<Collider>());
            foreach(var j in jumps)
            {
                var p=road.At(j.s-25,out var f);Marker(root,p-Vector3.Cross(Vector3.up,f).normalized*12,f,"GULLY JUMP / LAND STRAIGHT");
                var creek=GameObject.CreatePrimitive(PrimitiveType.Cube);creek.name="Creek crossing";creek.transform.SetParent(root);p=road.At(j.s+j.run+10,out f);creek.transform.SetPositionAndRotation(p+Vector3.up*.08f,Quaternion.LookRotation(f));creek.transform.localScale=new(24,.03f,4);creek.GetComponent<Renderer>().sharedMaterial=water;Object.DestroyImmediate(creek.GetComponent<Collider>());
            }
            Marker(root,a-Vector3.Cross(Vector3.up,af).normalized*13,af,"BIRCH HOLLOW / LEGAL SHORTCUT");
            for(float s=360;s<road.Length-100;s+=32)
            {
                var p=road.At(s,out var f);var right=Vector3.Cross(Vector3.up,f).normalized;
                foreach(int side in new[]{-1,1})
                {
                    var post=GameObject.CreatePrimitive(PrimitiveType.Cube);post.name="Lake route edge";post.transform.SetParent(root);var point=p+right*side*11.5f;point.y=Phase6Buildings.Ground(point);post.transform.position=point+Vector3.up*.6f;post.transform.localScale=new(.17f,1.2f,.17f);post.GetComponent<Renderer>().sharedMaterial=amber;Object.DestroyImmediate(post.GetComponent<Collider>());
                }
            }
            Forest(closed,lake);
            EditorUtility.SetDirty(race);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            File.WriteAllLines(Evidence+"/sites-after.txt",sites.Select(t=>t.name+" "+t.position.ToString("F5")+" "+t.rotation.ToString("F5")));
            File.WriteAllText(Evidence+"/course.txt",$"Lake-v1 length {road.Length}; gates {gates.Count}; independent terrain meshes {changed}; shortcut {shortcut.entryRoad}..{shortcut.exitRoad}; bypass {string.Join(",",shortcut.bypassedGates)}\n"+string.Join("\n",gates.Select((g,i)=>$"{i}: {road.Project(g.transform.position,out _)} {g.transform.position}")));
        }
        static void Forest(Vector3[] path,Vector3 lake)
        {
            var originalRoad=StreetLoopBuilder.Route();
            var woods=GameObject.Find("Woods replacing later subdivisions");var boxes=woods.GetComponentsInChildren<BoxCollider>();
            var shapes=new[]{"BroadCrown","IrregularCrown","UprightCrown"}.Select(n=>AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Vegetation/Phase6/"+n+".asset")).ToArray();
            var trunk=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Vegetation/Phase6/Trunk.asset");var mat=AssetDatabase.LoadAssetAtPath<Material>("Assets/Vegetation/Phase6/Forest.mat");
            var batches=new Dictionary<Vector2Int,List<CombineInstance>>();var temporary=new List<Mesh>();
            foreach(var box in boxes)
            {
                var bounds=box.bounds;var p=new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);float d=Near(p,path,out _);shortcut.Project(p,out var sd);d=Mathf.Min(d,sd);
                float ld=Vector2.Distance(new(p.x,p.z),new(lake.x,lake.z));
                if(d<16||ld<72){Object.DestroyImmediate(box.gameObject);continue;}
                if(d<28){float y=Phase6Buildings.Ground(p);box.transform.position+=Vector3.up*(y-p.y);p.y=y;}
                float h=bounds.size.y*2,patch=Mathf.PerlinNoise((p.x+913)/65,(p.z+771)/65),hash=Mathf.Repeat(Mathf.Sin(p.x*12.9898f+p.z*78.233f)*43758.5453f,1);
                int kind=patch<.43f?2:patch>.57f?1:0;
                float radius=Mathf.Min(h*Mathf.Lerp(.27f,.34f,hash)*1.35f,Mathf.Max(.1f,d-12));
                // Keep the accepted clear yards and road views in the copied scene.
                radius=Mathf.Min(radius,Mathf.Max(.1f,Phase6Buildings.YardDistance(p)-27),Mathf.Max(.1f,StreetLoopBuilder.Nearest(p,originalRoad,out _)-16));
                var key=new Vector2Int(Mathf.FloorToInt(p.x/160),Mathf.FloorToInt(p.z/160));if(!batches.TryGetValue(key,out var batch))batches[key]=batch=new();
                var crown=Object.Instantiate(shapes[kind]);var tint=Color.Lerp(new(.24f,.34f,.19f),new(.39f,.46f,.25f),Mathf.Clamp01(patch*.8f+hash*.2f));crown.colors=crown.colors.Select(c=>c*tint).ToArray();temporary.Add(crown);
                batch.Add(new CombineInstance{mesh=crown,transform=Matrix4x4.TRS(p+Vector3.up*h*.36f,Quaternion.Euler(0,hash*360,0),new(radius,h*Mathf.Lerp(.59f,.70f,hash),radius))});
                var bark=Object.Instantiate(trunk);bark.colors=Enumerable.Repeat(new Color(.25f,.20f,.145f),bark.vertexCount).ToArray();temporary.Add(bark);batch.Add(new CombineInstance{mesh=bark,transform=box.transform.localToWorldMatrix*Matrix4x4.TRS(box.center,Quaternion.identity,box.size)});
            }
            foreach(var mf in woods.GetComponentsInChildren<MeshFilter>())Object.DestroyImmediate(mf.gameObject);
            foreach(var pair in batches)
            {
                var mesh=new Mesh{indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};mesh.CombineMeshes(pair.Value.ToArray());AssetDatabase.CreateAsset(mesh,$"{Folder}/Forest-{pair.Key.x}-{pair.Key.y}.asset");
                var go=new GameObject("Lake forest batch",typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(woods.transform);go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<MeshRenderer>().sharedMaterial=mat;
            }
            foreach(var mesh in temporary)Object.DestroyImmediate(mesh);
        }
    }
}
