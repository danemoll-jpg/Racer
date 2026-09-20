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
        static Transform worldRoot;static RaceRoad street;static RaceDirector owner;static int piece;
        static Vector3 Ground(Vector3 p){p.y=Physics.RaycastAll(new Vector3(p.x,300,p.z),Vector3.down,600).Where(h=>h.collider.name.StartsWith("Ground_")).Select(h=>h.point.y).Append(Phase6Buildings.Ground(p)).Max();return p;}
        static void Beam(Transform root,string name,Vector3 a,Vector3 b,float width,Material mat){var t=Part(root,name,(a+b)*.5f,new(width,width,Vector3.Distance(a,b)),mat);t.localRotation=Quaternion.LookRotation(b-a);}
        static void Fence(Vector3 a,Vector3 b,bool chain)
        {
            a=Ground(a);b=Ground(b);var r=new GameObject(chain?"Grounded roadside chain-link":"Grounded wood crossbuck").transform;r.SetParent(worldRoot);r.position=a;
            var endpointA=new GameObject("Fence endpoint A").transform;endpointA.SetParent(r,false);
            var endpointB=new GameObject("Fence endpoint B").transform;endpointB.SetParent(r,false);endpointB.localPosition=b-a;
            var m=Mat(chain?"Fence galvanized":"Fence white wood",chain?new(.47f,.52f,.51f):new(.87f,.85f,.77f));var q=b-a;
            foreach(var p in new[]{Vector3.zero,q})Part(r,"Terrain seated post",p+Vector3.up*.8f,new(.14f,1.7f,.14f),m);
            foreach(float h in new[]{.25f,1.4f})Beam(r,"Terrain following rail",Vector3.up*h,q+Vector3.up*h,.09f,m);
            if(!chain){Beam(r,"Crossbuck",Vector3.up*.25f,q+Vector3.up*1.4f,.11f,m);Beam(r,"Crossbuck",Vector3.up*1.4f,q+Vector3.up*.25f,.11f,m);}
            else {int n=Mathf.CeilToInt(Vector3.ProjectOnPlane(q,Vector3.up).magnitude/.35f);for(int i=0;i<n;i++){var p=q*(i/(float)n);var end=q*((i+1)/(float)n);Beam(r,"Wire",p+Vector3.up*.25f,end+Vector3.up*1.4f,.018f,m);Beam(r,"Wire",p+Vector3.up*1.4f,end+Vector3.up*.25f,.018f,m);}}
            var filters=r.GetComponentsInChildren<MeshFilter>();var mesh=new Mesh();mesh.CombineMeshes(filters.Select(f=>new CombineInstance{mesh=f.sharedMesh,transform=r.worldToLocalMatrix*f.transform.localToWorldMatrix}).ToArray());mesh=MeshAsset(mesh,owner.gameObject.scene.name+"-fence-"+piece++);r.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;r.gameObject.AddComponent<MeshRenderer>().sharedMaterial=m;foreach(var f in filters)Object.DestroyImmediate(f.gameObject);
            var box=r.gameObject.AddComponent<BoxCollider>();box.center=mesh.bounds.center;box.size=mesh.bounds.size+Vector3.one*.15f;box.isTrigger=true;r.gameObject.AddComponent<BreakableProp>().surface=chain?SmashAudio.Surface.ChainLink:SmashAudio.Surface.Wood;
        }
        static void RunFence(Vector3 a,Vector3 b,bool chain){int n=Mathf.CeilToInt(Vector3.ProjectOnPlane(b-a,Vector3.up).magnitude/3);for(int i=0;i<n;i++)Fence(Vector3.Lerp(a,b,i/(float)n),Vector3.Lerp(a,b,(i+1)/(float)n),chain);}
        static void UnbatchHouse(Transform house)
        {
            var batch=GameObject.Find("Phase 6 - architectural render batches");if(!batch)return;
            foreach(var mf in batch.GetComponentsInChildren<MeshFilter>()){
                var v=mf.sharedMesh.vertices;var t=mf.sharedMesh.triangles;var keep=new List<int>();for(int i=0;i<t.Length;i+=3){var q=house.InverseTransformPoint(mf.transform.TransformPoint((v[t[i]]+v[t[i+1]]+v[t[i+2]])/3));if(Mathf.Abs(q.x)<24&&Mathf.Abs(q.z)<23&&q.y>-30&&q.y<24)continue;keep.AddRange(new[]{t[i],t[i+1],t[i+2]});}
                if(keep.Count==t.Length)continue;var mesh=Object.Instantiate(mf.sharedMesh);mesh.SetTriangles(keep,0);mesh.RecalculateBounds();mf.sharedMesh=MeshAsset(mesh,owner.gameObject.scene.name+"-unbatch-"+house.name+"-"+mf.name);
            }
            if(PrefabUtility.IsPartOfPrefabInstance(house))PrefabUtility.UnpackPrefabInstance(house.gameObject,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
            foreach(var renderer in house.GetComponentsInChildren<MeshRenderer>())renderer.enabled=true;
        }
        static void Properties()
        {
            var home=GameObject.Find("Dan - blue X").transform;var h2=GameObject.Find("Original house 2").transform;var h3=GameObject.Find("Original house 3").transform;var kyle=GameObject.Find("Friend across street - blue circle").transform;
            float sd=street.Project(home.position,out _),s2=street.Project(h2.position,out _),s3=street.Project(h3.position,out _);float direction=Mathf.Sign(Mathf.DeltaAngle(sd/street.Length*360,s2/street.Length*360));
            var old2=h2.position;UnbatchHouse(h2);UnbatchHouse(kyle);
            var oldRoad=street.At(s2,out _);float offset=Vector3.ProjectOnPlane(h2.position-oldRoad,Vector3.up).magnitude;s2+=direction*65;var center=street.At(s2,out var along2);var side=Vector3.Cross(Vector3.up,along2).normalized;if(Vector3.Dot(side,home.position-center)<0)side=-side;
            h2.position=Ground(center+side*offset);h2.rotation=Quaternion.LookRotation(-side);
            var roadHome=street.At(sd,out var alongHome);var face=Vector3.Cross(Vector3.up,alongHome).normalized;if(Vector3.Dot(face,roadHome-home.position)<0)face=-face;home.rotation=Quaternion.LookRotation(face);
            var roadKyle=street.At(street.Project(kyle.position,out _),out _);float roof=kyle.GetComponentsInChildren<MeshRenderer>().Where(r=>r.name.ToLowerInvariant().Contains("roof")).Select(r=>r.bounds.max.y).DefaultIfEmpty(kyle.position.y+8).Max()-kyle.position.y;
            var kp=kyle.position;kp.y=roadKyle.y-roof;kyle.position=kp;
            Vector3 driveEnd=kyle.position+kyle.forward*13;driveEnd.y=kp.y;
            var drive=new[]{roadKyle,Vector3.Lerp(roadKyle,driveEnd,.35f)+Vector3.up*.6f,driveEnd};
            Terrain("properties",p=>{
                if(p.x<350||p.x>555||p.z<-180||p.z>60)return p;
                street.Project(p,out float rd);if(rd<12)return p;
                var q=home.InverseTransformPoint(p);if(Mathf.Abs(q.x)<43&&q.z<14&&q.z>-64){float y=-3.4f*Smooth(3,7,-q.z)-.9f*Smooth(10,16,-q.z)-1.2f*Smooth(23,33,-q.z);float b=(1-Smooth(26,43,Mathf.Abs(q.x)))*(1-Smooth(4,14,q.z))*Smooth(-64,-48,q.z)*Smooth(12,22,rd);p.y=Mathf.Lerp(p.y,home.position.y+y,b);}
                float d=Vector3.ProjectOnPlane(p-h2.position,Vector3.up).magnitude;if(d<33)p.y=Mathf.Lerp(p.y,h2.position.y,1-Smooth(22,33,d));
                d=Vector3.ProjectOnPlane(p-kyle.position,Vector3.up).magnitude;if(d<31)p.y=Mathf.Lerp(p.y,kyle.position.y,1-Smooth(18,31,d));
                d=Near(p,drive,out var at);if(d<11)p.y=Mathf.Lerp(p.y,at.y,1-Smooth(4.5f,11,d));return p;
            });
            foreach(var prop in Object.FindObjectsByType<BreakableProp>().Where(p=>p.name.StartsWith("Property ")||p.name.Contains("fence")||p.name.Contains("crossbuck")).ToArray()){
                var p=prop.transform.position;if(p.x>350&&p.x<475&&p.z>-210&&p.z<65)Object.DestroyImmediate(prop.gameObject);
            }
            float start=sd-direction*35,end=s3+direction*36;float length=Mathf.Abs(end-start);float[] openings={sd-direction*20,sd+direction*20,s2,s3};
            Vector3 Front(float s){var p=street.At(s,out var f);var inward=Vector3.Cross(Vector3.up,f).normalized;if(Vector3.Dot(inward,home.position-p)<0)inward=-inward;return p+inward*11;}
            for(float t=0;t<length;t+=2.5f){float a=start+direction*t,b=start+direction*Mathf.Min(length,t+2.5f);if(openings.Any(s=>Mathf.Abs((a+b)*.5f-s)<6))continue;Fence(Front(a),Front(b),direction*((a+b)*.5f-s2)<-18);}
            // Dan has two separate side drives; rear enclosure is wood around the pool.
            foreach(float x in new[]{-22f,22f}){var a=home.TransformPoint(new Vector3(x,0,8));var b=home.TransformPoint(new Vector3(x,0,-27));RunFence(a,b,false);}
            RunFence(home.TransformPoint(new Vector3(-22,0,-27)),home.TransformPoint(new Vector3(22,0,-27)),false);
            var drives=openings.Take(2).Select((s,i)=>new[]{Front(s),home.TransformPoint(new Vector3(i==0?-22:22,0,9))}).ToArray();
            foreach(var line in drives){line[0]=Ground(line[0]);line[1]=Ground(line[1]);}
            Terrain("dan-drives",p=>{if(p.x<375||p.x>468||p.z<-60||p.z>30)return p;foreach(var line in drives){float d=Near(p,line,out var at);if(d<6)p.y=Mathf.Lerp(p.y,at.y,1-Smooth(3.7f,6,d));}return p;});
            ClearTrees(p=>Vector3.ProjectOnPlane(p-h2.position,Vector3.up).magnitude<27||Near(p,drive,out _)<7||drives.Any(d=>Near(p,d,out _)<6));
            var entry=Front(s3);var toward=h3.position-entry;toward.y=0;var sign=new GameObject("House 3 / Rocky Way Acres entrance").transform;sign.SetParent(worldRoot);sign.SetPositionAndRotation(Ground(entry),Quaternion.LookRotation(toward));
            var wood=Mat("Entrance timber",new(.25f,.16f,.085f));float beamY=6.2f;
            foreach(float x in new[]{-6.2f,6.2f}){var p=Ground(sign.TransformPoint(new Vector3(x,0,0)));float top=sign.position.y+beamY+.6f;var post=Part(sign,"Tall grounded entrance post",sign.InverseTransformPoint(p)+Vector3.up*(top-p.y)*.5f,new(.4f,top-p.y+.1f,.4f),wood);}
            Part(sign,"Rocky Way Acres board",new(0,beamY,0),new(12.8f,1.15f,.35f),wood);var text=new GameObject("Rocky Way Acres lettering").AddComponent<TextMesh>();text.transform.SetParent(sign,false);text.transform.localPosition=new(0,beamY,-.20f);text.text="Rocky Way Acres";text.anchor=TextAnchor.MiddleCenter;text.alignment=TextAlignment.Center;text.fontSize=64;text.characterSize=.33f;text.color=new(.94f,.9f,.72f);sign.gameObject.AddComponent<PhysicalSign>();var sensor=sign.gameObject.AddComponent<BoxCollider>();sensor.center=new(0,beamY,0);sensor.size=new(13,1.2f,.5f);sign.gameObject.AddComponent<BreakableProp>().surface=SmashAudio.Surface.Wood;
            var life=owner.GetComponent<AmbientLife>();if(life){life.football=life.football.Select(Ground).ToArray();life.coffee=life.coffee.Select(Ground).ToArray();life.smoking=life.smoking.Select(Ground).ToArray();}
            File.WriteAllText(Evidence+"/properties-"+owner.gameObject.scene.name+".txt",$"House 2 moved 65m (213.3ft) ALONG street: {old2} -> {h2.position}; Dan front {home.forward}; House2 front {h2.forward}; Kyle base {kyle.position.y}, roof target {roadKyle.y}; gates {string.Join(",",openings)}; frontage sections {piece}; House3 overhead board bottom 5.625m above entry. Each fence endpoint sampled separately.");
        }
        static void Campsite()
        {
            var r=new GameObject("Permanent mountainside camp / two seated guys").transform;r.SetParent(worldRoot);r.position=Ground(new(989,0,75));
            var cloth=Mat("Camp teal",new(.08f,.43f,.45f));var skin=Mat("Camp skin",new(.64f,.42f,.29f));var wood=Mat("Camp log",new(.22f,.11f,.04f));var dark=Mat("Camp dark",new(.06f,.075f,.08f));var fire=Mat("Camp warm fire",new(1,.38f,.04f));fire.EnableKeyword("_EMISSION");fire.SetColor("_EmissionColor",new Color(1,.25f,.015f)*1.5f);
            void OnGround(string name,Vector3 local,Vector3 scale,Material mat,PrimitiveType shape){var p=Ground(r.TransformPoint(local));Part(r,name,r.InverseTransformPoint(p)+Vector3.up*scale.y*.5f,scale,mat,false,shape);}
            OnGround("Small round pop-up tent",new(5,0,1),new(3.7f,1.8f,3.7f),cloth,PrimitiveType.Sphere);OnGround("Tent doorway",new(3.3f,0,1),new(.1f,1.3f,1.4f),dark,PrimitiveType.Sphere);
            for(int i=0;i<8;i++){float a=i*Mathf.PI/4;OnGround("Fire ring stone",new(Mathf.Cos(a)*.8f,0,Mathf.Sin(a)*.8f),new(.4f,.25f,.4f),Mat("Camp stone",new(.35f,.36f,.33f)),PrimitiveType.Sphere);}
            OnGround("Campfire",Vector3.zero,new(.65f,.75f,.65f),fire,PrimitiveType.Sphere);
            foreach(float x in new[]{-2.3f,2.3f}){var person=new GameObject("Seated guy").transform;person.SetParent(r);person.position=Ground(r.TransformPoint(new Vector3(x,0,-1.3f)));person.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(r.position-person.position,Vector3.up));Part(person,"Log seat",new(0,.28f,0),new(1.2f,.55f,.55f),wood);Part(person,"Torso shirt",new(0,.98f,0),new(.58f,.65f,.36f),x<0?cloth:Mat("Camp rust shirt",new(.6f,.23f,.13f)),false,PrimitiveType.Capsule);Part(person,"Head",new(0,1.5f,.02f),new(.36f,.43f,.36f),skin,false,PrimitiveType.Sphere);Part(person,"Hair",new(0,1.66f,0),new(.38f,.18f,.36f),dark,false,PrimitiveType.Sphere);foreach(float side in new[]{-.18f,.18f}){Part(person,"Bent thigh",new(side,.55f,.25f),new(.21f,.22f,.65f),dark);Part(person,"Lower leg",new(side,.27f,.55f),new(.19f,.5f,.2f),dark);Part(person,"Boot",new(side,.08f,.7f),new(.25f,.16f,.4f),wood);Beam(person,"Resting arm",new(side*1.7f,1.14f,0),new(side,.7f,.38f),.15f,skin);}foreach(float xeye in new[]{-.08f,.08f})Part(person,"Eye",new(xeye,1.54f,.18f),new(.045f,.045f,.025f),dark);}
            ClearTrees(p=>Vector3.ProjectOnPlane(p-r.position,Vector3.up).magnitude<8);
        }
        public static void WorldPass()
        {
            foreach(var path in ReverseReviewRelease.Scenes){EditorSceneManager.OpenScene(path);owner=Object.FindAnyObjectByType<RaceDirector>();if(GameObject.Find("CR091-096 exploration world"))continue;worldRoot=new GameObject("CR091-096 exploration world").transform;street=owner.ambientRoad?owner.ambientRoad:owner.road;street.Initialize();piece=0;
                Properties();Campsite();Collectibles();Signs();Map();
                owner.courseId=owner.gameObject.scene.name switch{"StreetLoopGreybox"=>"street-v14-discovery","LakeWoods"=>"lake-v7-discovery","StreetLoopReverse"=>"street-reverse-v5-discovery",_=>"forest-reverse-v5-discovery"};Save();
            }
            CorrectPropertiesAndProps();
            CorrectGuidanceAndMap();
        }
    }
}

