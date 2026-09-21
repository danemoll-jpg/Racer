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
    public static partial class DiscoveryAuthoring
    {
        public static string[] CR112Scenes=>ReverseReviewRelease.Scenes.Concat(new[]{"Assets/Scenes/MountainLoop.unity","Assets/Scenes/MountainLoopReverse.unity"}).ToArray();
        [MenuItem("Racer/Author CR112-118 from reviewed baseline")]
        public static void CR112RegenerateFromBaseline()
        {
            // Apply to the six baseline scenes after their baseline generation. Each
            // geometry pass refuses an already-updated scene instead of duplicating it.
            CR112Households();CR113Repairs();CR113Endpoints();CR117Geometry();CR117Landscape();CR117Connections();CR117SummitReturn();CR117ClimbSubgrade();CR120Apply();
        }
        public static void CR112Households()
        {
            var rows=new List<string>();
            foreach(var path in CR112Scenes){
                EditorSceneManager.OpenScene(path);owner=Object.FindAnyObjectByType<RaceDirector>();street=owner.ambientRoad?owner.ambientRoad:owner.road;street.Initialize();
                var dan=GameObject.Find("Dan - blue X").transform;var kyle=GameObject.Find("Friend across street - blue circle").transform;
                var life=owner.GetComponent<AmbientLife>();float sd=street.Project(dan.position,out _);
                Vector3 Yard(float along,float offset){var p=street.At(sd+along,out var f);var side=Vector3.Cross(Vector3.up,f).normalized;if(Vector3.Dot(side,dan.position-p)<0)side=-side;p+=side*offset;p.y=CR105Authoring.Ground(p)+.035f;return p;}
                life.coffee=new[]{Yard(-2,8.5f),Yard(.5f,8.5f)};
                life.football=new[]{Yard(-5,20),Yard(2,21),Yard(-2,27)};
                // Kyle's front overlooks the street from a lower terrace. Place the smokers
                // on its street-facing approach shoulder, not the old buried coordinates.
                Vector3 Kyle(float along,float side){var p=kyle.position+kyle.forward*along+kyle.right*side;p.y=CR105Authoring.Ground(p)+.035f;return p;}
                life.smoking=new[]{Kyle(21,20),Kyle(21,22.5f)};
                int i=0;var wildlife=owner.GetComponent<Wildlife>();for(int n=0;n<wildlife.habitats.Length;n++){if(wildlife.habitats[n].species!=Wildlife.Species.Turkey)continue;wildlife.habitats[n].position=Kyle(24,29+i*2.8f);wildlife.habitats[n].escape=kyle.right;i++;}
                ClearCompleteTrees(p=>life.football.Any(q=>Vector2.Distance(new(p.x,p.z),new(q.x,q.z))<8)||life.smoking.Any(q=>Vector2.Distance(new(p.x,p.z),new(q.x,q.z))<3));
                rows.Add(path+" coffee="+string.Join(";",life.coffee)+" football="+string.Join(";",life.football)+" smokers="+string.Join(";",life.smoking));
                Save();
            }
            Directory.CreateDirectory("Docs/CR112-118");File.WriteAllLines("Docs/CR112-118/household-authoring.txt",rows);
        }
    }
}
