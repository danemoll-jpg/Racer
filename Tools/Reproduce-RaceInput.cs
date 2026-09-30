var flow=UnityEngine.Object.FindAnyObjectByType<Racer.RaceFlow>();
if(!flow.Save.DirectoryPath.Contains("RaceInput"))throw new Exception("Isolated saves required");
var title=UnityEngine.Object.FindAnyObjectByType<Racer.StartupTitle>();if(title)UnityEngine.Object.Destroy(title.gameObject);
typeof(Racer.StartupTitle).GetField("completed",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic).SetValue(null,true);
flow.EnterMenuAfterTitle();AudioListener.volume=0;
var input=UnityEngine.InputSystem.InputSystem.settings;var background=input.backgroundBehavior;var editor=input.editorInputBehaviorInPlayMode;
input.backgroundBehavior=UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;input.editorInputBehaviorInPlayMode=UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
var pad=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Gamepad>();
int step=0;float countdown=0;double next=EditorApplication.timeSinceStartup+1;
var log=new System.Text.StringBuilder();UnityEditor.EditorApplication.CallbackFunction tick=null;
tick=()=>{if(EditorApplication.timeSinceStartup<next)return;next=EditorApplication.timeSinceStartup+1;AudioListener.volume=0;
try{
 if(step==0)UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad,new UnityEngine.InputSystem.LowLevel.GamepadState{rightTrigger=1});
 if(step==1){flow.StartRace();countdown=flow.CountdownRemaining;}
 if(step==2)log.AppendLine("REPRODUCED held throttle blocks countdown: "+(Racer.MenuInput.Blocked&&flow.CountdownRemaining==countdown));
 if(step==3)UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad,new UnityEngine.InputSystem.LowLevel.GamepadState{rightTrigger=1}.WithButton(UnityEngine.InputSystem.LowLevel.GamepadButton.Start));
 if(step==4){log.AppendLine("REPRODUCED menu button ignored: "+(flow.State==Racer.RaceFlow.Stage.Countdown));UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad,new UnityEngine.InputSystem.LowLevel.GamepadState{rightTrigger=1}.WithButton(UnityEngine.InputSystem.LowLevel.GamepadButton.Select));}
 if(step==5){log.AppendLine("REPRODUCED map button ignored: "+!flow.GetComponent<Racer.ExplorationMap>().Opened);UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad,new UnityEngine.InputSystem.LowLevel.GamepadState());}
 if(step==6){log.AppendLine("Release throttle clears lock: "+!Racer.MenuInput.Blocked);System.IO.File.WriteAllText("Docs/UI/RaceInput/reproduction.txt",log.ToString());EditorApplication.update-=tick;UnityEngine.InputSystem.InputSystem.RemoveDevice(pad);input.backgroundBehavior=background;input.editorInputBehaviorInPlayMode=editor;Racer.RaceFlow.ValidationSaveRoot=null;EditorApplication.isPlaying=false;}
 step++;
}catch(Exception e){EditorApplication.update-=tick;System.IO.File.WriteAllText("Docs/UI/RaceInput/reproduction-error.txt",e.ToString());}};
EditorApplication.update+=tick;return "Scheduled held-throttle reproduction";
