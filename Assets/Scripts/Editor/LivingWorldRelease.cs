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
    public static class LivingWorldRelease
    {
        public const string Evidence="Docs/CR057-060",Folder="Assets/Track/LivingWorld",Version="0.9.0-review1";
        public static void Apply()
        {
            if(Application.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Saved edit mode required");
            Directory.CreateDirectory(Evidence);Directory.CreateDirectory(Folder);AssetDatabase.Refresh();var log=new List<string>();
            foreach(var scene in new[]{StreetLoopBuilder.ScenePath,LakeCourseBuild.ScenePath})
            {
                EditorSceneManager.OpenScene(scene);var race=Object.FindAnyObjectByType<RaceDirector>();bool forest=race.Forest;race.courseId=forest?"lake-v3-shallows":"street-v9-life";
                // Preserve mounted and unknown text; remove only known unmounted authoring labels.
                int removed=0;foreach(var t in Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include).ToArray())
                {if(!SceneryText.IsFloating(t))continue;Object.DestroyImmediate(t.gameObject);removed++;}
                foreach(var canvas in Object.FindObjectsByType<Canvas>())if(canvas.renderMode==RenderMode.WorldSpace)log.Add("World canvas requires review: "+canvas.name);
                var waters=Object.FindObjectsByType<MeshRenderer>().Where(r=>r.name=="Friend's lake"||r.name=="Friend's lake - visible shoreline"||r.name=="J1 flowing creek"||r.name=="Creek surface").ToArray();
                foreach(var renderer in waters)
                {
                    var w=renderer.GetComponent<ShallowWater>()??renderer.gameObject.AddComponent<ShallowWater>();w.round=renderer.name.Contains("lake");
                    var collider=w.GetComponent<Collider>();if(collider)Object.DestroyImmediate(collider);
                    if(w.round&&forest)race.road.Initialize();
                    // Modify a copy of the existing terrain; preserve every point already above the shallow bed.
                    int count=0;foreach(var filter in GameObject.Find("Memory loop - north is +Z").GetComponentsInChildren<MeshFilter>())
                    {
                        var vertices=filter.sharedMesh.vertices;bool changed=false;
                        for(int i=0;i<vertices.Length;i++)
                        {
                            var p=filter.transform.TransformPoint(vertices[i]);var local=w.transform.InverseTransformPoint(p);
                            float edge=w.round?(new Vector2(local.x,local.z).magnitude-.5f)*Mathf.Min(w.transform.lossyScale.x,w.transform.lossyScale.z):Mathf.Max((Mathf.Abs(local.x)-.5f)*w.transform.lossyScale.x,(Mathf.Abs(local.z)-.5f)*w.transform.lossyScale.z);
                            if(edge>10)continue;
                            float bed=w.Surface-.65f+.85f*Mathf.SmoothStep(0,1,Mathf.InverseLerp(w.round?-17:-4,0,edge));float blend=1-Mathf.SmoothStep(0,1,Mathf.Clamp01(edge/10));float y=Mathf.Lerp(p.y,Mathf.Max(p.y,bed),blend);
                            if(w.round&&forest)
                            {
                                float station=race.road.Project(p,out float distance);var trail=race.road.At(station,out _);
                                // CR-056's d>7 lake carve left a near-vertical seam next to the grid.
                                // Raise its shore approach to a 10-degree bank; leave trail points intact.
                                float bank=trail.y-.18f*Mathf.Max(0,distance-race.road.HalfWidth(station));
                                if(distance<40)y=Mathf.Max(y,Mathf.Lerp(p.y,Mathf.Max(p.y,bank),blend));
                            }
                            if(y-p.y<.0001f)continue;p.y=y;vertices[i]=filter.transform.InverseTransformPoint(p);changed=true;
                        }
                        if(!changed)continue;
                        string path=Folder+"/"+(forest?"Forest-":"Street-")+filter.name+".asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
                        if(!mesh){mesh=Object.Instantiate(filter.sharedMesh);AssetDatabase.CreateAsset(mesh,path);}mesh.vertices=vertices;mesh.RecalculateNormals();mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);filter.sharedMesh=mesh;var mc=filter.GetComponent<MeshCollider>();mc.sharedMesh=null;mc.sharedMesh=mesh;count++;
                    }
                    log.Add(scene+" water "+w.name+" surface="+w.Surface+" supported tiles="+count);
                }
                Physics.SyncTransforms();
                var life=race.GetComponent<AmbientLife>()??race.gameObject.AddComponent<AmbientLife>();
                Vector3 Ground(Vector3 p){if(!Physics.Raycast(p+Vector3.up*200,Vector3.down,out var hit,500,1,QueryTriggerInteraction.Ignore))throw new InvalidOperationException("Unsupported ambience "+p);p.y=hit.point.y+.025f;return p;}
                SetHouseholds(life,race);
                var road=race.ambientRoad?race.ambientRoad:race.road;road.Initialize();var high=new List<Vector3>();var sparse=new List<Vector3>();
                for(float s=3760;s<4580;s+=70)foreach(int side in new[]{-1,1})
                {var at=road.At(s,out var f)+Vector3.Cross(Vector3.up,f).normalized*side*18;var p=Ground(at);if(Mathf.Abs(p.y-at.y)<3&&!Physics.CheckSphere(p+Vector3.up,.7f,~1,QueryTriggerInteraction.Ignore))high.Add(p);}
                foreach(float s in new[]{250f,1000f,1450f,2150f}){var at=road.At(s,out var f)+Vector3.Cross(Vector3.up,f).normalized*16;var p=Ground(at);if(Mathf.Abs(p.y-at.y)<3)sparse.Add(p);}
                life.highway=high.ToArray();life.residential=sparse.ToArray();
                SetStreetGroups(life,road);
                if(forest){var cave=Object.FindAnyObjectByType<WoodlandRoute>();life.batEntrance=cave.At(100,out var f);life.batForward=Vector3.ProjectOnPlane(f,Vector3.up).normalized;life.batRoost=cave.At(126,out _)+Vector3.up*7;}
                EditorUtility.SetDirty(life);EditorUtility.SetDirty(race);EditorSceneManager.MarkSceneDirty(race.gameObject.scene);EditorSceneManager.SaveScene(race.gameObject.scene);AssetDatabase.SaveAssets();
                log.Add(scene+" removed text="+removed+" highway slots="+high.Count+" sparse slots="+sparse.Count+" waters="+waters.Length);
            }
            File.WriteAllLines(Evidence+"/authoring.txt",log);
        }
        static void SetHouseholds(AmbientLife life,RaceDirector race)
        {
            Vector3 Ground(Vector3 p){p.y=Phase6Buildings.Ground(p)+.025f;return p;}
            var street=race.ambientRoad?race.ambientRoad:race.road;street.Initialize();var house=GameObject.Find("Dan - blue X").transform;
            var road=street.At(street.Project(house.position,out _),out _);var away=Vector3.ProjectOnPlane(house.position-road,Vector3.up).normalized;var along=Vector3.Cross(Vector3.up,away);
            // The accepted CR-045 rectangular fence stands 11 m from the road; 8.5 m is
            // outside its front panels, still beyond the traffic lane and clear of its gate.
            life.coffee=new[]{Ground(road+away*8.5f+along*11),Ground(road+away*8.5f+along*13.3f)};
            // CR-068: play inside the original yard, just behind its 11m front fence.
            // The old 37-44m setback was outside the ordinary chase-camera approach.
            life.football=new[]{Ground(road+away*14+along*-7),Ground(road+away*17+along*-2),Ground(road+away*14+along*4)};
            var friend=GameObject.Find("Friend across street - blue circle").transform;
            var fr=street.At(street.Project(friend.position,out _),out _);var fa=Vector3.ProjectOnPlane(friend.position-fr,Vector3.up).normalized;var ft=Vector3.Cross(Vector3.up,fa);
            life.smoking=new[]{Ground(fr+fa*13+ft*3),Ground(fr+fa*13+ft*5.5f)};
        }
        static void SetStreetGroups(AmbientLife life,RaceRoad street)
        {
            for(int i=2;i<life.highway.Length;i+=4)for(int side=0;side<2&&i+side<life.highway.Length;side++)
            {
                var partner=life.highway[i+side-2];street.At(street.Project(partner,out _),out var forward);
                var at=partner+Vector3.ProjectOnPlane(forward,Vector3.up).normalized*2.4f;at.y=Phase6Buildings.Ground(at)+.025f;life.highway[i+side]=at;
            }
        }
        public static void RefreshHouseholds()
        {
            if(Application.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Saved edit mode required");
            foreach(var scene in new[]{StreetLoopBuilder.ScenePath,LakeCourseBuild.ScenePath}){EditorSceneManager.OpenScene(scene);var race=Object.FindAnyObjectByType<RaceDirector>();var life=race.GetComponent<AmbientLife>();SetHouseholds(life,race);SetStreetGroups(life,race.ambientRoad?race.ambientRoad:race.road);EditorUtility.SetDirty(life);EditorSceneManager.MarkSceneDirty(race.gameObject.scene);EditorSceneManager.SaveScene(race.gameObject.scene);}
        }
        static double buildAt;
        public static void QueueBuild(){buildAt=EditorApplication.timeSinceStartup+2;EditorApplication.update-=Tick;EditorApplication.update+=Tick;}
        static void Tick(){if(EditorApplication.timeSinceStartup<buildAt)return;EditorApplication.update-=Tick;try{Build();File.WriteAllText(Evidence+"/build-done.txt","Complete");}catch(Exception e){File.WriteAllText(Evidence+"/build-done.txt",e.ToString());}}
        public static void Build()
        {
            if(Application.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Saved edit mode required");
            if(PlayerSettings.GetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.Standalone).Contains("ENABLE_RUNTIME_PIPELINE"))throw new InvalidOperationException("Runtime pipeline must remain excluded");
            PlayerSettings.bundleVersion=Version;EditorUserBuildSettings.development=EditorUserBuildSettings.allowDebugging=EditorUserBuildSettings.connectProfiler=false;
            string output="Builds/Racer-"+Version+"-Windows";Directory.CreateDirectory(output);
            var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{StreetLoopBuilder.ScenePath,LakeCourseBuild.ScenePath},locationPathName=output+"/Racer.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
            File.WriteAllText(Evidence+"/build.txt",$"{result.summary.result}; errors={result.summary.totalErrors}; warnings={result.summary.totalWarnings}; time={result.summary.totalTime}\n"+string.Join("\n",result.steps.SelectMany(s=>s.messages).Where(m=>m.type==LogType.Error||m.type==LogType.Warning).Select(m=>m.content)));
            if(result.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new InvalidOperationException("Build failed");
            File.WriteAllText(output+"/VERSION.txt",$"Racer {Version}\nWindows x64 review / CR-057 through CR-060\nRules: street-v9-life / lake-v3-shallows; development=false\nSource tree: SOURCE-SHA256.txt; completion commit recorded after validation\nUnity {Application.unityVersion}\n");
            Directory.CreateDirectory(output+"/Licenses");foreach(var notice in Directory.GetFiles("Assets/Plugins/LocalRadio","*.txt"))File.Copy(notice,output+"/Licenses/"+Path.GetFileName(notice),true);
        }
    }
}
