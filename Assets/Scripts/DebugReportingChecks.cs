#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace Racer
{
    // Explicit test host only, never attached by ordinary gameplay.
    public sealed class DebugReportingChecks : MonoBehaviour
    {
        const string Output="Docs/DebugReporting";
        readonly List<string> results=new();
        Keyboard keys; Gamepad pad; RaceDirector race; DeveloperLocationHud hud;
        void Check(bool ok,string message){results.Add((ok?"PASS ":"FAIL ")+message);File.WriteAllLines(Output+"/checks.txt",results);}
        IEnumerator KeyPress(Key key){InputSystem.QueueStateEvent(keys,new KeyboardState(key));yield return null;InputSystem.QueueStateEvent(keys,new KeyboardState());yield return null;yield return null;}
        IEnumerator Capture(){yield return KeyPress(Key.F4);float until=Time.realtimeSinceStartup+10;while(!hud.CommentOpen&&Time.realtimeSinceStartup<until)yield return null;Check(hud.CommentOpen&&!hud.Capturing,"F4 captured before comment dialog");}
        IEnumerator Start()
        {
            Directory.CreateDirectory(Output);AudioListener.volume=0;Application.runInBackground=true;
            keys=InputSystem.AddDevice<Keyboard>("Debug verification keyboard");pad=InputSystem.AddDevice<Gamepad>("Debug verification controller");
            race=FindAnyObjectByType<RaceDirector>();yield return null;yield return null;
            var title=FindAnyObjectByType<StartupTitle>();if(title)Destroy(title.gameObject);typeof(StartupTitle).GetField("completed",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,true);
            race.Flow.EnterMenuAfterTitle();race.opponents=race.traffic=false;race.laps=3;race.Flow.StartRace();while(race.Flow.State!=RaceFlow.Stage.Racing)yield return null;
            hud=DeveloperLocationHud.Instance;Check(hud&&!hud.Visible&&!DeveloperLocationHud.DebugEnabled,"Debug defaults off");
            yield return KeyPress(Key.F3);Check(hud.Visible&&!race.Flow.DebugMovementUsed,"F3 enables HUD without invalidating records");
            race.maximumRaceSeconds=.3f;yield return new WaitForSeconds(.8f);Check(!race.Racers[0].Dnf&&race.Flow.State==RaceFlow.Stage.Racing,"Debug mode suspends race timeout");race.maximumRaceSeconds=1800;
            var position=race.vehicle.transform.position;yield return Capture();
            Check(Time.timeScale==0&&!race.vehicle.GetComponent<VehicleInput>().enabled&&MenuInput.Blocked,"Comment owns input and freezes gameplay");
            var field=GameObject.Find("Comment").GetComponent<UnityEngine.UI.InputField>();
            foreach(char c in "Road needs inspection")InputSystem.QueueTextEvent(keys,c);yield return null;yield return null;
            Check(field.text=="Road needs inspection","Natural keyboard text input");
            ScreenCapture.CaptureScreenshot(Output+"/comment-overlay.png");
            yield return KeyPress(Key.Enter);
            Check(!hud.CommentOpen&&DeveloperLocationHud.Session.Count==1&&race.Flow.State==RaceFlow.Stage.Racing&&Time.timeScale==1,"Enter saves without leaking into pause");
            Check(!race.Flow.DebugMovementUsed,"HUD and capture preserve competitive eligibility");
            var first=DeveloperLocationHud.Session.Data.bugs[0];Check(first.course==race.courseName&&first.direction=="Reverse"&&first.mode=="Race"&&first.vehicle==race.vehicle.GetComponent<VehicleConfiguration>().profileId&&first.version==Application.version&&first.nextCheckpoint==race.Progress.NextGate,"Capture metadata matches live state");
            double priorBest=race.Flow.Save.Best.lap;race.Flow.Save.RecordLap(.1);race.Flow.Boards.Add("debug-verification/lap",race.Category,false,.1,"moto");
            var before=race.vehicle.Body.position;int gate=race.Progress.NextGate,lap=race.Progress.CompletedLaps;
            hud.StartFly();Check(hud.Flying&&Time.timeScale==0&&race.Flow.DebugMovementUsed,"Detached fly invalidates race and freezes world");
            Check(race.Flow.Save.Best.lap==priorBest&&!race.Flow.Boards.Board(race.Category,false).Any(e=>e.id=="debug-verification/lap"),"Debug run revokes this attempt's PB and Top 10 while preserving baseline");
            var cameraBefore=Camera.main.transform.position;InputSystem.QueueStateEvent(keys,new KeyboardState(Key.W,Key.E));yield return new WaitForSecondsRealtime(.25f);InputSystem.QueueStateEvent(keys,new KeyboardState());yield return null;
            Check(Vector3.Distance(Camera.main.transform.position,cameraBefore)>1&&Vector3.Distance(race.vehicle.Body.position,before)<.001f,"Fly keyboard movement moves camera only");
            cameraBefore=Camera.main.transform.position;InputSystem.QueueStateEvent(pad,new GamepadState{leftStick=new Vector2(.8f,0),rightTrigger=.7f,rightStick=new Vector2(.4f,.2f)});yield return new WaitForSecondsRealtime(.2f);InputSystem.QueueStateEvent(pad,new GamepadState());yield return null;
            Check(Vector3.Distance(Camera.main.transform.position,cameraBefore)>1,"Controller sticks/triggers move detached camera");
            var flyPosition=Camera.main.transform.position;yield return Capture();hud.SaveComment("Detached view: shoulder and road seam.");yield return null;
            var second=DeveloperLocationHud.Session.Data.bugs[1];Check(second.viewpoint=="Detached inspection camera"&&Vector3.Distance(second.position,flyPosition)<.01f&&second.debugMovementUsed,"F4 captures detached camera coordinates");
            hud.ReturnToVehicle();Check(!hud.Flying&&Time.timeScale==1&&race.Progress.NextGate==gate&&race.Progress.CompletedLaps==lap&&Vector3.Distance(race.vehicle.Body.position,before)<.01f,"Return preserves vehicle and race progression");
            race.Flow.Ghost.Boundary(race.Clock, before,Quaternion.identity,true,false);Check(!(bool)typeof(CleanLapGhost).GetField("active",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(race.Flow.Ghost),"Ghost remains disqualified across lap boundaries");
            yield return Capture();InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.East));yield return null;InputSystem.QueueStateEvent(pad,new GamepadState());yield return null;Check(!hud.CommentOpen&&DeveloperLocationHud.Session.Count==2,"Controller B cancels without saving a report");
            race.Flow.StartFreeRoam();yield return null;hud.StartFly();yield return null;yield return Capture();hud.SaveComment("Free roam inspection / third report.");yield return null;
            Check(DeveloperLocationHud.Session.Count==3&&DeveloperLocationHud.Session.Data.bugs[2].mode=="Free Roam","Multiple captures accumulate across race/free roam");
            Check(DeveloperLocationHud.Inspecting&&Time.timeScale==0,"Inspection cannot advance activity/discovery physics");hud.ReturnToVehicle();
            hud.OpenMenu();hud.Export();hud.OpenFolder();Check(File.Exists(hud.LastExport)&&hud.LastOpenedFolder==DeveloperLocationHud.Session.DirectoryPath&&hud.Error==null,"ZIP export and open-folder action");
            using(var zip=ZipFile.OpenRead(hud.LastExport)){Check(zip.Entries.Count(e=>e.FullName.EndsWith(".png"))==3&&zip.Entries.Any(e=>e.FullName.EndsWith("BUG_REPORT.md"))&&zip.Entries.Any(e=>e.FullName.EndsWith("bugs.json")),"ZIP contains report, JSON and three correlated screenshots");}
            var report=DeveloperLocationHud.Session;var json=JsonUtility.FromJson<DebugReportSession.Report>(File.ReadAllText(Path.Combine(report.DirectoryPath,"bugs.json")));var md=File.ReadAllText(Path.Combine(report.DirectoryPath,"BUG_REPORT.md"));Check(json.bugs.Count==3&&md.Contains("Screenshots/BUG-003.png")&&md.Contains("Road needs inspection"),"Markdown/JSON preserve comments and relative image references");
            File.WriteAllText(Output+"/session-path.txt",report.DirectoryPath+"\n"+hud.LastExport);ScreenCapture.CaptureScreenshot(Output+"/debug-menu.png");
            hud.CloseMenu();hud.SetEnabled(false);yield return null;yield return null;yield return null;Check(!DeveloperLocationHud.DebugEnabled&&!DeveloperLocationHud.Inspecting&&!DeveloperLocationHud.OwnsInput,"Exit restores ordinary controls");
            race.Flow.StartRace();while(race.Flow.State!=RaceFlow.Stage.Racing)yield return null;Check(!race.Flow.DebugMovementUsed,"Restart restores competitive eligibility");race.maximumRaceSeconds=.05f;yield return new WaitForSecondsRealtime(.4f);Check(race.Racers[0].Dnf,"Ordinary race timeout remains active when debug is off");
            File.WriteAllLines(Output+"/done.txt",results);
        }
        void OnDestroy(){if(keys!=null)InputSystem.RemoveDevice(keys);if(pad!=null)InputSystem.RemoveDevice(pad);}
    }
}
#endif
