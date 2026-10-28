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
        public bool Forest => forestOverride>=0 ? forestOverride==1 : (road&&road.forestTrail)||courseId=="lake-v2-forest" || courseId=="lake-v3-shallows";
        // 0.76 FreeRoamWorld: the selected course's vehicle rule (1 small vehicles only, 0 all); -1 = this scene's own course.
        [System.NonSerialized] public int forestOverride=-1;
        // 0.80: cars are allowed on every course unless CarAccess still restricts it (Forest keeps its other meanings).
        // FreeRoamWorld uses the selected course's scene.
        [System.NonSerialized] public string carRuleScene;
        public bool CarsRestricted => !CarAccess.CourseAllowsCars(string.IsNullOrEmpty(carRuleScene)?gameObject.scene.name:carRuleScene);
        // 0.88: a reward vehicle not yet earned (VehicleUnlocks) is not eligible for the player or the AI.
        // 0.106 Part C: the stunt vehicles only in Free Roam (FreeRoamWorld): never in a race, the campaign or for the AI
        public VehicleProfile[] EligibleVehicles => stunts?StuntRace.Vehicles:VehicleProfile.All.Where(p=>(!CarsRestricted||p.Small)&&!VehicleUnlocks.Locked(p)&&(!p.Stunt||StuntHere)).ToArray();
        public bool StuntHere=>gameObject.scene.name==RaceFlow.RoamScene||stunts;
        public string EligibleVehicle(string id)=>stunts?StuntRace.Eligible(id):VehicleUnlocks.Locked(id)||VehicleProfile.Find(id).Stunt&&!StuntHere?(CarsRestricted?"moto":"original"):CarsRestricted&&!VehicleProfile.Find(id).Small?"moto":VehicleProfile.Find(id).Id;
        // 0.119 Part B: a Stunt Race (StuntRace): stunt vehicles only for the player and every rival, air steering, the stunt park's landing rules,
        // tricks scored (a trick bonus off the race time, or Split score); its own records (RecordCourse). Set by RaceFlow (Race > Stunt Race),
        // CampaignRun (the Stunt chapter) or SplitScreen; false in every other race.
        [System.NonSerialized] public bool stunts, splitScore;
        public string RecordCourse=>(high?HighMode.RecordPrefix:"")+BaseRecordCourse;
        public string BaseRecordCourse=>stunts?StuntRace.RecordPrefix+courseId+(StuntShortcuts.Closed(this)?StuntShortcuts.RecordSuffix:"")+(splitScore?"-split":""):courseId; // 1.3.0-review3: the Mountain Loop Stunt Races with their shortcuts closed start new boards
        // 1.2: this race or Free Roam session is in HIGH MODE (set by RaceFlow at each start; a race keeps it until the next start): its own
        // records ("high-" boards), the gutter bumpers (HighBumpers) and the forgiving driving (HighAssist).
        // 1.2.0-review2: in split screen each player chooses: high1 / high2 (high = either of them: the bumpers are built, for the HIGH player only)
        [System.NonSerialized] public bool high, high1, high2;
        public bool HighFor(int player)=>SplitScreen.Active?(player==1?high1:high2):high;
        // 0.89 campaign: the player may choose only the vehicles the campaign has given them (plus the earned mower); the AI
        // keeps every eligible vehicle. Testing ("Unlock everything") lifts the campaign lock.
        public VehicleProfile[] PlayerVehicles => EligibleVehicles.Where(p=>!Campaign.VehicleLocked(p)).ToArray();
        public string PlayerVehicle(string id){var e=EligibleVehicle(id);if(!Campaign.VehicleLocked(VehicleProfile.Find(e)))return e;var first=PlayerVehicles.FirstOrDefault();return first!=null?first.Id:e;}
        public string courseId="street-v8-landings";
        public string courseName="Street Loop - Forward";
        public const double OrdinaryMissPenalty = 5;
        public bool opponents = true, traffic = true;
        public bool FreeRoam {get;set;}
        public int difficulty = 1;
        public string[] opponentRoster = {"tourer","moto","atv"};
        public string RosterLabel => string.Join(" / ",opponentRoster.Select(id=>VehicleProfile.Find(id).Name));
        public string DifficultyName => new[]{"Easy", "Normal", "Hard"}[Mathf.Clamp(difficulty,0,2)];
        public string ModeLabel => opponents ? $"Race vs {RivalCount} AI / " + DifficultyName : "Solo / time trial";
        // 0.89: the number of rivals is the roster's length (3 normally; a campaign event may set another field size).
        public int RivalCount => opponents ? opponentRoster.Length : 0;
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
        public string Category => $"{RecordCourse}-{(vehicle.GetComponent<VehicleConfiguration>() ? vehicle.GetComponent<VehicleConfiguration>().profileId : "original")}-{(opponents ? "race4-d" + difficulty+"-"+string.Join("-",opponentRoster) : "solo")}-{(traffic ? "traffic" : "clear")}{(FreeRoam ? "" : WeatherGrip.RecordTag(WeatherGrip.ChosenFor(this)))}-laps{laps}";
        VehicleRespawn respawn;
        float origin;
        float[] gateS;
        public WoodlandRoute[] Branches { get; private set; }
        // 1.3.0-review3: every shortcut of the course, and a Mountain Loop Stunt Race's closing of them (StuntShortcuts: Branches empty)
        public WoodlandRoute[] AllBranches { get; private set; }
        public void CloseBranches(bool closed) { if (AllBranches != null) Branches = closed ? System.Array.Empty<WoodlandRoute>() : AllBranches; }
        // 1.3.0-review6: some of them closed (a Leviathan Stunt Race: only the ones on the mountain)
        public void CloseBranches(System.Collections.Generic.ICollection<WoodlandRoute> closed) { if (AllBranches != null) Branches = System.Array.FindAll(AllBranches, b => !closed.Contains(b)); }
        public float Origin => origin;
        double startedAt, firstFinish = -1; bool winnerCelebrated;
        double debugTimeoutOffset, debugGraceOffset;
        GameObject gridVisual;
        // 0.113 Part B: rivals and traffic are copies of the player's vehicle. A Retry (RestartRace in the same scene) used to copy it as it was
        // after the race (whatever the race and the finish left on it); now every start copies the same clean copy, kept (inactive) from
        // the scene's first start, so a Retry builds the same field as a fresh start from the menu.
        GameObject cleanVehicle;
        public static bool LiveCopies; // checks only (Report106PlayerCheck retry113 -p113live: the 0.112 way, for the before); never set in play
        GameObject CleanVehicle()
        {
            if(LiveCopies)return vehicle.gameObject;
            if(cleanVehicle)return cleanVehicle;
            var holder=new GameObject("Clean vehicle for rivals (inactive)");holder.SetActive(false);UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(holder,gameObject.scene);
            cleanVehicle=Instantiate(vehicle.gameObject,holder.transform);cleanVehicle.name=vehicle.gameObject.name;
            return cleanVehicle;
        }
        static Material gridPaint;
        // 0.115: a Leviathan course replaces this scene's own (LeviathanCourse); null racing anything else
        public LeviathanSwap Leviathan { get; private set; }
        void Awake()
        {
            LeviathanCourse.Apply(this); Leviathan = GetComponent<LeviathanSwap>();
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
                Branches = AllBranches = Leviathan ? Leviathan.Branches : FindObjectsByType<WoodlandRoute>();
                origin = road.Project(gates[0].transform.position, out _);
                // 0.115: on a line that passes the same ground twice (Leviathan's stem and lake shore), each gate at its own pass
                gateS = gates.Select((g, i) => road.Relative(Leviathan ? road.ProjectNear(g.transform.position, Leviathan.GateHints[i], 30, out _) : road.Project(g.transform.position, out _), origin)).ToArray();
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
            if(CarsRestricted)
            {
                var configuration=vehicle.GetComponent<VehicleConfiguration>();
                if(!configuration.Profile.Small)configuration.Apply(EligibleVehicle(configuration.profileId));
                for(int i=0;i<opponentRoster.Length;i++)opponentRoster[i]=EligibleVehicle(opponentRoster[i]);
            }
            if (Flow)
                Flow.PrepareRestart();
            SplitScreen.Race?.ClearPlayerTwo(); // 0.90 Part D
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
            Racers[0].Name = SplitScreen.Active ? SplitScreen.NameOf(1) : PlayerNames.Player; // 0.94 Part A
            Racers[0].Dnf=false; Racers[0].FinishArmed=false; Racers[0].RecoveryStart=float.NaN; Racers[0].Recoveries=0;
            if (road && opponents && !FreeRoam)
            {
                var backyard=GetComponent<BackyardForwardCourse>();
                var grid = road.At(backyard?backyard.GridStation(this,0):origin - 32 - 7*Mathf.Max(0,RivalCount-3), out var direction);
                vehicle.Body.position = grid + Vector3.Cross(Vector3.up,direction).normalized * (backyard?backyard.GridSide(0):-2.2f) + Vector3.up * Mathf.Max(.4f,vehicle.suspensionLength-Physics.gravity.magnitude/vehicle.springStrength);
                vehicle.Body.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(direction,Vector3.up));
                vehicle.transform.SetPositionAndRotation(vehicle.Body.position,vehicle.Body.rotation);
                FindAnyObjectByType<ChaseCamera>()?.Snap();
            }
            // 0.89 campaign: a speed trap or flying-lap event starts on the road a run-up before the trap / the start line.
            if (road && !FreeRoam && CampaignRun.StartStation(this, out float station))
            {
                var at = road.At(station, out var heading);
                vehicle.Body.position = at + Vector3.up * Mathf.Max(.4f,vehicle.suspensionLength-Physics.gravity.magnitude/vehicle.springStrength);
                vehicle.Body.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(heading,Vector3.up));
                vehicle.transform.SetPositionAndRotation(vehicle.Body.position,vehicle.Body.rotation);
                respawn.SeedCoursePosition(vehicle.Body.position);
                FindAnyObjectByType<ChaseCamera>()?.Snap();
            }
            if (road)
                CreateCars();
            if (SplitScreen.Active) SplitScreen.Race?.PlayerTwo(); // 0.90 Part D: the rival slot becomes player 2
            StuntRaceScore.Prepare(this); // 0.119 Part B: a Stunt Race's tricks scored (removed in any other race)
            StuntBumps.Apply(this); // 0.120 Part C: a Stunt Race's bumps (removed in any other race)
            StuntShortcuts.Apply(this); // 1.3.0-review3: a Mountain Loop Stunt Race's shortcuts closed (open in any other race)
            HighBumpers.Apply(this); HighAssist.Attach(this); // 1.2: HIGH MODE races: the gutter bumpers (hidden otherwise); the players' forgiving driving
            ShowGrid();
            foreach (var r in Racers)
            {
                r.SampleOrigin(Time.timeAsDouble);
                r.Travel = 0;
                r.VerifiedRoad = 0;
                r.FinishApproach = 0;
                r.Branch.Clear();
            }

            firstFinish = -1; winnerCelebrated = false; MountainMissedGate = -1;
            debugTimeoutOffset=debugGraceOffset=0;
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
                BalanceLog.Begin(Flow); // 0.109 Part C: an attempt opens (races, campaign events, the split-screen events)
            }
        }

        public void AbandonEvent()
        {
            SplitScreen.Race?.ClearPlayerTwo();
            foreach(var driver in Drivers) if(driver) { driver.gameObject.SetActive(false); Destroy(driver.gameObject); }
            Drivers.Clear(); Racers.RemoveRange(1,Racers.Count-1);
            if(gridVisual) { gridVisual.SetActive(false); Destroy(gridVisual); }
            Progress.Restart(); Racers[0].Branch.Clear(); Racers[0].Dnf=false; Racers[0].FinishArmed=false;
            Clock=0; firstFinish=-1; winnerCelebrated=false; ClassificationFinal=false;
        }

        void CreateCars()
        {
            bool savedOpponents=opponents;if(FreeRoam)opponents=false;
            float spawn = road.Project(vehicle.transform.position, out _);
            int localPopulation = Mathf.Clamp(trafficCount, 0, 6);
            int population = localPopulation + Mathf.Clamp(highwayTrafficCount,0,24);
            Color[] colors = {new(.95f, .25f, .12f), new(.95f, .75f, .1f), new(.2f, .55f, 1), new(.2f, .78f, .38f), new(.66f, .36f, .9f)};
            // 0.75: each AI gets a random rider for this race (varied across the field, never the player's exact look).
            int rivals = opponents ? opponentRoster.Length : 0;
            var riders = RiderLook.Field(DriverVariation.Seed, Mathf.Max(3,rivals), RiderLook.Player);
            for (int i = 0; i < rivals + (traffic ? population : 0); i++)
            {
                bool racing = opponents && i < rivals;
                int n = racing ? i : i - rivals;
                var clone = Instantiate(CleanVehicle(), vehicle.transform.position, vehicle.transform.rotation);
                clone.name = racing ? new[]{"EMBER", "GOLD", "BLUE", "JADE", "VIOLET"}[n % 5] : "Traffic " + (n + 1);
                var car = clone.GetComponent<ArcadeVehicle>();
                foreach(var activityContact in clone.GetComponents<ActivityLandingContact>())Destroy(activityContact);
                var configuration = clone.GetComponent<VehicleConfiguration>();
                if (!configuration) configuration = clone.AddComponent<VehicleConfiguration>();
                // Every opponent uses the resolved real physics/visual profile. Ambient traffic keeps the classic car.
                configuration.riderLook = racing ? riders[n] : null; configuration.classicVisual = !racing;
                configuration.Apply(racing ? opponentRoster[n] : "original");
                // 0.94 Part A: a campaign rival wears their cast colour
                int member = racing ? CampaignRun.RivalMember(n) : -1; var paint = racing ? colors[(member >= 0 ? member : n) % colors.Length] : new Color(.55f,.55f,.5f);
                configuration.SetPaint(paint);
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
                        block.SetColor("_BaseColor", paint);
                        renderer.SetPropertyBlock(block);
                    }

                if(!racing)clone.AddComponent<AmbientVehicle>().Initialize();
                if(racing&&stunts)clone.AddComponent<RivalTricks>().Initialize(this,n); // 0.119 Part B: the rivals do tricks at the big jumps
                car.enabled = false;
                car.Body.isKinematic = false;
                var driver = clone.AddComponent<RoadDriver>();
                driver.Initialize(this, car, racing, racing ? 1 : (n % 2 == 0 ? 1 : -1), .94f + n * .025f);
                if(!racing) driver.Initialize(this,car,false,n%2==0?1:-1,.92f+(n%4)*.025f);
                driver.HighwayTraffic=!racing && n>=localPopulation;
                Drivers.Add(driver);
                if (racing)
                {
                    var state = new RacerState(CampaignRun.RivalName(n)??(RacePlaylists.Active!=null?"Rival "+Racers.Count:clone.name), car, gates.Length - 1, laps, true);
                    Racers.Add(state);
                    driver.Racer = state;
                    clone.GetComponent<RivalTricks>()?.Bind(state);
                }

                int h=n-localPopulation;
                var driveRoad=driver.DriveRoad;
                float station=driver.HighwayTraffic?(throughRoad?Mathf.Lerp(100,driveRoad.Length-100,(h+.5f)/Mathf.Max(1,highwayTrafficCount)):3800+(h/4)*175+(h%4)*22):(!racing&&ambientRoad?driveRoad.Project(vehicle.transform.position,out _):spawn)+200+n*driveRoad.Length/Mathf.Max(1,localPopulation);
                var backyard=GetComponent<BackyardForwardCourse>();
                driver.Place(racing ? (backyard?backyard.GridStation(this,n+1):spawn + 8 + n * 7) : station, racing ? (backyard?backyard.GridSide(n+1):(n % 2 == 0 ? 2.2f : -2.2f)) : driveRoad.TrafficLane(station,driver.Direction,n%4>=2));
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
            if(Progress.Finished && Flow && Flow.Save.Settings.estimateAiFinishes && !SplitScreen.Active) FinalizeUnfinishedAi();
            if (Racers.Any(r => r.Progress.Finished) && !winnerCelebrated)
            {
                winnerCelebrated = true;
                // 0.78: the winner (first across the line, player or AI) raises both fists; others do nothing special.
                // 0.92 Part F: in split-screen too
                if (Racers.Count > 1) Racers.Where(r => r.Progress.Finished).OrderBy(r => r.Progress.RaceTime(Clock)).First().Car.GetComponent<RiderGestures>()?.Celebrate();
            }
            // 0.92 Part F: in split-screen the 90 s to finish count from the first player home (an AI rival finishing first
            // does not start them); single-player unchanged
            if (Racers.Any(r => r.Progress.Finished && (!SplitScreen.Active || r == Racers[0] || (SplitScreen.Race && r == SplitScreen.Race.P2))) && firstFinish < 0)
                firstFinish = Clock;
            // Debug review time consumes neither the total race budget nor the post-finisher grace budget.
            if(DeveloperLocationHud.DebugEnabled){debugTimeoutOffset+=Time.fixedDeltaTime;if(firstFinish>=0)debugGraceOffset+=Time.fixedDeltaTime;}
            if (!DeveloperLocationHud.DebugEnabled && !Progress.Unlimited && (Clock - startedAt - debugTimeoutOffset >= maximumRaceSeconds*Mathf.Max(1,laps/3f) || (firstFinish >= 0 && Clock - firstFinish - debugGraceOffset >= finishGraceSeconds)))
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
                r.VerifiedRoad = 0; r.InFlight = false;
                r.Branch.Clear();
                r.SampleOrigin(now);
                return;
            } // Teleports do not create missed-gate chains.

            if (road && !float.IsNaN(r.RecoveryStart))
            {
                float advance = road.Relative(Station(r, position, out _), r.RecoveryStart);
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
                r.RoadPosition = road.Relative(Station(r, position, out lateral), origin);
            if (road && p.LapActive && p.LapValid && Branches != null)
            {
                if (!r.Branch.Route)
                    foreach (var branch in Branches)
                    {
                        int expected = System.Array.FindIndex(gateS, s => s > road.Relative(branch.entryRoad,origin));
                        if (expected <= 0 || p.NextGate < expected || p.NextGate > expected+1 || !branch.Enter(r.Previous,position,heading)) continue;
                        // 0.80: a car gets no credit on a shortcut closed to cars; it is off the route until it resets.
                        if (!CarAccess.Open(branch,r.Car)) continue;
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
                    branch.ProjectFor(r.Branch.Ai,position,out float branchLateral); // 0.113 Part A: a rival against its line
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
                // 0.121 (BUG-005): a flight's road progress counts once it lands back on the line. High over the line the station can move
                // more in one step than the rule above allows, and the frontier then stayed where that happened for good: a gate flown past
                // was never charged as missed, and on Leviathan the racer's station stayed pinned 60 m past it, so a rival turned back to it
                // again and again (rivals off the Hwy 92 kicker circling at Leviathan Forward CP 44). On landing it moves up to where the racer
                // is, never further than it flew from where it took off.
                if(r.Car.GroundedWheels==0){if(!r.InFlight){r.InFlight=true;r.FlightFrom=r.VerifiedRoad;r.FlightTravel=0;}r.FlightTravel+=step;}
                else if(r.InFlight&&r.Car.GroundedWheels>=2)
                {
                    r.InFlight=false;
                    if(lateral<18&&r.RoadPosition>r.VerifiedRoad&&r.RoadPosition<=r.FlightFrom+r.FlightTravel+12)r.VerifiedRoad=r.RoadPosition;
                }
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
                // 1.3.0-review5 Part D: on Leviathan, driving on through a gate already passed (not the last one) is a wrong turn onto another
                // pass of the course (the lake out-and-back's ends): TURN BACK at once, never that gate lit again
                if (player && Leviathan && p.LapActive && i > 0 && p.NextGate > 0 && i < p.NextGate - 1) GetComponent<LeviathanGuide>()?.Recrossed(r, i);
                if (p.LapActive && p.NextGate > 0 && (i == 0 || i > p.NextGate))
                {
                    // 0.115 Leviathan: the lake and mountain gates are not skipped by crossing a later one (turning past J would skip the
                    // mountain and its jump): the later gate does not count until they are passed
                    if (SkipsMandatory(p.NextGate, i == 0 ? gates.Length : i)) { if (player) { MountainMissedGate = p.NextGate; Trace(r, "mandatory gate skipped: later gate " + i + " not counted", position, now); } continue; }
                    ResolveMisses(r, i == 0 ? gates.Length : i, i==0?"finish reconciliation":"later gate crossed");
                }
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
            if (player && MountainMissedGate >= 0 && p.NextGate != MountainMissedGate) MountainMissedGate = -1; // 0.117: passed at last
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

        bool SkipsMandatory(int from, int until) { if (!Leviathan) return false; for (int g = from; g < until; g++) if (Leviathan.Mandatory(g)) return true; return false; }
        public float GateStation(int i) => gateS[i];
        // 0.115: the station of a racer on the line. A line that passes the same ground twice (Leviathan) is followed from where the racer
        // was (its recovery tracking, else the last station); any other course as before (the nearest point).
        float Station(RacerState r, Vector3 position, out float lateral)
        {
            if (!road.tracked) return road.Project(position, out lateral);
            var respawn = r.Car ? r.Car.GetComponent<VehicleRespawn>() : null;
            float hint = respawn && respawn.Tracking ? respawn.TrackingStation : r.TrackedStation;
            float s = float.IsNaN(hint) ? road.Project(position, out lateral) : road.ProjectNear(position, hint, 60, out lateral);
            // 0.117 (BUG-010-012): the racer is on the pass of the lap between its last gate and its next one. A station outside that stretch is
            // the other pass of a twice-driven road (Rocky Way Cut lands on one; the nearest point after a fall or a shortcut may be one): the
            // nearest point inside the stretch instead, so the next-CP guide, the course ahead and the wrong-way warning follow this pass
            if (CourseWindow(r, out float lo, out float hi) && (lateral > 30 || road.Relative(s, lo) > hi - lo))
                s = road.ProjectNear(position, lo + (hi - lo) * .5f, (hi - lo) * .5f, out lateral);
            else if (lateral > 30) s = road.Project(position, out lateral);
            r.TrackedStation = s; return s;
        }
        // 0.117: on a line that passes the same ground twice (Leviathan), the stretch of it a racer can be on: from 40 m before the last gate it
        // passed to 60 m past its next one (absolute stations; false before the lap starts or on any other course)
        public bool CourseWindow(RacerState r, out float lo, out float hi)
        {
            lo = hi = 0; if (!road || !road.tracked || gateS == null || r == null || !r.Progress.LapActive || r.Branch.Route) return false; // on a shortcut its bypassed gates are already credited: its own line rules
            int next = r.Progress.NextGate, last = next == 0 ? gates.Length - 1 : next - 1;
            lo = origin + gateS[last] - 40; hi = origin + (next == 0 ? road.Length + 30 : gateS[next] + 60); return true;
        }
        // 0.117 (BUG-006, Part D2 item 3): where a reset may put a racer on a tracked line: after the last gate it passed, never past its next one,
        // and not past the furthest point it has driven (absolute stations)
        public bool ResetSpan(RacerState r, out float lo, out float hi)
        {
            lo = hi = 0; if (!road || !road.tracked || gateS == null || r == null || !r.Progress.LapActive) return false;
            int next = r.Progress.NextGate, last = next == 0 ? gates.Length - 1 : next - 1;
            float a = last == 0 ? 0 : gateS[last], b = next == 0 ? road.Length : gateS[next], reached = r.VerifiedRoad + 5;
            // a jump taken (the furthest point driven is on its run-up or in its flight): up to its landing's end, so a crash on landing is not
            // put back before the jump (the ground under a flight is never driven, so the furthest point stops at its take-off)
            var flights = GetComponent<MountainFlights>();
            if (flights && flights.flights != null) foreach (var f in flights.flights) { float fa = road.Relative(f.approachStation, origin), fe = road.Relative(f.endStation, origin); if (r.VerifiedRoad >= fa && r.VerifiedRoad <= fe) reached = Mathf.Max(reached, fe); }
            lo = origin + a + 2; hi = origin + Mathf.Min(b - 3, Mathf.Max(a + 12, reached)); if (hi < lo + 1) hi = lo + 1; return true;
        }
        // 0.117 Part D2: the player crossed a later gate (or the finish) with Leviathan's lake / mountain gates still to pass: until they are, the
        // HUD says MOUNTAIN MISSED — GO BACK and points at the gate (its number: MountainMissedGate)
        public int MountainMissedGate { get; private set; } = -1;
        public Vector3 GatePosition(int i) => gates[i].transform.position;
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
                // 0.117 (BUG-007): on Leviathan the player is told which gate and where ("CP 4 (Hwy 92 corner): 120 m back")
                if(Leviathan&&r==Racers[0]&&Flow){var n=gates[gate].name;int k=n.IndexOf('(');Flow.MissedDetail=$"CP {gate}{(k>=0?" "+n.Substring(k):"")}: {DisplayUnits.Distance(Mathf.Max(0,r.RoadPosition-gateS[gate]))} back";}
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
        public float Score(RacerState r) => r.Progress.Finished ? laps * (road ? road.Length : 4000) : r.Progress.CompletedLaps * (road ? road.Length : 4000) + (r.Progress.LapActive ? r.RoadPosition : -1);
        public string Standings() => string.Join("\n", Ordered(true).Select((r, i) => $"{i + 1}. {r.Name}  {(r.Dnf ? "DNF" : r.Classified ? (r.Estimated?"~ ":"")+RaceHud.FormatTime(r.ClassifiedTime(Clock))+(r.Estimated?" Estimated":"") : "racing")}  (+{r.Progress.PenaltySeconds:0.0}s / {r.Progress.MissedGates} misses)"));
    }
}
