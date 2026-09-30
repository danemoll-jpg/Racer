var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
var flow=UnityEngine.Object.FindAnyObjectByType<Racer.RaceFlow>();var menus=flow.GetComponent<Racer.RaceMenus>();
if(!flow.Save.DirectoryPath.Contains("RecordFilters"))throw new Exception("Isolated saves required");
var title=UnityEngine.Object.FindAnyObjectByType<Racer.StartupTitle>();if(title)UnityEngine.Object.Destroy(title.gameObject);
typeof(Racer.StartupTitle).GetField("completed",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic).SetValue(null,true);flow.EnterMenuAfterTitle();AudioListener.volume=0;
string initial=flow.Race.Category,settings=JsonUtility.ToJson(flow.Save.Settings),song=flow.Radio.Song;var radio=flow.Radio;var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene().handle;
string era=flow.Race.courseId;string historical=era+"-older";var boards=flow.Boards;
for(int i=1;i<=12;i++)boards.Add("ui-lap-"+i,era+"-"+(i%2==0?"moto":"atv")+"-solo-clear-laps"+(i%5+1),false,120+i,i%2==0?"moto":"atv");
boards.Add("ui-history",historical+"-moto-solo-clear-laps5",false,90,"moto");
for(int i=1;i<=5;i++)boards.Add("ui-race-"+i,era+"-moto-solo-clear-laps"+i,true,120*i,"moto");
string archive=System.IO.Path.Combine(flow.Save.DirectoryPath,"top-ten-v1.json");byte[] before=System.IO.File.ReadAllBytes(archive);
object Get(string name)=>menus.GetType().GetField(name,flags).GetValue(menus);
void Set(string name,object value)=>menus.GetType().GetField(name,flags).SetValue(menus,value);
List<UnityEngine.UI.Button> Buttons()=>((List<UnityEngine.UI.Button>)Get("buttons")).Where(b=>b.gameObject.activeInHierarchy).ToList();
UnityEngine.UI.Button Button(string id)=>Buttons().Single(b=>b.name==id);
void Click(string id)=>Button(id).onClick.Invoke();
var log=new System.Text.StringBuilder();const string output="Docs/UI/RecordFilters";
void Check(bool good,string message){log.AppendLine((good?"PASS ":"FAIL ")+message);System.IO.File.WriteAllText(output+"/ui-checks.txt",log.ToString());}
int step=0;double next=EditorApplication.timeSinceStartup+.5;UnityEditor.EditorApplication.CallbackFunction tick=null;
tick=()=>{if(EditorApplication.timeSinceStartup<next||Racer.MenuInput.Blocked)return;next=EditorApplication.timeSinceStartup+.3;AudioListener.volume=0;
try{
 if(step==0){flow.OpenBoards();}
 if(step==1){Check((string)Get("recordsVehicle")=="All vehicles"&&Buttons().Count(b=>b.name.StartsWith("record-ui-lap-"))==10,"Default useful All Vehicles Top 10, capped and populated");Check(!Buttons().Any(b=>b.name=="categories"||b.name=="laps"),"Lap board exposes Filters, no Configurations or Laps");Racer.ThreeFeatureValidation.CaptureUi(output+"/lap-top-ten.png");Click("filters");}
 if(step==2){Check(!Buttons().Any(b=>b.name=="laps"),"Lap filter panel has no lap-count filter");Racer.ThreeFeatureValidation.CaptureUi(output+"/lap-filters.png");Click("vehicle");}
 if(step==3){Check(Buttons().Any(b=>b.name=="vehicle-moto")&&Buttons().Any(b=>b.name=="vehicle-atv")&&!Buttons().Any(b=>b.name=="vehicle-tourer"),"Vehicle choices use profiles present in saved era");Click("vehicle-moto");}
 if(step==4){Click("back");}
 if(step==5){Check(Buttons().Count(b=>b.name.StartsWith("record-ui-lap-"))==6,"Vehicle selection filters rendered table");Click("filters");}
 if(step==6){Click("era");}
 if(step==7){Click("era-"+historical);}
 if(step==8){Click("back");}
 if(step==9){Check(Buttons().Any(b=>b.name=="record-ui-history")&&!Buttons().Any(b=>b.name.StartsWith("record-ui-lap-")),"Historical selection shows only its era");Racer.ThreeFeatureValidation.CaptureUi(output+"/history.png");Click("current");}
 if(step==10){Set("recordTab",1);flow.RefreshMenu();}
 if(step==11){Check(Buttons().Any(b=>b.name=="laps"),"Race board has direct visible lap-count selector");Click("laps");}
 if(step==12){Check(Enumerable.Range(1,5).All(i=>Buttons().Any(b=>b.name=="laps-"+i)),"Race selector contains lengths 1 through 5");Click("laps-5");}
 if(step==13){Check(Buttons().Count(b=>b.name.StartsWith("record-ui-race-"))==1&&Buttons().Any(b=>b.name=="record-ui-race-5"),"Race filter renders only selected 5-lap total");Racer.ThreeFeatureValidation.CaptureUi(output+"/race-five-laps.png");Set("recordsVehicle","atv");flow.RefreshMenu();}
 if(step==14){Check(((UnityEngine.UI.Text)Get("details")).text.Contains("No times match"),"Empty filter combination explains recovery");Racer.ThreeFeatureValidation.CaptureUi(output+"/empty.png");Click("current");Set("recordTab",0);flow.RefreshMenu();}
 if(step>=15&&step<31){int index=(step-15)/2;if((step-15)%2==0){Click("record-tracks");if(index==0)Check(Buttons().Count(b=>b.name.StartsWith("record-track-"))==8,"All eight track/directions directly browsable");}else{Click("record-track-"+index);Check((int)Get("recordCourse")==index,"Track selection "+index+" retained");}}
 if(step==31){Check(flow.Race.Category==initial&&JsonUtility.ToJson(flow.Save.Settings)==settings&&scene==UnityEngine.SceneManagement.SceneManager.GetActiveScene().handle&&flow.Radio==radio&&flow.Radio.Song==song,"All Records actions preserve active race, settings, scene, radio instance and song");Check(before.SequenceEqual(System.IO.File.ReadAllBytes(archive)),"Browsing preserves isolated saved archive bytes");flow.CloseExtras();flow.Race.FreeRoam=false;typeof(Racer.RaceFlow).GetMethod("SetStage",flags).Invoke(flow,new object[]{Racer.RaceFlow.Stage.Racing});flow.Pause();flow.OpenBoards();}
 if(step==32){Check(flow.TrackBrowsingLocked&&!Button("record-tracks").interactable&&(int)Get("recordCourse")==-1,"Paused race locks Records to current track");Check(UnityEngine.Object.FindObjectsByType<UnityEngine.EventSystems.EventSystem>().Length==1&&Button("filters").GetComponentInParent<UnityEngine.UI.GraphicRaycaster>()!=null,"Existing EventSystem and interactive raycaster retained");}
 if(step++>=33){EditorApplication.update-=tick;System.IO.File.WriteAllText(output+"/ui-done.txt",log.ToString());Racer.RaceFlow.ValidationSaveRoot=null;EditorApplication.isPlaying=false;}
}catch(Exception e){EditorApplication.update-=tick;System.IO.File.WriteAllText(output+"/ui-error.txt",e.ToString());Racer.RaceFlow.ValidationSaveRoot=null;EditorApplication.isPlaying=false;}};
EditorApplication.update+=tick;return "Scheduled focused Records UI checks";
