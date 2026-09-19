using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Object=UnityEngine.Object;
namespace Racer.Editor
{
    public static partial class ReverseReviewRelease
    {
        public const string Evidence="Docs/CR070-074",Folder="Assets/Track/ReverseReview",Version="0.12.0-review1";
        public static readonly string[] Scenes={StreetLoopBuilder.ScenePath,LakeCourseBuild.ScenePath,"Assets/Scenes/StreetLoopReverse.unity","Assets/Scenes/ForestLoopReverse.unity"};
        [Serializable] class Inventory { public string name; public Vector3[] road; public Vector3[] gates; public Branch[] branches; public string[] objects; public float[] starts,ends; }
        [Serializable] class Branch { public string title; public Vector3[] points; public float entry,exit; public int[] gates; }
        static void Guard(){if(Application.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Saved edit mode required");Directory.CreateDirectory(Evidence);Directory.CreateDirectory(Folder);}
        public static void Inspect()
        {
            Guard();
            foreach(var path in Scenes.Take(2))
            {
                var scene=EditorSceneManager.OpenScene(path);var race=Object.FindAnyObjectByType<RaceDirector>();race.road.Initialize();
                var layout=Object.FindAnyObjectByType<ForestLayout>();
                var inventory=new Inventory{name=scene.name,road=race.road.points,branches=Object.FindObjectsByType<WoodlandRoute>().Select(b=>new Branch{title=b.title,points=b.points,entry=b.entryRoad,exit=b.exitRoad,gates=b.bypassedGates}).ToArray(),
                    starts=layout?layout.jumpStarts:Array.Empty<float>(),ends=layout?layout.jumpEnds:Array.Empty<float>(),objects=Object.FindObjectsByType<MeshCollider>().Where(c=>c.name.Contains("Takeoff")||c.name.Contains("ramp")||c.name.Contains("Ramp")||c.name.Contains("Stunt")).Select(c=>c.name+" | "+c.bounds).ToArray()};
                File.WriteAllText(Evidence+"/source-"+scene.name+".json",JsonUtility.ToJson(inventory,true));
                File.WriteAllLines(Evidence+"/signs-before-"+scene.name+".txt",Signs());
            }
        }
        static string[] Signs()=>Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include).Where(t=>t.GetComponentInParent<PhysicalSign>()).Select(t=>t.transform.parent.name+" | "+t.text.Replace('\n','|')+" | "+t.transform.position.ToString("F3")).OrderBy(s=>s).ToArray();
        public static void RemoveSign()
        {
            Guard();var report=new List<string>();
            foreach(string key in new[]{"Street","Forest"})
            {
                string path="Assets/Track/Signs/"+key+".json";var catalog=JsonUtility.FromJson<SignWildlifeRelease.Catalog>(File.ReadAllText(path));
                var keep=catalog.letters.Where(r=>!SceneryText.RetiredHairpin(r.text,r.parentWorld)).ToArray();report.Add(key+" retired catalog faces="+(catalog.letters.Length-keep.Length));catalog.letters=keep;File.WriteAllText(path,JsonUtility.ToJson(catalog,true));
            }
            AssetDatabase.Refresh();
            foreach(string path in Scenes.Take(2))
            {
                var scene=EditorSceneManager.OpenScene(path);var before=Signs();
                var targets=Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include).Where(t=>SceneryText.RetiredHairpin(t.text,t.transform.parent.position)).Select(t=>t.transform.parent).Distinct().ToArray();
                if(targets.Length>1)throw new Exception("Ambiguous sign identity");
                foreach(var t in targets)
                {
                    if(t.name!="Breakable sign - South Cherokee Lane Jamerson Rd"||t.GetComponentsInChildren<Collider>(true).Length!=1||t.childCount!=4)throw new Exception("Unexpected sign support");
                    report.Add(scene.name+" removed "+t.name+" at "+t.position+"; 2 faces, exclusive post/board/collider/breakable component");Object.DestroyImmediate(t.gameObject);
                }
                SignWildlifeRelease.Restore(scene);
                var after=Signs();if(!before.Where(s=>!s.StartsWith("Breakable sign - South Cherokee Lane Jamerson Rd |")).SequenceEqual(after))throw new Exception("Unrelated sign changed");
                EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);File.WriteAllLines(Evidence+"/signs-after-"+scene.name+".txt",after);
            }
            File.WriteAllLines(Evidence+"/sign-removal.txt",report);AssetDatabase.SaveAssets();
        }
    }
}
