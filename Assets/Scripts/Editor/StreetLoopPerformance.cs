using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace Racer.Editor {
public static class StreetLoopPerformance {
 static readonly List<double> Samples=new();static string label;static double last;static int frames,skip;static int lastFrame;static bool active;
 public static void Start(string name,bool forest){if(active)throw new InvalidOperationException("Profile still running");if(!Application.isPlaying)throw new InvalidOperationException("Play first");GameObject.Find("Woods replacing later subdivisions").SetActive(forest);label=name;Samples.Clear();frames=0;skip=30;lastFrame=Time.frameCount;last=EditorApplication.timeSinceStartup;active=true;EditorApplication.update+=Tick;}
 static void Tick(){if(Time.frameCount==lastFrame)return;lastFrame=Time.frameCount;double now=EditorApplication.timeSinceStartup;double dt=(now-last)*1000;last=now;if(skip-->0)return;Samples.Add(dt);if(++frames<180)return;EditorApplication.update-=Tick;active=false;Samples.Sort();File.AppendAllText("Docs/PHASE2_REVISION_PERFORMANCE.txt",$"{label}: 180 live Play frames, editor interval median {Samples[90]:F2}ms, p95 {Samples[171]:F2}ms. Resolution {Screen.width}x{Screen.height}; vSync {QualitySettings.vSyncCount}; target {Application.targetFrameRate}.\n");}
 public static void RestoreForest(){foreach(var t in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())if(t.name=="Woods replacing later subdivisions")t.SetActive(true);}
}
}
