using UnityEngine;
namespace Racer
{
    // Informational only: no progress, penalty or vehicle mutation.
    public sealed class WrongWayGuidance : MonoBehaviour
    {
        public bool Visible { get; private set; }
        public float WrongSeconds { get; private set; }
        public Vector3 Direction { get; private set; }
        public float SignedSpeed { get; private set; }
        public bool SampleValid { get; private set; }
        public const float Delay = 5f;
        RaceDirector race; Vector3 previous; float station,correctSeconds,uncertainSeconds,lostSeconds; bool sampled;
        WoodlandRoute branch; bool lastFallback;
        Vector3 lastRouteSupport,lastRouteForward;
        void Awake(){race=GetComponent<RaceDirector>();}
        public void Clear(){Visible=false;WrongSeconds=correctSeconds=uncertainSeconds=lostSeconds=0;sampled=false;branch=null;lastFallback=false;SignedSpeed=0;SampleValid=false;}
        public void Observe(float signedSpeed,bool grounded,bool valid,float dt)
        {
            if(dt<=0)return;
            // Short suspension/contact and projection noise freezes evidence, rather than
            // erasing seconds of real travel. Longer stops/flights end the observation.
            if(!valid||!grounded||Mathf.Abs(signedSpeed)<1)
            {uncertainSeconds+=dt;correctSeconds=0;if(uncertainSeconds>=.75f){Visible=false;WrongSeconds=0;}return;}
            if(signedSpeed>1)
            {correctSeconds+=dt;uncertainSeconds=0;if(correctSeconds>=.18f){Visible=false;WrongSeconds=0;}return;}
            if(signedSpeed<=-3)
            {correctSeconds=uncertainSeconds=0;WrongSeconds+=dt;if(WrongSeconds>=Delay)Visible=true;}
            else
            {correctSeconds=0;uncertainSeconds+=dt;if(uncertainSeconds>=.75f){Visible=false;WrongSeconds=0;}}
        }
        void FixedUpdate()
        {
            if(!race||!race.Flow||!race.road)return;
            if(race.Flow.State==RaceFlow.Stage.Paused)return;
            if(race.FreeRoam||race.Flow.State!=RaceFlow.Stage.Racing||race.Progress.Finished){Clear();return;}
            var car=race.vehicle;var respawn=car.GetComponent<VehicleRespawn>();
            if(respawn.Pending){Clear();return;}
            var p=car.Body.position;var active=race.Racers[0].Branch.Route;
            if(!sampled||Vector3.Distance(p,previous)>12)
            {
                Clear();branch=active;station=branch?branch.Project(p,out _):race.road.Project(p,out _);
                lastRouteSupport=branch?branch.At(station,out lastRouteForward):race.road.At(station,out lastRouteForward);
                previous=p;sampled=true;return;
            }
            if(active!=branch)
            {
                // A legal branch transition changes the coordinate system, not the
                // accumulated movement evidence. Skip one derivative at the join.
                branch=active;station=branch?branch.Project(p,out _):race.road.Project(p,out _);
                previous=p;return;
            }
            float lateral;float current=branch?branch.Project(p,out lateral):race.road.ProjectNear(p,station,16,out lateral);
            bool fallbackMain=false;
            if(branch && lateral>branch.halfWidth+7)
            {
                lostSeconds+=Time.fixedDeltaTime;
                // Keep the authorized deviation corridor attached to its shortcut.
                // Beyond it, a nearby main road can guide the driver without revoking
                // the separately held bypass entitlement.
                if(lostSeconds>=.25f&&lateral>branch.halfWidth+24)
                {
                    float main= race.road.Project(p,out float mainLateral);
                    if(mainLateral<=race.road.HalfWidth(main)+22)
                    {current=main;lateral=mainLateral;fallbackMain=true;}
                }
            }
            else if(!branch && lateral>race.road.HalfWidth(current)+7)
            {
                lostSeconds+=Time.fixedDeltaTime;
                if(lostSeconds>=.25f)
                {
                    // Reacquire spatially after a bounded search failure. Do not derive
                    // speed from the potentially large station jump on this frame.
                    float local=race.road.ProjectNear(p,station,240,out float localLateral);
                    float nearest=race.road.Project(p,out float nearestLateral);
                    current=localLateral<=nearestLateral+25?local:nearest;
                    lateral=Mathf.Min(localLateral,nearestLateral+25);
                }
            }
            else lostSeconds=0;
            bool branchSample=branch&&!fallbackMain;
            var support=branchSample?branch.At(current,out var f):race.road.At(current,out f);
            bool streetFallback=false;
            float coreWidth=branchSample?branch.halfWidth:race.road.HalfWidth(current);
            if(lateral<=coreWidth+3&&Mathf.Abs(p.y-support.y)<6){lastRouteSupport=support;lastRouteForward=f;}
            else if(race.Forest&&race.ambientRoad&&lostSeconds>=.25f)
            {
                float street=race.ambientRoad.Project(p,out float streetLateral);
                if(streetLateral<=race.ambientRoad.HalfWidth(street)+4)
                {
                    support=race.ambientRoad.At(street,out f);
                    float toward=Vector3.Dot(lastRouteSupport-p,f);
                    if(Mathf.Abs(toward)<5)toward=Vector3.Dot(lastRouteForward,f);
                    if(toward<0)f=-f;streetFallback=true;
                }
            }
            Direction=Vector3.ProjectOnPlane(f,Vector3.up).normalized;
            float delta=current-station;
            if(!branchSample)delta=Mathf.Repeat(delta+race.road.Length*.5f,race.road.Length)-race.road.Length*.5f;
            float movement=Vector3.Dot(p-previous,Direction)/Time.fixedDeltaTime;
            float speed=streetFallback||lostSeconds>=.25f||lastFallback!=fallbackMain?movement:delta/Time.fixedDeltaTime;
            // After a bounded off-course interval, keep the last locally associated
            // route direction usable on adjacent streets. Distance alone must not
            // permanently silence wrong-way guidance. No progress is awarded here.
            bool valid=(streetFallback || lostSeconds>=1 || lateral<=(branchSample?branch.halfWidth+24:race.road.HalfWidth(current)+22)) && Mathf.Abs(p.y-support.y)<60;
            // Both projection and real displacement must agree; nose orientation is irrelevant.
            if(Vector3.Dot(p-previous,Direction)*speed<0)valid=false;
            SignedSpeed=speed;SampleValid=valid;
            Observe(speed,car.GroundedWheels>=2,valid,Time.fixedDeltaTime);
            previous=p;station=current;lastFallback=fallbackMain;
        }
    }
}
