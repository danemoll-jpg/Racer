using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Object=UnityEngine.Object;
namespace Racer.Editor
{
    public static partial class DiscoveryAuthoring
    {
        public static void CorrectGuidanceAndMap()
        {
            foreach(var path in ReverseReviewRelease.Scenes){
                EditorSceneManager.OpenScene(path);owner=Object.FindAnyObjectByType<RaceDirector>();street=owner.ambientRoad?owner.ambientRoad:owner.road;street.Initialize();
                var old=GameObject.Find("CR098 summit guidance");if(old)Object.DestroyImmediate(old);
                worldRoot=new GameObject("CR098 summit guidance").transform;
                foreach(var text in Object.FindObjectsByType<TextMesh>().Where(t=>t.text.StartsWith("SUMMIT HOMEWARD FLIGHT")||t.text.StartsWith("RETURN VIA RIDGE TRAIL")).ToArray())Object.DestroyImmediate(text.transform.parent.gameObject);
                NewSign(new(995,0,128),new(-.65f,0,.76f),"GIANT SUMMIT JUMP\nTURN RIGHT / EAST APPROACH\nFollow the teal flags");
                NewSign(new(1040,0,140),new(.9f,0,.35f),"SUMMIT HOMEWARD FLIGHT\nBEAR LEFT AT TEAL FLAGS\nLaunch WEST toward home");
                NewSign(new(1085,0,198),Vector3.left,"SUMMIT HOMEWARD FLIGHT\n55–90 mph / STRAIGHT AHEAD\nWIDE LANDING / BRAKE AFTER");
                NewSign(new(797,0,208),Vector3.left,"LANDING / SLOW DOWN\nRIGHT TO RIDGE RETURN\nDO NOT CLIMB THE LAUNCH");
                NewSign(new(867,0,178),Vector3.right,"SAFE RETURN / RIDGE TRAIL\nKEEP RIGHT / PASS BELOW\nGIANT JUMP IS ONE WAY");
                var pole=Mat("Summit marker pole",new(.76f,.72f,.55f));var flag=Mat("Summit teal flags",new(.035f,.72f,.68f));
                for(int i=0;i<10;i++)foreach(float side in new[]{-1f,1f}){
                    var p=Ground(new Vector3(1070-i*10,0,190+side*15));var r=new GameObject("Summit approach teal marker").transform;r.SetParent(worldRoot);r.position=p;
                    Part(r,"Grounded marker post",new(0,1.4f,0),new(.10f,2.8f,.10f),pole);
                    Part(r,"Teal flag",new(-.55f,2.4f,0),new(1.15f,.6f,.035f),flag);
                }
                // Use the established depth-tested world lettering, so guidance
                // cannot appear through terrain or the back of its sign board.
                foreach(var text in Object.FindObjectsByType<TextMesh>().Where(t=>t.name=="Physical lettering")){
                    text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    text.GetComponent<Renderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Track/Woodland/Lettering.mat");
                }
                Map();var map=owner.GetComponent<ExplorationMap>();
                map.destinations=map.destinations.Append(new ExplorationMap.Destination{id="summit-homeward",title="Summit Homeward Flight / east approach",position=Ground(new Vector3(1068,0,190)),yaw=270}).ToArray();
                File.WriteAllText(Evidence+"/guidance-"+owner.gameObject.scene.name+".txt","Lake gateway > existing mountain ascent > summit camp/ridge junction (995,128) > east spur via (1040,140) > teal flags at (1085,198) > westbound runway at (1070,145,190). New summit landmark stays hidden until physical discovery, safe arrival at runway start. Flight heads toward neighborhood; slow on landing, north/right return to existing ridge trail. No new race or force boost.");
                Save();
            }
        }
    }
}
