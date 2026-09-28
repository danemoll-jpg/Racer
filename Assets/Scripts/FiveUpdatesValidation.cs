using System;using System.Collections;using System.IO;using System.Linq;using UnityEngine;using UnityEngine.SceneManagement;
namespace Racer {
 // Explicit opt-in delivery fixture; never runs during ordinary play.
 public sealed class FiveUpdatesValidation:MonoBehaviour {
  string output;readonly System.Collections.Generic.List<string> rows=new();
  public static void BeginEditor(string path){var v=new GameObject("Five requested changes validation").AddComponent<FiveUpdatesValidation>();v.output=Path.GetFullPath(path);DontDestroyOnLoad(v.gameObject);}
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Boot(){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,"-racerFiveUpdates");if(i<0||i+1>=a.Length||FindAnyObjectByType<FiveUpdatesValidation>())return;var v=new GameObject("Five requested changes validation").AddComponent<FiveUpdatesValidation>();v.output=Path.GetFullPath(a[i+1]);DontDestroyOnLoad(v.gameObject);}
  void Update(){AudioListener.volume=0;}
  IEnumerator Start(){
   Directory.CreateDirectory(output);
   foreach(string scene in new[]{"StreetLoopGreybox","ForestLoopReverse"}){
    if(SceneManager.GetActiveScene().name!=scene){SceneManager.LoadScene(scene);yield return null;}
    yield return new WaitForSecondsRealtime(1);
    var race=FindAnyObjectByType<RaceDirector>();
    var title=FindAnyObjectByType<StartupTitle>();if(title)Destroy(title.gameObject);
    race.Flow.EnterMenuAfterTitle();race.Flow.Radio?.GetComponent<AudioSource>()?.Stop();
    if(Application.isEditor)typeof(RaceFlow).GetProperty("Save").SetValue(race.Flow,new RacerSave(Path.Combine(output,"isolated-saves"),"five-updates"));
    race.opponents=true;race.traffic=false;race.Flow.Save.Settings.master=0;race.Flow.StartRace();yield return new WaitForSecondsRealtime(4);
    var car=race.vehicle;car.enabled=false;car.GetComponent<VehicleInput>().enabled=false;car.GetComponent<VehicleRespawn>().enabled=false;car.Body.isKinematic=true;
    var map=FindAnyObjectByType<RacingMiniMap>();Check(map!=null,scene+" minimap exists");
    Check(race.Racers.Count>1,scene+" actual opponent racers present");
    if(scene=="ForestLoopReverse"){Check(race.Branches.Any(b=>b.title=="McFadden Cut"),"McFadden display name");Check(!race.Branches.Any(b=>b.title=="Pine Ridge"),"Inactive Forward Pine Ridge omitted");Check(!FindObjectsByType<TextMesh>().Any(t=>t.text.Contains("HOUSE 3 DETOUR")),"Floating label absent");Check(FindObjectsByType<PhysicalSign>().Any(s=>s.GetComponentsInChildren<TextMesh>().Any(t=>t.text.Contains("McFADDEN CUT"))),"McFadden physical sign");}
    int index=0;foreach(float s in scene=="StreetLoopGreybox"?new[]{2205f,2260f,2950f}:new[]{150f,230f,370f}){
     var p=race.road.At(s,out var f);car.transform.SetPositionAndRotation(p+Vector3.up*.65f,Quaternion.LookRotation(Vector3.ProjectOnPlane(f,Vector3.up)));car.Body.position=car.transform.position;car.Body.rotation=car.transform.rotation;
     // Bring one real AI marker into the local window; its live transform remains the map source.
     var rival=race.Racers[1].Car;rival.Body.position=p+f*25;rival.transform.position=rival.Body.position;
     yield return new WaitForSecondsRealtime(.15f);
     var ahead=map.Project(car.transform.position+car.transform.forward*20);var player=map.Project(car.transform.position);
     Check(ahead.y>player.y+10&&Math.Abs(ahead.x-player.x)<.05f,scene+" heading up turn "+index);
     Check(Vector2.Distance(player,RacingMiniMap.PlayerPoint)<.05f,scene+" player below centre "+index);
     yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(output,scene+"-map-"+index+++".png"));yield return null;
    }
    if(scene=="ForestLoopReverse"){
     var wildlife=race.GetComponent<Wildlife>();var source=wildlife.Voice;source.Stop();Wildlife.QuietUntil=0;
     typeof(Wildlife).GetField("nextVoice",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).SetValue(wildlife,0f);
     bool played=wildlife.Call(Wildlife.Species.Coyote,car.transform.position+Vector3.right*20);
     Check(played&&source.isPlaying,"Coyote playback starts");Check(source.spatialBlend==1&&source.minDistance==12&&source.maxDistance==65&&source.dopplerLevel==0,"Existing spatial attenuation preserved");Check(!source.loop&&source.clip.length<=3.01f&&source.clip.length>=2.99f,"Short nonlooping field recording");Check(!wildlife.Call(Wildlife.Species.Coyote,car.transform.position),"Immediate repeat rejected");
     var pcm=new float[source.clip.samples*source.clip.channels];bool read=source.clip.GetData(pcm,0);Check(read&&pcm.Any(v=>Math.Abs(v)>.01f),"Decoded coyote signal nonzero");
    }
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
