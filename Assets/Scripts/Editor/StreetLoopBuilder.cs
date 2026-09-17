using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Racer.Editor
{
    // Coordinates traced from the annotated reference at 1311 x 1888 display resolution.
    // X east, Z north. Scale is approximate: 1.1 metres per reference pixel.
    public static class StreetLoopBuilder
    {
        public const string ScenePath = "Assets/Scenes/StreetLoopGreybox.unity";
        const string Folder = "Assets/Track/StreetLoop";
        static Vector3 P(float x, float y, float h) => new((x-650)*1.1f,h,(950-y)*1.1f);
        static readonly Vector3[] Knots = {
            P(940,450,8), P(940,510,13), P(940,610,30), P(945,705,46),
            P(968,766,50), P(1015,792,53), P(1063,855,56), P(1080,920,53),
            P(1082,992,59), P(1118,1070,57), P(1150,1150,42), P(1160,1210,28),
            P(1146,1280,25), P(1140,1360,27), P(1120,1420,24), P(1090,1445,23),
            P(1060,1400,25), P(1020,1342,29), P(987,1309,32), P(943,1308,33),
            P(901,1289,35), P(835,1243,39), P(771,1218,40), P(711,1208,39),
            P(676,1229,36), P(655,1290,31), P(647,1360,26), P(644,1430,21),
            P(618,1470,17), P(548,1478,15), P(445,1478,12), P(324,1461,9),
            P(200,1450,7), P(108,1450,6), P(90,1424,6), P(84,1250,7),
            P(80,1000,8), P(76,760,9), P(76,584,9), P(96,495,8),
            P(113,460,8), P(220,462,8), P(400,468,9), P(570,469,8),
            P(675,468,8), P(722,457,9), P(800,451,9), P(882,440,8), P(925,441,8)
        };
        public static List<Vector3> Route()
        {
            // Interpretive metres, not surveyed: same X/Z spline and landmark order.
            float[] heights = {8,22,52,78,82,76,86,77,88,82,38,27,25,33,24,23,30,38,30,42,35,46,37,44,35,40,31,36,30,30,29,8,7,6,6,7,8,9,9,8,8,8,9,8,8,9,9,8,8};
            var dense = new List<Vector3>();
            for(int i=0;i<Knots.Length;i++) {
                Vector3 a=Knots[(i+Knots.Length-1)%Knots.Length], b=Knots[i], c=Knots[(i+1)%Knots.Length], d=Knots[(i+2)%Knots.Length];
                int steps=Mathf.CeilToInt(Vector3.Distance(b,c)/2);
                a.y=heights[(i+Knots.Length-1)%Knots.Length]; b.y=heights[i]; c.y=heights[(i+1)%Knots.Length]; d.y=heights[(i+2)%Knots.Length];
                for(int j=0;j<steps;j++) { float t=j/(float)steps;
                    dense.Add(0.5f*((2*b)+(-a+c)*t+(2*a-5*b+4*c-d)*t*t+(-a+3*b-3*c+d)*t*t*t));
                }
            }
            return dense;
        }
        public static float Nearest(Vector3 p, List<Vector3> route, out Vector3 road)
        {
            float best=float.MaxValue; road=Vector3.zero;
            for(int i=0;i<route.Count;i++) {
                Vector3 a=route[i], b=route[(i+1)%route.Count], v=b-a; v.y=0;
                Vector3 q=p-a; q.y=0; float t=Mathf.Clamp01(Vector3.Dot(q,v)/v.sqrMagnitude);
                Vector3 r=Vector3.Lerp(a,b,t); float dist=(new Vector2(p.x-r.x,p.z-r.z)).sqrMagnitude;
                if(dist<best) {best=dist; road=r;}
            }
            return Mathf.Sqrt(best);
        }
        static float Ground(Vector3 p,List<Vector3> r) {
            float d=Nearest(p,r,out var road);
            float blend=Mathf.SmoothStep(0,1,Mathf.InverseLerp(16,110,d));
            float sum=0, weight=0; for(int i=0;i<r.Count;i+=12) { var q=p-r[i]; q.y=0; float w=1/Mathf.Pow(q.sqrMagnitude+400,2); sum+=r[i].y*w; weight+=w; }
            float baseHeight=Mathf.Lerp(road.y,sum/weight,Mathf.SmoothStep(0,1,Mathf.InverseLerp(18,75,d)));
            return baseHeight-1.1f + blend*(Mathf.PerlinNoise((p.x+1000)/180,(p.z+1000)/180)*18-9);
        }
        public static void RefreshTerrain() {
            StreetLoopRevision.Rebuild();
        }
        [MenuItem("Racer/Build Phase 2 Street Loop (new scene only)")]
        public static void Build() {
            if(Application.isPlaying || SceneManager.GetActiveScene().isDirty) throw new InvalidOperationException("Save current scene and exit Play mode first.");
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath)) throw new InvalidOperationException("Scene already exists; builder will not overwrite hand edits.");
            if(!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Track","StreetLoop");
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var roadMat=Mat("Road",new Color(.24f,.25f,.26f)); var grass=Mat("Ground",new Color(.35f,.40f,.30f));
            var shoulder=Mat("Shoulder",new Color(.46f,.45f,.39f)); var walls=Mat("House",new Color(.66f,.64f,.57f));
            var landmark=Mat("Landmark",new Color(.49f,.59f,.65f)); var roof=Mat("Roof",new Color(.32f,.33f,.33f));
            var trees=Mat("Woods",new Color(.22f,.30f,.22f)); var trunk=Mat("Trunk",new Color(.34f,.30f,.25f));
            var r=Route(); var root=new GameObject("Memory loop - north is +Z").transform;
            Ribbon("Road - continuous full real loop",r,4.5f,0,roadMat,root);
            Ribbon("Soft shoulders",r,9,-.12f,shoulder,root);
            // Shared-vertex ground grid follows road altitude; shoulders cover the near-road transition.
            int n=181; var v=new Vector3[n*n]; var tri=new List<int>();
            for(int z=0;z<n;z++) for(int x=0;x<n;x++) {var p=new Vector3(-800+x*9,0,-750+z*9); p.y=Ground(p,r);v[z*n+x]=p;
                if(x<n-1&&z<n-1) {int k=z*n+x;tri.AddRange(new[]{k,k+n,k+1,k+1,k+n,k+n+1});}}
            MeshObject("Rolling ground",v,tri.ToArray(),grass,root);
            var buildings=new GameObject("Remembered houses and approximate buildings").transform;
            void House(string name,float x,float y,bool key=false,float w=13,float depth=10) {
                var p=P(x,y,0);p.y=Ground(p,r);var group=new GameObject(name).transform;group.SetParent(buildings);
                Nearest(p,r,out var near); var facing=near-p;facing.y=0;group.position=p;group.rotation=Quaternion.LookRotation(facing);
                Cube("Foundation",group,new(0,.5f,0),new(w+1,1,depth+1),shoulder);
                Cube("House mass",group,new(0,3,0),new(w,5,depth),key?landmark:walls);
                Cube("Roof mass",group,new(0,5.8f,0),new(w+1,.8f,depth+1),roof);
            }
            House("Dan - blue X",970,854,true); House("Original house 1",1028,951,true);
            House("Original house 2",1049,1051,true); House("Original house 3",1080,1141,true);
            House("Friend across street - blue circle",1220,982,true);
            House("Remembered house behind southern hairpin",1090,1492,true);
            foreach(var p in new[]{new Vector2(45,660),new Vector2(113,820),new Vector2(43,1015),new Vector2(118,1130),new Vector2(43,1290),new Vector2(730,1260),new Vector2(690,1360),new Vector2(530,1441),new Vector2(367,1425)}) House("Approximate older residence",p.x,p.y);
            foreach(float x in new[]{180f,330,490,670,805}) House("Approximate main road business",x,404,false,26,19);
            var woods=new GameObject("Woods replacing later subdivisions").transform;var rng=new System.Random(1978);
            for(int i=0;i<1100;i++) {
                var p=new Vector3(Mathf.Lerp(-610,710,(float)rng.NextDouble()),0,Mathf.Lerp(-610,525,(float)rng.NextDouble()));
                float distance=Nearest(p,r,out _); if(distance<22)continue;
                bool clear=false;foreach(Transform b in buildings)if(Vector2.Distance(new(p.x,p.z),new(b.position.x,b.position.z))<27){clear=true;break;}
                if(clear)continue;p.y=Ground(p,r); float h=7+(float)rng.NextDouble()*8;
                var tree=new GameObject("Tree mass").transform;tree.SetParent(woods);tree.position=p;
                Cube("Trunk",tree,new(0,h*.3f,0),new(1.1f,h*.6f,1.1f),trunk);
                var crown=GameObject.CreatePrimitive(PrimitiveType.Sphere);crown.name="Canopy";crown.transform.SetParent(tree,false);crown.transform.localPosition=new(0,h*.75f,0);crown.transform.localScale=new(h*.65f,h*.7f,h*.65f);crown.GetComponent<Renderer>().sharedMaterial=trees;Object.DestroyImmediate(crown.GetComponent<Collider>());
            }
            var spawn=new GameObject("Fixed roadside respawn - north entrance").transform;
            spawn.position=r[10]+Vector3.up*1.1f;spawn.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(r[15]-r[10],Vector3.up));
            var car=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PrototypeCar.prefab"));
            car.transform.SetPositionAndRotation(spawn.position,spawn.rotation);var reset=car.GetComponent<VehicleRespawn>();reset.spawnPoint=spawn;PrefabUtility.RecordPrefabInstancePropertyModifications(reset);
            var cam=new GameObject("Chase Camera");cam.tag="MainCamera";var camera=cam.AddComponent<Camera>();camera.fieldOfView=65;camera.farClipPlane=1800;camera.nearClipPlane=.1f;cam.AddComponent<AudioListener>();var chase=cam.AddComponent<ChaseCamera>();chase.target=car.transform;chase.Snap();
            var sun=new GameObject("Sun").AddComponent<Light>();sun.type=LightType.Directional;sun.intensity=1.7f;sun.transform.rotation=Quaternion.Euler(50,-35,0);sun.shadows=LightShadows.Soft;
            RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;RenderSettings.ambientLight=new Color(.6f,.62f,.65f);
            EditorSceneManager.SaveScene(scene,ScenePath);AssetDatabase.SaveAssets();
            StreetLoopRevision.Rebuild();
        }
        static Material Mat(string name,Color c) {var m=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=name,color=c};m.SetFloat("_Smoothness",.08f);AssetDatabase.CreateAsset(m,Folder+"/"+name+".mat");return m;}
        static void Cube(string name,Transform parent,Vector3 p,Vector3 size,Material m){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=p;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=m;}
        static void Ribbon(string name,List<Vector3> r,float half,float dy,Material mat,Transform parent) {
            var v=new Vector3[(r.Count+1)*2];var t=new List<int>();
            for(int i=0;i<=r.Count;i++){int j=i%r.Count;var f=r[(j+1)%r.Count]-r[(j+r.Count-1)%r.Count];var side=Vector3.Cross(Vector3.up,f).normalized*half;v[i*2]=r[j]-side+Vector3.up*dy;v[i*2+1]=r[j]+side+Vector3.up*dy;if(i<r.Count){int k=i*2;t.AddRange(new[]{k,k+2,k+1,k+1,k+2,k+3});}}
            MeshObject(name,v,t.ToArray(),mat,parent);
        }
        static void MeshObject(string name,Vector3[] vertices,int[] triangles,Material mat,Transform parent){var mesh=new Mesh{name=name,indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};mesh.vertices=vertices;mesh.triangles=triangles;mesh.RecalculateNormals();mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,Folder+"/"+name+".asset");var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer),typeof(MeshCollider));go.transform.SetParent(parent);go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<MeshCollider>().sharedMesh=mesh;go.GetComponent<Renderer>().sharedMaterial=mat;}
    }
}

