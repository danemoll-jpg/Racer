using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Racer
{
    public sealed class RaceDirector : MonoBehaviour
    {
        public ArcadeVehicle vehicle;
        public RaceGate[] gates;
        [Min(1)]
        public int laps = 3;
        public RaceRoad road;
        [Min(0)]
        public float ordinaryPenalty = 5;
        [Min(1)]
        public float cutPenaltyMetresPerSecond = 10;
        public bool opponents = true, traffic = true;
        public int difficulty = 1;
        public string[] opponentRoster = {"tourer","moto","atv"};
        public string RosterLabel => string.Join(" / ",opponentRoster.Select(id=>VehicleProfile.Find(id).Name));
        public string DifficultyName => new[]{"Easy", "Normal", "Hard"}[Mathf.Clamp(difficulty,0,2)];
        public string ModeLabel => opponents ? "Race vs 3 AI / " + DifficultyName : "Solo / time trial";
        [Range(0, 6)]
        public int trafficCount = 4;
        public float finishGraceSeconds = 90, maximumRaceSeconds = 1200;
        public List<RacerState> Racers { get; } = new();
        public List<RoadDriver> Drivers { get; } = new();
        public RaceProgress Progress => Racers.Count > 0 ? Racers[0].Progress : null;
        public double Clock { get; private set; }

        public RaceFlow Flow { get; private set; }

        public bool ClassificationFinal { get; private set; }

        public int PlayerPosition => Ordered(false).IndexOf(Racers[0]) + 1;
        public string Category => $"street-v5-woodland-{(vehicle.GetComponent<VehicleConfiguration>() ? vehicle.GetComponent<VehicleConfiguration>().profileId : "original")}-{(opponents ? "race4-d" + difficulty+"-"+string.Join("-",opponentRoster) : "solo")}-{(traffic ? "traffic" : "clear")}-laps{laps}";
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
            // Recovery is neither a gate crossing nor a new lap; retain all earned progress.
            Racers[0].SampleOrigin(Clock);
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
            Progress.Restart();
            Racers[0].Dnf=false; Racers[0].FinishArmed=false; Racers[0].RecoveryStart=float.NaN; Racers[0].Recoveries=0;
            if (road && opponents)
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
                r.Branch.Clear();
            }

            firstFinish = -1;
            ClassificationFinal = false;
            startedAt = Time.timeAsDouble + 3;
            BreakableProp.RestoreRace();
            SmashAudio.Prepare();
            ResetSampling(vehicle.Body.position, Time.timeAsDouble);
            if (Flow)
            {
                Flow.SelectRecords(Category);
                Flow.BeginCountdown();
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
            float spawn = road.Project(vehicle.transform.position, out _);
            int population = Mathf.Clamp(trafficCount, 0, 6);
            Color[] colors = {new(.95f, .25f, .12f), new(.95f, .75f, .1f), new(.2f, .55f, 1)};
            for (int i = 0; i < (opponents ? 3 : 0) + (traffic ? population : 0); i++)
            {
                bool racing = opponents && i < 3;
                int n = racing ? i : i - (opponents ? 3 : 0);
                var clone = Instantiate(vehicle.gameObject);
                clone.name = racing ? new[]{"EMBER", "GOLD", "BLUE"}[n] : "Traffic " + (n + 1);
                var car = clone.GetComponent<ArcadeVehicle>();
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

                car.enabled = false;
                car.Body.isKinematic = false;
                var driver = clone.AddComponent<RoadDriver>();
                driver.Initialize(this, car, racing, racing ? 1 : (n % 2 == 0 ? 1 : -1), .94f + n * .025f);
                Drivers.Add(driver);
                if (racing)
                {
                    var state = new RacerState(clone.name, car, gates.Length - 1, laps);
                    Racers.Add(state);
                    driver.Racer = state;
                }

                driver.Place(racing ? spawn + 8 + n * 7 : spawn + 200 + n * road.Length / Mathf.Max(1, population), racing ? (n % 2 == 0 ? 2.2f : -2.2f) : 2.6f * driver.Direction);
            }
        }

        void ShowGrid()
        {
            if(gridVisual) { gridVisual.SetActive(false); Destroy(gridVisual); }
            if(!opponents || !road) return;
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
            Sample(vehicle.Body.position, vehicle.transform.forward, Clock);
            for (int i = 1; i < Racers.Count; i++)
                SampleRacer(Racers[i], Racers[i].Car.Body.position, Racers[i].Car.transform.forward, Clock, false);
            if (Racers.Any(r => r.Progress.Finished) && firstFinish < 0)
                firstFinish = Clock;
            if (Clock - startedAt >= maximumRaceSeconds || (firstFinish >= 0 && Clock - firstFinish >= finishGraceSeconds))
                foreach (var r in Racers)
                    if (!r.Progress.Finished)
                        r.Dnf = true;
            if (!ClassificationFinal && Racers.All(r => r.Progress.Finished || r.Dnf))
            {
                ClassificationFinal = true;
                Flow?.CompleteResults();
            }
        }

        public void Sample(Vector3 position, Vector3 heading, double now) => SampleRacer(Racers[0], position, heading, now, true);
        void SampleRacer(RacerState r, Vector3 position, Vector3 heading, double now, bool player)
        {
            if ((Flow && Flow.State != RaceFlow.Stage.Racing) || r.Progress.Finished || r.Dnf)
                return;
            var p = r.Progress;
            int completed = p.CompletedLaps, misses = p.MissedGates;
            double oldPenalty = p.PenaltySeconds;
            bool credited = false;
            float step = Vector3.Distance(position, r.Previous);
            if (step > 10)
            {
                p.ResetToGrid();
                r.FinishArmed = false;
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
                        if (expected <= 0 || p.NextGate != expected || !branch.Enter(r.Previous,position,heading)) continue;
                        r.Branch.Begin(branch); break;
                    }
                if (r.Branch.Route)
                {
                    var branch = r.Branch.Route;
                    bool exited = r.Branch.Advance(r.Previous,position,heading);
                    branch.Project(position,out float branchLateral);
                    // A partial main-road rejoin or reversing out of the entrance abandons this attempt.
                    // No gate credit has been issued, so ordinary road rules resume exactly once.
                    bool abandoned = !exited && ((lateral < 9 && branchLateral > branch.halfWidth+4)
                        || (branch.Project(position,out _) < 2 && Vector3.Dot(position-branch.points[0],branch.points[1]-branch.points[0]) < -1));
                    if(exited)
                    {
                        foreach(int gate in branch.bypassedGates)
                            if(p.NextGate==gate) p.Cross(gate,true,now);
                        r.RoadPosition=road.Relative(branch.exitRoad,origin);
                        int last=p.NextGate==0?gates.Length-1:p.NextGate-1;
                        r.Travel=Mathf.Max(0,r.RoadPosition-gateS[last]);
                        r.VerifiedRoad=r.RoadPosition;
                        r.Branch.Clear();
                    }
                    else if(abandoned) r.Branch.Clear();
                    else
                    {
                        // Shared entrance pavement may contain a real gate. Main-road drivers
                        // must not lose that physical crossing while a branch is provisionally active.
                        foreach(int gate in branch.bypassedGates)
                            if(p.NextGate==gate && gates[gate].TryCross(r.Previous,position,out bool gateForward,out float crossing)
                                && gateForward && Vector3.Dot(heading,gates[gate].transform.forward)>.25f)
                                p.Cross(gate,true,r.PreviousTime+(now-r.PreviousTime)*crossing);
                        r.RoadPosition=road.Relative(r.Branch.RoadPosition,origin);
                        if(r.RoadPosition>road.Length*.35f && r.RoadPosition<road.Length*.8f) r.FinishArmed=true;
                        r.Previous=position; r.PreviousTime=now;
                        if(player) Clock=now;
                        return;
                    }
                }
            }
            // Only new forward road distance counts against the cut charge. Driving circles,
            // reversing or accumulating an off-road odometer cannot buy away a skipped sector.
            if (road && p.LapActive)
            {
                if (lateral < 18) r.Travel += Mathf.Min(step, Mathf.Max(0, r.RoadPosition - r.VerifiedRoad));
                r.VerifiedRoad = Mathf.Max(r.VerifiedRoad, r.RoadPosition);
            }
            if (road && p.LapActive && r.RoadPosition > road.Length * .35f && r.RoadPosition < road.Length * .8f)
                r.FinishArmed = true;
            for (int i = 0; i < gates.Length; i++)
            {
                if (!gates[i].TryCross(r.Previous, position, out bool forward, out float fraction))
                    continue;
                forward &= Vector3.Dot(heading, gates[i].transform.forward) > .25f;
                if (!forward)
                    continue;
                if (i == 0 && p.LapActive && !r.FinishArmed)
                    continue;
                if (p.LapActive && p.NextGate > 0 && (i == 0 || i > p.NextGate))
                    ResolveMisses(r, i == 0 ? gates.Length : i);
                int expected = p.NextGate;
                p.Cross(i, true, r.PreviousTime + (now - r.PreviousTime) * fraction);
                if (i > 0 && i == expected && p.NextGate != expected)
                {
                    credited = true;
                    r.Travel = 0;
                }

                if (i == 0)
                {
                    r.Travel = 0;
                    r.FinishArmed = false;
                    r.VerifiedRoad = 0;
                }
            }

            if (road && p.LapActive && p.NextGate > 0 && lateral < 18 && step > .005f)
            {
                road.At(origin + r.RoadPosition, out var direction);
                if (Vector3.Dot(position - r.Previous, direction) > .001f && Vector3.Dot(heading, direction) > .25f)
                    while (p.NextGate > 0 && r.RoadPosition > gateS[p.NextGate] + 18 && r.RoadPosition < road.Length - 20)
                        ResolveMisses(r, p.NextGate + 1);
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

        void ResolveMisses(RacerState r, int until)
        {
            var p = r.Progress;
            while (p.NextGate > 0 && p.NextGate < until)
            {
                int gate = p.NextGate;
                float sector = road ? gateS[gate] - gateS[gate - 1] : 230;
                double penalty = ordinaryPenalty + Mathf.Max(0, sector - r.Travel - 25) / cutPenaltyMetresPerSecond;
                if (!p.Miss(gate, penalty))
                    break;
                r.Travel = Mathf.Max(0, r.Travel - sector);
            }
        }

        public List<RacerState> Ordered(bool final) => final ? Racers.OrderBy(r => !r.Progress.Finished).ThenBy(r => r.Progress.Finished ? r.Progress.AdjustedTime(Clock) : -Score(r)).ToList() : Racers.OrderByDescending(Score).ThenBy(r => r.Progress.Finished ? r.Progress.RaceTime(Clock) : 0).ToList();
        float Score(RacerState r) => r.Progress.Finished ? laps * (road ? road.Length : 4000) : r.Progress.CompletedLaps * (road ? road.Length : 4000) + (r.Progress.LapActive ? r.RoadPosition : -1);
        public string Standings() => string.Join("\n", Ordered(true).Select((r, i) => $"{i + 1}. {r.Name}  {(r.Dnf ? "DNF" : r.Progress.Finished ? RaceHud.FormatTime(r.Progress.AdjustedTime(Clock)) : "racing")}  (+{r.Progress.PenaltySeconds:0.0}s / {r.Progress.MissedGates} misses)"));
    }
}
