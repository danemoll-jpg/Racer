using System;using System.IO;using System.Reflection;using UnityEngine;using UnityEditor;using Racer;
// 0.67 Part A: Dan already zipped and delivered session 2026-10-02_03-04-39-449_547472. Close exactly that folder through
// the same code path as START NEW DEBUG SESSION ("close without export"; files preserved) so the new build does not resume
// it. Any other session folder (including a newer open one) is only read, never changed.
public static class Report067CloseSession {
 const string Delivered="2026-10-02_03-04-39-449_547472";
 public static void Run(){var root=Path.Combine(Application.persistentDataPath,"DebugReports");var dir=Path.Combine(root,Delivered);var json=Path.Combine(dir,"bugs.json");var log=new System.Collections.Generic.List<string>{"root="+root};
  try{var data=JsonUtility.FromJson<DebugReportSession.Report>(File.ReadAllText(json));
   if(data.sessionId!=Delivered)log.Add("Unexpected sessionId "+data.sessionId+"; nothing changed");
   else if(data.closed)log.Add($"{Delivered} already closed (closedAt={data.closedAt}); nothing changed");
   else{var ctor=typeof(DebugReportSession).GetConstructor(BindingFlags.NonPublic|BindingFlags.Instance,null,new[]{typeof(string),typeof(DebugReportSession.Report)},null);
    var session=(DebugReportSession)ctor.Invoke(new object[]{dir,data});int count=session.Count;session.Close();
    var stored=JsonUtility.FromJson<DebugReportSession.Report>(File.ReadAllText(json));
    log.Add($"Closed {session.Id} via DebugReportSession.Close (close without export): reports={count} closed={stored.closed} exported={stored.exported} closedAt={stored.closedAt}; screenshots kept={Directory.GetFiles(Path.Combine(dir,"Screenshots")).Length}");}
   var resumed=DebugReportSession.ResumeLatest(root);log.Add("Newest OPEN session the next launch resumes: "+(resumed==null?"none (next F4 starts a new session at BUG-001)":$"{resumed.Id} ({resumed.Count} reports; not changed by this tool)"));}
  catch(Exception e){log.Add("ERROR "+e);}
  File.WriteAllLines("Docs/Report067/delivered-session-closed.txt",log);EditorApplication.Exit(0);}
}
