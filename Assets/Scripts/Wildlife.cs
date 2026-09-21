using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Racer
{
    // A small fixed pool, seeded per race. No physics bodies, agents or road crossings.
    public sealed class Wildlife : MonoBehaviour
    {
        public enum Species { Bird, Squirrel, Frog, Deer, Coyote, Turkey }
        AudioClip turkeyCall;
        [Serializable] public struct Habitat { public Species species; public Vector3 position, escape; }
        public Habitat[] habitats=Array.Empty<Habitat>();
        public AudioClip[] birdCalls, squirrelCalls, frogCalls;
        public AudioClip batFlight;
        public AudioClip[] deerCalls,coyoteCalls;
        public int Seed {get;private set;}
        public int Sightings {get;private set;}
        public int Calls {get;private set;}
        public int SelectedCount {get;private set;}
        public static float QuietUntil;
        sealed class Animal { public Transform root,head,tail,left,right; public Transform[] legs; public Habitat site; public bool selected,retired,seen; public float phase,flee=-100,nextCall; public Vector3 start; }
        readonly List<Animal> animals=new(); readonly List<Material> materials=new();
        RaceDirector race; Camera cameraView; AudioSource voice; System.Random rng; float nextThink,nextVoice;
        public AudioSource Voice=>voice;
        public int ActiveCount { get {int n=0;foreach(var a in animals)if(a.root.gameObject.activeSelf)n++;return n;} }
        float Range(float a,float b)=>Mathf.Lerp(a,b,(float)rng.NextDouble());
        Material Mat(Color c){var m=new Material(Shader.Find("Universal Render Pipeline/Lit")){color=c,enableInstancing=true};materials.Add(m);return m;}
        Transform Part(Transform parent,string name,Vector3 p,Vector3 scale,Material mat,PrimitiveType type=PrimitiveType.Sphere)
        {var g=GameObject.CreatePrimitive(type);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=p;g.transform.localScale=scale;g.GetComponent<Collider>().enabled=false;Destroy(g.GetComponent<Collider>());g.GetComponent<Renderer>().sharedMaterial=mat;return g.transform;}
        void Awake()
        {
            race=GetComponent<RaceDirector>();cameraView=Camera.main;QuietUntil=0;
            var blue=Mat(new(.12f,.30f,.48f));var chest=Mat(new(.75f,.40f,.19f));var brown=Mat(new(.36f,.19f,.09f));var tail=Mat(new(.48f,.27f,.13f));var green=Mat(new(.25f,.42f,.10f));var cream=Mat(new(.77f,.76f,.44f));var black=Mat(new(.015f,.02f,.015f));var beak=Mat(new(.68f,.51f,.15f));var coyoteFur=Mat(new(.43f,.40f,.31f));
            foreach(var site in habitats)
            {
                var a=new Animal{site=site,root=new GameObject("Wildlife "+site.species).transform,phase=animals.Count*2.39996f};a.root.SetParent(transform,false);a.root.position=site.position;
                if(site.species==Species.Bird)
                {
                    Part(a.root,"Blue songbird body",new(0,.32f,0),new(.30f,.34f,.52f),blue);Part(a.root,"Rust breast",new(0,.31f,.16f),new(.25f,.27f,.24f),chest);
                    a.head=Part(a.root,"Bird head",new(0,.55f,.20f),Vector3.one*.25f,blue);Part(a.root,"Pointed beak",new(0,.53f,.37f),new(.08f,.07f,.20f),beak);
                    a.left=Part(a.root,"Left wing",new(-.18f,.34f,-.06f),new(.16f,.10f,.43f),blue);a.right=Part(a.root,"Right wing",new(.18f,.34f,-.06f),new(.16f,.10f,.43f),blue);
                    a.tail=Part(a.root,"Tail feathers",new(0,.29f,-.35f),new(.22f,.055f,.32f),blue);a.tail.localRotation=Quaternion.Euler(-20,0,0);
                    foreach(int side in new[]{-1,1})Part(a.root,"Bird shin",new(side*.085f,.13f,.015f),new(.035f,.20f,.035f),beak);
                    foreach(int side in new[]{-1,1}){Part(a.root,"Eye",new(side*.105f,.59f,.27f),Vector3.one*.045f,black);Part(a.root,"Bird foot",new(side*.085f,.06f,.03f),new(.035f,.12f,.16f),beak);}
                }
                else if(site.species==Species.Turkey)
                {
                    var feather=Mat(new(.17f,.12f,.075f));var bronze=Mat(new(.38f,.25f,.12f));var red=Mat(new(.68f,.08f,.07f));
                    Part(a.root,"Bronze turkey body",new(0,.63f,0),new(.65f,.70f,1.05f),feather);
                    a.head=new GameObject("Pecking turkey neck and head").transform;a.head.SetParent(a.root,false);a.head.localPosition=new(0,.72f,.39f);
                    Part(a.head,"Bare red neck",new(0,.18f,.05f),new(.14f,.45f,.17f),red);Part(a.head,"Blue gray head",new(0,.44f,.09f),new(.20f,.23f,.24f),blue);
                    Part(a.head,"Pale pointed bill",new(0,.41f,.25f),new(.08f,.08f,.19f),beak);Part(a.head,"Red wattle",new(.035f,.30f,.19f),new(.07f,.24f,.07f),red);
                    foreach(int side in new[]{-1,1})Part(a.head,"Turkey eye",new(side*.095f,.47f,.16f),Vector3.one*.035f,black);
                    a.tail=new GameObject("Broad barred turkey tail").transform;a.tail.SetParent(a.root,false);a.tail.localPosition=new(0,.70f,-.46f);
                    for(int i=-4;i<=4;i++){var pivot=new GameObject("Fan feather").transform;pivot.SetParent(a.tail,false);pivot.localRotation=Quaternion.Euler(-20,0,i*14);Part(pivot,"Bronze feather",new(0,.35f,0),new(.14f,.78f,.07f),bronze);Part(pivot,"Dark feather bar",new(0,.59f,-.01f),new(.145f,.085f,.08f),feather);Part(pivot,"Buff feather tip",new(0,.70f,0),new(.14f,.09f,.08f),cream);}
                    Part(a.root,"Folded left wing",new(-.31f,.67f,-.08f),new(.16f,.39f,.76f),bronze);Part(a.root,"Folded right wing",new(.31f,.67f,-.08f),new(.16f,.39f,.76f),bronze);
                    a.legs=new Transform[2];for(int i=0;i<2;i++){var leg=new GameObject("Turkey walking leg").transform;leg.SetParent(a.root,false);leg.localPosition=new(i==0?-.16f:.16f,.40f,.05f);Part(leg,"Scaled leg",new(0,-.18f,0),new(.055f,.39f,.055f),beak);Part(leg,"Three toe foot",new(0,-.36f,.065f),new(.16f,.035f,.23f),beak);a.legs[i]=leg;}a.left=a.legs[0];a.right=a.legs[1];
                }
                else if(site.species==Species.Squirrel)
                {
                    Part(a.root,"Squirrel haunch",new(0,.29f,-.09f),new(.36f,.46f,.47f),brown);Part(a.root,"Cream belly",new(0,.38f,.11f),new(.24f,.31f,.23f),cream);
                    Part(a.root,"Squirrel chest and neck",new(0,.46f,.08f),new(.28f,.37f,.32f),brown);
                    foreach(int side in new[]{-1,1}){Part(a.root,"Squirrel forearm",new(side*.13f,.40f,.17f),new(.13f,.23f,.20f),brown);Part(a.root,"Squirrel ankle",new(side*.16f,.14f,.015f),new(.14f,.23f,.20f),brown);}
                    a.head=Part(a.root,"Squirrel head",new(0,.61f,.14f),new(.27f,.26f,.30f),brown);Part(a.root,"Muzzle",new(0,.57f,.29f),new(.18f,.12f,.17f),tail);Part(a.root,"Nose",new(0,.60f,.36f),Vector3.one*.055f,black);
                    a.tail=new GameObject("Curled bushy tail").transform;a.tail.SetParent(a.root,false);a.tail.localPosition=new(0,.15f,-.23f);
                    Part(a.tail,"Tail base",new(0,.20f,-.17f),new(.22f,.5f,.26f),tail).localRotation=Quaternion.Euler(-24,0,0);Part(a.tail,"Tail plume",new(0,.60f,-.22f),new(.36f,.58f,.34f),tail);Part(a.tail,"Tail curl",new(0,.84f,-.09f),new(.32f,.28f,.29f),tail);
                    foreach(int side in new[]{-1,1}){Part(a.root,"Upright ear",new(side*.095f,.78f,.11f),new(.09f,.17f,.09f),brown);Part(a.root,"Dark eye",new(side*.119f,.65f,.23f),Vector3.one*.055f,black);Part(a.root,"Hind paw",new(side*.17f,.055f,.04f),new(.12f,.10f,.27f),brown);}
                    a.left=Part(a.root,"Foraging paw",new(-.13f,.36f,.24f),new(.085f,.19f,.09f),brown);a.right=Part(a.root,"Other paw",new(.13f,.36f,.24f),new(.085f,.19f,.09f),brown);
                }
                else if(site.species==Species.Deer||site.species==Species.Coyote)
                {
                    bool deer=site.species==Species.Deer;float h=deer?1.02f:.63f;var fur=deer?tail:coyoteFur;
                    Part(a.root,"Lean woodland body",new(0,h,0),new(deer?.46f:.35f,deer?.58f:.39f,deer?1.20f:1.05f),fur);
                    Part(a.root,"Light chest",new(0,h-.12f,.40f),new(.29f,.36f,.19f),cream);
                    a.head=new GameObject(deer?"Deer head":"Coyote head").transform;a.head.SetParent(a.root,false);a.head.localPosition=new(0,h+(deer?.51f:.17f),.58f);
                    Part(a.root,"Sloped neck",new(0,h+.22f,.43f),new(.26f,deer?.72f:.32f,.31f),fur).localRotation=Quaternion.Euler(-24,0,0);
                    Part(a.head,"Long head",Vector3.zero,new(.25f,.30f,.36f),fur);
                    Part(a.head,"Pointed muzzle",new(0,-.065f,.22f),new(.17f,.14f,.30f),fur);
                    Part(a.head,"Dark nose",new(0,-.06f,.37f),new(.095f,.075f,.075f),black);
                    foreach(int side in new[]{-1,1})
                    {
                        Part(a.head,"Upright woodland ear",new(side*.14f,.25f,-.025f),new(.13f,deer?.34f:.25f,.10f),fur).localRotation=Quaternion.Euler(-12,0,-side*22);
                        Part(a.head,"Ear lining",new(side*.15f,.25f,.024f),new(.075f,deer?.23f:.15f,.022f),cream).localRotation=Quaternion.Euler(-12,0,-side*22);
                        Part(a.head,"Alert eye",new(side*.119f,.037f,.115f),Vector3.one*.047f,black);
                    }
                    a.tail=Part(a.root,deer?"Short white flag tail":"Bushy low coyote tail",new(0,h-.04f,-.65f),new(deer?.14f:.19f,deer?.25f:.24f,deer?.20f:.61f),deer?cream:fur);a.tail.localRotation=Quaternion.Euler(deer?-25:-35,0,0);
                    var legs=new List<Transform>();
                    foreach(int side in new[]{-1,1})foreach(float z in new[]{-.40f,.36f})
                    {
                        var leg=new GameObject("Walking leg pivot").transform;leg.SetParent(a.root,false);leg.localPosition=new(side*.16f,h-.13f,z);
                        float length=h-.16f;Part(leg,"Slender leg",new(0,-length*.5f,0),new(deer?.075f:.11f,length,deer?.09f:.13f),fur);
                        Part(leg,deer?"Cloven hoof":"Paw",new(0,-length,.035f),new(.105f,.075f,.15f),deer?black:fur);legs.Add(leg);
                    }
                    a.legs=legs.ToArray();a.left=a.legs[0];a.right=a.legs[1];
                }
                else
                {
                    Part(a.root,"Frog body",new(0,.18f,-.04f),new(.43f,.27f,.46f),green);a.head=Part(a.root,"Broad frog head",new(0,.26f,.17f),new(.43f,.23f,.26f),green);a.tail=Part(a.root,"Calling throat",new(0,.18f,.27f),new(.26f,.16f,.13f),cream);
                    a.left=Part(a.root,"Folded left haunch",new(-.26f,.15f,-.14f),new(.24f,.25f,.35f),green);a.right=Part(a.root,"Folded right haunch",new(.26f,.15f,-.14f),new(.24f,.25f,.35f),green);
                    foreach(int side in new[]{-1,1}){Part(a.root,"Front leg",new(side*.20f,.11f,.23f),new(.095f,.22f,.12f),green);Part(a.root,"Back ankle",new(side*.31f,.08f,-.13f),new(.10f,.17f,.14f),green);}
                    foreach(int side in new[]{-1,1}){Part(a.root,"Raised eye",new(side*.145f,.38f,.18f),Vector3.one*.14f,green);Part(a.root,"Frog pupil",new(side*.145f,.40f,.24f),new(.085f,.06f,.045f),black);Part(a.root,"Webbed front foot",new(side*.22f,.025f,.29f),new(.17f,.045f,.16f),green);Part(a.root,"Long back foot",new(side*.34f,.025f,-.13f),new(.15f,.045f,.26f),green);}
                }
                var facial=new List<Transform>();foreach(Transform child in a.root)if(child!=a.head&&(child.name.Contains("eye")||child.name=="Eye"||child.name.Contains("pupil")||child.name.Contains("ear")||child.name=="Muzzle"||child.name=="Nose"||child.name=="Pointed beak"))facial.Add(child);foreach(var child in facial)child.SetParent(a.head,true);
                a.root.localScale=Vector3.one*(site.species==Species.Frog?1.4f:site.species>=Species.Deer?1:1.25f);animals.Add(a);a.root.gameObject.SetActive(false);
            }
            voice=new GameObject("Wildlife spatial voice").AddComponent<AudioSource>();voice.transform.SetParent(transform,false);voice.playOnAwake=false;voice.spatialBlend=1;voice.rolloffMode=AudioRolloffMode.Linear;voice.minDistance=12;voice.maxDistance=65;voice.dopplerLevel=0;voice.priority=80;
            turkeyCall=TurkeyVoice.Create();
            SelectPopulation();
            // Initial pool is placed before the first world frame. Rendering frustum-culls it.
            foreach(var a in animals)a.root.gameObject.SetActive(a.selected);
        }
        bool InView(Vector3 p)
        {if(!cameraView)return false;var v=cameraView.WorldToViewportPoint(p+Vector3.up*.5f);return v.z>0&&v.x>-.15f&&v.x<1.15f&&v.y>-.2f&&v.y<1.2f;}
        public void SelectPopulation()
        {
            Seed=AmbientLife.ForcedSeed!=0?AmbientLife.ForcedSeed:Environment.TickCount; rng=new System.Random(Seed^62061);SelectedCount=0;nextVoice=Time.time+Range(3,8);
            // Reserve visible retained animals first so later slots cannot push a restart over the cap.
            foreach(var a in animals)if(a.selected&&a.root.gameObject.activeSelf&&InView(a.root.position))SelectedCount++;
            // Preserve a visible individual through restart; make new selections only beyond view.
            // Shuffle the candidate order so appended species are not starved by the cap.
            foreach(var a in animals.Where(a=>a.site.species!=Species.Turkey).OrderBy(_=>rng.Next()).ToArray())
            {
                if(a.root.gameObject.activeSelf&&InView(a.root.position))continue;
                a.selected=SelectedCount<8&&rng.NextDouble()<.40;a.retired=false;a.seen=false;a.flee=-100;a.nextCall=Time.time+Range(1,12);
                a.root.position=a.site.position;a.root.rotation=Quaternion.Euler(0,Range(0,360),0);a.root.gameObject.SetActive(false);if(a.selected)SelectedCount++;
            }
            if(voice)voice.Stop();
            bool turkeys=rng.NextDouble()<.35;foreach(var a in animals.Where(a=>a.site.species==Species.Turkey)){if(a.root.gameObject.activeSelf&&InView(a.root.position))continue;a.selected=turkeys;a.retired=false;a.seen=false;a.flee=-100;a.nextCall=Time.time+Range(3,12);a.root.position=a.site.position;a.root.gameObject.SetActive(false);if(turkeys)SelectedCount++;}
        }
        public void ReserveBatSound(){QuietUntil=Time.time+4;if(voice)voice.Stop();}
        public bool Call(Species species,Vector3 at)
        {
            if(Time.time<QuietUntil||voice.isPlaying||Time.time<nextVoice)return false;
            var clips=species==Species.Turkey?new[]{turkeyCall}:species==Species.Bird?birdCalls:species==Species.Squirrel?squirrelCalls:species==Species.Deer?deerCalls:species==Species.Coyote?coyoteCalls:frogCalls;if(clips==null||clips.Length==0)return false;
            voice.transform.position=at;voice.clip=clips[rng.Next(clips.Length)];voice.pitch=Range(.94f,1.06f);voice.volume=.72f*(race.Flow.Save?.Settings.ambience??1);voice.Play();Calls++;nextVoice=Time.time+Range(7,14);return true;
        }
        void Update()
        {
            if(!race||!race.vehicle||Time.timeScale==0)return;
            voice.volume=.72f*(race.Flow.Save?.Settings.ambience??1);
            if(race.Flow.State!=RaceFlow.Stage.Racing){if(voice.isPlaying)voice.Stop();return;}
            if(Time.time<nextThink)return;nextThink=Time.time+1/30f;
            var player=race.vehicle.transform.position;
            foreach(var a in animals)
            {
                if(!a.selected)continue;float distance=Vector3.Distance(player,a.root.position);bool visible=InView(a.root.position);
                // Prime hidden selected slots at any distance, before a future bend reveals them.
                // Eight low-poly individuals are bounded; let renderer frustum culling handle
                // visibility and distance-cull their animation rather than spawning late.
                if(!a.root.gameObject.activeSelf){if(a.retired||visible)continue;a.root.gameObject.SetActive(true);}
                if(distance>160)continue;
                if(visible&&distance<65&&!a.seen){a.seen=true;Sightings++;}
                float t=Time.time+a.phase;
                if(a.legs!=null)
                {
                    bool walk=a.flee>=0||Mathf.Sin(t*.31f)>.2f;
                    float swing=walk?Mathf.Sin(t*(a.flee>=0?12:3))* (a.flee>=0?30:14):0;
                    for(int leg=0;leg<a.legs.Length;leg++)a.legs[leg].localRotation=Quaternion.Euler(swing*(leg==0||leg==3?1:-1),0,0);
                    a.tail.localRotation=Quaternion.Euler(-30,Mathf.Sin(t)*9,0);
                    if(a.flee<0&&walk){var p=a.site.position+a.site.escape*(1+Mathf.Sin(t*.31f))*1.1f;if(Physics.Raycast(p+Vector3.up*5,Vector3.down,out var hit,12,1,QueryTriggerInteraction.Ignore))p.y=hit.point.y+.03f;a.root.SetPositionAndRotation(p,Quaternion.LookRotation(a.site.escape));}
                }
                a.head.localRotation=Quaternion.Euler(Mathf.Sin(t*1.2f)*9,Mathf.Sin(t*.7f)*22,0);
                if(a.site.species==Species.Turkey&&a.flee<0)a.head.localRotation=Quaternion.Euler(Mathf.Max(0,Mathf.Sin(t*2.8f))*78,Mathf.Sin(t*.7f)*12,0);
                if(a.site.species==Species.Squirrel)a.tail.localRotation=Quaternion.Euler(Mathf.Sin(t*1.7f)*8,Mathf.Sin(t*.8f)*12,0);
                if(a.site.species==Species.Frog)a.tail.localScale=new Vector3(.26f,.16f,.13f)*(1+.12f*Mathf.Sin(t*2.2f));
                if(a.flee<0&&distance<35&&Time.time>a.nextCall){if(Call(a.site.species,a.root.position))a.nextCall=Time.time+Range(20,45);}
                if(a.flee<0&&distance<(a.site.species==Species.Bird?22:15)&&race.vehicle.Body.linearVelocity.magnitude>2){a.flee=Time.time;a.start=a.root.position;}
                if(a.flee>=0)
                {
                    float age=Time.time-a.flee;var direction=a.site.escape.normalized;float length=a.site.species==Species.Bird?Mathf.Min(age*5,55):a.legs!=null?Mathf.Min(age*4.5f,24):Mathf.Min(age*1.5f,5);
                    var p=a.start+direction*length;
                    if(a.site.species==Species.Bird){p.y+=Mathf.Min(age*3,30);float flap=Mathf.Sin(t*31)*65;a.left.localRotation=Quaternion.Euler(0,0,flap);a.right.localRotation=Quaternion.Euler(0,0,-flap);}
                    else{if(Physics.Raycast(p+Vector3.up*8,Vector3.down,out var ground,20,1,QueryTriggerInteraction.Ignore))p.y=ground.point.y+.03f;p.y+=age<3.3f?Mathf.Abs(Mathf.Sin(age*(a.site.species==Species.Frog?5:13)))*(a.site.species==Species.Frog?.45f:.12f):0;a.left.localRotation=Quaternion.Euler(Mathf.Sin(t*13)*22,0,0);a.right.localRotation=Quaternion.Euler(-Mathf.Sin(t*13)*22,0,0);}
                    a.root.SetPositionAndRotation(p,Quaternion.LookRotation(direction));
                    if(age>4&&!visible&&distance>35){a.root.gameObject.SetActive(false);a.retired=true;}
                }
                else if(a.site.species==Species.Bird){a.left.localRotation=Quaternion.Euler(0,0,Mathf.Sin(t)*4);a.right.localRotation=Quaternion.Euler(0,0,-Mathf.Sin(t)*4);}
            }
        }
        void OnDestroy(){foreach(var m in materials)if(m)Destroy(m);if(turkeyCall)Destroy(turkeyCall);}
    }
}
