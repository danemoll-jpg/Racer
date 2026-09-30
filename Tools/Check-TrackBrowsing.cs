var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
var flow=UnityEngine.Object.FindAnyObjectByType<Racer.RaceFlow>();var menus=flow.GetComponent<Racer.RaceMenus>();var world=flow.GetComponent<Racer.ExplorationMap>();
if(!flow.Save.DirectoryPath.Contains("TrackBrowsing"))throw new Exception("Isolated saves required");
var title=UnityEngine.Object.FindAnyObjectByType<Racer.StartupTitle>();if(title)UnityEngine.Object.Destroy(title.gameObject);
typeof(Racer.StartupTitle).GetField("completed",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic).SetValue(null,true);flow.EnterMenuAfterTitle();AudioListener.volume=0;
string initial=flow.Race.Category,settings=JsonUtility.ToJson(flow.Save.Settings);var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene().handle;
var log=new System.Text.StringBuilder();const string output="Docs/UI/TrackBrowsing";
object Get(object o,string name)=>o.GetType().GetField(name,flags).GetValue(o);
void Call(object o,string name,params object[] args)=>o.GetType().GetMethod(name,flags).Invoke(o,args);
UnityEngine.UI.Button Button(string id)=>((List<UnityEngine.UI.Button>)Get(menus,"buttons")).Single(b=>b.name==id&&b.gameObject.activeInHierarchy);
void Check(bool good,string message){log.AppendLine((good?"PASS ":"FAIL ")+message);System.IO.File.WriteAllText(output+"/checks.txt",log.ToString());}
int step=0;double next=EditorApplication.timeSinceStartup+.5;UnityEditor.EditorApplication.CallbackFunction tick=null;
tick=()=>{if(EditorApplication.timeSinceStartup<next||Racer.MenuInput.Blocked)return;next=EditorApplication.timeSinceStartup+.5;AudioListener.volume=0;
try{
 if(step==0){flow.OpenBoards();}
 if(step==1){settings=JsonUtility.ToJson(flow.Save.Settings);Button("record-tracks").onClick.Invoke();Check(((List<UnityEngine.UI.Button>)Get(menus,"buttons")).Count(b=>b.gameObject.activeInHierarchy&&b.name.StartsWith("record-track-"))==8,"Records selector lists all eight track/directions without saved entries");}
 if(step==2){Button("record-track-6").onClick.Invoke();}
 if(step==3){Check((int)Get(menus,"recordCourse")==6,"Records track selection stays inside Records");Check(flow.Race.Category==initial&&JsonUtility.ToJson(flow.Save.Settings)==settings,"Records browse preserves race configuration and settings");Racer.ThreeFeatureValidation.CaptureUi(output+"/records-empty.png");string category=Racer.CoursePreviewCatalog.Courses[6].id+initial.Substring(flow.Race.courseId.Length);flow.Boards.Add("track-browse-record",category,false,92,"moto");flow.RefreshMenu();}
 if(step==4){Check(Button("record-track-browse-record")!=null,"Selected track shows its own saved record");typeof(Racer.RaceMenus).GetField("recordTab",flags).SetValue(menus,1);flow.RefreshMenu();}
 if(step==5){Check((int)Get(menus,"recordCourse")==6,"Lap to Race tab retains selected track");Button("record-tracks").onClick.Invoke();}
 if(step==6){Button("record-track-7").onClick.Invoke();}
 if(step==7){Check((int)Get(menus,"recordCourse")==7,"Reverse selectable separately");Racer.ThreeFeatureValidation.CaptureUi(output+"/records-reverse.png");Button("current").onClick.Invoke();}
 if(step==8){Check((int)Get(menus,"recordCourse")==-1,"Current race restores original board");flow.CloseExtras();world.Open();Call(world,"OpenMapSheet",false,false,true);}
 if(step==9){var buttons=(List<UnityEngine.UI.Button>)Get(world,"sheetButtons");Check(buttons.Count==9,"Map track picker contains Cancel and eight routes");Racer.ThreeFeatureValidation.CaptureUi(output+"/map-picker.png");buttons.Single(b=>b.name=="map-track-6").onClick.Invoke();}
 if(step==10){Check((int)Get(world,"previewCourse")==6,"Map displays selected forward route");var overlay=(Racer.WorldMapCourseOverlay)Get(world,"courseOverlay");Check(Get(overlay,"preview")==Racer.CoursePreviewCatalog.Courses[6]&&overlay.gameObject.activeSelf,"Map overlay uses selected saved geometry and is visible");Racer.ThreeFeatureValidation.CaptureUi(output+"/map-forward.png");Call(world,"OpenMapSheet",false,false,true);}
 if(step==11){((List<UnityEngine.UI.Button>)Get(world,"sheetButtons")).Single(b=>b.name=="map-track-7").onClick.Invoke();}
 if(step==12){Check((int)Get(world,"previewCourse")==7,"Map switches to separate reverse geometry");Racer.ThreeFeatureValidation.CaptureUi(output+"/map-reverse.png");Check(flow.Race.Category==initial&&scene==UnityEngine.SceneManagement.SceneManager.GetActiveScene().handle&&JsonUtility.ToJson(flow.Save.Settings)==settings,"Browsing both screens never loads a scene or changes active race/settings");Call(world,"OpenMapSheet",false,false,true);}
 if(step==13){((List<UnityEngine.UI.Button>)Get(world,"sheetButtons"))[0].onClick.Invoke();Check((int)Get(world,"previewCourse")==7,"Cancel keeps previously selected route");world.Close();}
 if(step==14){flow.OpenCourses();}
 if(step==15){Button("course-6").onClick.Invoke();}
 if(step==16){Check((string)Get(menus,"page")=="course-preview"&&Button("use-track")!=null,"Race Setup track selection opens preview before scene selection");Racer.ThreeFeatureValidation.CaptureUi(output+"/race-track-preview.png");Check(flow.Race.Category==initial&&scene==UnityEngine.SceneManagement.SceneManager.GetActiveScene().handle,"Race Setup preview does not load the course until Use This Track");Button("back").onClick.Invoke();}
 if(step==17){flow.CloseGarage();flow.Race.FreeRoam=false;typeof(Racer.RaceFlow).GetMethod("SetStage",flags).Invoke(flow,new object[]{Racer.RaceFlow.Stage.Racing});flow.Pause();flow.OpenBoards();}
 if(step==18){Check(flow.TrackBrowsingLocked&&!Button("record-tracks").interactable&&(int)Get(menus,"recordCourse")==-1,"Race-paused Records locks to current race");world.Open();Check((int)Get(world,"previewCourse")==-1,"Race-paused Map restores current course");Call(world,"OpenMapSheet",false,false,true);Check(!(bool)Get(world,"sheetOpen"),"Map ignores track-picker request during a race");Racer.ThreeFeatureValidation.CaptureUi(output+"/map-race-locked.png");world.Close();flow.CloseExtras();flow.QuitRace();flow.Race.FreeRoam=true;typeof(Racer.RaceFlow).GetMethod("SetStage",flags).Invoke(flow,new object[]{Racer.RaceFlow.Stage.Racing});flow.Pause();world.Open();}
 if(step==19){Check(!flow.TrackBrowsingLocked,"Free Roam allows track browsing");Call(world,"OpenMapSheet",false,false,true);Check((bool)Get(world,"sheetOpen"),"Free Roam map opens track picker");world.Close();}
 if(step++>=20){EditorApplication.update-=tick;System.IO.File.WriteAllText(output+"/done.txt",log.ToString());Racer.RaceFlow.ValidationSaveRoot=null;EditorApplication.isPlaying=false;}
}catch(Exception e){EditorApplication.update-=tick;System.IO.File.WriteAllText(output+"/error.txt",e.ToString());Racer.RaceFlow.ValidationSaveRoot=null;EditorApplication.isPlaying=false;}};
EditorApplication.update+=tick;return "Scheduled focused track-browsing checks";
