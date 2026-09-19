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
        static double pendingAt;static string pending;
        public static void Queue(string job){pending=job;pendingAt=EditorApplication.timeSinceStartup+2;EditorApplication.update-=Work;EditorApplication.update+=Work;}
        static void Work()
        {
            if(EditorApplication.timeSinceStartup<pendingAt)return;EditorApplication.update-=Work;
            try{switch(pending){case "author":AuthorReverse();break;case "wildlife":AuthorWildlife();break;case "signs":CorrectMarkerFaces();break;case "build":Build();break;default:throw new Exception("Unknown job");}File.WriteAllText(Evidence+"/"+pending+"-done.txt","Complete");}
            catch(Exception e){File.WriteAllText(Evidence+"/"+pending+"-done.txt",e.ToString());Debug.LogException(e);}
        }
        public static void CorrectMarkerFaces()
        {
            Guard();var report=new List<string>();
            foreach(var path in Scenes.Skip(2))
            {
                var scene=EditorSceneManager.OpenScene(path);var branches=Object.FindObjectsByType<WoodlandRoute>();
                foreach(var label in Object.FindObjectsByType<TextMesh>().Where(t=>t.name=="Reverse physical sign text"))
                {
                    var sign=label.transform.parent;float nearest=float.MaxValue;Vector3 direction=Vector3.forward;
                    foreach(var b in branches){float s=b.Project(sign.position,out float lateral);if(lateral<nearest){nearest=lateral;b.At(s,out direction);}}
                    if(Vector3.Dot(sign.forward,direction)<0)sign.rotation*=Quaternion.Euler(0,180,0);
                    FitMarkerLabel(label);
                    report.Add(scene.name+" "+label.text+" approach dot="+Vector3.Dot(sign.forward,direction));
                }
                EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
                var camera=Camera.main;var oldPosition=camera.transform.position;var oldRotation=camera.transform.rotation;var oldTarget=camera.targetTexture;var oldActive=RenderTexture.active;
                var target=new RenderTexture(1280,720,24);var texture=new Texture2D(1280,720,TextureFormat.RGB24,false);
                try
                {
                    camera.targetTexture=target;
                    foreach(var b in branches)
                    {
                        var sign=Object.FindObjectsByType<TextMesh>().First(t=>t.text==b.title+" / OPTIONAL").transform.parent;
                        camera.transform.position=b.At(10,out _)+Vector3.up*2.6f;camera.transform.LookAt(sign.position);
                        camera.Render();RenderTexture.active=target;texture.ReadPixels(new Rect(0,0,1280,720),0,0);texture.Apply();
                        File.WriteAllBytes(Evidence+"/entrance-"+b.title.Replace(' ','-')+".png",texture.EncodeToPNG());
                    }
                }
                finally{camera.targetTexture=oldTarget;RenderTexture.active=oldActive;camera.transform.SetPositionAndRotation(oldPosition,oldRotation);Object.DestroyImmediate(target);Object.DestroyImmediate(texture);EditorSceneManager.SaveScene(scene);}
            }
            AssetDatabase.SaveAssets();File.WriteAllLines(Evidence+"/marker-facing.txt",report);
        }
        public static void AuthorWildlife()
        {
            Guard();AssetDatabase.Refresh();var report=new List<string>();
            foreach(var path in Scenes)
            {
                var scene=EditorSceneManager.OpenScene(path);var race=Object.FindAnyObjectByType<RaceDirector>();var wildlife=race.GetComponent<Wildlife>();
                var sites=wildlife.habitats.Where(h=>h.species<Wildlife.Species.Deer).ToList();var road=race.road;road.Initialize();
                float[] stations=race.Forest?new[]{170f,530f,820f,1190f,1500f,1800f}:new[]{520f,820f,1070f,1450f,2130f,2420f};
                for(int i=0;i<stations.Length;i++)
                {
                    var center=road.At(stations[i],out var f);var away=Vector3.Cross(Vector3.up,f).normalized*(i%2==0?1:-1);
                    for(int attempt=0;attempt<6;attempt++)
                    {
                        var p=center+away*(12+attempt*3);if(!Physics.Raycast(p+Vector3.up*45,Vector3.down,out var hit,100,1,QueryTriggerInteraction.Ignore)||hit.normal.y<.72f)continue;
                        p.y=hit.point.y+.03f;if(Physics.OverlapSphere(p+Vector3.up,1.3f,1,QueryTriggerInteraction.Ignore).Any(c=>!(c is MeshCollider)))continue;
                        var species=i%2==0?Wildlife.Species.Deer:Wildlife.Species.Coyote;
                        sites.Add(new Wildlife.Habitat{species=species,position=p,escape=away});report.Add(scene.name+" "+species+" "+p.ToString("F3"));break;
                    }
                }
                if(sites.Count(h=>h.species==Wildlife.Species.Deer)<2||sites.Count(h=>h.species==Wildlife.Species.Coyote)<2)throw new Exception("Insufficient woodland habitat "+scene.name);
                wildlife.habitats=sites.ToArray();
                wildlife.deerCalls=Enumerable.Range(1,2).Select(i=>AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Wildlife/Deer-original-"+i+".wav")).ToArray();
                wildlife.coyoteCalls=Enumerable.Range(1,2).Select(i=>AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Wildlife/Coyote-original-"+i+".wav")).ToArray();
                if(wildlife.deerCalls.Concat(wildlife.coyoteCalls).Any(c=>!c))throw new Exception("Missing imported audio");
                EditorUtility.SetDirty(wildlife);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets();File.WriteAllLines(Evidence+"/habitats.txt",report);
        }
        public static void Build()
        {
            Guard();PlayerSettings.bundleVersion=Version;EditorUserBuildSettings.development=EditorUserBuildSettings.allowDebugging=EditorUserBuildSettings.connectProfiler=false;
            EditorBuildSettings.scenes=Scenes.Select(p=>new EditorBuildSettingsScene(p,true)).ToArray();
            string output="Builds/Racer-"+Version+"-Windows";Directory.CreateDirectory(output);
            var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=Scenes,locationPathName=output+"/Racer.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
            File.WriteAllText(Evidence+"/build.txt",$"{result.summary.result}; errors={result.summary.totalErrors}; warnings={result.summary.totalWarnings}; time={result.summary.totalTime}\n"+string.Join("\n",result.steps.SelectMany(s=>s.messages).Where(m=>m.type==LogType.Error||m.type==LogType.Warning).Select(m=>m.content)));
            if(result.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Build failed");
            File.WriteAllText(output+"/VERSION.txt",$"Racer {Version}\nWindows x64 review / CR-070 through CR-074\nRules: street-v10-hairpin / lake-v3-shallows / street-reverse-v1 / forest-reverse-v1; development=false\nSafety checkpoint: 90abc69769528600ad652df1c3dbad985ae482d8\nSource tree: SOURCE-SHA256.txt; completion commit recorded after validation\nUnity {Application.unityVersion}\n");
            Directory.CreateDirectory(output+"/Licenses");foreach(var notice in Directory.GetFiles("Assets/Plugins/LocalRadio","*.txt"))File.Copy(notice,output+"/Licenses/"+Path.GetFileName(notice),true);
            File.Copy("Assets/Audio/Wildlife/LICENSE.txt",output+"/Licenses/Wildlife-CC0.txt",true);File.Copy("Assets/Audio/Wildlife/REVERSE-WILDLIFE-NOTICE.txt",output+"/Licenses/Reverse-wildlife-original.txt",true);
        }
    }
}
