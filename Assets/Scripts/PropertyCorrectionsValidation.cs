using System;using System.Collections;using System.IO;using System.Linq;using UnityEngine;using UnityEngine.InputSystem;using UnityEngine.InputSystem.LowLevel;using UnityEngine.SceneManagement;
namespace Racer {
 public sealed class PropertyCorrectionsValidation:MonoBehaviour {
  string output;readonly System.Collections.Generic.List<string> rows=new();
  public static void BeginEditor(string path){var v=new GameObject("Property corrections targeted checks").AddComponent<PropertyCorrectionsValidation>();v.output=Path.GetFullPath(path);DontDestroyOnLoad(v.gameObject);}
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,"-racerPropertyCorrections");if(i<0||i+1>=a.Length||FindAnyObjectByType<PropertyCorrectionsValidation>())return;var v=new GameObject("Property corrections targeted checks").AddComponent<PropertyCorrectionsValidation>();v.output=Path.GetFullPath(a[i+1]);DontDestroyOnLoad(v.gameObject);}
  void Update(){AudioListener.volume=0;}
  IEnumerator Start(){
   Directory.CreateDirectory(output);if(SceneManager.GetActiveScene().name!="ForestLoopReverse"){SceneManager.LoadScene("ForestLoopReverse");yield return null;}yield return new WaitForSecondsRealtime(1);
   var race=FindAnyObjectByType<RaceDirector>();var title=FindAnyObjectByType<StartupTitle>();if(title)Destroy(title.gameObject);race.Flow.EnterMenuAfterTitle();race.Flow.Radio?.GetComponent<AudioSource>()?.Stop();if(Application.isEditor)typeof(RaceFlow).GetProperty("Save").SetValue(race.Flow,new RacerSave(Path.Combine(output,"isolated-saves"),"property-corrections"));race.opponents=false;race.traffic=false;race.Flow.Save.Settings.master=0;race.Flow.StartRace();yield return new WaitForSecondsRealtime(4);
   var car=race.vehicle;car.enabled=false;car.GetComponent<VehicleInput>().enabled=false;car.GetComponent<VehicleRespawn>().enabled=false;car.Body.isKinematic=true;
   var hud=FindAnyObjectByType<DeveloperLocationHud>();Check(hud&&!hud.Visible,"XYZ HUD defaults off");
   var keyboard=InputSystem.AddDevice<Keyboard>();InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.F3));yield return null;yield return null;InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;
   Check(hud.Visible,"F3 shows XYZ HUD");var p=new Vector3(426.5f,84.5f,8.7f);car.transform.position=p;car.Body.position=p;yield return new WaitForSecondsRealtime(.15f);
   string prior=GUIUtility.systemCopyBuffer;InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.F4));yield return null;yield return null;InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;
   Check(GUIUtility.systemCopyBuffer=="Position: X=426.5, Y=84.5, Z=8.7 | Course: Forest Loop Reverse","F4 copies actual Unity world position and active course");Check(GameObject.Find("Location copied").GetComponent<UnityEngine.UI.Text>().enabled,"Temporary LOCATION COPIED confirmation visible");
   if(!Application.isEditor){yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(output,"xyz-hud.png"));}yield return new WaitForSecondsRealtime(1.9f);Check(!GameObject.Find("Location copied").GetComponent<UnityEngine.UI.Text>().enabled,"Copy confirmation expires without pausing");
   race.FreeRoam=true;hud.CopyLocation();Check(GUIUtility.systemCopyBuffer.EndsWith(" | Course: None"),"Free roam copies Course: None");race.FreeRoam=false;
   InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.F3));yield return null;yield return null;Check(!hud.Visible,"F3 hides HUD");InputSystem.RemoveDevice(keyboard);GUIUtility.systemCopyBuffer=prior;
   if(!Environment.GetCommandLineArgs().Contains("-locationHudOnly")){
   var wildlife=race.GetComponent<Wildlife>();var source=wildlife.Voice;source.Stop();Wildlife.QuietUntil=0;typeof(Wildlife).GetField("nextVoice",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).SetValue(wildlife,0f);
   Check(wildlife.coyoteCalls.Length==1&&wildlife.coyoteCalls[0].name=="ANMLWdog-coyote_howling-Elevenlabs","Only Dan's supplied recording is assigned");
   bool played=wildlife.Call(Wildlife.Species.Coyote,p+Vector3.right*20);Check(played&&source.isPlaying&&source.clip==wildlife.coyoteCalls[0],"Supplied recording actually plays from environmental position");
   Check(source.spatialBlend==1&&source.rolloffMode==AudioRolloffMode.Linear&&source.minDistance==12&&source.maxDistance==65&&source.dopplerLevel==0,"Full 3D linear distance attenuation 12–65m preserved");
   Check(source.pitch==1&&!source.loop&&source.volume<=.72f,"Natural pitch, existing ambient level, nonlooping");Check(!wildlife.Call(Wildlife.Species.Coyote,p),"Rapid repeat rejected");
   var pcm=new float[source.clip.samples*source.clip.channels];Check(source.clip.GetData(pcm,0)&&pcm.Any(v=>Math.Abs(v)>.01f)&&pcm.Max(v=>Math.Abs(v))<1,"Full imported recording decodes with nonzero unclipped signal");source.Stop();
   }
   File.WriteAllLines(Path.Combine(output,"checks.txt"),rows);
   #if UNITY_EDITOR
   if(Application.isEditor){UnityEditor.EditorApplication.isPlaying=false;yield break;}
   #endif
   Application.Quit(rows.Any(r=>r.StartsWith("FAIL"))?1:0);
  }
  void Check(bool pass,string what){rows.Add((pass?"PASS ":"FAIL ")+what);File.WriteAllLines(Path.Combine(output,"checks.txt"),rows);}
 }
}
