using UnityEngine;
namespace Racer
{
    // Informational only: no progress, penalty or vehicle mutation.
    public sealed class WrongWayGuidance : MonoBehaviour
    {
        public bool Visible { get; private set; }
        public float WrongSeconds { get; private set; }
        public Vector3 Direction { get; private set; }
        RaceDirector race; Vector3 previous; float station,correctSeconds; bool sampled;
        WoodlandRoute branch;
        void Awake(){race=GetComponent<RaceDirector>();}
        public void Clear(){Visible=false;WrongSeconds=correctSeconds=0;sampled=false;branch=null;}
        public void Observe(float signedSpeed,bool grounded,bool valid,float dt)
        {
            if(!valid||!grounded||Mathf.Abs(signedSpeed)<1){Visible=false;WrongSeconds=correctSeconds=0;return;}
            if(signedSpeed>1)
            {WrongSeconds=0;correctSeconds+=dt;if(correctSeconds>=.15f)Visible=false;return;}
            correctSeconds=0;
            if(signedSpeed<-(WrongSeconds>0?3:4.5f))
            {WrongSeconds+=dt;if(WrongSeconds>=3)Visible=true;}
            else {WrongSeconds=0;Visible=false;}
        }
        void FixedUpdate()
        {
            if(!race||!race.Flow||!race.road)return;
            if(race.Flow.State==RaceFlow.Stage.Paused)return;
            if(race.Flow.State!=RaceFlow.Stage.Racing||race.Progress.Finished){Clear();return;}
            var car=race.vehicle;var respawn=car.GetComponent<VehicleRespawn>();
            if(respawn.Pending){Clear();return;}
            var p=car.Body.position;var active=race.Racers[0].Branch.Route;
            if(!sampled||active!=branch||Vector3.Distance(p,previous)>12)
            {
                Clear();branch=active;station=branch?branch.Project(p,out _):race.road.Project(p,out _);
                previous=p;sampled=true;return;
            }
            float lateral;float current=branch?branch.Project(p,out lateral):race.road.ProjectNear(p,station,16,out lateral);
            var support=branch?branch.At(current,out var f):race.road.At(current,out f);
            Direction=Vector3.ProjectOnPlane(f,Vector3.up).normalized;
            float delta=current-station;
            if(!branch)delta=Mathf.Repeat(delta+race.road.Length*.5f,race.road.Length)-race.road.Length*.5f;
            float speed=delta/Time.fixedDeltaTime;
            bool valid=lateral<=(branch?branch.halfWidth:race.road.HalfWidth(current))+7 && Mathf.Abs(p.y-support.y)<4;
            // Both projection and real displacement must agree; nose orientation is irrelevant.
            if(Vector3.Dot(p-previous,Direction)*speed<0)valid=false;
            Observe(speed,car.GroundedWheels>=2,valid,Time.fixedDeltaTime);
            previous=p;station=current;
        }
    }
}
