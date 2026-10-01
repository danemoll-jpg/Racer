using System;using System.IO;using System.Linq;using System.Collections;using System.Reflection;using UnityEngine;using UnityEngine.InputSystem;using UnityEngine.InputSystem.LowLevel;
namespace Racer {
// Explicit local verification only; never attached in saved scenes.
public sealed class FinalTwoAudioChecks:MonoBehaviour {
 const string D="Docs/FinalTwo/audio";const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 IEnumerator Start(){Directory.CreateDirectory(D);Application.runInBackground=true;var race=FindAnyObjectByType<RaceDirector>();yield return null;yield return null;
 #if UNITY_EDITOR
 race.Flow.UseValidationSave(Path.GetFullPath("Temp/FinalTwoAudioSave"));
 #endif
 var mute=FindAnyObjectByType<TestAudioMute>();if(mute)mute.enabled=false;var title=FindAnyObjectByType<StartupTitle>();if(title)Destroy(title.gameObject);yield return null;
 var settings=race.Flow.Save.Settings;settings.master=.8f;settings.music=.6f;settings.vehicle=.75f;settings.ambience=1;settings.radioOn=true;race.Flow.Save.ApplySettings();if(!race.Flow.Radio)race.Flow.EnterMenuAfterTitle();race.Flow.Radio.SetFolder(Path.GetFullPath("BundleMusic"));
 race.Flow.OpenGarage();race.Flow.SelectVehicle("moto");race.Flow.CloseGarage();race.opponents=race.traffic=false;race.Flow.StartRace();while(race.Flow.State!=RaceFlow.Stage.Racing)yield return null;
 float until=Time.realtimeSinceStartup+20;while(!race.Flow.Radio.Playing&&Time.realtimeSinceStartup<until)yield return null;
 var car=race.vehicle;car.enabled=false;car.GetComponent<VehicleInput>().enabled=true;var pad=InputSystem.AddDevice<Gamepad>();InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
 var listener=FindAnyObjectByType<AudioListener>();var mix=listener.gameObject.AddComponent<CorrectionAudioCapture>();var life=FindObjectsByType<UndergroundLife>().Single(x=>!x.cave);var voice=life.GetComponentInChildren<AudioSource>();var source=voice.gameObject.AddComponent<CorrectionAudioCapture>();var b=race.Branches.Single(x=>x.title.StartsWith("Storm"));
 foreach(var profile in new[]{"moto","atv"}){
 car.GetComponent<VehicleConfiguration>().Apply(profile);var homes=(Vector3[])typeof(UndergroundLife).GetField("homes",F).GetValue(life);for(int i=0;i<life.rats.Length;i++){life.rats[i].position=homes[i];life.rats[i].gameObject.SetActive(true);}typeof(UndergroundLife).GetField("armed",F).SetValue(life,true);
 float start=b.Project(life.entrance,out _)-24;var p=b.At(start,out var f);car.Body.isKinematic=false;car.Body.position=p+Vector3.up*.55f;car.Body.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(f,Vector3.up));car.transform.SetPositionAndRotation(car.Body.position,car.Body.rotation);car.Body.linearVelocity=car.Body.angularVelocity=Vector3.zero;car.ClearSteering();Physics.SyncTransforms();FindAnyObjectByType<ChaseCamera>().Snap();race.Racers[0].Branch.Begin(b);race.ResetSampling(car.Body.position,race.Clock);
 for(int i=0;i<25;i++){car.Simulate(0,0,0,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();}car.Body.linearVelocity=f*22;int encounters=life.Encounters,sounds=life.Sounds;mix.Begin(5);source.Begin(5);float began=Time.time;bool engines=false,music=false;float closest=1000;int liveFrames=0;
 while(Time.time-began<4){InputSystem.QueueStateEvent(pad,new GamepadState{rightTrigger=.7f});float s=b.Project(car.Body.position,out _);var target=b.At(s+7,out _);var delta=Quaternion.Inverse(Quaternion.LookRotation(Vector3.ProjectOnPlane(car.transform.forward,Vector3.up)))*(target-car.Body.position);float steer=Mathf.Clamp(Mathf.Atan(2*car.wheelbase*Mathf.Sin(Mathf.Atan2(delta.x,delta.z))/7)/(Mathf.Lerp(car.slowSteerAngle,car.fastSteerAngle,Mathf.Clamp01(car.ForwardSpeed/car.topSpeed))*Mathf.Deg2Rad),-1,1);car.Simulate(car.ForwardSpeed<22?1:0,car.ForwardSpeed>23?.3f:0,steer,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();if(voice.isPlaying){liveFrames++;closest=Mathf.Min(closest,Vector3.Distance(listener.transform.position,voice.transform.position));engines|=car.GetComponents<AudioSource>().Any(a=>a.isPlaying&&a.clip&&a.clip.name.Contains("Engine")&&a.volume>.01f);music|=race.Flow.Radio.Playing;}}
 bool pass=life.Encounters==encounters+1&&life.Sounds==sounds+1&&engines&&music&&liveFrames>0&&AudioListener.volume>.7f;File.AppendAllText(D+"/results.txt",$"{(pass?"PASS":"FAIL")} {profile}: event={life.Encounters-encounters} sound={life.Sounds-sounds} engine={engines} radio={music} activeFrames={liveFrames} nearestListener={closest:F2}m master={AudioListener.volume} vehicle={settings.vehicle} music={settings.music} ambience={settings.ambience}; mix {mix.Finish(D+"/"+profile+"-mix.wav")}; rat source {source.Finish(D+"/"+profile+"-rats.wav")}\n");car.Body.isKinematic=true;
 }
 InputSystem.RemoveDevice(pad);if(mute)mute.enabled=true;settings.master=0;race.Flow.Save.ApplySettings();race.Flow.Pause();File.WriteAllText(D+"/done.txt","Two normal-audio local approaches complete. Listener DSP captured; human audibility remains Dan's review.");
 }
}}


