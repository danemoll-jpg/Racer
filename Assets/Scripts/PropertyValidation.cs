using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Racer
{
    public sealed class PropertyValidation:MonoBehaviour
    {
        static string Arg(string key,string fallback=""){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,key);return i>=0&&i+1<a.Length?a[i+1]:fallback;}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){if(Arg("-propertyCheck")==""||Arg("-racerTestSave")=="")return;var o=new GameObject("CR097 physical property validation");DontDestroyOnLoad(o);o.AddComponent<PropertyValidation>();}
        string dir;RaceDirector race;RaceFlow flow;ArcadeVehicle car;readonly List<string> checks=new();
        void Check(bool ok,string text){checks.Add((ok?"PASS ":"FAIL ")+text);File.WriteAllLines(dir+"/checks.txt",checks);}
        static float Ground(Vector3 p)=>Physics.RaycastAll(new(p.x,300,p.z),Vector3.down,600).Where(h=>h.collider.name.StartsWith("Ground_")).OrderBy(h=>h.distance).First().point.y;
        void Place(Vector3 p,Vector3 f){p.y=Ground(p)+car.suspensionLength-.12f;car.Body.position=p;car.Body.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(f,Vector3.up));car.transform.SetPositionAndRotation(p,car.Body.rotation);car.Body.linearVelocity=car.Body.angularVelocity=Vector3.zero;car.ClearSteering();Physics.SyncTransforms();race.ResetSampling(p,Time.timeAsDouble);FindAnyObjectByType<ChaseCamera>()?.Snap();}
        IEnumerator Start()
        {
            Application.runInBackground=true;dir=Arg("-evidence");Directory.CreateDirectory(dir);string course=Arg("-course","StreetLoopGreybox");if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name!=course)UnityEngine.SceneManagement.SceneManager.LoadScene(course);yield return null;yield return null;
            race=FindAnyObjectByType<RaceDirector>();flow=race.Flow;car=race.vehicle;race.opponents=race.traffic=false;flow.StartFreeRoam();car.enabled=false;car.GetComponent<VehicleInput>().enabled=false;
            Time.timeScale=float.TryParse(Arg("-testSpeed","1"),out var speedup)?speedup:1;
            var home=GameObject.Find("Dan - blue X").transform;var h2=GameObject.Find("Original house 2").transform;
            Check(Vector3.Dot(home.forward,h2.forward)>.9999f,"Dan and House 2 fronts parallel without changing positions");
            Check(!FindObjectsByType<Transform>().Any(t=>t.name.StartsWith("Friend across street - blue circle")&&t.name.Contains("fence")),"Kyle has no remaining named fence");
            Check(FindObjectsByType<Transform>().Count(t=>t.name=="Seated guy")==2&&GameObject.Find("Small domed camping tent"),"Dome tent and two seated campers retained");
            Check(FindObjectsByType<Transform>().Count(t=>t.name.StartsWith("Acorn clue / "))==22,"22 hidden collectible clue cairns preserved");
            var endpoints=FindObjectsByType<Transform>().Where(t=>t.name=="Fence endpoint A"||t.name=="Fence endpoint B").ToArray();
            var gaps=endpoints.Select(t=>new{point=t.position,gap=Mathf.Abs(t.position.y-Ground(t.position))}).ToArray();
            File.WriteAllLines(dir+"/fence-support.csv",new[]{"x,y,z,gap"}.Concat(gaps.Select(g=>$"{g.point.x:F3},{g.point.y:F3},{g.point.z:F3},{g.gap:F3}")));
            Check(gaps.Length>100&&gaps.All(g=>g.gap<.3f),"Every repaired/retained fence endpoint grounded; endpoints="+gaps.Length+" maxGap="+gaps.Select(g=>g.gap).DefaultIfEmpty(999).Max());
            Check(FindObjectsByType<BreakableProp>().Any(f=>f.name=="Grounded retained neighbor fence"),"Neighbor fencing retained across Kyle's street");
            bool approach=Arg("-propertyCheck")=="approach";
            var road=GameObject.Find(approach?"Summit approach and safe return":"House 3 valley driveway").GetComponent<RaceRoad>();
            var route=approach?road.points.Take(5).ToArray():new[]{road.points[0]-Vector3.ProjectOnPlane(road.points[1]-road.points[0],Vector3.up).normalized*8}.Concat(road.points).ToArray();
            foreach(var profile in race.EligibleVehicles){car.GetComponent<VehicleConfiguration>().Apply(profile.Id);
                foreach(bool reverse in new[]{false,true}){
                    var points=reverse?route.Reverse().ToArray():route;Place(points[0],points[1]-points[0]);BreakableProp.RestoreRace();int target=1;float began=Time.time,minUp=1,maxGap=0;string id=profile.Id+(reverse?"-out":"-in");var broken=new List<string>();void Broken(BreakableProp prop,ArcadeVehicle vehicle){if(vehicle==car)broken.Add(prop.name);}BreakableProp.BrokenByVehicle+=Broken;
                    using(var trace=new StreamWriter(dir+"/house3-"+id+".csv")){
                        trace.WriteLine("time,x,y,z,speed,wheels,up,target,groundGap,throttle,brake,steer");
                        while(Time.time-began<100&&target<points.Length){
                            while(target<points.Length-1&&Vector3.ProjectOnPlane(car.Body.position-points[target],Vector3.up).magnitude<5)target++;
                            var local=car.transform.InverseTransformPoint(points[target]);float steer=Mathf.Clamp(Mathf.Atan2(local.x,local.z)*2,-1,1);float desired=7;float throttle=Mathf.Clamp01((desired-car.ForwardSpeed)*.7f);float brake=car.ForwardSpeed>desired+.5f?.35f:0;
                            car.Simulate(throttle,brake,steer,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();var p=car.Body.position;minUp=Mathf.Min(minUp,car.transform.up.y);float gap=p.y-Ground(p);maxGap=Mathf.Max(gap,maxGap);trace.WriteLine($"{Time.time-began:F3},{p.x:F3},{p.y:F3},{p.z:F3},{car.ForwardSpeed:F3},{car.GroundedWheels},{car.transform.up.y:F3},{target},{gap:F3},{throttle:F3},{brake:F3},{steer:F3}");
                            if(Vector3.ProjectOnPlane(p-points.Last(),Vector3.up).magnitude<4){target=points.Length;break;}
                        }
                    }
                    Check(target==points.Length&&minUp>.65f&&maxGap<2.5f,id+" physically drove "+road.name+"; target="+target+"/"+points.Length+" minUp="+minUp+" maxGap="+maxGap);
                    BreakableProp.BrokenByVehicle-=Broken;Check(!broken.Any(n=>n.ToLowerInvariant().Contains("fence")||n.Contains("chain-link")||n.Contains("crossbuck")),id+" driveway clear of fencing; broken="+string.Join(";",broken));
                    ThreeFeatureValidation.CaptureUi(dir+"/house3-"+id+".png");Check(car.GetComponent<VehicleRespawn>().TryRecoverLocal(),id+" safe local reset");
                }
            }
            File.WriteAllText(dir+"/done.txt","Completed physical driving; inspect failures. Automated, no human/controller acceptance.");Application.Quit();
        }
    }
}
