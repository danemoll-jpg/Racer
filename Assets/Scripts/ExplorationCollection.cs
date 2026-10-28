using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
namespace Racer
{
    public sealed class ExplorationCollection:MonoBehaviour
    {
        [Serializable] public sealed class Site {public string id,title,approach;public Vector3 position,access;}
        // 0.88: rewardEarned = all acorns found once (the riding mower unlocked); kept when the hunt is restarted
        [Serializable] public sealed class Save {public int version=1;public List<string> found=new();public bool rewardEarned;}
        public Site[] sites=Array.Empty<Site>();
        public RaceRoad[] routes=Array.Empty<RaceRoad>();
        Save data=new();string path,error;RaceDirector race;Transform[] tokens;Vector3 previous;bool sampled;float feedbackUntil;string feedback;
        Material goldMaterial,capMaterial;
        public int Found=>data.found.Count(id=>sites.Any(s=>s.id==id));
        public bool Discovered(string id)=>data.found.Contains(id);
        public void ResetMovement(){sampled=false;}
        public bool RestartCollection(bool confirmed)
        {
            if(!confirmed||error!=null)return false;
            var fresh=new Save{rewardEarned=data.rewardEarned};
            try{AtomicSave.Write(path,JsonUtility.ToJson(fresh,true));data=fresh;sampled=false;feedback="Acorn hunt restarted / other saves preserved";feedbackUntil=Time.time+5;return true;}
            catch(Exception e){error="Collection could not restart: "+e.Message;return false;}
        }
        // 0.79 Part B: no standing counter; the count shows for a few seconds when an acorn is found (total: pause menu)
        public string Hud=>Time.time<feedbackUntil?feedback:"";
        // 0.93 Part C: the count and any save problem (the areas are listed by the map and the Exploration page)
        public string Header=>$"Woodland acorns: {Found}/{sites.Length} found"+(error!=null?"\n"+error:"");
        public string Summary=>$"Woodland acorns: {Found}/{sites.Length} found\n"+string.Join("\n",sites.GroupBy(s=>s.approach).Select(g=>$"{g.Key}: {g.Count(s=>data.found.Contains(s.id))}/{g.Count()}"))+"\n"+(error??"Discoveries persist across tracks and relaunch.");
        public void Initialize(RaceDirector owner,string root)
        {
            race=owner;path=Path.Combine(root,"woodland-acorns-v1.json");
            // 0.106 Part D: three more acorns on the stunt park's hardest spots (the top deck, the school bus's roof, the top of the loop)
            if(!sites.Any(s=>s.id.StartsWith("stunt-")))sites=sites.Concat(Enumerable.Range(0,StuntPark.Acorns.Length).Select(i=>new Site{id=StuntPark.Acorns[i].id,title=StuntPark.Acorns[i].title,approach=StuntPark.AcornArea,position=StuntPark.AcornAt(i),access=StuntPark.AcornAt(i)})).ToArray();
            try{if(File.Exists(path))data=JsonUtility.FromJson<Save>(File.ReadAllText(path))??new();data.found??=new();}catch(Exception e){error="Collection save unavailable: "+e.Message;}
            // 1.3.0-review5 Part A: an acorn the ground has since covered (woodland-21 4.2 m inside the mountain, woodland-23 1.6 m under the ground
            // and road by the lake: no vehicle could reach them) sits 1 m over that ground; the scenes are unchanged
            foreach(var s in sites){if(s.id.StartsWith("stunt-"))continue;float top=float.NegativeInfinity;foreach(var h in Physics.RaycastAll(s.position+Vector3.up*30,Vector3.down,30,~0,QueryTriggerInteraction.Ignore))if(h.collider.name.StartsWith("Ground_")&&!h.collider.attachedRigidbody&&h.point.y<s.position.y+6)top=Mathf.Max(top,h.point.y);if(top>s.position.y){Debug.Log($"Acorn {s.id} was {top-s.position.y:F1} m under the ground: lifted");s.position.y=top+1;}}

            VehicleUnlocks.Set(Found,data.rewardEarned||(sites.Length>0&&Found>=sites.Length));
            tokens=new Transform[sites.Length];
            var gold=new Material(Shader.Find("Universal Render Pipeline/Lit")){color=new Color(1,.63f,.13f),enableInstancing=true};
            var cap=new Material(gold){color=new Color(.32f,.12f,.025f)};
            goldMaterial=gold;capMaterial=cap;
            for(int i=0;i<sites.Length;i++)
            {
                var rootToken=new GameObject("Acorn / "+sites[i].id+" / "+sites[i].title).transform;rootToken.SetParent(transform);rootToken.position=sites[i].position;tokens[i]=rootToken;
                void Part(PrimitiveType type,Vector3 local,Vector3 scale,Material mat){var g=GameObject.CreatePrimitive(type);g.transform.SetParent(rootToken,false);g.transform.localPosition=local;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=mat;g.GetComponent<Collider>().enabled=false;Destroy(g.GetComponent<Collider>());}
                Part(PrimitiveType.Sphere,Vector3.zero,new(1.1f,1.5f,1.1f),gold);Part(PrimitiveType.Sphere,new(0,.52f,0),new(1.3f,.55f,1.3f),cap);Part(PrimitiveType.Cube,new(0,.85f,0),new(.16f,.4f,.16f),cap);
                rootToken.gameObject.SetActive(false);
            }
        }
        // 1.3.0-review5 Part A: acorns are collected in every Free Roam: one player, split screen (either human player's pickup
        // counts for the one shared hunt, the banner on that player's half) and HIGH MODE, with police and stunt modes played in it
        readonly Vector3[] last=new Vector3[2];readonly bool[] seen=new bool[2];
        ArcadeVehicle Player(int i)=>i==0?race.vehicle:SplitScreen.Active&&!SplitScreen.P2Ai&&SplitScreen.Race?SplitScreen.Race.P2Car:null;
        void FixedUpdate()
        {
            if(DeveloperLocationHud.Inspecting){sampled=false;return;}
            if(!race||!race.Flow)return;
            if(!sampled){seen[0]=seen[1]=false;sampled=true;}
            for(int k=0;k<2;k++)
            {
                var car=Player(k);if(!car||!car.Body){seen[k]=false;continue;}var p=car.Body.position;
                if(!seen[k]){last[k]=p;seen[k]=true;continue;}
                previous=last[k];var delta=p-previous;bool valid=delta.magnitude<=Mathf.Max(3,car.Body.linearVelocity.magnitude*Time.fixedDeltaTime*2+.3f);
                if(race.FreeRoam&&race.Flow.State==RaceFlow.Stage.Racing&&valid&&error==null)Collect(previous,delta,k+1);
                last[k]=p;
            }
        }
        void Collect(Vector3 previous,Vector3 delta,int player)
        {
            for(int i=0;i<sites.Length;i++)
            {
                if(data.found.Contains(sites[i].id))continue;
                float t=delta.sqrMagnitude>.001f?Mathf.Clamp01(Vector3.Dot(sites[i].position-previous,delta)/delta.sqrMagnitude):0;
                if(Vector3.Distance(previous+delta*t,sites[i].position)>2.8f)continue;
                data.found.Add(sites[i].id);bool reward=!data.rewardEarned&&Found>=sites.Length;if(reward)data.rewardEarned=true;
                try{AtomicSave.Write(path,JsonUtility.ToJson(data,true));VehicleUnlocks.Set(Found,data.rewardEarned);
                    // 0.88 Part D: the last acorn unlocks the riding mower. 0.95 Part F: every pickup has the banner (area and its
                    // count); the last one the unlock panel (UnlockNotice, shown by the menus) instead
                    if(reward)UnlockNotice.Raise(race.Flow.Save,UnlockNotice.Acorns(sites.Length));
                    else{var area=AcornAreas.At(this,sites[i].position);AcornBanner.Attach(race.Flow).Show(Found,sites.Length,area?.name,area?.Count(this),player);}}
                catch(Exception e){data.found.Remove(sites[i].id);if(reward)data.rewardEarned=false;error="Collection could not save: "+e.Message;}
            }
        }
        void LateUpdate()
        {
            if(tokens==null||!race)return;
            for(int i=0;i<tokens.Length;i++)
            {bool visible=race.FreeRoam&&!data.found.Contains(sites[i].id)&&SplitScreen.DistanceToPlayers(sites[i].position,race.vehicle.transform.position)<180;tokens[i].gameObject.SetActive(visible);if(visible){tokens[i].position=sites[i].position+Vector3.up*(Mathf.Sin(Time.time*2+i)*.16f);tokens[i].Rotate(0,Time.deltaTime*40,0);}}
        }
        void OnDestroy(){if(goldMaterial)Destroy(goldMaterial);if(capMaterial)Destroy(capMaterial);}
    }
}
