using System;using System.IO;using System.Linq;using System.Text;using System.Collections.Generic;using UnityEditor;using UnityEditorInternal;using UnityEditor.Profiling;using UnityEngine;
// Temporary 0.82 tool (copied into Assets/Editor/Report082Temp only while it runs): reads a HitchBench profiler capture
// (PROFILE_RAW) and lists, per script update method, the total and worst-frame main-thread time and its heaviest children.
public static class Report082Profile {
 public static void Run(){var raw=Environment.GetEnvironmentVariable("PROFILE_RAW");var sb=new StringBuilder();
  try{
  if(!ProfilerDriver.LoadProfile(raw,false))throw new Exception("cannot load "+raw);
  int first=ProfilerDriver.firstFrameIndex,last=ProfilerDriver.lastFrameIndex;sb.AppendLine($"{raw}: frames {first}..{last}");
  var total=new Dictionary<string,float>();var worst=new Dictionary<string,float>();var kids=new Dictionary<string,Dictionary<string,float>>();var frameMs=new List<(int f,float ms,string top)>();
  var list=new List<int>();var sub=new List<int>();var deep=new List<int>();
  for(int f=first;f<=last;f++){
   using var v=ProfilerDriver.GetHierarchyFrameDataView(f,0,HierarchyFrameDataView.ViewModes.MergeSamplesWithTheSameName,HierarchyFrameDataView.columnTotalTime,false);
   if(v==null||!v.valid)continue;
   var stack=new Stack<(int id,int depth)>();stack.Push((v.GetRootItemID(),0));var tops=new List<(string,float)>();
   while(stack.Count>0){var (id,d)=stack.Pop();list.Clear();v.GetItemChildren(id,list);
    foreach(var c in list){var n=v.GetItemName(c);float ms=v.GetItemColumnDataAsFloat(c,HierarchyFrameDataView.columnTotalTime);
     bool script=n.Contains("Update()")||n.Contains("Coroutine")||n.Contains(".Start()")||n.Contains("[Invoke]");
     if(script&&d>=1){total[n]=total.GetValueOrDefault(n)+ms;worst[n]=Mathf.Max(worst.GetValueOrDefault(n),ms);tops.Add((n,ms));
      if(!kids.ContainsKey(n))kids[n]=new();sub.Clear();v.GetItemChildren(c,sub);
      foreach(var k in sub){var kn=v.GetItemName(k);float km=v.GetItemColumnDataAsFloat(k,HierarchyFrameDataView.columnTotalTime);kids[n][kn]=kids[n].GetValueOrDefault(kn)+km;
       deep.Clear();v.GetItemChildren(k,deep);foreach(var g in deep){var gn=kn+" > "+v.GetItemName(g);kids[n][gn]=kids[n].GetValueOrDefault(gn)+v.GetItemColumnDataAsFloat(g,HierarchyFrameDataView.columnTotalTime);}}
      continue;}
     if(d<8)stack.Push((c,d+1));}}
   float frame=v.frameTimeMs;frameMs.Add((f,frame,string.Join(", ",tops.OrderByDescending(t=>t.Item2).Take(3).Select(t=>$"{t.Item1} {t.Item2:F1}"))));}
  sb.AppendLine($"frames read {frameMs.Count}, median {frameMs.Select(x=>x.ms).OrderBy(x=>x).ElementAtOrDefault(frameMs.Count/2):F1} ms");
  foreach(var kv in total.OrderByDescending(k=>k.Value).Take(25)){sb.AppendLine($"{kv.Value,9:F1} ms total, worst frame {worst[kv.Key],7:F1} ms  {kv.Key}");
   foreach(var k in kids[kv.Key].OrderByDescending(x=>x.Value).Take(8))sb.AppendLine($"           {k.Value,9:F1}  {k.Key}");}
  sb.AppendLine("slowest frames:");foreach(var x in frameMs.OrderByDescending(x=>x.ms).Take(15))sb.AppendLine($"  frame {x.f} {x.ms:F1} ms: {x.top}");
  // the heaviest call chain of the 3 slowest frames (deep profiles name every C# call)
  foreach(var x in frameMs.OrderByDescending(x=>x.ms).Take(3)){
   using var v=ProfilerDriver.GetHierarchyFrameDataView(x.f,0,HierarchyFrameDataView.ViewModes.MergeSamplesWithTheSameName,HierarchyFrameDataView.columnTotalTime,false);
   sb.AppendLine($"chain of frame {x.f}:");int id=v.GetRootItemID();
   for(int depth=0;depth<40;depth++){list.Clear();v.GetItemChildren(id,list);if(list.Count==0)break;
    var kidsHere=list.Select(c=>(c,ms:v.GetItemColumnDataAsFloat(c,HierarchyFrameDataView.columnTotalTime),self:v.GetItemColumnDataAsFloat(c,HierarchyFrameDataView.columnSelfTime))).OrderByDescending(k=>k.ms).ToList();
    var top=kidsHere[0];sb.AppendLine($"  {new string(' ',depth)}{top.ms:F1} ms (self {top.self:F1}) {v.GetItemName(top.c)}"+(kidsHere.Count>1?$"   [next: {v.GetItemName(kidsHere[1].c)} {kidsHere[1].ms:F1}]":""));id=top.c;}}
  // the slowest frame as a tree: every item over 100 ms, 9 levels deep
  foreach(var x in frameMs.OrderByDescending(f=>f.ms).Take(2)){using var v=ProfilerDriver.GetHierarchyFrameDataView(x.f,0,HierarchyFrameDataView.ViewModes.MergeSamplesWithTheSameName,HierarchyFrameDataView.columnTotalTime,false);
   sb.AppendLine($"tree of frame {x.f} ({x.ms:F0} ms):");
   void Walk(int id,int depth){if(depth>22)return;var l=new List<int>();v.GetItemChildren(id,l);foreach(var c in l.OrderByDescending(c=>v.GetItemColumnDataAsFloat(c,HierarchyFrameDataView.columnTotalTime))){float ms=v.GetItemColumnDataAsFloat(c,HierarchyFrameDataView.columnTotalTime);if(ms<150)break;sb.AppendLine($"  {new string(' ',depth*2)}{ms:F0} ms (self {v.GetItemColumnDataAsFloat(c,HierarchyFrameDataView.columnSelfTime):F0}) {v.GetItemName(c)}");Walk(c,depth+1);}}
   Walk(v.GetRootItemID(),0);}
  }catch(Exception e){sb.AppendLine(e.ToString());}
  File.WriteAllText(Environment.GetEnvironmentVariable("PROBE_OUT")+"/profile-"+Path.GetFileNameWithoutExtension(raw)+".txt",sb.ToString());EditorApplication.Exit(0);}
}
