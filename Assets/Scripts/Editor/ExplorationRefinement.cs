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
        public static void Refine()
        {
            wood=Mat("Weathered timber",new(.25f,.15f,.075f));leaf=Mat("Mountain foliage",new(.16f,.28f,.105f));leaf.SetFloat("_Smoothness",0);wood.SetFloat("_Smoothness",0);
            foreach(var path in ReverseReviewRelease.Scenes)
            {
                var scene=EditorSceneManager.OpenScene(path);race=Object.FindAnyObjectByType<RaceDirector>();var collection=race.GetComponent<ExplorationCollection>();var route=collection.routes[0];route.Initialize();var terrainRoot=GameObject.Find("Memory loop - north is +Z").transform;
                foreach(var mf in terrainRoot.GetComponentsInChildren<MeshFilter>())
                {
                    var v=mf.sharedMesh.vertices;if(!v.Any(p=>p.x>795&&p.x<940&&p.z>-170&&p.z<-45))continue;var mesh=Object.Instantiate(mf.sharedMesh);bool changed=false;
                    for(int i=0;i<v.Length;i++){var p=mf.transform.TransformPoint(v[i]);if(p.x<795||p.x>940||p.z<-170||p.z>-45)continue;float s=route.Project(p,out float d);if(s<110||s>260||d>34)continue;var at=route.At(s,out _);float blend=(1-Smooth(16,34,d))*Smooth(110,145,s)*(1-Smooth(225,260,s));p.y=Mathf.Lerp(p.y,at.y,blend);v[i]=mf.transform.InverseTransformPoint(p);changed=true;}
                    if(!changed){Object.DestroyImmediate(mesh);continue;}mesh.vertices=v;mesh.RecalculateNormals();mesh.RecalculateBounds();mf.sharedMesh=SaveMesh(mesh,scene.name+"-wide-creek-"+mf.name);mf.GetComponent<MeshCollider>().sharedMesh=mf.sharedMesh;
                }
                Physics.SyncTransforms();root=GameObject.Find("CR081 mountain exploration").transform;
                foreach(var batch in root.Cast<Transform>().Where(t=>t.name.StartsWith("Mountain woods")).ToArray())
                {
                    foreach(var mesh in batch.GetComponentsInChildren<MeshFilter>())Object.DestroyImmediate(mesh.gameObject);
                    foreach(var collider in batch.GetComponentsInChildren<BoxCollider>().ToArray())
                    {
                        var p=new Vector3(collider.bounds.center.x,0,collider.bounds.center.z);float s=route.Project(p,out var d);
                        if((s>105&&s<270&&d<28)||d<12){Object.DestroyImmediate(collider.gameObject);continue;}
                        float h=collider.bounds.size.y/.72f;p.y=Phase6Buildings.Ground(p);collider.transform.position=p+Vector3.up*h*.36f;
                        var g=new GameObject("Grounded mountain crown",typeof(MeshFilter),typeof(MeshRenderer));g.transform.SetParent(batch);g.transform.position=p+Vector3.up*h*.36f;g.transform.localScale=new(3.8f,h*.64f,3.8f);g.GetComponent<MeshFilter>().sharedMesh=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Vegetation/Phase6/IrregularCrown.asset");g.GetComponent<Renderer>().sharedMaterial=leaf;
                        Part(batch,"Complete trunk visual",p+Vector3.up*h*.36f,new(.55f,h*.72f,.55f),wood);
                    }
                    Combine(batch,scene.name+"-refined-"+batch.name.Replace(' ','-'));
                }
                var house=GameObject.Find("Dan - blue X").transform;var woods=GameObject.Find("Woods replacing later subdivisions").transform;
                foreach(var c in woods.GetComponentsInChildren<BoxCollider>().ToArray())
                {
                    var p=new Vector3(c.bounds.center.x,c.bounds.min.y,c.bounds.center.z);var q=house.InverseTransformPoint(p);
                    if(q.z<11&&q.z>-49&&Mathf.Abs(q.x)<25){Object.DestroyImmediate(c.gameObject);continue;}
                    if(Vector3.ProjectOnPlane(p-house.position,Vector3.up).magnitude<85)c.transform.position+=Vector3.up*(Phase6Buildings.Ground(p)-p.y);
                }
                RebuildMountainTrees(race,new List<string>());
                // The creek is visual and shallow beneath the authored gap; it adds no slab.
                var site=Object.FindObjectsByType<ActivitySite>().First(s=>s.id=="mountain-creek");var creek=Part(root,"Fern creek bed",site.transform.position+Vector3.up*-4,new(38,.03f,1.5f),Mat("Creek water",new(.12f,.4f,.43f)));creek.rotation=Quaternion.LookRotation(site.forward);creek.gameObject.AddComponent<ShallowWater>();
                var rows=new List<string>();foreach(var t in new[]{house,GameObject.Find("Original house 2").transform,GameObject.Find("Original house 3").transform,GameObject.Find("Friend across street - blue circle").transform})rows.Add(t.name+" "+t.position);
                File.WriteAllLines(Evidence+"/refinement-"+scene.name+".txt",rows.Prepend("Broader reverse creek landing support; coherent regrounded trees; no change to existing race ramps/gates."));
                EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets();
        }
    }
}
