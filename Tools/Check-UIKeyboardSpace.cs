var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
var flow=UnityEngine.Object.FindAnyObjectByType<Racer.RaceFlow>();var menus=flow.GetComponent<Racer.RaceMenus>();
var title=UnityEngine.Object.FindAnyObjectByType<Racer.StartupTitle>();if(title)UnityEngine.Object.Destroy(title.gameObject);
typeof(Racer.StartupTitle).GetField("completed",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic).SetValue(null,true);
flow.UseValidationSave(System.IO.Path.GetFullPath("Temp/UIFoundationSave"));flow.EnterMenuAfterTitle();
var settings=UnityEngine.InputSystem.InputSystem.settings;var oldBackground=settings.backgroundBehavior;var oldEditor=settings.editorInputBehaviorInPlayMode;
settings.backgroundBehavior=UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;settings.editorInputBehaviorInPlayMode=UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
var keyboard=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Keyboard>();
typeof(Racer.RaceMenus).GetMethod("OpenKeyboard",flags).Invoke(menus,new object[]{"draft",64,new Func<string,bool>(s=>true),null});
int step=0;double next=UnityEditor.EditorApplication.timeSinceStartup+.5;
UnityEditor.EditorApplication.CallbackFunction tick=null;tick=()=>{if(UnityEditor.EditorApplication.timeSinceStartup<next)return;next=UnityEditor.EditorApplication.timeSinceStartup+.4;
 try{
  if(step==0){UnityEngine.InputSystem.InputSystem.QueueTextEvent(keyboard,' ');UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.Space));}
  if(step==1){string value=(string)typeof(Racer.RaceMenus).GetField("keyboardDraft",flags).GetValue(menus);System.IO.File.WriteAllText("Docs/UI/Phase12/keyboard-space.txt",(value=="draft "?"PASS":"FAIL")+" Space types one space and does not activate focused key; draft="+value);UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState());}
  if(step==2){UnityEngine.ScreenCapture.CaptureScreenshot("Docs/UI/Phase12/keyboard-screen.png");}
  if(step++>=4){UnityEditor.EditorApplication.update-=tick;UnityEngine.InputSystem.InputSystem.RemoveDevice(keyboard);settings.backgroundBehavior=oldBackground;settings.editorInputBehaviorInPlayMode=oldEditor;Racer.RaceFlow.ValidationSaveRoot=null;UnityEditor.EditorApplication.isPlaying=false;}
 }catch(Exception e){UnityEditor.EditorApplication.update-=tick;System.IO.File.WriteAllText("Docs/UI/Phase12/keyboard-space.txt",e.ToString());}
};UnityEditor.EditorApplication.update+=tick;return "Scheduled single keyboard Space ownership check";
