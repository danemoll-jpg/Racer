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
    public static partial class DiscoveryAuthoring
    {
        const string Folder="Assets/Track/Discovery", Evidence=DiscoveryRelease.Evidence;
        static Material Mat(string name,Color color){string path=Folder+"/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit")){color=color,enableInstancing=true};AssetDatabase.CreateAsset(m,path);}return m;}
        static Mesh MeshAsset(Mesh mesh,string name){string path=Folder+"/"+name+".asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(old){EditorUtility.CopySerialized(mesh,old);Object.DestroyImmediate(mesh);return old;}AssetDatabase.CreateAsset(mesh,path);return mesh;}
        static float Smooth(float a,float b,float x)=>Mathf.SmoothStep(0,1,Mathf.InverseLerp(a,b,x));
        static float Near(Vector3 p,Vector3[] points,out Vector3 at){float best=float.MaxValue;at=default;for(int i=1;i<points.Length;i++){var a=points[i-1];var v=points[i]-a;v.y=0;var q=p-a;q.y=0;float t=Mathf.Clamp01(Vector3.Dot(q,v)/Mathf.Max(.001f,v.sqrMagnitude));float d=(q-v*t).sqrMagnitude;if(d<best){best=d;at=Vector3.Lerp(a,points[i],t);}}return Mathf.Sqrt(best);}
        static Transform Part(Transform parent,string name,Vector3 p,Vector3 size,Material material,bool solid=false,PrimitiveType shape=PrimitiveType.Cube){var o=GameObject.CreatePrimitive(shape);o.name=name;o.transform.SetParent(parent,false);o.transform.localPosition=p;o.transform.localScale=size;o.GetComponent<Renderer>().sharedMaterial=material;if(!solid)Object.DestroyImmediate(o.GetComponent<Collider>());return o.transform;}
        static void Terrain(string suffix,Func<Vector3,Vector3> adjust)
        {
            foreach(var mf in GameObject.Find("Memory loop - north is +Z").GetComponentsInChildren<MeshFilter>()){
                var v=mf.sharedMesh.vertices;bool changed=false;
                for(int i=0;i<v.Length;i++){var p=mf.transform.TransformPoint(v[i]);var q=adjust(p);if((p-q).sqrMagnitude>.000001f){v[i]=mf.transform.InverseTransformPoint(q);changed=true;}}
                if(!changed)continue;var m=Object.Instantiate(mf.sharedMesh);m.vertices=v;m.RecalculateNormals();m.RecalculateBounds();mf.sharedMesh=MeshAsset(m,UnityEngine.SceneManagement.SceneManager.GetActiveScene().name+"-"+suffix+"-"+mf.name);var collider=mf.GetComponent<MeshCollider>();collider.sharedMesh=null;collider.sharedMesh=mf.sharedMesh;
            }Physics.SyncTransforms();
        }
        static void Save(){var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();}
        public static float SummitHeight(float s){if(s<70)return 145+s*.08f;if(s<=100){float t=(s-70)/30;return 150.6f+18*t*t;}if(s<122)return Mathf.Lerp(168.6f,143,Smooth(100,122,s));return 143-(s-122)*.13f;}
        public static void Summit()
        {
            Directory.CreateDirectory(Folder);Directory.CreateDirectory(Evidence);AssetDatabase.Refresh();
            foreach(var path in ReverseReviewRelease.Scenes){EditorSceneManager.OpenScene(path);if(GameObject.Find("CR094 summit launch"))throw new Exception("First geometry already exists");
                var race=Object.FindAnyObjectByType<RaceDirector>();var collection=race.GetComponent<ExplorationCollection>();var root=new GameObject("CR094 summit launch").transform;root.position=new(1070,145,190);root.rotation=Quaternion.LookRotation(Vector3.left);
                var entry=new[]{new Vector3(990,164,115),new(1035,153,130),new(1083,145,153),new(1090,145,181),new(1070,145,190)};
                var exit=new[]{new Vector3(810,SummitHeight(260),190),new(790,118,173),new(794,115,129),new(810,116,66)};
                Terrain("summit-first",p=>{
                    if(p.x<765||p.x>1110||p.z<50||p.z>225)return p;
                    float s=1070-p.x,side=Mathf.Abs(p.z-190);float blend=Smooth(-25,0,s)*(1-Smooth(260,300,s))*(1-Smooth(12,30,side));
                    if(blend>0)foreach(var old in collection.routes)if(Near(p,old.points,out _)<13)blend=0;
                    if(blend>0)p.y=Mathf.Lerp(p.y,SummitHeight(s),blend);
                    foreach(var route in new[]{entry,exit}){float d=Near(p,route,out var at);if(d<14){float b=1-Smooth(6,14,d);p.y=Mathf.Lerp(p.y,at.y,b);}}
                    return p;});
                // Explicit raised takeoff terminates at the lip. The terrain below descends;
                // flight and landing use gravity and existing vehicle physics.
                var verts=new List<Vector3>();var tris=new List<int>();for(int i=0;i<=100;i++){float s=i;verts.Add(new(-12,SummitHeight(s)-145+.06f,s));verts.Add(new(12,SummitHeight(s)-145+.06f,s));if(i<100){int n=i*2;tris.AddRange(new[]{n,n+2,n+1,n+1,n+2,n+3});}}
                var mesh=new Mesh();mesh.SetVertices(verts);mesh.SetTriangles(tris,0);mesh.RecalculateNormals();var surface=new GameObject("Ground_Summit authored launch",typeof(MeshFilter),typeof(MeshRenderer),typeof(MeshCollider));surface.transform.SetParent(root,false);surface.GetComponent<MeshFilter>().sharedMesh=MeshAsset(mesh,"summit-launch");surface.GetComponent<MeshCollider>().sharedMesh=surface.GetComponent<MeshFilter>().sharedMesh;surface.GetComponent<Renderer>().sharedMaterial=Mat("Summit packed earth",new(.43f,.31f,.17f));
                var site=new GameObject("Summit Homeward Flight").AddComponent<ActivitySite>();site.transform.SetParent(root,false);site.transform.localPosition=new(0,SummitHeight(100)-145,100);site.id="summit-homeward";site.title="Summit Homeward Flight";site.kind=ActivitySite.Kind.Jump;site.forward=Vector3.left;site.radius=28;site.bronze=30;site.silver=65;site.gold=100;
                var route=new GameObject("Summit approach and safe return").AddComponent<RaceRoad>();route.transform.SetParent(root);route.points=entry.Concat(Enumerable.Range(1,26).Select(i=>new Vector3(1070-i*10,SummitHeight(i*10),190))).Concat(exit.Skip(1)).ToArray();route.forestTrail=true;route.Initialize();collection.routes=collection.routes.Concat(new[]{route}).ToArray();
                ClearTrees(p=>p.x>780&&p.x<1110&&p.z>148&&p.z<222);
                File.WriteAllText(Evidence+"/first-geometry-"+race.gameObject.scene.name+".txt","Author origin (1070,145,190), west-facing lip (970,168.6,190), 24m width, supported landing x=810..948 z=178..202; approach from summit east; return via (794,115,129). Unpolished before ordinary-frame testing.");Save();
            }
            // Fresh generation finishes with the corrected, tested geometry.
            RefineSummit();
        }
        static void ClearTrees(Func<Vector3,bool> remove)
        {
            // Remove complete collider/visual objects where available. Batched visuals are
            // trimmed by triangle position as well, so an invisible trunk is never left behind.
            foreach(var c in Object.FindObjectsByType<Collider>().Where(c=>c.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0||c.name.IndexOf("tree",StringComparison.OrdinalIgnoreCase)>=0).ToArray())if(remove(c.bounds.center))Object.DestroyImmediate(c.gameObject);
            foreach(var mf in Object.FindObjectsByType<MeshFilter>().Where(f=>f.transform.GetComponentsInParent<Transform>().Any(t=>t.name.Contains("woods")||t.name.Contains("Woods")||t.name.Contains("Forest tree"))).ToArray()){
                var v=mf.sharedMesh.vertices;var t=mf.sharedMesh.triangles;var keep=new List<int>();for(int i=0;i<t.Length;i+=3){var p=mf.transform.TransformPoint((v[t[i]]+v[t[i+1]]+v[t[i+2]])/3);if(!remove(p))keep.AddRange(new[]{t[i],t[i+1],t[i+2]});}
                if(keep.Count==t.Length)continue;var m=Object.Instantiate(mf.sharedMesh);m.SetTriangles(keep,0);m.RecalculateBounds();mf.sharedMesh=MeshAsset(m,UnityEngine.SceneManagement.SceneManager.GetActiveScene().name+"-cleared-"+mf.GetEntityId().ToString().Replace(':','-'));}
        }
    }
}

