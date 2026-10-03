#if UNITY_EDITOR
using System;using System.IO;using System.Linq;using System.Collections;using System.Collections.Generic;using System.Reflection;using UnityEngine;using UnityEngine.EventSystems;using UnityEngine.InputSystem;using UnityEngine.InputSystem.LowLevel;
namespace Racer {
// 0.70 Part A checks (muted, isolated save): every quit-to-desktop control asks "QUIT WOODSTOCK RUSH?" first, focus starts on
// CANCEL, Esc / B / Cancel return without quitting (keyboard, controller, mouse), and QUIT exits (ends play mode in the editor).
public sealed class Report070QuitChecks:MonoBehaviour {
 InputSettings.BackgroundBehavior oldBackground;InputSettings.EditorInputBehaviorInPlayMode oldEditorInput;
 void OnDestroy(){InputSystem.settings.backgroundBehavior=oldBackground;InputSystem.settings.editorInputBehaviorInPlayMode=oldEditorInput;}
 RaceDirector race;RaceFlow flow;RaceMenus menus;Gamepad pad;Keyboard keyboard;readonly List<string> rows=new();string output;
 static readonly BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
 void Check(bool ok,string label){rows.Add((ok?"PASS ":"FAIL ")+label);File.WriteAllLines(output+"/quit-checks.txt",rows);Debug.Log("REPORT070 "+rows[^1]);}
 object Get(string name)=>typeof(RaceMenus).GetField(name,Private).GetValue(menus);
 List<UnityEngine.UI.Button> Buttons=>(List<UnityEngine.UI.Button>)Get("buttons");
 UnityEngine.UI.Button Button(string id)=>Buttons.Single(b=>b.name==id&&b.gameObject.activeInHierarchy);
 string Label(string id)=>Button(id).GetComponentInChildren<UnityEngine.UI.Text>(true).text;
 IEnumerator Settle(){yield return null;yield return null;while(MenuInput.Blocked)yield return null;yield return null;}
 IEnumerator Key1(Key k){yield return Settle();InputSystem.QueueStateEvent(keyboard,new KeyboardState(k));yield return null;yield return null;InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return Settle();}
 IEnumerator Pad1(GamepadButton b){yield return Settle();InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(b));yield return null;yield return null;InputSystem.QueueStateEvent(pad,new GamepadState());yield return Settle();}
 IEnumerator Mouse1(string id){yield return Settle();Button(id).onClick.Invoke();yield return Settle();}
 IEnumerator Select(string id){yield return Settle();EventSystem.current.SetSelectedGameObject(Button(id).gameObject);yield return null;}
 bool QuitModal()=>menus.ModalOpen&&(string)Get("modalTitle")=="QUIT WOODSTOCK RUSH?";
 string Focus=>EventSystem.current.currentSelectedGameObject?EventSystem.current.currentSelectedGameObject.name:"none";
 void Opened(string how){Check(QuitModal()&&Focus=="cancel"&&Label("confirm")=="QUIT"&&Label("cancel")=="CANCEL",$"{how}: confirmation 'QUIT WOODSTOCK RUSH?' [CANCEL] [QUIT] shown, focus on {Focus}, still running={Application.isPlaying}");}
 IEnumerator Start(){AudioListener.volume=0;output=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(output);Application.runInBackground=true;race=FindAnyObjectByType<RaceDirector>();yield return null;yield return null;
  flow=race.Flow;flow.UseValidationSave(Path.GetFullPath("Temp/Report070Save"));
  var title=FindAnyObjectByType<StartupTitle>();if(title)Destroy(title.gameObject);typeof(StartupTitle).GetField("completed",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,true);flow.EnterMenuAfterTitle();
  // As in UIFoundationChecks: keyboard events reach the game without Game-view focus while the check runs (restored after).
  oldBackground=InputSystem.settings.backgroundBehavior;oldEditorInput=InputSystem.settings.editorInputBehaviorInPlayMode;
  InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
  menus=FindAnyObjectByType<RaceMenus>();pad=InputSystem.AddDevice<Gamepad>();keyboard=InputSystem.AddDevice<Keyboard>();yield return Settle();
  if(flow.State!=RaceFlow.Stage.Ready){flow.Pause();flow.QuitRace();yield return Settle();}
  // Main menu, keyboard: Space (menu Select) on QUIT GAME opens; Esc cancels.
  yield return Select("quit");Check(Label("quit")=="QUIT GAME","Main menu has QUIT GAME");yield return Key1(Key.Space);Opened("Main menu / keyboard Space");
  yield return Key1(Key.Escape);Check(!menus.ModalOpen&&flow.State==RaceFlow.Stage.Ready&&Application.isPlaying,"Main menu / keyboard Esc cancels, back on the main menu");
  // Main menu, controller: A opens; A on the default CANCEL returns; A again then B cancels.
  yield return Select("quit");yield return Pad1(GamepadButton.South);Opened("Main menu / controller A");
  yield return Pad1(GamepadButton.South);Check(!menus.ModalOpen&&Application.isPlaying,"Main menu / controller A on default CANCEL returns");
  yield return Select("quit");yield return Pad1(GamepadButton.South);yield return Pad1(GamepadButton.East);Check(!menus.ModalOpen&&flow.State==RaceFlow.Stage.Ready&&Application.isPlaying,"Main menu / controller B cancels");
  // Main menu, mouse: click QUIT GAME, click CANCEL.
  yield return Mouse1("quit");Opened("Main menu / mouse click");yield return Mouse1("cancel");Check(!menus.ModalOpen&&Application.isPlaying,"Main menu / mouse CANCEL returns");
  // Pause menu (in a race): Quit Game asks the same; B cancels back to the paused race.
  flow.OpenGarage();flow.SelectVehicle("moto");flow.CloseGarage();race.opponents=race.traffic=false;race.laps=1;flow.StartRace();
  while(flow.State!=RaceFlow.Stage.Racing){AudioListener.volume=0;yield return null;}flow.Pause();yield return Settle();
  Check(flow.State==RaceFlow.Stage.Paused&&Label("quit")=="Quit Game","Pause menu has Quit Game");
  yield return Select("quit");yield return Pad1(GamepadButton.South);Opened("Pause menu / controller A");
  yield return Pad1(GamepadButton.East);Check(!menus.ModalOpen&&flow.State==RaceFlow.Stage.Paused&&Application.isPlaying,"Pause menu / controller B cancels, race still paused");
  yield return Mouse1("quit");Opened("Pause menu / mouse click");yield return Key1(Key.Escape);yield return Settle();Check(!menus.ModalOpen&&flow.State==RaceFlow.Stage.Paused,"Pause menu / keyboard Esc cancels");
  // END RACE / RETURN TO MENU keeps its own confirmation (unchanged).
  yield return Mouse1("return");Check(menus.ModalOpen&&(string)Get("modalTitle")=="END RACE AND RETURN TO MENU?"&&Label("confirm")=="CONFIRM","END RACE / RETURN TO MENU confirmation unchanged");yield return Mouse1("cancel");
  // QUIT exits: in the editor RaceFlow.Quit ends play mode (Application.Quit in a build). Report070Play records the exit.
  yield return Mouse1("quit");Opened("Pause menu / before QUIT");File.WriteAllText(output+"/quit-pressed.txt","QUIT pressed at "+DateTime.Now.ToString("o"));
  Button("confirm").onClick.Invoke();yield return null;yield return null;
  Check(false,"QUIT did not leave play mode");UnityEditor.EditorApplication.Exit(1);}
}
}
#endif
