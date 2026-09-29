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
    // Local, reproducible authoring for the two new scenes. Source-world meshes are cloned.
    public static class BackyardAuthoring
    {
        const string Folder="Assets/Track/Backyard", Evidence="Docs/Backyard";
        static Transform root; static RaceDirector race; static RaceRoad street;
        static readonly List<Strip> strips=new();
        sealed class Strip { public string name;public Vector3[] points;public float width;public bool paved;public Strip(string n,Vector3[] p,float w,bool concrete=false){name=n;points=p;width=w;paved=concrete;} }
        static float Smooth(float a,float b,float x)=>Mathf.SmoothStep(0,1,Mathf.InverseLerp(a,b,x));
        static float Ground(Vector3 p)=>Physics.RaycastAll(new Vector3(p.x,400,p.z),Vector3.down,800,1,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name.StartsWith("Ground_")||h.collider.name=="Decorative Road pavement").OrderBy(h=>Math.Abs(h.point.y-p.y)).First().point.y;
        static Mesh Store(Mesh mesh,string name){string path=Folder+"/"+name+".asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(old){EditorUtility.CopySerialized(mesh,old);Object.DestroyImmediate(mesh);return old;}AssetDatabase.CreateAsset(mesh,path);return mesh;}
        static Material Mat(string name,Color color){string path=Folder+"/"+name+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);if(!mat){mat=new Material(Shader.Find("Universal Render Pipeline/Lit")){color=color,enableInstancing=true};mat.SetFloat("_Smoothness",.1f);AssetDatabase.CreateAsset(mat,path);}return mat;}
        static float Near(Vector3 p,Vector3[] line,out Vector3 at){float best=float.MaxValue;at=p;for(int i=1;i<line.Length;i++){var a=line[i-1];var v=line[i]-a;var d=p-a;v.y=d.y=0;float t=Mathf.Clamp01(Vector3.Dot(v,d)/Mathf.Max(.001f,v.sqrMagnitude));float q=(d-v*t).sqrMagnitude;if(q<best){best=q;at=Vector3.Lerp(a,line[i],t);}}return Mathf.Sqrt(best);}
        static Vector3[] Curve(params Vector3[] knots){var points=new List<Vector3>();for(int i=0;i<knots.Length-1;i++){var a=knots[Math.Max(0,i-1)];var b=knots[i];var c=knots[i+1];var d=knots[Math.Min(knots.Length-1,i+2)];int count=Math.Max(2,Mathf.CeilToInt(Vector3.Distance(b,c)));for(int j=0;j<count;j++){float t=j/(float)count;points.Add(.5f*(2*b+(-a+c)*t+(2*a-5*b+4*c-d)*t*t+(-a+3*b-3*c+d)*t*t*t));}}points.Add(knots[^1]);return points.ToArray();}
        static Vector3[] Line(Vector3 a,Vector3 b,float step=.5f){int n=Mathf.CeilToInt(Vector3.Distance(a,b)/step);return Enumerable.Range(0,n+1).Select(i=>Vector3.Lerp(a,b,i/(float)n)).ToArray();}
        static void Surface(Strip strip,string scene){var v=new List<Vector3>();var t=new List<int>();for(int i=0;i<strip.points.Length;i++){var p=strip.points[i];var f=strip.points[Math.Min(i+1,strip.points.Length-1)]-strip.points[Math.Max(0,i-1)];var right=Vector3.Cross(Vector3.up,f).normalized;v.Add(p-right*strip.width);v.Add(p+right*strip.width);if(i<strip.points.Length-1){int n=i*2;t.AddRange(new[]{n,n+2,n+1,n+1,n+2,n+3});}}var mesh=new Mesh{indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};mesh.SetVertices(v);mesh.SetTriangles(t,0);mesh.RecalculateNormals();mesh.RecalculateBounds();var go=new GameObject("Ground_Backyard "+strip.name,typeof(MeshFilter),typeof(MeshRenderer),typeof(MeshCollider));go.transform.SetParent(root);go.GetComponent<MeshFilter>().sharedMesh=Store(mesh,scene+"-"+strip.name);go.GetComponent<MeshCollider>().sharedMesh=go.GetComponent<MeshFilter>().sharedMesh;go.GetComponent<Renderer>().sharedMaterial=strip.paved?AssetDatabase.LoadAssetAtPath<Material>("Assets/Track/FiveUpdates/Beige concrete.mat"):Mat("Packed woodland dirt",new(.36f,.245f,.12f));}
        static float PropertyHeight(float x){if(x>=422.7f)return Mathf.Lerp(82.80f,79.73f,Mathf.InverseLerp(422.7f,463.6f,x));if(x>=409.5f)return Mathf.Lerp(79.46f,82.8f,Smooth(409.5f,422.7f,x));return 79.46f;}
        static void Property(string scene){foreach(var mf in Object.FindObjectsByType<MeshFilter>().Where(m=>m.name=="Ground_Dan beige concrete descent and parking")){var v=mf.sharedMesh.vertices;for(int i=0;i<v.Length;i++){var p=mf.transform.TransformPoint(v[i]);if(p.x<388||p.x>463.6f||Math.Abs(p.z-9.1f)>7)continue;float blend=1-Smooth(3.2f,7,Math.Abs(p.z-9.1f));p.y=Mathf.Lerp(p.y,PropertyHeight(p.x),blend);v[i]=mf.transform.InverseTransformPoint(p);}var mesh=Object.Instantiate(mf.sharedMesh);mesh.vertices=v;mesh.RecalculateNormals();mesh.RecalculateBounds();mf.sharedMesh=Store(mesh,scene+"-property-concrete");mf.GetComponent<MeshCollider>().sharedMesh=mf.sharedMesh;}}
        static void Landscape(string scene,bool onlyStrips=false){
            var bounded=strips.Select(s=>(s,b:new Bounds((s.points.Aggregate(Vector3.Min)+s.points.Aggregate(Vector3.Max))*.5f,s.points.Aggregate(Vector3.Max)-s.points.Aggregate(Vector3.Min)+new Vector3((s.width+10)*2,1000,(s.width+10)*2)))).ToArray();
            foreach(var mf in GameObject.Find("Memory loop - north is +Z").GetComponentsInChildren<MeshFilter>()){
                var verts=mf.sharedMesh.vertices;bool changed=false;
                for(int i=0;i<verts.Length;i++){var p=mf.transform.TransformPoint(verts[i]);var original=p;float nearest=float.MaxValue;Strip chosen=null;Vector3 on=p;
                    foreach(var item in bounded){if(!item.b.Contains(p))continue;float d=Near(p,item.s.points,out var q);if(d<nearest){nearest=d;chosen=item.s;on=q;}}
                    if(chosen!=null&&nearest<chosen.width+9&&p.x<458)p.y=Mathf.Lerp(p.y,on.y-.06f,1-Smooth(chosen.width,chosen.width+9,nearest));
                    if(!onlyStrips&&p.x>=388&&p.x<458&&Math.Abs(p.z-9.1f)<9)p.y=Mathf.Lerp(p.y,PropertyHeight(p.x)-.08f,1-Smooth(3,9,Math.Abs(p.z-9.1f)));
                    // Shallow drivable failure basin beneath the dump flight. Reverse uses its own wooded line.
                    if(!onlyStrips&&p.x>260&&p.x<332&&Math.Abs(p.z-12)<17){float b=Smooth(260,272,p.x)*(1-Smooth(325,332,p.x))*(1-Smooth(9,17,Math.Abs(p.z-12)));p.y=Mathf.Lerp(p.y,74.8f,b);}
                    if((p-original).sqrMagnitude>.000001f){verts[i]=mf.transform.InverseTransformPoint(p);changed=true;}
                }
                if(!changed)continue;var mesh=Object.Instantiate(mf.sharedMesh);mesh.vertices=verts;mesh.RecalculateNormals();mesh.RecalculateBounds();mf.sharedMesh=Store(mesh,scene+"-terrain-"+mf.name);mf.GetComponent<MeshCollider>().sharedMesh=mf.sharedMesh;
            }
            Physics.SyncTransforms();
        }
        static void ClearTrees(string scene){
            bool InTrail(Vector3 p)=>p.x<455&&strips.Any(s=>Near(p,s.points,out _)<s.width+2.4f)||(p.x>245&&p.x<335&&Math.Abs(p.z-12)<14)||(p.x>=388&&p.x<455&&Math.Abs(p.z-9.1f)<3.2f);
            foreach(var c in Object.FindObjectsByType<Collider>().Where(c=>c.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0||c.name.IndexOf("tree",StringComparison.OrdinalIgnoreCase)>=0).ToArray())if(InTrail(c.bounds.center))Object.DestroyImmediate(c.gameObject);
            int index=0;foreach(var mf in Object.FindObjectsByType<MeshFilter>().Where(f=>f.GetComponentsInParent<Transform>().Any(t=>t.name.Contains("woods")||t.name.Contains("Woods")||t.name.Contains("Forest tree"))).ToArray()){
                var v=mf.sharedMesh.vertices;var t=mf.sharedMesh.triangles;var keep=new List<int>();for(int i=0;i<t.Length;i+=3)if(!InTrail(mf.transform.TransformPoint((v[t[i]]+v[t[i+1]]+v[t[i+2]])/3)))keep.AddRange(new[]{t[i],t[i+1],t[i+2]});if(keep.Count==t.Length)continue;var m=Object.Instantiate(mf.sharedMesh);m.SetTriangles(keep,0);m.RecalculateBounds();mf.sharedMesh=Store(m,scene+"-trees-"+index++);
            }
        }
        static void Arrow(Vector3 p,Vector3 f,bool optional,string scene,int index){f=Vector3.ProjectOnPlane(f,Vector3.up).normalized;var side=Vector3.Cross(Vector3.up,f);float w=optional?.85f:1.2f;var shape=new[]{new Vector2(-w*.4f,-2),new Vector2(w*.4f,-2),new Vector2(w*.4f,0),new Vector2(w,0),new Vector2(0,2),new Vector2(-w,0),new Vector2(-w*.4f,0)};var v=shape.Select(q=>p+side*q.x+f*q.y).ToArray();for(int i=0;i<v.Length;i++)v[i].y=Ground(v[i])+.075f;var m=new Mesh{vertices=v,triangles=new[]{0,6,1,1,6,2,6,5,4,6,4,2,2,4,3}};m.RecalculateNormals();var g=new GameObject(optional?"Optional gold arrow":"Main teal arrow",typeof(MeshFilter),typeof(MeshRenderer));g.transform.SetParent(root);g.GetComponent<MeshFilter>().sharedMesh=Store(m,scene+"-arrow-"+index);g.GetComponent<Renderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Track/Discovery/CR117 "+(optional?"alternate gold":"main teal")+".mat");}
        static void Sign(Vector3 p,Vector3 heading,string text){var parent=new GameObject(text.Replace('\n',' ')).transform;parent.SetParent(root);parent.position=p;parent.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(heading,Vector3.up));var wood=Mat("Weathered trail sign",new(.19f,.12f,.055f));foreach(var part in new[]{("Post",new Vector3(0,1.2f,0),new Vector3(.18f,2.4f,.18f)),("Board",new Vector3(0,2.15f,0),new Vector3(5.5f,1.35f,.16f))}){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=part.Item1;g.transform.SetParent(parent,false);g.transform.localPosition=part.Item2;g.transform.localScale=part.Item3;g.GetComponent<Renderer>().sharedMaterial=wood;Object.DestroyImmediate(g.GetComponent<Collider>());}var label=new GameObject("Physical trail lettering").AddComponent<TextMesh>();label.transform.SetParent(parent,false);label.transform.localPosition=new(0,2.15f,-.09f);label.text=text;label.fontSize=64;label.characterSize=.085f;label.anchor=TextAnchor.MiddleCenter;label.alignment=TextAlignment.Center;label.color=new(1,.94f,.75f);parent.gameObject.AddComponent<PhysicalSign>();}
        public static void Apply(){
            if(Application.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Saved edit mode required");
            Directory.CreateDirectory(Folder);Directory.CreateDirectory(Evidence);AssetDatabase.Refresh();
            foreach(bool reverse in new[]{false,true}){
                string name=reverse?"DansBackyardReverse":"DansBackyard",path="Assets/Scenes/"+name+".unity";
                if(File.Exists(path))throw new Exception("New scene already exists; use a local correction rather than regenerating");
                var scene=EditorSceneManager.OpenScene("Assets/Scenes/StreetLoopGreybox.unity");EditorSceneManager.SaveScene(scene,path,true);scene=EditorSceneManager.OpenScene(path);
                race=Object.FindAnyObjectByType<RaceDirector>();street=race.road;street.Initialize();strips.Clear();root=new GameObject("Dan's Backyard authored course").transform;
                foreach(var b in Object.FindObjectsByType<WoodlandRoute>())Object.DestroyImmediate(b); // Unchanged old physical shortcuts remain exploration scenery, not active race entitlements.
                foreach(var f in Object.FindObjectsByType<ForestLayout>())Object.DestroyImmediate(f);
                foreach(var f in Object.FindObjectsByType<MountainFlights>())Object.DestroyImmediate(f);
                var oldGuidance=GameObject.Find("Route atlas direction guidance");if(oldGuidance)oldGuidance.SetActive(false);
                foreach(var mf in Object.FindObjectsByType<MeshFilter>().Where(m=>m.name.Contains("arrow")&&m.GetComponent<Renderer>()&&!m.GetComponent<Collider>()))mf.gameObject.SetActive(false);
                Property(name);Physics.SyncTransforms();
                var property=Line(new(463.6f,79.73f,8),new(391.9f,79.46f,9.7f)).Select(p=>new Vector3(p.x,PropertyHeight(p.x),p.z)).ToArray();
                var dirt=Curve(property[^1],new(375,78.1f,11),new(362,75.1f,13),new(355.4f,74.9f,13.56f));
                var launch=Line(dirt[^1],new(331.4f,78.5f,16.2f));for(int i=0;i<launch.Length;i++){float t=i/(float)(launch.Length-1);launch[i].y=74.9f+3.6f*t*t;}
                var landing=Curve(new(280,76.65f,10.55f),new(258.6f,76.6f,8.2f),new(240,72,2));
                var bypass=Curve(dirt[0],new(370,78, -11),new(344,76,-26),new(307,73,-28),new(276,74,-18),new(250,75,-7),landing[^1]);
                var forest=Curve(landing[^1],new(220,65,-25),new(202,57,-57),new(178,49,-78),new(142,44,-85),new(119,40,-103),new(111,38.6f,-108.6f),new(99,40,-108.6f));
                var gullyWest=Curve(new(94.8f,38.6f,-108.6f),new(80,38.2f,-109),new(69,39,-96),new(78,40.2f,-79),new(96.1f,40.5f,-83),new(112,41,-81));
                var homeward=Curve(gullyWest[^1],new(140,46,-47),new(180,53,-20),new(208,62,26),new(235,70,62),new(275,73,100),new(326,79,116),new(367,83,88),new(402,85,111),new(430,83,87),new(456.4f,81.34f,67.4f));
                var basin=Curve(new(96.1f,38,-128),new(96.1f,35.1f,-116),new(96.1f,35.1f,-104),new(96.1f,38,-91),new(96.1f,40.3f,-83),new(96.1f,40.5f,-75));
                strips.Add(new("wide dirt approach",dirt,5));strips.Add(new("dump launch",launch,5));strips.Add(new("dump landing",landing,7));strips.Add(new("reverse wooded dump bypass",bypass,4.1f));strips.Add(new("forest descent",forest,4.2f));strips.Add(new("gully right turn and shallow crossing",gullyWest,4.2f));strips.Add(new("winding forest return",homeward,4.2f));strips.Add(new("gully escape floor",basin,3));
                var dumpFloor=Line(new(325,74.8f,16),new(266,74.8f,9));strips.Add(new("dump recoverable floor",dumpFloor,10));
                var dumpExit=Curve(dumpFloor[^1],new(252,75.1f,15),new(241,72.2f,3),landing[^1]);strips.Add(new("dump gradual exit",dumpExit,4));
                // The shorter narrow ridge omits the wide northern switchback; legal branch metadata is directional.
                var cut=Curve(new(235,70,62),new(257,76,67),new(282,80,62),new(310,81,74),new(340,82,80),new(367,83,88));strips.Add(new("optional narrow ridge",cut,2.3f));
                Landscape(name);ClearTrees(name);foreach(var s in strips)Surface(s,name);Physics.SyncTransforms();
                float entry=street.Project(homeward[^1],out _),exit=street.Project(property[0],out _);var roadJoin=street.At(entry,out _);var roadLeave=street.At(exit,out _);
                var join=Line(homeward[^1],roadJoin).Select(p=>new Vector3(p.x,Ground(p)+.025f,p.z)).ToArray();var leave=Line(roadLeave,property[0]).Select(p=>new Vector3(p.x,Ground(p)+.025f,p.z)).ToArray();
                // Stop the new dirt at the road edge; existing asphalt carries the shared return.
                var joinVisual=join.Where(p=>{float station=street.Project(p,out float d);return d>street.HalfWidth(station)+.05f;}).ToArray();if(joinVisual.Length>1)Surface(new Strip("South Cherokee trail mouth",joinVisual,3),name);
                var asphalt=new List<Vector3>();float distance=street.Relative(exit,entry);if(distance>street.Length*.5f){distance=street.Relative(entry,exit);for(float s=0;s<distance;s+=1)asphalt.Add(street.At(entry-s,out _));}else for(float s=0;s<distance;s+=1)asphalt.Add(street.At(entry+s,out _));asphalt.Add(roadLeave);
                var points=new List<Vector3>();void Add(IEnumerable<Vector3> ps){foreach(var p in ps)if(points.Count==0||Vector3.Distance(points[^1],p)>.02f)points.Add(p);}
                Add(property);if(!reverse){Add(dirt);Add(launch);Add(Line(launch[^1],landing[0],1));Add(landing);}else Add(bypass);
                Add(forest);Add(Line(forest[^1],gullyWest[0],.4f));Add(gullyWest);Add(homeward);Add(join);Add(asphalt);Add(leave);if(Vector3.Distance(points[^1],points[0])<.1f)points.RemoveAt(points.Count-1);
                if(reverse)points=new[]{points[0]}.Concat(points.Skip(1).Reverse()).ToList();
                var road=new GameObject("Dan's Backyard main racing line").AddComponent<RaceRoad>();road.transform.SetParent(root);road.points=points.ToArray();road.forestTrail=true;road.Initialize();race.ambientRoad=street;race.road=road;race.reverseCourse=reverse;race.courseId=reverse?"backyard-reverse-v1":"backyard-forward-v1";race.courseName=reverse?"Dan's Backyard - Reverse":"Dan's Backyard - Forward";
                var branch=new GameObject("Optional ridge cut").AddComponent<WoodlandRoute>();branch.transform.SetParent(root);branch.title=reverse?"Narrow Ridge Descent":"Narrow Ridge Climb";branch.points=reverse?cut.Reverse().ToArray():cut;branch.entryRoad=road.Project(branch.points[0],out _);branch.exitRoad=road.Project(branch.points[^1],out _);branch.halfWidth=2.3f;branch.recommendedSpeed=24;branch.entrySpeed=18;branch.entrySpeedDistance=25;branch.entryInset=14;branch.entryMargin=1;branch.aiValidated=true;
                var template=race.gates[0].gameObject;var originals=race.gates.Select(g=>g.gameObject).ToArray();var stations=new List<float>{0};for(float s=95;s<road.Length-65;s+=110)stations.Add(s);var gates=new List<RaceGate>();foreach(float s in stations){var go=Object.Instantiate(template,root);var gate=go.GetComponent<RaceGate>();var p=road.At(s,out var f);go.name=gates.Count==0?"Dan's Backyard START FINISH":"Dan's Backyard CP "+gates.Count;go.transform.SetPositionAndRotation(p+Vector3.up*1.6f,Quaternion.LookRotation(Vector3.ProjectOnPlane(f,Vector3.up)));gate.halfWidth=7;gate.halfHeight=5;gate.upperHeight=18;foreach(var label in go.GetComponentsInChildren<TextMesh>())label.text=gates.Count==0?"START / FINISH":"CP "+gates.Count;gates.Add(gate);}foreach(var go in originals)Object.DestroyImmediate(go);race.gates=gates.ToArray();branch.bypassedGates=stations.Select((s,i)=>(s,i)).Where(q=>q.i>0&&road.Relative(q.s,branch.entryRoad)<road.Relative(branch.exitRoad,branch.entryRoad)).Select(q=>q.i).ToArray();
                race.requireOrderedGates=true;
                var spawn=race.vehicle.GetComponent<VehicleRespawn>().spawnPoint;spawn.position=road.At(road.Length-18,out var heading)+Vector3.up*.65f;spawn.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(heading,Vector3.up));race.vehicle.transform.SetPositionAndRotation(spawn.position,spawn.rotation);
                var flight=race.gameObject.AddComponent<MountainFlights>();if(!reverse)flight.flights=new[]{new MountainFlights.Flight{name="Backyard dump flight",start=dirt[0],lip=launch[^1],landingEnd=landing[^1],forward=Vector3.ProjectOnPlane(launch[^1]-launch[0],Vector3.up).normalized,approachStation=road.Project(dirt[0],out _),endStation=road.Project(landing[^1],out _)}};
                foreach(var feature in new[]{(launch[^1]-Vector3.left*3,launch[^1]+Vector3.left*8,7f),(forest[^1]+Vector3.right*2,gullyWest[0],6f)}){var ex=new GameObject("Jump lip recovery exclusion").AddComponent<JumpRecoveryExclusion>();ex.transform.SetParent(root);ex.start=feature.Item1;ex.end=feature.Item2;ex.halfWidth=feature.Item3;}
                int index=0;for(float s=28;s<road.Length-15;s+=35){var p=road.At(s,out var f);if(Math.Abs(Ground(p)-p.y)<1.1f)Arrow(p,f,false,name,index++);}for(float s=18;s<branch.Length-5;s+=30)Arrow(branch.At(s,out var f),f,true,name,index++);
                Sign(property[0]+Vector3.forward*6,reverse?Vector3.right:Vector3.left,"DAN'S BACKYARD\n"+(reverse?"REVERSE":"FORWARD"));var fork=road.At(branch.entryRoad-13,out var forkHeading);Sign(fork+Vector3.Cross(Vector3.up,forkHeading).normalized*6,forkHeading,"OPTIONAL RIDGE\nNARROW / TECHNICAL");if(!reverse)Sign(launch[0]+Vector3.forward*7,Vector3.left,"DUMP JUMP\nSTRAIGHT / KEEP SPEED");
                var junk=Mat("Old dump scrap",new(.29f,.22f,.17f));for(int i=0;i<12;i++){var g=GameObject.CreatePrimitive(i%3==0?PrimitiveType.Cylinder:PrimitiveType.Cube);g.name="Small discarded scrap pile";g.transform.SetParent(root);g.transform.position=new(279+i*3.5f,75.15f,23+(i%2)*1.3f);g.transform.localScale=new(1.2f,.6f,.9f);g.transform.rotation=Quaternion.Euler(0,i*47,12);g.GetComponent<Renderer>().sharedMaterial=junk;Object.DestroyImmediate(g.GetComponent<Collider>());}
                EditorUtility.SetDirty(race);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
                File.WriteAllText(Evidence+"/"+name+"-geometry.json",JsonUtility.ToJson(new Export{scene=name,main=road.points,shortcut=branch.points,gates=gates.Select(g=>g.transform.position).ToArray(),surfaces=strips.Select(s=>new ExportStrip{name=s.name,points=s.points,width=s.width}).ToArray()},true));
            }
            EditorBuildSettings.scenes=EditorBuildSettings.scenes.Concat(new[]{new EditorBuildSettingsScene("Assets/Scenes/DansBackyard.unity",true),new EditorBuildSettingsScene("Assets/Scenes/DansBackyardReverse.unity",true)}).GroupBy(s=>s.path).Select(g=>g.First()).ToArray();
            File.WriteAllText(Evidence+"/authoring-done.txt","Both new course scenes authored; physics and existing course meshes unchanged except the separately authorized Kyle fix. Final targeted tests pending.");
        }
        [Serializable] public class Export{public string scene;public Vector3[] main,shortcut,gates;public ExportStrip[] surfaces;}
        [Serializable] public class ExportStrip{public string name;public Vector3[] points;public float width;}
        public static void AlignDump(){
            foreach(string name in new[]{"DansBackyard","DansBackyardReverse"}){
                var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+name+".unity");race=Object.FindAnyObjectByType<RaceDirector>();street=race.ambientRoad;root=GameObject.Find("Dan's Backyard authored course").transform;
                var data=JsonUtility.FromJson<Export>(File.ReadAllText(Evidence+"/"+name+"-geometry.json"));strips.Clear();strips.AddRange(data.surfaces.Select(s=>new Strip(s.name,s.points,s.width)));
                var dirt=strips.Single(s=>s.name=="wide dirt approach");var launch=strips.Single(s=>s.name=="dump launch");var oldDirt=dirt.points;var oldLaunch=launch.points;
                dirt.points=Curve(new(391.9f,79.46f,9.7f),new(382,76.5f,21.8f),new(370,74.9f,20.44f),new(355.4f,74.9f,18.84f));launch.points=Line(dirt.points[^1],new(331.4f,78.5f,16.2f));for(int i=0;i<launch.points.Length;i++){float t=i/(float)(launch.points.Length-1);launch.points[i].y=74.9f+3.6f*t*t;}
                foreach(var strip in new[]{dirt,launch}){var go=GameObject.Find("Ground_Backyard "+strip.name);Object.DestroyImmediate(go);Surface(strip,name);}
                if(name=="DansBackyard"){
                    var road=race.road;float a=road.Project(oldDirt[0],out _),b=road.Project(oldLaunch[^1],out _);var before=new List<Vector3>();for(float s=0;s<a-.3f;s+=.8f)before.Add(road.At(s,out _));before.AddRange(dirt.points);before.AddRange(launch.points.Skip(1));for(float s=b+.5f;s<road.Length;s+=.8f)before.Add(road.At(s,out _));road.points=before.ToArray();typeof(RaceRoad).GetField("distance",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).SetValue(road,null);road.Initialize();
                    var flight=race.GetComponent<MountainFlights>().flights[0];flight.start=new(375,75,20.99f);flight.forward=Vector3.ProjectOnPlane(launch.points[^1]-launch.points[0],Vector3.up).normalized;flight.approachStation=road.Project(flight.start,out _);flight.endStation=road.Project(flight.landingEnd,out _);
                    foreach(var branch in Object.FindObjectsByType<WoodlandRoute>()){branch.entryRoad=road.Project(branch.points[0],out _);branch.exitRoad=road.Project(branch.points[^1],out _);branch.bypassedGates=race.gates.Select((g,i)=>(g,i)).Where(x=>x.i>0&&road.Relative(road.Project(x.g.transform.position,out _),branch.entryRoad)<road.Relative(branch.exitRoad,branch.entryRoad)).Select(x=>x.i).ToArray();}
                    data.main=road.points;
                }
                Landscape(name);ClearTrees(name);data.surfaces=strips.Select(s=>new ExportStrip{name=s.name,points=s.points,width=s.width}).ToArray();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();File.WriteAllText(Evidence+"/"+name+"-geometry.json",JsonUtility.ToJson(data,true));
            }
            File.WriteAllText(Evidence+"/dump-alignment-done.txt","Dump approach aligned to supplied landing X/Z; no physics changes.");
        }
        public static void FinalGuidance(){foreach(string name in new[]{"DansBackyard","DansBackyardReverse"}){
            var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+name+".unity");race=Object.FindAnyObjectByType<RaceDirector>();race.requireOrderedGates=true;root=GameObject.Find("Dan's Backyard authored course").transform;var road=race.road;road.Initialize();
            foreach(var gate in race.gates){float s=road.Project(gate.transform.position,out _);var p=road.At(s,out var f);gate.transform.SetPositionAndRotation(p+Vector3.up*1.6f,Quaternion.LookRotation(Vector3.ProjectOnPlane(f,Vector3.up)));}
            foreach(var t in root.Cast<Transform>().Where(t=>t.name=="Main teal arrow"||t.name=="Optional gold arrow").ToArray())Object.DestroyImmediate(t.gameObject);
            Physics.SyncTransforms();int index=0;for(float s=28;s<road.Length-15;s+=35){var p=road.At(s,out var f);if(Math.Abs(Ground(p)-p.y)<1.1f)Arrow(p,f,false,name,index++);}foreach(var b in Object.FindObjectsByType<WoodlandRoute>())for(float s=18;s<b.Length-5;s+=30)Arrow(b.At(s,out var f),f,true,name,index++);
            var sign=root.Cast<Transform>().FirstOrDefault(t=>t.name.StartsWith("DUMP JUMP"));if(sign)sign.position=new(355.4f,74.9f,25.84f);
            // Open only the panels obstructing the explicitly requested transition behind the property.
            foreach(var c in Object.FindObjectsByType<Collider>().Where(c=>c.name=="Grounded wood crossbuck"&&c.bounds.center.x>386&&c.bounds.center.x<393&&c.bounds.center.z>3&&c.bounds.center.z<17).ToArray())Object.DestroyImmediate(c.gameObject);
            foreach(var c in Object.FindObjectsByType<Collider>().Where(c=>(c.name.Contains("trunk")||c.name.Contains("Tree"))&&c.bounds.center.x>391&&c.bounds.center.x<455&&Math.Abs(c.bounds.center.z-9.1f)<3.2f).ToArray())Object.DestroyImmediate(c.gameObject);
            int treeIndex=0;foreach(var mf in Object.FindObjectsByType<MeshFilter>().Where(f=>f.GetComponentsInParent<Transform>().Any(t=>t.name.Contains("woods")||t.name.Contains("Woods")||t.name.Contains("Forest tree")))){var v=mf.sharedMesh.vertices;var t=mf.sharedMesh.triangles;var keep=new List<int>();for(int i=0;i<t.Length;i+=3){var p=mf.transform.TransformPoint((v[t[i]]+v[t[i+1]]+v[t[i+2]])/3);if(!(p.x>391&&p.x<455&&Math.Abs(p.z-9.1f)<3.2f))keep.AddRange(new[]{t[i],t[i+1],t[i+2]});}if(keep.Count==t.Length)continue;var m=Object.Instantiate(mf.sharedMesh);m.SetTriangles(keep,0);m.RecalculateBounds();mf.sharedMesh=Store(m,name+"-property-tree-clearance-"+treeIndex++);}
            var data=JsonUtility.FromJson<Export>(File.ReadAllText(Evidence+"/"+name+"-geometry.json"));data.gates=race.gates.Select(g=>g.transform.position).ToArray();File.WriteAllText(Evidence+"/"+name+"-geometry.json",JsonUtility.ToJson(data,true));EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        }File.WriteAllText(Evidence+"/guidance-done.txt","Final gates and collider-free arrows follow the saved main route.");}
        public static void LandingRunout(){
            foreach(string name in new[]{"DansBackyard","DansBackyardReverse"}){
                var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+name+".unity");race=Object.FindAnyObjectByType<RaceDirector>();root=GameObject.Find("Dan's Backyard authored course").transform;var road=race.road;road.Initialize();
                var data=JsonUtility.FromJson<Export>(File.ReadAllText(Evidence+"/"+name+"-geometry.json"));var saved=data.surfaces.Select(s=>new Strip(s.name,s.points,s.width)).ToList();
                var landing=saved.Single(s=>s.name=="dump landing");var forest=saved.Single(s=>s.name=="forest descent");var bypass=saved.Single(s=>s.name=="reverse wooded dump bypass");var escape=saved.Single(s=>s.name=="dump gradual exit");
                float a=road.Project(name=="DansBackyard"?landing.points[0]:new Vector3(142,44,-85),out _),b=road.Project(name=="DansBackyard"?new Vector3(142,44,-85):new Vector3(250,75,-7),out _);
                landing.points=Curve(new(280,76.65f,10.55f),new(258.6f,76.6f,8.2f),new(240,76.5f,6.2f),new(205,75.5f,2.3f));
                forest.points=Curve(landing.points[^1],new(185,72,-8),new(164,63,-28),new(151,54,-53),new(142,44,-85),new(119,40,-103),new(111,38.6f,-108.6f),new(99,40,-108.6f));
                bypass.points=Curve(new(391.9f,79.46f,9.7f),new(370,78,-11),new(344,76,-26),new(307,73,-28),new(276,74,-18),new(250,75,-7),landing.points[^1]);
                escape.points=Curve(new(266,74.8f,9),new(252,75.1f,15),landing.points[^1]);
                var approach=forest.points.TakeWhile(p=>p.x>142).Append(new Vector3(142,44,-85)).ToArray();
                var section=name=="DansBackyard"?landing.points.Concat(approach.Skip(1)).ToArray():approach.Reverse().Concat(bypass.points.Reverse().Skip(1).TakeWhile(p=>p.x<250)).Append(new Vector3(250,75,-7)).ToArray();
                var ps=new List<Vector3>();for(float s=0;s<a-.3f;s+=.8f)ps.Add(road.At(s,out _));ps.AddRange(section);for(float s=b+.5f;s<road.Length;s+=.8f)ps.Add(road.At(s,out _));road.points=ps.ToArray();typeof(RaceRoad).GetField("distance",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).SetValue(road,null);road.Initialize();
                strips.Clear();strips.AddRange(new[]{landing,forest,bypass,escape});Landscape(name,true);ClearTrees(name);foreach(var strip in strips){Object.DestroyImmediate(GameObject.Find("Ground_Backyard "+strip.name));Surface(strip,name);}
                foreach(var branch in Object.FindObjectsByType<WoodlandRoute>()){branch.entryRoad=road.Project(branch.points[0],out _);branch.exitRoad=road.Project(branch.points[^1],out _);}
                if(name=="DansBackyard"){var flight=race.GetComponent<MountainFlights>().flights[0];flight.endStation=road.Project(flight.landingEnd,out _);}
                data.main=road.points;data.surfaces=saved.Select(s=>new ExportStrip{name=s.name,points=s.points,width=s.width}).ToArray();File.WriteAllText(Evidence+"/"+name+"-geometry.json",JsonUtility.ToJson(data,true));EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            }
            FinalGuidance();File.WriteAllText(Evidence+"/runout-done.txt","Flattened far landing into a gradual forest turn; supplied landing and gully X/Z preserved.");
        }
        public static void SeparateReturn(){
            foreach(string name in new[]{"DansBackyard","DansBackyardReverse"}){
                var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+name+".unity");race=Object.FindAnyObjectByType<RaceDirector>();root=GameObject.Find("Dan's Backyard authored course").transform;var road=race.road;road.Initialize();var data=JsonUtility.FromJson<Export>(File.ReadAllText(Evidence+"/"+name+"-geometry.json"));var saved=data.surfaces.Select(s=>new Strip(s.name,s.points,s.width)).ToList();var home=saved.Single(s=>s.name=="winding forest return");
                var end=new Vector3(235,70,62);float a=road.Project(name=="DansBackyard"?home.points[0]:end,out _),b=road.Project(name=="DansBackyard"?end:home.points[0],out _);var fresh=Curve(home.points[0],new(116,43,-58),new(120,46,-35),new(136,51,0),new(165,59,32),new(200,67,48),end);int cut=Enumerable.Range(0,home.points.Length).OrderBy(i=>(home.points[i]-end).sqrMagnitude).First();home.points=fresh.Concat(home.points.Skip(cut+1)).ToArray();
                var ps=new List<Vector3>();for(float s=0;s<a-.3f;s+=.8f)ps.Add(road.At(s,out _));ps.AddRange(name=="DansBackyard"?fresh:fresh.Reverse());for(float s=b+.5f;s<road.Length;s+=.8f)ps.Add(road.At(s,out _));road.points=ps.ToArray();typeof(RaceRoad).GetField("distance",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).SetValue(road,null);road.Initialize();
                strips.Clear();strips.AddRange(saved);Landscape(name,true);ClearTrees(name);Object.DestroyImmediate(GameObject.Find("Ground_Backyard "+home.name));Surface(home,name);foreach(var branch in Object.FindObjectsByType<WoodlandRoute>()){branch.entryRoad=road.Project(branch.points[0],out _);branch.exitRoad=road.Project(branch.points[^1],out _);}
                if(name=="DansBackyard"){var f=race.GetComponent<MountainFlights>().flights[0];f.approachStation=road.Project(f.start,out _);f.endStation=road.Project(f.landingEnd,out _);}
                data.main=road.points;data.surfaces=saved.Select(s=>new ExportStrip{name=s.name,points=s.points,width=s.width}).ToArray();File.WriteAllText(Evidence+"/"+name+"-geometry.json",JsonUtility.ToJson(data,true));EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            }
            FinalGuidance();File.WriteAllText(Evidence+"/return-separation-done.txt","Return trail separated northwest of the new forest descent; all local trail surfaces considered in terrain blending.");
        }
        public static void ReverseGully(){
            const string name="DansBackyardReverse";var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+name+".unity");race=Object.FindAnyObjectByType<RaceDirector>();root=GameObject.Find("Dan's Backyard authored course").transform;var road=race.road;road.Initialize();if(GameObject.Find("Ground_Backyard reverse gully grounded crossing"))throw new Exception("Already authored");
            float entry=road.Project(new(80,38.2f,-109),out _),exit=road.Project(new(111,38.6f,-108.6f),out _);var a=road.At(entry,out _);var b=road.At(exit,out _);
            var bypass=Curve(a,new(78,38.1f,-120),new(86,37.8f,-134),new(105,38,-137),new(120,38.4f,-125),b);
            strips.Clear();strips.Add(new("reverse gully grounded crossing",bypass,4));Landscape(name,true);ClearTrees(name);Surface(strips[0],name);
            var ps=new List<Vector3>();for(float s=0;s<entry-.3f;s+=.8f)ps.Add(road.At(s,out _));ps.AddRange(bypass);for(float s=exit+.5f;s<road.Length;s+=.8f)ps.Add(road.At(s,out _));road.points=ps.ToArray();typeof(RaceRoad).GetField("distance",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).SetValue(road,null);road.Initialize();
            foreach(var branch in Object.FindObjectsByType<WoodlandRoute>()){branch.entryRoad=road.Project(branch.points[0],out _);branch.exitRoad=road.Project(branch.points[^1],out _);branch.bypassedGates=race.gates.Select((g,i)=>(g,i)).Where(x=>x.i>0&&road.Relative(road.Project(x.g.transform.position,out _),branch.entryRoad)<road.Relative(branch.exitRoad,branch.entryRoad)).Select(x=>x.i).ToArray();}
            var data=JsonUtility.FromJson<Export>(File.ReadAllText(Evidence+"/"+name+"-geometry.json"));data.main=road.points;data.surfaces=data.surfaces.Append(new ExportStrip{name=strips[0].name,points=bypass,width=4}).ToArray();File.WriteAllText(Evidence+"/"+name+"-geometry.json",JsonUtility.ToJson(data,true));EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();FinalGuidance();File.WriteAllText(Evidence+"/reverse-gully-done.txt","Reverse uses nearby grounded southern gully crossing; Forward gully and existing courses unchanged.");
        }
    }
}
