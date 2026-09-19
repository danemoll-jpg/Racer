using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;
namespace Racer.Editor
{
    public static class CombinedWorldUpdate
    {
        const string Folder="Assets/Track/CombinedReview";
        public static void Apply()
        {
            if(Application.isPlaying)throw new InvalidOperationException("Edit mode required");
            var race=Object.FindAnyObjectByType<RaceDirector>();var road=race.road;road.Initialize();
            // Shared rejoin/entrance gaps are only 70–80m: split the gap, rather than putting
            // a gate inside either shortcut. Other post-flight gates have >=75m clearance.
            foreach(var item in new[]{(7,1540f),(10,2185f),(11,2545f),(12,2830f),(13,3080f)})
            {var gate=race.gates[item.Item1];var p=road.At(item.Item2,out var f);gate.transform.SetPositionAndRotation(p+Vector3.up*1.6f,Quaternion.LookRotation(Vector3.ProjectOnPlane(f,Vector3.up)));gate.upperHeight=Mathf.Max(gate.upperHeight,24);}
            foreach(var branch in Object.FindObjectsByType<WoodlandRoute>())
            {branch.bypassedGates=Enumerable.Range(1,race.gates.Length-1).Where(i=>{float s=road.Project(race.gates[i].transform.position,out _);return s>branch.entryRoad&&s<branch.exitRoad;}).ToArray();EditorUtility.SetDirty(branch);}
            // Keep a solid lower facade, but inset it behind the launch lip. The old vertical
            // box extended beyond the mesh and snagged the motorcycle's nose in recorded runs.
            var house=GameObject.Find("Fox Gully drive-through house (former 44 -451)").transform;
            var facade=house.Find("Exit lower facade");facade.localPosition=new(0,2.35f,21.55f);facade.localScale=new(20,4.7f,.4f);
            if(!AssetDatabase.IsValidFolder(Folder))AssetDatabase.CreateFolder("Assets/Track","CombinedReview");
            if(!GameObject.Find("Decorative road continuations"))
            {
                var root=new GameObject("Decorative road continuations").transform;
                // Authored named junctions: Hwy at each end, Cherokee at its southern
                // junction, Jamerson westward. No arbitrary bend extensions.
                Extension(root,"Hwy 92 east",road.At(4640,out _),Vector3.right,16.4f);
                Extension(root,"Hwy 92 west",road.At(3755,out _),Vector3.left,16.4f);
                Extension(root,"South Cherokee Lane south",road.At(1195,out _),Vector3.back,9);
                Extension(root,"Jamerson Rd west",road.At(2640,out _),Vector3.left,9);
            }
            EditorSceneManager.MarkSceneDirty(race.gameObject.scene);EditorSceneManager.SaveScene(race.gameObject.scene);AssetDatabase.SaveAssets();
            Directory.CreateDirectory("Docs/CR046-049");
            File.WriteAllText("Docs/CR046-049/world.txt",string.Join("\n",race.gates.Select((g,i)=>$"CP{i:00} station={road.Project(g.transform.position,out _):F2}"))+"\n"+string.Join("\n",Object.FindObjectsByType<WoodlandRoute>().Select(b=>$"{b.title}: {b.entryRoad}..{b.exitRoad}; bypass={string.Join(",",b.bypassedGates)}")));
        }
        public static void RefineJunctions()
        {
            if(Application.isPlaying)throw new InvalidOperationException("Edit mode required");
            foreach(var item in new[]{("Hwy 92 east",70f),("Hwy 92 west",60f)})
            {
                var root=GameObject.Find(item.Item1).transform;
                if(root.Find("Connected highway apron"))continue;
                root.position+=root.forward*item.Item2;
                foreach(string name in new[]{"Supported continuation embankment","Decorative Road pavement"})
                {var t=root.Find(name);var p=t.localPosition;p.z-=item.Item2*.5f;t.localPosition=p;var s=t.localScale;s.z+=item.Item2;t.localScale=s;}
                new GameObject("Connected highway apron").transform.SetParent(root,false);
            }
            foreach(var boundary in Object.FindObjectsByType<CircuitBoundary>())
            {
                var root=boundary.transform;
                if(root.name.StartsWith("Hwy")&&!root.Find("Highway lane divider"))
                    foreach(float side in new[]{-4.1f,4.1f})for(int i=-5;i<42;i++)
                        Cube(root,"Highway lane divider",new(side,.04f,i*10),new(.15f,.02f,5),Mat("Lane white",new(.88f,.88f,.82f)),false);
                foreach(var car in root.GetComponent<ContinuationTraffic>().cars)
                    if(!car.Find("Wheel"))foreach(float x in new[]{-.85f,.85f})foreach(float z in new[]{-1.15f,1.15f})
                    {
                        var wheel=GameObject.CreatePrimitive(PrimitiveType.Cylinder);wheel.name="Wheel";wheel.transform.SetParent(car,false);wheel.transform.localPosition=new(x,-.65f,z);wheel.transform.localScale=new(.65f,.15f,.65f);wheel.transform.localRotation=Quaternion.Euler(0,0,90);wheel.GetComponent<Renderer>().sharedMaterial=Mat("Tire black",new(.04f,.04f,.045f));Object.DestroyImmediate(wheel.GetComponent<Collider>());
                    }
                foreach(var t in boundary.GetComponentsInChildren<Transform>())
                    if(!t.name.Contains("traffic") && !t.GetComponentsInParent<ContinuationTraffic>().Any(c=>c.cars.Contains(t.parent)))
                        GameObjectUtility.SetStaticEditorFlags(t.gameObject,StaticEditorFlags.BatchingStatic);
            }
            Apply();
        }
        static Material Mat(string name,Color color)
        {string path=Folder+"/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit")){color=color};AssetDatabase.CreateAsset(m,path);}return m;}
        static Transform Cube(Transform parent,string name,Vector3 p,Vector3 scale,Material mat,bool solid=true)
        {var o=GameObject.CreatePrimitive(PrimitiveType.Cube);o.name=name;o.transform.SetParent(parent,false);o.transform.localPosition=p;o.transform.localScale=scale;o.GetComponent<Renderer>().sharedMaterial=mat;if(!solid)Object.DestroyImmediate(o.GetComponent<Collider>());return o.transform;}
        static void Extension(Transform parent,string name,Vector3 start,Vector3 direction,float width)
        {
            var root=new GameObject(name).transform;root.SetParent(parent);root.SetPositionAndRotation(start,Quaternion.LookRotation(direction));root.gameObject.AddComponent<CircuitBoundary>();
            var asphalt=Mat("Asphalt",new(.22f,.23f,.24f));var earth=Mat("Supported earth",new(.32f,.35f,.24f));var concrete=Mat("Concrete",new(.55f,.55f,.48f));var yellow=Mat("Closure yellow",new(.94f,.65f,.08f));var black=Mat("Tunnel darkness",new(.012f,.015f,.02f));
            Cube(root,"Supported continuation embankment",new(0,-6,210),new(width+12,12,440),earth);
            Cube(root,"Decorative Road pavement",new(0,-.12f,210),new(width,.25f,440),asphalt);
            for(int i=0;i<42;i++)Cube(root,"Lane dash",new(0,.03f,i*10),new(.15f,.02f,5),yellow,false);
            // Closed event access, separate from the active loop; no traffic is sent through it.
            Cube(root,"Solid race closure",new(0,1.15f,25),new(width+8,2.3f,1.2f),concrete);
            for(float x=-width*.5f-4;x<=width*.5f+4;x+=1.6f)
            {Cube(root,"Closure reflective stripe",new(x,1.25f,24.36f),new(.7f,1.4f,.08f),yellow,false);Cube(root,"Closure fence upright",new(x,3.7f,25),new(.12f,3.6f,.12f),concrete);}
            for(int y=3;y<=5;y++)Cube(root,"Closure fence rail",new(0,y,25),new(width+8,.1f,.1f),concrete);
            // Visible high gantry reinforces the no-exit zone; swept runtime enforcement
            // catches flight above it and prevents reset candidates behind the closure.
            Cube(root,"Event gantry",new(0,7,25),new(width+8,.5f,.5f),concrete);
            var label=new GameObject("Race boundary sign",typeof(TextMesh));label.transform.SetParent(root,false);label.transform.localPosition=new(0,5.7f,24.2f);label.transform.localRotation=Quaternion.Euler(0,180,0);
            var text=label.GetComponent<TextMesh>();text.text="RACE ROUTE CLOSED\nFOLLOW CIRCUIT ARROWS";text.anchor=TextAnchor.MiddleCenter;text.alignment=TextAlignment.Center;text.characterSize=.24f;text.fontSize=48;text.color=Color.white;
            // Traffic's visible segment is beyond a curved/tunnel access separation. Covered
            // endpoints conceal recycling; there is never a solid barrier in its travel lane.
            foreach(float z in new[]{62f,402f})
            {Cube(root,"Traffic tunnel canopy",new(0,5,z),new(width+8,1,22),earth);Cube(root,"Traffic tunnel left",new(-width*.5f-1,2.5f,z),new(2,5,22),earth);Cube(root,"Traffic tunnel right",new(width*.5f+1,2.5f,z),new(2,5,22),earth);}
            Cube(root,"Distant tunnel backdrop",new(0,2.5f,429),new(width,5,1),black,false);
            var traffic=root.gameObject.AddComponent<ContinuationTraffic>();traffic.cars=new Transform[4];
            for(int i=0;i<4;i++)
            {var c=new GameObject("Distant local traffic").transform;c.SetParent(root,false);traffic.cars[i]=c;Cube(c,"Body",Vector3.zero,new(1.7f,.65f,3.7f),Mat("Traffic "+i,new Color(.2f+i*.13f,.3f,.35f)),false);Cube(c,"Cab",new(0,.55f,0),new(1.5f,.55f,1.8f),black,false);}
        }
    }
}
