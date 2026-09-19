using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
namespace Racer
{
    // Opt-in standalone tests against generated files and an isolated save only.
    public sealed class CollectionValidation:MonoBehaviour
    {
        readonly List<string> checks=new();
        string dir,root;
        LocalRadio radio;
        void Check(bool ok,string label){checks.Add((ok?"PASS ":"FAIL ")+label);File.WriteAllLines(dir+"/checks.txt",checks);}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot(){var args=Environment.GetCommandLineArgs();if(args.Contains("-collectionTest")&&args.Contains("-racerTestSave"))new GameObject("Collection validation").AddComponent<CollectionValidation>();}
        IEnumerator ScanWait(){float until=Time.realtimeSinceStartup+35;while(radio.Scanning&&Time.realtimeSinceStartup<until)yield return null;Check(!radio.Scanning,"Scan worker completed");}
        static void Tone(string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));using var w=new BinaryWriter(File.Create(path));int samples=44100*8;
            w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));w.Write(36+samples*2);w.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));w.Write(16);w.Write((short)1);w.Write((short)1);w.Write(44100);w.Write(88200);w.Write((short)2);w.Write((short)16);w.Write(System.Text.Encoding.ASCII.GetBytes("data"));w.Write(samples*2);
            for(int i=0;i<samples;i++)w.Write((short)(Math.Sin(i*2*Math.PI*330/44100)*3000));
        }
        IEnumerator Start()
        {
            yield return null;Application.runInBackground=true;
            dir=Path.GetFullPath("Docs/CR050-053/collection-"+(Environment.GetCommandLineArgs().Contains("-portableOnly")?"portable":"standalone"));Directory.CreateDirectory(dir);
            var flow=FindAnyObjectByType<RaceFlow>();radio=flow.Radio;
            if(Environment.GetCommandLineArgs().Contains("-portableOnly"))
            {
                radio.SetSource(true);yield return ScanWait();Check(radio.Folder==LocalRadio.BundledFolder,"Bundled resolved beside running installation: "+radio.Folder);Check(radio.Count>=2,"Nested bundled tracks discovered after extraction");
                yield return Playback();File.WriteAllText(dir+"/done.txt",$"{checks.Count(c=>c.StartsWith("PASS"))}/{checks.Count}");Application.Quit();yield break;
            }
            root=Path.GetFullPath("Temp/collection-fixture-"+Guid.NewGuid().ToString("N"));
            var generate=Task.Run(()=>{Tone(root+"/Artists/One/Album A/one.wav");Tone(root+"/Artists/One/Album B/two.wav");Tone(root+"/Artists/Two/Album C/three.wav");Tone(root+"/flat/four.wav");File.WriteAllText(root+"/Artists/notes.txt","unsupported fixture");});
            while(!generate.IsCompleted)yield return null;Check(generate.IsCompletedSuccessfully,"Generated nested fixtures");
            radio.SetRecursive(true);radio.SetFolder(root+"/Artists");yield return ScanWait();Check(radio.Count==3,"Collection root includes all artists/albums");yield return Playback();
            radio.SetFolder(root+"/Artists/One");yield return ScanWait();Check(radio.Count==2,"Artist root includes both albums");
            radio.SetRecursive(false);yield return ScanWait();Check(radio.Count==0,"Recursion Off excludes album subfolders");
            radio.SetRecursive(true);yield return ScanWait();Check(radio.Count==2,"Recursion On rediscovers albums");
            radio.SetFolder(root+"/flat");yield return ScanWait();Check(radio.Count==1,"Flat folder supported");yield return Playback();
            File.Copy(root+"/flat/four.wav",root+"/flat/added.wav");radio.Rescan();yield return ScanWait();Check(radio.Count==2,"Rescan discovers addition");
            File.Delete(root+"/flat/four.wav");radio.Rescan();yield return ScanWait();Check(radio.Count==1,"Rescan removes deleted path");
            var fresh=new RacerSave(flow.Save.DirectoryPath,"test");Check(fresh.Settings.musicRecursive&&fresh.Settings.musicFolder==radio.Folder&&fresh.Settings.musicSource=="custom","Root, recursion and source persist through new save instance");
            string large=root+"/large";generate=Task.Run(()=>{for(int i=0;i<12000;i++){string p=large+"/Artist "+(i/100)+"/Album/"+i+".wav";Directory.CreateDirectory(Path.GetDirectoryName(p));File.WriteAllBytes(p,new byte[]{82,73,70,70});}});
            while(!generate.IsCompleted)yield return null;Check(generate.IsCompletedSuccessfully,"Generated 12,000-path synthetic library");
            // Keep playback on a valid tone while scanning; the large files test discovery,
            // not 12,000 audio decodes. Drive using the unchanged production AI pilot.
            flow.Race.opponents=false;flow.Race.traffic=true;flow.StartRace();while(flow.State!=RaceFlow.Stage.Racing)yield return null;
            var pilot=flow.Race.vehicle.gameObject.AddComponent<RoadDriver>();pilot.Initialize(flow.Race,flow.Race.vehicle,true,1,1);pilot.Racer=flow.Race.Racers[0];
            var frames=new List<float>();var progress=new MusicCollection.Progress();var largeScan=Task.Run(()=>MusicCollection.Scan(large,true,CancellationToken.None,progress));
            while(!largeScan.IsCompleted){frames.Add(Time.unscaledDeltaTime*1000);yield return null;}
            Check(largeScan.Result.Paths.Count==12000&&!largeScan.Result.Limited,"12,000 tracks found without old 2,048 truncation");
            File.WriteAllText(dir+"/scan-frames.txt",$"12,000-file discovery while standalone driving/playback: frames={frames.Count}; max={(frames.Count>0?frames.Max():0):F2}ms; mean={(frames.Count>0?frames.Average():0):F2}ms. Background window measurements, not subjective hitch acceptance.");
            if(flow.Save.Settings.radioOn)radio.Toggle();
            frames.Clear();radio.SetFolder(large);
            while(radio.Scanning){frames.Add(Time.unscaledDeltaTime*1000);yield return null;}
            Check(radio.Count==12000,"Full radio Rescan installs all 12,000 paths while driving");
            var timer=System.Diagnostics.Stopwatch.StartNew();radio.Next();timer.Stop();
            File.AppendAllText(dir+"/scan-frames.txt",$"\nFull radio large-root scan/install frames={frames.Count}; max={(frames.Count>0?frames.Max():0):F2}ms; first full-collection shuffle request={timer.Elapsed.TotalMilliseconds:F2}ms. Synthetic path files are deliberately not playable audio.");
            radio.SetFolder(root+"/Artists");yield return ScanWait();if(!flow.Save.Settings.radioOn)radio.Toggle();yield return Playback();
            var cancel=new CancellationTokenSource();cancel.Cancel();var cancelled=MusicCollection.Scan(large,true,cancel.Token);Check(cancelled.Cancelled&&cancelled.Paths.Count==0,"Cancelled scan terminates without discovery");
            radio.SetFolder(large);radio.SetFolder(root+"/Artists");yield return ScanWait();Check(radio.Count==3&&radio.Folder.EndsWith("Artists"),"Changing root cancels old scan; no stale result installation");
            radio.Rescan();radio.CancelScan();yield return ScanWait();Check(radio.Count==3&&radio.ScanStatus.Contains("cancelled"),"Cancel retains previous collection");
            var library=(List<string>)typeof(LocalRadio).GetField("library",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(radio);
            var current=typeof(LocalRadio).GetField("current",BindingFlags.NonPublic|BindingFlags.Instance);
            var played=new HashSet<string>{(string)current.GetValue(radio)};for(int i=0;i<2;i++){radio.Next();yield return Playback();played.Add((string)current.GetValue(radio));}
            Check(played.Count==3&&played.Any(p=>p.Contains("Two")),"Shuffle bag includes all artists without replacement");
            radio.SetFolder(root+"/missing");yield return ScanWait();Check(radio.Count==0&&radio.ScanStatus.Contains("inaccessible"),"Missing folder reports scan counts");
            Directory.CreateDirectory(root+"/empty");radio.SetFolder(root+"/empty");yield return ScanWait();Check(radio.Count==0&&!radio.Playing,"Empty collection works");
            radio.SetSource(true);yield return ScanWait();Check(radio.Folder==LocalRadio.BundledFolder,"Bundled path uses installation, not current directory");
            Destroy(pilot);flow.Pause();flow.OpenSettings();var menus=flow.GetComponent<RaceMenus>();
            typeof(RaceMenus).GetField("musicPage",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(menus,true);typeof(RaceMenus).GetField("musicCollectionPage",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(menus,true);menus.Show();
            yield return new WaitForEndOfFrame();CombinedReviewValidation.Capture(dir+"/collection-settings.png");
            var ambient=FindObjectsByType<AmbientVehicle>();Check(ambient.Select(a=>a.BodyType).Distinct().Count()==4,"Standalone traffic has four body types");Check(ambient.Select(a=>a.PaintIndex).Distinct().Count()>=4,"Standalone traffic has varied paints");
            Check(flow.Race.Drivers.Where(d=>d.HighwayTraffic).Count()==16&&flow.Race.Drivers.Count==20,"Traffic density retained (16 highway + 4 local)");
            Check(ambient.Where(a=>a.GetComponent<BoxCollider>()).All(a=>a.GetComponent<BoxCollider>().size.x<=2.1f),"All traffic collision widths fit local and highway lanes");
            File.WriteAllText(dir+"/done.txt",$"{checks.Count(c=>c.StartsWith("PASS"))}/{checks.Count}");Application.Quit();
        }
        IEnumerator Playback()
        {
            if(!radio.Playing&&!radio.Loading)radio.Next();float until=Time.realtimeSinceStartup+15;while((radio.Loading||!radio.Playing)&&Time.realtimeSinceStartup<until)yield return null;
            var source=(AudioSource)typeof(LocalRadio).GetField("source",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(radio);var samples=new float[2048];source.GetOutputData(samples,0);yield return new WaitForSecondsRealtime(.2f);source.GetOutputData(samples,0);
            Check(radio.Playing&&samples.Max(Mathf.Abs)>.00001f,"Nested/flat WAV decoded DSP output (not subjective listening)");
        }
    }
}
