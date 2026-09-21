using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Racer
{
    public sealed class RaceDirector : MonoBehaviour
    {
        public ArcadeVehicle vehicle;
        public RaceGate[] gates;
        [Min(0)]
        public int laps = 3;
        public RaceRoad road;
        public RaceRoad ambientRoad;
        public RaceRoad throughRoad;
        public bool reverseCourse;
        public bool Forest => (road&&road.forestTrail)||courseId=="lake-v2-forest" || courseId=="lake-v3-shallows";
        public VehicleProfile[] EligibleVehicles => Forest ? VehicleProfile.All.Where(p=>p.Small).ToArray() : VehicleProfile.All;
        public string EligibleVehicle(string id)=>Forest&&!VehicleProfile.Find(id).Small?"moto":VehicleProfile.Find(id).Id;
        public string courseId="street-v8-landings";
        public string courseName="Street Loop";
        public const double OrdinaryMissPenalty = 5;
        public bool opponents = true, traffic = true;
        public bool FreeRoam {get;set;}
        public int difficulty = 1;
        public string[] opponentRoster = {"tourer","moto","atv"};
        public string RosterLabel => string.Join(" / ",opponentRoster.Select(id=>VehicleProfile.Find(id).Name));
        public string DifficultyName => new[]{"Easy", "Normal", "Hard"}[Mathf.Clamp(difficulty,0,2)];
        public string ModeLabel => opponents ? "Race vs 3 AI / " + DifficultyName : "Solo / time trial";
        [Range(0, 6)]
        public int trafficCount = 4;
        [Range(0,24)] public int highwayTrafficCount = 16;
        public float finishGraceSeconds = 90, maximumRaceSeconds = 1200;
        public List<RacerState> Racers { get; } = new();
        public List<RoadDriver> Drivers { get; } = new();
        public RaceProgress Progress => Racers.Count > 0 ? Racers[0].Progress : null;
        public double Clock { get; private set; }

        public RaceFlow Flow { get; private set; }

        public bool ClassificationFinal { get; private set; }

        public int PlayerPosition => Ordered(false).IndexOf(Racers[0]) + 1;
        public string Category => $"{courseId}-{(vehicle.GetComponent<VehicleConfiguration>() ? vehicle.GetComponent<VehicleConfiguration>().profileId : "original")}-{(opponents ? "race4-d" + difficulty+"-"+string.Join("-",opponentRoster) : "solo")}-{(traffic ? "traffic" : "clear")}-laps{laps}";
        VehicleRespawn respawn;
        float origin;
        float[] gateS;
        public WoodlandRoute[] Branches { get; private set; }
        public float Origin => origin;
        double startedAt, firstFinish = -1;
        GameObject gridVisual;
        static Material gridPaint;
        void Awake()
        {
            if (!vehicle || gates == null || gates.Length < 2)
            {
                enabled = false;
                return;
            }

            Flow = GetComponent<RaceFlow>();
            if(!GetComponent<WrongWayGuidance>())gameObject.AddComponent<WrongWayGuidance>();
            respawn = vehicle.GetComponent<VehicleRespawn>();
            Racers.Add(new RacerState("YOU", vehicle, gates.Length - 1, laps));
            if (road)
            {
                road.Initialize();
                Branches = FindObjectsByType<WoodlandRoute>();
                origin = road.Project(gates[0].transform.position, out _);
                gateS = gates.Select(g => road.Relative(road.Project(g.transform.position, out _), origin)).ToArray();
            }
        }

        void OnEnable()
        {
            if (respawn)
                respawn.Respawned += OnRespawn;
        }

        void OnDisable()
        {
            if (respawn)
                respawn.Respawned -= OnRespawn;
        }

        void OnRespawn()
        {
            GetComponent<WrongWayGuidance>()?.Clear();
            // Recovery is neither a gate crossing nor a new lap; retain all earned progress.
            Racers[0].SampleOrigin(Clock);
            Racers[0].FinishApproach=0;
            Racers[0].Branch.Recovered(vehicle.Body.position);
            if (Flow)
                Flow.ResetFeedback();
        }

        public void ResetSampling(Vector3 position, double now)
        {
            Racers[0].Previous = position;
            Racers[0].PreviousTime = Clock = now;
        }

        public void RestartRace()
        {
            GetComponent<WrongWayGuidance>()?.Clear();
            DriverVariation.Seed=AmbientLife.ForcedSeed!=0?AmbientLife.ForcedSeed:System.Environment.TickCount;
            GetComponent<AmbientLife>()?.SelectScenes();
            GetComponent<Wildlife>()?.SelectPopulation(true);
            if(Forest)
            {
                var configuration=vehicle.GetComponent<VehicleConfiguration>();
                if(!configuration.Profile.Small)configuration.Apply(EligibleVehicle(configuration.profileId));
                for(int i=0;i<opponentRoster.Length;i++)opponentRoster[i]=EligibleVehicle(opponentRoster[i]);
            }
            if (Flow)
                Flow.PrepareRestart();
            foreach (var d in Drivers)
                if (d)
                {
                    d.gameObject.SetActive(false);
                    Destroy(d.gameObject);
                }

            Drivers.Clear();
            Racers.RemoveRange(1, Racers.Count - 1);
            respawn.RestartAtStart();
            laps=opponents?Mathf.Clamp(laps,1,5):Mathf.Clamp(laps,0,5);
            Progress.ConfigureLaps(laps);
            Racers[0].Dnf=false; Racers[0].FinishArmed=false; Racers[0].RecoveryStart=float.NaN; Racers[0].Recoveries=0;
            if (road && opponents && !FreeRoam)
            {
                var grid = road.At(origin - 32, out var direction);
                vehicle.Body.position = grid - Vector3.Cross(Vector3.up,direction).normalized * 2.2f + Vector3.up * Mathf.Max(.4f,vehicle.suspensionLength-Physics.gravity.magnitude/vehicle.springStrength);
                vehicle.Body.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(direction,Vector3.up));
                vehicle.transform.SetPositionAndRotation(vehicle.Body.position,vehicle.Body.rotation);
                FindAnyObjectByType<ChaseCamera>()?.Snap();
            }
            if (road)
                CreateCars();
            ShowGrid();
            foreach (var r in Racers)
            {
                r.SampleOrigin(Time.timeAsDouble);
                r.Travel = 0;
                r.VerifiedRoad = 0;
                r.FinishApproach = 0;
                r.Branch.Clear();
            }

            firstFinish = -1;
            ClassificationFinal = false;
            startedAt = Time.timeAsDouble + 3;
            BreakableProp.RestoreRace();
            SmashAudio.Prepare();
            ResetSampling(vehicle.Body.position, Time.timeAsDouble);
            GetComponent<Wildlife>()?.CompleteTurkeyVisit();
            if (Flow)
            {
                Flow.SelectRecords(Category);
                if(FreeRoam)Flow.BeginRoaming();else Flow.BeginCountdown();
            }
        }

        public void AbandonEvent()
        {
            foreach(var driver in Drivers) if(driver) { driver.gameObject.SetActive(false); Destroy(driver.gameObject); }
            Drivers.Clear(); Racers.RemoveRange(1,Racers.Count-1);
            if(gridVisual) { gridVisual.SetActive(false); Destroy(gridVisual); }
            Progress.Restart(); Racers[0].Branch.Clear(); Racers[0].Dnf=false; Racers[0].FinishArmed=false;
            Clock=0; firstFinish=-1; ClassificationFinal=false;
        }

        void CreateCars()
        {
            bool savedOpponents=opponents;if(FreeRoam)opponents=false;
            float spawn = road.Project(vehicle.transform.position, out _);
            int localPopulation = Mathf.Clamp(trafficCount, 0, 6);
            int population = localPopulation + Mathf.Clamp(highwayTrafficCount,0,24);
            Color[] colors = {new(.95f, .25f, .12f), new(.95f, .75f, .1f), new(.2f, .55f, 1)};
            for (int i = 0; i < (opponents ? 3 : 0) + (traffic ? population : 0); i++)
            {
                bool racing = opponents && i < 3;
                int n = racing ? i : i - (opponents ? 3 : 0);
                var clone = Instantiate(vehicle.gameObject);
                clone.name = racing ? new[]{"EMBER", "GOLD", "BLUE"}[n] : "Traffic " + (n + 1);
                var car = clone.GetComponent<ArcadeVehicle>();
                foreach(var activityContact in clone.GetComponents<ActivityLandingContact>())Destroy(activityContact);
                var configuration = clone.GetComponent<VehicleConfiguration>();
                if (!configuration) configuration = clone.AddComponent<VehicleConfiguration>();
                // Every opponent uses the resolved real physics/visual profile.
                configuration.Apply(racing ? opponentRoster[n] : "original");
                configuration.SetPaint(racing?colors[n]:new Color(.55f,.55f,.5f));
                // Explicit test pilots must never be duplicated into opponents.
                foreach (var inherited in clone.GetComponents<RoadDriver>()) { inherited.enabled = false; Destroy(inherited); }
                clone.GetComponent<VehicleInput>().enabled = false;
                clone.GetComponent<VehicleRespawn>().enabled = false;
                var sound = clone.GetComponent<VehicleAudio>();
                if (sound)
                {
                    sound.enabled = false;
                    Destroy(sound);
                }

                foreach (var source in clone.GetComponents<AudioSource>())
                    Destroy(source);
                foreach (var renderer in clone.GetComponentsInChildren<Renderer>())
                    if (VehiclePaint.IsBodyPaint(renderer.sharedMaterial))
                    {
                        var block = new MaterialPropertyBlock();
                        block.SetColor("_BaseColor", racing ? colors[n] : new Color(.55f, .55f, .5f));
                        renderer.SetPropertyBlock(block);
                    }

                if(!racing)clone.AddComponent<AmbientVehicle>().Initialize();
                car.enabled = false;
                car.Body.isKinematic = false;
                var driver = clone.AddComponent<RoadDriver>();
                driver.Initialize(this, car, racing, racing ? 1 : (n % 2 == 0 ? 1 : -1), .94f + n * .025f);
                if(!racing) driver.Initialize(this,car,false,n%2==0?1:-1,.92f+(n%4)*.025f);
                driver.HighwayTraffic=!racing && n>=localPopulation;
                Drivers.Add(driver);
                if (racing)
                {
                    var state = new RacerState(RacePlaylists.Active!=null?"Rival "+Racers.Count:clone.name, car, gates.Length - 1, laps, true);
                    Racers.Add(state);
                    driver.Racer = state;
                }

                int h=n-localPopulation;
                var driveRoad=driver.DriveRoad;
                float station=driver.HighwayTraffic?(throughRoad?Mathf.Lerp(100,driveRoad.Length-100,(h+.5f)/Mathf.Max(1,highwayTrafficCount)):3800+(h/4)*175+(h%4)*22):(!racing&&ambientRoad?driveRoad.Project(vehicle.transform.position,out _):spawn)+200+n*driveRoad.Length/Mathf.Max(1,localPopulation);
                driver.Place(racing ? spawn + 8 + n * 7 : station, racing ? (n % 2 == 0 ? 2.2f : -2.2f) : driveRoad.TrafficLane(station,driver.Direction,n%4>=2));
            }
            opponents=savedOpponents;
        }

        void ShowGrid()
        {
            if(gridVisual) { gridVisual.SetActive(false); Destroy(gridVisual); }
            if(!opponents || !road || FreeRoam) return;
            if(!gridPaint) gridPaint=new Material(Shader.Find("Universal Render Pipeline/Lit")){color=new Color(.88f,.87f,.68f)};
            gridVisual=new GameObject("Starting grid paint");
            foreach(var racer in Racers)
            {
                float s=road.Project(racer.Car.transform.position,out _);
                var center=road.At(s,out var forward);
                var right=Vector3.Cross(Vector3.up,forward).normalized;
                center+=right*Vector3.Dot(racer.Car.transform.position-center,right)+Vector3.up*.04f;
                foreach(float side in new[]{-1.2f,1.2f})
                {
                    var bar=GameObject.CreatePrimitive(PrimitiveType.Cube); bar.name="Grid stripe"; bar.transform.SetParent(gridVisual.transform);
                    bar.transform.SetPositionAndRotation(center+right*side,Quaternion.LookRotation(forward)); bar.transform.localScale=new(.09f,.015f,5.2f);
                    bar.GetComponent<Collider>().enabled=false; Destroy(bar.GetComponent<Collider>()); bar.GetComponent<Renderer>().sharedMaterial=gridPaint;
                }
            }
        }

        void FixedUpdate()
        {
            if (Flow && Flow.State != RaceFlow.Stage.Racing)
                return;
            Clock = Time.fixedTimeAsDouble;
            if(FreeRoam)return;
            Sample(vehicle.Body.position, vehicle.transform.forward, Clock);
            for (int i = 1; i < Racers.Count; i++)
                SampleRacer(Racers[i], Racers[i].Car.Body.position, Racers[i].Car.transform.forward, Clock, false);
            foreach(var r in Racers)
                if(r.IsAi && !r.Classified && !r.Dnf)
                    r.Estimate.Sample(Clock,RemainingDistance(r),r.Car.GetComponent<VehicleConfiguration>().Profile.Speed,!float.IsNaN(r.RecoveryStart));
            if(Progress.Finished && Flow && Flow.Save.Settings.estimateAiFinishes) FinalizeUnfinishedAi();
            if (Racers.Any(r => r.Progress.Finished) && firstFinish < 0)
                firstFinish = Clock;
            if (!Progress.Unlimited && (Clock - startedAt >= maximumRaceSeconds*Mathf.Max(1,laps/3f) || (firstFinish >= 0 && Clock - firstFinish >= finishGraceSeconds)))
                foreach (var r in Racers)
                    if (!r.Classified)
                        r.Dnf = true;
            if (!ClassificationFinal && Racers.All(r => r.Classified || r.Dnf))
            {
                ClassificationFinal = true;
                Flow?.CompleteResults();
            }
        }

        public void Sample(Vector3 position, Vector3 heading, double now) => SampleRacer(Racers[0], position, heading, now, true);
        void SampleRacer(RacerState r, Vector3 position, Vector3 heading, double now, bool player)
        {
            if (FreeRoam || (Flow && Flow.State != RaceFlow.Stage.Racing) || r.Classified || r.Dnf)
                return;
            var p = r.Progress;
            int completed = p.CompletedLaps, misses = p.MissedGates;
            double oldPenalty = p.PenaltySeconds;
            bool credited = false;
            float step = Vector3.Distance(position, r.Previous);
            if (step > 10)
            {
                if(player)Flow?.Ghost?.Invalidate();
                p.ResetToGrid();
                r.FinishArmed = false;
                r.FinishApproach = 0;
                r.Travel = 0;
                r.VerifiedRoad = 0;
                r.Branch.Clear();
                r.SampleOrigin(now);
                return;
            } // Teleports do not create missed-gate chains.

            if (road && !float.IsNaN(r.RecoveryStart))
            {
                float advance = road.Relative(road.Project(position, out _), r.RecoveryStart);
                if (advance < 8 || advance > road.Length * .5f)
                {
                    r.Previous = position;
                    r.PreviousTime = now;
                    return;
                }

                r.RecoveryStart = float.NaN;
            }

            float lateral = 0;
            if (road)
                r.RoadPosition = road.Relative(road.Project(position, out lateral), origin);
            if (road && p.LapActive && p.LapValid && Branches != null)
            {
                if (!r.Branch.Route)
                    foreach (var branch in Branches)
                    {
                        int expected = System.Array.FindIndex(gateS, s => s > road.Relative(branch.entryRoad,origin));
                        if (expected <= 0 || p.NextGate < expected || p.NextGate > expected+1 || !branch.Enter(r.Previous,position,heading)) continue;
                        r.Branch.Begin(branch);
                        // A recognized entry grants only this route's authored bypass list.
                        // Ordered Cross cannot consume an unrelated gate or a future lap.
                        foreach(int gate in branch.bypassedGates)
                            if(p.NextGate==gate) p.Cross(gate,true,now);
                        Trace(r,"entry entitlement",position,now);
                        break;
                    }
                if (r.Branch.Route)
                {
                    var branch = r.Branch.Route;
                    bool exited = r.Branch.Advance(r.Previous,position,heading);
                    branch.Project(position,out float branchLateral);
                    // A brief shoulder excursion or reverse is recoverable. Only sustained
                    // forward travel on a separated road relinquishes branch tracking.
                    road.At(origin+r.RoadPosition,out var roadForward);
                    bool separatedRoad = lateral < 9 && branchLateral > branch.halfWidth+10
                        && Vector3.Dot(position-r.Previous,roadForward)>.01f;
                    r.Branch.RejoinSeconds = separatedRoad ? r.Branch.RejoinSeconds+(float)(now-r.PreviousTime) : 0;
                    bool beyondExit = r.RoadPosition > road.Relative(branch.exitRoad,origin)+40 && lateral<18
                        && (!courseId.StartsWith("mountain-") || branchLateral>branch.halfWidth+3);
                    bool abandoned = !exited && (r.Branch.RejoinSeconds > 1.5f || beyondExit);
                    // Entry credit is already final. Completion/abandonment only ends context.
                    if(exited)
                    {
                        foreach(int gate in branch.bypassedGates)
                            if(p.NextGate==gate) p.Cross(gate,true,now);
                        r.RoadPosition=road.Relative(branch.exitRoad,origin);
                        int last=p.NextGate==0?gates.Length-1:p.NextGate-1;
                        r.Travel=Mathf.Max(0,r.RoadPosition-gateS[last]);
                        r.VerifiedRoad=r.RoadPosition;
                        Trace(r,"completed",position,now); r.Branch.Clear();
                        // A verified exit earns this road position even if reset is
                        // pressed before the next supported-history sample.
                        r.Car.GetComponent<VehicleRespawn>()?.SeedCoursePosition(position);
                    }
                    else if(abandoned) { Trace(r,"deliberate road rejoin; credit retained",position,now); r.Branch.Clear(); }
                    else
                    {
                        // Retained context must never hide a real expected road-gate crossing,
                        // including a slow partial rejoin. Finish still requires branch resolution.
                        for(int gate=1;gate<gates.Length;gate++)
                            if(p.NextGate==gate && gates[gate].TryCross(r.Previous,position,r.Car.GetComponent<BoxCollider>(),out bool gateForward,out float crossing)
                                && gateForward)
                                p.Cross(gate,true,r.PreviousTime+(now-r.PreviousTime)*crossing);
                        r.RoadPosition=road.Relative(r.Branch.RoadPosition,origin);
                        if(r.RoadPosition>road.Length*.35f && r.RoadPosition<road.Length*.8f) r.FinishArmed=true;
                        r.Previous=position; r.PreviousTime=now;
                        if(player) Clock=now;
                        return;
                    }
                }
            }
            // Retain observed forward progress for diagnostics; it never changes penalty size.
            if (road && p.LapActive)
            {
                if (lateral < 18) r.Travel += Mathf.Min(step, Mathf.Max(0, r.RoadPosition - r.VerifiedRoad));
                // A wrap caused by reversing across START is not a new circuit's
                // witnessed forward road progress. Branch exits seed this frontier.
                if(r.RoadPosition<=r.VerifiedRoad+Mathf.Min(12,step*2+1))
                    r.VerifiedRoad = Mathf.Max(r.VerifiedRoad, r.RoadPosition);
            }
            if (road && p.LapActive && r.VerifiedRoad > road.Length * .35f && r.RoadPosition > road.Length * .35f && r.RoadPosition < road.Length * .8f)
                r.FinishArmed = true;
            if(road && p.LapActive && p.LapValid && r.FinishArmed && r.RoadPosition>road.Length-160 && lateral<60)
            {
                road.At(origin+r.RoadPosition,out var approachDirection);
                r.FinishApproach+=Mathf.Clamp(Vector3.Dot(position-r.Previous,approachDirection),-step,step);
                r.FinishApproach=Mathf.Clamp(r.FinishApproach,0,160);
            }
            for (int i = 0; i < gates.Length; i++)
            {
                bool opening=gates[i].TryCross(r.Previous, position,r.Car.GetComponent<BoxCollider>(), out bool forward, out float fraction);
                bool missedFinish=!opening && i==0 && road && p.LapActive && p.LapValid && r.FinishArmed
                    && r.FinishApproach>=20 && r.Branch.Route==null
                    && gates[0].TryFinishRegion(r.Previous,position,out fraction);
                if(missedFinish)forward=true;
                if (!opening && !missedFinish)
                    continue;
                if (!forward)
                    continue;
                if (i == 0 && p.LapActive && !r.FinishArmed)
                    continue;
                if (p.LapActive && p.NextGate > 0 && (i == 0 || i > p.NextGate))
                    ResolveMisses(r, i == 0 ? gates.Length : i, i==0?"finish reconciliation":"later gate crossed");
                int expected = p.NextGate;
                int beforeLaps=p.CompletedLaps;bool beforeActive=p.LapActive;
                double crossTime=r.PreviousTime + (now - r.PreviousTime) * fraction;
                if(missedFinish && p.MissFinish(crossTime) && player)Flow?.Ghost?.Invalidate();
                p.Cross(i, true, crossTime);
                if(player&&i==0&&(p.CompletedLaps>beforeLaps||(!beforeActive&&p.LapActive)))
                    Flow?.Ghost?.Boundary(crossTime,Vector3.Lerp(r.Previous,position,fraction),Flow.Ghost.CrossingRotation(fraction,r.Car.Body.rotation),p.CompletedLaps>beforeLaps,p.Finished);
                if (i > 0 && i == expected && p.NextGate != expected)
                {
                    credited = true;
                    r.Travel = 0;
                    // A witnessed ordered gate is an authoritative route anchor,
                    // including Forest's offset start-plane projection.
                    if(road)r.VerifiedRoad=Mathf.Max(r.VerifiedRoad,gateS[i]);
                }

                if (i == 0)
                {
                    r.Travel = 0;
                    r.FinishArmed = false;
                    r.FinishApproach = 0;
                    r.VerifiedRoad = road&&r.RoadPosition<30?r.RoadPosition:0;
                }
            }

            if (road && p.LapActive && p.NextGate > 0 && lateral < 18 && step > .005f)
            {
                road.At(origin + r.RoadPosition, out var direction);
                if (Vector3.Dot(position - r.Previous, direction) > .001f)
                    while (p.NextGate > 0 && r.VerifiedRoad > gateS[p.NextGate] + 18 && r.RoadPosition > gateS[p.NextGate] + 18 && r.RoadPosition < road.Length - 20)
                        ResolveMisses(r, p.NextGate + 1,"road gate passed outside span");
            }

            r.Previous = position;
            r.PreviousTime = now;
            if (player)
            {
                Clock = now;
                if (p.MissedGates > misses)
                    Flow?.CheckpointFeedback(false, p.PenaltySeconds - oldPenalty, p.MissedGates - misses);
                else if (credited && p.CompletedLaps == completed)
                    Flow?.CheckpointFeedback(true, 0, 0);
                if (p.CompletedLaps > completed)
                    Flow?.LapCompleted();
            }
        }

        void Trace(RacerState r,string reason,Vector3 position,double now)
        {
            if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-raceTrace")>=0)
                Debug.Log($"RACE_TRANSITION lap={r.Progress.CompletedLaps+1} racer={r.Name} reason={reason} branch={r.Branch.Route?.title??"none"} position={position} time={now:F3} next={r.Progress.NextGate} misses={r.Progress.MissedGates} seconds={r.Progress.PenaltySeconds}");
        }
        void ResolveMisses(RacerState r, int until, string reason="missed gate")
        {
            var p = r.Progress;
            while (p.NextGate > 0 && p.NextGate < until)
            {
                int gate = p.NextGate;
                float sector = road ? gateS[gate] - gateS[gate - 1] : 230;
                const double penalty = OrdinaryMissPenalty;
                if (!p.Miss(gate, penalty, reason, r.Branch.Route?.title??"none",Clock))
                    break;
                if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-raceTrace")>=0)
                    Debug.Log($"RACE_PENALTY racer={r.Name} source=ordinary-missed-gate gate={gate} seconds=5 road={r.RoadPosition:F3} branch={r.Branch.Route?.title??"none"} earned={r.Branch.Earned:F3}");
                r.Travel = Mathf.Max(0, r.Travel - sector);
            }
        }

        public float RemainingDistance(RacerState r)
        {
            float length=road?road.Length:4000;
            var p=r.Progress;
            float current=length;
            if(p.LapActive && p.LapValid && road)
            {
                if(r.Branch.Route)
                {
                    var branch=r.Branch.Route;
                    current=Mathf.Max(0,branch.Length-r.Branch.Position)+length-road.Relative(branch.exitRoad,origin);
                }
                else
                {
                    int last=p.NextGate==0?gateS.Length-1:Mathf.Max(0,p.NextGate-1);
                    float end=p.NextGate==0?length:gateS[p.NextGate];
                    road.Project(r.Car.Body.position,out float lateral);
                    // Outside the supported corridor retain only earned gate progress.
                    float station=lateral<=18?r.RoadPosition:gateS[last];
                    current=length-Mathf.Clamp(station,gateS[last],end);
                }
            }
            return Mathf.Max(0,(p.TargetLaps-p.CompletedLaps-1)*length+current);
        }
        public void FinalizeUnfinishedAi()
        {
            if(!Progress.Finished || ClassificationFinal) return;
            foreach(var r in Racers)
            {
                if(!r.IsAi || r.Classified || r.Dnf) continue;
                var profile=r.Car.GetComponent<VehicleConfiguration>().Profile;
                if(r.FinalizeEstimate(Clock,r.Estimate.Duration(Clock,RemainingDistance(r),profile,Forest,difficulty)))
                {
                    // Freeze in place and remove contact participation; no gate or transform changes.
                    if(!r.Car.Body.isKinematic) r.Car.Body.linearVelocity=r.Car.Body.angularVelocity=Vector3.zero;
                    r.Car.Body.isKinematic=true; r.Car.Body.detectCollisions=false;
                }
            }
            if(Racers.All(r=>r.Classified||r.Dnf)) { ClassificationFinal=true; Flow?.CompleteResults(); }
        }
        public List<RacerState> Ordered(bool final) => final ? Racers.OrderBy(r => !r.Classified).ThenBy(r => r.Classified ? r.ClassifiedTime(Clock) : -Score(r)).ToList() : Racers.OrderByDescending(Score).ThenBy(r => r.Classified ? r.ClassifiedTime(Clock) : 0).ToList();
        float Score(RacerState r) => r.Progress.Finished ? laps * (road ? road.Length : 4000) : r.Progress.CompletedLaps * (road ? road.Length : 4000) + (r.Progress.LapActive ? r.RoadPosition : -1);
        public string Standings() => string.Join("\n", Ordered(true).Select((r, i) => $"{i + 1}. {r.Name}  {(r.Dnf ? "DNF" : r.Classified ? (r.Estimated?"~ ":"")+RaceHud.FormatTime(r.ClassifiedTime(Clock))+(r.Estimated?" Estimated":"") : "racing")}  (+{r.Progress.PenaltySeconds:0.0}s / {r.Progress.MissedGates} misses)"));
    }
}
