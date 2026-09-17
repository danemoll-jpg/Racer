using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Racer.Editor
{
    public static class Phase4Setup
    {
        public const string RootName = "Phase 4 - Connector Jump";
        public static void Tune(ArcadeVehicle car, bool revised)
        {
            car.topSpeed = revised ? 44 : 38;
            car.acceleration = revised ? 13.5f : 12;
            car.steeringResponse = 8;
            car.slowSteerAngle = 33; car.fastSteerAngle = 10;
            car.yawResponse = revised ? 9 : 8;
            car.lateralGrip = revised ? 8.5f : 7;
            car.maxGripAcceleration = revised ? 25 : 22;
        }

        [MenuItem("Racer/Add Phase 4 Connector Jump")]
        public static void Build()
        {
            var scene = SceneManager.GetActiveScene();
            if (Application.isPlaying || scene.path != StreetLoopBuilder.ScenePath || scene.isDirty)
                throw new InvalidOperationException("Open saved StreetLoopGreybox outside Play mode.");
            if (GameObject.Find(RootName)) throw new InvalidOperationException("Jump already exists.");
            var route = StreetLoopBuilder.Route();
            var root = new GameObject(RootName);
            // Northbound, between CP13 (-159m Z) and CP14 (+71m Z).
            StreetLoopBuilder.Nearest(new Vector3(-627,0,-110),route,out var start);
            root.transform.position = start;
            root.transform.rotation = Quaternion.LookRotation(new Vector3(-.0165f,0,1));
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit")) {color=new Color(.7f,.36f,.08f)};
            AssetDatabase.CreateAsset(mat,"Assets/Materials/Phase4Ramp.mat");
            var vertices = new List<Vector3>(); var tris = new List<int>();
            // Smooth increasing slope: 24m run, 3m rise, 14 degree lip. Left 6m of road.
            for(int i=0;i<=48;i++)
            {
                float z=i*.5f, y=3*(z/24)*(z/24)+.012f;
                foreach(float x in new[]{-4.5f,1.5f})
                {
                    var world=root.transform.TransformPoint(new Vector3(x,0,z));
                    StreetLoopBuilder.Nearest(world,route,out var road);
                    vertices.Add(new Vector3(x,road.y-start.y+y,z));
                }
                if(i<48){int k=i*2;tris.AddRange(new[]{k,k+2,k+1,k+1,k+2,k+3});}
            }
            // Closed sides/back make the takeoff read as a substantial supported ramp.
            int topCount=vertices.Count;
            for(int i=0;i<topCount;i++){var p=vertices[i];p.y=-.3f;vertices.Add(p);}
            for(int i=0;i<48;i++) foreach(int side in new[]{0,1})
            {int a=i*2+side,b=a+2,c=a+topCount,d=b+topCount;tris.AddRange(side==0?new[]{a,c,b,b,c,d}:new[]{a,b,c,b,d,c});}
            tris.AddRange(new[]{96,194,97,97,194,195});
            var mesh=new Mesh{name="Single connector takeoff"};mesh.SetVertices(vertices);mesh.SetTriangles(tris,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh,"Assets/Track/Phase4Takeoff.asset");
            var ramp=new GameObject("Takeoff - 24m x 6m, 3m rise",typeof(MeshFilter),typeof(MeshRenderer),typeof(MeshCollider));
            ramp.transform.SetParent(root.transform,false);ramp.GetComponent<MeshFilter>().sharedMesh=mesh;ramp.GetComponent<MeshCollider>().sharedMesh=mesh;ramp.GetComponent<Renderer>().sharedMaterial=mat;
            // Existing 9m road plus supported shoulders is the landing surface: no gap or terrain edits.
            Mark(root.transform,"Approach",new Vector3(-1.5f,.04f,-35),new Vector3(5,.03f,1),mat);
            for(int z=36;z<=144;z+=12) foreach(float x in new[]{-7.5f,7.5f})
            {
                var world=root.transform.TransformPoint(new Vector3(x,0,z));
                if(Physics.Raycast(world+Vector3.up*20,Vector3.down,out var hit,40,1))
                {var mark=Mark(root.transform,"Landing edge",Vector3.zero,new Vector3(.25f,.04f,3),mat);mark.position=hit.point+Vector3.up*.04f;}
            }
            AddSigns();
            var car=Object.FindAnyObjectByType<ArcadeVehicle>();Tune(car,true);PrefabUtility.RecordPrefabInstancePropertyModifications(car);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        }
        static Transform Mark(Transform parent,string name,Vector3 p,Vector3 scale,Material mat)
        {var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=p;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=mat;Object.DestroyImmediate(g.GetComponent<Collider>());return g.transform;}
        public static void AddSigns()
        {
            var root=GameObject.Find(RootName).transform;
            if(root.Find("Jump approach sign"))return;
            var mat=AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Phase4Ramp.mat");
            var board=Mark(root,"Jump approach sign",new Vector3(-9,3,-36),new Vector3(8,3.5f,.2f),mat);
            var text=new GameObject("Recommended speed").AddComponent<TextMesh>();text.transform.SetParent(root,false);
            text.transform.localPosition=board.localPosition+new Vector3(0,0,-.15f);
            text.text="JUMP\n110 - 120 km/h\nROAD LANE ON RIGHT";text.anchor=TextAnchor.MiddleCenter;text.alignment=TextAlignment.Center;text.fontSize=64;text.characterSize=.09f;text.color=Color.white;
        }
    }
}
