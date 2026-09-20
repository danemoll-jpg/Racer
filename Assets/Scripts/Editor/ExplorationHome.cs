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
        static float HomeHeight(Vector3 q)
        {float rear=-3.4f*Smooth(3,7,-q.z)-.9f*Smooth(10,16,-q.z)-1.2f*Smooth(23,33,-q.z);return Mathf.Lerp(rear,-5.5f*Smooth(2,40,-q.z),Smooth(12,18,Mathf.Abs(q.x)));}
        public static void HomePass()
        {
            brick=Mat("Warm red brick",new(.51f,.28f,.20f));white=Mat("White porch trim",new(.89f,.87f,.80f));roof=Mat("Shallow silver roof",new(.49f,.52f,.48f));glass=Mat("Window glass",new(.13f,.25f,.26f));stone=Mat("Pool landscaping stone",new(.46f,.45f,.38f));wood=Mat("Weathered timber",new(.25f,.15f,.075f));leaf=Mat("Mountain foliage",new(.16f,.28f,.105f));
            var collected=new List<ExplorationCollection.Site>();
            foreach(var scenePath in ReverseReviewRelease.Scenes)
            {
                var scene=EditorSceneManager.OpenScene(scenePath);race=Object.FindAnyObjectByType<RaceDirector>();var house=GameObject.Find("Dan - blue X").transform;
                if(house.Find("Reference architecture"))throw new Exception("Home already reconstructed");
                var terrain=GameObject.Find("Memory loop - north is +Z").GetComponentsInChildren<MeshFilter>();
                var street=race.ambientRoad?race.ambientRoad:race.road;street.Initialize();
                foreach(var mf in terrain)
                {
                    var vertices=mf.sharedMesh.vertices;if(!vertices.Any(p=>Vector3.ProjectOnPlane(p-house.position,Vector3.up).sqrMagnitude<90*90))continue;
                    var mesh=Object.Instantiate(mf.sharedMesh);var colors=mesh.colors;bool changed=false;
                    for(int i=0;i<vertices.Length;i++)
                    {var p=mf.transform.TransformPoint(vertices[i]);var q=house.InverseTransformPoint(p);if(Mathf.Abs(q.x)>46||q.z>15||q.z<-66)continue;street.Project(p,out float rd);float blend=(1-Smooth(26,46,Mathf.Abs(q.x)))*(1-Smooth(4,15,q.z))*Smooth(-66,-48,q.z)*Smooth(13,22,rd);if(blend<=0)continue;p.y=Mathf.Lerp(p.y,house.position.y+HomeHeight(q),blend);vertices[i]=mf.transform.InverseTransformPoint(p);changed=true;}
                    if(!changed){Object.DestroyImmediate(mesh);continue;}mesh.vertices=vertices;mesh.RecalculateNormals();mesh.RecalculateBounds();mf.sharedMesh=SaveMesh(mesh,scene.name+"-home-"+mf.name);if(mf.TryGetComponent<MeshCollider>(out var mc))mc.sharedMesh=mf.sharedMesh;
                }
                Physics.SyncTransforms();
                if(PrefabUtility.IsPartOfPrefabInstance(house))PrefabUtility.UnpackPrefabInstance(house.gameObject,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
                foreach(Transform child in house.Cast<Transform>().ToArray())Object.DestroyImmediate(child.gameObject);
                var architecture=new GameObject("Reference architecture").transform;architecture.SetParent(house,false);
                Part(architecture,"Brick lower rear level",new(0,-1.45f,0),new(23,3.5f,10),brick,true);
                Part(architecture,"Long low brick main level",new(0,1.55f,0),new(23,3.1f,10),brick,true);
                var roofMesh=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Buildings/Phase6/Gable roof.asset");
                void Roof(Transform parent,Vector3 p,Vector3 scale){var g=new GameObject("Shallow roof",typeof(MeshFilter),typeof(MeshRenderer));g.transform.SetParent(parent,false);g.transform.localPosition=p;g.transform.localScale=scale;g.GetComponent<MeshFilter>().sharedMesh=roofMesh;g.GetComponent<Renderer>().sharedMaterial=roof;}
                Roof(architecture,new(0,3.1f,0),new(25,1.25f,12));
                Part(architecture,"White fascia",new(0,3.05f,0),new(24.5f,.16f,11.6f),white);
                Part(architecture,"Front porch roof",new(0,2.9f,6.2f),new(21,.18f,2.9f),white);
                Part(architecture,"Front porch floor",new(0,.02f,6),new(21,.18f,2.2f),stone,true);
                for(int i=-2;i<=2;i++)Part(architecture,"White porch post",new(i*4.7f,1.5f,7.2f),new(.19f,3,.19f),white,true);
                for(int i=-2;i<=2;i++)
                {float x=i*4.25f;Part(architecture,"White window surround",new(x,1.75f,5.05f),new(2.15f,1.75f,.12f),white);Part(architecture,"Window glazing",new(x,1.75f,5.13f),new(1.8f,1.45f,.08f),glass);foreach(float dx in new[]{-1.35f,1.35f})Part(architecture,"White shutter",new(x+dx,1.75f,5.13f),new(.42f,1.6f,.12f),white);Part(architecture,"Rear level window",new(x,-1.2f,-5.1f),new(1.8f,1.3f,.1f),glass);}
                Part(architecture,"Entrance door",new(-2.2f,1.15f,5.14f),new(1.1f,2.3f,.1f),white);
                Part(architecture,"Elevated wooden rear deck",new(0,.05f,-7.7f),new(19,.3f,5.4f),wood,true);
                for(int i=-9;i<=9;i+=3){float y=HomeHeight(new(i,0,-10));Part(architecture,"Deck support",new(i,(y+.1f)*.5f,-10),new(.24f,.1f-y,.24f),wood,true);}
                for(int i=-19;i<=19;i++)Part(architecture,"Deck railing baluster",new(i*.48f,.65f,-10.25f),new(.09f,1.1f,.09f),wood);
                Part(architecture,"Rear deck handrail",new(0,1.23f,-10.25f),new(19.5f,.16f,.16f),wood,true);
                foreach(float x in new[]{-9.5f,9.5f}){Part(architecture,"Side handrail",new(x,1.23f,-7.7f),new(.16f,.16f,5.2f),wood,true);for(int z=0;z<11;z++)Part(architecture,"Side railing",new(x,.65f,-5.2f-z*.48f),new(.09f,1.1f,.09f),wood);}
                for(int j=0;j<2;j++)for(int i=-5;i<=5;i++)foreach(float a in new[]{-45f,45f}){var slat=Part(architecture,"White lattice section",new(j==0?-6+i*.3f:6+i*.3f,-1.4f,-10.27f),new(.06f,2.5f,.06f),white);slat.localRotation=Quaternion.Euler(0,0,a);}
                Part(architecture,"Brick chimney",new(8.2f,.65f,-5.6f),new(1.5f,8.6f,1.5f),brick,true);Part(architecture,"Chimney cap",new(8.2f,5,-5.6f),new(1.8f,.2f,1.8f),stone);
                // Pool sits in the supported lower yard, with low rounded stone edges.
                Part(architecture,"Pool coping",new(0,-4.1f,-18.5f),new(13,.24f,8),stone,true);
                var water=Part(architecture,"Rear swimming pool",new(0,-3.94f,-18.5f),new(11.8f,.04f,6.8f),Mat("Pool water",new(.08f,.57f,.62f)));water.gameObject.AddComponent<ShallowWater>();
                var garage=new GameObject("Separate downhill garage kennel").transform;garage.SetParent(architecture,false);garage.localPosition=new(3,-5.5f,-37);
                Part(garage,"Garage walls",new(0,1.8f,0),new(17,3.6f,10),white,true);Roof(garage,new(0,3.6f,0),new(18.5f,1.35f,11.5f));
                foreach(float x in new[]{-5f,0f,5f}){Part(garage,"Garage door",new(x,1.5f,5.06f),new(4.2f,3,.1f),roof);for(int j=1;j<6;j++)Part(garage,"Door panel line",new(x,j*.5f,5.13f),new(4.2f,.04f,.03f),white);}
                var poolhouse=new GameObject("Smaller pool house left of garage").transform;poolhouse.SetParent(architecture,false);poolhouse.localPosition=new(-14,-5.5f,-34);
                Part(poolhouse,"Pool house walls",new(0,1.35f,0),new(5,2.7f,5),white,true);Roof(poolhouse,new(0,2.7f,0),new(6,.9f,6));Part(poolhouse,"Pool house door",new(0,1,2.55f),new(1,2,.1f),wood);
                for(int i=0;i<14;i++){float x=-10+i*1.5f;var p=house.TransformPoint(new(x,0,8.3f));p.y=Phase6Buildings.Ground(p);Part(architecture,"Front shrub",house.InverseTransformPoint(p)+Vector3.up*.55f,new(1.2f,1.1f,1.1f),leaf,false,PrimitiveType.Sphere);}
                for(int i=0;i<24;i++){float angle=i*Mathf.PI*2/24;var p=new Vector3(Mathf.Cos(angle)*8.5f,0,-18.5f+Mathf.Sin(angle)*5.4f);var world=house.TransformPoint(p);world.y=Phase6Buildings.Ground(world);Part(architecture,"Lower yard stone landscaping",house.InverseTransformPoint(world)+Vector3.up*.25f,new(1.4f,.5f,.9f),stone,false,PrimitiveType.Sphere);}
                Combine(architecture,scene.name+"-home-art");
                // Saved location samples are identical across all course variants.
                var collection=race.GetComponent<ExplorationCollection>();
                if(collected.Count==0)
                {
                    var locations=new[]{new Vector3(453,0,20),new(464,0,-35),new(459,0,-100),new(432,0,-190),new(500,0,100),new(532,0,215),new(495,0,330),new(330,0,535),new(20,0,535),new(-350,0,530),new(-623,0,150),new(-350,0,-400)};
                    for(int i=0;i<locations.Length;i++){float s=street.Project(locations[i],out _);var p=street.At(s,out var f)+Vector3.Cross(Vector3.up,f).normalized*2; p.y=Phase6Buildings.Ground(p)+1;collected.Add(new(){id="woodland-"+(i+1).ToString("00"),title=new[]{"Dan's fence","Kyle's turning","House 2 approach","Wooded descent","North bend","Pine shoulder","Hilltop","Market verge","Old highway","Western woods","Trickum approach","Southern return"}[i],position=p,approach="Neighborhood roads"});}
                    var route=collection.routes[0];for(int i=0;i<12;i++){float s=40+i*(route.Length-80)/12;var p=route.At(s,out var f);p.y=Phase6Buildings.Ground(p)+1;collected.Add(new(){id="woodland-"+(13+i).ToString("00"),title=new[]{"Lake gateway","Creek approach","Fern landing","Pine climb","High clearing","Ridge lip","Summit overlook","North saddle","Switchback","Laurel descent","Lake view","Homeward woods"}[i],position=p,approach=i<6?"Creek ascent":"Ridge return"});}
                }
                collection.sites=collected.ToArray();
                // Names are corrected by physical location, not a global text replacement.
                foreach(var label in Object.FindObjectsByType<TextMesh>())
                {var p=label.transform.position;if(label.text.IndexOf("Jamerson",StringComparison.OrdinalIgnoreCase)>=0&&p.x<-450)label.text=label.text.Replace("Jamerson","Trickum");}
                race.courseId=scene.name switch{"StreetLoopGreybox"=>"street-v13-exploration","LakeWoods"=>"lake-v6-exploration","StreetLoopReverse"=>"street-reverse-v4-exploration",_=>"forest-reverse-v4-exploration"};
                File.WriteAllText(Evidence+"/home-"+scene.name+".txt","Reference-informed approximate geometry, no image textures or likenesses. Home front "+house.position+" rotation="+house.eulerAngles+"; rear deck; pool; downhill garage and left pool house. 24 stable collectible locations. Rules="+race.courseId);
                EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets();
        }
    }
}
