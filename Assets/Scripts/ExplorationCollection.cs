using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
namespace Racer
{
    public sealed class ExplorationCollection:MonoBehaviour
    {
        [Serializable] public sealed class Site {public string id,title,approach;public Vector3 position;}
        [Serializable] public sealed class Save {public int version=1;public List<string> found=new();}
        public Site[] sites=Array.Empty<Site>();
        public RaceRoad[] routes=Array.Empty<RaceRoad>();
        Save data=new();string path,error;RaceDirector race;Transform[] tokens;Vector3 previous;bool sampled;float feedbackUntil;string feedback;
        public int Found=>data.found.Count(id=>sites.Any(s=>s.id==id));
        public string Hud=>Time.time<feedbackUntil?feedback:$"WOODLAND ACORNS {Found}/{sites.Length} / progress in pause menu";
        public string Summary=>$"Woodland acorns: {Found}/{sites.Length} found\n"+string.Join("\n",sites.GroupBy(s=>s.approach).Select(g=>$"{g.Key}: {g.Count(s=>data.found.Contains(s.id))}/{g.Count()}"))+"\n"+(error??"Discoveries persist across tracks and relaunch.");
        public void Initialize(RaceDirector owner,string root)
        {
            race=owner;path=Path.Combine(root,"woodland-acorns-v1.json");
            try{if(File.Exists(path))data=JsonUtility.FromJson<Save>(File.ReadAllText(path))??new();data.found??=new();}catch(Exception e){error="Collection save unavailable: "+e.Message;}
            tokens=new Transform[sites.Length];
            var gold=new Material(Shader.Find("Universal Render Pipeline/Lit")){color=new Color(1,.63f,.13f),enableInstancing=true};
            var cap=new Material(gold){color=new Color(.32f,.12f,.025f)};
            for(int i=0;i<sites.Length;i++)
            {
                var rootToken=new GameObject("Acorn / "+sites[i].id+" / "+sites[i].title).transform;rootToken.SetParent(transform);rootToken.position=sites[i].position;tokens[i]=rootToken;
                void Part(PrimitiveType type,Vector3 local,Vector3 scale,Material mat){var g=GameObject.CreatePrimitive(type);g.transform.SetParent(rootToken,false);g.transform.localPosition=local;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=mat;g.GetComponent<Collider>().enabled=false;Destroy(g.GetComponent<Collider>());}
                Part(PrimitiveType.Sphere,Vector3.zero,new(1.1f,1.5f,1.1f),gold);Part(PrimitiveType.Sphere,new(0,.52f,0),new(1.3f,.55f,1.3f),cap);Part(PrimitiveType.Cube,new(0,.85f,0),new(.16f,.4f,.16f),cap);
                rootToken.gameObject.SetActive(false);
            }
        }
        void FixedUpdate()
        {
            if(!race||!race.Flow)return;var p=race.vehicle.Body.position;
            if(!sampled){previous=p;sampled=true;return;}
            var delta=p-previous;bool valid=delta.magnitude<=Mathf.Max(3,race.vehicle.Body.linearVelocity.magnitude*Time.fixedDeltaTime*2+.3f);
            if(race.FreeRoam&&race.Flow.State==RaceFlow.Stage.Racing&&valid&&error==null)
            for(int i=0;i<sites.Length;i++)
            {
                if(data.found.Contains(sites[i].id))continue;
                float t=delta.sqrMagnitude>.001f?Mathf.Clamp01(Vector3.Dot(sites[i].position-previous,delta)/delta.sqrMagnitude):0;
                if(Vector3.Distance(previous+delta*t,sites[i].position)>2.8f)continue;
                data.found.Add(sites[i].id);
                try{AtomicSave.Write(path,JsonUtility.ToJson(data,true));feedback=$"ACORN FOUND / {sites[i].title}\n{Found}/{sites.Length} discovered";feedbackUntil=Time.time+5;}
                catch(Exception e){data.found.Remove(sites[i].id);error="Collection could not save: "+e.Message;}
            }
            previous=p;
        }
        void LateUpdate()
        {
            if(tokens==null||!race)return;
            for(int i=0;i<tokens.Length;i++)
            {bool visible=race.FreeRoam&&!data.found.Contains(sites[i].id)&&(race.vehicle.transform.position-sites[i].position).sqrMagnitude<180*180;tokens[i].gameObject.SetActive(visible);if(visible){tokens[i].position=sites[i].position+Vector3.up*(Mathf.Sin(Time.time*2+i)*.16f);tokens[i].Rotate(0,Time.deltaTime*40,0);}}
        }
    }
}
