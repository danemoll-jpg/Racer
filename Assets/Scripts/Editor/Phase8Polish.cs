using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

namespace Racer.Editor
{
    // Focused, deterministic visual authoring. Never moves gameplay, collision or environment roots.
    public static class Phase8Polish
    {
        const string Folder="Assets/Environment/Phase8";
        static Material Material(string name,Color color,string shader="Universal Render Pipeline/Lit")
        {
            string path=$"{Folder}/{name}.mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!m){m=new Material(Shader.Find(shader));AssetDatabase.CreateAsset(m,path);}
            m.color=color;if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",.18f);EditorUtility.SetDirty(m);return m;
        }
        static Mesh SaveMesh(string name,List<Vector3> verts,List<int> triangles)
        {
            string path=$"{Folder}/{name}.asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(!mesh){mesh=new Mesh();AssetDatabase.CreateAsset(mesh,path);}mesh.Clear();mesh.SetVertices(verts);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);return mesh;
        }
        static void Quad(List<Vector3> v,List<int> t,Vector3 a,Vector3 b,Vector3 c,Vector3 d)
        {int i=v.Count;v.AddRange(new[]{a,b,c,d});t.AddRange(new[]{i,i+1,i+2,i,i+2,i+3});}
        static void Visual(Transform root,string name,Mesh mesh,Material material)
        {var t=root.Find(name);if(!t){t=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer)).transform;t.SetParent(root,false);}t.gameObject.layer=2;t.GetComponent<MeshFilter>().sharedMesh=mesh;t.GetComponent<MeshRenderer>().sharedMaterial=material;}
        static void Box(List<Vector3> v,List<int> t,Vector3 c,Vector3 size)
        {
            var a=c-size*.5f;var b=c+size*.5f;
            Quad(v,t,new(a.x,a.y,b.z),new(b.x,a.y,b.z),new(b.x,b.y,b.z),new(a.x,b.y,b.z));
            Quad(v,t,new(b.x,a.y,a.z),new(a.x,a.y,a.z),new(a.x,b.y,a.z),new(b.x,b.y,a.z));
            Quad(v,t,new(a.x,a.y,a.z),new(a.x,a.y,b.z),new(a.x,b.y,b.z),new(a.x,b.y,a.z));
            Quad(v,t,new(b.x,a.y,b.z),new(b.x,a.y,a.z),new(b.x,b.y,a.z),new(b.x,b.y,b.z));
            Quad(v,t,new(a.x,b.y,b.z),new(b.x,b.y,b.z),new(b.x,b.y,a.z),new(a.x,b.y,a.z));
            Quad(v,t,new(a.x,a.y,a.z),new(b.x,a.y,a.z),new(b.x,a.y,b.z),new(a.x,a.y,b.z));
        }
        static void Car()
        {
            var car=Object.FindAnyObjectByType<ArcadeVehicle>().transform;
            foreach(string name in new[]{"Body","Cabin","Front direction stripe"}){var r=car.Find(name).GetComponent<Renderer>();r.enabled=false;PrefabUtility.RecordPrefabInstancePropertyModifications(r);}
            var root=car.Find("Phase 8 car visuals");if(!root){root=new GameObject("Phase 8 car visuals").transform;root.SetParent(car,false);}
            var v=new List<Vector3>();var t=new List<int>();
            // Chamfered body remains within the original 1.85 x .65 x 3.7 collision envelope.
            Vector3[] Ring(float y,float halfWidth,float halfLength,float cut)=>new[]{new Vector3(-halfWidth+cut,y,-halfLength),new Vector3(halfWidth-cut,y,-halfLength),new Vector3(halfWidth,y,-halfLength+cut),new Vector3(halfWidth,y,halfLength-cut),new Vector3(halfWidth-cut,y,halfLength),new Vector3(-halfWidth+cut,y,halfLength),new Vector3(-halfWidth,y,halfLength-cut),new Vector3(-halfWidth,y,-halfLength+cut)};
            var lower=Ring(-.275f,.925f,1.85f,.16f);var belt=Ring(.24f,.925f,1.85f,.16f);var upper=Ring(.375f,.82f,1.72f,.14f);
            for(int i=0;i<8;i++){int n=(i+1)%8;Quad(v,t,lower[n],lower[i],belt[i],belt[n]);Quad(v,t,belt[n],belt[i],upper[i],upper[n]);int k=v.Count;v.AddRange(new[]{new Vector3(0,.375f,0),upper[n],upper[i]});t.AddRange(new[]{k,k+1,k+2});}
            // Roof and restrained pillars frame a tapered glass cabin.
            Box(v,t,new(0,.88f,-.3f),new(1.27f,.04f,1.18f));
            foreach(float x in new[]{-.65f,.65f})Box(v,t,new(x,.63f,-.38f),new(.075f,.48f,.10f));
            Visual(root,"Paint",SaveMesh("CarPaint",v,t),Material("Car blue paint",new Color(.15f,.32f,.46f)));
            v.Clear();t.Clear();
            Vector3 bl=new(-.75f,.375f,-1.2f),br=new(.75f,.375f,-1.2f),fl=new(-.75f,.375f,.6f),fr=new(.75f,.375f,.6f);
            Vector3 btl=new(-.62f,.86f,-.88f),btr=new(.62f,.86f,-.88f),ftl=new(-.62f,.86f,.28f),ftr=new(.62f,.86f,.28f);
            Quad(v,t,br,bl,btl,btr);Quad(v,t,fl,fr,ftr,ftl);Quad(v,t,bl,fl,ftl,btl);Quad(v,t,fr,br,btr,ftr);
            Visual(root,"Glass",SaveMesh("CarGlass",v,t),Material("Car smoked blue glass",new Color(.075f,.14f,.18f)));
            v.Clear();t.Clear();Box(v,t,new(0,-.12f,-1.87f),new(1.50f,.12f,.06f));Box(v,t,new(0,-.12f,1.87f),new(1.50f,.12f,.06f));Box(v,t,new(0,.09f,1.861f),new(.64f,.13f,.02f));
            Visual(root,"Bumpers and grille",SaveMesh("CarTrim",v,t),Material("Car charcoal trim",new Color(.09f,.105f,.11f)));
            v.Clear();t.Clear();foreach(float x in new[]{-.60f,.60f})Box(v,t,new(x,.17f,-1.861f),new(.32f,.14f,.045f));
            Visual(root,"Tail lamps",SaveMesh("CarTailLamps",v,t),Material("Car red lamps",new Color(.66f,.09f,.065f)));
            v.Clear();t.Clear();foreach(float x in new[]{-.61f,.61f})Box(v,t,new(x,.18f,1.861f),new(.32f,.15f,.045f));Box(v,t,new(0,.05f,-1.887f),new(.32f,.12f,.01f));
            Visual(root,"Headlamps and plate",SaveMesh("CarLamps",v,t),Material("Car warm lamps",new Color(.84f,.81f,.64f)));
        }
        static void WorldText()
        {
            var texts=Object.FindObjectsByType<TextMesh>();var materials=new Dictionary<Texture,Material>();
            foreach(var text in texts)
            {
                var r=text.GetComponent<Renderer>();var tex=r.sharedMaterial.mainTexture;
                if(!materials.TryGetValue(tex,out var m))
                {
                    m=Material("Depth tested world lettering "+materials.Count,Color.white,"Racer/WorldText");m.mainTexture=tex;EditorUtility.SetDirty(m);materials[tex]=m;
                }
                r.sharedMaterial=m;PrefabUtility.RecordPrefabInstancePropertyModifications(r);
                if(text.name=="Direction arrow")
                {
                    var p=text.transform.position;
                    if(Physics.Raycast(p+Vector3.up*25,Vector3.down,out var hit,60,1,QueryTriggerInteraction.Ignore))
                    {var forward=Vector3.ProjectOnPlane(text.transform.parent.forward,hit.normal);text.transform.SetPositionAndRotation(hit.point+hit.normal*.045f,Quaternion.LookRotation(forward,hit.normal)*Quaternion.Euler(90,0,0));}
                }
            }
        }
        [MenuItem("Racer/Phase 8/Apply focused visual polish")]
        public static void Apply()
        {
            var scene=SceneManager.GetActiveScene();if(Application.isPlaying||scene.isDirty||scene.path!=StreetLoopBuilder.ScenePath)throw new InvalidOperationException("Open saved StreetLoopGreybox in Edit mode.");
            Directory.CreateDirectory(Folder);AssetDatabase.Refresh();Car();WorldText();
            var forest=AssetDatabase.LoadAssetAtPath<Material>("Assets/Vegetation/Phase6/Forest.mat");forest.SetFloat("_Vegetation",1);EditorUtility.SetDirty(forest);
            AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        }
    }
}

