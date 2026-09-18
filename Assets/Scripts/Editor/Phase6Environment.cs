using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object=UnityEngine.Object;

namespace Racer.Editor {
// Focused surface attributes only: never changes terrain vertices, colliders, buildings or yards.
public static class Phase6Environment {
 public const string Dir="Docs/Phase6Environment", Folder="Assets/Environment/Phase6";
 static float Smooth(float a,float b,float value)=>Mathf.SmoothStep(0,1,Mathf.InverseLerp(a,b,value));
 static void Guard(){if(Application.isPlaying||SceneManager.GetActiveScene().path!=StreetLoopBuilder.ScenePath||SceneManager.GetActiveScene().isDirty)throw new Exception("Open saved StreetLoopGreybox in edit mode");}
 [MenuItem("Racer/Phase 6/Apply remaining environment")]
 public static void Apply(){
  Guard();if(Object.FindAnyObjectByType<WoodlandAmbience>())throw new Exception("Environment batch already applied; use focused edits, not cumulative recoloring.");Directory.CreateDirectory(Folder);Directory.CreateDirectory(Dir);AssetDatabase.Refresh();
  var route=StreetLoopBuilder.Route();var arcs=new float[route.Count+1];for(int i=1;i<arcs.Length;i++)arcs[i]=arcs[i-1]+Vector3.Distance(route[i-1],route[i%route.Count]);
  var shops=GameObject.Find(Phase6Review.Root).transform.Cast<Transform>().Where(t=>t.name.Contains("business")&&t.position.x>-260&&t.position.x<80).ToArray();
  var mat=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Marked ground.mat");if(!mat){mat=new Material(Shader.Find("Racer/MarkedGround"));AssetDatabase.CreateAsset(mat,Folder+"/Marked ground.mat");}
  int tiles=0,colors=0,walkVertices=0;
  foreach(var f in GameObject.Find("Memory loop - north is +Z").GetComponentsInChildren<MeshFilter>()){
   var mesh=f.sharedMesh;var box=mesh.bounds;box.Expand(35);
   var segments=Enumerable.Range(0,route.Count).Where(i=>box.Contains(route[i])||box.Contains(route[(i+1)%route.Count])).ToArray();
   bool shopTile=shops.Any(t=>box.Contains(t.position));if(segments.Length==0&&!shopTile)continue;
   var vertices=mesh.vertices;var uv=new List<Vector4>(vertices.Length);var cs=mesh.colors;bool paint=false;
   for(int j=0;j<vertices.Length;j++){
    var p=vertices[j];float best=float.MaxValue,side=100,arc=0;Vector3 near=default;
    foreach(int i in segments){var a=route[i];var delta=route[(i+1)%route.Count]-a;var flat=new Vector3(delta.x,0,delta.z);float t=Mathf.Clamp01(Vector3.Dot(p-a,flat)/flat.sqrMagnitude);var q=a+delta*t;float d=(new Vector2(p.x-q.x,p.z-q.z)).sqrMagnitude;if(d>=best)continue;best=d;near=q;side=Vector3.Dot(p-q,Vector3.Cross(Vector3.up,flat).normalized);arc=Mathf.Lerp(arcs[i],arcs[i+1],t);}
    bool main=near.z>505&&near.x>-560&&near.x<240;bool neighborhood=near.x>285&&near.z> -340;
    uv.Add(new Vector4(side,arc,main||neighborhood?1:0,main?1:0));
    if(best<100&&(main||neighborhood))paint=true;
    // A few commercial frontages only. Flush pedestrian paving and feathered gravel joins.
    if(best<100)continue;
    foreach(var shop in shops){var foundation=shop.Find("Foundation");var local=shop.InverseTransformPoint(p);float half=foundation.localScale.x*.5f,front=foundation.localScale.z*.5f;
     float dx=Mathf.Max(0,Mathf.Abs(local.x)-half),dz=local.z-front;
     if(dx>3||dz<-.4f||dz>9)continue;
     float blend=(1-Smooth(0,3,dx))*(1-Smooth(5,9,dz));
     Color gravel=new(.40f,.40f,.34f);float paving=(1-Smooth(half-1,half+1,Mathf.Abs(local.x)))*Smooth(.3f,1.2f,dz)*(1-Smooth(3.3f,4.2f,dz));
     cs[j]=Color.Lerp(cs[j],Color.Lerp(gravel,new Color(.54f,.53f,.46f),paving),blend);colors++;if(paving>.5f)walkVertices++;
    }
   }
   if(paint){mesh.SetUVs(1,uv);f.GetComponent<Renderer>().sharedMaterial=mat;tiles++;}
   mesh.colors=cs;EditorUtility.SetDirty(mesh);
  }
  var sun=GameObject.Find("Sun").GetComponent<Light>();sun.intensity=1.25f;sun.color=new Color(1,.97f,.90f);sun.transform.rotation=Quaternion.Euler(58,-28,0);
  RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.64f,.70f,.78f);RenderSettings.ambientEquatorColor=new Color(.49f,.53f,.48f);RenderSettings.ambientGroundColor=new Color(.30f,.32f,.27f);
  // Permanently remove the already-disabled SSAO reference: identical Editor/player configuration.
  var renderer=AssetDatabase.LoadAssetAtPath<UniversalRendererData>("Assets/Settings/PC_Renderer.asset");renderer.rendererFeatures.RemoveAll(f=>f&&!f.isActive);EditorUtility.SetDirty(renderer);
  GenerateAudio();
  var ambience=Object.FindAnyObjectByType<WoodlandAmbience>();if(!ambience)ambience=new GameObject("Wooded neighborhood ambience").AddComponent<WoodlandAmbience>();
  ambience.wind=AssetDatabase.LoadAssetAtPath<AudioClip>(Folder+"/Soft leaves.wav");ambience.birds=Enumerable.Range(0,3).Select(i=>AssetDatabase.LoadAssetAtPath<AudioClip>(Folder+"/Bird "+i+".wav")).ToArray();
  EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());EditorSceneManager.SaveScene(SceneManager.GetActiveScene());AssetDatabase.SaveAssets();
  File.WriteAllText(Dir+"/implementation.txt",$"Marked terrain tiles={tiles}; commercial sites={shops.Length}; transition color entries={colors}; paving core vertices={walkVertices}. Zero new mesh renderers, lights or colliders. Terrain positions/normals/indices unchanged. Flush paving is surface color on collision terrain; no raised curb.\nSingle sun 1.7 -> 1.25, warm neutral, pitch 50 -> 58, yaw -35 -> -28. Flat ambient -> tri-light. Ground/forest now respond to ambient/main-light color. No shadow sampling or new postprocessing in custom shader. Disabled SSAO reference permanently removed from PC renderer; no build-only feature edits.\n");
 }
 static void Wave(string name,float[] samples,int rate){
  string path=Folder+"/"+name+".wav";using(var w=new BinaryWriter(File.Create(path))){w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));w.Write(36+samples.Length*2);w.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));w.Write(16);w.Write((short)1);w.Write((short)1);w.Write(rate);w.Write(rate*2);w.Write((short)2);w.Write((short)16);w.Write(System.Text.Encoding.ASCII.GetBytes("data"));w.Write(samples.Length*2);foreach(float x in samples)w.Write((short)(Mathf.Clamp(x,-1,1)*32767));}
  AssetDatabase.ImportAsset(path);var importer=(AudioImporter)AssetImporter.GetAtPath(path);var settings=importer.defaultSampleSettings;settings.loadType=AudioClipLoadType.DecompressOnLoad;settings.compressionFormat=AudioCompressionFormat.PCM;settings.sampleRateSetting=AudioSampleRateSetting.PreserveSampleRate;importer.defaultSampleSettings=settings;importer.forceToMono=true;importer.SaveAndReimport();
 }
 public static void GenerateAudio(){
  const int rate=22050;var rng=new System.Random(6018);var wind=new float[rate*32];float low=0,slow=0;
  for(int i=0;i<wind.Length;i++){float n=(float)rng.NextDouble()*2-1;low+=.16f*(n-low);slow+=.008f*(n-slow);float t=i/(float)rate;wind[i]=(low-slow)*.6f*(.65f+.2f*Mathf.Sin(t*Mathf.PI/8)+.15f*Mathf.Sin(t*Mathf.PI/4));}
  // Overlap the tail with the first second, then omit the overlapped tail.
  // End and start now remain adjacent filtered-noise samples without an artificial jump.
  for(int i=0;i<rate;i++){float t=i/(float)(rate-1);wind[i]=Mathf.Lerp(wind[wind.Length-rate+i],wind[i],t);}Wave("Soft leaves",wind.Take(wind.Length-rate).ToArray(),rate);
  for(int k=0;k<3;k++){var data=new float[(int)(rate*1.7f)];double phase=0;for(int i=0;i<data.Length;i++){float t=i/(float)rate;float pulse=t%(k==1?.43f:.54f);float dur=.16f+k*.035f;float envelope=pulse<dur?Mathf.Pow(Mathf.Sin(Mathf.PI*pulse/dur),2):0;float hz=1800+k*360+850*Mathf.Sin(pulse/dur*Mathf.PI);phase+=2*Math.PI*hz/rate;data[i]=.32f*envelope*(float)(Math.Sin(phase)+.12*Math.Sin(phase*2))*Mathf.Clamp01((1.65f-t)*5);}Wave("Bird "+k,data,rate);}
  File.WriteAllText(Folder+"/AUDIO_SOURCES.md","# Audio sources\nOriginal deterministic procedural synthesis created for Racer; no recordings, downloads, third-party samples or paid assets. Generated by Phase6Environment.GenerateAudio (seed 6018). These generated WAV assets are dedicated under CC0-1.0. Wind: filtered noise, 31 s mono PCM 22050 Hz, crossfaded loop. Birds: three 1.7 s enveloped frequency sweeps, scheduled 12–26 s apart with pitch variation. Two runtime sources; mono PCM files total about 1.52 MiB. Runtime decoder memory was not separately profiled. No music or vehicle audio changes.\n");AssetDatabase.Refresh();
 }
 public static void Build(){
  Guard();Directory.CreateDirectory("Builds/Phase6Environment");
  var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{StreetLoopBuilder.ScenePath},locationPathName="Builds/Phase6Environment/Racer.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
  File.WriteAllText(Dir+"/build.txt",$"{report.summary.result}; errors={report.summary.totalErrors}; warnings={report.summary.totalWarnings}; bytes={report.summary.totalSize}; duration={report.summary.totalTime}\n"+string.Join("\n",report.steps.SelectMany(s=>s.messages).Where(m=>m.type==LogType.Error||m.type==LogType.Warning).Select(m=>m.content)));
 }
}
}
