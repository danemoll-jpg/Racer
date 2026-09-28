if(Application.isPlaying)throw new Exception("Edit mode required");
var original=UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;var oldMode=Physics.simulationMode;float oldAudio=AudioListener.volume;
var rows=new List<string>();
try {
 UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/ForestLoopReverse.unity");AudioListener.volume=0;
 var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
 var race=UnityEngine.Object.FindAnyObjectByType<Racer.RaceDirector>();var car=race.vehicle;
 typeof(Racer.ArcadeVehicle).GetMethod("Awake",flags).Invoke(car,null);
 typeof(Racer.VehicleRespawn).GetMethod("Awake",flags).Invoke(car.GetComponent<Racer.VehicleRespawn>(),null);
 typeof(Racer.RaceDirector).GetMethod("Awake",flags).Invoke(race,null);
 foreach(var body in UnityEngine.Object.FindObjectsByType<Rigidbody>())if(body!=car.Body)body.gameObject.SetActive(false);
 typeof(Racer.RaceFlow).GetProperty("State").SetValue(race.Flow,Racer.RaceFlow.Stage.Racing);
 var state=new Racer.RacerState("fixture AI",car,race.gates.Length-1,3,true);race.Racers.Clear();race.Racers.Add(state);
 var route=race.Branches.Single(b=>b.title=="House 3 Detour");var driver=car.gameObject.AddComponent<Racer.RoadDriver>();driver.Initialize(race,car,true,1,1);driver.Racer=state;race.Drivers.Add(driver);driver.Shortcuts.Initialize(17038);
 state.Progress.Cross(0,true,0);
 int expected=Array.FindIndex(race.gates,g=>race.road.Relative(race.road.Project(g.transform.position,out _),race.Origin)>race.road.Relative(route.entryRoad,race.Origin));
 while(state.Progress.NextGate!=expected)state.Progress.Cross(state.Progress.NextGate,true,1);
 driver.Place(route.entryRoad-40,0);state.SampleOrigin(0);car.Body.isKinematic=false;
 var tick=typeof(Racer.RoadDriver).GetMethod("FixedUpdate",flags);var sample=typeof(Racer.RaceDirector).GetMethod("SampleRacer",flags);
 Physics.simulationMode=SimulationMode.Script;Racer.DriverVariation.Disabled=true;
 bool entered=false;float furthest=0;int switches=0;object previous=null;var planned=typeof(Racer.RoadDriver).GetField("plannedBranch",flags);
 for(int frame=0;frame<1800;frame++) {
  tick.Invoke(driver,null);Physics.Simulate(.02f);Physics.SyncTransforms();
  sample.Invoke(race,new object[]{state,car.Body.position,car.transform.forward,frame*.02,false});
  var choice=planned.GetValue(driver);if(choice!=previous){switches++;previous=choice;}
  if(state.Branch.Route==route){entered=true;furthest=Math.Max(furthest,state.Branch.Position);}
  if(furthest>route.entryInset+75)break;
 }
 rows.Add($"House 3 actual fixed-step physics approach: entered={entered}; branchProgress={furthest:F2}m; decisionCount={driver.Shortcuts.Decisions}; commitmentChanges={switches}; recoveries={driver.RecoveryCount}; misses={state.Progress.MissedGates}; choice={driver.Shortcuts.Diagnostic}");
 System.IO.File.WriteAllLines("Docs/ForgivingStrategy/ai-driving.txt",rows);
 if(!entered||furthest<route.entryInset+75||switches!=1||driver.Shortcuts.Decisions!=1||driver.RecoveryCount!=0||state.Progress.MissedGates!=0)throw new Exception(rows[0]);
 return rows[0];
} finally {Physics.simulationMode=oldMode;AudioListener.volume=oldAudio;Racer.DriverVariation.Disabled=false;UnityEditor.SceneManagement.EditorSceneManager.OpenScene(original);}
