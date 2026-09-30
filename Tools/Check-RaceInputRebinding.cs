var flow=UnityEngine.Object.FindAnyObjectByType<Racer.RaceFlow>();if(!flow.Save.DirectoryPath.Contains("RaceInput"))throw new Exception("Isolated saves required");
var title=UnityEngine.Object.FindAnyObjectByType<Racer.StartupTitle>();if(title)UnityEngine.Object.Destroy(title.gameObject);
typeof(Racer.StartupTitle).GetField("completed",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic).SetValue(null,true);flow.EnterMenuAfterTitle();AudioListener.volume=0;
var settings=UnityEngine.InputSystem.InputSystem.settings;var background=settings.backgroundBehavior;var editor=settings.editorInputBehaviorInPlayMode;
settings.backgroundBehavior=UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;settings.editorInputBehaviorInPlayMode=UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
var pad=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Gamepad>();
var rebound=new UnityEngine.InputSystem.InputAction("Rebound confirm",UnityEngine.InputSystem.InputActionType.Button,"<Gamepad>/rightTrigger");rebound.Enable();
var log=new System.Text.StringBuilder();int step=0,phase=0,at=0;double next=EditorApplication.timeSinceStartup+1;UnityEditor.EditorApplication.CallbackFunction tick=null;
// EditorApplication callbacks read the Editor input buffer. Consume and inspect
// bound controls in the player's dynamic input update, just like production UI.
Action afterInput=()=>{
 if(UnityEngine.InputSystem.LowLevel.InputState.currentUpdateType!=UnityEngine.InputSystem.LowLevel.InputUpdateType.Dynamic)return;
 if(phase==1&&pad.rightTrigger.isPressed){Racer.MenuInput.ConsumeThroughRelease(rebound);log.AppendLine("Runtime trigger value="+pad.rightTrigger.ReadValue());at=Time.frameCount;phase=2;}
 else if(phase==2&&Time.frameCount>at+3){log.AppendLine((Racer.MenuInput.Blocked?"PASS ":"FAIL ")+"Rebound UI trigger remains consumed while held");phase=3;}
 else if(phase==4&&!pad.rightTrigger.isPressed){at=Time.frameCount;phase=5;}
 else if(phase==5&&Time.frameCount>at+3){log.AppendLine((!Racer.MenuInput.Blocked&&pad.leftTrigger.isPressed?"PASS ":"FAIL ")+"Rebound UI trigger release unlocks despite held brake; blocked="+Racer.MenuInput.Blocked+" throttle="+pad.rightTrigger.ReadValue()+" brake="+pad.leftTrigger.ReadValue());phase=6;}
};UnityEngine.InputSystem.InputSystem.onAfterUpdate+=afterInput;
tick=()=>{if(EditorApplication.timeSinceStartup<next)return;next=EditorApplication.timeSinceStartup+.6;AudioListener.volume=0;
try{
 if(step==0){phase=1;UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad,new UnityEngine.InputSystem.LowLevel.GamepadState{rightTrigger=1,leftTrigger=.6f});}
 if(phase==3){UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad,new UnityEngine.InputSystem.LowLevel.GamepadState{leftTrigger=.6f});phase=4;}
 if(phase==6||step>15){if(phase!=6)log.AppendLine("FAIL runtime fixture timeout");EditorApplication.update-=tick;UnityEngine.InputSystem.InputSystem.onAfterUpdate-=afterInput;rebound.Dispose();UnityEngine.InputSystem.InputSystem.RemoveDevice(pad);settings.backgroundBehavior=background;settings.editorInputBehaviorInPlayMode=editor;System.IO.File.WriteAllText("Docs/UI/RaceInput/rebinding.txt",log.ToString());Racer.RaceFlow.ValidationSaveRoot=null;EditorApplication.isPlaying=false;}step++;
}catch(Exception e){EditorApplication.update-=tick;System.IO.File.WriteAllText("Docs/UI/RaceInput/rebinding-error.txt",e.ToString());}};EditorApplication.update+=tick;return "Scheduled isolated rebound-control inspection";
