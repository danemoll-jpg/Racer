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
    public static partial class ExplorationAuthoring
    {
        public static void FinishSupport()
        {
            foreach(var path in ReverseReviewRelease.Scenes)
            {
                var scene=EditorSceneManager.OpenScene(path);race=Object.FindAnyObjectByType<RaceDirector>();var collection=race.GetComponent<ExplorationCollection>();var rows=new List<string>{"route,station,x,y,z,obstacle"};
                var home=GameObject.Find("Dan - blue X").transform;
                foreach(var label in Object.FindObjectsByType<TextMesh>())
                    if(label.text=="TO Trickum Road"&&label.transform.position.x<-560&&label.transform.position.z<-530)label.text="TO Jamerson Road";
                var life=race.GetComponent<AmbientLife>();Vector3 GroundPerson(Vector3 p)=>new(p.x,Phase6Buildings.Ground(p)+.025f,p.z);
                life.football=life.football.Select(GroundPerson).ToArray();life.coffee=life.coffee.Select(GroundPerson).ToArray();life.smoking=life.smoking.Select(GroundPerson).ToArray();
                foreach(var fence in Object.FindObjectsByType<BreakableProp>().Where(p=>p.surface==SmashAudio.Surface.ChainLink||p.name.Contains("fence")||p.name.Contains("crossbuck")))
                    if(Vector3.ProjectOnPlane(fence.transform.position-home.position,Vector3.up).magnitude<85){var p=fence.transform.position;p.y=Phase6Buildings.Ground(p);fence.transform.position=p;}
                foreach(var route in collection.routes)
                {
                    for(int i=0;i<route.points.Length;i++){var p=route.points[i];p.y=Phase6Buildings.Ground(p);route.points[i]=p;}
                    typeof(RaceRoad).GetField("distance",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).SetValue(route,null);route.Initialize();
                    for(float s=0;s<route.Length;s+=5)
                    {
                        var p=route.At(s,out var f);var q=Quaternion.LookRotation(Vector3.ProjectOnPlane(f,Vector3.up));
                        var obstacles=Physics.OverlapBox(p+Vector3.up*1.1f,new Vector3(1,.7f,1.5f),q,~0,QueryTriggerInteraction.Ignore).Where(c=>!c.name.StartsWith("Ground_")&&!c.attachedRigidbody).Select(c=>c.name).Distinct();
                        rows.Add($"{route.name},{s:F1},{p.x:F2},{p.y:F2},{p.z:F2},{string.Join(";",obstacles)}");
                    }
                }
                File.WriteAllLines(Evidence+"/support-audit-"+scene.name+".csv",rows);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
                rows=new List<string>{"lip,station,lateral,x,y,z,nx,ny,nz,collider"};var mountain=collection.routes[0];
                foreach(float lip in new[]{213f,537f})for(float s=lip-80;s<=lip+100;s+=2)foreach(float lateral in new[]{-16f,-7f,0f,7f,16f})
                {var p=mountain.At(s,out var f)+Vector3.Cross(Vector3.up,f).normalized*lateral;var h=Physics.RaycastAll(new Vector3(p.x,300,p.z),Vector3.down,600).Where(h=>h.collider.name.StartsWith("Ground_")).OrderBy(h=>h.distance).First();rows.Add($"{lip},{s},{lateral},{p.x:F3},{h.point.y:F3},{p.z:F3},{h.normal.x:F3},{h.normal.y:F3},{h.normal.z:F3},{h.collider.name}");}
                File.WriteAllLines(Evidence+"/authored-ramp-geometry-"+scene.name+".csv",rows);
            }
            AssetDatabase.SaveAssets();
        }
        public static void ProtectCourses()
        {
            foreach(var path in ReverseReviewRelease.Scenes)
            {
                string name=Path.GetFileNameWithoutExtension(path);EditorSceneManager.OpenScene("Assets/ValidationBaseline/"+name+".unity");
                var baseline=GameObject.Find("Memory loop - north is +Z").GetComponentsInChildren<MeshFilter>().ToDictionary(f=>f.name,f=>f.sharedMesh.vertices);
                var scene=EditorSceneManager.OpenScene(path);race=Object.FindAnyObjectByType<RaceDirector>();var routes=new List<(Vector3[],float)>();routes.Add((race.road.points,race.Forest?10:14));if(race.ambientRoad)routes.Add((race.ambientRoad.points,14));foreach(var b in Object.FindObjectsByType<WoodlandRoute>())routes.Add((b.points,b.halfWidth+5));
                int restored=0;float largest=0;
                foreach(var mf in GameObject.Find("Memory loop - north is +Z").GetComponentsInChildren<MeshFilter>())
                {
                    if(!baseline.TryGetValue(mf.name,out var old))continue;var vertices=mf.sharedMesh.vertices;if(vertices.Length!=old.Length)throw new Exception("Original terrain topology mismatch "+mf.name);bool changed=false;
                    for(int i=0;i<vertices.Length;i++)
                    {
                        if(Mathf.Abs(vertices[i].y-old[i].y)<.001f)continue;var p=mf.transform.TransformPoint(vertices[i]);float blend=0;
                        foreach(var r in routes){float d=Near(p,r.Item1,out _);blend=Mathf.Max(blend,1-Smooth(r.Item2,r.Item2+10,d));}
                        if(blend<=0)continue;largest=Mathf.Max(largest,Mathf.Abs(vertices[i].y-old[i].y));vertices[i].y=Mathf.Lerp(vertices[i].y,old[i].y,blend);changed=true;restored++;
                    }
                    if(!changed)continue;var mesh=Object.Instantiate(mf.sharedMesh);mesh.vertices=vertices;mesh.RecalculateNormals();mesh.RecalculateBounds();mf.sharedMesh=SaveMesh(mesh,name+"-course-protected-"+mf.name);mf.GetComponent<MeshCollider>().sharedMesh=mf.sharedMesh;
                }
                Physics.SyncTransforms();var collection=race.GetComponent<ExplorationCollection>();foreach(var site in collection.sites)site.position=new(site.position.x,Phase6Buildings.Ground(site.position)+1,site.position.z);
                File.WriteAllText(Evidence+"/route-protection-"+name+".txt",$"Restored {restored} changed terrain vertices to safety-checkpoint support near all race/ambient roads and legal shortcut corridors; largest prior displacement={largest:F3}m. Route arrays, gates and ramp meshes unchanged.\n");
                EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets();
        }
    }
}
