var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
var flow=UnityEngine.Object.FindAnyObjectByType<Racer.RaceFlow>();
var menus=flow.GetComponent<Racer.RaceMenus>();
var title=UnityEngine.Object.FindAnyObjectByType<Racer.StartupTitle>();if(title)UnityEngine.Object.Destroy(title.gameObject);
typeof(Racer.StartupTitle).GetField("completed",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic).SetValue(null,true);
flow.UseValidationSave(System.IO.Path.GetFullPath("Temp/UIFoundationSave"));flow.EnterMenuAfterTitle();
var settings=UnityEngine.InputSystem.InputSystem.settings;
var oldBackground=settings.backgroundBehavior;var oldEditor=settings.editorInputBehaviorInPlayMode;
settings.backgroundBehavior=UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;
settings.editorInputBehaviorInPlayMode=UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
var pad=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Gamepad>();
var submit=(UnityEngine.InputSystem.InputAction)typeof(Racer.RaceMenus).GetField("submit",flags).GetValue(menus);
var log=new System.Text.StringBuilder();int step=0;double next=UnityEditor.EditorApplication.timeSinceStartup+.5;
Action<UnityEngine.InputSystem.InputAction.CallbackContext> performed=c=>log.AppendLine("PERFORMED frame="+UnityEngine.Time.frameCount+" blocked="+Racer.MenuInput.Blocked+" nav="+UnityEngine.EventSystems.EventSystem.current.sendNavigationEvents+" selected="+UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject?.name);
submit.performed+=performed;
UnityEditor.EditorApplication.CallbackFunction tick=null;tick=()=>{if(UnityEditor.EditorApplication.timeSinceStartup<next)return;next=UnityEditor.EditorApplication.timeSinceStartup+.3;
 try{
  if(step==0){UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(UnityEngine.GameObject.Find("race"));log.AppendLine("Ready action="+submit.enabled+" selected="+UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject?.name);}
  if(step==1)UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad,new UnityEngine.InputSystem.LowLevel.GamepadState().WithButton(UnityEngine.InputSystem.LowLevel.GamepadButton.South));
  if(step==2){log.AppendLine("A result state="+flow.State+" page="+typeof(Racer.RaceMenus).GetField("page",flags).GetValue(menus)+" selected="+UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject?.name);UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad,new UnityEngine.InputSystem.LowLevel.GamepadState());}
  if(step==3)UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad,new UnityEngine.InputSystem.LowLevel.GamepadState().WithButton(UnityEngine.InputSystem.LowLevel.GamepadButton.East));
  if(step==4){log.AppendLine("B result state="+flow.State+" page="+typeof(Racer.RaceMenus).GetField("page",flags).GetValue(menus));UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad,new UnityEngine.InputSystem.LowLevel.GamepadState());}
  if(step==5){Racer.ThreeFeatureValidation.CaptureUi("Docs/UI/Phase12/main-controller.png");}
  if(step++>=6){UnityEditor.EditorApplication.update-=tick;submit.performed-=performed;UnityEngine.InputSystem.InputSystem.RemoveDevice(pad);settings.backgroundBehavior=oldBackground;settings.editorInputBehaviorInPlayMode=oldEditor;Racer.RaceFlow.ValidationSaveRoot=null;System.IO.File.WriteAllText("Docs/UI/Phase12/first-submit.txt",log.ToString());}
 }catch(Exception e){UnityEditor.EditorApplication.update-=tick;System.IO.File.WriteAllText("Docs/UI/Phase12/first-submit.txt",log+e.ToString());}
};UnityEditor.EditorApplication.update+=tick;return "Scheduled isolated A/B trace after device/action initialization";
