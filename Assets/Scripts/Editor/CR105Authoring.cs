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
    public static class CR105Authoring
    {
        public const string Evidence="Docs/CR105-111";
        public static void MailboxAudit(){EditorSceneManager.OpenScene("Assets/Scenes/StreetLoopGreybox.unity");var t=GameObject.Find("Mailbox - Original house 3").transform;File.WriteAllLines(Evidence+"/mailbox3-ground.txt",new[]{"mailbox="+t.position+" house="+GameObject.Find("Original house 3").transform.position}.Concat(Physics.RaycastAll(t.position+Vector3.up*100,Vector3.down,200).OrderBy(h=>h.distance).Select(h=>h.collider.name+" "+h.point)).Concat(t.GetComponentsInChildren<Renderer>().Select(r=>r.name+" bounds="+r.bounds)));}
        public static void Mailbox3Shoulder(){foreach(var path in ReverseReviewRelease.Scenes.Concat(new[]{"Assets/Scenes/MountainLoop.unity","Assets/Scenes/MountainLoopReverse.unity"})){var scene=EditorSceneManager.OpenScene(path);var t=GameObject.Find("Mailbox - Original house 3").transform;var p=new Vector3(502.56f,81.76f,-126.82f)+t.forward*2;p.y=Ground(p);t.position=p;PrefabUtility.RecordPrefabInstancePropertyModifications(t);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);}}
        public static float Ground(Vector3 p)=>Physics.RaycastAll(new(p.x,400,p.z),Vector3.down,800,1,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name.StartsWith("Ground_")||h.collider.name=="Decorative Road pavement").Select(h=>h.point.y).DefaultIfEmpty(p.y).Max();
        public static void World()
        {
            Directory.CreateDirectory(Evidence);var rows=new List<string>();
            foreach(string path in ReverseReviewRelease.Scenes)
            {
                var scene=EditorSceneManager.OpenScene(path);var race=Object.FindAnyObjectByType<RaceDirector>();var street=race.ambientRoad?race.ambientRoad:race.road;
                foreach(var boundary in Object.FindObjectsByType<CircuitBoundary>())
                {
                    foreach(Transform t in boundary.transform.Cast<Transform>().ToArray())if(t.name.Contains("Closure")||t.name=="Solid race closure"||t.name=="Event gantry"||t.name=="Race boundary sign")Object.DestroyImmediate(t.gameObject);
                    if(boundary.name.StartsWith("Hwy")){var traffic=boundary.GetComponent<ContinuationTraffic>();if(traffic){foreach(var car in traffic.cars)if(car)Object.DestroyImmediate(car.gameObject);Object.DestroyImmediate(traffic);}}
                    rows.Add(scene.name+" open continuation "+boundary.name+" at "+boundary.transform.position);
                }
                if(!race.throughRoad)
                {
                    var west=GameObject.Find("Hwy 92 west").transform;var east=GameObject.Find("Hwy 92 east").transform;var points=new List<Vector3>();
                    for(float z=390;z>=0;z-=5){var p=west.TransformPoint(new Vector3(0,0,z));p.y=Ground(p)+.03f;points.Add(p);}
                    // Preserve the highway centerline, replacing only its turns onto the circuit.
                    for(float s=3780;s<=4600;s+=5)points.Add(street.At(s,out _));
                    for(float z=0;z<=390;z+=5){var p=east.TransformPoint(new Vector3(0,0,z));p.y=Ground(p)+.03f;points.Add(p);}
                    race.throughRoad=new GameObject("CR105 highway through route").AddComponent<RaceRoad>();race.throughRoad.points=points.ToArray();race.throughRoad.openHighway=true;race.throughRoad.Initialize();
                }
                foreach(string name in new[]{"Dan - blue X","Original house 2","Original house 3"})
                {
                    var mailbox=GameObject.Find("Mailbox - "+name).transform;var house=GameObject.Find(name).transform;
                    var reference=name=="Original house 3"?mailbox.position:house.position;float station=street.Project(reference,out _)+12;var center=street.At(station,out var f);var side=Vector3.Cross(Vector3.up,f).normalized;if(Vector3.Dot(side,house.position-center)<0)side=-side;
                    var p=center+side*9;p.y=Ground(p);mailbox.SetPositionAndRotation(p,Quaternion.LookRotation(-side));PrefabUtility.RecordPrefabInstancePropertyModifications(mailbox);rows.Add(scene.name+" "+mailbox.name+" "+p);
                }
                var wildlife=race.GetComponent<Wildlife>();if(!wildlife.habitats.Any(h=>h.species==Wildlife.Species.Turkey))
                {var home=GameObject.Find("Friend across street - blue circle").transform;var habitats=wildlife.habitats.ToList();for(int i=0;i<3;i++){var p=home.TransformPoint(new Vector3(19+i*2,0,-11));p.y=Ground(p)+.03f;habitats.Add(new Wildlife.Habitat{species=Wildlife.Species.Turkey,position=p,escape=home.right});}wildlife.habitats=habitats.ToArray();}
                EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets();File.WriteAllLines(Evidence+"/world-authoring.txt",rows);
        }
    }
}
