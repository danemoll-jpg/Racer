using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Racer.Editor
{
    public static class CollectionCourseUpdate
    {
        const string Dir="Docs/CR050-053";
        public static void Apply()
        {
            var race=Object.FindAnyObjectByType<RaceDirector>();
            if(Application.isPlaying||race.gameObject.scene.isDirty)throw new InvalidOperationException("Saved edit mode required");
            race.road.Initialize();Directory.CreateDirectory(Dir);
            if(race.gates.Length!=20)throw new InvalidOperationException("Expected original twenty gates; do not apply twice");
            Evidence(race,"before");
            // CP07 and CP09 are redundant mouth/landing gates. Consolidate the first
            // into CP06, the second into CP10. Road and branch geometry stay fixed.
            foreach(var item in new[]{(6,1680f),(10,2330f)})
            {
                var gate=race.gates[item.Item1];var p=race.road.At(item.Item2,out var f);
                gate.transform.SetPositionAndRotation(p+Vector3.up*1.6f,Quaternion.LookRotation(Vector3.ProjectOnPlane(f,Vector3.up)));
                gate.upperHeight=24;
            }
            var removed=new[]{race.gates[7],race.gates[9]};
            race.gates=race.gates.Where(g=>!removed.Contains(g)).ToArray();
            foreach(var g in removed)Object.DestroyImmediate(g.gameObject);
            for(int i=1;i<race.gates.Length;i++)
            {race.gates[i].name=$"CP {i:00}";race.gates[i].GetComponentsInChildren<TextMesh>().First(t=>t.name=="Gate label").text=$"CP {i:00}  >";}
            foreach(var branch in Object.FindObjectsByType<WoodlandRoute>())
            {
                branch.bypassedGates=Enumerable.Range(1,race.gates.Length-1).Where(i=>{float s=race.road.Project(race.gates[i].transform.position,out _);return s>branch.entryRoad&&s<branch.exitRoad;}).ToArray();
                EditorUtility.SetDirty(branch);
            }
            EditorUtility.SetDirty(race);EditorSceneManager.MarkSceneDirty(race.gameObject.scene);EditorSceneManager.SaveScene(race.gameObject.scene);AssetDatabase.SaveAssets();
            Evidence(race,"after");
        }
        static void Evidence(RaceDirector race,string label)
        {
            var road=race.road;
            File.WriteAllText(Dir+"/gates-"+label+".txt",string.Join("\n",race.gates.Select((g,i)=>$"ID {i} | {g.GetComponentsInChildren<TextMesh>().First(t=>t.name=="Gate label").text} | road={road.Project(g.transform.position,out _):F2} | position={g.transform.position} | span={g.halfWidth*2} height={g.upperHeight}"))+"\n"+string.Join("\n",Object.FindObjectsByType<WoodlandRoute>().Select(b=>$"{b.title}: {b.entryRoad}..{b.exitRoad}; bypass={string.Join(",",b.bypassedGates)}")));
            foreach(var area in new[]{("creek",new Vector3(330,0,-380)),("fox",new Vector3(-100,0,-530))})
            {
                var camera=new GameObject("Temporary evidence camera").AddComponent<Camera>();
                camera.transform.SetPositionAndRotation(area.Item2+new Vector3(0,290,-150),Quaternion.Euler(62,0,0));camera.orthographic=true;camera.orthographicSize=190;camera.farClipPlane=1000;
                var rt=new RenderTexture(1400,1000,24);camera.targetTexture=rt;camera.Render();var previous=RenderTexture.active;RenderTexture.active=rt;
                var texture=new Texture2D(1400,1000,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,1400,1000),0,0);texture.Apply();File.WriteAllBytes(Dir+"/"+area.Item1+"-"+label+".png",texture.EncodeToPNG());
                RenderTexture.active=previous;camera.targetTexture=null;Object.DestroyImmediate(texture);Object.DestroyImmediate(rt);Object.DestroyImmediate(camera.gameObject);
            }
        }
    }
}
