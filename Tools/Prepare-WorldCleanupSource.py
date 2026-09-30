from pathlib import Path
root=Path(__file__).resolve().parents[1]
def replace(path,old,new):
 p=root/path;s=p.read_text(encoding='utf-8-sig');assert old in s,path;p.write_text(s.replace(old,new),encoding='utf-8')
replace('Assets/Scripts/RaceDirector.cs','public string courseName="Street Loop";','public string courseName="Street Loop - Forward";')
replace('Assets/Scripts/RaceMenus.cs','banner.text="Finished — AI are still racing. Wait or simulate their remaining times.";', '''banner.text="Finished — AI are still racing.\\n"+CompleteRacePrompt;
                simulateRemaining.GetComponentInChildren<UnityEngine.UI.Text>().text=CompleteRacePrompt;''')
replace('Assets/Scripts/RaceMenus.cs','        bool waitingShown;', '''        bool waitingShown;
        // Show both available bindings: no guessed controller letter or new input scheme.
        public string CompleteRacePrompt {
            get {
                string keys=submit.GetBindingDisplayString(0);
                string pad=Gamepad.current==null?null:submit.GetBindingDisplayString(1);
                return "Press "+(string.IsNullOrEmpty(pad)?keys:pad+" / "+keys)+" to Complete Race";
            }
        }''')
replace('Assets/Scripts/ExplorationMap.cs','Resources.Load<WorldMapVisual>("WorldMaps/"+gameObject.scene.name)','Resources.Load<WorldMapVisual>("WorldMaps/PermanentWorld")')
replace('Assets/Scripts/ExplorationMap.cs','Dim terrain: unexplored.\\nVisited is NOT fully searched.','All roads / trails are permanent.\\nDim terrain: unexplored.\\nVisited is NOT fully searched.')
# Preserve old home ID. Newly designated properties can inherit prior physical exploration,
# but never reveal a property in an unvisited cell.
replace('Assets/Scripts/ExplorationMap.cs','            } catch(Exception e){error=e.Message;}','''                foreach(var d in destinations)
                    if(d.id.StartsWith("property-",StringComparison.Ordinal)&&Visited(d.position)&&!data.landmarks.Contains(d.id))
                    { data.landmarks.Add(d.id); dirty=true; }
            } catch(Exception e){error=e.Message;}''')
# Reuse the bounded prior fixture; point its output and shared map checks at this release.
s=(root/'Assets/Scripts/WorldMapChecks.cs').read_text(encoding='utf-8-sig').replace('WorldMapChecks','WorldCleanupChecks').replace('Docs/WorldMap','Docs/WorldCleanup').replace('-worldMapCheck','-worldCleanupCheck').replace('Temp/WorldMapSave','Temp/WorldCleanupSave').replace('"WorldMaps/"+race.gameObject.scene.name','"WorldMaps/PermanentWorld"').replace('Full menu uses high-resolution saved-scene render','Full menu uses shared permanent world render').replace('button.GetComponentInChildren<UnityEngine.UI.Text>().text=="COMPLETE RACE"','button.GetComponentInChildren<UnityEngine.UI.Text>().text==menus.CompleteRacePrompt&&menus.CompleteRacePrompt.Contains("Space")').replace('Exact requested button label','Explicit Space Complete Race prompt from submit binding')
s=s.replace(' File.WriteAllText(Output+"/ui-done.txt"', '''
 // Four existing Western gullies collection triggers, with short continuous grounded samples.
 flow.StartFreeRoam();yield return null;race.traffic=false;
 var vehicle=race.vehicle;vehicle.enabled=false;vehicle.GetComponent<VehicleInput>().enabled=false;
 vehicle.Body.isKinematic=true;var collection=race.GetComponent<ExplorationCollection>();
 foreach(var site in collection.sites.Where(s=>s.approach=="Western gullies")){
     for(int step=0;step<=65;step++){
         var p=Vector3.Lerp(site.access,site.position,Mathf.Clamp01((step-2)/60f));
         var ground=Physics.RaycastAll(new Vector3(p.x,300,p.z),Vector3.down,600).Where(h=>h.collider.name.StartsWith("Ground_")).OrderBy(h=>h.distance).First();
         vehicle.Body.position=ground.point+Vector3.up;yield return new WaitForFixedUpdate();
     }
     Check(collection.Discovered(site.id),"Existing Western gullies swept trigger fires: "+site.id);
 }
 Check(map.destinations.Count(d=>new[]{"Moll's","Roger's","McFadden's","Anderson's"}.Contains(d.title))==4,"Four unique corrected property destinations");
 foreach(var d in map.destinations.Where(d=>new[]{"Moll's","Roger's","McFadden's","Anderson's"}.Contains(d.title))){map.Reveal(d.position);Check(map.Travel(Array.IndexOf(map.destinations,d)),"Supported free-roam arrival: "+d.title);}
 var fixtureRoot=Path.GetFullPath("Temp/WorldCleanupMigration");Directory.CreateDirectory(fixtureRoot);
 var data=new ExplorationMap.Data();data.landmarks.Add("home");var visitedProperty=map.destinations.Single(d=>d.id=="property-anderson");
 var cell=ExplorationMap.Normalized(visitedProperty.position);data.visited.Add(Mathf.FloorToInt(cell.y*ExplorationMap.Rows)*ExplorationMap.Columns+Mathf.FloorToInt(cell.x*ExplorationMap.Columns));
 File.WriteAllText(Path.Combine(fixtureRoot,"exploration-map-"+ExplorationMap.Compatibility+".json"),JsonUtility.ToJson(data));
 var fixture=new GameObject("Isolated discovery migration").AddComponent<ExplorationMap>();fixture.destinations=map.destinations;fixture.Initialize(race,fixtureRoot);
 Check(fixture.Discovered("home")&&fixture.Discovered("property-anderson"),"Existing home ID and previously visited Anderson property retained");
 Check(!fixture.Discovered("property-mcfadden"),"Unvisited property remains undiscovered");Destroy(fixture.gameObject);
 var pad=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.XInput.XInputController>();yield return null;
 Check(menus.CompleteRacePrompt.Contains("A")&&menus.CompleteRacePrompt.Contains("Space"),"Controller A and keyboard Space labels resolve from existing submit action");
 UnityEngine.InputSystem.InputSystem.RemoveDevice(pad);
 File.WriteAllText(Output+"/ui-done.txt"''')
(root/'Assets/Scripts/WorldCleanupChecks.cs').write_text(s,encoding='utf-8')
print('Targeted runtime UI changes prepared.')
