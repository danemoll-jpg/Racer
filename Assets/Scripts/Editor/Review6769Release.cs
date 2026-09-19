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
    public static class Review6769Release
    {
        public const string Evidence="Docs/CR067-069",Version="0.11.0-review1",Folder="Assets/Track/HairpinReview";
        public static void Apply()
        {
            if(Application.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Saved edit mode required");
            Directory.CreateDirectory(Evidence);Directory.CreateDirectory(Folder);AssetDatabase.Refresh();
            var report=new List<string>();
            foreach(var scene in new[]{StreetLoopBuilder.ScenePath,LakeCourseBuild.ScenePath})
            {
                EditorSceneManager.OpenScene(scene);var race=Object.FindAnyObjectByType<RaceDirector>();
                var house=GameObject.Find("Remembered house behind southern hairpin").transform;
                if(Vector3.Distance(house.position,new(484,23.9f,-596.2f))>1)throw new Exception("Hairpin property identity changed");
                var street=race.ambientRoad?race.ambientRoad:race.road;street.Initialize();
                var extension=GameObject.Find("South Cherokee Lane south");
                if(extension)
                {
                    // Its own traffic/boundary are children/components; race roads and branches
                    // are separate authored arrays. Retire the entire scoped hierarchy.
                    if(extension.GetComponent<RaceRoad>()||extension.GetComponent<WoodlandRoute>())throw new Exception("Unexpected route reference");
                    report.Add(scene+" removed "+extension.GetComponentsInChildren<Collider>(true).Length+" extension colliders; house="+house.position+" rotation="+house.rotation);
                    Object.DestroyImmediate(extension);
                }
                // A narrow gravel driveway is painted into the existing collision heightfield.
                // No overlaid slab, raised lip, or road/terrain height changes.
                Vector3 start=house.position+house.forward*7;
                Vector3 end=street.At(street.Project(start,out _),out _);
                var axis=Vector3.ProjectOnPlane(end-start,Vector3.up);float length=axis.magnitude;axis/=length;
                int colored=0;
                foreach(var mf in GameObject.Find("Memory loop - north is +Z").GetComponentsInChildren<MeshFilter>())
                {
                    if(AssetDatabase.GetAssetPath(mf.sharedMesh).StartsWith(Folder+"/"))continue;
                    var mesh=mf.sharedMesh;var vs=mesh.vertices;var colors=mesh.colors;bool changed=false;
                    for(int i=0;i<vs.Length;i++)
                    {
                        var p=mf.transform.TransformPoint(vs[i]);var offset=Vector3.ProjectOnPlane(p-start,Vector3.up);
                        float along=Vector3.Dot(offset,axis);if(along<0||along>length)continue;
                        float across=(offset-axis*along).magnitude;if(across>3.5f)continue;
                        var s=street.Project(p,out _);var road=street.At(s,out _);float rd=Vector3.ProjectOnPlane(p-road,Vector3.up).magnitude;
                        float blend=(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(2.2f,3.5f,across)))*Mathf.SmoothStep(0,1,Mathf.InverseLerp(5,7,rd));
                        if(blend<=0)continue;
                        colors[i]=Color.Lerp(colors[i],new(.46f,.45f,.39f),blend);changed=true;colored++;
                    }
                    if(changed)
                    {
                        string path=Folder+"/"+(race.Forest?"Forest-":"Street-")+mf.name+".asset";
                        var copy=AssetDatabase.LoadAssetAtPath<Mesh>(path);
                        if(!copy){copy=Object.Instantiate(mesh);AssetDatabase.CreateAsset(copy,path);}
                        copy.colors=colors;EditorUtility.SetDirty(copy);mf.sharedMesh=copy;mf.GetComponent<MeshCollider>().sharedMesh=copy;
                    }
                }
                // Existing native terrain and trees already continue beneath the obsolete deck.
                // Add a few supported trees only in its released rear corridor, beyond the house.
                var priorWoods=GameObject.Find("Hairpin restored rear woodland");if(priorWoods)Object.DestroyImmediate(priorWoods);
                Physics.SyncTransforms();
                {
                    var woods=new GameObject("Hairpin restored rear woodland").transform;
                    var mat=AssetDatabase.LoadAssetAtPath<Material>("Assets/Vegetation/Phase6/Forest.mat");
                    var crown=Object.Instantiate(AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Vegetation/Phase6/BroadCrown.asset"));
                    var trunk=Object.Instantiate(AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Vegetation/Phase6/Trunk.asset"));
                    crown.colors=Enumerable.Repeat(new Color(.34f,.42f,.23f),crown.vertexCount).ToArray();
                    trunk.colors=Enumerable.Repeat(new Color(.25f,.22f,.15f),trunk.vertexCount).ToArray();
                    var parts=new List<CombineInstance>();int count=0;
                    for(int i=0;i<9;i++)
                    {
                        var p=new Vector3(501+(i%2)*6,0,-622-i*13);p.y=Phase6Buildings.Ground(p);
                        if(Physics.OverlapSphere(p+Vector3.up*3,3,1,QueryTriggerInteraction.Ignore).Any(c=>c is BoxCollider))continue;
                        float h=12+i%3*2;var t=new GameObject("Supported rear tree").transform;t.SetParent(woods);t.position=p;
                        var box=t.gameObject.AddComponent<BoxCollider>();box.center=new(0,h*.25f,0);box.size=new(.8f,h*.5f,.8f);
                        parts.Add(new(){mesh=trunk,transform=Matrix4x4.TRS(p+Vector3.up*h*.25f,Quaternion.identity,box.size)});
                        parts.Add(new(){mesh=crown,transform=Matrix4x4.TRS(p+Vector3.up*h*.32f,Quaternion.Euler(0,i*37,0),new(h*.31f,h*.66f,h*.31f))});count++;
                    }
                    var combined=new Mesh();combined.CombineMeshes(parts.ToArray());string path=Folder+"/"+(race.Forest?"Forest":"Street")+"-RearWoods.asset";
                    var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(existing){EditorUtility.CopySerialized(combined,existing);Object.DestroyImmediate(combined);combined=existing;}else AssetDatabase.CreateAsset(combined,path);
                    Object.DestroyImmediate(crown);Object.DestroyImmediate(trunk);
                    woods.gameObject.AddComponent<MeshFilter>().sharedMesh=combined;woods.gameObject.AddComponent<MeshRenderer>().sharedMaterial=mat;
                    report.Add(scene+" supported rear trees="+count);
                }
                if(!race.Forest)race.courseId="street-v10-hairpin";
                report.Add(scene+" driveway color vertices="+colored+"; all terrain heights and route arrays unchanged; remaining continuations="+string.Join(",",Object.FindObjectsByType<ContinuationTraffic>().Select(t=>t.name)));
                EditorUtility.SetDirty(race);Physics.SyncTransforms();EditorSceneManager.MarkSceneDirty(race.gameObject.scene);EditorSceneManager.SaveScene(race.gameObject.scene);AssetDatabase.SaveAssets();
            }
            File.WriteAllLines(Evidence+"/authoring.txt",report);
        }
        static double buildAt;
        public static void QueueBuild(){Directory.CreateDirectory(Evidence);buildAt=EditorApplication.timeSinceStartup+2;EditorApplication.update-=Tick;EditorApplication.update+=Tick;}
        static void Tick(){if(EditorApplication.timeSinceStartup<buildAt)return;EditorApplication.update-=Tick;try{Build();File.WriteAllText(Evidence+"/build-done.txt","Complete");}catch(Exception e){File.WriteAllText(Evidence+"/build-done.txt",e.ToString());}}
        public static void Build()
        {
            if(Application.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Saved edit mode required");
            PlayerSettings.bundleVersion=Version;EditorUserBuildSettings.development=EditorUserBuildSettings.allowDebugging=EditorUserBuildSettings.connectProfiler=false;
            string output="Builds/Racer-"+Version+"-Windows";Directory.CreateDirectory(output);
            var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{StreetLoopBuilder.ScenePath,LakeCourseBuild.ScenePath},locationPathName=output+"/Racer.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
            File.WriteAllText(Evidence+"/build.txt",$"{result.summary.result}; errors={result.summary.totalErrors}; warnings={result.summary.totalWarnings}; time={result.summary.totalTime}\n"+string.Join("\n",result.steps.SelectMany(s=>s.messages).Where(m=>m.type==LogType.Error||m.type==LogType.Warning).Select(m=>m.content)));
            if(result.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Build failed");
            File.WriteAllText(output+"/VERSION.txt",$"Racer {Version}\nWindows x64 review / CR-067 through CR-069\nRules: street-v10-hairpin / lake-v3-shallows; development=false\nSafety checkpoint: 255a61b2095c1cad871b16640afff30eb979df22\nSource tree: SOURCE-SHA256.txt; completion commit recorded after validation\nUnity {Application.unityVersion}\n");
            Directory.CreateDirectory(output+"/Licenses");foreach(var notice in Directory.GetFiles("Assets/Plugins/LocalRadio","*.txt"))File.Copy(notice,output+"/Licenses/"+Path.GetFileName(notice),true);
            File.Copy("Assets/Audio/Wildlife/LICENSE.txt",output+"/Licenses/Wildlife-CC0.txt",true);
        }
    }
}
