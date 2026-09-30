using System;using System.IO;using System.Linq;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer.Editor;
public static class ReviseReverseConstruction {
public static string Main(){if(EditorApplication.isCompiling||Application.isPlaying)throw new Exception("Compiled edit mode required");EditorApplication.delayCall+=()=>{try{const string target="Assets/Scenes/DansBackyardReverse.unity";const string source="Assets/Scenes/DansBackyardForward.unity";
if(!File.Exists("Docs/BackyardReverse/first-candidate/feature-drive.txt"))throw new Exception("Preserve candidate evidence first");
EditorSceneManager.OpenScene(source);var dependencies=AssetDatabase.GetDependencies(source);
var paths=Directory.GetFiles("Assets/Track/BackyardReverse","DansBackyardReverse-*.asset").Select(p=>p.Replace('\\','/')).Append("Assets/Track/BackyardReverse/Reverse worn earth.mat").ToArray();
if(paths.Any(p=>dependencies.Contains(p)))throw new Exception("Unexpected protected Forward dependency");
if(!AssetDatabase.DeleteAsset(target))throw new Exception("Could not replace checkpointed candidate scene");foreach(var path in paths)if(!AssetDatabase.DeleteAsset(path))throw new Exception("Could not remove checkpointed candidate asset "+path);
BackyardReverseAuthoring.Reverse();File.WriteAllText("Docs/BackyardReverse/revision-done.txt","Candidate 2 saved; first candidate preserved in cbe8d4a2 and first-candidate evidence");}catch(Exception e){File.WriteAllText("Docs/BackyardReverse/revision-error.txt",e.ToString());}};return "Revise checkpointed Reverse approaches";}}
