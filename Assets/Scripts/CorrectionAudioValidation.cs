using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Racer
{
    // Captures the listener DSP mix, not just an AudioSource call. Does not claim OS-device listening.
    public sealed class CorrectionAudioCapture : MonoBehaviour
    {
        readonly object gate=new(); float[] buffer; int used,channels,rate;
        public void Begin(float seconds){lock(gate){rate=AudioSettings.outputSampleRate;buffer=new float[(int)(seconds*rate)*8];used=0;}}
        void OnAudioFilterRead(float[] data,int count){lock(gate){if(buffer==null)return;channels=count;int n=Math.Min(data.Length,buffer.Length-used);Array.Copy(data,0,buffer,used,n);used+=n;}}
        public string Finish(string path)
        {
            lock(gate)
            {
                float peak=0;double sum=0;int clipping=0;
                using(var w=new BinaryWriter(File.Create(path)))
                {
                    w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));w.Write(36+used*2);w.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));w.Write(16);w.Write((short)1);w.Write((short)Math.Max(1,channels));w.Write(rate);w.Write(rate*Math.Max(1,channels)*2);w.Write((short)(Math.Max(1,channels)*2));w.Write((short)16);w.Write(System.Text.Encoding.ASCII.GetBytes("data"));w.Write(used*2);
                    for(int i=0;i<used;i++){peak=Mathf.Max(peak,Mathf.Abs(buffer[i]));sum+=buffer[i]*buffer[i];if(Mathf.Abs(buffer[i])>=1)clipping++;w.Write((short)(Mathf.Clamp(buffer[i],-1,1)*32767));}
                }
                string result=$"samples={used}, peak={peak:F6}, rms={Math.Sqrt(sum/Math.Max(1,used)):F6}, clipped={clipping}";buffer=null;return result;
            }
        }
    }
    public sealed class CorrectionAudioValidation : MonoBehaviour
    {
        const string Dir="Docs/CR041-045/audio-final";
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot(){var args=Environment.GetCommandLineArgs();if(args.Contains("-correctionAudio")&&args.Contains("-racerTestSave")){Application.runInBackground=true;new GameObject("Physical break and DSP validation").AddComponent<CorrectionAudioValidation>();}}
        IEnumerator Start()
        {
            yield return null;Directory.CreateDirectory(Dir);var race=FindAnyObjectByType<RaceDirector>();race.opponents=race.traffic=false;race.Flow.StartRace();while(race.Flow.State!=RaceFlow.Stage.Racing)yield return null;
            var listener=FindAnyObjectByType<AudioListener>();var capture=listener.gameObject.AddComponent<CorrectionAudioCapture>();
            File.WriteAllText(Dir+"/results.txt",$"Listener enabled={listener.enabled}; rate={AudioSettings.outputSampleRate}; DSP mix capture before OS endpoint, no subjective listening claimed.\n");
            var car=race.vehicle;var pad=InputSystem.AddDevice<Gamepad>();
            var poses=FindObjectsByType<BreakableProp>().ToDictionary(p=>p,p=>(center:p.GetComponent<BoxCollider>().bounds.center,forward:p.transform.forward));
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
            car.GetComponent<VehicleInput>().enabled=true;car.enabled=true;
            foreach(float master in new[]{.8f,.25f,0f})
            foreach(SmashAudio.Surface material in Enum.GetValues(typeof(SmashAudio.Surface)))
            {
                race.Flow.Save.Settings.master=master;race.Flow.Save.ApplySettings(); // Isolated save, never persisted.
                yield return new WaitForSeconds(.15f); // Allow the previous DSP block to drain before measuring mute.
                var candidates=FindObjectsByType<BreakableProp>().Where(p=>p.surface==material);
                if(material==SmashAudio.Surface.Glass)candidates=candidates.Where(p=>p.name=="Breakable sliding glass doors");
                if(material==SmashAudio.Surface.Wood)candidates=candidates.Where(p=>p.name=="Property white X");
                if(material==SmashAudio.Surface.ChainLink)candidates=candidates.Where(p=>p.name=="Property chain-link");
                var prop=candidates.OrderBy(p=>p.transform.position.z).First();
                var f=Vector3.ProjectOnPlane(poses[prop].forward,Vector3.up).normalized;var spawn=poses[prop].center-f*10;
                if(Physics.Raycast(spawn+Vector3.up*5,Vector3.down,out var support,40,1,QueryTriggerInteraction.Ignore))spawn.y=support.point.y+.7f;
                car.Body.isKinematic=false;car.Body.position=spawn;car.Body.rotation=Quaternion.LookRotation(f);car.transform.SetPositionAndRotation(car.Body.position,car.Body.rotation);car.Body.linearVelocity=f*18;car.Body.angularVelocity=Vector3.zero;Physics.SyncTransforms();race.ResetSampling(car.Body.position,Time.timeAsDouble);BreakableProp.RestoreRace();FindAnyObjectByType<ChaseCamera>().Snap();
                int before=FindAnyObjectByType<SmashAudio>().Events;capture.Begin(4);
                float until=Time.time+2.5f;
                while(Time.time<until){InputSystem.QueueStateEvent(pad,new GamepadState{rightTrigger=.75f});yield return null;}
                string file=Dir+"/"+material+"-master-"+master.ToString("F2",System.Globalization.CultureInfo.InvariantCulture)+".wav";
                File.AppendAllText(Dir+"/results.txt",$"{material} prop={prop.name} master={master} broken={prop.IsBroken} events={FindAnyObjectByType<SmashAudio>().Events-before} {capture.Finish(file)}\n");
                yield return new WaitForSeconds(.3f);
            }
            InputSystem.RemoveDevice(pad);File.WriteAllText(Dir+"/done.txt","Physical trigger impacts with loaded engine and listener mix captured; no OS endpoint or subjective listening claim.");Application.Quit();
        }
    }
}
