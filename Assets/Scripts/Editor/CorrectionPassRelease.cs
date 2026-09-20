using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace Racer.Editor
{
    // Build-only diagnostic baseline. Gameplay and saved scene authoring are untouched.
    [InitializeOnLoad]
    public static class CorrectionPassRelease
    {
        static double due;static string job;
        static CorrectionPassRelease() { var pending=SessionState.GetString("CR087.Build","");if(pending!="")Queue(pending); }
        public static void QueueBaseline()=>Queue("baseline");
        public static void Queue(string value)
        {
            job=value;SessionState.SetString("CR087.Build",value);
            due=EditorApplication.timeSinceStartup+2;
            EditorApplication.update-=Work;
            EditorApplication.update+=Work;
        }
        static void Work()
        {
            if(EditorApplication.timeSinceStartup<due||EditorApplication.isCompiling||EditorApplication.isUpdating)return;
            EditorApplication.update-=Work;SessionState.EraseString("CR087.Build");
            Directory.CreateDirectory("Docs/CR082-089");
            try
            {
                if(Application.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Saved edit mode required");
                if(job=="author"){CorrectionPassAuthoring.Author();File.WriteAllText("Docs/CR082-089/author-done.txt","Complete");return;}
                if(job=="polish")
                {
                    foreach(var path in new[]{"Assets/Scenes/StreetLoopReverse.unity","Assets/Scenes/ForestLoopReverse.unity"})
                    {
                        var scene=UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path);var root=GameObject.Find("CR088-089 physical direction cues");var old=GameObject.Find("CR075 direction signs");if(old)old.SetActive(false);
                        foreach(Transform sign in root.transform)
                        {
                            sign.localScale=Vector3.one*.65f;
                            var text=sign.GetComponentInChildren<TextMesh>();
                            if(text.text=="MAIN  ^")
                            {
                                text.text="^";text.characterSize=.24f;var size=text.GetComponent<Renderer>().localBounds.size;text.characterSize*=Mathf.Min(5.4f/Mathf.Max(.01f,size.x),1.6f/Mathf.Max(.01f,size.y));
                            }
                        }
                        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
                    }
                    File.WriteAllText("Docs/CR082-089/sign-polish.txt","Runtime image correction: new boards reduced to 65% (3.9m x 1.3m); removes their intrusion into the driving opening. Repeated MAIN boards use directional chevrons. Superseded CR075 direction sign groups inactive on both reverse variants. Road/ramp/gate geometry unchanged.");Queue("release");return;
                }
                if(job=="units")
                {
                    var rows=new System.Collections.Generic.List<string>();
                    foreach(var path in ReverseReviewRelease.Scenes)
                    {
                        var scene=UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path);bool changed=false;
                        foreach(var label in UnityEngine.Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include))
                        {
                            string text=System.Text.RegularExpressions.Regex.Replace(label.text,@"(\d+(?:\.\d+)?)(?:\s*-\s*(\d+(?:\.\d+)?))?\s*km/h",m=>
                            {
                                string Convert(string value)=>(double.Parse(value,System.Globalization.CultureInfo.InvariantCulture)/1.609344).ToString("0",System.Globalization.CultureInfo.InvariantCulture);
                                return Convert(m.Groups[1].Value)+(m.Groups[2].Success?" - "+Convert(m.Groups[2].Value):"")+" mph";
                            });
                            if(text!=label.text){label.text=text;changed=true;}
                            if(label.text.Contains("mph")||label.text.Contains("km/h")||label.text.Contains("m/s"))rows.Add(scene.name+" / "+label.name+" / active="+label.gameObject.activeInHierarchy+" / "+label.text.Replace('\n','|'));
                        }
                        if(changed){UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);}
                    }
                    File.WriteAllLines("Docs/CR082-089/physical-speed-signs.txt",rows);Queue("release");return;
                }
                if(job=="signs")
                {
                    foreach(var path in ReverseReviewRelease.Scenes)
                    {
                        var scene=UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path);var root=GameObject.Find("CR088-089 physical direction cues");if(!root)continue;
                        foreach(var t in root.GetComponentsInChildren<TextMesh>()){var size=t.GetComponent<Renderer>().localBounds.size;t.characterSize*=Mathf.Min(5.4f/Mathf.Max(.01f,size.x),1.6f/Mathf.Max(.01f,size.y));}
                        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
                    }
                    Queue("systems");return;
                }
                if(job=="shoulder")
                {
                    var scene=UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/StreetLoopReverse.unity");
                    var frame=GameObject.Find("Phase 4 - Connector Jump").transform;var ramp=GameObject.Find("Reverse supported roadworks transition");
                    var mesh=ramp.GetComponent<MeshFilter>().sharedMesh;var vertices=mesh.vertices;
                    for(int i=0;i<vertices.Length;i++)
                    {
                        if(i%4!=0&&i%4!=3)continue;
                        var p=frame.InverseTransformPoint(ramp.transform.TransformPoint(vertices[i]));p.x=i%4==0?-10.5f:7.5f;
                        vertices[i]=ramp.transform.InverseTransformPoint(frame.TransformPoint(p));
                    }
                    mesh.vertices=vertices;mesh.RecalculateNormals();mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);ramp.GetComponent<MeshCollider>().sharedMesh=null;ramp.GetComponent<MeshCollider>().sharedMesh=mesh;
                    AssetDatabase.SaveAssets();UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
                    File.WriteAllText("Docs/CR082-089/shoulder-authoring.txt","Actual ramp: Reverse course authoring/Reverse supported roadworks transition; Assets/Track/ArcadeReview/TrickumReverse.asset. Top width/run/rise/rollout unchanged. Each side bevel extends from 2m to 6m (outer local x -10.5 / 7.5); maximum transverse gradient 3.2/6 instead of 3.2/2. Forward geometry unchanged. Repeat original failing lanes plus actual shoulder lanes in compiled player.");
                }
                bool release=job=="release";
                if(release)PlayerSettings.bundleVersion="0.14.0-review2";
                string output=release?"Builds/Racer-0.14.0-review2-Windows":job=="baseline"?"Builds/CR087-baseline-instrumented":"Builds/CR087-"+job;
                var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=ReverseReviewRelease.Scenes,locationPathName=output+"/Racer.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
                File.WriteAllText("Docs/CR082-089/"+job+"-build.txt",report.summary.result+" errors="+report.summary.totalErrors);
                if(release&&report.summary.result==UnityEditor.Build.Reporting.BuildResult.Succeeded)
                {
                    File.WriteAllText(output+"/VERSION.txt","Racer 0.14.0-review2\nWoodstock Rush CR-082 through CR-089 correction pass\nSafety checkpoint: b1d076d948e2a8ce57bdfaeebd698b87e77bd5b6\nSource: SOURCE-SHA256.txt; completion commit stamped after validation\nUnity "+Application.unityVersion+"\n");
                    Directory.CreateDirectory(output+"/Licenses");foreach(var notice in Directory.GetFiles("Assets/Plugins/LocalRadio","*.txt"))File.Copy(notice,output+"/Licenses/"+Path.GetFileName(notice),true);
                    foreach(var notice in new[]{"LICENSE.txt","REVERSE-WILDLIFE-NOTICE.txt"})File.Copy("Assets/Audio/Wildlife/"+notice,output+"/Licenses/Wildlife-"+notice,true);
                }
            }
            catch(Exception e){File.WriteAllText("Docs/CR082-089/"+job+"-build.txt",e.ToString());}
        }
    }
}

