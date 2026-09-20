using System;
using System.Collections.Generic;
using UnityEngine;
namespace Racer
{
    public sealed class AmbientLife : MonoBehaviour
    {
        public Vector3[] football, coffee, smoking, highway, residential;
        public Vector3 batRoost, batEntrance, batForward;
        public int Seed {get;private set;}
        public int DanScene {get;private set;}
        public bool FriendScene {get;private set;}
        public int Population {get;private set;}
        public int Swarms {get;private set;}
        public static int ForcedSeed;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void ConfigureDiagnostics()
        {
            var args=Environment.GetCommandLineArgs();ForcedSeed=0;DriverVariation.Disabled=false;
            if(Array.IndexOf(args,"-racerTestSave")<0)return;
            int at=Array.IndexOf(args,"-lifeSeed");if(at>=0&&at+1<args.Length)int.TryParse(args[at+1],out ForcedSeed);
            DriverVariation.Disabled=Array.IndexOf(args,"-errorsDisabled")>=0;
        }
        sealed class Person {public Transform root, arm, otherArm, leftLeg, rightLeg, prop, smoke; public Vector3 home; public float phase;public int action;public bool selected;}
        readonly List<Person> people=new();
        Transform ball; Transform[] bats, leftWings,rightWings; Vector3[] flightStarts;
        Material skin, hair, trousers, propMat, batMat; Material[] shirts;
        RaceDirector race; AudioSource audioSource; AudioClip chirps;
        float batStart=-100,nextBat,awaySince=-1,nextAnimation; bool armed=true; Vector3 passTarget;
        public static AudioClip Sound(string title,float seconds,int seed,bool bat)
        {
            const int rate=22050;var data=new float[(int)(rate*seconds)];var rng=new System.Random(seed);float phase=0;
            for(int i=0;i<data.Length;i++){float t=i/(float)rate;phase+=2*Mathf.PI*(bat?2200+1600*Mathf.Sin(t*31):200)/rate;float envelope=Mathf.Sin(Mathf.PI*i/data.Length)*(bat?.25f+.75f*Mathf.Pow(Mathf.Sin(t*47),8):1);data[i]=((float)rng.NextDouble()*2-1)*(bat?.16f:.55f)*envelope+(bat?Mathf.Sin(phase)*.2f*envelope:0);}
            var clip=AudioClip.Create(title,data.Length,1,rate,false);clip.SetData(data,0);return clip;
        }
        Material Mat(Color c)=>new(Shader.Find("Universal Render Pipeline/Lit")){color=c,enableInstancing=true};
        Transform Part(Transform parent,string name,PrimitiveType type,Vector3 p,Vector3 scale,Material mat)
        {
            var g=GameObject.CreatePrimitive(type);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=p;g.transform.localScale=scale;g.GetComponent<Collider>().enabled=false;Destroy(g.GetComponent<Collider>());g.GetComponent<Renderer>().sharedMaterial=mat;return g.transform;
        }
        void Awake()
        {
            race=GetComponent<RaceDirector>();skin=Mat(new(.62f,.40f,.26f));hair=Mat(new(.12f,.08f,.05f));trousers=Mat(new(.16f,.20f,.26f));propMat=Mat(new(.83f,.79f,.67f));batMat=Mat(new(.08f,.065f,.075f));
            shirts=new[]{Mat(new(.5f,.18f,.12f)),Mat(new(.17f,.32f,.47f)),Mat(new(.56f,.49f,.19f)),Mat(new(.28f,.39f,.24f))};
            foreach(var p in football)Create(p,0,1);
            foreach(var p in coffee)Create(p,1,1);
            foreach(var p in smoking)Create(p,2,1);
            foreach(var p in highway)Create(p,3,1);
            foreach(var p in residential)Create(p,4,1);
            ball=Part(transform,"Football",PrimitiveType.Sphere,Vector3.zero,new(.36f,.21f,.21f),shirts[0]);ball.gameObject.SetActive(false);
            bats=new Transform[8];leftWings=new Transform[8];rightWings=new Transform[8];flightStarts=new Vector3[8];
            for(int i=0;i<bats.Length;i++)
            {
                var root=new GameObject("Cave bat").transform;root.SetParent(transform);bats[i]=root;
                Part(root,"Body",PrimitiveType.Sphere,Vector3.zero,new(.13f,.18f,.30f),batMat);
                Part(root,"Head",PrimitiveType.Sphere,new(0,.045f,.16f),new(.15f,.13f,.13f),batMat);
                foreach(int side in new[]{-1,1})Part(root,"Ear",PrimitiveType.Cube,new(side*.052f,.13f,.15f),new(.035f,.10f,.035f),batMat).localRotation=Quaternion.Euler(0,0,side*-18);
                leftWings[i]=Wing(root,-1);rightWings[i]=Wing(root,1);root.gameObject.SetActive(false);
            }
            var soundObject=new GameObject("Cave flutter source");soundObject.transform.SetParent(transform,false);audioSource=soundObject.AddComponent<AudioSource>();audioSource.playOnAwake=false;audioSource.spatialBlend=1;audioSource.minDistance=8;audioSource.maxDistance=65;audioSource.dopplerLevel=0;
            audioSource.rolloffMode=AudioRolloffMode.Linear;audioSource.minDistance=18;audioSource.maxDistance=90;audioSource.priority=70;
            audioSource.clip=GetComponent<Wildlife>()?.batFlight;
            // Ready/menu construction must not consume a saved race visit.
            foreach(var p in people)p.root.gameObject.SetActive(false);
        }
        Transform Wing(Transform parent,int side)
        {
            var g=new GameObject("Scalloped wing",typeof(MeshFilter),typeof(MeshRenderer));g.transform.SetParent(parent,false);
            var mesh=new Mesh();mesh.vertices=new[]{Vector3.zero,new Vector3(side*.58f,0,.10f),new Vector3(side*.45f,0,-.09f),new Vector3(side*.29f,0,-.065f),new Vector3(side*.23f,0,-.23f),new Vector3(side*.12f,0,-.13f),new Vector3(side*.055f,0,-.25f)};
            var tris=new List<int>();for(int i=1;i<6;i++){tris.AddRange(new[]{0,i,i+1,0,i+1,i});}mesh.triangles=tris.ToArray();mesh.RecalculateNormals();g.GetComponent<MeshFilter>().sharedMesh=mesh;g.GetComponent<Renderer>().sharedMaterial=batMat;return g.transform;
        }
        void Create(Vector3 at,int action,float scale)
        {
            var p=new Person{root=new GameObject("Ambient resident").transform,home=at,action=action,phase=people.Count*1.713f};p.root.SetParent(transform);p.root.position=at;p.root.localScale=new Vector3(scale*(.92f+people.Count%3*.08f),scale*(.96f+people.Count%3*.04f),scale);
            var shirt=shirts[people.Count%shirts.Length];
            Part(p.root,"Torso",PrimitiveType.Capsule,new(0,1.13f,0),new(.42f,.34f,.28f),shirt);
            Part(p.root,"Head",PrimitiveType.Sphere,new(0,1.63f,0),new(.28f,.32f,.29f),skin);
            Part(p.root,"Hair",PrimitiveType.Sphere,new(0,1.75f,-.015f),new(.29f,.13f,.29f),hair);
            p.leftLeg=Part(p.root,"Left leg",PrimitiveType.Capsule,new(-.12f,.45f,0),new(.16f,.40f,.18f),trousers);
            p.rightLeg=Part(p.root,"Right leg",PrimitiveType.Capsule,new(.12f,.45f,0),new(.16f,.40f,.18f),trousers);
            foreach(int side in new[]{-1,1})Part(p.root,"Shoe",PrimitiveType.Cube,new(side*.12f,.065f,.04f),new(.19f,.13f,.30f),hair);
            p.arm=new GameObject("Gesture shoulder").transform;p.arm.SetParent(p.root,false);p.arm.localPosition=new(.24f,1.36f,0);
            Part(p.arm,"Sleeve",PrimitiveType.Capsule,new(0,-.16f,0),new(.15f,.19f,.15f),shirt);
            Part(p.arm,"Hand",PrimitiveType.Sphere,new(0,-.39f,.01f),new(.13f,.14f,.13f),skin);
            p.otherArm=Part(p.root,"Other arm",PrimitiveType.Capsule,new(-.26f,1.08f,0),new(.15f,.26f,.15f),shirt);
            if(action==1){p.prop=Part(p.arm,"Coffee mug",PrimitiveType.Cylinder,new(0,-.37f,.12f),new(.13f,.095f,.13f),propMat);Part(p.prop,"Handle",PrimitiveType.Cube,new(.65f,0,0),new(.4f,.65f,.3f),propMat);}
            if(action==2){p.prop=Part(p.arm,"Cigarette",PrimitiveType.Cylinder,new(0,-.4f,.10f),new(.022f,.07f,.022f),propMat);p.prop.localRotation=Quaternion.Euler(90,0,0);p.smoke=Part(p.root,"Restrained smoke",PrimitiveType.Sphere,new(.15f,1.8f,.3f),Vector3.one*.04f,propMat);}
            var lod=p.root.gameObject.AddComponent<LODGroup>();lod.SetLODs(new[]{new LOD(.004f,p.root.GetComponentsInChildren<Renderer>())});lod.RecalculateBounds();
            people.Add(p);
        }
        public void SelectScenes()
        {
            Seed=ForcedSeed!=0?ForcedSeed:Guid.NewGuid().GetHashCode();var rng=new System.Random(Seed);
            int visit=rng.Next(4);DanScene=visit<2?visit:2;FriendScene=visit==2;
            if(ForcedSeed==0 && race.Flow?.Save!=null)
            {
                var save=race.Flow.Save;
                var schedule=save.Settings.households??=new();
                schedule.Next(rng,out int scene,out bool smokers);DanScene=scene;FriendScene=smokers;
                save.SaveSettings();
            }
            Population=0;
            // Retire every pooled household member before activating the new visit.
            foreach(var p in people){p.root.gameObject.SetActive(false);p.selected=false;if(p.smoke)p.smoke.gameObject.SetActive(false);}
            foreach(var p in people){p.selected=p.action==0?DanScene==0:p.action==1?DanScene==1:p.action==2?FriendScene:rng.NextDouble()<(p.action==3?.7:.3);p.root.position=p.home;p.root.gameObject.SetActive(p.selected);if(p.selected)Population++;}
            if(ball){ball.position=football[0]+Vector3.up*.95f;ball.gameObject.SetActive(DanScene==0);}batStart=-100;armed=true;nextBat=Time.time+3;awaySince=-1;foreach(var b in bats)b.gameObject.SetActive(false);audioSource.Stop();
        }
        void Update()
        {
            if(!race||!race.vehicle||Time.timeScale==0)return;
            Vector3 player=race.vehicle.transform.position;
            float distance=Vector3.Distance(player,batEntrance);
            if(distance>65){if(awaySince<0)awaySince=Time.time;if(Time.time-awaySince>12&&Time.time>nextBat)armed=true;}else awaySince=-1;
            bool racing=race.Flow && race.Flow.State==RaceFlow.Stage.Racing;
            if(racing&&race.Forest&&armed&&Time.time>nextBat&&distance<30&&distance>7&&Vector3.Dot(race.vehicle.Body.linearVelocity,batForward)>3)
            {
                armed=false;batStart=Time.time;nextBat=Time.time+45;Swarms++;passTarget=player+Vector3.up*3;
                audioSource.transform.position=batRoost;
                GetComponent<Wildlife>()?.ReserveBatSound();audioSource.volume=.85f*(race.Flow.Save?.Settings.ambience??1);audioSource.Play();
                for(int i=0;i<bats.Length;i++){flightStarts[i]=batRoost+new Vector3((i%3-1)*.65f,(i%2)*.4f,i*.17f);bats[i].gameObject.SetActive(true);}
            }
            float age=Time.time-batStart;
            audioSource.volume=.85f*(race.Flow.Save?.Settings.ambience??1);
            if(age>=0&&age<3&&bats.Length>0)audioSource.transform.position=bats[0].position;
            if(!racing&&audioSource.isPlaying)audioSource.Stop();
            for(int i=0;i<bats.Length;i++)
            {
                if(age>3.0f||!racing){bats[i].gameObject.SetActive(false);continue;}
                float t=Mathf.Clamp01((age-i*.025f)/2.7f);var side=Vector3.Cross(Vector3.up,batForward)*(i%2==0?-1:1);
                var bend=passTarget+side*(2.8f+i*.3f)+Vector3.up*(i%3*.45f);var end=passTarget-batForward*22+side*(9+i)+Vector3.up*7;
                bats[i].position=(1-t)*(1-t)*flightStarts[i]+2*(1-t)*t*bend+t*t*end;
                bats[i].rotation=Quaternion.LookRotation((2*(1-t)*(bend-flightStarts[i])+2*t*(end-bend)).normalized);
                float flap=Mathf.Sin(age*(35+i)+i)*48;leftWings[i].localRotation=Quaternion.Euler(0,0,flap);rightWings[i].localRotation=Quaternion.Euler(0,0,-flap);
            }
            if(Time.time<nextAnimation)return;nextAnimation=Time.time+1/30f;
            foreach(var p in people)
            {
                if(!p.selected||(player-p.home).sqrMagnitude>220*220)continue;
                float t=Time.time+p.phase;float gesture=.5f+.5f*Mathf.Sin(t*.9f);
                int personIndex=people.IndexOf(p);var face=p.action==0?football[(personIndex+1)%3]:p.action==1?coffee[(personIndex-3+1)%2]:p.action==2?smoking[(personIndex-5+1)%2]:p.home+Vector3.forward;
                p.root.rotation=Quaternion.Euler(0,Mathf.Atan2(face.x-p.home.x,face.z-p.home.z)*Mathf.Rad2Deg+Mathf.Sin(t*.6f)*9,0);
                p.arm.localRotation=Quaternion.Euler(p.action==0?-75-25*Mathf.Sin(t*1.6f):p.action==1||p.action==2?-20-115*Mathf.Pow(gesture,5):Mathf.Sin(t*2)*18,0,0);
                if(p.action==0){int thrower=Mathf.FloorToInt(Time.time/2.6f)%3;float moment=Mathf.Repeat(Time.time/2.6f,1);bool catchNext=personIndex==(thrower+1)%3;p.arm.localRotation=Quaternion.Euler(personIndex==thrower?-70-65*Mathf.Sin(Mathf.Clamp01(moment/.35f)*Mathf.PI):catchNext?-70-30*Mathf.Sin(moment*Mathf.PI):-15,0,0);p.otherArm.localRotation=Quaternion.Euler(catchNext?-60:0,0,0);}
                if(p.action>=3 && personIndex%3==0){float walk=Mathf.Sin(t*.28f);var point=p.home+Vector3.forward*walk*.7f;if(Physics.Raycast(point+Vector3.up*2,Vector3.down,out var ground,4,1,QueryTriggerInteraction.Ignore))point.y=ground.point.y+.025f;p.root.position=point;p.root.rotation=Quaternion.Euler(0,Mathf.Cos(t*.28f)>0?0:180,0);p.leftLeg.localRotation=Quaternion.Euler(Mathf.Sin(t*2)*12,0,0);p.rightLeg.localRotation=Quaternion.Euler(-Mathf.Sin(t*2)*12,0,0);}
                if(p.smoke){float puff=Mathf.Repeat(t,6)/6;p.smoke.localPosition=new(.15f,1.65f+puff*.8f,.3f);p.smoke.localScale=Vector3.one*(.06f+.16f*Mathf.Sin(puff*Mathf.PI));p.smoke.gameObject.SetActive(gesture>.65f);}
            }
            if(DanScene==0&&football.Length==3){float cycle=Time.time/2.6f;int i=Mathf.FloorToInt(cycle)%3;float t=Mathf.Clamp01((Mathf.Repeat(cycle,1)-.20f)/.65f);ball.position=Vector3.Lerp(football[i],football[(i+1)%3],t)+Vector3.up*(.95f+Mathf.Sin(t*Mathf.PI)*1.1f);ball.rotation=Quaternion.Euler(Time.time*190,0,45);}
        }
        void OnDestroy(){foreach(var m in shirts??Array.Empty<Material>())Destroy(m);foreach(var m in new[]{skin,hair,trousers,propMat,batMat})if(m)Destroy(m);if(chirps)Destroy(chirps);if(bats!=null)foreach(var b in bats)if(b)foreach(var mesh in b.GetComponentsInChildren<MeshFilter>())if(mesh.name=="Scalloped wing")Destroy(mesh.sharedMesh);}
    }
}
