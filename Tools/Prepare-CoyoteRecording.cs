using System;using System.IO;using System.Linq;using UnityEngine;using UnityEditor;
public static class PrepareCoyoteRecording {
 public static string Main(){
 const string source="Assets/Audio/Wildlife/Coyotes-NPS-Mojave-source.mp3";
 var importer=(AudioImporter)AssetImporter.GetAtPath(source);var settings=importer.defaultSampleSettings;settings.loadType=AudioClipLoadType.DecompressOnLoad;settings.compressionFormat=AudioCompressionFormat.PCM;importer.defaultSampleSettings=settings;importer.forceToMono=true;importer.SaveAndReimport();
 var clip=AssetDatabase.LoadAssetAtPath<AudioClip>(source);clip.LoadAudioData();var data=new float[clip.samples*clip.channels];if(!clip.GetData(data,0))throw new Exception("Cannot decode NPS source");
 int rate=clip.frequency;int n=rate*3;var used=new System.Collections.Generic.List<int>();var rows=new System.Collections.Generic.List<string>();
 for(int v=1;v<=2;v++){
 int best=-1;double bestEnergy=-1;
 for(int start=rate;start+n<data.Length-rate;start+=rate/4){if(used.Any(p=>Math.Abs(p-start)<n+rate))continue;double energy=0;for(int i=start;i<start+n;i++)energy+=data[i]*data[i];if(energy>bestEnergy){bestEnergy=energy;best=start;}}
 if(best<0)throw new Exception("No distinct excerpt available");used.Add(best);var x=data.Skip(best).Take(n).ToArray();float peak=x.Max(t=>Math.Abs(t));float gain=.68f/Math.Max(.001f,peak);
 using(var w=new BinaryWriter(File.Create($"Assets/Audio/Wildlife/Coyote-original-{v}.wav"))){w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));w.Write(36+n*2);w.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));w.Write(16);w.Write((short)1);w.Write((short)1);w.Write(rate);w.Write(rate*2);w.Write((short)2);w.Write((short)16);w.Write(System.Text.Encoding.ASCII.GetBytes("data"));w.Write(n*2);for(int i=0;i<n;i++){float fade=Math.Min(1,Math.Min(i/(rate*.08f),(n-1-i)/(rate*.16f)));w.Write((short)(Mathf.Clamp(x[i]*gain*fade,-1,1)*32767));}}
 rows.Add($"Variant {v}: source offset {best/(float)rate:F2}s; duration 3s; mono {rate}Hz PCM; peak <=0.68; gain={gain:F3}; 80/160ms boundary fades; original pitch, no reverb/spectral synthesis.");
 }
 File.WriteAllLines("Docs/FiveUpdates/coyote-mastering.txt",rows);AssetDatabase.Refresh();return string.Join("\n",rows);
 }
}
