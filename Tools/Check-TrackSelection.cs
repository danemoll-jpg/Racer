// Bounded UI regression fixture; runs in the existing Editor with isolated muted saves.
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
const string output="Docs/UI/TrackSelection";
var flow=UnityEngine.Object.FindAnyObjectByType<Racer.RaceFlow>();
if(!flow.Save.DirectoryPath.Contains("TrackSelection"))throw new Exception("Isolated saves required");
var title=UnityEngine.Object.FindAnyObjectByType<Racer.StartupTitle>();if(title)UnityEngine.Object.Destroy(title.gameObject);
typeof(Racer.StartupTitle).GetField("completed",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic).SetValue(null,true);
flow.EnterMenuAfterTitle();flow.GetComponent<Racer.RaceMenus>().OpenSetup();
var settings=UnityEngine.InputSystem.InputSystem.settings;var background=settings.backgroundBehavior;var editor=settings.editorInputBehaviorInPlayMode;
settings.backgroundBehavior=UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;
settings.editorInputBehaviorInPlayMode=UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
var pad=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Gamepad>();
var keyboard=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Keyboard>();
object Get(object o,string n)=>o.GetType().GetField(n,flags).GetValue(o);
void Call(object o,string n,params object[] a)=>o.GetType().GetMethod(n,flags).Invoke(o,a);
Racer.RaceMenus Menus()=>UnityEngine.Object.FindAnyObjectByType<Racer.RaceFlow>().GetComponent<Racer.RaceMenus>();
UnityEngine.UI.Button Button(string id)=>((List<UnityEngine.UI.Button>)Get(Menus(),"buttons")).Single(b=>b.name==id&&b.gameObject.activeInHierarchy);
void Focus(string id)=>UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(Button(id).gameObject);
void Press(UnityEngine.InputSystem.LowLevel.GamepadButton b)=>UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad,new UnityEngine.InputSystem.LowLevel.GamepadState().WithButton(b));
void Release(){UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad,new UnityEngine.InputSystem.LowLevel.GamepadState());UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState());}
var log=new System.Text.StringBuilder();
void Check(bool good,string message){log.AppendLine((good?"PASS ":"FAIL ")+message);System.IO.File.WriteAllText(output+"/checks.txt",log.ToString());if(!good)throw new Exception(message);}
int[] courses={6,3,7,2,4,5,1,0};int index=SessionState.GetInt("TrackSelectionResume",0),step=0;string category="",saved="";var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene().handle;double next=EditorApplication.timeSinceStartup+1,deadline=next+240;
UnityEditor.EditorApplication.CallbackFunction tick=null;
void Finish(){EditorApplication.update-=tick;Release();UnityEngine.InputSystem.InputSystem.RemoveDevice(pad);UnityEngine.InputSystem.InputSystem.RemoveDevice(keyboard);settings.backgroundBehavior=background;settings.editorInputBehaviorInPlayMode=editor;Racer.RaceFlow.ValidationSaveRoot=null;EditorApplication.isPlaying=false;}
tick=()=>{if(EditorApplication.timeSinceStartup<next)return;next=EditorApplication.timeSinceStartup+.5;AudioListener.volume=0;
try{
 if(EditorApplication.timeSinceStartup>deadline)throw new Exception("Bounded fixture timeout at "+step+" course "+index);
 // Always release held input before waiting for the production release barrier.
 if(step==3||step==23||step==28){Release();step++;return;}
 if(Racer.MenuInput.Blocked)return;
 flow=UnityEngine.Object.FindAnyObjectByType<Racer.RaceFlow>();if(!flow||flow.Save==null)return;
 if(step==0){category=flow.Race.Category;saved=JsonUtility.ToJson(flow.Save.Settings);scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene().handle;Button("tracks").onClick.Invoke();step++;return;}
 if(step==1){Focus("course-"+courses[index]);step++;return;}
 if(step==2){Check(flow.Race.Category==category&&JsonUtility.ToJson(flow.Save.Settings)==saved&&UnityEngine.SceneManagement.SceneManager.GetActiveScene().handle==scene,"Focus preserves race/settings/scene: "+Racer.RacePlaylists.Titles[courses[index]]);
   if(index==6)UnityEngine.EventSystems.ExecuteEvents.Execute(Button("course-"+courses[index]).gameObject,new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){button=UnityEngine.EventSystems.PointerEventData.InputButton.Left},UnityEngine.EventSystems.ExecuteEvents.pointerClickHandler);
   else if(index==7)UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.Space));
   else Press(UnityEngine.InputSystem.LowLevel.GamepadButton.South);step++;return;}
 if(step==4){int c=courses[index];Check(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name==Racer.RacePlaylists.Scenes[c]&&flow.State==Racer.RaceFlow.Stage.Ready&&(string)Get(Menus(),"page")=="race","Confirm loads exact course and returns to Race Setup: "+Racer.RacePlaylists.Titles[c]);
   Check(((UnityEngine.UI.Text)Get(Menus(),"details")).text.Contains(flow.Race.courseName),"Race Setup summary reflects loaded course");
   Check(flow.Race.EligibleVehicles.Any(v=>v.Id==flow.Save.Settings.vehicleId)&&flow.Race.opponentRoster.All(id=>flow.Race.EligibleVehicles.Any(v=>v.Id==id)),"Player and opponent eligibility follows course");
   if(index<2){Button("start").onClick.Invoke();step=5;}else{index++;step=index<courses.Length?0:20;}return;}
 if(step==5){Check((flow.State==Racer.RaceFlow.Stage.Countdown||flow.State==Racer.RaceFlow.Stage.Racing)&&UnityEngine.SceneManagement.SceneManager.GetActiveScene().name==Racer.RacePlaylists.Scenes[courses[index]],"Start Race uses confirmed course: "+Racer.RacePlaylists.Titles[courses[index]]);flow.Pause();flow.QuitRace();Menus().OpenSetup();index++;step=0;return;}
 if(step==20){category=flow.Race.Category;saved=JsonUtility.ToJson(flow.Save.Settings);scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene().handle;Button("tracks").onClick.Invoke();step++;return;}
 if(step==21){Focus("course-7");step++;return;}
 if(step==22){Press(UnityEngine.InputSystem.LowLevel.GamepadButton.East);step++;return;}
 if(step==24){Check(flow.State==Racer.RaceFlow.Stage.Ready&&(string)Get(Menus(),"page")=="race"&&flow.Race.Category==category&&JsonUtility.ToJson(flow.Save.Settings)==saved,"Controller B cancels highlighted Backyard Reverse without committing");Button("tracks").onClick.Invoke();step++;return;}
 if(step==25){Focus("course-7");step++;return;}
 if(step==26){Button("preview-track").onClick.Invoke();step++;return;}
 if(step==27){Check((int)Get(Menus(),"previewTrack")==7&&(string)Get(Menus(),"page")=="course-preview"&&flow.Race.Category==category,"Optional highlighted-track map preview remains read-only");Racer.ThreeFeatureValidation.CaptureUi(output+"/preview.png");Press(UnityEngine.InputSystem.LowLevel.GamepadButton.East);step++;return;}
 if(step==29){Check((string)Get(Menus(),"page")==""&&flow.Race.Category==category,"Back from preview does not commit");flow.CloseGarage();flow.OpenBoards();step++;return;}
 if(step==30){Button("record-tracks").onClick.Invoke();step++;return;}
 if(step==31){Button("record-track-2").onClick.Invoke();step++;return;}
 if(step==32){Check((int)Get(Menus(),"recordCourse")==2&&flow.Race.Category==category&&JsonUtility.ToJson(flow.Save.Settings)==saved&&UnityEngine.SceneManagement.SceneManager.GetActiveScene().handle==scene,"Records Forest Forward browsing preserves race/settings/scene");flow.CloseExtras();var map=flow.GetComponent<Racer.ExplorationMap>();map.Open();Call(map,"OpenMapSheet",false,false,true);step++;return;}
 if(step==33){var map=flow.GetComponent<Racer.ExplorationMap>();((List<UnityEngine.UI.Button>)Get(map,"sheetButtons")).Single(b=>b.name=="map-track-5").onClick.Invoke();step++;return;}
 if(step==34){var map=flow.GetComponent<Racer.ExplorationMap>();Check((int)Get(map,"previewCourse")==5&&flow.Race.Category==category&&JsonUtility.ToJson(flow.Save.Settings)==saved&&UnityEngine.SceneManagement.SceneManager.GetActiveScene().handle==scene,"Maps Mountain Reverse browsing preserves race/settings/scene");map.Close();Check((string)Get(Menus(),"page")=="race","Race Setup remains caller after read-only browsing");Racer.ThreeFeatureValidation.CaptureUi(output+"/setup.png");System.IO.File.WriteAllText(output+"/done.txt",log.ToString());Finish();return;}
}catch(Exception e){System.IO.File.WriteAllText(output+"/error.txt",e.ToString());Finish();}};
EditorApplication.update+=tick;return "Scheduled eight confirmation checks, two starts, B/preview and read-only Records/Maps checks";
