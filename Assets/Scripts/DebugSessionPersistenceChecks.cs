#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace Racer
{
    // 0.67 Part A: a debug session survives quit/relaunch until Export or an explicit new session.
    // A relaunch is simulated by DeveloperLocationHud.ResetValidationSession(), which forgets every
    // in-memory session exactly as a new process would; the next use must resume from disk.
    public sealed class DebugSessionPersistenceChecks : MonoBehaviour
    {
        const string Output="Docs/Report067/DebugPersistence";
        readonly List<string> checks=new();
        Keyboard keys; DeveloperLocationHud hud; RaceDirector race;
        InputSettings.BackgroundBehavior oldBackground;
        InputSettings.EditorInputBehaviorInPlayMode oldEditorInput;
        void Check(bool pass,string message) { checks.Add((pass?"PASS ":"FAIL ")+message);File.WriteAllLines(Output+"/checks.txt",checks); }
        IEnumerator Key(Key key) { InputSystem.QueueStateEvent(keys,new KeyboardState(key));yield return new WaitForSecondsRealtime(.08f);InputSystem.QueueStateEvent(keys,new KeyboardState());yield return new WaitForSecondsRealtime(.15f); }
        IEnumerator Capture(string comment)
        {
            yield return Key(UnityEngine.InputSystem.Key.F4);
            float end=Time.realtimeSinceStartup+5;while(!hud.CommentOpen&&Time.realtimeSinceStartup<end)yield return null;
            foreach(char c in comment)InputSystem.QueueTextEvent(keys,c);
            yield return null;yield return Key(UnityEngine.InputSystem.Key.Enter);
        }
        static string Digest(string path)=>Convert.ToBase64String(System.Security.Cryptography.SHA256.Create().ComputeHash(File.ReadAllBytes(path)));
        static Dictionary<string,string> Snapshot(string folder)=>Directory.GetFiles(folder,"*",SearchOption.AllDirectories).ToDictionary(p=>p,Digest);
        static bool Same(Dictionary<string,string> before,string folder)=>Snapshot(folder).Count==before.Count&&before.All(p=>File.Exists(p.Key)&&Digest(p.Key)==p.Value);
        static void Relaunch()=>DeveloperLocationHud.ResetValidationSession();
        IEnumerator Start()
        {
            DontDestroyOnLoad(gameObject);Directory.CreateDirectory(Output);oldBackground=InputSystem.settings.backgroundBehavior;oldEditorInput=InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            keys=InputSystem.AddDevice<Keyboard>();AudioListener.volume=0;Application.runInBackground=true;
            race=FindAnyObjectByType<RaceDirector>();yield return null;yield return null;
            var title=FindAnyObjectByType<StartupTitle>();if(title)Destroy(title.gameObject);typeof(StartupTitle).GetField("completed",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,true);
            race.Flow.UseValidationSave(Path.GetFullPath("Temp/DebugPersistenceSave"));race.Flow.EnterMenuAfterTitle();race.Flow.StartFreeRoam();yield return new WaitForSecondsRealtime(.2f);
            string root=Path.GetFullPath("Temp/DebugPersistenceSessions");if(Directory.Exists(root))Directory.Delete(root,true);
            DeveloperLocationHud.ValidationReportRoot=root;Relaunch();hud=DeveloperLocationHud.Instance;
            Check(DeveloperLocationHud.Session==null&&hud.SessionText.Contains("NEW"),"Empty report folder: first launch shows NEW session");
            yield return Key(UnityEngine.InputSystem.Key.F3);yield return Capture("Before quit one");yield return Capture("Before quit two");
            var a=DeveloperLocationHud.Session;Check(a.Count==2&&!a.Closed,"Session A open with BUG-001/002");

            // 1. capture -> quit -> relaunch -> next capture continues the same session and numbering.
            Relaunch();var resumed=DeveloperLocationHud.Session;
            Check(resumed!=null&&resumed!=a&&resumed.Id==a.Id&&resumed.DirectoryPath==a.DirectoryPath&&resumed.Count==2,"Relaunch resumes newest OPEN session from disk (new process state)");
            Check(hud.SessionText.Contains(a.Id)&&hud.SessionText.Contains("OPEN / Reports 2"),"HUD/menu show resumed session ID, OPEN state and count before capture");
            yield return Capture("After relaunch");
            Check(DeveloperLocationHud.Session.Id==a.Id&&DeveloperLocationHud.Session.Data.bugs.Select(b=>b.id).SequenceEqual(new[]{"BUG-001","BUG-002","BUG-003"})&&File.Exists(Path.Combine(a.DirectoryPath,"Screenshots/BUG-003.png")),"Next F4 after relaunch is BUG-003 in the same folder");
            var stored=JsonUtility.FromJson<DebugReportSession.Report>(File.ReadAllText(Path.Combine(a.DirectoryPath,"bugs.json")));
            Check(stored.bugs.Count==3&&!stored.closed&&stored.bugs[0].comment=="Before quit one"&&File.ReadAllText(Path.Combine(a.DirectoryPath,"BUG_REPORT.md")).Contains("Reports: 3"),"Resumed session keeps earlier reports in JSON and Markdown");
            Check(Directory.GetDirectories(root).Length==1,"No orphan folder created by relaunch");

            // Scene change (back to menu and a fresh course load) keeps the session.
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;UnityEngine.SceneManagement.SceneManager.LoadScene(scene);yield return null;yield return null;yield return new WaitForSecondsRealtime(.3f);
            Check(DeveloperLocationHud.Session!=null&&DeveloperLocationHud.Session.Id==a.Id&&DeveloperLocationHud.Session.Count==3,"Scene reload keeps the open session");
            race=FindAnyObjectByType<RaceDirector>();var title2=FindAnyObjectByType<StartupTitle>();if(title2)Destroy(title2.gameObject);
            race.Flow.UseValidationSave(Path.GetFullPath("Temp/DebugPersistenceSave"));race.Flow.EnterMenuAfterTitle();race.Flow.StartFreeRoam();yield return new WaitForSecondsRealtime(.2f);hud=DeveloperLocationHud.Instance;

            // 2. export -> relaunch -> NEW.
            hud.SetEnabled(true);hud.OpenMenu();hud.Export();var aFiles=Snapshot(a.DirectoryPath);
            Check(DeveloperLocationHud.Session.Closed&&File.Exists(hud.LastExport),"Export closes resumed session");
            Relaunch();Check(DeveloperLocationHud.Session==null&&hud.SessionText.Contains("NEW"),"Export -> relaunch -> NEW");
            hud.CloseMenu();yield return Capture("Fresh after export");var b=DeveloperLocationHud.Session;
            Check(b.Id!=a.Id&&b.Count==1&&b.Data.bugs[0].id=="BUG-001","After export, next F4 starts new session at BUG-001");

            // 3. new-session-confirm -> relaunch -> NEW.
            hud.OpenMenu();hud.StartNewSession();Check(hud.NewSessionConfirmationOpen,"New session asks for confirmation");
            float unblock=Time.realtimeSinceStartup+3;while(MenuInput.UiBlocked&&Time.realtimeSinceStartup<unblock)yield return null;
            hud.GetComponentsInChildren<UnityEngine.UI.Button>(true).Single(x=>x.name=="Close Session Without Export").onClick.Invoke();
            Check(b.Closed&&!b.Data.exported,"Confirmed new session closes B without export");var bFiles=Snapshot(b.DirectoryPath);
            Relaunch();Check(DeveloperLocationHud.Session==null,"Confirmed new session -> relaunch -> NEW");
            hud.CloseMenu();yield return Capture("Session C");var c=DeveloperLocationHud.Session;Check(c.Id!=b.Id&&c.Data.bugs[0].id=="BUG-001","Next F4 starts session C at BUG-001");

            // 4. corrupt json -> NEW with old folder untouched.
            File.WriteAllText(Path.Combine(c.DirectoryPath,"bugs.json"),"{ this is not json");var cFiles=Snapshot(c.DirectoryPath);
            Relaunch();Check(DeveloperLocationHud.Session==null&&hud.SessionText.Contains("NEW"),"Corrupt newest bugs.json -> NEW session");
            yield return Capture("Session D");var d=DeveloperLocationHud.Session;Check(d.Id!=c.Id&&d.Data.bugs[0].id=="BUG-001","After corrupt session, F4 starts D at BUG-001");
            Check(Same(cFiles,c.DirectoryPath),"Corrupt session folder left untouched");

            // Only the newest open session is resumed; older orphaned OPEN sessions are untouched.
            yield return Capture("Session D two");var dFiles=Snapshot(d.DirectoryPath);
            var older=Path.Combine(root,"2000-01-01_00-00-00-000_orphan");Directory.CreateDirectory(Path.Combine(older,"Screenshots"));
            File.WriteAllText(Path.Combine(older,"bugs.json"),JsonUtility.ToJson(new DebugReportSession.Report{started="2000-01-01T00:00:00Z",sessionId=Path.GetFileName(older),bugs=new(){new DebugReportSession.Bug{id="BUG-001",screenshot="Screenshots/BUG-001.png"}}}));var olderFiles=Snapshot(older);
            Relaunch();Check(DeveloperLocationHud.Session!=null&&DeveloperLocationHud.Session.Id==d.Id&&DeveloperLocationHud.Session.Count==2,"Newest OPEN session resumed, not an older orphan");
            Check(Same(olderFiles,older)&&Same(aFiles,a.DirectoryPath)&&Same(bFiles,b.DirectoryPath)&&Same(dFiles,d.DirectoryPath),"Resume reads only; all session history byte-for-byte unchanged");
            File.WriteAllLines(Output+"/done.txt",checks);
            UnityEditor.EditorApplication.Exit(checks.All(x=>x.StartsWith("PASS"))?0:1);
        }
        void OnDestroy(){InputSystem.settings.backgroundBehavior=oldBackground;InputSystem.settings.editorInputBehaviorInPlayMode=oldEditorInput;if(keys!=null)InputSystem.RemoveDevice(keys);}
    }
}
#endif
