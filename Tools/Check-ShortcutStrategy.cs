if(Application.isPlaying)throw new Exception("Edit mode required");
var original=UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
var rows=new List<string>();
void Check(bool ok,string text){if(!ok)throw new Exception(text);}
try {
 UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/ForestLoopReverse.unity");
 var race=UnityEngine.Object.FindAnyObjectByType<Racer.RaceDirector>();
 var routes=UnityEngine.Object.FindObjectsByType<Racer.WoodlandRoute>();var route=routes.Single(b=>b.title=="House 3 Detour");
 var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
 typeof(Racer.ArcadeVehicle).GetMethod("Awake",flags).Invoke(race.vehicle,null);
 typeof(Racer.RaceDirector).GetMethod("Awake",flags).Invoke(race,null);
 int[] counts=new int[3];var choices=new List<string>();
 for(int driver=0;driver<3;driver++) {
  var strategy=new Racer.ShortcutStrategy();strategy.Initialize(1200+driver*7919);
  var neutral=new Racer.ShortcutStrategy();neutral.Initialize(1200+driver*7919);
  var trailing=new Racer.ShortcutStrategy();trailing.Initialize(1200+driver*7919);
  string pattern="";
  for(int lap=0;lap<24;lap++) {
   Check(strategy.TryDecide(route,lap,0,0,out bool lead),"leader opportunity lost");
   neutral.TryDecide(route,lap,.5f,0,out bool mid);trailing.TryDecide(route,lap,1,250,out bool back);
   if(lead)counts[0]++;if(mid)counts[1]++;if(back)counts[2]++;
   pattern+=mid?"S":"M";
   Check(!neutral.TryDecide(route,lap,.5f,0,out _),"decision rerolled");
  }
  choices.Add(pattern);rows.Add($"driver {driver}: risk={strategy.Risk:F3}; neutral={pattern}");
 }
 Check(counts[0]>0&&counts[0]<counts[1]&&counts[1]<36&&counts[2]>counts[1]&&counts[2]<72,"probability sample did not meet acceptance");
 Check(choices.Distinct().Count()==3,"synchronized driver patterns");
 rows.Add($"72 opportunities per situation: leading={counts[0]}, neutral={counts[1]}, far-behind={counts[2]}; no guaranteed/forbidden choice; neutral main favored.");
 // Production entry selection: independent drivers at the same valid approach.
 var selected=new List<string>();
 for(int i=0;i<3;i++) {
  var go=UnityEngine.Object.Instantiate(race.vehicle.gameObject);var car=go.GetComponent<Racer.ArcadeVehicle>();
  typeof(Racer.ArcadeVehicle).GetMethod("Awake",flags).Invoke(car,null);
  var driver=go.AddComponent<Racer.RoadDriver>();driver.Initialize(race,car,true,1,1);driver.Shortcuts.Initialize(1200+i*7919);
  var state=new Racer.RacerState("driver"+i,car,race.gates.Length-1,3,true);driver.Racer=state;race.Racers.Add(state);
  state.Progress.Cross(0,true,0);state.RoadPosition=route.entryRoad-40;
  int expected=Array.FindIndex(race.gates,g=>race.road.Relative(race.road.Project(g.transform.position,out _),race.Origin)>race.road.Relative(route.entryRoad,race.Origin));
  while(state.Progress.NextGate!=expected)state.Progress.Cross(state.Progress.NextGate,true,1);
  var decide=typeof(Racer.RoadDriver).GetMethod("DecideShortcut",flags);var planned=typeof(Racer.RoadDriver).GetField("plannedBranch",flags);
  decide.Invoke(driver,new object[]{route.entryRoad-40});var committed=planned.GetValue(driver);
  for(int j=0;j<20;j++)decide.Invoke(driver,new object[]{route.entryRoad-40+j});
  Check(driver.Shortcuts.Decisions==1&&planned.GetValue(driver)==committed,"production choice not latched");
  selected.Add(committed==null?"MAIN":"SHORTCUT");rows.Add(driver.Shortcuts.Diagnostic);
  UnityEngine.Object.DestroyImmediate(go);race.Racers.Remove(state);
 }
 Check(selected.Distinct().Count()>1,"representative production drivers did not split");
 rows.Add("Production same opportunity choices: "+string.Join(", ",selected)+"; each decision latched across 20 approach updates.");
 System.IO.File.WriteAllLines("Docs/ForgivingStrategy/ai-checks.txt",rows);return string.Join("\n",rows);
} catch(Exception e) { System.IO.File.WriteAllText("Docs/ForgivingStrategy/ai-error.txt",e.ToString()+"\n"+string.Join("\n",rows));throw;} finally {UnityEditor.SceneManagement.EditorSceneManager.OpenScene(original);}

