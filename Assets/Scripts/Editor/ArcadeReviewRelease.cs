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
    [InitializeOnLoad]
    public static class ArcadeReviewRelease
    {
        public const string Evidence="Docs/CR075-080",Version="0.13.0-review7";
        static string job;static double due;
        static ArcadeReviewRelease(){var saved=SessionState.GetString("Racer.ArcadeReview.Pending","");if(saved!="")Queue(saved);}
        public static void Queue(string value){job=value;SessionState.SetString("Racer.ArcadeReview.Pending",value);due=EditorApplication.timeSinceStartup+2;EditorApplication.update-=Work;EditorApplication.update+=Work;}
        static void Work(){if(EditorApplication.timeSinceStartup<due||EditorApplication.isCompiling||EditorApplication.isUpdating)return;EditorApplication.update-=Work;SessionState.EraseString("Racer.ArcadeReview.Pending");Directory.CreateDirectory(Evidence);try{if(job=="author")Author();else if(job=="photos")RoadPhotos();else Build();File.WriteAllText(Evidence+"/"+job+"-done.txt","Complete "+DateTime.UtcNow.ToString("o"));}catch(Exception e){File.WriteAllText(Evidence+"/"+job+"-done.txt",e.ToString());Debug.LogException(e);}}
        static void RoadPhotos()
        {
            Guard();
            foreach(var path in ReverseReviewRelease.Scenes)
            {
                var scene=EditorSceneManager.OpenScene(path);var race=Object.FindAnyObjectByType<RaceDirector>();var road=race.Forest?race.ambientRoad:race.road;int i=0;
                foreach(var sign in Object.FindObjectsByType<TextMesh>().Where(t=>t.text=="Trickum Road"||t.text=="TO Jamerson Road"))
                {
                    float s=road.Project(sign.transform.position,out _);
                    foreach(int dir in new[]{1,-1}){var p=road.At(s-dir*25,out var f)+Vector3.up*2.5f;Capture(p,f*dir,Evidence+"/"+scene.name+"-road-role-"+i+"-"+dir+".png");}i++;
                }
            }
        }
        static void Guard(){if(Application.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Saved edit mode required");}
        public static void Author()
        {
            Guard();Directory.CreateDirectory("Assets/Track/ArcadeReview");AssetDatabase.Refresh();
            foreach(var path in ReverseReviewRelease.Scenes)
            {
                var scene=EditorSceneManager.OpenScene(path);var race=Object.FindAnyObjectByType<RaceDirector>();race.road.Initialize();
                var report=new List<string>{"Physical road roles: north commercial road = Hwy 92; west connecting road = Trickum Road; South Cherokee Lane ends at separate Jamerson Road. Existing topology retained."};
                foreach(var text in Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include))
                {
                    var p=text.transform.position;
                    if(text.text=="Hwy 92"&&p.x< -600&&p.z>450&&p.z<530){text.text="Trickum Road";report.Add("ROAD NAME corrected misleading highway sign at "+p);}
                    if(text.text=="Jamerson Rd"&&p.x< -560&&p.z< -530){text.text="TO Jamerson Road";report.Add("DESTINATION to separate Jamerson at "+p);}
                    if(text.text=="Hwy 92\nSouth Cherokee Lane")text.text="South Cherokee Lane\nJUNCTION: Hwy 92";
                    bool oldShortcut=text.name=="Shortcut advice"||text.text.StartsWith("Creek Leap")||text.text.StartsWith("Fox Gully")||text.text.StartsWith("Pine Ridge");
                    if(oldShortcut&&(race.reverseCourse||race.Forest||text.name=="Shortcut advice"))text.transform.parent.gameObject.SetActive(false);
                    if(race.reverseCourse&&text.name=="Recommended speed")text.transform.parent.gameObject.SetActive(false);
                }
                var old=GameObject.Find("CR075 direction signs");if(old)Object.DestroyImmediate(old);
                var root=new GameObject("CR075 direction signs").transform;
                foreach(var branch in Object.FindObjectsByType<WoodlandRoute>())
                {
                    float s=branch.entryRoad-28;var p=race.road.At(s,out var f);var mouth=branch.At(Mathf.Max(12,branch.entryInset),out _);var right=Vector3.Cross(Vector3.up,f).normalized;
                    string turn=Vector3.Dot(mouth-race.road.At(branch.entryRoad,out _),right)>0?">":"<";
                    Vector3 signPoint=p+right*(race.road.HalfWidth(s)+2.2f);int bestBlocked=int.MaxValue;
                    foreach(float shift in new[]{0f,-12f,12f})foreach(float side in new[]{1f,-1f})foreach(float setback in new[]{2.2f,3.5f,5f})
                    {
                        var candidate=Ground(p+f*shift+right*side*(race.road.HalfWidth(s)+setback));
                        int blocked=0;foreach(float look in new[]{32f,20f,12f})for(float edge=-3.5f;edge<=3.5f;edge+=.5f)foreach(float height in new[]{2.2f,2.8f,3.4f})
                            if(Physics.Linecast(race.road.At(s-look,out _)+Vector3.up*2.7f,candidate+Vector3.up*height+right*edge,1,QueryTriggerInteraction.Ignore))blocked++;
                        if(blocked<bestBlocked){bestBlocked=blocked;signPoint=candidate;}
                    }
                    Sign(root,signPoint,f,branch.title+" "+turn+"\nOPTIONAL / "+(race.reverseCourse?"REVERSE":"FORWARD"));
                    report.Add("APPROACH sightline "+branch.title+": "+bestBlocked+"/135 obstructed probe rays; sign="+signPoint);
                    report.Add($"SHORTCUT {branch.title}: approach {s:F1}; entrance {branch.entryRoad:F1}; exit {branch.exitRoad:F1}; bypass [{string.Join(",",branch.bypassedGates)}]");
                    Capture(race.road.At(s-32,out var approach)+Vector3.up*2.7f,approach,Evidence+"/"+scene.name+"-"+branch.title.Replace(' ','-')+"-approach.png");
                }
                if(race.Forest)
                {
                    foreach(float s in new[]{30f,race.road.Length-65}){var p=race.road.At(s,out var f);Sign(root,p+Vector3.Cross(Vector3.up,f).normalized*(race.road.HalfWidth(s)+2),f,"FOREST ROUTE  ^\nFOLLOW AMBER BOARDS");}
                }
                Ramp(race,report);
                Activities(race,report);
                foreach(var site in Object.FindObjectsByType<ActivitySite>())foreach(var profile in race.EligibleVehicles){site.Targets(profile.Id,out float b,out float silver,out float g);report.Add($"FINAL TARGETS {site.id}/{profile.Id}: {b}/{silver}/{g}; duration={site.Seconds}; launch/travel direction={site.forward}");}
                race.courseId=race.Forest?(race.reverseCourse?"forest-reverse-v2-arcade":"lake-v4-arcade"):(race.reverseCourse?"street-reverse-v2-arcade":"street-v11-arcade");
                foreach(var t in Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include).Where(t=>t.GetComponentInParent<PhysicalSign>()))report.Add((t.gameObject.activeInHierarchy?"ACTIVE ":"INACTIVE ")+t.text.Replace('\n','|')+" @ "+t.transform.position.ToString("F2"));
                File.WriteAllLines(Evidence+"/inventory-"+scene.name+".txt",report);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets();
        }
        static Vector3 Ground(Vector3 p)
        {
            var hits=Physics.RaycastAll(p+Vector3.up*30,Vector3.down,70,1,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name.StartsWith("Ground_")).OrderBy(h=>h.distance).ToArray();if(hits.Length>0)p.y=hits[0].point.y;return p;
        }
        static void Sign(Transform root,Vector3 p,Vector3 f,string words)
        {
            p=Ground(p);
            var sign=new GameObject(words).transform;sign.SetParent(root);sign.SetPositionAndRotation(p,Quaternion.LookRotation(Vector3.ProjectOnPlane(f,Vector3.up)));sign.gameObject.AddComponent<PhysicalSign>();
            var mat=AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Phase4Ramp.mat");
            void Box(Vector3 pos,Vector3 size){var b=GameObject.CreatePrimitive(PrimitiveType.Cube);b.transform.SetParent(sign,false);b.transform.localPosition=pos;b.transform.localScale=size;b.GetComponent<Renderer>().sharedMaterial=mat;Object.DestroyImmediate(b.GetComponent<Collider>());}
            Box(new(0,2.8f,0),new(7,1.6f,.14f));Box(new(0,1.4f,.08f),new(.18f,2.8f,.18f));
            var label=new GameObject("Mounted direction lettering").AddComponent<TextMesh>();label.transform.SetParent(sign,false);label.transform.localPosition=new(0,2.8f,-.09f);label.text=words;label.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");label.fontSize=64;label.characterSize=.12f;label.anchor=TextAnchor.MiddleCenter;label.alignment=TextAlignment.Center;label.color=Color.white;
            label.characterSize=Mathf.Min(.1f,1.5f/words.Split('\n').Max(line=>line.Length));
            label.GetComponent<Renderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Environment/Phase8/Depth tested world lettering 0.mat");
        }
        static void Capture(Vector3 p,Vector3 f,string path)
        {
            var cam=Camera.main;var oldP=cam.transform.position;var oldR=cam.transform.rotation;var oldT=cam.targetTexture;var active=RenderTexture.active;var rt=new RenderTexture(1280,720,24);var tex=new Texture2D(1280,720,TextureFormat.RGB24,false);
            try{cam.transform.SetPositionAndRotation(p,Quaternion.LookRotation(f));cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1280,720),0,0);tex.Apply();File.WriteAllBytes(path,tex.EncodeToPNG());}
            finally{cam.transform.SetPositionAndRotation(oldP,oldR);cam.targetTexture=oldT;RenderTexture.active=active;Object.DestroyImmediate(rt);Object.DestroyImmediate(tex);}
        }
        static void Ramp(RaceDirector race,List<string> report)
        {
            bool reverse=race.reverseCourse&&!race.Forest;var supportRoad=race.Forest?race.ambientRoad:race.road;
            var frame=GameObject.Find(Phase4Setup.RootName).transform;
            var ramp=reverse?GameObject.Find("Reverse supported roadworks transition"):frame.GetComponentsInChildren<MeshCollider>().First(c=>c.name.StartsWith("Takeoff -")).gameObject;
            foreach(var c in Object.FindObjectsByType<Collider>().Where(c=>c.name=="Graded aggregate shoulder")){c.gameObject.SetActive(false);}
            var vertices=new List<Vector3>();var triangles=new List<int>();float[] sides=reverse?new[]{-10.5f,-4.5f,1.5f,7.5f}:new[]{-6.5f,-4.5f,1.5f,3.5f};
            for(int i=0;i<=180;i++)
            {
                float x=i*.5f,z=reverse?100-x:x-10;float u=x-10;
                float length=reverse?42:40,height=reverse?3.2f:6.2f;
                float h=u<0?0:u<length?height*Mathf.Pow(u/length,2):u<length+24?height*(1-Mathf.SmoothStep(0,1,(u-length)/24)):0;
                for(int j=0;j<4;j++){var p=frame.TransformPoint(new(sides[j],0,z));p.y=supportRoad.At(supportRoad.Project(p,out _),out _).y+h*(j==0||j==3?0:1)+.018f;vertices.Add(p);}
                if(i<180)for(int j=0;j<3;j++){int a=i*4+j;triangles.AddRange(reverse?new[]{a,a+1,a+4,a+1,a+5,a+4}:new[]{a,a+4,a+1,a+1,a+4,a+5});}
            }
            // Vertices are authored in world coordinates. Preserve the original renderer transform.
            for(int i=0;i<vertices.Count;i++)vertices[i]=ramp.transform.InverseTransformPoint(vertices[i]);
            var mesh=new Mesh{name="Trickum continuous bevel and rollout"};mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
            string path="Assets/Track/ArcadeReview/Trickum"+(race.Forest?race.gameObject.scene.name:reverse?"Reverse":"Forward")+".asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(existing){EditorUtility.CopySerialized(mesh,existing);Object.DestroyImmediate(mesh);mesh=existing;}else AssetDatabase.CreateAsset(mesh,path);
            ramp.GetComponent<MeshFilter>().sharedMesh=mesh;ramp.GetComponent<MeshCollider>().sharedMesh=mesh;
            report.Add("RAMP Trickum "+(reverse?"Reverse":"Forward")+": original transform and launch height/run retained; stepped gravel shoulders replaced by continuous 2m bevels and 24m supported rollout; replacement surface remains collidable.");
        }
        static void Activities(RaceDirector race,List<string> report)
        {
            var previous=GameObject.Find("CR079 arcade sites");if(previous)Object.DestroyImmediate(previous);
            var root=new GameObject("CR079 arcade sites").transform;
            ActivitySite Site(string id,string title,ActivitySite.Kind kind,Vector3 point,Vector3 direction,float radius,float bronze,float silver,float gold)
            {
                var site=new GameObject(title).AddComponent<ActivitySite>();site.transform.SetParent(root);site.transform.position=point;site.id=id;site.title=title;site.kind=kind;site.forward=direction;site.radius=radius;site.bronze=bronze;site.silver=silver;site.gold=gold;site.bothDirections=kind==ActivitySite.Kind.Speed;
                report.Add($"ACTIVITY {id} / {title} / {kind} @ {point:F2} / B,S,G={bronze},{silver},{gold}; two-way={site.bothDirections}");return site;
            }
            var road=race.road;
            // Forest cameras belong on grounded stretches, outside launch profiles.
            float[] stations=race.Forest?(race.reverseCourse?new[]{1850f,2000f}:new[]{120f,250f}):new[]{race.reverseCourse?550f:3950f,race.reverseCourse?1350f:3300f};
            for(int i=0;i<stations.Length;i++)
            {
                var p=road.At(stations[i],out var f);var site=Site("speed-"+i,(race.Forest?"Forest":"Trickum / 92")+" speed trap "+(i+1),ActivitySite.Kind.Speed,p,f,road.HalfWidth(stations[i])+1,race.Forest?12:20,race.Forest?18:28,race.Forest?24:36);
                Sign(root,p+Vector3.Cross(Vector3.up,f).normalized*(site.radius+2),f,"SPEED CAMERA\nBOTH DIRECTIONS");
                Sign(root,p-Vector3.Cross(Vector3.up,f).normalized*(site.radius+2),-f,"SPEED CAMERA\nBOTH DIRECTIONS");
            }
            Vector3 jump,direction;
            var layout=Object.FindAnyObjectByType<ForestLayout>();
            if(race.Forest&&layout){jump=road.At(layout.jumpStarts[0]+(race.reverseCourse?55:34),out direction);}
            else{var frame=GameObject.Find(Phase4Setup.RootName).transform;jump=frame.TransformPoint(new(-1.5f,race.reverseCourse?3.2f:6.2f,race.reverseCourse?48:40));direction=frame.forward*(race.reverseCourse?-1:1);}
            var leap=Site("jump-01",race.Forest?"Forest opening jump":"Trickum pavement jump",ActivitySite.Kind.Jump,jump,direction,32,20,45,75);leap.Seconds=180;
            if(race.Forest&&race.reverseCourse){leap.bronze=18;leap.silver=30;leap.gold=40;}
            else if(race.Forest){leap.vehicleBronze=new[]{20f,20f,24f,15f};leap.vehicleSilver=new[]{45f,45f,40f,28f};leap.vehicleGold=new[]{75f,75f,58f,36f};}
            else if(race.reverseCourse){leap.vehicleBronze=new[]{14f,14f,15f,15f};leap.vehicleSilver=new[]{30f,30f,26f,30f};leap.vehicleGold=new[]{48f,48f,33f,38f};}
            Sign(root,jump-direction*38+Vector3.Cross(Vector3.up,direction)*10,direction,"JUMP CHALLENGE\nLAND CLEAN / PAUSE FOR TARGETS");
            var props=Object.FindObjectsByType<BreakableProp>().Where(p=>p.name=="Property chain-link"||p.name=="Property white X").OrderBy(p=>p.transform.position.x).ThenBy(p=>p.transform.position.z).ToArray();
            if(props.Length>=10)
            {
                var anchor=props.OrderBy(p=>Vector3.Distance(p.transform.position,new Vector3(420,85,-31))).First();
                var targets=props.Where(p=>Mathf.Abs(Vector3.Dot(p.transform.position-anchor.transform.position,anchor.transform.forward))<1&&Mathf.Abs(p.transform.position.y-anchor.transform.position.y)<1.5f&&Vector3.Distance(p.transform.position,anchor.transform.position)<50).OrderBy(p=>p.transform.position.x).Take(14).ToArray();
                var center=targets.Aggregate(Vector3.zero,(sum,p)=>sum+p.transform.position)/targets.Length;
                float s=(race.ambientRoad?race.ambientRoad:road).Project(center,out _);var approach=(race.ambientRoad?race.ambientRoad:road).At(s,out var f);
                var smash=Site("smash-01","Fence line smash",ActivitySite.Kind.Smash,center,f,85,3,6,10);smash.props=targets;smash.Seconds=8;
                Sign(root,approach+Vector3.ProjectOnPlane(center-approach,Vector3.up).normalized*10,f,"FENCE LINE SMASH\n3 / 6 / 10 PROPS IN 8 s");
            }
        }
        public static void Build()
        {
            Guard();PlayerSettings.bundleVersion=Version;EditorUserBuildSettings.development=false;var scenes=ReverseReviewRelease.Scenes;
            string output="Builds/Racer-"+Version+"-Windows";Directory.CreateDirectory(output);
            var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=scenes,locationPathName=output+"/Racer.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
            File.WriteAllText(Evidence+"/build.txt",$"{result.summary.result}; errors={result.summary.totalErrors}; warnings={result.summary.totalWarnings}; time={result.summary.totalTime}\n"+string.Join("\n",result.steps.SelectMany(s=>s.messages).Where(m=>m.type==LogType.Error||m.type==LogType.Warning).Select(m=>m.content)));
            if(result.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Build failed");
            File.WriteAllText(output+"/VERSION.txt",$"Racer {Version}\nCR-075 through CR-080 combined review / Windows x64\nSafety checkpoint: 13f989a408aaa6d085167d7cb480d9886af1997c\nSource: SOURCE-SHA256.txt; completion commit to be stamped after validation\nUnity {Application.unityVersion}\n");
            Directory.CreateDirectory(output+"/Licenses");foreach(var notice in Directory.GetFiles("Assets/Plugins/LocalRadio","*.txt"))File.Copy(notice,output+"/Licenses/"+Path.GetFileName(notice),true);
            foreach(var notice in new[]{"LICENSE.txt","REVERSE-WILDLIFE-NOTICE.txt"})File.Copy("Assets/Audio/Wildlife/"+notice,output+"/Licenses/Wildlife-"+notice,true);
        }
    }
}

