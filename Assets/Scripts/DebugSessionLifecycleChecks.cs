#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace Racer
{
    public sealed class DebugSessionLifecycleChecks : MonoBehaviour
    {
        const string Output="Docs/DebugLifecycle";
        readonly List<string> checks=new();
        Keyboard keys; Gamepad pad; Mouse mouse; DeveloperLocationHud hud;
        InputSettings.BackgroundBehavior oldBackground;
        InputSettings.EditorInputBehaviorInPlayMode oldEditorInput;
        void Check(bool pass,string message) { checks.Add((pass?"PASS ":"FAIL ")+message);File.WriteAllLines(Output+"/checks.txt",checks); }
        IEnumerator Key(Key key) { InputSystem.QueueStateEvent(keys,new KeyboardState(key));yield return new WaitForSecondsRealtime(.08f);InputSystem.QueueStateEvent(keys,new KeyboardState());yield return new WaitForSecondsRealtime(.15f); }
        IEnumerator Pad(GamepadButton key) { InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(key));yield return new WaitForSecondsRealtime(.08f);InputSystem.QueueStateEvent(pad,new GamepadState());yield return new WaitForSecondsRealtime(.15f); }
        UnityEngine.UI.Button Button(string name)=>hud.GetComponentsInChildren<UnityEngine.UI.Button>(true).Single(b=>b.name==name);
        IEnumerator Select(string name) { for(int i=0;i<14&&EventSystem.current.currentSelectedGameObject!=Button(name).gameObject;i++)yield return Pad(GamepadButton.DpadDown);Check(EventSystem.current.currentSelectedGameObject==Button(name).gameObject,"Controller reaches "+name); }
        IEnumerator Capture(string comment)
        {
            yield return Key(UnityEngine.InputSystem.Key.F4);
            float end=Time.realtimeSinceStartup+5;while(!hud.CommentOpen&&Time.realtimeSinceStartup<end)yield return null;
            Check(hud.CommentOpen,"F4 opens capture comment");
            foreach(char c in comment)InputSystem.QueueTextEvent(keys,c);
            yield return null;yield return Key(UnityEngine.InputSystem.Key.Enter);
        }
        static string Digest(string path)=>Convert.ToBase64String(System.Security.Cryptography.SHA256.Create().ComputeHash(File.ReadAllBytes(path)));
        IEnumerator Start()
        {
            Directory.CreateDirectory(Output);oldBackground=InputSystem.settings.backgroundBehavior;oldEditorInput=InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            keys=InputSystem.AddDevice<Keyboard>();pad=InputSystem.AddDevice<Gamepad>();mouse=InputSystem.AddDevice<Mouse>();AudioListener.volume=0;Application.runInBackground=true;
            var race=FindAnyObjectByType<RaceDirector>();yield return null;yield return null;
            var title=FindAnyObjectByType<StartupTitle>();if(title)Destroy(title.gameObject);typeof(StartupTitle).GetField("completed",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,true);
            race.Flow.UseValidationSave(Path.GetFullPath("Temp/DebugLifecycleSave"));race.Flow.EnterMenuAfterTitle();race.Flow.StartFreeRoam();yield return new WaitForSecondsRealtime(.2f);
            DeveloperLocationHud.ValidationReportRoot=Path.GetFullPath(Output+"/Sessions");DeveloperLocationHud.ResetValidationSession();hud=DeveloperLocationHud.Instance;
            yield return Key(UnityEngine.InputSystem.Key.F3);yield return Capture("First session report one");yield return Capture("First session report two");
            var first=DeveloperLocationHud.Session;Check(first.Count==2&&first.Data.bugs.Select(b=>b.id).SequenceEqual(new[]{"BUG-001","BUG-002"}),"First session numbers BUG-001 and BUG-002");
            Check(hud.HudText.Contains(first.Id)&&hud.HudText.Contains("Reports 2"),"HUD shows session ID and count");
            yield return Key(UnityEngine.InputSystem.Key.F6);ThreeFeatureValidation.CaptureUi(Output+"/menu-open-session.png");yield return Select("Export Bug Report ZIP");yield return Pad(GamepadButton.South);
            string zip=hud.LastExport;Check(first.Closed&&first.Data.exported&&File.Exists(zip),"Successful in-game export closes session");
            var stored=JsonUtility.FromJson<DebugReportSession.Report>(File.ReadAllText(Path.Combine(first.DirectoryPath,"bugs.json")));Check(stored.closed&&stored.exported&&stored.sessionId==first.Id,"Closed lifecycle state persisted in session folder");
            using(var archive=ZipFile.OpenRead(zip)) { var entry=archive.Entries.Single(e=>e.FullName.EndsWith("/bugs.json"));using var reader=new StreamReader(entry.Open());var data=JsonUtility.FromJson<DebugReportSession.Report>(reader.ReadToEnd());Check(data.closed&&data.bugs.Count==2&&archive.Entries.Count(e=>e.FullName.EndsWith(".png"))==2,"Exported ZIP contains closed state and both correlated screenshots"); }
            bool rejected=false;try { first.Save(first.Data.bugs[0]); }catch(InvalidOperationException){rejected=true;}Check(rejected,"Closed session rejects appended captures");
            var originalFiles=Directory.GetFiles(first.DirectoryPath,"*",SearchOption.AllDirectories).ToDictionary(p=>p,Digest);string zipHash=Digest(zip);
            ThreeFeatureValidation.CaptureUi(Output+"/menu-exported-session.png");yield return Capture("New session report one");
            var second=DeveloperLocationHud.Session;Check(second.Id!=first.Id&&second.DirectoryPath!=first.DirectoryPath&&!second.Closed,"Next F4 creates a different timestamped open session");
            Check(second.Count==1&&second.Data.bugs[0].id=="BUG-001","New session restarts at BUG-001, not BUG-003");
            Check(Directory.GetFiles(Path.Combine(second.DirectoryPath,"Screenshots")).Select(Path.GetFileName).SequenceEqual(new[]{"BUG-001.png"})&&!File.ReadAllText(Path.Combine(second.DirectoryPath,"BUG_REPORT.md")).Contains("First session report"),"New session contains no old screenshot or report entry");
            Check(originalFiles.All(p=>File.Exists(p.Key)&&Digest(p.Key)==p.Value)&&File.Exists(zip)&&Digest(zip)==zipHash,"Previous session files and ZIP preserved byte-for-byte");
            yield return Key(UnityEngine.InputSystem.Key.F6);yield return Select("START NEW DEBUG SESSION");yield return Pad(GamepadButton.South);
            Check(hud.NewSessionConfirmationOpen&&!second.Closed,"Unexported reports require explicit confirmation");ThreeFeatureValidation.CaptureUi(Output+"/confirmation.png");
            yield return Key(UnityEngine.InputSystem.Key.F4);Check(hud.NewSessionConfirmationOpen&&DeveloperLocationHud.Session==second&&second.Count==1,"F4 cannot bypass pending confirmation");
            yield return Pad(GamepadButton.East);Check(!hud.NewSessionConfirmationOpen&&!second.Closed,"Controller B cancels without closing session");
            yield return Select("START NEW DEBUG SESSION");yield return Pad(GamepadButton.South);yield return Key(UnityEngine.InputSystem.Key.Enter);Check(!hud.NewSessionConfirmationOpen&&!second.Closed,"Default keyboard confirmation keeps current session");
            yield return Select("START NEW DEBUG SESSION");yield return Pad(GamepadButton.South);
            var button=Button("Close Session Without Export");var rect=(RectTransform)button.transform;Canvas.ForceUpdateCanvases();Vector2 point=RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(rect.rect.center));
            InputSystem.QueueStateEvent(mouse,new MouseState{position=point});yield return new WaitForSecondsRealtime(.1f);InputSystem.QueueStateEvent(mouse,new MouseState{position=point}.WithButton(MouseButton.Left));yield return new WaitForSecondsRealtime(.1f);InputSystem.QueueStateEvent(mouse,new MouseState{position=point});yield return new WaitForSecondsRealtime(.2f);
            Check(second.Closed&&!second.Data.exported&&!hud.NewSessionConfirmationOpen&&Directory.Exists(second.DirectoryPath),"Mouse confirmation closes unexported session and preserves its folder");
            Check(DeveloperLocationHud.Session==second&&Directory.GetDirectories(DeveloperLocationHud.ValidationReportRoot).Length==2,"Manual new-session action waits for capture; no empty folder created");
            yield return Capture("After manual new session");var third=DeveloperLocationHud.Session;Check(third.Id!=second.Id&&third.Count==1&&third.Data.bugs[0].id=="BUG-001","Manual restart also begins a new BUG-001");
            // A locked image produces a real archive failure without changing any user files.
            using(var locked=new FileStream(Path.Combine(third.DirectoryPath,"Screenshots/BUG-001.png"),FileMode.Open,FileAccess.Read,FileShare.None)){hud.Export();Check(!third.Closed&&!third.Data.exported&&hud.Error!=null,"Failed export leaves reports in the active session");}
            stored=JsonUtility.FromJson<DebugReportSession.Report>(File.ReadAllText(Path.Combine(third.DirectoryPath,"bugs.json")));Check(!stored.closed&&stored.bugs.Count==1,"Failed export restores persisted OPEN state");
            Check(File.Exists(zip)&&Digest(zip)==zipHash,"Failed export does not alter earlier ZIP history");
            yield return Key(UnityEngine.InputSystem.Key.F6);ThreeFeatureValidation.CaptureUi(Output+"/menu-new-session.png");
            File.WriteAllText(Output+"/session-paths.txt",first.DirectoryPath+"\n"+zip+"\n"+second.DirectoryPath+"\n"+third.DirectoryPath);File.WriteAllLines(Output+"/done.txt",checks);
        }
        void OnDestroy(){InputSystem.settings.backgroundBehavior=oldBackground;InputSystem.settings.editorInputBehaviorInPlayMode=oldEditorInput;if(keys!=null)InputSystem.RemoveDevice(keys);if(pad!=null)InputSystem.RemoveDevice(pad);if(mouse!=null)InputSystem.RemoveDevice(mouse);}
    }
}
#endif
