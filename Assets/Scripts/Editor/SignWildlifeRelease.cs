using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;
namespace Racer.Editor
{
    public static class SignWildlifeRelease
    {
        public const string Evidence="Docs/CR061-062", Version="0.9.1-review1";
        [Serializable] public class Letter { public string parent,name,text,material; public Vector3 position,scale,parentWorld; public Quaternion rotation; public float size,line; public int fontSize,anchor,alignment; public Color color; }
        [Serializable] public class Catalog { public Letter[] letters; }
        static string PathOf(Transform t)=>t.parent?PathOf(t.parent)+"/"+t.name:t.name;
        static bool HistoricalSign(TextMesh t)=>t.name=="Road lettering"||t.name=="Shortcut advice"||t.name=="Recommended speed"||t.name=="Fictional storefront identity";
        public static void RecoverHistory()
        {
            if(Application.isPlaying||SceneManager.GetActiveScene().isDirty)throw new Exception("Saved edit mode required");
            Directory.CreateDirectory(Evidence);Directory.CreateDirectory("Assets/Track/Signs");AssetDatabase.Refresh();
            foreach(string key in new[]{"Street","Forest"})
            {
                var history=EditorSceneManager.OpenScene("Assets/SignHistoryTemp/"+key+".unity",OpenSceneMode.Additive);
                var texts=history.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<TextMesh>(true)).ToArray();
                File.WriteAllLines(Evidence+"/history-"+key+".txt",texts.Select(t=>(HistoricalSign(t)?"MOUNTED ":"UNMOUNTED ")+PathOf(t.transform)+" | "+t.text.Replace('\n','|')));
                var catalog=new Catalog{letters=texts.Where(HistoricalSign).Select(t=>new Letter{parent=PathOf(t.transform.parent),parentWorld=t.transform.parent.position,name=t.name,text=t.text,position=t.transform.localPosition,rotation=t.transform.localRotation,scale=t.transform.localScale,size=t.characterSize,line=t.lineSpacing,fontSize=t.fontSize,anchor=(int)t.anchor,alignment=(int)t.alignment,color=t.color,material=AssetDatabase.GetAssetPath(t.GetComponent<Renderer>().sharedMaterial)}).ToArray()};
                File.WriteAllText("Assets/Track/Signs/"+key+".json",JsonUtility.ToJson(catalog,true));EditorSceneManager.CloseScene(history,true);
            }
            AssetDatabase.DeleteAsset("Assets/SignHistoryTemp");AssetDatabase.Refresh();ApplySigns();
        }
        public static void Restore(Scene scene)
        {
            string key=scene.path==LakeCourseBuild.ScenePath?"Forest":"Street";
            var asset=AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Track/Signs/"+key+".json");if(!asset)return;
            var all=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).ToArray();
            int restored=0;
            foreach(var row in JsonUtility.FromJson<Catalog>(asset.text).letters)
            {
                var parent=all.Where(t=>PathOf(t)==row.parent).OrderBy(t=>Vector3.Distance(t.position,row.parentWorld)).FirstOrDefault();if(!parent)throw new Exception("Missing historic sign parent "+row.parent);
                // Match by position as several parents have two identically named faces/advice signs.
                var text=parent.GetComponentsInChildren<TextMesh>(true).FirstOrDefault(t=>t.name==row.name&&t.text==row.text&&Vector3.Distance(parent.InverseTransformPoint(t.transform.position),row.position)<.01f);
                if(!text){text=new GameObject(row.name).AddComponent<TextMesh>();text.transform.SetParent(parent,false);text.transform.localPosition=row.position;text.transform.localRotation=row.rotation;text.transform.localScale=row.scale;restored++;}
                text.text=row.text;text.characterSize=row.size;text.lineSpacing=row.line;text.fontSize=row.fontSize;text.anchor=(TextAnchor)row.anchor;text.alignment=(TextAlignment)row.alignment;text.color=row.color;text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                text.GetComponent<Renderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>(row.material);
                if(!text.GetComponent<PhysicalSign>())text.gameObject.AddComponent<PhysicalSign>();
                // Some early signs used sibling text. Bind to the actual panel without changing world placement.
                if(row.name=="Shortcut advice"||row.name=="Recommended speed")
                {
                    string boardName=row.name=="Shortcut advice"?"Shortcut direction sign":"Jump approach sign";
                    var board=parent.GetComponentsInChildren<Transform>(true).Where(t=>t.name==boardName).OrderBy(t=>Vector3.Distance(t.position,text.transform.position)).FirstOrDefault();
                    if(board)text.transform.SetParent(board,true);
                }
            }
            foreach(var text in scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<TextMesh>(true)).ToArray())if(SceneryText.IsFloating(text))Object.DestroyImmediate(text.gameObject);
            if(scene.path==LakeCourseBuild.ScenePath)Warning(scene);
            File.AppendAllText(Evidence+"/restoration.txt",key+" restored="+restored+" catalog="+JsonUtility.FromJson<Catalog>(asset.text).letters.Length+"\n");
        }
        static void Warning(Scene scene)
        {
            
            var cave=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<WoodlandRoute>()).First();
            var p=cave.At(48,out var f);f=Vector3.ProjectOnPlane(f,Vector3.up).normalized;p+=Vector3.Cross(Vector3.up,f)*13;
            p.y=Phase6Buildings.Ground(p);var existing=scene.GetRootGameObjects().FirstOrDefault(g=>g.name=="Cave warning sign");if(existing){existing.transform.SetPositionAndRotation(p,Quaternion.LookRotation(f));existing.transform.Find("Mounted warning board").localScale=new Vector3(11,3.1f,.18f);existing.GetComponent<BoxCollider>().size=new Vector3(11,4,.5f);return;}var root=new GameObject("Cave warning sign");SceneManager.MoveGameObjectToScene(root,scene);root.transform.SetPositionAndRotation(p,Quaternion.LookRotation(f));root.AddComponent<PhysicalSign>();
            var amber=AssetDatabase.LoadAssetAtPath<Material>("Assets/Track/Woodland/Amber.mat");
            if(!amber)amber=AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Phase4Ramp.mat");
            void Box(string name,Vector3 at,Vector3 size){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(root.transform,false);g.transform.localPosition=at;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=amber;Object.DestroyImmediate(g.GetComponent<Collider>());}
            Box("Mounted warning board",new(0,3,0),new(11,3.1f,.18f));foreach(int side in new[]{-1,1})Box("Warning post",new(side*2.7f,1.7f,.08f),new(.18f,3.4f,.18f));
            var t=new GameObject("Cave warning lettering").AddComponent<TextMesh>();t.transform.SetParent(root.transform,false);t.transform.localPosition=new(0,3,-.105f);t.text="Warning: Cave Ahead\nEnter at your Own Risk";t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.fontSize=72;t.characterSize=.13f;t.anchor=TextAnchor.MiddleCenter;t.alignment=TextAlignment.Center;t.color=Color.white;t.GetComponent<Renderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Track/Woodland/Lettering.mat");
            var sensor=root.AddComponent<BoxCollider>();sensor.center=new(0,2,0);sensor.size=new(11,4,.5f);sensor.isTrigger=true;root.AddComponent<BreakableProp>().surface=SmashAudio.Surface.Sign;
        }
        public static void ApplySigns()
        {
            foreach(var path in new[]{StreetLoopBuilder.ScenePath,LakeCourseBuild.ScenePath})
            {
                var s=EditorSceneManager.OpenScene(path);
                foreach(var t in Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include).Where(t=>HistoricalSign(t)&&t.name!="Fictional storefront identity").ToArray())Object.DestroyImmediate(t.gameObject);
                Restore(s);EditorSceneManager.MarkSceneDirty(s);EditorSceneManager.SaveScene(s);
            }
            AssetDatabase.SaveAssets();
        }
        public static void ApplyWildlife()
        {
            AssetDatabase.Refresh();
            foreach(string file in Directory.GetFiles("Assets/Audio/Wildlife","*.wav"))
            {
                var importer=(AudioImporter)AssetImporter.GetAtPath(file.Replace('\\','/'));importer.forceToMono=true;var settings=importer.defaultSampleSettings;settings.preloadAudioData=true;settings.loadType=AudioClipLoadType.DecompressOnLoad;settings.compressionFormat=AudioCompressionFormat.PCM;importer.defaultSampleSettings=settings;importer.SaveAndReimport();
            }
            foreach(var path in new[]{StreetLoopBuilder.ScenePath,LakeCourseBuild.ScenePath})
            {
                var scene=EditorSceneManager.OpenScene(path);var race=Object.FindAnyObjectByType<RaceDirector>();race.road.Initialize();if(race.ambientRoad)race.ambientRoad.Initialize();
                var wildlife=race.GetComponent<Wildlife>()??race.gameObject.AddComponent<Wildlife>();
                AudioClip Clip(string name)=>AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Wildlife/"+name+".wav");
                wildlife.birdCalls=new[]{Clip("bird-1"),Clip("bird-2")};wildlife.squirrelCalls=new[]{Clip("squirrel-1"),Clip("squirrel-2")};wildlife.frogCalls=new[]{Clip("frog-1"),Clip("frog-2")};wildlife.batFlight=Clip("bat-flight");
                var slots=new List<Wildlife.Habitat>();var cave=race.Forest?Object.FindAnyObjectByType<WoodlandRoute>():null;
                bool Add(Vector3 p,Wildlife.Species species,Vector3 escape)
                {
                    if(!Physics.Raycast(p+Vector3.up*150,Vector3.down,out var ground,350,1,QueryTriggerInteraction.Ignore))return false;p=ground.point+Vector3.up*.03f;
                    race.road.Project(p,out float d);if(d<race.road.HalfWidth(race.road.Project(p,out _))+3)return false;
                    if(race.ambientRoad){race.ambientRoad.Project(p,out float sd);if(sd<12)return false;}
                    if(cave){cave.Project(p,out float cd);if(cd<15)return false;}
                    if(Physics.CheckSphere(p+Vector3.up*.55f,.5f,~(1<<2),QueryTriggerInteraction.Ignore))return false;
                    if(slots.Any(h=>Vector3.Distance(h.position,p)<(species==Wildlife.Species.Frog?9:32)))return false;
                    slots.Add(new Wildlife.Habitat{species=species,position=p,escape=Vector3.ProjectOnPlane(escape,Vector3.up).normalized});return true;
                }
                for(int i=0;i<12;i++)
                {
                    float station=(race.Forest?140:250)+i*(race.Forest?145:240);var p=race.road.At(station,out var f);var side=Vector3.Cross(Vector3.up,f).normalized*(i%2==0?1:-1);
                    for(int j=0;j<5;j++)if(Add(p+side*(race.Forest?11+j*2:15+j*2),i%2==0?Wildlife.Species.Bird:Wildlife.Species.Squirrel,side))break;
                }
                var water=Object.FindObjectsByType<ShallowWater>().OrderByDescending(w=>w.round).First();
                // Dry shoreline with a readable view from nearby road/trail; no frogs under water.
                var shoreline=Enumerable.Range(0,48).Select(i=>
                {
                    float angle=i*Mathf.PI*2/48;var side=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle));var p=water.transform.position+(water.round?side*72:water.transform.rotation*new Vector3(side.x*18,0,side.z*8));race.road.Project(p,out float distance);return (p,side,distance);
                }).OrderBy(x=>x.distance);
                foreach(var candidate in shoreline)
                {
                    var p=candidate.p;var side=candidate.side;
                    race.road.Project(p,out float distance);if(distance>(race.Forest?65:110))continue;
                    float bankHeight=Phase6Buildings.Ground(p);if(bankHeight<=water.Surface+.03f||bankHeight>water.Surface+2)continue;
                    if(Add(p,Wildlife.Species.Frog,water.round?side:water.transform.rotation*side)&&slots.Count(h=>h.species==Wildlife.Species.Frog)>=3)break;
                }
                if(slots.Count(h=>h.species==Wildlife.Species.Frog)<2)throw new Exception("Insufficient dry shoreline frog habitat "+scene.name);
                wildlife.habitats=slots.ToArray();EditorUtility.SetDirty(wildlife);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
                File.WriteAllLines(Evidence+"/habitats-"+scene.name+".txt",slots.Select(h=>$"{h.species} {h.position:F3} away={h.escape:F3}"));
            }
            AssetDatabase.SaveAssets();
        }
        static double buildAt;
        public static void QueueBuild(){buildAt=EditorApplication.timeSinceStartup+2;EditorApplication.update-=Tick;EditorApplication.update+=Tick;}
        static void Tick(){if(EditorApplication.timeSinceStartup<buildAt)return;EditorApplication.update-=Tick;try{Build();File.WriteAllText(Evidence+"/build-done.txt","Complete");}catch(Exception e){File.WriteAllText(Evidence+"/build-done.txt",e.ToString());}}
        public static void Build()
        {
            if(Application.isPlaying||SceneManager.GetActiveScene().isDirty)throw new Exception("Saved edit mode required");
            PlayerSettings.bundleVersion=Version;EditorUserBuildSettings.development=EditorUserBuildSettings.allowDebugging=EditorUserBuildSettings.connectProfiler=false;
            string output="Builds/Racer-"+Version+"-Windows";Directory.CreateDirectory(output);
            var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{StreetLoopBuilder.ScenePath,LakeCourseBuild.ScenePath},locationPathName=output+"/Racer.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
            File.WriteAllText(Evidence+"/build.txt",$"{result.summary.result}; errors={result.summary.totalErrors}; warnings={result.summary.totalWarnings}; time={result.summary.totalTime}\n"+string.Join("\n",result.steps.SelectMany(s=>s.messages).Where(m=>m.type==LogType.Error||m.type==LogType.Warning).Select(m=>m.content)));
            if(result.summary.result!=BuildResult.Succeeded)throw new Exception("Build failed");
            File.WriteAllText(output+"/VERSION.txt",$"Racer {Version}\nWindows x64 review / CR-061 and CR-062\nRules: street-v9-life / lake-v3-shallows; development=false\nSource tree: SOURCE-SHA256.txt; completion commit recorded after validation\nUnity {Application.unityVersion}\n");
            Directory.CreateDirectory(output+"/Licenses");foreach(var notice in Directory.GetFiles("Assets/Plugins/LocalRadio","*.txt"))File.Copy(notice,output+"/Licenses/"+Path.GetFileName(notice),true);
            File.Copy("Assets/Audio/Wildlife/LICENSE.txt",output+"/Licenses/Wildlife-CC0.txt",true);
        }
    }
    // Scene processing runs for builds and play mode without altering source scene files.
    public sealed class PreservePhysicalSigns : IProcessSceneWithReport
    {
        public int callbackOrder=>100;
        public void OnProcessScene(Scene scene,BuildReport report){if(scene.path==StreetLoopBuilder.ScenePath||scene.path==LakeCourseBuild.ScenePath)SignWildlifeRelease.Restore(scene);}
    }
}






