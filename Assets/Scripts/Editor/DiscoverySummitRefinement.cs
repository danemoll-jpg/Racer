using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Object=UnityEngine.Object;
namespace Racer.Editor
{
    public static partial class DiscoveryAuthoring
    {
        public static float RefinedSummitHeight(float s){if(s<30)return 145+s*.08f;if(s<=100){float d=s-30;return 147.4f+.08f*d+.0052f*d*d;}if(s<132)return Mathf.Lerp(178.48f,154.5f,Smooth(100,132,s));return 154.5f-(s-132)*.015f;}
        public static void RefineSummit()
        {
            foreach(var path in ReverseReviewRelease.Scenes){EditorSceneManager.OpenScene(path);var race=Object.FindAnyObjectByType<RaceDirector>();var collection=race.GetComponent<ExplorationCollection>();var root=GameObject.Find("CR094 summit launch").transform;
                var exit=new[]{new Vector3(810,RefinedSummitHeight(260),190),new(785,151,220),new(805,151,250),new(850,151.5f,235),new(850,151.5f,190),new(875,151,175),new(901.34f,150.48f,153.63f)};
                Terrain("summit-landing-v3",p=>{if(p.x<700||p.x>1100||p.z<120||p.z>290)return p;float s=1070-p.x,side=Mathf.Abs(p.z-190);float width=s>110?26:16;float blend=Smooth(-25,0,s)*(1-Smooth(320,360,s))*(1-Smooth(width,width+18,side));
                    if(blend>0)foreach(var old in collection.routes.Take(2))if(Near(p,old.points,out _)<13)blend=0;if(blend>0)p.y=Mathf.Lerp(p.y,RefinedSummitHeight(s),blend);
                    float d=Near(p,exit,out var at);if(d<14)p.y=Mathf.Lerp(p.y,at.y,1-Smooth(6,14,d));return p;});
                var mf=root.GetComponentsInChildren<MeshFilter>().Single(m=>m.name=="Ground_Summit authored launch");var mesh=Object.Instantiate(mf.sharedMesh);var v=mesh.vertices;for(int i=0;i<v.Length;i++)v[i].y=RefinedSummitHeight(v[i].z)-145+.06f;mesh.vertices=v;mesh.RecalculateNormals();mesh.RecalculateBounds();mf.sharedMesh=MeshAsset(mesh,"summit-launch-v2");mf.GetComponent<MeshCollider>().sharedMesh=mf.sharedMesh;
                var site=root.GetComponentInChildren<ActivitySite>();site.transform.localPosition=new(0,RefinedSummitHeight(100)-145,100);
                var route=collection.routes.Single(r=>r.name=="Summit approach and safe return");route.points=route.points.Take(5).Concat(Enumerable.Range(1,26).Select(i=>new Vector3(1070-i*10,RefinedSummitHeight(i*10),190))).Concat(exit.Skip(1)).ToArray();
                ClearTrees(p=>Near(p,exit,out _)<9||p.x>720&&p.x<1100&&Mathf.Abs(p.z-190)<38);
                File.WriteAllText(Evidence+"/summit-refined-"+race.gameObject.scene.name+".txt","CR098 first corrected geometry: launch tangent continuous from .08 to .808; lip (970,178.48,190). Landing corridor begins after s132 at154.5m with .015 downhill grade, 52m full width, extended rollout to s320. Return loops north at runway height via (805,151,250), crosses at (850,151.5,190), then joins the EXISTING ridge at (901.34,150.48,153.63). No handling, impulses, or scoring threshold changes. See actual flight.csv for crest clearance and retained hard-landing score rejection.");Save();
            }
        }
    }
}
