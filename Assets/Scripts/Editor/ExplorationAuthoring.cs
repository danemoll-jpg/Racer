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
        const string Folder="Assets/Track/Exploration",Evidence="Docs/CR081-090";
        static Transform root;static Material soil,wood,leaf,brick,white,roof,glass,stone;static RaceDirector race;
        static readonly Vector3[] knots={new(690,79,-95),new(747,86,-120),new(820,98,-125),new(880,113,-85),new(1020,139,-60),new(1050,151,30),new(990,164,115),new(920,156,153),new(848,130,135),new(810,116,66),new(745,90,65),new(695,79,45),new(690,79,-95)};
        static Vector3[] trail;
        static Material Mat(string name,Color c){string p=Folder+"/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(p);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit")){color=c,enableInstancing=true};AssetDatabase.CreateAsset(m,p);}return m;}
        static Mesh SaveMesh(Mesh mesh,string name){string p=Folder+"/"+name+".asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(p);if(old){EditorUtility.CopySerialized(mesh,old);Object.DestroyImmediate(mesh);return old;}AssetDatabase.CreateAsset(mesh,p);return mesh;}
        static Transform Part(Transform parent,string name,Vector3 local,Vector3 size,Material material,bool collision=false,PrimitiveType type=PrimitiveType.Cube)
        {var go=GameObject.CreatePrimitive(type);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=local;go.transform.localScale=size;go.GetComponent<Renderer>().sharedMaterial=material;if(!collision)Object.DestroyImmediate(go.GetComponent<Collider>());return go.transform;}
        static float Smooth(float a,float b,float x)=>Mathf.SmoothStep(0,1,Mathf.InverseLerp(a,b,x));
        static Vector3[] Curve(Vector3[] k)
        {
            var points=new List<Vector3>();for(int i=0;i<k.Length-1;i++){var a=k[Mathf.Max(0,i-1)];var b=k[i];var c=k[i+1];var d=k[Mathf.Min(k.Length-1,i+2)];int n=Mathf.CeilToInt(Vector3.Distance(b,c)/2);for(int j=0;j<n;j++){float t=j/(float)n;points.Add(.5f*(2*b+(-a+c)*t+(2*a-5*b+4*c-d)*t*t+(-a+3*b-3*c+d)*t*t*t));}}points.Add(k[^1]);return points.ToArray();
        }
        static float Near(Vector3 p,Vector3[] points,out Vector3 at)
        {float best=float.MaxValue;at=default;for(int i=1;i<points.Length;i++){var a=points[i-1];var v=points[i]-a;v.y=0;var q=p-a;q.y=0;float t=Mathf.Clamp01(Vector3.Dot(q,v)/Mathf.Max(.001f,v.sqrMagnitude));float d=(q-v*t).sqrMagnitude;if(d<best){best=d;at=Vector3.Lerp(a,points[i],t);}}return Mathf.Sqrt(best);}
        static float Mountain(Vector3 p)
        {float baseHeight=78+88*Mathf.Exp(-((p.x-984)*(p.x-984)/22000+(p.z-100)*(p.z-100)/26000));float d=Near(p,trail,out var at);return Mathf.Lerp(baseHeight,at.y,1-Smooth(7,27,d));}
        static void Sign(Vector3 p,Vector3 forward,string text)
        {var t=new GameObject(text).transform;t.SetParent(root);t.SetPositionAndRotation(p,Quaternion.LookRotation(Vector3.ProjectOnPlane(forward,Vector3.up)));Part(t,"Trail sign post",new(0,1.3f,0),new(.16f,2.6f,.16f),wood,true);Part(t,"Trail sign board",new(0,2.35f,0),new(4.5f,1.1f,.16f),wood);var label=new GameObject("Direction lettering").AddComponent<TextMesh>();label.transform.SetParent(t,false);label.transform.localPosition=new(0,2.35f,-.1f);label.text=text;label.fontSize=48;label.characterSize=.11f;label.anchor=TextAnchor.MiddleCenter;label.alignment=TextAlignment.Center;label.color=new(1,.93f,.73f);}
        static void Combine(Transform parent,string name)
        {var filters=parent.GetComponentsInChildren<MeshFilter>();int i=0;foreach(var g in filters.GroupBy(f=>f.GetComponent<Renderer>().sharedMaterial)){var mesh=new Mesh{indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};mesh.CombineMeshes(g.Select(f=>new CombineInstance{mesh=f.sharedMesh,transform=parent.worldToLocalMatrix*f.transform.localToWorldMatrix}).ToArray());var go=new GameObject("Batched "+g.Key.name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(parent,false);go.GetComponent<MeshFilter>().sharedMesh=SaveMesh(mesh,name+"-"+i++);go.GetComponent<Renderer>().sharedMaterial=g.Key;}foreach(var f in filters){Object.DestroyImmediate(f.GetComponent<Renderer>());Object.DestroyImmediate(f);}foreach(var t in parent.GetComponentsInChildren<Transform>().Reverse().ToArray())if(t!=parent&&t.childCount==0&&t.GetComponents<Component>().Length==1)Object.DestroyImmediate(t.gameObject);}
        public static void MountainPass()
        {
            if(Application.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Saved edit mode required");
            Directory.CreateDirectory(Folder);Directory.CreateDirectory(Evidence);AssetDatabase.Refresh();
            soil=Mat("Trail soil",new(.39f,.29f,.15f));wood=Mat("Weathered timber",new(.25f,.15f,.075f));leaf=Mat("Mountain foliage",new(.16f,.28f,.105f));
            trail=Curve(knots);float station=0;
            // Two different authored profiles, integrated into the same terrain surface.
            for(int i=1;i<trail.Length;i++){station+=Vector2.Distance(new(trail[i-1].x,trail[i-1].z),new(trail[i].x,trail[i].z));float s=station-185;if(s>=0&&s<94)trail[i].y+=s<28?3.2f*(s/28)*(s/28):s<40?Mathf.Lerp(3.2f,-3,Smooth(28,40,s)):-3*(1-Smooth(48,94,s));s=station-505;if(s>=0&&s<90)trail[i].y+=s<32?4*(s/32)*(s/32):s<43?Mathf.Lerp(4,-2,Smooth(32,43,s)):-2*(1-Smooth(49,90,s));}
            foreach(var scenePath in ReverseReviewRelease.Scenes)
            {
                var scene=EditorSceneManager.OpenScene(scenePath);race=Object.FindAnyObjectByType<RaceDirector>();
                var old=GameObject.Find("CR081 mountain exploration");if(old)throw new Exception("Mountain already authored; refine existing assets instead of rebuilding");
                root=new GameObject("CR081 mountain exploration").transform;
                var route=new GameObject("Mountain return trails / exploration only").AddComponent<RaceRoad>();route.transform.SetParent(root);route.points=trail;route.forestTrail=true;route.Initialize();
                var collection=race.GetComponent<ExplorationCollection>()??race.gameObject.AddComponent<ExplorationCollection>();collection.routes=new[]{route};
                // Existing world already has a terrain skirt here. Reshape its actual surface,
                // keeping all road/forest course vertices west of the lake unchanged.
                int changed=0;
                foreach(var mf in GameObject.Find("Memory loop - north is +Z").GetComponentsInChildren<MeshFilter>())
                {
                    var original=mf.sharedMesh;var vertices=original.vertices;if(!vertices.Any(p=>p.x>700&&p.x<1200&&p.z>-260&&p.z<310))continue;
                    var mesh=Object.Instantiate(original);var colors=mesh.colors;if(colors.Length!=vertices.Length)colors=Enumerable.Repeat(new Color(.24f,.32f,.12f),vertices.Length).ToArray();
                    for(int i=0;i<vertices.Length;i++){var p=mf.transform.TransformPoint(vertices[i]);float blend=Smooth(700,728,p.x)*(1-Smooth(1110,1200,p.x))*Smooth(-260,-210,p.z)*(1-Smooth(240,310,p.z));if(blend<=0)continue;float d=Near(p,trail,out var at);p.y=Mathf.Lerp(p.y,Mountain(p),blend);vertices[i]=mf.transform.InverseTransformPoint(p);colors[i]=Color.Lerp(new(.22f,.32f,.13f),new(.46f,.33f,.18f),1-Smooth(5,8,d));changed++;}
                    mesh.vertices=vertices;mesh.colors=colors;mesh.RecalculateNormals();mesh.RecalculateBounds();mf.sharedMesh=SaveMesh(mesh,scene.name+"-mountain-"+mf.name);if(mf.TryGetComponent<MeshCollider>(out var mc))mc.sharedMesh=mf.sharedMesh;
                }
                Physics.SyncTransforms();
                // Extend beyond the old x=800 map edge with a matching supported seam.
                var terrainParent=GameObject.Find("Memory loop - north is +Z").transform;
                var terrainMaterial=terrainParent.GetComponentInChildren<MeshRenderer>().sharedMaterial;
                for(int tx=0;tx<4;tx++)for(int tz=0;tz<6;tz++)
                {
                    var v=new Vector3[51*51];var c=new Color[v.Length];var triangles=new List<int>();
                    for(int z=0;z<=50;z++)for(int x=0;x<=50;x++)
                    {var p=new Vector3(800+tx*100+x*2,0,-270+tz*100+z*2);float seam=Phase6Buildings.Ground(new(799.99f,0,p.z));p.y=Mathf.Lerp(seam,Mountain(p),Smooth(800,820,p.x));int n=z*51+x;v[n]=p;Near(p,trail,out _);float d=Near(p,trail,out _);c[n]=Color.Lerp(new(.22f,.32f,.13f),new(.46f,.33f,.18f),1-Smooth(5,8,d));if(x<50&&z<50)triangles.AddRange(new[]{n,n+51,n+1,n+1,n+51,n+52});}
                    var mesh=new Mesh();mesh.vertices=v;mesh.colors=c;mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();var go=new GameObject("Ground_Mountain_"+tx+"_"+tz,typeof(MeshFilter),typeof(MeshRenderer),typeof(MeshCollider));go.transform.SetParent(terrainParent);go.GetComponent<MeshFilter>().sharedMesh=SaveMesh(mesh,scene.name+"-extension-"+tx+"-"+tz);go.GetComponent<MeshCollider>().sharedMesh=go.GetComponent<MeshFilter>().sharedMesh;go.GetComponent<Renderer>().sharedMaterial=terrainMaterial;
                }
                // Full-tree removal, including render batches, is handled below using their
                // spatial bounds; only the new mountain footprint is replaced.
                foreach(var tree in Object.FindObjectsByType<BoxCollider>().Where(c=>c.name.Contains("Tree")||c.name.Contains("Trunk")).ToArray())
                    if(tree.bounds.center.x>700&&tree.bounds.center.x<1200&&tree.bounds.center.z>-260&&tree.bounds.center.z<310)Object.DestroyImmediate(tree.gameObject);
                RebuildMountainTrees(race,new List<string>()); var random=new System.Random(81090);
                for(int cx=0;cx<6;cx++)for(int cz=0;cz<6;cz++)
                {
                    var batch=new GameObject("Mountain woods "+cx+" "+cz).transform;batch.SetParent(root);
                    for(int i=0;i<38;i++){var p=new Vector3(728+cx*64+(float)random.NextDouble()*64,0,-210+cz*72+(float)random.NextDouble()*72);float d=Near(p,trail,out _);if(d<12)continue;p.y=Mountain(p);float h=9+(float)random.NextDouble()*9;Part(batch,"Mountain trunk",p+Vector3.up*h*.36f,new(.55f,h*.72f,.55f),wood,true);Part(batch,"Mountain crown",p+Vector3.up*h*.7f,new(5,h*.8f,5),leaf,false,PrimitiveType.Sphere);}
                    Combine(batch,scene.name+"-woods-"+cx+"-"+cz);
                }
                foreach(var entry in new[]{(213f,"mountain-creek","Fern Creek Leap"),(537f,"mountain-ridge","High Ridge Drop")})
                {var p=route.At(entry.Item1,out var f);var g=new GameObject(entry.Item3).AddComponent<ActivitySite>();g.transform.SetParent(root);g.transform.position=p;g.id=entry.Item2;g.title=entry.Item3;g.kind=ActivitySite.Kind.Jump;g.forward=f;g.radius=20;g.bronze=5;g.silver=14;g.gold=25;Sign(route.At(entry.Item1-50,out f)+Vector3.Cross(Vector3.up,f)*9,f,entry.Item3+"\n35 - 55 mph");}
                Sign(route.At(25,out var first)+Vector3.Cross(Vector3.up,first)*9,first,"MOUNTAIN TRAILS\nCreek ascent / Ridge return");
                Sign(route.At(route.Length-40,out var last)+Vector3.Cross(Vector3.up,last)*9,-last,"RIDGE RETURN\nLake / Neighborhood");
                Sign(route.At(650,out var summit)+Vector3.Cross(Vector3.up,summit)*9,summit,"SUMMIT\nBoth trails return to lake");
                File.WriteAllText(Evidence+"/mountain-"+scene.name+".txt","Changed terrain vertices="+changed+"; trail length="+route.Length+"; geometry profile starts=185/505; lips=213/537; no race gate or traffic assignment changed.");
                EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets();
        }
    }
}

