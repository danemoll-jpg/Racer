#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace Racer {
// 0.97 Part C: the offline recorder behind the trailer capture mode (editor only, never in a release build). While a clip is being
// recorded the game runs on a fixed time step (Time.captureFramerate, 60 frames a second): every frame is rendered at full quality
// whatever the GPU's real-time speed, the main camera draws into a RenderTexture (3840x2160 when that renders, else 2560x1440), each
// frame goes to ffmpeg as raw RGB (H.264, high quality) and the game's mixed sound (AudioRenderer: engine, rain, thunder; the radio is
// off) goes to a PCM file that is muxed in when the clip closes. Slow motion is Time.timeScale below 1 on this fixed step: more frames
// of real slow motion, never stretched.
public sealed class TrailerRecorder {
 public int Width, Height, Fps = 60; public string Ffmpeg; public bool Audio = true;
 public long Frames { get; private set; } public string Clip { get; private set; } public double EncodeSeconds { get; private set; }
 RenderTexture rt; Texture2D tex; Process ff; Stream pipe; FileStream pcm; Camera cam; RenderTexture previous; string videoPath, pcmPath; int channels, rate;
 bool audioOn; float oldCapture; bool recording; readonly Stopwatch watch = new();
 public bool Recording => recording;
 public Camera Camera => cam;
 static string Q(string p) => "\"" + p + "\"";
 public bool Begin(Camera camera, string clipPath, int width, int height) {
  Width = width; Height = height; cam = camera; Clip = clipPath; Directory.CreateDirectory(Path.GetDirectoryName(clipPath));
  videoPath = Path.ChangeExtension(clipPath, ".video.mp4"); pcmPath = Path.ChangeExtension(clipPath, ".pcm");
  try {
   rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB) { antiAliasing = 1, name = "Trailer capture" }; rt.Create();
  } catch (Exception e) { UnityEngine.Debug.LogWarning("Trailer RT failed: " + e.Message); return false; }
  if (!rt.IsCreated()) return false;
  tex = new Texture2D(width, height, TextureFormat.RGB24, false);
  // frames go in bottom-up from the render texture: the vertical flip is ffmpeg's. veryfast + crf 12 keeps the clip near lossless; the cut re-encodes once.
  var psi = new ProcessStartInfo(Ffmpeg, $"-y -hide_banner -loglevel error -f rawvideo -pix_fmt rgb24 -s {width}x{height} -r {Fps} -i - -vf vflip -c:v libx264 -preset veryfast -crf 12 -pix_fmt yuv420p {Q(videoPath)}") { UseShellExecute = false, RedirectStandardInput = true, CreateNoWindow = true, RedirectStandardError = false };
  ff = Process.Start(psi); pipe = ff.StandardInput.BaseStream;
  oldCapture = Time.captureFramerate; Time.captureFramerate = Fps;
  previous = cam.targetTexture; cam.targetTexture = rt;
  audioOn = false;
  if (Audio) { try { AudioRenderer.Start(); audioOn = true; rate = AudioSettings.outputSampleRate; channels = AudioSettings.speakerMode == AudioSpeakerMode.Mono ? 1 : 2; pcm = new FileStream(pcmPath, FileMode.Create); } catch (Exception e) { UnityEngine.Debug.LogWarning("AudioRenderer: " + e.Message); audioOn = false; } }
  Frames = 0; recording = true; watch.Restart(); return true;
 }
 // call after the frame has rendered (WaitForEndOfFrame)
 public void Grab() {
  if (!recording) return;
  var old = RenderTexture.active; RenderTexture.active = rt; tex.ReadPixels(new Rect(0, 0, Width, Height), 0, 0, false); tex.Apply(false); RenderTexture.active = old;
  var raw = tex.GetRawTextureData<byte>(); pipe.Write(raw.ToArray(), 0, raw.Length);
  if (audioOn) {
   int n = AudioRenderer.GetSampleCountForCaptureFrame(); using var buf = new NativeArray<float>(n * channels, Allocator.Temp);
   if (AudioRenderer.Render(buf)) { var bytes = new byte[buf.Length * 4]; Buffer.BlockCopy(buf.ToArray(), 0, bytes, 0, bytes.Length); pcm.Write(bytes, 0, bytes.Length); }
  }
  Frames++;
 }
 public void SaveStill(string path) { var old = RenderTexture.active; RenderTexture.active = rt; var t = new Texture2D(Width, Height, TextureFormat.RGB24, false); t.ReadPixels(new Rect(0, 0, Width, Height), 0, 0, false); t.Apply(false); RenderTexture.active = old; File.WriteAllBytes(path, t.EncodeToPNG()); UnityEngine.Object.Destroy(t); }
 public string End() {
  if (!recording) return Clip; recording = false;
  cam.targetTexture = previous; Time.captureFramerate = (int)oldCapture;
  if (audioOn) { AudioRenderer.Stop(); pcm.Flush(); pcm.Dispose(); }
  pipe.Flush(); pipe.Close(); ff.WaitForExit(120000); EncodeSeconds = watch.Elapsed.TotalSeconds;
  UnityEngine.Object.Destroy(rt); UnityEngine.Object.Destroy(tex);
  string result = Clip;
  if (audioOn && File.Exists(pcmPath) && new FileInfo(pcmPath).Length > 0) {
   var mux = new ProcessStartInfo(Ffmpeg, $"-y -hide_banner -loglevel error -i {Q(videoPath)} -f f32le -ar {rate} -ac {channels} -i {Q(pcmPath)} -c:v copy -c:a aac -b:a 256k -shortest {Q(Clip)}") { UseShellExecute = false, CreateNoWindow = true };
   var m = Process.Start(mux); m.WaitForExit(120000);
  } else if (File.Exists(videoPath)) { File.Copy(videoPath, Clip, true); }
  if (File.Exists(Clip)) { try { File.Delete(videoPath); } catch { } }
  return result;
 }
 // peak and mean level of the clip's recorded sound (a check that the sound really is in the file)
 public static (float peak, float rms) PcmLevel(string pcmFile) {
  if (!File.Exists(pcmFile)) return (0, 0); var b = File.ReadAllBytes(pcmFile); int n = b.Length / 4; if (n == 0) return (0, 0); var f = new float[n]; Buffer.BlockCopy(b, 0, f, 0, n * 4);
  float peak = 0; double sum = 0; foreach (var x in f) { peak = Mathf.Max(peak, Mathf.Abs(x)); sum += x * x; } return (peak, (float)Math.Sqrt(sum / n));
 }
}
}
#endif
