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
        public static void MountainLocalCorrections()
        {
            foreach(string name in new[]{"MountainLoop","MountainLoopReverse"}){
                var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+name+".unity");owner=Object.FindAnyObjectByType<RaceDirector>();owner.road.Initialize();worldRoot=GameObject.Find("CR110 mountain race geometry").transform;
                if(!owner.reverseCourse){var p=owner.road.At(940,out var f);owner.gates[^1].transform.SetPositionAndRotation(p+Vector3.up*1.6f,Quaternion.LookRotation(Vector3.ProjectOnPlane(f,Vector3.up)));}
                else{
                    // The unused upper 40 m of the catch slope overlapped the ridge entrance.
                    // Actual first-playable landings begin at s425 (ATV) / s449 (motorcycle).
                    var launch=GameObject.Find("CR110 reverse summit launch").transform;var old=GameObject.Find("Ground_CR110 reverse landing");Object.DestroyImmediate(old);
                    var landing=Enumerable.Range(0,271).Select(i=>launch.TransformPoint(new Vector3(0,LandingHeight(370+i*.5f)-152,370+i*.5f))).ToArray();MountainSurface("CR110 reverse landing",landing,24);
                    var ridge=Object.FindObjectsByType<WoodlandRoute>().Single(b=>b.title.Contains("Ridge Cut"));var main=owner.road.points.Concat(new[]{owner.road.points[0]}).ToArray();
                    Terrain("CR110 clear ridge landing junction",p=>{if(Vector2.Distance(new(p.x,p.z),new(1000,-55))>60)return p;float a=Near(p,main,out var pa),b=Near(p,ridge.points,out var pb);float d=Mathf.Min(a,b);if(d<18){float y=(a<b?pa.y:pb.y)-.2f;p.y=Mathf.Min(p.y,Mathf.Lerp(p.y,y,1-Smooth(10,18,d)));}return p;});
                }
                var signs=GameObject.Find("CR110 selected direction signs");if(signs)Object.DestroyImmediate(signs);var activity=GameObject.Find("South Face Summit Flight activity");if(activity)Object.DestroyImmediate(activity);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            }
            MountainDetails();
        }
        public static void MountainObstruction()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/MountainLoopReverse.unity");var race=Object.FindAnyObjectByType<RaceDirector>();race.road.Initialize();var p=new Vector3(1030.786f,142.118f,-49.35f);float station=race.road.Project(p,out _);race.road.At(station,out var f);var ridge=Object.FindObjectsByType<WoodlandRoute>().Single(b=>b.title.Contains("Ridge Cut"));var rows=new List<string>{"station="+station+" road="+race.road.At(station,out _)+" tangent="+f};foreach(var dir in new[]{f,(ridge.At(18,out _)-p).normalized,Vector3.back,Vector3.left})foreach(var hit in Physics.SphereCastAll(p+Vector3.up*.35f,.7f,dir,35,~0,QueryTriggerInteraction.Ignore).OrderBy(h=>h.distance))rows.Add("dir="+dir+" hit="+hit.collider.name+" distance="+hit.distance+" normal="+hit.normal+" at="+hit.point);File.WriteAllLines(CR105Authoring.Evidence+"/reverse-obstruction.txt",rows);
            var camera=Camera.main;camera.transform.SetPositionAndRotation(p+new Vector3(15,18,15),Quaternion.LookRotation(p-camera.transform.position));camera.transform.LookAt(p);ThreeFeatureValidation.CaptureUi(CR105Authoring.Evidence+"/reverse-obstruction.png");
        }
        public static void MountainRouting()
        {
            foreach(string name in new[]{"MountainLoop","MountainLoopReverse"}){
                var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+name+".unity");
                var race=Object.FindAnyObjectByType<RaceDirector>();race.road.Initialize();
                float station=race.reverseCourse?100:race.road.Length-65;
                var point=race.road.At(station,out var heading);race.gates[0].transform.SetPositionAndRotation(point+Vector3.up*1.6f,Quaternion.LookRotation(Vector3.ProjectOnPlane(heading,Vector3.up)));
                var first=race.gates[0];race.gates=new[]{first}.Concat(race.gates.Skip(1).OrderBy(g=>race.road.Relative(race.road.Project(g.transform.position,out _),station))).ToArray();
                foreach(var branch in Object.FindObjectsByType<WoodlandRoute>().Where(b=>b.title.Contains("Ridge Cut"))){branch.entrySpeed=14;branch.entrySpeedDistance=65;branch.aiValidated=true;}
                var signs=GameObject.Find("CR110 selected direction signs");if(signs)Object.DestroyImmediate(signs);
                var activity=GameObject.Find("South Face Summit Flight activity");if(activity)Object.DestroyImmediate(activity);
                EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            }
            MountainDetails();
        }
        public static void MountainDetails()
        {
            foreach(string name in new[]{"MountainLoop","MountainLoopReverse"})
            {
                var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+name+".unity");owner=Object.FindAnyObjectByType<RaceDirector>();owner.road.Initialize();
                if(GameObject.Find("CR110 selected direction signs"))throw new Exception("Direction signs already present");worldRoot=new GameObject("CR110 selected direction signs").transform;
                // The first playable run cut the tight closing bend before the start plane.
                // Put START on the supported straight preceding that bend, outside both choices.
                float startStation=owner.reverseCourse?100:owner.road.Length-65;
                var start=owner.road.At(startStation,out var startForward);
                owner.gates[0].transform.SetPositionAndRotation(start+Vector3.up*1.6f,Quaternion.LookRotation(Vector3.ProjectOnPlane(startForward,Vector3.up)));
                var spawn=owner.vehicle.GetComponent<VehicleRespawn>().spawnPoint;
                spawn.position=owner.road.At(startStation-35,out var spawnForward)+Vector3.up*.7f;
                spawn.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(spawnForward,Vector3.up));
                owner.vehicle.transform.SetPositionAndRotation(spawn.position,spawn.rotation);
                var font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");var lettering=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/CR102 lettering.mat");
                void Sign(Vector3 p,Vector3 forward,string text){var root=new GameObject(text.Replace('\n',' ')).transform;root.SetParent(worldRoot);p.y=CR105Authoring.Ground(p);root.SetPositionAndRotation(p,Quaternion.LookRotation(Vector3.ProjectOnPlane(forward,Vector3.up)));Part(root,"Post",new(0,1.7f,0),new(.2f,3.4f,.2f),Mat("Sign timber",new(.22f,.14f,.07f)));Part(root,"Backed direction board",new(0,2.8f,0),new(8,2.3f,.2f),Mat("Sign timber",new(.22f,.14f,.07f)));var label=new GameObject("Selected course direction").AddComponent<TextMesh>();label.transform.SetParent(root,false);label.transform.localPosition=new(0,2.8f,-.12f);label.font=font;label.fontSize=64;label.characterSize=.1f;label.anchor=TextAnchor.MiddleCenter;label.alignment=TextAlignment.Center;label.color=new(1,.94f,.74f);label.text=text;font.RequestCharactersInTexture(text,64);var renderer=label.GetComponent<Renderer>();renderer.sharedMaterial=lettering;var bounds=renderer.localBounds;label.transform.localScale=Vector3.one*Mathf.Min(7.3f/Mathf.Max(.01f,bounds.size.x),1.9f/Mathf.Max(.01f,bounds.size.y));root.gameObject.AddComponent<PhysicalSign>();}
                foreach(var branch in Object.FindObjectsByType<WoodlandRoute>()){var p=owner.road.At(branch.entryRoad-25,out var f);Sign(p+Vector3.Cross(Vector3.up,f).normalized*9,f,owner.courseName.ToUpperInvariant()+"\nOPTIONAL: "+branch.title.ToUpperInvariant());}
                for(int i=0;i<owner.gates.Length;i++){var gate=owner.gates[i];foreach(var label in gate.GetComponentsInChildren<TextMesh>(true))label.text=i==0?"START / FINISH":"CP "+i;var p=owner.road.At(owner.road.Project(gate.transform.position,out _)-12,out var f);Sign(p+Vector3.Cross(Vector3.up,f).normalized*9,f,(i==0?owner.courseName.ToUpperInvariant():"CHECKPOINT "+i)+"\nFOLLOW NUMBERED GATES");}
                if(owner.reverseCourse)
                {
                    var old=GameObject.Find("CR102 summit signs");if(old)old.SetActive(false);old=GameObject.Find("CR098 summit guidance");if(old)old.SetActive(false);
                    var launch=GameObject.Find("CR110 reverse summit launch").transform;Sign(launch.TransformPoint(new Vector3(18,0,25)),launch.forward,"SOUTH FACE FLIGHT\nFULL RUN-UP / FULL THROTTLE");Sign(launch.TransformPoint(new Vector3(18,0,175)),launch.forward,"MOUNTAIN LOOP REVERSE\nLAUNCH STRAIGHT");Sign(launch.TransformPoint(new Vector3(29,0,445)),launch.forward,"LAND STRAIGHT / BRAKE\nFOLLOW THE RETURN");
                    var site=new GameObject("South Face Summit Flight activity").AddComponent<ActivitySite>();site.transform.SetParent(launch,false);site.transform.localPosition=new(0,HomewardHeight(220)-152,220);site.id="summit-southface";site.title="South Face Summit Flight";site.kind=ActivitySite.Kind.Jump;site.forward=launch.forward;site.radius=28;site.bronze=80;site.silver=140;site.gold=200;
                }
                EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            }
        }
        static void MountainSurface(string name,Vector3[] points,float width)
        {
            var v=new List<Vector3>();var triangles=new List<int>();
            for(int i=0;i<points.Length;i++){var forward=points[Math.Min(i+1,points.Length-1)]-points[Math.Max(0,i-1)];var side=Vector3.Cross(Vector3.up,forward).normalized*width;v.Add(points[i]-side+Vector3.up*.04f);v.Add(points[i]+side+Vector3.up*.04f);if(i+1<points.Length){int n=i*2;triangles.AddRange(new[]{n,n+2,n+1,n+1,n+2,n+3});}}
            var mesh=new Mesh{indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};mesh.SetVertices(v);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();var go=new GameObject("Ground_"+name,typeof(MeshFilter),typeof(MeshRenderer),typeof(MeshCollider));go.transform.SetParent(worldRoot);go.GetComponent<MeshFilter>().sharedMesh=MeshAsset(mesh,owner.gameObject.scene.name+"-"+name);go.GetComponent<MeshCollider>().sharedMesh=go.GetComponent<MeshFilter>().sharedMesh;go.GetComponent<Renderer>().sharedMaterial=Mat("Summit packed earth",new(.43f,.31f,.17f));
            Terrain(name,p=>{float d=Near(p,points,out var at);if(d<width+8)p.y=Mathf.Lerp(p.y,at.y-.04f,1-Smooth(width,width+8,d));return p;});
            ClearTrees(p=>Near(p,points,out _)<width+4);
        }
        public static void MountainGeometry()
        {
            for(int direction=0;direction<2;direction++)
            {
                bool reverse=direction==1;var scene=EditorSceneManager.OpenScene("Assets/Scenes/LakeWoods.unity");string path="Assets/Scenes/"+(reverse?"MountainLoopReverse":"MountainLoop")+".unity";
                if(File.Exists(path))throw new Exception("Mountain scene exists; revise locally");EditorSceneManager.SaveScene(scene,path,true);scene=EditorSceneManager.OpenScene(path);owner=Object.FindAnyObjectByType<RaceDirector>();
                worldRoot=new GameObject("CR110 mountain race geometry").transform;
                foreach(var layout in Object.FindObjectsByType<ForestLayout>())Object.DestroyImmediate(layout);
                foreach(var branch in Object.FindObjectsByType<WoodlandRoute>())Object.DestroyImmediate(branch);
                var controls=new Vector3[]{new(690,79,-95),new(747,86,-120),new(820,98,-125),new(880,113,-85),new(1020,139,-60),new(1050,151,30),new(990,164,115),new(920,156,153),new(848,130,135),new(810,116,66),new(745,90,65),new(724,79,25),new(724,79,-45),new(690,79,-95)};
                if(reverse)Array.Reverse(controls);var route=new GameObject("Mountain racing line").AddComponent<RaceRoad>();route.transform.SetParent(worldRoot);route.points=Curve(controls).SkipLast(1).ToArray();route.forestTrail=true;route.Initialize();
                MountainSurface("CR110 main supported trail",route.points.Concat(new[]{route.points[0]}).ToArray(),6);
                owner.road=route;owner.reverseCourse=reverse;owner.courseName=reverse?"Mountain Loop Reverse":"Mountain Loop";owner.courseId=reverse?"mountain-reverse-v1":"mountain-forward-v1";
                var shortcuts=new List<WoodlandRoute>();
                WoodlandRoute Branch(string title,Vector3[] points,float width){var b=new GameObject(title).AddComponent<WoodlandRoute>();b.transform.SetParent(worldRoot);b.title=title;b.points=points;b.halfWidth=width;b.recommendedSpeed=30;b.entryRoad=route.Project(points[0],out _);b.exitRoad=route.Project(points[^1],out _);b.entryInset=18;b.entryMargin=2;b.bypassedGates=Array.Empty<int>();b.aiValidated=false;shortcuts.Add(b);return b;}
                Vector3 On(Vector3 p)=>route.At(route.Project(p,out _),out _);
                var ridge=Curve(new[]{On(new(747,86,-120)),new Vector3(803,99,-90),new(870,116,-45),new(950,130,-39),On(new(1020,139,-60))});if(reverse)Array.Reverse(ridge);Branch(reverse?"Downhill Ridge Cut":"Climbing Ridge Cut",ridge,3.6f);MountainSurface("CR110 narrow ridge cut",ridge,3.6f);
                if(!reverse)
                {
                    var summit=owner.GetComponent<ExplorationCollection>().routes.Single(r=>r.name=="Summit approach and safe return");var points=summit.points.ToArray();points[0]=On(points[0]);points[^1]=On(points[^1]);Branch("Homeward Summit Flight",points,13);
                }
                else
                {
                    var launch=new GameObject("CR110 reverse summit launch").transform;launch.SetParent(worldRoot);launch.position=new(990,152,285);launch.rotation=Quaternion.LookRotation(Vector3.back);
                    Vector3 P(float s,float y)=>launch.TransformPoint(new Vector3(0,y-152,s));
                    var entry=Curve(new[]{On(new(990,164,115)),new Vector3(1035,158,178),new(1070,152,252),new(1040,152,315),new(997,152,321),P(0,152)});
                    MountainSurface("CR110 reverse entry",entry,7);
                    var runway=Enumerable.Range(0,441).Select(i=>P(i*.5f,HomewardHeight(i*.5f))).ToArray();MountainSurface("CR110 reverse runway",runway,14);
                    // Supported descending catch slope beyond the mountain, not a reversed takeoff.
                    var landing=Enumerable.Range(0,351).Select(i=>P(330+i*.5f,LandingHeight(330+i*.5f))).ToArray();MountainSurface("CR110 reverse landing",landing,24);
                    var exit=Curve(new[]{landing[^1],new Vector3(958,91,-254),new(870,93,-220),new(810,97,-174),On(new(820,98,-125))});MountainSurface("CR110 reverse return",exit,7);
                    Branch("South Face Summit Flight",entry.Concat(runway.Skip(1)).Concat(landing).Concat(exit.Skip(1)).ToArray(),13);
                    ClearTrees(p=>{var q=launch.InverseTransformPoint(p);return q.z>180&&q.z<510&&Mathf.Abs(q.x)<30;});
                }
                // Gates bracket choices; none lies inside an optional branch interval.
                var template=owner.gates[0].gameObject;var originals=owner.gates.Select(g=>g.gameObject).ToArray();var stations=new List<float>{0};foreach(var b in shortcuts){stations.Add(Mathf.Repeat(b.entryRoad-32,route.Length));stations.Add(Mathf.Repeat(b.exitRoad+32,route.Length));}
                stations=stations.Where(s=>!shortcuts.Any(b=>route.Relative(s,b.entryRoad)>0&&route.Relative(s,b.entryRoad)<route.Relative(b.exitRoad,b.entryRoad))).OrderBy(s=>s).ToList();
                var gates=new List<RaceGate>();foreach(float s in stations){var go=Object.Instantiate(template,worldRoot);var g=go.GetComponent<RaceGate>();go.name=gates.Count==0?owner.courseName+" START FINISH":owner.courseName+" CP "+gates.Count;var p=route.At(s,out var f);go.transform.SetPositionAndRotation(p+Vector3.up*1.6f,Quaternion.LookRotation(Vector3.ProjectOnPlane(f,Vector3.up)));g.halfWidth=6;g.halfHeight=4;gates.Add(g);}foreach(var go in originals)Object.DestroyImmediate(go);owner.gates=gates.ToArray();
                var spawn=owner.vehicle.GetComponent<VehicleRespawn>().spawnPoint;spawn.position=route.At(route.Length-30,out var heading)+Vector3.up*.7f;spawn.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(heading,Vector3.up));owner.vehicle.transform.SetPositionAndRotation(spawn.position,spawn.rotation);
                // Old race signs are not direction guidance for the new mountain circuits.
                foreach(var sign in Object.FindObjectsByType<PhysicalSign>())if(sign.name.StartsWith("CR")&&sign.transform.position.x>680)sign.gameObject.SetActive(false);
                EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
                File.WriteAllLines(CR105Authoring.Evidence+"/geometry-"+scene.name+".txt",new[]{owner.courseId+" length="+route.Length+" gates="+gates.Count}.Concat(shortcuts.Select(b=>b.title+" entry="+b.entryRoad+" exit="+b.exitRoad+" length="+b.Length)));
            }
        }
    }
}
