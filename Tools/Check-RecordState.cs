var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;var flow=UnityEngine.Object.FindAnyObjectByType<Racer.RaceFlow>();var menus=flow.GetComponent<Racer.RaceMenus>();
if(!flow.Save.DirectoryPath.Contains("RecordFilters"))throw new Exception("Isolated saves required");
var title=UnityEngine.Object.FindAnyObjectByType<Racer.StartupTitle>();if(title)UnityEngine.Object.Destroy(title.gameObject);
typeof(Racer.StartupTitle).GetField("completed",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic).SetValue(null,true);flow.EnterMenuAfterTitle();AudioListener.volume=0;
string settings=JsonUtility.ToJson(flow.Save.Settings),category=flow.Race.Category,song=flow.Radio.Song;var radio=flow.Radio;var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene().handle;var log=new System.Text.StringBuilder();
void Set(string name,object value)=>menus.GetType().GetField(name,flags).SetValue(menus,value);
UnityEngine.UI.Button Button(string id)=>((List<UnityEngine.UI.Button>)menus.GetType().GetField("buttons",flags).GetValue(menus)).Single(b=>b.name==id&&b.gameObject.activeInHierarchy);
void Check(bool good,string message){log.AppendLine((good?"PASS ":"FAIL ")+message);}
int step=0;double next=EditorApplication.timeSinceStartup+.5;UnityEditor.EditorApplication.CallbackFunction tick=null;
tick=()=>{if(EditorApplication.timeSinceStartup<next||Racer.MenuInput.Blocked)return;next=EditorApplication.timeSinceStartup+.4;AudioListener.volume=0;
try{
 if(step==0){log.AppendLine("After title stabilization: settingsEqual="+(settings==JsonUtility.ToJson(flow.Save.Settings))+"; songEqual="+(song==flow.Radio.Song));if(settings!=JsonUtility.ToJson(flow.Save.Settings)){log.AppendLine("Before: "+settings);log.AppendLine("After: "+JsonUtility.ToJson(flow.Save.Settings));}settings=JsonUtility.ToJson(flow.Save.Settings);song=flow.Radio.Song;flow.OpenBoards();}
 if(step>=1&&step<=16){int i=(step-1)/2;if(step%2==1)Button("record-tracks").onClick.Invoke();else Button("record-track-"+i).onClick.Invoke();}
 if(step==17){Set("recordTab",1);Set("recordsLaps",5);Set("recordsVehicle","moto");flow.RefreshMenu();Button("filters").onClick.Invoke();}
 if(step==18){Button("reset-filters").onClick.Invoke();}
 if(step==19){Button("back").onClick.Invoke();}
 if(step==20){Button("current").onClick.Invoke();}
 if(step>=21){Check(category==flow.Race.Category,"Active race category unchanged");Check(settings==JsonUtility.ToJson(flow.Save.Settings),"Saved settings unchanged after stabilized baseline");Check(scene==UnityEngine.SceneManagement.SceneManager.GetActiveScene().handle,"No scene load");Check(radio==flow.Radio,"Same radio instance");Check(song==flow.Radio.Song,"Same song");Check(UnityEngine.EventSystems.EventSystem.current!=null,"Existing controller navigation EventSystem present");EditorApplication.update-=tick;System.IO.File.WriteAllText("Docs/UI/RecordFilters/state-checks.txt",log.ToString());Racer.RaceFlow.ValidationSaveRoot=null;EditorApplication.isPlaying=false;return;}
 step++;
}catch(Exception e){EditorApplication.update-=tick;System.IO.File.WriteAllText("Docs/UI/RecordFilters/state-error.txt",e.ToString());EditorApplication.isPlaying=false;}};
EditorApplication.update+=tick;return "Scheduled state preservation checks";
