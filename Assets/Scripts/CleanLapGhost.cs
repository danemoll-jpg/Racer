using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
namespace Racer
{
    [DefaultExecutionOrder(1000)]
    public sealed class CleanLapGhost:MonoBehaviour
    {
        public const string Rules="clean-lap-v1/handling-cr087-v1";
        [Serializable] public struct Pose {public float t;public Vector3 p;public Quaternion q;}
        [Serializable] public sealed class Lap {public string key,date;public double seconds;public List<Pose> poses=new();}
        RaceDirector race; string root,key; Lap best; readonly List<Pose> recording=new(24000);
        Transform visual;Material material; double start; int misses,cursor; bool active,invalid,sampled;Vector3 previous;Quaternion previousRotation;float nextSample;
        public bool Enabled {get;private set;}
        public string Status {get;private set;}="No compatible ghost. Complete a clean lap.";
        public string Key=>race.courseId+"/"+(race.reverseCourse?"reverse":"forward")+"/"+race.vehicle.GetComponent<VehicleConfiguration>().profileId+"/"+Rules;
        public Quaternion CrossingRotation(float fraction,Quaternion current)=>sampled?Quaternion.Slerp(previousRotation,current,fraction):current;
        public void Initialize(RaceDirector owner,string saveRoot)
        {race=owner;root=Path.Combine(saveRoot,"CleanLapGhosts");try{Enabled=File.Exists(Path.Combine(root,"preference.txt"))&&File.ReadAllText(Path.Combine(root,"preference.txt"))=="on";}catch{Enabled=false;}race.vehicle.GetComponent<VehicleRespawn>().Respawned+=Invalidate;}
        public void Toggle(){Enabled=!Enabled;try{Directory.CreateDirectory(root);AtomicSave.Write(Path.Combine(root,"preference.txt"),Enabled?"on":"off");}catch(Exception e){Status="Ghost preference could not save: "+e.Message;}if(!Enabled&&visual)visual.gameObject.SetActive(false);}
        public void ResetSession(){active=false;recording.Clear();sampled=false;cursor=0;if(visual)visual.gameObject.SetActive(false);}
        public void Invalidate(){invalid=true;Status="Clean-lap ghost rejected: reset or teleport. Next lap can qualify.";}
        string FileName=>Path.Combine(root,Key.Replace('/','_')+".json");
        public void Refresh()
        {
            if(key==Key)return;key=Key;best=null;cursor=0;
            if(visual)Destroy(visual.gameObject);
            if(material)Destroy(material);
            try
            {
                if(File.Exists(FileName)&&new FileInfo(FileName).Length<12000000)
                {
                    var loaded=JsonUtility.FromJson<Lap>(File.ReadAllText(FileName));
                    if(loaded!=null&&loaded.key==key&&loaded.seconds>0&&loaded.poses.Count>=2&&loaded.poses.Count<=24002&&loaded.poses[0].t==0&&loaded.poses.Zip(loaded.poses.Skip(1),(a,b)=>b.t>a.t).All(x=>x))best=loaded;
                }
                Status=best!=null?"Clean-lap PB ghost / "+RaceHud.FormatTime(best.seconds):"No compatible clean-lap ghost. Historical files are preserved.";
            }catch(Exception e){Status="Ghost unavailable: "+e.Message;}
        }
        public void Boundary(double time,Vector3 crossing,Quaternion rotation,bool completed,bool finished)
        {
            Refresh();
            if(completed&&active)
            {
                double seconds=time-start;
                if(!invalid&&race.Progress.MissedGates==misses&&seconds>1&&seconds<=1200&&recording.Count>10)
                {
                    recording.Add(new Pose{t=(float)seconds,p=crossing,q=rotation});
                    if(best==null||seconds<best.seconds)
                    {
                        var lap=new Lap{key=key,date=DateTime.UtcNow.ToString("o"),seconds=seconds,poses=new(recording)};
                        try{Directory.CreateDirectory(root);AtomicSave.Write(FileName,JsonUtility.ToJson(lap));best=lap;Status="NEW CLEAN-LAP GHOST / "+RaceHud.FormatTime(seconds);}
                        catch(Exception e){Status="Ghost could not be saved: "+e.Message;}
                    }
                }
                else Status="Lap excluded from clean-lap ghosts: reset, teleport, missed gate or recording limit.";
            }
            active=!finished;invalid=false;start=time;misses=race.Progress.MissedGates;cursor=0;recording.Clear();nextSample=0;
            if(active)recording.Add(new Pose{t=0,p=crossing,q=rotation});
        }
        void FixedUpdate()
        {
            if(!race||!race.Flow)return;
            if(race.Flow.State!=RaceFlow.Stage.Racing){if(visual)visual.gameObject.SetActive(false);return;}
            var car=race.vehicle;var p=car.Body.position;
            if(sampled&&Vector3.Distance(p,previous)>Mathf.Max(3,car.Body.linearVelocity.magnitude*Time.fixedDeltaTime*2+.3f))Invalidate();
            previous=p;previousRotation=car.Body.rotation;sampled=true;
            if(race.FreeRoam||!active)return;
            invalid|=!race.Progress.LapValid||race.Progress.MissedGates!=misses;
            float t=(float)(race.Clock-start);
            if(t>1200||recording.Count>=24000)invalid=true;
            if(!invalid&&t>=nextSample&&t>recording[^1].t){recording.Add(new Pose{t=t,p=p,q=car.Body.rotation});nextSample=t+.05f;}
        }
        void LateUpdate()
        {
            if(!race||!race.Flow)return;Refresh();
            bool show=Enabled&&best!=null&&active&&!race.FreeRoam&&race.Flow.State==RaceFlow.Stage.Racing;
            if(!show){if(visual)visual.gameObject.SetActive(false);return;}
            if(!visual)
            {
                visual=new GameObject("Personal best clean-lap ghost / visual only").transform;
                var body=VehicleVisual.Build(visual,race.vehicle.GetComponent<VehicleConfiguration>().Profile);
                material=new Material(Shader.Find("Universal Render Pipeline/Lit")){color=new Color(.3f,.9f,1,.3f),renderQueue=3000};
                material.SetFloat("_Surface",1);material.SetFloat("_SrcBlend",5);material.SetFloat("_DstBlend",10);material.SetFloat("_ZWrite",0);material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                foreach(var c in visual.GetComponentsInChildren<Collider>()){c.enabled=false;Destroy(c);}
                foreach(var r in body.GetComponentsInChildren<Renderer>()){r.sharedMaterials=Enumerable.Repeat(material,r.sharedMaterials.Length).ToArray();r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;}
            }
            float t=(float)(Time.timeAsDouble-start);var poses=best.poses;
            while(cursor+1<poses.Count-1&&poses[cursor+1].t<t)cursor++;
            var a=poses[cursor];var b=poses[Mathf.Min(cursor+1,poses.Count-1)];float u=Mathf.InverseLerp(a.t,b.t,t);
            visual.gameObject.SetActive(t<=best.seconds);visual.SetPositionAndRotation(Vector3.Lerp(a.p,b.p,u),Quaternion.Slerp(a.q,b.q,u));
        }
        void OnDestroy(){if(race)race.vehicle.GetComponent<VehicleRespawn>().Respawned-=Invalidate;if(visual)Destroy(visual.gameObject);if(material)Destroy(material);}
    }
}

