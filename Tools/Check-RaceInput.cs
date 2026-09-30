var flow=UnityEngine.Object.FindAnyObjectByType<Racer.RaceFlow>();
if(!flow.Save.DirectoryPath.Contains("RaceInput"))throw new Exception("Isolated saves required");
var title=UnityEngine.Object.FindAnyObjectByType<Racer.StartupTitle>();if(title)UnityEngine.Object.Destroy(title.gameObject);
typeof(Racer.StartupTitle).GetField("completed",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic).SetValue(null,true);
flow.EnterMenuAfterTitle();AudioListener.volume=0;
var settings=UnityEngine.InputSystem.InputSystem.settings;var background=settings.backgroundBehavior;var editor=settings.editorInputBehaviorInPlayMode;
settings.backgroundBehavior=UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;settings.editorInputBehaviorInPlayMode=UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
var pad=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Gamepad>();var keyboard=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Keyboard>();
var map=flow.GetComponent<Racer.ExplorationMap>();var menus=flow.GetComponent<Racer.RaceMenus>();
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
UnityEngine.UI.Button Button(string id)=>((List<UnityEngine.UI.Button>)typeof(Racer.RaceMenus).GetField("buttons",flags).GetValue(menus)).Single(b=>b.name==id&&b.gameObject.activeInHierarchy);
void Pad(UnityEngine.InputSystem.LowLevel.GamepadButton? button=null,float throttle=1){var state=new UnityEngine.InputSystem.LowLevel.GamepadState{rightTrigger=throttle,leftTrigger=.6f,leftStick=new Vector2(.7f,0)};if(button.HasValue)state=state.WithButton(button.Value);UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad,state);}
void Keys(params UnityEngine.InputSystem.Key[] keys)=>UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState(keys));
var log=new System.Text.StringBuilder();void Check(bool good,string label){log.AppendLine((good?"PASS ":"FAIL ")+label);System.IO.File.WriteAllText("Docs/UI/RaceInput/checks.txt",log.ToString());if(!good)throw new Exception(label);}
var steps=new List<Action>();
steps.Add(()=>Pad());steps.Add(()=>flow.StartRace());
steps.Add(()=>Check(!Racer.MenuInput.Blocked&&flow.CountdownRemaining<3,"Held throttle/brake/steering does not block countdown"));
// A few normal ticks allow the real countdown to finish; no time-scale shortcut.
for(int i=0;i<6;i++)steps.Add(()=>{});
steps.Add(()=>{Check(flow.State==Racer.RaceFlow.Stage.Racing&&!Racer.MenuInput.Blocked,"Race begins with continuous driving input");flow.Race.vehicle.Body.isKinematic=true;});
for(int cycle=1;cycle<=2;cycle++){
 int run=cycle;
 steps.Add(()=>Pad(UnityEngine.InputSystem.LowLevel.GamepadButton.Start));
 steps.Add(()=>Check(flow.State==Racer.RaceFlow.Stage.Paused&&Racer.MenuInput.Blocked,"Controller Pause opens once while held, cycle "+run));
 steps.Add(()=>Pad());
 steps.Add(()=>{Check(flow.State==Racer.RaceFlow.Stage.Paused&&!Racer.MenuInput.Blocked,"Releasing Pause unlocks menu with driving input held, cycle "+run);UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(Button("resume").gameObject);Pad(UnityEngine.InputSystem.LowLevel.GamepadButton.South);});
 steps.Add(()=>Check(flow.State==Racer.RaceFlow.Stage.Racing&&Racer.MenuInput.Blocked,"Held Confirm resumes once and cannot submit hidden menu, cycle "+run));
 steps.Add(()=>Pad());
 steps.Add(()=>{Check(!Racer.MenuInput.Blocked,"Confirm release unlocks with throttle still held, cycle "+run);Pad(UnityEngine.InputSystem.LowLevel.GamepadButton.Select);});
 steps.Add(()=>Check(map.Opened&&flow.State==Racer.RaceFlow.Stage.Paused&&Racer.MenuInput.Blocked,"Controller map opens once while held, cycle "+run));
 steps.Add(()=>Pad());
 steps.Add(()=>{Check(map.Opened&&!Racer.MenuInput.Blocked,"Map is interactive after Map release with throttle held, cycle "+run);Pad(UnityEngine.InputSystem.LowLevel.GamepadButton.East);});
 steps.Add(()=>Check(!map.Opened&&flow.State==Racer.RaceFlow.Stage.Racing&&Racer.MenuInput.Blocked,"Map Back closes and resumes once, cycle "+run));
 steps.Add(()=>Pad());steps.Add(()=>Check(!Racer.MenuInput.Blocked,"Map can be reopened without releasing driving controls, cycle "+run));
}
steps.Add(()=>{UnityEngine.InputSystem.InputSystem.QueueStateEvent(pad,new UnityEngine.InputSystem.LowLevel.GamepadState());Keys(UnityEngine.InputSystem.Key.W,UnityEngine.InputSystem.Key.A);});
for(int cycle=1;cycle<=2;cycle++){
 int run=cycle;
 steps.Add(()=>Keys(UnityEngine.InputSystem.Key.W,UnityEngine.InputSystem.Key.A,UnityEngine.InputSystem.Key.Escape));
 steps.Add(()=>Check(flow.State==Racer.RaceFlow.Stage.Paused&&Racer.MenuInput.Blocked,"Keyboard Escape pauses while W/A held, cycle "+run));
 steps.Add(()=>Keys(UnityEngine.InputSystem.Key.W,UnityEngine.InputSystem.Key.A));
 steps.Add(()=>{Check(!Racer.MenuInput.Blocked,"Escape release unlocks despite W/A, cycle "+run);Keys(UnityEngine.InputSystem.Key.W,UnityEngine.InputSystem.Key.A,UnityEngine.InputSystem.Key.Escape);});
 steps.Add(()=>Check(flow.State==Racer.RaceFlow.Stage.Racing,"Second Escape resumes without a duplicate pause, cycle "+run));
 steps.Add(()=>Keys(UnityEngine.InputSystem.Key.W,UnityEngine.InputSystem.Key.A));
 steps.Add(()=>Keys(UnityEngine.InputSystem.Key.W,UnityEngine.InputSystem.Key.A,UnityEngine.InputSystem.Key.M));
 steps.Add(()=>Check(map.Opened&&Racer.MenuInput.Blocked,"Keyboard M opens map once while held, cycle "+run));
 steps.Add(()=>Keys(UnityEngine.InputSystem.Key.W,UnityEngine.InputSystem.Key.A));
 steps.Add(()=>{Check(map.Opened&&!Racer.MenuInput.Blocked,"M release unlocks map despite W/A, cycle "+run);Keys(UnityEngine.InputSystem.Key.W,UnityEngine.InputSystem.Key.A,UnityEngine.InputSystem.Key.M);});
 steps.Add(()=>Check(!map.Opened&&flow.State==Racer.RaceFlow.Stage.Racing,"Second M closes map and resumes, cycle "+run));
 steps.Add(()=>Keys(UnityEngine.InputSystem.Key.W,UnityEngine.InputSystem.Key.A));
}
steps.Add(()=>Pad(UnityEngine.InputSystem.LowLevel.GamepadButton.Start));
steps.Add(()=>{Check(flow.State==Racer.RaceFlow.Stage.Paused&&Racer.MenuInput.Blocked,"Consumed Pause waits for its controller");UnityEngine.InputSystem.InputSystem.RemoveDevice(pad);});
steps.Add(()=>Check(!Racer.MenuInput.Blocked,"Controller disconnect clears consumed control without releasing keyboard driving keys"));
// The direct rebound-action check runs in the player input context in
// Check-RaceInputRebinding.cs, not in this EditorApplication callback.
int step=0;double next=EditorApplication.timeSinceStartup+1;UnityEditor.EditorApplication.CallbackFunction tick=null;
void Finish(){EditorApplication.update-=tick;if(pad.added)UnityEngine.InputSystem.InputSystem.RemoveDevice(pad);UnityEngine.InputSystem.InputSystem.RemoveDevice(keyboard);settings.backgroundBehavior=background;settings.editorInputBehaviorInPlayMode=editor;Racer.RaceFlow.ValidationSaveRoot=null;EditorApplication.isPlaying=false;}
tick=()=>{if(EditorApplication.timeSinceStartup<next)return;next=EditorApplication.timeSinceStartup+.6;AudioListener.volume=0;
try{if(step<steps.Count){steps[step++]();return;}System.IO.File.WriteAllText("Docs/UI/RaceInput/done.txt",log.ToString());Finish();}
catch(Exception e){System.IO.File.WriteAllText("Docs/UI/RaceInput/error.txt","Step "+step+"\n"+e);Finish();}};
EditorApplication.update+=tick;return "Scheduled bounded repeated controller/keyboard pause/map checks with continuous driving input";
